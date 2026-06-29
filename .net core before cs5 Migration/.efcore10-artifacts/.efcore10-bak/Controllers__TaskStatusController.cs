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
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class TaskStatusController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public TaskStatusController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,TaskStatus")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,TaskStatus")]
        public ActionResult GetTaskStatus()
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
            var v = (from a in db.TaskStatus
                     join b in db.Users on a.CreatedBy equals b.Id
                     select new
                     {
                         a.TaskStatusId,
                         a.StatusName,
                         b.UserName,
                         Depts = (from z in db.TaskStatusDepts
                                  join y in db.Departments on z.DeptId equals y.DepartmentID
                                  where z.TaskStatusId == a.TaskStatusId
                                  select new
                                  {
                                      DeptName = (y.DepartmentName != null) ? y.DepartmentName : "",
                                  }).ToList(),
                         Desgn = (from z in db.TaskStatusDesgs
                                  join y in db.Designations on z.DesgId equals y.DesignationID
                                  where z.TaskStatusId == a.TaskStatusId
                                  select new
                                  {
                                      DesgnName = (y.DesignationName != null) ? y.DesignationName : "",
                                  }).ToList(),
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
        [QkAuthorize(Roles = "Dev,Create TaskStatus")]
        public ActionResult Create()
        {
            var dept = db.Departments
                        .Select(s => new
                        {
                            ID = s.DepartmentID,
                            Name = s.DepartmentName
                        })
                        .ToList();
            ViewBag.Depart = QkSelect.List(dept, "ID", "Name");

            var desg = db.Designations
                      .Select(s => new
                      {
                          ID = s.DesignationID,
                          Name = s.DesignationName
                      })
                      .ToList();
            ViewBag.Desgn = QkSelect.List(desg, "ID", "Name");

            return PartialView();
        }


        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create TaskStatus")]
        public JsonResult Create(TaskStatusViewModel vmodel)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.TaskStatus.Any(c => c.StatusName == vmodel.StatusName);
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

                    TaskStatus statu = new TaskStatus();
                    statu.StatusName = vmodel.StatusName;
                    statu.Status = Status.active;
                    statu.CreatedDate = today;
                    statu.CreatedBy = UserId;
                    statu.Branch = BranchID;


                    db.TaskStatus.Add(statu);
                    db.SaveChanges();
                    Id = statu.TaskStatusId;


                    var Dep = Convert.ToString(vmodel.Department);
                    if (Dep != null && Dep != "")
                    {
                        long[] Deps = Dep.Split(',').Select(Int64.Parse).ToArray();

                        TaskStatusDept tskdept = new TaskStatusDept();
                        foreach (var depp in Deps)
                        {
                            tskdept.TaskStatusId = Id;
                            tskdept.DeptId = depp;
                            db.TaskStatusDepts.Add(tskdept);
                            db.SaveChanges();
                        }
                    }

                    var Deg = Convert.ToString(vmodel.Designation);
                    if (Deg != null && Deg != "")
                    {
                        long[] Degs = Deg.Split(',').Select(Int64.Parse).ToArray();

                        TaskStatusDesg tskdesg = new TaskStatusDesg();
                        foreach (var desg in Degs)
                        {
                            tskdesg.TaskStatusId = Id;
                            tskdesg.DesgId = desg;
                            db.TaskStatusDesgs.Add(tskdesg);
                            db.SaveChanges();
                        }
                    }


                    msg = "Task Status added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "TaskStatus", "TaskStatus", findip(), Id, "Task Status Added Successfully");
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
        [QkAuthorize(Roles = "Dev,Edit TaskStatus")]
        public ActionResult Edit(long id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskStatusViewModel vmodel = new TaskStatusViewModel();
            var pstat = db.TaskStatus.Find(id);
            vmodel.StatusName = pstat.StatusName;
            vmodel.TaskStatusId = pstat.TaskStatusId;

            if (pstat == null)
            {
                return NotFound();
            }

            var dep = db.TaskStatusDepts.Where(a => a.TaskStatusId == id).Select(a => a.DeptId).ToList();
            long[] deps = dep.ToArray();

            var depar = db.Departments
                  .Select(s => new
                  {
                      FieldName = s.DepartmentID,
                      FieldID = s.DepartmentName
                  })
                  .ToList();
            ViewBag.Depart = new MultiSelectList(depar, "FieldName", "FieldID", deps);


            var des = db.TaskStatusDesgs.Where(a => a.TaskStatusId == id).Select(a => a.DesgId).ToList();
            long[] dess = des.ToArray();

            var desg = db.Designations
                  .Select(s => new
                  {
                      FieldName = s.DesignationID,
                      FieldID = s.DesignationName
                  })
                  .ToList();
            ViewBag.Desgn = new MultiSelectList(desg, "FieldName", "FieldID", dess);


            return PartialView(vmodel);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit TaskStatus")]
        public JsonResult Edit(TaskStatusViewModel vmodel, long? id)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.TaskStatus.Any(c => c.StatusName == vmodel.StatusName && c.TaskStatusId != vmodel.TaskStatusId);
                if (Exists)
                {
                    msg = "Task Status already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    TaskStatus pstats = db.TaskStatus.Find(id);
                    pstats.StatusName = vmodel.StatusName;


                    db.Entry(pstats).State = EntityState.Modified;
                    db.SaveChanges();


                    var Dept = db.TaskStatusDepts.Where(a => a.TaskStatusId == id).FirstOrDefault();
                    if (Dept != null)
                    {
                        db.TaskStatusDepts.RemoveRange(db.TaskStatusDepts.Where(a => a.TaskStatusId == id));
                        db.SaveChanges();
                    }

                    var Dep = Convert.ToString(vmodel.Department);
                    if (Dep != null && Dep != "")
                    {
                        long[] Deps = Dep.Split(',').Select(Int64.Parse).ToArray();

                        TaskStatusDept tskdept = new TaskStatusDept();
                        foreach (var depp in Deps)
                        {
                            tskdept.TaskStatusId = pstats.TaskStatusId;
                            tskdept.DeptId = depp;
                            db.TaskStatusDepts.Add(tskdept);
                            db.SaveChanges();
                        }
                    }

                    var Desg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == id).FirstOrDefault();
                    if (Desg != null)
                    {
                        db.TaskStatusDesgs.RemoveRange(db.TaskStatusDesgs.Where(a => a.TaskStatusId == id));
                        db.SaveChanges();
                    }

                    var Deg = Convert.ToString(vmodel.Designation);
                    if (Deg != null && Deg != "")
                    {
                        long[] Degs = Deg.Split(',').Select(Int64.Parse).ToArray();

                        TaskStatusDesg tskdesg = new TaskStatusDesg();
                        foreach (var desg in Degs)
                        {
                            tskdesg.TaskStatusId = pstats.TaskStatusId;
                            tskdesg.DesgId = desg;
                            db.TaskStatusDesgs.Add(tskdesg);
                            db.SaveChanges();
                        }
                    }


                    msg = "Successfully Updated Task Status details.";
                    stat = true;

                    com.addlog(LogTypes.Updated, UserId, "TaskStatus", "TaskStatus", findip(), pstats.TaskStatusId, "Task Status Updated Successfully");

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
        [QkAuthorize(Roles = "Dev,Delete TaskStatus")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskStatus pstat = db.TaskStatus.Find(id);
            if (pstat == null)
            {
                return NotFound();
            }
            return PartialView(pstat);
        }

        // POST: ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete TaskStatus")]
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
                msg = "Task Status Deleted Successfully";
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

            TaskStatus pstat = db.TaskStatus.Find(id);
            db.TaskStatus.Remove(pstat);
            db.SaveChanges();

            var Dept = db.TaskStatusDepts.Where(a => a.TaskStatusId == id).FirstOrDefault();
            if (Dept != null)
            {
                db.TaskStatusDepts.RemoveRange(db.TaskStatusDepts.Where(a => a.TaskStatusId == id));
                db.SaveChanges();
            }

            var Desg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == id).FirstOrDefault();
            if (Desg != null)
            {
                db.TaskStatusDesgs.RemoveRange(db.TaskStatusDesgs.Where(a => a.TaskStatusId == id));
                db.SaveChanges();
            }

            com.addlog(LogTypes.Deleted, UserId, "TaskStatus", "TaskStatus", findip(), pstat.TaskStatusId, "Task Status Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.ProTasks.Any(c => c.TaskStatus == id))
            {
                msg = "Status Already used in Task";
            }
            else if (db.ProcessFlows.Any(c => c.TaskStatus == id))
            {
                msg = "Status Already used in Process Flow";
            }
            else if (db.TeamTaskStatus.Any(c => c.TaskStatusId == id))
            {
                msg = "Status Already used in Team";
            }
            else if (db.TaskRemarks.Any(c => c.TaskStatusID == id))
            {
                msg = "Status Already used in Task Remark";
            }
            else if (db.Checklists.Any(c => c.Stage == id))
            {
                msg = "Status Already used in Check lists";
            }
            else
            {
                msg = null;
            }
            return msg;
        }
        public JsonResult SearchTaskStatus2(string q, string x,long? tasktype)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            //conditions
            bool chkValue1 = false;
            bool chkValue2 = false;
            bool chkValue3 = false;
            bool chkValue4 = false;
            var UserId = User.Identity.GetUserId();
            var chkUserIsEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();

            //the logged user is not an employee
            if (chkUserIsEmp == null || chkUserIsEmp == 0)
            {
                chkValue1 = true;
            }
            else
            {
                var dept = db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                var desgn = db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                if (dept == null && desgn == null)
                {
                    chkValue1 = true;
                }
                else if (dept != null && desgn == null)
                {
                    chkValue2 = true;
                }
                else if (dept == null && desgn != null)
                {
                    chkValue3 = true;
                }
                else
                {
                    chkValue4 = true;
                }
            }

            //list only assigned statuses in team creation
            long[] agnstat = new long[] { };
            var chkteam = db.Teams.Where(a => a.TeamLead == chkUserIsEmp).Select(a => a.TeamId).ToList();
            var members = (from a in db.TeamMembers
                           where a.EmployeeId == chkUserIsEmp
                           select new
                           {
                               a.TeamId
                           }).ToList();

            var allteamid = chkteam.Union(members.Select(a => a.TeamId));

            if (allteamid == null || allteamid.Count() == 0)
            {
                agnstat = null;
            }
            else
            {
                agnstat = db.TeamTaskStatus.Where(a => allteamid.Contains(a.TeamId)).Select(a => a.TaskStatusId).Distinct().ToArray();
            }

            if (chkValue1 == true)//the logged user is not an employee &&  user is an employee with no designation & no department assigned
            {
                var alldata = (from b in db.TaskStatus
                               join c in db.TaskGroup on b.TaskStatusId equals c.TaskStatusId
                               where (tasktype == null || c.TaskTypeId == tasktype)
                               select new SelectFormat
                               {
                                   text = b.StatusName, //each json object will have 
                                   id = b.TaskStatusId
                               }).ToList();

                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from b in alldata
                                      where (b.text.ToLower().Contains(q.ToLower()) || b.text.Contains(q) || b.text.StartsWith(q) || b.text.EndsWith(q)) && (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from b in alldata
                                      where (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }//
            }
            else
            {



                IEnumerable<SelectFormat> full = new List<SelectFormat>();
                //nor dept and nor desgn
                var depdesg = (from b in db.TaskStatus
                               let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DeptId).ToList()
                               let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DesgId).ToList()
                               where chkdep.Count() == 0 && chkdesg.Count() == 0
                               select new SelectFormat
                               {
                                   text = b.StatusName, //each json object will have 
                                   id = b.TaskStatusId
                               });
                full = full.Union(depdesg);
                if (chkValue2 == true)//dept
                {

                    Int64 dept = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                    var dep = (from b in db.TaskStatus
                               let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DeptId).ToList()
                               where chkdep.Contains(dept)
                               select new SelectFormat
                               {
                                   text = b.StatusName, //each json object will have 
                                   id = b.TaskStatusId
                               });
                    full = full.Union(dep);

                }
                if (chkValue3 == true)//desgn
                {
                    Int64 desgn = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                    var dessg = (from b in db.TaskStatus
                                 let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DesgId).ToList()
                                 where chkdesg.Contains(desgn)
                                 select new SelectFormat
                                 {
                                     text = b.StatusName, //each json object will have 
                                     id = b.TaskStatusId
                                 });
                    full = full.Union(dessg);
                }
                if (chkValue4 == true)//dept//desgn
                {
                    Int64 dept = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                    Int64 desgn = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                    var depanddesg = (from b in db.TaskStatus
                                      let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DeptId).ToList()
                                      let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DesgId).ToList()
                                      where chkdep.Contains(dept) && chkdesg.Contains(desgn)
                                      select new SelectFormat
                                      {
                                          text = b.StatusName, //each json object will have 
                                          id = b.TaskStatusId
                                      });
                    full = full.Union(depanddesg);
                }
                var fu = full;
                if (tasktype != null)
                {
                    fu = (from a in full
                          join b in db.TaskGroup on a.id equals b.TaskStatusId
                          where (tasktype == null || b.TaskTypeId == tasktype)
                          select new SelectFormat
                          {
                              text = a.text, //each json object will have 
                              id = a.id
                          });
                }
                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from b in fu
                                      where (b.text.ToLower().Contains(q.ToLower()) || b.text.Contains(q) || b.text.StartsWith(q) || b.text.EndsWith(q))
                                       && (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from b in fu
                                      where (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchTaskStatus(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            //conditions
            bool chkValue1 = false;
            bool chkValue2 = false;
            bool chkValue3 = false;
            bool chkValue4 = false;
            var UserId = User.Identity.GetUserId();
            var chkUserIsEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();

            //the logged user is not an employee
            if (chkUserIsEmp == null || chkUserIsEmp == 0)
            {
                chkValue1 = true;
            }
            else
            {
                var dept = db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                var desgn = db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                if (dept == null && desgn == null)
                {
                    chkValue1 = true;
                }
                else if (dept != null && desgn == null)
                {
                    chkValue2 = true;
                }
                else if (dept == null && desgn != null)
                {
                    chkValue3 = true;
                }
                else
                {
                    chkValue4 = true;
                }
            }

            //list only assigned statuses in team creation
            long[] agnstat = new long[] { };
            var chkteam = db.Teams.Where(a => a.TeamLead == chkUserIsEmp).Select(a => a.TeamId).ToList();
            var members = (from a in db.TeamMembers
                           where a.EmployeeId == chkUserIsEmp
                           select new
                           {
                               a.TeamId
                           }).ToList();

            var allteamid = chkteam.Union(members.Select(a=>a.TeamId));

            if (allteamid == null || allteamid.Count() == 0)
            {
                agnstat = null;
            }
            else
            {
                agnstat = db.TeamTaskStatus.Where(a => allteamid.Contains(a.TeamId)).Select(a => a.TaskStatusId).Distinct().ToArray();
            }

            if (chkValue1 == true)//the logged user is not an employee &&  user is an employee with no designation & no department assigned
            {
                var alldata = (from b in db.TaskStatus
                               select new SelectFormat
                               {
                                   text = b.StatusName, //each json object will have 
                                   id = b.TaskStatusId
                               }).ToList();

                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from b in alldata
                                      where (b.text.ToLower().Contains(q.ToLower()) || b.text.Contains(q) || b.text.StartsWith(q) || b.text.EndsWith(q)) && (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from b in alldata
                                      where (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }//
            }
            else
            {



                IEnumerable<SelectFormat> full = new List<SelectFormat>();
                //nor dept and nor desgn
                var depdesg = (from b in db.TaskStatus
                               let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DeptId).ToList()
                               let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DesgId).ToList()
                               where chkdep.Count() == 0 && chkdesg.Count() == 0
                               select new SelectFormat
                               {
                                   text = b.StatusName, //each json object will have 
                                   id = b.TaskStatusId
                               });
                full = full.Union(depdesg);
                if (chkValue2 == true)//dept
                {

                    Int64 dept = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                    var dep = (from b in db.TaskStatus
                               let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DeptId).ToList()
                               where chkdep.Contains(dept)
                               select new SelectFormat
                               {
                                   text = b.StatusName, //each json object will have 
                                   id = b.TaskStatusId
                               });
                    full = full.Union(dep);

                }
                if (chkValue3 == true)//desgn
                {
                    Int64 desgn = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                    var dessg = (from b in db.TaskStatus
                                 let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DesgId).ToList()
                                 where chkdesg.Contains(desgn)
                                 select new SelectFormat
                                 {
                                     text = b.StatusName, //each json object will have 
                                     id = b.TaskStatusId
                                 });
                    full = full.Union(dessg);
                }
                if (chkValue4 == true)//dept//desgn
                {
                    Int64 dept = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                    Int64 desgn = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                    var depanddesg = (from b in db.TaskStatus
                                      let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DeptId).ToList()
                                      let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.TaskStatusId).Select(a => a.DesgId).ToList()
                                      where chkdep.Contains(dept) && chkdesg.Contains(desgn)
                                      select new SelectFormat
                                      {
                                          text = b.StatusName, //each json object will have 
                                          id = b.TaskStatusId
                                      });
                    full = full.Union(depanddesg);
                }

                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from b in full
                                      where (b.text.ToLower().Contains(q.ToLower()) || b.text.Contains(q) || b.text.StartsWith(q) || b.text.EndsWith(q))
                                       && (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from b in full
                                      where (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        public JsonResult SearchleadStatus(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            //conditions
            bool chkValue1 = false;
            bool chkValue2 = false;
            bool chkValue3 = false;
            bool chkValue4 = false;
            var UserId = User.Identity.GetUserId();
            var chkUserIsEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();

            //the logged user is not an employee
            if (chkUserIsEmp == null || chkUserIsEmp == 0)
            {
                chkValue1 = true;
            }
            else
            {
                var dept = db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                var desgn = db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                if (dept == null && desgn == null)
                {
                    chkValue1 = true;
                }
                else if (dept != null && desgn == null)
                {
                    chkValue2 = true;
                }
                else if (dept == null && desgn != null)
                {
                    chkValue3 = true;
                }
                else
                {
                    chkValue4 = true;
                }
            }

            //list only assigned statuses in team creation
            long[] agnstat = new long[] { };
            var chkteam = db.Teams.Where(a => a.TeamLead == chkUserIsEmp).Select(a => a.TeamId).ToList();
            var members = (from a in db.TeamMembers
                           where a.EmployeeId == chkUserIsEmp
                           select new
                           {
                               a.TeamId
                           }).ToList();

            var allteamid = chkteam.Union(members.Select(a => a.TeamId));

            if (allteamid == null || allteamid.Count() == 0)
            {
                agnstat = null;
            }
            else
            {
               
                agnstat = db.LeadTaskStatus.Where(a => allteamid.Contains(a.TeamId)).Select(a => a.TaskStatusId).Distinct().ToArray();
            }

            if (chkValue1 == true)//the logged user is not an employee &&  user is an employee with no designation & no department assigned
            {
                var alldata = (from b in db.LeadStatuss
                               select new SelectFormat
                               {
                                   text = b.StatusType, //each json object will have 
                                   id = b.LeadStatusID,
                               }).ToList();
                alldata.Insert(0, new SelectFormat { id = 500, text = "Lead Approved" });
                alldata.OrderBy(o => o.id);

                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from b in alldata
                                      where (b.text.ToLower().Contains(q.ToLower()) || b.text.Contains(q) || b.text.StartsWith(q) || b.text.EndsWith(q)) && (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from b in alldata
                                      where (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }//
            }
            else
            {
                var empid = db.TeamMembers.Any(o => o.EmployeeId == chkUserIsEmp);
                if (empid == false)
                    empid = db.Teams.Any(o => o.TeamLead == chkUserIsEmp);



                IEnumerable<SelectFormat> full = new List<SelectFormat>();
                //nor dept and nor desgn
                var depdesg = (from b in db.LeadStatuss
                               let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DeptId).ToList()
                               let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DesgId).ToList()
                               where chkdep.Count() == 0 && chkdesg.Count() == 0
                               && empid==true
                               select new SelectFormat
                               {
                                   text = b.StatusType, //each json object will have 
                                   id = b.LeadStatusID
                               });
               
                full = full.Union(depdesg);
                if (chkValue2 == true)//dept
                {

                    Int64 dept = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                    var dep = (from b in db.LeadStatuss
                               let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DeptId).ToList()
                               where chkdep.Contains(dept)
                               select new SelectFormat
                               {
                                   text = b.StatusType, //each json object will have 
                                   id = b.LeadStatusID
                               });
                    full = full.Union(dep);

                }
                if (chkValue3 == true)//desgn
                {
                    Int64 desgn = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                    var dessg = (from b in db.LeadStatuss
                                 let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DesgId).ToList()
                                 where chkdesg.Contains(desgn)
                                 select new SelectFormat
                                 {
                                     text = b.StatusType, //each json object will have 
                                     id = b.LeadStatusID
                                 });
                    full = full.Union(dessg);
                }
                if (chkValue4 == true)//dept//desgn
                {
                    Int64 dept = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                    Int64 desgn = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                    var depanddesg = (from b in db.LeadStatuss
                                      let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DeptId).ToList()
                                      let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DesgId).ToList()
                                      where chkdep.Contains(dept) && chkdesg.Contains(desgn)
                                      select new SelectFormat
                                      {
                                          text = b.StatusType, //each json object will have 
                                          id = b.LeadStatusID
                                      });
                    full = full.Union(depanddesg);
                }

                var fff = full.ToList();
                if(empid==true)
                fff.Insert(0, new SelectFormat { id = 500, text = "Lead Approved" });


                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from b in fff
                                      where (b.text.ToLower().Contains(q.ToLower()) || b.text.Contains(q) || b.text.StartsWith(q) || b.text.EndsWith(q))
                                       && (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from b in fff
                                      where (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchTaskStatusName3(string q, string x,long? tasktype)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.TaskStatus.Where(p => (p.StatusName.ToLower().Contains(q.ToLower()) || p.StatusName.Contains(q) || p.StatusName.StartsWith(q) || p.StatusName.EndsWith(q))&&(tasktype==null||db.TaskGroup.Where(o=>o.TaskTypeId==tasktype).Select(o=>o.TaskStatusId).ToList().Contains(p.TaskStatusId)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.StatusName, //each json object will have 
                                      id = b.TaskStatusId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.TaskStatus
                    .Where(p => (tasktype == null || db.TaskGroup.Where(o => o.TaskTypeId == tasktype).Select(o => o.TaskStatusId).ToList().Contains(p.TaskStatusId)))

                    .Select(b => new SelectFormat
                {
                    text = b.StatusName, //each json object will have 
                    id = b.TaskStatusId
                })
                .OrderBy(b => b.text).ToList();

            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Task Status " };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchTaskStatusName(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.TaskStatus.Where(p =>  p.StatusName.ToLower().Contains(q.ToLower()) || p.StatusName.Contains(q) || p.StatusName.StartsWith(q) || p.StatusName.EndsWith(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.StatusName, //each json object will have 
                                      id = b.TaskStatusId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.TaskStatus.Select(b => new SelectFormat
                {
                    text = b.StatusName, //each json object will have 
                    id = b.TaskStatusId
                }).OrderBy(b => b.text).ToList();

            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Task Status " };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchTaskStatusName2(string q, string x)
        {
            string stt = "All";
            List<SelectFormat> serialisedJson2;
 
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                 serialisedJson2 = db.LeadStatuss.Where(p => p.StatusType.ToLower().Contains(q.ToLower()) || p.StatusType.Contains(q) || p.StatusType.StartsWith(q) || p.StatusType.EndsWith(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.StatusType, //each json object will have 
                                      id = b.LeadStatusID
                                  })
                                  .OrderBy(b => b.text).ToList();
               

            }
            else
            {
                serialisedJson2 = db.LeadStatuss.Select(b => new SelectFormat
                {

                    text = b.StatusType, //each json object will have 
                    id = b.LeadStatusID
                }).OrderBy(b => b.text).ToList();
               

            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson2.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Task Status " };
                serialisedJson2.Insert(0, initial);
            }
            var initial2 = new SelectFormat() { id = 500, text = "Lead Approved" };
            serialisedJson2.Insert(0, initial2);
            return Json(serialisedJson2);
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
