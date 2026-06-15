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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using QuickSoft.ViewModel;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ItemCategoryController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ItemCategoryController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: ItemCategory
        [QkAuthorize(Roles = "Dev,Item Category")]
        public ActionResult Index()
        {
            var OpAll = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                               }, "Value", "Text", 1);

            ViewBag.Cats = OpAll;
            ViewBag.ParentModules = OpAll;
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Item Category")]
        public JsonResult GetItemCategory(long? ddlCategory, long? Parent)
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Item Category");
            var uDelete = User.IsInRole("Delete Item Category");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.ItemCategorys
                     join b in db.ItemCategorys on a.Parent equals b.ItemCategoryID into ItemCategorys
                     from b in ItemCategorys.DefaultIfEmpty()
                     where (ddlCategory == null || ddlCategory == 0 || a.ItemCategoryID == ddlCategory) &&
                           (Parent == null || Parent == 0 || a.Parent == Parent)
                     select new
                     {
                         a.Parent,
                         a.ItemCategoryID,
                         a.ItemCategoryName,
                         a.Description,
                         ParentName = b.ItemCategoryName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ItemCategoryName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
       
        [QkAuthorize(Roles = "Dev,Create Item Category")]
        public ActionResult Create()
        {
            var stands = db.ItemCategorys
                         .Select(s => new
                         {
                             FieldID = s.ItemCategoryName,
                             FieldName = s.ItemCategoryID
                         })
                          .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");
            return PartialView();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Item Category")]
        public JsonResult Create(ItemCategoryViewModel ItemCat)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            
            if (ModelState.IsValid)
            {
                var Exists = db.ItemCategorys.Any(c => c.ItemCategoryName == ItemCat.ItemCategoryName);
                
                if (Exists)
                {
                    msg = "Item category already exists.";
                    stat = false;
                }
                else
                {
                    var mods = new ItemCategory
                    {
                        ItemCategoryName = ItemCat.ItemCategoryName,
                        Description = ItemCat.Description,
                        Parent = ItemCat.Parent != null ? Convert.ToInt64(ItemCat.Parent) : 0,
                    };
                    db.ItemCategorys.Add(mods);
                    db.SaveChanges();
                    
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ItemCategory", "ItemCategorys", findip(), mods.ItemCategoryID, "Item Category Added Successfully");


                    msg = "Item category added successfully.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        // GET: ItemCategory/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Item Category")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemCategory info = db.ItemCategorys.Find(id);

            if (info == null)
            {
                return NotFound();
            }
            
            var stands = db.ItemCategorys.Where(s => s.ItemCategoryID != id)
                         .Select(s => new
                         {
                             FieldID = s.ItemCategoryName,
                             FieldName = s.ItemCategoryID
                         })
                         .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");

            return PartialView(info);
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Item Category")]
        public JsonResult Edit(long? id, ItemCategoryViewModel ItmCat)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                ItemCategory info = db.ItemCategorys.Find(id);
                info.ItemCategoryName = ItmCat.ItemCategoryName;
                info.Parent = ItmCat.Parent != null ? Convert.ToInt64(ItmCat.Parent) : 0;
                info.Description = ItmCat.Description;
                db.SaveChanges();
                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Updated, UserId, "ItemCategory", "ItemCategorys", findip(), info.ItemCategoryID, "Item Category Updated Successfully");
                msg = "Successfully updated Item category details.";
                stat = true;
            }            
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        
        [QkAuthorize(Roles = "Dev,Delete Item Category")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemCategory ItemCategory = db.ItemCategorys.Find(id);
            if (ItemCategory == null)
            {
                return NotFound();
            }
            return PartialView(ItemCategory);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Item Category")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Editable = db.ItemCategorys.Any(a => a.Editable == choice.No && a.ItemCategoryID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Category And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                var Exists = db.Items.Any(c => c.ItemCategoryID == id);
                if (Exists)
                {
                    msg = "Unable to delete Category, Items with this Category exists.";
                    stat = false;
                }
                else
                {
                    stat = DeleteFn(id);
                    msg = "Successfully deleted Item category details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Item Category")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Item Category, Unable to Delete " + notdel + " Item Category. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Item Category.", true);
            }
            else
            {
                Success("Deleted " + count + " Item Category.", true);
            }
            return RedirectToAction("Index", "ItemCategory");
        }
        private Boolean DeleteItem(long id)
        {
            var Exists = db.Items.Any(c => c.ItemCategoryID == id);
            bool res = (Exists) ? false : DeleteFn(id);
            return res;
        }

        public bool DeleteFn(long id)
        {
            ItemCategory ItemCategory = db.ItemCategorys.Find(id);
            if (ItemCategory != null)
            {
                db.ItemCategorys.RemoveRange(db.ItemCategorys.Where(a => a.ItemCategoryID == id));
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "ItemCategory", "ItemCategorys", findip(), ItemCategory.ItemCategoryID, "Item Category Deleted Successfully");
            db.SaveChanges();
            return true;
        }

        public JsonResult SearchCategory(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ItemCategorys.Where(p => p.ItemCategoryName.ToLower().Contains(q.ToLower()) || p.ItemCategoryName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemCategoryName, //each json object will have 
                                      id = b.ItemCategoryID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ItemCategorys.Select(b => new SelectFormat
                {
                    text = b.ItemCategoryName, //each json object will have 
                    id = b.ItemCategoryID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "default" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 1, text = "General" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchItemCategory(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ItemCategorys.Where(p => p.ItemCategoryName.ToLower().Contains(q.ToLower()) || p.ItemCategoryName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemCategoryName, //each json object will have 
                                      id = b.ItemCategoryID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ItemCategorys.Select(b => new SelectFormat
                {
                    text = b.ItemCategoryName, //each json object will have 
                    id = b.ItemCategoryID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Category" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
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
