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
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.IO;

namespace QuickSoft.Controllers
{
    public class VendorRelationController : Controller
    {
        ApplicationDbContext db;
        Common com;
        public VendorRelationController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: VendorRelation
        public ActionResult Index()
        {
            return View();
        }

        // ---- Vendor Relation dashboard (styled like the main dashboard) ----
        public ActionResult Dashboard()
        {
            var today = DateTime.Now;
            var lastdate = today.AddMonths(-1);
            ViewBag.today = today.ToString("dd-MM-yyyy");
            ViewBag.lastdate = lastdate.ToString("dd-MM-yyyy");

            HomeViewModel vmodel = new HomeViewModel();
            vmodel.totSupplierCount = Convert.ToString(db.Suppliers.Count());
            vmodel.totPurchaseEntryCount = Convert.ToString(db.PurchaseEntrys.Where(a => a.Status == 1).Count());

            // 12-month trend: Purchases vs Payments
            try
            {
                var trendStart = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
                var pByM = db.PurchaseEntrys.Where(p => p.Status == 1 && p.PEDate >= trendStart)
                    .GroupBy(p => new { p.PEDate.Year, p.PEDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, T = g.Sum(x => (decimal?)x.PEGrandTotal) ?? 0 }).ToList();
                var payByM = db.Payments.Where(p => p.Voucher != 0 && p.Date >= trendStart)
                    .GroupBy(p => new { p.Date.Year, p.Date.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, T = g.Sum(x => (decimal?)x.GrandTotal) ?? 0 }).ToList();
                var tl = new List<string>(); var tp = new List<decimal>(); var tpay = new List<decimal>();
                for (int i = 0; i < 12; i++)
                {
                    var d = trendStart.AddMonths(i);
                    tl.Add(d.ToString("MMM"));
                    tp.Add(pByM.Where(x => x.Year == d.Year && x.Month == d.Month).Select(x => x.T).FirstOrDefault());
                    tpay.Add(payByM.Where(x => x.Year == d.Year && x.Month == d.Month).Select(x => x.T).FirstOrDefault());
                }
                ViewBag.trendLabels = Newtonsoft.Json.JsonConvert.SerializeObject(tl);
                ViewBag.trendSales = Newtonsoft.Json.JsonConvert.SerializeObject(tp);
                ViewBag.trendPurchase = Newtonsoft.Json.JsonConvert.SerializeObject(tpay);
            }
            catch { ViewBag.trendLabels = "[]"; ViewBag.trendSales = "[]"; ViewBag.trendPurchase = "[]"; }

            // Per-period KPIs: Purchases, Payments, Purchase Returns, New Suppliers (+ delta vs previous period)
            try
            {
                DateTime endEx = today.Date.AddDays(1);
                DateTime dToday = today.Date, dYest = dToday.AddDays(-1);
                int dow = ((int)today.DayOfWeek + 6) % 7;
                DateTime wkStart = dToday.AddDays(-dow), wkPrev = wkStart.AddDays(-7);
                DateTime moStart = new DateTime(today.Year, today.Month, 1), moPrev = moStart.AddMonths(-1);
                DateTime yrStart = new DateTime(today.Year, 1, 1), yrPrev = yrStart.AddYears(-1);

                Func<DateTime, DateTime, decimal[]> sums = (from, to) => new[]
                {
                    db.PurchaseEntrys.Where(x => x.Status == 1 && x.PEDate >= from && x.PEDate < to).Select(x => (decimal?)x.PEGrandTotal).Sum() ?? 0m,
                    db.Payments.Where(x => x.Voucher != 0 && x.Date >= from && x.Date < to).Select(x => (decimal?)x.GrandTotal).Sum() ?? 0m,
                    db.PurchaseReturns.Where(x => x.PRDate >= from && x.PRDate < to).Select(x => (decimal?)x.PRGrandTotal).Sum() ?? 0m,
                    (decimal)db.Suppliers.Count(x => x.AccountID.CreatedDate >= from && x.AccountID.CreatedDate < to)
                };
                Func<decimal, decimal, double?> dlt = (cur, prev) => prev == 0 ? (double?)null : (double)Math.Round((cur - prev) / prev * 100, 1);
                Func<string, DateTime, DateTime, DateTime, object> mk = (label, from, to, prevFrom) =>
                {
                    var c = sums(from, to); var p = sums(prevFrom, from);
                    return new
                    {
                        label,
                        purchases = c[0], payments = c[1], returns = c[2], suppliers = c[3],
                        dPurchases = dlt(c[0], p[0]), dPayments = dlt(c[1], p[1]), dReturns = dlt(c[2], p[2]), dSuppliers = dlt(c[3], p[3])
                    };
                };
                var periodData = new
                {
                    today = mk("today", dToday, endEx, dYest),
                    week = mk("week", wkStart, endEx, wkPrev),
                    month = mk("month", moStart, endEx, moPrev),
                    year = mk("year", yrStart, endEx, yrPrev)
                };
                ViewBag.periodJson = Newtonsoft.Json.JsonConvert.SerializeObject(periodData);
            }
            catch { ViewBag.periodJson = "null"; }

            ViewBag.Active = "Dashboard";
            ViewBag.Title = "Vendor Relation";
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View(vmodel);
        }

    }
}
