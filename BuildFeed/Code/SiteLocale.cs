using System.Globalization;

namespace BuildFeed.Code
{
   public class SiteLocale
   {
      public static readonly SiteLocale[] AvailableLocales = new SiteLocale[]
      {
         new SiteLocale("ar"),
         new SiteLocale("bn"),
         new SiteLocale("cs"),
         new SiteLocale("de"),
         new SiteLocale("el"),
         new SiteLocale("en"),
         new SiteLocale("es"),
         new SiteLocale("fi"),
         new SiteLocale("fr"),
         new SiteLocale("he"),
         new SiteLocale("hr"),
         new SiteLocale("id"),
         new SiteLocale("it"),
         new SiteLocale("nl"),
         new SiteLocale("pl"),
         new SiteLocale("pt"),
         new SiteLocale("pt-br"),
         new SiteLocale("qps-ploc"),
         new SiteLocale("ro"),
         new SiteLocale("ru"),
         new SiteLocale("sk"),
         new SiteLocale("sl"),
         new SiteLocale("sv"),
         new SiteLocale("tr"),
         new SiteLocale("zh-cn"),
         new SiteLocale("zh-tw")
      };

      public CultureInfo Info { get; set; }
      public string LocaleId { get; set; }

      public string DisplayName => Info.NativeName;

      public SiteLocale(string localeId)
      {
         LocaleId = localeId;
         Info = CultureInfo.GetCultureInfo(localeId);
      }
   }
}