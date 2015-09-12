using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisMongoMigration.Mongo
{
   public class MongoRole
   {
      [BsonId]
      public Guid Id { get; set; }

      public string RoleName { get; set; }

      public Guid[] Users { get; set; }
   }
}
