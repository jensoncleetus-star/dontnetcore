using System.Linq.Dynamic.Core;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq.Dynamic;
using System.Net;


namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class LeadStatusController : BaseController
    {
        // GET: LeadStatus
        ApplicationDbContext db;
        Common com;

        public LeadStatusController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [QkAuthorize(Roles = "Dev,LeadStatus")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,LeadStatus")]
        public ActionResult GetLeadStatus()
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

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.LeadStatuss
                     join b in db.Users on a.CreatedBy equals b.Id
                     select new
                     {
                         a.LeadStatusID,
                         a.StatusType,
                         a.Details,
                         b.UserName
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.StatusType.ToString().ToLower().Contains(search.ToLower()));
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


       // [QkAuthorize(Roles = "Dev,Create LeadStatus")]
        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
       // [QkAuthorize(Roles = "Dev,Create LeadStatus")]
        public JsonResult Create([Bind("LeadStatusID,StatusType,Details")] LeadStatus lstatus)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.LeadStatuss.Any(c => c.StatusType == lstatus.StatusType);
                if (Exists)
                {
                    msg = "Lead Status Already Exist.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    lstatus.Status = Status.active;
                    lstatus.CreatedDate = today;
                    lstatus.CreatedBy = UserId;
                    lstatus.Branch = BranchID;
                    lstatus.Editable = choice.Yes;

                    db.LeadStatuss.Add(lstatus);
                    db.SaveChanges();
                    Id = lstatus.LeadStatusID;
                    msg = "Status Type added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "LeadStatus", "LeadStatuss", findip(), Id, "Status Type Added Successfully");



                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }


       // [QkAuthorize(Roles = "Dev,Edit LeadStatus")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeadStatus lstatus = db.LeadStatuss.Find(id);
            if (lstatus == null)
            {
                return NotFound();
            }
            return PartialView(lstatus);
        }

        [HttpPost]
       // [QkAuthorize(Roles = "Dev,Edit LeadStatus")]
        public JsonResult Edit(LeadStatus lstatus, long? id)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.LeadStatuss.Any(c => c.StatusType == lstatus.StatusType && c.LeadStatusID != lstatus.LeadStatusID);
                if (Exists)
                {
                    msg = "Project Type already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    LeadStatus lst = db.LeadStatuss.Find(id);
                    lst.StatusType = lstatus.StatusType;
                    lst.Details = lstatus.Details;

                    db.Entry(lst).State = EntityState.Modified;
                    db.SaveChanges();
                    msg = "Successfully updated Lead Status details.";
                    stat = true;

                    com.addlog(LogTypes.Updated, UserId, "LeadStatus", "LeadStatuss", findip(), lstatus.LeadStatusID, "Project Types Updated Successfully");

                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [QkAuthorize(Roles = "Dev,Delete LeadStatus")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeadStatus lstatus = db.LeadStatuss.Find(id);
            if (lstatus == null)
            {
                return NotFound();
            }
            return PartialView(lstatus);
        }

        // POST: ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [QkAuthorize(Roles = "Dev,Delete LeadStatus")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Customers.Any(c => c.LeadStat == id);
            if (1==2)
            {
                msg = "Unable to delete Type, Customer with this Status exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted LeadStatus.";

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public bool DeleteFn(long id)
        {
            LeadStatus Lead = db.LeadStatuss.Find(id);
            if (Lead != null)
            {
                db.LeadStatuss.RemoveRange(db.LeadStatuss.Where(a => a.LeadStatusID == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "LeadStatus", "LeadStatuss", findip(), Lead.LeadStatusID, "Lead Status Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }


        public JsonResult SearchLeadStatus(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.LeadStatuss.Where(p => p.StatusType.ToLower().Contains(q.ToLower()) || p.StatusType.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.StatusType, //each json object will have 
                                      id = b.LeadStatusID
                                  })
                                  .OrderBy(b => b.text).ToList();
            
            }
            else
            {
              
                serialisedJson = db.LeadStatuss.Select(b => new SelectFormat
                {
                    text = b.StatusType, //each json object will have 
                    id = b.LeadStatusID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Lead Status" };
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
