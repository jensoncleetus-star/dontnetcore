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
    public class ItemUnitController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ItemUnitController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: ItemUnit
        [QkAuthorize(Roles = "Dev,Item Unit")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Item Unit")]
        public ActionResult GetItemUnit(string UName)
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
            var uEdit = User.IsInRole("Edit Item Unit");
            var uDelete = User.IsInRole("Delete Item Unit");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.ItemUnits where (UName == null || UName == "" || a.ItemUnitName == UName) select new
                {
                    a.Description,
                    a.Editable,
                    a.ItemUnitID,
                    a.ItemUnitName,
                    Dev = uDev,
                    Edit = uEdit,
                    Delete = uDelete,

            });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ItemUnitName.ToString().ToLower().Contains(search.ToLower()));
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

        [QkAuthorize(Roles = "Dev,Create Item Unit")]
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: ItemCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Item Unit")]
        public JsonResult Create([Bind("ItemUnitID,ItemUnitName,Description")] ItemUnit ItemUnit)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ItemUnits.Any(c => c.ItemUnitName == ItemUnit.ItemUnitName);
                if (Exists)
                {
                    msg = "Item Unit already exists.";
                    stat = false;
                }
                else
                {
                    db.ItemUnits.Add(ItemUnit);
                    db.SaveChanges();
                    Id = ItemUnit.ItemUnitID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ItemUnit", "ItemUnits", findip(), ItemUnit.ItemUnitID, "ItemUnit Added Successfully");


                    msg = "Item Unit added successfully.";
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

        [QkAuthorize(Roles = "Dev,Edit Item Unit")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemUnit ItemUnit = db.ItemUnits.Find(id);
            if (ItemUnit == null)
            {
                return NotFound();
            }
            return PartialView(ItemUnit);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Item Unit")]
        public JsonResult Edit([Bind("ItemUnitID,ItemUnitName,Description")] ItemUnit ItemUnit)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ItemUnits.Any(c => c.ItemUnitName == ItemUnit.ItemUnitName && c.ItemUnitID != ItemUnit.ItemUnitID);
                if (Exists)
                {
                    msg = "Item Unit already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(ItemUnit).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "ItemUnit", "ItemUnits", findip(), ItemUnit.ItemUnitID, "ItemUnit Updated Successfully");


                    msg = "Successfully updated Item Unit details.";
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
        [QkAuthorize(Roles = "Dev,Delete Item Unit")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemUnit ItemUnit = db.ItemUnits.Find(id);
            if (ItemUnit == null)
            {
                return NotFound();
            }
            return PartialView(ItemUnit);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Item Unit")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Editable = db.ItemUnits.Any(a => a.Editable == choice.No && a.ItemUnitID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Unit And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                var Exists = db.Items.Any(c => c.ItemUnitID == id);
                if (Exists)
                {
                    msg = "Unable to delete Unit, Items with this Unit exists.";
                    stat = false;
                }
                else
                {
                    stat = DeleteFn(id);
                    msg = "Successfully deleted Item Unit details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Item Unit")]
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
                Success("Deleted " + count + " Item Unit, Unable to Delete " + notdel + " Item Unit. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Item Unit.", true);
            }
            else
            {
                Success("Deleted " + count + " Item Unit.", true);
            }
            return RedirectToAction("Index", "ItemUnit");
        }
        private Boolean DeleteItem(long id)
        {
            var Exists = db.Items.Any(c => c.ItemUnitID == id);
            bool res = (Exists) ? false : DeleteFn(id);
            return res;
        }

        public bool DeleteFn(long id)
        {
            ItemUnit ItemUnit = db.ItemUnits.Find(id);
            if (ItemUnit != null)
            {
                db.ItemUnits.RemoveRange(db.ItemUnits.Where(a => a.ItemUnitID == id));
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "ItemUnit", "ItemUnits", findip(), ItemUnit.ItemUnitID, "ItemUnit Deleted Successfully");
            db.SaveChanges();
            return true;
        }

        public JsonResult SearchUnit(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ItemUnits.Where(p => p.ItemUnitName.ToLower().Contains(q.ToLower()) || p.ItemUnitName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemUnitName, //each json object will have 
                                      id = b.ItemUnitID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ItemUnits.Select(b => new SelectFormat
                {
                    text = b.ItemUnitName, //each json object will have 
                    id = b.ItemUnitID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
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
