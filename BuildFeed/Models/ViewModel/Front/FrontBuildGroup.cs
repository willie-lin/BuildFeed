using System;

namespace BuildFeed.Models.ViewModel.Front
{
   public class FrontBuildGroup
   {
      public int BuildCount { get; set; }
      public BuildGroup Key { get; set; }
      public DateTime? LastBuild { get; set; }
   }
}