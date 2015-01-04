using BuildFeed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BuildFeed.Areas.admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class metaController : Controller
    {
        // GET: admin/meta
        public ActionResult index()
        {
            var currentItems = from i in MetaItem.Select()
                               group i by i.Id.Type into b
                               select b;

            var pendingLabs = MetaItem.SelectUnusedLabs();

            return View();
        }
    }
}