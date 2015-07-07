using System.Web.Mvc;
using System.Web.WebPages;

namespace BuildFeed.Code
{
    public static class MvcIntrinsics
    {
        public static HtmlHelper Html => ((WebViewPage) WebPageContext.Current.Page).Html;

        public static AjaxHelper Ajax => ((WebViewPage) WebPageContext.Current.Page).Ajax;

        public static UrlHelper Url => ((WebViewPage) WebPageContext.Current.Page).Url;
    }
}