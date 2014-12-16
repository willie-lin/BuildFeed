using NServiceKit.DataAnnotations;
using NServiceKit.DesignPatterns.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Required = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace BuildFeed.Models
{
    [DataObject]
    public class LabMeta : IHasId<string>
    {
        [Key]
        [Index]
        [@Required]
        public string Id { get; set; }

        [DisplayName("Page Content")]
        [AllowHtml]
        public string PageContent { get; set; }

        [DisplayName("Meta Description")]
        public string MetaDescription { get; set; }
    }
}