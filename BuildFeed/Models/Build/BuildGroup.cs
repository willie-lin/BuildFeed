namespace BuildFeed.Models
{
   public class BuildGroup
   {
      public byte Major { get; set; }
      public byte Minor { get; set; }
      public ushort Build { get; set; }
      public ushort? Revision { get; set; }

      public override string ToString() => Revision.HasValue ?
           $"{Major}.{Minor}.{Build}.{Revision.Value}" :
           $"{Major}.{Minor}.{Build}";
   }
}