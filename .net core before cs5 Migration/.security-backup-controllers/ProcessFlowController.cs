using QuickSoft.Web;
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
    public class ProcessFlowController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ProcessFlowController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: ProcessFlow
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,ProcessFlow List")]
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        [Authorize(Roles = "Dev,ProcessFlow List")]
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

            // EF Core 10 cannot translate the two inline nested .ToList() collection sub-projections
            // (AssignType / AssignedUsers) inside a server-side outer projection (was 500). Split into
            // scalar base rows + two bulk child lookups, then build the final shape client-side. This
            // mirrors the EF6 per-row N+1 behavior and is output-identical.
            var baseRows = (from a in db.ProcessFlows
                            join b in db.TaskStatus on a.TaskStatus equals b.TaskStatusId into tmp
                            from b in tmp.DefaultIfEmpty()
                            select new
                            {
                                a.ProcessFlowId,
                                a.RemoveUpdateUser,
                                a.RemoveUpdateUserTeams,
                                TaskStatus = b.StatusName,
                            }).ToList();

            var ids = baseRows.Select(r => r.ProcessFlowId).ToList();

            var assignTypeLookup = (from z in db.ProcessFlowAssignTypes
                                    join w in db.Teams on z.TeamId equals w.TeamId
                                    join y in db.Employees on w.TeamLead equals y.EmployeeId
                                    where ids.Contains(z.ProcessFlowId)
                                    select new
                                    {
                                        z.ProcessFlowId,
                                        w.TeamName,
                                    })
                                   .ToList()
                                   .GroupBy(x => x.ProcessFlowId)
                                   .ToDictionary(g => g.Key, g => g.Select(x => new { x.TeamName }).ToList());

            var assignedUsersLookup = (from z in db.ProcessFlowAssignUsers
                                       join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                       where ids.Contains(z.ProcessFlowId)
                                       select new
                                       {
                                           z.ProcessFlowId,
                                           id = y.EmployeeId,
                                           LastName = (y.LastName != null) ? y.LastName : "",
                                           FirstName = (y.FirstName != null) ? y.FirstName : "",
                                           MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                           Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                           y.Status
                                       })
                                      .ToList()
                                      .GroupBy(x => x.ProcessFlowId)
                                      .ToDictionary(g => g.Key, g => g.Select(x => new { x.id, x.LastName, x.FirstName, x.MiddleName, x.Img, x.Status }).ToList());

            var emptyAssignType = Enumerable.Empty<object>().Select(x => new { TeamName = (string)null }).ToList();
            var emptyAssignedUsers = Enumerable.Empty<object>().Select(x => new { id = default(long), LastName = "", FirstName = "", MiddleName = "", Img = "", Status = default(int) }).ToList();

            var UserView = baseRows.Select(o => new
            {
                id = o.ProcessFlowId,
                AssignType = assignTypeLookup.TryGetValue(o.ProcessFlowId, out var at) ? at : emptyAssignType,
                o.RemoveUpdateUser,
                o.RemoveUpdateUserTeams,
                AssignedUsers = assignedUsersLookup.TryGetValue(o.ProcessFlowId, out var au) ? au : emptyAssignedUsers,
                o.TaskStatus,
            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.TaskStatus.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create ProcessFlow")]
        public ActionResult Create(long? id)
        {
            ViewBag.AssignTypes = QkSelect.List(
                                           new List<SelectListItem>
                                           {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                           }, "Value", "Text", 0);

           
            var tsks = db.TaskStatus
                  .Select(s => new
                  {
                      ID = s.TaskStatusId,
                      Name = s.StatusName
                  })
                  .ToList();
            ViewBag.TaskStat = QkSelect.List(tsks, "ID", "Name");
            var taskgroup = db.ProTaskTypes
                  .Select(s => new
                  {
                      ID = s.TaskTypeId,
                      Name = s.TypeName
                  })
                  .ToList();
            ViewBag.TaskGroups = QkSelect.List(taskgroup, "ID", "Name");

            var ls = db.LeadStatuss
                  .Select(s => new
                  {
                      ID = s.LeadStatusID,
                      Name = s.StatusType
                  })
                  .ToList();
            ViewBag.LeadStatus = QkSelect.List(ls, "ID", "Name"); 

           

            var use = db.Employees
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");
            ViewBag.leadAssignUsers = QkSelect.List(use, "ID", "Name");
            return PartialView();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create ProcessFlow")]
        [ValidateAntiForgeryToken]
        public JsonResult Create(ProcessFlowViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var statusExists = db.ProcessFlows.Any(u => u.TaskStatus == vmodel.TaskStatus);
            if (statusExists)
            {
                msg = "An Process Flow with same Task Status exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        var s = error.ErrorMessage;
                    }
                }
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    ProcessFlow pf = new ProcessFlow();
                    pf.TaskStatus = vmodel.TaskStatus;
                    pf.RemoveUpdateUser = vmodel.RemoveUpdateUser;
                    pf.RemoveUpdateUserTeams = vmodel.RemoveUpdateUserTeams;

                    pf.CreatedDate = today;
                    pf.CreatedBy = UserId;
                    pf.Status = Status.active;
                    pf.Branch = BranchID;
                    pf.MoveToLead = vmodel.MoveToLead;
                    if(vmodel.LeadStatus!=null)
                    pf.LeadStatus = (long)vmodel.LeadStatus;
                    pf.assignexistinguser = vmodel.assignexistinguser;

                    db.ProcessFlows.Add(pf);
                    db.SaveChanges();
                    Int64 pfId = pf.ProcessFlowId;

                    if (vmodel.AssignTypeAll != null)
                    {
                        ProcessFlowAssignType tskax = new ProcessFlowAssignType();
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            tskax.ProcessFlowId = pfId;
                            tskax.TeamId = arr;
                            db.ProcessFlowAssignTypes.Add(tskax);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.leadAssignUsers != null)
                    {
                        ////TaskTeamMember team = new TaskTeamMember();
                    }
                    //assigned members
                    if (vmodel.AssignedUsers != null)
                    {
                        IList<ProcessFlowAssignUser> tskstat = new List<ProcessFlowAssignUser>();
                        foreach (var arr in vmodel.AssignedUsers)
                        {
                            tskstat.Add(new ProcessFlowAssignUser() { ProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (tskstat != null)
                        {
                            db.ProcessFlowAssignUsers.AddRange(tskstat);
                            db.SaveChanges();
                        }
                    }
                    if(vmodel.leadAssignUsers != null)
                    {
                        IList<ProcessFlowAssignUserstolead> tskstat1 = new List<ProcessFlowAssignUserstolead>();
                        foreach (var arr in vmodel.leadAssignUsers)
                        {
                            tskstat1.Add(new ProcessFlowAssignUserstolead() { ProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (tskstat1 != null)
                        {
                            db.ProcessFlowAssignUserstolead.AddRange(tskstat1);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Created, UserId, "ProcessFlow", "ProcessFlows", findip(), pf.ProcessFlowId, "Process Flow Added Successfully");
                    msg = "Successfully added Process Flow details.";
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
        [QkAuthorize(Roles = "Dev,Edit ProcessFlow")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProcessFlow teamaa = db.ProcessFlows.Find(id);

            if (teamaa == null)
            {
                return NotFound();
            }
            ProcessFlowViewModel vmodel = new ProcessFlowViewModel();

            vmodel.ProcessFlowId = teamaa.ProcessFlowId;
            vmodel.TaskStatus = teamaa.TaskStatus;
            vmodel.RemoveUpdateUser = teamaa.RemoveUpdateUser;
            vmodel.RemoveUpdateUserTeams = teamaa.RemoveUpdateUserTeams;
            vmodel.MoveToLead = teamaa.MoveToLead;
            vmodel.LeadStatus = teamaa.LeadStatus;
            vmodel.assignexistinguser = teamaa.assignexistinguser;
            var tsks = db.TaskStatus
                .Select(s => new
                {
                    ID = s.TaskStatusId,
                    Name = s.StatusName
                })
                .ToList();
            ViewBag.TaskStat = QkSelect.List(tsks, "ID", "Name");



            var atype = db.ProcessFlowAssignTypes.Where(a => a.ProcessFlowId == id).Select(a => a.TeamId).ToArray();
            var asstype = db.Teams
              .Select(s => new
              {
                  ID = s.TeamId,
                  Name = s.TeamName
              })
              .ToList();
            ViewBag.AssignTypes = new MultiSelectList(asstype, "ID", "Name", atype);


            vmodel.AssignedUsers = db.ProcessFlowAssignUsers.Where(a => a.ProcessFlowId == id).Select(a => a.EmployeeId).ToList().ToArray() ?? null;
            var ltsks = db.LeadStatuss
                 .Select(s => new KeyValue
                 {
                     ID = s.LeadStatusID,
                     Name = s.StatusType
                 })
                 .ToList();



            ViewBag.LeadStatuss = QkSelect.List(ltsks, "ID", "Name"); 
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.team = new MultiSelectList(use, "ID", "Name", vmodel.AssignedUsers);
            vmodel.leadAssignUsers = db.ProcessFlowAssignUserstolead.Where(a => a.ProcessFlowId == id).Select(a => a.EmployeeId).ToList().ToArray() ?? null;

            ViewBag.leadAssignUserss = new MultiSelectList(use, "ID", "Name", vmodel.leadAssignUsers);



            return PartialView(vmodel);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit ProcessFlow")]
        [HttpPost]
        public JsonResult Edit(long? id, ProcessFlowViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var statusExists = db.ProcessFlows.Any(u => u.TaskStatus == vmodel.TaskStatus && u.ProcessFlowId != id);
            if (statusExists)
            {
                msg = "An Process Flow with same Task Status exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    ProcessFlow pf = db.ProcessFlows.Find(id);

                    pf.TaskStatus = vmodel.TaskStatus;
                  
                    pf.RemoveUpdateUser = vmodel.RemoveUpdateUser;
                    pf.RemoveUpdateUserTeams = vmodel.RemoveUpdateUserTeams;
                    pf.MoveToLead = vmodel.MoveToLead;
                    if(vmodel.LeadStatus!=null)
                    pf.LeadStatus = (long)vmodel.LeadStatus;
                    pf.assignexistinguser = vmodel.assignexistinguser;
                    db.Entry(pf).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 pfId = pf.ProcessFlowId;

                    var pftype = db.ProcessFlowAssignTypes.Where(a => a.ProcessFlowId == pfId);
                    if (pftype != null)
                    {
                        db.ProcessFlowAssignTypes.RemoveRange(db.ProcessFlowAssignTypes.Where(a => a.ProcessFlowId == pfId));
                        db.SaveChanges();
                    }
                    if (vmodel.AssignTypeAll != null)
                    {
                        ProcessFlowAssignType tskax = new ProcessFlowAssignType();
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            tskax.ProcessFlowId = pfId;
                            tskax.TeamId = arr;
                            db.ProcessFlowAssignTypes.Add(tskax);
                            db.SaveChanges();
                        }
                    }


                    var teammembers = db.ProcessFlowAssignUsers.Where(a => a.ProcessFlowId == pfId);
                    if (teammembers != null)
                    {
                        db.ProcessFlowAssignUsers.RemoveRange(db.ProcessFlowAssignUsers.Where(a => a.ProcessFlowId == pfId));
                        db.SaveChanges();
                    }
                    if (vmodel.AssignedUsers != null)
                    {
                        IList<ProcessFlowAssignUser> tskstat = new List<ProcessFlowAssignUser>();
                        foreach (var arr in vmodel.AssignedUsers)
                        {
                            tskstat.Add(new ProcessFlowAssignUser() { ProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (tskstat != null)
                        {
                            db.ProcessFlowAssignUsers.AddRange(tskstat);
                            db.SaveChanges();
                        }
                    }
                    //assigned members
                    db.ProcessFlowAssignUserstolead.RemoveRange(db.ProcessFlowAssignUserstolead.Where(o => o.ProcessFlowId == pfId));
                      db.SaveChanges();

                    if (vmodel.leadAssignUsers != null)
                    {
                        IList<ProcessFlowAssignUserstolead> tskstat1 = new List<ProcessFlowAssignUserstolead>();
                        foreach (var arr in vmodel.leadAssignUsers)
                        {
                            tskstat1.Add(new ProcessFlowAssignUserstolead() { ProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (tskstat1 != null)
                        {
                            db.ProcessFlowAssignUserstolead.AddRange(tskstat1);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "ProcessFlow", "ProcessFlows", findip(), pfId, "Process Flow Updated Successfully");
                    msg = "Successfully Updated Process Flow details.";
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
        [Authorize(Roles = "Dev,Delete ProcessFlow")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProcessFlow pf = db.ProcessFlows.Find(id);
            if (pf == null)
            {
                return NotFound();
            }
            return PartialView(pf);
        }

        // POST: /Delete/5

        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete ProcessFlow")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            ProcessFlow pf = db.ProcessFlows.Find(id);

            var pfuser = db.ProcessFlowAssignUsers.Where(a => a.ProcessFlowId == id).ToList();
                foreach (var arr in pfuser)
                {
                    var pfmember = db.ProcessFlowAssignUsers.Where(a => a.ProcessFlowId == arr.ProcessFlowId);
                    if (pfmember != null)
                    {
                        db.ProcessFlowAssignUsers.RemoveRange(db.ProcessFlowAssignUsers.Where(a => a.ProcessFlowId == arr.ProcessFlowId));
                        db.SaveChanges();
                    }
                }
                db.ProcessFlows.RemoveRange(db.ProcessFlows.Where(a => a.ProcessFlowId == id));
                db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "ProcessFlow", "ProcessFlows", findip(), pf.ProcessFlowId, "Process Flow Deleted Successfully");
            stat = true;
            msg = "Successfully Deleted Process Flow details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
    }
}
