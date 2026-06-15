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
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    public class TaskGroupController : Controller
    {
        ApplicationDbContext db;
        Common com;
        // GET: TaskGroup
        public TaskGroupController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        //Get Create
        public ActionResult Create()
        {
            var ViewModel = new TaskGroupViewModel
            {
            };

            //DropDown Material Centre
            var Mcs = db.ProTaskTypes.Select(s => new
            {
                id = s.TaskTypeId,
                Name = s.TypeName
            }).ToList();
            ViewBag.tasktype = QkSelect.List(Mcs, "id", "Name");

            //DropDown Employee
            ViewBag.taskstatus = QkSelect.List(
                                         new List<SelectListItem>
                                         {
                                            new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                         }, "Value", "Text", 1);

            return PartialView(ViewModel);
        }

        //Post Create(Saving)
        [HttpPost]
        public JsonResult Create(TaskGroupViewModel ViewModel)
        {
            bool stat = false;
            string msg = "";
            Int64 taststatusid = 0;

            if (ModelState.IsValid)
            {
                if (ViewModel.TaskStatus != null)
                {
                    foreach (var arr in ViewModel.TaskStatus)
                    {
                        taststatusid = Convert.ToInt64(arr);

                        //Getting the Material Centre Name
                        var tasktypename = db.ProTaskTypes.Where(o => o.TaskTypeId == ViewModel.TaskType).Select(o => o.TypeName).FirstOrDefault();

                        //Getting the UserId of Employee
                        var taskstatus = db.TaskStatus.Where(o => o.TaskStatusId == taststatusid).Select(o => o.TaskStatusId).FirstOrDefault();

                        var tasktypen = db.TaskGroup.Where(a => a.TaskStatusId == taststatusid && a.TaskTypeName == tasktypename).FirstOrDefault();

                        if (tasktypen != null)
                        {
                            msg     =   "Task status already assigned to the same task type";
                            stat    =   false;
                        }
                        else
                        {

                            TaskGroup mc = new TaskGroup
                            {
                                TaskStatusId      =   taskstatus,
                                TaskTypeId        =   ViewModel.TaskType,
                                TaskTypeName      =   tasktypename 
                            };

                            db.TaskGroup.Add(mc);
                            db.SaveChanges();

                            msg     =   "Successfully added taskstatus into taskgroup ";
                            stat    =   true;
                        }

                    }
                }
                else
                {
                    msg     =   "Please select task type....";
                    stat    =   false;
                }
            }
            else
            {
                msg     =   "Looks like something went wrong. Please check your form.";
                stat    =   false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //Get Index
        public ActionResult Index()
        {
            return View();
        }

        //Function to retrieve data in Index page
        public JsonResult GetTaskTypewithstatus()
        {
            string search       =   Request.Form.GetValues("search[value]")[0];
            var draw            =   Request.Form.GetValues("draw").FirstOrDefault();
            var start           =   Request.Form.GetValues("start").FirstOrDefault();
            var length          =   Request.Form.GetValues("length").FirstOrDefault();
                       
            int pageSize        =   length != null ? Convert.ToInt32(length) : 0;
            int skip            =   start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal    =   0;

            var UserView = (from a in db.TaskGroup
                            join b in db.TaskStatus on a.TaskStatusId equals b.TaskStatusId into emp
                            from b in emp.DefaultIfEmpty()
                            select new
                            {
                                a.TaskGroupId ,

                                TaskStatus  = (from z in db.TaskGroup
                                             join y in db.TaskStatus on z.TaskStatusId equals y.TaskStatusId
                                             where z.TaskTypeId == a.TaskTypeId
                                             select new
                                             {
                                                 id         =   y.TaskStatusId,
                                                 TaskStatusName  =   (y.StatusName != null) ? y.StatusName : ""
                                             }).ToList(),
                                a.TaskTypeName
                            }).ToList().Select(o => new {
                                o.TaskGroupId,
                                o.TaskStatus,
                                o.TaskTypeName
                            }).GroupBy(x => x.TaskTypeName, (key, g) => g.OrderBy(m => m.TaskTypeName).FirstOrDefault());

            //SEARCH
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.TaskTypeName.ToString().ToLower().Contains(search.ToLower()));
            }
           
            recordsTotal    =   UserView.Count();
            var data        =   UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: TaskGroup/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var TaskGroup = db.TaskGroup.Where(a => a.TaskGroupId == id).FirstOrDefault();

            if (TaskGroup == null)
            {
                return NotFound();
            }                       

            TaskGroupViewModel ViewModel = new TaskGroupViewModel();

            //****** MC
            ViewModel.TaskType      =   TaskGroup.TaskTypeId ;
            ViewModel.TaskTypeName    =   TaskGroup.TaskTypeName ;

            var Mcs = db.ProTaskTypes.Select(s => new { ID = s.TaskTypeId, Name = s.TypeName }).ToList();
            ViewBag.Mc = QkSelect.List(Mcs, "ID", "Name");

            //****** Employees            
            var taskstatus = (from a in db.TaskGroup
                            join c in db.TaskStatus
                            on a.TaskTypeId equals c.TaskStatusId
                            where a.TaskTypeId  == TaskGroup.TaskTypeId
                              select a.TaskStatusId
                            ).ToList();

            ViewModel.TaskStatus = taskstatus.ToArray();  

            var use = db.TaskStatus.Select(s => new { ID = s.TaskStatusId , Name = s.StatusName  }).ToList();
            
            ViewBag.Employees = new MultiSelectList(use, "ID", "Name", ViewModel.TaskStatus);

            return PartialView(ViewModel);
        }

        // POST: Additional MC/Edit/5        
        [RedirectingAction]       
        [HttpPost]
        public JsonResult Edit(long? id, TaskGroupViewModel ViewModel)
        {
            bool stat = false;
            string msg;
            long taskstatus = 0, McId =0;

            if (ModelState.IsValid)
            {
                McId = Convert.ToInt64(id);

                //Delete All Employees for corresponding Material Centre
                var taskgroup = db.TaskGroup.Where(a => a.TaskTypeId ==ViewModel.TaskType);
                if (taskgroup != null)
                {
                    db.TaskGroup.RemoveRange(db.TaskGroup.Where(a => a.TaskTypeId == ViewModel.TaskType));
                    db.SaveChanges();
                }

                if (ViewModel.TaskTypeName != null)
                {
                    foreach (var arr in ViewModel.TaskStatus)
                    {
                        taskstatus = Convert.ToInt64(arr);

                        //Getting the Material Centre Name
                        var tasktype = db.ProTaskTypes.Where(o => o.TaskTypeId == ViewModel.TaskType).Select(o => o.TypeName).FirstOrDefault();

                        //Getting the UserId of Employee
                        var taskstatusid = db.TaskStatus.Where(o => o.TaskStatusId == taskstatus).Select(o => o.TaskStatusId).FirstOrDefault();
                                                
                        TaskGroup mc = new TaskGroup
                        {
                            TaskStatusId      =   taskstatusid,
                            TaskTypeId        =   ViewModel.TaskType,
                            TaskTypeName      =   ViewModel.TaskTypeName 
                        };

                        db.TaskGroup.Add(mc);
                        db.SaveChanges();                       
                    }

                    msg     =   "Successfully Updated ...";
                    stat    =   true;
                }
                else
                {
                    msg     =   "Please select Task Status...";
                    stat    =   false;
                }
            }
            else
            {
                msg     =   "Looks like something went wrong. Please check your form.";
                stat    =   false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //Delete GET
        [HttpGet]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var Obj = db.TaskGroup.Where(a => a.TaskGroupId == id).FirstOrDefault();

            if (Obj == null)
            {
                return NotFound();
            }
            return PartialView(Obj);
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteAction(long id)
        {
            bool stat;
            string msg;
            long McId = id;

            //***********Delete from table TaskGroup

            var Obj1 = db.TaskGroup.Where(a => a.TaskGroupId == McId).Select(o=>o.TaskTypeId).FirstOrDefault();
            var Obj = db.TaskGroup.Where(a => a.TaskTypeId == Obj1).ToList();
            if (Obj != null)
            {
                db.TaskGroup.RemoveRange(Obj);
                db.SaveChanges();
            }

            stat = true;
            msg = "Successfully deleted";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //DropDown Employee
        public JsonResult SearchEmployee(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(p => p.UserId != "" && p.LeavingDate == null && (p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(a => a.UserId != "" && a.LeavingDate == null).Select(b => new SelectFormat
                {
                    text = b.FirstName + " " + b.LastName, //each json object will have 
                    id = b.EmployeeId
                }).OrderBy(b => b.text).ToList();
            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
    }
}
