using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using BuildFeed.Local;
using HtmlAgilityPack;
using MongoDB.Bson.Serialization.Attributes;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace BuildFeed.Model
{
   [DataObject, BsonIgnoreExtraElements]
   public class Build
   {
      [Key, BsonId]
      public Guid Id { get; set; }

      public long? LegacyId { get; set; }

      [@Required]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_MajorVersion))]
      public uint MajorVersion { get; set; }

      [@Required]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_MinorVersion))]
      public uint MinorVersion { get; set; }

      [@Required]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_BuildNumber))]
      public uint Number { get; set; }

      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Revision))]
      [DisplayFormat(ConvertEmptyStringToNull = true)]
      public uint? Revision { get; set; }

      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_LabString))]
      public string Lab { get; set; }

      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_BuildTime))]
      [DisplayFormat(ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true, DataFormatString = "{0:yyMMdd-HHmm}")]
      public DateTime? BuildTime { get; set; }


      [@Required]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Added))]
      public DateTime Added { get; set; }

      [@Required]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Modified))]
      public DateTime Modified { get; set; }

      [@Required]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_SourceType))]
      [EnumDataType(typeof(TypeOfSource))]
      public TypeOfSource SourceType { get; set; }

      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_SourceDetails))]
      [AllowHtml]
      public string SourceDetails { get; set; }

      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_LeakDate))]
      [DisplayFormat(ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true)]
      public DateTime? LeakDate { get; set; }

      public string LabUrl { get; set; }

      public bool IsLeaked => SourceType == TypeOfSource.PublicRelease || SourceType == TypeOfSource.InternalLeak || SourceType == TypeOfSource.UpdateGDR || SourceType == TypeOfSource.UpdateLDR;

      public string FullBuildString
      {
         get
         {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{MajorVersion}.{MinorVersion}.{Number}");

            if (Revision.HasValue)
            {
               sb.Append($".{Revision}");
            }

            if (!string.IsNullOrWhiteSpace(Lab))
            {
               sb.Append($".{Lab}");
            }

            if (BuildTime.HasValue)
            {
               sb.Append($".{BuildTime.Value.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture.DateTimeFormat)}");
            }

            return sb.ToString();
         }
      }

      public string AlternateBuildString
      {
         get
         {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{MajorVersion}.{MinorVersion}.{Number}");

            if (Revision.HasValue)
            {
               sb.Append($".{Revision}");
            }

            if (!string.IsNullOrWhiteSpace(Lab))
            {
               sb.Append($" ({Lab}");

               if (BuildTime.HasValue)
               {
                  sb.Append($".{BuildTime.Value.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture.DateTimeFormat)}");
               }

               sb.Append(")");
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
            if (MajorVersion == 6
               && Number >= 5000)
            {
               return ProjectFamily.WindowsVista;
            }
            if (MajorVersion == 6)
            {
               return ProjectFamily.Longhorn;
            }
            if (MajorVersion == 5
               && Number >= 3000)
            {
               return ProjectFamily.Server2003;
            }
            if (MajorVersion == 5
               && Number >= 2205)
            {
               return ProjectFamily.WindowsXP;
            }
            if (MajorVersion == 5
               && MinorVersion == 50)
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
               return $"<a href=\"{uri}\" target=\"_blank\">{VariantTerms.Model_ExternalLink} <i class=\"fa fa-external-link\"></i></a>";
            }

            return SourceDetails;
         }
      }

      public string GenerateLabUrl() => !string.IsNullOrEmpty(Lab)
         ? Lab.Replace('/', '-').ToLower()
         : "";
   }
}