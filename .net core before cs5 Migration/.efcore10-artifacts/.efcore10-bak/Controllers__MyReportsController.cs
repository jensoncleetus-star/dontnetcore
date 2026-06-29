using QuickSoft.Web;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
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
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Runtime;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class MyReportsController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public MyReportsController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Reports

        #region pro Forma
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report Pro Forma")]
        public ActionResult ProForma()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);


            ViewBag.Customer = OpAll;

            companySet();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");

            ViewBag.getProj = QkSelect.List(
                             new List<SelectListItem>
                             {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;


            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Report Pro Forma")]
        public ActionResult GetProForma(string pfno, long? customer, string fromdate, string todate, long? ddlMC, long? SType, long? HireType, string HdateFrom, string HdateTo, long? project, long? task)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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

            SaleType St = new SaleType();
            DateTime? hfdate = null;
            DateTime? htdate = null;
            if (SType == 2)
            {

                if (SType != null)
                {
                    St = (SType == 1) ? SaleType.Sale : SaleType.Hire;
                };

                if (HdateFrom != "")
                {
                    hfdate = DateTime.Parse(HdateFrom, new CultureInfo("en-GB"));
                }
                if (HdateTo != "")
                {
                    htdate = DateTime.Parse(HdateTo, new CultureInfo("en-GB"));
                }
            }
            else
            {
                St = SaleType.Sale;
                hfdate = null;
                htdate = null;
            }
            ProForma PFentry = new ProForma();
            var v = (from a in db.ProFormas
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join d in db.Employees on a.PFCashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.MCs on a.MaterialCenter equals e.MCId into mcs
                     from e in mcs.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id into user
                     from g in user.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.ProFormaId, h2 = "Proforma" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where (pfno == "" || a.BillNo == pfno) &&
                        (customer == 0 || a.Customer == customer) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.PFDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.PFDate, tdate) >= 0)
                        && (SType == null || a.SaleType == St) &&
                        (HireType == 0 || HireType == null || h.HireType == HireType) &&
                        (HdateFrom == "" || HdateFrom == null || EF.Functions.DateDiffDay(h.StartDate, hfdate) <= 0) &&
                        (HdateTo == "" || HdateTo == null || EF.Functions.DateDiffDay(h.EndDate, htdate) >= 0)
                        && (!MCList.Any() || MCArray.Contains(a.MaterialCenter) || ddlMC == a.MaterialCenter)
                        && (project == 0 || project == null || a.Project == project)
                        && (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         a.ProFormaId,
                         a.PFNo,
                         a.BillNo,
                         a.PFDate,
                         a.PFGrandTotal,
                         TaxRegNo = i.TRN,
                         Customer = b.CustomerName,
                         EmpName = d.FirstName + " " + d.LastName,
                         User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.UserName).FirstOrDefault(),
                         MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All",
                         a.PFCreatedDate,
                         SaleType = a.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType
                     }).OrderBy(a => a.PFDate).ThenBy(a => a.PFCreatedDate);

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


        #endregion

        #region quotation
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report Quotation")]
        public ActionResult Quotation()
        {
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            ViewBag.Customer = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);

            companySet();

            ViewBag.Prjct = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                             }, "Value", "Text", 1);
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");


            ViewBag.getProj = QkSelect.List(
                             new List<SelectListItem>
                             {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Report Quotation")]
        public ActionResult GetQuotation(string quno, long? customer, string fromdate, string todate, long? project, long? SType, long? HireType, string HdateFrom, string HdateTo, long? task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key


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

            SaleType St = new SaleType();
            DateTime? hfdate = null;
            DateTime? htdate = null;
            if (SType == 2)
            {

                if (SType != null)
                {
                    St = (SType == 1) ? SaleType.Sale : SaleType.Hire;
                };

                if (HdateFrom != "")
                {
                    hfdate = DateTime.Parse(HdateFrom, new CultureInfo("en-GB"));
                }
                if (HdateTo != "")
                {
                    htdate = DateTime.Parse(HdateTo, new CultureInfo("en-GB"));
                }
            }
            else
            {
                St = SaleType.Sale;
                hfdate = null;
                htdate = null;
            }
            var v = (from b in db.Quotations
                     join c in db.Customers on b.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()
                     join f in db.Employees on b.QuotCashier equals f.EmployeeId into emp
                     from f in emp.DefaultIfEmpty()
                     join g in db.Projects on b.Project equals g.ProjectId into pjct
                     from g in pjct.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = b.QuotationId, h2 = "Quotation" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on c.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join k in db.ProTasks on b.ProTask equals k.ProTaskId into tas
                     from k in tas.DefaultIfEmpty()
                     where (quno == "" || b.BillNo == quno) &&
                        (customer == 0 || b.Customer == customer) && (project == null || project == 0 || b.Project == project) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(b.QuotDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(b.QuotDate, tdate) >= 0)
                        && (SType == null || b.SaleType == St) &&
                        (HireType == 0 || HireType == null || h.HireType == HireType) &&
                        (HdateFrom == "" || HdateFrom == null || EF.Functions.DateDiffDay(h.StartDate, hfdate) <= 0) &&
                        (HdateTo == "" || HdateTo == null || EF.Functions.DateDiffDay(h.EndDate, htdate) >= 0)
                        && (project == 0 || project == null || b.Project == project)
                        && (task == 0 || task == null || b.ProTask == task)
                     select new
                     {
                         b.QuotationId,
                         b.QuotNo,
                         b.BillNo,
                         b.QuotDate,
                         b.QuotItems,
                         b.QuotItemQuantity,
                         b.QuotDiscount,
                         b.QuotGrandTotal,
                         b.QuotTax,
                         TaxRegNo = i.TRN,
                         Customer = c.CustomerName,
                         ProjectName = (g.ProjectName != null && g.ProjectName != "") ? g.ProCode + "-" + g.ProjectName : "",
                         Task = (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",

                         EmpName = f.FirstName + " " + f.LastName,
                         User = db.Users.Where(s => s.Id == b.CreatedUserId).Select(s => s.UserName).FirstOrDefault(),
                         b.QuotTaxAmount,
                         // d.BranchName,
                         validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.QuotDate, (b.QuotValidity == null) ? 0 : b.QuotValidity + 1)) ? "Active" : "Expired",
                         b.QuotCreatedDate,
                         SaleType = b.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType
                     }).OrderBy(a => a.QuotDate).ThenBy(a => a.QuotCreatedDate);

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

        #endregion

        #region purchasequotation
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Report Quotation")]
        public ActionResult PurchaseQuotation()
        {
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            ViewBag.Supplier = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.Employee = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            companySet();


            ViewBag.getProj = QkSelect.List(
                            new List<SelectListItem>
                            {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Report Quotation")]
        public ActionResult GetPurchaseQuotation(string quno, long? supplier, long? employee, string fromdate, string todate, long? project, long? Task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            var v = (from b in db.PurchaseQuotations
                     join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                     from c in supp.DefaultIfEmpty()
                     join f in db.Employees on b.PQuotCashier equals f.EmployeeId into emp
                     from f in emp.DefaultIfEmpty()
                     join g in db.Projects on b.Project equals g.ProjectId into pjct
                     from g in pjct.DefaultIfEmpty()
                     join h in db.Accountss on c.Accounts equals h.AccountsID into acc
                     from h in acc.DefaultIfEmpty()
                     join i in db.ProTasks on b.ProTask equals i.ProTaskId into task
                     from i in task.DefaultIfEmpty()

                     where (quno == "" || b.BillNo == quno) &&
                        (supplier == 0 || b.Supplier == supplier) && (project == null || project == 0 || b.Project == project) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(b.PQuotDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(b.PQuotDate, tdate) >= 0) &&
                        (Task == 0 || Task == null || b.ProTask == Task)

                     select new
                     {
                         b.PQuotationId,
                         b.PQuotNo,
                         b.BillNo,
                         b.PQuotDate,
                         b.PQuotItems,
                         b.PQuotItemQuantity,
                         b.PQuotDiscount,
                         b.PQuotGrandTotal,
                         b.PQuotTax,
                         TaxRegNo = h.TRN,
                         Supplier = c.SupplierName,
                         ProjectName = (g.ProjectName != null && g.ProjectName != "") ? g.ProCode + "-" + g.ProjectName : "",
                         Task = (i.TaskName != null && i.TaskName != "") ? i.TaskCode + "-" + i.TaskName : "",

                         EmpName = f.FirstName + " " + f.LastName,
                         User = db.Users.Where(s => s.Id == b.CreatedUserId).Select(s => s.UserName).FirstOrDefault(),
                         b.PQuotTaxAmount,
                         validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.PQuotDate, (b.PQuotValidity == null) ? 0 : b.PQuotValidity + 1)) ? "Active" : "Expired",
                         b.PQuotCreatedDate,

                     }).OrderBy(a => a.PQuotDate).ThenBy(a => a.PQuotCreatedDate);

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

        #endregion

        #region purchasereturn
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report Purchase Return")]
        public ActionResult PurchaseReturn()
        {

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.Supplier = OpAll;

            companySet();

            return View();

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Report Purchase Return")]
        public ActionResult GetPurchaseReturn(string vno, long? type, long? supplier, string fromdate, string todate, long? ddlMC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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

            PurchaseReturn pentry = new PurchaseReturn();
            if (type == 1)
            {
                pentry.SupplierType = SupplierType.CashSale;
            }
            else if (type == 0)
            {
                pentry.SupplierType = SupplierType.CreditSale;
            }
            else { }


            var v = (from b in db.PurchaseReturns
                     join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                     from c in supp.DefaultIfEmpty()
                     join d in db.Branchs on b.Branch equals d.BranchID into brn
                     from d in brn.DefaultIfEmpty()
                     join e in db.PRPayments on b.PurchaseReturnId equals e.PurchaseReturnId into pay
                     from e in pay.DefaultIfEmpty()
                     join f in db.Employees on b.PRCashier equals f.EmployeeId into usr
                     from f in usr.DefaultIfEmpty()
                     join g in db.MCs on b.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join h in db.Accountss on c.Accounts equals h.AccountsID into acc
                     from h in acc.DefaultIfEmpty()
                     where (vno == "" || b.BillNo == vno) &&
                            (supplier == 0 || b.Supplier == supplier) &&
                            (type == null || b.SupplierType == pentry.SupplierType) &&
                            (fromdate == "" || EF.Functions.DateDiffDay(b.PRDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(b.PRDate, tdate) >= 0)
                            && (!MCList.Any() || MCArray.Contains(b.MaterialCenter) || ddlMC == b.MaterialCenter)
                     select new
                     {
                         b.PurchaseReturnId,
                         b.BillNo,
                         b.PRDate,
                         b.PRGrandTotal,
                         b.SupplierType,
                         TaxRegNo = h.TRN,
                         Supplier = c.SupplierName,
                         EmpName = f.FirstName + " " + f.LastName,
                         User = db.Users.Where(s => s.Id == b.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                         b.PayType,
                         PaymentStatus = b.Status,
                         PaymentTrans = db.PRTransactions.Any(k => k.PurchaseReturnId == b.PurchaseReturnId),
                         Branch = d.BranchName,
                         e.PReturnAmount,
                         BalanceAmount = b.PRGrandTotal - e.PReturnAmount,
                         b.PRCreatedDate,
                         MCName = (ddlMC != 0 || ddlMC != null) ? g.MCName : "All",
                     }).OrderBy(a => a.PRDate).ThenBy(a => a.PRCreatedDate);

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


        #endregion

        #region purchase Return Invoice itemwise
        [QkAuthorize(Roles = "Dev,PRInvoice ItemWise")]
        public ActionResult PRInvoiceItemWise()
        {
            var select = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.Item = select;
            ViewBag.Supplier = select;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [QkAuthorize(Roles = "Dev,PRInvoice ItemWise")]
        public ActionResult PRInvoiceWise(long? ddlItem, long? ddlSupplier, string from, string to, long? ddlMC)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            if (ddlItem != 0)
            {
                ViewBag.item = (from a in db.Items
                                where a.ItemID == ddlItem
                                select new
                                {
                                    ItemName = a.ItemCode + "-" + a.ItemName
                                }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.item = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.supplier = ddlSupplier;
            ViewBag.ddlmc = ddlMC;
            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,PRInvoice ItemWise")]
        public ActionResult getPRInvoiceWise(long? item, long? supplier, string fromdate, string to, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB"));
            }

            var v = (from a in db.PRItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.PurchaseReturns on a.PurchaseReturnId equals g.PurchaseReturnId
                     join c in db.Suppliers on g.Supplier equals c.SupplierID
                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || a.Item == item) && (supplier == 0 || g.Supplier == supplier)
                     && (fromdate == "" || EF.Functions.DateDiffDay(g.PRDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(g.PRDate, tdate) >= 0)
                     && (!MCList.Any() || MCArray.Contains(g.MaterialCenter) || ddmc == g.MaterialCenter)
                     select new
                     {
                         b.ItemID,
                         b.ItemName,
                         b.ItemCode,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Unit = (a.ItemUnit == b.ItemUnitID) ? e.ItemUnitName : f.ItemUnitName,
                         g.BillNo,
                         a.ItemQuantity,
                         a.ItemSubTotal,
                         a.ItemTaxAmount,
                         a.ItemTotalAmount,
                         a.ItemDiscount,
                         a.itemNote,
                         a.ItemUnitPrice,
                         PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.SellingPrice,
                         Supplier = c.SupplierName,
                         Date = g.PRDate
                     }).AsEnumerable().OrderBy(a => a.ItemName);

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
        #endregion

        #region salesreturn Not Using
        [HttpGet]
        public ActionResult SalesReturn()
        {
            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);


            ViewBag.Branch = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);

            ViewBag.getProj = QkSelect.List(
                                new List<SelectListItem>
                                {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            companySet();
            return View();
        }
        [HttpPost]
        public ActionResult GetSalesReturn(string vno, long? branch, long? type, long? customer, string fromdate, string todate, long? project, long? task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            SalesReturn sEntry = new SalesReturn();
            if (type == 1)
            {
                sEntry.CustomerType = CustomerType.Walking;

                var v = (from a in db.SalesReturns
                         join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                         join d in db.Employees on a.SRCashier equals d.EmployeeId
                         join e in db.Branchs on a.Branch equals e.BranchID

                         where (vno == "" || a.BillNo == vno) &&
                            (customer == 0 || a.Customer == customer) &&
                            (branch == 0 || a.Branch == branch) &&
                           (type == null || a.CustomerType == sEntry.CustomerType) &&
                            (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0)
                            && (project == 0 || project == null || a.Project == project)
                            && (task == 0 || task == null || a.ProTask == task)
                         select new
                         {
                             a.SalesReturnId,
                             a.BillNo,
                             a.SRDate,
                             a.SRGrandTotal,
                             CustomerName = "",
                             // Customer = b.CustomerCode + " - " + b.CustomerName,
                             EmpName = d.FirstName + " " + d.LastName,
                             User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                             a.PayType,
                             PaymentStatus = a.Status,
                             PaymentTrans = db.SETransactions.Any(k => k.SalesEntry == a.SalesEntryId),
                             Branch = e.BranchName,
                             c.SReturnAmount,
                             a.CustomerType,
                             SRBalanceAmount = a.SRGrandTotal - c.SReturnAmount,
                             a.SRCreatedDate
                         }).OrderBy(a => a.SRDate).ThenBy(a => a.SRCreatedDate);

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
            else if (type == 0)
            {
                sEntry.CustomerType = CustomerType.Customer;

                var v = (from a in db.SalesReturns
                         join b in db.Customers on a.Customer equals b.CustomerID into cust
                         from b in cust.DefaultIfEmpty()
                         join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                         join d in db.Employees on a.SRCashier equals d.EmployeeId
                         join e in db.Branchs on a.Branch equals e.BranchID
                         join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                         from i in acc.DefaultIfEmpty()
                         where (vno == "" || a.BillNo == vno) &&
                            (customer == 0 || a.Customer == customer) &&
                            (branch == 0 || a.Branch == branch) &&
                          (type == null || a.CustomerType == sEntry.CustomerType) &&
                           (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0)
                            && (project == 0 || project == null || a.Project == project)
                            && (task == 0 || task == null || a.ProTask == task)
                         select new
                         {
                             a.SalesReturnId,
                             a.BillNo,
                             a.SRDate,
                             a.SRGrandTotal,
                             TaxRegNo = i.TRN,
                             Customer = b.CustomerName,
                             EmpName = d.FirstName + " " + d.LastName,
                             User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                             a.PayType,
                             PaymentStatus = a.Status,
                             PaymentTrans = db.SETransactions.Any(k => k.SalesEntry == a.SalesEntryId),
                             Branch = e.BranchName,
                             c.SReturnAmount,
                             a.CustomerType,
                             SRBalanceAmount = a.SRGrandTotal - c.SReturnAmount,
                             a.SRCreatedDate
                         }).OrderBy(a => a.SRDate);


                recordsTotal = v.Count();
                var data = v.ToList();

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
            {
                sEntry.CustomerType = CustomerType.Customer;

                var v = (from a in db.SalesReturns
                         join b in db.Customers on a.Customer equals b.CustomerID into cust
                         from b in cust.DefaultIfEmpty()
                         join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                         join d in db.Employees on a.SRCashier equals d.EmployeeId
                         join e in db.Branchs on a.Branch equals e.BranchID
                         join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                         from i in acc.DefaultIfEmpty()
                         where (vno == "" || a.BillNo == vno) &&
                            (customer == 0 || a.Customer == customer) &&
                            (branch == 0 || a.Branch == branch) &&
                            (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0)
                            && (project == 0 || project == null || a.Project == project)
                            && (task == 0 || task == null || a.ProTask == task)
                         select new
                         {
                             a.SalesReturnId,
                             a.BillNo,
                             a.SRDate,
                             a.SRGrandTotal,
                             CustomerName = "",
                             Customer = b.CustomerName,
                             TaxRegNo = i.TRN,
                             EmpName = d.FirstName + " " + d.LastName,
                             User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                             a.PayType,
                             PaymentStatus = a.Status,
                             PaymentTrans = db.SETransactions.Any(k => k.SalesEntry == a.SalesEntryId),
                             Branch = e.BranchName,
                             c.SReturnAmount,
                             a.CustomerType,
                             SRBalanceAmount = a.SRGrandTotal - c.SReturnAmount,
                             a.SRCreatedDate
                         }).OrderBy(a => a.SRDate);


                recordsTotal = v.Count();
                var data = v.ToList();

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
        }


        #endregion

        #region creditsalereturn
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report Sales Return")]
        public ActionResult CreditSaleReturn()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                         new List<SelectListItem>
                         {
                          new SelectListItem { Selected = true, Text = "All", Value = "0"},
                         }, "Value", "Text", 1);

            ViewBag.Customer = OpAll;
            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.getProj = QkSelect.List(
                             new List<SelectListItem>
                             {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            companySet();

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Report Sales Return")]
        public ActionResult GetCreditSaleReturn(string vno, long? type, long? customer, string fromdate, string todate, long? ddMC, long? project, long? task)
        {

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            SalesReturn sEntry = new SalesReturn();
            sEntry.CustomerType = (type == 1) ? CustomerType.Walking : CustomerType.Customer;

            var v = (from a in db.SalesReturns
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId into pay
                     from c in pay.DefaultIfEmpty()
                     join d in db.Employees on a.SRCashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.MCs on a.MaterialCenter equals e.MCId into mcs
                     from e in mcs.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()

                     where (vno == "" || a.BillNo == vno)
                     && (customer == 0 || a.Customer == customer)
                        && (type == null || a.CustomerType == sEntry.CustomerType)
                         && (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0)
                       && (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0)
                       && (!MCList.Any() || MCArray.Contains(a.MaterialCenter) || ddMC == a.MaterialCenter)
                        && (project == 0 || project == null || a.Project == project)
                        && (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         a.SalesReturnId,
                         a.BillNo,
                         a.SRDate,
                         a.SRGrandTotal,
                         Customer = b.CustomerName,
                         TaxRegNo = i.TRN,
                         EmpName = d.FirstName + " " + d.LastName,
                         User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                         a.PayType,
                         PaymentStatus = a.Status,
                         PaymentTrans = db.SETransactions.Any(k => k.SalesEntry == a.SalesEntryId),
                         //  Branch = e.BranchName,
                         MCName = (ddMC != 0 || ddMC != null) ? e.MCName : "All",
                         c.SReturnAmount,
                         a.CustomerType,
                         SRBalanceAmount = a.SRGrandTotal - c.SReturnAmount,
                         a.SRCreatedDate
                     }).OrderBy(a => a.SRDate);

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


        #endregion

        #region Sales Return Invoice item wise
        // [QkAuthorize(Roles = "Dev,SReturn Invoice ItemWise")]
        public ActionResult SRInvoiceItemWise()
        {
            var select = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);
            ViewBag.Customer = select;
            ViewBag.Item = select;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            var OptAll = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);

            ViewBag.Category = OptAll;
            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            return View();
        }
        //  [QkAuthorize(Roles = "Dev,SReturn Invoice ItemWise")]
        public ActionResult SRInvoiceWise(long? ddlItem, long? ddlCustomer, long? ddlItemCategory, string from, string to, long? ddlMC)
        {
            if (ddlItem != 0)
            {
                ViewBag.item = (from a in db.Items
                                where a.ItemID == ddlItem
                                select new
                                {
                                    ItemName = a.ItemCode + "-" + a.ItemName
                                }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.item = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.customer = ddlCustomer;
            companySet();
            return View();
        }
        // [QkAuthorize(Roles = "Dev,SReturn Invoice ItemWise")]
        public ActionResult getSRInvoiceWise(long? item, long? customer, long? category, string fromdate, string to, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB"));
            }

            var v = (from a in db.SRItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.SalesReturns on a.SalesReturnId equals g.SalesReturnId
                     join c in db.Customers on g.Customer equals c.CustomerID
                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || a.Item == item) && (customer == 0 || g.Customer == customer)
                     && (fromdate == "" || EF.Functions.DateDiffDay(g.SRDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(g.SRDate, tdate) >= 0)
                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(g.MaterialCenter) || ddmc == g.MaterialCenter)
                     && (category == 0 || b.ItemCategoryID == category)
                     select new
                     {
                         b.ItemID,
                         b.ItemName,
                         b.ItemCode,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Unit = (a.ItemUnit == b.ItemUnitID) ? e.ItemUnitName : f.ItemUnitName,
                         g.BillNo,
                         a.ItemQuantity,
                         a.ItemSubTotal,
                         a.ItemTaxAmount,
                         a.ItemTotalAmount,
                         a.ItemDiscount,
                         a.itemNote,
                         a.ItemUnitPrice,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.SellingPrice,
                         Customer = c.CustomerName,
                         Date = g.SRDate
                     }).AsEnumerable().OrderBy(a => a.ItemName);

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
        #endregion

        #region deliverynote
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report Deliverynote")]
        public ActionResult Deliverynote()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.Customer = OpAll;

            companySet();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");


            ViewBag.getProj = QkSelect.List(
                              new List<SelectListItem>
                              {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                              }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            return View();
        }
        [QkAuthorize(Roles = "Dev,Report Deliverynote")]
        public ActionResult GetDeliverynote(string quno, long? customer, string fromdate, string todate, long? ddlMC, long? SType, long? HireType, string HdateFrom, string HdateTo, long? project, long? task)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

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
            SaleType St = new SaleType();
            DateTime? hfdate = null;
            DateTime? htdate = null;
            if (SType == 2)
            {

                if (SType != null)
                {
                    St = (SType == 1) ? SaleType.Sale : SaleType.Hire;
                };

                if (HdateFrom != "")
                {
                    hfdate = DateTime.Parse(HdateFrom, new CultureInfo("en-GB"));
                }
                if (HdateTo != "")
                {
                    htdate = DateTime.Parse(HdateTo, new CultureInfo("en-GB"));
                }
            }
            else
            {
                St = SaleType.Sale;
                hfdate = null;
                htdate = null;
            }
            var v = (from b in db.Deliverynotes
                     join c in db.Customers on b.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()
                     join f in db.Employees on b.DvCashier equals f.EmployeeId into emp
                     from f in emp.DefaultIfEmpty()
                     join e in db.MCs on b.MaterialCenter equals e.MCId into mcs
                     from e in mcs.DefaultIfEmpty()
                     join g in db.HireDetails on new { g1 = b.DeliverynoteId, g2 = "Delivernote" }
                     equals new { g1 = g.Reference, g2 = g.Section } into hir
                     from g in hir.DefaultIfEmpty()
                     join i in db.Accountss on c.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where (quno == "" || b.BillNo == quno) &&
                        (customer == 0 || b.Customer == customer) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(b.DvDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(b.DvDate, tdate) >= 0)
                        && (SType == null || SType == 0 || b.SaleType == St) &&
                        (HireType == 0 || HireType == null || g.HireType == HireType) &&
                        (HdateFrom == null || HdateFrom == "" || EF.Functions.DateDiffDay(g.StartDate, hfdate) <= 0) &&
                        (HdateTo == null || HdateTo == "" || EF.Functions.DateDiffDay(g.EndDate, htdate) >= 0)
                        && (!MCList.Any() || MCArray.Contains(b.MaterialCenter) || ddlMC == b.MaterialCenter)
                        && (project == 0 || project == null || b.Project == project)
                        && (task == 0 || task == null || b.ProTask == task)
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
                         TaxRegNo = i.TRN,

                         Customer = c.CustomerName,
                         EmpName = f.FirstName + " " + f.LastName,
                         User = db.Users.Where(s => s.Id == b.CreatedUserId).Select(s => s.UserName).FirstOrDefault(),
                         b.DvTaxAmount,
                         b.DvCreatedDate,
                         MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All",
                         validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.DvDate, (b.DvValidity == null) ? 0 : b.DvValidity + 1)) ? "Active" : "Expired",
                         SaleType = b.SaleType,
                         FromDate = g.StartDate,
                         ToDate = g.EndDate,
                         HireType = g.HireType
                     }).OrderBy(a => a.DvDate);

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

        #endregion

        #region stock //Not Using
        [HttpGet]
        public ActionResult Stock()
        {
            ViewBag.Category = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            companySet();

            return View();
        }
        [HttpPost]
        public ActionResult GetStock(string cname, long? category)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            var v = (from b in db.Items
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     where b.KeepStock == true &&
                        (b.ItemCode.Contains(cname) || b.ItemName.Contains(cname)) &&
                        (category == 0 || b.ItemCategoryID == category)
                     orderby b.ItemID ascending
                     select new
                     {
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         ItemWithCode = b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         b.OpeningStock,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,

                         PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                         SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


                         PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                         SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                         PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                         SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                         PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                         SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.ItemID,
                         o.ItemCode,
                         o.ItemName,
                         o.ItemWithCode,
                         o.ItemUnitID,
                         o.SubUnitId,
                         PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                         SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                         o.categoryname,
                         OpeningStock = o.OpeningStock,
                         MinStock = (o.MinStock != null) ? o.MinStock : 0,
                         o.ConFactor,

                         PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                         SubPurchase = (o.SubPurchase % o.ConFactor),
                         //purchase = (o.ItemUnitID != o.SubUnitId && o.ItemUnitID != null && o.SubUnitId != null) ?
                         //               (o.SubPurchase % o.ConFactor == 0) ?
                         //                   (o.PriPurchase + (o.SubPurchase / o.ConFactor)) + " " + o.PriUnit :
                         //                   (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)) + " " + o.PriUnit + " " + (o.SubPurchase % o.ConFactor) + " " + o.SubUnit :
                         //               o.PriPurchase + " " + o.PriUnit,

                         PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                         SubSale = (o.SubSale % o.ConFactor),
                         //sale = (o.ItemUnitID != o.SubUnitId && o.ItemUnitID != null && o.SubUnitId != null) ?
                         //               (o.SubSale % o.ConFactor == 0) ?
                         //                   (o.PriSale + (o.SubSale / o.ConFactor)) + " " + o.PriUnit :
                         //                   (o.PriSale + (int)(o.SubSale / o.ConFactor)) + " " + o.PriUnit + " " + (o.SubSale % o.ConFactor) + " " + o.SubUnit :
                         //               o.PriPurchase + " " + o.PriUnit,

                         PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                         SubPReturn = (o.SubPReturn % o.ConFactor),
                         //purchaseReturn = (o.ItemUnitID != o.SubUnitId && o.ItemUnitID != null && o.SubUnitId != null) ?
                         //               (o.SubPReturn % o.ConFactor == 0) ?
                         //                   (o.PriPReturn + (o.SubPReturn / o.ConFactor)) + " " + o.PriUnit :
                         //                   (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)) + " " + o.PriUnit + " " + (o.SubPReturn % o.ConFactor) + " " + o.SubUnit :
                         //               o.PriPurchase + " " + o.PriUnit,

                         PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                         SubSReturn = (o.SubSReturn % o.ConFactor),
                         //salesReturn = (o.ItemUnitID != o.SubUnitId && o.ItemUnitID != null && o.SubUnitId != null) ?
                         //               (o.SubSReturn % o.ConFactor == 0) ?
                         //                   (o.PriSReturn + (o.SubSReturn / o.ConFactor)) + " " + o.PriUnit :
                         //                   (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)) + " " + o.PriUnit + " " + (o.SubSReturn % o.ConFactor) + " " + o.SubUnit :
                         //               o.PriPurchase + " " + o.PriUnit,

                         pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                         subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                         total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                     }).OrderBy(a => a.ItemName);
            //    o.ItemID,
            //    o.ItemCode,
            //    o.ItemName,
            //    o.ItemWithCode,
            //    o.ItemUnitID,
            //    o.SubUnitId,
            //    o.PriUnit,
            //    o.SubUnit,
            //    o.categoryname,
            //    OpeningStock = o.OpeningStock + " " + o.PriUnit,
            //    MinStock = o.MinStock + " " + o.PriUnit,
            //    o.PriPurchase,
            //    o.SubPurchase,
            //    purchase = (o.ItemUnitID != o.SubUnitId && o.ItemUnitID != null && o.SubUnitId != null) ?
            //                            (o.SubPurchase == 0) ?
            //                                o.PriPurchase + " " + o.PriUnit :
            //                                o.PriPurchase + " " + o.PriUnit + ", " + o.SubPurchase + " " + o.SubUnit :
            //                            o.PriPurchase + " " + o.PriUnit,

            //    o.PriSale,
            //    o.SubSale,
            //    sale = (o.ItemUnitID != o.SubUnitId && o.ItemUnitID != null && o.SubUnitId != null) ?
            //                            (o.SubSale == 0) ?
            //                                o.PriSale + " " + o.PriUnit :
            //                                o.PriSale + o.SubSale + " " + o.PriUnit + ", " + o.SubSale + " " + o.SubUnit :
            //                            o.PriPurchase + " " + o.PriUnit,

            //    o.PriPReturn,
            //    o.SubPReturn,
            //    purchaseReturn = (o.ItemUnitID != o.SubUnitId && o.ItemUnitID != null && o.SubUnitId != null) ?
            //                            (o.SubPReturn == 0) ?
            //                                o.PriPReturn + " " + o.PriUnit :
            //                                o.PriPReturn + " " + o.PriUnit + ", " + o.SubPReturn + " " + o.SubUnit :
            //                            o.PriPurchase + " " + o.PriUnit,

            //    o.PriSReturn,
            //    o.SubSReturn,
            //    salesReturn = (o.ItemUnitID != o.SubUnitId && o.ItemUnitID != null && o.SubUnitId != null) ?
            //                            (o.SubSReturn == 0) ?
            //                                o.PriSReturn + " " + o.PriUnit :
            //                                o.PriSReturn + " " + o.PriUnit + ", " + o.SubSReturn + " " + o.SubUnit :
            //                            o.PriPurchase + " " + o.PriUnit,
            //    //total = (o.pritotal != 0)?o.pritotal+ " " + o.PriUnit+((o.subtotal!= 0)?o.subtotal:"") :

            //    total = (o.total % o.ConFactor == 0) ? (o.total / o.ConFactor) + " " + o.PriUnit :
            //                        (int)(o.total / o.ConFactor) + " " + o.PriUnit + ", " + (o.total % o.ConFactor) + " " + o.SubUnit,
            //    min = ((o.total / o.ConFactor) >= o.MinStock) ? "red" : "normal"


            //SORT
            recordsTotal = v.Count();
            var data = v.ToList();

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



        #endregion

        #region payment
        [QkAuthorize(Roles = "Dev,Report Payment")]
        public ActionResult Payment()
        {

            var Paidfrom = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(Paidfrom, "ID", "Name");

            ViewBag.PaidTo = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                           }, "Value", "Text", 1);


            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Report Payment")]
        public ActionResult GetPayment(string vno, long? payfrom, long? payto, string fromdate, string todate, int[] MOPay)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            List<ModeOfPayment> Mop = new List<ModeOfPayment>();
            if (MOPay != null && MOPay.Contains(1))
            {
                Mop.Add(ModeOfPayment.Cash);
            }
            if (MOPay != null && MOPay.Contains(2))
            {
                Mop.Add(ModeOfPayment.PDC);
            }
            if (MOPay != null && MOPay.Contains(3))
            {
                Mop.Add(ModeOfPayment.CDC);
            }
            var count = Mop.Count();
            var v = (from a in db.Payments
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                    (payfrom == 0 || payfrom == null || a.PayFrom == payfrom) &&
                    (payto == 0 || a.PayTo == payto) &&
                    (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                    a.editable == choice.Yes
                    && ((count == 0) || (Mop.Contains(a.MOPayment)))
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.PaymentId,
                         a.Date,
                         a.MOPayment,
                         a.PDCDate,
                         a.PayFrom,
                         a.PayTo,
                         a.TaxAmount,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.CreatedDate,
                         a.Discount,
                         a.Remark
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.PaymentId,
                         o.Date,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         o.MOPayment,
                         o.PDCDate,
                         o.PayFrom,
                         o.PayTo,
                         o.TaxAmount,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.CreatedDate,
                         o.Remark,
                         o.Discount
                     }).OrderBy(a => a.Date);

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


        #endregion

        #region receipt
        [QkAuthorize(Roles = "Dev,Report Receipt")]
        public ActionResult Receipt()
        {

            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            ViewBag.Paidfrom = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            companySet();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Report Receipt")]
        public ActionResult GetReceipt(string vno, long? payfrom, long? payto, string fromdate, string todate, int[] MOPay)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            List<ModeOfPayment> Mop = new List<ModeOfPayment>();
            if (MOPay != null && MOPay.Contains(1))
            {
                Mop.Add(ModeOfPayment.Cash);
            }
            if (MOPay != null && MOPay.Contains(2))
            {
                Mop.Add(ModeOfPayment.PDC);
            }
            if (MOPay != null && MOPay.Contains(3))
            {
                Mop.Add(ModeOfPayment.CDC);
            }
            var count = Mop.Count();
            var v = (from a in db.Receipts
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                      (payfrom == 0 || a.PayFrom == payfrom) &&
                      (payto == 0 || payto == null || a.PayTo == payto) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     a.editable == choice.Yes
                     && ((count == 0) || (Mop.Contains(a.MOPayment)))
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.ReceiptId,
                         a.Date,
                         a.MOPayment,
                         a.PDCDate,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.CreatedDate,
                         a.Remark,
                         a.Discount
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.ReceiptId,
                         o.Date,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         o.MOPayment,
                         o.PDCDate,
                         o.PayFrom,
                         o.PayTo,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.CreatedDate,
                         o.Remark,
                         o.Discount
                     }).OrderBy(a => a.Date);

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
        #endregion

        #region Contra Voucher
        [QkAuthorize(Roles = "Dev,Report ContraVoucher")]
        public ActionResult ContraVoucher()
        {
            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            ViewBag.Paidfrom = QkSelect.List(PaidTo, "ID", "Name");
            companySet();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Report ContraVoucher")]
        public ActionResult GetContraVoucher(string vno, long? payfrom, long? payto, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            var v = (from a in db.ContraVouchers
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                      (payfrom == 0 || a.PayFrom == payfrom) &&
                      (payto == 0 || payto == null || a.PayTo == payto) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     a.editable == choice.Yes
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.ContraVoucherId,
                         a.Date,
                         a.PayFrom,
                         a.PayTo,
                         a.Amount,
                         a.CreatedDate
                     }).OrderBy(a => a.Date);

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
        #endregion

        #region Journal
        [QkAuthorize(Roles = "Dev,Report Journal")]
        public ActionResult Journal()
        {

            var list = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.list = list;
            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Report Journal")]
        public ActionResult GetJournal(string vno, long? payfrom, long? payto, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            var v = (from a in db.Journals
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                    (payfrom == 0 || payfrom == null || a.PayFrom == payfrom) &&
                    (payto == 0 || a.PayTo == payto) &&
                    (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                    a.editable == choice.Yes
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.JournalId,
                         a.Date,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.CreatedDate
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.JournalId,
                         o.Date,
                         o.PayFrom,
                         o.PayTo,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.CreatedDate
                     }).OrderBy(a => a.Date);

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


        #endregion


        public ActionResult customerloginledger(long? id)
        {
            var userid = User.Identity.GetUserId();
            var user = db.Users.Find(userid);
            ViewBag.fromdt = new DateTime(DateTime.Now.Year - 1, 1, 1).ToString("dd-MM-yyyy");
            ViewBag.todt = new DateTime(DateTime.Now.Year + 1, 12, 31).ToString("dd-MM-yyyy");

            var customer = db.Customers.Where(o => o.CustomerName == user.Name && o.AccountNo == user.UserName).Select(o => o.Accounts).FirstOrDefault();
            if (id != null)
            {
                customer = db.Customers.Where(o => o.CustomerID == id).Select(o => o.Accounts).FirstOrDefault();
            }
            ViewBag.accid = customer;
            return View();
        }
        public ActionResult Ledgermin()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();
            return View();
        }
        #region Ledger
        [QkAuthorize(Roles = "Dev,Ledger")]
        public ActionResult Ledger()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Customer Ledger")]
        public ActionResult LedgerCustomer()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();
            return View();
        }

        public ActionResult LedgerCustomer2(long? AccountsId)
        {

            var accname = db.Accountss.Where(a => a.AccountsID == AccountsId).Select(a => new
            {

                id = a.AccountsID,
                name = a.Name
            }).ToList();
            ViewBag.Account = QkSelect.List(accname, "id", "name");



            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Customer Ledger")]
        public ActionResult LedgerCustomertax()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Vendor Ledger")]
        public ActionResult LedgerSupplier()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();
            return View();
        }



        [QkAuthorize(Roles = "Dev,Ledger")]
        public ActionResult LedgerProp()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Ledger,Customer Ledger,Vendor Ledger")]
        public ActionResult GetLedger2(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            Common com = new Common();
            LedgerViewModel vmodel = com.LedgerDatacommend(AccId, fromdate, todate, AccGroup, pdc);
            ViewBag.CustId = db.Customers.Where(z => z.Accounts == AccId).Select(z => z.CustomerID).FirstOrDefault();
            companySet();

            return View(vmodel);
        }
        //[HttpPost]
        // data-table fields listing
        [QkAuthorize(Roles = "Dev,Ledger,Customer Ledger,Vendor Ledger")]
        public ActionResult GetLedger(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            Common com = new Common();
            LedgerViewModel vmodel = com.LedgerDatacommend(AccId, fromdate, todate, AccGroup, pdc);
            ViewBag.CustId = db.Customers.Where(z => z.Accounts == AccId).Select(z => z.CustomerID).FirstOrDefault();
            companySet();

            return View(vmodel);
        }
        public ActionResult GetLedgertax(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            Common com = new Common();
            LedgertaxViewModel vmodel = com.LedgerDatacommendtax(AccId, fromdate, todate, AccGroup, pdc);
            companySet();
            return View(vmodel);
        }
        public ActionResult GetLedgermin(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            Common com = new Common();
            LedgerminiViewModel vmodel = com.LedgerDatacommendmini(AccId, fromdate, todate, AccGroup, pdc);
            companySet();
            return View(vmodel);
        }
        public ActionResult GetLedgerProp(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            Common com = new Common();
            LedgerProViewModel vmodel = com.LedgerDataProp(AccId, fromdate, todate, AccGroup, pdc);
            companySet();
            return View(vmodel);
        }
        public ActionResult getsaleledgers(long Cuss, string From, string To)
        {
            From = new DateTime(DateTime.Now.Year - 1, 1, 1).ToString("dd-MM-yyyy");
            To = new DateTime(DateTime.Now.Year + 1, 12, 31).ToString("dd-MM-yyyy");
            return RedirectToAction("Receivableledgercusdetails", new { Cuss = Cuss, From = From, To = To });

        }
        #endregion

        public ActionResult Receivableledgercusdetails(long Cuss, string From, string To)
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Remove Invoice From Recievable")]
        public ActionResult removeallallocate(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                DeleteRecivableLedgerConfirm(arr);
            }
            Success("Allocation Success");
            return RedirectToAction("Receivableledgerremove", "MyReports");
        }

        [HttpPost, ActionName("DeleteRecivableLedger")]
        [QkAuthorize(Roles = "Dev,Remove Invoice From Recievable")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteRecivableLedgerConfirm(long id)
        {
            bool stat = true;
            string msg;



            var sepay = db.SEPayments.Where(o => o.SalesEntry == id).FirstOrDefault();




            var myentry = new ReceiptBill
            {
                Amount = (sepay.SEBillAmount - sepay.SEPaidAmount),
                InvoiceNo = id,
                NewRefName = id.ToString(),
                Type = "Against Reference",
                Receipt = 0,
                BillType = "Sales",
                Status = Status.active
            };
            db.ReceiptBills.Add(myentry);
            db.SaveChanges();

            db.Entry(sepay).State = EntityState.Modified;
            sepay.SEPaidAmount = sepay.SEBillAmount;
            db.SaveChanges();
            msg = "Successfully Cleared";
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Updated, UserId, "SalesEntry", "SalesEntrys", findip(), id, "Successfully Allocate Invoice");

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [QkAuthorize(Roles = "Dev,Remove Invoice From Recievable")]
        public ActionResult DeleteRecivableLedger(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.salesentry = id;
            return PartialView();
        }
        [QkAuthorize(Roles = "Dev,Remove Invoice From Recievable")]
        public ActionResult Receivableledgerremove()
        {
            ViewBag.CSType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Customer", Value="Cus"},

            }, "Value", "Text");

            var OpAll = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "--Select Customer--", Value = ""},
                         }, "Value", "Text", 1);

            ViewBag.ListSup = OpAll;
            ViewBag.ListCust = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            companySet();

            ViewBag.Agetype = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "0", Value="0"},
                new SelectListItem() {Text = "30", Value="1"},
                new SelectListItem() {Text = "60", Value="2"},
                new SelectListItem() {Text = "90", Value="3"},
                new SelectListItem() {Text = "90+", Value="4"},
            }, "Value", "Text");
            return View();
        }
        public ActionResult Receivableledger()
        {
            ViewBag.CSType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Customer", Value="Cus"},

            }, "Value", "Text");

            var OpAll = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "--Select Customer--", Value = ""},
                         }, "Value", "Text", 1);

            ViewBag.ListSup = OpAll;
            ViewBag.ListCust = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            companySet();

            ViewBag.Agetype = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "0", Value="0"},
                new SelectListItem() {Text = "30", Value="1"},
                new SelectListItem() {Text = "60", Value="2"},
                new SelectListItem() {Text = "90", Value="3"},
                new SelectListItem() {Text = "90+", Value="4"},
            }, "Value", "Text");
            return View();
        }
        #region recievable outstanding

        public ActionResult Receivable()
        {
            ViewBag.CSType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Customer", Value="Cus"},
                new SelectListItem() {Text = "Supplier", Value="Sup"},
            }, "Value", "Text");

            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.ListSup = OpAll;
            ViewBag.ListCust = OpAll;
            companySet();

            ViewBag.Agetype = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "30", Value="1"},
                new SelectListItem() {Text = "60", Value="2"},
                new SelectListItem() {Text = "90", Value="3"},
                new SelectListItem() {Text = "90+", Value="4"},
            }, "Value", "Text");
            return View();
        }

        [HttpPost]
        public ActionResult Receivable(string ddlCSType, long? ddlSupplier, long? ddlCustomer, string From, string To, int? AgeDays, int ddlType, long? ddlEmployee)
        {
            return RedirectToAction("ViewReceivable", new { CSType = ddlCSType, Sups = ddlSupplier, Cuss = ddlCustomer, fromdate = From, todate = To, agedays = AgeDays, TTtype = ddlType, empp = ddlEmployee });
        }

        [HttpPost]

        public ActionResult GetReceivable(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int TTtype, long? empp, long? empp2)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var userpermission = User.IsInRole("All Customers");
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            if (sortColumn == "")
            {
                sortColumn = "Invoice";
            }
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            string result = "";

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime datenow = DateTime.Now;
            if (TTtype == 0)
            {
                if (fromdate != "")
                {
                    fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                }
                if (todate != "")
                {
                    tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                }
            }
            else
            {
                if (agedays != null)
                {
                    agedays = (agedays == 4) ? agedays : (agedays * 30);
                    string agedate = DateTime.Now.AddDays(-Convert.ToDouble(agedays)).ToString("dd-MM-yyyy");
                    tDateAge = DateTime.Parse(agedate, new CultureInfo("en-GB"));
                }
            }

            if (CSType == "Cus")
            {
                var v = (from a in db.SalesEntrys
                         join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                         from b in pay.DefaultIfEmpty()
                         join c in db.Customers on a.Customer equals c.CustomerID into rec
                         from c in rec.DefaultIfEmpty()
                         join d in db.SalesReturns on a.SalesEntryId equals d.SalesEntryId into slret
                         from d in slret.DefaultIfEmpty()
                         where (b.SEPaidAmount != b.SEBillAmount) &&
                           (Cuss == 0 || Cuss == null || a.Customer == Cuss) &&
                           (empp == 0 || empp == null || a.SECashier == empp) &&
                           (empp2 == 0 || empp2 == null || c.SalesPerson == empp2) &&
                           (fdate == null || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                           (tDateAge == null || agedays == 4 || EF.Functions.DateDiffDay(a.SEDate, tDateAge) <= 0)
                           && (userpermission == true || a.CreatedBy == UserId)

                         select new
                         {
                             id = a.SalesEntryId,
                             Date = a.SEDate,
                             Invoice = a.BillNo,
                             Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - ((d.SRGrandTotal == null) ? 0 : d.SRGrandTotal)),
                             total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                             paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),
                             Name = c.CustomerName,
                             LPO = a.PONo,
                             Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                             Ctyp = "Cus",
                             CustomerId = c.CustomerID
                         });
                //search
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    v = v.Where(p => p.Invoice.ToString().ToLower().Contains(search.ToLower()) ||
                                    p.Name.ToString().ToLower().Contains(search.ToLower())

                                     );

                }
                //SORT
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }

                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            else
            {
                var v = (from a in db.PurchaseReturns
                         join b in db.PRPayments on a.PurchaseReturnId equals b.PurchaseReturnId into pay
                         from b in pay.DefaultIfEmpty()
                         join c in db.Suppliers on a.Supplier equals c.SupplierID into rec
                         from c in rec.DefaultIfEmpty()
                         where (b.PReturnAmount != b.PRBillAmount) &&
                           (Sups == 0 || Sups == null || a.Supplier == Sups) &&
                           (fdate == null || EF.Functions.DateDiffDay(a.PRDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.PRDate, tdate) >= 0) &&
                           (tDateAge == null || EF.Functions.DateDiffDay(a.PRDate, tDateAge) >= 0)
                         select new
                         {
                             id = a.PurchaseReturnId,
                             Date = a.PRDate,
                             Invoice = a.BillNo,
                             Amount = (b.PRBillAmount - b.PReturnAmount),
                             total = b.PRBillAmount,
                             paid = b.PReturnAmount,
                             Name = c.SupplierName,
                             Days = EF.Functions.DateDiffDay(a.PRDate, datenow),
                             Ctyp = "Sup"

                         }).OrderBy(b => b.Date);

                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }
        public ActionResult DeleteremovePayClear(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.salesentry = id;
            return PartialView();
        }
        [RedirectingAction]
        [HttpPost, ActionName("DeleteremovePayClear")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteremoveConfirmedClear(long id)
        {
            bool stat = true;
            string msg;



            var sepay = db.PEPayments.Where(o => o.PurchaseEntry == id).FirstOrDefault();

            var myentry = new PaymentBill
            {
                Amount = (sepay.PEBillAmount - sepay.PEPaidAmount),
                InvoiceNo = id,
                NewRefName = id.ToString(),
                Type = "Against Reference",
                Payment = 0,
                BillType = "Purchase",
                Status = Status.active
            };
            db.PaymentBills.Add(myentry);
            db.SaveChanges();
            db.Entry(sepay).State = EntityState.Modified;
            sepay.PEPaidAmount = sepay.PEBillAmount;


            db.SaveChanges();
            msg = "Successfully Cleared";
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Updated, UserId, "PurchaseEntry", "PurchaseEntrys", findip(), id, "Successfully allocated Purchase Entry payment");

            msg = "Successfully Cleared";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }



        public ActionResult DeletePayClear(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.salesentry = id;
            return PartialView();
        }
        [RedirectingAction]
        [HttpPost, ActionName("DeletePayClear")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedClear(long id)
        {
            bool stat = true;
            string msg;
            db.PaymentBills.RemoveRange(db.PaymentBills.Where(o => o.InvoiceNo == id));
            db.PETransactions.RemoveRange(db.PETransactions.Where(o => o.PurchaseEntry == id));
            db.SaveChanges();
            var sepay = db.PEPayments.Where(o => o.PurchaseEntry == id).FirstOrDefault();
            db.Entry(sepay).State = EntityState.Modified;
            sepay.PEPaidAmount = 0;


            db.SaveChanges();

            msg = "Successfully Cleared";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }










        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.salesentry = id;
            return PartialView();
        }
        [RedirectingAction]
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = true;
            string msg;
            db.ReceiptBills.RemoveRange(db.ReceiptBills.Where(o => o.InvoiceNo == id));
            db.SETransactions.RemoveRange(db.SETransactions.Where(o => o.SalesEntry == id));
            db.SaveChanges();
            var sepay = db.SEPayments.Where(o => o.SalesEntry == id).FirstOrDefault();
            db.Entry(sepay).State = EntityState.Modified;
            sepay.SEPaidAmount = 0;


            db.SaveChanges();

            msg = "Successfully Cleared";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]

        public ActionResult GetReceivableLedger(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int? AgeDaystwo, int TTtype, long? empp)
        {

            var UserId = User.Identity.GetUserId();

            int recordsTotal = 0;

            var userpermission = User.IsInRole("All Customers");

            string result = "";

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime? tDateAgetwo = null;
            DateTime datenow = DateTime.Now;
            if (TTtype == 0)
            {
                if (fromdate != "")
                {
                    fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                }
                if (todate != "")
                {
                    tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                }
            }
            else
            {
                if (agedays != null)
                {
                    agedays = (agedays * 30);
                    string agedate = DateTime.Now.AddDays(-Convert.ToDouble(agedays)).ToString("dd-MM-yyyy");
                    tDateAge = DateTime.Parse(agedate, new CultureInfo("en-GB"));
                }
                if (AgeDaystwo != null)
                {
                    AgeDaystwo = (agedays == 4) ? (300 * 30) : (AgeDaystwo * 30);
                    string agedate = DateTime.Now.AddDays(-Convert.ToDouble(AgeDaystwo)).ToString("dd-MM-yyyy");
                    tDateAgetwo = DateTime.Parse(agedate, new CultureInfo("en-GB"));
                }
            }

            if (CSType == "Cus")
            {
                var v = (from a in db.SalesEntrys
                         join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                         from b in pay.DefaultIfEmpty()
                         join c in db.Customers on a.Customer equals c.CustomerID into rec
                         from c in rec.DefaultIfEmpty()

                         let salesreturn = (decimal?)(from d in db.SalesReturns
                                                      where d.SalesEntryId == a.SalesEntryId
                                                      select new
                                                      {
                                                          d.SRGrandTotal
                                                      }).Sum(o => o.SRGrandTotal) ?? 0


                         let payment = (decimal?)(from e in db.PaymentBills
                                                  join f in db.Payments on e.Payment equals f.PaymentId into reciept2

                                                  join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                  join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                  where h.SalesEntryId == a.SalesEntryId &&
                                                  e.BillType == "Sales Return"
                                                  select new
                                                  {
                                                      amt = e.Amount
                                                  }).Sum(o => o.amt) ?? 0
                         let paymentjrnl = (decimal?)(from e in db.JornalPaymentBills
                                                      join f in db.Journals on e.Jornal equals f.JournalId into reciept2

                                                      join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                      join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                      where h.SalesEntryId == a.SalesEntryId &&
                                                      e.BillType == "Sales Return"
                                                      select new
                                                      {
                                                          amt = e.Amount
                                                      }).Sum(o => o.amt) ?? 0

                         let reciept = (from e in db.ReceiptBills
                                        join f in db.Receipts on e.Receipt equals f.ReceiptId into reciept2
                                        from f in reciept2.DefaultIfEmpty()
                                        where e.InvoiceNo == a.SalesEntryId
                                        select new
                                        {
                                            f.VoucherNo,
                                            f.ReceiptId
                                        }).FirstOrDefault()
                         where
                           (Cuss == 0 || Cuss == null || a.Customer == Cuss) &&
                           (empp == 0 || empp == null || a.SECashier == empp) &&
                           (fdate == null || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                           (tDateAge == null || (EF.Functions.DateDiffDay(a.SEDate, tDateAgetwo) <= 0 && EF.Functions.DateDiffDay(a.SEDate, tDateAge) >= 0 && (b.SEBillAmount - b.SEPaidAmount) > 3))


                         select new
                         {
                             id = a.SalesEntryId,
                             Date = a.SEDate,
                             Invoice = a.BillNo,
                             recieptid = (reciept.ReceiptId == null) ? 0 : reciept.ReceiptId,
                             recieptno = (reciept.VoucherNo == null) ? "" : reciept.VoucherNo,
                             Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - (((salesreturn == null) ? 0 : salesreturn) - ((payment == null) ? 0 : payment) - ((paymentjrnl == null) ? 0 : paymentjrnl))),
                             total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                             paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                             salesreturns = (salesreturn == null) ? 0 : salesreturn,
                             payments = (payment == null) ? 0 : payment,
                             Name = (c.CustomerName == null) ? "Opening Invoice" : c.CustomerName,
                             LPO = a.PONo,
                             Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                             Ctyp = "Cus",
                             CustomerId = (a.Customer == 0) ? 0 : c.CustomerID
                         });
                //search
                var data = v.ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }
        [HttpPost]

        public ActionResult GetReceivableLedgerremove(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int? AgeDaystwo, int TTtype, long? empp)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var userpermission = User.IsInRole("All Customers");
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            if (sortColumn == "")
            {
                sortColumn = "Invoice";
            }
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            string result = "";

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime? tDateAgetwo = null;
            DateTime datenow = DateTime.Now;
            if (TTtype == 0)
            {
                if (fromdate != "")
                {
                    fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                }
                if (todate != "")
                {
                    tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                }
            }
            else
            {
                if (agedays != null)
                {
                    agedays = (agedays * 30);
                    string agedate = DateTime.Now.AddDays(-Convert.ToDouble(agedays)).ToString("dd-MM-yyyy");
                    tDateAge = DateTime.Parse(agedate, new CultureInfo("en-GB"));
                }
                if (AgeDaystwo != null)
                {
                    AgeDaystwo = (agedays == 4) ? (300 * 30) : (AgeDaystwo * 30);
                    string agedate = DateTime.Now.AddDays(-Convert.ToDouble(AgeDaystwo)).ToString("dd-MM-yyyy");
                    tDateAgetwo = DateTime.Parse(agedate, new CultureInfo("en-GB"));
                }
            }

            if (CSType == "Cus")
            {
                var v = (from a in db.SalesEntrys
                         join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                         from b in pay.DefaultIfEmpty()
                         join c in db.Customers on a.Customer equals c.CustomerID into rec
                         from c in rec.DefaultIfEmpty()

                         let salesreturn = (decimal?)(from d in db.SalesReturns
                                                      where d.SalesEntryId == a.SalesEntryId
                                                      select new
                                                      {
                                                          d.SRGrandTotal
                                                      }).Sum(o => o.SRGrandTotal) ?? 0


                         let payment = (decimal?)(from e in db.PaymentBills
                                                  join f in db.Payments on e.Payment equals f.PaymentId into reciept2

                                                  join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                  join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                  where h.SalesEntryId == a.SalesEntryId &&
                                                  e.BillType == "Sales Return"
                                                  select new
                                                  {
                                                      amt = e.Amount
                                                  }).Sum(o => o.amt) ?? 0
                         let paymentjrnl = (decimal?)(from e in db.JornalPaymentBills
                                                      join f in db.Journals on e.Jornal equals f.JournalId into reciept2

                                                      join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                      join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                      where h.SalesEntryId == a.SalesEntryId &&
                                                      e.BillType == "Sales Return"
                                                      select new
                                                      {
                                                          amt = e.Amount
                                                      }).Sum(o => o.amt) ?? 0

                         let reciept = (from e in db.ReceiptBills
                                        join f in db.Receipts on e.Receipt equals f.ReceiptId into reciept2
                                        from f in reciept2.DefaultIfEmpty()
                                        where e.InvoiceNo == a.SalesEntryId
                                        select new
                                        {
                                            f.VoucherNo,
                                            f.ReceiptId
                                        }).FirstOrDefault()
                         where
                           (Cuss == 0 || Cuss == null || a.Customer == Cuss) &&
                           (empp == 0 || empp == null || a.SECashier == empp) &&
                           (fdate == null || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                           (tDateAge == null || (EF.Functions.DateDiffDay(a.SEDate, tDateAgetwo) <= 0 && EF.Functions.DateDiffDay(a.SEDate, tDateAge) >= 0))


                         select new
                         {
                             id = a.SalesEntryId,
                             Date = a.SEDate,
                             Invoice = a.BillNo,
                             recieptid = (reciept.ReceiptId == null) ? 0 : reciept.ReceiptId,
                             recieptno = (reciept.VoucherNo == null) ? "" : reciept.VoucherNo,
                             Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - (((salesreturn == null) ? 0 : salesreturn) - ((payment == null) ? 0 : payment) - ((paymentjrnl == null) ? 0 : paymentjrnl))),
                             total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                             paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                             salesreturns = (salesreturn == null) ? 0 : salesreturn,
                             payments = (payment == null) ? 0 : payment,
                             Name = (c.CustomerName == null) ? "Opening Invoice" : c.CustomerName,
                             LPO = a.PONo,
                             Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                             Ctyp = "Cus",
                             CustomerId = (a.Customer == 0) ? 0 : c.CustomerID
                         });
                //search
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    v = v.Where(p => p.Invoice.ToString().ToLower().Contains(search.ToLower()) ||
                                    p.Name.ToString().ToLower().Contains(search.ToLower())

                                     );

                }
                //SORT
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }

                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        [HttpGet]
        public ActionResult ViewReceivable(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int TTtype, long? empp)
        {
            ViewBag.SupCus = CSType;

            ViewBag.SupId = Sups;
            ViewBag.CusId = Cuss;

            ViewBag.From = fromdate;
            ViewBag.To = todate;

            string CSName = "";
            string LPO = "";
            if (CSType == "Sup")
            {
                CSName = "Supplier : " + db.Suppliers.Where(x => x.SupplierID == Sups).Select(x => x.SupplierCode + " - " + x.SupplierName).FirstOrDefault();
            }
            else
            {
                CSName = "Customer : " + db.Customers.Where(x => x.CustomerID == Cuss).Select(x => x.CustomerCode + " - " + x.CustomerName).FirstOrDefault();
                LPO = db.SalesEntrys.Where(m => m.Customer == Cuss).Select(m => m.PONo).FirstOrDefault();
                ViewBag.LPNo = LPO != "" ? "LPO : " + LPO : "";
            }


            var opp = (from d in db.AccountsTransactions
                       join a in db.Customers on d.Account equals a.Accounts
                       where (Cuss == null || Cuss == 0 || a.CustomerID == Cuss) &&
                       d.Status == null
                       select d.Credit).AsEnumerable().DefaultIfEmpty(0).Sum();
            ViewBag.op = opp;
            ViewBag.ComName = CSName;
            companySet();
            return View();
        }
        #endregion

        #region Payable outstanding

        [QkAuthorize(Roles = "Dev,Payable Outstanding")]
        public ActionResult Payable()
        {

            ViewBag.CSType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Supplier", Value="Sup"},
                new SelectListItem() {Text = "Customer", Value="Cus"},
            }, "Value", "Text");


            var OpAll = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = true, Text = "All", Value = "0"},
                         }, "Value", "Text", 1);

            ViewBag.ListSup = OpAll;
            ViewBag.ListCust = OpAll;
            ViewBag.Agetype = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "30", Value="1"},
                new SelectListItem() {Text = "60", Value="2"},
                new SelectListItem() {Text = "90", Value="3"},
                new SelectListItem() {Text = "90+", Value="4"},
            }, "Value", "Text");
            return View();
        }
        [QkAuthorize(Roles = "Dev,Remove Invoice From Payment")]
        public ActionResult removePayableclear()
        {

            ViewBag.CSType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Supplier", Value="Sup"},
                new SelectListItem() {Text = "Customer", Value="Cus"},
            }, "Value", "Text");


            var OpAll = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "--Select Supplier--", Value = ""},
                         }, "Value", "Text", 1);

            ViewBag.ListSup = OpAll;
            ViewBag.ListCust = OpAll;
            ViewBag.Agetype = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "30", Value="1"},
                new SelectListItem() {Text = "60", Value="2"},
                new SelectListItem() {Text = "90", Value="3"},
                new SelectListItem() {Text = "90+", Value="4"},
            }, "Value", "Text");
            return View();
        }
        [QkAuthorize(Roles = "Dev,Payable Outstanding")]
        public ActionResult Payableclear()
        {

            ViewBag.CSType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Supplier", Value="Sup"},
                new SelectListItem() {Text = "Customer", Value="Cus"},
            }, "Value", "Text");


            var OpAll = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "--Select Supplier--", Value = ""},
                         }, "Value", "Text", 1);

            ViewBag.ListSup = OpAll;
            ViewBag.ListCust = OpAll;
            ViewBag.Agetype = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "30", Value="1"},
                new SelectListItem() {Text = "60", Value="2"},
                new SelectListItem() {Text = "90", Value="3"},
                new SelectListItem() {Text = "90+", Value="4"},
            }, "Value", "Text");
            return View();
        }

        [HttpPost]
        public ActionResult Payable(string ddlCSType, long? ddlSupplier, long? ddlCustomer, string From, string To, int? AgeDays, int ddlType)
        {
            return RedirectToAction("ViewPayable", new { CSType = ddlCSType, Sups = ddlSupplier, Cuss = ddlCustomer, fromdate = From, todate = To, agedays = AgeDays, TTtype = ddlType });
        }
        [HttpPost]
        public ActionResult Payableclear(string ddlCSType, long? ddlSupplier, long? ddlCustomer, string From, string To, int? AgeDays, int ddlType)
        {
            return RedirectToAction("ViewPayableclear", new { CSType = ddlCSType, Sups = ddlSupplier, Cuss = 0, fromdate = From, todate = To, agedays = AgeDays, TTtype = ddlType });
        }
        [HttpPost]
        public ActionResult removePayableclear(string ddlCSType, long? ddlSupplier, long? ddlCustomer, string From, string To, int? AgeDays, int ddlType)
        {
            return RedirectToAction("ViewremovePayableclear", new { CSType = ddlCSType, Sups = ddlSupplier, Cuss = 0, fromdate = From, todate = To, agedays = AgeDays, TTtype = ddlType });
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Payable Outstanding")]
        public ActionResult GetPayable(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int TTtype)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            if (sortColumn == "")
            {
                sortColumn = "Invoice";
            }
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string result = "";

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime datenow = DateTime.Now;
            if (TTtype == 0)
            {
                if (fromdate != "")
                {
                    fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                }
                if (todate != "")
                {
                    tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                }
            }
            else
            {
                if (agedays != null)
                {
                    agedays = (agedays == 4) ? agedays : (agedays * 30);
                    string agedate = DateTime.Now.AddDays(-Convert.ToDouble(agedays)).ToString("dd-MM-yyyy");
                    tDateAge = DateTime.Parse(agedate, new CultureInfo("en-GB"));
                }
            }
            if (CSType == "Sup")
            {
                var v = (from a in db.PurchaseEntrys
                         join b in db.PEPayments on a.PurchaseEntryId equals b.PurchaseEntry into pay
                         from b in pay.DefaultIfEmpty()
                         join c in db.Suppliers on a.Supplier equals c.SupplierID into rec
                         from c in rec.DefaultIfEmpty()
                         join d in db.PurchaseReturns on a.PurchaseEntryId equals d.purchaseEntryId into purre
                         from d in purre.DefaultIfEmpty()


                         where (b.PEPaidAmount != b.PEBillAmount) &&
                          (Sups == 0 || Sups == null || a.Supplier == Sups) &&
                           (fdate == null || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                            (tDateAge == null || agedays == 4 || EF.Functions.DateDiffDay(a.PEDate, tDateAge) <= 0)
                         select new
                         {
                             id = a.PurchaseEntryId,
                             Date = a.PEDate,
                             Invoice = a.BillNo,
                             Amount = (b.PEBillAmount - b.PEPaidAmount) - (d.PRGrandTotal == null ? 0 : d.PRGrandTotal),
                             total = b.PEBillAmount,
                             paid = b.PEPaidAmount,
                             Name = c.SupplierName,
                             a.Supplier,
                             Days = EF.Functions.DateDiffDay(a.PEDate, datenow),
                             Ctyp = "Sup"

                         }).ToList().Select(o => new
                         {
                             o.id,
                             o.Date,
                             o.Invoice,
                             o.Amount,
                             o.total,
                             o.paid,
                             o.Name,
                             o.Supplier,
                             o.Days,
                             o.Ctyp
                         });

                //search
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    v = v.Where(p => p.Invoice.ToString().ToLower().Contains(search.ToLower()) ||
                                    p.Name.ToString().ToLower().Contains(search.ToLower())

                                     );

                }

                //SORT
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            else
            {
                var v = (from a in db.SalesReturns
                         join b in db.SRPayments on a.SalesReturnId equals b.SalesReturnId into pay
                         from b in pay.DefaultIfEmpty()
                         join c in db.Customers on a.Customer equals c.CustomerID into rec
                         from c in rec.DefaultIfEmpty()
                         where (b.SReturnAmount != b.SRBillAmount) &&
                          (Cuss == 0 || Cuss == null || a.Customer == Cuss) &&
                           (fdate == null || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0) &&
                           (tDateAge == null || EF.Functions.DateDiffDay(a.SRDate, tDateAge) >= 0)
                         select new
                         {
                             id = a.SalesReturnId,
                             Date = a.SRDate,
                             Invoice = a.BillNo,
                             Amount = (b.SRBillAmount - b.SReturnAmount),
                             total = b.SRBillAmount,
                             paid = b.SReturnAmount,
                             Name = c.CustomerName,
                             a.Customer,
                             Days = EF.Functions.DateDiffDay(a.SRDate, datenow),
                             Ctyp = "Cus"
                         }).OrderBy(b => b.Date);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpGet]
        public ActionResult ViewPayable(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int TTtype)
        {
            ViewBag.SupCus = CSType;

            ViewBag.SupId = Sups;
            ViewBag.CusId = Cuss;

            ViewBag.From = fromdate;
            ViewBag.To = todate;

            string CSName = "";
            if (CSType == "Sup")
            {
                CSName = "Supplier : " + db.Suppliers.Where(x => x.SupplierID == Sups).Select(x => x.SupplierCode + " - " + x.SupplierName).FirstOrDefault();
            }
            else
            {
                CSName = "Customer : " + db.Customers.Where(x => x.CustomerID == Cuss).Select(x => x.CustomerCode + " - " + x.CustomerName).FirstOrDefault();
            }
            ViewBag.ComName = CSName;

            companySet();
            return View();
        }




        [HttpPost]
        [QkAuthorize(Roles = "Dev,Payable Outstanding")]
        public ActionResult GetPayableclear(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int TTtype)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            if (sortColumn == "")
            {
                sortColumn = "Invoice";
            }
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            string result = "";

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime datenow = DateTime.Now;
            if (TTtype == 0)
            {
                if (fromdate != "")
                {
                    fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                }
                if (todate != "")
                {
                    tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                }
            }
            else
            {
                if (agedays != null)
                {
                    agedays = (agedays == 4) ? agedays : (agedays * 30);
                    string agedate = DateTime.Now.AddDays(-Convert.ToDouble(agedays)).ToString("dd-MM-yyyy");
                    tDateAge = DateTime.Parse(agedate, new CultureInfo("en-GB"));
                }
            }
            if (CSType == "Sup")
            {
                var v = (from a in db.PurchaseEntrys
                         join b in db.PEPayments on a.PurchaseEntryId equals b.PurchaseEntry into pay
                         from b in pay.DefaultIfEmpty()
                         join c in db.Suppliers on a.Supplier equals c.SupplierID into rec
                         from c in rec.DefaultIfEmpty()
                         join d in db.PurchaseReturns on a.PurchaseEntryId equals d.purchaseEntryId into purre
                         from d in purre.DefaultIfEmpty()


                         where
                          (Sups == 0 || Sups == null || a.Supplier == Sups) &&
                           (fdate == null || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                            (tDateAge == null || agedays == 4 || EF.Functions.DateDiffDay(a.PEDate, tDateAge) <= 0)
                         select new
                         {
                             id = a.PurchaseEntryId,
                             Date = a.PEDate,
                             Invoice = a.BillNo,
                             Amount = (b.PEBillAmount - b.PEPaidAmount) - (d.PRGrandTotal == null ? 0 : d.PRGrandTotal),
                             total = b.PEBillAmount,
                             paid = b.PEPaidAmount,
                             Name = c.SupplierName,
                             a.Supplier,
                             Days = EF.Functions.DateDiffDay(a.PEDate, datenow),
                             Ctyp = "Sup"

                         }).ToList().Select(o => new
                         {
                             o.id,
                             o.Date,
                             o.Invoice,
                             o.Amount,
                             o.total,
                             o.paid,
                             o.Name,
                             o.Supplier,
                             o.Days,
                             o.Ctyp
                         });

                //search
                if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    v = v.Where(p => p.Invoice.ToString().ToLower().Contains(search.ToLower()) ||
                                    p.Name.ToString().ToLower().Contains(search.ToLower())

                                     );

                }

                //SORT
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            else
            {
                var v = (from a in db.SalesReturns
                         join b in db.SRPayments on a.SalesReturnId equals b.SalesReturnId into pay
                         from b in pay.DefaultIfEmpty()
                         join c in db.Customers on a.Customer equals c.CustomerID into rec
                         from c in rec.DefaultIfEmpty()
                         where (b.SReturnAmount != b.SRBillAmount) &&
                          (Cuss == 0 || Cuss == null || a.Customer == Cuss) &&
                           (fdate == null || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0) &&
                           (tDateAge == null || EF.Functions.DateDiffDay(a.SRDate, tDateAge) >= 0)
                         select new
                         {
                             id = a.SalesReturnId,
                             Date = a.SRDate,
                             Invoice = a.BillNo,
                             Amount = (b.SRBillAmount - b.SReturnAmount),
                             total = b.SRBillAmount,
                             paid = b.SReturnAmount,
                             Name = c.CustomerName,
                             a.Customer,
                             Days = EF.Functions.DateDiffDay(a.SRDate, datenow),
                             Ctyp = "Cus"
                         }).OrderBy(b => b.Date);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpGet]
        public ActionResult ViewPayableclear(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int TTtype)
        {
            ViewBag.SupCus = CSType;

            ViewBag.SupId = Sups;
            ViewBag.CusId = Cuss;

            ViewBag.From = fromdate;
            ViewBag.To = todate;

            string CSName = "";
            if (CSType == "Sup")
            {
                CSName = "Supplier : " + db.Suppliers.Where(x => x.SupplierID == Sups).Select(x => x.SupplierCode + " - " + x.SupplierName).FirstOrDefault();
            }
            else
            {
                CSName = "Customer : " + db.Customers.Where(x => x.CustomerID == Cuss).Select(x => x.CustomerCode + " - " + x.CustomerName).FirstOrDefault();
            }
            ViewBag.ComName = CSName;

            companySet();
            return View();
        }


        [HttpGet]
        public ActionResult ViewremovePayableclear(string CSType, long? Sups, long? Cuss, string fromdate, string todate, int? agedays, int TTtype)
        {
            ViewBag.SupCus = CSType;

            ViewBag.SupId = Sups;
            ViewBag.CusId = Cuss;

            ViewBag.From = fromdate;
            ViewBag.To = todate;

            string CSName = "";
            if (CSType == "Sup")
            {
                CSName = "Supplier : " + db.Suppliers.Where(x => x.SupplierID == Sups).Select(x => x.SupplierCode + " - " + x.SupplierName).FirstOrDefault();
            }
            else
            {
                CSName = "Customer : " + db.Customers.Where(x => x.CustomerID == Cuss).Select(x => x.CustomerCode + " - " + x.CustomerName).FirstOrDefault();
            }
            ViewBag.ComName = CSName;

            companySet();
            return View();
        }







        #endregion

        #region purchase order
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report PurchaseOrder")]
        public ActionResult PurchaseOrder()
        {
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);


            ViewBag.Employee = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            companySet();
            return View();
        }
        [HttpPost]
        public ActionResult GetPurchaseOrder(string vno, long? supplier, long? employee, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            var v = (from a in db.PurchaseOrders
                     join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Employees on a.POCashier equals c.EmployeeId into emp
                     from c in emp.DefaultIfEmpty()
                     join e in db.Branchs on a.Branch equals e.BranchID
                     join f in db.Accountss on b.Accounts equals f.AccountsID into acc
                     from f in acc.DefaultIfEmpty()
                     where (vno == "" || a.BillNo == vno) &&
                        (supplier == 0 || a.Supplier == supplier) &&
                        (employee == 0 || a.POCashier == employee) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.PODate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.PODate, tdate) >= 0)
                     select new
                     {
                         a.PurchaseOrderId,
                         a.PONo,
                         a.BillNo,
                         a.PODate,
                         a.POItems,
                         a.POItemQuantity,
                         a.PODiscount,
                         a.POGrandTotal,
                         a.POTax,
                         TaxRegNo = f.TRN,
                         Supplier = b.SupplierName,
                         EmpName = c.FirstName + " " + c.LastName,
                         User = db.Users.Where(s => s.Id == a.CreatedUserId).Select(s => s.UserName).FirstOrDefault(),
                         a.POTaxAmount,
                         a.POValidity,
                         validity = (DateTime.Now <= DbFunctionsCompat.AddDays(a.PODate, (a.POValidity == null) ? 0 : a.POValidity + 1)) ? "Active" : "Expired",
                         a.POCreatedDate
                     }).OrderBy(a => a.PODate).ThenBy(a => a.POCreatedDate);

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
        #endregion

        #region sales order
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report SalesOrder")]
        public ActionResult SalesOrder()
        {
            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);


            ViewBag.Employee = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            companySet();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");

            ViewBag.getProj = QkSelect.List(
                            new List<SelectListItem>
                            {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            return View();
        }
        [HttpPost]
        public ActionResult GetSalesOrder(string vno, long? customer, long? employee, string fromdate, string todate, long? SType, long? HireType, string HdateFrom, string HdateTo, long? project, long? task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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
            SaleType St = new SaleType();
            DateTime? hfdate = null;
            DateTime? htdate = null;
            if (SType == 2)
            {

                if (SType != null)
                {
                    St = (SType == 1) ? SaleType.Sale : SaleType.Hire;
                };

                if (HdateFrom != "")
                {
                    hfdate = DateTime.Parse(HdateFrom, new CultureInfo("en-GB"));
                }
                if (HdateTo != "")
                {
                    htdate = DateTime.Parse(HdateTo, new CultureInfo("en-GB"));
                }
            }
            else
            {
                St = SaleType.Sale;
                hfdate = null;
                htdate = null;
            }
            var v = (from a in db.SalesOrders
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Employees on a.SOCashier equals c.EmployeeId into emp
                     from c in emp.DefaultIfEmpty()
                     join e in db.Branchs on a.Branch equals e.BranchID
                     join g in db.HireDetails on new { g1 = a.SalesOrderId, g2 = "Sales order" }
                     equals new { g1 = g.Reference, g2 = g.Section } into hir
                     from g in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where (vno == "" || a.BillNo == vno) &&
                        (customer == 0 || a.Customer == customer) &&
                        (employee == 0 || a.SOCashier == employee) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.SODate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.SODate, tdate) >= 0)
                        && (SType == null || a.SaleType == St) &&
                        (HireType == 0 || HireType == null || g.HireType == HireType) &&
                        (HdateFrom == "" || HdateFrom == null || EF.Functions.DateDiffDay(g.StartDate, hfdate) <= 0) &&
                        (HdateTo == "" || HdateTo == null || EF.Functions.DateDiffDay(g.EndDate, htdate) >= 0)
                        && (project == 0 || project == null || a.Project == project)
                        && (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         a.SalesOrderId,
                         a.SONo,
                         a.BillNo,
                         a.SODate,
                         a.SOItems,
                         a.SOItemQuantity,
                         a.SODiscount,
                         a.SOGrandTotal,
                         a.SOTax,
                         TaxRegNo = i.TRN,
                         Customer = b.CustomerName,
                         EmpName = c.FirstName + " " + c.LastName,
                         User = db.Users.Where(s => s.Id == a.CreatedUserId).Select(s => s.UserName).FirstOrDefault(),
                         a.SOTaxAmount,
                         a.SOValidity,
                         validity = (DateTime.Now <= DbFunctionsCompat.AddDays(a.SODate, (a.SOValidity == null) ? 0 : a.SOValidity + 1)) ? "Active" : "Expired",
                         a.SOCreatedDate,
                         SaleType = a.SaleType,
                         FromDate = g.StartDate,
                         ToDate = g.EndDate,
                         HireType = g.HireType
                     }).OrderBy(a => a.SODate).ThenBy(a => a.SOCreatedDate);

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
        #endregion

        #region credit note
        //[QkAuthorize(Roles = "Dev,Report CreditNote")]
        public ActionResult CreditNote()
        {

            ViewBag.PaidTo = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.Paidfrom = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            companySet();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,Report CreditNote")]
        public ActionResult GetCreditNote(string vno, long? payfrom, long? payto, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            var v = (from a in db.CreditNotes
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.BillNo == vno) &&
                      (payfrom == 0 || a.PayFrom == payfrom) &&
                      (payto == 0 || payto == null || a.PayTo == payto) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.CNDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.CNDate, tdate) >= 0)
                     select new
                     {
                         VoucherNo = a.BillNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.CreditnoteId,
                         a.CNDate,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.CreatedDate
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.CreditnoteId,
                         o.CNDate,
                         o.PayFrom,
                         o.PayTo,
                         o.SubTotal,
                         o.GrandTotal,
                         o.CreatedDate
                     }).OrderBy(a => a.CNDate);

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
        #endregion

        #region debit note
        //[QkAuthorize(Roles = "Dev,Report DebitNote")]
        public ActionResult DebitNote()
        {
            ViewBag.PaidTo = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.Paidfrom = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            companySet();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,Report CreditNote")]
        public ActionResult GetDebitNote(string vno, long? payfrom, long? payto, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            var v = (from a in db.DrNotes
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.BillNo == vno) &&
                      (payfrom == 0 || a.PayFrom == payfrom) &&
                      (payto == 0 || payto == null || a.PayTo == payto) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.DNDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.DNDate, tdate) >= 0)
                     select new
                     {
                         VoucherNo = a.BillNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.DrNoteId,
                         a.DNDate,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.CreatedDate
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.DrNoteId,
                         o.DNDate,
                         o.PayFrom,
                         o.PayTo,
                         o.SubTotal,
                         o.GrandTotal,
                         o.CreatedDate
                     }).OrderBy(a => a.DNDate);

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
        #endregion

        #region Stock Adjustment Report
        [QkAuthorize(Roles = "Dev,Report StockAdjustment")]
        public ActionResult StockAdjustmentReport()
        {
            ViewBag.Item = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);
            //    Name = r.VoucherNo,
            //    ID = r.VoucherNo
            ViewBag.Voucher = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);
            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Report StockAdjustmentItemWise")]
        public ActionResult StockAdjustmentItemWiseReport()
        {
            ViewBag.Item = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                }, "Value", "Text", 0);
            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Report StockAdjustment")]
        public ActionResult GetStockAdjustmentReport(string Voucher, long? Item, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key


            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime datenow = DateTime.Now;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            // in case of customer or suplier account
            var v = (from a in db.StockAdjustments
                     join b in db.Items on a.ItemID equals b.ItemID into itm
                     from b in itm.DefaultIfEmpty()
                     join c in db.ItemUnits on a.ItemUnitID equals c.ItemUnitID into unit
                     from c in unit.DefaultIfEmpty()
                     where ((fromdate == null || EF.Functions.DateDiffDay(a.AdjDate, fdate) <= 0) &&
                              (todate == null || EF.Functions.DateDiffDay(a.AdjDate, tdate) >= 0) &&
                              (Item == 0 || a.ItemID == Item) && (Voucher == "" || Voucher == "0" || a.VoucherNo == Voucher))
                     select new
                     {
                         id = a.StockAdjustmentId,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Quantity = a.ItemQuantity,
                         Unit = c.ItemUnitName,
                         Date = a.AdjDate,
                         Type = a.AdjustmentType,
                         PurchaseRate = a.PurchaseRate,
                         a.Reason,
                         Voucher = a.VoucherNo,
                     }).OrderBy(a => a.id);


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

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Report StockAdjustmentItemWise")]
        public ActionResult GetStockAdjItemWiseReport(long? Item, string fromdate, string todate)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
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
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var v = (from a in db.StockAdjustments
                     join b in db.Items on a.ItemID equals b.ItemID into itm
                     from b in itm.DefaultIfEmpty()
                     join c in db.ItemUnits on a.ItemUnitID equals c.ItemUnitID into unit
                     from c in unit.DefaultIfEmpty()
                     where ((fdate == null || EF.Functions.DateDiffDay(a.AdjDate, fdate) <= 0) &&
                              (tdate == null || EF.Functions.DateDiffDay(a.AdjDate, tdate) >= 0) &&
                              (Item == 0 || a.ItemID == Item))
                     select new
                     {
                         id = a.StockAdjustmentId,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Quantity = a.ItemQuantity,
                         Unit = c.ItemUnitName,
                         Date = a.AdjDate,
                         Type = a.AdjustmentType,
                         PurchaseRate = a.PurchaseRate,
                         a.Reason,
                         Voucher = a.VoucherNo,
                     });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.Voucher.ToString().ToLower().Equals(search.ToLower()));
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

        #endregion

        #region Material Center Report
        public ActionResult MaterialFromPartyReport()
        {
            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            ViewBag.Voucher = OpAll;
            var acc = db.Accountss.Select(s => new
            {
                AccId = s.AccountsID,
                Name = s.Name
            }).ToList();
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.Party = QkSelect.List(acc, "AccId", "Name");
            companySet();
            return View();
        }

        public ActionResult MaterialToPartyReport()
        {
            var OpAll = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                         }, "Value", "Text", 1);

            ViewBag.Voucher = OpAll;
            var acc = db.Accountss.Select(s => new
            {
                AccId = s.AccountsID,
                Name = s.Name
            }).ToList();
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MC = OpAll;
            ViewBag.Party = QkSelect.List(acc, "AccId", "Name");
            companySet();
            return View();
        }

        [HttpPost]
        public ActionResult GetMaterialFromParty(string Voucher/*, MtFromType Type*/, long? Party, string fromdate, string todate, long? MC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (MC == 0 || MC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key


            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime datenow = DateTime.Now;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            // in case of customer or suplier account
            var v = (from a in db.MtFromPartys
                     join b in db.MCs on a.MC equals b.MCId
                     join c in db.Accountss on a.Party equals c.AccountsID into primary
                     from c in primary.DefaultIfEmpty()
                     where (Voucher == null || Voucher == "" || Voucher == a.VoucherNo) &&
                     (Party == null || Party == 0 || a.Party == Party)
                      && (!MCList.Any() || MCArray.Contains(a.MC) || MC == a.MC) && ((fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                              (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0))
                     select new
                     {
                         Date = a.Date,
                         Voucher = a.VoucherNo,
                         SelType = a.MtFromType,
                         Party = c.Name,
                         MaterialCenter = b.MCName,
                         TotalAmount = a.TotalAmount
                     }).OrderBy(a => a.Voucher);

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

        [HttpPost]
        public ActionResult GetMaterialToParty(string Voucher, long? Party, string fromdate, string todate, long? MC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (MC == 0 || MC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime datenow = DateTime.Now;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            var v = (from a in db.MtToPartys
                     join b in db.MCs on a.MC equals b.MCId
                     join c in db.Accountss on a.Party equals c.AccountsID into primary
                     from c in primary.DefaultIfEmpty()
                     where (Voucher == null || Voucher == "" || Voucher == a.VoucherNo) &&
                     (Party == null || Party == 0 || a.Party == Party) &&
                     ((fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0))
                     && (!MCList.Any() || MCArray.Contains(a.MC) || MC == a.MC)
                     select new
                     {
                         Date = a.Date,
                         Voucher = a.VoucherNo,
                         SelType = a.MtToType,
                         Party = c.Name,
                         MaterialCenter = b.MCName,
                         TotalAmount = a.TotalAmount
                     }).OrderBy(a => a.Voucher);

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
        #endregion

        #region Stock Transfer Report
        public ActionResult StockTransferReport()
        {
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");
            companySet();
            return View();
        }
        [HttpPost]
        public ActionResult GetStockTransferReport(string Voucher, long? MCFrom, long? MCTo, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (MCFrom == 0 || MCFrom == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime datenow = DateTime.Now;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            var v = (from a in db.StockTransfers
                     join b in db.MCs on a.MCFrom equals b.MCId
                     join c in db.MCs on a.MCTo equals c.MCId
                     where (Voucher == null || Voucher == "" || Voucher == a.Voucher) &&
                     (MCFrom == null || MCFrom == 0 || a.MCFrom == MCFrom) &&
                      (MCTo == null || MCTo == 0 || a.MCTo == MCTo)
                      && ((fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                      && (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0))
                     select new
                     {
                         Date = a.Date,
                         Voucher = a.Voucher,
                         MCFrom = b.MCName,
                         MCTo = c.MCName,
                         TotalAmount = a.TotalAmount
                     }).OrderBy(a => a.Voucher);

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
        #endregion

        #region Daily Summary
        [QkAuthorize(Roles = "Dev,Daily Summary Report")]
        public ActionResult DailySummary()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var userpermission = User.IsInRole("All Sales Entry");
            if (userpermission == true)
            {
                ViewBag.Upermission = "True";
            }
            else
            {
                ViewBag.Upermission = "False";
            }

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Daily Summary Report")]
        public ActionResult DailySummary(string Cashier, string From, long? ddlMC, bool stockable = false)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;


            #region Old Code
            // Basic Details

            //            where (Cashier == "All" || a.CreatedBy == Cashier) && a.SEDate == fdate && (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC)
            //                BillNo = a.BillNo,
            //                Date = a.SEDate,
            //                a.CreatedBy,
            //                CreatedDate = a.SECreatedDate,
            //                a.SENo,
            //                Tax = a.SETax,
            //                TaxAmount = a.SETaxAmount,
            //                STotal = a.SESubTotal,
            //                Note = a.SENote,
            //                GTotal = a.SEGrandTotal,
            //                Discount = a.SEDiscount,
            //                MCName = f.MCName,
            //                a.PayType,
            //                a.SaleType,
            //                a.Status,
            //                a.SalesEntryId,
            //                a.Customer,
            //                a.CustomerType,
            //                //MethodName = a.PaymentMethod == null ? "Cash" : e.MethodName,
            //                MethodName = (a.CustomerType == CustomerType.Card ? e.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit"))
            //


            //     .Select(b => new
            //         b.CustomerType,
            //         b.MethodName,
            //     }).Distinct().Select(o => new ItemSum
            //    // Item wise
            //                    where (Cashier == "All" || c.CreatedBy == Cashier) && c.SEDate == fdate
            //                        a.ItemID,
            //                        a.ItemName,
            //                        Item = a.ItemCode + "-" + a.ItemName,
            //                        PriUnit = e.ItemUnitName,
            //                        SubUnit = f.ItemUnitName,
            //                        ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
            //                        a.ItemUnitID,
            //                        a.SubUnitId,
            //                        a.SellingPrice,
            //                        PriSaleQty = (int?)(from i in db.SEItemss
            //                                            where (Cashier == "All" || j.CreatedBy == Cashier) && j.SEDate == fdate
            //                                             && (i.Item == a.ItemID)//&& i.ItemUnit == a.ItemUnitID)
            //                                            group i by i.ItemId into g
            //                                                Total = g.Sum(x => x.ItemQuantity)
            //                                            }).FirstOrDefault().Total ?? 0,
            //                        SubSaleQty = (int?)(from i in db.SEItemss
            //                                            (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId)
            //                                            group i by i.ItemId into g
            //                                                Total = g.Sum(x => x.ItemQuantity)
            //                                            }).FirstOrDefault().Total ?? 0,
            //                    }).Distinct().AsEnumerable().Select(o => new
            //                        o.ItemID,
            //                        o.Item,
            //                        o.ItemUnitID,
            //                        o.SubUnitId,
            //                        PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
            //                        SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
            //                        o.ConFactor,
            //                        o.PriSaleQty,
            //                        o.ItemName
            //            // type=b.,
            //            name = b.ItemName,
            //            quantity = b.PriSaleQty
            //    // Category Wise
            //                            Category = a.ItemCategoryName,
            //                            TotQty = (int?)(from i in db.SEItemss
            //                                            (k.ItemCategoryID == a.ItemCategoryID)
            //                                                qty = i.ItemQuantity
            //                                            }).Sum(x => x.qty) ?? 0,
            //                        }).Distinct().AsEnumerable().Select(o => new
            //                            o.Category,
            //                            o.TotQty,

            //            // type=b.,
            //            name = b.Category,
            //            quantity = b.TotQty
            #endregion


            #region Abandoned Code
            #endregion

            DailySummaryViewModel vmodel = DailyCommon(Cashier, From, ddlMC, stockable);



            companySet();







            var CustDailySummary = db.EnableSettings.Where(a => a.EnableType == "CustomizedDailySummary").FirstOrDefault();
            var CustDailySummarys = CustDailySummary != null ? CustDailySummary.Status : Status.inactive;
            ViewBag.CustDailySummary = CustDailySummarys;

            return View(vmodel);

        }
        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Daily Summary Report")]
        public ActionResult DailySummaryAjax(string Cashier, string From, long? ddlMC, bool stockable = false)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            var Daily = DailyCommon(Cashier, From, ddlMC, stockable);
            var CustDailySummary = db.EnableSettings.Where(a => a.EnableType == "CustomizedDailySummary").FirstOrDefault();
            var CustDailySummarys = CustDailySummary != null ? CustDailySummary.Status : Status.inactive;
            ViewBag.CustDailySummary = CustDailySummarys;

            return new QuickSoft.Models.LegacyJsonResult { Data = new { Daily, type = CustDailySummarys, mc = MCcheck } };

        }
        public DailySummaryViewModel DailyCommon(string Cashier, string From, long? ddlMC, bool stockable = false)
        {
            DailySummaryViewModel vmodel = new DailySummaryViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            if (From != "")
            {
                fdate = DateTime.ParseExact(From, format, new CultureInfo("en-GB"));
            }
            else
            {
                string today = Convert.ToString(System.DateTime.Now);
                fdate = DateTime.ParseExact(today, format, new CultureInfo("en-GB"));
            }
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            Cashier = userpermission == true ? Cashier : UserId;
            vmodel.time = System.DateTime.Now;
            vmodel.by = User.Identity.Name;
            vmodel.Date = DateTime.Parse(From, new CultureInfo("en-GB"));

            vmodel.UserName = db.Users.Where(a => a.Id == Cashier).Select(a => a.UserName).FirstOrDefault();

            //Total Purchase Credit
            var purdata = (from a in db.PurchaseEntrys
                           where a.PEDate == fdate
                           && (Cashier == "All" || a.CreatedBy == Cashier)
                           && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(a.MaterialCenter) || ddlMC == a.MaterialCenter)
                           //&& (userpermission == true || Cashier == null || Cashier == "All" || a.CreatedBy == UserId)
                           select new
                           {
                               PurchaseTotal = a.PEGrandTotal,
                               VatTotal = a.PETaxAmount,
                               Discount = a.PEDiscount,
                               a.SupplierType
                           }).ToList();

            var CustDailySummary = db.EnableSettings.Where(a => a.EnableType == "CustomizedDailySummary").FirstOrDefault();
            var CustDailySummarys = CustDailySummary != null ? CustDailySummary.Status : Status.inactive;
            if (CustDailySummarys == Status.active)
            {
                vmodel.PDiscount = purdata.Where(b => b.SupplierType == SupplierType.CashSale).Sum(b => b.Discount);
                vmodel.PVat = purdata.Where(b => b.SupplierType == SupplierType.CashSale).Sum(b => b.VatTotal);
                vmodel.PNet = purdata.Where(b => b.SupplierType == SupplierType.CashSale).Sum(b => b.PurchaseTotal);
                vmodel.PTotal = vmodel.PNet + vmodel.PDiscount - vmodel.PVat;


                vmodel.PDiscountC = purdata.Where(b => b.SupplierType == SupplierType.CreditSale).Sum(b => b.Discount);
                vmodel.PVatC = purdata.Where(b => b.SupplierType == SupplierType.CreditSale).Sum(b => b.VatTotal);
                vmodel.PNetC = purdata.Where(b => b.SupplierType == SupplierType.CreditSale).Sum(b => b.PurchaseTotal);
                vmodel.PTotalC = vmodel.PNetC + vmodel.PDiscountC - vmodel.PVatC;
            }
            else
            {
                vmodel.PDiscount = purdata.Sum(b => b.Discount);
                vmodel.PVat = purdata.Sum(b => b.VatTotal);
                vmodel.PNet = purdata.Sum(b => b.PurchaseTotal);
                vmodel.PTotal = vmodel.PNet + vmodel.PDiscount - vmodel.PVat;
            }

            var salesdata = (from a in db.SalesEntrys
                             join b in db.PaymentMethods on a.PaymentMethod equals b.PaymentMethodId into paymeth
                             from b in paymeth.DefaultIfEmpty()
                             join c in db.Customers on a.Customer equals c.CustomerID into cust
                             from c in cust.DefaultIfEmpty()
                             where a.SEDate == fdate
                             && (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC)
                             && (Cashier == "All" || a.CreatedBy == Cashier)
                             && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(a.MaterialCenter) || ddlMC == a.MaterialCenter)
                             //&& (Cashier == null || Cashier == "" || Cashier == "All" || a.CreatedBy == Cashier)
                             select new
                             {
                                 Id = a.SalesEntryId,
                                 BillNo = a.BillNo,
                                 SaleTotal = a.SEGrandTotal,
                                 PayMethod = a.PaymentMethod,
                                 PayType = (a.CustomerType == CustomerType.Walking ? (a.PaymentMethod == null || a.PaymentMethod == 0 ? "Cash" : b.MethodName) : "Credit"),
                                 SaleType = a.SaleType,
                                 CustomerType = a.CustomerType,
                                 CreatedBy = a.CreatedBy,
                                 Discount = a.SEDiscount,
                                 Vat = a.SETaxAmount,
                                 MaterialCenter = a.MaterialCenter,
                                 CustomerName = c.CustomerCode + "-" + c.CustomerName,
                                 CustomerId = a.Customer
                             }).ToList();

            vmodel.CustSum = salesdata.Where(a => a.PayType == "Cash").GroupBy(a => a.CustomerId, (key, group) => new CustDailySummary
            {
                CustomerName = group.Select(k => k.CustomerName).FirstOrDefault(),
                CustAmount = group.Sum(k => k.SaleTotal),
                CustCount = group.Count(),
            }).ToList();

            vmodel.CustSumC = salesdata.Where(a => a.PayType == "Credit").GroupBy(a => a.CustomerId, (key, group) => new CustDailySummary
            {
                CustomerName = group.Select(k => k.CustomerName).FirstOrDefault(),
                CustAmount = group.Sum(k => k.SaleTotal),
                CustCount = group.Count(),
            }).ToList();

            if (CustDailySummarys == Status.active)
            {

                vmodel.SDiscount = salesdata.Where(b => b.PayType == "Cash").Sum(b => b.Discount);
                vmodel.SVat = salesdata.Where(b => b.PayType == "Cash").Sum(b => b.Vat);
                vmodel.SaleNetAmount = salesdata.Where(b => b.PayType == "Cash").Sum(b => b.SaleTotal);
                vmodel.TotalSale = vmodel.SaleNetAmount + vmodel.SDiscount - vmodel.SVat;

                vmodel.SDiscountC = salesdata.Where(b => b.PayType == "Credit").Sum(b => b.Discount);
                vmodel.SVatC = salesdata.Where(b => b.PayType == "Credit").Sum(b => b.Vat);
                vmodel.SaleNetAmountC = salesdata.Where(b => b.PayType == "Credit").Sum(b => b.SaleTotal);
                vmodel.TotalSaleC = vmodel.SaleNetAmountC + vmodel.SDiscountC - vmodel.SVatC;
            }
            else
            {
                vmodel.SDiscount = salesdata.Sum(b => b.Discount);
                vmodel.SVat = salesdata.Sum(b => b.Vat);
                vmodel.SaleNetAmount = salesdata.Sum(b => b.SaleTotal);
                vmodel.TotalSale = vmodel.SaleNetAmount + vmodel.SDiscount - vmodel.SVat;
            }

            var salesRdata = (from a in db.SalesReturns
                              join c in db.Customers on a.Customer equals c.CustomerID into cust
                              from c in cust.DefaultIfEmpty()
                              where a.SRDate == fdate
                              && (Cashier == "All" || a.CreatedBy == Cashier)
                              && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(a.MaterialCenter) || ddlMC == a.MaterialCenter)
                              //&& (userpermission == true || Cashier == null || Cashier == "All" || a.CreatedBy == UserId)

                              select new
                              {
                                  Id = a.SalesReturnId,
                                  BillNo = a.BillNo,
                                  SaleretTotal = a.SRGrandTotal,
                                  PayType = a.PayType,
                                  SaleType = a.SaleType,
                                  CustomerType = a.CustomerType,
                                  Discount = a.SRDiscount,
                                  Vat = a.SRTaxAmount,
                                  CustomerId = a.Customer,
                                  CustomerName = c.CustomerCode + "-" + c.CustomerName,

                              }).ToList();




            if (CustDailySummarys == Status.active)
            {
                //Sales Return cash 
                vmodel.SaleRetDiscount = salesRdata.Where(a => a.CustomerType == CustomerType.Walking).Sum(b => b.Discount);
                vmodel.SaleRetVat = salesRdata.Where(a => a.CustomerType == CustomerType.Walking).Sum(b => b.Vat);
                vmodel.SaleRetNetAmount = salesRdata.Where(a => a.CustomerType == CustomerType.Walking).Sum(b => b.SaleretTotal);
                vmodel.SaleRetTotal = vmodel.SaleRetNetAmount - vmodel.SaleRetDiscount - vmodel.SaleRetVat;

                //Sales Return cash 
                vmodel.SaleRetDiscountC = salesRdata.Where(a => a.CustomerType == CustomerType.Customer).Sum(b => b.Discount);
                vmodel.SaleRetVatC = salesRdata.Where(a => a.CustomerType == CustomerType.Customer).Sum(b => b.Vat);
                vmodel.SaleRetNetAmountC = salesRdata.Where(a => a.CustomerType == CustomerType.Customer).Sum(b => b.SaleretTotal);
                vmodel.SaleRetTotalC = vmodel.SaleRetNetAmountC - vmodel.SaleRetDiscountC - vmodel.SaleRetVatC;
            }
            else
            {
                //Sales Return Total
                vmodel.SaleRetDiscount = salesRdata.Sum(b => b.Discount);
                vmodel.SaleRetVat = salesRdata.Sum(b => b.Vat);
                vmodel.SaleRetNetAmount = salesRdata.Sum(b => b.SaleretTotal);
                vmodel.SaleRetTotal = vmodel.SaleRetNetAmount - vmodel.SaleRetDiscount - vmodel.SaleRetVat;
            }

            vmodel.CustSumSR = salesRdata.Where(a => a.CustomerType == CustomerType.Walking).GroupBy(a => a.CustomerId, (key, group) => new CustDailySummary
            {
                CustomerName = group.Select(k => k.CustomerName).FirstOrDefault(),
                CustAmount = group.Sum(k => k.SaleretTotal),
                CustCount = group.Count(),
            }).ToList();

            vmodel.CustSumSRC = salesRdata.Where(a => a.CustomerType == CustomerType.Customer).GroupBy(a => a.CustomerId, (key, group) => new CustDailySummary
            {
                CustomerName = group.Select(k => k.CustomerName).FirstOrDefault(),
                CustAmount = group.Sum(k => k.SaleretTotal),
                CustCount = group.Count(),
            }).ToList();

            var Purretdata = (from a in db.PurchaseReturns
                              where a.PRDate == fdate
                              && (Cashier == "All" || a.CreatedBy == Cashier)
                              && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(a.MaterialCenter) || ddlMC == a.MaterialCenter)
                              //&& (userpermission == true || Cashier == null || Cashier == "All" || a.CreatedBy == UserId)

                              select new
                              {
                                  PurretTotal = a.PRGrandTotal,
                                  Discount = a.PRDiscount,
                                  Vat = a.PRTaxAmount
                              }).ToList();

            //Sales Return Total
            vmodel.PRDiscount = Purretdata.Sum(b => b.Discount);
            vmodel.PRVat = Purretdata.Sum(b => b.Vat);
            vmodel.PRNet = Purretdata.Sum(b => b.PurretTotal);

            vmodel.PRTotal = vmodel.PRNet - vmodel.PRDiscount - vmodel.PRVat;

            var StockTransfer = (from a in db.StockTransfers
                                 where a.Date == fdate
                                 && (Cashier == "All" || a.CreatedBy == Cashier)
                                 && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(a.MCTo) || ddlMC == a.MCTo)
                                 //&& (userpermission == true || Cashier == null || Cashier == "All" || a.CreatedBy == UserId)
                                 select new
                                 {
                                     Id = a.Id,
                                     Gtotal = a.TotalAmount,
                                 }).ToList();
            vmodel.STransTotal = StockTransfer.Sum(c => c.Gtotal);

            if (CustDailySummarys == Status.active)
            {
                //Net
                vmodel.NetSale = (vmodel.SaleNetAmount + vmodel.SaleNetAmountC) - (vmodel.SaleRetNetAmount + vmodel.SaleRetNetAmountC);
                vmodel.NetPurchase = (vmodel.PNet + vmodel.PNetC) - vmodel.PRNet;
            }
            else
            {
                vmodel.NetSale = vmodel.SaleNetAmount - vmodel.SaleRetNetAmount;
                vmodel.NetPurchase = vmodel.PNet - vmodel.PRNet;
            }


            var SaleCreditTotal = salesdata.Where(x => x.PayType == "Credit").Sum(c => c.SaleTotal);

            var SaleCashTotal = salesdata.Where(x => x.PayType == "Cash").Sum(c => c.SaleTotal);

            //                    where a.SRDate == fdate
            //                    && (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC)
            //                    && (Cashier == "All" || a.CreatedBy == Cashier)
            //                    && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(a.MaterialCenter) || ddlMC == a.MaterialCenter)
            //                    //&& (userpermission == true || Cashier == null || Cashier == "All" || a.CreatedBy == UserId)

            //                        Id = a.SalesReturnId,
            //                        BillNo = a.BillNo,
            //                        SaleretTotal = a.SRGrandTotal,
            //                        PayType = a.PayType,
            //                        SaleType = a.SaleType,
            //                        CustomerType = a.CustomerType



            //Receipt Data
            var RptData = (from a in db.Receipts
                           join c in db.Accountss on a.PayFrom equals c.AccountsID
                           where a.Date == fdate
                           && (Cashier == "All" || a.CreatedBy == Cashier)
                           && (a.editable == choice.Yes)
                           //&& (userpermission == true || Cashier == null || Cashier == "All" || a.CreatedBy == UserId)                           
                           select new
                           {
                               Id = a.ReceiptId,
                               RptGtotal = a.GrandTotal,
                               MOPay = a.MOPayment,
                               a.MOPayment,
                               c.AccountsID,
                               c.Name,
                               c.Group,
                           }).ToList();

            vmodel.CustSumR = RptData.Where(a => a.Group == 12 && a.MOPayment == ModeOfPayment.Cash).GroupBy(a => a.AccountsID, (key, group) => new CustDailySummary
            {
                CustomerName = group.Select(k => k.Name).FirstOrDefault(),
                CustAmount = group.Sum(k => k.RptGtotal),
                CustCount = group.Count(),
            }).ToList();


            vmodel.CustSumRC = RptData.Where(a => a.Group == 12 && a.MOPayment == ModeOfPayment.PDC || a.MOPayment == ModeOfPayment.CDC).GroupBy(a => a.AccountsID, (key, group) => new CustDailySummary
            {
                CustomerName = group.Select(k => k.Name).FirstOrDefault(),
                CustAmount = group.Sum(k => k.RptGtotal),
                CustCount = group.Count(),
            }).ToList();

            var RptTotal = RptData.Sum(c => c.RptGtotal);

            var RptCashTotal = RptData.Where(c => c.MOPay == ModeOfPayment.Cash).Sum(c => c.RptGtotal);

            var RptCrTotal = RptData.Where(c => c.MOPay == ModeOfPayment.PDC).Sum(c => c.RptGtotal);

            // Payment Data
            var PayData = (from a in db.Payments
                           where a.Date == fdate
                           && (Cashier == "All" || a.CreatedBy == Cashier)
                           && (a.editable == choice.Yes)
                           //&& (userpermission == true || Cashier == null || Cashier == "All" || a.CreatedBy == UserId)
                           select new
                           {
                               Id = a.PaymentId,
                               PayGtotal = a.GrandTotal,
                               MOPay = a.MOPayment,
                               a.MOPayment
                           }).ToList();

            var PayTotal = PayData.Sum(c => c.PayGtotal);

            var PayCashTotal = PayData.Where(c => c.MOPay == ModeOfPayment.Cash).Sum(c => c.PayGtotal);

            var PayCrTotal = PayData.Where(c => c.MOPay == ModeOfPayment.PDC).Sum(c => c.PayGtotal);


            if (CustDailySummarys == Status.active)
            {
                vmodel.Receipt = RptData.Where(c => c.MOPayment == ModeOfPayment.Cash).Sum(c => c.RptGtotal);
                vmodel.ChqReceipt = RptData.Where(c => c.MOPayment == ModeOfPayment.PDC || c.MOPayment == ModeOfPayment.CDC).Sum(c => c.RptGtotal);

                vmodel.Payment = PayData.Where(c => c.MOPayment == ModeOfPayment.Cash).Sum(c => c.PayGtotal);
                vmodel.ChqPayment = PayData.Where(c => c.MOPayment == ModeOfPayment.PDC || c.MOPayment == ModeOfPayment.CDC).Sum(c => c.PayGtotal);
            }
            else
            {
                vmodel.Receipt = RptData.Sum(c => c.RptGtotal);
                vmodel.Payment = PayData.Sum(c => c.PayGtotal);
            }

            #region Items
            if (stockable == true)
            {
                var SaleItem = (from a in db.Items
                                join b in db.SEItemss on a.ItemID equals b.Item
                                join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId
                                join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into punit
                                from e in punit.DefaultIfEmpty()
                                join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into sunit
                                from f in sunit.DefaultIfEmpty()
                                where c.SEDate == fdate
                                && (ddlMC == null || ddlMC == 0 || ddlMC == c.MaterialCenter)
                                && (Cashier == "All" || c.CreatedBy == Cashier)
                                //&& (userpermission == true || Cashier == null || Cashier == "All" || c.CreatedBy == UserId)
                                select new
                                {
                                    a.ItemID,
                                    a.ItemName,
                                    Item = a.ItemCode + "-" + a.ItemName,
                                    PriUnit = e.ItemUnitName,
                                    SubUnit = f.ItemUnitName,
                                    ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                                    a.ItemUnitID,
                                    a.SubUnitId,
                                    a.SellingPrice,

                                    PriSaleQty = (int?)(from i in db.SEItemss
                                                        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                        where j.SEDate == fdate
                                                        //&& (userpermission == true || Cashier == null || Cashier == "All" || j.CreatedBy == UserId)
                                                        && (Cashier == "All" || j.CreatedBy == Cashier)
                                                        && (i.Item == a.ItemID)
                                                        group i by i.ItemId into g
                                                        select new
                                                        {
                                                            Total = g.Sum(x => x.ItemQuantity)
                                                        }).FirstOrDefault().Total ?? 0,

                                    SubSaleQty = (int?)(from i in db.SEItemss
                                                        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                        where j.SEDate == fdate
                                                        //&& (userpermission == true || Cashier == null || Cashier == "All" || j.CreatedBy == UserId)
                                                        && (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId)
                                                        && (Cashier == "All" || j.CreatedBy == Cashier)
                                                        group i by i.ItemId into g
                                                        select new
                                                        {
                                                            Total = g.Sum(x => x.ItemQuantity)
                                                        }).FirstOrDefault().Total ?? 0,
                                }).Distinct().AsEnumerable().Select(o => new
                                {
                                    o.ItemID,
                                    o.Item,
                                    o.ItemUnitID,
                                    o.SubUnitId,
                                    PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                    SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                    o.ConFactor,
                                    o.PriSaleQty,
                                    o.ItemName,
                                }).OrderBy(a => a.ItemName).ToList();

                if (SaleItem.Any())
                {
                    vmodel.SaleItems = SaleItem.Select(b => new SaleItemSum
                    {
                        UName = b.PriUnit != null ? b.PriUnit : (b.SubUnit != null) ? b.SubUnit : "",
                        IName = b.ItemName,
                        Quantity = b.PriSaleQty
                    }).ToList();
                }

                var SaleRetItem = (from a in db.Items
                                   join b in db.SRItemss on a.ItemID equals b.Item
                                   join c in db.SalesReturns on b.SalesReturnId equals c.SalesReturnId
                                   join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into punit
                                   from e in punit.DefaultIfEmpty()
                                   join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into sunit
                                   from f in sunit.DefaultIfEmpty()
                                   where c.SRDate == fdate
                                   && (Cashier == "All" || c.CreatedBy == Cashier)
                                    //&& (userpermission == true || Cashier == null || Cashier == "All" || c.CreatedBy == UserId)                                    
                                    && (ddlMC == null || ddlMC == 0 || ddlMC == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemID,
                                       a.ItemName,
                                       Item = a.ItemCode + "-" + a.ItemName,
                                       PriUnit = e.ItemUnitName,
                                       SubUnit = f.ItemUnitName,
                                       ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                                       a.ItemUnitID,
                                       a.SubUnitId,
                                       a.SellingPrice,
                                       PriSaleQty = (int?)(from i in db.SRItemss
                                                           join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                           where j.SRDate == fdate
                                                           && (Cashier == "All" || j.CreatedBy == Cashier)
                                                           //&& (userpermission == true || Cashier == null || Cashier == "All" || j.CreatedBy == UserId)                                                           
                                                           && (i.Item == a.ItemID)
                                                           group i by i.ItemId into g
                                                           select new
                                                           {
                                                               Total = g.Sum(x => x.ItemQuantity)
                                                           }).FirstOrDefault().Total ?? 0,

                                       SubSaleQty = (int?)(from i in db.SRItemss
                                                           join j in db.SalesReturns on i.SalesReturnId equals j.SalesEntryId
                                                           where j.SRDate == fdate
                                                           && (Cashier == "All" || j.CreatedBy == Cashier)
                                                           //&& (userpermission == true || Cashier == null || Cashier == "All" || j.CreatedBy == UserId)
                                                           && (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId)
                                                           group i by i.ItemId into g
                                                           select new
                                                           {
                                                               Total = g.Sum(x => x.ItemQuantity)
                                                           }).FirstOrDefault().Total ?? 0,

                                   }).Distinct().AsEnumerable().Select(o => new
                                   {
                                       o.ItemID,
                                       o.Item,
                                       o.ItemUnitID,
                                       o.SubUnitId,
                                       PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                       SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                       o.ConFactor,
                                       o.PriSaleQty,
                                       o.ItemName
                                   }).OrderBy(a => a.ItemName).ToList();

                if (SaleRetItem.Any())
                {
                    vmodel.SaleRetItems = SaleRetItem.Select(b => new SaleItemSum
                    {
                        UName = b.PriUnit != null ? b.PriUnit : (b.SubUnit != null) ? b.SubUnit : "",
                        IName = b.ItemName,
                        Quantity = b.PriSaleQty
                    }).ToList();
                }

            }

            #endregion

            var MCName = db.MCs.Where(x => x.MCId == ddlMC).Select(x => x.MCName).FirstOrDefault();
            vmodel.MCName = MCName;
            vmodel.TotalCashSale = SaleCashTotal;
            vmodel.TotalCreditSale = SaleCreditTotal;


            vmodel.RptTotalCash = RptCashTotal;
            vmodel.RptTotalPdc = RptCrTotal;

            vmodel.PayTotalCash = PayCashTotal;
            vmodel.PayTotalPdc = PayCrTotal;

            vmodel.TotalVAT = vmodel.PVat + vmodel.PVatC + vmodel.SVat + vmodel.SVatC + vmodel.SaleRetVat + vmodel.PRVat;
            vmodel.TotalVAT = vmodel.TotalVAT != null ? vmodel.TotalVAT : 0;

            return vmodel;
        }

        #endregion

        #region Hire Return Note
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report HireReturn")]
        public ActionResult HireReturn()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();


            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.Customer = OpAll;

            companySet();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");


            ViewBag.getProj = QkSelect.List(
                                new List<SelectListItem>
                                {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            return View();
        }
        [QkAuthorize(Roles = "Dev,Report HireReturn")]
        public ActionResult GetHireReturn(string BillNo, long? customer, string fromdate, string todate, long? ddlMC, long? project, long? task)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

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
            var v = (from b in db.HireReturns
                     join c in db.Customers on b.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()
                     join f in db.Employees on b.Cashier equals f.EmployeeId into emp
                     from f in emp.DefaultIfEmpty()
                     join e in db.MCs on b.MaterialCenter equals e.MCId into mcs
                     from e in mcs.DefaultIfEmpty()
                     join i in db.Accountss on c.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where (BillNo == "" || b.BillNo == BillNo) &&
                        (customer == 0 || b.Customer == customer) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
                        && (!MCList.Any() || MCArray.Contains(b.MaterialCenter) || ddlMC == b.MaterialCenter)
                        && (project == 0 || project == null || b.Project == project)
                        && (task == 0 || task == null || b.ProTask == task)
                     select new
                     {
                         b.HireReturnId,
                         b.BillNo,
                         b.Date,
                         b.Items,
                         b.ItemQuantity,
                         TaxRegNo = i.TRN,
                         Customer = c.CustomerName,
                         EmpName = f.FirstName + " " + f.LastName,
                         User = db.Users.Where(s => s.Id == b.CreatedUserId).Select(s => s.UserName).FirstOrDefault(),
                         b.CreatedDate,
                         MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All"
                     }).OrderBy(a => a.Date);

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

        #endregion

        #region Item Cat Wise Profit Report
        [HttpGet]
        [QkAuthorize(Roles = "Dev,ItemCatProfit Report")]
        public ActionResult ItemCatProfit()
        {
            ViewBag.Category = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            return View();
        }
        [QkAuthorize(Roles = "Dev,ItemCatProfit Report")]
        public ActionResult GetItemCatProfit(string cname, long? category)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            var v = (from b in db.Items
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     where b.KeepStock == true &&
                     (cname == "" || b.ItemCode.Contains(cname) || b.ItemName.Contains(cname)) &&
                     (category == 0 || category == null || b.ItemCategoryID == category)
                     orderby b.ItemID ascending
                     select new
                     {
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         ItemWithCode = b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         b.OpeningStock,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,

                         PriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemTotalAmount).Sum() ?? 0,
                         SubPurchase = (b.ItemUnit != null) ? (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemTotalAmount).Sum() ?? 0 : 0,

                         QPriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                         QSubPurchase = (b.ItemUnit != null) ? (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0 : 0,


                         PriDiscount = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemDiscount).Sum() ?? 0,
                         SubDiscount = (b.ItemUnit != null) ? (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemDiscount).Sum() ?? 0 : 0,

                         PriTax = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemTaxAmount).Sum() ?? 0,
                         SubTax = (b.ItemUnit != null) ? (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemTaxAmount).Sum() ?? 0 : 0,

                         PriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemSubTotal).Sum() ?? 0,
                         SubSale = (b.ItemUnit != null) ? (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemSubTotal).Sum() ?? 0 : 0,

                         QPriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                         QSubSale = (b.ItemUnit != null) ? (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() : 0,

                         PriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemTotalAmount).Sum() ?? 0,
                         SubPReturn = (b.ItemUnit != null) ? (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemTotalAmount).Sum() ?? 0 : 0,

                         QtyPriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                         QtySubPReturn = (b.ItemUnit != null) ? (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0 : 0,

                         PriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemTotalAmount).Sum() ?? 0,
                         SubSReturn = (b.ItemUnit != null) ? (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemTotalAmount).Sum() ?? 0 : 0,

                         QPriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                         QSubSReturn = (b.ItemUnit != null) ? (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0 : 0,

                         OpStockCost = b.OpeningStock * b.PurchasePrice,

                         OpQty = (b.OpeningStock * b.ConFactor) ?? b.OpeningStock
                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.ItemID,
                         o.ItemCode,
                         Item = o.ItemName,
                         o.ItemWithCode,
                         o.ItemUnitID,
                         o.SubUnitId,
                         PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                         SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                         o.categoryname,

                         OpeningStock = o.OpeningStock,
                         o.OpStockCost,
                         o.ConFactor,

                         //Sale amount
                         PriSale = o.PriSale,
                         SubSale = o.SubSale,
                         //Sale Return amount
                         PriSReturn = o.PriSReturn,
                         SubSReturn = o.SubSReturn,
                         //Purchase Amount
                         PriPurchase = o.PriPurchase,
                         SubPurchase = o.SubPurchase,
                         //Purchase Return Amount
                         PriPReturn = o.PriPReturn,
                         SubPReturn = o.SubPReturn,

                         //Qty Sale
                         QPriSale = o.QPriSale,
                         QSubSale = o.SubSale,
                         //Qty SaleReturn
                         QPriSReturn = o.QPriSReturn,
                         QSubSReturn = o.QSubSReturn,
                         //Qty Purchase
                         QPriPur = o.QPriPurchase,
                         QSubPur = o.SubPurchase,
                         //Qty Purchase return
                         QPriPReturn = o.QtyPriPReturn,
                         QSubPReturn = o.QtySubPReturn,

                         //Tax
                         PriTax = o.PriTax,
                         SubTax = o.SubTax,
                         //Discount
                         PriDiscount = o.PriDiscount,
                         SubDiscount = o.SubDiscount,

                         NetProfit = 0,
                         //TSaleReturn = (o.PriSReturn + o.SubSReturn),//
                         //TDiscount = o.PriDiscount + o.SubDiscount,//

                         //TTax = o.PriTax + o.SubTax,//

                         //NetProfit = (o.QPriPurchase == 0 && o.QSubPurchase == 0 && o.OpQty == 0 ? 0 :
                         //((o.PriSale + o.SubSale)
                         //- (/*Sreturn*/(o.PriSReturn + o.SubSReturn))
                         //- (((o.PriPurchase + o.SubPurchase + o.OpStockRate) / (((o.QPriPurchase * o.ConFactor) + o.QSubPurchase) + o.OpQty))
                         //* /*quantity*/(((o.QPriSale * o.ConFactor) + o.QSubSale) - ((o.QPriSReturn * o.ConFactor) + o.QSubSReturn)))) -
                         //((/*discount*/(o.PriDiscount + o.SubDiscount)
                         //+/*tax*/(o.PriTax + o.SubTax))))

                     }).OrderBy(a => a.Item);

            //SORT
            recordsTotal = v.Count();
            var data = v.ToList();

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

        #endregion

        #region Hire Expiring List
        [HttpGet]
        [QkAuthorize(Roles = "Dev,HireExpire Report")]
        public ActionResult HireExpire()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();


            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.Customer = OpAll;

            companySet();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            return View();
        }
        [QkAuthorize(Roles = "Dev,HireExpire Report")]
        public ActionResult GetHireExpire(string BillNo, long? customer, string fromdate, string todate, long? Htype)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

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
            var fromv = "Sale";
            var tov = "SaleExtend";
            var v = (from b in db.SalesEntrys
                     join c in db.Customers on b.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = b.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join e in db.MCs on b.MaterialCenter equals e.MCId into mcs
                     from e in mcs.DefaultIfEmpty()
                     let sh = db.ConvertTransactionss.Where(ap => ap.From == b.SalesEntryId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()
                     let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.From == b.SalesEntryId).Select(x => x.From).FirstOrDefault()

                     let saleitem = (Int32?)db.SEItemss.Where(x => x.SalesEntry == b.SalesEntryId).Select(x => x.ItemQuantity).Sum() ?? 0
                     let hireitem = (Int32?)db.HrItems.Join(db.HireReturns, u => u.Hr, r => r.HireReturnId, (u, r) => new { u, r }).Where(x => x.r.Invoice == b.SalesEntryId).Select(x => x.u.ItemQuantity).Sum() ?? 0
                     where (BillNo == "" || b.BillNo == BillNo)
                     //&& (b.SalesEntryId != f.From)
                     && (customer == 0 || b.Customer == customer)
                     && (Htype == null || Htype == 0 || h.HireType == Htype)
                     && (fromdate == "" || EF.Functions.DateDiffDay(h.StartDate, fdate) <= 0)
                     && (todate == "" || EF.Functions.DateDiffDay(h.StartDate, tdate) >= 0)
                     && (b.SalesEntryId != chkextend)
                     && saleitem != hireitem
                     select new
                     {
                         b.SalesEntryId,
                         b.SENo,
                         HExtent = sh.ConvertFrom,
                         HireDetailId = h.HireDetailId,
                         HireType = h.HireType,
                         BillNo = b.BillNo,
                         Startdate = h.StartDate,
                         Enddate = h.EndDate,
                         GrandTotal = b.SEGrandTotal,
                         Customer = c.CustomerName,
                         SaleType = b.SaleType,
                         User = db.Users.Where(s => s.Id == b.CreatedBy).Select(s => s.UserName).FirstOrDefault(),
                         saleitem = hireitem
                     }).OrderBy(a => a.Startdate);

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

        #endregion


        #region Hire Expiring List
        [HttpGet]
        [QkAuthorize(Roles = "Dev,CrossHireExpire Report")]
        public ActionResult CrossHireExpire()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();


            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.Supplier = OpAll;

            companySet();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Purchase", Value="1"},
                new SelectListItem() {Text = "CrossHire", Value="2"},
            }, "Value", "Text");
            return View();
        }
        [QkAuthorize(Roles = "Dev,CrossHireExpire Report")]
        public ActionResult GetCrossHireExpire(string BillNo, long? supplier, string fromdate, string todate, long? Htype)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

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
            var fromv = "purchase";
            var tov = "PurchaseExtend";
            var v = (from a in db.PurchaseEntrys
                     join b in db.Suppliers on a.Supplier equals b.SupplierID
                     join h in db.HireDetails on new { h1 = a.PurchaseEntryId, h2 = "purchase" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join d in db.CrossHireReturns on a.PurchaseEntryId equals d.Invoice into hi
                     from d in hi.DefaultIfEmpty()
                     join g in db.CrossHrItems on a.PurchaseEntryId equals g.Hr into hr
                     from g in hr.DefaultIfEmpty()
                     let sh = db.ConvertTransactionss.Where(ap => ap.From == a.PurchaseEntryId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()
                     let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.From == a.PurchaseEntryId).Select(x => x.From).FirstOrDefault()

                     let peitem = (Int32?)db.PEItemss.Where(x => x.PurchaseEntry == a.PurchaseEntryId).Select(x => x.ItemQuantity).Sum() ?? 0
                     let hireitem = (Int32?)db.CrossHrItems.Join(db.CrossHireReturns, u => u.Hr, r => r.HireReturnId, (u, r) => new { u, r }).Where(x => x.r.Invoice == a.PurchaseEntryId).Select(x => x.u.ItemQuantity).Sum() ?? 0

                     where (BillNo == "" || a.BillNo == BillNo)
                     // && (a.PurchaseEntryId != f.From)
                     && (supplier == 0 || b.SupplierID == supplier)
                     && (Htype == null || Htype == 0 || h.HireType == Htype)
                     && (fromdate == "" || EF.Functions.DateDiffDay(h.StartDate, fdate) <= 0)
                     && (todate == "" || EF.Functions.DateDiffDay(h.StartDate, tdate) >= 0)
                     && (a.PurchaseEntryId != chkextend)
                     && peitem != hireitem
                     select new
                     {
                         a.PurchaseEntryId,
                         a.PENo,
                         HExtent = sh.ConvertFrom,
                         HireDetailId = h.HireDetailId,
                         HireType = h.HireType,
                         BillNo = a.BillNo,
                         Startdate = h.StartDate,
                         Enddate = h.EndDate,
                         GrandTotal = a.PEGrandTotal,
                         Supplier = b.SupplierName,
                         PurType = a.PurType,
                         User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.UserName).FirstOrDefault(),
                         saleitem = hireitem
                     }).OrderBy(a => a.Startdate);

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

        #endregion



        public ActionResult AmountReceivablePayable2()
        {
            companySet();
            return View();
        }

        [HttpGet]
        public ActionResult PurchaseUsage()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Item = OptAll;
            ViewBag.Brand = OptAll;
            ViewBag.Category = OptAll;
            ViewBag.Supplier = OptAll;
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            ViewBag.MC = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
           }, "Value", "Text", 0);

            ViewBag.SalesExecutive = QkSelect.List(
                                           new List<SelectListItem>
                                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                           }, "Value", "Text", 0);
            ViewBag.MatReqby = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);
            ViewBag.Employee = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);

            return View();
        }
        [HttpPost]
        public ActionResult GetPurchaseUsage(string srchtxt, long? Item, long? supplier, long? ItemBrand, long? ItemCategory, long? saleExe, long? matReqBy, string fromdate, string todate, long? mc)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
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

            decimal? daydiff = (decimal?)(tdate - fdate).Value.TotalDays;
            var peitemid = (from a in db.Items
                            join h in db.PEItemss on a.ItemID equals h.Item into temp4
                            from h in temp4.DefaultIfEmpty()
                            join b in db.PurchaseEntrys on h.PurchaseEntry equals b.PurchaseEntryId into cust
                            from b in cust.DefaultIfEmpty()

                            join c in db.Suppliers on b.Supplier equals c.SupplierID into temp
                            from c in temp.DefaultIfEmpty()
                            join e in db.ItemBrands on a.ItemBrandID equals e.ItemBrandID into temp2
                            from e in temp2.DefaultIfEmpty()
                            join f in db.Employees on b.PECashier equals f.EmployeeId into temp3
                            from f in temp3.DefaultIfEmpty()
                            join i in db.ItemUnits on a.SubUnitId equals i.ItemUnitID into temp5
                            from i in temp5.DefaultIfEmpty()
                            join j in db.ConvertTransactionss on b.PurchaseEntryId equals j.To into conn1
                            from j in conn1.DefaultIfEmpty()
                            join k in db.PurchaseOrders on j.From equals k.PurchaseOrderId into conn2
                            from k in conn2.DefaultIfEmpty()
                            join l in db.ConvertTransactionss on k.PurchaseOrderId equals l.To into conn3
                            from l in conn3.DefaultIfEmpty()
                            join m in db.MaterialRequisitions on l.From equals m.MaterialRequisitionId into conn4
                            from m in conn4.DefaultIfEmpty()

                            where (Item == 0 || h.Item == Item) &&
                               (supplier == 0 || b.Supplier == supplier) &&
                               (ItemBrand == 0 || a.ItemBrandID == ItemBrand) &&
                               (ItemCategory == 0 || a.ItemCategoryID == ItemCategory) &&
                               (saleExe == 0 || b.PECashier == saleExe) &&
                               (matReqBy == 0 || m.MRCashier == matReqBy) &&
                               (fromdate == "" || EF.Functions.DateDiffDay(b.PEDate, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(b.PEDate, tdate) >= 0) &&
                               (mc == null || mc == 0 || b.MaterialCenter == mc) &&
                               (srchtxt == "" || a.ItemName.Contains(srchtxt))
                            select new
                            {
                                a.ItemID
                            }).Distinct().Select(o => o.ItemID).ToArray();
            db.SetCommandTimeOut(60 * 60);

            var selitem = new SqlParameter("@ItemId", "");
            var selmc = new SqlParameter("@MCId", (object)mc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "");
            var stkble = new SqlParameter("@Stockble", 1);
            var catgry = new SqlParameter("@CategoryId", "");
            var fromdatee = new SqlParameter("@fromdate", "");
            var todatee = new SqlParameter("@todate", (object)fdate ?? DBNull.Value);
            var stype = new SqlParameter("@Stype", "1");

            var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod44 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdatee, todatee, stype).AsEnumerable().ToList();
            dataadd = dataadd.Where(o => peitemid.Contains(o.IItemID)).ToList();
            var seliteme = new SqlParameter("@ItemId", "");
            var selmce = new SqlParameter("@MCId", (object)mc ?? DBNull.Value);
            var brande = new SqlParameter("@BrandId", "");
            var stkblee = new SqlParameter("@Stockble", 1);
            var catgrye = new SqlParameter("@CategoryId", "");
            var fromdateee = new SqlParameter("@fromdate", "");

            var stypee = new SqlParameter("@Stype", "1");
            var todateee = new SqlParameter("@todate", (object)tdate ?? DBNull.Value);
            var dataadde = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod44 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", seliteme, selmce, brande, stkblee, catgrye, fromdateee, todateee, stypee).AsEnumerable().ToList();
            dataadde = dataadde.Where(o => peitemid.Contains(o.IItemID)).ToList();
            var selitemee = new SqlParameter("@ItemId", "");
            var selmcee = new SqlParameter("@MCId", (object)mc ?? DBNull.Value);
            var brandee = new SqlParameter("@BrandId", "");
            var stkbleee = new SqlParameter("@Stockble", 1);
            var catgryee = new SqlParameter("@CategoryId", "");
            var fromdateeee = new SqlParameter("@fromdate", "");

            var stypeee = new SqlParameter("@Stype", "1");
            var todateeee = new SqlParameter("@todate", "");
            var dataaddee = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitemee, selmcee, brandee, stkbleee, catgryee, fromdateeee, todateeee, stypeee).AsEnumerable().ToList();
            dataaddee = dataaddee.Where(o => peitemid.Contains(o.IItemID)).ToList();


            List<purchaseussageFINAL> final = new List<purchaseussageFINAL>();
            foreach (var i in peitemid)
            {
                if (dataadde.Any(o => o.IItemID == i))
                {
                    purchaseussageFINAL p = new purchaseussageFINAL();
                    p.closingstock = (dataadde.Where(o => o.IItemID == i).Sum(o => o.ITotalQty) < 0) ? 0 : dataadde.Where(o => o.IItemID == i).Sum(o => o.ITotalQty);
                    p.ItemCode = dataadde.Where(o => o.IItemID == i).Select(o => o.IItemCode).FirstOrDefault();
                    p.ItemName = dataadde.Where(o => o.IItemID == i).Select(o => o.IItemName).FirstOrDefault();
                    p.currentstock = (dataaddee.Where(o => o.IItemID == i).Sum(o => o.ITotalQty) < 0) ? 0 : dataaddee.Where(o => o.IItemID == i).Sum(o => o.ITotalQty);

                    p.openingstock = (dataadd.Where(o => o.IItemID == i).Sum(o => o.ITotalQty) < 0) ? 0 : dataadd.Where(o => o.IItemID == i).Sum(o => o.ITotalQty);
                    p.purchaseprice = dataadde.Where(o => o.IItemID == i).Select(o => o.IPurchasePrice).FirstOrDefault();
                    p.totalpurchase = dataadde.Where(o => o.IItemID == i && o.purpose == "P").Select(o => o.ITotalQty).FirstOrDefault() - dataadd.Where(o => o.IItemID == i && o.purpose == "P").Select(o => o.ITotalQty).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault();
                    p.totalpurchasereturn = (dataadde.Where(o => o.IItemID == i && o.purpose == "PR").Select(o => o.ITotalQty).FirstOrDefault() - dataadd.Where(o => o.IItemID == i && o.purpose == "PR").Select(o => o.ITotalQty).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault()) * -1;
                    p.totalsales = (dataadde.Where(o => o.IItemID == i && o.purpose == "S").Select(o => o.ITotalQty).FirstOrDefault() - dataadd.Where(o => o.IItemID == i && o.purpose == "S").Select(o => o.ITotalQty).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault()) * -1;

                    p.totalsalesreturn = dataadde.Where(o => o.IItemID == i && o.purpose == "SR").Select(o => o.ITotalQty).FirstOrDefault() - dataadd.Where(o => o.IItemID == i && o.purpose == "SR").Select(o => o.ITotalQty).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault();
                    p.totalstckin = dataadde.Where(o => o.IItemID == i && o.purpose == "STI").Select(o => o.ITotalQty).FirstOrDefault() - dataadd.Where(o => o.IItemID == i && o.purpose == "STI").Select(o => o.ITotalQty).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault();
                    p.totalstockout = dataadde.Where(o => o.IItemID == i && o.purpose == "STO").Select(o => o.ITotalQty).FirstOrDefault() - dataadd.Where(o => o.IItemID == i && o.purpose == "STO").Select(o => o.ITotalQty).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault();
                    p.daydiff = daydiff;
                    final.Add(p);
                }
            }


            //                    (mc == null || mc == 0 || b.MaterialCenter == mc)

            //                     ItemID = a.ItemID,
            //                     ItemName = a.ItemName,
            //                     purchaseprice=a.PurchasePrice,
            //                     //ItemUnitName = g.FirstOrDefault().ItemUnitName,

            //                     //PurchasePrice=g.FirstOrDefault().PurchasePrice,

            //                     ItemCode = a.ItemCode,
            //                     FromD = fromdate,
            //                     ToD = todate,

            //                 }).AsEnumerable()
            //.Select(x => new

            //    x.FromD,
            //    x.ToD,
            //        //x.ItemUnitName,
            //        x.ItemID,
            //    x.ItemName,
            //    x.purchaseprice,
            //        //amount=x.temp/ x.PurchasePrice,
            //        //x.amount,
            //        //// x.PurchaseQuantity,

            //        MaterialRequestedBy = (

            //                       (mc == null || mc == 0 || aa.MaterialCenter == mc)

            //                           Name = hh.FirstName
            //    x.ItemCode,
            //    daydiff,


            var data = final.ToList();
            recordsTotal = final.Count();
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
        public ActionResult PurchaseUsageDetails(long? itemID, string From, string To)
        {
            ViewBag.ItemName = db.Items.Where(a => a.ItemID == itemID).Select(a => a.ItemName).FirstOrDefault();
            ViewBag.fromdate = From;
            ViewBag.todate = To;

            ViewBag.Item = itemID;



            return View();
        }

        [HttpPost]
        public ActionResult GetPurchaseUsageDetails(long? Item, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
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

            var v = (from a in db.PurchaseEntrys
                     join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry into temp
                     from b in temp.DefaultIfEmpty()
                     join c in db.Suppliers on a.Supplier equals c.SupplierID into temp2
                     from c in temp2.DefaultIfEmpty()
                     join d in db.Employees on a.PECashier equals d.EmployeeId into temp3
                     from d in temp3.DefaultIfEmpty()
                     where (Item == 0 || b.Item == Item) &&

                         (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                         (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)
                     select new
                     {
                         d.FirstName,
                         a.PurchaseEntryId,
                         billNo = a.BillNo,
                         date = a.PEDate,
                         Supplier = c.SupplierName,






                     }).ToList()
        .Select(x => new
        {
            x.FirstName,
            x.PurchaseEntryId,
            x.billNo,
            x.date,
            x.Supplier,
            MaterialRequestedBy = (
                               from aa in db.PurchaseEntrys
                               join cc in db.PEItemss on aa.PurchaseEntryId equals cc.PurchaseEntry into conn
                               from cc in conn.DefaultIfEmpty()
                               join dd in db.ConvertTransactionss on aa.PurchaseEntryId equals dd.To into conn1
                               from dd in conn1.DefaultIfEmpty()
                               join ee in db.PurchaseOrders on dd.From equals ee.PurchaseOrderId into conn2
                               from ee in conn2.DefaultIfEmpty()
                               join ff in db.ConvertTransactionss on ee.PurchaseOrderId equals ff.To into conn3
                               from ff in conn3.DefaultIfEmpty()
                               join gg in db.MaterialRequisitions on ff.From equals gg.MaterialRequisitionId into conn4
                               from gg in conn4.DefaultIfEmpty()
                               join hh in db.Employees on gg.MRCashier equals hh.EmployeeId into conn5
                               from hh in conn5.DefaultIfEmpty()

                               where (aa.PurchaseEntryId == x.PurchaseEntryId) &&


                               (fromdate == "" || EF.Functions.DateDiffDay(aa.PEDate, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(aa.PEDate, tdate) >= 0)
                               select new
                               {

                                   Name = hh.FirstName
                               }).DefaultIfEmpty().ToList(),


        }).OrderBy(a => a.date);
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


        public decimal? GetItemStock(long? itemid)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            var ddmc = 0;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");


            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();
            if (data.Count() == 0)
            {
                return 0;
            }
            else
            {
                return data[0].ITotalQty;
            }

        }

        [HttpPost]
        public ActionResult AmountReceivablePayable2(long? ddlType, bool? pdc, string From, string To)
        {
            return RedirectToAction("ViewAmountReceivablePayable2", new { ddlType = ddlType, pdc = pdc, fromdate = From, todate = To });
        }

        [HttpPost]

        public ActionResult GetAmountReceivablePayable2(long? ddlType, bool? pdc, string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (pdc == true)
            {
                fromdate = "01-01-2000";
            }
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            string result = "";

            Common com = new Common();
            long[] accGp = null;
            //creditor
            if (ddlType == 0)
            {
                //sundry creditor
                var creparentid = new SqlParameter("@parentid", 14);
                var cregroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", creparentid).AsEnumerable().ToList();
                accGp = cregroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            else
            {
                //sundry debitor
                var debparentid = new SqlParameter("@parentid", 12);
                var debgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", debparentid).AsEnumerable().ToList();
                accGp = debgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }

            // The per-account let subqueries below use .AsEnumerable().DefaultIfEmpty(0).Sum(), which EF Core 10
            // cannot translate when the outer source is still server-side ("could not be translated"). Apply the
            // ledger filter on the server, then switch to client evaluation so each correlated sum runs as its own
            // translatable query.
            var v = (from a in db.Accountss.Where(a => accGp.Contains(a.Group)).AsEnumerable()
                     let CrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) &&

                      (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)

                     ).Select(b => b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     let DrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(b => b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     let CustomerID = db.Customers.Where(o => o.Accounts == a.AccountsID).Select(o => o.CustomerID).FirstOrDefault()
                     // accGp filter already applied on the server-side base source above.
                     select new
                     {
                         a.AccountsID,
                         CustomerID,
                         particulars = a.Name,
                         Credit = CrSum,
                         Debit = DrSum
                     }).AsEnumerable().Select(o => new
                     {
                         o.AccountsID,
                         o.CustomerID,

                         o.particulars,
                         o.Debit,
                         o.Credit,
                         DrCr = o.Debit > o.Credit ? "Dr" : "Cr",
                         Amount = o.Debit > o.Credit ? (o.Debit - o.Credit) : (o.Credit - o.Debit)
                     }).Where(a => a.Amount > 0);
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpGet]

        public ActionResult ViewAmountReceivablePayable2(long? ddlType, bool? pdc, string fromdate, string todate)
        {
            ViewBag.Type = ddlType;
            companySet();
            return View();
        }




        #region amount salesexecutive wise customer summery

        public ActionResult AmountReceivablePayable3()
        {
            companySet();
            return View();
        }

        [HttpPost]
        public ActionResult AmountReceivablePayable3(long? ddlType, bool? pdc, long? ddlEmployee)
        {
            return RedirectToAction("ViewAmountReceivablePayable3", new { ddlType = ddlType, pdc = pdc, emp = ddlEmployee });
        }

        //    //creditor
        //        //sundry creditor
        //        //sundry debitor


        //             let CrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && b.Purpose == "Receipt" && (pdc == true || b.Status == null)).Select(b => b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
        //             let DrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && b.Purpose == "Receipt" && (pdc == true || b.Status == null)).Select(b => b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()
        //             let CrSum2 = (decimal?)(from ac in db.AccountsTransactions
        //                                     (pdc == true || ac.Status == null)
        //                                     && se.SECashier == emp
        //                                     && ac.Account == a.AccountsID
        //                                         ac.Credit
        //                                     }).Sum(aaa => aaa.Credit) ?? 0
        //             let DrSum2 = (decimal?)(from ac in db.AccountsTransactions
        //                                     (pdc == true || ac.Status == null)
        //                                     && se.SECashier == emp
        //                                         && ac.Account == a.AccountsID
        //                                         ac.Debit
        //                                     }).Sum(aaa => aaa.Debit) ?? 0


        //             (emp == null || emp == 0 || c.SECashier == emp)
        //                 a.AccountsID,
        //                 particulars = a.Name,
        //                 Credit = CrSum2 + CrSum,
        //                 Debit = DrSum2 + DrSum,
        //                 b.CustomerID,
        //                 b.SalesPerson,
        //                 //emp=d.FirstName,
        //             }).Distinct().ToList().Select(o => new
        //                 o.AccountsID,
        //                 o.particulars,
        //                 o.Debit,
        //                 o.Credit,
        //                 DrCr = o.Debit > o.Credit ? "Dr" : "Cr",
        //                 Amount = o.Debit > o.Credit ? (o.Debit - o.Credit) : 0,
        //                 mobmodal = (

        //                       where (rrr.RelationID == o.CustomerID && rrr.RelationType == 0)
        //                           Num = co.Mobile,
        //                           Name = co.FirstName + "  " + co.LastName,
        //                           emails = co.EmailId,
        //                 emp = (from e in db.Employees
        //                        where e.EmployeeId == o.SalesPerson
        //                            e.FirstName
        //                        }).ToList()
        //                 //employee=o.emp,



        //        Content = result,
        //        ContentType = "application/json"

        [HttpPost]

        public ActionResult GetAmountReceivablePayable3(long? ddlType, bool? pdc, long? emp)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            string result = "";

            Common com = new Common();
            long[] accGp = null;
            //creditor
            if (ddlType == 0)
            {
                //sundry creditor
                var creparentid = new SqlParameter("@parentid", 14);
                var cregroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", creparentid).AsEnumerable().ToList();
                accGp = cregroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            else
            {
                //sundry debitor
                var debparentid = new SqlParameter("@parentid", 11);
                var debgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", debparentid).AsEnumerable().ToList();
                accGp = debgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }


            var v = (from a in db.Accountss
                     join b in db.Customers on a.AccountsID equals b.Accounts
                     join c in db.SalesEntrys on b.CustomerID equals c.Customer
                     join e in db.SEPayments on c.SalesEntryId equals e.SalesEntry
                     join d in db.Employees on c.SECashier equals d.EmployeeId


                     //   let CrSum = db.AccountsTransactions.Where(z => z.Account == a.AccountsID && c.SECashier == emp && (pdc == true || z.Status == null)).Select(b => b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     // let DrSum = db.AccountsTransactions.Where(z => z.Account == a.AccountsID && c.SECashier == emp && (pdc == true || z.Status == null)).Select(b => b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()

                     //let CrSum = db.AccountsTransactions.Where(z=>(z.Credit + z.Debit)==c.SEGrandTotal && c.SECashier== emp).Select(z=>z.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let DrSum = db.AccountsTransactions.Where(z => (z.Credit + z.Debit) == c.SEGrandTotal &&  c.SECashier == emp).Select(z => z.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()

                     where accGp.Contains(a.Group) &&
                      (emp == null || emp == 0 || c.SECashier == emp)
                     group new { a.AccountsID, b.CustomerID, a.Name, e.SEBillAmount, e.SEPaidAmount, b.SalesPerson } by new { a.AccountsID } into g
                     select new
                     {
                         AccountsID = g.Key.AccountsID,

                         particulars = g.FirstOrDefault().Name,
                         //amount =(from e in db.SEPayments
                         //         where e.SalesEntry==c.SalesEntryId
                         //             e.SEBillAmount


                         amount = g.Sum(k => k.SEBillAmount) - g.Sum(s => s.SEPaidAmount),
                         CustomerID = g.FirstOrDefault().CustomerID,
                         SalesPerson = g.FirstOrDefault().SalesPerson,
                         //emp=d.FirstName,
                     }).Distinct().ToList().Select(o => new
                     {

                         o.AccountsID,
                         o.CustomerID,
                         o.particulars,
                         o.amount,
                         //DrCr =o.Debit > o.Credit ? "Dr" : "Cr",
                         mobmodal = (
                               from co in db.Contacts
                               join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID

                               where (rrr.RelationID == o.CustomerID && rrr.RelationType == 0)
                               select new
                               {
                                   Num = co.Mobile,
                                   Name = co.FirstName + "  " + co.LastName,
                                   emails = co.EmailId,
                               }).ToList(),
                         emp = (from e in db.Employees
                                where e.EmployeeId == o.SalesPerson
                                select new
                                {
                                    e.FirstName
                                }).ToList()
                         //employee=o.emp,

                     }).Distinct().Where(a => a.amount > 0);
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            var data = v.Skip(skip).Take(pageSize).Distinct().ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        [HttpGet]

        public ActionResult ViewAmountReceivablePayable3(long? ddlType, bool? pdc, long? emp)
        {

            ViewBag.Type = ddlType;
            companySet();
            return View();
        }
        #endregion





































        #region amount Receivable/payable Report

        public ActionResult AmountReceivablePayable()
        {
            companySet();
            return View();
        }

        public ActionResult AmountReceivablePayableage()
        {
            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Amount Receivable Sales Man List")]
        public ActionResult AmountReceivablePayablesalesman()
        {
            companySet();
            return View();
        }

        [HttpPost]
        public ActionResult AmountReceivablePayable(long? ddlType, bool? pdc, long? ddlEmployee, string ondate)
        {
            return RedirectToAction("ViewAmountReceivablePayable", new { ddlType = ddlType, pdc = pdc, emp = ddlEmployee, ondate = ondate });
        }
        [HttpPost]
        public ActionResult AmountReceivablePayableage(long? ddlType, bool? pdc, long? ddlEmployee, string ondate)
        {
            return RedirectToAction("ViewAmountReceivablePayableage", new { ddlType = ddlType, pdc = pdc, emp = ddlEmployee, ondate = ondate });
        }
        public class remarkscust
        {
            public DateTime createdate { get; set; }
            public DateTime? nextdatetime { get; set; }
        }
        [HttpPost]

        public ActionResult GetAmountReceivablePayable(string nextdate, long? ddlType, bool? pdc, long? emp, string ondate, long? exp)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]")[0];
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            string result = "";
            int UpdatedDateExpiry = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "UpdatedDateExpiry").Select(o => o.TypeValue).FirstOrDefault());
            int NextDateExpiry = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "NextDateExpiry").Select(o => o.TypeValue).FirstOrDefault());


            DateTime? ondates = null;
            if (ondate != "")
            {
                ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
            }
            Common com = new Common();
            long[] accGp = null;
            //creditor
            if (ddlType == 0)
            {
                //sundry creditor
                var creparentid = new SqlParameter("@parentid", 14);
                var cregroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", creparentid).AsEnumerable().ToList();
                accGp = cregroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            else
            {
                //sundry debitor
                var debparentid = new SqlParameter("@parentid", 11);
                var debgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", debparentid).AsEnumerable().ToList();
                accGp = debgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            DateTime sedate = DateTime.Now.AddMonths(-6);
            DateTime datenow = DateTime.Now;
            DateTime ndate = DateTime.Now.AddYears(-2);
            var userid = User.Identity.GetUserId();
            var v2 = (from b in db.AccountsTransactions
                      join c in db.Customers on b.Account equals c.Accounts

                      where (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)
                      group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                      select new
                      {
                          Debit = g.Sum(o => o.Debit),
                          Credit = g.Sum(o => o.Credit),
                          Account = g.FirstOrDefault().Account

                      });
            var v = (from a in db.Accountss
                     join b in db.Customers on a.AccountsID equals b.Accounts
                     join c in db.Employees on b.SalesPerson equals c.EmployeeId into emps
                     from c in emps.DefaultIfEmpty()
                     join d in v2 on a.AccountsID equals d.Account
                     // (DateTime?) casts force EF Core to emit a plain nullable scalar subquery instead of
                     // COALESCE(subquery, '0001-01-01...'); that literal overflows SQL `datetime` (1753-9999) and
                     // 500s for every remark-less customer. Legacy value restored via ?? DateTime.MinValue below.
                     let createdDate = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.CreatedDate).FirstOrDefault()
                     let nexttime = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID && c.AddedUser == userid).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.nexttime).FirstOrDefault()
                     let nexttime1 = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.nexttime).FirstOrDefault()

                     //let CrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let DrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let aging = (from c in db.SalesEntrys
                     //             where d.SEPaidAmount == 0 && d.CustomerId == b.CustomerID


                     //             }).OrderBy(o => o.Days).Select(o => o.Days).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault()
                     where accGp.Contains(a.Group) &&
                     (emp == null || emp == 0 || b.SalesPerson == emp)
                     select new
                     {
                         b.CustomerID,
                         a.AccountsID,
                         particulars = a.Name,
                         Credit = d.Credit,
                         Debit = d.Debit,
                         days = "",
                         createdDate,
                         nexttime,
                         nexttime1,
                         expectedamount=(b.expectedamount==null)?0:b.expectedamount,
                         priority=(b.priority==null)?0:b.priority,

                         // nextfolloupdatetime=createdDate.nexttime,
                         FirstName = c.FirstName + " " + c.MiddleName + " " + c.LastName
                     }).AsEnumerable().Select(o => new
                     {
                         o.CustomerID,
                         o.AccountsID,
                         o.particulars,
                         o.Debit,
                         o.Credit,
                         o.priority,
                         o.expectedamount,
                         salesexecutive = (from a in db.SalesEntrys
                                           join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry
                                           join c in db.Employees on a.SECashier equals c.EmployeeId into grp
                                           from c in grp.DefaultIfEmpty()
                                           where a.Customer == o.CustomerID && b.SEPaidAmount != b.SEBillAmount
                                           && a.SEDate > sedate && a.SEDate <= ondates
                                           orderby a.SEDate descending
                                           select new
                                           {
                                               a.SalesEntryId,
                                               a.BillNo,
                                               execu = c.FirstName + " " + c.LastName,
                                               a.SEDate,
                                               balance = b.SEBillAmount - b.SEPaidAmount


                                           }).Take(5),



                         DrCr = o.Debit > o.Credit ? "Dr" : "Cr",
                         o.days,
                         createdDate = o.createdDate ?? DateTime.MinValue,
                         nextfolloupdatetime = ((o.nexttime ?? DateTime.MinValue) < ndate) ? (o.nexttime1 ?? DateTime.MinValue) : (o.nexttime ?? DateTime.MinValue),


                         o.FirstName,
                         Amount = o.Debit > o.Credit ? (o.Debit - o.Credit) : (o.Credit - o.Debit)
                     }).Where(a => a.Amount > 0);
            if (nextdate != "")
            {

                DateTime nextdates = DateTime.Parse(nextdate, new CultureInfo("en-GB"));
                v = v.Where(o => o.nextfolloupdatetime >= nextdates && o.nextfolloupdatetime < nextdates.AddDays(1));
            }
            if (exp == 1)
            {

                v = v.Where(o => (o.createdDate.AddMinutes(UpdatedDateExpiry)) < System.DateTime.Now);
                v = v.Where(o => o.createdDate > ndate);

            }

            if (exp == 3)
            {

                v = v.Where(o => (o.nextfolloupdatetime.AddMinutes(NextDateExpiry)) < System.DateTime.Now && o.nextfolloupdatetime > ndate);

            }
            if (exp == 2)
            {

                v = v.Where(o => o.createdDate < ndate);

            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            else
            {
                v = v.OrderByDescending(o => o.createdDate);
            }
            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        [HttpPost]

        public ActionResult GetAmountReceivablePayableage(string nextdate, long? ddlType, bool? pdc, long? emp, string ondate, long? exp)
        {
            db.SetCommandTimeOut(60 * 60);
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]")[0];
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            string result = "";
            int UpdatedDateExpiry = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "UpdatedDateExpiry").Select(o => o.TypeValue).FirstOrDefault());
            int NextDateExpiry = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "NextDateExpiry").Select(o => o.TypeValue).FirstOrDefault());


            DateTime? ondates = null;
            if (ondate != "")
            {
                ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
            }
            Common com = new Common();
            long[] accGp = null;
            //creditor
            if (ddlType == 0)
            {
                //sundry creditor
                var creparentid = new SqlParameter("@parentid", 14);
                var cregroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", creparentid).AsEnumerable().ToList();
                accGp = cregroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            else
            {
                //sundry debitor
                var debparentid = new SqlParameter("@parentid", 11);
                var debgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", debparentid).AsEnumerable().ToList();
                accGp = debgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            DateTime datenow = DateTime.Now;
            DateTime ndate = DateTime.Now.AddYears(-2);
            var userid = User.Identity.GetUserId();
            var v2 = (from b in db.AccountsTransactions
                      join c in db.Customers on b.Account equals c.Accounts

                      where (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)
                      group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                      select new
                      {
                          Debit = g.Sum(o => o.Debit),
                          Credit = g.Sum(o => o.Credit),
                          Account = g.FirstOrDefault().Account


                      });
            var onemonth = ondates.Value.AddMonths(-1);
            var twomonth = ondates.Value.AddMonths(-2);
            var threemonth = ondates.Value.AddMonths(-3);
            var totalreceivedmoney = (from b in db.AccountsTransactions
                                      join c in db.Customers on b.Account equals c.Accounts

                                      where

                                         b.Status == null
                                      group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                                      select new
                                      {
                                          Credit = g.Sum(o => o.Credit),

                                          Account = g.FirstOrDefault().Account


                                      });
            var vonemonth = (from b in db.AccountsTransactions
                             join c in db.Customers on b.Account equals c.Accounts

                             where
                               b.Date <= onemonth &&
                                b.Status == null
                             group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                             select new
                             {
                                 Debit = g.Sum(o => o.Debit),
                                 Credit = g.Sum(o => o.Credit),
                                 Account = g.FirstOrDefault().Account


                             });
            var vtwomonth = (from b in db.AccountsTransactions
                             join c in db.Customers on b.Account equals c.Accounts

                             where
                                b.Date <= twomonth &&
                         b.Status == null
                             group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                             select new
                             {
                                 Debit = g.Sum(o => o.Debit),
                                 Credit = g.Sum(o => o.Credit),
                                 Account = g.FirstOrDefault().Account


                             });
            var vthreemonths = (from b in db.AccountsTransactions
                                join c in db.Customers on b.Account equals c.Accounts

                                where
                                 b.Date <= threemonth &&
                          b.Status == null
                                group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                                select new
                                {
                                    Debit = g.Sum(o => o.Debit),
                                    Credit = g.Sum(o => o.Credit),
                                    Account = g.FirstOrDefault().Account


                                });
            var v = (from a in db.Accountss
                     join b in db.Customers on a.AccountsID equals b.Accounts
                     join c in db.Employees on b.SalesPerson equals c.EmployeeId into emps
                     from c in emps.DefaultIfEmpty()
                     join d in v2 on a.AccountsID equals d.Account
                     join e in vonemonth on a.AccountsID equals e.Account into one
                     from e in one.DefaultIfEmpty()
                     join f in vtwomonth on a.AccountsID equals f.Account into two
                     from f in two.DefaultIfEmpty()
                     join g in vthreemonths on a.AccountsID equals g.Account into three
                     from g in three.DefaultIfEmpty()
                     join recieved in totalreceivedmoney on a.AccountsID equals recieved.Account into four
                     from recieved in four.DefaultIfEmpty()
                         //  let createdDate = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID).OrderByDescending(c => c.CreatedDate).Select(o => o.CreatedDate).FirstOrDefault()
                         //  let nexttime = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID && c.AddedUser == userid).OrderByDescending(c => c.CreatedDate).Select(o => o.nexttime).FirstOrDefault()
                         //  let nexttime1 = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID).OrderByDescending(c => c.CreatedDate).Select(o => o.nexttime).FirstOrDefault()

                         //let CrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                         //let DrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()
                         //let aging = (from c in db.SalesEntrys
                         //             where d.SEPaidAmount == 0 && d.CustomerId == b.CustomerID


                         //             }).OrderBy(o => o.Days).Select(o => o.Days).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault()
                     where accGp.Contains(a.Group) &&
                     (emp == null || emp == 0 || b.SalesPerson == emp)
                     select new
                     {
                         b.CustomerID,
                         a.AccountsID,
                         particulars = a.Name,
                         Credit = d.Credit,
                         Debit = d.Debit,
                         onemothdebit = (e != null) ? e.Debit : 0,
                         onemothcredit = (e != null) ? e.Credit : 0,
                         twomonthdebit = (f != null) ? f.Debit : 0,
                         twomonthcredit = (f != null) ? f.Credit : 0,
                         threemonthdebit = (g != null) ? g.Debit : 0,
                         threemonthcredit = (g != null) ? g.Credit : 0,
                         totalrecieve = (recieved != null) ? recieved.Credit : 0,
                         days = "",


                         // nextfolloupdatetime=createdDate.nexttime,
                         FirstName = c.FirstName + " " + c.MiddleName + " " + c.LastName
                     }).AsEnumerable().Select(o => new
                     {
                         o.CustomerID,
                         o.AccountsID,
                         o.particulars,
                         o.Debit,
                         o.Credit,
                         DrCr = o.Debit > o.Credit ? "Dr" : "Cr",
                         o.days,




                         o.FirstName,
                         o.totalrecieve,
                         Amount = o.Debit > o.Credit ? (o.Debit - o.Credit) : (o.Credit - o.Debit),
                         Amountone = ((o.onemothdebit - o.totalrecieve) < 0) ? 0 : (o.onemothdebit - o.totalrecieve),
                         Amounttwo = ((o.twomonthdebit - o.totalrecieve) < 0) ? 0 : (o.onemothdebit - o.totalrecieve),
                         Amountthree = ((o.threemonthdebit - o.totalrecieve) < 0) ? 0 : (o.onemothdebit - o.totalrecieve),
                     }).AsEnumerable().Select(o => new
                     {
                         o.CustomerID,
                         o.AccountsID,
                         o.particulars,
                         o.Debit,
                         o.Credit,
                         o.DrCr,
                         o.days,




                         o.FirstName,
                         o.totalrecieve,
                         Amount = o.Amount,
                         Amountone = (o.Amountone - o.Amounttwo - o.Amountthree) < 0 ? 0 : (o.Amountone - o.Amounttwo - o.Amountthree),
                         Amounttwo = (o.Amounttwo - o.Amountthree) < 0 ? 0 : (o.Amounttwo - o.Amountthree),
                         o.Amountthree
                     }).Where(a => a.Amount > 0).ToList();



            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
            }

            var data = v.ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }


        public ActionResult GetAmountReceivablePayablessalesman(string nextdate, long? ddlType, bool? pdc, long? emp, string ondate, long? exp)
        {
            pdc = true;
            ondate = null;
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            string search = Request.Form.GetValues("search[value]")[0];
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            string result = "";
            int UpdatedDateExpiry = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "UpdatedDateExpiry").Select(o => o.TypeValue).FirstOrDefault());
            int NextDateExpiry = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "NextDateExpiry").Select(o => o.TypeValue).FirstOrDefault());


            DateTime? ondates = null;



            Common com = new Common();
            long[] accGp = null;
            //creditor
            if (ddlType == 0)
            {
                //sundry creditor
                var creparentid = new SqlParameter("@parentid", 14);
                var cregroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", creparentid).AsEnumerable().ToList();
                accGp = cregroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            else
            {
                //sundry debitor
                var debparentid = new SqlParameter("@parentid", 11);
                var debgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", debparentid).AsEnumerable().ToList();
                accGp = debgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            DateTime datenow = DateTime.Now;
            DateTime ndate = DateTime.Now.AddYears(-2);
            var userid = User.Identity.GetUserId();
            var v2 = (from b in db.AccountsTransactions
                      join c in db.Customers on b.Account equals c.Accounts

                      where (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)
                      group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                      select new
                      {
                          Debit = g.Sum(o => o.Debit),
                          Credit = g.Sum(o => o.Credit),
                          Account = g.FirstOrDefault().Account

                      });
            var v = (from a in db.Accountss
                     join b in db.Customers on a.AccountsID equals b.Accounts
                     join c in db.Employees on b.SalesPerson equals c.EmployeeId
                     join d in v2 on a.AccountsID equals d.Account

                     //let CrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let DrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let aging = (from c in db.SalesEntrys
                     //             where d.SEPaidAmount == 0 && d.CustomerId == b.CustomerID


                     //             }).OrderBy(o => o.Days).Select(o => o.Days).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault()
                     where accGp.Contains(a.Group) &&
                     (emp == null || emp == 0 || b.SalesPerson == emp)
                     select new
                     {
                         Credit = d.Credit,
                         Debit = d.Debit,



                         // nextfolloupdatetime=createdDate.nexttime,
                         FirstName = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                         c.EmployeeId,

                     }).AsEnumerable().Select(o => new
                     {
                         o.Debit,
                         o.Credit,
                         DrCr = o.Debit > o.Credit ? "Dr" : "Cr",
                         o.EmployeeId,

                         o.FirstName,
                         Amount = o.Debit > o.Credit ? (o.Debit - o.Credit) : (o.Credit - o.Debit)
                     }).Where(a => a.Amount > 0).GroupBy(o => o.FirstName).Select(o => new
                     {
                         FirstName = o.Key,
                         Amount = o.Sum(p => p.Amount),
                         empid = o.First().EmployeeId
                     });

            v = v.OrderByDescending(o => o.Amount);
            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpGet]

        public ActionResult ViewAmountReceivablePayable(long? ddlType, bool? pdc, long? emp, string ondate, long? exp)
        {
            ViewBag.Type = ddlType;
            ViewBag.salesman = "All";
            if (emp != null)
            {
                ViewBag.salesman = db.Employees.Where(o => o.EmployeeId == emp).Select(o => o.FirstName).SingleOrDefault();
            }
            companySet();
            return View();
        }
        [HttpGet]

        public ActionResult ViewAmountReceivablePayableage(long? ddlType, bool? pdc, long? emp, string ondate, long? exp)
        {
            ViewBag.Type = ddlType;
            ViewBag.salesman = "All";
            if (emp != null)
            {
                ViewBag.salesman = db.Employees.Where(o => o.EmployeeId == emp).Select(o => o.FirstName).SingleOrDefault();
            }
            companySet();
            return View();
        }

        //Get: View of Adding Remarks
        public ActionResult AddCustomerRemark(int? id)
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
            var CusRemark = new CustomerRemarkviewmodal
            {
                CustomerId = cus.CustomerID,
                nextfolloupdate = DateTime.Now.AddDays(1).ToString("dd-MM-yyyy"),
                nextfolloupdatetime = DateTime.Now.AddDays(0),
                mobnumber = (from co in db.Contacts
                             join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                             where (rrr.RelationID == cus.CustomerID && rrr.RelationType == 0)
                             select new
                             {

                                 Name = co.FirstName + " " + co.LastName + " : " + co.Mobile,

                             }).Select(o => o.Name).ToArray(),
                expectedamount=cus.expectedamount,
                priority= (cus.priority==null)?0:(int)cus.priority,



            };

            return PartialView(CusRemark);
        }

        //Saving of Remarks
        [HttpPost]
        public JsonResult AddCustomerRemark(CustomerRemarkviewmodal CusRemark)
        {
            Int64 CustomerId = CusRemark.CustomerId;

            if (ModelState.IsValid)
            {
                var customer = db.Customers.Find(CusRemark.CustomerId);
                var UserId = User.Identity.GetUserId();
                if (CusRemark.Remark != null && CusRemark.Remark != "" && UserId != null)
                {
                    Common com = new Common();

                    var Today = Convert.ToDateTime(System.DateTime.Now);

                    DateTime nextdate = DateTime.Parse(CusRemark.nextfolloupdate, new CultureInfo("en-GB"));

                    TimeSpan etime = ((DateTime)CusRemark.nextfolloupdatetime).TimeOfDay;

                    DateTime etimes = nextdate + etime;
                    CustomerRemark Obj = new CustomerRemark
                    {
                        CustomerId = CusRemark.CustomerId,
                        Remark = CusRemark.Remark,
                        AddedUser = UserId,
                        CreatedDate = Today,
                        nextdate = nextdate,
                        nexttime = etimes
                    };
                    db.CustomerRemarks.Add(Obj);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "MyReports", "CustomerRemarks", findip(), CustomerId, "Remarks Added Successfully..");
                    Success("Remark added successfully...", true);
                }
              if(CusRemark.expectedamount!=null)
                {
                    customer.expectedamount = CusRemark.expectedamount;
                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if (CusRemark.priority != null&&CusRemark.priority!=0)
                {
                   
                    customer.priority = CusRemark.priority;
                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();
                }

            }
            else
            {
                Danger("Failed to add Remarks...,please login again", true);
            }

            return Json(new { msg = "Success", status = true });

        }
        [HttpPost]
        public ActionResult GetContactDetails(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var v = (from co in db.Contacts
                     join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                     where (rrr.RelationID == CustomerId && rrr.RelationType == 0)
                     select new
                     {
                         Phone = co.Mobile,
                         Name = co.FirstName + " " + co.LastName,
                         emails = co.EmailId
                     }).ToList();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = v.Count();
            var data = v.ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        //Function to list the Remarks from table CustomerRemarks
        [HttpPost]
        public ActionResult GetAllRemarks(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.CustomerRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     where a.CustomerId == CustomerId && a.Remark != null
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.CustomerRemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remark,
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = v.Count();
            var data = v.ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        public ActionResult AddAccountRemark(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Accounts cus = db.Accountss.Find(id);

            if (cus == null)
            {
                return NotFound();
            }
            var AmcRemarks = new AddedRemarksvm
            {
                TransactionId = cus.AccountsID,
                TransactionType = "accountremark"


            };

            return PartialView(AmcRemarks);
        }

        [HttpPost]
        public JsonResult AddAccountRemark(AddedRemarksvm amcremarks)
        {
            Int64 amcid = amcremarks.TransactionId;
            var UserId=User.Identity.GetUserId();
            var Today = System.DateTime.Now;
            if (ModelState.IsValid)
            {
                if (amcremarks.Remarks != null)
                {
                    
                    AddedRemarks Obj = new AddedRemarks
                    {
                        TransactionId = amcremarks.TransactionId,
                        TransactionType = "accountremark",
                        Remarks = amcremarks.Remarks,
                        AddedUser = UserId,
                        CreatedDate = Today,
                       
                    };
                    db.AddedRemarks.Add(Obj);
                    db.SaveChanges();

                  

                   
                    com.addlog(LogTypes.Created, UserId, "accountremarks", "accountremarks", findip(), amcid, "accountremarks Remarks Added Successfully..");
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
        public ActionResult GetAllRemarksaccount(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.AddedRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
               
                     where a.TransactionId == CustomerId && a.TransactionType == "accountremark"
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.RemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remarks,
                   
                     }).ToList().Select(o => new
                     {
                         o.id,
                         o.CreatedDate,
                         o.EmpName,
                         o.Remarks,
                     
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
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
        public ActionResult GetTerms(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.Customers
                     where a.CustomerID == CustomerId
                     select new
                     {
                         termsandcondition = a.TermsandCondition,
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = v.Count();
            var data = v.ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        #endregion

        #region UnPaid Invoices Report
        public ActionResult UnPaidInvoices(long? particular)
        {
            var accname = db.Accountss.Where(a => a.AccountsID == particular).Select(a => a.Name).FirstOrDefault();
            ViewBag.Particulars = accname;
            ViewBag.Custo = db.Customers.Where(a => a.Accounts == particular).Select(a => a.CustomerID).FirstOrDefault();
            ViewBag.acntiD = db.Accountss.Where(a => a.AccountsID == particular).Select(a => a.AccountsID).FirstOrDefault();
            companySet();
            return View();
        }
        public ActionResult Ledger2(long? particular)
        {

            var accname = db.Accountss.Where(a => a.AccountsID == particular).Select(a => new
            {

                id = a.AccountsID,
                name = a.Name
            }).ToList();
            ViewBag.Account = QkSelect.List(accname, "id", "name");

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();





            return View();
        }
        public ActionResult Ledger3(long? particular)
        {

            var accname = db.Accountss.Where(a => a.AccountsID == particular).Select(a => new
            {

                id = a.AccountsID,
                name = a.Name
            }).ToList();
            ViewBag.Account = QkSelect.List(accname, "id", "name");

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                    .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            _FinancialYear();





            return View();
        }



        public ActionResult getUnPaidInvoices(long? particular)
        {
            var UserId = User.Identity.GetUserId();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;





            var sale = (from a in db.SalesEntrys
                        join b in db.Customers on a.Customer equals b.CustomerID
                        join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                        where b.Accounts == particular && a.CustomerType == CustomerType.Customer
                        select new
                        {
                            salesid = a.SalesEntryId,
                            Date = (DateTime?)a.SEDate,
                            Invoice = a.BillNo,
                            BalanceAmt = ((decimal?)a.SEGrandTotal ?? 0) - ((decimal?)c.SEPaidAmount ?? 0),
                        }).Where(a => a.BalanceAmt > 0).ToList();

            var sreturn = (from a in db.SalesReturns
                           join b in db.Customers on a.Customer equals b.CustomerID
                           join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                           where b.Accounts == particular && a.CustomerType == CustomerType.Customer
                           select new
                           {
                               salesid = (long)0,
                               Date = (DateTime?)a.SRDate,
                               Invoice = a.BillNo,
                               BalanceAmt = ((decimal?)a.SRGrandTotal ?? 0) - ((decimal?)c.SReturnAmount ?? 0),
                           }).Where(a => a.BalanceAmt > 0).ToList();

            var purchase = (from a in db.PurchaseEntrys
                            join b in db.Suppliers on a.Supplier equals b.SupplierID
                            join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                            where b.Accounts == particular && a.SupplierType == SupplierType.CreditSale
                            select new
                            {
                                salesid = (long)0,
                                Date = (DateTime?)a.PEDate,
                                Invoice = a.BillNo,
                                BalanceAmt = ((decimal?)a.PEGrandTotal ?? 0) - ((decimal?)c.PEPaidAmount ?? 0),
                            }).Where(a => a.BalanceAmt > 0).ToList();

            var preturn = (from a in db.PurchaseReturns
                           join b in db.Suppliers on a.Supplier equals b.SupplierID
                           join c in db.PRPayments on a.PurchaseReturnId equals c.PurchaseReturnId
                           where b.Accounts == particular && a.SupplierType == SupplierType.CreditSale
                           select new
                           {
                               salesid = (long)0,
                               Date = (DateTime?)a.PRDate,
                               Invoice = a.BillNo,
                               BalanceAmt = ((decimal?)a.PRGrandTotal ?? 0) - ((decimal?)c.PReturnAmount ?? 0),
                           }).Where(a => a.BalanceAmt > 0).ToList();

            var openbal = (from a in db.AccountsTransactions
                           where a.Account == particular && a.Purpose == "Opening Balance"
                           select new
                           {
                               salesid = (long)0,
                               Date = (DateTime?)a.CreatedDate,
                               Invoice = "Opening Balance",
                               BalanceAmt = a.Debit > a.Credit ? (a.Debit - a.Credit) : (a.Credit - a.Debit),
                           }).ToList();

            var full = sale.Union(purchase);
            full = full.Union(sreturn);
            full = full.Union(preturn);
            full = full.Union(openbal);

            var data = full.Skip(skip).Take(pageSize).ToList();
            recordsTotal = full.Count();

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
        #endregion

        #region Hire Notification List
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Report HireReturn")]
        public ActionResult HireNotification()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();


            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.Customer = OpAll;

            companySet();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            return View();
        }
        // [QkAuthorize(Roles = "Dev,Report HireReturn")]
        public ActionResult GetHireNotification(string BillNo, long? customer, string fromdate, string todate, long? Htype)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

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
            var v = (from b in db.SalesEntrys
                     join c in db.Customers on b.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = b.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join e in db.MCs on b.MaterialCenter equals e.MCId into mcs
                     from e in mcs.DefaultIfEmpty()
                     join d in db.HireReturns on h.HireDetailId equals d.HrNo into hi
                     from d in hi.DefaultIfEmpty()
                     join f in db.ConvertTransactionss on b.SalesEntryId equals f.From into fi
                     from f in fi.DefaultIfEmpty()
                     where (BillNo == "" || b.BillNo == BillNo)
                     && (b.SalesEntryId != f.From)
                       && (customer == 0 || b.Customer == customer) && (Htype == null || Htype == 0 || h.HireType == Htype) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(h.StartDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(h.StartDate, tdate) >= 0)

                     select new
                     {
                         HireDetailId = h.HireDetailId,
                         HireType = h.HireType,
                         BillNo = b.BillNo,
                         Startdate = h.StartDate,
                         Enddate = h.EndDate,
                         GrandTotal = b.SEGrandTotal,
                         Customer = c.CustomerName,
                         User = db.Users.Where(s => s.Id == b.CreatedBy).Select(s => s.UserName).FirstOrDefault()
                     }).OrderBy(a => a.Startdate);

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

        #endregion

        #region Purchase Order Outstanding

        [QkAuthorize(Roles = "Dev,POrder Outstanding")]
        public ActionResult POrderOutstanding()
        {
            ViewBag.Item = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.Supplier = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            return View();
        }

        [HttpPost]
        public ActionResult POrderOutstanding(long? ddlItem, long? ddlSupplier, string From, string To)
        {
            return RedirectToAction("ViewPOrderOutstanding", new { item = ddlItem, supp = ddlSupplier, fromdate = From, todate = To });
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,POrder Outstanding")]
        public ActionResult GetPOrderOutstanding(long? item, long? supp, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            string result = "";

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime datenow = DateTime.Now;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }


            var v = (from a in db.PurchaseOrderItems
                     join b in db.Items on a.Item equals b.ItemID into itm
                     from b in itm.DefaultIfEmpty()
                     join z in db.PurchaseOrders on a.PurchaseOrder equals z.PurchaseOrderId into po
                     from z in po.DefaultIfEmpty()
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()

                     join e in db.ConvertTransactionss on a.PurchaseOrder equals e.From into porder
                     from e in porder.DefaultIfEmpty()


                     join f in db.PurchaseOrderItems on new { f1 = b.ItemID, f2 = db.PurchaseOrderItems.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.POItemId).Max() }
                     equals new { f1 = f.Item, f2 = f.POItemId } into pur
                     from f in pur.DefaultIfEmpty()
                     where (fromdate == "" || EF.Functions.DateDiffDay(z.PODate, fdate) <= 0) &&
                                 (todate == "" || EF.Functions.DateDiffDay(z.PODate, tdate) >= 0)
                     orderby b.ItemID ascending
                     select new


                     {
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         ItemWithCode = b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         b.MinStock,
                         b.KeepStock,
                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                         PEntry = e != null ? e.To : 0,
                         Invoice = db.PurchaseOrders.Where(p => p.PurchaseOrderId == a.PurchaseOrder).Select(p => p.BillNo).FirstOrDefault(),
                         Date = db.PurchaseOrders.Where(p => p.PurchaseOrderId == a.PurchaseOrder).Select(p => p.PODate).FirstOrDefault(),
                     }).Distinct();
            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();

            var mydata =
               (from b in data

                let PriPOrder = (from a in db.PurchaseOrderItems
                                 join c in db.PurchaseOrders on a.PurchaseOrder equals c.PurchaseOrderId
                                 where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                 select new
                                 {
                                     a.ItemQuantity
                                 }).ToList().Sum(x => x.ItemQuantity)

                let SubPOrder = (from a in db.PurchaseOrderItems
                                 join c in db.PurchaseOrders on a.PurchaseOrder equals c.PurchaseOrderId
                                 where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                 select new
                                 {
                                     a.ItemQuantity
                                 }).ToList().Sum(x => x.ItemQuantity)

                let PriPurchase = (decimal?)(from a in db.PEItemss
                                             join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                             where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                             && (b.PEntry != 0 && a.PurchaseEntry == b.PEntry)
                                             select new
                                             {
                                                 a.ItemQuantity
                                             }).Sum(x => x.ItemQuantity) ?? 0

                let SubPurchase = (decimal?)(from a in db.PEItemss
                                             join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                             where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                             && (b.PEntry != 0 && a.PurchaseEntry == b.PEntry)
                                             select new
                                             {
                                                 a.ItemQuantity
                                             }).Sum(x => x.ItemQuantity) ?? 0



                select new
                {
                    b.Invoice,
                    b.Date,
                    b.ItemID,
                    b.ItemCode,
                    b.ItemName,
                    b.ItemWithCode,
                    b.ItemUnitID,
                    b.SubUnitId,
                    PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                    SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                    MinStock = (b.MinStock != null) ? b.MinStock : 0,
                    b.ConFactor,
                    stockable = b.KeepStock,

                    PriPOrder = (PriPOrder + (int)(SubPOrder / b.ConFactor)),
                    SubPOrder = (SubPOrder % b.ConFactor),

                    PriPurchase = (PriPurchase + (int)(SubPurchase / b.ConFactor)),
                    SubPurchase = (SubPurchase % b.ConFactor),

                    total = ((PriPOrder * b.ConFactor) + SubPOrder),
                    ptotal = (((PriPOrder - PriPurchase) * b.ConFactor) + (SubPOrder - SubPurchase)),

                }).OrderBy(a => a.ItemName).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,POrder Outstanding")]
        public ActionResult ViewPOrderOutstanding(long? item, long? supp, string fromdate, string todate)
        {
            ViewBag.Supplier = supp != 0 ? db.Suppliers.Where(a => a.SupplierID == supp).Select(a => a.SupplierName).FirstOrDefault() : "All";
            ViewBag.From = fromdate;
            ViewBag.To = todate;


            companySet();
            return View();
        }

        #endregion

        #region sales order outstanding

        [QkAuthorize(Roles = "Dev,SOrder Outstanding")]
        public ActionResult SOrderOutstanding()
        {
            ViewBag.Item = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.Cust = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            return View();
        }

        [HttpPost]
        public ActionResult SOrderOutstanding(long? ddlItem, long? ddlCustomer, string From, string To)
        {
            return RedirectToAction("ViewSOrderOutstanding", new { item = ddlItem, cust = ddlCustomer, fromdate = From, todate = To });
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,SOrder Outstanding")]
        public ActionResult GetSOrderOutstanding(long? item, long? cust, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            string result = "";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime datenow = DateTime.Now;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }


            var v = (from a in db.SalesOrderItems
                     join z in db.SalesOrders on a.SalesOrder equals z.SalesOrderId into so
                     from z in so.DefaultIfEmpty()
                     join b in db.Items on a.Item equals b.ItemID into itm
                     from b in itm.DefaultIfEmpty()
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()

                     join e in db.ConvertTransactionss on a.SalesOrder equals e.From into sorder
                     from e in sorder.DefaultIfEmpty()

                     join f in db.SalesOrderItems on new { f1 = b.ItemID, f2 = db.SalesOrderItems.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.SalesOrderItemId).Max() }
                     equals new { f1 = f.Item, f2 = f.SalesOrderItemId } into pur
                     from f in pur.DefaultIfEmpty()
                     orderby b.ItemID ascending
                     where (fromdate == "" || EF.Functions.DateDiffDay(z.SODate, fdate) <= 0) &&
                                 (todate == "" || EF.Functions.DateDiffDay(z.SODate, tdate) >= 0)
                     select new
                     {
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         ItemWithCode = b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         b.MinStock,
                         b.KeepStock,
                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                         SEntry = e != null ? e.To : 0,
                         Invoice = db.SalesOrders.Where(p => p.SalesOrderId == a.SalesOrder).Select(p => p.BillNo).FirstOrDefault(),
                         Date = db.SalesOrders.Where(p => p.SalesOrderId == a.SalesOrder).Select(p => p.SODate).FirstOrDefault(),
                     }).Distinct();
            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();

            var mydata =
               (from b in data

                let PriSOrder = (from a in db.SalesOrderItems
                                 join c in db.SalesOrders on a.SalesOrder equals c.SalesOrderId
                                 where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                 select new
                                 {
                                     a.ItemQuantity
                                 }).ToList().Sum(x => x.ItemQuantity)

                let SubSOrder = (from a in db.SalesOrderItems
                                 join c in db.SalesOrders on a.SalesOrder equals c.SalesOrderId
                                 where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                 select new
                                 {
                                     a.ItemQuantity
                                 }).ToList().Sum(x => x.ItemQuantity)

                let PriSale = (decimal?)(from a in db.SEItemss
                                         join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                         where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                         && (b.SEntry != 0 && a.SalesEntry == b.SEntry)
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).Sum(x => x.ItemQuantity) ?? 0

                let SubSale = (decimal?)(from a in db.SEItemss
                                         join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                         where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                         && (b.SEntry != 0 && a.SalesEntry == b.SEntry)
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).Sum(x => x.ItemQuantity) ?? 0

                select new
                {
                    b.Invoice,
                    b.Date,
                    b.ItemID,
                    b.ItemCode,
                    b.ItemName,
                    b.ItemWithCode,
                    b.ItemUnitID,
                    b.SubUnitId,
                    PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                    SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                    MinStock = (b.MinStock != null) ? b.MinStock : 0,
                    b.ConFactor,
                    stockable = b.KeepStock,

                    PriSOrder = (PriSOrder + (int)(SubSOrder / b.ConFactor)),
                    SubSOrder = (SubSOrder % b.ConFactor),

                    PriSale = (PriSale + (int)(SubSale / b.ConFactor)),
                    SubSale = (SubSale % b.ConFactor),

                    total = ((PriSOrder * b.ConFactor) + SubSOrder),
                    ptotal = (((PriSOrder - PriSale) * b.ConFactor) + (SubSOrder - SubSale)),

                }).OrderBy(a => a.ItemName).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,SOrder Outstanding")]
        public ActionResult ViewSOrderOutstanding(long? item, long? cust, string fromdate, string todate)
        {
            ViewBag.Customer = cust != 0 ? db.Customers.Where(a => a.CustomerID == cust).Select(a => a.CustomerName).FirstOrDefault() : "All";
            ViewBag.From = fromdate;
            ViewBag.To = todate;


            companySet();
            return View();
        }
        #endregion

        #region SaleRet Item CatWise
        //[QkAuthorize(Roles = "Dev,SalesReturn ItemWise")]
        public ActionResult SalesReturnItemWise()
        {
            var OptAll = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);

            ViewBag.Category = OptAll;
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
             new List<SelectListItem>
             {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
             }, "Value", "Text", 0);
            return View();
        }
        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,SalesReturn ItemWise")]
        public ActionResult SalesReturnItemWiseR(long? ddlItemCategory, long? ddmc, string From, string To)
        {
            var Crtime = System.DateTime.Now;
            var Crby = User.Identity.Name;
            var CrDate = DateTime.Parse(From, new CultureInfo("en-GB"));
            var UserId = User.Identity.GetUserId();
            var MCName = db.MCs.Where(x => x.MCId == ddmc).Select(x => x.MCName).FirstOrDefault();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            string cat = "All";
            if (ddlItemCategory != 0)
            {
                cat = db.ItemCategorys.Where(a => a.ItemCategoryID == ddlItemCategory).Select(a => a.ItemCategoryName).FirstOrDefault();
            }

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (From != "")
            {
                fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }
            if (To != "")
            {
                tdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }
            var SaleRetItem = (from a in db.Items
                               join b in db.SRItemss on a.ItemID equals b.Item
                               join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into punit
                               from e in punit.DefaultIfEmpty()
                               join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into sunit
                               from f in sunit.DefaultIfEmpty()
                               join g in db.ItemCategorys on a.ItemCategoryID equals g.ItemCategoryID into third
                               from g in third.DefaultIfEmpty()
                               where (ddlItemCategory == 0 || a.ItemCategoryID == ddlItemCategory)
                               select new
                               {
                                   a.ItemID,
                                   Item = a.ItemName,
                                   ItemName = a.ItemCode + "-" + a.ItemName,
                                   PriUnit = e.ItemUnitName,
                                   SubUnit = f.ItemUnitName,
                                   ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                                   a.ItemUnitID,
                                   a.SubUnitId,
                                   a.SellingPrice,
                                   g.ItemCategoryName,
                                   PriSaleQty = (int?)(from i in db.SRItemss
                                                       join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                       where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                       (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0)
                                                       && (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID)
                                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                       //  group i by i.ItemId into g
                                                       select new
                                                       {
                                                           i.ItemQuantity,
                                                       }).Sum(x => x.ItemQuantity) ?? 0,

                                   SubSaleQty = (int?)(from i in db.SRItemss
                                                       join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                       where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                       (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0)
                                                       && (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId && a.ItemUnitID != a.SubUnitId)
                                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                       // group i by i.ItemId into g
                                                       select new
                                                       {
                                                           i.ItemQuantity,
                                                       }).Sum(x => x.ItemQuantity) ?? 0,
                               }).Distinct().AsEnumerable().Select(o => new
                               {
                                   o.ItemID,
                                   o.Item,
                                   o.ItemUnitID,
                                   o.SubUnitId,
                                   PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                   SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                   o.ConFactor,
                                   o.PriSaleQty,
                                   o.SubSaleQty,
                                   o.ItemName,
                                   Quantity = (o.PriSaleQty * o.ConFactor) + o.SubSaleQty,
                                   Category = o.ItemCategoryName,
                               }).OrderBy(a => a.Item).Where(a => a.Quantity > 0).ToList();


            return new QuickSoft.Models.LegacyJsonResult { Data = new { SaleRetdata = SaleRetItem, Date = CrDate, By = Crby, Time = Crtime, MC = MCName, From = fdate, To = tdate, Category = cat } };
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn ItemWise")]
        public ActionResult SalesReturnItemWise(long? ddlItemCategory, long? ddmc, string From, string To)
        {
            companySet();
            DailySummaryViewModel vmodel = new DailySummaryViewModel();
            vmodel.time = System.DateTime.Now;
            vmodel.by = User.Identity.Name;
            vmodel.Date = DateTime.Parse(From, new CultureInfo("en-GB"));
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (From != "")
            {
                fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }
            if (To != "")
            {
                tdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }
            var SaleRetItem = (from a in db.Items
                               join b in db.SRItemss on a.ItemID equals b.Item
                               join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into punit
                               from e in punit.DefaultIfEmpty()
                               join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into sunit
                               from f in sunit.DefaultIfEmpty()
                               join g in db.ItemCategorys on a.ItemCategoryID equals g.ItemCategoryID into third
                               from g in third.DefaultIfEmpty()
                               where (ddlItemCategory == 0 || a.ItemCategoryID == ddlItemCategory)
                               select new
                               {
                                   a.ItemID,
                                   a.ItemName,
                                   Item = a.ItemCode + "-" + a.ItemName,
                                   PriUnit = e.ItemUnitName,
                                   SubUnit = f.ItemUnitName,
                                   ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                                   a.ItemUnitID,
                                   a.SubUnitId,
                                   a.SellingPrice,
                                   g.ItemCategoryName,
                                   PriSaleQty = (int?)(from i in db.SRItemss
                                                       join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                       where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                       (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0)
                                                       && (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID)
                                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                       //group i by i.ItemId into g
                                                       select new
                                                       {
                                                           i.ItemQuantity,
                                                       }).Sum(x => x.ItemQuantity) ?? 0,

                                   SubSaleQty = (int?)(from i in db.SRItemss
                                                       join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                       where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                       (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0)
                                                       && (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId && a.ItemUnitID != a.SubUnitId)
                                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                       // group i by i.ItemId into g
                                                       select new
                                                       {
                                                           i.ItemQuantity,
                                                       }).Sum(x => x.ItemQuantity) ?? 0,
                               }).Distinct().AsEnumerable().Select(o => new
                               {
                                   o.ItemID,
                                   o.Item,
                                   o.ItemUnitID,
                                   o.SubUnitId,
                                   PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                   SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                   o.ConFactor,
                                   o.PriSaleQty,
                                   o.SubSaleQty,
                                   o.ItemName,
                                   Qty = (int)(o.SubSaleQty / o.ConFactor) + o.PriSaleQty,
                                   Category = o.ItemCategoryName
                               }).OrderBy(a => a.ItemName).ToList();
            if (SaleRetItem.Any())
            {
                vmodel.SaleRetItemCatWise = SaleRetItem.Where(x => x.Qty != 0).Select(b => new SaleItemCategoryWise
                {
                    PUName = b.PriUnit != null ? b.PriUnit : (b.SubUnit != null) ? b.SubUnit : "",
                    SUName = b.SubUnit != null ? b.SubUnit : null,
                    Name = b.ItemName,
                    Category = b.Category,
                    PriTotal = b.PriSaleQty,
                    SubTotal = b.SubSaleQty,
                    Quantity = (int)(b.SubSaleQty / b.ConFactor) + b.PriSaleQty,
                    Confactor = b.ConFactor
                }).OrderBy(a => a.Category).ToList();
            }
            vmodel.from = From;
            vmodel.to = To;
            vmodel.category = (ddlItemCategory != 0) ? (db.ItemCategorys.Where(x => x.ItemCategoryID == ddlItemCategory).Select(y => y.ItemCategoryName).FirstOrDefault()) : "All";
            return View(vmodel);
        }
        #endregion

        #region Production
        [HttpGet]
        public ActionResult Production(long? id)
        {
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
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.Branch = QkSelect.List(Bnch, "Id", "Name");


            ViewBag.getProj = QkSelect.List(
                              new List<SelectListItem>
                              {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                              }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            companySet();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,Report Production")]
        public ActionResult GetProduction(string vno, long? mc, long? branch, string fromdate, string todate, long? project, long? task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            var v = (from a in db.Productions
                     join b in db.GeneratedItem on a.ProductionId equals b.Production into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.MCs on a.MaterialCenter equals c.MCId into payt
                     from c in payt.DefaultIfEmpty()
                     join d in db.Branchs on a.Branch equals d.BranchID into brnch
                     from d in brnch.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                      (mc == 0 || mc == null || a.MaterialCenter == mc) &&
                      (branch == 0 || branch == null || a.Branch == branch) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                     (project == 0 || project == null || a.Project == project) &&
                     (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         a.ProductionId,
                         Date = a.PEDate,
                         c.MCName,
                         d.BranchName,
                         a.CreatedDate
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.ProductionId,
                         o.Date,
                         MC = o.MCName,
                         Branch = o.BranchName,
                         o.CreatedDate
                     }).Distinct().OrderBy(a => a.Date);

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

        [HttpGet]
        public ActionResult ProductionSummary(long? id)
        {
            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            var mcs = db.BillOfMaterials.Select(s => new
            {
                Id = s.BOMId,
                Name = s.BOMName
            }).ToList();
            ViewBag.BOM = QkSelect.List(mcs, "Id", "Name");


            var Bnch = (from a in db.BillOfMaterials
                        join b in db.Items on a.ItemId equals b.ItemID into payf
                        from b in payf.DefaultIfEmpty()
                        select new
                        {
                            Id = a.ItemId,
                            Name = b.ItemName
                        }).Distinct().ToList();
            ViewBag.generated = QkSelect.List(Bnch, "Id", "Name");
            var mcchkk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchkk != null)
            {
                var mcss = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcss, "Id", "Name");
            }
            else
            {
                var mcss = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcss, "Id", "Name");
            }

            companySet();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,Report Production")]
        public ActionResult GetProductionSummary(long? BOM, long? generated, string fromdate, string todate,long? mc)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            var v = (from a in db.Productions
                     join b in db.GeneratedItem on a.ProductionId equals b.Production into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Items on b.Item equals c.ItemID into payt
                     from c in payt.DefaultIfEmpty()
                     join d in db.BillOfMaterials on b.BOM equals d.BOMId into brnch
                     from d in brnch.DefaultIfEmpty()
                     where (BOM == 0 || BOM == null || b.BOM == BOM) &&
                      (generated == 0 || generated == null || b.Item == generated) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                     (mc==null||mc==0||a.MaterialCenter ==mc)
                     select new
                     {
                         a.VoucherNo,
                         a.ProductionId,
                         Date = a.PEDate,
                         c.ItemName,
                         d.BOMName,
                         b.Qty,
                         b.Price,
                         b.Amount,
                         singlepcost= d.Labourcost,
                         productioncost =d.Expense *b.Qty,
                         labourcost=d.Labourcost*b.Qty,
                         materialcost=d.meterialcost *b.Qty,
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.ProductionId,
                         o.Date,
                        o.singlepcost,
                         generated = o.ItemName,
                         o.BOMName,
                         o.Qty,
                         o.Price,
                         o.Amount,
                         o.productioncost,
                         o.labourcost,
                         o.materialcost,
                         totalcost = o.productioncost+o.labourcost+o.materialcost
                     }).Distinct().OrderBy(a => a.Date);

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
        #endregion

        #region Unassemble
        [HttpGet]
        public ActionResult Unassemble(long? id)
        {
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
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.Branch = QkSelect.List(Bnch, "Id", "Name");
            companySet();

            ViewBag.getProj = QkSelect.List(
                              new List<SelectListItem>
                              {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                              }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            return View();
        }
        //[QkAuthorize(Roles = "Dev,Report Unassemble")]
        public ActionResult GetUnassemble(string vno, long? mc, long? branch, string fromdate, string todate, long? project, long? task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            var v = (from a in db.Unassembles
                     join b in db.ConsumedItem on a.UnassembleId equals b.Unassemble
                     join c in db.MCs on a.MaterialCenter equals c.MCId into payt
                     from c in payt.DefaultIfEmpty()
                     join d in db.Branchs on a.Branch equals d.BranchID into brnch
                     from d in brnch.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                      (mc == 0 || mc == null || a.MaterialCenter == mc) &&
                      (branch == 0 || branch == null || a.Branch == branch) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                     (project == 0 || project == null || a.Project == project) &&
                     (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         a.UnassembleId,
                         Date = a.PEDate,
                         c.MCName,
                         d.BranchName,
                         a.CreatedDate
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.UnassembleId,
                         o.Date,
                         MC = o.MCName,
                         Branch = o.BranchName,
                         o.CreatedDate
                     }).Distinct().OrderBy(a => a.Date);

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

        #endregion

        #region Material Receive Note
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report MRNote")]
        public ActionResult MRNote()
        {
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);


            ViewBag.Employee = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            companySet();
            return View();
        }
        [HttpPost]
        public ActionResult GetMRNote(string vno, long? supplier, long? employee, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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

            var v = (from a in db.MaterialReceiveNotes
                     join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Employees on a.Cashier equals c.EmployeeId into emp
                     from c in emp.DefaultIfEmpty()
                     join e in db.Branchs on a.Branch equals e.BranchID
                     join f in db.Accountss on b.Accounts equals f.AccountsID into acc
                     from f in acc.DefaultIfEmpty()
                     where (vno == "" || a.BillNo == vno) &&
                        (supplier == 0 || a.Supplier == supplier) &&
                        (employee == 0 || a.Cashier == employee) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.MRDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.MRDate, tdate) >= 0)
                     select new
                     {
                         a.MRId,
                         a.MRNo,
                         a.BillNo,
                         a.MRDate,
                         a.MRNItems,
                         a.MRNQuantity,
                         a.RequestedDate,
                         TaxRegNo = f.TRN,
                         Supplier = b.SupplierName,
                         EmpName = c.FirstName + " " + c.LastName,
                         User = db.Users.Where(s => s.Id == a.CreatedUserId).Select(s => s.UserName).FirstOrDefault(),
                         a.CreatedDate
                     }).OrderBy(a => a.MRDate).ThenBy(a => a.CreatedDate);

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
        #endregion

        #region Material Requisition
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report MaterialRequisition")]
        public ActionResult MaterialRequisition()
        {
            ViewBag.Item = QkSelect.List(
  new List<SelectListItem>
  {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
  }, "Value", "Text", 0);
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);


            ViewBag.Employee = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            ViewBag.getProj = QkSelect.List(
                             new List<SelectListItem>
                             {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);


            companySet();
            return View();
        }


        [HttpPost]
        public ActionResult GetMaterialRequisition(string vno, long? supplier, long? employee, string fromdate, string todate, long? ProjectName, long? Task, string RequestStat)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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
            var v = (from a in db.MaterialRequisitions
                     join c in db.Employees on a.MRCashier equals c.EmployeeId into emp
                     from c in emp.DefaultIfEmpty()
                     join e in db.Branchs on a.Branch equals e.BranchID into bran
                     from e in bran.DefaultIfEmpty()
                     join d in db.Projects on a.Project equals d.ProjectId into prj
                     from d in prj.DefaultIfEmpty()
                     join f in db.ProTasks on a.ProTask equals f.ProTaskId into task
                     from f in task.DefaultIfEmpty()

                     where (vno == "" || a.BillNo == vno) &&

                        (employee == 0 || a.MRCashier == employee) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.MRDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.MRDate, tdate) >= 0) &&
                        (ProjectName == 0 || ProjectName == null || a.Project == ProjectName) &&
                   (Task == 0 || Task == null || a.ProTask == Task) &&
                   (RequestStat == "" || RequestStat == "All" || a.RequestStatus == RequestStat) &&
                   (supplier == 0 || supplier == null || a.SupplierId == supplier)
                     select new
                     {
                         a.MaterialRequisitionId,
                         a.MRNo,
                         a.BillNo,
                         a.MRDate,
                         a.MRItems,
                         a.MRItemQuantity,

                         EmpName = c.FirstName + " " + c.LastName,
                         User = db.Users.Where(s => s.Id == a.CreatedUserId).Select(s => s.UserName).FirstOrDefault(),
                         a.MRValidity,
                         a.MRCreatedDate,
                         ProjectName = (d.ProjectName != null && d.ProjectName != "") ? d.ProCode + "-" + d.ProjectName : "",
                         Task = (f.TaskName != null && f.TaskName != "") ? f.TaskCode + "-" + f.TaskName : "",
                     }).OrderBy(a => a.MRDate).ThenBy(a => a.MRCreatedDate);

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
        #endregion

        #region Project
        #region ProjectProfitability
        [HttpGet]
        public ActionResult ProjectProfitability(long? id)
        {
            var UserId = User.Identity.GetUserId();

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
                .Select(s => new
                {
                    ID = s.ProjectId,
                    Name = s.ProjectName
                })
                .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");


            companySet();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,Report Production")]
        public ActionResult GetProfitability(long? customer, string fromdate, string todate, long? project)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? frm = null;
            DateTime? to = null;
            if (fromdate != "")
            {
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                frm = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            var v = db.Projects.Where(a => (customer == null || a.Customer == customer) && (project == null || project == 0 || a.ProjectId == project)).ToList();
            var ProjectList = v.Select(x => x.ProjectId).ToList();

            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => /*(a.Parent == 0) &&*/ ((a.AccountsGroupID == 7) || (a.Parent == 7)) || (a.AccountsGroupID == 13) || (a.Parent == 13) || ((a.AccountsGroupID == 31) || (a.Parent == 31))).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 29) || (a.Parent == 31)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => (a.Group == 7) || (a.Group == 13) || (a.Group == 31)).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 31) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = (a.Name == "Expense") ? 0 : a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null,
                order = ((a.Name == "Expense") || (a.Name == "Purchase")) ? 2 : ((a.Parent == 7) || (a.Name == "Revenue Accounts")) ? 1 : 2,
            }).OrderBy(z => z.order).ToList();

            var Parentlist = GroupList.Select(x => x.ID).ToList();


            var supparentid = new SqlParameter("@parentid", 15);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var incparentid = new SqlParameter("@parentid", 31);
            var incgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid).AsEnumerable().ToList();
            var incgpid = incgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var arr = supgpid.Union(incgpid).ToArray();


            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var ParentGroup = arr.Union(expgpid);
            var plist = (from a in db.Accountss
                         where (ParentGroup.Contains(a.Group))
                         select new
                         {
                             ID = a.AccountsID,
                             Group = a.Group
                         }).Distinct().ToList();

            var fullacclist = plist.Select(x => x.ID).ToList();
            var fullacclistexp = plist.Where(x => expgpid.Contains(x.Group)).Select(x => x.ID).ToList();
            var fullacclistincome = plist.Where(x => arr.Contains(x.Group)).Select(x => x.ID).ToList();

            // Per-project expense/income subqueries below project into nested .ToList().Sum() collections, which EF
            // Core 10 cannot translate inside an executed projection ("Expression of type 'List<anonymous>'").
            // Filter on the server, then evaluate the projection client-side so each correlated sum is its own query.
            var AccListParent = (from a in db.Projects.Where(a => (customer == null || a.Customer == customer) && (project == null || a.ProjectId == project)).AsEnumerable()
                                 select new
                                 {
                                     ProjectId = a.ProjectId,
                                     Name = a.ProjectName,
                                     parent = true,
                                     expense1 = (decimal?)(from ac in db.AccountsTransactions
                                                           where
                                                           (ac.Project == a.ProjectId) && fullacclistexp.Contains(ac.Account) && ac.Status == null && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                           select new
                                                           {
                                                               ac.Debit,
                                                           }).ToList().Sum(x => x.Debit),
                                     expense2 = (decimal?)(from ac in db.AccountsTransactions
                                                           where
                                                           (ac.Project == a.ProjectId) && fullacclistexp.Contains(ac.Account) && ac.Status == null && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                           select new
                                                           {
                                                               ac.Credit,
                                                           }).ToList().Sum(x => x.Credit),
                                     income1 = (decimal?)(from ac in db.AccountsTransactions
                                                          where
                                                          (ac.Project == a.ProjectId) && fullacclistincome.Contains(ac.Account) && ac.Status == null && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                          select new
                                                          {
                                                              ac.Debit,
                                                          }).ToList().Sum(x => x.Debit),
                                     income2 = (decimal?)(from ac in db.AccountsTransactions
                                                          where
                                                          (ac.Project == a.ProjectId) && fullacclistincome.Contains(ac.Account) && ac.Status == null && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                          select new
                                                          {
                                                              ac.Credit,
                                                          }).ToList().Sum(x => x.Credit),
                                     a.ProCode,
                                 }).Select(o => new profitabilityViewModel
                                 {
                                     ProjectId = o.ProjectId,
                                     Name = o.Name,
                                     parent = o.parent,
                                     expense = (decimal)(o.expense1 - o.expense2),
                                     income = (decimal)(o.income2 - o.income1),
                                     profit = (decimal)((o.income2 - o.income1) - (o.expense1 - o.expense2)),
                                     Procode = o.ProCode,
                                     Invoice = null,
                                     reference = 0,
                                 }).Distinct().ToList();

            var childdata1 = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.Projects on a.Project equals c.ProjectId
                              where a.Project == project && (c.ProjectId == a.Project) && fullacclistexp.Contains(a.Account) && a.Status == null && (todate == "" || EF.Functions.DateDiffDay(a.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, frm) <= 0)
                              select new profitabilityViewModel
                              {
                                  ProjectId = a.Project,
                                  Name = b.Name,
                                  parent = false,
                                  expense = a.Debit,
                                  income = 0,
                                  profit = 0,
                                  Procode = c.ProCode,
                                  reference = 0,
                                  Invoice = (a.Purpose == "Sale") ? db.SalesEntrys.Where(x => x.SalesEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Purchase") ? db.PurchaseEntrys.Where(x => x.PurchaseEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Receipt") ? db.Receipts.Where(x => x.ReceiptId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Payment") ? db.Payments.Where(x => x.PaymentId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Journal") ? db.Journals.Where(x => x.JournalId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : a.reference.ToString(),//,
                              }).ToList();
            var childdata2 = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.Projects on a.Project equals c.ProjectId
                              where a.Project == project && (c.ProjectId == a.Project) && fullacclistexp.Contains(a.Account) && a.Status == null && (todate == "" || EF.Functions.DateDiffDay(a.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, frm) <= 0)
                              select new profitabilityViewModel
                              {
                                  ProjectId = a.Project,
                                  Name = b.Name,
                                  parent = false,
                                  expense = -a.Credit,
                                  income = 0,
                                  profit = 0,
                                  Procode = c.ProCode,
                                  reference = 0,
                                  Invoice = (a.Purpose == "Sale") ? db.SalesEntrys.Where(x => x.SalesEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Purchase") ? db.PurchaseEntrys.Where(x => x.PurchaseEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Receipt") ? db.Receipts.Where(x => x.ReceiptId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Payment") ? db.Payments.Where(x => x.PaymentId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Journal") ? db.Journals.Where(x => x.JournalId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : a.reference.ToString(),//,
                              }).ToList();
            var chidpurchase = (from a in db.AccountsTransactions
                                join b in db.Accountss on a.Account equals b.AccountsID

                                join d in db.PEItemss on a.reference equals d.PurchaseEntry
                                join c in db.Projects on d.ProjectId equals c.ProjectId
                                where d.ProjectId == project && a.Status == null && (todate == "" || EF.Functions.DateDiffDay(a.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, frm) <= 0)
                                select new profitabilityViewModel
                                {
                                    ProjectId = a.Project,
                                    Name = b.Name,
                                    parent = false,
                                    expense = -a.Credit,
                                    income = 0,
                                    profit = 0,
                                    Procode = c.ProCode,
                                    reference = 0,
                                    Invoice = (a.Purpose == "Sale") ? db.SalesEntrys.Where(x => x.SalesEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Purchase") ? db.PurchaseEntrys.Where(x => x.PurchaseEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Receipt") ? db.Receipts.Where(x => x.ReceiptId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Payment") ? db.Payments.Where(x => x.PaymentId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Journal") ? db.Journals.Where(x => x.JournalId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : a.reference.ToString(),//,
                                }).Distinct().ToList();
            var childdata3 = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.Projects on a.Project equals c.ProjectId
                              where a.Project == project && (c.ProjectId == a.Project) && fullacclistincome.Contains(a.Account) && a.Status == null && (todate == "" || EF.Functions.DateDiffDay(a.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, frm) <= 0)
                              select new profitabilityViewModel
                              {
                                  ProjectId = a.Project,
                                  Name = b.Name,
                                  parent = false,
                                  expense = 0,
                                  income = -a.Debit,
                                  profit = 0,
                                  Procode = c.ProCode,
                                  reference = 0,
                                  Invoice = (a.Purpose == "Sale") ? db.SalesEntrys.Where(x => x.SalesEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Purchase") ? db.PurchaseEntrys.Where(x => x.PurchaseEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Receipt") ? db.Receipts.Where(x => x.ReceiptId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Payment") ? db.Payments.Where(x => x.PaymentId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Journal") ? db.Journals.Where(x => x.JournalId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : a.reference.ToString(),//,
                              }).ToList();
            var childdata4 = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.Projects on a.Project equals c.ProjectId
                              where a.Project == project && (c.ProjectId == a.Project) && fullacclistincome.Contains(a.Account) && a.Status == null && (todate == "" || EF.Functions.DateDiffDay(a.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, frm) <= 0)
                              select new profitabilityViewModel
                              {
                                  ProjectId = a.Project,
                                  Name = b.Name,
                                  parent = false,
                                  expense = 0,
                                  income = a.Credit,
                                  profit = 0,
                                  Procode = c.ProCode,
                                  reference = 0,
                                  Invoice = (a.Purpose == "Sale") ? db.SalesEntrys.Where(x => x.SalesEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Purchase") ? db.PurchaseEntrys.Where(x => x.PurchaseEntryId == a.reference).Select(y => y.BillNo).FirstOrDefault() : (a.Purpose == "Receipt") ? db.Receipts.Where(x => x.ReceiptId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Payment") ? db.Payments.Where(x => x.PaymentId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : (a.Purpose == "Journal") ? db.Journals.Where(x => x.JournalId == a.reference).Select(y => y.VoucherNo).FirstOrDefault() : a.reference.ToString(),//,
                              }).ToList();


            var list = AccListParent.Union(childdata1);
            list = list.Union(childdata2);
            list = list.Union(childdata3);
            list = list.Union(childdata4);
            list = list.Union(chidpurchase);


            var sortlist = list.Where(z => (z.expense != 0) || (z.income != 0)).OrderBy(a => a.ProjectId).ToList();
            var totalincome = list.Where(x => x.parent == true).Select(y => y.income).Sum();
            var totalexpense = list.Where(x => x.parent == true).Select(y => y.expense).Sum();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = sortlist });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion

        #region ProjectPLSummary
        [HttpGet]
        public ActionResult PLSummary(long? id)
        {
            var UserId = User.Identity.GetUserId();

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            companySet();
            return View();
        }
        [HttpGet]
        public ActionResult PLSummaryProperty(long? id)
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.Cust = OpAll;
            ViewBag.Alldata = QkSelect.List(
               new List<SelectListItem>
               {
                                        new SelectListItem { Selected = true, Text = "All", Value = "0"},
               }, "Value", "Text", 0);
            var UserId = User.Identity.GetUserId();
            var pro = db.PropertyMains.Select(o => new
            {
                PropertyId = o.Id,
                propertyName = o.Name

            }).ToList();
            /*    long pid = 0;
           pro.Add(new {
                   PropertyId =pid,
                   propertyName = "All"

               });

               */

            ViewBag.PropertyList = QkSelect.List(pro, "PropertyId", "propertyName");

            companySet();
            return View();
        }

        //[QkAuthorize(Roles = "Dev,Report Production")]
        public ActionResult GetPLSummary(long? customer, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? frm = null;
            DateTime? to = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                frm = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                to = tdate;
            }


            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => ((a.AccountsGroupID == 7) || (a.Parent == 7)) || (a.AccountsGroupID == 13) || (a.Parent == 13) || ((a.AccountsGroupID == 31) || (a.Parent == 31))).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 29) || (a.Parent == 31)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => (a.Group == 7) || (a.Group == 13) || (a.Group == 31)).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 31) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = (a.Name == "Expense") ? 0 : a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null,
                order = ((a.Name == "Expense") || (a.Name == "Purchase")) ? 2 : ((a.Parent == 7) || (a.Name == "Revenue Accounts")) ? 1 : 2,
            }).OrderBy(z => z.order).ToList();

            var Parentlist = GroupList.Select(x => x.ID).ToList();


            var supparentid = new SqlParameter("@parentid", 15);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var incparentid = new SqlParameter("@parentid", 31);
            var incgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid).AsEnumerable().ToList();
            var incgpid = incgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var arr = supgpid.Union(incgpid).ToArray();


            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var ParentGroup = arr.Union(expgpid);
            var plist = (from a in db.Accountss
                         where (ParentGroup.Contains(a.Group))
                         select new
                         {
                             ID = a.AccountsID,
                             Group = a.Group
                         }).Distinct().ToList();

            var fullacclist = plist.Select(x => x.ID).ToList();
            var fullacclistexp = plist.Where(x => expgpid.Contains(x.Group)).Select(x => x.ID).ToList();
            var fullacclistincome = plist.Where(x => arr.Contains(x.Group)).Select(x => x.ID).ToList();

            // Per-project expense/income subqueries below project into nested .ToList().Sum() collections, which EF
            // Core 10 cannot translate inside an executed projection ("Expression of type 'List<anonymous>'").
            // Filter on the server, then evaluate the projection client-side so each correlated sum is its own query.
            var mydata = (from a in db.Projects.Where(a => (customer == null || a.Customer == customer)).AsEnumerable()
                          select new
                          {
                              ProjectId = a.ProjectId,
                              ProjectName = a.ProjectName,
                              expense1 = (decimal?)(from ac in db.AccountsTransactions
                                                    where
                                                    (ac.Project == a.ProjectId) && fullacclistexp.Contains(ac.Account) && ac.Status == null && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                    select new
                                                    {
                                                        ac.Debit,
                                                    }).ToList().Sum(x => x.Debit),
                              expense2 = (decimal?)(from ac in db.AccountsTransactions
                                                    where
                                                    (ac.Project == a.ProjectId) && fullacclistexp.Contains(ac.Account) && ac.Status == null && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                    select new
                                                    {
                                                        ac.Credit,
                                                    }).ToList().Sum(x => x.Credit),
                              income1 = (decimal?)(from ac in db.AccountsTransactions
                                                   where
                                                   (ac.Project == a.ProjectId) && fullacclistincome.Contains(ac.Account) && ac.Status == null && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                   select new
                                                   {
                                                       ac.Debit,
                                                   }).ToList().Sum(x => x.Debit),
                              income2 = (decimal?)(from ac in db.AccountsTransactions
                                                   where
                                                   (ac.Project == a.ProjectId) && fullacclistincome.Contains(ac.Account) && ac.Status == null && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                   select new
                                                   {
                                                       ac.Credit,
                                                   }).ToList().Sum(x => x.Credit),
                              a.ProCode,
                          }).Select(o => new
                          {
                              ProjectId = o.ProjectId,
                              o.ProjectName,
                              expense = (o.expense1 - o.expense2),
                              income = (o.income2 - o.income1),
                              profit = ((o.income2 - o.income1) - (o.expense1 - o.expense2)),
                              o.ProCode,
                          }).Distinct().ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }
        #endregion
        public ActionResult GetPLSummaryProperty(long? customer, long? landlord, string fromdate, string todate, bool? pdc)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? frm = null;
            DateTime? to = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                frm = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                to = tdate;
            }


            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => ((a.AccountsGroupID == 7) || (a.Parent == 7)) || (a.AccountsGroupID == 13) || (a.Parent == 13) || ((a.AccountsGroupID == 31) || (a.Parent == 31))).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 29) || (a.Parent == 31)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => (a.Group == 7) || (a.Group == 13) || (a.Group == 31)).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 31) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = (a.Name == "Expense") ? 0 : a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null,
                order = ((a.Name == "Expense") || (a.Name == "Purchase")) ? 2 : ((a.Parent == 7) || (a.Name == "Revenue Accounts")) ? 1 : 2,
            }).OrderBy(z => z.order).ToList();

            var Parentlist = GroupList.Select(x => x.ID).ToList();


            var supparentid = new SqlParameter("@parentid", 15);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var incparentid = new SqlParameter("@parentid", 31);
            var incgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid).AsEnumerable().ToList();
            var incgpid = incgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var incparentid2 = new SqlParameter("@parentid", 37);
            var incgroupsdata2 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid2).AsEnumerable().ToList();
            var incgpid2 = incgroupsdata2.Select(a => a.AccountsGroupID).ToArray();

            var incparentid3 = new SqlParameter("@parentid", 32);
            var incgroupsdata3 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid3).AsEnumerable().ToList();
            var incgpid3 = incgroupsdata3.Select(a => a.AccountsGroupID).ToArray();


            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();




            var assetid = new SqlParameter("@parentid", 4);
            var assetdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", assetid).AsEnumerable().ToList();
            var assetgpid = assetdata.Select(a => a.AccountsGroupID).ToArray();

            /*
                        var incparentid3 = new SqlParameter("@parentid", 34);
                        var incgroupsdata3 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid3).AsEnumerable().ToList();
                        var incgpid3 = incgroupsdata3.Select(a => a.AccountsGroupID).ToArray();



                        var incparentid4 = new SqlParameter("@parentid", 35);
                        var incgroupsdata4 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid4).AsEnumerable().ToList();
                        var incgpid4 = incgroupsdata4.Select(a => a.AccountsGroupID).ToArray();



                        var incparentid5 = new SqlParameter("@parentid", 36);
                        var incgroupsdata5 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid5).AsEnumerable().ToList();
                        var incgpid5 = incgroupsdata5.Select(a => a.AccountsGroupID).ToArray();
            */
            var incom = incgpid.Union(incgpid3);
            incom = incom.Union(incgpid2);
            var arr = supgpid.Union(incgpid);

            arr = arr.Union(incom).ToArray();
            arr = arr.Union(assetgpid).ToArray();
            arr = arr.Union(expgpid).ToArray();



            var ParentGroup = arr.Union(expgpid);

            var plist = (from a in db.Accountss
                         where (ParentGroup.Contains(a.Group))
                         select new
                         {
                             ID = a.AccountsID,
                             Group = a.Group
                         }).Distinct().ToList();

            var fullacclist = plist.Select(x => x.ID).ToList();
            var fullacclistexp = plist.Where(x => expgpid.Contains(x.Group)).Select(x => x.ID).ToList();


            var fullacclistincome = plist.Where(x => incom.Contains(x.Group)).Select(x => x.ID).ToList();
            var fullasset = plist.Where(x => assetgpid.Contains(x.Group)).Select(x => x.ID).ToList();
            if (customer == 0 || customer == null)
                customer = 0;

            // Per-property expense/income/security subqueries below project into nested .ToList().Sum() collections,
            // which EF Core 10 cannot translate inside an executed projection ("Expression of type 'List<anonymous>'").
            // Resolve the property/landlord join + filter on the server, then evaluate the heavy projection client-side
            // so each correlated sum runs as its own translatable query.
            var propRows = (from a in db.PropertyMains
                            join b in db.Landlords on a.LandlordID equals b.LandlordID into cst
                            from b in cst.DefaultIfEmpty()
                            where (customer == 0 || a.Id == customer) &&
                                  (landlord == 0 || landlord == null || b.LandlordID == landlord)
                            select new { a.Id, a.Name, a.PropertyType }).AsEnumerable();

            var mydata = (from a in propRows
                          select new
                          {
                              ProjectId = a.Id,
                              ProjectName = a.Name,
                              expense1 = (decimal?)(from ac in db.AccountsTransactions
                                                    where
                                                    (ac.Project == a.Id) && fullacclistexp.Contains(ac.Account) && (pdc == true || ac.Status == null) && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                    select new
                                                    {
                                                        ac.Debit,
                                                    }).Sum(x => x.Debit) ?? 0,
                              expense2 = (decimal?)(from ac in db.AccountsTransactions
                                                    where
                                                    (ac.Project == a.Id) && fullacclistexp.Contains(ac.Account) && (pdc == true || ac.Status == null) && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                    select new
                                                    {
                                                        ac.Credit,
                                                    }).Sum(x => x.Credit) ?? 0,

                              income1 = (decimal?)(from ac in db.AccountsTransactions
                                                   where
                                                   (ac.Project == a.Id) && fullacclistincome.Contains(ac.Account) && (pdc == true || ac.Status == null) && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                   select new
                                                   {
                                                       ac.Debit,
                                                   }).Sum(x => x.Debit) ?? 0,
                              income2 = (decimal?)(from ac in db.AccountsTransactions
                                                   where
                                                   (ac.Project == a.Id) && fullacclistincome.Contains(ac.Account) && (pdc == true || ac.Status == null) && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                                                   select new
                                                   {
                                                       ac.Credit,
                                                   }).Sum(x => x.Credit) ?? 0,
                              purchase1 = (decimal?)(from ac in db.AccountsTransactions
                                                     where
                                                    (ac.Project == a.Id) && fullasset.Contains(ac.Account) && (pdc == true || ac.Status == null)
                                                     select new
                                                     {
                                                         ac.Credit,
                                                     }).Sum(x => x.Credit) ?? 0,
                              purchase2 = (decimal?)(from ac in db.AccountsTransactions
                                                     where
                                                    (ac.Project == a.Id) && fullasset.Contains(ac.Account) && (pdc == true || ac.Status == null)
                                                     select new
                                                     {
                                                         ac.Debit,
                                                     }).Sum(x => x.Debit) ?? 0,

                              security = (decimal?)(from r in db.Receipts
                                                    where r.Remark.ToLower().Contains("deposit") && r.Project == a.Id
                                                    select new
                                                    {
                                                        r.Paying
                                                    }).Sum(o => o.Paying) ?? 0,

                              a.PropertyType,
                          }).Select(o => new
                          {
                              ProjectId = o.ProjectId,
                              o.ProjectName,
                              expense = Math.Abs(o.expense1 - o.expense2),
                              income = Math.Abs(o.income2 - o.income1 - o.security),
                              profit = Math.Abs(o.income2 - o.income1 - o.security) - Math.Abs(o.expense1 - o.expense2),
                              purchase = Math.Abs(o.purchase2 - o.purchase1),
                              rateofreturn = 0,// ((o.income2 - o.income1 - o.security) - (o.expense1 - o.expense2)) / (((o.purchase3) == 0) ? 1 : (o.purchase3)),
                              o.PropertyType,
                          }).Distinct().ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }
        public ActionResult GetPurchaseView(long? customer, string fromdate, string todate, bool? pdc)
        {

            LedgerprofitViewModel vmodel = GetIncomepurchase(customer, fromdate, todate, pdc);
            companySet();
            return View(vmodel);
        }
        public LedgerprofitViewModel GetIncomepurchase(long? customer, string fromdate, string todate, bool? pdc)
        {
            LedgerprofitViewModel vmodel = new LedgerprofitViewModel();



            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? frm = null;
            DateTime? to = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                frm = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                to = tdate;
            }
            vmodel.from = frm;
            vmodel.to = to;


            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => ((a.AccountsGroupID == 7) || (a.Parent == 7)) || (a.AccountsGroupID == 13) || (a.Parent == 13) || ((a.AccountsGroupID == 31) || (a.Parent == 31))).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 29) || (a.Parent == 31)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => (a.Group == 7) || (a.Group == 13) || (a.Group == 31)).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 31) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = (a.Name == "Expense") ? 0 : a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null,
                order = ((a.Name == "Expense") || (a.Name == "Purchase")) ? 2 : ((a.Parent == 7) || (a.Name == "Revenue Accounts")) ? 1 : 2,
            }).OrderBy(z => z.order).ToList();

            var Parentlist = GroupList.Select(x => x.ID).ToList();


            var supparentid = new SqlParameter("@parentid", 15);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var incparentid = new SqlParameter("@parentid", 31);
            var incgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid).AsEnumerable().ToList();
            var incgpid = incgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var incparentid2 = new SqlParameter("@parentid", 37);
            var incgroupsdata2 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid2).AsEnumerable().ToList();
            var incgpid2 = incgroupsdata2.Select(a => a.AccountsGroupID).ToArray();
            var incparentid3 = new SqlParameter("@parentid", 32);
            var incgroupsdata3 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid3).AsEnumerable().ToList();
            var incgpid3 = incgroupsdata3.Select(a => a.AccountsGroupID).ToArray();


            var assetid = new SqlParameter("@parentid", 4);
            var assetdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", assetid).AsEnumerable().ToList();
            var assetgpid = assetdata.Select(a => a.AccountsGroupID).ToArray();

            /*
                        var incparentid3 = new SqlParameter("@parentid", 34);
                        var incgroupsdata3 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid3).AsEnumerable().ToList();
                        var incgpid3 = incgroupsdata3.Select(a => a.AccountsGroupID).ToArray();



                        var incparentid4 = new SqlParameter("@parentid", 35);
                        var incgroupsdata4 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid4).AsEnumerable().ToList();
                        var incgpid4 = incgroupsdata4.Select(a => a.AccountsGroupID).ToArray();



                        var incparentid5 = new SqlParameter("@parentid", 36);
                        var incgroupsdata5 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid5).AsEnumerable().ToList();
                        var incgpid5 = incgroupsdata5.Select(a => a.AccountsGroupID).ToArray();
            */



            var ParentGroup = assetgpid.ToArray();
            var plist = (from a in db.Accountss
                         where (ParentGroup.Contains(a.Group))
                         select new
                         {
                             ID = a.AccountsID,
                             Group = a.Group
                         }).Distinct().ToList();


            var fullassetincome = plist.Where(x => assetgpid.Contains(x.Group)).Select(x => x.ID).ToList();

            long pr = Convert.ToInt64(customer);



            vmodel.Ledger = (from ac in db.AccountsTransactions
                             join acc in db.Accountss on ac.Account equals acc.AccountsID
                             join j in db.Journals on ac.reference equals j.JournalId into jj
                             from jjj in jj.DefaultIfEmpty()
                             join p in db.Payments on ac.reference equals p.PaymentId into pp
                             from ppp in pp.DefaultIfEmpty()
                             join r in db.Receipts on ac.reference equals r.ReceiptId into rr
                             from rrr in rr.DefaultIfEmpty()


                             where
                             (ac.Project == pr) && fullassetincome.Contains(ac.Account) && (pdc == true || ac.Status == null) && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                             select new Ledgerprofit
                             {
                                 transid = ac.Id,
                                 transdate = ac.Date,
                                 Pupose = ac.Purpose,
                                 referance = ac.reference,
                                 voucherno = (jjj.VoucherNo != null) ? jjj.VoucherNo : (ppp.VoucherNo != null) ? ppp.VoucherNo : rrr.VoucherNo,
                                 ledger = acc.Name,
                                 amount = ac.Debit + ac.Credit

                             }).ToList();


            return vmodel;






        }
        public ActionResult GetIncomeView(long? customer, string fromdate, string todate, bool? pdc)
        {

            LedgerprofitViewModel vmodel = GetIncome(customer, fromdate, todate, pdc);
            companySet();
            return View(vmodel);
        }

        public LedgerprofitViewModel GetIncome(long? customer, string fromdate, string todate, bool? pdc)
        {
            LedgerprofitViewModel vmodel = new LedgerprofitViewModel();



            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? frm = null;
            DateTime? to = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                frm = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                to = tdate;
            }
            vmodel.from = frm;
            vmodel.to = to;


            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => ((a.AccountsGroupID == 7) || (a.Parent == 7)) || (a.AccountsGroupID == 13) || (a.Parent == 13) || ((a.AccountsGroupID == 31) || (a.Parent == 31))).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 29) || (a.Parent == 31)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => (a.Group == 7) || (a.Group == 13) || (a.Group == 31)).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 31) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = (a.Name == "Expense") ? 0 : a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null,
                order = ((a.Name == "Expense") || (a.Name == "Purchase")) ? 2 : ((a.Parent == 7) || (a.Name == "Revenue Accounts")) ? 1 : 2,
            }).OrderBy(z => z.order).ToList();

            var Parentlist = GroupList.Select(x => x.ID).ToList();


            var supparentid = new SqlParameter("@parentid", 15);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var incparentid = new SqlParameter("@parentid", 31);
            var incgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid).AsEnumerable().ToList();
            var incgpid = incgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var incparentid2 = new SqlParameter("@parentid", 37);
            var incgroupsdata2 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid2).AsEnumerable().ToList();
            var incgpid2 = incgroupsdata2.Select(a => a.AccountsGroupID).ToArray();
            var incparentid3 = new SqlParameter("@parentid", 32);
            var incgroupsdata3 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid3).AsEnumerable().ToList();
            var incgpid3 = incgroupsdata3.Select(a => a.AccountsGroupID).ToArray();

            /*
                        var incparentid3 = new SqlParameter("@parentid", 34);
                        var incgroupsdata3 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid3).AsEnumerable().ToList();
                        var incgpid3 = incgroupsdata3.Select(a => a.AccountsGroupID).ToArray();



                        var incparentid4 = new SqlParameter("@parentid", 35);
                        var incgroupsdata4 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid4).AsEnumerable().ToList();
                        var incgpid4 = incgroupsdata4.Select(a => a.AccountsGroupID).ToArray();



                        var incparentid5 = new SqlParameter("@parentid", 36);
                        var incgroupsdata5 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid5).AsEnumerable().ToList();
                        var incgpid5 = incgroupsdata5.Select(a => a.AccountsGroupID).ToArray();
            */
            var arr = supgpid.Union(incgpid);
            arr = arr.Union(incgpid3);
            arr = arr.Union(incgpid2).ToArray();

            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var ParentGroup = arr;
            var plist = (from a in db.Accountss
                         where (ParentGroup.Contains(a.Group))
                         select new
                         {
                             ID = a.AccountsID,
                             Group = a.Group
                         }).Distinct().ToList();

            var fullacclist = plist.Select(x => x.ID).ToList();
            var fullacclistexp = plist.Where(x => expgpid.Contains(x.Group)).Select(x => x.ID).ToList();
            var fullacclistincome = plist.Where(x => arr.Contains(x.Group)).Select(x => x.ID).ToList();

            long pr = Convert.ToInt64(customer);



            vmodel.Ledger = (from ac in db.AccountsTransactions
                             join acc in db.Accountss on ac.Account equals acc.AccountsID
                             join j in db.Journals on ac.reference equals j.JournalId into jj
                             from jjj in jj.DefaultIfEmpty()
                             join p in db.Payments on ac.reference equals p.PaymentId into pp
                             from ppp in pp.DefaultIfEmpty()
                             join r in db.Receipts on ac.reference equals r.ReceiptId into rr
                             from rrr in rr.DefaultIfEmpty()


                             where
                             (ac.Project == pr) && fullacclistincome.Contains(ac.Account) && (pdc == true || ac.Status == null) && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                             select new Ledgerprofit
                             {
                                 transid = ac.Id,
                                 transdate = ac.Date,
                                 Pupose = ac.Purpose,
                                 referance = ac.reference,
                                 voucherno = (jjj.VoucherNo != null) ? jjj.VoucherNo : (ppp.VoucherNo != null) ? ppp.VoucherNo : rrr.VoucherNo,
                                 ledger = acc.Name,
                                 amount = ac.Debit + ac.Credit,
                                 remark = rrr.Remark,
                             }).ToList();


            return vmodel;






        }





        public ActionResult GetExpenseView(long? customer, string fromdate, string todate, bool? pdc)
        {

            LedgerprofitViewModel vmodel = GetExpense(customer, fromdate, todate, pdc);
            companySet();
            return View(vmodel);
        }
        public LedgerprofitViewModel GetExpense(long? customer, string fromdate, string todate, bool? pdc)
        {
            LedgerprofitViewModel vmodel = new LedgerprofitViewModel();



            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? frm = null;
            DateTime? to = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                frm = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                to = tdate;
            }
            vmodel.from = frm;
            vmodel.to = to;


            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => ((a.AccountsGroupID == 7) || (a.Parent == 7)) || (a.AccountsGroupID == 13) || (a.Parent == 13) || ((a.AccountsGroupID == 31) || (a.Parent == 31))).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 29) || (a.Parent == 31)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => (a.Group == 7) || (a.Group == 13) || (a.Group == 31)).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 31) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = (a.Name == "Expense") ? 0 : a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null,
                order = ((a.Name == "Expense") || (a.Name == "Purchase")) ? 2 : ((a.Parent == 7) || (a.Name == "Revenue Accounts")) ? 1 : 2,
            }).OrderBy(z => z.order).ToList();

            var Parentlist = GroupList.Select(x => x.ID).ToList();


            var supparentid = new SqlParameter("@parentid", 15);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var incparentid = new SqlParameter("@parentid", 31);
            var incgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid).AsEnumerable().ToList();
            var incgpid = incgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var incparentid2 = new SqlParameter("@parentid", 37);
            var incgroupsdata2 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid2).AsEnumerable().ToList();
            var incgpid2 = incgroupsdata2.Select(a => a.AccountsGroupID).ToArray();

            /*
                        var incparentid3 = new SqlParameter("@parentid", 34);
                        var incgroupsdata3 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid3).AsEnumerable().ToList();
                        var incgpid3 = incgroupsdata3.Select(a => a.AccountsGroupID).ToArray();



                        var incparentid4 = new SqlParameter("@parentid", 35);
                        var incgroupsdata4 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid4).AsEnumerable().ToList();
                        var incgpid4 = incgroupsdata4.Select(a => a.AccountsGroupID).ToArray();



                        var incparentid5 = new SqlParameter("@parentid", 36);
                        var incgroupsdata5 = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid5).AsEnumerable().ToList();
                        var incgpid5 = incgroupsdata5.Select(a => a.AccountsGroupID).ToArray();
            */
            var arr = supgpid.Union(incgpid);

            arr = arr.Union(incgpid2).ToArray();

            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var ParentGroup = arr.Union(expgpid);
            var plist = (from a in db.Accountss
                         where (ParentGroup.Contains(a.Group))
                         select new
                         {
                             ID = a.AccountsID,
                             Group = a.Group
                         }).Distinct().ToList();

            var fullacclist = plist.Select(x => x.ID).ToList();
            var fullacclistexp = plist.Where(x => expgpid.Contains(x.Group)).Select(x => x.ID).ToList();
            var fullacclistincome = plist.Where(x => arr.Contains(x.Group)).Select(x => x.ID).ToList();

            long pr = Convert.ToInt64(customer);



            vmodel.Ledger = (from ac in db.AccountsTransactions
                             join acc in db.Accountss on ac.Account equals acc.AccountsID
                             join j in db.Journals on ac.reference equals j.JournalId into jj
                             from jjj in jj.DefaultIfEmpty()
                             join p in db.Payments on ac.reference equals p.PaymentId into pp
                             from ppp in pp.DefaultIfEmpty()
                             join r in db.Receipts on ac.reference equals r.ReceiptId into rr
                             from rrr in rr.DefaultIfEmpty()


                             where
                             (ac.Project == pr) && fullacclistexp.Contains(ac.Account) && (pdc == true || ac.Status == null) && (todate == "" || EF.Functions.DateDiffDay(ac.Date, to) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, frm) <= 0)
                             select new Ledgerprofit
                             {
                                 transid = ac.Id,
                                 transdate = ac.Date,
                                 Pupose = ac.Purpose,
                                 referance = ac.reference,
                                 voucherno = (jjj.VoucherNo != null) ? jjj.VoucherNo : (ppp.VoucherNo != null) ? ppp.VoucherNo : rrr.VoucherNo,
                                 ledger = acc.Name,
                                 amount = ac.Debit + ac.Credit

                             }).ToList();


            return vmodel;






        }













        #region SimplePL
        [HttpGet]
        public ActionResult SimplePL(long? id)
        {
            companySet();
            _FinancialYear();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,Report Production")]
        public ActionResult GetSimplePL(string fromdate, string todate)
        {
            String format = "dd-MM-yyyy";
            DateTime? tdate = null;
            DateTime? to = null;
            DateTime? fdate = null;
            DateTime? frm = null;
            if (todate != "")
            {
                tdate = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
                to = tdate;
            }
            if (fromdate != "")
            {
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                frm = fdate;
            }
            TrialBalanceAccViewModel vmodel = new TrialBalanceAccViewModel();
            vmodel.To = to;
            vmodel.From = frm;

            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => ((a.AccountsGroupID == 7) || (a.Parent == 7)) || (a.AccountsGroupID == 13) || (a.Parent == 13) || ((a.AccountsGroupID == 31) || (a.Parent == 31))).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 31)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => (a.Group == 7) || (a.Group == 13) || (a.Group == 31)).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 7) || (a.Parent == 13) || (a.Parent == 31) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = (a.Name == "Expense") ? 0 : a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null,
                order = ((a.Name == "Expense") || (a.Name == "Purchase")) ? 2 : ((a.Parent == 7) || (a.Name == "Revenue Accounts")) ? 1 : 2,
            }).OrderBy(z => z.order).ToList();

            var Parentlist = GroupList.Select(x => x.ID).ToList();
            var plist = (from a in db.Accountss
                         where (Parentlist.Contains(a.Group))
                         select new TrialBalanceAcc
                         {
                             ID = a.AccountsID,
                             Group = a.Group
                         }).Distinct().ToList();

            var Parlist = plist.Select(x => x.ID).ToList();



            var supparentid = new SqlParameter("@parentid", 15);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var incparentid = new SqlParameter("@parentid", 31);
            var incgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", incparentid).AsEnumerable().ToList();
            var incgpid = incgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var arr = supgpid.Union(incgpid).ToArray();


            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            var ParentGroup = arr.Union(expgpid);
            var acclistgp = (from a in db.Accountss
                             where (ParentGroup.Contains(a.Group))
                             select new
                             {
                                 ID = a.AccountsID,
                                 Group = a.Group
                             }).Distinct().ToList();

            var fullacclist = acclistgp.Select(x => x.ID).ToList();
            var fullacclistexp = acclistgp.Where(x => expgpid.Contains(x.Group)).Select(x => x.ID).ToList();
            var fullacclistincome = acclistgp.Where(x => arr.Contains(x.Group)).Select(x => x.ID).ToList();




            var AccList1 = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            where (fullacclist.Contains(a.Account)) && a.Status == null
                            && (a.Date <= to && a.Date >= frm)
                            //&& (a.Project != null)
                            select new TrialBalanceAcc
                            {
                                ID = b.AccountsID,
                                text = b.Name,
                                Parent = 0,
                                Type = "Account",
                                AccountId = b.AccountsID,
                                Debit = db.AccountsTransactions.Where(z => z.Account == a.Account && z.Status == null && z.Date <= to && z.Date >= frm).Select(y => (decimal?)y.Debit).Sum(),
                                Credit = db.AccountsTransactions.Where(z => z.Account == a.Account && z.Status == null && z.Date <= to && z.Date >= frm).Select(y => (decimal?)y.Credit).Sum(),
                                Group = b.Group,
                            }).Distinct().ToList();

            var accountids = AccList1.Select(a => a.AccountId).ToList();

            var AccList = AccList1;//.Union(AccListP);

            AccList = (from a in AccList
                       join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                       where (b.AccountsGroupID != 32) //&& (a.ID != 2)
                       select new TrialBalanceAcc
                       {
                           text = a.text,
                           Parent = a.Parent,
                           Type = a.Type,
                           AccountId = a.AccountId,
                           Debit = fullacclistexp.Contains((long)a.AccountId) ? ((a.Debit > a.Credit) ? (a.Debit - a.Credit) : 0) : ((a.Debit > a.Credit) ? (a.Debit - a.Credit) : 0),
                           Credit = fullacclistincome.Contains((long)a.AccountId) ? ((a.Credit > a.Debit) ? (a.Credit - a.Debit) : 0) : ((a.Credit > a.Debit) ? (a.Credit - a.Debit) : 0),
                           order = (fullacclistexp.Contains((long)a.AccountId)) ? 2 : 1
                       }).OrderBy(z => z.order).ToList();

            var List = GroupList.Union(AccList).ToList();
            var cresum = List.Where(z => z.order == 1).Select(z => z.Credit).Sum();
            var credebt = List.Where(z => z.order == 1).Select(z => z.Debit).Sum();
            var totcre = cresum - credebt;
            var debsum = List.Where(z => z.order == 2).Select(z => z.Debit).Sum();
            var debdebt = List.Where(z => z.order == 2).Select(z => z.Credit).Sum();
            var totdeb = debsum - debdebt;

            var ListNew = List.Select(a => new TrialBalanceAcc
            {
                ID = null,
                text = a.text,
                Parent = a.Parent,
                Type = a.Type,
                AccountId = a.AccountId,
                Debit = a.Debit != null ? a.Debit : 0,
                Credit = a.Credit != null ? a.Credit : 0,
                order = a.order,
                amount = (a.text == "Revenue Accounts") ? totcre : (a.text == "Expense") ? totdeb : 0
            }).OrderBy(z => z.order).ToList();

            vmodel.Data = ListNew;
            companySet();
            return View(vmodel);
        }
        #endregion
        #endregion

        #region Lead search
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report Leads")]
        public ActionResult Leads()
        {

            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);
            ViewBag.LeadStatus = QkSelect.List(
                         new List<SelectListItem>
                        {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                         }, "Value", "Text", 1);
            ViewBag.SalesExec = QkSelect.List(
                        new List<SelectListItem>
                        {
                                                new SelectListItem { Selected = false, Text = "All", Value = "0"},
                        }, "Value", "Text", 1);

            ViewBag.SrcLead = QkSelect.List(
                      new List<SelectListItem>
                      {
                                                new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Report Leads")]
        public ActionResult GetLeads(long? salesexec, long? srclead, long? leadstatus, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

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

            var cuscontactslist = (from a in db.Customers
                                   join b in db.ContactRelation on a.CustomerID equals b.RelationID
                                   join c in db.Contacts on b.ContactID equals c.ContactID
                                   where (b.RelationType == (long)CRMCustomerType.Customer)

                                   select new MobileViewModel
                                   {
                                       emails = c.EmailId,
                                       Name = a.CustomerName,
                                       Num = c.Mobile,
                                       ID = a.CustomerID
                                   });

            var leadcontactslist = (from a in db.Customers
                                    join b in db.ContactRelation on a.CustomerID equals b.RelationID
                                    join c in db.Contacts on b.ContactID equals c.ContactID
                                    where (a.Type == CRMCustomerType.Leads)

                                    select new MobileViewModel
                                    {
                                        emails = c.EmailId,
                                        Name = a.CustomerName,
                                        Num = c.Mobile,
                                        ID = a.CustomerID
                                    });

            var v = (from b in db.Customers

                     join cc in db.LeadStatuss on b.CurrentAction equals cc.LeadStatusID into ldstat
                     from cc in ldstat.DefaultIfEmpty()
                     join c in db.SourceOfLeads on b.SourceOfLead equals c.SourceOfLeadId into src
                     from c in src.DefaultIfEmpty()
                     join d in leadcontactslist on b.CustomerID equals d.ID into cont
                     from d in cont.DefaultIfEmpty()
                     join e in db.CustomerConversions on b.CustomerID equals e.CustomerID into con
                     from e in con.DefaultIfEmpty()
                     join f in db.Employees on b.SalesPerson equals f.EmployeeId into emp
                     from f in emp.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where b.Type == CRMCustomerType.Leads &&
                           (salesexec == null || salesexec == 0 ||

                           db.AssignedTos.Where(o => o.CustomerID == b.CustomerID && o.EmployeeId == salesexec).Select(o => o.EmployeeId).FirstOrDefault() == salesexec

                           ) &&
                           (srclead == 0 || b.SourceOfLead == srclead) &&
                               (leadstatus == null || leadstatus == 0 ||
                               b.CurrentAction == leadstatus
                              ) &&
                           (fromdate == "" || EF.Functions.DateDiffDay(e.CreatedDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(e.CreatedDate, tdate) >= 0)
                     select new
                     {
                         b.CustomerID,
                         b.CustomerCode,
                         b.CustomerName,
                         OrgCustomerName = (d != null) ? (string)(from xx in cuscontactslist
                                                                  where xx.Num.Contains(d.Num)
                                                                  select new
                                                                  {
                                                                      xx.Name
                                                                  }).FirstOrDefault().Name : "",

                         TaxRegNo = i.TRN,
                         b.CreditPeriod,
                         b.CreditLimit,
                         b.Location,
                         SalesPerson = f.FirstName + " " + f.LastName,
                         c.SrcName,
                         b.Remark,
                         Phone = (d == null) ? "" : d.Num,



                         lead = (from f in db.AssignedTos
                                 join g in db.Employees on f.EmployeeId equals g.EmployeeId into emps
                                 from g in emps.DefaultIfEmpty()
                                 where f.CustomerID == b.CustomerID
                                 && f.Status == "Assigned" && f.ChkStatus == 0
                                 select new { emp = g.FirstName }
                                 ).Distinct().ToList(),
                         b.BankName,
                         b.AccountNo,
                         b.IbanNo,
                         b.BranchName,
                         b.Swift,

                         e.CreatedDate,
                         leadstatus = cc.StatusType,
                         sourceofleads = c.SrcName
                     }).OrderBy(a => a.CreatedDate);


            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        #endregion

        #region pipeline search
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report PipeLine")]
        public ActionResult PipeLine()
        {

            ViewBag.SalesExec = QkSelect.List(
                        new List<SelectListItem>
                        {
                                                new SelectListItem { Selected = false, Text = "All", Value = "0"},
                        }, "Value", "Text", 1);

            ViewBag.SrcLead = QkSelect.List(
                      new List<SelectListItem>
                      {
                                                new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Report PipeLine")]
        public ActionResult GetPipeLine(long? salesexec, long? srclead, string phno, string mob, decimal? climit, int? cperiod, string pptype, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

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


            var v = (from b in db.Customers
                     join c in db.SourceOfLeads on b.SourceOfLead equals c.SourceOfLeadId into src
                     from c in src.DefaultIfEmpty()
                     join d in db.Contacts on b.Contact equals d.ContactID into cont
                     from d in cont.DefaultIfEmpty()
                     join e in db.CustomerConversions on b.CustomerID equals e.CustomerID into con
                     from e in con.DefaultIfEmpty()
                     join f in db.Employees on b.SalesPerson equals f.EmployeeId into emp
                     from f in emp.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where b.Type == CRMCustomerType.PipeLine && e.Type == CRMCustomerType.PipeLine &&
                           (salesexec == null || salesexec == 0 || b.SalesPerson == salesexec) &&
                           (srclead == 0 || b.SourceOfLead == srclead) &&
                           (phno == "" || d.Phone == phno) &&
                           (mob == "" || d.Mobile == mob) &&
                           (climit == 0 || climit == null || b.CreditLimit == climit) &&
                           (cperiod == 0 || cperiod == null || b.CreditPeriod == cperiod) &&
                           (pptype == "" || e.Remarks == pptype) &&

                           (fromdate == "" || EF.Functions.DateDiffDay(e.CreatedDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(e.CreatedDate, tdate) >= 0)
                     select new
                     {
                         b.CustomerID,
                         b.CustomerCode,
                         b.CustomerName,
                         TaxRegNo = i.TRN,
                         b.CreditPeriod,
                         b.CreditLimit,
                         b.Location,
                         SalesPerson = f.FirstName + " " + f.LastName,
                         c.SrcName,
                         b.Remark,
                         Details = d.Address + "<br/>" + d.City + " " + d.State + "-" + d.Country + " " + d.Zip,
                         Phone = d.Phone + "<br/>" + d.Mobile,
                         d.Fax,
                         d.EmailId,
                         d.Reference,
                         d.ContactPerson,

                         b.BankName,
                         b.AccountNo,
                         b.IbanNo,
                         b.BranchName,
                         b.Swift,

                         ConvertFrom = e.ConvertFrom,
                         e.CreatedDate
                     }).OrderBy(a => a.CreatedDate);


            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        #endregion

        #region customer search
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report Customer")]
        public ActionResult Customer()
        {

            ViewBag.SalesExec = QkSelect.List(
                        new List<SelectListItem>
                        {
                                                new SelectListItem { Selected = false, Text = "All", Value = "0"},
                        }, "Value", "Text", 1);

            ViewBag.SrcLead = QkSelect.List(
                      new List<SelectListItem>
                      {
                                                new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Report Customer")]
        public ActionResult GetCustomer(long? salesexec, long? srclead, string phno, string mob, decimal? climit, int? cperiod, string pptype, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

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


            var v = (from b in db.Customers
                     join c in db.SourceOfLeads on b.SourceOfLead equals c.SourceOfLeadId into src
                     from c in src.DefaultIfEmpty()
                     join d in db.Contacts on b.Contact equals d.ContactID into cont
                     from d in cont.DefaultIfEmpty()
                     join e in db.CustomerConversions on b.CustomerID equals e.CustomerID into con
                     from e in con.DefaultIfEmpty()
                     join f in db.Employees on b.SalesPerson equals f.EmployeeId into emp
                     from f in emp.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join j in db.Mobiles on b.Contact equals j.Contact into mobi
                     from j in mobi.DefaultIfEmpty()
                     where b.Type == CRMCustomerType.Customer && e.Type == CRMCustomerType.Customer &&
                           (salesexec == null || salesexec == 0 || b.SalesPerson == salesexec) &&
                           (srclead == 0 || b.SourceOfLead == srclead) &&
                           (phno == "" || d.Phone == phno) &&
                           (mob == "" || j.MobileNum == mob) &&
                           (climit == 0 || climit == null || b.CreditLimit == climit) &&
                           (cperiod == 0 || cperiod == null || b.CreditPeriod == cperiod) &&
                           (pptype == "" || e.ConvertFrom == pptype) &&

                           (fromdate == "" || EF.Functions.DateDiffDay(e.CreatedDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(e.CreatedDate, tdate) >= 0)
                     select new
                     {
                         b.CustomerID,
                         b.CustomerCode,
                         b.CustomerName,
                         TaxRegNo = i.TRN,
                         b.CreditPeriod,
                         b.CreditLimit,
                         b.Location,
                         SalesPerson = f.FirstName + " " + f.LastName,
                         c.SrcName,
                         b.Remark,
                         Details = d.Address + "<br/>" + d.City + " " + d.State + "-" + d.Country + " " + d.Zip,
                         Phone = d.Phone + "<br/>" + d.Mobile,

                         d.Fax,
                         d.EmailId,
                         d.Reference,
                         d.ContactPerson,

                         b.BankName,
                         b.AccountNo,
                         b.IbanNo,
                         b.BranchName,
                         b.Swift,

                         ConvertFrom = e.ConvertFrom,
                         e.CreatedDate
                     }).OrderBy(a => a.CreatedDate);


            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        #endregion

        #region approval report
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report ApprovalHistory")]
        public ActionResult Approval()
        {

            ViewBag.AppStat = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= ""},
                new SelectListItem() {Text = "Approved", Value="0"},
                new SelectListItem() {Text = "Rejected", Value="1"},
                new SelectListItem() {Text = "PendingApproval", Value="2"},
            }, "Value", "Text");

            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Types = OptAll;

            companySet();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Report ApprovalHistory")]
        public ActionResult GetApproval(string FromDate, string ToDate, string invoice, string type, long? appstat, bool sortfive)
        {
            int DateDiffDayClient(System.DateTime? s, System.DateTime? e) => (s.HasValue && e.HasValue) ? (int)(e.Value.Date - s.Value.Date).TotalDays : 0;
            var UserId = User.Identity.GetUserId();

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "" && FromDate != null)
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "" && ToDate != null)
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var MlaSEntry = db.EnableSettings.Where(a => a.EnableType == "MLASEntry").FirstOrDefault();
            var MlaSEntrys = MlaSEntry != null ? (MlaSEntry.Status == Status.active ? 1 : 0) : 0;
            var SEdit = User.IsInRole("Edit Sales Entry");

            var sale = (from a in db.SalesEntrys.Where(a => db.Approvals.Any(x => x.TransEntry == a.SalesEntryId && x.Type == "SalesEntry")).AsEnumerable()
                        let app = db.Approvals.Where(x => x.TransEntry == a.SalesEntryId && x.Type == "SalesEntry").Select(x => x.EmployeeId).ToList()
                        let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.SalesEntryId && x.Type == "SalesEntry").Select(x => x.ApprovalStatus).ToList()
                        let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.SalesEntryId && x.Type == "SalesEntry").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                           .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                           .ToList().Select(a => a.ApprovalStatus).ToList()
                        where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.SEDate, fdate) <= 0) &&
                        (ToDate == "" || DateDiffDayClient(a.SEDate, tdate) >= 0)
                        select new
                        {
                            Id = a.SalesEntryId,
                            BillNo = a.BillNo,
                            SDate = a.SEDate,
                            Type = "SalesEntry",
                            CreatedDate = a.SECreatedDate,
                            app = app,
                            AppStatus = AppStatus,
                            chkAppStatus = chkAppStatus,
                        }).ToList().Select(o => new
                        {
                            o.Id,
                            o.BillNo,
                            o.SDate,
                            o.Type,
                            o.CreatedDate,
                            ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                            Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                            StatPer = MlaSEntrys,
                            EditPer = SEdit,
                            Editpath = "CreditSale"
                        });

            var MlaSReturn = db.EnableSettings.Where(a => a.EnableType == "MLASReturn").FirstOrDefault();
            var MlaSReturns = MlaSReturn != null ? (MlaSReturn.Status == Status.active ? 1 : 0) : 0;
            var SREdit = User.IsInRole("Edit Sales Return");

            var saleret = (from a in db.SalesReturns.Where(a => db.Approvals.Any(x => x.TransEntry == a.SalesReturnId && x.Type == "SalesReturn")).AsEnumerable()
                           let app = db.Approvals.Where(x => x.TransEntry == a.SalesReturnId && x.Type == "SalesReturn").Select(x => x.EmployeeId).ToList()
                           let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.SalesReturnId && x.Type == "SalesReturn").Select(x => x.ApprovalStatus).ToList()
                           let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.SalesReturnId && x.Type == "SalesReturn").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                              .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                              .ToList().Select(a => a.ApprovalStatus).ToList()
                           where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.SRDate, fdate) <= 0) &&
                           (ToDate == "" || DateDiffDayClient(a.SRDate, tdate) >= 0)
                           select new
                           {
                               Id = a.SalesReturnId,
                               BillNo = a.BillNo,
                               SDate = a.SRDate,
                               Type = "SalesReturn",
                               CreatedDate = a.SRCreatedDate,
                               app = app,
                               AppStatus = AppStatus,
                               chkAppStatus = chkAppStatus,
                           }).ToList().Select(o => new
                           {
                               o.Id,
                               o.BillNo,
                               o.SDate,
                               o.Type,
                               o.CreatedDate,
                               ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                               Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                               StatPer = MlaSReturns,
                               EditPer = SREdit,
                               Editpath = "CreditSaleReturn"
                           });

            var MlaDNote = db.EnableSettings.Where(a => a.EnableType == "MLADNote").FirstOrDefault();
            var MlaDNotes = MlaDNote != null ? (MlaDNote.Status == Status.active ? 1 : 0) : 0;
            var DVEdit = User.IsInRole("Edit Deliverynote");

            var dvnote = (from a in db.Deliverynotes.Where(a => db.Approvals.Any(x => x.TransEntry == a.DeliverynoteId && x.Type == "Deliverynote")).AsEnumerable()
                          let app = db.Approvals.Where(x => x.TransEntry == a.DeliverynoteId && x.Type == "Deliverynote").Select(x => x.EmployeeId).ToList()
                          let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.DeliverynoteId && x.Type == "Deliverynote").Select(x => x.ApprovalStatus).ToList()
                          let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.DeliverynoteId && x.Type == "Deliverynote").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                             .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                             .ToList().Select(a => a.ApprovalStatus).ToList()
                          where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.DvDate, fdate) <= 0) &&
                          (ToDate == "" || DateDiffDayClient(a.DvDate, tdate) >= 0)
                          select new
                          {
                              Id = a.DeliverynoteId,
                              BillNo = a.BillNo,
                              SDate = a.DvDate,
                              Type = "Deliverynote",
                              CreatedDate = a.DvCreatedDate,
                              app = app,
                              AppStatus = AppStatus,
                              chkAppStatus = chkAppStatus,
                          }).ToList().Select(o => new
                          {
                              o.Id,
                              o.BillNo,
                              o.SDate,
                              o.Type,
                              o.CreatedDate,
                              ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                              Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                              StatPer = MlaDNotes,
                              EditPer = DVEdit,
                              Editpath = "Deliverynote"
                          });

            var MlaHReturn = db.EnableSettings.Where(a => a.EnableType == "MLAHReturn").FirstOrDefault();
            var MlaHReturns = MlaHReturn != null ? (MlaHReturn.Status == Status.active ? 1 : 0) : 0;
            var HREdit = User.IsInRole("Edit HireReturn");

            var hreturn = (from a in db.HireReturns.Where(a => db.Approvals.Any(x => x.TransEntry == a.HireReturnId && x.Type == "HireReturn")).AsEnumerable()
                           let app = db.Approvals.Where(x => x.TransEntry == a.HireReturnId && x.Type == "HireReturn").Select(x => x.EmployeeId).ToList()
                           let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.HireReturnId && x.Type == "HireReturn").Select(x => x.ApprovalStatus).ToList()
                           let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.HireReturnId && x.Type == "HireReturn").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                              .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                              .ToList().Select(a => a.ApprovalStatus).ToList()
                           where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.Date, fdate) <= 0) &&
                           (ToDate == "" || DateDiffDayClient(a.Date, tdate) >= 0)
                           select new
                           {
                               Id = a.HireReturnId,
                               BillNo = a.BillNo,
                               SDate = a.Date,
                               Type = "HireReturn",
                               CreatedDate = a.CreatedDate,
                               app = app,
                               AppStatus = AppStatus,
                               chkAppStatus = chkAppStatus,
                           }).ToList().Select(o => new
                           {
                               o.Id,
                               o.BillNo,
                               o.SDate,
                               o.Type,
                               o.CreatedDate,
                               ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                               Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                               StatPer = MlaHReturns,
                               EditPer = HREdit,
                               Editpath = "HireReturn"
                           });

            var MlaJCard = db.EnableSettings.Where(a => a.EnableType == "MLAJCard").FirstOrDefault();
            var MlaJCards = MlaJCard != null ? (MlaJCard.Status == Status.active ? 1 : 0) : 0;
            var JBEdit = User.IsInRole("Edit JobCard");

            var jbcard = (from a in db.JobCards.Where(a => db.Approvals.Any(x => x.TransEntry == a.JobCardId && x.Type == "JobCard")).AsEnumerable()
                          let app = db.Approvals.Where(x => x.TransEntry == a.JobCardId && x.Type == "JobCard").Select(x => x.EmployeeId).ToList()
                          let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.JobCardId && x.Type == "JobCard").Select(x => x.ApprovalStatus).ToList()
                          let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.JobCardId && x.Type == "JobCard").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                             .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                             .ToList().Select(a => a.ApprovalStatus).ToList()
                          where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.JCDate, fdate) <= 0) &&
                          (ToDate == "" || DateDiffDayClient(a.JCDate, tdate) >= 0)
                          select new
                          {
                              Id = a.JobCardId,
                              BillNo = a.JobCardNo,
                              SDate = a.JCDate,
                              Type = "JobCard",
                              CreatedDate = a.CreatedDate,
                              app = app,
                              AppStatus = AppStatus,
                              chkAppStatus = chkAppStatus,
                          }).ToList().Select(o => new
                          {
                              o.Id,
                              o.BillNo,
                              o.SDate,
                              o.Type,
                              o.CreatedDate,
                              ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                              Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                              StatPer = MlaJCards,
                              EditPer = JBEdit,
                              Editpath = "JobCard"
                          });

            var MlaMc = db.EnableSettings.Where(a => a.EnableType == "MLAMc").FirstOrDefault();
            var MlaMcs = MlaMc != null ? (MlaMc.Status == Status.active ? 1 : 0) : 0;
            var MREdit = User.IsInRole("Edit MaterialRequisition");

            var mr = (from a in db.MaterialRequisitions.Where(a => db.Approvals.Any(x => x.TransEntry == a.MaterialRequisitionId && x.Type == "MaterialRequisition")).AsEnumerable()
                      let app = db.Approvals.Where(x => x.TransEntry == a.MaterialRequisitionId && x.Type == "MaterialRequisition").Select(x => x.EmployeeId).ToList()
                      let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.MaterialRequisitionId && x.Type == "MaterialRequisition").Select(x => x.ApprovalStatus).ToList()
                      let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.MaterialRequisitionId && x.Type == "MaterialRequisition").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                         .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                         .ToList().Select(a => a.ApprovalStatus).ToList()
                      where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.MRDate, fdate) <= 0) &&
                      (ToDate == "" || DateDiffDayClient(a.MRDate, tdate) >= 0)
                      select new
                      {
                          Id = a.MaterialRequisitionId,
                          BillNo = a.BillNo,
                          SDate = a.MRDate,
                          Type = "MaterialRequisition",
                          CreatedDate = a.MRCreatedDate,
                          app = app,
                          AppStatus = AppStatus,
                          chkAppStatus = chkAppStatus,
                      }).ToList().Select(o => new
                      {
                          o.Id,
                          o.BillNo,
                          o.SDate,
                          o.Type,
                          o.CreatedDate,
                          ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                          Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                          StatPer = MlaMcs,
                          EditPer = MREdit,
                          Editpath = "MaterialRequisition"
                      });

            var MlaMRNote = db.EnableSettings.Where(a => a.EnableType == "MLAMRNote").FirstOrDefault();
            var MlaMRNotes = MlaMRNote != null ? (MlaMRNote.Status == Status.active ? 1 : 0) : 0;
            var MRNEdit = User.IsInRole("Edit MRNote");

            var mrnote = (from a in db.MaterialReceiveNotes.Where(a => db.Approvals.Any(x => x.TransEntry == a.MRId && x.Type == "MRNote")).AsEnumerable()
                          let app = db.Approvals.Where(x => x.TransEntry == a.MRId && x.Type == "MRNote").Select(x => x.EmployeeId).ToList()
                          let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.MRId && x.Type == "MRNote").Select(x => x.ApprovalStatus).ToList()
                          let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.MRId && x.Type == "MRNote").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                             .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                             .ToList().Select(a => a.ApprovalStatus).ToList()
                          where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.MRDate, fdate) <= 0) &&
                          (ToDate == "" || DateDiffDayClient(a.MRDate, tdate) >= 0)
                          select new
                          {
                              Id = a.MRId,
                              BillNo = a.BillNo,
                              SDate = a.MRDate,
                              Type = "MRNote",
                              CreatedDate = a.CreatedDate,
                              app = app,
                              AppStatus = AppStatus,
                              chkAppStatus = chkAppStatus,
                          }).ToList().Select(o => new
                          {
                              o.Id,
                              o.BillNo,
                              o.SDate,
                              o.Type,
                              o.CreatedDate,
                              ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                              Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                              StatPer = MlaMRNotes,
                              EditPer = MRNEdit,
                              Editpath = "MRNote"
                          });


            var MlaPList = db.EnableSettings.Where(a => a.EnableType == "MLAPList").FirstOrDefault();
            var MlaPLists = MlaPList != null ? (MlaPList.Status == Status.active ? 1 : 0) : 0;
            var PKEdit = User.IsInRole("Edit PackingList");

            var pklist = (from a in db.PackingLists.Where(a => db.Approvals.Any(x => x.TransEntry == a.PackinglistId && x.Type == "PackingList")).AsEnumerable()
                          let app = db.Approvals.Where(x => x.TransEntry == a.PackinglistId && x.Type == "PackingList").Select(x => x.EmployeeId).ToList()
                          let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PackinglistId && x.Type == "PackingList").Select(x => x.ApprovalStatus).ToList()
                          let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PackinglistId && x.Type == "PackingList").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                             .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                             .ToList().Select(a => a.ApprovalStatus).ToList()
                          where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.PLDate, fdate) <= 0) &&
                          (ToDate == "" || DateDiffDayClient(a.PLDate, tdate) >= 0)
                          select new
                          {
                              Id = a.PackinglistId,
                              BillNo = a.BillNo,
                              SDate = a.PLDate,
                              Type = "PackingList",
                              CreatedDate = a.CreatedDate,
                              app = app,
                              AppStatus = AppStatus,
                              chkAppStatus = chkAppStatus,
                          }).ToList().Select(o => new
                          {
                              o.Id,
                              o.BillNo,
                              o.SDate,
                              o.Type,
                              o.CreatedDate,
                              ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                              Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                              StatPer = MlaPLists,
                              EditPer = PKEdit,
                              Editpath = "PackingList"
                          });

            var MlaProd = db.EnableSettings.Where(a => a.EnableType == "MLAProd").FirstOrDefault();
            var MlaProds = MlaProd != null ? (MlaProd.Status == Status.active ? 1 : 0) : 0;
            var PREdit = User.IsInRole("Edit Production");

            // Project only the columns this report actually uses BEFORE switching to client evaluation, so the
            // materialisation doesn't SELECT * the whole Production entity for every approved production row.
            var prod = (from a in db.Productions.Where(a => db.Approvals.Any(x => x.TransEntry == a.ProductionId && x.Type == "Production"))
                            .Select(a => new { a.ProductionId, a.VoucherNo, a.PEDate, a.CreatedDate }).AsEnumerable()
                        let app = db.Approvals.Where(x => x.TransEntry == a.ProductionId && x.Type == "Production").Select(x => x.EmployeeId).ToList()
                        let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.ProductionId && x.Type == "Production").Select(x => x.ApprovalStatus).ToList()
                        let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.ProductionId && x.Type == "Production").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                           .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                           .ToList().Select(a => a.ApprovalStatus).ToList()
                        where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.PEDate, fdate) <= 0) &&
                         (ToDate == "" || DateDiffDayClient(a.PEDate, tdate) >= 0)
                        select new
                        {
                            Id = a.ProductionId,
                            BillNo = a.VoucherNo,
                            SDate = a.PEDate,
                            Type = "Production",
                            CreatedDate = a.CreatedDate,
                            app = app,
                            AppStatus = AppStatus,
                            chkAppStatus = chkAppStatus,
                        }).ToList().Select(o => new
                        {
                            o.Id,
                            o.BillNo,
                            o.SDate,
                            o.Type,
                            o.CreatedDate,
                            ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                            Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                            StatPer = MlaProds,
                            EditPer = PREdit,
                            Editpath = "Production"
                        });

            var MlaPForma = db.EnableSettings.Where(a => a.EnableType == "MLAPForma ").FirstOrDefault();
            var MlaPFormas = MlaPForma != null ? (MlaPForma.Status == Status.active ? 1 : 0) : 0;
            var PFEdit = User.IsInRole("Edit Pro Forma");

            var pforma = (from a in db.ProFormas.Where(a => db.Approvals.Any(x => x.TransEntry == a.ProFormaId && x.Type == "ProForma")).AsEnumerable()
                          let app = db.Approvals.Where(x => x.TransEntry == a.ProFormaId && x.Type == "ProForma").Select(x => x.EmployeeId).ToList()
                          let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.ProFormaId && x.Type == "ProForma").Select(x => x.ApprovalStatus).ToList()
                          let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.ProFormaId && x.Type == "ProForma").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                             .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                             .ToList().Select(a => a.ApprovalStatus).ToList()
                          where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.PFDate, fdate) <= 0) &&
                          (ToDate == "" || DateDiffDayClient(a.PFDate, tdate) >= 0)
                          select new
                          {
                              Id = a.ProFormaId,
                              BillNo = a.BillNo,
                              SDate = a.PFDate,
                              Type = "ProForma",
                              CreatedDate = a.PFCreatedDate,
                              app = app,
                              AppStatus = AppStatus,
                              chkAppStatus = chkAppStatus,
                          }).ToList().Select(o => new
                          {
                              o.Id,
                              o.BillNo,
                              o.SDate,
                              o.Type,
                              o.CreatedDate,
                              ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                              Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                              StatPer = MlaPFormas,
                              EditPer = PFEdit,
                              Editpath = "ProForma"
                          });

            var MlaPEntry = db.EnableSettings.Where(a => a.EnableType == "MLAPEntry").FirstOrDefault();
            var MlaPEntrys = MlaPEntry != null ? (MlaPEntry.Status == Status.active ? 1 : 0) : 0;
            var PEdit = User.IsInRole("Edit Purchase Entry");

            var purchase = (from a in db.PurchaseEntrys.Where(a => db.Approvals.Any(x => x.TransEntry == a.PurchaseEntryId && x.Type == "PurchaseEntry")).AsEnumerable()
                            let app = db.Approvals.Where(x => x.TransEntry == a.PurchaseEntryId && x.Type == "PurchaseEntry").Select(x => x.EmployeeId).ToList()
                            let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseEntryId && x.Type == "PurchaseEntry").Select(x => x.ApprovalStatus).ToList()
                            let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseEntryId && x.Type == "PurchaseEntry").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                               .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                               .ToList().Select(a => a.ApprovalStatus).ToList()
                            where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.PEDate, fdate) <= 0) &&
                            (ToDate == "" || DateDiffDayClient(a.PEDate, tdate) >= 0)
                            select new
                            {
                                Id = a.PurchaseEntryId,
                                BillNo = a.BillNo,
                                SDate = a.PEDate,
                                Type = "PurchaseEntry",
                                CreatedDate = a.PECreatedDate,
                                app = app,
                                AppStatus = AppStatus,
                                chkAppStatus = chkAppStatus,
                            }).ToList().Select(o => new
                            {
                                o.Id,
                                o.BillNo,
                                o.SDate,
                                o.Type,
                                o.CreatedDate,
                                ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                                Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                                StatPer = MlaPEntrys,
                                EditPer = PEdit,
                                Editpath = "PurchaseEntry"
                            });

            var MlaPOrder = db.EnableSettings.Where(a => a.EnableType == "MLAPOrder").FirstOrDefault();
            var MlaPOrders = MlaPOrder != null ? (MlaPOrder.Status == Status.active ? 1 : 0) : 0;
            var POEdit = User.IsInRole("Edit PurchaseOrder");

            var porder = (from a in db.PurchaseOrders.Where(a => db.Approvals.Any(x => x.TransEntry == a.PurchaseOrderId && x.Type == "PurchaseOrder")).AsEnumerable()
                          let app = db.Approvals.Where(x => x.TransEntry == a.PurchaseOrderId && x.Type == "PurchaseOrder").Select(x => x.EmployeeId).ToList()
                          let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseOrderId && x.Type == "PurchaseOrder").Select(x => x.ApprovalStatus).ToList()
                          let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseOrderId && x.Type == "PurchaseOrder").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                             .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                             .ToList().Select(a => a.ApprovalStatus).ToList()
                          where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.PODate, fdate) <= 0) &&
                          (ToDate == "" || DateDiffDayClient(a.PODate, tdate) >= 0)
                          select new
                          {
                              Id = a.PurchaseOrderId,
                              BillNo = a.BillNo,
                              SDate = a.PODate,
                              Type = "PurchaseOrder",
                              CreatedDate = a.POCreatedDate,
                              app = app,
                              AppStatus = AppStatus,
                              chkAppStatus = chkAppStatus,
                          }).ToList().Select(o => new
                          {
                              o.Id,
                              o.BillNo,
                              o.SDate,
                              o.Type,
                              o.CreatedDate,
                              ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                              Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                              StatPer = MlaPOrders,
                              EditPer = POEdit,
                              Editpath = "PurchaseOrder"
                          });

            var MlaPQuot = db.EnableSettings.Where(a => a.EnableType == "MLAPQuot").FirstOrDefault();
            var MlaPQuots = MlaPQuot != null ? (MlaPQuot.Status == Status.active ? 1 : 0) : 0;
            var PQEdit = User.IsInRole("Edit PurchaseQuotation");

            var pquot = (from a in db.PurchaseQuotations.Where(a => db.Approvals.Any(x => x.TransEntry == a.PQuotationId && x.Type == "PurchaseQuotation")).AsEnumerable()
                         let app = db.Approvals.Where(x => x.TransEntry == a.PQuotationId && x.Type == "PurchaseQuotation").Select(x => x.EmployeeId).ToList()
                         let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PQuotationId && x.Type == "PurchaseQuotation").Select(x => x.ApprovalStatus).ToList()
                         let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PQuotationId && x.Type == "PurchaseQuotation").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                            .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                            .ToList().Select(a => a.ApprovalStatus).ToList()
                         where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.PQuotDate, fdate) <= 0) &&
                         (ToDate == "" || DateDiffDayClient(a.PQuotDate, tdate) >= 0)
                         select new
                         {
                             Id = a.PQuotationId,
                             BillNo = a.BillNo,
                             SDate = a.PQuotDate,
                             Type = "PurchaseQuotation",
                             CreatedDate = a.PQuotCreatedDate,
                             app = app,
                             AppStatus = AppStatus,
                             chkAppStatus = chkAppStatus,
                         }).ToList().Select(o => new
                         {
                             o.Id,
                             o.BillNo,
                             o.SDate,
                             o.Type,
                             o.CreatedDate,
                             ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                             Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                             StatPer = MlaPQuots,
                             EditPer = PQEdit,
                             Editpath = "PurchaseQuotation"
                         });

            var MlaPReturn = db.EnableSettings.Where(a => a.EnableType == "MLAPReturn").FirstOrDefault();
            var MlaPReturns = MlaPReturn != null ? (MlaPReturn.Status == Status.active ? 1 : 0) : 0;
            var PREEdit = User.IsInRole("Edit Purchase Return");

            var preturn = (from a in db.PurchaseReturns.Where(a => db.Approvals.Any(x => x.TransEntry == a.PurchaseReturnId && x.Type == "PurchaseReturn")).AsEnumerable()
                           let app = db.Approvals.Where(x => x.TransEntry == a.PurchaseReturnId && x.Type == "PurchaseReturn").Select(x => x.EmployeeId).ToList()
                           let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseReturnId && x.Type == "PurchaseReturn").Select(x => x.ApprovalStatus).ToList()
                           let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseReturnId && x.Type == "PurchaseReturn").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                              .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                              .ToList().Select(a => a.ApprovalStatus).ToList()
                           where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.PRDate, fdate) <= 0) &&
                            (ToDate == "" || DateDiffDayClient(a.PRDate, tdate) >= 0)
                           select new
                           {
                               Id = a.PurchaseReturnId,
                               BillNo = a.BillNo,
                               SDate = a.PRDate,
                               Type = "PurchaseReturn",
                               CreatedDate = a.PRCreatedDate,
                               app = app,
                               AppStatus = AppStatus,
                               chkAppStatus = chkAppStatus,
                           }).ToList().Select(o => new
                           {
                               o.Id,
                               o.BillNo,
                               o.SDate,
                               o.Type,
                               o.CreatedDate,
                               ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                               Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                               StatPer = MlaPReturns,
                               EditPer = PREEdit,
                               Editpath = "PurchaseReturn"
                           });

            var MlaQuot = db.EnableSettings.Where(a => a.EnableType == "MLAQuot").FirstOrDefault();
            var MlaQuots = MlaQuot != null ? (MlaQuot.Status == Status.active ? 1 : 0) : 0;
            var QEdit = User.IsInRole("Edit Quotation");

            var quot = (from a in db.Quotations.Where(a => db.Approvals.Any(x => x.TransEntry == a.QuotationId && x.Type == "Quotation")).AsEnumerable()
                        let app = db.Approvals.Where(x => x.TransEntry == a.QuotationId && x.Type == "Quotation").Select(x => x.EmployeeId).ToList()
                        let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.QuotationId && x.Type == "Quotation").Select(x => x.ApprovalStatus).ToList()
                        let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.QuotationId && x.Type == "Quotation").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                           .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                           .ToList().Select(a => a.ApprovalStatus).ToList()
                        where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.QuotDate, fdate) <= 0) &&
                        (ToDate == "" || DateDiffDayClient(a.QuotDate, tdate) >= 0)
                        select new
                        {
                            Id = a.QuotationId,
                            BillNo = a.BillNo,
                            SDate = a.QuotDate,
                            Type = "Quotation",
                            CreatedDate = a.QuotCreatedDate,
                            app = app,
                            AppStatus = AppStatus,
                            chkAppStatus = chkAppStatus,
                        }).ToList().Select(o => new
                        {
                            o.Id,
                            o.BillNo,
                            o.SDate,
                            o.Type,
                            o.CreatedDate,
                            ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                            Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                            StatPer = MlaQuots,
                            EditPer = QEdit,
                            Editpath = "Quotation"
                        });

            var MlaSOrder = db.EnableSettings.Where(a => a.EnableType == "MLASOrder").FirstOrDefault();
            var MlaSOrders = MlaSOrder != null ? (MlaSOrder.Status == Status.active ? 1 : 0) : 0;
            var SOEdit = User.IsInRole("Edit SalesOrder");

            var sorder = (from a in db.SalesOrders.Where(a => db.Approvals.Any(x => x.TransEntry == a.SalesOrderId && x.Type == "SalesOrder")).AsEnumerable()
                          let app = db.Approvals.Where(x => x.TransEntry == a.SalesOrderId && x.Type == "SalesOrder").Select(x => x.EmployeeId).ToList()
                          let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.SalesOrderId && x.Type == "SalesOrder").Select(x => x.ApprovalStatus).ToList()
                          let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.SalesOrderId && x.Type == "SalesOrder").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                             .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                             .ToList().Select(a => a.ApprovalStatus).ToList()
                          where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.SODate, fdate) <= 0) &&
                            (ToDate == "" || DateDiffDayClient(a.SODate, tdate) >= 0)
                          select new
                          {
                              Id = a.SalesOrderId,
                              BillNo = a.BillNo,
                              SDate = a.SODate,
                              Type = "SalesOrder",
                              CreatedDate = a.SOCreatedDate,
                              app = app,
                              AppStatus = AppStatus,
                              chkAppStatus = chkAppStatus,
                          }).ToList().Select(o => new
                          {
                              o.Id,
                              o.BillNo,
                              o.SDate,
                              o.Type,
                              o.CreatedDate,
                              ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                              Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                              StatPer = MlaSOrders,
                              EditPer = SOEdit,
                              Editpath = "SalesOrder"
                          });

            var MlaSJour = db.EnableSettings.Where(a => a.EnableType == "MLASJour").FirstOrDefault();
            var MlaSJours = MlaSJour != null ? (MlaSJour.Status == Status.active ? 1 : 0) : 0;
            var SJEdit = User.IsInRole("Edit StockJournal");

            var stkjnl = (from a in db.StockJournals.Where(a => db.Approvals.Any(x => x.TransEntry == a.Id && x.Type == "StockJournal")).AsEnumerable()
                          let app = db.Approvals.Where(x => x.TransEntry == a.Id && x.Type == "StockJournal").Select(x => x.EmployeeId).ToList()
                          let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.Id && x.Type == "StockJournal").Select(x => x.ApprovalStatus).ToList()
                          let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.Id && x.Type == "StockJournal").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                             .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                             .ToList().Select(a => a.ApprovalStatus).ToList()
                          where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.SJDate, fdate) <= 0) &&
                           (ToDate == "" || DateDiffDayClient(a.SJDate, tdate) >= 0)
                          select new
                          {
                              a.Id,
                              BillNo = a.Voucher,
                              SDate = a.SJDate,
                              Type = "StockJournal",
                              CreatedDate = a.CreatedDate,
                              app = app,
                              AppStatus = AppStatus,
                              chkAppStatus = chkAppStatus,
                          }).ToList().Select(o => new
                          {
                              o.Id,
                              o.BillNo,
                              o.SDate,
                              o.Type,
                              o.CreatedDate,
                              ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                              Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                              StatPer = MlaSJours,
                              EditPer = SJEdit,
                              Editpath = "StockJournal"
                          });

            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? (MlaSTran.Status == Status.active ? 1 : 0) : 0;
            var STEdit = User.IsInRole("Edit StockTransfer");

            var stktrns = (from a in db.StockTransfers.Where(a => db.Approvals.Any(x => x.TransEntry == a.Id && x.Type == "StockTransfer")).AsEnumerable()
                           let app = db.Approvals.Where(x => x.TransEntry == a.Id && x.Type == "StockTransfer").Select(x => x.EmployeeId).ToList()
                           let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.Id && x.Type == "StockTransfer").Select(x => x.ApprovalStatus).ToList()
                           let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.Id && x.Type == "StockTransfer").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                              .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                              .ToList().Select(a => a.ApprovalStatus).ToList()
                           where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.Date, fdate) <= 0) &&
                           (ToDate == "" || DateDiffDayClient(a.Date, tdate) >= 0)
                           select new
                           {
                               a.Id,
                               BillNo = a.Voucher,
                               SDate = a.Date,
                               Type = "StockTransfer",
                               CreatedDate = a.CreatedDate,
                               app = app,
                               AppStatus = AppStatus,
                               chkAppStatus = chkAppStatus,
                           }).ToList().Select(o => new
                           {
                               o.Id,
                               o.BillNo,
                               o.SDate,
                               o.Type,
                               o.CreatedDate,
                               ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                               Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                               StatPer = MlaSTrans,
                               EditPer = STEdit,
                               Editpath = "StockTransfer"
                           });


            var MlaUAssem = db.EnableSettings.Where(a => a.EnableType == "MLAUAssem").FirstOrDefault();
            var MlaUAssems = MlaUAssem != null ? (MlaUAssem.Status == Status.active ? 1 : 0) : 0;
            var UEdit = User.IsInRole("Edit Unassemble");

            var unass = (from a in db.Unassembles.Where(a => db.Approvals.Any(x => x.TransEntry == a.UnassembleId && x.Type == "Unassemble")).AsEnumerable()
                         let app = db.Approvals.Where(x => x.TransEntry == a.UnassembleId && x.Type == "Unassemble").Select(x => x.EmployeeId).ToList()
                         let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.UnassembleId && x.Type == "Unassemble").Select(x => x.ApprovalStatus).ToList()
                         let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.UnassembleId && x.Type == "Unassemble").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                            .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                            .ToList().Select(a => a.ApprovalStatus).ToList()
                         where app.Count() > 0 && (FromDate == "" || DateDiffDayClient(a.PEDate, fdate) <= 0) &&
                         (ToDate == "" || DateDiffDayClient(a.PEDate, tdate) >= 0)
                         select new
                         {
                             Id = a.UnassembleId,
                             BillNo = a.VoucherNo,
                             SDate = a.PEDate,
                             Type = "Unassemble",
                             CreatedDate = a.CreatedDate,
                             app = app,
                             AppStatus = AppStatus,
                             chkAppStatus = chkAppStatus,
                         }).ToList().Select(o => new
                         {
                             o.Id,
                             o.BillNo,
                             o.SDate,
                             o.Type,
                             o.CreatedDate,
                             ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                             Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                             StatPer = MlaUAssems,
                             EditPer = UEdit,
                             Editpath = "Unassemble"
                         });


            var v = sale;//.Union(saleret);
            v = v.Union(dvnote);
            v = v.Union(hreturn);
            v = v.Union(jbcard);
            v = v.Union(mr);
            v = v.Union(mrnote);
            v = v.Union(pklist);
            v = v.Union(prod);
            v = v.Union(pforma);
            v = v.Union(purchase);
            v = v.Union(pquot);
            v = v.Union(porder);
            v = v.Union(preturn);
            v = v.Union(quot);
            v = v.Union(sorder);
            v = v.Union(stkjnl);
            v = v.Union(stktrns);
            v = v.Union(unass);

            v = v.OrderBy(a => a.CreatedDate);

            ApprovalStatus AppSt = new ApprovalStatus();
            if (appstat != null)
            {
                if (appstat == 0)
                {
                    AppSt = ApprovalStatus.Approved;
                }
                else if (appstat == 1)
                {
                    AppSt = ApprovalStatus.Rejected;
                }
                else
                {
                    AppSt = ApprovalStatus.PendingApproval;
                }
            };

            //for home page
            if (sortfive == true)
            {
                v = (from a in v
                     select new
                     {
                         a.Id,
                         a.BillNo,
                         a.SDate,
                         a.Type,
                         a.CreatedDate,
                         a.ApprovalStatus,
                         a.Approval,
                         a.StatPer,
                         a.EditPer,
                         a.Editpath
                     }).OrderByDescending(a => a.CreatedDate).Take(5).ToList();

                var data = v.ToList();
                return Json(new { data = data });
            }
            else
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();

                int recordsTotal = 0;
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                //Find Order Column
                var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

                v = (from a in v
                     where (invoice == "" || invoice == null || a.BillNo == invoice) &&
                           (type == "0" || type == null || a.Type == type) &&
                           (appstat == null || a.ApprovalStatus == AppSt)
                     select new
                     {
                         a.Id,
                         a.BillNo,
                         a.SDate,
                         a.Type,
                         a.CreatedDate,
                         a.ApprovalStatus,
                         a.Approval,
                         a.StatPer,
                         a.EditPer,
                         a.Editpath
                     }).ToList();


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

        }


        public JsonResult SearchAllType(string q, string x)
        {
            List<SelectFormatNew> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Approvals.Where(p => p.Type.Contains(q))
                              .Select(b => new SelectFormatNew
                              {
                                  text = b.Type,
                                  id = b.Type,
                              })
                              .OrderBy(b => b.text).Distinct().ToList();
            }
            else
            {
                serialisedJson = db.Approvals
                              .Select(b => new SelectFormatNew
                              {
                                  text = b.Type,
                                  id = b.Type,
                              })
                              .OrderBy(b => b.text).Distinct().ToList();
            }
            if (x == "all" || (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatNew() { id = "0", text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpGet]
        public ActionResult EditStatus(long id, string type)
        {
            ApprovalUpdate appup = new ApprovalUpdate();
            appup.Type = type;
            appup.TransEntry = id;

            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == type && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            return PartialView(appup);
        }

        [HttpPost]
        public ActionResult EditStatus(ApprovalUpdate App)
        {
            bool stat = false;
            string msg = "";
            var UserId = User.Identity.GetUserId();
            var CreatedBy = "";

            if (App.Type == "MaterialRequisition")
            {
                CreatedBy = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == App.TransEntry).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (App.Type == "Quotation")
            {
                CreatedBy = db.Quotations.Where(a => a.QuotationId == App.TransEntry).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (App.Type == "SalesEntry")
            {
                CreatedBy = db.SalesEntrys.Where(a => a.SalesEntryId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "SalesOrder")
            {
                CreatedBy = db.SalesOrders.Where(a => a.SalesOrderId == App.TransEntry).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (App.Type == "SalesReturn")
            {
                CreatedBy = db.SalesReturns.Where(a => a.SalesReturnId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "PurchaseQuotation")
            {
                CreatedBy = db.PurchaseQuotations.Where(a => a.PQuotationId == App.TransEntry).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (App.Type == "PurchaseEntry")
            {
                CreatedBy = db.PurchaseEntrys.Where(a => a.PurchaseEntryId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "PurchaseOrder")
            {
                CreatedBy = db.PurchaseOrders.Where(a => a.PurchaseOrderId == App.TransEntry).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (App.Type == "PurchaseReturn")
            {
                CreatedBy = db.PurchaseReturns.Where(a => a.PurchaseReturnId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "Deliverynote")
            {
                CreatedBy = db.Deliverynotes.Where(a => a.DeliverynoteId == App.TransEntry).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (App.Type == "JobCard")
            {
                CreatedBy = db.JobCards.Where(a => a.JobCardId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "ProForma")
            {
                CreatedBy = db.ProFormas.Where(a => a.ProFormaId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "StockTransfer")
            {
                CreatedBy = db.StockTransfers.Where(a => a.Id == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "StockJournal")
            {
                CreatedBy = db.StockJournals.Where(a => a.Id == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "PackingList")
            {
                CreatedBy = db.PackingLists.Where(a => a.PackinglistId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "HireReturn")
            {
                CreatedBy = db.HireReturns.Where(a => a.HireReturnId == App.TransEntry).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (App.Type == "MRNote")
            {
                CreatedBy = db.MaterialReceiveNotes.Where(a => a.MRId == App.TransEntry).Select(a => a.CreatedUserId).FirstOrDefault();
            }
            if (App.Type == "Production")
            {
                CreatedBy = db.Productions.Where(a => a.ProductionId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }
            if (App.Type == "Unassemble")
            {
                CreatedBy = db.Unassembles.Where(a => a.UnassembleId == App.TransEntry).Select(a => a.CreatedBy).FirstOrDefault();
            }

            var Createdby = db.ApprovalUpdates.Where(a => a.Type == App.Type && a.TransEntry == App.TransEntry).OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            var chkappby = db.ApprovalUpdates.Where(a => (a.ApprovedBy == UserId) && (a.TransEntry == App.TransEntry) && (a.Type == App.Type)).OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = CreatedBy;
                AppUp.Status = Status.active;
                AppUp.TransEntry = App.TransEntry;
                AppUp.Type = App.Type;

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

        #endregion

        #region Reminder
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Report Reminder")]
        public ActionResult Reminder()
        {
            ViewBag.RType = QkSelect.List(
                             new List<SelectListItem>
                             {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 0);

            return View();
        }
        //[QkAuthorize(Roles = "Dev,Report Reminder")]
        public ActionResult GetReminder(string status, string type, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var UserId = User.Identity.GetUserId();
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

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

            var v = (from a in db.Reminders
                     join b in db.Users on a.CreatedBy equals b.Id into emp
                     from b in emp.DefaultIfEmpty()

                     let remas = db.ReminderAssigneds.Where(x => x.ReminderId == a.ReminderId).Select(x => x.EmployeeId).ToList()
                     where
                     (type == "" || type == "0" || a.Type == type) &&
                     (status == "" || status == "0" || a.RStatus == status) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.RDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.RDate, tdate) >= 0) && remas.Contains(empId)
                     orderby a.RDate descending
                     select new
                     {
                         a.ReminderId,
                         a.CreatedDate,
                         CreatedBy = b.UserName,
                         //RequestBy = c.UserName,
                         a.RDate,
                         a.Note,
                         a.RStatus,
                         a.Type,
                         a.Reference,
                         validity = (DateTime.Now <= a.RDate) ? "Upcoming" : "Expired",
                         AssignedTo = (from aa in db.ReminderAssigneds
                                       join bb in db.Employees on aa.EmployeeId equals bb.EmployeeId into emps
                                       from bb in emps.DefaultIfEmpty()
                                       where aa.ReminderId == a.ReminderId
                                       select new { bb.FirstName, bb.LastName }
                                   ).Distinct().ToList(),
                     });

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
        #endregion

        #region PDC Report

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report PDC")]
        public ActionResult PDC()
        {
            var supparentid = new SqlParameter("@parentid", 8);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var arr = supgpid.ToArray();
            var bank = db.Accountss.Where(p => p.Group == 8 && arr.Contains(p.Group)).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Bank = QkSelect.List(bank, "ID", "Name");

            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Report PDC")]
        public ActionResult GetPDC(long? bank, string vtype, string pdcstat, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            choice pstat = new choice();
            if (pdcstat == "Cleared")
            {
                pstat = choice.Yes;
            }
            if (pdcstat == "Uncleared")
            {
                pstat = choice.No;
            }

            var v = (from a in db.PDCs
                     join b in db.Payments on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = b.PaymentId, f2 = "Payment" } into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.Receipts on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = c.ReceiptId, f2 = "Receipt" } into rec
                     from c in rec.DefaultIfEmpty()
                     join d in db.Accountss on b.PayTo equals d.AccountsID into payto
                     from d in payto.DefaultIfEmpty()
                     join e in db.Accountss on c.PayFrom equals e.AccountsID into payfrom
                     from e in payfrom.DefaultIfEmpty()
                     join f in db.Users on a.CreatedBy equals f.Id into user
                     from f in user.DefaultIfEmpty()
                     join g in db.Branchs on a.Branch equals g.BranchID into branch
                     from g in branch.DefaultIfEmpty()
                     join h in db.Journals on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = h.JournalId, f2 = "Journal" } into jnl
                     from h in jnl.DefaultIfEmpty()
                     where ((vtype == "all") ||
                           (vtype == "Payments" && a.PDCType == "Payment") || (vtype == "Reciepts" && a.PDCType == "Receipt") || (vtype == "Journals" && a.PDCType == "Journal")) &&
                           (bank == null || b.PayFrom == bank || c.PayTo == bank) &&
                           (pdcstat == "All" || a.RegStatus == pstat) &&
                           (fromdate == "" || EF.Functions.DateDiffDay(a.PDCDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(a.PDCDate, tdate) >= 0)
                           && (a.Type != 1)
                     select new
                     {
                         id = a.PDCid,
                         Date = a.PDCDate,
                         Voucher = (a.PDCType == "Payment") ? b.VoucherNo : ((a.PDCType == "Receipt") ? c.VoucherNo : h.VoucherNo), //(b.VoucherNo != null) ? b.VoucherNo : c.VoucherNo,
                         Account = (d.Name != null) ? d.Name : e.Name,
                         Issued = (decimal?)b.GrandTotal,
                         Receipt = (decimal?)c.Paying,
                         Journal = (decimal?)h.Paying,
                         check = a.CheckNo,
                         remark = a.Note,
                         CreatedBy = f.UserName,
                         a.CreatedDate,
                         Branch = g.BranchName
                     }).OrderBy(a => a.Date);

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


        #endregion


        #region AssignedTaskReport
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Report AssignedTask")]
        public ActionResult AssignedTask()
        {
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            return View();
        }
        [QkAuthorize(Roles = "Dev,Report AssignedTask")]
        public ActionResult GetAssignedTask(long? emp, string fromdate, string todate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var UserId = User.Identity.GetUserId();
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

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


            var v = (from t in db.TaskAssigneds
                     join a in db.ProTasks on t.ProTaskId equals a.ProTaskId into ptsk
                     from a in ptsk.DefaultIfEmpty()
                     join b in db.Projects on a.ProjectId equals b.ProjectId into pro
                     from b in pro.DefaultIfEmpty()
                     join c in db.ProTaskTypes on a.TaskType equals c.TaskTypeId into type
                     from c in type.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id into usr
                     from e in usr.DefaultIfEmpty()
                     join d in db.Customers on a.CustomerID equals d.CustomerID into cus
                     from d in cus.DefaultIfEmpty()
                     join f in db.TaskStatus on a.TaskStatus equals f.TaskStatusId into ttask
                     from f in ttask.DefaultIfEmpty()
                     join s in db.Employees on a.SalesPerson equals s.EmployeeId into emps
                     from s in emps.DefaultIfEmpty()
                     join i in db.Contacts on d.Contact equals i.ContactID into tmp
                     from i in tmp.DefaultIfEmpty()

                     let taskup = db.ProTaskUpdations.Where(x => x.ProTaskId == a.ProTaskId).OrderByDescending(x => x.CreatedDate).FirstOrDefault()
                     let mobnum = db.TaskMobiles.Where(x => x.ProTaskId == a.ProTaskId).Select(x => x.MobileNo).ToList()
                     let log = db.LogManagers.Where(lg => lg.LogID == a.ProTaskId.ToString() && lg.LogTable == "ProTasks").OrderByDescending(lg => lg.LogManagerID).FirstOrDefault()

                     where t.EmployeeId == emp &&
                     (fromdate == "" || EF.Functions.DateDiffDay(t.CreatedDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(t.CreatedDate, tdate) >= 0) //&& remas.Contains(empId)
                     select new
                     {
                         a.ProTaskId,
                         a.TaskName,
                         a.TaskCode,
                         b.ProjectName,
                         ProjectId = a.ProjectId == null ? 0 : b.ProjectId,
                         CustomerName = a.CustomerID == -2 ? "No Customer" : (a.CustomerID == null ? d.CustomerName : d.CustomerName),
                         CustomerID = a.CustomerID == -2 ? -2 : (a.CustomerID == null ? d.CustomerID : d.CustomerID),
                         e.UserName,
                         a.StartDate,
                         a.EndDate,
                         a.StartTime,
                         a.EndTime,
                         c.TypeName,
                         //a.TaskStatus,
                         //f.Status,
                         a.Priority,
                         TaskStat = f.StatusName,
                         a.CreatedDate,
                         s.FirstName,
                         s.LastName,
                         //AssignedTo = (from z in db.TaskAssigneds
                         //              where z.ProTaskId == a.ProTaskId && z.Status == "Assigned" && z.chkStatus == Status.active
                         //                  id = y.EmployeeId,
                         //                  LastName = (y.LastName != null) ? y.LastName : "",
                         //                  FirstName = (y.FirstName != null) ? y.FirstName : "",
                         //                  MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                         //                  Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                         //                  y.Status

                         a.CreatedBy,
                         UserId,
                         a.Ref1,
                         a.Ref2,
                         a.Ref3,
                         a.Ref4,
                         a.Ref5,
                         b.Location,
                         ldate = ((a != null) && (taskup.CreatedDate > log.LogTime)) ? taskup.CreatedDate : log.LogTime,
                         Updateduser = db.Users.Where(x => x.Id == taskup.CreatedBy).Select(x => x.UserName).FirstOrDefault(),
                         a.TaskDetails,
                         //mobnum
                     }).Distinct().ToList().OrderByDescending(x => x.ProTaskId).Select(o => new
                     {
                         o.ProTaskId,
                         o.TaskName,
                         o.TaskCode,
                         Project = o.ProjectId,
                         o.ProjectName,
                         o.CustomerID,
                         o.CustomerName,
                         o.UserName,
                         o.StartDate,
                         o.EndDate,
                         o.StartTime,
                         o.EndTime,
                         o.TypeName,

                         Priority = Enum.GetName(typeof(TaskPriority), o.Priority),
                         // o.AssignedTo,
                         o.TaskStat,
                         o.CreatedDate,
                         EmpName = o.FirstName + " " + o.LastName,
                         o.CreatedBy,
                         o.UserId,
                         o.Ref1,
                         o.Ref2,
                         o.Ref3,
                         o.Ref4,
                         o.Ref5,
                         o.Location,
                         o.ldate,
                         o.TaskDetails,
                         //o.mobnum,
                         o.Updateduser
                     });



            //         let taskup = db.ProTaskUpdations.Where(x => x.ProTaskId == a.ProTaskId).OrderByDescending(x => x.CreatedDate).FirstOrDefault()
            //         let mobnum = db.TaskMobiles.Where(x => x.ProTaskId == a.ProTaskId).Select(x => x.MobileNo).ToList()
            //         let log = db.LogManagers.Where(lg => lg.LogID == a.ProTaskId.ToString() && lg.LogTable == "ProTasks").OrderByDescending(lg => lg.LogManagerID).FirstOrDefault()

            //         let tskmgr = db.TaskAssigneds.Where(x => x.ProTaskId == a.ProTaskId).Select(x => x.EmployeeId).ToList()

            //         (todate == "" || EF.Functions.DateDiffDay(a.StartDate, tdate) >= 0) //&& remas.Contains(empId)
            //             a.ProTaskId,
            //             a.TaskName,
            //             a.TaskCode,
            //             b.ProjectName,
            //             ProjectId = a.ProjectId == null ? 0 : b.ProjectId,
            //             e.UserName,
            //             a.StartDate,
            //             a.EndDate,
            //             a.StartTime,
            //             a.EndTime,
            //             c.TypeName,
            //             //a.TaskStatus,
            //             //f.Status,
            //             a.Priority,
            //             TaskStat = f.StatusName,
            //             a.CreatedDate,
            //             s.FirstName,
            //             s.LastName,
            //             //AssignedTo = j.FirstName + " " + j.LastName,
            //             AssignedTo = (from z in db.TaskAssigneds
            //                           where z.ProTaskId == a.ProTaskId && z.Status == "Assigned" && z.chkStatus == Status.active
            //                               id = y.EmployeeId,
            //                               LastName = (y.LastName != null) ? y.LastName : "",
            //                               FirstName = (y.FirstName != null) ? y.FirstName : "",
            //                               MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
            //                               Img = (y.ImgFileName != null) ? y.ImgFileName : "",
            //                               y.Status
            //             TskLead = (from ac in db.TaskMobiles
            //                        where (ac.ProTaskId == a.ProTaskId)
            //                            Num = ac.MobileNo
            //             a.CreatedBy,
            //             UserId,
            //             a.Ref1,
            //             a.Ref2,
            //             a.Ref3,
            //             a.Ref4,
            //             a.Ref5,
            //             b.Location,
            //             ldate = ((a != null) && (taskup.CreatedDate > log.LogTime)) ? taskup.CreatedDate : log.LogTime,
            //             a.TaskDetails,
            //         }).ToList().Select(o => new
            //             o.ProTaskId,
            //             o.TaskName,
            //             o.TaskCode,
            //             Project = o.ProjectId,
            //             o.ProjectName,
            //             o.CustomerID,
            //             o.CustomerName,
            //             o.UserName,
            //             o.StartDate,
            //             o.EndDate,
            //             o.StartTime,
            //             o.EndTime,
            //             o.TypeName,

            //             o.AssignedTo,
            //             o.TaskStat,
            //             o.CreatedDate,
            //             EmpName = o.FirstName + " " + o.LastName,
            //             o.CreatedBy,
            //             o.UserId,
            //             o.Ref1,
            //             o.Ref2,
            //             o.Ref3,
            //             o.Ref4,
            //             o.Ref5,
            //             o.Location,
            //             o.ldate,
            //             o.TaskDetails,
            //             mobmodel = o.TskLead,
            //             o.Updateduser

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
        #endregion

        #region Item Serial Numbers Expiry       

        // GET: ItemSerialNoExpiry Report
        //[QkAuthorize(Roles = "Dev,Item Serial Number")]
        public ActionResult ItemSerialNumberExpiry()
        {

            ViewBag.Category = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            companySet();
            return View();
        }

        [HttpPost]
        public ActionResult GetItemDetails(long Item, string ExpiryDate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? Expdate = null;

            if (ExpiryDate != "")
            {
                Expdate = DateTime.Parse(ExpiryDate, new CultureInfo("en-GB"));
            }

            var v = (from a in db.ItemSerialNo
                     join b in db.Items on a.itemid equals b.ItemID
                     where
                       (Item == 0 || a.itemid == Item) &&
                       (ExpiryDate == "" || ExpiryDate == null || EF.Functions.DateDiffDay(a.expirydate, Expdate) >= 0)
                     select new
                     {
                         ItemSerialNoID = a.itemserialnoid,
                         ItemSerialNo = a.serialno,
                         ItemName = b.ItemName,
                         ExpiryDate = a.expirydate,
                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.ItemSerialNoID,
                         o.ItemSerialNo,
                         o.ItemName,
                         o.ExpiryDate
                     }).OrderBy(a => a.ItemName);

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

        #endregion

        #region Asset Details search
        [HttpGet]
        public ActionResult Asset()
        {
            //For Dropdown Transaction Type
            ViewBag.TransType = QkSelect.List(new List<SelectListItem>{
                                new SelectListItem() {Text = "All",    Value="0"},
                                new SelectListItem() {Text = "Asset From Inventory",  Value="1"},
                                new SelectListItem() {Text = "Asset Purchase",        Value="2"},
                            }, "Value", "Text");

            //(Given in Html slect2)

            //For Dropdown Asset Name 
            ViewBag.Assets = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            //For Dropdown Asset Account
            ViewBag.AssetAccounts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            //For Dropdown Depreciation Account
            ViewBag.DeprctnAccounts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            companySet();
            return View();
        }

        [HttpPost]
        public ActionResult Details(long TransType, long? Asset, long? AssetAccount, long? DeprAccnt, long? DeprPerc, string From, string To)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (From != "")
            {
                fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }
            if (To != "")
            {
                tdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }

            var v = (from a in db.AssetTransferMasters
                     join b in db.AssetTransferDetails on a.AssetEntryId equals b.AssetEntryId
                     join c in db.Accountss on b.AssetAccountId equals c.AccountsID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.Accountss on b.DepreciationAccountId equals d.AccountsID into secondary
                     from d in secondary.DefaultIfEmpty()
                     where
                       (Asset == 0 || Asset == null || b.AssetitementryId == Asset) &&
                       (AssetAccount == 0 || AssetAccount == null || b.AssetAccountId == AssetAccount) &&
                       (DeprAccnt == 0 || DeprAccnt == null || b.DepreciationAccountId == DeprAccnt) &&
                       (From == "" || From == null || EF.Functions.DateDiffDay(a.AssetEntryDate, fdate) <= 0) &&
                       (To == "" || To == null || EF.Functions.DateDiffDay(a.AssetEntryDate, tdate) >= 0) &&
                       (DeprPerc == null || b.DepreciationPercentage == DeprPerc) &&
                       (TransType == 0 || (TransType == 1 && a.StockTransferId != null) || (TransType == 2 && a.StockTransferId == null))
                     select new
                     {
                         AssetId = b.AssetitementryId,
                         b.AssetName,
                         AssetAccount = c.Name,
                         DeprctnAccnt = d.Name,
                         DeprctnPerc = b.DepreciationPercentage,
                         b.Quantity,
                         b.Price,
                         b.TotalPrice,
                         TransType = a.VendorName == null ? "From Inventory" : "From Purchase",
                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.AssetId,
                         o.AssetName,
                         o.AssetAccount,
                         o.DeprctnAccnt,
                         o.DeprctnPerc,
                         o.Quantity,
                         o.Price,
                         o.TotalPrice,
                         o.TransType
                     }).OrderBy(a => a.AssetName);

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
        #endregion

        #region Over Time As On Date

        [HttpGet]
        public ActionResult OverTimeAsOnDate()
        {
            //Employees
            ViewBag.Employees = QkSelect.List(
            new List<SelectListItem>
            {
              new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            companySet();
            return View();
        }

        //Function to list the work details of Employee
        [HttpPost]
        public ActionResult GetEmployeeDetails(string Type, long? EmployeeId, string From, long CutOff)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            if (From != "")
            {
                fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }

            if (1 == 1)
            {
                var v = (from a in db.servicereports
                         join c in db.servicereportmembers on a.servicereportid equals c.servicereportid
                         join e in db.Employees on c.employeeid equals e.EmployeeId
                         where (EmployeeId == 0 || e.EmployeeId == EmployeeId)

                          && (From == "" || From == null || (EF.Functions.DateDiffDay(a.starttime, fdate) == 0))
                         select new
                         {
                             e.EmployeeId,
                             EmployeeName = e.FirstName + " " + e.LastName,
                             //Task Assigned Time
                             Tasks = (from p in db.servicereports
                                      join q in db.servicereportmembers on p.servicereportid equals q.servicereportid
                                      where (q.employeeid == c.employeeid)

                                      && (From == "" || From == null || (EF.Functions.DateDiffDay(p.starttime, fdate) == 0))
                                      //|| (EF.Functions.DateDiffDay(q.logout, fdate) == 0))
                                      select new
                                      {
                                          p.starttime,
                                          p.endtime
                                      }).ToList()
                         }).AsEnumerable().Select(o => new
                         {
                             o.EmployeeId,
                             o.EmployeeName,
                             o.Tasks
                         }).GroupBy(a => a.EmployeeId, (key, g) => g.OrderBy(m => m.EmployeeName).FirstOrDefault()).OrderBy(x => x.EmployeeName).ToList();

                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            else
            {
                var v = (from a in db.EmpAttendances
                         join e in db.Employees on a.EmployeeName equals e.UserId
                         where (EmployeeId == 0 || e.EmployeeId == EmployeeId)
                         && a.Status == "Expired"
                         && (From == "" || From == null || (EF.Functions.DateDiffDay(a.login, fdate) == 0))
                         select new
                         {
                             e.EmployeeId,
                             EmployeeName = e.FirstName + " " + e.LastName,
                             //Total Time
                             Tasks = (from p in db.EmpAttendances
                                      where (p.EmployeeName == a.EmployeeName)
                                      && p.Status == "Expired"
                                      && (From == "" || From == null || (EF.Functions.DateDiffDay(p.login, fdate) == 0))
                                      select new
                                      {
                                          starttime = p.login,
                                          p.logout
                                      }).ToList()
                         }).Select(o => new
                         {
                             o.EmployeeId,
                             o.EmployeeName,
                             o.Tasks
                         }).GroupBy(a => a.EmployeeId, (key, g) => g.OrderBy(m => m.EmployeeName).FirstOrDefault()).OrderBy(x => x.EmployeeName).ToList();

                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }

        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult NonMovableItemdaterage()
        {
            ViewBag.Supplier = QkSelect.List(
                        new List<SelectListItem>
                        {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                        }, "Value", "Text", 1);
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;
            var OptAll2 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Category = OptAll2;
            var OptAll3 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Item = OptAll3;
            List<SelectListItem> SelectPeriod = new List<SelectListItem>() {
                    new SelectListItem {
                          Text = "3 Month", Value = "3"
                                        },
                    new SelectListItem {
                        Text = "2 Month", Value = "2"
                                   },
                    new SelectListItem {

                         Text = "1 Month", Value = "1"
                          },
                    new SelectListItem {

                         Text = "1 Year", Value = "12"
                          },

            };
            ViewBag.Period = SelectPeriod;
            //    Id = s.MCId,
            //    Name = s.MCName
            ViewBag.MC = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "", Value = "-1"},
                                }, "Value", "Text", 0);
            return View();
        }
        public ActionResult NonMovableItem()
        {
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;
            var OptAll2 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Category = OptAll2;
            var OptAll3 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Item = OptAll3;
            List<SelectListItem> SelectPeriod = new List<SelectListItem>() {
                    new SelectListItem {
                          Text = "3 Month", Value = "3"
                                        },
                    new SelectListItem {
                        Text = "2 Month", Value = "2"
                                   },
                    new SelectListItem {

                         Text = "1 Month", Value = "1"
                          },
                    new SelectListItem {

                         Text = "1 Year", Value = "12"
                          },

            };
            ViewBag.Period = SelectPeriod;
            //    Id = s.MCId,
            //    Name = s.MCName
            ViewBag.MC = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);
            return View();
        }
        public ActionResult NonMovableItemaging()
        {
            ViewBag.Supplier = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 0);

            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;
            var OptAll2 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Category = OptAll2;
            var OptAll3 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Item = OptAll3;
            List<SelectListItem> SelectPeriod = new List<SelectListItem>() {
                    new SelectListItem {
                          Text = "3 Month", Value = "3"
                                        },
                    new SelectListItem {
                        Text = "2 Month", Value = "2"
                                   },
                    new SelectListItem {

                         Text = "1 Month", Value = "1"
                          },
                    new SelectListItem {

                         Text = "1 Year", Value = "12"
                          },

            };
            ViewBag.Period = SelectPeriod;
            //    Id = s.MCId,
            //    Name = s.MCName
            ViewBag.MC = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);
            return View();
        }
        [HttpPost]
        public JsonResult GetNonMovableItemsaging(long? supplier, long? ddmc, long? brand, long? category, long? item, long aging, string srchtxt)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            DateTime datenow = DateTime.Now;
            DateTime? fdate = null;

            DateTime tdate = datenow.AddMonths(-1 * (int)aging).Date;

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


            var userpermission = User.IsInRole("All Customers");


            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");
            var stocks = 0.0;
            var CurrentStock = 0.0;
            string fromdate = "";
            var lartpurchaseitems = (from a in db.Items
                                     join b in db.PEItemss on a.ItemID equals b.Item
                                     join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                                     where (ddmc == 0 || c.MaterialCenter == ddmc) &&
                                     (item == null || item == 0 || a.ItemID == item) &&
                                     (brand == 0 || brand == null || a.ItemBrandID == brand) &&
                                     (category == 0 || category == null || a.ItemCategoryID == category) &&
                                     (supplier == 0 || supplier == null || c.Supplier == supplier)
                                     group b.Item by a.ItemID into grp
                                     select new
                                     {
                                         cn = grp.Count(),
                                         it = grp.Key
                                     }

                                   ).OrderByDescending(o => o.cn).ToList();
            recordsTotal = lartpurchaseitems.Count();
            // EF Core can't join db.Items against another IQueryable (lartpurchaseitems); materialize the
            // grouped counts, filter Items server-side by their IDs, then join the two lists in memory.
            var lartIds = lartpurchaseitems.Select(x => x.it).ToList();
            // EF Core eagerly evaluates srchtxt.ToUpper() as a query parameter; srchtxt binds to null for an
            // empty form field, so compute the (uppercased) search term null-safely outside the query.
            var srchU = (srchtxt ?? "").ToUpper();
            var itemsForJoin = db.Items
                            .Where(a => a.Status == Status.active && a.KeepStock == true &&
                            (srchU == "" || a.ItemName.ToUpper().Contains(srchU)) &&
                            lartIds.Contains(a.ItemID))
                            .Select(a => new { a.ItemID, a.ItemCode, a.ItemName, a.PurchasePrice }).ToList();
            var itemlist = (from a in itemsForJoin
                            join b in lartpurchaseitems on a.ItemID equals b.it
                            select new
                            {
                                a.ItemID,
                                a.ItemCode,
                                a.ItemName,
                                a.PurchasePrice,
                                b.cn,

                            }
                          ).OrderByDescending(o => o.cn).ToList().Skip(skip).Take(pageSize).Select(o => new
                          {
                              o.ItemID,
                              o.ItemCode,
                              o.ItemName,
                              o.PurchasePrice,
                              purchasestock = (supplier == 0 || supplier == null) ? GetItemWisestockondate(o.ItemID, ddmc, tdate) : purchasestock(o.ItemID, ddmc, tdate, supplier),
                              salesstock = getsalestock(o.ItemID, ddmc, tdate, supplier),

                              currentstock = (supplier == 0 || supplier == null) ? com.GetItemWisestock(o.ItemID, ddmc) : purchasestock(o.ItemID, ddmc, System.DateTime.Now, supplier),



                          }).ToList().Select(o => new
                          {
                              o.ItemID,
                              o.ItemCode,
                              o.ItemName,
                              o.PurchasePrice,
                              o.purchasestock,
                              salesstock = (supplier == 0 || supplier == null) ? o.salesstock : (o.salesstock >= o.purchasestock) ? o.purchasestock : o.salesstock,
                              currentstock = (supplier == 0 || supplier == null) ? o.currentstock : (o.salesstock >= o.currentstock) ? 0 : o.currentstock - o.salesstock,

                          }).ToList();




            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = itemlist });

        }
        public decimal GetItemWisestockondate(long itemid, long? ddmc, DateTime tdate)
        {
            var exist = db.PEItemss.Any(o => o.Item == itemid);
            if (!exist)
                return 0;
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", (object)tdate ?? DBNull.Value);
            var stype = new SqlParameter("@Stype", "1");

            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

            if (data.Count() > 0)
                return (data[0].ITotalQty == null) ? 0 : (decimal)data[0].ITotalQty;
            else
                return 0;



        }
        public decimal? getpurchasestock(long itemid, long? ddlmc, DateTime tdate)
        {

            var ppristock = (from a in db.PurchaseEntrys
                             join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                             join c in db.Items on new { f1 = b.Item, f2 = b.ItemUnit } equals
                             new { f1 = c.ItemID, f2 = c.ItemUnitID }
                             where a.PEDate <= tdate
                             && (ddlmc == null || ddlmc == 0 || ddlmc == 1 || a.MaterialCenter == ddlmc)
                             select new
                             {

                                 b.ItemQuantity
                             }).Sum(o => o.ItemQuantity);
            var psecstock = (from a in db.PurchaseEntrys
                             join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                             join c in db.Items on new { f1 = b.Item, f2 = b.ItemUnit } equals
                             new { f1 = c.ItemID, f2 = c.SubUnitId }
                             where a.PEDate <= tdate
                             && (ddlmc == null || ddlmc == 0 || ddlmc == 1 || a.MaterialCenter == ddlmc)
                             select new
                             {

                                 qty = b.ItemQuantity / c.ConFactor
                             }).Sum(o => o.qty);

            var totalpurchase = ppristock + psecstock;
            return totalpurchase;

        }
        public decimal? purchasestock(long itemid, long? ddlmc, DateTime tdate, long? supplier)
        {
            DateTime today = System.DateTime.Now;
            var itm = db.Items.Find(itemid);
            bool same = true;
            if (itm.SubUnitId == itm.ItemUnitID)
                same = true;
            else if (itm.SubUnitId != null)
                same = false;
            else
                same = true;
            var ppristock = (from a in db.PurchaseEntrys
                             join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                             join c in db.Items on new { f1 = b.Item } equals
                             new { f1 = c.ItemID }


                             where a.PEDate <= tdate
                             && (ddlmc == null || ddlmc == 0 || ddlmc == 1 || a.MaterialCenter == ddlmc)
                             && b.Item == itemid &&
                             (supplier == 0 || supplier == null || a.Supplier == supplier)
                             select new
                             {

                                 b.ItemQuantity
                             }).Select(o => o.ItemQuantity).AsEnumerable().DefaultIfEmpty(0).Sum();
            //                           where a.PEDate <= tdate
            //                            && (ddlmc == null || ddlmc == 0 || ddlmc == 1 || a.MaterialCenter == ddlmc)
            //                            (supplier == 0 || supplier == null || a.Supplier == supplier)

            //                               qty = b.ItemQuantity / c.ConFactor

            var totalpurchase = ppristock;// + ((same == true) ? 0 : psecstock);
            var totalsalestock = getsalestock(itemid, ddlmc, tdate, supplier, false);
            var balstock = totalpurchase - totalsalestock;
            if (balstock < 0)
            {
                totalpurchase = 0;
            }
            else
            {
                totalpurchase = (decimal)balstock;
            }
            return totalpurchase;

        }
        public decimal? getsalestock(long itemid, long? ddlmc, DateTime tdate, long? supplier, bool between = true)
        {
            DateTime today = System.DateTime.Now;
            var itm = db.Items.Find(itemid);
            bool same = true;
            if (itm.SubUnitId == itm.ItemUnitID)
                same = true;
            else if (itm.SubUnitId != null)
                same = false;
            else
                same = true;



            var ppristock = (from a in db.SalesEntrys
                             join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                             join c in db.Items on new { f1 = b.Item } equals
                             new { f1 = c.ItemID }
                             join d in db.PEItemss on c.ItemID equals d.Item into dd
                             from d in dd.DefaultIfEmpty()
                             join e in db.PurchaseEntrys on d.PurchaseEntry equals e.PurchaseEntryId into ee
                             from e in ee.DefaultIfEmpty()

                             where
                             (between == true || (a.SEDate <= tdate)) &&
                             (between == false || (a.SEDate >= tdate && a.SEDate <= today))
                             && (ddlmc == null || ddlmc == 0 || ddlmc == 1 || a.MaterialCenter == ddlmc)
                             && b.Item == itemid &&
                             (supplier == 0 || supplier == null || e.Supplier == supplier)
                             select new
                             {

                                 b.ItemQuantity
                             }).Select(o => o.ItemQuantity).AsEnumerable().DefaultIfEmpty(0).Sum();


            //                 (between == false || (a.SEDate >= tdate && a.SEDate <= today))
            //                          && (ddlmc == null || ddlmc == 0 || ddlmc == 1 || a.MaterialCenter == ddlmc)
            //             (supplier == 0 || supplier == null || e.Supplier == supplier)

            //                     qty = b.ItemQuantity / c.ConFactor

            var totalsales = ppristock;// +((same==false)? psecstock:0);
            return totalsales;

        }
        [HttpPost]
        public JsonResult GetNonMovableItemsdaterange(long? ddmc, string fromdate, string todate, long? brand, long? category, long? item, long? moves, long? supplier)
        {
            var UserId = User.Identity.GetUserId();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            DateTime datenow = DateTime.Now;
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


            var userpermission = User.IsInRole("All Customers");


            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");
            var stocks = 0.0;
            var CurrentStock = 0.0;
            //                         //join c in db.SalesEntrys on a.CustomerID equals c.Customer 
            //                     (todate == "" || EF.Functions.DateDiffDay(c.SECreatedDate, tdate) >= 0)
            //                     && a.KeepStock == true
            //                     && e.Supplier == supplier
            //                         a.ItemID




            var v1 = (from a in db.Items
                      join b in db.SEItemss on a.ItemID equals b.Item into temp
                      from b in temp.DefaultIfEmpty()
                      join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId into temp2
                      from c in temp2.DefaultIfEmpty()
                      where (fromdate == "" || EF.Functions.DateDiffDay(c.SECreatedDate, fdate) <= 0) &&
                      (todate == "" || EF.Functions.DateDiffDay(c.SECreatedDate, tdate) >= 0)
                      && a.KeepStock == true


                      /*(EF.Functions.DateDiffDay(c.SECreatedDate, datecheck) <= 0)*/
                      group new { a.ItemID, a.ItemName, a.ItemCode, a.PurchasePrice, a.ItemCategoryID, a.ItemBrandID } by a.ItemID into grp
                      select new
                      {
                          id = grp.FirstOrDefault().ItemID,
                          ItemName = grp.FirstOrDefault().ItemName,
                          ItemCode = grp.FirstOrDefault().ItemCode,
                          PurchasePrice = grp.FirstOrDefault().PurchasePrice,
                          CurrentStock,
                          ItemCategoryID = grp.FirstOrDefault().ItemCategoryID,
                          ItemBrandID = grp.FirstOrDefault().ItemBrandID,
                          count = grp.Count(),
                      })
            .Select(o => new
            {
                o.id,
                o.ItemName,
                o.ItemCode,
                o.PurchasePrice,
                o.CurrentStock,
                o.ItemCategoryID,
                o.ItemBrandID,
                o.count
            }).Distinct().ToList();

            var tramsactionitemspurchase = (from e in db.PEItemss
                                            join f in db.PurchaseEntrys on e.PurchaseEntry equals f.PurchaseEntryId
                                            where (fromdate == "" || EF.Functions.DateDiffDay(f.PEDate, fdate) <= 0) &&
                                             (todate == "" || EF.Functions.DateDiffDay(f.PEDate, tdate) >= 0) &&
                                     (ddmc == 0 || ddmc == null || f.MaterialCenter == ddmc)&&
                                           (supplier == 0||supplier==null||f.Supplier==supplier)
                                            select new
                                            {
                                                e.Item
                                            }).Distinct();

            var alltransactionitem = tramsactionitemspurchase.Select(o => o.Item).Distinct().ToList().ToArray();

            var v = (from d in db.Items

                     where
                      alltransactionitem.Contains(d.ItemID) 
                       && d.KeepStock == true
                     select new
                     {
                         id = d.ItemID,
                         d.ItemName,
                         d.ItemCode,
                         d.PurchasePrice,
                         CurrentStock,
                         d.ConFactor,
                         d.ItemCategoryID,
                         d.ItemBrandID
                     }).Select(o => new
                     {
                         o.id,
                         o.ItemName,
                         o.ItemCode,
                         o.PurchasePrice,
                         o.CurrentStock,
                         o.ConFactor,

                         o.ItemCategoryID,
                         o.ItemBrandID,
                         count = 0
                     }).Distinct().ToList();
            //   .Union(v11).Union(v14).Union(v15).Union(v16).Union(v17).Union(v18);//combain all movable item

            moves = moves - 1;
            var itemids = v1.Select(o => o.id).ToList().ToArray();

            var full = v.ToList();//seperating non-movable items from Total items
            var v20 = (from d in full
                       select new
                       {
                           id = d.id,
                           d.ItemName,
                           d.ItemCode,
                           d.PurchasePrice,
                           CurrentStock,
                           d.ItemCategoryID,
                           d.ItemBrandID,
                           d.ConFactor,

                       }).Select(o => new StockDetails4()
                       {
                           id = o.id,
                           ItemName = o.ItemName,
                           ItemCode = o.ItemCode,
                           PurchasePrice = o.PurchasePrice,
                           totalpruchase = 0,
                           CurrentStock = 0,
                           confactor = o.ConFactor,
                           ItemCategoryID = o.ItemCategoryID,
                           ItemBrandID = o.ItemBrandID
                       }).Distinct().ToList();



            var full2 = full.Select(z => z.id).ToList();

            var keepstk = 0;
            ddmc = ddmc != null ? ddmc : 0;
            List<StockDetailsmovement> datadd = new List<StockDetailsmovement>();
            for (int i = 0; i < full2.Count(); i++)
            {

                var itemid = full[i].id;


                var prqty = (from a in db.PurchaseEntrys
                             join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                             where
                             (ddmc == 0|| a.MaterialCenter == ddmc) &&
                             a.PEDate <= tdate &&
                             b.Item == itemid
                             select new
                             {
                                 b.ItemQuantity
                             }
                               ).Sum(o => o.ItemQuantity);
                var sttransfer = (from a in db.StockTransfers
                             join b in db.StockTransferItems on a.Id equals b.StockTransferId
                             where
                             (ddmc == 0 || a.MCTo == ddmc) &&
                             a.Date <= tdate &&
                             b.Item == itemid
                             select new
                             {
                                b.Quantity
                             }
                             ).ToList().Sum(o => o.Quantity);
                //                   where

                //                   b.Item == itemid
                //                       b.ItemQuantity


                var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
                var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
                var brands = new SqlParameter("@BrandId", "0");
                var stkble = new SqlParameter("@Stockble", "");
                var catgry = new SqlParameter("@CategoryId", "0");
                var fromdatee = new SqlParameter("@fromdate", "");
                var todatee = new SqlParameter("@todate", "");
                var stype = new SqlParameter("@Stype", "0");


                var tsales = db.Database.SqlQueryDedup<StockDataDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brands, stkble, catgry, fromdatee, todatee, stype).AsEnumerable().OrderBy(a => a.TDate).ToList();
                var saleqty = tsales.Where(o => (o.TItemType == "Sales" || o.TItemType == "Stock Transfered" || o.TItemType == "Stock Transferedadj" || o.TItemType == "Purchase Return"))
             .GroupBy(x => new { x.TDate, x.TItemType, x.UnitPrice, x.ItemId, x.Invoice }, (key, group) => new batchstock

             {

                 TDate = key.TDate,
                 OQty = group.Sum(o => o.Qty),
                 BQty = group.Sum(o => o.Qty),
                 UnitPrice = key.UnitPrice,
                 TItemType = key.TItemType,
                 currstock = group.Sum(o => o.Qty),
                 //confactor=(group.Select(o=>o.confactor).FirstOrDefault()== null)1? group.Select(o => o.confactor).FirstOrDefault()
                 transactiondid = group.Max(o => o.TItemId),
                 itemid = key.ItemId,
                 invoice = key.Invoice

             }).ToList().Sum(o => o.currstock);
                var currstock = (prqty+ sttransfer) - saleqty;








                StockDetailsmovement datadd2 = new StockDetailsmovement();
                int index = full.FindIndex(o => o.id == full2[i]);
                var pur= getlastpurchaseprice((long)v20[index].id, (long)ddmc);
                
                  v20[index].PurchasePrice = pur;
                v20[index].CurrentStock = currstock;
                v20[index].totalpruchase = (prqty + sttransfer);
















            }


            var data1 = (from a in v20
                         where (brand == 0 || a.ItemBrandID == brand) &&
                         (category == 0 || a.ItemCategoryID == category) &&
                         (item == 0 || a.id == item)
                         select new
                         {
                             id = a.id,
                             a.ItemName,
                             a.ItemCode,
                             a.PurchasePrice,
                             a.CurrentStock,
                             a.totalpruchase,


                             a.ItemCategoryID,
                             a.ItemBrandID
                         }).Where(o=>o.CurrentStock>0).ToList();

            recordsTotal = v20.Count();


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data1 });

        }
        public decimal getlastpurchaseprice(long itemid,long ddmcid)
        {
            var lpur = (from a in db.PurchaseEntrys
                        join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                        where a.MaterialCenter == ddmcid && b.Item == itemid
                        select new
                        {
                            b.ItemUnitPrice,
                            a.PEDate
                        }).OrderByDescending(o => o.PEDate).Select(o => o.ItemUnitPrice).FirstOrDefault();
        if(lpur==null)
            {
                var purprice = db.Items.Where(o => o.ItemID == itemid).Select(o => o.PurchasePrice).FirstOrDefault();
                return purprice;
            }
        else
            {
                return lpur;
            }
        }
        [HttpPost]
        public JsonResult GetNonMovableItems(long? ddmc, string fromdate, string todate, long? brand, long? category, long? item,long? moves)
        {
            var UserId = User.Identity.GetUserId();
           
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            DateTime datenow = DateTime.Now;
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


            var userpermission = User.IsInRole("All Customers");


            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");
            var stocks = 0.0;
            var CurrentStock = 0.0;

            //          (todate == "" || EF.Functions.DateDiffDay(f.SRCreatedDate, tdate) >= 0)
            //          group new {a.ItemID,a.ItemName,a.ItemCode,a.PurchasePrice,a.ItemCategoryID,a.ItemBrandID} by a.ItemID into grp
            //              id = grp.FirstOrDefault().ItemID,
            //              ItemName=grp.FirstOrDefault().ItemName,
            //              ItemCode = grp.FirstOrDefault().ItemCode,
            //              PurchasePrice = grp.FirstOrDefault().PurchasePrice,
            //              CurrentStock,
            //              ItemCategoryID = grp.FirstOrDefault().ItemCategoryID,
            //              ItemBrandID = grp.FirstOrDefault().ItemBrandID,
            //          })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID,
            //    o.count
            //          (todate == "" || EF.Functions.DateDiffDay(f.PECreatedDate, tdate) >= 0)
            //              id = a.ItemID,
            //              a.ItemName,
            //              a.ItemCode,
            //              a.PurchasePrice,
            //              CurrentStock,
            //              a.ItemCategoryID,
            //              a.ItemBrandID
            //          })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.PRCreatedDate, tdate) >= 0)
            //              id = a.ItemID,
            //              a.ItemName,
            //              a.ItemCode,
            //              a.PurchasePrice,
            //              CurrentStock,
            //              a.ItemCategoryID,
            //              a.ItemBrandID
            //          })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.POCreatedDate, tdate) >= 0)
            //              id = a.ItemID,
            //              a.ItemName,
            //              a.ItemCode,
            //              a.PurchasePrice,
            //              CurrentStock,
            //              a.ItemCategoryID,
            //              a.ItemBrandID
            //          })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.SOCreatedDate, tdate) >= 0)
            //              id = a.ItemID,
            //              a.ItemName,
            //              a.ItemCode,
            //              a.PurchasePrice,
            //              CurrentStock,
            //              a.ItemCategoryID,
            //              a.ItemBrandID
            //          })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.DvCreatedDate, tdate) >= 0)
            //              id = a.ItemID,
            //              a.ItemName,
            //              a.ItemCode,
            //              a.PurchasePrice,
            //              CurrentStock,
            //              a.ItemCategoryID,
            //              a.ItemBrandID
            //          })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.CreatedDate, tdate) >= 0)
            //              id = a.ItemID,
            //              a.ItemName,
            //              a.ItemCode,
            //              a.PurchasePrice,
            //              CurrentStock,
            //              a.ItemCategoryID,
            //              a.ItemBrandID
            //          })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.PFCreatedDate, tdate) >= 0)
            //               id = a.ItemID,
            //               a.ItemName,
            //               a.ItemCode,
            //               a.PurchasePrice,
            //               CurrentStock,
            //               a.ItemCategoryID,
            //               a.ItemBrandID
            //           })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.PQuotCreatedDate, tdate) >= 0)
            //               id = a.ItemID,
            //               a.ItemName,
            //               a.ItemCode,
            //               a.PurchasePrice,
            //               CurrentStock,
            //               a.ItemCategoryID,
            //               a.ItemBrandID
            //           })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.AssetEntryDate, tdate) >= 0)
            //               id = a.ItemID,
            //               a.ItemName,
            //               a.ItemCode,
            //               a.PurchasePrice,
            //               CurrentStock,
            //               a.ItemCategoryID,
            //               a.ItemBrandID
            //           })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.EntryDate, tdate) >= 0)
            //               id = a.ItemID,
            //               a.ItemName,
            //               a.ItemCode,
            //               a.PurchasePrice,
            //               CurrentStock,
            //               a.ItemCategoryID,
            //               a.ItemBrandID
            //           })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(e.CreatedDate, tdate) >= 0)
            //               id = a.ItemID,
            //               a.ItemName,
            //               a.ItemCode,
            //               a.PurchasePrice,
            //               CurrentStock,
            //               a.ItemCategoryID,
            //               a.ItemBrandID
            //           }).Select(o => new
            //               o.id,
            //               o.ItemName,
            //               o.ItemCode,
            //               o.PurchasePrice,
            //               o.CurrentStock,
            //               o.ItemCategoryID,
            //               o.ItemBrandID
            //          (todate == "" || EF.Functions.DateDiffDay(f.CreatedDate, tdate) >= 0)
            //               id = a.ItemID,
            //               a.ItemName,
            //               a.ItemCode,
            //               a.PurchasePrice,
            //               CurrentStock,
            //               a.ItemCategoryID,
            //               a.ItemBrandID
            //           })
            //               .Select(o => new
            //                   o.id,
            //                   o.ItemName,
            //                   o.ItemCode,
            //                   o.PurchasePrice,
            //                   o.CurrentStock,
            //                   o.ItemCategoryID,
            //                   o.ItemBrandID

            //          (todate == "" || EF.Functions.DateDiffDay(f.MRCreatedDate, tdate) >= 0)
            //               id = a.ItemID,
            //               a.ItemName,
            //               a.ItemCode,
            //               a.PurchasePrice,
            //               CurrentStock,
            //               a.ItemCategoryID,
            //               a.ItemBrandID
            //           })
            //.Select(o => new
            //    o.id,
            //    o.ItemName,
            //    o.ItemCode,
            //    o.PurchasePrice,
            //    o.CurrentStock,
            //    o.ItemCategoryID,
            //    o.ItemBrandID
            var v1 = (from a in db.Items
                      join b in db.SEItemss on a.ItemID equals b.Item into temp
                      from b in temp.DefaultIfEmpty()
                      join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId into temp2
                      from c in temp2.DefaultIfEmpty()
                      where (fromdate == "" || EF.Functions.DateDiffDay(c.SECreatedDate, fdate) <= 0) &&
                      (todate == "" || EF.Functions.DateDiffDay(c.SECreatedDate, tdate) >= 0)
                      /*(EF.Functions.DateDiffDay(c.SECreatedDate, datecheck) <= 0)*/
                      group new { a.ItemID, a.ItemName, a.ItemCode, a.PurchasePrice, a.ItemCategoryID, a.ItemBrandID } by a.ItemID into grp
                      select new
                      {
                          id = grp.FirstOrDefault().ItemID,
                          ItemName = grp.FirstOrDefault().ItemName,
                          ItemCode = grp.FirstOrDefault().ItemCode,
                          PurchasePrice = grp.FirstOrDefault().PurchasePrice,
                          CurrentStock,
                          ItemCategoryID = grp.FirstOrDefault().ItemCategoryID,
                          ItemBrandID = grp.FirstOrDefault().ItemBrandID,
                          count = grp.Count(),
                      })
            .Select(o => new
            {
                o.id,
                o.ItemName,
                o.ItemCode,
                o.PurchasePrice,
                o.CurrentStock,
                o.ItemCategoryID,
                o.ItemBrandID,
                o.count
            }).Distinct().ToList();

            var tramsactionitemspurchase = (from e in  db.PEItemss 
                                            join f in db.PurchaseEntrys on  e.PurchaseEntry equals f.PurchaseEntryId
                                            where (fromdate == "" || EF.Functions.DateDiffDay(f.PEDate, fdate) <= 0) &&
                                             (todate == "" || EF.Functions.DateDiffDay(f.PEDate, tdate) >= 0) &&
                                     (ddmc == 0 || ddmc == null || f.MaterialCenter == ddmc)
                                    select new
                                    {
                                        e.Item
                                    }).Distinct();
            
            var alltransactionitem = tramsactionitemspurchase.Select(o=>o.Item).Distinct().ToList().ToArray();

            var v = (from d in db.Items

                     where
                      alltransactionitem.Contains(d.ItemID)
                     select new
                     {
                         id = d.ItemID,
                         d.ItemName,
                         d.ItemCode,
                         d.PurchasePrice,
                         CurrentStock,
                         d.ItemCategoryID,
                         d.ItemBrandID
                     }).Select(o => new
                     {
                         o.id,
                         o.ItemName,
                         o.ItemCode,
                         o.PurchasePrice,
                         o.CurrentStock,
                         o.ItemCategoryID,
                         o.ItemBrandID,
                         count=0
                     }).Distinct().ToList();
            //   .Union(v11).Union(v14).Union(v15).Union(v16).Union(v17).Union(v18);//combain all movable item

            moves = moves - 1;
            var itemids = v1.Where(o=>o.count>moves).Select(o => o.id).ToList().ToArray();

            var full = v.Where(o=>!itemids.Contains(o.id)).ToList();//seperating non-movable items from Total items
            var v20 = (from d in full
                       select new
                       {
                           id = d.id,
                           d.ItemName,
                           d.ItemCode,
                           d.PurchasePrice,
                           CurrentStock,
                           d.ItemCategoryID,
                           d.ItemBrandID
                       }).Select(o => new StockDetails2()
                       {
                           id = o.id,
                           ItemName = o.ItemName,
                           ItemCode = o.ItemCode,
                           PurchasePrice = o.PurchasePrice,
                           CurrentStock = 0,
                           ItemCategoryID = o.ItemCategoryID,
                           ItemBrandID = o.ItemBrandID
                       }).Distinct().ToList();



            var full2 = full.Select(z => z.id).ToList();

            var keepstk = 0;
            ddmc = ddmc != null ? ddmc : 0;
            List<StockDetailsmovement> datadd = new List<StockDetailsmovement>();
            for (int i = 0; i < full2.Count(); i++)
            {

                var itemid = full[i].id;


                StockDetailsmovement datadd2 = new StockDetailsmovement();
                int index = full.FindIndex(o => o.id == full2[i]);
                datadd2.currstock = GetItemWisestock2(full2[i], ddmc);
                v20[index].CurrentStock = datadd2.currstock;


            }

            if (sortColumnDir == "asc")
            {
                v20 = v20.Where(z => z.CurrentStock > 0).OrderBy(z => z.CurrentStock).ToList();

            }
            else
            {
                v20 = v20.Where(z => z.CurrentStock > 0).OrderByDescending(z => z.CurrentStock).ToList();

            }
            var data1 = (from a in v20
                         where (brand == 0 || a.ItemBrandID == brand) &&
                         (category == 0 || a.ItemCategoryID == category) &&
                         (item == 0 || a.id == item)
                         select new
                         {
                             id = a.id,
                             a.ItemName,
                             a.ItemCode,
                             a.PurchasePrice,
                             a.CurrentStock,
                             a.ItemCategoryID,
                             a.ItemBrandID
                         }).ToList();

            recordsTotal = v20.Count();


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data1 });

        }
        public decimal? GetItemWisestock2(long? itemid, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "1");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

            if (data.Count > 0)
                return data[0].ITotalQty;
            else
                return 0;

        }
        public decimal? GetItemWisestock3(long? itemid, long? ddmc,DateTime? datefrom)
        {
            DateTime opdate =Convert.ToDateTime(datefrom).AddDays(-1);
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "1");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", (object)opdate ?? DBNull.Value);
            var stype = new SqlParameter("@Stype", "1");

            try
            {
                var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();


                return data[0].ITotalQty;
            }
            catch(Exception e)
            {
                return 0;
            }

        }

        public stockdetailsreturn GetItemWisetansaction(long? itemid, long? ddmc, DateTime? datefrom,DateTime? dateto,long? supplier,long? catogory,long? brandid)
        {

            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            ddmc = ddmc != null ? ddmc : 0;

            ////for supllier
            //// var sup = new SqlParameter("@SupplierId", "0");
            ////end

            var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", (object)brandid ?? DBNull.Value);

            //for supllier
            //end
            var supplierid = new SqlParameter("@Supplier", (object)supplier ?? DBNull.Value);
            var stkble = new SqlParameter("@Stockble", "1");
            var catgry = new SqlParameter("@CategoryId", (object)catogory ?? DBNull.Value);
            var fromdate = new SqlParameter("@fromdate", (object)datefrom ?? DBNull.Value);
            var todate = new SqlParameter("@todate", (object)dateto ?? DBNull.Value);
            var stype = new SqlParameter("@Stype", "0");

            var data = db.Database.SqlQueryDedup<StockDataDetails>("SP_AVCOMethod4 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype,@Supplier", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype, supplierid).AsEnumerable().ToList();

            
            var totalpurchase = data.Where(o => o.TItemType == "Purchase").Sum(o => o.Qty);
            var totalsales = data.Where(o => o.TItemType == "Sales").Sum(o => o.Qty);
            var totalpurchasereturn = data.Where(o => o.TItemType == "Purchase Return").Sum(o => o.Qty);
            var totalsalesreturn = data.Where(o => o.TItemType == "Sales Return").Sum(o => o.Qty);
            var totalstckin = data.Where(o => o.TItemType == "Stock Received").Sum(o => o.Qty);
            var totalstockout = data.Where(o => o.TItemType == "Stock Transfered").Sum(o => o.Qty);
            stockdetailsreturn d = new stockdetailsreturn
            {
totalpurchase=totalpurchase,
totalsales=totalsales,
totalpurchasereturn=totalpurchasereturn,
totalstckin=totalstckin,
totalstockout=totalstockout
            };

            return d;



        }

        public JsonResult SearchMC(string q, string x)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.MCs.Where(p => p.MCName.ToLower().Contains(q.ToLower()) || p.MCName.Contains(q))
                                  .Select(b => new SelectFormat3
                                  {
                                      text = b.MCName, //each json object will have 
                                      id = b.MCId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.MCs.Select(b => new SelectFormat3
                {
                    text = b.MCName, //each json object will have 
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();

            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
    }
}
