using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BuildFeed.Models
{
   public partial class Build
   {
      public async Task<BuildVersion[]> SelectAllVersions(int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<int> SelectAllVersionsCount() { throw new NotImplementedException(); }

      public async Task<List<BuildModel>> SelectVersion(uint major, uint minor, int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<int> SelectVersionCount(uint major, uint minor) { throw new NotImplementedException(); }
   }
}