using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using NServiceKit.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Security;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace BuildFeed.Auth
{
    public class RedisRoleProvider : RoleProvider
    {
        public override string ApplicationName
        {
            get { return ""; }
            set { }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();
                var uClient = rClient.As<RedisMember>();

                List<RedisRole> roles = new List<RedisRole>();
                roles.AddRange(from r in client.GetAll()
                               where roleNames.Any(n => n == r.RoleName)
                               select r);

                List<RedisMember> users = new List<RedisMember>();
                users.AddRange(from u in uClient.GetAll()
                               where usernames.Any(n => n == u.UserName)
                               select u);

                for (int i = 0; i < roles.Count; i++)
                {
                    List<Guid> newUsers = new List<Guid>();

                    if(roles[i].Users != null)
                    {
                        var usersToAdd = from u in users
                                         where !roles[i].Users.Any(v => v == u.Id)
                                         select u.Id;

                        newUsers.AddRange(roles[i].Users);

                        newUsers.AddRange(usersToAdd);
                    }

                    roles[i].Users = newUsers.ToArray();
                }

                client.StoreAll(roles);
            }
        }

        public override void CreateRole(string roleName)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();

                RedisRole rr = new RedisRole()
                {
                    Id = Guid.NewGuid(),
                    RoleName = roleName
                };

                client.Store(rr);
            }
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();

                var role = client.GetAll().SingleOrDefault(r => r.RoleName == roleName);

                if (role.Users.Length > 0 && throwOnPopulatedRole)
                {
                    throw new Exception("This role still has users");
                }

                client.DeleteById(role.Id);
                return true;
            }
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();
                var uClient = rClient.As<RedisMember>();

                var userIds = from r in client.GetAll()
                              where r.RoleName == roleName
                              from u in r.Users
                              select u;

                var users = uClient.GetByIds(userIds);

                return (from u in users
                        where u.UserName.Contains(usernameToMatch)
                        select u.UserName).ToArray();
            }
        }

        public override string[] GetAllRoles()
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();

                return (from r in client.GetAll()
                        select r.RoleName).ToArray();
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();
                var uClient = rClient.As<RedisMember>();

                var user = uClient.GetAll().SingleOrDefault(u => u.UserName == username);

                if (user == null)
                {
                    throw new Exception("Username does not exist");
                }

                return (from r in client.GetAll()
                        where r.Users != null
                        where r.Users.Any(u => u == user.Id)
                        select r.RoleName).ToArray();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();
                var uClient = rClient.As<RedisMember>();

                var userIds = from r in client.GetAll()
                              where r.RoleName == roleName
                              from u in r.Users
                              select u;

                var users = uClient.GetByIds(userIds);

                return (from u in users
                        select u.UserName).ToArray();
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();
                var uClient = rClient.As<RedisMember>();

                var user = uClient.GetAll().SingleOrDefault(u => u.UserName == username);

                if (user == null)
                {
                    throw new Exception();
                }

                var role = client.GetAll().SingleOrDefault(r => r.RoleName == roleName);

                if(role.Users == null)
                {
                    return false;
                }

                return role.Users.Any(u => u == user.Id);
            }
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();
                var uClient = rClient.As<RedisMember>();

                List<RedisRole> roles = new List<RedisRole>();
                roles.AddRange(from r in client.GetAll()
                               where roleNames.Any(n => n == r.RoleName)
                               select r);

                List<RedisMember> users = new List<RedisMember>();
                users.AddRange(from u in uClient.GetAll()
                               where usernames.Any(n => n == u.UserName)
                               select u);

                for (int i = 0; i < roles.Count; i++)
                {
                    roles[i].Users = (from u in roles[i].Users
                                      where !users.Any(v => v.Id == u)
                                      select u).ToArray();
                }

                client.StoreAll(roles);
            }
        }

        public override bool RoleExists(string roleName)
        {
            using (RedisClient rClient = new RedisClient(DatabaseConfig.Host, DatabaseConfig.Port, db: DatabaseConfig.Database))
            {
                var client = rClient.As<RedisRole>();

                return client.GetAll().Any(r => r.RoleName == roleName);
            }
        }
    }

    [DataObject]
    public class RedisRole : IHasId<Guid>
    {
        [Key]
        [Index]
        public Guid Id { get; set; }

        [@Required]
        [DisplayName("Role name")]
        [Key]
        public string RoleName { get; set; }

        public Guid[] Users { get; set; }
    }
}