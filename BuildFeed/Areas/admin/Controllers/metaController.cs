using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using BuildFeed.Areas.admin.Models.ViewModel;
using BuildFeed.Controllers;
using BuildFeed.Model;

namespace BuildFeed.Admin.Controllers
{
    [Authorize(Roles = "Administrators")]
    [RouteArea("admin")]
    [RoutePrefix("meta")]
    public class MetaController : BaseController
    {
        private readonly MetaItem _mModel;

        public MetaController()
        {
            _mModel = new MetaItem();
        }

        [Route("")]
        public async Task<ActionResult> Index()
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

        [Route("create/{type}/{value}")]
        public ActionResult Create(MetaType type, string value)
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
        [ValidateAntiForgeryToken]
        [Route("create/{type}/{value}")]
        public async Task<ActionResult> Create(MetaItemModel meta)
        {
            if (ModelState.IsValid)
            {
                await _mModel.Insert(meta);
                return RedirectToAction(nameof(Index));
            }

            return View(meta);
        }

        [Route("edit/{type}/{value}")]
        public async Task<ActionResult> Edit(MetaType type, string value)
        {
            return View(nameof(Create),
                await _mModel.SelectById(new MetaItemKey
                {
                    Type = type,
                    Value = value
                }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit/{type}/{value}")]
        public async Task<ActionResult> Edit(MetaItemModel meta)
        {
            if (ModelState.IsValid)
            {
                await _mModel.Update(meta);
                return RedirectToAction("Index");
            }

            return View(nameof(Create), meta);
        }
    }
}