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
        public ActionResult Dashboard()
        {
            var today = DateTime.Now;
            var CurrentYear = DateTime.Now.Year;
            var Currentmonth = DateTime.Now.Month;
            //every last 6 months first day
            var ThisMnth1stDay = new DateTime(CurrentYear, Currentmonth, 1);
            var ThisYear1stDay = new DateTime(CurrentYear, 1, 1);
            var LastMnth1stDay = ThisMnth1stDay.AddMonths(-1);
            var Last2ndMnth1stDay = ThisMnth1stDay.AddMonths(-2);
            var last3rdMnth1stDay = ThisMnth1stDay.AddMonths(-3);
            var Last4thMnth1stDay = ThisMnth1stDay.AddMonths(-4);
            var Last5thMnth1stDay = ThisMnth1stDay.AddMonths(-5);
            var Last6thMnth1stDay = ThisMnth1stDay.AddMonths(-6);
            var Last7thMnth1stDay = ThisMnth1stDay.AddMonths(-7);
            var Last8thMnth1stDay = ThisMnth1stDay.AddMonths(-8);
            var Last9thMnth1stDay = ThisMnth1stDay.AddMonths(-9);
            var Last10thMnth1stDay = ThisMnth1stDay.AddMonths(-10);
            var Last11thMnth1stDay = ThisMnth1stDay.AddMonths(-11);
            //every last 6 months Last day
            var LastMnthLastDay = ThisMnth1stDay.AddDays(-1);
            var Last2ndMnthLastDay = LastMnth1stDay.AddDays(-1);
            var last3rdMnthLastDay = Last2ndMnth1stDay.AddDays(-1);
            var Last4thMnthLastDay = last3rdMnth1stDay.AddDays(-1);
            var Last5thMnthLastDay = Last4thMnth1stDay.AddDays(-1);
            var Last6thMnthLastDay = Last5thMnth1stDay.AddDays(-1);
            var Last7thMnthLastDay = Last6thMnth1stDay.AddDays(-1);
            var Last8thMnthLastDay = Last7thMnth1stDay.AddDays(-1);
            var Last9thMnthLastDay = Last8thMnth1stDay.AddDays(-1);
            var Last10thMnthLastDay = Last9thMnth1stDay.AddDays(-1);
            var Last11thMnthLastDay = Last10thMnth1stDay.AddDays(-1);
            //converting month to string(eg:'1=jan','2=feb'....)
            ViewBag.mnth12 = Last11thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth11 = Last10thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth10 = Last9thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth9 = Last8thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth8 = Last7thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth7= Last6thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth6 = Last5thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth5 = Last4thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth4 = last3rdMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth3 = Last2ndMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth2 = LastMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth1 = today.ToString("MMM", CultureInfo.InvariantCulture);

            VendorRelationViewModel vmodel = new VendorRelationViewModel();
            vmodel.totSupplierCount= Convert.ToString(db.Suppliers.Count());
            vmodel.TodayPurchase = Convert.ToString(db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, today) <= 0 && EF.Functions.DateDiffDay(b.PEDate, today) >= 0)).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.ThisMonthPurchase= Convert.ToString(db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, today) >= 0)).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.ThisYearPurchase = Convert.ToString(db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, today) >= 0)).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastMonthPurchase = Convert.ToString(db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, LastMnthLastDay) >= 0)).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastTwoMonthPurchase = Convert.ToString(db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, Last2ndMnthLastDay) >= 0)).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastThreeMonthPurchase = Convert.ToString(db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, last3rdMnthLastDay) >= 0)).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastFourMonthPurchase = Convert.ToString(db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, Last4thMnthLastDay) >= 0)).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastFiveMonthPurchase = Convert.ToString(db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, Last5thMnthLastDay) >= 0)).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
           
            vmodel.ThisMonthPurchaseReturn= Convert.ToString(db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, today) >= 0)).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastMonthPurchaseReturn = Convert.ToString(db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, LastMnthLastDay) >= 0)).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastTwoMonthPurchaseReturn = Convert.ToString(db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, Last2ndMnthLastDay) >= 0)).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastThreeMonthPurchaseReturn = Convert.ToString(db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, last3rdMnthLastDay) >= 0)).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastFourMonthPurchaseReturn = Convert.ToString(db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, Last4thMnthLastDay) >= 0)).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastFiveMonthPurchaseReturn = Convert.ToString(db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, Last5thMnthLastDay) >= 0)).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());

            return View(vmodel);
        }

    }
}