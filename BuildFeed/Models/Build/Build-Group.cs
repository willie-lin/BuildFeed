using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildFeed.Models.ViewModel.Front;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BuildFeed.Models
{
   public partial class Build
   {
      public async Task<FrontBuildGroup[]> SelectAllGroups(int limit = -1, int skip = 0)
      {
         var query = _buildCollection.Aggregate()
                                     .Group(
                                            new BsonDocument
                                            {
                                               new BsonElement(
                                                  "_id", new BsonDocument
                                                         {
                                                            new BsonElement(nameof(BuildGroup.Major), $"${nameof(BuildModel.MajorVersion)}"),
                                                            new BsonElement(nameof(BuildGroup.Minor), $"${nameof(BuildModel.MinorVersion)}"),
                                                            new BsonElement(nameof(BuildGroup.Build), $"${nameof(BuildModel.Number)}"),
                                                            new BsonElement(nameof(BuildGroup.Revision), $"${nameof(BuildModel.Revision)}")
                                                         }),
                                               new BsonElement("date", new BsonDocument("$max", $"${nameof(BuildModel.BuildTime)}")),
                                               new BsonElement("count", new BsonDocument("$sum", 1))
                                            })
                                     .Sort(
                                           new BsonDocument
                                           {
                                              new BsonElement($"_id.{nameof(BuildGroup.Major)}", -1),
                                              new BsonElement($"_id.{nameof(BuildGroup.Minor)}", -1),
                                              new BsonElement($"_id.{nameof(BuildGroup.Build)}", -1),
                                              new BsonElement($"_id.{nameof(BuildGroup.Revision)}", -1)
                                           });

         if (limit > 0)
         {
            query = query
               .Limit(limit);
         }

         var grouping = await query
                                 .Skip(skip)
                                 .ToListAsync();

         return (from g in grouping
                 select new FrontBuildGroup
                        {
                           Key = new BuildGroup
                                 {
                                    Major = (uint) g["_id"].AsBsonDocument[nameof(BuildGroup.Major)].AsInt32,
                                    Minor = (uint) g["_id"].AsBsonDocument[nameof(BuildGroup.Minor)].AsInt32,
                                    Build = (uint) g["_id"].AsBsonDocument[nameof(BuildGroup.Build)].AsInt32,
                                    Revision = (uint) g["_id"].AsBsonDocument[nameof(BuildGroup.Revision)].AsInt32
                                 },
                           LastBuild = g["date"].ToNullableUniversalTime(),
                           BuildCount = g["count"].AsInt32
                        }).ToArray();
      }

      public async Task<int> SelectAllGroupsCount()
      {
         var grouping = await _buildCollection.Aggregate()
                                              .Group(
                                                     new BsonDocument
                                                     {
                                                        new BsonElement(
                                                           "_id", new BsonDocument
                                                                  {
                                                                     new BsonElement(nameof(BuildGroup.Major), $"${nameof(BuildModel.MajorVersion)}"),
                                                                     new BsonElement(nameof(BuildGroup.Minor), $"${nameof(BuildModel.MinorVersion)}"),
                                                                     new BsonElement(nameof(BuildGroup.Build), $"${nameof(BuildModel.Number)}"),
                                                                     new BsonElement(nameof(BuildGroup.Revision), $"${nameof(BuildModel.Revision)}")
                                                                  })
                                                     })
                                              .ToListAsync();
         return grouping.Count;
      }

      public async Task<List<BuildModel>> SelectGroup(BuildGroup group, int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<List<BuildModel>> SelectGroupCount(BuildGroup group) { throw new NotImplementedException(); }
   }
}