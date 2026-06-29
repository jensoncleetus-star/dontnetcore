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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class MaterialRequisitionController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public MaterialRequisitionController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [HttpPost]
        public ActionResult GetRequisitionReport(string srchtxt,string vno, long? supplier, long? employee, string fromdate, string todate, long? ProjectName, long? Task, string RequestStat, long? Item)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            var v = (from a in db.MaterialRequisitions
                     join c in db.Employees on a.MRCashier equals c.EmployeeId into emp
                     from c in emp.DefaultIfEmpty()
                     join e in db.Branchs on a.Branch equals e.BranchID into bran
                     from e in bran.DefaultIfEmpty()
                     join d in db.Projects on a.Project equals d.ProjectId into prj
                     from d in prj.DefaultIfEmpty()
                     join f in db.ProTasks on a.ProTask equals f.ProTaskId into task
                     from f in task.DefaultIfEmpty()
                     join g in db.MaterialRequisitionItems on a.MaterialRequisitionId equals g.MaterialRequisition into temp
                     from g in temp.DefaultIfEmpty()
                     join h in db.Items on g.Item equals h.ItemID into temp2
                     from h in temp2.DefaultIfEmpty()
                     join i in db.ItemUnits on g.ItemUnit equals i.ItemUnitID into temp3
                     from i in temp3.DefaultIfEmpty()
                    
                     where (vno == "" || a.BillNo == vno) &&

                        (employee == 0 || a.MRCashier == employee) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.MRDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.MRDate, tdate) >= 0) &&
                        (ProjectName == 0 || ProjectName == null || a.Project == ProjectName) &&
                   (Task == 0 || Task == null || a.ProTask == Task) &&
                   (RequestStat == "" || RequestStat == "All" || a.RequestStatus == RequestStat) &&
                   (srchtxt==""||(h.ItemName.Contains(srchtxt) || g.ItemNote.Contains(srchtxt)))&&
                   (supplier == 0 || supplier == null || a.SupplierId == supplier) &&
                   (Item == 0 || Item == null || g.Item == Item)
                    // group new { g.ItemUnit, h.SubUnitId, h.ItemID, a.MaterialRequisitionId, h.ItemCode, h.ItemName, i.ItemUnitName, g.ItemQuantity, h.ConFactor } by new { h.ItemID, g.ItemUnit } into g
                     select new
                     {
                         
                         MaterialRequisitionId= g.Item,
                         item = h.ItemCode + "-" + h.ItemName,
                         ItemUnit =i.ItemUnitName,//db.ItemUnits.Where(aa=>aa.ItemUnitID==h.ItemUnitID).Select(aa=>aa.ItemUnitName), //(h.SubUnitId == g.ItemUnit) ? i.ItemUnitName : "",
                         ItemQuantity = (h.SubUnitId == g.ItemUnit) ? g.ItemQuantity : g.ItemQuantity / h.ConFactor,
                         UnitPrice= h.PurchasePrice,
                         reqno=a.BillNo,
                         reqid=a.MaterialRequisitionId,
                         //a.MRNo,
                         //a.BillNo,
                         //a.MRDate,
                         //a.MRItems,
                         //a.MRItemQuantity,

                         //EmpName = c.FirstName + " " + c.LastName,
                         //a.MRValidity,
                         //a.MRCreatedDate,
                         //ProjectName = (d.ProjectName != null && d.ProjectName != "") ? d.ProCode + "-" + d.ProjectName : "",
                         //Task = (f.TaskName != null && f.TaskName != "") ? f.TaskCode + "-" + f.TaskName : "",
                     }).ToList();
            var v2 = (from a in v
                      group new { a.MaterialRequisitionId, a.item, a.ItemUnit, a.ItemQuantity, a.UnitPrice } by new { a.MaterialRequisitionId } into g
                      select new
                      {
                          MaterialRequisitionId = g.Key.MaterialRequisitionId,
                          item = g.FirstOrDefault().item,
                          reqno = (from a in db.MaterialRequisitions
                                   join b in db.MaterialRequisitionItems on a.MaterialRequisitionId equals b.MaterialRequisition
                                   where(b.Item==g.Key.MaterialRequisitionId &&
                                    (vno == "" || a.BillNo == vno) &&

                        (employee == 0 || a.MRCashier == employee) &&
                        (fromdate == "" || EF.Functions.DateDiffDay(a.MRDate, fdate) <= 0) &&
                        (todate == "" || EF.Functions.DateDiffDay(a.MRDate, tdate) >= 0) &&
                        (ProjectName == 0 || ProjectName == null || a.Project == ProjectName) &&
                   (Task == 0 || Task == null || a.ProTask == Task) &&
                   (RequestStat == "" || RequestStat == "All" || a.RequestStatus == RequestStat) &&
                   (supplier == 0 || supplier == null || a.SupplierId == supplier) )


                                   select new
                                   {
                                       a.BillNo,
                                       a.MaterialRequisitionId
                                   }
                                   ).ToList(),
                          ItemQuantity = g.Sum(k => k.ItemQuantity),
                          ItemUnit = g.FirstOrDefault().ItemUnit,
                          UnitPrice = g.FirstOrDefault().UnitPrice,
                      });


            var data = v2.ToList();
            recordsTotal = v2.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        [HttpGet]
        public ActionResult downloadprint(long MRId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;


            var UserId = User.Identity.GetUserId();
            var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;


            long? Prj = null;

            var fmapp = db.FieldMappings.Where(a => a.Section == "MR" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

            MaterialRequisition MRentry = db.MaterialRequisitions.Find(MRId);

            string qedate = MRentry.MRDate.ToString("dd-MM-yyyy");
            object item = "";
            object summary = "";
            object cdetails = "";
            object approval = "";
            object billsundry = "";

            var MReq = com.MaterialRequisitionData(MRId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, BranchCheck, ComHeadCheck);
            item = MReq["item"];
            summary = MReq["summary"];
            cdetails = MReq["cdetails"];
            approval = MReq["approval"];
            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;

            var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
            def = def == 0 ? 1 : def;
            var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
            bool stat = true;
            return Json(new { status = stat, item, summary, cdetails, approval, layout, fmapp });
        }

        [QkAuthorize(Roles = "Dev, MaterialRequisition List")]
        public ActionResult Index()
        {
            ViewBag.SalesExecutive = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");

            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

            //    ID = s.SupplierID,
            //    Name = s.SupplierName



            ViewBag.DropDowns = QkSelect.List(
                           new List<SelectListItem>
                           {
                            new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                           }, "Value", "Text", 1);

            var MlaMc = db.EnableSettings.Where(a => a.EnableType == "MLAMc").FirstOrDefault();
            var MlaMcs = MlaMc != null ? MlaMc.Status : Status.inactive;
            ViewBag.MLAMc = MlaMcs;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            _FinancialYear();

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindMReqn").FirstOrDefault();
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
               }).Take(1)
               .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             }).Take(1)
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            MaterialRequisitionViewModel vmodel = new MaterialRequisitionViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "MR" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "MR").ToList();

            var ref1 = db.MaterialRequisitions
            .Select(s => new
            {
                ID = s.Ref1,
                Name = s.Ref1
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.MaterialRequisitions
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            return View(vmodel);
        }
       
        [HttpPost]
        [QkAuthorize(Roles = "Dev,MaterialRequisition List")]
        public ActionResult GetMaterialRequisition(string reqstatus,string BillNo, string FromDate, string ToDate, string Stats, long? RequestedBy, string user, string Validity, string appstat,long? ProjectName,long? Task, string RemndrDate, string ref1, string ref2, string ref3, string ref4, string ref5, long? supplier)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? rdate = null;

            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB"));
            }
            if (RemndrDate != ""&& RemndrDate!=null)
            {
                rdate = DateTime.Parse(RemndrDate, new CultureInfo("en-GB"));
            }

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var fromv = "MOrder";
            var ToPO = "POrder";
            var ToPQ = "PQuote";
            var MRToPQuot = db.EnableSettings.Where(a => a.EnableType == "MRToPQuot").FirstOrDefault();
            var MRToPQuots = MRToPQuot != null ? MRToPQuot.Status : Status.inactive;

            Status st = new Status();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };


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

            var userpermission = User.IsInRole("All MaterialRequisition Entry");
            var UserId = User.Identity.GetUserId();

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var uDev = User.IsInRole("Dev");
            var uMaterialRequisitionView = User.IsInRole("View MaterialRequisition");
            var uEdit = User.IsInRole("Edit MaterialRequisition");
            var uDownload = User.IsInRole("Download MaterialRequisition");
            var uDelete = User.IsInRole("Delete MaterialRequisition");

            var serverQuery = (from b in db.MaterialRequisitions
                     join e in db.Employees on b.MRCashier equals e.EmployeeId into emp
                     from e in emp.DefaultIfEmpty()
                     join f in db.Projects on b.Project equals f.ProjectId into prj
                     from f in prj.DefaultIfEmpty()
                     join i in db.ProTasks on b.ProTask equals i.ProTaskId into task
                     from i in task.DefaultIfEmpty()
                     join g in db.Users on b.CreatedUserId equals g.Id
                     join h in db.Suppliers on b.SupplierId equals h.SupplierID into task2
                     from h in task2.DefaultIfEmpty()
                     join k in db.Customers on b.Customer equals k.CustomerID into task3
                     from k in task3.DefaultIfEmpty()

                     let po = db.ConvertTransactionss.Where(ap => ap.From == b.MaterialRequisitionId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPO).FirstOrDefault()
                     let pq = db.ConvertTransactionss.Where(ap => ap.From == b.MaterialRequisitionId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPQ).FirstOrDefault()

                     // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) can't be translated by
                     // EF Core 10 inside this projection — computed CLIENT-side after materialization via lookups
                     // keyed by MaterialRequisitionId (same split as EstimateController.GetEstimate).
                     where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                   (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.MRDate, fdate) <= 0) &&
                   (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.MRDate, tdate) >= 0) &&
                   (Stats == "" || Stats == null || b.Status == st)
                   && (user == "" || user == "All" || g.Id == user) &&
                   (supplier == 0 || supplier == null || b.SupplierId == supplier) &&
                   (ProjectName == 0 || ProjectName == null  || f.ProjectId == ProjectName) &&
                   (Task == 0 || Task == null  || i.ProTaskId == Task)
                   && (userpermission == true || b.CreatedUserId == UserId) &&
                   (reqstatus == null || reqstatus == "All" || b.RequestStatus == reqstatus) &&
                    (ref1 == "" || ref1 == null || b.Ref1 == ref1) &&
                    (ref2 == "" || ref2 == null || b.Ref2 == ref2) &&
                    (ref3 == "" || ref3 == null || b.Ref3 == ref3) &&
                    (ref4 == "" || ref4 == null || b.Ref4 == ref4) &&
                    (ref5 == "" || ref5 == null || b.Ref5 == ref5) &&
                    (RequestedBy == null || RequestedBy == 0 || e.EmployeeId == RequestedBy) &&
                    (RemndrDate == null || RemndrDate == "" || EF.Functions.DateDiffDay(b.ReminderDate, rdate) >= 0)
                     select new
                     {
                         POConvert = (po != null) ? po.ConvertTo : "",
                         PQConvert = (pq != null) ? pq.ConvertTo : "",

                         b.MaterialRequisitionId,
                         b.MRNo,
                         b.BillNo,
                         b.MRDate,
                         b.MRItems,
                         b.MRItemQuantity,
                         b.MRValidity,
                         b.ReminderDate,
                         b.MRCashier,
                         b.Remarks,
                         b.Project,
                         ProjectName = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",

                         Task = (i.TaskName != null && i.TaskName != "") ? i.TaskCode + "-" + i.TaskName : "",
                         ProjectNames = (f.ProjectName != null && f.ProjectName != "") ? f.ProjectName:"",
                         ProjectCode = (f.ProCode != null )? f.ProCode : "",

                         EmpName = e.FirstName + " " + e.LastName,
                         SuppName=h.SupplierName,
                         Customer=k.CustomerName,
                         AccountId=(k.Accounts!=null) ? k.Accounts : 0,

                         
                         user = g.UserName,

                         //validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.MRDate, (b.MRValidity == null) ? 0 : b.MRValidity + 1)) ? "Active" : "Expired",
                         Dev = uDev,
                         Details = uMaterialRequisitionView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,

                         CreatedDate=b.MRCreatedDate
                     });

            // Performance (audit P2): server-side paging on the common path; search/computed sorts fall back unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "AccountId","BillNo","CreatedDate","Customer","Delete","Details","Dev","Download","Edit","EmpName","MaterialRequisitionId","MRCashier","MRDate","MRItemQuantity","MRItems","MRNo","MRValidity","POConvert","PQConvert","Project","ProjectCode","ProjectName","ProjectNames","Remarks","ReminderDate","SuppName","Task","user" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("MaterialRequisitionId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side approval lookups keyed by MaterialRequisitionId (missing key -> empty/absent, no KeyNotFound).
            var mrIds = serverRows.Select(o => o.MaterialRequisitionId).ToList();
            // app = approver EmployeeIds for the requisition (keyed by TransEntry == MaterialRequisitionId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "MaterialRequisition" && mrIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "MaterialRequisition" && mrIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per requisition.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.MaterialRequisitionId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.MaterialRequisitionId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.MaterialRequisitionId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {

                         o.POConvert,
                         o.PQConvert,
                         o.MaterialRequisitionId,
                         o.MRNo,
                         o.BillNo,
                         o.MRDate,
                         o.MRItems,
                         o.MRItemQuantity,
                         o.MRValidity,
                         o.ReminderDate,
                         o.Remarks,
                         o.Project,
                         o.ProjectName,
                         o.Task,
                         o.ProjectCode,
                         o.ProjectNames,
                         o.EmpName,
                         o.SuppName,
                         o.Customer,
                         o.AccountId,
                         o.MRCashier,
                         o.user,
                         //o.validity,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         MRToPQuot = (o.PQConvert != "" && MRToPQuots == Status.active) ? false : true,

                     };
                     });

            if (appstat != "")
            {
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Equals(search.ToLower()) 
               
                                 );
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

        //GET --- View forApproved Users
        [QkAuthorize(Roles = "Dev, Approvals")]
        public ActionResult ApprovalList()
        {
            ViewBag.SalesExecutive = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");

            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

            ViewBag.DropDowns = QkSelect.List(
                           new List<SelectListItem>
                           {
                            new SelectListItem { Selected = false, Text = "All", Value  = "All"},
                           }, "Value", "Text", 1);

            var MlaMc = db.EnableSettings.Where(a => a.EnableType == "MLAMc").FirstOrDefault();
            var MlaMcs = MlaMc != null ? MlaMc.Status : Status.inactive;
            ViewBag.MLAMc = MlaMcs;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            _FinancialYear();

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindMReqn").FirstOrDefault();
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
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            MaterialRequisitionViewModel vmodel = new MaterialRequisitionViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "MR" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "MR").ToList();

            var ref1 = db.MaterialRequisitions
            .Select(s => new
            {
                ID = s.Ref1,
                Name = s.Ref1
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.MaterialRequisitions
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            return View(vmodel);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev, Approvals")]
        public ActionResult GetMaterialRequisitionApproval(string BillNo, string FromDate, string ToDate, string Stats, long? RequestedBy, string user, string Validity, string appstat, long? ProjectName, long? Task, string RemndrDate, string ref1, string ref2, string ref3, string ref4, string ref5)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? rdate = null;

            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB"));
            }
            if (RemndrDate != "")
            {
                rdate = DateTime.Parse(RemndrDate, new CultureInfo("en-GB"));
            }

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var fromv = "MOrder";
            var ToPO = "POrder";
            var ToPQ = "PQuote";
            var MRToPQuot = db.EnableSettings.Where(a => a.EnableType == "MRToPQuot").FirstOrDefault();
            var MRToPQuots = MRToPQuot != null ? MRToPQuot.Status : Status.inactive;

            Status st = new Status();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };


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

            var userpermission = User.IsInRole("All MaterialRequisition Entry");
            var UserId = User.Identity.GetUserId();

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var uDev = User.IsInRole("Dev");
            var uMaterialRequisitionView = User.IsInRole("View MaterialRequisition");
            var uEdit = User.IsInRole("Edit MaterialRequisition");
            var uDownload = User.IsInRole("Download MaterialRequisition");
            var uDelete = User.IsInRole("Delete MaterialRequisition");

            var v = (from b in db.MaterialRequisitions
                     join e in db.Employees on b.MRCashier equals e.EmployeeId into emp
                     from e in emp.DefaultIfEmpty()
                     join f in db.Projects on b.Project equals f.ProjectId into prj
                     from f in prj.DefaultIfEmpty()
                     join i in db.ProTasks on b.ProTask equals i.ProTaskId into task
                     from i in task.DefaultIfEmpty()
                     join g in db.Users on b.CreatedUserId equals g.Id

                     let po = db.ConvertTransactionss.Where(ap => ap.From == b.MaterialRequisitionId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPO).FirstOrDefault()
                     let pq = db.ConvertTransactionss.Where(ap => ap.From == b.MaterialRequisitionId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPQ).FirstOrDefault()

                     let app = db.Approvals.Where(a => a.TransEntry == b.MaterialRequisitionId && a.Type == "MaterialRequisition").Select(a => a.EmployeeId).ToList()
                     let AppStatus = db.ApprovalUpdates.Where(a => a.TransEntry == b.MaterialRequisitionId && a.Type == "MaterialRequisition").Select(a => a.ApprovalStatus).ToList()
                     let chkAppStatus = db.ApprovalUpdates.Where(a => a.TransEntry == b.MaterialRequisitionId && a.Type == "MaterialRequisition").GroupBy(l => l.ApprovedBy)
                                        .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                        .ToList().Select(a => a.ApprovalStatus).ToList()

                     where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                   (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.MRDate, fdate) <= 0) &&
                   (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.MRDate, tdate) >= 0) &&
                   (Stats == "" || Stats == null || b.Status == st)
                   && (user == "" || user == "All" || g.Id == user) &&
                   (ProjectName == 0 || ProjectName == null || f.ProjectId == ProjectName) &&
                   (Task == 0 || Task == null || i.ProTaskId == Task)
                   && (userpermission == true || b.CreatedUserId == UserId) &&
                    (ref1 == "" || ref1 == null || b.Ref1 == ref1) &&
                    (ref2 == "" || ref2 == null || b.Ref2 == ref2) &&
                    (ref3 == "" || ref3 == null || b.Ref3 == ref3) &&
                    (ref4 == "" || ref4 == null || b.Ref4 == ref4) &&
                    (ref5 == "" || ref5 == null || b.Ref5 == ref5) &&
                    (RequestedBy == null || RequestedBy == 0 || e.EmployeeId == RequestedBy) &&
                    (RemndrDate == null || RemndrDate == "" || EF.Functions.DateDiffDay(b.ReminderDate, rdate) >= 0)
                     select new
                     {
                         POConvert = (po != null) ? po.ConvertTo : "",
                         PQConvert = (pq != null) ? pq.ConvertTo : "",

                         b.MaterialRequisitionId,
                         b.MRNo,
                         b.BillNo,
                         b.MRDate,
                         b.MRItems,
                         b.MRItemQuantity,
                         b.MRValidity,
                         b.ReminderDate,
                         b.MRCashier,
                         b.Remarks,
                         b.Project,
                         ProjectName = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",

                         Task = (i.TaskName != null && i.TaskName != "") ? i.TaskCode + "-" + i.TaskName : "",
                         ProjectNames = (f.ProjectName != null && f.ProjectName != "") ? f.ProjectName : "",
                         ProjectCode = (f.ProCode != null) ? f.ProCode : "",

                         EmpName = e.FirstName + " " + e.LastName,

                         user = g.UserName,

                         //validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.MRDate, (b.MRValidity == null) ? 0 : b.MRValidity + 1)) ? "Active" : "Expired",
                         Dev = uDev,
                         Details = uMaterialRequisitionView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,

                         app = app,
                         AppStatus = AppStatus,
                         chkAppStatus = chkAppStatus,
                         CreatedDate = b.MRCreatedDate
                     }).ToList().Select(o => new
                     {

                         o.POConvert,
                         o.PQConvert,
                         o.MaterialRequisitionId,
                         o.MRNo,
                         o.BillNo,
                         o.MRDate,
                         o.MRItems,
                         o.MRItemQuantity,
                         o.MRValidity,
                         o.ReminderDate,
                         o.Remarks,
                         o.Project,
                         o.ProjectName,
                         o.Task,
                         o.ProjectCode,
                         o.ProjectNames,
                         o.EmpName,
                         o.MRCashier,
                         o.user,
                         //o.validity,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.app,
                         Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         MRToPQuot = (o.PQConvert != "" && MRToPQuots == Status.active) ? false : true,

                     }).ToList().Select(a => new
                     {
                         a.POConvert,
                         a.PQConvert,
                         a.MaterialRequisitionId,
                         a.MRNo,
                         a.BillNo,
                         a.MRDate,
                         a.MRItems,
                         a.MRItemQuantity,
                         a.MRValidity,
                         a.ReminderDate,
                         a.Remarks,
                         a.Project,
                         a.ProjectName,
                         a.Task,
                         a.ProjectCode,
                         a.ProjectNames,
                         a.EmpName,
                         a.MRCashier,
                         a.user,
                         //o.validity,
                         a.Dev,
                         a.Details,
                         a.Edit,
                         a.Download,
                         a.Delete,
                         a.app,
                         a.Approval, 
                         a.ApprovalStatus,
                         a.CreatedDate,
                        a.MRToPQuot,
                     }).Where(a=>a.Approval == true);

            if (appstat != "")
            {
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Equals(search.ToLower())

                                 );
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


        [QkAuthorize(Roles = "Dev, MaterialRequisition Entry")]
        public ActionResult Create()
        {
            var MRentry = new MaterialRequisitionViewModel
            {
                BillNo = InvoiceNo(),
                MRDate       = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                MRValidity   = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                ReminderDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "mr").Select(a => a.TermsCondit).FirstOrDefault(),
            };

            companySet();

            var userpermission = User.IsInRole("All MaterialRequisition Entry");
            var UserId = User.Identity.GetUserId();

            ViewBag.LastEntry = db.MaterialRequisitions.Where(a => a.CreatedUserId == UserId || userpermission == true).Select(p => p.MaterialRequisitionId).AsEnumerable().DefaultIfEmpty(0).Max();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;
           
            var cust = db.Customers
                .Where(o => o.Type == CRMCustomerType.Customer)
              .Select(s => new
              {
                  CustomerID = s.CustomerID,
                  CustomerDetails = s.CustomerCode + " - " + s.CustomerName
              }).Take(1).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
            _FinancialYear();

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
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      ID = s.EmployeeId,
                      Name = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaMc = db.EnableSettings.Where(a => a.EnableType == "MLAMc").FirstOrDefault();
            var MlaMcs = MlaMc != null ? MlaMc.Status : Status.inactive;
            ViewBag.MLAMc = MlaMcs;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            ViewBag.PopUpAddCust = false;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //field mapping
            MRentry.FieldMap = db.FieldMappings.Where(a => a.Section == "MR" && a.Status == Status.active).ToList();

            var sup = db.Suppliers.Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierCode + " - " + s.SupplierName
            }).ToList();
            ViewBag.Supp = QkSelect.List(sup, "SupplierID", "SupplierDetails");

            var ref1 = db.MaterialRequisitions
            .Select(s => new
            {
                ID = s.Ref1,
                Name = s.Ref1
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.MaterialRequisitions
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");
            return View(MRentry);
        }

        [QkAuthorize(Roles = "Dev,MaterialRequisition Entry")]
        public JsonResult CreateMaterialRequisition(string[][] array, string[] mrdata, string action)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                if (!BillExist(Convert.ToString(mrdata[8])))
                {

                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(mrdata[10]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                    var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                    var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                    var MakeChk = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
                    var MakeChks = MakeChk != null ? MakeChk.Status : Status.inactive;

                    //sales entry
                    MaterialRequisition MRentry = new MaterialRequisition();


                    MRentry.MRNo = GetMRNo();
                    MRentry.BillNo = Convert.ToString(mrdata[8]);
                    MRentry.MRDate = DateTime.Parse(mrdata[1], new CultureInfo("en-GB"));
                    MRentry.MRCashier = mrdata[0] != "" ? Convert.ToInt64(mrdata[0]) : 0;
                    MRentry.SupplierId = mrdata[22] != "" ? Convert.ToInt64(mrdata[22]) : 0;
                    MRentry.Customer = mrdata[23] != "" ? Convert.ToInt64(mrdata[23]) : 0;

                    MRentry.MRItems = Convert.ToInt32(mrdata[2]);
                    MRentry.MRItemQuantity = Convert.ToDecimal(mrdata[3]);


                    MRentry.MRNote = "";
                    MRentry.Mail = 0;
                    MRentry.MRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    MRentry.CreatedUserId = UserId;
                    MRentry.Status = Status.active;

                    MRentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "MaterialRequisition").Select(a => a.EmailTemplateID).FirstOrDefault();
                    MRentry.CompanyHeaderID = 0;
                    MRentry.Branch = Branch;
                    MRentry.MRValidity = DateTime.Parse(mrdata[4], new CultureInfo("en-GB"));

                    if (mrdata[21] != "")
                        MRentry.ReminderDate = DateTime.Parse(mrdata[21], new CultureInfo("en-GB"));

                    MRentry.Remarks = mrdata[9];
                    MRentry.Project = mrdata[11] != "" ? Convert.ToInt64(mrdata[11]) : 0;
                    MRentry.ProTask = mrdata[12] != "" ? Convert.ToInt64(mrdata[12]) : 0;

                    MRentry.TermsCondition = Convert.ToString(mrdata[14]);

                    MRentry.Ref1 = Convert.ToString(mrdata[15]);
                    MRentry.Ref2 = Convert.ToString(mrdata[16]);
                    MRentry.Ref3 = Convert.ToString(mrdata[17]);
                    MRentry.Ref4 = Convert.ToString(mrdata[18]);
                    MRentry.Ref5 = Convert.ToString(mrdata[19]);
                    MRentry.RequestStatus= Convert.ToString(mrdata[24]);
                    db.MaterialRequisitions.Add(MRentry);
                    db.SaveChanges();
                    Int64 mrId = 0;
                    mrId = MRentry.MaterialRequisitionId;



                    ////// add to SEItem
                    string result = string.Empty;
                    DataTable dtItem = new DataTable();
                    dtItem.Columns.Add("ItemUnit");                    
                    dtItem.Columns.Add("ItemQuantity");
                    dtItem.Columns.Add("Item");
                    dtItem.Columns.Add("Make");
                    dtItem.Columns.Add("ItemNote");
                    dtItem.Columns.Add("MaterialRequisition");
                    dtItem.Columns.Add("ItemRemark");
                    dtItem.Columns.Add("TargetPrice");
                    foreach (var arr in array)
                    {
                        DataRow dr = dtItem.NewRow();
                        dr["ItemUnit"] = arr[1];                       
                        dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                        dr["Item"] = Convert.ToInt32(arr[0]);
                        if (MakeChks == Status.active)
                        {
                            dr["Make"] = arr[8] != null ? Convert.ToUInt64(arr[8]) : 0;
                        }
                        else
                        {
                            dr["Make"] = 0;
                        }

                        dr["ItemNote"] = Convert.ToString(arr[13].Replace("\n", "<br />"));
                        dr["MaterialRequisition"] = mrId;
                        dr["ItemRemark"] = Convert.ToString(arr[9].Replace("\n", "<br />"));
                        dr["TargetPrice"] = (arr[7] != "" && arr[7] != null)? Convert.ToDecimal(arr[7]) : 0;

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
                                              ItemUnit = a.ItemUnit,
                                              Item = a.ItemId
                                          }).ToList();
                            foreach (var bu in bundle)
                            {
                                var qua = (bunQuan * bu.quantity);                 
                                DataRow dbu = dtItem.NewRow();
                                dbu["ItemUnit"] = bu.ItemUnit;                                
                                dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                                dbu["Item"] = bu.Item;
                                dbu["Make"] = arr[8] != null ? Convert.ToUInt64(arr[8]) : 0;                               
                                dbu["itemNote"] = "-:{Bundle_Item}";
                                dbu["MaterialRequisition"] = mrId;
                                dbu["ItemRemark"] = arr[9] != null ? Convert.ToString(arr[9].Replace("\n", "<br />")) : "";
                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }

                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeMRItemsNew";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertMRItems", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql, parameter);


                    //Approved By
                    var Appby = Convert.ToString(mrdata[13]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = mrId;
                            approval.Type = "MaterialRequisition";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    /************************* Reminder ***************************/
                    if (mrdata[21] != "")
                    {
                        //Reminder for Reminder Date
                        db.Reminderss.Add(new Reminderss
                        {
                            CreatedBy   =   User.Identity.GetUserId(),
                            Note        =   "Material Requisition - " + MRentry.BillNo,
                            RDate       =   DateTime.Parse(mrdata[21], new CultureInfo("en-GB")),
                            CreatedDate =   System.DateTime.Now,
                            Reference   =   mrId,
                            Type        =   "Material Requisition",
                            actionurl   =   "MaterialRequisition/Edit/",
                            RequestBy   =   UserId,
                            Status      =   0,
                            RStatus     =   "Open"
                        });
                        db.SaveChanges();
                    }

                    com.addlog(LogTypes.Created, UserId, "MaterialRequisition", "MaterialRequisition", findip(), mrId, "Successfully Submitted Material Requisition");

                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                    if (action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "MR" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                        //sales return
                        object item = "";
                        object summary = "";                      
                        object cdetails = "";
                        object approval = "";
                        if (mrId != 0)
                        {
                            var MReq = com.MaterialRequisitionData(mrId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, BranchCheck, ComHeadCheck);
                            item = MReq["item"];
                            summary = MReq["summary"];                            
                            cdetails = MReq["cdetails"];
                            approval= MReq["approval"];
                        }
                        var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, cdetails, approval, layout, fmapp } };
                    }
           
                    else if (action == "sendmail")
                    {
                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = mrdata[5];
                        string CcMail = mrdata[6];
                        string InvoiceNo = "_MaterialRequisition_" + MRentry.MRNo;

                        var em = db.EmailTemplates.Where(a => a.Head == "PurchaseOrder").FirstOrDefault();//
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;
                        }
                        else
                        {
                            message.Subject = "MaterialRequisition";
                            message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                                " <p>we are enclosing our  Material requisition for the items / services as requested by you during our discussions.<br/></p> " +
                                " <p>Looking forward to hear from you.</p>";
                        }

                        sm.SendPdfMail(generatePdf(mrId), ToMail, CcMail, InvoiceNo, message);

                        msg = "Successfully Material Requisition.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    else
                    {
                        msg = "Successfully submitted Material Requisition.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }

                }
                else
                {
                    msg = "Material Requisition No. Already Exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }

            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        [QkAuthorize(Roles = "Dev,Edit MaterialRequisition")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All MaterialRequisition Entry");
            var UserId = User.Identity.GetUserId();
            MaterialRequisition MRequisition = db.MaterialRequisitions.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.MaterialRequisitionId == id).FirstOrDefault();

            if (MRequisition == null)
            {
                return NotFound();
            }

            MaterialRequisitionViewModel vmodel = new MaterialRequisitionViewModel();
           
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var cust = db.Customers
                .Where(o => o.Type == CRMCustomerType.Customer)
              .Select(s => new
              {
                  CustomerID = s.CustomerID,
                  CustomerDetails = s.CustomerCode + " - " + s.CustomerName
              }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");

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

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            var pr = db.Projects
                            .Select(s => new
                            {
                                ID = s.ProjectId,
                                Name = s.ProCode + "-" + s.ProjectName
                            })
                            .ToList();
            ViewBag.getProj = QkSelect.List(pr, "ID", "Name");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "MaterialRequisition").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);


            var MlaMc = db.EnableSettings.Where(a => a.EnableType == "MLAMc").FirstOrDefault();
            var MlaMcs = MlaMc != null ? MlaMc.Status : Status.inactive;
            ViewBag.MLAMc = MlaMcs;

            vmodel = (from b in db.MaterialRequisitions
                      where b.MaterialRequisitionId == id
                      select new MaterialRequisitionViewModel
                      {
                          MRNo = b.MRNo,
                          MRDate = b.MRDate,
                          BillNo = b.BillNo,
                          MRValidity = b.MRValidity,
                          ReminderDate = b.ReminderDate,
                          MRCashier = b.MRCashier,
                          Remarks = b.Remarks,
                          Branch = b.Branch,
                          SupplierId = b.SupplierId,
                          Customer = b.Customer,
                          Project = b.Project != null ? b.Project : null,
                          ProTask = b.ProTask,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          TermsCondition = b.TermsCondition,
                          Requeststat=b.RequestStatus
                      }).FirstOrDefault();
            companySet();

            ViewBag.preEntry = db.MaterialRequisitions.Where(a => (a.MaterialRequisitionId < id) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.MaterialRequisitionId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.MaterialRequisitions.Where(a => (a.MaterialRequisitionId > id) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.MaterialRequisitionId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            var EditPermission = User.IsInRole("Disable MR Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "MaterialRequisition", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "MR" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummyMaterialRequisitionItems.Where(a => a.MaterialRequisition == id).FirstOrDefault();
            var SItem = db.MaterialRequisitionItems.Where(a => a.MaterialRequisition == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummyMaterialRequisitionItems.Where(a => a.MaterialRequisition == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    MaterialRequisitionItem sItem = new MaterialRequisitionItem();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.Make = arr.Make;
                    sItem.ItemRemark = arr.ItemRemark;
                    sItem.ItemNote = arr.ItemNote;
                    sItem.MaterialRequisition = arr.MaterialRequisition;
                    sItem.Item = arr.Item;
                    db.MaterialRequisitionItems.Add(sItem);
                    db.SaveChanges();
                }

                db.DummyMaterialRequisitionItems.RemoveRange(db.DummyMaterialRequisitionItems.Where(a => a.MaterialRequisition == id));
                db.SaveChanges();
            }

            var supp = db.Suppliers
                       .Select(s => new
                       {
                           SupplierID = s.SupplierID,
                           SupplierDetails = s.SupplierID + " - " + s.SupplierName
                       }).ToList();
            ViewBag.Supp = QkSelect.List(supp, "SupplierID", "SupplierDetails");

            var ref1 = db.MaterialRequisitions
                .Select(s => new
                {
                    ID = s.Ref1,
                    Name = s.Ref1
                }).Distinct()
                .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", vmodel.Ref1);

            var ref2 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", vmodel.Ref2);

            var ref3 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", vmodel.Ref3);

            var ref4 = db.MaterialRequisitions
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", vmodel.Ref4);

            var ref5 = db.MaterialRequisitions
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", vmodel.Ref5);

            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Edit MaterialRequisition")]
        public JsonResult UpdateMaterialRequisition(string[][] array, string[] mrdata, string action)
        {
            bool stat = false;
            string msg;
            Int64 mrEntryId = Convert.ToInt64(mrdata[9]);
            MaterialRequisition mrentry = db.MaterialRequisitions.Find(mrEntryId);
            if (BillExist(Convert.ToString(mrdata[8])) && Convert.ToString(mrdata[8]) != mrentry.BillNo)
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            if (ModelState.IsValid)
            {
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;


                var UserId = User.Identity.GetUserId();
                var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(mrdata[11]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }


                var EditPermission = User.IsInRole("Disable MR Edit After Approval");
                if (com.chkApproved(mrEntryId, EditPermission, "MaterialRequisition", UserId) == true)
                {
                    //sales entry

                    mrentry.BillNo = mrdata[8];
                    mrentry.MRNo = Convert.ToInt64(mrdata[7]);
                    mrentry.MRDate = DateTime.Parse(mrdata[1], new CultureInfo("en-GB"));
                    mrentry.MRCashier = mrdata[0] != "" ? Convert.ToInt64(mrdata[0]) : 0;
                    mrentry.MRValidity = DateTime.Parse(mrdata[4], new CultureInfo("en-GB"));
                    mrentry.SupplierId = mrdata[23] != "" ? Convert.ToInt64(mrdata[23]) : 0;
                    mrentry.Customer = mrdata[24] != "" ? Convert.ToInt64(mrdata[24]) : 0;

                    if (mrdata[22] != "")
                        mrentry.ReminderDate = DateTime.Parse(mrdata[22], new CultureInfo("en-GB"));

                    mrentry.MRItems = Convert.ToInt32(mrdata[2]);
                    mrentry.MRItemQuantity = mrdata[3] != "" ? Convert.ToDecimal(mrdata[3]) : 0;

                    mrentry.MRNote = "";
                    mrentry.Mail = 0;
                    mrentry.Status = Status.active;

                    mrentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "PurchaseOrder").Select(a => a.EmailTemplateID).FirstOrDefault();
                    mrentry.CompanyHeaderID = 0;
                    mrentry.Branch = Branch;

                    mrentry.Remarks = mrdata[10];

                    mrentry.Project = mrdata[12] != "" ? Convert.ToInt64(mrdata[12]) : 0;
                    mrentry.ProTask = mrdata[13] != "" ? Convert.ToInt64(mrdata[13]) : 0;

                    mrentry.TermsCondition = Convert.ToString(mrdata[15]);

                    mrentry.Ref1 = Convert.ToString(mrdata[16]);
                    mrentry.Ref2 = Convert.ToString(mrdata[17]);
                    mrentry.Ref3 = Convert.ToString(mrdata[18]);
                    mrentry.Ref4 = Convert.ToString(mrdata[19]);
                    mrentry.Ref5 = Convert.ToString(mrdata[20]);
                    mrentry.RequestStatus= Convert.ToString(mrdata[25]);
                    db.Entry(mrentry).State = EntityState.Modified;
                    db.SaveChanges();

                    var MRItem = db.MaterialRequisitionItems.Where(a => a.MaterialRequisition == mrEntryId).FirstOrDefault();
                    if (MRItem != null)
                    {
                        var SItems = db.MaterialRequisitionItems.Where(a => a.MaterialRequisition == mrEntryId).ToList();
                        foreach (var arr in SItems)
                        {
                            //add to dummy table
                            DummyMaterialRequisitionItem  dItem = new DummyMaterialRequisitionItem();
                            dItem.ItemUnit = arr.ItemUnit;
                            dItem.ItemQuantity = arr.ItemQuantity;
                            dItem.Make = arr.Make;
                            dItem.ItemRemark = arr.ItemRemark;
                            dItem.ItemNote = arr.ItemNote;
                            dItem.MaterialRequisition = arr.MaterialRequisition;
                            dItem.Item = arr.Item;
                            dItem.TargetPrice = arr.TargetPrice;
                            db.DummyMaterialRequisitionItems.Add(dItem);
                            db.SaveChanges();
                        }

                        db.MaterialRequisitionItems.RemoveRange(db.MaterialRequisitionItems.Where(a => a.MaterialRequisition == mrEntryId));
                        db.SaveChanges();
                    }

                    var MakeChk = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
                    var MakeChks = MakeChk != null ? MakeChk.Status : Status.inactive;

                    ////// add to SEItem

                    string result = string.Empty;
                    DataTable dtItem = new DataTable();
                    dtItem.Columns.Add("ItemUnit");
                    dtItem.Columns.Add("ItemQuantity");
                    dtItem.Columns.Add("Item");
                    dtItem.Columns.Add("Make");
                    dtItem.Columns.Add("ItemNote");
                    dtItem.Columns.Add("MaterialRequisition");
                    dtItem.Columns.Add("ItemRemark");
                    dtItem.Columns.Add("TargetPrice");
                    foreach (var arr in array)
                    {
                        DataRow dr = dtItem.NewRow();
                        dr["ItemUnit"] = arr[1];
                        dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                        dr["Item"] = Convert.ToInt32(arr[0]);
                        if (MakeChks == Status.active)
                        {
                            dr["Make"] = arr[8] != null ? Convert.ToUInt64(arr[8]) : 0;
                        }
                        else
                        {
                            dr["Make"] = 0;
                        }
                        dr["ItemNote"] = Convert.ToString(arr[13].Replace("\n", "<br />"));
                        dr["MaterialRequisition"] = mrEntryId;
                        dr["ItemRemark"] = Convert.ToString(arr[9].Replace("\n", "<br />"));
                        dr["TargetPrice"] = (arr[7] != "" && arr[7] != null) ? Convert.ToDecimal(arr[7]) : 0;
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
                                              ItemUnit = a.ItemUnit,
                                              Item = a.ItemId
                                          }).ToList();
                            foreach (var bu in bundle)
                            {
                                var qua = (bunQuan * bu.quantity);

                                DataRow dbu = dtItem.NewRow();
                                dbu["ItemUnit"] = bu.ItemUnit;
                                dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                                dbu["Item"] = bu.Item;
                                dbu["Make"] = arr[8] != null ? Convert.ToUInt64(arr[8]) : 0;
                                dbu["itemNote"] = "-:{Bundle_Item}";
                                dbu["MaterialRequisition"] = mrEntryId;
                                dbu["ItemRemark"] = arr[9] != null ? Convert.ToString(arr[9].Replace("\n", "<br />")) : "";
                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }

                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeMRItemsNew";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertMRItems", "@TableType");
                    //// execute sql 
                    var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                    if (ret > 0)
                    {
                        db.DummyMaterialRequisitionItems.RemoveRange(db.DummyMaterialRequisitionItems.Where(a => a.MaterialRequisition == mrEntryId));
                        db.SaveChanges();
                    }


                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == mrEntryId && a.Type == "MaterialRequisition").FirstOrDefault();
                    var MrnPO = db.Approvals.Where(a => a.TransEntry == mrEntryId && a.Type == "MaterialRequisition").FirstOrDefault();
                    if (MrnPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == mrEntryId && a.Type == "MaterialRequisition"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == mrEntryId && a.Type == "MaterialRequisition"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = Convert.ToString(mrdata[14]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = mrEntryId;
                            approval.Type = "MaterialRequisition";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    /************************* Reminder ***************************/
                    if (mrdata[22] != "")
                    {
                        db.Reminderss.RemoveRange(db.Reminderss.Where(a => a.Reference == mrEntryId && a.Type == "Material Requisition"));
                        db.SaveChanges();
                   
                        //Reminder for Reminder Date
                        db.Reminderss.Add(new Reminderss
                        {
                            CreatedBy   =   UserId,
                            Note        =   "Material Requisition - " + mrentry.BillNo,
                            RDate       =   DateTime.Parse(mrdata[22], new CultureInfo("en-GB")),
                            CreatedDate =   System.DateTime.Now,
                            Reference   =   mrEntryId,
                            Type        =   "Material Requisition",
                            actionurl   =   "MaterialRequisition/Edit/",
                            RequestBy   =   UserId,
                            Status      =   0,
                            RStatus     =   "Open"
                        });
                        db.SaveChanges();
                    }

                    com.addlog(LogTypes.Updated, UserId, "MaterialRequisition", "MaterialRequisitions", findip(), mrEntryId, "Successfully Updated Material Requisition");

                }
                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "MR" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
                    //sales return
                    object item = "";
                    object summary = "";
                    object cdetails = "";
                    object approval = "";
                    if (mrEntryId != 0)
                    {
                        var MReq = com.MaterialRequisitionData(mrEntryId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, BranchCheck, ComHeadCheck);
                        item = MReq["item"];
                        summary = MReq["summary"];
                        cdetails = MReq["cdetails"];
                        approval = MReq["approval"];
                    }
                    var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, cdetails, approval, layout, fmapp } };
                }
              
                else if (action == "sendmail")
                {
                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = mrdata[12];
                    string CcMail = mrdata[13];
                    string InvoiceNo = "_MaterialRequisition_" + mrentry.MRNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "MaterialRequisition").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "MaterialRequisition";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our Material Requisition for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(mrEntryId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully Updated Material Requisition.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Successfully Updated Material Requisition.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,View MaterialRequisition")]
        public ActionResult Details(long? id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;          

            MaterialRequisitionViewModel vmodel = new MaterialRequisitionViewModel();
            vmodel = (from b in db.MaterialRequisitions
                      join e in db.Employees on b.MRCashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join f in db.Customers on b.Customer equals f.CustomerID into cust
                      from f in cust.DefaultIfEmpty()
                      where b.MaterialRequisitionId == id
                      select new MaterialRequisitionViewModel
                      {
                          MRNo = b.MRNo,
                          BillNo = b.BillNo,
                          MRDate = b.MRDate,
                          CustomerName = f.CustomerName,
                          AccountId = f.Accounts,
                          EmployeeName = e.FirstName + " " + e.LastName,
                          MRValidity = b.MRValidity,
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
                          GrandTotal = (decimal?)db.MaterialRequisitionItems.Where(a => a.MaterialRequisition == id).Select(a => (a.ItemQuantity * ((a.TargetPrice == null)? 0 : a.TargetPrice))).Sum()
                      }).FirstOrDefault();

            vmodel.MRItem = db.MaterialRequisitionItems.Where(a => a.MaterialRequisition == id && a.ItemNote != "-:{Bundle_Item}")
            .Select(b => new MRItemViewModel
            {
                ItemQuantity    =   b.ItemQuantity, 
                TargetPrice     =   (b.TargetPrice == null) ? 0 : b.TargetPrice,
                TotalPrice      =   b.ItemQuantity * ((b.TargetPrice == null) ? 0 : b.TargetPrice),
                ItemNote = b.ItemNote != null ? b.ItemNote : "",               
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.MaterialRequisitionItems
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              join bi in db.BundleItems on ab.Item equals bi.ItemId   
                              join mi in db.ItemBundles on bi.ItemBundle equals mi.ItemBundleId 
                              where ab.MaterialRequisition == id && ab.ItemNote == "-:{Bundle_Item}"
                              && mi.mainItem == b.Item
                              select new ItemDetailViewModel
                              {
                                  ItemCode      = bb.ItemCode,
                                  ItemName      = bb.ItemName,
                                  ItemUnit      = cb.ItemUnitName,
                                  ItemQuantity  = ab.ItemQuantity,
                                  TargetPrice   = (ab.TargetPrice == null)? 0 : ab.TargetPrice
                              }).ToList()
            }).ToList();
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "MR" && a.Status == Status.active).ToList();
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Delete MaterialRequisition")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All MaterialRequisition Entry");
            var UserId = User.Identity.GetUserId();
            MaterialRequisition MR = db.MaterialRequisitions.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.MaterialRequisitionId == id).FirstOrDefault();

            if (MR == null)
            {
                return NotFound();
            }
            return PartialView(MR);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete MaterialRequisition")]
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
                stat = DeleteMR(id);
                msg = "Successfully deleted MaterialRequisition.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete MaterialRequisition")]
        public ActionResult DeleteAllMaterialRequisition(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteMRequisition(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " MaterialRequisition", true);
            return RedirectToAction("Index", "MaterialRequisition");
        }
        private Boolean DeleteMRequisition(long saleId)
        {
            var Msg = chkDeleteWithMsg(saleId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteMR(saleId);
            }
        }
        private Boolean DeleteMR(long id)
        {
            var UserId = User.Identity.GetUserId();
            MaterialRequisition MR = db.MaterialRequisitions.Find(id);
            var pitem = db.MaterialRequisitionItems.Where(a => a.MaterialRequisition == id).FirstOrDefault();
            if (pitem != null)
            {
                db.MaterialRequisitionItems.RemoveRange(db.MaterialRequisitionItems.Where(a => a.MaterialRequisition == id));
            }

            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "MaterialRequisition").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "MaterialRequisition"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "MaterialRequisition").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "MaterialRequisition"));
            }

            //********************Reminders
            var Reminds = db.Reminderss.Where(a => a.Reference == id && a.Type == "Material Requisition").FirstOrDefault();

            if (Reminds != null)
            {
                db.Reminderss.Remove(Reminds);
                db.SaveChanges();
            }

            //********************AddedRemarks
            var Remarks = db.AddedRemarks.Where(a => a.TransactionId == id && a.TransactionType == "MaterialRequisition").FirstOrDefault();

            if (Remarks != null)
            {
                db.AddedRemarks.RemoveRange(db.AddedRemarks.Where(a => a.TransactionId == id && a.TransactionType == "MaterialRequisition"));
                db.SaveChanges();
            }

            db.MaterialRequisitions.Remove(MR);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "MaterialRequisition", "MaterialRequisitions", findip(), MR.MaterialRequisitionId, "Successfully Deleted Material Requisition");

            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext2 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "MOrder" && x.ConvertTo == "PQuote").FirstOrDefault();            
            if (Ext2 != null)
            {
                var inv = db.PurchaseQuotations.Where(x => x.PQuotationId == Ext2.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Purchase Quotation : " + inv + ".";
            }
            else
            {
                msg = null;
            }
            return msg;
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download MaterialRequisition")]
        public ActionResult Download(long id)
        {
            var Data = db.MaterialRequisitions.Where(s => s.MaterialRequisitionId == id).FirstOrDefault();

            var custname = db.Employees.Where(s => s.EmployeeId == Data.MRCashier).Select(a => a.FirstName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "MaterialRequisition" + "-" + custname + "-" + billno + ".pdf");
            
        }

        public StringBuilder generatePdf(long id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

            var MakeChk = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var MakeChks = MakeChk != null ? MakeChk.Status : Status.inactive;
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            var details = (from b in db.MaterialRequisitions
                           join e in db.Employees on b.MRCashier equals e.EmployeeId into emp
                           from e in emp.DefaultIfEmpty()
                           join p in db.Projects on b.Project equals p.ProjectId into prjct
                           from p in prjct.DefaultIfEmpty()
                           join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                           from t in ptask.DefaultIfEmpty()
                           join c in db.Branchs on b.Branch equals c.BranchID into branch
                           from c in branch.DefaultIfEmpty()
                           where b.MaterialRequisitionId == id
                           select new
                           {


                               BillNo = b.BillNo,
                               Date = b.MRDate,
                               Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                               Remarks = b.Remarks,
                               validity = b.MRValidity,
                               BillId = b.MRNo,
                               ProjectCheck = ProjectCheck,
                               PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                               TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                               BranchNameCode = (c.BranchName != null && c.BranchName != "") ? c.BranchCode + "-" + c.BranchName : "",
                           }).FirstOrDefault();

            var saleitem = (from b in db.MaterialRequisitionItems
                            join c in db.Items on b.Item equals c.ItemID

                            join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                            from d in scaffold.DefaultIfEmpty()
                            join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                            from e in punit.DefaultIfEmpty()

                            join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                            from g in bundle.DefaultIfEmpty()
                            join h in db.ItemBrands on b.Make equals h.ItemBrandID into brn
                            from h in brn.DefaultIfEmpty()
                            let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()

                            where b.MaterialRequisition == id && b.ItemNote != "-:{Bundle_Item}"
                            select new
                            {

                                ItemNote = b.ItemNote,
                                ItemQuantity = b.ItemQuantity,
                                ItemCode = c.ItemCode,
                                ItemName = c.ItemName,
                                ItemUnit = e.ItemUnitName,
                                PartNumber = c.PartNumber,
                                ItemRemark=b.ItemRemark,
                                CBM = d.CBM,
                                Weight = d.Weight,
                                img = img,
                                ItemDescription = c.ItemDescription,
                                KeepStock = c.KeepStock,
                                Make = h.ItemBrandName,
                                ItemID = b.Item,
                                FileName = db.ItemImages.Where(a => a.ItemID == b.Item).Select(a => a.FileName).FirstOrDefault(),
                                bundle = (from ay in db.BundleItems
                                          join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                          join bb in db.Items on ay.ItemId equals bb.ItemID
                                          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                          from dd in scaffold.DefaultIfEmpty()
                                          join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                          from eb in bpunit.DefaultIfEmpty()
                                          let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                          where az.mainItem == b.Item
                                          select new
                                          {
                                              Id = bb.ItemID,

                                              ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),

                                              ItemNote = "",
                                              ItemCode = bb.ItemCode,
                                              ItemName = bb.ItemName,
                                              ItemUnit = eb.ItemUnitName,
                                              PartNumber = bb.PartNumber,
                                              CBM = dd.CBM,
                                              Weight = dd.Weight,
                                              img = bimg,
                                              KeepStock = bb.KeepStock,
                                              Item = ay.ItemId,                                          
                                              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                              ItemUnitID = bb.ItemUnitID,
                                              SubUnitId = bb.SubUnitId,
                                              ItemArabic = bb.ItemArabic,
                                              ItemDescription = bb.ItemDescription
                                          }).ToList(),
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
            var ApprovedID = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "MaterialRequisition" && a.Status == 0).Select(a => a.ApprovedBy).FirstOrDefault();
            var ApprovedBy = db.Employees
            .Select(s => new
            {
                CName = s.FirstName,
                lastname=s.LastName,
                user = s.UserId

            }).Where(s => s.user == ApprovedID).FirstOrDefault();
            

            int SI = 1;
          
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    var OptionBranch = "";
                    if (BranchCheck == Status.active && details.BranchNameCode != "")
                    {
                        OptionBranch = "<tr><th> Branch </th><td style = 'font-size:14px;font-weight:normal;' >: " + details.BranchNameCode + "</td ></tr >";
                    }
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>MaterialRequisition</b></td></tr></table>");

                    if (ProjectCheck == Status.active && details.PrjNameCode!="")
                    { 
                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                       "<td width='50%'> " +
                       "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Requested Date</b></i></th></tr><tr><td> " + details.validity.ToString("dd-MM-yyyy") + "</td></tr><tr><th><i><b>Project and Task</b></i></th></tr><tr><td> " + details.PrjNameCode + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + details.TaskName + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +
                       "<table  style='border: 0px; width: 100 %;'><tr><th>Bill No</th><td style='font-size:14px;font-weight:normal;'>: " + details.BillNo + "</td></tr><tr><th>Date تاريخ</th><td style='font-size:14px;font-weight:normal;'>: " + details.Date.ToString("dd-MM-yyyy") + "</td></tr><tr><th>Requested By</th><td style='font-size:14px;font-weight:normal;'>: " + details.Cashier + "</td></tr>" + OptionBranch+ "</table></td></tr></table>";
                    sb.Append(partyDetails);
                    }
                    else
                    {
                        string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                       "<td width='50%'> " +
                       "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Requested Date</b></i></th></tr><tr><td> " + details.validity.ToString("dd-MM-yyyy") + "</td></tr><tr><th><i><b></b></i></th></tr><tr><td></td></tr><tr><td style='font-size:14px;font-weight:normal;'></td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +
                       "<table  style='border: 0px; width: 100 %;'><tr><th>Bill No</th><td style='font-size:14px;font-weight:normal;'>: " + details.BillNo + "</td></tr><tr><th>Date تاريخ</th><td style='font-size:14px;font-weight:normal;'>: " + details.Date.ToString("dd-MM-yyyy") + "</td></tr><tr><th>Requested By</th><td style='font-size:14px;font-weight:normal;'>: " + details.Cashier + "</td></tr> " + OptionBranch + " </table></td></tr></table>";
                        sb.Append(partyDetails);
                    }
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
                    if (MakeChks == Status.active)
                    {
                        sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Make</th>");
                    }
                    sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Remark</th>");

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
                        if (item.bundle != null && item.bundle.Count > 0)
                        {
                            var desc = "<br/>[<span class='descr' data-name='Note'>";
                            foreach (var itemss in item.bundle)
                            {
                                desc += itemss.ItemName;
                                desc += " - " + (itemss.ItemQuantity) + " ";
                                desc += (itemss.ItemUnit != null) ? itemss.ItemUnit: "";
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
                            sb.Append(itemnote);
                            sb.Append("</div>");
                            sb.Append("</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemUnit + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemQuantity + "</td>");
                            if (MakeChks == Status.active)
                            {
                                sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.Make + "</td>");
                            }
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemRemark+ "</td>");

                        }
                        sb.Append("</tr>");
                        itemcount++;
                    }
                    sb.Append("</tbody>");
                    sb.Append("</table>");

                    
                    sb.Append("<table width='100%' style='border-collapse: collapse;border: .1px solid #ccc;font-size: 14px;'>");
                    string discount = "";
                    int count = 2;
                
                      discount += "<td style='border: .1px solid #ccc;padding: 10px;'><span style='direction:ltr'></span> </td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'></td></tr>";
   

                    string word = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;font-size: 15px;' colspan='5'><strong></strong></td><td style='border: .1px solid #ccc;padding: 10px;'></td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'></td></tr>";
                    string tc = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;' colspan='5' rowspan='" + count + "'> </td>";

                    string remarks = "";
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
                    if (ApprovedBy!=null)
                    {
                        sb.Append("<tr>");
                        sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");            
                        sb.Append("Approved BY " + ApprovedBy.CName +  "" + ApprovedBy.lastname+ "");                   
                        sb.Append("</td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</table>");
                }
            }
            return sb;
        }

        [HttpGet]
        public ActionResult GetMRItems(long MReqID)
        {
           var ConD = (from a in db.MaterialRequisitionItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemBrands on a.Make equals e.ItemBrandID into brn
                        from e in brn.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into tax
                        from f in tax.DefaultIfEmpty()
                        join g in db.MaterialRequisitions on a.MaterialRequisition equals g.MaterialRequisitionId into MR
                        from g in MR.DefaultIfEmpty()
                        join p in db.Projects on g.Project equals p.ProjectId into proj
                        from p in proj.DefaultIfEmpty()
                        join t in db.ProTasks on g.ProTask equals t.ProTaskId into protask
                        from t in protask.DefaultIfEmpty()
                        where a.MaterialRequisition == MReqID && a.ItemNote != "-:{Bundle_Item}"
                        select new
                        {
                            a.Item,
                            a.ItemQuantity,
                            a.ItemUnit,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",

                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ItemMake = a.Make,
                            ItemRemark = a.ItemRemark != null ? a.ItemRemark : "",

                            ItemMakeName = e.ItemBrandName,
                            ItemUnitPrice = a.TargetPrice,
                            ItemSubTotal = a.ItemQuantity * b.SellingPrice,
                            ItemTaxAmount=0,
                            ItemDiscount=0,
                            f.Percentage,

                            ProjectId = g.Project,
                            TaskId = g.ProTask,
                            p.ProjectName,
                            t.TaskName

                        }).ToList().Select(o => new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.note,
                            o.ItemNote,

                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.PriUnit,
                            o.SubUnit,
                            o.ItemMake,
                            o.ItemMakeName,
                            o.ItemRemark,

                            o.Percentage,
                            o.ItemUnitPrice,
                            o.ItemSubTotal,
                            ItemTax = o.Percentage,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            ItemTotalAmount = o.ItemSubTotal,
                            o.ProjectId,
                            o.TaskId,
                            o.ProjectName,
                            o.TaskName
                        });


            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }


        private string InvoiceNo(Int64 PNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "MaterialRequisition").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "MaterialRequisition").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.MaterialRequisitions.Select(p => p.MRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PNo = db.MaterialRequisitions.Max(p => p.MRNo + 1);
                    billNo = companyPrefix + PNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(PNo, billNo);
                    }
                }
            }
            else
            {
                PNo = PNo + 1;
                billNo = companyPrefix + PNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(PNo, billNo);
                }

            }
            return billNo;
        }

        private bool BillExist(string QENo)
        {
            var Exists = db.MaterialRequisitions.Any(c => c.BillNo == QENo);
            bool res = (Exists) ? true : false;
            return res;
        }

        private long GetMRNo()
        {
            Int64 PNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "MaterialRequisition").Select(a => a.number).FirstOrDefault();
            if ((db.MaterialRequisitions.Select(p => p.MRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                PNo = (number == 0) ? 1 : number;
            }
            else
            {
                PNo = db.MaterialRequisitions.Max(p => p.MRNo + 1);
            }

            return PNo;
        }

        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "MaterialRequisition" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "MaterialRequisition").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = MR.CreatedUserId;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "MaterialRequisition";

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
                            join d in db.MaterialRequisitions on b.TransEntry equals d.MaterialRequisitionId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedUserId equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "MaterialRequisition"
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

        //Get: View of Adding Remarks
        public ActionResult AddUpdates(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MaterialRequisition Obj = db.MaterialRequisitions.Find(id);

            if (Obj == null)
            {
                return NotFound();
            }
            var Remark = new AddedRemarks
            {
                TransactionId   = Obj.MaterialRequisitionId,
                TransactionType =   "MaterialRequisition"
            };

            return PartialView(Remark);
        }

        //Saving of Remarks
        [HttpPost]
        public ActionResult AddUpdates(AddedRemarks RequisitionRemark, int? id)
        {
            Int64 RequisitionId = RequisitionRemark.TransactionId;

            if (ModelState.IsValid)
            {
                if (RequisitionRemark.Remarks != null)
                {
                    Common com = new Common();
                    var UserId = User.Identity.GetUserId();
                    var Today = Convert.ToDateTime(System.DateTime.Now);

                    AddedRemarks Obj = new AddedRemarks
                    {
                        TransactionId = RequisitionRemark.TransactionId,
                        TransactionType =   "MaterialRequisition",
                        Remarks = RequisitionRemark.Remarks,
                        AddedUser = UserId,
                        CreatedDate = Today,
                    };
                    db.AddedRemarks.Add(Obj);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "MaterialRequistion", "AddedRemarks", findip(), RequisitionId, "Remarks Added Successfully..");
                    Success("Remarks added successfully...", true);
                }
            }
            else
            {
                Danger("Failed to add Remarks...", true);
            }
            return RedirectToAction("Index", "MaterialRequisition");
        }

        //Function to list the Remarks from table CustomerRemarks
        [HttpPost]
        public ActionResult GetAllRemarks(long? RequisitionId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            DateTime rmdate = System.DateTime.Now.AddDays(-30);

            var v = (from a in db.AddedRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     where a.TransactionId == RequisitionId && a.TransactionType == "MaterialRequisition" && a.Remarks != null && a.CreatedDate >= rmdate
                     orderby a.CreatedDate descending

                     select new
                     {
                         id = a.RemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remarks,
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = v.Count();
            var data = v.ToList();

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
