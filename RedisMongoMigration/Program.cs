using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MBuildModel = RedisMongoMigration.Mongo.BuildModel;
using MBuild = RedisMongoMigration.Mongo.Build;
using RBuild = RedisMongoMigration.Redis.Build;
using MongoLevelOfFlight = RedisMongoMigration.Mongo.MongoLevelOfFlight;
using RedisLevelOfFlight = RedisMongoMigration.Redis.RedisLevelOfFlight;
using MMember = RedisMongoMigration.Mongo.MongoMember;
using RMember = RedisMongoMigration.Redis.RedisMember;
using RRole = RedisMongoMigration.Redis.RedisRole;
using MRole = RedisMongoMigration.Mongo.MongoRole;

namespace RedisMongoMigration
{
   class Program
   {
      static void Main(string[] args)
      {
         var builds = from b in RBuild.Select()
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
                         FlightLevel = ExchangeFlights(b.FlightLevel)
                      };
         MBuild m = new MBuild();
         m.InsertAll(builds);
         Console.WriteLine("Builds: Complete");

         var members = from r in RMember.Select()
                       select new MMember()
                       {
                          CreationDate = r.CreationDate,
                          EmailAddress = r.EmailAddress,
                          Id = r.Id,
                          IsApproved = r.IsApproved,
                          IsLockedOut = r.IsLockedOut,
                          LastActivityDate = r.LastActivityDate,
                          LastLockoutDate = r.LastLockoutDate,
                          LastLoginDate = r.LastLoginDate,
                          LockoutWindowAttempts = r.LockoutWindowAttempts,
                          LockoutWindowStart = r.LockoutWindowStart,
                          PassHash = r.PassHash,
                          PassSalt = r.PassSalt,
                          UserName = r.UserName
                       };
         Console.WriteLine("Members: Complete");

         var roles = from r in RRole.Select()
                     select new MRole()
                     {
                        Id = r.Id,
                        RoleName = r.RoleName,
                        Users = r.Users
                     };
         Console.WriteLine("Roles: Complete");

         Console.ReadKey();
      }

      static MongoLevelOfFlight ExchangeFlights(RedisLevelOfFlight flight)
      {
         switch (flight)
         {
            case RedisLevelOfFlight.Low:
               return MongoLevelOfFlight.WIS;
            case RedisLevelOfFlight.High:
               return MongoLevelOfFlight.OSG;
            default:
               return MongoLevelOfFlight.None;
         }
      }
   }
}
