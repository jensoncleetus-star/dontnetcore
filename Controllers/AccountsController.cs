using QuickSoft.Web;
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
using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.IO;
using System.Net;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class AccountsController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AccountsController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        public ActionResult assetreport()
        {
            var OpAll = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                               }, "Value", "Text", 1);

            ViewBag.AccntGp = OpAll;

            ViewBag.Accs = OpAll;

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{

                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},

                        new SelectListItem() {Text = "All", Value= null},
            }, "Value", "Text");

            var TRN = db.Accountss.Where(x => x.TRN != null)
                             .Select(s => new
                             {
                                 ID = s.TRN,
                                 Name = s.TRN
                             })
                             .ToList();
            ViewBag.TRN = QkSelect.List(TRN, "ID", "Name");
            return View();
        }
        // GET: Accounts
        [QkAuthorize(Roles = "Dev,Expense Account")]
        public ActionResult Index()
        {
            var OpAll = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                               }, "Value", "Text", 1);

            ViewBag.AccntGp = OpAll;

            ViewBag.Accs = OpAll;

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
        
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},

                        new SelectListItem() {Text = "All", Value= null},
            }, "Value", "Text");

            var TRN = db.Accountss.Where(x => x.TRN != null)
                             .Select(s => new
                             {
                                 ID = s.TRN,
                                 Name = s.TRN
                             })
                             .ToList();
            ViewBag.TRN = QkSelect.List(TRN, "ID", "Name");
            return View();
        }

        //[QkAuthorize(Roles = "Dev,Accounts dashboard")]
        public ActionResult accountsdashboard(string fromdate, string todate)
        {
            long? AccGroup = null;
            bool? pdc = null;
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            string formattedtoDate = "";
            string formattedFromDate = "";
            if (fromdate != null)
            {
                formattedtoDate = todate;
                formattedFromDate = fromdate;
                ViewBag.frmdt = fromdate;
                ViewBag.todt = todate;
            }
            else
            {
                formattedtoDate = today.ToString("dd-MM-yyyy");
                formattedFromDate = today.AddYears(-3).ToString("dd-MM-yyyy");
                ViewBag.frmdt = formattedFromDate;
                ViewBag.todt = formattedtoDate;
            }

            int nextdateexpirty = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "NextDateExpiry").Select(o => o.TypeValue).FirstOrDefault());
            int updateddateexpirty = Convert.ToInt32(db.EnableSettings.Where(o => o.EnableType == "UpdatedDateExpiry").Select(o => o.TypeValue).FirstOrDefault());
            List<AccountMapUpdates> alldata = new List<AccountMapUpdates>();
            List<LedgerViewModel> alldata2 = new List<LedgerViewModel>();

            AccntDashboardViewModel ViewModel = new AccntDashboardViewModel();
            var data = (from a in db.accountmaps
                        join b in db.Accountss on a.AccountId equals b.AccountsID into cus2
                        from b in cus2.DefaultIfEmpty()
                        join c in db.AccountsTransactions on b.AccountsID equals c.Account into cus3
                        from c in cus3.DefaultIfEmpty()
                        group new { b.Name, a.AccountId, c.Debit, c.Credit, b.AccountsID } by new { a.AccountId } into g
                        select new AccountMapUpdates
                        {
                            Name = g.FirstOrDefault().Name,
                            ClosingBalance = (decimal?)0,
                            Accntid = g.FirstOrDefault().AccountsID,
                            FromDate = formattedFromDate,
                            toDate = formattedtoDate
                        }).ToList().Select(o => new AccountMapUpdates
                        {
                            Accntid = o.Accntid,
                            Name = o.Name,
                            ClosingBalance = com.GetDebit((long)o.Accntid, formattedFromDate, formattedtoDate, AccGroup, pdc),
                            FromDate = o.FromDate,
                            toDate = o.toDate,

                        });

            alldata.AddRange(data);

            var accountmapinglist = (from a in db.accountmaps
                                     group new { a.AccountId } by new { a.AccountId } into g
                                     select new
                                     {
                                         id = g.FirstOrDefault().AccountId,
                                     }).ToList();

            ViewModel.AccountMapUpdatess = alldata;

            //          group new { b.Debit, b.Credit, c.CustomerID } by new { b.Account } into g
            //              custid = g.FirstOrDefault().CustomerID

            //          }).Select(o => new
            //              bal = o.Debit - o.Credit,
            //              cid = o.custid



            //                      a.CustomerId,
            //                      a.CreatedDate
            //        ).OrderByDescending(o=>o.CreatedDate).GroupBy(o => o.CustomerId, (y, gr) => new
            //            lastupdateddate = gr.FirstOrDefault().CreatedDate,


            //        }).Where(o => DbFunctionsCompat.AddMinutes(o.lastupdateddate, updateddateexpirty) < System.DateTime.Now)
            //                 let  k= DbFunctionsCompat.AddMinutes(a.CreatedDate, updateddateexpirty)
            //                  group new { a.CreatedDate,k } by new { a.CustomerId } into gpr


            //                      CreatedDate = gpr.FirstOrDefault().CreatedDate,
            //                      update=gpr.FirstOrDefault().k,
            //        ).Where(o => o.update < System.DateTime.Now)


            DateTime ndate = DateTime.Now.AddYears(-2);
            var userid = User.Identity.GetUserId();

            long[] accGp = null;
            //creditor

            //sundry creditor
            var creparentid = new SqlParameter("@parentid", 11);
            var cregroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", creparentid).AsEnumerable().ToList();
            accGp = cregroupsdata.Select(a => a.AccountsGroupID).ToArray();


            var v2 = (from b in db.AccountsTransactions
                      join c in db.Customers on b.Account equals c.Accounts

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
                     // (DateTime?) casts force EF Core to emit a plain nullable scalar subquery instead of
                     // COALESCE(subquery, '0001-01-01...'); that literal overflows SQL `datetime` (1753-9999) and
                     // 500s for every remark-less customer. Legacy value restored via ?? DateTime.MinValue below.
                     let createdDate = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.CreatedDate).FirstOrDefault()
                     let nexttime = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID && c.AddedUser == userid).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.nexttime).FirstOrDefault()
                     let nexttime1 = db.CustomerRemarks.Where(c => c.CustomerId == b.CustomerID).OrderByDescending(c => c.CreatedDate).Select(o => (DateTime?)o.nexttime).FirstOrDefault()

                     //let CrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let DrSum = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && (pdc == true || b.Status == null) && (ondates == null || EF.Functions.DateDiffDay(b.Date, ondates) >= 0)).Select(b => b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()
                     //let aging = (from c in db.SalesEntrys
                     //             where d.SEPaidAmount == 0 && d.CustomerId == b.CustomerID


                     //             }).OrderBy(o => o.Days).Select(o => o.Days).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault()
                     where accGp.Contains(a.Group)
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


            var va = v.Where(o => (o.createdDate.AddMinutes(updateddateexpirty)) < System.DateTime.Now);

            va = va.Where(o => o.createdDate > ndate);
            ViewBag.updateddateexpirty = va.Count();




            var vb = v.Where(o => o.createdDate < ndate);

            ViewBag.notupdated = vb.Count();

            var vc = v.Where(o => (o.nextfolloupdatetime.AddMinutes(nextdateexpirty)) < System.DateTime.Now && o.nextfolloupdatetime > ndate);

            ViewBag.nextdateexpirty = vc.Count();











            //                      a.CustomerId,
            //                      a.nexttime
            //         ).OrderByDescending(o=>o.nexttime).GroupBy(o => o.CustomerId, (y, gr) => new
            //             nexdtdateupdated = gr.FirstOrDefault().nexttime,

            //         }).Where(o => DbFunctionsCompat.AddMinutes(o.nexdtdateupdated, nextdateexpirty) < System.DateTime.Now)

            //                  b.CreatedDate == null
            //                      a.CustomerID
            //                         b.CustomerID


            return View(ViewModel);

        }



        public JsonResult GetDatalink()
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
            Status st = new Status();
          

            var uDev = User.IsInRole("Dev");
            var uExpenseAccountStatus = User.IsInRole("Expense Account Status");

            var ModList = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into grp
                           from b in grp.DefaultIfEmpty()
                           join c in db.Users on a.CreatedBy equals c.Id into user
                           from c in user.DefaultIfEmpty()
                           where (a.Group != 12 && a.Group != 14 && a.Group != 8) &&
                          a.Name.Contains("asset")
                          && a.Status==Status.active
                          
                           select new
                           {
                               id = a.AccountsID,
                               a.Name,
                               a.Alias,
                               a.PrintName,
                               a.Note,
                               balance = (a.OpnBalanceCr > 0) ? (a.OpnBalanceCr != 0 ? a.OpnBalanceCr + " Cr." : "0.00") : (a.OpnBalance != 0 ? a.OpnBalance + " Dr." : "0.00"),
                               Credit = (db.AccountsTransactions.Where(d => d.Account == a.AccountsID && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0),
                               Debit = (db.AccountsTransactions.Where(b => b.Account == a.AccountsID && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0),
                               a.OpnBalance,
                               FirstName = c.Name,
                               a.Status,
                               a.OpnBalanceCr,
                               a.Editable,
                               Group = b.Name,
                               Dev = uDev,
                               ExpenseAccountStatus = uExpenseAccountStatus,
                               a.CreatedDate,
                               TRN = a.TRN

                           });
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.id.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Status.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Note.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Alias.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT
            if (sortColumn == "")
                sortColumn = "balance";
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });


        }



        [QkAuthorize(Roles = "Dev,Expense Account")]

        public JsonResult GetData(long? ddlAccounts, long? AccGroup, string Stats, string TRN, string Alias)
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
            Status st = new Status();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };

            var uDev = User.IsInRole("Dev");
            var uExpenseAccountStatus = User.IsInRole("Expense Account Status");

            var ModList = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into grp
                           from b in grp.DefaultIfEmpty()
                           join c in db.Users on a.CreatedBy equals c.Id into user
                           from c in user.DefaultIfEmpty()
                           where (a.Group != 12 && a.Group != 14 && a.Group != 8) &&
                           (ddlAccounts == null || ddlAccounts == 0 || a.AccountsID == ddlAccounts) &&
                           (AccGroup == null || AccGroup == 0 || a.Group == AccGroup)
                           && (Stats == null || Stats == "" || a.Status == st)
                           && (TRN == null || TRN == "" || a.TRN == TRN)
                           && (Alias == null || Alias == "" || a.Alias == Alias)
                           select new
                           {
                               id = a.AccountsID,
                               a.Name,
                               a.Alias,
                               a.PrintName,
                               a.Note,
                               balance = (a.OpnBalanceCr > 0) ? (a.OpnBalanceCr != 0 ? a.OpnBalanceCr + " Cr." : "0.00") : (a.OpnBalance != 0 ? a.OpnBalance + " Dr." : "0.00"),
                               Credit = (db.AccountsTransactions.Where(d => d.Account == a.AccountsID && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0),
                               Debit = (db.AccountsTransactions.Where(b => b.Account == a.AccountsID && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0),
                               a.OpnBalance,
                               FirstName = c.Name,
                               a.Status,
                               a.OpnBalanceCr,
                               a.Editable,
                               Group = b.Name,
                               Dev = uDev,
                               ExpenseAccountStatus = uExpenseAccountStatus,
                               a.CreatedDate,
                               TRN = a.TRN

                           });
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.id.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Status.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Note.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Alias.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT
            if (sortColumn == "")
                sortColumn = "balance";
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });


        }
        [QkAuthorize(Roles = "Dev,Create Expense Account")]
        public ActionResult Create()
        {
            AccountsViewModel bnkmodel = new AccountsViewModel();
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var mcs = db.Projects.Select(s => new
            {
                Id = s.ProjectId,
                Name = s.ProjectName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");

            var stands = db.AccountsGroups.Where(s => s.AccountsGroupID != 12 && s.AccountsGroupID != 14 && s.AccountsGroupID != 8
            && s.AccountsGroupID != 1 && s.AccountsGroupID != 2 && s.AccountsGroupID != 3 && s.AccountsGroupID != 6 && s.AccountsGroupID != 7
            && s.AccountsGroupID != 10 && s.AccountsGroupID != 11 && s.AccountsGroupID != 13)
                        .Select(s => new
                        {
                            FieldID = s.Name,
                            FieldName = s.AccountsGroupID
                        }).Take(1)
                         .ToList();
            ViewBag.AccGroup = QkSelect.List(stands, "FieldName", "FieldID");
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Expense Account")]
        public JsonResult Create(AccountsViewModel bnkview)
        {
            bool stat = false;
            string msg;
            Accounts Acc = new Accounts();
            if (bnkview.DC == DC.Debit)
            {
                Acc.OpnBalance = bnkview.OpnBalance;
                Acc.OpnBalanceCr = 0;
            }
            if (bnkview.DC == DC.Credit)
            {
                Acc.OpnBalance = 0;
                Acc.OpnBalanceCr = bnkview.OpnBalance;
            }

            Acc.PrintName = bnkview.PrintName;
            Acc.Name = bnkview.Name;
            Acc.Alias = bnkview.Alias;
            Acc.PrevBalance = bnkview.PrevBalance;
            Acc.Note = bnkview.Note;
            Acc.Status = bnkview.Status;
            Acc.Group = bnkview.Group;
            Acc.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
            Acc.Editable = 0;
            Acc.TRN = bnkview.TRN;
            if (bnkview.ddlMC != 0)
            {
                Acc.mc = bnkview.ddlMC;
            }
            db.Accountss.Add(Acc);
            db.SaveChanges();

            if (bnkview.OpnBalance > 0)
            {
                if (bnkview.DC == DC.Debit)
                {
                    com.addAccountTrasaction(bnkview.OpnBalance, 0, Acc.AccountsID, "Opening Balance", Acc.AccountsID, DC.Debit,null,null,null,Acc.mc);

                }
                if (bnkview.DC == DC.Credit)
                {
                    com.addAccountTrasaction(0, bnkview.OpnBalance, Acc.AccountsID, "Opening Balance", Acc.AccountsID, DC.Credit, null, null, null, Acc.mc);
                }
            }
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Created, UserId, "Accounts", "Accounts", findip(), Acc.AccountsID, "Account Created Successfully");


            msg = "Account Created Successfully.";
            stat = true;


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [QkAuthorize(Roles = "Dev,Edit Expense Account")]
        public ActionResult Edit(long id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var mcs = db.Projects.Select(s => new
            {
                Id = s.ProjectId,
                Name = s.ProjectName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");

            Accounts account = db.Accountss.Find(id);
            if (account == null)
            {
                return NotFound();
            }

            AccountsViewModel bnkmodel = new AccountsViewModel();

            bnkmodel.AccountsID = account.AccountsID;
            bnkmodel.Name = account.Name;
            bnkmodel.Note = account.Note;
            bnkmodel.Status = account.Status;
            bnkmodel.Group = account.Group;
            bnkmodel.TRN = account.TRN;
            bnkmodel.Alias = account.Alias;
            ViewBag.Editable = (account.Editable == choice.Yes) ? 1 : 0;
            if (account.OpnBalance == 0)
            {
                bnkmodel.DC = DC.Credit;
                bnkmodel.OpnBalance = account.OpnBalanceCr;
            }
            else
            {
                bnkmodel.DC = DC.Debit;
                bnkmodel.OpnBalance = account.OpnBalance;
            }
            bnkmodel.ddlMC = account.mc;
            var stands = db.AccountsGroups.Where(s => s.AccountsGroupID == bnkmodel.Group)
                     .Select(s => new
                     {
                         FieldID = s.Name,
                         FieldName = s.AccountsGroupID
                     })
                     .ToList();
            ViewBag.AccGroup = QkSelect.List(stands, "FieldName", "FieldID");

            return PartialView(bnkmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Expense Account")]
        public JsonResult Edit(AccountsViewModel bnkview, long id)
        {
            bool stat = false;
            string msg;
            var Editable = db.Accountss.Any(a => a.Editable == choice.No && a.AccountsID == id);

            if (1 == 2)
            {
                msg = "Sorry, It is a Pre-defined Account And Cannot be Edited.";
                stat = false;
            }
            else
            {
                Accounts Acc = new Accounts();
                Acc.Name = bnkview.Name;

                if (bnkview.DC == DC.Debit)
                {
                    Acc.OpnBalance = bnkview.OpnBalance;
                    Acc.OpnBalanceCr = 0;
                }
                if (bnkview.DC == DC.Credit)
                {
                    Acc.OpnBalance = 0;
                    Acc.OpnBalanceCr = bnkview.OpnBalance;
                }
                Acc.Note = bnkview.Note;
                Acc.Status = bnkview.Status;
                Acc.Group = bnkview.Group;
                Acc.TRN = bnkview.TRN;

                Acc.Alias = bnkview.Alias;

                if (bnkview.ddlMC != 0)
                {
                    Acc.mc = bnkview.ddlMC;
                }
                editAccount(Acc, bnkview.AccountsID, Editable);


                bool delete = com.DeleteAllAccountTransaction("Opening Balance", bnkview.AccountsID);

                if (bnkview.OpnBalance >= 0)
                {
                    if (bnkview.DC == DC.Debit)
                    {
                        com.addAccountTrasaction(bnkview.OpnBalance, 0, bnkview.AccountsID, "Opening Balance", bnkview.AccountsID, DC.Debit,null,null,null,bnkview.ddlMC);

                    }
                    if (bnkview.DC == DC.Credit)
                    {
                        com.addAccountTrasaction(0, bnkview.OpnBalance, bnkview.AccountsID, "Opening Balance", bnkview.AccountsID, DC.Credit, null, null, null, bnkview.ddlMC);
                    }
                }


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Updated, UserId, "Accounts", "Accounts", findip(), bnkview.AccountsID, "Successfully Updated Account");


                msg = "Successfully updated Account details.";
                stat = true;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        private long editAccount(Accounts ACS, long id, bool editable)
        {
            Accounts account = db.Accountss.Find(id);
            if (1 == 1)
            {
                account.PrintName = ACS.PrintName;
                account.Name = ACS.Name;
                account.Alias = ACS.Alias;
                account.OpnBalance = ACS.OpnBalance;
                account.OpnBalanceCr = ACS.OpnBalanceCr;
                account.PrevBalance = ACS.PrevBalance;
                account.Note = ACS.Note;
                account.Status = ACS.Status;
                account.Group = ACS.Group;
                account.TRN = ACS.TRN;
                account.Alias = ACS.Alias;
            }
            account.mc = ACS.mc;

            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();

            return db.SaveChanges();
        }


        // GET: Desg/Delete/5
        [QkAuthorize(Roles = "Dev,Delete Expense Account")]
        public ActionResult Delete(long id)
        {
            Accounts AccInfo = db.Accountss.Find(id);
            if (AccInfo == null)
            {
                return NotFound();
            }
            return PartialView(AccInfo);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Expense Account")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Editable = db.Accountss.Any(a => a.Editable == choice.No && a.AccountsID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Account And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                Accounts Accinfo = db.Accountss.Where(a => a.AccountsID == id && a.Editable == 0).FirstOrDefault();
                if (Accinfo != null)
                {
                    var payrec = chkDeleteWithMsg(id); ;
                    if (payrec != null)
                    {
                        msg = payrec;
                        stat = false;
                    }
                    else
                    {
                        stat = DeleteFn(Accinfo, id);
                        msg = "Successfully Deleted Accounts details.";
                    }
                }
                else
                {
                    msg = "Unable to delete Account.";
                    stat = false;
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Expense Account")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Accounts, Unable to Delete " + notdel + " Accounts. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Accounts.", true);
            }
            else
            {
                Success("Deleted " + count + " Accounts.", true);
            }
            return RedirectToAction("Index", "Accounts");
        }
        private Boolean DeleteItem(long id)
        {
            Accounts Accinfo = db.Accountss.Where(a => a.AccountsID == id && a.Editable == 0).FirstOrDefault();
            if (Accinfo != null)
            {
                //    || db.ContraVouchers.Any(c => c.PayFrom == id) || db.ContraVouchers.Any(c => c.PayTo == id)
                //    || db.Journals.Any(c => c.PayTo == id) || db.Journals.Any(c => c.PayFrom == id)
                //    || db.CreditNotes.Any(c => c.PayTo == id) || db.CreditNotes.Any(c => c.PayFrom == id)


                var Msg = chkDeleteWithMsg(id);
                if (Msg != null)
                {
                    return false;
                }
                else
                {
                    return DeleteFn(Accinfo, id);
                }
            }
            else
            {
                return false;
            }
        }

        public string chkDeleteWithMsg(long Id)
        {
            string msg = null;
            if (db.Payments.Any(c => c.PayTo == Id) || db.Payments.Any(c => c.PayFrom == Id))
            {
                msg = "Account Already used in Payments !!";
            }
            else if (db.Receipts.Any(c => c.PayFrom == Id) || db.Receipts.Any(c => c.PayTo == Id))
            {
                msg = "Account Already used in Receipts !!";
            }
            else if (db.ContraVouchers.Any(c => c.PayFrom == Id) || db.ContraVouchers.Any(c => c.PayTo == Id))
            {
                msg = "Account Already used in Contra Vouchers !!";
            }

            else if (db.Journals.Any(c => c.PayTo == Id) || db.Journals.Any(c => c.PayFrom == Id))
            {
                string v = db.Journals.Where(c => c.PayTo == Id).Select(o => o.VoucherNo).FirstOrDefault();
                msg = "Account Already used in Journals !! voucher no" + v;
            }
            else if (db.CreditNotes.Any(c => c.PayTo == Id) || db.CreditNotes.Any(c => c.PayFrom == Id))
            {
                msg = "Account Already used in CreditNotes !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }




        public bool DeleteFn(Accounts Accinfo, long? id)
        {
            bool delete = com.DeleteAllAccountTransaction("Opening Balance", Accinfo.AccountsID);
            if (Accinfo != null)
            {
                db.Accountss.RemoveRange(db.Accountss.Where(a => a.AccountsID == id));
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Master", "Accountss", findip(), Accinfo.AccountsID, "Successfully Deleted Accounts");
            db.SaveChanges();
            return true;
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View Contact")]
        public ActionResult ViewDetails(long? id)
        {
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Accounts con = db.Accountss.Find(id);

                if (con == null)
                {
                    return NotFound();
                }

                AccountsViewModel acc = new AccountsViewModel();

                acc = (from a in db.Accountss
                       join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into cong
                       from b in cong.DefaultIfEmpty()
                       where a.AccountsID == con.AccountsID
                       select new AccountsViewModel
                       {
                           Name = a.Name,
                           Alias = a.Alias,
                           TRN = a.TRN,
                           GroupName = b.Name,
                           Statusname = a.Status == Status.active ? "active" : "inactive",
                           Note = a.Note,
                           opbal = (a.OpnBalance == 0) ? a.OpnBalanceCr + " Cr" : a.OpnBalance + " Dr",

                       }).FirstOrDefault();

                return PartialView(acc);
            }
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Expense Account Status")]
        public ActionResult AccountStatus(string type, long id)
        {
            Accounts Acc = db.Accountss.Find(id);
            if (Acc == null)
            {
                return NotFound();
            }
            if (type == "active")
            {
                ViewBag.type = "Active";
                ViewBag.link = "active";
                ViewBag.status = Status.active;
            }
            else
            {
                ViewBag.type = "Inactive";
                ViewBag.link = "inactive";
                ViewBag.status = Status.inactive;
            }
            return PartialView();
        }
        // POST: master/ChangeStatus/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Expense Account Status")]
        public JsonResult AccountStatus(string type, long id, Accounts AccG)
        {
            bool stat = false;
            string msg;
            string types = ""; var Editable = db.Accountss.Any(a => a.Editable == choice.No && a.AccountsID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Account And Cannot be Changed.";
                stat = false;
            }
            else
            {
                Accounts Acc = db.Accountss.Find(id);
                if (AccG.Status == Status.inactive)
                {
                    types = " Inactive";
                    Acc.Status = Status.inactive;
                }
                else
                {
                    types = " Active";
                    Acc.Status = Status.active;
                }

                db.Entry(Acc).State = EntityState.Modified;
                var updates = db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Changed, UserId, "Master", "Accountss", findip(), Acc.AccountsID, "Successfully Changed the Accounts to" + types);


                stat = true;
                msg = " Successfully Changed the Accounts to" + types;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public JsonResult SupNExpAccounts(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            // Supplier accounts group -14, expense =13
            var supparentid = new SqlParameter("@parentid", 14);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var arr = supgpid.Union(expgpid).ToArray();
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Accountss.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Alias.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) || p.Alias.Contains(q)) && arr.Contains(p.Group) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Accountss.Where(p => arr.Contains(p.Group) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();

            }
            return Json(serialisedJson);
        }

        public JsonResult CusNExpAccounts(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            // customer accounts group -12, expense =13
            var cusparentid = new SqlParameter("@parentid", 12);
            var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).AsEnumerable().ToList();
            var cusgpid = cusgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var arr = cusgpid.Union(expgpid).ToArray();
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Accountss.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) || p.Alias.Contains(q)) && arr.Contains(p.Group) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Accountss.Where(p => arr.Contains(p.Group) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();

            }
            return Json(serialisedJson);
        }
        //exp only
        public JsonResult ExpAccounts(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            // expense =13
            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var arr = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Accountss.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) || p.Alias.Contains(q)) && arr.Contains(p.Group) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Accountss.Where(p => arr.Contains(p.Group) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();

            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Account" };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult SearchAccounts(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            // customer, suplier, expense account list
            // Supplier accounts group -14, expense =13, customer -12

            ////bank

            ////cash





            //account except walking customer//bank//cash-in-hand
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                //           where (a.AccountsID != 4 && (e.Mobile.ToLower().Contains(q.ToLower()) || a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && a.Status == Status.active)//&& arr.Contains(a.Group)
                //           && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId))
                //               text = a.Name,
                //               id = a.AccountsID,
                //               Name = b.Name
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.AccountsID != 4 && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && a.Status == Status.active)//&& arr.Contains(a.Group)
                          && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).Distinct().ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.AccountsID != 4 && a.Status == Status.active)//&& arr.Contains(a.Group)
                           && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        // list all accounts except walking customerse
        public JsonResult AllAccounts(string q, string x, string page)
        {
            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where a.AccountsID != 4 && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && (a.Status == Status.active) //&& a.Group != 23
                           //&& (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.AccountsID != 4) && (a.Status == Status.active)//&& a.Group != 23
                                                                                   // && (userpermission == true || a.CreatedBy == UserId)
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectMultiFormat() { id = 0, text = "Select Account" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult AllAccountsLand(string q, string x, string page)
        {
            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var hmt = (from a in db.Accountss
                           join c in db.Landlords on a.AccountsID equals c.Accounts
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()

                           where a.AccountsID != 4 && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && (a.Status == Status.active) //&& a.Group != 23
                           //&& (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join c in db.Landlords on a.AccountsID equals c.Accounts
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()

                           where (a.AccountsID != 4) && (a.Status == Status.active)//&& a.Group != 23
                                                                                   // && (userpermission == true || a.CreatedBy == UserId)
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectMultiFormat() { id = 0, text = "Select Account" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        //all Accounts Except Stock adjustment Expense
        public JsonResult AllAccountsExpStkAdj(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           where a.AccountsID != 499 && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && a.Status == Status.active
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           where a.AccountsID != 499 && a.Status == Status.active
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        public JsonResult SearchAccountsPayTo(string q, string x, string page)
        {
            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            List<SelectMultiFormat> serialisedJson;
            IList<long> acc;

            ////expense

            ////bank

            ////cash


            //account except walking customer//bank//cash-in-hand
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()

                           where (a.AccountsID != 4 && a.Status == Status.active && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)))//&& acc.Contains(a.Group)
                           && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).Distinct().ToList();

                serialisedJson = hmt;
            }
            else
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.AccountsID != 4 && a.Status == Status.active) //&& acc.Contains(a.Group)
                           && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;
            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult SearchAccountsPayToexpense(string q, string x, string page,long acgroup)
        {
            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            List<SelectMultiFormat> serialisedJson;
            IList<long> acc;

            ////expense
            ///

            var expparentid = new SqlParameter("@parentid", acgroup);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            ////bank

            ////cash


            //account except walking customer//bank//cash-in-hand
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()

                           where (a.AccountsID != 4 && a.Status == Status.active && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)))&& expgpid.Contains(a.Group)
                           && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).Distinct().ToList();

                serialisedJson = hmt;
            }
            else
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.AccountsID != 4 && a.Status == Status.active) && expgpid.Contains(a.Group)
                           && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;
            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        //list all bank accounts except walking customerse
        public JsonResult AllBankAccounts(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Accountss.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Alias.ToLower().Contains(q.ToLower()) || p.Alias.Contains(q) || p.Name.Contains(q)) && p.Status == Status.active && p.Group == 8)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Accountss.Where(p => p.Group == 8 && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();

            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult AccountsByTypeMini(string q, string x, string stt, string page)
        {
            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            IList<long> acc;
            if (x == "Customer")
            {
                var cusparentid = new SqlParameter("@parentid", 12);
                var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).AsEnumerable().ToList();
                var cusgpid = cusgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = cusgpid;

            }
            else if (x == "Supplier")
            {
                var supparentid = new SqlParameter("@parentid", 14);
                var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
                var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = supgpid;
            }
            else if (x == "Expense")
            {
                var expparentid = new SqlParameter("@parentid", 13);
                var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
                var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = expgpid;
            }
            else if (x == "Bank")
            {
                var expparentid = new SqlParameter("@parentid", 8);
                var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
                var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = expgpid;
            }
            else if (x == "Cash In Hand")
            {
                var expparentid = new SqlParameter("@parentid", 9);
                var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
                var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = expgpid;
            }
            else if (x == "All")
            {
                var allacounts = db.AccountsGroups.Select(a => a.AccountsGroupID).ToArray();
                acc = allacounts;
            }
            else if (x == "NoCash")
            {
                var allacounts = db.AccountsGroups.Where(a => a.AccountsGroupID != 9 && a.AccountsGroupID != 8).Select(a => a.AccountsGroupID).ToArray();
                acc = allacounts;

            }
            else if (x == "AllNoCash")
            {
                stt = "All";
                var allacounts = db.AccountsGroups.Where(a => a.AccountsGroupID != 9 && a.AccountsGroupID != 8).Select(a => a.AccountsGroupID).ToArray();
                acc = allacounts;
            }
            else
            {
                var group = Convert.ToInt64(x);
                var cusparentid = new SqlParameter("@parentid", group);
                var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).AsEnumerable().ToList();
                var arr = cusgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = arr;
            }
            var userid = User.Identity.GetUserId();
            var employeeid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            var accoutid = db.accountmaps.Where(o => o.EmployeeId == employeeid).Select(o => o.AccountId).ToArray();


            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                string[] searchkey = q.Split(' ');
                string secnd = "";
                string third = "";
                bool checksecnd = false;
                bool thirdcheck = false;
                if (searchkey.Count() > 1)
                {
                    secnd = searchkey[1];
                    q = searchkey[0];
                    checksecnd = true;
                }
                if (searchkey.Count() > 2)
                {
                    third = searchkey[2];
                    thirdcheck = true;
                }


                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()

                           where (a.AccountsID != 4 && a.Status == Status.active &&
                           (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)))
                           && (secnd == "" || a.Name.ToLower().Contains(secnd.ToLower()))
                            && (third == "" || a.Alias.ToLower().Contains(third.ToLower()))

                           && accoutid.Contains(a.AccountsID)
                           select new SelectMultiFormat

                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;
            }
            else
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.AccountsID != 4 && a.Status == Status.active && acc.Contains(a.Group))
                           && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId))
                           && accoutid.Contains(a.AccountsID)
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;
            }
            var isdata = serialisedJson.Any();
            if ((isdata) && stt == "All" && x != "All" && x != "NoCash" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())) || x == "AllNoCash")
            {
                var initial = new SelectMultiFormat() { id = -1, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        // complete account list by type
        public JsonResult AccountsByType(string q, string x, string stt, string page)
        {
            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            IList<long> acc;
            if (x == "Customer")
            {
                var cusparentid = new SqlParameter("@parentid", 12);
                var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).AsEnumerable().ToList();
                var cusgpid = cusgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = cusgpid;

            }
            else if (x == "Supplier")
            {
                var supparentid = new SqlParameter("@parentid", 14);
                var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
                var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = supgpid;
            }
            else if (x == "Expense")
            {
                var expparentid = new SqlParameter("@parentid", 13);
                var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
                var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = expgpid;
            }
            else if (x == "Bank")
            {
                var expparentid = new SqlParameter("@parentid", 8);
                var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
                var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = expgpid;
            }
            else if (x == "Cash In Hand")
            {
                var expparentid = new SqlParameter("@parentid", 9);
                var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
                var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = expgpid;
            }
            else if (x == "All")
            {
                var allacounts = db.AccountsGroups.Select(a => a.AccountsGroupID).ToArray();
                acc = allacounts;
            }
            else if (x == "NoCash")
            {
                var allacounts = db.AccountsGroups.Where(a => a.AccountsGroupID != 9 && a.AccountsGroupID != 8).Select(a => a.AccountsGroupID).ToArray();
                acc = allacounts;

            }
            else if (x == "AllNoCash")
            {
                stt = "All";
                var allacounts = db.AccountsGroups.Where(a => a.AccountsGroupID != 9 && a.AccountsGroupID != 8).Select(a => a.AccountsGroupID).ToArray();
                acc = allacounts;
            }
            else
            {
                var group = Convert.ToInt64(x);
                var cusparentid = new SqlParameter("@parentid", group);
                var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).AsEnumerable().ToList();
                var arr = cusgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                acc = arr;
            }
            long grpp = 0;
            if (x != "All" && x != "Customer" && x != "Supplier")
            {
                grpp = Convert.ToInt64(x);
            }
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                string[] searchkey = q.Split(' ');
                string secnd = "";
                string third = "";
                bool checksecnd = false;
                bool thirdcheck = false;
                string orgq = q;
                if (searchkey.Count() > 1)
                {
                    secnd = searchkey[1];
                    q = searchkey[0];
                    checksecnd = true;
                }
                if (searchkey.Count() > 2)
                {
                    third = searchkey[2];
                    thirdcheck = true;
                }

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.AccountsID != 4 && a.Status == Status.active)
                            && (secnd == "" || a.Name.ToLower().Contains(secnd.ToLower()))
                            && (third == "" || a.Name.ToLower().Contains(third.ToLower()))
                           && (a.Name.ToLower().Contains(orgq.ToLower()))
                           && (x == "All" || a.Group == grpp)
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;
            }
            else
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.AccountsID != 4 && a.Status == Status.active && acc.Contains(a.Group))
                           && (c.CustomerName == null || (userpermission == true || a.CreatedBy == UserId)) &&
                            (x == "All" || a.Group == grpp)
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;
            }
            var isdata = serialisedJson.Any();
            if ((isdata) && stt == "All" && x != "All" && x != "NoCash" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())) || x == "AllNoCash")
            {
                var initial = new SelectMultiFormat() { id = -1, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        // Search Account Group
        public JsonResult SearchGroup(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AccountsGroups.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Alias.ToLower().Contains(q.ToLower()) || p.Alias.Contains(q) || p.Name.Contains(q)))
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name,
                                  id = b.AccountsGroupID
                              })
                              .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.AccountsGroups.Select(b => new SelectFormat
                {
                    text = b.Name,
                    id = b.AccountsGroupID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = -1, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchGroupexp(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AccountsGroups.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Alias.ToLower().Contains(q.ToLower()) || p.Alias.Contains(q) || p.Name.Contains(q)))
                    .Where(o => o.AccountsGroupID == 29 || o.Parent == 29 || o.AccountsGroupID == 30 || o.Parent == 30)
                    .Select(b => new SelectFormat
                    {
                        text = b.Name,
                        id = b.AccountsGroupID
                    })
                              .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.AccountsGroups.Where(o => o.AccountsGroupID == 29 || o.Parent == 29 || o.AccountsGroupID == 30 || o.Parent == 30).Select(b => new SelectFormat
                {
                    text = b.Name,
                    id = b.AccountsGroupID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }

            var initial = new SelectFormat() { id = -1, text = stt };
            serialisedJson.Insert(0, initial);

            return Json(serialisedJson);
        }
        // Search Account Group
        public JsonResult SearchInAccount(string q, string x, long? y)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            long[] groups = { 12, 14, 8 };
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.AccountsGroups
                                  let f = db.Accountss.Where(xb => xb.Group == a.AccountsGroupID).Count()
                                  where f == 0 && a.AccountsGroupID != y && !groups.Contains(a.AccountsGroupID)
                                  && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsGroupID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.AccountsGroups
                                  let f = db.Accountss.Where(xb => xb.Group == a.AccountsGroupID).Count()
                                  where f == 0 && a.AccountsGroupID != y && !groups.Contains(a.AccountsGroupID)
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsGroupID
                                  }).OrderBy(b => b.text).ToList();
            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = -1, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Account Groups" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "AllGroup" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchInAccountGp(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            long[] groups = { 12, 14, 8 };
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.AccountsGroups
                                  where !groups.Contains(a.AccountsGroupID)
                                  && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsGroupID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.AccountsGroups
                                  where !groups.Contains(a.AccountsGroupID)
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsGroupID
                                  }).OrderBy(b => b.text).ToList();
            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = -1, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Account Groups" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        public JsonResult SearchPaymentFrom(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           where (a.Group == 9 || a.Group == 8 || a.Group == 3 || a.Group == 6) && a.Status == Status.active
                           && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;
            }
            else
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           where (a.Group == 9 || a.Group == 8) && a.Status == Status.active
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;
            }

            return Json(serialisedJson);
        }

        // Search Account Group
        public JsonResult SearchInAccGp(string q, string x, long? y)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                serialisedJson = db.AccountsGroups.Where(p => p.AccountsGroupID != 8 && p.AccountsGroupID != 12 && p.AccountsGroupID != 14 && (p.AccountsGroupID != y) && (p.Name.ToLower().Contains(q.ToLower()) || p.Alias.ToLower().Contains(q.ToLower()) || p.Alias.Contains(q) || p.Name.Contains(q)))
                         .Select(b => new SelectFormat
                         {
                             text = b.Name,
                             id = b.AccountsGroupID
                         })
                         .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.AccountsGroups.Where(p => p.AccountsGroupID != 8 && p.AccountsGroupID != 12 && p.AccountsGroupID != 14 && p.AccountsGroupID != y).Select(b => new SelectFormat
                {
                    text = b.Name,
                    id = b.AccountsGroupID
                }).OrderBy(b => b.text).ToList();
            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = -1, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Account Groups" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        public JsonResult SearchAccInAcc(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var parents = db.AccountsGroups.Select(a => a.Parent).ToList().ToArray();
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                serialisedJson = db.AccountsGroups.Where(p => p.AccountsGroupID != 8 && p.AccountsGroupID != 12 && p.AccountsGroupID != 14 && (p.Name.ToLower().Contains(q.ToLower()) || p.Alias.ToLower().Contains(q.ToLower()) || p.Alias.Contains(q) || p.Name.Contains(q))
              )
                         .Select(b => new SelectFormat
                         {
                             text = b.Name,
                             id = b.AccountsGroupID
                         })
                         .OrderBy(b => b.text).ToList();
            }
            else
            {//&& !parents.Contains(p.AccountsGroupID) REMOVED
                serialisedJson = db.AccountsGroups.Where(p => p.AccountsGroupID != 8 && p.AccountsGroupID != 12 && p.AccountsGroupID != 14).Select(b => new SelectFormat
                {
                    text = b.Name,
                    id = b.AccountsGroupID
                }).OrderBy(b => b.text).ToList();
            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = -1, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Account Groups" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        // purchse pending bills of a supplier
        [HttpPost]
        public JsonResult ChekPurchase(long id)
        {
            object data = null;
            var supplierid = (from a in db.Suppliers where a.Accounts == id select new { a.SupplierID }).SingleOrDefault();
            data = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1);
            if (supplierid != null)
            {
                data = (from a in db.PurchaseEntrys
                        join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                        where a.Supplier == supplierid.SupplierID && c.PEPaidAmount != c.PEBillAmount
                        select new
                        {
                            invoiceno = a.BillNo,
                            Date = a.PEDate,
                            total = a.PEGrandTotal,
                            paid = c.PEPaidAmount,
                            bill = a.PurchaseEntryId,
                            type = "Purchase Invoice"
                        }).ToList();
            }
            var custid = (from a in db.Customers where a.Accounts == id select new { a.CustomerID }).SingleOrDefault();
            if (custid != null)
            {
                data = (from a in db.SalesReturns
                        join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                        where a.Customer == custid.CustomerID && c.SReturnAmount != c.SRBillAmount
                        select new
                        {
                            invoiceno = a.BillNo,
                            Date = a.SRDate,
                            total = a.SRGrandTotal,
                            paid = c.SReturnAmount,
                            bill = a.SalesReturnId,
                            type = "Sale Return Invoice"
                        }).ToList();
            }
            var Balance = com.Accbalance(id);


            return Json(new { balance = Balance, data = data });
        }
        // sales pending bills of a customer
        [HttpPost]
        public JsonResult ChekSale(long id)
        {
            object data = null;
            var custid = (from a in db.Customers where a.Accounts == id select new { a.CustomerID }).SingleOrDefault();
            if (custid != null)
            {
                data = (from a in db.SalesEntrys
                        join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                        where a.Customer == custid.CustomerID && c.SEPaidAmount != c.SEBillAmount
                        select new
                        {
                            invoiceno = a.BillNo,
                            Date = a.SEDate,
                            total = a.SEGrandTotal,
                            paid = c.SEPaidAmount,
                            bill = a.SalesEntryId,
                            type = "Sale Entry Invoice"
                        }).ToList();
            }
            var supplierid = (from a in db.Suppliers where a.Accounts == id select new { a.SupplierID }).SingleOrDefault();
            if (supplierid != null)
            {
                data = (from a in db.PurchaseReturns
                        join c in db.PRPayments on a.purchaseEntryId equals c.PurchaseReturnId
                        where a.Supplier == supplierid.SupplierID && c.PRBillAmount != c.PReturnAmount
                        select new
                        {
                            invoiceno = a.BillNo,
                            Date = a.PRDate,
                            total = a.PRGrandTotal,
                            paid = c.PReturnAmount,
                            bill = a.PurchaseReturnId,
                            type = "Purchase Return Invoice"
                        }).ToList();
            }
            var Balance = com.Accbalance(id);
            return Json(new { balance = Balance, data = data });
        }
        public JsonResult SearchAccountsCNote(string q, string x, int? ctype, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            // Supplier accounts group -14, customer -12
            var supparentid = new SqlParameter("@parentid", 14);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var cusparentid = new SqlParameter("@parentid", 12);
            var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).AsEnumerable().ToList();
            var cusgpid = cusgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            long[] arr = new long[] { };
            arr = supgpid.Union(cusgpid).ToArray();

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                                  from b in gp.DefaultIfEmpty()
                                  where a.Status == Status.active && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && arr.Contains(a.Group) && a.AccountsID != 4
                                  select new SelectMultiFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID,
                                      Name = b.Name
                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                                  from b in gp.DefaultIfEmpty()
                                  where a.Status == Status.active && arr.Contains(a.Group) && a.AccountsID != 4
                                  select new SelectMultiFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID,
                                      Name = b.Name
                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchAccountsByIdSelect(string q, string x, long account)
        {
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 10;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;



            object data = null;
            var custid = (from a in db.Customers where a.Accounts == account select new { a.CustomerID }).SingleOrDefault();
            if (custid != null)
            {
                var invs = (from a in db.ReceiptBills
                            join b in db.SalesEntrys on a.InvoiceNo equals b.SalesEntryId
                            where b.Customer == custid.CustomerID &&
                            a.Amount == b.SEGrandTotal
                            select new
                            {
                                a.InvoiceNo
                            }
                         ).Select(o => o.InvoiceNo);
                data = (from a in db.SalesEntrys
                        join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                        join b in db.ReceiptBills on a.SalesEntryId equals b.InvoiceNo into rec
                        from b in rec.DefaultIfEmpty()

                        let salesreturn = (decimal?)(from d in db.SalesReturns
                                                     where d.SalesEntryId == a.SalesEntryId
                                                     select new
                                                     {
                                                         d.SRGrandTotal
                                                     }).Sum(o => o.SRGrandTotal) ?? 0
                        let payment = (decimal?)(from e in db.PaymentBills
                                                 join f in db.Payments on e.Payment equals f.PaymentId into reciept2

                                                 join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                 join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                 where h.SalesEntryId == a.SalesEntryId &&
                                                 e.BillType == "Sales Return"
                                                 select new
                                                 {
                                                     amt = e.Amount
                                                 }).Sum(o => o.amt) ?? 0
                        let paymentjrnl = (decimal?)(from e in db.JornalPaymentBills
                                                     join f in db.Journals on e.Jornal equals f.JournalId into reciept2

                                                     join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                     join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                     where h.SalesEntryId == a.SalesEntryId &&
                                                     e.BillType == "Sales Return"
                                                     select new
                                                     {
                                                         amt = e.Amount
                                                     }).Sum(o => o.amt) ?? 0
                        where a.Customer == custid.CustomerID && (b != null || (c.SEPaidAmount != c.SEBillAmount))
                        //&& (entry == null || a.SalesEntryId == entry)
                        && !invs.Contains(a.SalesEntryId)
                        select new
                        {
                            text = a.BillNo,
                            Date = a.SEDate,
                            Amount = c.SEBillAmount,
                            paid = c.SEPaidAmount,
                            id = a.SalesEntryId,
                            type = "Sales",
                            Balance = (c.SEBillAmount - c.SEPaidAmount) - (((salesreturn == null) ? 0 : salesreturn) - ((payment == null) ? 0 : payment) - ((paymentjrnl == null) ? 0 : paymentjrnl)),
                        }).Distinct().Where(b => b.Balance > 0 && b.text.Contains(q)).OrderByDescending(b => b.Date).Skip(skip).Take(pageSize).ToList();
            }
            var supplierid = (from a in db.Suppliers where a.Accounts == account select new { a.SupplierID }).SingleOrDefault();
            if (supplierid != null)
            {
                data = (from a in db.PurchaseReturns
                        join c in db.PRPayments on a.PurchaseReturnId equals c.PurchaseReturnId
                        join b in db.ReceiptBills on a.PurchaseReturnId equals b.InvoiceNo into rec
                        from b in rec.DefaultIfEmpty()
                        where a.Supplier == supplierid.SupplierID && (b == null || (c.PRBillAmount != c.PReturnAmount))
                        //&& (entry == null || a.PurchaseReturnId == entry)
                        select new
                        {
                            text = a.BillNo,
                            Date = a.PRDate,
                            Amount = c.PRBillAmount,
                            paid = c.PReturnAmount,
                            id = a.PurchaseReturnId,
                            type = "Purchase Return",
                            Balance = (c.PRBillAmount - c.PReturnAmount)
                        }).Distinct().Where(b => b.Balance > 0 && b.text.Contains(q)).OrderByDescending(b => b.Date).Skip(skip).Take(pageSize).ToList();
            }
            return Json(data);
        }

        public JsonResult SearchAccountsById(long account, long entry)
        {

            object data = null;
            var custid = (from a in db.Customers where a.Accounts == account select new { a.CustomerID }).SingleOrDefault();
            if (custid != null)
            {
                data = (from a in db.SalesEntrys
                        join b in db.ReceiptBills on a.SalesEntryId equals b.InvoiceNo into rec
                        from b in rec.DefaultIfEmpty()
                        join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                        let salesreturn = (decimal?)(from d in db.SalesReturns
                                                     where d.SalesEntryId == a.SalesEntryId
                                                     select new
                                                     {
                                                         d.SRGrandTotal
                                                     }).Sum(o => o.SRGrandTotal) ?? 0
                        let payment = (decimal?)(from e in db.PaymentBills
                                                 join f in db.Payments on e.Payment equals f.PaymentId into reciept2

                                                 join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                 join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                 where h.SalesEntryId == a.SalesEntryId &&
                                                 e.BillType == "Sales Return"
                                                 select new
                                                 {
                                                     amt = e.Amount
                                                 }).Sum(o => o.amt) ?? 0
                        let paymentjrnl = (decimal?)(from e in db.JornalPaymentBills
                                                     join f in db.Journals on e.Jornal equals f.JournalId into reciept2

                                                     join g in db.SalesReturns on e.InvoiceNo equals g.SalesReturnId

                                                     join h in db.SalesEntrys on g.SalesEntryId equals h.SalesEntryId

                                                     where h.SalesEntryId == a.SalesEntryId &&
                                                     e.BillType == "Sales Return"
                                                     select new
                                                     {
                                                         amt = e.Amount
                                                     }).Sum(o => o.amt) ?? 0

                        where a.Customer == custid.CustomerID && (b != null || (c.SEPaidAmount != c.SEBillAmount))
                        && a.SalesEntryId == entry
                        select new
                        {
                            text = a.BillNo,
                            Date = a.SEDate,
                            Amount = (c.SEBillAmount - c.SEPaidAmount) - (((salesreturn == null) ? 0 : salesreturn) - ((payment == null) ? 0 : payment) - ((paymentjrnl == null) ? 0 : paymentjrnl)),
                            paid = c.SEPaidAmount,
                            id = a.SalesEntryId,
                            type = "Sales",
                            Balance = (c.SEBillAmount - c.SEPaidAmount) - (((salesreturn == null) ? 0 : salesreturn) - ((payment == null) ? 0 : payment) - ((paymentjrnl == null) ? 0 : paymentjrnl)),
                        }).FirstOrDefault();
            }
            var supplierid = (from a in db.Suppliers where a.Accounts == account select new { a.SupplierID }).SingleOrDefault();
            if (supplierid != null)
            {
                data = (from a in db.PurchaseReturns
                        join c in db.PRPayments on a.PurchaseReturnId equals c.PurchaseReturnId
                        join b in db.ReceiptBills on a.PurchaseReturnId equals b.InvoiceNo into rec
                        from b in rec.DefaultIfEmpty()
                        where a.Supplier == supplierid.SupplierID && (b == null || (c.PRBillAmount != c.PReturnAmount))
                        && a.PurchaseReturnId == entry
                        select new
                        {
                            text = a.BillNo,
                            Date = a.PRDate,
                            Amount = b != null ? b.Amount : (c.PRBillAmount - c.PReturnAmount),
                            paid = c.PReturnAmount,
                            id = a.PurchaseReturnId,
                            type = "Purchase Return",
                            Balance = (c.PRBillAmount - c.PReturnAmount),
                        }).FirstOrDefault();
            }
            return Json(data);
        }


        public JsonResult SearchAccountsByIdpaySelect(string q, string x, long account)
        {
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 10;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;


            object data = null;
            object data2 = null;
            object data3 = null;
            var suppid = (from a in db.Suppliers where a.Accounts == account select new { a.SupplierID }).SingleOrDefault();
            if (suppid != null)
            {
                var invs = (from a in db.PaymentBills
                            join b in db.PurchaseEntrys on a.InvoiceNo equals b.PurchaseEntryId
                            where b.Supplier == suppid.SupplierID &&
                            a.Amount == b.PEGrandTotal
                            select new
                            {
                                a.InvoiceNo
                            }
                     ).Select(o => o.InvoiceNo);
                var datasup = (from a in db.PurchaseEntrys
                               join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                               join b in db.PaymentBills on a.PurchaseEntryId equals b.InvoiceNo into rec
                               from b in rec.DefaultIfEmpty()
                               where a.Supplier == suppid.SupplierID && (b != null || (c.PEPaidAmount != c.PEBillAmount))
                                //&& (entry == null || a.SalesEntryId == entry)
                                && !invs.Contains(a.PurchaseEntryId)
                               let retamount = db.PurchaseReturns.Where(o => o.purchaseEntryId == a.PurchaseEntryId).Select(o => o.PRGrandTotal).FirstOrDefault()
                               select new
                               {
                                   text = a.BillNo,
                                   Date = a.PEDate,
                                   Amount = c.PEBillAmount,
                                   paid = c.PEPaidAmount,
                                   id = a.PurchaseEntryId,
                                   type = "Purchase",
                                   Balance = (c.PEBillAmount - c.PEPaidAmount) - ((retamount != null) ? retamount : 0),
                               }).Distinct().Where(b => b.Balance > 0 && b.text.Contains(q)).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                var datasup2 = (from a in db.Journals
                                join b in db.AccountsTransactions on new { f1 = a.JournalId, f2 = "Journal" } equals
                                new { f1 = b.reference, f2 = b.Purpose }
                                where b.Account == account && a.Ref1 != ""
                                let payedamount = (from xx in db.PaymentBills
                                                   where xx.InvoiceNo == a.JournalId && xx.BillType == "Journal Expense"
                                                   select new
                                                   {
                                                       xx.Amount
                                                   }
                                                  ).Select(o => o.Amount).Sum()
                                select new
                                {
                                    text = a.Ref1,
                                    Date = a.Date,
                                    Amount = b.Credit,
                                    paid = (payedamount != null) ? payedamount : 0,
                                    id = a.JournalId,
                                    type = "Journal Expense",
                                    Balance = (payedamount != null) ? (b.Credit - payedamount) : b.Credit,
                                }).Distinct().Where(b => b.Balance > 0 && b.text.Contains(q)).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();


                var full = datasup.Union(datasup2);
                return Json(full);

            }
            var custid = (from a in db.Customers where a.Accounts == account select new { a.CustomerID }).SingleOrDefault();
            if (custid != null)
            {
                data = (from a in db.SalesReturns
                        join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                        join b in db.PaymentBills on a.SalesReturnId equals b.InvoiceNo into rec
                        from b in rec.DefaultIfEmpty()
                        where a.Customer == custid.CustomerID && (b != null || (c.SRBillAmount != c.SReturnAmount))
                        //&& (entry == null || a.PurchaseReturnId == entry)
                        select new
                        {
                            text = a.BillNo,
                            Date = a.SRDate,
                            Amount = c.SRBillAmount,
                            paid = c.SReturnAmount,
                            id = a.SalesReturnId,
                            type = "Sales Return",
                            Balance = (c.SRBillAmount - c.SReturnAmount)
                        }).Distinct().Where(b => b.Balance > 0 && b.text.Contains(q)).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }

            return Json(data);
        }

        public JsonResult SearchAccountsByIdpay(long account, long entry)
        {
            object data = null;
            var suppid = (from a in db.Suppliers where a.Accounts == account select new { a.SupplierID }).SingleOrDefault();
            if (suppid != null)
            {
                data = (from a in db.PurchaseEntrys
                        join b in db.PaymentBills on a.PurchaseEntryId equals b.InvoiceNo into rec
                        from b in rec.DefaultIfEmpty()
                        join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                        where a.Supplier == suppid.SupplierID && (b != null || (c.PEPaidAmount != c.PEBillAmount))
                        && a.PurchaseEntryId == entry
                        select new
                        {
                            text = a.BillNo,
                            Date = a.PEDate,
                            Amount = b != null ? b.Amount : (c.PEBillAmount - c.PEPaidAmount),
                            paid = c.PEPaidAmount,
                            id = a.PurchaseEntryId,
                            type = "Purchase",
                            Balance = (c.PEBillAmount - c.PEPaidAmount),
                        }).FirstOrDefault();
                if (data == null)
                {
                    data = (from a in db.Journals
                            join b in db.AccountsTransactions on new { f1 = a.JournalId, f2 = "Journal" } equals
                            new { f1 = b.reference, f2 = b.Purpose }
                            where b.Account == account && a.Ref1 != ""
                            && a.JournalId == entry
                            let payedamount = (from xx in db.PaymentBills
                                               where xx.InvoiceNo == a.JournalId && xx.BillType == "Journal Expense"
                                               select new
                                               {
                                                   xx.Amount
                                               }
                                              ).Select(o => o.Amount).Sum()
                            select new
                            {
                                text = a.Ref1,
                                Date = a.Date,
                                Amount = b.Credit,
                                paid = (payedamount != null) ? payedamount : 0,
                                id = a.JournalId,
                                type = "Journal Expense",
                                Balance = (payedamount != null) ? (b.Credit - payedamount) : b.Credit,
                            }).FirstOrDefault();
                }
            }
            var custid = (from a in db.Customers where a.Accounts == account select new { a.CustomerID }).SingleOrDefault();
            if (custid != null)
            {
                data = (from a in db.SalesReturns
                        join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                        join b in db.PaymentBills on a.SalesReturnId equals b.InvoiceNo into rec
                        from b in rec.DefaultIfEmpty()
                        where a.Customer == custid.CustomerID && (b != null || (c.SRBillAmount != c.SReturnAmount))
                        && a.SalesReturnId == entry
                        select new
                        {
                            text = a.BillNo,
                            Date = a.SRDate,
                            Amount = b != null ? b.Amount : (c.SRBillAmount - c.SReturnAmount),
                            paid = c.SReturnAmount,
                            id = a.SalesReturnId,
                            type = "Sales Return",
                            Balance = (c.SRBillAmount - c.SReturnAmount),
                        }).FirstOrDefault();
            }
            return Json(data);
        }

        public JsonResult chkAccountType(long account)
        {
            object data = "";
            var suppid = (from a in db.Suppliers where a.Accounts == account select new { a.SupplierID }).SingleOrDefault();
            if (suppid != null)
            {
                data = "Supplier";
            }

            var custid = (from a in db.Customers where a.Accounts == account select new { a.CustomerID }).SingleOrDefault();
            if (custid != null)
            {
                data = "Customer";
            }
            return Json(data);
        }

       
        [QkAuthorize(Roles = "Dev,Update Cash Balance")]
        public ActionResult UpdateCashBalance()
        {
            AccountsViewModel model = new AccountsViewModel();
            Accounts Acc = db.Accountss.Find(3L);


            model.OpnBalance = Acc.OpnBalance;
            return View(model);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Update Cash Balance")]
        public ActionResult UpdateCashBalance(AccountsViewModel bnkview)
        {
            Accounts Acc = db.Accountss.Find(3L);

            Acc.OpnBalance = bnkview.OpnBalance;
            Acc.OpnBalanceCr = 0;

            editAccount(Acc, 3, false);
            bool delete = com.DeleteAllAccountTransaction("Opening Balance", bnkview.AccountsID);

            if (bnkview.OpnBalance > 0)
            {
                com.addAccountTrasaction(bnkview.OpnBalance, 0, bnkview.AccountsID, "Opening Balance", bnkview.AccountsID, DC.Debit);
            }
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Updated, UserId, "Accounts", "Accounts", findip(), bnkview.AccountsID, "Successfully Updated Cash-In-Hand");


            Success("Successfully Updated Cash-In-Hand.", true);
            return RedirectToAction("UpdateCashBalance", "Accounts");

        }

       

        
        [QkAuthorize(Roles = "Dev,Expense Account")]
        public ActionResult Tree()
        {
            TreeViewModel vmodel = new TreeViewModel();
            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList();
            var GroupList = Group.Select(a => new Tree
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = a.Parent,
                Type = "Group",
                AccountId = null
            }).ToList();
            var AccList = Acc.Select(a => new Tree
            {
                ID = null,
                text = a.Name,
                Parent = a.Group,
                Type = "Account",
                AccountId = a.AccountsID
            }).ToList();
            var List = GroupList.Union(AccList).ToList();
            vmodel.Menu = List;
            return PartialView(vmodel);
        }
        [HttpPost]
        public JsonResult GetTree()
        {
            //Accounts Group Tree Structuring 
            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList();
            var GroupList = Group.Select(a => new Tree
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = a.Parent,
                Type = "Group",
                AccountId = null
            }).ToList();
            var AccList = Acc.Select(a => new Tree
            {
                ID = null,
                text = a.Name,
                Parent = a.Group,
                Type = "Account",
                AccountId = a.AccountsID
            }).ToList();
            var List = GroupList.Union(AccList).ToList();
            var lst = GenerateTree(List, 0);
            //    //Defining type of data column gives proper data table 
            //    //Setting column names as Property names
            //        //inserting property values to datatable rows

            return Json(new { data = lst });
        }

       
       
        public ActionResult Treenew(long? acgroup)
        {
            TreeViewModel vmodel = new TreeViewModel();
            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList();
            var GroupList = Group.Where(o => o.AccountsGroupID == acgroup).Select(a => new Tree
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = a.Parent,
                Type = "Group",
                AccountId = null
            }).ToList();
            var AccList = Acc.Select(a => new Tree
            {
                ID = null,
                text = a.Name,
                Parent = a.Group,
                Type = "Account",
                AccountId = a.AccountsID
            }).ToList();
            var List = GroupList.Union(AccList).ToList();
            vmodel.Menu = List;
            ViewBag.acgroups = acgroup;
            return View(vmodel);
        }
        [HttpPost]
        public JsonResult GetTreenew(long groupid)
        {
            //Accounts Group Tree Structuring 
            var Group = db.AccountsGroups.Where(o => o.AccountsGroupID == groupid).ToList();
            var Acc = db.Accountss.ToList();
            var GroupList = Group.Select(a => new Tree
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = a.Parent,
                Type = "Group",
                AccountId = null
            }).ToList();
            var AccList = Acc.Select(a => new Tree
            {
                ID = null,
                text = a.Name,
                Parent = a.Group,
                Type = "Account",
                AccountId = a.AccountsID
            }).ToList();
            var List = GroupList.Union(AccList).ToList();
            //    //Defining type of data column gives proper data table 
            //    //Setting column names as Property names
            //        //inserting property values to datatable rows

            return Json(new { data = "" });
        }
        public static IList<TreeItem> GenerateTree(List<Tree> collection, long? parent)
        {
            IList<TreeItem> lst = new List<TreeItem>();
            foreach (Tree c in collection.Where(c => c.Parent == parent))
            {
                lst.Add(new TreeItem
                {
                    ID = c.ID,
                    text = c.text,
                    nodes = GenerateTree(collection, c.ID)
                });
            }
            return lst;
        }
        public static IList<TreeItem> GenerateTreenew(List<Tree> collection, long? parent,long? groupid)
        {
            IList<TreeItem> lst = new List<TreeItem>();
            foreach (Tree c in collection.Where(c => c.Parent == groupid))
            {
                lst.Add(new TreeItem
                {
                    ID = c.ID,
                    text = c.text,
                    //nodes = GenerateTreenew(collection, c.ID)
                });
            }
            return lst;
        }



        [HttpGet]
        public JsonResult GetEmailByIdByAccount(int AccId)
        {
            var Acct = db.Accountss.Where(a => a.AccountsID == AccId).FirstOrDefault();
            //customer
            if (Acct.Group == 12)
            {

                var email = (from a in db.Customers
                             join b in db.Contacts on a.Contact equals b.ContactID into cont
                             from b in cont.DefaultIfEmpty()
                             where a.Accounts == Acct.AccountsID
                             select new
                             {
                                 b.EmailId,
                             }).FirstOrDefault();
                return Json(email);

            }
            else if (Acct.Group == 14)//supplier
            {
                var email = (from a in db.Suppliers
                             join b in db.Contacts on a.Contact equals b.ContactID into cont
                             from b in cont.DefaultIfEmpty()
                             where a.Accounts == Acct.AccountsID
                             select new
                             {
                                 b.EmailId,
                             }).FirstOrDefault();
                return Json(email);

            }
            else
            {
                return Json(null);
            }

        }

        public JsonResult Searchdetails(string q, string x, long? cust, string constat)
        {
            var StockItemsPerm = User.IsInRole("List Stockable Items");

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
            var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 10;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;
            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                        equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                        from g in pur.DefaultIfEmpty()
                        join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                        equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                        from h in sale.DefaultIfEmpty()
                        where
                        (b.Status == Status.active)
                && (b.ItemName.ToLower().Contains(q.ToLower())
                 && (secnd == "" || b.ItemName.ToLower().Contains(secnd.ToLower()))
                && (third == "" || b.ItemName.ToLower().Contains(third.ToLower())))
              || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q) || ((b.PartNumber.ToLower().Contains(q.ToLower()) || b.PartNumber.Contains(q)) && PartNoCheck == Status.active)
                        select new
                        {
                            text = b.ItemCode + " - " + b.ItemName,
                            id = b.ItemID,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.MinStock,
                            b.KeepStock,
                            Tax = f.Percentage,
                            b.ItemUnitID,
                            g = h,
                            PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                            lastSale = (decimal?)h.ItemUnitPrice,
                            lastSaleU = (decimal?)h.ItemUnit,

                            lastPur = (decimal?)g.ItemUnitPrice,
                            lastPurU = (decimal?)g.ItemUnit,

                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                            b.Barcode,

                            OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,

                            PriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,
                            // unassemble -----
                            // main item
                            PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,


                        }).Distinct().OrderBy(b => b.ItemName).Skip(skip).Take(pageSize).AsEnumerable().Select(o => new
                        {
                            o.text,
                            o.id,
                            unit = o.ItemUnitID,
                            price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                            cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                            o.KeepStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.Tax,
                            lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
                            lastSaleU = o.lastSaleU,
                            lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
                            lastPurU = o.lastPurU,
                            o.g,

                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            o.Barcode,
                            o.SubUnitId,
                            PartNumber = o.PartNumber,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.ConFactor,

                            //o.SellingPrice,
                            //o.PurchasePrice,
                            //o.BasePrice,
                            //o.MRP,

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            cust = cust,
                        }).OrderBy(b => b.text).ToList();
            if (constat == "SalesEntry" && StockItemsPerm == true)
            {
                item = item.Where(a => a.KeepStock == false || (a.KeepStock == true && a.total > 0)).Skip(skip).Take(pageSize).ToList();
            }
            return Json(item);
        }

        public JsonResult SearchAllAccounts(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           where a.Status == Status.active && a.Group != 12 && a.Group != 14 && a.Group != 8 && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q))
                           select new SelectFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           where a.Group != 12 && a.Group != 14 && a.Group != 8
                           select new SelectFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                serialisedJson = hmt;

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        public JsonResult SearchAccountsPDC2(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat2> serialisedJson;
            string stt = "All";
            var supparentid = new SqlParameter("@parentid", 8);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var arr = supgpid.ToArray();


            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                                  from b in gp.DefaultIfEmpty()
                                  where a.Status == Status.active && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && arr.Contains(a.Group) && a.AccountsID != 4
                                  select new SelectFormat2
                                  {
                                      text = a.Name,
                                      id = a.Name,

                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                                  from b in gp.DefaultIfEmpty()
                                  where a.Status == Status.active && arr.Contains(a.Group) && a.AccountsID != 4
                                  select new SelectFormat2
                                  {
                                      text = a.Name,
                                      id = a.Name,

                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat2() { id = "", text = stt };
                serialisedJson.Insert(0, initial);
            }

            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat2() { id = "", text = "Select Bank Accounts" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }





        public JsonResult SearchAccountsPDC(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var supparentid = new SqlParameter("@parentid", 8);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var arr = supgpid.ToArray();


            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                                  from b in gp.DefaultIfEmpty()
                                  where a.Status == Status.active && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && arr.Contains(a.Group) && a.AccountsID != 4
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID,

                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                                  from b in gp.DefaultIfEmpty()
                                  where a.Status == Status.active && arr.Contains(a.Group) && a.AccountsID != 4
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID,

                                  }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Bank Accounts" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpGet]
        public JsonResult AccountCheck(string Account, long? Accid)
        {
            var Check = db.Accountss.Where(x => x.Name == Account).Any();
            var rslt = false;
            if (Check == true)
            {
                if (Accid != 0)
                {
                    var cust = db.Accountss.Where(x => x.Name == Account).FirstOrDefault();
                    Check = (cust.AccountsID == Accid) ? false : true;
                }
            }
            rslt = (Check) ? true : false;
            return Json(rslt);
        }


        public JsonResult SearchIncomeAccount(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            //sale account
            var sparentid = new SqlParameter("@parentid", 15);
            var sgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", sparentid).AsEnumerable().ToList();
            var sgpid = sgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            //Income (Direct/Opr.)
            var parentid = new SqlParameter("@parentid", 31);
            var groupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", parentid).AsEnumerable().ToList();
            var gpid = groupsdata.Select(a => a.AccountsGroupID).ToArray();
            var rparentid = new SqlParameter("@parentid", 7);
            var rgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", rparentid).AsEnumerable().ToList();
            var rgpid = rgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                                  from b in gp.DefaultIfEmpty()
                                  where a.Status == Status.active && a.AccountsID != 1 && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && (sgpid.Contains(a.Group) || gpid.Contains(a.Group) || rgpid.Contains(a.Group))
                                  select new SelectFormat
                                  {
                                      text = a.Name, //each json object will have
                                      id = a.AccountsID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                                  from b in gp.DefaultIfEmpty()
                                  where a.Status == Status.active && a.AccountsID != 1 && (sgpid.Contains(a.Group) || gpid.Contains(a.Group) || rgpid.Contains(a.Group))
                                  select new SelectFormat
                                  {

                                      text = a.Name, //each json object will have
                                      id = a.AccountsID
                                  }).OrderBy(b => b.text).ToList();
            }
            if (x == "default" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 1, text = "Sale" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public ActionResult BulkUpload()
        {
            var viewModel = new AccountsViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public JsonResult BulkUploadAccount(string[][] array)
        {
            bool stat = false;
            string msg = "";

            foreach (var arr in array)
            {
                var name = arr[0];
                var namchk = db.Accountss.Where(x => x.Name == name).Any();
                if (!namchk)
                {
                    DC DebCre = new DC();
                    DebCre = arr[7] == "Debit" ? DC.Debit : DC.Credit;
                    var group = db.AccountsGroups.Where(x => x.Name == name).Select(r => r.AccountsGroupID).FirstOrDefault();

                    var opbal = ((arr[2]) == "") ? 0 : Convert.ToDecimal(arr[2]);
                    Accounts Acc = new Accounts();
                    if (DebCre == DC.Debit)
                    {
                        Acc.OpnBalance = Convert.ToDecimal(arr[2]);
                        Acc.OpnBalanceCr = 0;
                    }
                    if (DebCre == DC.Credit)
                    {
                        Acc.OpnBalance = 0;
                        Acc.OpnBalanceCr = Convert.ToDecimal(arr[2]);
                    }

                    Acc.PrintName = arr[0];
                    Acc.Name = arr[0];
                    Acc.Alias = arr[1];
                    Acc.PrevBalance = 0;
                    Acc.Note = arr[6];
                    Acc.Status = arr[4] == "inactive" ? Status.active : Status.inactive;
                    Acc.Group = group;
                    Acc.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    Acc.Editable = 0;
                    Acc.TRN = arr[5];

                    db.Accountss.Add(Acc);
                    db.SaveChanges();

                    if (opbal > 0)
                    {
                        if (DebCre == DC.Debit)
                        {
                            com.addAccountTrasaction(opbal, 0, Acc.AccountsID, "Opening Balance", Acc.AccountsID, DC.Debit);

                        }
                        if (DebCre == DC.Credit)
                        {
                            com.addAccountTrasaction(0, opbal, Acc.AccountsID, "Opening Balance", Acc.AccountsID, DC.Credit);
                        }
                    }
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Accounts", "Accounts", findip(), Acc.AccountsID, "Account Created Successfully");
                    msg = "Account Created Successfully.";
                    stat = true;
                }

            }



            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        public virtual ActionResult DownloadExcel(string file)
        {
            string fullPath = "";
            string fileName = "";

            fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/Accounts.xlsx"));
            fileName = "Account.xlsx";
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
    }

}
