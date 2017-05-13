using System.ComponentModel.DataAnnotations;

namespace BuildFeed.Model
{
    public enum ProjectFamily
    {
        None = 0,

        [Display(Name = "NT 5.0", Description = "Windows 2000")]
        Windows2000 = 1,

        [Display(Name = "Neptune")]
        Neptune = 2,

        [Display(Name = "Whistler", Description = "Windows XP")]
        WindowsXP = 3,

        [Display(Name = ".NET Server", Description = "Server 2003")]
        Server2003 = 4,

        [Display(Name = "Longhorn")]
        Longhorn = 5,

        [Display(Name = "Longhorn", Description = "Windows Vista")]
        WindowsVista = 6,

        [Display(Name = "Windows 7")]
        Windows7 = 7,

        [Display(Name = "Windows 8")]
        Windows8 = 8,

        [Display(Name = "Windows Blue", Description = "Windows 8.1")]
        WindowsBlue = 9,

        [Display(Name = "Threshold", Description = "Windows 10 (Initial Release)")]
        Threshold = 10,

        [Display(Name = "Threshold 2", Description = "Windows 10 (November Update)")]
        Threshold2 = 20,

        [Display(Name = "Redstone", Description = "Windows 10 (Anniversary Update)")]
        Redstone = 30,

        [Display(Name = "Redstone 2", Description = "Windows 10 (Creators Update)")]
        Redstone2 = 40,

        [Display(Name = "Redstone 2 (Feature Update)")]
        Feature2 = 41,

        [Display(Name = "Redstone 3", Description = "Windows 10 (Fall Creators Update)")]
        Redstone3 = 50
    }
}