using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using QuickSoft.Controllers;
using QuickSoft.ViewModel;
using System.Globalization;

namespace QuickSoft.Areas.Hr.Controllers
{
    [RedirectingAction]
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class EmployeeGradeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public EmployeeGradeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
       // GET: EmployeeGrade
        [QkAuthorize(Roles = "Dev,EmployeeGrade List")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,EmployeeGrade List")]
        public JsonResult GetEmployeeGrade()
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

            //var uDev = User.IsInRole("Dev");
            //var uEdit = User.IsInRole("Edit EmployeeGrade");
            //var uDelete = User.IsInRole("Delete EmployeeGrade");

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = db.EmployeeGrades.Select(b => new
            {
                b.EmployeeGradeId,
                b.GradeName,
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.GradeName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                //v = v.OrderBy(c => c.ProductCategoryID);
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        // GET: Field/Create
        [QkAuthorize(Roles = "Dev,Create EmployeeGrade")]
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: Dep/Create
        [HttpPost]
       // [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create EmployeeGrade")]
        public JsonResult Create(EmployeeGradeViewModel vmodel)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.EmployeeGrades.Any(c => c.GradeName == vmodel.GradeName);
                if (Exists)
                {
                    msg = "Grade Name already exists.";
                    stat = false;
                }
                else
                {
                    var empgrad = new EmployeeGrade
                    {
                        GradeName = vmodel.GradeName,
                        Note = vmodel.Note
                    };
                    db.EmployeeGrades.Add(empgrad);
                    db.SaveChanges();
                    Id = empgrad.EmployeeGradeId;

                    EmpGradeSalaryDetail sstr = new EmpGradeSalaryDetail();
                    foreach (var arr in vmodel.salarystr)
                    {
                        if (arr.PayHeadId > 0)
                        {
                            sstr.EmployeeGradeId = Id;
                            sstr.Payhead = arr.PayHeadId;
                            sstr.Rate = arr.Rate;
                            sstr.EffectFrom = DateTime.Parse(arr.EffectFrom.ToString(), new CultureInfo("en-GB"));

                            db.EmpGradeSalaryDetails.Add(sstr);
                            db.SaveChanges();
                        }
                    }

                   
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "EmployeeGrade", "EmployeeGrades", findip(), Id, "Employee Grade Added Successfully");

                    msg = "Successfully added Employee Grade details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };
        }


        // GET: dep/Edit/5
        [QkAuthorize(Roles = "Dev,Edit EmployeeGrade")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            EmployeeGrade empgr = db.EmployeeGrades.Find(id);

            if (empgr == null)
            {
                return NotFound();
            }
            EmployeeGradeViewModel vmodel = new EmployeeGradeViewModel();
            vmodel.EmployeeGradeId = empgr.EmployeeGradeId;
            vmodel.GradeName = empgr.GradeName;
            vmodel.Note = empgr.Note;

            return PartialView(vmodel);
        }

        // POST: department/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit EmployeeGrade")]
        public JsonResult Edit(int id, EmployeeGradeViewModel vmodel)
        {

            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.EmployeeGrades.Any(c => c.GradeName == vmodel.GradeName && c.EmployeeGradeId != id);
                if (Exists)
                {
                    msg = "Employee Grade already exists.";
                    stat = false;
                }
                else
                {
                    EmployeeGrade empgrde = db.EmployeeGrades.Find(vmodel.EmployeeGradeId);

                    empgrde.GradeName = vmodel.GradeName;
                    empgrde.Note = vmodel.Note;
                    
                    db.Entry(empgrde).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 empgrId = empgrde.EmployeeGradeId;

                    var grdetail = db.EmpGradeSalaryDetails.Where(a => a.EmployeeGradeId == id);
                    if (grdetail != null)
                    {
                        db.EmpGradeSalaryDetails.RemoveRange(db.EmpGradeSalaryDetails.Where(a => a.EmployeeGradeId == id));
                        db.SaveChanges();
                    }
                    EmpGradeSalaryDetail sstr = new EmpGradeSalaryDetail();
                    foreach (var arr in vmodel.salarystr)
                    {
                        if (arr.PayHeadId > 0)
                        {
                            sstr.EmployeeGradeId = empgrId;
                            sstr.Payhead = arr.PayHeadId;
                            sstr.Rate = arr.Rate;
                            sstr.EffectFrom = DateTime.Parse(arr.EffectFrom.ToString(), new CultureInfo("en-GB"));

                            db.EmpGradeSalaryDetails.Add(sstr);
                            db.SaveChanges();
                        }
                    }
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "EmployeeGrade", "EmployeeGrades", findip(), empgrId, "Employee Grade Updated Successfully");

                    msg = "Successfully updated Employee Grade details.";
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

        // GET: Desg/Delete/5
        [QkAuthorize(Roles = "Dev,Delete EmployeeGrade")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmployeeGrade empgr = db.EmployeeGrades.Find(id);
            if (empgr == null)
            {
                return NotFound();
            }
            return PartialView(empgr);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete EmployeeGrade")]
        public JsonResult Delete(long id, IFormCollection collection)
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
                EmployeeGrade empgr = db.EmployeeGrades.Find(id);

                var empgra = db.EmpGradeSalaryDetails.Where(a => a.EmployeeGradeId == id);
                if (empgra != null)
                {
                    db.EmpGradeSalaryDetails.RemoveRange(db.EmpGradeSalaryDetails.Where(a => a.EmployeeGradeId == id));
                    db.SaveChanges();
                }

                db.EmployeeGrades.Remove(empgr);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "EmployeeGrade", "EmployeeGrades", findip(), empgr.EmployeeGradeId, "Employee Grade Deleted Successfully");


                stat = true;
                msg = "Successfully Deleted Employee Grade details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.EmployeeWorkDetails.Any(c => c.EmployeeGrade == id))
            {
                msg = "Employee Grade Already used in Employee !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }
        public JsonResult SearchEmployeeGrade(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.EmployeeGrades
                                  where a.GradeName.ToLower().Contains(q.ToLower()) || a.GradeName.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.GradeName,
                                      id = a.EmployeeGradeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.EmployeeGrades.Select(a => new SelectFormat
                {
                    text = a.GradeName,
                    id = a.EmployeeGradeId
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee Grade" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpGet]
        public JsonResult GetEmployeeGradeById(int SalID)
        {
            var v = (from a in db.EmpGradeSalaryDetails
                     join b in db.Payheads on a.Payhead equals b.ID into phead
                     from b in phead.DefaultIfEmpty()
                     where a.EmployeeGradeId == SalID
                     select new
                     {
                         a.Payhead,
                         b.Name,
                         a.Rate,
                         a.EffectFrom,
                         b.CalculationPeriod,
                         b.Type,
                         b.CalculationType,
                         b.Compute
                     }).AsEnumerable().Select(o => new
                     {
                         PayHeadId=o.Payhead,
                         PayHead = o.Name,
                         o.Rate,
                         Date=o.EffectFrom,
                         Per = o.CalculationPeriod,
                         HeadType = Enum.GetName(typeof(PayHeadType), o.Type),
                         CalType = Enum.GetName(typeof(CalcTypePayHead), o.CalculationType),
                         Computed = Enum.GetName(typeof(ComputPayHead), o.Compute),
                     }).ToList();

            return Json(v);

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