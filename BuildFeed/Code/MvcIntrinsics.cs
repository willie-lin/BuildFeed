using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;

namespace BuildFeed.Code
{
    public static class MvcIntrinsics
    {
        public static System.Web.Mvc.HtmlHelper Html => ((System.Web.Mvc.WebViewPage) WebPageContext.Current.Page).Html;

        public static System.Web.Mvc.AjaxHelper Ajax => ((System.Web.Mvc.WebViewPage) WebPageContext.Current.Page).Ajax;

        public static System.Web.Mvc.UrlHelper Url => ((System.Web.Mvc.WebViewPage) WebPageContext.Current.Page).Url;
    }
}