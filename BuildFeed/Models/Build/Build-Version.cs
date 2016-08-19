using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Models
{
   public partial class Build
   {
      public async Task<BuildVersion[]> SelectAllVersions(int limit = -1, int skip = 0)
      {
         IAggregateFluent<BsonDocument> query = _buildCollection.Aggregate().Group(new BsonDocument("_id",
            new BsonDocument
            {
               new BsonElement(nameof(BuildVersion.Major), $"${nameof(BuildModel.MajorVersion)}"),
               new BsonElement(nameof(BuildVersion.Minor), $"${nameof(BuildModel.MinorVersion)}")
            })).Sort(new BsonDocument
            {
               new BsonElement($"_id.{nameof(BuildVersion.Major)}", -1),
               new BsonElement($"_id.{nameof(BuildVersion.Minor)}", -1)
            }).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         List<BsonDocument> grouping = await query.ToListAsync();

         return (from g in grouping
                 select new BuildVersion
                 {
                    Major = (uint)g["_id"].AsBsonDocument[nameof(BuildVersion.Major)].AsInt32,
                    Minor = (uint)g["_id"].AsBsonDocument[nameof(BuildVersion.Minor)].AsInt32
                 }).ToArray();
      }

      public async Task<long> SelectAllVersionsCount()
      {
         List<BsonDocument> query = await _buildCollection.Aggregate().Group(new BsonDocument("_id",
            new BsonDocument
            {
               new BsonElement(nameof(BuildVersion.Major), $"${nameof(BuildModel.MajorVersion)}"),
               new BsonElement(nameof(BuildVersion.Minor), $"${nameof(BuildModel.MinorVersion)}")
            })).ToListAsync();
         return query.Count;
      }

      public async Task<List<BuildModel>> SelectVersion(uint major, uint minor, int limit = -1, int skip = 0)
      {
         IFindFluent<BuildModel, BuildModel> query = _buildCollection.Find(new BsonDocument
         {
            new BsonElement(nameof(BuildModel.MajorVersion), major),
            new BsonElement(nameof(BuildModel.MinorVersion), minor)
         }).Sort(sortByOrder).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
      }

      public async Task<long> SelectVersionCount(uint major, uint minor) => await _buildCollection.CountAsync(new BsonDocument
      {
         new BsonElement(nameof(BuildModel.MajorVersion), major),
         new BsonElement(nameof(BuildModel.MinorVersion), minor)
      });
   }
}