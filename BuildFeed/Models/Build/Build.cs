using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BuildFeed.Models.ViewModel.Front;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Models
{
   public partial class Build
   {
      private const string BUILD_COLLECTION_NAME = "builds";
      private static readonly BsonDocument sortByAddedDate = new BsonDocument(nameof(BuildModel.Added), -1);
      private static readonly BsonDocument sortByCompileDate = new BsonDocument(nameof(BuildModel.BuildTime), -1);
      private static readonly BsonDocument sortByLeakedDate = new BsonDocument(nameof(BuildModel.LeakDate), -1);

      private static readonly BsonDocument sortByOrder = new BsonDocument
      {
         new BsonElement(nameof(BuildModel.MajorVersion), -1),
         new BsonElement(nameof(BuildModel.MinorVersion), -1),
         new BsonElement(nameof(BuildModel.Number), -1),
         new BsonElement(nameof(BuildModel.Revision), -1),
         new BsonElement(nameof(BuildModel.BuildTime), -1)
      };

      private readonly IMongoCollection<BuildModel> _buildCollection;
      private readonly IMongoDatabase _buildDatabase;
      private readonly MongoClient _dbClient;

      public Build()
      {
         _dbClient = new MongoClient(new MongoClientSettings
         {
            Server = new MongoServerAddress(MongoConfig.Host, MongoConfig.Port)
         });

         _buildDatabase = _dbClient.GetDatabase(MongoConfig.Database);
         _buildCollection = _buildDatabase.GetCollection<BuildModel>(BUILD_COLLECTION_NAME);
      }

      public async Task SetupIndexes()
      {
         List<BsonDocument> indexes = await (await _buildCollection.Indexes.ListAsync()).ToListAsync();
         if (indexes.All(i => i["name"] != "_idx_group"))
         {
            await _buildCollection.Indexes.CreateOneAsync(Builders<BuildModel>.IndexKeys.Combine(Builders<BuildModel>.IndexKeys.Descending(b => b.MajorVersion), Builders<BuildModel>.IndexKeys.Descending(b => b.MinorVersion), Builders<BuildModel>.IndexKeys.Descending(b => b.Number), Builders<BuildModel>.IndexKeys.Descending(b => b.Revision)), new CreateIndexOptions
            {
               Name = "_idx_group"
            });
         }

         if (indexes.All(i => i["name"] != "_idx_legacy"))
         {
            await _buildCollection.Indexes.CreateOneAsync(Builders<BuildModel>.IndexKeys.Ascending(b => b.LegacyId), new CreateIndexOptions
            {
               Name = "_idx_legacy"
            });
         }

         if (indexes.All(i => i["name"] != "_idx_lab"))
         {
            await _buildCollection.Indexes.CreateOneAsync(Builders<BuildModel>.IndexKeys.Ascending(b => b.Lab), new CreateIndexOptions
            {
               Name = "_idx_lab"
            });
         }
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<List<BuildModel>> Select() => await _buildCollection.Find(new BsonDocument()).ToListAsync();

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<BuildModel> SelectById(Guid id) => await _buildCollection.Find(Builders<BuildModel>.Filter.Eq(b => b.Id, id)).SingleOrDefaultAsync();

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<BuildModel> SelectByLegacyId(long id) => await _buildCollection.Find(Builders<BuildModel>.Filter.Eq(b => b.LegacyId, id)).SingleOrDefaultAsync();

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectBuildsByOrder(int limit = -1, int skip = 0)
      {
         IFindFluent<BuildModel, BuildModel> query = _buildCollection.Find(new BsonDocument()).Sort(sortByOrder).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<FrontPage> SelectFrontPage()
      {
         FrontPage fp = new FrontPage();

         IFindFluent<BuildModel, BuildModel> query = _buildCollection.Find(new BsonDocument
         {
            { nameof(BuildModel.LabUrl), new BsonDocument
            {
               { "$in", new BsonArray(ConfigurationManager.AppSettings["site:OSGLab"].Split(';')) }
            } }
         }).Sort(sortByCompileDate).Limit(1);

         fp.CurrentCanary = (await query.ToListAsync())[0];

         query = _buildCollection.Find(new BsonDocument
         {
            { nameof(BuildModel.LabUrl), new BsonDocument
            {
               { "$in", new BsonArray(ConfigurationManager.AppSettings["site:InsiderLab"].Split(';')) }
            } },
            { nameof(BuildModel.SourceType), new BsonDocument
            {
               { "$in", new BsonArray()
               {
                  TypeOfSource.PublicRelease, TypeOfSource.UpdateGDR
               } }
            } }
         }).Sort(sortByCompileDate).Limit(1);

         fp.CurrentInsider = (await query.ToListAsync())[0];

         query = _buildCollection.Find(new BsonDocument
         {
            { nameof(BuildModel.LabUrl), new BsonDocument
            {
               { "$in", new BsonArray(ConfigurationManager.AppSettings["site:ReleaseLab"].Split(';')) }
            } }
         }).Sort(sortByCompileDate).Limit(1);

         fp.CurrentRelease = (await query.ToListAsync())[0];

         return fp;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectBuildsByCompileDate(int limit = -1, int skip = 0)
      {
         IFindFluent<BuildModel, BuildModel> query = _buildCollection.Find(new BsonDocument()).Sort(sortByCompileDate).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectBuildsByAddedDate(int limit = -1, int skip = 0)
      {
         IFindFluent<BuildModel, BuildModel> query = _buildCollection.Find(new BsonDocument()).Sort(sortByAddedDate).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectBuildsByLeakedDate(int limit = -1, int skip = 0)
      {
         IFindFluent<BuildModel, BuildModel> query = _buildCollection.Find(new BsonDocument()).Sort(sortByLeakedDate).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
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
         List<BuildModel> generatedItems = new List<BuildModel>();
         foreach (BuildModel item in items)
         {
            item.Id = Guid.NewGuid();
            item.LabUrl = item.GenerateLabUrl();

            generatedItems.Add(item);
         }

         await _buildCollection.InsertManyAsync(generatedItems);
      }

      [DataObjectMethod(DataObjectMethodType.Update, true)]
      public async Task Update(BuildModel item)
      {
         BuildModel old = await SelectById(item.Id);
         item.Added = old.Added;
         item.Modified = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
         item.LabUrl = item.GenerateLabUrl();

         await _buildCollection.ReplaceOneAsync(Builders<BuildModel>.Filter.Eq(b => b.Id, item.Id), item);
      }

      [DataObjectMethod(DataObjectMethodType.Delete, true)]
      public async Task DeleteById(Guid id) { await _buildCollection.DeleteOneAsync(Builders<BuildModel>.Filter.Eq(b => b.Id, id)); }
   }
}