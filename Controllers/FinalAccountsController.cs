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

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class FinalAccountsController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public FinalAccountsController() 
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        #region Cash/Bank Book
        [QkAuthorize(Roles = "Dev,Cash/Bank Book")]
        public ActionResult CashOrBankBook()
        {
            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Account = QkSelect.List(PaidTo, "ID", "Name");
            _FinancialYear();
            return View();
        }
        [HttpPost]
        // data-table fields listing
        [QkAuthorize(Roles = "Dev,Cash/Bank Book")]
        public ActionResult GetCashBook(long AccId, string fromdate, string todate, bool? pdcinclude)
        {
            LedgerViewModel vmodel = com.LedgerDatacommend(AccId, fromdate, todate, null, pdcinclude);
            companySet();
            return View(vmodel);
        }
        #endregion
        
        #region Payment and Receipt A/C - incomplete
        [QkAuthorize(Roles = "Dev,Pay & Rec A/C")]
        public ActionResult PayandRec()
        {
            return View();
        }
        [QkAuthorize(Roles = "Dev,Pay & Rec A/C")]
        public ActionResult GetPayandRec(string fromdate, string todate, bool? acinclude)
        {
            string AccType = "";
            Dictionary<string, object> Balance = null;
            List<long> arr = new List<long>();
            return View();
        }
        public ActionResult PayandRecDetail(string fromdate, string todate)
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
            var ACCGroups = db.AccountsGroups.Where(a => a.Parent == 0).ToList();
            foreach (var acc in ACCGroups)
            {
                var grouplist = com.AllGroups(acc.AccountsGroupID);
            }



            var accounts = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => r.AccountsID).ToList();
            var groups = accounts.ToArray();

            var Reciept = (from a in db.Receipts
                           join b in db.Accountss on a.PayFrom equals b.AccountsID
                           join c in db.Accountss on a.PayTo equals c.AccountsID
                           join d in db.SalesEntrys on a.Reference equals d.SalesEntryId into sale
                           from d in sale.DefaultIfEmpty()
                           let bb = db.AccountsTransactions.Where(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           groups.Contains(c.Group) && a.editable == choice.No
                           select new
                           {
                               id = a.ReceiptId,
                               particulars = c.Name,
                               Date = (DateTime?)a.Date,
                               Invoice = (a.editable == choice.Yes) ? a.VoucherNo : (d.BillNo != null) ? d.BillNo : "",
                               Type = (a.RefType == "Direct Receipt") ? "Receipt" : a.RefType,
                               RAccount = (b.Name),
                               RAccountID = (long?)b.AccountsID,
                               Debit = (decimal?)a.Paying,
                               Credit = (decimal?)null,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Remark,
                           }).ToList();
            var Payment = (from a in db.Payments
                           join b in db.Accountss on a.PayTo equals b.AccountsID
                           join c in db.Accountss on a.PayFrom equals c.AccountsID
                           join d in db.PurchaseEntrys on a.Reference equals d.PurchaseEntryId into pur
                           from d in pur.DefaultIfEmpty()
                           let bb = db.AccountsTransactions.Where(at => at.Purpose == "Payment" && at.reference == a.PaymentId).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           groups.Contains(c.Group) && a.editable == choice.No
                           select new
                           {
                               id = a.PaymentId,
                               particulars = c.Name,
                               Date = (DateTime?)a.Date,
                               Invoice = (a.editable == choice.Yes) ? a.VoucherNo : (d.BillNo != null) ? d.BillNo : "",
                               Type = (a.RefType == "Direct Payment") ? "Payment" : (a.RefType == "SalesReturn") ? "Sales Return" : a.RefType,
                               RAccount = b.Name,
                               RAccountID = (long?)b.AccountsID,
                               Debit = (decimal?)null,
                               Credit = (decimal?)a.Paying,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Remark,

                           }).ToList();
            var JournalDr = (from a in db.Journals
                             join b in db.Accountss on a.PayFrom equals b.AccountsID
                             join c in db.Accountss on a.PayTo equals c.AccountsID
                             where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                             (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                             groups.Contains(c.Group)
                             select new
                             {
                                 id = a.JournalId,
                                 particulars = c.Name,

                                 Date = (DateTime?)a.Date,
                                 Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                 Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
                                 RAccount = b.Name,
                                 RAccountID = (long?)b.AccountsID,
                                 Debit = (decimal?)a.Paying,
                                 Credit = (decimal?)null,
                                 entry = (DateTime?)a.CreatedDate,
                                 Remark = a.Remark,

                             }).ToList();
            var JournalCr = (from a in db.Journals
                             join b in db.Accountss on a.PayTo equals b.AccountsID
                             join c in db.Accountss on a.PayFrom equals c.AccountsID
                             where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                             (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                             groups.Contains(c.Group)
                             select new
                             {
                                 id = a.JournalId,
                                 particulars = c.Name,
                                 Date = (DateTime?)a.Date,
                                 Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                 Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
                                 RAccount = b.Name,
                                 RAccountID = (long?)b.AccountsID,
                                 Debit = (decimal?)null,
                                 Credit = (decimal?)a.Paying,
                                 entry = (DateTime?)a.CreatedDate,
                                 Remark = a.Remark,
                             }).ToList();
            var full = Payment.Union(Reciept);
            var journal = JournalCr.Union(JournalDr);
            full = full.Union(journal);

            full = full.AsQueryable().OrderBy("Date asc, entry asc");
            var Ledger = (from a in full
                          select new Ledger
                          {
                              Date = a.Date,
                              Invoice = a.Invoice,
                              Type = a.Type,
                              RAccount = a.RAccount,
                              RAccountID = a.RAccountID,
                              Debit = a.Debit,
                              Credit = a.Credit,
                              particulars = a.particulars,
                              Remark = a.Remark,
                          }).ToList();
            return View();
        }
        #endregion

    
        #region Ledger Not Using -> old code for reference
        [QkAuthorize(Roles = "Dev,Ledger")]
        public ActionResult Ledger()
        {
         
            return View();
        }
        [HttpPost]
        // datatable fields listing
        [QkAuthorize(Roles = "Dev,Ledger")]
        public ActionResult GetLedger(long AccId, string fromdate, string todate, long? AccGroup, bool? pdcinclude)
        {

            LedgerViewModel vmodel = new LedgerViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;

            string AccType = "";
            Dictionary<string, object> Balance = null;
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
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.AccountsGroupID,
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            if (AccId == -1)
            {
                AccType = com.AccType(AccId, AccGroup);
                var cusparentid = new SqlParameter("@parentid", AccGroup);
                var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).AsEnumerable().ToList();
                arr = cusgroupsdata.Select(a => a.AccountsGroupID).ToList();
                long[] arry = db.Accountss.Where(a => arr.Contains(a.Group)).Select(a => a.AccountsID).ToArray();
                Balance = com.GroupOpenBlnc((long)AccGroup, fdate, AccType, pdcinclude, arry);
            }
            else
            {
                AccType = com.AccType(AccId);
                arr.Add(AccId);
                DateTime startD = (DateTime)fdate;
                Balance = com.AccOpenBlnc(AccId, startD, AccType, pdcinclude);
            }
            var groups = arr.ToArray();
            if (AccType == "Customer" && AccId != -1)
            {
                var Account = (from a in db.Customers
                               join b in db.Accountss on a.Accounts equals b.AccountsID
                               where a.Accounts == AccId || AccId == -1
                               select new
                               {
                                   a.CustomerCode,
                                   a.CustomerName,
                                   a.CustomerID,
                                   b.OpnBalance,
                                   b.OpnBalanceCr
                               }).FirstOrDefault();
                var Sale = (from a in db.SalesEntrys
                            join b in db.Customers on a.Customer equals b.CustomerID
                            where (fromdate == null || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                            (todate == null || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) && (b.Accounts == AccId || AccId == -1)
                            select new
                            {
                                id = a.SalesEntryId,
                                particulars = b.CustomerName,
                                Date = (DateTime?)a.SEDate,
                                Invoice = a.BillNo,
                                Type = "Sale",
                                RAccount = "Sale",
                                RAccountID = (long?)null,
                                Debit = (decimal?)a.SEGrandTotal,
                                Credit = (decimal?)null,
                                entry = (DateTime?)a.SECreatedDate,
                                Remark = a.Remarks,
                            });
                var SReturn = (from a in db.SalesReturns
                               join b in db.Customers on a.Customer equals b.CustomerID
                               where (fromdate == null || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0) && (b.Accounts == AccId || AccId == -1)
                               select new
                               {
                                   id = a.SalesReturnId,
                                   particulars = b.CustomerName,

                                   Date = (DateTime?)a.SRDate,
                                   Invoice = a.BillNo,
                                   Type = "Sales Return",
                                   RAccount = "Sales Return",
                                   RAccountID = (long?)null,
                                   Debit = (decimal?)null,
                                   Credit = (decimal?)a.SRGrandTotal,
                                   entry = (DateTime?)a.SRCreatedDate,
                                   Remark = a.Remarks,
                               });
                var Reciept = (from a in db.Receipts
                               join b in db.Accountss on a.PayTo equals b.AccountsID
                               join c in db.Accountss on a.PayFrom equals c.AccountsID
                               let bb = db.AccountsTransactions.Where(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId).FirstOrDefault()
                               where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayFrom == AccId || (AccId == -1 && groups.Contains(c.Group))) &&
                               (pdcinclude != true || bb != null || a.editable == choice.No)
                               select new
                               {
                                   id = a.ReceiptId,
                                   particulars = c.Name,

                                   Date = (DateTime?)a.Date,
                                   Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                   Type = (a.editable == choice.Yes) ? "Receipt" : "",
                                   RAccount = b.Name,
                                   RAccountID = (long?)b.AccountsID,
                                   Debit = (decimal?)null,
                                   Credit = (decimal?)a.Paying,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = a.Remark,
                               });
                var Payment = (from a in db.Payments
                               join b in db.Accountss on a.PayFrom equals b.AccountsID
                               join c in db.Accountss on a.PayTo equals c.AccountsID
                               let bb = db.AccountsTransactions.Where(at => at.Purpose == "Payment" && at.reference == a.PaymentId).FirstOrDefault()
                               where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayTo == AccId || (AccId == -1 && groups.Contains(c.Group))) &&
                               (pdcinclude != true || bb != null || a.editable == choice.No)
                               select new
                               {
                                   id = a.PaymentId,
                                   particulars = c.Name,

                                   Date = (DateTime?)a.Date,
                                   Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                   Type = (a.editable == choice.Yes) ? "Payment" : "",
                                   RAccount = b.Name,
                                   RAccountID = (long?)b.AccountsID,
                                   Debit = (decimal?)a.Paying,
                                   Credit = (decimal?)null,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = a.Remark,
                               });
                var JournalDr = (from a in db.Journals
                                 join b in db.Accountss on a.PayFrom equals b.AccountsID
                                 join c in db.Accountss on a.PayTo equals c.AccountsID
                                 where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayTo == AccId || (AccId == -1 && groups.Contains(c.Group)))
                                 select new
                                 {
                                     id = a.JournalId,
                                     particulars = c.Name,

                                     Date = (DateTime?)a.Date,
                                     Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                     Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
                                     RAccount = b.Name,
                                     RAccountID = (long?)b.AccountsID,
                                     Debit = (decimal?)a.Paying,
                                     Credit = (decimal?)null,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = a.Remark,

                                 });
                var JournalCr = (from a in db.Journals
                                 join b in db.Accountss on a.PayTo equals b.AccountsID
                                 join c in db.Accountss on a.PayFrom equals c.AccountsID
                                 where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayFrom == AccId || (AccId == -1 && groups.Contains(c.Group)))
                                 select new
                                 {
                                     id = a.JournalId,
                                     particulars = c.Name,
                                     Date = (DateTime?)a.Date,
                                     Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                     Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
                                     RAccount = b.Name,
                                     RAccountID = (long?)b.AccountsID,
                                     Debit = (decimal?)null,
                                     Credit = (decimal?)a.Paying,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = a.Remark,

                                 });
                var left = Sale.Union(Payment);
                var right = SReturn.Union(Reciept);
                var journal = JournalCr.Union(JournalDr);
                right = right.Union(journal);

                var full = left.Union(right);
                full = full.AsQueryable().OrderBy("Date asc, entry asc");
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
                                     particulars = a.particulars,
                                     Remark = a.Remark,
                                 }).ToList();
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
                vmodel.MainAccount = (AccId != -1) ? Account.CustomerName : Group.Name;
                vmodel.MainAccountID = AccId;
                vmodel.from = from;
                vmodel.to = to;
            }
            else if (AccType == "Supplier" && AccId != -1)
            {
                var Account = (from a in db.Suppliers
                               join b in db.Accountss on a.Accounts equals b.AccountsID
                               where a.Accounts == AccId
                               select new
                               {
                                   a.SupplierCode,
                                   a.SupplierName,
                                   a.SupplierID,
                                   b.OpnBalance,
                                   b.OpnBalanceCr
                               }).FirstOrDefault();

                var Purchase = (from a in db.PurchaseEntrys
                                join b in db.Suppliers on a.Supplier equals b.SupplierID
                                where (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                                (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) && (b.Accounts == AccId || AccId == -1)
                                select new
                                {
                                    id = a.PurchaseEntryId,
                                    particulars = b.SupplierName,
                                    Date = (DateTime?)a.PEDate,
                                    Invoice = a.BillNo,
                                    Type = "Purchase",
                                    RAccount = "Purchase",
                                    RAccountID = (long?)null,
                                    Debit = (decimal?)null,
                                    Credit = (decimal?)a.PEGrandTotal,
                                    entry = (DateTime?)a.PECreatedDate,
                                    Remark = a.Remarks,
                                });
                var PReturn = (from a in db.PurchaseReturns
                               join b in db.Suppliers on a.Supplier equals b.SupplierID
                               where (fromdate == "" || EF.Functions.DateDiffDay(a.PRDate, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(a.PRDate, tdate) >= 0) && (b.Accounts == AccId || AccId == -1)
                               select new
                               {
                                   id = a.PurchaseReturnId,
                                   particulars = b.SupplierName,
                                   Date = (DateTime?)a.PRDate,
                                   Invoice = a.BillNo,
                                   Type = "Purchase Return",
                                   RAccount = "Purchase Return",
                                   RAccountID = (long?)null,
                                   Debit = (decimal?)a.PRGrandTotal,
                                   Credit = (decimal?)null,
                                   entry = (DateTime?)a.PRCreatedDate,
                                   Remark = a.Remarks,
                               });
                var Reciept = (from a in db.Receipts
                               join b in db.Accountss on a.PayTo equals b.AccountsID
                               join c in db.Accountss on a.PayFrom equals c.AccountsID
                               let bb = db.AccountsTransactions.Where(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId).FirstOrDefault()
                               where (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayFrom == AccId || (AccId == -1 && groups.Contains(c.Group))) &&
                               (pdcinclude != true || bb != null || a.editable == choice.No)
                               select new
                               {
                                   id = a.ReceiptId,
                                   particulars = c.Name,

                                   Date = (DateTime?)a.Date,
                                   Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                   Type = (a.editable == choice.Yes) ? "Receipt" : "",
                                   RAccount = b.Name,
                                   RAccountID = (long?)b.AccountsID,
                                   Debit = (decimal?)null,
                                   Credit = (decimal?)a.Paying,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = a.Remark,
                               });
                var Payment = (from a in db.Payments
                               join b in db.Accountss on a.PayFrom equals b.AccountsID
                               join c in db.Accountss on a.PayTo equals c.AccountsID
                               let bb = db.AccountsTransactions.Where(at => at.Purpose == "Payment" && at.reference == a.PaymentId).FirstOrDefault()
                               where (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayTo == AccId || (AccId == -1 && groups.Contains(c.Group))) &&
                               (pdcinclude != true || bb != null || a.editable == choice.No)
                               select new
                               {
                                   id = a.PaymentId,
                                   particulars = c.Name,

                                   Date = (DateTime?)a.Date,
                                   Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                   Type = (a.editable == choice.Yes) ? "Payment" : "",
                                   RAccount = b.Name,
                                   RAccountID = (long?)b.AccountsID,
                                   Debit = (decimal?)a.Paying,
                                   Credit = (decimal?)null,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = a.Remark,

                               });
                var JournalDr = (from a in db.Journals
                                 join b in db.Accountss on a.PayFrom equals b.AccountsID
                                 join c in db.Accountss on a.PayTo equals c.AccountsID
                                 where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayTo == AccId || (AccId == -1 && groups.Contains(c.Group)))
                                 select new
                                 {
                                     id = a.JournalId,
                                     particulars = c.Name,

                                     Date = (DateTime?)a.Date,
                                     Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                     Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
                                     RAccount = b.Name,
                                     RAccountID = (long?)b.AccountsID,
                                     Debit = (decimal?)a.Paying,
                                     Credit = (decimal?)null,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = a.Remark,

                                 });
                var JournalCr = (from a in db.Journals
                                 join b in db.Accountss on a.PayTo equals b.AccountsID
                                 join c in db.Accountss on a.PayFrom equals c.AccountsID
                                 where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayFrom == AccId || (AccId == -1 && groups.Contains(c.Group)))
                                 select new
                                 {
                                     id = a.JournalId,
                                     particulars = c.Name,
                                     Date = (DateTime?)a.Date,
                                     Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
                                     Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
                                     RAccount = b.Name,
                                     RAccountID = (long?)b.AccountsID,
                                     Debit = (decimal?)null,
                                     Credit = (decimal?)a.Paying,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = a.Remark,
                                 });
                var left = PReturn.Union(Payment);
                var right = Purchase.Union(Reciept);
                var journal = JournalCr.Union(JournalDr);
                right = right.Union(journal);
                var full = left.Union(right);
                full = full.AsQueryable().OrderBy("Date asc, entry asc");
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
                                     particulars = a.particulars,
                                     Remark = a.Remark,
                                 }).ToList();
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
                vmodel.MainAccount = (AccId != -1) ? Account.SupplierName : Group.Name;
                vmodel.MainAccountID = AccId;
                vmodel.from = from;
                vmodel.to = to;
            }
            //                   where a.AccountsID == AccId
            //                       a.AccountsID,
            //                       a.Name,
            //                       a.Alias,
            //                       a.OpnBalance,
            //                       a.OpnBalanceCr

            //                   let bb = db.AccountsTransactions.Where(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId).FirstOrDefault()
            //                   (pdcinclude != true || bb != null || a.editable == choice.No)
            //                       id = a.ReceiptId,
            //                       particulars = c.Name,

            //                       Date = (DateTime?)a.Date,
            //                       Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
            //                       Type = (a.editable == choice.Yes) ? "Receipt" : "",
            //                       RAccount = b.Name,
            //                       RAccountID = (long?)b.AccountsID,
            //                       Debit = (decimal?)null,
            //                       Credit = (decimal?)a.Paying,
            //                       entry = (DateTime?)a.CreatedDate,
            //                       Remark = a.Remark,
            //                   let bb = db.AccountsTransactions.Where(at => at.Purpose == "Payment" && at.reference == a.PaymentId).FirstOrDefault()
            //                   (pdcinclude != true || bb != null || a.editable == choice.No)

            //                       id = a.PaymentId,
            //                       particulars = c.Name,

            //                       Date = (DateTime?)a.Date,
            //                       Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
            //                       Type = (a.editable == choice.Yes) ? "Payment" : "",
            //                       RAccount = b.Name,
            //                       RAccountID = (long?)b.AccountsID,
            //                       Debit = (decimal?)a.Paying,
            //                       Credit = (decimal?)null,
            //                       entry = (DateTime?)a.CreatedDate,
            //                       Remark = a.Remark,
            //                     (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayTo == AccId || (AccId == -1 && groups.Contains(c.Group)))
            //                         id = a.JournalId,
            //                         particulars = c.Name,

            //                         Date = (DateTime?)a.Date,
            //                         Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
            //                         Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
            //                         RAccount = b.Name,
            //                         RAccountID = (long?)b.AccountsID,
            //                         Debit = (decimal?)a.Paying,
            //                         Credit = (decimal?)null,
            //                         entry = (DateTime?)a.CreatedDate,
            //                         Remark = a.Remark,
            //                     (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayFrom == AccId || (AccId == -1 && groups.Contains(c.Group)))
            //                         id = a.JournalId,
            //                         particulars = c.Name,
            //                         Date = (DateTime?)a.Date,
            //                         Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
            //                         Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
            //                         RAccount = b.Name,
            //                         RAccountID = (long?)b.AccountsID,
            //                         Debit = (decimal?)null,
            //                         Credit = (decimal?)a.Paying,
            //                         entry = (DateTime?)a.CreatedDate,
            //                         Remark = a.Remark,
            //                         Date = a.Date,
            //                         Invoice = a.Invoice,
            //                         Type = a.Type,
            //                         RAccount = a.RAccount,
            //                         RAccountID = a.RAccountID,
            //                         Debit = a.Debit,
            //                         Credit = a.Credit,
            //                         particulars = a.particulars,
            //                         Remark = a.Remark,

            //                   where a.AccountsID == AccId
            //                       a.AccountsID,
            //                       a.Name,
            //                       a.Alias,
            //                       a.OpnBalance,
            //                       a.OpnBalanceCr
            //                   let bb = db.AccountsTransactions.Where(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId).FirstOrDefault()
            //                   (pdcinclude != true || bb != null || a.editable == choice.No)
            //                       id = a.ReceiptId,
            //                       particulars = c.Name,

            //                       Date = (DateTime?)a.Date,
            //                       Invoice = (a.editable == choice.Yes) ? a.VoucherNo : (d.BillNo != null) ? d.BillNo : "",
            //                       //  Type = "Receipt",
            //                       Type = (a.RefType == "Direct Receipt") ? "Receipt" : a.RefType,
            //                       RAccountID = (long?)b.AccountsID,
            //                       Debit = (decimal?)a.Paying,
            //                       Credit = (decimal?)null,
            //                       entry = (DateTime?)a.CreatedDate,
            //                       Remark = a.Remark,
            //                   let bb = db.AccountsTransactions.Where(at => at.Purpose == "Payment" && at.reference == a.PaymentId).FirstOrDefault()
            //                   (pdcinclude != true || bb != null || a.editable == choice.No)
            //                       id = a.PaymentId,
            //                       particulars = c.Name,

            //                       Date = (DateTime?)a.Date,
            //                       Invoice = (a.editable == choice.Yes) ? a.VoucherNo : (d.BillNo != null) ? d.BillNo : "",
            //                       //  Type = "Payment",
            //                       Type = (a.RefType == "Direct Payment") ? "Payment" : (a.RefType == "SalesReturn") ? "Sales Return" : a.RefType,
            //                       RAccount = b.Name,
            //                       RAccountID = (long?)b.AccountsID,
            //                       Debit = (decimal?)null,
            //                       Credit = (decimal?)a.Paying,
            //                       entry = (DateTime?)a.CreatedDate,
            //                       Remark = a.Remark,

            //                     (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayTo == AccId || (AccId == -1 && groups.Contains(c.Group)))
            //                         id = a.JournalId,
            //                         particulars = c.Name,

            //                         Date = (DateTime?)a.Date,
            //                         Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
            //                         Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
            //                         RAccount = b.Name,
            //                         RAccountID = (long?)b.AccountsID,
            //                         Debit = (decimal?)a.Paying,
            //                         Credit = (decimal?)null,
            //                         entry = (DateTime?)a.CreatedDate,
            //                         Remark = a.Remark,

            //                     (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayFrom == AccId || (AccId == -1 && groups.Contains(c.Group)))
            //                         id = a.JournalId,
            //                         particulars = c.Name,
            //                         Date = (DateTime?)a.Date,
            //                         Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
            //                         Type = (a.editable == choice.Yes) ? "Journal Entry" : "",
            //                         RAccount = b.Name,
            //                         RAccountID = (long?)b.AccountsID,
            //                         Debit = (decimal?)null,
            //                         Credit = (decimal?)a.Paying,
            //                         entry = (DateTime?)a.CreatedDate,
            //                         Remark = a.Remark,

            //                    (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayTo == AccId || (AccId == -1 && groups.Contains(c.Group)))
            //                        id = a.ContraVoucherId,
            //                        particulars = c.Name,
            //                        Date = (DateTime?)a.Date,
            //                        Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
            //                        Type = (a.editable == choice.Yes) ? "Contra Voucher" : "",
            //                        RAccount = b.Name,
            //                        RAccountID = (long?)b.AccountsID,
            //                        Debit = (decimal?)a.Amount,
            //                        Credit = (decimal?)null,
            //                        entry = (DateTime?)a.CreatedDate,
            //                        Remark = a.Remark
            //                    (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && (a.PayFrom == AccId || (AccId == -1 && groups.Contains(c.Group)))
            //                        id = a.ContraVoucherId,
            //                        particulars = c.Name,
            //                        Date = (DateTime?)a.Date,
            //                        Invoice = (a.editable == choice.Yes) ? a.VoucherNo : "",
            //                        Type = (a.editable == choice.Yes) ? "Contra Voucher" : "",
            //                        RAccount = b.Name,
            //                        RAccountID = (long?)b.AccountsID,
            //                        Debit = (decimal?)null,
            //                        Credit = (decimal?)a.Amount,
            //                        entry = (DateTime?)a.CreatedDate,
            //                        Remark = a.Remark

            //                         Date = a.Date,
            //                         Invoice = a.Invoice,
            //                         Type = a.Type,
            //                         RAccount = a.RAccount,
            //                         RAccountID = a.RAccountID,
            //                         Debit = a.Debit,
            //                         Credit = a.Credit,
            //                         particulars = a.particulars,
            //                         Remark = a.Remark,

            else
            {

                long[] acclist = { };
                if (AccId == -1)
                {
                    acclist = db.Accountss.Where(a => groups.Contains(a.Group)).Select(a => a.AccountsID).ToArray();
                }
                var Account = (from a in db.Accountss
                               where a.AccountsID == AccId
                               select new
                               {
                                   a.AccountsID,
                                   a.Name,
                                   a.Alias,
                                   a.OpnBalance,
                                   a.OpnBalanceCr
                               }).FirstOrDefault();

                var Reciept = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.Receipts on a.reference equals c.ReceiptId
                               where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                               (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) && (a.Purpose == "Receipt")
                               select new
                               {
                                   id = c.ReceiptId,
                                   particulars = b.Name,
                                   Date = (DateTime?)a.Date,
                                   Invoice = (c.editable == choice.Yes) ? c.VoucherNo : "",
                                   Type = a.Purpose,
                                   RAccount = b.Name,
                                   RAccountID = b.AccountsID,
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = c.Remark
                               });
                var Payment = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.Payments on a.reference equals c.PaymentId
                               where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                               (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment")
                               select new
                               {
                                   id = c.PaymentId,
                                   particulars = b.Name,

                                   Date = (DateTime?)a.Date,
                                   Invoice = (c.editable == choice.Yes) ? c.VoucherNo : "",
                                   Type = a.Purpose,
                                   RAccount = b.Name,
                                   RAccountID = b.AccountsID,
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = c.Remark
                               });

                var Purchase = (from a in db.AccountsTransactions
                                join b in db.Accountss on a.Account equals b.AccountsID
                                join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                                join d in db.Suppliers on c.Supplier equals d.SupplierID
                                where (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                                (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0) &&
                                (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                                select new
                                {
                                    id = c.PurchaseEntryId,
                                    particulars = d.SupplierName,

                                    Date = (DateTime?)c.PEDate,
                                    Invoice = c.BillNo,
                                    Type = a.Purpose,
                                    RAccount = b.Name,
                                    RAccountID = b.AccountsID,
                                    Debit = (decimal?)a.Debit,
                                    Credit = (decimal?)a.Credit,
                                    entry = (DateTime?)a.CreatedDate,
                                    Remark = c.Remarks
                                });
                var PReturn = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                               join d in db.Suppliers on c.Supplier equals d.SupplierID
                               where (fromdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0) &&
                               (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                               select new
                               {
                                   id = c.PurchaseReturnId,
                                   particulars = d.SupplierName,
                                   Date = (DateTime?)c.PRDate,
                                   Invoice = c.BillNo,
                                   Type = a.Purpose,
                                   RAccount = b.Name,
                                   RAccountID = b.AccountsID,
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = c.Remarks

                               });
                var Sale = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                            join d in db.Customers on c.Customer equals d.CustomerID
                            where (fromdate == null || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                            (todate == null || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) &&
                            (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                            select new
                            {
                                id = c.SalesEntryId,
                                particulars = d.CustomerName,

                                Date = (DateTime?)c.SEDate,
                                Invoice = c.BillNo,
                                Type = a.Purpose,
                                RAccount = b.Name,
                                RAccountID = b.AccountsID,
                                Debit = (decimal?)a.Debit,
                                Credit = (decimal?)a.Credit,
                                entry = (DateTime?)a.CreatedDate,
                                Remark = c.Remarks
                            });
                var SReturn = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.SalesReturns on a.reference equals c.SalesReturnId
                               join d in db.Customers on c.Customer equals d.CustomerID
                               where (fromdate == null || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0) &&
                               (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                               select new
                               {
                                   id = c.SalesReturnId,
                                   particulars = d.CustomerName,
                                   Date = (DateTime?)c.SRDate,
                                   Invoice = c.BillNo,
                                   Type = a.Purpose,
                                   RAccount = b.Name,
                                   RAccountID = b.AccountsID,
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = c.Remarks
                               });
                var Journal = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.Journals on a.reference equals c.JournalId
                               where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                               (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) &&
                               (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Journal"
                               select new
                               {
                                   id = c.JournalId,
                                   particulars = b.Name,
                                   Date = (DateTime?)a.Date,
                                   Invoice = c.VoucherNo,
                                   Type = "Journal Entry",
                                   RAccount = b.Name,
                                   RAccountID = b.AccountsID,
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = c.Remark
                               });
                var Contra = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
                              where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                              (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) &&
                              (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "ContraVoucher"
                              select new
                              {
                                  id = c.ContraVoucherId,
                                  particulars = b.Name,
                                  Date = (DateTime?)a.Date,
                                  Invoice = c.VoucherNo,
                                  Type = "Contra Voucher",
                                  RAccount = b.Name,
                                  RAccountID = b.AccountsID,
                                  Debit = (decimal?)a.Debit,
                                  Credit = (decimal?)a.Credit,
                                  entry = (DateTime?)a.CreatedDate,
                                  Remark = c.Remark
                              });
                var StockAdjustment = (from a in db.AccountsTransactions
                                       join b in db.Accountss on a.Account equals b.AccountsID
                                       join c in db.StockAdjustments on a.reference equals c.StockAdjustmentId
                                       where (fromdate == null || EF.Functions.DateDiffDay(c.AdjDate, fdate) <= 0) &&
                                       (a.Account == AccId || (AccId == -1 && acclist.Contains(a.Account))) &&
                                       (todate == null || EF.Functions.DateDiffDay(c.AdjDate, tdate) >= 0) && a.Purpose == "Stock Adjustment"
                                       select new
                                       {
                                           id = c.StockAdjustmentId,
                                           particulars = b.Name,
                                           Date = (DateTime?)a.Date,
                                           Invoice = c.VoucherNo,
                                           Type = "Contra Voucher",
                                           RAccount = b.Name,
                                           RAccountID = b.AccountsID,
                                           Debit = (decimal?)a.Debit,
                                           Credit = (decimal?)a.Credit,
                                           entry = (DateTime?)a.CreatedDate,
                                           Remark = c.Reason
                                       });

                var full = Payment.Union(Reciept);
                var pur = Purchase.Union(PReturn);
                var sal = Sale.Union(SReturn);
                var joc = Journal.Union(Contra);
                full = full.Union(pur);
                full = full.Union(sal);
                full = full.Union(joc);
                full = full.Union(StockAdjustment);
                full = full.AsQueryable().OrderBy("Date asc, entry asc");
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
                                     particulars = a.particulars,
                                     Remark = a.Remark
                                 }).ToList();
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
                vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
                vmodel.MainAccountID = AccId;
                vmodel.from = from;
                vmodel.to = to;
            }
            companySet();
            return View(vmodel);
        }
        #endregion

    }
}
