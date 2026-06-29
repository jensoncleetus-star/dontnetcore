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
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net.Mail;



namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class QuotationController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public QuotationController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Quotation 
        [QkAuthorize(Roles = "Dev,Quotation List")]
        public ActionResult Index()
        {
            ViewBag.SrcOfLead = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = true, Text = "Select Source Of Lead", Value = "0"},
                         }, "Value", "Text", 1);
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            ViewBag.Quot = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.SalesExecutive = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);

            ViewBag.Prjct = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                               }, "Value", "Text", 1);
            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            var MlaQuot = db.EnableSettings.Where(a => a.EnableType == "MLAQuot").FirstOrDefault();
            var MlaQuots = MlaQuot != null ? MlaQuot.Status : Status.inactive;
            ViewBag.MLAQuot = MlaQuots;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindQuot").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;
            //ProjectBasedBusiness
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var tsk = db.ProTasks
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            })
            .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            return View();
        }

        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object


            long id = Convert.ToInt64(Request.Form.GetValues("id").First());



            if (Request.Form.Files.Count > 0)
            {
                try
                {




                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/quotationdocument/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {


                                var fileCount = db.quotationdocuments.Select(a => a.qutid).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName = Path.GetFileName(file.FileName);

                                String extension = Path.GetExtension(fileName);

                                var FStatus = Status.active;
                                String newName = fileCount + extension;
                                string newFName = fileCount + extension;

                                var thumbName = "";
                                var resizeName = "";
                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/quotationdocument/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/quotationdocument/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/quotationdocument/"), newName);
                                file.SaveAs(newName);

                                var qtndoc = new quotationdocument
                                {
                                    quotationID = id,
                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.quotationdocuments.Add(qtndoc);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/quotationdocument/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/quotationdocument/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }









                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
        public ActionResult convert()
        {

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.SalesExecutive = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);

            ViewBag.Prjct = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                               }, "Value", "Text", 1);
            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            var MlaQuot = db.EnableSettings.Where(a => a.EnableType == "MLAQuot").FirstOrDefault();
            var MlaQuots = MlaQuot != null ? MlaQuot.Status : Status.inactive;
            ViewBag.MLAQuot = MlaQuots;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindQuot").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;
            //ProjectBasedBusiness
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var tsk = db.ProTasks
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            })
            .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            return PartialView();
        }

        [HttpPost]
        public JsonResult Getlead(long? Customer, string TaxReg, long? Mobile, long? Phone, decimal? CLimit, int? CPeriod, long? Employee, string TxType, string MailId, string Alias)
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

            TaxType ttype = new TaxType();
            if (TxType != null && TxType != "")
            {
                ttype = (TxType == "0") ? TaxType.ItemWise : TaxType.Exempt;
            }

            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");

            var v = (from a in db.Customers
                     join x in db.Accountss on a.Accounts equals x.AccountsID
                     join b in db.Contacts on a.Contact equals b.ContactID into tmp
                     from b in tmp.DefaultIfEmpty()
                     join c in db.Employees on a.SalesPerson equals c.EmployeeId into emp
                     from c in emp.DefaultIfEmpty()
                     where a.Type == CRMCustomerType.Leads &&
                           (Customer == null || Customer == 0 || a.CustomerID == Customer) &&
                           (TaxReg == null || TaxReg == "" || x.TRN == TaxReg) &&
                           (Mobile == null || Mobile == 0 || b.ContactID == Mobile) &&
                           (Phone == null || Phone == 0 || b.ContactID == Phone) &&
                           (CLimit == null || CLimit == 0 || a.CreditLimit == CLimit) &&
                           (CPeriod == null || CPeriod == 0 || a.CreditPeriod == CPeriod) &&
                           (Employee == null || Employee == 0 || a.SalesPerson == Employee) &&
                           (TxType == null || TxType == "" || a.TaxType == ttype) &&
                           (MailId == null || MailId == "" || b.EmailId == MailId)
                           && (userpermission == true || x.CreatedBy == UserId)
                           && (Alias == null || Alias == "" || x.Alias == Alias)
                     select new
                     {
                         id = a.CustomerID,
                         a.CustomerCode,
                         a.CustomerName,
                         TaxRegNo = x.TRN,
                         a.Location,
                         Address = b.Address != null ? b.Address : "" +
                         "<br/>" + b.City != null ? b.City : "" +
                         " " + b.State != null ? b.State : "" +
                         " " + b.Country != null ? b.Country : "" +
                         "<br/>" + b.Zip != null ? b.Zip : "",
                         Phone = b.Phone,
                         //Mobile = b.Mobile,
                         Email = b.EmailId,
                         CreditLimit = a.CreditLimit,
                         CreditPeriod = a.CreditPeriod,
                         OpnBalance = (x.OpnBalanceCr > 0) ? (x.OpnBalanceCr != 0 ? x.OpnBalanceCr + " Cr." : "0.00") : (x.OpnBalance != 0 ? x.OpnBalance + " Dr." : "0.00"),
                         Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0),
                         Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0),
                         Dev = uDev,
                         Details = uCustView,
                         Edit = uEdit,
                         Delete = uDelete,
                         Alias = x.Alias,
                         mobmodel = (from ac in db.Mobiles
                                     where (ac.Contact == a.Contact)
                                     select new MobileViewModel
                                     {
                                         Num = (ac.Name == "" || ac.Name == null) ? ac.MobileNum : ac.MobileNum + "-" + ac.Name,
                                         Name = ac.Name
                                     }).ToList(),

                     }).Select(o => new
                     {
                         o.id,
                         o.CustomerCode,
                         o.CustomerName,
                         o.TaxRegNo,
                         o.Location,
                         o.Address,
                         o.Phone,
                         o.Email,
                         o.CreditLimit,
                         o.CreditPeriod,
                         o.OpnBalance,
                         o.Credit,
                         o.Debit,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Delete,
                         o.Alias,
                         o.mobmodel,
                         currentbalance = (o.Debit > o.Credit) ? ((o.Debit - o.Credit) + " Dr.") : ((o.Credit - o.Debit) + " Cr."),
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.CustomerName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.CustomerCode.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.TaxRegNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Alias.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.OpnBalance.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.currentbalance.ToString().ToLower().Contains(search.ToLower())
                                 //p.CreditPeriod.ToString().ToLower().Contains(search.ToLower())
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
        public ActionResult downloadprint(long quotationId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;



            var UserId = User.Identity.GetUserId();
            var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;


            long? Prj = null;

            var fmapp = db.FieldMappings.Where(a => a.Section == "Quot" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            Quotation Quoentry = db.Quotations.Find(quotationId);

            string qedate = Quoentry.QuotDate.ToString("dd-MM-yyyy");

            var QuotData = com.QuotationData(quotationId, InPrintItemCode, PartNoCheck, 1000, ProjectCheck, ComHeadCheck);
            var item = QuotData.pdfItem.ToList();
            var summary = QuotData;
            var billsundry = QuotData.billsundry.ToList();

            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;

            bool stat = true;
            return Json(new { status = stat, item, summary, billsundry, fmapp, qtnid = quotationId });


        }
        [QkAuthorize(Roles = "Dev,Quotation Entry")]
        public ActionResult Create(long? id, string type, long? protaskid)
        {
            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var CurrencyEnable = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            ViewBag.EnableCurrency = CurrencyEnable;

            var Currency = db.CurrencyMasters
              .Select(s => new
              {
                  Id = s.Id,
                  Name = s.CurrencyCode
              }).ToList();
            ViewBag.CurrencyVal = QkSelect.List(Currency, "Id", "Name");
            ViewBag.SrcOfLead = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "Select Source Of Lead", Value = "0"},
                            }, "Value", "Text", 1);


            var ref1 = db.Quotations
                .Select(s => new
                {
                    ID = s.Ref1,
                    Name = s.Ref1
                }).Distinct()
                .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Quotations
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Quotations
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Quotations
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Quotations
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            var Quotentry = new QuotationViewModel
            {
                BillNo = InvoiceNo(),
                QuotDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),

                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "quote").Select(a => a.TermsCondit).FirstOrDefault()
            };
            Quotentry.quotationexpdate = System.DateTime.Now.AddDays(30);
            Quotentry.revision = Convert.ToString(Convert.ToInt64(Quotentry.revision) + 1);
            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;


            //    .Where(o => o.Type == CRMCustomerType.Leads)
            //  .Select(s => new
            //      CustomerID = s.CustomerID,
            //      CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            ViewBag.leads = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                             }, "Value", "Text", 0); //QkSelect.List(leads, "CustomerID", "CustomerDetails");
            //quotation type drop down MC
            var quot = db.QuotationTypes
                             .Select(s => new
                             {
                                 ID = s.QuotId,
                                 Name = s.QuotType
                             }).Distinct()
                             .ToList().OrderBy(s => s.Name);
            ViewBag.Quot = QkSelect.List(quot, "ID", "Name");
            var service = db.servicetypes
                        .Select(s => new
                        {
                            ID = s.servicetypeid,
                            Name = s.title
                        }).Distinct()
                        .ToList().OrderBy(s => s.Name);
            ViewBag.service = QkSelect.List(service, "ID", "Name");
            var userid = User.Identity.GetUserId();
            var use = (from a in db.Employees
                       join b in db.Users on a.UserId equals b.Id
                       where b.Status == 1
                       select a).Where(o => o.UserId == userid)
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            var pr = db.Projects
                   .Select(s => new
                   {
                       ID = s.ProjectId,
                       Name = s.ProCode + "-" + s.ProjectName
                   })
                   .ToList();
            ViewBag.Proj = QkSelect.List(pr, "ID", "Name");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
            if (protaskid != null)
            {
                var data = (
                    from p in db.ProTasks
                    join pro in db.Projects on p.ProjectId equals pro.ProjectId into proo
                    from pro in proo.DefaultIfEmpty()
                    join c in db.Customers on p.CustomerID equals c.CustomerID into coo
                    from c in coo.DefaultIfEmpty()
                    where p.ProTaskId == protaskid
                    select new
                    {
                        c.CustomerID,
                        p.ProTaskId,
                        pro.ProjectId,
                    }


                    ).FirstOrDefault();
                Quotentry.Customer = data.CustomerID;
                Quotentry.Project = data.ProjectId;
                Quotentry.ProTask = data.ProTaskId;
                Quotentry.SalesTypes = db.SalesTypes.ToList();

            }
            Quotation quentry = db.Quotations.Find(id);
            if (id != null)
            {
                //duplicate quotation
                if (type == "QuotExtend")
                {
                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    Quotentry.ConTypeId = quentry.QuotationId;
                    Quotentry.ConType = type;
                    Quotentry.QuotationId = id;
                    Quotentry.QuotCashier = quentry.QuotCashier;
                    Quotentry.QuotValidity = quentry.QuotValidity;
                    Quotentry.Customer = quentry.Customer;
                    Quotentry.Remarks = quentry.Remarks;
                    Quotentry.TermsCondition = quentry.TermsCondition;
                    Quotentry.Branch = quentry.Branch;
                    Quotentry.SaleType = quentry.SaleType;
                    Quotentry.SalesType = quentry.SalesType;
                    Quotentry.PaymentTerms = quentry.PaymentTerms;
                    string revision = quentry.revision == "" ? "0" : quentry.revision;
                    Quotentry.revision = "1";
                    Quotentry.Project = quentry.Project;
                    Quotentry.ProTask = quentry.ProTask;
                    Quotentry.SalesTypes = db.SalesTypes.ToList();
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Quotation").FirstOrDefault();
                        Quotentry.FromDate = Hdet.StartDate;
                        Quotentry.ToDate = Hdet.EndDate;
                        Quotentry.HireType = Hdet.HireType;
                    }

                }
            }

            companySet();
            if (quentry != null)
            {

                var cust = (from a in db.Customers
                            join b in db.Accountss on a.Accounts equals b.AccountsID

                            where a.Type == CRMCustomerType.Customer &&
                            !a.CustomerName.StartsWith("OLD-")
                            && a.CustomerID == quentry.Customer
                            select new
                            {
                                a.CustomerID,
                                a.CustomerCode,
                                a.CustomerName
                            }).ToList().Select(s => new
                            {
                                CustomerID = s.CustomerID,
                                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                            }).ToList();
                ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            }
            else
            {
                ViewBag.Custer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                                 }, "Value", "Text", 0);
            }
            var userpermission = User.IsInRole("All Quotation Entry");
            var UserId = User.Identity.GetUserId();

            ViewBag.LastEntry = db.Quotations.Where(a => a.CreatedUserId == UserId || userpermission == true).Select(p => p.QuotationId).AsEnumerable().DefaultIfEmpty(0).Max();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Contype = type;

            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaQuot = db.EnableSettings.Where(a => a.EnableType == "MLAQuot").FirstOrDefault();
            var MlaQuots = MlaQuot != null ? MlaQuot.Status : Status.inactive;
            ViewBag.MLAQuot = MlaQuots;
            //for Lastsale & lastPurchase in salelist/purchaselist popup
            var EnableAutoSave = db.EnableSettings.Where(a => a.EnableType == "Autosave").FirstOrDefault();
            var AutoSave = EnableAutoSave != null ? EnableAutoSave.Status : Status.inactive;
            ViewBag.AutoSave = AutoSave;

            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;

            Quotentry.SaleTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").Select(a => a.TypeValue).FirstOrDefault());
            Quotentry.PurTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").Select(a => a.TypeValue).FirstOrDefault());

            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //end

            //field mapping
            Quotentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Quot" && a.Status == Status.active).ToList();

            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            ViewBag.PrintLayout = PriLay;
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();
            ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");
            return View(Quotentry);
        }

        [QkAuthorize(Roles = "Dev,Quotation Entry")]
        public ActionResult Createmobile(long? id, string type, long? protaskid)
        {


            var Quotentry = new QuotationViewModel
            {
                BillNo = InvoiceNo(),
                QuotDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),

                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "quote").Select(a => a.TermsCondit).FirstOrDefault()
            };
            Quotentry.quotationexpdate = System.DateTime.Now.AddDays(30);
            Quotentry.revision = Convert.ToString(Convert.ToInt64(Quotentry.revision) + 1);
            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;
            var cust = db.Customers
                .Where(o => o.Type == CRMCustomerType.Customer)
              .Select(s => new
              {
                  CustomerID = s.CustomerID,
                  CustomerDetails = s.CustomerCode + " - " + s.CustomerName
              }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var leads = db.Customers
                .Where(o => o.Type == CRMCustomerType.Leads)
              .Select(s => new
              {
                  CustomerID = s.CustomerID,
                  CustomerDetails = s.CustomerCode + " - " + s.CustomerName
              }).ToList();
            ViewBag.leads = QkSelect.List(leads, "CustomerID", "CustomerDetails");
            //quotation type drop down MC
            var quot = db.QuotationTypes
                             .Select(s => new
                             {
                                 ID = s.QuotId,
                                 Name = s.QuotType
                             }).Distinct()
                             .ToList().OrderBy(s => s.Name);
            ViewBag.Quot = QkSelect.List(quot, "ID", "Name");

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            var pr = db.Projects
                   .Select(s => new
                   {
                       ID = s.ProjectId,
                       Name = s.ProCode + "-" + s.ProjectName
                   })
                   .ToList();
            ViewBag.Proj = QkSelect.List(pr, "ID", "Name");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
            if (protaskid != null)
            {
                var data = (
                    from p in db.ProTasks
                    join pro in db.Projects on p.ProjectId equals pro.ProjectId into proo
                    from pro in proo.DefaultIfEmpty()
                    join c in db.Customers on p.CustomerID equals c.CustomerID into coo
                    from c in coo.DefaultIfEmpty()
                    where p.ProTaskId == protaskid
                    select new
                    {
                        c.CustomerID,
                        p.ProTaskId,
                        pro.ProjectId,
                    }


                    ).FirstOrDefault();
                Quotentry.Customer = data.CustomerID;
                Quotentry.Project = data.ProjectId;
                Quotentry.ProTask = data.ProTaskId;
                Quotentry.SalesTypes = db.SalesTypes.ToList();

            }
            Quotation quentry = db.Quotations.Find(id);
            if (id != null)
            {
                //duplicate quotation
                if (type == "QuotExtend")
                {
                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    Quotentry.ConTypeId = quentry.QuotationId;
                    Quotentry.ConType = type;
                    Quotentry.QuotationId = id;
                    Quotentry.QuotCashier = quentry.QuotCashier;
                    Quotentry.QuotValidity = quentry.QuotValidity;
                    Quotentry.Customer = quentry.Customer;
                    Quotentry.Remarks = quentry.Remarks;
                    Quotentry.TermsCondition = quentry.TermsCondition;
                    Quotentry.Branch = quentry.Branch;
                    Quotentry.SaleType = quentry.SaleType;
                    Quotentry.SalesType = quentry.SalesType;
                    Quotentry.PaymentTerms = quentry.PaymentTerms;
                    string revision = quentry.revision == "" ? "0" : quentry.revision;
                    Quotentry.revision = Convert.ToString(Convert.ToInt64(revision)) + 1;
                    Quotentry.Project = quentry.Project;
                    Quotentry.ProTask = quentry.ProTask;
                    Quotentry.SalesTypes = db.SalesTypes.ToList();
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Quotation").FirstOrDefault();
                        Quotentry.FromDate = Hdet.StartDate;
                        Quotentry.ToDate = Hdet.EndDate;
                        Quotentry.HireType = Hdet.HireType;
                    }

                }
            }

            companySet();

            var userpermission = User.IsInRole("All Quotation Entry");
            var UserId = User.Identity.GetUserId();

            ViewBag.LastEntry = db.Quotations.Where(a => a.CreatedUserId == UserId || userpermission == true).Select(p => p.QuotationId).AsEnumerable().DefaultIfEmpty(0).Max();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Contype = type;

            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaQuot = db.EnableSettings.Where(a => a.EnableType == "MLAQuot").FirstOrDefault();
            var MlaQuots = MlaQuot != null ? MlaQuot.Status : Status.inactive;
            ViewBag.MLAQuot = MlaQuots;
            //for Lastsale & lastPurchase in salelist/purchaselist popup
            var EnableAutoSave = db.EnableSettings.Where(a => a.EnableType == "Autosave").FirstOrDefault();
            var AutoSave = EnableAutoSave != null ? EnableAutoSave.Status : Status.inactive;
            ViewBag.AutoSave = AutoSave;

            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;

            Quotentry.SaleTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").Select(a => a.TypeValue).FirstOrDefault());
            Quotentry.PurTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").Select(a => a.TypeValue).FirstOrDefault());

            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //end

            //field mapping
            Quotentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Quot" && a.Status == Status.active).ToList();

            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            ViewBag.PrintLayout = PriLay;
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();
            ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");
            return View(Quotentry);
        }

        [QkAuthorize(Roles = "Dev,Quotation Entry")]
        public JsonResult CreateQuotation(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {




            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var f = BillExist(quotdata[15]);
                if (f == true)
                {
                    stat = false;
                    msg = "Same Quotation Number Already Exists. please increase Qotation No";
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;



                var UserId = User.Identity.GetUserId();
                var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(quotdata[17]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                long? Prj = null;
                if (quotdata[18] != null && quotdata[18] != "")
                {
                    Prj = Convert.ToInt64(quotdata[18]);
                }
                long? quot = null;
                if (quotdata[39] != null && quotdata[39] != "")
                {
                    quot = Convert.ToInt64(quotdata[39]);
                }
                //sales entry
                Quotation Quoentry = new Quotation();
                if (quotdata[19] != null)
                {
                    string str = quotdata[19];
                    SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                    Quoentry.SaleType = Stype;
                }
                else
                {
                    Quoentry.SaleType = SaleType.Sale;
                }
                if (quotdata.Count() > 41)
                {
                    if (quotdata[41] != null && quotdata[41] != "")
                    {
                        Quoentry.sourceoflead = Convert.ToInt64(quotdata[41]);
                    }
                    if (quotdata[42] != null && quotdata[42] != "")
                    {
                        Quoentry.Currency = Convert.ToInt64(quotdata[42]);
                    }
                    if (quotdata[43] != null && quotdata[43] != "")
                    {
                        Quoentry.ConvertionRate = quotdata[43];
                    }
                    if (quotdata[44] != null && quotdata[44] != "")
                    {
                        Quoentry.FCTotal = Convert.ToDecimal(quotdata[44]);
                    }
                    if (quotdata[45] != null && quotdata[45] != "")
                    {
                        Quoentry.servicetype = Convert.ToInt64(quotdata[45]);
                    }
                }
                if (quotdata[40] != "autosave")
                {
                    Quoentry.QuotNo = GetQeNo(Quoentry.SaleType);
                    Quoentry.BillNo = quotdata[15];
                }
                else
                {
                    Quoentry.QuotNo = Convert.ToInt64(quotdata[15]);
                    Quoentry.BillNo = quotdata[15];
                }

                Quoentry.QuotDate = DateTime.Parse(quotdata[2], new CultureInfo("en-GB"));
                Quoentry.QuotCashier = quotdata[1] != "" ? Convert.ToInt64(quotdata[1]) : 0;
                Quoentry.Customer = Convert.ToInt64(quotdata[0]);
                Quoentry.QuotItems = Convert.ToInt32(quotdata[3]);
                Quoentry.QuotItemQuantity = (quotdata[4] == "NaN") ? 0 : Convert.ToDecimal(quotdata[4]);
                Quoentry.QuotSubTotal = Convert.ToDecimal(quotdata[8]);
                Quoentry.QuotTax = Convert.ToDecimal(quotdata[9]);
                Quoentry.QuotTaxAmount = Convert.ToDecimal(quotdata[5]);
                Quoentry.QuotDiscount = Convert.ToDecimal(quotdata[6]);
                Quoentry.QuotGrandTotal = Convert.ToDecimal(quotdata[7]);
                Quoentry.QuotNote = "";
                Quoentry.Mail = 0;
                Quoentry.QuotCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                Quoentry.CreatedUserId = UserId;
                Quoentry.Status = Status.active;

                Quoentry.TermsCondition = Convert.ToString(quotdata[11]);
                Quoentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "Quotation").Select(a => a.EmailTemplateID).FirstOrDefault();
                Quoentry.CompanyHeaderID = 0;
                Quoentry.Branch = Branch;
                Quoentry.QuotValidity = quotdata[10] == "" ? 0 : Convert.ToInt32(quotdata[10]);
                Quoentry.Remarks = quotdata[16];
                Quoentry.Project = Prj;
                Quoentry.ProTask = quotdata[27] != "" ? Convert.ToInt64(quotdata[27]) : 0;

                Quoentry.SalesType = Convert.ToInt64(quotdata[23]);
                Quoentry.PaymentTerms = (quotdata[24]);


                Quoentry.Ref1 = Convert.ToString(quotdata[29]);
                Quoentry.Ref2 = Convert.ToString(quotdata[30]);
                Quoentry.Ref3 = Convert.ToString(quotdata[31]);
                Quoentry.Ref4 = Convert.ToString(quotdata[32]);
                Quoentry.Ref5 = Convert.ToString(quotdata[33]);
                Quoentry.expdate = DateTime.Parse(quotdata[35], new CultureInfo("en-GB"));
                if (!String.IsNullOrEmpty(quotdata[36]))
                    Quoentry.leadsid = Convert.ToInt64(quotdata[36]);
                Quoentry.quotationstatus = Convert.ToInt32(quotdata[37]);
                //Quoentry.quotationtype = Convert.ToInt32()

                Quoentry.revision = Convert.ToString(quotdata[38]);
                Quoentry.quotationtype = quot;


                db.Quotations.Add(Quoentry);
                db.SaveChanges();
                Int64 quotationId = 0;
                Int64 checkid = 0;
                string check = "true";

                quotationId = Quoentry.QuotationId;
                if (Quoentry.SaleType == SaleType.Hire)
                {
                    HireDetail HDetils = new HireDetail();
                    HDetils.StartDate = DateTime.Parse(quotdata[20], new CultureInfo("en-GB"));
                    HDetils.EndDate = DateTime.Parse(quotdata[21], new CultureInfo("en-GB"));
                    HDetils.Section = "Quotation";
                    HDetils.Reference = quotationId;
                    HDetils.HireType = Convert.ToInt64(quotdata[22]);
                    db.HireDetails.Add(HDetils);
                    db.SaveChanges();
                }
                var CustomerName = db.Customers.Where(a => a.CustomerID == Quoentry.Customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();



                ////// add to SEItem
                string result = string.Empty;
                DataTable dtItem = new DataTable();
                dtItem.Columns.Add("ItemUnit");
                dtItem.Columns.Add("ItemUnitPrice");
                dtItem.Columns.Add("ItemQuantity");
                dtItem.Columns.Add("ItemSubTotal");
                dtItem.Columns.Add("ItemDiscount");
                dtItem.Columns.Add("ItemTax");
                dtItem.Columns.Add("ItemTaxAmount");
                dtItem.Columns.Add("ItemTotalAmount");
                dtItem.Columns.Add("ItemNote");
                dtItem.Columns.Add("Quotation");
                dtItem.Columns.Add("Item");



                foreach (var arr in array)
                {
                    DataRow dr = dtItem.NewRow();
                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = (arr[3] == "") ? 0 : Convert.ToDecimal(arr[3]);
                    dr["ItemQuantity"] = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                    dr["ItemTax"] = Convert.ToDecimal(arr[10]);
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                    if (arr.Length > 31)
                    {

                        dr["itemNote"] = Convert.ToString(arr[32].Replace("\n", "<br />"));
                        if (arr.Length > 36 && arr[37] != "")
                        {
                            dr["itemNote"] = Convert.ToString(arr[37]) + "||" + dr["itemNote"];
                        }
                    }
                    else if (arr.Length < 30)
                        dr["itemNote"] = Convert.ToString(arr[29]);

                    dr["Quotation"] = quotationId;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    dtItem.Rows.Add(dr);

                    var item = Convert.ToInt32(arr[0]);
                    var chkbundle = db.ItemBundles.Where(a => a.mainItem == item).Select(a => a.ItemBundleId).FirstOrDefault();
                    if (chkbundle > 0)
                    {
                        var bunQuan = Convert.ToDecimal(arr[2]);
                        var itemBundle = (from g in db.ItemBundles
                                          join b in db.Items on g.mainItem equals b.ItemID
                                          where b.ItemID == item
                                          select new
                                          {
                                              g.ItemBundleId
                                          }).FirstOrDefault();

                        long typ = Convert.ToInt64(quotdata[22]);

                        var bundle = (from a in db.BundleItems
                                      join b in db.Items on a.ItemId equals b.ItemID
                                      join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                      from c in primary.DefaultIfEmpty()
                                      let hir = db.HireRates.Where(x => x.ItemId == b.ItemID && x.type == typ).Select(y => y.Rate).FirstOrDefault()
                                      where a.ItemBundle == itemBundle.ItemBundleId
                                      select new
                                      {
                                          b.ItemCode,
                                          b.ItemName,
                                          c.ItemUnitName,
                                          ItemUnitPrice = (Quoentry.SaleType == SaleType.Sale) ? a.ItemSubTotal : (hir == null ? 0 : hir),
                                          quantity = a.ItemQuantity,
                                          ItemSubTotal = a.ItemSubTotal,
                                          ItemTax = a.ItemTax,
                                          ItemTaxAmount = a.ItemTaxAmount,
                                          ItemTotalAmount = a.ItemTotalAmount,
                                          ItemUnit = a.ItemUnit,
                                          Item = a.ItemId
                                      }).ToList();
                        foreach (var bu in bundle)
                        {
                            var qua = (bunQuan * bu.quantity);
                            var ItemSubTotal = qua * bu.ItemUnitPrice;
                            var buTaxAmount = (ItemSubTotal * bu.ItemTax) / 100;

                            decimal itemtax = 0;
                            decimal taxamt = 0;
                            decimal totamt = 0;

                            itemtax = bu.ItemTax;
                            taxamt = buTaxAmount;
                            totamt = (buTaxAmount + ItemSubTotal);

                            DataRow dbu = dtItem.NewRow();
                            dbu["ItemUnit"] = bu.ItemUnit;
                            dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                            dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                            dbu["ItemSubTotal"] = ItemSubTotal;
                            // add parent itemid in discount for reference
                            dbu["ItemDiscount"] = item;
                            dbu["ItemTax"] = itemtax;
                            dbu["ItemTaxAmount"] = taxamt;
                            dbu["ItemTotalAmount"] = totamt;
                            dbu["itemNote"] = "-:{Bundle_Item}";
                            dbu["Quotation"] = quotationId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }
                }

                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypeQuotItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertQuotationItems", "@TableType");
                //// execute sql 
                db.Database.ExecuteSqlRaw(sql, parameter);
                QuickSoft.Helpers.DocumentTotals.RecomputeQuotation(db, quotationId); // forward-correctness: header = SUM(lines)

                if (bsmodel != null)
                {
                    foreach (var bs in bsmodel)
                    {
                        var qtB = new QtBillSundry
                        {
                            Quotation = quotationId,
                            BillSundry = bs.BillSundry,
                            BsValue = bs.BsValue,
                            AmountType = bs.AmountType,
                            BsType = bs.BsType,
                            BsAmount = bs.BsAmount,
                        };
                        db.QtBillSundrys.Add(qtB);
                        db.SaveChanges();

                    }
                }


                if (quotdata[25] != null && quotdata[25] != "0" && quotdata[25] != "" && quotdata[26] != null && quotdata[26] != "" && quotdata[26] != "0")
                {
                    ConvertTransactions ConTran = new ConvertTransactions();

                    ConTran.ConvertFrom = quotdata[26];
                    ConTran.ConvertTo = "Quote";
                    ConTran.From = Convert.ToInt64(quotdata[25]);
                    ConTran.To = quotationId;
                    ConTran.Status = 0;
                    ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    ConTran.CreatedBy = UserId;
                    ConTran.Branch = Convert.ToInt32(Branch);

                    db.ConvertTransactionss.Add(ConTran);
                    db.SaveChanges();
                    com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Convertion");
                }

                //Approved By
                var Appby = Convert.ToString(quotdata[28]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = quotationId;
                        approval.Type = "Quotation";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }

                com.addlog(LogTypes.Created, UserId, "Quotation", "Quotations", findip(), quotationId, "Successfully Submitted Quotations");






                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "Quot" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;


                    string qedate = Quoentry.QuotDate.ToString("dd-MM-yyyy");

                    var QuotData = com.QuotationData(quotationId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                    var item = QuotData.pdfItem.ToList();
                    var summary = QuotData;
                    var billsundry = QuotData.billsundry.ToList();

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(quotdata[34]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp, qtnid = quotationId, test = check } };

                }
                else if (action == "sendmail")
                {

                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = quotdata[12];
                    string CcMail = quotdata[13];
                    string InvoiceNo = "_Quote_" + Quoentry.QuotNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "Quotation").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "Quotation";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our quotation for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(quotationId, checkid), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully submitted Quotation.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = quotationId } };
                }
                else
                {
                    msg = "Successfully submitted Quotation.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = quotationId, test = check } };
                }
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }



        [QkAuthorize(Roles = "Dev,Quotation Entry")]
        public JsonResult CreateQuotationAutosave(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {

            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {


                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;





                var UserId = User.Identity.GetUserId();
                var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(quotdata[17]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                long? Prj = null;
                if (quotdata[18] != null && quotdata[18] != "")
                {
                    Prj = Convert.ToInt64(quotdata[18]);
                }
                long? quot = null;
                if (quotdata[39] != null && quotdata[39] != "")
                {
                    quot = Convert.ToInt64(quotdata[39]);
                }
                //sales entry
                Quotation Quoentry = new Quotation();
                if (quotdata[19] != null)
                {
                    string str = quotdata[19];
                    SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                    Quoentry.SaleType = Stype;
                }
                else
                {
                    Quoentry.SaleType = SaleType.Sale;
                }
                if (quotdata[40] != "autosave")
                {
                    Quoentry.QuotNo = GetQeNo(Quoentry.SaleType);
                    Quoentry.BillNo = InvoiceNo();
                }
                else
                {
                    Quoentry.QuotNo = Convert.ToInt64(quotdata[15]);
                    Quoentry.BillNo = quotdata[15] + "- autosave";
                }


                Quoentry.QuotDate = DateTime.Parse(quotdata[2], new CultureInfo("en-GB"));
                Quoentry.QuotCashier = quotdata[1] != "" ? Convert.ToInt64(quotdata[1]) : 0;
                Quoentry.Customer = Convert.ToInt64(quotdata[0]);
                Quoentry.QuotItems = Convert.ToInt32(quotdata[3]);
                Quoentry.QuotItemQuantity = (quotdata[4] == "NaN") ? 0 : Convert.ToDecimal(quotdata[4]);
                Quoentry.QuotSubTotal = Convert.ToDecimal(quotdata[8]);
                Quoentry.QuotTax = Convert.ToDecimal(quotdata[9]);
                Quoentry.QuotTaxAmount = Convert.ToDecimal(quotdata[5]);
                Quoentry.QuotDiscount = Convert.ToDecimal(quotdata[6]);
                Quoentry.QuotGrandTotal = Convert.ToDecimal(quotdata[7]);
                Quoentry.QuotNote = "";
                Quoentry.Mail = 0;
                Quoentry.QuotCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                Quoentry.CreatedUserId = UserId;
                Quoentry.Status = Status.active;
                Quoentry.TermsCondition = Convert.ToString(quotdata[11]);
                Quoentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "Quotation").Select(a => a.EmailTemplateID).FirstOrDefault();
                Quoentry.CompanyHeaderID = 0;
                Quoentry.Branch = Branch;
                Quoentry.QuotValidity = quotdata[10] == "" ? 0 : Convert.ToInt32(quotdata[10]);
                Quoentry.Remarks = quotdata[16];
                Quoentry.Project = Prj;
                Quoentry.ProTask = quotdata[27] != "" ? Convert.ToInt64(quotdata[27]) : 0;

                Quoentry.SalesType = Convert.ToInt64(quotdata[23]);
                Quoentry.PaymentTerms = (quotdata[24]);


                Quoentry.Ref1 = Convert.ToString(quotdata[29]);
                Quoentry.Ref2 = Convert.ToString(quotdata[30]);
                Quoentry.Ref3 = Convert.ToString(quotdata[31]);
                Quoentry.Ref4 = Convert.ToString(quotdata[32]);
                Quoentry.Ref5 = Convert.ToString(quotdata[33]);
                Quoentry.expdate = DateTime.Parse(quotdata[35], new CultureInfo("en-GB"));
                if (!String.IsNullOrEmpty(quotdata[36]))
                    Quoentry.leadsid = Convert.ToInt64(quotdata[36]);
                Quoentry.quotationstatus = Convert.ToInt32(quotdata[37]);
                //Quoentry.quotationtype = Convert.ToInt32()

                Quoentry.revision = Convert.ToString(quotdata[38]);
                Quoentry.quotationtype = quot;


                db.Quotations.Add(Quoentry);
                db.SaveChanges();
                Int64 quotationId = 0;
                Int64 checkid = 0;
                quotationId = Quoentry.QuotationId;
                if (Quoentry.SaleType == SaleType.Hire)
                {
                    HireDetail HDetils = new HireDetail();
                    HDetils.StartDate = DateTime.Parse(quotdata[20], new CultureInfo("en-GB"));
                    HDetils.EndDate = DateTime.Parse(quotdata[21], new CultureInfo("en-GB"));
                    HDetils.Section = "Quotation";
                    HDetils.Reference = quotationId;
                    HDetils.HireType = Convert.ToInt64(quotdata[22]);
                    db.HireDetails.Add(HDetils);
                    db.SaveChanges();
                }
                var CustomerName = db.Customers.Where(a => a.CustomerID == Quoentry.Customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();



                ////// add to SEItem
                string result = string.Empty;
                DataTable dtItem = new DataTable();
                dtItem.Columns.Add("ItemUnit");
                dtItem.Columns.Add("ItemUnitPrice");
                dtItem.Columns.Add("ItemQuantity");
                dtItem.Columns.Add("ItemSubTotal");
                dtItem.Columns.Add("ItemDiscount");
                dtItem.Columns.Add("ItemTax");
                dtItem.Columns.Add("ItemTaxAmount");
                dtItem.Columns.Add("ItemTotalAmount");
                dtItem.Columns.Add("ItemNote");
                dtItem.Columns.Add("Quotation");
                dtItem.Columns.Add("Item");


                foreach (var arr in array)
                {
                    DataRow dr = dtItem.NewRow();
                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = (arr[3] == "") ? 0 : Convert.ToDecimal(arr[3]);
                    dr["ItemQuantity"] = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                    dr["ItemTax"] = Convert.ToDecimal(arr[10]);
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                    if (arr.Length > 31)
                        dr["itemNote"] = Convert.ToString(arr[32].Replace("\n", "<br />"));
                    else
                        dr["itemNote"] = "";

                    dr["Quotation"] = quotationId;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    dtItem.Rows.Add(dr);

                    var item = Convert.ToInt32(arr[0]);
                    var chkbundle = db.ItemBundles.Where(a => a.mainItem == item).Select(a => a.ItemBundleId).FirstOrDefault();
                    if (chkbundle > 0)
                    {
                        var bunQuan = Convert.ToDecimal(arr[2]);
                        var itemBundle = (from g in db.ItemBundles
                                          join b in db.Items on g.mainItem equals b.ItemID
                                          where b.ItemID == item
                                          select new
                                          {
                                              g.ItemBundleId
                                          }).FirstOrDefault();

                        long typ = Convert.ToInt64(quotdata[22]);

                        var bundle = (from a in db.BundleItems
                                      join b in db.Items on a.ItemId equals b.ItemID
                                      join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                      from c in primary.DefaultIfEmpty()
                                      let hir = db.HireRates.Where(x => x.ItemId == b.ItemID && x.type == typ).Select(y => y.Rate).FirstOrDefault()
                                      where a.ItemBundle == itemBundle.ItemBundleId
                                      select new
                                      {
                                          b.ItemCode,
                                          b.ItemName,
                                          c.ItemUnitName,
                                          ItemUnitPrice = (Quoentry.SaleType == SaleType.Sale) ? a.ItemSubTotal : (hir == null ? 0 : hir),
                                          quantity = a.ItemQuantity,
                                          ItemSubTotal = a.ItemSubTotal,
                                          ItemTax = a.ItemTax,
                                          ItemTaxAmount = a.ItemTaxAmount,
                                          ItemTotalAmount = a.ItemTotalAmount,
                                          ItemUnit = a.ItemUnit,
                                          Item = a.ItemId
                                      }).ToList();
                        foreach (var bu in bundle)
                        {
                            var qua = (bunQuan * bu.quantity);
                            var ItemSubTotal = qua * bu.ItemUnitPrice;
                            var buTaxAmount = (ItemSubTotal * bu.ItemTax) / 100;

                            decimal itemtax = 0;
                            decimal taxamt = 0;
                            decimal totamt = 0;

                            itemtax = bu.ItemTax;
                            taxamt = buTaxAmount;
                            totamt = (buTaxAmount + ItemSubTotal);

                            DataRow dbu = dtItem.NewRow();
                            dbu["ItemUnit"] = bu.ItemUnit;
                            dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                            dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                            dbu["ItemSubTotal"] = ItemSubTotal;
                            // add parent itemid in discount for reference
                            dbu["ItemDiscount"] = item;
                            dbu["ItemTax"] = itemtax;
                            dbu["ItemTaxAmount"] = taxamt;
                            dbu["ItemTotalAmount"] = totamt;
                            dbu["itemNote"] = "-:{Bundle_Item}";
                            dbu["Quotation"] = quotationId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }
                }

                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypeQuotItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertQuotationItems", "@TableType");
                //// execute sql 
                QuickSoft.Helpers.DocumentTotals.RecomputeQuotation(db, quotationId); // forward-correctness: header = SUM(lines)
                db.Database.ExecuteSqlRaw(sql, parameter);

                if (bsmodel != null)
                {
                    foreach (var bs in bsmodel)
                    {
                        var qtB = new QtBillSundry
                        {
                            Quotation = quotationId,
                            BillSundry = bs.BillSundry,
                            BsValue = bs.BsValue,
                            AmountType = bs.AmountType,
                            BsType = bs.BsType,
                            BsAmount = bs.BsAmount,
                        };
                        db.QtBillSundrys.Add(qtB);
                        db.SaveChanges();

                    }
                }


                if (quotdata[25] != null && quotdata[25] != "0" && quotdata[25] != "" && quotdata[26] != null && quotdata[26] != "" && quotdata[26] != "0")
                {
                    ConvertTransactions ConTran = new ConvertTransactions();

                    ConTran.ConvertFrom = quotdata[26];
                    ConTran.ConvertTo = "Quote";
                    ConTran.From = Convert.ToInt64(quotdata[25]);
                    ConTran.To = quotationId;
                    ConTran.Status = 0;
                    ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    ConTran.CreatedBy = UserId;
                    ConTran.Branch = Convert.ToInt32(Branch);

                    db.ConvertTransactionss.Add(ConTran);
                    db.SaveChanges();
                    com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Convertion");
                }

                //Approved By
                var Appby = Convert.ToString(quotdata[28]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = quotationId;
                        approval.Type = "Quotation";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }

                com.addlog(LogTypes.Created, UserId, "Quotation", "Quotations", findip(), quotationId, "Successfully Submitted Quotations");






                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "Quot" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;


                    string qedate = Quoentry.QuotDate.ToString("dd-MM-yyyy");

                    var QuotData = com.QuotationData(quotationId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                    var item = QuotData.pdfItem.ToList();
                    var summary = QuotData;
                    var billsundry = QuotData.billsundry.ToList();

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(quotdata[34]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp, qtnid = quotationId } };

                }
                else if (action == "sendmail")
                {

                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = quotdata[12];
                    string CcMail = quotdata[13];
                    string InvoiceNo = "_Quote_" + Quoentry.QuotNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "Quotation").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "Quotation";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our quotation for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(quotationId, checkid), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully submitted Quotation.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = quotationId } };
                }
                else
                {
                    msg = "Successfully submitted Quotation.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = quotationId } };
                }
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        [QkAuthorize(Roles = "Dev,Edit Quotation")]
        public ActionResult Edit(long? id)
        {
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.SrcOfLead = QkSelect.List(lead, "ID", "Name");

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.image = (from b in db.quotationdocuments
                             join c in db.Quotations on b.quotationID equals c.QuotationId
                             where c.QuotationId == id
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.qutid,
                                 quotationID = b.quotationID,
                                 FileName = b.FileName,
                             }).ToList();
            var userpermission = User.IsInRole("All Quotation Entry");
            var UserId = User.Identity.GetUserId();
            Quotation quentry = db.Quotations.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.QuotationId == id).FirstOrDefault();

            if (quentry == null)
            {
                return NotFound();
            }
            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var CurrencyEnable = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            ViewBag.EnableCurrency = CurrencyEnable;

            var Currency = db.CurrencyMasters
              .Select(s => new
              {
                  Id = s.Id,
                  Name = s.CurrencyCode
              }).ToList();
            ViewBag.CurrencyVal = QkSelect.List(Currency, "Id", "Name");
            QuotationViewModel vmodel = new QuotationViewModel();
            var cust = (from a in db.Customers
                        join b in db.Accountss on a.Accounts equals b.AccountsID

                        where a.Type == CRMCustomerType.Customer &&
                        !a.CustomerName.StartsWith("OLD-")
                         && a.CustomerID == quentry.Customer
                        select new
                        {
                            a.CustomerID,
                            a.CustomerCode,
                            a.CustomerName
                        }).ToList().Select(s => new
                        {
                            CustomerID = s.CustomerID,
                            CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                        }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var leads = db.Customers
    .Where(o => o.Type == CRMCustomerType.Leads && o.CustomerID == quentry.leadsid)
  .Select(s => new
  {
      CustomerID = s.CustomerID,
      CustomerDetails = s.CustomerCode + " - " + s.CustomerName
  }).ToList();
            ViewBag.leads = QkSelect.List(leads, "CustomerID", "CustomerDetails");

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var quot = db.QuotationTypes
                             .Select(s => new
                             {
                                 ID = s.QuotId,
                                 Name = s.QuotType
                             })
                             .ToList();
            ViewBag.Quot = QkSelect.List(quot, "ID", "Name");
            var serv = db.servicetypes
                    .Select(s => new
                    {
                        ID = s.servicetypeid,
                        Name = s.title
                    })
                    .ToList();
            ViewBag.service = QkSelect.List(serv, "ID", "Name");

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;
            var pr = db.Projects
                            .Select(s => new
                            {
                                ID = s.ProjectId,
                                Name = s.ProCode + "-" + s.ProjectName
                            })
                            .ToList();
            ViewBag.Proj = QkSelect.List(pr, "ID", "Name");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Quotation").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaQuot = db.EnableSettings.Where(a => a.EnableType == "MLAQuot").FirstOrDefault();
            var MlaQuots = MlaQuot != null ? MlaQuot.Status : Status.inactive;
            ViewBag.MLAQuot = MlaQuots;

            vmodel = (from b in db.Quotations
                      join f in db.HireDetails on new { f1 = b.QuotationId, f2 = "Quotation" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.QuotationId == id
                      select new QuotationViewModel
                      {
                          QuotNo = b.QuotNo,
                          Customer = b.Customer,
                          QuotDate = b.QuotDate,
                          sourceoflead = b.sourceoflead,
                          BillNo = b.BillNo,
                          QuotCashier = b.QuotCashier,
                          QuotDiscount = b.QuotDiscount,
                          QuotGrandTotal = b.QuotGrandTotal,
                          TermsCondition = b.TermsCondition,
                          Currency = b.Currency,
                          DConvertionRate = b.ConvertionRate,

                          QuotValidity = b.QuotValidity,
                          QuotationType = b.quotationtype,
                          servicetype=b.servicetype,
                          Remarks = b.Remarks,
                          Branch = b.Branch,
                          Project = b.Project != null ? b.Project : null,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          SalesType = b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          PaymentTerms = b.PaymentTerms,
                          ProTask = b.ProTask,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          quotationexpdate = b.expdate,
                          lead = b.leadsid,
                          revision = b.revision
                      }).FirstOrDefault();
            companySet();
            ViewBag.preEntry = db.Quotations.Where(a => a.QuotationId < id && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.QuotationId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Quotations.Where(a => a.QuotationId > id && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.QuotationId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.PopUpAddCust = false;

            var EditPermission = User.IsInRole("Disable Quot Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "Quotation", UserId);


            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Quot" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummyQuotItems.Where(a => a.Quotation == id).FirstOrDefault();
            var QItem = db.QuotationItems.Where(a => a.Quotation == id).FirstOrDefault();
            if (QItem == null && DItem != null)
            {
                var DItems = db.DummyQuotItems.Where(a => a.Quotation == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    QuotationItem QItems = new QuotationItem();
                    QItems.ItemUnit = arr.ItemUnit;
                    QItems.ItemUnitPrice = arr.ItemUnitPrice;
                    QItems.ItemQuantity = arr.ItemQuantity;
                    QItems.ItemSubTotal = arr.ItemSubTotal;
                    QItems.ItemDiscount = arr.ItemDiscount;
                    QItems.ItemTax = arr.ItemTax;
                    QItems.ItemTaxAmount = arr.ItemTaxAmount;
                    QItems.ItemTotalAmount = arr.ItemTotalAmount;
                    QItems.ItemNote = arr.ItemNote;
                    QItems.Quotation = arr.Quotation;
                    QItems.Item = arr.Item;
                    db.QuotationItems.Add(QItems);
                    db.SaveChanges();
                }

                db.DummyQuotItems.RemoveRange(db.DummyQuotItems.Where(a => a.Quotation == id));
                db.SaveChanges();
                QuickSoft.Helpers.DocumentTotals.RecomputeQuotation(db, id); // forward-correctness: header = SUM(lines)
            }
            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            ViewBag.PrintLayout = PriLay;
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();

            var ref1 = db.Quotations
              .Select(s => new
              {
                  ID = s.Ref1,
                  Name = s.Ref1
              }).Distinct()
              .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", vmodel.Ref1);

            var ref2 = db.Quotations
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", vmodel.Ref2);

            var ref3 = db.Quotations
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", vmodel.Ref3);

            var ref4 = db.Quotations
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", vmodel.Ref4);

            var ref5 = db.Quotations
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", vmodel.Ref5);

            ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Edit Quotation")]
        public ActionResult Editmobile(long? id)
        {


            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.image = (from b in db.quotationdocuments
                             join c in db.Quotations on b.quotationID equals c.QuotationId
                             where c.QuotationId == id
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.qutid,
                                 quotationID = b.quotationID,
                                 FileName = b.FileName,
                             }).ToList();
            var userpermission = User.IsInRole("All Quotation Entry");
            var UserId = User.Identity.GetUserId();
            Quotation quentry = db.Quotations.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.QuotationId == id).FirstOrDefault();

            if (quentry == null)
            {
                return NotFound();
            }
            QuotationViewModel vmodel = new QuotationViewModel();
            var cust = db.Customers
                .Select(s => new
                {
                    CustomerID = s.CustomerID,
                    CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var leads = db.Customers
    .Where(o => o.Type == CRMCustomerType.Leads)
  .Select(s => new
  {
      CustomerID = s.CustomerID,
      CustomerDetails = s.CustomerCode + " - " + s.CustomerName
  }).ToList();
            ViewBag.leads = QkSelect.List(leads, "CustomerID", "CustomerDetails");

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var quot = db.QuotationTypes
                             .Select(s => new
                             {
                                 ID = s.QuotId,
                                 Name = s.QuotType
                             })
                             .ToList();
            ViewBag.Quot = QkSelect.List(quot, "ID", "Name");


            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;
            var pr = db.Projects
                            .Select(s => new
                            {
                                ID = s.ProjectId,
                                Name = s.ProCode + "-" + s.ProjectName
                            })
                            .ToList();
            ViewBag.Proj = QkSelect.List(pr, "ID", "Name");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Quotation").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaQuot = db.EnableSettings.Where(a => a.EnableType == "MLAQuot").FirstOrDefault();
            var MlaQuots = MlaQuot != null ? MlaQuot.Status : Status.inactive;
            ViewBag.MLAQuot = MlaQuots;

            vmodel = (from b in db.Quotations
                      join f in db.HireDetails on new { f1 = b.QuotationId, f2 = "Quotation" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.QuotationId == id
                      select new QuotationViewModel
                      {
                          QuotNo = b.QuotNo,
                          Customer = b.Customer,
                          QuotDate = b.QuotDate,
                          BillNo = b.BillNo,
                          QuotCashier = b.QuotCashier,
                          QuotDiscount = b.QuotDiscount,
                          QuotGrandTotal = b.QuotGrandTotal,
                          TermsCondition = b.TermsCondition,
                          QuotValidity = b.QuotValidity,
                          QuotationType = b.quotationtype,
                          Remarks = b.Remarks,
                          Branch = b.Branch,
                          Project = b.Project != null ? b.Project : null,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          SalesType = b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          PaymentTerms = b.PaymentTerms,
                          ProTask = b.ProTask,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          quotationexpdate = b.expdate,
                          lead = b.leadsid
                      }).FirstOrDefault();
            companySet();
            ViewBag.preEntry = db.Quotations.Where(a => a.QuotationId < id && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.QuotationId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Quotations.Where(a => a.QuotationId > id && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.QuotationId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.PopUpAddCust = false;

            var EditPermission = User.IsInRole("Disable Quot Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "Quotation", UserId);


            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Quot" && a.Status == Status.active).ToList();


            //dummy table operations
            var DItem = db.DummyQuotItems.Where(a => a.Quotation == id).FirstOrDefault();
            var QItem = db.QuotationItems.Where(a => a.Quotation == id).FirstOrDefault();
            if (QItem == null && DItem != null)
            {
                var DItems = db.DummyQuotItems.Where(a => a.Quotation == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    QuotationItem QItems = new QuotationItem();
                    QItems.ItemUnit = arr.ItemUnit;
                    QItems.ItemUnitPrice = arr.ItemUnitPrice;
                    QItems.ItemQuantity = arr.ItemQuantity;
                    QItems.ItemSubTotal = arr.ItemSubTotal;
                    QItems.ItemDiscount = arr.ItemDiscount;
                    QItems.ItemTax = arr.ItemTax;
                    QItems.ItemTaxAmount = arr.ItemTaxAmount;
                    QItems.ItemTotalAmount = arr.ItemTotalAmount;
                    QItems.ItemNote = arr.ItemNote;
                    QItems.Quotation = arr.Quotation;
                    QItems.Item = arr.Item;
                    db.QuotationItems.Add(QItems);
                    db.SaveChanges();
                }

                db.DummyQuotItems.RemoveRange(db.DummyQuotItems.Where(a => a.Quotation == id));
                db.SaveChanges();
                QuickSoft.Helpers.DocumentTotals.RecomputeQuotation(db, id); // forward-correctness: header = SUM(lines)
            }
            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            ViewBag.PrintLayout = PriLay;
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();
            ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");
            return View(vmodel);
        }

        [HttpGet]
        public ActionResult GetQEItems2(long QuoteEntryID, string ConvertTo)
        {
            var temp = db.ConvertTransactionss.Where(a => a.From == QuoteEntryID && a.ConvertFrom == "Quote" && a.ConvertTo == ConvertTo).Select(a => a.To);
            List<ItemList2> DVItems = new List<ItemList2>();
            List<ItemList2> temp6 = new List<ItemList2>();
            List<ItemList2> DVitemsGroupBy = new List<ItemList2>();
            List<ItemList2> RemainingItems = new List<ItemList2>();
            foreach (var tem in temp)
            {
                var temp2 = (from a in db.DvItems
                             join b in db.Items on a.Item equals b.ItemID into t1
                             from b in t1.DefaultIfEmpty()
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             where a.Dv == tem
                             select new ItemList2
                             {
                                 Item = a.Item,
                                 ItemQuantity = a.ItemQuantity,
                                 ItemUnit = a.ItemUnit,
                                 ItemUnitPrice = a.ItemUnitPrice,
                                 ItemTax = a.ItemTax,
                                 ItemSubTotal = a.ItemSubTotal,
                                 ItemTaxAmount = a.ItemTaxAmount,
                                 ItemDiscount = a.ItemDiscount,
                                 note = a.ItemNote.Replace("<br />", "\n"),
                                 ItemNote = a.ItemNote != null ? a.ItemNote : "",
                                 ItemTotalAmount = a.ItemTotalAmount,
                                 ItemCode = b.ItemCode,
                                 ItemName = b.ItemName,
                                 ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                 ItemUnitID = b.ItemUnitID,
                                 SubUnitId = b.SubUnitId,
                                 PriUnit = c.ItemUnitName,
                                 SubUnit = d.ItemUnitName,
                                 BasePrice = b.BasePrice,
                                 SellingPrice = b.SellingPrice,
                                 PurchasePrice = b.PurchasePrice,
                                 MRP = b.MRP
                             });
                DVItems.AddRange(temp2);
            }
            DVitemsGroupBy = (from a in DVItems
                              group new { a.Item, a.ItemQuantity, a.ItemUnit, a.ItemUnitPrice, a.ItemTax, a.ItemSubTotal, a.ItemTaxAmount, a.ItemDiscount, a.note, a.ItemNote, a.ItemTotalAmount, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnitID, a.SubUnitId, a.PriUnit, a.SubUnit, a.BasePrice, a.SellingPrice, a.PurchasePrice, a.MRP } by new { a.Item } into g
                              select new ItemList2
                              {
                                  Item = g.FirstOrDefault().Item,
                                  ItemQuantity = g.Sum(k => -k.ItemQuantity),
                                  ItemUnit = g.FirstOrDefault().ItemUnit,
                                  ItemUnitPrice = g.FirstOrDefault().ItemUnitPrice,
                                  ItemTax = g.FirstOrDefault().ItemTax,
                                  ItemSubTotal = g.Sum(k => -k.ItemSubTotal),
                                  ItemTaxAmount = g.Sum(k => -k.ItemTaxAmount),
                                  ItemDiscount = g.Sum(k => -k.ItemDiscount),
                                  note = g.FirstOrDefault().note,
                                  ItemNote = g.FirstOrDefault().ItemNote,
                                  ItemTotalAmount = g.Sum(k => -k.ItemTotalAmount),
                                  ItemCode = g.FirstOrDefault().ItemCode,
                                  ItemName = g.FirstOrDefault().ItemName,
                                  ItemWithCode = g.FirstOrDefault().ItemWithCode,
                                  ItemUnitID = g.FirstOrDefault().ItemUnitID,
                                  SubUnitId = g.FirstOrDefault().SubUnitId,
                                  PriUnit = g.FirstOrDefault().PriUnit,
                                  SubUnit = g.FirstOrDefault().SubUnit,
                                  BasePrice = g.FirstOrDefault().BasePrice,
                                  SellingPrice = g.FirstOrDefault().SellingPrice,
                                  PurchasePrice = g.FirstOrDefault().PurchasePrice,
                                  MRP = g.FirstOrDefault().MRP
                              }).ToList();

            var ConD = (from a in db.QuotationItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Quotation == QuoteEntryID && a.ItemNote != "-:{Bundle_Item}"
                        select new ItemList2
                        {
                            Item = a.Item,
                            ItemQuantity = a.ItemQuantity,
                            ItemUnit = a.ItemUnit,
                            ItemUnitPrice = a.ItemUnitPrice,
                            ItemTax = a.ItemTax,
                            ItemSubTotal = a.ItemSubTotal,
                            ItemTaxAmount = a.ItemTaxAmount,
                            ItemDiscount = a.ItemDiscount,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
                            ItemTotalAmount = a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            ItemUnitID = b.ItemUnitID,
                            SubUnitId = b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            BasePrice = b.BasePrice,
                            SellingPrice = b.SellingPrice,
                            PurchasePrice = b.PurchasePrice,
                            MRP = b.MRP
                        });
            DVitemsGroupBy.AddRange(ConD);
            RemainingItems = (from a in DVitemsGroupBy
                              group new { a.Item, a.ItemQuantity, a.ItemUnit, a.ItemUnitPrice, a.ItemTax, a.ItemSubTotal, a.ItemTaxAmount, a.ItemDiscount, a.note, a.ItemNote, a.ItemTotalAmount, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnitID, a.SubUnitId, a.PriUnit, a.SubUnit, a.BasePrice, a.SellingPrice, a.PurchasePrice, a.MRP } by new { a.Item } into g
                              select new ItemList2
                              {
                                  Item = g.FirstOrDefault().Item,
                                  ItemQuantity = g.Sum(k => k.ItemQuantity),
                                  ItemUnit = g.FirstOrDefault().ItemUnit,
                                  ItemUnitPrice = g.FirstOrDefault().ItemUnitPrice,
                                  ItemTax = g.FirstOrDefault().ItemTax,
                                  ItemSubTotal = g.Sum(k => k.ItemSubTotal),
                                  ItemTaxAmount = g.Sum(k => k.ItemTaxAmount),
                                  ItemDiscount = g.Sum(k => k.ItemDiscount),
                                  note = g.FirstOrDefault().note,
                                  ItemNote = g.FirstOrDefault().ItemNote,
                                  ItemTotalAmount = g.Sum(k => k.ItemTotalAmount),
                                  ItemCode = g.FirstOrDefault().ItemCode,
                                  ItemName = g.FirstOrDefault().ItemName,
                                  ItemWithCode = g.FirstOrDefault().ItemWithCode,
                                  ItemUnitID = g.FirstOrDefault().ItemUnitID,
                                  SubUnitId = g.FirstOrDefault().SubUnitId,
                                  PriUnit = g.FirstOrDefault().PriUnit,
                                  SubUnit = g.FirstOrDefault().SubUnit,
                                  BasePrice = g.FirstOrDefault().BasePrice,
                                  SellingPrice = g.FirstOrDefault().SellingPrice,
                                  PurchasePrice = g.FirstOrDefault().PurchasePrice,
                                  MRP = g.FirstOrDefault().MRP
                              }).ToList();
            RemainingItems = RemainingItems.Where(a => a.ItemQuantity != 0).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(RemainingItems);
            return Json(result);
        }

        [HttpGet]
        public ActionResult GetQEItemsmc(long QuoteEntryID, long mc)
        {
            var ConD = (from a in db.QuotationItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Quotation == QuoteEntryID && a.ItemNote != "-:{Bundle_Item}"
                        orderby a.QuotationItemId
                        select new
                        {
                            a.Item,
                            a.ItemQuantity,
                            a.ItemUnit,
                            a.ItemUnitPrice,
                            a.ItemTax,
                            a.ItemSubTotal,
                            a.ItemTaxAmount,
                            a.ItemDiscount,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
                            a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            b.BasePrice,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.MRP,
                            b.PricingStrategy,
                            b.slreq,
                            b.KeepStock,
                            b.ConFactor,
                            b.ItemID
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.ItemID,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.note,
                            o.ItemNote,
                            o.ItemTotalAmount,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.PriUnit,
                            o.SubUnit,
                            o.BasePrice,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.MRP,
                            o.PricingStrategy,
                            o.slreq,
                            o.KeepStock,
                            o.ConFactor,
                            total = com.GetItemWisestock(o.Item, mc) - (o.ItemQuantity * o.ConFactor)

                        });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }

        [HttpGet]
        public ActionResult GetQEItems(long QuoteEntryID, long? mc)
        {
            long mcc = 0;
            if (mc != null)
                mcc = (long)mc;

            var ConD = (from a in db.QuotationItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Quotation == QuoteEntryID && a.ItemNote != "-:{Bundle_Item}"
                        orderby a.QuotationItemId
                        select new
                        {
                            a.Item,
                            a.ItemQuantity,
                            a.ItemUnit,
                            a.ItemUnitPrice,
                            a.ItemTax,
                            a.ItemSubTotal,
                            a.ItemTaxAmount,
                            a.ItemDiscount,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
                            a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            b.BasePrice,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.MRP,
                            b.PricingStrategy,
                            b.slreq,
                            b.ItemID,
                            b.KeepStock,
                            b.ConFactor
                            // total=com.GetItemWisestock(a.Item,mcc)
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.ItemID,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.note,
                            o.ItemNote,
                            o.ItemTotalAmount,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.PriUnit,
                            o.SubUnit,
                            o.BasePrice,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.MRP,
                            o.PricingStrategy,
                            o.slreq,
                            o.KeepStock,
                            o.ConFactor,
                            total = 0,// com.GetItemWisestock(o.Item, mcc) - (o.ItemQuantity * o.ConFactor),

                        });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }

        public JsonResult SearchQuotation(string q, string x, string page, long? customer)
        {

            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Quotations
                                  join c in db.Customers on b.Customer equals c.CustomerID into cnts
                                  from c in cnts.DefaultIfEmpty()

                                  where (
                                  (customer == null || customer == b.Customer) &&
                                  b.BillNo.ToLower().Contains(q.ToLower()) || c.CustomerName.ToLower().Contains(q.ToLower())
                                   || c.CustomerName.StartsWith(q) || c.CustomerName.EndsWith(q))

                                  select new SelectFormat
                                  {
                                      text = b.BillNo + "-" + c.CustomerName,
                                      id = b.QuotationId
                                  }).OrderByDescending(a => a.id).Take(pageSize).ToList();

            }
            else
            {
                serialisedJson = (from b in db.Quotations
                                  join c in db.Customers on b.Customer equals c.CustomerID into cnts
                                  from c in cnts.DefaultIfEmpty()
                                  where (customer == null || customer == b.Customer)
                                  select new SelectFormat
                                  {
                                      text = b.BillNo + "-" + c.CustomerName,
                                      id = b.QuotationId
                                  }).OrderByDescending(a => a.id).Skip(skip).Take(pageSize).ToList();
                //serialisedJson = db.Customers.Select(b => new SelectFormat
                //    text = b.CustomerCode + "-" + b.CustomerName,
                //    id = b.CustomerID

            }//
            if (x == "All" || x == "Both" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (string.IsNullOrEmpty(q) && (x == "No" || (x == "Both" && start == 0)))
            {
                var initial = new SelectFormat() { id = -2, text = "--No Quotation--" };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);

        }

        [QkAuthorize(Roles = "Dev,Edit Quotation")]
        public JsonResult UpdateQuotation(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {
            bool stat = false;
            string msg;
            if (quotdata[16] != null)
            {
                Int64 checkid = 0;
                Int64 quotEntryId = Convert.ToInt64(quotdata[16]);
                Quotation Quoentry = db.Quotations.Find(quotEntryId);
                if (com.islocked("Quot", Quoentry.QuotDate))
                {
                    msg = "Quotation Is Locked";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                if (BillExist(Convert.ToString(quotdata[15])) && Convert.ToString(quotdata[15]) != Quoentry.BillNo)
                {
                    msg = "Invoice No. Already Exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                if (ModelState.IsValid)
                {
                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;


                    var UserId = User.Identity.GetUserId();
                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(quotdata[18]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    var EditPermission = User.IsInRole("Disable Quot Edit After Approval");
                    if (com.chkApproved(quotEntryId, EditPermission, "Quotation", UserId) == true)
                    {

                        long? Prj = null;
                        if (quotdata[19] != "")
                        {
                            Prj = Convert.ToInt64(quotdata[19]);
                        }
                        long? qut = null;
                        if (quotdata[38] != "")
                        {
                            qut = Convert.ToInt64(quotdata[38]);
                        }
                        //sales entry
                        if (quotdata[20] != null)
                        {
                            string str = quotdata[20];
                            SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                            Quoentry.SaleType = Stype;
                        }
                        else
                        {
                            Quoentry.SaleType = SaleType.Sale;
                        }
                        Quoentry.BillNo = quotdata[15];
                        Quoentry.QuotNo = Convert.ToInt64(quotdata[14]);
                        Quoentry.QuotDate = DateTime.Parse(quotdata[2], new CultureInfo("en-GB"));
                        Quoentry.QuotCashier = quotdata[1] != "" ? Convert.ToInt64(quotdata[1]) : 0;
                        Quoentry.Customer = Convert.ToInt64(quotdata[0]);
                        Quoentry.QuotItems = Convert.ToInt32(quotdata[3]);
                        Quoentry.QuotItemQuantity = Convert.ToDecimal(quotdata[4]);
                        Quoentry.QuotSubTotal = Convert.ToDecimal(quotdata[8]);
                        Quoentry.QuotTax = Convert.ToDecimal(quotdata[9]);
                        Quoentry.QuotTaxAmount = Convert.ToDecimal(quotdata[5]);
                        Quoentry.QuotDiscount = Convert.ToDecimal(quotdata[6]);
                        Quoentry.QuotGrandTotal = Convert.ToDecimal(quotdata[7]);
                        Quoentry.QuotNote = "";
                        Quoentry.Mail = 0;
                        Quoentry.Status = Status.active;
                        Quoentry.TermsCondition = Convert.ToString(quotdata[11]);
                        Quoentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "Quotation").Select(a => a.EmailTemplateID).FirstOrDefault();
                        Quoentry.CompanyHeaderID = 0;
                        Quoentry.Branch = Branch;
                        Quoentry.QuotValidity = quotdata[10] == "" ? 0 : Convert.ToInt32(quotdata[10]);
                        Quoentry.Remarks = quotdata[17];
                        Quoentry.Project = Prj;
                        Quoentry.SalesType = Convert.ToInt64(quotdata[24]);
                        Quoentry.PaymentTerms = quotdata[25];
                        Quoentry.ProTask = quotdata[26] != "" ? Convert.ToInt64(quotdata[26]) : 0;
                        Quoentry.expdate = DateTime.Parse(quotdata[34], new CultureInfo("en-GB"));
                        Quoentry.leadsid = Convert.ToInt64(quotdata[36]);
                        Quoentry.quotationstatus = Convert.ToInt32(quotdata[36]);
                        Quoentry.revision = Convert.ToString(quotdata[37]);
                        Quoentry.quotationtype = qut;
                        if (quotdata.Count() > 39)
                        {
                            if (quotdata[39] != null && quotdata[39] != "")
                            {
                                Quoentry.sourceoflead = Convert.ToInt64(quotdata[39]);
                            }
                            if (quotdata[40] != null && quotdata[40] != "")
                            {
                                Quoentry.Currency = Convert.ToInt64(quotdata[40]);
                            }
                            if (quotdata[41] != null && quotdata[41] != "")
                            {
                                Quoentry.ConvertionRate = quotdata[41];
                            }
                            if (quotdata[42] != null && quotdata[42] != "")
                            {
                                Quoentry.FCTotal = Convert.ToDecimal(quotdata[42]);
                            }
                            if (quotdata[43] != null && quotdata[43] != "")
                            {
                                Quoentry.servicetype = Convert.ToInt64(quotdata[43]);
                            }
                        }

                        Quoentry.Ref1 = Convert.ToString(quotdata[28]);
                        Quoentry.Ref2 = Convert.ToString(quotdata[29]);
                        Quoentry.Ref3 = Convert.ToString(quotdata[30]);
                        Quoentry.Ref4 = Convert.ToString(quotdata[31]);
                        Quoentry.Ref5 = Convert.ToString(quotdata[32]);
                        Quoentry.expdate = DateTime.Parse(quotdata[34], new CultureInfo("en-GB"));
                        if (!String.IsNullOrEmpty(quotdata[35]))
                            Quoentry.leadsid = Convert.ToInt64(quotdata[35]);
                        Quoentry.quotationstatus = Convert.ToInt32(quotdata[36]);
                        Quoentry.revision = Convert.ToString(quotdata[37]);

                        db.Entry(Quoentry).State = EntityState.Modified;
                        db.SaveChanges();
                        var HireItem = db.HireDetails.Where(a => a.Reference == quotEntryId && a.Section == "Quotation").FirstOrDefault();
                        if (HireItem != null)
                        {
                            db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == quotEntryId && a.Section == "Quotation"));
                            db.SaveChanges();
                        }

                        if (Quoentry.SaleType == SaleType.Hire)
                        {
                            HireDetail HDetils = new HireDetail();
                            HDetils.StartDate = DateTime.Parse(quotdata[21], new CultureInfo("en-GB"));
                            HDetils.EndDate = DateTime.Parse(quotdata[22], new CultureInfo("en-GB"));
                            HDetils.Section = "Quotation";
                            HDetils.Reference = quotEntryId;
                            HDetils.HireType = Convert.ToInt64(quotdata[23]);
                            db.HireDetails.Add(HDetils);
                            db.SaveChanges();
                        }
                        var QEItem = db.QuotationItems.Where(a => a.Quotation == quotEntryId).FirstOrDefault();
                        if (QEItem != null)
                        {
                            var QItems = db.QuotationItems.Where(a => a.Quotation == quotEntryId).ToList();
                            foreach (var arr in QItems)
                            {
                                //add to dummy table
                                DummyQuotItem dItem = new DummyQuotItem();
                                dItem.ItemUnit = arr.ItemUnit;
                                dItem.ItemUnitPrice = arr.ItemUnitPrice;
                                dItem.ItemQuantity = arr.ItemQuantity;
                                dItem.ItemSubTotal = arr.ItemSubTotal;
                                dItem.ItemDiscount = arr.ItemDiscount;
                                dItem.ItemTax = arr.ItemTax;
                                dItem.ItemTaxAmount = arr.ItemTaxAmount;
                                dItem.ItemTotalAmount = arr.ItemTotalAmount;
                                dItem.ItemNote = arr.ItemNote;
                                dItem.Quotation = arr.Quotation;
                                dItem.Item = arr.Item;
                                db.DummyQuotItems.Add(dItem);
                                db.SaveChanges();
                            }

                            db.QuotationItems.RemoveRange(db.QuotationItems.Where(a => a.Quotation == quotEntryId));
                            db.SaveChanges();

                        }


                        ////// add to SEItem
                        string result = string.Empty;
                        DataTable dtItem = new DataTable();
                        dtItem.Columns.Add("ItemUnit");
                        dtItem.Columns.Add("ItemUnitPrice");
                        dtItem.Columns.Add("ItemQuantity");
                        dtItem.Columns.Add("ItemSubTotal");
                        dtItem.Columns.Add("ItemDiscount");
                        dtItem.Columns.Add("ItemTax");
                        dtItem.Columns.Add("ItemTaxAmount");
                        dtItem.Columns.Add("ItemTotalAmount");
                        dtItem.Columns.Add("ItemNote");
                        dtItem.Columns.Add("Quotation");
                        dtItem.Columns.Add("Item");


                        foreach (var arr in array)
                        {
                            DataRow dr = dtItem.NewRow();
                            dr["ItemUnit"] = arr[1];
                            dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                            dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                            dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                            dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                            dr["ItemTax"] = Convert.ToDecimal(arr[10]);
                            dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                            dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                            if (arr.Length > 30)
                                dr["itemNote"] = Convert.ToString(arr[32].Replace("\n", "<br />"));
                            if (arr.Length > 36)
                            {
                                if (arr.Length > 36)
                                    dr["itemNote"] = Convert.ToString(arr[37]) + "||" + dr["itemNote"];
                            }
                            else if (arr.Length < 30)
                                dr["itemNote"] = Convert.ToString(arr[29]);
                            dr["Quotation"] = quotEntryId;
                            dr["Item"] = Convert.ToInt32(arr[0]);
                            dtItem.Rows.Add(dr);

                            var item = Convert.ToInt32(arr[0]);
                            var chkbundle = db.ItemBundles.Where(a => a.mainItem == item).Select(a => a.ItemBundleId).FirstOrDefault();
                            if (chkbundle > 0)
                            {
                                var bunQuan = Convert.ToDecimal(arr[2]);
                                var itemBundle = (from g in db.ItemBundles
                                                  join b in db.Items on g.mainItem equals b.ItemID
                                                  where b.ItemID == item
                                                  select new
                                                  {
                                                      g.ItemBundleId
                                                  }).FirstOrDefault();
                                long typ = Convert.ToInt64(quotdata[23]);
                                var bundle = (from a in db.BundleItems
                                              join b in db.Items on a.ItemId equals b.ItemID
                                              join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                              from c in primary.DefaultIfEmpty()
                                              let hir = db.HireRates.Where(x => x.ItemId == b.ItemID && x.type == typ).Select(y => y.Rate).FirstOrDefault()
                                              where a.ItemBundle == itemBundle.ItemBundleId
                                              select new
                                              {
                                                  b.ItemCode,
                                                  b.ItemName,
                                                  c.ItemUnitName,
                                                  ItemUnitPrice = (Quoentry.SaleType == SaleType.Sale) ? a.ItemSubTotal : hir,
                                                  quantity = a.ItemQuantity,
                                                  ItemSubTotal = a.ItemSubTotal,
                                                  ItemTax = a.ItemTax,
                                                  ItemTaxAmount = a.ItemTaxAmount,
                                                  ItemTotalAmount = a.ItemTotalAmount,
                                                  ItemUnit = a.ItemUnit,
                                                  Item = a.ItemId
                                              }).ToList();
                                foreach (var bu in bundle)
                                {
                                    var qua = (bunQuan * bu.quantity);
                                    var ItemSubTotal = qua * bu.ItemUnitPrice;
                                    var buTaxAmount = (ItemSubTotal * bu.ItemTax) / 100;

                                    decimal itemtax = 0;
                                    decimal taxamt = 0;
                                    decimal totamt = 0;

                                    itemtax = bu.ItemTax;
                                    taxamt = buTaxAmount;
                                    totamt = (buTaxAmount + ItemSubTotal);

                                    DataRow dbu = dtItem.NewRow();
                                    dbu["ItemUnit"] = bu.ItemUnit;
                                    dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                                    dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                                    dbu["ItemSubTotal"] = ItemSubTotal;
                                    // add parent itemid in discount for reference
                                    dbu["ItemDiscount"] = item;
                                    dbu["ItemTax"] = itemtax;
                                    dbu["ItemTaxAmount"] = taxamt;
                                    dbu["ItemTotalAmount"] = totamt;
                                    dbu["itemNote"] = "-:{Bundle_Item}";
                                    dbu["Quotation"] = quotEntryId;
                                    dbu["Item"] = bu.Item;
                                    dtItem.Rows.Add(dbu);
                                }
                            }
                        }

                        SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                        parameter.SqlDbType = SqlDbType.Structured;
                        parameter.TypeName = "TableTypeQuotItems";
                        //// execute sp sql 
                        string sql = String.Format("EXEC {0} {1};", "SP_InsertQuotationItems", "@TableType");
                        //// execute sql 
                QuickSoft.Helpers.DocumentTotals.RecomputeQuotation(db, quotEntryId); // forward-correctness: header = SUM(lines)
                        var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                        if (ret > 0)
                        {
                            db.DummyQuotItems.RemoveRange(db.DummyQuotItems.Where(a => a.Quotation == quotEntryId));
                            db.SaveChanges();
                        }


                        var QtBs = db.QtBillSundrys.Where(a => a.Quotation == quotEntryId).FirstOrDefault();
                        if (QtBs != null)
                        {
                            db.QtBillSundrys.RemoveRange(db.QtBillSundrys.Where(a => a.Quotation == quotEntryId));
                            db.SaveChanges();
                        }
                        if (bsmodel != null)
                        {
                            foreach (var bs in bsmodel)
                            {
                                var qtB = new QtBillSundry
                                {
                                    Quotation = quotEntryId,
                                    BillSundry = bs.BillSundry,
                                    BsValue = bs.BsValue,
                                    AmountType = bs.AmountType,
                                    BsType = bs.BsType,
                                    BsAmount = bs.BsAmount,
                                };
                                db.QtBillSundrys.Add(qtB);
                                db.SaveChanges();

                            }
                        }

                        //Approved By
                        var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                        var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == quotEntryId && a.Type == "Quotation").FirstOrDefault();
                        var QnPO = db.Approvals.Where(a => a.TransEntry == quotEntryId && a.Type == "Quotation").FirstOrDefault();
                        if (QnPO != null)
                        {
                            if (chkapp != null)
                            {
                                db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == quotEntryId && a.Type == "Quotation"));
                                db.SaveChanges();
                            }
                            else
                            {
                                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == quotEntryId && a.Type == "Quotation"));
                                db.SaveChanges();
                            }
                        }
                        var Appby = Convert.ToString(quotdata[27]);
                        if (Appby != null && Appby != "")
                        {
                            long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                            Approval approval = new Approval();
                            foreach (var emp in Approve)
                            {
                                approval.TransEntry = quotEntryId;
                                approval.Type = "Quotation";
                                approval.EmployeeId = emp;
                                db.Approvals.Add(approval);
                                db.SaveChanges();
                            }
                        }
                        com.addlog(LogTypes.Updated, UserId, "Quotation", "Quotations", findip(), quotEntryId, "Successfully Updated Quotations");
                    }
                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                    if (action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "Quot" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                        string qedate = Quoentry.QuotDate.ToString("dd-MM-yyyy");

                        var QuotData = com.QuotationData(quotEntryId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                        var item = QuotData.pdfItem.ToList();
                        var summary = QuotData;
                        var billsundry = QuotData.billsundry.ToList();

                        var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                        var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                        var def = (PriLay == Status.active) ? Convert.ToInt64(quotdata[33]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp, qtnid = quotEntryId } };

                    }
                    else if (action == "sendmail")
                    {

                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = quotdata[12];
                        string CcMail = quotdata[13];
                        string InvoiceNo = "_Quote_" + Quoentry.QuotNo;

                        var em = db.EmailTemplates.Where(a => a.Head == "Quotation").FirstOrDefault();
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;
                        }
                        else
                        {
                            message.Subject = "Quotation";
                            message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                                " <p>we are enclosing our quotation for the items / services as requested by you during our discussions.<br/></p> " +
                                " <p>Looking forward to hear from you.</p>";
                        }
                        sm.SendPdfMail(generatePdf(quotEntryId, checkid), ToMail, CcMail, InvoiceNo, message);

                        msg = "Successfully Updated Quotation.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = quotEntryId } };
                    }
                    else
                    {
                        msg = "Successfully Updated Quotation.";
                        stat = true;

                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = quotEntryId } };
                    }
                    //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    stat = false;
                    msg = "Looks like something went wrong. Please check your form.";
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }
        [HttpGet]
       // [QkAuthorize(Roles = "Dev,Download Quotation")]
        public ActionResult Download(long id, long? ItemId)
        {

            var Data = db.Quotations.Where(s => s.QuotationId == id).FirstOrDefault();
            var custname = db.Customers.Where(s => s.CustomerID == Data.Customer).Select(a => a.CustomerName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;
            ViewBag.edit = "";
            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id, ItemId), "inactive");
            return File(ms, "application/pdf", "Quotation" + "-" + custname + "-" + billno + ".pdf");
        }
        public StringBuilder generatePdf(long quotationId, long? checkid)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var QuotData = com.QuotationData(quotationId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck, checkid);
            var item = QuotData.pdfItem.ToList();
            var summary = QuotData;
            var billsundry = QuotData.billsundry.ToList();


            return com.generatepdf(quotationId, summary, item, billsundry, "Quote");
        }



        //                   where b.QuotationId == quotationId // b.Customer == customer
        //                       BillNo = b.BillNo,
        //                       QuotNo = b.QuotNo,
        //                       Date = b.QuotDate,
        //                       QuotValidity = b.QuotValidity,
        //                       QuotGrandTotal = b.QuotGrandTotal,
        //                       PartyName = c.CustomerName,
        //                       CustomerEmail = d.EmailId,
        //                       Address = d.Address,
        //                       City = d.City,
        //                       SubTotal = b.QuotSubTotal,
        //                       Discount = b.QuotDiscount,
        //                       tc = b.QuotNote,
        //                       TaxAmount = b.QuotTaxAmount,
        //                       State = d.State,
        //                       Country = d.Country,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       GrandTotal = b.QuotGrandTotal,
        //                       TRN = c.TaxRegNo,
        //                       Email = d.EmailId,
        //                       Zip = d.Zip,
        //                       Phone = d.Phone,
        //                       Mobile = d.Mobile,
        //                       b.TermsCondition,
        //                       b.Remarks,
        //                       PrjNameCode = f.ProCode + "-" + f.ProjectName,
        //                       f.ProjectName,
        //                       f.ProCode,


        //                     where b.Quotation == quotationId && b.ItemNote != "-:{Bundle_Item}"
        //                         ItemUnitPrice = b.ItemUnitPrice,
        //                         ItemQuantity = b.ItemQuantity,
        //                         ItemSubTotal = b.ItemSubTotal,
        //                         ItemNote = b.ItemNote,
        //                         ItemTax = b.ItemTax,
        //                         ItemTaxAmount = b.ItemTaxAmount,
        //                         ItemTotalAmount = b.ItemTotalAmount,
        //                         ItemID = b.Item,
        //                         bundleitem = (from ab in db.QuotationItems
        //                                       where ab.Quotation == quotationId && ab.ItemNote == "-:{Bundle_Item}"
        //                                       && b.Item == ab.ItemDiscount

        //                                           bb.ItemCode,
        //                                           bb.ItemName,
        //                                           cb.ItemUnitName,
        //                                           ItemUnitPrice = ab.ItemUnitPrice,
        //                                           quantity = ab.ItemQuantity,
        //                                           ItemSubTotal = ab.ItemSubTotal,
        //                                           ItemTax = ab.ItemTax,
        //                                           ItemTaxAmount = ab.ItemTaxAmount,
        //                                           ItemTotalAmount = ab.ItemTotalAmount,

        //                                           ab.Item,
        //                                           ab.ItemQuantity,
        //                                           ab.ItemUnit,

        //                                           ItemDiscount = 0,

        //                                           ItemNote = ab.ItemNote,
        //                                           ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                           bb.ItemUnitID,
        //                                           bb.SubUnitId,
        //                                           PriUnit = cb.ItemUnitName,
        //                                           SubUnit = bd.ItemUnitName,
        //                                           bb.ItemArabic
        //                                       }).ToList()


        //        AmountType = b.AmountType,
        //        BsAmount = b.BsAmount,
        //        BsType = b.BsType,
        //        BsValue = b.BsValue != null ? b.BsValue : 0,
        //        BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()

        //    .Select(s => new
        //        CName = s.CPName,
        //        CAddress = s.CPAddress,
        //        CEmail = s.CPEmail,
        //        CTaxRegNo = s.TRN,
        //        CPhone = s.CPPhone,
        //        s.CPMobile,
        //        CLogo = s.CPLogo,



        //                "<td width='50%'> " +
        //                "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +

        //                    sb.Append("<img width='40px' height='70px' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + item.ItemID + "/" + item.FileName) + "'/>");






        //}``

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Quotation List,My ProTask")]
        public ActionResult GetQuotation(long? servicetype,string BillNo, long? QuotationType, string FromDate, string ToDate, long? customer, long? salesperson, long? project, string Stats, string user, int? Validity, string Saletype, long? HireType, string appstat, long? Task, string Mobile, long? sourceoflead)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

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

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var fromv = "Quote";

            var Tosales = "Sale";
            var ToPFA = "ProForma";
            var ToDVN = "DVNote";
            var ToSO = "SOrder";
            var ToBoq = "Boq";
            Status st = new Status();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };

            ApprovalStatus AppSt = new ApprovalStatus();
            if (appstat != "")
            {
                if (appstat == "0")
                {
                    AppSt = ApprovalStatus.Approved;
                }
                else if (appstat == "1")
                {
                    AppSt = ApprovalStatus.Rejected;
                }
                else
                {
                    AppSt = ApprovalStatus.PendingApproval;
                }
            };
            var userpermission = User.IsInRole("All Quotation Entry") || User.IsInRole("My ProTask");
            var UserId = User.Identity.GetUserId();
            SaleType St = new SaleType();
            if (Saletype != "")
            {
                St = (Saletype == "2") ? SaleType.Hire : SaleType.Sale;
            };

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uQuotationView = User.IsInRole("View Quotation");
            var uEdit = User.IsInRole("Edit Quotation");
            var uDownload = User.IsInRole("Download Quotation");
            var uDelete = User.IsInRole("Delete Quotation");
            var uQuotationEntry = User.IsInRole("Quotation Entry");


            var QuotToSale = db.EnableSettings.Where(a => a.EnableType == "QuotToSale").FirstOrDefault();
            var QuotToSales = QuotToSale != null ? QuotToSale.Status : Status.inactive;

            var QuotToPForma = db.EnableSettings.Where(a => a.EnableType == "QuotToPForma").FirstOrDefault();
            var QuotToPFormas = QuotToPForma != null ? QuotToPForma.Status : Status.inactive;

            var QuotToDvNote = db.EnableSettings.Where(a => a.EnableType == "QuotToDvNote").FirstOrDefault();
            var QuotToDvNotes = QuotToDvNote != null ? QuotToDvNote.Status : Status.inactive;

            var QuotToSOrder = db.EnableSettings.Where(a => a.EnableType == "QuotToSOrder").FirstOrDefault();
            var QuotToSOrders = QuotToSOrder != null ? QuotToSOrder.Status : Status.inactive;

            var QuotToBoq = db.EnableSettings.Where(a => a.EnableType == "Boq").FirstOrDefault();
            var QuotToBoqs = QuotToBoq != null ? QuotToBoq.Status : Status.inactive;


            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets and the five `qs/pfa/dvn/sor/bo` ConvertTransactionss subqueries).
            // Split SERVER from CLIENT: materialize only entity columns + simple scalars (left-joined entity
            // access like Customer/EmpName/user stays server-side) into serverRows, then build client lookups
            // keyed by QuotationId and re-project client-side with the SAME member names + order.
            var serverQuery = (from b in db.Quotations
                     join c in db.Customers on b.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()



                     join k in db.QuotationTypes on b.quotationtype equals k.QuotId into qut
                     from k in qut.DefaultIfEmpty()
                     join e in db.Employees on b.QuotCashier equals e.EmployeeId into emp
                     from e in emp.DefaultIfEmpty()
                     join f in db.Projects on b.Project equals f.ProjectId into prj
                     from f in prj.DefaultIfEmpty()

                     join h in db.Users on b.CreatedUserId equals h.Id
                     join i in db.HireDetails on new { i1 = b.QuotationId, i2 = "Quotation" }
                     equals new { i1 = i.Reference, i2 = i.Section } into hir
                     from i in hir.DefaultIfEmpty()
                     join j in db.ProTasks on b.ProTask equals j.ProTaskId into task
                     from j in task.DefaultIfEmpty()

                         // qs/pfa/dvn/sor/bo (ConvertTransactionss .FirstOrDefault subqueries) and
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are all computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.

                     where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                     (QuotationType == 0 || QuotationType == null || b.quotationtype == QuotationType) &&
                     (customer == 0 || customer == null || b.Customer == customer) && (project == 0 || project == null || b.Project == project) &&
                     (salesperson == 0 || salesperson == null || e.EmployeeId == salesperson) &&
                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(b.QuotDate, fdate) <= 0) &&
                     (ToDate == "" || FromDate == null || EF.Functions.DateDiffDay(b.QuotDate, tdate) >= 0)
                     && (Stats == null || b.Status == st)
                     //&& (user == null || user == "" || h.Id == user)
                     && (Validity == null || Validity == 0 || b.QuotValidity == Validity)
                     && (userpermission == true || c.SalesPerson == empl.EmployeeId || b.QuotCashier == empl.EmployeeId)
                     && (Saletype == "" || Saletype == null || St == b.SaleType) && (HireType == 0 || HireType == null || HireType == i.HireType)
                     && (Task == 0 || Task == null || j.ProTaskId == Task)
                       && (sourceoflead == 0 || sourceoflead == null || c.SourceOfLead == sourceoflead)
  && (servicetype == 0 || servicetype == null ||b.servicetype == servicetype)

                     select new
                     {
                         b.QuotationId,
                         b.QuotNo,
                         b.BillNo,

                         b.QuotDate,
                         b.QuotItems,
                         b.QuotDiscount,
                         b.QuotGrandTotal,
                         b.QuotTax,
                         b.QuotTaxAmount,
                         b.QuotItemQuantity,
                         b.QuotValidity,
                         k.QuotType,
                         b.Remarks,
                         b.Project,
                         ProjectName = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                         EmpName = e.FirstName + " " + e.LastName,
                         Customer = c.CustomerCode + " - " + c.CustomerName,
                         user = h.UserName,
                         validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.QuotDate, (b.QuotValidity == null) ? 0 : b.QuotValidity + 1)) ? "Active" : "Expired",
                         SaleType = b.SaleType,
                         Dev = uDev,
                         Details = uQuotationView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         QuotationEntry = uQuotationEntry,
                         Task = (j.TaskName != null && j.TaskName != "") ? j.TaskCode + "-" + j.TaskName : "",
                         CreatedDate = b.QuotCreatedDate

                     });

            // Performance (audit P2): the page-load path (no search, sorting on a plain entity column)
            // pages on the SERVER — only one page of rows is materialized instead of all ~27K, and the
            // three approval/convert lookups shrink to that page. Any search or computed-column sort
            // falls back to the original materialize-all path, so behaviour is unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            { "QuotationId","QuotNo","BillNo","QuotDate","QuotItems","QuotDiscount","QuotGrandTotal","QuotTax","QuotTaxAmount","QuotItemQuantity","QuotValidity","CreatedDate" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0
                && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn)
                    ? serverQuery.OrderBy("QuotationId asc")
                    : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by QuotationId (missing key -> empty/absent, no KeyNotFound).
            var quotIds = serverRows.Select(o => o.QuotationId).ToList();
            // The five convert markers: latest-or-any ConvertTransactionss row per (QuotationId, ConvertTo).
            var convLookup = db.ConvertTransactionss
                .Where(ap => ap.ConvertFrom == fromv && quotIds.Contains(ap.From)
                       && (ap.ConvertTo == Tosales || ap.ConvertTo == ToPFA || ap.ConvertTo == ToDVN || ap.ConvertTo == ToSO || ap.ConvertTo == ToBoq))
                .Select(ap => new { ap.From, ap.ConvertTo })
                .ToList()
                .ToLookup(ap => ap.From);
            // app = approver EmployeeIds for the quotation (nested collection, keyed by TransEntry == QuotationId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "Quotation" && quotIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "Quotation" && quotIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per quotation.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var conv = convLookup[o.QuotationId];
                         var SaleConvert = conv.Where(x => x.ConvertTo == Tosales).Select(x => x.ConvertTo).FirstOrDefault();
                         var PFAConvert = conv.Where(x => x.ConvertTo == ToPFA).Select(x => x.ConvertTo).FirstOrDefault();
                         var DVNConvert = conv.Where(x => x.ConvertTo == ToDVN).Select(x => x.ConvertTo).FirstOrDefault();
                         var SOConvert = conv.Where(x => x.ConvertTo == ToSO).Select(x => x.ConvertTo).FirstOrDefault();
                         var BoqConvert = conv.Where(x => x.ConvertTo == ToBoq).Select(x => x.ConvertTo).FirstOrDefault();
                         var app = appLookup[o.QuotationId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.QuotationId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.QuotationId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                         {

                         SaleConvert = SaleConvert,
                         PFAConvert = PFAConvert,
                         DVNConvert = DVNConvert,
                         SOConvert = SOConvert,
                         BoqConvert = BoqConvert,

                         o.QuotationId,
                         o.QuotNo,
                         o.BillNo,

                         o.QuotDate,
                         o.QuotItems,
                         o.QuotDiscount,
                         o.QuotGrandTotal,
                         o.QuotTax,
                         o.QuotTaxAmount,
                         o.QuotItemQuantity,
                         o.QuotValidity,
                         o.QuotType,
                         o.Remarks,
                         o.Project,
                         o.ProjectName,
                         o.Task,
                         o.EmpName,
                         o.Customer,
                         o.user,
                         o.validity,
                         o.SaleType,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.QuotationEntry,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (chkAppStatus != null && chkAppStatus.Count != 0 && chkAppStatus.Count() >= 1 ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         QuotToSale = (SaleConvert != null && QuotToSales == Status.active) ? false : true,
                         QuotToPForma = (PFAConvert != null && QuotToPFormas == Status.active) ? false : true,
                         QuotToDvNote = (DVNConvert != null && QuotToDvNotes == Status.active) ? false : true,
                         QuotToSOrder = (SOConvert != null && QuotToSOrders == Status.active) ? false : true,
                         QuotToBoq = (BoqConvert != null && QuotToBoqs == Status.active) ? false : true,
                         };
                     });

            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Equals(search.ToLower())
                                 //p.Customer.ToString().ToLower().Contains(search.ToLower())
                                 );
            }

            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!fastPage) { recordsTotal = v.Count(); }
            var data = fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }


        public JsonResult GetFillDetails(long CustId)
        {

            var ConD = (from a in db.Customers
                        join b in db.SalesEntrys
                        on a.CustomerID equals b.Customer into primary
                        from b in primary.DefaultIfEmpty()
                        join c in db.Employees
                        on a.SalesPerson equals c.EmployeeId into secondary
                        from c in secondary.DefaultIfEmpty()

                        where a.CustomerID == CustId

                        let Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0)
                        let Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0)

                        select new
                        {
                            a.CreditLimit,
                            currentbalance = (Debit > Credit) ? ((Debit - Credit)) : 0,
                            ddlEmployee = c.EmployeeId,
                            employeename = c.FirstName + " " + c.MiddleName + " " + c.LastName,

                        });
            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetQtBillSundry(long quoteID)
        {
            var QtBs = (from a in db.QtBillSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.Quotation == quoteID
                        select new
                        {
                            a.AmountType,
                            a.BillSundry,
                            a.BsAmount,
                            a.BsType,
                            a.BsValue,
                            c.BSName,
                            //a.PEBillSundryId,
                            //a.PurchaseEntry,
                            //c.BillSundryId
                        }).ToList();
            return Json(QtBs);
        }
        [HttpGet]
        public JsonResult getservicenote(long? servid)
        {
            if (servid == null)
                servid = 0;
            var nots = (from a in db.servicetypes

                        where a.servicetypeid == servid
                        select new
                        {
                            a.note
                        }).ToList().Select(o=>o.note).FirstOrDefault();
            if(nots==null)
            {
                nots = "";
            }
            return Json(nots);
        }

        [HttpGet]
        public ActionResult GetCustomer(int CustID)
        {
            var email = (from b in db.Quotations
                         join c in db.Customers on b.Customer equals c.CustomerID into cust
                         from c in cust.DefaultIfEmpty()
                         join d in db.Contacts on c.Contact equals d.ContactID into cnt
                         from d in cnt.DefaultIfEmpty()
                         where b.Customer == CustID
                         select new
                         {
                             CustomerEmail = d.EmailId,
                         }).FirstOrDefault();
            return Json(email);

        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Quotation")]
        public ActionResult Details(long? id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            QuotationViewModel vmodel = new QuotationViewModel();
            vmodel = (from b in db.Quotations
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.QuotCashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join f in db.Projects on b.Project equals f.ProjectId into prjct
                      from f in prjct.DefaultIfEmpty()
                      join t in db.SalesTypes on b.SalesType equals t.Id into ptype
                      from t in ptype.DefaultIfEmpty()
                      join u in db.HireDetails on new { h1 = b.QuotationId, h2 = "Quotation" }
                      equals new { h1 = u.Reference, h2 = u.Section } into hir
                      from u in hir.DefaultIfEmpty()
                      join v in db.HireTypes on u.HireType equals v.HireTypeId into htyp
                      from v in htyp.DefaultIfEmpty()
                      where b.QuotationId == id
                      select new QuotationViewModel
                      {
                          CustomerName = c.CustomerCode + " - " + c.CustomerName,
                          QuotNo = b.QuotNo,
                          BillNo = b.BillNo,
                          QuotDate = b.QuotDate,
                          TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
                          EmployeeName = e.FirstName + " " + e.LastName,
                          QuotDiscount = b.QuotDiscount,
                          QuotGrandTotal = b.QuotGrandTotal,
                          QuotValidity = b.QuotValidity,
                          QuotSubTotal = b.QuotSubTotal,
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          ProjectName = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                          //QuotCashier = e.Name,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                          PaymentTerms = b.PaymentTerms,
                          QTypeName = (b.SaleType == SaleType.Sale) ? "Sale" : ((b.SaleType == SaleType.Hire) ? "Hire" : "POS"),
                          QuotTypeName = t.Name,
                          EmailId = d.EmailId,

                          HType = (u != null) ? v.Name : "",
                          StartDate = (u != null) ? u.StartDate : null,
                          EndDate = (u != null) ? u.EndDate : null,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "Quotation"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();
            vmodel.QuotItem = db.QuotationItems.Where(a => a.Quotation == id && a.ItemNote != "-:{Bundle_Item}")
            .Select(b => new QuotItemViewModel
            {
                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemTax = b.ItemTax,
                ItemNote = b.ItemNote != null ? b.ItemNote : "",
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.QuotationItems
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.Quotation == id && ab.ItemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();
            vmodel.QtBillSundry = db.QtBillSundrys.Where(a => a.Quotation == id)
                .Select(b => new QtBillSundryViewModel
                {
                    AmountType = b.AmountType,
                    BsAmount = b.BsAmount,
                    BsType = b.BsType,
                    BsValue = b.BsValue,
                    Type = b.BsType == 0 ? "Add" : "Less",
                    AmtType = b.AmountType == 0 ? "" : "%",
                    BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
                }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Quot" && a.Status == Status.active).ToList();

            return View(vmodel);
        }
        private string InvoiceNo(Int64 QENo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == "Hire") ? "HireQuotation" : "Quotation";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            SaleType type = (section != "Hire") ? SaleType.Sale : SaleType.Hire;
            if (billNo == null)
            {
                if ((db.Quotations.Where(q => q.SaleType == type).Select(p => p.QuotNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    QENo = db.Quotations.Where(q => q.SaleType == type).Max(p => p.QuotNo + 1);
                    billNo = companyPrefix + QENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(QENo, billNo, section);
                    }
                }
            }
            else
            {
                QENo = QENo + 1;
                billNo = companyPrefix + QENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(QENo, billNo, section);
                }

            }
            return billNo;
        }

        private bool BillExist(string QENo)
        {
            var Exists = db.Quotations.Any(c => c.BillNo == QENo);
            bool res = (Exists) ? true : false;
            return res;
        }

        [QkAuthorize(Roles = "Dev,Delete Quotation")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete

            var userpermission = (User.IsInRole("All Quotation Entry") == true) ? true : (User.IsInRole("My ProTask") == true) ? true : false;
            var UserId = User.Identity.GetUserId();
            Quotation Quot = db.Quotations.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.QuotationId == id).FirstOrDefault();


            if (Quot == null)
            {
                return NotFound();
            }
            return PartialView(Quot);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Quotation")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Quotation.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Quotation")]
        public ActionResult DeleteAllQuotation(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteQuot(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Quotation", true);
            return RedirectToAction("Index", "Quotation");
        }

        private Boolean DeleteQuot(long saleId)
        {
            var Msg = chkDeleteWithMsg(saleId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(saleId);
            }
        }
        public JsonResult deleteexistingquotation(long qutno)
        {
            var UserId = User.Identity.GetUserId();

            Quotation QSum = db.Quotations.Where(a => a.QuotNo == qutno && a.CreatedUserId == UserId).FirstOrDefault();
            if (QSum != null)
            {
                var z = db.Quotations.Where(b => b.QuotNo == qutno).Select(b => b.QuotationId).FirstOrDefault();

                if (z != 0)
                {
                    db.QuotationItems.RemoveRange(db.QuotationItems.Where(a => a.Quotation == z));
                    db.SaveChanges();
                }
                if (QSum != null)
                {
                    db.Quotations.RemoveRange(db.Quotations.Where(a => a.QuotNo == qutno));
                    db.SaveChanges();
                }

            }
            string msg = "sucess";
            return Json(msg);
        }



        private Boolean DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            Quotation QSum = db.Quotations.Find(id);

            var QPro = db.QuotationItems.Where(a => a.Quotation == id).FirstOrDefault();
            if (QPro != null)
            {
                db.QuotationItems.RemoveRange(db.QuotationItems.Where(a => a.Quotation == id));
            }
            var qtBs = db.QtBillSundrys.Where(a => a.Quotation == id).FirstOrDefault();
            if (qtBs != null)
            {
                db.QtBillSundrys.RemoveRange(db.QtBillSundrys.Where(a => a.Quotation == id));

            }
            var HireItem = db.HireDetails.Where(a => a.Reference == id && a.Section == "Quotation").FirstOrDefault();
            if (HireItem != null)
            {
                db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == id && a.Section == "Quotation"));
            }
            var ConSale = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Quote").FirstOrDefault();
            if (ConSale != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Quote"));
            }

            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Quotation").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "Quotation"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Quotation").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Quotation"));
            }

            db.Quotations.Remove(QSum);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "Quotation", "Quotations", findip(), QSum.QuotationId, "Successfully Deleted Quotations");
            return true;
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "QuotExtend").FirstOrDefault();
            var Ext1 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "Quote" && x.ConvertTo == "ProForma").FirstOrDefault();
            var Ext2 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "Quote" && x.ConvertTo == "Sale").FirstOrDefault();
            var Ext3 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "Quote" && x.ConvertTo == "DVNote").FirstOrDefault();
            if (Ext != null)
            {
                var inv = db.Quotations.Where(x => x.QuotationId == Ext.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Extended to Quotation : " + inv + ".";
            }
            else if (Ext1 != null)
            {
                var inv = db.ProFormas.Where(x => x.ProFormaId == Ext1.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to ProForma : " + inv + "";
            }
            else if (Ext2 != null)
            {
                var inv = db.SalesEntrys.Where(x => x.SalesEntryId == Ext2.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Sale : " + inv + "";
            }
            else if (Ext3 != null)
            {
                var inv = db.Deliverynotes.Where(x => x.DeliverynoteId == Ext3.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Deliverynote : " + inv + "";
            }
            else
            {
                msg = null;
            }
            return msg;
        }


        private long GetQeNo(SaleType type)
        {
            Int64 QENo = 0;
            string prefix = (type == SaleType.Hire) ? "HireQuotation" : "Quotation";
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if ((db.Quotations.Where(a => a.SaleType == type).Select(p => p.QuotNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    QENo = 1;
                }
                else
                {
                    QENo = number;
                }
            }
            else
            {
                QENo = db.Quotations.Where(a => a.SaleType == type).Max(p => p.QuotNo + 1);
            }

            return QENo;
        }

        [HttpPost]
        public ActionResult GetHireInvoiceNum(string hiretype)
        {
            string hirerate = (hiretype == "Hire") ? InvoiceNo(0, null, hiretype) : InvoiceNo();
            return Json(hirerate);
        }

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

            var MR = db.Quotations.Where(a => a.QuotationId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "Quotation").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))

            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = MR.CreatedUserId;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "Quotation";

                db.ApprovalUpdates.Add(AppUp);
                db.SaveChanges();

                stat = true;
                msg = "Successfully Updated Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                stat = false;
                msg = "Updating Same Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        [HttpPost]
        public ActionResult GetAllStatusUpdation(long MCId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var UserView = (from b in db.ApprovalUpdates
                            join c in db.Users on b.ApprovedBy equals c.Id
                            join d in db.Quotations on b.TransEntry equals d.QuotationId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedUserId equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "Quotation"
                            select new
                            {
                                b.ApprovalUpdateID,
                                b.TransEntry,
                                b.Status,
                                b.ApprovalStatus,
                                b.CreatedDate,
                                b.Note,
                                RequestBy = u.UserName,
                                c.UserName,
                                ApprovedBy = "" //e.FirstName + " " + e.LastName,
                            }).Distinct().ToList().Select(o => new
                            {
                                o.ApprovalUpdateID,
                                o.TransEntry,
                                o.Status,
                                ApprovalStatus = Enum.GetName(typeof(ApprovalStatus), o.ApprovalStatus),

                                o.ApprovedBy,
                                o.RequestBy,
                                User = o.UserName, //db.Users.Where(a => a.Id == o.CreatedUser).Select(a => a.UserName).FirstOrDefault(),
                                o.CreatedDate,
                                Remarks = o.Note
                            });
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
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
