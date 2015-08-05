using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using System;
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
    }
}
