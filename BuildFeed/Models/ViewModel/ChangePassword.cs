using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Models.ViewModel
{
   public class ChangePassword
   {
      [Required]
      [MinLength(8)]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_ConfirmNewPassword))]
      [Compare("NewPassword")]
      public string ConfirmNewPassword { get; set; }

      [Required]
      [MinLength(8)]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_EnterNewPassword))]
      public string NewPassword { get; set; }

      [Required]
      [MinLength(8)]
      [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_EnterCurrentPassword))]
      public string OldPassword { get; set; }
   }
}