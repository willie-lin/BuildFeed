using System;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using BuildFeed.Code.Options;

namespace BuildFeed.Controllers
{
   public class BaseController : Controller
   {
      private const string LANG_COOKIE_NAME = "bf_lang";
      private const string THEME_COOKIE_NAME = "bf_theme";

      protected override void Initialize(RequestContext requestContext)
      {
         string langCookie = requestContext.HttpContext.Request.Cookies[LANG_COOKIE_NAME]?.Value;

         if (!string.IsNullOrEmpty(langCookie))
         {
            try
            {
               CultureInfo ci = CultureInfo.GetCultureInfo(langCookie);
               CultureInfo.CurrentCulture = ci;
               CultureInfo.CurrentUICulture = ci;
            }

            catch (CultureNotFoundException) { }
         }

         string themeCookie = requestContext.HttpContext.Request.Cookies[THEME_COOKIE_NAME]?.Value;
         SiteTheme theme = SiteTheme.Dark;
         if (!string.IsNullOrEmpty(themeCookie))
         {
            Enum.TryParse(themeCookie, out theme);
         }
         ViewBag.Theme = new Theme(theme);

         base.Initialize(requestContext);
      }
   }
}