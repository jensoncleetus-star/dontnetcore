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
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    public class ProTaskController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        string rootdomain;
        public ProTaskController()
        {
            db = new ApplicationDbContext();
            com = new Common();
            rootdomain = db.companys.Find(1L)?.CPWebsite;
           
        }
        [HttpGet]
        public ActionResult setopenclose(long id)
        {
            var Stat = QkSelect.List(
                new List<SelectListItem> {
               
                new SelectListItem { Value="1",Text="Close"}, new SelectListItem { Value="0",Text="Open"},},"Value","Text");
            ViewBag.openclose = Stat;
            ViewBag.protask = id;
            return PartialView();
        }
        [HttpGet]
        public ActionResult setopenclosenew(long id)
        {
            var Stat = QkSelect.List(
                new List<SelectListItem> {

                new SelectListItem { Value="1",Text="Close"}, new SelectListItem { Value="0",Text="Open"},}, "Value", "Text");
            ViewBag.openclose = Stat;
            ViewBag.protask = id;
            return PartialView();
        }
        [HttpPost]
        public ActionResult setopencloseupdatenew(long protaskid)
        {
            var us = User.Identity.GetUserId();
            var empid = db.Employees.Where(o => o.UserId == us).Select(o => o.EmployeeId).FirstOrDefault();
            var pr = db.ProTasks.Find(protaskid);
            pr.Ref1 = "ASSIGNED";
            db.Entry(pr).State = EntityState.Modified;
            db.SaveChanges();
            TaskAssigned tskass = new TaskAssigned();
                    tskass.ProTaskId = protaskid;
                    tskass.EmployeeId = empid;
                    tskass.Status = "Assigned";
                    tskass.AssignBy = us;
                    tskass.chkStatus = Status.active;
                    tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100);
                    db.TaskAssigneds.Add(tskass);
                    db.SaveChanges();


            
            bool stat = true;
            string msg = "Successfully Updated Status.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost]
        public ActionResult setopencloseupdate(int OpenClose,long protaskid)
        {
            var pr = db.ProTasks.Find(protaskid);
            pr.OpenClose = OpenClose;
            db.Entry(pr).State = EntityState.Modified;
            db.SaveChanges();
            bool stat = true;
            string msg = "Successfully Updated Status.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult mapview()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.Cust = OpAll;
            return View();
        }
        public ActionResult Gettaskmannercount(string emloyees, string manner)
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
            long[] mannersnew = { };
            long[] emloyeesnew = { };
            if (manner != "" && manner != null)
                mannersnew = manner.Split(',').Select(x => long.Parse(x)).ToArray();
            if (emloyees != "" && emloyees != null)
                emloyeesnew = emloyees.Split(',').Select(x => long.Parse(x)).ToArray();

            var olytech = (
                           from b in db.Employees 
                           join c in db.TeamMembers on b.EmployeeId equals c.EmployeeId



                           where c.TeamId == 4 
                           select new
                           {
                      b.EmployeeId

                           }).Select(o=>o.EmployeeId).ToList().ToArray();


            var v = (from a in db.ProTaskManners
                     join b in db.AssignTaskManners on a.TaskTypeId equals b.TaskMannerId
                     join c in db.Employees on b.EmployeeId equals c.EmployeeId
                     where (manner == "" ||manner==null|| mannersnew.Contains(a.TaskTypeId)) &&
                 (emloyees == null || emloyees == "" || emloyeesnew.Contains(c.EmployeeId))
                 &&
                      olytech.Contains(c.EmployeeId) 
                     group new { a.TypeName, c.FirstName, c.LastName,a.TaskTypeId } by new { b.EmployeeId } into grps
                     select new
                     {
                         idd = grps.FirstOrDefault().TaskTypeId,
                         empname =  grps.FirstOrDefault().FirstName + " " + grps.FirstOrDefault().LastName,
                         mannercount =grps.Count(),
                         taskmanner = grps.FirstOrDefault().TypeName,
                         designation = ""
                     }
                    
                     );
            var v1 = (from a in db.ProTaskManners
                     join b in db.AssignTaskManners on a.TaskTypeId equals b.TaskMannerId
                     join bb in db.ProTasks on b.ProTaskId equals bb.ProTaskId
                     join cc in db.TaskAssigneds on bb.ProTaskId equals cc.ProTaskId
                     join c in db.Employees on cc.EmployeeId equals c.EmployeeId
                      join d in db.Designations on c.DesignationID equals d.DesignationID into desig
                      from d in desig.DefaultIfEmpty()
                      where (manner == "" || manner == null || mannersnew.Contains(a.TaskTypeId)) &&
                      (emloyees == null || emloyees == "" || emloyeesnew.Contains(c.EmployeeId))
                      &&
                      olytech.Contains(c.EmployeeId) 
                      group new { a.TypeName, c.FirstName, c.LastName,d.DesignationName } by new { cc.EmployeeId, a.TaskTypeId } into grps
                      select new
                      {
                          idd = 1,
                          empname = grps.FirstOrDefault().FirstName + " " + grps.FirstOrDefault().LastName,
                          mannercount = grps.Count(),
                          taskmanner = grps.FirstOrDefault().TypeName,
                          designation = grps.FirstOrDefault().DesignationName
                      }
                 

                     );
            var data = v1.ToList();
            recordsTotal = data.Count();
            
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public long[] getmytaskmanners()
        {
            var usr = User.Identity.GetUserId();
            var empid = db.Employees.Where(o => o.UserId == usr).Select(o => o.EmployeeId).FirstOrDefault();
            var v1 = (from a in db.ProTaskManners
                      join b in db.AssignTaskManners on a.TaskTypeId equals b.TaskMannerId
                      join bb in db.ProTasks on b.ProTaskId equals bb.ProTaskId
                      join cc in db.TaskAssigneds on bb.ProTaskId equals cc.ProTaskId
                      join c in db.Employees on cc.EmployeeId equals c.EmployeeId
                      join d in db.Designations on c.DesignationID equals d.DesignationID into desig
                      from d in desig.DefaultIfEmpty()
                      where 
                      c.EmployeeId==empid

                    select new
                    {
                        a.TaskTypeId
                    }


                    ).Distinct();
            var data = v1.Select(o=>o.TaskTypeId).ToList().ToArray();
            return data;
        }
        public ActionResult mapviewshowemp(long? ddlEmployee,long? ddlDepartment,string From, string To,long? lastpos)
        {


            DateTime tdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            if(To=="")
            {
                tdate = System.DateTime.Now;
            }

            tdate = tdate.AddHours(23);
            tdate = tdate.AddMinutes(59);
            DateTime dateto = tdate;
            DateTime datefrom = DateTime.Parse(From, new CultureInfo("en-GB"));
            DateTime taskdatefrom = System.DateTime.Now.AddDays(-60);
            DateTime taskdateto = tdate;



            bool editprotask = User.IsInRole("Edit ProTask");
            bool editperiodic = User.IsInRole("AMC CREATE");
            var UserId = User.Identity.GetUserId();
            var EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            if (lastpos == null)
            {
                var amcopenandclose = (from a in db.LogManagers
                                       join c in db.Employees on a.User equals c.UserId

                                       where
                                      (ddlEmployee == null || ddlEmployee == 0 || c.EmployeeId == ddlEmployee) &&
     (From == "" || EF.Functions.DateDiffDay(a.LogTime, datefrom) <= 0) &&
                                    (To == "" || EF.Functions.DateDiffDay(a.LogTime, tdate) >= 0)


                                      && a.LogSection == "gpstrack"
                                       select new points
                                       {
                                           lat = a.LogTable,
                                           log = a.LogDetails,
                                           pointname = c.FirstName + " " + c.MiddleName + " " + c.LastName + " " + a.LogTime.ToString(),

                                           taskid = 0,
                                           type = "customer"

                                       }).ToList();






                return View(amcopenandclose);
            }
            else
            {
                var amcopenandclose = (from a in db.LogManagers
                                       join c in db.Employees on a.User equals c.UserId

                                       where
                                      (ddlEmployee == null || ddlEmployee == 0 || c.EmployeeId == ddlEmployee)



                                      && a.LogSection == "gpstrack"

                                       select new points
                                       {
                                           lat = a.LogTable,
                                           log = a.LogDetails,
                                           pointname = c.FirstName + " " + c.MiddleName + " " + c.LastName + " " + a.LogTime.ToString(),

                                           taskid = c.EmployeeId,
                                           type = "customer",
                                           logtime = a.LogTime

                                       }

                    ).ToList();

                var vv = amcopenandclose.GroupBy(o => o.taskid).Select(g => g.OrderByDescending(c => c.logtime).FirstOrDefault()).ToList();



                return View(vv.ToList());
            }
        }

        public ActionResult mapviewshow(string To, long? chkcustomer, long? chktask,long? chkperiodic,long? chkAmcs,string lat="",string log="")
        {

            
            DateTime  tdate = DateTime.Parse(To, new CultureInfo("en-GB"));

            tdate = tdate.AddHours(23);
            tdate = tdate.AddMinutes(59);
            DateTime dateto = tdate;
            DateTime datefrom = System.DateTime.Now.AddDays(-60);
            DateTime taskdatefrom = System.DateTime.Now.AddDays(-60);
            DateTime taskdateto = tdate;



            bool editprotask = User.IsInRole("Edit ProTask");
            bool editperiodic = User.IsInRole("AMC CREATE");
            var UserId = User.Identity.GetUserId();
            var EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            var empname = db.Employees.Find(EmpId);
            
           
               var   mypo = new points
                {
                    lat = lat,
                    log = log,
                    pointname = empname.FirstName + " " + empname.LastName,
                    type = "iam"

                };
            points mype = new points()

            {
                lat = lat,
                log = log,
                pointname = empname.FirstName + " " + empname.LastName,
                type = "iam"

            };
            
            var amcopenandclose = (from a in db.Amcs
                                   join b in db.PeriodicMaintenances on a.AmcId equals b.AmcId
                                   join c in db.Customers on a.CustomerId equals c.CustomerID

                                   where
                                  (a.Lattitude != null || c.Lattitude != null) && (a.Longitude != null || c.Longitude != null) &&
                                  (editperiodic == true)
                                  && a.StartDate <= dateto
                                  && a.OpenClose ==1

                                   select new points
                                   {
                                       lat = (a.Lattitude == null) ? c.Lattitude : a.Lattitude,
                                       log = (a.Longitude == null) ? c.Longitude : a.Longitude,
                                       pointname = "Amc No :-" + a.AmcNo.ToString() + "-" + c.CustomerName,
                                       customerid = a.CustomerId,
                                       amcid = a.AmcId,
                                       taskid = 0,
                                       type = (a.OpenClose == 0) ? "task" : "customer"

                                   }).ToList();

            var amc = (from a in db.Amcs
                     join b in db.PeriodicMaintenances on a.AmcId equals b.AmcId
                     join d in db.PeriodicMaintenanceDetails on b.PeriodicMaintenanceId equals d.PeriodicMaintenanceId
                     join c in db.Customers on a.CustomerId equals c.CustomerID
                      let assign = db.PeriodicMaintAssignedToes.Where(x => x.PeriodicMaintDtlId == d.PeriodicMaintDetailsId && x.Status == "Assigned" && x.ChkStatus == Status.active).Select(x => x.EmployeeId).ToList()

                      where d.PDate >= datefrom && d.PDate <= dateto &&
                     (a.Lattitude!=null||c.Lattitude!=null) && (a.Longitude !=null||c.Longitude !=null) &&
                     (editperiodic==true||assign.Contains(EmpId))
                     && (a.OpenClose == 0 || a.OpenClose == null)

                       select new points
                     {
                         lat =(a.Lattitude==null)?c.Lattitude:a.Lattitude,
                         log=(a.Longitude==null)?c.Longitude:a.Longitude,
                         pointname="Amc No :-"+a.AmcNo.ToString()+"-"+c.CustomerName+" "+d.PDate,
                         customerid=a.CustomerId,
                         amcid=a.AmcId,
                         taskid=0,
                         type="amc"

                     }).ToList();
            var task = (from a in db.ProTasks.Where(o=>o.Branch==1)
                        join c in db.Customers on a.CustomerID equals c.CustomerID
                        join am in db.Amcs on a.VModId equals am.AmcId into amcd
                        from am in amcd.DefaultIfEmpty()
                        let assign = db.TaskAssigneds.Where(x => x.ProTaskId == a.ProTaskId && x.Status == "Assigned" && x.chkStatus == Status.active).Select(x => x.EmployeeId).ToList()
                        
                        where a.logtime >= taskdatefrom && a.logtime <= taskdateto &&
                                (a.Lattitude != null || c.Lattitude != null) && (a.Longitude != null || c.Longitude != null) &&
                                 (editprotask == true || assign.Contains(EmpId))
                                    && (a.OpenClose == 0 || a.OpenClose == null)
                                    &&(am==null||am.OpenClose==0)
                        select new 
                        {
                            lat = (a.Lattitude == null) ? c.Lattitude : a.Lattitude,
                            log = (a.Longitude == null) ? c.Longitude : a.Longitude,
                            pointname= "Task Code :-" + a.TaskCode + "-" + a.TaskName,
                            customerid = c.CustomerID,
                            taskid = a.ProTaskId,
                            amcid = 0,
                            assign,
                            type = (a.TaskName.Contains("AMC - Periodic Maintenance") == true) ? "amc" : "task"

                        }).ToList().Select(o=>new points
                        { lat=o.lat,
                        log=o.log,
                        customerid=o.customerid,
                        taskid=o.taskid,
                        amcid=o.amcid,
                        type=o.type,


                            pointname =  o.pointname+ " " + "Task Assigend :-" + String.Join(",", db.Employees.Where(p => o.assign.Contains(p.EmployeeId)).Select(p => p.FirstName + " " + p.LastName).ToArray()) ,
                        });
            var periodictask = (from a in db.ProTasks.Where(o=>o.Branch==1)
                        join c in db.Customers on a.CustomerID equals c.CustomerID
                        let assign = db.TaskAssigneds.Where(x => x.ProTaskId == a.ProTaskId && x.Status == "Assigned" && x.chkStatus == Status.active).Select(x => x.EmployeeId).ToList()

                        where a.logtime >= taskdatefrom && a.logtime <= taskdateto &&
                                (a.Lattitude != null || c.Lattitude != null) && (a.Longitude != null || c.Longitude != null) &&
                                 (editprotask == true || assign.Contains(EmpId))
                                    && (a.OpenClose == 0 ||a.OpenClose==null)
                                    &&
                                    a.TaskName.Contains("AMC - Periodic Maintenance")

                                select new points
                        {
                            lat = (a.Lattitude == null) ? c.Lattitude : a.Lattitude,
                            log = (a.Longitude == null) ? c.Longitude : a.Longitude,
                            pointname = "Task Code :-" + a.TaskCode + "-" + a.TaskName,
                            customerid = c.CustomerID,
                            taskid=a.ProTaskId,
                            amcid=0,
                            type =  "amc" 

                        }).ToList();



            

            var taskar = task.Select(o => o.customerid).Distinct().ToArray();
            var customers = (from a in db.Customers
                             join t in db.ProTasks.Where(o=>o.Branch==1) on a.CustomerID equals t.CustomerID into tsk
                             from t in tsk.DefaultIfEmpty()
                             join am in db.Amcs on a.CustomerID equals am.CustomerId into amccus
                             from am in amccus.DefaultIfEmpty()

                             where
                             (editprotask == true || taskar.Contains(a.CustomerID))
                                 select new points
                                 {
                                     lat = (a.Lattitude==null)?(t.Lattitude==null)?am.Lattitude:t.Lattitude:a.Lattitude,
                                     log = (a.Longitude== null) ? (t.Longitude == null) ? am.Longitude : t.Longitude : a.Longitude,
                                     pointname = "Customer : "+a.CustomerName,
                                     customerid = a.CustomerID,
                                     amcid=0,
                                     taskid=0,
                                     type="customer"

                                 }).Where(o=>o.lat!=null).Distinct().ToList();
            List<points> v = new List<points>();
            if (lat != "")
            {
                v.Insert(0,mypo);
            }
            if(chkcustomer!=null)
            {
                v.AddRange(customers);
            }
            if(chktask!=null)
            {
                v.AddRange(task);

            }
            if(chkperiodic!=null)
            {
                v.AddRange(amc);
                v.AddRange(periodictask);
            }
            if(chkAmcs!=null)
            {
                v.AddRange(amcopenandclose);
                
            }
            return View(v);
        }
        public ActionResult taskcashreport()
        {
            var userid = User.Identity.GetUserId();
            var emp = db.Employees.Where(o => o.UserId == userid).FirstOrDefault();
            ViewBag.empid = emp.EmployeeId;
            ViewBag.EmpName = emp.FirstName;
            ViewBag.SalesExecutive = QkSelect.List(
                                    new List<SelectListItem>
                                    {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                    }, "Value", "Text", 0);
            return View();
        }
        [HttpPost]

        public ActionResult Gettaskcashreport(long? emmp, string From, string To)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var UserId = User.Identity.GetUserId();
            var empid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (From != "")
            {
                fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }
            if (To != "")
            {
                tdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }
            string uid = "";
            if(emmp!=0||emmp!=null)
             uid= db.Employees.Where(o => o.EmployeeId == emmp).Select(o => o.UserId).FirstOrDefault(); 
            var v = (from a in db.servicereports
                     join b in db.ProTasks.Where(o=>o.Branch==1) on a.protaskid equals b.ProTaskId
                     
                     join e in db.Users on a.createdby equals e.Id
                     join ee in db.Employees on e.Id equals ee.UserId

                     where
                         (From == "" || EF.Functions.DateDiffDay(a.starttime, fdate) <= 0)
                          && (To == "" || EF.Functions.DateDiffDay(a.starttime, tdate) >= 0)
                          && (emmp == 0 || emmp == null || a.createdby == uid)
                        select new
                        {
                            date = a.starttime,
                            task = b.TaskCode + " " + b.TaskName,
                            amount = a.amount,
                            cheque = a.chequenumber+ " "+a.bankname,
                            employee=ee.FirstName+" "+ee.MiddleName+ " "+ee.LastName

                        }).OrderByDescending(o=>o.date);

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
        [HttpPost]
        public bool setorder(long id, long? orders)
        {
            var pro = db.ProTasks.Find(id);
                pro.VTypId = orders;
            db.Entry(pro).State = EntityState.Modified;
            db.SaveChanges();
            return true;
        }
        // GET: ProTask
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,ProTask List")]
        public ActionResult Index()
        {
            ViewBag.BusiType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.VehicleTypes = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Vehicle Type", Value = "0"},
                               }, "Value", "Text", 0);

            ViewBag.VehicleManu = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Manufacturer", Value = "0"},
                               }, "Value", "Text", 0);
            ViewBag.VehicleModels = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "Select the Model", Value = "0"},
                              }, "Value", "Text", 0);

            ViewBag.TaskName = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.TaskType = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Projects = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.TStat = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);

            ViewBag.Employee = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            ViewBag.Locat = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
           }, "Value", "Text", 1);

            ViewBag.User = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
            }, "Value", "Text", 1);

            ViewBag.Mobile = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                         },
                           "Value", "Text", 1);
            //               ID = (int)e,
            //               Name = e.ToString()

            ViewBag.SalesExecutive = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
           }, "Value", "Text", 1);
            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindTask").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ProTaskViewModel vmodel = new ProTaskViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Task").ToList();



            var ref1 = db.ProTasks.Where(o => o.Ref1 != null)
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.ProTasks
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            return View(vmodel);
        }
        [HttpGet]
        public ActionResult cald(long? driver,string taskdate)
        {
            if (driver == null)
                driver = -1;
            var start = DateTime.Today;
            if (taskdate!=null)
            start = DateTime.Parse(taskdate, new CultureInfo("en-GB"));
         
            List<calview> clockQuery = new List<calview>();
            List<calview> clockQuery2 = new List<calview>();
            List<calview> clockQuery3 = new List<calview>();
            clockQuery = (from offset in Enumerable.Range(10, 38)

                          select new calview
                          {
                              dt = start.AddMinutes(30 * offset)
                          }).ToList();
            
            
            string[] cl = { "green", "blue", "gold", "red", "green", "blue", "gold", "red", "green", "blue", "gold", "red", "green", "blue", "gold", "red", "green", "blue", "gold", "red", "green", "blue", "gold", "red", "green", "blue", "gold", "red" };
            int clcount = 0;
            int taskcount = 0;
            for (var i = 0; i < 38; i++)
            {
                DateTime? fdate = null;
                fdate = clockQuery[i].dt;
                var task = (from a in db.ProTasks.Where(o=>o.Branch==1)
                            join e in db.Employees on a.driver equals e.EmployeeId
                            let AssignedTo = (from z in db.TaskAssigneds
                                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                              where z.ProTaskId == a.ProTaskId && z.Status == "Assigned" && z.chkStatus == Status.active
                                              select new
                                              {
                                                  //id = y.EmployeeId,
                                                  //LastName = (y.LastName != null) ? y.LastName : "",
                                                  FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                  //MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                  //Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                  //y.Status
                                              }).Distinct().ToList()

                            where (EF.Functions.DateDiffMinute(a.StartTime, fdate) >= 0) &&
                                (EF.Functions.DateDiffMinute(a.EndTime, fdate) <= 0) &&
                                a.driver == driver
                            select new
                            {
                                a.ProTaskId,
                                a.StartTime,
                                a.EndTime,
                                a.TaskName,
                                AssignedTo,
                                e.FirstName

                            }).ToList();
                taskcount = 0;
                foreach(var t in task)
                {
                    if (taskcount == 0)
                    {
                        string ass = String.Join(",", t.AssignedTo.Select(o => o.FirstName).ToArray());
                        string prid = t.ProTaskId.ToString();
                        clockQuery[i].selectedcol = true;
                        clockQuery[i].protaskid = prid;
                        clockQuery[0].firstname = t.FirstName;

                        clockQuery[i].colorname = "white";
                        if (i == 0)
                        {
                            clockQuery[i].taskname = t.TaskName;
                            clockQuery[i].taskassigned = ass;
                            clockQuery[i].colorname = cl[0];

                        }
                        else
                        {
                            if (clockQuery[i - 1].protaskid != clockQuery[i].protaskid)
                            {
                                clcount++;
                                clockQuery[i].taskname = t.TaskName;
                                clockQuery[i].taskassigned = ass;
                                 clockQuery[i].colorname = cl[0];

                            }
                            else if (clockQuery[i - 1].protaskid == clockQuery[i].protaskid)
                            {
                                clockQuery[i].colorname = cl[0];
                            }

                        }
                    }
                    if (taskcount == 1)
                    {
                        if (clockQuery2.Count() == 0)
                        {
                            clockQuery2 = (from offset in Enumerable.Range(10, 38)

                                           select new calview
                                           {
                                               dt = start.AddMinutes(30 * offset)
                                           }).ToList();
                        }
                        string ass = String.Join(",", t.AssignedTo.Select(o => o.FirstName).ToArray());
                        string prid = t.ProTaskId.ToString();
                        clockQuery2[i].selectedcol = true;
                        clockQuery2[i].protaskid = prid;
                        clockQuery2[0].firstname = t.FirstName;

                        clockQuery2[i].colorname = "white";
                        if (i == 0)
                        {
                            clockQuery2[i].taskname = t.TaskName;
                            clockQuery2[i].taskassigned = ass;
                            clockQuery2[i].colorname = cl[0];

                        }
                        else
                        {
                            if (clockQuery2[i - 1].protaskid != clockQuery2[i].protaskid)
                            {
                                clcount++;
                                clockQuery2[i].taskname = t.TaskName;
                                clockQuery2[i].taskassigned = ass;
                                clockQuery2[i].colorname = cl[0];

                            }
                            else if (clockQuery[i - 1].protaskid == clockQuery[i].protaskid)
                            {
                                clockQuery2[i].colorname = cl[0];
                            }

                        }
                    }
                    if (taskcount == 2)
                    {
                        if (clockQuery3.Count() == 0)
                        {
                            clockQuery3 = (from offset in Enumerable.Range(10, 38)

                                           select new calview
                                           {
                                               dt = start.AddMinutes(30 * offset)
                                           }).ToList();
                        }
                        string ass = String.Join(",", t.AssignedTo.Select(o => o.FirstName).ToArray());
                        string prid = t.ProTaskId.ToString();
                        clockQuery3[i].selectedcol = true;
                        clockQuery3[i].protaskid = prid;
                        clockQuery3[0].firstname = t.FirstName;

                        clockQuery3[i].colorname = "white";
                        if (i == 0)
                        {
                            clockQuery3[i].taskname = t.TaskName;
                            clockQuery3[i].taskassigned = ass;
                            clockQuery3[i].colorname = cl[0];

                        }
                        else
                        {
                            if (clockQuery3[i - 1].protaskid != clockQuery3[i].protaskid)
                            {
                                clcount++;
                                clockQuery3[i].taskname = t.TaskName;
                                clockQuery3[i].taskassigned = ass;
                                clockQuery3[i].colorname = cl[0];

                            }
                            else if (clockQuery3[i - 1].protaskid == clockQuery3[i].protaskid)
                            {
                                clockQuery3[i].colorname = cl[0];
                            }

                        }
                    }
                    taskcount++;
                }
                if (task.Count()>0)
                {
                }

            }
            calviewlist cls = new calviewlist();
            cls.mulcalview1 = clockQuery.ToList();
            cls.mulcalview2 = clockQuery2.ToList();
            cls.mulcalview3 = clockQuery3.ToList();
            return PartialView(cls);
        }
        public ActionResult bulkAssign(string Items)
        {
            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            var enumData = from pricingstatagytype e in Enum.GetValues(typeof(pricingstatagytype))
                           select new
                           {
                               ID = (int)e,
                               Name = e.ToString()
                           };
            ViewBag.priceStattyp = QkSelect.List(enumData, "Id", "Name");
            long[] itemsidd = new long[] { };

            if (Items != "")
            {
                itemsidd = Items.Split(',').Select(x => long.Parse(x)).ToArray();


                if (itemsidd.Count() > 0)
                {
                    var i = 0;
                    foreach (var dt in itemsidd)
                    {

                        var A1 = db.TaskAssigneds.Where(a => a.ProTaskId == dt && a.chkStatus == Status.active && a.Status == "Assigned").Select(a => a.EmployeeId).ToList().ToArray() ?? null;
                        var viewBagKey = "team" + i;
                        ViewData["team" + i] = new MultiSelectList(use, "ID", "Name", A1);
                        i++;
                    }

                }
            }
            var AssignedMembers = db.TaskAssigneds.Where(a => itemsidd.Contains(a.ProTaskId) && a.chkStatus == Status.active && a.Status == "Assigned").Select(a => a.EmployeeId).ToList().ToArray() ?? null;

            ViewBag.team = new MultiSelectList(use, "ID", "Name", AssignedMembers);
            List<taskUpdates> alldata = new List<taskUpdates>();
            SerialNoViewModel ViewModel = new SerialNoViewModel();




            ViewBag.ddlItem = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text ="", Value = ""},
                             }, "Value", "Text", 1);

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            if (Items != "")
            {
                foreach (long ItemId in itemsidd)
                {

                    //Item
                    var Item = db.Items.Select(s => new
                    {
                        Id = s.ItemID,
                        Name = s.ItemCode + "-" + s.ItemName,
                    }).ToList();
                    ViewBag.ddlItem = QkSelect.List(Item, "Id", "Name");

                    var data = (from a in db.ProTasks.Where(o=>o.Branch==1)
                                join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                                from d in cus.DefaultIfEmpty()
                                where (a.ProTaskId == ItemId)
                                select new taskUpdates
                                {
                                    ProTaskId = a.ProTaskId,
                                    TaskCode = a.TaskCode,
                                    TaskName = a.TaskName,
                                    CustomerName = d.CustomerName,

                                }).ToList().Select(o => new taskUpdates
                                {
                                    ProTaskId = o.ProTaskId,
                                    TaskCode = o.TaskCode,
                                    TaskName = o.TaskName,
                                    CustomerName = o.CustomerName,


                                });

                    alldata.AddRange(data);


                }
                ViewModel.taskUpdatess = alldata;
                return PartialView(ViewModel);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            }

        }
        [HttpPost]
        public ActionResult bulkAssign(SerialNoViewModel ViewModal)
        {

            var UserId = User.Identity.GetUserId();
            if (ViewModal.taskUpdatess != null)
            {
                foreach (var Row in ViewModal.taskUpdatess)
                {
                    if (Row.bulkAssign != null)
                    {
                        ProTask task = db.ProTasks.Find(Row.ProTaskId);
                        
                        task.OpenClose = 0;
                        db.Entry(task).State = EntityState.Modified;
                           db.SaveChanges();
                        var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == Row.ProTaskId && a.Status == "Assigned" && a.chkStatus == Status.active).Select(a => a.EmployeeId).ToArray();
                        var newusers = Row.bulkAssign.ToArray();

                        TaskAssigned tskass = new TaskAssigned();

                        foreach (var arr in tskasgn)
                        {
                            var taskassId = db.TaskAssigneds.Where(a => a.ProTaskId == Row.ProTaskId && a.Status == "Assigned" && a.chkStatus == Status.active && a.EmployeeId == arr).Select(a => a.TaskAssignedId).FirstOrDefault();
                            TaskAssigned tskassr = db.TaskAssigneds.Find(taskassId);
                            if (!newusers.Contains(arr))
                            {

                                tskassr.chkStatus = Status.inactive;
                                db.Entry(tskassr).State = EntityState.Modified;
                                db.SaveChanges();

                                tskass.ProTaskId = Row.ProTaskId;
                                tskass.EmployeeId = arr;
                                tskass.Status = "Removed";
                                tskass.AssignBy = UserId;
                                tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100); ;
                                db.TaskAssigneds.Add(tskass);
                                db.SaveChanges();
                            }
                            else
                            {
                                tskassr.chkStatus = Status.inactive;


                                db.Entry(tskassr).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }


                        foreach (var arr in Row.bulkAssign)
                        {
                            tskass.ProTaskId = Row.ProTaskId;
                            tskass.EmployeeId = arr;
                            tskass.Status = "Assigned";
                            tskass.AssignBy = UserId;
                            tskass.chkStatus = Status.active;
                            tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100);
                            db.TaskAssigneds.Add(tskass);
                            db.SaveChanges();
                            com.remideradd(rootdomain+"proTask/mytask", arr, UserId, "Task Assined",Row.ProTaskId);

                        }
                    }



                }
                Success("Successfully Updated", true);
            }

            return RedirectToAction("Index", "ProTask");

        }
        public ActionResult taskcal()
        {
            string dtt = System.DateTime.Now.ToString("dd-MM-yyyy");
            var dr = (from a in db.ProTasks.Where(o=>o.Branch==1)
                      join b in db.Employees on a.driver equals b.EmployeeId
                      where a.driver != null

                      select new SelectFormatpro
                      {
                         id=b.EmployeeId,
                         text= b.FirstName,
                         dt= dtt
                      }).Distinct().ToList();
          

                    
                    
            return View(dr);
        }

        [HttpPost]
        public ActionResult taskcal(string taskdate)
        {
            DateTime start = DateTime.Parse(taskdate, new CultureInfo("en-GB"));
            var dr = (from a in db.ProTasks.Where(o=>o.Branch==1)
                      join b in db.Employees on a.driver equals b.EmployeeId
                      where a.driver != null &&
                      a.StartDate == start
                      select new SelectFormatpro
                      {
                          id = b.EmployeeId,
                          text = b.FirstName,
                          dt=taskdate
                      }).Distinct().ToList();




            return View(dr);
        }
        public ActionResult signatureupload(long id)
        {
            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            return PartialView();
        }

        //Customer Satisfaction From "MyTask"
        public ActionResult CustomerSatisfactionMyTask(long id)
        {

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var SatsObj = db.CustomerSatisfactions.Where(a => a.ProTaskId == id).FirstOrDefault();

            CustomerSatisfactionViewModel VModel = new CustomerSatisfactionViewModel();

            if (SatsObj != null)
            {
                VModel.Id = SatsObj.Id;
                VModel.Comments = SatsObj.Comments;
                VModel.SatisfactionLevel = SatsObj.SatisfactionLevel;
                VModel.Signature = SatsObj.Signature;
                return PartialView(VModel);
            }
            else
            {
                VModel.Id = 0;
                return PartialView(VModel);
            }
        }

        //Customer Satisfaction From "Index"
        public ActionResult CustomerSatisfactionTask(long id)
        {

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var SatsObj = db.CustomerSatisfactions.Where(a => a.ProTaskId == id).FirstOrDefault();

            CustomerSatisfactionViewModel VModel = new CustomerSatisfactionViewModel();

            if (SatsObj != null)
            {
                VModel.Id = SatsObj.Id;
                VModel.Comments = SatsObj.Comments;
                VModel.SatisfactionLevel = SatsObj.SatisfactionLevel;
                VModel.Signature = SatsObj.Signature;
                return PartialView(VModel);
            }
            else
            {
                VModel.Id = 0;
                return PartialView(VModel);
            }
        }

        //POST -- UploadCustomerSatisfaction
        [HttpPost]
        public ActionResult UploadCustomerSatisfaction(long id, string imageData, string[] mtdata)
        {
            bool stat = false;
            string msg = "";

            var SatsObj = db.CustomerSatisfactions.Where(a => a.ProTaskId == id).FirstOrDefault();

            if (SatsObj != null)
            {
                db.CustomerSatisfactions.RemoveRange(db.CustomerSatisfactions.Where(a => a.ProTaskId == id)); ;

                //To Delete Image from folder
                string fullPath = LegacyWeb.MapPath("~/uploads/protaskdocuments/Signatures/" + SatsObj.Signature);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                //To Delete Thumb Image from folder
                string fullPathThmb = LegacyWeb.MapPath("~/uploads/protaskdocuments/Signatures/thumb_" + SatsObj.Signature);
                if (System.IO.File.Exists(fullPathThmb))
                {
                    System.IO.File.Delete(fullPathThmb);
                }
            }

            string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/Signatures/");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "- ").Replace(":", "") + ".png";
            string fileNameWitPath = path + fname;
            string thump = path + "thumb_" + fname;

            using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    byte[] data = Convert.FromBase64String(imageData);
                    bw.Write(data);
                    bw.Close();
                }

            }
            System.IO.File.Copy(fileNameWitPath, thump);

            //TaskUpdation
            ProTaskUpdation TaskUps = new ProTaskUpdation
            {
                ProTaskId = id,
                CreatedBy = User.Identity.GetUserId(),
                CreatedDate = System.DateTime.Now,
                Remarks = "Signature",
            };
            db.ProTaskUpdations.Add(TaskUps);
            db.SaveChanges();

            //Customer Satisfaction Table
            var Obj = new CustomerSatisfaction
            {
                ProTaskId = id,
                SatisfactionLevel = mtdata[0],
                Comments = mtdata[1],
                Signature = fname,//Path.GetFileName(file.FileName),
                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                CreatedBy = User.Identity.GetUserId(),
            };
            db.CustomerSatisfactions.Add(Obj);
            db.SaveChanges();

            msg = "Customer Satisfaction added successfully..!";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public void UploadSignature(long id, string imageData)
        {
            string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/");
            var FStatus = Status.active;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fname = DateTime.Now.ToString().Replace("/", "-").Replace(" ", "- ").Replace(":", "") + ".png";
            string fileNameWitPath = path + fname;
            string thump = path + "thumb_" + fname;
            using (FileStream fs = new FileStream(fileNameWitPath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    byte[] data = Convert.FromBase64String(imageData);
                    bw.Write(data);
                    bw.Close();
                }

            }
            System.IO.File.Copy(fileNameWitPath, thump);
            ProTaskUpdation TaskUps = new ProTaskUpdation
            {
                ProTaskId = id,
                CreatedBy = User.Identity.GetUserId(),
                CreatedDate = System.DateTime.Now,
                Remarks = "Signature",
            };
            db.ProTaskUpdations.Add(TaskUps);
            db.SaveChanges();
            long TaskUpdId = TaskUps.TaskUpdationID;
            var taskimg = new TaskImage
            {
                ProTaskId = id,
                TaskUpdationID = TaskUpdId,

                FileName = fname,//Path.GetFileName(file.FileName),
                Status = FStatus,
                CreatedDate = Convert.ToDateTime(System.DateTime.Now),

                CreatedBy = User.Identity.GetUserId(),
            };
            db.TaskImages.Add(taskimg);
            db.SaveChanges();

        }
        public ActionResult dashboard()
        {
            DateTime dd = DateTime.Now.AddDays(-60);
            var v = (
               from c in db.ProTasks.Where(o=>o.Branch==1)
               join c1 in db.TaskStatus on c.TaskStatus equals c1.TaskStatusId

               join d in db.protaskdashbordorder on c.TaskStatus equals d.task
               where c.CreatedDate >= dd
               orderby d.dashboardposition
               group new { c.TaskStatus, c1.StatusName } by new { c.TaskStatus } into g
               select new paramclass
               {
                   statusname = g.FirstOrDefault().StatusName,
                   satusid = (long)g.FirstOrDefault().TaskStatus,
                   count = g.Count(),
               }


                ).ToList();


            ViewBag.dashboard = v;

            var ass = (from l in db.TaskAssigneds
                       join p in db.ProTasks.Where(o=>o.Branch==1) on l.ProTaskId equals p.ProTaskId
                       where p.CreatedDate >= dd
                       group l by new { l.ProTaskId } into g
                       select new
                       {
                           protaskid = g.FirstOrDefault().ProTaskId,
                           createdate = g.Max(o => o.CreatedDate),

                       });

            var ddsss = ass.ToList();

            var vv = (from c in db.ProTasks.Where(o=>o.Branch==1)
                      join a in db.protaskdashbordorder on c.TaskStatus equals a.task
                      join b in db.TaskStatus on c.TaskStatus equals b.TaskStatusId
                      join l in ass on c.ProTaskId equals l.protaskid

                      let k = l.createdate.Value.AddMinutes(a.duration ?? 0)

                      orderby a.dashboardposition

                      group new { c.TaskStatus, b.StatusName, k } by new { b.StatusName } into g


                      select new
                      {
                          statusname = g.FirstOrDefault().StatusName,
                          satusid = g.FirstOrDefault().TaskStatus,
                          count = g.Count(),
                          ndate = g.FirstOrDefault().k
                      }



             ).Where(o => o.ndate < System.DateTime.Now).Select(o => new paramclass
             {

                 statusname = o.statusname,

                 satusid = (long)o.satusid,
                 count = o.count,


             }).ToList();


            ViewBag.expiredlead = vv;




            return View();
        }
        public ActionResult modallist()
        {
            return View();
        }
        public ActionResult GetAllTaskdash(long protaskstatus)
        {

            int days = 0;

            DateTime dd = DateTime.Now.AddDays(-60);
            DateTime datecheck = DateTime.Now.AddDays(-days);

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
            bool allcheck = User.IsInRole("All ProTask");
            bool editcheck = User.IsInRole("Edit ProTask");
            bool devcheck = User.IsInRole("Dev");


            TaskPriority Prior = new TaskPriority();


            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? remddate = null;






            var taskups = from element in db.ProTaskUpdations
                          group element by element.ProTaskId
                       into groups
                          select groups.OrderByDescending(p => p.CreatedDate).FirstOrDefault();





            /*     
             *      select new
                           {
                               pid = gcs.Key.ProTaskId,
                               crby = createdby.FirstOrDefault(),
                               crdate=createddate

                           }).OrderByDescending(p => p.crdate);
            var taskups = db.ProTaskUpdations.GroupBy(x => x.ProTaskId).Select(gr => new { protaskid = gr.Key, CreatedDate = gr.Max(o => o.CreatedDate), CreatedBy=gr.Max(p=>p.CreatedBy) }).ToList()
                .Select(o => new
                {
                    prid = o.protaskid,
                    CreatedDate = o.CreatedDate,
                    CreatedBy=o.CreatedBy

                });
            */

            var UserView = (from a in db.ProTasks.Where(o=>o.Branch==1)

                            join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                            from b in pro.DefaultIfEmpty()
                            join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                            from c in type.DefaultIfEmpty()
                            join e in db.Users on a.CreatedBy equals e.Id into usr
                            from e in usr.DefaultIfEmpty()
                            join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                            from d in cus.DefaultIfEmpty()
                            join f in db.TaskStatus on a.TaskStatus equals f.TaskStatusId into ttask
                            from f in ttask.DefaultIfEmpty()
                            join s in db.Employees on a.SalesPerson equals s.EmployeeId into emp
                            from s in emp.DefaultIfEmpty()
                            join i in db.Contacts on d.Contact equals i.ContactID into tmp
                            from i in tmp.DefaultIfEmpty()
                            join taskup in taskups on a.ProTaskId equals taskup.ProTaskId into tsk
                            from taskup in tsk.DefaultIfEmpty()


                            let Reminder = (from z in db.Reminders
                                            where z.Type == "Task" && z.Reference == a.ProTaskId

                                            orderby z.RDate descending
                                            select new
                                            {
                                                ReminderDate = z.RDate,
                                                validity = (DateTime.Now <= z.RDate) ? "Upcoming" : "Expired",

                                            }).FirstOrDefault()
                            //let taskup = db.ProTaskUpdations.Where(x => x.ProTaskId == a.ProTaskId).OrderByDescending(x => x.CreatedDate).FirstOrDefault()
                            let mobnum = db.TaskMobiles.Where(x => x.ProTaskId == a.ProTaskId).Select(x => x.MobileNo).ToList()
                            // let log = db.LogManagers.Where(lg => lg.LogID == a.ProTaskId.ToString() && lg.LogTable == "ProTasks").OrderByDescending(lg => lg.LogManagerID).FirstOrDefault()
                            where a.TaskStatus == protaskstatus && a.CreatedDate >= dd
                            select new
                            {
                                a.ProTaskId,
                                a.TaskName,
                                a.TaskCode,
                                b.ProjectName,
                                ProjectId = a.ProjectId == null ? 0 : b.ProjectId,
                                CustomerName = a.CustomerID == -2 ? "No Customer" : (a.CustomerID == null ? d.CustomerName : d.CustomerName),
                                CustomerID = a.CustomerID == -2 ? -2 : (a.CustomerID == null ? d.CustomerID : d.CustomerID),
                                e.UserName,
                                a.StartDate,
                                a.EndDate,
                                a.StartTime,
                                a.EndTime,
                                c.TypeName,
                                //a.TaskStatus,
                                //f.Status,
                                a.Priority,
                                TaskStat = f.StatusName,
                                a.CreatedDate,
                                s.FirstName,
                                s.LastName,
                                //AssignedTo = j.FirstName + " " + j.LastName,
                                AssignedTo = (from z in db.TaskAssigneds
                                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                              where z.ProTaskId == a.ProTaskId && z.Status == "Assigned" && z.chkStatus == Status.active
                                              select new
                                              {
                                                  id = y.EmployeeId,
                                                  LastName = (y.LastName != null) ? y.LastName : "",
                                                  FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                  MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                  Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                  y.Status
                                              }).Distinct().ToList(),
                                TskLead = (from ac in db.TaskMobiles
                                           where (ac.ProTaskId == a.ProTaskId)
                                           select new MobileViewModel
                                           {
                                               Num = ac.MobileNo
                                           }).ToList(),
                                a.CreatedBy,
                                UserId,
                                allcheck,
                                editcheck,
                                devcheck,
                                a.Ref1,
                                a.Ref2,
                                a.Ref3,
                                a.Ref4,
                                a.Ref5,
                                Reminder = Reminder,
                                a.Location,
                                ldate = ((a != null) && (taskup.CreatedDate > a.logtime)) ? taskup.CreatedDate : a.logtime,
                                Updateduser = db.Users.Where(x => x.Id == taskup.CreatedBy).Select(x => x.UserName).FirstOrDefault(),
                                a.TaskDetails,
                            })

                              .ToList().Select(o => new
                              {
                                  o.ProTaskId,
                                  o.TaskName,
                                  o.TaskCode,
                                  Project = o.ProjectId,
                                  o.ProjectName,
                                  o.CustomerID,
                                  o.CustomerName,
                                  o.UserName,
                                  o.StartDate,
                                  o.EndDate,
                                  o.StartTime,
                                  o.EndTime,
                                  o.TypeName,

                                  Priority = Enum.GetName(typeof(TaskPriority), o.Priority),
                                  o.AssignedTo,
                                  o.TaskStat,
                                  o.CreatedDate,
                                  EmpName = o.FirstName + " " + o.LastName,
                                  o.CreatedBy,
                                  o.UserId,
                                  o.allcheck,
                                  o.editcheck,
                                  o.devcheck,
                                  o.Ref1,
                                  o.Ref2,
                                  o.Ref3,
                                  o.Ref4,
                                  o.Ref5,
                                  ReminderDate = o.Reminder != null ? o.Reminder.ReminderDate : null,
                                  validity = (o.Reminder != null && o.Reminder.ReminderDate != null) ? o.Reminder.validity : null,
                                  o.Location,
                                  o.ldate,
                                  o.TaskDetails,
                                  mobmodel = o.TskLead,
                                  o.Updateduser
                              });


            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(
                    p => p.TaskName.ToString().ToLower().Contains(search.ToLower()) ||
                    p.TaskName.ToString().ToLower().StartsWith(search.ToLower()) ||
                    p.TaskName.ToString().ToLower().EndsWith(search.ToLower())
                );
            }


            db.Dispose();

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            UserView = UserView.Distinct();
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList().Distinct();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });


        }
        public ActionResult GetAllTaskdashexp(long protaskstatus)
        {

            int days = 0;
            DateTime dd = DateTime.Now.AddDays(-60);
            DateTime datecheck = DateTime.Now.AddDays(-days);

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
            bool allcheck = User.IsInRole("All ProTask");
            bool editcheck = User.IsInRole("Edit ProTask");
            bool devcheck = User.IsInRole("Dev");


            TaskPriority Prior = new TaskPriority();


            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? remddate = null;

            var ass = (from l in db.TaskAssigneds
                       group new { l.ProTaskId, l.CreatedDate } by new { l.ProTaskId, l.CreatedDate } into g
                       select new
                       {
                           protaskid = g.FirstOrDefault().ProTaskId,
                           createdate = g.FirstOrDefault().CreatedDate,

                       });

            // vv computed CLIENT-SIDE: the GroupBy().Select(g => g.FirstOrDefault().X) aggregation feeds a
            // Where(ndate < Now) that EF Core 10 cannot translate. Materialize the raw inputs, aggregate in
            // memory with identical semantics. vv is used only as a TaskStatus-membership filter on the main
            // query (v is never projected), so we derive vvSatusIds from it.
            var assRaw = db.TaskAssigneds
                          .Select(l => new { l.ProTaskId, l.CreatedDate }).Distinct().ToList()
                          .Select(g => new { protaskid = g.ProTaskId, createdate = g.CreatedDate })
                          .ToList();

            var vvRaw = (from c in db.ProTasks.Where(o => o.Branch == 1)
                         where c.CreatedDate >= dd
                         join a in db.protaskdashbordorder on c.TaskStatus equals a.task
                         join b in db.TaskStatus on c.TaskStatus equals b.TaskStatusId
                         select new { c.TaskStatus, c.ProTaskId, b.StatusName, a.duration }).ToList();

            var vv = (from c in vvRaw
                      join l in assRaw on c.ProTaskId equals l.protaskid
                      let k = l.createdate.Value.AddMinutes(c.duration ?? 0)
                      group new { c.TaskStatus, c.StatusName, k } by new { l.protaskid } into g
                      select new
                      {
                          statusname = g.FirstOrDefault().StatusName,
                          satusid = g.FirstOrDefault().TaskStatus,
                          count = g.Count(),
                          ndate = g.FirstOrDefault().k
                      })
                      .Where(o => o.ndate < System.DateTime.Now)
                      .Select(o => new paramclass
                      {
                          statusname = o.statusname,
                          satusid = (long)o.satusid,
                          count = o.count,
                      }).ToList();

            // distinct TaskStatus values surfaced by vv (replaces `join v in vv on a.TaskStatus equals v.satusid`).
            var vvSatusIds = vv.Select(v => (long?)v.satusid).Distinct().ToList();





            /*     
             *      select new
                           {
                               pid = gcs.Key.ProTaskId,
                               crby = createdby.FirstOrDefault(),
                               crdate=createddate

                           }).OrderByDescending(p => p.crdate);
            var taskups = db.ProTaskUpdations.GroupBy(x => x.ProTaskId).Select(gr => new { protaskid = gr.Key, CreatedDate = gr.Max(o => o.CreatedDate), CreatedBy=gr.Max(p=>p.CreatedBy) }).ToList()
                .Select(o => new
                {
                    prid = o.protaskid,
                    CreatedDate = o.CreatedDate,
                    CreatedBy=o.CreatedBy

                });
            */

            // SERVER query — pure entities only (EF Core 10 translatable). The `join v in vv` is replaced
            // by a TaskStatus-membership filter (vvSatusIds, computed in memory above). The taskups
            // GroupBy-latest join, the let Reminder/let mobnum subqueries and the nested
            // AssignedTo/TskLead/Updateduser collections are removed here and recomputed CLIENT-SIDE.
            var rawRows = (from a in db.ProTasks.Where(o => o.Branch == 1)
                           join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                           from b in pro.DefaultIfEmpty()
                           join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                           from c in type.DefaultIfEmpty()
                           join e in db.Users on a.CreatedBy equals e.Id into usr
                           from e in usr.DefaultIfEmpty()
                           join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                           from d in cus.DefaultIfEmpty()
                           join f in db.TaskStatus on a.TaskStatus equals f.TaskStatusId into ttask
                           from f in ttask.DefaultIfEmpty()
                           join s in db.Employees on a.SalesPerson equals s.EmployeeId into emp
                           from s in emp.DefaultIfEmpty()
                           where a.TaskStatus == protaskstatus && vvSatusIds.Contains(a.TaskStatus)
                           select new { a, b, c, e, d, f, s }).ToList();

            var ids = rawRows.Select(o => o.a.ProTaskId).ToList();

            // latest ProTaskUpdation per task (was the taskups GroupBy-latest + taskup join)
            var taskupLookup = db.ProTaskUpdations
                .Where(u => ids.Contains(u.ProTaskId)).ToList()
                .GroupBy(u => u.ProTaskId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(u => u.CreatedDate).First());

            // assignees (was the nested AssignedTo collection)
            var assignLookup = (from z in db.TaskAssigneds
                                where z.Status == "Assigned" && z.chkStatus == Status.active && ids.Contains(z.ProTaskId)
                                join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                select new { z.ProTaskId, id = y.EmployeeId, LastName = y.LastName ?? "", FirstName = y.FirstName ?? "", MiddleName = y.MiddleName ?? "", Img = y.ImgFileName ?? "", y.Status })
                               .ToList().ToLookup(z => z.ProTaskId);

            // task mobiles (was the nested TskLead collection / mobnum)
            var leadLookup = db.TaskMobiles.Where(x => ids.Contains(x.ProTaskId))
                              .Select(x => new { x.ProTaskId, x.MobileNo }).ToList().ToLookup(x => x.ProTaskId);

            // latest reminder per task (was the let Reminder subquery)
            var remLookup = db.Reminders.Where(z => z.Type == "Task" && ids.Contains(z.Reference))
                              .ToList().GroupBy(z => z.Reference)
                              .ToDictionary(g => g.Key, g => g.OrderByDescending(z => z.RDate).First());

            // updating-user names (was the Updateduser subquery)
            var updByIds = taskupLookup.Values.Where(u => u.CreatedBy != null).Select(u => u.CreatedBy).Distinct().ToList();
            var updUserLookup = db.Users.Where(u => updByIds.Contains(u.Id))
                                  .Select(u => new { u.Id, u.UserName }).ToList()
                                  .ToDictionary(u => u.Id, u => u.UserName);

            // CLIENT re-projection — identical member names & order to the original outer Select.
            var UserView = rawRows.Select(r =>
            {
                var a = r.a;
                ProTaskUpdation taskup = taskupLookup.TryGetValue(a.ProTaskId, out var tu) ? tu : null;
                var rem = remLookup.TryGetValue(a.ProTaskId, out var rr) ? rr : null;
                DateTime? ldate = (taskup != null && taskup.CreatedDate.HasValue && taskup.CreatedDate.Value > a.logtime) ? taskup.CreatedDate : (DateTime?)a.logtime;
                return new
                {
                    a.ProTaskId,
                    a.TaskName,
                    a.TaskCode,
                    Project = a.ProjectId == null ? 0 : (r.b != null ? r.b.ProjectId : 0),
                    ProjectName = r.b != null ? r.b.ProjectName : null,
                    CustomerID = a.CustomerID == -2 ? (long?)-2 : (a.CustomerID == null ? (r.d != null ? r.d.CustomerID : (long?)null) : (r.d != null ? r.d.CustomerID : (long?)null)),
                    CustomerName = a.CustomerID == -2 ? "No Customer" : (r.d != null ? r.d.CustomerName : null),
                    UserName = r.e != null ? r.e.UserName : null,
                    a.StartDate,
                    a.EndDate,
                    a.StartTime,
                    a.EndTime,
                    TypeName = r.c != null ? r.c.TypeName : null,
                    Priority = Enum.GetName(typeof(TaskPriority), a.Priority),
                    AssignedTo = assignLookup[a.ProTaskId].Select(z => new { z.id, z.LastName, z.FirstName, z.MiddleName, z.Img, z.Status }).Distinct().ToList(),
                    TaskStat = r.f != null ? r.f.StatusName : null,
                    a.CreatedDate,
                    EmpName = (r.s != null ? r.s.FirstName : "") + " " + (r.s != null ? r.s.LastName : ""),
                    a.CreatedBy,
                    UserId,
                    allcheck,
                    editcheck,
                    devcheck,
                    a.Ref1,
                    a.Ref2,
                    a.Ref3,
                    a.Ref4,
                    a.Ref5,
                    ReminderDate = rem != null ? rem.RDate : (DateTime?)null,
                    validity = (rem != null && rem.RDate != null) ? (DateTime.Now <= rem.RDate ? "Upcoming" : "Expired") : null,
                    a.Location,
                    ldate,
                    a.TaskDetails,
                    mobmodel = leadLookup[a.ProTaskId].Select(m => new MobileViewModel { Num = m.MobileNo }).ToList(),
                    Updateduser = (taskup != null && taskup.CreatedBy != null && updUserLookup.ContainsKey(taskup.CreatedBy)) ? updUserLookup[taskup.CreatedBy] : null,
                };
            });


            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(
                    p => p.TaskName.ToString().ToLower().Contains(search.ToLower()) ||
                    p.TaskName.ToString().ToLower().StartsWith(search.ToLower()) ||
                    p.TaskName.ToString().ToLower().EndsWith(search.ToLower())
                );
            }


            db.Dispose();

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            UserView = UserView.Distinct();
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList().Distinct();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });


        }
        [RedirectingAction]
        [Authorize(Roles = "Dev,My ProTask")]
        public ActionResult createimageupload()//TKUpdateStatus? status //IEnumerable<TKUpdateStatus> status
        {

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindTask").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ViewBag.TaskName = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.TaskType = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Projects = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.TStat = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Employee = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                         }, "Value", "Text", 1);
            //     .Select(s => new
            //         ID = s.EmployeeId,
            //         Name = s.FirstName + " " + s.LastName
            //     })

            ViewBag.User = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
            }, "Value", "Text", 1);

            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));

            ViewBag.SalesExecutive = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
           }, "Value", "Text", 1);


            ViewBag.Locat = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);
            ViewBag.Mobile = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                         },
                           "Value", "Text", 1);



            var ref1 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.ProTasks
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");


            ProTaskViewModel vmodel = new ProTaskViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Task").ToList();
            ViewBag.proid = Request.Query["protaskid"];
            return View(vmodel);

        }

        [RedirectingAction]
        [Authorize(Roles = "Dev,ProTask List")]
        [HttpPost]
        public ActionResult GetAllTaskContact(long flag, long? taskname, long? tasktype, long? customer, long? projects, long? assignedto, string createdby, string priority, string fromdate, string todate, long? taskstat, long? empl, string ref1, string ref2, string ref3, string ref4, string ref5, string remdate, string remstatus, string local, string Mobile, string LastUpdDays, long AssTo, string txtremarks, long? VType, long? Vmanu, long? Vmod, string OpnCls)//TKUpdateStatus? status //IEnumerable<TKUpdateStatus> status
        {




            int days = 0;
            if (LastUpdDays != "")
            {
                days = Convert.ToInt32(LastUpdDays);
            }
            int OandC = 0;
            if (OpnCls != "")
            {
                OandC = Convert.ToInt32(OpnCls);
            }
            DateTime datecheck = DateTime.Now.AddDays(-days);

            string search = Request.Form.GetValues("search[value]")[0];
            int taken = 300;
            if (search != "" && search.Length < 4)
            {
                taken = 100;
                search = "";
                flag = 0;
            }

            if (search == "" && taskname == 0 && tasktype == 0 && customer == 0 && projects == 0 && assignedto == 0 && createdby == "All" && priority == "0" && fromdate == "" && todate == "" && taskstat == 0 && empl == 0 && (ref1 == null || ref1 == "") && (ref2 == null || ref2 == "") && ref3 == null && ref4 == null && ref5 == null && remdate == "" && remstatus == "0" && local == "0" && Mobile == "All" && LastUpdDays == "0" && AssTo == 0 && txtremarks == "" && VType == 0 && Vmanu == 0 && Vmod == 0&& OpnCls=="")
            {
                flag = 0;
                taken = 100;
            }
            if (search != "")
            {
                flag = 1;
            }
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
            bool allcheck = User.IsInRole("All ProTask");
            bool editcheck = User.IsInRole("Edit ProTask");
            bool devcheck = User.IsInRole("Dev");


            TaskPriority Prior = new TaskPriority();
            if (priority == "1")
            {
                Prior = (TaskPriority)TaskPriority.Low;
            }
            if (priority == "2")
            {
                Prior = (TaskPriority)TaskPriority.Medium;
            }
            if (priority == "3")
            {
                Prior = (TaskPriority)TaskPriority.High;
            }

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? remddate = null;
            if (!string.IsNullOrEmpty(fromdate))
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(todate))
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(remdate))
            {
                remddate = DateTime.Parse(remdate, new CultureInfo("en-GB"));
            }





            DateTime crdate = DateTime.Now.AddDays(-3);
            DateTime todaylog = DateTime.Now.Date.AddDays(1);
            flag = 1;
            var taskassign = (from z in db.TaskAssigneds
                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                              where z.Status == "Assigned" && z.chkStatus == Status.active &&
                              (flag == 1 || z.CreatedDate >= crdate)
                              select z);

            var mob = (
            from co in db.Contacts

            join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
            join pro in db.ProTasks.Where(o=>o.Branch==1) on rrr.RelationID equals pro.ProTaskId
            join con in db.Country on co.CountryID equals con.CountryID into conn
            from con in conn.DefaultIfEmpty()
            where (rrr.RelationType == 11) &&
            (flag == 1 || (pro.logtime > crdate && pro.logtime < todaylog))
            select new 
            {

                Num = co.Mobile,
                ID = pro.ProTaskId

            });
            var TskLead = (from ac in db.TaskMobiles
                           join pro in db.ProTasks.Where(o=>o.Branch==1) on ac.ProTaskId equals pro.ProTaskId
                           where

            (flag == 1 || (pro.logtime > crdate && pro.logtime < todaylog))
                           select new 
                           {
                               Num = ac.MobileNo,
                               ID = pro.ProTaskId
                           });
            var fulls = mob.Union(TskLead).Where(o=>o.Num!=null);

            //              where
            //              (flag == 1 || (p.logtime > crdate && p.logtime < todaylog))

            //                  a.TaskStatusID,
            //                  a.TaskId,
            //                  a.CreatedDate,
            //                  c.StatusName,
            //                  a.TaskUpdationID


            // taskups (GroupBy-latest) removed — it was only used by a join whose result (`taskup`) this grid
            // never projects; EF Core 10 can't translate the GroupBy-FirstOrDefault join (ProjectionBinding).
            //              where (flag == 1 || (element.CreatedDate > crdate && element.CreatedDate < todaylog))
            //              group element by element.ProTaskId
            //           into groups

            var UserView = (from a in db.ProTasks.Where(o=>o.Branch==1)
                            join mobs in fulls on a.ProTaskId equals mobs.ID
                            join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                            from b in pro.DefaultIfEmpty()
                            join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                            from c in type.DefaultIfEmpty()
                            join e in db.Users on a.CreatedBy equals e.Id into usr
                            from e in usr.DefaultIfEmpty()
                            join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                            from d in cus.DefaultIfEmpty()
                            join f in db.TaskStatus on a.TaskStatus equals f.TaskStatusId into ttask
                            from f in ttask.DefaultIfEmpty()
                            join s in db.Employees on a.SalesPerson equals s.EmployeeId into emp
                            from s in emp.DefaultIfEmpty()
                            // taskups (GroupBy-latest) join removed — EF Core 10 can't translate it (ProjectionBinding)
                            // and `taskup` is never referenced in this grid's projection, so it's simply dropped.
                            join h in db.VehicleTypes on a.VTypId equals h.VTypeId into temp1
                            from h in temp1.DefaultIfEmpty()
                            join j in db.VehicleManufacturers on a.VManuId equals j.MId into temp2
                            from j in temp2.DefaultIfEmpty()
                            join k in db.VehicleModels on a.VModId equals k.ModelId into temp3
                            from k in temp3.DefaultIfEmpty()
                           // let mobsearch=fulls.Where(o=>Mobile=="All"||o.Num ==Mobile).Select(o=>o.Num).FirstOrDefault()
                            where (allcheck == true || a.CreatedBy == UserId) &&
                              (taskname == null || taskname == 0 || a.ProTaskId == taskname) &&
                              (tasktype == null || tasktype == 0 || a.TaskType == tasktype) &&
                              (customer == null || customer == 0 || a.CustomerID == customer) &&
                              (projects == null || projects == 0 || a.ProjectId == projects) &&
                              (createdby == "" || createdby == "All" || a.CreatedBy == createdby) &&
                              (priority == "0" || priority == null || a.Priority == Prior) &&
                                 (fromdate == "" || (a.CreatedDate != null && EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0)) &&
                                (todate == "" || (a.CreatedDate != null && EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0)) &&
                               (taskstat == 0 || taskstat == null || a.TaskStatus == taskstat) &&

                              (empl == 0 || empl == null || a.SalesPerson == empl) &&
                              (ref1 == "" || ref1 == null || a.Ref1 == ref1) &&
                              (ref2 == "" || ref2 == null || a.Ref2 == ref2) &&
                              (ref3 == "" || ref3 == null || a.Ref3 == ref3) &&
                              (ref4 == "" || ref4 == null || a.Ref4 == ref4) &&
                              (ref5 == "" || ref5 == null || a.Ref5 == ref5) &&
            
                              (VType == 0 || VType == null || a.VTypId == VType) &&
                              (Vmanu == 0 || Vmanu == null || a.VManuId == Vmanu) &&
                              (Vmod == 0 || Vmod == null || a.VModId == Vmod) 
                              &&(OpnCls == "" || OandC == null || (OandC == 0 && (a.OpenClose == 0 || a.OpenClose == null)) || a.OpenClose == OandC) 
                              && (local == "0" || local == "All" || local == null || a.Location == local) 
                             // && (Mobile == "All" || Mobile == "" ||  mobsearch==Mobile) 
                           &&(search == "" ||
                           a.TaskName.ToString().ToLower().Contains(search.ToLower()) ||
                    a.TaskName.ToString().ToLower().StartsWith(search.ToLower()) ||
                    a.TaskName.ToString().ToLower().EndsWith(search.ToLower()) ||
                    a.TaskCode.ToString().ToLower().StartsWith(search.ToLower())
                    )

                            select new
                            {
                                a.ProTaskId,
                                a.TaskName,
                               CustomerName = a.CustomerID == -2 ? "No Customer" : (a.CustomerID == null ? d.CustomerName : d.CustomerName),
                                CustomerID = a.CustomerID == -2 ? -2 : (a.CustomerID == null ? d.CustomerID : a.CustomerID),
                                UserId,
                                allcheck,
                                editcheck,
                                devcheck,
                                mobs.Num
                            }).ToList().Select(o => new
                              {
                                  o.ProTaskId,
                                  o.TaskName,
                                 
                                  o.CustomerID,
                                  o.CustomerName,
                            
                                  mobmodel = o.Num, //mobilesno(o.ProTaskId),
                              });

            //    // Apply search   
            //    UserView = UserView.Where(
            //        p.TaskCode.ToString().ToLower().StartsWith(search.ToLower())





            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            UserView = UserView.Distinct();
            recordsTotal = taken;// UserView.Count();
            var data = UserView.ToList().Distinct();


            // return new QuickSoft.Models.LegacyJsonResult { Data = new { data = data } };
            var jsonResult= Json(new { draw = draw, recordsFiltered = data.Count(), recordsTotal = data.Count(), data = data });

            return jsonResult;
        }

        [RedirectingAction]
        [Authorize(Roles = "Dev,ProTask List")]
        [HttpPost]
        public ActionResult GetAllTasks(long flag, long? taskname, long? tasktype, long? customer, long? projects, long? assignedto, string createdby, string priority, string fromdate, string todate, long? taskstat, long? empl, string ref1, string ref2, string ref3, string ref4, string ref5, string remdate, string remstatus, string local, string Mobile, string LastUpdDays, long AssTo, string txtremarks, long? VType, long? Vmanu, long? Vmod, string OpnCls,string ddlbind)//TKUpdateStatus? status //IEnumerable<TKUpdateStatus> status
        {



          
            int days = 0;
            if (LastUpdDays != "")
            {
                days = Convert.ToInt32(LastUpdDays);
            }
            int OandC = 0;
           
            DateTime datecheck = DateTime.Now.AddDays(-days);

            string search = Request.Form.GetValues("search[value]")[0];
            int taken = 300;
            if (search != "" && search.Length < 4)
            {
                taken = 50;
                search = "";
                flag = 0;
            }
            
            if (search == "" && taskname == 0 && tasktype == 0 && customer == 0 && projects == 0 && assignedto == 0 && createdby == "All" && priority == "0" && fromdate == "" && todate == "" && taskstat == 0 && empl == 0 && (ref1 == null|| ref1 == "") && (ref2 == null||  ref2=="")&& ref3 == null && ref4 == null && ref5 == null && remdate == "" && remstatus == "0" && local == "0" && Mobile == "All" && LastUpdDays == "0" &&txtremarks == "" && VType == 0 && Vmanu == 0 && Vmod == 0 && AssTo==0)
            {
                flag = 0;
                taken = 100;
            }
            if (search!="")
            {
                flag = 1;
            }
            if (ddlbind == "0" || ddlbind == "1")
            {
                flag = 1;
            }
            if (OpnCls != "")
            {
                OandC = Convert.ToInt32(OpnCls);
                flag = 1;
            }
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
            bool allcheck = true ;
            bool editcheck = User.IsInRole("Edit ProTask");
            bool devcheck = User.IsInRole("Dev");


            TaskPriority Prior = new TaskPriority();
            if (priority == "1")
            {
                Prior = (TaskPriority)TaskPriority.Low;
            }
            if (priority == "2")
            {
                Prior = (TaskPriority)TaskPriority.Medium;
            }
            if (priority == "3")
            {
                Prior = (TaskPriority)TaskPriority.High;
            }

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? remddate = null;
            if (!string.IsNullOrEmpty(fromdate))
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(todate))
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(remdate))
            {
                remddate = DateTime.Parse(remdate, new CultureInfo("en-GB"));
            }







            string qry = @"update ProTasks set TaskStatus =q2.TaskStatusID from ProTasks join(
select ProTaskId,TaskStatus,TaskStatusID  from ProTasks join
  (
(select  TaskId,TaskStatusID,CreatedDate,
RANK() OVER (PARTITION BY TaskId ORDER BY TaskUpdationID DESC) AS rk
from TaskRemarks where Remark!='Task Edited' and createddate>='2024-05-01' group by 
TaskId ,TaskStatusID,CreatedDate,TaskUpdationID ) ) as q1 on q1.TaskId =ProTasks.ProTaskId 
where q1.rk =1 and TaskStatus !=TaskStatusID  ) as q2  on q2.ProTaskId =ProTasks.ProTaskId
";
           var exec = db.Database.ExecuteSqlRaw(qry);


             qry = @"update ProTasks set logtime =q2.CreatedDate from  ProTasks 
 join (
select logtime,q1.CreatedDate,q1.ProTaskId from protasks join 
(select  ProTaskId,max(CreatedDate) as CreatedDate

from ProTaskUpdations where CreatedDate>'2025-05-01'
group by ProTaskId
) as q1 on q1.ProTaskId=ProTasks.ProTaskId  
where  q1.CreatedDate>protasks.logtime   ) as q2
on q2.ProTaskId =ProTasks.ProTaskId
";
             exec = db.Database.ExecuteSqlRaw(qry);
            /*     
             *      select new
                           {
                               pid = gcs.Key.ProTaskId,
                               crby = createdby.FirstOrDefault(),
                               crdate=createddate

                           }).OrderByDescending(p => p.crdate);
            var taskups = db.ProTaskUpdations.GroupBy(x => x.ProTaskId).Select(gr => new { protaskid = gr.Key, CreatedDate = gr.Max(o => o.CreatedDate), CreatedBy=gr.Max(p=>p.CreatedBy) }).ToList()
                .Select(o => new
                {
                    prid = o.protaskid,
                    CreatedDate = o.CreatedDate,
                    CreatedBy=o.CreatedBy

                });
            */
            DateTime rdates = DateTime.Now.AddDays(-30);
            DateTime crdate = DateTime.Now.AddDays(-2);
            DateTime todaylog = DateTime.Now.Date.AddDays(1);
            var taskassign = (from z in db.TaskAssigneds
                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                              where z.Status == "Assigned" && z.chkStatus == Status.active &&
                              (flag == 1 || z.CreatedDate >= crdate)
                              select z);

            var mob = (
            from co in db.Contacts
            
            join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
            join pro in db.ProTasks.Where(o=>o.Branch==1) on rrr.RelationID equals pro.ProTaskId
            join con in db.Country on co.CountryID equals con.CountryID into conn
            from con in conn.DefaultIfEmpty()
            where ( rrr.RelationType == 11)&&
            (flag == 1 ||(pro.logtime > crdate && pro.logtime < todaylog))
            select new MobileViewModel
            {

                Num = co.Mobile,
                ID=pro.ProTaskId

            });
            var TskLead = (from ac in db.TaskMobiles
                           join pro in db.ProTasks.Where(o=>o.Branch==1) on ac.ProTaskId equals pro.ProTaskId
                           where
                           
            (flag == 1 || (pro.logtime > crdate && pro.logtime < todaylog))
                           select new MobileViewModel
                           {
                               Num = ac.MobileNo,
                               ID=pro.ProTaskId
                           });
            var fulls = mob.Union(TskLead).ToList();

            //              where
            //              (flag == 1 || (p.logtime > crdate && p.logtime < todaylog))

            //                  a.TaskStatusID,
            //                  a.TaskId,
            //                  a.CreatedDate,
            //                  c.StatusName,
            //                  a.TaskUpdationID


            var taskups = from element in db.ProTaskUpdations
                          where (flag == 1 || (element.CreatedDate > crdate && element.CreatedDate < todaylog))
                          group element by element.ProTaskId

                       into groups
                          select groups.OrderByDescending(p => p.CreatedDate).FirstOrDefault();
            var cn = db.ProTasks.Count();
            if (cn < 400)
                flag = 1;
            var UserViews = (from a in db.ProTasks.Where(o => o.Branch == 1)

                             join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                             from b in pro.DefaultIfEmpty()
                             join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                             from c in type.DefaultIfEmpty()
                             join e in db.Users on a.CreatedBy equals e.Id into usr
                             from e in usr.DefaultIfEmpty()
                             join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                             from d in cus.DefaultIfEmpty()
                             join f in db.TaskStatus on a.TaskStatus equals f.TaskStatusId into ttask
                             from f in ttask.DefaultIfEmpty()
                             join s in db.Employees on a.SalesPerson equals s.EmployeeId into emp
                             from s in emp.DefaultIfEmpty()
                             join ss in db.Employees on a.salesexecutive equals ss.EmployeeId into emps
                             from ss in emps.DefaultIfEmpty()
                             // taskups (GroupBy-latest) join removed — EF Core 10 can't translate it; the latest
                             join h in db.VehicleTypes on a.VTypId equals h.VTypeId into temp1
                             from h in temp1.DefaultIfEmpty()
                             join j in db.VehicleManufacturers on a.VManuId equals j.MId into temp2
                             from j in temp2.DefaultIfEmpty()
                             join k in db.VehicleModels on a.VModId equals k.ModelId into temp3
                             from k in temp3.DefaultIfEmpty()
                                 //  let additionatsk = db.additionaltasks.Where(o => o.taskid == a.ProTaskId).Select(o => o.salesentryid).FirstOrDefault()
                                 // let salesinvocetask=db.SalesEntrys.Where(o=>o.ProTask==a.ProTaskId).Select(o=>o.SalesEntryId).FirstOrDefault()
                             let Reminder = (from z in db.Reminders
                                             where z.Type == "Task" && z.Reference == a.ProTaskId
                                             && z.CreatedDate >= rdates
                                             orderby z.RDate descending
                                             select new
                                             {
                                                 ReminderDate = z.RDate,
                                                 validity = (DateTime.Now <= z.RDate) ? "Upcoming" : "Expired",

                                             }).FirstOrDefault()
                             //let taskup = db.ProTaskUpdations.Where(x => x.ProTaskId == a.ProTaskId).OrderByDescending(x => x.CreatedDate).FirstOrDefault()
                             //let mobnum = db.TaskMobiles.Where(x => x.ProTaskId == a.ProTaskId).Select(x => x.MobileNo).ToList()



                             // let log = db.LogManagers.Where(lg => lg.LogID == a.ProTaskId.ToString() && lg.LogTable == "ProTasks").OrderByDescending(lg => lg.LogManagerID).FirstOrDefault()
                             //let mob = (
                             //where (rrr.RelationID == a.ProTaskId && rrr.RelationType == 11)

                             //    Num = co.Mobile,

                             //}).ToList()
                             // let tkstat = taskst.Where(o => o.TaskStatusID == taskstat && o.TaskId == a.ProTaskId).Select(o => o.TaskStatusID).FirstOrDefault() //db.TaskRemarks.Where(o => o.TaskStatusID == taskstat && o.TaskId == a.ProTaskId).Select(o => o.TaskStatusID).FirstOrDefault()
                             where (allcheck == true || a.CreatedBy == UserId) &&


                              // Recent-only restriction (logtime within 2 days) relaxed — the list now shows the
                              // most-recently-updated tasks via the server-side OrderBy(logtime)+Take(300) below,
                              // so it works on a static/stale data copy too (not just live data with recent activity).
                              (flag == 1 || true) &&
                               (taskname == null || taskname == 0 || a.ProTaskId == taskname) &&
                               (tasktype == null || tasktype == 0 || a.TaskType == tasktype) &&
                               (customer == null || customer == 0 || a.CustomerID == customer) &&
                               (projects == null || projects == 0 || a.ProjectId == projects) &&
                               (createdby == "" || createdby == "All" || a.CreatedBy == createdby) &&
                               (priority == "0" || priority == null || a.Priority == Prior) &&
                                  (fromdate == "" || (a.CreatedDate != null && EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0)) &&
                                 (todate == "" || (a.CreatedDate != null && EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0)) &&
                                (taskstat == 0 || taskstat == null || a.TaskStatus == taskstat) &&
                                 a.VManuId != 999 &&
                               (empl == 0 || empl == null || a.salesexecutive == empl) &&
                               (ref1 == "" || ref1 == null || a.Ref1 == ref1) &&
                               (ref2 == "" || ref2 == null || a.Ref2 == ref2) &&
                               (ref3 == "" || ref3 == null || a.Ref3 == ref3) &&
                               (ref4 == "" || ref4 == null || a.Ref4 == ref4) &&
                               (ref5 == "" || ref5 == null || a.Ref5 == ref5) &&
                               (remstatus == "0" || Reminder.validity == remstatus) &&

                               (VType == 0 || VType == null || a.VTypId == VType) &&
                               (Vmanu == 0 || Vmanu == null || a.VManuId == Vmanu) &&
                               (Vmod == 0 || Vmod == null || a.VModId == Vmod) &&
                               (OpnCls==""|| a.OpenClose==  OandC)&&
                               (remdate == "" || EF.Functions.DateDiffDay(a.StartDate, remddate) == 0)
                               && (local == "0" || local == "All" || local == null || a.Location == local) &&
                               true &&
                            (search == "" ||
                            a.TaskName.ToString().ToLower().Contains(search.ToLower()) ||
                     a.TaskName.ToString().ToLower().StartsWith(search.ToLower()) ||
                     a.TaskName.ToString().ToLower().EndsWith(search.ToLower()) ||
                     a.TaskCode.ToString().ToLower().StartsWith(search.ToLower())
                     )

                             select new
                             {
                                 a,
                                 b,
                                 c,
                                 d,
                                 e,
                                 f,
                                 s,
                                 ss,
                                 // The full computed projection is built CLIENT-SIDE below (after Take) — EF Core 10
                                 // can't translate this projection's subqueries/collections. ldate stays here so the
                                 // date Where() + ordering remain server-side. (Reminder is kept as a let for the
                                 // WHERE filter and recomputed client-side via a lookup.)
                                 //TaskStat = (from x in taskst
                                 //            where x.TaskId == a.ProTaskId

                                 //                x.TaskStatusID,
                                 //                x.CreatedDate,
                                 //                x.StatusName
                             });

            if (ddlbind != "")
            {
                var notinvoiced = db.Database.SqlQueryRaw<long>(@"select distinct ProTaskId from ProTasks where ProTaskId in(
 select distinct protaskid from ItemTasks a
 join ItemTaskMasters b on a.protaskid = b.TaskId where protaskid not in(
   select distinct protaskid  from ItemTasks where protaskid   in
(select distinct taskid from(select ProTask as taskid, SalesEntryId as saleid from SalesEntries
union select taskid, salesentryid as saleid  from additionaltaks ) as q1)))").ToArray();
                if (ddlbind == "0")
                {
                    UserViews = UserViews.Where(o => !notinvoiced.Contains(o.a.ProTaskId));
                }
                else
                {
                    UserViews = UserViews.Where(o => notinvoiced.Contains(o.a.ProTaskId));
                }
            }
            // The GroupBy-latest "taskup" can't be projected by EF Core 10 — materialize the server-filtered
            // tasks (WHERE#1 bounds this), then compute taskup/ldate + the date filter + ordering + Take client-side.
            var rawRows = UserViews.OrderByDescending(o => o.a.logtime).Take(300).ToList();
            var rawIds = rawRows.Select(o => o.a.ProTaskId).ToList();
            var taskupLookup = db.ProTaskUpdations
                .Where(u => rawIds.Contains(u.ProTaskId) && (flag == 1 || (u.CreatedDate > crdate && u.CreatedDate < todaylog)))
                .ToList().GroupBy(u => u.ProTaskId).ToDictionary(g => g.Key, g => g.OrderByDescending(u => u.CreatedDate).First());
            var takenTasks = rawRows
                .Select(r => new { o = r, tu = taskupLookup.ContainsKey(r.a.ProTaskId) ? taskupLookup[r.a.ProTaskId] : null })
                .Select(r => new { r.o, r.tu, ldate = (r.tu != null && r.tu.CreatedDate > r.o.a.logtime) ? r.tu.CreatedDate : r.o.a.logtime })
                .Where(r => (fromdate == "" || (r.ldate != null && fdate != null && r.ldate.Value.Date <= fdate.Value.Date))
                         && (todate == "" || (r.ldate != null && tdate != null && r.ldate.Value.Date >= tdate.Value.Date)))
                .OrderByDescending(r => r.ldate).Take(300).ToList();
            var atIds = takenTasks.Select(r => r.o.a.ProTaskId).ToList();
            var assignLookup = (from z in db.TaskAssigneds
                                where z.Status == "Assigned" && z.chkStatus == Status.active
                                      && (flag == 1 || z.CreatedDate >= crdate) && atIds.Contains(z.ProTaskId)
                                join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                select new { z.ProTaskId, id = y.EmployeeId, LastName = y.LastName ?? "", FirstName = y.FirstName ?? "", MiddleName = y.MiddleName ?? "", Img = y.ImgFileName ?? "", y.Status })
                               .ToList().ToLookup(z => z.ProTaskId);
            // Reminder + Updateduser were also untranslatable subqueries in the server projection — compute
            // them client-side for the taken tasks via lookups.
            var remLookup = db.Reminders.Where(z => z.Type == "Task" && z.CreatedDate >= rdates && atIds.Contains(z.Reference))
                              .ToList().GroupBy(z => z.Reference).ToDictionary(g => g.Key, g => g.OrderByDescending(z => z.RDate).First());
            var updByIds = takenTasks.Where(r => r.tu != null && r.tu.CreatedBy != null).Select(r => r.tu.CreatedBy).Distinct().ToList();
            var updUserLookup = db.Users.Where(u => updByIds.Contains(u.Id)).Select(u => new { u.Id, u.UserName }).ToList().ToDictionary(u => u.Id, u => u.UserName);
            var UserView = takenTasks.Select(r => new
                              {
                                  r.o.a.ProTaskId,
                                  r.o.a.TaskName,
                                  r.o.a.TaskCode,
                                  Project = r.o.a.ProjectId == null ? 0 : (r.o.b != null ? r.o.b.ProjectId : 0),
                                  ProjectName = r.o.b != null ? r.o.b.ProjectName : null,
                                  CustomerID = r.o.a.CustomerID == -2 ? (long?)-2 : (r.o.a.CustomerID ?? (r.o.d != null ? r.o.d.CustomerID : (long?)null)),
                                  CustomerName = r.o.a.CustomerID == -2 ? "No Customer" : (r.o.d != null ? r.o.d.CustomerName : null),
                                  UserName = r.o.e != null ? r.o.e.UserName : null,
                                  r.o.a.StartDate,
                                  r.o.a.EndDate,
                                  SalelsExecutive = (r.o.ss != null ? r.o.ss.FirstName : "") + " " + (r.o.ss != null ? r.o.ss.LastName : ""),
                                  r.o.a.StartTime,
                                  r.o.a.EndTime,
                                  TypeName = r.o.c != null ? r.o.c.TypeName : null,
                                  r.o.a.OpenClose,
                                  r.o.a.VTypId,
                                  r.o.a.VManuId,
                                  r.o.a.VModId,
                                  Priority = Enum.GetName(typeof(TaskPriority), r.o.a.Priority),
                                  TaskStat = r.o.f != null ? r.o.f.StatusName : null,
                                  r.o.a.CreatedDate,
                                  EmpName = (r.o.s != null ? r.o.s.FirstName : "") + " " + (r.o.s != null ? r.o.s.LastName : ""),
                                  r.o.a.CreatedBy,
                                  UserId,
                                  allcheck,
                                  editcheck,
                                  devcheck,
                                  r.o.a.Ref1,
                                  r.o.a.Ref2,
                                  r.o.a.Ref3,
                                  r.o.a.Ref4,
                                  r.o.a.Ref5,
                                  ReminderDate = remLookup.ContainsKey(r.o.a.ProTaskId) ? remLookup[r.o.a.ProTaskId].RDate : (DateTime?)null,
                                  validity = remLookup.ContainsKey(r.o.a.ProTaskId) ? (DateTime.Now <= remLookup[r.o.a.ProTaskId].RDate ? "Upcoming" : "Expired") : null,
                                  r.o.a.Location,
                                  r.ldate,
                                  r.o.a.TaskDetails,
                                  AssignedTo = assignLookup[r.o.a.ProTaskId].Select(z => new { z.id, z.LastName, z.FirstName, z.MiddleName, z.Img, z.Status }).Distinct().ToList(),
                                  mobmodel = fulls.Where(e => e.ID == r.o.a.ProTaskId).ToList(),
                                  Updateduser = (r.tu != null && r.tu.CreatedBy != null && updUserLookup.ContainsKey(r.tu.CreatedBy)) ? updUserLookup[r.tu.CreatedBy] : null,
                              }).Where(o => o.ldate <= todaylog);

            //    // Apply search   
            //    UserView = UserView.Where(
            //        p.TaskCode.ToString().ToLower().StartsWith(search.ToLower())

            if (txtremarks != "")
            {
                var taskupdate = db.TaskRemarks.Where(p => p.Remark.ToLower().Contains(txtremarks.ToLower())).Select(o => o.TaskId).Distinct().ToList();
                if (taskupdate.Count > 0)

                {


                    UserView = UserView.Where(o => taskupdate.Contains(o.ProTaskId));



                }
                else
                {
                    UserView = UserView.Where(o => taskupdate.Contains(-1));
                }
            }

            

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
         
            UserView = UserView.Distinct();
            recordsTotal =  UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList().Distinct();
        
           
           
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public string ResolveShortUrl(string shortUrl)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3"); // Mimic a browser
                var response = httpClient.GetAsync(shortUrl, HttpCompletionOption.ResponseHeadersRead);
                return response.Result.RequestMessage.RequestUri.ToString(); // This will be the final redirected URL
         
            }
        }
        public Dictionary<string, object> ExtractCoordinates(string surl)
        {
            Dictionary<string, Object> ret = new Dictionary<string, object>();
            // Example URL: https://www.google.com/maps/preview/place/Brandenburg+Gate,+Pariser+Platz,+10117+Berlin,+Germany/@52.5162746,13.3777041,2428a,13.1y/data=!4m2!3m1!1s0x47a851c655f20989:0x26bbfb4e84674c63
            // httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3"); // Mimic a browser
            try
            {
                string fullUrl = ResolveShortUrl(surl);

                var match = Regex.Match(fullUrl, @"@(-?\d+\.\d+),(-?\d+\.\d+)");
                var val = fullUrl.Split(',');
                var val1 = val[0].Split('/');
                var len = val1.Length;
                var lat = val1[len - 1];
                var val2 = val[1].Split('/');
                var log = val2[0];
                if (log.Contains("?"))
                {
                    var val3 = log.Split('?');
                    log = val3[0];
                }
                ret.Add("lat", lat.Replace("@", "").Replace("+", ""));
                ret.Add("log", log.Replace("@", "").Replace("+", ""));

                return ret;
                if (match.Success && match.Groups.Count == 3)
                {
                    //    double.TryParse(match.Groups[2].Value, out double longitude))
                    if (match.Groups[1].Value != "")
                    {
                        ret.Add("lat", match.Groups[1].Value);
                        ret.Add("log", match.Groups[2].Value);
                        return ret;

                    }
                }

                ret.Add("lat", "0");
                ret.Add("log", "0");
                return ret;
            
            }
            catch(Exception e)
            {
                ret.Add("lat", "0");
                ret.Add("log", "0");
                return ret;
            }
        }
        [RedirectingAction]
      

        public ActionResult taskcontacts()
        {
            ViewBag.BusiType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.VehicleTypes = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Vehicle Type", Value = "0"},
                               }, "Value", "Text", 0);

            ViewBag.VehicleManu = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Manufacturer", Value = "0"},
                               }, "Value", "Text", 0);
            ViewBag.VehicleModels = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "Select the Model", Value = "0"},
                              }, "Value", "Text", 0);

            ViewBag.TaskName = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.TaskType = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Projects = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.TStat = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);

            ViewBag.Employee = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            ViewBag.Locat = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
           }, "Value", "Text", 1);

            ViewBag.User = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
            }, "Value", "Text", 1);

            ViewBag.Mobile = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                         },
                           "Value", "Text", 1);
            //               ID = (int)e,
            //               Name = e.ToString()

            ViewBag.SalesExecutive = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
           }, "Value", "Text", 1);
            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindTask").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ProTaskViewModel vmodel = new ProTaskViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Task").ToList();



            var ref1 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.ProTasks
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            return View(vmodel);

                   }
      public List<MobileViewModel> mobilesno(long protaskid)
        {
            

              
                var mob = (
                from co in db.Contacts
                join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                join con in db.Country on co.CountryID equals con.CountryID into conn
                from con in conn.DefaultIfEmpty()
                where (rrr.RelationID == protaskid && rrr.RelationType == 11)
                select new MobileViewModel
                {

                    Num = co.Mobile,

                });
                var TskLead = (from ac in db.TaskMobiles
                               where (ac.ProTaskId == protaskid)
                               select new MobileViewModel
                               {
                                   Num = ac.MobileNo
                               });
            var full = mob.Union(TskLead).ToList();

            return full;
        }
        public ActionResult deleteservicerepot(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            servicereport itemtaskli = db.servicereports.Find(id);
            if (itemtaskli == null)
            {
                return NotFound();
            }
            return PartialView(itemtaskli);
        }

        [HttpPost]
        public ActionResult deleteservicerepotconfirm(long id)
        {
            db.servicereports.RemoveRange(db.servicereports.Where(o => o.servicereportid == id));
            db.SaveChanges();
            db.servicereportmembers.RemoveRange(db.servicereportmembers.Where(o => o.servicereportid == id));
            db.SaveChanges();

            var rec = db.Receipts.Where(o => o.Reference == id && o.Remark == "Service Charge");
            
            if (rec.FirstOrDefault() != null)
            {
                com.DeleteAccountTransaction("Receipt", rec.FirstOrDefault().ReceiptId);
            }
            db.Receipts.RemoveRange(rec);
            db.SaveChanges();
            bool stat = true;
            string msg;

           
                msg = "Success";
            Success(msg, true);

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost]
        public ActionResult ServiceReport(ServiceReportViewModel vmodel)
        {
            bool stat = false;
            string msg;
            long customeracid = db.Customers.Where(o => o.CustomerID == vmodel.CustomerID).Select(o => o.Accounts).FirstOrDefault();
            servicereport sr = new servicereport();
            DateTime? sDate = null;
            DateTime? eDate = null;
            DateTime? stimes = null;
            DateTime? etimes = null;
          
                sDate = System.DateTime.Now.Date;
                TimeSpan? stime = null;
                if (vmodel.StartTime != null)
                {
                    stime = ((DateTime)vmodel.StartTime).TimeOfDay;
                }
                stimes = sDate + stime;
            
                eDate = System.DateTime.Now.Date;
                TimeSpan? etime = null;
                if (vmodel.EndTime != null)
                {
                    etime = ((DateTime)vmodel.EndTime).TimeOfDay;
                }
                etimes = eDate + etime;

            var us = User.Identity.GetUserId();
            var ex = db.servicereports.Any(o => o.protaskid == vmodel.ProTaskId && o.createdby == us && o.starttime == stimes);
            if (ex)
            {
                msg = "Service Report Already Exist";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }

            sr.protaskid =vmodel.ProTaskId;
            sr.starttime = stimes;
            sr.endtime = etimes;
            sr.jobstatusid = vmodel.TaskStatus;
            sr.remark = vmodel.TaskDetails;
            sr.jobtypes = vmodel.jobtypes;
            sr.paytype = vmodel.paymenttypes;
            sr.amount = vmodel.amount;
            sr.chequenumber = vmodel.chequeno;
            sr.bankname = vmodel.bank;
            sr.createdby = User.Identity.GetUserId();
            db.servicereports.Add(sr);
            db.SaveChanges();

            ProTaskUpdation TaskUps = new ProTaskUpdation
            {
                ProTaskId = vmodel.ProTaskId,
                CreatedBy = User.Identity.GetUserId(),
                CreatedDate = etimes,
                Remarks = vmodel.TaskDetails+"<br/>"+"<a href = 'javascript:void(0)' title = 'Print' onclick = 'printreport(" + sr.servicereportid+")' >< i class='fa fa-lg fa-print'></i>Print Service Report</a>",
            };
            db.ProTaskUpdations.Add(TaskUps);
            db.SaveChanges();
            Int64 TaskUpdId = TaskUps.TaskUpdationID;
            var docinfo = new TaskRemark
            {
                CreatedDate = (DateTime)etimes,
               
                Remark = vmodel.TaskDetails + "<br/>" + "<a href = 'javascript:void(0)' title = 'Print' onclick = 'printreport(" + sr.servicereportid+")' ><i class='fa fa-lg fa-print'></i>Print Service Report</a>",
                AddedUser = User.Identity.GetUserId(),
                TaskId = vmodel.ProTaskId,
                TaskUpdationID = TaskUpdId,

            };
            db.TaskRemarks.Add(docinfo);
            db.SaveChanges();
            foreach (var emp in vmodel.AssignedMembers)
            {
                servicereportmember c = new servicereportmember();
                c.employeeid = emp;
                c.servicereportid = sr.servicereportid;
                db.servicereportmembers.Add(c);
                db.SaveChanges();
            }
            Int64 srid = sr.servicereportid;

            var userid = User.Identity.GetUserId();

            var today = Convert.ToDateTime(System.DateTime.Now);







            foreach (var empid in vmodel.AssignedMembers)
            {

                long dailyattid = 0;
                today = today.AddDays(0);
                var date = today.Date;
                var date2 = today.Date.AddDays(1);
                
                var present = (from a in db.DailyAttendances
                                where a.MonthYear.Month == date.Month &&
                                a.EmployeeId == empid
                                select new
                                {
                                    a.DailyAttendanceId
                                }).FirstOrDefault();

                if (present != null)
                    dailyattid = present.DailyAttendanceId;
              
                else
                    dailyattid = 0;

                
                db.DailyAttendanceDetails.RemoveRange(db.DailyAttendanceDetails.Where(a => a.AtDate >= date && a.AtDate < date2 && a.DailyAttendanceId == dailyattid));

                db.SaveChanges();



                DailyAttendance dattend = new DailyAttendance
                {
                    EmployeeId = empid,
                    MonthYear = today.Date,
                    Branch = 1,
                    CreatedDate = today,
                    CreatedBy = userid,
                    Status = Status.active,
                };
                if (dailyattid == 0)
                {
                    db.DailyAttendances.Add(dattend);
                    db.SaveChanges();
                    dailyattid = dattend.DailyAttendanceId;
                }
               
                DailyAttendanceDetail ddetail = new DailyAttendanceDetail();


                ddetail.DailyAttendanceId = dailyattid;
                ddetail.EmployeeId = empid;
                ddetail.AtDate = today.Date;
                ddetail.AtType = 4;
                ddetail.Overtime = 0;
                db.DailyAttendanceDetails.Add(ddetail);
                db.SaveChanges();
            }
















            if (1==2)
            {
                var user = User.Identity.GetUserId();
                var impids = db.Employees.Where(o => o.UserId == user).Select(o => o.EmployeeId).FirstOrDefault();
                var data2 = (from a in db.accountmaps
                             join b in db.Accountss
                             on a.AccountId equals b.AccountsID
                             where (a.EmployeeId == impids && a.PaymentTypeId == EmployeePaymentType.Cash)
                             select new
                             {
                                 EmployeeId = a.EmployeeId,
                                 AccountId = a.AccountId,
                                 AccountNames = b.Name,
                                 PaymentType = a.PaymentTypeId,


                                 //ChequeNo,
                                 //ChequeDate
                             }).FirstOrDefault();
                if (data2 != null)
                {


                    var Remark = "Direct Reciept From Sale Entry";
                    long payid;
                    Int64 custAccID = db.Customers.Where(a => a.CustomerID == (long)vmodel.CustomerID).Select(a => a.Accounts).FirstOrDefault();

                    //SETransaction

                    payid = com.addReceipt((DateTime) sDate, custAccID, (long)data2.AccountId, vmodel.amount, vmodel.amount,"Service Charge", user, 0, srid);
                    com.addAccountTrasaction(0, vmodel.amount, custAccID, "Receipt", payid, DC.Credit, sDate, null, null, null, vmodel.ProTaskId);
                    com.addAccountTrasaction(vmodel.amount, 0, (long)data2.AccountId, "Receipt", payid, DC.Debit, sDate, null, null, null, vmodel.ProTaskId);


                }

            }







            var proId = vmodel.ProTaskId;
            db.ContactRelation.RemoveRange(db.ContactRelation.Where(o => o.RelationID == proId && o.RelationType == 11));
            db.SaveChanges();
            if (vmodel.LstContacts != null && vmodel.LstContacts.Count > 0)
            {
                foreach (var item in vmodel.LstContacts)
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

                    try
                    {
                        // Your code...
                        // Could also be before try if you know the exception occurs in SaveChanges

                        db.Contacts.Add(contact);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;
                    }

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

            long rtask = 11;
            var uid = User.Identity.GetUserId();
            var mymc = db.MCs.Where(o => o.AssignedUser == uid).Select(o => o.MCId).FirstOrDefault();
            var printdata = (from a in db.ProTasks.Where(o=>o.Branch==1)
                             join b in db.servicereports on a.ProTaskId equals b.protaskid
                             join c in db.Customers on a.CustomerID equals c.CustomerID
                             join d in db.TaskStatus on b.jobstatusid equals d.TaskStatusId into takstat
                             from d in takstat.DefaultIfEmpty()
                             join e in db.Employees on c.SalesPerson equals e.EmployeeId into sal
                             from e in sal.DefaultIfEmpty()

                             where b.servicereportid == srid
                             select new
                             {
                                 b.servicereportid,
                                 a.TaskCode,
                                 a.TaskName,
                                 b.starttime,
                                 b.endtime,
                                 c.CustomerName,
                                 c.Addres,
                                 d.StatusName,
                                 b.remark,
                                 b.jobtypes,
                                 b.paytype,
                                 b.amount,
                                 b.chequenumber,
                                 b.bankname,
                                 date = b.starttime,
                                 b.createdby,
                                 editoption=(b.createdby==uid)?1:0,
                                 Salesperson = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                 contacts=(from c in db.Contacts
                                  join cr in db.ContactRelation
                                    on new { c.ContactID, RelationType = rtask }
                                 equals new { cr.ContactID, cr.RelationType }
                                  where (cr.RelationID == vmodel.ProTaskId)
                                  select new
                                  {

                                      ContactID = c.ContactID
                                       ,
                                      Name = c.Name
                                      ,
                                      FirstName = c.FirstName,
                                      LastName = c.LastName,
                                      Address = c.Address
                                      ,
                                      Country = c.Country
                                      ,
                                      State = c.State
                                      ,
                                      City = c.City
                                      ,
                                      Zip = c.Zip
                                      ,
                                      Phone = c.Phone
                                      ,
                                      Mobile = c.Mobile
                                      ,
                                      Fax = c.Fax
                                      ,
                                      EmailId = c.EmailId
                                      ,
                                      Reference = c.Reference
                                      ,
                                      ContactPerson = c.ContactPerson
                                      ,
                                      Status = c.Status
                                      ,
                                      Group = c.Group
                                      ,
                                      SalesPMob = c.SalesPMob
                                      ,
                                      TypeOfContact = c.TypeOfContact
                                      ,
                                      Website = c.Website
                                      ,
                                      CountryID = c.CountryID
                                      ,
                                      ContactTypeID = c.ContactTypeID
                                  }).ToList(),
                                 AssignedTo = (from z in db.servicereportmembers
                                               join y in db.Employees on z.employeeid equals y.EmployeeId
                                               where z.servicereportid == srid
                                               select new
                                               {
                                                   id = y.EmployeeId,
                                                   LastName = (y.LastName != null) ? y.LastName : "",
                                                   FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                   y.Status
                                               }).Distinct().ToList(),
                                 itemsused = (from a in db.itemtasklist
                                              join b in db.Items on a.itemid equals b.ItemID
                                              join c in db.MCs on a.mcfrom equals c.MCId
                                              join d in db.ItemUnits on a.unit equals d.ItemUnitID
                                              where a.protaskid==vmodel.ProTaskId && c.MCId==mymc
                                              select new
                                              {
                                                  b.ItemCode,
                                                  b.ItemName,
                                                  a.qty,
                                                  b.ConFactor,
                                                  SellingPrice= (a.unit==b.ItemUnitID)?b.SellingPrice:b.SellingPrice/b.ConFactor,
                                                  d.ItemUnitName,
                                                  total = b.SellingPrice * a.qty
                                              }
                                            ).ToList()

                             }).FirstOrDefault();


                        

            msg = "Service Report Saved Successfully.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg,serviceid= srid,prdata=printdata } };
         
        }

        public ActionResult Download(long id)
        {
            var SaleDet = db.servicereports.Where(s => s.servicereportid == id).FirstOrDefault();
          
            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Service Report" + "-" + SaleDet.servicereportid.ToString() + "-" + SaleDet.starttime.ToString() + ".pdf");

        }
        public StringBuilder generatePdf(long id)
        {
           
            
      
      


            return generatepdfservicereport(id);

        }
        public StringBuilder generatepdfservicereport(long id)
        {
            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            var srid = id;
            long rtask = 11;
            var uid = User.Identity.GetUserId();
            var mymc = db.MCs.Where(o => o.AssignedUser == uid).Select(o => o.MCId).FirstOrDefault();

            var printdata = (from a in db.ProTasks.Where(o=>o.Branch==1)

                             join b in db.servicereports on a.ProTaskId equals b.protaskid
                             join c in db.Customers on a.CustomerID equals c.CustomerID
                             join d in db.TaskStatus on b.jobstatusid equals d.TaskStatusId into takstat
                             from d in takstat.DefaultIfEmpty()
                             join e in db.Employees on c.SalesPerson equals e.EmployeeId into sal
                             from e in sal.DefaultIfEmpty()

                             where b.servicereportid == srid
                             select new
                             {
                                 b.servicereportid,
                                 a.TaskCode,
                                 a.TaskName,
                                 b.starttime,
                                 b.endtime,
                                 c.CustomerName,
                                 c.Addres,
                                 d.StatusName,
                                 b.remark,
                                 b.jobtypes,
                                 b.paytype,
                                 b.amount,
                                 b.chequenumber,
                                 b.bankname,
                                 date = b.starttime,
                                 b.createdby,
                                 editoption = (b.createdby == uid) ? 1 : 0,
                                 Salesperson = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                 contacts = (from c in db.Contacts
                                             join cr in db.ContactRelation
                                               on new { c.ContactID, RelationType = rtask }
                                            equals new { cr.ContactID, cr.RelationType }
                                             where (cr.RelationID == a.ProTaskId)
                                             select new
                                             {

                                                 ContactID = c.ContactID
                                                  ,
                                                 Name = c.Name
                                                 ,
                                                 FirstName = c.FirstName,
                                                 LastName = c.LastName,
                                                 Address = c.Address
                                                 ,
                                                 Country = c.Country
                                                 ,
                                                 State = c.State
                                                 ,
                                                 City = c.City
                                                 ,
                                                 Zip = c.Zip
                                                 ,
                                                 Phone = c.Phone
                                                 ,
                                                 Mobile = c.Mobile
                                                 ,
                                                 Fax = c.Fax
                                                 ,
                                                 EmailId = c.EmailId
                                                 ,
                                                 Reference = c.Reference
                                                 ,
                                                 ContactPerson = c.ContactPerson
                                                 ,
                                                 Status = c.Status
                                                 ,
                                                 Group = c.Group
                                                 ,
                                                 SalesPMob = c.SalesPMob
                                                 ,
                                                 TypeOfContact = c.TypeOfContact
                                                 ,
                                                 Website = c.Website
                                                 ,
                                                 CountryID = c.CountryID
                                                 ,
                                                 ContactTypeID = c.ContactTypeID
                                             }).ToList(),
                                 AssignedTo = (from z in db.servicereportmembers
                                               join y in db.Employees on z.employeeid equals y.EmployeeId
                                               where z.servicereportid == srid
                                               select new
                                               {
                                                   id = y.EmployeeId,
                                                   LastName = (y.LastName != null) ? y.LastName : "",
                                                   FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                   y.Status
                                               }).Distinct().ToList(),
                                 itemsused = (from aa in db.itemtasklist
                                              join b in db.Items on aa.itemid equals b.ItemID
                                              join c in db.MCs on aa.mcfrom equals c.MCId
                                              join d in db.ItemUnits on aa.unit equals d.ItemUnitID
                                              where aa.protaskid == a.ProTaskId && c.MCId == mymc
                                              select new
                                              {
                                                  b.ItemCode,
                                                  b.ItemName,
                                                  aa.qty,
                                                  b.ConFactor,
                                                  SellingPrice = (aa.unit == b.ItemUnitID) ? b.SellingPrice : b.SellingPrice / b.ConFactor,
                                                  d.ItemUnitName,
                                                  total = b.SellingPrice * aa.qty
                                              }
                                            ).ToList()

                             }).FirstOrDefault();


            var cdetails = db.companys
            .Select(s => new
            {
                CName = s.CPName,
                CAddress = s.CPAddress,
                CEmail = s.CPEmail,
                CTaxRegNo = s.TRN,
                CPhone = s.CPPhone,
                s.CPMobile,
                CLogo = s.CPLogo,

            }).FirstOrDefault();

            int SI = 1;

            InvoiceLayoutViewModel Vmodel = new InvoiceLayoutViewModel();
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Service Report</b></td></tr></table>");
                    string partyDetails = "<table class='table table-bordered'>" +
                    "<tr><td style='border-right: 0px;'>" +
         "<table class='table-nob noborder' style='border: 0px;width: 100%;'>" +
         "<tr><th>Job No</th>" +
          "<td>:" + printdata.servicereportid + "</td>" +
            "</tr><tr><th>Date</th><td>:" + Convert.ToDateTime(printdata.date).ToString("dd-MM-yyyy") + "</td></tr></table></td></tr></table> ";
        sb.Append(partyDetails);
                    string contactperson = "";
                    string contactnumbter = "";
                    foreach(var cn in printdata.contacts)
                    {
                        contactperson += "," + cn.ContactPerson;
                        contactnumbter += "," + cn.Mobile;

                    }

                      string customerdetails=" <u><b> Customer Details </b></u>"+
     
                     "<table class='table table-bordered'>"+
                    "<tr>"+
                        "<td style='border-right: 0px;' >"+
                            "<table class='table-nob noborder' style='border: 0px;width: 100%;'>"+
                             "<tr><th style='width:25% !important'>Customer</th><td>:"+ printdata.CustomerName + "</td>"+
                                "</tr><tr><th> Contact Person</th><td>: "+contactperson+"</td></tr>"+
                                "<tr> <th>Address</th><td>: "+ printdata.Addres + "</td >"+
                               "</tr> <tr>"+
                               "<th> Phone</th><td>: "+contactnumbter +"</td></tr>"+
                                 "</table></td></tr></table>";


                    sb.Append(customerdetails);
                    string assignto = "";
                    foreach(var ass in printdata.AssignedTo)
                    {
                        assignto += "," + ass.FirstName + " " + ass.LastName;
                    }
                    string jobdetails = "<u><b> Job Details </b></u>" +

                        "<table class='table table-bordered;width: 100%;'>" +
                       "<tr><td ><table class='table-nob noborder' style='border: 0px;width:100%'>" +
                        "<tr><th style='width:25% !important'>Referance</th><td>: " + printdata.Salesperson + "</td></tr><tr><th> Job Status</th>" +
                         "<td>: " + printdata.StatusName + "</td></tr><tr><th>Job Remarks</th><td>: " + printdata.remark +

                          "</td></tr><tr><th>Time Consumed From</th><td>: " + Convert.ToDateTime(printdata.starttime).ToString("hh:mm tt") + " to " + Convert.ToDateTime(printdata.endtime).ToString("hh:mm tt") +

                                "</td></tr><tr><th> Engineer / Technicians </th><td>:" + assignto + "</td></tr></table> </td> </tr></table>";

                        sb.Append(jobdetails);
                    var jobtype = "";
                    if (printdata.jobtypes == JobType.amc)
                        jobtype = "AMC";
                    else if (printdata.jobtypes == JobType.warrenty)
                    {
                        jobtype = "Under Warrenty";
                    }
                    else if (printdata.jobtypes == JobType.payed)
                    {
                        jobtype = "Paid Job";
                    }
                    var paytype = "";
                    if (printdata.paytype == PaymentType.cash && jobtype!= "Under Warrenty")
                        paytype = "CASH";
                    else if (printdata.paytype == PaymentType.cheque)
                    {
                        paytype = "CHEQUE";
                    }
                    else if (printdata.paytype == PaymentType.pending)
                    {
                        paytype = "PENDING";
                    }
                    else if (printdata.paytype == PaymentType.Bank)
                    {
                        paytype = "BANK";
                    }
                    string paymentdetails="<u><b> Payment Details </b></u>"+
  
                  "<table class='table table-bordered;width:100%'><tr><td style = 'border-right: 0px;' ><table class='table-nob noborder' style='border: 0px;width:100%'>"+
                                "<tr><th style='width:25% !important'>Job</th><td>:" + jobtype+"</td></tr>" +
                                "<tr><th> Payment Recieved</th><td>: "+paytype+"</td>" +

                                "</tr><tr><th> Amount </th><td>: "+printdata.amount+"</td>" +



                                  "</tr><tr><th>cheque no</th><td>: "+printdata.chequenumber +
                                   " </td></tr><tr><th>Bank</th><td>: " +printdata.bankname+
                                    "</td></tr></table></td></tr></table>";
                    sb.Append(paymentdetails);
                    var customer= "<table class='table table-bordered' style='width:100%'>" +
                    "<tr><th style='width:25% !important'>Customer Name</th><td> : </td></tr><tr><th>Customer Signature  </th><td> : </td>" +

                    "</tr></table>";
                    sb.Append(customer);
                    var stritems="";
                    foreach(var it in printdata.itemsused)
                    {
                        stritems += "<tr><Td>" + it.ItemCode + "</td><td>" + it.ItemName + "</td><td>" + it.qty + "</td></tr>";
                    }
                    string items="<u><b> Item Used</b></u>"+
                "<table id = 'itemused' width= '100%' >< thead ><tr><th> Sl No</th><th>Item Code</th>"+
     "<th>Item name</th><th>Qty</th></tr></thead><tbody>"+stritems+"</tbody>" +
     "</table>";

                }
}
            return sb;
        }

    [HttpPost]
        public ActionResult EditServiceReport(ServiceReportViewModel vmodel)
        {
            bool stat = false;
            string msg;
            long customeracid = db.Customers.Where(o => o.CustomerID == vmodel.CustomerID).Select(o => o.Accounts).FirstOrDefault();
            servicereport sr = db.servicereports.Find(vmodel.servicereportid);
            DateTime? sDate = null;
            DateTime? eDate = null;
            DateTime? stimes = null;
            DateTime? etimes = null;
            if (vmodel.StartDate != "")
            {
                sDate = DateTime.Parse(vmodel.StartDate, new CultureInfo("en-GB")); 
            }
            else
            {
                sDate = sr.starttime.Value.Date;
            }
            TimeSpan? stime = null;
            if (vmodel.StartTime != null)
            {
                stime = ((DateTime)vmodel.StartTime).TimeOfDay;
            }
            stimes = sDate + stime;
            if (vmodel.StartDate != "")
            {
                eDate = DateTime.Parse(vmodel.EndDate, new CultureInfo("en-GB"));
            }
            else
            {
                eDate = sr.endtime.Value.Date;
            }
            TimeSpan? etime = null;
            if (vmodel.EndTime != null)
            {
                etime = ((DateTime)vmodel.EndTime).TimeOfDay;
            }
            etimes = eDate + etime;


            sr.protaskid = vmodel.ProTaskId;
            sr.starttime = stimes;
            sr.endtime = etimes;
            sr.jobstatusid = vmodel.TaskStatus;
            sr.remark = vmodel.TaskDetails;
            sr.jobtypes = vmodel.jobtypes;
            sr.paytype = vmodel.paymenttypes;
            sr.amount = vmodel.amount;
            sr.chequenumber = vmodel.chequeno;
            sr.bankname = vmodel.bank;
            sr.createdby = sr.createdby;
            db.Entry(sr).State = EntityState.Modified;
            db.SaveChanges();
            var assmementer = db.servicereportmembers.Where(o => o.servicereportid == vmodel.servicereportid);
            db.servicereportmembers.RemoveRange(assmementer);
            db.SaveChanges();
            foreach (var emp in vmodel.AssignedMembers)
            {
                servicereportmember c = new servicereportmember();
                c.employeeid = emp;
                c.servicereportid = sr.servicereportid;
                db.servicereportmembers.Add(c);
                db.SaveChanges();
            }
            Int64 srid = sr.servicereportid;














            var rec = db.Receipts.Where(o => o.Reference == vmodel.servicereportid && o.Remark == "Service Charge");
           
          
            if (rec.FirstOrDefault() != null)
            {
                com.DeleteAccountTransaction("Receipt", rec.FirstOrDefault().ReceiptId);
            }
            db.Receipts.RemoveRange(rec);
            db.SaveChanges();

            if (1==2)
            {
                var user = User.Identity.GetUserId();
                var impids = db.Employees.Where(o => o.UserId == user).Select(o => o.EmployeeId).FirstOrDefault();
                var data2 = (from a in db.accountmaps
                             join b in db.Accountss
                             on a.AccountId equals b.AccountsID
                             where (a.EmployeeId == impids && a.PaymentTypeId == EmployeePaymentType.Cash)
                             select new
                             {
                                 EmployeeId = a.EmployeeId,
                                 AccountId = a.AccountId,
                                 AccountNames = b.Name,
                                 PaymentType = a.PaymentTypeId,


                                 //ChequeNo,
                                 //ChequeDate
                             }).FirstOrDefault();
                if (data2 != null)
                {


                    var Remark = "Direct Reciept From Sale Entry";
                    long payid;
                    Int64 custAccID = db.Customers.Where(a => a.CustomerID == (long)vmodel.CustomerID).Select(a => a.Accounts).FirstOrDefault();

                    //SETransaction

                   
                    payid = com.addReceipt((DateTime)sDate, custAccID, (long)data2.AccountId, vmodel.amount, vmodel.amount, "Service Charge", user, 0, srid);
                    com.addAccountTrasaction(0, vmodel.amount, custAccID, "Receipt", payid, DC.Credit, sDate, null, null, null, vmodel.ProTaskId);

                    com.addAccountTrasaction(vmodel.amount, 0, (long)data2.AccountId, "Receipt", payid, DC.Debit, sDate, null, null, null, vmodel.ProTaskId);


                }

            }







            var proId = vmodel.ProTaskId;
            db.ContactRelation.RemoveRange(db.ContactRelation.Where(o => o.RelationID == proId && o.RelationType == 11));
            db.SaveChanges();
            if (vmodel.LstContacts != null && vmodel.LstContacts.Count > 0)
            {
                foreach (var item in vmodel.LstContacts)
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

                    try
                    {
                        // Your code...
                        // Could also be before try if you know the exception occurs in SaveChanges

                        db.Contacts.Add(contact);
                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                    ve.PropertyName, ve.ErrorMessage);
                            }
                        }
                        throw;
                    }

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

            long rtask = 11;
            var uid = sr.createdby;
            var mymc = db.MCs.Where(o => o.AssignedUser == uid).Select(o => o.MCId).FirstOrDefault();
            var printdata = (from a in db.ProTasks.Where(o=>o.Branch==1)
                             join b in db.servicereports on a.ProTaskId equals b.protaskid
                             join c in db.Customers on a.CustomerID equals c.CustomerID
                             join d in db.TaskStatus on b.jobstatusid equals d.TaskStatusId into takstat
                             from d in takstat.DefaultIfEmpty()
                             join e in db.Employees on c.SalesPerson equals e.EmployeeId into sal
                             from e in sal.DefaultIfEmpty()

                             where b.servicereportid == srid
                             select new
                             {
                                 b.servicereportid,
                                 a.TaskCode,
                                 a.TaskName,
                                 b.starttime,
                                 b.endtime,
                                 c.CustomerName,
                                 c.Addres,
                                 d.StatusName,
                                 b.remark,
                                 b.jobtypes,
                                 b.paytype,
                                 b.amount,
                                 b.chequenumber,
                                 b.bankname,
                                 date = b.starttime,
                                 b.createdby,
                                 editoption = (b.createdby == uid) ? 1 : 0,
                                 Salesperson = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                 contacts = (from c in db.Contacts
                                             join cr in db.ContactRelation
                                               on new { c.ContactID, RelationType = rtask }
                                            equals new { cr.ContactID, cr.RelationType }
                                             where (cr.RelationID == vmodel.ProTaskId)
                                             select new
                                             {

                                                 ContactID = c.ContactID
                                                  ,
                                                 Name = c.Name
                                                 ,
                                                 FirstName = c.FirstName,
                                                 LastName = c.LastName,
                                                 Address = c.Address
                                                 ,
                                                 Country = c.Country
                                                 ,
                                                 State = c.State
                                                 ,
                                                 City = c.City
                                                 ,
                                                 Zip = c.Zip
                                                 ,
                                                 Phone = c.Phone
                                                 ,
                                                 Mobile = c.Mobile
                                                 ,
                                                 Fax = c.Fax
                                                 ,
                                                 EmailId = c.EmailId
                                                 ,
                                                 Reference = c.Reference
                                                 ,
                                                 ContactPerson = c.ContactPerson
                                                 ,
                                                 Status = c.Status
                                                 ,
                                                 Group = c.Group
                                                 ,
                                                 SalesPMob = c.SalesPMob
                                                 ,
                                                 TypeOfContact = c.TypeOfContact
                                                 ,
                                                 Website = c.Website
                                                 ,
                                                 CountryID = c.CountryID
                                                 ,
                                                 ContactTypeID = c.ContactTypeID
                                             }).ToList(),
                                 AssignedTo = (from z in db.servicereportmembers
                                               join y in db.Employees on z.employeeid equals y.EmployeeId
                                               where z.servicereportid == srid
                                               select new
                                               {
                                                   id = y.EmployeeId,
                                                   LastName = (y.LastName != null) ? y.LastName : "",
                                                   FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                   y.Status
                                               }).Distinct().ToList(),
                                 itemsused = (from a in db.itemtasklist
                                              join b in db.Items on a.itemid equals b.ItemID
                                              join c in db.MCs on a.mcfrom equals c.MCId
                                              join d in db.ItemUnits on a.unit equals d.ItemUnitID
                                              where a.protaskid == vmodel.ProTaskId && c.MCId == mymc
                                              select new
                                              {
                                                  b.ItemCode,
                                                  b.ItemName,
                                                  a.qty,
                                                  b.ConFactor,
                                                  SellingPrice = (a.unit == b.ItemUnitID) ? b.SellingPrice : b.SellingPrice / b.ConFactor,
                                                  d.ItemUnitName,
                                                  total = b.SellingPrice * a.qty
                                              }
                                            ).ToList()

                             }).FirstOrDefault();




            msg = "Service Report Saved Successfully.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, serviceid = srid, prdata = printdata } };

        }

        public ActionResult getservicereport(long protaskid)
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Category");
            var uDelete = User.IsInRole("Delete Category");
            var uid = User.Identity.GetUserId();
            var tl = db.Teams.Where(o => o.TeamId == 4).Select(o => o.TeamLead).FirstOrDefault();
            var tluserid = db.Employees.Where(o => o.EmployeeId == tl).Select(o => o.UserId).FirstOrDefault();
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var hr = db.Employees.Where(o => o.EmployeeId == 5).Select(o => o.UserId).FirstOrDefault();
            var developer = db.Employees.Where(o => o.EmployeeId == 27).Select(o => o.UserId).FirstOrDefault();

            var v = (from a in db.servicereports
                     join b in db.Employees on a.createdby equals b.UserId
                     where a.protaskid == protaskid && a.starttime != null
                     select new
                     {
                         a.starttime,
                         a.servicereportid,
                         editoption = (uid==tluserid||uid== hr || a.createdby == uid ) ? 1 : 0,
                         createdby=b.FirstName+ " "+b.MiddleName+" "+b.LastName,
                         deleteoption=(uid==tluserid || uid == hr || uid == developer) ?1:0


                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);

            }

            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public ActionResult getservicereportcust(long custid)
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Category");
            var uDelete = User.IsInRole("Delete Category");
            var uid = User.Identity.GetUserId();
            var tl = db.Teams.Where(o => o.TeamId == 4).Select(o => o.TeamLead).FirstOrDefault();
            var tluserid = db.Employees.Where(o => o.EmployeeId == tl).Select(o => o.UserId).FirstOrDefault();
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.servicereports
                     join b in db.Employees on a.createdby equals b.UserId
                     join p in db.ProTasks.Where(o=>o.Branch==1) on a.protaskid equals p.ProTaskId
                     where p.CustomerID == custid && a.starttime != null
                     select new
                     {
                         a.starttime,
                         a.servicereportid,
                         editoption = (uid == tluserid || a.createdby == uid) ? 1 : 0,
                         createdby = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                         deleteoption = (uid == tluserid) ? 1 : 0


                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);

            }

            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        public JsonResult printservicereport(long srid)
        {
            companySet();
            var uid = User.Identity.GetUserId();
            var mymc = db.MCs.Where(o => o.AssignedUser == uid).Select(o => o.MCId).FirstOrDefault();
            long protaskid = db.servicereports.Find(srid).protaskid;
            var printdata = (from a in db.ProTasks.Where(o=>o.Branch==1)
                             join b in db.servicereports on a.ProTaskId equals b.protaskid
                             join c in db.Customers on a.CustomerID equals c.CustomerID
                             join d in db.TaskStatus on b.jobstatusid equals d.TaskStatusId into takstat
                             from d in takstat.DefaultIfEmpty()
                             join e in db.Employees on c.SalesPerson equals e.EmployeeId into sal
                             from e in sal.DefaultIfEmpty()

                             where b.servicereportid == srid
                             select new
                             {
                                 b.servicereportid,
                                 a.TaskCode,
                                 a.TaskName,
                                 b.starttime,
                                 b.endtime,
                                 c.CustomerName,
                                 c.Addres,
                                 d.StatusName,
                                 b.remark,
                                 b.jobtypes,
                                 b.paytype,
                                 b.amount,
                                 b.chequenumber,
                                 b.bankname,
                                 date = b.starttime,
                                 Salesperson = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                 AssignedTo = (from z in db.servicereportmembers
                                               join y in db.Employees on z.employeeid equals y.EmployeeId
                                               where z.servicereportid == srid
                                               select new
                                               {
                                                   id = y.EmployeeId,
                                                   LastName = (y.LastName != null) ? y.LastName : "",
                                                   FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                   y.Status
                                               }).Distinct().ToList(),
                                 itemsused = (from a in db.itemtasklist
                                              join b in db.Items on a.itemid equals b.ItemID
                                              join c in db.MCs on a.mcfrom equals c.MCId
                                              join d in db.ItemUnits on a.unit equals d.ItemUnitID
                                              where a.protaskid == protaskid 
                                              select new
                                              {
                                                  c.MCName,
                                                  b.ItemCode,
                                                  b.ItemName,
                                                  a.qty,
                                                  b.ConFactor,
                                                  SellingPrice = (a.unit == b.ItemUnitID) ? b.SellingPrice : b.SellingPrice / b.ConFactor,
                                                  d.ItemUnitName,
                                                  total = b.SellingPrice * a.qty
                                              }
                                            ).ToList()

                             }).FirstOrDefault();




            
            return new QuickSoft.Models.LegacyJsonResult { Data = new {  prdata = printdata } };
            
        }
        public ActionResult EditService(long Id)
        {
            companySet();
            var orguserid = db.servicereports.Find(Id).createdby;
            var usid = orguserid;
            long protaskid = Id;
            var logintime = db.EmpAttendances.Where(o => o.protaskid == Id && o.EmployeeName == usid).OrderByDescending(o => o.Id).Select(o => o.login).FirstOrDefault();
            var logouttime = db.EmpAttendances.Where(o => o.protaskid == Id && o.EmployeeName == usid).OrderByDescending(o => o.Id).Select(o => o.logout).FirstOrDefault();

         
            servicereport rpt = db.servicereports.Find(Id);
            var pro = db.ProTasks.Find(rpt.protaskid);
            ServiceReportViewModel vmodel = new ServiceReportViewModel();
            vmodel.servicereportid = Id;

            string customername = db.Customers.Find(pro.CustomerID).CustomerName;
            vmodel.CustomerName = customername;
            vmodel.TaskCode = pro.TaskCode;
            vmodel.CustomerID = pro.CustomerID;
            vmodel.Location = pro.Location;
            vmodel.ProjectId = pro.ProjectId;
            vmodel.TaskDetails = pro.TaskDetails;
            vmodel.TaskName = pro.TaskName;
            vmodel.ProTaskId = pro.ProTaskId;
            vmodel.Priority = pro.Priority;
            vmodel.StartDate = (pro.StartDate != null) ? ((DateTime)pro.StartDate).ToString("dd-MM-yyyy") : "";
            vmodel.StartTime = rpt.starttime; //logintime;
            vmodel.EndDate = (pro.EndDate != null) ? ((DateTime)pro.EndDate).ToString("dd-MM-yyyy") : "";
            vmodel.EndTime = rpt.endtime;//System.DateTime.Now; //logouttime;
            vmodel.TaskStatus = rpt.jobstatusid;
            vmodel.Note = rpt.remark;
            vmodel.TaskDetails = rpt.remark;
            vmodel.paymenttypes = rpt.paytype;
            vmodel.jobtypes = rpt.jobtypes;
            vmodel.bank = rpt.bankname;
            vmodel.chequeno = rpt.chequenumber;
            vmodel.amount = (decimal)rpt.amount;
            vmodel.driver = pro.driver;
            vmodel.SalesPerson = pro.SalesPerson;

            vmodel.Ref1 = pro.Ref1;
            vmodel.Ref2 = pro.Ref2;
            vmodel.Ref3 = pro.Ref3;
            vmodel.Ref4 = pro.Ref4;
            vmodel.Ref5 = pro.Ref5;
            vmodel.StartDate = ((DateTime)rpt.starttime).ToString("dd-MM-yyyy");
            vmodel.EndDate = ((DateTime)rpt.endtime).ToString("dd-MM-yyyy");
            ViewBag.Stat = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem {  Text = "Completed", Value = "6"},
                                     new SelectListItem {  Text = "Pending", Value = "5"},
                               }, "Value", "Text");




            vmodel.TaskType = pro.TaskType;
            var UserId = User.Identity.GetUserId();
            vmodel.AssignedMembers = db.servicereportmembers.Where(a => a.servicereportid == rpt.servicereportid ).Select(a => a.employeeid).ToList().ToArray() ?? null;
            long rtask = 11;
            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();

            ViewBag.team = new MultiSelectList(use, "ID", "Name", vmodel.AssignedMembers);

            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();

            ViewBag.CountryCodes = db.Country.ToList();

            vmodel.LstContacts = (from c in db.Contacts
                                  join cr in db.ContactRelation
                                    on new { c.ContactID, RelationType = rtask }
                                 equals new { cr.ContactID, cr.RelationType }
                                  where (cr.RelationID == pro.ProTaskId)
                                  select new
                                  {

                                      ContactID = c.ContactID
                                       ,
                                      Name = c.Name
                                      ,
                                      FirstName = c.FirstName,
                                      LastName = c.LastName,
                                      Address = c.Address
                                      ,
                                      Country = c.Country
                                      ,
                                      State = c.State
                                      ,
                                      City = c.City
                                      ,
                                      Zip = c.Zip
                                      ,
                                      Phone = c.Phone
                                      ,
                                      Mobile = c.Mobile
                                      ,
                                      Fax = c.Fax
                                      ,
                                      EmailId = c.EmailId
                                      ,
                                      Reference = c.Reference
                                      ,
                                      ContactPerson = c.ContactPerson
                                      ,
                                      Status = c.Status
                                      ,
                                      Group = c.Group
                                      ,
                                      SalesPMob = c.SalesPMob
                                      ,
                                      TypeOfContact = c.TypeOfContact
                                      ,
                                      Website = c.Website
                                      ,
                                      CountryID = c.CountryID
                                      ,
                                      ContactTypeID = c.ContactTypeID
                                  }).AsEnumerable().Select(x => new Contact
                                  {

                                      ContactID = x.ContactID,
                                      Name = x.Name
                                      ,
                                      FirstName = x.FirstName,
                                      LastName = x.LastName,
                                      Address = x.Address
                                      ,
                                      Country = x.Country
                                      ,
                                      State = x.State
                                      ,
                                      City = x.City
                                      ,
                                      Zip = x.Zip
                                      ,
                                      Phone = x.Phone
                                      ,
                                      Mobile = x.Mobile
                                      ,
                                      Fax = x.Fax
                                      ,
                                      EmailId = x.EmailId
                                      ,
                                      Reference = x.Reference
                                      ,
                                      ContactPerson = x.ContactPerson
                                      ,
                                      Status = x.Status
                                      ,
                                      Group = x.Group
                                      ,
                                      SalesPMob = x.SalesPMob
                                      ,
                                      TypeOfContact = x.TypeOfContact
                                      ,
                                      Website = x.Website
                                      ,
                                      CountryID = x.CountryID
                                      ,
                                      ContactTypeID = x.ContactTypeID
                                  }).ToList();

            return View(vmodel);
        }
        public ActionResult ServiceReport(long Id)
        {
            companySet();
            var usid = User.Identity.GetUserId();
            long protaskid = Id;
            var logintime = db.EmpAttendances.Where(o => o.protaskid == Id && o.EmployeeName == usid).OrderByDescending(o=>o.Id).Select(o => o.login).FirstOrDefault();
            var logouttime = db.EmpAttendances.Where(o => o.protaskid == Id && o.EmployeeName == usid).OrderByDescending(o => o.Id).Select(o => o.logout).FirstOrDefault();

            var pro = db.ProTasks.Find(protaskid);
            var userid = User.Identity.GetUserId();
            var existingservice=db.servicereports.Where(o => o.createdby == userid && o.protaskid == protaskid).FirstOrDefault();
            if(existingservice!=null)
            {
            }
            ServiceReportViewModel vmodel = new ServiceReportViewModel();
            string customername = db.Customers.Find(pro.CustomerID).CustomerName;
            vmodel.CustomerName = customername;
            vmodel.TaskCode = pro.TaskCode;
            vmodel.CustomerID = pro.CustomerID;
            vmodel.Location = pro.Location;
            vmodel.ProjectId = pro.ProjectId;
            vmodel.TaskDetails = pro.TaskDetails;
            vmodel.TaskName = pro.TaskName;
            vmodel.ProTaskId = pro.ProTaskId;
            vmodel.Priority = pro.Priority;
            vmodel.StartDate = (pro.StartDate != null) ? ((DateTime)pro.StartDate).ToString("dd-MM-yyyy") : "";
            vmodel.EndDate = (pro.EndDate != null) ? ((DateTime)pro.EndDate).ToString("dd-MM-yyyy") : "";
            vmodel.driver = pro.driver;
            vmodel.SalesPerson = pro.SalesPerson;

            vmodel.Ref1 = pro.Ref1;
            vmodel.Ref2 = pro.Ref2;
            vmodel.Ref3 = pro.Ref3;
            vmodel.Ref4 = pro.Ref4;
            vmodel.Ref5 = pro.Ref5;
            ViewBag.Stat = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem {  Text = "Completed", Value = "6"},
                                     new SelectListItem {  Text = "Pending", Value = "5"},
                               }, "Value", "Text");




            vmodel.TaskType = pro.TaskType;
            var UserId = User.Identity.GetUserId();
            var empoid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
            vmodel.AssignedMembers = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId && a.chkStatus == Status.active && a.EmployeeId==empoid && a.Status == "Assigned").Select(a => a.EmployeeId).ToList().ToArray() ?? null;
            long rtask = 11;
            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            var assmember = vmodel.AssignedMembers;
            var extrateam = db.Teams.Where(o => o.TeamName.ToUpper().Contains("FIELD SERVICE")).Select(o => o.TeamId).FirstOrDefault();

            var olytech = (from a in assmember
                           join b in db.Employees on a equals b.EmployeeId
                           join c in db.TeamMembers on b.EmployeeId equals c.EmployeeId
                           


                           where c.TeamId == 4 || c.TeamId == 10007 || c.TeamId == 10008||c.TeamId== extrateam
                           select new
                           {
                             emid=(long)b.EmployeeId

                           }).ToList();
            
            var olytech2 = (from a in assmember
                            join b in db.Teams on a equals b.TeamLead
                            where b.TeamId == 4 || b.TeamId == 10007 || b.TeamId == 10008||b.TeamId==extrateam
                            select new
                            {
                                emid = (long)a
                            }).ToList();
             var olytech3=olytech.Union(olytech2);
            vmodel.AssignedMembers = olytech.Select(o=>o.emid).ToArray();
            
            ViewBag.team = new MultiSelectList(use, "ID", "Name", olytech3);
            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();

            ViewBag.CountryCodes = db.Country.ToList();
            
                vmodel.LstContacts = (from c in db.Contacts
                                      join cr in db.ContactRelation
                                        on new { c.ContactID, RelationType = rtask }
                                     equals new { cr.ContactID, cr.RelationType }
                                      where (cr.RelationID == Id)
                                      select new
                                      {

                                          ContactID = c.ContactID
                                           ,
                                          Name = c.Name
                                          ,
                                          FirstName = c.FirstName,
                                          LastName = c.LastName,
                                          Address = c.Address
                                          ,
                                          Country = c.Country
                                          ,
                                          State = c.State
                                          ,
                                          City = c.City
                                          ,
                                          Zip = c.Zip
                                          ,
                                          Phone = c.Phone
                                          ,
                                          Mobile = c.Mobile
                                          ,
                                          Fax = c.Fax
                                          ,
                                          EmailId = c.EmailId
                                          ,
                                          Reference = c.Reference
                                          ,
                                          ContactPerson = c.ContactPerson
                                          ,
                                          Status = c.Status
                                          ,
                                          Group = c.Group
                                          ,
                                          SalesPMob = c.SalesPMob
                                          ,
                                          TypeOfContact = c.TypeOfContact
                                          ,
                                          Website = c.Website
                                          ,
                                          CountryID = c.CountryID
                                          ,
                                          ContactTypeID = c.ContactTypeID
                                      }).AsEnumerable().Select(x => new Contact
                                      {

                                          ContactID = x.ContactID,
                                          Name = x.Name
                                          ,
                                          FirstName = x.FirstName,
                                          LastName = x.LastName,
                                          Address = x.Address
                                          ,
                                          Country = x.Country
                                          ,
                                          State = x.State
                                          ,
                                          City = x.City
                                          ,
                                          Zip = x.Zip
                                          ,
                                          Phone = x.Phone
                                          ,
                                          Mobile = x.Mobile
                                          ,
                                          Fax = x.Fax
                                          ,
                                          EmailId = x.EmailId
                                          ,
                                          Reference = x.Reference
                                          ,
                                          ContactPerson = x.ContactPerson
                                          ,
                                          Status = x.Status
                                          ,
                                          Group = x.Group
                                          ,
                                          SalesPMob = x.SalesPMob
                                          ,
                                          TypeOfContact = x.TypeOfContact
                                          ,
                                          Website = x.Website
                                          ,
                                          CountryID = x.CountryID
                                          ,
                                          ContactTypeID = x.ContactTypeID
                                      }).ToList();
            
            return View(vmodel);
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create ProTask")]
        public ActionResult Create(long? id,long? custid, string tsk)
        {
            var protask = new ProTaskViewModel
            {
                TaskCode = InvoiceNo(),
            };

            if (id != null)
            {

                var pro = db.ProTasks.Find(id);
                custid = pro.CustomerID;
                protask.CustomerID = pro.CustomerID;
                protask.Location = pro.Location;
                protask.ProjectId = (pro.ProjectId == null) ? 1 : pro.ProjectId;
                protask.TaskDetails = pro.TaskDetails;
                protask.TaskName = pro.TaskName;
                protask.SalesPerson = pro.SalesPerson;
                protask.SalesExecutive = pro.salesexecutive;

                protask.Ref1 = pro.Ref1;
                protask.Ref2 = pro.Ref2;
                protask.Ref3 = pro.Ref3;
                protask.Ref4 = pro.Ref4;
                protask.Ref5 = pro.Ref5;
                protask.VehicleType = pro.VTypId;
                protask.VehicleManufacturer = pro.VManuId;
                protask.VehicleModel = pro.VModId;
                protask.Lattitude = pro.Lattitude;
                protask.Longitude = pro.Longitude;
                protask.TaskType = pro.TaskType;


            }
            var cus = db.Customers.Where(o=>(custid==null||o.CustomerID==custid)).Take(1).Select(s => new { ID = s.CustomerID, Name = s.CustomerName });
            ViewBag.Customers = QkSelect.List(cus, "ID", "Name");

            var userpermission = User.IsInRole("All ProTask");
            var UserId = User.Identity.GetUserId();

            //     .Select(s => new
            //         Id=s.VTypeId,
            //         Name=s.Type
            //     })
            ViewBag.VehicleTypes = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Vehicle Type", Value = "0"},
                               }, "Value", "Text", 0);
            //    .Select(s => new
            //        Id = s.MId,
            //        Name = s.Manufacturer
            //    })
            ViewBag.VehicleManu = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Manufacturer", Value = "0"},
                               }, "Value", "Text", 0);

            //   .Select(s => new
            //       Id = s.ModelId,
            //       Name = s.Model
            //   })
            ViewBag.VehicleModels = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Model", Value = "0"},
                               }, "Value", "Text", 0);

            var use = (from c in db.Employees
                       join d in db.Users on c.UserId equals d.Id into usr
                       from d in usr.DefaultIfEmpty()
                       where d.Status == 1
                       select c)
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");


            var taskmanner = (from c in db.ProTaskManners
                             
                       select c)
                 .Select(s => new
                 {
                     ID = s.TaskTypeId,
                     Name = s.TypeName
                 })
                 .ToList();
            ViewBag.taskmanner = QkSelect.List(taskmanner, "ID", "Name");

            var project = db.Projects
                   .Select(s => new
                   {
                       ID = s.ProjectId,
                       Name = s.ProCode + " " + s.ProjectName
                   }).Where(o=>o.ID== 54625)
                   .ToList();
            ViewBag.Project = QkSelect.List(project, "ID", "Name");


            var ttype = db.ProTaskTypes.Select(r => new
            {
                ID = r.TaskTypeId,
                Name = r.TypeName
            }).ToList();
            ViewBag.tskType = QkSelect.List(ttype, "ID", "Name");

            ViewBag.PopUpAddCust = false;
            ViewBag.BusiType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();


            //  .Select(s => new
            //      ID = s.TaskStatusId,
            //      Name = s.StatusName
            //  })

            ViewBag.Stat = QkSelect.List(
                      new List<SelectListItem>
                      {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                      }, "Value", "Text", 0);
           

            //dummy viewbag
            //    ID = r.MobileNum,
            //    Name = r.MobileNum


            //  .Select(s => new
            //      ID = s.TeamId,
            //  })

            ViewBag.AssignTypes = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                 }, "Value", "Text", 0);

            var empl = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            })
                                         .ToList();
            ViewBag.Empl = QkSelect.List(empl, "ID", "Name");

            var assTo = db.Employees.Where(a => a.UserStatus == true)
               .Select(s => new
               {
                   ID = s.EmployeeId,
                   Name = s.FirstName + " " + s.LastName
               })
               .ToList();
            ViewBag.RAssignedTo = QkSelect.List(assTo, "ID", "Name");


            //field mapping
            protask.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();


            var ref1 = db.ProTasks.Where(o => o.Ref1 != null)
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.ProTasks
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");


            ViewBag.LastEntry = db.ProTasks.Where(p => (userpermission == true || p.CreatedBy == UserId)).Select(p => p.ProTaskId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var Reminders = db.EnableSettings.Where(a => a.EnableType == "RemindTask").FirstOrDefault();
            var Reminder = Reminders != null ? Reminders.Status : Status.inactive;
            ViewBag.Reminder = Reminder;
            var loc = db.ProTasks
                .Select(s => new
                {
                    ID = s.Location,
                    Name = s.Location
                }).Distinct().OrderBy(o => o.Name).ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");


            ViewBag.TskName = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                           }, "Value", "Text", 1);

            var tskimg = db.TaskImages.Where(a => a.Status == Status.inactive).ToList();
            string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/");
            foreach (var arr in tskimg)
            {

                try
                {
                    string[] splitlist = arr.FileName.Split('_');
                    if (splitlist.Length > 1)
                    {
                        string newpath = LegacyWeb.MapPath("~/uploads/protaskdocuments/" + splitlist[1]);
                        string filepath = LegacyWeb.MapPath("~/uploads/protaskdocuments/" + arr.FileName);
                        if (System.IO.File.Exists(newpath) && System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(Path.Combine(path, splitlist[1]));
                        }
                    }
                    TaskImage doc = db.TaskImages.Find(arr.TaskImageId);
                    doc.Status = Status.active;
                    db.Entry(doc).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch
                {
                    Exception ex;
                }

            }
            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();

            ViewBag.CountryCodes = db.Country.ToList();
            if (custid != null)
            {
                long rtask = 11;
                protask.CustomerID = custid;
                protask.SalesPerson = db.Customers.Where(o => o.CustomerID ==(long) custid).Select(o => o.SalesPerson).FirstOrDefault();
                protask.LstContacts = (from c in db.Contacts
                                       join cr in db.ContactRelation
                                         on c.ContactID
                                      equals cr.ContactID
                                       where (cr.RelationID == custid
                                       && cr.RelationType == 0)

                                       select new
                                       {

                                           ContactID = c.ContactID
                                            ,
                                           Name = c.Name
                                           ,
                                           FirstName = c.FirstName,
                                           LastName = c.LastName,
                                           Address = c.Address
                                           ,
                                           Country = c.Country
                                           ,
                                           State = c.State
                                           ,
                                           City = c.City
                                           ,
                                           Zip = c.Zip
                                           ,
                                           Phone = c.Phone
                                           ,
                                           Mobile = c.Mobile
                                           ,
                                           Fax = c.Fax
                                           ,
                                           EmailId =( c.EmailId==null)?"":c.EmailId
                                           ,
                                           Reference = c.Reference
                                           ,
                                           ContactPerson = c.ContactPerson
                                           ,
                                           Status = c.Status
                                           ,
                                           Group = c.Group
                                           ,
                                           SalesPMob = c.SalesPMob
                                           ,
                                           TypeOfContact = c.TypeOfContact
                                           ,
                                           Website = c.Website
                                           ,
                                           CountryID = c.CountryID
                                           ,
                                           ContactTypeID = c.ContactTypeID
                                       }).AsEnumerable().Select(x => new Contact
                                       {

                                           ContactID = x.ContactID,
                                           Name = x.Name
                                           ,
                                           FirstName = x.FirstName,
                                           LastName = x.LastName,
                                           Address = x.Address
                                           ,
                                           Country = x.Country
                                           ,
                                           State = x.State
                                           ,
                                           City = x.City
                                           ,
                                           Zip = x.Zip
                                           ,
                                           Phone = x.Phone
                                           ,
                                           Mobile = x.Mobile
                                           ,
                                           Fax = x.Fax
                                           ,
                                           EmailId = x.EmailId
                                           ,
                                           Reference = x.Reference
                                           ,
                                           ContactPerson = x.ContactPerson
                                           ,
                                           Status = x.Status
                                           ,
                                           Group = x.Group
                                           ,
                                           SalesPMob = x.SalesPMob
                                           ,
                                           TypeOfContact = x.TypeOfContact
                                           ,
                                           Website = x.Website
                                           ,
                                           CountryID = x.CountryID
                                           ,
                                           ContactTypeID = x.ContactTypeID
                                       }).ToList();
            }
            protask.StartDate = System.DateTime.Now.Date.ToString("dd-MM-yyyy");
         protask.StartTime= System.DateTime.Now;
            

            return View(protask);
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create ProTask")]
        [HttpPost]
        public ActionResult Create(ProTaskViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var Exists = db.ProTasks.Any(u => u.TaskName == vmodel.TaskName);
            if (Exists)
            {
                msg = "Task Name Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);


                    DateTime? sDate = null;
                    DateTime? eDate = null;
                    DateTime? stimes = null;
                    DateTime? etimes = null;
                    if (vmodel.StartDate != null)
                    {
                        sDate = DateTime.Parse(vmodel.StartDate.ToString(), new CultureInfo("en-GB"));
                        TimeSpan? stime = null;
                        if (vmodel.StartTime != null)
                        {
                            stime = ((DateTime)vmodel.StartTime).TimeOfDay;
                        }
                        stimes = sDate + stime;
                    }
                    if (vmodel.EndDate != null)
                    {
                        eDate = DateTime.Parse(vmodel.EndDate.ToString(), new CultureInfo("en-GB"));
                        TimeSpan? etime = null;
                        if (vmodel.EndTime != null)
                        {
                            etime = ((DateTime)vmodel.EndTime).TimeOfDay;
                        }
                        etimes = eDate + etime;
                    }

                    //seleted date added,for fullcalender


                    ProTask task = new ProTask();

                    task.TaskNo = GetProNo();
                    task.TaskCode = InvoiceNo();
                    task.TaskName = vmodel.TaskName;
                    task.ProjectId = vmodel.ProjectId;
                    task.CustomerID = vmodel.CustomerID;
                    task.TaskType = vmodel.TaskType;
                    task.StartDate = sDate;
                    task.StartTime = stimes;
                    task.EndDate = eDate;
                    task.EndTime = etimes;
                    task.Priority = vmodel.Priority;
                    // TaskStatus = TaskStatus.Created,
                    task.TaskDetails = vmodel.Note;
                    task.driver = vmodel.driver;
                    task.CreatedDate = today;
                    task.CreatedBy = UserId;
                    task.Status = Status.active;
                    task.Branch = BranchID;
                    task.Note = vmodel.Note != null ? vmodel.Note.ToString() : "";
                    task.Location = vmodel.Location;
                    task.TaskStatus = vmodel.TaskStatus;

                    task.SalesPerson = vmodel.SalesPerson;

                    task.Ref1 = vmodel.Ref1;
                    task.Ref2 = vmodel.Ref2;
                    task.Ref3 = vmodel.Ref3;
                    task.Ref4 = vmodel.Ref4;
                    task.Ref5 = vmodel.Ref5;
                    task.logtime = System.DateTime.Now;
                    task.VTypId = vmodel.VehicleType;
                    task.VManuId = vmodel.VehicleManufacturer;
                    task.VModId = vmodel.VehicleModel;
                    task.Lattitude = vmodel.Lattitude;
                    task.Longitude = vmodel.Longitude;
                    task.OpenClose = Convert.ToInt32(vmodel.OpenClose);
                    task.salesexecutive = vmodel.SalesExecutive;
                    if (vmodel.Ref5 != "" && vmodel.Ref5 != null)
                    {
                        var cord = ExtractCoordinates(vmodel.Ref5);
                        if (vmodel.Lattitude == "" || vmodel.Lattitude == null)
                        {
                         task.Lattitude = (string)cord["lat"];

                        }
                        if (vmodel.Longitude == "" || vmodel.Longitude == null)
                        {
                         task.Longitude = (string)cord["log"];

                        }
                    }
                    db.ProTasks.Add(task);
                    db.SaveChanges();
                    Int64 proId = task.ProTaskId;


                    if (vmodel.AssignTypeAll != null)
                    {
                        TaskAssignType tskax = new TaskAssignType();
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            tskax.ProTaskId = proId;
                            tskax.TeamId = arr;
                            db.TaskAssignTypes.Add(tskax);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.TaskManner != null)
                    {
                        AssignTaskManner tskax = new AssignTaskManner();
                        foreach (var arr in vmodel.TaskManner)
                        {
                            tskax.ProTaskId = proId;
                            tskax.TaskMannerId = arr;
                           
                            db.AssignTaskManners.Add(tskax);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.SuperWiser != null)
                    {
                        AssignTaskSupervisor tskax = new AssignTaskSupervisor();
                        foreach (var arr in vmodel.SuperWiser)
                        {
                            tskax.ProTaskId = proId;
                            tskax.EmployeeId= arr;
                            db.AssignTaskSupervisors.Add(tskax);
                            db.SaveChanges();
                        }
                    }
                    // create task updation
                    long? teamId = null;
                    //task updations
                    ProTaskUpdation TaskUp = new ProTaskUpdation
                    {
                        ProTaskId = proId,
                        //Status = TKUpdateStatus.Created,
                        CreatedBy = UserId,
                        CreatedDate = today,
                        //TaskTeamId = teamId
                    };
                    db.ProTaskUpdations.Add(TaskUp);
                    db.SaveChanges();
                    Int64 TaskUpdId = TaskUp.TaskUpdationID;







                    if (vmodel.LstContacts != null && vmodel.LstContacts.Count > 0)
                    {
                        foreach (var item in vmodel.LstContacts)
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














                    com.addlog(LogTypes.Created, UserId, "ProTask", "ProTasks", findip(), task.ProTaskId, "Task Created Successfully");

                    if (vmodel.AssignedMembers != null)
                    {
                        TaskAssigned tskass = new TaskAssigned();
                        foreach (var arr in vmodel.AssignedMembers)
                        {
                            tskass.ProTaskId = proId;
                            tskass.EmployeeId = arr;
                            tskass.Status = "Assigned";
                            tskass.AssignBy = UserId;
                            tskass.chkStatus = Status.active;
                            tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            db.TaskAssigneds.Add(tskass);
                            db.SaveChanges();
                            com.remideradd(rootdomain+"proTask/mytask", arr, UserId, "Task Assined",proId);
                        }
                    }

                    if (vmodel.mobmodel != null)
                    {
                        foreach (var arr in vmodel.mobmodel)
                        {
                            if (arr.Num != "null" && arr.Num != null)
                            {
                                var mob = new TaskMobile
                                {
                                    ProTaskId = proId,
                                    MobileNo = arr.Num,
                                    Name = arr.Name
                                };
                                db.TaskMobiles.Add(mob);
                                db.SaveChanges();
                            }
                        }
                    }



                    // file upload


                    // fileupload
                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {

                                var fileCount = db.TaskImages.Select(a => a.TaskImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName = Path.GetFileName(file.FileName);

                                String extension = Path.GetExtension(fileName);


                                String newName = fileCount + extension;
                                string newFName = fileCount + extension;
                                var FStatus = Status.active;

                                var thumbName = "";
                                var resizeName = "";
                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), newName);
                                file.SaveAs(newName);

                                var taskimg = new TaskImage
                                {
                                    ProTaskId = proId,
                                    TaskUpdationID = TaskUpdId,

                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                    CreatedBy = UserId,
                                };
                                db.TaskImages.Add(taskimg);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }


                    //------------reminder in task-------------------------------------------------

                    var Reminders = db.EnableSettings.Where(a => a.EnableType == "RemindTask").FirstOrDefault();
                    var Reminder = Reminders != null ? Reminders.Status : Status.inactive;
                    if (Reminder == Status.active)
                    {
                        Reminder reminds = new Reminder();
                        reminds.Reference = proId;
                        reminds.Note = vmodel.RemNote;
                        if (vmodel.RDate != null)
                        {
                            var rDate = DateTime.Parse(vmodel.RDate.ToString(), new CultureInfo("en-GB"));
                            //seleted date added,for fullcalender
                            TimeSpan time = (vmodel.RTime).TimeOfDay;
                            DateTime date = rDate + time;

                            reminds.RDate = date;
                        }

                        reminds.Type = "Task";
                        reminds.RStatus = "Open";

                        reminds.CreatedBy = UserId;
                        reminds.Status = Status.active;
                        reminds.CreatedDate = today;
                        db.Reminders.Add(reminds);
                        db.SaveChanges();
                        Int64 Id = reminds.ReminderId;

                        var Asby = vmodel.RemAssignedTo;
                        if (Asby != null && Asby.Length > 0)
                        {
                            ReminderAssigned remAs = new ReminderAssigned();
                            foreach (var emp in Asby)
                            {
                                remAs.ReminderId = Id;
                                remAs.EntryId = proId;
                                remAs.Type = "Task";
                                remAs.EmployeeId = emp;
                                db.ReminderAssigneds.Add(remAs);
                                db.SaveChanges();
                            }
                        }
                        com.addlog(LogTypes.Created, UserId, "Reminder", "Reminders", findip(), Id, "Reminder Added Successfully");
                        //---------------------------------------------------------------
                    }

                    msg = "Task details Updated Successfully.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
        }
        [HttpPost]
        public JsonResult addemployee(long? ProTaskId, ProTaskViewModel vmodel)
        {
            bool stat = false;
            string msg;
            ProTaskId = vmodel.ProTaskId;


            var empname = "";
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);

            if (vmodel.AssignedMembers != null)
            {
                var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == ProTaskId && a.Status == "Assigned" && a.chkStatus == Status.active).Select(a => a.EmployeeId).ToArray();
                var newusers = vmodel.AssignedMembers.ToArray();

                TaskAssigned tskass = new TaskAssigned();
                long flagg = 0;
                var usid = "";
                string em="";
                foreach (var chktim in newusers)
                {
                    var taskexists = db.TaskAssigneds.Any(a => a.Status == "Assigned" && a.chkStatus == Status.active&& a.EmployeeId==chktim);
                    if(taskexists)
                    {
                        em = em +" , "+ db.Employees.Where(o => o.EmployeeId == chktim).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();

                    }
                }
                if(em!="")
                {
                    msg = $"{em} is already assigned to other task ";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }

                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                //            Status = "Active",
                //            login = DateTime1,
                //            latitude = "",
                //            logitude = "",
                //            //   protaskid = 1,
                //        //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };




                foreach (var arr in vmodel.AssignedMembers)
                {                 
                    tskass.ProTaskId = (long)ProTaskId;
                    tskass.EmployeeId = arr;
                    tskass.Status = "Assigned";
                    tskass.AssignBy = UserId;
                    tskass.chkStatus = Status.active;
                    tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100);
                    db.TaskAssigneds.Add(tskass);
                    db.SaveChanges();
                    com.remideradd(rootdomain+"proTask/mytask", arr, UserId, "Task Assined",(long)ProTaskId);

                }
            }
            int flag = 0;
            var empass = db.TaskAssigneds.Where(o => o.ProTaskId == ProTaskId && o.chkStatus == 0 && o.Status == "Assigned" && o.AssignBy == UserId).ToList();

            //            EmployeeName = usid,
            //            protaskid = ProTaskId,
            //            Status = "Active",
            //            login = System.DateTime.Now,





            //        EmpAttDetails empdt = new EmpAttDetails
            //            protaskid = (long)ProTaskId,
            //            taskstatusid = 3,
            //            userid = usid,
            //            starttime = System.DateTime.Now,
            //            empattid = atttendance.Id,


 


            foreach (var emp in empass)
            {
                var usid = db.Employees.Where(o => o.EmployeeId == emp.EmployeeId).Select(o => o.UserId).FirstOrDefault();
                var empatt2 = db.EmpAttendances.Where(o => o.protaskid == null && o.logout == null && o.EmployeeName == usid).OrderByDescending(o => o.login).FirstOrDefault();

                if (empatt2 != null)
                {
                    flag = 1;
                    empatt2.protaskid = ProTaskId;
                    empatt2.login = today;
                    db.Entry(empatt2).State = EntityState.Modified;
                    db.SaveChanges();
                    var empattid = db.EmpAttendances.Where(o => o.protaskid == ProTaskId && o.EmployeeName == empatt2.EmployeeName).OrderByDescending(o => o.Id).FirstOrDefault();
                    //EmpAttDetails empdt = new EmpAttDetails
                    //    protaskid = (long)ProTaskId,
                    //    taskstatusid = 3,
                    //    userid = empatt2.EmployeeName,
                    //    starttime = System.DateTime.Now,
                    //    empattid = empattid.Id,

                }




            }
            if (flag == 1)
            {
                msg = "Task details Updated Successfully.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Task details Updated Successfully.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }
        public ActionResult addemployee(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTask pro = db.ProTasks.Find(id);
            if (pro == null)
            {
                return NotFound();
            }
            ProTaskViewModel vmodel = new ProTaskViewModel();

            vmodel.ProTaskId = id;
            var userid = User.Identity.GetUserId();
            vmodel.AssignedMembers = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId && a.chkStatus == Status.active && a.Status == "Assigned" && a.AssignBy == userid).Select(a => a.EmployeeId).ToList().ToArray() ?? null;
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");
            vmodel.AssignedMembers = null;
            ViewBag.team = new MultiSelectList(use, "ID", "Name", vmodel.AssignedMembers);

            var user = db.Users
                         .Select(s => new
                         {
                             ID = s.Id,
                             Name = s.UserName
                         })
                         .ToList();
            ViewBag.users = QkSelect.List(user, "ID", "Name");







            var empp = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Empl = QkSelect.List(empp, "ID", "Name");
            var userpermission = User.IsInRole("All ProTask");
            var UserId = User.Identity.GetUserId();





            return PartialView(vmodel);
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,My ProTask")]
        public ActionResult Assign(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTask pro = db.ProTasks.Find(id);
            if (pro == null)
            {
                return NotFound();
            }
            ProTaskViewModel vmodel = new ProTaskViewModel();
            vmodel.TaskCode = pro.TaskCode;
            vmodel.Location = pro.Location;
            vmodel.TaskDetails = pro.TaskDetails;
            vmodel.TaskName = pro.TaskName;
            vmodel.ProTaskId = pro.ProTaskId;
            vmodel.CustomerID = pro.CustomerID;
            vmodel.ProjectId = pro.ProjectId;
            vmodel.Note = pro.Note;
            vmodel.Priority = pro.Priority;
            vmodel.StartDate = (pro.StartDate != null) ? ((DateTime)pro.StartDate).ToString("dd-MM-yyyy") : "";
            vmodel.StartTime = pro.StartTime;
            vmodel.TaskStatus = pro.TaskStatus;
            vmodel.TaskType = pro.TaskType;

            vmodel.Ref1 = pro.Ref1;
            vmodel.Ref2 = pro.Ref2;
            vmodel.Ref3 = pro.Ref3;
            vmodel.Ref4 = pro.Ref4;
            vmodel.Ref5 = pro.Ref5;


            vmodel.AssignedMembers = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId && a.chkStatus == Status.active && a.Status == "Assigned").Select(a => a.EmployeeId).ToList().ToArray() ?? null;
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");
            ViewBag.team = new MultiSelectList(use, "ID", "Name", vmodel.AssignedMembers);

            var user = db.Users
                         .Select(s => new
                         {
                             ID = s.Id,
                             Name = s.UserName
                         })
                         .ToList();
            ViewBag.users = QkSelect.List(user, "ID", "Name");
            var pstat = db.TaskStatus
             .Select(s => new
             {
                 ID = s.TaskStatusId,
                 Name = s.StatusName
             })
             .ToList();
            ViewBag.Stat = QkSelect.List(pstat, "ID", "Name");

            var ttype = db.ProTaskTypes.Select(r => new
            {
                ID = r.TaskTypeId,
                Name = r.TypeName
            }).ToList();
            ViewBag.tskType = QkSelect.List(ttype, "ID", "Name");


            var atype = db.TaskAssignTypes.Where(a => a.ProTaskId == id).Select(a => a.TeamId).ToArray();
            var asstype = db.Teams
              .Select(s => new
              {
                  ID = s.TeamId,
                  Name = s.TeamName,
              })
              .ToList();
            ViewBag.AssignTypes = new MultiSelectList(asstype, "ID", "Name", atype);
            var empp = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Empl = QkSelect.List(empp, "ID", "Name");
            var userpermission = User.IsInRole("All ProTask");
            var UserId = User.Identity.GetUserId();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();


            var ref1 = db.ProTasks
     .Select(s => new
     {
         ID = s.Ref1,
         Name = s.Ref1
     }).Distinct().OrderBy(a => a.Name).ToList();
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", pro.Ref1);

            var ref2 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct().OrderBy(a => a.Name)
             .ToList();
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", pro.Ref2);

            var ref3 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct().OrderBy(a => a.Name)
             .ToList();
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", pro.Ref3);

            var ref4 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct().OrderBy(a => a.Name)
             .ToList();
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", pro.Ref4);

            var ref5 = db.ProTasks
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct().OrderBy(a => a.Name)
            .ToList();
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", pro.Ref5);

            return PartialView(vmodel);
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,My ProTask")]
        [HttpPost]
        public JsonResult Assign(long? id, ProTaskViewModel vmodel)
        {
            bool stat = false;
            string msg;

            if (1==1)
            {
                ProTask pro = db.ProTasks.Find(id);

                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);
                DateTime? sDate = null;
                DateTime? stimes = null;
                if (vmodel.StartDate != null)
                {
                    sDate = DateTime.Parse(vmodel.StartDate.ToString(), new CultureInfo("en-GB"));
                    TimeSpan? stime = null;
                    if (vmodel.StartTime != null)
                    {
                        stime = ((DateTime)vmodel.StartTime).TimeOfDay;
                    }
                    stimes = sDate + stime;
                }

                //seleted date added,for fullcalender
                ProTask task = db.ProTasks.Find(id);
                task.TaskType = vmodel.TaskType;
                task.Priority = vmodel.Priority;
                task.TaskStatus = vmodel.TaskStatus;
                task.OpenClose = 0;
                task.Ref1 = vmodel.Ref1;
                task.Ref2 = vmodel.Ref2;
                task.Ref3 = vmodel.Ref3;
                task.Ref4 = vmodel.Ref4;
                task.Ref5 = vmodel.Ref5;

                db.Entry(task).State = EntityState.Modified;
                db.SaveChanges();
                Int64 proId = task.ProTaskId;


                var tskx = db.TaskAssignTypes.Where(a => a.ProTaskId == pro.ProTaskId);
                if (tskx != null)
                {
                    db.TaskAssignTypes.RemoveRange(db.TaskAssignTypes.Where(a => a.ProTaskId == pro.ProTaskId));
                    db.SaveChanges();
                }
                if (vmodel.AssignTypeAll != null)
                {
                    TaskAssignType tskax = new TaskAssignType();
                    foreach (var arr in vmodel.AssignTypeAll)
                    {
                        tskax.ProTaskId = proId;
                        tskax.TeamId = arr;
                        db.TaskAssignTypes.Add(tskax);
                        db.SaveChanges();
                    }
                }

                // create task updation
                long? teamId = null;
                com.updateprotaskdate(task.ProTaskId);
                com.addlog(LogTypes.Updated, UserId, "ProTask", "ProTasks", findip(), task.ProTaskId, "Task Updated Successfully");

                var tstatus = db.ProTaskUpdations.Where(a => a.ProTaskId == pro.ProTaskId);
                if (tstatus != null)
                {
                    db.ProTaskUpdations.RemoveRange(db.ProTaskUpdations.Where(a => a.ProTaskId == pro.ProTaskId));
                    db.SaveChanges();
                }


                var skstat = GetStatusName(task.TaskStatus);
                ProTaskUpdation TaskUps = new ProTaskUpdation
                {
                    ProTaskId = proId,
                    //Status = TKUpdateStatus.Assigned,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    //TaskTeamId = teamId
                    Remarks = skstat + " Change To " + skstat,
                };
                db.ProTaskUpdations.Add(TaskUps);
                db.SaveChanges();
                Int64 TaskUpdId = TaskUps.TaskUpdationID;
                // file 


                if (1 == 1)
                {
                    long[] noempt = { 0 };
                    var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId && a.Status == "Assigned" && a.chkStatus == Status.active).Select(a => a.EmployeeId).ToArray();
                    var newusers = vmodel.AssignedMembers != null ? vmodel.AssignedMembers.ToArray() : noempt;

                    TaskAssigned tskass = new TaskAssigned();

                    foreach (var arr in tskasgn)
                    {
                        var taskassId = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId && a.Status == "Assigned" && a.chkStatus == Status.active && a.EmployeeId == arr).Select(a => a.TaskAssignedId).FirstOrDefault();
                        TaskAssigned tskassr = db.TaskAssigneds.Find(taskassId);
                        if (!newusers.Contains(arr))
                        {

                            tskassr.chkStatus = Status.inactive;
                            db.Entry(tskassr).State = EntityState.Modified;
                            db.SaveChanges();

                            tskass.ProTaskId = proId;
                            tskass.EmployeeId = arr;
                            tskass.Status = "Removed";
                            tskass.AssignBy = UserId;
                            tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100); ;
                            db.TaskAssigneds.Add(tskass);
                            db.SaveChanges();
                        }
                        else
                        {
                            tskassr.chkStatus = Status.inactive;
                            db.Entry(tskassr).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        var Userid = db.Employees.Where(o => o.EmployeeId == arr).Select(o => o.UserId).FirstOrDefault();
                        var maxo = from a in db.EmpAttendances
                                   where a.EmployeeName == Userid
                                   orderby a.login descending
                                   select a.Id;
                        var lastid = maxo.FirstOrDefault();






                    }
                    if (vmodel.AssignedMembers != null)
                    {
                        foreach (var arr in vmodel.AssignedMembers)
                        {
                            tskass.ProTaskId = proId;
                            tskass.EmployeeId = arr;
                            tskass.Status = "Assigned";
                            tskass.AssignBy = UserId;
                            tskass.chkStatus = Status.active;
                            tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100);
                            db.TaskAssigneds.Add(tskass);
                            db.SaveChanges();
                            com.remideradd(rootdomain+"proTask/mytask", arr, UserId, "Task Assined",proId);


                        }
                    }
                }
                msg = "Task details Updated Successfully.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit ProTask")]
        public ActionResult Edit(long? id, long? custid)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTask pro = db.ProTasks.Find(id);

            if (pro == null)
            {
                return NotFound();
            }

            ProTaskViewModel vmodel = new ProTaskViewModel();

            vmodel.TaskCode = pro.TaskCode;
            vmodel.CustomerID = pro.CustomerID;
            vmodel.Location = pro.Location;
            vmodel.ProjectId = (pro.ProjectId==null)?1:pro.ProjectId;
            vmodel.TaskDetails = pro.TaskDetails;
            vmodel.TaskName = pro.TaskName;
            vmodel.ProTaskId = pro.ProTaskId;
            vmodel.Note = pro.Note;
            vmodel.Priority = pro.Priority;
            vmodel.StartDate = (pro.StartDate != null) ? ((DateTime)pro.StartDate).ToString("dd-MM-yyyy") : "";
            vmodel.StartTime = pro.StartTime;
            vmodel.EndDate = (pro.EndDate != null) ? ((DateTime)pro.EndDate).ToString("dd-MM-yyyy") : "";
            vmodel.EndTime = pro.EndTime;
            vmodel.TaskStatus = pro.TaskStatus;
            vmodel.driver = pro.driver;
            vmodel.SalesPerson = pro.SalesPerson;
            vmodel.SalesExecutive = pro.salesexecutive;
            vmodel.TaskManner = db.AssignTaskManners.Where(a => a.ProTaskId == pro.ProTaskId).Select(a => a.TaskMannerId).ToList().ToArray() ?? null;
            vmodel.SuperWiser = db.AssignTaskSupervisors.Where(a => a.ProTaskId == pro.ProTaskId).Select(a => a.EmployeeId).ToList().ToArray() ?? null;

            vmodel.Ref1 = pro.Ref1;
            vmodel.Ref2 = pro.Ref2;
            vmodel.Ref3 = pro.Ref3;
            vmodel.Ref4 = pro.Ref4;
            vmodel.Ref5 = pro.Ref5;
            vmodel.VehicleType = pro.VTypId;
            vmodel.VehicleManufacturer = pro.VManuId;
            vmodel.VehicleModel = pro.VModId;
            vmodel.Lattitude = pro.Lattitude;
            vmodel.Longitude = pro.Longitude;
            vmodel.OpenClose = pro.OpenClose;
            vmodel.TaskType = pro.TaskType;
            var UserId = User.Identity.GetUserId();
            vmodel.AssignedMembers = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId && a.chkStatus == Status.active && a.Status == "Assigned").Select(a => a.EmployeeId).ToList().ToArray() ?? null;



            //            .Select(s => new
            //                ID = s.MobileNum,
            //                Name = s.MobileNum
            //            })
            //    //dummy viewbag
            //        ID = r.MobileNum,
            //        Name = r.MobileNum
            if (vmodel.VehicleType == null)
            {
                ViewBag.VehicleTypes = QkSelect.List(
                                   new List<SelectListItem>
                                   {
                                        new SelectListItem { Selected = false, Text = "Select the Vehicle Type", Value = "0"},
                                   }, "Value", "Text", 0);
            }
            else
            {
                var temp = db.VehicleTypes
                     .Select(s => new
                     {
                         Id = s.VTypeId,
                         Name = s.Type
                     })
                     .ToList();
                ViewBag.VehicleTypes = QkSelect.List(temp, "Id", "Name");
            }

            if (vmodel.VehicleManufacturer == null)
            {
                ViewBag.VehicleManu = QkSelect.List(
                                   new List<SelectListItem>
                                   {
                                        new SelectListItem { Selected = false, Text = "Select the Vehicle Type", Value = "0"},
                                   }, "Value", "Text", 0);
            }
            else
            {
                var temp1 = db.VehicleManufacturers
                    .Select(s => new
                    {
                        Id = s.MId,
                        Name = s.Manufacturer
                    })
                    .ToList();
                ViewBag.VehicleManu = QkSelect.List(temp1, "Id", "Name");
            }
            if (vmodel.VehicleModel == null)
            {
                ViewBag.VehicleModels = QkSelect.List(
                                   new List<SelectListItem>
                                   {
                                        new SelectListItem { Selected = false, Text = "Select the Vehicle Type", Value = "0"},
                                   }, "Value", "Text", 0);
            }
            else
            {
                var temp2 = db.VehicleModels
               .Select(s => new
               {
                   Id = s.ModelId,
                   Name = s.Model
               })
               .ToList();
                ViewBag.VehicleModels = QkSelect.List(temp2, "Id", "Name");
            }

            ViewBag.BusiType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();


            var use = (from c in db.Employees
                       join d in db.Users on c.UserId equals d.Id into usr
                       from d in usr.DefaultIfEmpty()
                       where d.Status == 1
                       select c)
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");
            ViewBag.team = new MultiSelectList(use, "ID", "Name", vmodel.AssignedMembers);

            var user = db.Users
                         .Select(s => new
                         {
                             ID = s.Id,
                             Name = s.UserName
                         })
                         .ToList();
            ViewBag.users = QkSelect.List(user, "ID", "Name");

            if (pro.CustomerID == -2)
            {
                ViewBag.Customers = QkSelect.List(
                          new List<SelectListItem>
                          {
                                                new SelectListItem { Selected = true, Text = "--No Customers--", Value = "-2"},
                          }, "Value", "Text", 0);
            }
            else
            {
                long? actcustomerid = vmodel.CustomerID;
                if (custid != null)
                {
                    actcustomerid = custid;
             }
                var cus = db.Customers.Where(o=>o.CustomerID==actcustomerid)
                  .Select(s => new
                  {
                      CustomerID = s.CustomerID,
                      CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                  }).ToList();
                ViewBag.Customers = QkSelect.List(cus, "CustomerID", "CustomerDetails");
            }
            var taskmanner = (from c in db.ProTaskManners

                              select c)
               .Select(s => new
               {
                   ID = s.TaskTypeId,
                   Name = s.TypeName
               })
               .ToList();


            ViewBag.taskmanners = new MultiSelectList(taskmanner, "ID", "Name",vmodel.TaskManner);
            var supervisers = (from c in db.Employees

                              select c)
               .Select(s => new
               {
                   ID = s.EmployeeId,
                   Name = s.FirstName + " " + s.LastName
               })
               .ToList();


            ViewBag.SuperWisers = new MultiSelectList(supervisers, "ID", "Name", vmodel.SuperWiser);
            var project = db.Projects.Where(o=>o.ProjectId==vmodel.ProjectId)
                   .Select(s => new
                   {
                       ID = s.ProjectId,
                       Name = s.ProCode + " " + s.ProjectName
                   })
                   .ToList();
            ViewBag.Project = QkSelect.List(project, "ID", "Name");

            var pstat = db.TaskStatus
             .Select(s => new
             {
                 ID = s.TaskStatusId,
                 Name = s.StatusName
             })
             .ToList();
            ViewBag.Stat = QkSelect.List(pstat, "ID", "Name");

            var ttype = db.ProTaskTypes.Select(r => new
            {
                ID = r.TaskTypeId,
                Name = r.TypeName
            }).ToList();
            ViewBag.tskType = QkSelect.List(ttype, "ID", "Name");


            var atype = db.TaskAssignTypes.Where(a => a.ProTaskId == id).Select(a => a.TeamId).ToArray();
            var asstype = db.Teams
              .Select(s => new
              {
                  ID = s.TeamId,
                  Name = s.TeamName,
              })
              .ToList();
            ViewBag.AssignTypes = new MultiSelectList(asstype, "ID", "Name", atype);



            ViewBag.TskName = QkSelect.List(
               new List<SelectListItem>
               {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
               }, "Value", "Text", 1);

            ViewBag.image = (from b in db.TaskImages
                             join c in db.ProTasks.Where(o=>o.Branch==1) on b.ProTaskId equals c.ProTaskId
                             where c.ProTaskId == id
                             select new TaskImageViewModel
                             {
                                 TaskImageId = b.TaskImageId,
                                 TaskId = b.ProTaskId,
                                 FileName = b.FileName,
                                 TaskName = c.TaskName
                             }).ToList();

            var empp = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Empl = QkSelect.List(empp, "ID", "Name");

            var loc = db.ProTasks
                .Select(s => new
                {
                    ID = s.Location,
                    Name = s.Location
                }).Distinct().ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");

            ViewBag.PopUpAddCust = false;

            var userpermission = User.IsInRole("All ProTask");
            UserId = User.Identity.GetUserId();

            ViewBag.preEntry = db.ProTasks.Where(a => a.ProTaskId < id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ProTaskId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.ProTasks.Where(a => a.ProTaskId > id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ProTaskId).DefaultIfEmpty().Min();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;


            var ref1 = db.ProTasks.Where(o=>o.Ref1!=null)
                 .Select(s => new
                 {
                     ID = s.Ref1,
                     Name = s.Ref1
                 }).Distinct()
                 .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", pro.Ref1);

            var ref2 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", pro.Ref2);

            var ref3 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", pro.Ref3);

            var ref4 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", pro.Ref4);

            var ref5 = db.ProTasks
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", pro.Ref5);
            long rtask = 11;
            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();

            ViewBag.CountryCodes = db.Country.ToList();
            if (custid == null)
            {

                vmodel.LstContacts = (from c in db.Contacts
                                      join cr in db.ContactRelation
                                        on new { c.ContactID, RelationType = rtask }
                                     equals new { cr.ContactID, cr.RelationType }
                                      where (cr.RelationID == id)
                                      select new
                                      {

                                          ContactID = c.ContactID
                                           ,
                                          Name = c.Name
                                          ,
                                          FirstName = c.FirstName,
                                          LastName = c.LastName,
                                          Address = c.Address
                                          ,
                                          Country = c.Country
                                          ,
                                          State = c.State
                                          ,
                                          City = c.City
                                          ,
                                          Zip = c.Zip
                                          ,
                                          Phone = c.Phone
                                          ,
                                          Mobile = c.Mobile
                                          ,
                                          Fax = c.Fax
                                          ,
                                          EmailId = c.EmailId
                                          ,
                                          Reference = c.Reference
                                          ,
                                          ContactPerson = c.ContactPerson
                                          ,
                                          Status = c.Status
                                          ,
                                          Group = c.Group
                                          ,
                                          SalesPMob = c.SalesPMob
                                          ,
                                          TypeOfContact = c.TypeOfContact
                                          ,
                                          Website = c.Website
                                          ,
                                          CountryID = c.CountryID
                                          ,
                                          ContactTypeID = c.ContactTypeID
                                      }).AsEnumerable().Select(x => new Contact
                                      {

                                          ContactID = x.ContactID,
                                          Name = x.Name
                                          ,
                                          FirstName = x.FirstName,
                                          LastName = x.LastName,
                                          Address = x.Address
                                          ,
                                          Country = x.Country
                                          ,
                                          State = x.State
                                          ,
                                          City = x.City
                                          ,
                                          Zip = x.Zip
                                          ,
                                          Phone = x.Phone
                                          ,
                                          Mobile = x.Mobile
                                          ,
                                          Fax = x.Fax
                                          ,
                                          EmailId = x.EmailId
                                          ,
                                          Reference = x.Reference
                                          ,
                                          ContactPerson = x.ContactPerson
                                          ,
                                          Status = x.Status
                                          ,
                                          Group = x.Group
                                          ,
                                          SalesPMob = x.SalesPMob
                                          ,
                                          TypeOfContact = x.TypeOfContact
                                          ,
                                          Website = x.Website
                                          ,
                                          CountryID = x.CountryID
                                          ,
                                          ContactTypeID = x.ContactTypeID
                                      }).ToList();
            }
            else
            {
                if (custid != null)
                {

                    vmodel.CustomerID = custid;
                    vmodel.LstContacts = (from c in db.Contacts
                                          join cr in db.ContactRelation
                                            on c.ContactID
                                         equals cr.ContactID
                                          where (cr.RelationID == custid)
                                          select new
                                          {

                                              ContactID = c.ContactID
                                               ,
                                              Name = c.Name
                                              ,
                                              FirstName = c.FirstName,
                                              LastName = c.LastName,
                                              Address = c.Address
                                              ,
                                              Country = c.Country
                                              ,
                                              State = c.State
                                              ,
                                              City = c.City
                                              ,
                                              Zip = c.Zip
                                              ,
                                              Phone = c.Phone
                                              ,
                                              Mobile = c.Mobile
                                              ,
                                              Fax = c.Fax
                                              ,
                                              EmailId = c.EmailId
                                              ,
                                              Reference = c.Reference
                                              ,
                                              ContactPerson = c.ContactPerson
                                              ,
                                              Status = c.Status
                                              ,
                                              Group = c.Group
                                              ,
                                              SalesPMob = c.SalesPMob
                                              ,
                                              TypeOfContact = c.TypeOfContact
                                              ,
                                              Website = c.Website
                                              ,
                                              CountryID = c.CountryID
                                              ,
                                              ContactTypeID = c.ContactTypeID
                                          }).AsEnumerable().Select(x => new Contact
                                          {

                                              ContactID = x.ContactID,
                                              Name = x.Name
                                              ,
                                              FirstName = x.FirstName,
                                              LastName = x.LastName,
                                              Address = x.Address
                                              ,
                                              Country = x.Country
                                              ,
                                              State = x.State
                                              ,
                                              City = x.City
                                              ,
                                              Zip = x.Zip
                                              ,
                                              Phone = x.Phone
                                              ,
                                              Mobile = x.Mobile
                                              ,
                                              Fax = x.Fax
                                              ,
                                              EmailId = x.EmailId
                                              ,
                                              Reference = x.Reference
                                              ,
                                              ContactPerson = x.ContactPerson
                                              ,
                                              Status = x.Status
                                              ,
                                              Group = x.Group
                                              ,
                                              SalesPMob = x.SalesPMob
                                              ,
                                              TypeOfContact = x.TypeOfContact
                                              ,
                                              Website = x.Website
                                              ,
                                              CountryID = x.CountryID
                                              ,
                                              ContactTypeID = x.ContactTypeID
                                          }).ToList();
                }
            }
            if (vmodel.OpenClose == 0)
            {

                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
                ViewBag.OpnCls = pstat2;

            }
            else if (vmodel.OpenClose == 1)
            {

                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                }
              };
                ViewBag.OpnCls = pstat2;
            }
            else
            {
                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.OpnCls = pstat2;
            }
            
            return View(vmodel);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit ProTask")]
        [HttpPost]
        public ActionResult Edit(long? id, ProTaskViewModel vmodel)
        {
            bool stat = false;
            string msg;

            var Exists = db.ProTasks.Any(u => u.TaskName == vmodel.TaskName && u.ProTaskId != id);
            if (Exists)
            {
                msg = "Task Name Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {

                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ProTask pro = db.ProTasks.Find(id);

                if (pro == null)
                {
                    return NotFound();
                }
                string oldtaskname = pro.TaskName;
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    DateTime? sDate = null;
                    DateTime? eDate = null;
                    DateTime? stimes = null;
                    DateTime? etimes = null;
                    if (vmodel.StartDate != null)
                    {
                        sDate = DateTime.Parse(vmodel.StartDate.ToString(), new CultureInfo("en-GB"));
                        TimeSpan? stime = null;
                        if (vmodel.StartTime != null)
                        {
                            stime = ((DateTime)vmodel.StartTime).TimeOfDay;
                        }
                        stimes = sDate + stime;
                    }
                    if (vmodel.EndDate != null)
                    {
                        eDate = DateTime.Parse(vmodel.EndDate.ToString(), new CultureInfo("en-GB"));
                        TimeSpan? etime = null;
                        if (vmodel.EndTime != null)
                        {
                            etime = ((DateTime)vmodel.EndTime).TimeOfDay;
                        }
                        etimes = eDate + etime;
                    }




                    //seleted date added,for fullcalender
                    ProTask task = db.ProTasks.Find(id);
                    task.driver = vmodel.driver;
                    task.TaskName = vmodel.TaskName;
                    task.ProjectId = vmodel.ProjectId;
                    task.TaskType = vmodel.TaskType;
                    task.StartDate = sDate;
                    task.StartTime = stimes;
                    task.EndDate = eDate;
                    task.EndTime = etimes;
                    task.Priority = vmodel.Priority;
                    task.TaskStatus = vmodel.TaskStatus;
                    task.Location = vmodel.Location;

                    task.SalesPerson = vmodel.SalesPerson;
                    task.CustomerID = vmodel.CustomerID;
                    task.VTypId = vmodel.VehicleType;
                    task.VManuId = vmodel.VehicleManufacturer;
                    task.VModId = vmodel.VehicleModel;

                    task.Ref1 = vmodel.Ref1;
                    task.Ref2 = vmodel.Ref2;
                    task.Ref3 = vmodel.Ref3;
                    task.Ref4 = vmodel.Ref4;
                    task.Ref5 = vmodel.Ref5;

                    task.TaskDetails = vmodel.Note;
                    task.Note = vmodel.Note != null ? vmodel.Note.ToString() : "";
                    task.salesexecutive = vmodel.SalesExecutive;

                    var oldtskstat = GetStatusName(task.TaskStatus);
                    var newtskstat = GetStatusName(vmodel.TaskStatus);
                    task.logtime = System.DateTime.Now;
                    task.Lattitude = vmodel.Lattitude;
                    task.Longitude = vmodel.Longitude;
                    task.OpenClose = vmodel.OpenClose;
                    if(task.VManuId==999&&task.VModId!=null)
                    {
                        task.VManuId = 998;
                    }
                    if (vmodel.Ref5 != "" && vmodel.Ref5 != null)
                    {
                        try
                        {
                            var cord = ExtractCoordinates(vmodel.Ref5);
                            if (vmodel.Lattitude == "" || vmodel.Lattitude == null)
                            {
                                task.Lattitude = (string)cord["lat"];

                            }
                            if (vmodel.Longitude == "" || vmodel.Longitude == null)
                            {
                                task.Longitude = (string)cord["log"];

                            }
                        }
                        catch(Exception e)
                        {

                        }
                    }
                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();
                    db.Reminders.RemoveRange(db.Reminders.Where(o => o.Note.Contains("12 Hours Task Still ")));
                    db.SaveChanges();
                    db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.Type == "TaskStillpending"));
                    db.SaveChanges();
                    var emps = db.Employees.Where(o => o.appaccessonly == true).Select(o => o.EmployeeId).ToList().ToArray();

                    var rem = (from a in db.ProTasks
                               join b in db.TaskAssigneds on a.ProTaskId equals b.ProTaskId
                               join c in db.TaskStatus on a.TaskStatus equals c.TaskStatusId
                               where (a.Ref1 == "ASSIGNED" || a.Ref1 == "APPOINTED") && a.OpenClose == 0
                               && EF.Functions.DateDiffHour(a.logtime, a.StartTime) < -12


                                 && b.Status == "Assigned" && b.chkStatus == Status.active

                               select new
                               {
                                   b.EmployeeId,
                                   a.ProTaskId,
                                   c.StatusName,
                                   taskname = a.TaskCode + "-" + a.TaskName,
                                   a.Ref1
                               }).Distinct();

                    if (rem.Count() > 0)
                    {
                        var pids = rem.Select(o => new
                        {
                            o.ProTaskId,
                            o.StatusName,
                            o.taskname,
                            o.Ref1

                        }).Distinct().ToList();
                        foreach (var pid in pids)
                        {
                            string tasknote = "12 Hours Task Still " + pid.Ref1 + " <br> Task Status : " + pid.StatusName + "<br> Task name : " + pid.taskname;
                            var remexist = db.Reminders.Any(o => o.Note == tasknote && o.Reference == pid.ProTaskId);
                            if (!remexist)
                            {
                                Reminder reminds = new Reminder();
                                reminds.Reference = pid.ProTaskId;
                                reminds.Note = tasknote;// "Task Still " +pid.Ref1+" <br> Task Status : " + pid.StatusName + "<br> Task name : " + pid.taskname;

                                var rDate = System.DateTime.Now.Date;
                                //seleted date added,for fullcalender



                                reminds.RDate = System.DateTime.Now;
                                reminds.Type = "/proTask/Details/" + pid.ProTaskId;
                                reminds.RStatus = "Close";
                                reminds.RequestBy = UserId;

                                reminds.CreatedBy = UserId;
                                reminds.Status = Status.active;
                                reminds.CreatedDate = System.DateTime.Now;
                                db.Reminders.Add(reminds);
                                db.SaveChanges();
                                long Id = reminds.ReminderId;
                                var asseimp = rem.Where(o => o.ProTaskId == pid.ProTaskId).Select(o => o.EmployeeId).Distinct().ToList().ToArray();
                                asseimp = asseimp.Concat(emps).ToArray();
                                var myemps = asseimp.Distinct().ToList().ToArray();
                                foreach (var arr in myemps)
                                {

                                    var exists = db.ReminderAssigneds.Any(o => o.EntryId == pid.ProTaskId && o.Type == "TaskStillpending" && o.EmployeeId == arr);



                                    if (!exists)
                                    {
                                        ReminderAssigned remAs = new ReminderAssigned();

                                        remAs.ReminderId = Id;
                                        remAs.EntryId = pid.ProTaskId;
                                        remAs.Type = "TaskStillpending";
                                        remAs.EmployeeId = arr;
                                        db.ReminderAssigneds.Add(remAs);
                                        db.SaveChanges();

                                    }
                                }
                            }

                        }
                    }

                    if (vmodel.Ref1=="OLD")
                    {
                        db.Reminders.RemoveRange(db.Reminders.Where(o => o.Reference == id&&o.Note.Contains("Task Notification")));
                        db.SaveChanges();
                        db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.EntryId == id && o.Type == "tasknotification"));
                        db.SaveChanges();
                    }
                    if (vmodel.OpenClose == 1)
                    {
                        db.Reminders.RemoveRange(db.Reminders.Where(o => o.Reference == id && o.Note.Contains("Task Notification")));
                        db.SaveChanges();
                        db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.EntryId == id && o.Type == "tasknotification"));
                        db.SaveChanges();
                    }
                    if (vmodel.TaskStatus ==1||vmodel.TaskStatus==2)
                    {
                        db.Reminders.RemoveRange(db.Reminders.Where(o => o.Reference == id && o.Note.Contains("Task Notification")));
                        db.SaveChanges();
                        db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.EntryId == id && o.Type == "tasknotification"));
                        db.SaveChanges();
                    }
                    Int64 proId = task.ProTaskId;
                    var assman = db.AssignTaskManners.Where(a => a.ProTaskId == pro.ProTaskId);
                    if (assman != null)
                    {
                        db.AssignTaskManners.RemoveRange(db.AssignTaskManners.Where(a => a.ProTaskId == pro.ProTaskId));
                        db.SaveChanges();
                    }
                    if (vmodel.TaskManner != null)
                    {
                        AssignTaskManner tskax = new AssignTaskManner();
                        foreach (var arr in vmodel.TaskManner)
                        {
                            tskax.ProTaskId = proId;
                            tskax.TaskMannerId = arr;
                            db.AssignTaskManners.Add(tskax);
                            db.SaveChanges();
                        }
                    }
                    var assusp = db.AssignTaskSupervisors.Where(a => a.ProTaskId == pro.ProTaskId);
                    if (assusp != null)
                    {
                        db.AssignTaskSupervisors.RemoveRange(db.AssignTaskSupervisors.Where(a => a.ProTaskId == pro.ProTaskId));
                        db.SaveChanges();
                    }
                    if (vmodel.SuperWiser != null)
                    {
                        AssignTaskSupervisor tskax = new AssignTaskSupervisor();
                        foreach (var arr in vmodel.SuperWiser)
                        {
                            tskax.ProTaskId = proId;
                            tskax.EmployeeId = arr;
                            db.AssignTaskSupervisors.Add(tskax);
                            db.SaveChanges();
                        }
                    }
                    var tskx = db.TaskAssignTypes.Where(a => a.ProTaskId == pro.ProTaskId);
                    if (tskx != null)
                    {
                        db.TaskAssignTypes.RemoveRange(db.TaskAssignTypes.Where(a => a.ProTaskId == pro.ProTaskId));
                        db.SaveChanges();
                    }
               
                    if (vmodel.AssignTypeAll != null)
                    {
                        TaskAssignType tskax = new TaskAssignType();
                        foreach (var arr in vmodel.AssignTypeAll)
                        {
                            tskax.ProTaskId = proId;
                            tskax.TeamId = arr;
                            db.TaskAssignTypes.Add(tskax);
                            db.SaveChanges();
                        }
                    }

                    var mobil = db.TaskMobiles.Where(a => a.ProTaskId == pro.ProTaskId);
                    if (mobil != null)
                    {
                        db.TaskMobiles.RemoveRange(db.TaskMobiles.Where(a => a.ProTaskId == pro.ProTaskId));
                        db.SaveChanges();
                    }
                    if (vmodel.mobmodel != null)
                    {
                        foreach (var arr in vmodel.mobmodel)
                        {
                            if (arr.Num != "null" && arr.Num != null)
                            {
                                var mob = new TaskMobile
                                {
                                    ProTaskId = proId,
                                    MobileNo = arr.Num.TrimStart('0'),
                                    Name = arr.Name
                                };
                                db.TaskMobiles.Add(mob);
                                db.SaveChanges();
                            }
                        }
                    }

                    // create task updation
                    long? teamId = null;
                    com.updateprotaskdate(task.ProTaskId);

                    if (vmodel.TaskName == oldtaskname)
                        com.addlog(LogTypes.Updated, UserId, "ProTask", "ProTasks", findip(), task.ProTaskId, "Task Updated Successfully");
                    else
                        com.addlog(LogTypes.Updated, UserId, "ProTask", "ProTasks", findip(), task.ProTaskId, vmodel.TaskName);
                    var tstatus = db.ProTaskUpdations.Where(a => a.ProTaskId == pro.ProTaskId);
                    if (tstatus != null)
                    {
                        db.ProTaskUpdations.RemoveRange(db.ProTaskUpdations.Where(a => a.ProTaskId == pro.ProTaskId));
                        db.SaveChanges();
                    }

                    ProTaskUpdation TaskUps = new ProTaskUpdation
                    {
                        ProTaskId = proId,
                        //Status = TKUpdateStatus.Assigned,
                        CreatedBy = UserId,
                        CreatedDate = today,
                        //TaskTeamId = teamId
                        Remarks = oldtskstat + " Change To " + newtskstat,
                    };
                    db.ProTaskUpdations.Add(TaskUps);
                    db.SaveChanges();
                    Int64 TaskUpdId = TaskUps.TaskUpdationID;
                    // file 

                    var docinfo = new TaskRemark
                    {
                        CreatedDate = DateTime.Now,

                        Remark = "Edited ",
                        AddedUser = User.Identity.GetUserId(),
                        TaskId = proId,
                        TaskUpdationID = TaskUpdId,
                        TaskStatusID = vmodel.TaskStatus,

                    };
                    db.TaskRemarks.Add(docinfo);
                    db.SaveChanges();

                    if (vmodel.AssignedMembers != null)
                    {
                        var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId && a.Status == "Assigned" && a.chkStatus == Status.active).Select(a => a.EmployeeId).ToArray();
                        var newusers = vmodel.AssignedMembers.ToArray();

                        TaskAssigned tskass = new TaskAssigned();

                        foreach (var arr in tskasgn)
                        {
                            var taskassId = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId && a.Status == "Assigned" && a.chkStatus == Status.active && a.EmployeeId == arr).Select(a => a.TaskAssignedId).FirstOrDefault();
                            TaskAssigned tskassr = db.TaskAssigneds.Find(taskassId);
                            if (!newusers.Contains(arr))
                            {

                                tskassr.chkStatus = Status.inactive;
                                db.Entry(tskassr).State = EntityState.Modified;
                                db.SaveChanges();

                                tskass.ProTaskId = proId;
                                tskass.EmployeeId = arr;
                                tskass.Status = "Removed";
                                tskass.AssignBy = UserId;
                                tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100); ;
                                db.TaskAssigneds.Add(tskass);
                                db.SaveChanges();
                            }
                            else
                            {
                                tskassr.chkStatus = Status.inactive;


                                db.Entry(tskassr).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }


                        foreach (var arr in vmodel.AssignedMembers)
                        {
                            tskass.ProTaskId = proId;
                            tskass.EmployeeId = arr;
                            tskass.Status = "Assigned";
                            tskass.AssignBy = UserId;
                            tskass.chkStatus = Status.active;
                            tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100);
                            db.TaskAssigneds.Add(tskass);
                            db.SaveChanges();
                            com.remideradd(rootdomain+"proTask/mytask", arr, UserId, "Task Assined",proId);

                        }
                    }


                    // fileupload
                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {

                                var fileCount = db.TaskImages.Select(a => a.TaskImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName = Path.GetFileName(file.FileName);

                                String extension = Path.GetExtension(fileName);


                                String newName = fileCount + extension;
                                string newFName = fileCount + extension;

                                var thumbName = "";
                                var resizeName = "";
                                var FStatus = Status.active;
                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), newName);
                                file.SaveAs(newName);

                                var taskimg = new TaskImage
                                {
                                    ProTaskId = proId,
                                    TaskUpdationID = TaskUpdId,

                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                    CreatedBy = UserId,
                                };
                                db.TaskImages.Add(taskimg);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }
                    db.ContactRelation.RemoveRange(db.ContactRelation.Where(o => o.RelationID == proId && o.RelationType == 11));
                    db.SaveChanges();
                    if (vmodel.LstContacts != null && vmodel.LstContacts.Count > 0)
                    {
                        foreach (var item in vmodel.LstContacts)
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

                            try
                            {
                                // Your code...
                                // Could also be before try if you know the exception occurs in SaveChanges

                                db.Contacts.Add(contact);
                                db.SaveChanges();
                            }
                            catch (DbEntityValidationException e)
                            {
                                foreach (var eve in e.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                            ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                throw;
                            }

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
                    msg = "Task details Updated Successfully.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {

                    msg = "Looks like something went wrong. Please check your form";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
        }
        public ActionResult showdescription(long id)
        {
            TaskImage im = db.TaskImages.Find(id);
            ViewBag.description = im.description;
            return PartialView();
        }
        public JsonResult Getmanhour(long protaskid, string stDate)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = "empname";
            var sortColumnDir = "asc";
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var UserId9 = User.Identity.GetUserId();
            DateTime? crdate = null;
            if (stDate != "")
            {
                crdate = DateTime.Parse(stDate, new CultureInfo("en-GB"));
            }
            //            let startt = db.EmpAttDetails.Where(o => o.protaskid == a.protaskid && o.userid == a.EmployeeName && o.empattid == a.Id).OrderBy(o => o.starttime).Select(o => o.starttime).FirstOrDefault()
            //            let detailid = db.EmpAttDetails.Where(o => o.protaskid == a.protaskid && o.userid == a.EmployeeName && o.empattid == a.Id).OrderBy(o => o.starttime).Select(o => o.empattdetailsid).FirstOrDefault()
            //            where a.protaskid == protaskid && a.EmployeeName==UserId9
            //                a.Id,
            //                empname = b.FirstName + " " + b.LastName,
            //                starttime = startt,
            //                empdetailid = detailid,
            //                a.login,
            //                a.logout



            var data = (from a in db.servicereports
                        join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                        join c in db.Employees on b.employeeid equals c.EmployeeId
                        where a.protaskid == protaskid
                        select new
                        {
                            Id = a.servicereportid,
                            empname = c.FirstName + " " + c.LastName,
                            starttime1 = a.starttime,
                            empdetailid = c.EmployeeId,
                            a.starttime,
                            endtime = (a.endtime < a.starttime) ? DbFunctionsCompat.AddDays(a.endtime, 1) : a.endtime

                        }).ToList().Select(o => new
                        {
                            o.Id,
                            o.empname,
                            o.starttime1,
                            o.empdetailid,
                            o.starttime,
                            o.endtime,
                            hours =((o.endtime-o.starttime).Value.Days==1) ?(o.endtime - o.starttime).Value.Hours+24: (o.endtime - o.starttime).Value.Hours ,
                            minute= (o.endtime - o.starttime).Value.Minutes,
                        }) ;

            //            (a.protaskid==protaskid)

            //                a.Id,
            //                empname= b.FirstName + " " + b.LastName,
            //                starttime=c.starttime,
            //                empdetailid=c.empattdetailsid,
            //                a.login,
            //                a.logout
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (stDate != "")
            {
                data = data.Where(z=>(EF.Functions.DateDiffDay(z.starttime, crdate) <= 0)&& (EF.Functions.DateDiffDay(z.starttime, crdate) >= 0));
            }
            data = data.OrderByDescending(c => c.endtime == null).ThenByDescending(n => n.starttime);
            recordsTotal = data.Count();
            var data2 = data.ToList();
            
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data2 });

        }
        [RedirectingAction]
        [Authorize(Roles = "Dev,View ProTask")]
        public ActionResult Details(long id)
        {
            companySet();
            var UserId = User.Identity.GetUserId();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTask protsk = db.ProTasks.Find(id);
            if (protsk == null)
            {
                return NotFound();
            }
            ViewProTaskViewModel vmodel = new ViewProTaskViewModel();
            vmodel = (from a in db.ProTasks
                      join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                      from b in pro.DefaultIfEmpty()
                      join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                      from c in type.DefaultIfEmpty()
                      join d in db.Customers on a.CustomerID equals d.CustomerID into cust
                      from d in cust.DefaultIfEmpty()
                      join f in db.Contacts on d.Contact equals f.ContactID into conts
                      from f in conts.DefaultIfEmpty()
                      join g in db.Projects on a.ProjectId equals g.ProjectId into proj
                      from g in proj.DefaultIfEmpty()
                      join h in db.TaskStatus on a.TaskStatus equals h.TaskStatusId into tstat
                      from h in tstat.DefaultIfEmpty()
                      join s in db.Employees on a.SalesPerson equals s.EmployeeId into emp
                      from s in emp.DefaultIfEmpty()
                      join t in db.Accountss on d.Accounts equals t.AccountsID into accs
                      from t in accs.DefaultIfEmpty()
                      join u in db.Users on a.CreatedBy equals u.Id
                      //  let log = db.LogManagers.Where(lg => lg.LogID == a.ProTaskId.ToString() && lg.LogTable == "ProTasks").OrderByDescending(lg => lg.LogManagerID).FirstOrDefault()

                      where a.ProTaskId == id
                      select new
                      {
                          a.TaskName,
                          a.CustomerID,
                          a.TaskCode,
                          ProLocation = b.Location,
                          a.Location,
                          a.TaskDetails,
                          c.TypeName,
                          d.CustomerCode,
                          d.CustomerName,
                          g.ProCode,
                          g.ProjectName,
                          h.StatusName,
                          a.Priority,
                          a.ProTaskId,
                          a.Note,
                          a.StartDate,
                          a.StartTime,
                          a.EndDate,
                          a.EndTime,
                          u.UserName,
                          a.CreatedDate,


                          f.Address,
                          f.City,
                          f.State,
                          f.Country,
                          f.Zip,
                          f.Phone,
                          //f.Mobile,
                          f.Fax,
                          f.EmailId,
                          f.Reference,
                          f.ContactPerson,
                          s.FirstName,
                          s.LastName,
                          Type = (a.CustomerID == -2 || a.CustomerID == null) ? 0 : d.Type,
                          a.Ref1,
                          a.Ref2,
                          a.Ref3,
                          a.Ref4,
                          a.Ref5,
                          TaxRegNo = t != null ? t.TRN : "",
                          Mobile = (from ac in db.TaskMobiles
                                    where (ac.ProTaskId == a.ProTaskId)
                                    select new MobileViewModel
                                    {
                                        Num = ac.MobileNo
                                    }).ToList(),
                          CustLocation = d.Location,
                          ldate = ((a != null) && (a.CreatedDate > a.logtime)) ? a.CreatedDate : a.logtime,
                      }).ToList().Select(o => new ViewProTaskViewModel
                      {
                          TaskName = o.TaskName,
                          TaskCode = o.TaskCode,
                          Location = o.Location,
                          ProLocation = o.ProLocation,
                          TaskDetails = o.TaskDetails,
                          TaskType = o.TypeName,
                          CustomerName = o.CustomerID == -2 ? "No Customer" : (o.CustomerID == null ? o.CustomerName : o.CustomerName),
                          CustomerCode = o.CustomerID == -2 ? "" : (o.CustomerID == null ? o.CustomerCode : o.CustomerCode),
                          CustLocation = o.CustLocation,
                          CustomerId = o.CustomerID,
                          // CustType = o.TypeName,
                          ProjectName = o.ProCode + "-" + o.ProjectName,
                          TaskStatus = o.StatusName,
                          Priority = Enum.GetName(typeof(TaskPriority), o.Priority),
                          ProTaskId = o.ProTaskId,
                          Note = o.Note,
                          StartDate = o.StartDate,
                          StartTime = o.StartTime,
                          EndDate = o.EndDate,
                          EndTime = o.EndTime,
                          CreatedBy = o.UserName,
                          CreatedDate = o.CreatedDate,

                          Address = o.Address,
                          City = o.City,
                          State = o.State,
                          Country = o.Country,
                          Zip = o.Zip,
                          Phone = o.Phone,
                          //Mobile = o.Mobile,
                          mob = o.Mobile,
                          Fax = o.Fax,
                          EmailId = o.EmailId,
                          Reference = o.Reference,
                          ContactPerson = o.ContactPerson,
                          SalesPersonName = o.FirstName + " " + o.LastName,
                          CustType = (o.CustomerID == -2 || o.CustomerID == null) ? "Customer " : Enum.GetName(typeof(CRMCustomerType), o.Type),
                          Ref1 = o.Ref1,
                          Ref2 = o.Ref2,
                          Ref3 = o.Ref3,
                          Ref4 = o.Ref4,
                          Ref5 = o.Ref5,
                          TaxRegNo = o.TaxRegNo,
                          UpDate = o.ldate
                      }).FirstOrDefault();


            vmodel.TaskAssign = (from a in db.TaskAssigneds
                                 join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                                 from b in emp.DefaultIfEmpty()
                                 where a.ProTaskId == id && a.chkStatus == Status.active && a.Status == "Assigned"
                                 select new TaskAssignToViewModel
                                 {
                                     Empname = b.FirstName + " " + b.MiddleName + " " + b.LastName + ", ",
                                 }).Distinct().ToList();
            vmodel.TaskDocuments = (from b in db.TaskImages
                                    join c in db.ProTasks.Where(o=>o.Branch==1) on b.ProTaskId equals c.ProTaskId
                                    where c.ProTaskId == id
                                    select new TaskDocumentViewModel
                                    {
                                        ProTaskId = c.ProTaskId,
                                        TaskDocumentId = b.TaskImageId,
                                        FileName = b.FileName,
                                    }).ToList();
            var leadid = db.ProTasks.Where(o => o.ProTaskId == id).Select(o => o.CustomerID).FirstOrDefault();
            var doc = (from a in db.LeadDocuments
                       where a.CustomerID == leadid
                       select new LeadDocumentViewModel
                       {
                           CustomerID = a.CustomerID,
                           LeadDocumentId = a.LeadDocumentId,
                           FileName = a.FileName,
                           notes = a.Notes
                       }).ToList();
            if (doc.Count > 0)
            {
                vmodel.LeadcreateDocuments = doc.ToList();
            }

            //LeadActivity
            string ids = id.ToString();

            var lact = (from a in db.LogManagers
                        join b in db.Users on a.User equals b.Id into user
                        from b in user.DefaultIfEmpty()
                        where (a.LogID == id.ToString()) && (a.LogTable == "ProTasks")
                        select new TaskTimelineViewModel
                        {
                            Name = b.UserName,
                            LogType = a.LogType.ToString(),
                            Time = a.LogTime,
                            Details = a.LogDetails,
                        }).ToList();


            var rem = (from a in db.TaskRemarks
                       join b in db.Users on a.AddedUser equals b.Id into emp
                       from b in emp.DefaultIfEmpty()
                       join c in db.TaskStatus on a.TaskStatusID equals c.TaskStatusId into stat
                       from c in stat.DefaultIfEmpty()
                       where a.TaskId == id && id!= 240742 
                       select new TaskTimelineViewModel
                       {
                           Name = b.UserName,
                           LogType = "Remark Added",
                           Time = a.CreatedDate,
                           Details = a.Remark,
                           TStatus = (c.StatusName != null) ? c.StatusName : "",
                           ProTaskId = a.TaskId,
                           RImages = (from z in db.TaskRemarks
                                      join y in db.TaskImages on z.TaskRemarkId equals y.TaskRemarkId into img
                                      from y in img.DefaultIfEmpty()
                                      where y.TaskUpdationID == a.TaskUpdationID
                                      select new TaskImageViewModel
                                      {
                                          TaskImageId = y.TaskImageId,
                                          FileName = (y.FileName != null) ? y.FileName : "",
                                      }).ToList(),
                           check = (from z in db.RemarkChecklists
                                    join y in db.ChecklistItems on z.Checklistitemid equals y.Id into img
                                    from y in img.DefaultIfEmpty()
                                    where z.Remark == a.TaskRemarkId
                                    select new ChecklistViewModel
                                    {
                                        Name = y.ListName,
                                        Note = z.Note,
                                        Chck = z.Check,

                                    }).ToList(),
                       }).ToList().Select(o => new TaskTimelineViewModel
                       {
                           Name = o.Name,
                           LogType = o.LogType,
                           Time = o.Time,
                           Details = o.Details,
                           TStatus = o.TStatus,
                           ProTaskId = o.ProTaskId,
                           RImages = o.RImages,
                           check = o.check,

                       }).ToList();

            vmodel.TaskAssignedVModel = (from a in db.TaskAssigneds
                                         join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                                         from b in emp.DefaultIfEmpty()
                                         join c in db.Users on a.AssignBy equals c.Id into use
                                         from c in use.DefaultIfEmpty()
                                         where a.ProTaskId == id && a.CreatedDate != null
                                         select new TaskTimelineViewModel
                                         {
                                             ProTaskId = a.ProTaskId,
                                             Name = c.UserName,
                                             LogType = a.Status,
                                             Time = (DateTime)a.CreatedDate,
                                             Details = b.FirstName + " " + b.LastName,
                                         }).ToList();



            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            var reminder = (from a in db.Reminders
                            let remas = db.ReminderAssigneds.Where(x => x.ReminderId == a.ReminderId).Select(x => x.EmployeeId).ToList()
                            where remas.Contains(empId) &&
                            a.Reference == id && a.Type == "Task"
                            select new TaskTimelineViewModel
                            {
                                Name = ((DateTime.Now <= a.RDate) ? "Upcoming" : "Expired"),
                                LogType = "Reminder",
                                Time = (DateTime)a.RDate,
                                Details = a.Note,
                            });

            var images = (from a in db.TaskImages
                          join c in db.Users on a.CreatedBy equals c.Id into use
                          from c in use.DefaultIfEmpty()
                          where a.ProTaskId == id && a.TaskRemarkId == null && a.CreatedDate != null
                          select new
                          {
                              Name = c.UserName,
                              LogType = "Image Uploaded",
                              Time = a.CreatedDate,
                              Details = "",
                              TStatus = "",
                              ProTaskId = a.ProTaskId,
                              RImages = (from z in db.TaskImages
                                         where z.ProTaskId == id && z.TaskImageId == a.TaskImageId && a.TaskRemarkId == null
                                         select new TaskImageViewModel
                                         {
                                             TaskImageId = z.TaskImageId,
                                             FileName = (z.FileName != null) ? z.FileName : "",
                                         }).ToList(),
                          }).ToList().Select(o => new TaskTimelineViewModel
                          {
                              Name = o.Name,
                              LogType = o.LogType,
                              Time = o.Time,
                              Details = o.Details,
                              TStatus = o.TStatus,
                              ProTaskId = o.ProTaskId,
                              RImages = o.RImages,
                              check = null,

                          }).ToList();

            var det = lact.Union(rem).Union(reminder).Union(images);
            var comp = det.OrderByDescending(a => a.Time);
            vmodel.TaskTimeLine = comp.ToList();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();

            var custID = vmodel.CustomerId;
            var lact2 = (from a in db.LogManagers
                         join b in db.Users on a.User equals b.Id into user
                         from b in user.DefaultIfEmpty()
                         where (a.LogID == custID.ToString()) && (a.LogTable == "Customers")
                         select new CustTimelineViewModel
                         {
                             Name = b.UserName,
                             LogType = a.LogType.ToString(),
                             Time = a.LogTime,
                             Details = a.LogDetails,
                         }).ToList();

            var asl2 = (from a in db.AssignedToLogs
                        join c in db.Users on a.AddedUser equals c.Id into usr
                        from c in usr.DefaultIfEmpty()
                        join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                        from b in emp.DefaultIfEmpty()
                        where a.CustomerID == custID
                        select new CustTimelineViewModel
                        {
                            Name = c.UserName,
                            LogType = a.Status,
                            Time = a.AssignedDate,
                            Details = a.Status + " Employee " + b.FirstName + " " + b.MiddleName + " " + b.LastName,
                        }).ToList();

            var comp2 = lact2.Union(asl2).OrderByDescending(a => a.Time);
            vmodel.CustTimeLine = comp2.ToList();

            return View(vmodel);

        }
        [HttpPost]
        public ActionResult editmanhour(long id, ProTaskViewModel pro)
        {
            long empattid;
            EmpAttDetails empattde = db.EmpAttDetails.Where(o => o.empattdetailsid == id).FirstOrDefault();
            if(pro.StartTime!=null)
            empattde.starttime = (DateTime)pro.StartTime;

            db.Entry(empattde).State = EntityState.Modified;

            db.SaveChanges();
            empattid = empattde.empattid;

            bool stat = true;
            string msg = "Successfully Update";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [RedirectingAction]
        public ActionResult editmanhour(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmpAttDetails pro = db.EmpAttDetails.Where(o => o.empattdetailsid == id).FirstOrDefault();





            ProTaskViewModel vmodel = new ProTaskViewModel();


            vmodel.StartDate = (pro.starttime != null) ? ((DateTime)pro.starttime).ToString("dd-MM-yyyy") : "";
            vmodel.StartTime = pro.starttime;

            EmpAttendance pro2 = db.EmpAttendances.Where(o => o.Id == pro.empattid).FirstOrDefault();
            vmodel.EndDate = (pro2.logout != null) ? ((DateTime)pro2.logout).ToString("dd-MM-yyyy") : "";
            vmodel.EndTime = pro2.logout;
            vmodel.ProTaskId = pro2.protaskid;




            ViewBag.empdetailid = id;


            if (pro == null)
            {
                return NotFound();
            }
            return PartialView(vmodel);
        }
        [RedirectingAction]
        public ActionResult Deletemanhour(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EmpAttendance pro = db.EmpAttendances.Find(id);
            if (pro == null)
            {
                return NotFound();
            }
            return PartialView(pro);
        }
        [HttpPost, ActionName("Deletemanhour")]
        public ActionResult DeletemanhourConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();

            db.EmpAttDetails.RemoveRange(db.EmpAttDetails.Where(o => o.empattid == id));
            db.SaveChanges();
            db.EmpAttendances.RemoveRange(db.EmpAttendances.Where(o => o.Id == id));
            db.SaveChanges();

            stat = true;
            msg = "Successfully Deleted Task details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        // GET: /Delete/5
        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete ProTask")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTask pro = db.ProTasks.Find(id);
            if (pro == null)
            {
                return NotFound();
            }
            var existing = (from a in db.SalesEntrys
                            join b in db.additionaltasks on a.SalesEntryId equals b.salesentryid into cnt
                            from b in cnt.DefaultIfEmpty()
                            where a.ProTask == id || b.taskid == id
                            select new
                            {
                                a.BillNo
                            }
                          ).FirstOrDefault();
            if(existing!=null)
            {
                return NotFound();

            }
            return PartialView(pro);
        }

        // POST: /Delete/5
        //[RedirectingAction]
        //[Authorize(Roles = "Dev,Delete ProTask")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
           
            var UserId = User.Identity.GetUserId();
            DeleteTask(id);

            stat = true;
            msg = "Successfully Deleted Task details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }




        public JsonResult invoice(long inv)
        {
            var protask = (from t in db.ProTasks.Where(o=>o.Branch==1)
                           join c in db.Customers on t.CustomerID equals c.CustomerID into cust

                           from c in cust.DefaultIfEmpty()
                           join p in db.Projects on t.ProjectId equals p.ProjectId into pro
                           from p in pro.DefaultIfEmpty()

                           where t.ProTaskId == inv
                           select new
                           {
                               t.TaskName,

                               t.ProTaskId,
                               t.ProjectId,
                               p.EndDate,
                               c.CustomerName,
                               p.ProjectName,
                               t.logtime,



                           }
                        ).FirstOrDefault();
            var taskimage = (from tm in db.TaskImages
                             where tm.ProTaskId == inv
                             orderby tm.CreatedDate descending
                             select new
                             {
                                 tm.TaskImageId,
                                 tm.description,
                                 tm.newdescription
                             }

                             ).ToList();



            return new QuickSoft.Models.LegacyJsonResult { Data = new { protask, taskimage } };
        }
        [RedirectingAction]
        [Authorize(Roles = "Dev,My ProTask")]
        public ActionResult MyTask()
        {
            ViewBag.BusiType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            
            ViewBag.VehicleTypes = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Vehicle Type", Value = "0"},
                               }, "Value", "Text", 0);

            ViewBag.VehicleManu = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Manufacturer", Value = "0"},
                               }, "Value", "Text", 0);
            ViewBag.VehicleModels = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "Select the Model", Value = "0"},
                              }, "Value", "Text", 0);

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindTask").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ViewBag.TaskName = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.TaskType = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Projects = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.TStat = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Employee = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                         }, "Value", "Text", 1);
            //     .Select(s => new
            //         ID = s.EmployeeId,
            //         Name = s.FirstName + " " + s.LastName
            //     })

            ViewBag.User = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
            }, "Value", "Text", 1);

            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));

            ViewBag.SalesExecutive = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
           }, "Value", "Text", 1);


            ViewBag.Locat = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);
            ViewBag.Mobile = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                         },
                           "Value", "Text", 1);



            var ref1 = db.ProTasks.Where(o => o.Ref1 != null)
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.ProTasks
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");


            ProTaskViewModel vmodel = new ProTaskViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Task").ToList();
            ViewBag.proid = Request.Query["protaskid"];
            return View(vmodel);
        }

        [RedirectingAction]
        [Authorize(Roles = "Dev,My ProTask")]
        public ActionResult MyNewTask()
        {
            ViewBag.BusiType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.VehicleTypes = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Vehicle Type", Value = "0"},
                               }, "Value", "Text", 0);

            ViewBag.VehicleManu = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Manufacturer", Value = "0"},
                               }, "Value", "Text", 0);
            ViewBag.VehicleModels = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "Select the Model", Value = "0"},
                              }, "Value", "Text", 0);

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindTask").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ViewBag.TaskName = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.TaskType = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Projects = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.TStat = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Employee = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                         }, "Value", "Text", 1);
            //     .Select(s => new
            //         ID = s.EmployeeId,
            //         Name = s.FirstName + " " + s.LastName
            //     })

            ViewBag.User = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
            }, "Value", "Text", 1);

            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));

            ViewBag.SalesExecutive = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
           }, "Value", "Text", 1);


            ViewBag.Locat = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);
            ViewBag.Mobile = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                         },
                           "Value", "Text", 1);



            var ref1 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.ProTasks
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.ProTasks
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");


            ProTaskViewModel vmodel = new ProTaskViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Task").ToList();
            ViewBag.proid = Request.Query["protaskid"];
            return View(vmodel);
        }


        [HttpPost]
        [RedirectingAction]
        [Authorize(Roles = "Dev,My ProTask")]
        public ActionResult GetMyTask(long? taskname, long? tasktype, long? customer, long? projects, long? assignedto, string createdby, string priority, string fromdate, string todate, long? taskstat, long? empl, string ref1, string ref2, string ref3, string ref4, string ref5, string remdate, string remstatus, string local, string Mobile, string LastUpdDays, long AssTo, string txtremarks, long? protaskid, long? VType, long? Vmanu, long? Vmod)//TKUpdateStatus? status //IEnumerable<TKUpdateStatus> status
        {

            int days = 0;
            if (LastUpdDays != "")
            {
                days = Convert.ToInt32(LastUpdDays);
            }
            DateTime datecheck = DateTime.Now.AddDays(-days);

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
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            bool allcheck = User.IsInRole("All ProTask");
            bool editcheck = User.IsInRole("Edit ProTask");
            bool devcheck = User.IsInRole("Dev");


            string qry = @"update ProTasks set TaskStatus =q2.TaskStatusID from ProTasks join(
select ProTaskId,TaskStatus,TaskStatusID  from ProTasks join
  (
(select  TaskId,TaskStatusID,CreatedDate,
RANK() OVER (PARTITION BY TaskId ORDER BY TaskUpdationID DESC) AS rk
from TaskRemarks where Remark!='Task Edited' and createddate>='2024-12-01' group by 
TaskId ,TaskStatusID,CreatedDate,TaskUpdationID ) ) as q1 on q1.TaskId =ProTasks.ProTaskId 
where q1.rk =1 and TaskStatus !=TaskStatusID  ) as q2  on q2.ProTaskId =ProTasks.ProTaskId
";
            var exec = db.Database.ExecuteSqlRaw(qry);


            qry = @"update ProTasks set logtime =q2.CreatedDate from  ProTasks 
 join (
select logtime,q1.CreatedDate,q1.ProTaskId from protasks join 
(select  ProTaskId,max(CreatedDate) as CreatedDate

from ProTaskUpdations where CreatedDate>'2023-01-01'
group by ProTaskId
) as q1 on q1.ProTaskId=ProTasks.ProTaskId  
where  q1.CreatedDate>protasks.logtime   ) as q2
on q2.ProTaskId =ProTasks.ProTaskId
";


            TaskPriority Prior = new TaskPriority();
            if (priority == "1")
            {
                Prior = (TaskPriority)TaskPriority.Low;
            }
            if (priority == "2")
            {
                Prior = (TaskPriority)TaskPriority.Medium;
            }
            if (priority == "3")
            {
                Prior = (TaskPriority)TaskPriority.High;
            }
            var leadtotaskconvert=User.IsInRole("Lead To Task Convert");
             DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? remddate = null;
            if (!string.IsNullOrEmpty(fromdate))
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(todate))
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(remdate))
            {
                remddate = DateTime.Parse(remdate, new CultureInfo("en-GB"));
            }
            DateTime assdate = DateTime.Now.AddDays(-90);
            var taskassign = (from z in db.TaskAssigneds
                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                              where z.Status == "Assigned" && z.chkStatus == Status.active &&
                              ( z.CreatedDate >= assdate)
                              select z);
            // taskups (GroupBy-latest) join removed — EF Core 10 can't translate it; the latest update per task
            // is fetched CLIENT-side after materialization (taskupLookup). The `let assign=...ToList()`+Contains
            // user filter is converted to a translatable .Any(...) EXISTS so it stays server-side (preserves the
            // "tasks assigned to me / lead-convert / my-created" scope). Reminder + AssignedTo + Updateduser +
            // ldate + the remstatus/days/AssTo/ldate filters are all computed client-side via lookups.
            var rawRows = (from a in db.ProTasks.Where(o=>o.Branch==1)
                            where (protaskid == null || a.ProTaskId == protaskid)
                            join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                            from b in pro.DefaultIfEmpty()
                            join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                            from c in type.DefaultIfEmpty()
                            join e in db.Users on a.CreatedBy equals e.Id into creat
                            from e in creat.DefaultIfEmpty()
                            join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                            from d in cus.DefaultIfEmpty()
                            join f in db.TaskStatus on a.TaskStatus equals f.TaskStatusId into ttask
                            from f in ttask.DefaultIfEmpty()
                            join s in db.Employees on a.SalesPerson equals s.EmployeeId into emp
                            from s in emp.DefaultIfEmpty()
                            where (taskassign.Any(x => x.ProTaskId == a.ProTaskId && x.EmployeeId == empId) ||(a.VManuId==999&& leadtotaskconvert==true)|| (a.VManuId == 998 && a.CreatedBy==UserId))
                            &&
                            (a.OpenClose == 0 || a.OpenClose == null) &&
                                               (taskname == null || taskname == 0 || a.ProTaskId == taskname) &&
                                               (tasktype == null || tasktype == 0 || a.TaskType == tasktype) &&
                                               (customer == null || customer == 0 || a.CustomerID == customer) &&
                                               (projects == null || projects == 0 || a.ProjectId == projects) &&
                                               (createdby == "" || createdby == "All" || a.CreatedBy == createdby) &&
                                               (priority == "0" || priority == null || a.Priority == Prior)
                                               //  (todate == "" || (a.CreatedDate != null && EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
                                               && (taskstat == 0 || taskstat == null || a.TaskStatus == taskstat)
                                               && (empl == 0 || empl == null || a.SalesPerson == empl) &&
                                               (ref1 == "" || ref1 == null || a.Ref1 == ref1) &&
                                               (ref2 == "" || ref2 == null || a.Ref2 == ref2) &&
                                               (ref3 == "" || ref3 == null || a.Ref3 == ref3) &&
                                               (ref4 == "" || ref4 == null || a.Ref4 == ref4) &&
                                               (ref5 == "" || ref5 == null || a.Ref5 == ref5) &&

                                               (VType == 0 || VType == null || a.VTypId == VType) &&
                              (Vmanu == 0 || Vmanu == null || a.VManuId == Vmanu) &&
                              (Vmod == 0 || Vmod == null || a.VModId == Vmod) &&

                                               (remdate == "" || EF.Functions.DateDiffDay(a.StartDate, remddate) == 0)
                                               && (local == "0" || local == "All" || local == null || a.Location == local)
                                             //  && (Mobile == "All" || (mobnum.Count() > 0 && mobnum.Contains(Mobile)))
                                               &&
                                                 (search == "" ||
                           a.TaskName.ToString().ToLower().Contains(search.ToLower()) ||
                    a.TaskName.ToString().ToLower().StartsWith(search.ToLower()) ||
                    a.TaskName.ToString().ToLower().EndsWith(search.ToLower()) ||
                    a.TaskCode.ToString().ToLower().StartsWith(search.ToLower())
                    )
                            select new { a, b, c, d, e, f, s }).ToList();

            // CLIENT-side lookups keyed by ProTaskId (missing key -> empty/absent, no KeyNotFound).
            var myIds = rawRows.Select(o => o.a.ProTaskId).ToList();
            var taskupLookup = db.ProTaskUpdations
                .Where(u => myIds.Contains(u.ProTaskId))
                .ToList().GroupBy(u => u.ProTaskId).ToDictionary(g => g.Key, g => g.OrderByDescending(u => u.CreatedDate).First());
            var assignLookup = (from z in db.TaskAssigneds
                                where myIds.Contains(z.ProTaskId) && z.Status == "Assigned" && z.chkStatus == Status.active
                                join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                select new { z.ProTaskId, id = y.EmployeeId, LastName = y.LastName ?? "", FirstName = y.FirstName ?? "", MiddleName = y.MiddleName ?? "", Img = y.ImgFileName ?? "", y.Status })
                               .ToList().ToLookup(z => z.ProTaskId);
            var remLookup = db.Reminders.Where(z => z.Type == "Task" && myIds.Contains(z.Reference))
                              .ToList().GroupBy(z => z.Reference).ToDictionary(g => g.Key, g => g.OrderByDescending(z => z.RDate).First());
            var updByIds = taskupLookup.Values.Where(u => u.CreatedBy != null).Select(u => u.CreatedBy).Distinct().ToList();
            var updUserLookup = db.Users.Where(u => updByIds.Contains(u.Id)).Select(u => new { u.Id, u.UserName }).ToList().ToDictionary(u => u.Id, u => u.UserName);

            var UserView = rawRows
                            .Select(r => new { o = r, tu = taskupLookup.ContainsKey(r.a.ProTaskId) ? taskupLookup[r.a.ProTaskId] : null })
                            .Select(r => new { r.o, r.tu, ldate = (r.tu != null && r.tu.CreatedDate > r.o.a.logtime) ? r.tu.CreatedDate : r.o.a.logtime,
                                               rem = remLookup.ContainsKey(r.o.a.ProTaskId) ? remLookup[r.o.a.ProTaskId] : null })
                            .Where(r =>
                                (remstatus == "0" || (r.rem != null && ((DateTime.Now <= r.rem.RDate ? "Upcoming" : "Expired") == remstatus)))
                              && (days == 0 || (r.tu != null && r.tu.CreatedDate <= datecheck && r.o.a.logtime <= datecheck))
                              && (fromdate == "" || (r.ldate != null && fdate != null && r.ldate.Value.Date <= fdate.Value.Date))
                              && (todate == "" || (r.ldate != null && tdate != null && r.ldate.Value.Date >= tdate.Value.Date))
                              && (AssTo == 0 || assignLookup[r.o.a.ProTaskId].Select(z => z.id).Contains(AssTo)))
                            .Select(r =>
                            {
                                var o = r.o;
                                var taskup = r.tu;
                                return new
                            {
                                o.a.ProTaskId,
                                o.a.TaskName,
                                o.a.TaskCode,
                                Project = o.a.ProjectId == null ? 0 : (o.b != null ? o.b.ProjectId : 0),
                                ProjectName = o.b != null ? o.b.ProjectName : null,
                                CustomerName = o.a.CustomerID == -2 ? "No Customer" : (o.d != null ? o.d.CustomerName : null),
                                CustomerID = o.a.CustomerID == -2 ? (long?)-2 : (o.a.CustomerID ?? (o.d != null ? o.d.CustomerID : (long?)null)),
                                UserName = o.e != null ? o.e.UserName : null,
                                o.a.StartDate,
                                o.a.EndDate,
                                o.a.StartTime,
                                o.a.EndTime,
                                TypeName = o.c != null ? o.c.TypeName : null,
                                o.a.VTypId,
                                o.a.VManuId,
                                o.a.VModId,
                                Priority = Enum.GetName(typeof(TaskPriority), o.a.Priority),
                                priorityint = o.a.Priority,
                                AssignedTo = assignLookup[o.a.ProTaskId].Select(z => new { z.id, z.LastName, z.FirstName, z.MiddleName, z.Img, z.Status }).Distinct().ToList(),
                                TaskStat = o.f != null ? o.f.StatusName : null,
                                EmpName = (o.s != null ? o.s.FirstName : "") + " " + (o.s != null ? o.s.LastName : ""),
                                ReminderDate = r.rem != null ? (DateTime?)r.rem.RDate : null,
                                validity = r.rem != null ? (DateTime.Now <= r.rem.RDate ? "Upcoming" : "Expired") : null,
                                o.a.Ref1,
                                o.a.Ref2,
                                o.a.Ref3,
                                o.a.Ref4,
                                o.a.Ref5,
                                o.a.Location,
                                ldate = r.ldate,
                                TaskDetails=(o.a.TaskDetails==""||o.a.TaskDetails==null)?o.a.Note:o.a.TaskDetails,
                                o.a.CreatedBy,
                                UserId,
                                allcheck,
                                editcheck,
                                devcheck,
                                mobmodel = mobilesno(o.a.ProTaskId),
                                Updateduser = (taskup != null && taskup.CreatedBy != null && updUserLookup.ContainsKey(taskup.CreatedBy)) ? updUserLookup[taskup.CreatedBy] : null,
                            };
                            })
                            // preserve the original "order by the grid's sort column, then cap at 300" before the
                            // final priority/vtype/ldate ordering below (sort applied on the projected output).
                            .AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).Take(300).AsEnumerable();

            //    // Apply search
            //    UserView = UserView.Where(p => p.TaskName.ToString().ToLower().Contains(search.ToLower())
            if (txtremarks != "")
            {
                var taskupdate = db.TaskRemarks.Where(p => p.Remark.ToLower().Contains(txtremarks.ToLower())).Select(o => o.TaskId).Distinct().ToList();
                if (taskupdate.Count > 0)

                {


                    UserView = UserView.Where(o => taskupdate.Contains(o.ProTaskId));



                }
                else
                {
                    UserView = UserView.Where(o => taskupdate.Contains(-1));
                }
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
            }
            else
            {
            }
            UserView = UserView.OrderByDescending(o=>o.priorityint).ThenByDescending(o=>o.VTypId).ThenByDescending(x => x.ldate);
            recordsTotal = UserView.Count();
            var data = UserView.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }


        [HttpPost]
        [RedirectingAction]
        [Authorize(Roles = "Dev,My ProTask")]
        public ActionResult GetMynewTask(long? taskname, long? tasktype, long? customer, long? projects, long? assignedto, string createdby, string priority, string fromdate, string todate, long? taskstat, long? empl, string ref1, string ref2, string ref3, string ref4, string ref5, string remdate, string remstatus, string local, string Mobile, string LastUpdDays, long AssTo, string txtremarks, long? protaskid, long? VType, long? Vmanu, long? Vmod)//TKUpdateStatus? status //IEnumerable<TKUpdateStatus> status
        {

            int days = 0;
            if (LastUpdDays != "")
            {
                days = Convert.ToInt32(LastUpdDays);
            }
            DateTime datecheck = DateTime.Now.AddDays(-days);

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
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            bool allcheck = User.IsInRole("All ProTask");
            bool editcheck = User.IsInRole("Edit ProTask");
            bool devcheck = User.IsInRole("Dev");


            string qry = @"update ProTasks set TaskStatus =q2.TaskStatusID from ProTasks join(
select ProTaskId,TaskStatus,TaskStatusID  from ProTasks join
  (
(select  TaskId,TaskStatusID,CreatedDate,
RANK() OVER (PARTITION BY TaskId ORDER BY TaskUpdationID DESC) AS rk
from TaskRemarks where Remark!='Task Edited' and createddate>='2024-12-01' group by 
TaskId ,TaskStatusID,CreatedDate,TaskUpdationID ) ) as q1 on q1.TaskId =ProTasks.ProTaskId 
where q1.rk =1 and TaskStatus !=TaskStatusID  ) as q2  on q2.ProTaskId =ProTasks.ProTaskId
";
            var exec = db.Database.ExecuteSqlRaw(qry);


            qry = @"update ProTasks set logtime =q2.CreatedDate from  ProTasks 
 join (
select logtime,q1.CreatedDate,q1.ProTaskId from protasks join 
(select  ProTaskId,max(CreatedDate) as CreatedDate

from ProTaskUpdations where CreatedDate>'2023-01-01'
group by ProTaskId
) as q1 on q1.ProTaskId=ProTasks.ProTaskId  
where  q1.CreatedDate>protasks.logtime   ) as q2
on q2.ProTaskId =ProTasks.ProTaskId
";


            TaskPriority Prior = new TaskPriority();
            if (priority == "1")
            {
                Prior = (TaskPriority)TaskPriority.Low;
            }
            if (priority == "2")
            {
                Prior = (TaskPriority)TaskPriority.Medium;
            }
            if (priority == "3")
            {
                Prior = (TaskPriority)TaskPriority.High;
            }

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? remddate = null;
            if (!string.IsNullOrEmpty(fromdate))
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(todate))
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(remdate))
            {
                remddate = DateTime.Parse(remdate, new CultureInfo("en-GB"));
            }
            DateTime assdate = DateTime.Now.AddDays(-90);
            var taskassign = (from z in db.TaskAssigneds
                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                              where z.Status == "Assigned" && z.chkStatus == Status.active &&
                              (z.CreatedDate >= assdate)
                              select z);
            var taskmanners = (from z in db.AssignTaskManners
                              join a in db.ProTasks on z.ProTaskId equals a.ProTaskId
                              where a.Ref1=="X"
                              select new
                              {
                                z.TaskMannerId

                              }
                              ).Distinct();
            var mytaskmanners = getmytaskmanners();
            // taskups (GroupBy-latest) join + AssignedTo collection + Reminder/Updateduser/ldate + the
            // remstatus/remdate/AssTo/ldate filters can't be translated by EF Core 10 inside this projection —
            // computed CLIENT-side after materialization via lookups (same split as GetAllTasks / GetMyTask).
            // The taskmanner scope (join t + mytaskmanners.Contains) becomes a translatable .Any(...) EXISTS so it
            // stays server-side (and avoids row multiplication from the ProTaskId==TaskMannerId join).
            var rawRows = (from a in db.ProTasks.Where(o => o.Branch == 1)
                            where (protaskid == null || a.ProTaskId == protaskid)
                            join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                            from b in pro.DefaultIfEmpty()
                            join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                            from c in type.DefaultIfEmpty()
                            join e in db.Users on a.CreatedBy equals e.Id into creat
                            from e in creat.DefaultIfEmpty()
                            join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                            from d in cus.DefaultIfEmpty()
                            join f in db.TaskStatus on a.TaskStatus equals f.TaskStatusId into ttask
                            from f in ttask.DefaultIfEmpty()
                            join s in db.Employees on a.SalesPerson equals s.EmployeeId into emp
                            from s in emp.DefaultIfEmpty()
                            where a.Ref1=="X" &&
                           db.AssignTaskManners.Any(t => t.TaskMannerId == a.ProTaskId && mytaskmanners.Contains(t.TaskMannerId))
                            &&
                            (a.OpenClose == 0 || a.OpenClose == null) &&
                                               (taskname == null || taskname == 0 || a.ProTaskId == taskname) &&
                                               (tasktype == null || tasktype == 0 || a.TaskType == tasktype) &&
                                               (customer == null || customer == 0 || a.CustomerID == customer) &&
                                               (projects == null || projects == 0 || a.ProjectId == projects) &&
                                               (createdby == "" || createdby == "All" || a.CreatedBy == createdby) &&
                                               (priority == "0" || priority == null || a.Priority == Prior)
                                               //  (todate == "" || (a.CreatedDate != null && EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
                                               && (taskstat == 0 || taskstat == null || a.TaskStatus == taskstat)
                                               && (empl == 0 || empl == null || a.SalesPerson == empl) &&
                                               (ref1 == "" || ref1 == null || a.Ref1 == ref1) &&
                                               (ref2 == "" || ref2 == null || a.Ref2 == ref2) &&
                                               (ref3 == "" || ref3 == null || a.Ref3 == ref3) &&
                                               (ref4 == "" || ref4 == null || a.Ref4 == ref4) &&
                                               (ref5 == "" || ref5 == null || a.Ref5 == ref5) &&

                                               (VType == 0 || VType == null || a.VTypId == VType) &&
                              (Vmanu == 0 || Vmanu == null || a.VManuId == Vmanu) &&
                              (Vmod == 0 || Vmod == null || a.VModId == Vmod) &&

                                               (local == "0" || local == "All" || local == null || a.Location == local)
                                               //  && (Mobile == "All" || (mobnum.Count() > 0 && mobnum.Contains(Mobile)))
                                               &&
                                                true &&
                                                 (search == "" ||
                           a.TaskName.ToString().ToLower().Contains(search.ToLower()) ||
                    a.TaskName.ToString().ToLower().StartsWith(search.ToLower()) ||
                    a.TaskName.ToString().ToLower().EndsWith(search.ToLower()) ||
                    a.TaskCode.ToString().ToLower().StartsWith(search.ToLower())
                    )
                            select new { a, b, c, d, e, f, s }).ToList();

            // CLIENT-side lookups keyed by ProTaskId (missing key -> empty/absent, no KeyNotFound).
            var myIds = rawRows.Select(o => o.a.ProTaskId).ToList();
            var taskupLookup = db.ProTaskUpdations
                .Where(u => myIds.Contains(u.ProTaskId))
                .ToList().GroupBy(u => u.ProTaskId).ToDictionary(g => g.Key, g => g.OrderByDescending(u => u.CreatedDate).First());
            var assignLookup = (from z in db.TaskAssigneds
                                where myIds.Contains(z.ProTaskId) && z.Status == "Assigned" && z.chkStatus == Status.active
                                join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                select new { z.ProTaskId, id = y.EmployeeId, LastName = y.LastName ?? "", FirstName = y.FirstName ?? "", MiddleName = y.MiddleName ?? "", Img = y.ImgFileName ?? "", y.Status })
                               .ToList().ToLookup(z => z.ProTaskId);
            var remLookup = db.Reminders.Where(z => z.Type == "Task" && myIds.Contains(z.Reference))
                              .ToList().GroupBy(z => z.Reference).ToDictionary(g => g.Key, g => g.OrderByDescending(z => z.RDate).First());
            var updByIds = taskupLookup.Values.Where(u => u.CreatedBy != null).Select(u => u.CreatedBy).Distinct().ToList();
            var updUserLookup = db.Users.Where(u => updByIds.Contains(u.Id)).Select(u => new { u.Id, u.UserName }).ToList().ToDictionary(u => u.Id, u => u.UserName);

            var UserView = rawRows
                            .Select(r => new { o = r, tu = taskupLookup.ContainsKey(r.a.ProTaskId) ? taskupLookup[r.a.ProTaskId] : null })
                            .Select(r => new { r.o, r.tu, ldate = (r.tu != null && r.tu.CreatedDate > r.o.a.logtime) ? r.tu.CreatedDate : r.o.a.logtime,
                                               rem = remLookup.ContainsKey(r.o.a.ProTaskId) ? remLookup[r.o.a.ProTaskId] : null })
                            .Where(r =>
                                (remstatus == "0" || (r.rem != null && ((DateTime.Now <= r.rem.RDate ? "Upcoming" : "Expired") == remstatus)))
                              && (remdate == "" || (r.rem != null && r.rem.RDate.HasValue && remddate != null && r.rem.RDate.Value.Date == remddate.Value.Date))
                              && (fromdate == "" || (r.ldate != null && fdate != null && r.ldate.Value.Date <= fdate.Value.Date))
                              && (todate == "" || (r.ldate != null && tdate != null && r.ldate.Value.Date >= tdate.Value.Date))
                              && (AssTo == 0 || assignLookup[r.o.a.ProTaskId].Select(z => z.id).Contains(AssTo)))
                            .Select(r =>
                            {
                                var o = r.o;
                                var taskup = r.tu;
                                return new
                            {
                                o.a.ProTaskId,
                                o.a.TaskName,
                                o.a.TaskCode,
                                Project = o.a.ProjectId == null ? 0 : (o.b != null ? o.b.ProjectId : 0),
                                ProjectName = o.b != null ? o.b.ProjectName : null,
                                CustomerName = o.a.CustomerID == -2 ? "No Customer" : (o.d != null ? o.d.CustomerName : null),
                                CustomerID = o.a.CustomerID == -2 ? (long?)-2 : (o.a.CustomerID ?? (o.d != null ? o.d.CustomerID : (long?)null)),
                                UserName = o.e != null ? o.e.UserName : null,
                                o.a.StartDate,
                                o.a.EndDate,
                                o.a.StartTime,
                                o.a.EndTime,
                                TypeName = o.c != null ? o.c.TypeName : null,
                                o.a.VTypId,
                                o.a.VManuId,
                                o.a.VModId,
                                Priority = Enum.GetName(typeof(TaskPriority), o.a.Priority),
                                AssignedTo = assignLookup[o.a.ProTaskId].Select(z => new { z.id, z.LastName, z.FirstName, z.MiddleName, z.Img, z.Status }).Distinct().ToList(),
                                TaskStat = o.f != null ? o.f.StatusName : null,
                                EmpName = (o.s != null ? o.s.FirstName : "") + " " + (o.s != null ? o.s.LastName : ""),
                                ReminderDate = r.rem != null ? (DateTime?)r.rem.RDate : null,
                                validity = r.rem != null ? (DateTime.Now <= r.rem.RDate ? "Upcoming" : "Expired") : null,
                                o.a.Ref1,
                                o.a.Ref2,
                                o.a.Ref3,
                                o.a.Ref4,
                                o.a.Ref5,
                                o.a.Location,
                                ldate = r.ldate,
                                TaskDetails = (o.a.TaskDetails == "" || o.a.TaskDetails == null) ? o.a.Note : o.a.TaskDetails,
                                o.a.CreatedBy,
                                UserId,
                                allcheck,
                                editcheck,
                                devcheck,
                                mobmodel = mobilesno(o.a.ProTaskId),
                                Updateduser = (taskup != null && taskup.CreatedBy != null && updUserLookup.ContainsKey(taskup.CreatedBy)) ? updUserLookup[taskup.CreatedBy] : null,
                            };
                            })
                            // preserve the original "order by the grid's sort column, then cap at 300".
                            .AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).Take(300).AsEnumerable();

            //    // Apply search   
            //    UserView = UserView.Where(p => p.TaskName.ToString().ToLower().Contains(search.ToLower())
            if (txtremarks != "")
            {
                var taskupdate = db.TaskRemarks.Where(p => p.Remark.ToLower().Contains(txtremarks.ToLower())).Select(o => o.TaskId).Distinct().ToList();
                if (taskupdate.Count > 0)

                {


                    UserView = UserView.Where(o => taskupdate.Contains(o.ProTaskId));



                }
                else
                {
                    UserView = UserView.Where(o => taskupdate.Contains(-1));
                }
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            else
            {
                UserView = UserView.OrderByDescending(x => x.ldate);
            }
            recordsTotal = UserView.Count();
            var data = UserView.Distinct().Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }




        [Authorize(Roles = "Dev,My ProTask")]
        public ActionResult GetMyTaskCalendar()//TKUpdateStatus? status //IEnumerable<TKUpdateStatus> status
        {

            int days = 0;

            DateTime datecheck = DateTime.Now.AddDays(-days);




            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            bool allcheck = User.IsInRole("All ProTask");
            bool editcheck = User.IsInRole("Edit ProTask");
            bool devcheck = User.IsInRole("Dev");


            TaskPriority Prior = new TaskPriority();


            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? remddate = null;

            var taskups = from element in db.ProTaskUpdations
                          group element by element.ProTaskId
                into groups
                          select groups.OrderByDescending(p => p.CreatedDate).FirstOrDefault();
            var UserView = (from a in db.ProTasks.Where(o=>o.Branch==1)
                            join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                            from b in pro.DefaultIfEmpty()
                            join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                            from c in type.DefaultIfEmpty()
                            join e in db.Users on a.CreatedBy equals e.Id into creat
                            from e in creat.DefaultIfEmpty()
                            join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                            from d in cus.DefaultIfEmpty()
                            join f in db.TaskStatus on a.TaskStatus equals f.TaskStatusId into ttask
                            from f in ttask.DefaultIfEmpty()
                            join s in db.Employees on a.SalesPerson equals s.EmployeeId into emp
                            from s in emp.DefaultIfEmpty()
                            join i in db.Contacts on d.Contact equals i.ContactID into tmp
                            from i in tmp.DefaultIfEmpty()
                            join taskup in taskups on a.ProTaskId equals taskup.ProTaskId into tsk
                            from taskup in tsk.DefaultIfEmpty()
                                //    let x = db.LogManagers.Where(lg => lg.LogID == a.CustomerID.ToString() && lg.LogTable == "Customers").OrderByDescending(lg => lg.LogManagerID).FirstOrDefault()

                            let mobnum = db.TaskMobiles.Where(x => x.ProTaskId == a.ProTaskId).Select(x => x.MobileNo).ToList()
                            let assign = db.TaskAssigneds.Where(x => x.ProTaskId == a.ProTaskId && x.Status == "Assigned" && x.chkStatus == Status.active).Select(x => x.EmployeeId).ToList()
                            // let log = db.LogManagers.Where(lg => lg.LogID == a.ProTaskId.ToString() && lg.LogTable == "ProTasks").OrderByDescending(lg => lg.LogManagerID).FirstOrDefault()
                            // let taskup = db.ProTaskUpdations.Where(x => x.ProTaskId == a.ProTaskId).OrderByDescending(x => x.CreatedDate).FirstOrDefault()
                            let Reminder = (from z in db.Reminders
                                            where z.Type == "Task" && z.Reference == a.ProTaskId
                                            orderby z.RDate descending
                                            select new
                                            {
                                                ReminderDate = z.RDate,
                                                validity = (DateTime.Now <= z.RDate) ? "Upcoming" : "Expired",
                                            }).FirstOrDefault()
                            where assign.Contains(empId)

                            select new
                            {
                                a.ProTaskId,
                                a.TaskName,
                                a.TaskCode,
                                b.ProjectName,
                                ProjectId = a.ProjectId == null ? 0 : b.ProjectId,
                                CustomerName = a.CustomerID == -2 ? "No Customer" : (a.CustomerID == null ? d.CustomerName : d.CustomerName),
                                CustomerID = a.CustomerID == -2 ? -2 : (a.CustomerID == null ? d.CustomerID : d.CustomerID),
                                // b.ProjectId,
                                e.UserName,
                                a.StartDate,
                                a.EndDate,
                                a.StartTime,
                                a.EndTime,
                                c.TypeName,
                                //a.TaskStatus,
                                //f.Status,
                                a.Priority,

                                TaskStat = f.StatusName,
                                s.FirstName,
                                s.LastName,
                                AssignedTo = (from z in db.TaskAssigneds
                                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                              where z.ProTaskId == a.ProTaskId && z.Status == "Assigned" && z.chkStatus == Status.active
                                              select new
                                              {
                                                  id = y.EmployeeId,
                                                  LastName = (y.LastName != null) ? y.LastName : "",
                                                  FirstName = (y.FirstName != null) ? y.FirstName : "",
                                                  MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                                  Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                                  y.Status
                                              }).ToList(),
                                TskMob = (from ac in db.TaskMobiles
                                          where (ac.ProTaskId == a.ProTaskId)
                                          select new MobileViewModel
                                          {
                                              Num = ac.MobileNo
                                          }).ToList(),
                                mobiles = (from ac in db.TaskMobiles
                                           where (ac.ProTaskId == a.ProTaskId)
                                           select new
                                           {
                                               Num = ac.MobileNo
                                           }),
                                Reminder = Reminder,
                                a.Location,
                                a.Ref1,
                                a.Ref2,
                                a.Ref3,
                                a.Ref4,
                                a.Ref5,
                                ldate = ((a != null) && (taskup.CreatedDate > a.logtime)) ? taskup.CreatedDate : a.logtime,
                                a.TaskDetails,
                                a.CreatedBy,
                                UserId,
                                allcheck,
                                editcheck,
                                devcheck,
                                Updateduser = db.Users.Where(x => x.Id == taskup.CreatedBy).Select(x => x.UserName).FirstOrDefault(),
                            }



                            ).OrderByDescending(o => o.ldate).ToList().Select(o => new
                            {

                                from = o.StartTime,//o.StartDate!=null?((DateTime)o.StartDate).ToString("yyyy-MM-dd"):null,
                                to = o.EndTime,//o.EndDate!=null?((DateTime)o.EndDate).ToString("yyyy-MM-dd"):null,

                                title = o.TaskName,
                                description = "/ProTask/MyTask/?protaskid=" + o.ProTaskId,
                                repeatEvery = 0,
                                location = "Customer Name: " + o.CustomerName + "<br> " + o.Location + String.Join("<br>", o.mobiles),
                                repeatEnds = o.EndDate,
                                color = "#00FF00",
                                colorText = "#FF0000",
                                group = "Group 1"
                            });



            //SORT

            recordsTotal = UserView.Count();
            var data = UserView.Skip(0).Take(50).ToList();
            return Json(new { data = data });

        }


        [RedirectingAction]
        [Authorize(Roles = "Dev,My ProTask")]
        public ActionResult Mine()
        {
            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindTask").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ViewBag.TaskName = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.TaskType = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Projects = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.TStat = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Employee = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                         }, "Value", "Text", 1);

            ViewBag.User = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
            }, "Value", "Text", 1);

            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));

            ViewBag.SalesExecutive = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
           }, "Value", "Text", 1);


            ViewBag.Locat = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);
            ViewBag.Mobile = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                         },
                           "Value", "Text", 1);

            ProTaskViewModel vmodel = new ProTaskViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Task" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Task").ToList();
            return View(vmodel);
        }



        [HttpGet]
        //[RedirectingAction]
        //[Authorize(Roles = "Dev,ProTask Status")]
        public ActionResult EditStatus(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProTask tasks = db.ProTasks.Find(id);
            //.Where(n => n.Type == 3)
            if (tasks == null)
            {
                return NotFound();
            }
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            ProTaskUpdatViewModel vmodel = new ProTaskUpdatViewModel();

            vmodel.ProTaskId = tasks.ProTaskId;
            return PartialView(vmodel);
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

        [HttpPost]
        public ActionResult GetTasksByProject(long? projectId)
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

            var UserView = (from a in db.ProTasks.Where(o=>o.Branch==1)
                            join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                            from b in pro.DefaultIfEmpty()
                            join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                            from c in type.DefaultIfEmpty()
                            join e in db.Users on a.CreatedBy equals e.Id
                            //let f = db.ProTaskUpdations.Where(cl => cl.ProTaskId == a.ProTaskId).OrderByDescending(cl => cl.TaskUpdationID).FirstOrDefault()
                            //let i = db.TaskTeams.Where(cl => cl.Task == a.ProTaskId).OrderByDescending(cl => cl.TaskTeamId).FirstOrDefault()
                            //where f != null && a.ProjectId == projectId
                            select new
                            {
                                a.ProTaskId,
                                a.TaskName,
                                b.ProjectName,
                                b.ProjectId,
                                e.UserName,
                                a.StartDate,
                                a.EndDate,
                                a.StartTime,
                                a.EndTime,
                                c.TypeName,
                                //a.TaskStatus,
                                //f.Status,
                                a.Priority,
                                //AssignedTo = j.FirstName + " " + j.LastName,
                                //TeamMembers = (from z in db.TaskTeamMembers
                                //               where z.TaskTeamId == i.TaskTeamId
                                //                   id = y.EmployeeId,
                                //                   LastName = (y.LastName != null) ? y.LastName : "",
                                //                   FirstName = (y.FirstName != null) ? y.FirstName : "",
                                //                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                //                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                                //                   y.Status
                                //               }).ToList()
                            }).ToList().Select(o => new
                            {
                                o.ProTaskId,
                                o.TaskName,
                                Project = o.ProjectId,
                                o.ProjectName,
                                o.UserName,
                                o.StartDate,
                                o.EndDate,
                                o.StartTime,
                                o.EndTime,
                                o.TypeName,
                                //AssignedTo = (o.AssignedTo != " ") ? o.AssignedTo : "Not assigned",
                                Priority = Enum.GetName(typeof(TaskPriority), o.Priority),
                                //o.TeamMembers
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.TaskName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        //[HttpGet]

        //                   //let f = db.ProTaskUpdations.Where(cl => cl.ProTaskId == a.ProTaskId).OrderByDescending(cl => cl.TaskUpdationID).FirstOrDefault()
        //                   //let i = db.TaskTeams.Where(cl => cl.Task == a.ProTaskId).OrderByDescending(cl => cl.TaskTeamId).FirstOrDefault()
        //                   //join j in db.Employees on i.TeamLead equals j.EmployeeId into team
        //                   //from j in team.DefaultIfEmpty()
        //                   //let k = db.TaskTeams.Where(cl => cl.Task == a.ProTaskId).OrderByDescending(cl => cl.TaskTeamId).ToList()
        //                   b.ProjectId == projectId //&& h.UserId == UserId //check emp is a user
        //                       a.ProTaskId,
        //                       a.TaskName,
        //                       TeamLead = j.FirstName + " " + j.LastName,
        //                       //TeamMembers = (from z in db.TaskTeamMembers
        //                       //               join y in db.Employees on z.EmployeeId equals y.EmployeeId
        //                       //               where z.TaskTeamId == i.TaskTeamId
        //                       //               select new
        //                       //                   id = y.EmployeeId,
        //                       //                   LastName = (y.LastName != null) ? y.LastName : "",
        //                       //                   FirstName = (y.FirstName != null) ? y.FirstName : "",
        //                       //                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
        //                       //                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
        //                       //                   y.Status


        //[HttpGet]
        //                   let f = db.ProTaskUpdations.Where(cl => cl.ProTaskId == a.ProTaskId).OrderByDescending(cl => cl.TaskUpdationID).FirstOrDefault()
        //                   //join f in db.ProTaskUpdations on a.ProTaskId equals f.ProTaskId into tskup
        //                   //from f in tskup.DefaultIfEmpty()
        //                       //let i = db.TaskTeams.Where(cl => cl.Task == a.ProTaskId).OrderByDescending(cl => cl.TaskTeamId).FirstOrDefault()
        //                   where f != null && i != null && a.ProTaskId == TasKID  //&& h.UserId == UserId //check emp is a user
        //                       f.TaskUpdationID,
        //                       a.ProTaskId,
        //                       a.TaskName,
        //                       i.TaskTeamId,
        //                       TeamLead = j.FirstName + " " + j.LastName,
        //                       //TeamMembers = (from z in db.TaskTeamMembers
        //                       //               join y in db.Employees on z.EmployeeId equals y.EmployeeId
        //                       //               where z.TaskTeamId == i.TaskTeamId
        //                       //               select new
        //                       //                   id = y.EmployeeId,
        //                       //                   LastName = (y.LastName != null) ? y.LastName : "",
        //                       //                   FirstName = (y.FirstName != null) ? y.FirstName : "",
        //                       //                   MiddleName = (y.B != null) ? y.MiddleName : "",
        //                       //                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
        //                       //                   y.Status


        public ActionResult Image2()
        {
            ViewBag.image = (from b in db.TaskImages
                             join c in db.ProTasks.Where(o=>o.Branch==1) on b.ProTaskId equals c.ProTaskId
                             where c.ProTaskId == 1
                             select new TaskImageViewModel
                             {
                                 TaskImageId = b.TaskImageId,
                                 TaskId = b.ProTaskId,
                                 FileName = b.FileName,
                                 TaskName = c.TaskName
                             }).ToList();
            return View();
        }


        //[RedirectingAction]
        // [QkAuthorize(Roles = "Dev,Edit ProTask")]
        public ActionResult Images(long? id)
        {
            ViewBag.image = (from b in db.TaskImages
                             join c in db.ProTasks.Where(o=>o.Branch==1) on b.ProTaskId equals c.ProTaskId
                             where c.ProTaskId == id
                             select new TaskImageViewModel
                             {
                                 TaskImageId = b.TaskImageId,
                                 TaskId = b.ProTaskId,
                                 FileName = b.FileName,
                                 TaskName = c.TaskName
                             }).ToList();
            ViewBag.TaskId = id;
            return PartialView();
        }

        [HttpPost]
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit ProTask")]
        public ActionResult ImageAdd(long id)
        {
            bool stat = false;
            string msg;
            string description = Request.Form[Request.Form.Keys.ElementAt(0)];
            if (String.IsNullOrEmpty(description))
            {


            }
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);
                var TaskUp = new ProTaskUpdation
                {
                    ProTaskId = id,
                    //Status = TKUpdateStatus.Created,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    //TaskTeamId = teamId
                };
                db.ProTaskUpdations.Add(TaskUp);
                db.SaveChanges();
                Int64 TaskUpdId = TaskUp.TaskUpdationID;
                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {
                    string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {

                            var fileCount = db.TaskImages.Select(a => a.TaskImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                            var fileName = Path.GetFileName(file.FileName);

                            String extension = Path.GetExtension(fileName);


                            String newName = Path.GetFileNameWithoutExtension(file.FileName)+" "+fileCount + extension;
                            string newFName = Path.GetFileNameWithoutExtension(file.FileName)+" "+fileCount + extension;
                            var FStatus = Status.active;
                            var thumbName = "";
                            var resizeName = "";
                            string[] validextenstions = { ".jpg", ".jfif", ".png", ".jpeg", ".PNG", ".JPG" };
                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg" || extension == ".PNG" || extension == ".JPG")
                            {
                                string defaultext = "";
                                thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1)+ defaultext;
                                string exten = Path.GetExtension(thumbName);
                                if (!validextenstions.Contains(exten))
                                    defaultext = ".jpg";
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), thumbName);
                                
                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1)+ defaultext;
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                newFName =  newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";

                            }
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), newName);
                            file.SaveAs(newName);

                            var taskimg = new TaskImage
                            {
                                ProTaskId = id,
                                TaskUpdationID = TaskUpdId,
                                description = description,
                                FileName = newFName,//Path.GetFileName(file.FileName),
                                Status = FStatus,
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                CreatedBy = UserId,
                            };
                            db.TaskImages.Add(taskimg);
                            db.SaveChanges();


                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg" || extension == ".PNG" || extension == ".JPG")
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
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                    lgimg.Save(resizeName);
                                }

                            }
                        }
                    }
                }

                com.addlog(LogTypes.Created, UserId, "TaskImage", "TaskImages", findip(), id, "Task Image Added Successfully");


            }
            msg = "Successfully added Task image.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }



        [HttpPost]
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit ProTask")]
        public ActionResult ImageAddNew(long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);
                var TaskUp = new ProTaskUpdation
                {
                    ProTaskId = id,
                    //Status = TKUpdateStatus.Created,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    //TaskTeamId = teamId
                };
                db.ProTaskUpdations.Add(TaskUp);
                db.SaveChanges();
                Int64 TaskUpdId = TaskUp.TaskUpdationID;
                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {
                    string path = LegacyWeb.MapPath("~/uploads/protaskdocumentsNew/");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {

                            var fileCount = db.TaskImages.Select(a => a.TaskImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                            var fileName = Path.GetFileName(file.FileName);

                            String extension = Path.GetExtension(fileName);


                            String newName = fileCount + extension;
                            string newFName = fileCount + extension;
                            var FStatus = Status.active;
                            var thumbName = "";
                            var resizeName = "";
                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                            {
                                thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocumentsNew/"), thumbName);

                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocumentsNew/"), resizeName);
                                newFName = "resize_" + newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";

                            }
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocumentsNew/"), newName);
                            file.SaveAs(newName);

                            //    ProTaskId = id,
                            //    TaskUpdationID = TaskUpdId,

                            //    Status = FStatus,
                            //    CreatedBy = UserId,




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
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocumentsNew/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocumentsNew/"), resizeName);
                                    lgimg.Save(resizeName);
                                }

                            }
                        }
                    }
                }

                com.addlog(LogTypes.Created, UserId, "TaskImage", "TaskImages", findip(), id, "Task Image Added Successfully");


            }
            msg = "Successfully added Task image.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit ProTask")]
        public JsonResult ImageDelete(long key)
        {
            bool stat = false;
            string msg;
            TaskImage tskImg = db.TaskImages.Find(key);
            if (tskImg != null)
            {
                db.TaskImages.Remove(tskImg);
                db.SaveChanges();
            }
            string fullPath = LegacyWeb.MapPath("~/uploads/protaskdocuments/" + tskImg.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            string fullPaththumb = LegacyWeb.MapPath("~/uploads/protaskdocuments/" + "thumb_" + tskImg.FileName);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }

            var UserId = User.Identity.GetUserId();

            com.addlog(LogTypes.Deleted, UserId, "ProTask", "TaskImages", findip(), tskImg.TaskImageId, "Task Image Deleted Successfully");


            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted Task Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }


        //[RedirectingAction]
        //[Authorize(Roles = "Dev,MyTask Calender")]
        public ActionResult MyTaskCalender()
        {
            return View();
        }
        //[HttpGet]
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,MyTask Calender")]

        //               let f = db.ProTaskUpdations.Where(cl => cl.ProTaskId == a.ProTaskId).OrderByDescending(cl => cl.TaskUpdationID).FirstOrDefault()
        //               let i = db.TaskTeams.Where(cl => cl.Task == a.ProTaskId).OrderByDescending(cl => cl.TaskTeamId).FirstOrDefault()
        //               let k = db.TaskTeams.Where(cl => cl.Task == a.ProTaskId).OrderByDescending(cl => cl.TaskTeamId).ToList()
        //               //let h = db.TaskTeams
        //               //       .Join(db.Employees,
        //               //          post => post.TeamLead,
        //               //          meta => meta.EmployeeId,
        //               //          (post, meta) => new { Post = post, Meta = meta }) // selection
        //               //       .Where(postAndMeta => postAndMeta.Meta.UserId == UserId && postAndMeta.Post.Task == a.ProTaskId).FirstOrDefault()

        //               //let l = db.TaskTeamMembers.
        //               //    Join(db.Employees,
        //               //    post1 => post1.EmployeeId,
        //               //      meta1 => meta1.EmployeeId,
        //               //      (post1, meta1) => new { Post1 = post1, Meta1 = meta1 }) // selection
        //               //   .Where(postAndMeta1 => postAndMeta1.Meta1.UserId == UserId && postAndMeta1.Post1.TaskTeamId == i.TaskTeamId).FirstOrDefault()
        //               //where( f != null && i != null) &&( h.Post != null || l.Post1!=null)//check emp is a user
        //              && (EF.Functions.DateDiffDay(a.StartDate, today) == 0)
        //                   a.ProTaskId,
        //                   a.TaskName,
        //                   b.ProjectName,
        //                   ProjectId = a.ProjectId == null ? 0 : b.ProjectId,

        //                   e.UserName,
        //                   a.StartDate,
        //                   a.EndDate,
        //                   a.StartTime,
        //                   a.EndTime,
        //                   c.TypeName,
        //                   a.TaskStatus,
        //                   f.Status,
        //                   a.Priority,
        //                   AssignedTo = j.FirstName + " " + j.LastName,
        //                   Assign = k,
        //                   Access = (j.UserId == UserId) ? "Allow" : "No",
        //                   //TeamMembers = (from z in db.TaskTeamMembers
        //                   //               join y in db.Employees on z.EmployeeId equals y.EmployeeId
        //                   //               where z.TaskTeamId == i.TaskTeamId
        //                   //               select new
        //                   //                   id = y.EmployeeId,
        //                   //                   LastName = (y.LastName != null) ? y.LastName : "",
        //                   //                   FirstName = (y.FirstName != null) ? y.FirstName : "",
        //                   //                   MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
        //                   //                   Img = (y.ImgFileName != null) ? y.ImgFileName : "",
        //                   //                   y.Status

        //               }).ToList().Select(o => new
        //                   o.ProTaskId,
        //                   o.TaskName,
        //                   Project = o.ProjectId,
        //                   o.ProjectName,
        //                   o.UserName,
        //                   o.StartDate,
        //                   o.EndDate,
        //                   o.StartTime,
        //                   o.EndTime,
        //                   o.TypeName,
        //                   AssignedTo = (o.AssignedTo != " ") ? o.AssignedTo : "Not assigned",
        //                   o.TeamMembers,
        //                   o.Assign,
        //                   o.Access

        //                        e.Access,
        //                        checkstatus,
        //                        id = e.ProTaskId,
        //                        title = e.TaskStatus,
        //                        description = "<br /> Task : " + e.TaskName + "<br /> Project : " + e.ProjectName + "<br /> ",
        //                        allDay = false


        public JsonResult SearchProTaskbyidtwo(string q, string x, long? customer)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {




                var serialisedJson1 = (
                     from a in db.ProTasks
                     where (customer == null || a.CustomerID == customer) && (a.TaskName.ToLower().Contains(q.ToLower()) || a.TaskCode.ToLower().Contains(q.ToLower()) || a.TaskCode.Contains(q) || a.TaskName.Contains(q))
                     orderby a.logtime descending
                     select new SelectFormat
                     {
                         text = a.TaskCode + " - " + a.TaskName, //each json object will have 
                        id = a.ProTaskId
                     }).ToList();
                var serialisedJson2 = (
                    from a in db.ProTasks

                    join b in db.customerleadrelation on a.CustomerID equals b.leadid
                    where (customer == null || b.customerid == customer) && (a.TaskName.ToLower().Contains(q.ToLower()) || a.TaskCode.ToLower().Contains(q.ToLower()) || a.TaskCode.Contains(q) || a.TaskName.Contains(q))
                    orderby a.logtime descending
                    select new SelectFormat
                    {
                        text = a.TaskCode + " - " + a.TaskName, //each json object will have 
                        id = a.ProTaskId
                    }).ToList();
                serialisedJson = serialisedJson1.Union(serialisedJson2).ToList();

            }
            else
            {
                var serialisedJson1 = (
                    from a in db.ProTasks
                    where (customer == null || a.CustomerID == customer)
                    orderby a.logtime descending
                    select new SelectFormat
                    {
                        text = a.TaskCode + " - " + a.TaskName, //each json object will have 
                        id = a.ProTaskId
                    }).ToList();
                var serialisedJson2 = (
                   from a in db.ProTasks
                   join b in db.customerleadrelation on a.CustomerID equals b.leadid
                   where (customer == null || b.customerid == customer)
                   orderby a.logtime descending
                   select new SelectFormat
                   {
                       text = a.TaskCode + " - " + a.TaskName, //each json object will have 
                        id = a.ProTaskId
                   }).ToList();
                serialisedJson = serialisedJson1.Union(serialisedJson2).ToList();


            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchProTaskbyid(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProTasks.Where(p => p.TaskName.ToLower().Contains(q.ToLower()) || p.TaskCode.ToLower().Contains(q.ToLower()) || p.TaskCode.Contains(q) || p.TaskName.Contains(q))//p.TaskStat==Stat.Open 
                       .OrderByDescending(o => o.logtime)
                    .Select(b => new SelectFormat
                    {
                        text = b.TaskCode + " - " + b.TaskName, //each json object will have 
                        id = b.ProTaskId
                    })
                                  .ToList();
            }
            else
            {
                serialisedJson = db.ProTasks.OrderByDescending(o => o.logtime).Select(b => new SelectFormat//Where(b=> b.TaskStat == Stat.Open)

                {
                    text = b.TaskCode + " - " + b.TaskName, //each json object will have 
                    id = b.ProTaskId
                }).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchProTask(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProTasks.Where(p => p.TaskName.ToLower().Contains(q.ToLower()) || p.TaskCode.ToLower().Contains(q.ToLower()) || p.TaskCode.Contains(q) || p.TaskName.Contains(q))//p.TaskStat==Stat.Open 
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.TaskCode + " - " + b.TaskName, //each json object will have 
                                      id = b.ProTaskId
                                  })
                                  .OrderByDescending(b => b.id).ToList();
            }
            else
            {
                serialisedJson = db.ProTasks.Select(b => new SelectFormat//Where(b=> b.TaskStat == Stat.Open)
                {
                    text = b.TaskCode + " - " + b.TaskName, //each json object will have 
                    id = b.ProTaskId
                }).OrderByDescending(b => b.id).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson.Take(10).ToList());
        }
        public JsonResult SearchProTaskTag(string q, string x)
        {
            List<SelectFormatDisabled> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProTasks.Where(p => p.TaskName.ToLower().Contains(q.ToLower()) || p.TaskCode.ToLower().Contains(q.ToLower()) || p.TaskCode.Contains(q) || p.TaskName.Contains(q))//p.TaskStat==Stat.Open 
                                  .Select(b => new SelectFormatDisabled
                                  {
                                      text = b.TaskCode + " - " + b.TaskName, //each json object will have 
                                      id = b.ProTaskId.ToString(),
                                      disabled = "true"
                                  })
                                  .OrderByDescending(b => b.id).ToList();
            }
            else
            {
                serialisedJson = db.ProTasks.Select(b => new SelectFormatDisabled//Where(b=> b.TaskStat == Stat.Open)
                {
                    text = b.TaskCode + " - " + b.TaskName, //each json object will have 
                    id = b.ProTaskId.ToString(),
                    disabled = "true"
                }).OrderByDescending(b => b.id).ToList();

            }//
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormatDisabled() { id = "0", text = "Select Task" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatDisabled() { id = "0", text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete ProTask")]
        public ActionResult DeleteAllTask(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = DeleteTask(arr);
                if (chk == true)
                {
                    count++;
                }
                else
                {
                    notdel++;
                }
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Tasks, Unable to Delete " + notdel + " Tasks. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Tasks.", true);
            }
            else
            {
                Success("Deleted " + count + " Tasks.", true);
            }
            return RedirectToAction("Index", "ProTask");
        }

        private Boolean DeleteTask(long tskid)
        {
            ProTask pro = db.ProTasks.Find(tskid);

            var tskupd = db.ProTaskUpdations.Where(cl => cl.ProTaskId == tskid).OrderByDescending(cl => cl.TaskUpdationID).FirstOrDefault();
            var UserId = User.Identity.GetUserId();

            var tstatus = db.ProTaskUpdations.Where(a => a.ProTaskId == pro.ProTaskId);
            if (tstatus != null)
            {
                db.ProTaskUpdations.RemoveRange(db.ProTaskUpdations.Where(a => a.ProTaskId == pro.ProTaskId));
                db.SaveChanges();
            }


            var ptskimg = db.TaskImages.Where(a => a.ProTaskId == pro.ProTaskId);
            if (ptskimg != null)
            {
                db.TaskImages.RemoveRange(db.TaskImages.Where(a => a.ProTaskId == pro.ProTaskId));
                db.SaveChanges();
            }

            var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId);
            if (tskasgn != null)
            {
                db.TaskAssigneds.RemoveRange(db.TaskAssigneds.Where(a => a.ProTaskId == pro.ProTaskId));
                db.SaveChanges();
            }

            var tskrmk = db.TaskRemarks.Where(a => a.TaskId == pro.ProTaskId);
            if (tskrmk != null)
            {
                db.TaskRemarks.RemoveRange(db.TaskRemarks.Where(a => a.TaskId == pro.ProTaskId));
                db.SaveChanges();
            }

            var tskasgntype = db.TaskAssignTypes.Where(a => a.ProTaskId == pro.ProTaskId);
            if (tskasgntype != null)
            {
                db.TaskAssignTypes.RemoveRange(db.TaskAssignTypes.Where(a => a.ProTaskId == pro.ProTaskId));
                db.SaveChanges();
            }

            var tskmob = db.TaskMobiles.Where(a => a.ProTaskId == pro.ProTaskId);
            if (tskmob != null)
            {
                db.TaskMobiles.RemoveRange(db.TaskMobiles.Where(a => a.ProTaskId == pro.ProTaskId));
                db.SaveChanges();
            }

            db.ProTasks.Remove(pro);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "ProTask", "ProTasks", findip(), pro.ProTaskId, "Task Deleted Successfully");
            return true;

        }


        public JsonResult SearchProject(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Projects
                                  join c in db.Customers on b.Customer equals c.CustomerID into cust
                                  from c in cust.DefaultIfEmpty()
                                  join d in db.Contacts on c.Contact equals d.ContactID into cont
                                  from d in cont.DefaultIfEmpty()
                                  join j in db.Mobiles on c.Contact equals j.Contact into mobi
                                  from j in mobi.DefaultIfEmpty()
                                  where (b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || b.ProjectName.Contains(q)) || (b.ProCode.Contains(q)) ||
                                        (c.CustomerName.ToLower().Contains(q.ToLower()) || c.CustomerName.Contains(q)) || (j.MobileNum.Contains(q)) || (d.Phone.Contains(q))
                                  //&& b.ProjectStat == Stat.Open
                                  select new SelectFormat
                                  {
                                      text = b.ProjectName,
                                      id = b.ProjectId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Projects.Select(b => new SelectFormat//Where(b=> b.TaskStat == Stat.Open)
                {
                    text = b.ProjectName, //each json object will have 
                    id = b.ProjectId
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Project" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchTaskByProject(string q, string x, long? project)
        {
            string NoProj = "--No Task--";
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            List<SelectStatusFormat> serialisedJson = (from a in db.ProTasks.Where(o=>o.Branch==1)
                                                       join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                                                       where project != null && a.ProjectId == project
                                                       //&& a.TaskStatus == Stat.Open
                                                        && (q==null||q=="")||a.TaskName.Contains(q)
                                                       select new SelectStatusFormat
                                                       {
                                                           id = a.ProTaskId,
                                                           text = a.TaskCode + "- " + a.TaskName,
                                                       }).Distinct().OrderBy(b => b.text).ToList();

            if (x == "--No Task--" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || NoProj.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = NoProj };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "Select Task" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "All" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchTaskByCustomer(string q, string x, long? customerid)
        {
            string NoProj = "--No Task--";
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            List<SelectStatusFormat> serialisedJson = (from a in db.ProTasks.Where(o=>o.Branch==1)
                                                       join b in db.Customers on a.CustomerID equals b.CustomerID into pro
                                                       where customerid != null && a.CustomerID == customerid
                                                       //&& a.TaskStatus == Stat.Open
                                                        && (q == null || q == "") || a.TaskName.Contains(q) || a.TaskCode.Contains(q)
                                                       select new SelectStatusFormat
                                                       {
                                                           id = a.ProTaskId,
                                                           text = a.TaskCode+"- "+a.TaskName,
                                                       }).Distinct().OrderBy(b => b.text).ToList();

            if (x == "--No Task--" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || NoProj.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = NoProj };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "Select Task" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "All" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchCustomer(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Customers
                                  join c in db.Contacts on b.Contact equals c.ContactID into cust
                                  from c in cust.DefaultIfEmpty()
                                  join j in db.Mobiles on b.Contact equals j.Contact into mobi
                                  from j in mobi.DefaultIfEmpty()
                                  where (b.CustomerName.ToLower().Contains(q.ToLower()) || b.CustomerCode.ToLower().Contains(q.ToLower()) || (b.CustomerName.Contains(q)) || (b.CustomerCode.Contains(q)) ||
                                        (j.MobileNum.Contains(q)) || (c.Phone.Contains(q)))
                                  select new SelectFormat
                                  {
                                      text = b.CustomerName,
                                      id = b.CustomerID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Customers.Select(b => new SelectFormat
                {
                    text = b.CustomerName, //each json object will have 
                    id = b.CustomerID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Customer" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        [HttpPost]
        public ActionResult GetAllRemarks(long TaskId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            DateTime currdate = System.DateTime.Now.AddYears(-1);
            var v = (from a in db.TaskRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.TaskStatus on a.TaskStatusID equals c.TaskStatusId
                     where a.TaskId == TaskId
                     && a.CreatedDate>currdate
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.TaskRemarkId,
                         a.CreatedDate,
                         empnae = b.UserName,
                         a.Remark,
                         Status = c.StatusName,
                         Statusid = c.TaskStatusId,
                         ProTaskId = a.TaskId,
                         RImages = "",
                     }).ToList().Select(o => new
                     {
                        // o.id,
                         o.CreatedDate,
                         o.empnae,
                         o.Remark,
                         o.Status,
                         o.ProTaskId,
                         //o.TaskDocumentId,
                         //o.FileName,
                         o.RImages,
                     //    o.Statusid
                     });
            var taskmainremarks = (from a in db.ProTasks
                                   where a.ProTaskId == TaskId
                            && a.CreatedDate > currdate
                                   select new
                                   {
                                      // id = a.ProTaskId,
                                       CreatedDate = a.CreatedDate,
                                       empnae = "",
                                       Remark = a.Note,
                                       Status = "",
                                       ProTaskId = a.ProTaskId,
                                       RImages ="",
                                     //  Statusid=a.ProTaskId,

                                   }).ToList().Select(o => new
                                   {
                                       //o.id,
                                       o.CreatedDate,
                                       o.empnae,
                                       o.Remark,
                                       o.Status,
                                       o.ProTaskId,
                                       //o.TaskDocumentId,
                                       //o.FileName,
                                       o.RImages,
                                       //o.Statusid
                                   }).ToList() ;
            var addedremarks = (from a in db.AddedRemarks
                                join b in db.Users on a.AddedUser equals b.Id into emp
                                from b in emp.DefaultIfEmpty()
                                where a.TransactionId == TaskId && a.TransactionType == "ProTaskRemarks"

 && a.CreatedDate > currdate
                                select new
                                {
                                    id = a.RemarkId,
                                    CreatedDate = a.CreatedDate,
                                    empnae = b.UserName,
                                    Remark = a.Remarks,
                                    Status = "",
                                    ProTaskId = a.TransactionId,
                                    //o.TaskDocumentId,
                                    //o.FileName,
                                   RImages ="",
                                    Statusid = a.TransactionId
                                }).ToList().Select(o => new
                                {
                                   
                                    o.CreatedDate,
                                    o.empnae,
                                    o.Remark,
                                    o.Status,
                                    o.ProTaskId,
                                    //o.TaskDocumentId,
                                    //o.FileName,
                                    o.RImages,
                                   
                                }).ToList();
           
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
            }
            v = addedremarks.Union(v);
            recordsTotal = v.Count();
            var data = v.Where(o => o.Remark != "Edited" ).Distinct().OrderByDescending(o=>o.CreatedDate).ToList();
            data = taskmainremarks.Union(data).Where(o => o.Remark != "Edited").ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpPost]
        public ActionResult GetAllRemarkstaskcust(long id)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find Order Column

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            DateTime currdate = System.DateTime.Now.AddYears(-1);
            var v = (from a in db.TaskRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.TaskStatus on a.TaskStatusID equals c.TaskStatusId
                     join d in db.ProTasks on a.TaskId equals d.ProTaskId
                     where d.CustomerID == id
                     && a.CreatedDate > currdate
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.TaskRemarkId,
                         a.CreatedDate,
                         TaskName=d.TaskCode+ " "+d.TaskName,
                         empnae = b.UserName,
                         a.Remark,
                         Status = c.StatusName,
                         Statusid = c.TaskStatusId,
                         ProTaskId = a.TaskId,
                         RImages = "",
                     }).ToList().Select(o => new
                     {
                         // o.id,
                         o.TaskName,
                         o.CreatedDate,
                         
                         o.empnae,
                         o.Remark,
                         o.Status,
                         o.ProTaskId,
                         //o.TaskDocumentId,
                         //o.FileName,
                         o.RImages,
                         //    o.Statusid
                     });
            var taskmainremarks = (from a in db.ProTasks
                                   where a.CustomerID == id
                            && a.CreatedDate > currdate
                                   select new
                                   {
                                       // id = a.ProTaskId,
                                       CreatedDate = a.CreatedDate,
                                       empnae = "",
                                       TaskName = a.TaskCode + " " + a.TaskName,
                                       Remark = a.Note,
                                       Status = "",
                                       ProTaskId = a.ProTaskId,
                                       RImages = "",
                                       //  Statusid=a.ProTaskId,

                                   }).ToList().Select(o => new
                                   {
                                       //o.id,
                                       o.TaskName,
                                       o.CreatedDate,
                                
                                       o.empnae,
                                       o.Remark,
                                       o.Status,
                                       o.ProTaskId,
                                       //o.TaskDocumentId,
                                       //o.FileName,
                                       o.RImages,
                                       //o.Statusid
                                   }).ToList();
            var addedremarks = (from a in db.AddedRemarks
                                join b in db.Users on a.AddedUser equals b.Id into emp
                                from b in emp.DefaultIfEmpty()
                                join d in db.ProTasks on a.TransactionId equals d.ProTaskId
                                
                                where d.CustomerID == id && a.TransactionType == "ProTaskRemarks"

 && a.CreatedDate > currdate
                                select new
                                {
                                    id = a.RemarkId,
                                    CreatedDate = a.CreatedDate,
                                    empnae = b.UserName,
                                    TaskName = d.TaskCode + " " + d.TaskName,
                                    Remark = a.Remarks,
                                    Status = "",
                                    ProTaskId = a.TransactionId,
                                    //o.TaskDocumentId,
                                    //o.FileName,
                                    RImages = "",
                                    Statusid = a.TransactionId
                                }).ToList().Select(o => new
                                {
                                    o.TaskName,
                                    o.CreatedDate,
                                    o.empnae,
                                    o.Remark,
                                    o.Status,
                                    o.ProTaskId,
                                    //o.TaskDocumentId,
                                    //o.FileName,
                                    o.RImages,

                                }).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
       
            v = addedremarks.Union(v);
            recordsTotal = v.Count();
            var data = v.Where(o => o.Remark != "Edited").Distinct().OrderByDescending(o => o.CreatedDate).ToList();
            data = taskmainremarks.Union(data).Where(o => o.Remark != "Edited").OrderBy(o=>o.TaskName).ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpPost]
        public ActionResult GetAllDocuments(long TaskId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var v = (from a in db.TaskDocuments
                     join b in db.DocumentTypes on a.DocumentTypeID equals b.ID
                     where a.ProtaskID == TaskId
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.TaskDocumentId,
                         a.CreatedDate,
                         Notes = a.Notes,
                         filename=a.FileName,
                         docname=b.Name



                     }).OrderByDescending(o=>o.CreatedDate).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [HttpPost]
        public ActionResult GetAllNotes(long? TaskId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.TaskRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.TaskStatus on a.TaskStatusID equals c.TaskStatusId into stat
                     from c in stat.DefaultIfEmpty()
                     where a.TaskRemarkId == TaskId
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.TaskRemarkId,
                         a.CreatedDate,
                         empnae = b.UserName,
                         a.Remark,
                         Status = c.StatusName,
                         Statusid = c.TaskStatusId,
                         ProTaskId = a.TaskId,
                         RImages = (from z in db.TaskRemarks
                                    join y in db.TaskImages on z.TaskRemarkId equals y.TaskRemarkId into img
                                    from y in img.DefaultIfEmpty()
                                    where y.TaskUpdationID == a.TaskUpdationID
                                    select new
                                    {
                                        // y.TaskImageId,
                                        FileName = (y.FileName != null) ? y.FileName : "",
                                        y.TaskUpdationID,
                                        // y.ProTaskId,
                                    }).ToList(),
                         CheckItems = (from aa in db.RemarkChecklists
                                       join ac in db.ChecklistItems on aa.Checklistitemid equals ac.Id into chk
                                       from ac in chk.DefaultIfEmpty()
                                       where aa.Remark == a.TaskRemarkId
                                       select new
                                       {
                                           Id = ac.Id,
                                           Name = ac.ListName,
                                           Note = aa.Note,
                                           Chck = aa.Check,
                                           Remark = aa.Id
                                       }).ToList()
                     }).ToList().Select(o => new
                     {
                         o.id,
                         o.CreatedDate,
                         o.empnae,
                         o.Remark,
                         o.Status,
                         o.ProTaskId,
                         //o.TaskDocumentId,
                         //o.FileName,
                         o.RImages,
                         o.Statusid,
                         o.CheckItems
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        public ActionResult AddAmcRemark(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProTask cus = db.ProTasks.Find(id);

            if (cus == null)
            {
                return NotFound();
            }
            var AmcRemarks = new AddedRemarksvm
            {
                TransactionId = cus.ProTaskId,
               TransactionType = "ProTaskRemarks"


            };

            return PartialView(AmcRemarks);
        }
        [HttpPost]
        public ActionResult GetAllRemarksadded(long? RequisitionId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            DateTime rmdate = System.DateTime.Now.AddYears(-2);

            var v = (from a in db.AddedRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     where a.TransactionId == RequisitionId && a.TransactionType == "ProTaskRemarks" && a.Remarks != null
                     orderby a.CreatedDate descending

                     select new
                     {
                         // id = a.RemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remarks,
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();


            var data = v.ToList().Select(o => new
            {
                CreatedDate = Convert.ToDateTime(o.CreatedDate.ToString("yyyy-MM-dd HH:mm")),
                EmpName = o.EmpName,
                o.Remarks,
            }).Distinct().ToList();
            recordsTotal = data.Count();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [HttpPost]
        public JsonResult AddAmcRemarks(AddedRemarksvm amcremarks)
        {
            Int64 amcid = amcremarks.TransactionId;
            bool exist = db.AddedRemarks.Any(o => o.TransactionId == amcid && o.TransactionType == "ProTaskRemarks" && o.Remarks == amcremarks.Remarks && EF.Functions.DateDiffMinute(o.CreatedDate, System.DateTime.Now) <= 0);
            if (exist)
            {

                return Json(new { msg = "Success", status = true });
            }
            if (ModelState.IsValid)
            {
                if (amcremarks.Remarks != null)
                {
                    Common com = new Common();
                    var UserId = User.Identity.GetUserId();
                    var Today = Convert.ToDateTime(System.DateTime.Now);




                    AddedRemarks Obj = new AddedRemarks
                    {
                        TransactionId = amcremarks.TransactionId,
                        TransactionType = "ProTaskRemarks",
                        Remarks = amcremarks.Remarks,
                        AddedUser = UserId,
                        CreatedDate = Today,
                        nextdate = Today,
                        nexttime = Today,
                    };
                    db.AddedRemarks.Add(Obj);
                    db.SaveChanges();

                    //AmcUpdation AmcUps = new AmcUpdation
                    //    TransId = amcid,
                    //    TransType = "Amc",
                    //    CreatedBy = UserId,
                    //    CreatedDate = Today,
                    //    Remarks = "",




                    //To Update Status and LogTime in Amc Table
                    ProTask AmcObj = db.ProTasks.Find(amcid);


                    AmcObj.logtime = Today;

                    db.Entry(AmcObj).State = EntityState.Modified;
                    db.SaveChanges();


                    com.addlog(LogTypes.Created, UserId, "ProTaskRemarks", "ProTaskRemarks", findip(), amcid, "ProTaskRemarks  Added Successfully..");
                    Success("Remarks added successfully...", true);
                }
            }
            else
            {
                Danger("Failed to add Remarks...", true);
            }
            return Json(new { msg = "Success", status = true });

        }
        public ActionResult AddRemark(long id)
        {
            var tremark = new RemarklistViewModel

            {
                TaskId = id
            };
            var taskgroup = db.ProTaskTypes
                          .Select(s => new
                          {
                              ID = s.TaskTypeId,
                              Name = s.TypeName
                          })
                          .ToList();

            ViewBag.TaskGroups = QkSelect.List(taskgroup, "ID", "Name");

            var pstat = db.TaskStatus
              .Select(s => new
              {
                  ID = s.TaskStatusId,
                  Name = s.StatusName
              })
              .ToList();
            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.DocList = db.DocumentTypes.ToList();
            var UserId = User.Identity.GetUserId();
            ViewBag.Stat = QkSelect.List(pstat, "ID", "Name");
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");
            tremark.AssignedMembers = db.TaskAssigneds.Where(a => a.ProTaskId == id && a.chkStatus == Status.active && a.Status == "Assigned" && a.AssignBy == UserId).Select(a => a.EmployeeId).ToList().ToArray() ?? null;
            tremark.tasktype = db.ProTasks.Where(o => o.ProTaskId == id).Select(o => o.TaskType).FirstOrDefault();
            var taskstatusavailable = (from a in db.TaskGroup
                                       where a.TaskTypeId == tremark.tasktype
                                       select a).ToList();
            if (taskstatusavailable.Count() == 0)
                tremark.tasktype = null;
            return PartialView(tremark);
        }
        [HttpGet]
        public ActionResult locationinstr()
        {
            return PartialView();
        }
        [HttpPost]
        public JsonResult AddRemarks(RemarklistViewModel tremark)
        {
            string msg;
            bool stat = false;
            string lat = "";
            string log = "";
            lat = Request.Form["lat"];
            log = Request.Form["log"];
            if(lat!="")
            {
                lat = "<a href='https://maps.google.com/?q=" + lat + "," + log + "'>Open in Google Map</a> point -> " + lat;
            }

            //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            DateTime logintime = System.DateTime.Now;
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var today = System.DateTime.Now;
                Int64 proId = tremark.TaskId;
                long[] stid = new long[] { 5, 6, 10017, 10018 };

                if (!stid.Contains((long)tremark.TaskStatusID))
                {
                    var empatt = db.EmpAttendances.Where(o => o.protaskid == null && o.logout == null && o.EmployeeName == UserId).OrderByDescending(o => o.login).FirstOrDefault();
                    if (empatt != null)
                    {
                    }
                    else
                    {
                        var taskassigned = (from a in db.TaskAssigneds
                                            join b in db.TeamMembers on a.EmployeeId equals b.EmployeeId
                                            where b.TeamId == 4 &&
                                            a.ProTaskId == proId && a.Status == "Assigned" && a.chkStatus == 0
                                            select new
                                            {
                                                a.EmployeeId
                                            }).Distinct().ToArray();


                       
                        foreach (var empid in taskassigned)
                        {
                            var uid = db.Employees.Where(o => o.EmployeeId == empid.EmployeeId).Select(o => o.UserId).FirstOrDefault();
                            var atttendance = new EmpAttendance
                            {
                                EmployeeName = uid,
                                Status = "Active",
                                login = logintime,
                                latitude = lat,
                                logitude = log,
                                protaskid= proId,
                                
                            };
                        }
                    }

                    var temp = db.EmpAttendances.Where(o => o.protaskid == proId && o.logout==null&&o.login== logintime).ToList();
                    foreach (var tt in temp)
                    {
                        var empattid = db.EmpAttendances.Where(o => o.protaskid == proId && o.EmployeeName == tt.EmployeeName).OrderByDescending(o => o.Id).FirstOrDefault();

                        EmpAttDetails empdt = new EmpAttDetails
                        {
                            protaskid = proId,
                            taskstatusid = (long)tremark.TaskStatusID,
                            userid = tt.EmployeeName,
                            starttime = logintime,
                            empattid = empattid.Id,
                        };
                    }
                }
                else if (stid.Contains((long)tremark.TaskStatusID))
                {
                    var taskassigned = (from a in db.TaskAssigneds
                                        join b in db.TeamMembers on a.EmployeeId equals b.EmployeeId
                                        where b.TeamId == 4 &&
                                        a.ProTaskId == proId && a.Status == "Assigned" && a.chkStatus == 0
                                        select new
                                        {
                                            a.EmployeeId
                                        }).Distinct().ToArray();


                    foreach (var emp in taskassigned)
                    {
                        var usid = db.Employees.Where(o => o.EmployeeId == emp.EmployeeId).Select(o => o.UserId).FirstOrDefault();
                        var empatt2 = db.EmpAttendances.Where(o => o.protaskid == proId && o.logout == null && o.EmployeeName == usid).OrderByDescending(o => o.login).FirstOrDefault();
                        if (empatt2 != null)
                        {
                            var logouttime = System.DateTime.Now;
                            var Userid = usid;
                            var maxo = from a in db.EmpAttendances
                                       where a.EmployeeName == Userid
                                       orderby a.login descending
                                       select a.Id;
                            var lastid = maxo.FirstOrDefault();
                            EmpAttendance lastlog = db.EmpAttendances.Find(lastid);
                           









                            

















                            var atttendance = new EmpAttendance
                            {
                                EmployeeName = usid,
                                Status = "Active",
                                login = logintime,
                                latitude = lat,
                                logitude = log,
                            };
                        }
                    }
               
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                
                }

                var tskstat = db.ProTasks.Where(a => a.ProTaskId == proId).Select(a => a.TaskStatus).FirstOrDefault();
                var oldtskstat = GetStatusName(tskstat);
                var newtskstat = GetStatusName(tremark.TaskStatusID);
                ProTaskUpdation TaskUps = new ProTaskUpdation
                {
                    ProTaskId = proId,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    Remarks = oldtskstat + " Change To " + newtskstat,
                };
                db.ProTaskUpdations.Add(TaskUps);
                db.SaveChanges();
                com.updateprotaskdate(proId);
                Int64 TaskUpdId = TaskUps.TaskUpdationID;
                var docinfo = new TaskRemark
                {
                    CreatedDate = today,
                    TaskStatusID = tremark.TaskStatusID,
                    Remark = tremark.Remark + lat + "," + log,
                    AddedUser = UserId,
                    TaskId = tremark.TaskId,
                    TaskUpdationID = TaskUpdId,

                };
                db.TaskRemarks.Add(docinfo);
                db.SaveChanges();
                Int64 TaskRemarkId = docinfo.TaskRemarkId;
                if (tremark.bstmodel != null)
                {
                    foreach (var arr in tremark.bstmodel)
                    {
                        RemarkChecklist remlist = new RemarkChecklist();
                        remlist.Checklistitemid = arr.Id;
                        remlist.Note = arr.Note;
                        remlist.Check = (arr.Check == "on") ? true : false;
                        remlist.Remark = TaskRemarkId;
                        db.RemarkChecklists.Add(remlist);
                        db.SaveChanges();
                    }
                }


                // fileupload
                var files = tremark.fileupload;
                if (files[0] !=null)
                {
                    string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {

                            var fileCount = db.TaskImages.Select(a => a.TaskImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                            var fileName = Path.GetFileName(file.FileName);

                            String extension = Path.GetExtension(fileName);


                            String newName = fileCount + extension;
                            string newFName = fileCount + extension;
                            var FStatus = Status.active;
                            var thumbName = "";
                            var resizeName = "";
                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                            {
                                thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), thumbName);

                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                newFName = "resize_" + newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";

                            }
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), newName);
                            file.SaveAs(newName);


                            var taskimg = new TaskImage
                            {
                                ProTaskId = proId,
                                TaskUpdationID = TaskUpdId,

                                FileName = newFName,//Path.GetFileName(file.FileName),
                                Status = FStatus,
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                TaskRemarkId = TaskRemarkId,
                                CreatedBy = UserId,
                            };
                            db.TaskImages.Add(taskimg);
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
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                    lgimg.Save(resizeName);
                                }

                            }
                        }
                    }
                }
                var count = 0;
                if (tremark.TaskDocuments != null)
                {
                    foreach (var arr in tremark.TaskDocuments)
                    {
                        IFormFile filetask = Request.Form.Files["TaskDocuments[" + count + "].FileName"];
                        if (filetask.FileName != null && filetask.FileName != "")
                        {
                            var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(filetask.FileName);
                            TaskDocument tsd = new TaskDocument()
                            {
                                DocumentTypeID = arr.DocumentTypeID,
                                FileName = fileNames,
                                Notes = arr.Notes,
                                ProtaskID = tremark.TaskId,
                                CreatedDate = DateTime.Now
                            };
                            db.TaskDocuments.Add(tsd);
                            db.SaveChanges();
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/protaskdocuments/");

                            filetask.SaveAs(Path.Combine(uploadUrl, fileNames));

                        }
                        count++;
                    }
                }
                var statusname = db.TaskStatus.Find(tremark.TaskStatusID).StatusName;
                ProTask task = db.ProTasks.Find(proId);
                string Type = "/proTask/Details/" + proId;
                string note =  "Task Notification <br> Task Status : " + statusname + "<br> Task name : " + task.TaskCode + " - " + task.TaskName;
                var exist = db.Reminders.Any(o => o.Note == note && o.Type == Type);
                var emps = db.Employees.Where(o => o.appaccessonly == true).Select(o => o.EmployeeId).ToList().ToArray();

                if (!exist && tremark.TaskStatusID != null)
                {
                   
                    task.TaskStatus = tremark.TaskStatusID;
                      




                    if (statusname == "WORK PENDING"|| statusname == "MCC COMPLETED" || statusname == "MCC PENDING" || statusname == "WORK COMPLETED" || statusname == "WORK POSTPONED" || statusname == "CNR")
                    {
                        Reminder reminds = new Reminder();
                        reminds.Reference = proId;
                        reminds.Note = "Task Notification <br> Task Status : " + statusname + "<br> Task name : " + task.TaskCode + " - " + task.TaskName;

                        var rDate = System.DateTime.Now.Date;
                        //seleted date added,for fullcalender



                        reminds.RDate = System.DateTime.Now;
                        reminds.Type = "/proTask/Details/" + proId;
                        reminds.RStatus = "Close";
                        reminds.RequestBy = UserId;

                        reminds.CreatedBy = UserId;
                        reminds.Status = Status.active;
                        reminds.CreatedDate = System.DateTime.Now;
                        db.Reminders.Add(reminds);
                        db.SaveChanges();
                        long Id = reminds.ReminderId;
                        foreach (var arr in emps)
                        {
                   

                           
                            if (1==1)
                            {
                                ReminderAssigned remAs = new ReminderAssigned();
                               
                                    remAs.ReminderId = Id;
                                    remAs.EntryId = proId;
                                    remAs.Type = "tasknotification";
                                    remAs.EmployeeId = arr;
                                    db.ReminderAssigneds.Add(remAs);
                                    db.SaveChanges();
                                
                            }            
                        }
                    }
                        db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();

                    var pflow = db.ProcessFlows.Where(a => a.TaskStatus == tremark.TaskStatusID).FirstOrDefault();
                    List<long> astypes = null;
                    if (pflow != null)
                    {
                        astypes = db.ProcessFlowAssignTypes.Where(a => a.ProcessFlowId == pflow.ProcessFlowId).Select(a => a.TeamId).ToList();
                    }

                    if (pflow != null)
                    {
                        if (pflow.RemoveUpdateUser == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();
                            //                    a.ProTaskId == proId && a.Status == "Assigned" && a.chkStatus == 0
                            //                        a.EmployeeId





                            if (UserEmp != null)
                            {
                                TaskAssigned tskass = new TaskAssigned();
                                var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == task.ProTaskId && a.Status == "Assigned" && a.EmployeeId == UserEmp && a.chkStatus == Status.active).ToList();
                                if (tskasgn != null)
                                {
                                    foreach (var arr in tskasgn)
                                    {
                                        TaskAssigned tskassr = db.TaskAssigneds.Find(arr.TaskAssignedId);
                                        tskassr.chkStatus = Status.inactive;
                                        db.Entry(tskassr).State = EntityState.Modified;
                                        db.SaveChanges();

                                        tskass.ProTaskId = task.ProTaskId;
                                        tskass.EmployeeId = arr.EmployeeId;
                                        tskass.Status = "Removed";
                                        tskass.AssignBy = UserId;
                                        tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100); ;
                                        db.TaskAssigneds.Add(tskass);
                                        db.SaveChanges();
                                    }
                                }
                            }

                            //08-02-2023
                            if (1==1)
                            {
                                TaskAssigned tskass = new TaskAssigned();
                                var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == task.ProTaskId && a.Status == "Assigned" && a.AssignBy == UserId && a.chkStatus == Status.active).ToList();
                                var createtask = User.IsInRole("Create ProTask");
                                var edittask= User.IsInRole("Edit ProTask");
                                if (tskasgn != null&&createtask==false&& edittask==false)
                                {
                                    foreach (var arr in tskasgn)
                                    {

                                        TaskAssigned tskassr = db.TaskAssigneds.Find(arr.TaskAssignedId);
                                        var outdoortech = db.TeamMembers.Any(o => o.EmployeeId == tskassr.EmployeeId && o.TeamId == 4);
                                        if (outdoortech == true)
                                        {
                                            tskassr.chkStatus = Status.inactive;
                                            db.Entry(tskassr).State = EntityState.Modified;
                                            db.SaveChanges();

                                            tskass.ProTaskId = task.ProTaskId;
                                            tskass.EmployeeId = arr.EmployeeId;
                                            tskass.Status = "Removed";
                                            tskass.AssignBy = UserId;
                                            tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100); ;
                                            db.TaskAssigneds.Add(tskass);
                                            db.SaveChanges();
                                        }
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
                            var tskteam = (from a in db.Teams
                                           where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || a.TeamLead != UserEmp)
                                           //where a.TeamLead == UserEmp
                                           select new
                                           {
                                               EmployeeId = a.TeamLead
                                           }).ToArray();

                            ////team mebers 
                            var tskmem = (from a in db.Teams
                                          join b in db.TeamMembers on a.TeamId equals b.TeamId
                                          join c in db.TeamTaskStatus on b.TeamId equals c.TeamId
                                          where fulldata.Contains(a.TeamId) && (c.TaskStatusId == tremark.TaskStatusID) && (pflow.RemoveUpdateUser == false || b.EmployeeId != UserEmp)
                                          select new
                                          {
                                              b.EmployeeId
                                          }).ToArray();

                            var emp = tskteam.Union(tskmem).Select(a => a.EmployeeId).ToList();
                            var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == task.ProTaskId && emp.Contains(a.EmployeeId) && a.Status == "Assigned" && a.chkStatus == Status.active).ToList();

                            TaskAssigned tskass = new TaskAssigned();
                            if (tskasgn != null)
                            {
                                foreach (var arr in tskasgn)
                                {
                                    TaskAssigned tskassr = db.TaskAssigneds.Find(arr.TaskAssignedId);
                                    tskassr.chkStatus = Status.inactive;
                                    db.Entry(tskassr).State = EntityState.Modified;
                                    db.SaveChanges();

                                    tskass.ProTaskId = task.ProTaskId;
                                    tskass.EmployeeId = arr.EmployeeId;
                                    tskass.Status = "Removed";
                                    tskass.AssignBy = UserId;
                                    tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100); ;
                                    db.TaskAssigneds.Add(tskass);
                                    db.SaveChanges();
                                }
                            }
                        
                        
                        
                        }

                        // process flow members assigned
                        var chkassgn = db.TaskAssigneds.Where(a => a.ProTaskId == task.ProTaskId && a.Status == "Assigned" && a.chkStatus == Status.active).Select(a => a.EmployeeId).ToList();

                        var pfmembers = db.ProcessFlowAssignUsers.Where(a => a.ProcessFlowId == pflow.ProcessFlowId).Select(a => a.EmployeeId).ToList();
                        var activemeber = (from a in pfmembers
                                           join b in db.Employees on a equals b.EmployeeId
                                           join c in db.Users on b.UserId equals c.Id
                                         where c.Status == 1
                                           select a).ToList();
                        TaskAssigned tskses = new TaskAssigned();
                        foreach (var arr in activemeber)
                        {
                            if (!chkassgn.Contains(arr))
                            {
                                tskses.ProTaskId = task.ProTaskId;
                                tskses.EmployeeId = arr;
                                tskses.Status = "Assigned";
                                tskses.AssignBy = UserId;
                                tskses.chkStatus = Status.active;
                                tskses.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100);
                                db.TaskAssigneds.Add(tskses);
                                com.remideradd(rootdomain+"proTask/mytask", arr, UserId, "Task Assined",task.ProTaskId);

                                db.SaveChanges();
                            }
                        }


                        //    //if (chktskasgn != null)
                        //    //foreach (var arr in pfmembers)



                        //                       where a.TeamLead == UserEmp
                        //                           b.EmployeeId


                        //        //user remove in task assigned



                        //            //team remove in task assigned



                        //        //if (chktskasgn != null)
                    }

                    db.Entry(task).State = EntityState.Modified;
                    db.SaveChanges();
                }


        
               
                var rem = (from a in db.ProTasks
                           join b in db.TaskAssigneds on a.ProTaskId equals b.ProTaskId
                           join c in db.TaskStatus on a.TaskStatus equals c.TaskStatusId
                           where (a.Ref1 == "ASSIGNED" || a.Ref1 == "APPOINTED") && a.OpenClose == 0
                           && EF.Functions.DateDiffHour(a.logtime, a.StartTime) < -12


                             && b.Status == "Assigned" && b.chkStatus == Status.active

                           select new
                           {
                               b.EmployeeId,
                               a.ProTaskId,
                               c.StatusName,
                               taskname = a.TaskCode + "-" + a.TaskName,
                               a.Ref1
                           }).Distinct();

                if (rem.Count() > 0)
                {
                    var pids = rem.Select(o => new
                    {
                        o.ProTaskId,
                        o.StatusName,
                        o.taskname,
                        o.Ref1

                    }).Distinct().ToList();
                    foreach (var pid in pids)
                    {
                        string tasknote = "12 Hours Task Still " + pid.Ref1 + " <br> Task Status : " + pid.StatusName + "<br> Task name : " + pid.taskname;
                        var remexist = db.Reminders.Any(o => o.Note == tasknote && o.Reference == pid.ProTaskId);
                        if (!remexist)
                        {
                            Reminder reminds = new Reminder();
                            reminds.Reference = pid.ProTaskId;
                            reminds.Note = tasknote;// "Task Still " +pid.Ref1+" <br> Task Status : " + pid.StatusName + "<br> Task name : " + pid.taskname;

                            var rDate = System.DateTime.Now.Date;
                            //seleted date added,for fullcalender



                            reminds.RDate = System.DateTime.Now;
                            reminds.Type = "/proTask/Details/" + pid.ProTaskId;
                            reminds.RStatus = "Close";
                            reminds.RequestBy = UserId;

                            reminds.CreatedBy = UserId;
                            reminds.Status = Status.active;
                            reminds.CreatedDate = System.DateTime.Now;
                            db.Reminders.Add(reminds);
                            db.SaveChanges();
                            long Id = reminds.ReminderId;
                            var asseimp = rem.Where(o => o.ProTaskId == pid.ProTaskId).Select(o => o.EmployeeId).Distinct().ToList().ToArray();
                            asseimp = asseimp.Concat(emps).ToArray();
                            var myemps = asseimp.Distinct().ToList().ToArray();
                            foreach (var arr in myemps)
                            {

                                var exists = db.ReminderAssigneds.Any(o => o.EntryId == pid.ProTaskId && o.Type == "TaskStillpending" && o.EmployeeId == arr);



                                if (!exists)
                                {
                                    ReminderAssigned remAs = new ReminderAssigned();

                                    remAs.ReminderId = Id;
                                    remAs.EntryId = pid.ProTaskId;
                                    remAs.Type = "TaskStillpending";
                                    remAs.EmployeeId = arr;
                                    db.ReminderAssigneds.Add(remAs);
                                    db.SaveChanges();

                                }
                            }
                        }

                    }
                }

                msg = "Remark added successfully.";
                stat = true;
                com.updateprotaskdate(proId);
                com.addlog(LogTypes.Created, UserId, "ProTasks", "TaskRemarks", findip(), proId, "Remark Added Successfully");
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        //[QkAuthorize(Roles = "Dev,Create LeadStatus")]

        //                //if(q.Status==Status.active && Convert.ToInt64(q.TypeValue)== tremark.TaskStatusID)
        //                // if(empid.Count()>0)
        //                    LeadTaskUpdation TaskUps2 = new LeadTaskUpdation
        //                        TaskId = (long)lead,
        //                        CreatedBy = UserId,
        //                        CreatedDate = today,
        //                        Remarks = (tremark.Remark == null) ? newtskstat : tremark.Remark,
        //                        leadstatus = movelead.LeadStatus,


        //                        Assigned.Add(new AssignedTo()
        //                            CustomerID = (long)lead,
        //                            EmployeeId = arr.EmployeeId,
        //                            Status = "Assigned",
        //                            AssignBy = UserId,
        //                            ChkStatus = (int)Status.active,
        //                            approve = false,









        //                        CreatedDate = today,
        //                        TaskUpdationID = TaskUpdId2,
        //                        Remark = (tremark.Remark == null) ? " " : tremark.Remark,
        //                        AddedUser = UserId,
        //                        TaskId = tremark.TaskId



        //                    // fileupload
        //                        string path = LegacyWeb.MapPath("~/uploads/leadtaskdocuments/");






        //                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), thumbName);

        //                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), resizeName);

        //                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), newName);


        //                                    TaskId = (long)lead,
        //                                    TaskUpdationID = TaskUpdId2,

        //                                    Status = FStatus,
        //                                    TaskRemarkId = rmid,
        //                                    CreatedBy = UserId,



        //                                        //portrait image  
        //                                        //landscape image  

        //                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), resizeName);
        //                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), resizeName);























        //                ProTaskUpdation TaskUps = new ProTaskUpdation
        //                    ProTaskId = proId,
        //                    CreatedBy = UserId,
        //                    CreatedDate = today,
        //                    Remarks = oldtskstat + " Change To " + newtskstat,














        ////------------------------





        //                    CreatedDate = today,
        //                    TaskStatusID = tremark.TaskStatusID,
        //                    Remark = tremark.Remark+lat+","+log,
        //                    AddedUser = UserId,
        //                    TaskId = tremark.TaskId,
        //                    TaskUpdationID = TaskUpdId,






        //                // fileupload
        //                    string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/");






        //                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), thumbName);

        //                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);

        //                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), newName);


        //                                ProTaskId = proId,
        //                                TaskUpdationID = TaskUpdId,

        //                                Status = FStatus,
        //                                TaskRemarkId = TaskRemarkId,
        //                                CreatedBy = UserId,


        //                                    //portrait image  
        //                                    //landscape image  

        //                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
        //                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
















        //                            ////team lead as user
        //                                           where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || a.TeamLead != UserEmp)
        //                                           //where a.TeamLead == UserEmp
        //                                               EmployeeId = a.TeamLead
        //                            ////team mebers 
        //                                          where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || b.EmployeeId != UserEmp)
        //                                              b.EmployeeId








        //                        // process flow members assigned




        //                        //if (pflow.RemoveUpdateUser == true)
        //                        //    if (UserEmp != null)
        //                        //        if (tskx != null)
        //                        //if (astypes != null)
        //                        //    foreach (var arr in astypes)
        //                        //if (pflow.RemoveUpdateUserTeams == true)
        //                        //    //if (chktskasgn != null)
        //                        //    //foreach (var arr in pfmembers)

        //                        //    if (pfmembers != null)


        //                        //        var tskteam = (from a in db.Teams
        //                        //                       join b in db.TeamMembers on a.TeamId equals b.TeamId into tea
        //                        //                       from b in tea.DefaultIfEmpty()
        //                        //                       where a.TeamLead == UserEmp
        //                        //                       select new
        //                        //                           b.EmployeeId


        //                        //        //user remove in task assigned
        //                        //        foreach (var arr in tskasgn)
        //                        //            if (!newusers.Contains(arr))


        //                        //            else

        //                        //        if (tskteam.Count() > 0)
        //                        //            //team remove in task assigned
        //                        //            foreach (var arr in tskteam.Select(a => a.EmployeeId).ToList())
        //                        //                if (!newusers.Contains(arr))


        //                        //                else

        //                        //        //if (chktskasgn != null)
        //                        //foreach (var arr in pfmembers)



        //                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        //                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        public string GetStatusName(long? Id)
        {
            string name = db.TaskStatus.Where(a => a.TaskStatusId == Id).Select(a => a.StatusName).FirstOrDefault();
            return name;
        }

        [HttpGet]
        public JsonResult GetAllMembers(long taskid)
        {
            var teamss = (from z in db.TaskAssigneds
                          join y in db.Employees on z.EmployeeId equals y.EmployeeId
                          where z.ProTaskId == taskid && z.Status == "Assigned" && z.chkStatus == Status.active
                          select new
                          {
                              emp = z.EmployeeId,
                              //lead = a.TeamLead,
                          }).ToList();
            return Json(teamss);
        }

        public ActionResult GetCustDetailTask(long Cuss)
        {
            var v = (from a in db.ProTasks.Where(o=>o.Branch==1)
                     join c in db.Customers on a.CustomerID equals c.CustomerID into rec
                     from c in rec.DefaultIfEmpty()
                     join d in db.Projects on a.ProjectId equals d.ProjectId into proj
                     from d in proj.DefaultIfEmpty()
                     where c.CustomerID == Cuss && c.Type == CRMCustomerType.Customer
                     select new
                     {
                         ProTaskId = a.ProTaskId,
                         TaskName = a.TaskCode + " " + a.TaskName,
                         ProjectName =(d.ProCode==null)?"":d.ProCode + " " + d.ProjectName,
                         ProjectId = (d.ProjectId==null)?0:d.ProjectId,
                         a.logtime,
                     }).OrderByDescending(b => b.logtime);

            var data = v.ToList();

            return Json(new { data = data });
        }

        public ActionResult ViewChecklist(long id)
        {
            RemarkCheckViewModel tremark = (from a in db.TaskRemarks
                                            join b in db.Users on a.AddedUser equals b.Id into emp
                                            from b in emp.DefaultIfEmpty()
                                            join c in db.TaskStatus on a.TaskStatusID equals c.TaskStatusId into stat
                                            from c in stat.DefaultIfEmpty()
                                            where a.TaskRemarkId == id
                                            orderby a.CreatedDate descending
                                            select new RemarkCheckViewModel
                                            {
                                                Id = a.TaskRemarkId,
                                                Createddate = a.CreatedDate,
                                                Employee = b.UserName,
                                                Remark = a.Remark,
                                                Status = c.StatusName,
                                                StatusId = c.TaskStatusId,
                                                ProTaskId = a.TaskId,
                                                RImages = (from z in db.TaskRemarks
                                                           join y in db.TaskImages on z.TaskRemarkId equals y.TaskRemarkId into img
                                                           from y in img.DefaultIfEmpty()
                                                           where y.TaskUpdationID == a.TaskUpdationID
                                                           select new ImageRemarkViewModel
                                                           {
                                                               FileName = (y.FileName != null) ? y.FileName : "",
                                                               TaskUpdationID = y.TaskUpdationID,
                                                           }).ToList(),
                                            }).FirstOrDefault();
            return PartialView(tremark);
        }

        [HttpGet]
        public JsonResult GetRemarkCheckItems(long? CheckID)
        {
            var ConD = (from a in db.RemarkChecklists
                        join c in db.ChecklistItems on a.Checklistitemid equals c.Id into chk
                        from c in chk.DefaultIfEmpty()
                        where a.Remark == CheckID
                        select new
                        {
                            Id = a.Id,
                            Name = c.ListName,
                            Chck = a.Check,
                            Note = a.Note

                        }).ToList();
            return Json(ConD);
        }

        [HttpPost]
        public ActionResult UpdateChecklist(RemarkCheckViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (vmodel.Id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var pro = db.RemarkChecklists.Select(x => x.Remark == vmodel.Id).Any();

            if (!pro)
            {
                return NotFound();
            }
            if (vmodel.CheckItems.Count > 0)
            {
                foreach (var arr in vmodel.CheckItems)
                {
                    RemarkChecklist remlist = db.RemarkChecklists.Find(arr.Id);
                    remlist.Note = arr.Note;
                    remlist.Check = (arr.Chck == true) ? true : false;
                    db.Entry(remlist).State = EntityState.Modified;
                    db.SaveChanges();
                }
                msg = "Remark Checklist details Updated Successfully.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        public JsonResult SearchAllMobileAndPhone(string q, string x)
        {
            List<SelectUserFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                serialisedJson = (from a in db.TaskMobiles
                                  where a.MobileNo.Contains(q)
                                  select new SelectUserFormat
                                  {
                                      text = a.MobileNo, //each json object will have 
                                      id = a.MobileNo,

                                  }).ToList();
            }
            else
            {
                serialisedJson = (from a in db.TaskMobiles
                                  select new SelectUserFormat
                                  {
                                      text = a.MobileNo, //each json object will have 
                                      id = a.MobileNo,

                                  }).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectUserFormat() { id = stt, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson.Take(10).ToList());
        }
        [HttpGet]
        public JsonResult GetMobile(long CnId)
        {
            var ConD = (from a in db.TaskMobiles
                        where a.ProTaskId == CnId
                        select new
                        {
                            Mob = a.MobileNo,
                            Name = a.Name
                        }).Distinct().ToList();
            return Json(ConD);
        }
        public ActionResult AddTaskImagenewandold(long id)
        {
            ViewProTaskViewModel tsk = new ViewProTaskViewModel();
            tsk.ProTaskId = id;

            tsk.TaskDocuments = (from b in db.TaskImages
                                 join c in db.ProTasks.Where(o=>o.Branch==1) on b.ProTaskId equals c.ProTaskId
                                 where c.ProTaskId == id
                                 orderby b.CreatedDate descending
                                 select new TaskDocumentViewModel
                                 {
                                     ProTaskId = c.ProTaskId,
                                     TaskDocumentId = b.TaskImageId,
                                     FileName = b.FileName,
                                     description = b.description,
                                     newdescription = b.newdescription,
                                 }).ToList();

            return PartialView(tsk);
        }
        [HttpPost]
        public ActionResult AddTaskImagenewandoldpost()
        {
            string description = "";
            long? taskdocumentid = null;
            long id;
            string type;
            string location;
            type = Request.Form["type"];
            description = Request.Form["description"];
            if (!String.IsNullOrEmpty(Request.Form["taskdocumentid"]))
                taskdocumentid = Convert.ToInt64(Request.Form["taskdocumentid"]);

            id = Convert.ToInt64(Request.Form["protaskid"]);
            if (type == "old")
                location = "protaskdocuments";
            else
            {
                location = "protaskdocumentsNew";
                if (db.TaskImages.Find(taskdocumentid) == null)
                {
                    Danger("Old Image or description not exists", true);
                    return RedirectToAction("createimageupload", "ProTask");
                }
            }



            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);
            var TaskUp = new ProTaskUpdation
            {
                ProTaskId = id,
                //Status = TKUpdateStatus.Created,
                CreatedBy = UserId,
                CreatedDate = today,
                //TaskTeamId = teamId
            };
            db.ProTaskUpdations.Add(TaskUp);
            db.SaveChanges();
            Int64 TaskUpdId = TaskUp.TaskUpdationID;
            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/" + location + "/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i];
                    if (file.Length > 0)
                    {
                        var taskimgg = db.TaskImages.Find(taskdocumentid);
                        if (taskimgg != null)
                        {

                            if (type == "old")
                                taskimgg.description = description;
                            else
                            {
                                if (String.IsNullOrEmpty(taskimgg.description))
                                {
                                    Danger("First Add Old Description");
                                    return RedirectToAction("createimageupload", "ProTask");
                                }
                                taskimgg.newdescription = description;
                            }
                            taskimgg.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            taskimgg.CreatedBy = UserId;
                            db.Entry(taskimgg).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            var taskimg2 = new TaskImage
                            {
                                ProTaskId = id,
                                TaskUpdationID = TaskUpdId,

                                FileName = "",//Path.GetFileName(file.FileName),
                                Status = Status.inactive,
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                CreatedBy = UserId,
                                description = description,
                            };
                            db.TaskImages.Add(taskimg2);
                            db.SaveChanges();
                            taskdocumentid = taskimg2.TaskImageId;
                        }


                        var fileName = Path.GetFileName(file.FileName);

                        String extension = Path.GetExtension(fileName);

                        extension = ".jpg";
                        String newName = taskdocumentid + extension;
                        string newFName = taskdocumentid + extension;
                        var FStatus = Status.active;
                        var thumbName = "";
                        var resizeName = "";
                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                        {
                            thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/" + location + "/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/" + location + "/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";

                        }
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/" + location + "/"), newName);
                        if (System.IO.File.Exists(newName))
                            System.IO.File.Delete(newName);
                        file.SaveAs(newName);





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
                                System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1000, 1000));
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/" + location + "/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/" + location + "/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                    else
                    {
                        var taskimgg = db.TaskImages.Find(taskdocumentid);
                        if (taskimgg != null)
                        {
                            if (type == "old")
                                taskimgg.description = description;
                            else
                                taskimgg.newdescription = description;
                            taskimgg.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            taskimgg.CreatedBy = UserId;
                            db.Entry(taskimgg).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            var taskimg2 = new TaskImage
                            {
                                ProTaskId = id,
                                TaskUpdationID = TaskUpdId,

                                FileName = "",//Path.GetFileName(file.FileName),
                                Status = Status.inactive,
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                CreatedBy = UserId,
                                description = description,
                            };
                            db.TaskImages.Add(taskimg2);
                            db.SaveChanges();
                        }



                    }
                }
            }


            return RedirectToAction("createimageupload", "ProTask");

        }
        public ActionResult AddTaskImage(long id)
        {
            ViewProTaskViewModel tsk = new ViewProTaskViewModel();
            tsk.ProTaskId = id;

            tsk.TaskDocuments = (from b in db.TaskImages
                                 join c in db.ProTasks.Where(o=>o.Branch==1) on b.ProTaskId equals c.ProTaskId
                                 where c.ProTaskId == id
                                 select new TaskDocumentViewModel
                                 {
                                     ProTaskId = c.ProTaskId,
                                     TaskDocumentId = b.TaskImageId,
                                     FileName = b.FileName,
                                 }).ToList();

            return PartialView(tsk);
        }

        public ActionResult DeleteImage(long id)
        {
            TaskDocumentViewModel vmodel = new TaskDocumentViewModel();
            vmodel.TaskDocumentId = id;
            return PartialView(vmodel);
        }
        public ActionResult DeleteDocument(long id)
        {
            TaskDocument vmodel = new TaskDocument();
            vmodel.TaskDocumentId = id;
            return PartialView(vmodel);
        }
        public ActionResult DeleteTaskDocument(TaskDocument vmodel)
        {
            bool stat = false;
            string msg;
            var tskImg = db.TaskDocuments.Where(a => a.TaskDocumentId == vmodel.TaskDocumentId).FirstOrDefault();
            if (tskImg != null)
            {

                string fullPath = LegacyWeb.MapPath("~/uploads/protaskdocuments/" + tskImg.FileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                

              
                db.TaskDocuments.Remove(tskImg);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();

                com.addlog(LogTypes.Deleted, UserId, "ProTask", "TaskDocument", findip(), tskImg.TaskDocumentId, "Task Document Deleted Successfully");

                stat = true;
                msg = "Successfully deleted Task Document.";
            }
            else
            {
                stat = false;
                msg = "Task Document Not Found..!!";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult DeleteTaskImage(TaskDocumentViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var tskImg = db.TaskImages.Where(a => a.TaskImageId == vmodel.TaskDocumentId).FirstOrDefault();
            if (tskImg != null)
            {

                string fullPath = LegacyWeb.MapPath("~/uploads/protaskdocuments/" + tskImg.FileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                string fullPaththumb = LegacyWeb.MapPath("~/uploads/protaskdocuments/" + "thumb_" + tskImg.FileName);
                if (System.IO.File.Exists(fullPaththumb))
                {
                    System.IO.File.Delete(fullPaththumb);
                }

                if (tskImg.FileName.Contains("resize_"))
                {
                    var filez = tskImg.FileName.Split('_');
                    if (filez.Length > 1)
                    {
                        var extnf = filez[1];
                        string fullPathz = LegacyWeb.MapPath("~/uploads/protaskdocuments/" + "thumb_" + extnf);
                        if (System.IO.File.Exists(fullPathz))
                        {
                            System.IO.File.Delete(fullPathz);
                        }
                    }
                }
                db.TaskImages.Remove(tskImg);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();

                com.addlog(LogTypes.Deleted, UserId, "ProTask", "TaskImages", findip(), tskImg.TaskImageId, "Task Image Deleted Successfully");

                stat = true;
                msg = "Successfully deleted Task Image.";
            }
            else
            {
                stat = false;
                msg = "Task Image Not Found..!!";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult Location(string q, string x)
        {
            List<SelectUserFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProTasks.Where(p => p.Location.ToLower().Contains(q.ToLower()) || p.Location.Contains(q))
                                  .Select(b => new SelectUserFormat
                                  {
                                      text = b.Location,
                                      id = b.Location
                                  }).Distinct().OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ProTasks.Select(b => new SelectUserFormat
                {
                    text = b.Location,
                    id = b.Location
                }).Distinct().OrderBy(b => b.text).ToList();

            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectUserFormat() { id = "0", text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        //        serialisedJson = (from a in db.ProTasks.Where(o=>o.Branch==1)
        //                          where a.Ref1.Contains(q) || a.Ref1.ToLower().Contains(q.ToLower())
        //                              text = a.Ref1, //each json object will have 
        //                              id = a.Ref1,

        //        serialisedJson = (from a in db.ProTasks.Where(o=>o.Branch==1)
        //                              text = a.Ref1, //each json object will have 
        //                              id = a.Ref1,

        //    }//

        public ActionResult DocView(long Id)
        {
            var tskImg = db.TaskImages.Where(a => a.TaskImageId == Id).FirstOrDefault();
            TaskDocumentViewModel vmodel = new TaskDocumentViewModel();
            vmodel.FileName = tskImg.FileName;
            return PartialView(vmodel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public JsonResult SearchVehicleType(string q, string x)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.VehicleTypes.Where(p => p.Type.ToLower().Contains(q.ToLower()) || p.Type.Contains(q))
                                  .Select(b => new SelectFormat3
                                  {
                                      text = b.Type, //each json object will have 
                                      id = b.VTypeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.VehicleTypes.Select(b => new SelectFormat3
                {
                    text = b.Type, //each json object will have 
                    id = b.VTypeId
                }).OrderBy(b => b.text).ToList();

            }//          

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult SearchManufacturer(string q, string x, long VTid)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.VehicleManufacturers
                                  where (VTid != 0 && a.VTyId == VTid) &&
                                        (q == null || a.Manufacturer.ToLower().Contains(q.ToLower()) || a.Manufacturer.Contains(q))
                                  select new SelectFormat3
                                  {
                                      text = a.Manufacturer,
                                      id = a.MId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.VehicleManufacturers
                                  where (a.VTyId == VTid)
                                  select new SelectFormat3
                                  {
                                      text = a.Manufacturer,
                                      id = a.MId
                                  }).OrderBy(b => b.text).ToList();

            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult SearchModel(string q, string x, long VMaID, long Vt)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.VehicleModels
                                  where (VMaID != 0 && a.MaId == VMaID && a.VTId == Vt) &&
                                        (q == null || a.Model.ToLower().Contains(q.ToLower()) || a.Model.Contains(q))
                                  select new SelectFormat3
                                  {
                                      text = a.Model,
                                      id = a.ModelId
                                  }).OrderBy(b => b.text).ToList();

            }
            else
            {
                serialisedJson = (from a in db.VehicleModels
                                  where (a.MaId == VMaID && a.VTId == Vt)
                                  select new SelectFormat3
                                  {
                                      text = a.Model,
                                      id = a.ModelId
                                  }).OrderBy(b => b.text).ToList();

            }//          

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult getVehiManufacturer(string q, string x)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.VehicleManufacturers.Where(p => p.Manufacturer.ToLower().Contains(q.ToLower()) || p.Manufacturer.Contains(q))
                                  .Select(b => new SelectFormat3
                                  {
                                      text = b.Manufacturer, //each json object will have 
                                      id = b.MId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.VehicleManufacturers.Select(b => new SelectFormat3
                {
                    text = b.Manufacturer, //each json object will have 
                    id = b.MId
                }).OrderBy(b => b.text).ToList();

            }//          

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult getVehiModel(string q, string x)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.VehicleModels.Where(p => p.Model.ToLower().Contains(q.ToLower()) || p.Model.Contains(q))
                                  .Select(b => new SelectFormat3
                                  {
                                      text = b.Model, //each json object will have 
                                      id = b.ModelId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.VehicleModels.Select(b => new SelectFormat3
                {
                    text = b.Model, //each json object will have 
                    id = b.ModelId
                }).OrderBy(b => b.text).ToList();

            }//          

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

    }
}
