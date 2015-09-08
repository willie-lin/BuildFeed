using System.Linq;
using System.Web.Mvc;
using BuildFeed.Areas.admin.Models.ViewModel;
using BuildFeed.Models;

namespace BuildFeed.Areas.admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class metaController : Controller
    {
        // GET: admin/meta
        public ActionResult index()
        {
            var currentItems = from i in new MetaItem().Select()
                               group i by i.Id.Type
                               into b
                               select b;

            var pendingLabs = new MetaItem().SelectUnusedLabs();

            return View(new MetaListing
            {
                CurrentItems = from i in new MetaItem().Select()
                               group i by i.Id.Type
                                           into b
                               orderby b.Key.ToString()
                               select b,
                NewItems = from i in (from l in new MetaItem().SelectUnusedLabs()
                                      select new MetaItemModel
                                      {
                                          Id = new MetaItemKey
                                          {
                                              Type = MetaType.Lab,
                                              Value = l
                                          }
                                      })
                                      .Concat(from v in new MetaItem().SelectUnusedVersions()
                                              select new MetaItemModel
                                              {
                                                  Id = new MetaItemKey
                                                  {
                                                      Type = MetaType.Version,
                                                      Value = v
                                                  }
                                              })
                                      .Concat(from y in new MetaItem().SelectUnusedYears()
                                              select new MetaItemModel
                                              {
                                                  Id = new MetaItemKey
                                                  {
                                                      Type = MetaType.Year,
                                                      Value = y
                                                  }
                                              })
                           group i by i.Id.Type
                                       into b
                           orderby b.Key.ToString()
                           select b
            });
        }

        public ActionResult create(MetaType type, string value)
        {
            return View(new MetaItemModel
            {
                Id = new MetaItemKey
                {
                    Type = type,
                    Value = value
                }
            });
        }

        [HttpPost]
        public ActionResult create(MetaItemModel meta)
        {
            if (ModelState.IsValid)
            {
                new MetaItem().Insert(meta);
                return RedirectToAction("index");
            }

            return View(meta);
        }

        public ActionResult edit(MetaType type, string value)
        {
            return View("create", new MetaItem().SelectById(new MetaItemKey
            {
                Type = type,
                Value = value
            }));
        }

        [HttpPost]
        public ActionResult edit(MetaItemModel meta)
        {
            if (ModelState.IsValid)
            {
                new MetaItem().Update(meta);
                return RedirectToAction("index");
            }

            return View("create", meta);
        }
    }
}