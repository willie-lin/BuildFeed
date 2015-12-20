using System.ComponentModel.DataAnnotations;

namespace BuildFeed.Models
{
   public enum ProjectFamily
   {
      None,

      [Display(Name = "Whistler")]
      Whistler,

      [Display(Name = "Longhorn")]
      Longhorn,

      [Display(Name = "Windows 7")]
      Windows7,

      [Display(Name = "Windows 8")]
      Windows8,

      [Display(Name = "Windows 8.1")]
      Windows81,

      [Display(Name = "Threshold")]
      Threshold,

      [Display(Name = "Threshold 2")]
      Threshold2,

      [Display(Name = "Redstone")]
      Redstone
   }
}