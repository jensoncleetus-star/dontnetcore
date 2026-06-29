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
using System.Net.Http.Headers;
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
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class FileDocumentController : BaseController
    {

        ApplicationDbContext db;
        Common com;
        public FileDocumentController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }






        public ActionResult documentexpiry()
        {
            ViewBag.Sect = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Employee", Value="Employee"},
                new SelectListItem() {Text = "General", Value="General"},


            }, "Value", "Text");
            return View();
        }
        [HttpPost]
        public ActionResult Getdocumentexpiry(string section, string date)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            DateTime? fdate = null;
            if (!string.IsNullOrEmpty(date))
            {
                fdate = DateTime.Parse(date, new CultureInfo("en-GB"));
            }
            CultureInfo culture = new CultureInfo("en-US");
            if (section == "Employee")
            {
                var v = (from a in db.EmployeeDocuments
                         join b in db.Employees on a.EmployeeId equals b.EmployeeId

                         where
                          (date == "" || EF.Functions.DateDiffDay(a.ExpiryDate, fdate) >= 0)
                         select new
                         {
                             Id = a.EmployeeDocumentId,
                             Date = a.ExpiryDate,
                             Name = b.FirstName + " " + b.MiddleName + " " + b.LastName,
                             DocumentType = a.DocumentName,
                             empid = b.EmployeeId,
                             doctype = "emp",

                         });
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }


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
            else if (section == "General")
            {
                var v = (from a in db.FileDocuments
                         join t in db.DocumentTypes on a.Documenttype equals t.ID into type
                         from t in type.DefaultIfEmpty()
                         join c in db.Users on a.CreatedBy equals c.Id into utype
                         from c in utype.DefaultIfEmpty()

                         where
                          (date == "" || EF.Functions.DateDiffDay(a.ExpiryDate, fdate) >= 0)

                         select new
                         {

                             Id = a.Id,
                             Date = a.ExpiryDate,
                             Name = a.DocumentName,
                             DocumentType = t.Name,
                             empid = a.Id,
                             doctype = "doc",
                         });
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }


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
            else
                return null;

        }







        public ActionResult AddAmcRemark(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            FileDocument cus = db.FileDocuments.Find(id);

            if (cus == null)
            {
                return NotFound();
            }
            var AmcRemarks = new AddedRemarksvm
            {
                TransactionId = cus.Id,
                TransactionType = "CustomerRemark"


            };

            return PartialView(AmcRemarks);
        }
        [HttpPost]
        public JsonResult AddRemark(AddedRemarksvm amcremarks)
        {
            Int64 amcid = amcremarks.TransactionId;

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
                        TransactionType = "filedocRemark",
                        Remarks = amcremarks.Remarks,
                        AddedUser = UserId,
                        CreatedDate = Today,
                        nextdate = Today,
                        nexttime = Today,
                    };
                    db.AddedRemarks.Add(Obj);
                    db.SaveChanges();






                    //To Update Status and LogTime in Amc Table
                    FileDocument AmcObj = db.FileDocuments.Find(amcid);


                    AmcObj.LogTime = Today;

                    db.Entry(AmcObj).State = EntityState.Modified;
                    db.SaveChanges();


                    com.addlog(LogTypes.Created, UserId, "filedocRemark", "AddedRemarks", findip(), amcid, "AMC Remarks Added Successfully..");
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

            var v = (from a in db.AddedRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     where a.TransactionId == RequisitionId && a.TransactionType == "filedocRemark" && a.Remarks != null
                     orderby a.CreatedDate descending

                     select new
                     {
                         id = a.RemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remarks,
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = v.Count();
            var data = v.ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        public ActionResult Index()
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
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            var items = db.FileDocuments.Select(s => new
            {
                ID = s.Id,
                Name = s.DocumentName
            }).ToList();
            ViewBag.Item = QkSelect.List(items, "ID", "Name");
            var docs = db.FileDocuments.Select(s => new
            {
                ID = s.Id,
                Name = s.Document
            }).ToList();
            ViewBag.Document = QkSelect.List(docs, "ID", "Name");

            var typ = db.DocumentTypes.Select(s => new
            {
                ID = s.ID,
                Name = s.Name
            }).ToList();
            ViewBag.typ = QkSelect.List(typ, "ID", "Name");

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");

            return View();
        }
        public ActionResult myfiles()
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
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            var items = db.FileDocuments.Select(s => new
            {
                ID = s.Id,
                Name = s.DocumentName
            }).ToList();
            ViewBag.Item = QkSelect.List(items, "ID", "Name");
            var docs = db.FileDocuments.Select(s => new
            {
                ID = s.Id,
                Name = s.Document
            }).ToList();
            ViewBag.Document = QkSelect.List(docs, "ID", "Name");

            var typ = db.DocumentTypes.Select(s => new
            {
                ID = s.ID,
                Name = s.Name
            }).ToList();
            ViewBag.typ = QkSelect.List(typ, "ID", "Name");

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");

            return View();
        }
        public ActionResult Assigndocument(long id)
        {
            ViewBag.docid = id;
            List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
            ViewBag.OpnCls = pstat2;
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            var items = db.FileDocuments.Select(s => new
            {
                ID = s.Id,
                Name = s.DocumentName
            }).ToList();
            ViewBag.Item = QkSelect.List(items, "ID", "Name");
            var docs = db.FileDocuments.Select(s => new
            {
                ID = s.Id,
                Name = s.Document
            }).ToList();
            ViewBag.Document = QkSelect.List(docs, "ID", "Name");

            var typ = db.DocumentTypes.Select(s => new
            {
                ID = s.ID,
                Name = s.Name
            }).ToList();
            ViewBag.typ = QkSelect.List(typ, "ID", "Name");

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");

            return View();
        }

        // GET: FileDocument
        public ActionResult googlemyIndex()
        {


            //
            var use = QkSelect.List(new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                             }, "Value", "Text", 0);





            ViewBag.Cashier = use;// QkSelect.List(use, "Value", "Text");
            return View();
        }
        public ActionResult googleIndex()
        {


            //
            var use = QkSelect.List(new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                             }, "Value", "Text");





            ViewBag.Cashier = use;// QkSelect.List(use, "Value", "Text");
            return View();
        }
        [QkAuthorize(Roles = "Dev,FileDocument List")]
        public ActionResult GetFileDocuments(long? openclose, string documentName, string documenttype, string document, string reminder, string expdate, string credate, string status, string user)
        {
            DateTime? exdate = null;
            DateTime? crdate = null;
            DateTime? remdate = null;

            if (expdate != "")
            {
                exdate = DateTime.Parse(expdate, new CultureInfo("en-GB"));
            }
            if (credate != "")
            {
                crdate = DateTime.Parse(credate, new CultureInfo("en-GB"));
            }
            if (reminder != "")
            {
                remdate = DateTime.Parse(reminder, new CultureInfo("en-GB"));
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
            Status st = new Status();
            if (status != "")
            {
                st = (status == "0") ? Status.active : Status.inactive;
            };

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit FileDocument");
            var uDelete = User.IsInRole("Delete FileDocument");
            DateTime cur = DateTime.Now;
            var v = (from a in db.FileDocuments
                     join t in db.DocumentTypes on a.Documenttype equals t.ID into type
                     from t in type.DefaultIfEmpty()
                     join c in db.Users on a.CreatedBy equals c.Id into utype
                     from c in utype.DefaultIfEmpty()

                     where (documentName == null || documentName == "" || a.DocumentName == documentName)
                           && (status == null || status == "" || a.Status == st)
                           && (expdate == null || expdate == "" || EF.Functions.DateDiffDay(a.ExpiryDate, exdate) == 0)
                           && (credate == null || credate == "" || EF.Functions.DateDiffDay(a.CreatedDate, crdate) == 0)
                           && (reminder == null || reminder == "" || EF.Functions.DateDiffDay(a.ReminderDate, remdate) == 0)
                           && (user == null || user == "" || c.Id == user)
                           && (documenttype == null || documenttype == "" || t.ID.ToString() == documenttype)

                           && (openclose == null || a.openclose == openclose)
                     select new
                     {

                         a.Id,
                         a.DocumentName,
                         documenttype = t.Name,
                         a.Note,
                         a.ExpiryDate,
                         a.ReminderDate,
                         a.CreatedDate,
                         color = (a.ReminderDate < cur && a.ExpiryDate > cur) ? 2 : (a.ExpiryDate < cur) ? 3 : 1,
                         user = c.UserName,
                         a.Branch,
                         a.Status,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         ldate = ((a != null) && (a.CreatedDate > a.LogTime)) ? a.CreatedDate : a.LogTime,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.DocumentName.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.documenttype.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.Note.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.ReminderDate.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.CreatedDate.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.user.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.ExpiryDate.ToString().ToLower().Contains(search.ToLower())
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
        public ActionResult GetmyFileDocuments(long? openclose, string documentName, string documenttype, string document, string reminder, string expdate, string credate, string status, string user)
        {
            DateTime? exdate = null;
            DateTime? crdate = null;
            DateTime? remdate = null;

            if (expdate != "")
            {
                exdate = DateTime.Parse(expdate, new CultureInfo("en-GB"));
            }
            if (credate != "")
            {
                crdate = DateTime.Parse(credate, new CultureInfo("en-GB"));
            }
            if (reminder != "")
            {
                remdate = DateTime.Parse(reminder, new CultureInfo("en-GB"));
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
            Status st = new Status();
            if (status != "")
            {
                st = (status == "0") ? Status.active : Status.inactive;
            };
           
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit FileDocument");
            var uDelete = User.IsInRole("Delete FileDocument");
            DateTime cur = DateTime.Now;
            var v = (from a in db.FileDocuments
                     join t in db.DocumentTypes on a.Documenttype equals t.ID into type
                     from t in type.DefaultIfEmpty()
                     join c in db.Users on a.CreatedBy equals c.Id into utype
                     from c in utype.DefaultIfEmpty()
                     join d in db.assigncommons on a.Id equals d.parentid
                     where (documentName == null || documentName == "" || a.DocumentName == documentName)
                           && (status == null || status == "" || a.Status == st)
                           && (expdate == null || expdate == "" || EF.Functions.DateDiffDay(a.ExpiryDate, exdate) == 0)
                           && (credate == null || credate == "" || EF.Functions.DateDiffDay(a.CreatedDate, crdate) == 0)
                           && (reminder == null || reminder == "" || EF.Functions.DateDiffDay(a.ReminderDate, remdate) == 0)
                           && (user == null || user == "" || c.Id == user)
                           && (documenttype == null || documenttype == "" || t.ID.ToString() == documenttype)
                           && (d.employeeid==empId && d.type== "filedocument")
                           && (openclose == null || a.openclose == openclose)
                     select new
                     {

                         a.Id,
                         a.DocumentName,
                         documenttype = t.Name,
                         a.Note,
                         a.ExpiryDate,
                         a.ReminderDate,
                         a.CreatedDate,
                         color = (a.ReminderDate < cur && a.ExpiryDate > cur) ? 2 : (a.ExpiryDate < cur) ? 3 : 1,
                         user = c.UserName,
                         a.Branch,
                         a.Status,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         ldate = ((a != null) && (a.CreatedDate > a.LogTime)) ? a.CreatedDate : a.LogTime,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.DocumentName.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.documenttype.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.Note.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.ReminderDate.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.CreatedDate.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.user.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.ExpiryDate.ToString().ToLower().Contains(search.ToLower())
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

        public ActionResult GetFileDocumentsgoogle(long[] emp, string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            long[] em = new long[] { };
            if (emp != null)
                em = emp.ToArray();
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
            var uEdit = User.IsInRole("Edit FileDocument");
            var uDelete = User.IsInRole("Delete FileDocument");
            var v = (from a in db.googlereviews
                     join b in db.Users on a.createedby equals b.Id
                     join c in db.Employees on a.QuotCashier equals c.EmployeeId
                     where (em.Count()== 0|| em.Contains(a.QuotCashier))
                     &&
                       (fromdate == "" || (a.QuotDate != null && EF.Functions.DateDiffDay(a.QuotDate, fdate) <= 0)) &&
                  (todate == "" || (a.QuotDate != null && EF.Functions.DateDiffDay(a.QuotDate, tdate) >= 0))


                     select new
                     {

                         Id = a.googlereviewid,
                         Employee = c.FirstName + " " + c.MiddleName + " " + c.LastName,


                         CreatedDate = a.createddate,
                         user = b.UserName,
                         reviewdate = a.QuotDate,

                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.Employee.ToString().ToLower().Contains(search.ToLower())
                                  );

            }
            //SORT


            v = v.OrderByDescending(o => o.reviewdate);

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        public ActionResult dashboard()
        {
            return View();
        }
        public ActionResult Getreviewdashboard(long? emp, string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;

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
            var uEdit = User.IsInRole("Edit FileDocument");
            var uDelete = User.IsInRole("Delete FileDocument");
            var v = (from a in db.googlereviews
                     join b in db.Users on a.createedby equals b.Id
                     join c in db.Employees on a.QuotCashier equals c.EmployeeId

                     where (fromdate == "" || (a.QuotDate != null && EF.Functions.DateDiffDay(a.QuotDate, fdate) <= 0)) &&
                               (todate == "" || (a.QuotDate != null && EF.Functions.DateDiffDay(a.QuotDate, tdate) >= 0))
                     group new { c.FirstName, c.MiddleName, c.LastName } by c.EmployeeId into gprs
                     select new
                     {
                         empid = gprs.Key,
                         employeename = gprs.Select(o => o.FirstName + " " + o.MiddleName + " " + o.LastName).FirstOrDefault(),
                         counts = gprs.Count()
                     }
                     );

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.employeename.ToString().ToLower().Contains(search.ToLower())
                                  );

            }
            //SORT


            v = v.OrderByDescending(o => o.counts);

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        public ActionResult GetmyFileDocumentsgoogle(string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;

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
            var uEdit = User.IsInRole("Edit FileDocument");
            var uDelete = User.IsInRole("Delete FileDocument");
            var v = (from a in db.googlereviews
                     join b in db.Users on a.createedby equals b.Id
                     join c in db.Employees on a.QuotCashier equals c.EmployeeId
                     where b.Id == UserId
                     &&
                       (fromdate == "" || (a.QuotDate != null && EF.Functions.DateDiffDay(a.QuotDate, fdate) <= 0)) &&
                  (todate == "" || (a.QuotDate != null && EF.Functions.DateDiffDay(a.QuotDate, tdate) >= 0))
                     select new
                     {

                         Id = a.googlereviewid,
                         Employee = c.FirstName + " " + c.MiddleName + " " + c.LastName,


                         CreatedDate = a.createddate,
                         user = b.UserName,
                         reviewdate = a.QuotDate,

                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.ToString().ToLower().Contains(search.ToLower()) ||
                                  p.Employee.ToString().ToLower().Contains(search.ToLower())
                                  );

            }
            //SORT


            v = v.OrderByDescending(o => o.reviewdate);

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create FileDocument")]
        public ActionResult Create()
        {
            List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
            List<SelectFormat> serialisedJson;
            serialisedJson = db.Employees
                   .Select(s => new SelectFormat
                   {
                       id = s.EmployeeId,
                       text = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            serialisedJson.Insert(0, initial);

            ViewBag.team = new MultiSelectList(serialisedJson, "id", "text");

            ViewBag.OpnCls = pstat2;
            ViewBag.LastEntry = db.FileDocuments.Select(p => p.Id).AsEnumerable().DefaultIfEmpty(0).Max();
            return View();
        }
       
          [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit Lock")]
        public ActionResult CreateLock()
        {
            ViewBag.Fields = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Sales", Value="Sales"},

     
                new SelectListItem() {Text = "Purchase", Value="Purchase"},
       
                new SelectListItem() {Text = "Quotation", Value="Quot"},



                new SelectListItem() {Text = "Payment", Value="Payment"},
                new SelectListItem() {Text = "Receipt", Value="Receipt"},
                new SelectListItem() {Text = "Journal", Value="Journal"},


            }, "Value", "Text");


            return View();

        }
        [HttpPost]
        public ActionResult CreateDocumentReminder(long days, long employeeid)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (1==1)
            {

                var UserId = User.Identity.GetUserId();


                if(1==1)
                {

                    var fm = db.DocExpiryReminders.Any(o => o.EmployeeID == employeeid);
                    if (fm)
                    {
                        db.DocExpiryReminders.RemoveRange(db.DocExpiryReminders.Where(o => o.EmployeeID == employeeid));
                        db.SaveChanges();
                    }
                    if (1==1)
                    {
                        DocExpiryReminder a = new DocExpiryReminder();
                        a.days = days;
                   
                        a.EmployeeID = employeeid;

                        db.DocExpiryReminders.Add(a);
                    }
                    db.SaveChanges();
                }

                msg = "Reminder Days added successfully.";
                stat = true;
                com.addlog(LogTypes.Created, UserId, "Reminder Days", "Rimnder Days Date", findip(), Id, "FieldMapping Successfully");

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpGet]
        public JsonResult getdocumentexpiryreminderset(long Name)
        {
            var ConD = (from a in db.DocExpiryReminders
                        where a.EmployeeID == Name
                        select new
                        {
                            days = a.days,
                          
                        }).ToList();
            return Json(ConD);
        }
        public ActionResult documentexpiryreminderset()
        {
            ViewBag.Fields = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = null, Value = null},
                      }, "Value", "Text", 1);


            return View();

        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create Google Review")]

        public ActionResult Creategooglereivew()
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
            ////QkSelect.List(




            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            googlereviewmodal vmodal = new googlereviewmodal();
            vmodal.QuotDate = System.DateTime.Now.Date.ToString("dd-MM-yyyy");
            return View(vmodal);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Google Review")]
        public ActionResult Creategooglereviewsave(googlereviewmodal fdoc)
        {

            IFormFileCollection files = Request.Form.Files;
            DateTime dt = DateTime.Parse(fdoc.QuotDate, new CultureInfo("en-GB"));
            var usr = User.Identity.GetUserId();
            if (files.Count > 0)
            {
                googlereview n = new googlereview
                {
                    QuotCashier = fdoc.QuotCashier,
                    QuotDate = dt,
                    createddate = System.DateTime.Now,
                    createedby = usr

                };
                db.googlereviews.Add(n);
                db.SaveChanges();
                var reviewid = n.googlereviewid;
                string path = LegacyWeb.MapPath("~/uploads/GoogleReivew/");
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
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/GoogleReivew/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/GoogleReivew/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";

                        }
                        var userid = User.Identity.GetUserId();
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/GoogleReivew/"), newName);
                        file.SaveAs(newName);

                        var FilemultipleDocument = new FilemultipleDocuments
                        {
                            Document = newSName,
                            RelationID = reviewid,
                            DocumentName = "Review",
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
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/GoogleReivew/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/GoogleReivew/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }
            }
            string msg = "Successfully Uploaded";
            bool stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create FileDocument")]
        public ActionResult Create(FileDocumentViewModel fdoc)
        {
            bool stat = false;
            string msg;
            Int64 fileid = 0;
            var UserId = User.Identity.GetUserId();
            var today = System.DateTime.Now;
            ViewBag.DocList = db.DocumentTypes.ToList();
            long RId = 0;
            if (fdoc.Lstfdocs != null && fdoc.Lstfdocs.Count > 0)
            {
                var fileNames = "";
              
                foreach (var item in fdoc.Lstfdocs)
                {
                    if (item.Document != null)
                    {

                    }
                    else
                    {
                        fileNames = item.DocumentName;
                    }


                    var FileDocuments = new FileDocument
                    {
                        Document = fileNames,
                        DocumentName = item.DocumentName,
                        Documenttype = item.Documenttype,
                        CreatedBy = UserId,
                        Note = item.Note,
                        CreatedDate = today,
                        Branch = item.Branch,
                        Status = item.Status,
                        ExpiryDate = item.ExpiryDate,
                        ReminderDate = item.ReminderDate,
                        openclose = 0,
                       reminderrepeate= fdoc.reminderrepeate,
                        //CreatedBy = UserId
                    };
                    db.FileDocuments.Add(FileDocuments);
                    db.SaveChanges();
                    fileid = FileDocuments.Id;
                    var reminds = new Reminder
                    {

                        Note = item.DocumentName + " Expired On " + item.ExpiryDate,
                        RDate = item.ReminderDate,
                        CreatedDate = System.DateTime.Now,
                        Reference = fileid,

                        Type = "/FileDocument/myfiles",


                        RequestBy = UserId,

                        CreatedBy = UserId,
                        Status = Status.active,


                        RStatus = "Close",

                    };
                    db.Reminders.Add(reminds);
                    db.SaveChanges();
                     RId = reminds.ReminderId;
                
                
                    //............ new fileupload jquery design  ..............
                    #region file http pulling method  using jquery

                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/FileDocuments/");
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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), newName);
                                file.SaveAs(newName);

                                var FilemultipleDocument = new FilemultipleDocuments
                                {
                                    Document = newSName,
                                    RelationID = fileid,
                                    DocumentName = item.DocumentName,
                                    CreatedBy = UserId,
                                    Note = item.Note,
                                    CreatedDate = today,
                                    Status = item.Status,
                                    ExpiryDate = item.ExpiryDate
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }
                }

            }
           
            #endregion
            if (fdoc.AssignedMembers != null)
            {
                foreach (var k in fdoc.AssignedMembers)
                {
                    assigncommon n = new assigncommon
                    {
                        employeeid = k,
                        parentid = fileid,
                        type="filedocument"
                    };
                    db.assigncommons.Add(n);
                    db.SaveChanges();
                    ReminderAssigned remAs = new ReminderAssigned();

                    remAs.ReminderId = RId;
                    remAs.EntryId = fileid;
                    remAs.Type = "filenotifications";
                    remAs.EmployeeId = k;
                    db.ReminderAssigneds.Add(remAs);
                    db.SaveChanges();
                }
            }
            com.addlog(LogTypes.Created, UserId, "FileDocument", "FileDocuments", findip(), (long)fileid, "Document  Created Successfully");
            msg = "Successfully Uploaded";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit FileDocument")]
        public ActionResult Edit(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
            ViewBag.OpnCls = pstat2;
            FileDocument docInfo = db.FileDocuments.Find(Id);


            FileDocumentViewModel FD = new FileDocumentViewModel();
            FD.DocumentName = docInfo.DocumentName;
            FD.Document = docInfo.Document;
            FD.Documentview = docInfo.Document;
            FD.CreatedDate = docInfo.CreatedDate;
            ViewBag.doctp = docInfo.Documenttype;
            FD.ExpiryDate = docInfo.ExpiryDate;
            FD.ReminderDate = docInfo.ReminderDate;
            FD.openclose = docInfo.openclose;
            FD.Note = docInfo.Note;
            FD.Status = docInfo.Status;
            FD.reminderrepeate = docInfo.reminderrepeate;
            if (docInfo == null)
            {
                return NotFound();
            }
            var max = docInfo.ExpiryDate != null ? docInfo.ExpiryDate.Value.ToString("MM-dd-yyyy") : "";
            FD.lstrealdoc = (from cd in db.FileDocuments
                             where Id == cd.Id
                             select new Realfdoc
                             {

                                 DocumentName = cd.DocumentName,
                                 Documenttype = cd.Documenttype,
                                 ExpiryDate = max,
                                 Note = cd.Note,

                             }
                                         ).ToList();
            ViewBag.DocList = db.DocumentTypes.ToList();

            ViewBag.image = (from m in db.MultipleDocuments
                             where Id == m.RelationID
                             select new Multiviewmodel
                             {

                                 Id = m.Id,
                                 Document = m.Document,
                                 filenamelead = m.Document,
                                 DocumentName = m.DocumentName


                             }
                                       ).ToList();
            ViewBag.preEntry = db.FileDocuments.Where(a => a.Id < Id).Select(a => a.Id).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.FileDocuments.Where(a => a.Id > Id).Select(a => a.Id).DefaultIfEmpty().Min();
            var assmebers = db.assigncommons.Where(o => o.parentid == Id).Select(o => o.employeeid).Distinct().ToArray() ?? null;
            List<SelectFormat> serialisedJson;
            serialisedJson = db.Employees
                   .Select(s => new SelectFormat
                   {
                       id = s.EmployeeId,
                       text = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            serialisedJson.Insert(0, initial);
            ViewBag.team = new MultiSelectList(serialisedJson, "id", "text", assmebers);

            return View(FD);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit FileDocument")]
        public ActionResult Edit(long Id, FileDocumentViewModel fdoc)
        {

            bool stat = false;
            string msg;
            long rid = 0;
            var UserId = User.Identity.GetUserId();


            FileDocument doc = db.FileDocuments.Find(Id);

            doc.DocumentName = fdoc.DocumentName;
            doc.ExpiryDate = fdoc.ExpiryDate;
            doc.ReminderDate = fdoc.ReminderDate;
            doc.Note = fdoc.Note;
            doc.Documenttype = fdoc.Documenttype;
            doc.Status = fdoc.Status;
            doc.openclose = fdoc.openclose;
            doc.LogTime = System.DateTime.Now;
            doc.reminderrepeate = fdoc.reminderrepeate;
            db.Entry(doc).State = EntityState.Modified;
            db.SaveChanges();

            db.Reminders.RemoveRange(db.Reminders.Where(a => a.Reference == Id && a.Note.Contains(" Expired On ")));
            db.SaveChanges();
            var reminds = new Reminder
            {
                Note = fdoc.DocumentName + " Expired On " + fdoc.ExpiryDate,
                RDate = fdoc.ReminderDate,
                CreatedDate = System.DateTime.Now,
                Reference = Id,

                Type = "/FileDocument/myfiles",


                RequestBy = UserId,

                CreatedBy = UserId,
                Status = Status.active,


                RStatus = "Close",

            };
            db.Reminders.Add(reminds);
            db.SaveChanges();

            var today = System.DateTime.Now;
            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/FileDocuments/");
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
                        var thumbName = "";
                        var resizeName = "";
                        var FStatus = Status.active;
                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                        {
                            thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";

                        }
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), newName);
                        file.SaveAs(newName);

                        var FilemultipleDocument = new FilemultipleDocuments
                        {
                            Document = newSName,
                            RelationID = doc.Id,
                            DocumentName = fdoc.DocumentName,
                            CreatedBy = UserId,
                            Note = fdoc.Note,
                            CreatedDate = today,
                            ExpiryDate = fdoc.ExpiryDate
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
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }
            }
          
             rid = reminds.ReminderId;
            if (fdoc.AssignedMembers != null)
            {
                var assmemb = db.assigncommons.Where(o => o.parentid == Id);
                db.assigncommons.RemoveRange(assmemb);
                db.SaveChanges();
                foreach (var k in fdoc.AssignedMembers)
                {
                    assigncommon n = new assigncommon
                    {
                        employeeid = k,
                        parentid = Id,
                        type = "filedocument"
                    };
                    db.assigncommons.Add(n);
                    db.SaveChanges();
                    ReminderAssigned remAs = new ReminderAssigned();

                    remAs.ReminderId = rid;
                    remAs.EntryId = Id;
                    remAs.Type = "filenotifications";
                    remAs.EmployeeId = k;
                    db.ReminderAssigneds.Add(remAs);
                    db.SaveChanges();
                }
            }
            com.addlog(LogTypes.Created, UserId, "FileDocument", "FileDocuments", findip(), (long)doc.Id, "Document  Edited Successfully");
            stat = true;
            msg = "File Updated Successfully.";
            //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            Success("File Updated Successfully", true);
            return Redirect(ControllerContext.HttpContext.Request.GetUrlReferrer().ToString());
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit FileDocument")]
        public JsonResult ImageDelete(long key)
        {
            bool stat = false;
            string msg;
            FilemultipleDocuments FilemultipleDocument = db.MultipleDocuments.Find(key);
            if (FilemultipleDocument != null)
            {
                db.MultipleDocuments.Remove(FilemultipleDocument);
                db.SaveChanges();
            }
            string fullPath = LegacyWeb.MapPath("~/uploads/FileDocuments/" + FilemultipleDocument.Document);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            string fullPaththumb = LegacyWeb.MapPath("~/uploads/FileDocuments/" + "thumb_" + FilemultipleDocument.Document);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }

            var UserId = User.Identity.GetUserId();

            com.addlog(LogTypes.Deleted, UserId, "FilemultipleDocument", "FilemultipleDocuments", findip(), FilemultipleDocument.Id, "Image Deleted Successfully");


            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted  Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [HttpGet]

        public ActionResult Deletegoogle(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            googlereview docInfo = db.googlereviews.Find(Id);
            if (docInfo == null)
            {
                return NotFound();
            }
            return PartialView(docInfo);
        }

        [HttpPost]

        public JsonResult Deletegoogle(long googlereviewid)
        {
            long Id = googlereviewid;
            googlereview docId = db.googlereviews.Find(googlereviewid);
            bool stat = false;
            string msg;
            if (docId != null)
            {
                foreach (var filesattach in db.MultipleDocuments.Where(d => d.RelationID == Id && d.DocumentName == "Review").ToList())
                {

                    if (filesattach != null)
                    {
                        db.MultipleDocuments.Remove(filesattach);
                        db.SaveChanges();


                        string fullPath = LegacyWeb.MapPath("~/uploads/GoogleReivew/" + filesattach.Document);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        string fullPaththumb = LegacyWeb.MapPath("~/uploads/GoogleReivew/" + "thumb_" + filesattach.Document);
                        if (System.IO.File.Exists(fullPaththumb))
                        {
                            System.IO.File.Delete(fullPaththumb);
                        }
                        string fullPathresize = LegacyWeb.MapPath("~/uploads/GoogleReivew/" + "resize_" + filesattach.Document);
                        if (System.IO.File.Exists(fullPathresize))
                        {
                            System.IO.File.Delete(fullPathresize);
                        }
                    }
                }
                //------------folder delete end-----------



                db.googlereviews.Remove(docId);
                db.SaveChanges();

                //*******************Reminders


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "google review", "google review", findip(), (long)docId.googlereviewid, "Details Deleted Successfully");
            }



            stat = true;
            msg = "FileDocument Deleted Successfully.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }




        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete FileDocument")]
        public ActionResult Delete(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileDocument docInfo = db.FileDocuments.Find(Id);
            if (docInfo == null)
            {
                return NotFound();
            }
            return PartialView(docInfo);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete FileDocument")]
        public JsonResult Delete(long Id)
        {
            FileDocument docId = db.FileDocuments.Find(Id);
            bool stat = false;
            string msg;
            if (docId != null)
            {
                foreach (var filesattach in db.MultipleDocuments.Where(d => d.RelationID == Id).ToList())
                {

                    if (filesattach != null)
                    {
                        db.MultipleDocuments.Remove(filesattach);
                        db.SaveChanges();


                        string fullPath = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + filesattach.Document);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        string fullPaththumb = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + "thumb_" + filesattach.Document);
                        if (System.IO.File.Exists(fullPaththumb))
                        {
                            System.IO.File.Delete(fullPaththumb);
                        }
                        string fullPathresize = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + "resize_" + filesattach.Document);
                        if (System.IO.File.Exists(fullPathresize))
                        {
                            System.IO.File.Delete(fullPathresize);
                        }
                    }
                }
                //------------folder delete end-----------


                var filename = docId.Document;
                var thname = "thumb_" + filename;

                if (filename != null)
                {
                    string storepathname = LegacyWeb.MapPath("~/uploads/FileDocuments/" + filename);
                    string thumbpathname = LegacyWeb.MapPath("~/uploads/FileDocuments/" + thname);

                    if (System.IO.File.Exists(storepathname))
                    {
                        System.IO.File.Delete(storepathname);
                        System.IO.File.Delete(thumbpathname);
                    }
                }
                db.FileDocuments.Remove(docId);
                db.SaveChanges();

                //*******************Reminders
                var Reminds = db.Reminderss.Where(a => a.Reference == Id && a.Type == "FileDocument").FirstOrDefault();

                if (Reminds != null)
                {
                    db.Reminderss.Remove(Reminds);
                    db.SaveChanges();
                }

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "FileDocument", "FileDocuments", findip(), (long)docId.Id, "Details Deleted Successfully");
            }



            stat = true;
            msg = "FileDocument Deleted Successfully.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }





        [QkAuthorize(Roles = "Dev,Download FileDocument")]
        public FileResult DownloadFile(long Id)
        {
            FileDocument docId = db.FileDocuments.Find(Id);
            var fileName = docId.Document;
            string filePath = LegacyWeb.MapPath("~/uploads/FileDocuments/" + fileName);
            return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete FileDocument")]
        public JsonResult FileDelete(long key)
        {
            bool stat = false;
            string msg;
            FileDocument docId = db.FileDocuments.Find(key);

            var filename = docId.Document;
            var thname = "thumb_" + filename;

            if (filename != null)
            {
                string storepathname = LegacyWeb.MapPath("~/uploads/FileDocuments/" + filename);
                string thumbpathname = LegacyWeb.MapPath("~/uploads/FileDocuments/" + thname);

                if (System.IO.File.Exists(storepathname))
                {
                    System.IO.File.Delete(storepathname);
                    System.IO.File.Delete(thumbpathname);
                }
            }
            db.Entry(docId).State = EntityState.Modified;
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Updated, UserId, "FileDocument", "FileDocuments", findip(), (long)docId.Id, "FileDocument Deleted Successfully");

            Int64 Id = key;
            stat = true;
            msg = "FileDocument Deleted Successfully.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [HttpGet]
        public ActionResult GetAttachments(long? fileid)
        {
            //Get the images list from repository
            var attachmentsList = (from b in db.FileDocuments
                                   where b.Id == fileid
                                   select new
                                   {
                                       AttachmentID = b.Id,
                                       FileName = b.Document,
                                       Path = "/uploads/FileDocuments/" + b.Document
                                   }).ToList();

            return Json(new { Data = attachmentsList });
        }

        public bool ThumbnailCallback()
        {
            return false;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create FileDocument")]
        public ActionResult Add()
        {
            ViewBag.LastEntry = db.FileDocuments.Select(p => p.Id).AsEnumerable().DefaultIfEmpty(0).Max();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create FileDocument")]
        public JsonResult Add(FileDocumentViewModel fdoc)
        {

            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var today = System.DateTime.Now;
            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/FileDocuments/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                string dates = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i];
                    if (file.Length > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        String extension = Path.GetExtension(fileName);
                        String newName = Guid.NewGuid().ToString() + extension;
                        if (extension == ".jpg" || extension == ".png" || extension == ".gif" || extension == ".jpeg")
                        {
                            var thumbName = "thumb_" + newName;
                            fileName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), newName);
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), thumbName);
                            file.SaveAs(fileName);

                            using (Image img = Image.FromFile(fileName))
                            {
                                int imgHeight = 100;
                                int imgWidth = 100;
                                if (img.Width < img.Height)
                                {
                                    //portrait image  
                                    imgHeight = 80;
                                    var imgRatio = (float)imgHeight / (float)img.Height;
                                    imgWidth = Convert.ToInt32(img.Height * imgRatio);
                                }
                                else if (img.Height < img.Width)
                                {
                                    //landscape image  
                                    imgWidth = 80;
                                    var imgRatio = (float)imgWidth / (float)img.Width;
                                    imgHeight = Convert.ToInt32(img.Height * imgRatio);
                                }
                                Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                                thumb.Save(thumbName);
                            }

                            var docinfo = new FileDocument
                            {
                                Document = newName,
                                DocumentName = fdoc.DocumentName,
                                ExpiryDate = fdoc.ExpiryDate,
                                Note = fdoc.Note,
                                CreatedDate = today,
                                Branch = fdoc.Branch,
                                Status = fdoc.Status,
                                CreatedBy = UserId
                            };
                            db.FileDocuments.Add(docinfo);
                            db.SaveChanges();
                            com.addlog(LogTypes.Created, UserId, "FileDocument", "FileDocuments", findip(), (long)docinfo.Id, "Document in Image form Details Added Successfully");

                        }
                        else if (extension == ".pdf" || extension == ".doc" || extension == ".docx")
                        {
                            fileName = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), newName);
                            file.SaveAs(fileName);

                            var docinfo = new FileDocument
                            {
                                Document = newName,
                                DocumentName = fdoc.DocumentName,
                                ExpiryDate = fdoc.ExpiryDate,
                                Note = fdoc.Note,
                                CreatedDate = today,
                                Branch = fdoc.Branch,
                                Status = fdoc.Status,
                                CreatedBy = UserId
                            };
                            db.FileDocuments.Add(docinfo);
                            db.SaveChanges();
                            com.addlog(LogTypes.Created, UserId, "FileDocument", "FileDocuments", findip(), (long)docinfo.Id, "Document Added Successfully");
                        }
                    }
                }
            }
            msg = "Successfully Uploaded";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpGet]
        public ActionResult Download(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            FileDocument docdownload = db.FileDocuments.Find(id);
            if (docdownload == null)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.MultipleDocuments
                                           where id == m.RelationID
                                           select new Multiviewmodel
                                           {

                                               Id = m.Id,
                                               Document = m.Document,
                                               filenamelead = m.Document,
                                               DocumentName = m.DocumentName


                                           }
                                        ).ToList();
                ViewBag.document = docdownload.DocumentName;

                return PartialView(filedoc);
            }




        }
        [HttpPost]
        public ActionResult Download(long docid)
        {
            var downdocs = from a in db.MultipleDocuments
                           where a.RelationID == docid
                           select a;
            foreach (var arr in downdocs)
            {
                var extension = ".jpg";
                var idofdoc = arr.Id;
                string path = AppDomain.CurrentDomain.BaseDirectory + "/uploads/FileDocuments/";
                byte[] fileBytes = System.IO.File.ReadAllBytes(path + idofdoc + extension);
                string fileName = idofdoc + extension;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            return View();
        }
        [HttpGet]
        public ActionResult Downloadgooglereview(long empid, string frmdate, string todate)
        {

            DateTime? fdate = null;
            if (frmdate != "")
                fdate = DateTime.Parse(frmdate, new CultureInfo("en-GB"));
            DateTime? tdate = null;
            if (todate != "")
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));

            if (empid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var docdownload = (from a in db.googlereviews
                               where a.QuotCashier == empid &&
                               (frmdate == "" || EF.Functions.DateDiffDay(a.QuotDate, fdate) <= 0) &&
             (todate == "" || EF.Functions.DateDiffDay(a.QuotDate, tdate) >= 0)
                               select a
                                       ).Select(o => o.googlereviewid).ToArray();

            if (1 == 2)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.MultipleDocuments
                                           where docdownload.Contains(m.RelationID) &&
                                           m.DocumentName == "Review"
                                           select new Multiviewmodel
                                           {

                                               Id = m.Id,
                                               Document = m.Document,
                                               filenamelead = m.Document,
                                               DocumentName = m.DocumentName


                                           }
                                        ).ToList();


                return View(filedoc);
            }




        }
        [HttpGet]
        public ActionResult Downloadgoogle(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            googlereview docdownload = db.googlereviews.Find(id);
            if (docdownload == null)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.MultipleDocuments
                                           where id == m.RelationID &&
                                           m.DocumentName == "Review"
                                           select new Multiviewmodel
                                           {

                                               Id = m.Id,
                                               Document = m.Document,
                                               filenamelead = m.Document,
                                               DocumentName = m.DocumentName


                                           }
                                        ).ToList();
                ViewBag.document = "Review " + docdownload.QuotDate.ToString("dd-mm-yyyy");

                return PartialView(filedoc);
            }




        }
        [HttpPost]
        public ActionResult Downloadgoogle(long docid)
        {
            var downdocs = from a in db.MultipleDocuments
                           where a.RelationID == docid
                           && a.DocumentName == "Review"
                           select a;
            foreach (var arr in downdocs)
            {
                var extension = ".jpg";
                var idofdoc = arr.Id;
                string path = AppDomain.CurrentDomain.BaseDirectory + "/uploads/GoogleReivew/";
                byte[] fileBytes = System.IO.File.ReadAllBytes(path + idofdoc + extension);
                string fileName = idofdoc + extension;
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            return View();
        }
    }
}
