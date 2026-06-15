using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using CustomHtml;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;

namespace QuickSoft.Areas.Hr.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class PayrollReportController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PayrollReportController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Hr/PayrollReport
        [QkAuthorize(Roles = "Dev,PaySlip")]
        public ActionResult PaySlip()
        {
            ViewBag.AllOption = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "", Value = "0"},
                                }, "Value", "Text", 0);

            var empl = (from a in db.Employees
                        join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                        from d in emp.DefaultIfEmpty()
                        where d.EmployeeAccount != null && a.LeavingDate == null
                        select new
                        {
                            ID = a.EmployeeId,
                            Name = a.FirstName + " " + a.LastName
                        }).ToList();

            ViewBag.Emp = QkSelect.List(empl, "ID", "Name");

            return View();
        }
        [HttpPost]
        public ActionResult PaySlip(long ddlEmployee, string MonthYear)
        {
            var myear = MonthYear;
            MonthYear = "01-" + MonthYear;
            DateTime sdate = DateTime.Parse(MonthYear, new CultureInfo("en-GB").DateTimeFormat);
            var lastdate = DateTime.DaysInMonth(sdate.Year, sdate.Month);

            string lastMonthYear = lastdate + "-" + myear;
            DateTime edate = DateTime.Parse(lastMonthYear, new CultureInfo("en-GB").DateTimeFormat);

            PaySlipViewModel vmodel = new PaySlipViewModel();
            vmodel = (from a in db.Employees
                      join b in db.Contacts on a.CAddress equals b.ContactID into cont1
                      from b in cont1.DefaultIfEmpty()
                      join c in db.Contacts on a.PAddress equals c.ContactID into cont2
                      from c in cont2.DefaultIfEmpty()
                      join d in db.Users on a.UserId equals d.Id into usr
                      from d in usr.DefaultIfEmpty()
                      join e in db.Branchs on a.BranchID equals e.BranchID

                      join p in db.EmployeePersonals on a.EmployeeId equals p.EmployeeId into empper
                      from p in empper.DefaultIfEmpty()
                      join bk in db.EmployeeBanks on a.EmployeeId equals bk.EmployeeId into ban
                      from bk in ban.DefaultIfEmpty()

                      join wk in db.EmployeeWorkDetails on a.EmployeeId equals wk.EmployeeId into wrk
                      from wk in wrk.DefaultIfEmpty()

                      join de in db.Departments on a.DepartmentID equals de.DepartmentID into dep
                      from de in dep.DefaultIfEmpty()
                      join ds in db.Designations on a.DesignationID equals ds.DesignationID into desg
                      from ds in desg.DefaultIfEmpty()

                      join pvs in db.PayrollVoucherSalarys on a.EmployeeId equals pvs.EmployeeId into pays
                      from pvs in pays.DefaultIfEmpty()
                      join pv in db.PayrollVouchers on pvs.PayrollVoucherId equals pv.PayrollVoucherId into pay
                      from pv in pay.DefaultIfEmpty()


                      where a.EmployeeId == ddlEmployee && pv.FromDate == sdate && pv.ToDate == edate//&& pv.PayrollVoucherId == PayVID
                      select new PaySlipViewModel
                      {
                          Employee = a.FirstName + " " + a.LastName,
                          EmployeeCode = a.EMPCode,
                          Designation = ds.DesignationName,
                          DateofJoin = a.JoinDate,

                          BankName = bk.BankName,
                          AccountNo = bk.AccountNo,
                          IbanNo = bk.IbanNo,
                          BranchName = bk.BranchName,
                          Swift = bk.Swift,
                          MonthYear = sdate,
                          Note =pv.Note
                      }).FirstOrDefault();

            vmodel.Attend = (from a in db.DailyAttendanceDetails
                             join d in db.AttendanceTypes on a.AtType equals d.Id into attype
                             from d in attype.DefaultIfEmpty()
                             join at in db.DailyAttendances on a.DailyAttendanceId equals at.DailyAttendanceId
                             where a.EmployeeId == ddlEmployee && at.MonthYear.Month == sdate.Month && at.MonthYear.Year == sdate.Year
                             select new
                             {
                                 a.AtType,
                                 d.Name,
                                 d.PeriodType,
                             }).ToList().GroupBy(x => new { AtType = x.AtType }, (key, group) => new AttendDetail
                             {
                                 Name = group.Select(y => y.Name).FirstOrDefault(),
                                 Value = group.Select(y => y.Name).Count(),
                                 PeriodType = group.Select(y => y.PeriodType).FirstOrDefault()
                             }).ToList();

            vmodel.Earning = (from a in db.PayrollVoucherSalarys
                              join d in db.Payheads on a.PayHeadId equals d.ID into attype
                              from d in attype.DefaultIfEmpty()
                              join pv in db.PayrollVouchers on a.PayrollVoucherId equals pv.PayrollVoucherId
                              where a.EmployeeId == ddlEmployee && d.Type == PayHeadType.EarningsforEmployees && pv.FromDate == sdate
                              && pv.ToDate == edate && d.affectnetsalary == true
                              select new EarnDuductDetail
                              {
                                  Name = d.NameinSlip != null ? d.NameinSlip : d.Name,
                                  Amount = a.Rate,
                                  GrossSalary = a.Rate,
                              }).ToList();

            vmodel.Deduction = (from a in db.PayrollVoucherSalarys
                                join d in db.Payheads on a.PayHeadId equals d.ID into attype
                                from d in attype.DefaultIfEmpty()
                                join pv in db.PayrollVouchers on a.PayrollVoucherId equals pv.PayrollVoucherId
                                where a.EmployeeId == ddlEmployee && d.Type == PayHeadType.DeductionsfromEmployees && pv.FromDate == sdate
                                && pv.ToDate == edate && d.affectnetsalary == true
                                select new EarnDuductDetail
                                {
                                    Name = d.NameinSlip != null ? d.NameinSlip : d.Name,
                                    Amount = a.Rate,
                                    GrossSalary = a.Rate,
                                }).ToList();


            companySet();
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,PaySheet")] 
        public ActionResult PaySheet()
        {
            ViewBag.AllOption = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            companySet();
            return View();
        }
        public ActionResult GetPaySheet(long? empl, string monthyear)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            var myear = monthyear;
            monthyear = "01-" + monthyear;
            DateTime sdate = DateTime.Parse(monthyear, new CultureInfo("en-GB").DateTimeFormat);
            var lastdate = DateTime.DaysInMonth(sdate.Year, sdate.Month);

            string lastMonthYear = lastdate + "-" + myear;
            DateTime edate = DateTime.Parse(lastMonthYear, new CultureInfo("en-GB").DateTimeFormat);


            var v = (from a in db.PayrollVouchers
                     join b in db.PayrollVoucherEmployees on a.PayrollVoucherId equals b.PayrollVoucherId into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.Employees on b.EmployeeId equals c.EmployeeId into emp
                     from c in emp.DefaultIfEmpty()
                     join d in db.Departments on c.DepartmentID equals d.DepartmentID into deprt
                     from d in deprt.DefaultIfEmpty()
                     join e in db.Designations on c.DesignationID equals e.DesignationID into desg
                     from e in desg.DefaultIfEmpty()

                     where (empl == 0 || empl == null || b.EmployeeId == empl)
                     && a.FromDate == sdate && a.ToDate == edate
                     select new
                     {
                         Department = d != null ? d.DepartmentName : "",
                         Employee = c.FirstName + " " + c.LastName,
                         EmpCode = c.EMPCode,
                         Designation = e != null ? e.DesignationName : "",
                         MonthYear = monthyear,
                         Absent = (decimal?)(from x in db.DailyAttendanceDetails
                                             join y in db.DailyAttendances on x.DailyAttendanceId equals y.DailyAttendanceId
                                             where x.AtType == 1 && x.EmployeeId == b.EmployeeId
                                              && y.MonthYear.Month == sdate.Month && y.MonthYear.Year == sdate.Year
                                             select new
                                             {
                                                 x.DailyAttendanceDetailId
                                             }).Count() ?? 0,
                         AbsentType = db.AttendanceTypes.Where(a => a.Id == 1).Select(a => a.PeriodType).FirstOrDefault(),


                         OverTime = (decimal?)(from x in db.DailyAttendanceDetails
                                               join y in db.DailyAttendances on x.DailyAttendanceId equals y.DailyAttendanceId
                                               where x.EmployeeId == b.EmployeeId
                                                && y.MonthYear.Month == sdate.Month && y.MonthYear.Year == sdate.Year
                                               select new
                                               {
                                                   x.Overtime
                                               }).Sum(c => c.Overtime) ?? 0,

                         OverTimeType = db.AttendanceTypes.Where(a => a.Id == 3).Select(a => a.PeriodType).FirstOrDefault(),

                         Present = (decimal?)(from x in db.DailyAttendanceDetails
                                              join y in db.DailyAttendances on x.DailyAttendanceId equals y.DailyAttendanceId
                                              where x.AtType == 4 && x.EmployeeId == b.EmployeeId
                                               && y.MonthYear.Month == sdate.Month && y.MonthYear.Year == sdate.Year
                                              select new
                                              {
                                                  x.DailyAttendanceDetailId
                                              }).Count() ?? 0,
                         PresentType = db.AttendanceTypes.Where(a => a.Id == 4).Select(a => a.PeriodType).FirstOrDefault(),

                         Basic = (decimal?)(from x in db.SalaryStrDetails
                                            join y in db.SalaryStructures on x.SalaryStrId equals y.SalaryStrId
                                            where y.EmployeeId == b.EmployeeId
                                             && x.PayHeadId == 2
                                            select new
                                            {
                                                x.Rate
                                            }).Sum(c => c.Rate) ?? 0,
                     });


            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [QkAuthorize(Roles = "Dev,PayrollStatement")]
        public ActionResult PayrollStatement()
        {
            ViewBag.AllOption = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            companySet();
            return View();
        }
        public ActionResult GetPayrollStatement(long? ddlEmployee, string MonthYear)
        {
            PayrollStatementViewModel vmodel = new PayrollStatementViewModel();

            vmodel.Phead = db.Payheads.Where(a => a.Name != "Gratuity" && a.Name != "Earnings In Annual Leave"
                            && a.Name != "Deduction In Annual Leave").Distinct().ToList();

            vmodel.PheadCount = db.Payheads.Where(a => a.Name != "Gratuity" && a.Name != "Earnings In Annual Leave"
                              && a.Name != "Deduction In Annual Leave").Distinct().ToList().Count();
            companySet();


            var myear = MonthYear;
            MonthYear = "01-" + MonthYear;
            DateTime sdate = DateTime.Parse(MonthYear, new CultureInfo("en-GB").DateTimeFormat);
            var lastdate = DateTime.DaysInMonth(sdate.Year, sdate.Month);

            string lastMonthYear = lastdate + "-" + myear;
            DateTime edate = DateTime.Parse(lastMonthYear, new CultureInfo("en-GB").DateTimeFormat);

            vmodel.PSettle = (from a in db.PayrollVoucherEmployees
                              join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                              from b in emp.DefaultIfEmpty()
                              join c in db.PayrollVouchers on a.PayrollVoucherId equals c.PayrollVoucherId
                              where c.FromDate == sdate && c.ToDate == edate && (ddlEmployee == 0 || ddlEmployee == null || a.EmployeeId == ddlEmployee)
                              select new
                              {
                                  Department = b != null ? (from i in db.Departments
                                                            where i.DepartmentID == b.DepartmentID
                                                            select new
                                                            {
                                                                i.DepartmentName
                                                            }).FirstOrDefault().DepartmentName : "",
                                  Employee = b != null ? b.FirstName + " " + b.LastName : "",
                                  EmployeeCode = b != null ? b.EMPCode : "",
                                  BankDetails = b != null ? (from i in db.EmployeeBanks
                                                             where i.EmployeeId == b.EmployeeId
                                                             select new
                                                             {
                                                                 Banks = i.BankName + "," + i.BranchName + "," + i.AccountNo + "," + i.IbanNo + "," + i.Swift,
                                                             }).FirstOrDefault().Banks : null,

                                  Phead = (from aa in db.Payheads
                                           where aa.Name != "Gratuity" && aa.Name != "Earnings In Annual Leave"
                                           && aa.Name != "Deduction In Annual Leave"
                                           select new PayheadVModel
                                           {
                                               ID = aa.ID,
                                               Name = aa.Name,
                                               Rate = (decimal?)(from bb in db.PayrollVoucherSalarys
                                                                 join cc in db.PayrollVouchers on bb.PayrollVoucherId equals cc.PayrollVoucherId
                                                                 where bb.PayHeadId == aa.ID && cc.FromDate == sdate && cc.ToDate == edate && bb.EmployeeId == b.EmployeeId
                                                                 select new
                                                                 {
                                                                     Rate = bb.Rate
                                                                 }).FirstOrDefault().Rate ?? 0,

                                           }).ToList(),
                                  TotalDr = db.PayrollVoucherSalarys.Where(x => x.CrDr == "Dr" && x.PayrollEmployeeId == a.PayrollEmployeeId).Select(x => x.Rate).Sum() ?? 0,
                                  TotalCr = db.PayrollVoucherSalarys.Where(x => x.CrDr == "Cr" && x.PayrollEmployeeId == a.PayrollEmployeeId).Select(x => x.Rate).Sum() ?? 0,
                                  Note = c.Note
                              }).AsEnumerable().Select(o => new PSettleVModel
                              {
                                  Department = o.Department,
                                  Employee = o.Employee,
                                  EmployeeCode = o.EmployeeCode,
                                  BankDetails = o.BankDetails,
                                  PheadVModel = o.Phead,
                                  Total = o.TotalDr - o.TotalCr,
                                  Note = o.Note
                              }).ToList();

            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,PaymentAdvice")]
        public ActionResult PaymentAdvice()
        {
            ViewBag.AllOption = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                              }, "Value", "Text", 0);
            companySet();
            return View();
        }
        [HttpPost]
        public ActionResult PaymentAdvice(long ddlEmployee, string MonthYear)
        {
            var myear = MonthYear;
            MonthYear = "01-" + MonthYear;
            DateTime sdate = DateTime.Parse(MonthYear, new CultureInfo("en-GB").DateTimeFormat);
            var lastdate = DateTime.DaysInMonth(sdate.Year, sdate.Month);

            string lastMonthYear = lastdate + "-" + myear;
            DateTime edate = DateTime.Parse(lastMonthYear, new CultureInfo("en-GB").DateTimeFormat);


            PaymentAdviceViewModel vmodel = new PaymentAdviceViewModel();
            vmodel.MonthYearF = sdate;
            vmodel.MonthYearT = edate;

            var bankacc = db.companys.Select(a => a.BankAccount).FirstOrDefault();
            if (bankacc != null)
            {
                vmodel.BankName = db.Accountss.Where(a => a.AccountsID == bankacc).Select(a => a.Name).FirstOrDefault();
                vmodel.CompanyAccount = db.Banks.Where(a => a.AccountId == bankacc).Select(a => a.AccountNo).FirstOrDefault();
            }

            vmodel.PAdvice = (from a in db.PayrollVoucherEmployees
                              join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                              from b in emp.DefaultIfEmpty()
                              join c in db.PayrollVouchers on a.PayrollVoucherId equals c.PayrollVoucherId
                              join d in db.EmployeeBanks on a.EmployeeId equals d.EmployeeId into empbnk
                              from d in empbnk.DefaultIfEmpty()
                              let paydr = db.PayrollVoucherSalarys.Where(x => x.CrDr == "Dr" && x.EmployeeId == a.EmployeeId && x.PayrollVoucherId == a.PayrollVoucherId).Select(a => a.Rate).Sum() ?? 0
                              let paycr = db.PayrollVoucherSalarys.Where(x => x.CrDr == "Cr" && x.EmployeeId == a.EmployeeId && x.PayrollVoucherId == a.PayrollVoucherId).Select(a => a.Rate).Sum() ?? 0
                              where c.FromDate == sdate && c.ToDate == edate && (ddlEmployee == 0 || ddlEmployee == null || a.EmployeeId == ddlEmployee)
                              select new PAdviceViewModel
                              {
                                  EmpName = b.FirstName + " " + b.LastName,
                                  AccountNumber = d.AccountNo,
                                  BankName = d.BankName,
                                  Branch = d.BranchName,
                                  Amount = paydr - paycr
                              }).ToList();

            companySet();
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,PayrollRegister")]
        public ActionResult PayrollRegister()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetPayrollRegister()
        {
            var year = DateTime.Now.Year;

            Dictionary<string, int> months = new Dictionary<string, int>();
            months.Add("January", 1);
            months.Add("February", 2);
            months.Add("March", 3);
            months.Add("April", 4);
            months.Add("May", 5);
            months.Add("June", 6);
            months.Add("July", 7);
            months.Add("August", 8);
            months.Add("September", 9);
            months.Add("October", 10);
            months.Add("November", 11);
            months.Add("December", 12);

            var v = (from a in months
                     select new
                     {
                         Month = a.Key+' '+year,
                         MonthVal = a.Value,
                         VCount = (decimal?)(from i in db.PayrollVouchers
                                             where (i.FromDate.Month == a.Value
                                             && i.FromDate.Year == year || i.ToDate.Month == a.Value
                                             && i.ToDate.Year == year)
                                             select new
                                             {
                                                 i.PayrollVoucherId
                                             }).Count() ?? 0,

                     }).ToList();
            var data = v.ToList();
            
            return Json(new { data = data });
        }

        #region Payroll Details 
        public ActionResult PayrollDetails()
        {
            companySet();
            return View();
        }
        public ActionResult getPayrollDetails(long month)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var v = (from a in db.PayrollVouchers
                     join b in db.Accountss on a.Acccount equals b.AccountsID into acc
                     from b in acc.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     where a.FromDate.Month == month || a.ToDate.Month == month
                     select new
                     {
                         a.PayrollVoucherId,
                         a.VoucherNo,
                         a.FromDate,
                         a.ToDate,
                         a.PRDate,
                         Account = b.Name,
                         e.UserName,
                         a.CreatedDate
                     }).AsEnumerable().OrderBy(a => a.PRDate);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion


        #region Employee Pay Head Breakup
        [QkAuthorize(Roles = "Dev,EmpPayHeadBreakup")]
        public ActionResult EmpPayHeadBreakup()
        {
            ViewBag.AllOption = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);


            var phead = db.Payheads
              .Select(s => new
              {
                  Id = s.ID,
                  Name = s.Name
              }).ToList();
            ViewBag.PayHead = QkSelect.List(phead, "Id", "Name");

            companySet();
            return View();
        }
        public ActionResult GetEmpPayHeadBreakup(long ddlEmployee, long? ddlPayHead, string MonthYear)
        {
            PayrollStatementViewModel vmodel = new PayrollStatementViewModel();
            companySet();


            var myear = MonthYear;
            MonthYear = "01-" + MonthYear;
            DateTime sdate = DateTime.Parse(MonthYear, new CultureInfo("en-GB").DateTimeFormat);
            var lastdate = DateTime.DaysInMonth(sdate.Year, sdate.Month);

            string lastMonthYear = lastdate + "-" + myear;
            DateTime edate = DateTime.Parse(lastMonthYear, new CultureInfo("en-GB").DateTimeFormat);

            vmodel.EmpPhead = (from a in db.PayrollVoucherSalarys
                               join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                               from b in emp.DefaultIfEmpty()
                               join c in db.PayrollVouchers on a.PayrollVoucherId equals c.PayrollVoucherId
                               join d in db.Departments on b.DepartmentID equals d.DepartmentID into dept
                               from d in dept.DefaultIfEmpty()
                               join e in db.Payheads on a.PayHeadId equals e.ID into phead
                               from e in phead.DefaultIfEmpty()
                               join f in db.Accountss on e.Account equals f.AccountsID into accid
                               from f in accid.DefaultIfEmpty()
                               where a.PayHeadId == ddlPayHead && c.FromDate == sdate && c.ToDate == edate && (ddlEmployee == 0 || ddlEmployee == null || a.EmployeeId == ddlEmployee)
                               select new EmpPayHeadBreakupViewModel
                               {
                                   Department = d.DepartmentName,
                                   Employee = b.FirstName + " " + b.LastName,
                                   OpeningBalance = f.OpnBalance,
                                   Debit = a.CrDr == "Dr" ? a.Rate : 0,
                                   Credit = a.CrDr == "Cr" ? a.Rate : 0,
                                   ClosingBalance = a.Rate,
                                   CrDr = a.CrDr
                               }).OrderBy(a => a.Department).ToList();

            return View(vmodel);
        }

        #endregion

        #region  Pay Head Employee Breakup
        [QkAuthorize(Roles = "Dev,PayHeadEmpBreakup")]
        public ActionResult PayHeadEmpBreakup()
        {
            var empl = (from a in db.Employees
                        join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                        from d in emp.DefaultIfEmpty()
                        where d.EmployeeAccount != null && a.LeavingDate == null
                        select new
                        {
                            ID = a.EmployeeId,
                            Name = a.FirstName + " " + a.LastName
                        }).ToList();

            ViewBag.Emp = QkSelect.List(empl, "ID", "Name");

            ViewBag.AllOption = QkSelect.List(
                                           new List<SelectListItem>
                                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                           }, "Value", "Text", 0);

            companySet();
            return View();
        }
        public ActionResult GetPayHeadEmpBreakup(long ddlEmployee, string MonthYear)
        {
            PayrollStatementViewModel vmodel = new PayrollStatementViewModel();
            companySet();


            var myear = MonthYear;
            MonthYear = "01-" + MonthYear;
            DateTime sdate = DateTime.Parse(MonthYear, new CultureInfo("en-GB").DateTimeFormat);
            var lastdate = DateTime.DaysInMonth(sdate.Year, sdate.Month);

            string lastMonthYear = lastdate + "-" + myear;
            DateTime edate = DateTime.Parse(lastMonthYear, new CultureInfo("en-GB").DateTimeFormat);

            vmodel.EmpPhead = (from a in db.PayrollVoucherSalarys
                               join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                               from b in emp.DefaultIfEmpty()
                               join c in db.PayrollVouchers on a.PayrollVoucherId equals c.PayrollVoucherId
                               join d in db.Departments on b.DepartmentID equals d.DepartmentID into dept
                               from d in dept.DefaultIfEmpty()
                               join e in db.Payheads on a.PayHeadId equals e.ID into phead
                               from e in phead.DefaultIfEmpty()
                               join f in db.Accountss on e.Account equals f.AccountsID into accid
                               from f in accid.DefaultIfEmpty()
                               where c.FromDate == sdate && c.ToDate == edate && (ddlEmployee == 0 || ddlEmployee == null || a.EmployeeId == ddlEmployee)
                               select new EmpPayHeadBreakupViewModel
                               {
                                   Debit = a.CrDr == "Dr" ? a.Rate : 0,
                                   Credit = a.CrDr == "Cr" ? a.Rate : 0,
                                   ClosingBalance = a.Rate,
                                   CrDr = a.CrDr,

                                   PayHead = e.Name,
                                   AccountName = db.AccountsGroups.Where(a => a.AccountsGroupID == f.Group).Select(a => a.Name).FirstOrDefault(),
                                   AccGroup = f.Group
                               }).OrderBy(a => a.AccGroup).ToList();

            return View(vmodel);
        }

        #endregion


        #region final Settlement
        [QkAuthorize(Roles = "Dev,FinalSettlement")]
        public ActionResult FinalSettlement()
        {
            ViewBag.AllOption = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            companySet();
            return View();
        }
        [HttpPost]
        public ActionResult GetFinalSettlement(long? empl, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            var v = (from b in db.FinalSettlements
                     join c in db.Employees on b.Employee equals c.EmployeeId into cust
                     from c in cust.DefaultIfEmpty()
                     where (empl == 0 || empl == null || b.Employee == empl)
                     && (fromdate == "" || fromdate == null || EF.Functions.DateDiffDay(b.Date, fdate) <= 0)
                     && (todate == "" || fromdate == null || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
                     select new
                     {
                         b.FinalSettlementID,
                        Employee= c.FirstName + " " + c.MiddleName + " " + c.LastName,
                        Date=b.Date,
                        gratuity=b.GratuityAmount,
                        netamount=b.NetAmount,
                        designation=b.Designation,
                        joindate=b.JoiningDate,
                        lastdate=b.LastworkingDate,
                        typeofsettlement=c.JobStatus,
                        created= db.Users.Where(s => s.Id == b.CreatedBy).Select(s => s.UserName).FirstOrDefault(),
                     }).OrderBy(a => a.Date);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion

        #region final Settlement
        [QkAuthorize(Roles = "Dev,LeaveSettlement")]
        public ActionResult LeaveSettlement()
        {
            ViewBag.AllOption = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            companySet();
            return View();
        }
        [HttpPost]
        public ActionResult GetLeaveSettlement(long? empl, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            var v = (from b in db.LeaveSettlements
                     join c in db.Employees on b.Employee equals c.EmployeeId into cust
                     from c in cust.DefaultIfEmpty()
                     where (empl == 0 || empl == null || b.Employee == empl)
                     && (fromdate == "" || fromdate == null || EF.Functions.DateDiffDay(b.Date, fdate) <= 0)
                     && (todate == "" || fromdate == null || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
                     select new
                     {
                         b.LeaveSettlementID,
                         Employee = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                         Date = b.Date,
                         typeofsettlement = c.JobStatus,
                         expecteddate=b.ExpectedResumeDate,
                         leavestart=b.LeaveStartDate,
                         leavesalary=b.LeaveSalary,
                         created = db.Users.Where(s => s.Id == b.CreatedBy).Select(s => s.UserName).FirstOrDefault(),
                     }).OrderBy(a => a.Date);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion
    }
}