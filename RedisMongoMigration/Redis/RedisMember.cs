using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using NServiceKit.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RedisMongoMigration.Redis
{

   [DataObject]
   public class RedisMember : IHasId<Guid>
   {
      [Key]
      [Index]
      public Guid Id { get; set; }

      [Key]
      public string UserName { get; set; }
      public byte[] PassHash { get; set; }
      public byte[] PassSalt { get; set; }

      [Key]
      public string EmailAddress { get; set; }

      public bool IsApproved { get; set; }
      public bool IsLockedOut { get; set; }

      public DateTime CreationDate { get; set; }
      public DateTime LastActivityDate { get; set; }
      public DateTime LastLockoutDate { get; set; }
      public DateTime LastLoginDate { get; set; }

      public DateTime LockoutWindowStart { get; set; }
      public int LockoutWindowAttempts { get; set; }

      public static IEnumerable<RedisMember> Select()
      {
         using (RedisClient rClient = new RedisClient("localhost", 6379, db: 1))
         {
            var client = rClient.As<RedisMember>();
            return client.GetAll();
         }
      }
   }
}
