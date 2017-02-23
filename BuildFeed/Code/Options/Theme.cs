using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BuildFeed.Local;

namespace BuildFeed.Code.Options
{
    public class Theme
    {
        private const string THEME_COOKIE_NAME = "bf_theme";

        public static Theme[] AvailableThemes = (from st in Enum.GetValues(typeof(SiteTheme)).Cast<SiteTheme>()
                                                 select new Theme(st)).ToArray();

        private readonly SiteTheme _siteTheme;

        public string CookieValue => _siteTheme.ToString();
        public string CssPath => $"~/res/css/{_siteTheme.ToString().ToLower()}.css";
        public string DisplayName => MvcExtensions.GetDisplayTextForEnum(_siteTheme);

        public Theme(SiteTheme st)
        {
            _siteTheme = st;
        }

        public static SiteTheme DetectTheme(HttpContextBase context)
        {
            string themeCookie = context.Request.Cookies[THEME_COOKIE_NAME]?.Value;
            SiteTheme theme = SiteTheme.Dark;
            if (!string.IsNullOrEmpty(themeCookie))
            {
                Enum.TryParse(themeCookie, out theme);
            }

            return theme;
        }
    }

    public enum SiteTheme
    {
        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Common_ThemeDark))]
        Dark = 0,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Common_ThemeLight))]
        Light
    }
}