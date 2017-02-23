using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Model
{
    public enum TypeOfSource
    {
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_PublicRelease))]
        PublicRelease = 0,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_InternalLeak))]
        InternalLeak = 1,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_UpdateGDR))]
        UpdateGDR = 2,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_UpdateLDR))]
        UpdateLDR = 3,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_AppPackage))]
        AppPackage = 4,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_BuildTools))]
        BuildTools = 5,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_Documentation))]
        Documentation = 6,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_Logging))]
        Logging = 7,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Model_Source_PrivateLeak))]
        PrivateLeak = 8
    }
}