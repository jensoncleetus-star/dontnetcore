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
using System.Drawing;


namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PurchaseOrderController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PurchaseOrderController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: PurchaseOrder
        //
        [QkAuthorize(Roles = "Dev,PurchaseOrder List")]
        public ActionResult Index()
        {
            ViewBag.Prjct = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                                  }, "Value", "Text", 1);
            var proj = db.Projects
             .Select(s => new
             {
                 ID = s.ProjectId,
                 Name = s.ProCode + " " + s.ProjectName
             })
             .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
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
            _FinancialYear();
            var MlaPOrder = db.EnableSettings.Where(a => a.EnableType == "MLAPOrder").FirstOrDefault();
            var MlaPOrders = MlaPOrder != null ? MlaPOrder.Status : Status.inactive;
            ViewBag.MLAPOrder = MlaPOrders;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindPOrder").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,PurchaseOrder List")]
        public ActionResult GetPurchaseOrder(long? project, string BillNo, string FromDate, string ToDate, long? supplier, long? salesperson, string Stats, string user, int? Validity, string appstat, long? PurchaseStatus)
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

            var fromv = "POrder";
            var ToPE = "Purchase";
            var ToMRNote = "MRNote";
            var POrderToMRNote = db.EnableSettings.Where(a => a.EnableType == "POrderToMRNote").FirstOrDefault();
            var POrderToMRNotes = POrderToMRNote != null ? POrderToMRNote.Status : Status.inactive;

            var POrderToPEntry = db.EnableSettings.Where(a => a.EnableType == "POrderToPEntry").FirstOrDefault();
            var POrderToPEntrys = POrderToPEntry != null ? POrderToPEntry.Status : Status.inactive;

            Status st = new Status();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };
            var userpermission = User.IsInRole("All PurchaseOrder Entry");
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

            var uDev = User.IsInRole("Dev");
            var uPurchaseOrderView = User.IsInRole("View PurchaseOrder");
            var uEdit = User.IsInRole("Edit PurchaseOrder");
            var uDownload = User.IsInRole("Download PurchaseOrder");
            var uDelete = User.IsInRole("Delete PurchaseOrder");
            if (project != null)
            {
                var serverQuery = (from b in db.PurchaseOrders
                         join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                         from c in supp.DefaultIfEmpty()
                         join e in db.Employees on b.POCashier equals e.EmployeeId into emp
                         from e in emp.DefaultIfEmpty()
                         join g in db.Users on b.CreatedUserId equals g.Id
                         join gg in db.ConvertTransactionss on new { f1 = b.PurchaseOrderId, f2 = "PQuote", f3 = "POrder" } equals
                          new { f1 = gg.To, f2 = gg.ConvertFrom, f3 = gg.ConvertTo } into ggg
                         from gg in ggg.DefaultIfEmpty()
                         join mm in db.PurchaseQuotations on gg.From equals mm.PQuotationId into sss
                         from mm in sss.DefaultIfEmpty()
                         let qs = db.ConvertTransactionss.Where(ap => ap.From == b.PurchaseOrderId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPE).FirstOrDefault()
                         let mrn = db.ConvertTransactionss.Where(ap => ap.From == b.PurchaseOrderId && ap.ConvertFrom == fromv && ap.ConvertTo == ToMRNote).FirstOrDefault()
                             // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                             // client-side after materialization — EF Core 10 can't translate them inside this query.

                         where (BillNo == null || BillNo == "" || b.BillNo == BillNo)
                        && (project == null || mm.Project == project) &&
                       (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.PODate, fdate) <= 0) &&
                       (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.PODate, tdate) >= 0) &&
                       (supplier == 0 || c.SupplierID == supplier) &&
                       (salesperson == 0 || e.EmployeeId == salesperson) && (Stats == null || b.Status == st)
                       && (user == null || user == "" || g.Id == user) && (Validity == null || Validity == 0 || b.POValidity == Validity)
                        && (PurchaseStatus == null || b.PurchaseOrderStatus == PurchaseStatus)
                       && (userpermission == true || b.CreatedUserId == UserId)

                         select new
                         {
                             POConvert = (qs != null) ? qs.ConvertTo : "",
                             MRNConvert = (mrn != null) ? mrn.ConvertTo : "",
                             PQuote ="",// db.ConvertTransactionss.Where(a => a.To == b.PurchaseOrderId && a.ConvertFrom == "PQuote" && a.ConvertTo == "POrder").Select(a => a.From).FirstOrDefault(),

                             b.PurchaseOrderId,
                             b.PONo,

                             b.BillNo,
                             b.PODate,
                             b.POItems,
                             b.POItemQuantity,
                             b.PODiscount,
                             b.POGrandTotal,
                             b.POTax,
                             b.POTaxAmount,
                             b.Remarks,
                             b.PurchaseOrderStatus,
                             EmpName = e.FirstName + " " + e.LastName,
                             Supplier = c.SupplierCode + " - " + c.SupplierName,
                             user = g.UserName,
                             validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.PODate, (b.POValidity == null) ? 0 : b.POValidity + 1)) ? "Active" : "Expired",
                             Dev = uDev,
                             Details = uPurchaseOrderView,
                             Edit = uEdit,
                             Download = uDownload,
                             Delete = uDelete,
                             //chkexists= (from g in db.ConvertTransactionss
                             //            where g.From == b.PurchaseOrderId

                             //            })
                             CreatedDate = b.POCreatedDate
                         });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BillNo","CreatedDate","Delete","Details","Dev","Download","Edit","EmpName","MRNConvert","POConvert","PODate","PODiscount","POGrandTotal","POItemQuantity","POItems","PONo","POTax","POTaxAmount","PQuote","PurchaseOrderId","PurchaseOrderStatus","Remarks","Supplier","user","validity" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("PurchaseOrderId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

                // CLIENT-side lookups keyed by PurchaseOrderId (missing key -> empty/absent, no KeyNotFound).
                var poIds = serverRows.Select(o => o.PurchaseOrderId).ToList();
                // app = approver EmployeeIds for the purchase order (nested collection, keyed by TransEntry == PurchaseOrderId).
                var appLookup = db.Approvals
                    .Where(a => a.Type == "PurchaseOrder" && poIds.Contains(a.TransEntry))
                    .Select(a => new { a.TransEntry, a.EmployeeId })
                    .ToList()
                    .ToLookup(a => a.TransEntry);
                // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
                var appUpdRows = db.ApprovalUpdates
                    .Where(a => a.Type == "PurchaseOrder" && poIds.Contains(a.TransEntry))
                    .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                    .ToList();
                var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
                // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per purchase order.
                var chkAppStatusLookup = appUpdRows
                    .GroupBy(a => a.TransEntry)
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(l => l.ApprovedBy)
                        .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                        .Select(a => a.ApprovalStatus).ToList());

                var v = serverRows.Select(o =>
                         {
                             var app = appLookup[o.PurchaseOrderId].Select(x => x.EmployeeId).ToList();
                             var AppStatus = appStatusLookup[o.PurchaseOrderId].Select(x => x.ApprovalStatus).ToList();
                             var chkAppStatus = chkAppStatusLookup.TryGetValue(o.PurchaseOrderId, out var ck) ? ck : new List<ApprovalStatus>();
                             return new
                         {

                             o.POConvert,
                             o.MRNConvert,

                             o.PurchaseOrderId,
                             o.PONo,
                             o.BillNo,
                             o.PODate,
                             o.POItems,
                             o.POItemQuantity,
                             o.PODiscount,
                             o.POGrandTotal,
                             o.POTax,
                             o.POTaxAmount,
                             o.Remarks,
                             o.PurchaseOrderStatus,
                             o.EmpName,
                             o.Supplier,
                             o.user,
                             o.validity,
                             o.Dev,
                             o.Details,
                             o.Edit,
                             o.Download,
                             o.Delete,
                             app = app,
                             Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                             ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                             o.CreatedDate,
                             POrderToPEntry = (o.POConvert != "" && POrderToPEntrys == Status.active) ? false : true,
                             POrderToMRNote = (o.MRNConvert != "" && POrderToMRNotes == Status.active) ? false : true,
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
                    v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower())
                    || p.Supplier.ToString().ToLower().Contains(search.ToLower())
                    //|| p.EmpName.ToString().ToLower().Contains(search.ToLower())
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
            else
            {
                var serverQuery = (from b in db.PurchaseOrders
                         join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                         from c in supp.DefaultIfEmpty()
                         join e in db.Employees on b.POCashier equals e.EmployeeId into emp
                         from e in emp.DefaultIfEmpty()
                         join g in db.Users on b.CreatedUserId equals g.Id
                         join gg in db.ConvertTransactionss on new { f1 = b.PurchaseOrderId, f2 = "PQuote", f3 = "POrder" } equals
                          new { f1 = gg.To, f2 = gg.ConvertFrom, f3 = gg.ConvertTo } into ggg
                         from gg in ggg.DefaultIfEmpty()
                         let qs = db.ConvertTransactionss.Where(ap => ap.From == b.PurchaseOrderId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPE).FirstOrDefault()
                         let mrn = db.ConvertTransactionss.Where(ap => ap.From == b.PurchaseOrderId && ap.ConvertFrom == fromv && ap.ConvertTo == ToMRNote).FirstOrDefault()
                             // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                             // client-side after materialization — EF Core 10 can't translate them inside this query.

                         where (BillNo == null || BillNo == "" || b.BillNo == BillNo)&&

                       (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.PODate, fdate) <= 0) &&
                       (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.PODate, tdate) >= 0) &&
                       (supplier == 0 || c.SupplierID == supplier) &&
                       (salesperson == 0 || e.EmployeeId == salesperson) && (Stats == null || b.Status == st)
                       && (user == null || user == "" || g.Id == user) && (Validity == null || Validity == 0 || b.POValidity == Validity)
                        && (PurchaseStatus == null || b.PurchaseOrderStatus == PurchaseStatus)
                       && (userpermission == true || b.CreatedUserId == UserId)

                         select new
                         {
                             POConvert = (qs != null) ? qs.ConvertTo : "",
                             MRNConvert = (mrn != null) ? mrn.ConvertTo : "",
                             PQuote ="" ,//db.ConvertTransactionss.Where(a => a.To == b.PurchaseOrderId && a.ConvertFrom == "PQuote" && a.ConvertTo == "POrder").Select(a => a.From).FirstOrDefault(),

                             b.PurchaseOrderId,
                             b.PONo,

                             b.BillNo,
                             b.PODate,
                             b.POItems,
                             b.POItemQuantity,
                             b.PODiscount,
                             b.POGrandTotal,
                             b.POTax,
                             b.POTaxAmount,
                             b.Remarks,
                             b.PurchaseOrderStatus,
                             EmpName = e.FirstName + " " + e.LastName,
                             Supplier = c.SupplierCode + " - " + c.SupplierName,
                             user = g.UserName,
                             validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.PODate, (b.POValidity == null) ? 0 : b.POValidity + 1)) ? "Active" : "Expired",
                             Dev = uDev,
                             Details = uPurchaseOrderView,
                             Edit = uEdit,
                             Download = uDownload,
                             Delete = uDelete,
                             //chkexists= (from g in db.ConvertTransactionss
                             //            where g.From == b.PurchaseOrderId

                             //            })
                             CreatedDate = b.POCreatedDate
                         });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BillNo","CreatedDate","Delete","Details","Dev","Download","Edit","EmpName","MRNConvert","POConvert","PODate","PODiscount","POGrandTotal","POItemQuantity","POItems","PONo","POTax","POTaxAmount","PQuote","PurchaseOrderId","PurchaseOrderStatus","Remarks","Supplier","user","validity" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("PurchaseOrderId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

                // CLIENT-side lookups keyed by PurchaseOrderId (missing key -> empty/absent, no KeyNotFound).
                var poIds = serverRows.Select(o => o.PurchaseOrderId).ToList();
                // app = approver EmployeeIds for the purchase order (nested collection, keyed by TransEntry == PurchaseOrderId).
                var appLookup = db.Approvals
                    .Where(a => a.Type == "PurchaseOrder" && poIds.Contains(a.TransEntry))
                    .Select(a => new { a.TransEntry, a.EmployeeId })
                    .ToList()
                    .ToLookup(a => a.TransEntry);
                // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
                var appUpdRows = db.ApprovalUpdates
                    .Where(a => a.Type == "PurchaseOrder" && poIds.Contains(a.TransEntry))
                    .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                    .ToList();
                var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
                // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per purchase order.
                var chkAppStatusLookup = appUpdRows
                    .GroupBy(a => a.TransEntry)
                    .ToDictionary(g => g.Key, g => g
                        .GroupBy(l => l.ApprovedBy)
                        .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                        .Select(a => a.ApprovalStatus).ToList());

                var v = serverRows.Select(o =>
                         {
                             var app = appLookup[o.PurchaseOrderId].Select(x => x.EmployeeId).ToList();
                             var AppStatus = appStatusLookup[o.PurchaseOrderId].Select(x => x.ApprovalStatus).ToList();
                             var chkAppStatus = chkAppStatusLookup.TryGetValue(o.PurchaseOrderId, out var ck) ? ck : new List<ApprovalStatus>();
                             return new
                         {

                             o.POConvert,
                             o.MRNConvert,

                             o.PurchaseOrderId,
                             o.PONo,
                             o.BillNo,
                             o.PODate,
                             o.POItems,
                             o.POItemQuantity,
                             o.PODiscount,
                             o.POGrandTotal,
                             o.POTax,
                             o.POTaxAmount,
                             o.Remarks,
                             o.PurchaseOrderStatus,
                             o.EmpName,
                             o.Supplier,
                             o.user,
                             o.validity,
                             o.Dev,
                             o.Details,
                             o.Edit,
                             o.Download,
                             o.Delete,
                             app = app,
                             Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                             ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                             o.CreatedDate,
                             POrderToPEntry = (o.POConvert != "" && POrderToPEntrys == Status.active) ? false : true,
                             POrderToMRNote = (o.MRNConvert != "" && POrderToMRNotes == Status.active) ? false : true,
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
                    v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower())
                    || p.Supplier.ToString().ToLower().Contains(search.ToLower())
                    //|| p.EmpName.ToString().ToLower().Contains(search.ToLower())
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
        }


        [QkAuthorize(Roles = "Dev,PurchaseOrder Entry")]
        [HttpGet]
        public ActionResult Create(long? id, string type)
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
            var ref1 = db.PurchaseOrders
            .Select(s => new
            {
                ID = s.Ref1,
                Name = s.Ref1
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.PurchaseOrders
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.PurchaseOrders
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.PurchaseOrders
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.PurchaseOrders
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            var POentry = new PurchaseOrderViewModel
            {
                BillNo = InvoiceNo(),
                PODate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "porder").Select(a => a.TermsCondit).FirstOrDefault(),
                PurchaseTypes = db.PurchaseTypes.ToList()
            };
            if (id != null)
            {
                if (type == "MOrder")
                {

                    MaterialRequisition morder = db.MaterialRequisitions.Find(id);
                    if (morder == null)
                    {
                        return NotFound();
                    }
                    POentry.ConTypeId = morder.MaterialRequisitionId;
                    POentry.ConType = type;
                    POentry.PODate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    POentry.POCashier = morder.MRCashier;
                    POentry.SupplierType = SupplierType.CreditSale;
                    POentry.Supplier = Convert.ToInt64(morder.SupplierId);
                    POentry.PODiscount = 0;
                    POentry.POGrandTotal = 0;
                    POentry.Remarks = morder.Remarks;
                    POentry.SupplierEmail = "";

                    POentry.convertFrom = type + " No";//label
                    POentry.convertBill = morder.BillNo;
                    POentry.TermsCondition = morder.TermsCondition;
                }
                if (type == "PQuote")
                {
                    PurchaseQuotation PQuote = db.PurchaseQuotations.Find(id);
                    if (PQuote == null)
                    {
                        return NotFound();
                    }
                    POentry.ConTypeId = PQuote.PQuotationId;
                    POentry.ConType = type;
                    POentry.CPQuotNo = PQuote.PQuotNo;
                    var CFrom = db.ConvertTransactionss.Where(a => a.To == PQuote.PQuotationId && a.ConvertFrom == "MOrder" && a.ConvertTo == "PQuote").Select(a => a.From).FirstOrDefault();
                    POentry.CMReqNo = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == CFrom).Select(a => a.BillNo).FirstOrDefault();
                    POentry.PODate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    POentry.POCashier = PQuote.PQuotCashier;
                    POentry.SupplierType = SupplierType.CreditSale;
                    POentry.Supplier = PQuote.Supplier;
                    POentry.PODiscount = PQuote.PQuotDiscount;
                    POentry.POGrandTotal = PQuote.PQuotGrandTotal;
                    POentry.Remarks = PQuote.Remarks;
                    var supp = db.Suppliers.Find(PQuote.Supplier);
                    POentry.SupplierEmail = db.Contacts.Where(a => a.ContactID == supp.Contact).Select(a => a.EmailId).FirstOrDefault();
                    POentry.Branch = PQuote.Branch;

                    POentry.convertFrom = type + " No";//label
                    POentry.convertBill = PQuote.BillNo;
                    POentry.TermsCondition = PQuote.TermsCondition;

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
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            companySet();

            var userpermission = User.IsInRole("All PurchaseOrder Entry");
            var UserId = User.Identity.GetUserId();

            ViewBag.LastEntry = db.PurchaseOrders.Where(a => a.CreatedUserId == UserId || userpermission == true).Select(p => p.PurchaseOrderId).AsEnumerable().DefaultIfEmpty(0).Max();

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
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

            ViewBag.ProjChk = ProjectCheck;
            ViewBag.Contype = type;
            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaPOrder = db.EnableSettings.Where(a => a.EnableType == "MLAPOrder").FirstOrDefault();
            var MlaPOrders = MlaPOrder != null ? MlaPOrder.Status : Status.inactive;
            ViewBag.MLAPOrder = MlaPOrders;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            POentry.FieldMap = db.FieldMappings.Where(a => a.Section == "LPO" && a.Status == Status.active).ToList();
            _FinancialYear();
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
            return View(POentry);
        }
        [HttpGet]
        public ActionResult downloadprint(long porderId)
        {

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            var UserId = User.Identity.GetUserId();
            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

          

            var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;


            PurchaseOrder POentry = db.PurchaseOrders.Find(porderId);  
            string qedate = POentry.PODate.ToString("dd-MM-yyyy");

                var fmapp = db.FieldMappings.Where(a => a.Section == "LPO" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                var ChkApp = com.chkApprovedBy(porderId, "PurchaseOrder", UserId);
                var ApprovedBy = (from x in db.Approvals
                                  join y in db.Employees on x.EmployeeId equals y.EmployeeId
                                  where x.Type == "PurchaseOrder" && x.TransEntry == porderId
                                  select new
                                  {
                                      ApprovedBy = y.FirstName + " " + y.LastName
                                  }).FirstOrDefault();

                var PurchaseData = com.PurchaseOrderData(porderId, InPrintItemCode, PartNoCheck, 1000, ComHeadCheck, null, null);
                var item = PurchaseData.pdfItem.ToList();
                var summary = PurchaseData;
                var billsundry = PurchaseData.billsundry.ToList();

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
             
               bool stat = true;
                return Json( new { status = stat, item, summary, billsundry,  fmapp, ApprovedBy, ChkApp, PurchaseOrderId = porderId });


        }
        [QkAuthorize(Roles = "Dev,PurchaseOrder Entry")]
        public JsonResult CreatePurchaseOrder(string[][] array, string[] podata, string action, POBillSundryViewModel bsmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                if (!BillExist(Convert.ToString(podata[15])))
                {

                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(podata[18]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                    var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                    var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                    //sales entry
                    PurchaseOrder POentry = new PurchaseOrder();

                    POentry.PONo = GetPONo();
                    POentry.BillNo = Convert.ToString(podata[15]);
                    POentry.PODate = DateTime.Parse(podata[2], new CultureInfo("en-GB"));
                    POentry.POCashier = podata[1] != "" ? Convert.ToInt64(podata[1]) : 0;
                    POentry.Supplier = Convert.ToInt64(podata[0]);

                    POentry.POItems = Convert.ToInt32(podata[3]);
                    POentry.POItemQuantity = Convert.ToDecimal(podata[4]);
                    POentry.POSubTotal = Convert.ToDecimal(podata[8]);
                    POentry.POTax = Convert.ToDecimal(podata[9]);
                    POentry.POTaxAmount = Convert.ToDecimal(podata[5]);
                    POentry.PODiscount = Convert.ToDecimal(podata[6]);
                    POentry.POGrandTotal = Convert.ToDecimal(podata[7]);
                    POentry.PONote = "";
                    POentry.Mail = 0;
                    POentry.POCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    POentry.CreatedUserId = UserId;
                    POentry.Status = Status.active;
                    POentry.TermsCondition = Convert.ToString(podata[11]);
                    POentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "PurchaseOrder").Select(a => a.EmailTemplateID).FirstOrDefault();
                    POentry.CompanyHeaderID = 0;
                    POentry.Branch = Branch;
                    POentry.POValidity = podata[10] != null ? Convert.ToInt32(podata[10]) : 0;
                    POentry.PurchaseType = Convert.ToInt64(podata[19]);
                    POentry.Remarks = podata[17];


                    if (podata[16] == "1")
                    {
                        POentry.SupplierType = SupplierType.CashSale;
                    }
                    else
                    {
                        POentry.SupplierType = SupplierType.CreditSale;
                    }


                    POentry.Ref1 = Convert.ToString(podata[25]);
                    POentry.Ref2 = Convert.ToString(podata[26]);
                    POentry.Ref3 = Convert.ToString(podata[27]);
                    POentry.Ref4 = Convert.ToString(podata[28]);
                    POentry.Ref5 = Convert.ToString(podata[29]);
                    POentry.PurchaseOrderStatus = Convert.ToInt32(podata[31]);
                    if (podata.Count() > 31)
                    {
                        if (podata[32] != null && podata[32] != "")
                        {
                            POentry.Currency = Convert.ToInt64(podata[32]);
                        }
                        if (podata[33] != null && podata[33] != "")
                        {
                            POentry.ConvertionRate = podata[33];
                        }
                    }
                    db.PurchaseOrders.Add(POentry);
                    db.SaveChanges();

                    Int64 porderId = 0;
                    porderId = POentry.PurchaseOrderId;

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

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

                    dtItem.Columns.Add("ProjectId");
                    dtItem.Columns.Add("TaskId");
                    dtItem.Columns.Add("Make");
                    dtItem.Columns.Add("ItemNote");

                    dtItem.Columns.Add("PurchaseOrder");
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
                        if (ProjChks == Status.active)
                        {
                            dr["ProjectId"] = Convert.ToInt64(arr[29]);
                            dr["TaskId"] = Convert.ToInt64(arr[30]);
                            dr["Make"] = Convert.ToInt32(arr[31]);
                            dr["ItemNote"] = Convert.ToString(arr[35].Replace("\n", "<br />"));
                        }
                        else
                        {
                            dr["ProjectId"] = 0;
                            dr["TaskId"] = 0;
                            dr["Make"] = Convert.ToInt32(arr[29]);
                            dr["ItemNote"] = Convert.ToString(arr[33].Replace("\n", "<br />"));
                        }
                        dr["PurchaseOrder"] = porderId;
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
                                dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                                dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                                dbu["ItemSubTotal"] = ItemSubTotal;
                                // add parent itemid in discount for reference
                                dbu["ItemDiscount"] = item;
                                dbu["ItemTax"] = itemtax;
                                dbu["ItemTaxAmount"] = taxamt;
                                dbu["ItemTotalAmount"] = totamt;
                                if (ProjChks == Status.active)
                                {
                                    dbu["ProjectId"] = Convert.ToInt64(arr[29]);
                                    dbu["TaskId"] = Convert.ToInt64(arr[30]);
                                    dbu["Make"] = Convert.ToInt32(arr[31]);
                                }
                                else
                                {
                                    dbu["ProjectId"] = 0;
                                    dbu["TaskId"] = 0;
                                    dbu["Make"] = Convert.ToInt32(arr[29]);
                                }
                                dbu["itemNote"] = "-:{Bundle_Item}";

                                dbu["PurchaseOrder"] = porderId;
                                dbu["Item"] = bu.Item;

                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }

                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypePOItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertPOrderItems", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql, parameter);

                    //billsundry
                    if (bsmodel.pobsundrys != null)
                    {
                        POBillSundry pobs = new POBillSundry();
                        foreach (var bs in bsmodel.pobsundrys)
                        {
                            pobs.PurchaseOrder = porderId;
                            pobs.BillSundry = bs.BillSundry;
                            pobs.BsValue = bs.BsValue;
                            pobs.AmountType = bs.AmountType;
                            pobs.BsType = bs.BsType;
                            pobs.BsAmount = bs.BsAmount;
                            db.POBillSundrys.Add(pobs);
                            db.SaveChanges();
                        }
                    }
                    if (podata[20] != null && podata[20] != "0" && podata[20] != "" && podata[21] != null && podata[21] != "" && podata[21] != "0")
                    {

                        ConvertTransactions ConTran = new ConvertTransactions();

                        ConTran.ConvertFrom = podata[21];
                        ConTran.ConvertTo = "POrder";
                        ConTran.From = Convert.ToInt64(podata[20]);
                        ConTran.To = POentry.PurchaseOrderId;
                        ConTran.Status = 0;
                        ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        ConTran.CreatedBy = UserId;
                        ConTran.Branch = Convert.ToInt32(BranchID);

                        db.ConvertTransactionss.Add(ConTran);
                        db.SaveChanges();
                        com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Convertion");
                    }
                    //Approved By
                    var Appby = Convert.ToString(podata[22]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = porderId;
                            approval.Type = "PurchaseOrder";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                            com.addlog(LogTypes.Created, UserId, "PurchaseOrder", "PurchaseOrders", findip(), porderId, "Successfully Submitted Purchase Order");
                        }
                    }
                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                    var CMReqNo = podata[23] != "" ? Convert.ToString(podata[23]) : "";
                    var CPQuotNo = podata[24] != "" ? Convert.ToInt64(podata[24]) : 0;

                    if (action == "print")
                    {
                        string qedate = POentry.PODate.ToString("dd-MM-yyyy");

                        var fmapp = db.FieldMappings.Where(a => a.Section == "LPO" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                        var ChkApp = com.chkApprovedBy(porderId, "PurchaseOrder", UserId);
                        var ApprovedBy = (from x in db.Approvals
                                          join y in db.Employees on x.EmployeeId equals y.EmployeeId
                                          where x.Type == "PurchaseOrder" && x.TransEntry == porderId
                                          select new
                                          {
                                              ApprovedBy = y.FirstName + " " + y.LastName
                                          }).FirstOrDefault();

                        var PurchaseData = com.PurchaseOrderData(porderId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck,CMReqNo, CPQuotNo);
                        var item = PurchaseData.pdfItem.ToList();
                        var summary = PurchaseData;
                        var billsundry = PurchaseData.billsundry.ToList();

                        var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                        var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                        var def = (PriLay == Status.active) ? Convert.ToInt64(podata[30]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp, ApprovedBy, ChkApp, PurchaseOrderId = porderId } };
                    }
                    else if (action == "sendmail")
                    {
                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = podata[12];
                        string CcMail = podata[13];
                        string InvoiceNo = "_PurchaseOrder_" + POentry.PONo;

                        var em = db.EmailTemplates.Where(a => a.Head == "PurchaseOrder").FirstOrDefault();
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;
                        }
                        else
                        {
                            message.Subject = "PurchaseOrder";
                            message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                                " <p>we are enclosing our purchase order for the items / services as requested by you during our discussions.<br/></p> " +
                                " <p>Looking forward to hear from you.</p>";
                        }
                        sm.SendPdfMail(generatePdf(porderId), ToMail, CcMail, InvoiceNo, message);

                        msg = "Successfully submitted Purchase Order.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, PurchaseOrderId = porderId } };
                    }
                    else
                    {
                        msg = "Successfully submitted Purchase Order.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, PurchaseOrderId = porderId } };
                    }
                    //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Purchase Order No. Already Exists.";
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

        [QkAuthorize(Roles = "Dev,Edit PurchaseOrder")]
        public ActionResult Edit(long? id)
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All PurchaseOrder Entry");
            var UserId = User.Identity.GetUserId();
            PurchaseOrder porder = db.PurchaseOrders.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.PurchaseOrderId == id).FirstOrDefault();

            if (porder == null)
            {
                return NotFound();
            }

            //Fetching values from AttachmentDocuments
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.PurchaseOrders
                             on a.TransactionID equals b.PurchaseOrderId
                             where (a.TransactionID == id && a.TransactionType == "Purchase Order")
                             select new PurchOrdrDocumentViewModel
                             {
                                 DocumentID     =   a.DocumentID,
                                 FileName       =   a.FileName,
                                 Status         =   a.Status,
                                 CreatedDate    =   a.CreatedDate
                             }).ToList();

            PurchaseOrderViewModel vmodel = new PurchaseOrderViewModel();
            var sup = db.Suppliers
                .Select(s => new
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
            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseOrder").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);


            var MlaPOrder = db.EnableSettings.Where(a => a.EnableType == "MLAPOrder").FirstOrDefault();
            var MlaPOrders = MlaPOrder != null ? MlaPOrder.Status : Status.inactive;
            ViewBag.MLAPOrder = MlaPOrders;

            var PQuote = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertFrom == "PQuote" && a.ConvertTo == "POrder").Select(a => a.From).FirstOrDefault();
            Int64 CFrom = 0;
            string CMReqNo = "";
            if (PQuote != null)
            {
                CFrom = db.ConvertTransactionss.Where(a => a.To == PQuote && a.ConvertFrom == "MOrder" && a.ConvertTo == "PQuote").Select(a => a.From).FirstOrDefault();
                CMReqNo = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == CFrom).Select(a => a.BillNo).FirstOrDefault();
            }

            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "POrder").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "MOrder")
                {
                    CBill = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "PQuote")
                {
                    CBill = db.PurchaseQuotations.Where(a => a.PQuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }


            vmodel = (from b in db.PurchaseOrders
                      where b.PurchaseOrderId == id
                      select new PurchaseOrderViewModel
                      {
                          PONo = b.PONo,
                          Supplier = b.Supplier,
                          PODate = b.PODate,
                          BillNo = b.BillNo,
                          POValidity = b.POValidity != null ? b.POValidity : 0,
                          POCashier = b.POCashier,
                          PODiscount = b.PODiscount,
                          POGrandTotal = b.POGrandTotal,
                          TermsCondition = b.TermsCondition,
                          Remarks = b.Remarks,
                          Branch = b.Branch,
                          PurchaseType = b.PurchaseType,
                          PurchaseTypes = db.PurchaseTypes.ToList(),
                          CMReqNo = CMReqNo,
                          convertBill = CBill,
                          convertFrom = CType,
                          Currency=b.Currency,
                          ConvertionRate =b.ConvertionRate,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();
            companySet();
            ViewBag.preEntry = db.PurchaseOrders.Where(a => (a.PurchaseOrderId < id) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.PurchaseOrderId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PurchaseOrders.Where(a => (a.PurchaseOrderId > id) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.PurchaseOrderId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;
            _FinancialYear();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;


            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            var EditPermission = User.IsInRole("Disable POrder Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "PurchaseOrder", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "LPO" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummyPOrderItems.Where(a => a.PurchaseOrder == id).FirstOrDefault();
            var SItem = db.PurchaseOrderItems.Where(a => a.PurchaseOrder == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummyPOrderItems.Where(a => a.PurchaseOrder == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    PurchaseOrderItem sItem = new PurchaseOrderItem();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.ItemNote = arr.ItemNote;
                    sItem.PurchaseOrder = arr.PurchaseOrder;
                    sItem.Item = arr.Item;
                    sItem.ProjectId = arr.ProjectId;
                    sItem.TaskId = arr.TaskId;
                    sItem.Make = arr.Make;
                    db.PurchaseOrderItems.Add(sItem);
                    db.SaveChanges();
                }

                db.PurchaseOrderItems.RemoveRange(db.PurchaseOrderItems.Where(a => a.PurchaseOrder == id));
                db.SaveChanges();
            }
            _FinancialYear();
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

            if (porder.PurchaseOrderStatus == 0)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.PurchaseStatus = pstat;

            }
            else if (porder.PurchaseOrderStatus == 1)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
              };
                ViewBag.PurchaseStatus = pstat;
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
                ViewBag.PurchaseStatus = pstat;
            }

            var ref1 = db.PurchaseOrders 
              .Select(s => new
              {
                  ID = s.Ref1,
                  Name = s.Ref1
              }).Distinct()
              .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", vmodel.Ref1);

            var ref2 = db.PurchaseOrders
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", vmodel.Ref2);

            var ref3 = db.PurchaseOrders
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", vmodel.Ref3);

            var ref4 = db.PurchaseOrders
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", vmodel.Ref4);

            var ref5 = db.PurchaseOrders
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", vmodel.Ref5);


            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Edit PurchaseOrder")]
        public JsonResult UpdatePurchaseOrder(string[][] array, string[] podata, string action, POBillSundryViewModel bsmodel)
        {
            bool stat = false;
            string msg;
            Int64 poEntryId = Convert.ToInt64(podata[16]);
            PurchaseOrder poentry = db.PurchaseOrders.Find(poEntryId);
            if (BillExist(Convert.ToString(podata[15])) && Convert.ToString(podata[15]) != poentry.BillNo)
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

                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(podata[19]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                var EditPermission = User.IsInRole("Disable POrder Edit After Approval");
                if (com.chkApproved(poEntryId, EditPermission, "PurchaseOrder", UserId) == true)
                {
                    //sales entry


                    poentry.BillNo = podata[15];
                    poentry.PONo = Convert.ToInt64(podata[14]);
                    poentry.PODate = DateTime.Parse(podata[2], new CultureInfo("en-GB"));
                    poentry.POCashier = podata[1] != "" ? Convert.ToInt64(podata[1]) : 0;
                    poentry.Supplier = Convert.ToInt64(podata[0]);

                    poentry.POItems = Convert.ToInt32(podata[3]);
                    poentry.POItemQuantity = Convert.ToDecimal(podata[4]);
                    poentry.POSubTotal = Convert.ToDecimal(podata[8]);
                    poentry.POTax = Convert.ToDecimal(podata[9]);
                    poentry.POTaxAmount = Convert.ToDecimal(podata[5]);
                    poentry.PODiscount = Convert.ToDecimal(podata[6]);
                    poentry.POGrandTotal = Convert.ToDecimal(podata[7]);
                    poentry.PONote = "";
                    poentry.Mail = 0;
                    poentry.Status = Status.active;
                    poentry.TermsCondition = Convert.ToString(podata[11]);
                    poentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "PurchaseOrder").Select(a => a.EmailTemplateID).FirstOrDefault();
                    poentry.CompanyHeaderID = 0;
                    poentry.Branch = Branch;
                    poentry.POValidity = podata[10] != null ? Convert.ToInt32(podata[10]) : 0;
                    poentry.Remarks = podata[18];
                    poentry.PurchaseType = Convert.ToInt64(podata[20]);
                    if (podata[17] == "1")
                    {
                        poentry.SupplierType = SupplierType.CashSale;
                    }
                    else
                    {
                        poentry.SupplierType = SupplierType.CreditSale;
                    }

                    poentry.Ref1 = Convert.ToString(podata[23]);
                    poentry.Ref2 = Convert.ToString(podata[24]);
                    poentry.Ref3 = Convert.ToString(podata[25]);
                    poentry.Ref4 = Convert.ToString(podata[26]);
                    poentry.Ref5 = Convert.ToString(podata[27]);

                    poentry.PurchaseOrderStatus = Convert.ToInt32(podata[29]);
                    if (podata.Count() > 30)
                    {
                        if (podata[30] != null && podata[30] != "")
                        {
                            poentry.Currency = Convert.ToInt64(podata[30]);
                        }
                        if (podata[31] != null && podata[31] != "")
                        {
                            poentry.ConvertionRate = podata[31];
                        }
                    }
                    db.Entry(poentry).State = EntityState.Modified;
                    db.SaveChanges();

                    var POItem = db.PurchaseOrderItems.Where(a => a.PurchaseOrder == poEntryId).FirstOrDefault();
                    if (POItem != null)
                    {

                        var PItems = db.PurchaseOrderItems.Where(a => a.PurchaseOrder == poEntryId).ToList();
                        foreach (var arr in PItems)
                        {
                            //add to dummy table
                            DummyPOrderItem dItem = new DummyPOrderItem();
                            dItem.ItemUnit = arr.ItemUnit;
                            dItem.ItemUnitPrice = arr.ItemUnitPrice;
                            dItem.ItemQuantity = arr.ItemQuantity;
                            dItem.ItemSubTotal = arr.ItemSubTotal;
                            dItem.ItemDiscount = arr.ItemDiscount;
                            dItem.ItemTax = arr.ItemTax;
                            dItem.ItemTaxAmount = arr.ItemTaxAmount;
                            dItem.ItemTotalAmount = arr.ItemTotalAmount;
                            dItem.ItemNote = arr.ItemNote;
                            dItem.PurchaseOrder = arr.PurchaseOrder;
                            dItem.Item = arr.Item;
                            dItem.ProjectId = arr.ProjectId;
                            dItem.TaskId = arr.TaskId;
                            dItem.Make = arr.Make;

                            db.DummyPOrderItems.Add(dItem);
                            db.SaveChanges();
                        }

                        db.PurchaseOrderItems.RemoveRange(db.PurchaseOrderItems.Where(a => a.PurchaseOrder == poEntryId));
                        db.SaveChanges();
                    }

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


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

                    dtItem.Columns.Add("ProjectId");
                    dtItem.Columns.Add("TaskId");
                    dtItem.Columns.Add("Make");
                    dtItem.Columns.Add("ItemNote");

                    dtItem.Columns.Add("PurchaseOrder");
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
                        if (ProjChks == Status.active)
                        {
                            dr["ProjectId"] = Convert.ToInt64(arr[29]);
                            dr["TaskId"] = Convert.ToInt64(arr[30]);
                            dr["Make"] = Convert.ToInt32(arr[31]);
                            dr["ItemNote"] = Convert.ToString(arr[35].Replace("\n", "<br />"));
                        }
                        else
                        {
                            dr["ProjectId"] = 0;
                            dr["TaskId"] = 0;
                            dr["Make"] = Convert.ToInt32(arr[29]);
                            dr["itemNote"] = Convert.ToString(arr[33].Replace("\n", "<br />"));
                        }
                        dr["PurchaseOrder"] = poEntryId;
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
                                dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                                dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                                dbu["ItemSubTotal"] = ItemSubTotal;
                                // add parent itemid in discount for reference
                                dbu["ItemDiscount"] = item;
                                dbu["ItemTax"] = itemtax;
                                dbu["ItemTaxAmount"] = taxamt;
                                dbu["ItemTotalAmount"] = totamt;
                                if (ProjChks == Status.active)
                                {
                                    dbu["ProjectId"] = Convert.ToInt64(arr[29]);
                                    dbu["TaskId"] = Convert.ToInt64(arr[30]);
                                    dbu["Make"] = Convert.ToInt32(arr[31]);

                                }
                                else
                                {
                                    dbu["ProjectId"] = 0;
                                    dbu["TaskId"] = 0;
                                    dbu["Make"] = Convert.ToInt32(arr[29]);
                                }
                                dbu["itemNote"] = "-:{Bundle_Item}";
                                dbu["PurchaseOrder"] = poEntryId;
                                dbu["Item"] = bu.Item;

                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }

                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypePOItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertPOrderItems", "@TableType");
                    //// execute sql 
                    var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                    if (ret > 0)
                    {
                        db.DummyPOrderItems.RemoveRange(db.DummyPOrderItems.Where(a => a.PurchaseOrder == poEntryId));
                        db.SaveChanges();
                    }

                    var POBs = db.POBillSundrys.Where(a => a.PurchaseOrder == poEntryId).FirstOrDefault();
                    if (POBs != null)
                    {
                        db.POBillSundrys.RemoveRange(db.POBillSundrys.Where(a => a.PurchaseOrder == poEntryId));
                        db.SaveChanges();
                    }

                    if (bsmodel.pobsundrys != null)
                    {
                        POBillSundry pobs = new POBillSundry();
                        foreach (var bs in bsmodel.pobsundrys)
                        {
                            pobs.PurchaseOrder = poEntryId;
                            pobs.BillSundry = bs.BillSundry;
                            pobs.BsValue = bs.BsValue;
                            pobs.AmountType = bs.AmountType;
                            pobs.BsType = bs.BsType;
                            pobs.BsAmount = bs.BsAmount;
                            db.POBillSundrys.Add(pobs);
                            db.SaveChanges();
                        }
                    }

                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == poEntryId && a.Type == "PurchaseOrder").FirstOrDefault();
                    var PurchasenPO = db.Approvals.Where(a => a.TransEntry == poEntryId && a.Type == "PurchaseOrder").FirstOrDefault();
                    if (PurchasenPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == poEntryId && a.Type == "PurchaseOrder"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == poEntryId && a.Type == "PurchaseOrder"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = Convert.ToString(podata[21]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = poEntryId;
                            approval.Type = "PurchaseOrder";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "PurchaseOrder", "PurchaseOrders", findip(), poEntryId, "Successfully Updated Purchase Order");

                }
                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                var CMReqNo = podata[22] != "" ? Convert.ToString(podata[22]) : "";
                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "LPO" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    string qedate = poentry.PODate.ToString("dd-MM-yyyy");
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    var PurchaseData = com.PurchaseOrderData(poEntryId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck, CMReqNo);
                    var item = PurchaseData.pdfItem.ToList();
                    var summary = PurchaseData;
                    var billsundry = PurchaseData.billsundry.ToList();

                    var ChkApp = com.chkApprovedBy(poEntryId, "PurchaseOrder", UserId);
                    var ApprovedBy = (from x in db.Approvals
                                      join y in db.Employees on x.EmployeeId equals y.EmployeeId
                                      where x.Type == "PurchaseOrder" && x.TransEntry == poEntryId
                                      select new
                                      {
                                          ApprovedBy = y.FirstName + " " + y.LastName
                                      }).FirstOrDefault();

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(podata[28]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp, ApprovedBy, ChkApp, PurchaseOrderId = poEntryId } };

                }
                else if (action == "sendmail")
                {
                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = podata[12];
                    string CcMail = podata[13];
                    string InvoiceNo = "_PurchaseOrder_" + poentry.PONo;

                    var em = db.EmailTemplates.Where(a => a.Head == "PurchaseOrder").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "PurchaseOrder";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our purchase order for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(poEntryId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully Updated Purchase Order.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, PurchaseOrderId = poEntryId } };
                }
                else
                {
                    msg = "Successfully Updated Purchase Order.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, PurchaseOrderId = poEntryId } };
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
        [QkAuthorize(Roles = "Dev,View PurchaseOrder")]
        public ActionResult Details(long? id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;
            var PQuote = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertFrom == "PQuote" && a.ConvertTo == "POrder").Select(a => a.From).FirstOrDefault();
            Int64 CFrom = 0;
            string CMReqNo = "";
            if (PQuote != null)
            {
                CFrom = db.ConvertTransactionss.Where(a => a.To == PQuote && a.ConvertFrom == "MOrder" && a.ConvertTo == "PQuote").Select(a => a.From).FirstOrDefault();
                CMReqNo = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == CFrom).Select(a => a.BillNo).FirstOrDefault();
            }
            PurchaseOrderViewModel vmodel = new PurchaseOrderViewModel();
            vmodel = (from b in db.PurchaseOrders
                      join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                      from c in supp.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.POCashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join t in db.PurchaseTypes on b.PurchaseType equals t.Id into ptype
                      from t in ptype.DefaultIfEmpty()
                      where b.PurchaseOrderId == id
                      select new PurchaseOrderViewModel
                      {
                          SupplierName = c.SupplierCode + " - " + c.SupplierName,
                          PONo = b.PONo,
                          BillNo = b.BillNo + ((CMReqNo != null) ? " MRQ NO:" + CMReqNo : ""),
                          PODate = b.PODate,
                          TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
                          EmployeeName = e.FirstName + " " + e.LastName,
                          PODiscount = b.PODiscount,
                          POGrandTotal = b.POGrandTotal,
                          POValidity = b.POValidity != null ? b.POValidity : 0,
                          //QuotCashier = e.Name,
                          PayType = (b.SupplierType == SupplierType.CashSale ? "Cash" : "Credit"),
                          PursTypeName = t.Name,

                          CreditPeriod = c.CreditPeriod,
                          Remarks = b.Remarks.Replace("\n", "<br />"),

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
                      }).FirstOrDefault();
            vmodel.POItem = db.PurchaseOrderItems.Where(a => a.PurchaseOrder == id && a.ItemNote != "-:{Bundle_Item}")
            .Select(b => new PurchaseOrderItemViewModel
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
                MakeName = db.ItemBrands.Where(a => a.ItemBrandID == b.Make).Select(a => a.ItemBrandName).FirstOrDefault(),
                bundleitem = (from ab in db.PurchaseOrderItems
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.PurchaseOrder == id && ab.ItemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();
            vmodel.PObs = db.POBillSundrys.Where(a => a.PurchaseOrder == id)
          .Select(b => new POBillSundryViewModel
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
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "LPO" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Delete PurchaseOrder")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All PurchaseOrder Entry");
            var UserId = User.Identity.GetUserId();
            PurchaseOrder PO = db.PurchaseOrders.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.PurchaseOrderId == id).FirstOrDefault();

            if (PO == null)
            {
                return NotFound();
            }
            return PartialView(PO);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete PurchaseOrder")]
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
                stat = DeletePO(id);
                msg = "Successfully deleted Purchase Order.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete PurchaseOrder")]
        public ActionResult DeleteAllPurchaseOrder(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeletePOrder(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " PurchaseOrder", true);
            return RedirectToAction("Index", "PurchaseOrder");
        }
        private Boolean DeletePOrder(long saleId)
        {
            var Msg = chkDeleteWithMsg(saleId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeletePO(saleId);
            }
        }
        private Boolean DeletePO(long id)
        {
            var UserId = User.Identity.GetUserId();
            PurchaseOrder PO = db.PurchaseOrders.Find(id);
            var pitem = db.PurchaseOrderItems.Where(a => a.PurchaseOrder == id).FirstOrDefault();
            if (pitem != null)
            {
                db.PurchaseOrderItems.RemoveRange(db.PurchaseOrderItems.Where(a => a.PurchaseOrder == id));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseOrder").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseOrder"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "PurchaseOrder").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "PurchaseOrder"));
            }
            var CPOrder = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "POrder").FirstOrDefault();
            if (CPOrder != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "POrder"));
            }
            db.PurchaseOrders.Remove(PO);
            db.SaveChanges();

            /*********** Delete from AttachmentDocuments Table *********************/
            List<AttachmentDocuments> DocumentLists = new List<AttachmentDocuments>();
            DocumentLists = db.AttachmentDocuments.Where(a => (a.TransactionID == id && a.TransactionType == "Purchase Order")).ToList();

            var i = 0;
            foreach (var row in DocumentLists)
            {
                //To remove the attached file from folder
                string FullPath = LegacyWeb.MapPath("~/uploads/PurchaseOrderDocuments/" + DocumentLists.ElementAt(i).FileName);

                if (System.IO.File.Exists(FullPath))
                {
                    System.IO.File.Delete(FullPath);
                }

                //To remove the attached file from server
                db.AttachmentDocuments.Remove(DocumentLists[i]);
                i++;
            }
            db.SaveChanges();

            /***********************************************************************/

            com.addlog(LogTypes.Deleted, UserId, "PurchaseOrder", "PurchaseOrders", findip(), PO.PurchaseOrderId, "Successfully Deleted Purchase Order");

            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext1 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "POrder" && x.ConvertTo == "Purchase").FirstOrDefault();
            var Ext2 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "POrder" && x.ConvertTo == "MRNote").FirstOrDefault();
            if (Ext1 != null)
            {
                var inv = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == Ext1.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Purchase : " + inv + ".";
            }
            else if (Ext2 != null)
            {
                var inv = db.MaterialReceiveNotes.Where(x => x.MRId == Ext2.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to MR Note: " + inv + ".";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        [HttpGet]
        public ActionResult GetPOBillSundry(long POrderID)
        {
            var PEBs = (from a in db.POBillSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.PurchaseOrder == POrderID
                        select new
                        {
                            a.AmountType,
                            a.BillSundry,
                            a.BsAmount,
                            a.BsType,
                            a.BsValue,
                            //a.PEBillSundryId,
                            //a.PurchaseEntry,
                            c.BSName
                            //c.BillSundryId
                        }).ToList();
            return Json(PEBs);
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download PurchaseOrder")]
        public ActionResult Download(long id)
        {
            var Data = db.PurchaseOrders.Where(s => s.PurchaseOrderId == id).FirstOrDefault();
            var supname = db.Suppliers.Where(s => s.SupplierID == Data.Supplier).Select(a => a.SupplierName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Purchase Order" + "-" + supname + "-" + billno + ".pdf");
        }
        public StringBuilder generatePdf(long porderId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var PurchaseData = com.PurchaseOrderData(porderId, InPrintItemCode, PartNoCheck, TimeOut);
            var item = PurchaseData.pdfItem.ToList();
            var summary = PurchaseData;
            var billsundry = PurchaseData.billsundry.ToList();


            return com.generatepdf(porderId, summary, item, billsundry, "LPO");
        }

        //                   where b.PurchaseOrderId == porderId // b.Customer == customer
        //                       BillNo = b.BillNo,
        //                       QuotNo = b.PONo,
        //                       Date = b.PODate,
        //                       QuotValidity = b.POValidity,
        //                       QuotGrandTotal = b.POGrandTotal,
        //                       PartyName = c.SupplierName,
        //                       CustomerEmail = d.EmailId,
        //                       Address = d.Address,
        //                       City = d.City,
        //                       SubTotal = b.POSubTotal,
        //                       Discount = b.PODiscount,
        //                       tc = b.PONote,
        //                       TaxAmount = b.POTaxAmount,
        //                       State = d.State,
        //                       Country = d.Country,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       GrandTotal = b.POGrandTotal,
        //                       TRN = c.TaxRegNo,
        //                       Email = d.EmailId,
        //                       Zip = d.Zip,
        //                       Phone = d.Phone,
        //                       Mobile = d.Mobile,
        //                       b.TermsCondition,
        //                       CreditPeriod = c.CreditPeriod,
        //                       b.Remarks

        //                  where b.PurchaseOrder == porderId && b.ItemNote != "-:{Bundle_Item}"
        //                      ItemUnitPrice = b.ItemUnitPrice,
        //                      ItemQuantity = b.ItemQuantity,
        //                      ItemTax = b.ItemTax,
        //                      ItemNote = b.ItemNote,
        //                      ItemTaxAmount = b.ItemTaxAmount,
        //                      ItemTotalAmount = b.ItemTotalAmount,
        //                      ItemSubTotal = b.ItemSubTotal,
        //                      ItemID = b.Item,
        //                      bundleitem = (from ab in db.PurchaseOrderItems
        //                                    where ab.PurchaseOrder == porderId && ab.ItemNote == "-:{Bundle_Item}"
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
        //                                        bb.ItemArabic
        //                                    }).ToList()

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











        [HttpGet]
        public ActionResult GetPOItems(long POrderID)
        {
            var ConD = (from a in db.PurchaseOrderItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                        from p in proj.DefaultIfEmpty()
                        join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                        from t in protask.DefaultIfEmpty()
                        join h in db.ItemBrands on a.Make equals h.ItemBrandID into brn
                        from h in brn.DefaultIfEmpty()
                        where a.PurchaseOrder == POrderID && a.ItemNote != "-:{Bundle_Item}"
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
                            a.ProjectId,
                            a.TaskId,
                            p.ProjectName,
                            t.TaskName,
                            ItemMake = h != null ? h.ItemBrandID : 0,
                            ItemMakeName = h != null ? h.ItemBrandName : ""
                        });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }
        [HttpGet]
        public ActionResult GetPOItems2(long POrderID, string ConvertTo)
        {
            var temp = db.ConvertTransactionss.Where(a => a.From == POrderID && a.ConvertFrom == "POrder" && a.ConvertTo == ConvertTo).Select(a => a.To);
            List<ItemList2> DVItems = new List<ItemList2>();
            List<ItemList2> temp6 = new List<ItemList2>();
            List<ItemList2> DVitemsGroupBy = new List<ItemList2>();
            List<ItemList2> RemainingItems = new List<ItemList2>();
            foreach (var tem in temp)
            {
                var temp2 = (from a in db.MRNoteItems
                             join b in db.Items on a.Item equals b.ItemID
                            join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                            from c in primary.DefaultIfEmpty()
                            join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                            from d in second.DefaultIfEmpty()
                            join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                            from p in proj.DefaultIfEmpty()
                            join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                            from t in protask.DefaultIfEmpty()
                            join h in db.ItemBrands on a.Make equals h.ItemBrandID into brn
                            from h in brn.DefaultIfEmpty()
                            where a.MRNote == tem 
                            select new ItemList2
                            {
                                Item = a.Item,
                                ItemQuantity = a.ItemQuantity,
                                ItemUnit = a.ItemUnit,
                                //ItemUnitPrice = a.ItemUnitPrice,
                                //ItemTax = a.ItemTax,
                                //ItemSubTotal = a.ItemSubTotal,
                                //ItemTaxAmount = a.ItemTaxAmount,
                                ItemDiscount = a.ItemDiscount,
                                note = a.ItemNote.Replace("<br />", "\n"),
                                ItemNote = a.ItemNote != null ? a.ItemNote : "",
                                //ItemTotalAmount = a.ItemTotalAmount,
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
                                MRP = b.MRP,
                                ProjectId=a.ProjectId,
                                TaskId=a.TaskId,
                                ProjectName=p.ProjectName,
                                TaskName=t.TaskName,
                                ItemMake = h != null ? h.ItemBrandID : 0,
                                ItemMakeName = h != null ? h.ItemBrandName : ""
                            });
                DVItems.AddRange(temp2);
            }
            DVitemsGroupBy = (from a in DVItems
                              group new { a.Item, a.ItemQuantity, a.ItemUnit, a.ItemDiscount, a.note, a.ItemNote, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnitID, a.SubUnitId, a.PriUnit, a.SubUnit, a.BasePrice, a.SellingPrice, a.PurchasePrice, a.MRP,a.ProjectId,a.TaskId ,a.ProjectName,a.TaskName,a.ItemMake,a.ItemMakeName } by new { a.Item } into g
                              select new ItemList2
                              {
                                  Item = g.FirstOrDefault().Item,
                                  ItemQuantity = g.Sum(k => -k.ItemQuantity),
                                  ItemUnit = g.FirstOrDefault().ItemUnit,
                                  //ItemUnitPrice = g.FirstOrDefault().ItemUnitPrice,
                                  //ItemTax = g.FirstOrDefault().ItemTax,
                                  ItemDiscount = g.Sum(k => -k.ItemDiscount),
                                  note = g.FirstOrDefault().note,
                                  ItemNote = g.FirstOrDefault().ItemNote,
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
                                  MRP = g.FirstOrDefault().MRP,
                                  ProjectId= g.FirstOrDefault().ProjectId,
                                  TaskId= g.FirstOrDefault().TaskId,
                                  ProjectName= g.FirstOrDefault().ProjectName,
                                  TaskName= g.FirstOrDefault().TaskName,
                                  ItemMake= g.FirstOrDefault().ItemMake,
                                  ItemMakeName= g.FirstOrDefault().ItemMakeName,
                              }).ToList();
            var ConD = (from a in db.PurchaseOrderItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                        from p in proj.DefaultIfEmpty()
                        join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                        from t in protask.DefaultIfEmpty()
                        join h in db.ItemBrands on a.Make equals h.ItemBrandID into brn
                        from h in brn.DefaultIfEmpty()
                        where a.PurchaseOrder == POrderID && a.ItemNote != "-:{Bundle_Item}"
                        select new ItemList2
                        {
                            Item = a.Item,
                            ItemQuantity = a.ItemQuantity,
                            ItemUnit = a.ItemUnit,
                            //ItemUnitPrice = a.ItemUnitPrice,
                            //ItemTax = a.ItemTax,
                            //ItemSubTotal = a.ItemSubTotal,
                            //ItemTaxAmount = a.ItemTaxAmount,
                            ItemDiscount = a.ItemDiscount,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
                            //ItemTotalAmount = a.ItemTotalAmount,
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
                            MRP = b.MRP,
                            ProjectId = a.ProjectId,
                            TaskId = a.TaskId,
                            ProjectName = p.ProjectName,
                            TaskName = t.TaskName,
                            ItemMake = h != null ? h.ItemBrandID : 0,
                            ItemMakeName = h != null ? h.ItemBrandName : ""
                        });
            DVitemsGroupBy.AddRange(ConD);
            RemainingItems = (from a in DVitemsGroupBy
                              group new { a.Item, a.ItemQuantity, a.ItemUnit, a.ItemDiscount, a.note, a.ItemNote, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnitID, a.SubUnitId, a.PriUnit, a.SubUnit, a.BasePrice, a.SellingPrice, a.PurchasePrice, a.MRP, a.ProjectId, a.TaskId, a.ProjectName, a.TaskName, a.ItemMake, a.ItemMakeName } by new { a.Item } into g
                              select new ItemList2
                              {
                                  Item = g.FirstOrDefault().Item,
                                  ItemQuantity = g.Sum(k => k.ItemQuantity),
                                  ItemUnit = g.FirstOrDefault().ItemUnit,
                                  //ItemUnitPrice = g.FirstOrDefault().ItemUnitPrice,
                                  //ItemTax = g.FirstOrDefault().ItemTax,
                                  ItemDiscount = g.Sum(k => k.ItemDiscount),
                                  note = g.FirstOrDefault().note,
                                  ItemNote = g.FirstOrDefault().ItemNote,
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
                                  MRP = g.FirstOrDefault().MRP,
                                  ProjectId = g.FirstOrDefault().ProjectId,
                                  TaskId = g.FirstOrDefault().TaskId,
                                  ProjectName = g.FirstOrDefault().ProjectName,
                                  TaskName = g.FirstOrDefault().TaskName,
                                  ItemMake = g.FirstOrDefault().ItemMake,
                                  ItemMakeName = g.FirstOrDefault().ItemMakeName,
                              }).ToList();
            RemainingItems = RemainingItems.Where(a => a.ItemQuantity != 0).ToList();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(RemainingItems);
            return Json(result);
        }
        [HttpGet]
        public JsonResult GetMultiPOItems(string POrderID)
        {
            long[] POrder = POrderID.Split(',').Select(Int64.Parse).ToArray();

            var ConD = (from a in db.PurchaseOrderItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                        from p in proj.DefaultIfEmpty()
                        join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                        from t in protask.DefaultIfEmpty()
                        where POrder.Contains(a.PurchaseOrder) && a.ItemNote != "-:{Bundle_Item}"
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
                            a.ProjectId,
                            a.TaskId,
                            p.ProjectName,
                            t.TaskName
                        }).ToList();

            return Json(ConD);
        }
        [HttpGet]
        public JsonResult GetMultiPOItems2(string POrderID, string ConvertTo)
        {
            long[] POrder = POrderID.Split(',').Select(Int64.Parse).ToArray();

            var temp = db.ConvertTransactionss.Where(a => POrder.Contains(a.From) && a.ConvertFrom == "POrder" && a.ConvertTo == ConvertTo).Select(a => a.To).ToArray();
            List<ItemList2> DVItems = new List<ItemList2>();
            List<ItemList2> temp6 = new List<ItemList2>();
            List<ItemList2> DVitemsGroupBy = new List<ItemList2>();
            List<ItemList2> RemainingItems = new List<ItemList2>();
            
                var temp2 = (from a in db.MRNoteItems
                             join b in db.Items on a.Item equals b.ItemID
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                             from p in proj.DefaultIfEmpty()
                             join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                             from t in protask.DefaultIfEmpty()
                             join h in db.ItemBrands on a.Make equals h.ItemBrandID into brn
                             from h in brn.DefaultIfEmpty()
                             where temp.Contains(a.MRNote)
                             select new ItemList2
                             {
                                 Item = a.Item,
                                 ItemQuantity = a.ItemQuantity,
                                 ItemUnit = a.ItemUnit,
                                 //ItemUnitPrice = a.ItemUnitPrice,
                                 //ItemTax = a.ItemTax,
                                 //ItemSubTotal = a.ItemSubTotal,
                                 //ItemTaxAmount = a.ItemTaxAmount,
                                 ItemDiscount = a.ItemDiscount,
                                 note = a.ItemNote.Replace("<br />", "\n"),
                                 ItemNote = a.ItemNote != null ? a.ItemNote : "",
                                 //ItemTotalAmount = a.ItemTotalAmount,
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
                                 MRP = b.MRP,
                                 ProjectId = a.ProjectId,
                                 TaskId = a.TaskId,
                                 ProjectName = p.ProjectName,
                                 TaskName = t.TaskName,
                                 ItemMake = h != null ? h.ItemBrandID : 0,
                                 ItemMakeName = h != null ? h.ItemBrandName : ""
                             });
                DVItems.AddRange(temp2);
            DVitemsGroupBy = (from a in DVItems
                              group new { a.Item, a.ItemQuantity, a.ItemUnit, a.ItemDiscount, a.note, a.ItemNote, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnitID, a.SubUnitId, a.PriUnit, a.SubUnit, a.BasePrice, a.SellingPrice, a.PurchasePrice, a.MRP, a.ProjectId, a.TaskId, a.ProjectName, a.TaskName, a.ItemMake, a.ItemMakeName } by new { a.Item ,a.ProjectId, a.TaskId } into g
                              select new ItemList2
                              {
                                  Item = g.FirstOrDefault().Item,
                                  ItemQuantity = g.Sum(k => -k.ItemQuantity),
                                  ItemUnit = g.FirstOrDefault().ItemUnit,
                                  //ItemUnitPrice = g.FirstOrDefault().ItemUnitPrice,
                                  //ItemTax = g.FirstOrDefault().ItemTax,
                                  ItemDiscount = g.Sum(k => -k.ItemDiscount),
                                  note = g.FirstOrDefault().note,
                                  ItemNote = g.FirstOrDefault().ItemNote,
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
                                  MRP = g.FirstOrDefault().MRP,
                                  ProjectId = g.FirstOrDefault().ProjectId,
                                  TaskId = g.FirstOrDefault().TaskId,
                                  ProjectName = g.FirstOrDefault().ProjectName,
                                  TaskName = g.FirstOrDefault().TaskName,
                                  ItemMake = g.FirstOrDefault().ItemMake,
                                  ItemMakeName = g.FirstOrDefault().ItemMakeName,
                              }).ToList();
            

            var ConD = (from a in db.PurchaseOrderItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                        from p in proj.DefaultIfEmpty()
                        join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                        from t in protask.DefaultIfEmpty()
                        join h in db.ItemBrands on a.Make equals h.ItemBrandID into brn
                        from h in brn.DefaultIfEmpty()
                        where POrder.Contains(a.PurchaseOrder) && a.ItemNote != "-:{Bundle_Item}"
                        select new ItemList2
                        {
                            Item = a.Item,
                            ItemQuantity = a.ItemQuantity,
                            ItemUnit = a.ItemUnit,
                            //ItemUnitPrice = a.ItemUnitPrice,
                            //ItemTax = a.ItemTax,
                            //ItemSubTotal = a.ItemSubTotal,
                            //ItemTaxAmount = a.ItemTaxAmount,
                            ItemDiscount = a.ItemDiscount,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
                            //ItemTotalAmount = a.ItemTotalAmount,
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
                            MRP = b.MRP,
                            ProjectId = a.ProjectId,
                            TaskId = a.TaskId,
                            ProjectName = p.ProjectName,
                            TaskName = t.TaskName,
                            ItemMake = h != null ? h.ItemBrandID : 0,
                            ItemMakeName = h != null ? h.ItemBrandName : ""
                        });
            DVitemsGroupBy.AddRange(ConD);
            RemainingItems = (from a in DVitemsGroupBy
                              group new { a.Item, a.ItemQuantity, a.ItemUnit, a.ItemDiscount, a.note, a.ItemNote, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnitID, a.SubUnitId, a.PriUnit, a.SubUnit, a.BasePrice, a.SellingPrice, a.PurchasePrice, a.MRP, a.ProjectId, a.TaskId, a.ProjectName, a.TaskName, a.ItemMake, a.ItemMakeName } by new { a.Item, a.ProjectId, a.TaskId } into g
                              select new ItemList2
                              {
                                  Item = g.FirstOrDefault().Item,
                                  ItemQuantity = g.Sum(k => k.ItemQuantity),
                                  ItemUnit = g.FirstOrDefault().ItemUnit,
                                  //ItemUnitPrice = g.FirstOrDefault().ItemUnitPrice,
                                  //ItemTax = g.FirstOrDefault().ItemTax,
                                  ItemDiscount = g.Sum(k => k.ItemDiscount),
                                  note = g.FirstOrDefault().note,
                                  ItemNote = g.FirstOrDefault().ItemNote,
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
                                  MRP = g.FirstOrDefault().MRP,
                                  ProjectId = g.FirstOrDefault().ProjectId,
                                  TaskId = g.FirstOrDefault().TaskId,
                                  ProjectName = g.FirstOrDefault().ProjectName,
                                  TaskName = g.FirstOrDefault().TaskName,
                                  ItemMake = g.FirstOrDefault().ItemMake,
                                  ItemMakeName = g.FirstOrDefault().ItemMakeName,
                              }).ToList();
            RemainingItems = RemainingItems.Where(a => a.ItemQuantity != 0).ToList();

            return Json(RemainingItems);
        }

        public JsonResult SearchPOrder(string q, string page, long supplier = -1)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.PurchaseOrders.Where(p => p.Supplier == supplier && (p.BillNo.ToLower().Contains(q.ToLower()) || p.BillNo.Contains(q)))
                        .Select(b => new SelectFormat
                        {
                            text = b.BillNo, //each json object will have 
                            id = b.PurchaseOrderId
                        }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.PurchaseOrders.Where(a => a.Supplier == supplier).Select(b => new SelectFormat
                {
                    text = b.BillNo, //each json object will have 
                    id = b.PurchaseOrderId
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            return Json(serialisedJson);
        }

        private string InvoiceNo(Int64 PNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "PurchaseOrder").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "PurchaseOrder").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.PurchaseOrders.Select(p => p.PONo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PNo = db.PurchaseOrders.Max(p => p.PONo + 1);
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
            var Exists = db.PurchaseOrders.Any(c => c.BillNo == QENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetPONo()
        {
            Int64 PNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "PurchaseOrder").Select(a => a.number).FirstOrDefault();
            if ((db.PurchaseOrders.Select(p => p.PONo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                PNo = (number == 0) ? 1 : number;
            }
            else
            {
                PNo = db.PurchaseOrders.Max(p => p.PONo + 1);
            }

            return PNo;
        }

        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "PurchaseOrder" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.PurchaseOrders.Where(a => a.PurchaseOrderId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "PurchaseOrder").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
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
                AppUp.Type = "PurchaseOrder";

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
                            join d in db.PurchaseOrders on b.TransEntry equals d.PurchaseOrderId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedUserId equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "PurchaseOrder"
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

        //Function To Upload Files in Create and Edit Mode
        public ActionResult UploadFiles()
        {
            if (Request.Form.Files.Count > 0)
            {
                string PurchOrdId = Request.Form.GetValues("id").First();
                long POId = 0;

                if(PurchOrdId.Contains("undefined"))
                {
                    var LastId = db.PurchaseOrders.OrderByDescending(a => a.PurchaseOrderId).FirstOrDefault();
                    POId = LastId.PurchaseOrderId;
                }
                else
                {
                    POId = Convert.ToInt64(PurchOrdId);
                }

                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/PurchaseOrderDocuments/");

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
                                        thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/PurchaseOrderDocuments/"), thumbName);

                                        resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/PurchaseOrderDocuments/"), resizeName);
                                        newFName = "resize_" + newFName;
                                        FStatus = Status.inactive;
                                    }
                                    else
                                    {
                                        var commonfilename = "Docs-Thump.png";
                                    }

                                    newName = Path.Combine(LegacyWeb.MapPath("~/uploads/PurchaseOrderDocuments/"), newName);
                                    file.SaveAs(newName);

                                  var PODocument = new AttachmentDocuments
                                  {
                                    TransactionID   =   POId,
                                    TransactionType =   "Purchase Order",
                                    FileName        =   newFName,
                                    Status          =   FStatus,
                                    CreatedDate     =   Convert.ToDateTime(System.DateTime.Now)
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/PurchaseOrderDocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/PurchaseOrderDocuments/"), resizeName);
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

        //To remove the attached file in Edit Mode(when pressing delete button in the attached file)
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit PurchaseOrder")]
        public JsonResult ImageDelete(long key)
        {
            //To remove the attached file(single row) from database
            AttachmentDocuments Document = db.AttachmentDocuments.Find(key);
            db.AttachmentDocuments.Remove(Document);
            db.SaveChanges();

            //To remove the attached file from folder
            string fullpath = LegacyWeb.MapPath("~/uploads/PurchaseOrderDocuments/" + Document.FileName);

            if (System.IO.File.Exists(fullpath))
            {
                System.IO.File.Delete(fullpath);
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "PurchaseOrder", "AttachmentDocuments", findip(), Document.DocumentID, "Purchase Order Document Deleted Successfully");

            bool status = true;
            string message = "Successfully deleted Purchase Order attachment details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = status, message = message, Id = key } };
        }

    }
}
