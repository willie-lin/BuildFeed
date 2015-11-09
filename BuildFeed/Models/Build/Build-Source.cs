using MongoDB.Driver;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BuildFeed.Models
{
   public partial class Build
   {
      [DataObjectMethod(DataObjectMethodType.Select, false)]
      public async Task<List<BuildModel>> SelectSource(TypeOfSource source, int skip, int limit)
      {
         return await _buildCollection.Find(b => b.SourceType == source)
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
      public async Task<long> SelectSourceCount(TypeOfSource source)
      {
         return await _buildCollection.Find(b => b.SourceType == source)
            .CountAsync();
      }
   }
}