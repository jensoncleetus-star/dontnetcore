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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.IO;
using System.Data;
using System.Drawing;
using System.Net;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class AMCController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        string rootdomain;
        public AMCController()
        {
            db = new ApplicationDbContext();
            com = new Common();
            rootdomain = db.companys.Find(1L)?.CPWebsite;
        }
        public string isvalidamc(long CustId)
        {
            DateTime dt = DateTime.Now.Date;
            var v = db.Amcs.Any(o => o.StartDate <= dt && o.EndDate >= dt && o.CustomerId==CustId);
            var emp = (from a in db.Customers
                       join c in db.Employees on a.SalesPerson equals c.EmployeeId
                       where a.CustomerID == CustId
                       select new
                       {
                           salesman = c.FirstName + " " + c.MiddleName
                       }).FirstOrDefault();
            if(v)
            {
                return "<span style='color:green;font-wight:bold'>Valid Amc</span><br> Sales Man : "+emp.salesman;
            }
            else
            {
                return "<span style='color:red;font-wight:bold'>In Valid Amc</span><br> Sales Man : "+emp.salesman;
            }
        }
        public ActionResult checkamc()
        {
            var OpAll = QkSelect.List(
                                   new List<SelectListItem>
                                   {
                                    new SelectListItem { Selected = true, Text = "--SELECT--", Value = "0"},
                                   }, "Value", "Text", 1);

            ViewBag.Cust = OpAll;
            return View();
        }
        public ActionResult AddamcImage(long? id)
        {
            amcdocattach tsk = new amcdocattach();
            tsk.amcid = (long)id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var docdownload = db.AmcDocuments.Where(a => a.TransId == id).ToList();
            if (1==2)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.AmcDocuments
                                           join b in db.DocumentTypes on m.DocumentTypeID equals b.ID into temp1
                                           from b in temp1.DefaultIfEmpty()
                                           where id == m.TransId
                                           orderby m.TransType
                                           select new Multiviewmodel
                                           {
                                               documentid=(from aa in db.AmcDocuments where 
                                                            aa.TransId==m.TransId && aa.FileName==m.FileName
                                                           select new
                                                           {
                                                               aa.DocumentId
                                                           }).Max(o=>o.DocumentId),
                                             Id = m.TransId,
                                               Document = m.FileName,
                                               filenamelead = m.FileName,
                                               DocumentName = (b.Name == null || b.Name == "") ? m.Notes : b.Name,
                                               Documentview = m.TransType,

                                           }
                                        ).Distinct().ToList();
                tsk.filedoc = filedoc;


            }
           
            return PartialView(tsk);
        }
     public ActionResult imageaddnew()
        {
            if (1 == 1)
            {
                var Userid = User.Identity.GetUserId();
                var max = from a in db.EmpAttendances
                          where a.EmployeeName == Userid
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
                       where a.EmployeeName == Useriddd
                       orderby a.Id descending
                       select a.login;
            var durb = from a in db.EmpAttendances
                       where a.EmployeeName == Useriddd
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
            

            return View();
        }
        [HttpPost]
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit ProTask")]
        public ActionResult ImageAdd(long id)
        {
            bool stat = false;
            string msg;
            string description = Request.Form[Request.Form.Keys.ElementAt(0)];
            if (1 == 1)
            {
                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);

                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {

                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {
                            var resizeName = "";

                            string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);

                            var uploadUrl = LegacyWeb.MapPath("~/uploads/AmcDocuments/");

                            if (!System.IO.Directory.Exists(uploadUrl))
                                System.IO.Directory.CreateDirectory(uploadUrl);
                            file.SaveAs(Path.Combine(uploadUrl, fileName));

                            //To remove the previously attached file from folder



                            AmcDocument Documents = new AmcDocument
                            {
                                TransId = id,
                                //DocumentTypeID = item.DocumentTypeID,
                                //Expiry = item.Expiry,
                                Notes = description,
                                FileName = fileName,
                                Status = Status.active,
                                CreatedDate = System.DateTime.Now
                            };
                            db.AmcDocuments.Add(Documents);
                            db.SaveChanges();



                        }
                    }
                }


            }
            
            msg = "Successfully added Task image.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
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
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit ProTask")]
        public ActionResult imageaddnew(string login, string logout, string dura, string fromapp)
        {
            bool stat = false;
            string msg;
           
            if (1 == 1)
            {
                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);

                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {

                    for (int i = 0; i < 1; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {
                            var resizeName = "";

                            string fileName = "temp" + System.IO.Path.GetExtension(file.FileName);

                            var uploadUrl = LegacyWeb.MapPath("~/uploads/AmcDocuments/");

                            if (!System.IO.Directory.Exists(uploadUrl))
                                System.IO.Directory.CreateDirectory(uploadUrl);
                            if (System.IO.File.Exists(Path.Combine(uploadUrl, fileName)))
                                System.IO.File.Delete(Path.Combine(uploadUrl, fileName));
                            file.SaveAs(Path.Combine(uploadUrl, fileName));

                            var directories = ImageMetadataReader.ReadMetadata(Path.Combine(uploadUrl, fileName));

                            var gps = directories.OfType<GpsDirectory>().FirstOrDefault();

                            var location = gps?.GetGeoLocation();

                            if (location != null)
                            {
                                // Read and Write:
                                String format = "dd-MM-yyyy";

                                CultureInfo culture = CultureInfo.InvariantCulture;
                                // use the uploaded file's timestamp as the photo date.
                                DateTime t = System.IO.File.GetLastWriteTime(Path.Combine(uploadUrl, fileName));
                                if (t < System.DateTime.Now.AddMonths(-1))//t < System.DateTime.Now.AddMinutes(2)
                                {
                                    Danger("Invalid");
                                  }
                                else
                                {

                                    var empid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                                    var lattandlong = db.geowalls.Where(o => o.EmployeeId == empid).Select(o => new
                                    {
                                        o.lat,
                                        o.log,
                                        o.distance
                                    }).ToList();


                                    var flag = 1;
                                    foreach (var orl in lattandlong)
                                    {
                                        var orgdist = distance(Convert.ToDouble(location.Latitude), Convert.ToDouble(location.Longitude), Convert.ToDouble(orl.lat), Convert.ToDouble(orl.log), 'K');

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
                                    if (flag == 1)
                                    {
                                        Danger("You Are  Away From Point,if you sure your postion. app kill (swaipt and kill) and open again to take your corrent location", true);
                                        ViewBag.laststatus = "Expired";
                                        ViewBag.duration = "0";
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
                                        var lat = location.Latitude.ToString();
                                         var log = location.Longitude.ToString();
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


                                             today = Convert.ToDateTime(System.DateTime.Now);
                                            long dailyattid = 0;
                                            today = today.AddDays(0);
                                            var date = today.Date;
                                            var date2 = today.Date.AddDays(1);








                                        }





                                    }
                                    if (ModelState.IsValid)
                                    {
                                        com.addlog(LogTypes.Updated, UserId, "gpstrack", location.Latitude.ToString(), findip(), 1, location.Longitude.ToString());
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

                                }
                            }
                            else
                            {
                                Danger("image not gio tagged");
                         
                            }
                        }
                    }
                }
else
                {
                    Danger("geo maped file not uploaded");
           
                }

            }
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

        public ActionResult AMCreport()
        {
            
            var ref1 = db.DocumentTypes
             .Select(s => new
             {
                 ID = s.ID,
                 Name = s.Name
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.documenttypes = QkSelect.List(ref1, "ID", "Name");

            
            return View();


        }
        // GET: AMC
        public ActionResult Index()
        {
            ViewBag.TStat = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            //For Dropdown Asset Account
            AMCViewModel vmodel = new AMCViewModel();
            ViewBag.AssignedTo = QkSelect.List(
                            new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                            }, "Value", "Text", 1);
            ViewBag.Contracts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "AMC" && a.Status == Status.active).ToList();

            var ref1 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Amcs
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

        
            return View(vmodel);

            
        }
        public ActionResult DownloadDoc(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var docdownload = db.AmcDocuments.Where(a=>a.TransId==id).ToList();
            if (docdownload.Count==0)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.AmcDocuments
                                           join b in db.DocumentTypes on m.DocumentTypeID equals b.ID into temp1
                                           from b in temp1.DefaultIfEmpty()
                                           where id == m.TransId 
                                           //orderby m.TransType 
                                           select new Multiviewmodel
                                           {

                                               Id = m.TransId,
                                               Document = m.FileName,
                                               filenamelead = m.FileName,
                                               DocumentName =( b.Name==null||b.Name=="")?m.Notes:b.Name,
                                                Documentview=m.TransType,

                                           }
                                        ).ToList();
               

                return PartialView(filedoc);
            }
        }
        public class ass
        {
            public long id { get; set; }
            public string LastName { get; set; }
            public string FirstName{ get; set; }
        public string MiddleName { get; set; }
        public string Img { get; set; }
        public int Status { get; set; }
    }
        public class amcindex
        {

            public long docdownload { get; set; }
            public long AmcId { get; set; }
            public long AmcNo { get; set; }
            public int? OpenClose { get; set; }
            public long? ContractLevelId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime? ReminderDate { get; set; }
            public long? CustomerId { get; set; }
            public string CustomerName { get; set; }
            public string ContractName { get; set; }
            public string ContractType { get; set; }
            public string Location { get; set; }
            public string StatusName { get; set; }
            public List<ass> AssignedTo { get; set; }

public DateTime? nexttime { get; set; }
public DateTime? ldate { get; set; }
        }
        //AMC List ---- GET
        [RedirectingAction]
        [HttpPost]
        public ActionResult GetDetails(long? ContractLevelId,string nextdate, long? AssignedTo, long? taskstat, long? ContractNo, long? Customer, long? ContractName, long? ContractType, long? contractstatus, string RFromDate, string RToDate, string StartDate, string EFromDate, string EndDate, string EndDateto, string opncls, long? location, string ref1, string ref2, string ref3, string ref4, string ref5)
        {
            var userid = User.Identity.GetUserId();
            // The AssignedTo dropdown's "All" option sets val(0) (see Index.cshtml), so an unset filter arrives as 0,
            // not null. Treat 0 as "no assignee filter" (employee id 0 never exists) so the default list isn't empty.
            if (AssignedTo == 0) AssignedTo = null;
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tdateto = null;
            DateTime? rfdate = null;
            DateTime? rtdate = null;
            DateTime? etdate = null;
            DateTime? rnextdate = null;
            if (StartDate != "")
            {
                fdate = DateTime.Parse(StartDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (nextdate != "")
            {
                rnextdate = DateTime.Parse(nextdate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (EFromDate != "")
            {
                etdate = DateTime.Parse(EFromDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            if (EndDate != "")
            {
                tdate = DateTime.Parse(EndDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (EndDateto != "")
            {
                tdateto = DateTime.Parse(EndDateto, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (RFromDate != "")
            {
                rfdate = DateTime.Parse(RFromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (RToDate != "")
            {
                rtdate = DateTime.Parse(RToDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            int OandC = 0;
            if (opncls != "")
            {
                OandC = Convert.ToInt32(opncls);
            }
            else
            {
                OandC = 0;
                opncls = "not blacnk";
            }
            var xx = (from z in db.AmcAssignedTos
                      join y in db.Employees on z.EmployeeId equals y.EmployeeId
                      where z.Status == "Assigned" && z.ChkStatus == Status.active
                      && y.EmployeeId == 1
                      select new
                      {
                          z.AmcId
                      }).Select(o => o.AmcId).Distinct().ToList().ToArray();
            if (AssignedTo != null)
            {
                xx = (from z in db.AmcAssignedTos
                      join y in db.Employees on z.EmployeeId equals y.EmployeeId
                      where z.Status == "Assigned" && z.ChkStatus == Status.active
                   && y.EmployeeId == AssignedTo
                      select new
                      {
                          z.AmcId
                      }).Select(o => o.AmcId).Distinct().ToList().ToArray();

            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            DateTime? today = System.DateTime.Now;
            var assto = (from z in db.AmcAssignedTos
                         join y in db.Employees on z.EmployeeId equals y.EmployeeId
                         where z.AmcId == 1 && z.Status == "Assigned" && z.ChkStatus == Status.active
                         select new ass
                         {
                             id = y.EmployeeId,
                             LastName = (y.LastName != null) ? y.LastName : "",
                             FirstName = (y.FirstName != null) ? y.FirstName : "",
                             MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                             Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                             Status = y.Status
                         }).Distinct().ToList();
            if (nextdate == "" && AssignedTo == null)
            {
                var serverQuery = (from a in db.Amcs
                         join b in db.Customers on a.CustomerId equals b.CustomerID into temp1
                         from b in temp1.DefaultIfEmpty()

                         join c in db.AmcContracts on a.ContractId equals c.ContractId into temp2
                         from c in temp2.DefaultIfEmpty()
                         join d in db.AmcContractTypes on a.ContractTypeId equals d.TypeId into temp3
                         from d in temp3.DefaultIfEmpty()
                         join e in db.AmcStatuss on a.AmcStatusId equals e.AmcStatusId into temp4
                         from e in temp4.DefaultIfEmpty()
                         join f in db.LocationNames on a.LocationId equals f.LocationId into temp5
                         from f in temp5.DefaultIfEmpty()

                             // AmcUp (latest AmcUpdation per Amc) join removed — EF Core 10 cannot translate the
                             // GroupBy-latest-per-group join; it is computed client-side after materialize (see below).
                         where
                         (ContractNo == 0 || ContractNo == null || a.AmcNo == ContractNo) &&
                         (Customer == 0 || Customer == null || a.CustomerId == Customer) &&
                          (ContractLevelId == 0 || ContractLevelId == null || a.ContractLevelId == (ContractLevelId-1)) &&

                         (ContractName == 0 || ContractName == null || a.ContractId == ContractName) &&
                         (ContractType == 0 || ContractType == null || a.ContractTypeId == ContractType) &&
                         (contractstatus == 0 || contractstatus == null || a.AmcStatusId == contractstatus) &&

                         (StartDate == "" || StartDate == null || EF.Functions.DateDiffDay(a.StartDate, fdate) <= 0) &&
                          (taskstat == 0 || taskstat == null || a.AmcStatusId == taskstat) &&
                          (EFromDate == "" || EFromDate == null || EF.Functions.DateDiffDay(a.EndDate, etdate) <= 0) &&
                         (EndDate == "" || EndDate == null || EF.Functions.DateDiffDay(a.EndDate, tdate) <= 0) &&
                         (EndDateto == "" || EndDateto == null || EF.Functions.DateDiffDay(a.EndDate, tdateto) >= 0) &&

                         (RFromDate == "" || RFromDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rfdate) <= 0) &&
                         (RToDate == "" || RToDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rtdate) >= 0) &&
                         (opncls == "" || opncls == "not blacnk" || OandC == null || a.OpenClose == OandC) &&
                         (AssignedTo == null || xx.Contains(a.AmcId)) &&
                         (location == null || location == 0 || a.LocationId == location)&&
                         (ref1 == "" || ref1 == null || a.Ref1 == ref1) &&
                                               (ref2 == "" || ref2 == null || a.Ref2 == ref2) &&
                                               (ref3 == "" || ref3 == null || a.Ref3 == ref3) &&
                                               (ref4 == "" || ref4 == null || a.Ref4 == ref4) &&
                                               (ref5 == "" || ref5 == null || a.Ref5 == ref5) 

                         select new
                         {
                             docdownload = 1,
                             AmcId = a.AmcId,
                             AmcNo = a.AmcNo,
                             a.OpenClose,
                             a.ContractLevelId,
                             StartDate = a.StartDate,
                             EndDate = a.EndDate,
                             ReminderDate = a.ReminderDate,
                             CustomerId = a.CustomerId,
                             CustomerName = (b.CustomerName == null) ? "No Name" : b.CustomerName,
                             ContractName = c.ContractName,
                             ContractType = d.Type,
                             Location = f.Location,
                             StatusName = e.StatusName,

                             LogTime = a.LogTime,

                         });

            // Performance (audit P2): server-side paging on the common path; search/computed sorts fall back unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "AmcId","AmcNo","ContractLevelId","ContractName","ContractType","CustomerId","CustomerName","docdownload","EndDate","Location","LogTime","OpenClose","ReminderDate","StartDate","StatusName" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("AmcId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

                // Latest AmcUpdation per Amc, computed client-side (EF Core 10 can't translate the GroupBy-latest join).
                var amcIdList = serverRows.Select(o => o.AmcId).ToList();
                var amcUpLookup = db.AmcUpdations
                    .Where(u => u.TransType == "Amc" && amcIdList.Contains(u.TransId))
                    .ToList()
                    .GroupBy(u => u.TransId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.CreatedDate).First());
                var v = serverRows.Select(o =>
                {
                    DateTime? ldateVal = o.LogTime;
                    if (amcUpLookup.TryGetValue(o.AmcId, out var up) && up.CreatedDate > o.LogTime)
                        ldateVal = up.CreatedDate;
                    return new amcindex
                    {
                        AmcId = o.AmcId,
                        AmcNo = o.AmcNo,
                        OpenClose = o.OpenClose,
                        ContractLevelId = o.ContractLevelId,
                        StartDate = o.StartDate,
                        EndDate = o.EndDate,
                        ReminderDate = o.ReminderDate,
                        CustomerId = o.CustomerId,
                        CustomerName = o.CustomerName,
                        ContractName = o.ContractName,
                        ContractType = o.ContractType,
                        Location = o.Location,
                        StatusName = o.StatusName,
                        ldate = ldateVal,
                    };
                }).OrderByDescending(o => o.ldate).ToList();
                //SEARCH
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    v = v.Where(p => p.AmcNo.ToString().ToLower().Contains(search.ToLower()) ||
                                    p.CustomerName.ToString().ToLower().Contains(search.ToLower()) ||
                                     p.ContractName.ToString().ToLower().Contains(search.ToLower())
                                ).ToList();

                }


                if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                if (!fastPage) { recordsTotal = v.Count(); }
                var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
                for (int i = 0; i < data.Count(); i++)
                {
                    var amcid = data[i].AmcId;
                    data[i].nexttime = db.AddedRemarks.Where(c => c.TransactionId == amcid && c.TransactionType == "amcremarks" && c.AddedUser == userid).OrderByDescending(c => c.CreatedDate).Select(o => o.nexttime).FirstOrDefault();
                    var sss = (from z in db.AmcAssignedTos
                               join y in db.Employees on z.EmployeeId equals y.EmployeeId
                               where z.AmcId == amcid && z.Status == "Assigned" && z.ChkStatus == Status.active
                               select new ass
                               {
                                   id = y.EmployeeId,
                                   LastName = (y.LastName != null) ? y.LastName : "",
                                   FirstName = (y.FirstName != null) ? y.FirstName : "",
                                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                   Status = y.Status
                               }).Distinct().ToList();
                    data[i].AssignedTo = sss;
                    data[i].docdownload = db.AmcDocuments.Where(aa => aa.TransId == amcid).Count();


                }
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });


            }
            else
            {

                var serverRows2 = (from a in db.Amcs
                         join b in db.Customers on a.CustomerId equals b.CustomerID into temp1
                         from b in temp1.DefaultIfEmpty()

                         join c in db.AmcContracts on a.ContractId equals c.ContractId into temp2
                         from c in temp2.DefaultIfEmpty()
                         join d in db.AmcContractTypes on a.ContractTypeId equals d.TypeId into temp3
                         from d in temp3.DefaultIfEmpty()
                         join e in db.AmcStatuss on a.AmcStatusId equals e.AmcStatusId into temp4
                         from e in temp4.DefaultIfEmpty()
                         join f in db.LocationNames on a.LocationId equals f.LocationId into temp5
                         from f in temp5.DefaultIfEmpty()
                             // AmcUp (latest update), nexttime (let-subquery) and AssignedTo (nested collection) are all
                             // computed client-side below — EF Core 10 can't translate any of the three inside this query.
                         where
                         (ContractNo == 0 || ContractNo == null || a.AmcNo == ContractNo) &&
                         (Customer == 0 || Customer == null || a.CustomerId == Customer) &&

                         (ContractName == 0 || ContractName == null || a.ContractId == ContractName) &&
                         (ContractType == 0 || ContractType == null || a.ContractTypeId == ContractType) &&
                         (contractstatus == 0 || contractstatus == null || a.AmcStatusId == contractstatus) &&

                         (StartDate == "" || StartDate == null || EF.Functions.DateDiffDay(a.StartDate, fdate) <= 0) &&
                          (taskstat == 0 || taskstat == null || a.AmcStatusId == taskstat) &&
                          (EFromDate == "" || EFromDate == null || EF.Functions.DateDiffDay(a.EndDate, etdate) <= 0) &&
                         (EndDate == "" || EndDate == null || EF.Functions.DateDiffDay(a.EndDate, tdate) <= 0) &&
                         (EndDateto == "" || EndDateto == null || EF.Functions.DateDiffDay(a.EndDate, tdateto) >= 0) &&

                         (RFromDate == "" || RFromDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rfdate) <= 0) &&
                         (RToDate == "" || RToDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rtdate) >= 0) &&
                         (opncls == "" || opncls == "not blacnk" || OandC == null || a.OpenClose == OandC) &&
                         (AssignedTo == null || xx.Contains(a.AmcId)) &&
                         (location == null || location == 0 || a.LocationId == location)


                         select new
                         {
                             AmcId = a.AmcId,
                             AmcNo = a.AmcNo,
                             a.OpenClose,
                             a.ContractLevelId,
                             StartDate = a.StartDate,
                             EndDate = a.EndDate,
                             ReminderDate = a.ReminderDate,
                             CustomerId = a.CustomerId,
                             CustomerName = (b.CustomerName == null) ? "No Name" : b.CustomerName,
                             ContractName = c.ContractName,
                             ContractType = d.Type,
                             Location = f.Location,
                             StatusName = e.StatusName,
                             LogTime = a.LogTime,
                         }).ToList();

                // Client-side enrichment (the three untranslatable members), keyed by AmcId.
                var amcIdList2 = serverRows2.Select(o => o.AmcId).ToList();
                var amcUpLookup2 = db.AmcUpdations
                    .Where(u => u.TransType == "Amc" && amcIdList2.Contains(u.TransId))
                    .ToList()
                    .GroupBy(u => u.TransId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.CreatedDate).First());
                var amcAssignLookup = (from z in db.AmcAssignedTos
                                       join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                       where z.Status == "Assigned" && z.ChkStatus == Status.active && amcIdList2.Contains(z.AmcId)
                                       select new { z.AmcId, y.EmployeeId, y.LastName, y.FirstName, y.MiddleName, y.ImgFileName, y.Status })
                                      .ToList()
                                      .GroupBy(x => x.AmcId)
                                      .ToDictionary(g => g.Key, g => g.Select(y => new ass
                                      {
                                          id = y.EmployeeId,
                                          LastName = (y.LastName != null) ? y.LastName : "",
                                          FirstName = (y.FirstName != null) ? y.FirstName : "",
                                          MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                          Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                          Status = y.Status
                                      }).Distinct().ToList());
                var amcRemLookup = db.AddedRemarks
                    .Where(c => c.TransactionType == "amcremarks" && c.AddedUser == userid && amcIdList2.Contains(c.TransactionId))
                    .ToList()
                    .GroupBy(c => c.TransactionId)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.CreatedDate).Select(o => o.nexttime).FirstOrDefault());
                var amcDocLookup = db.AmcDocuments
                    .Where(aa => amcIdList2.Contains(aa.TransId))
                    .ToList()
                    .GroupBy(aa => aa.TransId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var v = serverRows2.Select(o =>
                {
                    DateTime? ldateVal = o.LogTime;
                    if (amcUpLookup2.TryGetValue(o.AmcId, out var up) && up.CreatedDate > o.LogTime)
                        ldateVal = up.CreatedDate;
                    return new
                    {
                        docdownload = amcDocLookup.TryGetValue(o.AmcId, out var dc) ? dc : 0,
                        AmcId = o.AmcId,
                        AmcNo = o.AmcNo,
                        o.OpenClose,
                        o.ContractLevelId,
                        StartDate = o.StartDate,
                        EndDate = o.EndDate,
                        ReminderDate = o.ReminderDate,
                        CustomerId = o.CustomerId,
                        CustomerName = o.CustomerName,
                        ContractName = o.ContractName,
                        ContractType = o.ContractType,
                        Location = o.Location,
                        StatusName = o.StatusName,
                        AssignedTo = amcAssignLookup.TryGetValue(o.AmcId, out var asl) ? asl : new List<ass>(),
                        nexttime = amcRemLookup.TryGetValue(o.AmcId, out var nt) ? nt : (DateTime?)null,
                        ldate = ldateVal,
                    };
                }).ToList();

                if (AssignedTo != null)
                {
                    v = v.Where(x => x.AssignedTo.Select(z => z.id).ToList().Contains((long)AssignedTo)).ToList();
                }
                if (nextdate != "")
                {

                    DateTime nextdates = DateTime.Parse(nextdate, new CultureInfo("en-GB"));
                    DateTime nextdatesadd = nextdates.AddDays(1);
                    v = v.Where(o => o.nexttime >= nextdates && o.nexttime < nextdatesadd).ToList();
                }

                //SEARCH
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    v = v.Where(p => p.AmcNo.ToString().ToLower().Contains(search.ToLower()) ||
                                    p.CustomerName.ToString().ToLower().Contains(search.ToLower()) ||
                                     p.ContractName.ToString().ToLower().Contains(search.ToLower())
                                ).ToList();

                }


                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
                }

                recordsTotal = v.Count();
                var data = v.Skip(skip).Take(pageSize).ToList();
         
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
        }

        [RedirectingAction]
        [HttpPost]
        public ActionResult GetmyDetails(long? ContractLevelId,string nextdate, long? AssignedTo, long? taskstat, long? ContractNo, long? Customer, long? ContractName, long? ContractType, long? contractstatus, string RFromDate, string RToDate, string StartDate, string EFromDate, string EndDate, string EndDateto, string opncls,long? location)
        {
            // The AssignedTo dropdown's "All" option sets val(0) (see Index.cshtml), so an unset filter arrives as 0,
            // not null. Treat 0 as "no assignee filter" (employee id 0 never exists) so the default list isn't empty.
            if (AssignedTo == 0) AssignedTo = null;
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tdateto = null;
            DateTime? rfdate = null;
            DateTime? rtdate = null;
            DateTime? etdate = null;
            DateTime? rnextdate = null;
            if (StartDate != "")
            {
                fdate = DateTime.Parse(StartDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (nextdate != "")
            {
                rnextdate = DateTime.Parse(nextdate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (EFromDate != "")
            {
                etdate = DateTime.Parse(EFromDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            if (EndDate != "")
            {
                tdate = DateTime.Parse(EndDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (EndDateto != "")
            {
                tdateto = DateTime.Parse(EndDateto, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (RFromDate != "")
            {
                rfdate = DateTime.Parse(RFromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (RToDate != "")
            {
                rtdate = DateTime.Parse(RToDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            int OandC = 0;
            if (opncls != "")
            {
                OandC = Convert.ToInt32(opncls);
            }
            else
            {
                OandC = 0;
                opncls = "not blacnk";
            }
            var xx = (from z in db.AmcAssignedTos
                      join y in db.Employees on z.EmployeeId equals y.EmployeeId
                      where z.Status == "Assigned" && z.ChkStatus == Status.active
                      && y.EmployeeId == 1
                      select new
                      {
                          z.AmcId
                      }).Select(o => o.AmcId).Distinct().ToList().ToArray();
            if (AssignedTo != null)
            {
                xx = (from z in db.AmcAssignedTos
                      join y in db.Employees on z.EmployeeId equals y.EmployeeId
                      where z.Status == "Assigned" && z.ChkStatus == Status.active
                   && y.EmployeeId == AssignedTo
                      select new
                      {
                          z.AmcId
                      }).Select(o => o.AmcId).Distinct().ToList().ToArray();

            }
            var UserId = User.Identity.GetUserId();

            var EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var userid = User.Identity.GetUserId();
            // AmcUp (latest update), nexttime (let-subquery) and AssignedTo (nested collection) are all computed
            // client-side below — EF Core 10 can't translate any of the three inside this query. The "My" filter
            // (assign.Contains(EmpId)) is preserved server-side as a translatable EXISTS subquery.
            var serverRows = (from a in db.Amcs
                     join b in db.Customers on a.CustomerId equals b.CustomerID into temp1
                     from b in temp1.DefaultIfEmpty()

                     join c in db.AmcContracts on a.ContractId equals c.ContractId into temp2
                     from c in temp2.DefaultIfEmpty()
                     join d in db.AmcContractTypes on a.ContractTypeId equals d.TypeId into temp3
                     from d in temp3.DefaultIfEmpty()
                     join e in db.AmcStatuss on a.AmcStatusId equals e.AmcStatusId into temp4
                     from e in temp4.DefaultIfEmpty()
                     join f in db.LocationNames on a.LocationId equals f.LocationId into temp5
                     from f in temp5.DefaultIfEmpty()

                     where
                     (ContractNo == 0 || ContractNo == null || a.AmcNo == ContractNo) &&
                     (Customer == 0 || Customer == null || a.CustomerId == Customer) &&
                      (ContractLevelId == 0 || ContractLevelId == null || a.ContractLevelId == (ContractLevelId - 1)) &&

                     (ContractName == 0 || ContractName == null || a.ContractId == ContractName) &&
                     (ContractType == 0 || ContractType == null || a.ContractTypeId == ContractType) &&
                     (contractstatus == 0 || contractstatus == null || a.AmcStatusId == contractstatus) &&

                     (StartDate == "" || StartDate == null || EF.Functions.DateDiffDay(a.StartDate, fdate) <= 0) &&
                      (taskstat == 0 || taskstat == null || a.AmcStatusId == taskstat) &&
                      (EFromDate == "" || EFromDate == null || EF.Functions.DateDiffDay(a.EndDate, etdate) <= 0) &&
                     (EndDate == "" || EndDate == null || EF.Functions.DateDiffDay(a.EndDate, tdate) <= 0) &&
                     (EndDateto == "" || EndDateto == null || EF.Functions.DateDiffDay(a.EndDate, tdateto) >= 0) &&

                     (RFromDate == "" || RFromDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rfdate) <= 0) &&
                     (RToDate == "" || RToDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rtdate) >= 0) &&
                     (opncls == "" || opncls == "not blacnk" || OandC == null || a.OpenClose == OandC) &&
                     (AssignedTo == null || xx.Contains(a.AmcId))&&
                        db.AmcAssignedTos.Any(x => x.AmcId == a.AmcId && x.Status == "Assigned" && x.ChkStatus == Status.active && x.EmployeeId == EmpId) &&
                        (location==0||location==null||a.LocationId==location)

                     select new
                     {
                         AmcId = a.AmcId,
                         AmcNo = a.AmcNo,
                         a.OpenClose,
                         a.ContractLevelId,
                         StartDate = a.StartDate,
                         EndDate = a.EndDate,
                         ReminderDate = a.ReminderDate,
                         CustomerId = a.CustomerId,
                         CustomerName = (b.CustomerName == null) ? "No Name" : b.CustomerName,
                         ContractName = c.ContractName,
                         ContractType = d.Type,
                         Location = f.Location,
                         StatusName = e.StatusName,
                         LogTime = a.LogTime,
                     }).ToList();

            // Client-side enrichment (the three untranslatable members), keyed by AmcId.
            var amcIdList = serverRows.Select(o => o.AmcId).ToList();
            var amcUpLookup = db.AmcUpdations
                .Where(u => u.TransType == "Amc" && amcIdList.Contains(u.TransId))
                .ToList()
                .GroupBy(u => u.TransId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.CreatedDate).First());
            var amcAssignLookup = (from z in db.AmcAssignedTos
                                   join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                   where z.Status == "Assigned" && z.ChkStatus == Status.active && amcIdList.Contains(z.AmcId)
                                   select new { z.AmcId, y.EmployeeId, y.LastName, y.FirstName, y.MiddleName, y.ImgFileName, y.Status })
                                  .ToList()
                                  .GroupBy(x => x.AmcId)
                                  .ToDictionary(g => g.Key, g => g.Select(y => new ass
                                  {
                                      id = y.EmployeeId,
                                      LastName = (y.LastName != null) ? y.LastName : "",
                                      FirstName = (y.FirstName != null) ? y.FirstName : "",
                                      MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                      Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                      Status = y.Status
                                  }).Distinct().ToList());
            var amcRemLookup = db.AddedRemarks
                .Where(c => c.TransactionType == "amcremarks" && c.AddedUser == userid && amcIdList.Contains(c.TransactionId))
                .ToList()
                .GroupBy(c => c.TransactionId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(c => c.CreatedDate).Select(o => o.nexttime).FirstOrDefault());
            var amcDocLookup = db.AmcDocuments
                .Where(aa => amcIdList.Contains(aa.TransId))
                .ToList()
                .GroupBy(aa => aa.TransId)
                .ToDictionary(g => g.Key, g => g.Count());

            var v = serverRows.Select(o =>
            {
                DateTime? ldateVal = o.LogTime;
                if (amcUpLookup.TryGetValue(o.AmcId, out var up) && up.CreatedDate > o.LogTime)
                    ldateVal = up.CreatedDate;
                return new
                {
                    docdownload = amcDocLookup.TryGetValue(o.AmcId, out var dc) ? dc : 0,
                    AmcId = o.AmcId,
                    AmcNo = o.AmcNo,
                    o.OpenClose,
                    o.ContractLevelId,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    ReminderDate = o.ReminderDate,
                    CustomerId = o.CustomerId,
                    CustomerName = o.CustomerName,
                    ContractName = o.ContractName,
                    ContractType = o.ContractType,
                    Location = o.Location,
                    StatusName = o.StatusName,
                    AssignedTo = amcAssignLookup.TryGetValue(o.AmcId, out var asl) ? asl : new List<ass>(),
                    nexttime = amcRemLookup.TryGetValue(o.AmcId, out var nt) ? nt : (DateTime?)null,
                    ldate = ldateVal,
                };
            }).OrderByDescending(o => o.ldate).ToList();

            if (AssignedTo != null)
            {
                v = v.Where(x => x.AssignedTo.Select(z => z.id).ToList().Contains((long)AssignedTo)).ToList();
            }
            if (nextdate != "")
            {

                DateTime nextdates = DateTime.Parse(nextdate, new CultureInfo("en-GB"));
                DateTime nextdatesadd = nextdates.AddDays(1);
                v = v.Where(o => o.nexttime >= nextdates && o.nexttime < nextdatesadd).ToList();
            }

            //SEARCH
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.AmcNo.ToString().ToLower().Contains(search.ToLower()) ||
                                p.CustomerName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.ContractName.ToString().ToLower().Contains(search.ToLower())
                            ).ToList();

            }


            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        [RedirectingAction]
        [HttpPost]
        public ActionResult GetDetailsdocument( string FromDate, string ToDate,long?  documenttype)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            DateTime? fFromdate = null;
            DateTime? tTodate = null;
           
            if (FromDate != "")
            {
                fFromdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "")
            {
                tTodate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }
       
     
               

            var v = (from a in db.Amcs
                     join b in db.Customers on a.CustomerId equals b.CustomerID into temp1
                     from b in temp1.DefaultIfEmpty()
                     join c in db.AmcDocuments on a.AmcId equals c.TransId
                     join d in db.DocumentTypes on c.DocumentTypeID equals d.ID
                  where
                     (documenttype == 0 || documenttype == null || c.DocumentTypeID == documenttype) &&
    

                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(c.Expiry, fFromdate) <= 0) &&
           
                      (ToDate == "" || ToDate == null || EF.Functions.DateDiffDay(c.Expiry, tTodate) >= 0) &&
                      (c.Expiry!=null)





                     select new
                     {
                         docdownload = db.AmcDocuments.Where(aa => aa.TransId == a.AmcId).Count(),
                         AmcId = a.AmcId,
                         AmcNo = a.AmcNo,
                         a.OpenClose,
                         StartDate = a.StartDate,
                         EndDate = a.EndDate,
                         ReminderDate = a.ReminderDate,
                         CustomerId = a.CustomerId,
                         CustomerName = (b.CustomerName == null) ? "No Name" : b.CustomerName,
                        documenttype = d.Name,
                        expiryate =c.Expiry
                       
                         
                        
                     }).OrderByDescending(o => o.expiryate).ToList();

            //SEARCH
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.AmcNo.ToString().ToLower().Contains(search.ToLower()) ||
                                p.CustomerName.ToString().ToLower().Contains(search.ToLower()) 
                            ).ToList();

            }


            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var userid = User.Identity.GetUserId();
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }




        public JsonResult SearchTaskStatusName(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AmcStatuss.Where(p => p.StatusName.ToLower().Contains(q.ToLower()) || p.StatusName.Contains(q) || p.StatusName.StartsWith(q) || p.StatusName.EndsWith(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.StatusName, //each json object will have 
                                      id = b.AmcStatusId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.AmcStatuss.Select(b => new SelectFormat
                {
                    text = b.StatusName, //each json object will have 
                    id = b.AmcStatusId
                }).OrderBy(b => b.text).ToList();

            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Task Status " };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public ActionResult myamcnew()
        {

            ViewBag.TStat = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            //For Dropdown Asset Account
            AMCViewModel vmodel = new AMCViewModel();
            ViewBag.AssignedTo = QkSelect.List(
                            new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                            }, "Value", "Text", 1);
            ViewBag.Contracts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "AMC" && a.Status == Status.active).ToList();

            var ref1 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Amcs
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");


            return View(vmodel);

        }

        // GET: MyAMC
        public ActionResult MyAMC()
        {
            ViewBag.TStat = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            //For Dropdown Asset Account
            ViewBag.AssignedTo = QkSelect.List(
                            new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                            }, "Value", "Text", 1);
            ViewBag.Contracts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            return View();
        }

        //My AMC List ---- GET
        [RedirectingAction]
        [HttpPost]
        public ActionResult GetMyAMCDetails(long? ContractLevelId,long? ContractNo, long? Customer, long? ContractName, long? ContractType, string RFromDate, string RToDate, string StartDate, string EndDate, string EFromDate)
        {
            string search   =   Request.Form.GetValues("search[value]")[0];
            var draw        =   Request.Form.GetValues("draw").FirstOrDefault();
            var start       =   Request.Form.GetValues("start").FirstOrDefault();
            var length      =   Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn      =   Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir   =   Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? rfdate = null;
            DateTime? rtdate = null;
            DateTime? etdate = null;


            if (StartDate != "")
            {
                fdate = DateTime.Parse(StartDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (EndDate != "")
            {
                tdate = DateTime.Parse(EndDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (EFromDate != "")
            {
                etdate = DateTime.Parse(EFromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (RFromDate != "")
            {
                rfdate = DateTime.Parse(RFromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (RToDate != "")
            {
                rtdate = DateTime.Parse(RToDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            int pageSize        =   length != null ? Convert.ToInt32(length) : 0;
            int skip            =   start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal    =   0;
            var UserId          =   User.Identity.GetUserId();
            var EmpId           =   db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            // AmcUp (latest AmcUpdation per Amc) and AssignedTo (nested collection) are computed client-side below —
            // EF Core 10 cannot translate the GroupBy-latest-per-group join nor the nested .ToList() projection.
            // The "My" filter (assign.Contains(EmpId)) is preserved server-side as a translatable EXISTS subquery.
            var serverRows = (from a in db.Amcs
                     join b in db.Customers on a.CustomerId equals b.CustomerID into temp1
                     from b in temp1.DefaultIfEmpty()
                     join c in db.AmcContracts on a.ContractId equals c.ContractId into temp2
                     from c in temp2.DefaultIfEmpty()
                     join d in db.AmcContractTypes on a.ContractTypeId equals d.TypeId into temp3
                     from d in temp3.DefaultIfEmpty()
                     join e in db.AmcStatuss on a.AmcStatusId equals e.AmcStatusId into temp4
                     from e in temp4.DefaultIfEmpty()
                     join f in db.LocationNames on a.LocationId equals f.LocationId into temp5
                     from f in temp5.DefaultIfEmpty()
                     where
                     (a.OpenClose==0||a.OpenClose==null) &&
                     (ContractLevelId == 0 || ContractLevelId == null || a.ContractLevelId == (ContractLevelId - 1)) &&

                     (ContractNo    ==  0    || ContractNo      == null || a.AmcNo          ==  ContractNo) &&
                     (Customer      ==  0    || Customer        == null || a.CustomerId     ==  Customer) &&
                     (ContractName  ==  0    || ContractName    == null || a.ContractId     ==  ContractName) &&
                     (ContractType  ==  0    || ContractType    == null || a.ContractTypeId ==  ContractType) &&
                     (StartDate     ==  ""   || StartDate       == null || EF.Functions.DateDiffDay(a.StartDate, fdate) <= 0) &&
                     (EndDate       ==  ""   || EndDate         == null || EF.Functions.DateDiffDay(a.EndDate, tdate) >= 0)&&
                      (RFromDate == "" || RFromDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rfdate) <= 0) &&
                     (RToDate == "" || RToDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rtdate) >= 0)&&



                     db.AmcAssignedTos.Any(x => x.AmcId == a.AmcId && x.Status == "Assigned" && x.ChkStatus == Status.active && x.EmployeeId == EmpId)
                     select new
                     {
                         AmcId          =   a.AmcId,
                         a.ContractLevelId,
                         AmcNo          =   a.AmcNo,
                         StartDate      =   a.StartDate,
                         EndDate        =   a.EndDate,
                         ReminderDate   =   a.ReminderDate,
                         CustomerId     =   a.CustomerId,
                         CustomerName   =   b.CustomerName,
                         ContractName   =   c.ContractName,
                         ContractType   =   d.Type,
                         Location       =   f.Location,
                         StatusName     =   e.StatusName,
                         LogTime        =   a.LogTime,
                     }).ToList();

            // Latest AmcUpdation per Amc + assignees, computed client-side and keyed by AmcId.
            var amcIdList = serverRows.Select(o => o.AmcId).ToList();
            var amcUpLookup = db.AmcUpdations
                .Where(u => u.TransType == "Amc" && amcIdList.Contains(u.TransId))
                .ToList()
                .GroupBy(u => u.TransId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.CreatedDate).First());
            var amcAssignLookup = (from z in db.AmcAssignedTos
                                   join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                   where z.Status == "Assigned" && z.ChkStatus == Status.active && amcIdList.Contains(z.AmcId)
                                   select new { z.AmcId, y.EmployeeId, y.LastName, y.FirstName, y.MiddleName, y.ImgFileName, y.Status })
                                  .ToList()
                                  .GroupBy(x => x.AmcId)
                                  .ToDictionary(g => g.Key, g => g.Select(y => new ass
                                  {
                                      id = y.EmployeeId,
                                      LastName = (y.LastName != null) ? y.LastName : "",
                                      FirstName = (y.FirstName != null) ? y.FirstName : "",
                                      MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                      Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                      Status = y.Status
                                  }).Distinct().ToList());

            var v = serverRows.Select(o =>
            {
                DateTime? ldateVal = o.LogTime;
                if (amcUpLookup.TryGetValue(o.AmcId, out var up) && up.CreatedDate > o.LogTime)
                    ldateVal = up.CreatedDate;
                return new
                {
                    AmcId          =   o.AmcId,
                    o.ContractLevelId,
                    AmcNo          =   o.AmcNo,
                    StartDate      =   o.StartDate,
                    EndDate        =   o.EndDate,
                    ReminderDate   =   o.ReminderDate,
                    CustomerId     =   o.CustomerId,
                    CustomerName   =   o.CustomerName,
                    ContractName   =   o.ContractName,
                    ContractType   =   o.ContractType,
                    Location       =   o.Location,
                    StatusName     =   o.StatusName,
                    AssignedTo     =   amcAssignLookup.TryGetValue(o.AmcId, out var asl) ? asl : new List<ass>(),
                    ldate          =   ldateVal,
                };
            }).OrderByDescending(a=>a.ldate).ToList();

            //SEARCH
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.AmcNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.CustomerName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.ContractName.ToString().ToLower().Contains(search.ToLower())
                            ).ToList();
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
            }
            recordsTotal = v.Count();
            var data = v.OrderByDescending(o => o.ContractLevelId).ThenByDescending(x => x.ldate).Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: AMC Create
        public ActionResult Create(long? CustId)
        {
            DateTime Today  = System.DateTime.Now;
            DateTime NextYr = Today.AddYears(1);
            DateTime temp = NextYr.AddDays(-1);
            var ContractTypes = db.AmcContractTypes.Select(s => new
            {
                Id = s.TypeId,
                Name = s.Type,
            }).ToList();
            ViewBag.ContractTypes = QkSelect.List(ContractTypes, "Id", "Name");
            AMCViewModel viewModel = new AMCViewModel
            {
                AmcNo = GetContractNo(),
                StartDate   =   Today.ToString("dd-MM-yyyy"),
                EndDate     =   temp.ToString("dd-MM-yyyy"),
                ReminderDate =  NextYr.ToString("dd-MM-yyyy")
            };

            var AmcStatus = db.AmcStatuss.Where(a => a.AmcStatusId == 1).Select(a => a.StatusName).FirstOrDefault();

            ViewBag.Dropdowns = QkSelect.List(
                      new List<SelectListItem>
                      {
                                    new SelectListItem { Selected = true,Text =AmcStatus, Value ="1" },
                      }, "Value", "Text", 1);

            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            ViewBag.AssignTypes = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                              }, "Value", "Text", 0);

            viewModel.FieldMap = db.FieldMappings.Where(a => a.Section == "AMC" && a.Status == Status.active).ToList();
            viewModel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "AMC").ToList();

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            //For Optional Fields
            var ref1 = db.Amcs
             .Select(s => new
             {
                 ID     = s.Ref1,
                 Name   = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Amcs
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            //For Customers DropDown
            var cus = db.Customers.Where(o => (CustId == null || o.CustomerID == CustId)).Take(1).Select(s => new { ID = s.CustomerID, Name = s.CustomerName });
            ViewBag.Customers = QkSelect.List(cus, "ID", "Name");

            //To Fill The Contact Details Of Selected Customer
            if (CustId != null)
            {
                viewModel.CustomerId    =   Convert.ToInt64(CustId);
                viewModel.LstContacts   =   (from c in db.Contacts
                                            join cr in db.ContactRelation
                                            on c.ContactID equals cr.ContactID
                                            where (cr.RelationID == CustId)
                                            select new
                                            {
                                              ContactID     = c.ContactID,
                                              Name          = c.Name,
                                              FirstName     = c.FirstName,
                                              LastName      = c.LastName,
                                              Address       = c.Address,
                                              Country       = c.Country,
                                              State         = c.State,
                                              City          = c.City,
                                              Zip           = c.Zip,
                                              Phone         = c.Phone,
                                              Mobile        = c.Mobile,
                                              Fax           = c.Fax,
                                              EmailId       = c.EmailId,
                                              Reference     = c.Reference,
                                              ContactPerson = c.ContactPerson,
                                              Status        = c.Status,
                                              Group         = c.Group,
                                              SalesPMob     = c.SalesPMob,
                                              TypeOfContact = c.TypeOfContact,
                                              Website       = c.Website,
                                              CountryID     = c.CountryID,
                                              ContactTypeID = c.ContactTypeID
                                       }).AsEnumerable().Select(x => new Contact
                                       {
                                           ContactID        =   x.ContactID,
                                           Name             =   x.Name,
                                           FirstName        =   x.FirstName,
                                           LastName         =   x.LastName,
                                           Address          =   x.Address,
                                           Country          =   x.Country,
                                           State            =   x.State,
                                           City             =   x.City,
                                           Zip              =   x.Zip,
                                           Phone            =   x.Phone,
                                           Mobile           =   x.Mobile,
                                           Fax              =   x.Fax,
                                           EmailId          =   x.EmailId,
                                           Reference        =   x.Reference,
                                           ContactPerson    =   x.ContactPerson,
                                           Status           =   x.Status,
                                           Group            =   x.Group,
                                           SalesPMob        =   x.SalesPMob,
                                           TypeOfContact    =   x.TypeOfContact,
                                           Website          =   x.Website,
                                           CountryID        =   x.CountryID,
                                           ContactTypeID    =   x.ContactTypeID
                                       }).ToList();

                ViewBag.CountryCodes = db.Country.ToList();
                ViewBag.TypeList = db.ContactTypes.ToList();       
            }
            return View(viewModel);
        }

        //Function To Get ContractNo
        private long GetContractNo()
        {
            Int64 ContractNo = 0, LastNo = 0;

            LastNo = db.Amcs.Select(p => p.AmcNo).DefaultIfEmpty().Max();

            if (LastNo == 0)
                ContractNo = 1000;
            else
                ContractNo = LastNo + 1;

            return ContractNo;
        }

        //Function To Get Periodic Maintenance No
        private long GetPMaintNo()
        {
            Int64 PMaintNo = 0, LastNo = 0;

            LastNo = db.PeriodicMaintenances.Select(p => p.PeriodicMaintenanceNo).DefaultIfEmpty().Max();

            if (LastNo == 0 || LastNo == null)
                PMaintNo = 1000;
            else
                PMaintNo = LastNo + 1;

            return PMaintNo;
        }

        //Saving-- CREATE ---POST
        [HttpPost]
        public ActionResult Create(AMCViewModel AmcViewModel)
        {
            string msg;
            bool stat;
            DateTime Today = System.DateTime.Now;

            var AmcExists = db.Amcs.Any(u => u.AmcNo == AmcViewModel.AmcNo);
            if (AmcExists)
            {               
                Danger("An AMC with same Contract No. exists...", true);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Int64 AmcId = 0;
                    bool PerdMaintReq;
                    var UserId = User.Identity.GetUserId();
                    DateTime StartDate      =   DateTime.Parse(AmcViewModel.StartDate.ToString(), new CultureInfo("en-GB"));
                    DateTime EndDate        =   DateTime.Parse(AmcViewModel.EndDate.ToString(), new CultureInfo("en-GB"));
                    DateTime ReminderDate   =   DateTime.Parse(AmcViewModel.ReminderDate.ToString(), new CultureInfo("en-GB"));

                    var custcord = db.Customers.Where(o => o.CustomerID == AmcViewModel.CustomerId).Select(o => new { o.Lattitude, o.Longitude }).FirstOrDefault();

                   Amc AmcObj = new Amc
                    {
                        AmcNo               =   GetContractNo(),
                        ContractId          =   AmcViewModel.ContractId,
                        CustomerId          =   Convert.ToInt64(AmcViewModel.CustomerId),
                        StartDate           =   StartDate,
                        EndDate             =   EndDate,
                        ReminderDate        =   ReminderDate,
                        ContractTypeId      =   AmcViewModel.ContractTypeId,
                        ContractLevelId     =   AmcViewModel.ContractLevelId,
                        LocationId          =   AmcViewModel.LocationId,
                    
                        Lattitude           =   AmcViewModel.Lattitude,
                        Longitude           =   AmcViewModel.Longitude,
                        PeriodicMaintReqrd  =   AmcViewModel.PerdcMaintReq,
                        AmcStatusId         =   AmcViewModel.AmcStatusId,
                        AmcDetails          =   AmcViewModel.AmcDetails,
                        Notes               =   AmcViewModel.Notes,
                        CreatedBy           =   UserId,
                        CreatedDate         =   Today,
                        LogTime             =   Today,
                        Ref1                =   AmcViewModel.Ref1,
                        Ref2                =   AmcViewModel.Ref2,
                        Ref3                =   AmcViewModel.Ref3,
                        Ref4                =   AmcViewModel.Ref4,
                        Ref5                =   AmcViewModel.Ref5,
                        OpenClose           =   AmcViewModel.OpenClose,
                    };
                    if ((AmcViewModel.Lattitude == "" || AmcViewModel.Lattitude ==null)&& custcord.Lattitude!="")
                    {
                        AmcObj.Lattitude = custcord.Lattitude;
                        AmcObj.Longitude = custcord.Longitude;
                        AmcViewModel.Lattitude = custcord.Lattitude;
                        AmcViewModel.Longitude = custcord.Longitude;
                    }
                        db.Amcs.Add(AmcObj);
                    db.SaveChanges();


                    AmcId = AmcObj.AmcId;

                    /************************ Assigned Team ***************************/
                    if (AmcViewModel.AssignTypeAll != null)
                    {
                        AmcAssignedTeam TeamObj = new AmcAssignedTeam();
                        foreach (var arr in AmcViewModel.AssignTypeAll)
                        {
                            TeamObj.AmcId   = AmcId;
                            TeamObj.TeamId  = arr;
                            db.AmcAssignedTeams.Add(TeamObj);
                            db.SaveChanges();
                        }
                    }

                    /************************* Assigned To ***************************/
                    if (AmcViewModel.AssignedToo != null)
                    {
                        AmcAssignedTo AssignTo = new AmcAssignedTo();

                        foreach (var arr in AmcViewModel.AssignedToo)
                        {
                            AssignTo.AmcId          =   AmcId;
                            AssignTo.EmployeeId     =   arr;
                            AssignTo.Status         =   "Assigned";
                            AssignTo.AssignBy       =   UserId;
                            AssignTo.ChkStatus      =   Status.active;
                            AssignTo.CreatedDate    =   Convert.ToDateTime(System.DateTime.Now);
                            com.remideradd(rootdomain + "amc/myamc", arr, UserId, "amc Assined", AmcId);

                            db.AmcAssignedTos.Add(AssignTo);
                            db.SaveChanges();
                        }
                    }

                    //************For Saving Contacts
                    if (AmcViewModel.LstContacts != null && AmcViewModel.LstContacts.Count > 0)
                    {
                        AddContact(AmcViewModel, AmcId);
                    }

                    //************For saving Attach Documents
                    if (AmcViewModel.LstAmcDocument != null && AmcViewModel.LstAmcDocument.Count > 0)
                    {
                        AddDocuments(AmcViewModel, AmcId, Today, "Create");
                    }

                    //************For saving Periodic Maintenance Details
                    if (AmcViewModel.PerdcMaintReq == true)
                    {                       
                        AddPeriodicMaintenance(AmcViewModel, AmcId, Today);
                    }

                    /************************* Reminder ***************************/

                    //Reminder for Reminder Date
                    db.Reminderss.Add(new Reminderss
                    {
                        CreatedBy   =   User.Identity.GetUserId(),
                        Note        =   "AMC - "+AmcViewModel.AmcNo,
                        RDate       =   ReminderDate,
                        CreatedDate =   System.DateTime.Now,
                        Reference   =   AmcId,
                        Type        =   "AMC",
                        actionurl   =   "AMC/Edit/",
                        RequestBy   =   UserId,
                        Status      =   0,
                        RStatus     =   "Open"
                    });
                    db.SaveChanges();

                    #region amcdocreminder
                    var TodayS = Convert.ToDateTime(System.DateTime.Now);
                    var datecheck = TodayS.AddDays(7);
                    var expry = TodayS.AddMonths(-3);
                    var amcdoc = (from a in db.Amcs
                                  join b in db.AmcDocuments on a.AmcId equals b.TransId
                                  join c in db.DocumentTypes on b.DocumentTypeID equals c.ID
                                  join d in db.Customers on a.CustomerId equals d.CustomerID
                                  where b.Expiry <= datecheck
                                  && a.OpenClose == 0 && b.Expiry != null &&
                              b.Expiry >= expry

                                  select new
                                  {
                                      amcid = a.AmcId,
                                      documentid = b.DocumentId,
                                      documenttype = b.DocumentTypeID,
                                      b.Expiry,
                                      b.FileName,
                                      b.Notes,
                                      c.Name,
                                      a.AmcNo,
                                      d.CustomerName


                                  }).ToList();
                    var userid = User.Identity.GetUserId();

                    db.Reminders.RemoveRange(db.Reminders.Where(o => o.Note.Contains("AMC Document Expiry")));
                    db.SaveChanges();
                    db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.Type == "amcdocumentnotification"));
                    db.SaveChanges();
                    foreach (var v in amcdoc)
                    {

                        // v.documenttype  "ADMCC Certificate","ADMMC","AMC Contract"




                        if (v.documenttype == 45)
                        {




                            if (1 == 1)
                            {
                                if (1 == 1)
                                {

                                    Reminder reminds = new Reminder();
                                    reminds.Reference = v.amcid;
                                    reminds.Note = "AMC Document Expiry  : " + v.Name + "<br>  AMC No" + v.AmcNo + " Customer " + v.CustomerName;

                                    //seleted date added,for fullcalender



                                    reminds.RDate = System.DateTime.Now;
                                    reminds.Type = "/Amc/Edit/" + v.amcid;
                                    reminds.RStatus = "Close";
                                    reminds.RequestBy = userid;

                                    reminds.CreatedBy = userid;
                                    reminds.Status = Status.active;
                                    reminds.CreatedDate = System.DateTime.Now;
                                    db.Reminders.Add(reminds);
                                    db.SaveChanges();
                                    long Id = reminds.ReminderId;
                                    var emp = db.DocExpiryReminders.ToList();


                                    foreach (var arr in emp)
                                    {
                                        if (v.Expiry != null)
                                        {
                                            double minusday = Convert.ToDouble(arr.days) * -1;
                                            var datediff = (System.DateTime.Now.AddDays(minusday) - v.Expiry).Value.TotalDays;


                                            if (arr.days >= datediff)
                                            {
                                                ReminderAssigned remAs = new ReminderAssigned();

                                                remAs.ReminderId = Id;
                                                remAs.EntryId = v.amcid;
                                                remAs.Type = "amcdocumentnotification";
                                                remAs.EmployeeId = arr.EmployeeID;
                                                db.ReminderAssigneds.Add(remAs);
                                                db.SaveChanges();
                                            }
                                        }

                                    }
                                }
                            }
                        }





















                    }
                    ////***************************Periodic Maintenance
                    #endregion
                    /*
                    if (AmcViewModel.LstSalesEntry != null && AmcViewModel.LstSalesEntry.Count > 0)
                    {
                        foreach (var item in AmcViewModel.LstSalesEntry)
                        {
                            SalesEntry salesEntry = new SalesEntry();

                            salesEntry.BillNo = item.BillNo;
                            salesEntry.SEDate = item.SEDate;
                            salesEntry.SEGrandTotal = item.SEGrandTotal;
                            salesEntry.DueDate = item.DueDate;
                            salesEntry.DueReason = item.DueReason;
                            salesEntry.Customer = AmcId;
                            salesEntry.SECreatedDate = System.DateTime.Now;
                            db.SalesEntrys.Add(salesEntry);
                            db.SaveChanges();

                            long sid = salesEntry.SalesEntryId;

                            SEPayment SEpay = new SEPayment();
                            SEpay.SEDate = item.SEDate;
                            SEpay.SEEntryDate = Convert.EndDateTime(System.DateTime.Now);
                            SEpay.SEBillAmount = item.SEGrandTotal;
                            //walking customer
                            SEpay.SEPaidAmount = 0;
                            SEpay.CustomerId = AmcId;//saledata[0]
                            SEpay.CreatedBranch = 1;
                            SEpay.CreatedUserId = UserId;
                            SEpay.SECreatedDate = Convert.EndDateTime(System.DateTime.Now);
                            SEpay.Status = 1;
                            SEpay.SalesEntry = sid;
                            db.SEPayments.Add(SEpay);
                            db.SaveChanges();
                        }
                    }
                    */
                    com.addlog(LogTypes.Created, UserId, "AMC", "Amcs", findip(), AmcId, "Amc Added Successfully");

                    //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                   Success("Amc created successfully...", true);
                }
                else
                {                   
                    Danger("Failed to add Amc...", true);
                }
            }

            return RedirectToAction("Index", "AMC");
        }

        // GET:/Edit
        public ActionResult Edit(long? id, long? CustId)
        {
            Amc AmcObj = db.Amcs.Find(id);

            if (AmcObj == null)
            {
                return NotFound();
            }

            //Customers
           

            //Contracts
            var Contracts = db.AmcContracts.Select(s => new
            {
                Id = s.ContractId,
                Name = s.ContractName,
            }).ToList();
            ViewBag.Contracts = QkSelect.List(Contracts, "Id", "Name");

            //Location
            var Location = db.LocationNames.Select(s => new
            {
                Id = s.LocationId,
                Name = s.Location,
            }).ToList();
            ViewBag.Location = QkSelect.List(Location, "Id", "Name");

            //Contract Types
            var ContractTypes = db.AmcContractTypes.Select(s => new
            {
                Id = s.TypeId,
                Name = s.Type,
            }).ToList();
            ViewBag.ContractTypes = QkSelect.List(ContractTypes, "Id", "Name");

            //Contract Level
            ViewBag.ContractLevel = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = true, Text = "Select Contract Level", Value = "0"},
                       }, "Value", "Text", 1);

            //Contract Status
            var AmcStatus = db.AmcStatuss.Select(s => new
            {
                Id = s.AmcStatusId,
                Name = s.StatusName,
            }).ToList();
            ViewBag.AmcStatus = QkSelect.List(AmcStatus, "Id", "Name");
           
            AMCViewModel AmcModel = new AMCViewModel();
            
            AmcModel = (from a in db.Amcs
                        join b in db.AmcContracts on a.ContractId equals b.ContractId into cntr
                        from b in cntr.DefaultIfEmpty()
                        join c in db.Customers on a.CustomerId equals c.CustomerID
                        join d in db.AmcContractTypes on a.ContractTypeId equals d.TypeId into Temp1
                        from d in Temp1.DefaultIfEmpty()
                        join e in db.AmcStatuss on a.AmcStatusId equals e.AmcStatusId into Temp2
                        from e in Temp2.DefaultIfEmpty()
                        join f in db.LocationNames on a.LocationId equals f.LocationId into Temp3
                        from f in Temp3.DefaultIfEmpty()
                        join g in db.PeriodicMaintenances on a.AmcId equals g.AmcId into Temp4
                        from g in Temp4.DefaultIfEmpty()
                        where a.AmcId == id

                        select new AMCViewModel
                        {
                            AmcNo           =   a.AmcNo,
                            CustomerId      =   a.CustomerId,
                            ContractId      =   a.ContractId,
                            ContractTypeId  =   a.ContractTypeId,
                            ContractLevelId =   a.ContractLevelId,
                            LocationId      =   a.LocationId,
                            Lattitude       =   a.Lattitude,
                            Longitude       =   a.Longitude,
                            AmcStatusId     =   a.AmcStatusId,
                            PerdcMaintReq   =   a.PeriodicMaintReqrd,
                            AmcDetails      =   a.AmcDetails,
                            Notes           =   a.Notes,
                            Ref1            =   a.Ref1,
                            Ref2            =   a.Ref2,
                            Ref3            =   a.Ref3,
                            Ref4            =   a.Ref4,
                            Ref5            =   a.Ref5,
                            NoOfPMaint      =   g.NoOfPMaintenance,
                            OpenClose       =   a.OpenClose
                           
                        }).FirstOrDefault();
         
            AmcModel.StartDate = AmcObj.StartDate.ToString("dd-MM-yyyy");
            AmcModel.EndDate = AmcObj.EndDate.ToString("dd-MM-yyyy");
            AmcModel.ReminderDate = AmcObj.ReminderDate.ToString("dd-MM-yyyy");

            //Document           
            AmcModel.LstAmcDocument = (from cd in db.AmcDocuments
                                       where (cd.TransId == id )
                                       select new AmcDocumentViewModel
                                       {
                                           TransId          =   cd.TransId,
                                           Expiry           =   cd.Expiry,
                                           Notes            =   cd.Notes,
                                          // FileName         =   cd.FileName,
                                           FileNameAmc      =   cd.FileName,
                                           DocumentTypeID   =   cd.DocumentTypeID,
                                           TransType =cd.TransType
                                       }
                                     ).Distinct().ToList();           
            //Periodic Maintenance
            AmcModel.LstPerdcMaint = (from a in db.PeriodicMaintenanceDetails
                                      join b in db.PeriodicMaintenances
                                      on a.PeriodicMaintenanceId equals b.PeriodicMaintenanceId
                                      where b.AmcId == id
                                      select new PrdcMaintViewModel
                                      {
                                          AmcId = b.AmcId,
                                          PMaintId = b.PeriodicMaintenanceId,
                                          PDate = a.PDate,
                                          Notes = a.Notes
                                      }).ToList();

            //For optional fields
            AmcModel.FieldMap    = db.FieldMappings.Where(a => a.Section == "AMC" && a.Status == Status.active).ToList();
            AmcModel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "AMC").ToList();

            var ref1 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Amcs
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Amcs
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            //Assign Team
            AmcModel.AssignTypeAll = db.AmcAssignedTeams.Where(a => a.AmcId == id).Select(a => a.TeamId).ToList().ToArray() ?? null;

            var Teams = db.Teams
                    .Select(s => new
                    {
                        ID      =   s.TeamId,
                        Name    =   s.TeamName
                    })
                    .ToList();
            ViewBag.AssignTeam = new MultiSelectList(Teams, "ID", "Name", AmcModel.AssignTypeAll);

            //Assigned Too
            AmcModel.AssignedTo = db.AmcAssignedTos.Where(x => x.AmcId == id && x.Status == "Assigned" && x.ChkStatus == (int)Status.active).Select(a => a.EmployeeId).ToList().ToArray() ?? null;

            var TMembers = db.Employees
                   .Select(s => new
                   {
                       ID   =   s.EmployeeId,
                       Name =   s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.AssignTo = new MultiSelectList(TMembers, "ID", "Name", AmcModel.AssignedTo);

            ViewBag.CountryCodes    =   db.Country.ToList();
            ViewBag.TypeList        =   db.ContactTypes.ToList();
            ViewBag.DocList         =   db.DocumentTypes.ToList();
            var curryear = System.DateTime.Now.AddYears(1).Year;
            List<SelectListItem> val = new List<SelectListItem>();
            for(var i=curryear;i>=curryear-10;i--)

            {
               var it = new SelectListItem { Text = i.ToString(), Value = i.ToString() };
                val.Add(it);
            }
            ViewBag.docgroup = val.ToList();

            //To Fill in the text box of Periodic Maintenance
            var NoofPMaint = db.PeriodicMaintenances.Where(a => a.AmcId == id).Select(a => a.NoOfPMaintenance).FirstOrDefault();
            if (NoofPMaint == 0)
                ViewBag.NoofPMaint = "";
            else
                ViewBag.NoofPMaint = NoofPMaint;

            if (CustId == null)
            {
                //Contacts
                AmcModel.LstContacts = (from c in db.Contacts
                                        join cr in db.ContactRelation
                                        on new { c.ContactID, RelationType = (long)ContctRelation.Amc }
                                        equals new { cr.ContactID, cr.RelationType }
                                        where (cr.RelationID == id)
                                        select new
                                        {
                                            ContactID       =   c.ContactID,
                                            Name            =   c.Name,
                                            Address         =   c.Address,
                                            Country         =   c.Country,
                                            State           =   c.State,
                                            City            =   c.City,
                                            Zip             =   c.Zip,
                                            Phone           =   c.Phone,
                                            Mobile          =   c.Mobile,
                                            Fax             =   c.Fax,
                                            EmailId         =   c.EmailId,
                                            Reference       =   c.Reference,
                                            ContactPerson   =   c.ContactPerson,
                                            Status          =   c.Status,
                                            Group           =   c.Group,
                                            SalesPMob       =   c.SalesPMob,
                                            TypeOfContact   =   c.TypeOfContact,
                                            Website         =   c.Website,
                                            CountryID       =   c.CountryID,
                                            ContactTypeID   =   c.ContactTypeID,
                                            c.FirstName,
                                            c.LastName
                                        }).AsEnumerable().Select(x => new Contact
                                        {
                                            ContactID       =   x.ContactID,
                                            Name            =   x.Name,
                                            FirstName       =   x.FirstName,
                                            LastName        =   x.LastName,
                                            Address         =   x.Address,
                                            Country         =   x.Country,
                                            State           =   x.State,
                                            City            =   x.City,
                                            Zip             =   x.Zip,
                                            Phone           =   x.Phone,
                                            Mobile          =   x.Mobile,
                                            Fax             =   x.Fax,
                                            EmailId         =   x.EmailId,
                                            Reference       =   x.Reference,
                                            ContactPerson   =   x.ContactPerson,
                                            Status          =   x.Status,
                                            Group           =   x.Group,
                                            SalesPMob       =   x.SalesPMob,
                                            TypeOfContact   =   x.TypeOfContact,
                                            Website         =   x.Website,
                                            CountryID       =   x.CountryID,
                                            ContactTypeID   =   x.ContactTypeID
                                        }).ToList();
            }
            else
            {
                if (CustId != null)
                {
                    AmcModel.CustomerId = CustId;
                    AmcModel.LstContacts = (from c in db.Contacts
                                            join cr in db.ContactRelation
                                            on new { c.ContactID, RelationType = (long)ContctRelation.Customer }
                                            equals new { cr.ContactID, cr.RelationType }
                                            where (cr.RelationID == CustId)
                                            select new
                                            {
                                                ContactID       =   c.ContactID,
                                                Name            =   c.Name,
                                                Address         =   c.Address,
                                                Country         =   c.Country,
                                                State           =   c.State,
                                                City            =   c.City,
                                                Zip             =   c.Zip,
                                                Phone           =   c.Phone,
                                                Mobile          =   c.Mobile,
                                                Fax             =   c.Fax,
                                                EmailId         =   c.EmailId,
                                                Reference       =   c.Reference,
                                                ContactPerson   =   c.ContactPerson,
                                                Status          =   c.Status,
                                                Group           =   c.Group,
                                                SalesPMob       =   c.SalesPMob,
                                                TypeOfContact   =   c.TypeOfContact,
                                                Website         =   c.Website,
                                                CountryID       =   c.CountryID,
                                                ContactTypeID   =   c.ContactTypeID,
                                                c.FirstName,
                                                c.LastName
                                            }).AsEnumerable().Select(x => new Contact
                                            {
                                                ContactID       =   x.ContactID,
                                                Name            =   x.Name,
                                                FirstName       =   x.FirstName,
                                                LastName        =   x.LastName,
                                                Address         =   x.Address,
                                                Country         =   x.Country,
                                                State           =   x.State,
                                                City            =   x.City,
                                                Zip             =   x.Zip,
                                                Phone           =   x.Phone,
                                                Mobile          =   x.Mobile,
                                                Fax             =   x.Fax,
                                                EmailId         =   x.EmailId,
                                                Reference       =   x.Reference,
                                                ContactPerson   =   x.ContactPerson,
                                                Status          =   x.Status,
                                                Group           =   x.Group,
                                                SalesPMob       =   x.SalesPMob,
                                                TypeOfContact   =   x.TypeOfContact,
                                                Website         =   x.Website,
                                                CountryID       =   x.CountryID,
                                                ContactTypeID   =   x.ContactTypeID
                                            }).ToList();
                }
            }
            if (AmcModel.OpenClose == 0)
            {
                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "OPEN", Value = "0"
                },
                new SelectListItem {
                    Text = "CLOSE", Value = "1"
                }
              };
                ViewBag.OpnCls = pstat2;

            }
            else if (AmcModel.OpenClose == 1)
            {

                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "CLOSE", Value = "1"
                },
                new SelectListItem {
                    Text = "OPEN", Value = "0"
                }
              };
                ViewBag.OpnCls = pstat2;
            }
            else
            {
                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "OPEN", Value = "0"
                },
                new SelectListItem {
                    Text = "CLOSE", Value = "1"
                },
              };
                ViewBag.OpnCls = pstat2;
            }
            var Customers = db.Customers.Where(o=>o.CustomerID==AmcModel.CustomerId).Select(s => new
            {
                Id = s.CustomerID,
                Name = s.CustomerName,
            }).ToList();
            ViewBag.Customers = QkSelect.List(Customers, "Id", "Name");
            return View(AmcModel);
        }
        //POST ---Edit
        [HttpPost]
        public ActionResult Editdoc(amcdocattach AmcModel)
        {
            
            // For Saving Amc Documents
            if (AmcModel.AmcDocuments != null && AmcModel.AmcDocuments.Count > 0)
            {
                AddDocumentsdocadd(AmcModel, AmcModel.amcid, System.DateTime.Now, "Edit");
            }
            return Redirect(ControllerContext.HttpContext.Request.GetUrlReferrer().ToString());

        }
            //POST ---Edit
            [HttpPost]
        public ActionResult Edit(AMCViewModel AmcModel, Int64 id)
        {
            bool stat = false;
            string msg;
            bool PerdMaintReq;

            if (ModelState.IsValid)
            {
                if (AmcModel.OpenClose == 1)
                {
                    var exist = db.ProTasks.Any(o => o.VModId == id&&o.OpenClose==0);//o.StartDate == row.PDate && o.TaskName.Contains("AMC - Periodic Maintenance"));
                if(exist)
                    {
                        var taskcode = db.ProTasks.Where(o => o.VModId == id && o.OpenClose == 0).Select(o => o.TaskCode).FirstOrDefault();
                        Danger("AMC Can not Close. Because Task No : " + taskcode + " is Open.");
                        return RedirectToAction("Edit/" + id, "AMC");
                    }
                }
                var UserId = User.Identity.GetUserId();
                DateTime Today          =   System.DateTime.Now;
                DateTime StartDate      =   DateTime.Parse(AmcModel.StartDate.ToString(), new CultureInfo("en-GB"));
                DateTime EndDate        =   DateTime.Parse(AmcModel.EndDate.ToString(), new CultureInfo("en-GB"));
                DateTime ReminderDate   =   DateTime.Parse(AmcModel.ReminderDate.ToString(), new CultureInfo("en-GB"));
                var custcord = db.Customers.Where(o => o.CustomerID == AmcModel.CustomerId).Select(o => new { o.Lattitude, o.Longitude }).FirstOrDefault();

                var CodeExists = db.Amcs.Any(u => u.AmcNo == AmcModel.AmcNo && u.AmcId != id);
                if (CodeExists)
                {
                    Danger("AMC with same AMC No. exists.", true);
                    return RedirectToAction("Edit/"+id, "AMC");
                }
                else
                {
                    Amc AmcObj = db.Amcs.Find(id);                    

                    var OldAmcStat = GetStatusName(AmcObj.AmcStatusId);
                    var NewAmcStat = GetStatusName(AmcModel.AmcStatusId);


                    AmcObj.AmcNo                =   AmcModel.AmcNo;
                    AmcObj.ContractId           =   AmcModel.ContractId;
                    AmcObj.CustomerId           =   Convert.ToInt64(AmcModel.CustomerId);

                    AmcObj.StartDate            =   StartDate;
                    AmcObj.EndDate              =   EndDate;
                    AmcObj.ReminderDate         =   ReminderDate;

                    AmcObj.ContractLevelId      =   AmcModel.ContractLevelId;
                    AmcObj.AmcStatusId          =   AmcModel.AmcStatusId;
                    AmcObj.ContractTypeId       =   AmcModel.ContractTypeId;
                    AmcObj.Lattitude            =   AmcModel.Lattitude;
                    AmcObj.Longitude            =   AmcModel.Longitude;
                    AmcObj.LocationId           =   AmcModel.LocationId;

                    AmcObj.PeriodicMaintReqrd   =   AmcModel.PerdcMaintReq;

                    AmcObj.AmcDetails           =   AmcModel.AmcDetails;
                    AmcObj.Notes                =   AmcModel.Notes;
                    AmcObj.CreatedBy            =   UserId;
                    AmcObj.LogTime              =   Today;
                    AmcObj.Ref1                 =   AmcModel.Ref1;
                    AmcObj.Ref2                 =   AmcModel.Ref2;
                    AmcObj.Ref3                 =   AmcModel.Ref3;
                    AmcObj.Ref4                 =   AmcModel.Ref4;
                    AmcObj.Ref5                 =   AmcModel.Ref5;
                    AmcObj.OpenClose            =   AmcModel.OpenClose;
                    if((AmcModel.Lattitude==null|| AmcModel.Lattitude=="") && custcord.Lattitude!="")
                    {
                        AmcObj.Lattitude = custcord.Lattitude;
                        AmcObj.Longitude = custcord.Longitude;
                        AmcModel.Lattitude = custcord.Lattitude;
                        AmcModel.Longitude = custcord.Longitude;
                    }
                    db.Entry(AmcObj).State = EntityState.Modified;
                    db.SaveChanges();

                    Int64 AmcId = AmcObj.AmcId;

                    /************************* AmcUpdations  ***************************/
                    var AmcstatusId = db.AmcUpdations.Where(a => a.TransId == AmcObj.AmcId && a.TransType == "Amc");
                    if (AmcstatusId != null)
                    {
                        db.AmcUpdations.RemoveRange(db.AmcUpdations.Where(a => a.TransId == AmcObj.AmcId && a.TransType == "Amc"));
                        db.SaveChanges();
                    }

                    AmcUpdation AmcUps = new AmcUpdation
                    {
                        TransId      =  AmcId,
                        TransType   =   "Amc",
                        CreatedBy   =   UserId,
                        CreatedDate =   Today,
                        Remarks     =   OldAmcStat + " Change To " + NewAmcStat,
                    };
                    db.AmcUpdations.Add(AmcUps);
                    db.SaveChanges();

                    Int64 AmcUpdationID = AmcUps.UpdationID;

                    /************************* Assigned To ***************************/

                    if (AmcModel.AssignedTo != null)
                    {
                        var PrevAsgn    =   db.AmcAssignedTos.Where(a => a.AmcId == AmcId && a.Status == "Assigned" && a.ChkStatus == Status.active).Select(a => a.EmployeeId).ToArray();
                        var NewUsers    =   AmcModel.AssignedTo.ToArray();

                        AmcAssignedTo NewObj = new AmcAssignedTo();

                        foreach (var arr in PrevAsgn)
                        {
                            var AssgndToId = db.AmcAssignedTos.Where(a => a.AmcId == AmcId && a.Status == "Assigned" && a.ChkStatus == Status.active && a.EmployeeId == arr).Select(a => a.AssignedToId).FirstOrDefault();
                            AmcAssignedTo tskassr = db.AmcAssignedTos.Find(AssgndToId);
                            
                            //for removed members
                            if (!NewUsers.Contains(arr))
                            {
                                tskassr.ChkStatus = Status.inactive;
                                db.Entry(tskassr).State = EntityState.Modified;
                                db.SaveChanges();

                                NewObj.AmcId        =   AmcId;
                                NewObj.EmployeeId   =   arr;
                                NewObj.Status       =   "Removed";
                                NewObj.AssignBy     =   UserId;
                                NewObj.CreatedDate  =   Convert.ToDateTime(Today).AddMilliseconds(100); ;
                                db.AmcAssignedTos.Add(NewObj);
                                db.SaveChanges();
                            }
                            //for existing members--Updating status
                            else
                            {
                                tskassr.ChkStatus = Status.inactive;
                                db.Entry(tskassr).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }

                        foreach (var arr in AmcModel.AssignedTo)
                        {
                            NewObj.AmcId        = AmcId;
                            NewObj.EmployeeId   = arr;
                            NewObj.Status       = "Assigned";
                            NewObj.AssignBy     = UserId;
                            NewObj.ChkStatus    = Status.active;
                            NewObj.CreatedDate  = Convert.ToDateTime(Today).AddMilliseconds(100);
                            db.AmcAssignedTos.Add(NewObj);
                            db.SaveChanges();
                            com.remideradd(rootdomain + "amc/myamc", arr, UserId, "amc Assined", AmcId);
                        }
                    }
                    else
                    {
                        var PrevAsgn = db.AmcAssignedTos.Where(a => a.AmcId == AmcId && a.Status == "Assigned" && a.ChkStatus == Status.active);
                        db.AmcAssignedTos.RemoveRange(PrevAsgn);
                        db.SaveChanges();

                    }
                    /**************************** Assigned Team ***************************/
                    var AssgnTeams = db.AmcAssignedTeams.Where(a => a.AmcId == id);
                    if (AssgnTeams != null)
                    {
                        db.AmcAssignedTeams.RemoveRange(db.AmcAssignedTeams.Where(a => a.AmcId == id));
                        db.SaveChanges();
                    }

                    if (AmcModel.AssignTypeAll != null)
                    {
                        AmcAssignedTeam TeamObj = new AmcAssignedTeam();
                        foreach (var arr in AmcModel.AssignTypeAll)
                        {
                            TeamObj.AmcId   = AmcId;
                            TeamObj.TeamId  = arr;
                            db.AmcAssignedTeams.Add(TeamObj);
                            db.SaveChanges();
                        }
                    }

                    //***********Delete from Table Contacts
                    var ContRltns = db.ContactRelation.Where(a => a.RelationID == id && a.RelationType == 4);

                    if (ContRltns != null)
                    {
                        foreach (var row in ContRltns)
                        {
                            //***********Contacts
                            db.Contacts.RemoveRange(db.Contacts.Where(a => a.ContactID == row.ContactID));

                            //***********Mobiles
                            db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == row.ContactID));
                        }

                        //***********ContactRelations
                        db.ContactRelation.RemoveRange(db.ContactRelation.Where(a => a.RelationID == id && a.RelationType == 4));
                        db.SaveChanges();
                    }

                    //To Add New Contacts
                    if (AmcModel.LstContacts != null && AmcModel.LstContacts.Count > 0)
                    {
                        AddContact(AmcModel, AmcId);
                    }

                    //***********Delete from table Amc Documents
                    // For Saving Amc Documents
                    if (AmcModel.LstAmcDocument != null && AmcModel.LstAmcDocument.Count > 0)
                    {
                        AddDocuments(AmcModel, AmcId, Today, "Edit");
                    }
                    //** addreminder
                  
                        var PMaintDtls = db.PeriodicMaintenances.Where(a => a.AmcId == id).FirstOrDefault();

                    if (PMaintDtls != null)
                    {
                        //***********Delete from table PeriodicMaintenanceDetails
                        var PMaintDetails = db.PeriodicMaintenanceDetails.Where(a => a.PeriodicMaintenanceId == PMaintDtls.PeriodicMaintenanceId).FirstOrDefault();
                        if (PMaintDetails != null)
                        {
                            var p = db.ProTasks.Where(o => o.VModId == id && o.TaskName.Contains("AMC - Periodic Maintenance") && o.OpenClose == null).ToList();
                            
                            if (p != null)
                            {
                                
                                foreach (var d in p)
                                {
                                    bool deletable = true;
                                    var exist = db.TaskAssigneds.Any(o => o.ProTaskId ==d.ProTaskId);
                                    if (exist)
                                    {
                                        deletable = false;
                                    }

                                    if (deletable)
                                    {
                                        db.ProTasks.RemoveRange(db.ProTasks.Where(o => o.VModId == id && o.TaskName.Contains("AMC - Periodic Maintenance") && o.OpenClose == null));
                                        db.SaveChanges();
                                    }
                                   
                                }
                            }

                            db.PeriodicMaintenanceDetails.RemoveRange(db.PeriodicMaintenanceDetails.Where(a => a.PeriodicMaintenanceId == PMaintDtls.PeriodicMaintenanceId));
                            db.SaveChanges();
                        }

                        //***********Delete from table PeriodicMaintenance
                        PeriodicMaintenance PMaintObj = db.PeriodicMaintenances.Find(PMaintDtls.PeriodicMaintenanceId);
                        db.PeriodicMaintenances.Remove(PMaintObj);

                        db.SaveChanges();
                    }

                    //For saving Periodic Maintenance Details
                    if (AmcModel.PerdcMaintReq == true)
                    {                      
                        AddPeriodicMaintenance(AmcModel, AmcId, Today);
                    }

                    /**************************** Reminders ********************************/
                    db.Reminderss.RemoveRange(db.Reminderss.Where(a => a.Reference == id && a.Type == "AMC"));
                    db.SaveChanges();
                    
                    long StatusId = db.AmcStatuss.Where(a => a.StatusName == "Closed").Select(a => a.AmcStatusId).FirstOrDefault();
                    
                    //If Status is closed, no need to insert data into Reminderss
                    if (AmcModel.AmcStatusId != StatusId)
                    {


                        //Add Reminder For Reminder Date
                        db.Reminderss.Add(new Reminderss
                        {
                            CreatedBy   =   User.Identity.GetUserId(),
                            Note        =   "AMC - " + AmcModel.AmcNo,
                            RDate       =   ReminderDate,
                            CreatedDate =   Today,
                            Reference   =   AmcId,
                            Type        =   "AMC",
                            actionurl   =   "AMC/Edit/",
                            RequestBy   =   UserId,
                            Status      =   0,
                            RStatus     =   "Open"
                        });
                        db.SaveChanges();                      
                    }

                    com.addlog(LogTypes.Updated, UserId, "AMC", "AMCs", findip(), AmcObj.AmcId, "Amcs Updated Successfully");


                    Success("Successfully Updated AMC Details.", true);
                    return RedirectToAction("Index", "AMC");
                }
            }
            else
            {
                Danger("Failed to add AMC", true);
                return RedirectToAction("Edit/" + id, "AMC");
            }
        }

        // Delete GET
        [HttpGet]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Amc Obj = db.Amcs.Find(id);

            if (Obj == null)
            {
                return NotFound();
            }
            else
            {
                return PartialView(Obj);
            }
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteAction(long id)
        {
            bool stat = false;
            string msg;


                var chk = DeleteEntry(id);

                if (chk == true)
                {
                    stat = true;
                    msg = "Successfully deleted AMC.";
                }
                else
                {
                    stat = false;
                    msg = "Looks like something went wrong. Please check your form.";
                }
            
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //Function to check Whether Amc is used in any other transactions

        //Function to delete all the transactions
        [HttpPost]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;

            foreach (var arr in bill)
            {
                var chk = (DeleteEntry(arr) == true) ? count++ : count;
            }

            Success("Deleted " + count + " AMC Details..", true);
            return RedirectToAction("Index", "AMC");
        }

        //Function To Delete Each Entry
        private Boolean DeleteEntry(long AmcId)
        {
            int i = 0;

            //***********Delete from table Amc Documents
            var AmcDocs = db.AmcDocuments.Where(a => a.TransId == AmcId && a.TransType != "PeriodicMaintenance").ToList();
            if (AmcDocs != null)
            {
                db.AmcDocuments.RemoveRange(db.AmcDocuments.Where(a => a.TransId == AmcId && a.TransType != "PeriodicMaintenance"));
                db.SaveChanges();

                foreach (var row in AmcDocs)
                {
                    //To remove the attached file from folder
                    string FullPath = LegacyWeb.MapPath("~/uploads/AmcDocuments/" + row.FileName);

                    if (System.IO.File.Exists(FullPath))
                    {
                        System.IO.File.Delete(FullPath);
                    }

                    if(row.TransType == "Amc")
                    {
                        //To remove resize and thumb files from folder
                        string ResizePath = LegacyWeb.MapPath("~/uploads/AmcDocuments/resize_" + row.FileName);

                        if (System.IO.File.Exists(ResizePath))
                        {
                            System.IO.File.Delete(ResizePath);
                        }

                        string ThumbPath = LegacyWeb.MapPath("~/uploads/AmcDocuments/thumb_" + row.FileName);

                        if (System.IO.File.Exists(ThumbPath))
                        {
                            System.IO.File.Delete(ThumbPath);
                        }
                    }
                }
            }           

            //***********Delete from table AmcAssignedTos
            var AssgnTos = db.AmcAssignedTos.Where(a => a.AmcId == AmcId);
            if (AssgnTos != null)
            {
                db.AmcAssignedTos.RemoveRange(db.AmcAssignedTos.Where(a => a.AmcId == AmcId));
                db.SaveChanges();
            }

            //***********Delete from table AmcAssignedTeams
            var AsgnTeams = db.AmcAssignedTeams.Where(a => a.AmcId == AmcId);
            if (AsgnTeams != null)
            {
                db.AmcAssignedTeams.RemoveRange(db.AmcAssignedTeams.Where(a => a.AmcId == AmcId));
                db.SaveChanges();
            }

            //***********Delete from table Contacts
            var ContRltns = db.ContactRelation.Where(a => a.RelationID == AmcId && a.RelationType == 4);

            if (ContRltns != null)
            {
                foreach (var row in ContRltns)
                {
                    //***********Contacts
                    db.Contacts.RemoveRange(db.Contacts.Where(a => a.ContactID == row.ContactID));

                    //***********Mobiles
                    db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == row.ContactID));
                }

                //***********ContactRelations
                db.ContactRelation.RemoveRange(db.ContactRelation.Where(a => a.RelationID == AmcId && a.RelationType == 4));
                db.SaveChanges();
            }

            //***************************Periodic Maintenance
            var PMaintDtls = db.PeriodicMaintenances.Where(a => a.AmcId == AmcId).FirstOrDefault();

            if (PMaintDtls != null)
            {
                //***********Delete from table PeriodicMaintenanceDetails
                var PMaintDetails = db.PeriodicMaintenanceDetails.Where(a => a.PeriodicMaintenanceId == PMaintDtls.PeriodicMaintenanceId).FirstOrDefault();
                if (PMaintDetails != null)
                {
                    db.PeriodicMaintenanceDetails.RemoveRange(db.PeriodicMaintenanceDetails.Where(a => a.PeriodicMaintenanceId == PMaintDtls.PeriodicMaintenanceId));
                }

                //***********Delete from table PeriodicMaintenance
                PeriodicMaintenance PMaintObj = db.PeriodicMaintenances.Find(PMaintDtls.PeriodicMaintenanceId);
                db.PeriodicMaintenances.Remove(PMaintObj);
            }

            //********************Reminders
            var Reminds = db.Reminderss.Where(a => a.Reference == AmcId && a.Type == "AMC").FirstOrDefault();
            
            if (Reminds != null)
                db.Reminderss.Remove(Reminds);

            //********************Delete from table AmcUpdations
            var AmcUpdates = db.AmcUpdations.Where(a => a.TransId == AmcId && a.TransType != "PeriodicMaintenance");
            if (AmcUpdates != null)
            {
                db.AmcUpdations.RemoveRange(db.AmcUpdations.Where(a => a.TransId == AmcId && a.TransType != "PeriodicMaintenance"));
                db.SaveChanges();
            }

            //********************Delete from table AmcRemarks
            var AmcRemarks = db.AmcRemarks.Where(a => a.TransId == AmcId && a.TransType == "Amc");
            if (AmcRemarks != null)
            {
                db.AmcRemarks.RemoveRange(db.AmcRemarks.Where(a => a.TransId == AmcId && a.TransType == "Amc"));
                db.SaveChanges();
            }
           
            //***********Delete from table Amc
            Amc MastObj = db.Amcs.Find(AmcId);
            db.Amcs.Remove(MastObj);
            db.SaveChanges();

            return true;
        }

        //Contracts DropDown
        public JsonResult SearchAmcContracts(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.AmcContracts
                                  where (q == null || a.ContractName.ToLower().Contains(q.ToLower()) || a.ContractName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.ContractName,
                                      id = a.ContractId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.AmcContracts
                                  select new SelectFormat
                                  {
                                      text = a.ContractName,
                                      id = a.ContractId
                                  }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //Contract Type DropDown
        public JsonResult SearchAmcContractTypes(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.AmcContractTypes
                                  where (q == null || a.Type.ToLower().Contains(q.ToLower()) || a.Type.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.Type,
                                      id = a.TypeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.AmcContractTypes
                                  select new SelectFormat
                                  {
                                      text = a.Type,
                                      id = a.TypeId
                                  }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //Contract Status DropDown
        public JsonResult SearchContractStatus(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var userid = User.Identity.GetUserId();
            var chkUserIsEmp = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            long[] agnstat = new long[] { };
            var chkteam = db.Teams.Where(a => a.TeamLead == chkUserIsEmp).Select(a => a.TeamId).ToList();
            var members = (from a in db.TeamMembers
                           where a.EmployeeId == chkUserIsEmp
                           select new
                           {
                               a.TeamId
                           }).ToList();

            var allteamid = chkteam.Union(members.Select(a => a.TeamId));

            if (allteamid == null || allteamid.Count() == 0)
            {
                agnstat = null;
            }
            else
            {

                agnstat = db.TeamAmcStatus.Where(a => allteamid.Contains(a.TeamId)).Select(a => a.amcStatusId).Distinct().ToArray();
            }




            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AmcStatuss.Where(p => (p.StatusName.ToLower().Contains(q.ToLower()) || p.StatusName.Contains(q)) && agnstat.Contains(p.AmcStatusId))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.StatusName, //each json object will have 
                                      id = b.AmcStatusId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {                 
                serialisedJson = db.AmcStatuss.Where( p=>agnstat.Contains(p.AmcStatusId)).Select(b => new SelectFormat
                {
                    text = b.StatusName, //each json object will have 
                    id = b.AmcStatusId
                }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //Location DropDown
        public JsonResult SearchLocation(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.LocationNames.Where(p => p.Location.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Location, //each json object will have 
                                      id = b.LocationId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.LocationNames.Select(b => new SelectFormat
                {
                    text = b.Location, //each json object will have 
                    id = b.LocationId
                }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //AMC No. DropDown
        public JsonResult SearchAmcNo(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {   
                serialisedJson = (from a in db.Amcs
                                  join b in db.Customers on a.CustomerId equals b.CustomerID 
                                  where (a.AmcNo.ToString().ToLower().Contains(q.ToLower()) || a.AmcNo.ToString().Contains(q))
                                  select new SelectFormat
                                 {
                                     text   =   a.AmcNo.ToString() + " - "+b.CustomerName,
                                     id     =   a.AmcId
                                 }).OrderBy(b => b.text).ToList();
            }
            else
            {                
                serialisedJson = (from a in db.Amcs
                                  join b in db.Customers on a.CustomerId equals b.CustomerID
                                  select new SelectFormat
                                  {
                                      text  =   a.AmcNo.ToString() + " - " + b.CustomerName,
                                      id    =   a.AmcId
                                  }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //Status-->GET(From MyAmc)
        public ActionResult AddStatusUpdate(long id)
        {
            var ViewModel = new StatusUpdateViewModel

            {
                TransId = id,
                TransType = "Amc",
            };

            
            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            var UserId = User.Identity.GetUserId();           
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            ViewBag.Dropdowns = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                           }, "Value", "Text", 1);
            
            return PartialView(ViewModel);
        }

        //POST--->Add AMC Status
        [HttpPost]
        public JsonResult AddStatusUpdate(StatusUpdateViewModel ViewModel)
        {
            string msg;
            bool stat = false;
            string lat = "";
            string log = "";
            lat = Request.Form["lat"];
            log = Request.Form["log"];

            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var today = System.DateTime.Now;

                Int64 AmcId = ViewModel.TransId;

                var AmcStatusId =   db.Amcs.Where(a => a.AmcId == AmcId).Select(a => a.AmcStatusId).FirstOrDefault();
                var OldStatus   =   GetStatusName(AmcStatusId);
                var NewStatus   =   GetStatusName(ViewModel.AmcStatusId);

                var AmcstatusId = db.AmcUpdations.Where(a => a.TransId == AmcId && a.TransType == "MyAmc");
                if (AmcstatusId != null)
                {
                    db.AmcUpdations.RemoveRange(db.AmcUpdations.Where(a => a.TransId == AmcId && a.TransType == "MyAmc"));
                    db.SaveChanges();
                }

                AmcUpdation AmcUps = new AmcUpdation
                {
                    TransId     =   AmcId,
                    TransType   =   "Amc",
                    CreatedBy   =   UserId,
                    CreatedDate =   today,
                    Remarks     =   OldStatus + " Change To " + NewStatus,
                };
                db.AmcUpdations.Add(AmcUps);
                db.SaveChanges();

                Int64 AmcUpdId = AmcUps.UpdationID;

                var RemarkInfo = new AmcRemark
                {
                    CreatedDate =   today,
                    StatusID    =   ViewModel.AmcStatusId,
                    Remark      =   ViewModel.Remark+" " + lat + ", " + log,
                    AddedUser   =   UserId,
                    TransId     =   AmcId,
                    TransType   =   "Amc",
                    UpdationID  =   AmcUpdId
                };
                db.AmcRemarks.Add(RemarkInfo);
                db.SaveChanges();

                //To Update Status and LogTime in Amc Table
                Amc AmcObj = db.Amcs.Find(AmcId);

                AmcObj.AmcStatusId  = ViewModel.AmcStatusId;
                AmcObj.LogTime      = today;

                db.Entry(AmcObj).State = EntityState.Modified;
                db.SaveChanges();

                // fileupload
                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {
                    string path = LegacyWeb.MapPath("~/uploads/AmcDocuments/");
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);

                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {
                            var fileCount = db.AmcDocuments.Select(a => a.DocumentId).AsEnumerable().DefaultIfEmpty(0).Max();
                            var fileName = Path.GetFileName(file.FileName);

                            String extension = Path.GetExtension(fileName);

                            String newName = fileCount + extension;
                            string newFName = fileCount + extension;
                            string newFileName = fileCount + extension;
                            var FStatus = Status.active;
                            var thumbName = "";
                            var resizeName = "";
                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                            {
                                thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), thumbName);

                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), resizeName);
                                newFName = "resize_" + newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";
                            }
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), newName);
                            file.SaveAs(newName);

                            var AmcImage = new AmcDocument
                            {
                                TransId         =   AmcId,
                                TransType       =   "MyAmc",
                                FileName        =   newFileName,//Path.GetFileName(file.FileName),
                                Status          =   FStatus,
                                CreatedDate     =   today,
                               
                            };
                            db.AmcDocuments.Add(AmcImage);
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
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), resizeName);
                                    lgimg.Save(resizeName);
                                }

                            }
                        }
                    }
                }

                if (ViewModel.AmcStatusId != null)
                {
                    Amc Amcs = db.Amcs.Find(AmcId);
                  
                    var pflow = db.AmcProcessFlows.Where(a => a.AmcStatus == ViewModel.AmcStatusId).FirstOrDefault();
                   
                    if (pflow != null)
                    {
                        if (pflow.RemoveUpdateUser == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();
                            
                            if (UserEmp != null)
                            {
                                AmcAssignedTo NewAssgnTo = new AmcAssignedTo();
                                var PrevAssgnTo = db.AmcAssignedTos.Where(a => a.AmcId == Amcs.AmcId && a.Status == "Assigned" && a.EmployeeId == UserEmp && a.ChkStatus == Status.active).ToList();
                                if (PrevAssgnTo != null)
                                {
                                    foreach (var arr in PrevAssgnTo)
                                    {
                                        AmcAssignedTo Obj1 = db.AmcAssignedTos.Find(arr.AssignedToId);
                                        Obj1.ChkStatus = Status.inactive;
                                        db.Entry(Obj1).State = EntityState.Modified;
                                        db.SaveChanges();

                                        NewAssgnTo.AmcId        =   AmcId;
                                        NewAssgnTo.EmployeeId   =   arr.EmployeeId;
                                        NewAssgnTo.Status       =   "Removed";
                                        NewAssgnTo.AssignBy     =   UserId;
                                        NewAssgnTo.CreatedDate  =   Convert.ToDateTime(today).AddMilliseconds(100); ;
                                        db.AmcAssignedTos.Add(NewAssgnTo);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        if (pflow.RemoveUpdateUserTeams == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();


                            var team = db.Teams.Where(a => a.TeamLead == UserEmp).Select(a => a.TeamId).Distinct().ToList();
                            var teams = db.TeamMembers.Where(a => a.EmployeeId == UserEmp).Select(a => a.TeamId).Distinct().ToList();
                            var fulldata = team.Union(teams);
                            ////team lead as user
                            var AmcTeam = (from a in db.Teams
                                           where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || a.TeamLead != UserEmp)
                                           //where a.TeamLead == UserEmp
                                           select new
                                           {
                                               EmployeeId = a.TeamLead
                                           }).ToArray();
                            ////team mebers 
                            var AmcMem = (from a in db.Teams
                                          join b in db.TeamMembers on a.TeamId equals b.TeamId
                                          where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || b.EmployeeId != UserEmp)
                                          select new
                                          {
                                              b.EmployeeId
                                          }).ToArray();

                            var emp = AmcTeam.Union(AmcMem).Select(a => a.EmployeeId).ToList();
                            var AmcAssgn = db.AmcAssignedTos.Where(a => a.AmcId == AmcId && emp.Contains(a.EmployeeId) && a.Status == "Assigned" && a.ChkStatus == Status.active).ToList();

                            AmcAssignedTo NewObj = new AmcAssignedTo();
                            if (AmcAssgn != null)
                            {
                                foreach (var arr in AmcAssgn)
                                {
                                    AmcAssignedTo PrevObj = db.AmcAssignedTos.Find(arr.AssignedToId);
                                    PrevObj.ChkStatus = Status.inactive;
                                    db.Entry(PrevObj).State = EntityState.Modified;
                                    db.SaveChanges();

                                    NewObj.AmcId        =   AmcId;
                                    NewObj.EmployeeId   =   arr.EmployeeId;
                                    NewObj.Status       =   "Removed";
                                    NewObj.AssignBy     =   UserId;
                                    NewObj.CreatedDate  =   Convert.ToDateTime(today).AddMilliseconds(100); ;
                                    db.AmcAssignedTos.Add(NewObj);
                                    db.SaveChanges();
                                }
                            }
                        }

                        // process flow members assigned
                        var chkassgn = db.AmcAssignedTos.Where(a => a.AmcId == AmcId && a.Status == "Assigned" && a.ChkStatus == Status.active).Select(a => a.EmployeeId).ToList();

                        var pfmembers = db.AmcProcessFlowAssignUsers.Where(a => a.AmcProcessFlowId == pflow.AmcProcessFlowId).Select(a => a.EmployeeId).ToList();

                        AmcAssignedTo Obj = new AmcAssignedTo();
                        foreach (var arr in pfmembers)
                        {
                            if (!chkassgn.Contains(arr))
                            {
                                Obj.AmcId        =   AmcId;
                                Obj.EmployeeId   =   arr;
                                Obj.Status       =   "Assigned";
                                Obj.AssignBy     =   UserId;
                                Obj.ChkStatus    =   Status.active;
                                Obj.CreatedDate  =   Convert.ToDateTime(today).AddMilliseconds(100);
                                db.AmcAssignedTos.Add(Obj);
                                db.SaveChanges();
                                com.remideradd(rootdomain + "amc/myamc", arr, UserId, "amc Assined", AmcId);
                            }
                        }
                    }
                }

                msg = "Remark added successfully.";
                stat = true;

                com.addlog(LogTypes.Created, UserId, "AMC", "AmcRemarks", findip(), AmcId, "Remark Added Successfully");
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg} };
            }
        }

        public string GetStatusName(long? Id)
        {
            string Name = db.AmcStatuss.Where(a => a.AmcStatusId == Id).Select(a => a.StatusName).FirstOrDefault();
            return Name;
        }

        /***************************** Add Contacts *******************************/
        public int AddContact(AMCViewModel AmcViewModel, long AmcId)
        {
            Int64 ContactId, ContctGrpId;
            string MobNo = null;

            if (AmcViewModel.LstContacts != null && AmcViewModel.LstContacts.Count > 0)
            {
                //For fetching Contact Group ID
                Int64 ContactGrp = db.ContactGroups.Where(a => a.Name == "Amc").Select(a => a.ContactGroupID).FirstOrDefault();

                if (ContactGrp != 0)
                    ContctGrpId = ContactGrp;
                else
                {
                    var con = new ContactGroup
                    {
                        Name        =   "Amc",
                        Parent      =   1,
                        Editable    =   choice.No
                    };

                    db.ContactGroups.Add(con);
                    db.SaveChanges();
                    ContctGrpId = con.ContactGroupID;
                }

                //Saving
                foreach (var item in AmcViewModel.LstContacts)
                {
                    if (item.Mobile != null)
                        MobNo = item.Mobile.TrimStart(new Char[] { '0' });

                    if (item.FirstName != null && item.FirstName != "")
                    {
                        var contact = new Contact
                        {
                            Address         =   item.Name,
                            Country         =   item.Country,
                            FirstName       =   item.FirstName,
                            LastName        =   item.LastName,
                            Name            =   item.FirstName + " " + item.LastName,

                            TypeOfContact   =   item.TypeOfContact,
                            Mobile          =   MobNo,
                            Phone           =   item.Phone,
                            EmailId         =   item.EmailId,
                            Website         =   item.Website,
                            Group           =   ContctGrpId,
                            Status          =   Status.active,
                            CountryID       =   item.CountryID,
                            ContactTypeID   =   item.ContactTypeID
                        };
                        db.Contacts.Add(contact);
                        db.SaveChanges();

                        ContactId = contact.ContactID;

                        var mob = new Mobile
                        {
                            Contact     =   ContactId,
                            MobileNum   =   item.Mobile,
                            Name        =   item.FirstName + " " + item.LastName
                        };
                        db.Mobiles.Add(mob);
                        db.SaveChanges();

                        ContactRelation Relation = new ContactRelation();
                        Relation.ContactID      =   ContactId;
                        Relation.RelationType   =   (int)ContctRelation.Amc;//for Amc
                        Relation.RelationID     =   AmcId;
                        db.ContactRelation.Add(Relation);
                        db.SaveChanges();
                    }
                }
            }
            return db.SaveChanges();
        }
        public int AddDocumentsdocadd(amcdocattach AmcViewModel, long AmcId, DateTime Today, string Mode)
        {
            var UserId = User.Identity.GetUserId();

            if (AmcViewModel.AmcDocuments != null && AmcViewModel.AmcDocuments.Count > 0)
            {
                var fileName = "";
                int i = 0;

                foreach (var item in AmcViewModel.AmcDocuments)
                {
                    //In Create Mode and when FileName changes in Edit Mode
                    if (item.FileName != null)
                    {
                        //Files upload
                        IFormFile file = item.FileName;
                        fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);

                        var uploadUrl = LegacyWeb.MapPath("~/uploads/AmcDocuments/");

                        if (!System.IO.Directory.Exists(uploadUrl))
                            System.IO.Directory.CreateDirectory(uploadUrl);
                        file.SaveAs(Path.Combine(uploadUrl, fileName));

                        //To remove the previously attached file from folder
                        if (item.FileNameAmc != null)
                        {
                            string FullPath = LegacyWeb.MapPath("~/uploads/AmcDocuments/" + item.FileNameAmc);

                            if (System.IO.File.Exists(FullPath))
                            {
                                System.IO.File.Delete(FullPath);
                            }
                        }
                    }
                    //If already file name exists(Edit Mode)
                    else
                    {
                        fileName = item.FileNameAmc;
                    }
                    i++;

                    AmcDocument Documents = new AmcDocument
                    {
                        TransId = AmcId,
                        DocumentTypeID = item.DocumentTypeID,
                        Expiry = item.Expiry,
                        Notes = item.Notes,
                        TransType = item.TransType,
                        FileName = fileName,
                        Status = Status.active,
                        CreatedDate = Today
                    };
                    db.AmcDocuments.Add(Documents);
                    db.SaveChanges();
                }
            }
            return db.SaveChanges();

  
            com.addlog(LogTypes.Updated, UserId, "amc document", "amc document", findip(), AmcId);
        }

        /*********************************** File Upload **********************************/
        public int AddDocuments(AMCViewModel AmcViewModel, long AmcId, DateTime Today, string Mode)
        {
            var UserId = User.Identity.GetUserId();

            if (AmcViewModel.LstAmcDocument != null && AmcViewModel.LstAmcDocument.Count > 0)
            {
                var fileName = "";
                int i = 0;
                int c = 0;
                foreach (var item in AmcViewModel.LstAmcDocument)
                {
                    var amcdoc = db.AmcDocuments.Where(o => o.TransId == AmcId && o.FileName == item.FileNameAmc).FirstOrDefault();

                    if (item.FileName != null)
                    {
                          if (amcdoc != null)
                        {
                            db.AmcDocuments.RemoveRange(db.AmcDocuments.Where(o => o.TransId == AmcId && o.FileName == item.FileNameAmc));
                            db.SaveChanges();
                            string FullPath = LegacyWeb.MapPath("~/uploads/AmcDocuments/" + item.FileNameAmc);

                            if (System.IO.File.Exists(FullPath))
                            {
                                System.IO.File.Delete(FullPath);
                            }
                        }
                    }
                    amcdoc = db.AmcDocuments.Where(o => o.TransId == AmcId && o.FileName == item.FileNameAmc).FirstOrDefault();

                    //In Create Mode and when FileName changes in Edit Mode
                    if (item.FileName != null)
                    {
                        if (amcdoc == null)
                        {
                            //Files upload
                            IFormFile file = item.FileName;
                            fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);

                            var uploadUrl = LegacyWeb.MapPath("~/uploads/AmcDocuments/");

                            if (!System.IO.Directory.Exists(uploadUrl))
                                System.IO.Directory.CreateDirectory(uploadUrl);
                            file.SaveAs(Path.Combine(uploadUrl, fileName));

                            //To remove the previously attached file from folder
                            if (item.FileNameAmc != null)
                            {
                                string FullPath = LegacyWeb.MapPath("~/uploads/AmcDocuments/" + item.FileNameAmc);

                                if (System.IO.File.Exists(FullPath))
                                {
                                    System.IO.File.Delete(FullPath);
                                }
                            }


                            //If already file name exists(Edit Mode)
                            else
                            {
                                fileName = item.FileNameAmc;
                            }
                            i++;
                            if(fileName==null)
                            {
                                
                                fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                              

                                if (!System.IO.Directory.Exists(uploadUrl))
                                    System.IO.Directory.CreateDirectory(uploadUrl);
                                file.SaveAs(Path.Combine(uploadUrl, fileName));
                            }
                            AmcDocument Documents = new AmcDocument
                            {
                                TransId = AmcId,
                                DocumentTypeID = item.DocumentTypeID,
                                Expiry = item.Expiry,
                                Notes = item.Notes,
                                TransType = item.TransType,
                                FileName = fileName,
                                Status = Status.active,
                                CreatedDate = Today
                            };
                            db.AmcDocuments.Add(Documents);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        amcdoc.DocumentTypeID = item.DocumentTypeID;
                        amcdoc.Expiry = item.Expiry;
                        amcdoc.Notes = item.Notes;
                        amcdoc.TransType = item.TransType;

                        amcdoc.CreatedDate = Today;
                        db.Entry(amcdoc).State = EntityState.Modified;
                        db.SaveChanges();


                    }
                }
            }
            return db.SaveChanges();
            com.addlog(LogTypes.Updated, UserId, "amc document", "amc document", findip(), AmcId);
        }

        /***************************** Periodic Maintenance ***************************/
        public int AddPeriodicMaintenance(AMCViewModel AmcViewModel, long AmcId, DateTime Today)
        {
            long? NoOfMaint = 0;

            if (AmcViewModel.NoOfPMaint != null && AmcViewModel.LstPerdcMaint == null)
                NoOfMaint = AmcViewModel.NoOfPMaint;
            else if(AmcViewModel.LstPerdcMaint != null)
                NoOfMaint = AmcViewModel.LstPerdcMaint.Count;

            PeriodicMaintenance MasterObj = new PeriodicMaintenance();           

            MasterObj.PeriodicMaintenanceNo =   GetPMaintNo();
            MasterObj.AmcId                 =   AmcId;
            MasterObj.NoOfPMaintenance      =   NoOfMaint;
            MasterObj.CreatedDate           =   Today;
            MasterObj.LogTime               =   Today;

            db.PeriodicMaintenances.Add(MasterObj);
            db.SaveChanges();

            Int64 PerdcMainId = MasterObj.PeriodicMaintenanceId;

            if (AmcViewModel.LstPerdcMaint != null && AmcViewModel.LstPerdcMaint.Count > 0)
            {
                foreach (var row in AmcViewModel.LstPerdcMaint)
                {
                    if (row.PDate != null)
                    {
                        PeriodicMaintenanceDetail DetObj = new PeriodicMaintenanceDetail();
                        DetObj.PeriodicMaintenanceId = PerdcMainId;
                        DetObj.PDate        =   row.PDate;
                        DetObj.Notes        =   row.Notes;
                        DetObj.CreatedDate  =   Today;
                        DetObj.LogTime      =   Today;

                        db.PeriodicMaintenanceDetails.Add(DetObj);
                        db.SaveChanges();
                        long perdetid = DetObj.PeriodicMaintDetailsId;
                        var exist = db.ProTasks.Any(o => o.VManuId == perdetid);//o.StartDate == row.PDate && o.TaskName.Contains("AMC - Periodic Maintenance"));
                        if (!exist && AmcViewModel.OpenClose==0)
                        {
                            var existpro = db.ProTasks.Any(o => o.VModId == AmcId);//o.StartDate == row.PDate && o.TaskName.Contains("AMC - Periodic Maintenance"));

                            var UserId = User.Identity.GetUserId();
                            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                            var today = Convert.ToDateTime(System.DateTime.Now);
                            string taskname = "AMC - Periodic Maintenance " + " Amc No:" + AmcViewModel.AmcNo + " Customer :" + db.Customers.Find(AmcViewModel.CustomerId).CustomerName + " Date : " + row.PDate;
                            var taskexists = db.ProTasks.Any(o => o.TaskName == taskname);
                            if (!taskexists)
                            {
                                
                                ProTask amctask = new ProTask();
                                amctask.TaskNo = GetProNo();
                                amctask.TaskCode = InvoiceNo();
                                amctask.TaskName = "AMC - Periodic Maintenance " + " Amc No:"+ AmcViewModel.AmcNo+" Customer :" + db.Customers.Find(AmcViewModel.CustomerId).CustomerName + " Date : " + row.PDate;
                                amctask.Location = (AmcViewModel.LocationId != null) ? db.LocationNames.Find(AmcViewModel.LocationId).Location : "";
                                amctask.StartDate = row.PDate;
                                amctask.CreatedDate = row.PDate;
                                amctask.CreatedBy = UserId;
                                amctask.Status = Status.active;
                                amctask.Lattitude = AmcViewModel.Lattitude;
                                amctask.Longitude  = AmcViewModel.Longitude;
                                amctask.Branch = BranchID;
                                amctask.logtime = row.PDate;
                                amctask.CustomerID = AmcViewModel.CustomerId;
                                amctask.VManuId = perdetid;
                                amctask.VModId = AmcId;
               
                                var tasktype = db.ProTaskTypes.Where(o => o.TypeName == "PERIODIC MAINTEANCE").Select(o => o.TaskTypeId).FirstOrDefault();
                                if (tasktype != null && tasktype != 0)
                                {
                                    amctask.TaskType = tasktype;
                                }

                                db.ProTasks.Add(amctask);
                                db.SaveChanges();
                                Int64 proId = amctask.ProTaskId;
                                ProTaskUpdation TaskUp = new ProTaskUpdation
                                {
                                    ProTaskId = proId,
                                    //Status = TKUpdateStatus.Created,
                                    CreatedBy = UserId,
                                    CreatedDate = row.PDate,
                                    //TaskTeamId = teamId
                                };
                                db.ProTaskUpdations.Add(TaskUp);
                                db.SaveChanges();
                                Int64 TaskUpdId = TaskUp.TaskUpdationID;
                                if (AmcViewModel.LstContacts != null && AmcViewModel.LstContacts.Count > 0)
                                {
                                    foreach (var item in AmcViewModel.LstContacts)
                                    {
                                        var contact = new Contact
                                        {

                                            Address = item.Name,


                                            Country = item.Country,

                                            FirstName = item.FirstName,
                                            LastName = item.LastName,
                                            Name = item.FirstName + " " + item.LastName,

                                            TypeOfContact = item.TypeOfContact,
                                            Mobile = item.Mobile,
                                            Phone = item.Phone,

                                            EmailId = item.EmailId,

                                            ContactPerson = item.Name,
                                            Website = item.Website,
                                            Group = 2,
                                            Status = Status.active,
                                            CountryID = item.CountryID,
                                            ContactTypeID = item.ContactTypeID

                                        };
                                        db.Contacts.Add(contact);
                                        db.SaveChanges();
                                        var contactId = contact.ContactID;
                                        var mob = new Mobile
                                        {
                                            Contact = contactId,
                                            MobileNum = item.Mobile,
                                            Name = item.FirstName + " " + item.LastName
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                        var mob2 = new TaskMobile
                                        {
                                            ProTaskId = proId,
                                            MobileNo = item.Mobile,
                                            Name = item.FirstName + " " + item.LastName
                                        };
                                        db.TaskMobiles.Add(mob2);
                                        db.SaveChanges();

                                        ContactRelation Relation = new ContactRelation();
                                        Relation.ContactID = contactId;
                                        Relation.RelationType = 11;//for customer
                                        Relation.RelationID = proId;
                                        db.ContactRelation.Add(Relation);
                                        db.SaveChanges();
                                    }
                                }

                            }

                        }
                 
                    
                    }
                }
            }
            return db.SaveChanges();
        }
        public ActionResult AddAmcRemark(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Amc cus = db.Amcs.Find(id);

            if (cus == null)
            {
                return NotFound();
            }
            var AmcRemarks = new AddedRemarksvm
            {
                TransactionId = cus.AmcId,
                TransactionType = "amcremarks"


            };

            return PartialView(AmcRemarks);
        }

        //Saving of Remarks
        [HttpPost]
        public JsonResult AddRemark(AddedRemarksvm amcremarks)
        {
            Int64 amcid = amcremarks.TransactionId;
            var Today = Convert.ToDateTime(System.DateTime.Now);
            if (ModelState.IsValid)
            {
                if (amcremarks.Remarks != null)
                {





                    Common com = new Common();
                    var UserId = User.Identity.GetUserId();

                    DateTime? nextdate = null;
                    TimeSpan? etime = null;
                    DateTime? et = null;
                    if (amcremarks.nextfolloupdate != null)
                    {
                         nextdate = DateTime.Parse(amcremarks.nextfolloupdate, new CultureInfo("en-GB"));
                        if ((DateTime)amcremarks.nextfolloupdatetime != null)
                        {
                            etime = ((DateTime)amcremarks.nextfolloupdatetime).TimeOfDay;

                            et = nextdate + etime;
                        }
                        else
                        {
                            et = nextdate;   
                        }
                    }

                    AddedRemarks Obj = new AddedRemarks
                    {
                        TransactionId = amcremarks.TransactionId,
                        TransactionType = "amcremarks",
                        Remarks = amcremarks.Remarks,
                        AddedUser = UserId,
                        CreatedDate = Today,
                        nextdate = nextdate,
                         nexttime=et
                    };
                    db.AddedRemarks.Add(Obj);
                    db.SaveChanges();
                 
                    AmcUpdation AmcUps = new AmcUpdation
                    {
                        TransId = amcid,
                        TransType = "Amc",
                        CreatedBy = UserId,
                        CreatedDate = Today,
                        Remarks = "",
                    };
                    db.AmcUpdations.Add(AmcUps);
                    db.SaveChanges();

                    Int64 AmcUpdId = AmcUps.UpdationID;

                

                    //To Update Status and LogTime in Amc Table
                    Amc AmcObj = db.Amcs.Find(amcid);

           
                    AmcObj.LogTime = Today;

                    db.Entry(AmcObj).State = EntityState.Modified;
                    db.SaveChanges();
                    var existsamc = db.ReminderAssigneds.Where(o => o.EntryId == AmcObj.AmcId && o.Type == "amcfolowupstilpending");
                    db.ReminderAssigneds.RemoveRange(existsamc);
                    db.SaveChanges();
                    if (1==1)
                    {

                        var dt = System.DateTime.Now.AddYears(2);
                        var amcremarsk = (from a in db.AddedRemarks
                                          where  a.TransactionType == "amcremarks"
                                          select new
                                          {
                                              a.TransactionId,
                                              a.CreatedDate,
                                              nexttime = (a.nexttime == null) ? dt : a.nexttime


                                          }


                                        ).GroupBy(r => r.TransactionId)
            .Select(g => g.OrderByDescending(r => r.CreatedDate).FirstOrDefault()); // Or .Last() if already ordered ascending

                        var curdate = System.DateTime.Now;
                        var rem = (from a in db.Amcs
                                   join b in db.AmcAssignedTos on a.AmcId equals b.AmcId
                                   join d in amcremarsk on a.AmcId equals d.TransactionId
                                   join c in db.Customers on a.CustomerId equals c.CustomerID
                                    where  EF.Functions.DateDiffHour(curdate,d.nexttime ) < 12


                                   && b.Status == "Assigned" && b.ChkStatus == Status.active

                                   select new
                                   {
                                       b.EmployeeId,
                                       a.AmcId,
                                        d.nexttime,
                                        curdate,
                                       taskname = a.AmcNo + "-" + c.CustomerName,
                                  
                                   }).Distinct().ToList();

                        if (rem.Count() > 0)
                        {
                            var pids = rem.Select(o => new
                            {
                                o.AmcId,
                   
                                o.taskname,
                     

                            }).Distinct().ToList();
                            foreach (var pid in pids)
                            {
                                string tasknote = "12 Hours AMC  Next Followups " + pid.taskname ;
                                var remexist = db.Reminders.Any(o => o.Note == tasknote && o.Reference == pid.AmcId);
                                if (!remexist)
                                {
                                    Reminder reminds = new Reminder();
                                    reminds.Reference = pid.AmcId;
                                    reminds.Note = tasknote;// "Task Still " +pid.Ref1+" <br> Task Status : " + pid.StatusName + "<br> Task name : " + pid.taskname;

                                    var rDate = System.DateTime.Now.Date;
                                    //seleted date added,for fullcalender



                                    reminds.RDate = System.DateTime.Now;
                                    reminds.Type = "/AMC/Details/" + pid.AmcId;
                                    reminds.RStatus = "Close";
                                    reminds.RequestBy = UserId;

                                    reminds.CreatedBy = UserId;
                                    reminds.Status = Status.active;
                                    reminds.CreatedDate = System.DateTime.Now;
                                    db.Reminders.Add(reminds);
                                    db.SaveChanges();
                                    long Id = reminds.ReminderId;
                                    var asseimp = rem.Where(o => o.AmcId == pid.AmcId).Select(o => o.EmployeeId).Distinct().ToList().ToArray();
                                    var myemps = asseimp.Distinct().ToList().ToArray();
                                    foreach (var arr in myemps)
                                    {

                                        var exists = db.ReminderAssigneds.Any(o => o.EntryId == pid.AmcId && o.Type == "amcfolowupstilpending" && o.EmployeeId == arr);



                                        if (!exists)
                                        {
                                            ReminderAssigned remAs = new ReminderAssigned();

                                            remAs.ReminderId = Id;
                                            remAs.EntryId = pid.AmcId;
                                            remAs.Type = "amcfolowupstilpending";
                                            remAs.EmployeeId = arr;
                                            db.ReminderAssigneds.Add(remAs);
                                            db.SaveChanges();

                                        }
                                    }
                                }

                            }
                        }

                    }

                    com.addlog(LogTypes.Created, UserId, "amcremarks", "AddedRemarks", findip(), amcid, "AMC Remarks Added Successfully..");
                    Success("Remarks added successfully...", true);
                }
            }
            else
            {
                Danger("Failed to add Remarks...", true);
            }
            return Json(new { msg = "Success", status = true });

        }
        [HttpPost]
        public ActionResult GetAllRemarksadded(long? RequisitionId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            DateTime rmdate = System.DateTime.Now.AddYears(-2);

            var v = (from a in db.AmcRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()

                     where a.TransId == RequisitionId && a.TransType == "Amc" && a.Remark != null && a.CreatedDate >= rmdate
                     orderby a.CreatedDate descending

                     select new
                     {
                         // id = a.RemarkId,
                         CreatedDate = a.CreatedDate,
                         EmpName = b.UserName,
                         Remarks = a.Remark,
                     });
                     
            var v2 = (from a in db.AddedRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()

                     where a.TransactionId == RequisitionId && a.TransactionType == "amcremarks" && a.Remarks != null && a.CreatedDate >= rmdate
                     orderby a.CreatedDate descending

                     select new
                     {
                        // id = a.RemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remarks,
                     });

            var full = v.Union(v2).Distinct();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = full.Count();
            var data = full.ToList().Select(o=> new
            {
                CreatedDate=Convert.ToDateTime(o.CreatedDate.ToString("yyyy-MM-dd HH:mm")),
               o.EmpName,
               o.Remarks

            }).Distinct().OrderByDescending(o=>o.CreatedDate);

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpPost]
        public ActionResult GetAllamcRemarksadded(long id)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            DateTime rmdate = System.DateTime.Now.AddYears(-2);

            var v = (from a in db.AmcRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.Amcs on a.TransId equals c.AmcId
                     where c.CustomerId == id && a.TransType == "Amc" && a.Remark != null && a.CreatedDate >= rmdate
                     && c.OpenClose==0
                     orderby a.CreatedDate descending

                     select new
                     {
                         // id = a.RemarkId,
                         amcname=c.AmcNo + "- "+c.AmcDetails,
                         CreatedDate = a.CreatedDate,
                         EmpName = b.UserName,
                         Remarks = a.Remark,
                     });

            var v2 = (from a in db.AddedRemarks
                      join b in db.Users on a.AddedUser equals b.Id into emp
                      from b in emp.DefaultIfEmpty()
                      join c in db.Amcs on a.TransactionId equals c.AmcId 
                      where c.CustomerId == id && a.TransactionType == "amcremarks" && a.Remarks != null && a.CreatedDate >= rmdate
                        && c.OpenClose == 0
                      orderby a.CreatedDate descending

                      select new
                      {
                          // id = a.RemarkId,
                          amcname = c.AmcNo + "- " + c.AmcDetails,
                          a.CreatedDate,
                          EmpName = b.UserName,
                          a.Remarks,
                      });

            var full = v.Union(v2).Distinct();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = full.Count();
            var data = full.ToList().Select(o => new
            {
                o.amcname,
                CreatedDate = Convert.ToDateTime(o.CreatedDate.ToString("yyyy-MM-dd HH:mm")),
                o.EmpName,
                o.Remarks

            }).Distinct().OrderBy(o => o.amcname);

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        private long GetProNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.number).FirstOrDefault();
            if ((db.ProTasks.Select(p => p.TaskNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    SENo = 1;
                }
                else
                {
                    SENo = number;
                }
            }
            else
            {
                SENo = db.ProTasks.Max(p => p.TaskNo + 1);
            }

            return SENo;
        }
        private string InvoiceNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.number).FirstOrDefault();
                if ((db.ProTasks.Select(p => p.TaskNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.ProTasks.Max(p => p.TaskNo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(SENo, billNo);
                }

            }
            return billNo;
        }
               private bool BillExist(string SENo)
        {
            var Exists = db.ProTasks.Any(c => c.TaskCode == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        //Function to Get the data into view of Status update table
        [HttpPost]
        public ActionResult GetAllStatusUpdates(long AmcId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            //Find Order Column
            var sortColumn      =   Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir   =   Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.AmcRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.AmcStatuss on a.StatusID equals c.AmcStatusId
                     where a.TransId == AmcId && a.TransType != "PeriodicMaintenance"
                     orderby a.CreatedDate descending
                     select new
                     {
                         RemarkId   =   a.RemarkId,                         
                         EmpName    =   b.UserName,                         
                         Status     =   c.StatusName,
                         StatusId   =   c.AmcStatusId,
                         AmcId      =   a.TransId,
                         a.CreatedDate,
                         a.Remark
                     }).ToList().Select(o => new
                     {
                         o.RemarkId,
                         o.CreatedDate,
                         o.EmpName,
                         o.Remark,
                         o.Status,
                         o.AmcId,
                         o.StatusId
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }       

        //GET
        public ActionResult Details(long id)
        {
            ViewAMCViewModel vmodel = new ViewAMCViewModel();
            vmodel = (from a in db.Amcs
                      join b in db.AmcContracts on a.ContractId equals b.ContractId                       
                      join d in db.Customers on a.CustomerId equals d.CustomerID
                      join c in db.AmcContractTypes on a.ContractTypeId equals c.TypeId into temp2
                      from c in temp2.DefaultIfEmpty()
                      join h in db.AmcStatuss on a.AmcStatusId equals h.AmcStatusId into temp3
                      from h in temp3.DefaultIfEmpty()
                      join i in db.LocationNames on a.LocationId equals i.LocationId into temp4
                      from i in temp4.DefaultIfEmpty()
                      join t in db.Accountss on d.Accounts equals t.AccountsID into accs
                      from t in accs.DefaultIfEmpty()
                      join s in db.Country on d.CountryID equals s.CountryID into Countrys
                      from s in Countrys.DefaultIfEmpty()
                      join v in db.PeriodicMaintenances on a.AmcId equals v.AmcId into PMaints
                      from v in PMaints.DefaultIfEmpty()
                      join u in db.Users on a.CreatedBy equals u.Id
                      where a.AmcId == id
                      select new
                      {
                          a.AmcId,
                          a.AmcNo,
                          d.CustomerName,  
                          b.ContractName,
                          d.CustomerCode,
                          d.Addres,                        
                          s.CountryName,
                          i.Location,
                          c.Type,
                          a.ContractLevelId,
                          h.StatusName,
                          a.StartDate,
                          a.EndDate,
                          u.UserName,
                          a.CreatedDate,
                          a.Ref1,
                          a.Ref2,
                          a.Ref3,
                          a.Ref4,
                          a.Ref5,
                          CustLocation = i.Location,
                          a.AmcDetails,
                          a.Notes,
                          TaxRegNo = d.TaxRegNo,
                          NoOfPMaint = (v.AmcId != null)? v.NoOfPMaintenance : 0, 
                          ldate = ((a != null) && (a.CreatedDate > a.LogTime)) ? a.CreatedDate : a.LogTime,
                      }).ToList().Select(o => new ViewAMCViewModel
                      {
                          AmcId         =   o.AmcId,
                          AmcNo         =   o.AmcNo,
                          AmcStatus     =   o.StatusName,
                          CustomerName  =   o.CustomerName,
                          ContractType  =   o.Type,
                          ContractName  =   o.ContractName,
                          Location      =   o.Location,
                          StartDate     =   o.StartDate,
                          EndDate       =   o.EndDate,
                          CreatedBy     =   o.UserName,
                          CreatedDate   =   o.CreatedDate,
                          Ref1          =   o.Ref1,
                          Ref2          =   o.Ref2,
                          Ref3          =   o.Ref3,
                          Ref4          =   o.Ref4,
                          Ref5          =   o.Ref5,
                          UpDate        =   o.ldate,
                          CustomerCode  =   o.CustomerCode,
                          Address       =   o.Addres,
                          Country       =   o.CountryName,
                          CustLocation  =   o.CustLocation,
                          TaxRegNo      =   o.TaxRegNo,
                          NoOfPMaint    =   o.NoOfPMaint,
                          AmcDetails    =   o.AmcDetails,
                          Notes         =   o.Notes,
                          ContractLevel =   Enum.GetName(typeof(TaskPriority), o.ContractLevelId),
                      }).FirstOrDefault();

            vmodel.AssignTo = (from a in db.AmcAssignedTos
                                 join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                                 from b in emp.DefaultIfEmpty()
                                 where a.AmcId == id && a.ChkStatus == Status.active && a.Status == "Assigned"
                                 select new AssignToViewModel
                                 {
                                     Empname = b.FirstName + " " + b.MiddleName + " " + b.LastName + ", ",
                                 }).Distinct().ToList();

            var AmcId = db.Amcs.Where(o => o.AmcId == id).Select(o => o.AmcId).FirstOrDefault();
          
            vmodel.AmcDocuments = (from b in db.AmcDocuments
                                    where b.TransId == id
                                    select new AmcDocumentViewModel
                                    {
                                        TransId = b.TransId,
                                        DocumentId = (from aa in db.AmcDocuments
                                                                  where
                                        aa.TransId == b.TransId && aa.FileName == b.FileName
                                                                  select new
                                                                  {
                                                                      aa.DocumentId
                                                                  }).Max(o => o.DocumentId),
                                        FileNameAmc = b.FileName,
                                    }).Distinct().ToList();


      


            return View(vmodel);
        }

        //Function to see image from details window
        public ActionResult DocView(long Id)
        {
            var Img = db.AmcDocuments.Where(a => a.DocumentId == Id).FirstOrDefault();
            AmcDocumentViewModel vmodel = new AmcDocumentViewModel();
            vmodel.FileNameAmc = Img.FileName;
            return PartialView(vmodel);
        }

        //Function to delete image from details window(GET)
        public ActionResult DeleteImage(long id)
        {
            AmcDocumentViewModel vmodel = new AmcDocumentViewModel();
            vmodel.DocumentId = id;
            return PartialView(vmodel);
        }

        //Function to delete image from details window(POST)
        public ActionResult DeleteAmcImage(AmcDocumentViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var Img = db.AmcDocuments.Where(a => a.DocumentId == vmodel.DocumentId).FirstOrDefault();
            if (Img != null)
            {
                string fullPath = LegacyWeb.MapPath("~/uploads/AmcDocuments/" + Img.FileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                //To remove resize and thumb files from folder
                string fullPaththumb = LegacyWeb.MapPath("~/uploads/AmcDocuments/" + "thumb_" + Img.FileName);
                if (System.IO.File.Exists(fullPaththumb))
                {
                    System.IO.File.Delete(fullPaththumb);
                }
                
                string ResizePath = LegacyWeb.MapPath("~/uploads/AmcDocuments/resize_" + Img.FileName);

                if (System.IO.File.Exists(ResizePath))
                {
                    System.IO.File.Delete(ResizePath);
                }
                var amndel = db.AmcDocuments.Where(a => a.TransId == Img.TransId && Img.FileName == a.FileName);
                db.AmcDocuments.RemoveRange(amndel);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();

                com.addlog(LogTypes.Deleted, UserId, "AMC", "AmcDocuments", findip(), Img.DocumentId, "Amc Image Deleted Successfully");

                stat = true;
                msg = "Successfully deleted Amc Image.";
            }
            else
            {
                stat = false;
                msg = "Amc Image Not Found..!!";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        public ActionResult GetAllRemarks(long AmcId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.AmcRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.AmcStatuss on a.StatusID equals c.AmcStatusId
                     where a.TransId == AmcId && a.TransType == "Amc"
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.RemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remark,
                         Status = c.StatusName,
                         Statusid = c.AmcStatusId,
                     }).ToList().Select(o => new
                     {
                         o.id,
                         o.CreatedDate,
                         o.EmpName,
                         o.Remark,
                         o.Status,                        
                         o.Statusid
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        //Function to list the Amcs 
        [HttpPost]
        public ActionResult GetAllAmcs(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.Amcs
                     join b in db.AmcContracts on a.ContractId equals b.ContractId into temp
                     from b in temp.DefaultIfEmpty()
                     where a.CustomerId == CustomerId
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.AmcId,
                         a.AmcNo,
                         b.ContractName,
                         a.CreatedDate,
                         a.StartDate,
                         a.EndDate
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = v.Count();
            var data = v.ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        //function to list Periodic Details in Periodic Details Tab
        [HttpPost]
        public ActionResult GetAllPeriodic(long AmcId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.PeriodicMaintenances
                     join b in db.PeriodicMaintenanceDetails
                     on a.PeriodicMaintenanceId equals b.PeriodicMaintenanceId
                     join c in db.Amcs on a.AmcId equals c.AmcId
                     join d in db.AmcStatuss on b.PeriodicMaintStatus equals d.AmcStatusId into temp1
                     from d in temp1.DefaultIfEmpty()
                     where a.AmcId == AmcId
                     orderby b.LogTime descending
                     select new
                     {
                         id = b.PeriodicMaintDetailsId,
                         b.PDate,
                         b.Notes,
                         Status = d.StatusName,
                     }).ToList().Select(o => new
                     {
                         o.id,
                         o.PDate,
                         o.Notes,
                         o.Status
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

    }
}
