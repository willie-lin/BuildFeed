using System;
using System.Collections.Generic;
using System.Web.Routing;

namespace BuildFeed.Model.View
{
   public class SitemapData
   {
      public Dictionary<string, SitemapPagedAction[]> Actions { get; set; }
      public SitemapDataBuildGroup[] Builds { get; set; }

      public string[] Labs { get; set; }
   }

   public class SitemapDataBuildGroup
   {
      public SitemapDataBuild[] Builds { get; set; }
      public BuildGroup Id { get; set; }
   }

   public class SitemapDataBuild
   {
      public Guid Id { get; set; }
      public string Name { get; set; }
   }

   public class SitemapPagedAction
   {
      public string Action => UrlParams["action"].ToString();

      public string Name { get; set; }
      public int Pages { get; set; }

      public string UniqueId => UrlParams.GetHashCode().ToString("X8").ToLower();

      public RouteValueDictionary UrlParams { get; set; }
   }
}