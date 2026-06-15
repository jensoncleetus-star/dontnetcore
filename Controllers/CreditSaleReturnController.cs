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
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Data.OleDb;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class CreditSaleReturnController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public CreditSaleReturnController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult downloadprint(long salesReturnId)
        {
            companySet();
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                //field mappping
                var fmapp = db.FieldMappings.Where(a => a.Section == "SReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                var saleRetData = com.SalesReturnData(salesReturnId, Status.active, Status.inactive, 500, ProjectCheck);
                var item = saleRetData["item"];
                var summary = saleRetData["summary"];
                var billsundry = saleRetData["billsundry"];

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            var def = 0;
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                bool stat = true;
            return Json(new  { status = stat, item, summary, billsundry, layout, fmapp } );

        }

            // GET: SalesReturn 
            [QkAuthorize(Roles = "Dev,Sales Return List")]
        public ActionResult Index()
        {
            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;
            ViewBag.SuperUserEditvalue = SuperUserEditvalue;

            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            companySet();
            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.SalesExecutive = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);
            ViewBag.Balance = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value=null},
                new SelectListItem() {Text = "Fully Paid", Value="0"},
                new SelectListItem() {Text = "Pending", Value="1"},
            }, "Value", "Text");
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            _FinancialYear();
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

            var MlaSReturn = db.EnableSettings.Where(a => a.EnableType == "MLASReturn").FirstOrDefault();
            var MlaSReturns = MlaSReturn != null ? MlaSReturn.Status : Status.inactive;
            ViewBag.MLASReturn = MlaSReturns;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");


            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindSReturn").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
               .Select(s => new
               {
                   ID = s.ProjectId,
                   Name = s.ProCode + " " + s.ProjectName
               })
               .ToList();
            ViewBag.getProj = QkSelect.List(proj.Take(1).ToList(), "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk.Take(1).ToList(), "ID", "Name");

            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Return List")]
        public ActionResult GetSalesReturn(string BillNo, string FromDate, string ToDate, long? customer, long? salesperson, long? type, string user, int? Balance, long? MC, string appstat, long? ProjectName, long? Task)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB"));
            }

            var cType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer :CustomerType.Card;//: CustomerType.Customer;//CustomerType.Customer;


            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var userpermission = User.IsInRole("All Sales Return Entry");
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ApprovalStatus AppSt = new ApprovalStatus();
            if (appstat != "")
            {
                if (appstat == "0")
                {
                    AppSt = ApprovalStatus.Approved;
                }
                else if (appstat == "1")
                {
                    AppSt = ApprovalStatus.Rejected;
                }
                else
                {
                    AppSt = ApprovalStatus.PendingApproval;
                }
            };

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uSalesView = User.IsInRole("View Sales Return");
            var uEdit = User.IsInRole("Edit Sales Return");
            var uDownload = User.IsInRole("Download Sales Return");
            var uDelete = User.IsInRole("Delete Sales Return");
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.srdays).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
                editableDay = today.AddYears(-10);
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddMinutes(-userEditDays);
            }
            // dc.Configuration
            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets). Split SERVER from CLIENT: materialize only entity columns +
            // simple scalars (left-joined entity access stays server-side) into serverRows, then build client
            // lookups keyed by SalesReturnId and re-project client-side with the SAME member names + order.
            var serverQuery = (from a in db.SalesReturns
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                    join kk in db.SalesEntrys on a.SalesEntryId equals kk.SalesEntryId into crsales
                    from kk in crsales.DefaultIfEmpty()
                     join d in db.Employees on a.SRCashier equals d.EmployeeId into useremp
                     from d in useremp.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     join i in db.MCs on a.MaterialCenter equals i.MCId into mcs
                     from i in mcs.DefaultIfEmpty()
                     join j in db.Projects on a.Project equals j.ProjectId into prj
                     from j in prj.DefaultIfEmpty()
                     join k in db.ProTasks on a.ProTask equals k.ProTaskId into task
                     from k in task.DefaultIfEmpty()
                         //  let mc = db.MCs.Where(x => x.AssignedUser == a.CreatedBy).Select(x => x.MCId).FirstOrDefault()
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.

                     where (BillNo == "" || a.BillNo == BillNo) &&
                     a.Ref5 != "Credit Note" &&
                     (customer == 0 || customer == null || a.Customer == customer) &&
                     (salesperson == 0 || salesperson == null || a.SRCashier == salesperson) &&
                     (type == null || a.CustomerType == cType) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                     (ToDate == "" || ToDate == null || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0)
                     && (user == null || user == "" || e.Id == user)
                      // ((Balance == null) || (Balance == 1 ? (((decimal?)a.SRGrandTotal ?? 0) > ((decimal?)c.SReturnAmount)) : (((decimal?)a.SRGrandTotal ?? 0) == ((decimal?)c.SReturnAmount))))
                      //&& (mc == 0 || mc == a.MaterialCenter)
                      && (MC == null || a.MaterialCenter == MC)
                     //&& ((MCArray.Contains(a.MaterialCenter) && MC == a.MaterialCenter) || ((MC == null) && MCArray.Contains(a.MaterialCenter)))
                     && (userpermission == true || a.CreatedBy == UserId)
                     && (ProjectName == 0 || ProjectName == null || j.ProjectId == ProjectName)
                     && (Task == 0 || Task == null || k.ProTaskId == Task)

                     select new
                     {
                         validornot = tem != 1 && (EF.Functions.DateDiffMinute(a.SRCreatedDate, editableDay) <= 0 && EF.Functions.DateDiffMinute(a.SRCreatedDate, today) >= 0) ? "valid" : "invalid",
                         userEditDays = userEditDays,
                         a.SalesReturnId,
                         BillNo = a.BillNo,
                         SalesEntryId = a.SalesEntryId,
                         SRDate = a.SRDate,
                         SRGrandTotal = a.SRGrandTotal,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         EmpName = d.FirstName + " " + d.LastName,
                         User = e.UserName,
                         CustomerType = a.CustomerType,
                         PayType = a.PayType,
                         PaymentStatus = 0,//S (long?)c.Status,
                         PaymentTrans = 0,// db.SETransactions.Any(k => k.SalesEntry == a.SalesEntryId),
                         SReturnAmount = 0,// c.SReturnAmount,
                         BalanceAmt = a.SRGrandTotal - 0,
                         Remarks = a.Remarks,
                         Dev = uDev,
                         Details = uSalesView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         MC = i.MCName,
                         ProjectName = (j.ProjectName != null && j.ProjectName != "") ? j.ProCode + "-" + j.ProjectName : "",
                         Task = (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",
                         CreatedDate = a.SRCreatedDate,
                         SalesInvoice = kk.BillNo
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BalanceAmt","BillNo","CreatedDate","Customer","CustomerType","Delete","Details","Dev","Download","Edit","EmpName","MC","PaymentStatus","PaymentTrans","PayType","ProjectName","Remarks","SalesEntryId","SalesInvoice","SalesReturnId","SRDate","SReturnAmount","SRGrandTotal","Task","User","userEditDays","validornot" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("SalesReturnId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by SalesReturnId (missing key -> empty/absent, no KeyNotFound).
            var srIds = serverRows.Select(o => o.SalesReturnId).ToList();
            // app = approver EmployeeIds for the sales return (nested collection, keyed by TransEntry == SalesReturnId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "SalesReturn" && srIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "SalesReturn" && srIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per sales return.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.SalesReturnId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.SalesReturnId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.SalesReturnId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.validornot,
                         o.userEditDays,
                         o.SalesReturnId,
                         o.BillNo ,
                         o.SalesEntryId ,
                         SalesInvoice = o.SalesInvoice,


                         o.SRDate ,
                         o.SRGrandTotal ,
                         o.Customer ,
                         o.EmpName ,
                         o.User ,
                         o.CustomerType ,
                         o.PayType ,
                         o.PaymentStatus,
                         o.PaymentTrans ,
                         o.SReturnAmount ,
                         o.BalanceAmt ,
                         o.Remarks,
                         o.Dev,
                         o.Details,
                         Edit = uEdit,
                         o.Download,
                         o.Delete ,
                         o.MC,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0  ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.ProjectName,
                         o.Task,
                         o.CreatedDate

                     };
                     });

            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt && a.app.Count() > 0);
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p =>p.SalesInvoice!=null &&( p.BillNo.ToString().ToLower().Equals(search.ToLower())||
                                 p.SalesInvoice.ToString().ToLower().Equals(search.ToLower()) 
                                 //// p.CreditPeriod.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.SReturnAmount.ToString().ToLower().Contains(search.ToLower())
                                 ////p.SEBalanceAmount.ToString().ToLower().Contains(search.ToLower())
                                 ));
            }

            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            else
            {
                v = v.OrderByDescending(b => Convert.ToInt64(b.SalesReturnId));
            }

            if (!fastPage) { recordsTotal = v.Count(); }
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Sales Return Entry")]
        public ActionResult Create()
        {

            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;
            ViewBag.SuperUserEditvalue = SuperUserEditvalue;
            var EnableRackWise = db.EnableSettings.Where(a => a.EnableType == "RackWiseStock").FirstOrDefault();
            var RackCheck = EnableRackWise != null ? EnableRackWise.Status : Status.inactive;
            ViewBag.RackWiseStock = RackCheck;

            var ref1 = db.SalesReturns
            .Select(s => new
            {
                ID = s.Ref1,
                Name = s.Ref1
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.SalesReturns
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.SalesReturns
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.SalesReturns
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.SalesReturns
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");

            var use = db.Employees
                            .Select(s => new
                            {
                                ID = s.EmployeeId,
                                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                            })
                            .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var salesentry = new SalesReturnViewModel
            {
                BillNo = InvoiceNo(),
                SRDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                SRNote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "sreturn").Select(a => a.TermsCondit).FirstOrDefault(),
                SalesTypes = db.SalesTypes.ToList(),
            };
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName

                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();

                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                ViewBag.MC = QkSelect.List(serialisedJson, "id", "text");
                ViewBag.LastMc = serialisedJson.Select(a => a.id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

            companySet();
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            var userpermission = User.IsInRole("All Sales Return Entry");

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.LastEntry = db.SalesReturns.Where(p => (MCArray.Contains(p.MaterialCenter)) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.SalesReturnId).AsEnumerable().DefaultIfEmpty(0).Max();
            _FinancialYear();

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
               .Select(s => new
               {
                   ID = s.ProjectId,
                   Name = s.ProCode + " " + s.ProjectName
               })
               .ToList();
            ViewBag.getProj =QkSelect.List(proj.Take(1).ToList(), "ID", "Name");

            var tsk = db.ProTasks
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            })
            .ToList();
            ViewBag.getProTask = QkSelect.List(tsk.Take(1).ToList(), "ID", "Name");

            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
               .Select(s => new
               {
                   ID = s.EmployeeId,
                   Name = s.FirstName + " " + s.LastName
               })
               .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaSReturn = db.EnableSettings.Where(a => a.EnableType == "MLASReturn").FirstOrDefault();
            var MlaSReturns = MlaSReturn != null ? MlaSReturn.Status : Status.inactive;
            ViewBag.MLASReturn = MlaSReturns;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
                var UserIdd = User.Identity.GetUserId();
            var empid = (from a in db.Users
                         join b in db.Employees on a.Id equals b.UserId
                         where a.Id == UserIdd
                         select new
                         {
                             b.EmployeeId
                         }).FirstOrDefault();
            if (empid != null)
            {
                salesentry.SRCashier = empid.EmployeeId;
            }
            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;








































            //field mapping
            salesentry.FieldMap = db.FieldMappings.Where(a => a.Section == "SReturn" && a.Status == Status.active).ToList();

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            ViewBag.PrintLayout = PriLay;
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();
            ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", salesentry);
            }
            else
            {
                return View(salesentry);
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Return Entry")]
        public JsonResult CreateSalesReturn(string[][] array, string[] saledata, SRBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel, ICollection<RackStockPViewModel> bsrackData)
        {
            bool stat = false;
            string msg;



            var userpermission = User.IsInRole("All Sales Return Entry");
            var UserId = User.Identity.GetUserId();
               var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.srdays).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
                editableDay = today.AddYears(-10);
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddMinutes(-userEditDays);
            }
            var idd = Convert.ToInt64(saledata[16]);
            SalesEntry sr = db.SalesEntrys.Where(o => o.SalesEntryId == idd).FirstOrDefault();
            if (sr != null)
            {
                if ((sr.SEDate - editableDay).TotalMinutes < 0 || tem == 1)
                {
                    msg = "Edit Days Over";
                    stat = false;
                 //   return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }

            }










            if (!BillExist(Convert.ToString(saledata[18])))
            {
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

             UserId = User.Identity.GetUserId();
                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                long MC = 0;
                Int64 saleRAcc = (long)db.companys.Select(a => a.SReturnAccount).FirstOrDefault();
                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(saledata[22]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                if (saledata[21] != "")
                {
                    if (MCcheck == Status.active)
                    {

                        MC = Convert.ToInt64(saledata[21]);
                    }
                    else
                    {
                        MC = 1;
                    }
                }
                else
                {
                    MC = 1;
                }
                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                string action = saledata[15];
                Int64 saleEntryId = Convert.ToInt64(saledata[16]);
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var TaxAmount = Convert.ToDecimal(saledata[5]);
                var GrandTotal = Convert.ToDecimal(saledata[7]);
                var saleramount = GrandTotal - TaxAmount;
                var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                today = Convert.ToDateTime(System.DateTime.Now);
                var subtotal = Convert.ToDecimal(saledata[8]);
                
                SalesReturn SRentry = new SalesReturn();

                SRentry.SRNo = GetSRNo();
                SRentry.BillNo = Convert.ToString(saledata[18]);
                SRentry.SalesEntryId = Convert.ToInt64(saledata[16]);
                SRentry.SRDate = date;
                SRentry.SRCashier = saledata[1] != "" ? Convert.ToInt64(saledata[1]) : 0;
                if (saledata[27] != null)
                {
                    string str = saledata[27];
                    SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                    SRentry.SaleType = Stype;
                }
                else
                {
                    SRentry.SaleType = SaleType.Sale;
                }

                //walkin customer
                SRentry.CustomerType = (saledata[12] == "1")? CustomerType.Walking : CustomerType.Customer;

                SRentry.Customer = Convert.ToInt64(saledata[0]);
                SRentry.ReturnType = (saledata[17] == "1")?ReturnType.Direct : ReturnType.AgainstBill;
                //pay type for pos
                SRentry.PayType = "";//need change
                SRentry.SRItems = Convert.ToInt32(saledata[3]);
                SRentry.SRItemQuantity = Convert.ToDecimal(saledata[4]);
                SRentry.SRSubTotal = Convert.ToDecimal(saledata[8]);
                SRentry.SRTax = Convert.ToDecimal(saledata[9]);
                SRentry.SRTaxAmount = TaxAmount;
                SRentry.SRDiscount = Convert.ToDecimal(saledata[6]);
                SRentry.SRGrandTotal = GrandTotal;
                SRentry.SRNote = saledata[11];
                SRentry.Print = 1;
                SRentry.SRCreatedDate = today;
                SRentry.CreatedBy = UserId;
                SRentry.Status = 1;
                SRentry.Branch = Branch;
                SRentry.Remarks = saledata[20];
                SRentry.MaterialCenter = MC;
                SRentry.SalesType = Convert.ToInt64(saledata[23]);
                SRentry.Project = saledata[24] != "" ? Convert.ToInt64(saledata[24]) : 0;
                SRentry.ProTask = saledata[25] != "" ? Convert.ToInt64(saledata[25]) : 0;


                SRentry.Ref1 = Convert.ToString(saledata[28]);
                SRentry.Ref2 = Convert.ToString(saledata[29]);
                SRentry.Ref3 = Convert.ToString(saledata[30]);
                SRentry.Ref4 = Convert.ToString(saledata[31]);
                SRentry.Ref5 = Convert.ToString(saledata[32]);
                if (SRentry.Project != null && SRentry.Project != 0)
                {
                    SRentry.SReturnAccount = db.Projects.Where(a => a.ProjectId == SRentry.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                }
                else
                {
                    SRentry.SReturnAccount = saleRAcc;
                }

                db.SalesReturns.Add(SRentry);
                db.SaveChanges();
                Int64 salesRetId = SRentry.SalesReturnId;

                //To Update the quantity in Create Mode(ItemTransaction Table)
                com.ItemTransInCreateMode("SalesReturn", MC, 0, 0, array, UserId, today);

                ////// add to SEItem

                string result = string.Empty;
                DataTable dtItem = new DataTable();
                dtItem.Columns.Add("ItemUnit");
                dtItem.Columns.Add("ItemUnitPrice");
                dtItem.Columns.Add("ItemQuantity");
                dtItem.Columns.Add("ItemSubTotal");
                dtItem.Columns.Add("ItemDiscount");
                dtItem.Columns.Add("ItemTax");
                dtItem.Columns.Add("ItemTaxAmount");
                dtItem.Columns.Add("ItemTotalAmount");
                dtItem.Columns.Add("itemNote");
                dtItem.Columns.Add("SalesReturnId");
                dtItem.Columns.Add("Item");


                foreach (var arr in array)
                {
                    DataRow dr = dtItem.NewRow();
                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                    dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                    dr["ItemTax"] = arr[10] == null ? 0 : Convert.ToDecimal(arr[10]);
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                    dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                    dr["SalesReturnId"] = salesRetId;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    dtItem.Rows.Add(dr);


                    var item = Convert.ToInt32(arr[0]);
                    var chkbundle = db.ItemBundles.Where(a => a.mainItem == item).Select(a => a.ItemBundleId).FirstOrDefault();
                    if (chkbundle > 0)
                    {
                        var bunQuan = Convert.ToDecimal(arr[2]);
                        var itemBundle = (from g in db.ItemBundles
                                          join b in db.Items on g.mainItem equals b.ItemID
                                          where b.ItemID == item
                                          select new
                                          {
                                              g.ItemBundleId
                                          }).FirstOrDefault();
                        var bundle = (from a in db.BundleItems
                                      join b in db.Items on a.ItemId equals b.ItemID
                                      join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                      from c in primary.DefaultIfEmpty()
                                      where a.ItemBundle == itemBundle.ItemBundleId
                                      select new
                                      {
                                          b.ItemCode,
                                          b.ItemName,
                                          c.ItemUnitName,
                                          ItemUnitPrice = a.ItemUnitPrice,
                                          quantity = a.ItemQuantity,
                                          ItemSubTotal = a.ItemSubTotal,
                                          ItemTax = a.ItemTax,
                                          ItemTaxAmount = a.ItemTaxAmount,
                                          ItemTotalAmount = a.ItemTotalAmount,
                                          ItemUnit = a.ItemUnit,
                                          Item = a.ItemId
                                      }).ToList();
                        foreach (var bu in bundle)
                        {
                            var qua = (bunQuan * bu.quantity);
                            var ItemSubTotal = qua * bu.ItemUnitPrice;
                            var buTaxAmount = (ItemSubTotal * bu.ItemTax) / 100;

                            decimal itemtax = 0;
                            decimal taxamt = 0;
                            decimal totamt = 0;

                            itemtax = bu.ItemTax;
                            taxamt = buTaxAmount;
                            totamt = (buTaxAmount + ItemSubTotal);

                            DataRow dbu = dtItem.NewRow();
                            dbu["ItemUnit"] = bu.ItemUnit;
                            dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                            dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                            dbu["ItemSubTotal"] = ItemSubTotal;
                            // add parent itemid in discount for reference
                            dbu["ItemDiscount"] = item;
                            dbu["ItemTax"] = itemtax;
                            dbu["ItemTaxAmount"] = taxamt;
                            dbu["ItemTotalAmount"] = totamt;
                            dbu["itemNote"] = "-:{Bundle_Item}";
                            dbu["SalesReturnId"] = salesRetId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }
                }

                ////// create parameter 
                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypeSRItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertSRItems", "@TableType");
                //// execute sql 
                db.Database.ExecuteSqlRaw(sql, parameter);

                // batch stock
                if (bstmodel != null)
                {
                    foreach (var bst in bstmodel)
                    {
                        if (bst.BatchNo != "" && bst.BatchNo != null)
                        {
                            DateTime? exp = null;
                            DateTime? mfg = null;
                            if (bst.EXP != null && bst.EXP != "")
                            {
                                exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                            }
                            if (bst.MFG != null && bst.MFG != "")
                            {
                                mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                            }
                            decimal bStock = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStock = bst.StockIn * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockOut = 0;
                            Btst.StockIn = bStock;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = salesRetId;
                            Btst.Type = "SaleReturn";

                            Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            Btst.Date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                //billsundry
                if (bsrackData != null)
                {
                    foreach (var bst in bsrackData)
                    {
                        if (bst.StockOut != 0)
                        {

                            decimal bStockIn = 0;

                            shelfstockmovement Btst = new shelfstockmovement();
                            Btst.purpose = "Sales Return";
                            Btst.itemid = bst.Item;
                            Btst.unitid = (long)bst.Unit;
                            Btst.rackmciid = (long)com.getrackmcid(MC, bst.RackNo, bst.ShelfNo);
                            Btst.qty = bst.StockOut;

                            Btst.referenceid = salesRetId ;


                            Btst.createddate = DateTime.Now;
                            Btst.createdby = UserId;

                            db.shelfstockmovements.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                if (bsmodel.srbsundrys != null)
                {
                    string bsResult = string.Empty;


                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("SalesReturnId");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.srbsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["SalesReturnId"] = salesRetId;
                        drw["BillSundry"] = bs.BillSundry;
                        drw["BsValue"] = bs.BsValue;
                        drw["AmountType"] = bs.AmountType;
                        drw["BsType"] = bs.BsType;
                        drw["BsAmount"] = bs.BsAmount;

                        BsEntry.Rows.Add(drw);
                    }

                    ////// create parameter 
                    SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                    parameter1.SqlDbType = SqlDbType.Structured;
                    parameter1.TypeName = "TableTypeSRBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertSRBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);
                }

                //SEPayment
                SRPayment SRpay = new SRPayment();

                SRpay.CustomerId = Convert.ToInt64(saledata[0]);
                SRpay.SRDate = date;
                SRpay.SREntryDate = today;
                SRpay.SRBillAmount = GrandTotal;

               
                if (saledata[12] == "1")
                {
                    SRpay.SReturnAmount = GrandTotal;
                }
                else
                {
                    SRpay.SReturnAmount = Convert.ToDecimal(saledata[10]);
                }

                SRpay.CreatedBranch = Convert.ToInt32(BranchID);
                SRpay.CreatedUserId = UserId;
                SRpay.SRCreatedDate = today;
                SRpay.Status = 1;
                SRpay.SalesReturnId = salesRetId;
                db.SRPayments.Add(SRpay);
                db.SaveChanges();

                decimal amount = Convert.ToDecimal(saledata[10]);
                Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == SRentry.Customer).Select(a => a.Accounts).FirstOrDefault();
                Int64 saleAccId = saleRAcc;//db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).FirstOrDefault();
                if (SRentry.Project != null && SRentry.Project != 0)
                {
                    saleAccId = db.Projects.Where(a => a.ProjectId == SRentry.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                }

                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();

                //walkin customer
                if (saledata[12] == "1")
                {
                    //AccountsTransaction
                    amount = GrandTotal;
                }
                if (Convert.ToDecimal(saledata[10]) > 0 || saledata[12] == "1")
                {

                    var Remark = "Direct Payment From SalesReturn";
                    long payid;
                    //SETransaction
                    SRTransaction SRtran = new SRTransaction();

                    SRtran.CustomerId = Convert.ToInt64(saledata[0]);
                    SRtran.SRPayDate = date;
                    //walkin customer
                    if (saledata[12] == "1")
                    {
                        amount = GrandTotal;
                        SRtran.SRPayAmount = amount;
                    }
                    else
                    {
                        amount = Convert.ToDecimal(saledata[10]);
                        SRtran.SRPayAmount = amount;
                    }
                    payid = com.addPayment(date, cashAccId, custAccID, amount, amount, amount, Remark, UserId, BranchID, salesRetId, "SalesReturn");
                    SRtran.PaymentId = payid;
                    SRtran.SRCreatedDate = today;
                    SRtran.CreatedBranch = Convert.ToInt32(BranchID);
                    SRtran.CreatedUserId = UserId;
                    SRtran.Status = 1;
                    SRtran.SalesReturnId = salesRetId;

                    db.SRTransactions.Add(SRtran);
                    db.SaveChanges();
                }


                //bill sundry account
                var Gtotal = GrandTotal;
                if (bsmodel.srbsundrys != null)
                {
                    foreach (var bs in bsmodel.srbsundrys)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                        {
                            if (bs.BsAmount == null)
                                bs.BsAmount = 0;
                            var bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                saleramount = saleramount - bsamount;
                                com.addAccountTrasaction((decimal)bs.BsAmount, 0, (long)ChkAcc.SAccount, "Sale Return", salesRetId, DC.Debit, date, null, null, SRentry.Project, SRentry.ProTask);
                            }
                            else //substract
                            {
                                saleramount = saleramount + bsamount;
                                com.addAccountTrasaction(0, (decimal)bs.BsAmount, (long)ChkAcc.SAccount, "Sale Return", salesRetId, DC.Credit, date, null, null, SRentry.Project, SRentry.ProTask);
                            }
                        }
                    }
                }

                //add trasaction to sale account 
                com.addAccountTrasaction(saleramount, 0, saleAccId, "Sale Return", salesRetId, DC.Debit, date, null, null, SRentry.Project, SRentry.ProTask);
                //    //add sale trasaction 
                    com.addAccountTrasaction(0, GrandTotal, custAccID, "Sale Return", salesRetId, DC.Credit, date, null, null, SRentry.Project, SRentry.ProTask);
                
                // add vat input in account transaction
                if (TaxAmount > 0 && Convert.ToInt64(saledata[23])!=3)
                    com.addAccountTrasaction(TaxAmount, 0, VATOutput, "Sale Return", salesRetId, DC.Debit, date, null, null, SRentry.Project, SRentry.ProTask);
                if (Convert.ToDecimal(saledata[10]) > 0 || saledata[12] == "1")
                {
                    //if payment
                    com.addAccountTrasaction(amount, 0, custAccID, "Sale Return Payment", salesRetId, DC.Debit, date, null, null, SRentry.Project, SRentry.ProTask);
                    com.addAccountTrasaction(0, amount, cashAccId, "Sale Return Payment", salesRetId, DC.Credit, date, null, null, SRentry.Project, SRentry.ProTask);
                }
                com.addlog(LogTypes.Created, UserId, "SalesReturn", "SalesReturns", findip(), salesRetId, "Successfully Submitted Sales Return");

                //Stock Section
                //Approved By
                var Appby = Convert.ToString(saledata[26]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = salesRetId;
                        approval.Type = "SalesReturn";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }
                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                if (action == "print")
                {
                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    var saleRetData = com.SalesReturnData(salesRetId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck);
                    var item = saleRetData["item"];
                    var summary = saleRetData["summary"];
                    var billsundry = saleRetData["billsundry"];

                    var fmapp = db.FieldMappings.Where(a => a.Section == "SReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay==Status.active)?Convert.ToInt64(saledata[33]):Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout , fmapp } };
                }
                else if (action == "sendmail")
                {
                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = saledata[19];
                    string CcMail = "";
                    string InvoiceNo = "_SalesReturn_" + SRentry.BillNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "SalesReturn").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "Sales Return";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our sales return for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(salesRetId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully submitted Sales Return Entry.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Successfully submitted Sales Return Entry.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }


        }
        [HttpGet]
      
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            SalesReturnViewModel vmodel = new SalesReturnViewModel();

            vmodel = (from b in db.SalesReturns
                      join c in db.SRPayments on b.SalesReturnId equals c.SalesReturnId into pay
                      from c in pay.DefaultIfEmpty()
                      join y in db.SalesEntrys on b.SalesEntryId equals y.SalesEntryId into yyy
                      from y in yyy.DefaultIfEmpty()
                      join d in db.Employees on b.SRCashier equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join f in db.Customers on b.Customer equals f.CustomerID into cust
                      from f in cust.DefaultIfEmpty()
                      join g in db.MCs on b.MaterialCenter equals g.MCId into mcs
                      from g in mcs.DefaultIfEmpty()
                      join t in db.SalesTypes on b.SalesType equals t.Id into stype
                      from t in stype.DefaultIfEmpty()
                      join x in db.Contacts on f.Contact equals x.ContactID into cnt
                      from x in cnt.DefaultIfEmpty()
                      where b.SalesReturnId == id
                      select new SalesReturnViewModel
                      {
                          CustomerName = f.CustomerCode + " - " + f.CustomerName,
                          SalesEntryId = b.SalesEntryId,
                          BillNo = b.BillNo,
                          SRDate = b.SRDate,
                          SRNote = b.SRNote.Replace("\n", "<br />"),
                          EmployeeName = d.FirstName + " " + d.LastName,
                          CustomerType = b.CustomerType,
                          SRDiscount = b.SRDiscount,
                          SRTotal = b.SRDiscount + b.SRGrandTotal,
                          SRGrandTotal = b.SRGrandTotal,
                          SReturnAmount = b.CustomerType == 0 ? c.SReturnAmount : b.SRGrandTotal,
                          SRDueAmount = b.CustomerType == 0 ? b.SRGrandTotal - c.SReturnAmount : 0,
                          PayType = (b.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          MCName = g.MCName,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                          SaleTypeName = (b.SaleType == SaleType.Sale) ? "Sale" : ((b.SaleType == SaleType.Hire) ? "Hire" : "POS"),
                          SalesTypeName = t.Name,
                          EmailId = x.EmailId,

                          ReturnTypeName = (b.ReturnType == ReturnType.AgainstBill) ? "AgainstBill :-"+y.BillNo :"Direct",
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "SalesReturn"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();

            vmodel.SRItemss = db.SRItemss.Where(a => a.SalesReturnId == id && a.itemNote != "-:{Bundle_Item}")
            .Select(b => new SRItemViewModel
            {
                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemTax = b.ItemTax,
                itemNote = b.itemNote,
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.SRItemss
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.SalesReturnId == id && ab.itemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();
            vmodel.SRbs = db.SRBillSundrys.Where(a => a.SalesReturnId == id)
       .Select(b => new SRBillSundryViewModel
       {
           AmountType = b.AmountType,
           BsAmount = b.BsAmount,
           BsType = b.BsType,
           BsValue = b.BsValue,
           Type = b.BsType == 0 ? "Add" : "Less",
           AmtType = b.AmountType == 0 ? "" : "%",
           BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
       }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "SReturn" && a.Status == Status.active).ToList();

            return View(vmodel);
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit Sales Return")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var EnableRackWise = db.EnableSettings.Where(a => a.EnableType == "RackWiseStock").FirstOrDefault();
            var RackCheck = EnableRackWise != null ? EnableRackWise.Status : Status.inactive;
            ViewBag.RackWiseStock = RackCheck;
            var userpermission = User.IsInRole("All Sales Return Entry");
            var UserId = User.Identity.GetUserId();

            SalesReturn Salertn = db.SalesReturns.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesReturnId == id).FirstOrDefault();
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.srdays).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            if (userEditDays == 0 && userEdit != 0)
            {
                editableDay = today.AddYears(-10);
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddMinutes(-userEditDays);
            }
            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;

            if (SuperUserEditvalue == Status.active)
            {
                DateTime dt = System.DateTime.Now;
                var f = db.otpapproves.Where(o => o.entryid == id && o.purpose == "SalesReturn" && o.requestedby == UserId && o.expdate > dt && o.approvedby == UserId).FirstOrDefault();
                if (f != null)
                {
                    editableDay = editableDay.AddDays(-1000);
                }
            }
            if ((Salertn.SRCreatedDate - editableDay).TotalMinutes < 0 || tem == 1)
            {
                return NotFound();

            }


            if (Salertn == null)
            {
                return NotFound();
            }
            Int64 cashier = Convert.ToInt64(Salertn.SRCashier);
            Int64 customer = Salertn.Customer;
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.SalesReturns
                             on a.TransactionID equals b.SalesReturnId
                             where a.TransactionID == id && a.TransactionType == "Sales Return"
                             select new SalesRtnDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 SalesRtnId = a.TransactionID,
                                 FileName = a.FileName,
                                 CreatedDate = a.CreatedDate
                             }).ToList();

            string custname = "";
            string mobile = "";
            SalesReturnViewModel vmodel = new SalesReturnViewModel();

            custname = db.Customers.Where(a => a.CustomerID == customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();


            var cust = db.Customers
                .Select(s => new
                {
                    CustomerID = s.CustomerID,
                    CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            
            var use = db.Employees
                              .Select(s => new
                              {
                                  ID = s.EmployeeId,
                                  Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                              })
                              .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var saleentry = db.SalesEntrys.Where(a=>a.SalesEntryId== Salertn.SalesEntryId||Salertn.SalesEntryId==null)
                           .Select(s => new
                           {
                               ID = s.SalesEntryId,
                               Name = s.BillNo
                           }).Take(5)
                           .ToList();
            ViewBag.saleentry = QkSelect.List(saleentry, "ID", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            //        Id = s.MCId,
            //        Name = s.MCName
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");


            var proj = db.Projects
             .Select(s => new
             {
                 ID = s.ProjectId,
                 Name = s.ProCode + " " + s.ProjectName
             })
             .ToList();
            ViewBag.getProj =QkSelect.List(proj.Take(1).ToList(), "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk.Take(1).ToList(), "ID", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesReturn").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaSReturn = db.EnableSettings.Where(a => a.EnableType == "MLASReturn").FirstOrDefault();
            var MlaSReturns = MlaSReturn != null ? MlaSReturn.Status : Status.inactive;
            ViewBag.MLASReturn = MlaSReturns;

            vmodel = (from b in db.SalesReturns
                      join c in db.SRPayments on b.SalesReturnId equals c.SalesReturnId into srp
                      from c in srp.DefaultIfEmpty()
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      where b.SalesReturnId == id
                      select new SalesReturnViewModel
                      {
                          SRDate = b.SRDate,
                          BillNo = b.BillNo,
                          SalesEntryId = b.SalesEntryId,
                          SRCashier = b.SRCashier,
                          Customer = b.Customer,
                          SRDiscount = b.SRDiscount,
                          SRGrandTotal = b.SRGrandTotal,
                          // SReturnAmount = c.SReturnAmount,
                          CustomerType = b.CustomerType,
                          ReturnType = b.ReturnType,
                          // SRDueAmount = b.SRGrandTotal - c.SReturnAmount,
                          CustomerName = custname,
                          MobileNo = mobile,
                          SRNote = b.SRNote,
                          custEmailId = e.EmailId,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          SaleType=b.SaleType,
                          SalesType=b.SalesType,
                          SalesTypes= db.SalesTypes.ToList(),
                          Project=b.Project,
                          ProTask = b.ProTask,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();

            companySet();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            ViewBag.preEntry = db.SalesReturns.Where(a => a.SalesReturnId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesReturnId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.SalesReturns.Where(a => a.SalesReturnId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.SalesReturnId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;
            _FinancialYear();

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            ViewBag.PopUpAddCust = false;

            var EditPermission = User.IsInRole("Disable SReturn Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "SalesReturn", UserId);
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "SReturn" && a.Status == Status.active).ToList();
            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();


            //dummy table operations
            var DItem = db.DummySRItems.Where(a => a.SalesReturnId == id).FirstOrDefault();
            var SItem = db.SRItemss.Where(a => a.SalesReturnId == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummySRItems.Where(a => a.SalesReturnId == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    SRItems sItem = new SRItems();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.itemNote = arr.itemNote;
                    sItem.SalesReturnId = arr.SalesReturnId;
                    sItem.Item = arr.Item;
                    db.SRItemss.Add(sItem);
                    db.SaveChanges();
                }

                db.DummySRItems.RemoveRange(db.DummySRItems.Where(a => a.SalesReturnId == id));
                db.SaveChanges();
            }

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            ViewBag.PrintLayout = PriLay;
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();
            ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");
            var rtype = Request.Query["rtype"];


            var ref1 = db.SalesReturns
              .Select(s => new
              {
                  ID = s.Ref1,
                  Name = s.Ref1
              }).Distinct()
              .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", vmodel.Ref1);

            var ref2 = db.SalesReturns
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", vmodel.Ref2);

            var ref3 = db.SalesReturns
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", vmodel.Ref3);

            var ref4 = db.SalesReturns
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", vmodel.Ref4);

            var ref5 = db.SalesReturns
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", vmodel.Ref5);


           
            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            else
            {
                return View(vmodel);
            }
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Sales Return")]
        public JsonResult UpdateSalesReturn(string[][] array, string[] saledata, SRBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel, ICollection<RackStockPViewModel> bsrackData)
        {
            bool stat = false;
            string msg;
            Int64 salesReturnId = Convert.ToInt64(saledata[17]);
            SalesReturn SRentry = db.SalesReturns.Find(salesReturnId);
            var CurrentDate     = Convert.ToDateTime(System.DateTime.Now);
            if (BillExist(Convert.ToString(saledata[19])) && Convert.ToString(saledata[19]) != SRentry.BillNo)
            {

                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var UserId = User.Identity.GetUserId();
            long Branch = 0;
            Int64 saleRAcc = (long)db.companys.Select(a => a.SReturnAccount).FirstOrDefault();

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            if (BranchCheck == Status.active)
            {
                Branch = Convert.ToInt64(saledata[23]);
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

            long MC = 0;
            if (MCcheck == Status.active)
            {
                MC = Convert.ToInt64(saledata[22]);
            }
            else
            {
                MC = 1;
            }

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            var EditPermission = User.IsInRole("Disable SReturn Edit After Approval");
            if (com.chkApproved(salesReturnId, EditPermission, "SalesReturn", UserId) == true)
            {


                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var TaxAmount = Convert.ToDecimal(saledata[5]);
                var GrandTotal = Convert.ToDecimal(saledata[7]);
                var saleramount = GrandTotal - TaxAmount;
                var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));
                var subtotal = Convert.ToDecimal(saledata[8]);
                SRentry.SRDate = date;
                SRentry.SRCashier = saledata[1] != "" ? Convert.ToInt64(saledata[1]) : 0;

                var CustType = SRentry.CustomerType;
                //walkin customer
                SRentry.CustomerType = (saledata[12] == "1") ? CustomerType.Walking : CustomerType.Customer;
                SRentry.Customer = Convert.ToInt64(saledata[0]);
                SRentry.SalesEntryId = Convert.ToInt64(saledata[16]);
                if (saledata[18] == "1")
                {
                    SRentry.ReturnType = ReturnType.Direct;
                }
                else
                {
                    SRentry.ReturnType = ReturnType.AgainstBill;                    
                }

                //pay type for pos
                SRentry.BillNo = saledata[19];
                SRentry.PayType = "";//need change
                SRentry.SRItems = Convert.ToInt32(saledata[3]);
                SRentry.SRItemQuantity = Convert.ToDecimal(saledata[4]);
                SRentry.SRSubTotal = Convert.ToDecimal(saledata[8]);
                SRentry.SRTax = Convert.ToDecimal(saledata[9]);
                SRentry.SRTaxAmount = Convert.ToDecimal(saledata[5]);
                SRentry.SRDiscount = Convert.ToDecimal(saledata[6]);
                SRentry.SRGrandTotal = GrandTotal;
                SRentry.SRNote = saledata[11];
                SRentry.Print = 1;
                SRentry.Status = 1;
                SRentry.Branch = Branch;
                SRentry.Remarks = saledata[21];
                SRentry.MaterialCenter = MC;
                SRentry.SalesType = Convert.ToInt64(saledata[24]);
                SRentry.Project = saledata[25] != "" ? Convert.ToInt64(saledata[25]) : 0;
                SRentry.ProTask = saledata[26] != "" ? Convert.ToInt64(saledata[26]) : 0;

                SRentry.Ref1 = Convert.ToString(saledata[28]);
                SRentry.Ref2 = Convert.ToString(saledata[29]);
                SRentry.Ref3 = Convert.ToString(saledata[30]);
                SRentry.Ref4 = Convert.ToString(saledata[31]);
                SRentry.Ref5 = Convert.ToString(saledata[32]);
                if (SRentry.Project != null && SRentry.Project != 0)
                {
                    SRentry.SReturnAccount = db.Projects.Where(a => a.ProjectId == SRentry.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                }
                else
                {
                    SRentry.SReturnAccount = saleRAcc;
                }

                db.Entry(SRentry).State = EntityState.Modified;

                //To Update the quantity in Edit Mode(ItemTransaction Table)               
                com.ItemTransInEditMode("SalesReturn", MC, 0, 0, array, salesReturnId, UserId, CurrentDate);

                var SRItem = db.SRItemss.Where(a => a.SalesReturnId == salesReturnId).FirstOrDefault();
                if (SRItem != null)
                {
                    var SItems = db.SRItemss.Where(a => a.SalesReturnId == salesReturnId).ToList();
                    foreach (var arr in SItems)
                    {
                        //add to dummy table
                        DummySRItem dItem = new DummySRItem();
                        dItem.ItemUnit = arr.ItemUnit;
                        dItem.ItemUnitPrice = arr.ItemUnitPrice;
                        dItem.ItemQuantity = arr.ItemQuantity;
                        dItem.ItemSubTotal = arr.ItemSubTotal;
                        dItem.ItemDiscount = arr.ItemDiscount;
                        dItem.ItemTax = arr.ItemTax;
                        dItem.ItemTaxAmount = arr.ItemTaxAmount;
                        dItem.ItemTotalAmount = arr.ItemTotalAmount;
                        dItem.itemNote = arr.itemNote;
                        dItem.SalesReturnId = arr.SalesReturnId;
                        dItem.Item = arr.Item;
                        db.DummySRItems.Add(dItem);
                        db.SaveChanges();
                    }

                    db.SRItemss.RemoveRange(db.SRItemss.Where(a => a.SalesReturnId == salesReturnId));
                    db.SaveChanges();

                }

                string result = string.Empty;

                DataTable dtItem = new DataTable();
                dtItem.Columns.Add("ItemUnit");
                dtItem.Columns.Add("ItemUnitPrice");
                dtItem.Columns.Add("ItemQuantity");
                dtItem.Columns.Add("ItemSubTotal");
                dtItem.Columns.Add("ItemDiscount");
                dtItem.Columns.Add("ItemTax");
                dtItem.Columns.Add("ItemTaxAmount");
                dtItem.Columns.Add("ItemTotalAmount");
                dtItem.Columns.Add("itemNote");
                dtItem.Columns.Add("SalesReturnId");
                dtItem.Columns.Add("Item");

                foreach (var arr in array)
                {
                    DataRow dr = dtItem.NewRow();

                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                    dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                    dr["ItemTax"] = Convert.ToDecimal(arr[10]);
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                    dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                    dr["SalesReturnId"] = salesReturnId;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    dtItem.Rows.Add(dr);

                    var item = Convert.ToInt32(arr[0]);
                    var chkbundle = db.ItemBundles.Where(a => a.mainItem == item).Select(a => a.ItemBundleId).FirstOrDefault();
                    if (chkbundle > 0)
                    {
                        var bunQuan = Convert.ToDecimal(arr[2]);
                        var itemBundle = (from g in db.ItemBundles
                                          join b in db.Items on g.mainItem equals b.ItemID
                                          where b.ItemID == item
                                          select new
                                          {
                                              g.ItemBundleId
                                          }).FirstOrDefault();
                        var bundle = (from a in db.BundleItems
                                      join b in db.Items on a.ItemId equals b.ItemID
                                      join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                      from c in primary.DefaultIfEmpty()
                                      where a.ItemBundle == itemBundle.ItemBundleId
                                      select new
                                      {
                                          b.ItemCode,
                                          b.ItemName,
                                          c.ItemUnitName,
                                          ItemUnitPrice = a.ItemUnitPrice,
                                          quantity = a.ItemQuantity,
                                          ItemSubTotal = a.ItemSubTotal,
                                          ItemTax = a.ItemTax,
                                          ItemTaxAmount = a.ItemTaxAmount,
                                          ItemTotalAmount = a.ItemTotalAmount,
                                          ItemUnit = a.ItemUnit,
                                          Item = a.ItemId
                                      }).ToList();
                        foreach (var bu in bundle)
                        {
                            var qua = (bunQuan * bu.quantity);
                            var ItemSubTotal = qua * bu.ItemUnitPrice;
                            var buTaxAmount = (ItemSubTotal * bu.ItemTax) / 100;

                            decimal itemtax = 0;
                            decimal taxamt = 0;
                            decimal totamt = 0;

                            itemtax = bu.ItemTax;
                            taxamt = buTaxAmount;
                            totamt = (buTaxAmount + ItemSubTotal);

                            DataRow dbu = dtItem.NewRow();
                            dbu["ItemUnit"] = bu.ItemUnit;
                            dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                            dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                            dbu["ItemSubTotal"] = ItemSubTotal;
                            // add parent itemid in discount for reference
                            dbu["ItemDiscount"] = item;
                            dbu["ItemTax"] = itemtax;
                            dbu["ItemTaxAmount"] = taxamt;
                            dbu["ItemTotalAmount"] = totamt;
                            dbu["itemNote"] = "-:{Bundle_Item}";
                            dbu["SalesReturnId"] = salesReturnId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }
                }

                ////// create parameter 
                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypeSRItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertSRItems", "@TableType");
                //// execute sql 
                var ret = db.Database.ExecuteSqlRaw(sql, parameter);

                if (ret > 0)
                {
                    db.DummySRItems.RemoveRange(db.DummySRItems.Where(a => a.SalesReturnId == salesReturnId));
                    db.SaveChanges();
                }
                var PERack = db.shelfstockmovements.Where(a => a.referenceid == salesReturnId && a.purpose == "Sales Return").FirstOrDefault();
                if (PERack != null)
                {
                    db.shelfstockmovements.RemoveRange(db.shelfstockmovements.Where(a => a.referenceid == salesReturnId && a.purpose == "Sales Return"));
                    db.SaveChanges();
                }
                //rackstock
                
                
                
                if (bsrackData != null)
                {
                    foreach (var bst in bsrackData)
                    {
                        if (bst.StockOut != 0)
                        {

                            decimal bStockIn = 0;

                            shelfstockmovement Btst = new shelfstockmovement();
                            Btst.purpose = "Sales Return";
                            Btst.itemid = bst.Item;
                            Btst.unitid = (long)bst.Unit;
                            Btst.rackmciid = (long)com.getrackmcid(MC, bst.RackNo, bst.ShelfNo);
                            Btst.qty = bst.StockOut;

                            Btst.referenceid = salesReturnId;


                            Btst.createddate = DateTime.Now;
                            Btst.createdby = UserId;

                            db.shelfstockmovements.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                // batch stock

                var SEBst = db.BatchStocks.Where(a => a.Reference == salesReturnId && a.Type == "SaleReturn").FirstOrDefault();
                if (SEBst != null)
                {
                    db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == salesReturnId && a.Type == "SaleReturn"));
                    db.SaveChanges();
                }
                if (bstmodel != null)
                {
                    foreach (var bst in bstmodel)
                    {
                        if (bst.BatchNo != "" && bst.BatchNo != null)
                        {
                            DateTime? exp = null;
                            DateTime? mfg = null;
                            if (bst.EXP != null && bst.EXP != "")
                            {
                                exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                            }
                            if (bst.MFG != null && bst.MFG != "")
                            {
                                mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                            }
                            decimal bStock = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStock = bst.StockIn * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockIn = bStock;
                            Btst.StockOut = 0;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = salesReturnId;
                            Btst.Type = "SaleReturn";

                            Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            Btst.Date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }


                var SRBs = db.SRBillSundrys.Where(a => a.SalesReturnId == salesReturnId).FirstOrDefault();
                if (SRBs != null)
                {
                    db.SRBillSundrys.RemoveRange(db.SRBillSundrys.Where(a => a.SalesReturnId == salesReturnId));
                    db.SaveChanges();

                }
                if (bsmodel.srbsundrys != null)
                {
                    string bsResult = string.Empty;

                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("SalesReturnId");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.srbsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["SalesReturnId"] = salesReturnId;
                        drw["BillSundry"] = bs.BillSundry;
                        drw["BsValue"] = bs.BsValue;
                        drw["AmountType"] = bs.AmountType;
                        drw["BsType"] = bs.BsType;
                        drw["BsAmount"] = bs.BsAmount;

                        BsEntry.Rows.Add(drw);
                    }
                    ////// create parameter 
                    SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                    parameter1.SqlDbType = SqlDbType.Structured;
                    parameter1.TypeName = "TableTypeSRBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertSRBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);
                }

                decimal amount = Convert.ToDecimal(saledata[10]);
                Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == SRentry.Customer).Select(a => a.Accounts).FirstOrDefault();
                Int64 saleAccId = saleRAcc;//db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).FirstOrDefault();
                if (SRentry.Project != null && SRentry.Project!= 0)
                {
                    saleAccId = db.Projects.Where(a => a.ProjectId == SRentry.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                }


                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();
                //walking customer
                if (saledata[12] == "1")
                {
                    //AccountsTransaction
                    amount = GrandTotal;
                }


                //----------new added----------------
                if (saledata[12] == "1")//cash
                {
                    deleteAndUpdateTrans(salesReturnId, saledata, amount, custAccID, cashAccId, BranchID, UserId);
                    changePayBill(salesReturnId);
                }
                if (saledata[12] == "0")//credit
                {
                    if (CustType == CustomerType.Walking)//previous cash
                    {
                        deleteAndUpdateTrans(salesReturnId, saledata, amount, custAccID, cashAccId, BranchID, UserId);
                    }
                    if (CustType == CustomerType.Customer)//previous credit
                    {

                        decimal sumTran = db.SRTransactions.Where(a => a.SalesReturnId == salesReturnId).FirstOrDefault() != null ? (decimal?)db.SRTransactions.Where(a => a.SalesReturnId == salesReturnId).Select(a => a.SRPayAmount).Sum() ?? 0 : 0;
                        if (sumTran > GrandTotal)
                        {
                            var chkpay = db.PaymentBills.Where(a => a.InvoiceNo == salesReturnId && a.BillType == "Sales Return" && a.Type == "Against Reference").ToList();
                            if (chkpay != null)
                            {
                                var payamount = sumTran - GrandTotal;
                                amount = GrandTotal;
                                decimal TotAmt = GrandTotal;
                                foreach (var pbill in chkpay)
                                {
                                    TotAmt = TotAmt - pbill.Amount;
                                    if (TotAmt < 0 && pbill.Amount > payamount)
                                    {
                                        var reamt = pbill.Amount - payamount;


                                        PaymentBill paybillz = db.PaymentBills.Find(pbill.PaymentBillId);
                                        paybillz.Amount = reamt;
                                        db.Entry(paybillz).State = EntityState.Modified;
                                        db.SaveChanges();

                                        PaymentBill paybill = new PaymentBill();
                                        paybill.InvoiceNo = null;
                                        paybill.NewRefName = "";
                                        paybill.Payment = Convert.ToInt64(pbill.Payment);
                                        paybill.BillType = null;
                                        paybill.Amount = payamount;
                                        paybill.Type = "New Reference";
                                        paybill.Status = Status.active;

                                        db.PaymentBills.Add(paybill);
                                        db.SaveChanges();

                                    }
                                }
                                updatePepayment(salesReturnId, saledata, amount, BranchID, 0);
                            }
                            else
                            {
                                deleteAndUpdateTrans(salesReturnId, saledata, amount, custAccID, cashAccId, BranchID, UserId);
                            }
                        }
                        else
                        {
                            updatePepayment(salesReturnId, saledata, amount, BranchID, 1);
                        }

                    }
                }


                bool delete = com.DeleteAllAccountTransaction("Sale Return", salesReturnId);
                bool deletepay = com.DeleteAllAccountTransaction("Sale Return Payment", salesReturnId);


                //bill sundry account
                var Gtotal = GrandTotal;
                if (bsmodel.srbsundrys != null)
                {
                    foreach (var bs in bsmodel.srbsundrys)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                        {
                            if (bs.BsAmount == null)
                                bs.BsAmount = 0;
                            var bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                saleramount = saleramount - bsamount;
                                com.addAccountTrasaction((decimal)bs.BsAmount, 0, (long)ChkAcc.SAccount, "Sale Return", salesReturnId, DC.Debit, date, null, null, SRentry.Project, SRentry.ProTask);
                            }
                            else //substract
                            {
                                saleramount = saleramount + bsamount;
                                com.addAccountTrasaction(0, (decimal)bs.BsAmount, (long)ChkAcc.SAccount, "Sale Return", salesReturnId, DC.Credit, date, null, null, SRentry.Project, SRentry.ProTask);
                            }
                        }
                    }
                }

                //add trasaction to sale account 
                com.addAccountTrasaction(saleramount, 0, saleAccId, "Sale Return", salesReturnId, DC.Debit, date, null, null, SRentry.Project, SRentry.ProTask);
                //    //add sale trasaction
                    com.addAccountTrasaction(0, GrandTotal, custAccID, "Sale Return", salesReturnId, DC.Credit, date, null, null, SRentry.Project, SRentry.ProTask);
                // 
                // add vat input in account transaction
                if (TaxAmount > 0 && Convert.ToInt64(saledata[24]) != 3)
                    com.addAccountTrasaction(TaxAmount, 0, VATOutput, "Sale Return", salesReturnId, DC.Debit, date, null, null, SRentry.Project, SRentry.ProTask);
                if (Convert.ToDecimal(saledata[10]) > 0 || saledata[12] == "1")
                {
                    //if payment
                    com.addAccountTrasaction(amount, 0, custAccID, "Sale Return Payment", salesReturnId, DC.Debit, date, null, null, SRentry.Project, SRentry.ProTask);
                    com.addAccountTrasaction(0, amount, cashAccId, "Sale Return Payment", salesReturnId, DC.Credit, date, null, null, SRentry.Project, SRentry.ProTask);
                }
                com.addlog(LogTypes.Updated, UserId, "SaleReturn", "SaleReturns", findip(), salesReturnId, "Successfully Updated Sales Return");

            }

            //Approved By
            var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == salesReturnId && a.Type == "SalesReturn").FirstOrDefault();
            var MrnPO = db.Approvals.Where(a => a.TransEntry == salesReturnId && a.Type == "SalesReturn").FirstOrDefault();
            if (MrnPO != null)
            {
                if (chkapp != null)
                {
                    db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == salesReturnId && a.Type == "SalesReturn"));
                    db.SaveChanges();
                }
                else
                {
                    db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == salesReturnId && a.Type == "SalesReturn"));
                    db.SaveChanges();
                }
            }
            var Appby = Convert.ToString(saledata[27]);
            if (Appby != null && Appby != "")
            {
                long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                Approval approval = new Approval();
                foreach (var emp in Approve)
                {
                    approval.TransEntry = salesReturnId;
                    approval.Type = "SalesReturn";
                    approval.EmployeeId = emp;
                    db.Approvals.Add(approval);
                    db.SaveChanges();
                }
            }
            string action = saledata[15];
            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            if (action == "print")
            {
                var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                //field mappping
                var fmapp = db.FieldMappings.Where(a => a.Section == "SReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                var saleRetData = com.SalesReturnData(salesReturnId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck);
                var item = saleRetData["item"];
                var summary = saleRetData["summary"];
                var billsundry = saleRetData["billsundry"];

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                var def = (PriLay == Status.active) ? Convert.ToInt64(saledata[33]):Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout , fmapp } };
            }
            else if (action == "sendmail")
            {
                SendMail sm = new SendMail();
                MailMessage message = new MailMessage();
                string ToMail = saledata[19];
                string CcMail = "";
                string InvoiceNo = "_SalesReturn_" + SRentry.BillNo;

                var em = db.EmailTemplates.Where(a => a.Head == "SalesReturn").FirstOrDefault();
                if (em != null)
                {
                    message.Subject = em.Subject;
                    message.Body = em.EmailBody;
                }
                else
                {
                    message.Subject = "Sales Return";
                    message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                        " <p>we are enclosing our sales return for the items / services as requested by you during our discussions.<br/></p> " +
                        " <p>Looking forward to hear from you.</p>";
                }
                sm.SendPdfMail(generatePdf(salesReturnId), ToMail, CcMail, InvoiceNo, message);

                msg = "Successfully Updated Sales Return Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Successfully Updated Sales Return.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public void updatePepayment(long SEntryId, string[] sdata, decimal amount, long BranchID, int chk)
        {
            var GrandTotal = Convert.ToDecimal(sdata[7]);
            SRPayment SRpay = db.SRPayments.Where(a => a.SalesReturnId == SEntryId).FirstOrDefault();
            SRpay.CustomerId = Convert.ToInt64(sdata[0]);
            SRpay.SRDate = DateTime.Parse(sdata[2], new CultureInfo("en-GB"));
            SRpay.SRBillAmount = GrandTotal;

            if (chk == 0)
            {
                if (sdata[12] == "1")
                {
                    SRpay.SReturnAmount = GrandTotal;
                }
                else
                {
                    SRpay.SReturnAmount = amount != 0 ? amount : Convert.ToDecimal(sdata[10]);
                }
            }

            SRpay.CreatedBranch = Convert.ToInt32(BranchID);
            SRpay.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
            SRpay.Status = 1;
            db.Entry(SRpay).State = EntityState.Modified;
            db.SaveChanges();
        }
        public void deleteAndUpdateTrans(long salesReturnId, string[] saledata, decimal amount, long custAccID, long cashAccId, long BranchID, string UserId)
        {
            var GrandTotal = Convert.ToDecimal(saledata[7]);
            var date = DateTime.Parse(saledata[2], new CultureInfo("en-GB"));

            SRPayment SRpay = db.SRPayments.Where(a => a.SalesReturnId == salesReturnId).FirstOrDefault();
            SRpay.CustomerId = Convert.ToInt64(saledata[0]);
            SRpay.SRDate = date;
            SRpay.SREntryDate = Convert.ToDateTime(System.DateTime.Now);
            SRpay.SRBillAmount = GrandTotal;

            if (saledata[12] == "1")
            {
                SRpay.SReturnAmount = GrandTotal;
            }
            else
            {
                SRpay.SReturnAmount = amount != 0 ? amount : Convert.ToDecimal(saledata[10]);
            }

            SRpay.CreatedBranch = Convert.ToInt32(BranchID);
            SRpay.CreatedUserId = UserId;
            SRpay.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
            SRpay.Status = 1;

            db.Entry(SRpay).State = EntityState.Modified;
            db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.SalesReturnId == salesReturnId));
            db.Payments.RemoveRange(db.Payments.Where(a => a.Reference == salesReturnId && a.RefType == "SalesReturn"));
            db.SaveChanges();

           
            if (amount > 0 || Convert.ToDecimal(saledata[10]) > 0 || saledata[12] == "1")
            {

                var Remark = "Direct Payment From SalesReturn";
                long payid;
                //SETransaction
                SRTransaction SRtran = new SRTransaction();

                SRtran.CustomerId = Convert.ToInt64(saledata[0]);
                SRtran.SRPayDate = date;
                //walkin customer
                if (saledata[12] == "1")
                {
                    SRtran.SRPayAmount = amount;
                    payid = com.addPayment(date, cashAccId, custAccID, amount, amount, amount, Remark, UserId, BranchID, salesReturnId, "SalesReturn");
                }
                else
                {
                    amount = Convert.ToDecimal(saledata[10]);
                    SRtran.SRPayAmount = amount;
                    payid = com.addPayment(date, cashAccId, custAccID, amount, amount, amount, Remark, UserId, BranchID, salesReturnId, "SalesReturn");
                }
                SRtran.PaymentId = payid;
                SRtran.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                SRtran.CreatedBranch = Convert.ToInt32(BranchID);
                SRtran.CreatedUserId = UserId;
                SRtran.Status = 1;
                SRtran.SalesReturnId = salesReturnId;
                db.SRTransactions.Add(SRtran);
                db.SaveChanges();
            }
        }

        public void changePayBill(long salesReturnId)
        {
            //receipt bill changes
            var chkrec = db.PaymentBills.Where(a => a.InvoiceNo == salesReturnId && a.BillType == "Sales Return").ToList();
            if (chkrec != null)
            {
                db.PaymentBills.Where(a => a.InvoiceNo == salesReturnId && a.BillType == "Sales Return").ToList().ForEach(a => a.Type = "New Reference");
                db.SaveChanges();
            }
        }

        [HttpGet]
      
        public ActionResult Download(long id)
        {
            var Data = db.SalesReturns.Where(s => s.SalesReturnId == id).FirstOrDefault();
            var custname = db.Customers.Where(s => s.CustomerID == Data.Customer).Select(a => a.CustomerName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Sales Return" + "-" + custname + "-" + billno + ".pdf");


        }
        public StringBuilder generatePdf(long salesRetId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var details = (from a in db.SalesReturns
                           join b in db.Customers on a.Customer equals b.CustomerID into cust
                           from b in cust.DefaultIfEmpty()
                           join c in db.Contacts on b.Contact equals c.ContactID into cnt
                           from c in cnt.DefaultIfEmpty()
                           join d in db.SRPayments on a.SalesReturnId equals d.SalesReturnId into pay
                           from d in pay.DefaultIfEmpty()
                           join e in db.Employees on a.SRCashier equals e.EmployeeId into user
                           from e in user.DefaultIfEmpty()
                           join f in db.Accountss on b.Accounts equals f.AccountsID into acc
                           from f in acc.DefaultIfEmpty()
                           where a.SalesReturnId == salesRetId
                           select new
                           {
                               PartyName = b.CustomerName,
                               BillNo = a.BillNo,
                               Date = a.SRDate,
                               Note = a.SRNote,
                               Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                               Discount = a.SRDiscount,
                               Total = a.SRDiscount + a.SRGrandTotal,
                               GrandTotal = a.SRGrandTotal,
                               Paid = d != null ? d.SReturnAmount : 0,
                               Balance = d != null ? a.SRGrandTotal - d.SReturnAmount : 0,
                               SubTotal = a.SRSubTotal,
                               TaxAmount = a.SRTaxAmount,
                               c.Address,
                               c.City,
                               c.State,
                               c.Country,
                               c.Zip,
                               Email = c.EmailId,
                               Phone = c.Phone,
                               Mobile = (from ac in db.Mobiles
                                         where (ac.Contact == b.Contact)
                                         select new MobileViewModel
                                         {
                                             Num = ac.MobileNum,
                                             Name = ac.Name
                                         }).ToList(),
                               TRN = b.TaxID_TRN,
                               paytype = (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                               TermsCondition = a.SRNote,
                               BillId = a.SalesReturnId,
                               a.Remarks
                           }).FirstOrDefault();

            var saleitem = (from b in db.SRItemss
                            join c in db.Items on b.Item equals c.ItemID
                            join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                            from g in bundle.DefaultIfEmpty()
                            where b.SalesReturnId == salesRetId && b.itemNote != "-:{Bundle_Item}"
                            select new
                            {
                                ItemUnitPrice = b.ItemUnitPrice,
                                ItemQuantity = b.ItemQuantity,
                                ItemSubTotal = b.ItemSubTotal,
                                ItemTax = b.ItemTax,
                                ItemNote = b.itemNote,
                                ItemTaxAmount = b.ItemTaxAmount,
                                ItemTotalAmount = b.ItemTotalAmount,
                                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                                ItemID = b.Item,
                                FileName = db.ItemImages.Where(a => a.ItemID == b.Item).Select(a => a.FileName).FirstOrDefault(),
                                ItemDesc = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemDescription).FirstOrDefault(),
                                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                                bundleitem = (from ab in db.SRItemss
                                              join bb in db.Items on ab.Item equals bb.ItemID
                                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                              from cb in primary.DefaultIfEmpty()
                                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                              from bd in second.DefaultIfEmpty()
                                              where ab.SalesReturnId == salesRetId && ab.itemNote == "-:{Bundle_Item}"
                                              && b.Item == ab.ItemDiscount
                                              select new
                                              {

                                                  bb.ItemCode,
                                                  bb.ItemName,
                                                  cb.ItemUnitName,
                                                  ItemUnitPrice = ab.ItemUnitPrice,
                                                  quantity = ab.ItemQuantity,
                                                  ItemSubTotal = ab.ItemSubTotal,
                                                  ItemTax = ab.ItemTax,
                                                  ItemTaxAmount = ab.ItemTaxAmount,
                                                  ItemTotalAmount = ab.ItemTotalAmount,

                                                  ab.Item,
                                                  ab.ItemQuantity,
                                                  ab.ItemUnit,

                                                  ItemDiscount = 0,

                                                  ItemNote = ab.itemNote,
                                                  ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                  bb.ItemUnitID,
                                                  bb.SubUnitId,
                                                  PriUnit = cb.ItemUnitName,
                                                  SubUnit = bd.ItemUnitName,
                                                  bb.ItemArabic
                                              }).ToList()
                            }).ToList();

            var billsundry = db.SRBillSundrys.Where(n => n.SalesReturnId == salesRetId).Select(b => new
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();


            var cdetails = db.companys
            .Select(s => new
            {
                CName = s.CPName,
                CAddress = s.CPAddress,
                CEmail = s.CPEmail,
                CTaxRegNo = s.TRN,
                CPhone = s.CPPhone,
                s.CPMobile,
                CLogo = s.CPLogo,
            }).FirstOrDefault();

            int SI = 1;
            string address = "";
            if (details.Address != null)
            {
                address += details.Address;
            }
            if (details.City != null)
            {
                address += "<br />" + details.City;
            }
            else if (details.State != null)
            {
                address += "<br />" + details.State;
            }
            else if (details.Country != null)
            {
                address += "<br />" + details.Country;
            }
            else if (details.Zip != null)
            {
                address += "<br />" + details.Zip;
            }
            address += " <br/> Phone : ";
            if (details.TRN != null)
            {
                address += "<br/> TRN : " + details.TRN;
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {

                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Sales Return</b></td></tr></table>");
                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +
                        "<table  style='border: 0px; width: 100 %;'><tr><th>Invoice No</th><td style='font-size:14px;font-weight:normal;'>: " + details.BillNo + "</td></tr><tr><th>Date تاريخ</th><td style='font-size:14px;font-weight:normal;'>: " + details.Date.ToString("dd-MM-yyyy") + "</td></tr><tr><th>Sales Executive منفذ مبيعات</th><td style='font-size:14px;font-weight:normal;'>: " + details.Cashier + "</td></tr></table></td></tr></table>";

                    sb.Append(partyDetails);
                    sb.Append("<table width='100%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc;repeat-header:yes;'>");
                    sb.Append("<thead>");
                    sb.Append("<tr style='font-size:13px;'>");
                    sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>S/N</th>");
                    if (PartNoCheck == Status.active)
                    {
                        sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Part No</th>");
                    }
                    sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Item</th>");
                    sb.Append("<th width='5%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Unit</th>");
                    sb.Append("<th width='8%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Quantity</th>");
                    sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Unit Price</th>");
                    sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Amount</th>");
                    sb.Append("<th width='10%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Tax(5.00 %)</th>");
                    sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Total</th>");
                    sb.Append("</tr>");
                    sb.Append("</thead>");
                    sb.Append("<tbody>");
                    var itemcount = 0;
                    foreach (var item in saleitem)
                    {
                        var itemnote = "";
                        if (item.ItemNote != null)
                        {
                            itemnote = "<br /><small>" + item.ItemNote + "</small>";
                        }
                        if (item.bundleitem != null && item.bundleitem.Count > 0)
                        {
                            var desc = "<br/>[<span class='descr' data-name='Note'>";
                            foreach (var itemss in item.bundleitem)
                            {
                                desc += itemss.ItemName;
                                desc += " - " + (itemss.quantity) + " ";
                                desc += (itemss.ItemUnitName != null) ? itemss.ItemUnitName : "";
                                desc += "<br/>";
                            }
                            desc += "</span>]";
                            itemnote = itemnote + desc;
                        }
                        sb.Append("<tr style='font-size:10px;'>");
                        {
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + SI++ + "</td>");
                            if (PartNoCheck == Status.active)
                            {
                                var ParNo = (!string.IsNullOrEmpty(item.PartNumber) && !string.IsNullOrWhiteSpace(item.PartNumber)) ? item.PartNumber : "";
                                sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + ParNo + "</td>");
                            }
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>");
                            sb.Append(item.ItemName + "<br/>");
                            sb.Append("<img width='40px' height='70px' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + item.ItemID + "/" + item.FileName) + "'/>");
                            sb.Append("<br/>");
                            sb.Append("<div>");
                            sb.Append(item.ItemDesc + itemnote);
                            sb.Append("</div>");
                            sb.Append("</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemUnit + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemQuantity + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemUnitPrice + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemSubTotal + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemTaxAmount + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemTotalAmount + "</td>");
                        }
                        sb.Append("</tr>");
                        itemcount++;
                    }
                    sb.Append("</tbody>");
                    sb.Append("</table>");

                    string words = com.ConvertToWords(details.GrandTotal.ToString());
                    sb.Append("<table width='100%' style='border-collapse: collapse;border: .1px solid #ccc;font-size: 14px;'>");
                    string discount = "";
                    int count = 2;
                    if (details.Discount > 0)
                    {
                        discount = "<td style='border: .1px solid #ccc;padding: 10px;'>Discount خصم</td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.Discount + "</td></tr> ";
                        discount += "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;'>VAT<span style='direction:ltr'>(5.00%)</span> برميل</td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.TaxAmount + "</td></tr>";
                        count++;
                    }
                    else
                    {

                        discount += "<td style='border: .1px solid #ccc;padding: 10px;'>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.TaxAmount + "</td></tr>";
                    }

                    string bsundry = "";
                    if (billsundry != null)
                    {
                        foreach (var bilsun in billsundry)
                        {

                            bsundry += "<tr class='border-top'>";
                            bsundry += "<td style='border: .1px solid #ccc;padding: 10px;'>" + bilsun.BillSundry + "</td>";
                            bsundry += "<td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + Convert.ToDecimal(bilsun.BsAmount).ToString() + "</td>";
                            bsundry += "</tr>";

                            count++;
                        }
                    }
                    discount += bsundry;

                    discount += "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;'><b>Total المبلغ الإجمالي(AED)</b></td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'><b>" + details.GrandTotal + "</b></td></tr>";

                    string word = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;font-size: 15px;' colspan='5'><strong>" + words + " </strong></td><td style='border: .1px solid #ccc;padding: 10px;'>Amount كمية</td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.SubTotal + "</td></tr>";
                    string tc = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;' colspan='5' rowspan='" + count + "'> </td>";

                    string remarks = "";
                    if (!string.IsNullOrEmpty(details.TermsCondition) && !string.IsNullOrWhiteSpace(details.TermsCondition))
                    {
                        remarks += "<tr class='border-top'><td colspan='7' style='border: .1px solid #ccc;padding: 10px;'><strong><u>Terms And Conditions :</u></strong><br/>" + details.TermsCondition.Replace("\n", "<br />") + "</td></tr>";
                    }
                    if (!string.IsNullOrEmpty(details.Remarks) && !string.IsNullOrWhiteSpace(details.Remarks))
                    {
                        remarks += "<tr class='border-top'><td colspan='7' style='border: .1px solid #ccc;padding: 10px;'><strong><u>Remarks :</u></strong><br/>" + details.Remarks.Replace("\n", "<br />") + "</td></tr>";
                    }

                    sb.Append(word + tc + discount + remarks);
                    sb.Append("</table>");

                    sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                    sb.Append("<tr>");
                    sb.Append("<td align='left' width='50%' style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>Receiver's Signature:<br />توقيع المتلقي</div>");
                    sb.Append("</td>");
                    sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>");
                    sb.Append("For " + cdetails.CName + "");
                    sb.Append("</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                }
            }
            return sb;
        }

        [HttpGet]
        public ActionResult GetSaleTypes(int SeId)
        {
            var types = db.SalesReturns.Where(a => a.SalesReturnId == SeId)
                .Select(b => new
                {
                    b.ReturnType,
                    b.CustomerType
                }).FirstOrDefault();
            return Json(types);

        }

        [HttpGet]
        public ActionResult GetSRItems(long SalesReturnID)
        {
            var ConD = (from a in db.SRItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.SalesReturnId == SalesReturnID && a.itemNote != "-:{Bundle_Item}"
                        select new
                        {
                            a.Item,
                            a.ItemQuantity,
                            a.ItemUnit,
                            a.ItemUnitPrice,
                            a.ItemTax,
                            a.ItemSubTotal,
                            a.ItemTaxAmount,
                            a.ItemDiscount,
                            a.ItemTotalAmount,
                            note = a.itemNote.Replace("<br />", "\n"),
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            b.ConFactor,
                            b.KeepStock,
                            b.slreq,
                            batch = (from ay in db.BatchStocks
                                     join az in db.SRItemss on new { f1 = ay.Item, f2 = ay.Unit, f3 = ay.Reference, f4 = ay.Type }
                                          equals new { f1 = az.Item, f2 = az.ItemUnit, f3 = az.SalesReturnId, f4 = "SaleReturn" }
                                     where az.SalesReturnId == SalesReturnID && ay.Item == a.Item
                                     select new BatchStockPViewModel
                                     {
                                         BatchNo = ay.BatchNo,
                                         MFGd = ay.MFG,
                                         EXPd = ay.EXP,
                                         StockIn = ay.StockIn,
                                         StockOut = ay.StockOut,
                                         Item = ay.Item,
                                         cfactor = b.ConFactor,
                                         Priunit = b.ItemUnitID,
                                         Secunit = b.SubUnitId,
                                         Unit = ay.Unit,
                                         Cost = ay.Cost,
                                         origin = "SaleReturn",
                                         Order = ay.Order
                                     }).ToList(),
                            rack = (from ay in db.shelfstockmovements
                                    join az in db.SRItemss on new { f1 = ay.itemid, f2 = ay.unitid, f3 = ay.referenceid, f4 = ay.purpose }
                                     equals new { f1 = az.Item, f2 = (long)az.ItemUnit, f3 = az.SalesReturnId, f4 = "Sales Return" }
                                    join aa in db.rackmaterialcentres on ay.rackmciid equals aa.rackmcid
                                    join b in db.Shelves on aa.shelfid equals b.ShelfId
                                    join c in db.Racks on aa.rackid equals c.RackId

                                    where az.SalesReturnId == a.SalesReturnId && ay.itemid == a.Item
                                    select new
                                    {
                                        ShelfNo = aa.shelfid,
                                        RackNo = aa.rackid,
                                        ShelfName = b.shelfName,
                                        RackName = c.RackName,
                                        StockIn = ay.qty,
                                    }).ToList()
                        }).ToList();
            return Json(ConD);
        }
        [HttpGet]
        public ActionResult GetSRBillSundry(long SalesReturnID)
        {
            var SEBs = (from a in db.SRBillSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.SalesReturnId == SalesReturnID
                        select new
                        {
                            a.AmountType,
                            a.BillSundry,
                            a.BsAmount,
                            a.BsType,
                            a.BsValue,
                            //a.PEBillSundryId,
                            //a.PurchaseEntry,
                            c.BSName
                            //c.BillSundryId
                        }).ToList();
            return Json(SEBs);
        }

        [QkAuthorize(Roles = "Dev,Delete Sales Return")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Sales Return Entry");
            var UserId = User.Identity.GetUserId();
            SalesReturn SErt = db.SalesReturns.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.SalesReturnId == id).FirstOrDefault();
            if (SErt == null)
            {
                return NotFound();
            }
            return PartialView(SErt);
        }
        [HttpPost, ActionName("Delete")]
      //  [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Sales Return")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Sales Return Entry.";
            }
            #region Old Code
            ////var SET = db.AccountsTransactions.Where(a => a.Id == id).FirstOrDefault();

            #endregion            

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Sales Return")]
        public ActionResult DeleteAllSReturn(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSale(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Sales Return Entry.", true);
            return RedirectToAction("Index", "CreditSaleReturn");
        }

        private Boolean DeleteSale(long sId)
        {
            var Msg = chkDeleteWithMsg(sId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(sId);
            }
        }


        public bool DeleteFn(long sId)
        {
            var UserId = User.Identity.GetUserId();
            SalesReturn SErt = db.SalesReturns.Find(sId);
            var SRItem = db.SRItemss.Where(a => a.SRItemsId == sId);
            var SRP = db.SRPayments.Where(a => a.SalesReturnId == sId).FirstOrDefault();
            var SRPT = db.SRTransactions.Where(a => a.SalesReturnId == sId).ToList();
            var SRBs = db.SRBillSundrys.Where(a => a.SalesReturnId == sId).ToList();
            var customerId = db.SalesReturns.Where(a => a.SalesReturnId == sId).Select(a => a.Customer).FirstOrDefault();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            if (SRItem != null)
            {
                if (SErt.MaterialCenter != null)
                    /***************** Item Transaction ******************/
                    com.ItemTransInDeleteMode("SalesReturn", SErt.MaterialCenter, 0, 0, sId, UserId, CurrentDate);

                db.SRItemss.RemoveRange(db.SRItemss.Where(a => a.SalesReturnId == sId));
                db.SaveChanges();
            }
            if (SRBs != null)
            {
                db.SRBillSundrys.RemoveRange(db.SRBillSundrys.Where(a => a.SalesReturnId == sId));
                db.SaveChanges();
            }
            if (SRP != null)
            {
                db.SRPayments.RemoveRange(db.SRPayments.Where(a => a.SalesReturnId == sId));
                db.SaveChanges();
            }
            if (SRPT != null)
            {
                db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.SalesReturnId == sId));
                db.SaveChanges();
            }
            var pay = db.Payments.Where(a => a.Reference == sId && a.RefType == "SalesReturn").FirstOrDefault();
            if (pay != null)
            {
                db.Payments.RemoveRange(db.Payments.Where(a => a.Reference == sId && a.RefType == "SalesReturn"));
                db.SaveChanges();
            }
            var paybill = db.PaymentBills.Where(a => a.InvoiceNo == sId && a.BillType == "Sales Return" && a.Type == "Against Reference").ToList();
            if (paybill != null)
            {
                var recbillz = db.PaymentBills.Where(a => a.InvoiceNo == sId && a.BillType == "Sales Return" && a.Type == "Against Reference").ToList();
                recbillz.ForEach(a =>
                {
                    a.Type = "New Reference";
                    a.BillType = null;
                    a.InvoiceNo = null;
                });
                db.SaveChanges();
            }

            if (SErt.SalesEntryId != 0 && SErt.SalesEntryId != null)
            {
                SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == SErt.SalesEntryId).FirstOrDefault();
                SEP.SEPaidAmount = SEP.SEPaidAmount - SErt.SRGrandTotal;
                db.Entry(SEP).State = EntityState.Modified;
                db.SaveChanges();
            }
            var appr = db.Approvals.Where(a => a.TransEntry == sId && a.Type == "SalesReturn").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == sId && a.Type == "SalesReturn"));
                db.SaveChanges();
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == sId && a.Type == "SalesReturn").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == sId && a.Type == "SalesReturn"));
                db.SaveChanges();
            }


            // batch stock
            var SEBst = db.BatchStocks.Where(a => a.Reference == sId && a.Type == "SaleReturn").FirstOrDefault();
            if (SEBst != null)
            {
                db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == sId && a.Type == "SaleReturn"));
                db.SaveChanges();
            }

            bool delete = com.DeleteAllAccountTransaction("Sale Return", sId);
            bool deletepay = com.DeleteAllAccountTransaction("Sale Return Payment", sId);

            db.SalesReturns.Remove(SErt);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "SalesReturn", "SalesReturns", findip(), SErt.SalesReturnId, "Successfully Deleted Sales Return");
            return true;
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.PaymentBills.Any(u => u.InvoiceNo == id && u.Type == "Against Reference"))
            {
                msg = "Sales Return Already used in Payment Entry !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }
        private string InvoiceNo(Int64 SRNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "SalsReturn").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "SalsReturn").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.SalesReturns.Select(p => p.SRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SRNo = db.SalesReturns.Max(p => p.SRNo + 1);
                    billNo = companyPrefix + SRNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(SRNo, billNo);
                    }
                }
            }
            else
            {
                SRNo = SRNo + 1;
                billNo = companyPrefix + SRNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(SRNo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SRNo)
        {
            var Exists = db.SalesReturns.Any(c => c.BillNo == SRNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetSRNo()
        {
            Int64 PNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "SalsReturn").Select(a => a.number).FirstOrDefault();
            if ((db.SalesReturns.Select(p => p.SRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                PNo = (number == 0) ? 1 : number;
            }
            else
            {
                PNo = db.SalesReturns.Max(p => p.SRNo + 1);
            }
            return PNo;
        }

        [HttpGet]
        public JsonResult CheckInvoice(int saleId)
        {
            string msg = "0";
            var v = (from a in db.SalesReturns
                     join b in db.SalesEntrys on a.SalesEntryId equals b.SalesEntryId into team
                     from b in team.DefaultIfEmpty()
                     where a.SalesEntryId == saleId
                     select new
                     {
                         bill = b.BillNo,

                     }).FirstOrDefault();
            var s = db.customerbonus.Where(o => o.salesentryid == saleId).Select(o => o.claimamount).FirstOrDefault();
            if(s!=null)
            {
                return Json("1");
            }
            if (v == null)
            {
                return Json(msg);
            }
            else
            {
                return Json(v);
            }
           
            
        }

        [HttpGet]
        public JsonResult GetMaxReturnId(long? SalesEntryId, string SaveMode)
        {
            

            if (SaveMode == "Edit")
            {
                var RetnDtls = db.SalesReturns.Where(a => a.SalesEntryId == SalesEntryId).FirstOrDefault();

                if (RetnDtls != null)
                    return Json(db.SalesReturns.Where(a => a.SalesEntryId == SalesEntryId).Select(a => a.SRNo));
                else
                    return Json(InvoiceNo());
            }  
            else
                return Json(InvoiceNo());

        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "SalesReturn" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList();

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       where e != ApprovalStatus.PendingApproval && (appstat.Count == 0 || e != appstat.Select(a => a.ApprovalStatus).FirstOrDefault())
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            return PartialView();
        }

        [HttpPost]
        public ActionResult EditStatus(ApprovalUpdate App, long id)
        {
            bool stat = false;
            string msg = "";
            var UserId = User.Identity.GetUserId();

            var MR = db.SalesReturns.Where(a => a.SalesReturnId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "SalesReturn").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
      
            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = MR.CreatedBy;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "SalesReturn";

                db.ApprovalUpdates.Add(AppUp);
                db.SaveChanges();

                stat = true;
                msg = "Successfully Updated Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                stat = false;
                msg = "Updating Same Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        [HttpPost]
        public ActionResult GetAllStatusUpdation(long MCId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var UserView = (from b in db.ApprovalUpdates
                            join c in db.Users on b.ApprovedBy equals c.Id
                            join d in db.SalesReturns on b.TransEntry equals d.SalesReturnId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "SalesReturn"
                            select new
                            {
                                b.ApprovalUpdateID,
                                b.TransEntry,
                                b.Status,
                                b.ApprovalStatus,
                                b.CreatedDate,
                                b.Note,
                                RequestBy = u.UserName,
                                c.UserName,
                                ApprovedBy = "" //e.FirstName + " " + e.LastName,
                            }).Distinct().ToList().Select(o => new
                            {
                                o.ApprovalUpdateID,
                                o.TransEntry,
                                o.Status,
                                ApprovalStatus = Enum.GetName(typeof(ApprovalStatus), o.ApprovalStatus),

                                o.ApprovedBy,
                                o.RequestBy,
                                User = o.UserName, //db.Users.Where(a => a.Id == o.CreatedUser).Select(a => a.UserName).FirstOrDefault(),
                                o.CreatedDate,
                                Remarks = o.Note
                            });
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult UploadFiles()
        {
            if (Request.Form.Files.Count > 0)
            {
                string SalesReturnId = Request.Form.GetValues("id").First();
                long SRId = 0;

                if (SalesReturnId.Contains("undefined"))
                {
                    var LastId = db.SalesReturns.OrderByDescending(a => a.SalesReturnId).FirstOrDefault();
                    SRId = LastId.SalesReturnId;
                }
                else
                {
                    SRId = Convert.ToInt64(SalesReturnId);
                }

                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/SalesReturnDocuments/");

                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];

                            if (file.Length > 0)
                            {
                                var fileCount = db.AttachmentDocuments.Select(a => a.DocumentID).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName = Path.GetFileName(file.FileName);

                                String extension = Path.GetExtension(fileName);

                                var FStatus = Status.active;
                                String newName = fileCount + extension;
                                string newFName = fileCount + extension;
                                var thumbName = "";
                                var resizeName = "";

                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/SalesReturnDocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/SalesReturnDocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";
                                }

                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/SalesReturnDocuments/"), newName);
                                file.SaveAs(newName);

                                var PODocument = new AttachmentDocuments
                                {
                                    TransactionID = SRId,
                                    TransactionType = "Sales Return",
                                    FileName = newFName,
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.AttachmentDocuments.Add(PODocument);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/SalesReturnDocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/SalesReturnDocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }
                                }
                            }
                        }
                    }
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Return")]
        public JsonResult ImageDelete(long key)
        {
            //To remove the attached file(single row) from database
            AttachmentDocuments Document = db.AttachmentDocuments.Find(key);
            db.AttachmentDocuments.Remove(Document);
            db.SaveChanges();

            //To remove the attached file from folder
            string fullpath = LegacyWeb.MapPath("~/uploads/SalesReturnDocuments/" + Document.FileName);

            if (System.IO.File.Exists(fullpath))
            {
                System.IO.File.Delete(fullpath);
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "SalesReturn", "AttachmentDocuments", findip(), Document.DocumentID, "Sales Return Document Deleted Successfully");

            bool status = true;
            string message = "Successfully deleted Sales Return attachment details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = status, message = message, Id = key } };
        }


    }
}
