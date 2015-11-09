using BuildFeed.Local;
using System.ComponentModel.DataAnnotations;

namespace BuildFeed.Models
{
   public enum TypeOfSource
   {
      [Display(ResourceType = typeof(Model), Name = "PublicRelease")]
      PublicRelease = 0,

      [Display(ResourceType = typeof(Model), Name = "InternalLeak")]
      InternalLeak = 1,

      [Display(ResourceType = typeof(Model), Name = "UpdateGDR")]
      UpdateGDR = 2,

      [Display(ResourceType = typeof(Model), Name = "UpdateLDR")]
      UpdateLDR = 3,

      [Display(ResourceType = typeof(Model), Name = "AppPackage")]
      AppPackage = 4,

      [Display(ResourceType = typeof(Model), Name = "BuildTools")]
      BuildTools = 5,

      [Display(ResourceType = typeof(Model), Name = "Documentation")]
      Documentation = 6,

      [Display(ResourceType = typeof(Model), Name = "Logging")]
      Logging = 7,

      [Display(ResourceType = typeof(Model), Name = "PrivateLeak")]
      PrivateLeak = 8
   }
}