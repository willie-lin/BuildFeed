using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BuildFeed
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DateTimeModelBinder db = new DateTimeModelBinder();

            ModelBinders.Binders.Add(typeof(DateTime), db);
            ModelBinders.Binders.Add(typeof(DateTime?), db);
        }
    }
}
