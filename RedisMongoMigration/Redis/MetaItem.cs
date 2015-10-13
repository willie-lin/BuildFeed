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
   public class MetaItem : IHasId<MetaItemKey>
   {
      [Key]
      [Index]
      public MetaItemKey Id { get; set; }

      public string PageContent { get; set; }

      public string MetaDescription { get; set; }


      public static IEnumerable<MetaItem> Select()
      {
         using (RedisClient rClient = new RedisClient("localhost", 6379, db: 1))
         {
            var client = rClient.As<MetaItem>();
            return client.GetAll();
         }
      }
   }

   public struct MetaItemKey
   {
      public string Value { get; set; }
      public MetaType Type { get; set; }

      public MetaItemKey(string id)
      {
         var items = id.Split(':');
         Type = (MetaType)Enum.Parse(typeof(MetaType), items[0]);
         Value = items[1];
      }

      public override string ToString()
      {
         return $"{Type}:{Value}";
      }
   }
}
