using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Areas.Hr.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class WorkShiftController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public WorkShiftController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Hr/WorkShift
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,WorkShift List")]
        public ActionResult Index()
        {
            return View();
        }
        [RedirectingAction]
        [Authorize(Roles = "Dev,WorkShift List")]
        [HttpPost]
        public ActionResult GetWorkShift()
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

            var UserView = (from a in db.WorkShifts
                            join c in db.Users on a.CreatedBy equals c.Id
                            select new
                            {
                                id = a.WorkShiftId,
                                a.WorkShiftName,
                                a.StartTime,
                                a.EndTime,
                                a.LateCountTime
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.WorkShiftName.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create WorkShift")]
        public ActionResult Create(long? id)
        {          
            return PartialView();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create WorkShift")]
        public JsonResult Create(WorkShiftViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.WorkShifts.Any(c => c.WorkShiftName == vmodel.WorkShiftName);
                if (Exists)
                {
                    msg = "Work Shift Name already exists.";
                    stat = false;
                }
                else
                {

                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    var wksft = new WorkShift
                    {
                        WorkShiftName = vmodel.WorkShiftName,
                        StartTime=vmodel.StartTime,
                        EndTime = vmodel.EndTime,
                        LateCountTime=vmodel.LateCountTime,
                        CreatedDate = today,
                        CreatedBy = UserId,
                        Status = Status.active,
                        Branch = BranchID
                    };
                    db.WorkShifts.Add(wksft);
                    db.SaveChanges();
                    Int64 wksftId = wksft.WorkShiftId;

                    com.addlog(LogTypes.Created, UserId, "WorkShift", "WorkShifts", findip(), wksftId, "Work Shift Added Successfully");
                    msg = "Successfully added Work Shift details.";
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
        [QkAuthorize(Roles = "Dev,Edit WorkShift")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkShift wks = db.WorkShifts.Find(id);

            if (wks == null)
            {
                return NotFound();
            }
            WorkShiftViewModel vmodel = new WorkShiftViewModel();
            
            vmodel.WorkShiftName = wks.WorkShiftName;
            vmodel.StartTime = wks.StartTime;
            vmodel.EndTime = wks.EndTime;
            vmodel.LateCountTime = wks.LateCountTime;

            return PartialView(vmodel);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit WorkShift")]
        [HttpPost]
        public JsonResult Edit(long? id, WorkShiftViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.WorkShifts.Any(c => c.WorkShiftName == vmodel.WorkShiftName && c.WorkShiftId != id);
                if (Exists)
                {
                    msg = "Work Shift with The Same Name already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    WorkShift wksft = db.WorkShifts.Find(id);

                    wksft.WorkShiftName = vmodel.WorkShiftName;
                    wksft.StartTime = vmodel.StartTime;
                    wksft.EndTime = vmodel.EndTime;
                    wksft.LateCountTime = vmodel.LateCountTime;

                    db.Entry(wksft).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 wksftId = wksft.WorkShiftId;

                    com.addlog(LogTypes.Updated, UserId, "WorkShift", "WorkShifts", findip(), wksftId, "WorkShift Updated Successfully");
                    msg = "Successfully Updated Work Shift details.";
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

        // GET: /Delete/5
        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete WorkShift")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            WorkShift wrksft = db.WorkShifts.Find(id);
            if (wrksft == null)
            {
                return NotFound();
            }
            return PartialView(wrksft);
        }

        // POST: /Delete/5

        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete WorkShift")]
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
                msg = "Successfully Deleted Work Shift details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            WorkShift ws = db.WorkShifts.Find(id);

            db.WorkShifts.Remove(ws);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "WorkShift", "WorkShifts", findip(), ws.WorkShiftId, "Work Shift Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.EmployeeWorkDetails.Any(c => c.WorkShift == id))
            {
                msg = "Work Shift Already used in Employee !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        public JsonResult SearchWorkShift(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.WorkShifts
                                  where a.WorkShiftName.ToLower().Contains(q.ToLower()) || a.WorkShiftName.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.WorkShiftName,
                                      id = a.WorkShiftId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.WorkShifts.Select(b => new SelectFormat
                {
                    text = b.WorkShiftName,
                    id = b.WorkShiftId
                }).OrderBy(b => b.text).ToList();

            }//
            return Json(serialisedJson);
        }
    }
}