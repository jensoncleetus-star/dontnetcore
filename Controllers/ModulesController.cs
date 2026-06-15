using QuickSoft.Web;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using System.Net;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Controllers
{
    //[QkAuthorize(Roles = "Dev")]
    public class ModulesController : BaseController
    {
        ApplicationDbContext db;
        //Get Customer Details From Db
        Common com;
        public ModulesController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Modules
        public ActionResult Index()
        {
            return View();
        }

        // datatable fields 
        public JsonResult GetData()
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var ModList = (from a in db.AppModuless
                           join c in db.Users on a.Employee equals c.Id into user
                           from c in user.DefaultIfEmpty()
                           join b in db.AppModuless on a.Parent equals b.ModulesID into module
                           from b in module.DefaultIfEmpty()
                           select new
                           {
                               ModulesID = a.Id,
                               modulesids = a.ModulesID,
                               a.Name,
                               a.iconClass,
                               a.Link,
                               a.Description,
                               a.Status,
                               a.Editable,
                               ParentName = b.Name,
                           });
            ModList = ModList.Where(p => p.Editable == 0);
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.ModulesID.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Link.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Description.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Status.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.ParentName.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        // GET: Modules/Create
        public ActionResult Create()
        {

            var stands = db.AppModuless.Where(s => s.IsParent == (int)choice.Yes)
                          .Select(s => new
                          {
                              FieldID = s.Name,
                              FieldName = s.ModulesID
                          }).OrderBy(a=>a.FieldID)
                          .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");
            return View();
        }

        // POST: Modules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppModules modules)
        {

            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var moduleid = ModuleID();
                var idManager = new IdentityManager();
                string message = "That Module name has already been used";

                if (idManager.RoleExists(modules.Name))
                {
                    Warning(message, true);
                }
                else
                {
                    var modl = new AppModules
                    {
                        Name = modules.Name,
                        ModulesID = moduleid,
                        viewName = modules.viewName,
                        iconClass = modules.iconClass,
                        Link = modules.Link,
                        Parent = modules.Parent,
                        IsParent = modules.IsParent,
                        addMenu = modules.addMenu,
                        Description = modules.Description,
                        Status = modules.Status,
                        MenuOrder = modules.MenuOrder
                    };
                    idManager.CreateRole(modl);

                    com.addlog(LogTypes.Created, UserId, "Modules", "AppModules", findip(), moduleid, "Module Details Added Successfully");



                    Success("Successfully added Module details.", true);
                    return RedirectToAction("Create", "Modules");
                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form..", true);
            }

            var stands = db.AppModuless.Where(s => s.IsParent == (int)choice.Yes)
                 .Select(s => new
                 {
                     FieldID = s.Name,
                     FieldName = s.ModulesID
                 })
                 .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");
            return View();
        }

        // GET: Modules/Edit/5
        public ActionResult Edit(String id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppModules module = db.AppModuless.Find(id);
            if (module == null)
            {
                return NotFound();
            }
            AppModules Mod = new AppModules();

            Mod.ModulesID = module.ModulesID;
            Mod.Name = module.Name;
            Mod.Parent = module.Parent;
            Mod.iconClass = module.iconClass;
            Mod.Link = module.Link;
            Mod.Status = module.Status;
            Mod.Description = module.Description;
            Mod.IsParent = module.IsParent;
            Mod.addMenu = module.addMenu;
            Mod.MenuOrder = module.MenuOrder;
            Mod.viewName = module.viewName;

            var stands = db.AppModuless.Where(s => s.IsParent == (int)choice.Yes && s.ModulesID != Mod.ModulesID)
                          .Select(s => new
                          {
                              FieldID = s.Name,
                              FieldName = s.ModulesID
                          }).OrderBy(a => a.FieldID)
                          .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");
            return View(Mod);
        }

        // POST: Modules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppModules module, String id)
        {
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                AppModules Mod = db.AppModuless.Find(id);
               
                Mod.viewName = module.viewName;
                Mod.iconClass = module.iconClass;
                Mod.Link = module.Link;
                Mod.Parent = module.Parent;
                Mod.IsParent = module.IsParent;
                Mod.Description = module.Description;
                Mod.Status = module.Status;
                Mod.addMenu = module.addMenu;
                Mod.MenuOrder = module.MenuOrder;

                db.Entry(Mod).State = EntityState.Modified;
                db.SaveChanges();

                com.addlog(LogTypes.Updated, UserId, "Modules", "AppModules", findip(), Mod.ModulesID, "Module Details Updated Successfully");


                Success("Successfully updated Module details.", true);
                return RedirectToAction("", "Modules");
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form..", true);
                return Redirect(Request.GetUrlReferrer().ToString());
            }
        }

        // GET: Modules/Delete/5
        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppModules Mod = db.AppModuless.Find(id);
            if (Mod == null)
            {
                return NotFound();
            }
            return PartialView(Mod);
        }

        // POST: Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAction(string id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            AppModules Mod = db.AppModuless.Find(id);
            db.AppModuless.Remove(Mod);
            db.SaveChanges();
            stat = true;

            com.addlog(LogTypes.Deleted, UserId, "Modules", "AppModules", findip(), Mod.ModulesID, "Module Details Deleted Successfully");


            msg = "Successfully deleted Module details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // get module id
        private long ModuleID()
        {
            Int64 VoucherNum = 0;
            var VoucherNo = db.AppModuless.Select(p => p.ModulesID).AsEnumerable().DefaultIfEmpty(0).Max();
            if (VoucherNo == 0)
            {
                VoucherNum = 1;
            }
            else
            {
                VoucherNum = Convert.ToInt64(db.AppModuless.Max(p => p.ModulesID + 1));
            }
            return VoucherNum;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
