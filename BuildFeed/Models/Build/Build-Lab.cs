using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BuildFeed.Models
{
   public partial class Build
   {
      public Task<string[]> SelectAllLabs(int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<int> SelectAllLabsCount() { throw new NotImplementedException(); }

      public async Task<List<BuildModel>> SelectLab(string lab, int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<int> SelectLabCount(string lab) { throw new NotImplementedException(); }
   }
}