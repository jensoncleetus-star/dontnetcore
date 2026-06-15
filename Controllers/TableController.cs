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
using Microsoft.AspNetCore.Identity;
using QuickSoft.ViewModel;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class tableController : BaseController
    {
        // GET: Department
        //[QkAuthorize(Roles = "Dev,Table List")]
        public ActionResult Index()
        {
            return View();
        }
        ApplicationDbContext db;
        Common com;
        public tableController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }


        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Table List")]
        public JsonResult GetTable()
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
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = db.Tables.Select(b => new
            {
                id=b.TableId,
                b.TableName,
                b.TableStatus,
                b.Description,
                b.MaxSeats,
                Area = db.Areas.Where(a => a.AreaId == b.AreaId).Select(a => a.AreaName).FirstOrDefault(),
                User = db.Users.Where(a => a.Id == b.CreatedBy).Select(a => a.UserName).FirstOrDefault(),
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.TableName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.TableStatus.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.MaxSeats.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Area.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.User.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Description.ToString().ToLower().Contains(search.ToLower()) 
                );
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
        //  [QkAuthorize(Roles = "Dev,Get Start Attendance")]
        public ActionResult Createatt(long? Id)
        {
            
            try
            {
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
            catch (Exception e)
            {
                return RedirectToAction("register", "Users");
            }
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
        public ActionResult Createatt(string login, string logout, string dura, string lat, string log, string fromapp)
        {
            db = new ApplicationDbContext();
            com = new Common();
            if (lat == "" || lat == "null")
            {
                var u = User.Identity.GetUserId();
                DateTime t = DateTime.Now.AddDays(-10);
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
            if (lat == "" || lat == "null")
            {

                //        ID = s.Id,
                //        Name = s.UserName

                //                where f.Id == Useridd
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
            //        ID = s.Id,
            //        Name = s.UserName

            //                where f.Id == Useridd
            //              where a.EmployeeName == Userid
            //              orderby a.Id descending
            //               where a.EmployeeName == Useriddd
            //                orderby a.Id descending
            //               where a.EmployeeName == Useriddd
            //               orderby a.Id descending

            if (ModelState["ID"] != null)
            {
                ModelState["ID"].Errors.Clear();
            }
           
            if (1==1)
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
                    Success("Attendance Registered", true);
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
                                && a.Status == "Active"
                               orderby a.login descending
                               select a.Id;

                    if (maxo != null)
                    {
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

                    }
                    else
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
                        maxo = from a in db.EmpAttendances
                               where a.EmployeeName == Userid
                                && a.Status == "Active"
                               orderby a.login descending
                               select a.Id;

                        if (maxo != null)
                        {
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

                        }
                    }















                    Success("Attendance Registered", true);


                    var today = Convert.ToDateTime(System.DateTime.Now);
                    long dailyattid = 0;
                    today = today.AddDays(0);
                    var date = today.Date;
                    var date2 = today.Date.AddDays(1);








                }





            }
            if (1==1)
            {
                com.addlog(LogTypes.Updated, UserId, "gpstrack", lat, findip(), 1, log);
                msg = "Successfully Uploaded";
                stat = true;
              

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

        // GET: Field/Create
        //[QkAuthorize(Roles = "Dev,Create Table")]
        public ActionResult Create()
        {
            var area = db.Areas.Select(s => new
            {
                AreaId = s.AreaId,
                AreaName = s.AreaName
            }).ToList();
            ViewBag.Area = QkSelect.List(area, "AreaId", "AreaName");
            TableViewModel vmodel = new TableViewModel
            {
                Areas = db.Areas.ToList()
            };
            return View(vmodel);
        }

        // POST: Dep/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[QkAuthorize(Roles = "Dev,Create Table")]
        public ActionResult Create(Table tble)
        {
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.Tables.Any(c => c.TableName == tble.TableName);
                if (Exists)
                {
                    Danger("Table Name already exists.", false);
                    return RedirectToAction("Create", "Table");
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var tbl = new Table
                    {
                        TableName = tble.TableName,
                        MaxSeats=tble.MaxSeats,
                        Description=tble.Description,
                        TableStatus=tble.TableStatus,
                        AreaId=tble.AreaId,

                        CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                        CreatedBy = UserId,
                        Branch = Convert.ToInt64(BranchID),
                        Status = Status.active

                    };
                    db.Tables.Add(tbl);
                    db.SaveChanges();
                    Id = tbl.TableId;
                    com.addlog(LogTypes.Created, UserId, "Table", "Tables", findip(), tble.TableId, "Table Added Successfully");

                    Success("Successfully added Table details.", true);
                    return RedirectToAction("Create", "Table");
                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return (View());
            }
        }


        // GET: dep/Edit/5
        //[QkAuthorize(Roles = "Dev,Edit Table")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Table tble = db.Tables.Find(id);

            if (tble == null)
            {
                return NotFound();
            }
            var tab = new TableViewModel
            {
                TableName=tble.TableName,
                AreaId=tble.AreaId,
                MaxSeats=tble.MaxSeats,
                TableStatus=tble.TableStatus,
                Description=tble.Description,
                Areas = db.Areas.ToList()
            };
            var area = db.Areas.Select(s => new
            {
                AreaId = s.AreaId,
                AreaName = s.AreaName
            }).ToList();
            ViewBag.Area = QkSelect.List(area, "AreaId", "AreaName");
            return View(tab);
        }

        [HttpPost]
       //[QkAuthorize(Roles = "Dev,Edit Table")]
        public ActionResult Edit(long? id, TableViewModel tbl)
        {
            if (ModelState.IsValid)
            {
                var Exists = db.Tables.Any(c => c.TableName == tbl.TableName && c.TableId != id);
                if (Exists)
                {
                    Danger("Table  already exists.", false);
                    return RedirectToAction("Create", "Table");
                }
                else
                {
                    Table tab = db.Tables.Find(id);
                    tab.TableName = tbl.TableName;
                    tab.MaxSeats = tbl.MaxSeats;
                    tab.AreaId = tbl.AreaId;
                    tab.TableStatus = tbl.TableStatus;
                    tab.Description = tbl.Description;


                    db.Entry(tab).State = EntityState.Modified;
                    db.SaveChanges();
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "Table", "Tables", findip(), tab.TableId, "Table Updated Successfully");


                    Success("Successfully Updated Table details.", true);
                    return RedirectToAction("Index", "Table");
                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return (View());
            }
        }

        // GET: Desg/Delete/5
        //[QkAuthorize(Roles = "Dev,Delete Table")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Table tab = db.Tables.Find(id);
            if (tab == null)
            {
                return NotFound();
            }

            return PartialView(tab);
        }

        // POST: Field/Delete/5
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Table")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            Table tab = db.Tables.Find(id);
                db.Tables.Remove(tab);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "Table", "Tables", findip(), tab.TableId, "Table Deleted Successfully");


                stat = true;
                msg = "Successfully Deleted Table details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult AllTables()
        {
            var tables = db.Tables.Where(c => c.Status == Status.active).Select(b => new
            {
                b.TableId,
                b.TableName,
                b.MaxSeats,
                b.TableStatus,
                b.Description,
                b.AreaId,
                b.Branch,
                b.CreatedBy,
                b.CreatedDate,
                Area = db.Areas.Where(a => a.AreaId == b.AreaId).Select(a => a.AreaName).FirstOrDefault(),
                usedSeat = db.POSOrders.Where(c => c.TableId == b.TableId && c.OrderType == OrderType.DineIn && (c.OrderStatus == OrderStatus.PrintKOT || c.OrderStatus == OrderStatus.PrintBill || c.OrderStatus == OrderStatus.SaveOrder)).Select(c => c.PeopleCount).Sum() ?? 0,

            }).ToList().Select(o => new
            {
                o.TableId,
                o.TableName,
                o.MaxSeats,
                o.TableStatus,
                o.Description,
                o.AreaId,
                o.Branch,
                o.CreatedBy,
                o.CreatedDate,
                o.Area,
                o.usedSeat,
                Staus = Enum.GetName(typeof(TableStatus), o.TableStatus),
                CuStatus = ((o.TableStatus == TableStatus.Available) && (o.MaxSeats-o.usedSeat != o.MaxSeats))? Enum.GetName(typeof(TableStatus), TableStatus.InUse): Enum.GetName(typeof(TableStatus), o.TableStatus)
            }).ToList()
            .OrderBy(c => c.AreaId);

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(tables);
            return Json(tables);
        }

        public JsonResult getTableById(int TableId)
        {
            DateTime today = DateTime.Now.Date;
            var tables = db.Tables.Where(c => c.Status == Status.active && c.TableId== TableId).Select(b => new
            {
                b.TableId,
                b.TableName,
                b.MaxSeats,
                b.TableStatus,
                b.Description,
                b.AreaId,
                b.Branch,
                b.CreatedBy,
                b.CreatedDate,
                Area = db.Areas.Where(a => a.AreaId == b.AreaId).Select(a => a.AreaName).FirstOrDefault(),
                usedSeat =db.POSOrders.Where(c=>c.TableId==b.TableId && c.OrderType==OrderType.DineIn && (c.OrderStatus == OrderStatus.PrintKOT || c.OrderStatus==OrderStatus.PrintBill || c.OrderStatus == OrderStatus.SaveOrder)).Select(c=>c.PeopleCount).Sum() ?? 0,
            }).ToList().Select(o => new
            {
                o.TableId,
                o.TableName,
                o.MaxSeats,
                o.TableStatus,
                o.Description,
                o.AreaId,
                o.Branch,
                o.CreatedBy,
                o.CreatedDate,
                o.Area,
                o.usedSeat,
                Staus = Enum.GetName(typeof(TableStatus), o.TableStatus),
                CuStatus = ((o.TableStatus == TableStatus.Available) && (o.MaxSeats - o.usedSeat != o.MaxSeats)) ? Enum.GetName(typeof(TableStatus), TableStatus.InUse) : Enum.GetName(typeof(TableStatus), o.TableStatus)
            }).FirstOrDefault();
            return Json(tables);
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
