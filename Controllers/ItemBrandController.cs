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
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ItemBrandController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ItemBrandController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: ItemBrand
        [QkAuthorize(Roles = "Dev,Item Brand")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Item Brand")]
        public JsonResult GetItemBrand(string BName)
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
            var uEdit = User.IsInRole("Edit Item Brand");
            var uDelete = User.IsInRole("Delete Item Brand");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.ItemBrands where (BName == null ||BName == "" || a.ItemBrandName == BName ) select new
                {
                    a.Description,
                    a.Editable,
                    a.ItemBrandID,
                    a.ItemBrandName,
                    Dev = uDev,
                    Edit = uEdit,
                    Delete = uDelete,


            });

                //search
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    v = v.Where(p => p.ItemBrandName.ToString().ToLower().Contains(search.ToLower()));
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

        [QkAuthorize(Roles = "Dev,Create Item Brand")]
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: ItemCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Item Brand")]
        public JsonResult Create([Bind("ItemBrandID,ItemBrandName,Description")] ItemBrand ItemBrand)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ItemBrands.Any(c => c.ItemBrandName == ItemBrand.ItemBrandName);
                if (Exists)
                {
                    msg = "Item Brand already exists.";
                    stat = false;
                }
                else
                {
                    db.ItemBrands.Add(ItemBrand);
                    db.SaveChanges();
                    Id = ItemBrand.ItemBrandID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ItemBrand", "ItemBrands", findip(), ItemBrand.ItemBrandID, "Item Brand Added Successfully");


                    msg = "Item Brand added successfully.";
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

        [QkAuthorize(Roles = "Dev,Edit Item Brand")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemBrand ItemBrand = db.ItemBrands.Find(id);
            if (ItemBrand == null)
            {
                return NotFound();
            }
            return PartialView(ItemBrand);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Item Brand")]
        public JsonResult Edit([Bind("ItemBrandID,ItemBrandName,Description")] ItemBrand ItemBrand)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ItemBrands.Any(c => c.ItemBrandName == ItemBrand.ItemBrandName && c.ItemBrandID != ItemBrand.ItemBrandID);
                if (Exists)
                {
                    msg = "Item Brand already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(ItemBrand).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "ItemBrand", "ItemBrands", findip(), ItemBrand.ItemBrandID, "Item Brand Updated Successfully");


                    msg = "Successfully updated Item brand details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: ItemCategory/Delete/5
        [QkAuthorize(Roles = "Dev,Delete Item Brand")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemBrand ItemBrand = db.ItemBrands.Find(id);
            if (ItemBrand == null)
            {
                return NotFound();
            }
            return PartialView(ItemBrand);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Item Brand")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Editable = db.ItemBrands.Any(a => a.Editable == choice.No && a.ItemBrandID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Category And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                var Exists = db.Items.Any(c => c.ItemBrandID == id);
                if (Exists)
                {
                    msg = "Unable to delete Brand, Items with this Brand exists.";
                    stat = false;
                }
                else
                {
                    stat = Deletefn(id);
                    msg = "Successfully deleted Item brand details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Item Brand")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr)==true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Item Brand, Unable to Delete " + notdel + " Item Brand. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Item Brand.", true);
            }
            else
            {
                Success("Deleted " + count + " Item Brand.", true);
            }
            return RedirectToAction("Index", "ItemBrand");
        }
        private Boolean DeleteItem(long id)
        {
            var Exists = db.Items.Any(c => c.ItemBrandID == id);
            bool res = (Exists) ? false : Deletefn(id);
            return res;
        }
        public bool Deletefn(long id)
        {
            ItemBrand ItemBrand = db.ItemBrands.Find(id);
            if (ItemBrand != null)
            {
                db.ItemBrands.RemoveRange(db.ItemBrands.Where(a => a.ItemBrandID == id));
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "ItemBrand", "ItemBrands", findip(), ItemBrand.ItemBrandID, "Item Brand Deleted Successfully");
            db.SaveChanges();
            return true;
        }

        public JsonResult SearchBrand(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ItemBrands.Where(p => p.ItemBrandName.ToLower().Contains(q.ToLower()) || p.ItemBrandName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemBrandName, //each json object will have 
                                      id = b.ItemBrandID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ItemBrands.Select(b => new SelectFormat
                {
                    text = b.ItemBrandName, //each json object will have 
                    id = b.ItemBrandID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
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
