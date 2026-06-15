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
    public class ResignationController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ResignationController()
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
            ProResignRequest docdownload = db.ProResignRequests.Find(id);
            if (docdownload == null)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.MultipleDocuments
                                           where id == m.RelationID &&
                                           m.DocumentName == "Resigndocument"
                                           select new Multiviewmodel
                                           {

                                               Id = m.Id,
                                               Document = m.Document,
                                               filenamelead = m.Document,
                                               DocumentName = m.DocumentName


                                           }
                                        ).ToList();
                ViewBag.document = "Resign document " + docdownload.Createdate.ToString("dd-MM-yyyy");

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
            ProResignRequest Quot = db.ProResignRequests.Where(x => x.ResignRequestId == id).FirstOrDefault();


            if (Quot == null)
            {
                return NotFound();
            }
            return PartialView(Quot);
        }
        private Boolean DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            ProResignRequest QSum = db.ProResignRequests.Find(id);

            db.ProResignRequests.Remove(QSum);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "Resign Request", "Resign Reqyest", findip(), QSum.ResignRequestId, "Successfully Deleted Resignation");
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
                long Resignid = AmcId;
                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {
                    string path = LegacyWeb.MapPath("~/uploads/Resigndocument/");
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
                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), thumbName);

                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                                newFName = "resize_" + newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";

                            }
                            var userid = User.Identity.GetUserId();
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), newName);
                            file.SaveAs(newName);

                            var FilemultipleDocument = new FilemultipleDocuments
                            {
                                Document = newSName,
                                RelationID = Resignid,
                                DocumentName = "Resigndocument",
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
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                                    lgimg.Save(resizeName);
                                }

                            }
                        }
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
        
             [QkAuthorize(Roles = "Dev,ResignationList")]
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

        [QkAuthorize(Roles = "Dev,ResignationList")]
        public ActionResult Resignreport()
        {
           
            
            return View();
        }

        public ActionResult ResignDashborad(string fromdate, string todate)
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

           


            return View(vmodel);
        }
        public ActionResult myResign()
        {
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
            var v = (from a in db.ProResignRequests
                     join b in db.Employees on a.CreatedBy equals b.UserId
                     where b.UserId==userid
                     select new
                     {
                         a.ResignRequestId,
                         EmployeeName = b.FirstName + " " + b.LastName,
                         ResignType = (a.ResignType == 0) ? "Resign" : (a.ResignType == 1) ? "Medical Resign" : (a.ResignType == 2) ? "Day Off" : (a.ResignType == 3) ? "Annual Resign" : "Emergency Resign",


                         a.Resignfromtime,
                         a.Resigntotime,
                         a.Resignreason,
                         a.notes,
                         a.Status,
                         ApprovalStatus = true,
                     });
            var vc = v.ToList().Select(o => new
            {
                o.ResignRequestId,
                o.EmployeeName,
                o.ResignType,
                o.Status,
                Resignfromtime = Convert.ToDateTime(Convert.ToDateTime(o.Resignfromtime).ToString("yyyy-MM-dd hh:mm")),
                Resigntotime = Convert.ToDateTime(Convert.ToDateTime(o.Resigntotime).ToString("yyyy-MM-dd hh:mm")),
                Dayss = (Convert.ToDateTime(o.Resigntotime) - Convert.ToDateTime(o.Resignfromtime)).TotalDays,
                o.Resignreason,
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

        public ActionResult GetDataResign( string fromdate, string todate,int type)
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
                var v = (from b in db.Employees
                         join a in db.ProResignRequests on b.UserId equals a.CreatedBy into emp
                         from a in emp.DefaultIfEmpty()
                         join c in db.Designations on b.DesignationID equals c.DesignationID into des
                         from c in des.DefaultIfEmpty()
                         where (type == 10 || fdate >= a.Resignfromdate && fdate <= a.Resigntodate) &&

                                         (type == 10 || a.ResignType == type) &&
                                         b.Status == 1 &&
                                         a.Status==ApprovalStatus.Approved
                         select new
                         {

                             EmployeeName = b.FirstName + " " + b.LastName,
                             Designation = (c.DesignationName == null) ? "" : c.DesignationName,
                             JoiningDate = (b.JoinDate == null) ? b.CreatedDate : b.JoinDate,
                             ResignType = (a.ResignType == null) ? "" : (a.ResignType == 0) ? "Resign" : (a.ResignType == 1) ? "Medical Resign" : (a.ResignType == 2) ? "Day Off" : (a.ResignType == 3) ? "Annual Resign" : "Emergency Resign",
                             Resignfromtime = (a.Resignfromtime == null) ? null : a.Resignfromtime,
                             Resigntotime = (a.Resigntotime == null) ? null : a.Resigntotime,
                             Resignreason = (a.Resignreason == null) ? "" : a.Resignreason,
                             notes = (a.notes == null) ? "" : a.notes,
                             Status = (a.Status == null) ? ApprovalStatus.Completed : a.Status,
                             ApprovalStatus = true,
                         });
                var vc = v.ToList().Select(o => new
                {

                    o.EmployeeName,
                    o.ResignType,
                    o.Designation,
                    o.JoiningDate,
                    o.Status,
                    Resignfromtime = Convert.ToDateTime(Convert.ToDateTime(o.Resignfromtime).ToString("yyyy-MM-dd hh:mm")),
                    Resigntotime = Convert.ToDateTime(Convert.ToDateTime(o.Resigntotime).ToString("yyyy-MM-dd hh:mm")),
                    Dayss = (Convert.ToDateTime(o.Resigntotime) - System.DateTime.Now).TotalDays,
                    o.Resignreason,
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
                         let a = (from aa in db.ProResignRequests
                                  where aa.CreatedBy == b.UserId
                         && aa.Status == ApprovalStatus.Approved
                            
                                  select new
                                  {
                                      
                                      aa.approvedby,
                                      aa.approveddate,
                                      aa.Createdate,
                                      aa.CreatedBy,
                                      aa.Resignfromdate,
                                      aa.Resignfromtime,
                                      aa.Resignreason,
                                      aa.ResignRequestId,
                                      aa.Resigntodate,
                                      aa.notes,
                                      aa.Status,
                                      aa.Resigntotime,
                                      aa.ResignType,


                                  }
                                ).OrderByDescending(o=>o.Resignfromdate).FirstOrDefault()
                         where b.Status==1
                         select new
                         {


                             EmployeeName = b.FirstName + " " + b.LastName,
                             Designation = (c.DesignationName == null) ? "" : c.DesignationName,
                             JoiningDate = (b.JoinDate == null) ? b.CreatedDate : b.JoinDate,
                             ResignType = (a == null) ? "" : (a.ResignType == 0) ? "Resign" : (a.ResignType == 1) ? "Medical Resign" : (a.ResignType == 2) ? "Day Off" : (a.ResignType == 3) ? "Annual Resign" : "Emergency Resign",
                             Resignfromtime = (a == null) ? null : a.Resignfromtime,
                             Resigntotime = (a == null) ? null : a.Resigntotime,
                             Resignreason = (a == null) ? "" : a.Resignreason,
                             notes = (a == null) ? "" : a.notes,
                             Status = (a == null) ? ApprovalStatus.Completed : a.Status,
                             ApprovalStatus = true,
                         });
                var vc = v.ToList().Select(o => new
                {

                    o.EmployeeName,
                    o.ResignType,
                    o.Designation,
                    o.JoiningDate,
                    o.Status,
                    Resignfromtime =Convert.ToDateTime(Convert.ToDateTime(o.Resignfromtime).ToString("yyyy-MM-dd hh:mm")),
                    Resigntotime =  Convert.ToDateTime(Convert.ToDateTime(o.Resigntotime).ToString("yyyy-MM-dd hh:mm")),
                    Dayss = (o.Resignfromtime == null) ? 0 : (Convert.ToDateTime(o.Resigntotime) - System.DateTime.Now).TotalDays,
                    o.Resignreason,
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

        public ActionResult GetData(string FromDate,int appr)
        {

            DateTime? fdate = null;
         
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
            ApprovalStatus s;
            s = ApprovalStatus.PendingApproval;

            if (appr == 1)
                s = ApprovalStatus.PendingApproval;
            else if (appr == 2)
                s = ApprovalStatus.Approved;

            else if (appr == 3)
                s = ApprovalStatus.Rejected;
           

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
            var v = (from a in db.ProResignRequests
                     join b in db.Employees on a.CreatedBy equals b.UserId
                     where (appr==10||a.Status==s)&&
                     (FromDate==""||a.Resignfromdate>=fdate)
                     select new
                     {
                         a.ResignRequestId,
                         EmployeeName=b.FirstName+" "+b.LastName,
                         ResignType=(a.ResignType==0)?"Resign":(a.ResignType==1)? "Medical Resign":(a.ResignType == 2) ? "Day Off" :(a.ResignType == 3) ? "Annual Resign" :"Emergency Resign",
                         a.Resignfromtime,
                         a.Resigntotime,
                         a.Resignreason,
                         a.notes,
                         a.Status,
                         a.Createdate,
                         ApprovalStatus=true,
                     });
           var  vc=v.OrderByDescending(o=>o.Resignfromtime).ThenByDescending(o=>o.Status).ToList().Select(o => new
            {
                o.ResignRequestId,
                o.EmployeeName,
                o.ResignType,
                o.Status,
                Resignfromtime= Convert.ToDateTime(Convert.ToDateTime(o.Resignfromtime).ToString("yyyy-MM-dd hh:mm")),
                Resigntotime= Convert.ToDateTime(Convert.ToDateTime(o.Resigntotime).ToString("yyyy-MM-dd hh:mm")),
                Dayss= (Convert.ToDateTime(o.Resigntotime)- Convert.ToDateTime(o.Resignfromtime)).TotalDays,
                o.Resignreason,
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

            var MR = db.ProResignRequests.Where(a => a.ResignRequestId == id).FirstOrDefault();

            MR.Status = App.ApprovalStatus;
            MR.notes = MR.notes+"<br> Entry Date :"+System.DateTime.Now.ToString("dd-MM-yyyy")+" : "+ App.Note;
            db.Entry(MR).State = EntityState.Modified;
            db.SaveChanges();
                stat = true;
                msg = "Successfully Updated Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        
           

        }
        [QkAuthorize(Roles = "Dev,ResignationCreate")]
        public ActionResult Create()
        {
            var userid = User.Identity.GetUserId();
            ViewBag.salesman = "true";
            if (User.IsInRole("ResignationList"))
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
                    Text = "Resign", Value = "0"
                },
                new SelectListItem {
                    Text = "Medical Resign", Value = "1"
                }
                ,
                new SelectListItem {
                    Text = "Day off", Value = "2"
                }
                  ,
                new SelectListItem {
                    Text = "Annual Resign", Value = "3"
                }
                  ,
                new SelectListItem {
                    Text = "Emergecy Resign", Value = "4"
                }
              };
            ProResignRequestviewmodel st = new ProResignRequestviewmodel();
            st.Resignfromdate = System.DateTime.Now.AddDays(1).ToString("dd-MM-yyy");
            st.Resigntodate= System.DateTime.Now.AddDays(1).ToString("dd-MM-yyy");
            st.Resignfromtime = System.DateTime.Now.Date;
            st.Resigntotime = System.DateTime.Now.Date.AddHours(23.99);
            ViewBag.OpnCls = pstat2;
            
            st.SECashier = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();

            return View(st);
        }


        [HttpPost]
        [RedirectingAction]

        public JsonResult Edit(long? id,ProResignRequestviewmodel ttype)
        {
            bool stat = false;
            string msg;
           
                

            DateTime sDate = DateTime.Parse(ttype.Resignfromdate.ToString(), new CultureInfo("en-GB"));
            DateTime eDate = DateTime.Parse(ttype.Resigntodate.ToString(), new CultureInfo("en-GB"));


            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var today = Convert.ToDateTime(System.DateTime.Now);
            ProResignRequest vm = db.ProResignRequests.Find(id);
            vm.Createdate = System.DateTime.Now;
            vm.CreatedBy = db.Employees.Where(o => o.EmployeeId == ttype.SECashier).Select(o => o.UserId).FirstOrDefault();
            vm.Resignfromdate = sDate;
            vm.Resigntodate = eDate;
            vm.approveddate = System.DateTime.Now;
            vm.Status = ApprovalStatus.PendingApproval;
            TimeSpan? stime = null;
            if (ttype.Resignfromtime != null)
            {
                stime = ((DateTime)ttype.Resignfromtime).TimeOfDay;
            }
            else
            {
                stime = ((DateTime)sDate).TimeOfDay;

            }

            DateTime? stimes = sDate + stime;
            vm.Resignfromtime = stimes;
            TimeSpan? etime = null;
            if (ttype.Resigntotime != null)
            {
                etime = ((DateTime)ttype.Resigntotime).TimeOfDay;
            }
            else
            {
                etime = ((DateTime)eDate).TimeOfDay;
            }
            DateTime? etimes = eDate + etime;
            vm.Resigntotime = etimes;
            vm.Resignreason = ttype.Resignreason;
            vm.Status = ApprovalStatus.PendingApproval;
            vm.ResignType = ttype.ResignType;


            db.Entry(vm).State = EntityState.Modified;

            db.SaveChanges();
            long Resignid = vm.ResignRequestId;
            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/Resigndocument/");
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
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";

                        }
                        var userid = User.Identity.GetUserId();
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), newName);
                        file.SaveAs(newName);

                        var FilemultipleDocument = new FilemultipleDocuments
                        {
                            Document = newSName,
                            RelationID = Resignid,
                            DocumentName = "Resigndocument",
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
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }

            }


            db.SaveChanges();

            msg = "Resign added successfully.";
            stat = true;
            com.addlog(LogTypes.Updated, UserId, "ProResignRequest", "ProResignRequests", findip(), (long)id, "Task Type Added Successfully");


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = id } };
        }

        [HttpPost]
        [RedirectingAction]

        public JsonResult Create(ProResignRequestviewmodel ttype)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            DateTime sDate = DateTime.Parse(ttype.Resignfromdate.ToString(), new CultureInfo("en-GB"));
            DateTime eDate = DateTime.Parse(ttype.Resigntodate.ToString(), new CultureInfo("en-GB"));


            var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);
            ProResignRequest vm = new ProResignRequest();
                     vm.Createdate = System.DateTime.Now;
                    vm.CreatedBy = db.Employees.Where(o=>o.EmployeeId==ttype.SECashier).Select(o=>o.UserId).FirstOrDefault();
            vm.Resignfromdate = sDate;
            vm.Resigntodate = eDate;
            vm.approveddate = System.DateTime.Now;
            vm.Status = ApprovalStatus.PendingApproval;
            TimeSpan? stime = null;
            if (ttype.Resignfromtime != null)
            {
                stime = ((DateTime)ttype.Resignfromtime).TimeOfDay;
            }
            else
            {
                stime = ((DateTime)sDate).TimeOfDay;
            
        }

            DateTime? stimes = sDate + stime;
            vm.Resignfromtime = stimes;
            TimeSpan? etime = null;
            if (ttype.Resigntotime != null)
            {
                etime = ((DateTime)ttype.Resigntotime).TimeOfDay;
            }
            else
            {
                etime = ((DateTime)eDate).TimeOfDay;
            }
            DateTime? etimes = eDate + etime;
            vm.Resigntotime = etimes;
            vm.Resignreason = ttype.Resignreason;
            vm.Status = ApprovalStatus.PendingApproval;
            vm.ResignType = ttype.ResignType;


            db.ProResignRequests.Add(vm);
            db.SaveChanges();
            long Resignid = vm.ResignRequestId;
            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/Resigndocument/");
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
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";

                        }
                        var userid = User.Identity.GetUserId();
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), newName);
                        file.SaveAs(newName);

                        var FilemultipleDocument = new FilemultipleDocuments
                        {
                            Document = newSName,
                            RelationID = Resignid,
                            DocumentName = "Resigndocument",
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
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Resigndocument/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }

            }


            db.SaveChanges();
                 
                    msg = "Resign added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "ProResignRequest", "ProResignRequests", findip(), Id, "Task Type Added Successfully");
                
        
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [RedirectingAction]

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProResignRequest st = db.ProResignRequests.Find(id);

            

            ViewBag.image = (from b in db.MultipleDocuments
                             join c in db.ProResignRequests on b.RelationID equals c.ResignRequestId
                             where c.ResignRequestId == id && b.DocumentName== "Resigndocument"
                             select new TaskImageViewModel
                             {
                                 TaskImageId = b.Id,
                                 TaskId = id,
                                 FileName = b.Document ,
                                 TaskName = b.DocumentName,
                             }).ToList();
            var userid = User.Identity.GetUserId();
            ViewBag.salesman = "true";
            if (User.IsInRole("ResignationList"))
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
                    Text = "Resign", Value = "0"
                },
                new SelectListItem {
                    Text = "Medical Resign", Value = "1"
                }
                ,
                new SelectListItem {
                    Text = "Day off", Value = "2"
                }
                  ,
                new SelectListItem {
                    Text = "Annual Resign", Value = "3"
                }
                  ,
                new SelectListItem {
                    Text = "Emergecy Resign", Value = "4"
                }
              };
            ProResignRequestviewmodel sta = new ProResignRequestviewmodel();
            sta.Resignfromdate = st.Resignfromdate.ToString("dd-MM-yyy");
            sta.Resigntodate = st.Resigntodate.ToString("dd-MM-yyy");
            sta.SECashier = db.Employees.Where(o => o.UserId == st.CreatedBy).Select(o => o.EmployeeId).FirstOrDefault();
            sta.ResignType = st.ResignType;
            sta.Resignreason = st.Resignreason;
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


        //        serialisedJson = db.ProResignRequests.Where(p => p.TypeName.ToLower().Contains(q.ToLower()) || p.TypeName.Contains(q))
        //                          .Select(b => new SelectFormat
        //                              text = b.TypeName, //each json object will have 
        //                              id = b.TaskTypeId
        //                          })
        //        serialisedJson = db.ProResignRequests.Select(b => new SelectFormat
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
