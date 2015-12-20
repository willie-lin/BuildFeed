using System.Web.Optimization;

namespace BuildFeed
{
   public class BundleConfig
   {
      // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
      public static void RegisterBundles(BundleCollection bundles)
      {
         bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
             "~/Scripts/jquery-{version}.js"));

         bundles.Add(new ScriptBundle("~/bundles/jsrender").Include(
             "~/Scripts/jsrender*"));

         bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
             "~/Scripts/jquery.validate*"));

         bundles.Add(new StyleBundle("~/content/css").Include(
             "~/content/style.css"));

         bundles.Add(new StyleBundle("~/content/rtl").Include(
             "~/content/rtl.css"));
      }
   }
}