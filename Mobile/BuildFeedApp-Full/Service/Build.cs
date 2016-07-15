using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildFeedApp.Service
{
   public class Build
   {
      public Guid Id { get; set; }
      public long? LegacyId { get; set; }
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
      public LevelOfFlight FlightLevel { get; set; }

      public string LabUrl { get; set; }

      public bool IsLeaked
      {
         get
         {
            switch (SourceType)
            {
               case TypeOfSource.PublicRelease:
               case TypeOfSource.InternalLeak:
               case TypeOfSource.UpdateGDR:
               case TypeOfSource.UpdateLDR:
                  return true;
               default:
                  return false;
            }
         }
      }

      public string DateString => BuildTime.HasValue ? BuildTime.Value.ToString("yyMMdd-HHmm") : "";

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
   }

   public class FrontBuildGroup
   {
      public BuildGroup Key { get; set; }
      public DateTime? LastBuild { get; set; }
      public int BuildCount { get; set; }
   }

   public class BuildGroup
   {
      public byte Major { get; set; }
      public byte Minor { get; set; }
      public ushort Build { get; set; }
      public ushort? Revision { get; set; }

      public override string ToString() => Revision.HasValue ?
           $"{Major}.{Minor}.{Build}.{Revision.Value}" :
           $"{Major}.{Minor}.{Build}";
   }

   public enum TypeOfSource
   {
      PublicRelease,
      InternalLeak,
      UpdateGDR,
      UpdateLDR,
      AppPackage,
      BuildTools,
      Documentation,
      Logging,
      PrivateLeak
   }

   public enum LevelOfFlight
   {
      None = 0,
      WIS = 1,
      WIF = 2,
      OSG = 3,
      MSIT = 4,
      Canary = 5
   }
}
