using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BuildFeed.Local;
using MongoDB.Bson.Serialization.Attributes;

namespace BuildFeed.Model
{
    [BsonIgnoreExtraElements]
    public class BuildDetails
    {
        [Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_MajorVersion))]
        public uint MajorVersion { get; set; }

        [Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_MinorVersion))]
        public uint MinorVersion { get; set; }

        [Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_BuildNumber))]
        public uint Number { get; set; }

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Revision))]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public uint? Revision { get; set; }

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_LabString))]
        public string Lab { get; set; }

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_BuildTime))]
        [DisplayFormat(ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true,
            DataFormatString = "{0:yyMMdd-HHmm}")]
        public DateTime? BuildTime { get; set; }

        [Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_SourceType))]
        [EnumDataType(typeof(TypeOfSource))]
        public TypeOfSource SourceType { get; set; }

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_SourceDetails))]
        [AllowHtml]
        public string SourceDetails { get; set; }

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_LeakDate))]
        [DisplayFormat(ConvertEmptyStringToNull = true, ApplyFormatInEditMode = true)]
        public DateTime? LeakDate { get; set; }
    }
}