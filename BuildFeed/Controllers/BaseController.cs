using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using BuildFeed.Code.Options;

namespace BuildFeed.Controllers
{
   public class BaseController : Controller
   {
      private const string LANG_COOKIE_NAME = "bf_lang";

      protected override void Initialize(RequestContext requestContext)
      {
         string langCookie = requestContext.HttpContext.Request.Cookies[LANG_COOKIE_NAME]?.Value;

         if (!string.IsNullOrEmpty(langCookie))
         {
            try
            {
               CultureInfo ci = (CultureInfo)CultureInfo.GetCultureInfo(langCookie).Clone();

               // Get Gregorian Calendar in locale if available
               Calendar gc = ci.OptionalCalendars.FirstOrDefault(c => c is GregorianCalendar && ((GregorianCalendar)c).CalendarType == GregorianCalendarTypes.Localized);
               if (gc != null)
               {
                  ci.DateTimeFormat.Calendar = gc;
               }

               CultureInfo.CurrentCulture = ci;
               CultureInfo.CurrentUICulture = ci;
            }

            catch (CultureNotFoundException) { }
         }

         ViewBag.Theme = new Theme(Theme.DetectTheme(requestContext.HttpContext));

         base.Initialize(requestContext);
      }
   }
}