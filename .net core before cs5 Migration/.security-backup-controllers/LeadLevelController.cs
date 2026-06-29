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
    public class LeadLevelController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public LeadLevelController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: LeadLevel
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Indexchequestatus()
        {
            return View();
        }
        
                         [HttpPost]
        //   [QkAuthorize(Roles = "Dev,Lead Level")]
        public ActionResult GetChequeStatus(string ColName)
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
            var uEdit = User.IsInRole("Edit Lead Level");
            var uDelete = User.IsInRole("Delete Lead Level");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.chequeStatuses
                     where (ColName == "" || a.ChequeStatusName == ColName)
                     select new
                     {

                         a.chequestatusid,
                         a.ChequeStatusName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ChequeStatusName.ToString().ToLower().Contains(search.ToLower()));
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

        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,Lead Level")]
        public ActionResult GetLeadLevel(string ColName)
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
            var uEdit = User.IsInRole("Edit Lead Level");
            var uDelete = User.IsInRole("Delete Lead Level");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.LeadLevels
                     where (ColName == "" || a.Level == ColName)
                     select new
                     {

                         a.LevelId,
                         a.Level,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Level.ToString().ToLower().Contains(search.ToLower()));
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
        public ActionResult CreateCheque()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create LeadLevel")]
        public JsonResult CreateCheque( ChequeStatus st)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.chequeStatuses.Any(c => c.ChequeStatusName == st.ChequeStatusName);
                if (Exists)
                {
                    msg = "Cheque Status already exists.";
                    stat = false;
                }
                else
                {
                    db.chequeStatuses.Add(st);
                    db.SaveChanges();
                    Id = st.chequestatusid;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "chequestatus", "chequstatus", findip(), st.chequestatusid, "cheque status Added Successfully");

                    msg = "Cheque Status added successfully.";
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

        // POST: LeadLevel/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create LeadLevel")]
        public JsonResult Create([Bind("LevelId,Level")] LeadLevel Lead)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.LeadLevels.Any(c => c.Level == Lead.Level);
                if (Exists)
                {
                    msg = "LeadLevel already exists.";
                    stat = false;
                }
                else
                {
                    db.LeadLevels.Add(Lead);
                    db.SaveChanges();
                    Id = Lead.LevelId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "LeadLevel", "LeadLevels", findip(), Lead.LevelId, "Lead Level Added Successfully");

                    msg = "Lead Level added successfully.";
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
            LeadLevel Lead = db.LeadLevels.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }

        public ActionResult Editchequestatus(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChequeStatus st = db.chequeStatuses.Find(id);
            if (st == null)
            {
                return NotFound();
            }
            return PartialView(st);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Editchequestatus(ChequeStatus st)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.chequeStatuses.Any(c => c.ChequeStatusName == st.ChequeStatusName && c.chequestatusid != st.chequestatusid);
                if (Exists)
                {
                    msg = "Cheque Status already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(st).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "cheque status", "cheque status", findip(),st.chequestatusid, "cheque status Details Updated Successfully");


                    msg = "Successfully updated Cheque Status details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // POST: LeadType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("LevelId,Level")] LeadLevel Lead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.LeadLevels.Any(c => c.Level == Lead.Level && c.LevelId != Lead.LevelId);
                if (Exists)
                {
                    msg = "Lead Level already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Lead).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "LeadLevel", "LeadLevels", findip(), Lead.LevelId, "Lead Level Details Updated Successfully");


                    msg = "Successfully updated Lead Level details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        // [QkAuthorize(Roles = "Dev,Delete Lead Level")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeadLevel Lead = db.LeadLevels.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }

        public ActionResult DeleteChequeStatus(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChequeStatus Lead = db.chequeStatuses.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }
        [HttpPost, ActionName("DeleteChequeStatus")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Lead Level")]
        public JsonResult DeleteChequeStatusConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFnChequestatus(id);
            msg = "Successfully deleted LeadLevel.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Lead Level")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFn(id);
            msg = "Successfully deleted LeadLevel.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete LeadLevel")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " LeadLevel, Unable to Delete " + notdel + " LeadLevel. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "LaedLevel.", true);
            }
            else
            {
                Success("Deleted " + count + " LeadLevel.", true);
            }
            return RedirectToAction("Index", "LeadLevel");
        }


        public bool DeleteFn(long id)
        {
            LeadLevel Lead = db.LeadLevels.Find(id);
            if (Lead != null)
            {
                db.LeadLevels.RemoveRange(db.LeadLevels.Where(a => a.LevelId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "LeadLevel", "LeadLevel", findip(), Lead.LevelId, "Lead Type Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }
        public bool DeleteFnChequestatus(long id)
        {
            ChequeStatus Lead = db.chequeStatuses.Find(id);
            if (Lead != null)
            {
                db.chequeStatuses.RemoveRange(db.chequeStatuses.Where(a => a.chequestatusid == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "cheque status", "cheque status", findip(), Lead.chequestatusid, "Cheque Status Deleted Successfully");
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
