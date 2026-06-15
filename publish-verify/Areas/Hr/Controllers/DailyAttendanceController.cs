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
using Microsoft.Data.SqlClient;

namespace QuickSoft.Areas.Hr.Controllers
{
    [RedirectingAction]
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class DailyAttendanceController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public DailyAttendanceController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Hr/DailyAttendance
    
        public ActionResult Index()
        {
            return View();
        }
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Create DailyAttendance")]
        [QkAuthorize(Roles = "Dev,DailyAttendance")]
        [HttpGet]
        public ActionResult Create(long? id)
        {
            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            DailyAttendanceViewModel vmodel = new DailyAttendanceViewModel();
            var unit = (from a in db.AttendanceTypes
                        join b in db.payrollunits on a.Unit equals b.Id into pro
                        from b in pro.DefaultIfEmpty()
                        where a.Name == "OVER TIME HOURS"
                        select new
                        {
                            b.UnitName,
                        }).FirstOrDefault();
            vmodel.OTHr = unit.UnitName != null ? unit.UnitName.ToString() : "Hr";

            if (id != null)
            {
                DailyAttendance dattend = db.DailyAttendances.Find(id);
                vmodel.EmployeeId = dattend.EmployeeId;
                vmodel.MonthYear = dattend.MonthYear.ToString("MMMM-yyyy");

            }
            vmodel.AtType = db.AttendanceTypes.Where(a => a.Type == "1" || a.Type == "2").ToList();
           
            var dept = db.Departments
                   .Select(s => new
                   {
                       ID = s.DepartmentID,
                       Name = s.DepartmentName
                   })
                   .ToList();
            ViewBag.Depart = QkSelect.List(dept, "ID", "Name");

            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Monthly Wise", Value="0"},
                new SelectListItem() {Text = "Daily Wise", Value="1"},
            }, "Value", "Text");












  //          string conect =
  //"Data Source=WIN-DSHQGNDJBQ5\\SQL2008;" +
  //"Initial Catalog=HIKVISION;" +
  //"User id=sa;" +
  //"Password=QNT-2020@123;";
  //          var userid = User.Identity.GetUserId();
  //          using (SqlConnection myConnection = new SqlConnection(conect))
  //          {
  //              var empcods = db.Employees.ToList();
  //              DateTime dt = System.DateTime.Now;
  //              string year = System.DateTime.Now.Year.ToString();
  //              string month = System.DateTime.Now.Month.ToString().PadLeft(2,'0');
  //              string fullyear = year + "-" + month + "-01";
  //              //DateTime firstmont = DateTime.ParseExact(year + "-" + month + "-1","yyyy-mm-dd",new CultureInfo("En-Gb"));
  //              string DateString = "01-"+ month +"-"+ year;
  //              IFormatProvider culture = new CultureInfo("en-US", true);
  //              DateTime dateVal = DateTime.Parse(DateString, new CultureInfo("en-GB"));
  //              myConnection.Open();
  //              foreach (var emp in empcods)
  //              {
  //                  //var clr = db.DailyAttendances.Where(u => u.EmployeeId == emp.EmployeeId && u.MonthYear.Month ==dt.Month && u.MonthYear.Year == dt.Year);
  //                  //var attid = clr.FirstOrDefault().DailyAttendanceId;
  //                  //db.DailyAttendances.RemoveRange(clr);
  //                  //db.SaveChanges();
  //                  //var clrdet = db.DailyAttendanceDetails.Where(o => o.DailyAttendanceId == attid);
  //                  //db.DailyAttendanceDetails.RemoveRange(clrdet);
  //                  //db.SaveChanges();
                    
  //                  string oString = "Select employeeID,authdate from attlog where employeeID='"+ emp.EMPCode + "' and authdate>='"+ fullyear +"' group by authdate,employeeID";
  //                  SqlCommand oCmd = new SqlCommand(oString, myConnection);
                    

                   
                    
  //                  using (SqlDataReader oReader = oCmd.ExecuteReader())
  //                  {
  //                      if (oReader.HasRows)
  //                      {
  //                          DateTime dtt = DateTime.Parse(fullyear, new CultureInfo("en-GB"));
  //                          var ext = db.DailyAttendances.Any(o => o.EmployeeId == emp.EmployeeId && o.MonthYear == dtt);
  //                          Int64 DAId = 0;
  //                          if (!ext)
  //                          {
  //                              DailyAttendance dattend = new DailyAttendance
  //                              {
  //                                  EmployeeId = emp.EmployeeId,
  //                                  MonthYear = dtt,
  //                                  Branch = 1,
  //                                  CreatedDate = dt,
  //                                  CreatedBy = userid,
  //                                  Status = Status.active,
  //                              };

  //                              db.DailyAttendances.Add(dattend);
  //                              db.SaveChanges();
  //                               DAId = dattend.DailyAttendanceId;
  //                          }
  //                          else
  //                          {
  //                              var data = db.DailyAttendances.Where(o => o.EmployeeId == emp.EmployeeId && o.MonthYear == dtt).Select(o=>o.DailyAttendanceId).FirstOrDefault();
  //                               DAId = data;

  //                          }
  //                          DailyAttendanceDetail ddetail = new DailyAttendanceDetail();
  //                          while (oReader.Read())
  //                          {
                                
  //                              dt = DateTime.Parse(oReader["authdate"].ToString());
  //                              var data = db.DailyAttendanceDetails.Where(o => o.EmployeeId == emp.EmployeeId && o.DailyAttendanceId == DAId && o.AtDate == dt);
  //                              db.DailyAttendanceDetails.RemoveRange(data);
  //                              db.SaveChanges();
  //                              ddetail.DailyAttendanceId = DAId;
  //                              ddetail.EmployeeId = emp.EmployeeId;
  //                              ddetail.AtDate = dt; //DateTime.Parse(dt, new CultureInfo("en-GB"));
  //                              ddetail.AtType = 4;
  //                              ddetail.Overtime = 0;
  //                              db.DailyAttendanceDetails.Add(ddetail);
  //                              db.SaveChanges();

  //                          }

  //                      }
  //                  }
  //                    }
  //              myConnection.Close();

  //          }















            return View(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create DailyAttendance")]
        [QkAuthorize(Roles = "Dev,DailyAttendance")]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(DailyAttendanceViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var Exists = false;
            if (vmodel.ddlType == "0")
            {
                string MonthYear = "01-" + vmodel.MonthYear;
                DateTime sdate = DateTime.Parse(MonthYear, new CultureInfo("en-GB").DateTimeFormat);
                Exists = db.DailyAttendances.Any(u => u.EmployeeId == vmodel.EmployeeId && u.MonthYear.Month == sdate.Month && u.MonthYear.Year == sdate.Year && (u.DailyAttendanceId != vmodel.DailyAttendanceId));


                if (Exists)
                {
                    var clr = db.DailyAttendances.Where(u => u.EmployeeId == vmodel.EmployeeId && u.MonthYear.Month == sdate.Month && u.MonthYear.Year == sdate.Year && (u.DailyAttendanceId != vmodel.DailyAttendanceId));
                    var attid = clr.FirstOrDefault().DailyAttendanceId;
                    db.DailyAttendances.RemoveRange(clr);
                    db.SaveChanges();
                    var clrdet = db.DailyAttendanceDetails.Where(o => o.DailyAttendanceId == attid);
                    db.DailyAttendanceDetails.RemoveRange(clrdet);
                    db.SaveChanges();
                    Exists = false;

                }

                if (Exists)
                {
                   var clr= db.DailyAttendances.Where(u => u.EmployeeId == vmodel.EmployeeId && u.MonthYear.Month == sdate.Month && u.MonthYear.Year == sdate.Year && (u.DailyAttendanceId != vmodel.DailyAttendanceId));
                    var attid = clr.FirstOrDefault().DailyAttendanceId;
                    db.DailyAttendances.RemoveRange(clr);
                    db.SaveChanges();
                    var clrdet = db.DailyAttendanceDetails.Where(o => o.DailyAttendanceId == attid);
                    db.DailyAttendanceDetails.RemoveRange(clrdet);
                    db.SaveChanges();
                    Exists = false;

                }
            }
            if (vmodel.ddlType == "0" && Exists)
            {
                msg = "Daily Attendance Employee with Same Month Already exists.";
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

                Int64 DAId = 0;
                if (vmodel.ddlType == "0")
                {
                    var dailyatt = db.DailyAttendances.Where(a => a.DailyAttendanceId == vmodel.DailyAttendanceId).FirstOrDefault();
                    if (dailyatt != null)
                    {
                        db.DailyAttendances.RemoveRange(db.DailyAttendances.Where(a => a.DailyAttendanceId == vmodel.DailyAttendanceId));
                        db.SaveChanges();
                    }
                    DailyAttendance dattend = new DailyAttendance
                    {
                        EmployeeId = vmodel.EmployeeId,
                        MonthYear = DateTime.Parse(vmodel.MonthYear.ToString(), new CultureInfo("en-GB")),
                        Branch = Branch,
                        CreatedDate = today,
                        CreatedBy = UserId,
                        Status = Status.active,
                    };
                    db.DailyAttendances.Add(dattend);
                    db.SaveChanges();
                    DAId = dattend.DailyAttendanceId;

                    var attdet = db.DailyAttendanceDetails.Where(a => a.DailyAttendanceId == vmodel.DailyAttendanceId).FirstOrDefault();
                    if (attdet != null)
                    {
                        db.DailyAttendanceDetails.RemoveRange(db.DailyAttendanceDetails.Where(a => a.DailyAttendanceId == vmodel.DailyAttendanceId));
                        db.SaveChanges();
                    }
                    DailyAttendanceDetail ddetail = new DailyAttendanceDetail();
                    foreach (var arr in vmodel.addattendance)
                    {
                        if (arr.EmployeeId > 0 && arr.AtDate.ToString() != "")
                        {
                            ddetail.DailyAttendanceId = DAId;
                            ddetail.EmployeeId = arr.EmployeeId;
                            ddetail.AtDate = DateTime.Parse(arr.AtDate, new CultureInfo("en-GB"));
                            ddetail.AtType = arr.AtType == 0 ? 0 : arr.AtType;
                            ddetail.Overtime = arr.Overtime;
                            db.DailyAttendanceDetails.Add(ddetail);
                            db.SaveChanges();
                        }
                    }

                }
                else
                {
                    DailyAttendanceDetail ddetail = new DailyAttendanceDetail();
                    foreach (var arr in vmodel.addattendanced)
                    {
                        DateTime ADate = DateTime.Parse(vmodel.AtDate, new CultureInfo("en-GB"));
                        var dailyatt = db.DailyAttendances.Where(a => a.MonthYear.Month == ADate.Month && a.MonthYear.Year == ADate.Year && a.EmployeeId == arr.EmployeeId).FirstOrDefault();
                        if (dailyatt == null)
                        {
                            DailyAttendance dattend = new DailyAttendance
                            {
                                EmployeeId = arr.EmployeeId,
                                MonthYear = DateTime.Parse(vmodel.AtDate, new CultureInfo("en-GB")),
                                Branch = Branch,
                                CreatedDate = today,
                                CreatedBy = UserId,
                                Status = Status.active,
                            };
                            db.DailyAttendances.Add(dattend);
                            db.SaveChanges();
                            DAId = dattend.DailyAttendanceId;
                        }
                        else
                        {
                            DAId = dailyatt.DailyAttendanceId;
                        }

                        var attdet = db.DailyAttendanceDetails.Where(a => a.AtDate == ADate && a.DailyAttendanceId == vmodel.DailyAttendanceId && a.EmployeeId == arr.EmployeeId).FirstOrDefault();
                        if (attdet == null)
                        {
                            if (arr.EmployeeId > 0 && vmodel.AtDate.ToString() != "")
                            {
                                ddetail.DailyAttendanceId = DAId;
                                ddetail.EmployeeId = arr.EmployeeId;
                                ddetail.AtDate = DateTime.Parse(vmodel.AtDate, new CultureInfo("en-GB"));
                                ddetail.AtType = arr.AtType; //== 0 ? 4 : arr.AtType;
                                ddetail.Overtime = arr.Overtime;
                                db.DailyAttendanceDetails.Add(ddetail);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            DailyAttendanceDetail datt = db.DailyAttendanceDetails.Find(attdet.DailyAttendanceDetailId);
                            datt.AtType = arr.AtType;
                            datt.Overtime = arr.Overtime;
                            db.Entry(datt).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }


                com.addlog(LogTypes.Created, UserId, "DailyAttendance", "DailyAttendances", findip(), DAId, "Successfully Submitted Daily Attendance");

                msg = "Successfully submitted Daily Attendance.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        [HttpGet]
        public JsonResult GetDailyAttendance(long Emp, string MonthYear)
        {

            MonthYear = "01-" + MonthYear;
            DateTime sdate = DateTime.Parse(MonthYear, new CultureInfo("en-GB").DateTimeFormat);

            var empJoin = db.Employees.Where(a => a.EmployeeId == Emp).FirstOrDefault();
            if (empJoin == null) { return Json(0); }

            DateTime joindate = Convert.ToDateTime(empJoin.JoinDate); //DateTime.Parse(empJoin.JoinDate.ToString(), new CultureInfo("en-GB").DateTimeFormat);

            DateTime? leavedate = null;
            if (empJoin.LeavingDate != null)
            {
                leavedate = DateTime.Parse(empJoin.LeavingDate.ToString(), new CultureInfo("en-GB").DateTimeFormat);
            }


            if (sdate.Date >= joindate.Date)
            {

                //department
                var de = db.Employees.Where(a => a.EmployeeId == Emp).Select(a => a.DepartmentID).FirstOrDefault();
                //calender template
                var calendertemplate = db.Departments.Where(a => a.DepartmentID == de).Select(a => a.CalendarTemplateId).FirstOrDefault();
                if (calendertemplate == null)
                {
                    calendertemplate = db.CalendarTemplates.Where(a => a.DefaultValue == true).Select(a => a.CalendarTemplateID).FirstOrDefault();
                }
                //template holidays
                var tempdetail = db.WeeklyHolidays.Where(a => a.TemplateID == calendertemplate).Select(a => a.SelDay).ToList();

                var holiday = db.Holidays.Where(a => (a.FromDate.Month == sdate.Month || a.ToDate.Month == sdate.Month) && a.CalendarTemplateID == calendertemplate).ToList();

                var holidays = com.GetDates(sdate, tempdetail, holiday).Select(a => a.Date).ToList();

                var v = (from a in db.DailyAttendanceDetails
                         join b in db.AttendanceTypes on a.AtType equals b.Id into attype
                         from b in attype.DefaultIfEmpty()
                         where a.EmployeeId == Emp && a.AtDate.Month == sdate.Month && a.AtDate.Year == sdate.Year
                         select new
                         {
                             a.DailyAttendanceId,
                             Date = a.AtDate,
                             AtTypeId = a.AtType,
                             AtType = b.Name,
                             OverTime = a.Overtime,
                             EmpId = a.EmployeeId,
                         }).ToList().Select(o => new
                         {
                             o.DailyAttendanceId,
                             o.Date,
                             o.AtTypeId,
                             o.AtType,
                             o.OverTime,
                             o.EmpId,
                             Holiday = holidays.Contains(o.Date) ? true : false
                         }).ToList();

                DateTime last = sdate.AddMonths(1).AddSeconds(-1);
                DateTime fdate = sdate;
                DateTime tdate = last;

                List<HolidayListViewModel> hlist = new List<HolidayListViewModel>();

                for (DateTime i = fdate; i <= tdate; i = i.AddDays(1))
                {
                    HolidayListViewModel HModel = new HolidayListViewModel();
                    HModel.Date = i;
                    hlist.Add(HModel);
                }
                var vx = (from a in hlist
                          select new
                          {
                              DailyAttendanceId = 0,
                              Date = a.Date,
                              AtTypeId = "",
                              AtType = "",
                              OverTime = 0,
                              EmpId = Emp,
                          }).ToList().Select(o => new
                          {
                              o.DailyAttendanceId,
                              o.Date,
                              o.AtTypeId,
                              o.AtType,
                              o.OverTime,
                              o.EmpId,
                              Holiday = holidays.Contains(o.Date) ? true : false
                          }).ToList();
                if (v.Count() == 0)
                {
                    return Json(vx);
                }
                else
                {
                    var vy = (from a in vx
                              join b in v on a.Date equals b.Date into attype
                              from b in attype.DefaultIfEmpty()
                              select new
                              {
                                  DailyAttendanceId= b != null ? b.DailyAttendanceId : a.DailyAttendanceId,
                                  a.Date,
                                  AtTypeId = b !=null? b.AtTypeId :0,
                                  AtType= b != null ? b.AtType :"",
                                  OverTime = b != null ? b.OverTime : 0,
                                  a.EmpId,
                                  a.Holiday
                              }).ToList();

                    return Json(vy);
                }
            }
            else if (leavedate != null && sdate.Date < leavedate.Value.Date)
            {
                return Json(1);
            }
            else
            {
                return Json(0);
            }

        }



        [HttpGet]
        public JsonResult GetDeptDailyAttendance(long Dept, string AtDate)
        {
            if (string.IsNullOrWhiteSpace(AtDate)) { return Json(0); }
            DateTime sdate = DateTime.Parse(AtDate, new CultureInfo("en-GB").DateTimeFormat);
            //department
            //calender template
            var calendertemplate = db.Departments.Where(a => a.DepartmentID == Dept).Select(a => a.CalendarTemplateId).FirstOrDefault();
            if (calendertemplate == null)
            {
                calendertemplate = db.CalendarTemplates.Where(a => a.DefaultValue == true).Select(a => a.CalendarTemplateID).FirstOrDefault();
            }
            //template holidays
            var tempdetail = db.WeeklyHolidays.Where(a => a.TemplateID == calendertemplate).Select(a => a.SelDay).ToList();

            var holiday = db.Holidays.Where(a => (a.FromDate.Month == sdate.Month || a.ToDate.Month == sdate.Month) && a.CalendarTemplateID == calendertemplate).ToList();

            var holidays = com.GetDates(sdate, tempdetail, holiday).Select(a => a.Date).ToList();

            var v = (from a in db.DailyAttendanceDetails
                     join b in db.AttendanceTypes on a.AtType equals b.Id into attype
                     from b in attype.DefaultIfEmpty()
                     join d in db.Employees on a.EmployeeId equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.EmployeeWorkDetails on a.EmployeeId equals e.EmployeeId into wds
                     from e in wds.DefaultIfEmpty()
                     where EF.Functions.DateDiffDay(a.AtDate, sdate) == 0 && d.DepartmentID == Dept && d.LeavingDate == null && e.EmployeeAccount != null
                     select new
                     {
                         a.DailyAttendanceId,
                         Date = sdate,
                         AtTypeId = a.AtType,
                         AtType = b.Name,
                         OverTime = a.Overtime,
                         EmpId = a.EmployeeId,
                         EmpName = d.FirstName + " " + d.LastName
                     }).ToList().Select(o => new
                     {
                         o.DailyAttendanceId,
                         o.Date,
                         o.AtTypeId,
                         o.AtType,
                         o.OverTime,
                         o.EmpId,
                         o.EmpName,
                         Holiday = holidays.Contains(o.Date) ? true : false
                     }).ToList();

            var vx = (from a in db.Employees
                      join b in db.EmployeeWorkDetails on a.EmployeeId equals b.EmployeeId into wds
                      join c in db.Users on a.UserId equals c.Id
                      from b in wds.DefaultIfEmpty()
                      where (Dept==0||a.DepartmentID == Dept) 
                      && c.Status==1
                      && b.EmployeeAccount != null && a.LeavingDate == null
                      && EF.Functions.DateDiffDay(sdate, a.JoinDate) <= 0
                      // EF.Functions.DateDiffDay(sdate, a.LeavingDate) >= 0
                      select new
                      {
                          DailyAttendanceId = 0,
                          Date = sdate,
                          AtTypeId = "",
                          AtType = "",
                          OverTime = 0,
                          EmpId = a.EmployeeId,
                          EmpName = a.FirstName + " " + a.LastName
                      }).ToList().Select(o => new
                      {
                          o.DailyAttendanceId,
                          o.Date,
                          o.AtTypeId,
                          o.AtType,
                          o.OverTime,
                          o.EmpId,
                          o.EmpName,
                          Holiday = holidays.Contains(o.Date) ? true : false
                      }).ToList();

            if (v.Count() == 0)
            {
                return Json(vx);
            }
            else
            {
                var vy = (from a in vx
                          join b in v on a.EmpId equals b.EmpId into attype
                          from b in attype.DefaultIfEmpty()
                          select new
                          {
                              DailyAttendanceId = b != null ? b.DailyAttendanceId : a.DailyAttendanceId,
                              a.Date,
                              AtTypeId = b != null ? b.AtTypeId : 0,
                              AtType = b != null ? b.AtType : "",
                              OverTime = b != null ? b.OverTime : 0,
                              a.EmpId,
                              a.EmpName,
                              a.Holiday
                          }).ToList();

                return Json(vy);
            }

        }

        [HttpPost]
       
        public JsonResult GetDailyAttendance()
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


            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from b in db.DailyAttendances
                     join c in db.Employees on b.EmployeeId equals c.EmployeeId into ctemp
                     from c in ctemp.DefaultIfEmpty()
                     select new
                     {
                         b.DailyAttendanceId,
                         EmployeeName = c.FirstName + " " + c.LastName,
                         MonthAndYear = b.MonthYear,
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.EmployeeName.ToString().ToLower().Contains(search.ToLower()) || p.MonthAndYear.ToString().ToLower().Contains(search.ToLower())
                );
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

        public static List<DateTime> GetDates(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(year, month, day)) // Map each day to a date
                             .ToList(); // Load dates into a list
        }


        // GET: Desg/Delete/5
        [QkAuthorize(Roles = "Dev,Delete DailyAttendance")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DailyAttendance Datt = db.DailyAttendances.Find(id);
            if (Datt == null)
            {
                return NotFound();
            }

            return PartialView(Datt);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete DailyAttendance")]
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
                DailyAttendance daatt = db.DailyAttendances.Find(id);

                var daitem = db.DailyAttendanceDetails.Where(a => a.DailyAttendanceId == id).FirstOrDefault();
                if (daitem != null)
                {
                    db.DailyAttendanceDetails.RemoveRange(db.DailyAttendanceDetails.Where(a => a.DailyAttendanceId == id));
                    db.SaveChanges();
                }
                db.DailyAttendances.Remove(daatt);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "DailyAttendance", "DailyAttendances", findip(), daatt.DailyAttendanceId, "Daily Attendance Deleted Successfully");

                stat = true;
                msg = "Successfully Deleted Daily Attendance  details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;

            return msg;
        }


    }
}