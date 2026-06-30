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
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using QuickSoft.ViewModel;

namespace QuickSoft.Controllers
{
    public class VehicleUpdatesController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public VehicleUpdatesController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        public ActionResult GetVehiclereadingsstatus( long? emp, long? vehicle, long? usage, string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            var allentry = User.IsInRole("All vehicle"); // Security S21: was hardcoded true (IDOR — leaked all rows); matches this controller's own line 206.

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
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


            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit vehicle");
            var uDelete = User.IsInRole("Delete vehicle");

            var v = (from a in db.vehicleupdations
                     join b in db.Users on a.createdby equals b.Id
                     join c in db.Employees on a.employeeid equals c.EmployeeId
                     join d in db.ProTasks on a.protaskid equals d.ProTaskId into pro
                     from d in pro.DefaultIfEmpty()
                     join e in db.Customers on a.leadid equals e.CustomerID into cust
                     from e in cust.DefaultIfEmpty()
                     join f in db.vehiclemasters on a.vehicleid equals f.vehicleid

                     where (emp == 0 || emp == null || a.employeeid == emp)
                     &&
                     (vehicle == 0 || vehicle == null || a.vehicleid == vehicle) &&
                    (usage == 0 || usage == null || a.usetype == usage)
                     &&
                     (allentry == true || c.UserId == UserId) &&
                       (fromdate == "" || (a.createddate != null && EF.Functions.DateDiffDay(a.createddate, fdate) <= 0)) &&
                  (todate == "" || (a.createddate != null && EF.Functions.DateDiffDay(a.createddate, tdate) >= 0))
                     let logs = db.LogManagers.Where(o => o.LogTable == "Vehicle" && (o.LogSection == f.VechicleName + " " + f.RegistrationNumber)).OrderByDescending(o => o.LogTime).Select(o => o.LogDetails).FirstOrDefault()
                     group new { logs, a.vehicleid, c.EmployeeId, a.createddate, a.direction, f.openingkelometer, f.VechicleName, f.RegistrationNumber, c.FirstName, c.LastName, a.readings } by new { a.vehicleid, c.EmployeeId } into grp

                     select new
                     {
                         vehileid = grp.FirstOrDefault().vehicleid,
                         vehiclename = grp.FirstOrDefault().VechicleName + " " + grp.FirstOrDefault().RegistrationNumber,
                         Employee = grp.OrderByDescending(o => o.createddate).FirstOrDefault().FirstName + " " + grp.FirstOrDefault().LastName,
                         employeeid = grp.OrderByDescending(o => o.createddate).FirstOrDefault().EmployeeId,
                         firstreading = grp.OrderBy(o => o.createddate).FirstOrDefault().readings,
                         lastreading = grp.OrderByDescending(o => o.createddate).FirstOrDefault().readings,
                         rates = grp.FirstOrDefault().openingkelometer,
                         // difference = 0,//totaldiff(emp, vehicle, usage, fdate, fromdate, todate, tdate),// grp.().readings - grp.FirstOrDefault().readings,
                         // rate =0,// (grp.LastOrDefault().readings - grp.FirstOrDefault().readings) * grp.FirstOrDefault().openingkelometer,
                         perrate = grp.FirstOrDefault().openingkelometer,
                         status = (grp.OrderByDescending(o => o.createddate).FirstOrDefault().direction == 0) ? "Stared" : "Ended",
                         lasthandoverdetails = grp.OrderByDescending(o => o.createddate).FirstOrDefault().logs

                     }).ToList().Select(o => new
                     {
                         o.Employee,

                         o.vehileid,
                         o.vehiclename,
                         o.firstreading,
                         o.lastreading,
                         o.perrate,
                         difference = totaldiff(o.employeeid, o.vehileid, usage, fdate, fromdate, todate, tdate),

                         o.status,
                         o.lasthandoverdetails,
                         o.employeeid,
                     }).ToList().Select(o => new
                     {
                         o.Employee,

                         o.vehileid,
                         o.vehiclename,
                         o.firstreading,
                         o.lastreading,

                         o.difference,
                         rate = o.difference * o.perrate,

                         o.status,
                         o.lasthandoverdetails,
                         o.employeeid,
                     }).GroupBy(o=>o.employeeid,(key, group) => new
                     

                     {
                         Employee=group.FirstOrDefault().Employee,

                         //vehileid = group.FirstOrDefault().vehileid,
                         vehiclename = String.Join(",",group.Select(o=>o.vehiclename).Distinct().ToArray()),
                         firstreading = group.FirstOrDefault().firstreading,
                         lastreading = group.FirstOrDefault().lastreading,

                         difference =String.Join(",", group.Select(o=>o.difference).Distinct().ToArray()),
                         rate = group.Sum(o=>o.rate),

                         status = group.FirstOrDefault().status,
                         lasthandoverdetails = group.FirstOrDefault().lasthandoverdetails,
                        employeeid = group.FirstOrDefault().employeeid,
                     }
                     )
                     
                     .ToList();


            //search
            //                      p.Employee.ToString().ToLower().Contains(search.ToLower())

            //SORT




            recordsTotal = v.Count();
            var data = v;
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        public decimal totaldiff(long? emp,long? vehicle,long? usage,DateTime? fdate,string fromdate,string todate,DateTime? tdate)
        {
            var allreadings = (from x in db.vehicleupdations

                               where (emp == 0 || emp == null || x.employeeid == emp)
                               &&
                               (vehicle == 0 || vehicle == null || x.vehicleid == vehicle) &&
                              (usage == 0 || usage == null || x.usetype == usage) &&

                                 (fromdate == "" || (x.createddate != null && EF.Functions.DateDiffDay(x.createddate, fdate) <= 0)) &&
                            (todate == "" || (x.createddate != null && EF.Functions.DateDiffDay(x.createddate, tdate) >= 0))
                               orderby x.createddate
                               select new
                               {
                                   x.vehicleid,
                                   x.employeeid,
                                   x.direction,
                                   x.readings,
                                   x.createddate
                               }).OrderBy(o => o.createddate).ToList();
            decimal totalkm = 0;
            decimal alltotal = 0;
            decimal diff = 0;
            foreach (var allr in allreadings)
            {
                diff = 0;
                if (allr.direction == 0)
                    totalkm = allr.readings;
                else if (allr.direction == 1 && totalkm>0)
                {
                    diff = allr.readings - totalkm;
                }
                alltotal = alltotal + diff;
            }
            return alltotal;
        }
        public ActionResult GetVehiclereadings(long? taskname,long? leadname,long? emp, long? vehicle, long? usage, string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            var allentry = User.IsInRole("All vehicle");
          
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
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


            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit vehicle");
            var uDelete = User.IsInRole("Delete vehicle");
            var serverRows = (from a in db.vehicleupdations
                     join b in db.Users on a.createdby equals b.Id
                     join c in db.Employees on a.employeeid equals c.EmployeeId
                     join d in db.ProTasks on a.protaskid equals d.ProTaskId into pro
                     from d in pro.DefaultIfEmpty()
                     join e in db.Customers on a.leadid equals e.CustomerID into cust
                     from e in cust.DefaultIfEmpty()
                     join f in db.vehiclemasters on a.vehicleid equals f.vehicleid
                   
                     where (emp == 0|| emp == null || a.employeeid==emp)
                     &&
                     (vehicle == 0 || vehicle == null || a.vehicleid == vehicle) &&
                    (usage == 0 || usage == null || a.usetype == usage)
                     &&
                     (allentry==true||c.UserId==UserId) &&
                     (taskname==0||taskname==null||a.protaskid==taskname)
                     &&
                     (leadname == 0 || leadname == null || a.leadid == leadname) &&
                       (fromdate == "" || (a.createddate != null && EF.Functions.DateDiffDay(a.createddate, fdate) <= 0)) &&
                  (todate == "" || (a.createddate != null && EF.Functions.DateDiffDay(a.createddate, tdate) >= 0))

                  orderby a.createdby descending
                     select new
                     {

                       Id=a.vehicleupdateid,
                     Employee=c.FirstName+ " "+c.LastName,

            vehiclename=f.VechicleName+ " "+f.RegistrationNumber,
              startreading=a.readings,
              endreading=(a.direction==0)?"Start Reading":"End Reading",
              usage= (a.usetype == 4) ? "Company":(a.usetype==1)?"Personal": (a.usetype == 2)?"Task,Task Name : "+d.TaskCode+ " "+d.TaskName:"Leads, Lead Name"+e.CustomerCode+ " "+e.CustomerName,
              // amount computed client-side (see below): EF Core 10 cannot translate the correlated subquery
              // that projects an anonymous { totalkm } referencing outer columns then Sum()s it.
              startvupid=a.startvupid,
              openingkelometer=f.openingkelometer,
                    CreatedDate=a.createddate,
                   a.remarks,
                    user=a.createdby,
                    a.direction,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                     }).ToList();

            // CLIENT-side: readings of the referenced start-trip row, keyed by vehicleupdateid.
            var startReadings = db.vehicleupdations
                .Select(x => new { x.vehicleupdateid, x.readings })
                .ToList()
                .ToDictionary(x => x.vehicleupdateid, x => x.readings);

            var v = serverRows.Select(o => new
                     {
                       o.Id,
                       o.Employee,
                       o.vehiclename,
                       o.startreading,
                       o.endreading,
                       o.usage,
                       amount = (decimal?)((o.direction == 1)
                                  ? (o.startreading - (o.startvupid != null && startReadings.TryGetValue(o.startvupid.Value, out var sr) ? sr : 0)) * o.openingkelometer
                                  : 0),
                       o.CreatedDate,
                       o.remarks,
                       o.user,
                       o.direction,
                       o.Dev,
                       o.Edit,
                       o.Delete,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.vehiclename.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.Employee.ToString().ToLower().Contains(search.ToLower())
                                  );

            }
            //SORT


            v = v.OrderByDescending(o => o.CreatedDate).ThenBy(o=>o.vehiclename).ThenBy(o=>o.direction);

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [HttpPost]
        public ActionResult Edit( Vehicleupdateviewmodal upt)
        {
            string msg = "";
            bool stat = false;



            var inorout = db.vehicleupdations.Where(o => o.vehicleupdateid == upt.id).FirstOrDefault();

            vehicleupdation vupt = db.vehicleupdations.Where(o => o.vehicleupdateid == upt.id).FirstOrDefault();
            var userid = User.Identity.GetUserId();
            if (inorout == null)
            {
            }
            else if (inorout.direction == 0)
            {
            }
            else if (inorout.direction == 1)
            {
            }
            vupt.createdby = userid;
            vupt.employeeid = upt.employee;
            vupt.usetype = upt.usetype;
            vupt.vehicleid = upt.vehicleid;
            vupt.remarks = upt.remarks;
            if (upt.usetype == 2)
            {
                if (upt.taskid == null || upt.taskid == 0)
                {
                    msg = "Please Select Task";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    vupt.protaskid = upt.taskid;
                }
            }
            else if (upt.usetype == 3)
            {
                if (upt.leadid == null || upt.leadid == 0)
                {
                    msg = "Please Select leads";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    vupt.leadid = upt.leadid;
                }
            }
            vupt.readings = upt.reading;

            db.Entry(vupt).State = EntityState.Modified;
            db.SaveChanges();
            long upid = vupt.vehicleupdateid;




            string path = LegacyWeb.MapPath("~/uploads/odometer/");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            IFormFileCollection files = Request.Form.Files;

            for (int i = 0; i < files.Count; i++)
            {
                IFormFile file = files[i];
                if (file.Length > 0)
                {

                    var fileCount = db.MultipleDocuments.Select(a => a.Id).AsEnumerable().DefaultIfEmpty(0).Max();

                    var fileName = Path.GetFileName(file.FileName);

                    String extension = Path.GetExtension(fileName);


                    String newName = fileCount + extension;
                    string newFName = fileCount + extension;
                    string newSName = fileCount + extension;
                    var FStatus = Status.active;

                    var thumbName = "";
                    var resizeName = "";
                    if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                    {
                        thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), thumbName);

                        resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), resizeName);
                        newFName = "resize_" + newFName;
                        FStatus = Status.inactive;
                    }
                    else
                    {
                        var commonfilename = "Docs-Thump.png";

                    }

                    newName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), newName);
                    file.SaveAs(newName);

                    var FilemultipleDocument = new FilemultipleDocuments
                    {
                        Document = newSName,
                        RelationID = upid,
                        DocumentName = "odometer",
                        CreatedBy = userid,
                        Note = "",
                        CreatedDate = System.DateTime.Now,
                        Status = Status.active,
                        ExpiryDate = System.DateTime.Now.AddYears(100)
                    };
                    db.MultipleDocuments.Add(FilemultipleDocument);
                    db.SaveChanges();


                    if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                    {
                        Image img = Image.FromFile(newName);
                        int imgHeight = 100;
                        int imgWidth = 100;
                        if (img.Width < img.Height)
                        {
                            //portrait image  
                            imgHeight = 100;
                            var imgRatio = (float)imgHeight / (float)img.Height;
                            imgWidth = Convert.ToInt32(img.Height * imgRatio);
                        }
                        else if (img.Height < img.Width)
                        {
                            //landscape image  
                            imgWidth = 100;
                            var imgRatio = (float)imgWidth / (float)img.Width;
                            imgHeight = Convert.ToInt32(img.Height * imgRatio);
                        }
                        Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                        thumb.Save(thumbName);

                        Image lgimg = Image.FromFile(newName);
                        if (lgimg.Width > 1800 || lgimg.Height > 1800)
                        {
                            Image imgs = Image.FromFile(newName);
                            System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), resizeName);
                            thumbs.Save(resizeName);
                        }
                        else
                        {
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), resizeName);
                            lgimg.Save(resizeName);
                        }

                    }
                }
            }











            msg = "Saved Success";
            stat = true;


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult Updatetransfer(long fromemployee, long toemployee, long vehicleid)
        {
            string msg = "";
            bool stat = false;
            var inorout = db.vehicleupdations.Where(o => o.vehicleid == vehicleid).OrderByDescending(o => o.createddate).FirstOrDefault();
            if (inorout != null)
            {
                if(inorout.direction==0)
                {
                    msg = "Vehicle Starting position.  Must Stop First";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                
            }
            msg = "Hand Over Success";
            stat = true;
            //let logs = db.LogManagers.Where(o => o.LogTable == "Vehicle" && (o.LogSection == f.VechicleName + " " + f.RegistrationNumber)).OrderByDescending(o => o.LogTime).Select(o => o.LogDetails).FirstOrDefault()
var vehiclename = db.vehiclemasters.Where(o => o.vehicleid == vehicleid).Select(o => o.VechicleName + " " + o.RegistrationNumber).FirstOrDefault();
var userid = User.Identity.GetUserId();
            var toemp = db.Employees.Where(o => o.EmployeeId == toemployee).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
            var fromemp = db.Employees.Where(o => o.EmployeeId == fromemployee).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();

            com.addlog(LogTypes.Created, userid, vehiclename, "Vehicle", findip(), vehicleid, "Vehicle Hand from "+fromemp +" Over to " + toemp + " on " + System.DateTime.Now.ToString());

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult Update(Vehicleupdateviewmodal upt)
        {
            string msg = "";
            bool stat = false;
            vehicleupdation vupt = new vehicleupdation();


            var inorout = db.vehicleupdations.Where(o => o.vehicleid == upt.vehicleid).OrderByDescending(o => o.createddate).FirstOrDefault();
            var readingdiff = upt.reading-inorout.readings;
            if (upt.reading == 0)
            {
                msg = "reading zero not allowed";
                stat = false;


                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            if (upt.usetype == 4 && (upt.remarks == "" || upt.remarks == null))
            {
                msg = "Company Use Must Need Proper Remarks";
                stat = false;


                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            if (inorout!=null)
            {
                if(inorout.direction==1 && upt.reading!=inorout.readings)
                {
                   
                     msg = "Last Stop Reading Not Match,contact admin";
                     stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if(inorout.direction == 0&& upt.reading<inorout.readings)
                {
                    msg = "End Reading Must Be Greater";
                        
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if (readingdiff>2000)
                {                       
                    msg = "reading diffrence greater than 2000km.please enter correct reading";

                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if(inorout.protaskid!=upt.taskid && inorout.direction==0 &&upt.usetype==2)
                {
                    msg = "Task Not Match";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if (inorout.leadid != upt.leadid && inorout.direction == 0 && upt.usetype == 3)
                {
                    msg = "Lead Not Match";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if (inorout.leadid != upt.leadid && inorout.direction == 0 && upt.usetype == 3)
                {
                    msg = "Lead Not Match";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if (inorout.usetype != upt.usetype && inorout.direction == 0 && upt.usetype == 1)
                {
                    msg = "Type Different";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if (inorout.usetype != upt.usetype && inorout.direction == 0 && upt.usetype == 4)
                {
                    msg = "Type Different";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
          
            var userid = User.Identity.GetUserId();
            if (inorout==null)
            {
                vupt.direction = 0;
            }
            else if(inorout.direction==0)
            {
                vupt.direction = 1;
                vupt.startvupid = inorout.vehicleupdateid;
            }
            else if (inorout.direction == 1)
            {
                vupt.direction = 0;
            }
            vupt.createdby = userid;
            vupt.createddate = System.DateTime.Now;
            vupt.employeeid = upt.employee;
            vupt.usetype = upt.usetype;
            vupt.vehicleid = upt.vehicleid;
            vupt.remarks = upt.remarks;
            if(upt.usetype==2)
            {
                if(upt.taskid ==null||upt.taskid==0)
                {
                    msg = "Please Select Task";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };  
                }
                else
                {
                    vupt.protaskid = upt.taskid;
                }
            }
            else if (upt.usetype == 3)
            {
                if (upt.leadid == null || upt.leadid == 0)
                {
                    msg = "Please Select leads";
                    stat = false;


                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    vupt.leadid = upt.leadid;
                }
            }
            vupt.readings = upt.reading;
            db.vehicleupdations.Add(vupt);
            db.SaveChanges();
                long upid = vupt.vehicleupdateid;




            string path = LegacyWeb.MapPath("~/uploads/odometer/");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            IFormFileCollection files = Request.Form.Files;

            for (int i = 0; i < files.Count; i++)
            {
                IFormFile file = files[i];
                if (file.Length > 0)
                {

                    var fileCount = db.MultipleDocuments.Select(a => a.Id).AsEnumerable().DefaultIfEmpty(0).Max();

                    var fileName = Path.GetFileName(file.FileName);

                    String extension = Path.GetExtension(fileName);


                    String newName = fileCount + extension;
                    string newFName = fileCount + extension;
                    string newSName = fileCount + extension;
                    var FStatus = Status.active;

                    var thumbName = "";
                    var resizeName = "";
                    if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                    {
                        thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), thumbName);

                        resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), resizeName);
                        newFName = "resize_" + newFName;
                        FStatus = Status.inactive;
                    }
                    else
                    {
                        var commonfilename = "Docs-Thump.png";

                    }
              
                    newName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), newName);
                    file.SaveAs(newName);

                    var FilemultipleDocument = new FilemultipleDocuments
                    {
                        Document = newSName,
                        RelationID = upid,
                        DocumentName = "odometer",
                        CreatedBy = userid,
                        Note = "",
                        CreatedDate = System.DateTime.Now,
                        Status = Status.active,
                        ExpiryDate = System.DateTime.Now.AddYears(100)
                    };
                    db.MultipleDocuments.Add(FilemultipleDocument);
                    db.SaveChanges();


                    if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                    {
                        Image img = Image.FromFile(newName);
                        int imgHeight = 100;
                        int imgWidth = 100;
                        if (img.Width < img.Height)
                        {
                            //portrait image  
                            imgHeight = 100;
                            var imgRatio = (float)imgHeight / (float)img.Height;
                            imgWidth = Convert.ToInt32(img.Height * imgRatio);
                        }
                        else if (img.Height < img.Width)
                        {
                            //landscape image  
                            imgWidth = 100;
                            var imgRatio = (float)imgWidth / (float)img.Width;
                            imgHeight = Convert.ToInt32(img.Height * imgRatio);
                        }
                        Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                        thumb.Save(thumbName);

                        Image lgimg = Image.FromFile(newName);
                        if (lgimg.Width > 1800 || lgimg.Height > 1800)
                        {
                            Image imgs = Image.FromFile(newName);
                            System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), resizeName);
                            thumbs.Save(resizeName);
                        }
                        else
                        {
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/odometer/"), resizeName);
                            lgimg.Save(resizeName);
                        }

                    }
                }
            }











            msg = "Saved Success";
            stat = true;


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult Edit(long id)
        {
            var usrid = User.Identity.GetUserId();
           
            DateTime lup = System.DateTime.Now.AddDays(-15);
              Vehicleupdateviewmodal vmodal = new Vehicleupdateviewmodal();
            var data = db.vehicleupdations.Find(id);
            var empuseird = db.Employees.Where(o => o.EmployeeId == data.employeeid).Select(o => o.UserId).FirstOrDefault();

            var use = (from a in db.Employees
                       join b in db.Users on a.UserId equals b.Id
                       where b.Status == 1 && b.Id == empuseird
                       select a)
                           .Select(s => new
                           {
                               ID = s.EmployeeId,
                               Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                           })
                           .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            use = (from a in db.vehiclemasters

                   select a)
                       .Select(s => new
                       {
                           ID = s.vehicleid,
                           Name = s.VechicleName + " " + s.RegistrationNumber
                       })
                            .ToList();
            ViewBag.vehicle = QkSelect.List(use, "ID", "Name");
            use = (from a in db.ProTasks
                   join b in db.TaskAssigneds on a.ProTaskId equals b.ProTaskId
                   join c in db.Employees on b.EmployeeId equals c.EmployeeId
                   where c.UserId == empuseird && b.Status == "Assigned" && b.chkStatus == Status.active
                     && a.logtime > lup
                   select a)
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskCode + " " + s.TaskName
                })
                     .ToList();
            ViewBag.protasks = QkSelect.List(use, "ID", "Name");
            use = (from a in db.Customers
                   join b in db.AssignedTos on a.CustomerID equals b.CustomerID
                   join c in db.Employees on b.EmployeeId equals c.EmployeeId
                   where c.UserId == empuseird && b.Status == "Assigned" && b.ChkStatus == (int)Status.active
                     && a.logtime > lup

                   && a.Type == CRMCustomerType.Leads

                   select a)
                  .Select(s => new
                  {
                      ID = s.CustomerID,
                      Name = s.CustomerCode + " " + s.CustomerName
                  })
                       .ToList();
            ViewBag.leads = QkSelect.List(use, "ID", "Name");



            vmodal.employee = data.employeeid;
            vmodal.vehicleid = data.vehicleid;
            vmodal.usetype = (int)data.usetype;
            vmodal.leadid = data.leadid;
            vmodal.taskid = data.protaskid;
            vmodal.reading = data.readings;
            vmodal.id = id;
            ViewBag.image = (from a in db.MultipleDocuments
                             where a.RelationID == id
                     && a.DocumentName == "odometer"
                             select new TaskImageViewModel
                             {
                                 TaskImageId = a.Id,
                                 TaskId = a.RelationID,
                                 FileName = a.Document,
                                 TaskName = a.Document,
                             }).ToList();
            return View(vmodal);
         
        }
            public ActionResult Download(long id)
        {
            var downdocs = from a in db.MultipleDocuments
                           where a.RelationID == id
                           && a.DocumentName == "odometer"
                           select a;
            foreach (var arr in downdocs)
            {
                var extension = ".jpg";
                var idofdoc = arr.Id;
                string path = AppDomain.CurrentDomain.BaseDirectory + "/uploads/odometer/";
                byte[] fileBytes = System.IO.File.ReadAllBytes(path + arr.Document);
                string fileName = idofdoc + extension;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            return View();
        }
        public string Downloadimg(long id)
        {
            var downdocs = from a in db.MultipleDocuments
                           where a.RelationID == id
                           && a.DocumentName == "odometer"
                           select a;
            foreach (var arr in downdocs)
            {
                var extension = ".jpg";
                var idofdoc = arr.Id;
                string path = AppDomain.CurrentDomain.BaseDirectory + "/uploads/odometer/";
                byte[] fileBytes = System.IO.File.ReadAllBytes(path + arr.Document);
                string fileName = idofdoc + extension;
                return "/uploads/odometer/" + arr.Document;
            }
            return "";
        }
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vehicleupdation ptype = db.vehicleupdations.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete PropertyType")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFn(id);
            msg = "Successfully Deleted ";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            var ve = db.vehicleupdations.Where(o => o.vehicleupdateid == id);
            db.vehicleupdations.RemoveRange(ve);
            db.SaveChanges();
            return true;
        }
        public ActionResult VehicleList()
        {
            var use = QkSelect.List(new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                             }, "Value", "Text");




            ViewBag.TaskName = QkSelect.List(
                  new List<SelectListItem>
                  {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                  }, "Value", "Text", 1);
            ViewBag.LeadName = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Cashier = use;// QkSelect.List(use, "Value", "Text");
            var useg = (from a in db.vehiclemasters

                        select a)
                .Select(s => new
                {
                    ID = s.vehicleid,
                    Name = s.VechicleName + " " + s.RegistrationNumber
                })
                     .ToList();
            ViewBag.vehicle = QkSelect.List(useg, "ID", "Name");
            return View();
        }

        public ActionResult Index()
        {
            var use = QkSelect.List(new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                             }, "Value", "Text");




                        ViewBag.TaskName = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.LeadName = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Cashier = use;// QkSelect.List(use, "Value", "Text");
            var useg = (from a in db.vehiclemasters

                   select a)
                .Select(s => new
                {
                    ID = s.vehicleid,
                    Name = s.VechicleName + " " + s.RegistrationNumber
                })
                     .ToList();
            ViewBag.vehicle = QkSelect.List(useg, "ID", "Name");
            return View();
        }
        public ActionResult vehiclestatus()
        {
            var use = QkSelect.List(new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                             }, "Value", "Text");




            ViewBag.TaskName = QkSelect.List(
                  new List<SelectListItem>
                  {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                  }, "Value", "Text", 1);
            ViewBag.LeadName = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Cashier = use;// QkSelect.List(use, "Value", "Text");
            var useg = (from a in db.vehiclemasters

                        select a)
                .Select(s => new
                {
                    ID = s.vehicleid,
                    Name = s.VechicleName + " " + s.RegistrationNumber
                })
                     .ToList();
            ViewBag.vehicle = QkSelect.List(useg, "ID", "Name");
            return View();
        }
        [HttpPost]
        public string getstatus(long vehicleid)
        {
            var inorout = db.vehicleupdations.Where(o => o.vehicleid == vehicleid).OrderByDescending(o => o.createddate).FirstOrDefault();
            if(inorout==null)
            {
                return "Start";
            }
            else if(inorout.direction==0)
            {
                return "Stop";
            }
            else if (inorout.direction == 1)
            {
                return "Sart";
            }
            return "";
        }
        public ActionResult entrys()
           {
            var usrid = User.Identity.GetUserId();
            var use = (from a in db.Employees
                       join b in db.Users on a.UserId equals b.Id
                       where b.Status == 1 && b.Id == usrid
                       select a)
                            .Select(s => new
                            {
                                ID = s.EmployeeId,
                                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                            })
                            .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            use = (from a in db.vehiclemasters

                   select a)
                       .Select(s => new
                       {
                           ID = s.vehicleid,
                           Name = s.VechicleName + " " + s.RegistrationNumber
                       })
                            .ToList();
            ViewBag.vehicle = QkSelect.List(use, "ID", "Name");
            DateTime lup = System.DateTime.Now.AddDays(-15);
            use = (from a in db.ProTasks
                   join b in db.TaskAssigneds on a.ProTaskId equals b.ProTaskId
                   join c in db.Employees on b.EmployeeId equals c.EmployeeId
                   where c.UserId == usrid && b.Status == "Assigned" && b.chkStatus == Status.active
                    
                   select a)
                  .Select(s => new
                  {
                      ID = s.ProTaskId,
                      Name = s.TaskCode + " " + s.TaskName
                  })
                       .ToList();
            ViewBag.protasks = QkSelect.List(use, "ID", "Name");
            use = (from a in db.Customers
                   join b in db.AssignedTos on a.CustomerID equals b.CustomerID
                   join c in db.Employees on b.EmployeeId equals c.EmployeeId
                   where c.UserId == usrid && b.Status == "Assigned" && b.ChkStatus == (int)Status.active
                   
                   
                   &&  a.Type == CRMCustomerType.Leads
                  
                   select a)
                  .Select(s => new
                  {
                      ID = s.CustomerID,
                      Name = s.CustomerCode + " " + s.CustomerName
                  })
                       .ToList();
            ViewBag.leads = QkSelect.List(use, "ID", "Name");
            Vehicleupdateviewmodal vmodal = new Vehicleupdateviewmodal();


            return View(vmodal);
        }
        public ActionResult create()
        {
            var usrid = User.Identity.GetUserId();
            var use = (from a in db.Employees
                       join b in db.Users on a.UserId equals b.Id
                       where b.Status == 1 && b.Id == usrid
                       select a)
                            .Select(s => new
                            {
                                ID = s.EmployeeId,
                                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                            })
                            .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            use = (from a in db.vehiclemasters

                   select a)
                       .Select(s => new
                       {
                           ID = s.vehicleid,
                           Name = s.VechicleName + " " + s.RegistrationNumber
                       })
                            .ToList();
            ViewBag.vehicle = QkSelect.List(use, "ID", "Name");
            DateTime lup = System.DateTime.Now.AddDays(-15);
            use = (from a in db.ProTasks
                   join b in db.TaskAssigneds on a.ProTaskId equals b.ProTaskId
                   join c in db.Employees on b.EmployeeId equals c.EmployeeId
                   where c.UserId == usrid && b.Status == "Assigned" && b.chkStatus == Status.active

                   select a)
                  .Select(s => new
                  {
                      ID = s.ProTaskId,
                      Name = s.TaskCode + " " + s.TaskName
                  })
                       .ToList();
            ViewBag.protasks = QkSelect.List(use, "ID", "Name");
            use = (from a in db.Customers
                   join b in db.AssignedTos on a.CustomerID equals b.CustomerID
                   join c in db.Employees on b.EmployeeId equals c.EmployeeId
                   where c.UserId == usrid && b.Status == "Assigned" && b.ChkStatus == (int)Status.active


                   && a.Type == CRMCustomerType.Leads

                   select a)
                  .Select(s => new
                  {
                      ID = s.CustomerID,
                      Name = s.CustomerCode + " " + s.CustomerName
                  })
                       .ToList();
            ViewBag.leads = QkSelect.List(use, "ID", "Name");
            Vehicleupdateviewmodal vmodal = new Vehicleupdateviewmodal();


            return View(vmodal);
        }

    }
}
