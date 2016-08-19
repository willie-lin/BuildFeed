using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildFeed.Model.View;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Model
{
   public partial class Build
   {
      public async Task<FrontBuildGroup[]> SelectAllGroups(int limit = -1, int skip = 0)
      {
         IAggregateFluent<BsonDocument> query = _buildCollection.Aggregate().Group(new BsonDocument
         {
            new BsonElement("_id",
               new BsonDocument
               {
                  new BsonElement(nameof(BuildGroup.Major), $"${nameof(BuildModel.MajorVersion)}"),
                  new BsonElement(nameof(BuildGroup.Minor), $"${nameof(BuildModel.MinorVersion)}"),
                  new BsonElement(nameof(BuildGroup.Build), $"${nameof(BuildModel.Number)}"),
                  new BsonElement(nameof(BuildGroup.Revision), $"${nameof(BuildModel.Revision)}")
               }),
            new BsonElement("date", new BsonDocument("$max", $"${nameof(BuildModel.BuildTime)}")),
            new BsonElement("count", new BsonDocument("$sum", 1))
         }).Sort(new BsonDocument
         {
            new BsonElement($"_id.{nameof(BuildGroup.Major)}", -1),
            new BsonElement($"_id.{nameof(BuildGroup.Minor)}", -1),
            new BsonElement($"_id.{nameof(BuildGroup.Build)}", -1),
            new BsonElement($"_id.{nameof(BuildGroup.Revision)}", -1)
         }).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         List<BsonDocument> grouping = await query.ToListAsync();

         return (from g in grouping
                 select new FrontBuildGroup
                 {
                    Key = new BuildGroup
                    {
                       Major = (uint)g["_id"].AsBsonDocument[nameof(BuildGroup.Major)].AsInt32,
                       Minor = (uint)g["_id"].AsBsonDocument[nameof(BuildGroup.Minor)].AsInt32,
                       Build = (uint)g["_id"].AsBsonDocument[nameof(BuildGroup.Build)].AsInt32,
                       Revision = (uint?)g["_id"].AsBsonDocument[nameof(BuildGroup.Revision)].AsNullableInt32
                    },
                    LastBuild = g["date"].ToNullableUniversalTime(),
                    BuildCount = g["count"].AsInt32
                 }).ToArray();
      }

      public async Task<long> SelectAllGroupsCount()
      {
         List<BsonDocument> grouping = await _buildCollection.Aggregate().Group(new BsonDocument
         {
            new BsonElement("_id",
               new BsonDocument
               {
                  new BsonElement(nameof(BuildGroup.Major), $"${nameof(BuildModel.MajorVersion)}"),
                  new BsonElement(nameof(BuildGroup.Minor), $"${nameof(BuildModel.MinorVersion)}"),
                  new BsonElement(nameof(BuildGroup.Build), $"${nameof(BuildModel.Number)}"),
                  new BsonElement(nameof(BuildGroup.Revision), $"${nameof(BuildModel.Revision)}")
               })
         }).ToListAsync();
         return grouping.Count;
      }

      public async Task<List<BuildModel>> SelectGroup(BuildGroup group, int limit = -1, int skip = 0)
      {
         IFindFluent<BuildModel, BuildModel> query = _buildCollection.Find(new BsonDocument
         {
            new BsonElement(nameof(BuildModel.MajorVersion), group.Major),
            new BsonElement(nameof(BuildModel.MinorVersion), group.Minor),
            new BsonElement(nameof(BuildModel.Number), group.Build),
            new BsonElement(nameof(BuildModel.Revision), group.Revision)
         }).Sort(new BsonDocument
         {
            new BsonElement(nameof(BuildModel.BuildTime), 1)
         }).Skip(skip);

         if (limit > 0)
         {
            query = query.Limit(limit);
         }

         return await query.ToListAsync();
      }

      public async Task<long> SelectGroupCount(BuildGroup group) => await _buildCollection.CountAsync(new BsonDocument
      {
         new BsonElement(nameof(BuildModel.MajorVersion), @group.Major),
         new BsonElement(nameof(BuildModel.MinorVersion), @group.Minor),
         new BsonElement(nameof(BuildModel.Number), @group.Build),
         new BsonElement(nameof(BuildModel.Revision), @group.Revision)
      });
   }
}