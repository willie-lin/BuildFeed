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
   public class MetaItemModel
   {
      [BsonId]
      public MetaItemKey Id { get; set; }

      public string PageContent { get; set; }

      public string MetaDescription { get; set; }
   }

   public class MetaItem
   {
      private const string _buildCollectionName = "metaitem";

      private MongoClient _dbClient;
      private IMongoCollection<MetaItemModel> _buildCollection;

      public MetaItem()
      {
         _dbClient = new MongoClient(new MongoClientSettings()
         {
            Server = new MongoServerAddress("localhost", 27017)
         });

         _buildCollection = _dbClient.GetDatabase("BuildFeed").GetCollection<MetaItemModel>(_buildCollectionName);
      }

      public List<MetaItemModel> Select()
      {
         var task = _buildCollection.Find(new BsonDocument()).ToListAsync();
         task.Wait();
         return task.Result;
      }

      public void Insert(MetaItemModel item)
      {
         var task = _buildCollection.InsertOneAsync(item);
         task.Wait();
      }

      public void InsertAll(IEnumerable<MetaItemModel> items)
      {
         var task = _buildCollection.InsertManyAsync(items);
         task.Wait();
      }
   }
}
