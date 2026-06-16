using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using QuickSoft.Controllers;

namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class PropertyHomeController : BaseController
    {
        ApplicationDbContext db;
        public UserManager<ApplicationUser> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }

        public PropertyHomeController()
        {
            db = new ApplicationDbContext();
            UserManager = LegacyIdentity.UserManager(db);
            RoleManager = LegacyIdentity.RoleManager(db);
        }

        //    [QkAuthorize(Roles = "Dev,Dashboard")]
        [HttpGet]
        public ActionResult Index()
        {
            var UserId = User.Identity.GetUserId();
            //var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            // var BranchID = 1;
            var today = DateTime.Now;
            var lastdate = today.AddMonths(-1);
            ViewBag.today = today.ToString("dd-MM-yyyy");
            ViewBag.lastdate = lastdate.ToString("dd-MM-yyyy");
            //var today = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            Common com = new Common();
            var Balance = com.Accbalance(3);
            PropertyHomeViewModel vmodel = new PropertyHomeViewModel();
            
            //vmodel.totSaleEntryCount = Convert.ToString(db.SalesEntrys.Where(n => n.Branch == BranchID).Count());
            vmodel.totunitcount = Convert.ToString(db.PropertyUnits.Where(a => (a.CreatedBy == UserId)).Count());

            var units = db.PropertyUnits.Select(x => x.Id).ToList();
            var allocated = db.TenancyContracts.Select(x => x.Unit).ToList();
            var emptyunits = units.Except(allocated).ToList();
            vmodel.totemptyunitcount = Convert.ToString(emptyunits.Count());

            vmodel.totBrokerscount= Convert.ToString(db.Brokers.Count());
            vmodel.totContractorscount = Convert.ToString(db.Contractors.Count());
            vmodel.totDeveloperscount = Convert.ToString(db.Developers.Count());
            vmodel.totLandloardscount = Convert.ToString(db.Landlords.Count());
            vmodel.totTenantscount = Convert.ToString(db.Tenants.Count());
            vmodel.totPropertycount = Convert.ToString(db.PropertyMains.Count());
            vmodel.totUnitscount = Convert.ToString(db.PropertyUnits.Count());

            vmodel.totexpense = Convert.ToString(db.Maintenances.Select(x => x.Amount).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.totincome = Convert.ToString(db.TenancyContracts.Select(x => x.Rent).AsEnumerable().DefaultIfEmpty(0).Sum());

            vmodel.totPropertRegistrationscount= Convert.ToString(db.PropertyRegistrations.Count());
            vmodel.tottenentcontractscount = Convert.ToString(db.TenancyContracts.Count());
            vmodel.totRentalInvoicescount = Convert.ToString(db.Rentals.Count());

            ViewBag.Active = "Dashboard";
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View(vmodel);
        }

        // Rich Real Estate dashboard (KPIs + charts + alert lists). Self-contained (no ajax widgets),
        // computed server-side; all collections materialized before in-memory shaping (EF Core 10 safe).
        [HttpGet]
        public ActionResult Dashboard()
        {
            var today = DateTime.Now;

            int units = db.PropertyUnits.Count();
            var allocated = db.TenancyContracts.Where(c => c.Status == 0).Select(c => c.Unit).Distinct().ToList();
            int occupied = allocated.Count(); if (occupied > units) occupied = units;
            int vacant = units - occupied; if (vacant < 0) vacant = 0;

            ViewBag.Properties = db.PropertyMains.Count();
            ViewBag.Units = units;
            ViewBag.Occupied = occupied;
            ViewBag.Vacant = vacant;
            ViewBag.OccupancyPct = units > 0 ? (int)Math.Round(occupied * 100.0 / units) : 0;
            ViewBag.Landlords = db.Landlords.Count();
            ViewBag.Tenants = db.Tenants.Count();
            ViewBag.Developers = db.Developers.Count();
            ViewBag.Brokers = db.Brokers.Count();
            ViewBag.Contractors = db.Contractors.Count();
            ViewBag.ActiveContracts = db.TenancyContracts.Count(c => c.Status == 0);
            ViewBag.RentRoll = db.TenancyContracts.Where(c => c.Status == 0).Select(c => (decimal?)c.Rent).Sum() ?? 0;
            ViewBag.MaintenanceCount = db.Maintenances.Count();
            ViewBag.MaintenanceCost = db.Maintenances.Select(m => (decimal?)m.Amount).Sum() ?? 0;
            ViewBag.Registrations = db.PropertyRegistrations.Count();
            ViewBag.RegistrationValue = db.PropertyRegistrations.Select(r => (decimal?)r.Amount).Sum() ?? 0;
            var soonC = today.AddDays(60);
            ViewBag.ExpiringContracts = db.TenancyContracts.Count(c => c.EndDate >= today && c.EndDate <= soonC);
            var soonD = today.AddDays(90);
            ViewBag.ExpiringDocs = db.PropertyDocumentTypes.Count(d => d.ExpDate >= today && d.ExpDate <= soonD);

            // ---- charts ----
            var utRows = (from u in db.PropertyUnits
                          join t in db.PropertyUnitTypes on u.UnitType equals t.ID into tt
                          from t in tt.DefaultIfEmpty()
                          select new { t.Name }).ToList();
            ViewBag.UnitsByType = System.Text.Json.JsonSerializer.Serialize(utRows.GroupBy(x => string.IsNullOrEmpty(x.Name) ? "Other" : x.Name)
                                  .Select(g => new { label = g.Key, count = g.Count() }).OrderByDescending(x => x.count).ToList());

            var ptRows = (from p in db.PropertyMains
                          join t in db.PropertyTypes on p.PropertyType equals t.ID into tt
                          from t in tt.DefaultIfEmpty()
                          select new { t.Name }).ToList();
            ViewBag.PropsByType = System.Text.Json.JsonSerializer.Serialize(ptRows.GroupBy(x => string.IsNullOrEmpty(x.Name) ? "Other" : x.Name)
                                  .Select(g => new { label = g.Key, count = g.Count() }).OrderByDescending(x => x.count).ToList());

            ViewBag.Occupancy = System.Text.Json.JsonSerializer.Serialize(new { occupied = occupied, vacant = vacant });

            // ---- alert / activity lists ----
            var expC = (from c in db.TenancyContracts
                        where c.EndDate >= today
                        join t in db.Tenants on c.Tenant equals t.TenantID into tt from t in tt.DefaultIfEmpty()
                        join p in db.PropertyMains on c.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                        join u in db.PropertyUnits on c.Unit equals u.Id into uu from u in uu.DefaultIfEmpty()
                        orderby c.EndDate
                        select new { id = c.Id, tenant = t.TenantName, property = p.Name, unit = u.Name, end = c.EndDate, rent = c.Rent }).Take(6).ToList();
            ViewBag.ExpContracts = System.Text.Json.JsonSerializer.Serialize(expC.Select(x => new { x.id, tenant = x.tenant ?? "-", property = x.property ?? "-", unit = x.unit ?? "-", end = x.end.ToString("dd-MM-yyyy"), rent = x.rent ?? 0 }).ToList());

            var expD = (from d in db.PropertyDocumentTypes
                        where d.ExpDate >= today
                        join p in db.PropertyMains on d.Reference equals p.Id into pp from p in pp.DefaultIfEmpty()
                        join dt in db.DocumentTypes on d.DocumentType equals dt.ID into dd from dt in dd.DefaultIfEmpty()
                        orderby d.ExpDate
                        select new { id = d.ID, doc = dt.Name, property = p.Name, exp = d.ExpDate }).Take(6).ToList();
            ViewBag.ExpDocs = System.Text.Json.JsonSerializer.Serialize(expD.Select(x => new { x.id, doc = x.doc ?? "Document", property = x.property ?? "-", exp = (x.exp ?? today).ToString("dd-MM-yyyy") }).ToList());

            var reg = (from r in db.PropertyRegistrations
                       join p in db.PropertyMains on r.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                       join d in db.Developers on r.Developer equals d.DeveloperID into dd from d in dd.DefaultIfEmpty()
                       orderby r.RDate descending
                       select new { id = r.RegistrationID, property = p.Name, developer = d.DeveloperName, amount = r.Amount, date = r.RDate }).Take(6).ToList();
            ViewBag.RecentReg = System.Text.Json.JsonSerializer.Serialize(reg.Select(x => new { x.id, property = x.property ?? "-", developer = x.developer ?? "-", amount = x.amount, date = x.date.ToString("dd-MM-yyyy") }).ToList());

            // ---- insights: maintenance expense mapping by property + P&L summary ----
            var maintByProp = (from m in db.Maintenances
                               join p in db.PropertyMains on m.Property equals p.Id into pp from p in pp.DefaultIfEmpty()
                               select new { name = p.Name, amount = m.Amount }).ToList();
            ViewBag.MaintByProperty = System.Text.Json.JsonSerializer.Serialize(
                maintByProp.GroupBy(x => string.IsNullOrEmpty(x.name) ? "-" : x.name)
                           .Select(g => new { label = g.Key, value = g.Sum(x => x.amount) }).OrderByDescending(x => x.value).Take(8).ToList());
            decimal plIncome = (decimal)ViewBag.RentRoll;
            decimal plExpense = (decimal)ViewBag.MaintenanceCost;
            ViewBag.PLNet = plIncome - plExpense;
            ViewBag.PLMargin = plIncome > 0 ? (int)Math.Round((plIncome - plExpense) * 100m / plIncome) : 0;

            ViewBag.Active = "Dashboard";
            return View();
        }

        public ActionResult GetDocumentExpairy()
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            DateTime today = DateTime.Now;
            DateTime fdate = today.AddDays(90);



            CultureInfo culture = new CultureInfo("en-US");
            var v = (from a in db.PropertyDocumentTypes
                     join ty in db.DocumentTypes on a.DocumentType equals ty.ID
                     join c in db.Landlords on new { c1 = a.Reference }
                     equals new { c1 = c.LandlordID } into land
                     from c in land.DefaultIfEmpty()
                     join d in db.PropertyMains on new { d1 = a.Reference }
                     equals new { d1 = d.Id } into pro
                     from d in pro.DefaultIfEmpty()
                     join e in db.Brokers on new { e1 = a.Reference }
                     equals new { e1 = e.BrokerID } into bro
                     from e in bro.DefaultIfEmpty()
                     join f in db.Developers on new { f1 = a.Reference }
                     equals new { f1 = f.DeveloperID } into dev
                     from f in dev.DefaultIfEmpty()
                     join g in db.Contractors on new { g1 = a.Reference }
                     equals new { g1 = g.ContractorID } into con
                     from g in con.DefaultIfEmpty()
                     join h in db.Tenants on new { h1 = a.Reference }
                     equals new { h1 = h.TenantID } into tenan
                     from h in tenan.DefaultIfEmpty()
                     join i in db.PropertyUnits on new { i1 = a.Reference }
                     equals new { i1 = i.Id } into unit
                     from i in unit.DefaultIfEmpty()
                     join j in db.TenancyContracts on new { j1 = a.Reference }
                     equals new { j1 = j.Id } into Tencntrct
                     from j in Tencntrct.DefaultIfEmpty()
                     join k in db.PropertyRegistrations on a.Reference equals k.RegistrationID into proreg
                     from k in proreg.DefaultIfEmpty()

                     where (EF.Functions.DateDiffDay(a.ExpDate, fdate) >= 0)
                     let proptenent=(from aa in db.PropertyMains
                                     join bb in db.TenancyContracts on aa.Id equals bb.Property
                                     join cc in db.Tenants on bb.Tenant equals cc.TenantID
                                     where cc.TenantID==a.Reference && a.Purpose=="Tenant"
                                     select aa.Name
                                     ).FirstOrDefault()
                     select new
                     {

                         expdate = (DateTime)a.ExpDate,
                         propname= (proptenent!=null)? proptenent:(d.Name!=null)?d.Name:null,
                         purpose = (a.Purpose == "Tenancy") ? "Tenancy Contract <br><a href='/Property/TenancyContract/Edit/" + j.Id.ToString() + "'>Details</a>" :
                         (a.Purpose == "PropertyRegistration") ? "PropertyRegistration<br><a href='/Property/PropertyRegistration/Edit/" + k.RegistrationID.ToString() + "'>Details</a>" :
                         (a.Purpose=="Tenant") ? "Tenant Document<br>"+ proptenent + "<br>"+h.TenantName+"<br><a href='/Property/Tenant/Edit/" + h.TenantID.ToString() + "'>Details</a>" :
                         (a.Purpose == "Broker") ? "Brocker <br><a href='/Property/Broker/Edit/" + e.BrokerID.ToString() + "'>Details</a>" : 
                         (a.Purpose == "Landlord") ? "Land Lord<br>"+c.LandlordName+"<br><a href='/Property/Landlords/Edit/" + c.LandlordID.ToString() + "'>Details</a>" : 
                         (a.Purpose == "Contractor") ? "Contractor<br>"+g.ContractorName+"<br><a href='/Property/Contractor/Edit/" + g.ContractorID.ToString() + "'>Details</a>" :
                           (a.Purpose == "Property") ? "Property<br>"+d.Name+"<br><a href='/Property/PropertyMain/Edit/" + d.Id.ToString() + "'>Details</a>" :
                         (a.Purpose == "Developer") ? "Developer<br><a href='/Property/Developer/Edit/" + f.DeveloperID.ToString() + "'>Details</a>" : "",



                         documentname = ty.Name,
                         attachment = a.ID,

                     }) ;

            var rent = (from t in db.TenancyContracts
                        join t2 in db.Tenants on t.Tenant equals t2.TenantID
                       
                        where (EF.Functions.DateDiffDay(t.EndDate, fdate) >= 0)
                        let propname=(from a in db.PropertyMains
                                      join b in db.TenancyContracts on a.Id equals b.Property
                                      where b.Id==t.Id
                                      select a.Name
                                      ).FirstOrDefault()
                        select new
                        {
                            expdate = t.EndDate,
                            propname=(propname!=null)? propname:null,
                            purpose = "Tenancy Contract Rent <br>"+ propname+"<br>"+t2.TenantName+"<br><a href='/Property/TenancyContract/Edit/" + t.Id.ToString() + "'>Details</a>",



                            documentname = "Tenancy Contract Rent",
                            attachment = t.Id,
                        });

            
            v = v.Union(rent);


            v = v.OrderByDescending(c => c.expdate).Take(50);
            var data = v.ToList();
            return Json(new { data = data });
        }
        //  [HttpPost]
        public ActionResult GetCheques()
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            DateTime today = DateTime.Now;
            DateTime fdate = today.AddDays(10);
        

            var v = (from a in db.Cheques
                     where ((EF.Functions.DateDiffDay(a.Date, today) <= 0) &&
                               (EF.Functions.DateDiffDay(a.Date, fdate) >= 0) )
                     select new
                     {
                         a.ID,
                         a.Purpose,
                         a.Reference,
                         a.Date,
                         a.ChequeNo,
                         a.Amount,
                     });
            v = v.OrderByDescending(c => c.ID).Take(5);
            var data = v.ToList();
            return Json(new { data = data });
        }
        //  [HttpPost]
        public ActionResult GetExpiryCheques()
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            DateTime today = DateTime.Now;
            DateTime fdate = today.AddDays(-10);


            var v = (from a in db.Cheques
                     where ((EF.Functions.DateDiffDay(a.Date, today) >= 0))
                     select new
                     {
                         a.ID,
                         a.Purpose,
                         a.Reference,
                         a.Date,
                         a.ChequeNo,
                         a.Amount,
                     });
            v = v.OrderByDescending(c => c.ID).Take(5);
            var data = v.ToList();
            return Json(new { data = data });
        }
        //[HttpPost]
        public ActionResult GetRegistrations()
        {
            var UserId = User.Identity.GetUserId();
           
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var v = (from a in db.PropertyRegistrations
                     join b in db.Developers on a.Developer equals b.DeveloperID into dev
                     from b in dev.DefaultIfEmpty()
                     join c in db.PropertyMains on a.Property equals c.Id into pro
                     from c in pro.DefaultIfEmpty()
                     join d in db.Brokers on a.Broker equals d.BrokerID into bro
                     from d in bro.DefaultIfEmpty()
                     join e in db.Accountss on a.Owner equals e.AccountsID into own
                     from e in own.DefaultIfEmpty()
                     select new
                     {
                         a.RegistrationID,
                         Developer=b.DeveloperName,
                         Property=c.Name,
                         Broker=d.BrokerName,
                         Owner=e.Name,
                         Date=a.RDate
                     });
            v = v.OrderByDescending(c => c.RegistrationID).Take(5);
            var data = v.ToList();
            return Json(new { data = data });

        }
        //[HttpPost]
        public ActionResult GetTenancyContracts()
        {
            var UserId = User.Identity.GetUserId();

            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var v = (from a in db.TenancyContracts
                     join b in db.PropertyUnits on a.Unit equals b.Id into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.PropertyMains on a.Property equals c.Id into rec
                     from c in rec.DefaultIfEmpty()
                     join e in db.Durations on a.Duration equals e.Id into dur
                     from e in dur.DefaultIfEmpty()
                     select new
                     {
                         a.Id,
                         Unit = b.Name,
                         Property = c.Name,
                         a.StartDate,
                         a.EndDate,
                         DueDate = (a.DueDate == 1 || a.DueDate == 21) ? a.DueDate + "st" : (a.DueDate == 2 || a.DueDate == 22) ? a.DueDate + "nd" : (a.DueDate == 3 || a.DueDate == 23) ? a.DueDate + "rd" : a.DueDate + "th",
                         Duration=e.Name
                     });
            v = v.OrderByDescending(c => c.Id).Take(5);
            var data = v.ToList();
            return Json(new { data = data });

        }
        //[HttpPost]
        public ActionResult GetMaintenanceContracts()
        {
            var UserId = User.Identity.GetUserId();

            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var v = (from a in db.Maintenances
                     join b in db.Contractors on a.Contractor equals b.ContractorID into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.PropertyMains on a.Property equals c.Id into rec
                     from c in rec.DefaultIfEmpty()
                     select new
                     {
                         a.ID,
                         Contractor = b.ContractorName,
                         Property = c.Name,
                         a.StartDate,
                         a.EndDate,
                         a.Amount,
                         
                     });
            v = v.OrderByDescending(c => c.ID).Take(5);
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
                IList<string> Role = UserManager.GetRoles(user);
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
            return Content(@"<h3>System Basic Info</h3><br/>Product Key : " + ProductKey +
                "<br/>Today : " + today.ToString() +
                "<br/>Today enc : " + sld
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
            // DateTime? hfrmdate = DateTime.Parse(fdate, new CultureInfo("en-GB"));
            // DateTime? htodate = DateTime.Parse(tdate, new CultureInfo("en-GB"));
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
            // DateTime? hfrmdate = DateTime.Parse(fdate, new CultureInfo("en-GB"));
            // DateTime? htodate = DateTime.Parse(tdate, new CultureInfo("en-GB"));
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

        // ===================== MODULE HUB LANDING PAGES =====================
        // Static card-grid entry screens for each top menu group (no DB/EF — pure links),
        // so none of the module's EF Core 10 port-break fixes are touched.
        [HttpGet] public ActionResult LeasingHub() { ViewBag.Active = "Leasing"; return View(); }
        [HttpGet] public ActionResult PortfolioHub() { ViewBag.Active = "Portfolio"; return View(); }
        [HttpGet] public ActionResult MaintenanceHub() { ViewBag.Active = "Maintenance"; return View(); }
        [HttpGet] public ActionResult InsightsHub() { ViewBag.Active = "Reports"; return View(); }
        [HttpGet] public ActionResult ConfigHub() { ViewBag.Active = "Configuration"; return View(); }

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