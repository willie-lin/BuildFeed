using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MBuildModel = RedisMongoMigration.Mongo.BuildModel;
using MBuild = RedisMongoMigration.Mongo.Build;
using RBuild = RedisMongoMigration.Redis.Build;

namespace RedisMongoMigration
{
   class Program
   {
      static void Main(string[] args)
      {
         var builds = RBuild.Select();
         var newBuilds = from b in builds
                         select new MBuildModel()
                         {
                            Id = Guid.NewGuid(),
                            LegacyId = b.Id,

                            MajorVersion = b.MajorVersion,
                            MinorVersion = b.MinorVersion,
                            Number = b.Number,
                            Revision = b.Revision,
                            Lab = b.Lab,
                            BuildTime = b.BuildTime,

                            Added = b.Added,
                            Modified = b.Modified,
                            SourceType = b.SourceType,
                            SourceDetails = b.SourceDetails,
                            LeakDate = b.LeakDate,
                            FlightLevel = b.FlightLevel
                         };
         MBuild m = new MBuild();
         m.InsertAll(newBuilds);
         Console.WriteLine("Builds: Complete");
         Console.ReadKey();
      }
   }
}
