using QuickSoft.Web;
using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;

namespace QuickSoft.Controllers
{
    public class taskcalendarController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult monthlyall()
        {


            var use = (from a in db.Employees

                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName + " " + a.LastName
                       })
                           .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            ViewBag.driver = QkSelect.List(use, "id", "text");

            return View();
        }
        // GET: calendar/taskcalendar
        public ActionResult Index()
        {


            var use = (from a in db.Employees
                       where a.Status ==1
                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName + " " + a.LastName
                       })
                           .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            ViewBag.driver = QkSelect.List(use, "id", "text");

            return View();
        }
        public ActionResult monthly()
        {


            var use = (from a in db.Employees
                       join b in db.TeamMembers on a.EmployeeId equals b.EmployeeId
                       where b.TeamId == 4
                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName + " " + a.LastName
                       })
                           .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            ViewBag.driver = QkSelect.List(use, "id", "text");

            return View();
        }
        public ActionResult manual()
        {
            return View();
        }
        [HttpPost]
        public ActionResult index(long? Employee, string From, string To)
        {
            string fromdate = From;
            string todate = From;
            employeetimesheetlist vv = new employeetimesheetlist();
            //Find Order Column
            var use = (from a in db.Employees
                       join b in db.TeamMembers on a.EmployeeId equals b.EmployeeId
                       where b.TeamId == 4
                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName + " " + a.LastName
                       })
                           .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            ViewBag.driver = QkSelect.List(use, "id", "text");
            ViewBag.fromdate = From;

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            DateTime? fdate = null;
            DateTime? tdate = null;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB").DateTimeFormat);
                tdate = tdate.Value.AddDays(2);
            }

            vv.et = (from a in db.servicereports
                     join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                     join c in db.Employees on b.employeeid equals c.EmployeeId
                     where (Employee == 0 || Employee == null || b.employeeid == Employee) &&
                      (fromdate == "" || fromdate == null || EF.Functions.DateDiffDay(a.starttime, fdate) <= 0) &&

                      (todate == "" || fromdate == null || (EF.Functions.DateDiffDay(a.endtime, tdate) > 0 && EF.Functions.DateDiffDay(a.starttime, fdate) >= 0))


                     select new employeetimesheet
                     {
                         entryid = a.servicereportid,
                         EmployeeName = c.FirstName + " " + c.LastName,
                         servocedatefrom = a.starttime,
                         servocedateto = a.endtime,
                         endtime = (a.endtime < a.starttime) ? DbFunctionsCompat.AddDays(a.endtime, 1) : a.endtime,
                         protaskid = a.protaskid,

                         starttime = a.starttime

                     }).OrderBy(o => o.EmployeeName).ToList().Select(o => new employeetimesheet
                     {
                         entryid = o.entryid,
                         EmployeeName = o.EmployeeName,
                         servocedatefrom = o.servocedatefrom,
                         servocedateto = o.servocedateto,
                         protaskid = o.protaskid,
                         hours = (o.endtime - o.starttime).Value.Hours,
                         minute = (o.endtime - o.starttime).Value.Minutes,
                     }).OrderBy(o => o.EmployeeName).ThenBy(o => o.servocedatefrom).ToList();

            return View(vv);

        }
        [HttpPost]
        public ActionResult monthly(long? Employee, string From, string To, bool chktotal)
        {
            string fromdate = From;
            string todate = To;
            employeetimesheetlist vv = new employeetimesheetlist();
            //Find Order Column
            var use = (from a in db.Employees
                       join b in db.TeamMembers on a.EmployeeId equals b.EmployeeId
                       where b.TeamId == 4
                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName + " " + a.LastName
                       }).OrderBy(o => o.text)
                           .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            ViewBag.driver = QkSelect.List(use, "id", "text");
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            DateTime? fdate = null;
            DateTime? tdate = null;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB").DateTimeFormat);
                tdate = tdate.Value.AddDays(1);
            }
            List<HolidayListViewModel> hlist = new List<HolidayListViewModel>();
            for (DateTime i = (DateTime)fdate; i < tdate; i = i.AddDays(1))
            {
                HolidayListViewModel HModel = new HolidayListViewModel();
                HModel.Date = i;
                hlist.Add(HModel);
            }

            var emparray = (from a in db.Employees
                            join b in db.TeamMembers on a.EmployeeId equals b.EmployeeId
                            where b.TeamId == 4
                            && (Employee == null || Employee == 0 || a.EmployeeId == Employee)
                            select new SelectFormat
                            {
                                id = a.EmployeeId,

                                text = a.FirstName + " " + a.MiddleName + " " + a.LastName
                            }).OrderBy(o => o.text)
                         .Select(o => o.id).ToArray();
            List<employeetimesheet> ls = new List<employeetimesheet>();
            foreach (var emp in emparray)
            {
                foreach (var d in hlist)
                {
                    employeetimesheet a = new employeetimesheet();
                    a.entryid = 1;
                    a.entrydate = d.Date;
                    a.EmployeeName = db.Employees.Where(o => o.EmployeeId == emp).Select(o => new
                    {
                        eployeename = o.FirstName + " " + o.MiddleName + " " + o.LastName
                    }).Select(o => o.eployeename).FirstOrDefault();
                    a.totlhour = (decimal?)getemployeeworkinghour(emp, d.Date)["Totalhour"];
                    a.totalminute = (decimal?)getemployeeworkinghour(emp, d.Date)["overtime"];
                    ls.Add(a);
                }
            }

            ViewBag.withtotal = 0;
            if (chktotal == true)
                ViewBag.withtotal = 1;
            vv.et = ls;
            return View(vv);

        }
        [HttpPost]
        public ActionResult updateendtimes(ServiceReportViewModel vm)
        {
            if (vm.servicereportid != 0)
            {
                var att = db.EmpAttendances.Where(o => o.Id == vm.servicereportid).FirstOrDefault();

                DateTime sDate = DateTime.Parse(vm.StartDate.ToString(), new CultureInfo("en-GB"));
                DateTime eDate = DateTime.Parse(vm.EndDate.ToString(), new CultureInfo("en-GB"));
                TimeSpan? stime = null;
                if (vm.StartTime != null)
                {
                    stime = ((DateTime)vm.StartTime).TimeOfDay;
                }
                DateTime? stimes = sDate + stime;
                att.login = (DateTime)stimes;
                if (vm.EndTime != null)
                {
                    stime = ((DateTime)vm.EndTime).TimeOfDay;
                }
                stimes = eDate + stime;
                att.logout = (DateTime)stimes;
                if (vm.leavestatus != null)
                {
                    att.leavestatus = vm.leavestatus;
                }
                db.Entry(att).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                EmpAttendance att = new EmpAttendance();

                DateTime sDate = DateTime.Parse(vm.StartDate.ToString(), new CultureInfo("en-GB"));
                DateTime eDate = DateTime.Parse(vm.EndDate.ToString(), new CultureInfo("en-GB"));
                TimeSpan? stime = null;
                if (vm.StartTime != null)
                {
                    stime = ((DateTime)vm.StartTime).TimeOfDay;
                }
                DateTime? stimes = sDate + stime;
                att.login = (DateTime)stimes;
                if (vm.EndTime != null)
                {
                    stime = ((DateTime)vm.EndTime).TimeOfDay;
                }
                stimes = eDate + stime;
                att.EmployeeName = vm.Note;
                att.logout = (DateTime)stimes;
                if(vm.leavestatus!=null)
                {
                    att.leavestatus = vm.leavestatus;
                }
                db.EmpAttendances.Add(att);
                db.SaveChanges();
            }
            Success("Success", true);

            string msg = " success!";
            bool stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult updateendtime(string userid, long entryid, DateTime? dateatt)
        {
            if (entryid != 0)
            {
                var att = db.EmpAttendances.Where(o => o.Id == entryid).FirstOrDefault();
                ServiceReportViewModel a = new ServiceReportViewModel();
                a.StartDate = ((DateTime)att.login).ToString("dd-MM-yyyy");
                a.StartTime = att.login;
                a.EndDate = ((DateTime)att.login).ToString("dd-MM-yyyy");
                a.EndTime = att.logout;
                a.servicereportid = entryid;
                return View(a);
            }
            else
            {
                ServiceReportViewModel a = new ServiceReportViewModel();
                a.servicereportid = entryid;
                a.StartDate = ((DateTime)dateatt).ToString("dd-MM-yyyy");
                a.EndDate = ((DateTime)dateatt).ToString("dd-MM-yyyy"); ;
                a.Note = userid;
                return View(a);
            }

        }
        public ActionResult monthlyallcon()
        {


            var use = (from a in db.Employees

                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName + " " + a.LastName
                       })
                           .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            ViewBag.driver = QkSelect.List(use, "id", "text");

            return View();
        }
        [HttpPost]
        public ActionResult monthlyall(long? Employee, string From, string To, bool chktotal)
        {
            string fromdate = From;
            string todate = To;
            employeetimesheetlist vv = new employeetimesheetlist();
            //Find Order Column
            var use = (from a in db.Employees

                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName + " " + a.LastName
                       }).OrderBy(o => o.text)
                           .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            ViewBag.driver = QkSelect.List(use, "id", "text");
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            DateTime? fdate = null;
            DateTime? tdate = null;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB").DateTimeFormat);
                tdate = tdate.Value.AddDays(1);
            }
            List<HolidayListViewModel> hlist = new List<HolidayListViewModel>();
            for (DateTime i = (DateTime)fdate; i < tdate; i = i.AddDays(1))
            {
                HolidayListViewModel HModel = new HolidayListViewModel();
                HModel.Date = i;
                hlist.Add(HModel);
            }

            var emparray = (from a in db.Employees
                            join c in db.Users on a.UserId equals c.Id
                            where c.Status == 1
                                && a.Status == 1
                            where
                             (Employee == null || Employee == 0 || a.EmployeeId == Employee)
                            select new SelectFormat
                            {
                                id = a.EmployeeId,

                                text = a.FirstName + " " + a.MiddleName + " " + a.LastName
                            }).OrderBy(o => o.text)
                         .Select(o => o.id).ToArray();
            List<employeetimesheet> ls = new List<employeetimesheet>();
            foreach (var emp in emparray)
            {
                foreach (var d in hlist)
                {
                    employeetimesheet a = new employeetimesheet();
                    var duty = getemployeeworkinghourall(emp, d.Date);
                    a.entryid = (duty["entryid"] == null) ? 0 : (long)duty["entryid"];
                    a.entrydate = (DateTime?)duty["dutylaststartime"];
                    a.servocedatefrom = (DateTime?)duty["dutystart"];
                    a.attdate = d.Date;
                    a.leavestatus= (int?)duty["leavestatus"];
                    a.servocedateto = ((DateTime?)duty["dutyend"] == null || (DateTime?)duty["dutystart"] == null) ? (DateTime?)duty["dutyend"] : ((((DateTime?)duty["dutyend"] - (DateTime?)duty["dutystart"]).Value.TotalHours > 22) ? (DateTime?)duty["dutystart"] : (DateTime?)duty["dutyend"]);
                    a.EmployeeName = db.Employees.Where(o => o.EmployeeId == emp).Select(o => new
                    {
                        eployeename = o.FirstName + " " + o.LastName
                    }).Select(o => o.eployeename).FirstOrDefault();
                    a.userid = db.Employees.Where(o => o.EmployeeId == emp).Select(o => new
                    {
                        eployeename = o.UserId
                    }).Select(o => o.eployeename).FirstOrDefault();
                    a.totlhour = (decimal?)duty["Totalhour"];
                    a.totalminute = (decimal?)duty["overtime"];
                    ls.Add(a);
                }
            }

            var lss = ls.OrderBy(o => o.EmployeeName);
            ViewBag.withtotal = 0;
            if (chktotal == true)
                ViewBag.withtotal = 1;
            vv.et = lss.ToList();

            return View(vv);


        }
        [HttpPost]
        public ActionResult monthlyallcon(long? Employee, string From, string To, bool chktotal)
        {
            string fromdate = From;
            string todate = To;
            employeetimesheetlist vv = new employeetimesheetlist();
            //Find Order Column
            var use = (from a in db.Employees

                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName + " " + a.LastName
                       }).OrderBy(o => o.text)
                           .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            ViewBag.driver = QkSelect.List(use, "id", "text");
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            DateTime? fdate = null;
            DateTime? tdate = null;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB").DateTimeFormat);
                tdate = tdate.Value.AddDays(1);
            }
            List<HolidayListViewModel> hlist = new List<HolidayListViewModel>();
            for (DateTime i = (DateTime)fdate; i < tdate; i = i.AddDays(1))
            {
                HolidayListViewModel HModel = new HolidayListViewModel();
                HModel.Date = i;
                hlist.Add(HModel);
            }

            var emparray = (from a in db.Employees
                            join c in db.Users on a.UserId equals c.Id
                            where c.Status == 1
                                && a.Status == 1
                            where
                             (Employee == null || Employee == 0 || a.EmployeeId == Employee)
                            select new SelectFormat
                            {
                                id = a.EmployeeId,

                                text = a.FirstName + " " + a.MiddleName + " " + a.LastName
                            }).OrderBy(o => o.text)
                         .Select(o => o.id).ToArray();
            List<employeetimesheet> ls = new List<employeetimesheet>();
            foreach (var emp in emparray)
            {
                foreach (var d in hlist)
                {
                    employeetimesheet a = new employeetimesheet();
                    var duty = getemployeeworkinghourall(emp, d.Date);
                   
              
                    a.entryid = (duty["entryid"] == null) ? 0 : (long)duty["entryid"];
                    a.entrydate = (DateTime?)duty["dutylaststartime"];
                    a.servocedatefrom = (DateTime?)duty["dutystart"];
                    a.attdate = d.Date;
                    a.leavestatus = (int?)duty["leavestatus"];
                    a.servocedateto = ((DateTime?)duty["dutyend"] == null || (DateTime?)duty["dutystart"] == null) ? (DateTime?)duty["dutyend"] : ((((DateTime?)duty["dutyend"] - (DateTime?)duty["dutystart"]).Value.TotalHours > 22) ? (DateTime?)duty["dutystart"] : (DateTime?)duty["dutyend"]);
                    a.EmployeeName = db.Employees.Where(o => o.EmployeeId == emp).Select(o => new
                    {
                        eployeename = o.FirstName + " " + o.LastName
                    }).Select(o => o.eployeename).FirstOrDefault();
                    a.userid = db.Employees.Where(o => o.EmployeeId == emp).Select(o => new
                    {
                        eployeename = o.UserId
                    }).Select(o => o.eployeename).FirstOrDefault();
                    a.totlhour = (decimal?)duty["Totalhour"];
                    a.totalminute = (decimal?)duty["overtime"];
                    ls.Add(a);
                }
            }

            var lss = ls.OrderBy(o => o.EmployeeName);
            ViewBag.withtotal = 0;
            if (chktotal == true)
                ViewBag.withtotal = 1;
            vv.et = lss.ToList();
            return View(vv);

        }


        public Dictionary<string, object> getemployeeworkinghour(long empid, DateTime date)
        {
            var nextdaay = date.AddDays(1);
            var sr = (from a in db.servicereports
                      join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                      join c in db.Employees on b.employeeid equals c.EmployeeId
                      where c.EmployeeId == empid && a.starttime >= date && a.starttime < nextdaay
                      select new employeetimesheet
                      {
                          entryid = a.servicereportid,
                          EmployeeName = c.FirstName + " " + c.LastName,
                          servocedatefrom = a.starttime,
                          servocedateto = a.endtime,
                          endtime = (a.endtime < a.starttime) ? DbFunctionsCompat.AddDays(a.endtime, 1) : a.endtime,
                          protaskid = a.protaskid,

                          starttime = a.starttime

                      }).OrderBy(o => o.EmployeeName).ToList().Select(o => new employeetimesheet
                      {
                          entryid = o.entryid,
                          EmployeeName = o.EmployeeName,
                          servocedatefrom = o.servocedatefrom,
                          servocedateto = o.servocedateto,
                          protaskid = o.protaskid,
                          entrydate = o.endtime,
                          hours = (o.endtime - o.starttime).Value.Hours,
                          minute = (o.endtime - o.starttime).Value.Minutes,
                      }).OrderBy(o => o.EmployeeName).ThenBy(o => o.entrydate).ToList();
            var gp = (from v in sr
                      group new { v.EmployeeName, v.hours, v.minute } by new { v.entrydate, v.EmployeeName } into gt
                      select new employeetimesheet
                      {
                          entryid = 1,
                          EmployeeName = gt.Key.EmployeeName,
                          entrydate = gt.Key.entrydate,
                          totlhour = gt.Sum(o => o.hours),
                          totalminute = gt.Sum(o => o.minute)

                      }).ToList();
            Dictionary<string, object> Balance = null;
            var thour = gp.Sum(o => o.totlhour);
            var tminute = gp.Sum(o => o.totalminute);
            var tt = thour + tminute / 60;
            decimal? overtime = null;
            if (tt == null)
                overtime = -8;
            else if (tt > 7)
                overtime = tt - 10;
            else if (tt >= 4 && tt <= 7)
                overtime = tt - 9;
            else if (tt < 4)
                overtime = tt - 8;
            else
                overtime = -8;

            decimal? te = 0;
            var Data = new Dictionary<string, object>();

            var ttinhour = thour + (int)tminute / 60 + (tminute % 60) / 100;

            Data.Add("Totalhour", tt);
            Data.Add("overtime", overtime);

            return Data;

        }
        public Dictionary<string, object> getemployeeabsent(long empid, DateTime date)
        {
            DateTime? dutystartime = null;
            DateTime? dutyendtime = null;
            DateTime? dutylaststarttime = null;
            long? entryid = null;
            var nextdaay = date.AddDays(1);
            DateTime currdate = System.DateTime.Now;
            var firstentrytask = (from a in db.servicereports
                                  join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                                  join c in db.Employees on b.employeeid equals c.EmployeeId

                                  where c.EmployeeId == empid && a.starttime >= date && a.starttime < nextdaay
                                  select new
                                  {
                                      a.starttime
                                  }).Select(o => o.starttime).AsEnumerable().DefaultIfEmpty(currdate).FirstOrDefault();
            var firstentryapp = (from a in db.EmpAttendances
                                 join b in db.Employees on a.EmployeeName equals b.UserId
                                 where a.login >= date && a.login < nextdaay
                                 && b.EmployeeId == empid
                                 select new
                                 {
                                     a.login
                                 }).Select(o => o.login).AsEnumerable().DefaultIfEmpty(currdate).FirstOrDefault();
            if (firstentrytask < firstentryapp)
            {
                var sr = (from a in db.servicereports
                          join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                          join c in db.Employees on b.employeeid equals c.EmployeeId

                          where c.EmployeeId == empid && a.starttime >= date && a.starttime < nextdaay
                          select new employeetimesheet
                          {
                              entryid = a.servicereportid,
                              EmployeeName = c.FirstName + " " + c.LastName,
                              servocedatefrom = a.starttime,
                              servocedateto = a.endtime,
                              endtime = (a.endtime < a.starttime) ? DbFunctionsCompat.AddDays(a.endtime, 1) : a.endtime,
                              protaskid = a.protaskid,

                              starttime = a.starttime

                          }).OrderBy(o => o.EmployeeName).ToList().Select(o => new employeetimesheet
                          {
                              entryid = o.entryid,
                              EmployeeName = o.EmployeeName,
                              servocedatefrom = o.servocedatefrom,
                              servocedateto = o.servocedateto,
                              protaskid = o.protaskid,
                              entrydate = o.endtime,
                              hours = (o.endtime - o.starttime).Value.Hours,
                              minute = (o.endtime - o.starttime).Value.Minutes,
                          }).OrderBy(o => o.EmployeeName).ThenBy(o => o.entrydate).ToList();
                var gp = (from v in sr
                          group new { v.EmployeeName, v.entryid, v.hours, v.minute, v.servocedatefrom, v.servocedateto } by new { v.entrydate, v.EmployeeName } into gt
                          select new employeetimesheet
                          {
                              entryid = gt.FirstOrDefault().entryid,
                              EmployeeName = gt.Key.EmployeeName,
                              entrydate = gt.Key.entrydate,
                              totlhour = gt.Sum(o => o.hours),
                              totalminute = gt.Sum(o => o.minute),
                              servocedatefrom = gt.FirstOrDefault().servocedatefrom,
                              servocedateto = gt.FirstOrDefault().servocedateto

                          }).ToList();
                Dictionary<string, object> Balance = null;
                var thour = gp.Sum(o => o.totlhour);
                var tminute = gp.Sum(o => o.totalminute);
                var tt = thour + tminute / 60;
                if (gp.Count() > 0)
                    entryid = gp.Max(o => o.entryid);
                decimal? overtime = null;
                if (tt == null)
                    overtime = -8;
                else if (tt > 7)
                    overtime = tt - 10;
                else if (tt >= 4 && tt <= 7)
                    overtime = tt - 9;
                else if (tt < 4)
                    overtime = tt - 8;
                else
                    overtime = -8;
                dutystartime = gp.Min(o => o.servocedatefrom);
                dutylaststarttime = System.DateTime.Now;// gp.Max(o => o.servocedatefrom);
                dutyendtime = gp.Max(o => o.servocedateto);
                decimal? te = 0;
                var Data = new Dictionary<string, object>();

                var ttinhour = thour + (int)tminute / 60 + (tminute % 60) / 100;
                Data.Add("Totalhour", tt);
                Data.Add("overtime", overtime);
                Data.Add("dutystart", dutystartime);
                Data.Add("dutyend", dutyendtime);
                Data.Add("entryid", entryid);
                Data.Add("dutylaststartime", dutylaststarttime);
                return Data;

            }
            else
            {
                var nday = date.AddHours(22);
                var sr = (from a in db.EmpAttendances
                          join b in db.Employees on a.EmployeeName equals b.UserId
                          where a.login >= date && a.login < nday
                          && b.EmployeeId == empid
                          select new employeetimesheet
                          {
                              entryid = a.Id,
                              EmployeeName = b.FirstName + " " + b.LastName,
                              servocedatefrom = a.login,
                              servocedateto = (a.logout == null) ? a.login : a.logout,
                              endtime = (a.logout == null) ? a.login : a.logout,
                              protaskid = 1,

                              starttime = a.login

                          }).OrderBy(o => o.EmployeeName).ToList().Select(o => new employeetimesheet
                          {
                              entryid = o.entryid,
                              EmployeeName = o.EmployeeName,
                              servocedatefrom = o.servocedatefrom,
                              servocedateto = o.servocedateto,
                              protaskid = o.protaskid,
                              entrydate = o.endtime,
                              hours = (o.endtime - o.starttime).Value.Hours,
                              minute = (o.endtime - o.starttime).Value.Minutes,
                          }).OrderBy(o => o.EmployeeName).ThenBy(o => o.entrydate).ToList();
                var gp = (from v in sr
                          group new { v.EmployeeName, v.hours, v.minute, v.entryid, v.servocedatefrom, v.servocedateto } by new { v.entrydate, v.EmployeeName } into gt
                          select new employeetimesheet
                          {
                              entryid = gt.FirstOrDefault().entryid,
                              EmployeeName = gt.Key.EmployeeName,
                              entrydate = gt.Key.entrydate,
                              totlhour = gt.Sum(o => o.hours),
                              totalminute = gt.Sum(o => o.minute),
                              servocedatefrom = gt.FirstOrDefault().servocedatefrom,
                              servocedateto = gt.FirstOrDefault().servocedateto
                          }).ToList();

                var thour = gp.Sum(o => o.totlhour);
                var tminute = gp.Sum(o => o.totalminute);
                dutystartime = gp.Min(o => o.servocedatefrom);
                dutylaststarttime = gp.Max(o => o.servocedatefrom);
                dutyendtime = gp.Max(o => o.servocedateto);
                if (dutylaststarttime == dutyendtime)
                    thour = 0;
                else
                    thour = (decimal)(dutyendtime - dutystartime).Value.TotalHours;
                var tt = thour + tminute / 60;
                decimal? overtime = null;
                if (tt == null)
                    overtime = -8;
                else if (tt > 7)
                    overtime = tt - 10;
                else if (tt >= 4 && tt <= 7)
                    overtime = tt - 9;
                else if (tt < 4)
                    overtime = tt - 8;
                else
                    overtime = -8;

                var te = 0;
                var Data = new Dictionary<string, object>();
                if (gp.Count() > 0)
                    entryid = gp.Max(o => o.entryid);

                var ttinhour = thour + (int)tminute / 60 + (tminute % 60) / 100;
                Data.Add("Totalhour", tt);
                Data.Add("overtime", overtime);
                Data.Add("dutystart", dutystartime);
                Data.Add("dutyend", dutyendtime);
                Data.Add("entryid", entryid);
                Data.Add("dutylaststartime", dutylaststarttime);
                return Data;

            }

        }
        public Dictionary<string, object> getemployeeworkinghourall(long empid, DateTime date)
        {
            DateTime? dutystartime = null;
            DateTime? dutyendtime = null;
            DateTime? dutylaststarttime = null;
            long? entryid = null;
            var nextdaay = date.AddDays(1);
            DateTime currdate = System.DateTime.Now;
            var firstentrytask = (from a in db.servicereports
                                  join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                                  join c in db.Employees on b.employeeid equals c.EmployeeId

                                  where c.EmployeeId == empid && a.starttime >= date && a.starttime < nextdaay
                                  select new
                                  {
                                      a.starttime
                                  }).Select(o => o.starttime).AsEnumerable().DefaultIfEmpty(currdate).FirstOrDefault();
            var firstentryapp = (from a in db.EmpAttendances
                                 join b in db.Employees on a.EmployeeName equals b.UserId
                                 where a.login >= date && a.login < nextdaay
                                 && b.EmployeeId == empid
                                 select new
                                 {
                                     a.login
                                 }).Select(o => o.login).AsEnumerable().DefaultIfEmpty(currdate).FirstOrDefault();
            if (firstentrytask < firstentryapp)
            {
                var sr = (from a in db.servicereports
                          join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                          join c in db.Employees on b.employeeid equals c.EmployeeId

                          where c.EmployeeId == empid && a.starttime >= date && a.starttime < nextdaay
                          select new employeetimesheet
                          {
                              entryid = a.servicereportid,
                              EmployeeName = c.FirstName + " " + c.LastName,
                              servocedatefrom = a.starttime,
                              servocedateto = a.endtime,
                              endtime = (a.endtime < a.starttime) ? DbFunctionsCompat.AddDays(a.endtime, 1) : a.endtime,
                              protaskid = a.protaskid,
                              leavestatus=null,
                              starttime = a.starttime

                          }).OrderBy(o => o.EmployeeName).ToList().Select(o => new employeetimesheet
                          {
                              entryid = o.entryid,
                              EmployeeName = o.EmployeeName,
                              servocedatefrom = o.servocedatefrom,
                              servocedateto = o.servocedateto,
                              protaskid = o.protaskid,
                              entrydate = o.endtime,
                              hours = (o.endtime - o.starttime).Value.Hours,
                              minute = (o.endtime - o.starttime).Value.Minutes,
                          }).OrderBy(o => o.EmployeeName).ThenBy(o => o.entrydate).ToList();
                var gp = (from v in sr
                          group new { v.EmployeeName, v.entryid, v.hours, v.minute, v.servocedatefrom, v.servocedateto } by new { v.entrydate, v.EmployeeName } into gt
                          select new employeetimesheet
                          {
                              entryid = gt.FirstOrDefault().entryid,
                              EmployeeName = gt.Key.EmployeeName,
                              entrydate = gt.Key.entrydate,
                              totlhour = gt.Sum(o => o.hours),
                              totalminute = gt.Sum(o => o.minute),
                              servocedatefrom = gt.FirstOrDefault().servocedatefrom,
                              servocedateto = gt.FirstOrDefault().servocedateto

                          }).ToList();
                Dictionary<string, object> Balance = null;
                var thour = gp.Sum(o => o.totlhour);
                var tminute = gp.Sum(o => o.totalminute);
                var tt = thour + tminute / 60;
                if (gp.Count() > 0)
                    entryid = gp.Max(o => o.entryid);
                decimal? overtime = null;
                if (tt == null)
                    overtime = -8;
                else if (tt > 7)
                    overtime = tt - 10;
                else if (tt >= 4 && tt <= 7)
                    overtime = tt - 9;
                else if (tt < 4)
                    overtime = tt - 8;
                else
                    overtime = -8;
                dutystartime = gp.Min(o => o.servocedatefrom);
                dutylaststarttime = System.DateTime.Now;// gp.Max(o => o.servocedatefrom);
                dutyendtime = gp.Max(o => o.servocedateto);
                decimal? te = 0;
                var Data = new Dictionary<string, object>();

                var ttinhour = thour + (int)tminute / 60 + (tminute % 60) / 100;
                Data.Add("Totalhour", tt);
                Data.Add("overtime", overtime);
                Data.Add("dutystart", dutystartime);
                Data.Add("dutyend", dutyendtime);
                Data.Add("entryid", entryid);
                Data.Add("leavestatus", null);
                Data.Add("dutylaststartime", dutylaststarttime);
                return Data;

            }
            else
            {
                var nday = date.AddHours(22);
                var sr = (from a in db.EmpAttendances
                          join b in db.Employees on a.EmployeeName equals b.UserId
                          where a.login >= date && a.login < nday
                          && b.EmployeeId == empid
                          select new employeetimesheet
                          {
                              entryid = a.Id,
                              EmployeeName = b.FirstName + " " + b.LastName,
                              servocedatefrom = a.login,
                              servocedateto = (a.logout == null) ? a.login : a.logout,
                              endtime = (a.logout == null) ? a.login : a.logout,
                              protaskid = 1,
                              leavestatus=a.leavestatus,
                              starttime = a.login

                          }).OrderBy(o => o.EmployeeName).ToList().Select(o => new employeetimesheet
                          {
                              entryid = o.entryid,
                              EmployeeName = o.EmployeeName,
                              servocedatefrom = o.servocedatefrom,
                              servocedateto = o.servocedateto,
                              protaskid = o.protaskid,
                              entrydate = o.endtime,
                              leavestatus=o.leavestatus,
                              hours = (o.endtime - o.starttime).Value.Hours,
                              minute = (o.endtime - o.starttime).Value.Minutes,
                          }).OrderBy(o => o.EmployeeName).ThenBy(o => o.entrydate).ToList();
                var gp = (from v in sr
                          group new { v.EmployeeName, v.hours, v.minute, v.entryid, v.servocedatefrom, v.servocedateto,v.leavestatus } by new { v.entrydate, v.EmployeeName } into gt
                          select new employeetimesheet
                          {
                              entryid = gt.FirstOrDefault().entryid,
                              EmployeeName = gt.Key.EmployeeName,
                              entrydate = gt.Key.entrydate,
                              totlhour = gt.Sum(o => o.hours),
                              totalminute = gt.Sum(o => o.minute),
                              leavestatus=gt.FirstOrDefault().leavestatus,
                              servocedatefrom = gt.FirstOrDefault().servocedatefrom,
                              servocedateto = gt.FirstOrDefault().servocedateto
                          }).ToList();

                var thour = gp.Sum(o => o.totlhour);
                var tminute = gp.Sum(o => o.totalminute);
                dutystartime = gp.Min(o => o.servocedatefrom);
                dutylaststarttime = gp.Max(o => o.servocedatefrom);
                dutyendtime = gp.Max(o => o.servocedateto);
                if (dutylaststarttime == dutyendtime)
                    thour = 0;
                else
                    thour = (decimal)(dutyendtime - dutystartime).Value.TotalHours;
                var tt = thour + tminute / 60;
                decimal? overtime = null;
                if (tt == null)
                    overtime = -8;
                else if (tt > 7)
                    overtime = tt - 10;
                else if (tt >= 4 && tt <= 7)
                    overtime = tt - 9;
                else if (tt < 4)
                    overtime = tt - 8;
                else
                    overtime = -8;

                var te = 0;
                var Data = new Dictionary<string, object>();
                if (gp.Count() > 0)
                    entryid = gp.Max(o => o.entryid);
                var leavst = gp.Max(o => o.leavestatus);
                var ttinhour = thour + (int)tminute / 60 + (tminute % 60) / 100;
                Data.Add("Totalhour", tt);
                Data.Add("overtime", overtime);
                Data.Add("dutystart", dutystartime);
                Data.Add("dutyend", dutyendtime);
                Data.Add("entryid", entryid);
                Data.Add("dutylaststartime", dutylaststarttime);
                Data.Add("leavestatus", leavst);
                return Data;

            }

        }
        //[HttpPost]




        //    //Find Order Column
        //               where b.TeamId == 4
        //                    monthid =










        [HttpPost]
        public ActionResult getalltime(long empid, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            //Find Order Column

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            DateTime? fdate = null;
            DateTime? tdate = null;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB").DateTimeFormat);
                tdate = tdate.Value.AddDays(1);
            }

            var v = (from a in db.servicereports
                     join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                     join c in db.Employees on b.employeeid equals c.EmployeeId
                     where b.employeeid == empid &&
                      (fromdate == "" || fromdate == null || EF.Functions.DateDiffDay(a.starttime, fdate) <= 0) &&

                      (todate == "" || fromdate == null || EF.Functions.DateDiffDay(a.endtime, tdate) > 0)


                     select new
                     {
                         entryid = a.servicereportid,
                         EmployeeName = c.FirstName + " " + c.LastName,
                         servocedatefrom = a.starttime,
                         servocedateto = a.endtime,
                         endtime = (a.endtime < a.starttime) ? DbFunctionsCompat.AddDays(a.endtime, 1) : a.endtime
                         ,
                         a.starttime

                     }).ToList().Select(o => new
                     {
                         o.entryid,
                         o.EmployeeName,
                         o.servocedatefrom,
                         o.servocedateto,
                         o.starttime,
                         o.endtime,
                         hours = (o.endtime - o.starttime).Value.Hours,
                         minute = (o.endtime - o.starttime).Value.Minutes,
                     });
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT

            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: calendar/taskcalendar/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskAssigned taskAssigned = db.TaskAssigneds.Find(id);
            if (taskAssigned == null)
            {
                return NotFound();
            }
            return View(taskAssigned);
        }

        // GET: calendar/taskcalendar/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: calendar/taskcalendar/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("TaskAssignedId,ProTaskId,EmployeeId,AssignBy,Status,chkStatus,CreatedDate")] TaskAssigned taskAssigned)
        {
            if (ModelState.IsValid)
            {
                db.TaskAssigneds.Add(taskAssigned);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(taskAssigned);
        }

        // GET: calendar/taskcalendar/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskAssigned taskAssigned = db.TaskAssigneds.Find(id);
            if (taskAssigned == null)
            {
                return NotFound();
            }
            return View(taskAssigned);
        }

        // POST: calendar/taskcalendar/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("TaskAssignedId,ProTaskId,EmployeeId,AssignBy,Status,chkStatus,CreatedDate")] TaskAssigned taskAssigned)
        {
            if (ModelState.IsValid)
            {
                db.Entry(taskAssigned).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(taskAssigned);
        }

        // GET: calendar/taskcalendar/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskAssigned taskAssigned = db.TaskAssigneds.Find(id);
            if (taskAssigned == null)
            {
                return NotFound();
            }
            return View(taskAssigned);
        }

        // POST: calendar/taskcalendar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            TaskAssigned taskAssigned = db.TaskAssigneds.Find(id);
            db.TaskAssigneds.Remove(taskAssigned);
            db.SaveChanges();
            return RedirectToAction("Index");
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
