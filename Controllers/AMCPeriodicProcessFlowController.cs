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
    public class AMCPeriodicProcessFlowController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AMCPeriodicProcessFlowController()
        {
            db  = new ApplicationDbContext();
            com = new Common();
        }

        // GET: AMCPeriodicProcessFlow
        public ActionResult Index()
        {
            return View();
        }
        //INDEX--Function
        [RedirectingAction]
        [HttpPost]
        public ActionResult GetProcessFlow()
        {
            string search   =   Request.Form.GetValues("search[value]")[0];
            var draw        =   Request.Form.GetValues("draw").FirstOrDefault();
            var start       =   Request.Form.GetValues("start").FirstOrDefault();
            var length      =   Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn      =   Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir   =   Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var UserView = (from a in db.PeriodicProcessFlows
                            join b in db.AmcStatuss on a.PeriodicStatus equals b.AmcStatusId into tmp
                            from b in tmp.DefaultIfEmpty()
                            select new
                            {
                                a.PeriodicProcessFlowId,
                                a.RemoveUpdateUser,
                                a.RemoveUpdateUserTeams,
                                AssignType = (from z in db.PeriodicProcessFlowAssignTypes
                                              join w in db.Teams on z.TeamId equals w.TeamId
                                              join y in db.Employees on w.TeamLead equals y.EmployeeId
                                              where z.PerdcProcessFlowId == a.PeriodicProcessFlowId
                                              select new
                                              {
                                                  w.TeamName,
                                              }).ToList(),

                                AssignedUsers = (from z in db.PeriodicProcessFlowAssignUsers
                                                 join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                                 where z.PerdcProcessFlowId == a.PeriodicProcessFlowId
                                                 select new
                                                 {
                                                     id = y.EmployeeId,
                                                     LastName = (y.LastName != null) ? y.LastName : "",
                                                     FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                     MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                     Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                     y.Status
                                                 }).ToList(),
                                Status = b.StatusName,
                            }).ToList().Select(o => new
                            {
                                id = o.PeriodicProcessFlowId,
                                o.AssignType,
                                o.RemoveUpdateUser,
                                o.RemoveUpdateUserTeams,
                                o.AssignedUsers,
                                o.Status,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Status.ToString().ToLower().Contains(search.ToLower()));
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
                      ID    = s.AmcStatusId,
                      Name  = s.StatusName
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
            var statusExists = db.PeriodicProcessFlows.Any(u => u.PeriodicStatus == vmodel.PeriodicStatus);
            if (statusExists)
            {
                msg = "A Process Flow with same Periodic Status exists...";
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

                    PeriodicProcessFlow Obj = new PeriodicProcessFlow();
                    Obj.PeriodicStatus = vmodel.PeriodicStatus;
                    Obj.RemoveUpdateUser = vmodel.RemoveUpdateUser;
                    Obj.RemoveUpdateUserTeams = vmodel.RemoveUpdateUserTeams;
                    Obj.CreatedDate = today;
                    Obj.CreatedBy = UserId;
                    Obj.Status = Status.active;
                    Obj.Branch = BranchID;

                    db.PeriodicProcessFlows.Add(Obj);
                    db.SaveChanges();

                    Int64 pfId = Obj.PeriodicProcessFlowId;

                    if (vmodel.AssignTypeAll != null)
                    {
                        PeriodicProcessFlowAssignType AssgnTypeObj = new PeriodicProcessFlowAssignType();

                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            AssgnTypeObj.PerdcProcessFlowId = pfId;
                            AssgnTypeObj.TeamId = arr;
                            db.PeriodicProcessFlowAssignTypes.Add(AssgnTypeObj);
                            db.SaveChanges();
                        }
                    }

                    //Assigned Members
                    if (vmodel.AssignedUsers != null)
                    {
                        IList<PeriodicProcessFlowAssignUser> AssgnUserObj = new List<PeriodicProcessFlowAssignUser>();
                       
                        foreach (var arr in vmodel.AssignedUsers)
                        {
                            AssgnUserObj.Add(new PeriodicProcessFlowAssignUser() { PerdcProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (AssgnUserObj != null)
                        {
                            db.PeriodicProcessFlowAssignUsers.AddRange(AssgnUserObj);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Created, UserId, "AMCPeriodicProcessFlow", "PeriodicProcessFlows", findip(), Obj.PeriodicProcessFlowId, "Process Flow Added Successfully");
                    msg = "Successfully added Periodic Process Flow details.";
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

            PeriodicProcessFlow Obj = db.PeriodicProcessFlows.Find(id);

            if (Obj == null)
            {
                return NotFound();
            }
            AMCProcessFlowViewModel vmodel = new AMCProcessFlowViewModel();

            vmodel.PeriodicProcessFlowId    =   Obj.PeriodicProcessFlowId;
            vmodel.PeriodicStatus           =   Obj.PeriodicStatus;
            vmodel.RemoveUpdateUser         =   Obj.RemoveUpdateUser;
            vmodel.RemoveUpdateUserTeams    =   Obj.RemoveUpdateUserTeams;

            var AmcStatuss = db.AmcStatuss
                .Select(s => new
                {
                    ID      =   s.AmcStatusId,
                    Name    =   s.StatusName
                })
                .ToList();
            ViewBag.AmcStat = QkSelect.List(AmcStatuss, "ID", "Name");

            var atype = db.PeriodicProcessFlowAssignTypes.Where(a => a.PerdcProcessFlowId == id).Select(a => a.TeamId).ToArray();
            var asstype = db.Teams
              .Select(s => new
              {
                  ID    =   s.TeamId,
                  Name  =   s.TeamName
              })
              .ToList();
            ViewBag.AssignTypes = new MultiSelectList(asstype, "ID", "Name", atype);

            vmodel.AssignedUsers = db.PeriodicProcessFlowAssignUsers.Where(a => a.PerdcProcessFlowId == id).Select(a => a.EmployeeId).ToList().ToArray() ?? null;

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
            var statusExists = db.PeriodicProcessFlows.Any(u => u.PeriodicStatus == vmodel.PeriodicStatus && u.PeriodicProcessFlowId != id);
            if (statusExists)
            {
                msg = "A Process Flow with same Periodic Status exists...";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    PeriodicProcessFlow pf = db.PeriodicProcessFlows.Find(id);

                    pf.PeriodicStatus           =   vmodel.PeriodicStatus;
                    pf.RemoveUpdateUser         =   vmodel.RemoveUpdateUser;
                    pf.RemoveUpdateUserTeams    =   vmodel.RemoveUpdateUserTeams;

                    db.Entry(pf).State = EntityState.Modified;
                    db.SaveChanges();

                    Int64 pfId = pf.PeriodicProcessFlowId;

                    var pftype = db.PeriodicProcessFlowAssignTypes.Where(a => a.PerdcProcessFlowId == pfId);
                    if (pftype != null)
                    {
                        db.PeriodicProcessFlowAssignTypes.RemoveRange(db.PeriodicProcessFlowAssignTypes.Where(a => a.PerdcProcessFlowId == pfId));
                        db.SaveChanges();
                    }

                    if (vmodel.AssignTypeAll != null)
                    {
                        PeriodicProcessFlowAssignType AssgnType = new PeriodicProcessFlowAssignType();
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            AssgnType.PerdcProcessFlowId = pfId;
                            AssgnType.TeamId = arr;
                            db.PeriodicProcessFlowAssignTypes.Add(AssgnType);
                            db.SaveChanges();
                        }
                    }

                    var teammembers = db.PeriodicProcessFlowAssignUsers.Where(a => a.PerdcProcessFlowId == pfId);
                    if (teammembers != null)
                    {
                        db.PeriodicProcessFlowAssignUsers.RemoveRange(db.PeriodicProcessFlowAssignUsers.Where(a => a.PerdcProcessFlowId == pfId));
                        db.SaveChanges();
                    }
                    if (vmodel.AssignedUsers != null)
                    {
                        IList<PeriodicProcessFlowAssignUser> AmcAssgndUsers = new List<PeriodicProcessFlowAssignUser>();

                        foreach (var arr in vmodel.AssignedUsers)
                        {
                            AmcAssgndUsers.Add(new PeriodicProcessFlowAssignUser() { PerdcProcessFlowId = (long)pfId, EmployeeId = arr });
                        }
                        if (AmcAssgndUsers != null)
                        {
                            db.PeriodicProcessFlowAssignUsers.AddRange(AmcAssgndUsers);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Updated, UserId, "AMCPeriodicProcessFlow", "PeriodicProcessFlows", findip(), pfId, "Process Flow Updated Successfully");
                    msg = "Successfully Updated Process Flow details..";
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
            PeriodicProcessFlow pf = db.PeriodicProcessFlows.Find(id);
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
            PeriodicProcessFlow pf = db.PeriodicProcessFlows.Find(id);

            var pfuser = db.PeriodicProcessFlowAssignUsers.Where(a => a.PerdcProcessFlowId == id).ToList();

            foreach (var arr in pfuser)
            {
                var pfmember = db.PeriodicProcessFlowAssignUsers.Where(a => a.PerdcProcessFlowId == arr.PerdcProcessFlowId);
                if (pfmember != null)
                {
                    db.PeriodicProcessFlowAssignUsers.RemoveRange(db.PeriodicProcessFlowAssignUsers.Where(a => a.PerdcProcessFlowId == arr.PerdcProcessFlowId));
                    db.SaveChanges();
                }
            }
            db.PeriodicProcessFlows.RemoveRange(db.PeriodicProcessFlows.Where(a => a.PeriodicProcessFlowId == id));
            db.SaveChanges();            

            com.addlog(LogTypes.Deleted, UserId, "AMCPeriodicProcessFlow", "PeriodicProcessFlows", findip(), pf.PeriodicProcessFlowId, "Process Flow Deleted Successfully");
            stat = true;
            msg = "Successfully Deleted Process Flow details..";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
    }
}