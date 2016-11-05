using System.Globalization;
using System.Linq;
using System.Web;

namespace BuildFeed.Code.Options
{
   public class Locale
   {
      private const string LANG_COOKIE_NAME = "bf_lang";

      public static readonly Locale[] AvailableLocales =
      {
         new Locale("ar"),
         //new Locale("bn"),
         new Locale("cs"),
         new Locale("de"),
         new Locale("el"),
         new Locale("en"),
         new Locale("es"),
         new Locale("fa"),
         new Locale("fi"),
         new Locale("fr"),
         new Locale("he"),
         new Locale("hr"),
         new Locale("hu"),
         new Locale("id"),
         new Locale("it"),
         new Locale("ko"),
         new Locale("lt"),
         new Locale("nl"),
         new Locale("pl"),
         new Locale("pt"),
         new Locale("pt-br"),
         new Locale("qps-ploc"),
         new Locale("ro"),
         new Locale("ru"),
         new Locale("sk"),
         new Locale("sl"),
         new Locale("sv"),
         new Locale("tr"),
         new Locale("uk"),
         new Locale("vi"),
         new Locale("zh-hans"),
         new Locale("zh-hant")
      };

      public string DisplayName => Info.NativeName;

      public CultureInfo Info { get; set; }
      public string LocaleId { get; set; }

      public Locale(string localeId)
      {
         LocaleId = localeId;
         Info = CultureInfo.GetCultureInfo(localeId);
      }

      public static CultureInfo DetectCulture(HttpContextBase context)
      {
         string langCookie = context.Request.Cookies[LANG_COOKIE_NAME]?.Value;

         if (!string.IsNullOrEmpty(langCookie))
         {
            try
            {
               CultureInfo ci = (CultureInfo)CultureInfo.GetCultureInfo(langCookie).Clone();

               // Get Gregorian Calendar in locale if available
               Calendar gc = ci.OptionalCalendars.FirstOrDefault(c => c is GregorianCalendar && (((GregorianCalendar)c).CalendarType == GregorianCalendarTypes.Localized));
               if (gc != null)
               {
                  ci.DateTimeFormat.Calendar = gc;
               }

               return ci;
            }

            catch (CultureNotFoundException)
            {
            }
         }

         return CultureInfo.CurrentCulture;
      }
   }
}