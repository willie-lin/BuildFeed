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

        public Build[] NewBuilds { get; set; }
    }
}