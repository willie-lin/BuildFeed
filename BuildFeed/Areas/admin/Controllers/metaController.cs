using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BuildFeed.Areas.admin.Models.ViewModel;
using BuildFeed.Models;

namespace BuildFeed.Areas.admin.Controllers
{
   [Authorize(Roles = "Administrators")]
   public class metaController : Controller
   {
      private readonly MetaItem _mModel;

      public metaController() { _mModel = new MetaItem(); }

      // GET: admin/meta
      public async Task<ActionResult> index()
      {
         return View(new MetaListing
         {
            CurrentItems = from i in await _mModel.Select()
                           group i by i.Id.Type
                           into b
                           orderby b.Key.ToString()
                           select b,
            NewItems = from i in (from l in await _mModel.SelectUnusedLabs()
                                  select new MetaItemModel
                                  {
                                     Id = new MetaItemKey
                                     {
                                        Type = MetaType.Lab,
                                        Value = l
                                     }
                                  }).Concat(from v in await _mModel.SelectUnusedVersions()
                                            select new MetaItemModel
                                            {
                                               Id = new MetaItemKey
                                               {
                                                  Type = MetaType.Version,
                                                  Value = v
                                               }
                                            }).Concat(from y in await _mModel.SelectUnusedYears()
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
      public async Task<ActionResult> create(MetaItemModel meta)
      {
         if (ModelState.IsValid)
         {
            await _mModel.Insert(meta);
            return RedirectToAction("index");
         }

         return View(meta);
      }

      public async Task<ActionResult> edit(MetaType type, string value)
      {
         return View("create", await _mModel.SelectById(new MetaItemKey
         {
            Type = type,
            Value = value
         }));
      }

      [HttpPost]
      public async Task<ActionResult> edit(MetaItemModel meta)
      {
         if (ModelState.IsValid)
         {
            await _mModel.Update(meta);
            return RedirectToAction("index");
         }

         return View("create", meta);
      }
   }
}