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
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace QuickSoft.Controllers
{
    public class PriceCategoryController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PriceCategoryController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: PriceCategory
        public ActionResult Index()
        {

            return View();
        }
        [HttpPost]
        public ActionResult GetLocation(string SizeName)
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
            var uEdit = User.IsInRole("Edit PriceCategory");
            var uDelete = User.IsInRole("Delete PriceCategory");


            var v = (from a in db.PriceCategoryPercentages
                     join b in db.PriceCategoryMasters on a.Category equals b.CategoryId into cont1
                     from b in cont1.DefaultIfEmpty()
                    
                     select new
                     {
                         b.Category,
                         a.PriceCategory,
                         a.CategoryId,
                         a.Percentage,
                       
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete


                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Category.ToString().ToLower().Contains(search.ToLower()));
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

        public ActionResult Create()
        {

            //          .Select(s => new
            //              ID = s.CategoryId,
            //               Name = s.Category
            //          })



            var category = db.PriceCategoryMasters
                 .Select(s => new
                 {
                     ID = s.CategoryId,
                     Name = s.Category
                 }).Distinct()
                 .ToList().OrderBy(a => a.Name);
            ViewBag.Category = QkSelect.List(category, "ID", "Name");


            return PartialView();



        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create PriceCategory")]
        public ActionResult Create([Bind(" CategoryId,Category,PriceCategory,Percentage")] PriceCategoryPercentage Locations)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {
                var Exists = db.PriceCategoryPercentages.Any(c => c.Category == Locations.Category);
                if (Exists)
                {
                    msg = "Category Name already exists.";
                    stat = false;
                }
                else
                {
                    db.PriceCategoryPercentages.Add(Locations);
                db.SaveChanges();
                Id = Locations.CategoryId;

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Created, UserId, "PriceCategoryPercentage", "PriceCategoryPercentages", findip(), Locations.CategoryId, "Location Added Successfully");

                msg = "PriceCategory added successfully.";
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
        public ActionResult Edit(long? id)
        {



            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PriceCategoryPercentage Locations = db.PriceCategoryPercentages.Find(id);
            if (Locations == null)
            {
                return NotFound();
            }

            var category = db.PriceCategoryMasters
                 .Select(s => new
                 {
                     ID = s.CategoryId,
                     Name = s.Category
                 }).Distinct()
                 .ToList().OrderBy(a => a.Name);
            ViewBag.Category = QkSelect.List(category, "ID", "Name");

            return PartialView(Locations);
        }



        // POST: PriceCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Edit PriceCategory")]
        //*************************************************************//








        //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        //***************************************************************//

        public JsonResult Edit([Bind(" CategoryId,Category,PriceCategory,Percentage")] PriceCategoryPercentage Locations)

        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.PriceCategoryPercentages.Any(c => c.Category == Locations.Category &&  c.CategoryId != Locations.CategoryId);

                if (Exists)
                {
                    msg = "Price Category already exists.";
                    stat = false;
                }
                else
                {


                    db.Entry(Locations).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "PriceCategoryPercentage", "PriceCategoryPercentages", findip(), Locations.CategoryId, "Price Category Updated Successfully");
                    msg = "Successfully updated Price Category.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }




        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PriceCategoryPercentage Locations = db.PriceCategoryPercentages.Find(id);
            if (Locations == null)
            {
                return NotFound();
            }
            return PartialView(Locations);
        }

        // POST: PriceCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete PriceCategory")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFn(id);
            msg = "Successfully deleted PriceCategory details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete PriceCategory")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Details, Unable to Delete " + notdel + " Details. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Details.", true);
            }
            else
            {
                Success("Deleted " + count + " Details.", true);
            }
            return RedirectToAction("Index", "PriceCategory");
        }

        public bool DeleteFn(long id)
        {
            PriceCategoryPercentage Locations = db.PriceCategoryPercentages.Find(id);
            if (Locations != null)
            {
                db.PriceCategoryPercentages.RemoveRange(db.PriceCategoryPercentages.Where(a => a.CategoryId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "PriceCategory", "PriceCategorys", findip(), Locations.CategoryId, "PriceCategory Deleted Successfully");
                db.SaveChanges();
            }
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



        public JsonResult SearchPriceCategory(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.PriceCategoryMasters.Where(p => p.Category.ToLower().Contains(q.ToLower()) || p.Category.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Category, //each json object will have 
                                      id = b.CategoryId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.PriceCategoryMasters.Select(b => new SelectFormat
                {
                    text = b.Category, //each json object will have 
                    id = b.CategoryId
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Search by PriceCategory" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }









    }
}
