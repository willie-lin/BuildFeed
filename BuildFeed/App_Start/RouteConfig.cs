using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace BuildFeed
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.AppendTrailingSlash = true;
            routes.MapHttpRoute("API",
                "api/{action}/{id}",
                new
                {
                    controller = "api",
                    action = "GetBuilds",
                    id = UrlParameter.Optional
                });
            routes.MapMvcAttributeRoutes();
        }
    }
}