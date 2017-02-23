using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Model.View
{
    public class RegistrationUser
    {
        [Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_UserName))]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_EmailAddress))]
        public string EmailAddress { get; set; }

        [Required]
        [MinLength(8)]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_EnterPassword))]
        public string Password { get; set; }

        [Required]
        [MinLength(8)]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_ConfirmPassword))]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}