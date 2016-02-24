using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BuildFeed
{
   public class MvcApplication : System.Web.HttpApplication
   {
      protected void Application_Start()
      {
         // Don't bother looking for the legacy aspx view engine.
         ViewEngines.Engines.Clear();
         ViewEngines.Engines.Add(new RazorViewEngine());

         AreaRegistration.RegisterAllAreas();
         FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
         RouteConfig.RegisterRoutes(RouteTable.Routes);
         BundleConfig.RegisterBundles(BundleTable.Bundles);

         DateTimeModelBinder db = new DateTimeModelBinder();

         ModelBinders.Binders.Add(typeof(DateTime), db);
         ModelBinders.Binders.Add(typeof(DateTime?), db);

         MongoConfig.SetupIndexes();
      }

      public override string GetVaryByCustomString(HttpContext context, string custom)
      {
         switch (custom)
         {
            case "userName":
               return context.User.Identity.Name.ToLower();
            case "lang":
               return context.Request.Cookies["lang"]?.Value ?? CultureInfo.CurrentUICulture.IetfLanguageTag;
         }

         return "";
      }
   }
}
