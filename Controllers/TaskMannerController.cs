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
    public class TaskMannerController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public TaskMannerController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [RedirectingAction]
      
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [RedirectingAction]
     
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
            var v = (from a in db.ProTaskManners
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

        public ActionResult Create()
        {
            return PartialView();
        }


        [HttpPost]
        [RedirectingAction]
      
        public JsonResult Create([Bind("TaskTypeId,TypeName")] ProTaskManner ttype)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ProTaskManners.Any(c => c.TypeName == ttype.TypeName);
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


                    db.ProTaskManners.Add(ttype);
                    db.SaveChanges();
                    Id = ttype.TaskTypeId;
                    msg = "Task Type added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "ProTaskManner", "ProTaskManners", findip(), Id, "Task Type Added Successfully");
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
     
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTaskManner ttype = db.ProTaskManners.Find(id);
            if (ttype == null)
            {
                return NotFound();
            }
            return PartialView(ttype);
        }

        [HttpPost]
        [RedirectingAction]
 
        public JsonResult Edit(ProTaskManner ttype, long? id)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ProTaskManners.Any(c => c.TypeName == ttype.TypeName && c.TaskTypeId != ttype.TaskTypeId);
                if (Exists)
                {
                    msg = "Task Type already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    ProTaskManner tasktype = db.ProTaskManners.Find(id);
                    tasktype.TypeName = ttype.TypeName;


                    db.Entry(tasktype).State = EntityState.Modified;
                    db.SaveChanges();
                    msg = "Successfully Updated Task Type details.";
                    stat = true;

                    com.addlog(LogTypes.Updated, UserId, "ProTaskManner", "ProTaskManners", findip(), tasktype.TaskTypeId, "Task Type Updated Successfully");

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

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTaskManner ttype = db.ProTaskManners.Find(id);
            if (ttype == null)
            {
                return NotFound();
            }
            return PartialView(ttype);
        }

        // POST: ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [RedirectingAction]
       
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.ProTasks.Any(c => c.TaskType == id);
            if (1==2)
            {
                msg = "Unable to delete Type, Task with this Type exists.";
                stat = false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();

                ProTaskManner ttype = db.ProTaskManners.Find(id);
                db.ProTaskManners.Remove(ttype);
                db.SaveChanges();
                stat = true;
                msg = "Successfully deleted Task Type details.";

                com.addlog(LogTypes.Deleted, UserId, "ProTaskManner", "ProTaskManners", findip(), ttype.TaskTypeId, "Task Type Deleted Successfully");

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult SearchTaskType(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProTaskManners.Where(p => p.TypeName.ToLower().Contains(q.ToLower()) || p.TypeName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.TypeName, //each json object will have 
                                      id = b.TaskTypeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ProTaskManners.Select(b => new SelectFormat
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
