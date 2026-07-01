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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class WarrantyController : BaseController
    {
        // GET: Warranty
        ApplicationDbContext db;
        Common com;
        public WarrantyController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
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

        public ActionResult GetWarranty(string BillNo, string FromDate, string ToDate, long? customer, long? salesperson)
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



            var userpermission = User.IsInRole("All Quotation Entry");
            var UserId = User.Identity.GetUserId();

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from b in db.WarrantyCertificates
                     join c in db.Customers on b.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()



                     join e in db.Employees on b.WCashier equals e.EmployeeId into emp
                     from e in emp.DefaultIfEmpty()

                         //equals new { i1 = i.Reference, i2 = i.Section } into hir




                         //let qs = db.ConvertTransactionss.Where(ap => ap.From == b.QuotationId && ap.ConvertFrom == fromv && ap.ConvertTo == Tosales).FirstOrDefault()
                         //let pfa = db.ConvertTransactionss.Where(ap => ap.From == b.QuotationId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPFA).FirstOrDefault()
                         //let dvn = db.ConvertTransactionss.Where(ap => ap.From == b.QuotationId && ap.ConvertFrom == fromv && ap.ConvertTo == ToDVN).FirstOrDefault()
                         //let sor = db.ConvertTransactionss.Where(ap => ap.From == b.QuotationId && ap.ConvertFrom == fromv && ap.ConvertTo == ToSO).FirstOrDefault()
                         //let bo = db.ConvertTransactionss.Where(ap => ap.From == b.QuotationId && ap.ConvertFrom == fromv && ap.ConvertTo == ToBoq).FirstOrDefault()

                         //let app = db.Approvals.Where(a => a.TransEntry == b.QuotationId && a.Type == "Quotation").Select(a => a.EmployeeId).ToList()
                         //let AppStatus = db.ApprovalUpdates.Where(a => a.TransEntry == b.QuotationId && a.Type == "Quotation").Select(a => a.ApprovalStatus).ToList()
                         //let chkAppStatus = db.ApprovalUpdates.Where(a => a.TransEntry == b.QuotationId && a.Type == "Quotation").GroupBy(l => l.ApprovedBy)
                         //                   .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                         //                   .ToList().Select(a => a.ApprovalStatus).ToList()


                     where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                     (customer == 0 || customer == null || b.Customer == customer) &&
                     (salesperson == 0 || salesperson == null || e.EmployeeId == salesperson) &&
                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(b.WDate, fdate) <= 0) &&
                     (ToDate == "" || FromDate == null || EF.Functions.DateDiffDay(b.WDate, tdate) >= 0)
                     //&& (Stats == null || b.Status == st)
                     ////&& (user == null || user == "" || h.Id == user)
                     //&& (Validity == null || Validity == 0 || b.QuotValidity == Validity)
                     ////&& (userpermission == true || b.CreatedUserId == UserId)
                     //&& (Saletype == "" || Saletype == null || St == b.SaleType) && (HireType == 0 || HireType == null || HireType == i.HireType)
                     //&& (Task == 0 || Task == null || j.ProTaskId == Task)
                     select new
                     {
                         b.WarrantyId,
                         b.BillNo,
                         b.WDate,
                         b.WItems,
                         b.WDiscount,
                         b.WGrandTotal,
                         b.WTax,
                         b.WTaxAmount,
                         b.WItemQuantity,
                         Customer = c.CustomerCode + " - " + c.CustomerName,


                     }).ToList().Select(o => new
                     {

                         o.WarrantyId,
                         o.BillNo,
                         o.WDate,
                         o.WItems,
                         o.WDiscount,
                         o.WGrandTotal,
                         o.WTax,
                         o.WTaxAmount,
                         o.WItemQuantity,

                         o.Customer,

                     });



            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower())
                                 //p.Customer.ToString().ToLower().Contains(search.ToLower())
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
        public ActionResult GetItems(long QuoteEntryID)
        {
            var ConD = (from a in db.WItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Warranty == QuoteEntryID && a.itemNote != "-:{Bundle_Item}"
                        orderby a.WItemsId
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
                            a.WarrantyPeriod,
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
                            b.MRP
                        });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }

        [HttpGet]
        public JsonResult GetWBillSundry(long quoteID)
        {
            var QtBs = (from a in db.WBillSundries
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.Warranty == quoteID
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
        public ActionResult Edit(long? id)
        {
            ViewBag.Id = id;

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
            WarrantyCertificate quentry = db.WarrantyCertificates.Where(x => (userpermission == true) && x.WarrantyId == id).FirstOrDefault();

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
                }).Where(o=>o.CustomerID==quentry.Customer).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var leads = db.Customers
    .Where(o => o.Type == CRMCustomerType.Leads)
  .Select(s => new
  {
      CustomerID = s.CustomerID,
      CustomerDetails = s.CustomerCode + " - " + s.CustomerName
  }).Where(o => o.CustomerID == quentry.Customer).ToList();
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
            ViewBag.Templates = QkSelect.List(
            new List<SelectListItem>
            {
              new SelectListItem { Selected = false},
            }, "Value", "Text", 1);


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

            vmodel = (from b in db.WarrantyCertificates
                      where b.WarrantyId == id
                      select new QuotationViewModel
                      {
                          //QuotNo = b.QuotNo,
                          Customer = b.Customer,
                          QuotDate = b.WDate,
                          BillNo = b.BillNo,
                          QuotCashier = b.WCashier,
                          QuotDiscount = b.WDiscount,
                          QuotGrandTotal = b.WGrandTotal,
                          TermsCondition = b.WNote,
                          //QuotValidity = b.QuotValidity,
                          //QuotationType = b.quotationtype,
                          //Remarks = b.Remarks,
                          //Branch = b.Branch,
                          //Project = b.Project != null ? b.Project : null,
                          //SaleType = b.SaleType,
                          //FromDate = f.StartDate,
                          //ToDate = f.EndDate,
                          //HireType = f.HireType,
                          //SalesType = b.SalesType,
                          //PaymentTerms = b.PaymentTerms,
                          //ProTask = b.ProTask,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          InvoiceNo=b.InvoiceNo,
                          //quotationexpdate = b.expdate,
                          //lead = b.leadsid,
                          //revision = b.revision
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
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Warrenty" && a.Status == Status.active).ToList();

               var ref1 = db.WarrantyCertificates
            .Select(s => new
            {
                ID = s.Ref1,
                Name = s.Ref1
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", vmodel.Ref1);

            var ref2 = db.WarrantyCertificates
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", vmodel.Ref2);

            var ref3 = db.WarrantyCertificates
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", vmodel.Ref3);

            var ref4 = db.WarrantyCertificates
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", vmodel.Ref4);

            var ref5 = db.WarrantyCertificates
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", vmodel.Ref5);



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
        [HttpPost]
        public JsonResult Edit(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {
            bool stat = false;
            string msg;
            if (quotdata[16] != null)
            {
                Int64 checkid = 0;
                Int64 quotEntryId = Convert.ToInt64(quotdata[16]);
                WarrantyCertificate Quoentry = db.WarrantyCertificates.Find(quotEntryId);
                WItems itms = db.WItems.Find(quotEntryId);
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


                    Quoentry.BillNo = quotdata[15];
                    Quoentry.WDate = DateTime.Parse(quotdata[2], new CultureInfo("en-GB"));
                    Quoentry.WCashier = quotdata[1] != "" ? Convert.ToInt64(quotdata[1]) : 0;
                    Quoentry.Customer = Convert.ToInt64(quotdata[0]);
                    Quoentry.WItems = Convert.ToInt32(quotdata[3]);
                    Quoentry.WItemQuantity = (quotdata[4] == "NaN") ? 0 : Convert.ToDecimal(quotdata[4]);
                    Quoentry.WSubTotal = Convert.ToDecimal(quotdata[8]);
                    Quoentry.WTax = Convert.ToDecimal(quotdata[9]);
                    Quoentry.WTaxAmount = Convert.ToDecimal(quotdata[5]);
                    Quoentry.WDiscount = Convert.ToDecimal(quotdata[6]);
                    Quoentry.WGrandTotal = Convert.ToDecimal(quotdata[7]);
                    Quoentry.WNote = Convert.ToString(quotdata[11]);
                    Quoentry.Ref1 = Convert.ToString(quotdata[28]);
                    Quoentry.Ref2 = Convert.ToString(quotdata[29]);
                    Quoentry.Ref3 = Convert.ToString(quotdata[30]);
                    Quoentry.Ref4 = Convert.ToString(quotdata[31]);
                    Quoentry.Ref5 = Convert.ToString(quotdata[32]);
                    Quoentry.InvoiceNo= Convert.ToString(quotdata[39]);

                    db.Entry(Quoentry).State = EntityState.Modified;
                    db.SaveChanges();

                    ////// add to SEItem
                    var itm = db.WItems.Where(a => a.Warranty == quotEntryId).FirstOrDefault();
                    if (itm != null)
                    {
                        db.WItems.RemoveRange(db.WItems.Where(a => a.Warranty == quotEntryId));
                        db.SaveChanges();
                    }



                    var QtBs = db.WBillSundries.Where(a => a.Warranty == quotEntryId).FirstOrDefault();
                    if (QtBs != null)
                    {
                        db.WBillSundries.RemoveRange(db.WBillSundries.Where(a => a.Warranty == quotEntryId));
                        db.SaveChanges();
                    }
                    if (bsmodel != null)
                    {
                        foreach (var bs in bsmodel)
                        {
                            var qtB = new WBillSundry
                            {
                                Warranty = quotEntryId,
                                BillSundry = bs.BillSundry,
                                BsValue = bs.BsValue,
                                AmountType = bs.AmountType,
                                BsType = bs.BsType,
                                BsAmount = bs.BsAmount,
                            };
                            db.WBillSundries.Add(qtB);
                            db.SaveChanges();

                        }
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


                        string qedate = Quoentry.WDate.ToString("dd-MM-yyyy");

                        var QuotData = com.WarrantyCertificateData(quotEntryId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                        var item = QuotData.pdfItem.ToList();
                        var summary = QuotData;
                        var billsundry = QuotData.billsundry.ToList();

                        var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                        var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                        var def = (PriLay == Status.active) ? Convert.ToInt64(quotdata[34]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
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
                        string InvoiceNo = "_Quote_" + Quoentry.BillNo;

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

                        msg = "Successfully Updated WorkCompletion.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = quotEntryId } };
                    }
                    else
                    {
                        msg = "Successfully Updated WorkCompletion.";
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


        private bool BillExist(string QENo)
        {
            var Exists = db.WarrantyCertificates.Any(c => c.BillNo == QENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        public ActionResult Create(long? id, string type, long? protaskid)
        {
            var Quotentry = new QuotationViewModel
            {
                BillNo = InvoiceNo2(),
                QuotDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),

                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "quote").Select(a => a.TermsCondit).FirstOrDefault()
            };

            if (id != null)
            {
                if (type == "Sales")
                {
                    SalesEntry Sentry = db.SalesEntrys.Find(id);
                    if (Sentry == null)
                    {
                        return NotFound();
                    }
                    Quotentry.Customer = Sentry.Customer;
                }
            }
            //      .Select(s => new
            //          ID = s.EmailTemplateID,
            //          Name = s.EmailBody
            //      })
            ViewBag.Templates = QkSelect.List(
   new List<SelectListItem>
   {
                                    new SelectListItem { Selected = false},
   }, "Value", "Text", 1);


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
              }).Take(1).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var leads = db.Customers
                .Where(o => o.Type == CRMCustomerType.Leads)
              .Select(s => new
              {
                  CustomerID = s.CustomerID,
                  CustomerDetails = s.CustomerCode + " - " + s.CustomerName
              }).Take(1).ToList();
            ViewBag.leads = QkSelect.List(leads, "CustomerID", "CustomerDetails");
            var initial = new SelectFormat() { id = 0, text = "All" };
            var use2 = db.SalesEntrys.Select(s => new SelectFormat { id = s.SalesEntryId, text = s.BillNo }).ToList();

            use2.Insert(0, initial);

            long[] selmc2 = { };

            ViewBag.Item = new MultiSelectList(use2, "id", "text", selmc2);
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
            SalesEntry quentry = db.SalesEntrys.Find(id);
            if (id != null)
            {
                //duplicate quotation
                if (type == "Sales")
                {
                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    Quotentry.ConTypeId = quentry.SalesEntryId;
                    Quotentry.ConType = type;
                    Quotentry.QuotationId = id;
                    Quotentry.QuotCashier = quentry.SECashier;
                    Quotentry.Customer = quentry.Customer;
                    Quotentry.Remarks = quentry.Remarks;
                    Quotentry.Branch = quentry.Branch;
                    Quotentry.SaleType = quentry.SaleType;
                    Quotentry.SalesType = quentry.SalesType;
                    Quotentry.PaymentTerms = quentry.PaymentTerms;
                    Quotentry.Project = quentry.Project;
                    Quotentry.ProTask = quentry.ProTask;
                    Quotentry.SalesTypes = db.SalesTypes.ToList();
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales").FirstOrDefault();
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
            Quotentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Warrenty" && a.Status == Status.active).ToList();

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
            Quotentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Warrenty" && a.Status == Status.active).ToList();
            return View(Quotentry);

        }
        [HttpPost]
        public JsonResult Create(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {




            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var f = BillExist(quotdata[15]);
                if (f == true)
                {
                    stat = false;
                    msg = "Same WorkCompletion Number Already Exists. please increase WorkCompletion No";
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
                WarrantyCertificate Quoentry = new WarrantyCertificate();
                if (quotdata[19] != null)
                {
                    string str = quotdata[19];
                    SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                }
                else
                {
                }
                if (quotdata[40] != "autosave")
                {
                    Quoentry.BillNo = quotdata[15];
                }
                else
                {
                    Quoentry.BillNo = quotdata[15];
                }


                Quoentry.WDate = DateTime.Parse(quotdata[2], new CultureInfo("en-GB"));
                Quoentry.WCashier = quotdata[1] != "" ? Convert.ToInt64(quotdata[1]) : 0;
                Quoentry.Customer = Convert.ToInt64(quotdata[0]);
                Quoentry.WItems = Convert.ToInt32(quotdata[3]);
                Quoentry.WItemQuantity = (quotdata[4] == "NaN") ? 0 : Convert.ToDecimal(quotdata[4]);
                Quoentry.WSubTotal = Convert.ToDecimal(quotdata[8]);
                Quoentry.WTax = Convert.ToDecimal(quotdata[9]);
                Quoentry.WTaxAmount = Convert.ToDecimal(quotdata[5]);
                Quoentry.WDiscount = Convert.ToDecimal(quotdata[6]);
                Quoentry.WGrandTotal = Convert.ToDecimal(quotdata[7]);
                Quoentry.WNote = Convert.ToString(quotdata[11]); ;



                Quoentry.Ref1 = Convert.ToString(quotdata[29]);
                Quoentry.Ref2 = Convert.ToString(quotdata[30]);
                Quoentry.Ref3 = Convert.ToString(quotdata[31]);
                Quoentry.Ref4 = Convert.ToString(quotdata[32]);
                Quoentry.Ref5 = Convert.ToString(quotdata[33]);
                Quoentry.InvoiceNo = Convert.ToString(quotdata[41]);
                ////Quoentry.quotationtype = Convert.ToInt32()



                db.WarrantyCertificates.Add(Quoentry);
                db.SaveChanges();
                Int64 quotationId = 0;
                Int64 checkid = 0;
                string check = "true";

                quotationId = Quoentry.WarrantyId;




                ////// add to SEItem





                ////// execute sp sql 
                ////// execute sql 

                if (bsmodel != null)
                {
                    foreach (var bs in bsmodel)
                    {
                        var qtB = new WBillSundry
                        {
                            Warranty = quotationId,
                            BillSundry = bs.BillSundry,
                            BsValue = bs.BsValue,
                            AmountType = bs.AmountType,
                            BsType = bs.BsType,
                            BsAmount = bs.BsAmount,
                        };
                        db.WBillSundries.Add(qtB);
                        db.SaveChanges();

                    }
                }





                //Approved By








                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "Quot" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;


                    string qedate = Quoentry.WDate.ToString("dd-MM-yyyy");

                    var QuotData = com.WarrantyCertificateData(quotationId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
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
                    string InvoiceNo = "_Quote_" + Quoentry.BillNo;

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
        private string InvoiceNo2(Int64 PENo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == "Hire") ? "CrossHireInvoices" : "WorkCompletion";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            PurchaseHireType type = (section != "Hire") ? PurchaseHireType.Purchase : PurchaseHireType.CrossHire;

            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
                var num = db.WarrantyCertificates.OrderByDescending(p => p.WarrantyId).Select(p => p.BillNo).FirstOrDefault();
                if (num == null)
                {
                    billNo = Convert.ToString(1);
                }
                else
                {
                    var num2 = Convert.ToInt64(num);
                    var num3 = num2 + 1;
                    billNo = Convert.ToString(num3);
                }
            }
            else
            {


            }
            return billNo;
        }
    }
}
