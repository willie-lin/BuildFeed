using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisMongoMigration
{
   public class MetaItemKey
   {
      public string Value { get; set; }
      public MetaType Type { get; set; }

      public MetaItemKey()
      {

      }

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

   public enum MetaType
   {
      Lab,
      Version,
      Source,
      Year
   }
}
