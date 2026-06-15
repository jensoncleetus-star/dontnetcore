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
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Drawing;
using QuickSoft.ViewModel;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class LeaveRequestController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public LeaveRequestController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [HttpGet]
        public ActionResult Downloadgoogle(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProLeaveRequest docdownload = db.ProLeaveRequests.Find(id);
            if (docdownload == null)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.MultipleDocuments
                                           where id == m.RelationID &&
                                           m.DocumentName == "leavedocument"
                                           select new Multiviewmodel
                                           {

                                               Id = m.Id,
                                               Document = m.Document,
                                               filenamelead = m.Document,
                                               DocumentName = m.DocumentName


                                           }
                                        ).ToList();
                ViewBag.document = "leave document " + docdownload.Createdate.ToString("dd-MM-yyyy");

                return PartialView(filedoc);
            }




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
  
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
   
            var UserId = User.Identity.GetUserId();
            ProLeaveRequest Quot = db.ProLeaveRequests.Where(x => x.LeaveRequestId == id).FirstOrDefault();


            if (Quot == null)
            {
                return NotFound();
            }
            return PartialView(Quot);
        }
        private Boolean DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            ProLeaveRequest QSum = db.ProLeaveRequests.Find(id);

            db.ProLeaveRequests.Remove(QSum);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "Leave Request", "Leave Reqyest", findip(), QSum.LeaveRequestId, "Successfully Deleted Leaverequest");
            return true;
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
       
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
           
            if (1==2)
            {
                msg = "aaa";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Quotation.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

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

            if (1==1)
            {
                var UserId = User.Identity.GetUserId();
                var today = System.DateTime.Now;

                Int64 AmcId = ViewModel.TransId;
                long leaveid = AmcId;
                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {
                    string path = LegacyWeb.MapPath("~/uploads/leavedocument/");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
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
                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), thumbName);

                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                                newFName = "resize_" + newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";

                            }
                            var userid = User.Identity.GetUserId();
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), newName);
                            file.SaveAs(newName);

                            var FilemultipleDocument = new FilemultipleDocuments
                            {
                                Document = newSName,
                                RelationID = leaveid,
                                DocumentName = "leavedocument",
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
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                                    lgimg.Save(resizeName);
                                }

                            }
                        }
                    }

                }
                long? id = AmcId;
                db.Reminders.RemoveRange(db.Reminders.Where(o => o.Reference == id));
                db.SaveChanges();
                db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.EntryId == id && o.Type == "leave"));
                db.SaveChanges();
                var created = "";
                Reminder reminds = new Reminder();
                reminds.Reference = leaveid;
                reminds.Note = "Leave Reason : " + " Document Uploaded";

                var rDate = System.DateTime.Now.Date;
                //seleted date added,for fullcalender



                reminds.RDate = System.DateTime.Now;
                reminds.Type = "/LeaveRequest/Index";
                reminds.RStatus = "Close";
                reminds.RequestBy = UserId;

                reminds.CreatedBy = UserId;
                reminds.Status = Status.active;
                reminds.CreatedDate = today;
                db.Reminders.Add(reminds);
                db.SaveChanges();
                long Id = reminds.ReminderId;


                //Approved By
                long[] Asby = { 1 };
                if (Asby != null && Asby.Length > 0)
                {
                    ReminderAssigned remAs = new ReminderAssigned();
                    foreach (var emp in Asby)
                    {
                        remAs.ReminderId = Id;
                        remAs.EntryId = leaveid;
                        remAs.Type = "leave";
                        remAs.EmployeeId = emp;
                        db.ReminderAssigneds.Add(remAs);
                        db.SaveChanges();
                    }
                }
                msg = "file added successfully.";
                stat = true;

                com.addlog(LogTypes.Created, UserId, "AMC", "AmcRemarks", findip(), AmcId, "Remark Added Successfully");
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        [RedirectingAction]
        
             [QkAuthorize(Roles = "Dev,LeaveRequestList")]
        public ActionResult Index()
        {
            List<SelectListItem> pstat2 = new List<SelectListItem>() {
                 new SelectListItem {
                    Text = "Pending", Value = "1"
                },
                new SelectListItem {
                    Text = "Approved", Value = "2"
                },
                 new SelectListItem {
                    Text = "Partial Approved", Value = "4"
                },

            

                new SelectListItem {
                    Text = "Rejected", Value = "3"
                }
                ,
                new SelectListItem {
                    Text = "All", Value = "10"
                }

              };
               ViewBag.AppStat = QkSelect.List(pstat2, "Value", "Text");
            return View();
        }

        [RedirectingAction]

        [QkAuthorize(Roles = "Dev,LeaveRequestList")]
        public ActionResult leavereport()
        {
           
            
            return View();
        }
        public bool getemployeeleave(long empid,DateTime fdate)
        {
            var isleave = (from a in db.ProLeaveRequests
                           join b in db.Employees on a.CreatedBy equals b.UserId
                           where a.Status == ApprovalStatus.Approved &&
                           fdate >= a.leavefromdate && fdate <= a.leavetodate &&
                           b.EmployeeId == empid

                           select new
                           {
                               a.LeaveRequestId
                           }
                          ).Any();
            return isleave;
            return false;
        }
        public ActionResult LeaveDashborad(string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            DateTime? fdate = null;
            DateTime? tdate = null;


            if (fromdate != "" && fromdate != null)
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "" && todate != null)
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (fdate == null)
                fdate = DateTime.Now.Date;
            if (tdate == null)
                tdate = DateTime.Now;
            else
                tdate = tdate.Value.Date.AddHours(23);
            tdate = tdate.Value.Date.AddHours(23);
            ViewBag.datefrom = Convert.ToDateTime(fdate).ToString("dd-MM-yyyy");
            ViewBag.dateto = Convert.ToDateTime(tdate).ToString("dd-MM-yyyy");
            Leaveviewmodel vmodel = new Leaveviewmodel();
            vmodel.TotalEmployees = db.Employees.Where(o => o.Status == 1).Count();
            if (fdate.Value.Date == tdate.Value.Date)
            {
                vmodel.AnualLeave = (from a in db.ProLeaveRequests
                                     where a.Status == ApprovalStatus.Approved &&
                                     fdate >= a.leavefromdate && fdate <= a.leavetodate &&

                                          a.LeaveType == 3
                                     select new
                                     {
                                         a.LeaveRequestId
                                     }
                                    ).Count();
                vmodel.EmergencyLeave = (from a in db.ProLeaveRequests
                                         where a.Status == ApprovalStatus.Approved &&
                                     fdate >= a.leavefromdate && fdate <= a.leavetodate &&
                                          a.LeaveType == 4
                                         select new
                                         {
                                             a.LeaveRequestId
                                         }
                                ).Count();
                vmodel.Leave = (from a in db.ProLeaveRequests
                                where a.Status == ApprovalStatus.Approved &&
                                fdate >= a.leavefromdate && fdate <= a.leavetodate

                                     && a.LeaveType == 0
                                select new
                                {
                                    a.LeaveRequestId
                                }
                                ).Count();
                vmodel.Dayoff = (from a in db.ProLeaveRequests
                                 where a.Status == ApprovalStatus.Approved &&
                           fdate >= a.leavefromdate && fdate <= a.leavetodate &&
                           a.LeaveType == 2
                                 select new
                                 {
                                     a.LeaveRequestId
                                 }
                        ).Count();
                vmodel.Suspenstion = (from a in db.ProLeaveRequests
                                 where a.Status == ApprovalStatus.Approved &&
                           fdate >= a.leavefromdate && fdate <= a.leavetodate &&
                           a.LeaveType == 5
                                 select new
                                 {
                                     a.LeaveRequestId
                                 }
                      ).Count();
                vmodel.MedicalLeave = (from a in db.ProLeaveRequests
                                       where a.Status == ApprovalStatus.Approved &&
                                      fdate >= a.leavefromdate && fdate <= a.leavetodate &&
                                        a.LeaveType == 1
                                       select new
                                       {
                                           a.LeaveRequestId
                                       }
                       ).Count();
                return View(vmodel);
            }
            else
            {

                var use = (from a in db.Employees

                           select new SelectFormat
                           {
                               id = a.EmployeeId,
                               text = a.FirstName + " " + a.LastName
                           }).OrderBy(o => o.text)
                        .ToList();



                List<HolidayListViewModel> hlist = new List<HolidayListViewModel>();
                for (DateTime i = (DateTime)fdate; i < tdate; i = i.AddDays(1))
                {
                    HolidayListViewModel HModel = new HolidayListViewModel();
                    HModel.Date = i;
                    hlist.Add(HModel);
                }
                long[] emparray = { };
                foreach (var d in hlist)
                {
                    var emps = (from a in db.ProLeaveRequests
                                join b in db.Employees on a.CreatedBy equals b.UserId
                                where a.Status == ApprovalStatus.Approved &&
                                 d.Date >= a.leavefromdate && d.Date <= a.leavetodate 
                                       select new
                                       {
                                           b.EmployeeId
                                       }
                                     ).Select(o => o.EmployeeId).Distinct().ToList().ToArray();
                    emparray = emparray.Concat(emps).ToArray();
                }
                var leaveimployees = emparray.Distinct().ToArray();
                List<employeetimesheet> ls = new List<employeetimesheet>();
                int sero = 0;
                vmodel.MedicalLeave = 0;
                vmodel.Leave = 0;
                vmodel.Dayoff = 0;
                vmodel.EmergencyLeave = 0;
                vmodel.AnualLeave = 0;
                vmodel.Suspenstion = 0;
                long[] eMedicalLeave = { };
                long[] eLeave = { };
                long[] eDayoff = { };
                long[] eEmergencyLeave = { };
                long[] eAnualLeave = { };
                long[] eSuspenstion = { };
               

                foreach (var emp in leaveimployees)
                {
                    foreach (var d in hlist)
                    {
                        DateTime orgdate = d.Date.Date;
                        var AnualLeave = (from a in db.ProLeaveRequests
                                          join b in db.Employees on a.CreatedBy equals b.UserId
                                             where a.Status == ApprovalStatus.Approved &&
                                             d.Date >= a.leavefromdate && d.Date <= a.leavetodate &&
                                             b.EmployeeId==emp &&
                                                  a.LeaveType == 3 
                                             select new
                                             {
                                                 a.LeaveRequestId
                                             }
                                   ).Count();
                        var EmergencyLeave = (from a in db.ProLeaveRequests
                                              join b in db.Employees on a.CreatedBy equals b.UserId
                                              where a.Status == ApprovalStatus.Approved &&
                                              d.Date >= a.leavefromdate && d.Date <= a.leavetodate &&
                                              b.EmployeeId == emp && a.LeaveType == 4
                                                 select new
                                                 {
                                                     a.LeaveRequestId
                                                 }
                                        ).Count();
                        var Suspenstion = (from a in db.ProLeaveRequests
                                              join b in db.Employees on a.CreatedBy equals b.UserId
                                              where a.Status == ApprovalStatus.Approved &&
                                              d.Date >= a.leavefromdate && d.Date <= a.leavetodate &&
                                              b.EmployeeId == emp && a.LeaveType == 5
                                              select new
                                              {
                                                  a.LeaveRequestId
                                              }
                                    ).Count();
                        var Leave = (from a in db.ProLeaveRequests
                                     join b in db.Employees on a.CreatedBy equals b.UserId
                                     where a.Status == ApprovalStatus.Approved &&
                                     d.Date >= a.leavefromdate && d.Date <= a.leavetodate &&
                                     b.EmployeeId == emp &&

                                      a.LeaveType == 0
                                        select new
                                        {
                                            a.LeaveRequestId
                                        }
                                        ).Count();
                        var Dayoff = (from a in db.ProLeaveRequests
                                      join b in db.Employees on a.CreatedBy equals b.UserId
                                      where a.Status == ApprovalStatus.Approved &&
                                      d.Date >= a.leavefromdate && d.Date <= a.leavetodate &&
                                      b.EmployeeId == emp &&
                            a.LeaveType == 2
                                         select new
                                         {
                                             a.LeaveRequestId
                                         }
                                ).Count();
                        var MedicalLeave = (from a in db.ProLeaveRequests
                                            join b in db.Employees on a.CreatedBy equals b.UserId
                                            where a.Status == ApprovalStatus.Approved &&
                                            d.Date >= a.leavefromdate && d.Date <= a.leavetodate &&
                                            b.EmployeeId == emp && a.LeaveType == 1
                                               select new
                                               {
                                                   a.LeaveRequestId
                                               }
                               ).Count();


                        if (MedicalLeave != 0 && !eMedicalLeave.Contains(emp))
                        {
                            vmodel.MedicalLeave = vmodel.MedicalLeave + MedicalLeave;
                            eMedicalLeave = eMedicalLeave.Concat(new long[] { emp }).ToArray();
                        }
                        else if (Leave != 0 && !eLeave.Contains(emp))
                        {
                            vmodel.Leave = vmodel.Leave + Leave;
                            eLeave = eLeave.Concat(new long[] { emp }).ToArray();
                        }
                        else if (Dayoff != 0 && !eDayoff.Contains(emp))
                        { vmodel.Dayoff = vmodel.Dayoff + Dayoff;
                            eDayoff = eDayoff.Concat(new long[] { emp }).ToArray();
                        }
                          
                        else if (EmergencyLeave != 0 && !eEmergencyLeave.Contains(emp))
                        { vmodel.EmergencyLeave = vmodel.EmergencyLeave + EmergencyLeave;
                            eEmergencyLeave = eEmergencyLeave.Concat(new long[] { emp }).ToArray();
                        }
                         
                        else if (AnualLeave != 0 && !eAnualLeave.Contains(emp))
                        { vmodel.AnualLeave = vmodel.AnualLeave + AnualLeave;
                            eAnualLeave = eAnualLeave.Concat(new long[] { emp }).ToArray();
                        }
                     
                        else if (Suspenstion != 0 && !eSuspenstion.Contains(emp))
                            { vmodel.Suspenstion = vmodel.Suspenstion + Suspenstion;
                            eSuspenstion = eSuspenstion.Concat(new long[] { emp }).ToArray();

                        }
                            




                        {
                            employeetimesheet a = new employeetimesheet();
                            var duty = getemployeeleave(emp, d.Date);
                            a.entryid = 0;
                            a.entrydate = d.Date;
                            a.servocedatefrom = d.Date;
                            a.attdate = d.Date;
                            a.leavestatus = (duty == true) ? 0 : 1;
                            a.servocedateto = d.Date;
                            a.EmployeeName = db.Employees.Where(o => o.EmployeeId == emp).Select(o => new
                            {
                                eployeename = o.FirstName + " " + o.LastName
                            }).Select(o => o.eployeename).FirstOrDefault();
                            a.userid = db.Employees.Where(o => o.EmployeeId == emp).Select(o => new
                            {
                                eployeename = o.UserId
                            }).Select(o => o.eployeename).FirstOrDefault();
                            a.totlhour = 0;
                            a.totalminute = 0;
                            ls.Add(a);
                        }
                    }
                }

                var lss = ls.OrderBy(o => o.EmployeeName);
                ViewBag.withtotal = 0;

                vmodel.et = lss.ToList();



                return View(vmodel);
            }
        }
        public ActionResult myleave()
        {
            
            var uid = User.Identity.GetUserId();
            var impid = db.Employees.Where(o => o.UserId == uid).Select(o => o.EmployeeId).FirstOrDefault();
            db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.EmployeeId == impid && o.Type == "leave"));
            db.SaveChanges();
            return View();
        }
        [HttpPost]
        [RedirectingAction]

        public ActionResult GetmyData()
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
            var userid = User.Identity.GetUserId();
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.ProLeaveRequests
                     join b in db.Employees on a.CreatedBy equals b.UserId
                     where b.UserId==userid
                     select new
                     {
                         a.LeaveRequestId,
                         EmployeeName = b.FirstName + " " + b.LastName,
                         LeaveType = (a.LeaveType == 0) ? "Leave" : (a.LeaveType == 1) ? "Medical Leave" : (a.LeaveType == 2) ? "Day Off" : (a.LeaveType == 3) ? "Annual Leave" : (a.LeaveType == 5) ? "Suspenstion" : "Emergency Leave",

                         a.leavefromtime,
                         a.leavetotime,
                         a.leavereason,
                         a.notes,
                         a.Status,
                         ApprovalStatus = true,
                     });
            var vc = v.ToList().Select(o => new
            {
                o.LeaveRequestId,
                o.EmployeeName,
                o.LeaveType,
                o.Status,
                leavefromtime = Convert.ToDateTime(Convert.ToDateTime(o.leavefromtime).ToString("yyyy-MM-dd hh:mm")),
                leavetotime = Convert.ToDateTime(Convert.ToDateTime(o.leavetotime).ToString("yyyy-MM-dd hh:mm")),
                Dayss = (Convert.ToDateTime(o.leavetotime) - Convert.ToDateTime(o.leavefromtime)).TotalDays,
                o.leavereason,
                o.notes,
                o.ApprovalStatus



            });




            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                vc = vc.Where(p => p.EmployeeName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                vc = vc.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = vc.Count();
            var data = vc.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpPost]
        [RedirectingAction]

        public ActionResult GetDataleave( string fromdate, string todate,int type)
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
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? sdate = null;
            DateTime? edate = null;
            DateTime? ndate = null;
            DateTime? apfromdate = null;
            DateTime? aptodate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
         }
            
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            if (type != 10)
            {
                int[] leaves = { 0, 2 };
                var v = (from b in db.Employees
                         join a in db.ProLeaveRequests on b.UserId equals a.CreatedBy into emp
                         from a in emp.DefaultIfEmpty()
                         join c in db.Designations on b.DesignationID equals c.DesignationID into des
                         from c in des.DefaultIfEmpty()
                         where (type == 10 || fdate >= a.leavefromdate && fdate <= a.leavetodate) &&

                                         (type == 10 ||(type==0|| a.LeaveType == type)) &&
                                           (type == 10 || (type != 0 || leaves.Contains(a.LeaveType ))) &&
                                         b.Status == 1 &&
                                         a.Status==ApprovalStatus.Approved
                         select new
                         {

                             EmployeeName = b.FirstName + " " + b.LastName,
                             Designation = (c.DesignationName == null) ? "" : c.DesignationName,
                             JoiningDate = (b.JoinDate == null) ? b.CreatedDate : b.JoinDate,
                             LeaveType = (a.LeaveType == 0) ? "Leave" : (a.LeaveType == 1) ? "Medical Leave" : (a.LeaveType == 2) ? "Day Off" : (a.LeaveType == 3) ? "Annual Leave" : (a.LeaveType == 5) ? "Suspenstion" : "Emergency Leave",
                             leavefromtime = (a.leavefromtime == null) ? null : a.leavefromtime,
                             leavetotime = (a.leavetotime == null) ? null : a.leavetotime,
                             leavereason = (a.leavereason == null) ? "" : a.leavereason,
                             notes = (a.notes == null) ? "" : a.notes,
                             Status = (a.Status == null) ? ApprovalStatus.Completed : a.Status,
                             ApprovalStatus = true,
                         });
                var vc = v.ToList().Select(o => new
                {

                    o.EmployeeName,
                    o.LeaveType,
                    o.Designation,
                    o.JoiningDate,
                    o.Status,
                    leavefromtime = Convert.ToDateTime(Convert.ToDateTime(o.leavefromtime).ToString("yyyy-MM-dd hh:mm")),
                    leavetotime = Convert.ToDateTime(Convert.ToDateTime(o.leavetotime).ToString("yyyy-MM-dd hh:mm")),
                    Dayss = (Convert.ToDateTime(o.leavetotime) - System.DateTime.Now).TotalDays,
                    o.leavereason,
                    o.notes,
                    o.ApprovalStatus



                });




                //search
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    vc = vc.Where(p => p.EmployeeName.ToString().ToLower().Contains(search.ToLower()));
                }

                //SORT
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    vc = vc.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = vc.Count();
                var data = vc.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            else
            {

                var v = (from b in db.Employees

                         join c in db.Designations on b.DesignationID equals c.DesignationID into des
                         from c in des.DefaultIfEmpty()
                         let a = (from aa in db.ProLeaveRequests
                                  where aa.CreatedBy == b.UserId
                         && aa.Status == ApprovalStatus.Approved
                            
                                  select new
                                  {
                                      
                                      aa.approvedby,
                                      aa.approveddate,
                                      aa.Createdate,
                                      aa.CreatedBy,
                                      aa.leavefromdate,
                                      aa.leavefromtime,
                                      aa.leavereason,
                                      aa.LeaveRequestId,
                                      aa.leavetodate,
                                      aa.notes,
                                      aa.Status,
                                      aa.leavetotime,
                                      aa.LeaveType,


                                  }
                                ).OrderByDescending(o=>o.leavefromdate).FirstOrDefault()
                         where b.Status==1
                         select new
                         {


                             EmployeeName = b.FirstName + " " + b.LastName,
                             Designation = (c.DesignationName == null) ? "" : c.DesignationName,
                             JoiningDate = (b.JoinDate == null) ? b.CreatedDate : b.JoinDate,
                             LeaveType = (a == null) ? "" : (a.LeaveType == 0) ? "Leave" : (a.LeaveType == 1) ? "Medical Leave" : (a.LeaveType == 2) ? "Day Off" : (a.LeaveType == 3) ? "Annual Leave" : "Emergency Leave",
                             leavefromtime = (a == null) ? null : a.leavefromtime,
                             leavetotime = (a == null) ? null : a.leavetotime,
                             leavereason = (a == null) ? "" : a.leavereason,
                             notes = (a == null) ? "" : a.notes,
                             Status = (a == null) ? ApprovalStatus.Completed : a.Status,
                             ApprovalStatus = true,
                         });
                var vc = v.ToList().Select(o => new
                {

                    o.EmployeeName,
                    o.LeaveType,
                    o.Designation,
                    o.JoiningDate,
                    o.Status,
                    leavefromtime =Convert.ToDateTime(Convert.ToDateTime(o.leavefromtime).ToString("yyyy-MM-dd hh:mm")),
                    leavetotime =  Convert.ToDateTime(Convert.ToDateTime(o.leavetotime).ToString("yyyy-MM-dd hh:mm")),
                    Dayss = (o.leavefromtime == null) ? 0 : (Convert.ToDateTime(o.leavetotime) - System.DateTime.Now).TotalDays,
                    o.leavereason,
                    o.notes,
                    o.ApprovalStatus



                });




                //search
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    vc = vc.Where(p => p.EmployeeName.ToString().ToLower().Contains(search.ToLower()));
                }

                //SORT
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    vc = vc.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = vc.Count();
                var data = vc.Skip(skip).Take(pageSize).ToList();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            }
            }
        [HttpPost]
        [RedirectingAction]

        public ActionResult GetData(string FromDate,string ToDate,int appr)
        {

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
     
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB"));
            }
            ApprovalStatus s;
            s = ApprovalStatus.PendingApproval;

            if (appr == 1)
                s = ApprovalStatus.PendingApproval;
            else if (appr == 2)
                s = ApprovalStatus.Approved;

            else if (appr == 3)
                s = ApprovalStatus.Rejected;
            else if (appr == 4)
                s = ApprovalStatus.PartialApproval;


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
            ApprovalStatus[] aaa = { ApprovalStatus.PendingApproval, ApprovalStatus.PartialApproval};
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
       
            var v = (from a in db.ProLeaveRequests
                     join b in db.Employees on a.CreatedBy equals b.UserId
                     
                       where (appr == 10 || (s == ApprovalStatus.PendingApproval && aaa.Contains(a.Status))||a.Status==s) &&
                     (FromDate==""||fdate<= a.leavefromtime) &&
                     (ToDate == "" || a.leavetotime <= tdate) 
                  

                     select new
                     {
                         a.LeaveRequestId,
                         EmployeeName=b.FirstName+" "+b.LastName,
                         LeaveType=(a.LeaveType==0)?"Leave":(a.LeaveType==1)? "Medical Leave":(a.LeaveType == 2) ? "Day Off" :(a.LeaveType == 3) ? "Annual Leave" : (a.LeaveType == 5) ? "Suspenstion":"Emergency Leave",
                         a.leavefromtime,
                         a.leavetotime,
                         a.leavereason,
                         a.notes,
                         a.Status,
                         a.Createdate,
                         ApprovalStatus=true,
                     });
           var  vc=v.OrderByDescending(o=>o.leavefromtime).ThenByDescending(o=>o.Status).ToList().Select(o => new
            {
                o.LeaveRequestId,
                o.EmployeeName,
                o.LeaveType,
                o.Status,
                leavefromtime= Convert.ToDateTime(Convert.ToDateTime(o.leavefromtime).ToString("yyyy-MM-dd hh:mm")),
                leavetotime= Convert.ToDateTime(Convert.ToDateTime(o.leavetotime).ToString("yyyy-MM-dd hh:mm")),
                Dayss= (Convert.ToDateTime(o.leavetotime)- Convert.ToDateTime(o.leavefromtime)).TotalDays,
                o.leavereason,
                o.notes,
                o.ApprovalStatus



            });


  

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                vc = vc.Where(p => p.EmployeeName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                vc = vc.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = vc.Count();
            var data = vc.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [RedirectingAction]

        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "Quotation" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList();

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       where e != ApprovalStatus.PendingApproval && (appstat.Count == 0 || e != appstat.Select(a => a.ApprovalStatus).FirstOrDefault())
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            return PartialView();
        }

        [HttpPost]
        public ActionResult EditStatus(ApprovalUpdate App, long id)
        {
            bool stat = false;
            string msg = "";
            var UserId = User.Identity.GetUserId();

            var MR = db.ProLeaveRequests.Where(a => a.LeaveRequestId == id).FirstOrDefault();

            MR.Status = App.ApprovalStatus;
            MR.notes = MR.notes+"<br> Entry Date :"+System.DateTime.Now.ToString("dd-MM-yyyy")+" : "+ App.Note;
            db.Entry(MR).State = EntityState.Modified;
            db.SaveChanges();
            

            db.Reminders.RemoveRange(db.Reminders.Where(o => o.Reference == id));
            db.SaveChanges();
            db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.EntryId == id && o.Type == "leave"));
            db.SaveChanges();
            
            string leavestatus = "Rejected";
            if (App.ApprovalStatus == ApprovalStatus.Approved)
                leavestatus = "Approved";
            else if (App.ApprovalStatus == ApprovalStatus.PendingApproval)
                leavestatus = "Pending Approval";
            else if (App.ApprovalStatus == ApprovalStatus.Rejected)
                leavestatus = "Rejected";
            
            var created = "";
            Reminder reminds = new Reminder();
            reminds.Reference = id;
            reminds.Note =   "HR Note : " + leavestatus+"<br/>"+ MR.notes;

            var rDate = System.DateTime.Now.Date;
            //seleted date added,for fullcalender



            reminds.RDate = System.DateTime.Now;
            reminds.Type = "/LeaveRequest/myleave";
            reminds.RStatus = "Close";
            reminds.RequestBy = UserId;

            reminds.CreatedBy = UserId;
            reminds.Status = Status.active;
            reminds.CreatedDate = System.DateTime.Now;
            db.Reminders.Add(reminds);
            db.SaveChanges();
            long Id = reminds.ReminderId;

            var createdby = db.ProLeaveRequests.Where(o => o.LeaveRequestId == id).Select(o => o.CreatedBy).FirstOrDefault();
            var empids = db.Employees.Where(o => o.UserId == createdby).Select(o => o.EmployeeId).FirstOrDefault();
            //Approved By
            long[] Asby = {empids };
            if (Asby != null && Asby.Length > 0)
            {
                ReminderAssigned remAs = new ReminderAssigned();
                foreach (var emp in Asby)
                {
                    remAs.ReminderId = Id;
                    remAs.EntryId = id;
                    remAs.Type = "leave";
                    remAs.EmployeeId = emp;
                    db.ReminderAssigneds.Add(remAs);
                    db.SaveChanges();
                }
            }
            stat = true;
            msg = "Successfully Updated Status.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [QkAuthorize(Roles = "Dev,LeaveRequestCreate")]
        public ActionResult Create()
        {
            var userid = User.Identity.GetUserId();
            ViewBag.salesman = "true";
            if (User.IsInRole("LeaveRequestList"))
            {
                ViewBag.salesman = "false";
            }

            var use = (from a in db.Employees
                       join b in db.Users on a.UserId equals b.Id
                       where b.Status == 1
                       select a).Where(o => o.UserId == userid).Select(s => new
                       {
                           ID = s.EmployeeId,
                           Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                       })
                            .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");


            List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Leave", Value = "0"
                },
                new SelectListItem {
                    Text = "Medical leave", Value = "1"
                }
               
                  ,
                new SelectListItem {
                    Text = "Annual Leave", Value = "3"
                }
                  ,
                new SelectListItem {
                    Text = "Emergecy Leave", Value = "4"
                }
                 ,
                new SelectListItem {
                 Text = "Suspention", Value = "5"
                }
              };
            ProLeaveRequestviewmodel st = new ProLeaveRequestviewmodel();
            st.leavefromdate = System.DateTime.Now.AddDays(1).ToString("dd-MM-yyy");
            st.leavetodate= System.DateTime.Now.AddDays(1).ToString("dd-MM-yyy");
            st.leavefromtime = System.DateTime.Now.Date;
            st.leavetotime = System.DateTime.Now.Date.AddHours(23.99);
            ViewBag.OpnCls = pstat2;
            
            st.SECashier = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();

            return View(st);
        }


        [HttpPost]
        [RedirectingAction]

        public JsonResult Edit(long? id,ProLeaveRequestviewmodel ttype)
        {
            bool stat = false;
            string msg;
           
                

            DateTime sDate = DateTime.Parse(ttype.leavefromdate.ToString(), new CultureInfo("en-GB"));
            DateTime eDate = DateTime.Parse(ttype.leavetodate.ToString(), new CultureInfo("en-GB"));


            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var today = Convert.ToDateTime(System.DateTime.Now);
            ProLeaveRequest vm = db.ProLeaveRequests.Find(id);
            vm.Createdate = System.DateTime.Now;
            vm.CreatedBy = db.Employees.Where(o => o.EmployeeId == ttype.SECashier).Select(o => o.UserId).FirstOrDefault();
            vm.leavefromdate = sDate;
            vm.leavetodate = eDate;
            vm.approveddate = System.DateTime.Now;
            vm.Status = ApprovalStatus.PendingApproval;
            TimeSpan? stime = null;
            if (ttype.leavefromtime != null)
            {
                stime = ((DateTime)ttype.leavefromtime).TimeOfDay;
            }
            else
            {
                stime = ((DateTime)sDate).TimeOfDay;

            }

            DateTime? stimes = sDate + stime;
            vm.leavefromtime = stimes;
            TimeSpan? etime = null;
            if (ttype.leavetotime != null)
            {
                etime = ((DateTime)ttype.leavetotime).TimeOfDay;
            }
            else
            {
                etime = ((DateTime)eDate).TimeOfDay;
            }
            DateTime? etimes = eDate + etime;
            vm.leavetotime = etimes;
            vm.leavereason = ttype.leavereason;
            vm.Status = ApprovalStatus.PendingApproval;
            vm.LeaveType = ttype.LeaveType;


            db.Entry(vm).State = EntityState.Modified;

            db.SaveChanges();
            long leaveid = vm.LeaveRequestId;
            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/leavedocument/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
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
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";

                        }
                        var userid = User.Identity.GetUserId();
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), newName);
                        file.SaveAs(newName);

                        var FilemultipleDocument = new FilemultipleDocuments
                        {
                            Document = newSName,
                            RelationID = leaveid,
                            DocumentName = "leavedocument",
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
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }

            }


            db.SaveChanges();
            db.Reminders.RemoveRange(db.Reminders.Where(o => o.Reference == id));
            db.SaveChanges();
            db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.EntryId == id && o.Type == "leave"));
            db.SaveChanges();
            var created = "";
            Reminder reminds = new Reminder();
            reminds.Reference = leaveid;
            reminds.Note = "Leave Reason : " + ttype.leavereason;

            var rDate = System.DateTime.Now.Date;
            //seleted date added,for fullcalender



            reminds.RDate = System.DateTime.Now;
            reminds.Type = "/LeaveRequest/Index";
            reminds.RStatus = "Close";
            reminds.RequestBy = UserId;

            reminds.CreatedBy = UserId;
            reminds.Status = Status.active;
            reminds.CreatedDate = today;
            db.Reminders.Add(reminds);
            db.SaveChanges();
            long Id = reminds.ReminderId;


            //Approved By
            long[] Asby = { 1 };
            if (Asby != null && Asby.Length > 0)
            {
                ReminderAssigned remAs = new ReminderAssigned();
                foreach (var emp in Asby)
                {
                    remAs.ReminderId = Id;
                    remAs.EntryId = leaveid;
                    remAs.Type = "leave";
                    remAs.EmployeeId = emp;
                    db.ReminderAssigneds.Add(remAs);
                    db.SaveChanges();
                }
            }
            msg = "Leave added successfully.";
            stat = true;
            com.addlog(LogTypes.Updated, UserId, "ProLeaveRequest", "ProLeaveRequests", findip(), (long)id, "Task Type Added Successfully");


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = id } };
        }

        [HttpPost]
        [RedirectingAction]

        public JsonResult Create(ProLeaveRequestviewmodel ttype)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            DateTime sDate = DateTime.Parse(ttype.leavefromdate.ToString(), new CultureInfo("en-GB"));
            DateTime eDate = DateTime.Parse(ttype.leavetodate.ToString(), new CultureInfo("en-GB"));


            var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);
            ProLeaveRequest vm = new ProLeaveRequest();
                     vm.Createdate = System.DateTime.Now;
                    vm.CreatedBy = db.Employees.Where(o=>o.EmployeeId==ttype.SECashier).Select(o=>o.UserId).FirstOrDefault();
            vm.leavefromdate = sDate;
            vm.leavetodate = eDate;
            vm.approveddate = System.DateTime.Now;
            vm.Status = ApprovalStatus.PendingApproval;
            TimeSpan? stime = null;
            if (ttype.leavefromtime != null)
            {
                stime = ((DateTime)ttype.leavefromtime).TimeOfDay;
            }
            else
            {
                stime = ((DateTime)sDate).TimeOfDay;
            
        }

            DateTime? stimes = sDate + stime;
            vm.leavefromtime = stimes;
            TimeSpan? etime = null;
            if (ttype.leavetotime != null)
            {
                etime = ((DateTime)ttype.leavetotime).TimeOfDay;
            }
            else
            {
                etime = ((DateTime)eDate).TimeOfDay;
            }
            DateTime? etimes = eDate + etime;
            vm.leavetotime = etimes;
            vm.leavereason = ttype.leavereason;
            vm.Status = ApprovalStatus.PendingApproval;
            vm.LeaveType = ttype.LeaveType;


            db.ProLeaveRequests.Add(vm);
            db.SaveChanges();
            long leaveid = vm.LeaveRequestId;
            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/leavedocument/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
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
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";

                        }
                        var userid = User.Identity.GetUserId();
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), newName);
                        file.SaveAs(newName);

                        var FilemultipleDocument = new FilemultipleDocuments
                        {
                            Document = newSName,
                            RelationID = leaveid,
                            DocumentName = "leavedocument",
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
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leavedocument/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }

            }


            db.SaveChanges();
         

            var created = "";
            Reminder reminds = new Reminder();
            reminds.Reference = leaveid;
            reminds.Note ="Leave Reason : "+ttype.leavereason;

            var rDate = System.DateTime.Now.Date;
            //seleted date added,for fullcalender
          
       

            reminds.RDate = System.DateTime.Now;
            reminds.Type = "/LeaveRequest/Index";
            reminds.RStatus = "Close";
            reminds.RequestBy = UserId;

            reminds.CreatedBy = UserId;
            reminds.Status = Status.active;
            reminds.CreatedDate = today;
            db.Reminders.Add(reminds);
            db.SaveChanges();
            Id = reminds.ReminderId;


            //Approved By
            long[] Asby = { 5 };
            if (Asby != null && Asby.Length > 0)
            {
                ReminderAssigned remAs = new ReminderAssigned();
                foreach (var emp in Asby)
                {
                    remAs.ReminderId = Id;
                    remAs.EntryId = leaveid;
                    remAs.Type = "leave";
                    remAs.EmployeeId = emp;
                    db.ReminderAssigneds.Add(remAs);
                    db.SaveChanges();
                }
            }
            msg = "Leave added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "ProLeaveRequest", "ProLeaveRequests", findip(), Id, "Task Type Added Successfully");
                
        
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [RedirectingAction]

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProLeaveRequest st = db.ProLeaveRequests.Find(id);

            

            ViewBag.image = (from b in db.MultipleDocuments
                             join c in db.ProLeaveRequests on b.RelationID equals c.LeaveRequestId
                             where c.LeaveRequestId == id && b.DocumentName== "leavedocument"
                             select new TaskImageViewModel
                             {
                                 TaskImageId = b.Id,
                                 TaskId = id,
                                 FileName = b.Document ,
                                 TaskName = b.DocumentName,
                             }).ToList();
            var userid = User.Identity.GetUserId();
            ViewBag.salesman = "true";
            if (User.IsInRole("LeaveRequestList"))
            {
                ViewBag.salesman = "false";
            }

            var use = (from a in db.Employees
                       join b in db.Users on a.UserId equals b.Id
                       where b.Status == 1
                       select a).Where(o => o.UserId == st.CreatedBy).Select(s => new
                       {
                           ID = s.EmployeeId,
                           Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                       })
                            .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");


            List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Leave", Value = "0"
                },
                new SelectListItem {
                    Text = "Medical leave", Value = "1"
                }
                ,
                new SelectListItem {
                    Text = "Day off", Value = "2"
                }
                  ,
                new SelectListItem {
                    Text = "Annual Leave", Value = "3"
                }
                  ,
                new SelectListItem {
                    Text = "Emergecy Leave", Value = "4"
                }
              };
            ProLeaveRequestviewmodel sta = new ProLeaveRequestviewmodel();
            sta.leavefromdate = st.leavefromdate.ToString("dd-MM-yyy");
            sta.leavetodate = st.leavetodate.ToString("dd-MM-yyy");
            sta.SECashier = db.Employees.Where(o => o.UserId == st.CreatedBy).Select(o => o.EmployeeId).FirstOrDefault();
            sta.LeaveType = st.LeaveType;
            sta.leavereason = st.leavereason;
            ViewBag.OpnCls = pstat2;


            return View(sta);
          
        }

        //[HttpPost]
        //[RedirectingAction]






        //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        // GET: ProductCategory/Delete/5
        //[RedirectingAction]


        //// POST: ProductCategory/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[RedirectingAction]




        //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };


        //        serialisedJson = db.ProLeaveRequests.Where(p => p.TypeName.ToLower().Contains(q.ToLower()) || p.TypeName.Contains(q))
        //                          .Select(b => new SelectFormat
        //                              text = b.TypeName, //each json object will have 
        //                              id = b.TaskTypeId
        //                          })
        //        serialisedJson = db.ProLeaveRequests.Select(b => new SelectFormat
        //            text = b.TypeName, //each json object will have 
        //            id = b.TaskTypeId

        //    }//

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
