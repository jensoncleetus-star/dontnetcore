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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic;
using System.Net;
using System.Collections;

namespace QuickSoft.Controllers
{
    public class BRSController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public BRSController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: BankReconciliation
        public ActionResult Index()
        {
            var PaidTo = db.Accountss.Where((p => p.Group == 8)).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.AccName = QkSelect.List(PaidTo, "ID", "Name");
            ViewBag.ItemType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All Bank Entries", Value="1"},
                new SelectListItem() {Text = "Cleared Bank Entries", Value="2"},
                new SelectListItem() {Text = "Uncleared Bank Entries", Value="3"}
            }, "Value", "Text");
            _FinancialYear();
            return View();
        }

        public ActionResult ShowBRS()
        {
            var PaidTo = db.Accountss.Where((p => p.Group == 8)).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.AccName = QkSelect.List(PaidTo, "ID", "Name");
            return View();
        }

        public ActionResult StatementView()
        {
            var PaidTo = db.Accountss.Where((p => p.Group == 8)).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.AccName = QkSelect.List(PaidTo, "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult GetBRS(long? Account, string FromDate, string ToDate, int ShowType)
        {
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

            IEnumerable<BRSViewModel> full = new List<BRSViewModel>();
            var ContraVFrom = (from b in db.ContraVouchers
                               join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                               from c in payfrom.DefaultIfEmpty()
                               where (b.PayTo == Account)
                               && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0)
                               && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
                               select new BRSViewModel
                               {
                                   Id = b.ContraVoucherId,
                                   Date = b.Date,
                                   Type = "ContraVoucher",
                                   vchno = b.VoucherNo,
                                   AcName = c.Name,
                                   Deposits = null,
                                   Withdrawal = b.Amount,
                                   ClearDate = null,
                                   BankAmount = 0,
                                   Narration = b.Remark,
                                   CheType = "CASH",
                                   PDCDate = null,
                               }).ToList();
            var ContraVTo = (from b in db.ContraVouchers
                             join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                             from c in payfrom.DefaultIfEmpty()
                             where (b.PayFrom == Account)
                             && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0)
                             && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
                             select new BRSViewModel
                             {
                                 Id = b.ContraVoucherId,
                                 Date = b.Date,
                                 Type = "ContraVoucher",
                                 vchno = b.VoucherNo,
                                 AcName = c.Name,
                                 Deposits = null,
                                 Withdrawal = b.Amount,
                                 ClearDate = null,
                                 BankAmount = 0,
                                 Narration = b.Remark,
                                 CheType = "CASH",
                                 PDCDate = null,
                             }).ToList();           
            if (ShowType == 1)
            {
                var Paymnt = (from b in db.Payments
                              join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                              from c in payfrom.DefaultIfEmpty()
                              join d in db.PDCs on new { f1 = b.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                              from d in pay.DefaultIfEmpty()
                              where (b.PayFrom == Account)
                              && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                              && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                              select new BRSViewModel
                              {
                                  Id = b.PaymentId,
                                  Date = b.Date,
                                  Type = "Payment",
                                  vchno = b.VoucherNo,
                                  AcName = c.Name,
                                  Deposits = b.Paying,
                                  Withdrawal = null,
                                  ClearDate = (DateTime?)d.ClearDate,
                                  BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                                  Narration = b.Remark,
                                  CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                  PDCDate = b.PDCDate,
                              }).ToList();
                var Recpt = (from b in db.Receipts
                             join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                             from c in payfrom.DefaultIfEmpty()
                             join d in db.PDCs on new { f1 = b.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                             from d in pay.DefaultIfEmpty()
                             where (b.PayTo == Account)
                             && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                             && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                             select new BRSViewModel
                             {
                                 Id = b.ReceiptId,
                                 Date = b.Date,
                                 Type = "Receipt",
                                 vchno = b.VoucherNo,
                                 AcName = c.Name,
                                 Deposits = null,
                                 Withdrawal = b.Paying,
                                 ClearDate = (DateTime?)d.ClearDate,
                                 BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                                 Narration = b.Remark,
                                 CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                 PDCDate = b.PDCDate,
                             }).ToList();
                var JournalFrom = (from b in db.Journals
                                   join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                                   from c in payfrom.DefaultIfEmpty()
                                   join d in db.PDCs on new { f1 = b.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                                   from d in pay.DefaultIfEmpty()
                                   join e in db.AccountsTransactions on new { g1 = b.JournalId, g2 = "Journal" } equals new { g1 = e.reference, g2 = e.Purpose } into trans
                                   from e in trans.DefaultIfEmpty()
                                   where (e.Account == Account)
                                   && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                                   && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                                  && e.Debit>0
                                   select new BRSViewModel
                                   {
                                       Id = b.JournalId,
                                       Date = b.Date,
                                       Type = "Journal",
                                       vchno = b.VoucherNo,
                                       AcName = c.Name,
                                       Deposits = null,
                                       Withdrawal = e.Debit,
                                       ClearDate = (DateTime?)d.ClearDate,
                                       BankAmount = 0,
                                       Narration = b.Remark,
                                       CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                       PDCDate = b.PDCDate,
                                   }).ToList();
                var JournalTo = (from b in db.Journals
                                 join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                                 from c in payfrom.DefaultIfEmpty()
                                 join d in db.PDCs on new { f1 = b.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                                 from d in pay.DefaultIfEmpty()
                                 join e in db.AccountsTransactions on new { g1 = b.JournalId, g2 = "Journal" } equals new { g1 = e.reference, g2 = e.Purpose } into trans
                                 from e in trans.DefaultIfEmpty()
                                 where (e.Account == Account)
                                 && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                                 && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                                && e.Credit>0
                                 select new BRSViewModel
                                 {
                                     Id = b.JournalId,
                                     Date = b.Date,
                                     Type = "Journal",
                                     vchno = b.VoucherNo,
                                     AcName = c.Name,
                                     Deposits = e.Credit,
                                     Withdrawal = null,
                                     ClearDate = (DateTime?)d.ClearDate,
                                     BankAmount = 0,
                                     Narration = b.Remark,
                                     CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                     PDCDate = b.PDCDate,
                                 }).ToList();
                full = Paymnt.Union(Recpt);
                full = full.Union(ContraVFrom);
                full = full.Union(ContraVTo);
                full = full.Union(JournalFrom);
                full = full.Union(JournalTo);
            }
            else if (ShowType == 2)
            {
                var Paymnt = (from b in db.Payments
                              join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                              from c in payfrom.DefaultIfEmpty()
                              join d in db.PDCs on new { f1 = b.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                              from d in pay.DefaultIfEmpty()
                              where (b.PayFrom == Account)
                              && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                              && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                              && (b.MOPayment == 0 || d.ClearDate != null)
                              select new BRSViewModel
                              {
                                  Id = b.PaymentId,
                                  Date = b.Date,
                                  Type = "Payment",
                                  vchno = b.VoucherNo,
                                  AcName = c.Name,
                                  Deposits = b.Paying,
                                  Withdrawal = null,
                                  ClearDate = d.ClearDate,
                                  BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                                  Narration = b.Remark,
                                  CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                  PDCDate = b.PDCDate,
                              });
                var Recpt = (from b in db.Receipts
                             join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                             from c in payfrom.DefaultIfEmpty()
                             join d in db.PDCs on new { f1 = b.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                             from d in pay.DefaultIfEmpty()
                             where (b.PayTo == Account)
                             && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                             && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                             && (b.MOPayment == 0 || d.ClearDate != null)
                             select new BRSViewModel
                             {
                                 Id = b.ReceiptId,
                                 Date = b.Date,
                                 Type = "Reciept",
                                 vchno = b.VoucherNo,
                                 AcName = c.Name,
                                 Deposits = null,
                                 Withdrawal = b.Paying,
                                 ClearDate = d.ClearDate,
                                 BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                                 Narration = b.Remark,
                                 CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                 PDCDate = b.PDCDate,
                             });
                var JournalFrom = (from b in db.Journals
                                   join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                                   from c in payfrom.DefaultIfEmpty()
                                   join d in db.PDCs on new { f1 = b.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                                   from d in pay.DefaultIfEmpty()
                                   join e in db.AccountsTransactions on new { g1 = b.JournalId, g2 = "Journal", g3 = b.PayTo } equals new { g1 = e.reference, g2 = e.Purpose, g3 = e.Account } into trans
                                   from e in trans.DefaultIfEmpty()
                                   where (b.PayTo == Account)
                                   && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                                   && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                                   && (b.MOPayment == 0 || d.ClearDate != null)
                                   select new BRSViewModel
                                   {
                                       Id = b.JournalId,
                                       Date = b.Date,
                                       Type = "Journal",
                                       vchno = b.VoucherNo,
                                       AcName = c.Name,
                                       Deposits = null,
                                       Withdrawal = e.Debit,
                                       ClearDate = (DateTime?)d.ClearDate,
                                       BankAmount = 0,
                                       Narration = b.Remark,
                                       CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                       PDCDate = b.PDCDate,
                                   }).ToList();
                var JournalTo = (from b in db.Journals
                                 join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                                 from c in payfrom.DefaultIfEmpty()
                                 join d in db.PDCs on new { f1 = b.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                                 from d in pay.DefaultIfEmpty()
                                 where (b.PayFrom == Account)
                                 && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                                 && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                                 && (b.MOPayment == 0 || d.ClearDate != null)
                                 select new BRSViewModel
                                 {
                                     Id = b.JournalId,
                                     Date = b.Date,
                                     Type = "Journal",
                                     vchno = b.VoucherNo,
                                     AcName = c.Name,
                                     Deposits = b.Paying,
                                     Withdrawal = null,
                                     ClearDate = (DateTime?)d.ClearDate,
                                     BankAmount = 0,
                                     Narration = b.Remark,
                                     CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                     PDCDate = b.PDCDate,
                                 }).ToList();
                full = Paymnt.Union(Recpt);
                full = full.Union(ContraVFrom);
                full = full.Union(ContraVTo);
                full = full.Union(JournalFrom);
                full = full.Union(JournalTo);
            }
            else if (ShowType == 3)
            {
                var Paymnt = (from b in db.Payments
                              join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                              from c in payfrom.DefaultIfEmpty()
                              join d in db.PDCs on new { f1 = b.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                              from d in pay.DefaultIfEmpty()
                              where (b.PayFrom == Account)
                              && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                              && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                              && d.ClearDate == null  && b.MOPayment!=0
                              select new BRSViewModel
                              {
                                  Id = b.PaymentId,
                                  Date = b.Date,
                                  Type = "Payment",
                                  vchno = b.VoucherNo,
                                  AcName = c.Name,
                                  Deposits = b.Paying,
                                  Withdrawal = null,
                                  ClearDate = d.ClearDate,
                                  BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                                  Narration = b.Remark,
                                  CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                  PDCDate = b.PDCDate,
                              });
                var Recpt = (from b in db.Receipts
                             join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                             from c in payfrom.DefaultIfEmpty()
                             join d in db.PDCs on new { f1 = b.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                             from d in pay.DefaultIfEmpty()
                             where (b.PayTo == Account)
                             && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                             && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                             && d.ClearDate == null && b.MOPayment != 0
                             select new BRSViewModel
                             {
                                 Id = b.ReceiptId,
                                 Date = b.Date,
                                 Type = "Reciept",
                                 vchno = b.VoucherNo,
                                 AcName = c.Name,
                                 Deposits = null,
                                 Withdrawal = b.Paying,
                                 ClearDate = d.ClearDate,
                                 BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                                 Narration = b.Remark,
                                 CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                 PDCDate = b.PDCDate,
                             });
                var JournalFrom = (from b in db.Journals
                                   join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                                   from c in payfrom.DefaultIfEmpty()
                                   join d in db.PDCs on new { f1 = b.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                                   from d in pay.DefaultIfEmpty()
                                   join e in db.AccountsTransactions on new { g1 = b.JournalId, g2 = "Journal", g3 = b.PayTo } equals new { g1 = e.reference, g2 = e.Purpose, g3 = e.Account } into trans
                                   from e in trans.DefaultIfEmpty()
                                   where (b.PayTo == Account)
                                   && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                                   && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                                   && d.ClearDate == null && b.MOPayment != 0
                                   select new BRSViewModel
                                   {
                                       Id = b.JournalId,
                                       Date = b.Date,
                                       Type = "Journal",
                                       vchno = b.VoucherNo,
                                       AcName = c.Name,
                                       Deposits = null,
                                       Withdrawal = e.Debit,
                                       ClearDate = d.ClearDate,
                                       BankAmount = (d.ClearDate != null) ? e.Debit : 0,
                                       Narration = b.Remark,
                                       CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                       PDCDate = b.PDCDate,
                                   }).ToList();
                var JournalTo = (from b in db.Journals
                                 join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                                 from c in payfrom.DefaultIfEmpty()
                                 join d in db.PDCs on new { f1 = b.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                                 from d in pay.DefaultIfEmpty()
                                 where (b.PayFrom == Account)
                                 && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                                 && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                                 && d.ClearDate == null && b.MOPayment != 0
                                 select new BRSViewModel
                                 {
                                     Id = b.JournalId,
                                     Date = b.Date,
                                     Type = "Journal",
                                     vchno = b.VoucherNo,
                                     AcName = c.Name,
                                     Deposits = b.Paying,
                                     Withdrawal = null,
                                     ClearDate = d.ClearDate,
                                     BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                                     Narration = b.Remark,
                                     CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                     PDCDate = b.PDCDate,
                                 }).ToList();
                full = Paymnt.Union(Recpt);
                full = full.Union(ContraVFrom);
                full = full.Union(ContraVTo);
                full = full.Union(JournalFrom);
                full = full.Union(JournalTo);
            }

            var rslt = tablefooter(Account, FromDate, ToDate, full);

            BRSFinalViewModel BRS = new BRSFinalViewModel();
            BRS.MainAccount = db.Accountss.Where(x => x.AccountsID == Account).Select(x => x.Name).FirstOrDefault();
            BRS.from = fdate;
            BRS.to = tdate;
            BRS.OpeningBalance = Convert.ToDecimal(rslt[5]);
            BRS.LedgerAmount= Convert.ToDecimal(rslt[2]);
            BRS.Debit= Convert.ToDecimal(rslt[0]);
            BRS.Credit = Convert.ToDecimal(rslt[1]);
            BRS.BRS = full.OrderBy(x=>x.Date);
            BRS.footer = rslt[3].ToString();
            BRS.Balance = Convert.ToDecimal(rslt[4]);
            BRS.ShowType = ShowType;
            BRS.Account = (long)Account;
            return View(BRS);
        }
        
        public ArrayList tablefooter(long? Account,string FromDate, string ToDate,IEnumerable<BRSViewModel> full)
        {
            LedgerProViewModel reslt = com.LedgerData((long)Account, FromDate, ToDate, 8, null);
            decimal? bal = reslt.blnceType == "Dr." ? (0- reslt.OpeningBalance) : reslt.OpeningBalance;
            decimal? OpBal = bal;
            decimal? dr = 0;
            decimal? cr = 0;
            decimal? cleardr = 0;
            decimal? clearcr = 0;
            decimal? otherdr = 0;
            decimal? othercr = 0;
            foreach (var arr in full)
            {
                if(arr.ClearDate==null && arr.CheType !="CASH")
                {
                     var newarr=(arr.Withdrawal != null) ? (dr = dr + arr.Withdrawal) : (cr = cr + arr.Deposits);
                }
                else if(arr.CheType == "CASH" )
                {
                    var newarr = (arr.Withdrawal != null) ? (otherdr = otherdr + arr.Withdrawal) : (othercr = othercr + arr.Deposits);
                }
                else
                {
                    var newarr = (arr.Withdrawal != null) ? (cleardr = cleardr + arr.Withdrawal) : (clearcr = clearcr + arr.Deposits);
                }
            }
            bal = (bal + cleardr+otherdr) - clearcr-othercr;
            decimal? bankbal = (bal - dr) + cr;
            var arrlst = new ArrayList();
            arrlst.Add(dr);
            arrlst.Add(cr);
            arrlst.Add(bankbal);//bank amount
            string Less = "Balance as per company books" + bankbal + "\n Amounts not reflected in bank" + dr + "&emsp;" + cr + "\n Balance as per Bank:" + bal;
            arrlst.Add(Less);
            arrlst.Add(bal);//ledger amount
            arrlst.Add(OpBal);//Op bal
            return arrlst;
        }

        //        serialisedJson = db.Accountss.Where(p => p.Name.Contains(q))
        //                          .Select(b => new SelectFormat
        //                              text = b.Name, //each json object will have 
        //                              id = b.AccountsID
        //                          })
        //        serialisedJson = db.Accountss.Select(b => new SelectFormat
        //            text = b.Name, //each json object will have 
        //            id = b.AccountsID

        //    }//


        [HttpGet]
        public ActionResult Edit()
        {
            long Id=Convert.ToInt64(Request.Query["id"]);
            string Type = Request.Query["Type"];
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if(Type== "Receipt")
            {
                Receipt rcpt = db.Receipts.Find(Id);
            }
            else if (Type == "Payment")
            {
                Payment pymt = db.Payments.Find(Id);
            }
            else if (Type == "Journal")
            {
                Journal jou = db.Journals.Find(Id);
            }
            else
            {
                return NotFound();
            }


            BRSViewModel vmodel = new BRSViewModel();
            if (Type == "Receipt")
            {

                vmodel = (from b in db.Receipts
                          join c in db.Accountss on b.PayFrom equals c.AccountsID
                          join d in db.PDCs on b.ReceiptId equals d.Reference
                          where b.ReceiptId == Id
                          select new BRSViewModel
                          {
                              Id = Id,
                              Date = b.Date,
                              Type = "Receipt",
                              vchno = b.VoucherNo,
                              AcName = c.Name,
                              Deposits = b.Paying,
                              Withdrawal = 0,
                              ClearDate = d.ClearDate,
                              BankAmount = b.Paying,
                              Narration = ""
                          }).FirstOrDefault();
            }
            else if (Type == "Payment")
            {
                vmodel = (from b in db.Payments
                          join c in db.Accountss on b.PayFrom equals c.AccountsID
                          join d in db.PDCs on b.PaymentId equals d.Reference
                          where b.PaymentId == Id
                          select new BRSViewModel
                          {
                              Id = Id,
                              Date = b.Date,
                              Type = "Payment",
                              vchno = b.VoucherNo,
                              AcName = c.Name,
                              Deposits = b.Paying,
                              Withdrawal = 0,
                              ClearDate = d.ClearDate,
                              BankAmount = b.Paying,
                              Narration = ""
                          }).FirstOrDefault();
            }
            else if (Type == "Journal")
            {
                vmodel = (from b in db.Journals
                          join c in db.Accountss on b.PayFrom equals c.AccountsID
                          join d in db.PDCs on b.JournalId equals d.Reference
                          where b.JournalId == Id
                          select new BRSViewModel
                          {
                              Id = Id,
                              Date = b.Date,
                              Type = "Journal",
                              vchno = b.VoucherNo,
                              AcName = c.Name,
                              Deposits = b.Paying,
                              Withdrawal = 0,
                              ClearDate = d.ClearDate,
                              BankAmount = b.Paying,
                              Narration = ""
                          }).FirstOrDefault();
            }
            companySet();

            return PartialView(vmodel);
        }
        [HttpPost]
        public JsonResult UpdateBRS(BRSViewModel BRS)//(string Cleardate, long Id, string Type)
        {
            bool stat = false;
            string msg = "";
            var date = BRS.ClearDate;
            var Id = BRS.Id;
            var Type = BRS.Type;
            var ClrDate = db.PDCs.Where(x => x.Reference == Id && x.PDCType==Type).FirstOrDefault();
            if (ClrDate != null)
            {
                ClrDate.ClearDate = date;
                db.SaveChanges();
                stat = true;
                msg = "Success";
            }
            else
            {
                stat = false;
                msg = "Updation Failed";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        public ActionResult GetStatement(long Account, string FromDate, string ToDate)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            BRSStatement vmodel = new BRSStatement();
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
            ////Sum of UnCleared Entries
           
            IEnumerable<BRSViewModel> full = new List<BRSViewModel>();
            var ContraVFrom = (from b in db.ContraVouchers
                               join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                               from c in payfrom.DefaultIfEmpty()
                               where (b.PayTo == Account)
                               && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0)
                               && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
                               select new BRSViewModel
                               {
                                   Id = b.ContraVoucherId,
                                   Date = b.Date,
                                   Type = "ContraVoucher",
                                   vchno = b.VoucherNo,
                                   AcName = c.Name,
                                   Deposits = null,
                                   Withdrawal = b.Amount,
                                   ClearDate = null,
                                   BankAmount = 0,
                                   Narration = b.Remark,
                                   CheType = "CASH",
                                   PDCDate = null,
                               }).ToList();
            var ContraVTo = (from b in db.ContraVouchers
                             join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                             from c in payfrom.DefaultIfEmpty()
                             where (b.PayFrom == Account)
                             && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0)
                             && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
                             select new BRSViewModel
                             {
                                 Id = b.ContraVoucherId,
                                 Date = b.Date,
                                 Type = "ContraVoucher",
                                 vchno = b.VoucherNo,
                                 AcName = c.Name,
                                 Deposits = null,
                                 Withdrawal = b.Amount,
                                 ClearDate = null,
                                 BankAmount = 0,
                                 Narration = b.Remark,
                                 CheType = "CASH",
                                 PDCDate = null,
                             }).ToList();

            var Paymnt = (from b in db.Payments
                          join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                          from c in payfrom.DefaultIfEmpty()
                          join d in db.PDCs on new { f1 = b.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                          from d in pay.DefaultIfEmpty()
                          where (b.PayFrom == Account)
                          && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                          && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                          && d.ClearDate == null && b.MOPayment != 0
                          select new BRSViewModel
                          {
                              Id = b.PaymentId,
                              Date = b.Date,
                              Type = "Payment",
                              vchno = b.VoucherNo,
                              AcName = c.Name,
                              Deposits = b.Paying,
                              Withdrawal = null,
                              ClearDate = d.ClearDate,
                              BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                              Narration = b.Remark,
                              CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                              PDCDate = b.PDCDate,
                          });
            var Recpt = (from b in db.Receipts
                         join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                         from c in payfrom.DefaultIfEmpty()
                         join d in db.PDCs on new { f1 = b.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                         from d in pay.DefaultIfEmpty()
                         where (b.PayTo == Account)
                         && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                         && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                         && d.ClearDate == null && b.MOPayment != 0
                         select new BRSViewModel
                         {
                             Id = b.ReceiptId,
                             Date = b.Date,
                             Type = "Reciept",
                             vchno = b.VoucherNo,
                             AcName = c.Name,
                             Deposits = null,
                             Withdrawal = b.Paying,
                             ClearDate = d.ClearDate,
                             BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                             Narration = b.Remark,
                             CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                             PDCDate = b.PDCDate,
                         });
            var JournalFrom = (from b in db.Journals
                               join c in db.Accountss on b.PayFrom equals c.AccountsID into payfrom
                               from c in payfrom.DefaultIfEmpty()
                               join d in db.PDCs on new { f1 = b.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                               from d in pay.DefaultIfEmpty()
                               join e in db.AccountsTransactions on new { g1 = b.JournalId, g2 = "Journal", g3 = b.PayTo } equals new { g1 = e.reference, g2 = e.Purpose, g3 = e.Account } into trans
                               from e in trans.DefaultIfEmpty()
                               where (b.PayTo == Account)
                               && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                               && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                               && d.ClearDate == null && b.MOPayment != 0
                               select new BRSViewModel
                               {
                                   Id = b.JournalId,
                                   Date = b.Date,
                                   Type = "Journal",
                                   vchno = b.VoucherNo,
                                   AcName = c.Name,
                                   Deposits = null,
                                   Withdrawal = e.Debit,
                                   ClearDate = d.ClearDate,
                                   BankAmount = (d.ClearDate != null) ? e.Debit : 0,
                                   Narration = b.Remark,
                                   CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                   PDCDate = b.PDCDate,
                               }).ToList();
            var JournalTo = (from b in db.Journals
                             join c in db.Accountss on b.PayTo equals c.AccountsID into payfrom
                             from c in payfrom.DefaultIfEmpty()
                             join d in db.PDCs on new { f1 = b.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pay
                             from d in pay.DefaultIfEmpty()
                             where (b.PayFrom == Account)
                             && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, fdate) <= 0)
                             && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay((b.MOPayment == ModeOfPayment.PDC || b.MOPayment == ModeOfPayment.CDC) ? d.PDCDate : b.Date, tdate) >= 0)
                             && d.ClearDate == null && b.MOPayment != 0
                             select new BRSViewModel
                             {
                                 Id = b.JournalId,
                                 Date = b.Date,
                                 Type = "Journal",
                                 vchno = b.VoucherNo,
                                 AcName = c.Name,
                                 Deposits = b.Paying,
                                 Withdrawal = null,
                                 ClearDate = d.ClearDate,
                                 BankAmount = (d.ClearDate != null) ? b.Paying : 0,
                                 Narration = b.Remark,
                                 CheType = (b.MOPayment == ModeOfPayment.PDC) ? "PDC" : (b.MOPayment == ModeOfPayment.CDC) ? "CDC" : "CASH",
                                 PDCDate = b.PDCDate,
                             }).ToList();
            full = Paymnt.Union(Recpt);
            full = full.Union(ContraVFrom);
            full = full.Union(ContraVTo);
            full = full.Union(JournalFrom);
            full = full.Union(JournalTo);

            decimal? debtsum = full.Select(x => x.Withdrawal).Sum();
            decimal? crdtsum = full.Select(x => x.Deposits).Sum();

                     
            var DicBalance = com.Accbalance(Account,ToDate);
            decimal balance = (decimal)DicBalance["amount"];


            decimal BankBalance = balance - (decimal)(debtsum) + (decimal)(crdtsum);

            string LedgerString = "Bank Balance as per ledger as on'" + ToDate + "'";
            string Less = "Less:\n Cheques deposited from'" + FromDate + "' to '" + ToDate + "' but not cleared";
            string Add = "Add:\n Cheques issued from'" + FromDate + "' to '" + ToDate + "' but not cleared";
            string ActualBalance = "Actual Bank Balance as on '" + ToDate + "'";

            IList<BRSStatement> StatmntShow = new List<BRSStatement>() {
                new BRSStatement(){ Narration=LedgerString, Amount=balance},
                new BRSStatement(){ Narration=Less, Amount=(decimal)debtsum},
                new BRSStatement(){ Narration=Add, Amount=(decimal)crdtsum},
                new BRSStatement(){ Narration=ActualBalance, Amount=BankBalance}
            };

            recordsTotal = StatmntShow.Count();
            var data = StatmntShow.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

    }
}
