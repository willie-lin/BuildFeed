using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BuildFeed.Models
{
   public partial class Build
   {
      public Task<TypeOfSource[]> SelectAllSources(int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<int> SelectAllSourcesCount() { throw new NotImplementedException(); }

      public async Task<List<BuildModel>> SelectSource(TypeOfSource source, int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<int> SelectSourceCount(TypeOfSource source) { throw new NotImplementedException(); }
   }
}