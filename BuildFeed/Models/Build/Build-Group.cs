using BuildFeed.Models.ViewModel.Front;
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
      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<Tuple<BuildGroup, List<BuildModel>>> SelectBuildGroup(BuildGroup bGroup)
      {
         var pipeline = _buildCollection
            .Find(new BsonDocument
            {
               { "MajorVersion", BsonValue.Create(bGroup.Major) },
               { "MinorVersion", BsonValue.Create(bGroup.Minor) },
               { "Number", BsonValue.Create(bGroup.Build) },
               { "Revision", BsonValue.Create(bGroup.Revision) },
            })
            .SortByDescending(b => b.BuildTime);

         return new Tuple<BuildGroup, List<BuildModel>>(bGroup, await pipeline.ToListAsync());
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<FrontBuildGroup>> SelectBuildGroups(int limit, int skip)
      {
         return await _buildCollection.Aggregate()
            .Group(b => new BuildGroup()
            {
               Major = b.MajorVersion,
               Minor = b.MinorVersion,
               Build = b.Number,
               Revision = b.Revision
            },
            bg => new FrontBuildGroup()
            {
               Key = bg.Key,
               BuildCount = bg.Count(),
               LastBuild = bg.Max(b => b.BuildTime)
            })
            .SortByDescending(b => b.Key.Major)
            .ThenByDescending(b => b.Key.Minor)
            .ThenByDescending(b => b.Key.Build)
            .ThenByDescending(b => b.Key.Revision)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, true)]
      public async Task<int> SelectBuildGroupsCount()
      {
         var pipeline = _buildCollection.Aggregate()
            .Group(b => new BuildGroup()
            {
               Major = b.MajorVersion,
               Minor = b.MinorVersion,
               Build = b.Number,
               Revision = b.Revision
            },
            bg => new BsonDocument());

         return (await pipeline.ToListAsync()).Count;
      }
   }
}