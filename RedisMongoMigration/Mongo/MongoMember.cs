using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisMongoMigration.Mongo
{
   public class MongoMember
   {
      [BsonId]
      public Guid Id { get; set; }

      public string UserName { get; set; }
      public byte[] PassHash { get; set; }
      public byte[] PassSalt { get; set; }
      public string EmailAddress { get; set; }

      public bool IsApproved { get; set; }
      public bool IsLockedOut { get; set; }

      public DateTime CreationDate { get; set; }
      public DateTime LastActivityDate { get; set; }
      public DateTime LastLockoutDate { get; set; }
      public DateTime LastLoginDate { get; set; }

      public DateTime LockoutWindowStart { get; set; }
      public int LockoutWindowAttempts { get; set; }
   }
}
