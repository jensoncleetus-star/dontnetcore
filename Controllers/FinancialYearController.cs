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
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    public class FinancialYearController : BaseController
    {
        ApplicationDbContext db;
        Common com;

        public FinancialYearController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: FinancialYear
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            var SetYear = db.FinancialYears
                          .Select(s => new
                          {
                              ID = s.id,
                              Name = s.Start + "-" + s.End,
                          })
                          .ToList();
            ViewBag.FinYear = QkSelect.List(SetYear, "ID", "Name");
            ViewBag.StartDate = db.FinancialYears.Select(i => i.Start).FirstOrDefault();
            return View();
        }
        public ActionResult AddFinDate()
        {
            return PartialView();
        }

        [HttpPost]
        public JsonResult SetNewFinancialYear()
        {
            bool stat = true;
            string msg="";
            var oldyear = db.FinancialYears.Where(x => x.Status == Status.active).FirstOrDefault();
            oldyear.Status = Status.inactive;
            db.Entry(oldyear).State = EntityState.Modified;
            db.SaveChanges();
            var MaxDate = db.FinancialYears.Max(i => i.End);
            if (MaxDate != null && MaxDate < DateTime.Now)
            {
                var NewDate = MaxDate.Value.Date;
                var CompId = db.FinancialYears.Where(y => y.End == MaxDate).Select(x => x).FirstOrDefault();
                FinancialYear FY = new FinancialYear();
                FY.Company = CompId.Company;
                FY.Start = MaxDate.Value.AddDays(1);
                FY.End = FY.Start.Value.AddYears(1).AddDays(-1);
                FY.Active = choice.Yes;
                FY.Status = Status.active;
                db.FinancialYears.Add(FY);
                db.SaveChanges();
                Int64 FinID = FY.id;
                FinancialYearChange(MaxDate.Value.AddDays(1).ToString(), FinID);
                msg = "Reset Financial Year Successfully.";
            }
            else if(MaxDate > DateTime.Now)
            {
                stat = false;
                msg = "Previous year not Completed";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost]
        public JsonResult TempFinancialYear(long? TempYear)
        {
            bool stat = true;
            string msg = "";
            HttpCookie Newcookie=null ;
            if (TempYear != null)
            {
                string cookievalue;
                HttpCookie Datecookie = new HttpCookie("FinYearID", TempYear.ToString());
                Datecookie.Expires.AddDays(0);
                HttpContext.Response.SetCookie(Datecookie);
                Newcookie = Request.Cookies["FinYearID"];
                msg = "Financial Year Successfully Changed.";
                stat = true;
            }
            else
            {

                msg = "Select a Valid Financial Year.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg} };
        }
        public JsonResult AddFinYear(string SetDate)
        {
            FinancialYear FY = new FinancialYear();
            FY.Company = db.companys.Select(x => x.CompanyID).First();
            FY.Start = Convert.ToDateTime(SetDate);
            FY.End = FY.Start.Value.AddYears(1).AddDays(-1);
            FY.Active = choice.Yes;
            FY.Status = Status.active;
            db.FinancialYears.Add(FY);
            db.SaveChanges();
            Int64 FinID = FY.id;
            var MaxDate= Convert.ToDateTime(SetDate);
            FinancialYearChange(MaxDate.ToString(), FinID);
            string msg = "Financial Year Successfully Changed.";
            bool stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public bool FinancialYearChange(string SetDate, long Id)
        {
            bool stat = false;
            var Actlist = db.Accountss.ToList();
            string clbal = "Closing Balance" + Id;
            var checkyear = db.AccountsTransactions.Where(x => x.Purpose == clbal).Any();
            if (!checkyear)
            {
                foreach (var acc in Actlist)
                {
                    decimal Cre;
                    decimal Deb;
                    var DicBalance = com.Accbalance(acc.AccountsID);
                    decimal balance = (decimal)DicBalance["amount"];
                    string type = (string)DicBalance["type"];
                    Cre = (type == "Cr") ? balance : 0;
                    Deb = (type != "Cr") ? balance : 0;
                    if ((Cre != 0) || (Deb != 0))
                    {
                        DC typ = new DC();
                        typ = (type == "Cr") ? DC.Credit : DC.Debit;
                        var add = com.addAccountTrasaction(Deb, Cre, acc.AccountsID, clbal, 0, typ, Convert.ToDateTime(SetDate), null, null, null, null);
                    }
                }
            }
            return stat;
        }
    }
}
