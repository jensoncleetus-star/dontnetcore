using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class PropertyTransactionsController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PropertyTransactionsController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/PropertyTransactions
        public ActionResult Index()
        {
            var proj = db.PropertyMains
              .Select(s => new
              {
                  ID = s.Id,
                  Name = s.Code + " " + s.Name
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.PropertyUnits
             .Select(s => new
             {
                 ID = s.Id,
                 Name = s.Name
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");
            return View();
        }

        
public ActionResult GetPropertyTransactions(long Property,long? Unit, string fromdate, string todate)
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
            TransactionPropertySummaryViewModel vmodel = new TransactionPropertySummaryViewModel();

            vmodel.from = from;
            vmodel.to = to;
            var pro = db.PropertyMains.Find(Property);
            vmodel.property = pro.Name;

           
            var Account = (from a in db.Accountss
                           select new
                           {
                               a.AccountsID,
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).ToList();
            //string AccType = com.AccType(AccId);
            //var Balance = com.AccOpenBlnc(AccId, fdate, AccType, pdcinclude);
            //long[] acclist = db.Accountss.Select(a => a.AccountsID).ToArray();

            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Project==Property) &&
                           (Unit == null || a.ProTask==Unit) &&
                           (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Receipt"
                           select new
                           {
                               id = c.ReceiptId,
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
            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Project == Property) &&
                           (Unit == null || a.ProTask == Unit) &&
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
                           (a.Project == Property) &&
                           (Unit == null || a.ProTask == Unit) &&
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
                          (a.Project == Property) &&
                           (Unit == null || a.ProTask == Unit) &&
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
            //var Drnote = (from a in db.AccountsTransactions
            //              join b in db.Accountss on a.Account equals b.AccountsID
            //              join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
            //              where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
            //              (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Dr. Note"
            //              select new
            //              {
            //                  id = c.ContraVoucherId,
            //                  RAccountID = b.AccountsID,
            //                  Debit = (decimal?)a.Debit,
            //                  Credit = (decimal?)a.Credit,
            //              }).GroupBy(x => new { x.RAccountID }, (y, group) => new Transaction
            //              {
            //                  Count = group.Select(k => k.id).Count(),
            //                  RAccount = y.RAccountID,
            //                  Debit = (decimal)group.Sum(k => k.Debit),
            //                  Credit = (decimal)group.Sum(k => k.Credit),
            //              }).ToList();
            //var Crnote = (from a in db.AccountsTransactions
            //              join b in db.Accountss on a.Account equals b.AccountsID
            //              join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
            //              where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
            //              (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Cr. Note"
            //              select new Transaction
            //              {
            //                  id = c.ContraVoucherId,
            //                  RAccountID = b.AccountsID,
            //                  Debit = (decimal?)a.Debit,
            //                  Credit = (decimal?)a.Credit,
            //              }).GroupBy(x => new { x.RAccountID }, (y, group) => new
            //              {
            //                  Count = group.Select(k => k.id).Count(),
            //                  RAccount = y.RAccountID,
            //                  Debit = (decimal)group.Sum(k => k.Debit),
            //                  Credit = (decimal)group.Sum(k => k.Credit),
            //              }).ToList();
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
                                 //join j in Drnote on a.AccountsID equals j.RAccount into Drn
                                 //from j in Drn.DefaultIfEmpty()
                                 //join k in Crnote on a.AccountsID equals k.RAccount into Crn
                                 //from k in Crn.DefaultIfEmpty()
                             select new TransactionSummary
                             {
                                 id = a.AccountsID,
                                 Name = a.Name,
                                 Opening = com.AccOpenBlnc(a.AccountsID, fdate, "", true),
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
                             }).Where(o => o.ConCr != 0 || o.ConDr != 0 || o.CrnoteCr != 0 || o.CrnoteDr != 0 || o.DrnoteCr != 0 || o.DrnoteDr != 0 ||
                             o.JourCr != 0 || o.JourDr != 0 || o.PayCr != 0 || o.PayDr != 0 || o.PRetCr != 0 || o.PRetDr != 0 || o.PurCr != 0 ||
                             o.PurDr != 0 || o.RecCr != 0 || o.RecDr != 0 || o.SaleCr != 0 || o.SaleDr != 0 || o.SRetCr != 0 || o.SRetDr != 0).ToList();
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