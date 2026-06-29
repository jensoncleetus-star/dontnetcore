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
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Net;
using System.IO;

using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Data.SqlClient;
namespace QuickSoft.Controllers
{
    public class ParticularPartyController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ParticularPartyController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Rack
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            var cust = db.Customers.Select(s => new
            {
                Id = s.CustomerID,
                Name = s.CustomerCode + "-" + s.CustomerName,
            }).Take(1).ToList();
            ViewBag.cus = QkSelect.List(cust, "Id", "Name");
            var supl = db.Suppliers.Select(s => new
            {
                Id = s.SupplierID,
                Name = s.SupplierCode + " - " + s.SupplierName
            }).Take(1).ToList();
            ViewBag.sup = QkSelect.List(supl, "Id", "Name");
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetParticularParty(string ColName)
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Lead Rejection");
            var uDelete = User.IsInRole("Delete Lead Rejection");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.ParticularParties
                     where (ColName == "" || a.PartyName == ColName)
                     select new
                     {
                         a.Id,
                         a.PartyID,
                         PartyType = (a.PartyType == 1) ? "Supplier" : "Customer",
                         a.PartyName,
                         //Dev = uDev,
                         //Edit = uEdit,
                         //Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.PartyName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);

            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(long? PartyType,long? customerid,long? supplierid,string PartyName)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            long partyid = 0;
            if(PartyType==0)
            {
                partyid = (long)customerid;
            }
            else
            {
                partyid = (long)supplierid;
            }
                var Exists = db.ParticularParties.Any(c => c.PartyID == partyid && c.PartyType == PartyType);
                if (Exists)
                {
                    msg = "Details already exists.";
                    stat = false;
                }
                else
                {
                ParticularParty cn = new ParticularParty
                {
                    PartyID = partyid,
                    PartyName = PartyName,
                    PartyType = PartyType

                };
                    db.ParticularParties.Add(cn);
                    db.SaveChanges();
                    Id = cn.Id;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ParticularParty", "ParticularParties", findip(), Id, "ParticularParty Added Successfully");

                    msg = "ParticularParty added successfully.";
                    stat = true;
                }


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            
            ParticularParty Lead = db.ParticularParties.Find(id);
            ViewBag.partyTyp = Lead.PartyType;
            if (Lead == null)
            {
                return NotFound();
            }
            if (Lead.PartyType == 1)
            {
                var supl = db.Suppliers.Where(s=>s.SupplierID==Lead.PartyID).Select(s => new
                {
                    Id = s.SupplierID,
                    Name = s.SupplierCode + " - " + s.SupplierName
                }).ToList();
                ViewBag.sup = QkSelect.List(supl, "Id", "Name");

                var cust = db.Customers.Select(s => new
                {
                    Id = s.CustomerID,
                    Name = s.CustomerCode + "-" + s.CustomerName,
                }).ToList();
                ViewBag.cus = QkSelect.List(cust, "Id", "Name");
            }
            else
            {
                var supl = db.Suppliers.Select(s => new
                {
                    Id = s.SupplierID,
                    Name = s.SupplierCode + " - " + s.SupplierName
                }).ToList();
                ViewBag.sup = QkSelect.List(supl, "Id", "Name");

                var cust = db.Customers.Where(s=>s.CustomerID==Lead.PartyID).Select(s => new
                {
                    Id = s.CustomerID,
                    Name = s.CustomerCode + "-" + s.CustomerName,
                }).ToList();
                ViewBag.cus = QkSelect.List(cust, "Id", "Name");
            }
           
            
            return PartialView(Lead);
        }
        public ActionResult Dashboard()
        {
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var CurrentYear = DateTime.Now.Year;
            var Currentmonth = DateTime.Now.Month;
            var Currentday = DateTime.Now.Day;
            var lastdate = today.AddDays(-30);

            //every last six months first day
            var ThisMnth1stDay = new DateTime(CurrentYear, Currentmonth, 1);
            var ThisYear1stDay = new DateTime(CurrentYear, 1, 1);
            var LastMnth1stDay = ThisMnth1stDay.AddMonths(-1);
            var Last2ndMnth1stDay = ThisMnth1stDay.AddMonths(-2);
            var last3rdMnth1stDay = ThisMnth1stDay.AddMonths(-3);
            var Last4thMnth1stDay = ThisMnth1stDay.AddMonths(-4);
            var Last5thMnth1stDay = ThisMnth1stDay.AddMonths(-5);
            //every last six months Last day
            var LastMnthLastDay = ThisMnth1stDay.AddDays(-1);
            var Last2ndMnthLastDay = LastMnth1stDay.AddDays(-1);
            var last3rdMnthLastDay = Last2ndMnth1stDay.AddDays(-1);
            var Last4thMnthLastDay = last3rdMnth1stDay.AddDays(-1);
            var Last5thMnthLastDay = Last4thMnth1stDay.AddDays(-1);
            //converting month to string(eg:'1=jan','2=feb'....)
            ViewBag.mnth2 = Last5thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth3 = Last4thMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth4 = last3rdMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth5 = Last2ndMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth6 = LastMnth1stDay.ToString("MMM", CultureInfo.InvariantCulture);
            ViewBag.mnth7 = today.ToString("MMM", CultureInfo.InvariantCulture);
            var excludeparty = db.ParticularParties.Where(a => a.PartyType == 0).Select(a => a.PartyID).ToArray();
            HomeViewModel vmodel = new HomeViewModel();
            var allCustomer = User.IsInRole("All Customers");
            var userpermissionPayment = User.IsInRole("All Payment Entry");
            vmodel.totCustomerCount = Convert.ToString(db.Customers.Where(x => x.Type == CRMCustomerType.Customer && (allCustomer == true)).Count());

            var thisyearsale= db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var thisyearsalertn= db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.SalesCredit = Convert.ToString(thisyearsale - thisyearsalertn);

            var todaysales = db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, today) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0)&&(!excludeparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var todaysalesrtn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, today) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.todaySales = Convert.ToString(todaysales - todaysalesrtn);

            var thismonthsale=db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var thismonthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.ThisMonthSales = Convert.ToString(thismonthsale - thismonthsalertn);
            var lastmnthsale = db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, LastMnthLastDay) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var lastmnthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, LastMnthLastDay) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.LastMonthSales = Convert.ToString(lastmnthsale- lastmnthsalertn);
            var last2mnthsale = db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last2ndMnthLastDay) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            var last2mnthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, Last2ndMnthLastDay) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();

            vmodel.LastTwoMonthSales = Convert.ToString(last2mnthsale- last2mnthsalertn);
            vmodel.LastThreeMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, last3rdMnthLastDay) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastFourMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last4thMnthLastDay) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastFiveMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last5thMnthLastDay) >= 0) && (!excludeparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());

            vmodel.ThisMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, today) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, LastMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastSecondMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, Last2ndMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastThirdMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, last3rdMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastForthMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, Last4thMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastFifthMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, Last5thMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());





            return View(vmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit(long? PartyType, long? customerid, long? supplierid, string PartyName,long Id)
        {
            bool stat = false;
            string msg;
            var Exists = true;
            if (ModelState.IsValid)
            {
                if (PartyType == 1)
                {
                    Exists = db.ParticularParties.Any(c => c.PartyID == supplierid && c.PartyType == PartyType && c.Id != Id);
                }
                else
                {
                    Exists = db.ParticularParties.Any(c => c.PartyID == customerid && c.PartyType == PartyType && c.Id != Id);
                }
                
                if (Exists)
                {
                    msg = "ParticularParty already exists.";
                    stat = false;
                }
                else
                {
                    ParticularParty Party = db.ParticularParties.Find(Id);
                    Party.PartyType = PartyType;
                    if (PartyType == 1)
                    {
                        Party.PartyID = supplierid;
                    }
                    else
                    {
                        Party.PartyID = customerid;
                    }
                    Party.PartyName = PartyName;
                    db.Entry(Party).State = EntityState.Modified;
                    db.SaveChanges();


                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "ParticularParty", "ParticularParty", findip(), Id, "ParticularParty Details Updated Successfully");


                    msg = "Successfully updated ParticularParty details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParticularParty Lead = db.ParticularParties.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Lead Type")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.ParticularParties.Any(c => c.Id == id);
            if (Exists)
            {

                stat = DeleteFn(id);
                msg = "Successfully deleted ParticularParty.";
            }
            else
            {
                msg = "Unable to delete ParticularParty.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete LeadType")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " LeadType, Unable to Delete " + notdel + " LeadType. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "LaedType.", true);
            }
            else
            {
                Success("Deleted " + count + " LeadType.", true);
            }
            return RedirectToAction("Index", "LeadType");
        }


        public bool DeleteFn(long id)
        {
            ParticularParty Lead = db.ParticularParties.Find(id);
            if (Lead != null)
            {
                db.ParticularParties.RemoveRange(db.ParticularParties.Where(a => a.Id == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "ParticularParty", "ParticularParties", findip(), Lead.Id, "ParticularParty Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }
    }
}
