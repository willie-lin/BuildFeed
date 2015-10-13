using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Security;
using System.Collections.Specialized;
using System.Configuration.Provider;

namespace MongoAuth
{
    public class MongoRoleProvider : RoleProvider
    {
        private const string _roleCollectionName = "roles";
        private const string _memberCollectionName = "members";
        private MongoClient _dbClient;
        private IMongoCollection<MongoRole> _roleCollection;
        private IMongoCollection<MongoMember> _memberCollection;

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

            _roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(_roleCollectionName);
            _memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(_memberCollectionName);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            var roleTask = _roleCollection
                .Find(r => roleNames.Contains(r.RoleName))
                .ToListAsync();
            roleTask.Wait();
            List<MongoRole> roles = roleTask.Result;

            var userTask = _memberCollection
                .Find(u => usernames.Contains(u.UserName))
                .ToListAsync();
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

                var update = _roleCollection
                    .ReplaceOneAsync(Builders<MongoRole>.Filter.Eq(r => r.Id, roles[i].Id), roles[i]);
                update.Wait();
            }
        }

        public override void CreateRole(string roleName)
        {
            MongoRole r = new MongoRole()
            {
                Id = Guid.NewGuid(),
                RoleName = roleName
            };

            var task = _roleCollection
                .InsertOneAsync(r);
            task.Wait();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            var role = _roleCollection
                .Find(r => r.RoleName == roleName)
                .SingleOrDefaultAsync();
            role.Wait();

            if (role.Result != null && role.Result.Users.Length > 0 && throwOnPopulatedRole)
            {
                throw new ProviderException(Properties.Resources.RoleNotEmpty);
            }

            var task = _roleCollection
                .DeleteOneAsync(r => r.RoleName == roleName);
            task.Wait();

            return true;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            var role = _roleCollection
                .Find(r => r.RoleName == roleName)
                .SingleOrDefaultAsync();
            role.Wait();

            if (role == null)
            {
                return Array.Empty<string>();
            }

            var users = _memberCollection
                .Find(u => role.Result.Users.Contains(u.Id) && u.UserName.ToLower().Contains(usernameToMatch.ToLower()))
                .ToListAsync();
            users.Wait();

            return users.Result
                .Select(r => r.UserName)
                .ToArray();
        }

        public override string[] GetAllRoles()
        {
            var roles = _roleCollection
                .Find(new BsonDocument())
                .ToListAsync();
            roles.Wait();

            return roles.Result
                .Select(r => r.RoleName)
                .ToArray();
        }

        public override string[] GetRolesForUser(string username)
        {
            var user = _memberCollection
                .Find(u => u.UserName.ToLower() == username.ToLower())
                .SingleOrDefaultAsync();
            user.Wait();

            if (user == null)
            {
                return Array.Empty<string>();
            }

            var role = _roleCollection
                .Find(r => r.Users != null && r.Users.Contains(user.Result.Id))
                .ToListAsync();
            role.Wait();

            return role.Result
                .Select(r => r.RoleName)
                .ToArray();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            var role = _roleCollection
                .Find(r => r.RoleName == roleName)
                .SingleOrDefaultAsync();
            role.Wait();

            if (role == null)
            {
                return Array.Empty<string>();
            }

            var users = _memberCollection
                .Find(u => role.Result.Users.Contains(u.Id))
                .ToListAsync();
            users.Wait();

            return users.Result
                .Select(u => u.UserName)
                .ToArray();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var user = _memberCollection
                .Find(u => u.UserName.ToLower() == username.ToLower())
                .SingleOrDefaultAsync();
            var role = _roleCollection
                .Find(r => r.RoleName == roleName)
                .SingleOrDefaultAsync();
            user.Wait();
            role.Wait();

            if (user.Result == null || role.Result == null || role.Result.Users == null)
            {
                return false;
            }

            return role.Result.Users.Contains(user.Result.Id);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            var roleTask = _roleCollection
                .Find(r => roleNames.Contains(r.RoleName))
                .ToListAsync();
            roleTask.Wait();
            List<MongoRole> roles = roleTask.Result;

            var userTask = _memberCollection
                .Find(u => usernames.Contains(u.UserName))
                .ToListAsync();
            userTask.Wait();
            List<MongoMember> users = userTask.Result;

            for (int i = 0; i < roles.Count; i++)
            {
                roles[i].Users = (from u in roles[i].Users
                                  where !users.Any(v => v.Id == u)
                                  select u).ToArray();

                var update = _roleCollection
                    .ReplaceOneAsync(Builders<MongoRole>.Filter.Eq(r => r.Id, roles[i].Id), roles[i]);
                update.Wait();
            }
        }

        public override bool RoleExists(string roleName)
        {
            var role = _roleCollection
                .Find(r => r.RoleName == roleName)
                .SingleOrDefaultAsync();
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