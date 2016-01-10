using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BuildFeed.Models
{
   public partial class Build
   {
      public async Task<int[]> SelectAllYears(int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<int> SelectAllYearsCount() { throw new NotImplementedException(); }

      public async Task<List<BuildModel>> SelectYear(int year, int limit = -1, int skip = 0) { throw new NotImplementedException(); }

      public async Task<int> SelectYearCount(int year) { throw new NotImplementedException(); }
   }
}