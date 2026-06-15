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
    [RedirectingAction]
    public class SupplierController : BaseController
    {

        ApplicationDbContext db;
        Common com;
        public SupplierController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        public ActionResult SupplierProducts()
        {
            ViewBag.Items = QkSelect.List(
                  new List<SelectListItem>
                  {
                             new SelectListItem { Selected = false, Text = "Select Items", Value = ""},
                  }, "Value", "Text", 1);
            ViewBag.Category = QkSelect.List(
                 new List<SelectListItem>
                 {
                             new SelectListItem { Selected = false, Text = "Select Category", Value = ""},
                 }, "Value", "Text", 1);
            ViewBag.Brand = QkSelect.List(
                 new List<SelectListItem>
                 {
                             new SelectListItem { Selected = false, Text = "Select Brand", Value = ""},
                 }, "Value", "Text", 1);

            return View();
        }

        // GET: Customer
        [QkAuthorize(Roles = "Dev,Supplier")]
        public ActionResult Index()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.Supp = OpAll;
            ViewBag.Mobile = OpAll;
            ViewBag.Phone = OpAll;

            ViewBag.TxType = QkSelect.List(new List<SelectListItem>
                {
                    new SelectListItem { Text = "All"},
                    new SelectListItem { Text = "Item Wise", Value = "0"},
                    new SelectListItem { Text = "Exempt", Value = "1"},
                }, "Value", "Text");

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
            ViewBag.mnth7 = Last6thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth6 = Last5thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth5 = Last4thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth4 = last3rdMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth3 = Last2ndMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth2 = LastMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth1 = today.ToString("MMM", CultureInfo.InvariantCulture);
            var excludePurchaseparty = db.ParticularParties.Where(a => a.PartyType == 1).Select(a => a.PartyID).ToArray();
            VRDashboardViewModel vmodel = new VRDashboardViewModel();

            var todayurchase = db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, today) <= 0 && EF.Functions.DateDiffDay(b.PEDate, today) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var todayurchasertn = db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, today) <= 0 && EF.Functions.DateDiffDay(b.PRDate, today) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.TodayPurchase = Convert.ToString(todayurchase - todayurchasertn);
            vmodel.TodayPurchaseReturn = Convert.ToString(todayurchasertn);
            var thismnthpurchase = db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, today) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var thismnthpurchasertn = db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, today) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.ThisMonthPurchase = Convert.ToString(thismnthpurchase - thismnthpurchasertn);
            vmodel.ThisMonthPurchaseReturn = Convert.ToString(thismnthpurchasertn);

            var thisyearpurchase = db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, today) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var thisyearpurchasertn = db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, today) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.ThisYearPurchase = Convert.ToString(thisyearpurchase - thisyearpurchasertn);
            vmodel.ThisYearPurchaseReturn = Convert.ToString(thisyearpurchasertn);

            var lastmnthpurchase = db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, LastMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var lastmnthpurchasertn = db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, LastMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.LastMonthPurchase = Convert.ToString(lastmnthpurchase - lastmnthpurchasertn);
            vmodel.LastMonthPurchaseReturn = Convert.ToString(lastmnthpurchasertn);

            var last2mnthpurchase = db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, Last2ndMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var last2mnthpurchasertn = db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, Last2ndMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.LastTwoMonthPurchase = Convert.ToString(last2mnthpurchase - last2mnthpurchasertn);
            vmodel.LastTwoMonthPurchaseReturn = Convert.ToString(last2mnthpurchasertn);

            var last3mnthPurchase = db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, last3rdMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var last3mnthPurchasertn = db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, last3rdMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.LastThreeMonthPurchase = Convert.ToString(last3mnthPurchase - last3mnthPurchasertn);
            vmodel.LastThreeMonthPurchaseReturn = Convert.ToString(last3mnthPurchasertn);

            var last4mnthpurchase = db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, Last4thMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var last4mnthpurchasertn = db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, Last4thMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.LastFourMonthPurchase = Convert.ToString(last4mnthpurchase - last4mnthpurchasertn);
            vmodel.LastFourMonthPurchaseReturn = Convert.ToString(last4mnthpurchasertn);

            var last5mnthpurchase = db.PurchaseEntrys.Where(b => (EF.Functions.DateDiffDay(b.PEDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PEDate, Last5thMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var last5mnthpurchasertn = db.PurchaseReturns.Where(b => (EF.Functions.DateDiffDay(b.PRDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.PRDate, Last5thMnthLastDay) >= 0) && (!excludePurchaseparty.Contains(b.Supplier))).Select(b => b.PRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.LastFiveMonthPurchase = Convert.ToString(last5mnthpurchase - last5mnthpurchasertn);
            vmodel.LastFiveMonthPurchaseReturn = Convert.ToString(last5mnthpurchasertn);

            return View(vmodel);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Supplier")]
        public JsonResult GetSupplier(long? opcls,long? Supplier, string TaxReg, long? Mobile, long? Phone, decimal? CLimit, int? CPeriod, string TxType,string Address, string MailId, string Alias)
        {
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

            TaxType ttype = new TaxType();
            if (TxType != null && TxType != "")
            {
                ttype = (TxType == "0") ? TaxType.ItemWise : TaxType.Exempt;
            }

            var userpermission = User.IsInRole("All Suppliers");
            var UserId = User.Identity.GetUserId();

            var uDev = User.IsInRole("Dev");
            var uSupplierView = User.IsInRole("View Supplier");
            var uEdit = User.IsInRole("Edit Supplier");
            var uDelete = User.IsInRole("Delete Supplier");

            var v = (from a in db.Suppliers
                     join x in db.Accountss on a.Accounts equals x.AccountsID
                     join b in db.Contacts on a.Contact equals b.ContactID into tmp
                     from b in tmp.DefaultIfEmpty()

                     //let mob = (
                     //           where (rrr.RelationID == a.SupplierID && rrr.RelationType == 1)

                     //               Num = co.Mobile,
                     //               Name = co.Name,
                     //               emails = co.EmailId,


                     //           }).ToList()
                     where (Supplier == null || Supplier == 0 || a.SupplierID == Supplier) &&
                           (TaxReg == null || TaxReg == "" || x.TRN == TaxReg) &&
                           (Mobile == null || Mobile == 0 || b.ContactID == Mobile) &&
                           (Phone == null || Phone == 0 || a.CreditPeriod == CPeriod) &&
                           (TxType == null || TxType == "" || a.TaxType == ttype) &&
                           (MailId == null || MailId == "" || b.EmailId == MailId)
                           && (userpermission == true || x.CreatedBy == UserId)
                           && (Alias == null || Alias == "" || x.Alias == Alias)
                     &&(opcls == null || a.Status == opcls) 
                     select new
                     {
                         id = a.SupplierID,
                         a.SupplierCode,
                         a.SupplierName,
                         TaxRegNo = x.TRN,
                         a.logtime,
                         Address= a.Addres,
                         Phone = b.Phone,
                         // Mobile = b.Mobile,
                         Email = b.EmailId,
                         CreditLimit = a.CreditLimit,
                         CreditPeriod = a.CreditPeriod,
                         OpnBalance = (x.OpnBalanceCr > 0) ? (x.OpnBalanceCr != 0 ? x.OpnBalanceCr + " Cr." : "0.00") : (x.OpnBalance != 0 ? x.OpnBalance + " Dr." : "0.00"),
                         Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0),
                         Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0),
                         Dev = uDev,
                         Details = uSupplierView,
                         Edit = uEdit,
                         Delete = uDelete,
                         Alias = x.Alias,
                  
                         mobmodel = (from co in db.Contacts
                                     join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                     where (rrr.RelationID == a.SupplierID && rrr.RelationType == 1)
                                     select new
                                     {

                                         Num = co.Mobile,
                                         Name = co.Name,
                                         emails = co.EmailId,


                                     }).ToList(),

                     }).Select(o => new
                     {
                         o.id,
                         o.SupplierCode,
                         o.SupplierName,
                         o.TaxRegNo,
                         o.Address,
                         o.Phone,
                         o.Email,
                         o.CreditLimit,
                         o.CreditPeriod,
                         o.OpnBalance,
                         o.Credit,
                         o.Debit,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.logtime,
                         o.Delete,
                         o.Alias,
                         o.mobmodel,
                         currentbalance = (o.Debit > o.Credit) ? ((o.Debit - o.Credit) + " Dr.") : ((o.Credit - o.Debit) + " Cr."),
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.SupplierName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.SupplierCode.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.TaxRegNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Alias.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.OpnBalance.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.currentbalance.ToString().ToLower().Contains(search.ToLower())
                                 //p.CreditPeriod.ToString().ToLower().Contains(search.ToLower())
                                 );
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            v = v.OrderByDescending(o=>o.logtime);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [HttpPost]
        public JsonResult GetSuppliers(long? Item, long? Brand, long? Category)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

    

 

            var v = (from a in db.Suppliers
                     join x in db.SupplierItems on a.SupplierID equals x.SupplierId into xx
                     from x in xx.DefaultIfEmpty()
                     join bb in db.SupplierBrands on a.SupplierID equals bb.SupplierId into bbb
                     from bb in bbb.DefaultIfEmpty()
                     join c in db.SupplierCategories on a.SupplierID equals c.SupplierId into cc
                     from c in cc.DefaultIfEmpty()
                     join p in db.PurchaseEntrys on a.SupplierID equals p.Supplier into pp
                     from p in pp.DefaultIfEmpty()
                     join pe in db.PEItemss on p.PurchaseEntryId equals pe.PurchaseEntry into pee
                     from pe in pee.DefaultIfEmpty()
                     join it in db.Items on pe.Item equals it.ItemID into itt
                     from it in itt.DefaultIfEmpty()
                     join br in db.ItemBrands on it.ItemBrandID equals br.ItemBrandID into brr
                     from br in brr.DefaultIfEmpty()
                     join cat in db.ItemCategorys on it.ItemCategoryID equals cat.ItemCategoryID into catt
                     from cat in catt.DefaultIfEmpty()
                   
                     let mob = (
                                from co in db.Contacts
                                join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                where (rrr.RelationID == a.SupplierID && rrr.RelationType == 1)
                                select new
                                {

                                    Num = co.Mobile,
                                    Name = co.Name,
                                    emails = co.EmailId,


                                }).ToList()
                     where (Item == null || Item == 0 || x.ItemId == Item || it.ItemID == Item) &&
                           (Brand == null || Brand == 0 || bb.BrandId == Brand || br.ItemBrandID == Brand) &&
                           (Category == null || Category == 0 ||c.CategoryId == Category || cat.ItemCategoryID == Category)
 //(Category == null || Category == 0 || cat.ItemCategoryID == Category) 

 && a.Status != 0

                     select new
                     {
                         id = a.SupplierID,
                         a.SupplierCode,
                         a.SupplierName,
                       

                         mobmodel = mob,


                     }).Select(o => new
                     {
                         o.id,
                         o.SupplierCode,
                         o.SupplierName,
                         
                         o.mobmodel,
                       
                     }).GroupBy(x => x.SupplierName, (key, g) => g.OrderByDescending(m => m.id).FirstOrDefault()); ;
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.SupplierName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.SupplierCode.ToString().ToLower().Contains(search.ToLower()) 
                                               );
            }

            //SORT
            var data = v;
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpGet]
        public JsonResult CustomerCheck(string customer, long? cusid)
        {
            var CusCheck = db.Suppliers.Where(x => x.SupplierName == customer ).Any();
            var rslt = false;
            if (CusCheck == true)
            {
                if (cusid != 0)
                {
                    var cust = db.Suppliers.Where(x => x.SupplierName == customer ).FirstOrDefault();
                    CusCheck = (cust.SupplierID == cusid) ? false : true;
                }
            }
            rslt = (CusCheck) ? true : false;
            return Json(rslt);
        }

        public JsonResult SearchCustCheck(string q, string x)
        {
            List<SelectFormatDisabled> serialisedJson;
            string stt = " ";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Suppliers
                                  join c in db.Contacts on b.Contact equals c.ContactID into cnts
                                  from c in cnts.DefaultIfEmpty()
                                  join d in db.Accountss on b.Accounts equals d.AccountsID into acc
                                  from d in acc.DefaultIfEmpty()
                                  where (d.Alias.ToLower().Contains(q.ToLower()) || b.SupplierName.ToLower().Contains(q.ToLower()) || b.SupplierCode.ToLower().Contains(q.ToLower()) || d.Alias.Contains(q) || b.SupplierName.Contains(q) || b.SupplierCode.Contains(q) || c.Phone.Replace(" ", "").Contains(q) //|| j.MobileNum.Replace(" ", "").Contains(q)
                                  || b.SupplierName.StartsWith(q) || b.SupplierName.EndsWith(q)) 
                       
                                  select new SelectFormatDisabled
                                  {
                                      text = b.SupplierCode + " - " + b.SupplierName, //each json object will have 
                                      id = b.SupplierID.ToString(),
                                      disabled = "true"
                                  }).Take(10)
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Suppliers
                         
                                  select new SelectFormatDisabled
                                  {
                                      text = b.SupplierCode + " - " + b.SupplierName, //each json object will have 
                                      id = b.SupplierID.ToString(),
                                      disabled = "true"
                                  }).Take(10).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatDisabled() { id = "0", text = stt, disabled = "true" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        [QkAuthorize(Roles = "Dev,Create Supplier")]
        public ActionResult Create()
        {
            var userpermission = User.IsInRole("All Suppliers");
            var UserId = User.Identity.GetUserId();
            ViewBag.CustName = QkSelect.List(
                  new List<SelectListItem>
                  {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
}, "Value", "Text", 1);
            var viewModel = new SupplierSubmitViewModel
            {
                SupplierCode = SuppCode(),
            };

            //enable bill to bill payment
            var ToPayment = db.EnableSettings.Where(a => a.EnableType == "BillToBillPayment").FirstOrDefault();
            var BillTo = ToPayment != null ? (ToPayment.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToPayment = BillTo;

            ViewBag.LastEntry = db.Suppliers.Where(p => (userpermission == true)).Select(p => p.SupplierID).AsEnumerable().DefaultIfEmpty(0).Max();

            ViewBag.MultiSelect = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                              }, "Value", "Text", 0);

            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", viewModel);
            }
            else
            {
                return View(viewModel);
            }
        }


        private string SuppCode(Int64 SNo = 0, string SCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Supplier").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Supplier").Select(a => a.number).FirstOrDefault();
            if (SCode == null)
            {
                if ((db.Suppliers.Select(p => p.SupplierID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        SCode = prefix + 1;
                    }
                    else
                    {
                        SCode = prefix + number;
                    }
                }
                else
                {
                    SNo = db.Suppliers.Max(p => p.SupplierID + 1);
                    SCode = prefix + SNo;
                    if (CodeExist(SCode))
                    {
                        SCode = SuppCode(SNo, SCode);
                    }

                }
            }
            else
            {
                SNo = SNo + 1;
                SCode = prefix + SNo;
                if (CodeExist(SCode))
                {
                    SCode = SuppCode(SNo, SCode);
                }
            }
            return SCode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.Suppliers.Any(c => c.SupplierCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }
        [HttpPost]
        public ActionResult setopencloseupdate(int OpenClose, long protaskid)
        {
            var pr = db.Suppliers.Find(protaskid);
            pr.Status = OpenClose;
            pr.logtime = System.DateTime.Now;
            db.Entry(pr).State = EntityState.Modified;
            db.SaveChanges();
            bool stat = true;
            string msg = "Successfully Updated Status.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        public ActionResult setopenclose(long id)
        {
            var Stat = QkSelect.List(
                new List<SelectListItem> {

                new SelectListItem { Value="0",Text="Close"}, new SelectListItem { Value="1",Text="Open"},}, "Value", "Text");
            ViewBag.openclose = Stat;
            ViewBag.protask = id;
            return PartialView();
        }
        [HttpPost]

        // [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Supplier")]
        public ActionResult Create(SupplierSubmitViewModel sumodel)
        {
            var cusExists = db.Suppliers.Any(u => u.SupplierCode == sumodel.SupplierCode);
            if (cusExists)
            {
                Danger("A Supplier with same Supplier Code exists.", true);
                return RedirectToAction("Create", "Supplier");
            }
            else
            {
                if (ModelState["SupplierID"] != null)
                {
                    ModelState["SupplierID"].Errors.Clear();
                }
                if (ModelState["StateID"] != null)
                {
                    ModelState["StateID"].Errors.Clear();
                }
                if (ModelState["LocationID"] != null)
                {
                    ModelState["LocationID"].Errors.Clear();
                }




                if (ModelState.IsValid)
                {
                    Int64 contactId = 0;
                    Int64 accountId = 0;
                    var UserId = User.Identity.GetUserId();
                    var Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();



                    Accounts account = new Accounts();
                    account.Name = sumodel.SupplierName;
                    account.Alias = sumodel.Alias;
                    account.PrintName = sumodel.SupplierName;
                    account.Group = 14;
                    account.Status = Status.active;
                    account.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    account.CreatedBy = UserId;
                    account.TRN = sumodel.TaxRegNo;
                    if (sumodel.DC == DC.Debit)
                    {
                        account.OpnBalance = sumodel.OpnBalance;
                        account.OpnBalanceCr = 0;
                    }
                    if (sumodel.DC == DC.Credit)
                    {
                        account.OpnBalance = 0;
                        account.OpnBalanceCr = sumodel.OpnBalance;
                    }

                    db.Accountss.Add(account);
                    db.SaveChanges();
                    accountId = account.AccountsID;

                    //adding supplier contact

                    Supplier sup = new Supplier
                    {

                        Contact = contactId,
                        Accounts = accountId,
                        SupplierName = sumodel.SupplierName,
                        SupplierCode = sumodel.SupplierCode,
                        CreditLimit = sumodel.CreditLimit != null ? (decimal)sumodel.CreditLimit : 0,
                        CreditPeriod = sumodel.CreditPeriod != null ? (int)sumodel.CreditPeriod : 0,
                        Remark = sumodel.Address,
                        BankName = sumodel.BankName,
                        AccountNo = sumodel.AccountNo,
                        BranchName = sumodel.BranchName,
                        IbanNo = sumodel.IbanNo,
                        Swift = sumodel.Swift,
                        TaxType = sumodel.TaxType,
                        Addres = sumodel.Addres,
                        Status=1,
                        logtime=System.DateTime.Now
                    };
                    db.Suppliers.Add(sup);
                    db.SaveChanges();


                    Int64 SuppId = sup.SupplierID;
                    var invoices = "";
                    if (sumodel.invoicedata != null)
                    {
                        bool first = true;
                        foreach (var arr in sumodel.invoicedata)
                        {
                            if (arr.Invoice != null && arr.Amount > 0)
                            {
                                if (!BillExist(arr.Invoice))
                                {
                                    //move to purchase
                                    var sale = InsertToPurchase(arr, SuppId, UserId, Branch);
                                }
                                else
                                {
                                    invoices += (first == true) ? arr.Invoice : " , " + arr.Invoice;
                                    first = false;
                                }
                            }
                        }
                    }
                    //adding dropdown contacts on supplier

                    if (sumodel.LstContacts != null && sumodel.LstContacts.Count > 0)
                    {
                        foreach (var item in sumodel.LstContacts)
                        {
                            SupplierSubmitViewModel supmodel = new SupplierSubmitViewModel();
                            {
                                var Addsupplier = new SupplierSubmitViewModel
                                {



                                    SupplierName = sumodel.SupplierName,
                                    ContactID = sumodel.Contact,
                                    SupplierCode = sumodel.SupplierCode,
                                    SupplierID = sumodel.SupplierID,
                                    Country = sumodel.Country,
                                    TypeOfContact = sumodel.TypeOfContact,
                                    ContactTypeID = sumodel.ContactTypeID,
                                    Mobile = sumodel.Mobile,
                                    Phone = sumodel.Phone,

                                    EmailId = sumodel.EmailId,
                                    Website = sumodel.Website,
                                    CountryID = sumodel.CountryID,



                                };
                            }
                            var contact = new Contact

                            {

                                // contactId = item.ContactID_ContactID,

                                Country = item.Country,
                                FirstName = item.FirstName,
                                LastName = item.LastName,
                                Name = item.FirstName + " " + item.LastName,
                                TypeOfContact = item.TypeOfContact,
                                Mobile = item.Mobile,
                                Phone = item.Phone,
                                EmailId = item.EmailId,
                                Website = item.Website,
                                Group = 2,
                                Status = Status.active,
                                CountryID = item.CountryID,
                                ContactTypeID = item.ContactTypeID




                            };

                            db.Contacts.Add(contact);
                            db.SaveChanges();
                            contactId = contact.ContactID;
                            //    Contact = contactId,
                            //    MobileNum = item.Mobile,
                            //    Name = item.FirstName + " " + item.LastName

                            ContactRelation Relation = new ContactRelation();
                            Relation.ContactID = contactId;
                            Relation.RelationType = (long)ContctRelation.supplier;//for supplier
                            Relation.RelationID = SuppId;
                            db.ContactRelation.Add(Relation);
                            db.SaveChanges();

                        }
                    }
                    var today = System.DateTime.Now;
                    if (sumodel.SuppliDocumentviewmodel != null && sumodel.SuppliDocumentviewmodel.Count > 0)
                    {
                        var fileName = "";
                        int i = 0;
                        foreach (var item in sumodel.SuppliDocumentviewmodel)
                        {
                            if (item.FilePath != null)
                            {
                                // files upload
                                IFormFile file = Request.Form.Files[i];
                                fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                var uploadUrl = LegacyWeb.MapPath("~/uploads/supplierdocuments/");
                                if (!Directory.Exists(uploadUrl))
                                    Directory.CreateDirectory(uploadUrl);
                                file.SaveAs(Path.Combine(uploadUrl, fileName));
                            }
                            else
                            {
                                fileName = item.FilePath;
                            }
                            i++;

                            CustomerDocument CustomerDocuments = new CustomerDocument
                            {

                                CutomerID = SuppId,
                                DoucumentType = "supplier",
                                DocumentTypeID = item.DocumentTypeID,
                                Expiry = item.Expiry,
                                Notes = item.Notes,
                                FilePath = fileName
                            };
                            db.CustomerDocuments.Add(CustomerDocuments);
                            db.SaveChanges();


                        }
                    }

                    //Supplier Items
                    if (sumodel.SupplierItems != null )
                    {
                        SupplierItems ItemObj = new SupplierItems();

                        foreach (var row in sumodel.SupplierItems)
                        {
                            ItemObj.SupplierId = SuppId;
                            ItemObj.ItemId = row;
                     
                            db.SupplierItems.Add(ItemObj);
                            db.SaveChanges();
                        }
                    }

                    //Supplier Categories
                    if (sumodel.SupplierCategories != null)
                    {
                        SupplierCategories CatgObj = new SupplierCategories();

                        foreach (var row in sumodel.SupplierCategories)
                        {
                            CatgObj.SupplierId = SuppId;
                            CatgObj.CategoryId = row;

                            db.SupplierCategories.Add(CatgObj);
                            db.SaveChanges();
                        }
                    }

                    //Supplier Brands
                    if (sumodel.SupplierBrands != null)
                    {
                        SupplierBrands BrandObj = new SupplierBrands();

                        foreach (var row in sumodel.SupplierBrands)
                        {
                            BrandObj.SupplierId = SuppId;
                            BrandObj.BrandId = row;

                            db.SupplierBrands.Add(BrandObj);
                            db.SaveChanges();
                        }
                    }

                    if (invoices != "")
                    {
                        Warning(invoices + " Already exists");
                    }

                    if (sumodel.OpnBalance > 0)
                    {
                        if (sumodel.DC == DC.Debit)
                        {
                            com.addAccountTrasaction(sumodel.OpnBalance, 0, account.AccountsID, "Opening Balance", account.AccountsID, DC.Debit);

                        }
                        if (sumodel.DC == DC.Credit)
                        {
                            com.addAccountTrasaction(0, sumodel.OpnBalance, account.AccountsID, "Opening Balance", account.AccountsID, DC.Credit);
                        }
                    }

                    com.addlog(LogTypes.Created, UserId, "Supplier", "Suppliers", findip(), sup.SupplierID, "Successfully added Supplier details");


                    Success("Successfully added Supplier details.", true);
                    return RedirectToAction("Create", "Supplier");
                }
                else
                {
                    Warning("Looks like something went wrong. Please check your form.", true);
                    // Re-populate the dropdown SelectLists (as GET Create does) and pass the submitted model back, so an
                    // invalid submit re-renders the form with validation messages instead of throwing
                    // "There is no ViewData item of type 'IEnumerable<SelectListItem>'" -> 500. (jQuery validation
                    // normally blocks invalid submits, but guard the server path too.)
                    var up = User.IsInRole("All Suppliers");
                    ViewBag.CustName = QkSelect.List(new List<SelectListItem> { new SelectListItem { Selected = false, Text = null, Value = null } }, "Value", "Text", 1);
                    var toPay = db.EnableSettings.Where(a => a.EnableType == "BillToBillPayment").FirstOrDefault();
                    ViewBag.BillToPayment = toPay != null ? (toPay.Status == Status.active ? 0 : 1) : 1;
                    ViewBag.LastEntry = db.Suppliers.Where(p => up).Select(p => p.SupplierID).AsEnumerable().DefaultIfEmpty(0).Max();
                    ViewBag.MultiSelect = QkSelect.List(new List<SelectListItem> { new SelectListItem { Selected = false, Text = "", Value = "" } }, "Value", "Text", 0);
                    return View(sumodel);
                }
            }
        }

        public Boolean InsertToPurchase(ReferenceAccountViewModel arr, long SuppId, string UserId, long branch)
        {
            PurchaseEntry PEentry = new PurchaseEntry();
            PEentry.PENo = 0;
            PEentry.BillNo = arr.Invoice;
            PEentry.PEDate = DateTime.Parse(arr.RADate, new CultureInfo("en-GB"));
            PEentry.PECashier = 0;
            PEentry.Supplier = SuppId;
            PEentry.PayType = "";//need change
            PEentry.PEItems = 0;
            PEentry.PEItemQuantity = 0;
            PEentry.PESubTotal = 0;
            PEentry.PETax = 0;
            PEentry.PETaxAmount = 0;
            PEentry.PEDiscount = 0;
            PEentry.PEGrandTotal = 0;
            PEentry.PENote = "";
            PEentry.Print = 1;
            PEentry.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
            PEentry.CreatedBy = UserId;
            PEentry.Status = 0;
            PEentry.Branch = branch;
            PEentry.Remarks = "Purchase Entry From Supplier Creation";
            PEentry.SupplierType = SupplierType.CashSale;

            db.PurchaseEntrys.Add(PEentry);
            db.SaveChanges();
            Int64 purchaseEntryId = PEentry.PurchaseEntryId;



            PEPayment PEpay = new PEPayment();
            PEpay.SupplierId = SuppId;
            PEpay.PEDate = DateTime.Parse(arr.RADate, new CultureInfo("en-GB"));
            PEpay.PEEntryDate = Convert.ToDateTime(System.DateTime.Now);
            PEpay.PEBillAmount = (decimal)arr.Amount;
            PEpay.PEPaidAmount = 0;
            PEpay.CreatedBranch = branch;
            PEpay.CreatedUserId = UserId;
            PEpay.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
            PEpay.Status = 0;
            PEpay.PurchaseEntry = purchaseEntryId;
            db.PEPayments.Add(PEpay);
            db.SaveChanges();
            return true;
        }

        private bool BillExist(string SENo)
        {
            var Exists = db.PurchaseEntrys.Any(c => c.BillNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }

        // GET: Supplier/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Supplier")]
        public ActionResult Edit(long? id)
        {

            var userpermission = User.IsInRole("All Suppliers");
            var UserId = User.Identity.GetUserId();
            ViewBag.DocList = db.DocumentTypes.ToList();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sup = (from a in db.Suppliers
                       join b in db.Accountss on a.Accounts equals b.AccountsID
                       where a.SupplierID == id && (userpermission == true || b.CreatedBy == b.CreatedBy)
                       select new
                       {
                           a.Contact,
                           a.Accounts,
                           a.SupplierName,
                           a.SupplierCode,
                           a.CreditLimit,
                           a.CreditPeriod,
                           a.Remark,
                           TaxRegNo = b.TRN,
                           a.TaxType,
                           a.Addres,
                           a.BankName,
                           a.AccountNo,
                           a.IbanNo,
                           a.BranchName,
                           a.Swift,
                           a.SupplierID
                       }).FirstOrDefault();

            if (sup == null)
            {
                return NotFound();
            }


            //enable bill to bill payment
            var ToPayment = db.EnableSettings.Where(a => a.EnableType == "BillToBillPayment").FirstOrDefault();
            var BillTo = ToPayment != null ? (ToPayment.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToPayment = BillTo;
            Contact cont = db.Contacts.Find(sup.Contact);
            Accounts account = db.Accountss.Find(sup.Accounts);

            SupplierSubmitViewModel supmodel = new SupplierSubmitViewModel();

            supmodel.SupplierID = sup.SupplierID;
            supmodel.SupplierName = sup.SupplierName;
            supmodel.SupplierCode = sup.SupplierCode;
            supmodel.TaxRegNo = account.TRN;
            supmodel.CreditLimit = sup.CreditLimit;
            supmodel.CreditPeriod = sup.CreditPeriod;
            supmodel.Remark = sup.Remark;
            supmodel.TaxType = sup.TaxType;
            supmodel.Addres = sup.Addres;
            supmodel.BankName = sup.BankName;
            supmodel.AccountNo = sup.AccountNo;
            supmodel.IbanNo = sup.IbanNo;
            supmodel.BranchName = sup.BranchName;
            supmodel.Swift = sup.Swift;
            supmodel.Alias = account.Alias;
            if (account.OpnBalance == 0)
            {
                supmodel.DC = DC.Credit;
                supmodel.OpnBalance = account.OpnBalanceCr;
            }
            else
            {
                supmodel.DC = DC.Debit;
                supmodel.OpnBalance = account.OpnBalance;
            }
            supmodel.SuppliDocumentviewmodel = (from cd in db.CustomerDocuments
                                                where cd.CutomerID == sup.SupplierID & cd.DoucumentType == "Supplier"

                                                select new SupplierDocumentviewmodel
                                                {

                                                    CutomerID = cd.CutomerID,
                                                    ContactId = cd.ContactId,
                                                    //DoucumentType = cd.DoucumentType,
                                                    Expiry = cd.Expiry,
                                                    FilePath = cd.FilePath,
                                                    filenamelead = cd.FilePath,
                                                    Notes = cd.Notes,

                                                }
                                        ).ToList();

            supmodel.LstContacts = (from c in db.Contacts
                                    join cr in db.ContactRelation
                                    on new { c.ContactID, RelationType = (long)ContctRelation.supplier }
                                 equals new { cr.ContactID, cr.RelationType }
                                    where (cr.RelationID == supmodel.SupplierID)
                                    select new
                                    {

                                        ContactID = c.ContactID
                                        ,
                                        Name = c.Name
                                        ,
                                        FirstName = c.FirstName
                                        ,
                                        LastName = c.LastName
                                        ,
                                        Address = c.Address
                                        ,
                                        Country = c.Country
                                        ,
                                        State = c.State
                                        ,
                                        City = c.City
                                        ,
                                        Zip = c.Zip
                                        ,
                                        Phone = c.Phone
                                        ,
                                        Mobile = c.Mobile
                                        ,
                                        Fax = c.Fax
                                        ,
                                        EmailId = c.EmailId
                                        ,
                                        Reference = c.Reference
                                        ,
                                        ContactPerson = c.ContactPerson
                                        ,
                                        Status = c.Status
                                        ,
                                        Group = c.Group
                                        ,
                                        SalesPMob = c.SalesPMob
                                        ,
                                        TypeOfContact = c.TypeOfContact
                                        ,
                                        Website = c.Website
                                        ,
                                        CountryID = c.CountryID
                                        ,
                                        ContactTypeID = c.ContactTypeID


                                    }).AsEnumerable().Select(x => new Contact
                                    {


                                        ContactID = x.ContactID
                                        ,
                                        Name = x.Name
                                        ,
                                        FirstName = x.FirstName
                                        ,

                                        LastName = x.LastName
                                        ,
                                        Address = x.Address
                                        ,
                                        Country = x.Country
                                        ,
                                        State = x.State
                                        ,
                                        City = x.City
                                        ,
                                        Zip = x.Zip
                                        ,
                                        Phone = x.Phone
                                        ,
                                        Mobile = x.Mobile
                                        ,
                                        Fax = x.Fax
                                        ,
                                        EmailId = x.EmailId
                                        ,
                                        Reference = x.Reference
                                        ,
                                        ContactPerson = x.ContactPerson
                                        ,
                                        Status = x.Status
                                        ,
                                        Group = x.Group
                                        ,
                                        SalesPMob = x.SalesPMob
                                        ,
                                        TypeOfContact = x.TypeOfContact
                                        ,
                                        Website = x.Website
                                        ,
                                        ContactTypeID = x.ContactTypeID
                                        ,
                                        CountryID = x.CountryID
                                        ,
                                    }).ToList();
            var oldcontacts = (from c in db.Contacts
                               join d in db.Suppliers on c.ContactID
         equals d.Contact
                               where c.ContactID == sup.Contact
                               select new
                               {
                                   ContactID = c.ContactID
                                        ,
                                   Name = c.Name
                                        ,
                                   FirstName = c.FirstName
                                        ,

                                   LastName = c.LastName
                                        ,
                                   Address = c.Address
                                        ,
                                   Country = c.Country
                                        ,
                                   State = c.State
                                        ,
                                   City = c.City
                                        ,
                                   Zip = c.Zip
                                        ,
                                   Phone = c.Phone
                                        ,
                                   Mobile = c.Mobile
                                        ,
                                   Fax = c.Fax
                                        ,
                                   EmailId = c.EmailId
                                        ,
                                   Reference = c.Reference
                                        ,
                                   ContactPerson = c.ContactPerson
                                        ,
                                   Status = c.Status
                                        ,
                                   Group = c.Group
                                        ,
                                   SalesPMob = c.SalesPMob
                                        ,
                                   TypeOfContact = c.TypeOfContact
                                        ,
                                   Website = c.Website
                                        ,
                                   CountryID = c.CountryID
                                        ,
                                   ContactTypeID = c.ContactTypeID
                               }).AsEnumerable().Select(x => new Contact
                               {


                                   ContactID = x.ContactID
                                   ,
                                   Name = x.Name
                                   ,
                                   FirstName = x.FirstName
                                   ,

                                   LastName = x.LastName
                                   ,
                                   Address = x.Address
                                   ,
                                   Country = x.Country
                                   ,
                                   State = x.State
                                   ,
                                   City = x.City
                                   ,
                                   Zip = x.Zip
                                   ,
                                   Phone = x.Phone
                                   ,
                                   Mobile = x.Mobile
                                   ,
                                   Fax = x.Fax
                                   ,
                                   EmailId = x.EmailId
                                   ,
                                   Reference = x.Reference
                                   ,
                                   ContactPerson = x.ContactPerson
                                   ,
                                   Status = x.Status
                                   ,
                                   Group = x.Group
                                   ,
                                   SalesPMob = x.SalesPMob
                                   ,
                                   TypeOfContact = x.TypeOfContact
                                   ,
                                   Website = x.Website
                                   ,
                                   CountryID = x.CountryID
                                   ,
                                   ContactTypeID = x.ContactTypeID
                               }).ToList();


            supmodel.LstContacts = oldcontacts.Union(supmodel.LstContacts).ToList();


            ViewBag.preEntry = db.Suppliers.Where(a => a.SupplierID < id && (userpermission == true)).Select(a => a.SupplierID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Suppliers.Where(a => a.SupplierID > id && (userpermission == true)).Select(a => a.SupplierID).DefaultIfEmpty().Min();
            ViewBag.TypeList = db.ContactTypes.ToList();
            ViewBag.CountryCodes = db.Country.ToList();

            //Supplier Items
            supmodel.SupplierItems      =   db.SupplierItems.Where(a => a.SupplierId == id).Select(a => a.ItemId).ToList().ToArray() ?? null;
           
            //Supplier Categories
            supmodel.SupplierCategories =   db.SupplierCategories.Where(a => a.SupplierId == id).Select(a => a.CategoryId).ToList().ToArray() ?? null;

            //Supplier Brands
            supmodel.SupplierBrands     =   db.SupplierBrands.Where(a => a.SupplierId == id).Select(a => a.BrandId).ToList().ToArray() ?? null;

            //Supplier Items
            var Items = db.Items
                        .Select(s => new
                        {
                            ID      =   s.ItemID,
                            Name    =   s.ItemName,
                        }).ToList();
            
            //Supplier Categories
            var Categories = db.ItemCategorys
                            .Select(s => new
                            {
                                ID      =   s.ItemCategoryID,
                                Name    =   s.ItemCategoryName
                            }).ToList();
            
           //Supplier Brands
           var Brands = db.ItemBrands
                        .Select(s => new
                        {
                            ID      =   s.ItemBrandID,
                            Name    =   s.ItemBrandName
                        }).ToList();

            ViewBag.SupplierItem       =   new MultiSelectList(Items,      "ID", "Name", supmodel.SupplierItems);

            ViewBag.SupplierCategorys   =   new MultiSelectList(Categories, "ID", "Name", supmodel.SupplierCategories);

            ViewBag.SupplierBrand      =   new MultiSelectList(Brands,     "ID", "Name", supmodel.SupplierBrands);
        
            var rtype = Request.Query["rtype"];

            if (rtype == "APP")
            {
                return View("App/Edit", supmodel);
            }
            else
            {
                return View(supmodel);
            }
        }

        // POST: supplier/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Supplier")]
        public ActionResult Edit(SupplierSubmitViewModel supmodel, long? id)
        {
            if (ModelState.IsValid)
            {
                var CodeExists = db.Suppliers.Any(u => u.SupplierCode == supmodel.SupplierCode && u.SupplierID != id);
                if (CodeExists)
                {
                    Danger("A Supplier with same Supplier code exists.", true);
                    return RedirectToAction("Edit/" + id, "Supplier");
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    Supplier sup = db.Suppliers.Find(id);
                    sup.SupplierName = supmodel.SupplierName;
                    sup.SupplierCode = supmodel.SupplierCode;
                    sup.CreditLimit = supmodel.CreditLimit != null ? (decimal)supmodel.CreditLimit : 0;
                    sup.CreditPeriod = supmodel.CreditPeriod != null ? (int)supmodel.CreditPeriod : 0;
                    sup.Remark = supmodel.Remark;
                    sup.BankName = supmodel.BankName;
                    sup.AccountNo = supmodel.AccountNo;
                    sup.BranchName = supmodel.BranchName;
                    sup.IbanNo = supmodel.IbanNo;
                    sup.Swift = supmodel.Swift;
                    sup.TaxType = supmodel.TaxType;
                    sup.Addres = supmodel.Addres;
                    sup.Contact = 0;
                    sup.logtime = System.DateTime.Now;
                    db.Entry(sup).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 SupId = sup.SupplierID;




                    if (supmodel.LstContacts == null)   //for deleting first added contact as lst=null
                    {

                        db.ContactRelation.RemoveRange(db.ContactRelation.Where(ro => ro.RelationID == SupId));
                        db.Contacts.RemoveRange(db.Contacts.Where(d => d.ContactID == sup.Contact));
                    }
                    if (supmodel.LstContacts != null)   //for deleting second added contact as lst=true
                    {
                        db.Contacts.RemoveRange(db.Contacts.Where(d => d.ContactID == sup.Contact));
                    }
                    if (supmodel.SuppliDocumentviewmodel == null)
                    {

                        db.CustomerDocuments.RemoveRange(db.CustomerDocuments.Where(a => a.CutomerID == SupId));
                        db.SaveChanges();
                    }



                    if (supmodel.LstContacts != null && supmodel.LstContacts.Count > 0)
                    {
                        long contactId = 0;
                        long[] contactRelationids = new long[supmodel.LstContacts.Count];



                        int rowRelationcount = 0;
                        foreach (var item in supmodel.LstContacts)
                        {
                            var contact = new Contact
                            {
                                Address = supmodel.Address,
                                ContactID = item.ContactID,
                                Name = item.FirstName + " " + item.LastName,
                                City = supmodel.City,
                                State = supmodel.State,
                                Country = item.Country,
                                Zip = supmodel.Zip,
                                FirstName = item.FirstName,
                                LastName = item.LastName,
                                TypeOfContact = item.TypeOfContact,
                                Mobile = item.Mobile,
                                Phone = item.Phone,
                                Fax = supmodel.Fax,
                                EmailId = item.EmailId,
                                Reference = supmodel.Reference,
                                ContactPerson = supmodel.ContactPerson,
                                Website = item.Website,
                                Group = 2,
                                Status = Status.active,
                                CountryID = item.CountryID,
                                ContactTypeID = item.ContactTypeID

                            };


                            if (item.ContactID > 0 && contactId == sup.Contact)
                            {

                                contact.ContactID = item.ContactID;
                                db.Entry(contact).State = EntityState.Modified;
                                db.SaveChanges();
                                db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == item.ContactID));
                                db.SaveChanges();
                            }
                            else
                            {
                                db.Contacts.Add(contact);
                                db.SaveChanges();
                                contactId = contact.ContactID;

                            }





                            //    Contact = contactId,
                            //    MobileNum = item.Mobile,
                            //    Name = item.FirstName + " " + item.LastName



                            var ContactReltion = (from a in db.ContactRelation
                                                  where a.RelationID == id && a.RelationType == (long)ContctRelation.supplier && a.ContactID == contact.ContactID
                                                  select new
                                                  {
                                                      a.ContactRelationID,
                                                      a.ContactID,
                                                      a.RelationType,
                                                      a.RelationID
                                                  }).FirstOrDefault();



                            ContactRelation Relation = new ContactRelation();
                            Relation.ContactID = contact.ContactID;
                            Relation.RelationType = (long)ContctRelation.supplier;//for supplier
                            Relation.RelationID = SupId;




                            if (ContactReltion != null && ContactReltion.ContactRelationID > 0)
                            {
                                Relation.ContactRelationID = ContactReltion.ContactRelationID;
                                db.Entry(Relation).State = EntityState.Modified;
                                db.SaveChanges();
                                contactRelationids[rowRelationcount] = ContactReltion.ContactRelationID;
                                rowRelationcount++;

                            }
                            else
                            {
                                db.ContactRelation.Add(Relation);
                                db.SaveChanges();
                                contactRelationids[rowRelationcount] = Relation.ContactRelationID;
                                rowRelationcount++;
                            }

                        }


                        //delete
                        if (contactRelationids != null && contactRelationids.Count() > 0)
                        {

                            var results = db.ContactRelation.Where(x => !contactRelationids.Contains(x.ContactRelationID)
                            && x.RelationType == (int)ContctRelation.supplier
                            && x.RelationID == sup.SupplierID
                            );
                            if (results != null && results.Count() > 0)
                            {
                                foreach (var item in results)
                                {
                                    ContactRelation contactRelation = db.ContactRelation.Find(item.ContactRelationID);
                                    if (contactRelation != null)
                                    {
                                        db.ContactRelation.Remove(contactRelation);
                                    }

                                    Contact contact1 = db.Contacts.Find(item.ContactID);
                                    if (contact1 != null)
                                    {
                                        db.Contacts.Remove(contact1);
                                    }
                                }
                                db.SaveChanges();
                            }
                        }
                    }



                    if (supmodel.SuppliDocumentviewmodel != null && supmodel.SuppliDocumentviewmodel.Count > 0)
                    {
                        var fileName = "";
                        int i = 0;

                        long[] CustomerDocumentIds = new long[supmodel.SuppliDocumentviewmodel.Count];
                        int rowcount = 0;

                        foreach (var item in supmodel.SuppliDocumentviewmodel)
                        {
                            if (item.FilePath != null)
                            {
                                // files upload
                                IFormFile file = Request.Form.Files[i];
                                fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                var uploadUrl = LegacyWeb.MapPath("~/uploads/supplierdocuments/");
                                if (!Directory.Exists(uploadUrl))
                                    Directory.CreateDirectory(uploadUrl);
                                file.SaveAs(Path.Combine(uploadUrl, fileName));
                            }
                            else
                            {
                                fileName = item.filenamelead;
                                
                            }

                            i++;


                            CustomerDocument CustomerDocuments = new CustomerDocument
                            {
                                CutomerID = SupId,
                                DocumentTypeID = item.DocumentTypeID,
                                DoucumentType = "supplier",
                                Expiry = item.Expiry,
                                Notes = item.Notes,
                                FilePath = fileName
                            };

                            if (item.DocumnetId > 0)
                            {

                                CustomerDocuments.DocumnetId = item.DocumnetId;
                                db.Entry(CustomerDocuments).State = EntityState.Modified;
                                db.CustomerDocuments.RemoveRange(db.CustomerDocuments.Where(a => a.DocumnetId == CustomerDocuments.DocumnetId));
                                db.SaveChanges();
                            }
                            else
                            {

                                db.CustomerDocuments.Add(CustomerDocuments);
                                db.SaveChanges();
                                item.DocumnetId = CustomerDocuments.DocumnetId;
                            }
                            CustomerDocumentIds[rowcount] = item.DocumnetId;
                            rowcount++;



                        }



                        if (CustomerDocumentIds != null && CustomerDocumentIds.Count() > 0)
                        {

                            var resul = db.CustomerDocuments.Where(x => !CustomerDocumentIds.Contains(x.DocumnetId) && (x.CutomerID == sup.SupplierID));

                            if (resul != null && resul.Count() > 0)
                            {
                                foreach (var item in resul)
                                {
                                    CustomerDocument customerDocuments = db.CustomerDocuments.Find(item.DocumnetId);
                                    db.CustomerDocuments.Remove(customerDocuments);
                                }
                                db.SaveChanges();
                            }
                        }
                    }

                    Accounts account = db.Accountss.Find(sup.Accounts);
                    account.PrintName = supmodel.SupplierName;
                    account.Name = supmodel.SupplierName;
                    account.Alias = supmodel.Alias;
                    account.TRN = supmodel.TaxRegNo;

                    if (supmodel.DC == DC.Debit)
                    {
                        account.OpnBalance = supmodel.OpnBalance;
                        account.OpnBalanceCr = 0;
                    }
                    if (supmodel.DC == DC.Credit)
                    {
                        account.OpnBalance = 0;
                        account.OpnBalanceCr = supmodel.OpnBalance;
                    }


                    db.Entry(account).State = EntityState.Modified;
                    db.SaveChanges();

                    if (supmodel.invoicedata != null)
                    {
                        var purclist = db.PurchaseEntrys.Where(a => a.Supplier == SupId && a.Status == 0).ToList();
                        List<string> bills = new List<string>();
                        foreach (var arr in supmodel.invoicedata)
                        {
                            if (arr.Invoice != null && arr.Amount > 0)
                            {
                                var purchase = db.PurchaseEntrys.Where(a => a.BillNo == arr.Invoice && a.Status == 0).FirstOrDefault();
                                if (purchase != null)
                                {
                                    var purchases = UpdatePurchase(arr, SupId, UserId, Branch);
                                    bills.Add(arr.Invoice);
                                }
                                else
                                {
                                    if (!BillExist(arr.Invoice))
                                    {
                                        var sale = InsertToPurchase(arr, SupId, UserId, Branch);
                                    }
                                }


                            }
                        }

                        //delete other purchase
                        foreach (var plist in purclist)
                        {
                            if (!bills.Contains(plist.BillNo))
                            {
                                PurchaseEntry PEntry = db.PurchaseEntrys.Find(plist.PurchaseEntryId);
                                DeleteBills(PEntry);
                            }
                        }

                    }
                    //    Warning(invoices + " Already exists");


                    bool delete = com.DeleteAllAccountTransaction("Opening Balance", account.AccountsID);
                    // DELETE PEpayments data
                    var PETran = (from a in db.PETransactions
                                  where a.SupplierId == 1 && a.PaymentId == 0
                                  orderby a.PETransactionId
                                  select new
                                  {
                                      a.PurchaseEntry,
                                      a.PEPayAmount
                                  }).ToList();
                    if (PETran.Count > 0)
                    {
                        foreach (var ditem in PETran)
                        {
                            var paying = ditem.PEPayAmount;
                            PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.PurchaseEntry).FirstOrDefault();
                            PEP.PEPaidAmount = PEP.PEPaidAmount - Convert.ToDecimal(paying);
                            db.Entry(PEP).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == id));
                    }

                    // Delete PRpayments data
                    var PRTran = (from a in db.PRTransactions
                                  where a.SupplierId == id && a.Recieptid == 0
                                  orderby a.PRTransactionId
                                  select new
                                  {
                                      a.PurchaseReturnId,
                                      a.PRPayAmount
                                  }).ToList();
                    if (PRTran.Count > 0)
                    {
                        foreach (var ditem in PRTran)
                        {
                            var paying = ditem.PRPayAmount;
                            PRPayment SEP = db.PRPayments.Where(a => a.PurchaseReturnId == ditem.PurchaseReturnId).FirstOrDefault();
                            SEP.PReturnAmount = SEP.PReturnAmount - Convert.ToDecimal(paying);
                            db.Entry(SEP).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == id));
                    }

                    /**************************** Supplier Items ***************************/
                    var SupplierItems = db.SupplierItems.Where(a => a.SupplierId == id);
                    if (SupplierItems != null)
                    {
                        db.SupplierItems.RemoveRange(db.SupplierItems.Where(a => a.SupplierId == id));
                        db.SaveChanges();
                    }

                    if (supmodel.SupplierItems != null)
                    {
                        SupplierItems ItemObj = new SupplierItems();

                        foreach (var arr in supmodel.SupplierItems)
                        {
                            ItemObj.SupplierId = SupId;
                            ItemObj.ItemId = arr;
                            db.SupplierItems.Add(ItemObj);
                            db.SaveChanges();
                        }
                    }

                    /**************************** Supplier Category ***************************/
                    var SupplierCategory = db.SupplierItems.Where(a => a.SupplierId == id);
                    if (SupplierCategory != null)
                    {
                        db.SupplierCategories.RemoveRange(db.SupplierCategories.Where(a => a.SupplierId == id));
                        db.SaveChanges();
                    }

                    if (supmodel.SupplierCategories != null)
                    {
                        SupplierCategories CatObj = new SupplierCategories();

                        foreach (var arr in supmodel.SupplierCategories)
                        {
                            CatObj.SupplierId = SupId;
                            CatObj.CategoryId = arr;
                            db.SupplierCategories.Add(CatObj);
                            db.SaveChanges();
                        }
                    }

                    /**************************** Supplier Brands ***************************/
                    var SupplierBrands = db.SupplierItems.Where(a => a.SupplierId == id);
                    if (SupplierBrands != null)
                    {
                        db.SupplierBrands.RemoveRange(db.SupplierBrands.Where(a => a.SupplierId == id));
                        db.SaveChanges();
                    }

                    if (supmodel.SupplierBrands != null)
                    {
                        SupplierBrands BrandObj = new SupplierBrands();

                        foreach (var arr in supmodel.SupplierBrands)
                        {
                            BrandObj.SupplierId = SupId;
                            BrandObj.BrandId = arr;
                            db.SupplierBrands.Add(BrandObj);
                            db.SaveChanges();
                        }
                    }

                    /*****************************************************************/
                    if (supmodel.OpnBalance > 0)
                    {
                       
                        DateTime opdate = DateTime.Parse("01-01-2010", new CultureInfo("en-GB"));
                        if (supmodel.DC == DC.Debit)
                        {
                            com.addAccountTrasaction(supmodel.OpnBalance, 0, account.AccountsID, "Opening Balance", account.AccountsID, DC.Debit, opdate);

                        }
                        if (supmodel.DC == DC.Credit)
                        {
                            com.addAccountTrasaction(0, supmodel.OpnBalance, account.AccountsID, "Opening Balance", account.AccountsID, DC.Credit, opdate);
                        }
                    }

                    com.addlog(LogTypes.Updated, UserId, "Supplier", "Suppliers", findip(), sup.SupplierID, "Successfully Updated Supplier details");


                    Success("Successfully Updated Supplier Details.", true);
                    return RedirectToAction("Edit/" + id, "Supplier");

                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form..", true);
                return View();
            }
        }

        //GET: Supplier/Delete/5
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete Supplier")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Suppliers");
            var UserId = User.Identity.GetUserId();

            var Sup1 = (from a in db.Suppliers
                        join b in db.Accountss on a.Accounts equals b.AccountsID
                        where a.SupplierID == id && (userpermission == true || b.CreatedBy == UserId)
                        select new
                        {
                            SupplierID = a.SupplierID,
                        }).FirstOrDefault();

            if (Sup1 == null)
            {
                return NotFound();
            }
            else
            {
                Supplier Sup = db.Suppliers.Find(id);
                return PartialView(Sup);
            }
        }

        // POST: customer/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Supplier")]
        public ActionResult DeleteAction(long id)
        {
            bool stat = false;
            string msg;
            #region Old Code
            //    || db.PurchaseOrders.Any(c => c.Supplier == id)
            //    || db.Payments.Any(x => x.PayFrom == sup.Accounts) || db.Payments.Any(x => x.PayTo == sup.Accounts)
            //    || db.Receipts.Any(x => x.PayFrom == sup.Accounts) || db.Receipts.Any(x => x.PayTo == sup.Accounts)
            //    || db.Journals.Any(x => x.PayFrom == sup.Accounts) || db.Journals.Any(x => x.PayTo == sup.Accounts)                
            #endregion
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Supplier Deleted Successfully";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Supplier")]
        public ActionResult DeleteAllSupplier(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSupp(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Supplier, Unable to Delete " + notdel + " Supplier. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Supplier.", true);
            }
            else
            {
                Success("Deleted " + count + " Supplier.", true);
            }
            return RedirectToAction("Index", "Supplier");
        }

        private Boolean DeleteSupp(long supid)
        {
            var Msg = chkDeleteWithMsg(supid);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(supid);
            }
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            Supplier sup = db.Suppliers.Find(id);
            if (db.PurchaseEntrys.Any(c => c.Supplier == id))
            {
                msg = "Supplier Already used in PurchaseEntry !!";
            }
            else if (db.PurchaseReturns.Any(c => c.Supplier == id))
            {
                msg = "Supplier Already used in Purchase Return !!";
            }
            else if (db.PurchaseOrders.Any(c => c.Supplier == id))
            {
                msg = "Supplier Already used in Purchase Order !!";
            }
            else if (db.Payments.Any(x => x.PayFrom == sup.Accounts) || db.Payments.Any(x => x.PayTo == sup.Accounts))
            {
                msg = "Supplier Already used in Payments !!";
            }
            else if (db.Receipts.Any(x => x.PayFrom == sup.Accounts) || db.Receipts.Any(x => x.PayTo == sup.Accounts))
            {
                msg = "Supplier Already used in Receipt !!";
            }
            else if (db.Journals.Any(x => x.PayFrom == sup.Accounts) || db.Journals.Any(x => x.PayTo == sup.Accounts))
            {
                msg = "Supplier Already used in Journal !!";
            }
            else if (db.Items.Any(x => x.Supplier == id))
            {
                msg = "Supplier Already used in Items !!";
            }
            else if (db.PurchaseQuotations.Any(x => x.Supplier == id))
            {
                msg = "Supplier Already used in Purchase Quotation !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }
        public bool DeleteFn(long supid)
        {
            Supplier sup = db.Suppliers.Find(supid);
            Contact con = db.Contacts.Find(sup.Contact);
            Accounts acc = db.Accountss.Find(sup.Accounts);

            if (con != null)
            {
                db.Contacts.RemoveRange(db.Contacts.Where(a => a.ContactID == sup.Contact));
            }

            if (acc != null)
            {
                db.Accountss.RemoveRange(db.Accountss.Where(a => a.AccountsID == sup.Accounts));
            }


            //***********Delete from table SupplierItems
            var SupplierItems = db.SupplierItems.Where(a => a.SupplierId == supid);
            if (SupplierItems != null)
            {
                db.SupplierItems.RemoveRange(db.SupplierItems.Where(a => a.SupplierId == supid));
                db.SaveChanges();
            }

            //***********Delete from table SupplierCategory
            var SupplierCategories = db.SupplierCategories.Where(a => a.SupplierId == supid);
            if (SupplierCategories != null)
            {
                db.SupplierCategories.RemoveRange(db.SupplierCategories.Where(a => a.SupplierId == supid));
                db.SaveChanges();
            }

            //***********Delete from table SupplierBrands
            var SupplierBrands = db.SupplierItems.Where(a => a.SupplierId == supid);
            if (SupplierBrands != null)
            {
                db.SupplierBrands.RemoveRange(db.SupplierBrands.Where(a => a.SupplierId == supid));
                db.SaveChanges();
            }

            if (sup != null)
            {
                db.Suppliers.RemoveRange(db.Suppliers.Where(a => a.SupplierID == supid));
            }
            db.SaveChanges();

            bool delete = com.DeleteAllAccountTransaction("Opening Balance", acc.AccountsID);

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Supplier", "Suppliers", findip(), sup.SupplierID, "Successfully Deleted Supplier details");

            return true;
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Supplier")]
        public ActionResult ViewDetails(int? id)
        {
            SupplierSubmitViewModel suppmodel = new SupplierSubmitViewModel();
            suppmodel = (from a in db.Suppliers
                         join b in db.Contacts on a.Contact equals b.ContactID into tmp
                         from b in tmp.DefaultIfEmpty()
                         join e in db.Accountss on a.Accounts equals e.AccountsID
                         where a.SupplierID == id
                         select new SupplierSubmitViewModel
                         {
                             SupplierName = a.SupplierName,
                             SupplierCode = a.SupplierCode,
                             CreditLimit = a.CreditLimit,
                             CreditPeriod = a.CreditPeriod,
                             Remark = a.Remark,

                             TaxRegNo = e.TRN,

                             BankName = a.BankName,
                             AccountNo = a.AccountNo,
                             BranchName = a.BranchName,
                             IbanNo = a.IbanNo,
                             Swift = a.Swift,

                             Address = b.Address,
                             City = b.City,
                             State = b.State,
                             Country = b.Country,
                             Zip = b.Zip,
                             Phone = b.Phone,
                             //Mobile = b.Mobile,
                             Fax = b.Fax,
                             EmailId = b.EmailId,
                             Reference = b.Reference,
                             ContactPerson = b.ContactPerson,
                             OpnBalance = e.OpnBalance != 0 ? e.OpnBalance : e.OpnBalanceCr,
                             mobmodel = (from ac in db.Mobiles
                                         where (ac.Contact == a.Contact)
                                         select new MobileViewModel
                                         {
                                             Num = ac.MobileNum,
                                             Name = ac.Name
                                         }).ToList(),
                             Alias = e.Alias
                         }).FirstOrDefault();

            return View(suppmodel);

        }
        public JsonResult SearchSupplier(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            var userpermission = User.IsInRole("All Suppliers");
            var UserId = User.Identity.GetUserId();

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Suppliers
                                 
                                  join d in db.Accountss on b.Accounts equals d.AccountsID
                                  
                                  where (d.Alias.ToLower().Contains(q.ToLower()) || b.SupplierName.ToLower().Contains(q.ToLower()) || b.SupplierCode.ToLower().Contains(q.ToLower()) || d.Alias.Contains(q) || b.SupplierName.Contains(q) || b.SupplierCode.Contains(q) )
                                  && (userpermission == true || d.CreatedBy == UserId)
                                  && b.Status !=0
                                  select new SelectFormat
                                  {
                                      text = b.SupplierCode + "-" + b.SupplierName,
                                      id = b.SupplierID
                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Suppliers
                                  
                                  join d in db.Accountss on b.Accounts equals d.AccountsID
                                  where (userpermission == true || d.CreatedBy == UserId)
                                  && b.Status != 0
                                  select new SelectFormat
                                  {
                                      text = b.SupplierCode + "-" + b.SupplierName, //each json object will have 
                                      id = b.SupplierID
                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Supplier" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchSupplier2(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            var userpermission = User.IsInRole("All Suppliers");
            var UserId = User.Identity.GetUserId();

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Suppliers
                                  join c in db.Contacts on b.Contact equals c.ContactID into cnts
                                  from c in cnts.DefaultIfEmpty()
                                  join d in db.Accountss on b.Accounts equals d.AccountsID
                                  join e in db.Mobiles on c.ContactID equals e.Contact into mobi
                                  from e in mobi.DefaultIfEmpty()
                                  where (d.Alias.ToLower().Contains(q.ToLower()) || b.SupplierName.ToLower().Contains(q.ToLower()) || b.SupplierCode.ToLower().Contains(q.ToLower()) || d.Alias.Contains(q) || b.SupplierName.Contains(q) || b.SupplierCode.Contains(q) || c.Phone.Replace(" ", "").Contains(q) || e.MobileNum.Replace(" ", "").Contains(q))
                                 && b.Status != 0
                                  select new SelectFormat
                                  {
                                      text = b.SupplierCode + "-" + b.SupplierName,
                                      id = b.SupplierID
                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Suppliers
                                  join c in db.Contacts on b.Contact equals c.ContactID into cnts
                                  from c in cnts.DefaultIfEmpty()
                                  join d in db.Accountss on b.Accounts equals d.AccountsID
                                  where   b.Status != 0
                                  select new SelectFormat
                                  {
                                      text = b.SupplierCode + "-" + b.SupplierName, //each json object will have 
                                      id = b.SupplierID
                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Supplier" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create Supplier")]
        public ActionResult AddSupplier()
        {
            var viewModel = new SupplierSubmitViewModel
            {
                SupplierCode = SuppCode(),
            };
            return PartialView(viewModel);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Supplier")]
        public JsonResult AddSupplier(SupplierSubmitViewModel supmodel)
        {
            bool stat = false;
            string msg;
            var supExists = db.Suppliers.Any(u => u.SupplierCode == supmodel.SupplierCode);
            if (supExists)
            {
                msg = "A Supplier with same Supplier Code exists.";
                stat = false;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Int64 contactId = 0;
                    Int64 accountId = 0;
                    Int64 SuppId = 0;

                    Accounts account = new Accounts();
                    account.Name = supmodel.SupplierName;
                    account.Alias = supmodel.Alias;
                    account.PrintName = supmodel.SupplierName;
                    account.Group = 14;
                    account.Status = Status.active;
                    account.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    account.TRN = supmodel.TaxRegNo;
                    if (supmodel.DC == DC.Debit)
                    {
                        account.OpnBalance = supmodel.OpnBalance;
                        account.OpnBalanceCr = 0;
                    }
                    if (supmodel.DC == DC.Credit)
                    {
                        account.OpnBalance = 0;
                        account.OpnBalanceCr = supmodel.OpnBalance;
                    }

                    db.Accountss.Add(account);
                    db.SaveChanges();
                    accountId = account.AccountsID;

                    Supplier sup = new Supplier
                    {
                        Contact = contactId,
                        Accounts = accountId,
                        SupplierName = supmodel.SupplierName,
                        SupplierCode = supmodel.SupplierCode,
                        //TaxRegNo = supmodel.TaxRegNo,

                        CreditLimit = supmodel.CreditLimit != null ? (decimal)supmodel.CreditLimit : 0,
                        CreditPeriod = supmodel.CreditPeriod != null ? (int)supmodel.CreditPeriod : 0,
                        Remark = supmodel.Remark,
                        BankName = supmodel.BankName,
                        AccountNo = supmodel.AccountNo,
                        BranchName = supmodel.BranchName,
                        IbanNo = supmodel.IbanNo,
                        Swift = supmodel.Swift,
                        TaxType = supmodel.TaxType,
                        Addres = supmodel.Addres
                    };
                    db.Suppliers.Add(sup);
                    SuppId = sup.SupplierID;
                    db.SaveChanges();
                    if (supmodel.LstContacts != null && supmodel.LstContacts.Count > 0)
                    {

                        foreach (var item in supmodel.LstContacts)
                        {
                            var contact = new Contact
                            {

                                Address = item.Name,
                                City = supmodel.City,
                                State = supmodel.State,
                                Country = item.Country,
                                Zip = supmodel.Zip,
                                FirstName = item.FirstName,
                                LastName = item.LastName,
                                Name = item.FirstName + " " + item.LastName,

                                TypeOfContact = item.TypeOfContact,
                                Mobile = item.Mobile,
                                Phone = item.Phone,
                                Fax = supmodel.Fax,
                                EmailId = item.EmailId,
                                Reference = supmodel.Reference,
                                ContactPerson = supmodel.ContactPerson,
                                Website = item.Website,
                                Group = 2,
                                Status = Status.active,
                                CountryID = item.CountryID,
                                ContactTypeID = item.ContactTypeID

                            };
                            db.Contacts.Add(contact);
                            db.SaveChanges();
                            SuppId = sup.SupplierID;
                            contactId = contact.ContactID;
                            var mob = new Mobile
                            {
                                Contact = contactId,
                                MobileNum = item.Mobile,
                                Name = item.FirstName + " " + item.LastName
                            };
                            db.Mobiles.Add(mob);
                            db.SaveChanges();


                            ContactRelation Relation = new ContactRelation();
                            Relation.ContactID = contactId;
                            Relation.RelationType = (int)ContctRelation.supplier;//for customer
                            Relation.RelationID = SuppId;
                            db.ContactRelation.Add(Relation);
                            db.SaveChanges();

                        }
                    }


                    if (supmodel.OpnBalance > 0)
                    {
                        if (supmodel.DC == DC.Debit)
                        {
                            com.addAccountTrasaction(supmodel.OpnBalance, 0, account.AccountsID, "Opening Balance", account.AccountsID, DC.Debit);

                        }
                        if (supmodel.DC == DC.Credit)
                        {
                            com.addAccountTrasaction(0, supmodel.OpnBalance, account.AccountsID, "Opening Balance", account.AccountsID, DC.Credit);
                        }
                    }


                    var UserId = User.Identity.GetUserId();


                    msg = "Successfully added Supplier details.";
                    stat = true;
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                }

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public JsonResult getcontactsbymobile(string mobile)
        {
            var serialisedJson = (from c in db.Contacts
                                  join j in db.Mobiles on c.ContactID equals j.Contact into mobi
                                  from j in mobi.DefaultIfEmpty()
                                  where (j.MobileNum.Contains(mobile))
                                  select new
                                  {
                                      c.FirstName,
                                      c.ContactID,
                                      c.LastName,
                                      c.Name,
                                      c.Address,
                                      j.MobileNum,
                                      c.EmailId,
                                      c.Website,
                                      c.CountryID,
                                      c.ContactTypeID
                                  }).ToList();
            return Json(serialisedJson);

        }

        [HttpGet]
        public JsonResult GetSupplierEmailById(int SuppId)
        {
            var email = (from a in db.Suppliers
                         join b in db.Contacts on a.Contact equals b.ContactID into cont
                         from b in cont.DefaultIfEmpty()
                         where a.SupplierID == SuppId
                         select new
                         {
                             b.EmailId,
                         }).FirstOrDefault();
            return Json(email);

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Supplier")]
        public ActionResult UpdateSupplier(SupplierSubmitViewModel supmodel)
        {
            bool stat = false;
            string msg;
            var id = supmodel.SupplierID;
            var UserId = User.Identity.GetUserId();
            var Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            if (ModelState.IsValid)
            {
                var CodeExists = db.Suppliers.Any(u => u.SupplierCode == supmodel.SupplierCode && u.SupplierID != id);
                if (CodeExists)
                {
                    msg = "A Supplier with same Supplier code exists.";
                    stat = false;
                }
                else
                {
                    Supplier sup = db.Suppliers.Find(id);
                    sup.SupplierName = supmodel.SupplierName;
                    sup.SupplierCode = supmodel.SupplierCode;
                    sup.CreditLimit = supmodel.CreditLimit != null ? (decimal)supmodel.CreditLimit : 0;
                    sup.CreditPeriod = supmodel.CreditPeriod != null ? (int)supmodel.CreditPeriod : 0;
                    sup.Remark = supmodel.Remark;
                    sup.BankName = supmodel.BankName;
                    sup.AccountNo = supmodel.AccountNo;
                    sup.BranchName = supmodel.BranchName;
                    sup.IbanNo = supmodel.IbanNo;
                    sup.Swift = supmodel.Swift;
                    sup.TaxType = supmodel.TaxType;
                    sup.Addres = supmodel.Addres;




                    db.Entry(sup).State = EntityState.Modified;
                    db.SaveChanges();


                    Int64 SupId = sup.SupplierID;
                    Contact cont = db.Contacts.Find(sup.Contact);
                    cont.Name = supmodel.SupplierName;
                    cont.Address = supmodel.Address;
                    cont.City = supmodel.City;
                    cont.State = supmodel.State;
                    cont.Country = supmodel.Country;
                    cont.Zip = supmodel.Zip;
                    cont.Phone = supmodel.Phone;
                    cont.Mobile = supmodel.Mobile;
                    cont.Fax = supmodel.Fax;
                    cont.EmailId = supmodel.EmailId;
                    cont.Reference = supmodel.Reference;
                    cont.ContactPerson = supmodel.ContactPerson;
                    cont.SalesPMob = supmodel.SalesPMob;

                    db.Entry(cont).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == sup.Contact));
                    db.SaveChanges();
                    if (supmodel.mobmodel != null)
                    {
                        foreach (var arr in supmodel.mobmodel)
                        {
                            if (arr.Num != null)
                            {
                                var mob = new Mobile
                                {
                                    Contact = sup.Contact,
                                    MobileNum = arr.Num,
                                    Name = arr.Name
                                };
                                db.Mobiles.Add(mob);
                                db.SaveChanges();
                            }
                        }
                    }
                    Accounts account = db.Accountss.Find(sup.Accounts);
                    account.PrintName = supmodel.SupplierName;
                    account.Name = supmodel.SupplierName;
                    account.Alias = supmodel.Alias;
                    account.TRN = supmodel.TaxRegNo;
                    if (supmodel.DC == DC.Debit)
                    {
                        account.OpnBalance = supmodel.OpnBalance;
                        account.OpnBalanceCr = 0;
                    }
                    if (supmodel.DC == DC.Credit)
                    {
                        account.OpnBalance = 0;
                        account.OpnBalanceCr = supmodel.OpnBalance;
                    }

                    db.Entry(account).State = EntityState.Modified;
                    db.SaveChanges();

                    if (supmodel.invoicedata != null)
                    {
                        List<string> bills = new List<string>();
                        foreach (var arr in supmodel.invoicedata)
                        {
                            if (arr.Invoice != null && arr.Amount > 0)
                            {
                                var purchase = db.PurchaseEntrys.Where(a => a.BillNo == arr.Invoice && a.Status == 0).FirstOrDefault();
                                if (purchase != null)
                                {

                                    var purchases = UpdatePurchase(arr, SupId, UserId, Branch);
                                    bills.Add(arr.Invoice);
                                }
                                else
                                {
                                    if (!BillExist(arr.Invoice))
                                    {
                                        var purchas = InsertToPurchase(arr, SupId, UserId, Branch);
                                    }
                                }
                            }
                        }

                        //delete other sales
                        var purclist = db.PurchaseEntrys.Where(a => a.Supplier == SupId && a.Status == 0).ToList();
                        foreach (var plist in purclist)
                        {
                            if (!bills.Contains(plist.BillNo))
                            {
                                PurchaseEntry PEntry = db.PurchaseEntrys.Find(plist.PurchaseEntryId);
                                DeleteBills(PEntry);
                            }
                        }

                    }
                    //    Warning(invoices + " Already exists");

                    bool delete = com.DeleteAllAccountTransaction("Opening Balance", account.AccountsID);
                    // DELETE PEpayments data
                    var PETran = (from a in db.PETransactions
                                  where a.SupplierId == 1 && a.PaymentId == 0
                                  orderby a.PETransactionId
                                  select new
                                  {
                                      a.PurchaseEntry,
                                      a.PEPayAmount
                                  }).ToList();
                    if (PETran.Count > 0)
                    {
                        foreach (var ditem in PETran)
                        {
                            var paying = ditem.PEPayAmount;
                            PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.PurchaseEntry).FirstOrDefault();
                            PEP.PEPaidAmount = PEP.PEPaidAmount - Convert.ToDecimal(paying);
                            db.Entry(PEP).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == id));
                    }

                    // Delete PRpayments data
                    var PRTran = (from a in db.PRTransactions
                                  where a.SupplierId == id && a.Recieptid == 0
                                  orderby a.PRTransactionId
                                  select new
                                  {
                                      a.PurchaseReturnId,
                                      a.PRPayAmount
                                  }).ToList();
                    if (PRTran.Count > 0)
                    {
                        foreach (var ditem in PRTran)
                        {
                            var paying = ditem.PRPayAmount;
                            PRPayment SEP = db.PRPayments.Where(a => a.PurchaseReturnId == ditem.PurchaseReturnId).FirstOrDefault();
                            SEP.PReturnAmount = SEP.PReturnAmount - Convert.ToDecimal(paying);
                            db.Entry(SEP).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == id));
                    }
                    if (supmodel.OpnBalance > 0)
                    {
                        if (supmodel.DC == DC.Debit)
                        {
                            com.addAccountTrasaction(supmodel.OpnBalance, 0, account.AccountsID, "Opening Balance", account.AccountsID, DC.Debit);

                        }
                        if (supmodel.DC == DC.Credit)
                        {
                            com.addAccountTrasaction(0, supmodel.OpnBalance, account.AccountsID, "Opening Balance", account.AccountsID, DC.Credit);
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "Supplier", "Suppliers", findip(), sup.SupplierID, "Successfully Updated Supplier details");

                    msg = "Successfully updated Supplier details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public void DeleteBills(PurchaseEntry PEntry)
        {
            var PEP = db.PEPayments.Where(a => a.PurchaseEntry == PEntry.PurchaseEntryId).FirstOrDefault();
            if (PEP != null)
            {
                db.PEPayments.RemoveRange(db.PEPayments.Where(a => a.PurchaseEntry == PEntry.PurchaseEntryId));
            }

            var rec = db.Payments.Where(a => a.Reference == PEntry.PurchaseEntryId && a.RefType == "Purchase").FirstOrDefault();
            if (rec != null)
            {
                db.Payments.RemoveRange(db.Payments.Where(a => a.Reference == PEntry.PurchaseEntryId && a.RefType == "Purchase"));
            }

            var paybill = db.PaymentBills.Where(a => a.InvoiceNo == PEntry.PurchaseEntryId && a.BillType == "Purchase" && a.Type == "Against Reference").ToList();
            if (paybill != null)
            {
                var paybillz = db.PaymentBills.Where(a => a.InvoiceNo == PEntry.PurchaseEntryId && a.BillType == "Purchase" && a.Type == "Against Reference").ToList();
                paybillz.ForEach(a =>
                {
                    a.Type = "New Reference";
                    a.BillType = null;
                    a.InvoiceNo = null;
                });
                db.SaveChanges();
            }
            db.PurchaseEntrys.Remove(PEntry);
            db.SaveChanges();
        }

        [HttpGet]
        public JsonResult GetSupplierAccRef(int EntryID)
        {
            var data = (from a in db.PurchaseEntrys
                        join b in db.Suppliers on a.Supplier equals b.SupplierID into acc
                        from b in acc.DefaultIfEmpty()
                        join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                        where b.SupplierID == EntryID && a.Status == 0
                        select new
                        {
                            b.Accounts,
                            a.BillNo,
                            Amount = c.PEBillAmount,
                            Paid = c.PEPaidAmount,
                            Date = a.PEDate,
                            Type = "Purchase"
                        }).ToList();
            return Json(data);
        }

        public Boolean UpdatePurchase(ReferenceAccountViewModel arr, long SupId, string UserId, long branch)
        {
            var purchase = db.PurchaseEntrys.Where(a => a.BillNo == arr.Invoice && a.Status == 0).FirstOrDefault();
            PEPayment PEpay = db.PEPayments.Where(a => a.PurchaseEntry == purchase.PurchaseEntryId).FirstOrDefault();

            PEpay.PEBillAmount = (decimal)arr.Amount;
            db.Entry(PEpay).State = EntityState.Modified;
            db.SaveChanges();
            return true;
        }

        public JsonResult SearchMobile(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Suppliers
                                  join c in db.Contacts on b.Contact equals c.ContactID into cus
                                  from c in cus.DefaultIfEmpty()
                                  join d in db.Mobiles on b.Contact equals d.Contact into mobi
                                  from d in mobi.DefaultIfEmpty()
                                  where (d.MobileNum.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = d.MobileNum, //each json object will have 
                                      id = c.ContactID
                                  })
                                  .OrderBy(b => b.text).ToList();

            }
            else
            {
                serialisedJson = (from b in db.Suppliers
                                  join c in db.Contacts on b.Contact equals c.ContactID into cus
                                  from c in cus.DefaultIfEmpty()
                                  join d in db.Mobiles on b.Contact equals d.Contact into mobi
                                  from d in mobi.DefaultIfEmpty()
                                  select new SelectFormat
                                  {
                                      text = d.MobileNum, //each json object will have 
                                      id = c.ContactID
                                  })
                                  .OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult SearchPhone(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Suppliers
                                  join c in db.Contacts on b.Contact equals c.ContactID into cus
                                  from c in cus.DefaultIfEmpty()
                                  where (c.Phone.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = c.Phone, //each json object will have 
                                      id = c.ContactID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Suppliers
                                  join c in db.Contacts on b.Contact equals c.ContactID into cus
                                  from c in cus.DefaultIfEmpty()
                                  select new SelectFormat
                                  {
                                      text = c.Phone, //each json object will have 
                                      id = c.ContactID
                                  })
                                  .OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Supplier")]
        public ActionResult Details(int? id)
        {
            ViewBag.Prjct = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                              }, "Value", "Text", 1);

            SupplierSubmitViewModel suppmodel = new SupplierSubmitViewModel();

            suppmodel = (from a in db.Suppliers
                         join b in db.Contacts on a.Contact equals b.ContactID into tmp
                         from b in tmp.DefaultIfEmpty()
                         join e in db.Accountss on a.Accounts equals e.AccountsID
                         join d in db.Mobiles on b.ContactID equals d.Contact into mobi
                         from d in mobi.DefaultIfEmpty()
                         where a.SupplierID == id
                         select new SupplierSubmitViewModel
                         {
                             SupplierID = a.SupplierID,
                             SupplierName = a.SupplierName,
                             SupplierCode = a.SupplierCode,
                             CreditLimit = a.CreditLimit,
                             CreditPeriod = a.CreditPeriod,
                             Remark = a.Remark,


                             TaxRegNo = e.TRN,

                             //Lattitude = a.Lattitude,
                             //Longitude = a.Longitude,

                             BankName = a.BankName,
                             AccountsID = e.AccountsID,
                             AccountNo = a.AccountNo,
                             BranchName = a.BranchName,
                             IbanNo = a.IbanNo,
                             Swift = a.Swift,
                             TaxType = a.TaxType,
                             Addres = a.Addres,
                             Address = b.Address,
                             City = b.City,
                             State = b.State,
                             Country = b.Country,
                             Zip = b.Zip,
                             Phone = b.Phone,
                             //Mobile = b.Mobile,
                             Fax = b.Fax,
                             EmailId = b.EmailId,
                             Reference = b.Reference,
                             ContactPerson = b.ContactPerson,
                             OpnBalance = e.OpnBalance,
                             DCname = e.OpnBalanceCr == 0 ? "Dr." : "Cr.",
                             mobmodel = (from ac in db.Mobiles
                                         where (ac.Contact == a.Contact)
                                         select new MobileViewModel
                                         {
                                             Num = ac.MobileNum,
                                             Name = ac.Name
                                         }).ToList(),
                             Alias = e.Alias
                         }).FirstOrDefault();

            return View(suppmodel);

        }

        [HttpGet]
        public JsonResult SupplierCheck(string Supplier, long? supid)
        {
            var SuppCheck = db.Suppliers.Where(x => x.SupplierName == Supplier).Any();
            var rslt = false;
            if (SuppCheck == true)
            {
                if (supid != 0)
                {
                    var supp = db.Suppliers.Where(x => x.SupplierName == Supplier).FirstOrDefault();
                    SuppCheck = (supp.SupplierID == supid) ? false : true;
                }
            }
            rslt = (SuppCheck) ? true : false;
            return Json(rslt);
        }

        [HttpGet]
        public JsonResult GetMobile(long CnId)
        {
            var ConD = (from a in db.Mobiles
                        join b in db.Suppliers on a.Contact equals b.Contact
                        where b.SupplierID == CnId
                        select new
                        {
                            Mob = a.MobileNum,
                            Name = a.Name
                        }).ToList();
            return Json(ConD);
        }

        public ActionResult BulkUpload()
        {
            var viewModel = new SupplierSubmitViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public JsonResult BulkUploadSupplier(string[][] array)
        {
            var UserId = User.Identity.GetUserId();

            var Msg = "";
            bool Stat = false;

            foreach (var arr in array)
            {
                var name = arr[0];
                var namchk = db.Suppliers.Where(x => x.SupplierName == name).Any();
                if (!namchk)
                {
                    long accountId = 0;
                    long contactId = 0;

                    DC DebCre = new DC();
                    var opbal = ((arr[21]) == "") ? 0 : Convert.ToDecimal(arr[21]);

                    //Data Analysing Section for Prefix Category Brand Tax and Unit
                    var Name = (arr[0] == "") ? null : arr[0];

                    if (Name != null)
                    {

                        Accounts account = new Accounts();
                        account.Name = arr[0];
                        account.Alias = arr[1];
                        account.PrintName = arr[0];
                        account.Group = 14;
                        account.Status = Status.active;
                        account.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        account.CreatedBy = UserId;
                        account.TRN = arr[2];

                        DebCre = arr[22] == "Debit" ? DC.Debit : DC.Credit;
                        if (DebCre == DC.Debit)
                        {
                            account.OpnBalance = opbal;
                            account.OpnBalanceCr = 0;
                        }
                        if (DebCre == DC.Credit)
                        {
                            account.OpnBalance = 0;
                            account.OpnBalanceCr = opbal;
                        }

                        db.Accountss.Add(account);
                        db.SaveChanges();
                        accountId = account.AccountsID;

                        Contact cont = new Contact();
                        cont.Name = arr[0];
                        cont.Address = arr[6];
                        cont.City = arr[10];
                        cont.State = arr[11];
                        cont.Country = arr[11];
                        cont.Zip = arr[12];
                        cont.Phone = arr[7];
                        //Mobile = cumodel.Mobile,
                        cont.Fax = arr[13];
                        cont.EmailId = (arr[9] == "") ? null : arr[9];
                        cont.Reference = arr[14];
                        cont.ContactPerson = arr[15];
                        cont.Group = 3;
                        cont.Status = Status.active;
                        db.Contacts.Add(cont);
                        db.SaveChanges();
                        contactId = cont.ContactID;
                        if (arr[8] != null)
                        {
                            string s = arr[8];
                            var nums = s.Split(','); //Array.ConvertAll(s.Split(','), string);
                            foreach (var nu in nums)
                            {
                                if (nu != "null" && nu != null)
                                {
                                    var mobchk = db.Mobiles.Where(x => x.MobileNum == nu && x.Contact == contactId).Any();
                                    if (!mobchk)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = contactId,
                                            MobileNum = nu,
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }

                                }
                            }
                        }
                        Supplier sup = new Supplier
                        {
                            Contact = contactId,
                            Accounts = accountId,

                            SupplierName = arr[0],
                            SupplierCode = arr[0],
                            CreditLimit = (arr[3] == "") ? 0 : Convert.ToDecimal(arr[3]),
                            CreditPeriod = (arr[4] == "") ? 0 : Convert.ToInt32(arr[4]),
                            BankName = arr[16],
                            AccountNo = arr[17],
                            IbanNo = arr[18],
                            BranchName = arr[19],
                            Swift = arr[20],

                        };
                        db.Suppliers.Add(sup);
                        db.SaveChanges();

                        if (opbal > 0)
                        {
                            if (opbal != 0 && DebCre == DC.Debit)
                            {
                                com.addAccountTrasaction(opbal, 0, accountId, "Opening Balance", accountId, DC.Debit);
                            }
                            if (opbal != 0 && DebCre == DC.Credit)
                            {
                                com.addAccountTrasaction(0, opbal, accountId, "Opening Balance", accountId, DC.Credit);
                            }
                        }
                        Msg = "Success";
                        Stat = true;
                    }
                    else
                    {
                        Msg = "Name Already exists";
                        Stat = false;
                    }

                }

            }


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = Stat, message = Msg } };
        }


        [HttpGet]
        public virtual ActionResult DownloadExcel(string file)
        {
            string fullPath = "";
            string fileName = "";

            fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/Supplier.xlsx"));
            fileName = "GeneralItem.xlsx";


            return File(fullPath, "application/vnd.ms-excel", fileName);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        string msg;





    }

}



