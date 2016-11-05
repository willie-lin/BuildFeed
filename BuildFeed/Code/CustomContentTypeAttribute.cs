using System.Web.Mvc;

namespace BuildFeed.Code
{
   // this class is a hacky workaround because Microsoft just don't feel like caching the correct content type
   public class CustomContentTypeAttribute : ActionFilterAttribute
   {
      public string ContentType { get; set; }

      public override void OnResultExecuted(ResultExecutedContext filterContext)
      {
         filterContext.HttpContext.Response.ContentType = ContentType;
      }
   }
}