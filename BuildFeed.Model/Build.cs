using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using BuildFeed.Local;
using HtmlAgilityPack;
using MongoDB.Bson.Serialization.Attributes;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace BuildFeed.Model
{
    [DataObject]
    [BsonIgnoreExtraElements]
    public class Build : BuildDetails
    {
        [Key]
        [BsonId]
        public Guid Id { get; set; }

        public long? LegacyId { get; set; }

        [@Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Added))]
        public DateTime Added { get; set; }

        [@Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Modified))]
        public DateTime Modified { get; set; }

        public string LabUrl { get; private set; }

        public bool IsLeaked => SourceType == TypeOfSource.PublicRelease ||
                                SourceType == TypeOfSource.InternalLeak ||
                                SourceType == TypeOfSource.UpdateGDR;

        public string FullBuildString { get; private set; }

        public string AlternateBuildString { get; private set; }

        public List<ItemHistory<BuildDetails>> History { get; set; }

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Search_Version))]
        public ProjectFamily Family { get; private set; }

        public string SourceDetailsFiltered
        {
            get
            {
                var hDoc = new HtmlDocument();
                hDoc.LoadHtml($"<div>{SourceDetails}</div>");

                if (string.IsNullOrWhiteSpace(hDoc.DocumentNode.InnerText))
                {
                    return "";
                }

                if (Uri.IsWellFormedUriString(hDoc.DocumentNode.InnerText, UriKind.Absolute))
                {
                    var uri = new Uri(hDoc.DocumentNode.InnerText, UriKind.Absolute);
                    return
                        $"<a href=\"{uri}\" target=\"_blank\">{VariantTerms.Model_ExternalLink} <i class=\"fa fa-external-link\"></i></a>";
                }

                return SourceDetails;
            }
        }

        public void RegenerateCachedProperties()
        {
            GenerateFullBuildString();
            GenerateAlternateBuildString();
            GenerateLabUrl();
            GenerateFamily();
        }

        private void GenerateFullBuildString()
        {
            var sb = new StringBuilder();
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

            FullBuildString = sb.ToString();
        }

        private void GenerateAlternateBuildString()
        {
            var sb = new StringBuilder();
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
                    sb.Append(
                        $".{BuildTime.Value.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture.DateTimeFormat)}");
                }

                sb.Append(")");
            }

            AlternateBuildString = sb.ToString();
        }

        private void GenerateLabUrl()
        {
            string url = !string.IsNullOrEmpty(Lab)
                ? Lab.Replace('/', '-').ToLower()
                : "";

            LabUrl = url;
        }

        private void GenerateFamily()
        {
            // start with lab-based overrides
            if (Lab?.StartsWith("rs4", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                Family = ProjectFamily.Redstone4;
            }
            else if (Lab?.StartsWith("rs3", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                Family = ProjectFamily.Redstone3;
            }
            else if (Lab?.StartsWith("feature2", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                Family = ProjectFamily.Feature2;
            }
            else if (Lab?.StartsWith("rs2", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                Family = ProjectFamily.Redstone2;
            }
            else if (Lab?.StartsWith("rs1", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                Family = ProjectFamily.Redstone;
            }
            else if (Lab?.StartsWith("th2", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                Family = ProjectFamily.Threshold2;
            }

            // move on to version number guesses
            else if (Number >= 17600)
            {
                Family = ProjectFamily.Redstone5;
            }
            else if (Number >= 16350)
            {
                Family = ProjectFamily.Redstone4;
            }
            else if (Number >= 15140)
            {
                Family = ProjectFamily.Redstone3;
            }
            else if (Number >= 14800)
            {
                Family = ProjectFamily.Redstone2;
            }
            else if (Number >= 11000)
            {
                Family = ProjectFamily.Redstone;
            }
            else if (Number >= 10500)
            {
                Family = ProjectFamily.Threshold2;
            }
            else if (Number >= 9650)
            {
                Family = ProjectFamily.Threshold;
            }
            else if (Number >= 9250)
            {
                Family = ProjectFamily.WindowsBlue;
            }
            else if (Number >= 7650)
            {
                Family = ProjectFamily.Windows8;
            }
            else if (Number >= 6400)
            {
                Family = ProjectFamily.Windows7;
            }
            else if (MajorVersion == 6 && Number >= 5000)
            {
                Family = ProjectFamily.WindowsVista;
            }
            else if (MajorVersion == 6)
            {
                Family = ProjectFamily.Longhorn;
            }
            else if (MajorVersion == 5 && MinorVersion == 50)
            {
                Family = ProjectFamily.Neptune;
            }
            else if (MajorVersion == 5 && Number >= 3000)
            {
                Family = ProjectFamily.Server2003;
            }
            else if (MajorVersion == 5 && Number >= 2205)
            {
                Family = ProjectFamily.WindowsXP;
            }
            else if (MajorVersion == 5)
            {
                Family = ProjectFamily.Windows2000;
            }

            // ¯\_(ツ)_/¯
            else
            {
                Family = ProjectFamily.None;
            }
        }
    }
}