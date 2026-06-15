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
    public class AttendanceReportController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AttendanceReportController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Hr/AttendanceReport
        public ActionResult AttendanceSheet()
        {
            ViewBag.AllOption = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);
            return View();
        }
        public ActionResult AttendanceGps()
        {
            ViewBag.AllOption = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);
            return View();
        }
        [HttpPost]
        public ActionResult GetAttendanceSheet(long? Emp, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            //var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            //var MCList = MCList1;
            //if (!MCList.Any() && (MC == 0 || MC == 1))
            //{
            //    MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            //}
            //var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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

            var datas =(from a in db.DailyAttendanceDetails
                          join b in db.Employees on a.EmployeeId equals b.EmployeeId into emps
                          from b in emps.DefaultIfEmpty()
                          join c in db.Departments on b.DepartmentID equals c.DepartmentID into dpart
                          from c in dpart.DefaultIfEmpty()
                          join d in db.DailyAttendances on a.DailyAttendanceId equals d.DailyAttendanceId
                          where (Emp == 0 || Emp == null || a.EmployeeId == Emp)
                                && (fromdate == "" || EF.Functions.DateDiffDay(a.AtDate, fdate) <= 0)
                                && (todate == "" || EF.Functions.DateDiffDay(a.AtDate, tdate) >= 0)
                          select new
                          {
                              Depart = c.DepartmentName,
                              EmployeeId = a.EmployeeId,
                              Employee = b.FirstName + " " + b.LastName,
                             a.AtType,
                             
                              //a.Unit,

                              //absent
                               absent = (decimal?)(from i in db.DailyAttendanceDetails
                                            join j in db.DailyAttendances on i.DailyAttendanceId equals j.DailyAttendanceId
                                            where (Emp == 0 || Emp == null || i.EmployeeId == Emp)
                                                  && (fromdate == "" || EF.Functions.DateDiffDay(i.AtDate, fdate) <= 0)
                                                  && (todate == "" || EF.Functions.DateDiffDay(i.AtDate, tdate) >= 0)
                                                  && i.DailyAttendanceId == j.DailyAttendanceId && i.AtType == 1 &&
                                                  i.EmployeeId==b.EmployeeId
                                     
                                         select new
                                         {
                                             a.AtType
                                         }).Count(),

                              //overtime
                              overtime = (decimal?)(decimal?)(from i in db.DailyAttendanceDetails
                                                              join j in db.DailyAttendances on i.DailyAttendanceId equals j.DailyAttendanceId
                                                              where (Emp == 0 || Emp == null || i.EmployeeId == Emp)
                                                                    && (fromdate == "" || EF.Functions.DateDiffDay(i.AtDate, fdate) <= 0)
                                                                    && (todate == "" || EF.Functions.DateDiffDay(i.AtDate, tdate) >= 0)
                                                                    && i.DailyAttendanceId == j.DailyAttendanceId && i.AtType == 4
                                                              group i by i.DailyAttendanceId into g
                                                              select new
                                                              {
                                                                  Total = g.Sum(o=>o.Overtime)
                                                              }).FirstOrDefault().Total ?? 0,

                              //present
                              present = (decimal?)(from i in db.DailyAttendanceDetails
                                                             join j in db.DailyAttendances on i.DailyAttendanceId equals j.DailyAttendanceId
                                                             where (Emp == 0 || Emp == null || i.EmployeeId == Emp)
                                                                   && (fromdate == "" || EF.Functions.DateDiffDay(i.AtDate, fdate) <= 0)
                                                                   && (todate == "" || EF.Functions.DateDiffDay(i.AtDate, tdate) >= 0)
                                                                   && i.DailyAttendanceId == j.DailyAttendanceId && i.AtType == 4
                                                             &&
                                                  i.EmployeeId == b.EmployeeId
                                                   select new
                                                             {
                                                                 i.AtType
                                                             }).Count()

                          }).ToList();

            
            var v = (from a in datas
                     select new
                     {
                         a.Depart,
                         a.EmployeeId,
                         a.Employee,
                         a.AtType,
                         a.absent,
                         a.overtime,
                         a.present,
                         //a.Unit,
                     }).ToList().
                     GroupBy(x => new { EmployeeId = x.EmployeeId}, (key, group) => new
                    {
                         AttendanceType = group.Select(y => y.AtType).FirstOrDefault(),
                         EmployeeId = key.EmployeeId,
                         Employee = group.Select(y => y.Employee).FirstOrDefault(),
                         Depart = group.Select(y => y.Depart).FirstOrDefault(),
                         Unit ="",
                         absent = group.FirstOrDefault().absent,
                         overtime = group.FirstOrDefault().overtime,
                         present = group.FirstOrDefault().present,
                     }).ToList();


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

        public ActionResult AttendanceRegister() {
            return View();
        }
        [HttpPost]
        public ActionResult GetAttendanceRegister()
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
                         Month = a.Key,
                         MonthVal=a.Value,
                         VCount= (decimal?)(from i in db.Attendances
                                           where i.AtDate.Month == a.Value
                                           && i.AtDate.Year == year
                                            select new
                                            {
                                                i.AttendanceId
                                            }).Count() ?? 0,
                     }).ToList();
            var data = v.ToList();

            return Json(new { data = data });
        }


        #region Attendance Details 
        public ActionResult AttendanceDetails()
        {
            companySet();
            return View();
        }
        public ActionResult getAttendanceDetails(long month)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var v = (from a in db.AttendanceDetails
                     join b in db.Attendances on a.AttendanceId equals b.AttendanceId
                     join c in db.Employees on a.EmployeeId equals c.EmployeeId into emp
                     from c in emp.DefaultIfEmpty()
                     join d in db.AttendanceTypes on a.AttendanceType equals d.Id into attype
                     from d in attype.DefaultIfEmpty()
                     where b.AtDate.Month == month
                     select new
                     {
                         b.VoucherNo,
                         b.AtDate,
                         Employee=c.FirstName +" "+c.LastName,
                         AtType=d.Name,
                         Value=a.Value,
                         Unit=a.Unit,
                     }).AsEnumerable().OrderBy(a => a.AtDate);

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