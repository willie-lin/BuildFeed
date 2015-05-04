using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuildFeed.Models.ViewModel.Front
{
    public class FrontBuildGroup
    {
        public BuildGroup Key { get; set; }
        public DateTime? LastBuild { get; set; }
        public int BuildCount { get; set; }
    }
}