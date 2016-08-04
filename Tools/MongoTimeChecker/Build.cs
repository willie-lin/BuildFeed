using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisMongoMigration
{
   public enum TypeOfSource
   {
      PublicRelease,
      InternalLeak,
      UpdateGDR,
      UpdateLDR,
      AppPackage,
      BuildTools,
      Documentation,
      Logging,
      PrivateLeak
   }
}
