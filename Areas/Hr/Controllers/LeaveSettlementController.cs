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
    public class LeaveSettlementController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public LeaveSettlementController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [RedirectingAction]

        public ActionResult Index()
        {
            return View();
        }
        [RedirectingAction]
        [HttpPost]

        public ActionResult GetLeaveSettlement(long? BName, long? Item, decimal? Qty, long? Unit)
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

            var v = (from a in db.LeaveSettlements
                     join b in db.Employees on a.Employee equals b.EmployeeId into emp
                     from b in emp.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     select new
                     {
                         a.LeaveSettlementID,
                         EmpName = b.FirstName + " " + b.LastName,
                         a.Date,
                         a.LeaveStartDate,
                         a.ExpectedResumeDate,
                         a.CreatedDate,
                         e.UserName,
                         a.DutyResumeDate
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.EmpName.ToString().ToLower().Contains(search.ToLower()));
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

        public ActionResult Create()
        {
            LeaveSettlementViewModel vmodel = new LeaveSettlementViewModel();
            vmodel.Date = System.DateTime.Now.ToString("dd-MM-yyyy");

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

            vmodel.FieldPayHead = db.Payheads.Where(a => a.Type == PayHeadType.EarningsInAnnualLeave || a.Type == PayHeadType.DeductionInAnnualLeave).ToList();

            return View(vmodel);
        }

        [HttpPost]

        public JsonResult Create(LeaveSettlementViewModel vmodel, string fnval)
        {
            string msg;
            bool stat;
            DateTime LSdate = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
            var Exists = db.LeaveSettlements.Any(c => c.Employee == vmodel.Employee && c.Date.Year == LSdate.Year);
            if (1==2)
            {
                msg = "Leave Settlement already exists in Selected year..!!";
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

                LeaveSettlement attend = new LeaveSettlement();

                attend.Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                attend.Remarks = vmodel.Remarks;
                attend.Employee = vmodel.Employee;
                if (vmodel.LeaveStartDate != null)
                {
                    attend.LeaveStartDate = DateTime.Parse(vmodel.LeaveStartDate.ToString(), new CultureInfo("en-GB"));
                }
                if (vmodel.ExpectedResumeDate != null)
                {
                    attend.ExpectedResumeDate = DateTime.Parse(vmodel.ExpectedResumeDate.ToString(), new CultureInfo("en-GB"));
                }

                attend.TotalWorkingDays = vmodel.TotalWorkingDays;
                attend.NoDaysWorked = vmodel.DaysWorked;
                attend.LeaveEntitled = vmodel.LeaveEntitled;
                attend.LeaveSalary = vmodel.LeaveSalary;
                attend.Netamount = vmodel.NetAmount;
                attend.Branch = Branch;
                attend.CreatedDate = today;
                attend.CreatedBy = UserId;
                attend.Status = Status.active;

                db.LeaveSettlements.Add(attend);
                db.SaveChanges();
                Int64 LSId = attend.LeaveSettlementID;
                LeaveSettlementPayHead lsphead = new LeaveSettlementPayHead();
                if (vmodel.Additiondetails != null)
                {
                    foreach (var arr in vmodel.Additiondetails)
                    {
                        if (arr.Amount != null)
                        {
                            lsphead.LeaveSettlementID = LSId;
                            lsphead.PayHeadID = Convert.ToInt64(arr.Payhead);
                            lsphead.PayHeadAmt = Convert.ToInt64(arr.Amount);
                            db.LeaveSettlementPayHeads.Add(lsphead);
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
                            lsphead.LeaveSettlementID = LSId;
                            lsphead.PayHeadID = Convert.ToInt64(arr.Payhead);
                            lsphead.PayHeadAmt = Convert.ToInt64(arr.Amount);
                            db.LeaveSettlementPayHeads.Add(lsphead);
                            db.SaveChanges();
                        }
                    }
                }

                Employee emp = db.Employees.Find(vmodel.Employee);
                emp.JobStatus = JobStatus.OnLeave;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();

                com.addlog(LogTypes.Created, UserId, "LeaveSettlement", "LeaveSettlements", findip(), LSId, "Successfully Submitted Leave Settlement");
                if ((fnval) == "print")
                {
                    //var data = vmodel;
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel;
                    var LastDutyResumeDate = DateTime.Parse(vmodel.LastDutyResumeDate.ToString(), new CultureInfo("en-GB"));
                    var Data = (from a in db.LeaveSettlements
                                join b in db.Employees on a.Employee equals b.EmployeeId into empl
                                from b in empl.DefaultIfEmpty()
                                join e in db.Users on a.CreatedBy equals e.Id
                                where (a.LeaveSettlementID == LSId)
                                select new
                                {
                                    a.LeaveSettlementID,
                                    EmpName = b.FirstName + " " + b.LastName,
                                    a.Date,
                                    a.LeaveStartDate,
                                    a.ExpectedResumeDate,
                                    a.CreatedDate,
                                    e.UserName,
                                    a.DutyResumeDate,
                                    a.Netamount,
                                    a.NoDaysWorked,
                                    a.Remarks,
                                    a.TotalWorkingDays,
                                    b.EMPCode,
                                    a.LeaveEntitled,
                                    a.LeaveSalary,
                                    LastDutyResumeDate = vmodel.LastDutyResumeDate,
                                    Addition = (from a in db.LeaveSettlementPayHeads
                                                join b in db.Payheads on a.PayHeadID equals b.ID into item
                                                from b in item.DefaultIfEmpty()
                                                where a.LeaveSettlementID == LSId && b.Type == PayHeadType.EarningsInAnnualLeave
                                                select
                                                   a.PayHeadAmt
                                              ).Sum(),
                                    Deduction = (from a in db.LeaveSettlementPayHeads
                                                 join b in db.Payheads on a.PayHeadID equals b.ID into item
                                                 from b in item.DefaultIfEmpty()
                                                 where a.LeaveSettlementID == LSId && b.Type == PayHeadType.DeductionInAnnualLeave
                                                 select
                                                    a.PayHeadAmt
                                                 ).Sum(),

                                }).FirstOrDefault();
                    var Additionlist = (from a in db.LeaveSettlementPayHeads
                                        join b in db.Payheads on a.PayHeadID equals b.ID into item
                                        from b in item.DefaultIfEmpty()
                                        where a.LeaveSettlementID == LSId && b.Type == PayHeadType.EarningsInAnnualLeave
                                        select new
                                        {
                                            b.NameinSlip,
                                            a.PayHeadAmt
                                        }).ToList();
                    var Deductionlist = (from a in db.LeaveSettlementPayHeads
                                         join b in db.Payheads on a.PayHeadID equals b.ID into item
                                         from b in item.DefaultIfEmpty()
                                         where a.LeaveSettlementID == LSId && b.Type == PayHeadType.DeductionInAnnualLeave
                                         select new
                                         {
                                             b.NameinSlip,
                                             a.PayHeadAmt
                                         }).ToList();
                    var arr = new ArrayList();
                    arr.Add(Data);
                    //var fmapp = db.FieldMappings.Where(a => a.Section == "PropertyRegistration" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully Updated Leave Settlement details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, DedList = Deductionlist, AddList = Additionlist, ComHeadCheck } };
                }
                else
                {
                    msg = "Successfully added Leave Settlement details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

            }
        }

        [RedirectingAction]

        [HttpGet]
        public ActionResult Edit(long? id)
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var UserId = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaveSettlement leavesett = db.LeaveSettlements.Find(id);

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var use = db.Employees
                           .Select(s => new
                           {
                               ID = s.EmployeeId,
                               Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                           })
                           .ToList();
            ViewBag.Employ = QkSelect.List(use, "ID", "Name");

            if (leavesett == null)
            {
                return NotFound();
            }

            LeaveSettlementViewModel vmodel = new LeaveSettlementViewModel();
            vmodel.LeaveSettlementID = (long)id;
            vmodel.Remarks = leavesett.Remarks;
            vmodel.Employee = leavesett.Employee;

            vmodel.TotalWorkingDays = leavesett.TotalWorkingDays;
            vmodel.DaysWorked = leavesett.NoDaysWorked;
            vmodel.LeaveEntitled = leavesett.LeaveEntitled;
            vmodel.LeaveSalary = leavesett.LeaveSalary;
            vmodel.Date = leavesett.Date.ToString("dd-MM-yyyy");
            vmodel.NetAmount = leavesett.Netamount;
            //vmodel.BasicSalary=db.sal
            vmodel.LeaveStartDate = leavesett.LeaveStartDate != null ? Convert.ToDateTime(leavesett.LeaveStartDate).ToString("dd-MM-yyyy") : "";
            vmodel.ExpectedResumeDate = leavesett.ExpectedResumeDate != null ? Convert.ToDateTime(leavesett.ExpectedResumeDate).ToString("dd-MM-yyyy") : "";

            var empl = db.Employees.Where(a => a.EmployeeId == leavesett.Employee).FirstOrDefault();
            if (empl != null)
            {
                vmodel.JoiningDate = empl.JoinDate;
                vmodel.LastDutyResumeDate = leavesett.DutyResumeDate != null ? Convert.ToDateTime(leavesett.DutyResumeDate).ToString("dd-MM-yyyy") : Convert.ToDateTime(empl.JoinDate).ToString("dd-MM-yyyy");
            }

            vmodel.payheaddetail = (from b in db.LeaveSettlementPayHeads
                                    where b.LeaveSettlementID == id
                                    select new SalaryStrDetailsViewModel
                                    {
                                        PayHeadId = b.PayHeadID,
                                        Rate = b.PayHeadAmt,
                                    }).ToList();


            vmodel.FieldPayHead = db.Payheads.Where(a => a.Type == PayHeadType.EarningsInAnnualLeave || a.Type == PayHeadType.DeductionInAnnualLeave).ToList();

            var v = (from a in db.SalaryStructures
                     join b in db.SalaryStrDetails on a.SalaryStrId equals b.SalaryStrId into emp
                     from b in emp.DefaultIfEmpty()
                     where (a.EmployeeId == leavesett.Employee && b.PayHeadId == 1)
                     select new
                     {
                         b.Rate
                     }).FirstOrDefault();
            if (v != null)
                vmodel.BasicSalary = v.Rate;
            else
                vmodel.BasicSalary = 0;
            return View(vmodel);
        }
        [HttpPost]
        [RedirectingAction]
 
        public ActionResult Edit(LeaveSettlementViewModel vmodel, string fnval)
        {
            bool stat = false;
            string msg;
            DateTime LSdate = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
            var Exists = db.LeaveSettlements.Any(c => c.Employee == vmodel.Employee && c.Date.Year == LSdate.Year && c.LeaveSettlementID != vmodel.LeaveSettlementID);
            if (Exists)
            {
                msg = "Leave Settlement already exists in Selected year..!!";
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
                LeaveSettlement leaveset = db.LeaveSettlements.Find(vmodel.LeaveSettlementID);

                if (BranchCheck == Status.active)
                {
                    Branch = (long)vmodel.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }


                leaveset.Employee = vmodel.Employee;
                leaveset.Remarks = vmodel.Remarks;
                leaveset.Branch = Branch;
                leaveset.Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                if (vmodel.LeaveStartDate != null)
                {
                    leaveset.LeaveStartDate = DateTime.Parse(vmodel.LeaveStartDate.ToString(), new CultureInfo("en-GB"));
                }
                if (vmodel.ExpectedResumeDate != null)
                {
                    leaveset.ExpectedResumeDate = DateTime.Parse(vmodel.ExpectedResumeDate.ToString(), new CultureInfo("en-GB"));
                }

                leaveset.TotalWorkingDays = vmodel.TotalWorkingDays;
                leaveset.NoDaysWorked = vmodel.DaysWorked;
                leaveset.LeaveEntitled = vmodel.LeaveEntitled;
                leaveset.LeaveSalary = vmodel.LeaveSalary;
                leaveset.Netamount = vmodel.NetAmount;

                db.Entry(leaveset).State = EntityState.Modified;
                db.SaveChanges();
                Int64 LeaveSetId = leaveset.LeaveSettlementID;



                var leapay = db.LeaveSettlementPayHeads.Where(a => a.LeaveSettlementID == LeaveSetId).FirstOrDefault();
                if (leapay != null)
                {
                    db.LeaveSettlementPayHeads.RemoveRange(db.LeaveSettlementPayHeads.Where(a => a.LeaveSettlementID == LeaveSetId));
                    db.SaveChanges();
                }
                LeaveSettlementPayHead lsphead = new LeaveSettlementPayHead();
                if (vmodel.Additiondetails != null)
                {
                    foreach (var arr in vmodel.Additiondetails)
                    {
                        if (arr.Amount != null)
                        {
                            lsphead.LeaveSettlementID = LeaveSetId;
                            lsphead.PayHeadID = Convert.ToInt64(arr.Payhead);
                            lsphead.PayHeadAmt = Convert.ToInt64(arr.Amount);
                            db.LeaveSettlementPayHeads.Add(lsphead);
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
                            lsphead.LeaveSettlementID = LeaveSetId;
                            lsphead.PayHeadID = Convert.ToInt64(arr.Payhead);
                            lsphead.PayHeadAmt = Convert.ToInt64(arr.Amount);
                            db.LeaveSettlementPayHeads.Add(lsphead);
                            db.SaveChanges();
                        }
                    }
                }


                Employee emp = db.Employees.Find(vmodel.Employee);
                emp.JobStatus = JobStatus.OnLeave;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();


                com.addlog(LogTypes.Updated, UserId, "LeaveSettlement", "LeaveSettlements", findip(), LeaveSetId, "Successfully Updated Leave Settlement");
                if ((fnval) == "print")
                {
                    //var data = vmodel;
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel;
                    // EF Core 10: same fix as FinalSettlement — the GroupBy-by-bool + Select(FirstOrDefault entity)
                    // is untranslatable. Rewrite as a direct ordered scalar subquery; cast (DateTime?) to avoid the
                    // COALESCE(...,'0001-01-01') datetime overflow, and coalesce to MinValue for legacy parity.
                    var LWD = db.DailyAttendanceDetails
                                .Where(z => z.EmployeeId == vmodel.Employee && z.AtType == 4)
                                .OrderByDescending(t => t.AtDate)
                                .Select(t => (DateTime?)t.AtDate)
                                .FirstOrDefault() ?? DateTime.MinValue;
                    var LSD = DateTime.Parse(vmodel.LeaveStartDate, new CultureInfo("en-GB").DateTimeFormat);
                    var LastDutyResumeDate= DateTime.Parse(vmodel.LastDutyResumeDate.ToString(), new CultureInfo("en-GB"));
                    var Data = (from a in db.LeaveSettlements
                                join b in db.Employees on a.Employee equals b.EmployeeId into empl
                                from b in empl.DefaultIfEmpty()
                                join e in db.Users on a.CreatedBy equals e.Id
                                where (a.LeaveSettlementID == vmodel.LeaveSettlementID)
                                select new
                                {
                                    a.LeaveSettlementID,
                                    EmpName = b.FirstName + " " + b.LastName,
                                    JoiningDate = b.JoinDate,
                                    LastworkingDate = LWD,

                                    a.Date,
                                    a.LeaveStartDate,
                                    a.ExpectedResumeDate,
                                    a.CreatedDate,
                                    e.UserName,
                                    a.DutyResumeDate,
                                    a.Netamount,
                                    a.NoDaysWorked,
                                    a.Remarks,
                                    a.TotalWorkingDays,
                                    b.EMPCode,
                                    a.LeaveEntitled,
                                    a.LeaveSalary,
                                    LastResumeDate=vmodel.LastDutyResumeDate,
                                    bsalary = (from aa in db.SalaryStructures
                                               join bb in db.SalaryStrDetails on aa.SalaryStrId equals bb.SalaryStrId into sal
                                               from bb in sal.DefaultIfEmpty()
                                               let payh = db.Payheads.Where(a => a.Name == "Basic Pay").Select(a => a.ID).FirstOrDefault()
                                               where aa.EmployeeId == b.EmployeeId && bb.PayHeadId == payh
                                               select new
                                               {
                                                   bb.Rate
                                               }).FirstOrDefault(),
                                    Addition = (from a in db.LeaveSettlementPayHeads
                                                join b in db.Payheads on a.PayHeadID equals b.ID into item
                                                from b in item.DefaultIfEmpty()
                                                where a.LeaveSettlementID == vmodel.LeaveSettlementID && b.Type == PayHeadType.EarningsInAnnualLeave
                                                select
                                                   a.PayHeadAmt
                                              ).Sum(),
                                    Deduction = (from a in db.LeaveSettlementPayHeads
                                                 join b in db.Payheads on a.PayHeadID equals b.ID into item
                                                 from b in item.DefaultIfEmpty()
                                                 where a.LeaveSettlementID == vmodel.LeaveSettlementID && b.Type == PayHeadType.DeductionInAnnualLeave
                                                 select
                                                    a.PayHeadAmt
                                                 ).Sum(),
                                    Absent = (from x in db.DailyAttendanceDetails
                                              join y in db.DailyAttendances on x.DailyAttendanceId equals y.DailyAttendanceId
                                              join z in db.AttendanceTypes on x.AtType equals z.Id
                                              where x.EmployeeId == vmodel.Employee
                                              && (EF.Functions.DateDiffDay(x.AtDate, LastDutyResumeDate) <= 0)
                                              && (EF.Functions.DateDiffDay(x.AtDate, LSD) >= 0)
                                              && z.Type == "2" //LEAVE WITHOUT PAY
                                              select new
                                              {
                                                  x.DailyAttendanceDetailId
                                              }).Count(),
                                }).FirstOrDefault();

                    var Additionlist = (from a in db.LeaveSettlementPayHeads
                                        join b in db.Payheads on a.PayHeadID equals b.ID into item
                                        from b in item.DefaultIfEmpty()
                                        where a.LeaveSettlementID == vmodel.LeaveSettlementID && b.Type == PayHeadType.EarningsInAnnualLeave
                                        select new
                                        {
                                            b.NameinSlip,
                                            a.PayHeadAmt
                                        }).ToList();
                    var Deductionlist = (from a in db.LeaveSettlementPayHeads
                                         join b in db.Payheads on a.PayHeadID equals b.ID into item
                                         from b in item.DefaultIfEmpty()
                                         where a.LeaveSettlementID == vmodel.LeaveSettlementID && b.Type == PayHeadType.DeductionInAnnualLeave
                                         select new
                                         {
                                             b.NameinSlip,
                                             a.PayHeadAmt
                                         }).ToList();
                    //var resendleave = db.LeaveSettlements.Where(x => x.Employee == vmodel.Employee && x.LeaveSettlementID != vmodel.LeaveSettlementID).OrderByDescending(t => t.LeaveSettlementID).FirstOrDefault();
                    //if (resendleave != null)
                    //{
                    //    //Previous Resume Date
                    //    var PreviousResumeDate = (resendleave.DutyResumeDate == null) ? resendleave.ExpectedResumeDate : resendleave.DutyResumeDate;
                    //    //Emp Join Date
                    //    var joindate = db.Employees.Where(x => x.EmployeeId == vmodel.Employee).Select(q => q.JoinDate).FirstOrDefault();
                    //    // Last resume or join date
                    //    var ResDate = ((DateTime)PreviousResumeDate==null)?(DateTime)joindate: (DateTime)PreviousResumeDate;
                    //    //total month worked after leave if
                    //    var totmonth = ((Data.LastworkingDate.Year - ResDate.Year) * 12) + Data.LastworkingDate.Month - ResDate.Month;
                    //    //Salary given details
                    //    var salarygiven = (from x in db.SalaryStructures
                    //                       where x.EmployeeId == vmodel.Employee
                    //                       && (EF.Functions.DateDiffDay(x.EFDate, vmodel.LastDutyResumeDate) <= 0)
                    //                       && (EF.Functions.DateDiffDay(x.EFDate, LSD) >= 0)
                    //                       select new
                    //                       {
                    //                           Year = x.EFDate.Year,
                    //                           Month = x.EFDate.Month
                    //                       }).ToList();
                    //    //count of months salary given in this period
                    //    var salarygivenmonths = (salarygiven).Count();
                    //    var resumeyear = ResDate.Year;
                    //    var resumemonth = ResDate.Month;
                    //    if (totmonth != salarygivenmonths)
                    //    {
                    //        for(var i=0;totmonth>salarygivenmonths;i++)
                    //        {


                    //            salarygivenmonths++;
                    //        }
                    //    }
                    //}

                    var arr = new ArrayList();
                    arr.Add(Data);
                    //var fmapp = db.FieldMappings.Where(a => a.Section == "PropertyRegistration" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully Updated Leave Settlement details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, DedList = Deductionlist, AddList = Additionlist, ComHeadCheck } };
                }
                else
                {
                    msg = "Successfully Updated Leave Settlement details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
        }



        [RedirectingAction]
   
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LeaveSettlement leavset = db.LeaveSettlements.Find(id);
            if (leavset == null)
            {
                return NotFound();
            }
            return PartialView(leavset);
        }

        [RedirectingAction]

        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            LeaveSettlement leaveset = db.LeaveSettlements.Find(id);

            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Leave Settlement.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete LeaveSettlement")]
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
                Success("Deleted " + count + " Leave Settlement, Unable to Delete " + notdel + " Leave Settlement. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Leave Settlement.", true);
            }
            else
            {
                Success("Deleted " + count + " Leave Settlement.", true);
            }
            return RedirectToAction("Index", "Leave Settlement");
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
            LeaveSettlement leaveset = db.LeaveSettlements.Find(id);
            if (leaveset != null)
            {
                db.LeaveSettlements.RemoveRange(db.LeaveSettlements.Where(a => a.LeaveSettlementID == id));
                db.SaveChanges();
            }
            var leapay = db.LeaveSettlementPayHeads.Where(a => a.LeaveSettlementID == id).FirstOrDefault();
            if (leapay != null)
            {
                db.LeaveSettlementPayHeads.RemoveRange(db.LeaveSettlementPayHeads.Where(a => a.LeaveSettlementID == id));
                db.SaveChanges();
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "LeaveSettlement", "LeaveSettlements", findip(), id, "Successfully Deleted Leave Settlement");
            db.SaveChanges();
            return true;
        }


        public ActionResult UpdateResumeDate(long id)
        {
            UpdateResumeDateViewModel vmodel = new UpdateResumeDateViewModel();
            vmodel.LeaveSettlementID = id;
            return PartialView(vmodel);
        }
        [HttpPost]
        public JsonResult UpdateResumeDate(UpdateResumeDateViewModel vmodel)
        {
            bool stat = false;
            string msg;

            LeaveSettlement leaveset = db.LeaveSettlements.Find(vmodel.LeaveSettlementID);
            leaveset.DutyResumeDate = DateTime.Parse(vmodel.DutyResumeDate, new CultureInfo("en-GB"));
            db.Entry(leaveset).State = EntityState.Modified;
            db.SaveChanges();

            Employee emp = db.Employees.Find(leaveset.Employee);
            emp.DutyResumeDate = DateTime.Parse(vmodel.DutyResumeDate, new CultureInfo("en-GB"));
            db.Entry(emp).State = EntityState.Modified;
            db.SaveChanges();

            msg = "Successfully Updated Resume Date..!!";
            stat = true;

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        public JsonResult GetPayheads(long ID)
        {
            var ConD = (from a in db.LeaveSettlementPayHeads
                        join b in db.Payheads on a.PayHeadID equals b.ID
                        where a.LeaveSettlementID == ID
                        select new
                        {
                            Payhead = a.PayHeadID,
                            Amount = a.PayHeadAmt,
                            Type = b.Type,
                            Name = b.NameinSlip
                        }).ToList();
            return Json(ConD);
        }
    }
}