using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration.Provider;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Security;
using MongoAuth.Properties;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace MongoAuth
{
   public class MongoRoleProvider : RoleProvider
   {
      private const string MEMBER_COLLECTION_NAME = "members";
      private const string ROLE_COLLECTION_NAME = "roles";
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

         _dbClient = new MongoClient(new MongoClientSettings
         {
            Server = new MongoServerAddress(DatabaseConfig.Host, DatabaseConfig.Port)
         });

         _roleCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoRole>(ROLE_COLLECTION_NAME);
         _memberCollection = _dbClient.GetDatabase(DatabaseConfig.Database).GetCollection<MongoMember>(MEMBER_COLLECTION_NAME);
      }

      public override void AddUsersToRoles(string[] usernames, string[] roleNames)
      {
         Task<List<MongoRole>> roleTask = _roleCollection.Find(r => roleNames.Contains(r.RoleName)).ToListAsync();
         roleTask.Wait();
         List<MongoRole> roles = roleTask.Result;

         Task<List<MongoMember>> userTask = _memberCollection.Find(u => usernames.Contains(u.UserName)).ToListAsync();
         userTask.Wait();
         List<MongoMember> users = userTask.Result;

         for (int i = 0; i < roles.Count; i++)
         {
            List<Guid> newUsers = new List<Guid>();

            if (roles[i].Users != null)
            {
               newUsers.AddRange(roles[i].Users);
            }

            IEnumerable<Guid> usersToAdd = from u in users
                                           where newUsers.All(v => v != u.Id)
                                           select u.Id;

            newUsers.AddRange(usersToAdd);

            roles[i].Users = newUsers.ToArray();

            Task<ReplaceOneResult> update = _roleCollection.ReplaceOneAsync(Builders<MongoRole>.Filter.Eq(r => r.Id, roles[i].Id), roles[i]);
            update.Wait();
         }
      }

      public override void CreateRole(string roleName)
      {
         MongoRole r = new MongoRole
         {
            Id = Guid.NewGuid(),
            RoleName = roleName
         };

         Task task = _roleCollection.InsertOneAsync(r);
         task.Wait();
      }

      public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
      {
         Task<MongoRole> role = _roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
         role.Wait();

         if (role.Result != null
            && role.Result.Users.Length > 0
            && throwOnPopulatedRole)
         {
            throw new ProviderException(Resources.RoleNotEmpty);
         }

         Task<DeleteResult> task = _roleCollection.DeleteOneAsync(r => r.RoleName == roleName);
         task.Wait();

         return true;
      }

      public override string[] FindUsersInRole(string roleName, string usernameToMatch)
      {
         Task<MongoRole> role = _roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
         role.Wait();

         if (role.Result == null)
         {
            return Array.Empty<string>();
         }

         Task<List<MongoMember>> users = _memberCollection.Find(u => role.Result.Users.Contains(u.Id) && u.UserName.ToLower().Contains(usernameToMatch.ToLower())).ToListAsync();
         users.Wait();

         return users.Result.Select(r => r.UserName).ToArray();
      }

      public override string[] GetAllRoles()
      {
         Task<List<MongoRole>> roles = _roleCollection.Find(new BsonDocument()).ToListAsync();
         roles.Wait();

         return roles.Result.Select(r => r.RoleName).ToArray();
      }

      public override string[] GetRolesForUser(string username)
      {
         Task<MongoMember> user = _memberCollection.Find(u => u.UserName.ToLower() == username.ToLower()).SingleOrDefaultAsync();
         user.Wait();

         if (user.Result == null)
         {
            return Array.Empty<string>();
         }

         Task<List<MongoRole>> role = _roleCollection.Find(r => r.Users != null && r.Users.Contains(user.Result.Id)).ToListAsync();
         role.Wait();

         return role.Result.Select(r => r.RoleName).ToArray();
      }

      public override string[] GetUsersInRole(string roleName)
      {
         Task<MongoRole> role = _roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
         role.Wait();

         if (role.Result == null)
         {
            return Array.Empty<string>();
         }

         Task<List<MongoMember>> users = _memberCollection.Find(u => role.Result.Users.Contains(u.Id)).ToListAsync();
         users.Wait();

         return users.Result.Select(u => u.UserName).ToArray();
      }

      public override bool IsUserInRole(string username, string roleName)
      {
         Task<MongoMember> user = _memberCollection.Find(u => u.UserName.ToLower() == username.ToLower()).SingleOrDefaultAsync();
         Task<MongoRole> role = _roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
         user.Wait();
         role.Wait();

         if (user.Result == null
            || role.Result?.Users == null)
         {
            return false;
         }

         return role.Result.Users.Contains(user.Result.Id);
      }

      public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
      {
         Task<List<MongoRole>> roleTask = _roleCollection.Find(r => roleNames.Contains(r.RoleName)).ToListAsync();
         roleTask.Wait();
         List<MongoRole> roles = roleTask.Result;

         Task<List<MongoMember>> userTask = _memberCollection.Find(u => usernames.Contains(u.UserName)).ToListAsync();
         userTask.Wait();
         List<MongoMember> users = userTask.Result;

         foreach (MongoRole t in roles)
         {
            t.Users = (from u in t.Users
                       where users.All(v => v.Id != u)
                       select u).ToArray();

            Task<ReplaceOneResult> update = _roleCollection.ReplaceOneAsync(Builders<MongoRole>.Filter.Eq(r => r.Id, t.Id), t);
            update.Wait();
         }
      }

      public override bool RoleExists(string roleName)
      {
         Task<MongoRole> role = _roleCollection.Find(r => r.RoleName == roleName).SingleOrDefaultAsync();
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