using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoTimeFix
{
   class Program
   {

      static void Main(string[] args)
      {
         const string _buildCollectionName = "builds";

         MongoClient _dbClient = new MongoClient(new MongoClientSettings()
         {
            Server = new MongoServerAddress("localhost", 27017)
         });

         IMongoCollection<BuildModel> _buildCollection = _dbClient.GetDatabase("BuildFeed").GetCollection<BuildModel>(_buildCollectionName);

         var task = _buildCollection.Find(new BsonDocument()).ToListAsync();
         task.Wait();

         foreach(var b in task.Result)
         {
            if (b.BuildTime.HasValue)
            {
               b.BuildTime = DateTime.SpecifyKind(b.BuildTime.Value.AddHours(1), DateTimeKind.Utc);
            }
            if (b.LeakDate.HasValue)
            {
               b.LeakDate = DateTime.SpecifyKind(b.LeakDate.Value.AddHours(1), DateTimeKind.Utc);
            }
            _buildCollection.ReplaceOneAsync(f => f.Id == b.Id, b);
         }
      }
   }

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
      public LevelOfFlight FlightLevel { get; set; }

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

   public enum LevelOfFlight
   {
      None = 0,
      WIS = 1,
      WIF = 2,
      OSG = 3,
      MSIT = 4,
      Canary = 5
   }

   public enum TypeOfSource
   {
      PublicRelease,
      InternalLeak,
      UpdateGDR,
      UpdateLDR,
      AppPackage,
      BuildTools,
      Documentation,
      Logging,
      PrivateLeak
   }
}
