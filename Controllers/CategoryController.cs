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
    public class CategoryController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public CategoryController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: LeadLevel
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,Category")]
        public ActionResult GetCategory(string ColName)
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
            var uEdit = User.IsInRole("Edit Category");
            var uDelete = User.IsInRole("Delete Category");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.PriceCategoryMasters
                     where (ColName == "" || a.Category == ColName)
                     select new
                     {

                         a.CategoryId,
                         a.Category,
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
            return PartialView();
        }

        // POST:  Category/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create LeadLevel")]
        public ActionResult Create([Bind("CategoryId,Category")] PriceCategoryMaster Lead)
        {
            bool stat = false;
            string msg;
            string msg1;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.PriceCategoryMasters.Any(c => c.Category == Lead.Category);
                if (Exists)
                {
                    msg = "Category already exists.";
                    stat = false;
                }
                else
                {
                    db.PriceCategoryMasters.Add(Lead);
                    db.SaveChanges();
                    Id = Lead.CategoryId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Category", "Categorys", findip(), Lead.CategoryId, "Category Added Successfully");

                    msg = "Successfully Created";
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
            PriceCategoryMaster Lead = db.PriceCategoryMasters.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }



        // POST: /Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("CategoryId,Category")] PriceCategoryMaster Lead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.PriceCategoryMasters.Any(c => c.Category== Lead.Category && c.CategoryId != Lead.CategoryId);
                if (Exists)
                {
                    msg = "Category already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Lead).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "Category", "Categorys", findip(), Lead.CategoryId, "Category Updated Successfully");


                    msg = "Successfully updated Category details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        // [QkAuthorize(Roles = "Dev,Delete Category")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
          PriceCategoryMaster   Lead = db.PriceCategoryMasters.Find(id);
            
           
                if (Lead == null)
                {
                    return NotFound();
                }
           


            
            return PartialView(Lead);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Category")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;


            var Exists = db.PriceCategoryPercentages.Any(c => c.Category == id);
            if (Exists)
            {
                msg = " Cannot delete Category.";
                stat = false;
            }
            else
            {

                stat = DeleteFn(id);
                msg = "Successfully delete Category .";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete Category")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Category, Unable to Delete " + notdel + " Category. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "Category.", true);
            }
            else
            {
                Success("Deleted " + count + " Category.", true);
            }
            return RedirectToAction("Index", "Category");
        }


        public bool DeleteFn(long id)
        {
            PriceCategoryMaster Lead = db.PriceCategoryMasters.Find(id);
            if (Lead != null)
            {
                db.PriceCategoryMasters.RemoveRange(db.PriceCategoryMasters.Where(a => a.CategoryId== id));
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Category", "Category", findip(), Lead.CategoryId, "Category Deleted Successfully");
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

