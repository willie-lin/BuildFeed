using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace MongoAuth
{
    public class MongoMembershipProvider : MembershipProvider
    {
        private const string MEMBER_COLLECTION_NAME = "members";

        private bool _enablePasswordReset = true;
        private int _maxInvalidPasswordAttempts = 5;

        private IMongoCollection<MongoMember> _memberCollection;
        private int _minRequiredNonAlphanumericCharacters = 1;
        private int _minRequriedPasswordLength = 8;
        private int _passwordAttemptWindow = 60;
        private bool _requiresUniqueEmail = true;

        public override string ApplicationName { get; set; }

        public override bool EnablePasswordReset => _enablePasswordReset;

        public override bool EnablePasswordRetrieval => false;

        public override int MaxInvalidPasswordAttempts => _maxInvalidPasswordAttempts;

        public override int MinRequiredNonAlphanumericCharacters => _minRequiredNonAlphanumericCharacters;

        public override int MinRequiredPasswordLength => _minRequriedPasswordLength;

        public override int PasswordAttemptWindow => _passwordAttemptWindow;

        public override MembershipPasswordFormat PasswordFormat => MembershipPasswordFormat.Hashed;

        public override string PasswordStrengthRegularExpression => "";

        public override bool RequiresQuestionAndAnswer => false;

        public override bool RequiresUniqueEmail => _requiresUniqueEmail;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            base.Initialize(name, config);

            _enablePasswordReset = TryReadBool(config["enablePasswordReset"], _enablePasswordReset);
            _maxInvalidPasswordAttempts = TryReadInt(config["maxInvalidPasswordAttempts"], _maxInvalidPasswordAttempts);
            _minRequiredNonAlphanumericCharacters = TryReadInt(config["minRequiredNonAlphanumericCharacters"], _minRequiredNonAlphanumericCharacters);
            _minRequriedPasswordLength = TryReadInt(config["minRequriedPasswordLength"], _minRequriedPasswordLength);
            _passwordAttemptWindow = TryReadInt(config["passwordAttemptWindow"], _passwordAttemptWindow);
            _requiresUniqueEmail = TryReadBool(config["requiresUniqueEmail"], _requiresUniqueEmail);


            MongoClientSettings settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(DatabaseConfig.Host, DatabaseConfig.Port)
            };

            if (!string.IsNullOrEmpty(DatabaseConfig.Username) && !string.IsNullOrEmpty(DatabaseConfig.Password))
            {
                settings.Credentials = new List<MongoCredential>
                {
                    MongoCredential.CreateCredential(DatabaseConfig.Database, DatabaseConfig.Username, DatabaseConfig.Password)
                };
            }

            MongoClient dbClient = new MongoClient(settings);

            _memberCollection = dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(MEMBER_COLLECTION_NAME);

#pragma warning disable 4014
            SetupIndexes();
#pragma warning restore 4014
        }

        public async Task SetupIndexes()
        {
            List<BsonDocument> indexes = await (await _memberCollection.Indexes.ListAsync()).ToListAsync();

            if (indexes.All(i => i["name"] != "_idx_username"))
            {
                await _memberCollection.Indexes.CreateOneAsync(Builders<MongoMember>.IndexKeys.Ascending(b => b.UserName),
                    new CreateIndexOptions
                    {
                        Name = "_idx_username",
                        Unique = true
                    });
            }

            if (indexes.All(i => i["name"] != "_idx_email"))
            {
                await _memberCollection.Indexes.CreateOneAsync(Builders<MongoMember>.IndexKeys.Ascending(b => b.EmailAddress),
                    new CreateIndexOptions
                    {
                        Name = "_idx_email",
                        Unique = true
                    });
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            bool isAuthenticated = ValidateUser(username, oldPassword);

            if (isAuthenticated)
            {
                Task<MongoMember> task = _memberCollection.Find(m => m.UserName.ToLower() == username.ToLower()).SingleOrDefaultAsync();
                task.Wait();
                MongoMember mm = task.Result;

                if (mm == null)
                {
                    return false;
                }

                var salt = new byte[24];
                byte[] hash = CalculateHash(newPassword, ref salt);

                mm.PassSalt = salt;
                mm.PassHash = hash;

                Task<ReplaceOneResult> replaceTask = _memberCollection.ReplaceOneAsync(m => m.Id == mm.Id, mm);
                replaceTask.Wait();

                return replaceTask.IsCompleted;
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

            Task<long> dupeUsers = _memberCollection.Find(m => m.UserName.ToLower() == username.ToLower()).CountAsync();
            Task<long> dupeEmails = _memberCollection.Find(m => m.EmailAddress.ToLower() == email.ToLower()).CountAsync();
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
                var salt = new byte[24];
                byte[] hash = CalculateHash(password, ref salt);

                MongoMember mm = new MongoMember
                {
                    Id = Guid.NewGuid(),
                    UserName = username,
                    PassHash = hash,
                    PassSalt = salt,
                    EmailAddress = email,
                    IsApproved = isApproved,
                    IsLockedOut = false,
                    CreationDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.MinValue,
                    LastActivityDate = DateTime.MinValue,
                    LastLockoutDate = DateTime.MinValue
                };

                Task insertTask = _memberCollection.InsertOneAsync(mm);
                insertTask.Wait();

                if (insertTask.Status == TaskStatus.RanToCompletion)
                {
                    status = MembershipCreateStatus.Success;
                    mu = new MembershipUser(Name, mm.UserName, mm.Id, mm.EmailAddress, "", "", mm.IsApproved, mm.IsLockedOut, mm.CreationDate, mm.LastLoginDate, mm.LastActivityDate, DateTime.MinValue, mm.LastLockoutDate);
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
            Task<DeleteResult> task = _memberCollection.DeleteOneAsync(m => m.UserName.ToLower() == username.ToLower());
            task.Wait();

            return task.Result.IsAcknowledged && task.Result.DeletedCount == 1;
        }

        public bool DeleteUser(Guid id)
        {
            Task<DeleteResult> task = _memberCollection.DeleteOneAsync(m => m.Id == id);
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

            IFindFluent<MongoMember, MongoMember> users = _memberCollection.Find(new BsonDocument()).Sort(Builders<MongoMember>.Sort.Ascending(m => m.UserName));

            Task<long> totalRecordsTask = users.CountAsync();
            totalRecordsTask.Wait();
            totalRecords = Convert.ToInt32(totalRecordsTask.Result);

            users = users.Skip(pageIndex * pageSize).Limit(pageSize);
            Task<List<MongoMember>> pageItemsTask = users.ToListAsync();
            pageItemsTask.Wait();

            foreach (MongoMember mm in pageItemsTask.Result)
            {
                muc.Add(new MembershipUser(Name, mm.UserName, mm.Id, mm.EmailAddress, "", "", mm.IsApproved, mm.IsLockedOut, FixupDatesFromMongo(mm.CreationDate), FixupDatesFromMongo(mm.LastLoginDate), FixupDatesFromMongo(mm.LastActivityDate), DateTime.MinValue, FixupDatesFromMongo(mm.LastLockoutDate)));
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
            Task<MongoMember> task = _memberCollection.Find(f => f.UserName.ToLower() == username.ToLower()).FirstOrDefaultAsync();
            task.Wait();

            MongoMember mm = task.Result;

            return mm == null
                ? null
                : new MembershipUser(Name, mm.UserName, mm.Id, mm.EmailAddress, "", "", mm.IsApproved, mm.IsLockedOut, FixupDatesFromMongo(mm.CreationDate), FixupDatesFromMongo(mm.LastLoginDate), FixupDatesFromMongo(mm.LastActivityDate), DateTime.MinValue, FixupDatesFromMongo(mm.LastLockoutDate));
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            Task<MongoMember> task = _memberCollection.Find(f => f.Id == (Guid)providerUserKey).FirstOrDefaultAsync();
            task.Wait();

            MongoMember mm = task.Result;

            return mm == null
                ? null
                : new MembershipUser(Name, mm.UserName, mm.Id, mm.EmailAddress, "", "", mm.IsApproved, mm.IsLockedOut, FixupDatesFromMongo(mm.CreationDate), FixupDatesFromMongo(mm.LastLoginDate), FixupDatesFromMongo(mm.LastActivityDate), DateTime.MinValue, FixupDatesFromMongo(mm.LastLockoutDate));
        }

        public override string GetUserNameByEmail(string email)
        {
            Task<MongoMember> task = _memberCollection.Find(f => f.EmailAddress.ToLower() == email.ToLower()).FirstOrDefaultAsync();
            task.Wait();

            return task.Result.UserName;
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public void ChangeApproval(Guid id, bool newStatus)
        {
            Task<UpdateResult> task = _memberCollection.UpdateOneAsync(Builders<MongoMember>.Filter.Eq(u => u.Id, id), Builders<MongoMember>.Update.Set(u => u.IsApproved, newStatus));
            task.Wait();
        }

        public void ChangeLockStatus(Guid id, bool newStatus)
        {
            var updateDefinition = new List<UpdateDefinition<MongoMember>>
            {
                Builders<MongoMember>.Update.Set(u => u.IsLockedOut, newStatus)
            };

            if (newStatus)
            {
                updateDefinition.Add(Builders<MongoMember>.Update.Set(u => u.LastLockoutDate, DateTime.UtcNow));
            }
            else
            {
                updateDefinition.Add(Builders<MongoMember>.Update.Set(u => u.LockoutWindowAttempts, 0));
                updateDefinition.Add(Builders<MongoMember>.Update.Set(u => u.LastLockoutDate, DateTime.MinValue));
            }

            Task<UpdateResult> task = _memberCollection.UpdateOneAsync(Builders<MongoMember>.Filter.Eq(u => u.Id, id), Builders<MongoMember>.Update.Combine(updateDefinition));
            task.Wait();
        }

        public override bool UnlockUser(string userName)
        {
            Task<UpdateResult> task = _memberCollection.UpdateOneAsync(Builders<MongoMember>.Filter.Eq(m => m.UserName.ToLower(), userName.ToLower()), Builders<MongoMember>.Update.Set(m => m.IsLockedOut, false));
            task.Wait();

            return task.Result.IsAcknowledged && task.Result.ModifiedCount == 1;
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            Task<MongoMember> task = _memberCollection.Find(f => f.UserName.ToLower() == username.ToLower()).FirstOrDefaultAsync();
            task.Wait();
            MongoMember mm = task.Result;

            if (mm == null
                || !(mm.IsApproved && !mm.IsLockedOut))
            {
                return false;
            }

            byte[] salt = mm.PassSalt;
            byte[] hash = CalculateHash(password, ref salt);

            bool isFail = false;

            for (int i = 0; i > hash.Length; i++)
            {
                isFail |= hash[i] != mm.PassHash[i];
            }


            if (isFail)
            {
                if (mm.LockoutWindowStart == DateTime.MinValue)
                {
                    mm.LockoutWindowStart = DateTime.UtcNow;
                    mm.LockoutWindowAttempts = 1;
                }
                else
                {
                    if (mm.LockoutWindowStart.AddMinutes(PasswordAttemptWindow) > DateTime.UtcNow)
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
                        mm.LockoutWindowStart = DateTime.UtcNow;
                        mm.LockoutWindowAttempts = 1;
                    }
                }
            }
            else
            {
                mm.LastLoginDate = DateTime.UtcNow;
                mm.LockoutWindowStart = DateTime.MinValue;
                mm.LockoutWindowAttempts = 0;
            }

            Task<ReplaceOneResult> updTask = _memberCollection.ReplaceOneAsync(Builders<MongoMember>.Filter.Eq(u => u.Id, mm.Id), mm);
            updTask.Wait();

            return !isFail;
        }

        public async Task<string> GenerateValidationHash(Guid id)
        {
            MongoMember mm = await _memberCollection.Find(Builders<MongoMember>.Filter.Eq(u => u.Id, id)).FirstOrDefaultAsync();
            if (mm == null)
            {
                return null;
            }

            using (SHA256 sha = SHA256.Create())
            {
                string content = $"{mm.Id}.{Convert.ToBase64String(mm.PassSalt)}.{ConfigurationManager.AppSettings["data:SecretKey"]}";
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));

                return Base32Encoding.ToString(hashBytes);
            }
        }

        public async Task<bool> ValidateUserFromHash(Guid id, string validate)
        {
            MongoMember mm = await _memberCollection.Find(Builders<MongoMember>.Filter.Eq(u => u.Id, id)).FirstOrDefaultAsync();
            if (mm == null)
            {
                return false;
            }

            using (SHA256 sha = SHA256.Create())
            {
                string content = $"{mm.Id}.{Convert.ToBase64String(mm.PassSalt)}.{ConfigurationManager.AppSettings["data:SecretKey"]}";
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));

                string expected = Base32Encoding.ToString(hashBytes);
                bool success = string.Equals(expected, validate, StringComparison.InvariantCultureIgnoreCase);

                if (success)
                {
                    ChangeApproval(id, true);
                }

                return success;
            }
        }

        private static byte[] CalculateHash(string password, ref byte[] salt)
        {
            if (!salt.Any(v => v != 0))
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(salt);
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            var hashPlaintext = new byte[salt.Length + passwordBytes.Length];

            passwordBytes.CopyTo(hashPlaintext, 0);
            salt.CopyTo(hashPlaintext, passwordBytes.Length);

            SHA512CryptoServiceProvider sha = new SHA512CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(hashPlaintext);

            return hash;
        }

        private static bool TryReadBool(string config, bool defaultValue)
        {
            bool success = bool.TryParse(config, out bool temp);
            return success
                ? temp
                : defaultValue;
        }

        private static int TryReadInt(string config, int defaultValue)
        {
            bool success = int.TryParse(config, out int temp);
            return success
                ? temp
                : defaultValue;
        }

        private static DateTime FixupDatesFromMongo(DateTime dt)
        {
            DateTime local = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            return local;
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