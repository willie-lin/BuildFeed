namespace BuildFeed.Models.ViewModel.Front
{
   public class FrontPage
   {
      public BuildModel CurrentCanary { get; set; }
      public BuildModel CurrentInsider { get; set; }
      public BuildModel CurrentRelease { get; set; }
   }
}