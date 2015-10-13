using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;

namespace BuildFeed.Code
{
   public class LocalController : Controller
   {
      protected override void Initialize(RequestContext requestContext)
      {
         var cookie = requestContext.HttpContext.Request.Cookies["lang"];
         if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
         {
            try
            {
               CultureInfo ci = new CultureInfo(cookie.Value);
               CultureInfo.CurrentCulture = ci;
               CultureInfo.CurrentUICulture = ci;
            }
            catch(CultureNotFoundException cnex)
            {

            }
         }

         base.Initialize(requestContext);
      }
   }
}