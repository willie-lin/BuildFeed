using System.Globalization;
using System.Web;
using System.Web.Mvc;
using BuildFeed.Code.Options;

namespace BuildFeed.Code
{
    public class OutputCachePushAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            bool isRtl = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
            var theme = new Theme(Theme.DetectTheme(filterContext.HttpContext));

            filterContext.HttpContext.Response.PushPromise("/res/css/default.css");
            filterContext.HttpContext.Response.PushPromise(VirtualPathUtility.ToAbsolute(theme.CssPath));
            if (isRtl)
            {
                filterContext.HttpContext.Response.PushPromise("/res/css/rtl.css");
            }

            filterContext.HttpContext.Response.PushPromise("/res/ts/bfs.js");
        }
    }
}