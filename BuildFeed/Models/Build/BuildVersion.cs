namespace BuildFeed.Models
{

   public struct BuildVersion
   {
      public byte Major { get; set; }
      public byte Minor { get; set; }

      public BuildVersion(byte major, byte minor)
      {
         Major = major;
         Minor = minor;
      }

      public override string ToString() => $"{Major}.{Minor}";
   }
}