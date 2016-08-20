namespace BuildFeed.Model.View
{
   public class FrontPage
   {
      public Build CurrentCanary { get; set; }
      public Build CurrentInsider { get; set; }
      public Build CurrentRelease { get; set; }
   }
}