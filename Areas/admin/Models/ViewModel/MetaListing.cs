using BuildFeed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuildFeed.Areas.admin.Models.ViewModel
{
    public class MetaListing
    {
        public IEnumerable<IGrouping<MetaType, MetaItem>> CurrentItems { get; set; }
        public IEnumerable<IGrouping<MetaType, MetaItem>> NewItems { get; set; }
    }
}