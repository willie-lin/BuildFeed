using System;

namespace BuildFeed.Models.ApiModel
{
   public class NewBuild
   {
      public NewBuildObject[] NewBuilds { get; set; }
      public string Password { get; set; }
      public string Username { get; set; }
   }

   public class NewBuildObject
   {
      public DateTime? BuildTime { get; set; }
      public string Lab { get; set; }
      public uint MajorVersion { get; set; }
      public uint MinorVersion { get; set; }
      public uint Number { get; set; }
      public uint? Revision { get; set; }
   }
}