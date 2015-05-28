using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;

namespace BuildFeed.Code
{
    public static class MvcIntrinsics
    {
        public static System.Web.Mvc.HtmlHelper Html
        {
            get { return ((System.Web.Mvc.WebViewPage)WebPageContext.Current.Page).Html; }
        }

        public static System.Web.Mvc.AjaxHelper Ajax
        {
            get { return ((System.Web.Mvc.WebViewPage)WebPageContext.Current.Page).Ajax; }
        }

        public static System.Web.Mvc.UrlHelper Url
        {
            get { return ((System.Web.Mvc.WebViewPage)WebPageContext.Current.Page).Url; }
        }

    }
}