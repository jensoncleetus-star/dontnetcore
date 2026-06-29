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
using System.Net;
using System.IO;
namespace QuickSoft.Controllers
{
    public class ReminderController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ReminderController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        public ActionResult Index()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult addnotification(ReminderViewModel re)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);

            var created = "";
            Reminder reminds = new Reminder();
            reminds.Reference = re.Reference;
            reminds.Note = re.Note;

            var rDate = System.DateTime.Now.Date;
            //seleted date added,for fullcalender
            TimeSpan time = (re.RTime).TimeOfDay;
            DateTime date = rDate;
            if (re.RDate != "")
            {
                rDate = DateTime.Parse(re.RDate.ToString(), new CultureInfo("en-GB"));
                date = rDate + time;
            }

            reminds.RDate = date;
            reminds.Type = re.Type;
            reminds.RStatus = "Open";
            reminds.RequestBy = UserId;

            reminds.CreatedBy = UserId;
            reminds.Status = Status.active;
            reminds.CreatedDate = today;
            db.Reminders.Add(reminds);
            db.SaveChanges();
            Id = reminds.ReminderId;


            //Approved By
            var Asby = re.AssignedTo;
            if (Asby != null && Asby.Length > 0)
            {
                ReminderAssigned remAs = new ReminderAssigned();
                foreach (var emp in Asby)
                {
                    remAs.ReminderId = Id;
                    remAs.EntryId = re.Reference;
                    remAs.Type = re.Type;
                    remAs.EmployeeId = emp;
                    db.ReminderAssigneds.Add(remAs);
                    db.SaveChanges();
                }
            }

            msg = "Reminder added successfully.";
            stat = true;
            com.addlog(LogTypes.Created, UserId, "Reminder", "Reminders", findip(), Id, "Reminder Added Successfully");
            var assTo = db.Employees.Where(a => a.UserStatus == true)
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.AssignedTo = QkSelect.List(assTo, "ID", "Name");

            var remind = new ReminderViewModel
            {
                Reference = 1,
                Type = "app"
            };

            return View(remind);
        }
        public ActionResult addnotification()
        {
            var assTo = db.Employees.Where(a => a.UserStatus == true)
               .Select(s => new
               {
                   ID = s.EmployeeId,
                   Name = s.FirstName + " " + s.LastName
               })
               .ToList();
            ViewBag.AssignedTo = QkSelect.List(assTo, "ID", "Name");

            var remind = new ReminderViewModel
            {
                Reference = 1,
                Type = ""
            };
            return View(remind);
        }
        public ActionResult AddReminder(long id, string type)
        {
            var assTo = db.Employees.Where(a => a.UserStatus == true)
               .Select(s => new
               {
                   ID = s.EmployeeId,
                   Name = s.FirstName + " " + s.LastName
               })
               .ToList();
            ViewBag.AssignedTo = QkSelect.List(assTo, "ID", "Name");

            var remind = new ReminderViewModel
            {
                Reference = id,
                Type = type
            };
            return PartialView(remind);
        }
        [HttpPost]
        public string addlog(string source)
        {
            var user = User.Identity.GetUserId();
            if (user != "1c7fca9e-cefc-4ff9-a0b5-ce2ea33793ca")
            {
                DateTime dt = System.DateTime.Now.AddDays(-2);
                db.LogManagers.RemoveRange(db.LogManagers.Where(o => o.LogTime <= dt && o.LogTable == "monitor"));
                db.SaveChanges();
                com.addlog(LogTypes.Logged, user, "monitor", "monitor", findip(), 0, source);

            }
            return "9";
        }
        [HttpPost]
        public JsonResult AddReminder(ReminderViewModel re)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);

            var created = "";
            if (re.Type == "Task")
            {
                created = db.ProTasks.Where(a => a.ProTaskId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "Sale")
            {
                created = db.SalesEntrys.Where(a => a.SalesEntryId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "Quot")
            {
                created = db.Quotations.Where(a => a.QuotationId == re.Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (re.Type == "SReturn")
            {
                created = db.SalesReturns.Where(a => a.SalesReturnId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "SOrder")
            {
                created = db.SalesOrders.Where(a => a.SalesOrderId == re.Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (re.Type == "ProForma")
            {
                created = db.ProFormas.Where(a => a.ProFormaId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "Purchase")
            {
                created = db.PurchaseEntrys.Where(a => a.PurchaseEntryId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "PReturn")
            {
                created = db.PurchaseReturns.Where(a => a.PurchaseReturnId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "POrder")
            {
                created = db.PurchaseOrders.Where(a => a.PurchaseOrderId == re.Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (re.Type == "PQuot")
            {
                created = db.PurchaseQuotations.Where(a => a.PQuotationId == re.Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (re.Type == "DVNote")
            {
                created = db.Deliverynotes.Where(a => a.DeliverynoteId == re.Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (re.Type == "HReturn")
            {
                created = db.HireReturns.Where(a => a.HireReturnId == re.Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (re.Type == "MReqn")
            {
                created = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == re.Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (re.Type == "MRNote")
            {
                created = db.MaterialReceiveNotes.Where(a => a.MRId == re.Reference).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (re.Type == "JobCard")
            {
                created = db.JobCards.Where(a => a.JobCardId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "PKList")
            {
                created = db.PackingLists.Where(a => a.PackinglistId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }

            if (re.Type == "StkTrans")
            {
                created = db.StockTransfers.Where(a => a.Id == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "StkJnl")
            {
                created = db.StockJournals.Where(a => a.Id == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "Prod")
            {
                created = db.Productions.Where(a => a.ProductionId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (re.Type == "Unass")
            {
                created = db.Unassembles.Where(a => a.UnassembleId == re.Reference).Select(a => a.CreatedBy).FirstOrDefault();
            }

            Reminder reminds = new Reminder();
            reminds.Reference = re.Reference;
            reminds.Note = re.Note;

            var rDate = DateTime.Parse(re.RDate.ToString(), new CultureInfo("en-GB"));
            //seleted date added,for fullcalender
            TimeSpan time = (re.RTime).TimeOfDay;
            DateTime date = rDate + time;


            reminds.RDate = date;
            reminds.Type = re.Type;
            reminds.RStatus = "Open";
            reminds.RequestBy = created;

            reminds.CreatedBy = UserId;
            reminds.Status = Status.active;
            reminds.CreatedDate = today;
            db.Reminders.Add(reminds);
            db.SaveChanges();
            Id = reminds.ReminderId;


            //Approved By
            var Asby = re.AssignedTo;
            if (Asby != null && Asby.Length > 0)
            {
                ReminderAssigned remAs = new ReminderAssigned();
                foreach (var emp in Asby)
                {
                    remAs.ReminderId = Id;
                    remAs.EntryId = re.Reference;
                    remAs.Type = re.Type;
                    remAs.EmployeeId = emp;
                    db.ReminderAssigneds.Add(remAs);
                    db.SaveChanges();
                }
            }

            msg = "Reminder added successfully.";
            stat = true;
            com.addlog(LogTypes.Created, UserId, "Reminder", "Reminders", findip(), Id, "Reminder Added Successfully");
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [HttpPost]
        public ActionResult GetAllReminder(long Id, string type)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var v = (from a in db.Reminders
                     join b in db.Users on a.CreatedBy equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.Users on a.RequestBy equals c.Id into use
                     from c in use.DefaultIfEmpty()
                     where a.Reference == Id && a.Type == type
                     orderby a.CreatedDate descending
                     select new
                     {
                         a.ReminderId,
                         a.CreatedDate,
                         CreatedBy = b.UserName,
                         RequestBy = c.UserName,
                         a.RDate,
                         a.Note,
                         a.RStatus,
                         AssignedTo = (from aa in db.ReminderAssigneds
                                       join bb in db.Employees on aa.EmployeeId equals bb.EmployeeId into emps
                                       from bb in emps.DefaultIfEmpty()
                                       where aa.ReminderId == a.ReminderId && aa.Type == a.Type
                                       select new { FirstName = bb.FirstName, LastName = bb.LastName != null ? bb.LastName : "" }
                                    ).ToList(),
                         CheckUser = (UserId == a.CreatedBy) ? true : false,
                         Date = SqlFunctions.DateName("day", a.RDate) + "-" + SqlFunctions.DateName("month", a.RDate) + "-" + SqlFunctions.DateName("year", a.RDate) + " " + SqlFunctions.DateName("hh", a.RDate) + ":" + SqlFunctions.DateName("mi", a.RDate)
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
        public void snoopthis(long Id, string until = null)
        {
            var userid = User.Identity.GetUserId();

            var empid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();

            // snooze until a user-picked date/time; fall back to +24h if none / invalid
            DateTime snoozeUntil;
            if (string.IsNullOrWhiteSpace(until) ||
                !DateTime.TryParse(until, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out snoozeUntil))
            {
                snoozeUntil = System.DateTime.Now.AddHours(24);
            }

            var reminder = db.ReminderAssigneds.Where(o => o.EmployeeId == empid && o.ReminderId == Id).Select(o => o.ReminderAssignedID).ToList();
            foreach (var remiders in reminder)
            {
                db.Snoozees.RemoveRange(db.Snoozees.Where(o => o.reminderassignedid == remiders));
                db.SaveChanges();
                snooze c = new snooze
                {
                    createddate = snoozeUntil,
                    reminderassignedid = remiders

                };
                db.Snoozees.Add(c);
                db.SaveChanges();
            }
        }
        public void completed(long Id)
        {
            var userid = User.Identity.GetUserId();

            var empid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();


            var reminder = db.ReminderAssigneds.Where(o => o.EmployeeId == empid && o.ReminderId == Id).Select(o => o.ReminderAssignedID).ToList();
            if (1 == 1)
            {

                var fildid = db.Reminders.Where(o => o.ReminderId == Id).Select(o =>
      o.Reference

    ).FirstOrDefault();
                var fil = db.FileDocuments.Find(fildid);
                if (fil.reminderrepeate == 6)
                {
                    fil.ReminderDate = System.DateTime.Now.AddYears(-10);
                    fil.ExpiryDate = fil.ReminderDate;

                }
                else if (fil.reminderrepeate == 0)
                {
                    fil.ReminderDate = System.DateTime.Now.AddDays(1);
                    fil.ExpiryDate = fil.ReminderDate;

                }
                else if (fil.reminderrepeate == 1)
                {
                    DateTime today = DateTime.Today;
                    int daysUntilSaturday = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;

                    // If today is Saturday and you want the *actual* next one (in 7 days)
                    if (daysUntilSaturday == 0) daysUntilSaturday = 7;

                    DateTime nextSaturday = today.AddDays(daysUntilSaturday);
                    fil.ReminderDate = nextSaturday;
                    fil.ExpiryDate = fil.ReminderDate;
                }
                else if (fil.reminderrepeate == 2)
                {
                    DateTime today = DateTime.Today;
                    int daysUntilSaturday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;

                    // If today is Saturday and you want the *actual* next one (in 7 days)
                    if (daysUntilSaturday == 0) daysUntilSaturday = 7;

                    DateTime nextSaturday = today.AddDays(daysUntilSaturday);
                    fil.ReminderDate = nextSaturday;
                    fil.ExpiryDate = fil.ReminderDate;
                }
                else if (fil.reminderrepeate == 3)
                {
                    fil.ReminderDate = System.DateTime.Now.AddDays(7);
                    fil.ExpiryDate = fil.ReminderDate;
                }
                else if (fil.reminderrepeate == 4)
                {
                    fil.ReminderDate = System.DateTime.Now.AddDays(30);
                    fil.ExpiryDate = fil.ReminderDate;
                }
                else if (fil.reminderrepeate == 5)
                {
                    fil.ReminderDate = System.DateTime.Now.AddDays(365);
                    fil.ExpiryDate = fil.ReminderDate;
                }
                db.Entry(fil).State = EntityState.Modified;
                db.SaveChanges();
                var reminderss = db.ReminderAssigneds.Where(o => o.EmployeeId == empid && o.ReminderId == Id).Select(o => o.ReminderAssignedID).ToList();
                foreach (var remidersss in reminder)
                {
                    db.Snoozees.RemoveRange(db.Snoozees.Where(o => o.reminderassignedid == remidersss));
                    db.SaveChanges();
                    snooze c = new snooze
                    {
                        createddate = (DateTime)fil.ReminderDate,
                        reminderassignedid = remidersss

                    };
                    db.Snoozees.Add(c);
                    db.SaveChanges();
                }

            }
        }

        public void leadreassign(long id)
        {
            var remi = db.Reminders.Find(id);
            long leadid = remi.Reference;
            long req = db.Employees.Where(o => o.UserId == remi.RequestBy).Select(o => o.EmployeeId).FirstOrDefault();
            string leadname = db.Customers.Where(o => o.CustomerID == leadid).Select(o => o.CustomerName).FirstOrDefault();
            var UserId = User.Identity.GetUserId();
            if (req != null)
            {
                IList<AssignedTo> Assigned = new List<AssignedTo>();
                IList<AssignedToLog> AssignLog = new List<AssignedToLog>();
                if (1 == 1)
                {
                    var leadasss = db.AssignedTos.Where(o => o.CustomerID == leadid && o.EmployeeId == req && o.Status == "Assigned").ToList();
                    db.AssignedTos.RemoveRange(leadasss);
                    db.SaveChanges();
                    var leadassslog = db.AssignedToLogs.Where(o => o.CustomerID == leadid && o.EmployeeId == req && o.Status == "Assigned").ToList();
                    db.AssignedToLogs.RemoveRange(leadassslog);
                    db.SaveChanges();
                    //Assigned.Add(new AssignedTo()
                    //    CustomerID = leadid,
                    //    EmployeeId = req,
                    //    Status = "Assigned",
                    //    AssignBy = UserId,
                    //    ChkStatus = (int)Status.active


                    //AssignLog.Add(new AssignedToLog()
                    //    CustomerID = leadid,
                    //    EmployeeId = req,
                    //    Status = "Assigned",
                    //    AssignedDate = System.DateTime.Now,
                    //    AddedUser = UserId,

                }
            }
        }
        public void StopNotificationrejectlead(long id)
        {
            leadreassign(id);
            var userid = User.Identity.GetUserId();

            var empid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();


            var reminder = db.ReminderAssigneds.Where(o => o.EmployeeId == empid && o.ReminderId == id);
            db.ReminderAssigneds.RemoveRange(reminder);
            db.SaveChanges();
        }
        public void StopNotification(long Id)
        {
            var userid = User.Identity.GetUserId();

            var empid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();


            var reminder = db.ReminderAssigneds.Where(o => o.EmployeeId == empid && o.ReminderId == Id);
            db.ReminderAssigneds.RemoveRange(reminder);
            db.SaveChanges();
        }
        [HttpPost]
        public ActionResult GetAllReminderall()
        {


            //for searching
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
            DateTime rmdate = System.DateTime.Now.AddDays(30);

            //         where
            //          (EF.Functions.DateDiffDay(a.ExpiryDate, rmdate) >= 0)
            //             Id = a.EmployeeDocumentId,
            //             Date = a.ExpiryDate,
            //             Name = b.FirstName + " " + b.MiddleName + " " + b.LastName,
            //             DocumentType = a.DocumentName,
            //             empid=b.EmployeeId,
            //             doctype="emp",





            ////SORT

            //-------------------------------



            var userid = User.Identity.GetUserId();
            var emid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            bool stat = true;
            DateTime curdate = System.DateTime.Now;
            string msg = "EmployeeAttendance Deleted Successfully.";

            var assignid = (from a in db.Snoozees
                            join b in db.ReminderAssigneds on a.reminderassignedid equals b.ReminderAssignedID

                            where
                            b.EmployeeId == emid
                            && EF.Functions.DateDiffMinute(curdate, a.createddate) > 0
                            select new
                            {
                                a.reminderassignedid,
                                diff = EF.Functions.DateDiffMinute(curdate, a.createddate)
                            }
                           ).ToList();
            var assignids = assignid.Select(o => o.reminderassignedid).ToList().ToArray();
            var v = (from aa in db.Reminders
                     join b in db.Employees on aa.CreatedBy equals b.UserId
                     join c in db.ReminderAssigneds on aa.ReminderId equals c.ReminderId

                     where
                     aa.RStatus == "Close"
                     && aa.Note != ""
                     && (aa.Note.Contains("Leave Reason : ") || aa.Note.Contains("Vehicle Reminder  Service  :") || aa.Note.Contains("12 Hours AMC  Next Followups ") || aa.Note.Contains("HR Note") || aa.Note.Contains("Task Notification") || aa.Note.Contains("12 Hours Task Still ") || aa.Note.Contains("12 Hours Task Still ") || aa.Note.Contains("24 Hours leads not Updation") || aa.Note.Contains("Tenancy Contract Expired") || aa.Note.Contains("AMC Document Expiry  :") || aa.Note.Contains("SOP Notice  :") )
                     && c.EmployeeId == emid &&
                     !assignids.Contains(c.ReminderAssignedID)
                     select new
                     {

                         Note = aa.Note,
                         refs = aa.Reference,
                         //sendby = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                         Id = aa.ReminderId,
                         ur = aa.Type,
                         action = "",
                     



                     });
            DateTime remdates = System.DateTime.Now.AddDays(-2).Date;
            var v2 = (from aa in db.Reminders
                      join b in db.Employees on aa.CreatedBy equals b.UserId
                      join c in db.ReminderAssigneds on aa.ReminderId equals c.ReminderId

                      where

                    aa.Note != "" &&
                    aa.RDate <= remdates
                      && (aa.Note.Contains(" Expired On ")) && c.EmployeeId == emid &&
                      !assignids.Contains(c.ReminderAssignedID)
                      select new
                      {

                          Note = aa.Note,
                          refs = aa.Reference,
                          //  sendby = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                          Id = aa.ReminderId,
                          ur = aa.Type,
                          action = "",


                         
                      });
            var v3 = (from aa in db.Reminders
                    
                      join c in db.ReminderAssigneds on aa.ReminderId equals c.ReminderId
                      join d in db.AssignedTos on aa.Reference equals d.CustomerID 

                      where
                     d.Status == "Assigned" &&
                    aa.Note != "" &&
                  
                       aa.Note.Contains(" Lead Assignd by ") && c.EmployeeId == emid &&
                      !assignids.Contains(c.ReminderAssignedID)
                      select new
                      {

                          Note = aa.Note,
                          refs=aa.Reference,
                          //  sendby = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                          Id = aa.ReminderId,
                          ur = aa.Type,
                          action = "",



                      });
            v = v.Union(v2).Union(v3).OrderByDescending(o => o.Id);
            recordsTotal = v.Count();   // fix "Showing 0 to 0 of 0" — report the real total
            var data = v.Skip(skip).Take(pageSize).ToList().Select(o => new
            {
                o.Id,
                o.Note,
                o.refs,
                //o.sendby,
                url = (o.Note.Contains("Task Notification") || o.Note.Contains("Vehicle Reminder  Service  :") || o.Note.Contains("12 Hours AMC  Next Followups ") || o.Note.Contains("12 Hours Task Still ") || o.Note.Contains("24 Hours leads not Updation") || o.Note.Contains("Tenancy Contract Expired")) ? " <a href='" + o.ur + "' target='_new'>Open Link</a> &nbsp;&nbsp;|&nbsp;&nbsp;   <a href='javascript:void(0)' onclick='snoopthis(" + o.Id + ")' style='color:red' >Snooze</a>" : (o.Note.Contains(" Expired On ")) ? " <a href='" + o.ur + "' target='_new'>Open Link</a>     &nbsp;&nbsp;|&nbsp;&nbsp;<a href='javascript:void(0)' onclick='snoopthis(" + o.Id + ")'      style='color:red' >Snooze</a> &nbsp; &nbsp;| &nbsp; &nbsp;<a href = 'javascript:void(0)' onclick = 'completed(" + o.Id + ")' style = 'color:green'> Completed </a> " : o.Note.Contains("SOP Notice  :") ? " <a href = '" + o.ur + "' target = '_new' > Open Link </a> " : (o.Note.Contains(" Lead Assignd by ")) ? "<a href = 'javascript:void(0)' onclick = 'stopnotify(" + o.Id + ")' style = 'color:green'> Accept </a> &nbsp;&nbsp;<a href='/Leads/Details/"+o.refs+"' target='_new'>Open Link</a>" : " <a href = '" + o.ur + "' target = '_new' > Open Link </a> &nbsp; &nbsp;| &nbsp; &nbsp;<a href = 'javascript:void(0)' onclick = 'stopnotify(" + o.Id + ")' style = 'color:red' ><i class='fa fa-stop' aria-hidden='true'></i></a>",//: "<a href = 'javascript:void(0)' onclick='stopnotify(" + o.Id + ")' style='color:red' ><i class='fa fa-stop' aria-hidden='true'></i></a>",



                //url =(o.Note.Contains(" Expired On ") ||o.Note.Contains("Task Notification") || o.Note.Contains("Vehicle Reminder  Service  :") || o.Note.Contains("12 Hours AMC  Next Followups ") || o.Note.Contains("12 Hours Task Still ") || o.Note.Contains("24 Hours leads not Updation") || o.Note.Contains("Tenancy Contract Expired")) ? " <a href='" + o.ur + "' target='_new'>Open Link</a> &nbsp;&nbsp;|&nbsp;&nbsp;<a href='javascript:void(0)' onclick='snoopthis(" + o.Id + ")' style='color:red' >Snooze</a>" :o.Note.Contains("SOP Notice  :")? "<a href='" + o.ur + "' target='_new'>Open Link</a>" : " <a href='" + o.ur + "' target='_new'>Open Link</a> &nbsp;&nbsp;|&nbsp;&nbsp;<a href='javascript:void(0)' onclick='stopnotify(" + o.Id + ")' style='color:red' ><i class='fa fa-stop' aria-hidden='true'></i></a>" ,//: "<a href='javascript:void(0)' onclick='stopnotify(" + o.Id + ")' style='color:red' ><i class='fa fa-stop' aria-hidden='true'></i></a>",
            }).Distinct().ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        public bool validurl(string ur)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(ur, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }
        public int getremcount()
        {


            var userid = User.Identity.GetUserId();
            var emid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            bool stat = true;
            string msg = "EmployeeAttendance Deleted Successfully.";
            DateTime curdate = System.DateTime.Now;
            var assignids = (from a in db.Snoozees
                             join b in db.ReminderAssigneds on a.reminderassignedid equals b.ReminderAssignedID

                             where EF.Functions.DateDiffMinute(curdate, a.createddate) > 0 &&
                             b.EmployeeId == emid
                             select new
                             {
                                 a.reminderassignedid
                             }
                           ).Select(o => o.reminderassignedid).ToList().ToArray();
            var v = (from aa in db.Reminders
                     join b in db.Employees on aa.CreatedBy equals b.UserId
                     join c in db.ReminderAssigneds on aa.ReminderId equals c.ReminderId
                     where
                     aa.RStatus == "Close"
                     && aa.Note != ""
   && ( aa.Note.Contains("Leave Reason : ") || aa.Note.Contains("12 Hours AMC  Next Followups ") || aa.Note.Contains("HR Note") || aa.Note.Contains("Vehicle Reminder  Service  :") || aa.Note.Contains("Task Notification") || aa.Note.Contains("12 Hours Task Still ") || aa.Note.Contains("24 Hours leads not Updation") || aa.Note.Contains("Tenancy Contract Expired") || aa.Note.Contains("AMC Document Expiry  :") || aa.Note.Contains("SOP Notice  :"))

                     && c.EmployeeId == emid &&
                      !assignids.Contains(c.ReminderAssignedID)
                     select new
                     {

                         Note = aa.Note,
                         //sendby = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                         Id = aa.ReminderId,
                         ur = aa.Type,
                         action = ""


                     });
            DateTime remdates = System.DateTime.Now.AddDays(-2).Date;
            var v2 = (from aa in db.Reminders
                      join b in db.Employees on aa.CreatedBy equals b.UserId
                      join c in db.ReminderAssigneds on aa.ReminderId equals c.ReminderId

                      where

                    aa.Note != "" &&
                    aa.RDate <= remdates
                      && (aa.Note.Contains(" Expired On ")) && c.EmployeeId == emid &&
                      !assignids.Contains(c.ReminderAssignedID)
                      select new
                      {

                          Note = aa.Note,
                          //sendby = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                          Id = aa.ReminderId,
                          ur = aa.Type,
                          action = "",



                      });
            var v3 = (from aa in db.Reminders
                  
                      join c in db.ReminderAssigneds on aa.ReminderId equals c.ReminderId
                      join d in db.AssignedTos on aa.Reference equals d.CustomerID

                      where
                     d.Status == "Assigned" &&
                    aa.Note != "" 
                
                      && aa.Note.Contains(" Lead Assignd by ") && c.EmployeeId == emid &&
                      !assignids.Contains(c.ReminderAssignedID)
                      select new
                      {

                          Note = aa.Note,
                          //  sendby = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                          Id = aa.ReminderId,
                          ur = aa.Type,
                          action = "",



                      });
            v = v.Union(v2).Union(v3).OrderByDescending(o => o.Id);
            if (v.Count() > 0)
                return 1;
            else
                return 0;
        }

        [HttpPost]
        public ActionResult GetAllReminderallcount()
        {


            //for searching



            var UserId = User.Identity.GetUserId();
            DateTime rmdate = System.DateTime.Now.AddDays(60);
            var v = (from a in db.EmployeeDocuments
                     join b in db.Employees on a.EmployeeId equals b.EmployeeId

                     where
                      (EF.Functions.DateDiffDay(a.ExpiryDate, rmdate) >= 0)
                     select new
                     {
                         Id = a.EmployeeDocumentId,
                         Date = a.ExpiryDate,
                         Name = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                         DocumentType = a.DocumentName,
                         empid = b.EmployeeId,
                         doctype = "emp",
                     });






            var data = v.ToList();
            return Json(new { data = data });
        }
        public ActionResult remnew()
        {


            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var v = (from a in db.Reminderss
                     join b in db.Users on a.CreatedBy equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.Users on a.RequestBy equals c.Id into use
                     from c in use.DefaultIfEmpty()
                     where b.Id == UserId

                     orderby a.CreatedDate descending
                     select new
                     {
                         a.ReminderId,
                         a.CreatedDate,
                         CreatedBy = b.UserName,
                         RequestBy = c.UserName,
                         a.RDate,
                         a.Note,
                         a.RStatus,
                         a.actionurl,
                         AssignedTo = (from aa in db.ReminderAssignedss
                                       join bb in db.Employees on aa.EmployeeId equals bb.EmployeeId into emps
                                       from bb in emps.DefaultIfEmpty()
                                       where aa.ReminderId == a.ReminderId && aa.Type == a.Type
                                       select new { FirstName = bb.FirstName, LastName = bb.LastName != null ? bb.LastName : "" }
                                    ).ToList(),
                         CheckUser = (UserId == a.CreatedBy) ? true : false,
                         Date = SqlFunctions.DateName("day", a.RDate) + "-" + SqlFunctions.DateName("month", a.RDate) + "-" + SqlFunctions.DateName("year", a.RDate) + " " + SqlFunctions.DateName("hh", a.RDate) + ":" + SqlFunctions.DateName("mi", a.RDate)
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT

            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { data = recordsTotal });
        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Reminder rem = db.Reminders.Find(id);

            if (rem == null)
            {
                return NotFound();
            }

            ReminderViewModel remind = new ReminderViewModel();
            remind.ReminderId = rem.ReminderId;
            remind.Type = rem.Type;
            remind.Reference = rem.Reference;
            remind.RDate = Convert.ToDateTime(rem.RDate).ToString("dd-MM-yyyy");
            remind.RTime = Convert.ToDateTime(rem.RDate);
            remind.Note = rem.Note;
            remind.RStatus = rem.RStatus;

            var emp = db.ReminderAssigneds.Where(a => a.ReminderId == id && a.Type == rem.Type).Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.AssignedTos = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            return PartialView(remind);
        }


        [HttpPost]
        public JsonResult EditRemind(ReminderViewModel rem)
        {
            bool stat = false;
            string msg;

            Reminder reminds = db.Reminders.Find(rem.ReminderId);
            reminds.Note = rem.Note;

            var rDate = DateTime.Parse(rem.RDate.ToString(), new CultureInfo("en-GB"));
            //seleted date added,for fullcalender
            TimeSpan time = (rem.RTime).TimeOfDay;
            DateTime date = rDate + time;

            reminds.RDate = date;
            reminds.RStatus = rem.RStatus;

            db.Entry(reminds).State = EntityState.Modified;
            db.SaveChanges();
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Updated, UserId, "Reminder", "Reminders", findip(), reminds.ReminderId, "Reminder Updated Successfully");

            msg = "Successfully updated Reminder.";
            stat = true;

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reminder remind = db.Reminders.Find(id);
            if (remind == null)
            {
                return NotFound();
            }
            return PartialView(remind);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();
            Reminder rem = db.Reminders.Find(id);

            var remass = db.ReminderAssigneds.Where(a => a.ReminderId == id).FirstOrDefault();
            if (remass != null)

            {
                db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(a => a.ReminderId == id));
                db.SaveChanges();
            }

            db.Reminders.Remove(rem);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "Reminder", "Reminders", findip(), id, "Reminder Deleted Successfully");
            stat = true;
            msg = "Successfully Deleted Reminder details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }


        public ActionResult Deletenew(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reminderss remind = db.Reminderss.Find(id);
            if (remind == null)
            {
                return NotFound();
            }
            return PartialView(remind);
        }

        [HttpPost]
        public ActionResult DeleteConfirmednew(string note)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();

            var rem = db.Reminderss.Where(o => o.Note == note);


            db.Reminderss.RemoveRange(rem);

            db.SaveChanges();


            stat = true;
            msg = "Successfully Deleted Reminder details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult SearchType(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            List<SelectFormatNew> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ReminderAssigneds.Where(p => (p.Type.ToLower().Contains(q.ToLower()) || p.Type.Contains(q)))
                              .Select(b => new SelectFormatNew
                              {
                                  text = b.Type,
                                  id = b.Type
                              })
                              .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.ReminderAssigneds
                              .Select(b => new SelectFormatNew
                              {
                                  text = b.Type,
                                  id = b.Type
                              })
                              .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatNew() { id = "0", text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpPost]
        public JsonResult GetReminderbyId(string types, long refd)
        {
            DateTime? newdate = null;
            DateTime? olddate = null;
            DateTime today = DateTime.Now;

            var UserId = User.Identity.GetUserId();
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var v = (from a in db.Reminders
                     join b in db.Users on a.CreatedBy equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     let remas = db.ReminderAssigneds.Where(x => x.ReminderId == a.ReminderId).Select(x => x.EmployeeId).ToList()
                     where remas.Contains(empId) &&
                     a.Type == types && a.Reference == refd
                     orderby a.RDate descending
                     select new
                     {
                         a.ReminderId,
                         a.CreatedDate,
                         CreatedBy = b.UserName,
                         //RequestBy = c.UserName,
                         a.RDate,
                         a.Note,
                         a.RStatus,
                         a.Type,
                         a.Reference,
                         validity = (DateTime.Now <= a.RDate) ? "Upcoming" : "Expired",
                         AssignedTo = (from aa in db.ReminderAssigneds
                                       join bb in db.Employees on aa.EmployeeId equals bb.EmployeeId into emps
                                       from bb in emps.DefaultIfEmpty()
                                       where aa.ReminderId == a.ReminderId
                                       select new { bb.FirstName, bb.LastName }
                                   ).ToList(),
                     });

            v = v.OrderByDescending(c => c.RDate);
            var data = v.ToList();
            return Json(new { data = data });
        }
    }
}
