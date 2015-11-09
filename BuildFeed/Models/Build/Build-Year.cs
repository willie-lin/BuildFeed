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
      public async Task<List<BuildModel>> SelectYear(int year, int skip, int limit)
      {
         return await _buildCollection.Find(b => b.BuildTime != null &&
         (b.BuildTime > new DateTime(year, 1, 1, 0, 0, 0)) &&
         (b.BuildTime < new DateTime(year, 12, 31, 23, 59, 59)))
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
      public async Task<List<int>> SelectYears()
      {
         var result = await _buildCollection.Aggregate()
            .Match(b => b.BuildTime != null)
            .Group(b => ((DateTime)b.BuildTime).Year,
            // incoming bullshit hack
            bg => new Tuple<int>(bg.Key))
            .SortByDescending(b => b.Item1)
            .ToListAsync();

         // work ourselves out of aforementioned bullshit hack
         return result.Select(b => b.Item1).ToList();
      }

      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<long> SelectYearCount(int year)
      {
         return await _buildCollection.Find(b => b.BuildTime != null &&
         (b.BuildTime > new DateTime(year, 1, 1, 0, 0, 0)) &&
         (b.BuildTime < new DateTime(year, 12, 31, 23, 59, 59)))
            .CountAsync();
      }
   }
}