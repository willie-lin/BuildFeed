using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Security;
using System.Collections.Specialized;

namespace MongoAuth
{
    public class MongoRoleProvider : RoleProvider
    {
        private const string _RoleCollectionName = "roles";
        private const string _MemberCollectionName = "members";
        private MongoClient _dbClient;

        public override string ApplicationName
        {
            get { return ""; }
            set { }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);

            _dbClient = new MongoClient(new MongoClientSettings()
            {
                Server = new MongoServerAddress(DatabaseConfig.Host, DatabaseConfig.Port)
            });
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);
            var memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(_MemberCollectionName);

            var roleTask = roleCollection.Find(r => roleNames.Contains(r.RoleName)).ToListAsync();
            roleTask.Wait();
            List<MongoRole> roles = roleTask.Result;

            var userTask = memberCollection.Find(u => usernames.Contains(u.UserName)).ToListAsync();
            userTask.Wait();
            List<MongoMember> users = userTask.Result;

            for (int i = 0; i < roles.Count; i++)
            {
                List<Guid> newUsers = new List<Guid>();

                if (roles[i].Users != null)
                {
                    newUsers.AddRange(roles[i].Users);
                }

                var usersToAdd = from u in users
                                 where !newUsers.Any(v => v == u.Id)
                                 select u.Id;

                newUsers.AddRange(usersToAdd);

                roles[i].Users = newUsers.ToArray();

                var update = roleCollection.ReplaceOneAsync(Builders<MongoRole>.Filter.Eq(r => r.Id, roles[i].Id), roles[i]);
                update.Wait();
            }
        }

        public override void CreateRole(string roleName)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);

            MongoRole r = new MongoRole()
            {
                Id = Guid.NewGuid(),
                RoleName = roleName
            };

            var task = roleCollection.InsertOneAsync(r);
            task.Wait();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);

            var role = roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
            role.Wait();

            if (role.Result != null && role.Result.Users.Length > 0 && throwOnPopulatedRole)
            {
                throw new Exception("This role still has users");
            }

            roleCollection.DeleteOneAsync(r => r.RoleName == roleName);
            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);
            var memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(_MemberCollectionName);

            var role = roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
            role.Wait();

            if (role == null)
            {
                throw new Exception("Role does not exist");
            }

            var users = memberCollection.Find(u => role.Result.Users.Contains(u.Id) && u.UserName.ToLower().Contains(usernameToMatch.ToLower())).ToListAsync();
            users.Wait();

            return users.Result
                .Select(r => r.UserName)
                .ToArray();
        }

        public override string[] GetAllRoles()
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);
            var roles = roleCollection.Find(new BsonDocument()).ToListAsync();
            roles.Wait();
            return roles.Result
                .Select(r => r.RoleName)
                .ToArray();
        }

        public override string[] GetRolesForUser(string username)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);
            var memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(_MemberCollectionName);

            var user = memberCollection.Find(u => u.UserName.ToLower() == username.ToLower()).SingleOrDefaultAsync();
            user.Wait();

            if (user == null)
            {
                throw new Exception("User does not exist");
            }

            var role = roleCollection.Find(new BsonDocument()).ToListAsync();
            role.Wait();

            return (from r in role.Result
                    where r.Users != null
                    where r.Users.Any(u => u == user.Result.Id)
                    select r.RoleName).ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);
            var memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(_MemberCollectionName);

            var role = roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
            role.Wait();

            if (role == null)
            {
                throw new Exception("Role does not exist");
            }

            var users = memberCollection.Find(u => role.Result.Users.Contains(u.Id)).ToListAsync();
            users.Wait();

            return users.Result
                .Select(u => u.UserName)
                .ToArray();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);
            var memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(_MemberCollectionName);

            var user = memberCollection.Find(u => u.UserName.ToLower() == username.ToLower()).SingleOrDefaultAsync();
            user.Wait();

            if (user.Result == null)
            {
                throw new Exception("User does not exist");
            }

            var role = roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();

            if (role.Result == null)
            {
                throw new Exception("Role does not exist");
            }

            if (role.Result.Users == null)
            {
                return false;
            }

            return role.Result.Users.Any(u => u == user.Result.Id);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);
            var memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(_MemberCollectionName);

            var roleTask = roleCollection.Find(r => roleNames.Any(n => n == r.RoleName)).ToListAsync();
            roleTask.Wait();
            List<MongoRole> roles = roleTask.Result;

            var userTask = memberCollection.Find(u => usernames.Any(n => n == u.UserName)).ToListAsync();
            userTask.Wait();
            List<MongoMember> users = userTask.Result;

            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].Users = (from u in roles[i].Users
                                  where !users.Any(v => v.Id == u)
                                  select u).ToArray();

                var update = roleCollection.ReplaceOneAsync(Builders<MongoRole>.Filter.Eq(r => r.Id, roles[i].Id), roles[i]);
                update.Wait();
            }
        }

        public override bool RoleExists(string roleName)
        {
            var roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_RoleCollectionName);

            var role = roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
            role.Wait();

            return role.Result != null;
        }
    }

    [DataObject]
    public class MongoRole
    {
        [BsonId]
        public Guid Id { get; set; }

        public string RoleName { get; set; }

        public Guid[] Users { get; set; }
    }
}