using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Web.Mvc;
using BuildFeed.Local;
using HtmlAgilityPack;
using MongoDB.Bson.Serialization.Attributes;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace BuildFeed.Models
{
   [DataObject]
   public class BuildModel
   {
      [Key, BsonId]
      public Guid Id { get; set; }

      public long? LegacyId { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "MajorVersion")]
      public uint MajorVersion { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "MinorVersion")]
      public uint MinorVersion { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "Number")]
      public uint Number { get; set; }

      [Display(ResourceType = typeof(Model), Name = "Revision")]
      [DisplayFormat(ConvertEmptyStringToNull = true)]
      public uint? Revision { get; set; }

      [Display(ResourceType = typeof(Model), Name = "Lab")]
      public string Lab { get; set; }

      [Display(ResourceType = typeof(Model), Name = "BuildTime")]
      [DisplayFormat(ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true, DataFormatString = "{0:yyMMdd-HHmm}")]
      public DateTime? BuildTime { get; set; }


      [@Required]
      [Display(ResourceType = typeof(Model), Name = "Added")]
      public DateTime Added { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "Modified")]
      public DateTime Modified { get; set; }

      [@Required]
      [Display(ResourceType = typeof(Model), Name = "SourceType")]
      [EnumDataType(typeof(TypeOfSource))]
      public TypeOfSource SourceType { get; set; }

      [Display(ResourceType = typeof(Model), Name = "SourceDetails")]
      [AllowHtml]
      public string SourceDetails { get; set; }

      [Display(ResourceType = typeof(Model), Name = "LeakDate")]
      [DisplayFormat(ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
      public DateTime? LeakDate { get; set; }

      [Display(ResourceType = typeof(Model), Name = "FlightLevel")]
      [EnumDataType(typeof(LevelOfFlight))]
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

      public ProjectFamily Family
      {
         get
         {
            if (Number >= 14800)
            {
               return ProjectFamily.Redstone2;
            }
            if (Number >= 11000)
            {
               return ProjectFamily.Redstone;
            }
            if (Number >= 10500)
            {
               return ProjectFamily.Threshold2;
            }
            if (Number >= 9700)
            {
               return ProjectFamily.Threshold;
            }
            if (Number >= 9250)
            {
               return ProjectFamily.Windows81;
            }
            if (Number >= 7650)
            {
               return ProjectFamily.Windows8;
            }
            if (Number >= 6020)
            {
               return ProjectFamily.Windows7;
            }
            if (MajorVersion == 6 && Number >= 5000)
            {
               return ProjectFamily.WindowsVista;
            }
            if (MajorVersion == 6)
            {
               return ProjectFamily.Longhorn;
            }
            if (MajorVersion == 5 && Number >= 3000)
            {
               return ProjectFamily.Server2003;
            }
            if (MajorVersion == 5 && Number >= 2205)
            {
               return ProjectFamily.WindowsXP;
            }
            if (MajorVersion == 5 && MinorVersion == 50)
            {
               return ProjectFamily.Neptune;
            }
            if (MajorVersion == 5)
            {
               return ProjectFamily.Windows2000;
            }
            return ProjectFamily.None;
         }
      }

      public string GenerateLabUrl() => (Lab ?? "").Replace('/', '-')
                                                   .ToLower();

      public string SourceDetailsFiltered
      {
         get
         {
            HtmlDocument hDoc = new HtmlDocument();
            hDoc.LoadHtml($"<div>{SourceDetails}</div>");
            if (string.IsNullOrWhiteSpace(hDoc.DocumentNode.InnerText))
            {
               return "";
            }
            if (Uri.IsWellFormedUriString(hDoc.DocumentNode.InnerText, UriKind.Absolute))
            {
               Uri uri = new Uri(hDoc.DocumentNode.InnerText, UriKind.Absolute);
               return $"<a href=\"{uri}\" target=\"_blank\">External link <i class=\"fa fa-external-link\"></i></a>";
            }
            return SourceDetails;
         }
      }
   }
}