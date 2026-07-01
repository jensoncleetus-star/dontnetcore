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
    public class MRNoteController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public MRNoteController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: MRNote
        //
        [QkAuthorize(Roles = "Dev,MRNote List")]
        public ActionResult Index()
        {
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.SalesExecutive = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);

            ViewBag.MRType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value=null},
                new SelectListItem() {Text = "Against PO", Value="0"},
                new SelectListItem() {Text = "Direct", Value="1"},
            }, "Value", "Text");

            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            var MlaMRNote = db.EnableSettings.Where(a => a.EnableType == "MLAMRNote").FirstOrDefault();
            var MlaMRNotes = MlaMRNote != null ? MlaMRNote.Status : Status.inactive;
            ViewBag.MLAMRNote = MlaMRNotes;
            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");
            _FinancialYear();

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindMRNote").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,MRNote List")]
        public ActionResult GetMRNote(string BillNo, string FromDate, string ToDate, long? supplier, long? salesperson, string type, string user, string appstat)
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

            var userpermission = User.IsInRole("All MRNote Entry");
            var UserId = User.Identity.GetUserId();

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

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var fromv = "MRNote";
            var ToPE = "Purchase";
            var MRNotetToPEntry = db.EnableSettings.Where(a => a.EnableType == "MRNotetToPEntry").FirstOrDefault();
            var MRNotetToPEntrys = MRNotetToPEntry != null ? MRNotetToPEntry.Status : Status.inactive;

            var uDev = User.IsInRole("Dev");
            var uMRNoteView = User.IsInRole("View MRNote");
            var uEdit = User.IsInRole("Edit MRNote");
            var uDownload = User.IsInRole("Download MRNote");
            var uDelete = User.IsInRole("Delete MRNote");

            var serverQuery = (from b in db.MaterialReceiveNotes
                     join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                     from c in supp.DefaultIfEmpty()
                     join e in db.Employees on b.Cashier equals e.EmployeeId into emp
                     from e in emp.DefaultIfEmpty()
                     join g in db.Users on b.CreatedUserId equals g.Id

                     let pe = db.ConvertTransactionss.Where(ap => ap.From == b.MRId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPE).FirstOrDefault()
                     // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) can't be translated by
                     // EF Core 10 inside this projection — computed CLIENT-side after materialization via lookups
                     // keyed by MRId (same split as EstimateController.GetEstimate).
                     where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                   (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.MRDate, fdate) <= 0) &&
                   (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.MRDate, tdate) >= 0) &&
                   (supplier == 0 || c.SupplierID == supplier) &&
                   (type == null || type=="" || b.Type == type) &&
                   (salesperson == 0 || e.EmployeeId == salesperson)
                   && (user == null || user == "" || g.Id == user)
                   && (userpermission == true || b.CreatedUserId == UserId)
                     select new
                     {
                         PEConvert = (pe != null) ? pe.ConvertTo : "",
                         b.MRId,
                         b.MRNo,
                         b.BillNo,
                         b.MRDate,
                         b.RequestedDate,
                         b.Type,

                         b.MRNItems,
                         b.MRNQuantity,
                         b.Remarks,
                         EmpName = e.FirstName + " " + e.LastName,
                         Supplier = c.SupplierCode + " - " + c.SupplierName,
                         user = g.UserName,
                         Dev = uDev,
                         Details = uMRNoteView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         b.CreatedDate
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BillNo","CreatedDate","Delete","Details","Dev","Download","Edit","EmpName","MRDate","MRId","MRNItems","MRNo","MRNQuantity","PEConvert","Remarks","RequestedDate","Supplier","Type","user" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("MRId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side approval lookups keyed by MRId (missing key -> empty/absent, no KeyNotFound).
            var mrIds = serverRows.Select(o => o.MRId).ToList();
            // app = approver EmployeeIds for the MRNote (keyed by TransEntry == MRId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "MRNote" && mrIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "MRNote" && mrIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per MRNote.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.MRId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.MRId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.MRId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.PEConvert,
                         o.MRId,
                         o.MRNo,
                         o.BillNo,
                         o.MRDate,
                         o.RequestedDate,
                         o.Type,

                         o.MRNItems,
                         o.MRNQuantity,

                         o.Remarks,
                         o.EmpName,
                         o.Supplier,
                         o.user,
                         o.Dev ,
                         o.Details ,
                         o.Edit,
                         o.Download,
                         Delete = uDelete,
                         app = app,
                         Approval = app.Count==0?true:((app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false),
                         ApprovalStatus =app.Count==0 ? ApprovalStatus.Approved:((app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval),
                         o.CreatedDate,
                         MRNotetToPEntry = (o.PEConvert != "" && MRNotetToPEntrys == Status.active) ? false : true,

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
                v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower()));
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

            var fromv = "MRNote";
            var Tosales = "Purchase";
            var tov = "MRnExtend";


            var userpermission = true ;
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
            var v = (from b in db.MaterialReceiveNotes
                     join a in db.Suppliers on b.Supplier equals a.SupplierID
                     join d in db.Employees on b.Cashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     
                     join i in db.ConvertTransactionss on new { i1 = b.MRId, i2 = "MRNote" }
                     equals new { i1 = i.From, i2 = i.ConvertFrom } into ct
                     from i in ct.DefaultIfEmpty()
                     let qs = db.ConvertTransactionss.Where(ap => ap.From == b.MRId && ap.ConvertFrom == fromv && ap.ConvertTo == Tosales).FirstOrDefault()
                    // let hoe = db.ConvertTransactionss.Where(ap => ap.From == b.MRId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()
                     // let mc = db.MCs.Where(x => x.AssignedUser == b.CreatedUserId).Select(x => x.MCId).FirstOrDefault()
                     where (b.MRId != i.From)
     
                     && (customer == null || customer == 0 || customer == b.Supplier)
                     select new
                     {
                         SaleConvert = (qs != null) ? qs.ConvertTo : "",
                         b.MRId,
                         HExtent = "MRN",
                         b.MRNo,
                         b.BillNo,
                         b.MRDate,
                         b.MRNItems,
                         b.MRNQuantity,
                       
                         EmpName = d.FirstName + " " + d.LastName,
                         Customer = a.SupplierCode + " - " + a.SupplierName,
                      
                         a.Remark,
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
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        private bool custcheck(long[] bill)
        {
            long customer = 0;
            bool result = false;
            foreach (var arr in bill)
            {
                long Exists = db.MaterialReceiveNotes.Where(c => c.MRId == arr).Select(x => x.Supplier).FirstOrDefault();
                if (customer == 0)
                {
                    customer = Exists;
                }
                else if (customer == Exists)
                {

                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        [HttpPost]
        public JsonResult multidvnote(long[] bill)
        {
            int recordsTotal = 0;
            var salesentry = new PurchaseEntryViewModel();

            IEnumerable<MultiDvItemViewModel> itemlist = new List<MultiDvItemViewModel>();
            var Custmrchk = custcheck(bill);
            if (Custmrchk == true)
            {
                foreach (var arr in bill)
                {
                    var type = "MRNote";
                    MaterialReceiveNote dventry = db.MaterialReceiveNotes.Find(arr);
                    if (1 == 1)
                    {
                        var Ditems = db.MRNoteItems.Where(x => x.MRNote == dventry.MRId).Select(y => y.Item).ToList();
                        var v = (from f in db.MRNoteItems
                                 join g in db.MaterialReceiveNotes on f.MRNote equals g.MRId
                                 join h in db.Suppliers on g.Supplier equals h.SupplierID into sec
                                 from h in sec.DefaultIfEmpty()
                                 join b in db.Items on f.Item equals b.ItemID
                                 join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                 from c in primary.DefaultIfEmpty()
                                 join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                 from d in second.DefaultIfEmpty()
                                 join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                                 from e in cat.DefaultIfEmpty()
                                 where Ditems.Contains(b.ItemID) && (f.MRNote == arr) && f.ItemNote != "-:{Bundle_Item}"
                                 select new MultiDvItemViewModel
                                 {
                                     Item = f.Item,
                                     ItemQuantity = f.ItemQuantity,
                                     ItemUnit = f.ItemUnit,
                                    
                                
                                   
                                     ItemDiscount = f.ItemDiscount,
                                    
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
                                     Customer = g.Supplier,
                                     Custname = h.SupplierName,
                                     CustCode = h.SupplierCode,
                                     DVInvoice = g.BillNo
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
                         note = g.First().note,
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

                var custmrname = itemlist.Select(x => x.Custname).FirstOrDefault();
                var custmrcode = itemlist.Select(x => x.CustCode).FirstOrDefault();
                var customer = itemlist.Select(x => x.Customer).FirstOrDefault();
                var customername = custmrname + "-" + custmrcode;
                var Invoice = itemlist.Select(x => new { x.DVInvoice }).Distinct().ToList();
                return new QuickSoft.Models.LegacyJsonResult { Data = new { Result = result, Customer = customer, CustName = customername, MultiInv = Invoice } };
            }
            else
            {
                var result = "";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { Result = result } };
            }
        }

        [QkAuthorize(Roles = "Dev,MRNote Entry")]
        public ActionResult Create(long? id, string type)
        {
            var PartialMaterial = db.EnableSettings.Where(a => a.EnableType == "PartialMaterialConversion").FirstOrDefault();
            var ViewPartialMaterial = PartialMaterial != null ? PartialMaterial.Status : Status.inactive;
            ViewBag.ViewPartialMaterial = ViewPartialMaterial;
            var POentry = new MRNoteViewModel
            {
                BillNo = InvoiceNo(),
                MRDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "mrnote").Select(a => a.TermsCondit).FirstOrDefault(),
            };
            if (id != null)
            {
                if (type == "POrder")
                {

                    PurchaseOrder porder = db.PurchaseOrders.Find(id);
                    if (porder == null)
                    {
                        return NotFound();
                    }
                    POentry.ConTypeId = porder.PurchaseOrderId;
                    POentry.ConType = type;

                    POentry.MRDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    POentry.Cashier = porder.POCashier;
                    POentry.Supplier = porder.Supplier;
                    POentry.CPorderNo = porder.PONo;
                    var CFrom = db.ConvertTransactionss.Where(a => a.To == porder.PurchaseOrderId && a.ConvertFrom == "PQuote" && a.ConvertTo =="POrder").Select(a => a.From).FirstOrDefault();
                    POentry.CPQuotNo = db.PurchaseQuotations.Where(a => a.PQuotationId == CFrom).Select(a => a.PQuotNo).FirstOrDefault();

                    var SFrom = db.ConvertTransactionss.Where(a => a.To == CFrom && a.ConvertFrom == "MOrder" && a.ConvertTo =="PQuote").Select(a => a.From).FirstOrDefault();
                    POentry.CMReqNo = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == SFrom).Select(a => a.MRNo).FirstOrDefault();
                    POentry.Remarks = porder.Remarks;
                    var supp = db.Suppliers.Find(porder.Supplier);
                    POentry.SupplierEmail = db.Contacts.Where(a => a.ContactID == supp.Contact).Select(a => a.EmailId).FirstOrDefault();
                    POentry.Branch = porder.Branch;

                    POentry.convertFrom = type + " No";//label
                    POentry.convertBill = porder.BillNo;

                    POentry.TermsCondition = porder.TermsCondition;

                }
            }
            var sup = db.Suppliers.Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierCode + " - " + s.SupplierName
            }).ToList();
            ViewBag.Supp = QkSelect.List(sup, "SupplierID", "SupplierDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashiers = QkSelect.List(use, "ID", "Name");

            companySet();
            var userpermission = User.IsInRole("All MRNote Entry");
            var UserId = User.Identity.GetUserId();

            ViewBag.LastEntry = db.MaterialReceiveNotes.Where(a => a.CreatedUserId == UserId || userpermission == true).Select(p => p.MRId).AsEnumerable().DefaultIfEmpty(0).Max();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            ViewBag.MRType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Against PO", Value="0"},
                new SelectListItem() {Text = "Direct", Value="1"},
            }, "Value", "Text");

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
            var po = db.PurchaseOrders
              .Select(s => new
              {
                  Id = s.PurchaseOrderId,
                  Name = s.BillNo
              }).ToList();
            ViewBag.POrder = QkSelect.List(po, "Id", "Name");
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      ID = s.EmployeeId,
                      Name = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaMRNote = db.EnableSettings.Where(a => a.EnableType == "MLAMRNote").FirstOrDefault();
            var MlaMRNotes = MlaMRNote != null ? MlaMRNote.Status : Status.inactive;
            ViewBag.MLAMRNote = MlaMRNotes;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            ViewBag.Contype = type;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            //field mapping
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            POentry.FieldMap = db.FieldMappings.Where(a => a.Section == "MRNote" && a.Status == Status.active).ToList();
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
            return View(POentry);
        }

       [QkAuthorize(Roles = "Dev,MRNote Entry")]
        public JsonResult CreateMRNote(string[][] array, string[] podata, string action)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {

                if (!BillExist(Convert.ToString(podata[8])))
                {

                //suppMail, CreatedUserEmail, MRNo, BillNo, Type, SuppType, Remarks,
                // POVal, ApprovedBy, ReqDate, Branch]

                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
                    var mcanddeliverystockeffect= db.EnableSettings.Where(a => a.EnableType == "mcanddeliverystockeffect").FirstOrDefault();
                    var enmcanddeliverystockeffect = mcanddeliverystockeffect != null ? mcanddeliverystockeffect.Status : Status.inactive;
                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(podata[15]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                    var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                    var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                    
                    MaterialReceiveNote MRNote = new MaterialReceiveNote();

                    MRNote.MRNo = GetMRNNo();
                    MRNote.BillNo = Convert.ToString(podata[8]);
                    MRNote.MRDate = DateTime.Parse(podata[2], new CultureInfo("en-GB"));
                    MRNote.Cashier = podata[1] != "" ? Convert.ToInt64(podata[1]) : 0;
                    MRNote.Supplier = Convert.ToInt64(podata[0]);
                    if(podata[28]!=""&& podata[28]!=null)
                    MRNote.materialcenter= Convert.ToInt64(podata[28]);
                    MRNote.MRNQuantity = Convert.ToDecimal(podata[4]);
                    MRNote.Note = "";
                    MRNote.Mail = 0;
                    MRNote.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    MRNote.CreatedUserId = UserId;
                    MRNote.Status = Status.active;
                    MRNote.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "MRNote").Select(a => a.EmailTemplateID).FirstOrDefault();
                    MRNote.CompanyHeaderID = 0;
                    MRNote.Branch = Branch;
                    MRNote.Remarks = podata[11];
                    MRNote.Type = podata[9];
                   
                    MRNote.RequestedDate = (podata[14] != null && podata[14] != "")? DateTime.Parse(podata[14], new CultureInfo("en-GB")):(DateTime?)null;

                    MRNote.TermsCondition = Convert.ToString(podata[21]);

                    MRNote.Ref1 = Convert.ToString(podata[22]);
                    MRNote.Ref2 = Convert.ToString(podata[23]);
                    MRNote.Ref3 = Convert.ToString(podata[24]);
                    MRNote.Ref4 = Convert.ToString(podata[25]);
                    MRNote.Ref5 = Convert.ToString(podata[26]);

                    db.MaterialReceiveNotes.Add(MRNote);
                    db.SaveChanges();
                    Int64 MRId = 0;
                    MRId = MRNote.MRId;

                    //purchaseorderno
                    var poid= Convert.ToString(podata[12]);
                    if (poid != null && poid !="")
                    {
                        long[] POrder = poid.Split(',').Select(Int64.Parse).ToArray();
                        MRNotePOrder mrporder = new MRNotePOrder();
                        foreach (var mrn in POrder)
                        {
                            mrporder.MRId = MRId;
                            mrporder.POrderId = mrn;
                            db.MRNotePOrders.Add(mrporder);
                            db.SaveChanges();
                        }
                    }

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    ////// add to SEItem
                    string result = string.Empty;
                    DataTable dtItem = new DataTable();
                    dtItem.Columns.Add("ItemUnit");
                    dtItem.Columns.Add("ItemQuantity");
                    dtItem.Columns.Add("ItemDiscount");
                    dtItem.Columns.Add("Make");
                    dtItem.Columns.Add("Remarks");
                    dtItem.Columns.Add("ProjectId");
                    dtItem.Columns.Add("TaskId");
                    
                    dtItem.Columns.Add("ItemNote");
                    dtItem.Columns.Add("MRNote");
                    dtItem.Columns.Add("Item");

                    List<BOMItem> bomitem=new List<BOMItem>();
                    foreach (var arr in array)
                    {
                        DataRow dr = dtItem.NewRow();
                        BOMItem bom = new BOMItem
                        {

                            ItemId = Convert.ToInt32(arr[0]),
                            Quantity = Convert.ToDecimal(arr[2]),
                            Unit = Convert.ToInt64(arr[1]),
                            BOMItemId=1,
                            BOMId=1

                        
                        
                        

                        };
                        bomitem.Add(bom);
                        dr["ItemUnit"] = arr[1];

                        dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                        dr["ItemDiscount"] = 0;
                        dr["Make"] = Convert.ToInt64(arr[3]);
                        dr["Remarks"] = Convert.ToString(arr[4].Replace("\n", "<br />"));

                        if (ProjChks == Status.active)
                        {
                            dr["ProjectId"] = Convert.ToInt64(arr[5]);
                            dr["TaskId"] = Convert.ToInt64(arr[6]);
                            dr["ItemNote"] = Convert.ToString(arr[7].Replace("\n", "<br />"));
                        }
                        else
                        {
                            dr["ProjectId"] = 0;
                            dr["TaskId"] = 0;
                            dr["ItemNote"] = Convert.ToString(arr[5].Replace("\n", "<br />"));
                        }
                        dr["MRNote"] = MRId;
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
                                dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                                dbu["ItemDiscount"] = item;
                                dbu["Make"] = Convert.ToInt64(arr[3]);
                                dbu["Remarks"] = Convert.ToString(arr[4].Replace("\n", "<br />"));
                                if (ProjChks == Status.active)
                                {
                                    dbu["ProjectId"] = Convert.ToInt64(arr[5]);
                                    dbu["TaskId"] = Convert.ToInt64(arr[6]);
                                }
                                else
                                {
                                    dbu["ProjectId"] = 0;
                                    dbu["TaskId"] = 0;
                                }

                                dbu["ItemNote"] = "-:{Bundle_Item}";
                                dbu["MRNote"] = MRId;
                                dbu["Item"] = bu.Item;
                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }

                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeMRNoteItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertMRNoteItems", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql, parameter);

                    if(enmcanddeliverystockeffect==Status.active)
                    {
                        com.stocktransfer("Material Recieve Note , Voucher no:" + MRNote.MRNo, (long)MRNote.materialcenter, UserId, bomitem);
                    }
                    
                    var Appby = Convert.ToString(podata[13]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = MRId;
                            approval.Type = "MRNote";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                    if (podata[16] != null && podata[16] != "0" && podata[16] != "" && podata[17] != null && podata[17] != "" && podata[17] != "0")
                    {

                        ConvertTransactions ConTran = new ConvertTransactions();

                        ConTran.ConvertFrom = podata[17];
                        ConTran.ConvertTo = "MRNote";
                        ConTran.From = Convert.ToInt64(podata[16]);
                        ConTran.To = MRNote.MRId;
                        ConTran.Status = 0;
                        ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        ConTran.CreatedBy = UserId;
                        ConTran.Branch = Convert.ToInt32(BranchID);

                        db.ConvertTransactionss.Add(ConTran);
                        db.SaveChanges();
                        com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Convertion");
                    }
                    com.addlog(LogTypes.Created, UserId, "MaterialReceiveNote", "MaterialReceiveNote", findip(), MRId, "Successfully Submitted Material Receive Note");

                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                    var CPorderNo = podata[18] != "" ? Convert.ToInt64(podata[18]) : 0;
                    var CPQuotNo = podata[19] != "" ? Convert.ToInt64(podata[19]) : 0;
                    var CMReqNo = podata[20] != "" ? Convert.ToInt64(podata[20]) : 0;
                    if (action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "MRNote" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        string qedate = MRNote.MRDate.ToString("dd-MM-yyyy");
                        var EnableProjects = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProjects == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                        var MRNoteData = com.MRNoteData(MRId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, CPorderNo, CPQuotNo, CMReqNo, ComHeadCheck);
                        var item = MRNoteData["item"];
                        var summary = MRNoteData["summary"];
                        var approval = MRNoteData["approval"];

                        var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, approval, layout, fmapp } };
                    }
                    else if (action == "sendmail")
                    {
                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = podata[5];
                        string CcMail = podata[6];
                        string InvoiceNo = "_MRNote_" + MRNote.MRNo;

                        var em = db.EmailTemplates.Where(a => a.Head == "MRNote").FirstOrDefault();
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;
                        }
                        else
                        {
                            message.Subject = "Material Receive Note";
                            message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                                " <p>we are enclosing our Material Receive Note for the items / services as requested by you during our discussions.<br/></p> " +
                                " <p>Looking forward to hear from you.</p>";
                        }
                        sm.SendPdfMail(generatePdf(MRId), ToMail, CcMail, InvoiceNo, message);

                        msg = "Successfully submitted Material Receive Note.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    else
                    {
                        msg = "Successfully submitted Material ReceiveNote";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Material Receive Note No. Already Exists.";
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
        [QkAuthorize(Roles = "Dev,Edit MRNote")]
        public ActionResult Edit(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            var mcanddeliverystockeffect = db.EnableSettings.Where(a => a.EnableType == "mcanddeliverystockeffect").FirstOrDefault();
            var enmcanddeliverystockeffect = mcanddeliverystockeffect != null ? mcanddeliverystockeffect.Status : Status.inactive;

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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All MRNote Entry");
             UserId = User.Identity.GetUserId();
            MaterialReceiveNote mrnote = db.MaterialReceiveNotes.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.MRId == id).FirstOrDefault();

            if (mrnote == null)
            {
                return NotFound();
            }
            MRNoteViewModel vmodel = new MRNoteViewModel();
            vmodel.materialcenter = mrnote.materialcenter;
            var sup = db.Suppliers
                .Select(s => new
                {
                    SupplierID = s.SupplierID,
                    SupplierDetails = s.SupplierCode + " - " + s.SupplierName
                }).ToList();
            ViewBag.Supp = QkSelect.List(sup, "SupplierID", "SupplierDetails");

            ViewBag.MRType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Against PO", Value="0"},
                new SelectListItem() {Text = "Direct", Value="1"},
            }, "Value", "Text");

           

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

            var po = db.MRNotePOrders.Where(a => a.MRId == id).Select(a => a.POrderId).ToList();
            long[] poIds = po.ToArray();

            var stands = db.PurchaseOrders
                       .Select(s => new
                       {
                          FieldID = s.BillNo,
                          FieldName = s.PurchaseOrderId
                       })
                        .ToList();
            ViewBag.POrder = new MultiSelectList(stands, "FieldName", "FieldID", poIds);



            var use = db.Employees
                            .Select(s => new
                            {
                                ID = s.EmployeeId,
                                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                            })
                            .ToList();
            ViewBag.MRCashier = QkSelect.List(use, "ID", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "MRNote").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaMRNote = db.EnableSettings.Where(a => a.EnableType == "MLAMRNote").FirstOrDefault();
            var MlaMRNotes = MlaMRNote != null ? MlaMRNote.Status : Status.inactive;
            ViewBag.MLAMRNote = MlaMRNotes;

            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "MRNote").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "POrder")
                {
                    CBill = db.PurchaseOrders.Where(a => a.PurchaseOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
               
            }

            vmodel = (from b in db.MaterialReceiveNotes
                      where b.MRId == id
                      select new MRNoteViewModel
                      {
                          MRNo = b.MRNo,
                          Supplier = b.Supplier,
                          MRDate = b.MRDate,
                          RequestedDate = b.RequestedDate,
                          BillNo = b.BillNo,
                          Cashier = b.Cashier,
                          Remarks = b.Remarks,
                          Branch = b.Branch,
                          Type = b.Type,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1=b.Ref1,
                          Ref2=b.Ref2,
                          Ref3=b.Ref3,
                          Ref4=b.Ref4,
                          Ref5=b.Ref5,
                          TermsCondition=b.TermsCondition
                      }).FirstOrDefault();
            companySet();
            ViewBag.preEntry = db.MaterialReceiveNotes.Where(a => (a.MRId < id) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.MRId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.MaterialReceiveNotes.Where(a => (a.MRId > id) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.MRId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;
            _FinancialYear();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            ViewBag.PopUpAddCust = false;

            var EditPermission = User.IsInRole("Disable MRNote Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "MRNote", UserId);
            var conv = db.ConvertTransactionss.Any(o => o.From == (long)id && o.ConvertFrom == "MRNote");
            if (conv)
                ViewBag.ChkApp = (conv == true) ? false : true ;
                            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "MRNote" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummyMRNoteItems.Where(a => a.MRNote == id).FirstOrDefault();
            var SItem = db.MRNoteItems.Where(a => a.MRNote == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummyMRNoteItems.Where(a => a.MRNote == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    MRNoteItem sItem = new MRNoteItem();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.Remarks = arr.Remarks;
                    sItem.ItemNote = arr.ItemNote;
                    sItem.MRNote = arr.MRNote;
                    sItem.Item = arr.Item;
                    sItem.ProjectId = arr.ProjectId;
                    sItem.TaskId = arr.TaskId;
                    sItem.Make = arr.Make;

                    db.MRNoteItems.Add(sItem);
                    db.SaveChanges();
                }

                db.DummyMRNoteItems.RemoveRange(db.DummyMRNoteItems.Where(a => a.MRNote == id));
                db.SaveChanges();
            }
           
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Edit MRNote")]
        public JsonResult UpdateMRNote(string[][] array, string[] podata, string action)
        {
            bool stat = false;
            string msg;
            Int64 EntryId = Convert.ToInt64(podata[16]);
            MaterialReceiveNote MRentry = db.MaterialReceiveNotes.Find(EntryId);
            var mcanddeliverystockeffect = db.EnableSettings.Where(a => a.EnableType == "mcanddeliverystockeffect").FirstOrDefault();
            var enmcanddeliverystockeffect = mcanddeliverystockeffect != null ? mcanddeliverystockeffect.Status : Status.inactive;

            if (BillExist(Convert.ToString(podata[8])) && Convert.ToString(podata[8]) != MRentry.BillNo)
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            if (ModelState.IsValid)
            {

                //suppMail, CreatedUserEmail, MRNo, BillNo, Type, SuppType, Remarks,
                // POVal, ApprovedBy, ReqDate, Branch]
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
                    Branch = Convert.ToInt64(podata[15]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }


                var EditPermission = User.IsInRole("Disable MRNote Edit After Approval");
                if (com.chkApproved(EntryId, EditPermission, "MRNote", UserId) == true)
                {


                    MRentry.BillNo = podata[8];
                    MRentry.MRNo = Convert.ToInt64(podata[7]);
                    MRentry.MRDate = DateTime.Parse(podata[2], new CultureInfo("en-GB"));
                    MRentry.Cashier = podata[1] != "" ? Convert.ToInt64(podata[1]) : 0;
                    MRentry.Supplier = Convert.ToInt64(podata[0]);

                    MRentry.MRNItems = Convert.ToInt32(podata[3]);
                    MRentry.MRNQuantity = Convert.ToDecimal(podata[4]);

                    MRentry.Note = "";
                    MRentry.Mail = 0;
                    MRentry.Status = Status.active;

                    MRentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "MRNote").Select(a => a.EmailTemplateID).FirstOrDefault();
                    MRentry.CompanyHeaderID = 0;
                    MRentry.Branch = Branch;
                    MRentry.Remarks = podata[11];
                    MRentry.Type = Convert.ToString(podata[9]);

                    MRentry.RequestedDate = (podata[14] != null && podata[14] != "") ? DateTime.Parse(podata[14], new CultureInfo("en-GB")) : (DateTime?)null;

                    MRentry.TermsCondition = Convert.ToString(podata[17]);

                    MRentry.Ref1 = Convert.ToString(podata[18]);
                    MRentry.Ref2 = Convert.ToString(podata[19]);
                    MRentry.Ref3 = Convert.ToString(podata[20]);
                    MRentry.Ref4 = Convert.ToString(podata[21]);
                    MRentry.Ref5 = Convert.ToString(podata[22]);
                    if (podata[24] != "" && podata[24] != null)
                        MRentry.materialcenter = Convert.ToInt64(podata[24]);
                    db.Entry(MRentry).State = EntityState.Modified;
                    db.SaveChanges();


                    //purchaseorderno
                    var MrnPO = db.MRNotePOrders.Where(a => a.MRId == EntryId).FirstOrDefault();
                    if (MrnPO != null)
                    {
                        db.MRNotePOrders.RemoveRange(db.MRNotePOrders.Where(a => a.MRId == EntryId));
                        db.SaveChanges();
                    }
                    var poid = Convert.ToString(podata[12]);
                    if (poid != null && poid != "")
                    {
                        long[] POrder = poid.Split(',').Select(Int64.Parse).ToArray();
                        MRNotePOrder mrporder = new MRNotePOrder();
                        foreach (var mrn in POrder)
                        {
                            mrporder.MRId = MRentry.MRId;
                            mrporder.POrderId = mrn;
                            db.MRNotePOrders.Add(mrporder);
                            db.SaveChanges();
                        }
                    }



                    var POItem = db.MRNoteItems.Where(a => a.MRNote == EntryId).FirstOrDefault();
                    if (POItem != null)
                    {
                        var SItems = db.MRNoteItems.Where(a => a.MRNote == EntryId).ToList();
                        foreach (var arr in SItems)
                        {
                            //add to dummy table
                            DummyMRNoteItem dItem = new DummyMRNoteItem();
                            dItem.ItemUnit = arr.ItemUnit;
                            dItem.ItemQuantity = arr.ItemQuantity;
                            dItem.ItemDiscount = arr.ItemDiscount;
                            dItem.ProjectId = arr.ProjectId;
                            dItem.TaskId = arr.TaskId;
                            dItem.Make = arr.Make;
                            dItem.ItemNote = arr.ItemNote;
                            dItem.MRNote = arr.MRNote;
                            dItem.Item = arr.Item;
                            db.DummyMRNoteItems.Add(dItem);
                            db.SaveChanges();
                        }

                        db.MRNoteItems.RemoveRange(db.MRNoteItems.Where(a => a.MRNote == EntryId));
                        db.SaveChanges();
                    }

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                    ////// add to SEItem
                    string result = string.Empty;
                    DataTable dtItem = new DataTable();
                    dtItem.Columns.Add("ItemUnit");
                    dtItem.Columns.Add("ItemQuantity");
                    dtItem.Columns.Add("ItemDiscount");
                    dtItem.Columns.Add("Make");
                    dtItem.Columns.Add("Remarks");

                    dtItem.Columns.Add("ProjectId");
                    dtItem.Columns.Add("TaskId");

                    dtItem.Columns.Add("ItemNote");
                    dtItem.Columns.Add("MRNote");
                    dtItem.Columns.Add("Item");
                    List<BOMItem> bomitem = new List<BOMItem>();

                    foreach (var arr in array)
                    {
                        BOMItem bom = new BOMItem
                        {

                            ItemId = Convert.ToInt32(arr[0]),
                            Quantity = Convert.ToDecimal(arr[2]),
                            Unit = Convert.ToInt64(arr[1]),
                            BOMItemId = 1,
                            BOMId = 1





                        };
                        bomitem.Add(bom);

                        DataRow dr = dtItem.NewRow();
                        dr["ItemUnit"] = arr[1];
                        dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                        dr["ItemDiscount"] = 0;
                        dr["Make"] = Convert.ToInt64(arr[3]);
                        dr["Remarks"] = Convert.ToString(arr[4].Replace("\n", "<br />"));
                        if (ProjChks == Status.active)
                        {
                            dr["ProjectId"] = Convert.ToInt64(arr[5]);
                            dr["TaskId"] = Convert.ToInt64(arr[6]);
                            dr["ItemNote"] = Convert.ToString(arr[7].Replace("\n", "<br />"));
                        }
                        else
                        {
                            dr["ProjectId"] = 0;
                            dr["TaskId"] = 0;
                            dr["ItemNote"] = Convert.ToString(arr[5].Replace("\n", "<br />"));
                        }
                        dr["MRNote"] = EntryId;
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
                                dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                                dbu["ItemDiscount"] = item;
                                dbu["Make"] = Convert.ToInt64(arr[3]);
                                dbu["Remarks"] = Convert.ToString(arr[4].Replace("\n", "<br />"));
                                if (ProjChks == Status.active)
                                {
                                    dbu["ProjectId"] = Convert.ToInt64(arr[5]);
                                    dbu["TaskId"] = Convert.ToInt64(arr[6]);
                                }
                                else
                                {
                                    dbu["ProjectId"] = 0;
                                    dbu["TaskId"] = 0;
                                }
                                dbu["ItemNote"] = "-:{Bundle_Item}";
                                dbu["MRNote"] = EntryId;
                                dbu["Item"] = bu.Item;
                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }
                    if (enmcanddeliverystockeffect == Status.active)
                    {
                        var stocktanferid = db.StockTransfers.Where(o => o.Voucher == "Material Recieve Note , Voucher no:" + MRentry.MRNo).Select(o => o.Id).FirstOrDefault();
                       if(stocktanferid!=0&& stocktanferid!=null)
                        Deletetocktransfer(stocktanferid);
                        com.stocktransfer("Material Recieve Note , Voucher no:" + MRentry.MRNo, (long)MRentry.materialcenter, UserId, bomitem);
                    }
                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeMRNoteItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertMRNoteItems", "@TableType");
                    //// execute sql 
                    var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                    if (ret > 0)
                    {
                        db.DummyMRNoteItems.RemoveRange(db.DummyMRNoteItems.Where(a => a.MRNote == EntryId));
                        db.SaveChanges();
                    }

                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == EntryId && a.Type == "MRNote").FirstOrDefault();
                    var Mrn = db.Approvals.Where(a => a.TransEntry == EntryId && a.Type == "MRNote").FirstOrDefault();
                    if (Mrn != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == EntryId && a.Type == "MRNote"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == EntryId && a.Type == "MRNote"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = Convert.ToString(podata[13]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = EntryId;
                            approval.Type = "MRNote";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "MRNote", "MRNotes", findip(), EntryId, "Successfully Updated Material Receive Note");
                }
                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "MRNote" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    string qedate = MRentry.MRDate.ToString("dd-MM-yyyy");

                    var MRNoteData = com.MRNoteData(EntryId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck);
                    var item = MRNoteData["item"];
                    var summary = MRNoteData["summary"];
                    var approval = MRNoteData["approval"];

                    var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, approval, layout , fmapp } };

                }
                else
                {
                    msg = "Successfully Updated Material Receive Note.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
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
        [QkAuthorize(Roles = "Dev,View MRNote")]
        public ActionResult Details(long? id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            MRNoteViewModel vmodel = new MRNoteViewModel();
            vmodel = (from b in db.MaterialReceiveNotes
                      join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                      from c in supp.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.Cashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      where b.MRId == id
                      select new MRNoteViewModel
                      {
                          SupplierName = c.SupplierCode + " - " + c.SupplierName,
                          MRNo = b.MRNo,
                          BillNo = b.BillNo,
                          MRDate = b.MRDate,
                          RequestedDate = b.RequestedDate,

                          EmployeeName = e.FirstName + " " + e.LastName,
                          //QuotCashier = e.Name,
                          Type = (b.Type == "0" ? "Against PO" : "Direct"),
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
                          Ref1 =b.Ref1,
                          Ref2=b.Ref2,
                          Ref3=b.Ref3,
                          Ref4=b.Ref4,
                          Ref5=b.Ref5,
                      }).FirstOrDefault();
            vmodel.MRNItem = db.MRNoteItems.Where(a => a.MRNote == id && a.ItemNote != "-:{Bundle_Item}")
            .Select(b => new MRNoteItemViewModel
            {
                ItemQuantity = b.ItemQuantity,
                ItemNote = b.ItemNote != null ? b.ItemNote : "",
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.MRNoteItems
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.MRNote == id && ab.ItemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();
          

            vmodel.MRNPo = (from b in db.MRNotePOrders
                      join c in db.PurchaseOrders on b.POrderId equals c.PurchaseOrderId into porder
                      from c in porder.DefaultIfEmpty()
                      select new MRNotePOrderViewModel
                      {
                          POrder = c.BillNo,
                      }).Distinct().ToList();

            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "MRNote" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Delete MRNote")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All MRNote Entry");
            var UserId = User.Identity.GetUserId();
            MaterialReceiveNote PO = db.MaterialReceiveNotes.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.MRId == id).FirstOrDefault();

            MaterialReceiveNote Mrn = db.MaterialReceiveNotes.Find(id);
            if (Mrn == null)
            {
                return NotFound();
            }
            return PartialView(Mrn);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete MRNote")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            #region Old Code
            #endregion

            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteMR(id);
                msg = "Successfully deleted Material Receive Note.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete MRNote")]
        public ActionResult DeleteAllMRNote(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteMRNote(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + "Material Receive Note", true);
            return RedirectToAction("Index", "MRNote");
        }
        private Boolean DeleteMRNote(long saleId)
        {
            var Msg = chkDeleteWithMsg(saleId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteMR(saleId);
            }
        }
        private Boolean DeleteMR(long id)
        {
            var UserId = User.Identity.GetUserId();
            MaterialReceiveNote MRnote = db.MaterialReceiveNotes.Find(id);
            var pitem = db.MRNoteItems.Where(a => a.MRNote == id).FirstOrDefault();
            if (pitem != null)
            {
                db.MRNoteItems.RemoveRange(db.MRNoteItems.Where(a => a.MRNote == id));
            }

            var MrnPO = db.MRNotePOrders.Where(a => a.MRId == id).FirstOrDefault();
            if (MrnPO != null)
            {
                db.MRNotePOrders.RemoveRange(db.MRNotePOrders.Where(a => a.MRId == id));
                db.SaveChanges();
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "MRNote").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "MRNote"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "MRNote").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "MRNote"));
            }
            var CMRNote = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "MRNote").FirstOrDefault();
            if (CMRNote != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "MRNote"));
            }
            db.MaterialReceiveNotes.Remove(MRnote);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "MRNote", "MRNotes", findip(), MRnote.MRId, "Successfully Deleted Material Receive Note");

            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            //purchase Entry
            var Ext = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "MRNote" && x.ConvertTo =="Purchase").FirstOrDefault();
            if (Ext != null)
            {
                var inv = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == Ext.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to PurchaseEntry : " + inv + ".";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download MRNote")]
        public ActionResult Download(long id)
        {
            var Data = db.MaterialReceiveNotes.Where(s => s.MRId == id).FirstOrDefault();
            var supname = db.Suppliers.Where(s => s.SupplierID == Data.Supplier).Select(a => a.SupplierName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Material ReceiveNote" + "-" + supname + "-" + billno + ".pdf");
        }
        public StringBuilder generatePdf(long id)
        {
            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var details = (from b in db.MaterialReceiveNotes
                           join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                           from c in supp.DefaultIfEmpty()
                           join d in db.Contacts on c.Contact equals d.ContactID into cnt
                           from d in cnt.DefaultIfEmpty()
                           join e in db.Employees on b.Cashier equals e.EmployeeId into emp
                           from e in emp.DefaultIfEmpty()
                           join g in db.Users on b.CreatedUserId equals g.Id
                           join h in db.Accountss on c.Accounts equals h.AccountsID
                           where b.MRId == id
                           select new
                           {
                               BillNo = b.BillNo,
                               Date = b.MRDate,
                               Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                               Remarks = b.Remarks,
                               RequestedDate = b.RequestedDate,
                               BillId = b.MRNo,
                               CreatedBy = g.UserName,
                               PartyName = c.SupplierCode + "-" + c.SupplierName,
                               Address = d.Address,
                               City = d.City,
                               State = d.State,
                               Country = d.Country,
                               Zip = d.Zip,
                               Email = d.EmailId,
                               Phone = d.Phone,
                               Mobile = (from ac in db.Mobiles
                                         where (ac.Contact == c.Contact)
                                         select new MobileViewModel
                                         {
                                             Num = ac.MobileNum,
                                             Name = ac.Name
                                         }).ToList(),
                               TRN = h.TRN,
                           }).FirstOrDefault();

            var saleitem = (from b in db.MRNoteItems
                            join c in db.Items on b.Item equals c.ItemID

                            join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                            from d in scaffold.DefaultIfEmpty()
                            join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                            from e in punit.DefaultIfEmpty()

                            join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                            from g in bundle.DefaultIfEmpty()
                            join h in db.ItemBrands on b.Make equals h.ItemBrandID into brn
                            from h in brn.DefaultIfEmpty()
                            join p in db.Projects on b.ProjectId equals p.ProjectId into proj
                            from p in proj.DefaultIfEmpty()
                            join t in db.ProTasks on b.TaskId equals t.ProTaskId into protask
                            from t in protask.DefaultIfEmpty()
                            where b.MRNote == id && b.ItemNote != "-:{Bundle_Item}"
                            select new
                            {
                                ItemID=b.Item,
                                ItemNote = b.ItemNote,
                                ItemQuantity = b.ItemQuantity,
                                ItemCode = c.ItemCode,
                                ItemName = c.ItemName,
                                ItemUnit = e.ItemUnitName,
                                PartNumber = c.PartNumber,
                                CBM = d.CBM,
                                Weight = d.Weight,
                                ItemDescription = c.ItemDescription,
                                KeepStock = c.KeepStock,
                                ItemMakeID = b.Make,
                                Make = h.ItemBrandName,
                                Remarks = b.Remarks,
                                p.ProjectName,
                                t.TaskName,
                                FileName = db.ItemImages.Where(a => a.ItemID == b.Item).Select(a => a.FileName).FirstOrDefault(),
                                bundle = (from ay in db.BundleItems
                                          join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                          join bb in db.Items on ay.ItemId equals bb.ItemID
                                          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                          from dd in scaffold.DefaultIfEmpty()
                                          join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                          from eb in bpunit.DefaultIfEmpty()
                                          let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                          where az.mainItem == b.Item
                                          select new
                                          {
                                              Id = bb.ItemID,
                                              ItemUnitPrice = ay.ItemUnitPrice,
                                              ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                              ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                              ItemNote = "",
                                              ItemTax = ay.ItemTax,
                                              ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                              ItemTotalAmount = ay.ItemTotalAmount,
                                              ItemCode = bb.ItemCode,
                                              ItemName = bb.ItemName,
                                              ItemUnit = eb.ItemUnitName,
                                              PartNumber = bb.PartNumber,
                                              CBM = dd.CBM,
                                              Weight = dd.Weight,
                                              img = bimg,
                                              KeepStock = bb.KeepStock,
                                              Item = ay.ItemId,
                                              ItemDiscount = 0,
                                              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                              ItemUnitID = bb.ItemUnitID,
                                              SubUnitId = bb.SubUnitId,
                                              ItemArabic = bb.ItemArabic,
                                              ItemDescription = bb.ItemDescription
                                          }).ToList(),
                            }).ToList();


            var cdetails = db.companys
            .Select(s => new
            {
                CName = s.CPName,
                CAddress = s.CPAddress,
                CEmail = s.CPEmail,
                CTaxRegNo = s.TRN,
                CPhone = s.CPPhone,
                s.CPMobile,
                CLogo = s.CPLogo,
            }).FirstOrDefault();

            int SI = 1;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

            string address = "";
            if (details.Address != null)
            {
                address += details.Address;
            }
            if (details.City != null)
            {
                address += details.Address != null ? "<br />" + details.City : details.City;
            }
            else if (details.State != null)
            {
                address += address != "" ? "<br />" + details.State : details.State;
            }
            else if (details.Country != null)
            {
                address += address != "" ? "<br />" + details.Country : details.Country;
            }
            else if (details.Zip != null)
            {
                address += address != "" ? "<br />" + details.Zip : details.Zip;
            }
            address += " <br/> Phone : ";
            if (details.Mobile != null)
            {
                address += details.Mobile;
                if (details.Phone != null)
                {
                    address += ", " + details.Phone;
                }
            }
            else
            {
                if (details.Phone != null)
                {
                    address += details.Phone;
                }
            }
            if (details.Email != null)
            {
                address += "<br/> Email : " + details.Email;
            }
            if (details.TRN != "")
            {
                address += "<br/><b>TRN</b> : " + details.TRN;
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Material Receive Note</b></td></tr></table>");

                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Supplier</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +
                        "<table  style='border: 0px; width: 100 %;'><tr><th>Doc No</th><td style='font-size:14px;font-weight:normal;'>: " + details.BillNo + "</td></tr><tr><th>Date</th><td style='font-size:14px;font-weight:normal;'>: " + details.Date.ToString("dd-MM-yyyy") + "</td></tr><tr><th>Requested By </th><td style='font-size:14px;font-weight:normal;'>: " + details.Cashier + "</td></tr>";

                    if (details.RequestedDate != null)
                    {
                        partyDetails += "<tr><th>Requested Date </th><td style='font-size:14px;font-weight:normal;'>: " +  Convert.ToDateTime(details.RequestedDate).ToString("dd-MM-yyyy") + "</td></tr>";
                    }


                    partyDetails += "</table></td></tr></table>";
                    sb.Append(partyDetails);


                    sb.Append("<table width='100%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc;repeat-header:yes;'>");
                    sb.Append("<thead>");
                    sb.Append("<tr style='font-size:13px;'>");
                    sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>S/N</th>");
                    if (PartNoCheck == Status.active)
                    {
                        sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Part No</th>");
                    }
                    sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Item</th>");
                    sb.Append("<th width='5%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Unit</th>");
                    sb.Append("<th width='8%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Quantity</th>");

                    if (Make == Status.active)
                    {
                        sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Make</th>");
                    }
                    sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Remarks</th>");
                    if (ProjChks == Status.active)
                    {
                        sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Project</th>");
                        sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Task</th>");
                    }

                    sb.Append("</tr>");
                    sb.Append("</thead>");
                    sb.Append("<tbody>");
                    var itemcount = 0;
                    foreach (var item in saleitem)
                    {
                        var itemnote = "";
                        if (item.ItemNote != null)
                        {
                            itemnote = "<br /><small>" + item.ItemNote + "</small>";
                        }
                        if (item.bundle != null && item.bundle.Count > 0)
                        {
                            var desc = "<br/>[<span class='descr' data-name='Note'>";
                            foreach (var itemss in item.bundle)
                            {
                                desc += itemss.ItemName;
                                desc += " - " + (itemss.ItemQuantity) + " ";
                                desc += (itemss.ItemUnit != null) ? itemss.ItemUnit : "";
                                desc += "<br/>";
                            }
                            desc += "</span>]";
                            itemnote = itemnote + desc;
                        }
                        sb.Append("<tr style='font-size:10px;'>");
                        {
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + SI++ + "</td>");
                            if (PartNoCheck == Status.active)
                            {
                                var ParNo = (!string.IsNullOrEmpty(item.PartNumber) && !string.IsNullOrWhiteSpace(item.PartNumber)) ? item.PartNumber : "";
                                sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + ParNo + "</td>");
                            }
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>");
                            sb.Append(item.ItemName + "<br/>");
                            sb.Append("<img width='40px' height='70px' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + item.ItemID + "/" + item.FileName) + "'/>");
                            sb.Append("<br/>");
                            sb.Append("<div>");
                            sb.Append(itemnote);
                            sb.Append("</div>");
                            sb.Append("</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemUnit + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemQuantity + "</td>");
                            if (Make == Status.active)
                            {
                                sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.Make + "</td>");
                            }
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.Remarks + "</td>");
                            if (ProjChks == Status.active)
                            {
                                sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ProjectName + "</td>");
                                sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.TaskName + "</td>");
                            }
                        }
                        sb.Append("</tr>");
                        itemcount++;
                    }
                    sb.Append("</tbody>");
                    sb.Append("</table>");


                    sb.Append("<table width='100%' style='border-collapse: collapse;border: .1px solid #ccc;font-size: 14px;'>");
                    string discount = "";
                    int count = 2;

                    discount += "<td style='border: .1px solid #ccc;padding: 10px;'><span style='direction:ltr'></span> </td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'></td></tr>";


                    string word = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;font-size: 15px;' colspan='5'><strong></strong></td><td style='border: .1px solid #ccc;padding: 10px;'></td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'></td></tr>";
                    string tc = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;' colspan='5' rowspan='" + count + "'> </td>";

                   
                    sb.Append(word + tc + discount);
                   
                    sb.Append("</table>");
                    if (!string.IsNullOrEmpty(details.Remarks) && !string.IsNullOrWhiteSpace(details.Remarks))
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;'><strong><u>Remarks :</u></strong><br/>" + details.Remarks.Replace("\n", "<br />") + "</td></tr>");
                        sb.Append("</table>");
                    }
                    sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                    sb.Append("<tr>");
                    sb.Append("<td align='left' width='50%' style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>Receiver's Signature:<br />توقيع المتلقي</div>");
                    sb.Append("</td>");
                    sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>");
                    sb.Append("For " + cdetails.CName + "");
                    sb.Append("</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                }
            }
            return sb;
        }

        [HttpGet]
        public ActionResult GetMRNItems(long MRNID)
        {
            var ConD = (from a in db.MRNoteItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                        from p in proj.DefaultIfEmpty()
                        join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                        from t in protask.DefaultIfEmpty()
                        join n in db.ItemBrands on a.Make equals n.ItemBrandID into bran
                        from n in bran.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into tax
                        from f in tax.DefaultIfEmpty()
                        where a.MRNote == MRNID && a.ItemNote != "-:{Bundle_Item}"
                        select new
                        {
                            a.Item,
                            a.ItemQuantity,
                            a.ItemUnit,
                            a.ItemDiscount,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            Remarks = a.Remarks.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
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
                            a.ProjectId,
                            a.TaskId,
                            p.ProjectName,
                            t.TaskName,
                            a.Make,
                            MakeName = n.ItemBrandName,

                            ItemUnitPrice = b.SellingPrice,
                            ItemSubTotal = a.ItemQuantity * b.SellingPrice,
                            ItemTax = f.Percentage,
                            ItemTaxAmount = 0,
                        });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }

        private string InvoiceNo(Int64 PNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "MRNote").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "MRNote").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.MaterialReceiveNotes.Select(p => p.MRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PNo = db.MaterialReceiveNotes.Max(p => p.MRNo + 1);
                    billNo = companyPrefix + PNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(PNo, billNo);
                    }
                }
            }
            else
            {
                PNo = PNo + 1;
                billNo = companyPrefix + PNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(PNo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string QENo)
        {
            var Exists = db.MaterialReceiveNotes.Any(c => c.BillNo == QENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetMRNNo()
        {
            Int64 PNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "MRNote").Select(a => a.number).FirstOrDefault();
            if ((db.MaterialReceiveNotes.Select(p => p.MRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                PNo = (number == 0) ? 1 : number;
            }
            else
            {
                PNo = db.MaterialReceiveNotes.Max(p => p.MRNo + 1);
            }

            return PNo;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "MRNote" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.MaterialReceiveNotes.Where(a => a.MRId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "MRNote").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
         
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
                AppUp.Type = "MRNote";

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
                            join d in db.MaterialReceiveNotes on b.TransEntry equals d.MRId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedUserId equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "MRNote"
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
