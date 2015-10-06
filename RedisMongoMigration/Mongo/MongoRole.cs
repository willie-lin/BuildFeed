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
   public class RoleModel
   {
      [BsonId]
      public Guid Id { get; set; }

      public string RoleName { get; set; }

      public Guid[] Users { get; set; }
   }

   public class MongoRole
   {
      private const string _buildCollectionName = "roles";

      private MongoClient _dbClient;
      private IMongoCollection<RoleModel> _buildCollection;

      public MongoRole()
      {
         _dbClient = new MongoClient(new MongoClientSettings()
         {
            Server = new MongoServerAddress("localhost", 27017)
         });

         _buildCollection = _dbClient.GetDatabase("BuildFeed").GetCollection<RoleModel>(_buildCollectionName);
      }

      public List<RoleModel> Select()
      {
         var task = _buildCollection.Find(new BsonDocument()).ToListAsync();
         task.Wait();
         return task.Result;
      }

      public void Insert(RoleModel item)
      {
         item.Id = Guid.NewGuid();
         var task = _buildCollection.InsertOneAsync(item);
         task.Wait();
      }

      public void InsertAll(IEnumerable<RoleModel> items)
      {
         var task = _buildCollection.InsertManyAsync(items);
         task.Wait();
      }
   }
}
