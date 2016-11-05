using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using BuildFeed.Code.Options;

namespace BuildFeed.Controllers
{
   public class BaseController : Controller
   {
      protected override void Initialize(RequestContext requestContext)
      {
         CultureInfo ci = Locale.DetectCulture(requestContext.HttpContext);
         CultureInfo.CurrentCulture = ci;
         CultureInfo.CurrentUICulture = ci;

         ViewBag.Theme = new Theme(Theme.DetectTheme(requestContext.HttpContext));

         base.Initialize(requestContext);
      }
   }
}