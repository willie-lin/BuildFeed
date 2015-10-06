using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisMongoMigration.Mongo
{
   public class MemberModel
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

   public class MongoMember
   {
      private const string _buildCollectionName = "members";

      private MongoClient _dbClient;
      private IMongoCollection<MemberModel> _buildCollection;

      public MongoMember()
      {
         _dbClient = new MongoClient(new MongoClientSettings()
         {
            Server = new MongoServerAddress("localhost", 27017)
         });

         _buildCollection = _dbClient.GetDatabase("BuildFeed").GetCollection<MemberModel>(_buildCollectionName);
      }

      public List<MemberModel> Select()
      {
         var task = _buildCollection.Find(new BsonDocument()).ToListAsync();
         task.Wait();
         return task.Result;
      }

      public void Insert(MemberModel item)
      {
         item.Id = Guid.NewGuid();
         var task = _buildCollection.InsertOneAsync(item);
         task.Wait();
      }

      public void InsertAll(IEnumerable<MemberModel> items)
      {
         var task = _buildCollection.InsertManyAsync(items);
         task.Wait();
      }
   }
}
