using System.Linq.Dynamic.Core;
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class DayBookController : BaseController
    {
        ApplicationDbContext db;
        public DayBookController()
        {
            db = new ApplicationDbContext();
        }
        // GET: DayBook
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,DayBook")]
        public ActionResult Index()
        {
            return View();
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,DayBook")]
        public ActionResult GetData(string fromdate, string todate)
        {
            LedgerViewModel vmodel = new LedgerViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Common com = new Common();
            List<long> arr = new List<long>();
            if (fromdate != "")
            {
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }

            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Receipt"
                           select new
                           {
                               id = c.ReceiptId,
                               Date = (DateTime?)a.Date,
                               Invoice = (c.editable == choice.Yes) ? c.VoucherNo : "",
                               Type = a.Purpose,
                               RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).ToList();


            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                     
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment")
                           select new
                           {
                               id = c.PaymentId,
                               Date = (DateTime?)a.Date,
                               Invoice = (c.editable == choice.Yes) ? c.VoucherNo : "",
                               Type = a.Purpose,
                               RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).ToList();
            var journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Journal"
                           select new
                           {
                               id = c.JournalId,
                               Date = (DateTime?)a.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).ToList();

            var Purchase = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                            join d in db.Suppliers on c.Supplier equals d.SupplierID
                            where (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                            select new
                            {
                                id = c.PurchaseEntryId,
                                Date = (DateTime?)c.PEDate,
                                Invoice = c.BillNo,
                                Type = a.Purpose,
                                RAccount = b.Name,
                                RAccountID = b.AccountsID,
                                Debit = (decimal?)a.Debit,
                                Credit = (decimal?)a.Credit,
                            }).ToList();
            var PReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                           join d in db.Suppliers on c.Supplier equals d.SupplierID
                           where (fromdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0) && a.Purpose == "Purchase Return"
                           select new
                           {
                               id = c.PurchaseReturnId,
                               Date = (DateTime?)c.PRDate,
                               Invoice = c.BillNo,
                               Type = a.Purpose,
                               RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).ToList();
            var Sale = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                        join d in db.Customers on c.Customer equals d.CustomerID
                        where (fromdate == null || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                        select new
                        {
                            id = c.SalesEntryId,
                            Date = (DateTime?)c.SEDate,
                            Invoice = c.BillNo,
                            Type = a.Purpose,
                            RAccount = b.Name,
                            RAccountID = b.AccountsID,
                            Debit = (decimal?)a.Debit,
                            Credit = (decimal?)a.Credit,
                        }).ToList();
            var SReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.SalesReturns on a.reference equals c.SalesReturnId
                           join d in db.Customers on c.Customer equals d.CustomerID
                           where (fromdate == null || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0) && a.Purpose == "Sale Return"
                           select new
                           {
                               id = c.SalesReturnId,
                               Date = (DateTime?)c.SRDate,
                               Invoice = c.BillNo,
                               Type = a.Purpose,
                               RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).ToList();


            /* PENDING CREDIT NOTES*/

            //                  (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "CreditNote"
            //                      id = c.CreditnoteId,
            //                      Date = (DateTime?)c.Date,
            //                      Invoice = c.BillNo,
            //                      Type = a.Purpose,
            //                      RAccount = b.Name,
            //                      RAccountID = b.AccountsID,
            //                      Debit = (decimal?)a.Debit,
            //                      Credit = (decimal?)a.Credit,


            var full = Payment.Union(Reciept);
            var pur = Purchase.Union(PReturn);
            var sal = Sale.Union(SReturn);

            full = full.Union(pur);
            full = full.Union(sal);

            full = full.Union(journal);
            

            full = full.AsQueryable().OrderBy("Date asc");
            vmodel.Ledger = (from a in full

                             select new Ledger
                             {
                                 Date = a.Date,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                             }).ToList();


            vmodel.from = from;
            vmodel.to = to;

            companySet();
            return View(vmodel);

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
