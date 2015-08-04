using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace MongoAuth
{
    public class MongoMembershipProvider : MembershipProvider
    {
        private const string _memberCollectionName = "members";

        private bool _enablePasswordReset = true;
        private int _maxInvalidPasswordAttempts = 5;
        private int _minRequiredNonAlphanumericCharacters = 1;
        private int _minRequriedPasswordLength = 12;
        private int _passwordAttemptWindow = 60;
        private bool _requiresUniqueEmail = true;

        private MongoClient _dbClient;
        private IMongoCollection<MongoMember> _memberCollection;

        public override string ApplicationName { get; set; }

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

            _dbClient = new MongoClient(new MongoClientSettings()
            {
                Server = new MongoServerAddress(DatabaseConfig.Host, DatabaseConfig.Port)
            });
            _memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(_memberCollectionName);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            bool isAuthenticated = ValidateUser(username, oldPassword);

            if (isAuthenticated)
            {
                var task = _memberCollection
                    .Find(m => m.UserName.ToLower() == username.ToLower())
                    .SingleOrDefaultAsync();
                task.Wait();
                var mm = task.Result;

                if (mm == null)
                {
                    return false;
                }

                byte[] salt = new byte[24];
                byte[] hash = calculateHash(newPassword, ref salt);

                mm.PassSalt = salt;
                mm.PassHash = hash;

                var replaceTask = _memberCollection
                    .ReplaceOneAsync(m => m.Id == mm.Id, mm);
                replaceTask.Wait();

                if (replaceTask.IsCompleted)
                {
                    return true;
                }

                return false;
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

            var dupeUsers = _memberCollection
                .Find(m => m.UserName.ToLower() == username.ToLower())
                .CountAsync();
            var dupeEmails = _memberCollection
                .Find(m => m.EmailAddress.ToLower() == email.ToLower())
                .CountAsync();
            dupeUsers.Wait();
            dupeEmails.Wait();

            if (dupeUsers.Result > 0)
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }
            else if (dupeEmails.Result > 0)
            {
                status = MembershipCreateStatus.DuplicateEmail;
            }
            else
            {
                byte[] salt = new byte[24];
                byte[] hash = calculateHash(password, ref salt);

                MongoMember mm = new MongoMember()
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

                var insertTask = _memberCollection
                    .InsertOneAsync(mm);
                insertTask.Wait();

                if (insertTask.Status == TaskStatus.RanToCompletion)
                {

                    status = MembershipCreateStatus.Success;
                    mu = new MembershipUser(this.Name, mm.UserName, mm.Id, mm.EmailAddress, "", "", mm.IsApproved, mm.IsLockedOut, mm.CreationDate, mm.LastLoginDate, mm.LastActivityDate, DateTime.MinValue, mm.LastLockoutDate);
                }
                else
                {
                    status = MembershipCreateStatus.ProviderError;
                }
            }

            return mu;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            var task = _memberCollection
                .DeleteOneAsync(m => m.UserName.ToLower() == m.UserName.ToLower());
            task.Wait();

            return task.Result.IsAcknowledged && task.Result.DeletedCount == 1;
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

            var users = _memberCollection
                .Find(new BsonDocument());

            var totalRecordsTask = users
                .CountAsync();
            totalRecordsTask.Wait();
            totalRecords = Convert.ToInt32(totalRecordsTask.Result);

            users = users
                .Skip(pageIndex * pageSize)
                .Limit(pageSize);
            var pageItemsTask = users.ToListAsync();
            pageItemsTask.Wait();

            foreach (var mm in pageItemsTask.Result)
            {
                muc.Add(new MembershipUser(this.Name, mm.UserName, mm.Id, mm.EmailAddress, "", "", mm.IsApproved, mm.IsLockedOut, mm.CreationDate, mm.LastLoginDate, mm.LastActivityDate, DateTime.MinValue, mm.LastLockoutDate));
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
            var task = _memberCollection
                .Find(f => f.UserName.ToLower() == username.ToLower())
                .FirstOrDefaultAsync();
            task.Wait();

            var mm = task.Result;

            return mm == null ? null : new MembershipUser(this.Name, mm.UserName, mm.Id, mm.EmailAddress, "", "", mm.IsApproved, mm.IsLockedOut, mm.CreationDate, mm.LastLoginDate, mm.LastActivityDate, DateTime.MinValue, mm.LastLockoutDate);
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var task = _memberCollection
                .Find(f => f.Id == (Guid)providerUserKey)
                .FirstOrDefaultAsync();
            task.Wait();

            var mm = task.Result;

            return mm == null ? null : new MembershipUser(this.Name, mm.UserName, mm.Id, mm.EmailAddress, "", "", mm.IsApproved, mm.IsLockedOut, mm.CreationDate, mm.LastLoginDate, mm.LastActivityDate, DateTime.MinValue, mm.LastLockoutDate);
        }

        public override string GetUserNameByEmail(string email)
        {
            var task = _memberCollection
                .Find(f => f.EmailAddress.ToLower() == email.ToLower())
                .FirstOrDefaultAsync();
            task.Wait();

            return task.Result.UserName;
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public void ChangeApproval(Guid Id, bool newStatus)
        {
            var task = _memberCollection
                .UpdateOneAsync(
                    Builders<MongoMember>.Filter.Eq(u => u.Id, Id),
                    Builders<MongoMember>.Update.Set(u => u.IsApproved, newStatus));
            task.Wait();
        }

        public void ChangeLockStatus(Guid Id, bool newStatus)
        {
            var updateDefinition = new List<UpdateDefinition<MongoMember>>();
            updateDefinition.Add(Builders<MongoMember>.Update.Set(u => u.IsLockedOut, newStatus));

            if (newStatus)
            {
                updateDefinition.Add(Builders<MongoMember>.Update.Set(u => u.LastLockoutDate, DateTime.Now));
            }
            else
            {
                updateDefinition.Add(Builders<MongoMember>.Update.Set(u => u.LockoutWindowAttempts, 0));
                updateDefinition.Add(Builders<MongoMember>.Update.Set(u => u.LastLockoutDate, DateTime.MinValue));
            }

            var task = _memberCollection
                .UpdateOneAsync(
                    Builders<MongoMember>.Filter.Eq(u => u.Id, Id),
                    Builders<MongoMember>.Update.Combine(updateDefinition));
            task.Wait();
        }

        public override bool UnlockUser(string userName)
        {
            var task = _memberCollection
                .UpdateOneAsync(
                    Builders<MongoMember>.Filter.Eq(m => m.UserName.ToLower(), userName.ToLower()),
                    Builders<MongoMember>.Update.Set(m => m.IsLockedOut,
                    false));
            task.Wait();

            return task.Result.IsAcknowledged && task.Result.ModifiedCount == 1;
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            var task = _memberCollection
                .Find(f => f.UserName.ToLower() == username.ToLower())
                .FirstOrDefaultAsync();
            task.Wait();
            var mm = task.Result;

            if (mm == null || !(mm.IsApproved && !mm.IsLockedOut))
            {
                return false;
            }

            byte[] salt = mm.PassSalt;
            byte[] hash = calculateHash(password, ref salt);

            bool isFail = false;

            for (int i = 0; i > hash.Length; i++)
            {
                isFail |= (hash[i] != mm.PassHash[i]);
            }


            if (isFail)
            {
                if (mm.LockoutWindowStart == DateTime.MinValue)
                {
                    mm.LockoutWindowStart = DateTime.Now;
                    mm.LockoutWindowAttempts = 1;
                }
                else
                {
                    if (mm.LockoutWindowStart.AddMinutes(PasswordAttemptWindow) > DateTime.Now)
                    {
                        // still within window
                        mm.LockoutWindowAttempts++;
                        if (mm.LockoutWindowAttempts >= MaxInvalidPasswordAttempts)
                        {
                            mm.IsLockedOut = true;
                        }
                    }
                    else
                    {
                        // outside of window, reset
                        mm.LockoutWindowStart = DateTime.Now;
                        mm.LockoutWindowAttempts = 1;
                    }
                }
            }
            else
            {
                mm.LastLoginDate = DateTime.Now;
                mm.LockoutWindowStart = DateTime.MinValue;
                mm.LockoutWindowAttempts = 0;
            }

            var updTask = _memberCollection
                .ReplaceOneAsync(
                    Builders<MongoMember>.Filter.Eq(u => u.Id, mm.Id),
                    mm);
            updTask.Wait();

            return !isFail;
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

    public class MongoMember
    {
        [BsonId]
        public Guid Id { get; set; }

        public string UserName { get; set; }
        public byte[] PassHash { get; set; }
        public byte[] PassSalt { get; set; }
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
