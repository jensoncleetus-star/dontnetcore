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
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class JobCardController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public JobCardController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,JobCard List")]
        public ActionResult Index()
        {
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

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

            var MlaJCard = db.EnableSettings.Where(a => a.EnableType == "MLAJCard").FirstOrDefault();
            var MlaJCards = MlaJCard != null ? MlaJCard.Status : Status.inactive;
            ViewBag.MLAJCard = MlaJCards;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindJobCard").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            return View();
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,JobCard List")]
        public ActionResult GetJobCard(string BillNo, string FromDate, string ToDate, long? customer, long? mechanic, long? salesperson, string appstat)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

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

            string search = Request.Form.GetValues("search[value]")[0];
           
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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


            var userpermission = User.IsInRole("All JobCard Entry");
            var UserId = User.Identity.GetUserId();

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uJobCardView = User.IsInRole("View JobCard");
            var uEdit = User.IsInRole("Edit JobCard");
            var uDelete = User.IsInRole("Delete JobCard");

            var serverQuery = (from a in db.JobCards
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Employees on a.Mechanic equals c.EmployeeId into mech
                     from c in mech.DefaultIfEmpty()
                     let d = db.Employees.Where(x => x.EmployeeId == a.ReceivedBy).FirstOrDefault()
                     join e in db.Users on a.CreatedBy equals e.Id

                     // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) can't be translated by
                     // EF Core 10 inside this projection — computed CLIENT-side after materialization via lookups
                     // keyed by JobCardId (same split as QuotationController.GetQuotation / EstimateController.GetEstimate).
                     where (BillNo == null || BillNo == "" || a.JobCardNo == BillNo) &&
                   (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.JCDate, fdate) <= 0) &&
                   (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.JCDate, tdate) >= 0) &&
                   (customer == 0 || b.CustomerID == customer) &&
                   (mechanic == 0 || c.EmployeeId == mechanic) &&
                   (salesperson == 0 || d.EmployeeId == salesperson)
                     select new
                     {
                         a.JobCardId,
                         a.JobCardNo,
                         a.JCDate,
                         a.Details,
                         a.PWCModel,
                         e.UserName,
                         Customer = b.CustomerCode + "-" + b.CustomerName,
                         Mechanic = c.FirstName + " " + c.LastName,
                         ReceivedBy = d.FirstName + " " + d.LastName,
                         a.TotalAmount,
                         Dev = uDev,
                         DetailsView = uJobCardView,
                         Edit = uEdit,
                         Delete = uDelete,

                         a.CreatedDate
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "CreatedDate","Customer","Delete","Details","DetailsView","Dev","Edit","JCDate","JobCardId","JobCardNo","Mechanic","PWCModel","ReceivedBy","TotalAmount","UserName" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("JobCardId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side approval lookups keyed by JobCardId (missing key -> empty/absent, no KeyNotFound).
            var jcIds = serverRows.Select(o => o.JobCardId).ToList();
            // app = approver EmployeeIds for the job card (keyed by TransEntry == JobCardId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "JobCard" && jcIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "JobCard" && jcIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per job card.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(b => b.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.JobCardId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.JobCardId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.JobCardId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {

                         o.JobCardId,
                         o.JobCardNo,
                         o.JCDate,
                         o.Details,
                         o.PWCModel,
                         o.UserName,
                         o.Customer ,
                         o.Mechanic ,
                         o.ReceivedBy ,
                         o.TotalAmount,
                         o.Dev,
                         o.DetailsView ,
                         o.Edit ,
                         o.Delete ,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                     };
                     });
            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.JobCardNo.ToString().ToLower().Contains(search.ToLower()));
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

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create JobCard")]
        public ActionResult Create()
        {
            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
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
            ViewBag.Emp = QkSelect.List(use, "ID", "Name");

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

            var UserId = User.Identity.GetUserId();
            var userpermission = User.IsInRole("All JobCard Entry");
            ViewBag.LastEntry = db.JobCards.Where(a => a.CreatedBy == UserId || userpermission == true).Select(p => p.JobCardId).AsEnumerable().DefaultIfEmpty(0).Max();

            var jcmodel = new JobCardViewModel
            {
                JobCardNo = CardNo(),
                JCDate = Convert.ToDateTime(DateTime.Now).ToString("dd-MM-yyyy"),
            };
            companySet();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
              .Select(s => new
              {
                  ID = s.EmployeeId,
                  Name = s.FirstName + " " + s.LastName
              })
              .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaJCard = db.EnableSettings.Where(a => a.EnableType == "MLAJCard").FirstOrDefault();
            var MlaJCards = MlaJCard != null ? MlaJCard.Status : Status.inactive;
            ViewBag.MLAJCard = MlaJCards;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            jcmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "JobCard" && a.Status == Status.active).ToList();

            return View(jcmodel);
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create JobCard")]
        public JsonResult Create(JobCardViewModel vmodel)
        {
            bool stat = false;
            string msg;

            if (!BillExist(Convert.ToString(vmodel.JobCardNo)))
            {
                if (ModelState.IsValid)
                {
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    var UserId = User.Identity.GetUserId();                    
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    var jcdate = DateTime.Parse(vmodel.JCDate.ToString(), new CultureInfo("en-GB"));

                    if (BranchCheck == Status.active)
                    {
                        Branch = vmodel.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    //jobcard
                    var jcard = new JobCard
                    {
                        JCNo = GetJcNo(),
                        JobCardNo = vmodel.JobCardNo,
                        JCDate = jcdate,
                        Customer = vmodel.Customer,
                        Mechanic = vmodel.Mechanic,
                        ReceivedBy = vmodel.ReceivedBy,
                        PWCModel = vmodel.PWCModel,
                        Details = vmodel.Details,
                        TotalAmount = vmodel.TotalAmount,

                        CreatedDate = today,
                        CreatedBy = UserId,
                        Status = Status.active,
                        Branch = Branch,
                        Ref1=vmodel.Ref1,
                        Ref2=vmodel.Ref2,
                        Ref3=vmodel.Ref3,
                        Ref4=vmodel.Ref4,
                        Ref5=vmodel.Ref5,
                    };
                    db.JobCards.Add(jcard);
                    db.SaveChanges();
                    Int64 JCId = jcard.JobCardId;

                    //items
                    JCItem jcItem = new JCItem();
                    foreach (var arr in vmodel.jcitems)
                    {
                        if (arr.Item != 0)
                        {
                            jcItem.JobCard = JCId;
                            jcItem.Item = arr.Item;
                            jcItem.ItemTotalAmount = arr.ItemTotalAmount;

                            db.JCItems.Add(jcItem);
                            db.SaveChanges();
                        }
                    }

                    //Approved By
                    var Appby = vmodel.ApprovedBy;
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = JCId;
                            approval.Type = "JobCard";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Created, UserId, "JobCard", "JobCards", findip(), JCId, "JobCard Added Successfully");

                    if (vmodel.action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "JobCard" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
                        var summary = (from a in db.JobCards
                                       join b in db.Customers on a.Customer equals b.CustomerID into cust
                                       from b in cust.DefaultIfEmpty()
                                       join c in db.Employees on a.Mechanic equals c.EmployeeId into mech
                                       from c in mech.DefaultIfEmpty()
                                       join d in db.Contacts on b.Contact equals d.ContactID into cnt
                                       from d in cnt.DefaultIfEmpty()
                                       join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                       from i in acc.DefaultIfEmpty()
                                       let e = db.Employees.Where(x => x.EmployeeId == a.ReceivedBy).FirstOrDefault()
                                       join f in db.Users on a.CreatedBy equals f.Id
                                       where a.JobCardId == JCId
                                       select new
                                       {
                                           PartyName = b.CustomerName,
                                           a.JobCardId,
                                           CardNo = a.JobCardNo,
                                           Date = a.JCDate,
                                           a.Details,
                                           PWCModel = a.PWCModel,
                                           f.UserName,
                                           MechName = c.FirstName + " " + c.LastName,
                                           Employee = e.FirstName + " " + e.LastName,
                                           a.TotalAmount,
                                           d.Address,
                                           d.City,
                                           d.State,
                                           d.Country,
                                           d.Zip,
                                           Email = d.EmailId,
                                           Phone = d.Phone,
                                           Mobile = (from ac in db.Mobiles
                                                     where (ac.Contact == b.Contact)
                                                     select new MobileViewModel
                                                     {
                                                         Num = ac.MobileNum,
                                                         Name = ac.Name
                                                     }).ToList(),
                                           TRN = i.TRN,
                                           a.Ref1,
                                           a.Ref2,
                                           a.Ref3,
                                           a.Ref4,
                                           a.Ref5,
                                           ComHeadCheck = ComHeadCheck
                                       }).FirstOrDefault();

                        var item = db.JCItems.Where(n => n.JobCard == JCId).Select(b => new
                        {
                            ItemTotalAmount = b.ItemTotalAmount,
                            ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                            ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault()

                        }).ToList();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, fmapp } };
                    }
                    else
                    {
                        msg = "Successfully added JobCard.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }

                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                msg = "JobCard No Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit JobCard")]
        public ActionResult Edit(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobCard jc = db.JobCards.Find(id);
            if (jc == null)
            {
                return NotFound();
            }
            JobCardViewModel vmodel = new JobCardViewModel();
            vmodel.JobCardNo = jc.JobCardNo;
            vmodel.JCDate = Convert.ToDateTime(jc.JCDate).ToString("dd-MM-yyyy");
            vmodel.Customer = jc.Customer;
            vmodel.Mechanic = jc.Mechanic;
            vmodel.ReceivedBy = jc.ReceivedBy;
            vmodel.PWCModel = jc.PWCModel;
            vmodel.Details = jc.Details;
            vmodel.TotalAmount = jc.TotalAmount;
            vmodel.Branch = jc.Branch;
            vmodel.Ref1 = jc.Ref1;
            vmodel.Ref2 = jc.Ref2;
            vmodel.Ref3 = jc.Ref3;
            vmodel.Ref4 = jc.Ref4;
            vmodel.Ref5 = jc.Ref5;

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
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
            ViewBag.Emp = QkSelect.List(use, "ID", "Name");

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

            var UserId = User.Identity.GetUserId();
            var userpermission = User.IsInRole("All JobCard Entry");
            ViewBag.preEntry = db.JobCards.Where(a => a.JobCardId < id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.JobCardId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.JobCards.Where(a => a.JobCardId > id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.JobCardId).DefaultIfEmpty().Min();

            companySet();
            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "JobCard").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaJCard = db.EnableSettings.Where(a => a.EnableType == "MLAJCard").FirstOrDefault();
            var MlaJCards = MlaJCard != null ? MlaJCard.Status : Status.inactive;
            ViewBag.MLAJCard = MlaJCards;

            var EditPermission = User.IsInRole("Disable JobCard Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "JobCard", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "JobCard" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit JobCard")]
        public JsonResult Edit(JobCardViewModel vmodel, long? id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                var UserId = User.Identity.GetUserId();                
                var today = Convert.ToDateTime(System.DateTime.Now);
                var jcdate = DateTime.Parse(vmodel.JCDate.ToString(), new CultureInfo("en-GB"));

                if (BranchCheck == Status.active)
                {
                    Branch = vmodel.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }

                var EditPermission = User.IsInRole("Disable JobCard Edit After Approval");
                if (com.chkApproved((long)id, EditPermission, "JobCard", UserId) == true)
                {

                    //jobcard
                    JobCard jcard = db.JobCards.Find(id);

                    jcard.JobCardNo = vmodel.JobCardNo;
                    jcard.JCDate = jcdate;
                    jcard.Customer = vmodel.Customer;
                    jcard.Mechanic = vmodel.Mechanic;
                    jcard.ReceivedBy = vmodel.ReceivedBy;
                    jcard.PWCModel = vmodel.PWCModel;
                    jcard.Details = vmodel.Details;
                    jcard.TotalAmount = vmodel.TotalAmount;
                    jcard.Branch = Branch;
                    jcard.Ref1 = vmodel.Ref1;
                    jcard.Ref2 = vmodel.Ref2;
                    jcard.Ref3 = vmodel.Ref3;
                    jcard.Ref4 = vmodel.Ref4;
                    jcard.Ref5 = vmodel.Ref5;
                    db.Entry(jcard).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 JCId = jcard.JobCardId;


                    var jcitem = db.JCItems.Where(a => a.JobCard == JCId).FirstOrDefault();
                    if (jcitem != null)
                    {
                        db.JCItems.RemoveRange(db.JCItems.Where(a => a.JobCard == JCId));
                        db.SaveChanges();
                    }
                    //items
                    JCItem jcItem = new JCItem();
                    foreach (var arr in vmodel.jcitems)
                    {
                        if (arr.Item != 0)
                        {
                            jcItem.JobCard = JCId;
                            jcItem.Item = arr.Item;
                            jcItem.ItemTotalAmount = arr.ItemTotalAmount;

                            db.JCItems.Add(jcItem);
                            db.SaveChanges();
                        }
                    }

                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == JCId && a.Type == "JobCard").FirstOrDefault();
                    var MrnPO = db.Approvals.Where(a => a.TransEntry == JCId && a.Type == "JobCard").FirstOrDefault();
                    if (MrnPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == JCId && a.Type == "JobCard"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == JCId && a.Type == "JobCard"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = vmodel.ApprovedBy;
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = JCId;
                            approval.Type = "JobCard";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "JobCard", "JobCards", findip(), JCId, "JobCard Updated Successfully");
                }
                if (vmodel.action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "JobCard" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
                    var summary = (from a in db.JobCards
                                   join b in db.Customers on a.Customer equals b.CustomerID into cust
                                   from b in cust.DefaultIfEmpty()
                                   join c in db.Employees on a.Mechanic equals c.EmployeeId into mech
                                   from c in mech.DefaultIfEmpty()
                                   join d in db.Contacts on b.Contact equals d.ContactID into cnt
                                   from d in cnt.DefaultIfEmpty()
                                   join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                   from i in acc.DefaultIfEmpty()
                                   let e = db.Employees.Where(x => x.EmployeeId == a.ReceivedBy).FirstOrDefault()
                                   join f in db.Users on a.CreatedBy equals f.Id
                                   where a.JobCardId == id
                                   select new
                                   {
                                       PartyName = b.CustomerName,
                                       a.JobCardId,
                                       CardNo = a.JobCardNo,
                                       Date = a.JCDate,
                                       a.Details,
                                       PWCModel = a.PWCModel,
                                       f.UserName,
                                       MechName = c.FirstName + " " + c.LastName,
                                       Employee = e.FirstName + " " + e.LastName,
                                       a.TotalAmount,
                                       d.Address,
                                       d.City,
                                       d.State,
                                       d.Country,
                                       d.Zip,
                                       Email = d.EmailId,
                                       Phone = d.Phone,
                                       Mobile = (from ac in db.Mobiles
                                                 where (ac.Contact == b.Contact)
                                                 select new MobileViewModel
                                                 {
                                                     Num = ac.MobileNum,
                                                     Name = ac.Name
                                                 }).ToList(),
                                       TRN = i.TRN,
                                       a.Ref1,
                                       a.Ref2,
                                       a.Ref3,
                                       a.Ref4,
                                       a.Ref5,
                                       ComHeadCheck = ComHeadCheck
                                   }).FirstOrDefault();

                    var item = db.JCItems.Where(n => n.JobCard == id).Select(b => new
                    {
                        ItemTotalAmount = b.ItemTotalAmount,
                        ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                        ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault()

                    }).ToList();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary , fmapp } };
                }
                else
                {
                    msg = "JobCard Updated Successfully";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete JobCard")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            JobCard jcard = db.JobCards.Find(id);
            if (jcard == null)
            {
                return NotFound();
            }
            return PartialView(jcard);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete JobCard")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(long id)
        {
            try
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
                    msg = "Successfully deleted JobCard.";
                }
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete JobCard")]
        public ActionResult DeleteAllJobCard(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteJb(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " JobCard.", true);
            return RedirectToAction("Index", "JobCard");
        }
        private Boolean DeleteJb(long Id)
        {
            var Msg = chkDeleteWithMsg(Id);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(Id);
            }
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            return msg;
        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            JobCard jcard = db.JobCards.Find(id);
            var jcitem = db.JCItems.Where(a => a.JobCard == id);
            if (jcitem != null)
            {
                db.JCItems.RemoveRange(db.JCItems.Where(a => a.JobCard == id));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "JobCard").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "JobCard"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "JobCard").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "JobCard"));
            }
            db.JobCards.Remove(jcard);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "JobCard", "JobCards", findip(), id, "Successfully Deleted JobCard");

            return true;
        }


        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,View JobCard")]
        public ActionResult Details(long? id)
        {
            JobCardViewModel vmodel = new JobCardViewModel();
            vmodel = (from a in db.JobCards
                      join b in db.Customers on a.Customer equals b.CustomerID into cust
                      from b in cust.DefaultIfEmpty()
                      join c in db.Employees on a.Mechanic equals c.EmployeeId into mech
                      from c in mech.DefaultIfEmpty()
                      let d = db.Employees.Where(x => x.EmployeeId == a.ReceivedBy).FirstOrDefault()
                      join e in db.Users on a.CreatedBy equals e.Id
                      where a.JobCardId == id
                      select new JobCardViewModel
                      {
                          JobCardId = a.JobCardId,
                          JobCardNo = a.JobCardNo,
                          JobCDate = a.JCDate,
                          Details = a.Details,
                          PWCModel = a.PWCModel,
                          UserName = e.UserName,
                          CustName = b.CustomerCode + "-" + b.CustomerName,
                          MechName = c.FirstName + " " + c.LastName,
                          RecName = d.FirstName + " " + d.LastName,
                          TotalAmount = a.TotalAmount,
                          Ref1=a.Ref1,
                          Ref2=a.Ref2,
                          Ref3=a.Ref3,
                          Ref4=a.Ref4,
                          Ref5=a.Ref5,
                      }).FirstOrDefault();

            vmodel.JCItem = db.JCItems.Where(a => a.JobCard == id)
            .Select(b => new JCItemViewModel
            {
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
            }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "JobCard" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [HttpGet]
        public ActionResult GetJCItems(long JobCardID)
        {
            var ConD = (from a in db.JCItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.JobCard == JobCardID
                        select new
                        {
                            a.Item,
                            a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                        });
            return Json(ConD);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,JobCard Setting")]
        public ActionResult JobCardSetting()
        {
            return View();
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,JobCard Setting")]
        public JsonResult JobCardSetting(JobCardViewModel vmodel)
        {
            bool stat = false;
            string msg;
                var UserId = User.Identity.GetUserId();

                var jcitem = db.JobCardItemSettings.FirstOrDefault();
                if (jcitem != null)
                {
                    db.JobCardItemSettings.RemoveRange(db.JobCardItemSettings);
                    db.SaveChanges();
                }

                //items
                JobCardItemSetting jcItem = new JobCardItemSetting();
            if (vmodel.jcitems != null)
            {
                foreach (var arr in vmodel.jcitems)
                {
                    if (arr.Item != 0)
                    {
                        jcItem.Item = arr.Item;
                        jcItem.ItemTotalAmount = arr.ItemTotalAmount;

                        db.JobCardItemSettings.Add(jcItem);
                        db.SaveChanges();
                    }
                }
            }




                com.addlog(LogTypes.Created, UserId, "JobCardItemSetting", "JobCardItemSettings", findip(), jcItem.JCItemId, "JobCard Labour/Parts Added Successfully");

                msg = "Successfully added JobCard Labour/Parts.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpGet]
        public ActionResult GetDefaultItems()
        {
            var ConD = (from a in db.JobCardItemSettings
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        select new
                        {
                            a.Item,
                            a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                        });
            return Json(ConD);
        }


        private long GetJcNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "JobCard").Select(a => a.number).FirstOrDefault();
            if ((db.JobCards.Select(p => p.JCNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = (number == 0) ? 1 : number;                
            }
            else
            {
                SENo = db.JobCards.Max(p => p.JCNo + 1);
            }

            return SENo;
        }
        private string CardNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "JobCard").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "JobCard").Select(a => a.number).FirstOrDefault();
                if ((db.JobCards.Select(p => p.JCNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.JobCards.Max(p => p.JCNo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = CardNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = CardNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SENo)
        {
            var Exists = db.JobCards.Any(c => c.JobCardNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "JobCard" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.JobCards.Where(a => a.JobCardId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "JobCard").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

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
                AppUp.Type = "JobCard";

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
                            join d in db.JobCards on b.TransEntry equals d.JobCardId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "JobCard"
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
    }
}
