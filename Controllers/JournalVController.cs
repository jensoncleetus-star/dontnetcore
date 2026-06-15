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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Drawing;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class JournalVController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public JournalVController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "Journalapp" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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


            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "Journalapp").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = UserId;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "Journalapp";

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
        // GET: Journal
        [QkAuthorize(Roles = "Dev,Journal List")]
        public ActionResult Index()
        {
            ViewBag.PayFrom = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            ViewBag.PayTo = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.InvoiceNo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);


            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

            return View();
        }
    [QkAuthorize(Roles = "Dev,Journal List")]
        public JsonResult GetData(string InvoiceNo,string SaleInvoiceNo, string FromDate, string ToDate, long? PayFrom, long? PayTo, string user, long? type,int? vnature)
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Journal");
            var uDownload = User.IsInRole("Download Journal");
            var uDelete = User.IsInRole("Delete Journal");
            var userpermission = User.IsInRole("All Journal Entry");
            var Paymode = ModeOfPayment.PDC;
            if (type == 1)
            {
                Paymode = ModeOfPayment.PDC;
            }
            else if (type == 0)
            {
                Paymode = ModeOfPayment.Cash;
            }
            else
            {

            }
            var UserId = User.Identity.GetUserId();
            var v = (from a in db.Journals
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                     from c in payto.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id into usrs
                     from g in usrs.DefaultIfEmpty()
                     join h in db.SalesEntrys on a.InvoiceNo equals h.BillNo into salez
                     from h in salez.DefaultIfEmpty()
                     where (a.editable == choice.Yes)
                     && ((InvoiceNo == null || InvoiceNo == "" || a.VoucherNo == InvoiceNo) 
                    && 
                    (SaleInvoiceNo == null || SaleInvoiceNo == "" || SaleInvoiceNo == "0" || a.InvoiceNo == SaleInvoiceNo) &&
                     (type == null || a.MOPayment == Paymode) &&
                    (vnature == null || a.VATNature == vnature) &&
                    (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                    (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                    (PayFrom == 0 || PayFrom == null || a.PayFrom == PayFrom) &&
                    (PayTo == 0 || PayTo == null || a.PayTo == PayTo)) && (user == null || user == "" || g.Id == user)
                    && (userpermission == true || a.CreatedBy == UserId)
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         SaleInvoiceNo = a.InvoiceNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.JournalId,
                         a.Date,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.editable,
                         a.CreatedDate,
                         a.MOPayment,
                         a.VATNature
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.JournalId,
                         o.Date,
                         o.PayFrom,
                         o.PayTo,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.editable,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         Download = uDownload,
                         o.CreatedDate,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         VATNature = o.VATNature == 0 ? "Not Applicable" : (o.VATNature == 1 ? "Registered Expense (B2B)" : null)
                     });

            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p =>// p.JournalId.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.VoucherNo.ToString().ToLower().Equals(search.ToLower())
                                 //p.GrandTotal.ToString().ToLower().Contains(search.ToLower())
                                 );

            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data2 = v.Skip(skip).Take(pageSize).ToList();
            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var data = (from o in data2
                        let app = db.Approvals.Where(x => x.TransEntry == o.JournalId && x.Type == "Journalapp").Select(x => x.EmployeeId).ToList()
                        let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == o.JournalId && x.Type == "Journalapp").Select(x => x.ApprovalStatus).ToList()
                        let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == o.JournalId && x.Type == "Journalapp").GroupBy(l => l.ApprovedBy)
                                           .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                           .ToList().Select(x => x.ApprovalStatus).ToList()
                        select new
                        {
                            o.VoucherNo,
                            o.Payer,
                            o.Reciever,
                            o.JournalId,
                            o.Date,
                            o.PayFrom,
                            o.PayTo,
                            o.SubTotal,
                            o.GrandTotal,
                            o.Paying,
                            o.editable,

                            o.CreatedDate,

                            app = app,


                            AppStatus = AppStatus,
                            chkAppStatus = chkAppStatus,



                            o.modeofpay,
                            o.VATNature

                        }).Select(o => new
                        {
                            o.VoucherNo,
                            o.Payer,
                            o.Reciever,
                            o.JournalId,
                            o.Date,
                            o.PayFrom,
                            o.PayTo,
                            o.SubTotal,
                            o.GrandTotal,
                            o.Paying,
                            o.editable,
                            Dev = uDev,
                            Edit = uEdit,
                            Delete = uDelete,
                            Download = uDownload,
                            o.CreatedDate,
                            o.modeofpay,
                            o.VATNature,
                            o.app,
                            Approval = (o.app != null && empl.EmployeeId != null && empl.EmployeeId != 0) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                            ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,


                        }).ToList();


            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [QkAuthorize(Roles = "Dev,Create Journal")]
        public ActionResult Createexpense()
        {
            _FinancialYear();
            ViewBag.BillToReceipt = 0;
            ViewBag.BillToPayment = 0;
            var list = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                                }, "Value", "Text", 1);
            ViewBag.list = list;


            ViewBag.Paidfrom = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1); ;
            ViewBag.Paidfrom1 = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                          }, "Value", "Text", 1); ;
            ViewBag.Paidfrom2 = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                            }, "Value", "Text", 1); ;
            ViewBag.PaidTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1);
            ViewBag.InvoiceNo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search InvoiceNo", Value = ""},
                             }, "Value", "Text", 1);

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

            var Journal = new JournalViewModel
            {
                VoucherNo = JournalVoucherNo(),
                Date = (System.DateTime.Now).ToString("dd-MM-yyyy")
            };

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            //    .Select(s => new
            //        ID = s.ProjectId,
            //        Name = s.ProjectName
            //    })

            // .Select(s => new
            //     ID = s.ProTaskId,
            //     Name = s.TaskName
            // })
            //company info
            companySet();
            ViewBag.LastEntry = db.Journals.Where(p => p.editable == choice.Yes).Select(p => p.JournalId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            var AccJnl = db.EnableSettings.Where(a => a.EnableType == "AccInJournal").FirstOrDefault();
            var AccJnls = AccJnl != null ? AccJnl.Status : Status.inactive;
            ViewBag.AccJnl = AccJnls;

            //field mapping
            Journal.FieldMap = db.FieldMappings.Where(a => a.Section == "Journal" && a.Status == Status.active).ToList();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View(Journal);
        }
        // create 
        [QkAuthorize(Roles = "Dev,Create Journal")]
        public ActionResult Create(long? id, string type)
        {
            _FinancialYear();
            ViewBag.BillToReceipt = 0;
            ViewBag.BillToPayment = 0;
         var list = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1);
            ViewBag.list = list;
           

            ViewBag.Paidfrom = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1); ;
            ViewBag.Paidfrom1 = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                          }, "Value", "Text", 1); ;
            ViewBag.Paidfrom2 = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                            }, "Value", "Text", 1); ;
            ViewBag.PaidTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1);
            ViewBag.InvoiceNo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search InvoiceNo", Value = ""},
                             }, "Value", "Text", 1);

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

            var Journal = new JournalViewModel
            {
                VoucherNo = JournalVoucherNo(),
                Date = (System.DateTime.Now).ToString("dd-MM-yyyy")
            };

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            //    .Select(s => new
            //        ID = s.ProjectId,
            //        Name = s.ProjectName
            //    })

            // .Select(s => new
            //     ID = s.ProTaskId,
            //     Name = s.TaskName
            // })
            //company info
            companySet();
            ViewBag.LastEntry = db.Journals.Where(p => p.editable == choice.Yes).Select(p => p.JournalId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            var AccJnl = db.EnableSettings.Where(a => a.EnableType == "AccInJournal").FirstOrDefault();
            var AccJnls = AccJnl != null ? AccJnl.Status : Status.inactive;
            ViewBag.AccJnl = AccJnls;

            //field mapping
            Journal.FieldMap = db.FieldMappings.Where(a => a.Section == "Journal" && a.Status == Status.active).ToList();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            if (id != null)
            {
                Journal rpt = db.Journals.Find(id);
                string Mop = Convert.ToString(rpt.MOPayment);
                //duplicate quotation
                if (type == "JVExtend")
                {
                  

                    

                     
                    Journal.jnlitems = (from a in db.AccountsTransactions
                                        join b in db.Accountss on a.Account equals b.AccountsID
                                        join p in db.Projects on a.Project equals p.ProjectId into proj
                                        from p in proj.DefaultIfEmpty()
                                        join t in db.ProTasks on a.ProTask equals t.ProTaskId into protask
                                        from t in protask.DefaultIfEmpty()
                                        where a.reference == rpt.JournalId && a.Purpose == "Journal"
                                        select new JournalVItems2
                                        {
                                            AccType = (a.Type == 0) ? 0 : 1,
                                            AccountID = a.Account,
                                            Debit = a.Debit,
                                            Credit = a.Credit,
                                            Narration = a.Narration,
                                            AccountName = b.Name,
                                            ProjectId = a.Project,
                                            ProjectName = p.ProjectName,
                                            TaskId = a.ProTask,
                                            TaskName = t.TaskName
                                        }).ToList();
                    return View(Journal);
                }
                else
                {
                    return NotFound();
                }
             
            }
                    return View(Journal);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Journal")]
        public JsonResult Create(JournalVViewModel vmodel)
        {
            string msg;
            bool stat;
            ////  if (!journalBillExist(Convert.ToString(vmodel.VoucherNo)))
            var AutoBillVoucher = db.EnableSettings.Where(a => a.EnableType == "AutomaticVoucherNo").FirstOrDefault();
            var AutoBillVouchers = AutoBillVoucher != null ? AutoBillVoucher.Status : Status.inactive;

            if (!com.payBillExist(Convert.ToString(vmodel.VoucherNo)) || AutoBillVouchers == Status.active)
            {
                var UserId = User.Identity.GetUserId();

                long Branch = 0;
                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
                if (BranchCheck == Status.active)
                {
                    Branch = vmodel.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                DateTime Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                var today = Convert.ToDateTime(System.DateTime.Now);
                var jornNo = JournalMaxvoucher();
                long PayFrom = vmodel.jnlitems.Where(a => a.Credit != null && a.Credit != 0).Select(a => a.AccountID).FirstOrDefault();
                long PayTo = vmodel.jnlitems.Where(a => a.Debit != null && a.Debit != 0).Select(a => a.AccountID).FirstOrDefault();

                DateTime? pdcDate = null;
                if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                {
                    //same datepicker in CDC
                    pdcDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                }
                vmodel.VoucherNo = (AutoBillVouchers == Status.active) ? JournalVoucherNo() : Convert.ToString(vmodel.VoucherNo);

                Journal JOR = new Journal
                {
                    Voucher = jornNo,
                    VoucherNo = vmodel.VoucherNo,
                    Date = Date,
                    PayFrom = PayFrom,
                    PayTo = PayTo,
                    Remark = vmodel.Remark,

                    Balance = 0,
                    GrandTotal = (decimal)vmodel.Paying,
                    Paying = (decimal)vmodel.Paying,
                    Status = Status.active,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    Branch = Branch,
                    editable = choice.Yes,
                    Reference = 0,
                    RefType = "Journal",
                    VATNature = vmodel.VATNature,
                    Ref1 = vmodel.Ref1,
                    Ref2 = vmodel.Ref2,
                    Ref3 = vmodel.Ref3,
                    Ref4 = vmodel.Ref4,
                    Ref5 = vmodel.Ref5,

                    MOPayment = vmodel.MOPayment,
                    PDCDate = pdcDate,
                    InvoiceNo=vmodel.InvoiceNo,

                };
                db.Journals.Add(JOR);
                db.SaveChanges();
                Int64 JournalId = JOR.JournalId;
                bool? Astatus = null;
                var Bills = "";
                var retval = 0;
                long[] bill = null;

                Approval approval = new Approval();
                var Approve = db.AssignedTos.Where(o => o.CustomerID == -1).Select(o => o.EmployeeId).ToArray();
                foreach (var emp in Approve)
                {
                    approval.TransEntry = JournalId;
                    approval.Type = "Journalapp";
                    approval.EmployeeId = emp;
                    db.Approvals.Add(approval);
                    db.SaveChanges();
                }
                if (vmodel.invoicedatapay != null && vmodel.invoicedataref == null && vmodel.invoicedataref2 == null)
                {
                    JornalPaymentBill paybill = new JornalPaymentBill();
                    foreach (var arr in vmodel.invoicedatapay)
                    {
                        if (arr.Type == "Against Reference")
                        {
                            decimal payAmt = db.PETransactions.Where(a => a.PurchaseEntry == arr.InvoiceNo && a.PaymentId == 0).FirstOrDefault() != null ? (decimal?)db.PETransactions.Where(a => a.PaymentId == arr.InvoiceNo && a.PaymentId == 0).ToList().Select(a => a.PEPayAmount).Sum() ?? 0 : 0;
                            if (payAmt > 0)
                            {
                                PEPayment PEpay = db.PEPayments.Where(a => a.PurchaseEntry == arr.InvoiceNo).FirstOrDefault();
                                PEpay.PEPaidAmount = PEpay.PEPaidAmount - payAmt;
                                db.Entry(PEpay).State = EntityState.Modified;

                                db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == 0 && a.PurchaseEntry == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                            decimal payAmtSr = db.SRTransactions.Where(a => a.SalesReturnId == arr.InvoiceNo && a.PaymentId == 0).FirstOrDefault() != null ? (decimal?)db.SRTransactions.Where(a => a.PaymentId == arr.InvoiceNo && a.PaymentId == 0).Select(a => a.SRPayAmount).Sum() ?? 0 : 0;
                            if (payAmtSr > 0)
                            {
                                SRPayment SRpay = db.SRPayments.Where(a => a.SalesReturnId == arr.InvoiceNo).FirstOrDefault();
                                SRpay.SReturnAmount = SRpay.SReturnAmount - payAmtSr;
                                db.Entry(SRpay).State = EntityState.Modified;

                                db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == 0 && a.SalesReturnId == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                        }
                        paybill.InvoiceNo = arr.InvoiceNo;
                        paybill.NewRefName = arr.NewRefName;
                        paybill.Jornal = JournalId;
                        paybill.BillType = arr.BillType;
                        paybill.Amount = arr.Amount;
                        paybill.Type = arr.Type; //arr.Type;
                        paybill.Status = arr.Status;

                        db.JornalPaymentBills.Add(paybill);
                        db.SaveChanges();
                    };
                }


















                if (vmodel.invoicedataref != null)
                {
                    JornalBill recbill = new JornalBill();
                    foreach (var arr in vmodel.invoicedataref)
                    {

                        if (arr.Type == "Against Reference")
                        {
                            decimal payAmt = db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).ToList().Select(a => a.SEPayAmount).Sum() ?? 0 : 0;
                            if (payAmt > 0)
                            {
                                SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == arr.InvoiceNo).FirstOrDefault();
                                SEpay.SEPaidAmount = SEpay.SEPaidAmount - payAmt;
                                db.Entry(SEpay).State = EntityState.Modified;

                                db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == 0 && a.SalesEntry == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                            decimal payAmtTr = db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).Select(a => a.PRPayAmount).Sum() ?? 0 : 0;
                            if (payAmtTr > 0)
                            {
                                PRPayment PRpay = db.PRPayments.Where(a => a.PurchaseReturnId == arr.InvoiceNo).FirstOrDefault();
                                PRpay.PReturnAmount = PRpay.PReturnAmount - payAmtTr;
                                db.Entry(PRpay).State = EntityState.Modified;

                                db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == 0 && a.PurchaseReturnId == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                        }

                        recbill.InvoiceNo = arr.InvoiceNo;
                        recbill.NewRefName = arr.NewRefName;
                        recbill.Jornal = JournalId;
                        recbill.BillType = arr.BillType;
                        recbill.Amount = arr.Amount;
                        recbill.Type = arr.Type; //arr.Type;
                        recbill.Status = arr.Status;
                        recbill.payfrom = vmodel.payfrom1;
                        db.JornaltBills.Add(recbill);
                        db.SaveChanges();
                    };
                }
                
                if (vmodel.invoicedataref2 != null)
                {
                    JornalBill recbill = new JornalBill();
                    foreach (var arr in vmodel.invoicedataref2)
                    {

                        if (arr.Type == "Against Reference")
                        {
                            decimal payAmt = db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).ToList().Select(a => a.SEPayAmount).Sum() ?? 0 : 0;
                            if (payAmt > 0)
                            {
                                SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == arr.InvoiceNo).FirstOrDefault();
                                SEpay.SEPaidAmount = SEpay.SEPaidAmount - payAmt;
                                db.Entry(SEpay).State = EntityState.Modified;

                                db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == 0 && a.SalesEntry == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                            decimal payAmtTr = db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).Select(a => a.PRPayAmount).Sum() ?? 0 : 0;
                            if (payAmtTr > 0)
                            {
                                PRPayment PRpay = db.PRPayments.Where(a => a.PurchaseReturnId == arr.InvoiceNo).FirstOrDefault();
                                PRpay.PReturnAmount = PRpay.PReturnAmount - payAmtTr;
                                db.Entry(PRpay).State = EntityState.Modified;

                                db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == 0 && a.PurchaseReturnId == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                        }

                        recbill.InvoiceNo = arr.InvoiceNo;
                        recbill.NewRefName = arr.NewRefName;
                        recbill.Jornal = JournalId;
                        recbill.BillType = arr.BillType;
                        recbill.Amount = arr.Amount;
                        recbill.Type = arr.Type; //arr.Type;
                        recbill.Status = arr.Status;
                        recbill.payfrom = vmodel.payfrom2;
                        db.JornaltBills.Add(recbill);
                        db.SaveChanges();
                    };
                }
                if ( vmodel.invoicedataref != null)
                {
                    
                    if (BranchCheck == Status.active)
                    {
                        Branch = vmodel.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    decimal PayTotal = Convert.ToDecimal(vmodel.Paying);
            
                    decimal PaytoAmount = PayTotal ;
                    var billCheck = com.BillClearJornal((long)vmodel.payfrom1, PaytoAmount, JournalId, Date, BranchID, UserId, null, vmodel.invoicedataref);
                    Astatus = null;
                    // if payment done update to transaction  \\creit      //debit   
                }

                if (vmodel.invoicedataref2 != null)
                {

                    if (BranchCheck == Status.active)
                    {
                        Branch = vmodel.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    decimal PayTotal = Convert.ToDecimal(vmodel.Paying);

                    decimal PaytoAmount = PayTotal;
                    var billCheck = com.BillClearJornal((long)vmodel.payfrom2, PaytoAmount, JournalId, Date, BranchID, UserId, null, vmodel.invoicedataref2);
                    Astatus = null;
                    // if payment done update to transaction  \\creit      //debit   
                }

                if(vmodel.invoicedatapay!=null)
                {

                    if (BranchCheck == Status.active)
                    {
                        Branch = vmodel.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    decimal PayTotal = Convert.ToDecimal(vmodel.Paying);
                    var Balance = com.Accbalance((long)vmodel.PayTo);
                    string acctype = Convert.ToString(Balance["acctype"]);
                    decimal PaytoAmount = PayTotal;
                    var billCheck = com.BillClearPaymentjornal((long)vmodel.PayTo,PayTotal, JournalId, Date, BranchID, UserId, acctype, null, vmodel.invoicedatapay);
                    Astatus = null;
                }
                if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                {
                    if (vmodel.MOPayment == ModeOfPayment.PDC)
                    {
                        Astatus = true;
                    }
                   
                    if (vmodel.invoicedataref != null && vmodel.invoicedataref.Where(p => p.Type == "Against Reference").ToList() != null)
                    {
                        Bills = String.Join(";", vmodel.invoicedataref.Where(p => p.Type == "Against Reference").Select(p => p.InvoiceNo.ToString()).ToArray());
                    }
                    PDC pd = new PDC
                    {
                        PDCDate = (DateTime)pdcDate,
                        PDCType = "Journal",
                        Reference = JournalId,
                        CheckNo = vmodel.CheckNo,
                        Bank = vmodel.Bank,
                        Note = vmodel.pdcNote,
                        RegStatus = choice.No,
                        Status = Status.active,
                        CreatedBy = UserId,
                        CreatedDate = today,
                        Branch = Branch,
                        editable = choice.Yes,
                        Bills = Bills,
                        Type = (vmodel.MOPayment == ModeOfPayment.PDC) ? 0 : 1,
                    };
                    db.PDCs.Add(pd);
                    db.SaveChanges();
                }


                foreach (var jnVal in vmodel.jnlitems)
                {
                    if (jnVal.Debit > 0 || jnVal.Credit > 0)
                    {
                        if (jnVal.AccType == 0)//debit
                        {
                            com.addAccountTrasaction(Convert.ToDecimal(jnVal.Debit), 0, jnVal.AccountID, "Journal", JournalId, DC.Debit, Date, Astatus, jnVal.Narration, jnVal.ProjectId, jnVal.TaskId);
                        }
                        else
                        {
                            com.addAccountTrasaction(0, Convert.ToDecimal(jnVal.Credit), jnVal.AccountID, "Journal", JournalId, DC.Credit, Date, Astatus, jnVal.Narration, jnVal.ProjectId, jnVal.TaskId);
                        }
                    }
                }
                com.clearsepayment();
                com.addlog(LogTypes.Created, UserId, "Journal", "Journals", findip(), JournalId, "Successfully added Journal details");
                if (vmodel.submittype == "print")
                {
                    vmodel.MOPay = Enum.GetName(typeof(ModeOfPayment), vmodel.MOPayment);
                    vmodel.UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.UserName).FirstOrDefault();
                    vmodel.Date = Date.ToString("dd-MM-yyyy");
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    vmodel.ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    vmodel.jnlitems = (from a in db.AccountsTransactions
                                       join b in db.Accountss on a.Account equals b.AccountsID
                                       join p in db.Projects on a.Project equals p.ProjectId into proj
                                       from p in proj.DefaultIfEmpty()
                                       join t in db.ProTasks on a.ProTask equals t.ProTaskId into protask
                                       from t in protask.DefaultIfEmpty()
                                       where a.reference == JournalId && a.Purpose == "Journal"
                                       select new JournalVItems
                                       {
                                           AccType = (a.Type == 0) ? 0 : 1,
                                           AccountID = a.Account,
                                           Debit = a.Debit,
                                           Credit = a.Credit,
                                           Narration = a.Narration != null ? a.Narration : "",
                                           AccountName = b.Name,
                                           ProjectName = p.ProjectName,
                                           TaskName = t.TaskName,
                                       }).ToList();
                }
                var fmapp = db.FieldMappings.Where(a => a.Section == "Journal" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                msg = "Successfully Created Journal details.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, type = vmodel.submittype, message = msg, fmapp = fmapp } };
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
        }


   
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Journal rpt = db.Journals.Find(id);
            
            if (rpt == null)
            {
                return NotFound();
            }
            string Mop =Convert.ToString(rpt.MOPayment);
            //Fetching values from AttachmentDocuments
         

            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.Journals
                             on a.TransactionID equals b.JournalId
                             where a.TransactionID == id && a.TransactionType == "Journal"
                             select new JournalDocumentViewModel
                             {
                                 DocumentID     =   a.DocumentID,
                                 JournalId      =   a.TransactionID,
                                 FileName       =   a.FileName,
                                 CreatedDate    =   a.CreatedDate
                             }).ToList();

            ViewBag.JournalID = id;
            
            var Journal = new JournalViewModel
            {
                VoucherNo = rpt.VoucherNo,
                Date = rpt.Date.ToString("dd-MM-yyyy"),
                //PayFrom1 = rpt.PayFrom1,
                //PayTo = rpt.PayTo,
                Remark = rpt.Remark,
                Paying = rpt.Paying,
                Branch = rpt.Branch,
                VATNature = rpt.VATNature,
                Ref1 = rpt.Ref1,
                Ref2 = rpt.Ref2,
                Ref3 = rpt.Ref3,
                Ref4 = rpt.Ref4,
                Ref5 = rpt.Ref5,
                PayTo=rpt.PayTo,
                MOPayment = (ModeOfPayment)Enum.Parse(typeof(ModeOfPayment), Mop), 
            
              
                PDCDate = rpt.PDCDate != null ? (rpt.PDCDate).Value.ToString("dd-MM-yyyy") : rpt.PDCDate.ToString(),
                InvoiceNo =rpt.InvoiceNo,
                Bank = db.PDCs.Where(p => (p.Reference == rpt.JournalId && p.PDCType == "Journal")).Select(p => p.Bank).FirstOrDefault(),
                CheckNo = db.PDCs.Where(p => (p.Reference == rpt.JournalId && p.PDCType == "Journal")).Select(p => p.CheckNo).FirstOrDefault(),
            };
            Journal.jnlitems = (from a in db.AccountsTransactions
                                join b in db.Accountss on a.Account equals b.AccountsID
                                join p in db.Projects on a.Project equals p.ProjectId into proj
                                from p in proj.DefaultIfEmpty()
                                join t in db.ProTasks on a.ProTask equals t.ProTaskId into protask
                                from t in protask.DefaultIfEmpty()
                                where a.reference == rpt.JournalId && a.Purpose == "Journal"
                                select new JournalVItems2
                                {
                                    AccType = (a.Type == 0) ? 0 : 1,
                                    AccountID = a.Account,
                                    Debit = a.Debit,
                                    Credit = a.Credit,
                                    Narration = a.Narration,
                                    AccountName = b.Name,
                                    ProjectId = a.Project,
                                    ProjectName = p.ProjectName,
                                    TaskId = a.ProTask,
                                    TaskName = t.TaskName
                                }).ToList();
           
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;
            if (BranchCheck == Status.active)
            {
                var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
                ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
            }
            

            ViewBag.Paidfrom2 = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                            }, "Value", "Text", 1); ;
           
            var ToReceipt = db.EnableSettings.Where(a => a.EnableType == "BillToBillReceipt").FirstOrDefault();
            var BillTo = ToReceipt != null ? (ToReceipt.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToReceipt = BillTo;
            var ToPayment = db.EnableSettings.Where(a => a.EnableType == "BillToBillPayment").FirstOrDefault();
            var BillTo2 = ToPayment != null ? (ToPayment.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToPayment = BillTo2;
            var InvoiceNo = db.Journals.Where(a => a.InvoiceNo == rpt.InvoiceNo).
                                Select(r => new
                                {
                                    ID = r.InvoiceNo,
                                    Name = r.InvoiceNo,
                                }).ToList();

            ViewBag.InvoiceNo = QkSelect.List(InvoiceNo, "ID", "Name");
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            //    .Select(s => new
            //        ID = s.ProjectId,
            //        Name = s.ProjectName
            //    })

            // .Select(s => new
            //     ID = s.ProTaskId,
            //     Name = s.TaskName
            // })
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //companyinfo
            companySet();
            ViewBag.preEntry = db.Journals.Where(a => a.JournalId < id && a.editable == choice.Yes).Select(a => a.JournalId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Journals.Where(a => a.JournalId > id && a.editable == choice.Yes).Select(a => a.JournalId).DefaultIfEmpty().Min();

            var AccJnl = db.EnableSettings.Where(a => a.EnableType == "AccInJournal").FirstOrDefault();
            var AccJnls = AccJnl != null ? AccJnl.Status : Status.inactive;
            ViewBag.AccJnl = AccJnls;

            //field mapping
            Journal.FieldMap = db.FieldMappings.Where(a => a.Section == "Journal" && a.Status == Status.active).ToList();

            //chk pdcregulated
            var pdcchk = db.PDCs.Where(x => x.PDCType == "Journal" && x.PDCRegDate != null && x.Reference == id).FirstOrDefault();
            ViewBag.chkPdc = pdcchk != null ? pdcchk.RegStatus : choice.No;
            _FinancialYear();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var aa = (from a in db.JornaltBills
                      where (a.Jornal == id)
                      select new
                      {
                          a.payfrom
                      }
                                    ).Distinct().ToList();
            if (aa.Count > 0)
            {
                if (aa[0] != null)
                    Journal.PayFrom1 = Convert.ToInt64(aa[0].payfrom);
                else
                    Journal.PayFrom1 = 0;
            }
                if (aa.Count() > 1)
            {
                if (aa[1] != null)
                    Journal.PayFrom2 = Convert.ToInt64(aa[1].payfrom);
                else
                    Journal.PayFrom2 = 0;
            }
            var PaidTo = db.Accountss.Where(a => a.AccountsID == Journal.PayTo && a.Group != 23).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).Where(o => o.ID == Journal.PayTo).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");
            var PaidFr = (from b in db.Accountss
                          select new SelectFormat
                          {
                              id = b.AccountsID,
                              text = b.Name

                          }).Where(o=>o.id==Journal.PayFrom1).ToList();
            SelectFormat abc = new SelectFormat()
            {
                id = 0,
                text = "Select Account"
            };
            PaidFr.Insert(0, abc);
            ViewBag.PaidTo = QkSelect.List(PaidFr, "id", "text");
            ViewBag.Paidfrom1 = QkSelect.List(PaidFr, "id", "text");
            return View(Journal);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Journal")]
        public ActionResult Edit(JournalVViewModel vmodel, long id)
        {
            string msg;
            bool stat;
            var Editable = db.Journals.Any(a => a.editable == choice.No && a.JournalId == id);
            if (Editable)
            {
                msg = "Sorry,This Journal Cannot be Editable.";
                stat = false;
            }
            else
            {

              











                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = vmodel.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }

                var joudet = db.Journals.Find(id);
                if (com.islocked("Journal", joudet.Date))
                {
                    msg = "This Entry Is Locked";

                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                Journal rec = joudet;
                Journal RecTemp = joudet;
                var today = Convert.ToDateTime(System.DateTime.Now);
                DateTime Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                long PayFrom1 = vmodel.jnlitems.Where(a => a.Credit != null && a.Credit != 0).Select(a => a.AccountID).FirstOrDefault();
                long PayTo1 = vmodel.jnlitems.Where(a => a.Debit != null && a.Debit != 0).Select(a => a.AccountID).FirstOrDefault();
                
                
                joudet.PayFrom = PayFrom1;
                joudet.PayTo = PayTo1;
                joudet.VoucherNo = vmodel.VoucherNo;
                db.Entry(joudet).State = EntityState.Modified;
                db.SaveChanges();
                  
                long PayFrom = vmodel.jnlitems.Where(a => a.Credit != null && a.Credit != 0).Select(a => a.AccountID).FirstOrDefault();
                long PayTo = (long)vmodel.PayTo;
                //----------------------------------------------------------------------------
                var JournalId = rec.JournalId;
                bool delete = com.DeleteAllAccountTransaction("Journal", rec.JournalId);
                var Bills = "";
                if (vmodel.invoicedataref != null && vmodel.invoicedataref.Where(p => p.Type == "Against Reference").ToList() != null)
                {
                    Bills = String.Join(";", vmodel.invoicedataref.Where(p => p.Type == "Against Reference").Select(p => p.InvoiceNo.ToString()).ToArray());
                }
                if (RecTemp.MOPayment == vmodel.MOPayment)
                {
                    if (RecTemp.MOPayment == ModeOfPayment.PDC || RecTemp.MOPayment == ModeOfPayment.CDC)
                    {
                        PDC pdc = db.PDCs.Where(a => (a.Reference == RecTemp.JournalId) && (a.PDCType == "Journal")).FirstOrDefault();
                        pdc.PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                        pdc.Note = vmodel.pdcNote;
                        pdc.CheckNo = vmodel.CheckNo;
                        pdc.Bank = vmodel.Bank;
                        pdc.Type = (vmodel.MOPayment == ModeOfPayment.PDC) ? 0 : 1;
                        pdc.Bills = Bills;
                       

                        db.Entry(pdc).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (RecTemp.MOPayment == ModeOfPayment.PDC || RecTemp.MOPayment == ModeOfPayment.CDC)
                    {
                        var pdcdel = db.PDCs.Where(a => (a.Reference == RecTemp.JournalId) && (a.PDCType == "Journal")).FirstOrDefault();
                        db.PDCs.Remove(pdcdel);
                        if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                        {
                            PDC pd = new PDC
                            {
                                PDCDate = DateTime.Parse(vmodel.PDCDate, new CultureInfo("en-GB")),
                                PDCType = "Journal",
                                Reference = RecTemp.JournalId,
                                CheckNo = vmodel.CheckNo,
                                Bank = vmodel.Bank,
                                Note = vmodel.pdcNote,
                                RegStatus = choice.No,
                                Status = Status.active,
                                CreatedBy = UserId,
                                CreatedDate = today,
                                Branch = Branch,
                                Type = (vmodel.MOPayment == ModeOfPayment.PDC) ? 0 : 1,
                                editable = choice.Yes,
                                Bills = Bills,
                            };
                            db.PDCs.Add(pd);
                        }
                    }
                    else
                    {
                        if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                        {
                            PDC pd = new PDC
                            {
                                PDCDate = DateTime.Parse(vmodel.PDCDate, new CultureInfo("en-GB")),
                                PDCType = "Journal",
                                Reference = RecTemp.JournalId,
                                CheckNo = vmodel.CheckNo,
                                Bank = vmodel.Bank,
                                Note = vmodel.pdcNote,
                                RegStatus = choice.No,
                                Status = Status.active,
                                CreatedBy = UserId,
                                CreatedDate = today,
                                Branch = Branch,
                                Type = (vmodel.MOPayment == ModeOfPayment.PDC) ? 0 : 1,
                                editable = choice.Yes,
                                Bills = null,
                            };
                            db.PDCs.Add(pd);
                        }
                    }
                }

                rec.Date = Date;
                rec.PayTo = vmodel.jnlitems.Where(a => a.Debit != null && a.Debit != 0).Select(a => a.AccountID).FirstOrDefault();

                rec.PayFrom = PayFrom;
                rec.Paying = (decimal)vmodel.Paying;
                rec.GrandTotal = (decimal)vmodel.Paying;
                rec.Remark = vmodel.Remark;
                rec.Branch = Branch;
                rec.VATNature = vmodel.VATNature;

                rec.Ref1 = vmodel.Ref1;
                rec.Ref2 = vmodel.Ref2;
                rec.Ref3 = vmodel.Ref3;
                rec.Ref4 = vmodel.Ref4;
                rec.Ref5 = vmodel.Ref5;

                rec.MOPayment = vmodel.MOPayment;
                bool? Astatus = null;
                if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                {
                    rec.PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                    if (RecTemp.MOPayment == vmodel.MOPayment)
                    {
                        Astatus = null;
                    }
                   else if (vmodel.MOPayment == ModeOfPayment.PDC)
                    {
                        Astatus = true;
                    }
                }
                else
                {
                    rec.PDCDate = null;
                }
                rec.InvoiceNo = vmodel.InvoiceNo;
                db.Entry(rec).State = EntityState.Modified;
                db.SaveChanges();
                foreach (var jnVal in vmodel.jnlitems)
                {
                    if (jnVal.Debit > 0 || jnVal.Credit > 0)
                    {
                        var pdc=db.PDCs.Where(a => (a.Reference == RecTemp.JournalId) && (a.PDCType == "Journal")).FirstOrDefault();
                        if(pdc!=null&&vmodel.MOPayment==ModeOfPayment.PDC)
                        {
                            if (pdc.RegStatus == choice.No)
                                Astatus = true;
                            else
                                Astatus = null;

                        }
                        if (jnVal.AccType == 0)//debit
                        {
                            com.addAccountTrasaction(Convert.ToDecimal(jnVal.Debit), 0, jnVal.AccountID, "Journal", JournalId, DC.Debit, Date, Astatus, jnVal.Narration, jnVal.ProjectId, jnVal.TaskId);
                        }
                        else
                        {
                            com.addAccountTrasaction(0, Convert.ToDecimal(jnVal.Credit), jnVal.AccountID, "Journal", JournalId, DC.Credit, Date, Astatus, jnVal.Narration, jnVal.ProjectId, jnVal.TaskId);
                        }
                    }
                }
                bool? stats = null;
                if (vmodel.MOPayment == ModeOfPayment.PDC)
                {
                    stats = true;
                }
                var vdate = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                if (vmodel.payfrom1 == null)
                {
                    var Dsupplierid = (from a in db.Suppliers where a.Accounts == vmodel.PayTo select new { a.SupplierID }).SingleOrDefault();
                    if (Dsupplierid != null)
                    {
                        decimal Amtsum = 0;
                        var data = (from a in db.PETransactions
                                    where a.SupplierId == Dsupplierid.SupplierID && a.PaymentId == id
                                    orderby a.PETransactionId
                                    select new
                                    {
                                        a.PurchaseEntry,
                                        a.PEPayAmount
                                    }).ToList();
                        if (data.Count > 0)
                        {
                            foreach (var ditem in data)
                            {
                                var paying = ditem.PEPayAmount;
                                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.PurchaseEntry).FirstOrDefault();
                                PEP.PEPaidAmount = PEP.PEPaidAmount - Convert.ToDecimal(paying);
                                db.Entry(PEP).State = EntityState.Modified;
                                db.SaveChanges();

                                Amtsum += ditem.PEPayAmount;
                            }
                            db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == id));
                            db.SaveChanges();
                            if (vmodel.GrandTotal > Amtsum)
                            {
                                decimal payAmt = vmodel.GrandTotal - Amtsum;
                                foreach (var ditem in data)
                                {
                                    if (payAmt > 0)
                                    {
                                        PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.PurchaseEntry).FirstOrDefault();
                                        //add to petransactions
                                        PETransaction PEPT = new PETransaction();
                                        PEPT.PurchaseEntry = PEP.PurchaseEntry;
                                        PEPT.SupplierId = Convert.ToInt64(PEP.SupplierId);
                                        PEPT.PEPayDate = vdate;
                                        PEPT.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                        PEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                        PEPT.CreatedUserId = UserId;
                                        PEPT.PaymentId = JournalId;
                                        PEPT.Status = 0;
                                        // transaction 
                                        var balnceamount = PEP.PEBillAmount - PEP.PEPaidAmount;
                                        if (balnceamount >= payAmt)
                                        {
                                            PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(payAmt);
                                            PEPT.PEPayAmount = Convert.ToDecimal(payAmt);
                                            payAmt = 0;

                                        }
                                        else
                                        {
                                            PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(balnceamount);
                                            PEPT.PEPayAmount = Convert.ToDecimal(balnceamount);
                                            payAmt -= balnceamount;
                                        }
                                        PEP.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                        if (PEP.PEBillAmount == PEP.PEPaidAmount)
                                        {
                                            PEP.Status = 1;
                                        }
                                        db.Entry(PEP).State = EntityState.Modified;
                                        db.SaveChanges();
                                        db.PETransactions.Add(PEPT);
                                        db.SaveChanges();

                                    }
                                }
                            }
                        }
                    }

                }



















                var firstac = db.JornaltBills.Where(o => o.Jornal == id).Select(o => o.payfrom).FirstOrDefault();

                var Customerid = (from a in db.Customers where a.Accounts == firstac select new { a.CustomerID }).SingleOrDefault();
                if (Customerid != null)
                {
                    decimal Amtsum = 0;
                    var data = (from a in db.SETransactions
                                where a.CustomerId == Customerid.CustomerID && a.Recieptid == id
                                orderby a.SETransactionId
                                select new
                                {
                                    a.SalesEntry,
                                    a.SEPayAmount
                                }).ToList();
                    if (data.Count > 0)
                    {
                        foreach (var ditem in data)
                        {
                            var paying = ditem.SEPayAmount;
                            SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.SalesEntry).FirstOrDefault();
                            if (SEP != null)
                            {
                                SEP.SEPaidAmount = SEP.SEPaidAmount - Convert.ToDecimal(paying);
                                db.Entry(SEP).State = EntityState.Modified;
                                db.SaveChanges();
                            }

                            Amtsum += ditem.SEPayAmount;
                        }
                        db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == id));
                        db.SaveChanges();
                        if (vmodel.GrandTotal > Amtsum)
                        {
                            decimal payAmt = vmodel.GrandTotal - Amtsum;
                            foreach (var ditem in data)
                            {
                                if (payAmt > 0)
                                {
                                    SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.SalesEntry).FirstOrDefault();
                                    //add to petransactions
                                    SETransaction SEPT = new SETransaction();
                                    SEPT.SalesEntry = SEP.SalesEntry;
                                    SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                                    SEPT.SEPayDate = Date;
                                    SEPT.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                    SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                    SEPT.CreatedUserId = UserId;
                                    SEPT.Status = 0;

                                    SEPT.Recieptid = rec.JournalId;
                                    // transaction 
                                    var balnceamount = SEP.SEBillAmount - SEP.SEPaidAmount;
                                    if (balnceamount >= payAmt)
                                    {
                                        SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(payAmt);
                                        SEPT.SEPayAmount = Convert.ToDecimal(payAmt);
                                        payAmt = 0;
                                    }
                                    else
                                    {
                                        SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(balnceamount);
                                        SEPT.SEPayAmount = Convert.ToDecimal(balnceamount);
                                        payAmt -= balnceamount;
                                    }
                                    SEP.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                    if (SEP.SEBillAmount == SEP.SEPaidAmount)
                                    {
                                        SEP.Status = 1;
                                    }
                                    db.Entry(SEP).State = EntityState.Modified;
                                    db.SaveChanges();
                                    db.SETransactions.Add(SEPT);
                                    db.SaveChanges();
                                }
                            }
                        }

                    }
                }


                var retval = 0;
                long[] bill = null;
                if (vmodel.invoicedataref != null)

                {
                    if (vmodel.invoicedataref.Where(a => a.Type == "Against Reference").ToList() != null)
                    {
                        bill = vmodel.invoicedataref.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        Bills = String.Join(";", bill.Select(p => p.ToString()).ToArray());
                    }

                    var RecBill = db.JornaltBills.Where(a => a.Jornal == id).FirstOrDefault();
                    if (RecBill != null)
                    {
                        var RBill = db.JornaltBills.Where(a => a.Jornal == id).ToList();
                        foreach (var arr in RBill)
                        {
                            //add to dummy table
                            DummyJornalBill bills = new DummyJornalBill();
                            bills.Jornal = arr.Jornal;
                            bills.InvoiceNo = arr.InvoiceNo;
                            bills.BillType = arr.BillType;
                            bills.Amount = arr.Amount;
                            bills.Type = arr.Type;
                            bills.NewRefName = arr.NewRefName;
                            bills.Status = arr.Status;

                            db.DummyJornalBills.Add(bills);
                            db.SaveChanges();
                        }

                        db.JornaltBills.RemoveRange(db.JornaltBills.Where(a => a.Jornal == id));
                       db.SaveChanges();

                    }

               










                    JornalBill recbill = new JornalBill();
                    foreach (var arr in vmodel.invoicedataref)
                    {
                        if (arr.Type == "Against Reference")
                        {
                            decimal payAmt = db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).Select(a => a.SEPayAmount).Sum() ?? 0 : 0;
                            if (payAmt > 0)
                            {
                                SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == arr.InvoiceNo).FirstOrDefault();
                                SEpay.SEPaidAmount = SEpay.SEPaidAmount - payAmt;
                                db.Entry(SEpay).State = EntityState.Modified;

                                db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == 0 && a.SalesEntry == arr.InvoiceNo));
                                db.SaveChanges();
                            }

                            decimal payAmtTr = db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).Select(a => a.PRPayAmount).Sum() ?? 0 : 0;
                            if (payAmtTr > 0)
                            {
                                PRPayment PRpay = db.PRPayments.Where(a => a.PurchaseReturnId == arr.InvoiceNo).FirstOrDefault();
                                PRpay.PReturnAmount = PRpay.PReturnAmount - payAmtTr;
                                db.Entry(PRpay).State = EntityState.Modified;

                                db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == 0 && a.PurchaseReturnId == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                        }

                        recbill.Jornal = id;
                        recbill.InvoiceNo = arr.InvoiceNo;
                        recbill.BillType = arr.BillType;
                        recbill.Amount = arr.Amount;
                        recbill.Type = arr.Type; //arr.Type;
                        recbill.NewRefName = arr.NewRefName;
                        recbill.Status = arr.Status;
                        recbill.payfrom = vmodel.payfrom1;

                        db.JornaltBills.Add(recbill);
                        retval = db.SaveChanges();
                    };
                }


              



                var secac = db.JornaltBills.Where(o => o.Jornal == id).Select(o => o.payfrom).Distinct().ToList();
                if (secac.Count() > 1)
                {
                    var Customerid2 = (from a in db.Customers where a.Accounts == secac[1] select new { a.CustomerID }).SingleOrDefault();
                    if (Customerid2 != null)
                    {
                        decimal Amtsum = 0;
                        var data = (from a in db.SETransactions
                                    where a.CustomerId == Customerid2.CustomerID && a.Recieptid == id
                                    orderby a.SETransactionId
                                    select new
                                    {
                                        a.SalesEntry,
                                        a.SEPayAmount
                                    }).ToList();
                        if (data.Count > 0)
                        {
                            foreach (var ditem in data)
                            {
                                var paying = ditem.SEPayAmount;
                                SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.SalesEntry).FirstOrDefault();
                                SEP.SEPaidAmount = SEP.SEPaidAmount - Convert.ToDecimal(paying);
                                db.Entry(SEP).State = EntityState.Modified;
                                db.SaveChanges();

                                Amtsum += ditem.SEPayAmount;
                            }
                            db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == id));
                            db.SaveChanges();
                            if (vmodel.GrandTotal > Amtsum)
                            {
                                decimal payAmt = vmodel.GrandTotal - Amtsum;
                                foreach (var ditem in data)
                                {
                                    if (payAmt > 0)
                                    {
                                        SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.SalesEntry).FirstOrDefault();
                                        //add to petransactions
                                        SETransaction SEPT = new SETransaction();
                                        SEPT.SalesEntry = SEP.SalesEntry;
                                        SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                                        SEPT.SEPayDate = Date;
                                        SEPT.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                        SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                        SEPT.CreatedUserId = UserId;
                                        SEPT.Status = 0;

                                        SEPT.Recieptid = rec.JournalId;
                                        // transaction 
                                        var balnceamount = SEP.SEBillAmount - SEP.SEPaidAmount;
                                        if (balnceamount >= payAmt)
                                        {
                                            SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(payAmt);
                                            SEPT.SEPayAmount = Convert.ToDecimal(payAmt);
                                            payAmt = 0;
                                        }
                                        else
                                        {
                                            SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(balnceamount);
                                            SEPT.SEPayAmount = Convert.ToDecimal(balnceamount);
                                            payAmt -= balnceamount;
                                        }
                                        SEP.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                        if (SEP.SEBillAmount == SEP.SEPaidAmount)
                                        {
                                            SEP.Status = 1;
                                        }
                                        db.Entry(SEP).State = EntityState.Modified;
                                        db.SaveChanges();
                                        db.SETransactions.Add(SEPT);
                                        db.SaveChanges();
                                    }
                                }
                            }

                        }
                    }
                }

                if (vmodel.invoicedataref2 != null)

                {
                    if (vmodel.invoicedataref2.Where(a => a.Type == "Against Reference").ToList() != null)
                    {
                        bill = vmodel.invoicedataref2.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        Bills = String.Join(";", bill.Select(p => p.ToString()).ToArray());
                    }

                    var RecBill = db.JornaltBills.Where(a => a.Jornal == id).FirstOrDefault();
                    if (RecBill != null)
                    {
                        var RBill = db.JornaltBills.Where(a => a.Jornal == id).ToList();
                        foreach (var arr in RBill)
                        {
                            //add to dummy table
                            DummyJornalBill bills = new DummyJornalBill();
                            bills.Jornal = arr.Jornal;
                            bills.InvoiceNo = arr.InvoiceNo;
                            bills.BillType = arr.BillType;
                            bills.Amount = arr.Amount;
                            bills.Type = arr.Type;
                            bills.NewRefName = arr.NewRefName;
                            bills.Status = arr.Status;

                            db.DummyJornalBills.Add(bills);
                            db.SaveChanges();
                        }


                    }











                    JornalBill recbill = new JornalBill();
                    foreach (var arr in vmodel.invoicedataref2)
                    {
                        if (arr.Type == "Against Reference")
                        {
                            decimal payAmt = db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).Select(a => a.SEPayAmount).Sum() ?? 0 : 0;
                            if (payAmt > 0)
                            {
                                SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == arr.InvoiceNo).FirstOrDefault();
                                SEpay.SEPaidAmount = SEpay.SEPaidAmount - payAmt;
                                db.Entry(SEpay).State = EntityState.Modified;

                                db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == 0 && a.SalesEntry == arr.InvoiceNo));
                                db.SaveChanges();
                            }

                            decimal payAmtTr = db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).Select(a => a.PRPayAmount).Sum() ?? 0 : 0;
                            if (payAmtTr > 0)
                            {
                                PRPayment PRpay = db.PRPayments.Where(a => a.PurchaseReturnId == arr.InvoiceNo).FirstOrDefault();
                                PRpay.PReturnAmount = PRpay.PReturnAmount - payAmtTr;
                                db.Entry(PRpay).State = EntityState.Modified;

                                db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == 0 && a.PurchaseReturnId == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                        }

                        recbill.Jornal = id;
                        recbill.InvoiceNo = arr.InvoiceNo;
                        recbill.BillType = arr.BillType;
                        recbill.Amount = arr.Amount;
                        recbill.Type = arr.Type; //arr.Type;
                        recbill.NewRefName = arr.NewRefName;
                        recbill.Status = arr.Status;
                        recbill.payfrom = vmodel.payfrom2;

                        db.JornaltBills.Add(recbill);
                        retval = db.SaveChanges();
                    };
                }





                if (vmodel.invoicedatapay != null)
                {
                    if (vmodel.invoicedatapay.Where(a => a.Type == "Against Reference").ToList() != null)
                    {
                        bill = vmodel.invoicedatapay.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        Bills = String.Join(";", bill.Select(p => p.ToString()).ToArray());
                    }

                    var RecBill2 = db.JornalPaymentBills.Where(a => a.Jornal == id).FirstOrDefault();
                    if (RecBill2 != null)
                    {
                        var PBill = db.JornalPaymentBills.Where(a => a.Jornal == id).ToList();
                        foreach (var arr in PBill)
                        {
                            //add to dummy table
                            DummyPaymentBill bills = new DummyPaymentBill();
                            bills.Payment = arr.Jornal;
                            bills.InvoiceNo = arr.InvoiceNo;
                            bills.BillType = arr.BillType;
                            bills.Amount = arr.Amount;
                            bills.Type = arr.Type;
                            bills.NewRefName = arr.NewRefName;
                            bills.Status = arr.Status;
                            db.DummyPaymentBills.Add(bills);
                            db.SaveChanges();
                        }

                        db.JornalPaymentBills.RemoveRange(db.JornalPaymentBills.Where(a => a.Jornal == id));
                        db.SaveChanges();

                    }
                    JornalPaymentBill paybill = new JornalPaymentBill();
                    foreach (var arr in vmodel.invoicedatapay)
                    {
                        if (arr.Type == "Against Reference")
                        {
                            decimal payAmt = db.PETransactions.Where(a => a.PurchaseEntry == arr.InvoiceNo && a.PaymentId == 0).FirstOrDefault() != null ? (decimal?)db.PETransactions.Where(a => a.PaymentId == arr.InvoiceNo && a.PaymentId == 0).Select(a => a.PEPayAmount).Sum() ?? 0 : 0;
                            if (payAmt > 0)
                            {
                                PEPayment PEpay = db.PEPayments.Where(a => a.PurchaseEntry == arr.InvoiceNo).FirstOrDefault();
                                PEpay.PEPaidAmount = PEpay.PEPaidAmount - payAmt;
                                db.Entry(PEpay).State = EntityState.Modified;

                                db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == 0 && a.PurchaseEntry == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                            decimal? payAmtSr = db.SRTransactions.Where(a => a.SalesReturnId == arr.InvoiceNo && a.PaymentId == 0).FirstOrDefault() != null ? (decimal?)db.SRTransactions.Where(a => a.SalesReturnId == arr.InvoiceNo && a.PaymentId == 0).Select(a => a.SRPayAmount).Sum() ?? 0 : 0;
                            if (payAmtSr > 0)
                            {
                                SRPayment SRpay = db.SRPayments.Where(a => a.SalesReturnId == arr.InvoiceNo).FirstOrDefault();
                                SRpay.SReturnAmount =(decimal)( SRpay.SReturnAmount - payAmtSr);
                                db.Entry(SRpay).State = EntityState.Modified;

                                db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == 0 && a.SalesReturnId == arr.InvoiceNo));
                                db.SaveChanges();
                            }
                        }

                        paybill.Jornal = id;
                        paybill.InvoiceNo = arr.InvoiceNo;
                        paybill.BillType = arr.BillType;
                        paybill.Amount = arr.Amount;
                        paybill.Type = arr.Type; //arr.Type;
                        paybill.NewRefName = arr.NewRefName;
                        paybill.Status = arr.Status;

                        db.JornalPaymentBills.Add(paybill);
                        retval = db.SaveChanges();
                    };
                }
                if (retval > 0)
                {
                    db.DummyPaymentBills.RemoveRange(db.DummyPaymentBills.Where(a => a.Payment == id));
                    db.SaveChanges();
                }










                if (retval > 0)
                {
                    db.DummyReceiptBills.RemoveRange(db.DummyReceiptBills.Where(a => a.Receipt == id));
                    db.SaveChanges();
                }
                DateTime? pdcDate = null;
                //stats == null && !(vmodel.MOPayment == ModeOfPayment.PDC && (RecTemp.MOPayment == ModeOfPayment.Cash || RecTemp.MOPayment == ModeOfPayment.CDC))
                if (1==1)
                {

                    if (vmodel.invoicedataref != null)
                    {
                        var billCheck = com.BillClearJornal((long)vmodel.payfrom1, (decimal)vmodel.Paying, rec.JournalId, Date, BranchID, UserId, null, vmodel.invoicedataref);
                    }
                    if (vmodel.invoicedataref2 != null)
                    {
                        var billCheck2 = com.BillClearJornal((long)vmodel.payfrom2, (decimal)vmodel.Paying, rec.JournalId, Date, BranchID, UserId, null, vmodel.invoicedataref2);
                    }
                    if (vmodel.invoicedatapay != null&& vmodel.invoicedataref2 == null& vmodel.invoicedataref == null)
                    {
                        var Balance = com.Accbalance((long)vmodel.PayTo);
                        string acctype = Convert.ToString(Balance["acctype"]);
                        var billCheck = com.BillClearPaymentjornal((long) vmodel.PayTo, (decimal)vmodel.Paying, rec.JournalId, Date, BranchID, UserId, acctype, null, vmodel.invoicedatapay);
                    }
                }



                com.addlog(LogTypes.Updated, UserId, "Journal", "Journals", findip(), rec.JournalId, "Journal Updated Successfully");
                com.clearsepayment();
                if (vmodel.submittype == "print")
                {
                    vmodel.MOPay = Enum.GetName(typeof(ModeOfPayment), vmodel.MOPayment);
                    vmodel.UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.UserName).FirstOrDefault();
                    vmodel.Date = Date.ToString("dd-MM-yyyy");
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    vmodel.ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    vmodel.jnlitems = (from a in db.AccountsTransactions
                                       join b in db.Accountss on a.Account equals b.AccountsID
                                       join p in db.Projects on a.Project equals p.ProjectId into proj
                                       from p in proj.DefaultIfEmpty()
                                       join t in db.ProTasks on a.ProTask equals t.ProTaskId into protask
                                       from t in protask.DefaultIfEmpty()

                                       where a.reference == JournalId && a.Purpose == "Journal"
                                       select new JournalVItems
                                       {
                                           AccType = (a.Type == 0) ? 0 : 1,
                                           AccountID = a.Account,
                                           Debit = a.Debit,
                                           Credit = a.Credit,
                                           Narration = a.Narration != null ? a.Narration : "",
                                           AccountName = b.Name,
                                           ProjectName = p.ProjectName,
                                           TaskName = t.TaskName,
                                       }).ToList();
                }
                msg = "Successfully Updated Journal details.";
                stat = true;
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            db.Approvals.RemoveRange(db.Approvals.Where(o => o.TransEntry == id && o.Type == "Journalapp"));
            db.SaveChanges();
            Approval approval = new Approval();
            var Approve = db.AssignedTos.Where(o => o.CustomerID == -1).Select(o => o.EmployeeId).ToArray();
            foreach (var emp in Approve)
            {
                approval.TransEntry = id;
                approval.Type = "Journalapp";
                approval.EmployeeId = emp;
                db.Approvals.Add(approval);
                db.SaveChanges();
            }

            var fmapp = db.FieldMappings.Where(a => a.Section == "Journal" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, message = msg, type = vmodel.submittype, fmapp = fmapp } };
        }


        [QkAuthorize(Roles = "Dev,Delete Journal")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Journal rec = db.Journals.Find(id);
            if (rec == null)
            {
                return NotFound();
            }
            return PartialView(rec);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Journal")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            #region Old Code
            #endregion
            var chk = DeleteJournal(id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Journal details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Journal")]
        public ActionResult DeleteAllJournal(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteJournal(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Journal, Unable to Delete " + notdel + " Journal. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Journal.", true);
            }
            else
            {
                Success("Deleted " + count + " Journal.", true);
            }
            return RedirectToAction("Index", "JournalV");
        }
        private Boolean DeleteJournal(long JrId)
        {
            var Editable = db.Journals.Any(a => a.editable == choice.No && a.JournalId == JrId);
            if (Editable)
            {
                return false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                Journal Rec = db.Journals.Find(JrId);

                if (Rec.MOPayment == ModeOfPayment.PDC || Rec.MOPayment == ModeOfPayment.CDC)
                {
                    var pdcdel = db.PDCs.Where(a => (a.Reference == Rec.JournalId) && (a.PDCType == "Journal")).FirstOrDefault();
                    if (pdcdel != null)
                    {
                        db.PDCs.Remove(pdcdel);
                        db.SaveChanges();
                    }
                }

                db.Journals.Remove(Rec);
                db.SaveChanges();

                /*********** Delete from AttachmentDocuments Table *********************/
                List<AttachmentDocuments> DocumentLists = new List<AttachmentDocuments>();

                //List all the documents attached corresponding to the JournalId
                DocumentLists = db.AttachmentDocuments.Where(a => (a.TransactionID == JrId && a.TransactionType == "Journal")).ToList();

                var i = 0;
                foreach(var row in DocumentLists)
                {
                    //To remove the attached file from folder
                    string FullPath = LegacyWeb.MapPath("~/uploads/JournalDocuments/" + DocumentLists.ElementAt(i).FileName);

                    if (System.IO.File.Exists(FullPath))
                    {
                        System.IO.File.Delete(FullPath);
                    }

                    //To remove the attached file from server
                    db.AttachmentDocuments.Remove(DocumentLists[i]);
                    i++;
                }
                db.SaveChanges();
                /***********************************************************************/

                bool delete = com.DeleteAllAccountTransaction("Journal", JrId);
                db.JornaltBills.RemoveRange(db.JornaltBills.Where(a => a.Jornal == JrId));

                db.SaveChanges();
                com.clearpepayment();
                com.clearsepayment();
                com.addlog(LogTypes.Deleted, UserId, "Journal", "Journals", findip(), JrId, "Journal Deleted Successfully");

                return true;
            }
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download Journal")]
        public ActionResult Download(long id)
        {
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            string HFCheck = ComHeadCheck.ToString();

            var JouDet = db.Journals.Where(s => s.JournalId == id).FirstOrDefault();
            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = JouDet.VoucherNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            return File(ms, "application/pdf", "Journal Voucher" + "-" /*+ accname + "-" */+ billno + ".pdf");


        }

        public StringBuilder generatePdf(long id)
        {

            var details = (from a in db.Journals
                           join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                           from b in payfrom.DefaultIfEmpty()
                           join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                           from c in payto.DefaultIfEmpty()
                           join g in db.Users on a.CreatedBy equals g.Id
                           where a.JournalId == id
                           select new
                           {
                               VoucherNo = a.VoucherNo,
                               Payer = b.Name,
                               Reciever = c.Name,
                               a.JournalId,
                               a.Date,
                               a.CreatedDate,
                               a.PayFrom,
                               a.PayTo,
                               a.SubTotal,
                               a.GrandTotal,
                               a.Paying,
                               a.editable,
                               User = g.UserName,
                               a.VATNature
                               //HFStatus = ComHeadCheck
                           }).FirstOrDefault();

            var invoices = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.Projects on a.Project equals c.ProjectId into pro
                            from c in pro.DefaultIfEmpty()
                            join d in db.ProTasks on a.ProTask equals d.ProTaskId into tsk
                            from d in tsk.DefaultIfEmpty()
                            where a.reference == id && a.Purpose == "Journal"
                            select new
                            {
                                AccType = (a.Type == 0) ? 0 : 1,
                                AccountID = a.Account,
                                Debit = a.Debit,
                                Credit = a.Credit,
                                Narration = a.Narration,
                                AccountName = b.Name,
                                c.ProjectName,
                                d.TaskName
                            }).ToList();


            var comdetails = db.companys
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
            string fsdata = "";

            if (details.VoucherNo != null && details.VoucherNo != "")
            {
                fsdata += details.VoucherNo;
            }
            if (details.Date != null)
            {
                fsdata += details.Date;
            }

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {

                    //                      "<tr style = 'border-top: 0px' >" +
                    //                      "<td width = '50%' style = 'padding: 0px 10px 0px 0px;border-right: 0px' >" +
                    //                      "<table class='table-nob'><tr><th></th><td></td><td></td></tr></table> " +
                    //                "</td><td style = 'padding:0px;' ><table class='table-nob jewel-cus' style='border-left: 1px solid #898989;border: 1px solid #000;width: 100%;height: auto;'>" +
                    //                "<tr><th>Voucher No</th><td>: </td></tr>" +
                    //                "<tr><th> Date </th><td>: </td></tr>" +
                    //                "<tr><th> Prepared By</th><td>: </td></tr>" +
                    //                "<tr><th> Time </th><td>: </td></tr>" +

                    //                 "<h4 style='margin-bottom: 5px;text-align: left;border: 1px solid #795548;background: #795548 !important;height: 6px;'>" +
                    //                 "<strong style='border:1px solid #000;padding:3px;background:#ffffff !important;'>JOURNAL VOUCHER</strong>" +


                    string vchname = "<table width='100%' style='border-collapse:collapse;font-size:12px;border: 0px; repeat-header:yes;'>" +
                                     "<tr><td width='30%' height='3%' rowspan='3' class='text-center' style='border: 1px solid #000;padding-top:10px;text-align: justify;font-size: 20px;font-weight: bold;'>JOURNAL VOUCHER " +
                                     "</td><td style='border: 0px;' height='1%' width='70%'></td></tr><tr><td width='70%' height='1%' style='border: 5px solid #000;background:#795548 !important;'></td></tr><tr><td width='70%' height='1%' style='border: 0px;'></td></tr></table>";

                    sb.Append(vchname);

                    string partyDetails = "<table width='100%' style='border-collapse:collapse;font-size:12px;border: 0px !important; repeat-header:yes;'>" +
                                          "<tr style = 'border-top: 0px' >" +
                                          "<td width='60%' style='padding: 0px 10px 0px 0px; border - right: 0px'>" +
                                          "<table style='border: 0px;'><tr><td width = '100%' style='border: 0px;'></td></tr></table> " +
                                          "</td>" +
                                          "<td width='40%' style='padding: 3px;vertical-align: top;border: .1px solid #000;'>" +
                                          "<table>" +
                                          "<tr><th style='border-bottom: .1px dotted #ccc;padding: 5px;'>Voucher No</th><td style='border-bottom: .1px dotted #ccc;padding: 5px;'>: " + details.VoucherNo + "</td></tr>" +
                                          "<tr><th style='border-bottom: .1px dotted #ccc;padding: 5px;'> Date </th><td style='border-bottom: .1px dotted #ccc;padding: 5px;'>: " + details.Date.ToString("dd-MM-yyyy") + "</td></tr>" +
                                          "<tr><th style='border-bottom: .1px dotted #ccc;padding: 5px;'> Prepared By</th><td style='border-bottom: .1px dotted #ccc;padding: 5px;'>: " + details.User + "</td></tr>";

                    if (details.VATNature == 1)
                    {
                        partyDetails += "<tr><th style='border-bottom: .1px dotted #ccc;padding: 5px;'> VAT Nature </th><td style='border-bottom: .1px dotted #ccc;padding: 5px;'>:<small> Registered Expense(B2B) </small></td></tr>";
                    }

                    partyDetails += "</table> ";
                    partyDetails += "</td></tr></table>";

                    sb.Append(partyDetails);


                    sb.Append("<table width='100%' style='margin-top: 10px;border-collapse:collapse;font-size:12px;border: .1px solid #ccc; repeat-header:yes;'>");
                    sb.Append("<thead>");
                    sb.Append("<tr style='font-size:13px;background: #ccc !important;'>");
                    sb.Append("<th rowspan='2' style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>S/N</th>");
                    sb.Append("<th rowspan='2' style='padding: 5px;vertical-align: top;text-align:center; border: .1px solid #ccc;'>Account Description</th>");

                    if (ProjChks == Status.active)
                    {
                        sb.Append("<th rowspan='2' style='padding: 5px;vertical-align: top;text-align:center; border: .1px solid #ccc;'>Prject</th>");
                        sb.Append("<th rowspan='2' style='padding: 5px;vertical-align: top;text-align:center; border: .1px solid #ccc;'>Task</th>");
                    }

                    sb.Append("<th colspan='2' style='padding: 5px;vertical-align: top;text-align:center; border: .1px solid #ccc;'>Amount (AED)</th>");
                    sb.Append("</tr>");
                    sb.Append("<tr style='font-size:13px;background: #ccc !important;'>");
                    sb.Append("<th style='padding: 5px;vertical-align: top;text-align:center; border: .1px solid #ccc;'>Debit (AED)</th>");
                    sb.Append("<th style='padding: 5px;vertical-align: top;text-align:center; border: .1px solid #ccc;'>Credit (AED)</th>");
                    sb.Append("</tr>");


                    sb.Append("</thead>");
                    sb.Append("<tbody>");
                    var itemcount = 0;
                    decimal debit = 0;
                    decimal credit = 0;
                    var colspan = "";
                    foreach (var item in invoices)
                    {
                        var accType = item.AccType == 1 ? "Credit" : "Debit";

                        var dr = item.Debit > 0 ? item.Debit.ToString() : "";
                        var cr = item.Credit > 0 ? item.Credit.ToString() : "";

                        sb.Append("<tr style='font-size:10px;'>");
                        sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + SI++ + "</td>");
                        sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>");
                        sb.Append(item.AccountName + "<br /><small>" + item.Narration + "</small>");
                        sb.Append("</td>");
                        if (ProjChks == Status.active)
                        {
                            colspan = "colspan='3'";
                            var pro = item.ProjectName != null ? item.ProjectName : "";
                            var tsk = item.TaskName != null ? item.TaskName : "";
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + pro + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + tsk + "</td>");
                        }


                        sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + dr + "</td>");
                        sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + cr + "</td>");

                        sb.Append("</tr>");

                        itemcount++;

                        debit += item.Debit;
                        credit += item.Credit;
                    }
                    var size = itemcount < 3 ? 580 : (itemcount < 10 ? 300 : (580 - (itemcount * 50)));
                    sb.Append("<tr><td height='" + size + "px' colspan='4'></td></tr>");

                    sb.Append("<tr style='font-size:10px;background: #ccc !important;'>");
                    sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>(" + itemcount + " Records)</td>");
                    sb.Append("<td " + colspan + " style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;text-align: right;'>Total Debit/Credit (AED)</td>");
                    sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align:right;border: .1px solid #ccc;'>" + debit + "</td>");
                    sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align:right;border: .1px solid #ccc;'>" + credit + "</td>");
                    sb.Append("</tr>");

                    sb.Append("</tbody>");
                    sb.Append("</table>");

                    sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                    sb.Append("<tr>");
                    sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>");
                    sb.Append("For " + comdetails.CName + "");
                    sb.Append("</div>");
                    sb.Append("</td>");
                    sb.Append("<td align='left' width='347px' style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>Receiver's Signature:<br />توقيع المتلقي</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                }
            }
            return sb;
        }





        public long JournalMaxvoucher()
        {
            Int64 SENo = 0;
            if ((db.Journals.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = 1;
            }
            else
            {
                SENo = db.Journals.Max(p => p.Voucher + 1);
            }

            return SENo;
        }
        public string JournalVoucherNo(Int64 SENo = 0, string billNo = null)
        {
            Int32 number = db.CodePrefixs.Where(a => a.section == "Journal").Select(a => a.number).FirstOrDefault();
            var prefix = db.CodePrefixs.Where(a => a.section == "Journal").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.Journals.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (prefix + 1) : (prefix + number);
                }
                else
                {
                    SENo = db.Journals.Max(p => p.Voucher + 1);
                    billNo = prefix + SENo;
                    if (journalBillExist(billNo))
                    {
                        billNo = JournalVoucherNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = prefix + SENo;
                if (journalBillExist(billNo))
                {
                    billNo = JournalVoucherNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool journalBillExist(string SENo)
        {
            var Exists = db.Journals.Any(c => c.VoucherNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private bool journalBillExist(string SENo, long? recid = null)
        {
            bool res;
            if (recid != null)
            {
                var Exists = db.Journals.Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
                return res;
            }
            else
            {
                var Exists = db.Journals.Where(a => a.JournalId != recid).Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
                return res;
            }
        }


        [RedirectingAction]
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View Journal")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var MOP = db.Receipts.Where(x => x.ReceiptId == id).Select(y => y.MOPayment).FirstOrDefault();
            ViewBag.MOPayment = (MOP == ModeOfPayment.Cash) ? 0 : 1;

            Journal rpt = db.Journals.Find(id);

            JournalVViewModel vmodel = new JournalVViewModel();

            vmodel = (from b in db.Journals
                      join f in db.PDCs on b.JournalId equals f.Reference into pdc
                      from f in pdc.DefaultIfEmpty()
                      where b.JournalId == id
                      select new JournalVViewModel
                      {
                          VoucherNo = b.VoucherNo,
                          MOPayment = b.MOPayment,
                          jouDate = (b.Date),
                          pdcdat = b.PDCDate,
                          CheckNo = f.CheckNo,
                          VATNature=b.VATNature,
                          GrandTotal=b.GrandTotal,
                          SubTotal=b.SubTotal,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                      }).FirstOrDefault();

            vmodel.jounlitems = (from a in db.AccountsTransactions
                      join b in db.Accountss on a.Account equals b.AccountsID
                      join p in db.Projects on a.Project equals p.ProjectId into proj
                      from p in proj.DefaultIfEmpty()
                      join t in db.ProTasks on a.ProTask equals t.ProTaskId into protask
                      from t in protask.DefaultIfEmpty()
                      where a.reference == id && a.Purpose == "Journal"
                      select new JournalVItems
                      {
                          AccType = (a.Type == 0) ? 0 : 1,
                          AccountID = a.Account,
                          Debit = a.Debit,
                          Credit = a.Credit,
                          Narration = a.Narration,
                          AccountName = b.Name,
                          ProjectId = a.Project,
                          ProjectName = p.ProjectName,
                          TaskId = a.ProTask,
                          TaskName = t.TaskName
                      }).ToList();
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Journal" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Function To Upload Files in Create and Edit Mode
        public ActionResult UploadFiles()
        {
          
            string Id = Request.Form.GetValues("id").First();
            long JournalID = 0;

            if (Id.Contains("undefined"))
            {
                var LastID  =   db.Journals.OrderByDescending(o => o.JournalId).FirstOrDefault();
                JournalID   =   LastID.JournalId;

            }
            else
            {
                JournalID = Convert.ToInt64(Id);
            }

            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/JournalDocuments/");

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

                                var FStatus     =   Status.active;
                                String newName  =   fileCount + extension;
                                string newFName =   fileCount + extension;
                                var thumbName   =   "";
                                var resizeName  =   "";

                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/JournalDocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/JournalDocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";
                                }

                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/JournalDocuments/"), newName);
                                file.SaveAs(newName);

                                var JDocument = new AttachmentDocuments
                                {
                                    TransactionID   =   JournalID,
                                    TransactionType =   "Journal",
                                    FileName        =   newFName,
                                    Status          =   FStatus,
                                    CreatedDate     =   Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.AttachmentDocuments.Add(JDocument);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/JournalDocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/JournalDocuments/"), resizeName);
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

        //To remove the attached file in Edit Mode(when pressing delete button in the attached file)
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Journal")]
        public JsonResult ImageDelete(long key)
        {
            bool status = false;
            string message;

            //To remove the attached file(single row) from database
            AttachmentDocuments Document = db.AttachmentDocuments.Find(key);
            db.AttachmentDocuments.Remove(Document);
            db.SaveChanges();

            //To remove the attached file from folder
            string fullpath = LegacyWeb.MapPath("~/uploads/JournalDocuments/" + Document.FileName);

            if (System.IO.File.Exists(fullpath))
            {
                System.IO.File.Delete(fullpath);
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "JournalV", "AttachmentDocuments", findip(), Document.DocumentID, "Journal Document Deleted Successfully");

            status = true;
            message = "Successfully deleted Journal Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = status, message = message, Id = key } };
        }



        public JsonResult SearchInvoiceNo(string q, string x, string page)
        {
            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "--select--";
            List<SelectUserFormat> serialisedJson;
            IList<long> acc;
           
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                var hmt = (from a in db.SalesEntrys
                           join b in db.Customers on a.Customer equals b.CustomerID into gp
                           from b in gp.DefaultIfEmpty()


                           where (a.BillNo.ToLower().Contains(q.ToLower()) || b.CustomerName.ToLower().Contains(q.ToLower()) || a.BillNo.Contains(q) || b.CustomerName.Contains(q))

                           select new SelectUserFormat
                           {
                               text = a.BillNo + " - " + b.CustomerName,
                               id =a.BillNo,
                          
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).Distinct().ToList();

                serialisedJson = hmt;
            }
            else
            {

                var hmt = (from a in db.SalesEntrys
                           join b in db.Customers on a.Customer equals b.CustomerID into gp
                           from b in gp.DefaultIfEmpty()


                          

                           select new SelectUserFormat
                           {
                               text =a.BillNo + " - " + b.CustomerName,
                               id =a.BillNo,
                               
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).Distinct().ToList();

                serialisedJson = hmt;
            }
                var initial = new SelectUserFormat() { id = "--Select--", text = "--Select--" };
                serialisedJson.Insert(0, initial);

            return Json(serialisedJson);
        }


    }
}
