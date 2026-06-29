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
    public class EstimateController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public EstimateController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Quotation 
        [QkAuthorize(Roles = "Dev,Quotation List")]
        public ActionResult Index()
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
        [QkAuthorize(Roles = "Dev,Quotation Entry")]
        public ActionResult Create(long? id, string type)
        {
            
        
            var Quotentry = new EstimateViewModel
            {
                BillNo = InvoiceNo(),
                EsttDate = System.DateTime.Now.ToString("dd-MM-yyyy"),
               
                };
            
           
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


  
           

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            ViewBag.Proj = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "--No Project--"},
                                }, "Value", "Text");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

           

           
            companySet();

            var UserId = User.Identity.GetUserId();
            

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

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            
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
        public JsonResult CreateEstimate(EstimateViewModel vm, string fnval)
        {
            string msg = "";
            bool stat = false;
            var UserId = User.Identity.GetUserId();
            Estimate es = new Estimate
            {

                QuotNo = vm.QuotNo,
                BillNo = vm.BillNo,

                EsttDate = DateTime.Parse(vm.EsttDate.ToString(), new CultureInfo("en-GB")),// vm.EsttDate,// DateTime.Parse(vm.EsttDate, new CultureInfo("en-GB")),
                Customer = vm.Customer,
                CreatedUser = UserId,
                QuotGrandTotal = vm.QuotGrandTotal,
        Remarks=vm.Remarks,
       Project=vm.Project,
       ProTask=vm.ProTask,
        

        joborderno =vm.joborderno,
       buildingno =vm.buildingno,
         siteno =vm.siteno,
        flatno =vm.flatno,
      quoteref=vm.quoteref 
    };
         
            db.Estimates.Add(es);
            db.SaveChanges();
          
            long estimateid = es.EstimateId;
            var estimateitem = vm.QuotItem;
            if(estimateitem!=null)
            {

                foreach (var Items in estimateitem)
                {
                    if (Items.amount != 0)
                    {
                        EstimateItems est = new EstimateItems
                        {
                            amount = Items.amount,
                            description = Items.description,
                            invno = Items.invno,

                            invdate = DateTime.Parse(Items.invdate.ToString(), new CultureInfo("en-GB")),//Items.invdate
                            EstimateId = estimateid,




                        };
                        db.EstimateItems.Add(est);
                        db.SaveChanges();
                    }
                  
                  
                    if (vm.bsmodel != null)
                    {
                        foreach (var bs in vm.bsmodel)
                        {
                            var qtB = new EsBillSundries
                            {
                                Estimate = estimateid,
                                BillSundry =Convert.ToInt64(bs.BillSundry),
                                BsValue = bs.BsValue,
                                AmountType = bs.AmountType,
                                BsType = bs.BsType,
                                BsAmount = bs.BsAmount,
                            };
                            db.EsBillSundries.Add(qtB);
                            db.SaveChanges();

                        }
                    }
                    var Appby = Convert.ToString(Request.Form["ddlApprovedBy"]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = estimateid;
                            approval.Type = "Estimate";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }


                }
            }
            if ((fnval) == "print" || (fnval) == "undefined")
            {
                var data = (
                    from e in db.Estimates
                    join c in db.Customers on e.Customer equals c.CustomerID into cst
                    from c in cst.DefaultIfEmpty()
                    join p in db.Projects on e.Project equals p.ProjectId into pr
                    from p in pr.DefaultIfEmpty()
                    join t in db.ProTasks on e.ProTask equals t.ProTaskId into protask
                    from t in protask.DefaultIfEmpty()
                    where e.EstimateId == estimateid
                    select new
                    {
                        e.EsttDate,
                        e.BillNo,
                        e.quoteref,
                        e.joborderno,
                        e.siteno,
                        e.QuotNo,
                        e.flatno,
                        e.buildingno,

                        p.ProjectName,
                        t.TaskName,
                        c.CustomerName,
                        e.QuotGrandTotal,
                        e.Remarks,

                    }

                    ).FirstOrDefault();
                var estitems = (
                   from it in db.EstimateItems
                   where it.EstimateId == estimateid
                   select new
                   {
                       it.invdate,
                       it.invno,
                       it.description,
                       it.amount

                   }).ToList();
               var billsundry = db.EsBillSundries.Where(n => n.Estimate == estimateid).Select(b => new pdfBillSundryViewModel
                {
                    AmountType = b.AmountType,
                    BsAmount = b.BsAmount,
                    BsType = b.BsType,
                    BsValue = b.BsValue != null ? b.BsValue : 0,
                    BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
                }).ToList();

                msg = "Successfully Created Estimate.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, data, estitems, billsundry } };
            }
            else
            {
                msg = "Successfully Created Estimate.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }


        }
        [HttpGet]
        public JsonResult GetEstItems(long Estimateid)
        {
            var data = (
                from i in db.EstimateItems
                where i.EstimateId == Estimateid
                select i
                ).ToList();
          
            return Json(data);
        }
            [QkAuthorize(Roles = "Dev,Edit Quotation")]
        public ActionResult Edit(long? id)
        {
            var data = (
                from e in db.Estimates
                where e.EstimateId == id
                select new EstimateViewModel
                {
                    quoteref = e.quoteref,
                    BillNo = e.BillNo,
                    EstimateId = e.EstimateId,
                    EsttDate =e.EsttDate.ToString(),
                    Customer = e.Customer,
                    buildingno = e.buildingno,
                    CreatedUser = e.CreatedUser,
                    flatno = e.flatno,
                    joborderno = e.joborderno,
                    Project = e.Project,
                    ProTask = e.ProTask,
                    QuotNo = e.QuotNo,
                    QuotGrandTotal = e.QuotGrandTotal,
                    siteno = e.siteno,

                    //QuotItem = (from i in db.EstimateItems
                    //            where i.EstimateId == id
                    //             EstimateItemId= i.EstimateItemId,
                    //                invdate=i.invdate,
                    //                invno= i.invno,
                    //                description=i.description,
                    //                amount=i.amount
                    //            }).ToList()

                }).ToList();
            Estimate es = new Estimate();
                es = db.Estimates.Find(id);
            EstimateViewModel vmodel = new EstimateViewModel();
            vmodel.BillNo = es.BillNo;
            vmodel.EstimateId = es.EstimateId;
            vmodel.EsttDate = es.EsttDate.ToString();
            vmodel.flatno = es.flatno;
            vmodel.joborderno = es.joborderno;
            vmodel.Project = es.Project;
            vmodel.Customer = es.Customer;
            vmodel.ProTask = es.ProTask;
            vmodel.siteno = es.siteno;
            vmodel.buildingno = es.buildingno;
            vmodel.quoteref = es.quoteref;
            vmodel.QuotGrandTotal = es.QuotGrandTotal;
            vmodel.Remarks = es.Remarks;
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





            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;
            
            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Estimate").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();
            var UserId = User.Identity.GetUserId();
            ViewBag.approvalstatus=com.chkApproved2((long)id, true, "Estimate", UserId);
            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var proj = db.Projects
    .Select(s => new
    {
        ID = s.ProjectId,
        Name = s.ProjectName
    })
    .ToList();
            ViewBag.Proj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");




            companySet();

            return View(vmodel);
        }

        public JsonResult SearchQuotation(string q, string x, string page)
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
                                  
                                  where ( b.BillNo.ToLower().Contains(q.ToLower()) || c.CustomerName.ToLower().Contains(q.ToLower()) 
                                   || c.CustomerName.StartsWith(q) || c.CustomerName.EndsWith(q))
                                  
                                  select new SelectFormat
                                  {
                                      text = b.BillNo + "-" + c.CustomerName,
                                      id = b.QuotationId
                                  }).OrderByDescending(a=>a.id).Take(pageSize).ToList();

            }
            else
            {
                serialisedJson = (from b in db.Quotations
                                  join c in db.Customers on b.Customer equals c.CustomerID into cnts
                                  from c in cnts.DefaultIfEmpty()
                                 
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
        [HttpPost]
        public JsonResult UpdateEstimate(EstimateViewModel est, string fnval)
        {
            string msg = "";
            bool stat = false;
            var UserId = User.Identity.GetUserId();
            Estimate es = db.Estimates.Find(est.EstimateId);
            es.CreatedUser = UserId;
            es.BillNo = est.BillNo;
                es.buildingno = est.buildingno;
            es.Customer = est.Customer;

            es.EsttDate = DateTime.Parse(est.EsttDate, new CultureInfo("en-GB"));
            es.flatno = est.flatno;
            es.joborderno = est.joborderno;
            es.Project = est.Project;
            es.ProTask = est.ProTask;
            es.quoteref = est.quoteref;
            es.QuotGrandTotal = est.QuotGrandTotal;
            es.QuotNo = est.QuotNo;
            es.Remarks = est.Remarks;
            es.siteno = est.siteno;
            db.Entry(es).State = EntityState.Modified;
            db.SaveChanges();
            if(est.QuotItem!=null)
            {
                db.EstimateItems.RemoveRange(db.EstimateItems.Where(o => o.EstimateId == est.EstimateId));
                db.SaveChanges();
                foreach(var Items in est.QuotItem)
                {
                    if (Items.amount != 0)
                    {
                        EstimateItems esi = new EstimateItems
                        {
                            amount = Items.amount,
                            description = Items.description,
                            invdate =DateTime.Parse(Items.invdate,new CultureInfo("en-GB")),
                            invno = Items.invno,
                            EstimateId = (long)est.EstimateId,

                        };
                        db.EstimateItems.Add(esi);
                        db.SaveChanges();
                    }
                }
            }
            if (est.bsmodel != null)
            {
                db.EsBillSundries.RemoveRange(db.EsBillSundries.Where(o=>o.Estimate == (long)est.EstimateId));
                 db.SaveChanges();
                foreach (var bs in est.bsmodel)
                {
                    var qtB = new EsBillSundries
                    {
                        Estimate = (long)est.EstimateId,
                        BillSundry = Convert.ToInt64(bs.BillSundry),
                        BsValue = bs.BsValue,
                        AmountType = bs.AmountType,
                        BsType = bs.BsType,
                        BsAmount = bs.BsAmount,
                    };
                    db.EsBillSundries.Add(qtB);
                    db.SaveChanges();

                }
            }
            //Approved By
            var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == est.EstimateId && a.Type == "Estimate").FirstOrDefault();
            var QnPO = db.Approvals.Where(a => a.TransEntry==est.EstimateId && a.Type == "Estimate").FirstOrDefault();
            if (QnPO != null)
            {
                if (chkapp != null)
                {
                    db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == est.EstimateId && a.Type == "Estimate"));
                    db.SaveChanges();
                }
                else
                {
                    db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == est.EstimateId && a.Type == "Estimate"));
                    db.SaveChanges();
                }
            }
            var Appby = Convert.ToString(Request.Form["ddlApprovedBy"]);
            if (Appby != null && Appby != "")
            {
                long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                Approval approval = new Approval();
                foreach (var emp in Approve)
                {
                    approval.TransEntry = (long)est.EstimateId;
                    approval.Type = "Estimate";
                    approval.EmployeeId = emp;
                    db.Approvals.Add(approval);
                    db.SaveChanges();
                }
            }
            if ((fnval) == "print")
            {
                var data = (
                    from e in db.Estimates
                    join c in db.Customers on e.Customer equals c.CustomerID into cst
                    from c in cst.DefaultIfEmpty()
                    join p in db.Projects on e.Project equals p.ProjectId into pr
                    from p in pr.DefaultIfEmpty()
                    join t in db.ProTasks on e.ProTask equals t.ProTaskId into protask
                    from t in protask.DefaultIfEmpty()
                    where e.EstimateId ==est.EstimateId
                    select new
                    {
                        e.EsttDate,
                        e.BillNo,
                        e.quoteref,
                        e.joborderno,
                        e.siteno,
                        e.QuotNo,
                        e.flatno,

                        p.ProjectName,
                        t.TaskName,
                        c.CustomerName,
                        e.QuotGrandTotal,
                        e.Remarks,

                    }

                    ).FirstOrDefault();
                var estitems = (
                   from it in db.EstimateItems
                   where it.EstimateId == est.EstimateId
                   select new
                   {
                       it.invdate,
                       it.invno,
                       it.description,
                       it.amount

                   }).ToList();
                var billsundry = db.EsBillSundries.Where(n => n.Estimate == est.EstimateId).Select(b => new pdfBillSundryViewModel
                {
                    AmountType = b.AmountType,
                    BsAmount = b.BsAmount,
                    BsType = b.BsType,
                    BsValue = b.BsValue != null ? b.BsValue : 0,
                    BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
                }).ToList();
                msg = "Successfully Created Estimate.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, data, estitems, billsundry } };
            }
            else
            {
                msg = "Successfully Created Estimate.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }
        [HttpGet]
        public JsonResult GetEsBillSundry(long quoteID)
        {
            var QtBs = (from a in db.EsBillSundries
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.Estimate == quoteID
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
        [QkAuthorize(Roles = "Dev,Edit Quotation")]
        public JsonResult UpdateQuotation(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {
            bool stat = false;
            string msg;
            Int64 quotEntryId = Convert.ToInt64(quotdata[16]);
            Quotation Quoentry = db.Quotations.Find(quotEntryId);
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
                    Quoentry.QuotDate = DateTime.ParseExact(quotdata[2], "MM/dd/yyyy", null); 
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
                    Quoentry.expdate = DateTime.ParseExact(quotdata[34], "MM/dd/yyyy", null);
                    Quoentry.leadsid = Convert.ToInt64(quotdata[36]);
                    Quoentry.quotationstatus = Convert.ToInt32(quotdata[36]);
                    Quoentry.revision = Convert.ToString(quotdata[37]);
                    Quoentry.Ref1 = Convert.ToString(quotdata[28]);
                    Quoentry.Ref2 = Convert.ToString(quotdata[29]);
                    Quoentry.Ref3 = Convert.ToString(quotdata[30]);
                    Quoentry.Ref4 = Convert.ToString(quotdata[31]);
                    Quoentry.Ref5 = Convert.ToString(quotdata[32]);
                    Quoentry.expdate = DateTime.ParseExact(quotdata[34], "MM/dd/yyyy", null);
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
                        dr["itemNote"] = Convert.ToString(arr[32].Replace("\n", "<br />"));
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
                    var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                QuickSoft.Helpers.DocumentTotals.RecomputeQuotation(db, quotEntryId); // forward-correctness: header = SUM(lines)
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
                    sm.SendPdfMail(generatePdf(quotEntryId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully Updated Quotation.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = quotEntryId } };
                }
                else
                {
                    msg = "Successfully Updated Quotation.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg ,qtnid = quotEntryId } };
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
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download Quotation")]
        public ActionResult Download(long id)
        {
            var Data = db.Quotations.Where(s => s.QuotationId == id).FirstOrDefault();
            var custname = db.Customers.Where(s => s.CustomerID == Data.Customer).Select(a => a.CustomerName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Quotation" + "-" + custname + "-" + billno + ".pdf");
        }
        public StringBuilder generatePdf(long quotationId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var QuotData = com.QuotationData(quotationId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck);
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







        [HttpPost]
        [QkAuthorize(Roles = "Dev,Quotation List")]
        public ActionResult GetEstimate(string BillNo, string FromDate, string ToDate, long? customer, long? salesperson, long? project, string Stats, string user, int? Validity, string Saletype, long? HireType, string appstat, long? Task)
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
            var userpermission = User.IsInRole("All Quotation Entry");
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

            //dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets). Split SERVER from CLIENT: materialize only entity columns +
            // simple scalars (left-joined entity access like Customer stays server-side) into serverRows, then
            // build client lookups keyed by EstimateId and re-project client-side with the SAME member names + order.
            var serverQuery = (from b in db.Estimates
                     join c in db.Customers on b.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()


                     join f in db.Projects on b.Project equals f.ProjectId into prj
                     from f in prj.DefaultIfEmpty()


                     join j in db.ProTasks on b.ProTask equals j.ProTaskId into task
                     from j in task.DefaultIfEmpty()
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.
                     where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                     (customer == 0 || customer == null || b.Customer == customer) && (project == 0 || project == null || b.Project == project) &&

                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(b.EsttDate, fdate) <= 0) &&
                     (ToDate == "" || FromDate == null || EF.Functions.DateDiffDay(b.EsttDate, tdate) >= 0)


                     && (Task == 0 || Task == null || j.ProTaskId == Task)
                     select new
                     {


                         b.EstimateId,
                         b.BillNo,
                         b.quoteref,
                         b.QuotNo,
                         b.EsttDate,

                         b.QuotGrandTotal,

                         b.Remarks,
                         b.Project,
                         ProjectName = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",

                         Customer = c.CustomerCode + " - " + c.CustomerName,

                         Dev = uDev,
                         Details = uQuotationView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         QuotationEntry = uQuotationEntry,
                         Task = (j.TaskName != null && j.TaskName != "") ? j.TaskCode + "-" + j.TaskName : "",

                     });

            // Performance (audit P2): server-side paging on the common path; search/computed sorts fall back unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BillNo","Customer","Delete","Details","Dev","Download","Edit","EstimateId","EsttDate","Project","ProjectName","QuotationEntry","quoteref","QuotGrandTotal","QuotNo","Remarks","Task" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("EstimateId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by EstimateId (missing key -> empty/absent, no KeyNotFound).
            var estIds = serverRows.Select(o => o.EstimateId).ToList();
            // app = approver EmployeeIds for the estimate (nested collection, keyed by TransEntry == EstimateId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "Estimate" && estIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "Estimate" && estIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per estimate.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.EstimateId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.EstimateId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.EstimateId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                         {



                         o.EstimateId,
                         o.EsttDate,
                         o.BillNo,
                         o.quoteref,
                         o.QuotNo,

                         o.QuotGrandTotal,


                         o.Remarks,
                         o.Project,
                         o.ProjectName,
                         o.Task,

                         o.Customer,

                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.QuotationEntry,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,

                         };
                     });



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
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

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
                if ((db.Estimates.Select(p => p.EstimateId).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    QENo = db.Estimates.Max(p => p.EstimateId + 1);
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
            var Exists = db.Estimates.Any(c => c.BillNo == QENo);
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
            var userpermission = User.IsInRole("All Quotation Entry");
            var UserId = User.Identity.GetUserId();
            Estimate Quot = db.Estimates.Where(x => (x.CreatedUser == UserId || userpermission == true) && x.EstimateId == id).FirstOrDefault();


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
            
                stat = DeleteFn(id);
              string msg = "Successfully deleted Estimate.";
         

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

        private Boolean DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            Estimate QSum = db.Estimates.Find(id);

            var QPro = db.EstimateItems.Where(a => a.EstimateId == id).FirstOrDefault();
            if (QPro != null)
            {
                db.EstimateItems.RemoveRange(db.EstimateItems.Where(a => a.EstimateId == id));
            }
            var qtBs = db.EsBillSundries.Where(a => a.Estimate == id).FirstOrDefault();
            if (qtBs != null)
            {
                db.EsBillSundries.RemoveRange(db.EsBillSundries.Where(a => a.Estimate == id));

            }
           

            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Estimate").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "Estimate"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Estimate").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Estimate"));
            }

            db.Estimates.Remove(QSum);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "estimate", "estimates", findip(), QSum.EstimateId, "Successfully Deleted Quotations");
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
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "Estimate" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.Estimates.Where(a => a.EstimateId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "Estimate").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))

            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = MR.CreatedUser;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "Estimate";

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
