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
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class LeadsController : BaseController
    {
        //
        ApplicationDbContext db;
        Common com;
        public LeadsController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        public ActionResult alls()
        {
            followupviewmodel vmodel = new followupviewmodel();
            vmodel.totalleads = GetAllMyLeads().ToString();
            vmodel.totaltask = GetMyTaskcount().ToString();
            var UserId = User.Identity.GetUserId();
            vmodel.empid = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            vmodel.dates = System.DateTime.Now.ToString("dd-M-yyyy", CultureInfo.InvariantCulture);
            vmodel.totalcustomerfollowups = GetAmountReceivablePayable("", 1, true, vmodel.empid, vmodel.dates, null).ToString();
            vmodel.totalamc = GetMyAMCDetails(null, null, null, null, "", "", "", "", "").ToString();
            return View(vmodel);

        }
    
        public ActionResult leaddashboard()
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

            HomeViewModel vmodel = new HomeViewModel();
            var allCustomer = User.IsInRole("All Customers");
            var userpermissionPayment = User.IsInRole("All Payment Entry");
            vmodel.totCustomerCount = Convert.ToString(db.Customers.Where(x => x.Type == CRMCustomerType.Customer && (allCustomer == true)).Count());
            vmodel.SalesCredit = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.todaySales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, today) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.ThisMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, LastMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastTwoMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last2ndMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastThreeMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, last3rdMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastFourMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last4thMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            vmodel.LastFiveMonthSales = Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last5thMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());

            vmodel.ThisMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, today) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, LastMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastSecondMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, Last2ndMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastThirdMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, last3rdMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastForthMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, Last4thMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());
            vmodel.LastFifthMonthLeadsCount = Convert.ToString(db.Customers.Where(x => (EF.Functions.DateDiffDay(x.CreatedDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(x.CreatedDate, Last5thMnthLastDay) >= 0) && x.Type == CRMCustomerType.Leads && (allCustomer == true)).Count());

            var v = (from a in db.Customers
                     join b in db.leaddashbordorder on a.CurrentAction equals b.lead
                     join c in db.LeadStatuss on a.CurrentAction equals c.LeadStatusID
                     let k = ((a.EndTime == null) ? a.logtime : a.EndTime).Value.AddMinutes(b.duration ?? 0)
                     where a.Type == CRMCustomerType.Leads
                     && a.OpenClose!=1
                     select new
                     {
                         statusname = c.StatusType,
                         satusid = c.LeadStatusID,
                         dt = k

                     }).GroupBy(o => new { o.satusid }, (y, group) => new
                     {
                         statusname = group.FirstOrDefault().statusname,
                         count = group.Count(),
                         statusid = y.satusid
                     }).Select(o => new paramclass
                     {
                         statusname = o.statusname,
                         count = o.count,
                         satusid = o.statusid
                     }).Distinct().ToList();


            ViewBag.dashboard = v;
            //           group new { l.CustomerID, l.CreatedDate } by new { l.CustomerID, l.CreatedDate } into g
            //               Customer = g.FirstOrDefault().CustomerID,
            //               createdate = g.FirstOrDefault().CreatedDate,


            //          let k = DbFunctionsCompat.AddMinutes(l.createdate, a.duration)


            //          group new { c.CurrentAction, b.StatusType, k } by new { l.Customer } into g


            //              statusname = g.FirstOrDefault().StatusType,
            //              satusid = g.FirstOrDefault().CurrentAction,
            //              ndate = g.FirstOrDefault().k



            // ).Where(o => o.ndate < System.DateTime.Now).Select(o => new paramclass

            //     statusname = o.statusname,

            //     satusid = o.satusid,
            //     count = o.count


            var vv = (from a in db.Customers
                      join b in db.leaddashbordorder on a.CurrentAction equals b.lead
                      join c in db.LeadStatuss on a.CurrentAction equals c.LeadStatusID
                      let k = ((a.EndTime == null) ? a.logtime : a.EndTime).Value.AddMinutes(b.duration ?? 0)
                      where a.Type == CRMCustomerType.Leads
                       && a.OpenClose != 1
                      select new
                      {
                          statusname = c.StatusType,
                          satusid = c.LeadStatusID,
                          dt = k

                      }).Where(o => o.dt < System.DateTime.Now).GroupBy(o => new { o.satusid }, (y, group) => new
                      {
                          statusname = group.FirstOrDefault().statusname,
                          count = group.Count(),
                          statusid = y.satusid
                      }).Select(o => new paramclass
                      {
                          statusname = o.statusname,
                          count = o.count,
                          satusid = o.statusid
                      }).Distinct().ToList();

            ViewBag.expiredlead = vv;




            return View(vmodel);
        }
        public long GetMyAMCDetails(long? ContractNo, long? Customer, long? ContractName, long? ContractType, string RFromDate, string RToDate, string StartDate, string EndDate, string EFromDate)
        {

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? rfdate = null;
            DateTime? rtdate = null;
            DateTime? etdate = null;


            if (StartDate != "")
            {
                fdate = DateTime.Parse(StartDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (EndDate != "")
            {
                tdate = DateTime.Parse(EndDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (EFromDate != "")
            {
                etdate = DateTime.Parse(EFromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (RFromDate != "")
            {
                rfdate = DateTime.Parse(RFromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (RToDate != "")
            {
                rtdate = DateTime.Parse(RToDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            // EF Core 10 cannot translate the original AMC dashboard query: the AmcUps subquery
            // (GroupBy -> Select(g => g.OrderByDescending().FirstOrDefault()) of a full entity) LEFT-JOINed in,
            // plus a nested AssignedTo collection projection. This helper only returns a COUNT, and none of the
            // original LEFT JOINs (Customers/AmcContracts/AmcContractTypes/AmcStatuss/LocationNames/AmcUps)
            // appear in the WHERE or fan out rows, so they cannot affect the count. Count the matching Amcs
            // directly; the assignment filter becomes a server-side EXISTS.
            var v = (from a in db.Amcs
                     where
                     (ContractNo == 0 || ContractNo == null || a.AmcNo == ContractNo) &&
                     (Customer == 0 || Customer == null || a.CustomerId == Customer) &&
                     (ContractName == 0 || ContractName == null || a.ContractId == ContractName) &&
                     (ContractType == 0 || ContractType == null || a.ContractTypeId == ContractType) &&
                     (StartDate == "" || StartDate == null || EF.Functions.DateDiffDay(a.StartDate, fdate) <= 0) &&
                     (EndDate == "" || EndDate == null || EF.Functions.DateDiffDay(a.EndDate, tdate) >= 0) &&
                      (RFromDate == "" || RFromDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rfdate) <= 0) &&
                     (RToDate == "" || RToDate == null || EF.Functions.DateDiffDay(a.ReminderDate, rtdate) >= 0) &&



                     // assign.Contains(EmpId) rewritten as a server-translatable EXISTS — the original
                     // `let assign = ....ToList()` is not translatable in EF Core 10; semantics are identical
                     // (EmpId is in the set of active "Assigned" employees for this AmcId).
                     db.AmcAssignedTos.Any(x => x.AmcId == a.AmcId && x.Status == "Assigned" && x.ChkStatus == Status.active && x.EmployeeId == EmpId)
                     // Count only: the original projection built a nested AssignedTo collection (.ToList()),
                     // which EF Core 10 cannot translate ("ProjectionBindingExpression could not be translated").
                     // None of the projected columns or the ldate ordering affect the COUNT (the LEFT JOINs never
                     // fan out and no WHERE predicate uses them), so project the key alone.
                     select a.AmcId).ToList();

            return v.Count();
        }
        public ActionResult customerdashboard()
        {
            var uid = User.Identity.GetUserId();
            var customerid = db.Customers.Where(o => o.BankName == uid).FirstOrDefault();
            // This is the CUSTOMER-portal dashboard; a non-customer (employee/admin) has no linked
            // Customer row -> send them to the main app dashboard instead of NRE-ing on customerid.
            if (customerid == null) return RedirectToAction("Index", "Home");
            var totalsales = (decimal?)db.SalesEntrys.Where(o => o.Customer == customerid.CustomerID && o.SEDate >= customerid.startbonusdate).Select(o => (decimal?) o.SEGrandTotal).Sum() ??0;
            var totalsalesretun = (decimal?)db.SalesReturns.Where(o => o.Customer == customerid.CustomerID && o.SRDate >= customerid.startbonusdate).Select(o => (decimal?)o.SRGrandTotal).Sum() ??0;
            var totalinvoiceamount = totalsales - totalsalesretun;
            customerdashboards vmodel = new customerdashboards();
            vmodel.invoices = (totalinvoiceamount == null) ? "Nill" : totalinvoiceamount.ToString();
            vmodel.StartDate = (customerid.startbonusdate == null) ? "" : customerid.startbonusdate.Value.ToString("dd-MM-yyyy");
            return View(vmodel);
        }
        public ActionResult followups()
        {

            followupviewmodel vmodel = new followupviewmodel();
            vmodel.totalleads = GetAllMyLeads().ToString();
            vmodel.totaltask = GetMyTaskcount().ToString();
            var UserId = User.Identity.GetUserId();
            vmodel.empid = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            vmodel.dates = System.DateTime.Now.ToString("dd-M-yyyy", CultureInfo.InvariantCulture);
            vmodel.totalcustomerfollowups = GetAmountReceivablePayable("", 1, true, vmodel.empid, vmodel.dates, null).ToString();
            vmodel.totalamc = GetMyAMCDetails(null, null, null, null, "", "", "", "", "").ToString();
            return View(vmodel);
        }
        public long GetAmountReceivablePayable(string nextdate, long? ddlType, bool? pdc, long? emp, string ondate, long? exp)
        {
            int recordsTotal = 0;
            string result = "";
            int UpdatedDateExpiry = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "UpdatedDateExpiry").Select(o => o.TypeValue).FirstOrDefault());
            int NextDateExpiry = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "NextDateExpiry").Select(o => o.TypeValue).FirstOrDefault());


            DateTime? ondates = null;
            if (ondate != "")
            {
                ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
            }
            Common com = new Common();
            long[] accGp = null;
            //creditor
            if (ddlType == 0)
            {
                //sundry creditor
                var creparentid = new SqlParameter("@parentid", 10);
                var cregroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", creparentid).AsEnumerable().ToList();
                accGp = cregroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            else
            {
                //sundry debitor
                var debparentid = new SqlParameter("@parentid", 11);
                var debgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", debparentid).AsEnumerable().ToList();
                accGp = debgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            }
            DateTime datenow = DateTime.Now;
            DateTime ndate = DateTime.Now.AddYears(-2);
            var userid = User.Identity.GetUserId();
            var v2 = (from b in db.AccountsTransactions
                      join c in db.Customers on b.Account equals c.Accounts

                      where (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)
                      group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                      select new
                      {
                          Debit = g.Sum(o => o.Debit),
                          Credit = g.Sum(o => o.Credit),
                          Account = g.FirstOrDefault().Account

                      });
            var v = (from a in db.Accountss
                     join b in db.Customers on a.AccountsID equals b.Accounts
                     join c in db.Employees on b.SalesPerson equals c.EmployeeId into emps
                     from c in emps.DefaultIfEmpty()
                     join d in v2 on a.AccountsID equals d.Account
                     // Cast subquery scalars to DateTime? so EF Core emits a plain nullable scalar subquery
                     // (NULL when the customer has no remarks). Selecting the non-nullable DateTime directly
                     // makes EF Core 10 wrap it in COALESCE(subquery, '0001-01-01...'), and that literal is out
                     // of range for SQL `datetime` (1753-9999) -> "Conversion failed converting date/time from
                     // character string" for every remark-less customer. Legacy EF6 returned default(DateTime)
                     // client-side; we restore that exact value after materialization (see ?? DateTime.MinValue below).
                     let createdDate = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.CreatedDate).FirstOrDefault()
                     let nexttime = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID && c.AddedUser == userid).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.nexttime).FirstOrDefault()
                     let nexttime1 = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.nexttime).FirstOrDefault()

                     //let CrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let DrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let aging = (from c in db.SalesEntrys
                     //             where d.SEPaidAmount == 0 && d.CustomerId == b.CustomerID


                     //             }).OrderBy(o => o.Days).Select(o => o.Days).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault()
                     where accGp.Contains(a.Group) &&
                     (emp == null || emp == 0 || b.SalesPerson == emp)
                     select new
                     {
                         b.CustomerID,
                         a.AccountsID,
                         particulars = a.Name,
                         Credit = d.Credit,
                         Debit = d.Debit,
                         days = "",
                         createdDate,
                         nexttime,
                         nexttime1,

                         // nextfolloupdatetime=createdDate.nexttime,
                         FirstName = c.FirstName + " " + c.MiddleName + " " + c.LastName
                     }).AsEnumerable().Select(o => new
                     {
                         o.CustomerID,
                         o.AccountsID,
                         o.particulars,
                         o.Debit,
                         o.Credit,
                         DrCr = o.Debit > o.Credit ? "Dr" : "Cr",
                         o.days,
                         createdDate = o.createdDate ?? DateTime.MinValue,
                         nextfolloupdatetime = ((o.nexttime ?? DateTime.MinValue) < ndate) ? (o.nexttime1 ?? DateTime.MinValue) : (o.nexttime ?? DateTime.MinValue),


                         o.FirstName,
                         Amount = o.Debit > o.Credit ? (o.Debit - o.Credit) : (o.Credit - o.Debit)
                     }).Where(a => a.Amount > 0);
            if (nextdate != "")
            {

                DateTime nextdates = DateTime.Parse(nextdate, new CultureInfo("en-GB"));
                v = v.Where(o => o.nextfolloupdatetime >= nextdates && o.nextfolloupdatetime < nextdates.AddDays(1));
            }
            if (exp == 1)
            {

                v = v.Where(o => (o.createdDate.AddMinutes(UpdatedDateExpiry)) < System.DateTime.Now);
                v = v.Where(o => o.createdDate > ndate);

            }

            if (exp == 3)
            {

                v = v.Where(o => (o.nextfolloupdatetime.AddMinutes(NextDateExpiry)) < System.DateTime.Now && o.nextfolloupdatetime > ndate);

            }
            if (exp == 2)
            {

                v = v.Where(o => o.createdDate < ndate);

            }
            return v.ToList().Count();
        }

        public long GetMyTaskcount()//TKUpdateStatus? status //IEnumerable<TKUpdateStatus> status
        {

            int days = 0;

            DateTime datecheck = DateTime.Now.AddDays(-days);





            var UserId = User.Identity.GetUserId();
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            bool allcheck = User.IsInRole("All ProTask");
            bool editcheck = User.IsInRole("Edit ProTask");
            bool devcheck = User.IsInRole("Dev");


            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? remddate = null;
            DateTime assdate = DateTime.Now.AddDays(-90);
            var taskassign = (from z in db.TaskAssigneds
                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                              where z.Status == "Assigned" && z.chkStatus == Status.active &&
                              (z.CreatedDate >= assdate)
                              select z);
            var UserView = (from a in db.ProTasks

                            where taskassign.Any(x => x.ProTaskId == a.ProTaskId && x.Status == "Assigned" && x.chkStatus == Status.active && x.EmployeeId == empId) &&
                            (a.OpenClose == 0 || a.OpenClose == null)
                            select new
                            {
                                a.ProTaskId,

                            })
                            .ToList().Select(o => new
                            {
                                o.ProTaskId,

                            });

            return UserView.Count();
        }

        public long GetAllMyLeads()
        {

            int days = 0;

            DateTime crdate = DateTime.Now.AddDays(-100);
            DateTime datecheck = DateTime.Now.AddDays(-days);
            int taken = 300;

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? ndate = null;

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

















            var v = (from a in db.Customers






                     let f = db.AssignedTos.Where(cl => cl.CustomerID == a.CustomerID && cl.approve == false && cl.Status == "Assigned" && cl.ChkStatus == (int)Status.active).Select(cl => cl.EmployeeId).ToList()

                     let g = db.LeadTaskUpdations.Where(lr => lr.TaskId == a.CustomerID && lr.CreatedBy == UserId).OrderByDescending(a => a.TaskUpdationID).FirstOrDefault()

                     where a.Type == CRMCustomerType.Leads
                     && f.Contains(empl.EmployeeId)
                     && (a.OpenClose == 0 || a.OpenClose == null)

                     select new
                     {
                         id = a.CustomerID,
                         ldate = ((g != null) && (g.CreatedDate > a.logtime)) ? g.CreatedDate : a.logtime,

                     }).Where(o => o.ldate > crdate).ToList().Select(o => new
                     {
                         o.id,




                     });


            return v.Count();


        }


        public ActionResult accountsdashboard()
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
            DateTime firstdate = new DateTime(2000, 1, 1);

            var fdate = new DateTime(today.Year, today.Month, 1);
            var cashfdate = new DateTime(today.Year - 50, today.Month, 1);
            var tdate = today;
            var fun = 2;
            var mc = 1;
            var dexpenses = Common.GetChildAccGroupmc(29, "Expenses (Direct/Mfg.)", "liability", fdate, tdate, fun, 2, mc).ToList();
            long[] accGp = null;
            var creparentid = new SqlParameter("@parentid", 10);
            var cregroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", creparentid).AsEnumerable().ToList();
            accGp = cregroupsdata.Select(a => a.AccountsGroupID).ToArray();
            //in direct expenses
            var indexpenses = Common.GetChildAccGroupmc(30, "Expenses (Indirect/Admn.)", "liability", fdate, tdate, fun, 2, mc).ToList();
            var cashinhand = Common.GetChildAccGroupmc(9, "Cash In Hand", "Asset", cashfdate, tdate, fun, 2, mc).ToList();
            decimal retval = 0;

            var CrSum = (from a in db.AccountsTransactions
                         join b in db.Accountss on a.Account equals b.AccountsID
                         where accGp.Contains(b.Group) &&
                         a.Date >= fdate && a.Date <= tdate
                         select new
                         {
                             Credit = (a == null) ? 0 : a.Credit,
                             Debit = (a == null) ? 0 : a.Debit,
                         }).ToList().Sum(o => o.Credit);
            var CrSum1 = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          where accGp.Contains(b.Group) &&
                          a.Date >= fdate && a.Date <= tdate
                          select new
                          {
                              Credit = (a == null) ? 0 : a.Credit,
                              Debit = (a == null) ? 0 : a.Debit,
                          }).ToList().Sum(o => o.Debit);
            CrSum = Math.Abs(CrSum - CrSum1);
            var debparentid = new SqlParameter("@parentid", 11);
            var debgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", debparentid).AsEnumerable().ToList();
            accGp = debgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var DrSum = (from a in db.AccountsTransactions
                         join b in db.Accountss on a.Account equals b.AccountsID
                         where accGp.Contains(b.Group) &&
                         a.Date >= fdate && a.Date <= tdate
                         select new
                         {
                             Credit = (a == null) ? 0 : a.Credit,
                             Debit = (a == null) ? 0 : a.Debit,
                         }).ToList().Sum(o => o.Debit);
            var DrSum1 = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          where accGp.Contains(b.Group) &&
                          a.Date >= fdate && a.Date <= tdate
                          select new
                          {
                              Credit = (a == null) ? 0 : a.Credit,
                              Debit = (a == null) ? 0 : a.Debit,
                          }).ToList().Sum(o => o.Credit);
            DrSum = Math.Abs(DrSum - DrSum1);
            var assets = Common.GetChildAccGroupmc(4, "assets", "Asset", fdate, tdate, fun, 2, mc).ToList();
            ViewBag.asset = assets.Sum(o => o.Debit - o.Credit);
            ViewBag.datefrom = fdate;
            ViewBag.dateto = tdate;
            ViewBag.amountrecieve = Math.Abs(Math.Round(Convert.ToDouble(DrSum), 2));
            ViewBag.amountpay = Math.Round(Convert.ToDouble(CrSum), 2);
            ViewBag.cash = Math.Round(Convert.ToDouble(cashinhand.Sum(o => o.Debit - o.Credit)), 2);
            ViewBag.expense = Math.Round(Convert.ToDouble(dexpenses.Sum(o => o.Debit - o.Credit) + indexpenses.Sum(o => o.Debit - o.Credit)), 2);

            dexpenses = Common.GetChildAccGroupmc(29, "Expenses (Direct/Mfg.)", "liability", ThisMnth1stDay, tdate, fun, 2, mc).ToList();
            HomeViewModel vmodel = new HomeViewModel();
            //in direct expenses
            indexpenses = Common.GetChildAccGroupmc(30, "Expenses (Indirect/Admn.)", "liability", ThisMnth1stDay, tdate, fun, 2, mc).ToList();
            var totalexpense = Math.Round(Convert.ToDouble(dexpenses.Sum(o => o.Debit - o.Credit) + indexpenses.Sum(o => o.Debit - o.Credit)), 2);
            vmodel.ThisMonthSales = totalexpense.ToString();
            dexpenses = Common.GetChildAccGroupmc(29, "Expenses (Direct/Mfg.)", "liability", LastMnth1stDay, LastMnthLastDay, fun, 2, mc).ToList();
            indexpenses = Common.GetChildAccGroupmc(30, "Expenses (Indirect/Admn.)", "liability", LastMnth1stDay, LastMnthLastDay, fun, 2, mc).ToList();
            totalexpense = Math.Round(Convert.ToDouble(dexpenses.Sum(o => o.Debit - o.Credit) + indexpenses.Sum(o => o.Debit - o.Credit)), 2);

            vmodel.LastMonthSales = totalexpense.ToString();// Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, LastMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            dexpenses = Common.GetChildAccGroupmc(29, "Expenses (Direct/Mfg.)", "liability", Last2ndMnth1stDay, last3rdMnthLastDay, fun, 2, mc).ToList();
            indexpenses = Common.GetChildAccGroupmc(30, "Expenses (Indirect/Admn.)", "liability", Last2ndMnth1stDay, last3rdMnthLastDay, fun, 2, mc).ToList();
            totalexpense = Math.Round(Convert.ToDouble(dexpenses.Sum(o => o.Debit - o.Credit) + indexpenses.Sum(o => o.Debit - o.Credit)), 2);

            vmodel.LastTwoMonthSales = totalexpense.ToString();//Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last2ndMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            dexpenses = Common.GetChildAccGroupmc(29, "Expenses (Direct/Mfg.)", "liability", last3rdMnth1stDay, last3rdMnthLastDay, fun, 2, mc).ToList();
            indexpenses = Common.GetChildAccGroupmc(30, "Expenses (Indirect/Admn.)", "liability", last3rdMnth1stDay, last3rdMnthLastDay, fun, 2, mc).ToList();
            totalexpense = Math.Round(Convert.ToDouble(dexpenses.Sum(o => o.Debit - o.Credit) + indexpenses.Sum(o => o.Debit - o.Credit)), 2);

            vmodel.LastThreeMonthSales = totalexpense.ToString();// Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, last3rdMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            dexpenses = Common.GetChildAccGroupmc(29, "Expenses (Direct/Mfg.)", "liability", Last4thMnth1stDay, Last4thMnthLastDay, fun, 2, mc).ToList();
            indexpenses = Common.GetChildAccGroupmc(30, "Expenses (Indirect/Admn.)", "liability", Last4thMnth1stDay, Last4thMnthLastDay, fun, 2, mc).ToList();
            totalexpense = Math.Round(Convert.ToDouble(dexpenses.Sum(o => o.Debit - o.Credit) + indexpenses.Sum(o => o.Debit - o.Credit)), 2);

            vmodel.LastFourMonthSales = totalexpense.ToString();// Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last4thMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());
            dexpenses = Common.GetChildAccGroupmc(29, "Expenses (Direct/Mfg.)", "liability", Last5thMnth1stDay, Last5thMnthLastDay, fun, 2, mc).ToList();
            indexpenses = Common.GetChildAccGroupmc(30, "Expenses (Indirect/Admn.)", "liability", Last5thMnth1stDay, Last5thMnthLastDay, fun, 2, mc).ToList();
            totalexpense = Math.Round(Convert.ToDouble(dexpenses.Sum(o => o.Debit - o.Credit) + indexpenses.Sum(o => o.Debit - o.Credit)), 2);

            vmodel.LastFiveMonthSales = totalexpense.ToString();// Convert.ToString(db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last5thMnthLastDay) >= 0)).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum());



            return View(vmodel);
        }
        public ActionResult cashdashboard(string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            DateTime? fdate = null;
            DateTime? tdate = null;


            if (fromdate != "" && fromdate != null)
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "" && todate != null)
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (fdate == null)
                fdate = DateTime.Now.Date;
            if (tdate == null)
                tdate = DateTime.Now.Date;
            ViewBag.datefrom = fdate;
            ViewBag.dateto = tdate;
            var acs = db.accountmaps.Select(o => o.AccountId).Distinct().ToArray();
            var creditsales = (

                         from a in db.SalesEntrys

                         join e in db.SEPayments on a.SalesEntryId equals e.SalesEntry
                         join b in db.MCs on a.MaterialCenter equals b.MCId
                         where
                         (fdate == null || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                         where (e.SEBillAmount - e.SEPaidAmount) > 0
                         select new
                         {
                             credit = e.SEBillAmount - e.SEPaidAmount,
                             b.MCId
                         }).GroupBy(o => new { o.MCId }, (y, group) => new
                         {
                             shop = y.MCId,
                             cr = group.Sum(o => o.credit)
                         }).ToList();


            var sales = (
        from b in db.AccountsTransactions
        join a in db.SalesEntrys on b.reference equals a.SalesEntryId into sal
        from a in sal.DefaultIfEmpty()
        join e in db.SEPayments on a.SalesEntryId equals e.SalesEntry into sepay
        from e in sepay.DefaultIfEmpty()
        join c in db.Accountss on b.Account equals c.AccountsID
        join d in db.MCs on a.MaterialCenter equals d.MCId into mcs
        from d in mcs.DefaultIfEmpty()
        where
        (fdate == null || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) &&
          (tdate == null || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
          && acs.Contains(b.Account)

        select new
        {

            c.Name,
            b.Debit,
            b.Credit,
            b.Purpose,
            creditbal = 0,//(e!=null)?(e.SEBillAmount-e.SEPaidAmount):0,
            showroom = (c != null) ? (c.Name.Contains("MUS") ? "MUSSAFA" : c.Name.Contains("ABU") ? "ABUDHABI" : "ALAIN") : "",
            //a.SEGrandTotal,
        }).ToList();

            ViewBag.mussafacashsales = sales.Where(o => o.showroom == "MUSSAFA" &&
            o.Purpose == "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Debit);
            ViewBag.mussafacardsales = sales.Where(o => o.showroom == "MUSSAFA" &&
            o.Purpose == "Sale Payment" && o.Name.Contains("CARD")).Sum(o => o.Debit);
            ViewBag.mussafabanksales = sales.Where(o => o.showroom == "MUSSAFA" &&
            o.Purpose == "Sale Payment" && o.Name.Contains("BANK")).Sum(o => o.Debit);
            ViewBag.mussafacreditsales = creditsales.Where(o => o.shop == 20085).Select(o => o.cr).FirstOrDefault();
            ViewBag.mussafacashexpense = sales.Where(o => o.showroom == "MUSSAFA" &&
            o.Purpose != "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Credit);
            ViewBag.mussafacashincome = sales.Where(o => o.showroom == "MUSSAFA" &&
            o.Purpose != "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Debit);
            ViewBag.mussafanetcash = ViewBag.mussafacashincome + ViewBag.mussafacashsales - ViewBag.mussafacashexpense;

            ViewBag.abudhabicashsales = sales.Where(o => o.showroom == "ABUDHABI" &&
        o.Purpose == "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Debit);
            ViewBag.abudhabicardsales = sales.Where(o => o.showroom == "ABUDHABI" &&
            o.Purpose == "Sale Payment" && o.Name.Contains("CARD")).Sum(o => o.Debit);
            ViewBag.abudhabibanksales = sales.Where(o => o.showroom == "ABUDHABI" &&
            o.Purpose == "Sale Payment" && o.Name.Contains("BANK")).Sum(o => o.Debit);
            ViewBag.abudhabicreditsales = creditsales.Where(o => o.shop == 20086).Select(o => o.cr).FirstOrDefault();
            ViewBag.abudhabicashexpense = sales.Where(o => o.showroom == "ABUDHABI" &&
            o.Purpose != "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Credit);
            ViewBag.abudhabicashincome = sales.Where(o => o.showroom == "ABUDHABI" &&
            o.Purpose != "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Debit);
            ViewBag.abudhabinetcash = ViewBag.abudhabicashincome + ViewBag.abudhabicashsales - ViewBag.abudhabicashexpense;

            ViewBag.alaincashsales = sales.Where(o => o.showroom == "ALAIN" &&
        o.Purpose == "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Debit);
            ViewBag.alaincardsales = sales.Where(o => o.showroom == "ALAIN" &&
            o.Purpose == "Sale Payment" && o.Name.Contains("CARD")).Sum(o => o.Debit);
            ViewBag.alainbanksales = sales.Where(o => o.showroom == "ALAIN" &&
            o.Purpose == "Sale Payment" && o.Name.Contains("BANK")).Sum(o => o.Debit);
            ViewBag.alaincreditsales = creditsales.Where(o => o.shop == 20087).Select(o => o.cr).FirstOrDefault();
            ViewBag.alaincashexpense = sales.Where(o => o.showroom == "ALAIN" &&
            o.Purpose != "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Credit);
            ViewBag.alaincashincome = sales.Where(o => o.showroom == "ALAIN" &&
            o.Purpose != "Sale Payment" && o.Name.Contains("CASH")).Sum(o => o.Debit);
            ViewBag.alainnetcash = ViewBag.alaincashincome + ViewBag.alaincashsales - ViewBag.alaincashexpense;




            return View();
        }


        public ActionResult leadsnewdashboard(string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            DateTime? fdate = null;
            DateTime? tdate = null;


            if (fromdate != "" && fromdate != null)
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "" && todate != null)
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            int day = DateTime.Now.Date.Day - 1;
            if (fdate == null)
                fdate = DateTime.Now.Date.AddDays(-day);
            if (tdate == null)
                tdate = DateTime.Now;
            else
                tdate = tdate.Value.Date.AddHours(23);
            ViewBag.datefrom = Convert.ToDateTime(fdate).ToString("dd-MM-yyyy");
            ViewBag.dateto = Convert.ToDateTime(tdate).ToString("dd-MM-yyyy");
            LeadDashboardviewmodel vmodel = new LeadDashboardviewmodel();
            var v = (from a in (db.Customers
                                .Where(a => a.Type == CRMCustomerType.Leads && a.CreatedDate >= fdate &&
                                            a.CreatedDate <= tdate)
                                .Select(a => new { a.CustomerID })
                                .AsEnumerable())

                         //  let k = DbFunctionsCompat.AddMinutes(((a.EndTime == null) ? a.logtime : a.EndTime), b.duration)

                     let upt = (from x in db.LeadTaskUpdations
                                join y in db.LeadStatuss on x.leadstatus equals y.LeadStatusID
                                where x.TaskId == a.CustomerID

                                group new { y.StatusType, y.LeadStatusID, x.TaskId } by new { y.LeadStatusID, x.TaskId } into grps
                                select new
                                {
                                    st = grps.Key,
                                    win = grps.Where(o => o.LeadStatusID == 9).GroupBy(xx => xx.TaskId).Count(),
                                    loss = grps.Where(o => o.LeadStatusID == 20019).GroupBy(xx => xx.TaskId).Count(),
                                    balance = grps.Select(o => o.TaskId).FirstOrDefault(),

                                }
                              ).ToList()


                     select new LeadDashboardviewmodel
                     {
                         Totalbalance = upt.Select(o => o.balance).Count(),
                         TotalSuccess = upt.Select(o => o.win).Sum(),




                         Totalfaled = upt.Select(o => o.loss).Sum(),


                     }).ToList();

            var vemployeeby = (from a in (db.Customers
                                          .Where(a => a.Type == CRMCustomerType.Leads && a.CreatedDate >= fdate &&
                                                      a.CreatedDate <= tdate)
                                          .Select(a => new { a.CustomerID })
                                          .AsEnumerable())

                                   //  let k = DbFunctionsCompat.AddMinutes(((a.EndTime == null) ? a.logtime : a.EndTime), b.duration)

                               let upt = (from x in db.LeadTaskUpdations
                                          join y in db.LeadStatuss on x.leadstatus equals y.LeadStatusID
                                          join z in db.Employees on x.CreatedBy equals z.UserId
                                          where x.TaskId == a.CustomerID

                                          group new { y.StatusType, y.LeadStatusID, z.UserId, x.TaskId } by new { y.LeadStatusID, x.CreatedBy, x.TaskId } into grps
                                          select new
                                          {
                                              st = grps.Key,
                                              win = grps.Where(o => o.LeadStatusID == 9).GroupBy(xx => xx.TaskId).Count(),
                                              loss = grps.Where(o => o.LeadStatusID == 20019).GroupBy(xx => xx.TaskId).Count(),
                                              balance = grps.Where(o => o.LeadStatusID != 20019 && o.LeadStatusID != 9).GroupBy(xx => xx.TaskId).Count(),
                                              taskid = grps.Select(o => o.TaskId).FirstOrDefault(),
                                              empid = db.Employees.Where(o => o.UserId == grps.FirstOrDefault().UserId).Select(o => new { empname = o.EmployeeId }).FirstOrDefault().empname,
                                              empname = db.Employees.Where(o => o.UserId == grps.FirstOrDefault().UserId).Select(o => new { empname = o.FirstName + " " + o.LastName }).FirstOrDefault().empname
                                          }
                                        ).ToList()


                               select new LeadDashboardemp
                               {
                                   Totalbalance = upt.Select(o => o.balance).Distinct().Count(),
                                   TotalSuccess = upt.Select(o => new { o.taskid, o.win }).Distinct().Select(o => o.win).Sum(),
                                   Totalfaled = upt.Select(o => o.loss).Sum(),
                                   EmployeeName = upt.Select(o => o.empname).FirstOrDefault(),
                                   empid = upt.Select(o => o.empid).FirstOrDefault(),





                               }



                             ).ToList();
            var vvemployeeby = (from a in vemployeeby
                                group new { a.EmployeeName, a.empid, a.TotalSuccess, a.Totalfaled, a.Totalbalance } by new { a.EmployeeName, a.taskid } into grps
                                select new LeadDashboardemp
                                {
                                    EmployeeName = grps.FirstOrDefault().EmployeeName,
                                    empid = grps.FirstOrDefault().empid,
                                    TotalSuccess = grps.Sum(o => o.TotalSuccess),
                                    Totalfaled = grps.Sum(o => o.Totalfaled),
                                    Totalbalance = grps.Select(o => o.Totalbalance).Count(),

                                }
                           ).ToList();


            var vsrcby = (from a in (db.Customers
                                     .Where(a => a.Type == CRMCustomerType.Leads && a.CreatedDate >= fdate &&
                                                 a.CreatedDate <= tdate)
                                     .Select(a => new { a.CustomerID, a.SourceOfLead })
                                     .AsEnumerable())

                              //  let k = DbFunctionsCompat.AddMinutes(((a.EndTime == null) ? a.logtime : a.EndTime), b.duration)

                          let upt = (from x in db.LeadTaskUpdations
                                     join y in db.LeadStatuss on x.leadstatus equals y.LeadStatusID
                                     join s in db.SourceOfLeads on a.SourceOfLead equals s.SourceOfLeadId
                                     where x.TaskId == a.CustomerID

                                     group new { y.StatusType, y.LeadStatusID, s.SrcName, x.TaskId } by y.LeadStatusID into grps
                                     select new
                                     {
                                         st = grps.Key,
                                         win = grps.Where(o => o.LeadStatusID == 9).GroupBy(xx => xx.TaskId).Count(),
                                         loss = grps.Where(o => o.LeadStatusID == 20019).GroupBy(xx => xx.TaskId).Count(),
                                         balance = grps.Select(o => o.TaskId).FirstOrDefault(),



                                         SourceName = grps.FirstOrDefault().SrcName, //db.Employees.Where(o => o.UserId == grps.FirstOrDefault().UserId).Select(o => new { empname = o.FirstName + " " + o.LastName }).FirstOrDefault().empname
                                     }
                                   ).ToList()


                          select new LeadDashboardsource
                          {
                              Totalbalance = upt.Select(o => o.balance).Distinct().Count(),
                              TotalSuccess = upt.Select(o => o.win).Sum(),
                              Totalfaled = upt.Select(o => o.loss).Sum(),
                              SourceName = upt.Select(o => o.SourceName).FirstOrDefault()






                          }



                             ).ToList();
            var vvsourceby = (from a in vsrcby
                              group new { a.TotalSuccess, a.Totalfaled, a.Totalbalance, a.SourceName } by a.SourceName into grps
                              select new LeadDashboardsource
                              {
                                  SourceName = grps.FirstOrDefault().SourceName,

                                  TotalSuccess = grps.Sum(o => o.TotalSuccess),
                                  Totalfaled = grps.Sum(o => o.Totalfaled),
                                  Totalbalance = grps.Select(o => o.Totalbalance).Count(),
                              }
                           ).ToList();
            /* vmodel.TotalSuccess = v[0].sucess;
             vmodel.Totalfaled = v[0].fail;
             vmodel.Totalbalance = v[0].balance;
             vmodel.GrandTotal = v[0].GrandTotal;*/
            /* GroupBy(o => new { o.satusid }, (y, group) => new
             {
                 statusname = group.FirstOrDefault().statusname,
                 count = group.Count(),
                 wincount = group.Where(o => o.satusid == 9).Count(),
                 losscount = group.Where(o => o.satusid == 20019).Count(),
                 statusid = y.satusid,
                 customerid = group.FirstOrDefault().custid
             }).Distinct().ToList();*/
            LeadDashboardviewmodel final = new LeadDashboardviewmodel();
            final.Totalbalance = v.Select(o => o.Totalbalance).Count();
            final.Totalfaled = v.Sum(o => o.Totalfaled);
            final.TotalSuccess = v.Sum(o => o.TotalSuccess);
            final.GrandTotal = final.Totalbalance + final.TotalSuccess + final.Totalfaled;
            final.empdash = vvemployeeby;
            final.srcdash = vvsourceby;
            //let assignemp = (from f in db.AssignedTos
            //                 where f.CustomerID == a.CustomerID
            //               && f.Status == "Assigned" && f.ChkStatus == 0
            //                     empnames = gg.FirstName + " " + gg.LastName
            //                 })
            //         let sourceoflead = (from f in db.Employees
            //                             where f.EmployeeId == a.SourceOfLead
            //                                 empnames = f.FirstName + " " + f.LastName
            //                            }).FirstOrDefault()
            /*var emplist= (from f in db.AssignedTos
                        join ff in db.Customers on f.CustomerID equals ff.CustomerID
                          join gg in db.Employees on f.EmployeeId equals gg.EmployeeId
                          where  f.Status == "Assigned" && f.ChkStatus == 0 &&
                      cids.Contains(f.CustomerID)
                          select new
                          {
                              empame=gg.FirstName+" "+gg.LastName
                          }*/
            return View(final);
        }


        public ActionResult dashboard()
        {
            bool isquicknet = db.companys.Any(o => o.CPName.Contains("QUICK NET COMPUTERS"));
            if (!isquicknet)
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
                var excludeSalesparty = db.ParticularParties.Where(a => a.PartyType == 0).Select(a => a.PartyID).ToArray();
                var excludePurchaseparty = db.ParticularParties.Where(a => a.PartyType == 1).Select(a => a.PartyID).ToArray();
                HomeViewModel vmodel = new HomeViewModel();
                var allCustomer = User.IsInRole("All Customers");
                var userpermissionPayment = User.IsInRole("All Payment Entry");
                vmodel.totCustomerCount = Convert.ToString(db.Customers.Where(x => x.Type == CRMCustomerType.Customer && (allCustomer == true)).Count());

                var thisyearsale = db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var thisyearsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.SalesCredit = Convert.ToString(thisyearsale - thisyearsalertn);
                vmodel.ThisYearSalesRtn = Convert.ToString(thisyearsalertn);

                var todaysales = (from a in db.SalesEntrys
                                  join c in db.MCs on a.MaterialCenter equals c.MCId
                                  where !c.MCName.Contains("OLD-")
                                  select a)
                                 .Where(b => (EF.Functions.DateDiffDay(b.SEDate, today) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var todaysalesrtn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, today) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.todaySales = Convert.ToString(todaysales - todaysalesrtn);
                vmodel.dailySalesrtn = Convert.ToString(todaysalesrtn);

                var thismonthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where !c.MCName.Contains("OLD-")
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var thismonthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.ThisMonthSales = Convert.ToString(thismonthsale - thismonthsalertn);
                vmodel.ThismnthSalesRtn = Convert.ToString(thismonthsalertn);

                var lastmnthsale = (from a in db.SalesEntrys
                                    join c in db.MCs on a.MaterialCenter equals c.MCId
                                    where !c.MCName.Contains("OLD-")
                                    select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, LastMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var lastmnthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, LastMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastMonthSales = Convert.ToString(lastmnthsale - lastmnthsalertn);
                vmodel.LastMonthSalesrtn = Convert.ToString(lastmnthsalertn);
                var last2mnthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where !c.MCName.Contains("OLD-")
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last2ndMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var last2mnthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, Last2ndMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastTwoMonthSales = Convert.ToString(last2mnthsale - last2mnthsalertn);
                vmodel.LastTwoMonthSalesrtn = Convert.ToString(last2mnthsalertn);

                var last3mnthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where !c.MCName.Contains("OLD-")
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, last3rdMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var last3mnthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, last3rdMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastThreeMonthSales = Convert.ToString(last3mnthsale - last3mnthsalertn);
                vmodel.LastThreeMonthSalesrtn = Convert.ToString(last3mnthsalertn);

                var last4mnthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where !c.MCName.Contains("OLD-")
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last4thMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var last4mnthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, Last4thMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastFourMonthSales = Convert.ToString(last4mnthsale - last4mnthsalertn);
                vmodel.LastFourMonthSalesrtn = Convert.ToString(last4mnthsalertn);

                var last5mnthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where !c.MCName.Contains("OLD-")
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last5thMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var last5mnthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, Last5thMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastFiveMonthSales = Convert.ToString(last5mnthsale - last5mnthsalertn);
                vmodel.LastFiveMonthSalesrtn = Convert.ToString(last5mnthsalertn);



                return View(vmodel);
            }
           else
            
            {
                var UserId = User.Identity.GetUserId();
                var today = DateTime.Now;
                var CurrentYear = DateTime.Now.Year;
                var Currentmonth = DateTime.Now.Month;
                var Currentday = DateTime.Now.Day;
                var lastdate = today.AddDays(-30);
                long[] mcs = {20085,20086,20087};
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
                var excludeSalesparty = db.ParticularParties.Where(a => a.PartyType == 0).Select(a => a.PartyID).ToArray();
                var excludePurchaseparty = db.ParticularParties.Where(a => a.PartyType == 1).Select(a => a.PartyID).ToArray();
                HomeViewModel vmodel = new HomeViewModel();
                var allCustomer = User.IsInRole("All Customers");
                var userpermissionPayment = User.IsInRole("All Payment Entry");
                vmodel.totCustomerCount = Convert.ToString(db.Customers.Where(x => x.Type == CRMCustomerType.Customer && (allCustomer == true)).Count());

                var thisyearsale = db.SalesEntrys.Where(b => (EF.Functions.DateDiffDay(b.SEDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var thisyearsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, ThisYear1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.SalesCredit = Convert.ToString(thisyearsale - thisyearsalertn);
                vmodel.ThisYearSalesRtn = Convert.ToString(thisyearsalertn);

                var todaysales = (from a in db.SalesEntrys
                                  join c in db.MCs on a.MaterialCenter equals c.MCId
                                  where mcs.Contains((long)a.MaterialCenter)
                                  select a)
                                 .Where(b => (EF.Functions.DateDiffDay(b.SEDate, today) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var todaysalesrtn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, today) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.todaySales = Convert.ToString(todaysales );
                vmodel.dailySalesrtn = Convert.ToString(todaysalesrtn);

                var thismonthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where mcs.Contains((long)a.MaterialCenter)
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var thismonthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, ThisMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, today) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.ThisMonthSales = Convert.ToString(thismonthsale - thismonthsalertn);
                vmodel.ThismnthSalesRtn = Convert.ToString(thismonthsalertn);

                var lastmnthsale = (from a in db.SalesEntrys
                                    join c in db.MCs on a.MaterialCenter equals c.MCId
                                    where mcs.Contains((long)a.MaterialCenter)
                                    select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, LastMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var lastmnthsalertn = db.SalesReturns.Where(o => mcs.Contains((long) o.MaterialCenter)).Where(b => (EF.Functions.DateDiffDay(b.SRDate, LastMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, LastMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastMonthSales = Convert.ToString(lastmnthsale - lastmnthsalertn);
                vmodel.LastMonthSalesrtn = Convert.ToString(lastmnthsalertn);
                var last2mnthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where mcs.Contains((long)a.MaterialCenter)
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last2ndMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var last2mnthsalertn = db.SalesReturns.Where(b => (EF.Functions.DateDiffDay(b.SRDate, Last2ndMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, Last2ndMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastTwoMonthSales = Convert.ToString(last2mnthsale - last2mnthsalertn);
                vmodel.LastTwoMonthSalesrtn = Convert.ToString(last2mnthsalertn);

                var last3mnthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where mcs.Contains((long)a.MaterialCenter)
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, last3rdMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var last3mnthsalertn = db.SalesReturns.Where(o => mcs.Contains((long)o.MaterialCenter)).Where(b => (EF.Functions.DateDiffDay(b.SRDate, last3rdMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, last3rdMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastThreeMonthSales = Convert.ToString(last3mnthsale - last3mnthsalertn);
                vmodel.LastThreeMonthSalesrtn = Convert.ToString(last3mnthsalertn);

                var last4mnthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where mcs.Contains((long)a.MaterialCenter)
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last4thMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var last4mnthsalertn = db.SalesReturns.Where(o => mcs.Contains((long)o.MaterialCenter)).Where(b => (EF.Functions.DateDiffDay(b.SRDate, Last4thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, Last4thMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastFourMonthSales = Convert.ToString(last4mnthsale - last4mnthsalertn);
                vmodel.LastFourMonthSalesrtn = Convert.ToString(last4mnthsalertn);

                var last5mnthsale = (from a in db.SalesEntrys
                                     join c in db.MCs on a.MaterialCenter equals c.MCId
                                     where mcs.Contains((long)a.MaterialCenter)
                                     select a).Where(b => (EF.Functions.DateDiffDay(b.SEDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SEDate, Last5thMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SEGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                var last5mnthsalertn = db.SalesReturns.Where(o => mcs.Contains((long)o.MaterialCenter)).Where(b => (EF.Functions.DateDiffDay(b.SRDate, Last5thMnth1stDay) <= 0 && EF.Functions.DateDiffDay(b.SRDate, Last5thMnthLastDay) >= 0) && (!excludeSalesparty.Contains(b.Customer))).Select(b => b.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
                vmodel.LastFiveMonthSales = Convert.ToString(last5mnthsale - last5mnthsalertn);
                vmodel.LastFiveMonthSalesrtn = Convert.ToString(last5mnthsalertn);



                return View(vmodel);
            }
        }
        public ActionResult modallist()
        {
            return View();
        }
        public ActionResult GetAllLeadsdash(long leadid)
        {

            int days = 0;

            DateTime datecheck = DateTime.Now.AddDays(-days);

            DateTime datenow = DateTime.Now;
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = "ldate";
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            int taken = 300;

            // Perf + stability (audit P2, batch 7): the original single query computed `let g` (latest
            // LeadTaskUpdation), `let m` (latest LogManagers row, string-keyed) and the assigned-employee
            // collection PER ROW server-side — on a 3,900-lead status it exceeded the 30 s SQL command
            // timeout, so the dashboard drill-down failed outright (and took ~4 s even for 17 rows).
            // Restructured to the proven lead-grid shape: translatable base query -> set-based lookups ->
            // client ldate + TOP `taken` -> contact prefetch. Row definitions unchanged (byte-identical
            // golden on the old-reachable small statuses, both companies, 2026-06-13).
            var dashBase = (from a in db.Customers
                     join b in db.Contacts on a.Contact equals b.ContactID into Contact
                     from b in Contact.DefaultIfEmpty()
                     join c in db.SourceOfLeads on a.SourceOfLead equals c.SourceOfLeadId into SOL
                     from c in SOL.DefaultIfEmpty()
                     join d in db.CustomerConversions on a.CustomerID equals d.CustomerID into cs
                     from d in cs.DefaultIfEmpty()
                     join i in db.Accountss on a.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where
                     a.Type == CRMCustomerType.Leads && a.CurrentAction == leadid
                     select new
                     {
                         id = a.CustomerID,
                         LeadName = a.CustomerName,
                         LeadCode = a.CustomerCode,
                         //TaxRegNo = i.TRN,
                         TaxRegNo = a.TaxRegNo,
                         a.Location,
                         Phone = b.Phone,
                         //Mobile = b.Mobile,
                         Email = b.EmailId,
                         SrcLead = c.SrcName,
                         CreatedDate = (DateTime?)d.CreatedDate,
                     }).ToList();

            var dashIds = dashBase.Select(o => o.id).Distinct().ToList();
            var dashIdStrings = dashIds.Select(x => x.ToString()).ToList();
            // latest LeadTaskUpdation per lead (was `let g`)
            var dashGLookup = db.LeadTaskUpdations
                                .Where(lr => dashIds.Contains(lr.TaskId))
                                .ToList()
                                .GroupBy(lr => lr.TaskId)
                                .ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(lr => lr.TaskUpdationID).First());
            // latest Customers audit row per lead (was `let m`; LogID stores the lead id as a string)
            var dashMLookup = db.LogManagers
                                .Where(lg => lg.LogTable == "Customers" && dashIdStrings.Contains(lg.LogID))
                                .ToList()
                                .GroupBy(lg => lg.LogID)
                                .ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(lg => lg.LogManagerID).First());
            // assigned-employee names per lead (was the nested `lead` collection projection)
            var dashLeadAssignLookup = (from f in db.AssignedTos
                                        join g in db.Employees on f.EmployeeId equals g.EmployeeId into emps
                                        from g in emps.DefaultIfEmpty()
                                        where dashIds.Contains(f.CustomerID)
                                        && f.Status == "Assigned" && f.ChkStatus == 0
                                        select new { f.CustomerID, emp = g.FirstName })
                                       .ToList().ToLookup(o => o.CustomerID);

            var dashRows = dashBase.Select(o =>
                     {
                         var g = dashGLookup.ContainsKey(o.id) ? dashGLookup[o.id] : null;
                         var m = dashMLookup.ContainsKey(o.id.ToString()) ? dashMLookup[o.id.ToString()] : null;
                         var mLogTime = (m != null) ? (DateTime?)m.LogTime : null;
                         return new
                         {
                             o.id,
                             o.LeadName,
                             o.LeadCode,
                             o.TaxRegNo,
                             o.Location,
                             o.Phone,
                             o.Email,
                             ldate = ((g != null) && (g.CreatedDate > mLogTime)) ? g.CreatedDate : mLogTime,
                             o.SrcLead,
                             o.CreatedDate,
                             lead = dashLeadAssignLookup[o.id].Select(e => new { emp = e.emp }).ToList(),
                         };
                     // ThenBy(id): the old SQL ORDER BY returned the null/tied-ldate group in clustered-key
                     // (id ascending) order — make the client sort match it deterministically.
                     }).OrderByDescending(x => x.ldate).ThenBy(x => x.id).Take(taken).ToList();

            // contact numbers for just the surviving rows (was a per-row query in the projection below)
            var dashVIds = dashRows.Select(o => o.id).Distinct().ToList();
            var dashMobLookup = (from co in db.Contacts
                                 join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                 where rrr.RelationType == 2 && dashVIds.Contains(rrr.RelationID)
                                 select new { rrr.RelationID, Num = co.Mobile, Name = co.FirstName + " " + co.LastName, emails = co.EmailId })
                                .ToList().ToLookup(x => x.RelationID);

            var v = dashRows.Select(o => new
                     {
                         o.id,

                         o.CreatedDate,
                         o.Email,
                         o.ldate,
                         o.lead,
                         o.LeadCode,
                         o.LeadName,
                         o.Location,
                         o.Phone,
                         o.SrcLead,
                         o.TaxRegNo,
                         mobmodel = dashMobLookup[o.id].Select(x => new { x.Num, x.Name, x.emails }).ToList()


                     });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.LeadName.ToString().ToLower().Contains(search.ToLower()));
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
        public ActionResult GetAllLeadsexp(long leadid)
        {

            int days = 0;

            DateTime datecheck = DateTime.Now.AddDays(-days);

            DateTime datenow = DateTime.Now;
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = "ldate";
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            //           group new { l.CustomerID, l.CreatedDate } by new { l.CustomerID, l.CreatedDate } into g
            //               Customer = g.FirstOrDefault().CustomerID,
            //               createdate = g.FirstOrDefault().CreatedDate,



            //          let k = DbFunctionsCompat.AddMinutes(l.createdate, a.duration)


            //          group new {  c.CurrentAction, k } by new { l.Customer } into g


            //              customerid = g.Key.Customer,
            //              ndate = g.FirstOrDefault().k,
            //       //       leadstatusid=g.FirstOrDefault().LeadStatusID



            // ).Where(o => o.ndate < System.DateTime.Now).Select(o => new

            //     customer = o.customerid,
            //     //o.leadstatusid



            //           group new { l.CustomerID, l.CreatedDate } by new { l.CustomerID, l.CreatedDate } into g
            //               Customer = g.FirstOrDefault().CustomerID,
            //               createdate = g.FirstOrDefault().CreatedDate,


            //          let k = DbFunctionsCompat.AddMinutes(l.createdate, a.duration)


            //          group new { c.CurrentAction, b.StatusType, k } by new { l.Customer } into g


            //              statusname = g.Key.Customer,
            //              satusid = g.FirstOrDefault().CurrentAction,
            //              ndate = g.FirstOrDefault().k



            // ).Where(o => o.ndate < System.DateTime.Now).Select(o => new paramclass

            //     //statusname = o.statusname,

            //     satusid = o.statusname,
            //     count = o.count,


            // (audit P2, batch 7) DbFunctionsCompat.AddMinutes has no EF Core SQL mapping — this endpoint
            // threw on every call since the port. DateTime.AddMinutes translates to DATEADD(minute, ...),
            // the same SQL legacy EF6 DbFunctions produced; the keyed GroupBy collapses to Distinct().
            var vv = (from a in db.Customers
                      join b in db.leaddashbordorder on a.CurrentAction equals b.lead
                      join c in db.LeadStatuss on a.CurrentAction equals c.LeadStatusID
                      let k = ((a.EndTime == null) ? a.logtime : a.EndTime).Value.AddMinutes((double)b.duration)
                      where a.Type == CRMCustomerType.Leads && a.CurrentAction == leadid
                      && a.OpenClose!=1
                      select new
                      {
                          dt = (DateTime?)k,
                          a.CustomerID

                      }).Where(o => o.dt < System.DateTime.Now)
                        .Select(o => o.CustomerID).Distinct()
                        .Select(o => new
                      {
                          customer = o
                      });
            // Bug fix + perf (audit P2, batch 7): EF Core cannot translate this projection (the
            // Mobiles-UNION-contacts collection) — the endpoint threw InvalidOperationException on
            // EVERY call since the port (legacy EF6 evaluated it client-side). Restructured to the
            // proven lead-grid shape: translatable base query -> set-based lookups -> client
            // projection. Row definitions are unchanged from the original LINQ.
            var expBase = (from a in db.Customers
                     join vvv in vv on a.CustomerID equals vvv.customer
                     join b in db.Contacts on a.Contact equals b.ContactID into Contact
                     from b in Contact.DefaultIfEmpty()
                     join c in db.SourceOfLeads on a.SourceOfLead equals c.SourceOfLeadId into SOL
                     from c in SOL.DefaultIfEmpty()
                     join d in db.CustomerConversions on a.CustomerID equals d.CustomerID into cs
                     from d in cs.DefaultIfEmpty()
                     join i in db.Accountss on a.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where
                     a.Type == CRMCustomerType.Leads && a.CurrentAction == leadid
                      && a.OpenClose != 1
                     select new
                     {
                         id = a.CustomerID,
                         LeadName = a.CustomerName,
                         LeadCode = a.CustomerCode,
                         //TaxRegNo = i.TRN,
                         TaxRegNo = a.TaxRegNo,
                         a.Location,
                         Phone = b.Phone,
                         //Mobile = b.Mobile,
                         Email = b.EmailId,
                         SrcLead = c.SrcName,
                         CreatedDate = (DateTime?)d.CreatedDate,
                         ContactKey = a.Contact,
                     }).ToList();

            var expIds = expBase.Select(o => o.id).Distinct().ToList();
            var expIdStrings = expIds.Select(x => x.ToString()).ToList();
            var expContactKeys = expBase.Select(o => o.ContactKey).Distinct().ToList();
            // latest LeadTaskUpdation per lead (was `let g`)
            var expGLookup = db.LeadTaskUpdations
                               .Where(lr => expIds.Contains(lr.TaskId))
                               .ToList()
                               .GroupBy(lr => lr.TaskId)
                               .ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(lr => lr.TaskUpdationID).First());
            // latest Customers audit row per lead (was `let m`; LogID stores the lead id as a string)
            var expMLookup = db.LogManagers
                               .Where(lg => lg.LogTable == "Customers" && expIdStrings.Contains(lg.LogID))
                               .ToList()
                               .GroupBy(lg => lg.LogID)
                               .ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(lg => lg.LogManagerID).First());
            // assigned-employee names per lead (was the nested `lead` collection projection)
            var expAssignLookup = (from f in db.AssignedTos
                                   join g in db.Employees on f.EmployeeId equals g.EmployeeId into emps
                                   from g in emps.DefaultIfEmpty()
                                   where expIds.Contains(f.CustomerID)
                                   && f.Status == "Assigned" && f.ChkStatus == 0
                                   select new { f.CustomerID, emp = g.FirstName })
                                  .ToList().ToLookup(o => o.CustomerID);
            // contact-numbers per lead (was `let mob`)
            var expRelLookup = (from co in db.Contacts
                                join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                where rrr.RelationType == 2 && expIds.Contains(rrr.RelationID)
                                select new { rrr.RelationID, Num = co.Mobile, Name = co.FirstName + " " + co.LastName, emails = co.EmailId })
                               .ToList().ToLookup(x => x.RelationID);
            // legacy Mobiles rows per contact (first half of the mobmodel UNION)
            var expMobilesLookup = db.Mobiles
                                     .Where(ac => expContactKeys.Contains(ac.Contact))
                                     .ToList()
                                     .ToLookup(ac => ac.Contact);

            var v = expBase.Select(o =>
                     {
                         var g = expGLookup.ContainsKey(o.id) ? expGLookup[o.id] : null;
                         var m = expMLookup.ContainsKey(o.id.ToString()) ? expMLookup[o.id.ToString()] : null;
                         var mLogTime = (m != null) ? (DateTime?)m.LogTime : null;
                         return new
                         {
                             id = o.id,
                             LeadName = o.LeadName,
                             LeadCode = o.LeadCode,
                             TaxRegNo = o.TaxRegNo,
                             o.Location,
                             Phone = o.Phone,
                             Email = o.Email,
                             ldate = ((g != null) && (g.CreatedDate > mLogTime)) ? g.CreatedDate : mLogTime,
                             SrcLead = o.SrcLead,
                             CreatedDate = o.CreatedDate,
                             lead = expAssignLookup[o.id].Select(e => new { emp = e.emp }).ToList(),
                             mobmodel = expMobilesLookup[o.ContactKey]
                                            .Select(ac => new
                                            {
                                                Num = (ac.Name == "" || ac.Name == null) ? ac.MobileNum : ac.MobileNum + "-" + ac.Name,
                                                Name = ac.Name,
                                                emails = o.Email,
                                            })
                                            .Union(expRelLookup[o.id].Select(x => new { x.Num, x.Name, x.emails }))
                                            .ToList(),
                         };
                     });

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.LeadName.ToString().ToLower().Contains(search.ToLower()));
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
        public ActionResult AddamcImage(long? id)
        {
            amcdocattach tsk = new amcdocattach();
            tsk.amcid = (long)id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var docdownload = db.LeadDocuments.Where(a => a.CustomerID == id).ToList();
            if (1 == 2)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.LeadDocuments
                                           join b in db.DocumentTypes on m.DocumentTypeID equals b.ID into temp1
                                           from b in temp1.DefaultIfEmpty()
                                           where id == m.CustomerID
                                           orderby m.DoucumentType
                                           select new Multiviewmodel
                                           {
                                               documentid = (from aa in db.LeadDocuments
                                                             where
                                   aa.CustomerID == m.CustomerID && aa.FileName == m.FileName
                                                             select new
                                                             {
                                                                 aa.LeadDocumentId
                                                             }).Max(o => o.LeadDocumentId),
                                               Id = m.CustomerID,
                                               Document = m.FileName,
                                               filenamelead = m.FileName,
                                               DocumentName = (b.Name == null || b.Name == "") ? m.Notes : b.Name,
                                               Documentview = m.DoucumentType,

                                           }
                                        ).Distinct().ToList();
                tsk.filedoc = filedoc;


            }

            return PartialView(tsk);
        }
        public ActionResult DownloadDoc(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var docdownload = db.LeadDocuments.Where(a => a.CustomerID == id).ToList();
            if (docdownload.Count == 0)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.LeadDocuments
                                           join b in db.DocumentTypes on m.DocumentTypeID equals b.ID into temp1
                                           from b in temp1.DefaultIfEmpty()
                                           where id == m.CustomerID
                                           //orderby m.TransType 
                                           select new Multiviewmodel
                                           {

                                               Id = m.CustomerID,
                                               Document = m.FileName,
                                               filenamelead = m.FileName,
                                               DocumentName = (b.Name == null || b.Name == "") ? m.Notes : b.Name,
                                               Documentview = m.DoucumentType,

                                           }
                                        ).ToList();


                return PartialView(filedoc);
            }
        }
        public int AddDocumentsdocadd(amcdocattach AmcViewModel, long AmcId, DateTime Today, string Mode)
        {
            var UserId = User.Identity.GetUserId();

            if (AmcViewModel.AmcDocuments != null && AmcViewModel.AmcDocuments.Count > 0)
            {
                var fileName = "";
                int i = 0;

                foreach (var item in AmcViewModel.AmcDocuments)
                {
                    //In Create Mode and when FileName changes in Edit Mode
                    if (item.FileName != null)
                    {
                        //Files upload
                        IFormFile file = item.FileName;
                        fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);

                        var uploadUrl = LegacyWeb.MapPath("~/uploads/leaddocuments/");

                        if (!System.IO.Directory.Exists(uploadUrl))
                            System.IO.Directory.CreateDirectory(uploadUrl);
                        file.SaveAs(Path.Combine(uploadUrl, fileName));

                        //To remove the previously attached file from folder
                        if (item.FileNameAmc != null)
                        {
                            string FullPath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + item.FileNameAmc);

                            if (System.IO.File.Exists(FullPath))
                            {
                                System.IO.File.Delete(FullPath);
                            }
                        }
                    }
                    //If already file name exists(Edit Mode)
                    else
                    {
                        fileName = item.FileNameAmc;
                    }
                    i++;

                    LeadDocument Documents = new LeadDocument
                    {
                        CustomerID = AmcId,
                        DocumentTypeID = (long)item.DocumentTypeID,
                        Expiry =(DateTime) item.Expiry,
                        Notes = item.Notes,
                        
                        FileName = fileName,
                        Status = Status.active,
                        CreatedDate = Today
                    };
                    db.LeadDocuments.Add(Documents);
                    db.SaveChanges();
                }
            }
            com.updateleaddate(AmcId);
            return db.SaveChanges();
            com.addlog(LogTypes.Updated, UserId, "lead document", "lead document", findip(), AmcId);
        }
        public ActionResult DeleteImage(long id)
        {
            AmcDocumentViewModel vmodel = new AmcDocumentViewModel();
            vmodel.DocumentId = id;
            return PartialView(vmodel);
        }

        //Function to delete image from details window(POST)
        public ActionResult DeleteAmcImage(AmcDocumentViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var Img = db.LeadDocuments.Where(a => a.LeadDocumentId == vmodel.DocumentId).FirstOrDefault();
            if (Img != null)
            {
                string fullPath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + Img.FileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                //To remove resize and thumb files from folder
                string fullPaththumb = LegacyWeb.MapPath("~/uploads/leaddocuments/" + "thumb_" + Img.FileName);
                if (System.IO.File.Exists(fullPaththumb))
                {
                    System.IO.File.Delete(fullPaththumb);
                }

                string ResizePath = LegacyWeb.MapPath("~/uploads/leaddocuments/resize_" + Img.FileName);

                if (System.IO.File.Exists(ResizePath))
                {
                    System.IO.File.Delete(ResizePath);
                }
                var amndel = db.LeadDocuments.Where(a => a.CustomerID == Img.CustomerID && Img.FileName == a.FileName);
                db.LeadDocuments.RemoveRange(amndel);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();

                com.addlog(LogTypes.Deleted, UserId, "Leads", "LeadsDocument", findip(), Img.LeadDocumentId, "Leads Image Deleted Successfully");

                stat = true;
                msg = "Successfully deleted Leads Image.";
            }
            else
            {
                stat = false;
                msg = "Leads Image Not Found..!!";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost]
        public ActionResult Editdoc(amcdocattach AmcModel)
        {

            // For Saving Amc Documents
            if (AmcModel.AmcDocuments != null && AmcModel.AmcDocuments.Count > 0)
            {
                AddDocumentsdocadd(AmcModel, AmcModel.amcid, System.DateTime.Now, "Edit");
            }
            return Redirect(ControllerContext.HttpContext.Request.GetUrlReferrer().ToString());

        }
        // [QkAuthorize(Roles = "Dev,Leads List")]
        public ActionResult Index()
        {
            ViewBag.LeadName = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.LeadSource = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.CreatedBy = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.Location = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.LeadStatus = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);



            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);


            ViewBag.Mobile = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                            },
                              "Value", "Text", 1);

            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));
            ViewBag.TaskStatus = QkSelect.List(Enum.GetValues(typeof(TKUpdateStatus)));
            LeadsViewModel cusmodel = new LeadsViewModel();
            cusmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Leads" && a.Status == Status.active).ToList();
            if (cusmodel.FieldMap.Count() > 0)
                ViewBag.opref1 = cusmodel.FieldMap[0].FieldName;
            else
                ViewBag.opref1 = "";
            if (cusmodel.FieldMap.Count() > 1)
                @ViewBag.opref2 = cusmodel.FieldMap[1].FieldName;
            else
                @ViewBag.opref2 = "";
            cusmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Leads").ToList();
            var ref1 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Customers
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            return View(cusmodel);
        }
        public ActionResult leadreport()
        {
            ViewBag.LeadName = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.LeadSource = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.CreatedBy = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.Location = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.LeadStatus = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);



            ViewBag.AssignedTo = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);


            ViewBag.Mobile = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                            },
                              "Value", "Text", 1);

            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));
            ViewBag.TaskStatus = QkSelect.List(Enum.GetValues(typeof(TKUpdateStatus)));
            LeadsViewModel cusmodel = new LeadsViewModel();
            cusmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Leads" && a.Status == Status.active).ToList();
            if (cusmodel.FieldMap.Count() > 0)
                ViewBag.opref1 = cusmodel.FieldMap[0].FieldName;
            else
                ViewBag.opref1 = "";
            if (cusmodel.FieldMap.Count() > 1)
                @ViewBag.opref2 = cusmodel.FieldMap[1].FieldName;
            else
                @ViewBag.opref2 = "";
            cusmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Leads").ToList();
            var ref1 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Customers
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            return View(cusmodel);
        }

        [HttpPost]
        //  [QkAuthorize(Roles = "Dev,Leads List")]
        public ActionResult GetAllLeads(long flag, string LeadCode, long? LeadName, long? LeadSource, string CreatedBy, string Location, long? LeadStatus, long? LeadCustomer, long? AssignedTo, string LeadLevel, string Mobile, string fromdate, string todate, string StartDate, string EndDate, string LastUpdDays, string ref1, string ref2, string ref3, string ref4, string ref5, string nextdate, string opncls, string apfrom, string apto)
        {
            // BUG A fix: the "All" option posts AssignedTo = 0 (not null). Treat 0 as "no filter".
            if (AssignedTo == 0) AssignedTo = null;

            int days = 0;
            int taken = 100000;
            DateTime today = System.DateTime.Now.Date;
            if (LastUpdDays != "")
            {
                days = Convert.ToInt32(LastUpdDays);
            }
            int OandC = 0;
            if (opncls != "")
            {
                OandC = Convert.ToInt32(opncls);
            }
            DateTime datecheck = DateTime.Now.AddDays(-days);

            DateTime datenow = DateTime.Now;
            string search = Request.Form.GetValues("search[value]")[0];
            if (search != "")
            {
                flag = 0;
            }
            if (ref3 == "")
                ref3 = null;
            if (LeadCode == "" && LeadName == 0 && LeadSource == 0 && CreatedBy == "0" && Location == "" && LeadStatus == null && LeadCustomer == 0 && AssignedTo == null && LeadLevel == "All" && Mobile == "All" && fromdate == "" && todate == "" && StartDate == "" && EndDate == "" && LastUpdDays == "0" && ref1 == "" && ref2 == "" && ref3 == null && ref4 == null && ref5 == null && nextdate == "")
            {
                flag = 0;
            }
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? sdate = null;
            DateTime? edate = null;
            DateTime? ndate = null;
            DateTime? apfromdate = null;
            DateTime? aptodate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (nextdate != "")
            {
                ndate = DateTime.Parse(nextdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (StartDate != "")
            {
                sdate = DateTime.Parse(StartDate, new CultureInfo("en-GB"));
            }
            if (EndDate != "")
            {
                edate = DateTime.Parse(EndDate, new CultureInfo("en-GB"));
            }
            if (apfrom != "")
            {
                apfromdate = DateTime.Parse(apfrom, new CultureInfo("en-GB"));
            }
            if (apto != "")
            {
                aptodate = DateTime.Parse(apto, new CultureInfo("en-GB"));
            }
            DateTime crdate = DateTime.Now.AddDays(-365);
            var xx = db.AssignedTos.Where(cl => cl.Status == "Assigned" && cl.ChkStatus == 0 && cl.EmployeeId == AssignedTo).Select(o => o.CustomerID).Distinct().ToList().ToArray();

            if (AssignedTo != null)
                xx = db.AssignedTos.Where(cl => cl.Status == "Assigned" && cl.ChkStatus == 0 && cl.EmployeeId == AssignedTo).Select(o => o.CustomerID).Distinct().ToList().ToArray();
            if (OandC == 1 && AssignedTo != null)
            {

                xx = db.AssignedTos.Where(cl => cl.EmployeeId == AssignedTo).Select(o => o.CustomerID).Distinct().ToList().ToArray();

            }
            bool ref1greater, ref1lessthan, ref2greater, ref2lessthan, ref3greater, ref3lessthan, ref4greater, ref4lessthan, ref5greater, ref5lessthan;
            bool ref1number, ref2number, ref3number, ref4number, ref5number;
            decimal ref1numberc, ref2numberc, ref3numberc, ref4numberc, ref5numberc;
            bool ref1numbercless, ref2numbercless, ref3numbercless, ref4numbercless, ref5numbercless;
            ref1numberc = ref2numberc = ref3numberc = ref4numberc = ref5numberc = 0;
            ref1numbercless = ref2numbercless = ref3numbercless = ref4numbercless = ref5numbercless = false;
            ref1greater = ref1lessthan = ref2greater = ref2lessthan = ref3greater = ref3lessthan = ref4greater = ref4lessthan = ref5greater = ref5lessthan = false;
            ref1number = ref2number = ref3number = ref4number = ref5number = false;
            if (ref1 != "" && ref1 != null)
            {
                

                if (ref1.Contains(">"))
                {
                    ref1greater = true;
                    ref1numberc = Convert.ToDecimal(ref1.Replace("<", "").Replace(">", ""));
                    ref1number = true;

                }
                else if (ref1.Contains("<"))
                {
                    ref1greater = false;
                    ref1numberc = Convert.ToDecimal(ref1.Replace("<", "").Replace(">", ""));
                    ref1numbercless = true;
                    ref1number = true;

                }
                else
                    ref1number = false;
            }



            if (ref2 != "" && ref2 != null)
            {
               
                if (ref2.Contains(">"))
                {
                    ref2greater = true;
                    ref2numberc = Convert.ToDecimal(ref2.Replace("<", "").Replace(">", ""));
                    ref2number = true;

                }
                else if (ref2.Contains("<"))
                {
                    ref2greater = false;
                    ref2numberc = Convert.ToDecimal(ref2.Replace("<", "").Replace(">", ""));
                    ref2numbercless = true;
                    ref2number = true;
                }
                else
                {
                    ref2number = false;
                }
                }




                if (ref3 != "" && ref3 != null)
            {
              
                if (ref3.Contains(">"))
                {
                    ref3greater = true;
                    ref3numberc = Convert.ToDecimal(ref3.Replace("<", "").Replace(">", ""));
                    ref3number = true;


                }
                else if (ref3.Contains("<"))
                {
                    ref3greater = false;
                    ref3numberc = Convert.ToDecimal(ref3.Replace("<", "").Replace(">", ""));
                    ref3numbercless = true;
                    ref3number = true;
                }
                else
                {
                    ref3number = false;

                }
                }
                if (ref4 != "" && ref4 != null)
            {
             
               
                if (ref4.Contains(">"))
                {
                    ref4greater = true;
                    ref4numberc = Convert.ToDecimal(ref4.Replace("<", "").Replace(">", ""));
                    ref4number = true;
                }
                else if (ref4.Contains("<"))
                {
                    ref4greater = false;
                    ref4numberc = Convert.ToDecimal(ref4.Replace("<", "").Replace(">", ""));
                    ref4numbercless = true;
                    ref4number = true;
                }
                else
                {
                    ref4number = false;
                }
                }
            if (ref5 != "" && ref5 != null)
            {
              

               
                if (ref5.Contains(">"))
                {
                    ref5greater = true;
                    ref5numberc = Convert.ToDecimal(ref5.Replace("<", "").Replace(">", ""));
                    ref5number = true;
                }
                else if (ref5.Contains("<"))
                {

                    ref5numberc = Convert.ToDecimal(ref5.Replace("<", "").Replace(">", ""));
                    ref5greater = false;
                    ref5numbercless = true;
                    ref5number = true;
                }

                 

                    ref4number = false;
            }

            var userid = User.Identity.GetUserId();
            // EF Core 10 cannot translate the nested-collection projections (lead/appointemployee/mobmodel),
            // the collection-valued `let mob`, or the latest-per-lead `let g/gg` correlated subqueries.
            // SERVER query: keep the joins + the translatable WHERE; project entity columns + left-join scalars.
            // The g/gg-dependent WHERE conditions and the nested members are computed client-side below.
            var serverRows = (from a in db.Customers
                     join h in db.leadcustomerrelation on a.CustomerID equals h.leadid into lead
                     from h in lead.DefaultIfEmpty()
                     join c in db.SourceOfLeads on a.SourceOfLead equals c.SourceOfLeadId into SOL
                     from c in SOL.DefaultIfEmpty()
                     join d in db.CustomerConversions on a.CustomerID equals d.CustomerID into cs
                     from d in cs.DefaultIfEmpty()
                     join i in db.Accountss on a.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join l in db.LocationNames on a.LocationID equals l.LocationId into loc
                     from l in loc.DefaultIfEmpty()
                     join m in db.LeadTypes on a.LeadType equals m.TypeId into leadty
                     from m in leadty.DefaultIfEmpty()
                     join n in db.LeadCondition on a.LeadCondition equals n.id into ldcon
                      from n in ldcon.DefaultIfEmpty()
                     join nn in db.LeadStatuss on a.CurrentAction equals nn.LeadStatusID into ldconn
                     from nn in ldconn.DefaultIfEmpty()

                     where
                     a.Type == CRMCustomerType.Leads &&
                     (flag == 1 || a.logtime > crdate) &&
                     (LeadCode == null || LeadCode == "" || a.CustomerCode == LeadCode) &&
                     (LeadName == 0 || a.CustomerID == LeadName) &&
                     (LeadSource == 0 || a.SourceOfLead == LeadSource) &&
                     (LeadStatus == null || a.CurrentAction == LeadStatus) &&
                     (LeadCustomer == 0 || h.customerid == LeadCustomer) &&
                     (CreatedBy == "0" || d.CreatedUser == CreatedBy) &&
                     (LeadLevel == "All" || a.LeadLevel == LeadLevel) &&
                     (apfrom == "" || EF.Functions.DateDiffDay(a.StartDate, apfromdate) <= 0) &&
                     (apto == "" || EF.Functions.DateDiffDay(a.StartDate, aptodate) >= 0) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0) &&
                     (StartDate == "" || EF.Functions.DateDiffDay(a.StartDate, sdate) <= 0) &&
                     (EndDate == "" || (EF.Functions.DateDiffDay(a.EndDate, edate) >= 0 && a.logtime <= edate)) &&
                     // Mobile filter is re-applied client-side below via mobmodel (same as the original tail).
                     (Location == "" || a.Location.Contains(Location)) &&
                     (ref1 == "" || ref1 == null || (ref1number == true && (ref1greater == true || ref1numbercless == true) )|| a.Ref1 == ref1) &&
                     (ref2 == "" || ref2 == null || (ref2number == true && (ref2greater == true || ref2numbercless == true)) || a.Ref2 == ref2) &&
                     (ref3 == "" || ref3 == null || (ref3number == true && (ref3greater == true || ref3numbercless == true)) || a.Ref3 == ref3) &&
                     (ref4 == "" || ref4 == null || (ref4number == true && (ref4greater == true || ref4numbercless == true)) || a.Ref4 == ref4) &&
                     (ref5 == "" || ref5 == null || (ref5number == true && (ref5greater == true || ref5numbercless == true)) || a.Ref5 == ref5) &&
                     // (nextdate ...) and (days ...) filters reference the latest LeadTaskUpdation (g) -> moved client-side.
                     (opncls == "" || OandC == null || a.OpenClose == OandC)
                     &&
                     (AssignedTo == null || xx.Contains(a.CustomerID))
                     // (days == 0 || ((EF.Functions.DateDiffDay(g.CreatedDate,datecheck)<=0) && (EF.Functions.DateDiffDay(g.CreatedDate,datenow)>=0))
                     // || ((EF.Functions.DateDiffDay(m.LogTime, datecheck) <=0) && (EF.Functions.DateDiffDay(m.LogTime, datenow) >=0)))
                     select new
                     {
                         id = a.CustomerID,
                         LeadName = a.CustomerName,
                         LeadCode = a.CustomerCode,
                         TaxRegNo = a.TaxRegNo,
                         l.Location,
                         a.OpenClose,
                         nn.StatusType,
                         a.StartDate,
                         a.StartTime,
                         a.Ref1,
                         a.Ref2,
                         a.Ref3,
                         a.Ref4,
                         a.Ref5,
                         a.EndTime,
                         a.EndDate,
                         a.logtime,
                         SrcLead = c.SrcName,
                         CreatedDate = (DateTime?)d.CreatedDate,
                         leadtype = m.Type,
                         n.LeadCondition
                     }).ToList();

            // ---- client-side lookups (keyed by lead/customer id) for the parts EF Core 10 can't translate ----
            var leadIds = serverRows.Select(o => o.id).Distinct().ToList();
            // latest LeadTaskUpdation per lead (was `let g`)
            var gLookup = db.LeadTaskUpdations
                            .Where(lr => leadIds.Contains(lr.TaskId))
                            .ToList()
                            .GroupBy(lr => lr.TaskId)
                            .ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(lr => lr.TaskUpdationID).First());
            // latest "Appointment Modified" LeadTaskUpdation per lead (was `let gg`)
            var ggLookup = db.LeadTaskUpdations
                            .Where(lr => leadIds.Contains(lr.TaskId) && lr.Remarks == "Appointment Modified")
                            .ToList()
                            .GroupBy(lr => lr.TaskId)
                            .ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(lr => lr.TaskUpdationID).First());
            // assigned-employee names per lead (was the nested `lead` collection projection)
            var leadAssignLookup = (from f in db.AssignedTos
                                    join g in db.Employees on f.EmployeeId equals g.EmployeeId into emps
                                    from g in emps.DefaultIfEmpty()
                                    where leadIds.Contains(f.CustomerID)
                                    && f.Status == "Assigned" && f.ChkStatus == 0
                                    select new { f.CustomerID, emp = g.FirstName })
                                   .ToList().ToLookup(o => o.CustomerID);
            // employee display name by UserId (was the nested `appointemployee` subquery, keyed off gg.CreatedBy)
            var appointEmpLookup = (from kk in db.Employees
                                    join kkk in db.Users on kk.UserId equals kkk.Id
                                    select new { kkk.Id, empname = kk.FirstName + " " + kk.MiddleName + " " + kk.LastName })
                                   .ToList().ToLookup(o => o.Id);

            // g-dependent WHERE conditions (were `let g`-based: `g.nextdate == ndate` / `g.CreatedDate <= datecheck`)
            // applied client-side on the latest LeadTaskUpdation. A missing/null g => comparison is false (matches SQL).
            var filteredRows = serverRows.Where(o =>
            {
                var g = gLookup.ContainsKey(o.id) ? gLookup[o.id] : null;
                return (nextdate == "" || (g != null && g.nextdate == ndate))
                    && (days == 0 || (g != null && g.CreatedDate <= datecheck));
            }).ToList();

            // Perf (audit P2, batch 6): the second projection below used to run TWO correlated DB queries
            // PER ROW (custdetails + mobmodel) when the grid materialized — ~6,200 queries on a default
            // load (3,111 window leads) and ~30,000 with any filter set (all 15,213 leads, 10-13 s).
            // Prefetch both as single set-based queries over the surviving lead ids; projected shape and
            // per-lead row order are unchanged (byte-identical golden on both companies, 2026-06-13).
            var vIds = filteredRows.Select(o => o.id).Distinct().ToList();
            var custDetailsLookup = (from cc in db.Customers
                                     join dd in db.leadcustomerrelation on cc.CustomerID equals dd.customerid
                                     where vIds.Contains(dd.leadid)
                                     select new { dd.leadid, cc.CustomerID, CustomerName = cc.CustomerCode + " - " + cc.CustomerName })
                                    .ToList()
                                    .GroupBy(x => x.leadid)
                                    .ToDictionary(g2 => g2.Key, g2 => g2.Select(x => new { x.CustomerID, x.CustomerName }).First());
            var mobLookup = (from co in db.Contacts
                             join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                             join con in db.Country on co.CountryID equals con.CountryID into conn
                             from con in conn.DefaultIfEmpty()
                             where rrr.RelationType == 2 && vIds.Contains(rrr.RelationID)
                             select new { rrr.RelationID, Num = co.Mobile, Name = co.Name, emails = co.EmailId })
                            .ToList()
                            .ToLookup(x => x.RelationID);

            // ---- client re-projection to the ORIGINAL first-projection shape (so the downstream ref filter
            //      + second Select + search/sort/paging all bind unchanged) ----
            var v = filteredRows.Select(o =>
                     {
                         var g = gLookup.ContainsKey(o.id) ? gLookup[o.id] : null;
                         var gg = ggLookup.ContainsKey(o.id) ? ggLookup[o.id] : null;
                         var appointemployee = (gg != null && gg.CreatedBy != null)
                                                ? appointEmpLookup[gg.CreatedBy].Select(e => new { empname = e.empname }).FirstOrDefault()
                                                : null;
                         return new
                         {
                             id = o.id,
                             LeadName = o.LeadName,
                             LeadCode = o.LeadCode,
                             //TaxRegNo = i.TRN,
                             nextdate = (o.EndTime == null) ? (g != null ? g.nextdate : null) : o.EndTime,
                             finishdate = (g != null && g.finishtime != null) ? g.finishtime : o.EndDate,
                             TaxRegNo = o.TaxRegNo,
                             o.Location,
                             o.OpenClose,
                             o.StatusType,
                             o.StartDate,
                             o.StartTime,
                             o.Ref1,
                             o.Ref2,
                             o.Ref3,
                             o.Ref4,
                             o.Ref5,
                             ldate = ((g != null) && (g.CreatedDate > o.logtime)) ? g.CreatedDate : o.logtime,
                             appointemployee = appointemployee,
                             SrcLead = o.SrcLead,
                             CreatedDate = o.CreatedDate,
                             lead = leadAssignLookup[o.id].Select(e => new { emp = e.emp }).Distinct().ToList(),
                             leadtype = o.leadtype,
                             o.LeadCondition
                         };
                     })
                     .OrderByDescending(x => x.ldate).Take(taken).Where(o =>

                     ((ref1greater == false) ||ref1numberc<Convert.ToDecimal(o.Ref1))&&

                     ((ref2greater == false) || ref2numberc < Convert.ToDecimal(o.Ref2)) &&

                     (( ref3greater == false) || ref3numberc < Convert.ToDecimal(o.Ref3)) &&

                     (( ref4greater == false) || ref4numberc < Convert.ToDecimal(o.Ref4)) &&

                     (( ref5greater == false) || ref5numberc < Convert.ToDecimal(o.Ref5)) &&
                     ((ref1numbercless == false ) || ref1numberc > Convert.ToDecimal(o.Ref1)) &&

                     ((ref2numbercless == false ) || ref2numberc > Convert.ToDecimal(o.Ref2)) &&

                     ((ref3numbercless == false ) || ref3numberc > Convert.ToDecimal(o.Ref3)) &&

                     ((ref4numbercless == false ) || ref4numberc > Convert.ToDecimal(o.Ref4)) &&

                     ((ref5numbercless == false ) || ref5numberc > Convert.ToDecimal(o.Ref5))


                     ).
                     
                     Select(o => new
                     {
                         o.OpenClose,
                         o.id,
                         o.ldate,
                         o.lead,
                         o.LeadCode,
                         o.LeadName,
                         o.Location,
                         o.nextdate,
                         o.finishdate,
                         o.SrcLead,
                         o.Ref1,
                         o.Ref2,
                         o.Ref3,
                         o.Ref4,
                         o.Ref5,
                         o.StatusType,
                         o.TaxRegNo,
                          custdetails = custDetailsLookup.ContainsKey(o.id) ? custDetailsLookup[o.id] : null,
                         o.appointemployee,
                         o.StartDate,
                         o.leadtype,
                         o.LeadCondition,
                         StartTime = (o.StartTime != null) ? o.StartTime.Value.ToShortTimeString() : "",
                         color = (o.StartDate < today && o.StartDate != null) ? 3 : 0,
                         mobmodel = mobLookup[o.id].Select(x => new { x.Num, x.Name, x.emails }).Take(300).ToList()

                     });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.LeadName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
            }
            
            var leadlist = v.OrderByDescending(o => o.ldate).ToList();
            if (Mobile != "All")
            {
                leadlist = leadlist.Where(o => o.mobmodel.Any(s => s.Num == Mobile)).ToList();
            }

            recordsTotal = leadlist.Count();
            var data = leadlist.Skip(skip).Take(pageSize).ToList().Distinct().ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }





        [HttpPost]
        //  [QkAuthorize(Roles = "Dev,Leads List")]
        public ActionResult GetAllLeadsreport(long flag, string LeadCode, long? LeadName, long? LeadSource, string CreatedBy, string Location, long? LeadStatus, long? LeadCustomer, long? AssignedTo, string LeadLevel, string Mobile, string fromdate, string todate, string StartDate, string EndDate, string LastUpdDays, string ref1, string ref2, string ref3, string ref4, string ref5, string nextdate, string opncls, string apfrom, string apto, string type, long? employeename, long? sourcename)
        {
            // BUG A fix: the "All" option posts AssignedTo = 0 (not null). Treat 0 as "no filter".
            if (AssignedTo == 0) AssignedTo = null;

            int days = 0;
            int taken = 100000;
            DateTime today = System.DateTime.Now.Date;
            if (LastUpdDays != "")
            {
                days = Convert.ToInt32(LastUpdDays);
            }
            int OandC = 0;
            if (opncls != "")
            {
                OandC = Convert.ToInt32(opncls);
            }
            DateTime datecheck = DateTime.Now.AddDays(-days);

            DateTime datenow = DateTime.Now;
            string search = Request.Form.GetValues("search[value]")[0];
            if (search != "")
            {
                flag = 0;
            }
            if (ref3 == "")
                ref3 = null;
            if (LeadCode == "" && LeadName == 0 && LeadSource == 0 && CreatedBy == "0" && Location == "" && LeadStatus == null && LeadCustomer == 0 && AssignedTo == null && LeadLevel == "All" && Mobile == "All" && fromdate == "" && todate == "" && StartDate == "" && EndDate == "" && LastUpdDays == "0" && ref1 == "" && ref2 == "" && ref3 == null && ref4 == null && ref5 == null && nextdate == "")
            {
                flag = 0;
            }
            long leadstatus = 0;
            if (type == "1")
                leadstatus = 9;
            else if (type == "2")
                leadstatus = 20019;
            else if (type == "3")
                leadstatus = -1;

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? sdate = null;
            DateTime? edate = null;
            DateTime? ndate = null;
            DateTime? apfromdate = null;
            DateTime? aptodate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (nextdate != "")
            {
                ndate = DateTime.Parse(nextdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (StartDate != "")
            {
                sdate = DateTime.Parse(StartDate, new CultureInfo("en-GB"));
            }
            if (EndDate != "")
            {
                edate = DateTime.Parse(EndDate, new CultureInfo("en-GB"));
            }
            if (apfrom != "")
            {
                apfromdate = DateTime.Parse(apfrom, new CultureInfo("en-GB"));
            }
            if (apto != "")
            {
                aptodate = DateTime.Parse(apto, new CultureInfo("en-GB"));
            }
            DateTime crdate = DateTime.Now.AddDays(-365);
            var xx = db.AssignedTos.Where(cl => cl.Status == "Assigned" && cl.ChkStatus == 0 && cl.EmployeeId == AssignedTo).Select(o => o.CustomerID).Distinct().ToList().ToArray();

            if (AssignedTo != null)
                xx = db.AssignedTos.Where(cl => cl.Status == "Assigned" && cl.ChkStatus == 0 && cl.EmployeeId == AssignedTo).Select(o => o.CustomerID).Distinct().ToList().ToArray();


            var userid = User.Identity.GetUserId();
            // Perf (audit P2, batch 6): tksup (the full LeadTaskUpdations projection, ~107K rows on the
            // service company) is only consulted by the type==1/2/3 and employeename filters below —
            // skip materializing it when none of those are active (Take(0) keeps the element type).
            bool needTksup = type == "1" || type == "2" || type == "3" || employeename != null;
            var tksupQuery = (from xxx in db.LeadTaskUpdations
                         where
                           (fromdate == "" || EF.Functions.DateDiffDay(xxx.CreatedDate, fdate) <= 0)
                         select new
                         {
                             st = xxx.leadstatus,
                             cusid = xxx.TaskId,
                             emid = db.Employees.Where(o => o.UserId == xxx.CreatedBy).Select(o => o.EmployeeId).FirstOrDefault()


                         }
                    );
            var tksup = needTksup ? tksupQuery.ToList() : tksupQuery.Take(0).ToList();
            // EF Core 10 cannot translate the nested-collection projections (lead/mobmodel) or the collection-valued
            // `let mob`. SERVER query keeps the joins + translatable WHERE; the `lead` collection + Mobile filter
            // are computed client-side below. (`x`/`z` lets removed: `x` is redundant with the `xx.Contains` filter,
            // `z` is unused.)
            var serverRows = (from a in db.Customers
                     join h in db.leadcustomerrelation on a.CustomerID equals h.leadid into lead
                     from h in lead.DefaultIfEmpty()
                     join c in db.SourceOfLeads on a.SourceOfLead equals c.SourceOfLeadId into SOL
                     from c in SOL.DefaultIfEmpty()
                     join d in db.CustomerConversions on a.CustomerID equals d.CustomerID into cs
                     from d in cs.DefaultIfEmpty()
                     join i in db.Accountss on a.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join l in db.LocationNames on a.LocationID equals l.LocationId into loc
                     from l in loc.DefaultIfEmpty()
                     where
                      a.Type == CRMCustomerType.Leads &&
                     (flag == 1 || a.logtime > crdate) &&
                     (LeadCode == null || LeadCode == "" || a.CustomerCode == LeadCode) &&
                     (LeadName == 0 || a.CustomerID == LeadName) &&
                     (LeadSource == 0 || a.SourceOfLead == LeadSource) &&
                     (LeadStatus == null || a.CurrentAction == LeadStatus) &&
                     (LeadCustomer == 0 || h.customerid == LeadCustomer) &&
                     (CreatedBy == "0" || d.CreatedUser == CreatedBy) &&
                     (LeadLevel == "All" || a.LeadLevel == LeadLevel) &&
                     (apfrom == "" || EF.Functions.DateDiffDay(a.StartDate, apfromdate) <= 0) &&
                     (apto == "" || EF.Functions.DateDiffDay(a.StartDate, aptodate) >= 0) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0) &&
                     (StartDate == "" || EF.Functions.DateDiffDay(a.StartDate, sdate) <= 0) &&
                     (EndDate == "" || (EF.Functions.DateDiffDay(a.EndDate, edate) >= 0 && a.logtime <= edate)) &&
                     // Mobile filter re-applied client-side below via mobmodel (same as the original tail).
                     (Location == "" || a.Location.Contains(Location)) &&
                     (ref1 == "" || ref1 == null || a.Ref1 == ref1) &&
                     (ref2 == "" || ref2 == null || a.Ref2 == ref2) &&
                     (ref3 == "" || ref3 == null || a.Ref3 == ref3) &&
                     (ref4 == "" || ref4 == null || a.Ref4 == ref4) &&
                     (ref5 == "" || ref5 == null || a.Ref5 == ref5) &&



                     (AssignedTo == null || xx.Contains(a.CustomerID))
                     // (days == 0 || ((EF.Functions.DateDiffDay(g.CreatedDate,datecheck)<=0) && (EF.Functions.DateDiffDay(g.CreatedDate,datenow)>=0))
                     // || ((EF.Functions.DateDiffDay(m.LogTime, datecheck) <=0) && (EF.Functions.DateDiffDay(m.LogTime, datenow) >=0)))
                     select new
                     {
                         id = a.CustomerID,
                         LeadName = a.CustomerName,
                         LeadCode = a.CustomerCode,
                         //TaxRegNo = i.TRN,
                         nextdate = a.EndTime,
                         TaxRegNo = a.TaxRegNo,
                         l.Location,
                         a.OpenClose,
                         //Phone = b.Phone,
                         //Mobile = b.Mobile,
                         // Email = b.EmailId,
                         a.StartDate,
                         a.StartTime,

                         ldate = a.logtime,


                         SrcLead = c.SrcName,
                         CreatedDate = (DateTime?)d.CreatedDate,
                     }).ToList();

            // assigned-employee names per lead (was the nested `lead` collection projection); ToLookup => empty for missing keys.
            var reportLeadIds = serverRows.Select(o => o.id).Distinct().ToList();
            var leadAssignLookup = (from f in db.AssignedTos
                                    join g in db.Employees on f.EmployeeId equals g.EmployeeId into emps
                                    from g in emps.DefaultIfEmpty()
                                    where reportLeadIds.Contains(f.CustomerID)
                                    && f.Status == "Assigned"
                                    select new { f.CustomerID, emp = (g == null) ? "" : g.FirstName, empids = (long?)g.EmployeeId })
                                   .ToList().ToLookup(o => o.CustomerID);

            // Perf (audit P2, batch 6): mobmodel used to run one DB query PER ROW when the report grid
            // materialized (~3,100 queries on a default load). Prefetch once over the lead ids; shape
            // and per-lead order unchanged (byte-identical golden on both companies, 2026-06-13).
            var rptMobLookup = (from co in db.Contacts
                                join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                join con in db.Country on co.CountryID equals con.CountryID into conn
                                from con in conn.DefaultIfEmpty()
                                where rrr.RelationType == 2 && reportLeadIds.Contains(rrr.RelationID)
                                select new { rrr.RelationID, Num = co.Mobile, Name = co.Name, emails = co.EmailId })
                               .ToList()
                               .ToLookup(x => x.RelationID);

            var v = serverRows.Select(o => new
                     {
                         o.id,
                         o.LeadName,
                         o.LeadCode,
                         o.nextdate,
                         o.TaxRegNo,
                         o.Location,
                         o.OpenClose,
                         o.StartDate,
                         o.StartTime,
                         o.ldate,
                         o.SrcLead,
                         o.CreatedDate,
                         lead = leadAssignLookup[o.id].Select(e => new { emp = e.emp, empids = e.empids }).Distinct().ToList(),
                     }).OrderByDescending(x => x.ldate).Take(taken).Select(o => new
                     {
                         o.OpenClose,
                         o.id,
                         o.ldate,
                         o.lead,
                         o.LeadCode,
                         o.LeadName,
                         o.Location,
                         o.nextdate,
                         o.SrcLead,
                         o.TaxRegNo,

                         o.StartDate,
                         StartTime = (o.StartTime != null) ? o.StartTime.Value.ToShortTimeString() : "",
                         color = (o.StartDate < today && o.StartDate != null) ? 3 : 0,
                         mobmodel = rptMobLookup[o.id].Select(x => new { x.Num, x.Name, x.emails }).ToList()

                     });
            if (type == "1" || type == "2")
                v = v.Where(uu => tksup.Where(ok => ok.cusid == uu.id && ok.st == leadstatus).Count() > 0);
            else if (type == "3")
            {
                var vv = tksup.Where(ok => ok.st == 9 || ok.st == 20019).Select(O => O.cusid).ToList().ToArray();


                v = v.Where(uu => !vv.Contains(uu.id));
            }
            if (employeename != null)
            {
                v = v.Where(uu => tksup.Where(ok => uu.id == ok.cusid && ok.emid == employeename).Count() > 0);

            }

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.LeadName.ToString().ToLower().Contains(search.ToLower()));
            }


            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            var leadlist = v.ToList();
            if (Mobile != "All")
            {
                leadlist = leadlist.Where(o => o.mobmodel.Any(s => s.Num == Mobile)).ToList();
            }

            recordsTotal = leadlist.Count();
            var data = leadlist.Skip(skip).Take(pageSize).ToList().Distinct().ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        //
        public ActionResult GetAllLeadsTest(long? LeadName, long? LeadSource, string CreatedBy, string Location, long? LeadStatus, long? AssignedTo)
        {
            // BUG A fix: the "All" option posts AssignedTo = 0 (not null). Treat 0 as "no filter".
            if (AssignedTo == 0) AssignedTo = null;

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

            // AssignedTo filter via a pre-computed id set (replaces the `let x` correlated subquery; same proven
            // pattern as GetAllLeads/GetAllLeadsreport). Combined with the BUG A normalization above, AssignedTo == 0
            // ("All") now means no filter instead of "assigned to employee 0" (= empty).
            var xx = db.AssignedTos.Where(cl => cl.EmployeeId == AssignedTo).Select(o => o.CustomerID).Distinct().ToList().ToArray();

            // EF Core 10 cannot translate the nested-collection projections (lead/mobmodel). SERVER query keeps the
            // joins + WHERE + scalar columns; the collections are built client-side via lookups below.
            var serverQuery = (from a in db.Customers
                     join b in db.Contacts on a.Contact equals b.ContactID into Contact
                     from b in Contact.DefaultIfEmpty()
                     join c in db.SourceOfLeads on a.SourceOfLead equals c.SourceOfLeadId into SOL
                     from c in SOL.DefaultIfEmpty()
                     join d in db.CustomerConversions on a.CustomerID equals d.CustomerID into cs
                     from d in cs.DefaultIfEmpty()
                     join i in db.Accountss on a.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()

                     where a.Type == CRMCustomerType.Leads &&

                      (LeadName == 0 || a.CustomerID == LeadName) &&
                      (LeadSource == 0 || a.SourceOfLead == LeadSource) &&
                      (LeadStatus == null || a.LeadStat == LeadStatus) &&


                      (CreatedBy == "0" || d.CreatedUser == CreatedBy) &&
                      (Location == "" || a.Location.Contains(Location)) &&

                      (AssignedTo == null || AssignedTo == 0 || xx.Contains(a.CustomerID))


                     select new
                     {


                         id = a.CustomerID,
                         LeadName = a.CustomerName,
                         LeadCode = a.CustomerCode,
                         TaxRegNo = i.TRN,
                         a.Location,
                         a.Contact,

                         Phone = b.Phone,
                         //Mobile = b.Mobile,
                         Email = b.EmailId,



                         SrcLead = c.SrcName,
                     });

            // Performance (audit P2): server-side paging on the common path; search/computed sorts fall back unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "Contact","Email","id","LeadCode","LeadName","Location","Phone","SrcLead","TaxRegNo" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("id asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // assigned-employee names per lead + mobile numbers per contact, computed client-side (were nested ToList()s).
            var testLeadIds = serverRows.Select(o => o.id).Distinct().ToList();
            var testContacts = serverRows.Select(o => o.Contact).Distinct().ToList();
            var leadAssignLookup = (from f in db.AssignedTos
                                    join g in db.Employees on f.EmployeeId equals g.EmployeeId into emps
                                    from g in emps.DefaultIfEmpty()
                                    where testLeadIds.Contains(f.CustomerID)
                                    select new { f.CustomerID, emp = g.FirstName })
                                   .ToList().ToLookup(o => o.CustomerID);
            var mobLookup = (from ac in db.Mobiles
                             where testContacts.Contains(ac.Contact)
                             select new { ac.Contact, ac.MobileNum, ac.Name })
                            .ToList().ToLookup(o => o.Contact);

            var v = serverRows.Select(o => new
                     {
                         o.id,
                         o.LeadName,
                         o.LeadCode,
                         o.TaxRegNo,
                         o.Location,
                         o.Phone,
                         o.Email,
                         o.SrcLead,
                         // nw
                         lead = leadAssignLookup[o.id].Select(e => new { emp = e.emp }).ToList(),
                         mobmodel = mobLookup[o.Contact].Select(m => new MobileViewModel
                                     {
                                         Num = m.MobileNum,
                                         Name = m.Name
                                     }).ToList(),
                     });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.LeadName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!fastPage) { recordsTotal = v.Count(); }
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        //


        [QkAuthorize(Roles = "Dev,MyLeads")]
        public ActionResult MyLeads()
        {


          


            ViewBag.LeadName = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.LeadSource = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.CreatedBy = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.Location = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.LeadStatus = QkSelect.List(
                             new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                             }, "Value", "Text", 1);


            ViewBag.Mobile = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "All",Value  = "All"},
                         },
                           "Value", "Text", 1);





            ViewBag.Prior = QkSelect.List(Enum.GetValues(typeof(TaskPriority)));
            ViewBag.TaskStatus = QkSelect.List(Enum.GetValues(typeof(TKUpdateStatus)));
            return View();

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,MyLeads")]
        public ActionResult GetAllMyLeads(long flag, string LeadCode, long? LeadName, long? LeadSource, string CreatedBy, string Location, long? LeadStatus, string LeadLevel, string Mobile, string fromdate, string todate, string LastUpdDays, string nextdate)
        {
           



            int days = 0;
            if (LastUpdDays != "")
            {
                days = Convert.ToInt32(LastUpdDays);
            }
            DateTime today = System.DateTime.Now.Date;
            DateTime crdate = DateTime.Now.AddDays(-100);
            DateTime datecheck = DateTime.Now.AddDays(-days);
            int taken = 300;
            DateTime datenow = DateTime.Now;
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
            var UserId = User.Identity.GetUserId();
            var havecreateleadpermission = User.IsInRole("Create Leads");
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? ndate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            if (nextdate != "")
            {
                ndate = DateTime.Parse(nextdate, new CultureInfo("en-GB"));
            }
            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            if (search != "")
            {
                flag = 0;
            }

            // "Leads assigned to me" id-set (replaces the collection-valued `let f` + `f.Contains(empl.EmployeeId)`
            // WHERE term, which EF Core 10 also exposes as `check = f` in the projection -> untranslatable).
            var myLeadIds = db.AssignedTos
                              .Where(cl => cl.approve == false && cl.Status == "Assigned" && cl.ChkStatus == (int)Status.active
                                        && cl.EmployeeId == empl.EmployeeId)
                              .Select(cl => cl.CustomerID).Distinct().ToList();

            // EF Core 10 cannot translate the nested-collection projections (appointemployee/assin/mobmodel), the
            // collection `let f`, or the latest-per-lead `let g/gg`. SERVER query keeps the joins + translatable WHERE
            // and projects scalar columns; g/gg-dependent values + the date filters are computed client-side below.
            var serverRows = (from a in db.Customers
                     join b in db.Contacts on a.Contact equals b.ContactID into tmp
                     from b in tmp.DefaultIfEmpty()
                     join c in db.SourceOfLeads on a.SourceOfLead equals c.SourceOfLeadId into src
                     from c in src.DefaultIfEmpty()

                     join d in db.CustomerConversions on a.CustomerID equals d.CustomerID
                     join u in db.Users on d.CreatedUser equals u.Id into usrs
                     from u in usrs.DefaultIfEmpty()
                     join i in db.Accountss on a.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join l in db.LocationNames on a.LocationID equals l.LocationId into loc
                     from l in loc.DefaultIfEmpty()
                     join nn in db.LeadStatuss on a.CurrentAction equals nn.LeadStatusID into ldconn
                     from nn in ldconn.DefaultIfEmpty()

                     where a.Type == CRMCustomerType.Leads
                     && myLeadIds.Contains(a.CustomerID) &&


                       (LeadCode == null || LeadCode == "" || a.CustomerCode == LeadCode) &&
                       (LeadName == 0 || a.CustomerID == LeadName) &&
                      (LeadSource == 0 || a.SourceOfLead == LeadSource) &&
                      (LeadStatus == null || a.CurrentAction == LeadStatus) &&
                      (CreatedBy == "0" || d.CreatedUser == CreatedBy) &&
                      (Location == "" || a.Location.Contains(Location)) &&
                      (LeadLevel == "All" || a.LeadLevel == LeadLevel) &&
                      // (nextdate ...) and the two (days ...) filters reference the latest LeadTaskUpdation (g) -> client-side.
                      (fromdate == "" || EF.Functions.DateDiffDay(d.CreatedDate, fdate) <= 0) &&
                      (todate == "" || EF.Functions.DateDiffDay(d.CreatedDate, tdate) >= 0) &&
                        (a.OpenClose == 0 || a.OpenClose == null)
                     select new
                     {
                         id = a.CustomerID,
                         LeadName = a.CustomerName,
                         LeadCode = a.CustomerCode,
                         nn.StatusType,

                         TaxRegNo = a.TaxRegNo,
                         l.Location,
                         EndTime = a.EndTime,
                         EndDate = a.EndDate,
                         logtime = a.logtime,
                         Address = b.Address + "<br/>" + b.City + " " + b.State + " " + b.Country + "<br/>" + b.Zip,
                         Phone = b.Phone,
                         //Mobile = b.Mobile,
                         Email = b.EmailId,
                         a.CreditLimit,
                         a.CreatedBy,
                         a.CreditPeriod,
                         a.StartDate,
                         a.StartTime,
                         SrcLead = c.SrcName,
                     }).ToList();

            // ---- client-side lookups for the parts EF Core 10 can't translate ----
            var myLeadResultIds = serverRows.Select(o => o.id).Distinct().ToList();
            // latest LeadTaskUpdation per lead (was `let g`)
            var gLookup = db.LeadTaskUpdations
                            .Where(lr => myLeadResultIds.Contains(lr.TaskId))
                            .ToList()
                            .GroupBy(lr => lr.TaskId)
                            .ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(lr => lr.TaskUpdationID).First());
            // latest "Appointment Modified" LeadTaskUpdation per lead (was `let gg`)
            var ggLookup = db.LeadTaskUpdations
                            .Where(lr => myLeadResultIds.Contains(lr.TaskId) && lr.Remarks == "Appointment Modified")
                            .ToList()
                            .GroupBy(lr => lr.TaskId)
                            .ToDictionary(grp => grp.Key, grp => grp.OrderByDescending(lr => lr.TaskUpdationID).First());
            // EmployeeIds assigned to each lead (was the collection `let f`, surfaced as `check`)
            var checkLookup = db.AssignedTos
                            .Where(cl => myLeadResultIds.Contains(cl.CustomerID) && cl.approve == false && cl.Status == "Assigned" && cl.ChkStatus == (int)Status.active)
                            .Select(cl => new { cl.CustomerID, cl.EmployeeId })
                            .ToList().ToLookup(o => o.CustomerID);
            // assigned-employee names per lead (was the nested `assin` collection)
            var assinLookup = (from f in db.AssignedTos
                               join g in db.Employees on f.EmployeeId equals g.EmployeeId into emp
                               from g in emp.DefaultIfEmpty()
                               where myLeadResultIds.Contains(f.CustomerID)
                               select new { f.CustomerID, g.FirstName })
                              .ToList().ToLookup(o => o.CustomerID);
            // employee display name by UserId (was the nested `appointemployee` subquery, keyed off gg.CreatedBy)
            var appointEmpLookup = (from kk in db.Employees
                                    join kkk in db.Users on kk.UserId equals kkk.Id
                                    select new { kkk.Id, empname = kk.FirstName + " " + kk.MiddleName + " " + kk.LastName })
                                   .ToList().ToLookup(o => o.Id);

            // g-dependent WHERE conditions (the `(nextdate...)` and the two `(days...)` terms) applied client-side.
            // EF.Functions.DateDiffDay(x,y) == whole-day (y - x); reproduced with .Date comparisons. Null g/date => false.
            var filteredRows = serverRows.Where(o =>
            {
                var g = gLookup.ContainsKey(o.id) ? gLookup[o.id] : null;
                bool nextOk = nextdate == "" || ((g != null && g.nextdate == ndate) || o.EndDate == ndate);
                bool days1 = days == 0 || ((g != null && g.CreatedDate <= datecheck) || (o.logtime != null && o.logtime <= datecheck));
                bool gInRange = g != null && g.CreatedDate != null
                                && g.CreatedDate.Value.Date >= datecheck.Date && g.CreatedDate.Value.Date <= datenow.Date;
                bool logInRange = o.logtime != null
                                && o.logtime.Value.Date >= datecheck.Date && o.logtime.Value.Date <= datenow.Date;
                bool days2 = days == 0 || (gInRange || logInRange);
                return nextOk && days1 && days2;
            }).ToList();

            // Perf (audit P2, batch 6): the projection below used to run TWO correlated DB queries PER ROW
            // (mobmodel + custdetails) — and the tail enumerates `v` up to three times (Count x2 + page),
            // re-running them each time. Prefetch both once over the surviving lead ids; shape and
            // per-lead order unchanged (byte-identical golden on both companies, 2026-06-13).
            var myVIds = filteredRows.Select(o => o.id).Distinct().ToList();
            var myMobLookup = (from co in db.Contacts
                               join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                               where rrr.RelationType == 2 && myVIds.Contains(rrr.RelationID)
                               select new { rrr.RelationID, Num = co.Mobile, Name = co.FirstName + " " + co.LastName, emails = co.EmailId })
                              .ToList()
                              .ToLookup(x => x.RelationID);
            var myCustLookup = (from cc in db.Customers
                                join dd in db.leadcustomerrelation on cc.CustomerID equals dd.customerid
                                where myVIds.Contains(dd.leadid)
                                select new { dd.leadid, cc.CustomerID, CustomerName = cc.CustomerCode + " - " + cc.CustomerName })
                               .ToList()
                               .GroupBy(x => x.leadid)
                               .ToDictionary(g2 => g2.Key, g2 => g2.Select(x => new { x.CustomerID, x.CustomerName }).First());

            // ---- client re-projection to the ORIGINAL first-projection shape ----
            var v = filteredRows.Select(o =>
                     {
                         var g = gLookup.ContainsKey(o.id) ? gLookup[o.id] : null;
                         var gg = ggLookup.ContainsKey(o.id) ? ggLookup[o.id] : null;
                         var appointemployee = (gg != null && gg.CreatedBy != null)
                                                ? appointEmpLookup[gg.CreatedBy].Select(e => new { empname = e.empname }).FirstOrDefault()
                                                : null;
                         return new
                         {
                             id = o.id,
                             LeadName = o.LeadName,
                             LeadCode = o.LeadCode,
                             o.StatusType,

                             TaxRegNo = o.TaxRegNo,
                             finishdate = (g != null && g.finishtime != null) ? g.finishtime : null,
                             o.Location,
                             nextdate = (o.EndTime == null) ? (g != null ? g.nextdate : null) : o.EndTime,
                             Address = o.Address,
                             Phone = o.Phone,
                             //Mobile = b.Mobile,
                             Email = o.Email,
                             o.CreditLimit,
                             o.CreatedBy,
                             o.CreditPeriod,
                             o.StartDate,
                             o.StartTime,
                             appointemployee = appointemployee,
                             ldate = ((g != null) && (g.CreatedDate > o.logtime)) ? g.CreatedDate : o.logtime,
                             SrcLead = o.SrcLead,
                             check = checkLookup[o.id].Select(e => e.EmployeeId).ToList(),
                             assin = assinLookup[o.id].Select(e => new { e.FirstName }).Distinct().ToList(),
                         };
                     }).OrderByDescending(x => x.ldate).Take(taken).Select(o => new
                     {
                         o.id,
                         o.ldate,
                         o.LeadCode,
                         o.LeadName,
                         o.Location,
                         o.nextdate,
                         o.Phone,
                         o.SrcLead,
                         o.TaxRegNo,
                         o.check,
                         o.appointemployee,
                         o.StartDate,
                         StartTime = (o.StartTime != null) ? o.StartTime.Value.ToShortTimeString() : "",
                         color = (o.StartDate < today && o.StartDate != null) ? 3 : 0,
                         o.assin,
                         o.Address,
                         o.CreditLimit,
                         o.CreditPeriod,
                         o.Email,
                         o.finishdate,
                       o.StatusType,
                         mobmodel = myMobLookup[o.id].Select(x => new { x.Num, x.Name, x.emails }).ToList(),

                         ownlead = (o.CreatedBy == UserId && havecreateleadpermission == false) ? true : false,
                          custdetails = myCustLookup.ContainsKey(o.id) ? myCustLookup[o.id] : null,
                     });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.LeadName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count() > 0 ? v.Count() : 0;
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        // GET: /CRUD/create/5  
        //[HttpGet]
        //Get: View of Adding Remarks
        public ActionResult AddCustomerRemark(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer cus = db.Customers.Find(id);

            if (cus == null)
            {
                return NotFound();
            }
            var CusRemark = new Appointmentviewmodal
            {
                CustomerId = cus.CustomerID,
                nextfolloupdate = DateTime.Now.AddDays(1).ToString("dd-MM-yyyy"),
                nextfolloupdatetime = DateTime.Now.AddDays(0),
                Remark = ""


            };

            return PartialView(CusRemark);
        }

        //Saving of Remarks
        [HttpPost]
        public JsonResult AddCustomerRemark(Appointmentviewmodal CusRemark)
        {
            Int64 CustomerId = CusRemark.CustomerId;

            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                if (UserId != null)
                {
                    Common com = new Common();

                    var Today = Convert.ToDateTime(System.DateTime.Now);

                    DateTime nextdate = DateTime.Parse(CusRemark.nextfolloupdate, new CultureInfo("en-GB"));

                    TimeSpan etime = ((DateTime)CusRemark.nextfolloupdatetime).TimeOfDay;

                    DateTime etimes = nextdate + etime;
                    //CustomerRemark Obj = new CustomerRemark
                    //    CustomerId = CusRemark.CustomerId,
                    //    Remark = CusRemark.Remark,
                    //    AddedUser = UserId,
                    //    CreatedDate = Today,
                    //    nextdate = nextdate,
                    //    nexttime = etimes
                    if (!CusRemark.cancel)
                    {
                        var ld = db.Customers.Find(CusRemark.CustomerId);
                        ld.StartDate = nextdate;
                        ld.StartTime = etimes;
                        db.Entry(ld).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        var ld = db.Customers.Find(CusRemark.CustomerId);
                        ld.StartDate = null;
                        ld.StartTime = null;
                        db.Entry(ld).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    LeadTaskUpdation TaskUps = new LeadTaskUpdation
                    {
                        TaskId = CusRemark.CustomerId,
                        CreatedBy = UserId,
                        CreatedDate = Today,

                        Remarks = "Appointment Modified",
                        leadstatus = 4,
                    };
                    db.LeadTaskUpdations.Add(TaskUps);
                    db.SaveChanges();
                    com.updateleaddate(CusRemark.CustomerId);
                    com.addlog(LogTypes.Created, UserId, "Leads", "CustomerRemarks", findip(), CustomerId, "Leads Appointment Added Successfully..");
                    Success("Appointment added successfully...", true);
                }
                else
                {
                    Danger("Failed to add Remarks...,please login again", true);
                }
            }
            else
            {
                Danger("Failed to add Remarks...,please login again", true);
            }

            return Json(new { msg = "Success", status = true });

        }
        public ActionResult Create(long? format)
        {
            ViewBag.format = 1;
            if (format != null)
            {
                ViewBag.format = 2;

            }
            ViewBag.AssignTypes = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                               }, "Value", "Text", 0);

            //----------------for customer name-----------------
            ViewBag.Customer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "",Value = ""},
                                 }, "Value", "Text", 1);
            //end
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");


            ViewBag.SalesPerson = QkSelect.List(use, "ID", "Name");
            var viewModel = new LeadsViewModel
            {
                CustomerCode = LeadCode()
            };
            if (ViewBag.format == 2)
            {
                var userid = User.Identity.GetUserId();
                var sel = db.Employees.Where(o => o.UserId == userid)
                   .Select(s => new
                   {
                       ID = s.EmployeeId,

                   }).ToArray();
                var TMembers = db.Employees
                       .Select(s => new
                       {
                           ID = s.EmployeeId,
                           Name = s.FirstName + " " + s.LastName
                       })
                       .ToList();
                ViewBag.Employees = new MultiSelectList(TMembers, "ID", "Name", sel);
                viewModel.AssignedToo = db.Employees.Where(o => o.UserId == userid)
                  .Select(s => s.EmployeeId).ToList().ToArray();

            }
            var UserId = User.Identity.GetUserId();
            var salesman = db.Employees.Where(o => o.UserId == UserId).Select(o => new
            {
                o.EmployeeId,
                empname = o.FirstName + " " + o.LastName
            }
                ).ToList();


            var Country = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName,
            }).ToList();
            ViewBag.Country = QkSelect.List(Country, "Id", "Name");

            var State = db.States.Select(s => new
            {
                Id = s.StateID,
                Name = s.StateName,
            }).ToList();
            ViewBag.States = QkSelect.List(State, "Id", "Name");


            var Location = db.LocationNames.Select(s => new
            {
                Id = s.LocationId,
                Name = s.Location,
            }).ToList();
            ViewBag.Location = QkSelect.List(Location, "Id", "Name");



            ViewBag.CustName = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                            }, "Value", "Text", 1);

            ViewBag.Phone = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value =null},
                          }, "Value", "Text", 1);
            ViewBag.Mobile = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                          }, "Value", "Text", 1);


            var emps = db.Employees.Select(s => new { ID = s.EmployeeId, Name = s.FirstName + " " + s.LastName }).ToList();
            ViewBag.AssignedTo = QkSelect.List(emps, "ID", "Name");

            if (format == 2)
            {
                if (salesman.Count() > 0)
                {
                    viewModel.SalesPerson = salesman[0].EmployeeId;
                    viewModel.SourceOfLead = salesman[0].EmployeeId;
                    ViewBag.AssignedTo = QkSelect.List(salesman, "EmployeeId", "empname");


                }
                else
                {
                    ViewBag.AssignedTo = QkSelect.List(emps, "EmployeeId", "empname");
                }
            }
            viewModel.FieldMap = db.FieldMappings.Where(a => a.Section == "Leads" && a.Status == Status.active).ToList();
            viewModel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Leads").ToList();
            var ref1 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Customers
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.SrcLead = QkSelect.List(lead, "ID", "Name");

            //    Id = s.CustomerID,
            //    Name = s.CustomerName,

            ViewBag.LeadType = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = true, Text = "Select Lead type", Value = "0"},
                       }, "Value", "Text", 1);

            ViewBag.LeadLevel = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = true, Text = "Select Lead Level", Value = "0"},
                       }, "Value", "Text", 1);

            ViewBag.LeadStat = QkSelect.List(
                        new List<SelectListItem>
                        {
                                    new SelectListItem { Selected = true, Text = "Select Lead Status", Value = "0"},
                        }, "Value", "Text", 1);


            ViewBag.SrcOfLead = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = true, Text = "Select Source Of Lead", Value = "0"},
                         }, "Value", "Text", 1);

            var Actions = db.LeadStatuss.Select(s => new
            {
                Id = s.LeadStatusID,
                Name = s.StatusType,
            }).ToList();
            ViewBag.CurrentAction = QkSelect.List(Actions, "Id", "Name");
            ViewBag.NextAction = QkSelect.List(Actions, "Id", "Name");




            var LeadConditions = db.LeadCondition.Select(s => new
            {
                Id = s.id,
                Name = s.LeadCondition,
            }).ToList();
            ViewBag.LeadCondition = QkSelect.List(LeadConditions, "Id", "Name");



            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;
            var leadstat = db.LeadStatuss.Select(r => new
            {
                ID = r.LeadStatusID,
                Name = r.StatusType
            }).ToList();
            ViewBag.LeadStat = QkSelect.List(leadstat, "ID", "Name");
            ViewBag.LastEntry = db.Customers.Where(p => p.Type == CRMCustomerType.Leads).Select(p => p.CustomerID).AsEnumerable().DefaultIfEmpty(0).Max();

            var cusimg = db.LeadDocuments.Where(a => a.Status == Status.inactive).ToList();
            string path = LegacyWeb.MapPath("~/uploads/leaddocuments/");
            foreach (var arr in cusimg)
            {

                try
                {
                    string[] splitlist = arr.FileName.Split('_');
                    if (splitlist.Length > 1)
                    {
                        string newpath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + splitlist[1]);
                        string filepath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + arr.FileName);
                        if (System.IO.File.Exists(newpath) && System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(Path.Combine(path, splitlist[1]));
                        }
                    }
                    LeadDocument doc = db.LeadDocuments.Find(arr.LeadDocumentId);
                    doc.Status = Status.active;
                    db.Entry(doc).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch
                {
                    Exception ex;
                }
            }
            return View(viewModel);
        }

        public ActionResult Createownlead(long? format)
        {
            ViewBag.format = 2;
            ViewBag.AssignTypes = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                               }, "Value", "Text", 0);

            //----------------for customer name-----------------
            ViewBag.Customer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "",Value = "0"},
                                 }, "Value", "Text", 1);
            //end
            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            var userid = User.Identity.GetUserId();

            var sel = db.Employees.Where(o => o.UserId == userid)
                   .Select(s => new
                   {
                       ID = s.EmployeeId,

                   }).ToArray();
            var TMembers = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.Employees = new MultiSelectList(TMembers, "ID", "Name", sel);

            ViewBag.SalesPerson = QkSelect.List(use, "ID", "Name");
            var viewModel = new LeadsViewModel
            {
                CustomerCode = LeadCode()
            };
            var UserId = User.Identity.GetUserId();
            var salesman = db.Employees.Where(o => o.UserId == UserId).Select(o => new
            {
                o.EmployeeId,
                empname = o.FirstName + " " + o.LastName
            }
                ).ToList();


            var Country = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName,
            }).ToList();
            ViewBag.Country = QkSelect.List(Country, "Id", "Name");

            var State = db.States.Select(s => new
            {
                Id = s.StateID,
                Name = s.StateName,
            }).ToList();
            ViewBag.States = QkSelect.List(State, "Id", "Name");


            var Location = db.LocationNames.Select(s => new
            {
                Id = s.LocationId,
                Name = s.Location,
            }).ToList();
            ViewBag.Location = QkSelect.List(Location, "Id", "Name");



            ViewBag.CustName = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                            }, "Value", "Text", 1);

            ViewBag.Phone = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value =null},
                          }, "Value", "Text", 1);
            ViewBag.Mobile = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                          }, "Value", "Text", 1);


            var emps = db.Employees.Select(s => new { ID = s.EmployeeId, Name = s.FirstName + " " + s.LastName }).ToList();
            ViewBag.AssignedTo = QkSelect.List(emps, "ID", "Name");

            if (format == 2)
            {
                if (salesman.Count() > 0)
                {
                    viewModel.SalesPerson = salesman[0].EmployeeId;
                    viewModel.SourceOfLead = salesman[0].EmployeeId;
                    ViewBag.AssignedTo = QkSelect.List(salesman, "EmployeeId", "empname");


                }
                else
                {
                    ViewBag.AssignedTo = QkSelect.List(emps, "EmployeeId", "empname");
                }
            }
            viewModel.FieldMap = db.FieldMappings.Where(a => a.Section == "Leads" && a.Status == Status.active).ToList();
            viewModel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Leads").ToList();
            var ref1 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Customers
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.SrcLead = QkSelect.List(lead, "ID", "Name");

            //    Id = s.CustomerID,
            //    Name = s.CustomerName,

            ViewBag.LeadType = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = true, Text = "Select Lead type", Value = "0"},
                       }, "Value", "Text", 1);

            ViewBag.LeadLevel = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = true, Text = "Select Lead Level", Value = "0"},
                       }, "Value", "Text", 1);

            ViewBag.LeadStat = QkSelect.List(
                        new List<SelectListItem>
                        {
                                    new SelectListItem { Selected = true, Text = "Select Lead Status", Value = "0"},
                        }, "Value", "Text", 1);


            ViewBag.SrcOfLead = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = true, Text = "Select Source Of Lead", Value = "0"},
                         }, "Value", "Text", 1);

            var Actions = db.LeadStatuss.Select(s => new
            {
                Id = s.LeadStatusID,
                Name = s.StatusType,
            }).ToList();
            ViewBag.CurrentAction = QkSelect.List(Actions, "Id", "Name");
            ViewBag.NextAction = QkSelect.List(Actions, "Id", "Name");




            var LeadConditions = db.LeadCondition.Select(s => new
            {
                Id = s.id,
                Name = s.LeadCondition,
            }).ToList();
            ViewBag.LeadCondition = QkSelect.List(LeadConditions, "Id", "Name");



            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;
            var leadstat = db.LeadStatuss.Select(r => new
            {
                ID = r.LeadStatusID,
                Name = r.StatusType
            }).ToList();
            ViewBag.LeadStat = QkSelect.List(leadstat, "ID", "Name");
            ViewBag.LastEntry = db.Customers.Where(p => p.Type == CRMCustomerType.Leads).Select(p => p.CustomerID).AsEnumerable().DefaultIfEmpty(0).Max();

            var cusimg = db.LeadDocuments.Where(a => a.Status == Status.inactive).ToList();
            string path = LegacyWeb.MapPath("~/uploads/leaddocuments/");
            foreach (var arr in cusimg)
            {

                try
                {
                    string[] splitlist = arr.FileName.Split('_');
                    if (splitlist.Length > 1)
                    {
                        string newpath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + splitlist[1]);
                        string filepath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + arr.FileName);
                        if (System.IO.File.Exists(newpath) && System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(Path.Combine(path, splitlist[1]));
                        }
                    }
                    LeadDocument doc = db.LeadDocuments.Find(arr.LeadDocumentId);
                    doc.Status = Status.active;
                    db.Entry(doc).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch
                {
                    Exception ex;
                }
            }
            viewModel.AssignedToo = db.Employees.Where(o => o.UserId == userid)
                   .Select(s => s.EmployeeId).ToList().ToArray();
            viewModel.StartDatee = (System.DateTime.Now).ToString("dd-MM-yyyy");
            return View(viewModel);
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create Leads")]
        public ActionResult Create(LeadsSubmitViewModel vmodel)
        {
            if (ModelState.IsValid)
            {

                var Exists = db.Customers.Any(c => c.CustomerCode == vmodel.CustomerCode);
                string custcode = Exists ? CustCode() : vmodel.CustomerCode;
                if (Exists)
                {
                    Danger("Leads with same Leads code exists.", true);
                    return RedirectToAction("Create", "Leads");
                }
                else
                {
                    DateTime? eDate = null;
                    DateTime? etimes = null;
                    if (vmodel.EndDatee != null)
                    {
                        eDate = DateTime.Parse(vmodel.EndDatee, new CultureInfo("en-GB"));
                        TimeSpan? etime = null;
                        if (vmodel.EndTime != null)
                        {
                            etime = ((DateTime)vmodel.EndTime).TimeOfDay;
                        }
                        etimes = eDate + etime;
                    }
                    DateTime? sDate = null;
                    DateTime? stimes = null;
                    if (vmodel.StartDatee != null)
                    {
                        sDate = DateTime.Parse(vmodel.StartDatee, new CultureInfo("en-GB"));
                        TimeSpan? etime = null;
                        if (vmodel.StartTime != null)
                        {
                            etime = ((DateTime)vmodel.StartTime).TimeOfDay;
                        }
                        stimes = sDate + etime;
                    }


                    var UserId = User.Identity.GetUserId();
                    Int64 contactId = 0;
                    var contact = new Contact
                    {
                        Name = vmodel.CustomerName,
                        FirstName = vmodel.CustomerName,
                        LastName = vmodel.CustomerName,
                        Address = vmodel.Address,
                        //City = vmodel.City,
                        //State = vmodel.State,
                        //Country = vmodel.Country,
                        //Zip = vmodel.Zip,
                        //Phone = vmodel.Phone,
                        ////Mobile = vmodel.Mobile,
                        //Fax = vmodel.Fax,
                        //EmailId = vmodel.EmailId,
                        //Reference = vmodel.Reference,
                        //ContactPerson = vmodel.ContactPerson,
                        Group = 2,
                        Status = Status.active,
                    };
                    db.Contacts.Add(contact);
                    db.SaveChanges();
                    contactId = contact.ContactID;
                    //                    Contact = contactId,
                    //                    MobileNum = arr.Num,
                    //                    Name = arr.Name




                    if (vmodel.EndDatee != null)
                    {
                        eDate = DateTime.Parse(vmodel.EndDatee.ToString(), new CultureInfo("en-GB"));
                    }
                    var scop = db.LeadTypes.Where(o => o.TypeId == 0).Select(o => o.Type).ToList().ToArray();
                    if (vmodel.ScopeOfWork != null)
                        scop = db.LeadTypes.Where(o => vmodel.ScopeOfWork.Contains(o.TypeId)).Select(o => o.Type).ToList().ToArray();

                    if (vmodel.format != "1" && vmodel.format != null)
                    {
                        vmodel.TaxRegNo = vmodel.StartDatee + "-" + vmodel.CustomerName + "-" + string.Join(",", scop) + "-" + vmodel.Remark;
                        vmodel.CustomerName = vmodel.TaxRegNo;

                    }

                    Customer cus = new Customer
                    {
                        Contact = contactId,
                        EntryNo = GetEntryNo(),
                        CustomerName = vmodel.CustomerName,
                        CustomerCode = custcode,
                        CreditLimit = vmodel.CreditLimit != null ? (decimal)vmodel.CreditLimit : 0,
                        CreditPeriod = vmodel.CreditPeriod != null ? (int)vmodel.CreditPeriod : 0,
                        SalesPerson = vmodel.SalesPerson,

                        Location = vmodel.Location,
                        TaxRegNo = vmodel.TaxRegNo,
                        Remark = vmodel.Remark,
                        OpenClose = vmodel.OpenClose,

                        SourceOfLead = vmodel.SourceOfLead,
                        Type = CRMCustomerType.Leads,
                        LeadStat = vmodel.LeadStat,
                        LeadLevel = vmodel.LeadLevel,
                        LeadType = vmodel.LeadType,
                        CountryID = vmodel.CountryID,
                        StateID = vmodel.StateID,
                        LocationID = vmodel.LocationID,
                        LeadCondition = vmodel.LeadCondition,
                        CurrentAction = vmodel.CurrentAction,
                        NextAction = vmodel.NextAction,
                        // CreatedDate=vmodel.DateTime
                        CreatedBy = User.Identity.GetUserId(),
                        CreatedDate = System.DateTime.Now,
                        StartDate = sDate,
                        StartTime = stimes,
                        EndDate = eDate,
                        EndTime = etimes,
                        Ref1 = vmodel.Ref1,
                        Ref2 = vmodel.Ref2,
                        Ref3 = vmodel.Ref3,
                        Ref4 = vmodel.Ref4,
                        Ref5 = vmodel.Ref5,

                    };
                    if (vmodel.Ref5 != "" && vmodel.Ref5 != null)
                    {
                        var cord = com.ExtractCoordinates(vmodel.Ref5);
                        
                            cus.Lattitude = (string)cord["lat"];

                        
                            cus.Longitude = (string)cord["log"];

                        
                    }
                    string yyy = vmodel.LeadLevel;
                    db.Customers.Add(cus);
                    db.SaveChanges();
                    long custID = 0;
                    custID = cus.CustomerID;

                    //-----------------leadcustomerrelation Table----------------------
                    long CustsId = cus.CustomerID;

                    leadcustomerrelation cr = new leadcustomerrelation();
                    cr.leadid = CustsId;
                    cr.customerid = Convert.ToInt64(vmodel.CustomerID); ;

                    db.leadcustomerrelation.Add(cr);

                    db.SaveChanges();
                    //------------------End ------------------------



                    CustomerConversion userConv = new CustomerConversion
                    {
                        CustomerID = cus.CustomerID,
                        Type = CRMCustomerType.Leads,
                        ConvertFrom = "Direct",
                        ConvertedUser = UserId,
                        ConvertedDate = System.DateTime.Now,
                        CreatedUser = UserId,
                        CreatedDate = System.DateTime.Now,
                        Remarks = "Direct"
                    };
                    db.CustomerConversions.Add(userConv);
                    db.SaveChanges();

                    if (vmodel.LstContacts != null && vmodel.LstContacts.Count > 0)
                    {
                        foreach (var item in vmodel.LstContacts)
                        {

                            var contact1 = new Contact
                            {

                                Address = item.Name,
                                City = vmodel.City,
                                State = vmodel.State,
                                Country = item.Country,
                                Zip = vmodel.Zip,
                                FirstName = item.FirstName,
                                LastName = item.LastName,
                                Name = item.FirstName + " " + item.LastName,

                                TypeOfContact = item.TypeOfContact,
                                Mobile = item.Mobile,
                                Phone = item.Phone,
                                Fax = vmodel.Fax,
                                EmailId = item.EmailId,
                                Reference = vmodel.Reference,
                                ContactPerson = vmodel.ContactPerson,
                                Website = item.Website,
                                Group = 2,
                                Status = Status.active,
                                CountryID = item.CountryID,
                                ContactTypeID = item.ContactTypeID
                                //Name = cumodel.CustomerName,
                                //    Address = cumodel.Address,
                                //    City = cumodel.City,
                                //    State = cumodel.State,
                                //    Country = cumodel.Country,
                                //    Zip = cumodel.Zip,
                                //    Phone = cumodel.Phone,
                                //    //Mobile = cumodel.Mobile,
                                //    Fax = cumodel.Fax,
                                //    EmailId = cumodel.EmailId,
                                //    Reference = cumodel.Reference,
                                //    ContactPerson = cumodel.ContactPerson,
                                //    Group = 2,
                                //    Status = Status.active,

                            };




                            db.Contacts.Add(contact1);
                            db.SaveChanges();
                            contactId = contact1.ContactID;


                            ContactRelation Relation = new ContactRelation();
                            Relation.ContactID = contactId;
                            Relation.RelationType = (int)ContctRelation.lead;//for customer
                            Relation.RelationID = custID;
                            db.ContactRelation.Add(Relation);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.format != "1")
                    {

                        var contactsr = db.ContactRelation.Where(o => o.RelationID == vmodel.CustomerID && o.RelationType == 0).ToList();
                        foreach (var it in contactsr)
                        {
                            ContactRelation Relation = new ContactRelation();
                            Relation.ContactID = it.ContactID;
                            Relation.RelationType = (int)ContctRelation.lead;//for customer
                            Relation.RelationID = custID;
                            db.ContactRelation.Add(Relation);
                            db.SaveChanges();
                        }
                    }
                    //            // files upload
                    //            var uploadUrl = LegacyWeb.MapPath("~/uploads/customerdocuments/");

                    //        CutomerDocument cutomerDocument = new CutomerDocument
                    //            CutomerID = custID,
                    //            DoucumentType = item.DoucumentType,
                    //            Expiry = item.Expiry,
                    //            Notes = item.Notes,
                    //            FilePath = fileName


                    com.addlog(LogTypes.Created, UserId, "Leads", "Customers", findip(), cus.CustomerID, "Leads Added Successfully");
                    com.updateleaddate(cus.CustomerID);
                    if (vmodel.AssignedToo != null)
                    {
                        IList<AssignedTo> Assigned = new List<AssignedTo>();
                        IList<AssignedToLog> AssignLog = new List<AssignedToLog>();
                        foreach (var arr in vmodel.AssignedToo)
                        {

                            Assigned.Add(new AssignedTo()
                            {
                                CustomerID = (long)cus.CustomerID,
                                EmployeeId = arr,
                                Status = "Assigned",
                                AssignBy = UserId,
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100),
                                ChkStatus = (int)Status.active
                            });
                            var empnames = db.Employees.Where(o => o.UserId == UserId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                            var leadname = vmodel.CustomerName + " Lead Assignd by " + empnames;
                            com.remideradd("/Leads/MyLeads", arr, UserId, leadname, (long)cus.CustomerID);


                            AssignLog.Add(new AssignedToLog()
                            {
                                CustomerID = (long)cus.CustomerID,
                                EmployeeId = arr,
                                Status = "Assigned",
                                AssignedDate = System.DateTime.Now,
                                AddedUser = UserId,
                            });

                        }
                        if (Assigned != null)
                        {
                            db.AssignedTos.AddRange(Assigned);
                            db.SaveChanges();
                        }
                        if (AssignLog != null)
                        {
                            db.AssignedToLogs.AddRange(AssignLog);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.ScopeOfWork != null)
                    {
                        foreach (var em in vmodel.AssignedToo)
                        {
                            foreach (var item in vmodel.ScopeOfWork)
                            {
                                ScopeOfWorksData d = new ScopeOfWorksData();
                                d.CustomerID = (long)cus.CustomerID;
                                d.ScopeId = item;
                                d.employeeid = em;
                                db.ScopeOfWorksDatas.Add(d);
                                db.SaveChanges();

                            }
                        }
                    }
                    if (vmodel.bstmodel != null)
                    {
                        foreach (var arr in vmodel.bstmodel)
                        {
                            ScopeOfWorkRemarkChecklist remlist = new ScopeOfWorkRemarkChecklist();
                            remlist.Checklistitemid = arr.Id;
                            remlist.ScopeOfWorkItemsid = 0;
                            remlist.Note = arr.Note;
                            remlist.Check = (arr.Check == "on") ? true : false;
                            remlist.Remark = custID;
                            db.ScopeOfWorkRemarkChecklists.Add(remlist);
                            db.SaveChanges();
                        }
                    }
                    if (vmodel.AssignTypeAll != null)
                    {
                        foreach (var item in vmodel.AssignTypeAll)
                        {
                            AssignedTeams assignedTeams = new AssignedTeams();
                            assignedTeams.CustomerID = (long)cus.CustomerID;
                            assignedTeams.TeamID = item;
                            db.AssignedTeams.Add(assignedTeams);
                            db.SaveChanges();
                        }
                    }

                    // fileupload
                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/leaddocuments/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {

                                var fileCount = db.LeadDocuments.Select(a => a.LeadDocumentId).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName = Path.GetFileName(file.FileName);

                                String extension = Path.GetExtension(fileName);


                                String newName = fileCount + extension;
                                string newFName = fileCount + extension;
                                var FStatus = Status.active;
                                var thumbName = "";
                                var resizeName = "";
                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), newName);
                                file.SaveAs(newName);
                                string ldoctype = string.Empty;
                                DateTime lExpry = System.DateTime.Now;
                                string lNotes = string.Empty;
                                if (vmodel.LstLeadDocument != null && vmodel.LstLeadDocument.Count > 0)
                                {

                                    ldoctype = vmodel.LstLeadDocument[i].DoucumentType;
                                    lExpry = vmodel.LstLeadDocument[i].Expiry;
                                    lNotes = vmodel.LstLeadDocument[i].Notes;
                                }

                                var LeadDoc = new LeadDocument
                                {
                                    CustomerID = cus.CustomerID,
                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                    DoucumentType = ldoctype,
                                    Expiry = lExpry,
                                    Notes = lNotes
                                };
                                db.LeadDocuments.Add(LeadDoc);
                                db.SaveChanges();

                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    Image img = Image.FromFile(newName);
                                    int imgHeight = 100;
                                    int imgWidth = 100;
                                    if (img.Width < img.Height)
                                    {
                                        //portrait image  
                                        imgHeight = 100;
                                        var imgRatio = (float)imgHeight / (float)img.Height;
                                        imgWidth = Convert.ToInt32(img.Height * imgRatio);
                                    }
                                    else if (img.Height < img.Width)
                                    {
                                        //landscape image  
                                        imgWidth = 100;
                                        var imgRatio = (float)imgWidth / (float)img.Width;
                                        imgHeight = Convert.ToInt32(img.Height * imgRatio);
                                    }
                                    Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                                    thumb.Save(thumbName);

                                    Image lgimg = Image.FromFile(newName);
                                    if (lgimg.Width > 1800 || lgimg.Height > 1800)
                                    {
                                        Image imgs = Image.FromFile(newName);
                                        System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leaddocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }

                    //file

                    Success("Successfully Added Leads details.", true);
                    if (vmodel.format == "1")
                        return RedirectToAction("Create", "Leads");
                    else
                        return RedirectToAction("Index", "Home");

                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form..", true);
                //    ID = r.SourceOfLeadId,
                //    Name = r.SrcName
                return (View());
            }

        }
        [HttpPost]
        public ActionResult MoveLeadsupdate(long ddlEmployee,long ddlEmployee2)
        {
            db.AssignedTos.Where(o => o.EmployeeId == ddlEmployee && o.Status == "Assigned").ToList().ForEach(o => o.EmployeeId = ddlEmployee2);
            db.SaveChanges();
            return RedirectToAction("Index", "Leads");
        }
        [HttpGet]
        public ActionResult MoveLeads()
        {
            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.Empl2 = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                          }, "Value", "Text", 1);
            ViewBag.Empl = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                          }, "Value", "Text", 1);
            ViewBag.Customer = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);

            companySet();

            ViewBag.Prjct = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                             }, "Value", "Text", 1);
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");


            ViewBag.getProj = QkSelect.List(
                             new List<SelectListItem>
                             {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            return View();
        }

        // GET:/Edit
        [QkAuthorize(Roles = "Dev,Edit Leads")]
        public ActionResult Edit(long? id)
        {
            ViewBag.format = 1;
            var sc = db.ScopeOfWorksDatas.Any(o => o.CustomerID == id);
            if (sc)
            {

            }
            Customer cus = db.Customers.Find(id);
            if (cus == null)
            {
                return NotFound();
            }

            var custs = (from c in db.Customers
                         join d in db.leadcustomerrelation on c.CustomerID equals d.customerid
                         where d.leadid == id
                         select new
                         {
                             CustomerID = d.customerid,
                             CustomerDetails = c.CustomerCode + " - " + c.CustomerName

                         }).ToList();
            if (custs.Count == 0)
            {
                ViewBag.Customr = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                    new SelectListItem { Selected = false, Text = "",Value = "0"},
                                  }, "Value", "Text", 1);
            }
            else
            {
                ViewBag.Customr = QkSelect.List(custs, "CustomerID", "CustomerDetails");
            }
            //end   



            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName
                    })
                    .ToList();

            ViewBag.SalesPerson = QkSelect.List(use, "ID", "Name");
            ViewBag.CountryCodes = db.Country.ToList();


            var Country = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName,
            }).ToList();
            ViewBag.Country = QkSelect.List(Country, "Id", "Name");

            var State = db.States.Select(s => new
            {
                Id = s.StateID,
                Name = s.StateName,
            }).ToList();
            ViewBag.States = QkSelect.List(State, "Id", "Name");


            var Location = db.LocationNames.Select(s => new
            {
                Id = s.LocationId,
                Name = s.Location,
            }).ToList();
            ViewBag.Location = QkSelect.List(Location, "Id", "Name");



            ViewBag.CustName = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                            }, "Value", "Text", 1);

            ViewBag.Phone = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value =null},
                          }, "Value", "Text", 1);
            ViewBag.Mobile = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                          }, "Value", "Text", 1);
            var viewModel = new LeadsViewModel
            {
                CustomerCode = CustCode(),
            };

            var emps = db.Employees.Select(s => new { ID = s.EmployeeId, Name = s.FirstName + " " + s.LastName }).ToList();
            ViewBag.AssignedTo = QkSelect.List(emps, "ID", "Name");

            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.SrcOfLead = QkSelect.List(lead, "ID", "Name");

            var leadTypes = db.LeadTypes.Select(s => new
            {
                Id = s.TypeId,
                Name = s.Type,
            }).ToList();
            ViewBag.LeadTypes = QkSelect.List(leadTypes, "Id", "Name");



            ViewBag.LeadLevel = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = true, Text = "Select Lead Level", Value = "0"},
                       }, "Value", "Text", 1);



            var LeadStatus = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = true, Text = "Select Lead Status", Value = "0"},
                       }, "Value", "Text", 1);

            var Actions = db.LeadStatuss.Select(s => new
            {
                Id = s.LeadStatusID,
                Name = s.StatusType,
            }).OrderBy(o=>o.Name).ToList();
            ViewBag.LeadStatus = QkSelect.List(Actions, "Id", "Name");
            ViewBag.CurrentActions = QkSelect.List(Actions, "Id", "Name");
            ViewBag.NextActions = QkSelect.List(Actions, "Id", "Name");



            var LeadConditions = db.LeadCondition.Select(s => new
            {
                Id = s.id,
                Name = s.LeadCondition,
            }).ToList();
            ViewBag.LeadCondition = QkSelect.List(LeadConditions, "Id", "Name");
            ViewBag.TypeList = db.ContactTypes.ToList();


            Contact cont = db.Contacts.Find(cus.Contact);
            Accounts acc = db.Accountss.Find(cus.Accounts);
            LeadsViewModel cusmodel = new LeadsViewModel();

            cusmodel.CustomerID = cus.CustomerID;
            cusmodel.CustomerName = cus.CustomerName;
            cusmodel.CustomerCode = cus.CustomerCode;
            cusmodel.CreditLimit = cus.CreditLimit;
            cusmodel.CreditPeriod = cus.CreditPeriod;
            cusmodel.Remark = cus.Remark;
            cusmodel.SalesPerson = cus.SalesPerson;
            cusmodel.OpenClose = cus.OpenClose;
            cusmodel.Location = cus.Location;
            cusmodel.TaxRegNo = acc != null ? acc.TRN : null;
            cusmodel.SourceOfLead = cus.SourceOfLead;
            cusmodel.LeadType = cus.LeadType;
            cusmodel.LeadStat = cus.LeadStat;
            //------------ For Fetching Customer----------------------
            cusmodel.CustomerID = (from c in db.Customers
                                   join d in db.leadcustomerrelation on c.CustomerID equals d.customerid
                                   where d.leadid == id
                                   select new
                                   {
                                       c.CustomerID,


                                   }).ToList().Select(o => o.CustomerID).FirstOrDefault();
            //End
            cusmodel.NextAction = cus.NextAction;
            cusmodel.CurrentAction = cus.CurrentAction;
            cusmodel.Address = cont.Address;
            cusmodel.City = cont.City;
            cusmodel.State = cont.State;
            cusmodel.Country = cont.Country;


            cusmodel.Zip = cont.Zip;
            cusmodel.Phone = cont.Phone;
            cusmodel.Fax = cont.Fax;
            cusmodel.EmailId = cont.EmailId;
            cusmodel.Reference = cont.Reference;
            cusmodel.ContactPerson = cont.ContactPerson;

            cusmodel.EmailId = cont.EmailId;
            cusmodel.Reference = cont.Reference;
            cusmodel.ContactPerson = cont.ContactPerson;



            cusmodel.BankName = cus.BankName;
            cusmodel.AccountNo = cus.AccountNo;
            cusmodel.IbanNo = cus.IbanNo;
            cusmodel.BranchName = cus.BranchName;
            cusmodel.Swift = cus.Swift;
            cusmodel.CurrentAction = cus.CurrentAction;
            cusmodel.NextAction = cus.NextAction;
            cusmodel.CountryID = cus.CountryID;
            cusmodel.StateID = cus.StateID;
            cusmodel.LocationID = cus.LocationID;
            cusmodel.TaxRegNo = cus.TaxRegNo;

            cusmodel.Ref1 = cus.Ref1;
            cusmodel.Ref2 = cus.Ref2;
            cusmodel.Ref3 = cus.Ref3;
            cusmodel.Ref4 = cus.Ref4;
            cusmodel.Ref5 = cus.Ref5;

            cusmodel.StartDatee = (cus.StartDate != null) ? ((DateTime)cus.StartDate).ToString("dd-MM-yyyy") : "";
            cusmodel.EndDatee = (cus.EndDate != null) ? ((DateTime)cus.EndDate).ToString("dd-MM-yyyy") : "";
            cusmodel.EndTime = cus.EndTime;
            cusmodel.LeadType = cus.LeadType;

            cusmodel.LeadLevel = cus.LeadLevel;

            cusmodel.LstLeadDocument = (from cd in db.LeadDocuments
                                        where cd.CustomerID == cus.CustomerID
                                        select cd
                                         ).ToList();

            cusmodel.LstContacts = (from c in db.Contacts
                                    join cr in db.ContactRelation
                                    on new { c.ContactID, RelationType = (long)ContctRelation.lead }
                                 equals new { cr.ContactID, cr.RelationType }
                                    where (cr.RelationID == cus.CustomerID)
                                    select new
                                    {

                                        ContactID = c.ContactID
                                         ,
                                        Name = c.Name
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
                                        ContactTypeID = c.ContactTypeID,
                                        c.FirstName,
                                        c.LastName

                                    }).AsEnumerable().Select(x => new Contact
                                    {

                                        ContactID = x.ContactID,
                                        Name = x.Name
                                        ,
                                        FirstName = x.FirstName,
                                        LastName = x.LastName,
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



            var leadstate = db.LeadStatuss.Select(r => new
            {
                ID = r.LeadStatusID,
                Name = r.StatusType
            }).ToList();
            ViewBag.LeadStats = QkSelect.List(leadstate, "ID", "Name");

            var cust = db.Customers.Select(r => new
            {
                ID = r.CustomerID,
                Name = r.CustomerName
            }).ToList();
            ViewBag.CustName = QkSelect.List(cust, "ID", "Name");

            var AssignedTo = db.AssignedTos.Where(x => x.CustomerID == id && x.Status == "Assigned" && x.ChkStatus == (int)Status.active).Select(a => a.EmployeeId).ToList().ToArray() ?? null;
            cusmodel.AssignedToo = AssignedTo;

            var emp = db.Employees.Select(s => new { ID = s.EmployeeId, Name = s.FirstName + " " + s.LastName }).ToList();
            ViewBag.team = new MultiSelectList(emp, "ID", "Name", AssignedTo);
            var scworks = db.LeadTypes.Select(s => new { ID = s.TypeId, Name = s.Type }).ToList();
            var asignscworks = db.ScopeOfWorksDatas.Where(o => o.CustomerID == id).Select(o => o.ScopeId).Distinct().ToList().ToArray();

            var AssignedTeam = db.AssignedTeams.Where(a => a.CustomerID == id).Select(a => a.TeamID).ToList().ToArray();
            cusmodel.AssignTypeAll = AssignedTeam;
            ViewBag.DocList = db.DocumentTypes.ToList();
            cusmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Leads" && a.Status == Status.active).ToList();
            cusmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Leads").ToList();
            var ref1 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref1,
                 Name = s.Ref1
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.Customers
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.Customers
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            ViewBag.image = (from b in db.LeadDocuments
                             join c in db.Customers on b.CustomerID equals c.CustomerID
                             where c.CustomerID == id
                             select new LeadDocumentViewModel
                             {
                                 LeadDocumentId = b.LeadDocumentId,
                                 CustomerID = b.CustomerID,
                                 FileName = b.FileName,

                             }).ToList();

            ViewBag.preEntry = db.Customers.Where(a => a.CustomerID < id && a.Type == CRMCustomerType.Leads).Select(a => a.CustomerID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Customers.Where(a => a.CustomerID > id && a.Type == CRMCustomerType.Leads).Select(a => a.CustomerID).DefaultIfEmpty().Min();







            cusmodel.AssignTypeAll = db.AssignedTeams.Where(a => a.CustomerID == id).Select(a => a.TeamID).ToList().ToArray() ?? null;

            var Teams = db.Teams
                    .Select(s => new
                    {
                        ID = s.TeamId,
                        Name = s.TeamName
                    })
                    .ToList();
            ViewBag.AssignTypes = new MultiSelectList(Teams, "ID", "Name", cusmodel.AssignTypeAll);

            cusmodel.AssignedTo = db.AssignedTos.Where(x => x.CustomerID == id && x.Status == "Assigned" && x.ChkStatus == (int)Status.active).Select(a => a.EmployeeId).ToList().ToArray() ?? null;

            var TMembers = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.Employees = new MultiSelectList(TMembers, "ID", "Name", cusmodel.AssignedTo);


            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;














            /*
            IList<AssignedTo> Assigned = new List<AssignedTo>();
                IList<AssignedToLog> AssignLog = new List<AssignedToLog>();
                foreach (var arr in vmodel.AssignedTo)
                {

                    Assigned.Add(new AssignedTo()
                    {
                        CustomerID = (long)cus.CustomerID,
                        EmployeeId = arr,
                        Status = "Assigned",
                        AssignBy = UserId,
                        CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100),
                        ChkStatus = (int)Status.active
                    });

                    AssignLog.Add(new AssignedToLog()
                    {
                        CustomerID = (long)cus.CustomerID,
                        EmployeeId = arr,
                        Status = "Assigned",
                        AssignedDate = System.DateTime.Now,
                        AddedUser = UserId,
                    });

                }
                if (Assigned != null)
                {
                    db.AssignedTos.AddRange(Assigned);
                    db.SaveChanges();
                }
                if (AssignLog != null)
                {
                    db.AssignedToLogs.AddRange(AssignLog);
                    db.SaveChanges();
                }
         

            if (vmodel.AssignTypeAll != null)
            {
                foreach (var item in vmodel.AssignTypeAll)
                {
                    AssignedTeams assignedTeams = new AssignedTeams();
                    assignedTeams.CustomerID = (long)cus.CustomerID;
                    assignedTeams.TeamID = item;
                    db.AssignedTeams.Add(assignedTeams);
                }
            }




            */



            if (cusmodel.OpenClose == 0)
            {

                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
                ViewBag.OpnCls = pstat2;

            }
            else if (cusmodel.OpenClose == 1)
            {

                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                }
              };
                ViewBag.OpnCls = pstat2;
            }
            else
            {
                List<SelectListItem> pstat2 = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.OpnCls = pstat2;
            }

           
                ViewBag.LeadType = new MultiSelectList(scworks, "ID", "Name", asignscworks);

            
            return View(cusmodel);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Leads")]
        public ActionResult Edit(LeadsSubmitViewModel cusmodel, Int64 id)
        {
            bool stat = false;
            string msg;
          

            if (ModelState.IsValid)
            {
                DateTime? eDate = null;
                DateTime? etimes = null;
                if (cusmodel.EndDatee != null)
                {
                    eDate = DateTime.Parse(cusmodel.EndDatee, new CultureInfo("en-GB"));
                    TimeSpan? etime = null;
                    if (cusmodel.EndTime != null)
                    {
                        etime = ((DateTime)cusmodel.EndTime).TimeOfDay;
                    }
                    etimes = eDate + etime;
                }
                DateTime? sDate = null;
                DateTime? stimes = null;
                if (cusmodel.StartDatee != null)
                {
                    sDate = DateTime.Parse(cusmodel.StartDatee, new CultureInfo("en-GB"));
                    TimeSpan? etime = null;
                    if (cusmodel.StartTime != null)
                    {
                        etime = ((DateTime)cusmodel.StartTime).TimeOfDay;
                    }
                    stimes = sDate + etime;
                }

                var UserId = User.Identity.GetUserId();
                var CodeExists = db.Customers.Any(u => u.CustomerCode == cusmodel.CustomerCode && u.CustomerID != id);
                if (CodeExists)
                {

                    Danger("Leads with same Leads code exists.", true);
                    return RedirectToAction("Edit", "Leads");
                }
                else
                {

                    Customer cus = db.Customers.Find(id);




                    if (cusmodel.EndDatee != null)
                    {
                        eDate = DateTime.Parse(cusmodel.EndDatee.ToString(), new CultureInfo("en-GB"));
                    }
                    ViewBag.format = 1;


                    var sc = db.ScopeOfWorksDatas.Any(o => o.CustomerID == id);
                    if (sc)
                    {
                        ViewBag.format = 2;

                    }
                    var scop = db.LeadTypes.Where(o => o.TypeId == 0).Select(o => o.Type).ToList().ToArray();
                    if (cusmodel.ScopeOfWork != null)
                        scop = db.LeadTypes.Where(o => cusmodel.ScopeOfWork.Contains(o.TypeId)).Select(o => o.Type).ToList().ToArray();

                    if (ViewBag.format == 2)
                    {
                        cus.TaxRegNo = cusmodel.StartDatee + "-" + cusmodel.CustomerName + "-" + string.Join(",", scop);
                        cus.CustomerName = cus.TaxRegNo;

                    }
                    else
                    {
                        cus.CustomerName = cusmodel.CustomerName;
                        cus.TaxRegNo = cusmodel.TaxRegNo;

                    }
                    cus.CustomerCode = cusmodel.CustomerCode;
                    cus.CreditLimit = cusmodel.CreditLimit != null ? (decimal)cusmodel.CreditLimit : 0;
                    cus.CreditPeriod = cusmodel.CreditPeriod != null ? (int)cusmodel.CreditPeriod : 0;
                    cus.Remark = cusmodel.Remark;
                    cus.SalesPerson = cusmodel.SalesPerson;
                    cus.LeadType = cusmodel.LeadType;
                    cus.OpenClose = cusmodel.OpenClose;

                    cus.LocationID = cusmodel.LocationID;
                    cus.SourceOfLead = cusmodel.SourceOfLead;
                    cus.LeadStat = cusmodel.LeadStat;
                    cus.LeadLevel = cusmodel.LeadLevel;
                    cus.CurrentAction = cusmodel.CurrentAction;
                    cus.NextAction = cusmodel.NextAction;

                    cus.BankName = cusmodel.BankName;
                    cus.AccountNo = cusmodel.AccountNo;
                    cus.BranchName = cusmodel.BranchName;
                    cus.IbanNo = cusmodel.IbanNo;
                    cus.Swift = cusmodel.Swift;
                    cus.StartDate = sDate;
                    cus.StartTime = stimes;
                    cus.EndDate = eDate;
                    cus.EndTime = etimes;
                    cus.Ref1 = cusmodel.Ref1;
                    cus.Ref2 = cusmodel.Ref2;
                    cus.Ref3 = cusmodel.Ref3;
                    cus.Ref4 = cusmodel.Ref4;
                    cus.Ref5 = cusmodel.Ref5;
                    cus.logtime = System.DateTime.Now;
                    db.Entry(cus).State = EntityState.Modified;
                    db.SaveChanges();

                    var CustsId = cus.CustomerID;
                    db.leadcustomerrelation.RemoveRange(db.leadcustomerrelation.Where(a => a.leadid == CustsId));


                    leadcustomerrelation mt = new leadcustomerrelation();
                    if (CustsId != null)
                    {
                        mt.leadid = CustsId;
                    }
                    else
                    {
                        mt.leadid = 0;
                    }
                    mt.customerid = Convert.ToInt64(cusmodel.CustomerID);


                    db.leadcustomerrelation.Add(mt);
                    db.SaveChanges();



                    Int64 CustId = cus.CustomerID;

                    if (cusmodel.AssignedToo != null)
                    {
                        var assignlist = db.AssignedTos.Where(a => a.CustomerID == id);
                        IList<AssignedToLog> UnAssignLog = new List<AssignedToLog>();
                        foreach (var ast in assignlist)
                        {
                            if (cusmodel.AssignedToo.Contains(ast.EmployeeId))
                            {
                                // exist
                            }
                            else
                            {
                                //    CustomerID = cus.CustomerID,
                                //    EmployeeId = ast.EmployeeId,
                                //    Status = "Un Assigned",
                                //    AssignedDate = System.DateTime.Now,
                                //    AddedUser = UserId,


                                UnAssignLog.Add(new AssignedToLog()
                                {
                                    CustomerID = (long)cus.CustomerID,
                                    EmployeeId = ast.EmployeeId,
                                    Status = "Unassigned",
                                    AssignedDate = System.DateTime.Now,
                                    AddedUser = UserId,
                                });

                            }

                        }
                        if (UnAssignLog != null)
                        {
                            db.AssignedToLogs.AddRange(UnAssignLog);
                            db.SaveChanges();
                        }
                        IList<AssignedToLog> AssignLog = new List<AssignedToLog>();
                        foreach (var assito in cusmodel.AssignedToo)
                        {
                            var x = db.AssignedTos.Where(a => a.EmployeeId == assito && a.CustomerID == cus.CustomerID);
                            if (!x.Any())
                            {

                                AssignLog.Add(new AssignedToLog()
                                {
                                    CustomerID = (long)cus.CustomerID,
                                    EmployeeId = assito,
                                    Status = "Assigned",
                                    AssignedDate = System.DateTime.Now,
                                    AddedUser = UserId,
                                });

                            }
                        }

                        if (AssignLog != null)
                        {
                            db.AssignedToLogs.AddRange(AssignLog);
                            db.SaveChanges();
                        }
                    }
                    var assig = db.AssignedTos.Where(a => a.CustomerID == id);
                    if (assig != null)
                    {
                        db.AssignedTos.RemoveRange(db.AssignedTos.Where(a => a.CustomerID == id));
                        db.SaveChanges();
                    }
                    var assigg = db.ScopeOfWorksDatas.Where(a => a.CustomerID == id);
                    if (assigg != null)
                    {
                        db.ScopeOfWorksDatas.RemoveRange(db.ScopeOfWorksDatas.Where(a => a.CustomerID == id));
                        db.SaveChanges();
                    }

                    if (cusmodel.ScopeOfWork != null)
                    {
                        foreach (var emp in cusmodel.AssignedToo)
                        {
                            foreach (var item in cusmodel.ScopeOfWork)
                            {
                                ScopeOfWorksData d = new ScopeOfWorksData();
                                d.CustomerID = (long)cus.CustomerID;
                                d.ScopeId = item;
                                d.employeeid = emp;
                                db.ScopeOfWorksDatas.Add(d);
                                db.SaveChanges();

                            }
                        }
                    }
                    var assiggg = db.ScopeOfWorkRemarkChecklists.Where(a => a.Remark == id);
                    if (assiggg != null)
                    {
                        db.ScopeOfWorkRemarkChecklists.RemoveRange(db.ScopeOfWorkRemarkChecklists.Where(a => a.Remark == id));
                        db.SaveChanges();
                    }
                    if (cusmodel.bstmodel != null)
                    {
                        foreach (var arr in cusmodel.bstmodel)
                        {
                            ScopeOfWorkRemarkChecklist remlist = new ScopeOfWorkRemarkChecklist();
                            remlist.Checklistitemid = arr.Id;
                            remlist.ScopeOfWorkItemsid = 0;
                            remlist.Note = arr.Note;
                            remlist.Check = (arr.Check == "on") ? true : false;
                            remlist.Remark = id;
                            db.ScopeOfWorkRemarkChecklists.Add(remlist);
                            db.SaveChanges();
                        }
                    }
                    ViewBag.DocList = db.DocumentTypes.ToList();
                    if (cusmodel.AssignedToo != null)
                    {
                        IList<AssignedTo> Assigned = new List<AssignedTo>();


                        foreach (var arr in cusmodel.AssignedToo)
                        {



                            Assigned.Add(new AssignedTo()
                            {
                                CustomerID = (long)cus.CustomerID,
                                EmployeeId = arr,
                                Status = "Assigned",
                                AssignBy = UserId,
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100),
                                ChkStatus = (int)Status.active
                            });
                         //   com.remideradd("http://uk.ath.cx:1091/Leads/MyLeads", arr, UserId, "Lead Assigned", id);
                            var empnames = db.Employees.Where(o => o.UserId == UserId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                            var leadname = cusmodel.CustomerName + " Lead Assignd by " + empnames;
                            com.remideradd("/Leads/MyLeads", arr, UserId, leadname, id);

                        }
                        if (Assigned != null)
                        {
                            db.AssignedTos.AddRange(Assigned);
                            db.SaveChanges();

                        }
                    }
                    if (cusmodel.AssignTypeAll != null)
                    {

                        var Teams = db.AssignedTeams.Where(s => s.CustomerID == (long)cus.CustomerID);

                        if (Teams.Count() > 0)
                        {
                            foreach (var item in Teams)
                            {
                                db.AssignedTeams.Remove(item);
                            }
                            db.SaveChanges();

                        }



                        foreach (var item in cusmodel.AssignTypeAll)
                        {
                            AssignedTeams assignedTeams = new AssignedTeams();
                            assignedTeams.CustomerID = (long)cus.CustomerID;
                            assignedTeams.TeamID = item;
                            db.AssignedTeams.Add(assignedTeams);
                            db.SaveChanges();
                        }
                    }
                    // to remove documents

                    // fileupload


                    if (cusmodel.LstLeadDocument != null && cusmodel.LstLeadDocument.Count > 0)//Edit already existing file values
                    {
                        IFormFileCollection files = Request.Form.Files;
                        string path = LegacyWeb.MapPath("~/uploads/leaddocuments/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        int count = 0;
                        foreach (var item in cusmodel.LstLeadDocument)
                        {

                            IFormFile file = Request.Form.Files["LstLeadDocument[" + count + "].FileName"];

                            if (file.FileName != "")
                            {
                                var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                var uploadUrl = LegacyWeb.MapPath("~/uploads/leaddocuments/");
                                file.SaveAs(Path.Combine(uploadUrl, fileNames));
                                var leadDocument = new LeadDocument
                                {
                                    DocumentTypeID = item.DocumentTypeID,
                                    Expiry = item.Expiry,
                                    Notes = item.Notes,
                                    FileName = fileNames,
                                    CustomerID = cus.CustomerID,
                                    CreatedDate = System.DateTime.Now
                                };

                                if (item.LeadDocumentId > 0)
                                {
                                    LeadDocument cn = db.LeadDocuments.Find(item.LeadDocumentId);

                                    cn.CreatedDate = System.DateTime.Now;
                                    cn.Expiry = item.Expiry;
                                    cn.Notes = item.Notes;
                                    cn.FileName = fileNames;
                                    db.Entry(cn).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    db.LeadDocuments.Add(leadDocument);
                                    db.SaveChanges();
                                }

                            }

                            count++;

                        }






                    }

                    if (cusmodel.LstContacts != null && cusmodel.LstContacts.Count > 0)
                    {
                        long contactId = 0;
                        long[] contactRelationids = new long[cusmodel.LstContacts.Count];
                        int rowRelationcount = 0;
                        foreach (var item in cusmodel.LstContacts)
                        {
                            var contact = new Contact
                            {
                                ContactID = item.ContactID,
                                Name = cusmodel.CustomerName,
                                Address = cusmodel.Address,
                                City = cusmodel.City,
                                State = cusmodel.State,
                                Country = item.Country,
                                Zip = cusmodel.Zip,


                                TypeOfContact = item.TypeOfContact,
                                Mobile = item.Mobile,
                                Phone = item.Phone,
                                Fax = cusmodel.Fax,
                                EmailId = item.EmailId,
                                FirstName = item.FirstName,
                                LastName = item.LastName,

                                Website = item.Website,
                                Reference = cusmodel.Reference,
                                ContactPerson = cusmodel.ContactPerson,

                                Group = 2,
                                Status = Status.active,
                                CountryID = item.CountryID,
                                ContactTypeID = item.ContactTypeID

                            };
                            if (item.ContactID > 0)
                            {
                                Contact cnt = db.Contacts.Find(item.ContactID);
                                cnt.ContactTypeID = item.ContactTypeID;
                                cnt.Mobile = item.Mobile;
                                cnt.FirstName = item.FirstName;
                                cnt.LastName = item.LastName;
                                cnt.Name = item.Name;
                                cnt.EmailId = item.EmailId;
                                cnt.ContactID = item.ContactID;
                                cnt.Website = item.Website;
                                cnt.CountryID = item.CountryID;

                                db.Entry(cnt).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                db.Contacts.Add(contact);
                                db.SaveChanges();
                                contactId = contact.ContactID;
                                var ContactReltion = (from a in db.ContactRelation
                                                      where a.RelationID == id && a.RelationType == (long)ContctRelation.lead && a.ContactID == contact.ContactID
                                                      select new
                                                      {
                                                          a.ContactRelationID,
                                                          a.ContactID,
                                                          a.RelationType,
                                                          a.RelationID
                                                      }).FirstOrDefault();



                                ContactRelation Relation = new ContactRelation();
                                Relation.ContactID = contact.ContactID;
                                Relation.RelationType = (long)ContctRelation.lead;//for customer
                                Relation.RelationID = (long)cus.CustomerID;
                                db.ContactRelation.Add(Relation);
                                db.SaveChanges();
                                contactRelationids[rowRelationcount] = Relation.ContactRelationID;
                                rowRelationcount++;

                            }








                        }



                    }









                    com.addlog(LogTypes.Updated, UserId, "Leads", "Customers", findip(), cus.CustomerID, "Leads Updated Successfully");
                    com.updateleaddate(cus.CustomerID);
                   if(cusmodel.converttotask)
                    {
                        string taskname = cusmodel.CustomerName;
                       var taskexists = db.ProTasks.Any(o => (o.VManuId == 999||o.VManuId==998) && o.VModId==id);
                        if (!taskexists)
                        {

                            ProTask amctask = new ProTask();
                            amctask.TaskNo = GetProNo();
                            amctask.TaskCode = InvoiceNo();
                            amctask.TaskName = cusmodel.CustomerName;
                            
                            amctask.Location = (cusmodel.Location != null) ? cusmodel.Location:"OLD";
                            amctask.StartDate = System.DateTime.Now;
                            amctask.CreatedDate = System.DateTime.Now;
                            amctask.CreatedBy = cus.CreatedBy;
                            amctask.Status = Status.active;
                            amctask.Branch = 1;
                     
                            amctask.logtime = System.DateTime.Now;
                            amctask.CustomerID = cusmodel.CustomerID;
                            amctask.VManuId = 999;
                            amctask.VModId = id;

                      

                            db.ProTasks.Add(amctask);
                            db.SaveChanges();
                            Int64 proId = amctask.ProTaskId;
                            ProTaskUpdation TaskUp = new ProTaskUpdation
                            {
                                ProTaskId = proId,
                                //Status = TKUpdateStatus.Created,
                                CreatedBy = UserId,
                                CreatedDate = System.DateTime.Now,
                                //TaskTeamId = teamId
                            };
                            db.ProTaskUpdations.Add(TaskUp);
                            db.SaveChanges();
                            Int64 TaskUpdId = TaskUp.TaskUpdationID;
                            if (cusmodel.LstContacts != null && cusmodel.LstContacts.Count > 0)
                            {
                                foreach (var item in cusmodel.LstContacts)
                                {
                                    var contact = new Contact
                                    {

                                        Address = item.Name,


                                        Country = item.Country,

                                        FirstName = item.FirstName,
                                        LastName = item.LastName,
                                        Name = item.FirstName + " " + item.LastName,

                                        TypeOfContact = item.TypeOfContact,
                                        Mobile = item.Mobile,
                                        Phone = item.Phone,

                                        EmailId = item.EmailId,

                                        ContactPerson = item.Name,
                                        Website = item.Website,
                                        Group = 2,
                                        Status = Status.active,
                                        CountryID = item.CountryID,
                                        ContactTypeID = item.ContactTypeID

                                    };
                                    db.Contacts.Add(contact);
                                    db.SaveChanges();
                                    var contactId = contact.ContactID;
                                    var mob = new Mobile
                                    {
                                        Contact = contactId,
                                        MobileNum = item.Mobile,
                                        Name = item.FirstName + " " + item.LastName
                                    };
                                    db.Mobiles.Add(mob);
                                    db.SaveChanges();
                                    var mob2 = new TaskMobile
                                    {
                                        ProTaskId = proId,
                                        MobileNo = item.Mobile,
                                        Name = item.FirstName + " " + item.LastName
                                    };
                                    db.TaskMobiles.Add(mob2);
                                    db.SaveChanges();

                                    ContactRelation Relation = new ContactRelation();
                                    Relation.ContactID = contactId;
                                    Relation.RelationType = 11;//for customer
                                    Relation.RelationID = proId;
                                    db.ContactRelation.Add(Relation);
                                    db.SaveChanges();
                                }
                            }

                        }
                    }
                    db.Reminders.RemoveRange(db.Reminders.Where(o => o.Note.Contains("24 Hours leads not Updation ")));
                    db.SaveChanges();
                    db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.Type == "leadStillpending"));
                    db.SaveChanges();
                    var systime = System.DateTime.Now;
                    var rem = (from a in db.Customers
                               join b in db.AssignedTos on a.CustomerID equals b.CustomerID
                               join c in db.LeadStatuss on a.CurrentAction equals c.LeadStatusID into statt
                               from c in statt.DefaultIfEmpty()
                               where EF.Functions.DateDiffHour(systime, a.logtime) < -24
                               && a.Type == CRMCustomerType.Leads

                          && a.OpenClose == 0

                              && b.Status == "Assigned" && b.ChkStatus == 0 &&
                              
                              b.EmployeeId != 10192 && b.EmployeeId != 10245

                               select new
                               {
                                   b.EmployeeId,
                                   a.CustomerID,
                                   c.StatusType,
                                   timedifference = EF.Functions.DateDiffHour(systime, a.logtime),

                                   taskname = a.CustomerCode + "-" + a.CustomerName,

                               }).Distinct();

                    if (rem.Count() > 0)
                    {
                        var pids = rem.Select(o => new
                        {
                            o.CustomerID,
                            o.StatusType,
                            o.taskname,


                        }).Distinct().ToList();
                        foreach (var pid in pids)
                        {
                            string tasknote = "24 Hours leads not Updation <br> Lead Status : " + pid.StatusType + "<br> Lead name : " + pid.taskname;
                            var remexist = db.Reminders.Any(o => o.Note == tasknote && o.Reference == pid.CustomerID);
                            if (!remexist)
                            {
                                Reminder reminds = new Reminder();
                                reminds.Reference = pid.CustomerID;
                                reminds.Note = tasknote;// "Task Still " +pid.Ref1+" <br> Task Status : " + pid.StatusName + "<br> Task name : " + pid.taskname;

                                var rDate = System.DateTime.Now.Date;
                                //seleted date added,for fullcalender



                                reminds.RDate = System.DateTime.Now;
                                reminds.Type = "/leads/Details/" + pid.CustomerID;
                                reminds.RStatus = "Close";
                                reminds.RequestBy = User.Identity.GetUserId();

                                reminds.CreatedBy = reminds.RequestBy;
                                reminds.Status = Status.active;
                                reminds.CreatedDate = System.DateTime.Now;
                                db.Reminders.Add(reminds);
                                db.SaveChanges();
                                long idd = reminds.ReminderId;
                                var asseimp = rem.Where(o => o.CustomerID == pid.CustomerID).Select(o => o.EmployeeId).Distinct().ToList().ToArray();
                                var myemps = asseimp.Distinct().ToList().ToArray();
                                foreach (var arr in myemps)
                                {

                                    var exists = db.ReminderAssigneds.Any(o => o.EntryId == pid.CustomerID && o.Type == "leadStillpending" && o.EmployeeId == arr);



                                    if (!exists)
                                    {
                                        ReminderAssigned remAs = new ReminderAssigned();

                                        remAs.ReminderId = idd;
                                        remAs.EntryId = pid.CustomerID;
                                        remAs.Type = "leadStillpending";
                                        remAs.EmployeeId = arr;
                                        db.ReminderAssigneds.Add(remAs);
                                        db.SaveChanges();

                                    }
                                }
                            }

                        }
                    }


                    msg = "Successfully updated Leads details";
                    stat = true;
                    ViewBag.format = 1;


                    sc = db.ScopeOfWorksDatas.Any(o => o.CustomerID == id);
                    if (sc)
                    {
                        ViewBag.format = 2;

                    }
                    if (ViewBag.format == 1)
                    {
                        Success("Successfully Updated Lead Details.", true);
                        return RedirectToAction("Index", "Leads");
                    }
                    else
                        return RedirectToAction("Edit/" + id.ToString(), "Leads");


                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form..";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }


        //GET: customer/Delete/5
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete Leads")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer Cus = db.Customers.Find(id);
            if (Cus == null)
            {
                return NotFound();
            }
            return PartialView(Cus);
        }

        // POST: customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [QkAuthorize(Roles = "Dev,Delete Leads")]
        public ActionResult DeleteAction(long? id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var custcon = db.CustomerConversions.Where(a => a.CustomerID == id);
            if (custcon != null)
            {
                db.CustomerConversions.RemoveRange(db.CustomerConversions.Where(a => a.CustomerID == id));
            }
            Customer Cus = db.Customers.Find(id);

            Contact con = db.Contacts.Find(Cus.Contact);
            if (con != null)
            {
                db.Contacts.Remove(con);
                db.SaveChanges();
            }


            // delete documents
            var ldoc = db.LeadDocuments.Where(a => a.CustomerID == id);
            if (ldoc != null)
            {
                db.LeadDocuments.RemoveRange(db.LeadDocuments.Where(a => a.CustomerID == id));
                db.SaveChanges();
            }


            //Delete from leadcustomerrelation table
            var custlead = db.leadcustomerrelation.Where(a => a.leadid == id);
            if (custlead != null)
            {
                db.leadcustomerrelation.RemoveRange(db.leadcustomerrelation.Where(a => a.leadid == id));
                db.SaveChanges();
            }
            //end




            // delete assigned to
            var assign = db.AssignedTos.Where(a => a.CustomerID == id);
            if (assign != null)
            {
                db.AssignedTos.RemoveRange(db.AssignedTos.Where(a => a.CustomerID == id));
                db.SaveChanges();
            }
            var rems = db.LeadRemarks.Where(r => r.CustomerID == id);
            if (rems != null)
            {
                db.LeadRemarks.RemoveRange(db.LeadRemarks.Where(r => r.CustomerID == id));
                db.SaveChanges();
            }
            db.Customers.Remove(Cus);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "Leads", "Customers", findip(), Cus.CustomerID, "Leads Deleted Successfully");
            com.updateleaddate(Cus.CustomerID);
            stat = true;
            msg = "Successfully Deleted Leads details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Leads")]
        public ActionResult DeleteAllLeads(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = DeleteLeads(arr);
                if (chk == true)
                {
                    count++;
                }
            }
            Success("Deleted " + count + " Leads.", true);
            return RedirectToAction("Index", "Leads");
        }
        private Boolean DeleteLeads(long id)
        {
            var UserId = User.Identity.GetUserId();
            var custcon = db.CustomerConversions.Where(a => a.CustomerID == id);
            if (custcon != null)
            {
                db.CustomerConversions.RemoveRange(db.CustomerConversions.Where(a => a.CustomerID == id));
                db.SaveChanges();
            }
            Customer Cus = db.Customers.Find(id);

            Contact con = db.Contacts.Find(Cus.Contact);
            if (con != null)
            {
                var mobile = db.Mobiles.Where(a => a.Contact == con.ContactID);
                if (mobile != null)
                {
                    db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == con.ContactID));
                    db.SaveChanges();
                }

                db.Contacts.Remove(con);
                db.SaveChanges();
            }

            var re = db.LeadRemarks.Where(a => a.CustomerID == id);
            if (re != null)
            {
                db.LeadRemarks.RemoveRange(db.LeadRemarks.Where(a => a.CustomerID == id));
                db.SaveChanges();
            }

            var asg = db.AssignedTos.Where(a => a.CustomerID == id);
            if (asg != null)
            {
                db.AssignedTos.RemoveRange(db.AssignedTos.Where(a => a.CustomerID == id));
                db.SaveChanges();
            }

            db.Customers.Remove(Cus);
            db.SaveChanges();
            com.updateleaddate(Cus.CustomerID);
            com.addlog(LogTypes.Deleted, UserId, "Leads", "Customers", findip(), Cus.CustomerID, "Leads Deleted Successfully");
            return true;
        }

        [HttpGet]
     
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer cus = db.Customers.Find(id);
            if (cus == null)
            {
                return NotFound();
            }
            LeadsDetailsViewModel cusmodel = new LeadsDetailsViewModel();

            cusmodel.CustomerName = cus.CustomerName;
            cusmodel.CustomerCode = cus.CustomerCode;
            cusmodel.CreditLimit = cus.CreditLimit;
            cusmodel.CreditPeriod = cus.CreditPeriod;

            cusmodel.Remark = cus.Remark;
            cusmodel.SalesEmp = db.Employees.Where(a => a.EmployeeId == cus.SalesPerson).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();

            cusmodel.Location = cus.Location;
            cusmodel.SrcLead = db.LeadStatuss.Where(a => a.LeadStatusID == cus.LeadStat).Select(a => a.StatusType).FirstOrDefault();

            cusmodel.BankName = cus.BankName;
            cusmodel.AccountNo = cus.AccountNo;
            cusmodel.BranchName = cus.BranchName;
            cusmodel.IbanNo = cus.IbanNo;
            cusmodel.Swift = cus.Swift;

            Accounts acc = db.Accountss.Find(cus.Accounts);
            cusmodel.TaxRegNo = cus.TaxRegNo;

            var mob = (from co in db.Contacts
                       join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                       where (rrr.RelationID == id && rrr.RelationType == 2)
                       select new MobileViewModel
                       {
                           Num = co.Mobile,
                           Name = co.FirstName + " " + co.LastName

                       }).ToList();



            var leaddoc = db.LeadTaskImages.Where(a => a.TaskId == id).ToList();
            if (1 == 1)
            {
                cusmodel.LeadDocuments = (from a in db.LeadTaskImages
                                          where a.TaskId == id
                                          select new LeadDocumentViewModel
                                          {
                                              CustomerID = a.TaskId,
                                              LeadDocumentId = a.TaskImageId,
                                              FileName = a.FileName,

                                          }).ToList();


            }


            var doc = (from a in db.LeadDocuments
                       where a.CustomerID == id
                       select new LeadDocumentViewModel
                       {
                           CustomerID = a.CustomerID,
                           LeadDocumentId = a.LeadDocumentId,
                           FileName = a.FileName,
                       }).ToList();
            if (doc.Count > 0)
            {
                cusmodel.LeadcreateDocuments = doc.ToList();
            }
            cusmodel.check = (from z in db.ScopeOfWorkRemarkChecklists
                              join y in db.ScopeOfWorkItems on z.Checklistitemid equals y.Id into img
                              from y in img.DefaultIfEmpty()
                              where z.Remark == id
                              select new ChecklistViewModel
                              {
                                  Name = y.ListName,
                                  Note = z.Note,
                                  Chck = z.Check,

                              }).ToList();
            cusmodel.allscope = (from a in db.ScopeOfWorksDatas
                                 join b in db.LeadTypes on a.ScopeId equals b.TypeId
                                 let check = (from z in db.ScopeOfWorkRemarkChecklists
                                              join y in db.ScopeOfWorkItems on z.Checklistitemid equals y.Id into img
                                              from y in img.DefaultIfEmpty()
                                              join x in db.ScopeOfWorks on y.Checklist equals x.ChecklistId
                                              where z.Remark == a.CustomerID && x.Stage == b.TypeId
                                              select new ChecklistViewModel
                                              {
                                                  Name = y.ListName,
                                                  Note = z.Note,
                                                  Chck = z.Check,

                                              }).Distinct().ToList()
                                 where a.CustomerID == id
                                 select new workscops
                                 {
                                     workname = b.Type,
                                     checklist = check,
                                     explanation = cusmodel.Remark,

                                 }
                              ).ToList();
            cusmodel.leadStausDisp = (from c in db.Customers
                                      join d in db.customerleadrelation on c.CustomerID equals d.customerid
                                      where d.leadid == id
                                      select new
                                      {
                                          c.CustomerName,
                                          c.CustomerID,
                                          link = "<a href='/Customer/Details/" + c.CustomerID + "'>" + c.CustomerName + "</a>"
                                      }).ToList().Select(o => o.link).FirstOrDefault();
            // 
            cusmodel.LeadAssign = (from a in db.AssignedTos
                                   join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                                   from b in emp.DefaultIfEmpty()
                                   where a.CustomerID == id
                                   select new LeadAssignToViewModel
                                   {

                                       Empname = b.FirstName + " " + b.MiddleName + " " + b.LastName + ",",
                                   }).ToList();

            cusmodel.LeadCreated = (from a in db.Customers
                                    join b in db.CustomerConversions on a.CustomerID equals b.CustomerID into conv
                                    from b in conv.DefaultIfEmpty()
                                    join c in db.Users on b.CreatedUser equals c.Id into emp
                                    from c in emp.DefaultIfEmpty()
                                    where a.CustomerID == id
                                    select new LeadCreatedViewModel
                                    {
                                        CreatedUser = c.UserName,
                                        CreatedDate = b.CreatedDate.ToString(),
                                        Converted = b.ConvertFrom,
                                    }).ToList();
            //LeadActivity
            string ids = id.ToString();

            // Lead Time Line

            var lact = (from z in db.LeadTaskUpdations

                        join cust in db.Customers on z.TaskId equals cust.CustomerID into custt
                        from cust in custt.DefaultIfEmpty()
                        join st in db.LeadStatuss on z.leadstatus equals st.LeadStatusID into lss
                        from st in lss.DefaultIfEmpty()




                        join b in db.Users on z.CreatedBy equals b.Id into emp
                        from b in emp.DefaultIfEmpty()



                        where (z.TaskId == id)

                        select new LeadTimelineViewModel
                        {
                            TStatus = st.StatusType,
                            Name = b.UserName,
                            LogType = "Updated",
                            Time = z.CreatedDate,
                            Details = z.Remarks,
                            RImages = (from zz in db.LeadTaskUpdations
                                       join img in db.LeadTaskImages on zz.TaskUpdationID equals img.TaskUpdationID
                                       where (img.TaskUpdationID == z.TaskUpdationID)
                                       select new TaskImageViewModel
                                       {
                                           TaskId = zz.TaskUpdationID,
                                           TaskImageId = img.TaskImageId,
                                           FileName = img.FileName,
                                       }).ToList(),


                            //details = b.UserName + " " + Enum.GetName(typeof(LogTypes), a.LogType) +" "+ a.LogSection +" on "+a.LogTime
                        }).ToList();

            var rem = (from a in db.LeadTaskRemarks

                       join b in db.Users on a.AddedUser equals b.Id into emp
                       from b in emp.DefaultIfEmpty()
                       where a.TaskId == id && a.Remark != null && (a.TaskUpdationID == 1 && a.TaskStatusID == 9999)

                       select new LeadTimelineViewModel
                       {
                           TStatus = "Remark Added",
                           Name = b.UserName,
                           LogType = "Remark Added",
                           Time = a.CreatedDate,
                           Details = a.Remark,
                       }).ToList();



            var asl = (from a in db.AssignedToLogs
                       join c in db.Users on a.AddedUser equals c.Id into usr
                       from c in usr.DefaultIfEmpty()
                       join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                       from b in emp.DefaultIfEmpty()

                       where a.CustomerID == id
                       select new LeadTimelineViewModel
                       {
                           TStatus = "test",
                           Name = c.UserName,
                           LogType = a.Status,
                           Time = a.AssignedDate,
                           Details = a.Status + " Employee " + b.FirstName + " " + b.MiddleName + " " + b.LastName,
                       }).ToList();
            var det = lact.Union(rem);
            var comp = det.Union(asl).OrderByDescending(a => a.Time);
            cusmodel.LeadTimeLine = comp.ToList();
            cusmodel.mobmodel = mob;
            return View(cusmodel);
        }
        [HttpPost]
        public JsonResult GetData(long CustomerId)
        {
            Customer cus = db.Customers.Find(CustomerId);
            Contact cont = db.Contacts.Find(cus.Contact);

            return new QuickSoft.Models.LegacyJsonResult { Data = new { data = cus, contact = cont, Id = CustomerId } };
        }
        [HttpPost]
        public ActionResult GetAllRemarks(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //Find Order Column
            var sortColumn = "CreateDate";// Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = "Dec";// Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var v = (from a in db.LeadTaskRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     where a.TaskId == CustomerId && a.Remark != null
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = 1,
                         CreatedDate = a.CreatedDate,
                         sec = a.CreatedDate.Second,
                         mil=a.CreatedDate.Millisecond,
                         empnae = b.UserName,
                         //c.StatusType,
                         a.Remark,
                     }).ToList().Select(o => new
                     {
                         o.id,

                         CreatedDate = o.CreatedDate.AddSeconds(-o.sec).AddMilliseconds(-o.mil),
                         o.empnae,
                         o.Remark

                     });
            

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            //    // Apply search   

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
            }
            recordsTotal = v.Count();
            var data = v.ToList().Distinct();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Leads")]
        public JsonResult ImageDelete(long key)
        {


            bool stat = false;
            string msg;
            LeadDocument proImg = db.LeadDocuments.Find(key);
            db.LeadDocuments.Remove(proImg);
            db.SaveChanges();

            string fullPath = LegacyWeb.MapPath("~/uploads/leaddocuments/" + proImg.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Leads", "LeadDocuments", findip(), proImg.LeadDocumentId, "Lead Document Deleted Successfully");

            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted Project Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }
        private string CustCode(Int64 CNo = 0, string CCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.prefix).FirstOrDefault();

            if (CCode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.number).FirstOrDefault();
                if ((db.Customers.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        CCode = prefix + 1;
                    }
                    else
                    {
                        CCode = prefix + number;
                    }
                }
                else
                {
                    CNo = db.Customers.Max(p => p.EntryNo + 1);
                    CCode = prefix + CNo;
                    if (CodeExist(CCode))
                    {
                        CCode = CustCode(CNo, CCode);
                    }

                }
            }
            else
            {
                CNo = CNo + 1;
                CCode = prefix + CNo;
                if (CodeExist(CCode))
                {
                    CCode = CustCode(CNo, CCode);
                }
            }
            return CCode;
        }
        private string LeadCode(Int64 CNo = 0, string CCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Lead").Select(a => a.prefix).FirstOrDefault();
            if (prefix == null)
                prefix = "L";

            if (CCode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Lead").Select(a => a.number).FirstOrDefault();
                if ((db.Customers.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        CCode = prefix + 1;
                    }
                    else
                    {
                        CCode = prefix + number;
                    }
                }
                else
                {
                    CNo = db.Customers.Max(p => p.EntryNo + 1);
                    CCode = prefix + CNo;
                    if (CodeExist(CCode))
                    {
                        CCode = CustCode(CNo, CCode);
                    }

                }
            }
            else
            {
                CNo = CNo + 1;
                CCode = prefix + CNo;
                if (CodeExist(CCode))
                {
                    CCode = CustCode(CNo, CCode);
                }
            }
            return CCode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.Customers.Any(c => c.CustomerCode == Code);
            if (Exists)
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        private long GetEntryNo()
        {
            Int64 ENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.number).FirstOrDefault();
            if ((db.Customers.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    ENo = 1;
                }
                else
                {
                    ENo = number;
                }
            }
            else
            {
                ENo = db.Customers.Max(p => p.EntryNo + 1);
            }
            return ENo;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public JsonResult SearchContactCreate(string q, string x)
        {
            List<SelectFormatDisabled> serialisedJson;
            string stt = "";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var mobs = (from a in db.Customers
                            join b in db.Contacts on a.Contact equals b.ContactID into tmp
                            from b in tmp.DefaultIfEmpty()
                            join c in db.Mobiles on a.Contact equals c.Contact into mobi
                            from c in mobi.DefaultIfEmpty()
                            where a.Type == CRMCustomerType.Leads && c.MobileNum.Contains(q)


                            select new SelectFormatDisabled
                            {
                                text = c.MobileNum, //each json object will have 
                                id = c.MobileNum,
                                disabled = "true",

                            });


                var phones = (from a in db.Customers
                              join b in db.Contacts on a.Contact equals b.ContactID into tmp
                              from b in tmp.DefaultIfEmpty()
                              where a.Type == CRMCustomerType.Leads && b.Phone.Contains(q)


                              select new SelectFormatDisabled
                              {
                                  text = b.Phone, //each json object will have 
                                  id = b.Phone,
                                  disabled = "true",

                              });


                serialisedJson = phones.Union(mobs).OrderBy(b => b.text).ToList();

            }
            else
            {
                var mobs = (from a in db.Customers
                            join b in db.Contacts on a.Contact equals b.ContactID into tmp
                            from b in tmp.DefaultIfEmpty()
                            join c in db.Mobiles on a.Contact equals c.Contact into mobi
                            from c in mobi.DefaultIfEmpty()
                            where a.Type == CRMCustomerType.Leads

                            select new SelectFormatDisabled

                            {
                                text = c.MobileNum, //each json object will have 
                                id = c.MobileNum,
                                disabled = "true"
                            }).OrderBy(b => b.text).ToList();

                var phones = (from a in db.Customers
                              join b in db.Contacts on a.Contact equals b.ContactID into tmp
                              from b in tmp.DefaultIfEmpty()
                              where a.Type == CRMCustomerType.Leads

                              select new SelectFormatDisabled

                              {
                                  text = b.Phone, //each json object will have 
                                  id = b.Phone,
                                  disabled = "true"
                              });
                serialisedJson = phones.Union(mobs).OrderBy(b => b.text).ToList();
            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatDisabled() { id = stt, text = stt, disabled = "true" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchMobileCreate(string q, string x)
        {
            List<SelectFormatDisabled> serialisedJson;
            string stt = "";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                serialisedJson = (from a in db.Customers
                                  join b in db.Contacts on a.Contact equals b.ContactID into tmp
                                  from b in tmp.DefaultIfEmpty()
                                  join c in db.Mobiles on a.Contact equals c.Contact into mobi
                                  from c in mobi.DefaultIfEmpty()
                                  where a.Type == CRMCustomerType.Leads && c.MobileNum.Contains(q)


                                  select new SelectFormatDisabled
                                  {
                                      text = c.MobileNum, //each json object will have 
                                      id = c.MobileNum,
                                      disabled = "true",

                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Customers
                                  join b in db.Contacts on a.Contact equals b.ContactID into tmp
                                  from b in tmp.DefaultIfEmpty()
                                  join c in db.Mobiles on a.Contact equals c.Contact into mobi
                                  from c in mobi.DefaultIfEmpty()
                                  where a.Type == CRMCustomerType.Leads

                                  select new SelectFormatDisabled

                                  {
                                      text = c.MobileNum, //each json object will have 
                                      id = c.MobileNum,
                                      disabled = "true"
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatDisabled() { id = stt, text = stt, disabled = "true" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchLeadsCreate(string q, string x)
        {
            List<SelectFormat2> serialisedJson;
            string stt = " ";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Customers.Where(p => p.Type == CRMCustomerType.Leads && (p.CustomerName.ToLower().Contains(q.ToLower()) || p.CustomerCode.ToLower().Contains(q.ToLower()) || p.CustomerName.Contains(q) || p.CustomerCode.Contains(q)))
                                  .Select(b => new SelectFormat2
                                  {
                                      text = b.CustomerCode + " - " + b.CustomerName, //each json object will have 
                                      id = b.CustomerName.ToString()

                                  }).Take(10)
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Customers.Where(p => p.Type == CRMCustomerType.Leads).Select(b => new SelectFormat2
                {
                    text = b.CustomerCode + " - " + b.CustomerName,
                    id = b.CustomerName.ToString()

                }).Take(10).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat2() { id = stt, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchLeads(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Customers.Where(p => p.Type == CRMCustomerType.Leads && (p.CustomerName.ToLower().Contains(q.ToLower()) || p.CustomerCode.ToLower().Contains(q.ToLower()) || p.CustomerName.Contains(q) || p.CustomerCode.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.CustomerCode + " - " + b.CustomerName, //each json object will have 
                                      id = b.CustomerID
                                  }).Take(10)
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Customers.Where(p => p.Type == CRMCustomerType.Leads).Select(b => new SelectFormat
                {
                    text = b.CustomerCode + " - " + b.CustomerName, //each json object will have 
                                                                    // text = b.CustomerName, //each json object will have 
                    id = b.CustomerID
                }).Take(10).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //Lead Source
        public JsonResult SearchLeadSource(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.SourceOfLeads.Where(p => p.SrcName.ToLower().Contains(q.ToLower()) || p.SrcName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.SrcName, //each json object will have 
                                      id = b.SourceOfLeadId
                                  }).Take(10)
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.SourceOfLeads.Select(b => new SelectFormat
                {
                    text = b.SrcName, //each json object will have 
                    id = b.SourceOfLeadId
                }).Take(10).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        // search mobile and phone together on index and my leads
        public JsonResult SearchMobileAndPhone(string q, string x)
        {
            List<SelectUserFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                var mobs = (from a in db.Customers

                            join rrr in db.ContactRelation on a.CustomerID equals rrr.RelationID
                            join b in db.Contacts on rrr.ContactID equals b.ContactID into tmp
                            from b in tmp.DefaultIfEmpty()
                            where a.Type == CRMCustomerType.Leads

                            where a.Type == CRMCustomerType.Leads && rrr.RelationID == a.CustomerID && b.Mobile.Contains(q)
                            select new SelectUserFormat
                            {
                                text = b.Mobile, //each json object will have 
                                id = b.Mobile,

                            });






                serialisedJson = mobs.Take(10).OrderBy(b => b.text).ToList();
            }
            else
            {
                var mobs = (from a in db.Customers

                            join rrr in db.ContactRelation on a.CustomerID equals rrr.RelationID
                            join b in db.Contacts on rrr.ContactID equals b.ContactID into tmp
                            from b in tmp.DefaultIfEmpty()
                            where a.Type == CRMCustomerType.Leads



                            select new SelectUserFormat

                            {
                                text = b.Mobile, //each json object will have 
                                id = b.Mobile,
                            });



                serialisedJson = mobs.Take(10).OrderBy(b => b.text).ToList();
            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectUserFormat() { id = stt, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //SearchMobile
        public JsonResult SearchMobile(string q, string x)
        {
            List<SelectUserFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                serialisedJson = (from a in db.Customers
                                  join b in db.Contacts on a.Contact equals b.ContactID into tmp
                                  from b in tmp.DefaultIfEmpty()
                                  join c in db.Mobiles on a.Contact equals c.Contact into mobi
                                  from c in mobi.DefaultIfEmpty()
                                  where a.Type == CRMCustomerType.Leads && c.MobileNum.Contains(q)


                                  select new SelectUserFormat
                                  {
                                      text = c.MobileNum, //each json object will have 
                                      id = c.MobileNum,

                                  }).Take(10)
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Customers
                                  join b in db.Contacts on a.Contact equals b.ContactID into tmp
                                  from b in tmp.DefaultIfEmpty()
                                  join c in db.Mobiles on a.Contact equals c.Contact into mobi
                                  from c in mobi.DefaultIfEmpty()
                                  where a.Type == CRMCustomerType.Leads

                                  select new SelectUserFormat

                                  {
                                      text = c.MobileNum, //each json object will have 
                                      id = c.MobileNum
                                  }).Take(10).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectUserFormat() { id = stt, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        //SearchLocation
        public JsonResult SearchLocation(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Customers.Where(p => p.Type == CRMCustomerType.Leads && (p.Location.ToLower().Contains(q.ToLower()) || p.Location.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Location, //each json object will have 
                                      id = b.CustomerID
                                  }).Take(10)
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Customers.Where(b => b.Type == CRMCustomerType.Leads).Select(b => new SelectFormat
                {
                    text = b.Location, //each json object will have 
                    id = b.CustomerID
                }).Take(10).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchleadStatus(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            //conditions
            bool chkValue1 = false;
            bool chkValue2 = false;
            bool chkValue3 = false;
            bool chkValue4 = false;
            var UserId = User.Identity.GetUserId();
            var chkUserIsEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();

            //the logged user is not an employee
            if (chkUserIsEmp == null || chkUserIsEmp == 0)
            {
                chkValue1 = true;
            }
            else
            {
                var dept = db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                var desgn = db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                if (dept == null && desgn == null)
                {
                    chkValue1 = true;
                }
                else if (dept != null && desgn == null)
                {
                    chkValue2 = true;
                }
                else if (dept == null && desgn != null)
                {
                    chkValue3 = true;
                }
                else
                {
                    chkValue4 = true;
                }
            }

            //list only assigned statuses in team creation
            long[] agnstat = new long[] { };
            var chkteam = db.Teams.Where(a => a.TeamLead == chkUserIsEmp).Select(a => a.TeamId).ToList();
            var members = (from a in db.TeamMembers
                           where a.EmployeeId == chkUserIsEmp
                           select new
                           {
                               a.TeamId
                           }).ToList();

            var allteamid = chkteam.Union(members.Select(a => a.TeamId));

            if (allteamid == null || allteamid.Count() == 0)
            {
                agnstat = null;
            }
            else
            {

                agnstat = db.LeadTaskStatus.Where(a => allteamid.Contains(a.TeamId)).Select(a => a.TaskStatusId).Distinct().ToArray();
            }

            if (chkValue1 == true)//the logged user is not an employee &&  user is an employee with no designation & no department assigned
            {
                var alldata = (from b in db.LeadStatuss
                               select new SelectFormat
                               {
                                   text = b.StatusType, //each json object will have 
                                   id = b.LeadStatusID,
                               }).ToList();

                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from b in alldata
                                      where (b.text.ToLower().Contains(q.ToLower()) || b.text.Contains(q) || b.text.StartsWith(q) || b.text.EndsWith(q)) && (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from b in alldata
                                      where (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }//
            }
            else
            {



                IEnumerable<SelectFormat> full = new List<SelectFormat>();
                //nor dept and nor desgn
                var depdesg = (from b in db.LeadStatuss
                               let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DeptId).ToList()
                               let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DesgId).ToList()
                               where chkdep.Count() == 0 && chkdesg.Count() == 0
                               select new SelectFormat
                               {
                                   text = b.StatusType, //each json object will have 
                                   id = b.LeadStatusID
                               });
                full = full.Union(depdesg);
                if (chkValue2 == true)//dept
                {

                    Int64 dept = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                    var dep = (from b in db.LeadStatuss
                               let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DeptId).ToList()
                               where chkdep.Contains(dept)
                               select new SelectFormat
                               {
                                   text = b.StatusType, //each json object will have 
                                   id = b.LeadStatusID
                               });
                    full = full.Union(dep);

                }
                if (chkValue3 == true)//desgn
                {
                    Int64 desgn = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                    var dessg = (from b in db.LeadStatuss
                                 let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DesgId).ToList()
                                 where chkdesg.Contains(desgn)
                                 select new SelectFormat
                                 {
                                     text = b.StatusType, //each json object will have 
                                     id = b.LeadStatusID
                                 });
                    full = full.Union(dessg);
                }
                if (chkValue4 == true)//dept//desgn
                {
                    Int64 dept = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DepartmentID).FirstOrDefault();
                    Int64 desgn = (long)db.Employees.Where(a => a.EmployeeId == chkUserIsEmp).Select(a => a.DesignationID).FirstOrDefault();
                    var depanddesg = (from b in db.LeadStatuss
                                      let chkdep = db.TaskStatusDepts.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DeptId).ToList()
                                      let chkdesg = db.TaskStatusDesgs.Where(a => a.TaskStatusId == b.LeadStatusID).Select(a => a.DesgId).ToList()
                                      where chkdep.Contains(dept) && chkdesg.Contains(desgn)
                                      select new SelectFormat
                                      {
                                          text = b.StatusType, //each json object will have 
                                          id = b.LeadStatusID
                                      });
                    full = full.Union(depanddesg);
                }

                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from b in full
                                      where (b.text.ToLower().Contains(q.ToLower()) || b.text.Contains(q) || b.text.StartsWith(q) || b.text.EndsWith(q))
                                       && (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from b in full
                                      where (agnstat == null || agnstat.Contains(b.id))
                                      select new SelectFormat
                                      {
                                          text = b.text,
                                          id = b.id
                                      }).OrderBy(b => b.text).ToList();
                }
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        //SearchLeadStatus

        //        serialisedJson = db.LeadStatuss.Where(p => p.StatusType.Contains(q))
        //                          .Select(b => new SelectFormat
        //                              text = b.StatusType, //each json object will have 
        //                              id = b.LeadStatusID
        //                          })
        //        serialisedJson = db.LeadStatuss.Select(b => new SelectFormat
        //            text = b.StatusType, //each json object will have 
        //            id = b.LeadStatusID

        //    }//
        //SearchAssignedTo

        public JsonResult SearchAssignedTo(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(p => p.FirstName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Select(b => new SelectFormat
                                  {
                                      text = b.FirstName, //each json object will have 
                                      id = b.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public string closelead(int? id)
        {

            Customer cus = db.Customers.Find(id);


            cus.OpenClose = 1;
            db.Entry(cus).State = EntityState.Modified;
            db.SaveChanges();

            return "<h2>Lead Closed</h2>";
        }
        public ActionResult AddRemark(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer cus = db.Customers.Find(id);

            if (cus == null)
            {
                return NotFound();
            }
            var lremark = new LeadTaskRemark
            {

                TaskId = cus.CustomerID
            };

            //    ID = r.LeadStatusID,
            //    Name = r.StatusType

            return PartialView(lremark);
        }


        [HttpPost]

        public JsonResult AddRemark(LeadTaskRemark lremark)
        {


          
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);


                var docinfo = new LeadTaskRemark
                {
                    CreatedDate = today,
                    TaskStatusID = 9999,
                    Remark = lremark.Remark,
                    AddedUser = UserId,
                    TaskId = lremark.TaskId,
                    TaskUpdationID = 1,

                };
                db.LeadTaskRemarks.Add(docinfo);
                LeadTaskUpdation TaskUps = new LeadTaskUpdation
                {
                    TaskId = lremark.TaskId,
                    CreatedBy = UserId,
                    CreatedDate = today,

                    Remarks = lremark.Remark,
                    leadstatus = 4,
                };
                db.LeadTaskUpdations.Add(TaskUps);
                db.SaveChanges();
                msg = "Remark added successfully.";
                com.updateleaddate(lremark.TaskId);
                db.Reminders.RemoveRange(db.Reminders.Where(o => o.Note.Contains("24 Hours leads not Updation ")));
                db.SaveChanges();
                db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.Type == "leadStillpending"));
                db.SaveChanges();
                var systime = System.DateTime.Now;
                var rem = (from a in db.Customers
                           join b in db.AssignedTos on a.CustomerID equals b.CustomerID
                           join c in db.LeadStatuss on a.CurrentAction equals c.LeadStatusID into statt
                           from c in statt.DefaultIfEmpty()
                           where EF.Functions.DateDiffHour(systime, a.logtime) < -24
                           && a.Type == CRMCustomerType.Leads

                      && a.OpenClose == 0

                          && b.Status == "Assigned" && b.ChkStatus == 0
                          &&
                              b.EmployeeId != 10192 && b.EmployeeId != 10245
                           select new
                           {
                               b.EmployeeId,
                               a.CustomerID,
                               c.StatusType,
                               timedifference = EF.Functions.DateDiffHour(systime, a.logtime),

                               taskname = a.CustomerCode + "-" + a.CustomerName,

                           }).Distinct();

                if (rem.Count() > 0)
                {
                    var pids = rem.Select(o => new
                    {
                        o.CustomerID,
                        o.StatusType,
                        o.taskname,


                    }).Distinct().ToList();
                    foreach (var pid in pids)
                    {
                        string tasknote = "24 Hours leads not Updation <br> Lead Status : " + pid.StatusType + "<br> Lead name : " + pid.taskname;
                        var remexist = db.Reminders.Any(o => o.Note == tasknote && o.Reference == pid.CustomerID);
                        if (!remexist)
                        {
                            Reminder reminds = new Reminder();
                            reminds.Reference = pid.CustomerID;
                            reminds.Note = tasknote;// "Task Still " +pid.Ref1+" <br> Task Status : " + pid.StatusName + "<br> Task name : " + pid.taskname;

                            var rDate = System.DateTime.Now.Date;
                            //seleted date added,for fullcalender



                            reminds.RDate = System.DateTime.Now;
                            reminds.Type = "/leads/Details/" + pid.CustomerID;
                            reminds.RStatus = "Close";
                            reminds.RequestBy = User.Identity.GetUserId();

                            reminds.CreatedBy = reminds.RequestBy;
                            reminds.Status = Status.active;
                            reminds.CreatedDate = System.DateTime.Now;
                            db.Reminders.Add(reminds);
                            db.SaveChanges();
                            long idd = reminds.ReminderId;
                            var asseimp = rem.Where(o => o.CustomerID == pid.CustomerID).Select(o => o.EmployeeId).Distinct().ToList().ToArray();
                            var myemps = asseimp.Distinct().ToList().ToArray();
                            foreach (var arr in myemps)
                            {

                                var exists = db.ReminderAssigneds.Any(o => o.EntryId == pid.CustomerID && o.Type == "leadStillpending" && o.EmployeeId == arr);



                                if (!exists)
                                {
                                    ReminderAssigned remAs = new ReminderAssigned();

                                    remAs.ReminderId = idd;
                                    remAs.EntryId = pid.CustomerID;
                                    remAs.Type = "leadStillpending";
                                    remAs.EmployeeId = arr;
                                    db.ReminderAssigneds.Add(remAs);
                                    db.SaveChanges();

                                }
                            }
                        }

                    }
                }

                stat = true;
                com.addlog(LogTypes.Created, UserId, "Leads", "LeadRemarks", findip(), Id, "Remark Added Successfully");
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
        


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [HttpGet]
        public JsonResult GetMobile(long CnId)
        {
            var ConD = (from a in db.Mobiles
                        join b in db.Customers on a.Contact equals b.Contact
                        where b.CustomerID == CnId
                        select new
                        {
                            Mob = a.MobileNum,
                            Name = a.Name
                        }).ToList();
            return Json(ConD);
        }

        public JsonResult GetCheckItems(long CheckID)
        {
            //            where c.Stage == CheckID
            //                Id = a.Id,
            //                Name = a.ListName,
            //                Note = a.AddNote,


            var ConD = (from a in db.LeadChecklistItems
                        join c in db.LeadChecklists on a.Checklist equals c.ChecklistId into chk
                        from c in chk.DefaultIfEmpty()
                        where c.Stage == CheckID
                        select new
                        {
                            Id = a.Checklist,
                            Name = a.ListName,
                            Note = a.AddNote,
                        }).ToList();
            return Json(ConD);
        }

        public ActionResult LeadModal(long id)
        {
            var isfireman = db.companys.Any(o => o.CPName.Contains("FIREMAN SAFETY SERVICES SOLE PROPRIETORSHIP"));
            ViewBag.isfiremanbag =  isfireman;
           
            Customer customer = db.Customers.Find(id);
            
            var UserID = User.Identity.GetUserId();




            var tremark = new LeadTaskRemarkviewmodal
            {
                TaskStatusID = customer.CurrentAction,
                
                TaskId = customer.CustomerID,
                CreatedDate = System.DateTime.Now,
                EndTime = System.DateTime.Now,
                FinishTime = System.DateTime.Now.AddHours(1),
                EndDate = System.DateTime.Now,
                AssignedTo = db.AssignedTos.Where(x => x.CustomerID == id && x.AssignBy == UserID && x.Status == "Assigned" && x.ChkStatus == (int)Status.active).Select(a => a.EmployeeId).ToList().ToArray() ?? null
                
            };
            if(isfireman)
            {
                tremark.Ref1 = customer.Ref1;
                tremark.Ref2 = customer.Ref2;
                tremark.Ref3 = customer.Ref3;
                tremark.Ref4 = customer.Ref4;
                tremark.Ref5 = customer.Ref5;



            }

            var Location = db.LocationNames.Select(s => new
            {
                Id = s.LocationId,
                Name = s.Location,
            }).ToList();
            ViewBag.Location = QkSelect.List(Location, "Id", "Name");
            var use = db.Employees

                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.Employee = new MultiSelectList(use, "ID", "Name", tremark.AssignedTo);

            var status = db.LeadStatuss
                  .Select(s => new KeyValue
                  {
                      ID = s.LeadStatusID,
                      Name = s.StatusType
                  }).ToList();
            status.Add(new KeyValue { ID = (long)LeadApprovedPermanantValue.LeadApproved, Name = LeadApprovedPermanantValue.LeadApproved.ToString() });

            ViewBag.CurrentAction = QkSelect.List(status, "Id", "Name");
            ViewBag.NextAction = QkSelect.List(status, "Id", "Name");


            ViewBag.LeadCheckedlist = new MultiSelectList(db.LeadChecklistItems, "Id", "ListName");






            tremark.FieldMap = db.FieldMappings.Where(a => a.Section == "Leads" && a.Status == Status.active).ToList();

            
            return PartialView(tremark);
        }

        public string GetStatusName(long? Id)
        {
            string name = db.LeadStatuss.Where(a => a.LeadStatusID == Id).Select(a => a.StatusType).FirstOrDefault();
            return name;
        }

        [HttpPost]
        public JsonResult LeadModal(LeadRemarklistViewModel tremark)
        {
            string msg = "";
            bool stat = false;


           





            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var today = System.DateTime.Now;
                Int64 proId = tremark.TaskId;
                string leadname = db.Customers.Where(o => o.CustomerID == tremark.TaskId).Select(o => o.CustomerName).FirstOrDefault();
                db.Reminderss.Add(new Reminderss
                {
                    CreatedBy = User.Identity.GetUserId(),
                    Note = leadname + " " + "LEAD",
                    RDate =tremark.CreatedDate,
                    CreatedDate = System.DateTime.Now,
                    actionurl = "#",
                    RequestBy = User.Identity.GetUserId(),
                    Status = 0,
                    RStatus = "Open",



                });
                db.SaveChanges();

                var tskstat = db.Customers.Where(a => a.CustomerID == proId).Select(a => a.CurrentAction).FirstOrDefault();
                var oldtskstat = GetStatusName(tskstat);
                var newtskstat = GetStatusName(tremark.TaskUpdationID);
                DateTime? eDate = null;
                DateTime? etimes = null;
                DateTime? fDate = null;
                DateTime? ftimes = null;
                if (tremark.CreatedDate != null)
                {
                    eDate =tremark.CreatedDate;
                    TimeSpan? etime = null;
                    if (tremark.EndTime != null)
                    {
                        etime = ((DateTime)tremark.EndTime).TimeOfDay;
                    }
                    etimes = eDate + etime;
                }
                if (tremark.EndDate != null)
                {
                    fDate = eDate;
                    TimeSpan? ftime = null;
                    if (tremark.FinishTime != null)
                    {
                        ftime = ((DateTime)tremark.FinishTime).TimeOfDay;
                    }
                    ftimes = fDate + ftime;
                }
                LeadTaskUpdation TaskUps = new LeadTaskUpdation
                {
                    TaskId = proId,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    nextdate = tremark.CreatedDate,
                    nexttime = etimes,
                    finishdate = tremark.EndDate,
                    finishtime=ftimes,
                    Remarks = tremark.Remark,
                  
                    leadstatus = (long)tremark.TaskUpdationID,
                };
                DateTime? nextleaddate = tremark.CreatedDate;
                db.LeadTaskUpdations.Add(TaskUps);
                db.SaveChanges();
                if (nextleaddate != null)
                {
                    db.Customers.Where(o => o.CustomerID == proId).ToList().ForEach(o => { o.EndTime = etimes; o.EndDate = nextleaddate; });
                    db.SaveChanges();
                }
                var id = TaskUps.TaskUpdationID;



                Int64 TaskUpdId = TaskUps.TaskUpdationID;
                var docinfo = new LeadTaskRemark
                {
                    CreatedDate = today,
                    TaskUpdationID = id,
                    Remark = tremark.Remark,
                    AddedUser = UserId,
                    TaskId = tremark.TaskId


                };
                db.LeadTaskRemarks.Add(docinfo);
                db.SaveChanges();
                var TaskRemarkId = docinfo.TaskRemarkId;



                if (tremark.bstmodel != null)
                {
                    foreach (var arr in tremark.bstmodel)
                    {
                        LeadRemarkChecklist remlist = new LeadRemarkChecklist();
                        remlist.Checklistitemid = arr.Id;
                        remlist.Note = arr.Note;
                        remlist.Check = (arr.Check == "on") ? true : false;
                        remlist.Remark = TaskRemarkId;
                        db.LeadRemarkChecklists.Add(remlist);
                        db.SaveChanges();
                    }
                }


                // fileupload
                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {
                    string path = LegacyWeb.MapPath("~/uploads/leadtaskdocuments/");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {

                            var fileCount = db.LeadTaskImages.Select(a => a.TaskImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                            var fileName = Path.GetFileName(file.FileName);

                            String extension = Path.GetExtension(fileName);


                            String newName = fileCount + extension;
                            string newFName = fileCount + extension;
                            var FStatus = Status.active;
                            var thumbName = "";
                            var resizeName = "";
                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                            {
                                thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), thumbName);

                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), resizeName);
                                newFName = "resize_" + newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";

                            }
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), newName);
                            file.SaveAs(newName);


                            var taskimg = new LeadTaskImage
                            {
                                TaskId = proId,
                                TaskUpdationID = TaskUpdId,

                                FileName = newFName,//Path.GetFileName(file.FileName),
                                Status = FStatus,
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                TaskRemarkId = TaskRemarkId,
                                CreatedBy = UserId,
                            };
                            db.LeadTaskImages.Add(taskimg);
                            db.SaveChanges();



                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                            {
                                Image img = Image.FromFile(newName);
                                int imgHeight = 100;
                                int imgWidth = 100;
                                if (img.Width < img.Height)
                                {
                                    //portrait image  
                                    imgHeight = 100;
                                    var imgRatio = (float)imgHeight / (float)img.Height;
                                    imgWidth = Convert.ToInt32(img.Height * imgRatio);
                                }
                                else if (img.Height < img.Width)
                                {
                                    //landscape image  
                                    imgWidth = 100;
                                    var imgRatio = (float)imgWidth / (float)img.Width;
                                    imgHeight = Convert.ToInt32(img.Height * imgRatio);
                                }
                                Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                                thumb.Save(thumbName);

                                Image lgimg = Image.FromFile(newName);
                                if (lgimg.Width > 1800 || lgimg.Height > 1800)
                                {
                                    Image imgs = Image.FromFile(newName);
                                    System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/leadtaskdocuments/"), resizeName);
                                    lgimg.Save(resizeName);
                                }

                            }
                        }
                    }
                }

                //shiyas

                if (tremark.TaskUpdationID != null)
                {
                    Customer customer = db.Customers.Find(proId);
                    customer.CurrentAction = (int)tremark.TaskUpdationID;
                    customer.NextAction = (int)tremark.TaskUpdationID;

                    var TaskStatus = tremark.TaskUpdationID;

                    var pflow = db.LeadProcessFlows.Where(a => a.LeadStatus == tremark.TaskUpdationID).FirstOrDefault();
                    List<long> astypes = null;
                    if (pflow != null)
                    {
                        astypes = db.LeadProcessFlowAssignTypes.Where(a => a.LeadProcessFlowId == pflow.LeadProcessFlowId).Select(a => a.TeamId).ToList();
                    }

                    if (pflow != null)
                    {
                        if (pflow.RemoveUpdateUser == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();
                            if (UserEmp != 0)
                            {

                                var tskasgn = db.AssignedTos.Where(a => a.CustomerID == tremark.TaskId && a.Status == "Assigned" && a.EmployeeId == UserEmp && a.ChkStatus == (int)Status.active).ToList();
                                if (tskasgn != null)
                                {
                                    foreach (var arr in tskasgn)
                                    {
                                        AssignedTo tskassr = db.AssignedTos.Find(arr.AssignedToId);
                                        tskassr.Status = "Removed";
                                        tskassr.ChkStatus = (int)Status.inactive;
                                        db.Entry(tskassr).State = EntityState.Modified;
                                        db.SaveChanges();







                                    }

                                }
                            }
                        }
                        if (pflow.RemoveUpdateUserTeams == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();


                            var team = db.Teams.Where(a => a.TeamLead == UserEmp).Select(a => a.TeamId).Distinct().ToList();
                            var teams = db.TeamMembers.Where(a => a.EmployeeId == UserEmp).Select(a => a.TeamId).Distinct().ToList();
                            var fulldata = team.Union(teams);
                            ////team lead as user
                            var tskteam = (from a in db.Teams
                                           where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || a.TeamLead != UserEmp)
                                           //where a.TeamLead == UserEmp
                                           select new
                                           {
                                               EmployeeId =(long?) a.TeamLead
                                           }).ToArray();
                            ////team mebers 
                            var tskmem = (from a in db.Teams
                                          join b in db.TeamMembers on a.TeamId equals b.TeamId into tea
                                          from b in tea.DefaultIfEmpty()
                                          where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || b.EmployeeId != UserEmp)
                                          select new
                                          {
                                              EmployeeId=(long?)b.EmployeeId
                                          }).ToArray();




                            var emp = tskteam.Union(tskmem).Select(a => a.EmployeeId).ToList();
                            var UserEmps = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();
                            var tskasgn = db.AssignedTos.Where(a => a.CustomerID == tremark.TaskId && a.Status == "Assigned" && a.ChkStatus == (int)Status.active&&emp.Contains(a.EmployeeId)).ToList();

                            AssignedTo tskass = new AssignedTo();
                            if (tskasgn != null)
                            {
                                foreach (var arr in tskasgn)
                                {
                                    AssignedTo tskassr = db.AssignedTos.Find(arr.AssignedToId);
                                    tskassr.ChkStatus = (int)Status.inactive;

                                    tskassr.Status = "Removed";
                                    tskassr.ChkStatus = (int)Status.inactive;

                                    db.Entry(tskassr).State = EntityState.Modified;
                                    db.SaveChanges();

                                }

                            }

                        }

                        // process flow members assigned
                        var chkassgn = db.AssignedTos.Where(a => a.CustomerID == proId && a.Status == "Assigned" && a.ChkStatus == 0).Select(a => a.EmployeeId).ToList();

                        var pfmembers = db.LeadProcessFlowAssignUsers.Where(a => a.LeadProcessFlowId == pflow.LeadProcessFlowId).Select(a => a.EmployeeId).ToList();










                        IList<AssignedTo> Assigned = new List<AssignedTo>();
                        IList<AssignedToLog> AssignLog = new List<AssignedToLog>();
                        foreach (var arr in pfmembers)
                        {

                            Assigned.Add(new AssignedTo()
                            {
                                CustomerID = proId,
                                EmployeeId = arr,
                                Status = "Assigned",
                                AssignBy = UserId,
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100),
                                ChkStatus = (int)Status.active,
                                approve = pflow.approvalreq,
                            });
                          //  com.remideradd("http://uk.ath.cx:1091/Leads/MyLeads", arr, UserId, "Lead Assigned", proId);
                            var CustomerName = db.Customers.Find(proId).CustomerName;
                            var empnames = db.Employees.Where(o => o.UserId == UserId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                            var leadnames = CustomerName + " Lead Assignd by " + empnames;
                            com.remideradd("/Leads/MyLeads", arr, UserId, leadnames, proId);
                            var emuserid = db.Employees.Where(o => o.EmployeeId == arr).Select(o => o.UserId).FirstOrDefault();
                            db.Reminderss.Add(new Reminderss
                            {
                                CreatedBy = emuserid,
                                Note = leadname + " " + "LEAD",
                                RDate = tremark.CreatedDate,
                                CreatedDate = System.DateTime.Now,
                                actionurl = "#",
                                RequestBy = User.Identity.GetUserId(),
                                Status = 0,
                                RStatus = "Open",




                            });
                            db.SaveChanges();

                            AssignLog.Add(new AssignedToLog()
                            {
                                CustomerID = proId,
                                EmployeeId = arr,
                                Status = "Assigned",
                                AssignedDate = System.DateTime.Now,
                                AddedUser = UserId,
                            });

                        }
                        if (Assigned != null)
                        {
                            db.AssignedTos.AddRange(Assigned);
                            db.SaveChanges();
                        }
                        if (AssignLog != null)
                        {
                            db.AssignedToLogs.AddRange(AssignLog);
                            db.SaveChanges();
                        }
                    }



















                }




                //shiyas end




























                if (tremark.TaskUpdationID != null)
                {
                    Customer customer = db.Customers.Find(proId);
                    customer.CurrentAction = (int)tremark.TaskUpdationID;
                    customer.NextAction = (int)tremark.TaskUpdationID;

                    //additional assign from form
                    if (tremark.AssignedTo != null && tremark.AssignedTo.Count() > 0)
                    {
                        foreach (var assito in tremark.AssignedTo)
                        {
                            var x = db.AssignedTos.Where(a => a.EmployeeId == assito && a.CustomerID == proId);

                            if (x != null && x.Count() > 0)
                            {

                            }
                            else
                            {
                                IList<AssignedTo> Assigned = new List<AssignedTo>();
                                var pflow2 = db.LeadProcessFlows.Where(a => a.LeadStatus == tremark.TaskUpdationID).FirstOrDefault();

                                Assigned.Add(new AssignedTo()
                                {


                                    CustomerID = proId,

                                    Status = "Assigned",
                                    AssignBy = UserId,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                    ChkStatus = (int)Status.active,
                                    approve = (pflow2 == null) ? false : pflow2.approvalreq,
                                    EmployeeId = assito,

                                });
                                var CustomerName = db.Customers.Find(proId).CustomerName;
                                var empnames = db.Employees.Where(o => o.UserId == UserId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                                var leadnames = CustomerName + " Lead Assignd by " + empnames;
                                com.remideradd("/Leads/MyLeads", assito, UserId, leadnames, proId);

                                //com.remideradd("http://uk.ath.cx:1091/Leads/MyLeads", assito, UserId, "Lead Assigned", proId);


                                if (Assigned != null)
                                {
                                    db.AssignedTos.AddRange(Assigned);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    var pflow = db.LeadProcessFlows.Where(a => a.LeadStatus == tremark.TaskUpdationID).FirstOrDefault();
                    List<long> astypes = null;
                    if (pflow != null)
                    {
                        astypes = db.LeadProcessFlowAssignTypes.Where(a => a.LeadProcessFlowId == pflow.LeadProcessFlowId).Select(a => a.TeamId).ToList();
                    }


                    if (pflow != null)
                    {
                        //Create Approval
                        var UserEmps = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();
                        if (UserEmps != 0)
                        {
                            var tskasg = db.AssignedTos.Where(a => a.CustomerID == tremark.TaskId && a.Status == "Assigned" && a.EmployeeId == UserEmps && a.ChkStatus == (int)Status.active).ToList();

                            //tskasg != null)
                            if (1 == 1)
                            {
                                if (1 == 1)
                                {




                                    if (pflow.approvalreq == true)
                                    {
                                        var approvedmasters = db.LeadApprovals.Where(o => o.LeadProcessFlowId == pflow.LeadProcessFlowId).Select(o => new { apvemi = o.LeadEmployeeId }).Distinct().ToList();
                                        var processflowEmployees = db.LeadProcessFlowAssignUsers.Where(x => x.LeadProcessFlowId == pflow.LeadProcessFlowId).ToList();
                                        if (approvedmasters != null && approvedmasters.Count() > 0)
                                        {
                                            if (approvedmasters.Count() > 0)
                                            {

                                                foreach (var apempid in approvedmasters)
                                                {

                                                    long empID = processflowEmployees.Select(x => x.EmployeeId).FirstOrDefault();
                                                    LeadApprovedEmployees leadApprovedEmployees = new LeadApprovedEmployees();
                                                    leadApprovedEmployees.LeadID = tremark.TaskId;
                                                    leadApprovedEmployees.EmployeeID = apempid.apvemi;
                                                    leadApprovedEmployees.CreatedUser = UserId;
                                                    leadApprovedEmployees.CreatedDate = System.DateTime.Now;
                                                    leadApprovedEmployees.Status = LeadApprovalStatus.Approved.ToString();
                                                    db.LeadApprovedEmployees.Add(leadApprovedEmployees);
                                                    db.SaveChanges();
                                                }
                                            }
                                            else
                                            {
                                            }

                                            var Approved = db.LeadApprovedEmployees.Where(x => x.LeadID == tremark.TaskId).ToList();
                                            if (Approved != null && Approved.Count() == processflowEmployees.Count())
                                            {
                                                foreach (var item in processflowEmployees)
                                                {
                                                    LeadApprovedEmployees leadApprovedEmployees = db.LeadApprovedEmployees.Where(x => x.EmployeeID == item.EmployeeId && x.LeadID == tremark.TaskId).FirstOrDefault();
                                                    leadApprovedEmployees.Status = LeadApprovalStatus.Approved.ToString();
                                                    db.Entry(leadApprovedEmployees).State = EntityState.Modified;
                                                }
                                                db.SaveChanges();
                                            }
                                        }
                                    }

                                }
                            }
                        }

                        //remove user
                        if (pflow.RemoveUpdateUser == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();
                            if (UserEmp != 0)
                            {

                                var tskasgn = db.AssignedTos.Where(a => a.CustomerID == tremark.TaskId && a.Status == "Assigned" && a.EmployeeId == UserEmp && a.ChkStatus == (int)Status.active).ToList();
                                if (tskasgn != null)
                                {
                                    foreach (var arr in tskasgn)
                                    {
                                        AssignedTo tskassr = db.AssignedTos.Find(arr.AssignedToId);
                                        tskassr.Status = "Removed";
                                        tskassr.ChkStatus = (int)Status.inactive;
                                        db.Entry(tskassr).State = EntityState.Modified;
                                        db.SaveChanges();






                                    }

                                }
                            
                            
                            }
                        }


                        //remove entire team
                        if (pflow.RemoveUpdateUserTeams == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();


                            var team = db.Teams.Where(a => a.TeamLead == UserEmp).Select(a => a.TeamId).Distinct().ToList();
                            var teams = db.TeamMembers.Where(a => a.EmployeeId == UserEmp).Select(a => a.TeamId).Distinct().ToList();
                            var fulldata = team.Union(teams);
                            ////team lead as user
                            var tskteam = (from a in db.Teams
                                           where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || a.TeamLead != UserEmp)
                                           //where a.TeamLead == UserEmp
                                           select new
                                           {
                                               EmployeeId = (long?)a.TeamLead
                                           }).ToArray();
                            ////team mebers 
                            var tskmem = (from a in db.Teams
                                          join b in db.TeamMembers on a.TeamId equals b.TeamId into tea
                                          from b in tea.DefaultIfEmpty()
                                          where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || b.EmployeeId != UserEmp)
                                          select new
                                          {
                                              EmployeeId=(long?) b.EmployeeId
                                          }).ToArray();




                            var emp = tskteam.Union(tskmem).Select(a => a.EmployeeId).ToList();
                            var tskasgn = db.TaskAssigneds.Where(a => a.ProTaskId == tremark.TaskId && emp.Contains(a.EmployeeId) && a.Status == "Assigned" && a.chkStatus == Status.active).ToList();

                            AssignedTo tskass = new AssignedTo();
                            if (tskasgn != null)
                            {
                                foreach (var arr in tskasgn)
                                {
                                    AssignedTo tskassr = db.AssignedTos.Find(arr.TaskAssignedId);
                                    tskassr.ChkStatus = (int)Status.inactive;
                                    db.Entry(tskassr).State = EntityState.Modified;
                                    db.SaveChanges();

                                    tskass.CustomerID = tremark.TaskId;
                                    tskass.EmployeeId = arr.EmployeeId;
                                    tskass.Status = "Removed";
                                    tskass.AssignBy = UserId;
                                    tskass.CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100);
                                    db.Entry(tskass).State = EntityState.Modified;
                                    db.SaveChanges();
                                }

                            }

                        }

                    }

                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();
                    if (pflow != null)
                    {
                        if (pflow.movetofieldservice == true && pflow.approvalreq == false)
                        {








                            // fileupload































                            //move to field service
                            UserId = User.Identity.GetUserId();
                            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                            today = Convert.ToDateTime(System.DateTime.Now);

                            DateTime? sDate = null;

                            DateTime? stimes = null;
                            DateTime? starttime = null;

                            sDate = System.DateTime.Now;
                            TimeSpan? stime = null;

                            stime = ((DateTime)System.DateTime.Now).TimeOfDay;

                            stimes = sDate + stime;

                            eDate = System.DateTime.Now;
                            TimeSpan? etime = null;

                            etime = ((DateTime)System.DateTime.Now).TimeOfDay;

                            etimes = eDate + etime;


                            var v = db.ProTasks.Where(o => o.CustomerID == proId).FirstOrDefault();
                            if (v == null)
                            {
                                ProTask task = new ProTask();

                                task.TaskNo = GetProNo();
                                task.TaskCode = InvoiceNo();
                                task.TaskName = customer.CustomerName;
                                task.ProjectId = 1;
                                task.CustomerID = customer.CustomerID;
                                task.TaskType = customer.LeadType;
                                task.StartDate = sDate;
                                task.StartTime = stimes;
                                task.EndDate = eDate;
                                task.EndTime = etimes;
                                task.CreatedDate = today;
                                task.CreatedBy = User.Identity.GetUserId();
                                task.Status = Status.active;
                                task.Branch = BranchID;


                                if (db.LocationNames.Find(customer.LocationID) != null)
                                {
                                    var locationName = db.LocationNames.Find(customer.LocationID);
                                }

                                var lpw = db.LeadProcessFlows.Where(o => o.LeadStatus == tremark.TaskUpdationID).Select(o => o.taskid).FirstOrDefault();

                                task.TaskStatus = lpw;





                                Int64 TaskRemarkId2 = docinfo.TaskRemarkId;






                                task.SalesPerson = 0;
                                task.logtime = System.DateTime.Now;

                                db.ProTasks.Add(task);
                                db.SaveChanges();
                                proId = task.ProTaskId;





















                            }
                            else
                            {
                                var lpw = db.LeadProcessFlows.Where(o => o.LeadStatus == tremark.TaskUpdationID).Select(o => o.taskid).FirstOrDefault();


                                v.TaskStatus = lpw;
                                v.CreatedBy = User.Identity.GetUserId();
                                db.Entry(v).State = EntityState.Modified;
                                db.SaveChanges();
                                proId = v.ProTaskId;
                            }

                            db.ContactRelation.RemoveRange(db.ContactRelation.Where(o => o.RelationID == proId && o.RelationType == 11));
                            db.SaveChanges();
                            var vvv = (
                                from r in db.ContactRelation
                                join c in db.Contacts on r.ContactID equals c.ContactID
                                where r.RelationID == tremark.TaskId
                                select c
                                ).ToList();

                            if (vvv != null && vvv.Count > 0)
                            {
                                foreach (var item in vvv)
                                {
                                    var contact = new Contact
                                    {

                                        Address = item.Name,


                                        Country = item.Country,

                                        FirstName = item.FirstName,
                                        LastName = item.LastName,
                                        Name = item.FirstName + " " + item.LastName,

                                        TypeOfContact = item.TypeOfContact,
                                        Mobile = item.Mobile,
                                        Phone = item.Phone,

                                        EmailId = item.EmailId,

                                        ContactPerson = item.Name,
                                        Website = item.Website,
                                        Group = 2,
                                        Status = Status.active,
                                        CountryID = item.CountryID,
                                        ContactTypeID = item.ContactTypeID

                                    };
                                    db.Contacts.Add(contact);
                                    db.SaveChanges();
                                    var contactId = contact.ContactID;


                                    ContactRelation Relation = new ContactRelation();
                                    Relation.ContactID = contactId;
                                    Relation.RelationType = 11;//for task
                                    Relation.RelationID = proId;
                                    db.ContactRelation.Add(Relation);
                                    db.SaveChanges();
                                }
                            }


                            ProTaskUpdation TaskUp = new ProTaskUpdation
                            {
                                ProTaskId = proId,
                                CreatedBy = User.Identity.GetUserId(),
                                CreatedDate = today,
                            };
                            db.ProTaskUpdations.Add(TaskUp);
                            db.SaveChanges();
                            TaskUpdId = TaskUp.TaskUpdationID;

                            IFormFileCollection filess = Request.Form.Files;
                            if (filess.Count > 0)
                            {
                                string path = LegacyWeb.MapPath("~/uploads/protaskdocuments/");
                                if (!Directory.Exists(path))
                                    Directory.CreateDirectory(path);

                                for (int i = 0; i < filess.Count; i++)
                                {
                                    IFormFile file = files[i];
                                    if (file.Length > 0)
                                    {

                                        var fileCount = db.TaskImages.Select(a => a.TaskImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                                        var fileName = Path.GetFileName(file.FileName);

                                        String extension = Path.GetExtension(fileName);


                                        String newName = fileCount + extension;
                                        string newFName = fileCount + extension;
                                        var FStatus = Status.active;
                                        var thumbName = "";
                                        var resizeName = "";
                                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                        {
                                            thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), thumbName);

                                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                            newFName = "resize_" + newFName;
                                            FStatus = Status.inactive;
                                        }
                                        else
                                        {
                                            var commonfilename = "Docs-Thump.png";

                                        }
                                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), newName);
                                        file.SaveAs(newName);
                                        ProTaskUpdation TaskUpss = new ProTaskUpdation
                                        {
                                            ProTaskId = proId,
                                            CreatedBy = UserId,
                                            CreatedDate = today,
                                            Remarks = "file from leads",
                                        };
                                        db.ProTaskUpdations.Add(TaskUpss);
                                        db.SaveChanges();


                                        Int64 TaskUpdIds = TaskUps.TaskUpdationID;

                                        var taskimg = new TaskImage
                                        {
                                            ProTaskId = proId,
                                            TaskUpdationID = TaskUpdIds,

                                            FileName = newFName,//Path.GetFileName(file.FileName),
                                            Status = FStatus,
                                            CreatedDate = Convert.ToDateTime(System.DateTime.Now),

                                            CreatedBy = UserId,
                                        };
                                        db.TaskImages.Add(taskimg);
                                        db.SaveChanges();


                                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                        {
                                            Image img = Image.FromFile(newName);
                                            int imgHeight = 100;
                                            int imgWidth = 100;
                                            if (img.Width < img.Height)
                                            {
                                                //portrait image  
                                                imgHeight = 100;
                                                var imgRatio = (float)imgHeight / (float)img.Height;
                                                imgWidth = Convert.ToInt32(img.Height * imgRatio);
                                            }
                                            else if (img.Height < img.Width)
                                            {
                                                //landscape image  
                                                imgWidth = 100;
                                                var imgRatio = (float)imgWidth / (float)img.Width;
                                                imgHeight = Convert.ToInt32(img.Height * imgRatio);
                                            }
                                            Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                                            thumb.Save(thumbName);

                                            Image lgimg = Image.FromFile(newName);
                                            if (lgimg.Width > 1800 || lgimg.Height > 1800)
                                            {
                                                Image imgs = Image.FromFile(newName);
                                                System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                                thumbs.Save(resizeName);
                                            }
                                            else
                                            {
                                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/protaskdocuments/"), resizeName);
                                                lgimg.Save(resizeName);
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                msg = "Lead Remark added successfully.";
                stat = true;
                com.addlog(LogTypes.Created, UserId, "Leads", "LeadRemarks", findip(), id, "lead Status Updated");
                com.updateleaddate(proId);
                db.Reminders.RemoveRange(db.Reminders.Where(o => o.Note.Contains("24 Hours leads not Updation ")));
                db.SaveChanges();
                db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.Type == "leadStillpending"));
                db.SaveChanges();
                var systime = System.DateTime.Now;
                var rem = (from a in db.Customers
                           join b in db.AssignedTos on a.CustomerID equals b.CustomerID
                           join c in db.LeadStatuss on a.CurrentAction equals c.LeadStatusID into statt
                           from c in statt.DefaultIfEmpty()
                           where EF.Functions.DateDiffHour(systime, a.logtime) < -24
                           && a.Type == CRMCustomerType.Leads
&&
                              b.EmployeeId != 10192 && b.EmployeeId != 10245
                      && a.OpenClose == 0
                    
                          && b.Status == "Assigned" && b.ChkStatus == 0

                           select new
                           {
                               b.EmployeeId,
                               a.CustomerID,
                               c.StatusType,
                               timedifference = EF.Functions.DateDiffHour(systime, a.logtime),

                               taskname = a.CustomerCode + "-" + a.CustomerName,

                           }).Distinct();

                if (rem.Count() > 0)
                {
                    var pids = rem.Select(o => new
                    {
                        o.CustomerID,
                        o.StatusType,
                        o.taskname,


                    }).Distinct().ToList();
                    foreach (var pid in pids)
                    {
                        string tasknote = "24 Hours leads not Updation <br> Lead Status : " + pid.StatusType + "<br> Lead name : " + pid.taskname;
                        var remexist = db.Reminders.Any(o => o.Note == tasknote && o.Reference == pid.CustomerID);
                        if (!remexist)
                        {
                            Reminder reminds = new Reminder();
                            reminds.Reference = pid.CustomerID;
                            reminds.Note = tasknote;// "Task Still " +pid.Ref1+" <br> Task Status : " + pid.StatusName + "<br> Task name : " + pid.taskname;

                            var rDate = System.DateTime.Now.Date;
                            //seleted date added,for fullcalender



                            reminds.RDate = System.DateTime.Now;
                            reminds.Type = "/leads/Details/" + pid.CustomerID;
                            reminds.RStatus = "Close";
                            reminds.RequestBy = User.Identity.GetUserId();

                            reminds.CreatedBy = reminds.RequestBy;
                            reminds.Status = Status.active;
                            reminds.CreatedDate = System.DateTime.Now;
                            db.Reminders.Add(reminds);
                            db.SaveChanges();
                            long idd = reminds.ReminderId;
                            var asseimp = rem.Where(o => o.CustomerID == pid.CustomerID).Select(o => o.EmployeeId).Distinct().ToList().ToArray();
                            var myemps = asseimp.Distinct().ToList().ToArray();
                            foreach (var arr in myemps)
                            {

                                var exists = db.ReminderAssigneds.Any(o => o.EntryId == pid.CustomerID && o.Type == "leadStillpending" && o.EmployeeId == arr);



                                if (!exists)
                                {
                                    ReminderAssigned remAs = new ReminderAssigned();

                                    remAs.ReminderId = idd;
                                    remAs.EntryId = pid.CustomerID;
                                    remAs.Type = "leadStillpending";
                                    remAs.EmployeeId = arr;
                                    db.ReminderAssigneds.Add(remAs);
                                    db.SaveChanges();

                                }
                            }
                        }

                    }
                }
                var isfireman = db.companys.Any(o => o.CPName.Contains("FIREMAN SAFETY SERVICES SOLE PROPRIETORSHIP"));
              
                if(isfireman)
                {
                    var cus = db.Customers.Find(tremark.TaskId);
                    if(tremark.Ref1 !="" && tremark.Ref1!=null)
                    {
                        cus.Ref1 = tremark.Ref1;
                    }
                    if (tremark.Ref2 != "" && tremark.Ref2 != null)
                    {
                        cus.Ref2 = tremark.Ref2;
                    }
                    if (tremark.Ref3 != "" && tremark.Ref3 != null)
                    {
                        cus.Ref3 = tremark.Ref3;
                    }
                    if (tremark.Ref4 != "" && tremark.Ref4 != null)
                    {
                        cus.Ref4 = tremark.Ref4;
                    }
                    if (tremark.Ref5 != "" && tremark.Ref5 != null)
                    {
                        cus.Ref5 = tremark.Ref5;
                    }
                    db.Entry(cus).State = EntityState.Modified;
                    db.SaveChanges();




                }

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        public ActionResult LeadApprovalModal()
        {
            return View();
        }

        public ActionResult GetLeadApprovedDetails(string SizeName)
        {
            var UserID = User.Identity.GetUserId();

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
            var uEdit = User.IsInRole("Edit Item Size");
            var uDelete = User.IsInRole("Delete Item Size");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.LeadApprovedEmployees
                     join b in db.Customers on a.LeadID equals b.CustomerID
                     join c in db.Employees on a.EmployeeID equals c.EmployeeId
                     where (c.UserId == UserID && a.Status == LeadApprovalStatus.Approved.ToString())
                     select new
                     {
                         LeadName = b.CustomerName,
                         b.CustomerName,
                         CreatedDate = a.CreatedDate.ToString(),
                         a.ID,
                         EmployeeName = c.FirstName + " " + c.LastName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete


                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.EmployeeName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = 1;
            var data = v.Skip(skip).Take(pageSize).Distinct().ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [HttpGet]
        public ActionResult ApprovalorrejectCRM(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var Cus1 = (from a in db.LeadApprovedEmployees
                        where a.ID == id
                        select new
                        {
                            ID = a.ID,
                        }).FirstOrDefault();

            if (Cus1 == null)
            {
                return NotFound();
            }
            else
            {
                LeadApprovedEmployees Cus = db.LeadApprovedEmployees.Find(id);
                return PartialView(Cus);
            }
        }



        [HttpPost, ActionName("ApprovalorrejectCRM")]
        public ActionResult ApprovalorrejectCRMAction(long id)
        {
            bool stat = false;
            string msg;

            stat = ApproveCrm(id);
            msg = "Approved Succefully";

            // return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            if (1 == 1)
            {






                var leadid = db.LeadApprovedEmployees.Where(o => o.ID == id).Select(c => c.LeadID).FirstOrDefault();
                var tskasgn = db.AssignedTos.Where(a => a.CustomerID == leadid && a.Status == "Assigned" && a.ChkStatus == (int)Status.active).ToList();
                db.AssignedTos.Where(a => a.CustomerID == leadid && a.Status == "Assigned" && a.ChkStatus == (int)Status.active).ToList().ForEach(o => o.approve = false);
                db.SaveChanges();






            }

            Success("Approved Succefully", true);
            return RedirectToAction("LeadApprovalModal", "Leads");
        }

        public bool ApproveCrm(long paramID)
        {
            LeadApprovedEmployees leadApprovedEmployees = db.LeadApprovedEmployees.Find(paramID);
            Customer customer = db.Customers.Find(leadApprovedEmployees.LeadID);
            long nextaction = customer.NextAction;
            var lpw = db.LeadProcessFlows.Where(o => o.LeadStatus == nextaction).Select(o => o.taskid).FirstOrDefault();

            db.AssignedTos.Where(o => o.approve == true && o.CustomerID == leadApprovedEmployees.LeadID).ToList().ForEach(o => o.approve = false);
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var today = Convert.ToDateTime(System.DateTime.Now);

            DateTime? sDate = null;
            DateTime? eDate = null;
            DateTime? stimes = null;
            DateTime? etimes = null;

            sDate = System.DateTime.Now;
            TimeSpan? stime = null;

            stime = ((DateTime)System.DateTime.Now).TimeOfDay;

            stimes = sDate + stime;

            eDate = System.DateTime.Now;
            TimeSpan? etime = null;

            etime = ((DateTime)System.DateTime.Now).TimeOfDay;

            etimes = eDate + etime;
            Int64 proId = 1;

            var v = db.ProTasks.Where(o => o.CustomerID == leadApprovedEmployees.LeadID).FirstOrDefault();
            if (v == null)
            {
                ProTask task = new ProTask();

                task.TaskNo = GetProNo();
                task.TaskCode = InvoiceNo();
                task.TaskName = customer.CustomerName;
                task.ProjectId = 1;
                task.CustomerID = customer.CustomerID;
                task.TaskType = customer.LeadType;
                task.StartDate = sDate;
                task.StartTime = stimes;
                task.EndDate = eDate;
                task.EndTime = etimes;
                task.CreatedDate = today;
                task.CreatedBy = User.Identity.GetUserId();
                task.Status = Status.active;
                task.Branch = BranchID;


                if (db.LocationNames.Find(customer.LocationID) != null)
                {
                    var locationName = db.LocationNames.Find(customer.LocationID);
                }

                task.TaskStatus = lpw;
                task.SalesPerson = 0;
                task.logtime = System.DateTime.Now;

                db.ProTasks.Add(task);
                db.SaveChanges();
                proId = task.ProTaskId;


            }
            else
            {
                v.TaskStatus = lpw;
                v.CreatedBy = User.Identity.GetUserId();
                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
                proId = v.ProTaskId;
            }
            db.ContactRelation.RemoveRange(db.ContactRelation.Where(o => o.RelationID == proId && o.RelationType == 11));
            db.SaveChanges();
            var vvv = (
                from r in db.ContactRelation
                join c in db.Contacts on r.ContactID equals c.ContactID
                where r.RelationID == leadApprovedEmployees.LeadID
                select c
                ).ToList();

            if (vvv != null && vvv.Count > 0)
            {
                foreach (var item in vvv)
                {
                    var contact = new Contact
                    {

                        Address = item.Name,


                        Country = item.Country,

                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Name = item.FirstName + " " + item.LastName,

                        TypeOfContact = item.TypeOfContact,
                        Mobile = item.Mobile,
                        Phone = item.Phone,

                        EmailId = item.EmailId,

                        ContactPerson = item.Name,
                        Website = item.Website,
                        Group = 2,
                        Status = Status.active,
                        CountryID = item.CountryID,
                        ContactTypeID = item.ContactTypeID

                    };
                    db.Contacts.Add(contact);
                    db.SaveChanges();
                    var contactId = contact.ContactID;


                    ContactRelation Relation = new ContactRelation();
                    Relation.ContactID = contactId;
                    Relation.RelationType = 11;//for task
                    Relation.RelationID = proId;
                    db.ContactRelation.Add(Relation);
                    db.SaveChanges();
                }
            }

            ProTaskUpdation TaskUp = new ProTaskUpdation
            {
                ProTaskId = proId,
                CreatedBy = User.Identity.GetUserId(),
                CreatedDate = today,
            };
            db.ProTaskUpdations.Add(TaskUp);
            db.SaveChanges();
            Int64 TaskUpdId = TaskUp.TaskUpdationID;

            if (TaskUpdId > 0)
            {
                leadApprovedEmployees.Status = ApprovalStatus.Completed.ToString();

                db.Entry(leadApprovedEmployees).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }


        [HttpGet]
        public ActionResult Approvalreject(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var Cus1 = (from a in db.LeadApprovedEmployees
                        where a.ID == id
                        select new
                        {
                            ID = a.ID,
                        }).FirstOrDefault();

            if (Cus1 == null)
            {
                return NotFound();
            }
            else
            {
                LeadApprovedEmployees Cus = db.LeadApprovedEmployees.Find(id);
                return PartialView(Cus);
            }
        }



        [HttpPost, ActionName("Approvalreject")]
        public ActionResult ApprovalrejectAction(long id)
        {
            bool stat = false;
            string msg;
            if (1 == 1)
            {






                var leadid = db.LeadApprovedEmployees.Where(o => o.ID == id).Select(c => c.LeadID).FirstOrDefault();
                var tskasgn = db.AssignedTos.Where(a => a.CustomerID == leadid && a.Status == "Assigned" && a.ChkStatus == (int)Status.active).ToList();


                if (tskasgn != null)
                {
                    foreach (var arr in tskasgn)
                    {
                        AssignedTo tskassr = db.AssignedTos.Find(arr.AssignedToId);
                        tskassr.ChkStatus = (int)Status.inactive;

                        tskassr.Status = "Removed";
                        tskassr.ChkStatus = (int)Status.inactive;

                        db.Entry(tskassr).State = EntityState.Modified;
                        db.SaveChanges();

                    }

                }

            }

            stat = RejectApproval(id);
            msg = "Rejected Succefully";

            //  return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            Success("Approved Rejected", true);
            return RedirectToAction("LeadApprovalModal", "Leads");
        }

        public bool RejectApproval(long paramID)
        {
            LeadApprovedEmployees leadApprovedEmployees = db.LeadApprovedEmployees.Find(paramID);


            leadApprovedEmployees.Status = ApprovalStatus.Rejected.ToString();

            db.Entry(leadApprovedEmployees).State = EntityState.Modified;
            db.SaveChanges();
            return true;

        }


        private long GetProNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.number).FirstOrDefault();
            if ((db.ProTasks.Select(p => p.TaskNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    SENo = 1;
                }
                else
                {
                    SENo = number;
                }
            }
            else
            {
                SENo = db.ProTasks.Max(p => p.TaskNo + 1);
            }

            return SENo;
        }

        private string InvoiceNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.number).FirstOrDefault();
                if ((db.ProTasks.Select(p => p.TaskNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.ProTasks.Max(p => p.TaskNo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(SENo, billNo);
                }

            }
            return billNo;
        }

        private bool BillExist(string SENo)
        {
            var Exists = db.ProTasks.Any(c => c.TaskCode == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }

    }
}
