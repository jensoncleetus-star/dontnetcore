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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
namespace QuickSoft.Controllers
{ 
     [QkAuthorize]
public class CustomerRelationController : BaseController
    {
        ApplicationDbContext db;
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        // GET: CustomerRelation
        public CustomerRelationController()
        {
            db = new ApplicationDbContext();
            UserManager = LegacyIdentity.UserManager(db);
            RoleManager = LegacyIdentity.RoleManager(db);
        }
        [RedirectingAction]
        //    [
        public ActionResult Index()
        {
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var lastdate = today.AddMonths(-1);
            ViewBag.today = today.ToString("dd-MM-yyyy");
            ViewBag.lastdate = lastdate.ToString("dd-MM-yyyy");
            Common com = new Common();
            var Balance = com.Accbalance(3);
            HomeViewModel vmodel = new HomeViewModel();

            var userpermissionSale = User.IsInRole("All Sales Entry");
            var userpermissionPurchase = User.IsInRole("All Purchase Entry");
            var userpermissionQuotation = User.IsInRole("All Quotation Entry");
            var userpermissionSalereturn = User.IsInRole("All Sales Return Entry");
            var userpermissionPurchasereturn = User.IsInRole("All Purchase Return Entry");
            var userpermissionPayment = User.IsInRole("All Payment Entry");
            var userpermissionReceipt = User.IsInRole("All Receipt Entry");
            var allCustomer = User.IsInRole("All Customers");
            var allSupplier = User.IsInRole("All Suppliers");

            vmodel.totCustomerCount = Convert.ToString(db.Customers.Where(x => x.Type == CRMCustomerType.Customer && (allCustomer == true || x.AccountID.CreatedBy == UserId)).Count());
            vmodel.totSupplierCount = Convert.ToString(db.Suppliers.Where(x => (allCustomer == true || x.AccountID.CreatedBy == UserId)).Count());
            vmodel.totUsersCount = Convert.ToString(db.Users.Count());
            vmodel.totSalesExecCount = Convert.ToString(db.Employees.Count());
            vmodel.cashinhand = Balance["amount"] + " " + Balance["type"];


            vmodel.totSaleEntryCount = Convert.ToString(db.SalesEntrys.Where(a => (userpermissionSale == true || a.CreatedBy == UserId)).Count());

            vmodel.totQuotCount = Convert.ToString(db.Quotations.Where(a => (userpermissionQuotation == true || a.CreatedUserId == UserId)).Count());

            vmodel.totPurchaseEntryCount = Convert.ToString(db.PurchaseEntrys.Where(a => (userpermissionPurchase == true || a.CreatedBy == UserId)).Count());

            vmodel.totSalesReturnCount = Convert.ToString(db.SalesReturns.Where(a => (userpermissionPurchase == true || a.CreatedBy == UserId)).Count());

            vmodel.totPurchaseReturnCount = Convert.ToString(db.PurchaseReturns.Where(a => (userpermissionPurchasereturn == true || a.CreatedBy == UserId)).Count());

            vmodel.todCustomerCount = Convert.ToString(db.Customers.Where(n => n.Type == CRMCustomerType.Customer && (allCustomer == true || n.AccountID.CreatedBy == UserId) && EF.Functions.DateDiffDay(n.AccountID.CreatedDate, lastdate) <= 0 && EF.Functions.DateDiffDay(n.AccountID.CreatedDate, today) >= 0).Count());

            vmodel.todVendorCount = Convert.ToString(db.Suppliers.Where(n => (allSupplier == true || n.AccountID.CreatedBy == UserId) && EF.Functions.DateDiffDay(n.AccountID.CreatedDate, lastdate) <= 0 && EF.Functions.DateDiffDay(n.AccountID.CreatedDate, today) >= 0).Count());

            vmodel.todQuotationCount = Convert.ToString(db.Quotations.Where(n => (EF.Functions.DateDiffDay(n.QuotCreatedDate, lastdate) <= 0 && EF.Functions.DateDiffDay(n.QuotCreatedDate, today) >= 0) && (userpermissionQuotation == true || n.CreatedUserId == UserId)).Count());

            vmodel.todSalesEntryCount = Convert.ToString(db.SalesEntrys.Where(n => (EF.Functions.DateDiffDay(n.SECreatedDate, lastdate) <= 0 && EF.Functions.DateDiffDay(n.SECreatedDate, today) >= 0) && (userpermissionSale == true || n.CreatedBy == UserId)).Count());

            vmodel.todPurchaseEntryCount = Convert.ToString(db.PurchaseEntrys.Where(n => (EF.Functions.DateDiffDay(n.PECreatedDate, lastdate) <= 0 && EF.Functions.DateDiffDay(n.PECreatedDate, today) >= 0) && (userpermissionPurchase == true || n.CreatedBy == UserId)).Count());

            //payments
            vmodel.todExpenseCount = Convert.ToString(db.Payments.Where(n => (EF.Functions.DateDiffDay(n.CreatedDate, today) == 0) && (userpermissionPayment == true || n.CreatedBy == UserId)).Count());

            vmodel.SalesCredit = Convert.ToString(db.SEPayments.Where(a => (userpermissionPayment == true || a.CreatedUserId == UserId)).Select(n => n.SEBillAmount - n.SEPaidAmount).AsEnumerable().DefaultIfEmpty(0).Sum());

            vmodel.PurchaseCredit = Convert.ToString(db.PEPayments.Where(a => (userpermissionPurchase == true || a.CreatedUserId == UserId)).Select(n => n.PEBillAmount - n.PEPaidAmount).AsEnumerable().DefaultIfEmpty(0).Sum());

            vmodel.totPayments = Convert.ToString(db.Payments.Where(a => (a.Voucher != 0) && (userpermissionPayment == true || a.CreatedBy == UserId)).Select(n => n.GrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());

            vmodel.totReciepts = Convert.ToString(db.Receipts.Where(a => (a.Voucher != 0) && (userpermissionReceipt == true || a.CreatedBy == UserId)).Select(n => n.GrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());

            vmodel.totPayment = Convert.ToString(db.Payments.Where(a => (a.Voucher != 0) && (userpermissionPayment == true || a.CreatedBy == UserId)).Count());
            vmodel.totReceipt = Convert.ToString(db.Receipts.Where(a => (a.Voucher != 0) && (userpermissionReceipt == true || a.CreatedBy == UserId)).Count());

            ViewBag.Active = "Dashboard";
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View(vmodel);
        }
        //  [HttpPost]


        //[HttpPost]
        public ActionResult GetSalesEntry()
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            };
            var MCArray = MCList.ToArray();

            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var userpermission = User.IsInRole("All Sales Entry");
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                     //    where a.Branch == BranchID
                     where (userpermission == true || a.CreatedBy == UserId)
                     && MCArray.Contains(a.MaterialCenter)
                     select new
                     {
                         a.SalesEntryId,
                         a.BillNo,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         a.SEDate,
                         a.SEGrandTotal,
                         c.SEPaidAmount,
                         SEBalanceAmount = a.SEGrandTotal - c.SEPaidAmount
                     });
            v = v.OrderByDescending(c => c.SalesEntryId).Take(5);
            var data = v.ToList();
            return Json(new { data = data });
        }
        public ActionResult GetSalesOrder()
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            
            var MCArray = MCList.ToArray();

            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var userpermission = User.IsInRole("All SalesOrder");
            var v = (from a in db.SalesOrders
                     join b in db.Customers on a.Customer equals b.CustomerID
                    
                     //    where a.Branch == BranchID
                     where (userpermission == true || a.CreatedUserId == UserId)
                    
                     select new
                     {
                         a.SalesOrderId,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         a.SODate,
                         a.SOGrandTotal,
                        
                     });
            v = v.OrderByDescending(c => c.SalesOrderId).Take(5);
            var data = v.ToList();
            return Json(new { data = data });

        }


        [HttpPost]
        public ActionResult GetQuotEntry()
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var userpermission = User.IsInRole("All Quotation  Entry");
            var v = (from b in db.Quotations
                     where (userpermission == true || b.CreatedUserId == UserId)
                     select new
                     {
                         b.QuotationId,
                         b.QuotNo,
                         Customer = db.Customers.Where(a => a.CustomerID == b.Customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault(),
                         b.QuotDate,
                         b.QuotGrandTotal,
                         validity = (b.QuotDate.AddDays((b.QuotValidity == null) ? 0 : (b.QuotValidity.Value + 1)) >= DateTime.Now) ? "Active" : "Expired"
                     });
            v = v.OrderByDescending(c => c.QuotationId).Take(5);
            var data = v.ToList();
            return Json(new { data = data });

        }



        
        public ActionResult Menu(string id = "")
        {
            ViewBag.Active = id;
            ViewBag.User = User.Identity.Name;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            if (BusinessType == "Property")
            {
                return PartialView("_PropertyMenu");
            }
            else
            {
                var user = User.Identity.GetUserId();
                IList<string> Role = UserManager.GetRolesAsync(user).Result;
                MenuViewModel vmodel = new MenuViewModel();
                vmodel.Menu = db.AppModuless.Where(p => Role.Contains(p.Name) && p.addMenu == choice.Yes).OrderBy(a => a.MenuOrder).ToList();
                return PartialView(vmodel);
            }
        }
        
        public ContentResult Company(string id = "")
        {
            var Company = db.companys.Where(a => a.CompanyID == 1).Select(a => a.CPName).FirstOrDefault();
            return Content(Company);
        }

        [AllowAnonymous]
        public ActionResult Expired()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Error()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult DateError()
        {
            var details = db.SystemConfigs.SingleOrDefault();
            var systemtype = (SystemType)Enum.Parse(typeof(SystemType), Security.Decrypt(details.SystemTypes, General.keyval));
            var sdate = Security.Decrypt(details.StartDate, General.keyval);
            var today = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
            DateTime startDate;
            DateTime lastDate;
            String format = "dd-MM-yyyy";
            try
            {
                startDate = DateTime.ParseExact(sdate, format, new CultureInfo("en-GB"));
            }
            catch
            {
                startDate = Convert.ToDateTime(sdate);
            }
            if (!string.IsNullOrEmpty(details.sld) && !string.IsNullOrWhiteSpace(details.sld))
            {
                var sld = Security.Decrypt(details.sld, General.keyval);
                try
                {
                    lastDate = DateTime.ParseExact(sld, format, new CultureInfo("en-GB"));
                }
                catch
                {
                    lastDate = Convert.ToDateTime(sld);
                }
                if ((today >= startDate) && (lastDate <= today))
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View();
        }

        
        public ActionResult CheckExpire()
        {
            var details = db.SystemConfigs.SingleOrDefault();
            var systemtype = (SystemType)Enum.Parse(typeof(SystemType), Security.Decrypt(details.SystemTypes, General.keyval));
            ExpireViewModel vmodel = new ExpireViewModel();
            if (systemtype == SystemType.Demo)
            {
                var startDate = Convert.ToDateTime(Security.Decrypt(details.StartDate, General.keyval));
                var endDate = Convert.ToDateTime(Security.Decrypt(details.EndDate, General.keyval));
                var timeperiod = Convert.ToInt32(Security.Decrypt(details.Extentdays, General.keyval));
                DateTime newDate = startDate.AddDays(timeperiod);
                var today = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                var addedDays = startDate.AddDays(10);
                if (newDate < addedDays)
                {
                    vmodel.Message = "Your Trial Period Will Expire Soon.. <br/> Please Buy a Liscence";
                }
                vmodel.Type = "Demo Version";
            }
            return PartialView();
        }

        [AllowAnonymous]
        public JsonResult Unauthorize()
        {
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = false, message = "Sorry You Dont Have Permission to Access This Section" } };
        }

        [AllowAnonymous]
        public ActionResult info()
        {
            var Mac = Convert.ToString(Security.GetMacAddress());
            var ProductKey = Security.kEYgEN();
            var date = Convert.ToDateTime("13/06/2018", new CultureInfo("en-GB"));
            var encodes = Security.Encrypt(date.ToString(), General.keyval);
            var insdate = Security.Encrypt(System.DateTime.Now.ToString("dd-MM-yyyy").ToString(), General.keyval);
            var sld = Convert.ToString(insdate);

            var today = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
            var d90 = Security.Encrypt(Convert.ToString(90), General.keyval);
            var d180 = Security.Encrypt(Convert.ToString(180), General.keyval);
            var d270 = Security.Encrypt(Convert.ToString(270), General.keyval);
            var d360 = Security.Encrypt(Convert.ToString(360), General.keyval);
            var d1800 = Security.Encrypt(Convert.ToString(1800), General.keyval);
            return Content(@"<h3>System Basic Info</h3><br/>Product Key : " + ProductKey +
                "<br/>Today : " + today.ToString() +
                "<br/>Today enc : " + sld +
                "<br/>90 days : " + d90 +
                "<br/>180 days : " + d180 +
                "<br/>270 days : " + d270 +
                "<br/>360 days : " + d360 +
                "<br/>1800 days : " + d1800
             );
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult loggedIn()
        {
            var v = User.Identity.IsAuthenticated;
            return Json(new { v });
        }

        private string EncodeServerName(string serverName)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(serverName));
        }

        private string DecodeServerName(string encodedServername)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedServername));
        }
        //[HttpPost]
        public ActionResult GetHireExp()
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            DateTime? Curdate = DateTime.Now;
            DateTime? fdate = Curdate.Value.AddDays(3);
            var fromv = "Sale";
            var tov = "SaleExtend";

            var uDev = User.IsInRole("Dev");
            var uSalesEntry = User.IsInRole("Sales Entry");

            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID
                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join d in db.HireReturns on a.SalesEntryId equals d.Invoice into hi
                     from d in hi.DefaultIfEmpty()
                     join f in db.ConvertTransactionss on a.SalesEntryId equals f.From into fi
                     from f in fi.DefaultIfEmpty()
                     join g in db.HrItems on h.HireDetailId equals g.HrItemId into hr
                     from g in hr.DefaultIfEmpty()
                     let sh = db.ConvertTransactionss.Where(ap => ap.From == a.SalesEntryId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()
                     let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.From == a.SalesEntryId).Select(x => x.From).FirstOrDefault()

                     let saleitem = (Int32?)db.SEItemss.Where(x => x.SalesEntry == a.SalesEntryId).Select(x => x.ItemQuantity).Sum() ?? 0
                     let hireitem = (Int32?)db.HrItems.Join(db.HireReturns, u => u.Hr, r => r.HireReturnId, (u, r) => new { u, r }).Where(x => x.r.Invoice == a.SalesEntryId).Select(x => x.u.ItemQuantity).Sum() ?? 0
                     where (a.SaleType == SaleType.Hire)
                     //&& (a.SalesEntryId != d.Invoice)
                     ////&& 
                     && (h.HireDetailId != d.HrNo)
                     && (EF.Functions.DateDiffDay(h.EndDate, fdate) >= 0)
                     && (a.SalesEntryId != f.From)
                     && (a.SalesEntryId != chkextend)
                     && saleitem != hireitem
                     select new
                     {
                         a.BillNo,
                         HExtent = sh.ConvertFrom,
                         a.SaleType,
                         a.SalesEntryId,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         StartDate = h.StartDate,
                         EndDate = h.EndDate,
                         Dev = uDev,
                         SalesEntry = uSalesEntry
                     });
            v = v.OrderByDescending(c => c.BillNo).Take(5);
            var data = v.ToList();
            return Json(new { data = data });

        }

        public ActionResult GetCrossHireExp()
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            DateTime? Curdate = DateTime.Now;
            DateTime? fdate = Curdate.Value.AddDays(3);
            var fromv = "purchase";
            var tov = "PurchaseExtend";

            var uDev = User.IsInRole("Dev");
            var uPurchaseEntry = User.IsInRole("Purchase Entry");

            var v = (from a in db.PurchaseEntrys
                     join b in db.Suppliers on a.Supplier equals b.SupplierID
                     join h in db.HireDetails on new { h1 = a.PurchaseEntryId, h2 = "purchase" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join d in db.CrossHireReturns on a.PurchaseEntryId equals d.Invoice into hi
                     from d in hi.DefaultIfEmpty()
                     join f in db.ConvertTransactionss on a.PurchaseEntryId equals f.From into fi
                     from f in fi.DefaultIfEmpty()
                     join g in db.CrossHrItems on a.PurchaseEntryId equals g.Hr into hr
                     from g in hr.DefaultIfEmpty()
                     let sh = db.ConvertTransactionss.Where(ap => ap.From == a.PurchaseEntryId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()
                     let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.From == a.PurchaseEntryId).Select(x => x.From).FirstOrDefault()

                     let peitem = (Int32?)db.PEItemss.Where(x => x.PurchaseEntry == a.PurchaseEntryId).Select(x => x.ItemQuantity).Sum() ?? 0
                     let hireitem = (Int32?)db.CrossHrItems.Join(db.CrossHireReturns, u => u.Hr, r => r.HireReturnId, (u, r) => new { u, r }).Where(x => x.r.Invoice == a.PurchaseEntryId).Select(x => x.u.ItemQuantity).Sum() ?? 0

                     where (a.PurType == PurchaseHireType.CrossHire)
                     //&& (a.SalesEntryId != d.Invoice)
                     ////&& 
                     && (h.HireDetailId != d.HrNo)
                     && (EF.Functions.DateDiffDay(h.EndDate, fdate) >= 0)
                     && (a.PurchaseEntryId != f.From)
                     && (a.PurchaseEntryId != chkextend)
                     && peitem != hireitem

                     select new
                     {
                         a.BillNo,
                         HExtent = sh.ConvertFrom,
                         a.PurType,
                         a.PurchaseEntryId,
                         Supplier = b.SupplierCode + " - " + b.SupplierName,
                         StartDate = h.StartDate,
                         EndDate = h.EndDate,
                         Dev = uDev,
                         PurchaseEntry = uPurchaseEntry
                     });
            v = v.OrderByDescending(c => c.BillNo).Take(5);
            var data = v.ToList();
            return Json(new { data = data });

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
