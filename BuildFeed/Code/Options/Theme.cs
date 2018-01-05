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

        public string CookieValue => Value.ToString();
        public string CssPath => $"~/res/css/{Value.ToString().ToLower()}.css";
        public string DisplayName => MvcExtensions.GetDisplayTextForEnum(Value);
        public SiteTheme Value { get; }

        public Theme(SiteTheme st)
        {
            Value = st;
        }

        public static SiteTheme DetectTheme(HttpContextBase context)
        {
            string themeCookie = context.Request.Cookies[THEME_COOKIE_NAME]?.Value;
            var theme = SiteTheme.Dark;
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
        Light,

        [Display(ResourceType = typeof(VariantTerms), Name = nameof(VariantTerms.Common_ThemeWinter))]
        Winter
    }
}