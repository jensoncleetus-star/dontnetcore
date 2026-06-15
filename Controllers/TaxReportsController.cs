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
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class TaxReportsController : BaseController
    {
        ApplicationDbContext db;
        public TaxReportsController()
        {
            db = new ApplicationDbContext();
        }

        #region All Tax
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Tax Summary")]
        public ActionResult Tax()
        {
            companySet();
            return View();
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Tax Summary")]
        public JsonResult GetTax(string fromdate, string todate, string emirates)
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

            //sales
            var SETaxableAmt = (from b in db.SalesEntrys
                                join c in db.Customers on b.Customer equals c.CustomerID into cust
                                from c in cust.DefaultIfEmpty()
                                join d in db.Contacts on c.Contact equals d.ContactID into con
                                from d in con.DefaultIfEmpty()
                                where (b.SalesType != 2) && b.Status == 1
                                && (emirates == "" || d.State == emirates)
                                && (fromdate == "" || EF.Functions.DateDiffDay(b.SEDate, fdate) <= 0)
                                && (todate == "" || EF.Functions.DateDiffDay(b.SEDate, tdate) >= 0)
                                select
                                b.SESubTotal - b.SEDiscount).AsEnumerable().DefaultIfEmpty(0).Sum();



            var SETaxAmt = (from b in db.SalesEntrys
                            join c in db.Customers on b.Customer equals c.CustomerID into cust
                            from c in cust.DefaultIfEmpty()
                            join d in db.Contacts on c.Contact equals d.ContactID into con
                            from d in con.DefaultIfEmpty()
                            where (b.SalesType != 2) && b.Status == 1
                            && (emirates == "" || d.State == emirates)
                            && (fromdate == "" || EF.Functions.DateDiffDay(b.SEDate, fdate) <= 0)
                            && (todate == "" || EF.Functions.DateDiffDay(b.SEDate, tdate) >= 0)
                            select b.SETaxAmount).AsEnumerable().DefaultIfEmpty(0).Sum();

            //                    where
            //                    (todate == "" || EF.Functions.DateDiffDay(b.SEDate, tdate) >= 0)


            //sales return
            var SRTaxableAmt = (from b in db.SalesReturns
                                join c in db.Customers on b.Customer equals c.CustomerID into cust
                                from c in cust.DefaultIfEmpty()
                                join d in db.Contacts on c.Contact equals d.ContactID into con
                                from d in con.DefaultIfEmpty()
                                where (b.SalesType != 2)
                                && (emirates == "" || d.State == emirates)
                                && (fromdate == "" || EF.Functions.DateDiffDay(b.SRDate, fdate) <= 0)
                                && (todate == "" || EF.Functions.DateDiffDay(b.SRDate, tdate) >= 0)
                                select b.SRSubTotal - b.SRDiscount).AsEnumerable().DefaultIfEmpty(0).Sum();



            var SRTaxAmt = (from b in db.SalesReturns
                            join c in db.Customers on b.Customer equals c.CustomerID into cust
                            from c in cust.DefaultIfEmpty()
                            join d in db.Contacts on c.Contact equals d.ContactID into con
                            from d in con.DefaultIfEmpty()
                            where (b.SalesType != 2)
                            && (emirates == "" || d.State == emirates)
                            && (fromdate == "" || EF.Functions.DateDiffDay(b.SRDate, fdate) <= 0)
                            && (todate == "" || EF.Functions.DateDiffDay(b.SRDate, tdate) >= 0)
                            select b.SRTaxAmount).AsEnumerable().DefaultIfEmpty(0).Sum();



            //purchase
            var PETaxableAmt = (from b in db.PurchaseEntrys
                                join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                                from c in supp.DefaultIfEmpty()
                                join d in db.Contacts on c.Contact equals d.ContactID into con
                                from d in con.DefaultIfEmpty()
                                where (b.PurchaseType != 2) && b.Status == 1
                                && (emirates == "" || d.State == emirates)
                                && (fromdate == "" || EF.Functions.DateDiffDay(b.PEDate, fdate) <= 0)
                                && (todate == "" || EF.Functions.DateDiffDay(b.PEDate, tdate) >= 0)
                                select b.PESubTotal - b.PEDiscount).AsEnumerable().DefaultIfEmpty(0).Sum();

            var PETaxAmt = (from b in db.PurchaseEntrys
                            join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                            from c in supp.DefaultIfEmpty()
                            join d in db.Contacts on c.Contact equals d.ContactID into con
                            from d in con.DefaultIfEmpty()
                            where (b.PurchaseType != 2) && b.Status == 1
                            && (emirates == "" || d.State == emirates)
                            && (fromdate == "" || EF.Functions.DateDiffDay(b.PEDate, fdate) <= 0)
                            && (todate == "" || EF.Functions.DateDiffDay(b.PEDate, tdate) >= 0)
                            select b.PETaxAmount).AsEnumerable().DefaultIfEmpty(0).Sum();


            //purchase return
            var PRTaxableAmt = (from b in db.PurchaseReturns
                                join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                                from c in supp.DefaultIfEmpty()
                                join d in db.Contacts on c.Contact equals d.ContactID into con
                                from d in con.DefaultIfEmpty()
                                where (b.PurchaseType != 2)
                                && (emirates == "" || d.State == emirates)
                                && (fromdate == "" || EF.Functions.DateDiffDay(b.PRDate, fdate) <= 0)
                                && (todate == "" || EF.Functions.DateDiffDay(b.PRDate, tdate) >= 0)
                                select b.PRSubTotal - b.PRDiscount).AsEnumerable().DefaultIfEmpty(0).Sum();


            var PRTaxAmt = (from b in db.PurchaseReturns
                            join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                            from c in supp.DefaultIfEmpty()
                            join d in db.Contacts on c.Contact equals d.ContactID into con
                            from d in con.DefaultIfEmpty()
                            where (b.PurchaseType != 2)
                            && (emirates == "" || d.State == emirates)
                            && (fromdate == "" || EF.Functions.DateDiffDay(b.PRDate, fdate) <= 0)
                            && (todate == "" || EF.Functions.DateDiffDay(b.PRDate, tdate) >= 0)
                            select b.PRTaxAmount).AsEnumerable().DefaultIfEmpty(0).Sum();

            var Payments = (from a in db.Payments
                            where ((fromdate == "" || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
                            select new
                            {
                                Tax = a.TaxAmount
                            }).ToList();
            var journalIN = (from a in db.AccountsTransactions
                             where a.Purpose == "Journal" && a.Account == 501 && ((fromdate == null || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                             (todate == null || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
                             select new
                             {
                                 Tax = (a.Credit == 0) ? a.Debit : a.Credit,
                             }).ToList();
            var journalOUT = (from a in db.AccountsTransactions
                              where a.Purpose == "Journal" && a.Account == 502 && ((fromdate == null || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                              (todate == null || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
                              select new
                              {
                                  Tax = (a.Credit == 0) ? a.Debit : a.Credit,
                              }).ToList();

            var Payment = Payments.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var JouIN = journalIN.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var JouOUT = journalOUT.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();


            return new QuickSoft.Models.LegacyJsonResult { Data = new { SETaxableAmt, SETaxAmt, SRTaxableAmt, SRTaxAmt, PETaxAmt, PETaxableAmt, PRTaxAmt, PRTaxableAmt, fromdate, todate, Payment, JouIN, JouOUT } };//, preturn = preturn,purchase = purchase, 


        }

        #endregion


        #region purchase tax
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Tax")]
        public ActionResult PurchaseTax()
        {
            ViewBag.Supplier = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);


            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Tax")]
        public ActionResult GetPurchaseTax(string vno, long? supplier, long? type, string fromdate, string todate, string emirates)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            PurchaseEntry pentry = new PurchaseEntry();
            if (type == 1)
            {
                pentry.SupplierType = SupplierType.CashSale;
            }
            else if (type == 0)
            {
                pentry.SupplierType = SupplierType.CreditSale;
            }
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
            var v = (from b in db.PurchaseEntrys
                     join d in db.Suppliers on b.Supplier equals d.SupplierID into supp
                     from d in supp.DefaultIfEmpty()
                     join c in db.Contacts on d.Contact equals c.ContactID into con
                     from c in con.DefaultIfEmpty()
                     where (b.PurchaseType != 2) && b.Status == 1
                     && (vno == "" || b.BillNo == vno)
                     && (emirates == "" || c.State == emirates)
                     && (supplier == null || supplier == 0 || b.Supplier == supplier)
                     && (type == null || b.SupplierType == pentry.SupplierType)
                     &&
                           (fromdate == "" || EF.Functions.DateDiffDay(b.PEDate, fdate) <= 0) &&//b.PEDate
                           (todate == "" || EF.Functions.DateDiffDay(b.PEDate, tdate) >= 0)

                     select new
                     {
                         b.PurchaseEntryId,
                         b.PEDate,
                         b.BillNo,
                         b.PEDiscount,
                         b.PESubTotal,
                         b.PEGrandTotal,
                         b.PETaxAmount,
                         CreatedBranch = db.Branchs.Where(a => a.BranchID == b.Branch).Select(a => a.BranchName).FirstOrDefault(),
                         Supplier = d.SupplierName
                     }).OrderBy(a => a.PEDate);

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


        #region purchase Return tax
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Return Tax")]
        public ActionResult PurchaseReturnTax()
        {
            ViewBag.Supplier = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);


            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Return Tax")]
        public ActionResult GetPurchaseReturnTax(string vno, long? supplier, long? type, string fromdate, string todate, string emirates)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            PurchaseReturn pentry = new PurchaseReturn();
            if (type == 1)
            {
                pentry.SupplierType = SupplierType.CashSale;
            }
            else if (type == 0)
            {
                pentry.SupplierType = SupplierType.CreditSale;
            }

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


            var v = (from b in db.PurchaseReturns
                     join d in db.Suppliers on b.Supplier equals d.SupplierID into supp
                     from d in supp.DefaultIfEmpty()
                     join c in db.Contacts on d.Contact equals c.ContactID into con
                     from c in con.DefaultIfEmpty()
                     where (b.PurchaseType != 2) &&
                     (vno == "" || b.BillNo == vno) &&
                            (emirates == "" || c.State == emirates) &&
                           (supplier == null || supplier == 0 || b.Supplier == supplier) &&
                            (type == null || b.SupplierType == pentry.SupplierType) &&
                           (fromdate == "" || EF.Functions.DateDiffDay(b.PRDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(b.PRDate, tdate) >= 0)
                     select new
                     {
                         b.PurchaseReturnId,
                         b.PRDate,
                         b.BillNo,
                         b.PRDiscount,
                         b.PRSubTotal,
                         b.SupplierType,
                         b.PRGrandTotal,
                         b.PRTaxAmount,
                         CreatedBranch = db.Branchs.Where(a => a.BranchID == b.Branch).Select(a => a.BranchName).FirstOrDefault(),
                         Supplier = d.SupplierName
                     });

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


        #region Sale tax
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Sale Tax")]
        public ActionResult SaleTax()
        {
            ViewBag.Customer = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);

            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            companySet();


            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sale Tax")]
        public ActionResult GetSaleTax(string vno, long? customer, long? type, string fromdate, string todate, string emirates, string satype)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
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


            SalesEntry sEntry = new SalesEntry();

            sEntry.CustomerType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer : CustomerType.Card;

            SaleType St = new SaleType();
            if (satype != "")
            {
                St = (satype == "1") ? SaleType.Sale : SaleType.Hire;
            };

            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Contacts on b.Contact equals c.ContactID into con
                     from c in con.DefaultIfEmpty()
                     where (a.SalesType != 2) && a.Status == 1 &&
                     (vno == "" || a.BillNo == vno) &&
                        (customer == 0 || a.Customer == customer) &&
                         (emirates == "" || c.State == emirates) &&
                        (satype == "" || a.SaleType == St) &&
                        (type == null || a.CustomerType == sEntry.CustomerType) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                     select new
                     {
                         a.SalesEntryId,
                         a.SEDate,
                         a.BillNo,
                         a.SEDiscount,
                         a.SESubTotal,
                         a.SEGrandTotal,
                         a.CustomerType,
                         a.SETaxAmount,
                         CreatedBranch = db.Branchs.Where(c => c.BranchID == a.Branch).Select(a => a.BranchName).FirstOrDefault(),
                         Customer = b.CustomerName,

                     }).OrderBy(a => a.SEDate);


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
        public ActionResult GetSaleTax2(string vno, long? customer, long? type, string fromdate, string todate, string emirates, string satype)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
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


            SalesEntry sEntry = new SalesEntry();

            sEntry.CustomerType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer : CustomerType.Card;

            SaleType St = new SaleType();
            if (satype != "")
            {
                St = (satype == "1") ? SaleType.Sale : SaleType.Hire;
            };

            var v = (from a in db.SalesEntrys

                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Contacts on b.Contact equals c.ContactID into con
                     from c in con.DefaultIfEmpty()
                     where (a.SalesType != 2) && a.Status == 1 &&
                     (vno == "" || a.BillNo == vno) &&
                        (customer == 0 || a.Customer == customer) &&
                         (emirates == "" || c.State == emirates) &&
                        (satype == "" || a.SaleType == St) &&
                        (type == null || a.CustomerType == sEntry.CustomerType) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                     select new
                     {
                         a.SalesEntryId,
                         a.SEDate,
                         // EF Core 10 cannot translate the nested `(...).ToList().Sum()` collection projections
                         // (List<anonymous> inside the executed select). Use correlated scalar Sum() instead —
                         // same value, fully server-translatable.
                         grossamout = db.SEItemss
                                       .Where(d => d.SalesEntry == a.SalesEntryId)
                                       .Sum(d => (decimal?)d.ItemSubTotal) ?? 0,
                         dcharge = db.POSOrders
                                     .Where(e => e.POSOrderId == a.OrderRefer)
                                     .Sum(e => (decimal?)e.dcharge) ?? 0,
                         a.BillNo,
                         a.SEDiscount,
                         a.SESubTotal,
                         a.SEGrandTotal,
                         a.CustomerType,
                         a.SETaxAmount,
                         CreatedBranch = db.Branchs.Where(c => c.BranchID == a.Branch).Select(a => a.BranchName).FirstOrDefault(),
                         Customer = b.CustomerName,

                     }).OrderBy(a => a.SEDate);


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

        #region Sale Return tax
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Sales Return Tax")]
        public ActionResult SaleReturnTax()
        {
            ViewBag.Customer = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);

            companySet();


            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Return Tax")]
        public ActionResult GetSalesReturnTax(string vno, long? customer, long? type, string fromdate, string todate, string emirates)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
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

            SalesReturn sEntry = new SalesReturn();

            sEntry.CustomerType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer : CustomerType.Card;

            var v = (from a in db.SalesReturns
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Contacts on b.Contact equals c.ContactID into con
                     from c in con.DefaultIfEmpty()
                     where (a.SalesType != 2) &&
                     (vno == "" || a.BillNo == vno) &&
                        (emirates == "" || c.State == emirates) &&
                        (customer == 0 || a.Customer == customer) &&
                        (type == null || a.CustomerType == sEntry.CustomerType) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0)
                     select new
                     {
                         a.SalesReturnId,
                         a.SRDate,
                         a.BillNo,
                         a.SRDiscount,
                         a.SRSubTotal,
                         a.SRGrandTotal,
                         a.CustomerType,
                         a.SRTaxAmount,
                         CreatedBranch = db.Branchs.Where(c => c.BranchID == a.Branch).Select(a => a.BranchName).FirstOrDefault(),
                         Customer = b.CustomerName,

                     });


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


        #region Expense Tax
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Expense Tax")]
        public ActionResult ExpenseTax()
        {

            ViewBag.ExpAcc = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);

            companySet();

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Expense Tax")]
        public ActionResult GetExpenseTax(string vno, string fromdate, string todate, long? expacc)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
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

            var v = (from a in db.Payments
                     join b in db.Accountss on a.PayTo equals b.AccountsID into acc
                     from b in acc.DefaultIfEmpty()
                     where (b.Group == 13) && (a.TaxAmount != 0) && (vno == "" || a.VoucherNo == vno) &&
                        (expacc == 0 || b.AccountsID == expacc) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0)
                     select new
                     {
                         a.PaymentId,
                         Date = a.CreatedDate,
                         ExpAcc = b.Name,
                         a.VoucherNo,
                         a.SubTotal,
                         a.TaxAmount,
                         a.Paying,
                         a.GrandTotal
                     });


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


        #region UAE VAT
        [QkAuthorize(Roles = "Dev,UAE VAT")]
        public ActionResult UaeVat()
        {

            return View();
        }


        [QkAuthorize(Roles = "Dev,UAE VAT")]
        public ActionResult GetUaeVat(string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != null)
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (fromdate != null)
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            companySet();
            UaeVatReportViewModel vmodel = new UaeVatReportViewModel();
            vmodel.Sales = (from b in db.SalesEntrys
                            join c in db.Customers on b.Customer equals c.CustomerID into cust
                            from c in cust.DefaultIfEmpty()
                            join cr in db.ContactRelation on c.CustomerID equals cr.RelationID into cnt
                            from cr in cnt.DefaultIfEmpty()
                            join bb in db.States on c.StateID equals bb.StateID into tmp
                            from bb in tmp.DefaultIfEmpty()
                            where (b.SalesType != 2) && b.Status == 1 &&
                            (fromdate == "" || EF.Functions.DateDiffDay(b.SECreatedDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(b.SECreatedDate, tdate) >= 0) &&
                            c.Type == CRMCustomerType.Customer
                            select new UaeVat
                            {
                                Subtotal = b.SESubTotal,
                                Discount = b.SEDiscount,
                                Tax = b.SETaxAmount,
                                State = bb.StateName
                            }).ToList();
            vmodel.SReturn = (from b in db.SalesReturns
                              join c in db.Customers on b.Customer equals c.CustomerID into cust
                              from c in cust.DefaultIfEmpty()
                              join d in db.States on c.StateID equals d.StateID into con
                              from d in con.DefaultIfEmpty()
                              where (b.SalesType != 2) &&
                             (fromdate == "" || EF.Functions.DateDiffDay(b.SRCreatedDate, fdate) <= 0) &&
                             (todate == "" || EF.Functions.DateDiffDay(b.SRCreatedDate, tdate) >= 0)
                              select new UaeVat
                              {
                                  Subtotal = b.SRSubTotal,
                                  Discount = b.SRDiscount,
                                  Tax = b.SRTaxAmount,
                                  State = d.StateName
                              }).ToList();

            vmodel.Purchase = (from b in db.PurchaseEntrys
                               join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                               from c in supp.DefaultIfEmpty()
                               join d in db.Contacts on c.Contact equals d.ContactID into con
                               from d in con.DefaultIfEmpty()
                               where (b.PurchaseType != 2) && b.Status == 1 &&
                        (fromdate == "" || EF.Functions.DateDiffDay(b.PECreatedDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(b.PECreatedDate, tdate) >= 0)
                               select new UaeVat
                               {
                                   Subtotal = b.PESubTotal,
                                   Discount = b.PEDiscount,
                                   Tax = b.PETaxAmount,
                                   State = d.State
                               }).ToList();
            vmodel.PReturn = (from b in db.PurchaseReturns
                              join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                              from c in supp.DefaultIfEmpty()
                              join d in db.Contacts on c.Contact equals d.ContactID into con
                              from d in con.DefaultIfEmpty()
                              where (b.PurchaseType != 2) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(b.PRCreatedDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(b.PRCreatedDate, tdate) >= 0)
                              select new UaeVat
                              {
                                  Subtotal = b.PRSubTotal,
                                  Discount = b.PRDiscount,
                                  Tax = b.PRTaxAmount,
                                  State = d.State
                              }).ToList();

            vmodel.Payments = (from a in db.Payments
                               where ((fromdate == "" || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
                               select new UaeVat
                               {
                                   Subtotal = a.SubTotal,
                                   Discount = a.Discount,
                                   Tax = a.TaxAmount
                               }).ToList();
            var journalIN = (from a in db.AccountsTransactions
                             where a.Purpose == "Journal" && a.Account == 501 && ((fromdate == null || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                             (todate == null || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
                             select new UaeVat
                             {
                                 Subtotal = (a.Credit == 0) ? a.Debit * 20 : a.Credit * 20,
                                 Discount = 0,
                                 Tax = (a.Credit == 0) ? a.Debit : a.Credit,
                             }).ToList();
            var journalOUT = (from a in db.AccountsTransactions
                              where a.Purpose == "Journal" && a.Account == 502 && ((fromdate == null || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                              (todate == null || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
                              select new UaeVat
                              {
                                  Subtotal = (a.Credit == 0) ? a.Debit * 20 : a.Credit * 20,
                                  Discount = 0,
                                  Tax = (a.Credit == 0) ? a.Debit : a.Credit,
                              }).ToList();

            var arr = new ArrayList();
            //Abudhabi
            var AbuSETaxable = vmodel.Sales.Where(a => a.State == "Abu Dhabi").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AbuSETaxAmt = vmodel.Sales.Where(a => a.State == "Abu Dhabi").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AbuSRTaxable = vmodel.SReturn.Where(a => a.State == "Abu Dhabi").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AbuSRTaxAmt = vmodel.SReturn.Where(a => a.State == "Abu Dhabi").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AbuReturn = AbuSETaxable - AbuSRTaxable;
            var AbuTax = AbuSETaxAmt - AbuSRTaxAmt;



            var NetSale = vmodel.Sales.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var NetReturn = vmodel.SReturn.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var NetSaleTax = (NetSale - NetReturn);

            var NetPurchase = vmodel.Purchase.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var NetPReturn = vmodel.PReturn.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var NetPurchaseTax = (NetPurchase - NetPReturn);

            var ExpTax = vmodel.Payments.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();

            var JouIN = journalIN.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var JouOUT = journalOUT.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();

            var PayableTax = NetSaleTax + JouOUT - (NetPurchaseTax + ExpTax + JouIN);
            decimal PTax = 0;
            if (PayableTax < 0)
            {
                PTax = PayableTax + ExpTax;
            }
            else
            {
                PTax = PayableTax - ExpTax;
            }
            //Ajman
            var AjmSETaxable = vmodel.Sales.Where(a => a.State == "Ajman").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AjmSETaxAmt = vmodel.Sales.Where(a => a.State == "Ajman").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AjmSRTaxable = vmodel.SReturn.Where(a => a.State == "Ajman").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AjmSRTaxAmt = vmodel.SReturn.Where(a => a.State == "Ajman").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AjmReturn = AjmSETaxable - AjmSRTaxable;
            var AjmTax = AjmSETaxAmt - AjmSRTaxAmt;

            //Dubai
            var DubSETaxable = vmodel.Sales.Where(a => a.State == "Dubai").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var DubSETaxAmt = vmodel.Sales.Where(a => a.State == "Dubai").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var DubSRTaxable = vmodel.SReturn.Where(a => a.State == "Dubai").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var DubSRTaxAmt = vmodel.SReturn.Where(a => a.State == "Dubai").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var DubReturn = DubSETaxable - DubSRTaxable;
            var DubTax = DubSETaxAmt - DubSRTaxAmt;

            //Fujairah
            var FujSETaxable = vmodel.Sales.Where(a => a.State == "Fujairah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var FujSETaxAmt = vmodel.Sales.Where(a => a.State == "Fujairah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var FujSRTaxable = vmodel.SReturn.Where(a => a.State == "Fujairah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var FujSRTaxAmt = vmodel.SReturn.Where(a => a.State == "Fujairah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var FujReturn = FujSETaxable - FujSRTaxable;
            var FujTax = FujSETaxAmt - FujSRTaxable;

            //Ras al-Khaimah
            var RasSETaxable = vmodel.Sales.Where(a => a.State == "Ras al-Khaimah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var RasSETaxAmt = vmodel.Sales.Where(a => a.State == "Ras al-Khaimah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var RasSRTaxable = vmodel.SReturn.Where(a => a.State == "Ras al-Khaimah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var RasSRTaxAmt = vmodel.SReturn.Where(a => a.State == "Ras al-Khaimah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var RasReturn = RasSETaxable - RasSRTaxable;
            var RasTax = RasSETaxAmt - RasSRTaxAmt;

            //Sharjah
            var shaSETaxable = vmodel.Sales.Where(a => a.State == "Sharjah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var shaSETaxAmt = vmodel.Sales.Where(a => a.State == "Sharjah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var shaSRTaxable = vmodel.SReturn.Where(a => a.State == "Sharjah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var shaSRTaxAmt = vmodel.SReturn.Where(a => a.State == "Sharjah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var shaReturn = shaSETaxable - shaSRTaxable;
            var shaTax = shaSETaxAmt - shaSRTaxAmt;

            //Umm al-Quwain
            var UmmSETaxable = vmodel.Sales.Where(a => a.State == "Umm al-Quwain").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var UmmSETaxAmt = vmodel.Sales.Where(a => a.State == "Umm al-Quwain").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var UmmSRTaxable = vmodel.SReturn.Where(a => a.State == "Umm al-Quwain").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var UmmSRTaxAmt = vmodel.SReturn.Where(a => a.State == "Umm al-Quwain").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var UmmReturn = UmmSETaxable - UmmSRTaxable;
            var UmmTax = UmmSETaxAmt - UmmSRTaxAmt;

            //Abudhabi Purchase
            var AbuPETaxable = vmodel.Purchase.Where(a => a.State == "Abu Dhabi").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AbuPETaxAmt = vmodel.Purchase.Where(a => a.State == "Abu Dhabi").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AbuPRTaxable = vmodel.PReturn.Where(a => a.State == "Abu Dhabi").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AbuPRTaxAmt = vmodel.PReturn.Where(a => a.State == "Abu Dhabi").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AbuPReturn = AbuPETaxable - AbuPRTaxable;
            var AbuPTax = AbuPETaxAmt - AbuPRTaxAmt;

            //Ajman
            var AjmPETaxable = vmodel.Purchase.Where(a => a.State == "Ajman").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AjmPETaxAmt = vmodel.Purchase.Where(a => a.State == "Ajman").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AjmPRTaxable = vmodel.PReturn.Where(a => a.State == "Ajman").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AjmPRTaxAmt = vmodel.PReturn.Where(a => a.State == "Ajman").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var AjmPReturn = AjmPETaxable - AjmPRTaxable;
            var AjmPTax = AjmPETaxAmt - AjmPRTaxAmt;

            //Dubai
            var DubPETaxable = vmodel.Purchase.Where(a => a.State == "Dubai").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var DubPETaxAmt = vmodel.Purchase.Where(a => a.State == "Dubai").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var DubPRTaxable = vmodel.PReturn.Where(a => a.State == "Dubai").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var DubPRTaxAmt = vmodel.PReturn.Where(a => a.State == "Dubai").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var DubPReturn = DubPETaxable - DubPRTaxable;
            var DubPTax = DubPETaxAmt - DubPRTaxAmt;

            //Fujairah
            var FujPETaxable = vmodel.Purchase.Where(a => a.State == "Fujairah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var FujPETaxAmt = vmodel.Purchase.Where(a => a.State == "Fujairah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var FujPRTaxable = vmodel.PReturn.Where(a => a.State == "Fujairah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var FujPRTaxAmt = vmodel.PReturn.Where(a => a.State == "Fujairah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var FujPReturn = FujPETaxable - FujPRTaxable;
            var FujPTax = FujPETaxAmt - FujPRTaxable;

            //Ras al-Khaimah
            var RasPETaxable = vmodel.Purchase.Where(a => a.State == "Ras al-Khaimah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var RasPETaxAmt = vmodel.Purchase.Where(a => a.State == "Ras al-Khaimah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var RasPRTaxable = vmodel.PReturn.Where(a => a.State == "Ras al-Khaimah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var RasPRTaxAmt = vmodel.PReturn.Where(a => a.State == "Ras al-Khaimah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var RasPReturn = RasPETaxable - RasPRTaxable;
            var RasPTax = RasPETaxAmt - RasPRTaxAmt;

            //Sharjah
            var shaPETaxable = vmodel.Purchase.Where(a => a.State == "Sharjah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var shaPETaxAmt = vmodel.Purchase.Where(a => a.State == "Sharjah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var shaPRTaxable = vmodel.PReturn.Where(a => a.State == "Sharjah").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var shaPRTaxAmt = vmodel.PReturn.Where(a => a.State == "Sharjah").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var shaPReturn = shaPETaxable - shaPRTaxable;
            var shaPTax = shaPETaxAmt - shaPRTaxAmt;

            //Umm al-Quwain
            var UmmPETaxable = vmodel.Purchase.Where(a => a.State == "Umm al-Quwain").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var UmmPETaxAmt = vmodel.Purchase.Where(a => a.State == "Umm al-Quwain").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var UmmPRTaxable = vmodel.PReturn.Where(a => a.State == "Umm al-Quwain").Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var UmmPRTaxAmt = vmodel.PReturn.Where(a => a.State == "Umm al-Quwain").Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            var UmmPReturn = UmmPETaxable - UmmPRTaxable;
            var UmmPTax = UmmPETaxAmt - UmmPRTaxAmt;
            var PETaxable = vmodel.Payments.Select(a => (a.Subtotal - a.Discount)).AsEnumerable().DefaultIfEmpty(0).Sum();
            var PETaxAmt = vmodel.Payments.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();

            arr.Add(PETaxable);
            arr.Add(PETaxAmt);

            arr.Add(AbuSETaxable);
            arr.Add(AbuSETaxAmt);
            arr.Add(AbuSRTaxable);
            arr.Add(AbuSRTaxAmt);
            arr.Add(AbuReturn);
            arr.Add(AbuTax);

            arr.Add(AjmSETaxable);
            arr.Add(AjmSETaxAmt);
            arr.Add(AjmSRTaxable);
            arr.Add(AjmSRTaxAmt);
            arr.Add(AjmReturn);
            arr.Add(AjmTax);

            arr.Add(DubSETaxable);
            arr.Add(DubSETaxAmt);
            arr.Add(DubSRTaxable);
            arr.Add(DubSRTaxAmt);
            arr.Add(DubReturn);
            arr.Add(DubTax);

            arr.Add(FujSETaxable);
            arr.Add(FujSETaxAmt);
            arr.Add(FujSRTaxable);
            arr.Add(FujSRTaxAmt);
            arr.Add(FujReturn);
            arr.Add(FujTax);

            arr.Add(RasSETaxable);
            arr.Add(RasSETaxAmt);
            arr.Add(RasSRTaxable);
            arr.Add(RasSRTaxAmt);
            arr.Add(RasReturn);
            arr.Add(RasTax);

            arr.Add(shaSETaxable);
            arr.Add(shaSETaxAmt);
            arr.Add(shaSRTaxable);
            arr.Add(shaSRTaxAmt);
            arr.Add(shaReturn);
            arr.Add(shaTax);

            arr.Add(UmmSETaxable);
            arr.Add(UmmSETaxAmt);
            arr.Add(UmmSRTaxable);
            arr.Add(UmmSRTaxAmt);
            arr.Add(UmmReturn);
            arr.Add(UmmTax);

            arr.Add(AbuPETaxable);
            arr.Add(AbuPETaxAmt);
            arr.Add(AbuPRTaxable);
            arr.Add(AbuPRTaxAmt);
            arr.Add(AbuPReturn);
            arr.Add(AbuPTax);

            arr.Add(AjmPETaxable);
            arr.Add(AjmPETaxAmt);
            arr.Add(AjmPRTaxable);
            arr.Add(AjmPRTaxAmt);
            arr.Add(AjmPReturn);
            arr.Add(AjmPTax);

            arr.Add(DubPETaxable);
            arr.Add(DubPETaxAmt);
            arr.Add(DubPRTaxable);
            arr.Add(DubPRTaxAmt);
            arr.Add(DubPReturn);
            arr.Add(DubPTax);

            arr.Add(FujPETaxable);
            arr.Add(FujPETaxAmt);
            arr.Add(FujPRTaxable);
            arr.Add(FujPRTaxAmt);
            arr.Add(FujPReturn);
            arr.Add(FujPTax);

            arr.Add(RasPETaxable);
            arr.Add(RasPETaxAmt);
            arr.Add(RasPRTaxable);
            arr.Add(RasPRTaxAmt);
            arr.Add(RasPReturn);
            arr.Add(RasPTax);

            arr.Add(shaPETaxable);
            arr.Add(shaPETaxAmt);
            arr.Add(shaPRTaxable);
            arr.Add(shaPRTaxAmt);
            arr.Add(shaPReturn);
            arr.Add(shaPTax);

            arr.Add(UmmPETaxable);
            arr.Add(UmmPETaxAmt);
            arr.Add(UmmPRTaxable);
            arr.Add(UmmPRTaxAmt);
            arr.Add(UmmPReturn);
            arr.Add(UmmPTax);

            arr.Add(NetSale);
            arr.Add(NetReturn);
            arr.Add(NetSaleTax);
            arr.Add(NetPurchase);
            arr.Add(NetPReturn);
            arr.Add(NetPurchaseTax);
            arr.Add(ExpTax);
            arr.Add(PayableTax);
            arr.Add(PTax);

            return new QuickSoft.Models.LegacyJsonResult { Data = new { arr, sales = vmodel.Sales } };//, preturn = preturn,purchase = purchase, 

        }
        #endregion

        #region VatReport
        [QkAuthorize(Roles = "Dev,Vat Report")]
        public ActionResult Vat()
        {
            return View();
        }
        public ActionResult ExpenseTaxReport()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetExpenseTaxReport(string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (fromdate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;

            VatViewModel Vmodel = new VatViewModel();
            Vmodel.from = fromdate;
            Vmodel.to = todate;

            var v = (from a in db.AccountsTransactions
                     join b in db.Accountss on a.Account equals b.AccountsID
                     join c in db.Journals on a.reference equals c.JournalId
                     //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                     join d in db.PDCs on c.JournalId equals d.Reference into pdcs
                     from d in pdcs.DefaultIfEmpty()
                     let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                     let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                     let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                     let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()
                     where (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                     (a.Account == 501) &&
                     (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Journal"
                     select new
                     {
                         AccountId = a.Account,
                         Date = (DateTime?)a.Date,
                         Invoice = c.VoucherNo,
                         Type = "Journal Entry",
                         RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                         RAccountID = bd.AccountsID,
                         Debit = (decimal?)a.Debit,
                         Credit = (decimal?)a.Credit,
                         particulars = b.Name,
                         Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                         Amount = (decimal)c.GrandTotal,
                         MainId = c.JournalId,
                         TRN = bd.TRN,
                         TransactionId = a.Id,
                         Account = a.Account,
                         Reference = a.reference
                     }).ToList();

            var data = v;
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            //    Content = result,
            //    ContentType = "application/json"
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }


        [HttpPost]
        // data-table fields listing
        [QkAuthorize(Roles = "Dev,Vat Report")]
        public ActionResult GetVat(string fromdate, string todate)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != null)
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (fromdate != null)
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            companySet();

            Common com = new Common();
            var ispos = db.companys.Any(o => o.CPName.Contains("FARM KING ROASTERY"));
            LedgerViewModel vmodelOUTPUT = new LedgerViewModel();
            if (!ispos)
             vmodelOUTPUT = com.LedgerDatacommend(502, fromdate, todate, 24, false);//cr
            else
             vmodelOUTPUT = com.LedgerDatacommendpos(502, fromdate, todate, 24, false);//cr

            LedgerViewModel vmodelINPUT = com.LedgerDatacommend(501, fromdate, todate, 24, false);

            decimal? output = 0;
            decimal? input = 0;
            decimal? inputtax = 0;
            decimal? outputamount = 0;
            decimal? inputamount = 0;
            var arrout = new ArrayList();
            foreach (var arr in vmodelOUTPUT.Ledger)
            {
                var x = (arr.Debit == 0) ? arr.Credit : (arr.Debit);
                if (arr.Type != "Journal Entry")
                {
                    output = (arr.Debit == 0) ? (output + x) : (output - x);
                    outputamount = (arr.Debit == 0) ? (outputamount + arr.Amount) : (outputamount - arr.Amount);
                }
                else
                {
                    if (arrout.Contains(arr.Invoice))
                    {
                        output = output + x;
                    }
                    else
                    {
                        arrout.Add(arr.Invoice);
                        output = output + x;
                        inputamount = (inputamount + (arr.Amount - arr.Debit - arr.Credit));
                    }
                }
            }
            var arrin = new ArrayList();
            foreach (var arr in vmodelINPUT.Ledger)
            {
                var x = (arr.Debit == 0) ? (arr.Credit) : arr.Debit;
                if (arr.Type != "Journal Entry")
                {
                    input = (arr.Debit == 0) ? (input - x) : (input + x);
                    inputamount = (arr.Debit == 0) ? (inputamount - arr.Amount) : (inputamount + arr.Amount);
                }

            }

            //                 (todate == null || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0))
            //                     Tax = (a.Credit == 0) ? a.Debit : a.Credit,
            //                     Type = a.Type

            var Account = (from a in db.Accountss
                           where a.AccountsID == 501
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();


            var JournalIn = (from a in db.AccountsTransactions
                             join b in db.Accountss on a.Account equals b.AccountsID
                             join c in db.Journals on a.reference equals c.JournalId
                             join d in db.PDCs on c.JournalId equals d.Reference into pdcs
                             from d in pdcs.DefaultIfEmpty()
                             let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                             let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                             let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                             let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()
                             where (d.PDCType == "Journal" && fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                             (a.Account == 501) &&
                             (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Journal"
                             && (a.Status == null)
                             select new
                             {
                                 id = c.JournalId,
                                 particulars = b.Name,
                                 Date = (DateTime?)a.Date,
                                 Invoice = c.VoucherNo,
                                 Type = "Journal Entry",
                                 RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                                 RAccountID = bd.AccountsID,
                                 Debit = (decimal?)a.Debit,
                                 Credit = (decimal?)a.Credit,
                                 entry = (DateTime?)a.CreatedDate,
                                 Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                                 Amount = (decimal)c.GrandTotal,
                                 TRN = bd.TRN,
                                 TransactionId = a.Id,
                                 Account = a.Account,
                                 reference = a.reference
                             });

            var arrinJournal = new ArrayList();
            try
            {
                foreach (var arr in JournalIn)
                {
                    var x = (arr.Debit == 0) ? (arr.Credit) : arr.Debit;

                    if (arr.Type == "Journal Entry")
                    {
                        if (arrinJournal.Contains(arr.Invoice))
                        {
                            inputtax = inputtax + x;
                        }
                        else
                        {
                            arrinJournal.Add(arr.Invoice);
                            inputtax = inputtax + x;
                        }
                    }
                }
            }
            
        catch(Exception e)
            {
            }
            decimal? ExpenseTax = inputtax;
            var journalIN = (from a in db.AccountsTransactions
                             where a.Purpose == "Journal" && a.Account == 501 && ((fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                             (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0))
                             select new
                             {
                                 Tax = (a.Credit == 0) ? a.Debit : a.Credit,
                             }).ToList();
            var JouIN = journalIN.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();
            ExpenseTax = JouIN;
            VatViewModel Vmodel = new VatViewModel();
            Vmodel.from = fromdate;
            Vmodel.to = todate;
            Vmodel.Inward = input;

            var cresum = vmodelOUTPUT.Ledger.Where(y => y.Debit == 0 && y.MainId != 0).Select(x => x.Amount).Sum();
            var dbtsum = vmodelOUTPUT.Ledger.Where(y => y.Credit == 0 && y.MainId != 0).Select(x => x.Amount).Sum();
            


            Vmodel.Vat = vmodelOUTPUT.Ledger.Where(x => x.MainId != 0);
            Vmodel.from = fromdate;
            Vmodel.to = todate;
            


            Vmodel.Outward = output;
            Vmodel.TotalAmountIn = inputamount;
            Vmodel.TotalAmountOut = cresum - dbtsum;

            Vmodel.ExpenseTax = ExpenseTax;
            Vmodel.Diff = Vmodel.Outward - (Vmodel.Inward + Vmodel.ExpenseTax);


            return View(Vmodel);
        }

        [HttpGet]
        public ActionResult GetDetailsVat(string taxtype, string fromdate, string todate)
        {
            companySet();
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != null)
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (fromdate != null)
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            VatViewModel Vmodel = new VatViewModel();
            LedgerViewModel VatLedger = new LedgerViewModel();
            Common com = new Common();

            if (taxtype == "out")
            {
                //VATOUTPUT
                var ispos = db.companys.Any(o => o.CPName.Contains("FARM KING ROASTERY"));
            if(!ispos)
                VatLedger = com.LedgerDatacommend(502, fromdate, todate, 24, false);//cr                
            else
             VatLedger = com.LedgerDatacommendpos(502, fromdate, todate, 24, false);//cr                

            }
            else
            {
                //VATINPUT
                VatLedger = com.LedgerDatacommend(501, fromdate, todate, 24, false);//dr
            }

            foreach (var arr in VatLedger.Ledger)
            {
                if (arr.Type == "Journal Entry")
                {
                    var LedgerData = VatLedger.Ledger;
                    var Vatinputcount = LedgerData.Where(a => a.Account == 501 && a.Type == "Journal Entry" && a.Reference == arr.Reference).Count();
                    if (Vatinputcount > 0)
                    {
                        var entry = LedgerData.SkipWhile(i => i.TransactionId != arr.TransactionId).Skip(1).FirstOrDefault();
                        var actentry = (from a in db.AccountsTransactions
                                        join b in db.Accountss on a.Account equals b.AccountsID
                                        where a.Purpose == "Journal" && a.reference == arr.Reference
                                        select new
                                        {
                                            TransactionId = a.Id,
                                            particulars = b.Name,
                                            Date = (DateTime?)a.Date,
                                            Debit = (decimal?)a.Debit,
                                            Credit = (decimal?)a.Credit,
                                            entry = (DateTime?)a.CreatedDate,
                                            TRN = b.TRN,
                                            Account = a.Account,
                                            reference = a.reference,
                                            Type = a.Type
                                        }).ToList();
                        var actdta = actentry.SkipWhile(a => a.TransactionId != arr.TransactionId).ToList();
                        var supDet = actdta.Where(a => a.Credit != 0).FirstOrDefault();
                        arr.RAccount = supDet != null ? supDet.particulars : arr.RAccount;
                        arr.TRN = supDet != null ? supDet.TRN : arr.TRN;
                        arr.Amount = supDet != null ? (supDet.Credit - arr.Debit) : arr.Amount;

                    }
                    else
                    {
                        var totamount = (arr.Debit == 0) ? (arr.Amount - arr.Credit) : (arr.Amount - arr.Debit);
                        arr.Amount = totamount;
                    }
                }
                else
                {
                }
            }

            if (taxtype == "out")
            {
                //VATOUTPUT
                var totcre = VatLedger.Ledger.Where(y => y.MainId != 0).Select(x => x.Credit).Sum();
                var totdbt = VatLedger.Ledger.Where(y => y.MainId != 0).Select(x => x.Debit).Sum();
                Vmodel.Vat = VatLedger.Ledger.Where(x => x.MainId != 0);
                Vmodel.from = fromdate;
                Vmodel.to = todate;
                Vmodel.Payable = totcre - totdbt;
                var cresum = VatLedger.Ledger.Where(y => y.Debit == 0 && y.MainId != 0).Select(x => x.Amount).Sum();
                var dbtsum = VatLedger.Ledger.Where(y => y.Credit == 0 && y.MainId != 0).Select(x => x.Amount).Sum();
                Vmodel.Amount = cresum - dbtsum;
                Vmodel.type = "OutWard";
            }
            else if (taxtype == "in")
            {
                //VATINPUT
                var totcre = VatLedger.Ledger.Where(y => y.MainId != 0 && y.Type != "Journal Entry").Select(x => x.Credit).Sum();
                var totdbt = VatLedger.Ledger.Where(y => y.MainId != 0 && y.Type != "Journal Entry").Select(x => x.Debit).Sum();
                Vmodel.Vat = VatLedger.Ledger.Where(x => x.MainId != 0 && x.Type != "Journal Entry");
                Vmodel.from = fromdate;
                Vmodel.to = todate;
                Vmodel.Payable = totdbt - totcre;
                var cresum = VatLedger.Ledger.Where(y => y.Debit == 0 && y.MainId != 0 && y.Type != "Journal Entry").Select(x => x.Amount).Sum();
                var dbtsum = VatLedger.Ledger.Where(y => y.Credit == 0 && y.MainId != 0 && y.Type != "Journal Entry").Select(x => x.Amount).Sum();
                Vmodel.Amount = dbtsum - cresum;
                Vmodel.type = "InWard";
            }
            else
            {
                //VATINPUT -- Expense
                var totcre = VatLedger.Ledger.Where(y => y.MainId != 0 && y.Type == "Journal Entry").Select(x => x.Credit).Sum();
                var totdbt = VatLedger.Ledger.Where(y => y.MainId != 0 && y.Type == "Journal Entry").Select(x => x.Debit).Sum();
                Vmodel.Vat = VatLedger.Ledger.Where(x => x.MainId != 0 && x.Type == "Journal Entry");
                Vmodel.from = fromdate;
                Vmodel.to = todate;
                Vmodel.Payable = totdbt + totcre;
                var cresum = VatLedger.Ledger.Where(y => y.Debit == 0 && y.MainId != 0 && y.Type == "Journal Entry").Select(x => x.Amount).Sum();
                var dbtsum = VatLedger.Ledger.Where(y => y.Credit == 0 && y.MainId != 0 && y.Type == "Journal Entry").Select(x => x.Amount).Sum();
                Vmodel.Amount = dbtsum - cresum;
                Vmodel.type = "Expense";
            }

            Vmodel.Id = VatLedger.Ledger.Where(x => x.MainId != 0).Select(x => x.MainId).FirstOrDefault();

            return View(Vmodel);
        }
        #endregion

        public ActionResult GetNewDetailsVat(string taxtype, string fromdate, string todate)/*string taxtype,*/
        {
            companySet();
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != null)
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (fromdate != null)
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            VatViewModel Vmodel = new VatViewModel();
            Ledger Vvmodel = new Ledger();
            LedgerViewModel VatLedger = new LedgerViewModel();
            Common com = new Common();








            var journalIN = (from a in db.AccountsTransactions
                             where a.Purpose == "Journal" && a.Account == 501 && ((fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                             (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0))
                             select new
                             {
                                 Tax = (a.Credit == 0) ? a.Debit : a.Credit,
                             }).ToList();
            var JouIN = journalIN.Select(a => a.Tax).AsEnumerable().DefaultIfEmpty(0).Sum();



            //VATINPUT -- Expense

            Vmodel.Vat = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          join c in db.Journals on a.reference equals c.JournalId
                          //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                          join d in db.PDCs on c.JournalId equals d.Reference into pdcs
                          from d in pdcs.DefaultIfEmpty()
                          let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                          let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                          let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                          let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()
                          where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                          (a.Account == 501) &&
                          (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Journal"

                          select new Ledger
                          {
                              AccountId = a.Account,
                              Date = (DateTime?)a.Date,
                              Invoice = c.VoucherNo,
                              Type = "Journal Entry",
                              RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                              RAccountID = bd.AccountsID,
                              Debit = (decimal?)a.Debit,
                              Credit = (decimal?)a.Credit,
                              particulars = b.Name,
                              Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                              Amount = (decimal)c.GrandTotal,
                              MainId = c.JournalId,
                              TRN = bd.TRN,
                              TransactionId = a.Id,
                              Account = a.Account,
                              Reference = a.reference
                          });
            Vmodel.from = fromdate;
            Vmodel.to = todate;
            Vmodel.Payable = JouIN;
            Vmodel.type = "Expense";




            return View(Vmodel);
        }
        #region Sale tax2
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Sale Tax")]
        public ActionResult SaleTax2()
        {
            ViewBag.Customer = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);

            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            companySet();


            return View();
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
    }
}
