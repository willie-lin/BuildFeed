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
      public async Task<List<BuildModel>> SelectVersion(uint major, uint minor, int skip, int limit)
      {
         byte bMajor = Convert.ToByte(major), bMinor = Convert.ToByte(minor);
         var test = await _buildCollection.Find(Builders<BuildModel>.Filter.And(Builders<BuildModel>.Filter.Eq(b => b.MajorVersion, bMajor), Builders<BuildModel>.Filter.Eq(b => b.MinorVersion, bMinor)))
             .SortByDescending(b => b.BuildTime)
             .ThenByDescending(b => b.MajorVersion)
             .ThenByDescending(b => b.MinorVersion)
             .ThenByDescending(b => b.Number)
             .ThenByDescending(b => b.Revision)
             .Skip(skip)
             .Limit(limit)
             .ToListAsync();
         return test;
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildVersion>> SelectVersions()
      {
         var result = await _buildCollection.Aggregate()
            // the group method in mongodb's c# driver sucks balls and throws a hissy fit over far too much.
            .Group(b => new BuildVersion(b.MajorVersion, b.MinorVersion), bg => new BsonDocument())
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         var typed = from r in result
                     select new BuildVersion
                     {
                        Major = (uint)r["_id"]["Major"].ToInt32(),
                        Minor = (uint)r["_id"]["Minor"].ToInt32()
                     };

         return (from t in typed
                 orderby t.Major descending,
                         t.Minor descending
                 select t).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<long> SelectVersionCount(uint major, uint minor)
      {
         return await _buildCollection.Find(Builders<BuildModel>.Filter.And(Builders<BuildModel>.Filter.Eq(b => b.MajorVersion, major), Builders<BuildModel>.Filter.Eq(b => b.MinorVersion, minor)))
            .CountAsync();
      }
   }
}