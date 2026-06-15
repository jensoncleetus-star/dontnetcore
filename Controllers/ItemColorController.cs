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

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ItemColorController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ItemColorController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: ItemColor
        [QkAuthorize(Roles = "Dev,Item Color")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Item Color")]
        public ActionResult GetItemColor(string ColName)
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
            var uEdit = User.IsInRole("Edit Item Color");
            var uDelete = User.IsInRole("Delete Item Color");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.ItemColors where (ColName == null || ColName == "" || a.ItemColorName == ColName) select new
            {
                a.Editable,
                a.ItemColorID,
                a.ItemColorName,
                Dev = uDev,
                Edit = uEdit,
                Delete = uDelete
            });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ItemColorName.ToString().ToLower().Contains(search.ToLower()));
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

        [QkAuthorize(Roles = "Dev,Create Item Color")]
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: ItemCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Item Color")]
        public JsonResult Create([Bind("ItemColorID,ItemColorName")] ItemColor ItemColor)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ItemColors.Any(c => c.ItemColorName == ItemColor.ItemColorName);
                if (Exists)
                {
                    msg = "Item Color already exists.";
                    stat = false;
                }
                else
                {
                    db.ItemColors.Add(ItemColor);
                    db.SaveChanges();
                    Id = ItemColor.ItemColorID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ItemColor", "ItemColors", findip(), ItemColor.ItemColorID, "Item Color Added Successfully");

                    msg = "Item Color added successfully.";
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

        [QkAuthorize(Roles = "Dev,Edit Item Color")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemColor ItemColor = db.ItemColors.Find(id);
            if (ItemColor == null)
            {
                return NotFound();
            }
            return PartialView(ItemColor);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Item Color")]
        public JsonResult Edit([Bind("ItemColorID,ItemColorName")] ItemColor ItemColor)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ItemColors.Any(c => c.ItemColorName == ItemColor.ItemColorName && c.ItemColorID != ItemColor.ItemColorID);
                if (Exists)
                {
                    msg = "Item Color already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(ItemColor).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "ItemColor", "ItemColors", findip(), ItemColor.ItemColorID, "Item Color Updated Successfully");


                    msg = "Successfully updated Item Color details.";
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
        [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemColor ItemColor = db.ItemColors.Find(id);
            if (ItemColor == null)
            {
                return NotFound();
            }
            return PartialView(ItemColor);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Editable = db.ItemColors.Any(a => a.Editable == choice.No && a.ItemColorID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Color And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                var Exists = db.Items.Any(c => c.ItemColorID == id);
                if (Exists)
                {
                    msg = "Unable to delete Color, Items with this Color exists.";
                    stat = false;
                }
                else
                {
                    stat = DeleteFn(id);
                    msg = "Successfully deleted Item Color details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Item Color")]
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
                Success("Deleted " + count + " Item Color, Unable to Delete " + notdel + " Item Color. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Item Color.", true);
            }
            else
            {
                Success("Deleted " + count + " Item Color.", true);
            }
            return RedirectToAction("Index", "ItemColor");
        }
        private Boolean DeleteItem(long id)
        {
            var Exists = db.Items.Any(c => c.ItemColorID == id);
            bool res = (Exists) ? false : DeleteFn(id);
            return res;
        }

        public bool DeleteFn(long id)
        {
            ItemColor ItemColor = db.ItemColors.Find(id);
            if (ItemColor != null)
            {
                db.ItemColors.RemoveRange(db.ItemColors.Where(a => a.ItemColorID == id));
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "ItemColor", "ItemColors", findip(), ItemColor.ItemColorID, "Item Color Deleted Successfully");
            db.SaveChanges();
            return true;
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
