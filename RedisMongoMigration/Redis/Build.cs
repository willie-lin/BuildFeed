using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using NServiceKit.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RedisMongoMigration.Redis
{
    [DataObject]
    public class Build : IHasId<long>
    {
        [Key]
        [AutoIncrement]
        [Index]
        public long Id { get; set; }
        public byte MajorVersion { get; set; }
        public byte MinorVersion { get; set; }
        public ushort Number { get; set; }
        public ushort? Revision { get; set; }
        public string Lab { get; set; }
        public DateTime? BuildTime { get; set; }

        public DateTime Added { get; set; }
        public DateTime Modified { get; set; }
        public TypeOfSource SourceType { get; set; }
        public string SourceDetails { get; set; }
        public DateTime? LeakDate { get; set; }
        public RedisLevelOfFlight FlightLevel { get; set; }

        public bool IsLeaked
        {
            get
            {
                switch (SourceType)
                {
                    case TypeOfSource.PublicRelease:
                    case TypeOfSource.InternalLeak:
                    case TypeOfSource.UpdateGDR:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public string FullBuildString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}.{1}.{2}", MajorVersion, MinorVersion, Number);

                if (Revision.HasValue)
                {
                    sb.AppendFormat(".{0}", Revision);
                }

                if (!string.IsNullOrWhiteSpace(Lab))
                {
                    sb.AppendFormat(".{0}", Lab);
                }

                if (BuildTime.HasValue)
                {
                    sb.AppendFormat(".{0:yyMMdd-HHmm}", BuildTime);
                }

                return sb.ToString();
            }
      }

      public static IEnumerable<Build> Select()
      {
         using (RedisClient rClient = new RedisClient("localhost", 6379, db: 1))
         {
            var client = rClient.As<Build>();
            return client.GetAll();
         }
      }
   }

   public enum RedisLevelOfFlight
   {
      None = 0,
      Low = 1,
      Medium = 2,
      High = 3
   }
}
