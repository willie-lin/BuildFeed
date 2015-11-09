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
      public async Task<List<BuildModel>> SelectInVersionOrder()
      {
         return await _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .ThenByDescending(b => b.BuildTime)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectInVersionOrder(int limit, int skip)
      {
         return await _buildCollection.Find(new BsonDocument())
             .SortByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .ThenByDescending(b => b.BuildTime)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectVersion(int major, int minor, int skip, int limit)
      {
         return await _buildCollection.Find(b => b.MajorVersion == major && b.MinorVersion == minor)
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
      public async Task<List<BuildVersion>> SelectVersions()
      {
         var result = await _buildCollection.Aggregate()
            .Group(b => new BuildVersion()
            {
               Major = b.MajorVersion,
               Minor = b.MinorVersion,
            },
            // incoming bullshit hack
            bg => new Tuple<BuildVersion>(bg.Key))
            .SortByDescending(b => b.Item1.Major)
            .ThenByDescending(b => b.Item1.Minor)
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<long> SelectVersionCount(int major, int minor)
      {
         return await _buildCollection.Find(b => b.MajorVersion == major && b.MinorVersion == minor)
            .CountAsync();
      }
   }
}