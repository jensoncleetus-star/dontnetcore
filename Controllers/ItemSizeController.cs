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
using System.Data;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ItemSizeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ItemSizeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: ItemUnit
        [QkAuthorize(Roles = "Dev,Item Size")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Item Size")]
        public ActionResult GetItemSize(string SizeName)
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
            var uEdit = User.IsInRole("Edit Item Size");
            var uDelete = User.IsInRole("Delete Item Size");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.ItemSizes where (SizeName == null || SizeName == "" || a.ItemSizeName == SizeName) select new
            {
                a.Description,
                a.Editable,
                a.ItemSizeID,
                a.ItemSizeName,
                Dev = uDev,
                Edit = uEdit,
                Delete = uDelete


            } );

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ItemSizeName.ToString().ToLower().Contains(search.ToLower()));
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

        [QkAuthorize(Roles = "Dev,Create Item Size")]
        public ActionResult Create()
        {
            try
            {
                return PartialView();

            }
            catch (Exception ex)
            {
                return (ActionResult)ex.Data;
            }
        }

        // POST: ItemCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Item Size")]
        public JsonResult Create([Bind("ItemSizeID,ItemSizeName,Description")] ItemSize ItemSize)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ItemSizes.Any(c => c.ItemSizeName == ItemSize.ItemSizeName);
                if (Exists)
                {
                    msg = "Item Size already exists.";
                    stat = false;
                }
                else
                {
                    db.ItemSizes.Add(ItemSize);
                    db.SaveChanges();
                    Id = ItemSize.ItemSizeID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ItemSize", "ItemSizes", findip(), ItemSize.ItemSizeID, "Item Size Added Successfully");

                    msg = "Item Size added successfully.";
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

        [QkAuthorize(Roles = "Dev,Edit Item Size")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemSize ItemSize = db.ItemSizes.Find(id);
            if (ItemSize == null)
            {
                return NotFound();
            }
            return PartialView(ItemSize);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Item Size")]
        public JsonResult Edit([Bind("ItemSizeID,ItemSizeName,Description")] ItemSize ItemSize)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ItemSizes.Any(c => c.ItemSizeName == ItemSize.ItemSizeName && c.ItemSizeID != ItemSize.ItemSizeID);
                if (Exists)
                {
                    msg = "Item Size already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(ItemSize).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "ItemSize", "ItemSizes", findip(), ItemSize.ItemSizeID, "Item Size Updated Successfully");


                    msg = "Successfully updated Item Size details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: ItemCategory/Delete/5
        [QkAuthorize(Roles = "Dev,Delete Item Size")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemSize ItemSize = db.ItemSizes.Find(id);
            if (ItemSize == null)
            {
                return NotFound();
            }
            return PartialView(ItemSize);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Item Size")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Editable = db.ItemSizes.Any(a => a.Editable == choice.No && a.ItemSizeID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Size And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                var Exists = db.Items.Any(c => c.ItemSizeID == id);
                if (Exists)
                {
                    msg = "Unable to delete Size, Items with this Size exists.";
                    stat = false;
                }
                else
                {
                    stat = DeleteFn(id);
                    msg = "Successfully deleted Item Size details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Item Size")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr) == true)? count++: notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Item Size, Unable to Delete " + notdel + " Item Size. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Item Size.", true);
            }
            else
            {
                Success("Deleted " + count + " Item Size.", true);
            }
            return RedirectToAction("Index", "ItemSize");
        }
        private Boolean DeleteItem(long id)
        {
            var Exists = db.Items.Any(c => c.ItemSizeID == id);
            bool res = (Exists) ? false : DeleteFn(id);
            return res;
        }

        public bool DeleteFn(long id)
        {
            ItemSize ItemSize = db.ItemSizes.Find(id);
            if (ItemSize != null)
            {
                db.ItemSizes.RemoveRange(db.ItemSizes.Where(a => a.ItemSizeID == id));
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "ItemSize", "ItemSizes", findip(), ItemSize.ItemSizeID, "Item Size Deleted Successfully");
            db.SaveChanges();
            return true;
        }

        //For DropDown ItemSize
        public JsonResult SearchItemSize(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ItemSizes.Where(p => p.ItemSizeName.ToLower().Contains(q.ToLower()) || p.ItemSizeName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text  = b.ItemSizeName, //each json object will have 
                                      id    = b.ItemSizeID
                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.ItemSizes.Select(b => new SelectFormat
                {
                    text    = b.ItemSizeName, //each json object will have 
                    id      = b.ItemSizeID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
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
