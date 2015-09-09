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

   public enum LevelOfFlight
   {
      None = 0,
      Low = 1,
      Medium = 2,
      High = 3
   }
}
