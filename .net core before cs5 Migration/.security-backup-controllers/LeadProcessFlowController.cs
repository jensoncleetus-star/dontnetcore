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
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    public class LeadProcessFlowController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public LeadProcessFlowController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: ProcessFlow
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Lead ProcessFlow List")]
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        //[Authorize(Roles = "Dev,Lead ProcessFlow List")]
        [HttpPost]
        public ActionResult GetProcessFlow()
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

            var UserView = (from a in db.LeadProcessFlows
                            join b in db.LeadStatuss on a.LeadStatus equals b.LeadStatusID into tmp
                            from b in tmp.DefaultIfEmpty()
                            select new
                            {
                                a.LeadProcessFlowId,
                                a.RemoveUpdateUser,
                                a.RemoveUpdateUserTeams,
                                //AssignTypes = (a.AssignType != -1) ? db.Employees.Where(x => x.EmployeeId == a.AssignType).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(): "Individual",

                                ////TeamLead = (from z in db.Teams
                                ////            join y in db.Employees on z.TeamLead equals y.EmployeeId
                                ////            where z.TeamId == a.AssignType
                                ////            select new
                                ////            {
                                ////                Name = y.FirstName + " " + y.LastName
                                ////            }).FirstOrDefault(),
                                LeadStatus =a.LeadStatus==500? "Lead Approved": b.StatusType,
                            }).ToList().Select(o => new
                            {
                                id = o.LeadProcessFlowId,
                                AssignType = (from z in db.LeadProcessFlowAssignTypes
                                              join w in db.Teams on z.TeamId equals w.TeamId
                                              join y in db.Employees on w.TeamLead equals y.EmployeeId
                                              where z.LeadProcessFlowId == o.LeadProcessFlowId
                                              select new
                                              {
                                                  w.TeamName,
                                              }).ToList(),
                                o.RemoveUpdateUser,
                                o.RemoveUpdateUserTeams,
                                AssignedUsers = (from z in db.LeadProcessFlowAssignUsers
                                                 join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                                 where z.LeadProcessFlowId == o.LeadProcessFlowId
                                                 select new
                                                 {
                                                     id = y.EmployeeId,
                                                     LastName = (y.LastName != null) ? y.LastName : "",
                                                     FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                     MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                     Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                     y.Status
                                                 }).ToList(),
                                o.LeadStatus ,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.LeadStatus.ToString().ToLower().Contains(search.ToLower()));
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [RedirectingAction]
        // [QkAuthorize(Roles = "Dev,Create Lead ProcessFlow")]
        public ActionResult Create(long? id)
        {
            LeadProcessFlowsViewModel vmodel = new LeadProcessFlowsViewModel();
            ViewBag.AssignTypes = QkSelect.List(
                                           new List<SelectListItem>
                                           {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                           }, "Value", "Text", 0);

            var tsks = db.LeadStatuss
                  .Select(s => new
                  {
                      ID = s.LeadStatusID,
                      Name = s.StatusType
                  })
                  .ToList();


            ViewBag.LeadStatus = QkSelect.List(tsks, "ID", "Name");


            var use = db.Employees
                  .Select(s => new
                  {
                      ID = s.EmployeeId,
                      Name = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");
            var user = db.Employees
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.AssignUsers = QkSelect.List(user, "ID", "Name");
            var tskss = db.TaskStatus
                  .Select(s => new  KeyValue
                  {
                      ID = s.TaskStatusId,
                      Name = s.StatusName
                  })
                  .ToList();

            ViewBag.TaskStatus = QkSelect.List(tskss, "ID", "Name");
            return PartialView();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create Lead ProcessFlow")]
        [ValidateAntiForgeryToken]
        public JsonResult Create(LeadProcessFlowsViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var statusExists = db.LeadProcessFlows.Any(u => u.LeadStatus == vmodel.LeadStatus);
            if (statusExists)
            {
                msg = "An Lead Process Flow with same Lead Status exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    LeadProcessFlow pf = new LeadProcessFlow();
                    pf.LeadStatus = vmodel.LeadStatus;
                    pf.RemoveUpdateUser = vmodel.RemoveUpdateUser;
                    pf.RemoveUpdateUserTeams = vmodel.RemoveUpdateUserTeams;
                    if(vmodel.ApprovalRequierd==true)
                    {
                        pf.approvalreq = true;
                    }

                    pf.CreatedDate = today;
                    pf.CreatedBy = UserId;
                    pf.Status = Status.active;
                    pf.Branch = BranchID;
                    if (vmodel.MoveToFieldService == true)
                    {
                        pf.movetofieldservice = true;
                        pf.taskid = vmodel.TaskStatus;
                    }
                    db.LeadProcessFlows.Add(pf);
                    db.SaveChanges();
                    Int64 pfId = pf.LeadProcessFlowId;

                    if (vmodel.AssignTypeAll != null)
                    {
                        LeadProcessFlowAssignType tskax = new LeadProcessFlowAssignType();
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            tskax.LeadProcessFlowId = pfId;
                            tskax.TeamId = arr;
                            db.LeadProcessFlowAssignTypes.Add(tskax);
                            db.SaveChanges();
                        }
                    }

                    //assigned members
                    if (vmodel.AssignedUsers != null)
                    {
                        IList<LeadProcessFlowAssignUser> tskstat = new List<LeadProcessFlowAssignUser>();
                        foreach (var arr in vmodel.AssignedUsers)
                        {
                            tskstat.Add(new LeadProcessFlowAssignUser() { LeadProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (tskstat != null)
                        {
                            db.LeadProcessFlowAssignUsers.AddRange(tskstat);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.ApprovalRequierd == true)
                    {
                        if (vmodel.AssignUsers != null)
                        {
                            IList<LeadApprovals> LA = new List<LeadApprovals>();
                            foreach (var arr in vmodel.AssignUsers)
                            {
                                LA.Add(new LeadApprovals() { LeadProcessFlowId = (long)pfId, LeadEmployeeId = arr, LeadTaskStatus = vmodel.TaskStatus.Value });
                            }
                            if (LA != null)
                            {
                                db.LeadApprovals.AddRange(LA);
                                db.SaveChanges();
                            }
                        }
                    }
                   
                        com.addlog(LogTypes.Created, UserId, "LeadProcessFlow", "LeadProcessFlows", findip(), pf.LeadProcessFlowId, "Lead Process Flow Added Successfully");
                    msg = "Successfully added Lead Process Flow details.";
                    stat = true;
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                }
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        [RedirectingAction]
        // [QkAuthorize(Roles = "Dev,Edit ProcessFlow")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeadProcessFlow teamaa = db.LeadProcessFlows.Find(id);
            var user = db.Employees
                .Select(s => new
                {
                    ID = s.EmployeeId,
                    Name = s.FirstName + " " + s.LastName
                })
                .ToList();


            var apuserd=db.LeadApprovals.Where(o => o.LeadProcessFlowId == id).Select(o => o.LeadEmployeeId).ToList().ToArray()??null;
            ViewBag.AssignUserss = new MultiSelectList(user, "ID", "Name", apuserd);
            if (teamaa == null)
            {
                return NotFound();
            }
            LeadProcessFlowsViewModel vmodel = new LeadProcessFlowsViewModel();

            vmodel.LeadProcessFlowId = teamaa.LeadProcessFlowId;
            vmodel.LeadStatus = teamaa.LeadStatus;
            vmodel.RemoveUpdateUser = teamaa.RemoveUpdateUser;
            vmodel.RemoveUpdateUserTeams = teamaa.RemoveUpdateUserTeams;
            vmodel.ApprovalRequierd = teamaa.approvalreq;
            vmodel.MoveToFieldService = teamaa.movetofieldservice;
            vmodel.TaskStatus = teamaa.taskid;

            var tsks = db.LeadStatuss
                .Select(s => new KeyValue
                {
                    ID = s.LeadStatusID,
                    Name = s.StatusType
                })
                .ToList();


            ViewBag.LeadStatus = QkSelect.List(tsks, "ID", "Name", vmodel.LeadStatus);
            var ta = db.TaskStatus
                .Select(s => new KeyValue
                {
                    ID = s.TaskStatusId,
                    Name = s.StatusName
                }
            ).ToList();


            ViewBag.TaskStatuss = QkSelect.List(ta, "ID", "Name", teamaa.taskid);

            var atype = db.LeadProcessFlowAssignTypes.Where(a => a.LeadProcessFlowId == id).Select(a => a.TeamId).ToArray();
            var asstype = db.Teams
              .Select(s => new
              {
                  ID = s.TeamId,
                  Name = s.TeamName
              })
              .ToList();
            ViewBag.AssignTypes = new MultiSelectList(asstype, "ID", "Name", atype);
            vmodel.AssignedUsers = db.LeadProcessFlowAssignUsers.Where(a => a.LeadProcessFlowId == id).Select(a => a.EmployeeId).ToList().ToArray() ?? null;
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.team = new MultiSelectList(use, "ID", "Name", vmodel.AssignedUsers);

            vmodel.AssignUsers = db.LeadApprovals.Where(a => a.LeadProcessFlowId == id).Select(a => a.LeadEmployeeId).ToList().ToArray() ?? null;
            var use1 = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.AssignUser = new MultiSelectList(use1, "ID", "Name", vmodel.AssignUsers);



            return PartialView(vmodel);
        }

        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit Lead ProcessFlow")].
        [HttpPost]
        public JsonResult Edit(long? id, LeadProcessFlowsViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var statusExists = db.LeadProcessFlows.Any(u => u.LeadStatus == vmodel.LeadStatus && vmodel.LeadStatus!=500 && u.LeadProcessFlowId != id);

            if (statusExists)
            {
                msg = "An Lead Process Flow with same Lead Status exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    LeadProcessFlow pf = db.LeadProcessFlows.Find(id);

                    pf.LeadStatus = vmodel.LeadStatus;
                    if (vmodel.ApprovalRequierd == true)
                    {
                        pf.approvalreq = true;
                    }
                    else
                    {
                        pf.approvalreq = false;
                    }
                    pf.RemoveUpdateUser = vmodel.RemoveUpdateUser;
                    pf.RemoveUpdateUserTeams = vmodel.RemoveUpdateUserTeams;
                    if (vmodel.MoveToFieldService == true)
                    {
                        pf.movetofieldservice = true;
                        pf.taskid = vmodel.TaskStatus;
                    }
                    else
                    {
                        pf.movetofieldservice = false;
                        pf.taskid = vmodel.TaskStatus;
                    }
                    db.Entry(pf).State = EntityState.Modified;
                    db.SaveChanges();



                    Int64 pfId = pf.LeadProcessFlowId;

                    var pftype = db.LeadProcessFlowAssignTypes.Where(a => a.LeadProcessFlowId == pfId);
                    if (pftype != null)
                    {
                        db.LeadProcessFlowAssignTypes.RemoveRange(db.LeadProcessFlowAssignTypes.Where(a => a.LeadProcessFlowId == pfId));
                        db.SaveChanges();
                    }
                    if (vmodel.AssignTypeAll != null)
                    {
                        LeadProcessFlowAssignType tskax = new LeadProcessFlowAssignType();
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            tskax.LeadProcessFlowId = pfId;
                            tskax.TeamId = arr;
                            db.LeadProcessFlowAssignTypes.Add(tskax);
                            db.SaveChanges();
                        }
                    }


                    var teammembers = db.LeadProcessFlowAssignUsers.Where(a => a.LeadProcessFlowId == pfId);
                    if (teammembers != null)
                    {
                        db.LeadProcessFlowAssignUsers.RemoveRange(db.LeadProcessFlowAssignUsers.Where(a => a.LeadProcessFlowId == pfId));
                        db.SaveChanges();
                    }

                    //assigned members
                    if (vmodel.AssignedUsers != null)
                    {
                        IList<LeadProcessFlowAssignUser> tskstat = new List<LeadProcessFlowAssignUser>();
                        foreach (var arr in vmodel.AssignedUsers)
                        {
                            tskstat.Add(new LeadProcessFlowAssignUser() { LeadProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (tskstat != null)
                        {
                            db.LeadProcessFlowAssignUsers.AddRange(tskstat);
                            db.SaveChanges();
                        }
                    }
                    
                    if(vmodel.LeadStatus== 500)
                    {
                        var ABC = db.LeadApprovals.Where(a => a.LeadProcessFlowId == pfId);
                        if (ABC != null)
                        {
                            db.LeadApprovals.RemoveRange(db.LeadApprovals.Where(a => a.LeadProcessFlowId == pfId));
                            db.SaveChanges();
                        }
                          if (vmodel.AssignUsers != null)
                            {
                                IList<LeadApprovals> LA = new List<LeadApprovals>();
                                foreach (var arr in vmodel.AssignUsers)
                                {
                                    LA.Add(new LeadApprovals() { LeadProcessFlowId = (long)pfId, LeadEmployeeId = arr });
                                }
                                if (LA != null)
                                {
                                    db.LeadApprovals.AddRange(LA);
                                    db.SaveChanges();
                                }
                            }
                       
                    }
                    if (vmodel.ApprovalRequierd == true)
                    {
                        if (vmodel.AssignUsers != null)
                        {
                            IList<LeadApprovals> LA = new List<LeadApprovals>();
                            foreach (var arr in vmodel.AssignUsers)
                            {
                                LA.Add(new LeadApprovals() { LeadProcessFlowId = (long)pfId, LeadEmployeeId = arr, LeadTaskStatus = vmodel.TaskStatus.Value });
                            }
                            if (LA != null)
                            {
                                db.LeadApprovals.AddRange(LA);
                                db.SaveChanges();
                            }
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "LeadProcessFlow", "LeadProcessFlows", findip(), pfId, "Lead Process Flow Updated Successfully");
                    msg = "Successfully Updated Lead Process Flow details.";
                    stat = true;
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                }

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }


        // GET: /Delete/5
        [RedirectingAction]
        // [Authorize(Roles = "Dev,Delete Lead ProcessFlow")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeadProcessFlow pf = db.LeadProcessFlows.Find(id);
            if (pf == null)
            {
                return NotFound();
            }
            return PartialView(pf);
        }

        // POST: /Delete/5

        [RedirectingAction]
        // [Authorize(Roles = "Dev,Delete LeadProcessFlow")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            LeadProcessFlow pf = db.LeadProcessFlows.Find(id);

            var pfuser = db.LeadProcessFlowAssignUsers.Where(a => a.LeadProcessFlowId == id).ToList();
            if (pfuser.Any())
            {
                foreach (var arr in pfuser)
                {
                    var pfmember = db.LeadProcessFlowAssignUsers.Where(a => a.LeadProcessFlowId == arr.LeadProcessFlowId);
                    if (pfmember != null)
                    {
                        db.LeadProcessFlowAssignUsers.RemoveRange(db.LeadProcessFlowAssignUsers.Where(a => a.LeadProcessFlowId == arr.LeadProcessFlowId));
                        db.SaveChanges();
                    }
                }
               
            }

            var lpft = db.LeadProcessFlowAssignTypes.Where(a => a.LeadProcessFlowId == id).ToList();
            if (lpft != null)
            {
                foreach (var lp in lpft)
                {
                    var lpfst = db.LeadProcessFlowAssignTypes.Where(a => a.LeadProcessFlowId == lp.LeadProcessFlowId).FirstOrDefault();
                    if (lpfst != null)
                    {
                        db.LeadProcessFlowAssignTypes.RemoveRange(db.LeadProcessFlowAssignTypes.Where(a => a.LeadProcessFlowId == lp.LeadProcessFlowId));
                        db.SaveChanges();
                    }

                }

            }

            var lpf = db.LeadApprovals.Where(a => a.LeadProcessFlowId == id).ToList();
            if(lpf!=null)
            {
                foreach(var lp in lpf)
                {
                    var lpfs = db.LeadApprovals.Where(a => a.LeadProcessFlowId == lp.LeadProcessFlowId).FirstOrDefault();
                    if(lpfs!=null)
                    {
                        db.LeadApprovals.RemoveRange(db.LeadApprovals.Where(a => a.LeadProcessFlowId == lp.LeadProcessFlowId));
                        db.SaveChanges();
                    }

                }
              
            }

            db.LeadProcessFlows.RemoveRange(db.LeadProcessFlows.Where(a => a.LeadProcessFlowId == id));
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "LeadProcessFlow", "LeadProcessFlows", findip(), pf.LeadProcessFlowId, "Lead Process Flow Deleted Successfully");
            stat = true;
            msg = "Successfully Deleted Lead Process Flow details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
    }
}
