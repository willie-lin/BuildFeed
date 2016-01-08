using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BuildFeed.Models
{

   public partial class Build
   {
      private const string _buildCollectionName = "builds";

      private MongoClient _dbClient;
      private IMongoCollection<BuildModel> _buildCollection;

      public Build()
      {
         _dbClient = new MongoClient(new MongoClientSettings()
         {
            Server = new MongoServerAddress(MongoConfig.Host, MongoConfig.Port)
         });

         _buildCollection = _dbClient.GetDatabase(MongoConfig.Database).GetCollection<BuildModel>(_buildCollectionName);
      }

      public async Task SetupIndexes()
      {
         var indexes = await (await _buildCollection.Indexes.ListAsync()).ToListAsync();
         if(!indexes.Any(i => i["name"] == "_idx_group"))
         {
            await _buildCollection.Indexes.CreateOneAsync(Builders<BuildModel>.IndexKeys.Combine(
               Builders<BuildModel>.IndexKeys.Descending(b => b.MajorVersion),
               Builders<BuildModel>.IndexKeys.Descending(b => b.MinorVersion),
               Builders<BuildModel>.IndexKeys.Descending(b => b.Number),
               Builders<BuildModel>.IndexKeys.Descending(b => b.Revision)
               ), new CreateIndexOptions()
               {
                  Name = "_idx_group"
               });
         }
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<List<BuildModel>> Select()
      {
         return await _buildCollection.Find(new BsonDocument()).ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<List<BuildModel>> SelectLatest(int limit, int skip)
      {
         return await _buildCollection.Find(new BsonDocument())
            .SortByDescending(b => b.Added)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<List<BuildModel>> SelectLatestLeaked(int limit, int skip)
      {
         return await _buildCollection.Find(b => b.LeakDate != null)
            .SortByDescending(b => b.LeakDate)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<BuildModel> SelectById(Guid id)
      {
         return await _buildCollection.Find(f => f.Id == id).SingleOrDefaultAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<BuildModel> SelectByLegacyId(long id)
      {
         return await _buildCollection.Find(f => f.LegacyId == id).SingleOrDefaultAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectInBuildOrder()
      {
         return await _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .ToListAsync();

      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectInBuildOrder(int limit, int skip)
      {
         return await _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectFlight(LevelOfFlight flight, int limit, int skip)
      {
         return await _buildCollection.Find(b => b.FlightLevel == flight)
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Insert, true)]
      public async Task Insert(BuildModel item)
      {
         item.Id = Guid.NewGuid();
         item.LabUrl = item.GenerateLabUrl();
         await _buildCollection.InsertOneAsync(item);
      }

      [DataObjectMethod(DataObjectMethodType.Insert, false)]
      public async Task InsertAll(IEnumerable<BuildModel> items)
      {
         foreach(var item in items)
         {
            item.Id = Guid.NewGuid();
            item.LabUrl = item.GenerateLabUrl();
         }

         await _buildCollection.InsertManyAsync(items);
      }

      [DataObjectMethod(DataObjectMethodType.Update, true)]
      public async Task Update(BuildModel item)
      {
         BuildModel old = await SelectById(item.Id);
         item.Added = old.Added;
         item.Modified = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
         item.LabUrl = item.GenerateLabUrl();

         await _buildCollection.ReplaceOneAsync(f => f.Id == item.Id, item);
      }

      [DataObjectMethod(DataObjectMethodType.Delete, true)]
      public async Task DeleteById(Guid id)
      {
         await _buildCollection.DeleteOneAsync(f => f.Id == id);
      }
   }
}