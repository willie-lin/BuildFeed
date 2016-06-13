using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BuildFeed.Local;

namespace BuildFeed.Code.Options
{
   public class Theme
   {
      public static Theme[] AvailableThemes = (from st in Enum.GetValues(typeof(SiteTheme)).Cast<SiteTheme>()
                                               select new Theme(st)).ToArray();

      private readonly SiteTheme _siteTheme;

      public string CookieValue => _siteTheme.ToString();
      public string CssPath => $"~/content/{_siteTheme.ToString().ToLower()}.min.css";
      public string DisplayName => MvcExtensions.GetDisplayTextForEnum(_siteTheme);

      public Theme(SiteTheme st) { _siteTheme = st; }
   }

   public enum SiteTheme
   {
      [Display(ResourceType = typeof(Common), Name = nameof(Common.ThemeLight))]
      Light = 0,

      [Display(ResourceType = typeof(Common), Name = nameof(Common.ThemeDark))]
      Dark
   }
}