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
    public class ProTaskTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ProTaskTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,ProTaskType")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,ProTaskType")]
        public ActionResult GetTaskType()
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
            var v = (from a in db.ProTaskTypes
                     join b in db.Users on a.CreatedBy equals b.Id
                     select new
                     {
                         a.TaskTypeId,
                         a.TypeName,
                         b.UserName
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.TypeName.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create ProTaskType")]
        public ActionResult Create()
        {
            return PartialView();
        }


        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create ProTaskType")]
        public JsonResult Create([Bind("TaskTypeId,TypeName")] ProTaskType ttype)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ProTaskTypes.Any(c => c.TypeName == ttype.TypeName);
                if (Exists)
                {
                    msg = "Task Type already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    ttype.Status = Status.active;
                    ttype.CreatedDate = today;
                    ttype.CreatedBy = UserId;
                    ttype.Branch = BranchID;


                    db.ProTaskTypes.Add(ttype);
                    db.SaveChanges();
                    Id = ttype.TaskTypeId;
                    msg = "Task Type added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "ProTaskType", "ProTaskTypes", findip(), Id, "Task Type Added Successfully");
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
        [QkAuthorize(Roles = "Dev,Edit ProTaskType")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTaskType ttype = db.ProTaskTypes.Find(id);
            if (ttype == null)
            {
                return NotFound();
            }
            return PartialView(ttype);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit ProTaskType")]
        public JsonResult Edit(ProTaskType ttype, long? id)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ProTaskTypes.Any(c => c.TypeName == ttype.TypeName && c.TaskTypeId != ttype.TaskTypeId);
                if (Exists)
                {
                    msg = "Task Type already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    ProTaskType tasktype = db.ProTaskTypes.Find(id);
                    tasktype.TypeName = ttype.TypeName;


                    db.Entry(tasktype).State = EntityState.Modified;
                    db.SaveChanges();
                    msg = "Successfully Updated Task Type details.";
                    stat = true;

                    com.addlog(LogTypes.Updated, UserId, "ProTaskType", "ProTaskTypes", findip(), tasktype.TaskTypeId, "Task Type Updated Successfully");

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
        [QkAuthorize(Roles = "Dev,Delete ProTaskType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTaskType ttype = db.ProTaskTypes.Find(id);
            if (ttype == null)
            {
                return NotFound();
            }
            return PartialView(ttype);
        }

        // POST: ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete ProTaskType")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.ProTasks.Any(c => c.TaskType == id);
            if (Exists)
            {
                msg = "Unable to delete Type, Task with this Type exists.";
                stat = false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();

                ProTaskType ttype = db.ProTaskTypes.Find(id);
                db.ProTaskTypes.Remove(ttype);
                db.SaveChanges();
                stat = true;
                msg = "Successfully deleted Task Type details.";

                com.addlog(LogTypes.Deleted, UserId, "ProTaskType", "ProTaskTypes", findip(), ttype.TaskTypeId, "Task Type Deleted Successfully");

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult SearchTaskType(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProTaskTypes.Where(p => p.TypeName.ToLower().Contains(q.ToLower()) || p.TypeName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.TypeName, //each json object will have 
                                      id = b.TaskTypeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ProTaskTypes.Select(b => new SelectFormat
                {
                    text = b.TypeName, //each json object will have 
                    id = b.TaskTypeId
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
