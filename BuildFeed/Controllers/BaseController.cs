using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using BuildFeed.Code.Options;

namespace BuildFeed.Controllers
{
    public class BaseController : Controller
    {
        public static string VersionString = Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyMetadataAttribute))
                .OfType<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key == "GitHash")
                ?.Value
            ?? "N/A";

        protected override void Initialize(RequestContext requestContext)
        {
            CultureInfo ci = Locale.DetectCulture(requestContext.HttpContext);
            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;

            ViewBag.Theme = new Theme(Theme.DetectTheme(requestContext.HttpContext));
            ViewBag.Version = VersionString;

            base.Initialize(requestContext);
        }
    }
}