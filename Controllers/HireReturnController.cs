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
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class HireReturnController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public HireReturnController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: HireReturn 
        [QkAuthorize(Roles = "Dev,HireReturn List")]
        public ActionResult Index()
        {
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

            ViewBag.Invoice = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                               }, "Value", "Text", 1);

            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

            var MlaHReturn = db.EnableSettings.Where(a => a.EnableType == "MLAHReturn").FirstOrDefault();
            var MlaHReturns = MlaHReturn != null ? MlaHReturn.Status : Status.inactive;
            ViewBag.MLAHReturn = MlaHReturns;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindHReturn").FirstOrDefault();
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

            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,HireReturn List")]
        public ActionResult GetHireReturn(string BillNo, string FromDate, string ToDate, long? customer, long? salesperson, string user, long? invoice, string appstat, long? ProjectName, long? Task)
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

            var fromv = "DVNote";
            var Tosales = "Sale";

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

            var userpermission = User.IsInRole("All HireReturn Entry");
            var UserId = User.Identity.GetUserId();

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uHireReturnView = User.IsInRole("View HireReturn");
            var uEdit = User.IsInRole("Edit HireReturn");
            var uDownload = User.IsInRole("Download HireReturn");
            var uDelete = User.IsInRole("Delete HireReturn");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from b in db.HireReturns
                     join a in db.Customers on b.Customer equals a.CustomerID
                     join d in db.Employees on b.Cashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.SalesEntrys on b.Invoice equals e.SalesEntryId into sale
                     from e in sale.DefaultIfEmpty()
                     join g in db.Users on b.CreatedUserId equals g.Id
                     join j in db.Projects on b.Project equals j.ProjectId into prj
                     from j in prj.DefaultIfEmpty()
                     join k in db.ProTasks on b.ProTask equals k.ProTaskId into task
                     from k in task.DefaultIfEmpty()

                     let qs = db.ConvertTransactionss.Where(ap => ap.From == b.HireReturnId && ap.ConvertFrom == fromv && ap.ConvertTo == Tosales).FirstOrDefault()
                     let mc = db.MCs.Where(x => x.AssignedUser == b.CreatedUserId).Select(x => x.MCId).FirstOrDefault()

                     let app = db.Approvals.Where(x => x.TransEntry == b.HireReturnId && x.Type == "HireReturn").Select(x => x.EmployeeId).ToList()
                     let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == b.HireReturnId && x.Type == "HireReturn").Select(x => x.ApprovalStatus).ToList()
                     let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == b.HireReturnId && x.Type == "HireReturn").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                        .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                        .Select(x => x.ApprovalStatus).ToList()

                     where (BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                     (customer == null || customer == 0 || a.CustomerID == customer) &&
                     (salesperson == null || salesperson == 0 || d.EmployeeId == salesperson) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0) &&
                     (user == null || user == "" || g.Id == user) &&
                     (mc == 0 || mc == b.MaterialCenter) &&
                     (invoice == null || invoice == b.Invoice) &&
                     (userpermission == true || b.CreatedUserId == UserId)
                      && (ProjectName == 0 || ProjectName == null || j.ProjectId == ProjectName)
                     && (Task == 0 || Task == null || k.ProTaskId == Task)
                     select new
                     {
                         SaleConvert = (qs != null) ? qs.ConvertTo : "",
                         b.HireReturnId,
                         b.HrNo,
                         b.BillNo,
                         b.Date,
                         b.Items,
                         b.ItemQuantity,
                         EmpName = d.FirstName + " " + d.LastName,
                         Customer = a.CustomerCode + " - " + a.CustomerName,
                         User = g.UserName,
                         // EF Core can't translate the EF6 SqlFunctions.StringConvert shim inside a correlated
                         // subquery; long.ToString() translates to CONVERT(varchar,...) = STR(..).Trim() for ids.
                         convertSale = db.SalesEntrys.Where(ab => ab.ConvertType == "DVNote" && ab.ConvertNo == b.HireReturnId.ToString()).Select(ab => ab.BillNo).FirstOrDefault(),
                         test = b.HireReturnId.ToString(),
                         a.Remark,
                         Invoice = e.BillNo,
                         Dev = uDev,
                         Details = uHireReturnView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,

                         app = app,
                         AppStatus = AppStatus,
                         chkAppStatus = chkAppStatus,
                         ProjectName = (j.ProjectName != null && j.ProjectName != "") ? j.ProCode + "-" + j.ProjectName : "",
                         Task = (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",
                         b.CreatedDate
                     }).ToList().Select(o => new
                     {

                         o.SaleConvert ,
                         o.HireReturnId,
                         o.HrNo,
                         o.BillNo,
                         o.Date,
                         o.Items,
                         o.ItemQuantity,
                         o.EmpName ,
                         o.Customer ,
                         o.User ,
                         o.convertSale ,
                         o.test ,
                         o.Remark,
                         o.Invoice,
                         o.Dev ,
                         o.Details ,
                         o.Edit ,
                         o.Download ,
                         o.Delete ,
                         o.app,
                         Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.ProjectName,
                         o.Task,
                         o.CreatedDate
                     });
            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower()));
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

        [QkAuthorize(Roles = "Dev,HireReturn Entry")]
        public ActionResult Create(long? id, string type)
        {

            var userpermission = User.IsInRole("All HireReturn Entry");
            var UserId = User.Identity.GetUserId();

            var entry = new HireReturnViewModel
            {
                BillNo = InvoiceNo(),
                Date = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "dvnote").Select(a => a.TermsCondit).FirstOrDefault()

            };
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            if (id != null)
            {
                if (type == "Quote")
                {
                    Quotation quentry = db.Quotations.Find(id);

                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    entry.ConTypeId = quentry.QuotationId;
                    entry.ConType = type;
                    entry.Date = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    entry.Cashier = quentry.QuotCashier;
                    entry.Customer = quentry.Customer;
                    entry.Remarks = quentry.Remarks;
                    entry.Branch = quentry.Branch;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        entry.Project = quentry.Project;
                        entry.ProTask = quentry.ProTask;
                    }
                    entry.TermsCondition = quentry.TermsCondition;

                    //entry.convertFrom = type + " No";//label
                }
                if (type == "ProForma")
                {
                    ProForma pfentry = db.ProFormas.Find(id);

                    if (pfentry == null)
                    {
                        return NotFound();
                    }
                    entry.ConTypeId = pfentry.ProFormaId;
                    entry.ConType = type;
                    entry.Date = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    entry.Cashier = pfentry.PFCashier;
                    entry.Customer = pfentry.Customer;
                    entry.Remarks = pfentry.Remarks;
                    entry.Branch = pfentry.Branch;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        entry.Project = pfentry.Project;
                        entry.ProTask = pfentry.ProTask;
                    }
                    entry.TermsCondition = pfentry.PFNote;
                    //entry.convertFrom = type + " No";//label
                }

            }

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            companySet();


            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.LastEntry = db.HireReturns.Where(p => MCArray.Contains(p.MaterialCenter) && (userpermission == true || p.CreatedUserId == UserId)).Select(p => p.HireReturnId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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

            var RemoveItemData = db.EnableSettings.Where(a => a.EnableType == "RemoveItemData").FirstOrDefault();
            var CheckItemData = RemoveItemData != null ? RemoveItemData.Status : Status.inactive;
            ViewBag.ItemDataCheck = CheckItemData;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
             var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            //         equals new { b1 = b.ConvertFrom, b2 = b.From } into conv
            //         into emp from d in emp.DefaultIfEmpty()
            //         where (a.SaleType == SaleType.Hire) && (a.SalesEntryId != b.From) && (a.SalesEntryId != d.HireReturnId)
            //             ID = a.SalesEntryId,
            //             Name = a.BillNo
            //         })

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

            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      ID = s.EmployeeId,
                      Name = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaHReturn = db.EnableSettings.Where(a => a.EnableType == "MLAHReturn").FirstOrDefault();
            var MlaHReturns = MlaHReturn != null ? MlaHReturn.Status : Status.inactive;
            ViewBag.MLAHReturn = MlaHReturns;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            entry.FieldMap = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Status == Status.active).ToList();

            return View(entry);
        }

        [QkAuthorize(Roles = "Dev,HireReturn Entry")]
        public JsonResult CreateHireReturn(string[][] array, string[] dvdata, string action)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                if (!BillExist(Convert.ToString(dvdata[9])))
                {
                    Int64 dvId = 0;
                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
                    Int64 HrNo = 0;
                    using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTran = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var UserId = User.Identity.GetUserId();
                            long Branch = 0;
                            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                            if (BranchCheck == Status.active)
                            {
                                Branch = Convert.ToInt64(dvdata[12]);
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
                                MC = Convert.ToInt32(dvdata[11]);
                            }
                            else
                            {
                                MC = 1;
                            }
                            var Today = System.DateTime.Now;
                            var EmailTemp = db.EmailTemplates.Where(a => a.Head == "HireReturn").Select(a => a.EmailTemplateID).FirstOrDefault();

                            //sales entry
                            HrNo = GetNo();
                            HireReturn entry = new HireReturn();
                            entry.HrNo = HrNo;
                            entry.BillNo = Convert.ToString(dvdata[9]);
                            entry.Customer = Convert.ToInt64(dvdata[0]);
                            entry.Cashier = dvdata[1] != "" ? Convert.ToInt64(dvdata[1]) : 0;
                            entry.Date = DateTime.Parse(dvdata[2], new CultureInfo("en-GB"));
                            entry.Items = Convert.ToInt32(dvdata[3]);
                            entry.ItemQuantity = Convert.ToDecimal(dvdata[4]);
                            entry.TermsCondition = Convert.ToString(dvdata[5]);
                            entry.Remarks = dvdata[10];
                            entry.RtType = "Return";
                            entry.MaterialCenter = MC;
                            entry.Note = "";
                            entry.Mail = 0;
                            entry.CreatedDate = Today;
                            entry.CreatedUserId = UserId;
                            entry.Status = Status.active;
                            entry.EmailTemplateID = EmailTemp;
                            entry.CompanyHeaderID = 0;
                            entry.Branch = Branch;
                            entry.Invoice = Convert.ToInt32(dvdata[14]);
                            entry.Project = dvdata[15] != "" ? Convert.ToInt64(dvdata[15]) : 0;
                            entry.ProTask = dvdata[16] != "" ? Convert.ToInt64(dvdata[16]) : 0;

                            entry.Ref1 = Convert.ToString(dvdata[18]);
                            entry.Ref2 = Convert.ToString(dvdata[19]);
                            entry.Ref3 = Convert.ToString(dvdata[20]);
                            entry.Ref4 = Convert.ToString(dvdata[21]);
                            entry.Ref5 = Convert.ToString(dvdata[22]);

                            db.HireReturns.Add(entry);
                            db.SaveChanges();
                            dvId = entry.HireReturnId;



                            ////// add to SEItem
                            string result = string.Empty;
                            DataTable dtItem = new DataTable();
                            dtItem.Columns.Add("ItemUnit");
                            dtItem.Columns.Add("ItemUnitPrice");
                            dtItem.Columns.Add("ItemQuantity");
                            dtItem.Columns.Add("ItemDiscount");
                            dtItem.Columns.Add("ItemNote");

                            dtItem.Columns.Add("ReceivedQty");
                            dtItem.Columns.Add("DamageQty");
                            dtItem.Columns.Add("MissingQty");


                            dtItem.Columns.Add("Hr");
                            dtItem.Columns.Add("Item");


                            foreach (var arr in array)
                            {
                                DataRow dr = dtItem.NewRow();

                                dr["ItemUnit"] = (arr[1] == null || arr[1] == "") ? null : (long?)Convert.ToInt64(arr[1]);
                                dr["ItemUnitPrice"] = Convert.ToDecimal(arr[6]);
                                dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                                dr["ItemDiscount"] = Convert.ToDecimal(arr[11]);
                                dr["itemNote"] = Convert.ToString(arr[9].Replace("\n", "<br />"));

                                dr["ReceivedQty"] = arr[3] != "" ? Convert.ToDecimal(arr[3]) : 0;
                                dr["DamageQty"] = arr[4] != "" ? Convert.ToDecimal(arr[4]) : 0;
                                dr["MissingQty"] = arr[5] != "" ? Convert.ToDecimal(arr[5]) : 0;

                                dr["Hr"] = dvId;
                                dr["Item"] = Convert.ToInt32(arr[0]);

                                dtItem.Rows.Add(dr);
                            }

                            ////// create parameter 
                            SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                            parameter.SqlDbType = SqlDbType.Structured;
                            parameter.TypeName = "TableTypeHrItems";
                            //// execute sp sql 
                            string sql = String.Format("EXEC {0} {1};", "SP_InsertHrItems", "@TableType");
                            //// execute sql 
                            db.Database.ExecuteSqlRaw(sql, parameter);
                            //Approved By
                            var Appby = Convert.ToString(dvdata[17]);
                            if (Appby != null && Appby != "")
                            {
                                long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                                Approval approval = new Approval();
                                foreach (var emp in Approve)
                                {
                                    approval.TransEntry = dvId;
                                    approval.Type = "HireReturn";
                                    approval.EmployeeId = emp;
                                    db.Approvals.Add(approval);
                                    db.SaveChanges();
                                }
                            }

                            com.addlog(LogTypes.Created, UserId, "HireReturn", "HireReturns", findip(), dvId, "Successfully Submitted HireReturn");


                            //send mail to company address
                            var sendmail = db.EnableSettings.Where(a => a.EnableType == "AutomaticMailInTransactions").FirstOrDefault();
                            var sendcmail = sendmail != null ? sendmail.Status : Status.inactive;
                            if (sendcmail == Status.active)
                            {
                                var custname = db.Customers.Where(a => a.CustomerID == entry.Customer).Select(a => a.CustomerName).FirstOrDefault();
                                var salesman = db.Employees.Where(a => a.EmployeeId == entry.Cashier).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
                                var username = db.Users.Where(a => a.Id == entry.CreatedUserId).Select(a => a.UserName).FirstOrDefault();

                                var totrec = db.HrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.ReceivedQty).Sum() ?? 0;
                                var totdam = db.HrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.DamageQty).Sum() ?? 0;
                                var totmis = db.HrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.MissingQty).Sum() ?? 0;

                                CompanyEmailFormat CEmail = new CompanyEmailFormat();
                                CEmail.BillNo = "Hire Return-" + entry.BillNo;
                                CEmail.Subject = "<table style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;' colspan='2' align='center'><b>Hire Return Created</b></td><tr/> " +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Date               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.Date.ToString("dd-MM-yyyy") + "</td><tr/>" +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Customer           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + custname + "</td><tr/>" +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Sales Executive    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + salesman + "</td><tr/>" +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created Date       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.CreatedDate.ToString("dd-MM-yyyy hh:mm:ss tt") + "</td><tr/>" +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created User       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + username + "</td><tr/>" +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Against Invoice    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.Invoice + "</td><tr/>" +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Received     :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totrec + "</td><tr/>" +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Damage       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totdam + "</td><tr/>" +
                                        "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Missing      :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totmis + "</td><tr/></table>";

                                com.SendToCompanyMail(CEmail);
                            }




                            dbTran.Commit();
                        }
                        catch (Exception ex)
                        {
                            dbTran.Rollback();
                            msg = "Failed to Submit Return Note. " + ex.Message;
                            stat = false;
                            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                        }
                    }
              
                    if (action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;


                        Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                        TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                        var HData = com.HireReturnData(dvId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                        var item = HData.pdfItem.ToList();
                        var summary = HData;

                        var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout, fmapp } };
                    }
                    else if (action == "sendmail")
                    {
                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = dvdata[6];
                        string CcMail = dvdata[7];
                        string InvoiceNo = "_HireReturn_" + HrNo;

                        var em = db.EmailTemplates.Where(a => a.Head == "HireReturn").FirstOrDefault();
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;
                        }
                        else
                        {
                            message.Subject = "RETURN NOTE";
                            message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                                " <p>we are enclosing our delivery note for the items / services as requested by you during our discussions.<br/></p> " +
                                " <p>Looking forward to hear from you.</p>";
                        }
                        sm.SendPdfMail(generatePdf(dvId), ToMail, CcMail, InvoiceNo, message);

                        msg = "Successfully submitted Return Note.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    else
                    {
                        msg = "Successfully submitted Return Note.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                }
                else
                {
                    msg = "Return Note No. Already Exists.";
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


        [QkAuthorize(Roles = "Dev,Edit HireReturn")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All HireReturn Entry");
            var UserId = User.Identity.GetUserId();
            HireReturn dvnote = db.HireReturns.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.HireReturnId == id).FirstOrDefault();

            if (dvnote == null)
            {
                return NotFound();
            }
            HireReturnViewModel vmodel = new HireReturnViewModel();
            var cust = db.Customers
                .Select(s => new
                {
                    CustomerID = s.CustomerID,
                    CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashiers = QkSelect.List(use, "ID", "Name");

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



            vmodel = (from b in db.HireReturns
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.SalesEntrys on b.Invoice equals f.SalesEntryId into sale
                      from f in sale.DefaultIfEmpty()
                      where b.HireReturnId == id
                      select new HireReturnViewModel
                      {
                          HrNo = b.HrNo,
                          Customer = b.Customer,
                          Date = b.Date,
                          BillNo = b.BillNo,
                          Cashier = b.Cashier,
                          TermsCondition = b.TermsCondition,
                          custEmailId = e.EmailId,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          RtType = b.RtType,
                          Invoice = b.Invoice,
                          InvoiceNo = f.BillNo,
                          Project = b.Project,
                          ProTask = b.ProTask,
                          //convertBill = CBill,
                          //convertFrom = CType,
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

            ViewBag.preEntry = db.HireReturns.Where(a => a.HireReturnId < id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.HireReturnId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.HireReturns.Where(a => a.HireReturnId > id && MCArray.Contains(a.MaterialCenter) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.HireReturnId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var RemoveItemData = db.EnableSettings.Where(a => a.EnableType == "RemoveItemData").FirstOrDefault();
            var CheckItemData = RemoveItemData != null ? RemoveItemData.Status : Status.inactive;
            ViewBag.ItemDataCheck = CheckItemData;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

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

            ViewBag.PopUpAddCust = false;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "HireReturn").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaHReturn = db.EnableSettings.Where(a => a.EnableType == "MLAHReturn").FirstOrDefault();
            var MlaHReturns = MlaHReturn != null ? MlaHReturn.Status : Status.inactive;
            ViewBag.MLAHReturn = MlaHReturns;

            var EditPermission = User.IsInRole("Disable HReturn Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "HireReturn", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Edit HireReturn")]
        public JsonResult UpdateHireReturn(string[][] array, string[] dvdata, string action)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                Int64 dvId = Convert.ToInt64(dvdata[14]);
                HireReturn entry = db.HireReturns.Find(dvId);

                if (BillExist(Convert.ToString(dvdata[9])) && Convert.ToString(dvdata[9]) != entry.BillNo)
                {
                    msg = "Invoice No. Already Exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
                Int64 HrNo = 0;
                using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var UserId = User.Identity.GetUserId();
                        var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                        var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                        long Branch = 0;

                        var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                        var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                        if (BranchCheck == Status.active)
                        {
                            Branch = Convert.ToInt64(dvdata[12]);
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
                            MC = Convert.ToInt32(dvdata[11]);
                        }
                        else
                        {
                            MC = 1;
                        }

                        var EditPermission = User.IsInRole("Disable HReturn Edit After Approval");
                        if (com.chkApproved(dvId, EditPermission, "HireReturn", UserId) == true)
                        {
                            entry.HrNo = HrNo;
                            entry.BillNo = Convert.ToString(dvdata[9]);
                            entry.Customer = Convert.ToInt64(dvdata[0]);
                            entry.Cashier =(dvdata[1] != null && dvdata[1] != "") ? Convert.ToInt64(dvdata[1]) : 0;
                            entry.Date = DateTime.Parse(dvdata[2], new CultureInfo("en-GB"));
                            entry.Items = Convert.ToInt32(dvdata[3]);
                            entry.ItemQuantity = Convert.ToDecimal(dvdata[4]);
                            entry.TermsCondition = Convert.ToString(dvdata[5]);
                            entry.Remarks = dvdata[10];
                            entry.RtType = "Return";
                            entry.MaterialCenter = MC;
                            entry.Note = "";
                            entry.Mail = 0;
                            entry.Status = Status.active;
                            entry.CompanyHeaderID = 0;
                            entry.Branch = Branch;
                            entry.Project = dvdata[16] != "" ? Convert.ToInt64(dvdata[16]) : 0;
                            entry.ProTask = dvdata[17] != "" ? Convert.ToInt64(dvdata[17]) : 0;


                            entry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "HireReturn").Select(a => a.EmailTemplateID).FirstOrDefault();

                            entry.Ref1 = Convert.ToString(dvdata[19]);
                            entry.Ref2 = Convert.ToString(dvdata[20]);
                            entry.Ref3 = Convert.ToString(dvdata[21]);
                            entry.Ref4 = Convert.ToString(dvdata[22]);
                            entry.Ref5 = Convert.ToString(dvdata[23]);

                            db.Entry(entry).State = EntityState.Modified;
                            db.SaveChanges();
                            Int64 delyId = entry.HireReturnId;
                            HrNo = entry.HrNo;
                            var HireItem = db.HireDetails.Where(a => a.Reference == delyId && a.Section == "Delivernote").FirstOrDefault();
                            if (HireItem != null)
                            {
                                db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == delyId && a.Section == "Delivernote"));
                                db.SaveChanges();
                            }

                            var DVItem = db.HrItems.Where(a => a.Hr == delyId).FirstOrDefault();
                            if (DVItem != null)
                            {
                                db.HrItems.RemoveRange(db.HrItems.Where(a => a.Hr == delyId));
                                db.SaveChanges();
                            }

                            ////// add to SEItem
                            string result = string.Empty;
                            DataTable dtItem = new DataTable();
                            dtItem.Columns.Add("ItemUnit");
                            dtItem.Columns.Add("ItemUnitPrice");
                            dtItem.Columns.Add("ItemQuantity");
                            dtItem.Columns.Add("ItemDiscount");
                            dtItem.Columns.Add("ItemNote");

                            dtItem.Columns.Add("ReceivedQty");
                            dtItem.Columns.Add("DamageQty");
                            dtItem.Columns.Add("MissingQty");


                            dtItem.Columns.Add("Hr");
                            dtItem.Columns.Add("Item");



                            foreach (var arr in array)
                            {
                                DataRow dr = dtItem.NewRow();

                                dr["ItemUnit"] = (arr[1] == null || arr[1] == "") ? null : (long?)Convert.ToInt64(arr[1]);
                                dr["ItemUnitPrice"] = Convert.ToDecimal(arr[6]);
                                dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                                dr["ItemDiscount"] = Convert.ToDecimal(arr[11]);
                                dr["itemNote"] = Convert.ToString(arr[9].Replace("\n", "<br />"));

                                dr["ReceivedQty"] = arr[3] != "" ? Convert.ToDecimal(arr[3]) : 0;
                                dr["DamageQty"] = arr[4] != "" ? Convert.ToDecimal(arr[4]) : 0;
                                dr["MissingQty"] = arr[5] != "" ? Convert.ToDecimal(arr[5]) : 0;

                                dr["Hr"] = dvId;
                                dr["Item"] = Convert.ToInt32(arr[0]);

                                dtItem.Rows.Add(dr);
                            }

                            ////// create parameter 
                            SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                            parameter.SqlDbType = SqlDbType.Structured;
                            parameter.TypeName = "TableTypeHrItems";
                            //// execute sp sql 
                            string sql = String.Format("EXEC {0} {1};", "SP_InsertHrItems", "@TableType");
                            //// execute sql 
                            db.Database.ExecuteSqlRaw(sql, parameter);

                            //Approved By
                            var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                            var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == dvId && a.Type == "HireReturn").FirstOrDefault();

                            var MrnPO = db.Approvals.Where(a => a.TransEntry == dvId && a.Type == "HireReturn").FirstOrDefault();
                            if (MrnPO != null)
                            {
                                if (chkapp != null)
                                {
                                    db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == dvId && a.Type == "HireReturn"));
                                    db.SaveChanges();
                                }
                                else
                                {

                                    db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == dvId && a.Type == "HireReturn"));
                                    db.SaveChanges();
                                }
                            }
                            var Appby = Convert.ToString(dvdata[18]);
                            if (Appby != null && Appby != "")
                            {
                                long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                                Approval approval = new Approval();
                                foreach (var emp in Approve)
                                {
                                    approval.TransEntry = dvId;
                                    approval.Type = "HireReturn";
                                    approval.EmployeeId = emp;
                                    db.Approvals.Add(approval);
                                    db.SaveChanges();
                                }
                            }
                            com.addlog(LogTypes.Updated, UserId, "HireReturn", "HireReturns", findip(), dvId, "Successfully Updated HireReturn");
                        }
                        //send mail to company address
                        var sendmail = db.EnableSettings.Where(a => a.EnableType == "AutomaticMailInTransactions").FirstOrDefault();
                        var sendcmail = sendmail != null ? sendmail.Status : Status.inactive;
                        if (sendcmail == Status.active)
                        {
                            var custname = db.Customers.Where(a => a.CustomerID == entry.Customer).Select(a => a.CustomerName).FirstOrDefault();
                            var salesman = db.Employees.Where(a => a.EmployeeId == entry.Cashier).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault();
                            var username = db.Users.Where(a => a.Id == entry.CreatedUserId).Select(a => a.UserName).FirstOrDefault();

                            var totrec = db.HrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.ReceivedQty).Sum() ?? 0;
                            var totdam = db.HrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.DamageQty).Sum() ?? 0;
                            var totmis = db.HrItems.Where(a => a.Hr == entry.HireReturnId).Select(a => a.MissingQty).Sum() ?? 0;

                            CompanyEmailFormat CEmail = new CompanyEmailFormat();
                            CEmail.BillNo = "Hire Return-" + entry.BillNo;
                            CEmail.Subject = "<table style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;' colspan='2' align='center'><b>Hire Return Updated</b></td><tr/> " +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Date               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.Date.ToString("dd-MM-yyyy") + "</td><tr/>" +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Customer           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + custname + "</td><tr/>" +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Sales Executive    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + salesman + "</td><tr/>" +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created Date       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.CreatedDate.ToString("dd-MM-yyyy hh:mm:ss tt") + "</td><tr/>" +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Created User       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + username + "</td><tr/>" +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Against Invoice    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + entry.Invoice + "</td><tr/>" +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Received     :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totrec + "</td><tr/>" +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Damage       :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totdam + "</td><tr/>" +
                                    "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Total Missing      :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + totmis + "</td><tr/></table>";

                            com.SendToCompanyMail(CEmail);
                        }

                        dbTran.Commit();

                    }
                    catch (Exception ex)
                    {
                        dbTran.Rollback();
                        msg = "Failed to Update Return Note.";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                }

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    var HData = com.HireReturnData(dvId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                    var item = HData.pdfItem.ToList();
                    var summary = HData;

                    var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout, fmapp } };

                }
                else if (action == "sendmail")
                {
                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = dvdata[6];
                    string CcMail = dvdata[7];
                    string InvoiceNo = "_HireReturn_" + HrNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "HireReturn").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "RETURN NOTE";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our delivery note for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(dvId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully Updated Return Note.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Successfully Updated Return Note.";
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

        [QkAuthorize(Roles = "Dev,Delete HireReturn")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All HireReturn Entry");
            var UserId = User.Identity.GetUserId();
            HireReturn Hr = db.HireReturns.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.HireReturnId == id).FirstOrDefault();

            if (Hr == null)
            {
                return NotFound();
            }
            return PartialView();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete HireReturn")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var chk = DeleteHR(id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Return Note.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete HireReturn")]
        public ActionResult DeleteAllNote(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteHR(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Return Note.", true);
            return RedirectToAction("Index", "HireReturn");
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download HireReturn")]
        public ActionResult Download(long id)
        {
            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = db.HireReturns.Where(s => s.HireReturnId == id).Select(s => s.BillNo).FirstOrDefault();

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", cname + "_HireReturn_" + billno + "_" + System.DateTime.Now.ToShortDateString() + ".pdf");
        }
        public StringBuilder generatePdf(long id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var HData = com.HireReturnData(id, InPrintItemCode, PartNoCheck, TimeOut);
            var item = HData.pdfItem.ToList();
            var summary = HData;

            return com.generatepdf(id, summary, item, null, "Hire Return");
        }


        //                   where b.HireReturnId == dvId
        //                       BillNo = b.BillNo,
        //                       No = b.HrNo,
        //                       Date = b.Date,
        //                       tc = b.Note,
        //                       PartyName = c.CustomerName,
        //                       CustomerCode = c.CustomerCode,
        //                       CustomerEmail = d.EmailId,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       Address = d.Address,
        //                       Phone = d.Phone,
        //                       Mobile = d.Mobile,
        //                       Email = d.EmailId,
        //                       TRN = c.TaxRegNo,
        //                       City = d.City,
        //                       State = d.State,
        //                       Country = d.Country,
        //                       Zip = d.Zip,
        //                       b.TermsCondition,
        //                       c.CreditPeriod,
        //                       b.Remarks

        //                  where b.Hr == dvId && b.ItemNote != "-:{Bundle_Item}"
        //                      ItemUnitPrice = b.ItemUnitPrice,
        //                      ItemQuantity = b.ItemQuantity,
        //                      ItemNote = b.ItemNote,
        //                      ItemCode = c.ItemCode,
        //                      ItemName = c.ItemName,
        //                      itemDesc = c.ItemDescription,
        //                      ItemID = b.Item,
        //                      FileName = d.FileName,
        //                      ItemUnit = e.ItemUnitName,
        //                      ItemDesc = c.ItemDescription,
        //                      PartNumber = c.PartNumber,
        //                      bundleitem = (from ab in db.HrItems
        //                                    where ab.Hr == dvId && ab.ItemNote == "-:{Bundle_Item}"
        //                                    && b.Item == ab.ItemDiscount
        //                                        bb.ItemCode,
        //                                        bb.ItemName,
        //                                        cb.ItemUnitName,
        //                                        ItemUnitPrice = ab.ItemUnitPrice,
        //                                        quantity = ab.ItemQuantity,

        //                                        ab.Item,
        //                                        ab.ItemQuantity,
        //                                        ab.ItemUnit,

        //                                        ItemDiscount = 0,

        //                                        ItemNote = ab.ItemNote,
        //                                        ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                        bb.ItemUnitID,
        //                                        bb.SubUnitId,
        //                                        PriUnit = cb.ItemUnitName,
        //                                        SubUnit = bd.ItemUnitName,
        //                                        bb.ItemArabic,

        //    .Select(s => new
        //        CName = s.CPName,
        //        CAddress = s.CPAddress,
        //        CEmail = s.CPEmail,
        //        CTaxRegNo = s.TRN,
        //        CPhone = s.CPPhone,
        //        s.CPMobile,
        //        CLogo = s.CPLogo,




        //                "<td width='50%'> " +
        //                "<table  style='border: 0px; width: 100%;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +





        [HttpGet]
        [QkAuthorize(Roles = "Dev,View HireReturn")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            HireReturnViewModel vmodel = new HireReturnViewModel();
            vmodel = (from b in db.HireReturns
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.Cashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join f in db.MCs on b.MaterialCenter equals f.MCId into mcs
                      from f in mcs.DefaultIfEmpty()

                      join p in db.SalesEntrys on b.Invoice equals p.SalesEntryId into pur
                      from p in pur.DefaultIfEmpty()
                      join u in db.HireDetails on new { h1 = p.SalesEntryId, h2 = "Sales" }
                      equals new { h1 = u.Reference, h2 = u.Section } into hir
                      from u in hir.DefaultIfEmpty()
                      join x in db.HireTypes on u.HireType equals x.HireTypeId into htyp
                      from x in htyp.DefaultIfEmpty()
                      where b.HireReturnId == id
                      select new HireReturnViewModel
                      {
                          CustomerName = c.CustomerCode + " - " + c.CustomerName,
                          HrNo = b.HrNo,
                          BillNo = b.BillNo,
                          Date = b.Date,
                          TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
                          EmployeeName = e.FirstName + " " + e.LastName,
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          MCName = f.MCName,
                          RtType = b.RtType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                          InBillNo = p.BillNo,
                          HType = x.Name,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "HireReturn"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();

            var v = (from b in db.HrItems
                     join c in db.Items on b.Item equals c.ItemID
                     join d in db.ItemImages on b.Item equals d.ItemID into itimg
                     from d in itimg.DefaultIfEmpty()
                     join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into itunit
                     from e in itunit.DefaultIfEmpty()
                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join h in db.HireReturns on b.Hr equals h.HireReturnId
                     where b.Hr == id && b.ItemNote != "-:{Bundle_Item}"
                     select new
                     {
                         b.ItemUnitPrice,
                         b.ItemQuantity,
                         b.ItemNote,
                         c.ItemCode,
                         c.ItemName,
                         e.ItemUnitName,
                         c.PartNumber,

                         b.ReceivedQty,
                         b.DamageQty,
                         b.MissingQty,
                         b.Item,

                         RetQty = (decimal?)(from aa in db.HrItems
                                             join bb in db.HireReturns on aa.Hr equals bb.HireReturnId
                                             where aa.Item == b.Item && bb.Invoice == h.Invoice
                                             && aa.ItemNote != "-:{Bundle_Item}"
                                             select new
                                             {
                                                 aa.ItemQuantity
                                             }).Sum(a => a.ItemQuantity) ?? 0,

                         DvQty = (decimal?)(from ab in db.SEItemss
                                            join ba in db.SalesEntrys on ab.SalesEntry equals ba.SalesEntryId
                                            where ab.Item == b.Item && ba.SaleType == SaleType.Hire
                                            && ba.SalesEntryId == h.Invoice
                                            && ab.itemNote != "-:{Bundle_Item}"
                                            select new
                                            {
                                                ab.ItemQuantity
                                            }).Sum(a => a.ItemQuantity) ?? 0,
                     }).ToList();

            vmodel.HrItem = (from o in v
                             select new HrItemViewModel
                             {
                                 ItemUnitPrice = o.ItemUnitPrice,
                                 ItemQuantity = o.ItemQuantity,
                                 ItemNote = o.ItemNote,
                                 ItemCode = o.ItemCode,
                                 ItemName = o.ItemName,
                                 ItemUnit = o.ItemUnitName,
                                 PartNumber = o.PartNumber,

                                 ReceivedQty = o.ReceivedQty,
                                 DamageQty = o.DamageQty,
                                 MissingQty = o.MissingQty,

                                 bundleitem = (from ab in db.HrItems
                                               join bb in db.Items on ab.Item equals bb.ItemID
                                               join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                               from cb in primary.DefaultIfEmpty()
                                               join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                               from bd in second.DefaultIfEmpty()
                                               join zz in db.HireReturns on ab.Hr equals zz.HireReturnId
                                               where ab.Hr == id && ab.ItemNote == "-:{Bundle_Item}"
                                               && o.Item == ab.ItemDiscount
                                               select new ItemDetailViewModel
                                               {
                                                   ItemCode = bb.ItemCode,
                                                   ItemName = bb.ItemName,
                                                   ItemUnit = cb.ItemUnitName,
                                                   ItemQuantity = ab.ItemQuantity,

                                                   ReceivedQty = ab.ReceivedQty,
                                                   DamageQty = ab.DamageQty,
                                                   MissingQty = ab.MissingQty,


                                                   RetQty = (decimal?)(from xx in db.HrItems
                                                                       join yy in db.HireReturns on xx.Hr equals yy.HireReturnId
                                                                       where xx.ItemNote == "-:{Bundle_Item}"
                                                                       && yy.HireReturnId == id
                                                                       && ab.Item == xx.Item
                                                                       && xx.ItemDiscount == ab.ItemDiscount
                                                                       select new
                                                                       {
                                                                           xx.ItemQuantity
                                                                       }).Sum(a => a.ItemQuantity) ?? 0,

                                                   DvQty = (decimal?)(from xx in db.SEItemss
                                                                      join yy in db.SalesEntrys on xx.SalesEntry equals yy.SalesEntryId
                                                                      where xx.itemNote == "-:{Bundle_Item}"
                                                                      && yy.SalesEntryId == zz.Invoice
                                                                      && ab.Item == xx.Item
                                                                      && xx.ItemDiscount == ab.ItemDiscount
                                                                      select new
                                                                      {
                                                                          xx.ItemQuantity
                                                                      }).Sum(a => a.ItemQuantity) ?? 0,

                                               }).ToList(),

                             }).ToList();
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "HReturn" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [HttpGet]
        public ActionResult GetCustomer(int CustID)
        {
            var email = (from b in db.HireReturns
                         join c in db.Customers on b.Customer equals c.CustomerID into cust
                         from c in cust.DefaultIfEmpty()
                         join d in db.Contacts on c.Contact equals d.ContactID into cnt
                         from d in cnt.DefaultIfEmpty()
                         where b.Customer == CustID
                         select new
                         {
                             CustomerEmail = d.EmailId,
                         }).FirstOrDefault();
            return Json(email);

        }

        [HttpGet]
        public ActionResult GetHrItems(long entryId, long? ItemId)
        {

            var v = (from a in db.HrItems
                     join b in db.Items on a.Item equals b.ItemID
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                     from f in taxss.DefaultIfEmpty()
                     join g in db.HireReturns on a.Hr equals g.HireReturnId
                     where a.Hr == entryId && a.ItemNote != "-:{Bundle_Item}"
                     && (ItemId == null || a.Item == ItemId) //&& b.KeepStock == true
                     select new
                     {
                         a.Item,
                         a.ItemQuantity,
                         a.ItemUnit,
                         a.ItemUnitPrice,
                         a.ItemDiscount,
                         note = a.ItemNote.Replace("<br />", "\n"),

                         b.ItemCode,
                         b.ItemName,
                         ItemWithCode = b.ItemCode + " - " + b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         b.OpeningStock,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         Tax = f.Percentage,
                         b.SellingPrice,
                         b.PurchasePrice,
                         b.BasePrice,
                         b.MRP,
                         b.KeepStock,

                         a.ReceivedQty,
                         a.DamageQty,
                         a.MissingQty,


                         RetItemQuantity = (decimal?)(from aa in db.HrItems
                                                      join bb in db.HireReturns on aa.Hr equals bb.HireReturnId
                                                      where aa.Item == a.Item && bb.Invoice == g.Invoice
                                                      && aa.ItemNote != "-:{Bundle_Item}"
                                                      select new
                                                      {
                                                          aa.ItemQuantity
                                                      }).Sum(a => a.ItemQuantity) ?? 0,

                         DvItemQuantity = (decimal?)(from ab in db.SEItemss
                                                     join ba in db.SalesEntrys on ab.SalesEntry equals ba.SalesEntryId
                                                     where ab.Item == a.Item && ba.SaleType == SaleType.Hire
                                                     && ba.SalesEntryId == g.Invoice
                                                     && ab.itemNote != "-:{Bundle_Item}"
                                                     select new
                                                     {
                                                         ab.ItemQuantity
                                                     }).Sum(a => a.ItemQuantity) ?? 0,

                     }).ToList();

            var data = (from o in v
                        select new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,

                            o.ItemDiscount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,

                            RetItemQuantity = o.RetItemQuantity,
                            DvItemQuantity = o.DvItemQuantity,
                            id = o.Item,
                            text = o.ItemCode + " - " + o.ItemName,
                            balance = o.DvItemQuantity - o.RetItemQuantity,

                            o.ReceivedQty,
                            o.DamageQty,
                            o.MissingQty,

                            bundle = (from ab in db.HrItems
                                      join fb in db.BundleItems on ab.Item equals fb.ItemId into bunit
                                      from fb in bunit.DefaultIfEmpty()
                                      join ay in db.ItemBundles on fb.ItemBundle equals ay.ItemBundleId into bun
                                      from ay in bun.DefaultIfEmpty()
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                      from dd in scaffold.DefaultIfEmpty()
                                      join eb in db.ItemUnits on fb.ItemUnit equals eb.ItemUnitID into bpunit
                                      from eb in bpunit.DefaultIfEmpty()
                                      join zz in db.HireReturns on ab.Hr equals zz.HireReturnId
                                      let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                      where ab.Hr == entryId && ab.ItemNote == "-:{Bundle_Item}"
                                      && o.ItemID == ab.ItemDiscount && ay.mainItem == ab.ItemDiscount
                                      select new
                                      {
                                          Id = bb.ItemID,
                                          ItemUnitPrice = ab.ItemUnitPrice,
                                          ItemQuantity = ab.ItemQuantity,
                                          ItemDiscount = ab.ItemDiscount,
                                          ItemNote = "",

                                          ItemCode = bb.ItemCode,
                                          ItemName = bb.ItemName,
                                          ItemUnit = eb.ItemUnitName,
                                          PartNumber = bb.PartNumber,
                                          PNoStatus = "",
                                          CBM = dd.CBM,
                                          Weight = dd.Weight,
                                          img = bimg,
                                          note = ab.ItemNote.Replace("<br />", "\n"),

                                          KeepStock = bb.KeepStock,

                                          ab.Item,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          bb.ItemArabic,
                                          bb.ItemDescription,
                                          BaseQty = (ab.ItemQuantity / o.ItemQuantity),

                                          balance = o.DvItemQuantity - o.RetItemQuantity,
                                          ab.ReceivedQty,
                                          ab.DamageQty,
                                          ab.MissingQty,

                                          RetItemQuantity = (decimal?)(from xx in db.HrItems
                                                                       join yy in db.HireReturns on xx.Hr equals yy.HireReturnId
                                                                       where xx.ItemNote == "-:{Bundle_Item}"
                                                                       && yy.HireReturnId == entryId
                                                                       && ab.Item == xx.Item
                                                                       && xx.ItemDiscount == ab.ItemDiscount
                                                                       select new
                                                                       {
                                                                           xx.ItemQuantity
                                                                       }).Sum(a => a.ItemQuantity) ?? 0,

                                          DvItemQuantity = (decimal?)(from xx in db.SEItemss
                                                                      join yy in db.SalesEntrys on xx.SalesEntry equals yy.SalesEntryId
                                                                      where xx.itemNote == "-:{Bundle_Item}"
                                                                      && yy.SalesEntryId == zz.Invoice
                                                                      && ab.Item == xx.Item
                                                                      && xx.ItemDiscount == ab.ItemDiscount
                                                                      select new
                                                                      {
                                                                          xx.ItemQuantity
                                                                      }).Sum(a => a.ItemQuantity) ?? 0,


                                      }).ToList(),



                        }).ToList();
            return Json(data);
        }

        [HttpPost]
        public JsonResult GetHrByIds(long[] array)//long[] array 
        {
            var result = (from a in db.HrItems
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                          from c in primary.DefaultIfEmpty()
                          join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                          from d in second.DefaultIfEmpty()
                          where array.Contains(a.Hr)
                          select new
                          {
                              b.ItemID,
                              a.Item,
                              a.ItemQuantity,
                              a.ItemUnit,
                              a.ItemUnitPrice,
                              a.ItemDiscount,
                              ItemCode = b.ItemCode,
                              note = a.ItemNote.Replace("<br />", "\n"),
                              ItemName = b.ItemName,
                              ItemWithCode = b.ItemCode + " - " + b.ItemName,
                              b.ItemUnitID,
                              b.SubUnitId,
                              PriUnit = c.ItemUnitName,
                              SubUnit = d.ItemUnitName,
                              b.BasePrice,
                              b.SellingPrice,
                              b.PurchasePrice,
                              b.MRP,
                              a.Hr
                          });
            return Json(result);
        }

        private Boolean DeleteHR(long sId)
        {
            var UserId = User.Identity.GetUserId();
            HireReturn note = db.HireReturns.Find(sId);

            var item = db.HrItems.Where(a => a.Hr == sId).FirstOrDefault();
            if (item != null)
            {
                db.HrItems.RemoveRange(db.HrItems.Where(a => a.Hr == sId));
            }

            var appr = db.Approvals.Where(a => a.TransEntry == sId && a.Type == "HireReturn").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == sId && a.Type == "HireReturn"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == sId && a.Type == "HireReturn").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == sId && a.Type == "HireReturn"));
            }
            db.HireReturns.Remove(note);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "HireReturn", "HireReturns", findip(), note.HireReturnId, "Successfully Deleted HireReturn");
            return true;
        }

        private string InvoiceNo(Int64 HrNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "HireReturn").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "HireReturn").Select(a => a.number).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.HireReturns.Select(p => p.HrNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    HrNo = db.HireReturns.Max(p => p.HrNo + 1);
                    billNo = companyPrefix + HrNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(HrNo, billNo);
                    }
                }
            }
            else
            {
                HrNo = HrNo + 1;
                billNo = companyPrefix + HrNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(HrNo, billNo);
                }
            }
            return billNo;
        }

        private bool BillExist(string No)
        {
            var Exists = db.HireReturns.Any(c => c.BillNo == No);
            bool res = (Exists) ? true : false;
            return res;
        }

        private long GetNo()
        {
            Int64 No = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "HireReturn").Select(a => a.number).FirstOrDefault();
            if ((db.HireReturns.Select(p => p.HrNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    No = 1;
                }
                else
                {
                    No = number;
                }
            }
            else
            {
                No = db.HireReturns.Max(p => p.HrNo + 1);
            }

            return No;
        }

        [HttpPost]
        public ActionResult GetHireType(long Invoice)
        {
            var customer = db.SalesEntrys.Where(x => x.SalesEntryId == Invoice).Select(x => x.Customer).FirstOrDefault();
            var Hire = (from b in db.SalesEntrys
                        join h in db.HireDetails on new { h1 = Invoice, h2 = "Sales" }
                        equals new { h1 = h.Reference, h2 = h.Section } into hir
                        from h in hir.DefaultIfEmpty()
                        join c in db.Customers on b.Customer equals c.CustomerID into cust
                        from c in cust.DefaultIfEmpty()
                        join d in db.Contacts on c.Contact equals d.ContactID into cnt
                        from d in cnt.DefaultIfEmpty()
                        where b.SalesEntryId == Invoice
                        select new
                        {
                            Hty = h.HireType
                        }).FirstOrDefault();
            return Json(Hire);

        }

        public JsonResult SearchInvoiceItem(string q, string x, long? cust, long? mc, string constat, long? Invoice = 0)
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

            var item = (from i in db.SEItemss
                        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                        join b in db.Items on i.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                            //equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                            //equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                        where (b.Status == Status.active && (check == true || b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.PartNumber.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q) || (b.PartNumber.Contains(q) && PartNoCheck == Status.active)))
                        && (i.SalesEntry == Invoice)
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

                            PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                            lastSale = (decimal?)null,
                            lastSaleU = (decimal?)null,

                            lastPur = (decimal?)null,
                            lastPurU = (decimal?)null,

                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                            b.Barcode,

                            OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,

                            PriPurchase = (decimal?)(from v in db.PEItemss
                                                     join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                     where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.ItemQuantity
                                                     }).Sum(c => c.ItemQuantity) ?? 0,

                            SubPurchase = (decimal?)(from v in db.PEItemss
                                                     join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                     where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.ItemQuantity
                                                     }).Sum(c => c.ItemQuantity) ?? 0,

                            PriSale = (decimal?)(from v in db.SEItemss
                                                 join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                 where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                 && (mc == 0 || mc == w.MaterialCenter)
                                                 select new
                                                 {
                                                     v.ItemQuantity
                                                 }).Sum(c => c.ItemQuantity) ?? 0,

                            SubSale = (decimal?)(from v in db.SEItemss
                                                 join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                 where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                 && (mc == 0 || mc == w.MaterialCenter)
                                                 select new
                                                 {
                                                     v.ItemQuantity
                                                 }).Sum(c => c.ItemQuantity) ?? 0,

                            PriPReturn = (decimal?)(from v in db.PRItemss
                                                    join w in db.PurchaseReturns on v.PurchaseReturnId equals w.purchaseEntryId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            SubPReturn = (decimal?)(from v in db.PRItemss
                                                    join w in db.PurchaseReturns on v.PurchaseReturnId equals w.purchaseEntryId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            PriSReturn = (decimal?)(from v in db.SRItemss
                                                    join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            SubSReturn = (decimal?)(from v in db.SRItemss
                                                    join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            //PriProdItem = (decimal?)db.Productions.Where(a => (a.MaterialCenter == mc || mc == 0) && a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            //SubProdItem = (decimal?)db.Productions.Where(a => (a.MaterialCenter == mc || mc == 0) && a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            PriProdItem = (decimal?)(from v in db.GeneratedItem
                                                     join w in db.Productions on v.Production equals w.ProductionId
                                                     where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.Qty
                                                     }).Sum(c => c.Qty) ?? 0,
                            SubProdItem = (decimal?)(from v in db.GeneratedItem
                                                     join w in db.Productions on v.Production equals w.ProductionId
                                                     where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.Qty
                                                     }).Sum(c => c.Qty) ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)(from v in db.ProItems
                                                      join w in db.Productions on v.Production equals w.ProductionId
                                                      where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      select new
                                                      {
                                                          v.Quantity
                                                      }).Sum(c => c.Quantity) ?? 0,

                            SubProdCItem = (decimal?)(from v in db.ProItems
                                                      join w in db.Productions on v.Production equals w.ProductionId
                                                      where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      select new
                                                      {
                                                          v.Quantity
                                                      }).Sum(c => c.Quantity) ?? 0,

                            // main item
                            //PriUnItem = (decimal?)db.Unassembles.Where(a => (a.MaterialCenter == mc || mc == 0) && a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            //SubUnItem = (decimal?)db.Unassembles.Where(a => (a.MaterialCenter == mc || mc == 0) && a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            PriUnItem = (decimal?)(from v in db.ConsumedItem
                                                   join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                   where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                   && (mc == 0 || mc == w.MaterialCenter)
                                                   select new
                                                   {
                                                       v.Qty
                                                   }).Sum(c => c.Qty) ?? 0,
                            SubUnItem = (decimal?)(from v in db.ConsumedItem
                                                   join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                   where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                   select new
                                                   {
                                                       v.Qty
                                                   }).Sum(c => c.Qty) ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)(from v in db.UnassembleItems
                                                    join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                    where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.Quantity
                                                    }).Sum(c => c.Quantity) ?? 0,

                            SubUnCItem = (decimal?)(from v in db.UnassembleItems
                                                    join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                    where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.Quantity
                                                    }).Sum(c => c.Quantity) ?? 0,


                        }).Distinct();
            var vd = item.OrderBy(b => b.ItemName).Skip(skip).Take(pageSize).ToList();
            if (constat != "SalesEntry" || StockItemsPerm != true)
            {

            }
            else
            {
                vd = item.OrderBy(b => b.ItemName).ToList();
            }
            var data = vd.Select(o => new
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
            //&& (constat != "SalesEntry" || (StockItemsPerm != true || b.KeepStock == true || b.OpeningStock > 0 ))
            if (constat == "SalesEntry" && StockItemsPerm == true)
            {
                data = data.Where(a => a.KeepStock == true && a.total > 0).Skip(skip).Take(pageSize).ToList();
            }

            return Json(data);
        }

        public JsonResult SearchHireItem(string q, string x, long? sentry, long? qrystr)
        {

            var data = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.SalesEntrys on a.SalesEntry equals g.SalesEntryId
                        where a.SalesEntry == sentry && a.itemNote != "-:{Bundle_Item}"
                        && g.SaleType == SaleType.Hire //&& b.KeepStock == true
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

                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,

                            RetItemQuantity = (decimal?)(from aa in db.HireReturns
                                                         join bb in db.HrItems on aa.HireReturnId equals bb.Hr
                                                         where bb.Item == a.Item && aa.Invoice == sentry
                                                         && bb.ItemNote != "-:{Bundle_Item}"
                                                         select new
                                                         {
                                                             bb.ItemQuantity
                                                         }).Sum(a => a.ItemQuantity) ?? 0,

                            DvItemQuantity = (decimal?)(from ab in db.SalesEntrys
                                                        join ba in db.SEItemss on ab.SalesEntryId equals ba.SalesEntry
                                                        where ba.Item == a.Item && ab.SaleType == SaleType.Hire
                                                        && ab.SalesEntryId == sentry
                                                        && ba.itemNote != "-:{Bundle_Item}"
                                                        select new
                                                        {
                                                            ba.ItemQuantity
                                                        }).Sum(a => a.ItemQuantity) ?? 0,
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,

                            RetItemQuantity = o.RetItemQuantity,
                            DvItemQuantity = o.DvItemQuantity,
                            id = o.Item,
                            text = o.ItemCode + " - " + o.ItemName,
                            balance = o.DvItemQuantity - o.RetItemQuantity
                        }).ToList();

            if (qrystr == null)
            {
                data = data.Where(a => a.balance > 0).ToList();
            }
            else
            {
                var invoice = db.HrItems.Where(a => a.Hr == qrystr).Select(a => a.Item).ToArray();
                data = data.Where(a => invoice.Contains(a.Item)).ToList();
            }
            return Json(data);
        }


        public JsonResult SearchHireItemById(long? entryId, long? ItemId)
        {

            var v = (from a in db.SEItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                     from f in taxss.DefaultIfEmpty()
                     join g in db.SalesEntrys on a.SalesEntry equals g.SalesEntryId
                     where a.SalesEntry == entryId && a.itemNote != "-:{Bundle_Item}"
                     && g.SaleType == SaleType.Hire && a.Item == ItemId //&& b.KeepStock == true
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

                         b.ItemCode,
                         b.ItemName,
                         ItemWithCode = b.ItemCode + " - " + b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         b.OpeningStock,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         Tax = f.Percentage,
                         b.SellingPrice,
                         b.PurchasePrice,
                         b.BasePrice,
                         b.MRP,
                         b.KeepStock,

                         RetItemQuantity = (decimal?)(from aa in db.HireReturns
                                                      join bb in db.HrItems on aa.HireReturnId equals bb.Hr
                                                      where bb.Item == a.Item && aa.Invoice == entryId
                                                      && bb.ItemNote != "-:{Bundle_Item}"
                                                      select new
                                                      {
                                                          bb.ItemQuantity
                                                      }).Sum(a => a.ItemQuantity) ?? 0,

                         DvItemQuantity = (decimal?)(from ab in db.SalesEntrys
                                                     join ba in db.SEItemss on ab.SalesEntryId equals ba.SalesEntry
                                                     where ba.Item == a.Item && ab.SaleType == SaleType.Hire
                                                     && ab.SalesEntryId == entryId
                                                     && ba.itemNote != "-:{Bundle_Item}"
                                                     select new
                                                     {
                                                         ba.ItemQuantity
                                                     }).Sum(a => a.ItemQuantity) ?? 0,

                     }).ToList();

            var data = (from o in v
                        select new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,

                            RetItemQuantity = o.RetItemQuantity,
                            DvItemQuantity = o.DvItemQuantity,
                            id = o.Item,
                            text = o.ItemCode + " - " + o.ItemName,
                            balance = o.DvItemQuantity - o.RetItemQuantity,

                            bundle = (from ab in db.SEItemss
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                      from dd in scaffold.DefaultIfEmpty()

                                      join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                      from eb in bpunit.DefaultIfEmpty()

                                      let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                      where ab.SalesEntry == entryId && ab.itemNote == "-:{Bundle_Item}"
                                      && o.ItemID == ab.ItemDiscount
                                      select new
                                      {
                                          Id = bb.ItemID,
                                          ItemUnitPrice = ab.ItemUnitPrice,
                                          ItemQuantity = ab.ItemQuantity,
                                          ItemSubTotal = ab.ItemSubTotal,
                                          ItemDiscount = ab.ItemDiscount,
                                          ItemNote = "",
                                          ItemTax = ab.ItemTax,
                                          ItemTaxAmount = ab.ItemTaxAmount,
                                          ItemTotalAmount = ab.ItemTotalAmount,

                                          ItemCode = bb.ItemCode,
                                          ItemName = bb.ItemName,
                                          ItemUnit = eb.ItemUnitName,
                                          //ItemUnitName = eb.ItemUnit
                                          PartNumber = bb.PartNumber,
                                          PNoStatus = "",
                                          CBM = dd.CBM,
                                          Weight = dd.Weight,
                                          img = bimg,
                                          note = ab.itemNote.Replace("<br />", "\n"),

                                          KeepStock = bb.KeepStock,

                                          ab.Item,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          bb.ItemArabic,
                                          bb.ItemDescription,
                                          BaseQty = (ab.ItemQuantity / o.ItemQuantity),
                                          RetItemQuantity = (decimal?)(from xx in db.HrItems
                                                                       join yy in db.HireReturns on xx.Hr equals yy.HireReturnId
                                                                       where xx.ItemNote == "-:{Bundle_Item}"
                                                                       && yy.Invoice == entryId
                                                                       && ab.Item == xx.Item
                                                                       && xx.ItemDiscount == ab.ItemDiscount
                                                                       select new
                                                                       {
                                                                           xx.ItemQuantity
                                                                       }).Sum(a => a.ItemQuantity) ?? 0,

                                          DvItemQuantity = (decimal?)(from xx in db.SEItemss
                                                                      join yy in db.SalesEntrys on xx.SalesEntry equals yy.SalesEntryId
                                                                      where xx.itemNote == "-:{Bundle_Item}"
                                                                      && yy.SalesEntryId == entryId
                                                                      && ab.Item == xx.Item
                                                                      && xx.ItemDiscount == ab.ItemDiscount
                                                                      select new
                                                                      {
                                                                          xx.ItemQuantity
                                                                      }).Sum(a => a.ItemQuantity) ?? 0,
                                      }).ToList(),
                        }).ToList();

            return Json(data);
        }




        public JsonResult SearchHireEntry(string q)
        {
            object serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                serialisedJson = (from a in db.HireReturns
                                  join b in db.SalesEntrys on a.Invoice equals b.SalesEntryId
                                  where a.BillNo.ToLower().Contains(q.ToLower()) || a.BillNo.Contains(q)
                                  select new
                                  {
                                      id = b.SalesEntryId,
                                      text = b.BillNo
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.HireReturns
                                  join b in db.SalesEntrys on a.Invoice equals b.SalesEntryId
                                  select new
                                  {
                                      id = b.SalesEntryId,
                                      text = b.BillNo
                                  }).OrderBy(b => b.text).ToList();
            }
            return Json(serialisedJson);
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "HireReturn" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.HireReturns.Where(a => a.HireReturnId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "HireReturn").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
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
                AppUp.Type = "HireReturn";

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
                            join d in db.HireReturns on b.TransEntry equals d.HireReturnId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedUserId equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "HireReturn"
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
