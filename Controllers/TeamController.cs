using QuickSoft.Web;
using System.Linq.Dynamic.Core;
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
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    public class TeamController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public TeamController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Team
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Task Team List")]
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        [Authorize(Roles = "Dev,Task Team List")]
        [HttpPost]
        public ActionResult GetTeam()
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

            var UserView = (from a in db.Teams
                            join b in db.Employees on a.TeamLead equals b.EmployeeId into emp
                            from b in emp.DefaultIfEmpty()
                            join c in db.Users on a.CreatedBy equals c.Id
                            select new
                            {
                                a.TeamId,
                                c.UserName,
                                a.TeamName,
                                a.TeamTag,
                                TeamLead = b.FirstName + " " + b.LastName,
                                TeamMembers = (from z in db.TeamMembers
                                               join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                               where z.TeamId == a.TeamId
                                               select new
                                               {
                                                   id = y.EmployeeId,
                                                   LastName = (y.LastName != null) ? y.LastName : "",
                                                   FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                   y.Status
                                               }).ToList(),
                                TaskStatus = (from z in db.TeamTaskStatus
                                              join y in db.TaskStatus on z.TaskStatusId equals y.TaskStatusId
                                              where z.TeamId == a.TeamId
                                              select new
                                              {
                                                  Id = y.TaskStatusId,
                                                  Name = y.StatusName
                                              }).ToList(),
                                LeadStatus = (from z in db.LeadTaskStatus
                                              join y in db.LeadStatuss on z.TaskStatusId equals y.LeadStatusID
                                              where z.TeamId == a.TeamId
                                              select new
                                              {
                                                  Id = y.LeadStatusID,
                                                  Name = y.StatusType
                                              }).ToList(),
                              AmcStatus = (from z in db.TeamAmcStatus
                                           join y in db.AmcStatuss on z.amcStatusId equals y.AmcStatusId
                                            where z.TeamId == a.TeamId
                                            select new
                                            {
                                                Id = y.AmcStatusId,
                                                Name = y.StatusName
                                            }).ToList()

                            }).ToList().Select(o => new {
                                id = o.TeamId,
                                o.TeamName,
                                o.TeamTag,
                                o.UserName,
                                o.TeamLead,
                                o.TeamMembers,
                                o.TaskStatus,
                                o.LeadStatus,
                                o.AmcStatus
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.TeamLead.ToString().ToLower().Contains(search.ToLower()) ||
                                               p.TeamName.ToString().ToLower().Contains(search.ToLower()) ||
                                               p.TeamMembers.ToString().ToLower().Contains(search.ToLower()));
                                             

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
        [QkAuthorize(Roles = "Dev,Create Task Team")]
        public ActionResult Create(long? id, long? type)
        {
            var use = (from c in db.Employees 
                        join d in db.Users on c.UserId equals d.Id into usr
                         from d in usr.DefaultIfEmpty()
                         where d.Status==1
                          
                  select new
                  {
                      ID = c.EmployeeId,
                      Name = c.FirstName + " " + c.LastName
                  })
                  .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            var tag = db.Teams
                 .Select(s => new
                 {
                     ID = s.TeamTag,
                     Name = s.TeamTag
                 }).Distinct().ToList();
            ViewBag.TagTeam = QkSelect.List(tag, "ID", "Name");

            var tsks = db.TaskStatus
                  .Select(s => new
                  {
                      ID = s.TaskStatusId,
                      Name = s.StatusName
                  }).ToList();
            ViewBag.TaskStat = QkSelect.List(tsks, "ID", "Name");
            var amcs = db.AmcStatuss
                .Select(s => new
                {
                    ID = s.AmcStatusId,
                    Name = s.StatusName
                }).ToList();
            ViewBag.amcstat = QkSelect.List(amcs, "ID", "Name");



            var leadstatus = db.LeadStatuss.Select(s => new SelectFormat
            {
                id = s.LeadStatusID,
                text = s.StatusType
            }).ToList();
            leadstatus.Insert(0, new SelectFormat { id = 500, text = "Lead Approved" });




            ViewBag.leadstatus = QkSelect.List(leadstatus, "id", "text");




            return PartialView();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Task Team")]
        [ValidateAntiForgeryToken]
        public JsonResult Create(TeamViewModel vmodel)
        {
            bool stat = false;
            string msg;
           
            if (ModelState.IsValid)
            {
                var Exists = db.Teams.Any(c => c.TeamLead == vmodel.TeamLead);
                if (Exists)
                {
                    msg = "Team with The Same TeamLead already exists.";
                    stat = false;
                }

                var ExistsTeam = db.Teams.Any(c => c.TeamName == vmodel.TeamName);
                if (ExistsTeam)
                {
                    msg = "Team with The Same TeamName already exists.";
                    stat = false;
                }

                else
                {

                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    var pro = new Team
                    {
                        TeamLead = vmodel.TeamLead,
                        CreatedDate = today,
                        CreatedBy = UserId,
                        TeamName = vmodel.TeamName,
                        TeamTag = vmodel.TeamTag,
                        Status = Status.active,
                        Branch = BranchID
                    };
                    db.Teams.Add(pro);
                    db.SaveChanges();
                    Int64 teamId = pro.TeamId;

                    //team members
                    if (vmodel.TeamMembers != null)
                    {
                        IList<TeamMember> teammeb = new List<TeamMember>();
                        foreach (var arr in vmodel.TeamMembers)
                        {
                            if (arr != vmodel.TeamLead)
                            {
                                teammeb.Add(new TeamMember() { TeamId = (long)teamId, EmployeeId = arr });
                            }
                        }
                        if (teammeb != null)
                        {
                            db.TeamMembers.AddRange(teammeb);
                            db.SaveChanges();
                        }
                    }

                    //task status
                    if (vmodel.TaskStatus != null)
                    {
                        IList<TeamTaskStatus> tskstat = new List<TeamTaskStatus>();
                        foreach (var arr in vmodel.TaskStatus)
                        {
                            tskstat.Add(new TeamTaskStatus() { TeamId = (long)teamId, TaskStatusId = (long)arr });
                        }
                        if (tskstat != null)
                        {
                            db.TeamTaskStatus.AddRange(tskstat);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.LeadStatus != null)
                    {
                        IList<LeadTaskStatus> leadstatus = new List<LeadTaskStatus>();
                        foreach (var arr in vmodel.LeadStatus)
                        {
                            leadstatus.Add(new LeadTaskStatus() { TeamId = (long)teamId, TaskStatusId = (long)arr });
                        }
                        if (leadstatus != null)
                        {
                            db.LeadTaskStatus.AddRange(leadstatus);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.amcstatus != null)
                    {
                        IList<TeamAmcStatus> teamamcstatuss = new List<TeamAmcStatus>();
                        foreach (var arr in vmodel.amcstatus)
                        {
                            teamamcstatuss.Add(new TeamAmcStatus() { TeamId = (long)teamId, amcStatusId = (long)arr });
                        }
                        if (teamamcstatuss != null)
                        {
                            db.TeamAmcStatus.AddRange(teamamcstatuss);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Created, UserId, "Team", "Teams", findip(), pro.TeamId, "Task Team Added Successfully");
                    msg = "Successfully added Task Team details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Task Team")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team pro = db.Teams.Find(id);

            if (pro == null)
            {
                return NotFound();
            }
            TeamViewModel vmodel = new TeamViewModel();
            var Team = db.Teams.Where(a => a.TeamId == id).OrderBy(a => a.TeamId).Take(1).SingleOrDefault();

            vmodel.TeamLead = pro.TeamLead;
            vmodel.TeamName = pro.TeamName;
            vmodel.TeamTag = pro.TeamTag;

            vmodel.TeamMembers = (Team != null) ? db.TeamMembers.Where(a => a.TeamId == Team.TeamId).Select(a => a.EmployeeId).ToList().ToArray() : null;

            var use = (from c in db.Employees
                         join d in db.Users on c.UserId equals d.Id into usr
                         from d in usr.DefaultIfEmpty()
                         where d.Status == 1

                         select new
                         {
                             ID = c.EmployeeId,
                             Name = c.FirstName + " " + c.LastName
                         })
                  .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");
            ViewBag.team = new MultiSelectList(use, "ID", "Name", vmodel.TeamMembers);

            var tag = db.Teams
                     .Select(s => new
                     {
                         ID = s.TeamTag,
                         Name = s.TeamTag
                     }).Distinct().ToList();
            ViewBag.TagTeam = QkSelect.List(tag, "ID", "Name");

            if (Team != null)
            {
                vmodel.TaskStatus = (Team != null) ? db.TeamTaskStatus.Where(a => a.TeamId == Team.TeamId).Select(a => a.TaskStatusId).ToList().ToArray() : null;
                vmodel.LeadStatus = (Team != null) ? db.LeadTaskStatus.Where(a => a.TeamId == Team.TeamId).Select(a => a.TaskStatusId).ToList().ToArray() : null;
                vmodel.amcstatus = (Team != null) ? db.TeamAmcStatus.Where(a => a.TeamId == Team.TeamId).Select(a => a.amcStatusId).ToList().ToArray() : null;


            }
            if (vmodel.LeadStatus.Count()>0)
            {
                vmodel.CustomerRelation = true;
            }
            if (vmodel.amcstatus.Count() > 0)
            {
                vmodel.CustomerRelation = false;
                vmodel.amcteam = true;
            }
            var tskstat = db.TaskStatus.Select(s => new { ID = s.TaskStatusId, Name = s.StatusName }).ToList();
            ViewBag.TaskStat = new MultiSelectList(tskstat, "ID", "Name", vmodel.TaskStatus);

            var leadstatus = db.LeadStatuss.Select(s => new SelectFormat
            {
                id = s.LeadStatusID,
                text = s.StatusType
            }).ToList();
            leadstatus.Insert(0, new SelectFormat { id = 500, text = "Lead Approved" });
            var amcs = db.AmcStatuss
                            .Select(s => new
                            {
                                ID = s.AmcStatusId,
                                Name = s.StatusName
                            }).ToList();
            ViewBag.amcstat = QkSelect.List(amcs, "ID", "Name");

            ViewBag.leadstat = new MultiSelectList(leadstatus, "id", "text", vmodel.LeadStatus);
            return PartialView(vmodel);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Task Team")]
        [HttpPost]
        public JsonResult Edit(long? id, TeamViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {

                    var UserId = User.Identity.GetUserId();
                    Team pro = db.Teams.Find(id);

                    pro.TeamLead = vmodel.TeamLead;
                    pro.TeamName = vmodel.TeamName;
                    pro.TeamTag = vmodel.TeamTag;

                    db.Entry(pro).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 teamId = pro.TeamId;

                    var teammembers = db.TeamMembers.Where(a => a.TeamId == teamId);
                    if (teammembers != null)
                    {
                        db.TeamMembers.RemoveRange(db.TeamMembers.Where(a => a.TeamId == teamId));
                        db.SaveChanges();
                    }

                    //team members
                    if (vmodel.TeamMembers != null)
                    {
                        IList<TeamMember> teammeb = new List<TeamMember>();
                        foreach (var arr in vmodel.TeamMembers)
                        {
                            if (arr != vmodel.TeamLead)
                            {
                                teammeb.Add(new TeamMember() { TeamId = (long)teamId, EmployeeId = arr });
                            }
                        }
                        if (teammeb != null)
                        {
                            db.TeamMembers.AddRange(teammeb);
                            db.SaveChanges();
                        }
                    }
                var tskstatusamc = db.TeamAmcStatus.Where(a => a.TeamId == teamId);
                if (tskstatusamc != null)
                {
                    db.TeamAmcStatus.RemoveRange(db.TeamAmcStatus.Where(a => a.TeamId == teamId));
                    db.SaveChanges();
                }
                //task status
                if (vmodel.amcstatus != null)
                {
                    IList<TeamAmcStatus> tskstat = new List<TeamAmcStatus>();
                    foreach (var arr in vmodel.amcstatus)
                    {
                        tskstat.Add(new TeamAmcStatus() { TeamId = (long)teamId, amcStatusId = arr });
                    }
                    if (tskstat != null)
                    {
                        db.TeamAmcStatus.AddRange(tskstat);
                        db.SaveChanges();
                    }
                }
                var tskstatus = db.TeamTaskStatus.Where(a => a.TeamId == teamId);
                    if (tskstatus != null)
                    {
                        db.TeamTaskStatus.RemoveRange(db.TeamTaskStatus.Where(a => a.TeamId == teamId));
                        db.SaveChanges();
                    }
                    //task status
                    if (vmodel.TaskStatus != null)
                    {
                        IList<TeamTaskStatus> tskstat = new List<TeamTaskStatus>();
                        foreach (var arr in vmodel.TaskStatus)
                        {
                            tskstat.Add(new TeamTaskStatus() { TeamId = (long)teamId, TaskStatusId = arr });
                        }
                        if (tskstat != null)
                        {
                            db.TeamTaskStatus.AddRange(tskstat);
                            db.SaveChanges();
                        }
                    }
                    var leadsatus = db.LeadTaskStatus.Where(a => a.TeamId == teamId);
                    if (leadsatus != null)
                    {
                        db.LeadTaskStatus.RemoveRange(db.LeadTaskStatus.Where(a => a.TeamId == teamId));
                        db.SaveChanges();
                    }
                    if (vmodel.LeadStatus != null)
                    {
                        IList<LeadTaskStatus> leadstatus = new List<LeadTaskStatus>();
                        foreach (var arr in vmodel.LeadStatus)
                        {
                            leadstatus.Add(new LeadTaskStatus() { TeamId = (long)teamId, TaskStatusId = (long)arr });
                        }
                        if (leadstatus != null)
                        {
                            db.LeadTaskStatus.AddRange(leadstatus);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Updated, UserId, "Team", "Teams", findip(), pro.TeamId, "Task Team Updated Successfully");
                    msg = "Successfully Updated Task Team details.";
                    stat = true;
                
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        // GET: /Delete/5
        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete Task Team")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Team pro = db.Teams.Find(id);
            if (pro == null)
            {
                return NotFound();
            }
            return PartialView(pro);
        }

        // POST: /Delete/5

        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete Task Team")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
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
                msg = "Successfully Deleted Task Teams details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            Team pro = db.Teams.Find(id);
            if (pro != null)
            {
                var tstatus = db.TeamTaskStatus.Where(a => a.TeamId == pro.TeamId);
                if (tstatus != null)
                {
                    db.TeamTaskStatus.RemoveRange(db.TeamTaskStatus.Where(a => a.TeamId == pro.TeamId));
                    db.SaveChanges();
                }

                var team = db.Teams.Where(a => a.TeamId == id).ToList();
                if (team.Any())
                {
                    foreach (var arr in team)
                    {
                        var tmember = db.TeamMembers.Where(a => a.TeamId == arr.TeamId);
                        if (tmember != null)
                        {
                            db.TeamMembers.RemoveRange(db.TeamMembers.Where(a => a.TeamId == arr.TeamId));
                            db.SaveChanges();
                        }
                    }
                    db.Teams.RemoveRange(db.Teams.Where(a => a.TeamId == id));
                    db.SaveChanges();
                }

                com.addlog(LogTypes.Deleted, UserId, "Team", "Teamss", findip(), pro.TeamId, "Task Teams Deleted Successfully");
            }                return true;
        }
        public string chkDeleteWithMsg(long id)
            {
                string msg = null;
                if (db.ProcessFlowAssignTypes.Any(c => c.TeamId == id))
                {
                    msg = null;
                }
                else if (db.TaskAssignTypes.Any(c => c.TeamId == id))
                {
                    msg = "Project Already used in Task !!";
                }
                else
                {
                    msg = null;
                }
           
            return msg;
        }

        //[HttpGet]
        //                       where a.TeamId == Team.TeamId
        //                           a.EmployeeId


        [HttpGet]
        public JsonResult GetAllMembers(long[] assign)
        {
            var teamss = (from a in db.Teams
                          join b in db.TeamMembers on a.TeamId equals b.TeamId into teams
                          from b in teams.DefaultIfEmpty()
                         where assign.Contains(a.TeamId) 
                          select new
                          {
                              emp = (b.EmployeeId != null)? b.EmployeeId : 0,
                              lead = a.TeamLead,
                          }).ToList();
            return Json(teamss);
        }

        public JsonResult SearchTeam(string q, string x)
        {

            var UserId = User.Identity.GetUserId();
            List<SelectFormat> serialisedJson;
            string stt = "Individual";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Teams
                                  join b in db.Employees on a.TeamLead equals b.EmployeeId into teams
                                  from b in teams.DefaultIfEmpty()
                                  where a.TeamName.Contains(q) || a.TeamName.ToLower().Contains(q.ToLower()) //|| b.FirstName.ToLower().Contains(q.ToLower()) || b.LastName.ToLower().Contains(q.ToLower()) || b.FirstName.Contains(q) || b.LastName.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.TeamName, //db.Employees.Where(c => c.EmployeeId == a.TeamLead).Select(c => c.FirstName + " "+ c.LastName).FirstOrDefault(), //each json object will have 
                                      id = a.TeamId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Teams.Select(b => new SelectFormat
                {
                    text = b.TeamName,// db.Employees.Where(a => a.EmployeeId == b.TeamLead).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault(),//each json object will have 
                    id = b.TeamId
                }).OrderBy(b => b.text).ToList();

            }//
            return Json(serialisedJson);
        }
    }
}
