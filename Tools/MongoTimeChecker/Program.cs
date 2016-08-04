using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MBuildModel = RedisMongoMigration.Mongo.BuildModel;
using MBuild = RedisMongoMigration.Mongo.Build;
using RBuild = RedisMongoMigration.Redis.Build;

namespace MongoTimeChecker
{
   class Program
   {
      static void Main(string[] args)
      {
         var builds = RBuild.Select();

         MBuild mBuildObj = new MBuild();
         foreach (var build in builds)
         {
            var mBuild = mBuildObj.SelectByLegacyId(build.Id);
            if(mBuild != null)
            {
               bool isSame = mBuild.BuildTime == build.BuildTime;
               if(!isSame)
               {
                  Console.WriteLine($"{build.FullBuildString}: {build.BuildTime} != {mBuild.BuildTime}");
                  DateTime dt = DateTime.SpecifyKind(build.BuildTime.Value, DateTimeKind.Utc);
                  mBuildObj.UpdateDateOfLegacy(build.Id, dt);
               }
            }
         }

         Console.ReadKey();
      }
   }
}
