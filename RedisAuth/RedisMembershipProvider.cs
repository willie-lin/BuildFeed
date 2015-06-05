using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using NServiceKit.Redis;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace RedisAuth
{
    public class RedisMembershipProvider : MembershipProvider
    {
        private bool _enablePasswordReset = true;
        private int _maxInvalidPasswordAttempts = 5;
        private int _minRequiredNonAlphanumericCharacters = 1;
        private int _minRequriedPasswordLength = 12;
        private int _passwordAttemptWindow = 60;
        private bool _requiresUniqueEmail = true;

        public override string ApplicationName
        {
            get { return ""; }
            set { }
        }

        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return _minRequriedPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return ""; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            base.Initialize(name, config);

            _enablePasswordReset = tryReadBool(config["enablePasswordReset"], _enablePasswordReset);
            _maxInvalidPasswordAttempts = tryReadInt(config["maxInvalidPasswordAttempts"], _maxInvalidPasswordAttempts);
            _minRequiredNonAlphanumericCharacters = tryReadInt(config["minRequiredNonAlphanumericCharacters"], _minRequiredNonAlphanumericCharacters);
            _minRequriedPasswordLength = tryReadInt(config["minRequriedPasswordLength"], _minRequriedPasswordLength);
            _passwordAttemptWindow = tryReadInt(config["passwordAttemptWindow"], _passwordAttemptWindow);
            _requiresUniqueEmail = tryReadBool(config["requiresUniqueEmail"], _requiresUniqueEmail);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            bool isAuthenticated = ValidateUser(username, oldPassword);

            if (isAuthenticated)
            {
                using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
                {
                    var client = rClient.As<RedisMember>();
                    var rm = client.GetAll().SingleOrDefault(m => m.UserName.ToLower() == username.ToLower());

                    byte[] salt = new byte[24];
                    byte[] hash = calculateHash(newPassword, ref salt);

                    rm.PassSalt = salt;
                    rm.PassHash = hash;

                    client.Store(rm);

                    return true;
                }
            }

            return false;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            if (password.Length < MinRequiredPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            MembershipUser mu = null;

            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var users = client.GetAll();

                if (users.Any(m => m.UserName.ToLower() == username.ToLower()))
                {
                    status = MembershipCreateStatus.DuplicateUserName;
                }
                else if (users.Any(m => m.EmailAddress.ToLower() == email.ToLower()))
                {
                    status = MembershipCreateStatus.DuplicateEmail;
                }
                else
                {
                    byte[] salt = new byte[24];
                    byte[] hash = calculateHash(password, ref salt);

                    RedisMember rm = new RedisMember()
                    {
                        Id = Guid.NewGuid(),
                        UserName = username,
                        PassHash = hash,
                        PassSalt = salt,
                        EmailAddress = email,

                        IsApproved = false,
                        IsLockedOut = false,

                        CreationDate = DateTime.Now,
                        LastLoginDate = DateTime.MinValue,
                        LastActivityDate = DateTime.MinValue,
                        LastLockoutDate = DateTime.MinValue
                    };

                    client.Store(rm);

                    status = MembershipCreateStatus.Success;
                    mu = new MembershipUser(this.Name, rm.UserName, rm.Id, rm.EmailAddress, "", "", rm.IsApproved, rm.IsLockedOut, rm.CreationDate, rm.LastLoginDate, rm.LastActivityDate, DateTime.MinValue, rm.LastLockoutDate);
                }
            }

            return mu;
        }

        private static byte[] calculateHash(string password, ref byte[] salt)
        {
            if (!salt.Any(v => v != 0))
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(salt);
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            byte[] hashPlaintext = new byte[salt.Length + passwordBytes.Length];

            passwordBytes.CopyTo(hashPlaintext, 0);
            salt.CopyTo(hashPlaintext, passwordBytes.Length);

            SHA512CryptoServiceProvider sha = new SHA512CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(hashPlaintext);

            return hash;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var user = client.GetAll().SingleOrDefault(m => m.UserName.ToLower() == username.ToLower());

                if (user != null)
                {
                    client.DeleteById(user.Id);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection muc = new MembershipUserCollection();

            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var users = client.GetAll();

                totalRecords = users.Count;
                var pageItems = users.Skip(pageIndex * pageSize).Take(pageSize);

                foreach (var rm in pageItems)
                {
                    muc.Add(new MembershipUser(this.Name, rm.UserName, rm.Id, rm.EmailAddress, "", "", rm.IsApproved, rm.IsLockedOut, rm.CreationDate, rm.LastLoginDate, rm.LastActivityDate, DateTime.MinValue, rm.LastLockoutDate));
                }
            }

            return muc;
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var rm = client.GetAll().SingleOrDefault(m => m.UserName.ToLower() == username.ToLower());

                if (rm == null)
                {
                    return null;
                }
                else
                {
                    return new MembershipUser(this.Name, rm.UserName, rm.Id, rm.EmailAddress, "", "", rm.IsApproved, rm.IsLockedOut, rm.CreationDate, rm.LastLoginDate, rm.LastActivityDate, DateTime.MinValue, rm.LastLockoutDate);
                }
            }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var rm = client.GetById(providerUserKey);

                if (rm == null)
                {
                    return null;
                }
                else
                {
                    return new MembershipUser(this.Name, rm.UserName, rm.Id, rm.EmailAddress, "", "", rm.IsApproved, rm.IsLockedOut, rm.CreationDate, rm.LastLoginDate, rm.LastActivityDate, DateTime.MinValue, rm.LastLockoutDate);
                }
            }
        }

        public override string GetUserNameByEmail(string email)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var rm = client.GetAll().SingleOrDefault(m => m.EmailAddress.ToLower() == email.ToLower());

                if (rm == null)
                {
                    return "";
                }
                else
                {
                    return rm.UserName;
                }
            }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public void ChangeApproval(Guid Id, bool newStatus)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var rm = client.GetById(Id);

                if (rm != null)
                {
                    rm.IsApproved = newStatus;
                    client.Store(rm);
                }
            }
        }

        public void ChangeLockStatus(Guid Id, bool newStatus)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var rm = client.GetById(Id);

                if (rm != null)
                {
                    rm.IsLockedOut = newStatus;

                    if (newStatus)
                    {
                        rm.LastLockoutDate = DateTime.Now;
                    }
                    else
                    {
                        rm.LockoutWindowAttempts = 0;
                        rm.LockoutWindowStart = DateTime.MinValue;
                    }

                    client.Store(rm);
                }
            }
        }

        public override bool UnlockUser(string userName)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var rm = client.GetAll().SingleOrDefault(m => m.UserName.ToLower() == userName.ToLower());

                rm.IsLockedOut = false;

                client.Store(rm);

                return true;
            }
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisMember>();
                var rm = client.GetAll().SingleOrDefault(m => m.UserName.ToLower() == username.ToLower());

                if (rm == null || !(rm.IsApproved && !rm.IsLockedOut))
                {
                    return false;
                }

                byte[] salt = rm.PassSalt;
                byte[] hash = calculateHash(password, ref salt);

                bool isFail = false;

                for (int i = 0; i > hash.Length; i++)
                {
                    isFail |= (hash[i] != rm.PassHash[i]);
                }

                if (isFail)
                {
                    if (rm.LockoutWindowStart == DateTime.MinValue)
                    {
                        rm.LockoutWindowStart = DateTime.Now;
                        rm.LockoutWindowAttempts = 1;
                    }
                    else
                    {
                        if (rm.LockoutWindowStart.AddMinutes(PasswordAttemptWindow) > DateTime.Now)
                        {
                            // still within window
                            rm.LockoutWindowAttempts++;
                            if (rm.LockoutWindowAttempts >= MaxInvalidPasswordAttempts)
                            {
                                rm.IsLockedOut = true;
                            }
                        }
                        else
                        {
                            // outside of window, reset
                            rm.LockoutWindowStart = DateTime.Now;
                            rm.LockoutWindowAttempts = 1;
                        }
                    }
                }
                else
                {
                    rm.LastLoginDate = DateTime.Now;
                    rm.LockoutWindowStart = DateTime.MinValue;
                    rm.LockoutWindowAttempts = 0;
                }
                client.Store(rm);

                return !isFail;
            }
        }

        private static bool tryReadBool(string config, bool defaultValue)
        {
            bool temp = false;
            bool success = bool.TryParse(config, out temp);
            return success ? temp : defaultValue;
        }

        private static int tryReadInt(string config, int defaultValue)
        {
            int temp = 0;
            bool success = int.TryParse(config, out temp);
            return success ? temp : defaultValue;
        }
    }

    [DataObject]
    public class RedisMember : IHasId<Guid>
    {
        [Key]
        [Index]
        public Guid Id { get; set; }

        [@Required]
        [DisplayName("Username")]
        [Key]
        public string UserName { get; set; }

        [@Required]
        public byte[] PassHash { get; set; }

        [@Required]
        public byte[] PassSalt { get; set; }

        [@Required]
        [DisplayName("Email Address")]
        [Key]
        public string EmailAddress { get; set; }

        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public DateTime LastLoginDate { get; set; }

        public DateTime LockoutWindowStart { get; set; }
        public int LockoutWindowAttempts { get; set; }
    }
}