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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class DeliverynoteController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public DeliverynoteController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Deliverynote 
        [QkAuthorize(Roles = "Dev,Deliverynote List")]
        public ActionResult Index()
        {
            var RemoveItemData = db.EnableSettings.Where(a => a.EnableType == "RemoveItemData").FirstOrDefault();
            var CheckItemData = RemoveItemData != null ? RemoveItemData.Status : Status.inactive;
            ViewBag.ItemDataCheck = CheckItemData;
            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");
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
            var created = db.Users.Where(o=>o.Discount==null).Select(s => new
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
            var UserId = User.Identity.GetUserId();
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;


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
            var MlaDNote = db.EnableSettings.Where(a => a.EnableType == "MLADNote").FirstOrDefault();
            var MlaDNotes = MlaDNote != null ? MlaDNote.Status : Status.inactive;
            ViewBag.MLADNote = MlaDNotes;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindDNote").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

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
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             }).Take(1)
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            return View();
        }
        [QkAuthorize(Roles = "Dev,Deliverynote Entry")]
        public ActionResult Create(long? id, string type)
        {


            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var entry = new DeliverynoteViewModel
            {
                BillNo = InvoiceNo(),
                DvDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "dvnote").Select(a => a.TermsCondit).FirstOrDefault(),
                SalesTypes = db.SalesTypes.ToList(),

            };
            if (id != null)
            {
                if (type == "Quote")
                {
                    Quotation quentry = db.Quotations.Find(id);

                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    entry.ConTypeId = quentry.QuotationId;
                    entry.ConType = type;
                    entry.DvDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    entry.DvCashier = quentry.QuotCashier;
                    entry.Customer = quentry.Customer;
                    entry.DvDiscount = quentry.QuotDiscount;
                    entry.DvGrandTotal = quentry.QuotGrandTotal;
                    entry.Remarks = quentry.Remarks;
                    entry.Branch = quentry.Branch;
                    entry.SaleType = quentry.SaleType;
                    entry.SalesType = quentry.SalesType;

                    entry.TermsCondition = quentry.TermsCondition;

                    entry.convertFrom = type + " No";//label
                    entry.convertBill = quentry.BillNo;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        entry.Project = quentry.Project;
                        entry.ProTask = quentry.ProTask;
                    }
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Quotation").FirstOrDefault();
                        entry.FromDate = Hdet.StartDate;
                        entry.ToDate = Hdet.EndDate;
                        entry.HireType = Hdet.HireType;
                    }

                }
                if (type == "ProForma")
                {
                    ProForma pfentry = db.ProFormas.Find(id);

                    if (pfentry == null)
                    {
                        return NotFound();
                    }
                    entry.ConTypeId = pfentry.ProFormaId;
                    entry.ConType = type;
                    entry.DvDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    entry.DvCashier = pfentry.PFCashier;
                    entry.Customer = pfentry.Customer;
                    entry.DvDiscount = pfentry.PFDiscount;
                    entry.DvGrandTotal = pfentry.PFGrandTotal;
                    entry.Location = pfentry.Location;
                    entry.Remarks = pfentry.Remarks;
                    entry.Branch = pfentry.Branch;
                    entry.SaleType = pfentry.SaleType;
                    entry.SalesType = pfentry.SalesType;
                    entry.PaymentTerms = pfentry.PaymentTerms;

                    entry.TermsCondition = pfentry.PFNote;

                    entry.convertFrom = type + " No";//label
                    entry.convertBill = pfentry.BillNo;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        entry.Project = pfentry.Project;
                        entry.ProTask = pfentry.ProTask;
                    }
                    if (pfentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Proforma").FirstOrDefault();
                        entry.FromDate = Hdet.StartDate;
                        entry.ToDate = Hdet.EndDate;
                        entry.HireType = Hdet.HireType;
                    }
                }
                if(type == "DvExtend")
                {
                    Deliverynote DvEntry = db.Deliverynotes.Find(id);

                    if(DvEntry == null)
                    {
                        return NotFound();
                    }
                    entry.ConTypeId = DvEntry.DeliverynoteId;
                    entry.ConType = type;
                    entry.DvDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    entry.DvCashier = DvEntry.DvCashier;
                    entry.Customer = DvEntry.Customer;
                    entry.DvDiscount = DvEntry.DvDiscount;
                    entry.DvGrandTotal = DvEntry.DvGrandTotal;
                    entry.Location = DvEntry.Location;
                    entry.Remarks = DvEntry.Remarks;
                    entry.Branch = DvEntry.Branch;
                    entry.SaleType = DvEntry.SaleType;
                    entry.DvValidity = DvEntry.DvValidity;
                    entry.LPONo = DvEntry.LPONo;
                    entry.SalesType = DvEntry.SalesType;
                    entry.PaymentTerms = DvEntry.PaymentTerms;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        entry.Project = DvEntry.Project;
                        entry.ProTask = DvEntry.ProTask;
                    }

                    entry.TermsCondition = DvEntry.TermsCondition;

                    if (DvEntry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Delivernote").FirstOrDefault();
                        entry.FromDate = Hdet.EndDate;                        
                        entry.HireType = Hdet.HireType;
                    }
                }
                if (type == "SOrder" && ViewBag.BusinessType== "ProjectBasedBusiness")
                {
                    SalesOrder Sorder = db.SalesOrders.Find(id);

                    if (Sorder == null)
                    {
                        return NotFound();
                    }
                    entry.ConTypeId = Sorder.SalesOrderId;
                    entry.ConType = type;
                    entry.DvDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    entry.DvCashier = Sorder.SOCashier;
                    entry.Customer = Sorder.Customer;
                    entry.DvDiscount = Sorder.SODiscount;
                    entry.DvGrandTotal = Sorder.SOGrandTotal;
                    entry.Remarks = Sorder.Remarks;
                    entry.Branch = Sorder.Branch;
                    entry.SaleType = Sorder.SaleType;
                    entry.SalesType = Sorder.SalesType;

                    entry.convertFrom = type + " No";//label
                    entry.convertBill = Sorder.BillNo;

                    entry.TermsCondition = Sorder.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        entry.Project = Sorder.Project;
                        entry.ProTask = Sorder.ProTask;
                    }
                    if (Sorder.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "SalesOrder").FirstOrDefault();
                        entry.FromDate = Hdet.StartDate;
                        entry.ToDate = Hdet.EndDate;
                        entry.HireType = Hdet.HireType;
                    }

                }
                if (type == "Sales")
                {
                    SalesEntry quentry = db.SalesEntrys.Find(id);

                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    entry.ConTypeId = quentry.SalesEntryId;
                    entry.ConType = type;
                    entry.DvDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    entry.DvCashier = quentry.SECashier;
                    entry.Customer = quentry.Customer;
                    entry.DvDiscount = quentry.SEDiscount;
                    entry.DvGrandTotal = quentry.SEGrandTotal;
                    entry.Remarks = quentry.Remarks;
                    entry.Branch = quentry.Branch;
                    entry.SaleType = quentry.SaleType;
                    entry.SalesType = quentry.SalesType;

                    entry.TermsCondition = quentry.PaymentTerms;

                    entry.convertFrom = type + " No";//label
                    entry.convertBill = quentry.BillNo;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        entry.Project = quentry.Project;
                        entry.ProTask = quentry.ProTask;
                    }
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Quotation").FirstOrDefault();
                        entry.FromDate = Hdet.StartDate;
                        entry.ToDate = Hdet.EndDate;
                        entry.HireType = Hdet.HireType;
                    }

                }
            }

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            companySet();
            var userpermission = User.IsInRole("All Deliverynote Entry");
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            ViewBag.LastEntry = db.Deliverynotes.Where(p => MCArray.Contains(p.MaterialCenter) && (userpermission == true || p.CreatedUserId == UserId)).Select(p => p.DeliverynoteId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

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
            }).Take(1)
            .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var RemoveItemData = db.EnableSettings.Where(a => a.EnableType == "RemoveItemData").FirstOrDefault();
            var CheckItemData = RemoveItemData != null ? RemoveItemData.Status : Status.inactive;
            ViewBag.ItemDataCheck = CheckItemData;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
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
            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
              .Select(s => new
              {
                  ID = s.EmployeeId,
                  Name = s.FirstName + " " + s.LastName
              })
              .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaDNote = db.EnableSettings.Where(a => a.EnableType == "MLADNote").FirstOrDefault();
            var MlaDNotes = MlaDNote != null ? MlaDNote.Status : Status.inactive;
            ViewBag.MLADNote = MlaDNotes;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            entry.FieldMap = db.FieldMappings.Where(a => a.Section == "DvNote" && a.Status == Status.active).ToList();
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

            return View(entry);
        }
        [HttpGet]
        public ActionResult downloadprint(long dvId)
        {

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            var UserId = User.Identity.GetUserId();
            var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

            long MC = 0;

            var fmapp = db.FieldMappings.Where(a => a.Section == "DvNote" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                var DeliveryNoteData = com.DeliveryNoteData(dvId, InPrintItemCode, PartNoCheck, 1000, ProjectCheck, ComHeadCheck);
                var item = DeliveryNoteData.pdfItem.ToList();
                var summary = DeliveryNoteData;

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                
                bool stat = true;
            return Json(new { status = stat, item, summary, fmapp });
            
        }
        [QkAuthorize(Roles = "Dev,Deliverynote Entry")]
        public JsonResult CreateDeliverynote(string[][] array, string[] dvdata, string action)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                if (!BillExist(Convert.ToString(dvdata[15])))
                {
                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                    var UserId = User.Identity.GetUserId();
                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(dvdata[23]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }
                    var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                    var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                    long MC = 0;
                    if (MCcheck == Status.active)
                    {
                        MC = Convert.ToInt32(dvdata[20]);
                    }
                    else
                    {
                        MC = 1;
                    }

                    //sales entry
                    Deliverynote Dventry = new Deliverynote();
                    if (dvdata[24] != null)
                    {
                        string str = dvdata[24];
                        SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                        Dventry.SaleType = Stype;
                    }
                    else
                    {
                        Dventry.SaleType = SaleType.Sale;
                    }
                    Dventry.DvNo = GetDvNo(Dventry.SaleType);
                    Dventry.BillNo = Convert.ToString(dvdata[15]);
                    Dventry.DvDate = DateTime.Parse(dvdata[2], new CultureInfo("en-GB"));
                    Dventry.DvCashier = dvdata[1] != "" ? Convert.ToInt64(dvdata[1]) : 0;
                    Dventry.Customer = Convert.ToInt64(dvdata[0]);

                    Dventry.DvItems = Convert.ToInt32(dvdata[3]);
                    Dventry.DvItemQuantity = Convert.ToDecimal(dvdata[4]);
                    Dventry.DvSubTotal = Convert.ToDecimal(dvdata[8]);
                    Dventry.DvTax = Convert.ToDecimal(dvdata[9]);
                    Dventry.DvTaxAmount = Convert.ToDecimal(dvdata[5]);
                    Dventry.DvDiscount = Convert.ToDecimal(dvdata[6]);
                    Dventry.DvGrandTotal = Convert.ToDecimal(dvdata[7]);
                    Dventry.DvNote = "";
                    Dventry.Mail = 0;
                    Dventry.DvCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    Dventry.CreatedUserId = UserId;
                    Dventry.Status = Status.active;
                    Dventry.TermsCondition = Convert.ToString(dvdata[11]);
                    Dventry.EmailTemplateID = Dventry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "DeliveryNote").Select(a => a.EmailTemplateID).FirstOrDefault();
                    Dventry.CompanyHeaderID = 0;
                    Dventry.Branch = Branch;
                    Dventry.DvValidity = Convert.ToInt32(dvdata[10]);
                    Dventry.Location = dvdata[16];
                    Dventry.Remarks = dvdata[19];
                    Dventry.MaterialCenter = MC;
                    Dventry.SalesType = Convert.ToInt64(dvdata[28]);
                    Dventry.PaymentTerms = dvdata[29];
                    Dventry.Project = dvdata[30] != "" ? Convert.ToInt64(dvdata[30]) : 0;
                    Dventry.ProTask = dvdata[31] != "" ? Convert.ToInt64(dvdata[31]) : 0;
                    //pay type
                    Dventry.CustomerType = (dvdata[17] == "1")? CustomerType.Walking: CustomerType.Customer;
                    Dventry.LPONo = dvdata[18];

                    Dventry.Ref1 = Convert.ToString(dvdata[33]);
                    Dventry.Ref2 = Convert.ToString(dvdata[34]);
                    Dventry.Ref3 = Convert.ToString(dvdata[35]);
                    Dventry.Ref4 = Convert.ToString(dvdata[36]);
                    Dventry.Ref5 = Convert.ToString(dvdata[37]);
                   
                    db.Deliverynotes.Add(Dventry);
                    db.SaveChanges();
                    Int64 dvId = 0;
                    dvId = Dventry.DeliverynoteId;

                    if (Dventry.SaleType == SaleType.Hire)
                    {
                        HireDetail HDetils = new HireDetail();
                        HDetils.StartDate = DateTime.Parse(dvdata[25], new CultureInfo("en-GB"));
                        HDetils.EndDate = DateTime.Parse(dvdata[26], new CultureInfo("en-GB"));
                        HDetils.Section = "Delivernote";
                        HDetils.Reference = dvId;
                        HDetils.HireType = Convert.ToInt64(dvdata[27]);
                        db.HireDetails.Add(HDetils);
                        db.SaveChanges();
                    }

                    var CustomerName = db.Customers.Where(a => a.CustomerID == Dventry.Customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();


                    if (dvdata[21] != null && dvdata[21] != "0" && dvdata[21] != "" && dvdata[22] != null && dvdata[22] != "" && dvdata[22] != "0")
                    {
                        ConvertTransactions ConTran = new ConvertTransactions();

                        ConTran.ConvertFrom = dvdata[22];
                        ConTran.ConvertTo = "DVNote";
                        ConTran.From = Convert.ToInt64(dvdata[21]);
                        ConTran.To = dvId;
                        ConTran.Status = 0;
                        ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        ConTran.CreatedBy = UserId;
                        ConTran.Branch = Convert.ToInt32(BranchID);

                        db.ConvertTransactionss.Add(ConTran);
                        db.SaveChanges();
                        com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Convertion");
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
                    dtItem.Columns.Add("Dv");
                    dtItem.Columns.Add("Item");

                    var mcanddeliverystockeffect = db.EnableSettings.Where(a => a.EnableType == "mcanddeliverystockeffect").FirstOrDefault();
                    var enmcanddeliverystockeffect = mcanddeliverystockeffect != null ? mcanddeliverystockeffect.Status : Status.inactive;
                    List<BOMItem> bomitem = new List<BOMItem>();
                    foreach (var arr in array)
                    {
                        DataRow dr = dtItem.NewRow();
                        BOMItem bom = new BOMItem
                        {

                            ItemId = Convert.ToInt32(arr[0]),
                            Quantity = Convert.ToDecimal(arr[2]),
                            Unit = Convert.ToInt64(arr[1]),
                            BOMItemId = 1,
                            BOMId = 1





                        };
                        bomitem.Add(bom);

                        dr["ItemUnit"] = arr[1];
                        dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                        dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                        dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                        dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                        dr["ItemTax"] = Convert.ToDecimal(arr[10]);
                        dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                        dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                        if(arr.Length>29)
                        dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                        dr["Dv"] = dvId;
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
                            long typ = Convert.ToInt64(dvdata[27]);
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
                                              ItemUnitPrice = (Dventry.SaleType == SaleType.Sale) ? a.ItemSubTotal : hir,
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
                                dbu["Dv"] = dvId;
                                dbu["Item"] = bu.Item;
                                dtItem.Rows.Add(dbu);
                            }
                        }
                    }

                    ////// create parameter 
                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeDvItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertDvItems", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql, parameter);
                    if (enmcanddeliverystockeffect == Status.active)
                    {
                        com.stocktransfertotask("Delivery Note , Voucher no:" + Dventry.DvNo, (long)Dventry.MaterialCenter, UserId, bomitem);
                    }
                    //Approved By
                    var Appby = Convert.ToString(dvdata[32]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = dvId;
                            approval.Type = "Deliverynote";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }



                    com.addlog(LogTypes.Created, UserId, "Deliverynote", "Deliverynotes", findip(), dvId, "Successfully Submitted Deliverynote");

                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                    if (action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "DvNote" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                        var DeliveryNoteData = com.DeliveryNoteData(dvId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                        var item = DeliveryNoteData.pdfItem.ToList();
                        var summary = DeliveryNoteData;

                        var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                        var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                        var def = (PriLay == Status.active) ? Convert.ToInt64(dvdata[38]):Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout , fmapp } };
                    }
                    else if (action == "sendmail")
                    {
                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = dvdata[12];
                        string CcMail = dvdata[13];
                        string InvoiceNo = "_DeliveryNote_" + Dventry.DvNo;

                        var em = db.EmailTemplates.Where(a => a.Head == "DeliveryNote").FirstOrDefault();
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;
                        }
                        else
                        {
                            message.Subject = "DELIVERY NOTE";
                            message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                                " <p>we are enclosing our delivery note for the items / services as requested by you during our discussions.<br/></p> " +
                                " <p>Looking forward to hear from you.</p>";
                        }
                        sm.SendPdfMail(generatePdf(dvId), ToMail, CcMail, InvoiceNo, message);

                        msg = "Successfully submitted Delivery Note.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    else
                    {
                        msg = "Successfully submitted Delivery Note.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }

                    //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Delivery Note No. Already Exists.";
                    stat = false;
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
        [QkAuthorize(Roles = "Dev,Download Deliverynote")]
        public ActionResult Download(long id)
        {

            var Data = db.Deliverynotes.Where(s => s.DeliverynoteId == id).FirstOrDefault();
            var custname = db.Customers.Where(s => s.CustomerID == Data.Customer).Select(a => a.CustomerName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Delivery Note" + "-" + custname + "-" + billno + ".pdf");

        }
        public StringBuilder generatePdf(long dvId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var DeliveryNoteData = com.DeliveryNoteData(dvId, InPrintItemCode, PartNoCheck, TimeOut);
            var item = DeliveryNoteData.pdfItem.ToList();
            var summary = DeliveryNoteData;


            return com.generatepdf(dvId, summary, item, null, "Delivery Note");
        }



        //                   where b.DeliverynoteId == dvId
        //                       BillNo = b.BillNo,
        //                       DvNo = b.DvNo,
        //                       Date = b.DvDate,
        //                       DvValidity = b.DvValidity,
        //                       GrandTotal = b.DvGrandTotal,
        //                       TaxAmount = b.DvTaxAmount,
        //                       Discount = b.DvDiscount,
        //                       SubTotal = b.DvSubTotal,
        //                       tc = b.DvNote,
        //                       PartyName = c.CustomerName,
        //                       CustomerEmail = d.EmailId,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       Address = d.Address,
        //                       Phone = d.Phone,
        //                       Mobile = d.Mobile,
        //                       Email = d.EmailId,
        //                       TRN = c.TaxRegNo,
        //                       City = d.City,
        //                       State = d.State,
        //                       Country = d.Country,
        //                       Zip = d.Zip,
        //                       b.TermsCondition,
        //                       b.Location,
        //                       b.LPONo,
        //                       c.CreditPeriod,
        //                       b.Remarks

        //                  where b.Dv == dvId && b.ItemNote != "-:{Bundle_Item}"
        //                      ItemUnitPrice = b.ItemUnitPrice,
        //                      ItemQuantity = b.ItemQuantity,
        //                      ItemTax = b.ItemTax,
        //                      ItemSubTotal = b.ItemSubTotal,
        //                      ItemNote = b.ItemNote,
        //                      ItemTaxAmount = b.ItemTaxAmount,
        //                      ItemTotalAmount = b.ItemTotalAmount,
        //                      ItemID = b.Item,
        //                      bundleitem = (from ab in db.DvItems
        //                                    where ab.Dv == dvId && ab.ItemNote == "-:{Bundle_Item}"
        //                                    && b.Item == ab.ItemDiscount
        //                                        bb.ItemCode,
        //                                        bb.ItemName,
        //                                        cb.ItemUnitName,
        //                                        ItemUnitPrice = ab.ItemUnitPrice,
        //                                        quantity = ab.ItemQuantity,
        //                                        ItemSubTotal = ab.ItemSubTotal,
        //                                        ItemTax = ab.ItemTax,
        //                                        ItemTaxAmount = ab.ItemTaxAmount,
        //                                        ItemTotalAmount = ab.ItemTotalAmount,

        //                                        ab.Item,
        //                                        ab.ItemQuantity,
        //                                        ab.ItemUnit,

        //                                        ItemDiscount = 0,

        //                                        ItemNote = ab.ItemNote,
        //                                        ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                        bb.ItemUnitID,
        //                                        bb.SubUnitId,
        //                                        PriUnit = cb.ItemUnitName,
        //                                        SubUnit = bd.ItemUnitName,
        //                                        bb.ItemArabic,

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
        [QkAuthorize(Roles = "Dev,Deliverynote List")]
        public ActionResult GetDeliverynote(string BillNo, string FromDate, string ToDate, long? customer, long? salesperson, string Stats, string user, int? Validity, string Saletype, long? HireType, long? MC, string appstat, long? ProjectName, long? Task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "" )
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
            if (ToDate != "" )
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

            var fromv = "DVNote";
            var ToPF = "ProForma";
            var Tosales = "Sale";
            var tov = "DvExtend";

            var DvNoteToSale = db.EnableSettings.Where(a => a.EnableType == "DvNoteToSale").FirstOrDefault();
            var DvNoteToSales = DvNoteToSale != null ? DvNoteToSale.Status : Status.inactive;

            var DvNoteToPF = db.EnableSettings.Where(a => a.EnableType == "DvNoteToPF").FirstOrDefault();
            var DvNoteToPFs = DvNoteToPF != null ? DvNoteToPF.Status : Status.inactive;

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

            var userpermission = User.IsInRole("All Deliverynote Entry");
            var UserId = User.Identity.GetUserId();
            SaleType St = new SaleType();
            if (Saletype != "")
            {
                St = (Saletype == "2") ? SaleType.Hire : SaleType.Sale;
            };
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uDeliveryNoteView = User.IsInRole("View Deliverynote");
            var uEdit = User.IsInRole("Edit Deliverynote");
            var uDownload = User.IsInRole("Download Deliverynote");
            var uDelete = User.IsInRole("Delete Deliverynote");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets and the `qs/hoe/pfa` ConvertTransactionss subqueries).
            // Split SERVER from CLIENT: materialize only entity columns + simple scalars (left-joined entity
            // access like Customer/EmpName/User stays server-side) into serverRows, then build client lookups
            // keyed by DeliverynoteId and re-project client-side with the SAME member names + order.
            var serverQuery = (from b in db.Deliverynotes
                     join a in db.Customers on b.Customer equals a.CustomerID
                     join d in db.Employees on b.DvCashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on b.CreatedUserId equals g.Id
                     join h in db.HireDetails on new { h1 = b.DeliverynoteId, h2 = "Delivernote" }
                    equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.MCs on b.MaterialCenter equals i.MCId into mcs
                     from i in mcs.DefaultIfEmpty()
                     join j in db.Projects on b.Project equals j.ProjectId into prj
                     from j in prj.DefaultIfEmpty()
                     join k in db.ProTasks on b.ProTask equals k.ProTaskId into task
                     from k in task.DefaultIfEmpty()

                         // qs/hoe/pfa (ConvertTransactionss .FirstOrDefault subqueries) and
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are all computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.

                     where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                     (customer == null || customer == 0 || a.CustomerID == customer) &&
                     (salesperson == null || salesperson == 0 || d.EmployeeId == salesperson) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.DvDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.DvDate, tdate) >= 0)
                     && (Stats == null || b.Status == st)
                     && (user == null || user == "" || g.Id == user) && (Validity == null || Validity == 0 || b.DvValidity == Validity)
                     //  && (mc == 0 || mc == b.MaterialCenter)
                     //&& ((MCArray.Contains(b.MaterialCenter) && MC == b.MaterialCenter) || ((MC == null) && MCArray.Contains(b.MaterialCenter)))
                     && (userpermission == true || b.CreatedUserId == UserId)
                     && (Saletype == "" || Saletype == null || St == b.SaleType) && (HireType == 0 || HireType == null || HireType == h.HireType)
                     && (ProjectName == 0 || ProjectName == null || j.ProjectId == ProjectName)
                     && (Task == 0 || Task == null || k.ProTaskId == Task)
                     select new
                     {
                         b.DeliverynoteId,
                         b.DvNo,
                         b.BillNo,
                         b.DvDate,
                         b.DvItems,
                         b.DvItemQuantity,
                         b.DvDiscount,
                         b.DvGrandTotal,
                         b.DvTax,
                         b.DvValidity,
                         b.DvTaxAmount,
                         EmpName = d.FirstName + " " + d.LastName,
                         Customer = a.CustomerCode + " - " + a.CustomerName,
                         User = g.UserName,
                         validity = (DateTime.Now <= b.DvDate.AddDays((b.DvValidity == null) ? 0 : (b.DvValidity.Value + 1))) ? "Active" : "Expired",
                         convertSale = db.SalesEntrys.Where(ab => ab.ConvertType == "DVNote" && ab.ConvertNo == b.DeliverynoteId.ToString()).Select(ab => ab.BillNo).FirstOrDefault(),
                         test = b.DeliverynoteId.ToString(),
                         b.Location,
                         a.Remark,
                         SaleType = b.SaleType,
                         Dev = uDev,
                         Details = uDeliveryNoteView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         MC=i.MCName,

                         ProjectName = (j.ProjectName != null && j.ProjectName != "") ? j.ProCode + "-" + j.ProjectName : "",
                         Task = (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",

                         CreatedDate=b.DvCreatedDate
                     });

            // Performance (audit P2): server-side paging on the common path; search/computed sorts fall back unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BillNo","convertSale","CreatedDate","Customer","Delete","DeliverynoteId","Details","Dev","Download","DvDate","DvDiscount","DvGrandTotal","DvItemQuantity","DvItems","DvNo","DvTax","DvTaxAmount","DvValidity","Edit","EmpName","Location","MC","ProjectName","Remark","SaleType","Task","test","User","validity" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("DeliverynoteId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by DeliverynoteId (missing key -> empty/absent, no KeyNotFound).
            var dvIds = serverRows.Select(o => o.DeliverynoteId).ToList();
            // The three convert markers: latest-or-any ConvertTransactionss row per (DeliverynoteId, From/To combo).
            var convRows = db.ConvertTransactionss
                .Where(ap => dvIds.Contains(ap.From)
                       && ((ap.ConvertFrom == fromv && (ap.ConvertTo == Tosales || ap.ConvertTo == ToPF))
                           || (ap.ConvertFrom == tov && ap.ConvertTo == fromv)))
                .Select(ap => new { ap.From, ap.ConvertFrom, ap.ConvertTo })
                .ToList();
            var convLookup = convRows.ToLookup(ap => ap.From);
            // app = approver EmployeeIds for the delivery note (nested collection, keyed by TransEntry == DeliverynoteId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "Deliverynote" && dvIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "Deliverynote" && dvIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per delivery note.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var conv = convLookup[o.DeliverynoteId];
                         var SaleConvert = conv.Where(x => x.ConvertFrom == fromv && x.ConvertTo == Tosales).Select(x => x.ConvertTo).FirstOrDefault() ?? "";
                         var HExtent = conv.Where(x => x.ConvertFrom == tov && x.ConvertTo == fromv).Select(x => x.ConvertTo).FirstOrDefault();
                         var PFConvert = conv.Where(x => x.ConvertFrom == fromv && x.ConvertTo == ToPF).Select(x => x.ConvertTo).FirstOrDefault();
                         var app = appLookup[o.DeliverynoteId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.DeliverynoteId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.DeliverynoteId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {

                         SaleConvert = SaleConvert,
                         PFConvert = PFConvert,
                         o.DeliverynoteId,
                         HExtent = HExtent,
                         o.DvNo,
                         o.BillNo,
                         o.DvDate,
                         o.DvItems,
                         o.DvItemQuantity,
                         o.DvDiscount,
                         o.DvGrandTotal,
                         o.DvTax,
                         o.DvValidity,
                         o.DvTaxAmount,
                         o.EmpName ,
                         o.Customer ,
                         o.User ,
                         o.validity ,
                         o.convertSale,
                         o.test ,
                         o.Location,
                         o.Remark,
                         o.SaleType ,
                         o.Dev ,
                         o.Details ,
                         o.Edit,
                         o.Download ,
                         o.Delete,
                         o.MC ,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.ProjectName,
                         o.Task,
                         o.CreatedDate,
                         DvNoteToSale = (SaleConvert != "" && DvNoteToSales == Status.active) ? false : true,
                         DvNoteToPF = (PFConvert != null && DvNoteToPFs == Status.active) ? false : true,
                     };
                     });
            if (appstat != "")
            {
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
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Deliverynote")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            DeliverynoteViewModel vmodel = new DeliverynoteViewModel();
            vmodel = (from b in db.Deliverynotes
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.DvCashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join f in db.MCs on b.MaterialCenter equals f.MCId into mcs
                      from f in mcs.DefaultIfEmpty()
                      join t in db.SalesTypes on b.SalesType equals t.Id into stype
                      from t in stype.DefaultIfEmpty()
                      join u in db.HireDetails on new { h1 = b.DeliverynoteId, h2 = "Delivernote" }
                      equals new { h1 = u.Reference, h2 = u.Section } into hir
                      from u in hir.DefaultIfEmpty()
                      join v in db.HireTypes on u.HireType equals v.HireTypeId into htyp
                      from v in htyp.DefaultIfEmpty()
                      where b.DeliverynoteId == id
                      select new DeliverynoteViewModel
                      {
                          CustomerName = c.CustomerCode + " - " + c.CustomerName,
                          DvNo = b.DvNo,
                          BillNo = b.BillNo,
                          DvDate = b.DvDate,
                          TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
                          EmployeeName = e.FirstName + " " + e.LastName,
                          DvDiscount = b.DvDiscount,
                          DvGrandTotal = b.DvGrandTotal,
                          DvValidity = b.DvValidity,
                          Location = b.Location,
                          PayType = (b.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                          LPONo = b.LPONo,
                          CreditPeriod = c != null ? c.CreditPeriod : 0,
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          MCName = f.MCName,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                          SaleTypeName = (b.SaleType == SaleType.Sale) ? "Sale" : ((b.SaleType == SaleType.Hire) ? "Hire" : "POS"),
                          SalesTypeName = t.Name,
                          EmailId = d.EmailId,
                          PaymentTerms = b.PaymentTerms,

                          HType = (u != null) ? v.Name : "",
                          StartDate = (u != null) ? u.StartDate : null,
                          EndDate = (u != null) ? u.EndDate : null,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "Deliverynote"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();
            vmodel.DvItem = db.DvItems.Where(a => a.Dv == id && a.ItemNote != "-:{Bundle_Item}")
            .Select(b => new DvItemViewModel
            {
                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemTax = b.ItemTax,
                ItemNote = b.ItemNote,
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.DvItems
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.Dv == id && ab.ItemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "DvNote" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [HttpGet]
        public ActionResult GetCustomer(int CustID)
        {
            var email = (from b in db.Deliverynotes
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
                        string path = LegacyWeb.MapPath("~/uploads/deliverynote/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {


                                var fileCount = db.Deliverynotes.Select(a => a.DeliverynoteId).AsEnumerable().DefaultIfEmpty(20000).Max();

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/deliverynote/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/deliverynote/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                string Realname = newName;

                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/deliverynote/"), newName);
                                if (System.IO.File.Exists(newName))
                                {
                                    //delete existing file
                                    System.IO.File.Delete(newName);
                                }
                                file.SaveAs(newName);


                                var stocktransferdoc = new AttachmentDocuments
                                {
                                    TransactionID = id,
                                    TransactionType = "Deliverynote",
                                    FileName = newFName,
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.AttachmentDocuments.Add(stocktransferdoc);
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
                                    if (System.IO.File.Exists(thumbName))
                                    {
                                        //delete existing file
                                        System.IO.File.Delete(thumbName);
                                    }
                                    thumb.Save(thumbName);

                                    Image lgimg = Image.FromFile(newName);
                                    if (lgimg.Width > 1800 || lgimg.Height > 1800)
                                    {
                                        Image imgs = Image.FromFile(newName);
                                        System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/stocktransfer/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/stocktransfer/"), resizeName);
                                        if (System.IO.File.Exists(resizeName))
                                        {
                                            //delete existing file
                                            System.IO.File.Delete(resizeName);
                                        }
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
        [QkAuthorize(Roles = "Dev,Edit Deliverynote")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.image = (from b in db.AttachmentDocuments
                             join c in db.Deliverynotes on b.TransactionID equals c.DeliverynoteId
                             where c.DeliverynoteId == id && b.TransactionType == "Deliverynote"
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.DocumentID,
                                 quotationID = b.TransactionID,
                                 FileName = b.FileName,
                             }).ToList();
            var userpermission = User.IsInRole("All Deliverynote Entry");
            var UserId = User.Identity.GetUserId();
            Deliverynote dvnote = db.Deliverynotes.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.DeliverynoteId == id).FirstOrDefault();
            
            if (dvnote == null)
            {
                return NotFound();
            }
            DeliverynoteViewModel vmodel = new DeliverynoteViewModel();
            var cust = db.Customers
                .Select(s => new
                {
                    CustomerID = s.CustomerID,
                    CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                }).Where(o=>o.CustomerID==dvnote.Customer).Take(1).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

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

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "DVNote").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "Quote")
                {
                    CBill = db.Quotations.Where(a => a.QuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
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

            vmodel = (from b in db.Deliverynotes
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.DeliverynoteId, f2 = "Delivernote" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.DeliverynoteId == id
                      select new DeliverynoteViewModel
                      {
                          DvNo = b.DvNo,
                          Customer = b.Customer,
                          DvDate = b.DvDate,
                          BillNo = b.BillNo,
                          DvCashier = b.DvCashier,
                          DvDiscount = b.DvDiscount,
                          DvGrandTotal = b.DvGrandTotal,
                          TermsCondition = b.TermsCondition,
                          custEmailId = e.EmailId,
                          Location = b.Location,
                          LPONo = b.LPONo,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          SalesType=b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          PaymentTerms=b.PaymentTerms,
                          Project=b.Project,
                          ProTask=b.ProTask,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();
            companySet();


            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.preEntry = db.Deliverynotes.Where(a => a.DeliverynoteId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.DeliverynoteId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Deliverynotes.Where(a => a.DeliverynoteId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.DeliverynoteId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var RemoveItemData = db.EnableSettings.Where(a => a.EnableType == "RemoveItemData").FirstOrDefault();
            var CheckItemData = RemoveItemData != null ? RemoveItemData.Status : Status.inactive;
            ViewBag.ItemDataCheck = CheckItemData;

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

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects.Where(o=>o.ProjectId==vmodel.Project)
              .Select(s => new
              {
                  ID = s.ProjectId,
                  Name = s.ProCode + " " + s.ProjectName
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks.Where(o=>o.ProTaskId==vmodel.ProTask)
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            })
            .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            ViewBag.PopUpAddCust = false;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Deliverynote").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaDNote = db.EnableSettings.Where(a => a.EnableType == "MLADNote").FirstOrDefault();
            var MlaDNotes = MlaDNote != null ? MlaDNote.Status : Status.inactive;
            ViewBag.MLADNote = MlaDNotes;

            var EditPermission = User.IsInRole("Disable DvNote Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "Deliverynote", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "DvNote" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummyDvItems.Where(a => a.Dv == id).FirstOrDefault();
            var DvItem = db.DvItems.Where(a => a.Dv == id).FirstOrDefault();
            if (DvItem == null && DItem != null)
            {
                var DItems = db.DummyDvItems.Where(a => a.Dv == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    DvItem sItem = new DvItem();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.ItemNote = arr.ItemNote;
                    sItem.Dv = arr.Dv;
                    sItem.Item = arr.Item;
                    db.DvItems.Add(sItem);
                    db.SaveChanges();
                }

                db.DummyDvItems.RemoveRange(db.DummyDvItems.Where(a => a.Dv == id));
                db.SaveChanges();
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

        [QkAuthorize(Roles = "Dev,Edit Deliverynote")]
        public JsonResult UpdateDeliverynote(string[][] array, string[] dvdata, string action)
        {
            bool stat = false;
            string msg;
            var mcanddeliverystockeffect = db.EnableSettings.Where(a => a.EnableType == "mcanddeliverystockeffect").FirstOrDefault();
            var enmcanddeliverystockeffect = mcanddeliverystockeffect != null ? mcanddeliverystockeffect.Status : Status.inactive;

            if (ModelState.IsValid)
            {
                Int64 dvId = Convert.ToInt64(dvdata[16]);
                Deliverynote Dventry = db.Deliverynotes.Find(dvId);
                if (BillExist(Convert.ToString(dvdata[15])) && Convert.ToString(dvdata[15]) != Dventry.BillNo)
                {
                    msg = "Invoice No. Already Exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg,dvId } };
                }
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                
                var UserId = User.Identity.GetUserId();
                var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(dvdata[22]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }

                var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                long MC = 0;
                if (MCcheck == Status.active)
                {
                    MC = Convert.ToInt32(dvdata[21]);
                }
                else
                {
                    MC = 1;
                }

                var EditPermission = User.IsInRole("Disable DvNote Edit After Approval");
                if (com.chkApproved(dvId, EditPermission, "Deliverynote", UserId) == true)
                {

                    //sales entry

                    Dventry.DvDate = DateTime.Parse(dvdata[2], new CultureInfo("en-GB"));
                    Dventry.DvCashier = dvdata[1] != "" ? Convert.ToInt64(dvdata[1]) : 0;
                    Dventry.Customer = Convert.ToInt64(dvdata[0]);
                    Dventry.BillNo = dvdata[15];

                    Dventry.DvItems = Convert.ToInt32(dvdata[3]);
                    Dventry.DvItemQuantity = Convert.ToDecimal(dvdata[4]);
                    Dventry.DvSubTotal = Convert.ToDecimal(dvdata[8]);
                    Dventry.DvTax = Convert.ToDecimal(dvdata[9]);
                    Dventry.DvTaxAmount = Convert.ToDecimal(dvdata[5]);
                    Dventry.DvDiscount = Convert.ToDecimal(dvdata[6]);
                    Dventry.DvGrandTotal = Convert.ToDecimal(dvdata[7]);
                    Dventry.DvNote = "";
                    Dventry.Mail = 0;
                    Dventry.Status = Status.active;
                    Dventry.TermsCondition = Convert.ToString(dvdata[11]);
                    Dventry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "DeliveryNote").Select(a => a.EmailTemplateID).FirstOrDefault();

                    Dventry.CompanyHeaderID = 0;
                    Dventry.Branch = Branch;
                    Dventry.DvValidity = Convert.ToInt32(dvdata[10]);
                    Dventry.Location = dvdata[17];
                    Dventry.Remarks = dvdata[20];
                    Dventry.MaterialCenter = MC;
                    Dventry.SalesType = Convert.ToInt64(dvdata[27]);
                    Dventry.PaymentTerms = (dvdata[28]);
                    Dventry.Project = dvdata[29] != "" ? Convert.ToInt64(dvdata[29]) : 0;
                    Dventry.ProTask = dvdata[30] != "" ? Convert.ToInt64(dvdata[30]) : 0;
                    if (dvdata[23] != null)
                    {
                        string str = dvdata[23];
                        SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                        Dventry.SaleType = Stype;
                    }
                    else
                    {
                        Dventry.SaleType = SaleType.Sale;
                    }
                    //pay type
                    Dventry.CustomerType = (dvdata[18] == "1") ? CustomerType.Walking : CustomerType.Customer;
                    Dventry.LPONo = dvdata[19];

                    Dventry.Ref1 = Convert.ToString(dvdata[32]);
                    Dventry.Ref2 = Convert.ToString(dvdata[33]);
                    Dventry.Ref3 = Convert.ToString(dvdata[34]);
                    Dventry.Ref4 = Convert.ToString(dvdata[35]);
                    Dventry.Ref5 = Convert.ToString(dvdata[36]);


                    db.Entry(Dventry).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 delyId = Dventry.DeliverynoteId;
                    var HireItem = db.HireDetails.Where(a => a.Reference == delyId && a.Section == "Delivernote").FirstOrDefault();
                    if (HireItem != null)
                    {
                        db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == delyId && a.Section == "Delivernote"));
                        db.SaveChanges();
                    }
                    if (Dventry.SaleType == SaleType.Hire)
                    {
                        HireDetail HDetils = new HireDetail();
                        HDetils.StartDate = DateTime.Parse(dvdata[24], new CultureInfo("en-GB"));
                        HDetils.EndDate = DateTime.Parse(dvdata[25], new CultureInfo("en-GB"));
                        HDetils.Section = "Delivernote";
                        HDetils.Reference = delyId;
                        HDetils.HireType = Convert.ToInt64(dvdata[26]);
                        db.HireDetails.Add(HDetils);
                        db.SaveChanges();
                    }

                    var DVItem = db.DvItems.Where(a => a.Dv == delyId).FirstOrDefault();
                    if (DVItem != null)
                    {
                        var dItems = db.DvItems.Where(a => a.Dv == delyId).ToList();
                        foreach (var arr in dItems)
                        {
                            //add to dummy table
                            DummyDvItem dItem = new DummyDvItem();
                            dItem.ItemUnit = arr.ItemUnit;
                            dItem.ItemUnitPrice = arr.ItemUnitPrice;
                            dItem.ItemQuantity = arr.ItemQuantity;
                            dItem.ItemSubTotal = arr.ItemSubTotal;
                            dItem.ItemDiscount = arr.ItemDiscount;
                            dItem.ItemTax = arr.ItemTax;
                            dItem.ItemTaxAmount = arr.ItemTaxAmount;
                            dItem.ItemTotalAmount = arr.ItemTotalAmount;
                            dItem.ItemNote = arr.ItemNote;
                            dItem.Dv = arr.Dv;
                            dItem.Item = arr.Item;
                            db.DummyDvItems.Add(dItem);
                            db.SaveChanges();
                        }

                        db.DvItems.RemoveRange(db.DvItems.Where(a => a.Dv == delyId));
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
                    dtItem.Columns.Add("Dv");
                    dtItem.Columns.Add("Item");

                    List<BOMItem> bomitem = new List<BOMItem>();

                    foreach (var arr in array)
                    {
                        DataRow dr = dtItem.NewRow();
                        BOMItem bom = new BOMItem
                        {

                            ItemId = Convert.ToInt32(arr[0]),
                            Quantity = Convert.ToDecimal(arr[2]),
                            Unit = Convert.ToInt64(arr[1]),
                            BOMItemId = 1,
                            BOMId = 1





                        };
                        bomitem.Add(bom);
                        dr["ItemUnit"] = arr[1];
                        dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                        dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                        dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                        dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                        dr["ItemTax"] = Convert.ToDecimal(arr[10]);
                        dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                        dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                        dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                        dr["Dv"] = dvId;
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
                            long typ = Convert.ToInt64(dvdata[26]);
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
                                              ItemUnitPrice = (Dventry.SaleType == SaleType.Sale) ? a.ItemSubTotal : hir,
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
                                dbu["ItemDiscount"] = item;
                                dbu["ItemTax"] = itemtax;
                                dbu["ItemTaxAmount"] = taxamt;
                                dbu["ItemTotalAmount"] = totamt;
                                dbu["itemNote"] = "-:{Bundle_Item}";
                                dbu["Dv"] = dvId;
                                dbu["Item"] = bu.Item;
                                dtItem.Rows.Add(dbu);
                            }
                        }
                    }

                    ////// create parameter 
                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeDvItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertDvItems", "@TableType");
                    //// execute sql 
                    var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                    if (ret > 0)
                    {
                        db.DummyDvItems.RemoveRange(db.DummyDvItems.Where(a => a.Dv == delyId));
                        db.SaveChanges();
                    }
                    if (enmcanddeliverystockeffect == Status.active)
                    {
                        var stocktanferid = db.StockTransfers.Where(o => o.Voucher == "Delivery Note , Voucher no:" + Dventry.DvNo).Select(o => o.Id).FirstOrDefault();
                        if (stocktanferid != 0 && stocktanferid != null)
                            Deletetocktransfer(stocktanferid);
                        com.stocktransfertotask("Delivery Note , Voucher no:" + Dventry.DvNo, (long)Dventry.MaterialCenter, UserId, bomitem);
                    }

                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == dvId && a.Type == "Deliverynote").FirstOrDefault();
                    var MrnPO = db.Approvals.Where(a => a.TransEntry == dvId && a.Type == "Deliverynote").FirstOrDefault();
                    if (MrnPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == dvId && a.Type == "Deliverynote"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == dvId && a.Type == "Deliverynote"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = Convert.ToString(dvdata[31]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = dvId;
                            approval.Type = "Deliverynote";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Updated, UserId, "Deliverynote", "Deliverynotes", findip(), dvId, "Successfully Updated Deliverynote");

                }
                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "DvNote" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    var DeliveryNoteData = com.DeliveryNoteData(dvId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                    var item = DeliveryNoteData.pdfItem.ToList();
                    var summary = DeliveryNoteData;

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(dvdata[37]):Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout, fmapp, dvId } };

                }
                else if (action == "sendmail")
                {
                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = dvdata[12];
                    string CcMail = dvdata[13];
                    string InvoiceNo = "_DeliveryNote_" + Dventry.DvNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "DeliveryNote").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "DELIVERY NOTE";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our delivery note for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(dvId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully Updated Delivery Note.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, dvId } };
                }
                else
                {
                    msg = "Successfully Updated Delivery Note.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, dvId } };
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
        public ActionResult GetDvItems(long DvID)
        {
            var ConD = (from a in db.DvItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Dv == DvID && a.ItemNote != "-:{Bundle_Item}"
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
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            b.BasePrice,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.MRP
                        });
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }
        [HttpGet]
        public ActionResult GetDvItemsmc(long DvID,long mc)
        {
            var ConD = (from a in db.DvItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Dv == DvID && a.ItemNote != "-:{Bundle_Item}"
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
                            note = a.ItemNote.Replace("<br />", "\n"),
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
                            b.ItemID,
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
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
                            total = com.GetItemWisestock(o.Item, mc) 

                        });
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }
        public Boolean Deletetocktransfer(long Id)
        {

            var UserId = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            StockTransfer SEen = db.StockTransfers.Find(Id);
            var SEItem = db.StockTransferItems.Where(a => a.StockTransferId == Id);
            var SEItemdummy = db.DummyStkTrsItem2.Where(a => a.StockTransferId == Id);

            var SEBs = db.StockTransferBSundrys.Where(a => a.StockTransferId == Id).FirstOrDefault();

            if (SEItem != null)
            {
                db.StockTransferItems.RemoveRange(db.StockTransferItems.Where(a => a.StockTransferId == Id));
            }
            if (SEItemdummy != null)
            {

                db.DummyStkTrsItem2.RemoveRange(db.DummyStkTrsItem2.Where(a => a.StockTransferId == Id));
            }

            if (SEBs != null)
            {
                db.StockTransferBSundrys.RemoveRange(db.StockTransferBSundrys.Where(a => a.StockTransferId == Id));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == Id && a.Type == "StockTransfer").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == Id && a.Type == "StockTransfer"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == Id && a.Type == "StockTransfer").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == Id && a.Type == "StockTransfer"));
            }

            /***************** Item Transaction ******************/
            if (SEen != null)
                com.ItemTransInDeleteMode("StockTransfer", 0, SEen.MCFrom, SEen.MCTo, Id, UserId, CurrentDate);

            db.StockTransfers.Remove(SEen);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "StockTransfer", "StockTransfers", findip(), SEen.Id, "Successfully Deleted StockTransfer Entry");

            return true;
        }
        [HttpPost]
        public JsonResult GetDVByIds(long[] array)//long[] array 
        {
            var result = (from a in db.DvItems
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                          from c in primary.DefaultIfEmpty()
                          join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                          from d in second.DefaultIfEmpty()
                          where array.Contains(a.Dv)
                          select new
                          {
                              b.ItemID,
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
                              note = a.ItemNote.Replace("<br />", "\n"),
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
                              a.Dv
                          });
            return Json(result);
        }


        //[HttpGet]
        //                where a.Dv == DvID
        //                    a.Item,
        //                    a.ItemQuantity,
        //                    a.ItemUnit,
        //                    a.ItemUnitPrice,
        //                    a.ItemTax,
        //                    a.ItemSubTotal,
        //                    a.ItemTaxAmount,
        //                    a.ItemDiscount,
        //                    a.ItemTotalAmount,

        //                    b.ItemCode,
        //                    b.ItemName,
        //                    ItemWithCode = b.ItemCode + " - " + b.ItemName,
        //                    b.ItemUnitID,
        //                    b.SubUnitId,
        //                    PriUnit = c.ItemUnitName,
        //                    SubUnit = d.ItemUnitName,
        //                    ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
        //                    b.ItemID,
        //                    b.OpeningStock,
        //                    b.MinStock,
        //                    categoryname = e.ItemCategoryName,
        //                    Tax = f.Percentage,
        //                    b.SellingPrice,
        //                    b.PurchasePrice,
        //                    b.BasePrice,
        //                    b.MRP,
        //                    b.KeepStock,

        //                    PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
        //                    SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


        //                    PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
        //                    SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

        //                    PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
        //                    SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

        //                    PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
        //                    SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0

        //                    o.Item,
        //                    o.ItemQuantity,
        //                    o.ItemUnit,
        //                    o.ItemUnitPrice,
        //                    o.ItemTax,
        //                    o.ItemSubTotal,
        //                    o.ItemTaxAmount,
        //                    o.ItemDiscount,
        //                    o.ItemTotalAmount,
        //                    o.ItemID,
        //                    o.ItemCode,
        //                    o.ItemName,
        //                    o.ItemWithCode,
        //                    o.ItemUnitID,
        //                    o.SubUnitId,
        //                    o.note,
        //                    PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
        //                    SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
        //                    o.categoryname,
        //                    o.Tax,
        //                    OpeningStock = o.OpeningStock,
        //                    MinStock = (o.MinStock != null) ? o.MinStock : 0,
        //                    o.ConFactor,
        //                    o.SellingPrice,
        //                    o.PurchasePrice,
        //                    o.BasePrice,
        //                    o.MRP,
        //                    price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
        //                    o.KeepStock,






        [QkAuthorize(Roles = "Dev,Delete Deliverynote")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Deliverynote Entry");
            var UserId = User.Identity.GetUserId();
            Deliverynote Dv = db.Deliverynotes.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.DeliverynoteId == id).FirstOrDefault();

            if (Dv == null)
            {
                return NotFound();
            }
            return PartialView(Dv);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Deliverynote")]
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
                msg = "Successfully deleted Delivery Note.";
            }


            #region Old Code

            #endregion

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Deliverynote")]
        public ActionResult DeleteAllDvNote(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteDV(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Delivery Note.", true);
            return RedirectToAction("Index", "DeliveryNote");
        }

        private Boolean DeleteDV(long sId)
        {
            var Msg = chkDeleteWithMsg(sId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(sId);
            }
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext1 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "DVNote").FirstOrDefault();
            var Ext2 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "DvExtend").FirstOrDefault();
            if (Ext1 != null)
            {
                var inv = db.SalesEntrys.Where(x => x.SalesEntryId == Ext1.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Invoice: " + inv + ".";
            }
            else if (Ext2 != null)
            {
                var inv = db.Deliverynotes.Where(x => x.DeliverynoteId == Ext2.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Deliverynote: " + inv + ".";
            }
            else
            {
                msg = null;
            }

            return msg;
        }

        public bool DeleteFn(long sId)
        {
            var UserId = User.Identity.GetUserId();
            Deliverynote Dvnote = db.Deliverynotes.Find(sId);
            var mcanddeliverystockeffect = db.EnableSettings.Where(a => a.EnableType == "mcanddeliverystockeffect").FirstOrDefault();
            var enmcanddeliverystockeffect = mcanddeliverystockeffect != null ? mcanddeliverystockeffect.Status : Status.inactive;

            if (enmcanddeliverystockeffect == Status.active)
            {
                var stocktanferid = db.StockTransfers.Where(o => o.Voucher == "Delivery Note , Voucher no:" + Dvnote.DvNo).Select(o => o.Id).FirstOrDefault();

                Deletetocktransfer(stocktanferid);
                      }
            var Dvitem = db.DvItems.Where(a => a.Dv == sId).FirstOrDefault();
            if (Dvitem != null)
            {
                db.DvItems.RemoveRange(db.DvItems.Where(a => a.Dv == sId));
            }

            var ConDel = db.ConvertTransactionss.Where(a => a.To == sId && a.ConvertTo == "DVNote").FirstOrDefault();
            if (ConDel != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == sId && a.ConvertTo == "DVNote"));
            }
            var HireItem = db.HireDetails.Where(a => a.Reference == sId && a.Section == "Delivernote").FirstOrDefault();
            if (HireItem != null)
            {
                db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == sId && a.Section == "Delivernote"));

            }

            var appr = db.Approvals.Where(a => a.TransEntry == sId && a.Type == "Deliverynote").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == sId && a.Type == "Deliverynote"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == sId && a.Type == "Deliverynote").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == sId && a.Type == "Deliverynote"));
            }
            db.Deliverynotes.Remove(Dvnote);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "Deliverynote", "Deliverynotes", findip(), Dvnote.DeliverynoteId, "Successfully Deleted Deliverynote");

            return true;
        }

        private string InvoiceNo(Int64 DvNo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == "Hire") ? "HireDelivernote" : "Deliverynote";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            SaleType type = (section != "Hire") ? SaleType.Sale : SaleType.Hire;
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
                if ((db.Deliverynotes.Where(q => q.SaleType == type).Select(p => p.DvNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    DvNo = db.Deliverynotes.Where(q => q.SaleType == type).Max(p => p.DvNo + 1);
                    billNo = companyPrefix + DvNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(DvNo, billNo,section);
                    }
                }
            }
            else
            {
                DvNo = DvNo + 1;
                billNo = companyPrefix + DvNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(DvNo, billNo,section);
                }

            }
            return billNo;
        }
        private bool BillExist(string DvNo)
        {
            var Exists = db.Deliverynotes.Any(c => c.BillNo == DvNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetDvNo(SaleType type)
        {
            Int64 DvNo = 0;
            string prefix = (type == SaleType.Hire) ? "HireDelivernote" : "Deliverynote";
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if ((db.Deliverynotes.Where(q => q.SaleType == type).Select(p => p.DvNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                DvNo = (number == 0) ? 1 : number;
            }
            else
            {
                DvNo = db.Deliverynotes.Where(a => a.SaleType == type).Max(p => p.DvNo + 1);
            }

            return DvNo;
        }
        [HttpPost]
        public ActionResult GetHireInvoiceNum(string hiretype)
        {
            string hirerate = (hiretype == "Hire") ? InvoiceNo(0, null, hiretype) : InvoiceNo();
            return Json(hirerate);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Deliverynote List")]
        public ActionResult MultipleDeliverynote(long? customer, long? type)
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

            var fromv = "DVNote";
            var Tosales = "Sale";
            var tov = "DvExtend";

            
            var userpermission = User.IsInRole("All Deliverynote Entry");
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var sal = (type == 1) ? SaleType.Sale : (type == 2) ? SaleType.Hire : SaleType.POS;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from b in db.Deliverynotes
                     join a in db.Customers on b.Customer equals a.CustomerID
                     join d in db.Employees on b.DvCashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on b.CreatedUserId equals g.Id
                     join h in db.HireDetails on new { h1 = b.DeliverynoteId, h2 = "Delivernote" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.ConvertTransactionss on new { i1 = b.DeliverynoteId, i2 = "DVNote" }
                     equals new { i1 = i.From, i2 = i.ConvertFrom } into ct
                     from i in ct.DefaultIfEmpty()
                     let qs = db.ConvertTransactionss.Where(ap => ap.From == b.DeliverynoteId && ap.ConvertFrom == fromv && ap.ConvertTo == Tosales).FirstOrDefault()
                     let hoe = db.ConvertTransactionss.Where(ap => ap.From == b.DeliverynoteId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()
                     // let mc = db.MCs.Where(x => x.AssignedUser == b.CreatedUserId).Select(x => x.MCId).FirstOrDefault()
                     where(b.DeliverynoteId!=i.From) 
                     && (type==0 || sal==b.SaleType)
                     && (customer == null || customer == 0 || customer==b.Customer)
                     select new
                     {
                         SaleConvert = (qs != null) ? qs.ConvertTo : "",
                         b.DeliverynoteId,
                         HExtent = hoe.ConvertFrom,
                         b.DvNo,
                         b.BillNo,
                         b.DvDate,
                         b.DvItems,
                         b.DvItemQuantity,
                         b.DvDiscount,
                         b.DvGrandTotal,
                         b.DvTax,
                         b.DvValidity,
                         b.DvTaxAmount,
                         EmpName = d.FirstName + " " + d.LastName,
                         Customer = a.CustomerCode + " - " + a.CustomerName,
                         User = g.UserName,
                         validity = (DateTime.Now <= b.DvDate.AddDays((b.DvValidity == null) ? 0 : (b.DvValidity.Value + 1))) ? "Active" : "Expired",
                         // EF Core can't translate the EF6 SqlFunctions.StringConvert shim inside a correlated
                         // subquery; long.ToString() translates to CONVERT(varchar,...) = STR(..).Trim() for ids.
                         convertSale = db.SalesEntrys.Where(ab => ab.ConvertType == "DVNote" && ab.ConvertNo == b.DeliverynoteId.ToString()).Select(ab => ab.BillNo).FirstOrDefault(),
                         test = b.DeliverynoteId.ToString(),
                         b.Location,
                         a.Remark,
                         SaleType = b.SaleType
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
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [HttpPost]
        public JsonResult multidvnote(long[] bill)
        {
            int recordsTotal = 0;
            var salesentry = new SalesEntryViewModel();
            
            IEnumerable<MultiDvItemViewModel> itemlist = new List<MultiDvItemViewModel>();
            var Custmrchk= custcheck(bill);
            if (Custmrchk == true)
            {                
                foreach (var arr in bill)
                {
                    var type = "dvnote";
                    Deliverynote dventry = db.Deliverynotes.Find(arr);
                    var Exists = db.SalesEntrys.Any(r => r.ConvertNo.Contains(arr.ToString()) && r.ConvertType == "DVNote");//(x => x.ConvertNo == dventry.DeliverynoteId.ToString());
                    if (1==1)
                    {
                        var Ditems = db.DvItems.Where(x => x.Dv == dventry.DeliverynoteId).Select (y=>y.Item).ToList();
                        var v = (from f in db.DvItems
                                 join g in db.Deliverynotes on f.Dv equals g.DeliverynoteId
                                 join h in db.Customers on g.Customer equals h.CustomerID into sec
                                 from h in sec.DefaultIfEmpty()
                                 join b in db.Items on f.Item equals b.ItemID
                                 join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                 from c in primary.DefaultIfEmpty()
                                 join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                 from d in second.DefaultIfEmpty()
                                 join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                                 from e in cat.DefaultIfEmpty()
                                 where Ditems.Contains(b.ItemID) && (f.Dv == arr) &&  f.ItemNote != "-:{Bundle_Item}"
                                 select new MultiDvItemViewModel
                                 {
                                     Item = f.Item,
                                     ItemQuantity = f.ItemQuantity,
                                     ItemUnit = f.ItemUnit,
                                     ItemUnitPrice = f.ItemUnitPrice,
                                     ItemTax = f.ItemTax,
                                     ItemSubTotal = f.ItemSubTotal,
                                     ItemTaxAmount = f.ItemTaxAmount,
                                     ItemDiscount = f.ItemDiscount,
                                     ItemTotalAmount = f.ItemTotalAmount,
                                     ItemCode = b.ItemCode,
                                     note = f.ItemNote.Replace("<br />", "\n"),
                                     ItemName = b.ItemName,
                                     ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                     ItemUnitID = b.ItemUnitID,
                                     SubUnitId = b.SubUnitId,
                                     PriUnit = c.ItemUnitName,
                                     SubUnit = d.ItemUnitName,
                                     BasePrice = b.BasePrice,
                                     SellingPrice = b.SellingPrice,
                                     PurchasePrice = b.PurchasePrice,
                                     MRP = b.MRP,
                                     Customer=g.Customer,
                                     Custname=h.CustomerName,
                                     CustCode=h.CustomerCode,
                                     DVInvoice=g.BillNo
                                 }).ToList();
                        itemlist = itemlist.Union(v);
                    }
                }

                var result = itemlist
                     .GroupBy(p => new { p.Item, p.ItemUnit })
                     .Select(g => new MultiDvItemViewModel
                     {
                         Item = g.First().Item,
                         ItemName = g.First().ItemName,
                         note= g.First().note,
                         ItemQuantity = g.Sum(i => i.ItemQuantity),
                         ItemWithCode = g.First().ItemWithCode,
                         ItemUnitPrice = g.First().ItemUnitPrice,
                         ItemSubTotal = g.Sum(i => i.ItemSubTotal),
                         ItemUnit = g.First().ItemUnit,
                         ItemUnitID = g.First().ItemUnitID,
                         SubUnitId = g.First().SubUnitId,
                         PriUnit = g.First().PriUnit,
                         SubUnit = g.First().SubUnit,
                         ItemTaxAmount = g.Sum(i => i.ItemTaxAmount),
                         ItemTax = g.First().ItemTax,
                         ItemDiscount = g.Sum(i => i.ItemDiscount),
                         ItemTotalAmount = g.Sum(i => i.ItemTotalAmount),
                         Dvnum = bill,
                     }).ToList();

                var custmrname= itemlist.Select(x => x.Custname).FirstOrDefault();
                var custmrcode = itemlist.Select(x => x.CustCode).FirstOrDefault();
                var customer = itemlist.Select(x => x.Customer).FirstOrDefault();
                var customername = custmrname + "-" + custmrcode;
                var Invoice = itemlist.Select(x=>new { x.DVInvoice }).Distinct().ToList();
                return new QuickSoft.Models.LegacyJsonResult { Data = new { Result= result, Customer=customer,CustName=customername, MultiInv= Invoice } };
            }
            else
            {
                 var result = "";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { Result = result } };
                    }
        }

        private bool custcheck(long[] bill)
        {
            long customer=0;
            bool result=false;
            foreach (var arr in bill)
            {
                long Exists = db.Deliverynotes.Where(c => c.DeliverynoteId == arr).Select(x=>x.Customer).FirstOrDefault();
                if (customer==0)
                {
                    customer = Exists;
                }
                else if(customer == Exists)
                {
                   
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "Deliverynote" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.Deliverynotes.Where(a => a.DeliverynoteId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "Deliverynote").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
          
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
                AppUp.Type = "Deliverynote";

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
                            join d in db.Deliverynotes on b.TransEntry equals d.DeliverynoteId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedUserId equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "Deliverynote"
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
