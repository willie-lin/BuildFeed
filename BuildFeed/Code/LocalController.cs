using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BuildFeed.Code
{
   public class LocalController : Controller
   {
      protected override void Initialize(RequestContext requestContext)
      {
         HttpCookie cookie = requestContext.HttpContext.Request.Cookies["lang"];
         if (!string.IsNullOrEmpty(cookie?.Value))
         {
            try
            {
               CultureInfo ci = CultureInfo.GetCultureInfo(cookie.Value);
               CultureInfo.CurrentCulture = ci;
               CultureInfo.CurrentUICulture = ci;
            }
            catch (CultureNotFoundException) { }
         }

         base.Initialize(requestContext);
      }
   }
}