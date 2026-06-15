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
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

using Microsoft.AspNetCore.Http;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PipeLineController : BaseController
    {

        ApplicationDbContext db;
        Common com;
        public PipeLineController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [QkAuthorize(Roles = "Dev,PipeLine List")]
        public ActionResult Index()
        {
            ViewBag.LeadName = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.LeadSource = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.CreatedBy = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.Location = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.LeadStatus = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);



            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);




            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));
            ViewBag.TaskStatus = QkSelect.List(Enum.GetValues(typeof(TKUpdateStatus)));
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,PipeLine List")]
        public ActionResult GetAllPipeLine(long? LeadName, long? LeadSource, string CreatedBy, string Location, long? LeadStatus, long? AssignedTo, string LeadLevel, string fromdate, string todate)
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
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            var v = (from a in db.Customers
                     join b in db.Contacts on a.Contact equals b.ContactID into tmp
                     from b in tmp.DefaultIfEmpty()
                     join c in db.SourceOfLeads on a.SourceOfLead equals c.SourceOfLeadId into src
                     from c in src.DefaultIfEmpty()
                     join i in db.Accountss on a.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()

                     let cx = db.CustomerConversions.Where(h => h.CustomerID == a.CustomerID).FirstOrDefault()

                     let x = db.AssignedTos.Where(cl => cl.CustomerID == a.CustomerID && (AssignedTo == null || cl.EmployeeId == AssignedTo)).FirstOrDefault()
                     where a.Type == CRMCustomerType.PipeLine &&

                     (LeadName == 0 || a.CustomerID == LeadName) &&
                     (LeadSource == 0 || a.SourceOfLead == LeadSource) &&
                     (LeadStatus == null || a.LeadStat == LeadStatus) &&
                    (CreatedBy == "0" || cx.CreatedUser == CreatedBy) &&
                     (LeadLevel == "All" || a.LeadLevel == LeadLevel) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(cx.CreatedDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(cx.CreatedDate, tdate) >= 0) &&
                    (Location == "" || a.Location.Contains(Location)) && (AssignedTo == null || x != null)
                     select new
                     {
                         id = a.CustomerID,
                         Name = a.CustomerName,
                         Code = a.CustomerCode,
                         TaxRegNo = i.TRN,
                         a.Location,
                         Address = b.Address + "<br/>" + b.City + " " + b.State + " " + b.Country + "<br/>" + b.Zip,
                         Phone = b.Phone,
                         //Mobile = b.Mobile,
                         Email = b.EmailId,
                         a.CreditLimit,
                         a.CreditPeriod,
                         a.Type,
                         SrcLead = c.SrcName,
                         lead = (from f in db.AssignedTos
                                 join g in db.Employees on f.EmployeeId equals g.EmployeeId into emps
                                 from g in emps.DefaultIfEmpty()
                                 where f.CustomerID == a.CustomerID

                                 select new { emp = g.FirstName }
                                 ).ToList(),
                         mobmodel = (from ac in db.Mobiles
                                     where (ac.Contact == a.Contact)
                                     select new MobileViewModel
                                     {
                                         Num = (ac.Name == "" || ac.Name == null) ? ac.MobileNum : ac.MobileNum + "-" + ac.Name,
                                         Name = ac.Name
                                     }).ToList(),
                     });

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()));
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


        [QkAuthorize(Roles = "Dev,MyPipeline")]
        public ActionResult MyPipeline()
        {
            ViewBag.LeadName = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.LeadSource = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.CreatedBy = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.Location = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.LeadStatus = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);








            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));
            ViewBag.TaskStatus = QkSelect.List(Enum.GetValues(typeof(TKUpdateStatus)));
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,MyPipeline")]
        public ActionResult GetAllMyPipeLine(long? LeadName, long? LeadSource, string CreatedBy, string Location, long? LeadStatus)
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
            var UserId = User.Identity.GetUserId();

            Employee emp = new Employee();
            emp.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var v = (from a in db.Customers
                     join b in db.Contacts on a.Contact equals b.ContactID into tmp
                     from b in tmp.DefaultIfEmpty()
                     join c in db.SourceOfLeads on a.SourceOfLead equals c.SourceOfLeadId into src
                     from c in src.DefaultIfEmpty()
                     join i in db.Accountss on a.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     let f = db.AssignedTos.Where(cl => cl.CustomerID == a.CustomerID).Select(cl => cl.EmployeeId).ToList()
                     let con = db.CustomerConversions.Where(cl => cl.ConvertFrom == "Lead" && cl.CustomerID == a.CustomerID).Select(cl => cl.CreatedUser).FirstOrDefault()
                     where a.Type == CRMCustomerType.PipeLine && f.Contains(emp.EmployeeId) && //f != null && //(u.Id == UserId) &&
                      (LeadName == 0 || a.CustomerID == LeadName) &&
                      (LeadSource == 0 || a.SourceOfLead == LeadSource) &&
                      (LeadStatus == null || a.LeadStat == LeadStatus) &&
                      (CreatedBy == "0" || con == CreatedBy) &&
                      (Location == "" || a.Location.Contains(Location))
                     select new
                     {
                         id = a.CustomerID,
                         Name = a.CustomerName,
                         Code = a.CustomerCode,
                         TaxRegNo = i.TRN,
                         a.Location,
                         Address = b.Address + "<br/>" + b.City + " " + b.State + " " + b.Country + "<br/>" + b.Zip,
                         Phone = b.Phone,
                         //Mobile = b.Mobile,
                         Email = b.EmailId,
                         a.CreditLimit,
                         a.CreditPeriod,
                         SrcLead = c.SrcName,
                         //lead = (from f in db.AssignedTos
                         //        where f.CustomerID == a.CustomerID

                         //        ).ToList()
                         mobmodel = (from ac in db.Mobiles
                                     where (ac.Contact == a.Contact)
                                     select new MobileViewModel
                                     {
                                         Num = ac.MobileNum,
                                         Name = ac.Name
                                     }).ToList(),
                     });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()));
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

        // GET: /CRUD/create/5  
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create PipeLine")]
        public ActionResult Create(long? id)
        {
            PipeLineViewModel cusmodel = new PipeLineViewModel();
            if (id != null)
            {

                Customer cus = db.Customers.Find(id);
                if (cus == null)
                {
                    return NotFound();
                }
                Contact cont = db.Contacts.Find(cus.Contact);
                Accounts acc = db.Accountss.Find(cus.Accounts);
                cusmodel.CustomerID = id;
                cusmodel.CustomerName = cus.CustomerName;
                cusmodel.CustomerCode = cus.CustomerCode;
                cusmodel.CreditLimit = cus.CreditLimit;
                cusmodel.CreditPeriod = cus.CreditPeriod;
                cusmodel.Remark = cus.Remark;
                cusmodel.SalesPerson = cus.SalesPerson;
                cusmodel.Location = cus.Location;

                cusmodel.TaxRegNo = acc != null ? acc.TRN : null;

                cusmodel.SourceOfLead = cus.SourceOfLead;

                cusmodel.Address = cont.Address;
                cusmodel.City = cont.City;
                cusmodel.State = cont.State;
                cusmodel.Country = cont.Country;
                cusmodel.Zip = cont.Zip;
                cusmodel.Phone = cont.Phone;
                cusmodel.Fax = cont.Fax;
                cusmodel.EmailId = cont.EmailId;
                cusmodel.Reference = cont.Reference;
                cusmodel.ContactPerson = cont.ContactPerson;

                cusmodel.EmailId = cont.EmailId;
                cusmodel.Reference = cont.Reference;
                cusmodel.ContactPerson = cont.ContactPerson;


                cusmodel.BankName = cus.BankName;
                cusmodel.AccountNo = cus.AccountNo;
                cusmodel.IbanNo = cus.IbanNo;
                cusmodel.BranchName = cus.BranchName;
                cusmodel.Swift = cus.Swift;
                cusmodel.LeadStat = cus.LeadStat;
                cusmodel.LeadLevel = cus.LeadLevel;



                cusmodel.ConvertFrom = "Lead";


            }
            else
            {
                cusmodel.CustomerCode = CustCode();
            }

            ViewBag.CustName = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                           }, "Value", "Text", 1);

            ViewBag.SrcOfLead = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = true, Text = "Select Source Of Lead", Value = "0"},
                         }, "Value", "Text", 1);

            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;
            var use = db.Employees
                          .Select(s => new
                          {
                              ID = s.EmployeeId,
                              Name = s.FirstName + " " + s.LastName
                          })
                          .ToList();
            ViewBag.users = QkSelect.List(use, "ID", "Name");

            var assign = db.Employees.Select(s => new { ID = s.EmployeeId, Name = s.FirstName + " " + s.LastName }).ToList();
            ViewBag.AssignedTo = QkSelect.List(assign, "ID", "Name");


            ViewBag.LeadStat = QkSelect.List(
                      new List<SelectListItem>
                      {
                                    new SelectListItem { Selected = true, Text = "Select Lead Status", Value = "0"},
                      }, "Value", "Text", 1);

            var leadstat = db.LeadStatuss.Select(r => new
            {
                ID = r.LeadStatusID,
                Name = r.StatusType
            }).ToList();
            ViewBag.LeadStat = QkSelect.List(leadstat, "ID", "Name");

            ViewBag.LastEntry = db.Customers.Where(p => p.Type == CRMCustomerType.PipeLine).Select(p => p.CustomerID).AsEnumerable().DefaultIfEmpty(0).Max();

            var cusimg = db.LeadDocuments.Where(a => a.Status == Status.inactive).ToList();
            string path = LegacyWeb.MapPath("~/uploads/leaddocuments/");
            foreach (var arr in cusimg)
            {

                try
                {
                    string[] splitlist = arr.FileName.Split('_');
                    if (splitlist.Length > 1)
                    {
                        string newpath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + splitlist[1]);
                        string filepath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + arr.FileName);
                        if (System.IO.File.Exists(newpath) && System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(Path.Combine(path, splitlist[1]));
                        }
                    }
                    LeadDocument doc = db.LeadDocuments.Find(arr.LeadDocumentId);
                    doc.Status = Status.active;
                    db.Entry(doc).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch
                {
                    Exception ex;
                }
            }

            return View(cusmodel);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create PipeLine")]
        public ActionResult Create(PipeLineSubmitViewModel vmodel)
        {
            if (ModelState.IsValid)
            {
                var Exists = db.Customers.Any(c => c.CustomerCode == vmodel.CustomerCode && c.Type == CRMCustomerType.PipeLine);
                if (Exists)
                {
                    Danger("PipeLine with same PipeLine code exists.", true);
                    return RedirectToAction("Create", "PipeLine");
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    Int64 contactId = 0;
                    Int64 custId = 0;
                    if (vmodel.CustomerID != null)
                    {

                        Customer cus = db.Customers.Find(vmodel.CustomerID);
                        cus.CustomerName = vmodel.CustomerName;
                        cus.CustomerCode = vmodel.CustomerCode;
                        cus.CreditLimit = vmodel.CreditLimit != null ? (decimal)vmodel.CreditLimit : 0;
                        cus.CreditPeriod = vmodel.CreditPeriod != null ? (int)vmodel.CreditPeriod : 0;
                        cus.Remark = vmodel.Remark;
                        cus.SalesPerson = vmodel.SalesPerson;

                        cus.Location = vmodel.Location;
                        cus.SourceOfLead = vmodel.SourceOfLead;
                        cus.Type = CRMCustomerType.PipeLine;

                        cus.BankName = vmodel.BankName;
                        cus.AccountNo = vmodel.AccountNo;
                        cus.BranchName = vmodel.BranchName;
                        cus.IbanNo = vmodel.IbanNo;
                        cus.Swift = vmodel.Swift;
                        cus.LeadStat = vmodel.LeadStat;
                        cus.LeadLevel = vmodel.LeadLevel;

                        db.Entry(cus).State = EntityState.Modified;
                        db.SaveChanges();
                        custId = cus.CustomerID;

                        Contact cont = db.Contacts.Find(cus.Contact);

                        cont.Name = vmodel.CustomerName;
                        cont.Address = vmodel.Address;
                        cont.City = vmodel.City;
                        cont.State = vmodel.State;
                        cont.Country = vmodel.Country;
                        cont.Zip = vmodel.Zip;
                        cont.Phone = vmodel.Phone;
                        cont.Fax = vmodel.Fax;
                        cont.EmailId = vmodel.EmailId;
                        cont.Reference = vmodel.Reference;
                        cont.ContactPerson = vmodel.ContactPerson;


                        db.Entry(cont).State = EntityState.Modified;
                        db.SaveChanges();
                        if (vmodel.mobmodel != null)
                        {
                            foreach (var arr in vmodel.mobmodel)
                            {
                                if (arr.Num != null)
                                {
                                    var mobchk = db.Mobiles.Where(x => x.MobileNum == arr.Num && x.Contact == contactId).Any();
                                    if (!mobchk)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = contactId,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        if (vmodel.TaxRegNo != null)
                        {
                            Accounts acc = db.Accountss.Find(cus.Accounts);
                            acc.TRN = vmodel.TaxRegNo;
                            db.Entry(acc).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var contact = new Contact
                        {
                            Name = vmodel.CustomerName,
                            Address = vmodel.Address,
                            City = vmodel.City,
                            State = vmodel.State,
                            Country = vmodel.Country,
                            Zip = vmodel.Zip,
                            Phone = vmodel.Phone,
                            //Mobile = vmodel.Mobile,
                            Fax = vmodel.Fax,
                            EmailId = vmodel.EmailId,
                            Reference = vmodel.Reference,
                            ContactPerson = vmodel.ContactPerson,
                            Group = 2,
                            Status = Status.active,

                        };
                        db.Contacts.Add(contact);
                        db.SaveChanges();
                        contactId = contact.ContactID;

                        if (vmodel.mobmodel.Count() > 0)
                        {
                            foreach (var arr in vmodel.mobmodel)
                            {
                                if (arr.Num != null)
                                {
                                    var mobchk = db.Mobiles.Where(x => x.MobileNum == arr.Num && x.Contact == contactId).Any();
                                    if (!mobchk)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = contactId,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        Customer cus = new Customer
                        {
                            Contact = contactId,
                            EntryNo = GetEntryNo(),
                            CustomerName = vmodel.CustomerName,
                            CustomerCode = vmodel.CustomerCode,
                            CreditLimit = vmodel.CreditLimit != null ? (decimal)vmodel.CreditLimit : 0,
                            CreditPeriod = vmodel.CreditPeriod != null ? (int)vmodel.CreditPeriod : 0,
                            SalesPerson = vmodel.SalesPerson,
                            //Lattitude = "",
                            //Longitude = "",
                            Location = vmodel.Location,
                            //TaxRegNo = vmodel.TaxRegNo,
                            Remark = vmodel.Remark,

                            SourceOfLead = vmodel.SourceOfLead,
                            Type = CRMCustomerType.PipeLine,
                            LeadStat = vmodel.LeadStat,
                            LeadLevel = vmodel.LeadLevel

                        };
                        db.Customers.Add(cus);
                        db.SaveChanges();
                        custId = cus.CustomerID;
                    }



                    var convert = "";
                    var remark = "";
                    if (vmodel.ConvertFrom != null)
                    {
                        convert = vmodel.ConvertFrom;
                        remark = "Convert";
                    }
                    else
                    {
                        convert = "Direct";
                        remark = "Direct";
                    }

                    CustomerConversion userConv = new CustomerConversion
                    {
                        CustomerID = custId,
                        Type = CRMCustomerType.PipeLine,
                        ConvertFrom = convert,
                        ConvertedUser = UserId,
                        ConvertedDate = System.DateTime.Now,
                        CreatedUser = UserId,
                        CreatedDate = System.DateTime.Now,
                        Remarks = remark
                    };

                    db.CustomerConversions.Add(userConv);
                    db.SaveChanges();
                    if (vmodel.AssignedTo != null)
                    {
                        IList<AssignedTo> Assigned = new List<AssignedTo>();
                        foreach (var arr in vmodel.AssignedTo)
                        {

                            Assigned.Add(new AssignedTo() { CustomerID = (long)custId, EmployeeId = arr });

                        }
                        if (Assigned != null)
                        {
                            db.AssignedTos.AddRange(Assigned);
                            db.SaveChanges();
                        }
                    }

                    // fileupload
                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/leaddocuments/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {

                                var fileCount = db.LeadDocuments.Select(a => a.LeadDocumentId).AsEnumerable().DefaultIfEmpty(0).Max();

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), newName);
                                file.SaveAs(newName);

                                var LeadDoc = new LeadDocument
                                {
                                    CustomerID = custId,
                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.LeadDocuments.Add(LeadDoc);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }


                    com.addlog(LogTypes.Created, UserId, "Customer", "Customers", findip(), custId, "PipeLine Added Successfully");

                    Success("Successfully Added PipeLine details.", true);
                    return RedirectToAction("Create", "PipeLine");
                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form..", true);
                //    ID = r.SourceOfLeadId,
                //    Name = r.SrcName
                return (View());
            }

        }


        // GET:/Edit
        [QkAuthorize(Roles = "Dev,Edit PipeLine")]
        public ActionResult Edit(long? id)
        {
            Customer cus = db.Customers.Find(id);
            if (cus == null)
            {
                return NotFound();
            }
            Contact cont = db.Contacts.Find(cus.Contact);
            Accounts acc = db.Accountss.Find(cus.Accounts);
            PipeLineViewModel cusmodel = new PipeLineViewModel();

            cusmodel.CustomerName = cus.CustomerName;
            cusmodel.CustomerCode = cus.CustomerCode;
            cusmodel.CreditLimit = cus.CreditLimit;
            cusmodel.CreditPeriod = cus.CreditPeriod;
            cusmodel.Remark = cus.Remark;
            cusmodel.SalesPerson = cus.SalesPerson;
            cusmodel.Location = cus.Location;
            cusmodel.TaxRegNo = acc != null ? acc.TRN : null;
            cusmodel.SourceOfLead = cus.SourceOfLead;
            cusmodel.LeadLevel = cus.LeadLevel;
            cusmodel.LeadStat = cus.LeadStat;
            cusmodel.SourceOfLead = cus.SourceOfLead;

            cusmodel.Address = cont.Address;
            cusmodel.City = cont.City;
            cusmodel.State = cont.State;
            cusmodel.Country = cont.Country;
            cusmodel.Zip = cont.Zip;
            cusmodel.Phone = cont.Phone;
            cusmodel.Fax = cont.Fax;
            cusmodel.EmailId = cont.EmailId;
            cusmodel.Reference = cont.Reference;
            cusmodel.ContactPerson = cont.ContactPerson;

            cusmodel.EmailId = cont.EmailId;
            cusmodel.Reference = cont.Reference;
            cusmodel.ContactPerson = cont.ContactPerson;


            var cust = db.Customers.Select(r => new
            {
                ID = r.CustomerName,
                Name = r.CustomerName
            }).ToList();
            ViewBag.CustName = QkSelect.List(cust, "ID", "Name");



            var use = db.Employees
                            .Select(s => new
                            {
                                ID = s.EmployeeId,
                                Name = s.FirstName + " " + s.LastName
                            })
                            .ToList();
            ViewBag.users = QkSelect.List(use, "ID", "Name");
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.SrcOfLead = QkSelect.List(lead, "ID", "Name");
            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;

            //
            var AssignedTo = db.AssignedTos.Where(a => a.CustomerID == id).Select(a => a.EmployeeId).ToList().ToArray();
            cusmodel.AssignedTo = AssignedTo;

            var emp = db.Employees.Select(s => new { ID = s.EmployeeId, Name = s.FirstName + " " + s.LastName }).ToList();
            ViewBag.team = new MultiSelectList(emp, "ID", "Name", AssignedTo);
            //

            var leadstat = db.LeadStatuss.Select(r => new
            {
                ID = r.LeadStatusID,
                Name = r.StatusType
            }).ToList();
            ViewBag.LeadStats = QkSelect.List(leadstat, "ID", "Name");

            ViewBag.image = (from b in db.LeadDocuments
                             join c in db.Customers on b.CustomerID equals c.CustomerID
                             where c.CustomerID == id
                             select new LeadDocumentViewModel
                             {
                                 LeadDocumentId = b.LeadDocumentId,
                                 CustomerID = b.CustomerID,
                                 FileName = b.FileName,

                             }).ToList();

            ViewBag.preEntry = db.Customers.Where(a => a.CustomerID < id && a.Type == CRMCustomerType.PipeLine).Select(a => a.CustomerID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Customers.Where(a => a.CustomerID > id && a.Type == CRMCustomerType.PipeLine).Select(a => a.CustomerID).DefaultIfEmpty().Min();

            return View(cusmodel);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit PipeLine")]
        public ActionResult Edit(PipeLineSubmitViewModel cusmodel, Int64 id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var CodeExists = db.Customers.Any(u => u.CustomerCode == cusmodel.CustomerCode && u.CustomerID != id);
                if (CodeExists)
                {

                    Danger("PipeLine with same PipeLine code exists.", true);
                    return RedirectToAction("Edit", "PipeLine");
                }
                else
                {

                    Customer cus = db.Customers.Find(id);
                    cus.CustomerName = cusmodel.CustomerName;
                    cus.CustomerCode = cusmodel.CustomerCode;
                    cus.CreditLimit = cusmodel.CreditLimit != null ? (decimal)cusmodel.CreditLimit : 0;
                    cus.CreditPeriod = cusmodel.CreditPeriod != null ? (int)cusmodel.CreditPeriod : 0;
                    cus.Remark = cusmodel.Remark;
                    cus.SalesPerson = cusmodel.SalesPerson;

                    cus.Location = cusmodel.Location;

                    cus.SourceOfLead = cusmodel.SourceOfLead;

                    cus.LeadStat = cusmodel.LeadStat;
                    cus.LeadLevel = cusmodel.LeadLevel;


                    db.Entry(cus).State = EntityState.Modified;
                    db.SaveChanges();

                    Contact cont = db.Contacts.Find(cus.Contact);

                    cont.Name = cusmodel.CustomerName;
                    cont.Address = cusmodel.Address;
                    cont.City = cusmodel.City;
                    cont.State = cusmodel.State;
                    cont.Country = cusmodel.Country;
                    cont.Zip = cusmodel.Zip;
                    cont.Phone = cusmodel.Phone;
                    cont.Fax = cusmodel.Fax;
                    cont.EmailId = cusmodel.EmailId;
                    cont.Reference = cusmodel.Reference;
                    cont.ContactPerson = cusmodel.ContactPerson;


                    db.Entry(cont).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == cus.Contact));
                    db.SaveChanges();

                    if (cusmodel.mobmodel != null)
                    {
                        foreach (var arr in cusmodel.mobmodel)
                        {
                            if (arr.Num != null)
                            {
                                var mobchk = db.Mobiles.Where(x => x.MobileNum == arr.Num && x.Contact == cus.Contact).Any();
                                if (!mobchk)
                                {
                                    var mob = new Mobile
                                    {
                                        Contact = cus.Contact,
                                        MobileNum = arr.Num,
                                        Name = arr.Name
                                    };
                                    db.Mobiles.Add(mob);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    if (cusmodel.TaxRegNo != null)
                    {
                        Accounts acc = db.Accountss.Find(cus.Accounts);
                        acc.TRN = cusmodel.TaxRegNo;
                        db.Entry(acc).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    var assig = db.AssignedTos.Where(a => a.CustomerID == id);
                    if (assig != null)
                    {
                        db.AssignedTos.RemoveRange(db.AssignedTos.Where(a => a.CustomerID == id));
                        db.SaveChanges();
                    }
                    if (cusmodel.AssignedTo != null)
                    {
                        IList<AssignedTo> Assigned = new List<AssignedTo>();
                        foreach (var arr in cusmodel.AssignedTo)
                        {

                            Assigned.Add(new AssignedTo() { CustomerID = (long)cus.CustomerID, EmployeeId = arr });

                        }
                        if (Assigned != null)
                        {
                            db.AssignedTos.AddRange(Assigned);
                            db.SaveChanges();
                        }
                    }
                    // fileupload
                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/customerdocuments/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {

                                var fileCount = db.LeadDocuments.Select(a => a.LeadDocumentId).AsEnumerable().DefaultIfEmpty(0).Max();

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/customerdocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/customerdocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/customerdocuments/"), newName);
                                file.SaveAs(newName);

                                var LeadDoc = new LeadDocument
                                {
                                    CustomerID = cus.CustomerID,
                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.LeadDocuments.Add(LeadDoc);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/customerdocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/customerdocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }

                    com.addlog(LogTypes.Updated, UserId, "Customer", "Customers", findip(), cus.CustomerID, "PipeLine Updated Successfully");

                    msg = "Successfully updated Leads details";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }
            }
            else
            {


                msg = "Looks like something went wrong. Please check your form..";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }


        //GET: customer/Delete/5
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete PipeLine")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer Cus = db.Customers.Find(id);
            if (Cus == null)
            {
                return NotFound();
            }
            return PartialView(Cus);
        }

        // POST: customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [QkAuthorize(Roles = "Dev,Delete PipeLine")]
        public ActionResult DeleteAction(long? id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var custcon = db.CustomerConversions.Where(a => a.CustomerID == id);
            if (custcon != null)
            {
                db.CustomerConversions.RemoveRange(db.CustomerConversions.Where(a => a.CustomerID == id));
            }
            Customer Cus = db.Customers.Find(id);

            Contact con = db.Contacts.Find(Cus.Contact);
            if (con != null)
            {
                db.Contacts.Remove(con);
                db.SaveChanges();
            }

            // delete documents
            var ldoc = db.LeadDocuments.Where(a => a.CustomerID == id);
            if (ldoc != null)
            {
                db.LeadDocuments.RemoveRange(db.LeadDocuments.Where(a => a.CustomerID == id));
                db.SaveChanges();
            }
            // delete assigned to
            var assign = db.AssignedTos.Where(a => a.CustomerID == id);
            if (assign != null)
            {
                db.AssignedTos.RemoveRange(db.AssignedTos.Where(a => a.CustomerID == id));
                db.SaveChanges();
            }
            db.Customers.Remove(Cus);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "Customer", "Customers", findip(), Cus.CustomerID, "PipeLine Deleted Successfully");

            stat = true;
            msg = "Successfully Deleted PipeLine details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete PipeLine")]
        public ActionResult DeleteAllPipeLine(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = DeletePipeLine(arr);
                if (chk == true)
                {
                    count++;
                }
            }
            Success("Deleted " + count + " PipeLine.", true);
            return RedirectToAction("Index", "PipeLine");
        }
        private Boolean DeletePipeLine(long id)
        {
            var UserId = User.Identity.GetUserId();
            var custcon = db.CustomerConversions.Where(a => a.CustomerID == id);
            if (custcon != null)
            {
                db.CustomerConversions.RemoveRange(db.CustomerConversions.Where(a => a.CustomerID == id));
                db.SaveChanges();
            }
            Customer Cus = db.Customers.Find(id);

            Contact con = db.Contacts.Find(Cus.Contact);
            if (con != null)
            {
                var mobile = db.Mobiles.Where(a => a.Contact == con.ContactID);
                if (mobile != null)
                {
                    db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == con.ContactID));
                    db.SaveChanges();
                }

                db.Contacts.Remove(con);
                db.SaveChanges();
            }

            db.Customers.Remove(Cus);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "Customer", "Customers", findip(), Cus.CustomerID, "PipeLine Deleted Successfully");
            return true;
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,View PipeLine")]
        public ActionResult Details(long? id)
        {
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Customer cus = db.Customers.Find(id);
                Accounts acc = db.Accountss.Find(cus.Accounts);
                if (cus == null)
                {
                    return NotFound();
                }
                PipeLineSubmitViewModel cusmodel = new PipeLineSubmitViewModel();

                cusmodel.CustomerName = cus.CustomerName;
                cusmodel.CustomerCode = cus.CustomerCode;
                cusmodel.CreditLimit = cus.CreditLimit;
                cusmodel.CreditPeriod = cus.CreditPeriod;
                cusmodel.Remark = cus.Remark;
                cusmodel.SalesEmp = db.Employees.Where(a => a.EmployeeId == cus.SalesPerson).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();

                cusmodel.Location = cus.Location;
                cusmodel.TaxRegNo = acc != null ? acc.TRN : null;
                cusmodel.SrcLead = db.SourceOfLeads.Where(a => a.SourceOfLeadId == cus.SourceOfLead).Select(a => a.SrcName).FirstOrDefault();

                cusmodel.BankName = cus.BankName;
                cusmodel.AccountNo = cus.AccountNo;
                cusmodel.BranchName = cus.BranchName;
                cusmodel.IbanNo = cus.IbanNo;
                cusmodel.Swift = cus.Swift;


                Contact cont = db.Contacts.Find(cus.Contact);

                cusmodel.Address = cont.Address;
                cusmodel.City = cont.City;
                cusmodel.State = cont.State;
                cusmodel.Country = cont.Country;
                cusmodel.Zip = cont.Zip;
                cusmodel.Phone = cont.Phone;
                cusmodel.Fax = cont.Fax;
                cusmodel.EmailId = cont.EmailId;
                cusmodel.Reference = cont.Reference;
                cusmodel.ContactPerson = cont.ContactPerson;

                cusmodel.mobmodel = (from ac in db.Mobiles
                                     where (ac.Contact == cus.Contact)
                                     select new MobileViewModel
                                     {
                                         Num = ac.MobileNum,
                                         Name = ac.Name
                                     }).ToList();
                return View(cusmodel);
            }
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit PipeLine")]
        public JsonResult ImageDelete(long key)
        {
            bool stat = false;
            string msg;
            LeadDocument proImg = db.LeadDocuments.Find(key);
            db.LeadDocuments.Remove(proImg);
            db.SaveChanges();

            string fullPath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + proImg.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "PipeLine", "LeadDocuments", findip(), proImg.LeadDocumentId, "Lead Document Deleted Successfully");


            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted Project Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        private string CustCode(Int64 CNo = 0, string CCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.prefix).FirstOrDefault();

            if (CCode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.number).FirstOrDefault();
                if ((db.Customers.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        CCode = prefix + 1;
                    }
                    else
                    {
                        CCode = prefix + number;
                    }
                }
                else
                {
                    CNo = db.Customers.Max(p => p.EntryNo + 1);
                    CCode = prefix + CNo;
                    if (CodeExist(CCode))
                    {
                        CCode = CustCode(CNo, CCode);
                    }

                }
            }
            else
            {
                CNo = CNo + 1;
                CCode = prefix + CNo;
                if (CodeExist(CCode))
                {
                    CCode = CustCode(CNo, CCode);
                }
            }
            return CCode;
        }


        private bool CodeExist(string Code)
        {
            var Exists = db.Customers.Any(c => c.CustomerCode == Code);
            if (Exists)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private long GetEntryNo()
        {
            Int64 ENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.number).FirstOrDefault();
            if ((db.Customers.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    ENo = 1;
                }
                else
                {
                    ENo = number;
                }
            }
            else
            {
                ENo = db.Customers.Max(p => p.EntryNo + 1);
            }
            return ENo;
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,View PipeLine")]
        public ActionResult ViewDetails(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer cus = db.Customers.Find(id);
            if (cus == null)
            {
                return NotFound();
            }
            PipeLineDetailsViewModel cusmodel = new PipeLineDetailsViewModel();

            cusmodel.CustomerName = cus.CustomerName;
            cusmodel.CustomerCode = cus.CustomerCode;
            cusmodel.CreditLimit = cus.CreditLimit;
            cusmodel.CreditPeriod = cus.CreditPeriod;
            cusmodel.Remark = cus.Remark;
            cusmodel.SalesEmp = db.Employees.Where(a => a.EmployeeId == cus.SalesPerson).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();

            cusmodel.Location = cus.Location;
            cusmodel.SrcLead = db.SourceOfLeads.Where(a => a.SourceOfLeadId == cus.SourceOfLead).Select(a => a.SrcName).FirstOrDefault();

            cusmodel.BankName = cus.BankName;
            cusmodel.AccountNo = cus.AccountNo;
            cusmodel.BranchName = cus.BranchName;
            cusmodel.IbanNo = cus.IbanNo;
            cusmodel.Swift = cus.Swift;

            Accounts acc = db.Accountss.Find(cus.Accounts);
            cusmodel.TaxRegNo = acc != null ? acc.TRN : null;

            Contact cont = db.Contacts.Find(cus.Contact);

            cusmodel.Address = cont.Address;
            cusmodel.City = cont.City;
            cusmodel.State = cont.State;
            cusmodel.Country = cont.Country;
            cusmodel.Zip = cont.Zip;
            cusmodel.Phone = cont.Phone;
            cusmodel.Fax = cont.Fax;
            cusmodel.EmailId = cont.EmailId;
            cusmodel.Reference = cont.Reference;
            cusmodel.ContactPerson = cont.ContactPerson;
            cusmodel.LeadLevel = cus.LeadLevel;

            var mob = (from ac in db.Mobiles
                       where (ac.Contact == cus.Contact)
                       select new MobileViewModel
                       {
                           Num = ac.MobileNum,
                           Name = ac.Name
                       }).ToList();

            var leaddoc = db.LeadDocuments.Where(a => a.CustomerID == id).ToList();
            if (leaddoc.Any())
            {
                cusmodel.LeadDocuments = (from b in db.LeadDocuments
                                          join c in db.Customers on b.CustomerID equals c.CustomerID
                                          where c.CustomerID == id
                                          select new LeadDocumentViewModel
                                          {
                                              CustomerID = c.CustomerID,
                                              LeadDocumentId = b.LeadDocumentId,
                                              FileName = b.FileName,
                                          }).ToList();
            }

            cusmodel.LeadAssign = (from a in db.AssignedTos
                                   join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                                   from b in emp.DefaultIfEmpty()
                                   where a.CustomerID == id
                                   select new LeadAssignToViewModel
                                   {

                                       Empname = b.FirstName + " " + b.MiddleName + " " + b.LastName + ",",
                                   }).ToList();

            cusmodel.LeadCreated = (from a in db.Customers
                                    join b in db.CustomerConversions on a.CustomerID equals b.CustomerID into conv
                                    from b in conv.DefaultIfEmpty()
                                    join c in db.Users on b.CreatedUser equals c.Id into emp
                                    from c in emp.DefaultIfEmpty()
                                    where a.CustomerID == id
                                    select new LeadCreatedViewModel
                                    {
                                        CreatedUser = c.UserName,
                                        CreatedDate = b.CreatedDate.ToString(),
                                        Converted = b.ConvertFrom,
                                    }).ToList();
            //LeadActivity
            string ids = id.ToString();

            // Lead Time Line

            var lact = (from a in db.LogManagers
                        join b in db.Users on a.User equals b.Id into user
                        from b in user.DefaultIfEmpty()
                        where (a.LogID == id.ToString()) && (a.LogTable == "Customers")

                        select new LeadTimelineViewModel
                        {

                            Name = b.UserName,
                            LogType = a.LogType.ToString(),
                            Time = a.LogTime,
                            Details = a.LogDetails,


                            //details = b.UserName + " " + Enum.GetName(typeof(LogTypes), a.LogType) +" "+ a.LogSection +" on "+a.LogTime
                        }).ToList();

            var rem = (from a in db.LeadRemarks
                       join b in db.Users on a.AddedUser equals b.Id into emp
                       from b in emp.DefaultIfEmpty()
                       where a.CustomerID == id

                       select new LeadTimelineViewModel
                       {
                           Name = b.UserName,
                           LogType = "Remark Added",
                           Time = a.CreatedDate,
                           Details = a.Remark,
                       }).ToList();

            var asl = (from a in db.AssignedToLogs
                       join c in db.Users on a.AddedUser equals c.Id into usr
                       from c in usr.DefaultIfEmpty()
                       join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                       from b in emp.DefaultIfEmpty()

                       where a.CustomerID == id
                       select new LeadTimelineViewModel
                       {
                           Name = c.UserName,
                           LogType = a.Status,
                           Time = a.AssignedDate,
                           Details = a.Status + " Employee " + b.FirstName + " " + b.MiddleName + " " + b.LastName,
                       }).ToList();
            var det = lact.Union(rem);
            var comp = det.Union(asl).OrderByDescending(a => a.Time);
            cusmodel.LeadTimeLine = comp.ToList();
            return View(cusmodel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public JsonResult SearchPipeLine(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Customers.Where(p => p.Type == CRMCustomerType.PipeLine && (p.CustomerName.ToLower().Contains(q.ToLower()) || p.CustomerCode.ToLower().Contains(q.ToLower()) || p.CustomerName.Contains(q) || p.CustomerCode.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.CustomerCode + " - " + b.CustomerName, //each json object will have 
                                      id = b.CustomerID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Customers.Where(p => p.Type == CRMCustomerType.PipeLine).Select(b => new SelectFormat
                {
                    text = b.CustomerCode + " - " + b.CustomerName, //each json object will have 
                                                                    // text = b.CustomerName, //each json object will have 
                    id = b.CustomerID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpGet]
        public JsonResult GetMobile(long CnId)
        {
            var ConD = (from a in db.Mobiles
                        join b in db.Customers on a.Contact equals b.Contact
                        where b.CustomerID == CnId
                        select new
                        {
                            Mob = a.MobileNum,
                            Name = a.Name
                        }).ToList();
            return Json(ConD);
        }
        [HttpPost]
        public ActionResult GetAllRemarks(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var v = (from a in db.LeadRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     where a.CustomerID == CustomerId
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.LeadRemarkId,
                         a.CreatedDate,
                         empnae = b.UserName,
                         //c.StatusType,
                         a.Remark,
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            //    // Apply search   

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
