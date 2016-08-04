using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Models.ViewModel
{
    public class LoginUser
    {
        [Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_UserName))]
        public string UserName { get; set; }

        [Required]
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_Password))]
        public string Password { get; set; }

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Support_RememberMe))]
        public bool RememberMe { get; set; }
    }
}