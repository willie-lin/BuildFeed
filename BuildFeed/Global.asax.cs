using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using BuildFeed.Code;
using BuildFeed.Code.Options;
using BuildFeed.Model;

namespace BuildFeed
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Disable ASP.NET MVC version header
            MvcHandler.DisableMvcResponseHeader = true;

            // Don't bother looking for the legacy aspx view engine.
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            DateTimeModelBinder db = new DateTimeModelBinder();

            ModelBinders.Binders.Add(typeof(DateTime), db);
            ModelBinders.Binders.Add(typeof(DateTime?), db);

            ModelMappings.Initialise();

            Roles.CreateRole("Administrators");
            Roles.CreateRole("Editors");
            Roles.CreateRole("Users");

            MongoConfig.SetupIndexes();
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            string[] parts = custom.Split(';');
            var varyParts = new List<string>();
            HttpContextWrapper contextWrapper = new HttpContextWrapper(context);

            foreach (string part in parts)
            {
                switch (part)
                {
                    case "userName":
                        varyParts.Add($"user:{context.User.Identity.Name}");
                        break;
                    case "lang":
                        varyParts.Add($"lang:{Locale.DetectCulture(contextWrapper).IetfLanguageTag}");
                        break;
                    case "theme":
                        varyParts.Add($"theme:{Theme.DetectTheme(contextWrapper)}");
                        break;
                }
            }

            return string.Join(";", varyParts.OrderBy(s => s));
        }
    }
}