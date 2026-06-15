using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    public class LeadConditionController : BaseController
    {


        ApplicationDbContext db;
        Common com;
        public LeadConditionController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: LeadCondition
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,LeadConditions")]
        public ActionResult GetLeadCondition(string ColName)
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
            var uEdit = User.IsInRole("Edit LeadCondition");
            var uDelete = User.IsInRole("Delete LeadCondition");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.LeadCondition
                     where (ColName == "" || a.LeadCondition == ColName)
                     select new
                     {

                         a.id,
                         a.LeadCondition,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.LeadCondition.ToString().ToLower().Contains(search.ToLower()));
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

        // POST: LeadCondition/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create LeadCondition")]
        public JsonResult Create(leadconditionsview Lead)
        {
            bool stat = false;
            string msg;
            Int64 LeadId = 0;
            if (!ModelState.IsValid)
            {
                var modelErrors = new List<string>();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var modelError in modelState.Errors)
                    {
                        modelErrors.Add(modelError.ErrorMessage);
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var Exists = db.LeadCondition.Any(c => c.LeadCondition== Lead.LeadCondition);
                if (Exists)
                {
                    msg = "LeadCondition already exists.";
                    stat = false;
                }
                else
                {
                    LeadConditions c = new LeadConditions();
                    c.LeadCondition = Lead.LeadCondition;

                    db.LeadCondition.Add(c);
                    db.SaveChanges();
                    LeadId = Lead.leadid;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "LeadCondition", "LeadConditions", findip(), Lead.leadid, "Lead Conditions Added Successfully");

                    msg = "Lead Condition added successfully.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = LeadId } };
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
             LeadConditions Lead= db.LeadCondition.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }



        // POST: LeadTypeLeadCondition/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit(LeadConditions Lead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.LeadCondition.Any(c => c.LeadCondition == Lead.LeadCondition);
                if (Exists)
                {
                    msg = "Lead Condition already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Lead).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "LeadCondition", "LeadConditions", findip(), Lead.id, "Lead Conditions Updated Successfully");


                    msg = "Successfully updated Lead Conditions.";
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
            LeadConditions Lead = db.LeadCondition.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Lead Type")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Customers.Any(c => c.LeadCondition== id);
            if (Exists)
            {
                msg = "Unable to delete Condition, Customer with this Condition exists.";
                stat = false;
            }

            stat = DeleteFn(id);
            msg = "Successfully deleted LeadCondition.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete LeadType")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " LeadCondition, Unable to Delete " + notdel + " LeadCondition. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "LeadCondition.", true);
            }
            else
            {
                Success("Deleted " + count + " LeadCondition.", true);
            }
            return RedirectToAction("Index", "LeadCondition");
        }


        public bool DeleteFn(long id)
        {
            LeadConditions Lead = db.LeadCondition.Find(id);
            if (Lead != null)
            {
                db.LeadCondition.RemoveRange(db.LeadCondition.Where(a => a.id == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "LeadCondition", "LeadConditions", findip(), Lead.id, "LeadCondition Deleted Successfully");
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