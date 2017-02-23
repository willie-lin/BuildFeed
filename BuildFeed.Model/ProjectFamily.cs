using System.ComponentModel.DataAnnotations;

namespace BuildFeed.Model
{
    public enum ProjectFamily
    {
        None,

        [Display(Name = "Windows 2000")]
        Windows2000,

        [Display(Name = "Neptune")]
        Neptune,

        [Display(Name = "Windows XP")]
        WindowsXP,

        [Display(Name = "Server 2003")]
        Server2003,

        [Display(Name = "Longhorn")]
        Longhorn,

        [Display(Name = "Vista")]
        WindowsVista,

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
        Redstone,

        [Display(Name = "Redstone 2")]
        Redstone2
    }
}