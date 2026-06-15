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
    public class AMCStatusController : BaseController
    {
        ApplicationDbContext db;
        Common com;

        public AMCStatusController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: AmcStatus
        public ActionResult Index()
        {
            return View();
        }

        //Function for Index--POST
        [HttpPost]
        [RedirectingAction]
        public ActionResult GetAmcStatus()
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

           var v = (from a in db.AmcStatuss
                     join b in db.Users on a.CreatedBy equals b.Id
                     select new
                     {
                         a.AmcStatusId,
                         a.StatusName,
                         b.UserName,
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
            var page = v.Skip(skip).Take(pageSize).ToList();
            var ids = page.Select(r => r.AmcStatusId).ToList();

            // bulk lookups (two round-trips total, mirrors EF6 per-row result but batched)
            var deptLk = (from z in db.AmcStatusDepts
                          join y in db.Departments on z.DeptId equals y.DepartmentID
                          where ids.Contains(z.AmcStatusId)
                          select new { z.AmcStatusId, DeptName = y.DepartmentName ?? "" })
                         .ToList()
                         .ToLookup(x => x.AmcStatusId);

            var desgLk = (from z in db.AmcStatusDesgs
                          join y in db.Designations on z.DesgId equals y.DesignationID
                          where ids.Contains(z.AmcStatusId)
                          select new { z.AmcStatusId, DesgnName = y.DesignationName ?? "" })
                         .ToList()
                         .ToLookup(x => x.AmcStatusId);

            var data = page.Select(r => new
            {
                r.AmcStatusId,
                r.StatusName,
                r.UserName,
                Depts = deptLk[r.AmcStatusId].Select(x => new { x.DeptName }).ToList(),
                Desgn = desgLk[r.AmcStatusId].Select(x => new { x.DesgnName }).ToList(),
            }).ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        //Create--GET
        [RedirectingAction]
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

        //Saving--Create
        [HttpPost]
        [RedirectingAction]
        public JsonResult Create(AmcStatusViewModel vmodel)
        {
            bool stat = false;
            string msg;
            Int64 AmcStatusId = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.AmcStatuss.Any(c => c.StatusName == vmodel.StatusName);
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

                    AmcStatus Obj = new AmcStatus();
                    Obj.StatusName  =   vmodel.StatusName;
                    Obj.Status      =   Status.active;
                    Obj.CreatedDate =   today;
                    Obj.CreatedBy   =   UserId;
                    Obj.Branch      =   BranchID;

                    db.AmcStatuss.Add(Obj);
                    db.SaveChanges();

                    AmcStatusId = Obj.AmcStatusId;

                    var Dep = Convert.ToString(vmodel.Department);
                    if (Dep != null && Dep != "")
                    {
                        long[] Deps = Dep.Split(',').Select(Int64.Parse).ToArray();

                        AmcStatusDept AmcDept = new AmcStatusDept();
                        foreach (var depp in Deps)
                        {
                            AmcDept.AmcStatusId = AmcStatusId;
                            AmcDept.DeptId = depp;
                            db.AmcStatusDepts.Add(AmcDept);
                            db.SaveChanges();
                        }
                    }

                    var Deg = Convert.ToString(vmodel.Designation);
                    if (Deg != null && Deg != "")
                    {
                        long[] Degs = Deg.Split(',').Select(Int64.Parse).ToArray();

                        AmcStatusDesg AmcDesg = new AmcStatusDesg();
                        foreach (var desg in Degs)
                        {
                            AmcDesg.AmcStatusId = AmcStatusId;
                            AmcDesg.DesgId = desg;
                            db.AmcStatusDesgs.Add(AmcDesg);
                            db.SaveChanges();
                        }
                    }

                    msg = "Amc Status added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "AMCStatus", "AmcStatus", findip(), AmcStatusId, "Amc Status Added Successfully");
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = AmcStatusId } };
        }
        
        //GET--Edit
        [RedirectingAction]
        public ActionResult Edit(long id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AmcStatusViewModel vmodel = new AmcStatusViewModel();
            var pstat = db.AmcStatuss.Find(id);
            vmodel.StatusName = pstat.StatusName;
            vmodel.AmcStatusId = pstat.AmcStatusId;

            if (pstat == null)
            {
                return NotFound();
            }

            var dep = db.AmcStatusDepts.Where(a => a.AmcStatusId == id).Select(a => a.DeptId).ToList();
            long[] deps = dep.ToArray();

            var depar = db.Departments
                  .Select(s => new
                  {
                      FieldName = s.DepartmentID,
                      FieldID = s.DepartmentName
                  })
                  .ToList();
            ViewBag.Depart = new MultiSelectList(depar, "FieldName", "FieldID", deps);


            var des = db.AmcStatusDesgs.Where(a => a.AmcStatusId == id).Select(a => a.DesgId).ToList();
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

        //POST---Edit
        [HttpPost]
        [RedirectingAction]
        public JsonResult Edit(AmcStatusViewModel vmodel, long? id)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.AmcStatuss.Any(c => c.StatusName == vmodel.StatusName && c.AmcStatusId != vmodel.AmcStatusId);
                if (Exists)
                {
                    msg = "AMC Status already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                    AmcStatus Obj   = db.AmcStatuss.Find(id);
                    Obj.StatusName  = vmodel.StatusName;
                    Obj.CreatedBy   = UserId;
                    Obj.CreatedDate = today;
                    Obj.Branch      = BranchID;

                    db.Entry(Obj).State = EntityState.Modified;
                    db.SaveChanges();

                    var Dept = db.AmcStatusDepts.Where(a => a.AmcStatusId == id).FirstOrDefault();
                    if (Dept != null)
                    {
                        db.AmcStatusDepts.RemoveRange(db.AmcStatusDepts.Where(a => a.AmcStatusId == id));
                        db.SaveChanges();
                    }

                    var Dep = Convert.ToString(vmodel.Department);
                    if (Dep != null && Dep != "")
                    {
                        long[] Deps = Dep.Split(',').Select(Int64.Parse).ToArray();

                        AmcStatusDept AmcDept = new AmcStatusDept();
                        foreach (var depp in Deps)
                        {
                            AmcDept.AmcStatusId = Obj.AmcStatusId;
                            AmcDept.DeptId = depp;
                            db.AmcStatusDepts.Add(AmcDept);
                            db.SaveChanges();
                        }
                    }

                    var Desg = db.AmcStatusDesgs.Where(a => a.AmcStatusId == id).FirstOrDefault();
                    if (Desg != null)
                    {
                        db.AmcStatusDesgs.RemoveRange(db.AmcStatusDesgs.Where(a => a.AmcStatusId == id));
                        db.SaveChanges();
                    }

                    var Deg = Convert.ToString(vmodel.Designation);
                    if (Deg != null && Deg != "")
                    {
                        long[] Degs = Deg.Split(',').Select(Int64.Parse).ToArray();

                        AmcStatusDesg AmcDesg = new AmcStatusDesg();
                        foreach (var desg in Degs)
                        {
                            AmcDesg.AmcStatusId = Obj.AmcStatusId;
                            AmcDesg.DesgId = desg;
                            db.AmcStatusDesgs.Add(AmcDesg);
                            db.SaveChanges();
                        }
                    }

                    msg = "Successfully Updated Amc Status details.";
                    stat = true;

                    com.addlog(LogTypes.Updated, UserId, "AMCStatus", "AmcStatus", findip(), Obj.AmcStatusId, "AMC Status Updated Successfully");
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: AMCStatus/Delete/5
        [RedirectingAction]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AmcStatus pstat = db.AmcStatuss.Find(id);
            if (pstat == null)
            {
                return NotFound();
            }
            return PartialView(pstat);
        }

        // POST: AmcStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [RedirectingAction]
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
                msg = "Amc Status Deleted Successfully";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();

            AmcStatus pstat = db.AmcStatuss.Find(id);
            db.AmcStatuss.Remove(pstat);
            db.SaveChanges();

            var Dept = db.AmcStatusDepts.Where(a => a.AmcStatusId == id).FirstOrDefault();
            if (Dept != null)
            {
                db.AmcStatusDepts.RemoveRange(db.AmcStatusDepts.Where(a => a.AmcStatusId == id));
                db.SaveChanges();
            }

            var Desg = db.AmcStatusDesgs.Where(a => a.AmcStatusId == id).FirstOrDefault();
            if (Desg != null)
            {
                db.AmcStatusDesgs.RemoveRange(db.AmcStatusDesgs.Where(a => a.AmcStatusId == id));
                db.SaveChanges();
            }

            com.addlog(LogTypes.Deleted, UserId, "AmcStatus", "AMCStatus", findip(), pstat.AmcStatusId, "AMC Status Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.Amcs.Any(c => c.AmcStatusId == id))
            {
                msg = "Status Already used in AMC";
            }
            else if (db.AmcProcessFlows.Any(c => c.AmcStatus == id))
            {
                msg = "Status Already used in Process Flow";
            }
            else if (db.PeriodicProcessFlows.Any(c => c.PeriodicStatus == id))
            {
                msg = "Status Already used in Periodic Process Flow";
            }
            else if (db.AmcRemarks.Any(c => c.StatusID == id))
            {
                msg = "Status Already used in AMC Remark";
            }
            else
            {
                msg = null;
            }
            return msg;
        }
    }
}