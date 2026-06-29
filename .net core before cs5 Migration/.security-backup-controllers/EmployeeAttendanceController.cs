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
using QuickSoft.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net;
using System.Drawing;
using System.Globalization;
using QuickSoft.ViewModel;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class EmployeeAttendanceController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public EmployeeAttendanceController()
        {
            db = new ApplicationDbContext();
            com = new Common();

        }
        // GET: EmployeeAttendance
        public ActionResult Index()
        {
            db = new ApplicationDbContext();
            com = new Common();
            var created = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            var createdtask = db.ProTasks.Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            }).Take(1).ToList();





            ViewBag.Task = QkSelect.List(createdtask, "ID", "Name");
            return View();
        }
        [HttpGet]
        //  [QkAuthorize(Roles = "Dev,Get Start Attendance")]
        public ActionResult Create(long? Id)
        {
            return RedirectToAction("createatt", "Table");
            db = new ApplicationDbContext();
            com = new Common();


            if (1 == 1)
                {
                    var Userid = User.Identity.GetUserId();
                    var max = from a in db.EmpAttendances
                              where a.EmployeeName == Userid && a.Status != null
                              orderby a.Id descending
                              select a.Status;
                    if (max.FirstOrDefault() == "Active")
                    {
                        ViewBag.laststatus = max.FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.laststatus = "Expired";
                    }

                }
                else
                {
                    ViewBag.laststatus = "Expired";
                }

                var Useriddd = User.Identity.GetUserId();
                var dura = from a in db.EmpAttendances
                           where a.EmployeeName == Useriddd && a.Status != null
                           orderby a.Id descending
                           select a.login;
                var durb = from a in db.EmpAttendances
                           where a.EmployeeName == Useriddd && a.Status != null
                           orderby a.Id descending
                           select a.logout;
                var Durationsa = dura.FirstOrDefault();
                var Durationsb = durb.FirstOrDefault();
                if (Durationsb == null)
                {
                    if (Durationsa != null)
                    {
                        ViewBag.duration = Durationsa;
                    }
                    else
                    {
                        ViewBag.duration = "0";
                    }

                }
                else
                {
                    ViewBag.duration = "0";
                }




                var Useridd = User.Identity.GetUserId();
                var lname = from f in db.Users
                            where f.Id == Useridd
                            select f.UserName;
                ViewBag.lastname = lname.FirstOrDefault();
                ViewBag.now = System.DateTime.Now;
                db.Dispose();
                return View();
         
        }
        private double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                    dist = dist * 1000;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Create Start Attendance")]
        public ActionResult Create(string login, string logout, string dura, string lat, string log,string fromapp)
        {
            db = new ApplicationDbContext();
            com = new Common();
            if (lat == "" ||  lat == "null")
            {
                var u = User.Identity.GetUserId();
                DateTime t = DateTime.Now.AddDays(-100);
                var amcopenandclose = (from a in db.LogManagers
                                       join c in db.Employees on a.User equals c.UserId

                                       where
                                     a.LogTime >= t

                                     && a.User == u
                                      && a.LogSection == "gpstrack"
                                       select new points
                                       {
                                           lat = a.LogTable,
                                           log = a.LogDetails,
                                           logtime = a.LogTime
                                       }).OrderByDescending(o => o.logtime).FirstOrDefault();
                if (amcopenandclose != null)
                {
                    lat = amcopenandclose.lat;
                    log = amcopenandclose.log;
                }
            }
                if (lat==""||lat=="null")
            {

                if (1 == 1)
                {
                    Danger("attendance not saved,Your app locaton permission not ok.give allow location permission in app permissions, or uninstall and reinstall app", true);
                    //    ID = s.Id,
                    //    Name = s.UserName

                    //            where f.Id == Useridd
                    var created = db.Users.Select(s => new
                    {
                        ID = s.Id,
                        Name = s.UserName
                    }).ToList();
                    ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

                    var Useridd = User.Identity.GetUserId();
                    var lname = from f in db.Users
                                where f.Id == Useridd 
                                select f.UserName;
                    ViewBag.lastname = lname.FirstOrDefault();
                    var Userid = User.Identity.GetUserId();
                    var max = from a in db.EmpAttendances
                              where a.EmployeeName == Userid && a.Status != null
                              orderby a.Id descending
                              select a.Status;
                    if (max.FirstOrDefault() == "Active")
                    {
                        ViewBag.laststatus = max.FirstOrDefault();
                    }
                    else
                    {
                        ViewBag.laststatus = "Expired";
                    }
                    var Useriddd = User.Identity.GetUserId();
                    var duraa = from a in db.EmpAttendances
                                where a.EmployeeName == Useriddd && a.Status != null
                                orderby a.Id descending
                                select a.login;
                    var durb = from a in db.EmpAttendances
                               where a.EmployeeName == Useriddd && a.Status != null
                               orderby a.Id descending
                               select a.logout;
                    var Durationsa = duraa.FirstOrDefault();
                    var Durationsb = durb.FirstOrDefault();
                    if (Durationsb == null)
                    {
                        if (Durationsa != null)
                        {
                            ViewBag.duration = Durationsa;
                        }
                        else
                        {
                            ViewBag.duration = "0";
                        }

                    }
                    else
                    {
                        ViewBag.duration = "0";
                    }

                    return View();
                }
            }
            var UserId = User.Identity.GetUserId();
            var empid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
            var lattandlong = db.geowalls.Where(o => o.EmployeeId == empid).Select(o => new
            {
                o.lat,
                o.log,
                o.distance
            }).ToList();
            
            bool stat = false;
            string msg;
            var flag = 1;
            foreach (var orl in lattandlong)
            {
                var orgdist = distance(Convert.ToDouble(lat), Convert.ToDouble(log), Convert.ToDouble(orl.lat), Convert.ToDouble(orl.log), 'K');

                if (orgdist < 0)
                    orgdist = orgdist * -1;

                if (orgdist >= Convert.ToDouble(orl.distance))
                {
                    flag = 1;
                }
                else
                {
                    flag = 0;
                    break;
                }
            }
            if(flag==1)
            {
                Danger("You Are  Away From Point,if you sure your postion. app kill (swaipt and kill) and open again to take your corrent location",true);
                ViewBag.laststatus = "Expired";
                ViewBag.duration = "0";
                var created = db.Users.Select(s => new
                {
                    ID = s.Id,
                    Name = s.UserName
                }).ToList();
                ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

                var Useridd = User.Identity.GetUserId();
                var lname = from f in db.Users
                            where f.Id == Useridd
                            select f.UserName;
                ViewBag.lastname = lname.FirstOrDefault();
                return View();
            }
            if (ModelState["ID"] != null)
            {
                ModelState["ID"].Errors.Clear();
            }
            if (empid == null || empid == 0)
                ModelState.AddModelError("102", "User Not Connect With Employee");
            if (ModelState.IsValid)
            {
                var Userid = User.Identity.GetUserId();

                var DateTime = System.DateTime.Now;
                if (login != null)
                {
                    var atttendance = new EmpAttendance
                    {
                        EmployeeName = User.Identity.GetUserId(),
                        Status = "Active",
                        login = DateTime,
                        latitude = lat,
                        logitude = log,
                        //   protaskid = 1,
                    };
                    db.EmpAttendances.Add(atttendance);
                    db.SaveChanges();
                    //EmpAttDetails empdt = new EmpAttDetails
                    //    protaskid = 1,
                    //    taskstatusid = 3,
                    //    userid = Userid,
                    //    starttime = System.DateTime.Now,
                    //    empattid = atttendance.Id,

                }
                if (logout != null)
                {
                    var maxo = from a in db.EmpAttendances
                               where a.EmployeeName == Userid
                                && a.Status== "Active"
                               orderby a.login descending
                               select a.Id;
                    var lastid = maxo.FirstOrDefault();


                    EmpAttendance lastlog = db.EmpAttendances.Find(lastid);
                    var duration = (DateTime) - (lastlog.login);
                    lastlog.EmployeeName = User.Identity.GetUserId();
                    lastlog.Status = "Expired";
                    lastlog.logout = DateTime;
                    lastlog.endlatitude = lat;
                    lastlog.endlogitude = log;
                    db.Entry(lastlog).State = EntityState.Modified;
                    db.SaveChanges();
                     maxo = from a in db.EmpAttendances
                               where a.EmployeeName == Userid
                                && a.Status == "Active"
                               orderby a.login descending
                               select a.Id;
                     lastid = maxo.FirstOrDefault();


                     lastlog = db.EmpAttendances.Find(lastid);
                     duration = (DateTime) - (lastlog.login);
                    lastlog.EmployeeName = User.Identity.GetUserId();
                    lastlog.Status = "Expired";
                    lastlog.logout = DateTime;
                    lastlog.endlatitude = lat;
                    lastlog.endlogitude = log;
                    db.Entry(lastlog).State = EntityState.Modified;
                    db.SaveChanges();

                    var today = Convert.ToDateTime(System.DateTime.Now);
                    long dailyattid = 0;
                    today = today.AddDays(0);
                    var date = today.Date;
                    var date2 = today.Date.AddDays(1);
                

                     





                }





            }
            if (ModelState.IsValid)
            {
                com.addlog(LogTypes.Updated, UserId, "gpstrack", lat, findip(), 1, log);
                msg = "Successfully Uploaded";
                stat = true;
                Success("Attendance Registered", true);

            }
            else
            {
                msg = "User not connected with employee";
                stat = false;
                Danger(msg, true);
            }
            db.Dispose();
            return Redirect(ControllerContext.HttpContext.Request.GetUrlReferrer().ToString());
        }


        [HttpPost]
        public ActionResult GetAttendance(string user, string login, string logout, string task, bool loggedIn)
        {
            db = new ApplicationDbContext();
            com = new Common();
            var logStat = "";
            if (loggedIn == true)
            {
                logStat = "Active";
            }
            DateTime? exdate = null;
            DateTime? crdate = null;
            var userfound = "";
            var taskfound = "";

            if (logout != "")
            {
                exdate = DateTime.Parse(logout, new CultureInfo("en-GB"));
            }
            else
            {
                exdate = System.DateTime.Now.Date.AddDays(-60);
            }
            if (login != "")
            {
                crdate = DateTime.Parse(login, new CultureInfo("en-GB"));
            }
            else
            {
                crdate = System.DateTime.Now.Date;
            }
            if (user != "")
            {
                long empid = Convert.ToInt64(user);
                Employee emp = db.Employees.Find(empid);
                userfound = emp.UserId;
            }
            if (task != "")
            {
                long taskid = Convert.ToInt64(task);
                ProTask tmp = db.ProTasks.Find(taskid);
                taskfound = tmp.TaskName;
            }
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();


            var v = (from a in db.EmpAttendances
                     join c in db.Users on a.EmployeeName equals c.Id into utype
                     from c in utype.DefaultIfEmpty()
                     join d in db.ProTasks on a.protaskid equals d.ProTaskId into proo
                     from d in proo.DefaultIfEmpty()
                     join g in db.Employees on a.EmployeeName equals g.UserId into htype
                     from g in htype.DefaultIfEmpty()




                     where (user == null || user == "" || g.UserId == userfound) &&
                    (logStat == null || logStat == "" || a.Status == logStat)&&
                       (task == null || task == "" || d.TaskName == taskfound) &&
                       (login == null || login == "" || EF.Functions.DateDiffDay(a.login, crdate) <= 0) &&
                      (logout == null || logout == "" || EF.Functions.DateDiffDay(a.logout, exdate) >= 0)

                     select new
                     {
                         empattdetailsid=a.Id,
                         taskname = d.TaskName,
                         starttime = a.login,
                         // StatusName = f.TaskName,
                         logout = a.logout,
                         user = g.FirstName + " " + g.MiddleName + " " + g.LastName,
                         a.Status,
                         startl = "https://maps.google.com/?q=" + a.latitude + "," + a.logitude,
                         endl = "https://maps.google.com/?q=" + a.endlatitude + "," + a.endlogitude,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(
                    p => p.empattdetailsid.ToString().ToLower().Contains(search.ToLower()) ||
                    p.user.ToString().ToLower().StartsWith(search.ToLower()) ||
                    p.Status.ToString().ToLower().EndsWith(search.ToLower()) ||
                    p.starttime.ToString().ToLower().StartsWith(search.ToLower())
                    );

            }
            //SORT

                v = v.OrderByDescending(c => c.logout==null).ThenByDescending(n => n.starttime);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        //------------Employee Attentance Report
        //        ID = s.Id,
        //        Name = s.UserName

        [HttpPost]
        public ActionResult GetReport(string user, string From, string To)
        {
            DateTime? exdate = null;
            DateTime? crdate = null;

            if (To != "")
            {
                exdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }
            if (From != "")
            {
                crdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }

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
            var UserId = User.Identity.GetUserId();


            var v = (from c in db.Users
                     where (from a in db.EmpAttendances
                            where a.EmployeeName == c.Id
                              && (From == null || From == "" || EF.Functions.DateDiffDay(a.login, crdate) <= 0)
                              && (To == null || To == "" || EF.Functions.DateDiffDay(a.login, exdate) >= 0)
                            select a).Any()
                        && (user == null || user == "" || c.Id == user)
                     select new
                     {

                         id = (long)(from a in db.EmpAttendances
                                     where a.EmployeeName == c.Id
                                       && (From == null || From == "" || EF.Functions.DateDiffDay(a.login, crdate) <= 0)
                                       && (To == null || To == "" || EF.Functions.DateDiffDay(a.login, exdate) >= 0)
                                     select (long?)a.Id).FirstOrDefault(),
                         user = c.UserName,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.id.ToString().ToLower().Equals(search.ToLower()));

            }
            //SORT

            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.Distinct().AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [HttpPost]
        public JsonResult tockenstr(string tk)
        {
            bool stat = true;
            string msg = "EmployeeAttendance Deleted Successfully.";
            if (tk == "")
                return Json(new { status = stat, message = msg });
            
            var userid = User.Identity.GetUserId();
            var exist = db.LogManagers.Any(o => o.User == userid && o.LogSection == tk);
            if(exist)
            {
                return Json(new { status = stat, message = msg });
            }
          if(userid!=null|| userid!="")
            com.addlog(LogTypes.Updated, userid, tk, "firebase", findip(), 1, "firebase");
            msg = "";
            DateTime rmd = System.DateTime.Now.Date.AddDays(-30);
            var v = (from a in db.Reminders
                     join b in db.Employees on a.CreatedBy equals b.UserId
                     where a.CreatedDate >= rmd &&
                     a.RequestBy == userid &&
                     a.RStatus == "Open"
                     && a.Note != ""
                     select new
                     {

                         note = a.Note,
                         empname = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                         a.ReminderId

                     }).OrderByDescending(o=>o.ReminderId).FirstOrDefault();

            string sender = v.empname;
            msg = v.note;
            var ed = db.Reminders.Where(o => o.ReminderId == v.ReminderId).FirstOrDefault();
            ed.RStatus = "Close";
            db.Entry(ed).State = EntityState.Modified;
            db.SaveChanges();

        
                  
            return Json(new { status = stat,from=sender, message = msg });

        }
        public bool IsDisposed()
        {
            bool result = true;
            var typeDbContext = typeof(DbContext);
            var isDisposedTypeField = typeDbContext.GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);

            if (isDisposedTypeField != null)
            {
                result = (bool)isDisposedTypeField.GetValue(this);
            }

            return result;
        }
        [HttpGet]
       
        public JsonResult locationstttttttt(string lat,string log)
        {
            

            bool stat = true;
            string msg = "EmployeeAttendance Deleted Successfully.";
            string url = "";
            string ver = "1.2";
        

            var userid = User.Identity.GetUserId();

            if (userid != null)
            {
                if ( IsDisposed())
                {
                    db = new ApplicationDbContext();
                    com = new Common();

                    var emid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
                    DateTime sysd = System.DateTime.Now;
                    DateTime sys=System.DateTime.Now.Date.AddDays(-1);
                    var v = (from aa in db.Reminders
                             join b in db.Employees on aa.CreatedBy equals b.UserId
                             join c in db.ReminderAssigneds on aa.ReminderId equals c.ReminderId
                             where

                              aa.Note != ""
                             && c.EmployeeId == emid
                             && aa.RDate < sysd
                             && aa.RDate>=sys
                             && aa.RStatus != "Close"
                    select new
                             {

                                 note = aa.Note,
                                 empname = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                                 aa.ReminderId,
                                 ur = aa.Type

                             }).OrderByDescending(o => o.ReminderId).FirstOrDefault();

                    string sender = "";
                    msg = "";

                    if (v != null)
                    {
                        (from aaa in db.Reminders
                                  join b in db.Employees on aaa.CreatedBy equals b.UserId
                                  join c in db.ReminderAssigneds on aaa.ReminderId equals c.ReminderId
                                  where

                                   aaa.Note == v.note
                                  && c.EmployeeId == emid
                                 
                                  && aaa.RStatus != "Close"

                                  select aaa
                                  ).ToList().ForEach(dd => dd.RStatus = "Close");

                        db.SaveChanges();
                        sender = v.empname;
                        msg = v.note;
                        url = v.ur;


                    }

                    return Json(new { status = stat, from = sender, message = msg, myurl = url, version = ver });

                    if (log == "" || log == null)
                        return Json(new { status = stat, from = sender, message = msg, myurl = url, version = ver });

                    DateTime dt = System.DateTime.Now.AddMinutes(-20);

                    var ldate = db.LogManagers.Any(o => o.LogSection == "gpstrack" && o.User == userid && o.LogTime >= dt);
                    if (ldate == false)
                        com.addlog(LogTypes.Updated, userid, "gpstrack", lat, findip(), 2, log);
                    else
                        return Json(new { status = stat, from = sender, message = msg, myurl = url, version = ver });

                    DateTime a = System.DateTime.Now.AddDays(-1);
                    db.LogManagers.RemoveRange(db.LogManagers.Where(o => o.LogSection == "gpstrack" && o.LogTime <= a));
                    db.SaveChanges();
                    return Json(new { status = stat, from = sender, message = msg, myurl = url, version = ver });

                }
            else
                {

                    return Json(new { status = true, from = "", message = "", myurl = "", version = ver });


                }
            }
            else
            {

                return Json(new { status = true, from = "", message = "", myurl = "", version = ver });


            }
        }
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Delete FileDocument")]
        public ActionResult Delete(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmpAttDetails selectattentance = db.EmpAttDetails.Find(Id);
            if (selectattentance == null)
            {
                return NotFound();
            }
            return PartialView(selectattentance);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete FileDocument")]
        public JsonResult Delete(long empattdetailsid)
        {
            EmpAttDetails selectattentance = db.EmpAttDetails.Find(empattdetailsid);
            bool stat = false;
            string msg;
            if (selectattentance != null)
            {

                db.EmpAttDetails.Remove(selectattentance);
                db.SaveChanges();
                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "EmployeeAttendance", "EmployeeAttendance", findip(), (long)selectattentance.empattdetailsid, "Details Deleted Successfully");
            }



            stat = true;
            msg = "EmployeeAttendance Deleted Successfully.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Delete FileDocument")]
        public ActionResult Edit(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmpAttDetails selectattentancedeteils = db.EmpAttDetails.Find(Id);
            EmpAttendance selectempattendance = db.EmpAttendances.Where(a => a.Id == selectattentancedeteils.empattid).FirstOrDefault();
            Employee selectemployee = db.Employees.Where(a => a.UserId == selectattentancedeteils.userid).FirstOrDefault();
            var newemp = new EmpAttendanceViewModel()
            {
                EmpattendancedetailsId = selectattentancedeteils.empattdetailsid,
                EmpattendanceId = selectattentancedeteils.empattid,
                starttime = selectattentancedeteils.starttime,
                EmployeeId = selectemployee.EmployeeId,
                logout = selectempattendance.logout



            };
            if (selectattentancedeteils == null)
            {
                return NotFound();
            }
            if(1==1)
            {

                //---------all user dropdown-------
                var user = db.Employees.Select(s => new
                {
                    ID = s.EmployeeId,
                    Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                }).ToList();
                ViewBag.User = QkSelect.List(user, "ID", "Name");
                //-----------------------------





            }
            return PartialView(newemp);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[QkAuthorize(Roles = "Dev,Edit EmployeeAttendance")]
        public JsonResult Edit(EmpAttendanceViewModel emmodel)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                Employee selectemployee = db.Employees.Where(a => a.EmployeeId == emmodel.EmployeeId).FirstOrDefault();
                EmpAttDetails lastdetails = db.EmpAttDetails.Find(emmodel.EmpattendancedetailsId);
                lastdetails.userid = selectemployee.UserId;
                lastdetails.starttime = emmodel.starttime;
                db.Entry(lastdetails).State = EntityState.Modified;
                db.SaveChanges();


                EmpAttendance lastlog = db.EmpAttendances.Find(emmodel.EmpattendanceId);
                lastlog.EmployeeName = selectemployee.UserId;
                lastlog.logout = emmodel.logout;
                db.Entry(lastlog).State = EntityState.Modified;
                db.SaveChanges();

                msg = "Successfully updated attendance details.";
                stat = true;

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public JsonResult UserSearch(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Employees.Where(p => p.UserId != "" && (p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Employees.Select(b => new SelectFormat
                {
                    text = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                    id = b.EmployeeId
                }).OrderBy(b => b.text).ToList();
            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public JsonResult TaskSearch(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProTasks.Where(p => p.TaskName != "" && (p.TaskName.ToLower().Contains(q.ToLower()) || p.TaskName.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.TaskName,
                                      id = b.ProTaskId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ProTasks.Select(b => new SelectFormat
                {
                    text = b.TaskName,
                    id = b.ProTaskId
                }).OrderBy(b => b.text).ToList();
            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


    }
}
