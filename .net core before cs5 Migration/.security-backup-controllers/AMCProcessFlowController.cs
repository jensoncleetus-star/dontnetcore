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
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;


namespace QuickSoft.Controllers
{
    public class AMCProcessFlowController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AMCProcessFlowController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: AmcProcessFlow
        public ActionResult Index()
        {
            return View();
        }

        //INDEX--Function
        [RedirectingAction]
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

            var UserView = (from a in db.AmcProcessFlows
                            join b in db.AmcStatuss on a.AmcStatus equals b.AmcStatusId into tmp
                            from b in tmp.DefaultIfEmpty()
                            select new
                            {
                                a.AmcProcessFlowId,
                                a.RemoveUpdateUser,
                                a.RemoveUpdateUserTeams,
                                AssignType = (from z in db.AmcProcessFlowAssignTypes
                                              join w in db.Teams on z.TeamId equals w.TeamId
                                              join y in db.Employees on w.TeamLead equals y.EmployeeId
                                              where z.AmcProcessFlowId == a.AmcProcessFlowId
                                              select new
                                              {
                                                  w.TeamName,
                                              }).ToList(),
                               
                                AssignedUsers = (from z in db.AmcProcessFlowAssignUsers
                                                 join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                                 where z.AmcProcessFlowId == a.AmcProcessFlowId
                                                 select new
                                                 {
                                                     id = y.EmployeeId,
                                                     LastName = (y.LastName != null) ? y.LastName : "",
                                                     FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                     MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                     Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                     y.Status
                                                 }).ToList(),
                                AmcStatus = b.StatusName,
                            }).ToList().Select(o => new
                            {
                                id = o.AmcProcessFlowId,
                                o.AssignType,
                                o.RemoveUpdateUser,
                                o.RemoveUpdateUserTeams,
                                o.AssignedUsers,
                                o.AmcStatus,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.AmcStatus.ToString().ToLower().Contains(search.ToLower()));
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

        //CREATE ---GET
        [RedirectingAction]
        public ActionResult Create(long? id)
        {
            ViewBag.AssignTypes = QkSelect.List(
                                           new List<SelectListItem>
                                           {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                           }, "Value", "Text", 0);


            var AmcStat = db.AmcStatuss
                  .Select(s => new
                  {
                      ID = s.AmcStatusId,
                      Name = s.StatusName
                  })
                  .ToList();
            ViewBag.AmcStat = QkSelect.List(AmcStat, "ID", "Name");          


            var use = db.Employees
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            return PartialView();
        }
        //Create--POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(AMCProcessFlowViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var statusExists = db.AmcProcessFlows.Any(u => u.AmcStatus == vmodel.AmcStatus);
            if (statusExists)
            {
                msg = "A Process Flow with same Amc Status exists.";
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

                    AmcProcessFlow Obj = new AmcProcessFlow();
                    Obj.AmcStatus = vmodel.AmcStatus;
                    Obj.RemoveUpdateUser = vmodel.RemoveUpdateUser;
                    Obj.RemoveUpdateUserTeams = vmodel.RemoveUpdateUserTeams;
                    Obj.CreatedDate = today;
                    Obj.CreatedBy = UserId;
                    Obj.Status = Status.active;
                    Obj.Branch = BranchID;

                    db.AmcProcessFlows.Add(Obj);
                    db.SaveChanges();
                    Int64 pfId = Obj.AmcProcessFlowId;

                    if (vmodel.AssignTypeAll != null)
                    {
                        AmcProcessFlowAssignType AssgnTypeObj = new AmcProcessFlowAssignType();
                        
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            AssgnTypeObj.AmcProcessFlowId = pfId;
                            AssgnTypeObj.TeamId = arr;
                            db.AmcProcessFlowAssignTypes.Add(AssgnTypeObj);
                            db.SaveChanges();
                        }
                    }
                    
                    //Assigned Members
                    if (vmodel.AssignedUsers != null)
                    {
                        IList<AmcProcessFlowAssignUser> AssgnUserObj = new List<AmcProcessFlowAssignUser>();
                        foreach (var arr in vmodel.AssignedUsers)
                        {
                            AssgnUserObj.Add(new AmcProcessFlowAssignUser() { AmcProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (AssgnUserObj != null)
                        {
                            db.AmcProcessFlowAssignUsers.AddRange(AssgnUserObj);
                            db.SaveChanges();
                        }
                    }
                   
                    com.addlog(LogTypes.Created, UserId, "AMCProcessFlow", "AmcProcessFlows", findip(), Obj.AmcProcessFlowId, "Process Flow Added Successfully");
                    msg = "Successfully added Amc Process Flow details.";
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

        //Edit--GET
        [RedirectingAction]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AmcProcessFlow Obj = db.AmcProcessFlows.Find(id);

            if (Obj == null)
            {
                return NotFound();
            }
            AMCProcessFlowViewModel vmodel = new AMCProcessFlowViewModel();
            
            vmodel.AmcProcessFlowId         = Obj.AmcProcessFlowId;
            vmodel.AmcStatus                = Obj.AmcStatus;
            vmodel.RemoveUpdateUser         = Obj.RemoveUpdateUser;
            vmodel.RemoveUpdateUserTeams    = Obj.RemoveUpdateUserTeams;

            var AmcStatuss = db.AmcStatuss
                .Select(s => new
                {
                    ID      =   s.AmcStatusId,
                    Name    =   s.StatusName
                })
                .ToList();
            ViewBag.AmcStat = QkSelect.List(AmcStatuss, "ID", "Name");

            var atype = db.AmcProcessFlowAssignTypes.Where(a => a.AmcProcessFlowId == id).Select(a => a.TeamId).ToArray();
            var asstype = db.Teams
              .Select(s => new
              {
                  ID = s.TeamId,
                  Name = s.TeamName
              })
              .ToList();
            ViewBag.AssignTypes = new MultiSelectList(asstype, "ID", "Name", atype);

            vmodel.AssignedUsers = db.AmcProcessFlowAssignUsers.Where(a => a.AmcProcessFlowId == id).Select(a => a.EmployeeId).ToList().ToArray() ?? null;
           
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.team = new MultiSelectList(use, "ID", "Name", vmodel.AssignedUsers);
           
            return PartialView(vmodel);
        }

        //Edit--POST
        [RedirectingAction]
        [HttpPost]
        public JsonResult Edit(long? id, AMCProcessFlowViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var statusExists = db.AmcProcessFlows.Any(u => u.AmcStatus == vmodel.AmcStatus && u.AmcProcessFlowId != id);
            if (statusExists)
            {
                msg = "A Process Flow with same Amc Status exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    AmcProcessFlow pf = db.AmcProcessFlows.Find(id);

                    pf.AmcStatus = vmodel.AmcStatus;

                    pf.RemoveUpdateUser = vmodel.RemoveUpdateUser;
                    pf.RemoveUpdateUserTeams = vmodel.RemoveUpdateUserTeams;
                    db.Entry(pf).State = EntityState.Modified;
                    db.SaveChanges();

                    Int64 pfId = pf.AmcProcessFlowId;

                    var pftype = db.AmcProcessFlowAssignTypes.Where(a => a.AmcProcessFlowId == pfId);
                    if (pftype != null)
                    {
                        db.AmcProcessFlowAssignTypes.RemoveRange(db.AmcProcessFlowAssignTypes.Where(a => a.AmcProcessFlowId == pfId));
                        db.SaveChanges();
                    }

                    if (vmodel.AssignTypeAll != null)
                    {
                        AmcProcessFlowAssignType AssgnType = new AmcProcessFlowAssignType();
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            AssgnType.AmcProcessFlowId = pfId;
                            AssgnType.TeamId = arr;
                            db.AmcProcessFlowAssignTypes.Add(AssgnType);
                            db.SaveChanges();
                        }
                    }

                    var teammembers = db.AmcProcessFlowAssignUsers.Where(a => a.AmcProcessFlowId == pfId);
                    if (teammembers != null)
                    {
                        db.AmcProcessFlowAssignUsers.RemoveRange(db.AmcProcessFlowAssignUsers.Where(a => a.AmcProcessFlowId == pfId));
                        db.SaveChanges();
                    }
                    if (vmodel.AssignedUsers != null)
                    {
                        IList<AmcProcessFlowAssignUser> AmcAssgndUsers = new List<AmcProcessFlowAssignUser>();
                       
                        foreach (var arr in vmodel.AssignedUsers)
                        {
                            AmcAssgndUsers.Add(new AmcProcessFlowAssignUser() { AmcProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (AmcAssgndUsers != null)
                        {
                            db.AmcProcessFlowAssignUsers.AddRange(AmcAssgndUsers);
                            db.SaveChanges();
                        }
                    }
                   
                    com.addlog(LogTypes.Updated, UserId, "AMCProcessFlow", "AmcProcessFlows", findip(), pfId, "Process Flow Updated Successfully");
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
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AmcProcessFlow pf = db.AmcProcessFlows.Find(id);
            if (pf == null)
            {
                return NotFound();
            }
            return PartialView(pf);
        }

        // POST: /Delete/5
        [RedirectingAction]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            AmcProcessFlow pf = db.AmcProcessFlows.Find(id);

            var pfuser = db.AmcProcessFlowAssignUsers.Where(a => a.AmcProcessFlowId == id).ToList();
           

            foreach (var arr in pfuser)
            {
                var pfmember = db.AmcProcessFlowAssignUsers.Where(a => a.AmcProcessFlowId == arr.AmcProcessFlowId);
                if (pfmember != null)
                {
                    db.AmcProcessFlowAssignUsers.RemoveRange(db.AmcProcessFlowAssignUsers.Where(a => a.AmcProcessFlowId == arr.AmcProcessFlowId));
                    db.SaveChanges();
                }
            }
            db.AmcProcessFlows.RemoveRange(db.AmcProcessFlows.Where(a => a.AmcProcessFlowId == id));
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "AMCProcessFlow", "AmcProcessFlows", findip(), pf.AmcProcessFlowId, "Process Flow Deleted Successfully");
            stat = true;
            msg = "Successfully Deleted Process Flow details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
    }
}
