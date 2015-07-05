using BuildFeed.Areas.admin.Models.ViewModel;
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

            return View(new MetaListing()
            {
                CurrentItems = from i in MetaItem.Select()
                               group i by i.Id.Type into b
                               orderby b.Key.ToString()
                               select b,

                NewItems = from i in (from l in MetaItem.SelectUnusedLabs()
                                      select new MetaItem()
                                      {
                                          Id = new MetaItemKey()
                                          {
                                              Type = MetaType.Lab,
                                              Value = l
                                          }
                                      }).Concat(
                                        from v in MetaItem.SelectUnusedVersions()
                                        select new MetaItem()
                                        {
                                            Id = new MetaItemKey()
                                            {
                                                Type = MetaType.Version,
                                                Value = v
                                            }
                                      }).Concat(
                                        from y in MetaItem.SelectUnusedYears()
                                        select new MetaItem()
                                        {
                                            Id = new MetaItemKey()
                                            {
                                                Type = MetaType.Year,
                                                Value = y
                                            }
                                      })
                           group i by i.Id.Type into b
                           orderby b.Key.ToString()
                           select b
            });
        }

        public ActionResult create(MetaType type, string value)
        {
            return View(new MetaItem() { Id = new MetaItemKey() { Type = type, Value = value } });
        }

        [HttpPost]
        public ActionResult create(MetaItem meta)
        {
            if (ModelState.IsValid)
            {
                MetaItem.Insert(meta);
                return RedirectToAction("index");
            }

            return View(meta);
        }

        public ActionResult edit(MetaType type, string value)
        {
            return View("create", MetaItem.SelectById(new MetaItemKey() { Type = type, Value = value }));
        }

        [HttpPost]
        public ActionResult edit(MetaItem meta)
        {
            if (ModelState.IsValid)
            {
                MetaItem.Update(meta);
                return RedirectToAction("index");
            }

            return View("create", meta);
        }
    }
}