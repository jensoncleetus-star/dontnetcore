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
    public class LeadRejectionController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public LeadRejectionController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: LeadRejection
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
     //   [QkAuthorize(Roles = "Dev,Item Color")]
        public ActionResult GetLeadRejection(string ColName)
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
            var uEdit = User.IsInRole("Edit Lead Rejection");
            var uDelete = User.IsInRole("Delete Lead Rejection");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.LeadRejections
                     where ( ColName == "" ||a.Details == ColName)
                     select new
                     {
                        
                         a.RejectionId,
                         a.Details,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Details.ToString().ToLower().Contains(search.ToLower()));
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


        //[QkAuthorize(Roles = "Dev,Create Lead Rejection")]
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: ItemCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
       // [QkAuthorize(Roles = "Dev,Create Item Color")]
        public JsonResult Create([Bind("RejectionId,Details")] LeadRejection Lead)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.LeadRejections.Any(c => c.Details == Lead.Details);
                if (Exists)
                {
                    msg = "Detalis already exists.";
                    stat = false;
                }
                else
                {
                    db.LeadRejections.Add(Lead);
                    db.SaveChanges();
                    Id = Lead.RejectionId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "LeadRejection", "LeadRejections", findip(), Lead.RejectionId, "Lead Rejection Details Added Successfully");

                    msg = "Lead Rejection Details added successfully.";
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

        //  [QkAuthorize(Roles = "Dev,Edit Lead Rejection")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeadRejection Lead = db.LeadRejections.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
       // [QkAuthorize(Roles = "Dev,Edit Item Color")]
        public JsonResult Edit([Bind("RejectionId,Details")] LeadRejection Lead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.LeadRejections.Any(c => c.Details == Lead.Details && c.RejectionId !=Lead.RejectionId);
                if (Exists)
                {
                    msg = "Item Color already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Lead).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "LeadRejection", "LeadRejections", findip(), Lead.RejectionId, "Lead Rejection Details Updated Successfully");


                    msg = "Successfully updated Lead Rejection details.";
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

        // [QkAuthorize(Roles = "Dev,Delete Lead Rejection")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
             LeadRejection Lead= db.LeadRejections.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
      //  [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            
                    stat = DeleteFn(id);
                    msg = "Successfully deleted LeadRejection Details.";
               
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete LeadRejection Details")]
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
            return RedirectToAction("Index", "LeadRejection");
        }

        public bool DeleteFn(long id)
        {
            LeadRejection Lead = db.LeadRejections.Find(id);
            if (Lead != null)
            {
                db.LeadRejections.RemoveRange(db.LeadRejections.Where(a => a.RejectionId == id));

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "LeadRejection", "LeadRejections", findip(), Lead.RejectionId, "Details Deleted Successfully");
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
    }
}
