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
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class AccountSummaryController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AccountSummaryController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Account Daily Balance
        #region Daily Balance
        [QkAuthorize(Roles = "Dev,Daily Balance")]
        public ActionResult Balances()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);
            return View();
        }
        [QkAuthorize(Roles = "Dev,Daily Balance")]
        public ActionResult GetBalances(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {

            AccountDaySummaryViewModel vmodel = new AccountDaySummaryViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            Common com = new Common();
            long[] Accounts = { };
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
            if (AccId == -1)
            {
                Accounts = com.AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            Balance = com.OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);

            if ((string)Balance["type"] == "Cr")
            {
                vmodel.OpeningBalance = (decimal)Balance["amount"];
                vmodel.blnceType = (string)Balance["type"];
            }
            else
            {
                vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                vmodel.blnceType = (string)Balance["type"];

            }
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;
            // date list
            var count = 0;
            // Transactions

            var full = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account)))
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = a.Id,
                               Date = (DateTime)a.Date,
                               Debit = (decimal)a.Debit,
                               Credit = (decimal)a.Credit,
                               entry = (DateTime)a.CreatedDate,
                           }).GroupBy(x => new { x.Date }, (y, group) => new
                           {
                               Count = group.Select(k => k.id).Count(),
                               Date = (DateTime)y.Date,
                               Debit = (decimal)group.Sum(k => k.Debit),
                               Credit = (decimal)group.Sum(k => k.Credit),
                           });
            var test = full.ToList();
            vmodel.Ledger = (from a in full
                             select new AccountDaySummary
                             {
                                 Date = a.Date,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 Balance = (a.Debit > a.Credit) ? (a.Debit - a.Credit) : (a.Credit - a.Debit),
                                 BalanceType = (a.Debit > a.Credit) ? "Dr." : "Cr."
                             }).ToList();
            companySet();
            return View(vmodel);
        }

        #endregion
        // GET: Account Daily Summary
        #region Daily Summary
       
        public ActionResult Daily()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);
            return View();
        }
        [QkAuthorize(Roles = "Dev,Daily Summary")]
        public ActionResult GetDaily(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {

            AccountDaySummaryViewModel vmodel = new AccountDaySummaryViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            Common com = new Common();
            long[] Accounts = { };
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
            if (AccId == -1)
            {
                Accounts = com.AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            Balance = com.OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);

            if ((string)Balance["type"] == "Cr")
            {
                vmodel.OpeningBalance = (decimal)Balance["amount"];
                vmodel.blnceType = (string)Balance["type"];
            }
            else
            {
                vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                vmodel.blnceType = (string)Balance["type"];

            }
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;
            // date list
            var count = 0;
            DateTime tdate1 = tdate.Value.AddMonths(1).AddDays(-1);
            var dates = new List<DateTime>();
            for (var dt = (DateTime)fdate; dt <= tdate; dt = dt.AddDays(1))
            {
                count++;
                dates.Add(dt.Date);
            }
            var months = new List<DateTime>();

            for (var dt = (DateTime)fdate; dt <= tdate1; dt = dt.AddDays(1))
            {
                count++;
                months.Add(dt.AddDays(1));
            }
            var full = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                        (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account)))
                        && (pdc == true || a.Status == null)
                        select new
                        {
                            id = a.Id,
                            Date = (DateTime)a.Date,
                            Debit = (decimal)a.Debit,
                            Credit = (decimal)a.Credit,
                            entry = (DateTime)a.CreatedDate,
                        }).GroupBy(x => new { x.Date }, (y, group) => new
                        {
                            Count = group.Select(k => k.id).Count(),
                            Date = (DateTime)y.Date,
                            Debit = (decimal)group.Sum(k => k.Debit),
                            Credit = (decimal)group.Sum(k => k.Credit),
                        });
            var test = full.ToList();
            vmodel.Ledger = (from a in full
                             select new AccountDaySummary
                             {
                                 Date = a.Date,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 Balance = (a.Debit > a.Credit) ? (a.Debit - a.Credit) : (a.Credit - a.Debit),
                                 BalanceType = (a.Debit > a.Credit) ? "Dr." : "Cr."
                             }).ToList();
            companySet();
            return View(vmodel);
        }
        #endregion
        // GET: Account Monthly Summary
        #region Monthly Summary
        [QkAuthorize(Roles = "Dev,Monthly Summary")]
        public ActionResult Monthly()
        {
            ViewBag.Account = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                         }, "Value", "Text", 1);
            return View();
        }
        [QkAuthorize(Roles = "Dev,Monthly Summary")]
        public ActionResult GetMonthly(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            AccountMonthSummaryViewModel vmodel = new AccountMonthSummaryViewModel();
            String format = "dd-MM-yyyy";
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            long[] Accounts = { };

            DateTime fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            DateTime? tdate1 = DateTime.Parse(todate, new CultureInfo("en-GB"));
            // get last date of month
            DateTime tdate = tdate1.Value.AddMonths(1).AddDays(-1);
            // date list
            var count = 0;
            var dates = new List<DateTime>();
            for (var dt = (DateTime)fdate; dt <= tdate; dt = dt.AddDays(1))
            {
                count++;
                dates.Add(dt.Date);
            }

            if (AccId == -1)
            {
                Accounts = com.AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            Balance = com.OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);

            if ((string)Balance["type"] == "Cr")
            {
                vmodel.OpeningBalance = (decimal)Balance["amount"];
                vmodel.blnceType = (string)Balance["type"];
            }
            else
            {
                vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                vmodel.blnceType = (string)Balance["type"];

            }
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;
            var full = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                        (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account)))
                        && (pdc == true || a.Status == null)
                        select new
                        {
                            id = a.Id,
                            Date = (DateTime)a.Date,
                            Debit = (decimal)a.Debit,
                            Credit = (decimal)a.Credit,
                            entry = (DateTime)a.CreatedDate,
                        }).GroupBy(x => new { x.Date }, (y, group) => new
                        {
                            Count = group.Select(k => k.id).Count(),
                            Date = (DateTime)y.Date,
                            Debit = (decimal)group.Sum(k => k.Debit),
                            Credit = (decimal)group.Sum(k => k.Credit),
                        });
            var test = full.ToList();
            vmodel.Ledger = (from a in full
                             select new
                             {
                                 Date = a.Date,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                             }).GroupBy(x => new { Years = x.Date.Year, Months = x.Date.Month }, (key, group) => new
                             {
                                 Month = key.Months.ToString(),
                                 Year = key.Years.ToString(),
                                 Debit = group.Sum(k => k.Debit),
                                 Credit = group.Sum(k => k.Credit),
                             }).Select(o => new AccountMonthSummary
                             {
                                 Month = o.Month,
                                 Year = o.Year,
                                 Debit = o.Debit,
                                 Credit = o.Credit,
                                 Balance = (o.Debit > o.Credit) ? (o.Debit - o.Credit) : (o.Credit - o.Debit),
                                 BalanceType = (o.Debit > o.Credit) ? "Dr." : "Cr."
                             }).ToList();
            companySet();
            return View(vmodel);
        }
        #endregion
        // All transaction Summary
        #region transaction Summary
        [QkAuthorize(Roles = "Dev,Transaction Summary")]
        public ActionResult Transaction()
        {
            return View();
        }
        [QkAuthorize(Roles = "Dev,Transaction Summary")]
        public ActionResult GetTransaction(string fromdate, string todate, bool? pdcinclude)
        {
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
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
            TransactionSummaryViewModel vmodel = new TransactionSummaryViewModel();

            vmodel.from = from;
            vmodel.to = to;
            var Account = (from a in db.Accountss
                           select new
                           {
                               a.AccountsID,
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).ToList();

            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Receipt"
                           select new
                           {
                               id = c.ReceiptId,
                               RAccountID =b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
                           {
                               Count = group.Select(k => k.id).Count(),
                               RAccount = y.RAccountID,
                               Debit = (decimal)group.Sum(k => k.Debit),
                               Credit = (decimal)group.Sum(k => k.Credit),
                           }).ToList();
            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment")
                           select new
                           {
                               id = c.PaymentId,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
                           {
                               Count = group.Select(k => k.id).Count(),
                               RAccount = y.RAccountID,
                               Debit = (decimal)group.Sum(k => k.Debit),
                               Credit = (decimal)group.Sum(k => k.Credit),
                           }).ToList();
            var Purchase = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                            where (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0) && a.Purpose == "Purchase"
                            select new
                            {
                                id = c.PurchaseEntryId,
                                RAccountID = b.AccountsID,
                                Debit = (decimal?)a.Debit,
                                Credit = (decimal?)a.Credit,
                            }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
                            {
                                Count = group.Select(k => k.id).Count(),
                                RAccount = y.RAccountID,
                                Debit = (decimal)group.Sum(k => k.Debit),
                                Credit = (decimal)group.Sum(k => k.Credit),
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
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
                           {
                               Count = group.Select(k => k.id).Count(),
                               RAccount = y.RAccountID,
                               Debit = (decimal)group.Sum(k => k.Debit),
                               Credit = (decimal)group.Sum(k => k.Credit),
                           }).ToList();
            var Sale = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                        where (fromdate == null || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) && a.Purpose == "Sale"
                        select new
                        {
                            id = c.SalesEntryId,
                            RAccountID = b.AccountsID,
                            Debit = (decimal?)a.Debit,
                            Credit = (decimal?)a.Credit,
                        }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
                        {
                            Count = group.Select(k => k.id).Count(),
                            RAccount = y.RAccountID,
                            Debit = (decimal)group.Sum(k => k.Debit),
                            Credit = (decimal)group.Sum(k => k.Credit),
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
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
                           {
                               Count = group.Select(k => k.id).Count(),
                               RAccount = y.RAccountID,
                               Debit = (decimal)group.Sum(k => k.Debit),
                               Credit = (decimal)group.Sum(k => k.Credit),
                           }).ToList();
            var Journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Journal"
                           select new
                           {
                               id = c.JournalId,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                           }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
                           {
                               Count = group.Select(k => k.id).Count(),
                               RAccount = y.RAccountID,
                               Debit = (decimal)group.Sum(k => k.Debit),
                               Credit = (decimal)group.Sum(k => k.Credit),
                           }).ToList();
            var Contra = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
                          where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                          (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "ContraVoucher"
                          select new
                          {
                              id = c.ContraVoucherId,
                              RAccountID = b.AccountsID,
                              Debit = (decimal?)a.Debit,
                              Credit = (decimal?)a.Credit,
                          }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
                          {
                              Count = group.Select(k => k.id).Count(),
                              RAccount = y.RAccountID,
                              Debit = (decimal)group.Sum(k => k.Debit),
                              Credit = (decimal)group.Sum(k => k.Credit),
                          }).ToList();
            //              (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Dr. Note"
            //                  id = c.ContraVoucherId,
            //                  RAccountID = b.AccountsID,
            //                  Debit = (decimal?)a.Debit,
            //                  Credit = (decimal?)a.Credit,
            //              }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
            //                  RAccount = y.RAccountID,
            //              (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Cr. Note"
            //                  id = c.ContraVoucherId,
            //                  RAccountID = b.AccountsID,
            //                  Debit = (decimal?)a.Debit,
            //                  Credit = (decimal?)a.Credit,
            //              }).GroupBy(x => new { x.RAccountID }, (y, group) => new
            //                  RAccount = y.RAccountID,
            //----------------------------------------------------------------

            vmodel.Ledger = (from a in Account
                             join b in Reciept on a.AccountsID equals b.RAccount into Rec
                             from b in Rec.DefaultIfEmpty()
                             join c in Payment on a.AccountsID equals c.RAccount into Pay
                             from c in Pay.DefaultIfEmpty()
                             join d in Purchase on a.AccountsID equals d.RAccount into Pur
                             from d in Pur.DefaultIfEmpty()
                             join e in PReturn on a.AccountsID equals e.RAccount into PRet
                             from e in PRet.DefaultIfEmpty()
                             join f in Sale on a.AccountsID equals f.RAccount into Sal
                             from f in Sal.DefaultIfEmpty()
                             join g in SReturn on a.AccountsID equals g.RAccount into SRet
                             from g in SRet.DefaultIfEmpty()
                             join h in Journal on a.AccountsID equals h.RAccount into Jour
                             from h in Jour.DefaultIfEmpty()
                             join i in Contra on a.AccountsID equals i.RAccount into Con
                             from i in Con.DefaultIfEmpty()
                             select new TransactionSummary
                             {
                                 id = a.AccountsID,
                                 Name = a.Name,
                                 Opening = com.AccOpenBlnc(a.AccountsID, fdate, "", pdcinclude),
                                 SaleCr = f != null ? f.Credit : 0,
                                 SaleDr = f != null ? f.Debit : 0,
                                 PurCr = d != null ? d.Credit : 0,
                                 PurDr = d != null ? d.Debit : 0,
                                 PayCr = c != null ? c.Credit : 0,
                                 PayDr = c != null ? c.Debit : 0,
                                 RecCr = b != null ? b.Credit : 0,
                                 RecDr = b != null ? b.Debit : 0,
                                 JourCr = h != null ? h.Credit : 0,
                                 JourDr = h != null ? h.Debit : 0,
                                 SRetCr = g != null ? g.Credit : 0,
                                 SRetDr = g != null ? g.Debit : 0,
                                 PRetCr = e != null ? e.Credit : 0,
                                 PRetDr = e != null ? e.Debit : 0,
                                 ConCr = i != null ? i.Credit : 0,
                                 ConDr = i != null ? i.Debit : 0,
                                 //DrnoteCr = j.Credit,
                                 //DrnoteDr = j.Debit,
                                 //CrnoteCr = k.Credit,
                                 //CrnoteDr = k.Debit,
                             }).Where(o=>o.ConCr!=0||o.ConDr!=0||o.CrnoteCr!=0||o.CrnoteDr != 0 || o.DrnoteCr != 0 || o.DrnoteDr != 0 ||
                             o.JourCr != 0 || o.JourDr != 0 || o.PayCr != 0 || o.PayDr != 0 || o.PRetCr != 0 || o.PRetDr != 0 || o.PurCr != 0 ||
                             o.PurDr != 0 || o.RecCr != 0 || o.RecDr != 0 || o.SaleCr != 0 || o.SaleDr != 0 || o.SRetCr != 0 || o.SRetDr != 0).ToList();
            return View(vmodel);
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
