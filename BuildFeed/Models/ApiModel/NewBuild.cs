using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuildFeed.Models.ApiModel
{
    public class NewBuild
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public NewBuildObject[] NewBuilds { get; set; }
    }

    public class NewBuildObject
    {
        public uint MajorVersion { get; set; }
        public uint MinorVersion { get; set; }
        public uint Number { get; set; }
        public uint? Revision { get; set; }
        public string Lab { get; set; }
        public DateTime? BuildTime { get; set; }
    }
}