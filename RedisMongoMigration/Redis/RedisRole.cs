using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using NServiceKit.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RedisMongoMigration.Redis
{
    [DataObject]
    public class RedisRole : IHasId<Guid>
    {
        [Key]
        [Index]
        public Guid Id { get; set; }

        [Key]
        public string RoleName { get; set; }

        public Guid[] Users { get; set; }

      public static IEnumerable<RedisRole> Select()
      {
         using (RedisClient rClient = new RedisClient("localhost", 6379, db: 1))
         {
            var client = rClient.As<RedisRole>();
            return client.GetAll();
         }
      }
   }
}
