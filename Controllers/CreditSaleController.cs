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
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Drawing;

using Microsoft.AspNetCore.Http;


namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class CreditSaleController : BaseController
    {

        ApplicationDbContext db;
        Common com;

        public CreditSaleController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }


   

        // GET: SalesEntry 
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Sales Entry List,No Tax Sales")]
        public ActionResult Index(long? saletype)
        {
            ViewBag.taxexeceptinvoice = 1;
            if (saletype != null)
            {
                ViewBag.taxexeceptinvoice = 0;

            }
            var cusomernames = db.SalesEntrys
           .Select(s => new
           {
               ID = s.customername,
               Name = s.customername
           }).Distinct()
           .ToList().OrderBy(a => a.Name);
            ViewBag.customernamenotax = QkSelect.List(cusomernames, "ID", "Name");
            var stat = db.Users.Find(User.Identity.GetUserId());

            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;
            ViewBag.SuperUserEditvalue = SuperUserEditvalue;
            if (stat.Status == 0)
            {
                return RedirectToAction("Login", "Users");
            }
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

            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();
            ViewBag.PayMethod = QkSelect.List(pay, "ID", "Name");
            companySet();
            ViewBag.Balance = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value=null},
                new SelectListItem() {Text = "Fully Paid", Value="0"},
                new SelectListItem() {Text = "Pending", Value="1"},
            }, "Value", "Text");
            var created = db.Users.Where(o => o.Discount == null).Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            _FinancialYear();
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
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var UserId = User.Identity.GetUserId();

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindSale").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjChks;

            var proj = db.Projects
               .Select(s => new
               {
                   ID = s.ProjectId,
                   Name = s.ProCode + " " + s.ProjectName
               }).Take(1)
               .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             }).Take(1)
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            //*************For Optional Fields
            SalesEntryViewModel vmodel = new SalesEntryViewModel();

            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();

            var ref1 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            if (UserId != null)
            {
                DateTime pastdate = DateTime.Now.AddDays(-100);
                DateTime? passupdated = db.LogManagers.Where(o => o.LogDetails == "UpdateProfile" && o.User == UserId).OrderByDescending(o => o.LogTime).Select(o => o.LogTime).FirstOrDefault();
                if (passupdated == null)
                    passupdated = pastdate;
                var diff = (DateTime.Now - (DateTime)passupdated).TotalDays;
                var passwordchangedays = db.EnableSettings.Where(o => o.EnableType == "passwordchangedays").SingleOrDefault();
                double days = 30;
                if (passwordchangedays != null)
                    days = passwordchangedays.TypeValue == null ? 30 : Convert.ToDouble(passwordchangedays.TypeValue);

                var hash = db.Users.Find(User.Identity.GetUserId());

                if (diff > days)
                {

                    return RedirectToAction("UpdateProfile", "Users");
                }
                else if (Request.Cookies["QuickERP2"] == null)
                {
                }
                else
                {
                    if (Request.Cookies["QuickERP2"] != hash.PasswordHash.ToString())
                    {
                    }
                }
            }
            return View(vmodel);
        }

        //GET..Used Material Index
        public ActionResult UsedMaterialsIndex()
        {
            ViewBag.Customer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.TaskName = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            return View();
        }



        //GET..Used Material Index
        public ActionResult TaskMannerReport()
        {
            ViewBag.Customer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            var taskmanner = (from c in db.ProTaskManners

                              select c)
                 .Select(s => new
                 {
                     ID = s.TaskTypeId,
                     Name = s.TypeName
                 })
                 .ToList();
            ViewBag.taskmanner = QkSelect.List(taskmanner, "ID", "Name");

            return View();
        }






        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Sales Entry")]
        [HttpGet]
        public ActionResult Createfast(long? id, string type)
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;
            ViewBag.Message = "Form submitted.";
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var EnableCreditLimit = db.EnableSettings.Where(a => a.EnableType == "CreditLimit").FirstOrDefault();
            var AutoCreditLimit = EnableCreditLimit != null ? EnableCreditLimit.Status : Status.inactive;
            ViewBag.AutoCreditLimit = AutoCreditLimit;


            var userpermission = User.IsInRole("All Sales Entry");


            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
            var UserIdd = User.Identity.GetUserId();
            var empid = (from a in db.Users
                         join b in db.Employees on a.Id equals b.UserId
                         where a.Id == UserIdd
                         select new
                         {
                             b.EmployeeId
                         }).FirstOrDefault();

            var salesentry = new SalesEntryViewModel
            {
                BillNo = InvoiceNo(),
                SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                SENote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "sales").Select(a => a.TermsCondit).FirstOrDefault(),
                SalesTypes = db.SalesTypes.ToList()


            };
            if (empid != null)
            {
                salesentry.SECashier = empid.EmployeeId;
            }


            if (id != null)
            {
                if (type == "Quote")
                {
                    Quotation quentry = db.Quotations.Find(id);
                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = quentry.QuotationId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = quentry.QuotCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = quentry.Customer;
                    salesentry.SEDiscount = quentry.QuotDiscount;
                    salesentry.SEGrandTotal = quentry.QuotGrandTotal;
                    var custmr = db.Customers.Find(quentry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = quentry.BillNo;
                    salesentry.Remarks = quentry.Remarks;
                    salesentry.ConvertType = type;
                    salesentry.Branch = quentry.Branch;
                    salesentry.SalesType = quentry.SalesType;
                    salesentry.SaleType = quentry.SaleType;
                    salesentry.PaymentTerms = quentry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = quentry.BillNo;

                    salesentry.SENote = quentry.TermsCondition;

                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = quentry.Project;
                        salesentry.ProTask = quentry.ProTask;
                    }
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Quotation").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "DVNote")
                {
                    Deliverynote dventry = db.Deliverynotes.Find(id);
                    if (dventry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = dventry.DeliverynoteId;
                    salesentry.ConType = type;
                    salesentry.PONo = dventry.LPONo;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = dventry.DvCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = dventry.Customer;
                    salesentry.SEDiscount = dventry.DvDiscount;
                    salesentry.SEGrandTotal = dventry.DvGrandTotal;
                    salesentry.Location = dventry.Location;
                    var custmr = db.Customers.Find(dventry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = dventry.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = dventry.Remarks;
                    salesentry.Branch = dventry.Branch;
                    salesentry.SalesType = dventry.SalesType;
                    salesentry.SaleType = dventry.SaleType;
                    salesentry.PaymentTerms = dventry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = dventry.BillNo;

                    salesentry.SENote = dventry.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = dventry.Project;
                        salesentry.ProTask = dventry.ProTask;
                    }
                    if (dventry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Delivernote").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "ProForma")
                {
                    ProForma PFentry = db.ProFormas.Find(id);
                    if (PFentry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = PFentry.ProFormaId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = PFentry.PFCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = PFentry.Customer;
                    salesentry.SEDiscount = PFentry.PFDiscount;
                    salesentry.SEGrandTotal = PFentry.PFGrandTotal;
                    salesentry.Location = PFentry.Location;
                    var custmr = db.Customers.Find(PFentry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = PFentry.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = PFentry.Remarks;
                    salesentry.Branch = PFentry.Branch;
                    salesentry.SalesType = PFentry.SalesType;
                    salesentry.SaleType = PFentry.SaleType;
                    salesentry.PaymentTerms = PFentry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = PFentry.BillNo;

                    salesentry.SENote = PFentry.PFNote;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = PFentry.Project;
                        salesentry.ProTask = PFentry.ProTask;
                    }
                    if (PFentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Proforma").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "SOrder")
                {
                    SalesOrder SOrder = db.SalesOrders.Find(id);
                    if (SOrder == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = SOrder.SalesOrderId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = SOrder.SOCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = SOrder.Customer;
                    salesentry.SEDiscount = SOrder.SODiscount;
                    salesentry.SEGrandTotal = SOrder.SOGrandTotal;
                    var custmr = db.Customers.Find(SOrder.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = SOrder.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = SOrder.Remarks;
                    salesentry.Branch = SOrder.Branch;
                    salesentry.SalesType = SOrder.SalesType;
                    salesentry.SaleType = SOrder.SaleType;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = SOrder.BillNo;

                    salesentry.SENote = SOrder.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = SOrder.Project;
                        salesentry.ProTask = SOrder.ProTask;
                    }
                    if (SOrder.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales order").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "SaleExtend")
                {
                    SalesEntry sentry = db.SalesEntrys.Find(id);
                    if (sentry == null)
                    {
                        return NotFound();
                    }

                    var Extension = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.To == id).Select(y => y.From).FirstOrDefault();
                    List<ConvertTransactions> Trans;
                    List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                    int num = 0;
                    if (Extension != 0)
                    {
                        Trans = ExtNum((long)id, ExtList);
                        num = Trans.Count + 1;
                    }
                    else
                    {
                        num = 1;
                    }

                    salesentry.BillNo = InvoiceNo(0, null, "Hire") + "/Ex-" + num;
                    salesentry.PONo = sentry.PONo;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = sentry.SECashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = sentry.Customer;
                    salesentry.SEDiscount = sentry.SEDiscount;
                    salesentry.SEGrandTotal = sentry.SEGrandTotal;
                    var custmr = db.Customers.Find(sentry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConTypeId = sentry.SalesEntryId;
                    salesentry.ConType = type;
                    salesentry.Remarks = sentry.Remarks;
                    salesentry.Branch = sentry.Branch;
                    salesentry.SaleType = sentry.SaleType;
                    salesentry.SalesType = sentry.SalesType;
                    salesentry.Location = sentry.Location;
                    salesentry.MaterialCenter = sentry.MaterialCenter;
                    salesentry.PaymentTerms = sentry.PaymentTerms;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = sentry.Project;
                        salesentry.ProTask = sentry.ProTask;
                    }
                    salesentry.SENote = sentry.SENote;

                    //salesentry.convertFrom = type + " No";//label

                    if (sentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales").FirstOrDefault();
                        salesentry.FromDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                    }
                }

            }

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            ViewBag.Item = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                             }, "Value", "Text", 0);

            var list = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                           }, "Value", "Text", 1);
            ViewBag.PayMethod = list;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;


            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
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


            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.LastEntry = db.SalesEntrys.Where(p => MCArray.Contains(p.MaterialCenter) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.SalesEntryId).AsEnumerable().DefaultIfEmpty(0).Max();

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var dvsale = db.EnableSettings.Where(a => a.EnableType == "DvToSale").FirstOrDefault();
            var dvsalecheck = dvsale != null ? dvsale.Status : Status.inactive;
            ViewBag.DVEnable = dvsalecheck;

            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;
            ViewBag.ReturnInSale = returninsales;

            var pay = db.PaymentMethods.FirstOrDefault();
            ViewBag.payVisible = pay != null ? true : false;


            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Contype = type;

            ViewBag.ContType = "SalesEntry";
            // material center
            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var AutoBillSale = db.EnableSettings.Where(a => a.EnableType == "AutomaticBillNoInSales").FirstOrDefault();
            var AutoBillSales = AutoBillSale != null ? AutoBillSale.Status : Status.inactive;
            ViewBag.AutoBillInSale = AutoBillSales;


            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
                .Select(s => new
                {
                    ID = s.ProjectId,
                    Name = s.ProCode + " " + s.ProjectName
                })
                .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                .Select(s => new
                {
                    ID = s.EmployeeId,
                    Name = s.FirstName + " " + s.LastName
                })
                .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;


            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;

            salesentry.SaleTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").Select(a => a.TypeValue).FirstOrDefault());
            salesentry.PurTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").Select(a => a.TypeValue).FirstOrDefault());

            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;

            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var usedmaterial = db.EnableSettings.Where(a => a.EnableType == "Usedmaterials").FirstOrDefault();
            var usedmat = usedmaterial != null ? usedmaterial.Status : Status.inactive;
            ViewBag.UsedMaterials = usedmat;

            var discountper = db.Users.Where(x => x.Id == UserId).Select(y => y.Discount).FirstOrDefault();
            ViewBag.Discountpercent = discountper;

            _FinancialYear();
            companySet();

            //field mapping
            salesentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();
            var ref1 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            //fieldmap end

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

            var TaxInclusive = db.EnableSettings.Where(a => a.EnableType == "TaxInclusive").FirstOrDefault();
            var TaxInclusives = TaxInclusive != null ? TaxInclusive.Status : Status.inactive;
            ViewBag.TaxInclusive = TaxInclusives;

            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", salesentry);
            }
            else
            {
                return View(salesentry);
            }
        }








        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Sales Entry,No Tax Sales")]
        [HttpGet]
        public ActionResult Create(long? id, string type, long? saletype)
        {
            
            var mcanddeliverystockeffect = db.EnableSettings.Where(a => a.EnableType == "mcanddeliverystockeffect").FirstOrDefault();
            var enmcanddeliverystockeffect = mcanddeliverystockeffect != null ? mcanddeliverystockeffect.Status : Status.inactive;

            var enbonusforcustomer = db.EnableSettings.Where(a => a.EnableType == "bonusforcustomer").FirstOrDefault();
            var bonusforcustomer = enbonusforcustomer != null ? enbonusforcustomer.Status : Status.inactive;
            ViewBag.bonuscust = bonusforcustomer;
            ViewBag.fixedsellingprice = false;
            var setsellingpricefixeds = db.EnableSettings.Where(c => c.EnableType == "setsellingpricefixed").SingleOrDefault();
            if(setsellingpricefixeds!=null)
            {
                var editpower = User.IsInRole("Edit Sales Entry");
                var uid = User.Identity.GetUserId();
                var days = db.UserEditDayss.Any(o => (o.days > 10080 || o.days == 0) && o.userid == uid);
                
                if(setsellingpricefixeds.Status==0 && !days)
                {
                    ViewBag.fixedsellingprice = true;
                }
            }
            ViewBag.taxexeceptinvoice = 1;
            if (saletype != null)
            {
                ViewBag.taxexeceptinvoice = 0;
            }
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            var enablepricetratagy = db.EnableSettings.Where(a => a.EnableType == "enablepricestratagy").FirstOrDefault();
            var pricetratagy = enablepricetratagy != null ? enablepricetratagy.Status : Status.inactive;
            ViewBag.pricestratagy = pricetratagy;
            ;
            ViewBag.BranchCheck = BranchCheck;
            var EnableRackWise = db.EnableSettings.Where(a => a.EnableType == "RackWiseStock").FirstOrDefault();
            var RackCheck = EnableRackWise != null ? EnableRackWise.Status : Status.inactive;
            ViewBag.RackWiseStock = RackCheck;
            ViewBag.Message = "Form submitted.";
            ViewBag.disableupdate = User.IsInRole("Disable Update Option In Sales");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var EnableCreditLimit = db.EnableSettings.Where(a => a.EnableType == "CreditLimit").FirstOrDefault();
            var AutoCreditLimit = EnableCreditLimit != null ? EnableCreditLimit.Status : Status.inactive;
            ViewBag.AutoCreditLimit = AutoCreditLimit;
            var CustDetails = db.EnableSettings.Where(a => a.EnableType == "CustomerDetailInInvoice").FirstOrDefault();
            var ViewCustDetails = CustDetails != null ? CustDetails.Status : Status.inactive;
            ViewBag.ViewCustDetails = ViewCustDetails;

            var userpermission = User.IsInRole("All Sales Entry");


            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
            var UserIdd = User.Identity.GetUserId();



            var impids2 = db.Employees.Where(o => o.UserId == UserIdd).Select(o => o.EmployeeId).FirstOrDefault();
            var data22 = (from a in db.accountmaps
                          join b in db.Accountss
                          on a.AccountId equals b.AccountsID
                          where (a.EmployeeId == impids2)
                          select new
                          {
                              EmployeeId = a.EmployeeId,
                              AccountId = a.AccountId,
                              AccountNames = b.Name,
                              PaymentType = a.PaymentTypeId,
                              Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                              //ChequeNo,
                              //ChequeDate
                          }).ToList();
            ViewBag.salesman = "true";
            if (data22 == null || data22.Count() == 0)
            {
                ViewBag.salesman = "false";
            }










            var empid = (from a in db.Users
                         join b in db.Employees on a.Id equals b.UserId
                         where a.Id == UserIdd
                         select new
                         {
                             b.EmployeeId
                         }).FirstOrDefault();

            var salesentry = new SalesEntryViewModel
            {
                BillNo = InvoiceNo(),
                SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                SENote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "sales").Select(a => a.TermsCondit).FirstOrDefault(),
                SalesTypes = db.SalesTypes.ToList()


            };
            if (empid != null)
            {
                salesentry.SECashier = empid.EmployeeId;
            }


            if (id != null)
            {
                if (type == "Quote")
                {
                    Quotation quentry = db.Quotations.Find(id);
                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = quentry.QuotationId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = quentry.QuotCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = quentry.Customer;
                    salesentry.SEDiscount = quentry.QuotDiscount;
                    salesentry.SEGrandTotal = quentry.QuotGrandTotal;
                    var custmr = db.Customers.Find(quentry.Customer);
                    salesentry.custEmailId =(custmr==null)?"": db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = quentry.BillNo;
                    salesentry.Remarks = quentry.Remarks;
                    salesentry.ConvertType = type;
                    salesentry.Branch = quentry.Branch;
                    salesentry.SalesType = quentry.SalesType;
                    salesentry.SaleType = quentry.SaleType;
                    salesentry.PaymentTerms = quentry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = quentry.BillNo;

                    salesentry.SENote = quentry.TermsCondition;

                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = quentry.Project;
                        salesentry.ProTask = quentry.ProTask;
                    }
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Quotation").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "DVNote")
                {
                    Deliverynote dventry = db.Deliverynotes.Find(id);
                    if (dventry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = dventry.DeliverynoteId;
                    salesentry.ConType = type;

                    salesentry.PONo = dventry.LPONo;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = dventry.DvCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = dventry.Customer;
                    salesentry.SEDiscount = dventry.DvDiscount;
                    salesentry.SEGrandTotal = dventry.DvGrandTotal;
                    salesentry.Location = dventry.Location;
                    var custmr = db.Customers.Find(dventry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = dventry.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = dventry.Remarks;
                    salesentry.Branch = dventry.Branch;
                    salesentry.SalesType = dventry.SalesType;
                    salesentry.SaleType = dventry.SaleType;
                    salesentry.PaymentTerms = dventry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = dventry.BillNo;
                    salesentry.DVNotDate = dventry.DvDate;
                    DateTime createdDate = db.Deliverynotes.Where(c => c.BillNo == dventry.BillNo).Select(c => c.DvDate).FirstOrDefault();
                    string dateString = String.Format("{0:dd/MM/yyyy}", createdDate);

                    ViewBag.dvdate = dateString;

                    salesentry.SENote = dventry.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = dventry.Project;
                        salesentry.ProTask = dventry.ProTask;
                    }
                    if (dventry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Delivernote").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "ProForma")
                {
                    ProForma PFentry = db.ProFormas.Find(id);
                    if (PFentry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = PFentry.ProFormaId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = PFentry.PFCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = PFentry.Customer;
                    salesentry.SEDiscount = PFentry.PFDiscount;
                    salesentry.SEGrandTotal = PFentry.PFGrandTotal;
                    salesentry.Location = PFentry.Location;
                    var custmr = db.Customers.Find(PFentry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = PFentry.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = PFentry.Remarks;
                    salesentry.Branch = PFentry.Branch;
                    salesentry.SalesType = PFentry.SalesType;
                    salesentry.SaleType = PFentry.SaleType;
                    salesentry.PaymentTerms = PFentry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = PFentry.BillNo;

                    salesentry.SENote = PFentry.PFNote;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = PFentry.Project;
                        salesentry.ProTask = PFentry.ProTask;
                    }
                    if (PFentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Proforma").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "SOrder")
                {
                    SalesOrder SOrder = db.SalesOrders.Find(id);
                    if (SOrder == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = SOrder.SalesOrderId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = SOrder.SOCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = SOrder.Customer;
                    salesentry.SEDiscount = SOrder.SODiscount;
                    salesentry.SEGrandTotal = SOrder.SOGrandTotal;
                    var custmr = db.Customers.Find(SOrder.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = SOrder.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = SOrder.Remarks;
                    salesentry.Branch = SOrder.Branch;
                    salesentry.SalesType = SOrder.SalesType;
                    salesentry.SaleType = SOrder.SaleType;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = SOrder.BillNo;

                    salesentry.SENote = SOrder.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = SOrder.Project;
                        salesentry.ProTask = SOrder.ProTask;
                    }
                    if (SOrder.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales order").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "SaleExtend")
                {
                    SalesEntry sentry = db.SalesEntrys.Find(id);
                    if (sentry == null)
                    {
                        return NotFound();
                    }

                    var Extension = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.To == id).Select(y => y.From).FirstOrDefault();
                    List<ConvertTransactions> Trans;
                    List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                    int num = 0;
                    if (Extension != 0)
                    {
                        Trans = ExtNum((long)id, ExtList);
                        num = Trans.Count + 1;
                    }
                    else
                    {
                        num = 1;
                    }

                    salesentry.BillNo = InvoiceNo();
                    salesentry.PONo = sentry.PONo;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = sentry.SECashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = sentry.Customer;
                    salesentry.SEDiscount = sentry.SEDiscount;
                    salesentry.SEGrandTotal = sentry.SEGrandTotal;
                    var custmr = db.Customers.Find(sentry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConTypeId = sentry.SalesEntryId;
                    salesentry.ConType = type;
                    salesentry.Remarks = sentry.Remarks;
                    salesentry.Branch = sentry.Branch;
                    salesentry.SaleType = sentry.SaleType;
                    salesentry.SalesType = sentry.SalesType;
                    salesentry.Location = sentry.Location;
                    salesentry.MaterialCenter = sentry.MaterialCenter;
                    salesentry.PaymentTerms = sentry.PaymentTerms;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = sentry.Project;
                        salesentry.ProTask = sentry.ProTask;
                    }
                    salesentry.SENote = sentry.SENote;

                    //salesentry.convertFrom = type + " No";//label

                    if (sentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales").FirstOrDefault();
                        salesentry.FromDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                    }
                }
                if (type == "Purchase")
                {
                    PurchaseEntry PEentry = db.PurchaseEntrys.Find(id);
                    if (PEentry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = PEentry.PurchaseEntryId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = PEentry.PECashier;
                    salesentry.SEDiscount = PEentry.PEDiscount;
                    salesentry.SEGrandTotal = PEentry.PEGrandTotal;
                    salesentry.ConvertNo = PEentry.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = PEentry.Remarks;
                    salesentry.Branch = PEentry.Branch;
                    salesentry.SalesType = PEentry.PurchaseType;
                    salesentry.MaterialCenter = PEentry.MaterialCenter;
                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = PEentry.BillNo;


                }
            }


            //            !a.CustomerName.StartsWith("OLD-")
            //                a.CustomerID,
            //                a.CustomerCode,
            //                a.CustomerName
            //            }).ToList().Select(s => new
            //    CustomerID = s.CustomerID,
            //    CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            ViewBag.Customr = QkSelect.List(
                             new List<SelectListItem>
                             {
                                 new SelectListItem { Selected = false, Text = "", Value = ""},
                             }, "Value", "Text", 0);
            if (type == "SOrder"||type== "DVNote"|| type == "Quote")
            {
                var cust = (from a in db.Customers
                            join b in db.Accountss on a.Accounts equals b.AccountsID

                            where a.Type == CRMCustomerType.Customer &&
                            !a.CustomerName.StartsWith("OLD-")
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
                ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            }
            var pri = db.PriceCategories.Where(o => o.active == false)
                         .Select(s => new
                         {
                             ID = s.value,
                             Name = s.description
                         }).ToList();

            pri.Insert(0, db.PriceCategories.Where(o => o.active == true)
                             .Select(s => new
                             {
                                 ID = s.value,
                                 Name = s.description
                             }).FirstOrDefault());
            ViewBag.pricecategorylist = QkSelect.List(pri, "ID", "Name");
            ViewBag.Item = QkSelect.List(
                             new List<SelectListItem>
                             {
                                 new SelectListItem { Selected = false, Text = "", Value = "0"},
                             }, "Value", "Text", 0);

            var list = QkSelect.List(
                           new List<SelectListItem>
                           {
                                 new SelectListItem { Selected = false, Text = "", Value = ""},
                           }, "Value", "Text", 1);
            ViewBag.PayMethod = list;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;


            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();


            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName

                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();

                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                ViewBag.MC = QkSelect.List(serialisedJson, "id", "text");
                ViewBag.LastMc = serialisedJson.Select(a => a.id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Where(p => p.Status == Status.active).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
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
            var delper = User.IsInRole("Edit Sales Entry");
            var userid = User.Identity.GetUserId();
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


            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.LastEntry = db.SalesEntrys.Where(p => MCArray.Contains(p.MaterialCenter) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.SalesEntryId).AsEnumerable().DefaultIfEmpty(0).Max();

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var dvsale = db.EnableSettings.Where(a => a.EnableType == "DvToSale").FirstOrDefault();
            var dvsalecheck = dvsale != null ? dvsale.Status : Status.inactive;
            ViewBag.DVEnable = dvsalecheck;

            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;
            ViewBag.ReturnInSale = returninsales;

            var pay = db.PaymentMethods.FirstOrDefault();
            ViewBag.payVisible = pay != null ? true : false;


            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Contype = type;

            ViewBag.ContType = "SalesEntry";
            // material center
            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var AutoBillSale = db.EnableSettings.Where(a => a.EnableType == "AutomaticBillNoInSales").FirstOrDefault();
            var AutoBillSales = AutoBillSale != null ? AutoBillSale.Status : Status.inactive;
            ViewBag.AutoBillInSale = AutoBillSales;


            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
                .Select(s => new
                {
                    ID = s.ProjectId,
                    Name = s.ProCode + " " + s.ProjectName
                }).Take(1)
                .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.ProTasks.Where(o => o.ProTaskId == -1)
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = new MultiSelectList(tsk, "ID", "Name");
            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                .Select(s => new
                {
                    ID = s.EmployeeId,
                    Name = s.FirstName + " " + s.LastName
                })
                .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;


            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;

            salesentry.SaleTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").Select(a => a.TypeValue).FirstOrDefault());
            salesentry.PurTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").Select(a => a.TypeValue).FirstOrDefault());

            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;

            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var usedmaterial = db.EnableSettings.Where(a => a.EnableType == "Usedmaterials").FirstOrDefault();
            var usedmat = usedmaterial != null ? usedmaterial.Status : Status.inactive;
            ViewBag.UsedMaterials = usedmat;
            var usedmaterialinSEitems = db.EnableSettings.Where(a => a.EnableType == "UsedmaterialsItemsInSE").FirstOrDefault();
            var usedmatinSEitems = usedmaterialinSEitems != null ? usedmaterialinSEitems.Status : Status.inactive;
            ViewBag.UsedMaterialsinSEitems = usedmatinSEitems;
            var discountper = db.Users.Where(x => x.Id == UserId).Select(y => y.Discount).FirstOrDefault();
            ViewBag.Discountpercent = discountper;

            _FinancialYear();
            companySet();

            //field mapping
            salesentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();
            var ref1 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            var cusomernames = db.SalesEntrys
           .Select(s => new
           {
               ID = s.customername,
               Name = s.customername
           }).Distinct()
           .ToList().OrderBy(a => a.Name);
            ViewBag.customernamenotax = QkSelect.List(cusomernames, "ID", "Name");

            //fieldmap end

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

            var TaxInclusive = db.EnableSettings.Where(a => a.EnableType == "TaxInclusive").FirstOrDefault();
            var TaxInclusives = TaxInclusive != null ? TaxInclusive.Status : Status.inactive;
            ViewBag.TaxInclusive = TaxInclusives;

            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;
            if (enmcanddeliverystockeffect == Status.active && type == "DVNote")
            {
                salesentry.MaterialCenter = db.MCs.Where(o => o.MCName == "TASK CENTER").Select(o => o.MCId).SingleOrDefault();
            }
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", salesentry);
            }
            else
            {
                return View(salesentry);
            }
        }
        public JsonResult GetFillDetails(long CustId)
        {
            var enbonusforcustomer = db.EnableSettings.Where(a => a.EnableType == "bonusforcustomer").FirstOrDefault();
            var bonusforcustomer = enbonusforcustomer != null ? enbonusforcustomer.Status : Status.inactive;
            var bonuscust = bonusforcustomer;
            DateTime datenow = DateTime.Now;
            if (CustId == 0)
                return Json(null);
            var creditperiod = db.Customers.Find(CustId).CreditPeriod;

            decimal ret = 0;
            var totalpayed = (from a in db.SalesEntrys
                              join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                              from b in pay.DefaultIfEmpty()
                              join c in db.Customers on a.Customer equals c.CustomerID into rec
                              from c in rec.DefaultIfEmpty()

                              let salesreturn = (decimal?)(from d in db.SalesReturns
                                                           where d.SalesEntryId == a.SalesEntryId
                                                           select d.SRGrandTotal).Sum() ?? 0


                              let payment = (decimal?)(from e in db.PaymentBills
                                                       join f in db.Payments on e.Payment equals f.PaymentId into reciept2

                                                       join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                       join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                       where h.SalesEntryId == a.SalesEntryId &&
                                                       e.BillType == "Sales Return"
                                                       select e.Amount).Sum() ?? 0
                              let paymentjrnl = (decimal?)(from e in db.JornalPaymentBills
                                                           join f in db.Journals on e.Jornal equals f.JournalId into reciept2

                                                           join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                           join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                           where h.SalesEntryId == a.SalesEntryId &&
                                                           e.BillType == "Sales Return"
                                                           select e.Amount).Sum() ?? 0

                              where
                                a.Customer == CustId

                              // Only the summed Amount is consumed below. The original projected an UNUSED `reciept`
                              // sub-object (a correlated FirstOrDefault collection subquery over a left-joined Receipt)
                              // plus other discarded fields; EF Core 10 cannot translate that collection subquery
                              // ("Unable to translate a collection subquery in a projection") -> 500. Amount does NOT
                              // depend on reciept, so project it directly — identical sum, now translatable.
                              select (decimal?)(((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - (((salesreturn == null) ? 0 : salesreturn) - ((payment == null) ? 0 : payment) - ((paymentjrnl == null) ? 0 : paymentjrnl)))
                              ).AsEnumerable().DefaultIfEmpty(0).Sum();


            // EF Core 10 cannot translate this single-customer projection as one tree (nested
            // collection sub-lists + GroupBy-of-anonymous + select-new{}-before-Sum + Distinct
            // over correlated subqueries). The outer query returns exactly ONE row (CustId filter),
            // so materialize the base row first, then build each field as its own standalone
            // correlated query that EF translates independently. Output is byte-identical.
            var ConD = (from a in db.Customers
                        join c in db.Employees on a.SalesPerson equals c.EmployeeId into secondary
                        from c in secondary.DefaultIfEmpty()
                        join d in db.CustomerTyps on a.CustomerType equals d.TypeId into temp
                        from d in temp.DefaultIfEmpty()
                        where a.CustomerID == CustId

                        let Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0)
                        let Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0)

                        select new { a, c, d, Credit, Debit }).AsEnumerable().Select(r => new
                        {
                            r.a.Accounts,
                            CusType = r.a.CustomerType == null ? "" : r.d.Type,
                            CreditLimit = r.a.CreditLimit == null ? 0 : r.a.CreditLimit,
                            currentbalance = ((r.Debit > r.Credit) ? ((r.Debit - r.Credit)) : r.Debit - r.Credit),
                            acbalance = (r.Debit > r.Credit) ? ((r.Debit - r.Credit) + " Dr.") : ((r.Credit - r.Debit) + " Cr."),
                            mob = (from co in db.Contacts
                                   join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                   join con in db.Country on co.CountryID equals con.CountryID into conn
                                   from con in conn.DefaultIfEmpty()
                                   where (rrr.RelationID == CustId && rrr.RelationType == 0)
                                   select new MobileViewModel
                                   {
                                       Num = "+" + con.CountryCode + co.Mobile,
                                       Name = co.FirstName + "  " + co.LastName,
                                       emails = co.EmailId,
                                   }).ToList(),
                            // GroupBy-Account-then-Sum(per-group Credit)-then-Sum equals summing all
                            // Credit across the same row set (partition by Account is a pure cover, no
                            // filter/Distinct/Take), so the flat Sum is mathematically identical.
                            pdc = (decimal?)(from b in db.AccountsTransactions
                                             join c in db.Customers on b.Account equals c.Accounts

                                             where b.Status != null && c.CustomerID == CustId
                                             select (decimal?)b.Credit).Sum() ?? 0,
                            //pdc = (decimal?)(

                            //       r.MOPayment == ModeOfPayment.PDC
                            //           r.GrandTotal

                            //        ).Sum(x => x.GrandTotal) ?? 0,

                            ddlEmployee = r.c.EmployeeId == null ? 0 : r.c.EmployeeId,
                            doc = (from bb in db.CustomerDocuments
                                   join cc in db.DocumentTypes on bb.DocumentTypeID equals cc.ID into temp2
                                   from cc in temp2.DefaultIfEmpty()
                                   where bb.CutomerID == CustId
                                   select new
                                   {
                                       cc.Name,
                                       bb.Expiry,
                                       bb.FilePath,
                                   }).ToList(),
                            billage = (from a in db.SalesEntrys
                                       join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                                       from b in pay.DefaultIfEmpty()
                                       join c in db.Customers on a.Customer equals c.CustomerID into rec
                                       from c in rec.DefaultIfEmpty()

                                       let salesreturn = (decimal?)(from d in db.SalesReturns
                                                                    where d.SalesEntryId == a.SalesEntryId
                                                                    select d.SRGrandTotal).Sum() ?? 0
                                       where
                                            (a.Customer == CustId) &&
                                            (c.CreditPeriod > 0) &&
                                            totalpayed > 2




                                       select new
                                       {
                                           id = a.SalesEntryId,
                                           Date = a.SEDate,
                                           Invoice = a.BillNo,
                                           c.CreditPeriod,
                                           Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - ((salesreturn == null) ? 0 : salesreturn)),
                                           total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                                           paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                                           Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                                           Ctyp = "Cus"
                                       }).Where(o => o.Amount > 3 && o.Days > creditperiod).OrderByDescending(o => o.Days).Distinct().Take(5).ToList(),
                            billagenotexp = (from a in db.SalesEntrys
                                             join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                                             from b in pay.DefaultIfEmpty()
                                             join c in db.Customers on a.Customer equals c.CustomerID into rec
                                             from c in rec.DefaultIfEmpty()
                                             join d in db.SalesReturns on a.SalesEntryId equals d.SalesEntryId into slret
                                             from d in slret.DefaultIfEmpty()

                                             where
                                               (a.Customer == CustId)




                                             select new
                                             {
                                                 id = a.SalesEntryId,
                                                 Date = a.SEDate,
                                                 Invoice = a.BillNo,
                                                 c.CreditPeriod,
                                                 Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - ((d.SRGrandTotal == null) ? 0 : d.SRGrandTotal)),
                                                 total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                                                 paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                                                 Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                                                 Ctyp = "Cus"
                                             }).Where(o => o.Amount > 3 && o.Days <= creditperiod).OrderByDescending(o => o.Days).Distinct().Take(5).ToList(),
                            bonusvalid = bonuscust,
                            bonusclimable = (decimal?)(from a in db.customerbonus
                                                       where a.customerid == -1
                                                       select (a.climableamount - a.claimamount)).Sum() ?? 0,
                            includepdc = !r.a.includepdc,

                        }).ToList();

            return Json(ConD);
        }
        public Dictionary<string, object> GetFillDetailsobj(long CustId)
        {
            DateTime datenow = DateTime.Now;
            var creditperiod = db.Customers.Find(CustId).CreditPeriod;

            decimal ret = 0;


            var ConD = (from a in db.Customers
                        join c in db.Employees on a.SalesPerson equals c.EmployeeId into secondary
                        from c in secondary.DefaultIfEmpty()
                        join d in db.CustomerTyps on a.CustomerType equals d.TypeId into temp
                        from d in temp.DefaultIfEmpty()
                        where a.CustomerID == CustId

                        let Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0)
                        let Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0)

                        select new
                        {
                            a.Accounts,
                            CusType = a.CustomerType == null ? "" : d.Type,
                            CreditLimit = a.CreditLimit == null ? 0 : a.CreditLimit,
                            currentbalance = ((Debit > Credit) ? ((Debit - Credit)) : 0),
                            acbalance = (Debit > Credit) ? ((Debit - Credit) + " Dr.") : ((Credit - Debit) + " Cr."),
                            mob = (from co in db.Contacts
                                   join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                   join con in db.Country on co.CountryID equals con.CountryID into conn
                                   from con in conn.DefaultIfEmpty()
                                   where (rrr.RelationID == CustId && rrr.RelationType == 0)
                                   select new MobileViewModel
                                   {
                                       Num = "+" + con.CountryCode + co.Mobile,
                                       Name = co.FirstName + "  " + co.LastName,
                                       emails = co.EmailId,
                                   }).ToList(),
                            pdc = (decimal?)(from b in db.AccountsTransactions
                                             join c in db.Customers on b.Account equals c.Accounts

                                             where b.Status != null && c.CustomerID == CustId
                                             group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                                             select new
                                             {

                                                 GrandTotal = g.Sum(o => o.Credit)


                                             }).Sum(x => x.GrandTotal) ?? 0,
                            //pdc = (decimal?)(

                            //       r.MOPayment == ModeOfPayment.PDC
                            //           r.GrandTotal

                            //        ).Sum(x => x.GrandTotal) ?? 0,

                            ddlEmployee = c.EmployeeId == null ? 0 : c.EmployeeId,
                            doc = (from bb in db.CustomerDocuments
                                   join cc in db.DocumentTypes on bb.DocumentTypeID equals cc.ID into temp2
                                   from cc in temp2.DefaultIfEmpty()
                                   where bb.CutomerID == CustId
                                   select new
                                   {
                                       cc.Name,
                                       bb.Expiry,
                                       bb.FilePath,
                                   }).ToList(),
                            billage = (from a in db.SalesEntrys
                                       join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                                       from b in pay.DefaultIfEmpty()
                                       join c in db.Customers on a.Customer equals c.CustomerID into rec
                                       from c in rec.DefaultIfEmpty()
                                       join d in db.SalesReturns on a.SalesEntryId equals d.SalesEntryId into slret
                                       from d in slret.DefaultIfEmpty()

                                       where
                                         (a.Customer == CustId) &&
                                         (c.CreditPeriod > 0)




                                       select new
                                       {
                                           id = a.SalesEntryId,
                                           Date = a.SEDate,
                                           Invoice = a.BillNo,
                                           c.CreditPeriod,
                                           Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - ((d.SRGrandTotal == null) ? 0 : d.SRGrandTotal)),
                                           total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                                           paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                                           Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                                           Ctyp = "Cus"
                                       }).Where(o => o.Amount > 3 && o.Days > creditperiod).OrderByDescending(o => o.Days).Distinct().Take(5).ToList(),
                            billagenotexp = (from a in db.SalesEntrys
                                             join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                                             from b in pay.DefaultIfEmpty()
                                             join c in db.Customers on a.Customer equals c.CustomerID into rec
                                             from c in rec.DefaultIfEmpty()
                                             join d in db.SalesReturns on a.SalesEntryId equals d.SalesEntryId into slret
                                             from d in slret.DefaultIfEmpty()

                                             where
                                               (a.Customer == CustId) &&
                                               (c.CreditPeriod > 0)




                                             select new
                                             {
                                                 id = a.SalesEntryId,
                                                 Date = a.SEDate,
                                                 Invoice = a.BillNo,
                                                 c.CreditPeriod,
                                                 Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - ((d.SRGrandTotal == null) ? 0 : d.SRGrandTotal)),
                                                 total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                                                 paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                                                 Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                                                 Ctyp = "Cus"
                                             }).Where(o => o.Amount > 3 && o.Days <= creditperiod).OrderByDescending(o => o.Days).Distinct().Take(5).ToList(),

                            includepdc = a.includepdc,

                        }).ToList();
            var Data = new Dictionary<string, object>();
            Data.Add("1", ConD[0]);

            return Data;
        }

        public JsonResult GetFillDatas(long CustId, long EntryId)
        {

            var ConD = (from a in db.Customers
                        join b in db.SalesEntrys
                        on a.CustomerID equals b.Customer into primary
                        from b in primary.DefaultIfEmpty()
                        join c in db.Employees
                        on a.SalesPerson equals c.EmployeeId into secondary
                        from c in secondary.DefaultIfEmpty()

                        where a.CustomerID == CustId && b.SalesEntryId == EntryId

                        let Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0)
                        let Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0)

                        select new
                        {
                            a.CreditLimit,
                            currentbalance = (((Debit > Credit) ? ((Debit - Credit)) : 0)) - (b.SEGrandTotal),
                            acbalance = (Debit > Credit) ? ((Debit - Credit) + " Dr.") : ((Credit - Debit) + " Cr."),


                            ddlEmployee = c.EmployeeId,

                        });
            return Json(ConD);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        [HttpGet]
        public ActionResult Editmobile(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            SalesEntry Saleentry = db.SalesEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesEntryId == id).FirstOrDefault();

            SalesEntry Salentry = db.SalesEntrys.Find(id);
            ViewBag.disableupdate = User.IsInRole("Disable Update Option In Sales");
            if (Saleentry == null)
            {
                return NotFound();
            }
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddDays(-userEditDays);
            }


            if ((Saleentry.SEDate - editableDay).TotalDays < 0 || tem == 1)
            {
                return NotFound();

            }

            //Fetching the image from AttachmentDocuments
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.SalesEntrys
                             on a.TransactionID equals b.SalesEntryId
                             where a.TransactionID == id && a.TransactionType == "CreditSale"
                             select new CreditSaleDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 CreditSaleId = a.TransactionID,
                                 FileName = a.FileName,
                                 CreatedDate = a.CreatedDate
                             }).ToList();
            //................................................

            Int64 cashier = Convert.ToInt64(Saleentry.SECashier);
            Int64 customer = Saleentry.Customer;

            string custname = "";
            SalesEntryViewModel vmodel = new SalesEntryViewModel();

            custname = db.Customers.Where(a => a.CustomerID == customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var use = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {//.Where(s => s.AssignedUser == UserId)
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

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

            var proj = db.Projects
              .Select(s => new
              {
                  ID = s.ProjectId,
                  Name = s.ProCode + " " + s.ProjectName
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
              .Select(s => new
              {
                  ID = s.ProTaskId,
                  Name = s.TaskName
              })
              .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesEntry").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Sale").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "Quote")
                {
                    CBill = db.Quotations.Where(a => a.QuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "DVNote")
                {
                    CBill = db.Deliverynotes.Where(a => a.DeliverynoteId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "ProForma")
                {
                    CBill = db.ProFormas.Where(a => a.ProFormaId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "SOrder")
                {
                    CBill = db.SalesOrders.Where(a => a.SalesOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }


            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.SalesEntryId, f2 = "Sales" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.SalesEntryId == id
                      select new SalesEntryViewModel
                      {
                          SENo = b.SENo,
                          PONo = b.PONo,
                          SENote = b.SENote,
                          SEDate = b.SEDate,
                          BillNo = b.BillNo,
                          SECashier = b.SECashier,
                          Customer = b.Customer,
                          SEDiscount = b.SEDiscount,
                          SEGrandTotal = b.SEGrandTotal,
                          // SEPaidAmount = c.SEPaidAmount,
                          CustomerType = b.CustomerType,
                          // SEDueAmount = b.SEGrandTotal - c.SEPaidAmount,
                          CustomerName = custname,
                          PaymentMethod = b.PaymentMethod,
                          custEmailId = e.EmailId,
                          SalesType = b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          Location = b.Location,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          Currency = b.Currency,
                          DConvertionRate = b.ConvertionRate,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          HSCode = b.HSCode,
                          PaymentTerms = b.PaymentTerms,
                          Project = b.Project,
                          ProTask = b.ProTask,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();

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

            companySet();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();


            ViewBag.preEntry = db.SalesEntrys.Where(a => a.SalesEntryId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.SalesEntrys.Where(a => a.SalesEntryId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Min();


            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var paymethod = db.PaymentMethods.FirstOrDefault();
            ViewBag.payVisible = paymethod != null ? true : false;

            var list = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                            }, "Value", "Text", 1);

            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();
            if (vmodel.PaymentMethod == null)
            {
                ViewBag.PayMethod = list;
            }
            else
            {
                ViewBag.PayMethod = QkSelect.List(pay, "ID", "Name");
            }
            _FinancialYear();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.ContType = "SalesEntry";

            ViewBag.PopUpAddCust = false;


            var EditPermission = User.IsInRole("Disable Sale Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "SalesEntry", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();

            var ref1 = db.SalesEntrys
               .Select(s => new
               {
                   ID = s.Ref1,
                   Name = s.Ref1
               }).Distinct()
               .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", Salentry.Ref1);

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", Salentry.Ref2);

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", Salentry.Ref3);

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", Salentry.Ref4);

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", Salentry.Ref5);
            long rtask = 11;
            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();
            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            //dummy table operations
            var DItem = db.DummySEItems.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SItem = db.SEItemss.Where(a => a.SalesEntry == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummySEItems.Where(a => a.SalesEntry == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    SEItems sItem = new SEItems();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.itemNote = arr.itemNote;
                    sItem.SalesEntry = arr.SalesEntry;
                    sItem.Item = arr.Item;
                    db.SEItemss.Add(sItem);
                    db.SaveChanges();
                }

                db.DummySEItems.RemoveRange(db.DummySEItems.Where(a => a.SalesEntry == id));
                db.SaveChanges();
                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, id); // forward-correctness: header = SUM(lines)
            }

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var usedmaterial = db.EnableSettings.Where(a => a.EnableType == "Usedmaterials").FirstOrDefault();
            var usedmat = usedmaterial != null ? usedmaterial.Status : Status.inactive;
            ViewBag.UsedMaterials = usedmat;

            var discountper = db.Users.Where(x => x.Id == UserId).Select(y => y.Discount).FirstOrDefault();
            ViewBag.Discountpercent = discountper;

            var TaxInclusive = db.EnableSettings.Where(a => a.EnableType == "TaxInclusive").FirstOrDefault();
            var TaxInclusives = TaxInclusive != null ? TaxInclusive.Status : Status.inactive;
            ViewBag.TaxInclusive = TaxInclusives;

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

            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;
            ViewBag.ReturnInSale = returninsales;

            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            if (rtype == "mobile")
            {
                return View("Editmobile", vmodel);
            }
            else
            {
                return View(vmodel);
            }

        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Sales Entry")]
        [HttpGet]
        public ActionResult Createmobile(long? id, string type)
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;
            ViewBag.Message = "Form submitted.";
            ViewBag.disableupdate = User.IsInRole("Disable Update Option In Sales");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var EnableCreditLimit = db.EnableSettings.Where(a => a.EnableType == "CreditLimit").FirstOrDefault();
            var AutoCreditLimit = EnableCreditLimit != null ? EnableCreditLimit.Status : Status.inactive;
            ViewBag.AutoCreditLimit = AutoCreditLimit;
            var CustDetails = db.EnableSettings.Where(a => a.EnableType == "CustomerDetailInInvoice").FirstOrDefault();
            var ViewCustDetails = CustDetails != null ? CustDetails.Status : Status.inactive;
            ViewBag.ViewCustDetails = ViewCustDetails;

            var userpermission = User.IsInRole("All Sales Entry");


            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
            var UserIdd = User.Identity.GetUserId();
            var empid = (from a in db.Users
                         join b in db.Employees on a.Id equals b.UserId
                         where a.Id == UserIdd
                         select new
                         {
                             b.EmployeeId
                         }).FirstOrDefault();

            var salesentry = new SalesEntryViewModel
            {
                BillNo = InvoiceNo(),
                SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                SENote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "sales").Select(a => a.TermsCondit).FirstOrDefault(),
                SalesTypes = db.SalesTypes.ToList()


            };
            if (empid != null)
            {
                salesentry.SECashier = empid.EmployeeId;
            }


            if (id != null)
            {
                if (type == "Quote")
                {
                    Quotation quentry = db.Quotations.Find(id);
                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = quentry.QuotationId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = quentry.QuotCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = quentry.Customer;
                    salesentry.SEDiscount = quentry.QuotDiscount;
                    salesentry.SEGrandTotal = quentry.QuotGrandTotal;
                    var custmr = db.Customers.Find(quentry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = quentry.BillNo;
                    salesentry.Remarks = quentry.Remarks;
                    salesentry.ConvertType = type;
                    salesentry.Branch = quentry.Branch;
                    salesentry.SalesType = quentry.SalesType;
                    salesentry.SaleType = quentry.SaleType;
                    salesentry.PaymentTerms = quentry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = quentry.BillNo;

                    salesentry.SENote = quentry.TermsCondition;

                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = quentry.Project;
                        salesentry.ProTask = quentry.ProTask;
                    }
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Quotation").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "DVNote")
                {
                    Deliverynote dventry = db.Deliverynotes.Find(id);
                    if (dventry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = dventry.DeliverynoteId;
                    salesentry.ConType = type;
                    salesentry.PONo = dventry.LPONo;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = dventry.DvCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = dventry.Customer;
                    salesentry.SEDiscount = dventry.DvDiscount;
                    salesentry.SEGrandTotal = dventry.DvGrandTotal;
                    salesentry.Location = dventry.Location;
                    var custmr = db.Customers.Find(dventry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = dventry.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = dventry.Remarks;
                    salesentry.Branch = dventry.Branch;
                    salesentry.SalesType = dventry.SalesType;
                    salesentry.SaleType = dventry.SaleType;
                    salesentry.PaymentTerms = dventry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = dventry.BillNo;
                    salesentry.DVNotDate = dventry.DvDate;
                    DateTime createdDate = db.Deliverynotes.Where(c => c.BillNo == dventry.BillNo).Select(c => c.DvDate).FirstOrDefault();
                    string dateString = String.Format("{0:dd/MM/yyyy}", createdDate);

                    ViewBag.dvdate = dateString;

                    salesentry.SENote = dventry.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = dventry.Project;
                        salesentry.ProTask = dventry.ProTask;
                    }
                    if (dventry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Delivernote").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "ProForma")
                {
                    ProForma PFentry = db.ProFormas.Find(id);
                    if (PFentry == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = PFentry.ProFormaId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = PFentry.PFCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = PFentry.Customer;
                    salesentry.SEDiscount = PFentry.PFDiscount;
                    salesentry.SEGrandTotal = PFentry.PFGrandTotal;
                    salesentry.Location = PFentry.Location;
                    var custmr = db.Customers.Find(PFentry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = PFentry.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = PFentry.Remarks;
                    salesentry.Branch = PFentry.Branch;
                    salesentry.SalesType = PFentry.SalesType;
                    salesentry.SaleType = PFentry.SaleType;
                    salesentry.PaymentTerms = PFentry.PaymentTerms;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = PFentry.BillNo;

                    salesentry.SENote = PFentry.PFNote;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = PFentry.Project;
                        salesentry.ProTask = PFentry.ProTask;
                    }
                    if (PFentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Proforma").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "SOrder")
                {
                    SalesOrder SOrder = db.SalesOrders.Find(id);
                    if (SOrder == null)
                    {
                        return NotFound();
                    }
                    salesentry.ConTypeId = SOrder.SalesOrderId;
                    salesentry.ConType = type;
                    salesentry.PONo = null;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = SOrder.SOCashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = SOrder.Customer;
                    salesentry.SEDiscount = SOrder.SODiscount;
                    salesentry.SEGrandTotal = SOrder.SOGrandTotal;
                    var custmr = db.Customers.Find(SOrder.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConvertNo = SOrder.BillNo;
                    salesentry.ConvertType = type;
                    salesentry.Remarks = SOrder.Remarks;
                    salesentry.Branch = SOrder.Branch;
                    salesentry.SalesType = SOrder.SalesType;
                    salesentry.SaleType = SOrder.SaleType;

                    salesentry.convertFrom = type + " No";//label
                    salesentry.convertBill = SOrder.BillNo;

                    salesentry.SENote = SOrder.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = SOrder.Project;
                        salesentry.ProTask = SOrder.ProTask;
                    }
                    if (SOrder.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales order").FirstOrDefault();
                        salesentry.FromDate = Hdet.StartDate;
                        salesentry.ToDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                        salesentry.BillNo = InvoiceNo(0, null, "Hire");
                    }
                }
                if (type == "SaleExtend")
                {
                    SalesEntry sentry = db.SalesEntrys.Find(id);
                    if (sentry == null)
                    {
                        return NotFound();
                    }

                    var Extension = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.To == id).Select(y => y.From).FirstOrDefault();
                    List<ConvertTransactions> Trans;
                    List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                    int num = 0;
                    if (Extension != 0)
                    {
                        Trans = ExtNum((long)id, ExtList);
                        num = Trans.Count + 1;
                    }
                    else
                    {
                        num = 1;
                    }

                    salesentry.BillNo = InvoiceNo(0, null, "Hire") + "/Ex-" + num;
                    salesentry.PONo = sentry.PONo;
                    salesentry.SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    salesentry.SECashier = sentry.SECashier;
                    salesentry.CustomerType = CustomerType.Customer;
                    salesentry.Customer = sentry.Customer;
                    salesentry.SEDiscount = sentry.SEDiscount;
                    salesentry.SEGrandTotal = sentry.SEGrandTotal;
                    var custmr = db.Customers.Find(sentry.Customer);
                    salesentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    salesentry.ConTypeId = sentry.SalesEntryId;
                    salesentry.ConType = type;
                    salesentry.Remarks = sentry.Remarks;
                    salesentry.Branch = sentry.Branch;
                    salesentry.SaleType = sentry.SaleType;
                    salesentry.SalesType = sentry.SalesType;
                    salesentry.Location = sentry.Location;
                    salesentry.MaterialCenter = sentry.MaterialCenter;
                    salesentry.PaymentTerms = sentry.PaymentTerms;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        salesentry.Project = sentry.Project;
                        salesentry.ProTask = sentry.ProTask;
                    }
                    salesentry.SENote = sentry.SENote;

                    //salesentry.convertFrom = type + " No";//label

                    if (sentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales").FirstOrDefault();
                        salesentry.FromDate = Hdet.EndDate;
                        salesentry.HireType = Hdet.HireType;
                    }
                }

            }

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            ViewBag.Item = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                             }, "Value", "Text", 0);

            var list = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                           }, "Value", "Text", 1);
            ViewBag.PayMethod = list;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;


            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();


            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName

                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();

                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                ViewBag.MC = QkSelect.List(serialisedJson, "id", "text");
                ViewBag.LastMc = serialisedJson.Select(a => a.id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
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


            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.LastEntry = db.SalesEntrys.Where(p => MCArray.Contains(p.MaterialCenter) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.SalesEntryId).AsEnumerable().DefaultIfEmpty(0).Max();

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var dvsale = db.EnableSettings.Where(a => a.EnableType == "DvToSale").FirstOrDefault();
            var dvsalecheck = dvsale != null ? dvsale.Status : Status.inactive;
            ViewBag.DVEnable = dvsalecheck;

            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;
            ViewBag.ReturnInSale = returninsales;

            var pay = db.PaymentMethods.FirstOrDefault();
            ViewBag.payVisible = pay != null ? true : false;


            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Contype = type;

            ViewBag.ContType = "SalesEntry";
            // material center
            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var AutoBillSale = db.EnableSettings.Where(a => a.EnableType == "AutomaticBillNoInSales").FirstOrDefault();
            var AutoBillSales = AutoBillSale != null ? AutoBillSale.Status : Status.inactive;
            ViewBag.AutoBillInSale = AutoBillSales;


            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
                .Select(s => new
                {
                    ID = s.ProjectId,
                    Name = s.ProCode + " " + s.ProjectName
                })
                .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.ProTasks.Where(o => o.ProTaskId == -1)
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = new MultiSelectList(tsk, "ID", "Name");
            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                .Select(s => new
                {
                    ID = s.EmployeeId,
                    Name = s.FirstName + " " + s.LastName
                })
                .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;


            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;

            salesentry.SaleTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").Select(a => a.TypeValue).FirstOrDefault());
            salesentry.PurTransCount = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").Select(a => a.TypeValue).FirstOrDefault());

            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;

            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var usedmaterial = db.EnableSettings.Where(a => a.EnableType == "Usedmaterials").FirstOrDefault();
            var usedmat = usedmaterial != null ? usedmaterial.Status : Status.inactive;
            ViewBag.UsedMaterials = usedmat;
            var usedmaterialinSEitems = db.EnableSettings.Where(a => a.EnableType == "UsedmaterialsItemsInSE").FirstOrDefault();
            var usedmatinSEitems = usedmaterialinSEitems != null ? usedmaterialinSEitems.Status : Status.inactive;
            ViewBag.UsedMaterialsinSEitems = usedmatinSEitems;
            var discountper = db.Users.Where(x => x.Id == UserId).Select(y => y.Discount).FirstOrDefault();
            ViewBag.Discountpercent = discountper;

            _FinancialYear();
            companySet();

            //field mapping
            salesentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();
            var ref1 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");


            //fieldmap end

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

            var TaxInclusive = db.EnableSettings.Where(a => a.EnableType == "TaxInclusive").FirstOrDefault();
            var TaxInclusives = TaxInclusive != null ? TaxInclusive.Status : Status.inactive;
            ViewBag.TaxInclusive = TaxInclusives;

            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", salesentry);
            }
            else
            {
                return View(salesentry);
            }

        }

        public bool deleteproduction(long voucherid)
        {
            string vid = voucherid.ToString();
            long id = db.Productions.Where(o => o.VoucherNo == vid).Select(o => o.ProductionId).FirstOrDefault();
            var UserId = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            Production pro = db.Productions.Find(id);

            //To Update the quantity in Delete Mode(ItemTransaction Table)

            var proItem = db.ProItems.Where(a => a.Production == id);
            if (proItem != null)
            {
                db.ProItems.RemoveRange(db.ProItems.Where(a => a.Production == id));
            }
            var progen = db.GeneratedItem.Where(a => a.Production == id);
            if (progen != null)
            {
                db.GeneratedItem.RemoveRange(db.GeneratedItem.Where(a => a.Production == id));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Production").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "Production"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Production").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Production"));
            }
            if (pro != null)
                db.Productions.Remove(pro);
            com.addlog(LogTypes.Deleted, UserId, "Production", "Productions", findip(), id, "Successfully Deleted Production");
            db.SaveChanges();
            return true;
        }
        public bool createproduction(long bomid, long voucherno, long mcc)
        {

            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var today = Convert.ToDateTime(System.DateTime.Now);

            var Date = today;

            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();


            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

            long MC = 0;

            MC = mcc;
            Int64 PrNom = voucherno;
            string voucher = PrNom.ToString();
            string Note = "";
            long MaterialCenter = MC;
            Int64 proId = 0;


            Production pro = new Production
            {
                VoucherNo = voucher,
                PrNo = PrNom,
                PEDate = Date,
                Note = "",
                MaterialCenter = MC,
                CreatedDate = today,
                CreatedBy = UserId,
                Status = Status.active,
                Branch = Branch,

            };
            db.Productions.Add(pro);
            db.SaveChanges();
            proId = pro.ProductionId;

            //To Update the quantity in Create Mode(ItemTransaction Table)
            var bomitem = db.BillOfMaterialsoffers.Find(bomid).ItemId;

            long? BOM = bomid;

            var it = db.Items.Find(bomitem);


            GeneratedItems con = new GeneratedItems
            {
                Production = proId,
                BOM = bomid,
                Item = bomitem,
                Qty = 1,
                Unit = it.ItemUnitID,
                Expense = 0,
                Price = it.SellingPrice,
                Amount = it.SellingPrice,
            };
            db.GeneratedItem.Add(con);
            db.SaveChanges();
            var proitem = db.BOMItemsoffers.Where(o => o.BOMOfferId == bomid).ToList();
            foreach (var arrItem in proitem)
            {
                if (BOM == arrItem.BOMOfferId)
                {
                    var itt = db.Items.Find(arrItem.ItemId);

                    ProItem prItem = new ProItem();
                    {
                        prItem.Production = proId;
                        prItem.ItemId = arrItem.ItemId;
                        prItem.Unit = arrItem.Unit;
                        prItem.Quantity = arrItem.Quantity;
                        prItem.PPrice = it.SellingPrice;
                        prItem.PAmount = it.SellingPrice * arrItem.Quantity;
                        db.ProItems.Add(prItem);
                        db.SaveChanges();
                    }


                }
            }
            BOM = null;



            com.addlog(LogTypes.Created, UserId, "Production", "Productions", findip(), proId, "Successfully Submitted Productions");





            return true;


        }


        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Entry,No Tax Sales")]
        public JsonResult CreateSale(string[][] array, string[] saledata, SEBillSundryViewModel bsmodel, string[][] arrayR, ICollection<BatchStockPViewModel> bstmodel, ICollection<UBatchStockPViewModel> ubstmodel, string[][] arrayused, long? protask, ICollection<commissionViewmodel> commission, string TenderingMode, string Mode, ICollection<SettlementViewModel> SettlementData, decimal? BalanceAmount, ICollection<RackStockPViewModel> bsrackData)
        {
     

            var currdate = User.IsInRole("Read Only Sales Invoice Date");
           

            bool stat = false;
            string msg;
            if (!currdate)
            {

                //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            //-----stop hang--//
            #region stophang
            string result2 = string.Empty;
            DataTable dtItem2 = new DataTable();
            dtItem2.Columns.Add("ItemUnit");
            dtItem2.Columns.Add("ItemUnitPrice");
            dtItem2.Columns.Add("ItemQuantity");
            dtItem2.Columns.Add("ItemSubTotal");
            dtItem2.Columns.Add("ItemDiscount");
            dtItem2.Columns.Add("ItemTax");
            dtItem2.Columns.Add("ItemTaxAmount");
            dtItem2.Columns.Add("ItemTotalAmount");
            dtItem2.Columns.Add("itemNote");
            dtItem2.Columns.Add("SaleEntry");
            dtItem2.Columns.Add("Item");
            dtItem2.Columns.Add("Type");

            foreach (var arr in array)
            {
                var qty = 0;// db.BatchStocks.Where(x => x.Invoice == Convert.ToString(saledata[17]) && x.Item == Convert.ToInt32(arr[0]) && x.type == "Sale").Select(y => y.Quantity).Sum();
                if (arr[0] != null)
                {
                    DataRow dr = dtItem2.NewRow();
                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = Convert.ToDecimal(arr[4]);
                    dr["ItemQuantity"] = (qty > 0) ? qty : Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[6]);
                    dr["ItemDiscount"] = Convert.ToDecimal((arr[7] == "") ? 0 : Convert.ToDecimal(arr[7]));
                    dr["ItemTax"] = Convert.ToDecimal(arr[11]);//arr[10]
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[10]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[12]);
                    if (arr[29] == "http://")
                        dr["itemNote"] = "";
                    if (arr.Count() > 30)
                        dr["itemNote"] = Convert.ToString(arr[33].Replace("http://", ""));
                    else
                        dr["itemNote"] = "";


                    dr["SaleEntry"] = 1;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    dr["Type"] = false;

                    dtItem2.Rows.Add(dr);
                    var sdate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                    var mcc = Convert.ToInt64(saledata[26]);
                    long itid = Convert.ToInt32(arr[0]);
                    //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                    long itemID = Convert.ToInt32(arr[0]);

                }
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
                    long typ = Convert.ToInt64(saledata[36]);
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
                                      ItemUnitPrice = a.ItemSubTotal,
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


                        DataRow dbu = dtItem2.NewRow();
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
                        dbu["SaleEntry"] = 1;
                        dbu["Item"] = bu.Item;
                        dbu["Type"] = false;
                        dtItem2.Rows.Add(dbu);
                    }
                }

            }

            #endregion
            //----stom hang--//
            var convertType = saledata[21] != "" ? saledata[21] : null;
            var convertNo = saledata[22] != "" ? saledata[22] : null;
            if (convertType != null)
            {
                if (convertType == "Purchase")
                {
                    stat = true;
                }
                var convertSale = db.SalesEntrys.Where(ab => ab.ConvertType == convertType && ab.ConvertNo == convertNo).Select(ab => ab.BillNo).FirstOrDefault();
                if (convertSale == null)
                {
                    stat = true;
                }
            }
            else
            {
                stat = true;
            }
            if (stat == true)
            {
                var AutoBillSale = db.EnableSettings.Where(a => a.EnableType == "AutomaticBillNoInSales").FirstOrDefault();
                var AutoBillSales = AutoBillSale != null ? AutoBillSale.Status : Status.inactive;

                if (db.SalesEntrys.Any())
                {
                    var useid = User.Identity.GetUserId();
                    DateTime endate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                    var cis = Convert.ToInt64(saledata[0]);
                    var gt = Convert.ToDecimal(saledata[7]);
                    var seq = Convert.ToDecimal(saledata[4]);
                    bool ext = db.SalesEntrys.Any(o => o.SEDate == endate && o.Customer == cis && o.CreatedBy == useid && o.SEGrandTotal ==gt  && o.SEItemQuantity == seq);

                    //            where a.SalesEntryId == saidold
                    //                a.Customer,
                    //                a.CreatedBy,
                    //                a.SEGrandTotal,
                    //                a.SEItemQuantity,
                    //                a.BillNo

                    if(ext)
                    {
                       // return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }

                    //    //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }
                if (!BillExist(Convert.ToString(saledata[17])) || AutoBillSales == Status.active)
                {
                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                    Int64 saleAcc = (long)db.companys.Select(a => a.SaleAccount).FirstOrDefault();

                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;
                    long MC = 0;
                    long MCRet = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                    var HideItmName = db.EnableSettings.Where(a => a.EnableType == "HideItemName").FirstOrDefault();
                    var HideItmNameIfDiscriptionOn = HideItmName != null ? HideItmName.Status : Status.inactive;

                    var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                    var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(saledata[29]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    if (MCcheck == Status.active)
                    {
                        MC = Convert.ToInt64(saledata[26]);
                        MCRet = Convert.ToInt64(saledata[38]);
                    }
                    else
                    {
                        MC = 1;
                        MCRet = 1;
                    }

                    var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
                    ViewBag.EnableCurrency = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;

                    long Currency = 0;
                    var ConRate = "";
                    if (ViewBag.EnableCurrency == 0)
                    {

                        Currency = Convert.ToInt64(saledata[30]);
                        ConRate = saledata[31];
                    }
                    else
                    {
                        Currency = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.Id).FirstOrDefault();
                        ConRate = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.ConvertionRate).FirstOrDefault();
                    }

                    string action = saledata[15];
                    //add to saleEntries                    
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var TaxAmount = Convert.ToDecimal(saledata[5]);
                    var SEGrandTotal = Convert.ToDecimal(saledata[7]);
                    var saleamount = SEGrandTotal - TaxAmount;
                    var subtotal = Convert.ToDecimal(saledata[8]);
                    //sales entry                    
                    SalesEntry SEentry = new SalesEntry();
                    if (saledata[33] != null)
                    {
                        string str = saledata[33];
                        SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                        SEentry.SaleType = Stype;
                    }
                    else
                    {
                        SEentry.SaleType = SaleType.Sale;
                    }
                    SEentry.SENo = GetSeNo(saledata[24]);
                    SEentry.customername = saledata[13];
                    SEentry.phonenumber = saledata[14];

                    if (saledata[24] == "2")
                    {
                        SEentry.BillNo = (AutoBillSales == Status.active) ? InvoiceNo(0, null, "TaxExempt") : Convert.ToString(saledata[17]);

                    }
                    else
                    {
                        SEentry.BillNo = (AutoBillSales == Status.active) ? InvoiceNo() : Convert.ToString(saledata[17]);
                    }
                    SEentry.SEDate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                    SEentry.SECashier = saledata[1] != "" ? Convert.ToInt64(saledata[1]) : 0;
                    SEentry.SalesStatus = Convert.ToInt32(saledata[63]);
                    decimal perval = Convert.ToDecimal(saledata[64]);
                    SEentry.pricecategoryid = db.PriceCategories.Where(o => o.value == perval).Select(o => o.pricestratagyid).FirstOrDefault();

                    if (TenderingMode == "inactive")
                    {
                        SEentry.CustomerType = (saledata[12] == "2") ? CustomerType.Card : ((saledata[12] == "1") ? CustomerType.Walking : CustomerType.Customer);
                    }
                    else
                    {
                        if (BalanceAmount != SEGrandTotal)
                            SEentry.CustomerType = CustomerType.Walking;
                        else
                            SEentry.CustomerType = CustomerType.Customer;
                    }
                    SEentry.Customer = Convert.ToInt64(saledata[0]);
                    SEentry.PONo = saledata[18];
                    SEentry.PayType = "invoice";
                    SEentry.SEItems = Convert.ToInt32(saledata[3]);
                    if (saledata[4] != "NaN")
                    {
                        SEentry.SEItemQuantity = Convert.ToDecimal(saledata[4]);
                    }
                    else
                    {
                        SEentry.SEItemQuantity = 0;
                    }
                    SEentry.SESubTotal = Convert.ToDecimal(saledata[8]);
                    SEentry.SETax = Convert.ToDecimal(saledata[9]);
                    SEentry.SETaxAmount = TaxAmount;
                    SEentry.SEDiscount = Convert.ToDecimal(saledata[6]);
                    SEentry.SEGrandTotal = SEGrandTotal;
                    SEentry.SENote = saledata[11];
                    SEentry.ConvertType = saledata[21] != "" ? saledata[21] : null;
                    SEentry.ConvertNo = saledata[22] != "" ? saledata[22] : null;

                    SEentry.Print = 1;
                    SEentry.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    SEentry.CreatedBy = UserId;
                    SEentry.Status = 1;
                    SEentry.Branch = Branch;
                    SEentry.Location = saledata[23];
                    SEentry.SalesType = Convert.ToInt64(saledata[24]);
                    SEentry.Remarks = saledata[25];
                    SEentry.MaterialCenter = MC; //Convert.ToInt64(saledata[26]);
                    SEentry.Currency = Currency;
                    SEentry.ConvertionRate = ConRate;
                    SEentry.FCTotal = Convert.ToDecimal(saledata[32]);
                    SEentry.HSCode = saledata[49];
                    SEentry.PaymentTerms = saledata[50];
                    SEentry.Project = saledata[51] != "" ? Convert.ToInt64(saledata[51]) : 0;

                    SEentry.PaymentMethod = (saledata[12] == "2") ? (long?)Convert.ToInt64(saledata[20]) : null;
                    //paymethod                                                                                         

                    SEentry.Ref1 = saledata[57] == null ? "" : Convert.ToString(saledata[57]);
                    SEentry.Ref2 = Convert.ToString(saledata[58]);
                    SEentry.Ref3 = Convert.ToString(saledata[59]);
                    SEentry.Ref4 = Convert.ToString(saledata[60]);
                    SEentry.Ref5 = Convert.ToString(saledata[61]);
                    var bonusclaim = Convert.ToDecimal(saledata[65]);
                    if (SEentry.Project != null && SEentry.Project != 0)
                    {
                        SEentry.SaleAccount = db.Projects.Where(a => a.ProjectId == SEentry.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                    }
                    else
                    {
                        SEentry.SaleAccount = saleAcc;
                    }

                    db.SalesEntrys.Add(SEentry);
                    if (db.SalesEntrys.Any())
                    {
                        var useid = User.Identity.GetUserId();
                        DateTime endate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                        var cis = Convert.ToInt64(saledata[0]);
                        var gt = Convert.ToDecimal(saledata[7]);
                        var seq = Convert.ToDecimal(saledata[4]);
                        bool ext = db.SalesEntrys.Any(o => o.SEDate == endate && o.Customer == cis && o.CreatedBy == useid && o.SEGrandTotal == gt && o.SEItemQuantity == seq);

                        //            where a.SalesEntryId == saidold
                        //                a.Customer,
                        //                a.CreatedBy,
                        //                a.SEGrandTotal,
                        //                a.SEItemQuantity,
                        //                a.BillNo

                        if (ext)
                        {
                            msg = "Same  Invoice. Already Exists.Go Sales List and Edit";
                            stat = false;
                            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                        }
                    }
                        db.SaveChanges();
                    var checkdup = db.SalesEntrys.Where(o => o.BillNo == SEentry.BillNo).ToList();
                    if (checkdup.Count > 1)
                    {
                        var sen = db.SalesEntrys.Find(checkdup[1].SalesEntryId);
                        sen.BillNo = InvoiceNo();
                        db.Entry(sen).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    Int64 salesEntryId = SEentry.SalesEntryId;

                    var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

                    //To Update the quantity in Create Mode(ItemTransaction Table)
                    if (array != null && MC != 0)
                        com.ItemTransInCreateMode("CreditSale", MC, 0, 0, array, UserId, CurrentDate);

                    //To Update the used quantity in Create Mode(ItemTransaction Table)
                    if (arrayused != null && MC != 0)
                        com.ItemTransInCreateMode("CreditSaleUsedMaterials", MC, 0, 0, arrayused, UserId, CurrentDate);

                    if (SEentry.SaleType == SaleType.Hire)
                    {
                        HireDetail HDetils = new HireDetail();
                        HDetils.StartDate = DateTime.Parse(saledata[34], new CultureInfo("en-GB"));
                        HDetils.EndDate = DateTime.Parse(saledata[35], new CultureInfo("en-GB"));
                        HDetils.Section = "Sales";
                        HDetils.HireType = Convert.ToInt64(saledata[36]);
                        HDetils.Reference = salesEntryId;
                        db.HireDetails.Add(HDetils);
                        db.SaveChanges();
                    }


                    if (saledata[27] != null && saledata[27] != "0" && saledata[27] != "" && saledata[28] != null && saledata[28] != "" && saledata[28] != "0")
                    {
                        string[] List = saledata[27].Split(',');

                        foreach (var arr in List)
                        {
                            ConvertTransactions ConTran = new ConvertTransactions();

                            ConTran.ConvertFrom = saledata[28];
                            ConTran.ConvertTo = "Sale";
                            ConTran.From = Convert.ToInt64(arr);
                            ConTran.To = salesEntryId;
                            ConTran.Status = 0;
                            ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            ConTran.CreatedBy = UserId;
                            ConTran.Branch = Convert.ToInt32(BranchID);

                            db.ConvertTransactionss.Add(ConTran);
                            db.SaveChanges();
                            com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Conversion");

                        }
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
                    dtItem.Columns.Add("itemNote");
                    dtItem.Columns.Add("SaleEntry");
                    dtItem.Columns.Add("Item");
                    dtItem.Columns.Add("Type");

                    foreach (var arr in array)
                    {
                        var qty = 0;// db.BatchStocks.Where(x => x.Invoice == Convert.ToString(saledata[17]) && x.Item == Convert.ToInt32(arr[0]) && x.type == "Sale").Select(y => y.Quantity).Sum();
                        if (arr[0] != null)
                        {
                            DataRow dr = dtItem.NewRow();
                            dr["ItemUnit"] = arr[1];
                            dr["ItemUnitPrice"] = Convert.ToDecimal(arr[4]);
                            dr["ItemQuantity"] = (qty > 0) ? qty : Convert.ToDecimal(arr[2]);
                            dr["ItemSubTotal"] = Convert.ToDecimal(arr[6]);
                            dr["ItemDiscount"] = Convert.ToDecimal((arr[7] == "") ? 0 : Convert.ToDecimal(arr[7]));
                            dr["ItemTax"] = Convert.ToDecimal(arr[11]);//arr[10]
                            dr["ItemTaxAmount"] = Convert.ToDecimal(arr[10]);
                            dr["ItemTotalAmount"] = Convert.ToDecimal(arr[12]);
                            if (arr[29] == "http://")
                                dr["itemNote"] = "";
                            if (arr.Count() > 30)
                                dr["itemNote"] = Convert.ToString(arr[33].Replace("http://", ""));
                            else
                                dr["itemNote"] = "";


                            dr["SaleEntry"] = salesEntryId;
                            dr["Item"] = Convert.ToInt32(arr[0]);
                            dr["Type"] = false;
                            dtItem.Rows.Add(dr);



                            //---production-------//
                            /*var dt = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));

                            long itemID = Convert.ToInt32(arr[0]);
                            var ifbill = db.BillOfMaterialsoffers.Any(o => o.ItemId == itemID && o.BOMDateStart <= dt && o.BOMDateEnd >= dt);
                            bool available = ifbill;
                            if (ifbill)
                            {

                                var bomid = db.BillOfMaterialsoffers.Where(o => o.ItemId == itemID && o.BOMDateStart <= dt && o.BOMDateEnd >= dt).Select(o => o.BOMOfferId).FirstOrDefault();
                                var reqitem = db.BOMItemsoffers.Where(o => o.BOMOfferId == bomid).ToList();
                                if (bomid > 0 && reqitem.Count() > 0)
                                {
                                    foreach (var reqit in reqitem)
                                    {
                                        available = true;
                                    }
                                    if (available && com.GetItemWisestock(itemID, MC) < 1)
                                    {
                                        createproduction(bomid, salesEntryId, MC);
                                    }
                                }
                            }*/
                        }
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
                            long typ = Convert.ToInt64(saledata[36]);
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
                                              ItemUnitPrice = (SEentry.SaleType == SaleType.Sale) ? a.ItemSubTotal : hir,
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
                                if (SEentry.SalesType == 2)
                                {
                                    itemtax = 0;
                                    taxamt = 0;
                                    totamt = ItemSubTotal;
                                }
                                else
                                {
                                    itemtax = bu.ItemTax;
                                    taxamt = buTaxAmount;
                                    totamt = (buTaxAmount + ItemSubTotal);
                                }

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
                                dbu["SaleEntry"] = salesEntryId;
                                dbu["Item"] = bu.Item;
                                dbu["Type"] = false;
                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }


                    ////// create parameter
                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeSEItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertSEItems", "@TableType");
                    //// execute sql
                    db.Database.ExecuteSqlRaw(sql, parameter);
                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, salesEntryId); // forward-correctness: header = SUM(lines)
                    Int64 Usedmaterials = 0;
                    if (arrayused != null)
                    {
                        if (protask == null)
                            Usedmaterials = SaveUsedMaterials(UserId, Branch, saledata, arrayused, salesEntryId, null);
                        else
                            Usedmaterials = SaveUsedMaterials(UserId, Branch, saledata, arrayused, salesEntryId, protask);
                    }
                    var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                    if (commission != null)
                    {
                        foreach (var cm in commission)
                        {
                            commission cmm = new commission
                            {
                                agent = Convert.ToInt64(cm.agent),
                                commisionmode = Convert.ToInt32(cm.commisionmode),
                                commisiontype = Convert.ToInt32(cm.commisiontype),
                                comvalue = Convert.ToInt64(cm.comvalue),
                                salesid = salesEntryId
                            };
                            db.commissions.Add(cmm);
                            db.SaveChanges();
                        }
                    }

                    // batch stock
                    if (bstmodel != null)
                    {

                        foreach (var bst in bstmodel)
                        {
                            if (bst.BatchNo != "" && bst.BatchNo != null)
                            {
                                DateTime? exp = null;
                                DateTime? mfg = null;
                                if (bst.EXP != null && bst.EXP != "")
                                {
                                    exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                                }
                                if (bst.MFG != null && bst.MFG != "")
                                {
                                    mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                                }
                                decimal bStock = 0;
                                if (bst.Unit == bst.Priunit)
                                {
                                    bStock = bst.StockOut * bst.cfactor;
                                }
                                BatchStock Btst = new BatchStock();
                                Btst.BatchNo = bst.BatchNo;
                                Btst.Item = bst.Item;
                                Btst.Unit = bst.Unit;
                                Btst.Cost = bst.Cost;
                                Btst.StockOut = bStock;
                                Btst.StockIn = 0;
                                Btst.Order = bst.Order;
                                Btst.EXP = exp;
                                Btst.MFG = mfg;
                                Btst.Reference = salesEntryId;
                                Btst.Type = "Sales";

                                Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                Btst.Date = date;


                                db.BatchStocks.Add(Btst);
                                db.SaveChanges();
                            }
                        }

                    }


                    // batch stock in usedmaterials
                    if (ubstmodel != null)
                    {
                        foreach (var bst in ubstmodel)
                        {
                            if (bst.BatchNo != "" && bst.BatchNo != null)
                            {
                                DateTime? exp = null;
                                DateTime? mfg = null;
                                if (bst.EXP != null && bst.EXP != "")
                                {
                                    exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                                }
                                if (bst.MFG != null && bst.MFG != "")
                                {
                                    mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                                }
                                decimal bStock = 0;
                                if (bst.Unit == bst.Priunit)
                                {
                                    bStock = bst.StockOut * bst.cfactor;
                                }
                                BatchStock Btst = new BatchStock();
                                Btst.BatchNo = bst.BatchNo;
                                Btst.Item = bst.Item;
                                Btst.Unit = bst.Unit;
                                Btst.Cost = bst.Cost;
                                Btst.StockOut = bStock;
                                Btst.StockIn = 0;
                                Btst.Order = bst.Order;
                                Btst.EXP = exp;
                                Btst.MFG = mfg;
                                Btst.Reference = salesEntryId;
                                Btst.Type = "UsedSales";

                                Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                Btst.Date = date;


                                db.BatchStocks.Add(Btst);
                            }
                        }
                        db.SaveChanges();
                    }

                    if (bsrackData != null)
                    {
                        foreach (var bst in bsrackData)
                        {
                            if (bst.StockOut != 0)
                            {

                                decimal bStockIn = 0;

                                shelfstockmovement Btst = new shelfstockmovement();
                                Btst.purpose = "Sales";
                                Btst.itemid = bst.Item;
                                Btst.unitid = (long)bst.Unit;
                                Btst.rackmciid = (long)com.getrackmcid(MC, bst.RackNo, bst.ShelfNo);
                                Btst.qty = bst.StockOut;

                                Btst.referenceid = salesEntryId;


                                Btst.createddate = DateTime.Now;
                                Btst.createdby = UserId;

                                db.shelfstockmovements.Add(Btst);
                            }
                        }
                        db.SaveChanges();
                    }
                    //billsundry
                    if (bonusclaim > 0)
                    {
                        long discountre = 497;
                        saleamount = saleamount - bonusclaim;
                        com.addAccountTrasaction(0, bonusclaim, discountre, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);

                        customerbonus cusb = new customerbonus();
                        cusb.claimamount = bonusclaim;
                        cusb.customerid = Convert.ToInt64(saledata[0]);
                        cusb.salesentryid = salesEntryId;
                        db.customerbonus.Add(cusb);
                        db.SaveChanges();

                    }
                    string bsResult = string.Empty;
                    if (bsmodel.sebsundrys != null)
                    {
                        DataTable BsEntry = new DataTable();
                        BsEntry.Columns.Add("SalesEntry");
                        BsEntry.Columns.Add("BillSundry");
                        BsEntry.Columns.Add("BsValue");
                        BsEntry.Columns.Add("AmountType");
                        BsEntry.Columns.Add("BsType");
                        BsEntry.Columns.Add("BsAmount");

                        foreach (var bs in bsmodel.sebsundrys)
                        {
                            DataRow drw = BsEntry.NewRow();
                            drw["SalesEntry"] = salesEntryId;
                            drw["BillSundry"] = bs.BillSundry;
                            drw["BsValue"] = bs.BsValue;
                            drw["AmountType"] = bs.AmountType;
                            drw["BsType"] = bs.BsType;
                            drw["BsAmount"] = bs.BsAmount;

                            BsEntry.Rows.Add(drw);
                        }


                        ////// create parameter 
                        SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                        parameter1.SqlDbType = SqlDbType.Structured;
                        parameter1.TypeName = "TableTypeSEBillSundry";
                        //// execute sp sql 
                        string sql1 = String.Format("EXEC {0} {1};", "SP_InsertSEBillSundry", "@TableType");
                        //// execute sql 
                        db.Database.ExecuteSqlRaw(sql1, parameter1);
                        //-------------------------------------
                    }

                    decimal amount = Convert.ToDecimal(saledata[10]);
                    Int64 custAccID = db.Customers.Where(a => a.CustomerID == SEentry.Customer).Select(a => a.Accounts).FirstOrDefault();

                    Int64 saleAccId = saleAcc;//db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).FirstOrDefault();
                    if (SEentry.Project != null && SEentry.Project != 0)
                    {
                        saleAccId = db.Projects.Where(a => a.ProjectId == SEentry.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                    }

                    Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                    Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).FirstOrDefault();

                    //bill sundry account
                    var Gtotal = SEGrandTotal;
                    decimal deductions = 0;
                    if (bsmodel.sebsundrys != null)
                    {
                        foreach (var bs in bsmodel.sebsundrys)
                        {
                            var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                            if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                            {
                                decimal bsamount = 0;
                                if (bs.BsAmount == null)
                                    bsamount = 0;
                                else
                                    bsamount = (decimal)bs.BsAmount;

                                if (ChkAcc.BSType == 0)//additive
                                {
#pragma warning disable IDE0054 // Use compound assignment
                                    saleamount = saleamount - bsamount;
#pragma warning restore IDE0054 // Use compound assignment
                                    com.addAccountTrasaction(0, bsamount, (long)ChkAcc.SAccount, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);
                                }
                                else //substract
                                {
                                    saleamount = saleamount + bsamount;
                                    com.addAccountTrasaction(bsamount, 0, (long)ChkAcc.SAccount, "Sale", salesEntryId, DC.Debit, date, null, null, SEentry.Project, SEentry.ProTask);
                                }
                            }
                            else
                            {
                                decimal bsamount = 0;
                                if (bs.BsAmount == null)
                                    bsamount = 0;
                                else
                                    bsamount = (decimal)bs.BsAmount;
                                if (ChkAcc.BSType == 0)//additive
                                {
#pragma warning disable IDE0054 // Use compound assignment
                                    deductions = deductions - bsamount;
#pragma warning restore IDE0054 // Use compound assignment
                                }
                                else //substract
                                {
                                    deductions = deductions + bsamount;
                                }

                            }
                        }
                    }
                    bool? status = null;

                    if (SEentry.SalesType == 3)
                    {//voucher wise
                        com.addAccountTrasaction(0, SEentry.SESubTotal - deductions, saleAccId, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);
                    }
                    else
                    {
                        com.addAccountTrasaction(0, saleamount, saleAccId, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);
                    }
                    // add vat output in account transaction
                    if (TaxAmount > 0 && SEentry.SalesType != 3)
                        com.addAccountTrasaction(0, TaxAmount, VATOutput, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);

                    //add trasaction to sale account with sale entry credit amount
                    com.addAccountTrasaction(SEGrandTotal, 0, custAccID, "Sale", salesEntryId, DC.Debit, date, null, null, SEentry.Project, SEentry.ProTask);


                    if (TenderingMode == "inactive" || (Mode == "CancelFromSettlement" && SettlementData == null))
                    {
                        //SEPayment
                        SEPayment SEpay = new SEPayment();
                        SEpay.SEDate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                        SEpay.SEEntryDate = Convert.ToDateTime(System.DateTime.Now);
                        SEpay.SEBillAmount = Convert.ToDecimal(saledata[7]);//saledata[7]
                                                                            //walking customer
                        if (saledata[12] == "1" || saledata[12] == "2")
                        {
                            SEpay.SEPaidAmount = Convert.ToDecimal(saledata[7]);
                        }
                        else
                        {

                            SEpay.SEPaidAmount = Convert.ToDecimal(saledata[10]);
                        }
                        SEpay.CustomerId = Convert.ToInt64(saledata[0]);//saledata[0]
                        SEpay.CreatedBranch = Convert.ToInt32(BranchID);
                        SEpay.CreatedUserId = UserId;
                        SEpay.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        SEpay.Status = 1;
                        SEpay.SalesEntry = salesEntryId;
                        db.SEPayments.Add(SEpay);
                        db.SaveChanges();

                        //card or cash
                        if (saledata[12] == "2" || saledata[12] == "1")
                        {
                            // payment method
                            long? paymethod = (long?)Convert.ToInt64(saledata[20]);
                            cashAccId = paymethod == 0 ? cashAccId : (long)db.PaymentMethods.Where(a => a.PaymentMethodId == paymethod).Select(a => a.AccountId).FirstOrDefault();
                            //AccountsTransaction
                            amount = SEGrandTotal;
                        }


                        if (Convert.ToDecimal(saledata[10]) > 0 || (saledata[12] == "2" || saledata[12] == "1"))
                        {
                            var Remark = "Direct Reciept From Sale Entry";
                            long payid;
                            //SETransaction
                            SETransaction SEtran = new SETransaction();
                            SEtran.CustomerId = Convert.ToInt64(saledata[0]); //saledata[0]
                            SEtran.SEPayDate = date;
                            SEtran.SEPayAmount = amount;
                            payid = com.addReceipt(date, custAccID, cashAccId, amount, amount, Remark, UserId, BranchID, salesEntryId);
                            SEtran.Recieptid = payid;
                            SEtran.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            SEtran.CreatedBranch = Convert.ToInt32(BranchID);
                            SEtran.CreatedUserId = UserId;
                            SEtran.Status = 1;
                            SEtran.SalesEntry = salesEntryId;
                            db.SETransactions.Add(SEtran);
                            db.SaveChanges();
                        }

                        ////string Narration = null, long? project = null, long? task = null)
                        //    //add sale trasaction with customer debt amount


                        if (Convert.ToDecimal(saledata[10]) > 0 || (saledata[12] == "2" || saledata[12] == "1"))
                        {
                            //if payment
                            com.addAccountTrasaction(0, amount, custAccID, "Sale Payment", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);
                            com.addAccountTrasaction(amount, 0, cashAccId, "Sale Payment", salesEntryId, DC.Debit, date, null, null, SEentry.Project, SEentry.ProTask);
                        }


                    }
                    else
                    {
                        //Call Function to save settlement details
                        UpdateSettlement(SettlementData, salesEntryId, SEentry.SEDate, SEentry.Customer, SEentry.Project, SEentry.ProTask, SEentry.SEGrandTotal, BranchID, custAccID);
                    }


                    //Approved By
                    var Appby = Convert.ToString(saledata[53]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = salesEntryId;
                            approval.Type = "SalesEntry";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                    var addtask = Convert.ToString(saledata[52]);
                    int i = 0;
                    if (addtask != null && addtask != "")
                    {

                        long[] addtaskar = addtask.Split(',').Select(Int64.Parse).ToArray();

                        additionaltaks adt = new additionaltaks();
                        foreach (var emp in addtaskar)
                        {
                            if (i == 0)
                            {
                                SEentry = db.SalesEntrys.Find(salesEntryId);

                                SEentry.ProTask = (emp != null || emp != 0) ? emp : 0;
                                db.Entry(SEentry).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                adt.salesentryid = salesEntryId;
                                adt.taskid = emp;

                                db.additionaltasks.Add(adt);
                                db.SaveChanges();

                            }
                            i++;
                        }
                    }
                    com.addlog(LogTypes.Created, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Successfully Submitted Sales Entry");
                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                    Int64 SReturnId = 0;
                    var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
                    var returninsales = returninsale != null ? returninsale.Status : Status.inactive;

                    //Save Sales Return Details Only If 'Sales Return In Sales Entry' is Active in Configuration
                    if (returninsales == Status.active)
                    {
                        if (arrayR != null)
                        {
                            var paytype = Convert.ToString(saledata[56]);
                            SReturnId = SaveSalesReturn(salesEntryId, UserId, Branch, MCRet, saledata, bsmodel, arrayR, paytype, "CreateMode");
                        }
                    }

                    var autopur = db.EnableSettings.Where(a => a.EnableType == "stockcheckinvoice").FirstOrDefault();
                    var autopurcheck = autopur != null ? autopur.Status : Status.inactive;

                    if (autopurcheck == Status.active)
                    {

                        var paytype = Convert.ToString(saledata[56]);

                    }


                    //send mail to company address
                    var sendmail = db.EnableSettings.Where(a => a.EnableType == "AutomaticMailInTransactions").FirstOrDefault();
                    var sendcmail = sendmail != null ? sendmail.Status : Status.inactive;
                    if (sendcmail == Status.active)
                    {
                        var custname = db.Customers.Where(a => a.CustomerID == SEentry.Customer).Select(a => a.CustomerName).FirstOrDefault();
                        var salesman = db.Employees.Where(a => a.EmployeeId == SEentry.SECashier).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
                        var PayType = (SEentry.CustomerType == CustomerType.Card ? (db.PaymentMethods.Where(a => a.PaymentMethodId == SEentry.PaymentMethod).Select(a => a.MethodName).FirstOrDefault()) : (SEentry.CustomerType == CustomerType.Walking ? "Cash" : "Credit"));
                        var username = db.Users.Where(a => a.Id == SEentry.CreatedBy).Select(a => a.UserName).FirstOrDefault();

                        CompanyEmailFormat CEmail = new CompanyEmailFormat();
                        CEmail.BillNo = "Tax invoice-" + SEentry.BillNo;
                        CEmail.Subject = "<table style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;' colspan='2' align='center'><b>Tax Invoice Created </b></td><tr/> " +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Type               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + Enum.GetName(typeof(SaleType), SEentry.SaleType) + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Date               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SEDate.ToString("dd-MM-yyyy") + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Customer           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + custname + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Sales Executive    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + salesman + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Payment Type       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + PayType + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created Date       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SECreatedDate.ToString("dd-MM-yyyy hh:mm:ss tt") + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created User       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + username + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Sub Total          :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SESubTotal + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Tax Amount         :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SETaxAmount + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Discount           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SEDiscount + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><b> Grand Total    :</b></td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><td><b> " + SEentry.SEGrandTotal + "</b></td><tr/></table>";

                        com.SendToCompanyMail(CEmail);
                    }

                    var ConvertFrom = Convert.ToString(saledata[54]);
                    var ConvertBill = Convert.ToString(saledata[55]);

                    if (action == "print")
                    {
                        var conv = db.ConvertTransactionss.Any(u => u.To == salesEntryId);
                        List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
                        if (conv)
                        {
                            List<string> ExList = new List<string>();
                            List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                            ExtList = ExtNumDetails((long)salesEntryId, ExtList);
                            var Extended = ExtList.Select(z => z.To).ToList();
                            Int32 count = 0;


                            var ConvModel = (from a in db.ConvertTransactionss
                                             join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                                             from b in primary.DefaultIfEmpty()
                                             where Extended.Contains(a.To)
                                             select new ConvertTransactionsViewModel
                                             {
                                                 ConvertFrom = (a.ConvertFrom == "SaleExtend") ? "Sale" : (a.ConvertFrom == "Quote") ? "Quotation" : (a.ConvertFrom == "DVNote") ? "Delivery Note" : a.ConvertFrom,
                                                 Id = b.SalesEntryId,
                                                 BillNo = b.BillNo,
                                                 CreatedDate = a.CreatedDate,
                                                 From = a.From
                                             }).OrderBy(b => b.CreatedDate).ToList();

                            var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                            ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                            parentvm.BillNo = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                            parentvm.Id = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.SalesEntryId).FirstOrDefault();
                            parentvm.ConvertFrom = "SaleExtend";
                            ConvModel.Add(parentvm);
                            ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                            var str = ConvExt.Find(c => c.Id == salesEntryId);
                            ConvExt.Remove(str);


                            //ConvExt = (from a in db.ConvertTransactionss
                            //           where Extended.Contains(a.To)

                            //               Id = a.Id,
                            //               BillNo = b.BillNo,
                            //               CreatedDate = a.CreatedDate,
                            //               From = a.From
                        }
                        //sales

                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                        var saleData = com.SaleData(salesEntryId, InPrintItemCode, HideItmNameIfDiscriptionOn, PartNoCheck, TimeOut, ProjectCheck, ConvertFrom, ConvertBill);
                        var item = saleData.pdfItem.ToList();
                        var summary = saleData;
                        var billsundry = saleData.billsundry.ToList();

                        var fmapp = db.FieldMappings.Where(a => a.Section == "Sales" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                        var st = ConvExt.Find(c => c.BillNo == SEentry.BillNo);
                        ConvExt.Remove(st);
                        var ConvExtList = ConvExt;

                        //sales return
                        object itemR = "";
                        object summaryR = "";
                        object billsundryR = "";
                        if (SReturnId != 0)
                        {
                            var saleRetData = com.SalesReturnData(SReturnId, InPrintItemCode, PartNoCheck, TimeOut);
                            itemR = saleRetData["item"];
                            summaryR = saleRetData["summary"];
                            billsundryR = saleRetData["billsundry"];
                        }
                        var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                        var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                        var def = (PriLay == Status.active) ? Convert.ToInt64(saledata[62]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, itemR, summaryR, billsundryR, layout, ConvExtList, fmapp, salesEntryId } };
                    }
                    else if (action == "sendmail")
                    {
                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = saledata[19];
                        string CcMail = "";
                        string InvoiceNo = "_TaxInvoice_" + SEentry.BillNo;
                        string mess = db.EmailTemplates.Find(3L).EmailBody;
                        var em = db.EmailTemplates.Where(a => a.Head == "TaxInvoice").FirstOrDefault();
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;

                            message.Subject = "Tax Invoice";







                            DateTime datenow = DateTime.Now;
                            var creditperiod = db.Customers.Find(Convert.ToInt64(saledata[0])).CreditPeriod;

                            decimal ret = 0;
                            long CustId = Convert.ToInt64(saledata[0]);

                            var ConD = (from a in db.Customers
                                        join c in db.Employees on a.SalesPerson equals c.EmployeeId into secondary
                                        from c in secondary.DefaultIfEmpty()
                                        join d in db.CustomerTyps on a.CustomerType equals d.TypeId into temp
                                        from d in temp.DefaultIfEmpty()
                                        where a.CustomerID == CustId

                                        let Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0)
                                        let Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0)

                                        select new
                                        {
                                            a.Accounts,
                                            CusType = a.CustomerType == null ? "" : d.Type,
                                            CreditLimit = a.CreditLimit == null ? 0 : a.CreditLimit,
                                            currentbalance = ((Debit > Credit) ? ((Debit - Credit)) : 0),
                                            acbalance = (Debit > Credit) ? ((Debit - Credit) + " Dr.") : ((Credit - Debit) + " Cr."),
                                            mob = (from co in db.Contacts
                                                   join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                                   join con in db.Country on co.CountryID equals con.CountryID into conn
                                                   from con in conn.DefaultIfEmpty()
                                                   where (rrr.RelationID == CustId && rrr.RelationType == 0)
                                                   select new MobileViewModel
                                                   {
                                                       Num = "+" + con.CountryCode + co.Mobile,
                                                       Name = co.FirstName + "  " + co.LastName,
                                                       emails = co.EmailId,
                                                   }).ToList(),
                                            pdc = (decimal?)(from b in db.AccountsTransactions
                                                             join c in db.Customers on b.Account equals c.Accounts

                                                             where b.Status != null && c.CustomerID == CustId
                                                             group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                                                             select new
                                                             {

                                                                 GrandTotal = g.Sum(o => o.Credit)


                                                             }).Sum(x => x.GrandTotal) ?? 0,
                                            //pdc = (decimal?)(

                                            //       r.MOPayment == ModeOfPayment.PDC
                                            //           r.GrandTotal

                                            //        ).Sum(x => x.GrandTotal) ?? 0,

                                            ddlEmployee = c.EmployeeId == null ? 0 : c.EmployeeId,

                                            billage = (from a in db.SalesEntrys
                                                       join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                                                       from b in pay.DefaultIfEmpty()
                                                       join c in db.Customers on a.Customer equals c.CustomerID into rec
                                                       from c in rec.DefaultIfEmpty()
                                                       join d in db.SalesReturns on a.SalesEntryId equals d.SalesEntryId into slret
                                                       from d in slret.DefaultIfEmpty()

                                                       where
                                                         (a.Customer == CustId) &&
                                                         (c.CreditPeriod > 0)




                                                       select new
                                                       {
                                                           id = a.SalesEntryId,
                                                           Date = a.SEDate,
                                                           Invoice = a.BillNo,
                                                           c.CreditPeriod,
                                                           Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - ((d.SRGrandTotal == null) ? 0 : d.SRGrandTotal)),
                                                           total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                                                           paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                                                           Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                                                           Ctyp = "Cus"
                                                       }).Where(o => o.Amount > 3 && o.Days > creditperiod).OrderByDescending(o => o.Days).Distinct().Take(5).ToList(),
                                            includepdc = a.includepdc,

                                        }).ToList();











                            mess = mess.Replace("|pdc|", ConD[0].pdc.ToString());
                            mess = mess.Replace("|balance|", (ConD[0].currentbalance - ((ConD[0].pdc == null) ? 0 : ConD[0].pdc)).ToString());

                            message.Body = mess;

                        }
                        if (mess.Contains("|attachment|") || mess.Contains("|attachement|"))
                            sm.SendPdfMail(generatePdf(salesEntryId), ToMail, CcMail, InvoiceNo, message);
                        else
                            sm.sendMailwithoutattachment(ToMail, CcMail, InvoiceNo, message);


                        var conv = db.ConvertTransactionss.Any(u => u.To == salesEntryId);
                        List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
                        if (conv)
                        {
                            List<string> ExList = new List<string>();
                            List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                            ExtList = ExtNumDetails((long)salesEntryId, ExtList);
                            var Extended = ExtList.Select(z => z.To).ToList();
                            Int32 count = 0;


                            var ConvModel = (from a in db.ConvertTransactionss
                                             join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                                             from b in primary.DefaultIfEmpty()
                                             where Extended.Contains(a.To)
                                             select new ConvertTransactionsViewModel
                                             {
                                                 ConvertFrom = (a.ConvertFrom == "SaleExtend") ? "Sale" : (a.ConvertFrom == "Quote") ? "Quotation" : (a.ConvertFrom == "DVNote") ? "Delivery Note" : a.ConvertFrom,
                                                 Id = b.SalesEntryId,
                                                 BillNo = b.BillNo,
                                                 CreatedDate = a.CreatedDate,
                                                 From = a.From
                                             }).OrderBy(b => b.CreatedDate).ToList();

                            var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                            ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                            parentvm.BillNo = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                            parentvm.Id = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.SalesEntryId).FirstOrDefault();
                            parentvm.ConvertFrom = "SaleExtend";
                            ConvModel.Add(parentvm);
                            ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                            var str = ConvExt.Find(c => c.Id == salesEntryId);
                            ConvExt.Remove(str);


                            //ConvExt = (from a in db.ConvertTransactionss
                            //           where Extended.Contains(a.To)

                            //               Id = a.Id,
                            //               BillNo = b.BillNo,
                            //               CreatedDate = a.CreatedDate,
                            //               From = a.From
                        }
                        //sales

                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                        var saleData = com.SaleData(salesEntryId, InPrintItemCode, HideItmNameIfDiscriptionOn, PartNoCheck, TimeOut, ProjectCheck, ConvertFrom, ConvertBill);
                        var item = saleData.pdfItem.ToList();
                        var summary = saleData;
                        var billsundry = saleData.billsundry.ToList();

                        var fmapp = db.FieldMappings.Where(a => a.Section == "Sales" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                        var st = ConvExt.Find(c => c.BillNo == SEentry.BillNo);
                        ConvExt.Remove(st);
                        var ConvExtList = ConvExt;

                        //sales return
                        object itemR = "";
                        object summaryR = "";
                        object billsundryR = "";
                        if (SReturnId != 0)
                        {
                            var saleRetData = com.SalesReturnData(SReturnId, InPrintItemCode, PartNoCheck, TimeOut);
                            itemR = saleRetData["item"];
                            summaryR = saleRetData["summary"];
                            billsundryR = saleRetData["billsundry"];
                        }
                        var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                        var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                        var def = (PriLay == Status.active) ? Convert.ToInt64(saledata[62]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, itemR, summaryR, billsundryR, layout, ConvExtList, fmapp, salesEntryId } };


                    }
                    else
                    {
                        msg = "Successfully submitted Sales Entry.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, SalesEntryID = salesEntryId } };
                    }
                    //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Invoice No. Already Exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                msg = "Sorry This Conversion Not possible";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        public Int64 SaveSalesReturn(long salesEntryId, string UserId, long Branch, long Mc, string[] saledata, SEBillSundryViewModel bsmodel, string[][] arrayR, string ptype, string Mode)
        {
            var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
            var today = Convert.ToDateTime(System.DateTime.Now);
            string action = "";
            decimal TaxAmount = 0, GrandTotal = 0, saleramount = 0;
            Int64 salesRetId = 0;
            if (Mode == "EditMode")
            {
                var SalesRtn = db.SalesReturns.Where(a => a.SalesEntryId == salesEntryId).FirstOrDefault();

                //Header Table
                if (SalesRtn != null)
                {
                    //Sales Return Detail Table
                    var SalesRtnDtl = db.SRItemss.Where(a => a.SalesReturnId == SalesRtn.SalesReturnId).FirstOrDefault();

                    if (SalesRtnDtl != null)
                    {
                        db.SRItemss.RemoveRange(db.SRItemss.Where(a => a.SRItemsId == SalesRtnDtl.SRItemsId));
                        db.SaveChanges();
                    }

                    //billsundry
                    var SRBs = db.SRBillSundrys.Where(a => a.SalesReturnId == SalesRtn.SalesReturnId).FirstOrDefault();
                    if (SRBs != null)
                    {
                        db.SRBillSundrys.RemoveRange(db.SRBillSundrys.Where(a => a.SalesReturnId == SalesRtn.SalesReturnId));
                        db.SaveChanges();
                    }

                    db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.SalesReturnId == SalesRtn.SalesReturnId));
                    db.Payments.RemoveRange(db.Payments.Where(a => a.Reference == SalesRtn.SalesReturnId && a.RefType == "SalesReturn"));
                    db.SRPayments.RemoveRange(db.SRPayments.Where(a => a.SalesReturnId == SalesRtn.SalesReturnId));
                    db.SaveChanges();

                    bool delete = com.DeleteAllAccountTransaction("Sale Return", SalesRtn.SalesReturnId);
                    bool deletepay = com.DeleteAllAccountTransaction("Sale Return Payment", SalesRtn.SalesReturnId);

                    db.SalesReturns.Remove(SalesRtn);
                    db.SaveChanges();
                }
            }

            if (arrayR != null)
            {
                SalesReturn SRentry = new SalesReturn();
                Int64 saleRAcc = (long)db.companys.Select(a => a.SReturnAccount).FirstOrDefault();

                SRentry.SalesEntryId = salesEntryId;
                SRentry.SRDate = date;
                SRentry.SRCashier = saledata[1] != "" ? Convert.ToInt64(saledata[1]) : 0;
                SRentry.SaleType = SaleType.Sale;

                //walkin customer
                SRentry.CustomerType = (ptype == "1") ? CustomerType.Walking : CustomerType.Customer;
                SRentry.Customer = Convert.ToInt64(saledata[0]);
                SRentry.SReturnAccount = saleRAcc;
                if (Mode == "CreateMode")
                {
                    action = saledata[15];
                    SRentry.SRNo = GetSRNo();
                    TaxAmount = Convert.ToDecimal(saledata[43]);
                    GrandTotal = Convert.ToDecimal(saledata[45]);
                    SRentry.BillNo = Convert.ToString(saledata[37]);
                    SRentry.SRItems = Convert.ToInt32(saledata[39]);
                    SRentry.SRItemQuantity = Convert.ToDecimal(saledata[40]);
                    SRentry.SRSubTotal = Convert.ToDecimal(saledata[41]);
                    SRentry.SRTax = Convert.ToDecimal(saledata[42]);
                    SRentry.SRDiscount = Convert.ToDecimal(saledata[44]);
                    SRentry.SRNote = saledata[47];
                    SRentry.Remarks = saledata[48];
                }
                else
                {
                    action = saledata[18];
                    TaxAmount = Convert.ToDecimal(saledata[47]);
                    GrandTotal = Convert.ToDecimal(saledata[48]);
                    SRentry.SRItems = Convert.ToInt32(saledata[49]);
                    SRentry.SRItemQuantity = Convert.ToDecimal(saledata[50]);
                    SRentry.SRSubTotal = Convert.ToDecimal(saledata[51]);
                    SRentry.SRTax = Convert.ToDecimal(saledata[52]);
                    SRentry.SRDiscount = Convert.ToDecimal(saledata[53]);
                    SRentry.SRNote = saledata[54];
                    SRentry.Remarks = saledata[55];
                    SRentry.BillNo = Convert.ToString(saledata[56]);
                    SRentry.SRNo = Convert.ToInt64(saledata[56]);
                }

                saleramount = GrandTotal - TaxAmount;

                SRentry.ReturnType = ReturnType.Direct;
                //pay type for pos
                SRentry.PayType = "";//need change           
                SRentry.SRTaxAmount = TaxAmount;
                SRentry.SRGrandTotal = GrandTotal;
                SRentry.Print = 1;
                SRentry.SRCreatedDate = today;
                SRentry.CreatedBy = UserId;
                SRentry.Status = 1;
                SRentry.Branch = Branch;
                SRentry.MaterialCenter = Mc;

                db.SalesReturns.Add(SRentry);
                db.SaveChanges();

                salesRetId = SRentry.SalesReturnId;

                // add to SRItem
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
                dtItem.Columns.Add("itemNote");
                dtItem.Columns.Add("SalesReturnId");
                dtItem.Columns.Add("Item");

                foreach (var arr in arrayR)
                {
                    DataRow dr = dtItem.NewRow();
                    var item = Convert.ToInt32(arr[0]);

                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                    dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                    dr["ItemTax"] = arr[10] == null ? 0 : Convert.ToDecimal(arr[10]);
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                    dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                    dr["SalesReturnId"] = salesRetId;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    dtItem.Rows.Add(dr);

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
                        var bundle = (from a in db.BundleItems
                                      join b in db.Items on a.ItemId equals b.ItemID
                                      join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                      from c in primary.DefaultIfEmpty()
                                      where a.ItemBundle == itemBundle.ItemBundleId
                                      select new
                                      {
                                          b.ItemCode,
                                          b.ItemName,
                                          c.ItemUnitName,
                                          ItemUnitPrice = a.ItemUnitPrice,
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
                            dbu["SalesReturnId"] = salesRetId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }
                }

                ////// create parameter 
                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypeSRItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertSRItems", "@TableType");
                //// execute sql 
                db.Database.ExecuteSqlRaw(sql, parameter);

                //billsundry  
                if (bsmodel.sebsundryr != null)
                {
                    string bsResult = string.Empty;

                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("SalesReturnId");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.sebsundryr)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["SalesReturnId"] = salesRetId;
                        drw["BillSundry"] = bs.BillSundry;
                        drw["BsValue"] = bs.BsValue;
                        drw["AmountType"] = bs.AmountType;
                        drw["BsType"] = bs.BsType;
                        drw["BsAmount"] = bs.BsAmount;

                        BsEntry.Rows.Add(drw);
                    }

                    ////// create parameter 
                    SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                    parameter1.SqlDbType = SqlDbType.Structured;
                    parameter1.TypeName = "TableTypeSRBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertSRBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);
                }

                //SRPayment
                SRPayment SRpay = new SRPayment();

                SRpay.CustomerId = Convert.ToInt64(saledata[0]);
                SRpay.SRDate = date;
                SRpay.SREntryDate = today;
                SRpay.SRBillAmount = GrandTotal;

                if (ptype == "1")
                {
                    SRpay.SReturnAmount = GrandTotal;
                }
                else
                {
                    if (Mode == "CreateMode")
                        SRpay.SReturnAmount = Convert.ToDecimal(saledata[46]);
                    else
                        SRpay.SReturnAmount = Convert.ToDecimal(saledata[57]);
                }

                SRpay.CreatedBranch = Convert.ToInt32(Branch);
                SRpay.CreatedUserId = UserId;
                SRpay.SRCreatedDate = today;
                SRpay.Status = 1;
                SRpay.SalesReturnId = salesRetId;
                db.SRPayments.Add(SRpay);
                db.SaveChanges();

                decimal amount = 0;

                if (Mode == "CreateMode")
                    amount = Convert.ToDecimal(saledata[46]);
                else
                    amount = Convert.ToDecimal(saledata[57]);

                Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == SRentry.Customer).Select(a => a.Accounts).FirstOrDefault();
                Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).FirstOrDefault();
                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();

                //walkin customer
                if (ptype == "1")
                {
                    //AccountsTransaction
                    amount = GrandTotal;
                }
                if (amount > 0 || ptype == "1")
                {

                    var Remark = "Direct Payment From SalesReturn";
                    long payid;
                    //SETransaction
                    SRTransaction SRtran = new SRTransaction();

                    SRtran.CustomerId = Convert.ToInt64(saledata[0]);
                    SRtran.SRPayDate = date;
                    //walkin customer
                    if (ptype == "1")
                    {
                        amount = GrandTotal;
                        SRtran.SRPayAmount = amount;
                    }
                    else
                    {
                        amount = Convert.ToDecimal(saledata[10]);
                        SRtran.SRPayAmount = amount;
                    }

                    payid = com.addPayment(date, cashAccId, custAccID, amount, amount, amount, Remark, UserId, Branch, salesRetId, "SalesReturn");

                    SRtran.PaymentId = payid;
                    SRtran.SRCreatedDate = today;
                    SRtran.CreatedBranch = Convert.ToInt32(Branch);
                    SRtran.CreatedUserId = UserId;
                    SRtran.Status = 1;
                    SRtran.SalesReturnId = salesRetId;

                    db.SRTransactions.Add(SRtran);
                    db.SaveChanges();
                }

                //add trasaction to sale account
                com.addAccountTrasaction(saleramount, 0, saleAccId, "Sale Return", salesRetId, DC.Debit, date);
                //add sale trasaction 
                com.addAccountTrasaction(0, GrandTotal, custAccID, "Sale Return", salesRetId, DC.Credit, date);
                // add vat input in account transaction
                if (TaxAmount > 0)
                    com.addAccountTrasaction(TaxAmount, 0, VATOutput, "Sale Return", salesRetId, DC.Debit, date);
                if (amount > 0 || ptype == "1")
                {
                    //if payment
                    com.addAccountTrasaction(amount, 0, custAccID, "Sale Return Payment", salesRetId, DC.Debit, date);
                    com.addAccountTrasaction(0, amount, cashAccId, "Sale Return Payment", salesRetId, DC.Credit, date);
                }
                com.addlog(LogTypes.Created, UserId, "SalesReturn", "SalesReturns", findip(), salesRetId, "Successfully Submitted Sales Return");

            }
            return salesRetId;
        }

        public Int64 SavePurhase(long salesEntryId, string UserId, long Branch, long Mc, string[] saledata, string[][] array, SEBillSundryViewModel bsmodel, string ptype, string Mode)
        {
            var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
            var today = Convert.ToDateTime(System.DateTime.Now);
            string action = "";
            decimal TaxAmount = 0, GrandTotal = 0, saleramount = 0;
            Int64 salesRetId = 0;
            var sebill = db.SalesEntrys.Where(o => o.SalesEntryId == salesEntryId).Select(o => o.BillNo).FirstOrDefault();
            if (Mode == "EditMode")
            {
                var purchaseentry = db.PurchaseEntrys.Where(a => a.BillNo == sebill).FirstOrDefault();

                //Header Table
                if (purchaseentry != null)
                {
                    //Sales Return Detail Table
                    var peitemss = db.PEItemss.Where(a => a.PurchaseEntry == purchaseentry.PurchaseEntryId).FirstOrDefault();

                    if (peitemss != null)
                    {
                        db.PEItemss.RemoveRange(db.PEItemss.Where(a => a.PurchaseEntry == peitemss.PurchaseEntry));
                        db.SaveChanges();
                    }



                    bool delete = com.DeleteAllAccountTransaction("Purchase", purchaseentry.PurchaseEntryId);

                    db.PurchaseEntrys.Remove(purchaseentry);
                    db.SaveChanges();
                }
            }


            PurchaseEntry pentry = new PurchaseEntry();
            Int64 PurchaseAcc = (long)db.companys.Select(a => a.PurchaseAccount).FirstOrDefault();

            pentry.BillNo = sebill;
            pentry.PEDate = date;
            pentry.PECashier = saledata[1] != "" ? Convert.ToInt64(saledata[1]) : 0;

            //walkin customer
            pentry.Supplier = Convert.ToInt64(saledata[0]);
            pentry.PurchaseAccount = PurchaseAcc;
            if (Mode == "CreateMode")
            {
                action = saledata[15];
                pentry.PENo = GetPeNo();
                TaxAmount = Convert.ToDecimal(saledata[43]);
                GrandTotal = Convert.ToDecimal(saledata[45]);
                pentry.BillNo = Convert.ToString(saledata[37]);
                pentry.PEItems = Convert.ToInt32(saledata[39]);
                pentry.PEItemQuantity = Convert.ToDecimal(saledata[40]);
                pentry.PESubTotal = Convert.ToDecimal(saledata[41]);
                pentry.PETax = Convert.ToDecimal(saledata[42]);
                pentry.PEDiscount = Convert.ToDecimal(saledata[44]);

            }
            else
            {
                action = saledata[18];
                TaxAmount = 0;
                GrandTotal = 0;
                pentry.PEItems = 0;
                pentry.PEItemQuantity = 0;
                pentry.PESubTotal = 0;
                pentry.PETax = 0;
                pentry.PEDiscount = 0;


            }

            saleramount = GrandTotal - TaxAmount;


            //pay type for pos
            pentry.PayType = "";//need change           
            pentry.PETaxAmount = 0;
            pentry.PEGrandTotal = 0;
            pentry.Print = 1;
            pentry.PECreatedDate = today;
            pentry.CreatedBy = UserId;
            pentry.Status = 1;
            pentry.Branch = Branch;
            pentry.MaterialCenter = Mc;

            db.PurchaseEntrys.Add(pentry);
            db.SaveChanges();

            var purchaseentryid = pentry.PurchaseEntryId;

            // add to SRItem
            string result = string.Empty;
            decimal grandtotal = 0;

            foreach (var arr in array)
            {
                var item = Convert.ToInt64(arr[0]);
                var items = db.Items.Find(item);

                if (items.accmap == true)
                {

                    PEItems dItem = new PEItems();
                    dItem.ItemUnit = Convert.ToInt64(arr[1]);
                    dItem.ItemUnitPrice = items.PurchasePrice;
                    dItem.ItemQuantity = Convert.ToDecimal(arr[2]);
                    dItem.ItemSubTotal = items.PurchasePrice * Convert.ToDecimal(arr[2]);
                    dItem.ItemDiscount = 0;
                    dItem.ItemTax = 0;
                    dItem.ItemTaxAmount = 0;
                    dItem.ItemTotalAmount = items.PurchasePrice * Convert.ToDecimal(arr[2]);

                    dItem.PurchaseEntry = purchaseentryid;
                    dItem.Item = item;

                    db.PEItemss.Add(dItem);
                    db.SaveChanges();



                    decimal amount = items.PurchasePrice * Convert.ToDecimal(arr[2]);
                    grandtotal = grandtotal + amount;

                    Int64 supplieracid = db.Suppliers.Where(a => a.SupplierID == items.Supplier).Select(a => a.Accounts).FirstOrDefault();

                    PEPayment PEpay = new PEPayment();

                    PEpay.SupplierId = (long)items.Supplier;
                    PEpay.PEDate = pentry.PEDate;
                    PEpay.PEEntryDate = Convert.ToDateTime(System.DateTime.Now);
                    PEpay.PEBillAmount = amount;


                    PEpay.PEPaidAmount = 0;


                    PEpay.CreatedBranch = Branch;
                    PEpay.CreatedUserId = UserId;
                    PEpay.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    PEpay.Status = 1;
                    PEpay.PurchaseEntry = purchaseentryid;
                    db.PEPayments.Add(PEpay);
                    db.SaveChanges();


                    //if payment
                    //add trasaction to purchase account

                    com.addAccountTrasaction(amount, 0, PurchaseAcc, "Purchase", purchaseentryid, DC.Debit, date);

                    com.addAccountTrasaction(0, amount, supplieracid, "Purchase", purchaseentryid, DC.Credit, date);
                }
            }
            db.PurchaseEntrys.Where(o => o.PurchaseEntryId == purchaseentryid).ToList().ForEach(o => o.PEGrandTotal = grandtotal);
            db.SaveChanges();
            com.addlog(LogTypes.Created, UserId, "purchaseentry", "purchaseentry", findip(), salesRetId, "Successfully Submitted Sales Return");


            return purchaseentryid;
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Entry List,No Tax Sales,My ProTask")]
        public ActionResult GetSalesEntry(long? vouchertype, string InvoiceNo, long? salesstatus, string FromDate, string ToDate, long? customer, long? salesperson, long? paymethod, long? type, string user, int? Balance, string Saletype, long? HireType, long? MC, string appstat, long? ProjectName, long? Task, string Ref1, string Ref2, string Ref3, string Ref4, string Ref5, string customername)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            Int64 temp = 502;
            // Dev/superuser sees every entry (it already unlocks all actions on this screen and is the
            // first role in [QkAuthorize]); otherwise full visibility requires "All Sales Entry"/"My ProTask",
            // and everyone else is scoped to their own entries by the where-clause below. Without the Dev
            // bypass an admin login that lacks the separate "All Sales Entry" grant sees an empty grid.
            var userpermission = (User.IsInRole("Dev") == true) ? true : (User.IsInRole("All Sales Entry")==true)?true:( User.IsInRole("My ProTask") ==true)?true:false;
            var UserId = User.Identity.GetUserId();
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            var cType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer : CustomerType.Card;

            paymethod = paymethod == 0 ? null : paymethod;

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //saleToDVconversion
            var SaleToDvNote = db.EnableSettings.Where(a => a.EnableType == "SaletoDVNConvert").FirstOrDefault();
            var SaleToDvNotes = SaleToDvNote != null ? SaleToDvNote.Status : Status.inactive;

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var admclist1 = db.AdditionalMc.Where(a => a.UserId == UserId).Select(a => (long?)a.McId).ToList();
            var admclist = admclist1;
            if (admclist.Count() > 0)
            {
                MCList.AddRange(admclist);
            }
            var MCArray = MCList.ToArray();

            SaleType St = new SaleType();
            if (Saletype != "")
            {
                St = (Saletype == "2") ? SaleType.Hire : SaleType.Sale;
            };

            var fromv = "Sale";
            var tov = "SaleExtend";
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

            long? EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uCreditSalaryView = User.IsInRole("View Sales Entry");
            var uEdit = User.IsInRole("Edit Sales Entry");
            var uDownload = User.IsInRole("Download Sales Entry");
            var uDelete = User.IsInRole("Delete Sales Entry");
            //saleToDVconversion
            var ToDVN = "DVNote";
            //search
            string SearchW = "";
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                SearchW = search.ToLower();
            }
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
                editableDay = today.AddYears(-10);
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddMinutes(-userEditDays);
            }


            //editnew
            var temm = 0;
            var todayy = DateTime.Now;
            var editableDayy = DateTime.Now;
            var userEditDayss = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.seitem).FirstOrDefault();
            var userEditt = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDayss == 0 && userEditt != 0)
            {
                editableDayy = todayy.AddYears(-10);
            }
            else if (userEditt == 0)
            {
                temm = 1;

            }
            else
            {
                editableDayy = todayy.AddMinutes(-userEditDayss);
            }







            long[] ar = { 1, 3 };
            if (vouchertype == 2)
            {
                ar = new long[] { 2 };
            }
            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry into pays
                     from c in pays.DefaultIfEmpty()
                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.PaymentMethods on a.PaymentMethod equals e.PaymentMethodId into paymeth
                     from e in paymeth.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.MCs on a.MaterialCenter equals i.MCId into mcs
                     from i in mcs.DefaultIfEmpty()
                     join j in db.Projects on a.Project equals j.ProjectId into prj
                     from j in prj.DefaultIfEmpty()
                     join jj in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = temp }
                   equals new { j1 = jj.reference, j2 = jj.Purpose, j3 = jj.Account } into hir1
                     from jj in hir1.DefaultIfEmpty()
                     join l in db.Employees on b.SalesPerson equals l.EmployeeId into lemp
                     from l in lemp.DefaultIfEmpty()

                         // let dvn = db.ConvertTransactionss.Where(ap => ap.From == a.SalesEntryId && ap.ConvertFrom == fromv && ap.ConvertTo == ToDVN).FirstOrDefault()
                         // let sh = db.ConvertTransactionss.Where(ap => ap.From == a.SalesEntryId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()

                         //let app = db.Approvals.Where(x => x.TransEntry == a.SalesEntryId && x.Type == "SalesEntry").Select(x => (long?)x.EmployeeId).ToList()
                         //let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.SalesEntryId && x.Type == "SalesEntry").Select(x => x.ApprovalStatus).ToList()
                         //let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.SalesEntryId && x.Type == "SalesEntry").GroupBy(l => l.ApprovedBy)
                         //.Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                         // .ToList().Select(a => a.ApprovalStatus).ToList()

                     where a.Status == 1 &&
                     (userpermission == true || b.SalesPerson == empl.EmployeeId || a.SECashier == empl.EmployeeId || a.CreatedBy == UserId) &&
                     (customername == "" || customername == null || a.customername == customername) &&
                     (ar.Contains(a.SalesType)) &&
                     (InvoiceNo == "" || a.BillNo.Contains(InvoiceNo)) &&
                     (FromDate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                     (ToDate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                     (customer == 0 || a.Customer == customer) &&
                     (salesperson == 0 || salesperson == null || a.SECashier == salesperson) &&
                     (type == null || a.CustomerType == cType) &&
                     (paymethod == null || a.PaymentMethod == paymethod)
                     && (MC == null || a.MaterialCenter == MC)
                     // && ((MCArray.Contains(a.MaterialCenter) && MC == a.MaterialCenter) || ((MC == null) && MCArray.Contains(a.MaterialCenter)))
                     && ((Balance == null) || (Balance == 1 ? (((decimal?)a.SEGrandTotal ?? 0) > ((decimal?)c.SEPaidAmount)) : (((decimal?)a.SEGrandTotal ?? 0) == ((decimal?)c.SEPaidAmount))))
                     && (Saletype == "" || Saletype == null || St == a.SaleType) && (HireType == 0 || HireType == null || HireType == h.HireType)
                     && (ProjectName == 0 || ProjectName == null || j.ProjectId == ProjectName)
                     //&& (Task == 0 || Task == null || k.ProTaskId == Task)
                    && (salesstatus == null || a.SalesStatus == salesstatus)
                     && (SearchW == "" || a.BillNo.ToLower().Contains(SearchW) || (a.customername != null && a.customername.ToLower().Contains(SearchW))) &&
                    (Ref1 == "" || Ref1 == null || a.Ref1 == Ref1) &&
                    (Ref2 == "" || Ref2 == null || a.Ref2 == Ref2) &&
                    (Ref3 == "" || Ref3 == null || a.Ref3 == Ref3) &&
                    (Ref4 == "" || Ref4 == null || a.Ref4 == Ref4) &&
                    (Ref5 == "" || Ref5 == null || a.Ref5 == Ref5)
                     select new
                     {
                         DVNConvert = "",// dvn.ConvertTo,
                         validornot = tem != 1 && (EF.Functions.DateDiffMinute(a.SECreatedDate, editableDay) <= 0 && EF.Functions.DateDiffMinute(a.SECreatedDate, today) >= 0) ? "valid" : "invalid",
                         userEditDays = userEditDays,
                         validornott = temm != 1 && (EF.Functions.DateDiffMinute(a.SECreatedDate, editableDayy) <= 0 && EF.Functions.DateDiffMinute(a.SECreatedDate, todayy) >= 0) ? "valid" : "invalid",
                         userEditDayss = userEditDayss,
                         tax = jj.Credit == null ? 0 : jj.Credit,
                         a.SalesEntryId,
                         a.SENo,
                         HExtent = 0,// sh.ConvertFrom,
                         //HExtent = "sss",
                         a.BillNo,
                         a.PONo,
                         a.SEDate,
                         a.SEGrandTotal,
                         Customer = (b == null) ? "Not Select Customer" : (b.CustomerCode + " - " + b.CustomerName),
                         EmpName = d.FirstName + " " + d.LastName,
                         custemp = l.FirstName + " " + l.LastName,
                         user = g.UserName,
                         a.CustomerType,
                         PayType = (a.CustomerType == CustomerType.Card ? "Card" : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                         PaymentStatus = (c == null) ? 0 : c.Status,
                         PaymentTrans = db.SETransactions.Any(k => k.SalesEntry == a.SalesEntryId),
                         SEPaidAmount = (c == null) ? 0 : c.SEPaidAmount,
                         BalanceAmt = (c != null) ? ((decimal?)a.SEGrandTotal ?? 0) - ((decimal?)c.SEPaidAmount ?? 0) : 0,
                         a.Location,
                         a.Remarks,
                         SaleType = a.SaleType,
                         Dev = uDev,
                         Details = uCreditSalaryView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         MC = i.MCName,
                         ProjectCode = (j.ProCode != null) ? j.ProCode : "",
                         ProjectNames = (j.ProjectName != null && j.ProjectName != "") ? j.ProjectName : "",
                         Task = "",// (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",
                         app = "",
                         AppStatus = "",
                         chkAppStatus = "",
                         CreatedDate = a.SECreatedDate

                     }).ToList().Select(o => new
                     {
                         o.DVNConvert,
                         o.tax,
                         o.validornot,
                         o.userEditDays,
                         o.SalesEntryId,
                         o.SENo,
                         o.HExtent,
                         o.BillNo,
                         o.PONo,
                         o.SEDate,
                         o.SEGrandTotal,
                         o.Customer,
                         o.EmpName,
                         o.user,
                         o.CustomerType,
                         o.PayType,
                         o.PaymentStatus,
                         o.PaymentTrans,
                         o.SEPaidAmount,
                         o.BalanceAmt,
                         o.Location,
                         o.Remarks,
                         o.SaleType,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.MC,
                         o.app,
                         o.ProjectCode,
                         o.ProjectNames,
                         o.Task,
                         o.custemp,
                         Approval = true,// (o.app != null && EmployeeId != null) ? (o.app.Contains(EmployeeId) ? true : false) : false,
                         ApprovalStatus = ApprovalStatus.Approved,//(o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         SaleToDvNConvert = SaleToDvNotes,
                         o.validornott,
                         o.userEditDayss
                     });

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
        public ActionResult GetSEItems2(long SalesEntryID)
        {
            var ConD = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.SalesEntry == SalesEntryID && a.itemNote != "-:{Bundle_Item}"
                        orderby a.SEItemsId
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
                            note = a.itemNote.Replace("<br />", "\n"),
                            ItemNote = a.itemNote != null ? a.itemNote : "",
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
                            b.KeepStock
                        });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }
        //Function for retrieving details in Used Material List
        [HttpPost]
        public ActionResult GetUsdMatrlsDetails(string InvoiceNo, long? Customer, long? Task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            //search
            string SearchW = "";
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                SearchW = search.ToLower();
            }

            var v = (from a in db.SalesEntrys
                     join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                     join c in db.Customers on a.Customer equals c.CustomerID
                     join d in db.ProTasks on a.ProTask equals d.ProTaskId into task
                     from d in task.DefaultIfEmpty()
                     join e in db.Items on b.Item equals e.ItemID
                     join f in db.ItemUnits on b.ItemUnit equals f.ItemUnitID into unit
                     from f in unit.DefaultIfEmpty()
                     where
                     (InvoiceNo == "" || a.BillNo==InvoiceNo) &&
                     (Customer == 0 || a.Customer == Customer) &&
                     (Task == 0 || Task == null || d.ProTaskId == Task)
                     && b.Type == true
                     select new
                     {
                         a.SalesEntryId,
                         e.ItemName,
                         f.ItemUnitName,
                         b.ItemQuantity,

                         b.ItemSubTotal,
                         b.ItemTaxAmount,
                         e.ConFactor,
                         e.SubUnitId,
                         e.ItemUnitID,
                         b.ItemUnit,
                         ItemUnitPrice = b.ItemUnitPrice,// (e.SubUnitId == b.ItemUnit) ? e.SellingPrice / e.ConFactor : e.SellingPrice,
                     }).ToList().Select(o => new
                     {
                         o.SalesEntryId,
                         o.ItemName,
                         o.ItemUnitName,
                         o.ItemQuantity,
                         //ItemUnitPrice=(o.SubUnitId==o.ItemUnit)?o.ItemUnitPrice/o.ConFactor:o.ItemUnitPrice,
                         o.ItemUnitPrice,
                         o.ItemSubTotal,
                         o.ItemTaxAmount,
                         Total = o.ItemUnitPrice * o.ItemQuantity
                     });

            //SEARCH
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(
                    p => p.ItemName.ToString().ToLower().Contains(search.ToLower()) ||
                    p.ItemName.ToString().ToLower().StartsWith(search.ToLower()) ||
                    p.ItemName.ToString().ToLower().EndsWith(search.ToLower()));
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

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry,No Tax Sales")]
        [HttpGet]
        public ActionResult Edit(long? id, long? saletype)
        {
            var enbonusforcustomer = db.EnableSettings.Where(a => a.EnableType == "bonusforcustomer").FirstOrDefault();
            var bonusforcustomer = enbonusforcustomer != null ? enbonusforcustomer.Status : Status.inactive;
            ViewBag.bonuscust = bonusforcustomer;

            ViewBag.taxexeceptinvoice = 1;
            if (saletype != null)
            {
                ViewBag.taxexeceptinvoice = 0;
            }
            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;
            ViewBag.SuperUserEditvalue = SuperUserEditvalue;

            var enablepricetratagy = db.EnableSettings.Where(a => a.EnableType == "enablepricestratagy").FirstOrDefault();
            var pricetratagy = enablepricetratagy != null ? enablepricetratagy.Status : Status.inactive;
            ViewBag.pricestratagy = pricetratagy;


            var pri = db.PriceCategories.Where(o => o.active == false)
                      .Select(s => new
                      {
                          ID = s.value,
                          Name = s.description
                      }).ToList();

            pri.Insert(0, db.PriceCategories.Where(o => o.active == true)
                             .Select(s => new
                             {
                                 ID = s.value,
                                 Name = s.description
                             }).FirstOrDefault());
            ViewBag.pricecategorylist = QkSelect.List(pri, "ID", "Name");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var EnableRackWise = db.EnableSettings.Where(a => a.EnableType == "RackWiseStock").FirstOrDefault();
            var RackCheck = EnableRackWise != null ? EnableRackWise.Status : Status.inactive;
            ViewBag.RackWiseStock = RackCheck;
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
                editableDay = today.AddYears(-10);
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddMinutes(-userEditDays);
            }


            SalesEntry Saleentry = db.SalesEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesEntryId == id).FirstOrDefault();
            if (SuperUserEditvalue == Status.active)
            {
                DateTime dt = System.DateTime.Now;
                var f = db.otpapproves.Where(o => o.entryid == id && o.purpose == "Sales" && o.requestedby == UserId && o.expdate > dt && o.approvedby == UserId).FirstOrDefault();
                if (f != null)
                {
                    editableDay = editableDay.AddDays(-1000);
                }
            }
            if ((Saleentry.SECreatedDate - editableDay).TotalMinutes < 0 || tem == 1)
            {
                return NotFound();

            }
            var imp = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();

            var empmcs = db.AdditionalMc.Where(o => o.UserId == UserId).Select(o => o.McId).ToArray();
            var data2222 = (from a in db.accountmaps
                            join b in db.Accountss
                            on a.AccountId equals b.AccountsID
                            where (a.EmployeeId == imp)
                            select new
                            {
                                EmployeeId = a.EmployeeId,
                                AccountId = a.AccountId,
                                AccountNames = b.Name,
                                PaymentType = a.PaymentTypeId,
                                Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                                //ChequeNo,
                                //ChequeDate
                            }).ToList();



            var Gtotal = db.SalesEntrys.Where(x => (x.SalesEntryId == id)).Select(x => x.SEGrandTotal).FirstOrDefault();
            ViewBag.Gtotal = Gtotal;

            SalesEntry Salentry = db.SalesEntrys.Find(id);
            ViewBag.disableupdate = User.IsInRole("Disable Update Option In Sales");
            if (Saleentry == null)
            {
                return NotFound();
            }
            var EnableCreditLimit = db.EnableSettings.Where(a => a.EnableType == "CreditLimit").FirstOrDefault();
            var AutoCreditLimit = EnableCreditLimit != null ? EnableCreditLimit.Status : Status.inactive;
            ViewBag.AutoCreditLimit = AutoCreditLimit;
            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;


            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;
            if (Saleentry.SalesStatus == 0)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
                ViewBag.sstatus = pstat;

            }
            else if (Saleentry.SalesStatus == 1)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                }
              };
                ViewBag.sstatus = pstat;
            }
            else
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.sstatus = pstat;
            }

            //Fetching the image from AttachmentDocuments
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.SalesEntrys
                             on a.TransactionID equals b.SalesEntryId
                             where a.TransactionID == id && a.TransactionType == "CreditSale"
                             select new CreditSaleDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 CreditSaleId = a.TransactionID,
                                 FileName = a.FileName,
                                 CreatedDate = a.CreatedDate
                             }).ToList();
            //................................................

            Int64 cashier = Convert.ToInt64(Saleentry.SECashier);
            Int64 customer = Saleentry.Customer;

            string custname = "";
            SalesEntryViewModel vmodel = new SalesEntryViewModel();

            custname = db.Customers.Where(a => a.CustomerID == customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();

            //    CustomerID = s.CustomerID,
            //    CustomerDetails = s.CustomerCode + " - " + s.CustomerName


            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var use = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;
            List<SelectFormat> mcss;
            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName
                mcss = db.MCs.Where(s => s.MCId == Saleentry.MaterialCenter).Select(s => new SelectFormat
                {
                    text = s.MCName,
                    id = s.MCId
                }).OrderBy(b => b.text).ToList().ToList();


                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();
                serialisedJson = mcss.Union(serialisedJson).ToList();
                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                ViewBag.MC = QkSelect.List(serialisedJson, "id", "text");
                ViewBag.LastMc = serialisedJson.Select(a => a.id).FirstOrDefault();
            }

            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

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

            var proj = db.Projects.Where(o => o.ProjectId == vmodel.Project)
              .Select(s => new
              {
                  ID = s.ProjectId,
                  Name = s.ProCode + " " + s.ProjectName
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            })
            .ToList();
            var saleentytask = Saleentry.ProTask;
            var ss = db.ProTasks.Find(Saleentry.ProTask);
            if (ss != null)
            {
                var saleentrytaskname = ss.TaskCode + "-" + ss.TaskName;
                var tsk2 = (from a in db.additionaltasks
                            join b in db.ProTasks on a.taskid equals b.ProTaskId
                            where a.salesentryid == id

                            select new
                            {
                                ID = a.taskid,
                                Name = b.TaskCode + "-" + b.TaskName
                            })
               .ToList();
                tsk2.Add(new { ID = (long)saleentytask, Name = saleentrytaskname });
                vmodel.ProTasks = tsk2.Select(o => o.ID).ToArray();
                ViewBag.getProTask = new MultiSelectList(tsk2, "ID", "Name", tsk2.Select(o => o.ID).ToArray());

            }
            else
            {
                var tskd = db.ProTasks.Where(o => o.ProTaskId == -1)
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskCode + "-" + s.TaskName
             })
             .ToList();
                ViewBag.getProTask = QkSelect.List(tskd, "ID", "Name");
            }

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesEntry").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var CBill = "";
            var CType = "";
            DateTime createdDate = new DateTime();
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Sale").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "Quote")
                {
                    CBill = db.Quotations.Where(a => a.QuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "DVNote")
                {
                    CBill = db.Deliverynotes.Where(a => a.DeliverynoteId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                    createdDate = db.Deliverynotes.Where(c => c.BillNo == CBill).Select(c => c.DvDate).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "ProForma")
                {
                    CBill = db.ProFormas.Where(a => a.ProFormaId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "SOrder")
                {
                    CBill = db.SalesOrders.Where(a => a.SalesOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }
            var users = db.LogManagers.Where(o => o.LogType == LogTypes.Created && o.LogID == id.ToString() && o.LogTable == "SalesEntrys").Select(o => o.User).FirstOrDefault();
            var impids = db.Employees.Where(o => o.UserId == users).Select(o => o.EmployeeId).FirstOrDefault();
            var data2 = (from a in db.accountmaps
                         join b in db.Accountss
                         on a.AccountId equals b.AccountsID
                         where (a.EmployeeId == impids && a.PaymentTypeId != EmployeePaymentType.Account)
                         select new
                         {
                             EmployeeId = a.EmployeeId,
                             AccountId = a.AccountId,
                             AccountNames = b.Name,
                             PaymentType = a.PaymentTypeId,
                             Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                             //ChequeNo,
                             //ChequeDate
                         }).ToList();
            var impids2 = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
            var data22 = (from a in db.accountmaps
                          join b in db.Accountss
                          on a.AccountId equals b.AccountsID
                          where (a.EmployeeId == impids2 && a.PaymentTypeId != EmployeePaymentType.Account)
                          select new
                          {
                              EmployeeId = a.EmployeeId,
                              AccountId = a.AccountId,
                              AccountNames = b.Name,
                              PaymentType = a.PaymentTypeId,
                              Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                              //ChequeNo,
                              //ChequeDate
                          }).ToList();
            ViewBag.anothershowroom = 0;
            if (users != UserId && data2.Count() > 0)
            {
                ViewBag.anothershowroom = 1;
            }

            if (data2.Count() > 0)
            {
                ViewBag.settmode = "active";
                ViewBag.settmodeempid = data2.FirstOrDefault().EmployeeId;
            }
            ViewBag.superuser = false;
            ViewBag.superuser = User.IsInRole("Delete Sales Entry");

            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry into sepay
                      from c in sepay.DefaultIfEmpty()
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.SalesEntryId, f2 = "Sales" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.SalesEntryId == id
                      select new SalesEntryViewModel
                      {
                          SENo = b.SENo,
                          PONo = b.PONo,
                          SENote = b.SENote,
                          SEDate = b.SEDate,
                          BillNo = b.BillNo,
                          pricecategoryid = b.pricecategoryid,
                          SECashier = b.SECashier,
                          Customer = b.Customer,
                          SEDiscount = b.SEDiscount,
                          SEGrandTotal = b.SEGrandTotal,
                          // SEPaidAmount = c.SEPaidAmount,
                          CustomerType = b.CustomerType,
                          // SEDueAmount = b.SEGrandTotal - c.SEPaidAmount,
                          CustomerName = custname,
                          PaymentMethod = b.PaymentMethod,
                          custEmailId = e.EmailId,
                          SalesType = b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          Location = b.Location,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          Currency = b.Currency,
                          DConvertionRate = b.ConvertionRate,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          HSCode = b.HSCode,
                          PaymentTerms = b.PaymentTerms,
                          Project = b.Project,
                          ProTask = b.ProTask,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          DVNotDate = createdDate,
                      }).FirstOrDefault();
            if (ConvertTran != null)
            {
                if (ConvertTran.ConvertFrom == "DVNote")
                {
                    DateTime createdDate2 = db.Deliverynotes.Where(c => c.BillNo == CBill).Select(c => c.DvDate).FirstOrDefault();
                    string dateString = String.Format("{0:dd/MM/yyyy}", createdDate2);

                    ViewBag.dvdate = dateString;
                }
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

            companySet();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();


            ViewBag.preEntry = db.SalesEntrys.Where(a => a.SalesEntryId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.SalesEntrys.Where(a => a.SalesEntryId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Min();


            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var paymethod = db.PaymentMethods.FirstOrDefault();
            ViewBag.payVisible = paymethod != null ? true : false;

            var list = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                            }, "Value", "Text", 1);

            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();
            if (vmodel != null)
            {
                if (vmodel.PaymentMethod == null)
                {
                    ViewBag.PayMethod = list;
                }
                else
                {
                    ViewBag.PayMethod = QkSelect.List(pay, "ID", "Name");
                }
            }
            _FinancialYear();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.ContType = "SalesEntry";

            ViewBag.PopUpAddCust = false;



            var EditPermission = User.IsInRole("Disable Sale Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "SalesEntry", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            //field mapping
            if (vmodel != null)
                vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();

            var ref1 = db.SalesEntrys
               .Select(s => new
               {
                   ID = s.Ref1,
                   Name = s.Ref1
               }).Distinct()
               .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", Salentry.Ref1);
            var cusomernames = db.SalesEntrys
           .Select(s => new
           {
               ID = s.customername,
               Name = s.customername
           }).Distinct()
           .ToList().OrderBy(a => a.Name);
            ViewBag.customernamenotax = QkSelect.List(cusomernames, "ID", "Name", Salentry.customername);

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", Salentry.Ref2);

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", Salentry.Ref3);

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", Salentry.Ref4);

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", Salentry.Ref5);
            long rtask = 11;
            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();
            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            //dummy table operations
            var DItem = db.DummySEItems.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SItem = db.SEItemss.Where(a => a.SalesEntry == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummySEItems.Where(a => a.SalesEntry == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    SEItems sItem = new SEItems();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.itemNote = arr.itemNote;
                    sItem.SalesEntry = arr.SalesEntry;
                    sItem.Item = arr.Item;
                    db.SEItemss.Add(sItem);
                    db.SaveChanges();
                }

                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, id); // forward-correctness: header = SUM(lines)
                db.DummySEItems.RemoveRange(db.DummySEItems.Where(a => a.SalesEntry == id));
                db.SaveChanges();
            }

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var usedmaterial = db.EnableSettings.Where(a => a.EnableType == "Usedmaterials").FirstOrDefault();
            var usedmat = usedmaterial != null ? usedmaterial.Status : Status.inactive;
            ViewBag.UsedMaterials = usedmat;

            var discountper = db.Users.Where(x => x.Id == UserId).Select(y => y.Discount).FirstOrDefault();
            ViewBag.Discountpercent = discountper;

            var TaxInclusive = db.EnableSettings.Where(a => a.EnableType == "TaxInclusive").FirstOrDefault();
            var TaxInclusives = TaxInclusive != null ? TaxInclusive.Status : Status.inactive;
            ViewBag.TaxInclusive = TaxInclusives;

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

            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;
            ViewBag.ReturnInSale = returninsales;
            var perce = db.PriceCategories.Where(o => o.pricestratagyid == vmodel.pricecategoryid).Select(o => o.value).FirstOrDefault();
            ViewBag.priceper = perce;
            var rtype = Request.Query["rtype"];
            var cust = (from a in db.Customers
                        join b in db.Accountss on a.Accounts equals b.AccountsID

                        where a.Type == CRMCustomerType.Customer &&
                        !a.CustomerName.StartsWith("OLD-")
                        && a.CustomerID == vmodel.Customer
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
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            else
            {
                if (vmodel.SalesType == 2)
                {
                    ViewBag.taxexeceptinvoice = 0;
                }
                vmodel.CustomerName = Saleentry.customername;
                if (bonusforcustomer == Status.active)
                {
                    var bon = db.customerbonus.Where(o => o.customerid == customer && o.salesentryid == id).Select(o => o.claimamount).FirstOrDefault();
                    if (bon != null)
                    {
                        vmodel.bonus = bon;
                    }
                }
                return View(vmodel);
            }

        }
        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Add Task Invoice")]
        public ActionResult UpdateTaskSale(string[][] array, string[] saledata, SEBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel, ICollection<UBatchStockPViewModel> ubstmodel, string[][] arrayused, ICollection<commissionViewmodel> commission, string TenderingMode, string Mode, ICollection<SettlementViewModel> SettlementData, decimal? BalanceAmount, string[][] arrayR, ICollection<RackStockPViewModel> bsrackData)
        {
            var UserId = User.Identity.GetUserId();
            long Branch = 0;
            string msg = "";
            bool stat = false;
            Int64 salesEntryId = Convert.ToInt64(saledata[15]);
            SalesEntry SEentry = db.SalesEntrys.Find(salesEntryId);
            Int64 Usedmaterials = 0;
            SEentry.Remarks = saledata[23];

            db.Entry(SEentry).State = EntityState.Modified;
            db.SaveChanges();
            db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == salesEntryId && a.Type == true));
            db.SaveChanges();

            if (arrayused != null)
            {
                Usedmaterials = SaveUsedMaterials(UserId, Branch, saledata, arrayused, salesEntryId, null);
            }

            var existingtask = db.additionaltasks.Where(o => o.salesentryid == salesEntryId).FirstOrDefault();
            if (existingtask != null)
            {
                db.additionaltasks.RemoveRange(db.additionaltasks.Where(o => o.salesentryid == salesEntryId));
                db.SaveChanges();
                SEentry = db.SalesEntrys.Find(salesEntryId);

                SEentry.ProTask = 0;
                db.Entry(SEentry).State = EntityState.Modified;
                db.SaveChanges();
            }
            var addtask = Convert.ToString(saledata[36]);


            int i = 0;
            if (addtask != null && addtask != "")
            {

                long[] addtaskar = addtask.Split(',').Select(Int64.Parse).ToArray();

                additionaltaks adt = new additionaltaks();
                foreach (var emp in addtaskar)
                {
                    if (i == 0)
                    {
                        SEentry = db.SalesEntrys.Find(salesEntryId);

                        SEentry.ProTask = (emp != null || emp != 0) ? emp : 0;
                        db.Entry(SEentry).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        adt.salesentryid = salesEntryId;
                        adt.taskid = emp;

                        db.additionaltasks.Add(adt);
                        db.SaveChanges();

                    }
                    i++;
                }
            }
            else
            {
                SEentry.ProTask = 0;

                db.Entry(SEentry).State = EntityState.Modified;
                db.SaveChanges();
            }

            msg = "Successfully Updated Task Sales Entry.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, SalesEntryID = salesEntryId } };

        }

        [RedirectingAction]
      //  [QkAuthorize(Roles = "Dev,Add Task Invoice")]
        [HttpGet]
        public ActionResult Edittask(long? id)
        {
            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;
            ViewBag.SuperUserEditvalue = SuperUserEditvalue;

            var enablepricetratagy = db.EnableSettings.Where(a => a.EnableType == "enablepricestratagy").FirstOrDefault();
            var pricetratagy = enablepricetratagy != null ? enablepricetratagy.Status : Status.inactive;
            ViewBag.pricestratagy = pricetratagy;


            var pri = db.PriceCategories.Where(o => o.active == false)
                      .Select(s => new
                      {
                          ID = s.value,
                          Name = s.description
                      }).ToList();

            pri.Insert(0, db.PriceCategories.Where(o => o.active == true)
                             .Select(s => new
                             {
                                 ID = s.value,
                                 Name = s.description
                             }).FirstOrDefault());
            ViewBag.pricecategorylist = QkSelect.List(pri, "ID", "Name");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var EnableRackWise = db.EnableSettings.Where(a => a.EnableType == "RackWiseStock").FirstOrDefault();
            var RackCheck = EnableRackWise != null ? EnableRackWise.Status : Status.inactive;
            ViewBag.RackWiseStock = RackCheck;
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var editableDay = DateTime.Now;

            var imp = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();

            var empmcs = db.AdditionalMc.Where(o => o.UserId == UserId).Select(o => o.McId).ToArray();
            var data2222 = (from a in db.accountmaps
                            join b in db.Accountss
                            on a.AccountId equals b.AccountsID
                            where (a.EmployeeId == imp)
                            select new
                            {
                                EmployeeId = a.EmployeeId,
                                AccountId = a.AccountId,
                                AccountNames = b.Name,
                                PaymentType = a.PaymentTypeId,
                                Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                                //ChequeNo,
                                //ChequeDate
                            }).ToList();



            var Gtotal = db.SalesEntrys.Where(x => (x.SalesEntryId == id)).Select(x => x.SEGrandTotal).FirstOrDefault();
            ViewBag.Gtotal = Gtotal;

            SalesEntry Salentry = db.SalesEntrys.Find(id);
            ViewBag.disableupdate = User.IsInRole("Disable Update Option In Sales");
            SalesEntry Saleentry = db.SalesEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesEntryId == id).FirstOrDefault();

            if (Saleentry == null)
            {
                return NotFound();
            }
            var EnableCreditLimit = db.EnableSettings.Where(a => a.EnableType == "CreditLimit").FirstOrDefault();
            var AutoCreditLimit = EnableCreditLimit != null ? EnableCreditLimit.Status : Status.inactive;
            ViewBag.AutoCreditLimit = AutoCreditLimit;
            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;


            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;
            if (Saleentry.SalesStatus == 0)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
                ViewBag.sstatus = pstat;

            }
            else if (Saleentry.SalesStatus == 1)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                }
              };
                ViewBag.sstatus = pstat;
            }
            else
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.sstatus = pstat;
            }

            //Fetching the image from AttachmentDocuments
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.SalesEntrys
                             on a.TransactionID equals b.SalesEntryId
                             where a.TransactionID == id && a.TransactionType == "CreditSale"
                             select new CreditSaleDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 CreditSaleId = a.TransactionID,
                                 FileName = a.FileName,
                                 CreatedDate = a.CreatedDate
                             }).ToList();
            //................................................

            Int64 cashier = Convert.ToInt64(Saleentry.SECashier);
            Int64 customer = Saleentry.Customer;

            string custname = "";
            SalesEntryViewModel vmodel = new SalesEntryViewModel();

            custname = db.Customers.Where(a => a.CustomerID == customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();

            //    CustomerID = s.CustomerID,
            //    CustomerDetails = s.CustomerCode + " - " + s.CustomerName


            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var use = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;
            List<SelectFormat> mcss;
            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName
                mcss = db.MCs.Where(s => s.MCId == Saleentry.MaterialCenter).Select(s => new SelectFormat
                {
                    text = s.MCName,
                    id = s.MCId
                }).OrderBy(b => b.text).ToList().ToList();


                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();
                serialisedJson = mcss.Union(serialisedJson).ToList();
                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                ViewBag.MC = QkSelect.List(serialisedJson, "id", "text");
                ViewBag.LastMc = serialisedJson.Select(a => a.id).FirstOrDefault();
            }

            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

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

            var proj = db.Projects.Where(o => o.ProjectId == vmodel.Project)
              .Select(s => new
              {
                  ID = s.ProjectId,
                  Name = s.ProCode + " " + s.ProjectName
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            })
            .ToList();
            var saleentytask = Saleentry.ProTask;
            var ss = db.ProTasks.Find(Saleentry.ProTask);
            if (ss != null)
            {
                var saleentrytaskname = ss.TaskCode + "-" + ss.TaskName;
                var tsk2 = (from a in db.additionaltasks
                            join b in db.ProTasks on a.taskid equals b.ProTaskId
                            where a.salesentryid == id

                            select new
                            {
                                ID = a.taskid,
                                Name = b.TaskCode + "-" + b.TaskName
                            })
               .ToList();
                tsk2.Add(new { ID = (long)saleentytask, Name = saleentrytaskname });
                vmodel.ProTasks = tsk2.Select(o => o.ID).ToArray();
                ViewBag.getProTask = new MultiSelectList(tsk2, "ID", "Name", tsk2.Select(o => o.ID).ToArray());

            }
            else
            {
                var tskd = db.ProTasks.Where(o => o.ProTaskId == -1)
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskCode + "-" + s.TaskName
             })
             .ToList();
                ViewBag.getProTask = QkSelect.List(tskd, "ID", "Name");
            }

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesEntry").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var CBill = "";
            var CType = "";
            DateTime createdDate = new DateTime();
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Sale").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "Quote")
                {
                    CBill = db.Quotations.Where(a => a.QuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "DVNote")
                {
                    CBill = db.Deliverynotes.Where(a => a.DeliverynoteId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                    createdDate = db.Deliverynotes.Where(c => c.BillNo == CBill).Select(c => c.DvDate).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "ProForma")
                {
                    CBill = db.ProFormas.Where(a => a.ProFormaId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "SOrder")
                {
                    CBill = db.SalesOrders.Where(a => a.SalesOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }
            var users = db.LogManagers.Where(o => o.LogType == LogTypes.Created && o.LogID == id.ToString() && o.LogTable == "SalesEntrys").Select(o => o.User).FirstOrDefault();
            var impids = db.Employees.Where(o => o.UserId == users).Select(o => o.EmployeeId).FirstOrDefault();
            var data2 = (from a in db.accountmaps
                         join b in db.Accountss
                         on a.AccountId equals b.AccountsID
                         where (a.EmployeeId == impids && a.PaymentTypeId != EmployeePaymentType.Account)
                         select new
                         {
                             EmployeeId = a.EmployeeId,
                             AccountId = a.AccountId,
                             AccountNames = b.Name,
                             PaymentType = a.PaymentTypeId,
                             Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                             //ChequeNo,
                             //ChequeDate
                         }).ToList();
            var impids2 = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
            var data22 = (from a in db.accountmaps
                          join b in db.Accountss
                          on a.AccountId equals b.AccountsID
                          where (a.EmployeeId == impids2 && a.PaymentTypeId != EmployeePaymentType.Account)
                          select new
                          {
                              EmployeeId = a.EmployeeId,
                              AccountId = a.AccountId,
                              AccountNames = b.Name,
                              PaymentType = a.PaymentTypeId,
                              Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                              //ChequeNo,
                              //ChequeDate
                          }).ToList();
            ViewBag.anothershowroom = 0;
            if (users != UserId && data2.Count() > 0)
            {
                ViewBag.anothershowroom = 1;
            }

            if (data2.Count() > 0)
            {
                ViewBag.settmode = "active";
                ViewBag.settmodeempid = data2.FirstOrDefault().EmployeeId;
            }
            ViewBag.superuser = false;
            ViewBag.superuser = User.IsInRole("Delete Sales Entry");

            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry into sepay
                      from c in sepay.DefaultIfEmpty()
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.SalesEntryId, f2 = "Sales" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.SalesEntryId == id
                      select new SalesEntryViewModel
                      {
                          SENo = b.SENo,
                          PONo = b.PONo,
                          SENote = b.SENote,
                          SEDate = b.SEDate,
                          BillNo = b.BillNo,
                          pricecategoryid = b.pricecategoryid,
                          SECashier = b.SECashier,
                          Customer = b.Customer,
                          SEDiscount = b.SEDiscount,
                          SEGrandTotal = b.SEGrandTotal,
                          // SEPaidAmount = c.SEPaidAmount,
                          CustomerType = b.CustomerType,
                          // SEDueAmount = b.SEGrandTotal - c.SEPaidAmount,
                          CustomerName = custname,
                          PaymentMethod = b.PaymentMethod,
                          custEmailId = e.EmailId,
                          SalesType = b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          Location = b.Location,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          Currency = b.Currency,
                          DConvertionRate = b.ConvertionRate,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          HSCode = b.HSCode,
                          PaymentTerms = b.PaymentTerms,
                          Project = b.Project,
                          ProTask = b.ProTask,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          DVNotDate = createdDate,
                      }).FirstOrDefault();
            if (ConvertTran != null)
            {
                if (ConvertTran.ConvertFrom == "DVNote")
                {
                    DateTime createdDate2 = db.Deliverynotes.Where(c => c.BillNo == CBill).Select(c => c.DvDate).FirstOrDefault();
                    string dateString = String.Format("{0:dd/MM/yyyy}", createdDate2);

                    ViewBag.dvdate = dateString;
                }
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

            companySet();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();


            ViewBag.preEntry = db.SalesEntrys.Where(a => a.SalesEntryId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.SalesEntrys.Where(a => a.SalesEntryId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Min();


            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var paymethod = db.PaymentMethods.FirstOrDefault();
            ViewBag.payVisible = paymethod != null ? true : false;

            var list = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                            }, "Value", "Text", 1);

            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();
            if (vmodel != null)
            {
                if (vmodel.PaymentMethod == null)
                {
                    ViewBag.PayMethod = list;
                }
                else
                {
                    ViewBag.PayMethod = QkSelect.List(pay, "ID", "Name");
                }
            }
            _FinancialYear();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.ContType = "SalesEntry";

            ViewBag.PopUpAddCust = false;



            var EditPermission = User.IsInRole("Disable Sale Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "SalesEntry", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            //field mapping
            if (vmodel != null)
                vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();

            var ref1 = db.SalesEntrys
               .Select(s => new
               {
                   ID = s.Ref1,
                   Name = s.Ref1
               }).Distinct()
               .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", Salentry.Ref1);

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", Salentry.Ref2);

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", Salentry.Ref3);

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", Salentry.Ref4);

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", Salentry.Ref5);
            long rtask = 11;
            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();
            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            //dummy table operations
            var DItem = db.DummySEItems.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SItem = db.SEItemss.Where(a => a.SalesEntry == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummySEItems.Where(a => a.SalesEntry == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    SEItems sItem = new SEItems();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.itemNote = arr.itemNote;
                    sItem.SalesEntry = arr.SalesEntry;
                    sItem.Item = arr.Item;
                    db.SEItemss.Add(sItem);
                    db.SaveChanges();
                }

                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, id); // forward-correctness: header = SUM(lines)
                db.DummySEItems.RemoveRange(db.DummySEItems.Where(a => a.SalesEntry == id));
                db.SaveChanges();
            }

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var usedmaterial = db.EnableSettings.Where(a => a.EnableType == "Usedmaterials").FirstOrDefault();
            var usedmat = usedmaterial != null ? usedmaterial.Status : Status.inactive;
            ViewBag.UsedMaterials = usedmat;

            var discountper = db.Users.Where(x => x.Id == UserId).Select(y => y.Discount).FirstOrDefault();
            ViewBag.Discountpercent = discountper;

            var TaxInclusive = db.EnableSettings.Where(a => a.EnableType == "TaxInclusive").FirstOrDefault();
            var TaxInclusives = TaxInclusive != null ? TaxInclusive.Status : Status.inactive;
            ViewBag.TaxInclusive = TaxInclusives;

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

            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;
            ViewBag.ReturnInSale = returninsales;
            var perce = db.PriceCategories.Where(o => o.pricestratagyid == vmodel.pricecategoryid).Select(o => o.value).FirstOrDefault();
            ViewBag.priceper = perce;
            var rtype = Request.Query["rtype"];
            var cust = (from a in db.Customers
                        join b in db.Accountss on a.Accounts equals b.AccountsID

                        where a.Type == CRMCustomerType.Customer &&
                        !a.CustomerName.StartsWith("OLD-")
                        && a.CustomerID == vmodel.Customer
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
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            else
            {
                return View(vmodel);
            }

        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        [HttpGet]
        public ActionResult Editfast(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            SalesEntry Saleentry = db.SalesEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesEntryId == id).FirstOrDefault();

            var Gtotal = db.SalesEntrys.Where(x => (x.SalesEntryId == id)).Select(x => x.SEGrandTotal).FirstOrDefault();
            ViewBag.Gtotal = Gtotal;

            SalesEntry Salentry = db.SalesEntrys.Find(id);
            ViewBag.disableupdate = User.IsInRole("Disable Update Option In Sales");
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddDays(-userEditDays);
            }


            if ((Saleentry.SEDate - editableDay).TotalMinutes < 0 || tem == 1)
            {
                return NotFound();

            }
            if (Saleentry == null)
            {
                return NotFound();
            }
            var EnableCreditLimit = db.EnableSettings.Where(a => a.EnableType == "CreditLimit").FirstOrDefault();
            var AutoCreditLimit = EnableCreditLimit != null ? EnableCreditLimit.Status : Status.inactive;
            ViewBag.AutoCreditLimit = AutoCreditLimit;

            //Fetching the image from AttachmentDocuments
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.SalesEntrys
                             on a.TransactionID equals b.SalesEntryId
                             where a.TransactionID == id && a.TransactionType == "CreditSale"
                             select new CreditSaleDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 CreditSaleId = a.TransactionID,
                                 FileName = a.FileName,
                                 CreatedDate = a.CreatedDate
                             }).ToList();
            //................................................

            Int64 cashier = Convert.ToInt64(Saleentry.SECashier);
            Int64 customer = Saleentry.Customer;

            string custname = "";
            SalesEntryViewModel vmodel = new SalesEntryViewModel();

            custname = db.Customers.Where(a => a.CustomerID == customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var use = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {//.Where(s => s.AssignedUser == UserId)
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

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

            var proj = db.Projects
              .Select(s => new
              {
                  ID = s.ProjectId,
                  Name = s.ProCode + " " + s.ProjectName
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
              .Select(s => new
              {
                  ID = s.ProTaskId,
                  Name = s.TaskName
              })
              .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesEntry").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Sale").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "Quote")
                {
                    CBill = db.Quotations.Where(a => a.QuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "DVNote")
                {
                    CBill = db.Deliverynotes.Where(a => a.DeliverynoteId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "ProForma")
                {
                    CBill = db.ProFormas.Where(a => a.ProFormaId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "SOrder")
                {
                    CBill = db.SalesOrders.Where(a => a.SalesOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }


            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.SalesEntryId, f2 = "Sales" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.SalesEntryId == id
                      select new SalesEntryViewModel
                      {
                          SENo = b.SENo,
                          PONo = b.PONo,
                          SENote = b.SENote,
                          SEDate = b.SEDate,
                          BillNo = b.BillNo,
                          SECashier = b.SECashier,
                          Customer = b.Customer,
                          SEDiscount = b.SEDiscount,
                          SEGrandTotal = b.SEGrandTotal,
                          // SEPaidAmount = c.SEPaidAmount,
                          CustomerType = b.CustomerType,
                          // SEDueAmount = b.SEGrandTotal - c.SEPaidAmount,
                          CustomerName = custname,
                          PaymentMethod = b.PaymentMethod,
                          custEmailId = e.EmailId,
                          SalesType = b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          Location = b.Location,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          Currency = b.Currency,
                          DConvertionRate = b.ConvertionRate,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          HSCode = b.HSCode,
                          PaymentTerms = b.PaymentTerms,
                          Project = b.Project,
                          ProTask = b.ProTask,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();

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

            companySet();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();


            ViewBag.preEntry = db.SalesEntrys.Where(a => a.SalesEntryId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.SalesEntrys.Where(a => a.SalesEntryId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Min();


            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var paymethod = db.PaymentMethods.FirstOrDefault();
            ViewBag.payVisible = paymethod != null ? true : false;

            var list = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                            }, "Value", "Text", 1);

            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();
            if (vmodel.PaymentMethod == null)
            {
                ViewBag.PayMethod = list;
            }
            else
            {
                ViewBag.PayMethod = QkSelect.List(pay, "ID", "Name");
            }
            _FinancialYear();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.ContType = "SalesEntry";

            ViewBag.PopUpAddCust = false;


            var EditPermission = User.IsInRole("Disable Sale Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "SalesEntry", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();

            var ref1 = db.SalesEntrys
               .Select(s => new
               {
                   ID = s.Ref1,
                   Name = s.Ref1
               }).Distinct()
               .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", Salentry.Ref1);

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", Salentry.Ref2);

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", Salentry.Ref3);

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", Salentry.Ref4);

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", Salentry.Ref5);
            ViewBag.Ref44 = db.SalesEntrys.Find(id).Ref4.ToString();
            ViewBag.Ref55 = db.SalesEntrys.Find(id).Ref5.ToString();

            var cusomernames = db.SalesEntrys
            .Select(s => new
            {
                ID = s.customername,
                Name = s.customername
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.customernamenotax = QkSelect.List(cusomernames, "ID", "Name", Salentry.customername);

            long rtask = 11;
            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();
            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            //dummy table operations
            var DItem = db.DummySEItems.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SItem = db.SEItemss.Where(a => a.SalesEntry == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummySEItems.Where(a => a.SalesEntry == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    SEItems sItem = new SEItems();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.itemNote = arr.itemNote;
                    sItem.SalesEntry = arr.SalesEntry;
                    sItem.Item = arr.Item;
                    db.SEItemss.Add(sItem);
                    db.SaveChanges();
                }

                db.DummySEItems.RemoveRange(db.DummySEItems.Where(a => a.SalesEntry == id));
                db.SaveChanges();
                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, id); // forward-correctness: header = SUM(lines)
            }

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var usedmaterial = db.EnableSettings.Where(a => a.EnableType == "Usedmaterials").FirstOrDefault();
            var usedmat = usedmaterial != null ? usedmaterial.Status : Status.inactive;
            ViewBag.UsedMaterials = usedmat;

            var discountper = db.Users.Where(x => x.Id == UserId).Select(y => y.Discount).FirstOrDefault();
            ViewBag.Discountpercent = discountper;

            var TaxInclusive = db.EnableSettings.Where(a => a.EnableType == "TaxInclusive").FirstOrDefault();
            var TaxInclusives = TaxInclusive != null ? TaxInclusive.Status : Status.inactive;
            ViewBag.TaxInclusive = TaxInclusives;

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

            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;
            ViewBag.ReturnInSale = returninsales;

            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            else
            {
                return View(vmodel);
            }

        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Entry List")]
        public ActionResult UpdateSaleNew(string[][] array, string[] saledata, SEBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel, ICollection<UBatchStockPViewModel> ubstmodel, string[][] arrayused, ICollection<commissionViewmodel> commission, string TenderingMode, string Mode, ICollection<SettlementViewModel> SettlementData, decimal? BalanceAmount, string[][] arrayR, ICollection<RackStockPViewModel> bsrackData)
        {

            bool stat = false;
            string msg;
            long SalesEntryID = 0;
            Int64 salesEntryId = Convert.ToInt64(saledata[15]);
            SalesEntry SEentry = db.SalesEntrys.Find(salesEntryId);
            SEentry.SalesStatus = Convert.ToInt32(saledata[59]);



            //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };



            string action = saledata[18];
            var seitemss = db.SEItemss.Where(o => o.SalesEntry == salesEntryId).ToList();
            var i = 0;
            var SEItem = db.SEItemss.Where(a => a.SalesEntry == salesEntryId).ToArray();
            var salese = db.SalesEntrys.Find(salesEntryId);
            salese.materialcost = 100000;
            db.Entry(salese).State = EntityState.Modified;
            db.SaveChanges();
            foreach (var arr in array)
            {
                var convitem = Convert.ToInt32(arr[0]);
                if (SEItem[i].Item != Convert.ToInt32(arr[0]))
                {
                    var itemid = Convert.ToInt32(arr[0]);
                    var it = db.Items.Where(o => o.ItemID == itemid).FirstOrDefault();
                    SEItems sitms = db.SEItemss.Find(SEItem[i].SEItemsId);
                    if (it.ConFactor > 1)
                    {
                        sitms.ItemUnit = Convert.ToInt64(arr[1]);

                    }
                    sitms.Item = Convert.ToInt32(arr[0]);
                    db.Entry(sitms).State = EntityState.Modified;
                    db.SaveChanges();
                }
                i++;

            }



            msg = "Successfully Updated Sales Entry.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, SalesEntryID = salesEntryId } };
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Item Sales Entry")]
        [HttpGet]
        public ActionResult EditNew(long? id)
        {
            var enablepricetratagy = db.EnableSettings.Where(a => a.EnableType == "enablepricestratagy").FirstOrDefault();
            var pricetratagy = enablepricetratagy != null ? enablepricetratagy.Status : Status.inactive;
            ViewBag.pricestratagy = pricetratagy;
            var pri = db.PriceCategories.Where(o => o.active == false)
                      .Select(s => new
                      {
                          ID = s.value,
                          Name = s.description
                      }).ToList();

            pri.Insert(0, db.PriceCategories.Where(o => o.active == true)
                             .Select(s => new
                             {
                                 ID = s.value,
                                 Name = s.description
                             }).FirstOrDefault());
            ViewBag.pricecategorylist = QkSelect.List(pri, "ID", "Name");
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var EnableRackWise = db.EnableSettings.Where(a => a.EnableType == "RackWiseStock").FirstOrDefault();
            var RackCheck = EnableRackWise != null ? EnableRackWise.Status : Status.inactive;
            ViewBag.RackWiseStock = RackCheck;
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddDays(-userEditDays);
            }


            SalesEntry Saleentry = db.SalesEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesEntryId == id).FirstOrDefault();
            if ((Saleentry.SEDate - editableDay).TotalDays < 0 || tem == 1)
            {

            }
            var Gtotal = db.SalesEntrys.Where(x => (x.SalesEntryId == id)).Select(x => x.SEGrandTotal).FirstOrDefault();
            ViewBag.Gtotal = Gtotal;

            SalesEntry Salentry = db.SalesEntrys.Find(id);
            ViewBag.disableupdate = User.IsInRole("Disable Update Option In Sales");
            if (Saleentry == null)
            {
                return NotFound();
            }
            var EnableCreditLimit = db.EnableSettings.Where(a => a.EnableType == "CreditLimit").FirstOrDefault();
            var AutoCreditLimit = EnableCreditLimit != null ? EnableCreditLimit.Status : Status.inactive;
            ViewBag.AutoCreditLimit = AutoCreditLimit;
            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;


            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;
            if (Saleentry.SalesStatus == 0)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
                ViewBag.sstatus = pstat;

            }
            else if (Saleentry.SalesStatus == 1)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                }
              };
                ViewBag.sstatus = pstat;
            }
            else
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.sstatus = pstat;
            }

            //Fetching the image from AttachmentDocuments
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.SalesEntrys
                             on a.TransactionID equals b.SalesEntryId
                             where a.TransactionID == id && a.TransactionType == "CreditSale"
                             select new CreditSaleDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 CreditSaleId = a.TransactionID,
                                 FileName = a.FileName,
                                 CreatedDate = a.CreatedDate
                             }).ToList();
            //................................................

            Int64 cashier = Convert.ToInt64(Saleentry.SECashier);
            Int64 customer = Saleentry.Customer;

            string custname = "";
            SalesEntryViewModel vmodel = new SalesEntryViewModel();

            custname = db.Customers.Where(a => a.CustomerID == customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer && s.CustomerID == 0).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var use = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;
            List<SelectFormat> mcss;
            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName
                mcss = db.MCs.Where(s => s.MCId == Saleentry.MaterialCenter).Select(s => new SelectFormat
                {
                    text = s.MCName,
                    id = s.MCId
                }).OrderBy(b => b.text).ToList().ToList();


                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();
                serialisedJson = mcss.Union(serialisedJson).ToList();
                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                ViewBag.MC = QkSelect.List(serialisedJson, "id", "text");
                ViewBag.LastMc = serialisedJson.Select(a => a.id).FirstOrDefault();
            }

            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

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

            var proj = db.Projects
              .Select(s => new
              {
                  ID = s.ProjectId,
                  Name = s.ProCode + " " + s.ProjectName
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            })
            .ToList();
            var saleentytask = Saleentry.ProTask;
            var ss = db.ProTasks.Find(Saleentry.ProTask);
            if (ss != null)
            {
                var saleentrytaskname = ss.TaskCode + "-" + ss.TaskName;
                var tsk2 = (from a in db.additionaltasks
                            join b in db.ProTasks on a.taskid equals b.ProTaskId
                            where a.salesentryid == id

                            select new
                            {
                                ID = a.taskid,
                                Name = b.TaskCode + "-" + b.TaskName
                            })
               .ToList();
                tsk2.Add(new { ID = (long)saleentytask, Name = saleentrytaskname });
                vmodel.ProTasks = tsk2.Select(o => o.ID).ToArray();
                ViewBag.getProTask = new MultiSelectList(tsk2, "ID", "Name", tsk2.Select(o => o.ID).ToArray());

            }
            else
            {
                var tskd = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskCode + "-" + s.TaskName
             })
             .ToList();
                ViewBag.getProTask = QkSelect.List(tskd, "ID", "Name");
            }

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesEntry").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? MlaSEntry.Status : Status.inactive;
            ViewBag.MLASEntry = MlaSEntrys;

            var CBill = "";
            var CType = "";
            DateTime createdDate = new DateTime();
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Sale").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "Quote")
                {
                    CBill = db.Quotations.Where(a => a.QuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "DVNote")
                {
                    CBill = db.Deliverynotes.Where(a => a.DeliverynoteId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                    createdDate = db.Deliverynotes.Where(c => c.BillNo == CBill).Select(c => c.DvDate).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "ProForma")
                {
                    CBill = db.ProFormas.Where(a => a.ProFormaId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "SOrder")
                {
                    CBill = db.SalesOrders.Where(a => a.SalesOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }
            var users = db.LogManagers.Where(o => o.LogType == LogTypes.Created && o.LogID == id.ToString() && o.LogTable == "SalesEntrys").Select(o => o.User).FirstOrDefault();
            var impids = db.Employees.Where(o => o.UserId == users).Select(o => o.EmployeeId).FirstOrDefault();
            var data2 = (from a in db.accountmaps
                         join b in db.Accountss
                         on a.AccountId equals b.AccountsID
                         where (a.EmployeeId == impids && a.PaymentTypeId != EmployeePaymentType.Account)
                         select new
                         {
                             EmployeeId = a.EmployeeId,
                             AccountId = a.AccountId,
                             AccountNames = b.Name,
                             PaymentType = a.PaymentTypeId,
                             Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                             //ChequeNo,
                             //ChequeDate
                         }).ToList();
            var impids2 = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
            var data22 = (from a in db.accountmaps
                          join b in db.Accountss
                          on a.AccountId equals b.AccountsID
                          where (a.EmployeeId == impids2 && a.PaymentTypeId != EmployeePaymentType.Account)
                          select new
                          {
                              EmployeeId = a.EmployeeId,
                              AccountId = a.AccountId,
                              AccountNames = b.Name,
                              PaymentType = a.PaymentTypeId,
                              Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == id && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),

                              //ChequeNo,
                              //ChequeDate
                          }).ToList();
            ViewBag.anothershowroom = 0;
            if (users != UserId && data2.Count() > 0)
            {
                ViewBag.anothershowroom = 1;
            }

            if (data2.Count() > 0)
            {
                ViewBag.settmode = "active";
                ViewBag.settmodeempid = data2.FirstOrDefault().EmployeeId;
            }
            ViewBag.superuser = false;
            ViewBag.superuser = User.IsInRole("Delete Sales Entry");

            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry into sepay
                      from c in sepay.DefaultIfEmpty()
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.SalesEntryId, f2 = "Sales" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.SalesEntryId == id
                      select new SalesEntryViewModel
                      {
                          SENo = b.SENo,
                          PONo = b.PONo,
                          SENote = b.SENote,
                          SEDate = b.SEDate,
                          BillNo = b.BillNo,
                          pricecategoryid = b.pricecategoryid,
                          SECashier = b.SECashier,
                          Customer = b.Customer,
                          SEDiscount = b.SEDiscount,
                          SEGrandTotal = b.SEGrandTotal,
                          // SEPaidAmount = c.SEPaidAmount,
                          CustomerType = b.CustomerType,
                          // SEDueAmount = b.SEGrandTotal - c.SEPaidAmount,
                          CustomerName = custname,
                          PaymentMethod = b.PaymentMethod,
                          custEmailId = e.EmailId,
                          SalesType = b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          Location = b.Location,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          Currency = b.Currency,
                          DConvertionRate = b.ConvertionRate,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          HSCode = b.HSCode,
                          PaymentTerms = b.PaymentTerms,
                          Project = b.Project,
                          ProTask = b.ProTask,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          DVNotDate = createdDate,
                      }).FirstOrDefault();
            if (ConvertTran != null)
            {
                if (ConvertTran.ConvertFrom == "DVNote")
                {
                    DateTime createdDate2 = db.Deliverynotes.Where(c => c.BillNo == CBill).Select(c => c.DvDate).FirstOrDefault();
                    string dateString = String.Format("{0:dd/MM/yyyy}", createdDate2);

                    ViewBag.dvdate = dateString;
                }
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

            companySet();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();


            ViewBag.preEntry = db.SalesEntrys.Where(a => a.SalesEntryId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.SalesEntrys.Where(a => a.SalesEntryId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesEntryId).DefaultIfEmpty().Min();


            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var paymethod = db.PaymentMethods.FirstOrDefault();
            ViewBag.payVisible = paymethod != null ? true : false;

            var list = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                            }, "Value", "Text", 1);

            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();
            if (vmodel != null)
            {
                if (vmodel.PaymentMethod == null)
                {
                    ViewBag.PayMethod = list;
                }
                else
                {
                    ViewBag.PayMethod = QkSelect.List(pay, "ID", "Name");
                }
            }
            _FinancialYear();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.ContType = "SalesEntry";

            ViewBag.PopUpAddCust = false;



            var EditPermission = User.IsInRole("Disable Sale Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "SalesEntry", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            //field mapping
            if (vmodel != null)
                vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();

            var ref1 = db.SalesEntrys
               .Select(s => new
               {
                   ID = s.Ref1,
                   Name = s.Ref1
               }).Distinct()
               .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", Salentry.Ref1);

            var ref2 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", Salentry.Ref2);

            var ref3 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", Salentry.Ref3);

            var ref4 = db.SalesEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", Salentry.Ref4);

            var ref5 = db.SalesEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", Salentry.Ref5);
            long rtask = 11;
            ViewBag.TypeList = db.ContactTypes.ToList();
            var CountryCode1 = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName + " (" + s.CountryCode + ")",
            }).ToList();
            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            //dummy table operations
            var DItem = db.DummySEItems.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SItem = db.SEItemss.Where(a => a.SalesEntry == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummySEItems.Where(a => a.SalesEntry == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    SEItems sItem = new SEItems();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.itemNote = arr.itemNote;
                    sItem.SalesEntry = arr.SalesEntry;
                    sItem.Item = arr.Item;
                    db.SEItemss.Add(sItem);
                    db.SaveChanges();
                }

                db.DummySEItems.RemoveRange(db.DummySEItems.Where(a => a.SalesEntry == id));
                db.SaveChanges();
                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, id); // forward-correctness: header = SUM(lines)
            }

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var usedmaterial = db.EnableSettings.Where(a => a.EnableType == "Usedmaterials").FirstOrDefault();
            var usedmat = usedmaterial != null ? usedmaterial.Status : Status.inactive;
            ViewBag.UsedMaterials = usedmat;

            var discountper = db.Users.Where(x => x.Id == UserId).Select(y => y.Discount).FirstOrDefault();
            ViewBag.Discountpercent = discountper;

            var TaxInclusive = db.EnableSettings.Where(a => a.EnableType == "TaxInclusive").FirstOrDefault();
            var TaxInclusives = TaxInclusive != null ? TaxInclusive.Status : Status.inactive;
            ViewBag.TaxInclusive = TaxInclusives;

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

            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;
            ViewBag.ReturnInSale = returninsales;
            var perce = db.PriceCategories.Where(o => o.pricestratagyid == vmodel.pricecategoryid).Select(o => o.value).FirstOrDefault();
            ViewBag.priceper = perce;
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            else
            {
                return View(vmodel);
            }

        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult qrcodeprint(long? id)
        {
            return RedirectToAction("qrcodeprint/" + id, "Users");

            //           // RoleManager = LegacyIdentity.RoleManager(db);




            //        // This doesn't count login failures towards account lockout
            //        // To enable password failures to trigger account lockout, change to lockoutOnFailure: true
            //        var result = (await SignInManager.PasswordSignInAsync(username, password, true, lockoutOnFailure: false)).ToSignInStatus();


            //                                .AuthenticationManager

            //                //if (name != null)
            //                //else




            //            default:








        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult confirmotp(long salesEntryId, string otp, string purpose, string fdate = "")
        {
            DateTime nw = System.DateTime.Now;
            var user = User.Identity.GetUserId();
            if (purpose == "Sales")
            {
                var con = (

                           from c in db.otpapproves
                           join d in db.SalesEntrys on c.entryid equals d.SalesEntryId
                           join e in db.UserEditDayss on c.approvedby equals e.userid
                           where d.SalesEntryId == salesEntryId && c.otp == otp && c.purpose == "Sales"
                           && c.expdate >= nw
                           select new
                           {
                               e.days,
                               d.SECreatedDate,
                               c.optid
                           }
                           ).FirstOrDefault();
                if (con == null)
                {


                    bool st = false;
                    return Json(new { status = st, message = "Otp Expired" });
                }

                else
                {
                    var editableDay = DateTime.Now;

                    if (con.days == 0)
                    {
                        editableDay = nw.AddYears(-10);

                    }
                    else
                    {
                        editableDay = nw.AddMinutes(-con.days);
                    }
                    if ((con.SECreatedDate - editableDay).TotalMinutes < 0)
                    {
                        bool st = false;
                        return Json(new { status = st, message = "Super User Edit Days Expired" });

                    }
                    else
                    {
                        var ap = db.otpapproves.Where(o => o.optid == con.optid && o.otp == otp && o.entryid == salesEntryId).FirstOrDefault();
                        ap.approvedby = user;
                        db.Entry(ap).State = EntityState.Modified;
                        db.SaveChanges();
                        bool st = true;
                        return Json(new { status = st, otp = 0 });
                    }
                }

            }


            if (purpose == "Salesitem")
            {
                var con = (

                           from c in db.otpapproves
                           join d in db.SalesEntrys on c.entryid equals d.SalesEntryId
                           join e in db.UserEditDayss on c.approvedby equals e.userid
                           where d.SalesEntryId == salesEntryId && c.otp == otp && c.purpose == "Sales"
                           && c.expdate >= nw
                           select new
                           {
                               e.seitem,
                               d.SECreatedDate,
                               c.optid
                           }
                           ).FirstOrDefault();
                if (con == null)
                {


                    bool st = false;
                    return Json(new { status = st, message = "Otp Expired" });
                }

                else
                {
                    var editableDay = DateTime.Now;

                    if (con.seitem == 0)
                    {
                        editableDay = nw.AddYears(-10);

                    }
                    else
                    {
                        editableDay = nw.AddMinutes(-con.seitem);
                    }
                    if ((con.SECreatedDate - editableDay).TotalMinutes < 0)
                    {
                        bool st = false;
                        return Json(new { status = st, message = "Super User Edit Days Expired" });

                    }
                    else
                    {
                        var ap = db.otpapproves.Where(o => o.optid == con.optid && o.otp == otp && o.entryid == salesEntryId).FirstOrDefault();
                        ap.approvedby = user;
                        db.Entry(ap).State = EntityState.Modified;
                        db.SaveChanges();
                        bool st = true;
                        return Json(new { status = st, otp = 0 });
                    }
                }

            }
            else if (purpose == "Purchase" && salesEntryId != 0)
            {
                var con = (

                           from c in db.otpapproves
                           join d in db.PurchaseEntrys on c.entryid equals d.PurchaseEntryId
                           join e in db.UserEditDayss on c.approvedby equals e.userid
                           where d.PurchaseEntryId == salesEntryId && c.otp == otp && c.purpose == purpose
                           && c.expdate >= nw
                           select new
                           {
                               e.pedays,
                               d.PECreatedDate,
                               c.optid
                           }
                           ).FirstOrDefault();
                if (con == null)
                {


                    bool st = false;
                    return Json(new { status = st, message = "Otp Expired" });
                }
                else
                {
                    var editableDay = DateTime.Now;

                    if (con.pedays == 0)
                    {
                        editableDay = nw.AddYears(-10);

                    }
                    else
                    {
                        editableDay = nw.AddMinutes(-Convert.ToDouble(con.pedays));
                    }
                    if ((con.PECreatedDate - editableDay).TotalMinutes < 0)
                    {
                        bool st = false;
                        return Json(new { status = st, message = "Super User Edit Days Expired" });

                    }
                    else
                    {
                        var ap = db.otpapproves.Where(o => o.optid == con.optid && o.otp == otp && o.entryid == salesEntryId && o.purpose == purpose).FirstOrDefault();
                        ap.approvedby = user;
                        db.Entry(ap).State = EntityState.Modified;
                        db.SaveChanges();
                        bool st = true;
                        return Json(new { status = st, otp = 0 });
                    }
                }

            }
            else if (purpose == "Purchase" && salesEntryId == 0)
            {
                DateTime? date = null;

                if (fdate != "")
                {
                    date = DateTime.Parse(fdate, new CultureInfo("en-GB"));
                }
                var con = (

                           from c in db.otpapproves

                           join e in db.UserEditDayss on c.approvedby equals e.userid
                           where c.entryid == 0 && c.otp == otp && c.purpose == purpose
                           && c.expdate >= nw
                           select new
                           {
                               e.pedays,
                               PECreatedDate = date,
                               c.optid
                           }
                           ).FirstOrDefault();
                if (con == null)
                {


                    bool st = false;
                    return Json(new { status = st, message = "Otp Expired" });
                }
                else
                {
                    var editableDay = DateTime.Now;

                    if (con.pedays == 0)
                    {
                        editableDay = nw.AddYears(-10);

                    }
                    else
                    {
                        editableDay = nw.AddMinutes(-Convert.ToDouble(con.pedays));
                    }
                    if (((DateTime)con.PECreatedDate - editableDay).TotalMinutes < 0)
                    {
                        bool st = false;
                        return Json(new { status = st, message = "Super User Edit Days Expired" });

                    }
                    else
                    {
                        var ap = db.otpapproves.Where(o => o.optid == con.optid && o.otp == otp && o.entryid == salesEntryId && o.purpose == purpose).FirstOrDefault();
                        ap.approvedby = user;
                        db.Entry(ap).State = EntityState.Modified;
                        db.SaveChanges();
                        bool st = true;
                        return Json(new { status = st, otp = 0 });
                    }
                }

            }
            else if (purpose == "SalesReturn")
            {
                var con = (

                           from c in db.otpapproves
                           join d in db.SalesReturns on c.entryid equals d.SalesReturnId
                           join e in db.UserEditDayss on c.approvedby equals e.userid
                           where d.SalesReturnId == salesEntryId && c.otp == otp && c.purpose == purpose
                           && c.expdate >= nw
                           select new
                           {
                               e.srdays,
                               d.SRCreatedDate,
                               c.optid
                           }
                           ).FirstOrDefault();
                if (con == null)
                {


                    bool st = false;
                    return Json(new { status = st, message = "Otp Expired" });
                }
                else
                {
                    var editableDay = DateTime.Now;

                    if (con.srdays == 0)
                    {
                        editableDay = nw.AddYears(-10);

                    }
                    else
                    {
                        editableDay = nw.AddDays(-Convert.ToDouble(con.srdays));
                    }
                    if ((con.SRCreatedDate - editableDay).TotalMinutes < 0)
                    {
                        bool st = false;
                        return Json(new { status = st, message = "Super User Edit Days Expired" });

                    }
                    else
                    {
                        var ap = db.otpapproves.Where(o => o.optid == con.optid && o.otp == otp && o.entryid == salesEntryId && o.purpose == purpose).FirstOrDefault();
                        ap.approvedby = user;
                        db.Entry(ap).State = EntityState.Modified;
                        db.SaveChanges();
                        bool st = true;
                        return Json(new { status = st, otp = 0 });
                    }
                }

            }
            else if (purpose == "PurchaseReurn")
            {
                var con = (

                           from c in db.otpapproves
                           join d in db.PurchaseReturns on c.entryid equals d.PurchaseReturnId
                           join e in db.UserEditDayss on c.approvedby equals e.userid
                           where d.PurchaseReturnId == salesEntryId && c.otp == otp && c.purpose == purpose
                           && c.expdate >= nw
                           select new
                           {
                               e.prdays,
                               d.PRCreatedDate,
                               c.optid
                           }
                           ).FirstOrDefault();
                if (con == null)
                {


                    bool st = false;
                    return Json(new { status = st, message = "Otp Expired" });
                }
                else
                {
                    var editableDay = DateTime.Now;

                    if (con.prdays == 0)
                    {
                        editableDay = nw.AddYears(-10);

                    }
                    else
                    {
                        editableDay = nw.AddMinutes(-Convert.ToDouble(con.prdays));
                    }
                    if ((con.PRCreatedDate - editableDay).TotalMinutes < 0)
                    {
                        bool st = false;
                        return Json(new { status = st, message = "Super User Edit Days Expired" });

                    }
                    else
                    {
                        var ap = db.otpapproves.Where(o => o.optid == con.optid && o.otp == otp && o.entryid == salesEntryId && o.purpose == purpose).FirstOrDefault();
                        ap.approvedby = user;
                        db.Entry(ap).State = EntityState.Modified;
                        db.SaveChanges();
                        bool st = true;
                        return Json(new { status = st, otp = 0 });
                    }
                }

            }
            else if (purpose == "override credit limit and credit period")
            {
                var con = (

                           from c in db.otpapproves
                           join d in db.SalesEntrys on c.entryid equals d.SalesEntryId
                           join e in db.UserEditDayss on c.approvedby equals e.userid
                           where d.SalesEntryId == salesEntryId && c.otp == otp && c.purpose == "override credit limit and credit period"
                           && c.expdate >= nw
                           select new
                           {
                               e.days,
                               d.SECreatedDate,
                               c.optid
                           }
                           ).FirstOrDefault();
                if (con == null)
                {


                    bool st = false;
                    return Json(new { status = st, message = "Otp Expired" });
                }
                else
                {
                    var editableDay = DateTime.Now;

                    if (con.days == 0)
                    {
                        editableDay = nw.AddYears(-10);

                    }
                    else
                    {
                        editableDay = nw.AddMinutes(-con.days);
                    }
                    if ((con.SECreatedDate - editableDay).TotalMinutes < 0)
                    {
                        bool st = false;
                        return Json(new { status = st, message = "Super User Edit Days Expired" });

                    }
                    else
                    {
                        var ap = db.otpapproves.Where(o => o.optid == con.optid && o.otp == otp && o.entryid == salesEntryId).FirstOrDefault();
                        ap.approvedby = user;
                        db.Entry(ap).State = EntityState.Modified;
                        db.SaveChanges();
                        bool st = true;
                        return Json(new { status = st, otp = 0 });
                    }
                }

            }
            else
            {
                bool st = false;
                return Json(new { status = st, otp = 0 });

            }
        }
        [AllowAnonymous]
        [HttpGet]
        public string reqotp()
        {

            DateTime nw = System.DateTime.Now;
            var user = User.Identity.GetUserId();
            var empid = db.Employees.Where(o => o.UserId == user).Select(o => o.EmployeeId).FirstOrDefault();
            if (1 == 1)
            {
                if (1 == 1)
                {
                    var myotp = (from c in db.otpapproves
                                 join d in db.Employees on c.approvedby equals d.UserId
                                 where c.approvedby == user &&
                                 c.expdate >= nw
                                 select new
                                 {

                                     c.otp,
                                     d.FirstName,
                                     c.purpose,
                                     c.entryid
                                 }
                       ).Distinct().ToList();

                    if (myotp != null)
                    {

                        string s = "";
                        foreach (var r in myotp)
                        {
                            string billno = "";
                            if (r.purpose == "Sales")
                            {
                                billno = db.SalesEntrys.Where(o => o.SalesEntryId == r.entryid).Select(o => o.BillNo).FirstOrDefault();
                            }
                            else if (r.purpose == "Purchase")
                            {
                                billno = db.PurchaseEntrys.Where(o => o.PurchaseEntryId == r.entryid).Select(o => o.BillNo).FirstOrDefault();


                            }
                            else if (r.purpose == "SalesReturn")
                            {
                                billno = db.SalesReturns.Where(o => o.SalesReturnId == r.entryid).Select(o => o.BillNo).FirstOrDefault();


                            }
                            if (r.purpose == "override credit limit and credit period")
                            {
                                billno = db.SalesEntrys.Where(o => o.SalesEntryId == r.entryid).Select(o => o.BillNo).FirstOrDefault();
                            }
                            if (r.purpose == "PurchaseReurn")
                            {
                                billno = db.PurchaseReturns.Where(o => o.PurchaseReturnId == r.entryid).Select(o => o.BillNo).FirstOrDefault();
                            }
                            s = s + "Otp for " + r.purpose + "  bill no : " + billno + " Request by :" + r.FirstName + " Otp is : " + r.otp + "\n";
                        }
                        return s;
                    }
                    else
                    {
                        return " no otp";
                    }
                }
                else
                {
                    return " no otp";
                }
            }
            return " no otp";
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult getotp(long salesEntryId, string purpose, string fdate = "", long? mcpass = 0)
        {
            DateTime? date = null;

            if (fdate != "")
            {
                date = DateTime.Parse(fdate, new CultureInfo("en-GB"));
            }
            DateTime nw = System.DateTime.Now;
            var user = User.Identity.GetUserId();
            var otp = db.otpapproves.Where(o => o.entryid == salesEntryId && o.requestedby == user && o.approvedby != user && o.purpose == purpose && o.expdate >= nw).Select(o => o.otp).FirstOrDefault();

            Random r = new Random();
            var x = r.Next(0, 1000000);
            string s = x.ToString("000000");
            string otps = "";

            if (otp == "" || otp == null)
            {

                //                where c.SalesEntryId == salesEntryId
                //                && ap.requestedby == user && ap.purpose == "Sales" && ap.expdate >= nw
                //                    aa.days,
                //                    c.SECreatedDate



                long? mc = null;

                if (purpose == "Sales" || purpose == "Salesitem")
                {
                    mc = db.SalesEntrys.Where(o => o.SalesEntryId == salesEntryId).Select(o => o.MaterialCenter).FirstOrDefault();
                    purpose = "Sales";
                }
                else if (purpose == "Purchase")
                    mc = db.PurchaseEntrys.Where(o => o.PurchaseEntryId == salesEntryId).Select(o => o.MaterialCenter).FirstOrDefault();
                else if (purpose == "SalesReturn")
                    mc = db.SalesReturns.Where(o => o.SalesReturnId == salesEntryId).Select(o => o.MaterialCenter).FirstOrDefault();
                else if (purpose == "override credit limit and credit period")
                {
                    mc = db.SalesEntrys.Where(o => o.SalesEntryId == salesEntryId).Select(o => o.MaterialCenter).FirstOrDefault();

                }
                else if (purpose == "PurchaseReurn")
                {
                    mc = db.PurchaseReturns.Where(o => o.PurchaseReturnId == salesEntryId).Select(o => o.MaterialCenter).FirstOrDefault();

                }
                if (mcpass != 0 && purpose == "Purchase" && fdate != "")
                {
                    mc = mcpass;
                }
                var mcsuper = (from a in db.SuperUsers
                               join b in db.Employees on a.employeeid equals b.EmployeeId
                               join c in db.Users on b.UserId equals c.Id
                               where a.mcid == mc && a.purpose == purpose
                               select new
                               {
                                   c.Id
                               }).ToList().ToArray();
                if (mcsuper.Count() > 0)
                {
                    db.otpapproves.RemoveRange(db.otpapproves.Where(o => o.entryid == salesEntryId && o.purpose == purpose));
                    db.SaveChanges();
                    Random rr = new Random();
                    foreach (var c in mcsuper)
                    {

                        var xx = rr.Next(0, 1000000);
                        string ss = xx.ToString("000000");

                        otpapprove a = new otpapprove
                        {
                            entryid = salesEntryId,
                            purpose = purpose,
                            expdate = nw.AddMinutes(20),
                            requestedby = user,
                            approvedby = c.Id,

                            otp = ss,
                        };

                        otps = "new";
                        db.otpapproves.Add(a);
                        db.SaveChanges();
                    }
                    bool st = true;
                    return Json(new { status = st, otp = mcsuper.Count() });

                }
                else
                {
                    bool st = false;
                    return Json(new { status = st, otp = 0 });

                }



            }
            else
            {
                bool st = true;
                return Json(new { status = st, otp = 0 });

            }
            ////       var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
            ////     var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();




            //             where a.SalesEntryId == salesEntryId
            //                 b.emailid,
            //                 a.BillNo,





            bool stat = true;
            return Json(new { status = stat, otp = otps });

        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult downloadprintpos(long salesEntryId)
        {
            POSViewModel vmodel = new POSViewModel();
             vmodel.posData = db.PPosDatas.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();

            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            string sedate = db.SalesEntrys.Where(o=>o.SalesEntryId==salesEntryId).Select(o=>o.SEDate).FirstOrDefault().ToString("dd-MM-yyyy");
            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
            var HideItmName = db.EnableSettings.Where(a => a.EnableType == "HideItemName").FirstOrDefault();
            var HideItmNameIfDiscriptionOn = HideItmName != null ? HideItmName.Status : Status.inactive;
            string ConvertFrom = "";
            string ConvertBill = "";
            bool stat = false;
            string msg;
            var conv = db.ConvertTransactionss.Any(u => u.To == salesEntryId);

            var sales = (from a in db.SalesEntrys
                         join b in db.Customers on a.Customer equals b.CustomerID into cust
                         from b in cust.DefaultIfEmpty()
                         join c in db.Contacts on b.Contact equals c.ContactID into cnt
                         from c in cnt.DefaultIfEmpty()
                         join f in db.WalkinCustomers on a.SalesEntryId equals f.SalesEntryId into walk
                         from f in walk.DefaultIfEmpty()
                         join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                         from d in pay.DefaultIfEmpty()
                         join e in db.Employees on a.SECashier equals e.EmployeeId into user
                         from e in user.DefaultIfEmpty()
                         where a.SalesEntryId == salesEntryId
                         select new
                         {
                             CustomerName = a.CustomerType == CustomerType.Customer ? b.CustomerName : f.CustomerName,
                             SENo = a.SENo,
                             PONo = a.PONo,
                             BillNo = a.BillNo,
                             Date = a.SEDate,
                             Note = a.SENote,
                             CustomerType = a.CustomerType,
                             Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                             SEDiscount = a.SEDiscount,
                             SETotal = a.SEDiscount + a.SEGrandTotal,
                             SEGrandTotal = a.SEGrandTotal,
                             SEPaidAmount = a.SEGrandTotal,
                             SEDueAmount = 0,
                             SETaxAmount = a.SETaxAmount,
                             Address = a.CustomerType == CustomerType.Customer ? (c.Address + " " + c.City + " " + c.State + " " + c.Country + " " + c.Zip) : "",
                             Email = a.CustomerType == CustomerType.Customer ? c.EmailId : "",
                             Phone = a.CustomerType == CustomerType.Customer ? c.Phone : "",
                             Mobile = a.CustomerType == CustomerType.Customer ? c.Mobile : f.MobileNo,
                             TRN = a.CustomerType == CustomerType.Customer ? b.TaxRegNo : "",
                             SubTotal = a.SESubTotal,
                             TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),
                             a.SETax

                         }).FirstOrDefault();
            var item = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()

                        join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                        from f in unit.DefaultIfEmpty()

                        join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                        from g in bundle.DefaultIfEmpty()
                        where a.SalesEntry == salesEntryId
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
                            a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            b.ItemArabic,
                            a.itemNote,
                            ItemNote = a.itemNote,
                            g.BundleType,
                            b.ItemType,
                            f.ItemUnitName,
                            bundle = (from ab in db.SEItemss
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                      from cb in primary.DefaultIfEmpty()
                                      join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                      from bd in second.DefaultIfEmpty()

                                      join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                      from un in unit.DefaultIfEmpty()

                                      where ab.SalesEntry == salesEntryId
                                      && b.ItemID == ab.ItemDiscount
                                      select new
                                      {
                                          bb.ItemCode,
                                          bb.ItemName,
                                          // cb.ItemUnitName,
                                          un.ItemUnitName,
                                          ItemUnitPrice = ab.ItemUnitPrice,
                                          quantity = ab.ItemQuantity,
                                          ItemSubTotal = ab.ItemSubTotal,
                                          ItemTax = ab.ItemTax,
                                          ItemTaxAmount = ab.ItemTaxAmount,
                                          ItemTotalAmount = ab.ItemTotalAmount,

                                          ab.Item,
                                          ab.ItemQuantity,
                                          ab.ItemUnit,

                                          ItemDiscount = 0,

                                          ItemNote = ab.itemNote,

                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          PriUnit = cb.ItemUnitName,
                                          SubUnit = bd.ItemUnitName,
                                          bb.ItemArabic,
                                      }).ToList()

                        }).ToList();
            stat = true;
              return Json ( new { billno = sales.BillNo, status = stat, item, sales, PosDate = vmodel.posData });



        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult downloadprint(long salesEntryId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();

            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
            var HideItmName = db.EnableSettings.Where(a => a.EnableType == "HideItemName").FirstOrDefault();
            var HideItmNameIfDiscriptionOn = HideItmName != null ? HideItmName.Status : Status.inactive;
            string ConvertFrom = "";
            string ConvertBill = "";
            var conv = db.ConvertTransactionss.Any(u => u.To == salesEntryId);
            List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
            if (conv)
            {
                List<string> ExList = new List<string>();
                List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                ExtList = ExtNumDetails((long)salesEntryId, ExtList);
                var Extended = ExtList.Select(z => z.To).ToList();
                Int32 count = 0;


                var ConvModel = (from a in db.ConvertTransactionss
                                 join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                                 from b in primary.DefaultIfEmpty()
                                 where Extended.Contains(a.To)
                                 select new ConvertTransactionsViewModel
                                 {
                                     ConvertFrom = (a.ConvertFrom == "SaleExtend") ? "Sale" : (a.ConvertFrom == "Quote") ? "Quotation" : (a.ConvertFrom == "DVNote") ? "Delivery Note" : a.ConvertFrom,
                                     Id = b.SalesEntryId,
                                     BillNo = b.BillNo,
                                     CreatedDate = a.CreatedDate,
                                     From = a.From
                                 }).OrderBy(b => b.CreatedDate).ToList();

                var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                parentvm.BillNo = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                parentvm.Id = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.SalesEntryId).FirstOrDefault();
                parentvm.ConvertFrom = "SaleExtend";
                ConvModel.Add(parentvm);
                ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                var str = ConvExt.Find(c => c.Id == salesEntryId);
                ConvExt.Remove(str);
            }

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


            var fmapp = db.FieldMappings.Where(a => a.Section == "Sales" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


            var saleData = com.SaleData(salesEntryId, InPrintItemCode, HideItmNameIfDiscriptionOn, PartNoCheck, TimeOut, ProjectCheck, ConvertFrom, ConvertBill);
            var item = saleData.pdfItem.ToList();
            var summary = saleData;
            var billsundry = saleData.billsundry.ToList();


            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            bool stat = true;
            //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, fmapp, SalesEntryID = salesEntryId } };
            return Json(new { status = stat, item, summary, billsundry, fmapp, SalesEntryID = salesEntryId });


        }
        [HttpGet]
        public ActionResult downloadprint2(long salesEntryId)
        {
            ViewBag.salesEntryId = salesEntryId;
            return View();
        }
        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry,No Tax Sales")]
        public ActionResult UpdateSale(string[][] array, string[] saledata, SEBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel, ICollection<UBatchStockPViewModel> ubstmodel, string[][] arrayused, ICollection<commissionViewmodel> commission, string TenderingMode, string Mode, ICollection<SettlementViewModel> SettlementData, decimal? BalanceAmount, string[][] arrayR, ICollection<RackStockPViewModel> bsrackData)
        {
            string result2 = string.Empty;
            DataTable dtItem2 = new DataTable();
            dtItem2.Columns.Add("ItemUnit");
            dtItem2.Columns.Add("ItemUnitPrice");
            dtItem2.Columns.Add("ItemQuantity");
            dtItem2.Columns.Add("ItemSubTotal");
            dtItem2.Columns.Add("ItemDiscount");
            dtItem2.Columns.Add("ItemTax");
            dtItem2.Columns.Add("ItemTaxAmount");
            dtItem2.Columns.Add("ItemTotalAmount");
            dtItem2.Columns.Add("itemNote");
            dtItem2.Columns.Add("SaleEntry");
            dtItem2.Columns.Add("Item");
            dtItem2.Columns.Add("Type");

            foreach (var arr in array)
            {
                DataRow dr = dtItem2.NewRow();
                dr["ItemUnit"] = arr[1];
                dr["ItemUnitPrice"] = Convert.ToDecimal(arr[4]);
                dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                dr["ItemSubTotal"] = Convert.ToDecimal(arr[6]);
                dr["ItemDiscount"] = Convert.ToDecimal(arr[7]);
                dr["ItemTax"] = Convert.ToDecimal(arr[11]);//arr[10]
                dr["ItemTaxAmount"] = Convert.ToDecimal(arr[10]);
                dr["ItemTotalAmount"] = Convert.ToDecimal(arr[12]);
                //if (Convert.ToString(arr[29].Replace("\n", "<br />")) == "http://")

                dr["itemNote"] = arr[33];
                dr["SaleEntry"] = 1;
                dr["Item"] = Convert.ToInt32(arr[0]);
                dr["Type"] = false;

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
                    long typ = Convert.ToInt64(saledata[32]);
                    var bundle = (from a in db.BundleItems
                                  join b in db.Items on a.ItemId equals b.ItemID
                                  join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                  from c in primary.DefaultIfEmpty()
                                  let hir = db.HireRates.Where(m => m.ItemId == b.ItemID && m.type == typ).Select(y => y.Rate).FirstOrDefault()
                                  where a.ItemBundle == itemBundle.ItemBundleId
                                  select new
                                  {
                                      b.ItemCode,
                                      b.ItemName,
                                      c.ItemUnitName,
                                      ItemUnitPrice = a.ItemSubTotal,
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


                        DataRow dbu = dtItem2.NewRow();
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
                        dbu["SaleEntry"] = 1;
                        dbu["Item"] = bu.Item;
                        dbu["Type"] = false;
                    }
                }

            }


            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            long SalesEntryID = 0;
            Int64 salesEntryId = Convert.ToInt64(saledata[15]);
            SalesEntry SEentry = db.SalesEntrys.Find(salesEntryId);
            SEentry.SalesStatus = Convert.ToInt32(saledata[59]);
            var perid = Convert.ToDecimal(saledata[60]);
            SEentry.pricecategoryid = db.PriceCategories.Where(o => o.value == perid).Select(o => o.pricestratagyid).FirstOrDefault();
            bool stat = false;
            string msg;
            long MCRet = 0;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
            var HideItmName = db.EnableSettings.Where(a => a.EnableType == "HideItemName").FirstOrDefault();
            var HideItmNameIfDiscriptionOn = HideItmName != null ? HideItmName.Status : Status.inactive;
            var ConvertFrom = Convert.ToString(saledata[38]);
            var ConvertBill = Convert.ToString(saledata[39]);
            //

            if (saledata[18] == "printonly")
            {
                var conv = db.ConvertTransactionss.Any(u => u.To == salesEntryId);
                List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
                if (conv)
                {
                    List<string> ExList = new List<string>();
                    List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                    ExtList = ExtNumDetails((long)salesEntryId, ExtList);
                    var Extended = ExtList.Select(z => z.To).ToList();
                    Int32 count = 0;


                    var ConvModel = (from a in db.ConvertTransactionss
                                     join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                                     from b in primary.DefaultIfEmpty()
                                     where Extended.Contains(a.To)
                                     select new ConvertTransactionsViewModel
                                     {
                                         ConvertFrom = (a.ConvertFrom == "SaleExtend") ? "Sale" : (a.ConvertFrom == "Quote") ? "Quotation" : (a.ConvertFrom == "DVNote") ? "Delivery Note" : a.ConvertFrom,
                                         Id = b.SalesEntryId,
                                         BillNo = b.BillNo,
                                         CreatedDate = a.CreatedDate,
                                         From = a.From
                                     }).OrderBy(b => b.CreatedDate).ToList();

                    var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                    ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                    parentvm.BillNo = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                    parentvm.Id = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.SalesEntryId).FirstOrDefault();
                    parentvm.ConvertFrom = "SaleExtend";
                    ConvModel.Add(parentvm);
                    ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                    var str = ConvExt.Find(c => c.Id == salesEntryId);
                    ConvExt.Remove(str);
                }

                var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                var fmapp = db.FieldMappings.Where(a => a.Section == "Sales" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                var saleData = com.SaleData(salesEntryId, InPrintItemCode, HideItmNameIfDiscriptionOn, PartNoCheck, TimeOut, ProjectCheck, ConvertFrom, ConvertBill);
                var item = saleData.pdfItem.ToList();
                var summary = saleData;
                var billsundry = saleData.billsundry.ToList();


                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                var def = (PriLay == Status.active) ? Convert.ToInt64(saledata[45]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                var ConvExtList = ConvExt;
                var st = ConvExt.Find(c => c.BillNo == SEentry.BillNo);
                ConvExt.Remove(st);
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, ConvExtList, fmapp, SalesEntryID = salesEntryId } };
            }

            if (com.islocked("Sales",SEentry.SEDate ))
            {
                msg = "This Sales Is Locked";
               
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            if (BillExist(Convert.ToString(saledata[16])) && Convert.ToString(saledata[16]) != SEentry.BillNo)
            {

                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            Int64 saleAcc = (long)db.companys.Select(a => a.SaleAccount).FirstOrDefault();


            var UserId = User.Identity.GetUserId();
            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;



            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

            if (BranchCheck == Status.active)
            {
                Branch = Convert.ToInt64(saledata[25]);
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            long MC = 0;
            if (MCcheck == Status.active)
            {
                MC = Convert.ToInt64(saledata[24]);
                MCRet = Convert.ToInt64(saledata[58]);
            }
            else
            {
                MC = 1;
                MCRet = 1;
            }

            long Currency = 0;
            var ConRate = "";
            if (ViewBag.EnableCurrency == 0)
            {

                Currency = Convert.ToInt64(saledata[26]);
                ConRate = saledata[27];
            }
            else
            {
                Currency = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.Id).FirstOrDefault();
                ConRate = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.ConvertionRate).FirstOrDefault();
            }
            string action = saledata[18];
            var seitemss = db.SEItemss.Where(o => o.SalesEntry == salesEntryId).ToList();
            int flag = 0;
            foreach (var arr in seitemss)
            {
                flag = 0;

                for (var i = 0; i < array.Count(); i++)
                {
                    if (array[i][0] == arr.Item.ToString())
                    {
                        flag = 1;
                    }
                }
                if (flag == 0)
                {
                    var itemcode = db.Items.Where(o => o.ItemID == arr.Item).FirstOrDefault();
                }
            }
            foreach (var abc in array)
            {
                flag = 0;
                foreach (var sei in seitemss)
                {
                    if (sei.Item.ToString() == abc[0])
                    {
                        flag = 1;
                    }
                }
                if (flag == 0)
                {
                    long k = Convert.ToInt64(abc[0]);
                    var itemcode = db.Items.Where(o => o.ItemID == k).FirstOrDefault();
                    com.addlog(LogTypes.Updated, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Item code " + itemcode.ItemCode + " Added");
                }
            }
            var EditPermission = User.IsInRole("Disable Sale Edit After Approval");
            if (com.chkApproved(salesEntryId, EditPermission, "SalesEntry", UserId) == true)
            {
                #region updates


                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var TaxAmount = Convert.ToDecimal(saledata[5]);

                var SEGrandTotal = Convert.ToDecimal(saledata[7]);
                var saleamount = SEGrandTotal - TaxAmount;
                var subtotal = Convert.ToDecimal(saledata[8]);

                SEentry.SEDate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                SEentry.SECashier = saledata[1] != "" ? Convert.ToInt64(saledata[1]) : 0;
                if (saledata[29] != null)
                {
                    string str = saledata[29];
                    SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                    SEentry.SaleType = Stype;
                }
                else
                {
                    SEentry.SaleType = SaleType.Sale;
                }

                var PreCust = SEentry.CustomerType;

                var CustType = (saledata[12] == "2") ? CustomerType.Card : (saledata[12] == "1") ? CustomerType.Walking : (saledata[12] == "3") ? CustomerType.Online : (saledata[12] == "4") ? CustomerType.OnlineAccount : CustomerType.Customer;

                if (TenderingMode == "inactive")
                    SEentry.CustomerType = CustType;
                else
                {
                    if (BalanceAmount != SEGrandTotal)
                        SEentry.CustomerType = CustomerType.Walking;
                    else
                        SEentry.CustomerType = CustomerType.Customer;
                }




                SEentry.Customer = Convert.ToInt64(saledata[0]);
                SEentry.PONo = saledata[17];

                //for pos

                SEentry.PayType = "";//need change
                SEentry.BillNo = Convert.ToString(saledata[16]);
                SEentry.SEItems = Convert.ToInt32(saledata[3]);
                SEentry.SEItemQuantity = Convert.ToDecimal(saledata[4]);
                SEentry.SESubTotal = Convert.ToDecimal(saledata[8]);
                SEentry.SETax = Convert.ToDecimal(saledata[9]);
                SEentry.SETaxAmount = TaxAmount;
                SEentry.SEDiscount = Convert.ToDecimal(saledata[6]);
                SEentry.SEGrandTotal = SEGrandTotal;
                SEentry.SENote = saledata[11];
                SEentry.Print = 1;
                SEentry.Status = 1;
                SEentry.Branch = Branch;

                SEentry.Location = saledata[21];
                SEentry.SalesType = Convert.ToInt64(saledata[22]);
                SEentry.Remarks = saledata[23];
                SEentry.MaterialCenter = MC;
                SEentry.Currency = Currency;
                SEentry.ConvertionRate = ConRate;
                SEentry.FCTotal = Convert.ToDecimal(saledata[28]);
                SEentry.Project = saledata[35] != "" ? Convert.ToInt64(saledata[35]) : 0;

                if (saledata[12] == "2")
                {
                    SEentry.PaymentMethod = (long?)Convert.ToInt64(saledata[20]);//paymethod
                }
                else
                {
                    SEentry.PaymentMethod = null;
                }
                SEentry.HSCode = saledata[33];
                SEentry.PaymentTerms = saledata[34];


                SEentry.Ref1 = Convert.ToString(saledata[40]);
                SEentry.Ref2 = Convert.ToString(saledata[41]);
                SEentry.Ref3 = Convert.ToString(saledata[42]);
                SEentry.Ref4 = Convert.ToString(saledata[43]);
                SEentry.Ref5 = Convert.ToString(saledata[44]);

                if (SEentry.Project != null && SEentry.Project != 0)
                {
                    SEentry.SaleAccount = db.Projects.Where(a => a.ProjectId == SEentry.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                }
                else
                {
                    SEentry.SaleAccount = saleAcc;
                }
                SEentry.materialcost = 1000000;
                db.Entry(SEentry).State = EntityState.Modified;

                var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

                //To Update the quantity in Edit Mode(ItemTransaction Table)
                if (array != null && MC != 0)
                    com.ItemTransInEditMode("CreditSale", MC, 0, 0, array, salesEntryId, UserId, CurrentDate);

                //To update the used quantity in Edit Mode(ItemTransaction Table)
                if (arrayused != null && MC != 0)
                    com.ItemTransInEditMode("CreditSaleUsedMaterials", MC, 0, 0, arrayused, salesEntryId, UserId, CurrentDate);

                var HireItem = db.HireDetails.Where(a => a.Reference == salesEntryId && a.Section == "Sales").FirstOrDefault();
                if (HireItem != null)
                {
                    db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == salesEntryId && a.Section == "Sales"));
                    db.SaveChanges();
                }

                if (SEentry.SaleType == SaleType.Hire)
                {
                    HireDetail HDetils = new HireDetail();
                    HDetils.StartDate = DateTime.Parse(saledata[30], new CultureInfo("en-GB"));
                    HDetils.EndDate = DateTime.Parse(saledata[31], new CultureInfo("en-GB"));
                    HDetils.Section = "Sales";
                    HDetils.Reference = salesEntryId;
                    HDetils.HireType = Convert.ToInt64(saledata[32]);
                    db.HireDetails.Add(HDetils);
                    db.SaveChanges();
                }


                var SEItem = db.SEItemss.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
                if (SEItem != null)
                {
                    var SItems = db.SEItemss.Where(a => a.SalesEntry == salesEntryId).ToList();
                    db.DummySEItems.RemoveRange(db.DummySEItems.Where(a => a.SalesEntry == salesEntryId));
                    db.SaveChanges();
                    foreach (var arr in SItems)
                    {
                        //add to dummy table
                        DummySEItem dItem = new DummySEItem();
                        dItem.ItemUnit = arr.ItemUnit;
                        dItem.ItemUnitPrice = arr.ItemUnitPrice;
                        dItem.ItemQuantity = arr.ItemQuantity;
                        dItem.ItemSubTotal = arr.ItemSubTotal;
                        dItem.ItemDiscount = arr.ItemDiscount;
                        dItem.ItemTax = arr.ItemTax;
                        dItem.ItemTaxAmount = arr.ItemTaxAmount;
                        dItem.ItemTotalAmount = arr.ItemTotalAmount;
                        dItem.itemNote = arr.itemNote;
                        dItem.SalesEntry = arr.SalesEntry;
                        dItem.Item = arr.Item;
                        db.DummySEItems.Add(dItem);
                        db.SaveChanges();
                    }

                    db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == salesEntryId));
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
                dtItem.Columns.Add("itemNote");
                dtItem.Columns.Add("SaleEntry");
                dtItem.Columns.Add("Item");
                dtItem.Columns.Add("Type");

                foreach (var arr in array)
                {
                    DataRow dr = dtItem.NewRow();
                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = Convert.ToDecimal(arr[4]);
                    dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[6]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[7]);
                    dr["ItemTax"] = Convert.ToDecimal(arr[11]);//arr[10]
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[10]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[12]);
                    //if (Convert.ToString(arr[29].Replace("\n", "<br />")) == "http://")

                    dr["itemNote"] = arr[33];
                    dr["SaleEntry"] = salesEntryId;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    dr["Type"] = false;
                    dtItem.Rows.Add(dr);

                    var item = Convert.ToInt32(arr[0]);

                    //---production-------//

                    long itemID = Convert.ToInt32(arr[0]);
                    var dt = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));

                    var ifbill = db.BillOfMaterialsoffers.Any(o => o.ItemId == itemID && o.BOMDateStart <= dt && o.BOMDateEnd >= dt);
                    bool available = ifbill;
                    if (ifbill)
                    {

                        var bomid = db.BillOfMaterialsoffers.Where(o => o.ItemId == itemID && o.BOMDateStart <= dt && o.BOMDateEnd >= dt).Select(o => o.BOMOfferId).FirstOrDefault();

                        if (bomid > 0)
                        {
                            var reqitem = db.BOMItemsoffers.Where(o => o.BOMOfferId == bomid).ToList();
                            deleteproduction(salesEntryId);
                            foreach (var reqit in reqitem)
                            {
                                available = true;
                            }
                            if (available)
                            {
                                createproduction(bomid, salesEntryId, MC);
                            }
                        }
                    }
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
                        long typ = Convert.ToInt64(saledata[32]);
                        var bundle = (from a in db.BundleItems
                                      join b in db.Items on a.ItemId equals b.ItemID
                                      join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                      from c in primary.DefaultIfEmpty()
                                      let hir = db.HireRates.Where(m => m.ItemId == b.ItemID && m.type == typ).Select(y => y.Rate).FirstOrDefault()
                                      where a.ItemBundle == itemBundle.ItemBundleId
                                      select new
                                      {
                                          b.ItemCode,
                                          b.ItemName,
                                          c.ItemUnitName,
                                          ItemUnitPrice = (SEentry.SaleType == SaleType.Sale) ? a.ItemSubTotal : hir,
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
                            if (SEentry.SalesType == 2)
                            {
                                itemtax = 0;
                                taxamt = 0;
                                totamt = ItemSubTotal;
                            }
                            else
                            {
                                itemtax = bu.ItemTax;
                                taxamt = buTaxAmount;
                                totamt = (buTaxAmount + ItemSubTotal);
                            }

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
                            dbu["SaleEntry"] = salesEntryId;
                            dbu["Item"] = bu.Item;
                            dbu["Type"] = false;
                            dtItem.Rows.Add(dbu);
                        }
                    }

                }

                ////// create parameter 
                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypeSEItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertSEItems", "@TableType");
                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, salesEntryId); // forward-correctness: header = SUM(lines)
                //// execute sql 
                var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                Int64 Usedmaterials = 0;
                if (arrayused != null)
                {
                    Usedmaterials = SaveUsedMaterials(UserId, Branch, saledata, arrayused, salesEntryId, null);
                }
                if (ret > 0)
                {
                }


                // batch stock

                var SEBst = db.BatchStocks.Where(a => a.Reference == salesEntryId && a.Type == "Sales").FirstOrDefault();
                if (SEBst != null)
                {
                    db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == salesEntryId && a.Type == "Sales"));
                    db.SaveChanges();
                }

                //Used batchstock

                var USEBst = db.BatchStocks.Where(a => a.Reference == salesEntryId && a.Type == "UsedSales").FirstOrDefault();
                if (USEBst != null)
                {
                    db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == salesEntryId && a.Type == "UsedSales"));
                    db.SaveChanges();
                }
                var PERack = db.shelfstockmovements.Where(a => a.referenceid == salesEntryId && a.purpose == "Sales").FirstOrDefault();
                if (PERack != null)
                {
                    db.shelfstockmovements.RemoveRange(db.shelfstockmovements.Where(a => a.referenceid == salesEntryId && a.purpose == "Sales"));
                    db.SaveChanges();
                }
                //rackstock
                if (bsrackData != null)
                {
                    foreach (var bst in bsrackData)
                    {
                        if (bst.StockOut != 0)
                        {

                            decimal bStockIn = 0;

                            shelfstockmovement Btst = new shelfstockmovement();
                            Btst.purpose = "Sales";
                            Btst.itemid = bst.Item;
                            Btst.unitid = (long)bst.Unit;
                            Btst.rackmciid = (long)com.getrackmcid(MC, bst.RackNo, bst.ShelfNo);
                            Btst.qty = bst.StockOut;

                            Btst.referenceid = salesEntryId;


                            Btst.createddate = DateTime.Now;
                            Btst.createdby = UserId;

                            db.shelfstockmovements.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                db.commissions.RemoveRange(db.commissions.Where(o => o.salesid == salesEntryId));
                db.SaveChanges();
                if (commission != null)
                {
                    foreach (var cm in commission)
                    {
                        commission cmm = new commission
                        {
                            agent = Convert.ToInt64(cm.agent),
                            commisionmode = Convert.ToInt32(cm.commisionmode),
                            commisiontype = Convert.ToInt32(cm.commisiontype),
                            comvalue = Convert.ToInt64(cm.comvalue),
                            salesid = salesEntryId
                        };
                        db.commissions.Add(cmm);
                        db.SaveChanges();
                    }
                }
                if (bstmodel != null)
                {
                    foreach (var bst in bstmodel)
                    {
                        if (bst.BatchNo != "" && bst.BatchNo != null)
                        {
                            DateTime? exp = null;
                            DateTime? mfg = null;
                            if (bst.EXP != null && bst.EXP != "")
                            {
                                exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                            }
                            if (bst.MFG != null && bst.MFG != "")
                            {
                                mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                            }
                            decimal bStock = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStock = bst.StockOut * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockOut = bStock;
                            Btst.StockIn = 0;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = salesEntryId;
                            Btst.Type = "Sales";

                            Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            Btst.Date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                //Usedbatchstocks

                if (ubstmodel != null)
                {
                    foreach (var bst in ubstmodel)
                    {
                        if (bst.BatchNo != "" && bst.BatchNo != null)
                        {
                            DateTime? exp = null;
                            DateTime? mfg = null;
                            if (bst.EXP != null && bst.EXP != "")
                            {
                                exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                            }
                            if (bst.MFG != null && bst.MFG != "")
                            {
                                mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                            }
                            decimal bStock = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStock = bst.StockOut * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockOut = bStock;
                            Btst.StockIn = 0;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = salesEntryId;
                            Btst.Type = "UsedSales";

                            Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            Btst.Date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }


                var SEBs = db.SEBillSundrys.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
                if (SEBs != null)
                {
                    db.SEBillSundrys.RemoveRange(db.SEBillSundrys.Where(a => a.SalesEntry == salesEntryId));
                    db.SaveChanges();
                }
                if (bsmodel.sebsundrys != null)
                {
                    string bsResult = string.Empty;
                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("SalesEntry");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.sebsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["SalesEntry"] = salesEntryId;
                        drw["BillSundry"] = bs.BillSundry;
                        drw["BsValue"] = bs.BsValue;
                        drw["AmountType"] = bs.AmountType;
                        drw["BsType"] = bs.BsType;
                        drw["BsAmount"] = bs.BsAmount;

                        BsEntry.Rows.Add(drw);
                    }

                    ////// create parameter 
                    SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                    parameter1.SqlDbType = SqlDbType.Structured;
                    parameter1.TypeName = "TableTypeSEBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertSEBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);

                }


                decimal amount = Convert.ToDecimal(saledata[10]);
                var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                Int64 custAccID = db.Customers.Where(a => a.CustomerID == SEentry.Customer).Select(a => a.Accounts).FirstOrDefault();

                Int64 saleAccId = saleAcc;//db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).FirstOrDefault();
                if (SEentry.Project != null && SEentry.Project != 0)
                {
                    saleAccId = db.Projects.Where(a => a.ProjectId == SEentry.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                }

                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATOutput = db.Accountss.Where(a => (a.Group == 24 || a.Group == 0) && a.Name == "VAT Output").Select(a => a.AccountsID).FirstOrDefault();


                ////Remove Cheque Details 

                if (TenderingMode == "inactive" || (Mode == "CancelFromSettlement" && SettlementData == null))
                {
                    //card // walkin customer
                    if (saledata[12] == "2" || saledata[12] == "1")
                    {
                        //AccountsTransaction
                        amount = SEGrandTotal;
                    }



                    //----------new added----------------
                    if (saledata[12] == "2" || saledata[12] == "1")//cash//card
                    {
                        deleteAndUpdateTrans(salesEntryId, saledata, amount, custAccID, cashAccId, BranchID, UserId);
                        changeRecBill(salesEntryId);
                    }
                    if (saledata[12] == "0")//credit
                    {
                        if (PreCust == CustomerType.Walking || PreCust == CustomerType.Card)//previous cash
                        {
                            deleteAndUpdateTrans(salesEntryId, saledata, amount, custAccID, cashAccId, BranchID, UserId);
                        }
                        if (PreCust == CustomerType.Customer)//previous credit
                        {

                            decimal sumTran = db.SETransactions.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault() != null ? (decimal?)db.SETransactions.Where(a => a.SalesEntry == salesEntryId).Select(a => a.SEPayAmount).Sum() ?? 0 : 0;
                            if (sumTran > SEGrandTotal)
                            {
                                var chkrec = db.ReceiptBills.Where(a => a.InvoiceNo == salesEntryId && a.BillType == "Sales" && a.Type == "Against Reference").ToList();
                                if (chkrec != null)
                                {
                                    var recamount = sumTran - SEGrandTotal;
                                    amount = SEGrandTotal;
                                    decimal TotAmt = SEGrandTotal;
                                    foreach (var rbill in chkrec)
                                    {
                                        TotAmt = TotAmt - rbill.Amount;
                                        if (TotAmt < 0 && rbill.Amount > recamount)
                                        {
                                            var reamt = rbill.Amount - recamount;

                                            ReceiptBill recbillz = db.ReceiptBills.Find(rbill.ReceiptBillId);
                                            recbillz.Amount = reamt;
                                            db.Entry(recbillz).State = EntityState.Modified;
                                            db.SaveChanges();

                                            ReceiptBill recbill = new ReceiptBill();
                                            recbill.InvoiceNo = rbill.InvoiceNo;
                                            recbill.NewRefName = rbill.NewRefName;
                                            recbill.Receipt = Convert.ToInt64(rbill.Receipt);
                                            recbill.BillType = null;
                                            recbill.Amount = recamount;
                                            recbill.Type = "New Reference";
                                            recbill.Status = Status.active;

                                            db.ReceiptBills.Add(recbill);
                                            db.SaveChanges();

                                        }
                                    }
                                    updateSepayment(salesEntryId, saledata, amount, BranchID, 0);
                                }
                                else
                                {
                                    deleteAndUpdateTrans(salesEntryId, saledata, amount, custAccID, cashAccId, BranchID, UserId);
                                }
                            }
                            else
                            {
                                updateSepayment(salesEntryId, saledata, amount, BranchID, 1);
                            }

                        }
                    }

                }

                bool delete = com.DeleteAllAccountTransaction("Sale", salesEntryId);
                bool deletepay = com.DeleteAllAccountTransaction("Sale Payment", salesEntryId);
                var enbonusforcustomer = db.EnableSettings.Where(a => a.EnableType == "bonusforcustomer").FirstOrDefault();
                var bonusforcustomer = enbonusforcustomer != null ? enbonusforcustomer.Status : Status.inactive;

                if (bonusforcustomer == Status.active)
                {
                    var bon = db.customerbonus.Where(o => o.customerid == SEentry.Customer && o.salesentryid == SalesEntryID).Select(o => o.claimamount).FirstOrDefault();
                    if (bon != null)
                    {
                        var bonusclaim = bon;
                        if (bonusclaim > 0)
                        {
                            long discountre = 497;
                            saleamount = saleamount - (decimal)bonusclaim;
                            com.addAccountTrasaction(0, (decimal)bonusclaim, discountre, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);

                        }


                    }
                }


                //bill sundry account
                var Gtotal = SEGrandTotal;
                decimal deductions = 0;
                if (bsmodel.sebsundrys != null)
                {
                    foreach (var bs in bsmodel.sebsundrys)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                        {


                            var bsamount = bs.BsAmount == null ? 0 : (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                saleamount = saleamount - bsamount;
                                com.addAccountTrasaction(0, (decimal)bsamount, (long)ChkAcc.SAccount, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);
                            }
                            else //substract
                            {
                                saleamount = saleamount + bsamount;
                                com.addAccountTrasaction((decimal)bsamount, 0, (long)ChkAcc.SAccount, "Sale", salesEntryId, DC.Debit, date, null, null, SEentry.Project, SEentry.ProTask);
                            }
                        }
                        else
                        {
                            decimal bsamount = 0;
                            if (bs.BsAmount == null)
                                bsamount = 0;
                            else
                                bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
#pragma warning disable IDE0054 // Use compound assignment
                                deductions = deductions - bsamount;
#pragma warning restore IDE0054 // Use compound assignment
                            }
                            else //substract
                            {
                                deductions = deductions + bsamount;
                            }

                        }
                    }
                }

                //add sale trasaction with customer debt amount
                com.addAccountTrasaction(SEGrandTotal, 0, custAccID, "Sale", salesEntryId, DC.Debit, date, null, null, SEentry.Project, SEentry.ProTask);

                if (SEentry.SalesType == 3)
                {//voucher wise
                    com.addAccountTrasaction(0, SEentry.SESubTotal - deductions, saleAccId, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);
                }
                else
                {
                    com.addAccountTrasaction(0, saleamount, saleAccId, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);
                }

                // add vat input in account transaction
                if (TaxAmount > 0 && SEentry.SalesType != 3)
                    com.addAccountTrasaction(0, TaxAmount, VATOutput, "Sale", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);

                int flat = 0;
                if (TenderingMode == "inactive" || (Mode == "CancelFromSettlement" && SettlementData == null))
                {
                    if (saledata[12] == "2" || saledata[12] == "1")
                    {
                        // payment method
                        long? paymethod = saledata[20].ToString() == "" ? null : (long?)Convert.ToInt64(saledata[20]);

                        cashAccId = paymethod == 0 ? cashAccId : (long)db.PaymentMethods.Where(a => a.PaymentMethodId == paymethod).Select(a => a.AccountId).FirstOrDefault();
                    }

                    if (Convert.ToDecimal(saledata[10]) > 0 || (saledata[12] == "2" || saledata[12] == "1"))
                    {
                        //if payment
                        com.addAccountTrasaction(0, amount, custAccID, "Sale Payment", salesEntryId, DC.Credit, date, null, null, SEentry.Project, SEentry.ProTask);
                        com.addAccountTrasaction(amount, 0, cashAccId, "Sale Payment", salesEntryId, DC.Debit, date, null, null, SEentry.Project, SEentry.ProTask);
                    }


                }
                else
                {
                    flat = 1;
                    //Call Function to save settlement details
                    UpdateSettlement(SettlementData, salesEntryId, SEentry.SEDate, SEentry.Customer, SEentry.Project, SEentry.ProTask, SEentry.SEGrandTotal, BranchID, custAccID);
                }
                var existingtask = db.additionaltasks.Where(o => o.salesentryid == salesEntryId).FirstOrDefault();
                if (existingtask != null)
                {
                    db.additionaltasks.RemoveRange(db.additionaltasks.Where(o => o.salesentryid == salesEntryId));
                    db.SaveChanges();
                    SEentry = db.SalesEntrys.Find(salesEntryId);

                    SEentry.ProTask = 0;
                    db.Entry(SEentry).State = EntityState.Modified;
                    db.SaveChanges();
                }
                var addtask = Convert.ToString(saledata[36]);


                int i = 0;
                if (addtask != null && addtask != "")
                {

                    long[] addtaskar = addtask.Split(',').Select(Int64.Parse).ToArray();

                    additionaltaks adt = new additionaltaks();
                    foreach (var emp in addtaskar)
                    {
                        if (i == 0)
                        {
                            SEentry = db.SalesEntrys.Find(salesEntryId);

                            SEentry.ProTask = (emp != null || emp != 0) ? emp : 0;
                            db.Entry(SEentry).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            adt.salesentryid = salesEntryId;
                            adt.taskid = emp;

                            db.additionaltasks.Add(adt);
                            db.SaveChanges();

                        }
                        i++;
                    }
                }
                else
                {
                    SEentry.ProTask = 0;
                    db.Entry(SEentry).State = EntityState.Modified;
                    db.SaveChanges();
                }
                //Approved By
                var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == salesEntryId && a.Type == "SalesEntry").FirstOrDefault();

                var MrnPO = db.Approvals.Where(a => a.TransEntry == salesEntryId && a.Type == "SalesEntry").FirstOrDefault();
                if (MrnPO != null)
                {
                    if (chkapp != null)
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == salesEntryId && a.Type == "SalesEntry"));
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == salesEntryId && a.Type == "SalesEntry"));
                        db.SaveChanges();
                    }

                }
                var Appby = Convert.ToString(saledata[37]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();
                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = salesEntryId;
                        approval.Type = "SalesEntry";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }

                com.addlog(LogTypes.Updated, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Successfully Updated Sales Entry");
                if (flat == 0)
                {

                }
                #endregion
            }

            Int64 SReturnId = 0;
            var returninsale = db.EnableSettings.Where(a => a.EnableType == "SalesReturnInSales").FirstOrDefault();
            var returninsales = returninsale != null ? returninsale.Status : Status.inactive;

            //Save Sales Return Details Only If 'Sales Return In Sales Entry' is Active in Configuration
            if (returninsales == Status.active)
            {
                var paytype = Convert.ToString(saledata[46]);
                SReturnId = SaveSalesReturn(salesEntryId, UserId, Branch, MCRet, saledata, bsmodel, arrayR, paytype, "EditMode");
            }
            var autopur = db.EnableSettings.Where(a => a.EnableType == "stockcheckinvoice").FirstOrDefault();
            var autopurcheck = autopur != null ? autopur.Status : Status.inactive;

            if (autopurcheck == Status.active)
            {

                var paytype = Convert.ToString(saledata[56]);
                var purchaseid = SavePurhase(salesEntryId, UserId, Branch, MCRet, saledata, array, bsmodel, paytype, "EditMode");

            }
            //send mail to company address
            var sendmail = db.EnableSettings.Where(a => a.EnableType == "AutomaticMailInTransactions").FirstOrDefault();
            var sendcmail = sendmail != null ? sendmail.Status : Status.inactive;
            if (sendcmail == Status.active)
            {
                var custname = db.Customers.Where(a => a.CustomerID == SEentry.Customer).Select(a => a.CustomerName).FirstOrDefault();
                var salesman = db.Employees.Where(a => a.EmployeeId == SEentry.SECashier).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
                var PayType = (SEentry.CustomerType == CustomerType.Card ? (db.PaymentMethods.Where(a => a.PaymentMethodId == SEentry.PaymentMethod).Select(a => a.MethodName).FirstOrDefault()) : (SEentry.CustomerType == CustomerType.Walking ? "Cash" : "Credit"));
                var username = db.Users.Where(a => a.Id == SEentry.CreatedBy).Select(a => a.UserName).FirstOrDefault();

                CompanyEmailFormat CEmail = new CompanyEmailFormat();
                CEmail.BillNo = "Tax invoice-" + SEentry.BillNo;
                CEmail.Subject = "<table style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;' colspan='2' align='center'><b>Tax Invoice Updated</b></td><tr/> " +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Type               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + Enum.GetName(typeof(SaleType), SEentry.SaleType) + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Date               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SEDate.ToString("dd-MM-yyyy") + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Customer           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + custname + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Sales Executive    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + salesman + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Payment Type       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + PayType + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created Date       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SECreatedDate.ToString("dd-MM-yyyy hh:mm:ss tt") + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created User       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + username + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Sub Total          :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SESubTotal + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Tax Amount         :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SETaxAmount + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Discount           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + SEentry.SEDiscount + "</td><tr/>" +
                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><b> Grand Total    :</b></td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><b> " + SEentry.SEGrandTotal + "</b></td><tr/></table>";

                com.SendToCompanyMail(CEmail);
            }


            if (action == "print")
            {
                var conv = db.ConvertTransactionss.Any(u => u.To == salesEntryId);
                List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
                if (conv)
                {
                    List<string> ExList = new List<string>();
                    List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                    ExtList = ExtNumDetails((long)salesEntryId, ExtList);
                    var Extended = ExtList.Select(z => z.To).ToList();
                    Int32 count = 0;


                    var ConvModel = (from a in db.ConvertTransactionss
                                     join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                                     from b in primary.DefaultIfEmpty()
                                     where Extended.Contains(a.To)
                                     select new ConvertTransactionsViewModel
                                     {
                                         ConvertFrom = (a.ConvertFrom == "SaleExtend") ? "Sale" : (a.ConvertFrom == "Quote") ? "Quotation" : (a.ConvertFrom == "DVNote") ? "Delivery Note" : a.ConvertFrom,
                                         Id = b.SalesEntryId,
                                         BillNo = b.BillNo,
                                         CreatedDate = a.CreatedDate,
                                         From = a.From
                                     }).OrderBy(b => b.CreatedDate).ToList();

                    var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                    ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                    parentvm.BillNo = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                    parentvm.Id = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.SalesEntryId).FirstOrDefault();
                    parentvm.ConvertFrom = "SaleExtend";
                    ConvModel.Add(parentvm);
                    ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                    var str = ConvExt.Find(c => c.Id == salesEntryId);
                    ConvExt.Remove(str);
                }

                var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                var fmapp = db.FieldMappings.Where(a => a.Section == "Sales" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                var saleData = com.SaleData(salesEntryId, InPrintItemCode, HideItmNameIfDiscriptionOn, PartNoCheck, TimeOut, ProjectCheck, ConvertFrom, ConvertBill);
                var item = saleData.pdfItem.ToList();
                var summary = saleData;
                var billsundry = saleData.billsundry.ToList();


                //sales return
                object itemR = "";
                object summaryR = "";
                object billsundryR = "";
                if (SReturnId != 0)
                {
                    var saleRetData = com.SalesReturnData(SReturnId, InPrintItemCode, PartNoCheck, TimeOut);
                    itemR = saleRetData["item"];
                    summaryR = saleRetData["summary"];
                    billsundryR = saleRetData["billsundry"];
                }

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                var def = (PriLay == Status.active) ? Convert.ToInt64(saledata[45]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                var ConvExtList = ConvExt;
                var st = ConvExt.Find(c => c.BillNo == SEentry.BillNo);
                ConvExt.Remove(st);
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, itemR, summaryR, billsundryR, layout, ConvExtList, fmapp, SalesEntryID = salesEntryId } };
            }

            else if (action == "sendmail")
            {
                SendMail sm = new SendMail();
                MailMessage message = new MailMessage();
                string ToMail = saledata[19];
                string CcMail = "";
                string InvoiceNo = "_TaxInvoice_" + SEentry.BillNo;
                string mess = db.EmailTemplates.Find(3L).EmailBody;
                var em = db.EmailTemplates.Where(a => a.Head == "TaxInvoice").FirstOrDefault();
                if (em != null)
                {
                    message.Subject = em.Subject;
                    message.Body = em.EmailBody;

                    message.Subject = "Tax Invoice";







                    DateTime datenow = DateTime.Now;
                    var creditperiod = db.Customers.Find(Convert.ToInt64(saledata[0])).CreditPeriod;

                    decimal ret = 0;
                    long CustId = Convert.ToInt64(saledata[0]);

                    var ConD = (from a in db.Customers
                                join c in db.Employees on a.SalesPerson equals c.EmployeeId into secondary
                                from c in secondary.DefaultIfEmpty()
                                join d in db.CustomerTyps on a.CustomerType equals d.TypeId into temp
                                from d in temp.DefaultIfEmpty()
                                where a.CustomerID == CustId

                                let Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0)
                                let Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0)

                                select new
                                {
                                    a.Accounts,
                                    CusType = a.CustomerType == null ? "" : d.Type,
                                    CreditLimit = a.CreditLimit == null ? 0 : a.CreditLimit,
                                    currentbalance = ((Debit > Credit) ? ((Debit - Credit)) : 0),
                                    acbalance = (Debit > Credit) ? ((Debit - Credit) + " Dr.") : ((Credit - Debit) + " Cr."),
                                    mob = (from co in db.Contacts
                                           join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                           join con in db.Country on co.CountryID equals con.CountryID into conn
                                           from con in conn.DefaultIfEmpty()
                                           where (rrr.RelationID == CustId && rrr.RelationType == 0)
                                           select new MobileViewModel
                                           {
                                               Num = "+" + con.CountryCode + co.Mobile,
                                               Name = co.FirstName + "  " + co.LastName,
                                               emails = co.EmailId,
                                           }).ToList(),
                                    pdc = (decimal?)(from b in db.AccountsTransactions
                                                     join c in db.Customers on b.Account equals c.Accounts

                                                     where b.Status != null && c.CustomerID == CustId
                                                     group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                                                     select new
                                                     {

                                                         GrandTotal = g.Sum(o => o.Credit)


                                                     }).Sum(x => x.GrandTotal) ?? 0,
                                    //pdc = (decimal?)(

                                    //       r.MOPayment == ModeOfPayment.PDC
                                    //           r.GrandTotal

                                    //        ).Sum(x => x.GrandTotal) ?? 0,

                                    ddlEmployee = c.EmployeeId == null ? 0 : c.EmployeeId,

                                    billage = (from a in db.SalesEntrys
                                               join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                                               from b in pay.DefaultIfEmpty()
                                               join c in db.Customers on a.Customer equals c.CustomerID into rec
                                               from c in rec.DefaultIfEmpty()
                                               join d in db.SalesReturns on a.SalesEntryId equals d.SalesEntryId into slret
                                               from d in slret.DefaultIfEmpty()

                                               where
                                                 (a.Customer == CustId) &&
                                                 (c.CreditPeriod > 0)




                                               select new
                                               {
                                                   id = a.SalesEntryId,
                                                   Date = a.SEDate,
                                                   Invoice = a.BillNo,
                                                   c.CreditPeriod,
                                                   Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - ((d.SRGrandTotal == null) ? 0 : d.SRGrandTotal)),
                                                   total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                                                   paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                                                   Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                                                   Ctyp = "Cus"
                                               }).Where(o => o.Amount > 3 && o.Days > creditperiod).OrderByDescending(o => o.Days).Distinct().Take(5).ToList(),
                                    includepdc = a.includepdc,

                                }).ToList();











                    mess = mess.Replace("|pdc|", ConD[0].pdc.ToString());
                    mess = mess.Replace("|balance|", (ConD[0].currentbalance - ((ConD[0].pdc == null) ? 0 : ConD[0].pdc)).ToString());

                    message.Body = mess;

                }
                if (mess.Contains("|attachment|"))
                    sm.SendPdfMail(generatePdf(salesEntryId), ToMail, CcMail, InvoiceNo, message);
                else
                    sm.sendMailwithoutattachment(ToMail, CcMail, InvoiceNo, message);

                var conv = db.ConvertTransactionss.Any(u => u.To == salesEntryId);
                List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
                if (conv)
                {
                    List<string> ExList = new List<string>();
                    List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                    ExtList = ExtNumDetails((long)salesEntryId, ExtList);
                    var Extended = ExtList.Select(z => z.To).ToList();
                    Int32 count = 0;


                    var ConvModel = (from a in db.ConvertTransactionss
                                     join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                                     from b in primary.DefaultIfEmpty()
                                     where Extended.Contains(a.To)
                                     select new ConvertTransactionsViewModel
                                     {
                                         ConvertFrom = (a.ConvertFrom == "SaleExtend") ? "Sale" : (a.ConvertFrom == "Quote") ? "Quotation" : (a.ConvertFrom == "DVNote") ? "Delivery Note" : a.ConvertFrom,
                                         Id = b.SalesEntryId,
                                         BillNo = b.BillNo,
                                         CreatedDate = a.CreatedDate,
                                         From = a.From
                                     }).OrderBy(b => b.CreatedDate).ToList();

                    var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                    ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                    parentvm.BillNo = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                    parentvm.Id = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.SalesEntryId).FirstOrDefault();
                    parentvm.ConvertFrom = "SaleExtend";
                    ConvModel.Add(parentvm);
                    ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                    var str = ConvExt.Find(c => c.Id == salesEntryId);
                    ConvExt.Remove(str);
                }

                var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                var fmapp = db.FieldMappings.Where(a => a.Section == "Sales" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                var saleData = com.SaleData(salesEntryId, InPrintItemCode, HideItmNameIfDiscriptionOn, PartNoCheck, TimeOut, ProjectCheck, ConvertFrom, ConvertBill);
                var item = saleData.pdfItem.ToList();
                var summary = saleData;
                var billsundry = saleData.billsundry.ToList();


                //sales return
                object itemR = "";
                object summaryR = "";
                object billsundryR = "";
                if (SReturnId != 0)
                {
                    var saleRetData = com.SalesReturnData(SReturnId, InPrintItemCode, PartNoCheck, TimeOut);
                    itemR = saleRetData["item"];
                    summaryR = saleRetData["summary"];
                    billsundryR = saleRetData["billsundry"];
                }

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                var def = (PriLay == Status.active) ? Convert.ToInt64(saledata[45]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                var ConvExtList = ConvExt;
                var st = ConvExt.Find(c => c.BillNo == SEentry.BillNo);
                ConvExt.Remove(st);
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, itemR, summaryR, billsundryR, layout, ConvExtList, fmapp, SalesEntryID = salesEntryId } };
            }


            else
            {

                msg = "Successfully Updated Sales Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, SalesEntryID = salesEntryId } };
            }
            ////    }
            //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        public void deleteAndUpdateTrans(long salesEntryId, string[] saledata, decimal amount, long custAccID, long cashAccId, long BranchID, string UserId)
        {
            var SEGrandTotal = Convert.ToDecimal(saledata[7]);
            var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));

            SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
            if (SEpay != null)
            {
                SEpay.SEDate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                SEpay.SEEntryDate = Convert.ToDateTime(System.DateTime.Now);
                SEpay.SEBillAmount = SEGrandTotal;
                //card //walkin customer
                if (saledata[12] == "2" || saledata[12] == "1")
                {
                    SEpay.SEPaidAmount = SEGrandTotal;
                }
                else
                {
                    SEpay.SEPaidAmount = amount != 0 ? amount : Convert.ToDecimal(saledata[10]);
                }
                SEpay.CustomerId = Convert.ToInt64(saledata[0]);
                SEpay.CreatedBranch = Convert.ToInt32(BranchID);
                SEpay.Status = 1;

                db.Entry(SEpay).State = EntityState.Modified;
            }
            db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.SalesEntry == salesEntryId));

            db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == salesEntryId && a.RefType == "Sales"));
            db.SaveChanges();

            if (amount > 0 || Convert.ToDecimal(saledata[10]) > 0 || (saledata[12] == "2" || saledata[12] == "1"))
            {
                //SETransaction
                SETransaction SEtran = new SETransaction();

                var Remark = "Direct Reciept From Sale Entry";
                long payid;


                SEtran.SEPayDate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                //card //walkin customer
                if (saledata[12] == "2" || saledata[12] == "1")
                {
                    SEtran.SEPayAmount = amount;
                }
                else
                {
                    SEtran.SEPayAmount = amount;
                }
                SEtran.CustomerId = Convert.ToInt64(saledata[0]);
                payid = com.addReceipt(date, custAccID, cashAccId, amount, amount, Remark, UserId, BranchID, salesEntryId);
                SEtran.Recieptid = payid;
                SEtran.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                SEtran.CreatedBranch = Convert.ToInt32(BranchID);
                SEtran.CreatedUserId = UserId;
                SEtran.SalesEntry = salesEntryId;
                SEtran.Status = 1;

                db.SETransactions.Add(SEtran);
                db.SaveChanges();
            }
        }

        public void updateSepayment(long salesEntryId, string[] saledata, decimal amount, long BranchID, int chk)
        {
            var SEGrandTotal = Convert.ToDecimal(saledata[7]);
            SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
            if (SEpay != null)
            {
                SEpay.SEDate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                SEpay.SEBillAmount = SEGrandTotal;

                if (chk == 0)
                {
                    //card //walkin customer
                    if (saledata[12] == "2" || saledata[12] == "1")
                    {
                        SEpay.SEPaidAmount = SEGrandTotal;
                    }
                    else
                    {
                        SEpay.SEPaidAmount = amount != 0 ? amount : Convert.ToDecimal(saledata[10]);
                    }
                }

                SEpay.CustomerId = Convert.ToInt64(saledata[0]);
                SEpay.CreatedBranch = Convert.ToInt32(BranchID);
                SEpay.Status = 1;
                db.Entry(SEpay).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                SEPayment SEpay1 = new SEPayment();
                SEpay1.SEDate = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                SEpay1.SEEntryDate = Convert.ToDateTime(System.DateTime.Now);
                SEpay1.SEBillAmount = Convert.ToDecimal(saledata[7]);//saledata[7]
                                                                     //walking customer
                if (saledata[12] == "1" || saledata[12] == "2")
                {
                    SEpay1.SEPaidAmount = Convert.ToDecimal(saledata[7]);
                }
                else
                {

                    SEpay1.SEPaidAmount = Convert.ToDecimal(saledata[10]);
                }
                SEpay1.CustomerId = Convert.ToInt64(saledata[0]);//saledata[0]
                SEpay1.CreatedBranch = Convert.ToInt32(BranchID);
                SEpay1.CreatedUserId = User.Identity.GetUserId();
                SEpay1.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                SEpay1.Status = 1;
                SEpay1.SalesEntry = salesEntryId;
                db.SEPayments.Add(SEpay1);
                db.SaveChanges();

            }

        }

        public void changeRecBill(long salesEntryId)
        {
            //receipt bill changes
            var chkrec = db.ReceiptBills.Where(a => a.InvoiceNo == salesEntryId && a.BillType == "Sales").ToList();
            if (chkrec != null)
            {
                db.ReceiptBills.Where(a => a.InvoiceNo == salesEntryId && a.BillType == "Sales").ToList().ForEach(a => a.Type = "New Reference");
                db.SaveChanges();
            }
        }

        [RedirectingAction]
        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Sales Entry")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            SalesEntryViewModel vmodel = new SalesEntryViewModel();
            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry into pay
                      from c in pay.DefaultIfEmpty()
                      join d in db.Employees on b.SECashier equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join f in db.Customers on b.Customer equals f.CustomerID into cust
                      from f in cust.DefaultIfEmpty()
                      join g in db.PaymentMethods on b.PaymentMethod equals g.PaymentMethodId into paymeth
                      from g in paymeth.DefaultIfEmpty()
                      join h in db.MCs on b.MaterialCenter equals h.MCId into mcs
                      from h in mcs.DefaultIfEmpty()
                      join t in db.SalesTypes on b.SalesType equals t.Id into stype
                      from t in stype.DefaultIfEmpty()
                      join u in db.HireDetails on new { h1 = b.SalesEntryId, h2 = "Sales" }
                      equals new { h1 = u.Reference, h2 = u.Section } into hir
                      from u in hir.DefaultIfEmpty()
                      join v in db.HireTypes on u.HireType equals v.HireTypeId into htyp
                      from v in htyp.DefaultIfEmpty()
                      join x in db.Contacts on f.Contact equals x.ContactID into cnt
                      from x in cnt.DefaultIfEmpty()
                      where b.SalesEntryId == id
                      select new
                      {
                          f.CustomerCode,
                          f.CustomerName,
                          b.SENo,
                          b.BillNo,
                          b.PONo,
                          b.SEDate,
                          b.SaleType,
                          SENote = b.SENote.Replace("\n", "<br />"),
                          d.FirstName,
                          d.LastName,
                          b.CustomerType,
                          b.SEDiscount,
                          b.SEGrandTotal,
                          SEPaidAmount = b.CustomerType == 0 ? c.SEPaidAmount : b.SEGrandTotal,
                          SEDueAmount = b.CustomerType == 0 ? b.SEGrandTotal - c.SEPaidAmount : 0,
                          PayType = (b.CustomerType == CustomerType.Card ? g.MethodName : (b.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                          b.ConvertType,
                          b.ConvertNo,
                          b.Location,
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          h.MCName,
                          t.Name,
                          x.EmailId,
                          b.HSCode,
                          b.PaymentTerms,

                          HType = (u != null) ? v.Name : "",
                          StartDate = (u != null) ? u.StartDate : null,
                          EndDate = (u != null) ? u.EndDate : null,

                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "SalesEntry"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).AsEnumerable().Select(o => new SalesEntryViewModel
                      {

                          CustomerName = o.CustomerCode + " - " + o.CustomerName,
                          SENo = o.SENo,
                          BillNo = o.BillNo,
                          PONo = o.PONo,
                          SEDate = o.SEDate,
                          SENote = o.SENote.Replace("\n", "<br />"),
                          EmployeeName = o.FirstName + " " + o.LastName,
                          CustomerType = o.CustomerType,
                          SEDiscount = o.SEDiscount,
                          SETotal = o.SEDiscount + o.SEGrandTotal,
                          SEGrandTotal = o.SEGrandTotal,
                          SEPaidAmount = o.SEPaidAmount,
                          SEDueAmount = o.SEDueAmount,
                          PayType = o.PayType,
                          ConvertType = o.ConvertType,
                          ConvertNo = o.ConvertNo,
                          Location = o.Location,
                          Remarks = o.Remarks.Replace("\n", "<br />"),
                          MCName = o.MCName,
                          SaleTypeName = Enum.GetName(typeof(SaleType), o.SaleType),
                          SalesTypeName = o.Name,

                          Ref1 = o.Ref1,
                          Ref2 = o.Ref2,
                          Ref3 = o.Ref3,
                          Ref4 = o.Ref4,
                          Ref5 = o.Ref5,
                          EmailId = o.EmailId,
                          HSCode = o.HSCode,
                          PaymentTerms = o.PaymentTerms,
                          HType = o.HType,
                          StartDate = o.StartDate,
                          EndDate = o.EndDate,
                          Emp = o.Emp,
                      }).FirstOrDefault();



            vmodel.SEItem = db.SEItemss.Where(a => a.SalesEntry == id && a.Type == false && a.itemNote != "-:{Bundle_Item}")
            .Select(b => new SEItemViewModel
            {
                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemTax = b.ItemTax,
                itemNote = b.itemNote,
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.SEItemss
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.SalesEntry == id && ab.itemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList(),
                ItemList = (from ac in db.SalesEntrys
                            join bc in db.BatchStocks
                            on ac.SalesEntryId equals bc.Reference
                            where (ac.SalesEntryId == id && bc.Item == b.Item && bc.Type == "Sales")
                            select new batches
                            {
                                batch = bc.BatchNo
                            }).ToList(),

            }).ToList();
            vmodel.SEbs = db.SEBillSundrys.Where(a => a.SalesEntry == id)
         .Select(b => new SEBillSundryViewModel
         {
             AmountType = b.AmountType,
             BsAmount = b.BsAmount,
             BsType = b.BsType,
             BsValue = b.BsValue,
             Type = b.BsType == 0 ? "Add" : "Less",
             AmtType = b.AmountType == 0 ? "" : "%",
             BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
         }).ToList();

            var conv = db.ConvertTransactionss.Any(u => u.To == id);
            vmodel.ConvModel = null;
            if (conv)
            {
                List<string> ExList = new List<string>();
                List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                ExtList = ExtNumDetails((long)id, ExtList);
                var Extended = ExtList.Select(z => z.To).ToList();
                Int32 count = 0;


                var ConvModel = (from a in db.ConvertTransactionss
                                 join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                                 from b in primary.DefaultIfEmpty()
                                 where Extended.Contains(a.To)
                                 select new ConvertTransactionsViewModel
                                 {
                                     ConvertFrom = a.ConvertFrom,
                                     Id = b.SalesEntryId,
                                     BillNo = b.BillNo,
                                     CreatedDate = a.CreatedDate,
                                     From = a.From
                                 }).OrderBy(b => b.CreatedDate).ToList();

                var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                parentvm.BillNo = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                parentvm.Id = db.SalesEntrys.Where(x => x.SalesEntryId == parent).Select(y => y.SalesEntryId).FirstOrDefault();
                parentvm.ConvertFrom = "SaleExtend";
                ConvModel.Add(parentvm);
                vmodel.ConvModel = ConvModel.OrderBy(x => x.Id).ToList();

                var st = vmodel.ConvModel.Find(c => c.BillNo == vmodel.BillNo);
                vmodel.ConvModel.Remove(st);
            }
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Sales" && a.Status == Status.active).ToList();

            return View(vmodel);
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public ActionResult cancel(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            SalesEntry SEen = db.SalesEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesEntryId == id).FirstOrDefault();

            if (SEen == null)
            {
                return NotFound();
            }
            return PartialView(SEen);
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            SalesEntry SEen = db.SalesEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesEntryId == id).FirstOrDefault();

            if (SEen == null)
            {
                return NotFound();
            }
            return PartialView(SEen);
        }
        [RedirectingAction]
        [HttpPost, ActionName("Cancel")]
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        //[ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(long id)
        {
            bool stat = false;
            string msg;
  
            var sales = db.SalesEntrys.Find(id);
           
                var orgbillno = sales.BillNo;
                var billno = InvoiceNo();
                sales.BillNo = billno;
                db.Entry(sales).State = EntityState.Modified;
                db.SaveChanges();
            db.Journals.Where(o => o.InvoiceNo == orgbillno).ToList().ForEach(o => o.InvoiceNo = billno);
            db.SaveChanges();
            db.Payments.Where(o => o.InvoiceNo == orgbillno).ToList().ForEach(o => o.InvoiceNo = billno);
            db.SaveChanges();
            var count = db.SalesEntrys.Where(o => o.BillNo == orgbillno).Count();
            if (count == 0)
            {
                sales.BillNo = orgbillno;
                var seno = db.SalesEntrys.Select(o => o.SENo).Max() + 1;
                sales.ProTask = null;
                sales.SENo = seno;
                sales.SEGrandTotal = 0;
                sales.SEItemQuantity = 0;
                db.SalesEntrys.Add(sales);
                db.SaveChanges();
            }




            
            stat = true;
                msg = "Successfully Canceled Sales Entry.";


            
           return RedirectToAction("Index", "CreditSale");

        }
        [RedirectingAction]
        [HttpPost, ActionName("Delete")]
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                deleteproduction(id);
                msg = "Successfully deleted Sales Entry.";
            }

            #region Old Code

            ////var SET = db.AccountsTransactions.Where(a => a.Id == id).FirstOrDefault();
            #endregion            

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]
        public JsonResult salesref1(string oldname, string newname)
        {
            bool stat = false;
            string msg;

            msg = "Successfully deleted Sales Entry.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public ActionResult DeleteAllSales(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSale(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Sales Entry.", true);
            return RedirectToAction("Index", "CreditSale");
        }

        private Boolean DeleteSale(long saleId)
        {
            var Msg = chkDeleteWithMsg(saleId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return false;// DeleteFn(saleId);
            }
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "SaleExtend").FirstOrDefault();
            if (db.HireReturns.Any(u => u.Invoice == id))
            {
                msg = "Sales Entry Already used in Hire Return !!";
            }

            else if (db.SalesReturns.Any(u => u.SalesEntryId == id))
            {
                msg = "Sales Entry Already used in Sales Return !!";
            }
            else if (db.ReceiptBills.Any(u => u.InvoiceNo == id && u.Type == "Against Reference"))
            {
                msg = "Sales Entry Already used in Receipt Entry !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }

        public bool DeleteFn(long saleId)
        {
            var UserId = User.Identity.GetUserId();
            SalesEntry SEen = db.SalesEntrys.Find(saleId);
            var SEItem = db.SEItemss.Where(a => a.SEItemsId == saleId);
            var SEP = db.SEPayments.Where(a => a.SalesEntry == saleId).FirstOrDefault();
            var SEPT = db.SETransactions.Where(a => a.SalesEntry == saleId).ToList();
            var SEBs = db.SEBillSundrys.Where(a => a.SalesEntry == saleId).FirstOrDefault();
            var customerId = db.SalesEntrys.Where(a => a.SalesEntryId == saleId).Select(a => a.Customer).FirstOrDefault();

            var HireItem = db.HireDetails.Where(a => a.Reference == saleId && a.Section == "Sales").FirstOrDefault();

            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            /***************** Item Transaction ******************/
            if (SEen.MaterialCenter != null)
                com.ItemTransInDeleteMode("CreditSale", SEen.MaterialCenter, 0, 0, saleId, UserId, CurrentDate);

            if (SEItem != null)
            {
                db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == saleId));

            }
            if (SEBs != null)
            {
                db.SEBillSundrys.RemoveRange(db.SEBillSundrys.Where(a => a.SalesEntry == saleId));

            }
            if (SEP != null)
            {
                db.SEPayments.RemoveRange(db.SEPayments.Where(a => a.SalesEntry == saleId));
            }
            if (SEPT != null)
            {
                db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.SalesEntry == saleId));
            }

            var rec = db.Receipts.Where(a => a.Reference == saleId && a.RefType == "Sales").FirstOrDefault();
            if (rec != null)
            {
                db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == saleId && a.RefType == "Sales"));
            }

            var recbill = db.ReceiptBills.Where(a => a.InvoiceNo == saleId && a.BillType == "Sales" && a.Type == "Against Reference").ToList();
            if (recbill != null)
            {
                var recbillz = db.ReceiptBills.Where(a => a.InvoiceNo == saleId && a.BillType == "Sales" && a.Type == "Against Reference").ToList();
                recbillz.ForEach(a =>
                {
                    a.Type = "New Reference";
                    a.BillType = null;
                    a.InvoiceNo = null;
                });
                db.SaveChanges();
            }


            var ConSale = db.ConvertTransactionss.Where(a => a.To == saleId && a.ConvertTo == "Sale").FirstOrDefault();
            if (ConSale != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == saleId && a.ConvertTo == "Sale"));
            }
            if (HireItem != null)
            {
                db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == saleId && a.Section == "Sales"));
            }

            var appr = db.Approvals.Where(a => a.TransEntry == saleId && a.Type == "SalesEntry").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == saleId && a.Type == "SalesEntry"));
            }

            var app = db.ApprovalUpdates.Where(a => a.TransEntry == saleId && a.Type == "SalesEntry").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == saleId && a.Type == "SalesEntry"));
            }

            ////Delete Cheque Details




            db.SalesEntrys.Remove(SEen);


            // batch stock
            var SEBst = db.BatchStocks.Where(a => a.Reference == saleId && a.Type == "Sales").FirstOrDefault();
            if (SEBst != null)
            {
                db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == saleId && a.Type == "Sales"));
                db.SaveChanges();
            }

            ///*********** Delete from AttachmentDocuments Table *********************/

            ////List all the documents attached corresponding to the JournalId

            //    //To remove the attached file from folder
            //    string FullPath = LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/" + DocumentLists.ElementAt(i).FileName);


            //    //To remove the attached file from server
            ///***********************************************************************/


            bool delete = com.DeleteAllAccountTransaction("Sale", saleId);
            bool deletepay = com.DeleteAllAccountTransaction("Sale Payment", saleId);

            db.SaveChanges();


            var autopur = db.EnableSettings.Where(a => a.EnableType == "stockcheckinvoice").FirstOrDefault();
            var autopurcheck = autopur != null ? autopur.Status : Status.inactive;
            if (autopurcheck == Status.active)
            {
                var pur = db.PurchaseEntrys.Where(o => o.BillNo == SEen.BillNo).FirstOrDefault();
                db.PurchaseEntrys.RemoveRange(db.PurchaseEntrys.Where(o => o.BillNo == SEen.BillNo));
                db.SaveChanges();
                db.PEItemss.RemoveRange(db.PEItemss.Where(o => o.PurchaseEntry == pur.PurchaseEntryId));
                bool del = com.DeleteAllAccountTransaction("Purchase", pur.PurchaseEntryId);
            }
            com.addlog(LogTypes.Deleted, UserId, "SalesEntry", "SalesEntrys", findip(), SEen.SalesEntryId, "Successfully Deleted Sales Entry");

            return true;
        }
        public bool updateseprice(long salesid)
        {
            var today = System.DateTime.Now.AddMonths(-12);
            var v = (from a in db.SalesEntrys
                     join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                     where b.Type == true && b.ItemUnitPrice == 0
                     && a.SEDate >= today
                     select b
                   ).ToList();
            foreach (var se in v)
            {
                var sedate = db.SalesEntrys.Where(o => o.SalesEntryId == se.SalesEntry).Select(o => o.SEDate).FirstOrDefault();

                var pr = com.GetItemPurchasePriceoldnew(se.Item, sedate);

                if (pr > 0)
                {
                    var item = db.Items.Find(se.Item);
                    if (se.ItemUnit == item.ItemUnitID)
                    {
                        SEItems it = db.SEItemss.Find(se.SEItemsId);
                        it.ItemUnitPrice = pr;
                        db.Entry(it).State = EntityState.Modified;
                        db.SaveChanges();


                    }
                    else
                    {
                        if (item.ConFactor == 1)
                        {
                            SEItems it = db.SEItemss.Find(se.SEItemsId);
                            it.ItemUnitPrice = pr;
                            db.Entry(it).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            SEItems it = db.SEItemss.Find(se.SEItemsId);
                            it.ItemUnitPrice = pr / item.ConFactor;
                            db.Entry(it).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
            }
            return true;
        }

        public JsonResult GetTaskItems2(string taskid, long? salesid)
        {
          if(taskid=="")
            {
                return Json(0);
            }
            long[] cleanLongArray = taskid.Split(',')
           .Select(sValue => long.Parse(sValue.Trim())) // Trim whitespace and parse each to a long

           .ToArray();
            long[] stocktrasferid = { };

            foreach (var prid in cleanLongArray)
            {
                var taskcode = db.ProTasks.Where(o => o.ProTaskId == prid).Select(o => o.TaskCode).FirstOrDefault();
                var voucherno = "Field-Task " + taskcode;
             long[] stids = db.StockTransfers.Where(o => o.Voucher == voucherno).Select(o => o.Id).ToList().ToArray();
                stocktrasferid = stocktrasferid.Concat(stids).ToArray();
            }
            var mc = db.MCs.Where(o => o.MCName == "TASK CENTER").Select(o => o.MCId).SingleOrDefault();
            if (taskid != "")
            {
                var sedate = db.SalesEntrys.Where(o => o.SalesEntryId == salesid).Select(o => o.SEDate).FirstOrDefault();
                long[] addtaskar = taskid.Split(',').Select(Int64.Parse).ToArray();
                var existing = (from a in db.SalesEntrys
                                join b in db.additionaltasks on a.SalesEntryId equals b.salesentryid into cst
                                from b in cst.DefaultIfEmpty()
                                where (addtaskar.Contains((long)a.ProTask) || addtaskar.Contains((long)b.taskid))
                                &&
                                (salesid == null || a.SalesEntryId != salesid)
                                select new
                                {
                                    a.BillNo,

                                }).FirstOrDefault();

                if (existing != null)
                {
                    return Json(existing);

                }
                var data = (from a in db.ItemTask
                            join z in db.ItemTaskMasters on a.protaskid.ToString() equals z.TaskId.ToString()
                            join b in db.Items on a.ItemId equals b.ItemID
                            join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                            from c in primary.DefaultIfEmpty()
                            join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                            from d in second.DefaultIfEmpty()
                            join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                            from e in cat.DefaultIfEmpty()
                            join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                            from f in taxss.DefaultIfEmpty()
                            where addtaskar.Contains(a.protaskid)
                           
                            select new
                            {
                                a.ItemId,
                                a.Quantity,
                                a.Unit,
                                b.SellingPrice,
                                b.TaxID,
                                z.TaskMasterId,
                                a.TaskId,

                                b.ItemCode,
                                b.ItemName,
                                ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                b.ItemUnitID,
                                b.SubUnitId,

                                PriUnit = c.ItemUnitName,
                                SubUnit = d.ItemUnitName,
                                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                b.ItemID,
                                b.OpeningStock,
                                b.MinStock,
                                categoryname = e.ItemCategoryName,
                                Tax = f.Percentage,
                                //Price= transferprice,
                                b.PurchasePrice,
                                b.BasePrice,
                                b.MRP,
                                b.KeepStock,

                                //PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                                //SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                                //PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                                //SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                                //PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                                //SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                                //PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                                //SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                                a.protaskid

                            }).OrderBy(s => s.TaskId).AsEnumerable().Select(o => new
                            {
                                item = o.ItemId,
                                ItemQuantity = o.Quantity,
                                itemprqty=(o.Unit==o.ItemUnitID)?o.Quantity:o.Quantity/o.ConFactor,
                                availablstock = 0,//getstock(mc, o.ItemID, o.protaskid),
                                ItemUnit = o.Unit,
                                ItemUnitPrice = o.SellingPrice,
                                ItemTax = o.Tax,
                                totalprice=o.PurchasePrice* ((o.Unit == o.ItemUnitID) ? o.Quantity : o.Quantity / o.ConFactor),
                                ItemSubTotal = 0,
                                ItemTaxAmount = 0,
                                ItemDiscount = 0,
                                ItemTotalAmount = 0,
                                o.ItemID,
                                o.ItemCode,
                                o.ItemName,
                                o.ItemWithCode,
                                o.ItemUnitID,
                                o.SubUnitId,
                                
                                note = ' ',
                                PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                o.categoryname,
                                o.TaxID,
                                OpeningStock = o.OpeningStock,
                                MinStock = (o.MinStock != null) ? o.MinStock : 0,
                                o.ConFactor,
                                SellingPrice =o.PurchasePrice,//(o.SellingPrice != 0) ? o.SellingPrice : com.GetItemPurchasePriceoldnew(o.ItemID, sedate),
                                o.PurchasePrice,
                                o.BasePrice,
                                o.MRP,
                               // price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                                o.KeepStock,
                                o.protaskid,
                                batch = 0,

                                Type = 265,
                                o.TaskMasterId,
                  

                                prices = (from gg in db.itemtasklist
                                          join h in db.StockTransferItems on gg.itemid equals h.Item into stkit
                                          from h in stkit.DefaultIfEmpty()
                                          join i in db.StockTransfers on h.StockTransferId equals i.Id into sttr
                                          from i in sttr.DefaultIfEmpty()
                                          where addtaskar.Contains(gg.protaskid) && stocktrasferid.Contains(i.Id)
                                          select new
                                         {
                                             Price =(o.Unit ==o.SubUnitId && h.Price < 5 && o.ConFactor > 1) ? h.Price * o.ConFactor : h.Price,

                                             h.Item,
                                             h.Unit
                                         }).Where(p => p.Item == o.ItemId).Select(p => p.Price).Average(),

                            }).GroupBy(p=> new { p.item,p.ItemUnit},(key,g)=> new 
                            {
                                item = key.item,
                                ItemQuantity =g.Sum(o=>o.ItemQuantity),
                                itemprqty = g.Sum(o=>o.itemprqty),
                                availablstock = getstock(mc, key.item,g.FirstOrDefault().protaskid),// g.FirstOrDefault().availablstock,
                                ItemUnit =key.ItemUnit,
                                ItemUnitPrice =g.Average(o=>o.ItemUnitPrice),
                                ItemTax = g.FirstOrDefault().ItemTax,

                                ItemSubTotal = 0,
                                ItemTaxAmount = 0,
                                ItemDiscount = 0,
                                ItemTotalAmount = 0,
                                ItemID=g.FirstOrDefault().ItemID,
                                ItemCode = g.FirstOrDefault().ItemCode,
                                ItemName = g.FirstOrDefault().ItemName,
                                ItemWithCode = g.FirstOrDefault().ItemWithCode,
                                ItemUnitID = g.FirstOrDefault().ItemUnitID,
                                SubUnitId = g.FirstOrDefault().SubUnitId,
                                note = ' ',
                                PriUnit =  g.FirstOrDefault().PriUnit,
                                SubUnit = g.FirstOrDefault().SubUnit,
                                categoryname = g.FirstOrDefault().categoryname,
                                TaxID = g.FirstOrDefault().TaxID,
                                OpeningStock = g.FirstOrDefault().ItemID,
                                MinStock = g.FirstOrDefault().MinStock,
                                ConFactor = g.FirstOrDefault().ConFactor,
                                SellingPrice = g.FirstOrDefault().prices,
                                PurchasePrice = g.FirstOrDefault().prices,
                                BasePrice = g.FirstOrDefault().BasePrice,
                                MRP = g.FirstOrDefault().MRP,
                                price  = g.FirstOrDefault().ItemID,
                                KeepStock = g.FirstOrDefault().KeepStock,

                                batch = 0,

                                Type = 265,
                                TaskMasterId = g.FirstOrDefault().TaskMasterId,


                            }).ToList();
                

                return Json(data);

            }
            else
            {
                return Json(0);
            }






        }
        public JsonResult GetTaskItemspurchase(long taskid)
        {

            //            where b.TaskId == taskid
            //                c.ItemID,
            //                c.ItemUnit,
            //                c.ItemName,
            //                a.Quantity,
            //                a.Unit,
            //                a.ItemId,
            //                c.SellingPrice,
            //                c.ItemUnitID


            var data = (from a in db.PEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        where a.TaskId == taskid

                        select new
                        {
                            a.Item,
                            a.ItemQuantity,
                            a.ItemUnit,
                            b.SellingPrice,
                            b.TaxID,
                            TaskMasterId = 1,
                            a.TaskId,

                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,

                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,

                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,



                        }).OrderBy(s => s.TaskId).AsEnumerable().Select(o => new
                        {
                            item = o.Item,
                            ItemQuantity = o.ItemQuantity,
                            ItemUnit = o.ItemUnit,
                            ItemUnitPrice = o.SellingPrice,
                            ItemTax = o.Tax,

                            ItemSubTotal = 0,
                            ItemTaxAmount = 0,
                            ItemDiscount = 0,
                            ItemTotalAmount = 0,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            note = ' ',
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.TaxID,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                            subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),

                            batch = 0,

                            Type = 265,
                            o.TaskMasterId


                        }).ToList();

            return Json(data);







        }
        public JsonResult GetTaskItems(long taskid)
        {
            var mc = db.MCs.Where(o => o.MCName == "TASK CENTER").Select(o => o.MCId).SingleOrDefault();

            //            where b.TaskId == taskid
            //                c.ItemID,
            //                c.ItemUnit,
            //                c.ItemName,
            //                a.Quantity,
            //                a.Unit,
            //                a.ItemId,
            //                c.SellingPrice,
            //                c.ItemUnitID


            var data = (from a in db.ItemTask
                        join z in db.ItemTaskMasters on a.protaskid.ToString() equals z.TaskId.ToString()
                        join b in db.Items on a.ItemId equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        where a.protaskid == taskid

                        select new
                        {
                            a.ItemId,
                            a.Quantity,
                            a.Unit,
                            b.SellingPrice,
                            b.TaxID,
                            z.TaskMasterId,
                            a.TaskId,

                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,

                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,

                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            a.protaskid,


                        }).OrderBy(s => s.TaskId).AsEnumerable().Select(o => new
                        {
                            item = o.ItemId,
                            ItemQuantity = o.Quantity,
                            ItemUnit = o.Unit,
                            ItemUnitPrice = o.SellingPrice,
                            ItemTax = o.Tax,

                            availablstock = getstock(mc, o.ItemID, o.protaskid),
                            ItemSubTotal = 0,
                            ItemTaxAmount = 0,
                            ItemDiscount = 0,
                            ItemTotalAmount = 0,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            note = ' ',
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.TaxID,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                            subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),

                            batch = 0,

                            Type = 265,
                            o.TaskMasterId


                        }).ToList();

            return Json(data);







        }
       public decimal getstock(long mc,long itemid,long protaskid)
        {
            var salesid = db.SalesEntrys.Any(o => o.ProTask == protaskid);
            if (!salesid)
            {
                salesid = db.additionaltasks.Any(o => o.taskid == protaskid);
                }
            
            if(!salesid)
            {
                return (decimal)com.GetItemWisestock(itemid, mc);
            }
            else
            {
                var salesentryid = db.SalesEntrys.Where(o => o.ProTask == protaskid).Select(o=>o.SalesEntryId).FirstOrDefault();
                if (salesentryid==null||salesentryid==0)
                {
                    salesentryid = db.additionaltasks.Where(o => o.taskid == protaskid).Select(o => o.salesentryid).FirstOrDefault();

                }
                var seit = db.SEItemss.Where(o => o.SalesEntry == salesentryid&&o.Item==itemid);
                decimal salestock = 0;
                foreach (var st in seit)
                {
                   
                    var it = db.Items.Find(st.Item);
                    if(st.ItemUnit==it.ItemUnitID)
                    {
                        salestock = salestock + st.ItemQuantity;
                    }
                    else
                    {
                        salestock = salestock + st.ItemQuantity/it.ConFactor;
                    }
                }
                return (decimal)com.GetItemWisestock(itemid, mc)+salestock;


            }
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download Sales Entry")]
        public ActionResult Download(long id)
        {
            return RedirectToAction("Edit/"+id, "CreditSale");
            var SaleDet = db.SalesEntrys.Where(s => s.SalesEntryId == id).FirstOrDefault();
            var custname = db.Customers.Where(s => s.CustomerID == SaleDet.Customer).Select(a => a.CustomerName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = SaleDet.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Tax Invoice" + "-" + custname + "-" + billno + ".pdf");

        }
        public StringBuilder generatePdf(long id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
            var HideItmName = db.EnableSettings.Where(a => a.EnableType == "HideItemName").FirstOrDefault();
            var HideItmNameIfDiscriptionOn = HideItmName != null ? HideItmName.Status : Status.inactive;
            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var saleData = com.SaleData(id, InPrintItemCode, HideItmNameIfDiscriptionOn, PartNoCheck, TimeOut);
            var item = saleData.pdfItem.ToList();
            var summary = saleData;
            var billsundry = saleData.billsundry.ToList();


            return com.generatepdf2(id, summary, item, billsundry, "Sale");

        }




        //                   where a.SalesEntryId == id
        //                       PartyName = b.CustomerName,
        //                       BillNo = a.BillNo,
        //                       Date = a.SEDate,
        //                       Note = a.SENote,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       Discount = a.SEDiscount,

        //                       GrandTotal = a.SEGrandTotal,
        //                       Paid = d.SEPaidAmount,
        //                       Balance = a.SEGrandTotal - d.SEPaidAmount,
        //                       SubTotal = a.SESubTotal,
        //                       TaxAmount = a.SETaxAmount,
        //                       c.Address,
        //                       c.City,
        //                       c.State,
        //                       c.Country,
        //                       c.Zip,
        //                       Email = c.EmailId,
        //                       Phone = c.Phone,
        //                       Mobile = c.Mobile,
        //                       TRN = b.TaxRegNo,
        //                       TermsCondition = a.SENote,
        //                       BillId = a.SENo,
        //                       a.PONo,
        //                       a.ConvertType,
        //                       a.ConvertNo,
        //                       a.Location,
        //                       a.Remarks,
        //                       Currency = a.Currency,
        //                       ConvertionRate = a.ConvertionRate,
        //                       FCTotal = a.FCTotal,

        //                    where b.SalesEntry == id && b.itemNote != "-:{Bundle_Item}"
        //                        ItemUnitPrice = b.ItemUnitPrice,
        //                        ItemQuantity = b.ItemQuantity,
        //                        ItemSubTotal = b.ItemSubTotal,
        //                        ItemNote = b.itemNote,
        //                        ItemTax = b.ItemTax,
        //                        ItemTaxAmount = b.ItemTaxAmount,
        //                        ItemTotalAmount = b.ItemTotalAmount,
        //                        ItemID = b.Item,
        //                        bundleitem = (from ab in db.SEItemss
        //                                      where ab.SalesEntry == id && ab.itemNote == "-:{Bundle_Item}"
        //                                      && b.Item == ab.ItemDiscount

        //                                          bb.ItemCode,
        //                                          bb.ItemName,
        //                                          cb.ItemUnitName,
        //                                          ItemUnitPrice = ab.ItemUnitPrice,
        //                                          quantity = ab.ItemQuantity,
        //                                          ItemSubTotal = ab.ItemSubTotal,
        //                                          ItemTax = ab.ItemTax,
        //                                          ItemTaxAmount = ab.ItemTaxAmount,
        //                                          ItemTotalAmount = ab.ItemTotalAmount,

        //                                          ab.Item,
        //                                          ab.ItemQuantity,
        //                                          ab.ItemUnit,

        //                                          ItemDiscount = 0,

        //                                          ItemNote = ab.itemNote,
        //                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                          bb.ItemUnitID,
        //                                          bb.SubUnitId,
        //                                          PriUnit = cb.ItemUnitName,
        //                                          SubUnit = bd.ItemUnitName,
        //                                          bb.ItemArabic
        //                                      }).ToList()

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





        //            //if (layName == "Scaffold")
        //            //    string partyDetails = "<table style='border:1px #ccc;' width='100%'><tr style='border-top:.1px #ccc;'> " +
        //            //        "<td width='50%'>" +
        //            //        "<table style='border: 0px;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table>" +
        //            //        "</td>" +
        //            //        "<td width='50%'>" +
        //            //        "<table>" +
        //            //        "<tr>" +
        //            //        //"<td width='15%' style='border:1px solid #000 !important;padding:4px;'>Invoice No:</td>" +
        //            //        //"<td width='15%' style='border:1px solid #000 !important;padding:4px;'><b><label style='font-size:large;font-weight:600;'>" + details.BillNo + "</label></b></td>" +
        //            //        //"<td width='5%' style='border:1px solid #000 !important;padding:4px;'>Date</td>" +

        //            //        "<td style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Invoice No:</td>" +
        //            //        "<td style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>aa</td>" +
        //            //        "<td style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Date</td>" +
        //            //        "<td style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>aa</td>" +

        //            //        //"<td width='20%' style='border:1px solid #000 !important;padding:4px;'>" + details.Date.ToString("dd-MM-yyyy") + "</td>" +
        //            //        "</tr>"+
        //            //        "</table>" +
        //            //        "</td>" +
        //            //    //"<tr><td style='border: 1px solid #000 !important;padding: 4px;'>Sales Executive</td>" +
        //            //    //"<td colspan='3' style='border: 1px solid #000 !important;padding:4px;'>" + details.Cashier + "</td></tr>" +

        //            //    //"<tr><td colspan ='2' style='border:1px solid #000 !important;padding: 4px;'>LPO No.</td>" +
        //            //    //"<td colspan ='2' style ='border:1px solid #000 !important;padding: 4px;'>"+ details.Cashier + "</td></tr>" +
        //            //    //"<tr><td colspan ='2' style ='border:1px solid #000 !important;padding: 4px;'> PAYMENT TERMS </td>" +
        //            //    //"<td colspan ='2' style ='border:1px solid #000 !important;padding: 4px;'></td></tr>";


        //            //else
        //                "<td width='50%'> " +
        //                "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +







        //                    sb.Append("<img width='40px' height='70px' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + item.ItemID + "/" + item.FileName) + "'/>");













        [HttpGet]
        public JsonResult GetSEItems(long SalesEntryID)
        {
            var mc = db.SalesEntrys.Where(o => SalesEntryID == SalesEntryID).Select(o => o.MC).FirstOrDefault();
            var prid = db.SalesEntrys.Where(o => o.SalesEntryId == SalesEntryID).Select(o => o.pricecategoryid).FirstOrDefault();
            decimal? perc = 0;
            if (prid != null)
            {
                perc = db.PriceCategories.Where(o => o.pricestratagyid == prid).Select(o => o.value).FirstOrDefault();

                if (perc == null)
                {
                    perc = 0;
                }

            }
            var overridecost = User.IsInRole("Override Selling Price Below Cost");
            var ConD = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.SalesEntrys on a.SalesEntry equals g.SalesEntryId
                        where a.SalesEntry == SalesEntryID && a.itemNote != "-:{Bundle_Item}"
                        && a.Type != true
                        && (mc == 0 || mc == null || g.MaterialCenter == mc)
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
                            a.ItemTotalAmount,
                            note = a.itemNote.Replace("<br />", "\n"),

                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            BasePrice = b.PurchasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.slreq,
                            b.PricingStrategy,
                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            percentage = perc,
                            batch = (from ay in db.BatchStocks
                                     join az in db.SEItemss on new { f1 = ay.Item, f2 = ay.Unit, f3 = ay.Reference, f4 = ay.Type }
                                           equals new { f1 = az.Item, f2 = az.ItemUnit, f3 = az.SalesEntry, f4 = "Sales" }
                                     where az.SalesEntry == a.SalesEntry && ay.Item == a.Item



                                     //where az.SaleEntryId == SalesEntryID && ay.Type == "Sales" 


                                     select new BatchStockPViewModel
                                     {
                                         BatchNo = ay.BatchNo,
                                         MFGd = ay.MFG,
                                         EXPd = ay.EXP,
                                         StockIn = ay.StockIn,
                                         StockOut = ay.StockOut,
                                         Item = ay.Item,
                                         cfactor = b.ConFactor,
                                         Priunit = b.ItemUnitID,
                                         Secunit = b.SubUnitId,
                                         Unit = ay.Unit,
                                         Cost = ay.Cost,
                                         origin = "Sales",
                                         Order = ay.Order
                                     }).ToList(),
                            rack = (from ay in db.shelfstockmovements
                                    join az in db.SEItemss on new { f1 = ay.itemid, f2 = ay.unitid, f3 = ay.referenceid, f4 = ay.purpose }
                                     equals new { f1 = az.Item, f2 = (long)az.ItemUnit, f3 = az.SalesEntry, f4 = "Sales" }
                                    join aa in db.rackmaterialcentres on ay.rackmciid equals aa.rackmcid
                                    join b in db.Shelves on aa.shelfid equals b.ShelfId
                                    join c in db.Racks on aa.rackid equals c.RackId

                                    where az.SalesEntry == a.SalesEntry && ay.itemid == a.Item
                                    select new
                                    {
                                        ShelfNo = aa.shelfid,
                                        RackNo = aa.rackid,
                                        ShelfName = b.shelfName,
                                        RackName = c.RackName,
                                        StockIn = ay.qty,
                                    }).ToList()

                        }).ToList().Select(o => new
                        {
                            o.Item,
                            o.percentage,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.PricingStrategy,
                            BasePrice = (overridecost == true) ? 0 : o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.slreq,
                            o.rack,
                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                            subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),

                            o.batch
                        }).ToList();
            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetSEItemsorg(long SalesEntryID)
        {
            var mc = db.SalesEntrys.Where(o => SalesEntryID == SalesEntryID).Select(o => o.MC).FirstOrDefault();
            var prid = db.SalesEntrys.Where(o => o.SalesEntryId == SalesEntryID).Select(o => o.pricecategoryid).FirstOrDefault();
            decimal? perc = 0;
            if (prid != null)
            {
                perc = db.PriceCategories.Where(o => o.pricestratagyid == prid).Select(o => o.value).FirstOrDefault();

                if (perc == null)
                {
                    perc = 0;
                }

            }
            var overridecost = User.IsInRole("Override Selling Price Below Cost");
            var ConD = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID

                        where a.SalesEntry == SalesEntryID
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
                            a.ItemTotalAmount,
                            note = a.itemNote.Replace("<br />", "\n"),
                            b.ItemID,
                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnitPrice,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.note,
                        }).ToList();
            return Json(ConD);
        }



        [HttpGet]
        public JsonResult GetSEItemsNotSR(long SalesEntryID)
        {
            var sritems = (from g in db.SRItemss
                           join h in db.SalesReturns on g.SalesReturnId equals h.SalesReturnId
                           where h.SalesEntryId == SalesEntryID
                           select g.Item).ToList();
            var ConD = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()

                        where a.SalesEntry == SalesEntryID && a.itemNote != "-:{Bundle_Item}"
                        && a.Type != true && !sritems.Contains(a.Item)
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
                            a.ItemTotalAmount,
                            note = a.itemNote.Replace("<br />", "\n"),

                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.slreq,

                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            batch = (from ay in db.BatchStocks
                                     join az in db.SalesEntrys on ay.Reference equals az.SalesEntryId into abc
                                     from az in abc.DefaultIfEmpty()

                                     where az.SalesEntryId == SalesEntryID && ay.Type == "Sales" && b.ItemID == ay.Item


                                     select new BatchStockPViewModel
                                     {
                                         BatchNo = ay.BatchNo,
                                         MFGd = ay.MFG,
                                         EXPd = ay.EXP,
                                         StockIn = ay.StockIn,
                                         StockOut = ay.StockOut,
                                         Item = ay.Item,
                                         cfactor = b.ConFactor,
                                         Priunit = b.ItemUnitID,
                                         Secunit = b.SubUnitId,
                                         Unit = ay.Unit,
                                         Cost = ay.Cost,
                                         origin = "Sales",
                                         Order = ay.Order
                                     }).ToList()
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.slreq,
                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                            subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),

                            o.batch
                        }).ToList();
            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetSEItemsHire(long SalesEntryID)
        {
            var v = (from a in db.SEItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                     from f in taxss.DefaultIfEmpty()
                     join g in db.SalesEntrys on a.SalesEntry equals g.SalesEntryId
                     where a.SalesEntry == SalesEntryID && a.itemNote != "-:{Bundle_Item}"
                     && g.SaleType == SaleType.Hire //&& b.KeepStock == true
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
                         a.ItemTotalAmount,
                         note = a.itemNote.Replace("<br />", "\n"),

                         b.ItemCode,
                         b.ItemName,
                         ItemWithCode = b.ItemCode + " - " + b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         b.OpeningStock,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         Tax = f.Percentage,
                         b.SellingPrice,
                         b.PurchasePrice,
                         b.BasePrice,
                         b.MRP,
                         b.KeepStock,

                         RetItemQuantity = (decimal?)(from aa in db.HrItems
                                                      join bb in db.HireReturns on aa.Hr equals bb.HireReturnId
                                                      where aa.Item == a.Item && bb.Invoice == SalesEntryID
                                                      && aa.ItemNote != "-:{Bundle_Item}"
                                                      select new
                                                      {
                                                          aa.ItemQuantity
                                                      }).Sum(a => a.ItemQuantity) ?? 0,

                         DvItemQuantity = (decimal?)(from ab in db.SEItemss
                                                     join ba in db.SalesEntrys on ab.SalesEntry equals ba.SalesEntryId
                                                     where ab.Item == a.Item && ba.SaleType == SaleType.Hire
                                                     && ba.SalesEntryId == SalesEntryID
                                                     && ab.itemNote != "-:{Bundle_Item}"
                                                     select new
                                                     {
                                                         ab.ItemQuantity
                                                     }).Sum(a => a.ItemQuantity) ?? 0,

                     }).ToList();

            var ConD = (from o in v
                        select new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,

                            RetItemQuantity = o.RetItemQuantity,
                            DvItemQuantity = o.DvItemQuantity,

                            bundle = (from ab in db.SEItemss
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                      from dd in scaffold.DefaultIfEmpty()

                                      join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                      from eb in bpunit.DefaultIfEmpty()

                                      let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                      where ab.SalesEntry == SalesEntryID && ab.itemNote == "-:{Bundle_Item}"
                                      && o.ItemID == ab.ItemDiscount
                                      select new
                                      {
                                          Id = bb.ItemID,
                                          ItemUnitPrice = ab.ItemUnitPrice,
                                          ItemQuantity = ab.ItemQuantity,
                                          ItemSubTotal = ab.ItemSubTotal,
                                          ItemDiscount = ab.ItemDiscount,
                                          ItemNote = "",
                                          ItemTax = ab.ItemTax,
                                          ItemTaxAmount = ab.ItemTaxAmount,
                                          ItemTotalAmount = ab.ItemTotalAmount,

                                          ItemCode = bb.ItemCode,
                                          ItemName = bb.ItemName,
                                          ItemUnit = eb.ItemUnitName,
                                          PartNumber = bb.PartNumber,
                                          PNoStatus = "",
                                          CBM = dd.CBM,
                                          Weight = dd.Weight,
                                          img = bimg,
                                          note = ab.itemNote.Replace("<br />", "\n"),

                                          KeepStock = bb.KeepStock,

                                          ab.Item,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          bb.ItemArabic,
                                          bb.ItemDescription,
                                          BaseQty = (ab.ItemQuantity / o.ItemQuantity),

                                          RetItemQuantity = (decimal?)(from xx in db.HrItems
                                                                       join yy in db.HireReturns on xx.Hr equals yy.HireReturnId
                                                                       where xx.ItemNote == "-:{Bundle_Item}"
                                                                       && yy.Invoice == SalesEntryID
                                                                       && ab.Item == xx.Item
                                                                       && xx.ItemDiscount == ab.ItemDiscount
                                                                       select new
                                                                       {
                                                                           xx.ItemQuantity
                                                                       }).Sum(a => a.ItemQuantity) ?? 0,

                                          DvItemQuantity = (decimal?)(from xx in db.SEItemss
                                                                      join yy in db.SalesEntrys on xx.SalesEntry equals yy.SalesEntryId
                                                                      where xx.itemNote == "-:{Bundle_Item}"
                                                                      && yy.SalesEntryId == SalesEntryID
                                                                      && ab.Item == xx.Item
                                                                      && xx.ItemDiscount == ab.ItemDiscount
                                                                      select new
                                                                      {
                                                                          xx.ItemQuantity
                                                                      }).Sum(a => a.ItemQuantity) ?? 0,


                                      }).ToList(),

                        }).ToList();

            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetCommission(long SalesEntryID)
        {
            var SEBs = (from a in db.commissions
                        join c in db.SalesEntrys on a.salesid equals c.SalesEntryId
                        join d in db.Employees on a.agent equals d.EmployeeId
                        where a.salesid == SalesEntryID
                        select new
                        {
                            a.agent,
                            a.commisionmode,
                            a.commisiontype,
                            a.comvalue,
                            d.FirstName

                        }).ToList();
            return Json(SEBs);
        }

        [HttpGet]
        public JsonResult GetSEBillSundry(long SalesEntryID)
        {
            var SEBs = (from a in db.SEBillSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.SalesEntry == SalesEntryID
                        select new
                        {
                            a.AmountType,
                            a.BillSundry,
                            BsAmount = (a.BsAmount == null) ? 0 : a.BsAmount,
                            a.BsType,
                            BsValue = (a.BsValue == null) ? 0 : a.BsValue,
                            //a.PEBillSundryId,
                            //a.PurchaseEntry,
                            c.BSName
                            //c.BillSundryId
                        }).ToList();
            return Json(SEBs);
        }

        public JsonResult SearchSaleEntry(string q, string page, int stype)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                if (stype == 1)
                {

                    serialisedJson = db.SalesEntrys.Where(p =>  p.Status == 1 && (p.BillNo.ToLower().StartsWith(q.ToLower()) || p.BillNo.StartsWith(q)))
                            .Select(b => new SelectFormat
                            {
                                text = b.BillNo, //each json object will have 
                                id = b.SalesEntryId
                            }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    serialisedJson = db.SalesEntrys.Where(p => p.SaleType == SaleType.Hire && p.Status == 1 && (p.BillNo.ToLower().StartsWith(q.ToLower()) || p.BillNo.StartsWith(q)))
                           .Select(b => new SelectFormat
                           {
                               text = b.BillNo, //each json object will have 
                               id = b.SalesEntryId
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                }
            }
            else
            {
                if (stype == 1)
                {
                    serialisedJson = db.SalesEntrys.Where(p => p.Status == 1).Select(b => new SelectFormat
                    {
                        text = b.BillNo, //each json object will have 
                        id = b.SalesEntryId
                    }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    serialisedJson = db.SalesEntrys.Where(p => p.SaleType == SaleType.Hire && p.Status == 1).Select(b => new SelectFormat
                    {
                        text = b.BillNo, //each json object will have 
                        id = b.SalesEntryId
                    }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                }
            }
            return Json(serialisedJson);
        }

        [HttpGet]
        public JsonResult GetSalesEntryById(int saleId)
        {
            bool stat = true;
            string msg;
            SalesEntry sEntry = new SalesEntry();
            sEntry.CustomerType = db.SalesEntrys.Where(a => a.SalesEntryId == saleId).Select(a => a.CustomerType).FirstOrDefault();
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.srdays).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
                editableDay = today.AddYears(-10);
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddMinutes(-userEditDays);
            }
            var idd = Convert.ToInt64(saleId);
            SalesEntry sr = db.SalesEntrys.Where(o => o.SalesEntryId == idd).FirstOrDefault();
            if (sr != null)
            {
                if ((sr.SEDate - editableDay).TotalMinutes < 0 || tem == 1)
                {
                    msg = "Edit Days Over";
                    stat = false;
                    //   return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }

            }






            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                     join d in db.MCs on a.MaterialCenter equals d.MCId into mcst
                     from d in mcst.DefaultIfEmpty()
                     where a.SalesEntryId == saleId
                     select new
                     {
                         CustomerName = b.CustomerCode + "-" + b.CustomerName,
                         a.Customer,
                         a.CustomerType,
                         a.SEDiscount,
                         a.SEGrandTotal,
                         a.MaterialCenter,
                         MaterialCenterName = d.MCName,
                         c.SEPaidAmount,
                         SEDueAmount = a.SEGrandTotal - c.SEPaidAmount,
                         valid = stat,
                         editableDay = userEditDays

                     }).FirstOrDefault();
            return Json(v);
        }
        
        private long GetSeNo(string invoicetype)
        {
            long type = 1;
            if (invoicetype == "2")
                type = 2;
       
            Int64 SENo = 0;
            string prefix = (type == 1) ? "Invoice" : "TaxExempt";
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if (type == 1)
            {
                if ((db.SalesEntrys.Where(a =>
                a.SaleType == SaleType.Sale).Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    SENo = (number == 0) ? 1 : number;
                }
                else
                {
                    SENo = db.SalesEntrys.Where(a => a.SaleType == SaleType.Sale).Max(p => p.SENo + 1);
                }
            }
            else
            {
                if ((db.SalesEntrys.Where(a =>
                               a.SalesType == 2).Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    SENo = (number == 0) ? 1 : number;
                }
                else
                {
                    SENo = db.SalesEntrys.Where(a => a.SalesType == 2).Max(p => p.SENo + 1);
                }
            }

            return SENo;
        }
        private string InvoiceNo(Int64 SENo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == null) ? "Invoice" : "TaxExempt";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            SaleType type = (section != "TaxExempt") ? SaleType.Sale : SaleType.Hire;
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
                Int64 num = 0;
                if (section == "TaxExempt")
                {
                    num = db.SalesEntrys.Where(q => q.SalesType == 2).Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max();

                }
                else
                {
                    num = db.SalesEntrys.Where(q => q.SaleType == type).Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max();

                }
                if (num == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    if (section == "TaxExempt")
                    {
                        SENo = db.SalesEntrys.Where(q => q.SalesType == 2).Max(p => p.SENo + 1);

                    }
                    else
                    {
                        SENo = db.SalesEntrys.Where(q => q.SaleType == type).Max(p => p.SENo + 1);

                    }
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo, section))
                    {
                        billNo = InvoiceNo(SENo, billNo, section);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo,section))
                {
                    billNo = InvoiceNo(SENo, billNo, section);
                }

            }
            return billNo;
        }
        [HttpGet]
        private string InvoiceNotaxexcept(Int64 SENo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == "Hire") ? "HireInvoice" : "Invoice";
            prefix = "TaxExempt";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            long type = 2;
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
                Int64 num = db.SalesEntrys.Where(q => q.SalesType == type).Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max();
                if (num == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.SalesEntrys.Where(q => q.SalesType == type).Max(p => p.SENo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNotaxexcept(SENo, billNo, section);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNotaxexcept(SENo, billNo, section);
                }

            }
            return billNo;
        }

        private bool BillExist(string SENo,string section="")
        {
            if (section != "TaxExempt")
            {
                var Exists = db.SalesEntrys.Any(c => c.BillNo == SENo);
                bool res = (Exists) ? true : false;
                return res;
            }
            else
            {
                var Exists = db.SalesEntrys.Where(q => q.SalesType == 2).Any(c => c.BillNo == SENo);
                bool res = (Exists) ? true : false;
                return res;
            }
        
        }
        [HttpGet]
        public JsonResult chkBillExist(string SENo)
        {
            var Exists = db.SalesEntrys.Any(c => c.BillNo == SENo);
            bool res = (Exists) ? true : false;
            return Json(res);
        }


        public JsonResult SearchInvoiceSale(string q, long accId)
        {
            var custId = db.Customers.Where(a => a.Accounts == accId).Select(a => a.CustomerID).FirstOrDefault();
            object serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var sale = (from a in db.SalesEntrys
                            join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                            //let camt = db.CNInvoices.Where(m => m.EntryNo == a.SalesEntryId).Sum(m => m.CreditAmount)
                            where //!cninvoice.Contains(a.BillNo) && 
                            (a.BillNo.ToLower().Contains(q.ToLower()) || a.BillNo.Contains(q)) && a.Customer == custId
                            //&& (camt == null || (a.SEGrandTotal != camt || camt < c.SEBillAmount))
                            && ((c.SEBillAmount > (c.SEPaidAmount + (c.CreditAmount != null ? c.CreditAmount : 0))))
                            select new
                            {
                                text = a.BillNo, //each json object will have 
                                id = a.BillNo//SalesEntryId
                            }).OrderBy(b => b.text).ToList();

                serialisedJson = sale;
            }
            else
            {
                var sale = (from a in db.SalesEntrys
                            join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                            //let camt = db.CNInvoices.Where(m => m.EntryNo == a.SalesEntryId).Sum(m => m.CreditAmount)
                            where //!cninvoice.Contains(a.BillNo) && 
                            a.Customer == custId
                            //&& (camt == null || (a.SEGrandTotal != camt || camt < c.SEBillAmount))
                            && ((c.SEBillAmount > (c.SEPaidAmount + (c.CreditAmount != null ? c.CreditAmount : 0))))
                            select new
                            {
                                text = a.BillNo, //each json object will have 
                                id = a.BillNo//SalesEntryId
                            }).OrderBy(b => b.text).ToList();

                serialisedJson = sale;
            }
            return Json(serialisedJson);
        }

        [HttpGet]
        public JsonResult GetSalesById(string billNo)
        {
            var Cnenable = db.EnableSettings.Where(a => a.EnableType == "ContinuesInvoiceNo").FirstOrDefault();
            var Continues = Cnenable != null ? Cnenable.Status : Status.inactive;
            object sale = null;
            object v = null;
            var entryNo = db.SalesEntrys.Where(a => a.BillNo == billNo).Select(a => a.SalesEntryId).FirstOrDefault();
            decimal paidamt = (decimal?)db.CNInvoices.Where(a => a.EntryNo == entryNo).Sum(a => a.CreditAmount) ?? 0;

            sale = db.SalesEntrys.Where(a => a.SalesEntryId == entryNo).FirstOrDefault();
            if (sale != null)
            {
                v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry
                     where a.SalesEntryId == entryNo
                     select new
                     {
                         SeEntry = a.SalesEntryId,
                         InvoiceNo = a.BillNo,
                         CustomerName = b.CustomerCode + "-" + b.CustomerName,
                         a.Customer,
                         a.CustomerType,
                         GrandTotal = a.SEGrandTotal,
                         TaxAmount = a.SETaxAmount,
                         Tax = a.SETax,
                         SubTotal = a.SESubTotal,
                         NoteType = "sale",
                         InvoiceDate = a.SEDate,
                         CreditAmount = d.CreditAmount,
                         PaidAmount = d.SEPaidAmount,//+ paidamt
                         BalanceAmt = ((decimal?)a.SEGrandTotal ?? 0) - (((decimal?)d.SEPaidAmount ?? 0) + ((decimal?)d.CreditAmount ?? 0)),//+ paidamt
                     }).FirstOrDefault();
            }
            return Json(v);
        }

        public Int32 chkInCompleteTransaction()
        {
            var entry = (from a in db.SalesEntrys
                         join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry into pay
                         from c in pay.DefaultIfEmpty()
                         where c.SalesEntry == null
                         select new
                         {
                             a.SalesEntryId
                         }).ToList();

            foreach (var entryid in entry)
            {
            }


            return 0;
        }

        [HttpPost]
        public ActionResult GetHireInvoiceNum(string hiretype)
        {
            string hirerate = (hiretype == "Hire") ? InvoiceNo(0, null, hiretype) : InvoiceNo();
            if (hiretype == "TaxExempt")
            {
                hirerate = InvoiceNotaxexcept();
            }
            return Json(hirerate);
        }
        [HttpPost]
        public ActionResult GetInvoiceNum(string hiretype)
        {
            string hirerate = (hiretype == "Hire") ? InvoiceNo(0, null, hiretype) : InvoiceNo();

            hirerate = InvoiceNo();

            return Json(hirerate);
        }

        public JsonResult SearchHireEntry(string q, long cust, long? project)
        {
            object serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                serialisedJson = (from a in db.SalesEntrys
                                  let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.From == a.SalesEntryId).Select(x => x.From).FirstOrDefault()
                                  where (a.SaleType == SaleType.Hire) && (project == null || project == 0 || a.Project == project) && a.Customer == cust && (a.BillNo.ToLower().Contains(q.ToLower()) || a.BillNo.Contains(q)) && (a.SalesEntryId != chkextend)
                                  select new
                                  {
                                      id = a.SalesEntryId,
                                      text = a.BillNo
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.SalesEntrys
                                  let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.From == a.SalesEntryId).Select(x => x.From).FirstOrDefault()
                                  where (a.SaleType == SaleType.Hire) && (project == null || project == 0 || a.Project == project) && a.Customer == cust && (a.SalesEntryId != chkextend)
                                  select new
                                  {
                                      id = a.SalesEntryId,
                                      text = a.BillNo

                                  }).OrderBy(b => b.text).ToList();
            }
            return Json(serialisedJson);
        }

        public List<ConvertTransactions> ExtNum(long id, List<ConvertTransactions> ExtList)
        {
            ConvertTransactions Ext = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.To == id).FirstOrDefault();
            if (Ext != null)
            {
                ExtList.Add(Ext);
                ExtNum(Ext.From, ExtList);
            }
            return ExtList;
        }
        public List<ConvertTransactions> ExtNumDetails(long id, List<ConvertTransactions> ExtList)
        {
            ConvertTransactions Ext = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.To == id).FirstOrDefault();
            if (Ext != null)
            {

                ExtList.Add(Ext);
                ExtNumDetails(Ext.From, ExtList);
            }
            else if (Ext == null)
            {
                ConvertTransactions st = new ConvertTransactions();
                st.To = id;
                st.From = 0;
                ExtList.Add(st);
            }

            return ExtList;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "SalesEntry" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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
            var MR = db.SalesEntrys.Where(a => a.SalesEntryId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => (a.ApprovedBy == UserId) && (a.TransEntry == id) && (a.Type == "SalesEntry")).OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = MR.CreatedBy;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "SalesEntry";

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
                            join d in db.SalesEntrys on b.TransEntry equals d.SalesEntryId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "SalesEntry"
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

        [HttpPost]
        public ActionResult GetLastSales(int? salecount, long? ItemId, long? customer, long? mc)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int scount = salecount ?? 10;

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var v = (from b in db.SEItemss
                     join c in db.Items on b.Item equals c.ItemID
                     join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                     from e in punit.DefaultIfEmpty()

                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join s in db.SalesEntrys on b.SalesEntry equals s.SalesEntryId
                     join p in db.Customers on s.Customer equals p.CustomerID
                     join m in db.MCs on s.MaterialCenter equals m.MCId
                     where b.Item == ItemId && (customer == null || s.Customer == customer)
                     && (mc == null || s.MaterialCenter == mc)
                     select new
                     {
                         b.SEItemsId,
                         // Faithful port: legacy rows can hold NULL in these non-nullable struct columns.
                         // EF Core 10 throws "Nullable object must have a value" materializing NULL into a
                         // non-nullable DateTime/decimal — cast to nullable so it round-trips (JSON unchanged).
                         SEDate = (DateTime?)s.SEDate,
                         s.BillNo,
                         CustomerName = p.CustomerName,
                         mc = m.MCName,
                         ItemUnitPrice = (decimal?)b.ItemUnitPrice,
                         ItemQuantity = (decimal?)b.ItemQuantity,
                         ItemSubTotal = (decimal?)b.ItemSubTotal,
                         ItemDiscount = (decimal?)b.ItemDiscount,
                         ItemTax = (decimal?)b.ItemTax,
                         itemNote = b.itemNote,
                         ItemTaxAmount = (decimal?)b.ItemTaxAmount,
                         ItemTotalAmount = (decimal?)b.ItemTotalAmount,
                         ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                         ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                         ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                         PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                         //bundleitem = (from ab in db.SEItemss
                         //              where ab.itemNote == "-:{Bundle_Item}"
                         //              && b.Item == ab.ItemDiscount
                         //                  ItemCode = bb.ItemCode,
                         //                  ItemName = bb.ItemName,
                         //                  ItemUnit = cb.ItemUnitName,
                         //                  ItemQuantity = ab.ItemQuantity,
                         //              }).ToList()
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.OrderByDescending(o => o.SEDate);
            }
            recordsTotal = v.Count();
            var data = v.Take(scount).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        public Int64 SaveUsedMaterials(string UserId, long Branch, string[] saledata, string[][] arrayused, long salesEntryId, long? protask)
        {
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
            dtItem.Columns.Add("itemNote");
            dtItem.Columns.Add("SaleEntry");
            dtItem.Columns.Add("Item");
            dtItem.Columns.Add("Type");
            long itemidu = 0;
            foreach (var arr in arrayused)
            {
                if (arr[0] != null)
                {
                    var qty = 0;
                    DataRow dr = dtItem.NewRow();
                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                    dr["ItemQuantity"] = (qty > 0) ? qty : Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                    dr["ItemTax"] = Convert.ToDecimal(arr[10]);//arr[10]
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                    dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                    dr["SaleEntry"] = salesEntryId;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    itemidu = Convert.ToInt64(arr[0]);
                    dr["type"] = true;

                    if (protask != null)
                    {
                        db.ItemTask.Where(a => a.ItemId == itemidu && a.protaskid == protask).ToList().ForEach(o => {
                            o.invoiced = 2;
                            o.seitemid = salesEntryId;
                        });
                        db.SaveChanges();

                    }

                    dtItem.Rows.Add(dr);
                }
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
                    long typ = Convert.ToInt64(saledata[36]);
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
                                      ItemUnitPrice = a.ItemUnitPrice,
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
                        dbu["SaleEntry"] = salesEntryId;
                        dbu["Item"] = bu.Item;
                        dbu["Type"] = true;
                        dtItem.Rows.Add(dbu);
                    }
                }

            }


            ////// create parameter
            SqlParameter parameter = new SqlParameter("@TableType", dtItem);
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = "TableTypeSEItems";
            //// execute sp sql 
            string sql = String.Format("EXEC {0} {1};", "SP_InsertSEItems", "@TableType");
                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, salesEntryId); // forward-correctness: header = SUM(lines)
            //// execute sql
            db.Database.ExecuteSqlRaw(sql, parameter);

            var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));

            ////add trasaction to sale account
            ////add sale trasaction 
            //// add vat input in account transaction
            //    //if payment
            return salesEntryId;
        }


        [HttpGet]
        public JsonResult GetUsedSEItems(long SalesEntryID)
        {
            var ConD = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        where a.SalesEntry == SalesEntryID && a.itemNote != "-:{Bundle_Item}"
                        && a.Type == true
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
                            a.ItemTotalAmount,
                            note = a.itemNote.Replace("<br />", "\n"),

                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.slreq,

                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            batch = (from ay in db.BatchStocks
                                     join az in db.SalesEntrys on ay.Reference equals az.SalesEntryId into abc
                                     from az in abc.DefaultIfEmpty()

                                     where az.SalesEntryId == SalesEntryID && ay.Type == "UsedSales" && b.ItemID == ay.Item

                                     select new UBatchStockPViewModel
                                     {
                                         BatchNo = ay.BatchNo,
                                         MFGd = ay.MFG,
                                         EXPd = ay.EXP,
                                         StockIn = ay.StockIn,
                                         StockOut = ay.StockOut,
                                         Item = ay.Item,
                                         cfactor = b.ConFactor,
                                         Priunit = b.ItemUnitID,
                                         Secunit = b.SubUnitId,
                                         Unit = ay.Unit,
                                         Cost = ay.Cost,
                                         origin = "Sales",
                                         Order = ay.Order
                                     }).ToList(),
                            a.Type,
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.slreq,
                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                            subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),

                            o.batch,
                            o.Type
                        }).ToList();
            return Json(ConD);
        }

        //For Uploading the File
        public ActionResult UploadFiles()
        {
            if (Request.Form.Files.Count > 0)
            {
                string SaleId = Request.Form.GetValues("id").First();
                long SId = 0;

                if (SaleId.Contains("undefined"))
                {
                    var LastId = db.SalesEntrys.OrderByDescending(a => a.SalesEntryId).FirstOrDefault();
                    SId = LastId.SalesEntryId;
                }
                else
                {
                    SId = Convert.ToInt64(SaleId);
                }

                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/");

                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];

                            if (file.Length > 0)
                            {
                                var fileCount = db.AttachmentDocuments.Select(a => a.DocumentID).AsEnumerable().DefaultIfEmpty(0).Max();

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";
                                }
                                string Realname = newName;
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/"), newName);
                                file.SaveAs(newName);

                                var PODocument = new AttachmentDocuments
                                {
                                    TransactionID = SId,
                                    TransactionType = "CreditSale",
                                    FileName = Realname,
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.AttachmentDocuments.Add(PODocument);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/"), resizeName);
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

        //end

        //For deleteing when any of the image deleted
        public JsonResult ImageDelete(long key)
        {
            //To remove the attached file(single row) from database
            AttachmentDocuments Document = db.AttachmentDocuments.Find(key);
            db.AttachmentDocuments.Remove(Document);
            db.SaveChanges();

            //To remove the attached file from folder
            string fullpath = LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/" + Document.FileName);

            if (System.IO.File.Exists(fullpath))
            {
                System.IO.File.Delete(fullpath);
            }
            string fullPaththumb = LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/" + "thumb_" + Document.FileName);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }
            string fullPathresize = LegacyWeb.MapPath("~/uploads/CreditSaleDocuments/" + "resize_" + Document.FileName);
            if (System.IO.File.Exists(fullPathresize))
            {
                System.IO.File.Delete(fullPathresize);
            }
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "CreditSale", "AttachmentDocuments", findip(), Document.DocumentID, "CreditSale Deleted Successfully");

            bool status = true;
            string message = "Successfully deleted Creditsale attachment details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = status, message = message, Id = key } };
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Function To Return All Serial No.s As a string(Seperated By ,
        [HttpGet]
        public JsonResult GetAllSerialNos(long SalesEntryId, long ItemId)
        {
            var ItemList = (from a in db.SalesEntrys
                            join b in db.BatchStocks
                            on a.SalesEntryId equals b.Reference
                            where (a.SalesEntryId == SalesEntryId && b.Item == ItemId && b.Type == "Sales")
                            select new
                            {
                                BatchNo = b.BatchNo
                            }).ToList();

            var j = 0;
            string SlNos = "";

            //Appending SerialNo.s
            for (j = 0; j < ItemList.Count; j++)
            {
                SlNos += ItemList[j].BatchNo + ", ";
            }

            return Json(SlNos);
        }

        //Function To Save the Employee Petty Cash Details Pop-Up
        public void UpdateSettlement(ICollection<SettlementViewModel> SettlementData, long SalesEntryId, DateTime SEDate, long CustomerId, long? ProjectId, long? ProTaskId, decimal TotalAmount, long BranchId, long CustAccountId)
        {
            decimal PaidAmount = 0, BalanceAmount = 0;

            if (SettlementData != null && SettlementData.Count > 0)
            {
                long Branch = 0;

                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);
                var salesentrys = db.SalesEntrys.Where(o => o.SalesEntryId == SalesEntryId).FirstOrDefault();
                salesentrys.Ref4 = SettlementData.ElementAt(0).cuscash;
                salesentrys.Ref5 = SettlementData.ElementAt(0).balcash;
                db.Entry(salesentrys).State = EntityState.Modified;
                db.SaveChanges();
                BalanceAmount = SettlementData.ElementAt(0).BalanceAmount;
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();

                //Remove Receipt Details And Inserting New
                var ReceiptDtls = db.Receipts.Where(a => a.Reference == SalesEntryId && a.RefType == "Sales").FirstOrDefault();
                if (ReceiptDtls != null)
                {
                    db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == SalesEntryId && a.RefType == "Sales"));
                    db.SaveChanges();
                }

                //Remove SETransaction Details And Inserting New
                var SETransDtls = db.SETransactions.Where(a => a.SalesEntry == SalesEntryId).FirstOrDefault();
                if (SETransDtls != null)
                {
                    db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.SalesEntry == SalesEntryId));
                    db.SaveChanges();
                }

                foreach (var row in SettlementData)
                {

                    if (row.Amount > 0)
                    {
                        com.addAccountTrasaction(row.Amount, 0, (long)row.AccountId, "Sale Payment", SalesEntryId, DC.Debit, SEDate, null, null, ProjectId, ProTaskId);

                        PaidAmount = PaidAmount + row.Amount;

                        //Save to Cheque Details---> Only If Cheque Payment Type Value is Entered

                        //        ChequeDetails Obj = new ChequeDetails
                        //            TransId     =   SalesEntryId,
                        //            TransType   =   "Sale Payment",
                        //            ChequeDate  =   row.ChequeDate,
                        //            ChequeNo    =   row.ChequeNo

                        /******************************** SE Transaction & Receipt *************************************/

                        var Remark = "Direct Reciept From Sale Entry";
                        long payid;

                        //SETransaction
                        SETransaction SEtran = new SETransaction();

                        SEtran.CustomerId = CustomerId;
                        SEtran.SEPayDate = SEDate;
                        SEtran.SEPayAmount = row.Amount;
                        payid = com.addReceipt(SEDate, CustAccountId, (long)row.AccountId, row.Amount, TotalAmount, Remark, UserId, BranchId, SalesEntryId);
                        SEtran.Recieptid = payid;
                        SEtran.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        SEtran.CreatedBranch = Convert.ToInt32(BranchId);
                        SEtran.CreatedUserId = UserId;
                        SEtran.Status = 1;
                        SEtran.SalesEntry = SalesEntryId;
                        db.SETransactions.Add(SEtran);
                        db.SaveChanges();

                        /***********************************************************************************************/
                    }
                }

                //Credit The Customer Account With Paid Amount
                if (CustAccountId != 0 && CustAccountId != null && PaidAmount > 0)
                    com.addAccountTrasaction(0, PaidAmount, CustAccountId, "Sale Payment", SalesEntryId, DC.Credit, SEDate, null, null, ProjectId, ProTaskId);


                /*************************************** SE Payment *********************************************/
                //Delete Already Existing Payment & Insert New
                SEPayment SEpayExists = db.SEPayments.Where(a => a.SalesEntry == SalesEntryId).FirstOrDefault();

                if (SEpayExists != null)
                {
                    db.SEPayments.RemoveRange(db.SEPayments.Where(a => a.SalesEntry == SalesEntryId));
                    db.SaveChanges();
                }

                SEPayment SEpay = new SEPayment();
                SEpay.SEDate = SEDate;
                SEpay.SEEntryDate = today;
                SEpay.SEBillAmount = TotalAmount;
                SEpay.SEPaidAmount = PaidAmount;
                SEpay.CustomerId = CustomerId;//saledata[0]
                SEpay.CreatedBranch = Branch;
                SEpay.CreatedUserId = UserId;
                SEpay.SECreatedDate = today;
                SEpay.Status = 1;
                SEpay.SalesEntry = SalesEntryId;
                db.SEPayments.Add(SEpay);
                db.SaveChanges();
                /***********************************************************************************************/
            }
        }

        //Function To Get Employees Petty Cash Details
        [HttpPost]
        public JsonResult EmpPettyCashDetails(long EmployeeId, long CustomerId, long SalesEntryId, string Mode, bool taxtype)
        {
            var CustomerName = "";

            CustomerName = db.Customers.Where(x => x.CustomerID == CustomerId).Select(x => x.CustomerName).FirstOrDefault();

            var data = (from a in db.accountmaps
                        join b in db.Accountss
                        on a.AccountId equals b.AccountsID
                        where (a.EmployeeId == EmployeeId && a.PaymentTypeId != EmployeePaymentType.Account)
                        && a.notintaxinvoice == taxtype
                        select new
                        {
                            EmployeeId = a.EmployeeId,
                            AccountId = a.AccountId,
                            AccountNames = b.Name,
                            PaymentType = a.PaymentTypeId,
                            desciption = a.description,
                            Amount = 0,
                            CustomerName = CustomerName,
                            a.level
                        }).OrderBy(o => o.level).ToList();

            if (Mode == "Create")
            {
                return Json(data);
            }
            else
            {
                var CustAccountId = db.Customers.Where(a => a.CustomerID == CustomerId).Select(a => a.Accounts).FirstOrDefault();



                var data2 = (from a in db.accountmaps
                             join b in db.Accountss
                             on a.AccountId equals b.AccountsID
                             where (a.EmployeeId == EmployeeId && a.PaymentTypeId != EmployeePaymentType.Account)
                             && a.notintaxinvoice == taxtype
                             select new
                             {
                                 EmployeeId = a.EmployeeId,
                                 AccountId = a.AccountId,
                                 AccountNames = b.Name,
                                 PaymentType = a.PaymentTypeId,
                                 desciption = a.description,
                                 a.level,
                                 Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == SalesEntryId && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),
                                 CustomerName
                                 //ChequeNo,
                                 //ChequeDate
                             }).OrderBy(o => o.level).ToList();

                return Json(data2);
            }
        }

        [HttpPost]
        public JsonResult EmpPettyCashDetailsreturn(long EmployeeId, long CustomerId, long SalesEntryId, string Mode)
        {
            var CustomerName = "";
            var user = db.SalesEntrys.Where(o => o.SalesEntryId == SalesEntryId).Select(o => o.CreatedBy).FirstOrDefault();
            EmployeeId = db.Employees.Where(o => o.UserId == user).Select(o => o.EmployeeId).FirstOrDefault();
            CustomerName = db.Customers.Where(x => x.CustomerID == CustomerId).Select(x => x.CustomerName).FirstOrDefault();

            var data = (from a in db.accountmaps
                        join b in db.Accountss
                        on a.AccountId equals b.AccountsID
                        where (a.EmployeeId == EmployeeId && a.PaymentTypeId != EmployeePaymentType.Account)
                        select new
                        {
                            EmployeeId = a.EmployeeId,
                            AccountId = a.AccountId,
                            AccountNames = b.Name,
                            PaymentType = a.PaymentTypeId,
                            Amount = 0,
                            CustomerName = CustomerName
                        }).OrderBy(o => o.PaymentType).ToList();

            if (Mode == "Create")
            {
                return Json(data);
            }
            else
            {
                var CustAccountId = db.Customers.Where(a => a.CustomerID == CustomerId).Select(a => a.Accounts).FirstOrDefault();



                var data2 = (from a in db.accountmaps
                             join b in db.Accountss
                             on a.AccountId equals b.AccountsID
                             where (a.EmployeeId == EmployeeId && a.PaymentTypeId != EmployeePaymentType.Account)
                             select new
                             {
                                 EmployeeId = a.EmployeeId,
                                 AccountId = a.AccountId,
                                 AccountNames = b.Name,
                                 PaymentType = a.PaymentTypeId,
                                 Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == SalesEntryId && x.Purpose == "Sale Payment" && x.Account == a.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),
                                 CustomerName
                                 //ChequeNo,
                                 //ChequeDate
                             }).OrderBy(o => o.PaymentType).ToList();

                return Json(data2);
            }
        }
        private long GetPeNo()
        {
            Int64 PENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Purchase").Select(a => a.number).FirstOrDefault();
            if ((db.PurchaseEntrys.Select(p => p.PENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    PENo = 1;
                }
                else
                {
                    PENo = number;
                }
            }
            else
            {
                PENo = db.PurchaseEntrys.Max(p => p.PENo + 1);
            }

            return PENo;
        }
        private long GetSRNo()
        {
            Int64 PNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "SalsReturn").Select(a => a.number).FirstOrDefault();
            if ((db.SalesReturns.Select(p => p.SRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                PNo = (number == 0) ? 1 : number;
            }
            else
            {
                PNo = db.SalesReturns.Max(p => p.SRNo + 1);
            }
            return PNo;
        }

        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public string rewritebook(string fromdate, string endate)
        {
            deletebillsun(fromdate, endate);
            salesrewritebook(fromdate, endate);
            salesrtrewritebook(fromdate, endate);
            purchaserewritebook(fromdate, endate);
            purchasertrewritebook(fromdate, endate);
            return "Success";
        }
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public string salesrewritebook(string fromdate, string endate)
        {


            DateTime edt = DateTime.Parse(endate, new CultureInfo("en-GB"));
            DateTime frdt = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            var sales = db.SalesEntrys.Where(o => o.SEDate <= edt & o.SEDate >= frdt).ToList();
            foreach (var st in sales)
            {
                if (1 == 1)
                {
                    var bssun = db.SEBillSundrys.Where(o => o.SalesEntry == st.SalesEntryId).ToList();
                    foreach (var bs in bssun)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                        {
                            decimal bsamount = 0;
                            if (bs.BsAmount == null)
                                bsamount = 0;
                            else
                                bsamount = (decimal)bs.BsAmount;

                            if (ChkAcc.BSType == 0)//additive
                            {

                                com.addAccountTrasaction(0, bsamount, (long)ChkAcc.SAccount, "Sale", st.SalesEntryId, DC.Credit, st.SEDate, null, null, st.Project, st.ProTask);
                            }
                            else //substract
                            {

                                com.addAccountTrasaction(bsamount, 0, (long)ChkAcc.SAccount, "Sale", st.SalesEntryId, DC.Debit, st.SEDate, null, null, st.Project, st.ProTask);
                            }
                        }
                    }
                }
            }

            return "success";
        }
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public string salesrtrewritebook(string fromdate, string endate)
        {


            DateTime edt = DateTime.Parse(endate, new CultureInfo("en-GB"));
            DateTime frdt = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            var sales = db.SalesReturns.Where(o => o.SRDate <= edt && o.SRDate >= frdt).ToList();
            foreach (var st in sales)
            {
                if (1 == 1)
                {
                    var bssun = db.SRBillSundrys.Where(o => o.SalesReturnId == st.SalesReturnId).ToList();
                    foreach (var bs in bssun)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                        {
                            decimal bsamount = 0;
                            if (bs.BsAmount == null)
                                bsamount = 0;
                            else
                                bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {

                                com.addAccountTrasaction(bsamount, 0, (long)ChkAcc.SAccount, "Sale Return", st.SalesReturnId, DC.Credit, st.SRDate, null, null, st.Project, st.ProTask);
                            }
                            else //substract
                            {

                                com.addAccountTrasaction(0, bsamount, (long)ChkAcc.SAccount, "Sale Return", st.SalesReturnId, DC.Debit, st.SRDate, null, null, st.Project, st.ProTask);
                            }
                        }
                    }
                }
            }

            return "success";
        }
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public string deletebillsun(string fromdate, string endate)
        {

            {
                DateTime edt = DateTime.Parse(endate, new CultureInfo("en-GB"));
                DateTime frdt = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                var sales = db.PurchaseEntrys.Where(o => o.PEDate <= edt && o.PEDate >= frdt).ToList();
                foreach (var st in sales)
                {
                    var pebi = (from a in db.BillSundrys
                                join b in db.PEBillSundrys on a.BillSundryId equals b.BillSundry
                                where b.PurchaseEntry == st.PurchaseEntryId
                                select new
                                {
                                    b.PurchaseEntry,
                                    a.PAccount,
                                }
                               ).ToList();
                    foreach (var peb in pebi)
                    {
                        db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(o => o.reference == peb.PurchaseEntry && o.Purpose == "Purchase" && o.Account == peb.PAccount).ToList());
                        db.SaveChanges();
                    }
                }
            }



            {
                DateTime frdt = DateTime.Parse(fromdate, new CultureInfo("en-GB"));

                DateTime edt = DateTime.Parse(endate, new CultureInfo("en-GB"));
                var sales = db.PurchaseReturns.Where(o => o.PRDate <= edt && o.PRDate >= frdt).ToList();
                foreach (var st in sales)
                {
                    var pebi = (from a in db.BillSundrys
                                join b in db.PRBillSundrys on a.BillSundryId equals b.BillSundry
                                where b.PurchaseReturnId == st.PurchaseReturnId
                                select new
                                {
                                    b.PurchaseReturnId,
                                    a.PAccount,
                                }
                               ).ToList();
                    foreach (var peb in pebi)
                    {
                        db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(o => o.reference == peb.PurchaseReturnId && o.Purpose == "Purchase Return" && o.Account == peb.PAccount).ToList());
                        db.SaveChanges();
                    }
                }
            }

            {
                DateTime frdt = DateTime.Parse(fromdate, new CultureInfo("en-GB"));

                DateTime edt = DateTime.Parse(endate, new CultureInfo("en-GB"));
                var sales = db.SalesEntrys.Where(o => o.SEDate <= edt && o.SEDate >= frdt).ToList();
                foreach (var st in sales)
                {
                    var pebi = (from a in db.BillSundrys
                                join b in db.SEBillSundrys on a.BillSundryId equals b.BillSundry
                                where b.SalesEntry == st.SalesEntryId
                                select new
                                {
                                    b.SalesEntry,
                                    a.SAccount,
                                }
                               ).ToList();
                    foreach (var peb in pebi)
                    {
                        db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(o => o.reference == peb.SalesEntry && o.Purpose == "Sale" && o.Account == peb.SAccount).ToList());
                        db.SaveChanges();
                    }
                }
            }

            {
                DateTime frdt = DateTime.Parse(fromdate, new CultureInfo("en-GB"));

                DateTime edt = DateTime.Parse(endate, new CultureInfo("en-GB"));
                var sales = db.SalesReturns.Where(o => o.SRDate <= edt && o.SRDate >= frdt).ToList();
                foreach (var st in sales)
                {
                    var pebi = (from a in db.BillSundrys
                                join b in db.SRBillSundrys on a.BillSundryId equals b.BillSundry
                                where b.SalesReturnId == st.SalesReturnId
                                select new
                                {
                                    b.SalesReturnId,
                                    a.SAccount,
                                }
                               ).ToList();
                    foreach (var peb in pebi)
                    {

                        db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(o => o.reference == peb.SalesReturnId && o.Purpose == "Sale Return" && o.Account == peb.SAccount).ToList());
                        db.SaveChanges();
                    }
                }
            }





            return "success";












        }
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public string purchaserewritebook(string fromdate, string endate)
        {
            DateTime frdt = DateTime.Parse(fromdate, new CultureInfo("en-GB"));

            DateTime edt = DateTime.Parse(endate, new CultureInfo("en-GB"));
            var sales = db.PurchaseEntrys.Where(o => o.PEDate <= edt && o.PEDate >= frdt).ToList();
            foreach (var st in sales)
            {
                if (1 == 1)
                {
                    var bssun = db.PEBillSundrys.Where(o => o.PurchaseEntry == st.PurchaseEntryId).ToList();
                    foreach (var bs in bssun)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                        {
                            decimal bsamount = 0;
                            if (bs.BsAmount == null)
                                bsamount = 0;
                            else
                                bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {

                                com.addAccountTrasaction(bsamount, 0, (long)ChkAcc.PAccount, "Purchase", st.PurchaseEntryId, DC.Credit, st.PEDate);
                            }
                            else //substract
                            {

                                com.addAccountTrasaction(0, bsamount, (long)ChkAcc.PAccount, "Purchase", st.PurchaseEntryId, DC.Debit, st.PEDate);
                            }
                        }
                    }
                }
            }

            return "success";
        }

        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public string purchasertrewritebook(string fromdate, string endate)
        {
            DateTime frdt = DateTime.Parse(fromdate, new CultureInfo("en-GB"));

            DateTime edt = DateTime.Parse(endate, new CultureInfo("en-GB"));
            var sales = db.PurchaseReturns.Where(o => o.PRDate <= edt && o.PRDate >= frdt).ToList();
            foreach (var st in sales)
            {
                if (1 == 1)
                {
                    var bssun = db.PRBillSundrys.Where(o => o.PurchaseReturnId == st.PurchaseReturnId).ToList();
                    foreach (var bs in bssun)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                        {
                            decimal bsamount = 0;
                            if (bs.BsAmount == null)
                                bsamount = 0;
                            else
                                bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {

                                com.addAccountTrasaction(0, bsamount, (long)ChkAcc.PAccount, "Purchase Return", st.PurchaseReturnId, DC.Credit, st.PRDate);
                            }
                            else //substract
                            {

                                com.addAccountTrasaction(bsamount, 0, (long)ChkAcc.PAccount, "Purchase Return", st.PurchaseReturnId, DC.Debit, st.PRDate);
                            }
                        }
                    }
                }
            }

            return "success";
        }








        [HttpGet]
        public JsonResult GetSalesReturnItems(long SalesEntryID)
        {
            var ConD = (from a in db.SRItemss
                        join z in db.SalesReturns on a.SalesReturnId equals z.SalesReturnId
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        where z.SalesEntryId == SalesEntryID && a.itemNote != "-:{Bundle_Item}"
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
                            a.ItemTotalAmount,
                            note = a.itemNote.Replace("<br />", "\n"),

                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.slreq,
                            z.Remarks,
                            z.SRNote,
                            z.CustomerType,
                            z.SRNo,
                            z.SalesReturnId,
                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.slreq,
                            o.Remarks,
                            o.SRNote,
                            o.CustomerType,
                            o.SRNo,
                            o.SalesReturnId,
                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),
                             
                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                            subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),

                        }).ToList();
            return Json(ConD);
        }

    }
}
