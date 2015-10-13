using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace RedisMongoMigration.Mongo
{
   public class BuildModel
   {
      [BsonId]
      public Guid Id { get; set; }
      public long? LegacyId { get; set; }
      public byte MajorVersion { get; set; }
      public byte MinorVersion { get; set; }
      public ushort Number { get; set; }
      public ushort? Revision { get; set; }
      public string Lab { get; set; }
      public DateTime? BuildTime { get; set; }

      public DateTime Added { get; set; }
      public DateTime Modified { get; set; }
      public TypeOfSource SourceType { get; set; }
      public string SourceDetails { get; set; }
      public DateTime? LeakDate { get; set; }
      public MongoLevelOfFlight FlightLevel { get; set; }

      public string LabUrl { get; set; }

      public bool IsLeaked
      {
         get
         {
            switch (SourceType)
            {
               case TypeOfSource.PublicRelease:
               case TypeOfSource.InternalLeak:
               case TypeOfSource.UpdateGDR:
               case TypeOfSource.UpdateLDR:
                  return true;
               default:
                  return false;
            }
         }
      }

      public string FullBuildString
      {
         get
         {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}.{1}.{2}", MajorVersion, MinorVersion, Number);

            if (Revision.HasValue)
            {
               sb.AppendFormat(".{0}", Revision);
            }

            if (!string.IsNullOrWhiteSpace(Lab))
            {
               sb.AppendFormat(".{0}", Lab);
            }

            if (BuildTime.HasValue)
            {
               sb.AppendFormat(".{0:yyMMdd-HHmm}", BuildTime);
            }

            return sb.ToString();
         }
      }

      public string GenerateLabUrl() => (Lab ?? "").Replace('/', '-').ToLower();
   }

   public class Build
   {
      private const string _buildCollectionName = "builds";

      private MongoClient _dbClient;
      private IMongoCollection<BuildModel> _buildCollection;

      public Build()
      {
         _dbClient = new MongoClient(new MongoClientSettings()
         {
            Server = new MongoServerAddress("localhost", 27017)
         });

         _buildCollection = _dbClient.GetDatabase("BuildFeed").GetCollection<BuildModel>(_buildCollectionName);
      }

      public List<BuildModel> Select()
      {
         var task = _buildCollection.Find(new BsonDocument()).ToListAsync();
         task.Wait();
         return task.Result;
      }

      public void Insert(BuildModel item)
      {
         item.Id = Guid.NewGuid();
         var task = _buildCollection.InsertOneAsync(item);
         task.Wait();
      }

      public void InsertAll(IEnumerable<BuildModel> items)
      {
         var task = _buildCollection.InsertManyAsync(items);
         task.Wait();
      }
   }

   public enum MongoLevelOfFlight
   {
      None = 0,
      WIS = 1,
      WIF = 2,
      OSG = 3,
      MSIT = 4,
      Canary = 5
   }
}
