using System.ComponentModel.DataAnnotations;
using BuildFeed.Local;

namespace BuildFeed.Models
{
   public enum LevelOfFlight
   {
      [Display(ResourceType = typeof(Model), Name = "FlightNone")]
      None = 0,

      [Display(ResourceType = typeof(Model), Name = "FlightWIS")]
      WIS = 1,

      [Display(ResourceType = typeof(Model), Name = "FlightWIF")]
      WIF = 2,

      [Display(ResourceType = typeof(Model), Name = "FlightOSG")]
      OSG = 3,

      [Display(ResourceType = typeof(Model), Name = "FlightMSIT")]
      MSIT = 4,

      [Display(ResourceType = typeof(Model), Name = "FlightCanary")]
      Canary = 5
   }
}