using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;

namespace BuildFeed
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapHttpRoute(
                name: "API",
                routeTemplate: "api/{action}/{id}",
                defaults: new { controller = "api", action = "GetBuilds", id = UrlParameter.Optional }
            );

            routes.AppendTrailingSlash = true;

            routes.MapMvcAttributeRoutes();
        }
    }
}
