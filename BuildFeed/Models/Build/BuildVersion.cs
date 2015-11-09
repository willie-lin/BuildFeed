namespace BuildFeed.Models
{

   public class BuildVersion
   {
      public byte Major { get; set; }
      public byte Minor { get; set; }

      public BuildVersion()
      {
      }

      public BuildVersion(byte major, byte minor)
      {
         Major = major;
         Minor = minor;
      }

      public override string ToString() => $"{Major}.{Minor}";
   }
}