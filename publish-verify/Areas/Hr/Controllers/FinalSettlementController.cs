using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections;

namespace QuickSoft.Areas.Hr.Controllers
{
    [RedirectingAction]
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class FinalSettlementController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public FinalSettlementController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,FinalSettlement List")]
        public ActionResult Index()
        {
            return View();
        }
        [RedirectingAction]
        [HttpPost]
        // [QkAuthorize(Roles = "Dev,FinalSettlement List")]
        public ActionResult GetFinalSettlement(long? BName, long? Item, decimal? Qty, long? Unit)
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

            var v = (from a in db.FinalSettlements
                     join b in db.Employees on a.Employee equals b.EmployeeId into emp
                     from b in emp.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     select new
                     {
                         a.FinalSettlementID,
                         EmpName = b.FirstName + " " + b.LastName,
                         a.Date,
                         a.Reason,
                         a.JoiningDate,
                         a.LastworkingDate,
                         a.DeductionDays,
                         a.NoDaysAbsent,
                         a.NoDaysWorked,
                         a.TotalDays,
                         a.GratuityDays,
                         a.GratuityAmount,
                         a.NetAmount,
                         a.Remarks,
                         e.UserName,
                         a.Designation,
                         b.JobStatus,
                     }).AsEnumerable().Select(o => new
                     {
                         o.FinalSettlementID,
                         o.EmpName,
                         o.Date,
                         o.Reason,
                         o.JoiningDate,
                         o.LastworkingDate,
                         o.DeductionDays,
                         o.NoDaysAbsent,
                         o.NoDaysWorked,
                         o.TotalDays,
                         o.GratuityDays,
                         o.GratuityAmount,
                         o.NetAmount,
                         o.Remarks,
                         o.UserName,
                         o.Designation,
                         TypeofSettlement = Enum.GetName(typeof(JobStatus), o.JobStatus),
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.EmpName.ToString().ToLower().Equals(search.ToLower()));
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


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create FinalSettlement")]
        public ActionResult Create()
        {
            FinalSettlementViewModel vmodel = new FinalSettlementViewModel();
            vmodel.Date = System.DateTime.Now.ToString("dd-MM-yyyy");
            vmodel.LastDutyDate = System.DateTime.Now.ToString("dd-MM-yyyy");
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            ViewBag.SelVal = QkSelect.List(
                              new List<SelectListItem>
                              {
                                   new SelectListItem { Selected = true, Text = "Select", Value = ""},
                              }, "Value", "Text", 0);

            ViewBag.Typeofset = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Resinged", Value="1"},
                new SelectListItem() {Text = "Termination", Value="2"},
            }, "Value", "Text");

            return View(vmodel);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create FinalSettlement")]
        public JsonResult Create(FinalSettlementViewModel vmodel, string fnval)
        {
            string msg;
            bool stat;
            DateTime LSdate = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
            var Exists = db.FinalSettlements.Any(c => c.Employee == vmodel.Employee);
            if (Exists)
            {
                msg = "Final Settlement already exists in Selected Employee..!!";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {

                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = (long)vmodel.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }

                FinalSettlement attend = new FinalSettlement();

                attend.Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                attend.Remarks = vmodel.Remarks;
                attend.Employee = vmodel.Employee;
                attend.GratuityAmount = vmodel.GratuityAmount;
                attend.GratuityDays = vmodel.GratuityDays;
                attend.JoiningDate = DateTime.Parse(vmodel.JoiningDate.ToString(), new CultureInfo("en-GB"));
                attend.LastworkingDate = DateTime.Parse(vmodel.LastworkingDate.ToString(), new CultureInfo("en-GB"));
                attend.LastDutyDate = DateTime.Parse(vmodel.LastDutyDate.ToString(), new CultureInfo("en-GB"));
                attend.NetAmount = vmodel.NetAmount;
                attend.NoDaysAbsent = vmodel.NoDaysAbsent;
                attend.NoDaysWorked = vmodel.NoDaysWorked;
                attend.Remarks = vmodel.Remarks;
                attend.TypeofSettlement = Convert.ToInt64(vmodel.TypeofSettlement);
                attend.TotalDays = vmodel.TotalDays;
                attend.DeductionDays = vmodel.DeductionDays;
                attend.Branch = Branch;
                attend.CreatedDate = today;
                attend.CreatedBy = UserId;
                attend.Status = Status.active;

                db.FinalSettlements.Add(attend);
                db.SaveChanges();
                Int64 LSId = attend.FinalSettlementID;
                PayheadFS PFs = new PayheadFS();
                if (vmodel.Additiondetails != null)
                {
                    foreach (var arr in vmodel.Additiondetails)
                    {
                        if (arr.Amount != null)
                        {
                            PFs.Amount = Convert.ToInt64(arr.Amount);
                            PFs.FinalSettlementId = LSId;
                            PFs.PayheadId = Convert.ToInt64(arr.Payhead);
                            db.PayheadFSs.Add(PFs);
                            db.SaveChanges();
                        }
                    }
                }
                if (vmodel.Deductiondetails != null)
                {
                    foreach (var arr in vmodel.Deductiondetails)
                    {
                        if (arr.Amount != null)
                        {
                            PFs.Amount = Convert.ToInt64(arr.Amount);
                            PFs.FinalSettlementId = LSId;
                            PFs.PayheadId = Convert.ToInt64(arr.Payhead);
                            db.PayheadFSs.Add(PFs);
                            db.SaveChanges();
                        }
                    }
                }

                Employee emp = db.Employees.Find(vmodel.Employee);
                emp.JobStatus = (vmodel.TypeofSettlement == "1" || vmodel.TypeofSettlement == "2") ? JobStatus.Resigned : JobStatus.Absconding;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();

                com.addlog(LogTypes.Created, UserId, "FinalSettlement", "FinalSettlements", findip(), LSId, "Successfully Submitted Leave Settlement");
                if ((fnval) == "print")
                {
                    //var data = vmodel;
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel;

                    var Data = (from a in db.FinalSettlements
                                join b in db.Employees on a.Employee equals b.EmployeeId into emplo
                                from b in emplo.DefaultIfEmpty()
                                join e in db.Users on a.CreatedBy equals e.Id
                                where a.Employee == vmodel.Employee
                                select new
                                {
                                    a.FinalSettlementID,
                                    EmpName = b.FirstName + " " + b.LastName,
                                    b.EMPCode,
                                    a.Date,
                                    a.Reason,
                                    a.JoiningDate,
                                    a.LastworkingDate,
                                    a.DeductionDays,
                                    a.NoDaysAbsent,
                                    a.NoDaysWorked,
                                    a.TotalDays,
                                    a.GratuityDays,
                                    a.GratuityAmount,
                                    a.NetAmount,
                                    a.Remarks,
                                    e.UserName,
                                    a.Designation,
                                    b.JobStatus,
                                    b.EmployeeId,
                                }).AsEnumerable().Select(o => new
                                {
                                    o.FinalSettlementID,
                                    o.EmpName,
                                    o.EMPCode,
                                    o.Date,
                                    o.Reason,
                                    o.JoiningDate,
                                    o.LastworkingDate,
                                    o.DeductionDays,
                                    o.NoDaysAbsent,
                                    o.NoDaysWorked,
                                    o.TotalDays,
                                    o.GratuityDays,
                                    o.GratuityAmount,
                                    o.NetAmount,
                                    o.Remarks,
                                    o.UserName,
                                    o.Designation,
                                    TypeofSettlement = Enum.GetName(typeof(JobStatus), o.JobStatus),
                                }).FirstOrDefault();

                    var bsalary = (from aa in db.SalaryStructures
                                   join bb in db.SalaryStrDetails on aa.SalaryStrId equals bb.SalaryStrId into sal
                                   from bb in sal.DefaultIfEmpty()
                                   let payh = db.Payheads.Where(a => a.Name == "Basic Pay").Select(a => a.ID).FirstOrDefault()
                                   where aa.EmployeeId == vmodel.Employee && bb.PayHeadId == payh
                                   select new
                                   {
                                       bb.Rate
                                   }).FirstOrDefault();
                    var Addition = (from a in db.PayheadFSs
                                    join b in db.Payheads on a.PayheadId equals b.ID into item
                                    from b in item.DefaultIfEmpty()
                                    where a.FinalSettlementId == vmodel.FinalSettlementID && b.Type == PayHeadType.EarningsInAnnualLeave
                                    select
                                       a.Amount
                            ).Sum();
                    var Deduction = (from a in db.PayheadFSs
                                     join b in db.Payheads on a.PayheadId equals b.ID into item
                                     from b in item.DefaultIfEmpty()
                                     where a.FinalSettlementId == vmodel.FinalSettlementID && b.Type == PayHeadType.DeductionInAnnualLeave
                                     select
                                        a.Amount
                                        ).Sum();
                    var Additionlist = (from a in db.PayheadFSs
                                        join b in db.Payheads on a.PayheadId equals b.ID into item
                                        from b in item.DefaultIfEmpty()
                                        where a.FinalSettlementId == LSId && b.Type == PayHeadType.EarningsInAnnualLeave
                                        select new
                                        {
                                            b.NameinSlip,
                                            a.Amount
                                        }).ToList();
                    var Deductionlist = (from a in db.PayheadFSs
                                         join b in db.Payheads on a.PayheadId equals b.ID into item
                                         from b in item.DefaultIfEmpty()
                                         where a.FinalSettlementId == LSId && b.Type == PayHeadType.DeductionInAnnualLeave
                                         select new
                                         {
                                             b.NameinSlip,
                                             a.Amount
                                         }).ToList();
                    var arr = new ArrayList();
                    arr.Add(Data);
                    //var fmapp = db.FieldMappings.Where(a => a.Section == "PropertyRegistration" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully Updated Final Settlement.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, DedList = Deductionlist, AddList = Additionlist, ComHeadCheck, Addition, Deduction, bsalary } };
                }
                else
                {
                    msg = "Successfully Updated Final Settlement.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult {
                        Data = new
                        {
                            status = stat,
                            message = msg
                        }
                    };
                }

            }
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit FinalSettlement")]
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinalSettlement Fs = db.FinalSettlements.Find(id);

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            ViewBag.SelVal = QkSelect.List(
                              new List<SelectListItem>
                              {
                                   new SelectListItem { Selected = true, Text = "Select", Value = ""},
                              }, "Value", "Text", 0);

            ViewBag.Typeofset = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Resinged", Value="1"},
                new SelectListItem() {Text = "Termination", Value="2"},
            }, "Value", "Text");

            if (Fs == null)
            {
                return NotFound();
            }
            var use = db.Employees.Where(a => a.EmployeeId == Fs.Employee)
                          .Select(s => new
                          {
                              ID = s.EmployeeId,
                              Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                          })
                          .ToList();
            ViewBag.Employ = QkSelect.List(use, "ID", "Name");
            FinalSettlementViewModel vmodel = new FinalSettlementViewModel();
            vmodel.FinalSettlementID = (long)id;
            vmodel.Remarks = Fs.Remarks;
            vmodel.Employee = Fs.Employee;
            vmodel.Date = Fs.Date.ToString("dd-MM-yyyy");
            vmodel.TypeofSettlement = Fs.TypeofSettlement.ToString();
            vmodel.TotalDays = Fs.TotalDays;
            vmodel.DeductionDays = Fs.DeductionDays;
            vmodel.NetAmount = Fs.NetAmount;
            vmodel.GratuityAmount = Fs.GratuityAmount;
            vmodel.GratuityDays = Fs.GratuityDays;
            vmodel.NoDaysAbsent = Fs.NoDaysAbsent;
            vmodel.NoDaysWorked = Fs.NoDaysWorked;
            vmodel.LastworkingDate = Fs.LastworkingDate != null ? Convert.ToDateTime(Fs.LastworkingDate).ToString("dd-MM-yyyy") : "";
            vmodel.LastDutyDate = Fs.LastDutyDate != null ? Convert.ToDateTime(Fs.LastDutyDate).ToString("dd-MM-yyyy") : "";
            vmodel.JoiningDate = Fs.JoiningDate != null ? Convert.ToDateTime(Fs.JoiningDate).ToString("dd-MM-yyyy") : "";


            return View(vmodel);
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit FinalSettlement")]
        public ActionResult Edit(FinalSettlementViewModel vmodel, string fnval)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);
            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            FinalSettlement FS = db.FinalSettlements.Find(vmodel.FinalSettlementID);

            FS.Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
            FS.Remarks = vmodel.Remarks;
            FS.Employee = vmodel.Employee;
            FS.GratuityAmount = vmodel.GratuityAmount;
            FS.GratuityDays = vmodel.GratuityDays;
            FS.JoiningDate = DateTime.Parse(vmodel.JoiningDate.ToString(), new CultureInfo("en-GB"));
            FS.LastworkingDate = DateTime.Parse(vmodel.LastworkingDate.ToString(), new CultureInfo("en-GB"));
            FS.NetAmount = vmodel.NetAmount;
            FS.NoDaysAbsent = vmodel.NoDaysAbsent;
            FS.NoDaysWorked = vmodel.NoDaysWorked;
            FS.Remarks = vmodel.Remarks;
            FS.TypeofSettlement = Convert.ToInt64(vmodel.TypeofSettlement);

            FS.Branch = Branch;
            FS.CreatedDate = today;
            FS.CreatedBy = UserId;
            FS.Status = Status.active;
            if (BranchCheck == Status.active)
            {
                Branch = (long)vmodel.Branch;
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }
            db.Entry(FS).State = EntityState.Modified;
            db.SaveChanges();
            Int64 ID = FS.FinalSettlementID;

            db.PayheadFSs.RemoveRange(db.PayheadFSs.Where(a => a.FinalSettlementId == ID));
            db.SaveChanges();
            PayheadFS PFs = new PayheadFS();
            if (vmodel.Additiondetails != null)
            {
                foreach (var arr in vmodel.Additiondetails)
                {
                    if (arr.Amount != null)
                    {
                        PFs.Amount = (arr.Amount);
                        PFs.FinalSettlementId = ID;
                        PFs.PayheadId = Convert.ToInt64(arr.Payhead);
                        db.PayheadFSs.Add(PFs);
                        db.SaveChanges();
                    }
                }
            }
            if (vmodel.Deductiondetails != null)
            {
                foreach (var arr in vmodel.Deductiondetails)
                {
                    if (arr.Amount != null)
                    {
                        PFs.Amount = (arr.Amount);
                        PFs.FinalSettlementId = ID;
                        PFs.PayheadId = Convert.ToInt64(arr.Payhead);
                        db.PayheadFSs.Add(PFs);
                        db.SaveChanges();
                    }
                }
            }
            Employee emp = db.Employees.Find(vmodel.Employee);
            emp.JobStatus = (vmodel.TypeofSettlement == "1" || vmodel.TypeofSettlement == "2") ? JobStatus.Resigned : JobStatus.Absconding;
            db.Entry(emp).State = EntityState.Modified;
            db.SaveChanges();


            com.addlog(LogTypes.Updated, UserId, "FinalSettlement", "FinalSettlements", findip(), ID, "Successfully Updated Final Settlement");
            if ((fnval) == "print")
            {
                //var data = vmodel;
                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                var prodata = vmodel;

                var Data = (from a in db.FinalSettlements
                            join b in db.Employees on a.Employee equals b.EmployeeId into emplo
                            from b in emplo.DefaultIfEmpty()
                            join e in db.Users on a.CreatedBy equals e.Id
                            where a.FinalSettlementID == ID
                            select new
                            {
                                a.FinalSettlementID,
                                EmpName = b.FirstName + " " + b.LastName,
                                b.EMPCode,
                                a.Date,
                                a.Reason,
                                a.JoiningDate,
                                a.LastworkingDate,
                                a.DeductionDays,
                                a.NoDaysAbsent,
                                a.NoDaysWorked,
                                a.TotalDays,
                                a.GratuityDays,
                                a.GratuityAmount,
                                a.NetAmount,
                                a.Remarks,
                                e.UserName,
                                a.Designation,
                                b.JobStatus,
                                b.EmployeeId,
                            }).AsEnumerable().Select(o => new
                            {
                                o.FinalSettlementID,
                                o.EmpName,
                                o.EMPCode,
                                o.Date,
                                o.Reason,
                                o.JoiningDate,
                                o.LastworkingDate,
                                o.DeductionDays,
                                o.NoDaysAbsent,
                                o.NoDaysWorked,
                                o.TotalDays,
                                o.GratuityDays,
                                o.GratuityAmount,
                                o.NetAmount,
                                o.Remarks,
                                o.UserName,
                                o.Designation,
                                TypeofSettlement = Enum.GetName(typeof(JobStatus), o.JobStatus),
                            }).FirstOrDefault();
                var bsalary = (from aa in db.SalaryStructures
                               join bb in db.SalaryStrDetails on aa.SalaryStrId equals bb.SalaryStrId into sal
                               from bb in sal.DefaultIfEmpty()
                               let payh = db.Payheads.Where(a => a.Name == "Basic Pay").Select(a => a.ID).FirstOrDefault()
                               where aa.EmployeeId == vmodel.Employee && bb.PayHeadId == payh
                               select new
                               {
                                   bb.Rate
                               }).FirstOrDefault();
                var Addition = (from a in db.PayheadFSs
                                join b in db.Payheads on a.PayheadId equals b.ID into item
                                from b in item.DefaultIfEmpty()
                                where a.FinalSettlementId == vmodel.FinalSettlementID && b.Type == PayHeadType.EarningsInAnnualLeave
                                select
                                   a.Amount
                        ).Sum();
                var Deduction = (from a in db.PayheadFSs
                                 join b in db.Payheads on a.PayheadId equals b.ID into item
                                 from b in item.DefaultIfEmpty()
                                 where a.FinalSettlementId == vmodel.FinalSettlementID && b.Type == PayHeadType.DeductionInAnnualLeave
                                 select
                                    a.Amount
                                    ).Sum();
                var Additionlist = (from a in db.PayheadFSs
                                    join b in db.Payheads on a.PayheadId equals b.ID into item
                                    from b in item.DefaultIfEmpty()
                                    where a.FinalSettlementId == vmodel.FinalSettlementID && b.Type == PayHeadType.EarningsInAnnualLeave
                                    select new
                                    {
                                        b.NameinSlip,
                                        a.Amount
                                    }).ToList();
                var Deductionlist = (from a in db.PayheadFSs
                                     join b in db.Payheads on a.PayheadId equals b.ID into item
                                     from b in item.DefaultIfEmpty()
                                     where a.FinalSettlementId == vmodel.FinalSettlementID && b.Type == PayHeadType.DeductionInAnnualLeave
                                     select new
                                     {
                                         b.NameinSlip,
                                         a.Amount
                                     }).ToList();
                var arr = new ArrayList();
                arr.Add(Data);
                //var fmapp = db.FieldMappings.Where(a => a.Section == "PropertyRegistration" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                msg = "Successfully Updated Final Settlement.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, DedList = Deductionlist, AddList = Additionlist, ComHeadCheck, Addition, Deduction, bsalary } };
            }
            else
            {
                msg = "Successfully Updated Final Settlement.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult {
                    Data = new
                    {
                        status = stat,
                        message = msg
                    }
                };
            }

        }



        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete FinalSettlement")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FinalSettlement leavset = db.FinalSettlements.Find(id);
            if (leavset == null)
            {
                return NotFound();
            }
            return PartialView(leavset);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete FinalSettlement")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            FinalSettlement leaveset = db.FinalSettlements.Find(id);

            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Final Settlement.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete FinalSettlement")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Final Settlement, Unable to Delete " + notdel + " Final Settlement. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Final Settlement.", true);
            }
            else
            {
                Success("Deleted " + count + " Final Settlement.", true);
            }
            return RedirectToAction("Index", "Final Settlement");
        }
        private Boolean DeleteItem(long id)
        {
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(id);
            }
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            //if ()
            //{
            //}
            //else
            //{
            //    msg = null;
            //}
            return msg;
        }

        public bool DeleteFn(long id)
        {
            FinalSettlement leaveset = db.FinalSettlements.Find(id);
            if (leaveset != null)
            {
                db.FinalSettlements.RemoveRange(db.FinalSettlements.Where(a => a.FinalSettlementID == id));
                db.SaveChanges();
                Employee emp = db.Employees.Find(id);
                emp.JobStatus = JobStatus.Working;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();
            }
            var leapay = db.PayheadFSs.Where(a => a.FinalSettlementId == id).FirstOrDefault();
            if (leapay != null)
            {
                db.PayheadFSs.RemoveRange(db.PayheadFSs.Where(a => a.FinalSettlementId == id));
                db.SaveChanges();
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "FinalSettlement", "FinalSettlements", findip(), id, "Successfully Deleted Leave Settlement");
            db.SaveChanges();
            return true;
        }



        [HttpGet]
        public JsonResult GetFinalDetails(long? Employee, long? Termination)
        {
            //var LWD = db.DailyAttendanceDetails.Where(z => z.EmployeeId == Employee && z.AtType == 4).Select(y => y.AtDate).Last();
            // EF Core 10: the legacy GroupBy-by-bool + Select(FirstOrDefault entity) cannot be translated.
            // Rewrite as a direct ordered scalar subquery. Cast to (DateTime?) so EF emits a plain nullable
            // subquery (NULL when the employee has no AtType==4 row) instead of COALESCE(...,'0001-01-01'),
            // whose literal overflows SQL datetime. Coalesce back to DateTime.MinValue so the output matches
            // the legacy MVC5 app exactly (FirstOrDefault() on the non-nullable AtDate yielded MinValue).
            var LWD = db.DailyAttendanceDetails
                        .Where(z => z.EmployeeId == Employee && z.AtType == 4)
                        .OrderByDescending(t => t.AtDate)
                        .Select(t => (DateTime?)t.AtDate)
                        .FirstOrDefault() ?? DateTime.MinValue;

            var v = (from a in db.Employees
                     join b in db.EmployeeWorkDetails on a.EmployeeId equals b.EmployeeId into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.Designations on a.DesignationID equals c.DesignationID into des
                     from c in des.DefaultIfEmpty()
                     join d in db.SalaryStructures on a.EmployeeId equals d.EmployeeId into Salstr
                     from d in Salstr.DefaultIfEmpty()
                     let app = db.SalaryStrDetails.Where(x => x.SalaryStrId == d.SalaryStrId && x.PayHeadId == 1).Select(x => (long?)x.Rate).FirstOrDefault()
                     where (a.EmployeeId == Employee) //&& (e.PayHeadId==1)
                     select new FinalSettlementGetViewModel
                     {
                         JoiningDate = a.JoinDate,
                         LastworkingDate = LWD,
                         Reason = a.ReasonForLeaving,
                         Designation = c.DesignationName,
                         Basic = app,
                         JobStatus = a.JobStatus,
                         DeductionDays = 0,
                         Deduction = 0,
                         TotalDays = (a.TotalWorkingDays == null) ? 0 : a.TotalWorkingDays,
                         NoDaysWorked = (a.NoDaysWorked == null) ? 0 : a.NoDaysWorked,
                         Addition = 0,
                         NoDaysAbsent = db.DailyAttendanceDetails.Count(xx => xx.EmployeeId == Employee && xx.AtType == 1),
                         LastDutyReumeDate = a.LastDutyResumeDate,
                     }).ToList().Select(o => new
                     {
                         o.JoiningDate,
                         o.LastworkingDate,
                         o.Reason,
                         o.Designation,
                         o.Basic,
                         TypeofSettlement = Enum.GetName(typeof(JobStatus), o.JobStatus),
                         LastResumeDate = db.DailyAttendanceDetails.Where(x => x.EmployeeId == Employee && x.AtType == 4).OrderByDescending(t => t.AtDate).FirstOrDefault(),
                         o.LastDutyReumeDate,
                         Absent = (from x in db.DailyAttendanceDetails
                                   join y in db.DailyAttendances on x.DailyAttendanceId equals y.DailyAttendanceId
                                   join z in db.AttendanceTypes on x.AtType equals z.Id
                                   where x.EmployeeId == Employee
                                   && (EF.Functions.DateDiffDay(x.AtDate, o.JoiningDate) <= 0)
                                   && (EF.Functions.DateDiffDay(x.AtDate, o.LastworkingDate) >= 0)
                                   && z.Type == "2" //LEAVE WITHOUT PAY
                                   select new
                                   {
                                       x.DailyAttendanceDetailId
                                   }).Count(),

                         Present = (o.NoDaysWorked) + (from x in db.DailyAttendanceDetails
                                                       join y in db.DailyAttendances on x.DailyAttendanceId equals y.DailyAttendanceId
                                                       join z in db.AttendanceTypes on x.AtType equals z.Id
                                                       where x.EmployeeId == Employee
                                                       && (EF.Functions.DateDiffDay(x.AtDate, o.JoiningDate) <= 0)
                                                       && (EF.Functions.DateDiffDay(x.AtDate, o.LastworkingDate) >= 0)
                                                       && z.Id == 4 //LEAVE WITHOUT PAY
                                                       select new
                                                       {
                                                           x.DailyAttendanceDetailId
                                                       }).Count(),

                         Totaldays = (o.TotalDays) + (from x in db.DailyAttendanceDetails
                                                      where x.EmployeeId == Employee
                                                      && (EF.Functions.DateDiffDay(x.AtDate, o.JoiningDate) <= 0)
                                                      && (EF.Functions.DateDiffDay(x.AtDate, o.LastworkingDate) >= 0)
                                                      //&& z.Id == 4 //LEAVE WITHOUT PAY
                                                      select new
                                                      {
                                                          x.DailyAttendanceDetailId
                                                      }).Count(),

                     }).FirstOrDefault();
            var Grat = (from a in db.Payheads
                        join b in db.GratuityDetailss on a.ID equals b.Payhead
                        where a.Type == PayHeadType.Gratuity
                        select new
                        {
                            a.NameinSlip,
                            b.From,
                            To = (b.To == null) ? 0 : b.To,
                            b.Days,
                            a.GratuityDays
                        }).ToList();
            var Payrollstartdate = db.companys.Select(x => x.Payrolldate).FirstOrDefault();
            EmployeeAttendanceSummary previousdata = new EmployeeAttendanceSummary();
            if (Payrollstartdate < v.JoiningDate)
            {
                previousdata = db.EmployeeAttendanceSummarys.Where(x => x.EmployeeId == Employee).FirstOrDefault();
            }
            return Json(new { Main = v, Gratuity = Grat, Previous = previousdata });
        }

        [HttpGet]
        public JsonResult GetPayheads(long ID)
        {
            var ConD = (from a in db.PayheadFSs
                        join b in db.Payheads on a.PayheadId equals b.ID
                        where a.FinalSettlementId == ID
                        select new
                        {
                            Payhead = a.PayheadId,
                            Amount = a.Amount,
                            Type = b.Type,
                            Name = b.NameinSlip
                        }).ToList();
            return Json(ConD);
        }
    }
}