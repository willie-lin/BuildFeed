using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Models.ViewModel
{
   public class RegistrationUser
   {
      [Required]
      [MinLength(8)]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_ConfirmPassword))]
      [Compare("Password")]
      public string ConfirmPassword { get; set; }

      [Required]
      [EmailAddress]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_EmailAddress))]
      public string EmailAddress { get; set; }

      [Required]
      [MinLength(8)]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_EnterPassword))]
      public string Password { get; set; }

      [Required]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_UserName))]
      public string UserName { get; set; }
   }
}