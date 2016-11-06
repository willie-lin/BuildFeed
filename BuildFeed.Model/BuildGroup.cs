namespace BuildFeed.Model
{
   public struct BuildGroup
   {
      public uint Major { get; set; }
      public uint Minor { get; set; }
      public uint Build { get; set; }
      public uint? Revision { get; set; }

      public override string ToString() => Revision.HasValue
         ? $"{Major}.{Minor}.{Build}.{Revision.Value}"
         : $"{Major}.{Minor}.{Build}";
   }
}