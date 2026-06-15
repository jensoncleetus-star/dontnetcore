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
    public class ProjectStatusController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ProjectStatusController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,ProjectStatus")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,ProjectStatus")]
        public ActionResult GetProjectStatus()
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
            var v = (from a in db.ProjectStatus
                     join b in db.Users on a.CreatedBy equals b.Id
                     select new
                     {
                         a.ProjectStatusId,
                         a.StatusName,
                         b.UserName
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.StatusName.ToString().ToLower().Contains(search.ToLower()));
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
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create ProjectStatus")]
        public ActionResult Create()
        {
            return PartialView();
        }


        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create ProjectStatus")]
        public JsonResult Create([Bind("ProjectStatusId,StatusName")] ProjectStatus statu)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ProjectStatus.Any(c => c.StatusName == statu.StatusName);
                if (Exists)
                {
                    msg = "Status Name already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    statu.Status = Status.active;
                    statu.CreatedDate = today;
                    statu.CreatedBy = UserId;
                    statu.Branch = BranchID;


                    db.ProjectStatus.Add(statu);
                    db.SaveChanges();
                    Id = statu.ProjectStatusId;
                    msg = "Project Status added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "ProjectStatus", "ProjectStatus", findip(), Id, "Project Status Added Successfully");
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit ProjectStatus")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectStatus pstat = db.ProjectStatus.Find(id);
            if (pstat == null)
            {
                return NotFound();
            }
            return PartialView(pstat);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit ProjectStatus")]
        public JsonResult Edit(ProjectStatus pstat, long? id)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ProjectStatus.Any(c => c.StatusName == pstat.StatusName && c.ProjectStatusId != pstat.ProjectStatusId);
                if (Exists)
                {
                    msg = "Project Status already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    ProjectStatus pstats = db.ProjectStatus.Find(id);
                    pstats.StatusName = pstat.StatusName;


                    db.Entry(pstats).State = EntityState.Modified;
                    db.SaveChanges();
                    msg = "Successfully Updated Project Status details.";
                    stat = true;

                    com.addlog(LogTypes.Updated, UserId, "ProjectStatus", "ProjectStatus", findip(), pstats.ProjectStatusId, "Project Status Updated Successfully");

                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: ProductCategory/Delete/5
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete ProjectStatus")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectStatus pstat = db.ProjectStatus.Find(id);
            if (pstat == null)
            {
                return NotFound();
            }
            return PartialView(pstat);
        }

        // POST: ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete ProjectStatus")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Project Status Deleted Successfully";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        private Boolean DeleteCust(long custid)
        {
            var Msg = chkDeleteWithMsg(custid);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(custid);
            }

        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();

            ProjectStatus pstat = db.ProjectStatus.Find(id);
            db.ProjectStatus.Remove(pstat);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "ProjectStatus", "ProjectStatus", findip(), pstat.ProjectStatusId, "Project Status Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.Projects.Any(c => c.ProjectStatus == id))
            {
                msg = "Project Status Already used in Project";
            }
            else
            {
                msg = null;
            }
            return msg;
        }
        public JsonResult SearchProjectStatus(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProjectStatus.Where(p => p.StatusName.ToLower().Contains(q.ToLower()) || p.StatusName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.StatusName, //each json object will have 
                                      id = b.ProjectStatusId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ProjectStatus.Select(b => new SelectFormat
                {
                    text = b.StatusName, //each json object will have 
                    id = b.ProjectStatusId
                }).OrderBy(b => b.text).ToList();

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
