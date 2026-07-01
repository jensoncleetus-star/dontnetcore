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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Drawing;

namespace QuickSoft.Controllers
{
    public class PRApprovalsController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PRApprovalsController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: PRApprovals
        [HttpGet]
        //[QkAuthorize(Roles = "Dev, Petty Cash Payment")]
        public ActionResult Create()
        {
            var UserId = User.Identity.GetUserId();
            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            // payfrom account
            //    ID = r.AccountsID,
            //    Name = r.Name

            var Paidfrom = (from a in db.Accountss
                            join b in db.accountmaps on a.AccountsID equals b.AccountId
                            where b.EmployeeId == EmpId && b.PaymentTypeId == EmployeePaymentType.Cash
                            select new
                            {
                                ID = a.AccountsID,
                                Name = a.Name
                            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(Paidfrom, "ID", "Name");

            var userid = User.Identity.GetUserId();
            var eimpid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            var PaidTo = (from a in db.accountmaps
                          join b in db.Accountss on a.AccountId equals b.AccountsID
                          where a.PaymentTypeId == EmployeePaymentType.Account && a.EmployeeId == eimpid
                          select new
                          {
                              ID = a.AccountId,
                              Name = b.Name
                          }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            ViewBag.InvoiceNo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search InvoiceNo", Value = ""},
                             }, "Value", "Text", 1);

            ViewBag.Invoiceno = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Select InvoiceNo", Value = ""},
                             }, "Value", "Text", 1);

            var Invoice = db.SalesEntrys
              .Select(s => new
              {
                  Id = s.SENo,
                  Name = db.Customers.Where(a => a.CustomerID == s.Customer).FirstOrDefault()
              }).ToList();
            ViewBag.SetBranch = QkSelect.List(Invoice, "Id", "Name");

            var payment = new PaymentViewModel
            {
                VoucherNo = com.PayVoucherNo2(),
                Date = (System.DateTime.Now).ToString("dd-MM-yyyy"),
                Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList()
            };

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
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            //enable bill to bill payment
            var ToPayment = db.EnableSettings.Where(a => a.EnableType == "BillToBillPayment").FirstOrDefault();
            var BillTo = ToPayment != null ? (ToPayment.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToPayment = BillTo;

            //enable project
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            //companyinfo
            companySet();
            var userpermission = User.IsInRole("All Payment Entry");
            ViewBag.LastEntry = db.Payments.Where(p => (p.editable == choice.Yes) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.PaymentId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            _FinancialYear();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            //field mapping
            payment.FieldMap = db.FieldMappings.Where(a => a.Section == "Payment" && a.Status == Status.active).ToList();

            var MlPayment = db.EnableSettings.Where(a => a.EnableType == "Payment").FirstOrDefault();
            var MlPayments = MlPayment != null ? MlPayment.Status : Status.inactive;
            ViewBag.MlPayment = MlPayments;

            var appby = db.Employees.Where(a => a.UserStatus == true)
                      .Select(s => new
                      {
                          ID = s.EmployeeId,
                          Name = s.FirstName + " " + s.LastName
                      })
                      .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", payment);
            }
            else
            {
                return View(payment);
            }
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Petty Cash Payment")]
        public JsonResult Create(PaymentViewModel vmodel)
        {
            string msg;
            bool stat;
            if (!com.payBillExist2(Convert.ToString(vmodel.VoucherNo)))
            {

                //               where (a.CheckNo == vmodel.CheckNo && vmodel.PayFrom == b.PayFrom && a.PDCType == "Payment")

                //pdCheck == true && RepeatChequeNos == Status.inactive
                if (1 == 2)
                {
                    msg = "Check Number Already Exist ! Enter The Correct One..";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
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

                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                    DateTime? pdcDate = null;
                    if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                    {
                        pdcDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                    }


                    decimal SubTotal;
                    decimal Paying;
                    decimal GrandTotal;
                    decimal TaxAmount = Convert.ToDecimal(vmodel.TaxAmount);
                    decimal PayTotal = Convert.ToDecimal(vmodel.Paying);
                    decimal Discount = Convert.ToDecimal(vmodel.Discount);
                    decimal PaytoAmount = PayTotal - TaxAmount + Discount;
                    var Balance = com.Accbalance(vmodel.PayTo);
                    if (Balance["acctype"] == (object)"Expense")
                    {
                        SubTotal = vmodel.SubTotal;
                        GrandTotal = vmodel.GrandTotal;
                        Paying = vmodel.GrandTotal;
                    }
                    else
                    {
                        SubTotal = vmodel.SubTotal;
                        GrandTotal = vmodel.Paying;
                        Paying = vmodel.Paying;
                    }


                    IFormFile file = Request.Form.Files["RecieptDoc"];
                    Int64 PaymentId = com.addPayment2(Date, vmodel.PayFrom, vmodel.PayTo, SubTotal, Paying, GrandTotal, vmodel.Remark, UserId, Branch, 0, "Direct Payment", vmodel.TaxPer, vmodel.TaxAmount, vmodel.MOPayment, vmodel.Tax, pdcDate, choice.Yes, vmodel.CheckNo, vmodel.Bank, vmodel.pdcNote, vmodel.VoucherNo, vmodel.invoicedata, vmodel.Discount, vmodel.Project, vmodel.ProTask, vmodel.Ref1, vmodel.Ref2, vmodel.Ref3, vmodel.Ref4, vmodel.Ref5, vmodel.InvoiceNo, vmodel.PaymentStatus, vmodel.Override);

                    if (file != null)
                    {
                        var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        vmodel.Ref5 = fileNames;
                        // check the current payto account is an a direct expense

                        if (file.FileName != "")
                        {
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/RecieptDoc/");
                            if (!Directory.Exists(uploadUrl))
                                Directory.CreateDirectory(uploadUrl);
                            file.SaveAs(Path.Combine(uploadUrl, fileNames));

                        }

                    }

                    //Approved By
                    var Appby = Convert.ToString(vmodel.ApprovedBy);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = PaymentId;
                            approval.Type = "Payment Approval2";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    var MlPayment = db.EnableSettings.Where(a => a.EnableType == "Payment").FirstOrDefault();
                    var MlPayments = MlPayment != null ? MlPayment.Status : Status.inactive;
                    var fmapp = db.FieldMappings.Where(a => a.Section == "Payment" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, type = vmodel.submittype, message = msg, fmapp = fmapp } };


                    // if payment done update to transaction

                    //// Add log

                    if (vmodel.submittype == "print")
                    {
                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        vmodel.ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";

                        vmodel.MOPay = Enum.GetName(typeof(ModeOfPayment), vmodel.MOPayment);
                        vmodel.creditor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
                        vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
                        vmodel.GrandTotal = (vmodel.GrandTotal + vmodel.TaxAmount) - (vmodel.Discount);

                    }
                    var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    vmodel.BusinessType = BusinessType;
                    if (BusinessType == "Scaffold")
                    {
                        vmodel.User = db.Users.Where(x => x.Id == UserId).Select(y => y.UserName).FirstOrDefault();
                        var v = (from a in db.Suppliers
                                 join x in db.Accountss on a.Accounts equals x.AccountsID
                                 join b in db.Contacts on a.Contact equals b.ContactID into tmp
                                 from b in tmp.DefaultIfEmpty()
                                 join j in db.Mobiles on a.Contact equals j.Contact into mobi
                                 from j in mobi.DefaultIfEmpty()
                                 where (vmodel.creditor == a.SupplierName)
                                 select new
                                 {
                                     Mobile = (from ac in db.Mobiles
                                               where (ac.Contact == a.Contact)
                                               select new MobileViewModel
                                               {
                                                   Num = ac.MobileNum,
                                                   Name = ac.Name
                                               }).ToList(),
                                     Email = b.EmailId,
                                     Phone = b.Phone
                                 });

                        vmodel.Email = v.Select(x => x.Email).FirstOrDefault();
                        vmodel.Phone = v.Select(x => x.Phone).FirstOrDefault();

                        vmodel.mobmodel = v.Select(x => x.Mobile).FirstOrDefault();

                    }

                    var Oustanding = (from a in db.PurchaseEntrys
                                      join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                                      join e in db.PaymentMethods on a.PurchaseType equals e.PaymentMethodId into paymeth
                                      from e in paymeth.DefaultIfEmpty()
                                      join f in db.Suppliers on vmodel.creditor equals f.SupplierName into supp
                                      from f in supp.DefaultIfEmpty()
                                      where
                                      (a.Supplier == f.SupplierID)
                                      && ((((decimal?)a.PEGrandTotal ?? 0) - ((decimal?)c.PEPaidAmount)) > 0)
                                      select new
                                      {
                                          Date = a.PEDate,
                                          VoucherNo = a.BillNo,
                                          Balance = ((decimal?)a.PEGrandTotal ?? 0) - ((decimal?)c.PEPaidAmount ?? 0)
                                      }).ToList();
                    if (Oustanding.Count != 0)
                    {
                        vmodel.Totamt = Oustanding.Select(x => x.Balance).Sum();
                    }

                    msg = "Successfully Created Payment details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { tbldata = Oustanding, status = stat, data = vmodel, type = vmodel.submittype, message = msg, fmapp = fmapp } };
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
        //[QkAuthorize(Roles = "Dev, Petty Cash Payment Approvals")]
        public ActionResult Index()
        {
            var Paidfrom = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PayFrom = QkSelect.List(Paidfrom, "ID", "Name");

            ViewBag.PayTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.InvoiceNo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);


            var MlaMc = db.EnableSettings.Where(a => a.EnableType == "MLAMc").FirstOrDefault();
            var MlaMcs = MlaMc != null ? MlaMc.Status : Status.inactive;
            ViewBag.MLAMc = MlaMcs;

            List<SelectListItem> Modes = new List<SelectListItem>();
            Modes.Add(new SelectListItem() { Text = "Cash", Value = "0" });
            Modes.Add(new SelectListItem() { Text = "PDC", Value = "1" });
            ViewBag.PayMode = QkSelect.List(Modes, "Value", "Text");
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            _FinancialYear();
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Approvals")]
        public JsonResult GetPaymentApproval(string InvoiceNo, string SaleInvoiceNo, string FromDate, string ToDate, long? type, long? PayFrom, long? PayTo, string user)
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
                Paymode = ModeOfPayment.CDC;
            }

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var userpermission = User.IsInRole("All Payment Entry");
            var UserId = User.Identity.GetUserId();


            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();




            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Payment");
            var uDelete = User.IsInRole("Delete Payment");
            var uDownload = User.IsInRole("Download Payment");

            var acid = db.accountmaps.Select(o => o.AccountId).ToArray();

            // EF Core 10 cannot translate the app/AppStatus/chkAppStatus nested-collection + GroupBy-latest
            // lets. Materialize entity columns/scalars server-side into serverRows, then build client lookups
            // keyed by PaymentId (TransType == "Payment Approval2") and re-project client-side below.
            var serverQuery = (from a in db.DummyPayments

                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                     from c in payto.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id into User
                     from g in User.DefaultIfEmpty()

                     where (a.editable == choice.Yes)
                     && a.OverrideStatus == "inactive" &&
                     acid.Contains(a.PayFrom) &&
                     ((InvoiceNo == null || InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                     ((SaleInvoiceNo == null || SaleInvoiceNo == "0" || SaleInvoiceNo == "" || a.InvoiceNo == SaleInvoiceNo)) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     (type == null || a.MOPayment == Paymode) &&
                     (PayFrom == null || PayFrom == 0 || a.PayFrom == PayFrom) &&
                     (PayTo == null || PayTo == 0 || a.PayTo == PayTo))
                     && (user == null || user == "" || g.Id == user)
                     &&(a.stat==Status.active)
                     //    && (userpermission == true || a.CreatedBy == UserId)
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         SaleInvoiceNo = a.InvoiceNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.PaymentId,
                         a.Date,
                         a.MOPayment,
                         a.PDCDate,
                         a.PayFrom,
                         a.PayTo,
                         a.TaxAmount,
                         //a.SubTotal,
                         SubTotal = a.SubTotal != 0 ? a.SubTotal : a.GrandTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.editable,
                         a.Discount,
                         a.CreatedDate,
                     });

            // Performance (audit P2): server-side paging on the common path; search/computed sorts fall back unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "CreatedDate","Date","Discount","editable","GrandTotal","MOPayment","Payer","PayFrom","Paying","PaymentId","PayTo","PDCDate","Reciever","SaleInvoiceNo","SubTotal","TaxAmount","VoucherNo" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("PaymentId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by PaymentId (TransType == "Payment Approval2"); missing key -> empty list.
            var payIds = serverRows.Select(o => o.PaymentId).ToList();
            var appLookup = db.Approvals
                .Where(z => z.Type == "Payment Approval2" && payIds.Contains(z.TransEntry))
                .Select(z => new { z.TransEntry, z.EmployeeId })
                .ToList()
                .ToLookup(z => z.TransEntry);
            var appUpdRows = db.ApprovalUpdates
                .Where(y => y.Type == "Payment Approval2" && payIds.Contains(y.TransEntry))
                .Select(y => new { y.TransEntry, y.ApprovalStatus, y.ApprovedBy, y.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(y => y.TransEntry);
            var chkAppStatusLookup = appUpdRows
                .GroupBy(y => y.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(x => x.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.PaymentId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.PaymentId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.PaymentId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.PaymentId,
                         o.Date,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         o.MOPayment,
                         o.PDCDate,
                         o.PayFrom,
                         o.PayTo,
                         o.TaxAmount,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.editable,
                         Dev = uDev,
                         //Edit = uEdit,
                         //Delete = uDelete,
                         //Download = uDownload,
                         Discount = o.Discount,
                         o.CreatedDate,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : chkAppStatus.Contains(ApprovalStatus.Approved) ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval // (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,

                         };
                     }).Where(a => a.ApprovalStatus == ApprovalStatus.PendingApproval);
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p =>/* p.PaymentId.ToString().ToLower().Contains(search.ToLower()) ||*/
                                 p.VoucherNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.MOPayment.ToString().ToLower().Contains(search.ToLower())Reciept
                                 p.PayTo.ToString().ToLower().Contains(search.ToLower())
                                 //p.GrandTotal.ToString().ToLower().Contains(search.ToLower())
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
        public ActionResult Edit(long id)
        {
            var userpermission = User.IsInRole("All Payment Entry");
            var UserId = User.Identity.GetUserId();
            long EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            ViewBag.EmployeeId = EmpId;

            DummyPayment pay = db.DummyPayments.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.PaymentId == id).FirstOrDefault();
            if (pay == null)
            {
                return NotFound();
            }

            ViewBag.PaymentId = id;

            //Fetching values from AttachmentDocuments
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.DummyPayments
                             on a.TransactionID equals b.PaymentId
                             where (a.TransactionID == id && a.TransactionType == "Payment2")
                             select new PaymentDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 FileName = a.FileName,
                                 Status = a.Status,
                                 CreatedDate = a.CreatedDate
                             }).ToList();

            //    ID = r.AccountsID,
            //    Name = r.Name

            var Paidfrom = db.Accountss.Where(p =>p.AccountsID==pay.PayFrom).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(Paidfrom, "ID", "Name");
            var userid = User.Identity.GetUserId();
            var eimpid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            var PaidTo = (from a in db.accountmaps
                          join b in db.Accountss on a.AccountId equals b.AccountsID
                          where a.PaymentTypeId==EmployeePaymentType.Cash && a.EmployeeId==eimpid
                           select new
                                {
                                    ID = a.AccountId,
                                    Name = b.Name
                                }).ToList();

            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            var InvoiceNo = db.DummyPayments.Where(a => a.InvoiceNo == pay.InvoiceNo).
                                Select(r => new
                                {
                                    ID = r.InvoiceNo,
                                    Name = r.InvoiceNo,
                                }).ToList();

            ViewBag.InvoiceNo = QkSelect.List(InvoiceNo, "ID", "Name");

            var Balance = com.Accbalance(pay.PayTo);
            ViewBag.acctype = Balance["acctype"];
            var pdcDate = (pay.PDCDate != null) ? ((DateTime)pay.PDCDate).ToString("dd-MM-yyyy") : "";

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            //enable project
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

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
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var payment = new PaymentViewModel
            {
                VoucherNo = pay.VoucherNo,
                Date = (pay.Date).ToString("dd-MM-yyyy"),
                InvoiceNo = pay.InvoiceNo,
                PayTo = pay.PayTo,
                PayFrom = pay.PayFrom,
                MOPayment = pay.MOPayment,
                PDCDate = pay.PDCDate != null ? (pay.PDCDate).Value.ToString("dd-MM-yyyy") : pay.PDCDate.ToString(),
                Remark = pay.Remark,
                Tax = pay.Tax,
                TaxAmount = pay.TaxAmount,
                TaxPer = pay.TaxPer,
                GrandTotal = pay.GrandTotal + pay.Discount,
                SubTotal = pay.SubTotal,
                Paying = pay.Paying,
                Branch = pay.Branch,
                Balance = pay.Balance,
                CheckNo = db.PDCs.Where(p => (p.Reference == pay.PaymentId && p.PDCType == "Payment")).Select(p => p.CheckNo).FirstOrDefault(),
                Bank = db.PDCs.Where(p => (p.Reference == pay.PaymentId && p.PDCType == "Payment")).Select(p => p.Bank).FirstOrDefault(),
                Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList(),
                Discount = pay.Discount,
                Project = pay.Project,
                ProTask = pay.ProTask,
                Ref1 = pay.Ref1,
                Ref2 = pay.Ref2,
                Ref3 = pay.Ref3,
                Ref4 = pay.Ref4,
                Ref5 = pay.Ref5,
                PaymentStatus = pay.PaymentStatus
            };
            ViewBag.fname = payment.Ref5;
            //companyinfo .Where(p => p.editable == 0)
            companySet();
            ViewBag.preEntry = db.Payments.Where(a => a.PaymentId < id && a.editable == choice.Yes && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.PaymentId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Payments.Where(a => a.PaymentId > id && a.editable == choice.Yes && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.PaymentId).DefaultIfEmpty().Min();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            _FinancialYear();
            var VoucherEdit = db.EnableSettings.Where(a => a.EnableType == "EnableVoucherEdit").FirstOrDefault();
            ViewBag.EditVoucher = VoucherEdit != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            //field mapping
            payment.FieldMap = db.FieldMappings.Where(a => a.Section == "Payment" && a.Status == Status.active).ToList();

            //chk pdcregulated
            var pdcchk = db.PDCs.Where(x => x.PDCType == "Payment" && x.PDCRegDate != null && x.Reference == id).FirstOrDefault();
            ViewBag.chkPdc = pdcchk != null ? pdcchk.RegStatus : choice.No;

            //dummy table operations
            var DBill = db.DummyPaymentBills.Where(a => a.Payment == id).FirstOrDefault();
            var PBill = db.PaymentBills.Where(a => a.Payment == id).FirstOrDefault();
            if (PBill == null && DBill != null)
            {
                var DBills = db.DummyPaymentBills.Where(a => a.Payment == id).ToList();
                foreach (var arr in DBills)
                {
                    //add to se-item table
                    PaymentBill bills = new PaymentBill();
                    bills.Payment = arr.Payment;
                    bills.InvoiceNo = arr.InvoiceNo;
                    bills.BillType = arr.BillType;
                    bills.Amount = arr.Amount;
                    bills.Type = arr.Type;
                    bills.NewRefName = arr.NewRefName;
                    bills.Status = arr.Status;
                    db.PaymentBills.Add(bills);
                    db.SaveChanges();
                }

                db.DummyPaymentBills.RemoveRange(db.DummyPaymentBills.Where(a => a.Payment == id));
                db.SaveChanges();
            }

            var MlPayment = db.EnableSettings.Where(a => a.EnableType == "Payment").FirstOrDefault();
            var MlPayments = MlPayment != null ? MlPayment.Status : Status.inactive;
            ViewBag.MlPayment = MlPayments;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Payment Approval2").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            //Payment Status
            if (payment.PaymentStatus == 0)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.PaymentStatus = pstat;

            }
            else if (payment.PaymentStatus == 1)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
              };
                ViewBag.PaymentStatus = pstat;
            }
            else
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {

                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                      };
                ViewBag.PaymentStatus = pstat;
            }

            var EditPermission = User.IsInRole("Disable payment approval edit after approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "Payment Approval2", UserId);

            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Edit", payment);
            }
            else
            {
                return View(payment);
            }
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Edit Payment")]
        public ActionResult Edit(long id, PaymentViewModel vmodel)
        {
            string msg;
            bool stat;

            var fmapp = db.FieldMappings.Where(a => a.Section == "Payment" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

            var Editable = db.DummyPayments.Any(a => a.editable == choice.No && a.PaymentId == id);
            if (Editable)
            {
                msg = "Sorry,This Payment Cannot be Editable.";
                stat = false;
            }
            else
            {
                //               where (b.PaymentId != id && a.CheckNo == vmodel.CheckNo && vmodel.PayFrom == b.PayFrom && a.PDCType == "Payment")


                if (1==2)
                {
                    msg = "Check Number Already Exist ! Try Another One..";
                    stat = false;
                }
                else
                {
                    var today = Convert.ToDateTime(System.DateTime.Now);
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
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var paydet = db.DummyPayments.Find(id);
                    DummyPayment Pay = paydet;
                    DummyPayment Paytemp = paydet;
                    DateTime? pdcDate = null;

                    if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                    {
                        choice regstatus = db.PDCs.Where(x => x.Reference == id && x.PDCType == "Payment").Select(x => x.RegStatus).SingleOrDefault();
                        if (regstatus == choice.Yes)
                            pdcDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));

                    }
                    var vdate = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));

                    var Bills = "";
                    var retval = 0;
                    long[] bill = null;

                    if (vmodel.invoicedata != null)
                    {
                        if (vmodel.invoicedata.Where(a => a.Type == "Against Reference").ToList() != null)
                        {
                            bill = vmodel.invoicedata.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                            Bills = String.Join(";", bill.Select(p => p.ToString()).ToArray());
                        }

                        var RecBill = db.DummyPayBills.Where(a => a.Payment == id).FirstOrDefault();

                        if (RecBill != null)
                        {
                            var PBill = db.DummyPayBills.Where(a => a.Payment == id).ToList();
                            foreach (var arr in PBill)
                            {
                                //add to dummy table
                                DummyPaymentBill bills = new DummyPaymentBill();
                                bills.Payment = arr.Payment;
                                bills.InvoiceNo = arr.InvoiceNo;
                                bills.BillType = arr.BillType;
                                bills.Amount = arr.Amount;
                                bills.Type = arr.Type;
                                bills.NewRefName = arr.NewRefName;
                                bills.Status = arr.Status;
                                db.DummyPaymentBills.Add(bills);
                                db.SaveChanges();
                            }

                            db.DummyPayBills.RemoveRange(db.DummyPayBills.Where(a => a.Payment == id));
                            db.SaveChanges();
                        }

                        DummyPayBill paybill = new DummyPayBill();
                        foreach (var arr in vmodel.invoicedata)
                        {
                            paybill.Payment = id;
                            paybill.InvoiceNo = arr.InvoiceNo;
                            paybill.BillType = arr.BillType;
                            paybill.Amount = arr.Amount;
                            paybill.Type = arr.Type; //arr.Type;
                            paybill.NewRefName = arr.NewRefName;
                            paybill.Status = arr.Status;

                            db.DummyPayBills.Add(paybill);
                            retval = db.SaveChanges();
                        };
                    }
                    if (retval > 0)
                    {
                        db.DummyPaymentBills.RemoveRange(db.DummyPaymentBills.Where(a => a.Payment == id));
                        db.SaveChanges();
                    }

                    bool? accstat = null;
                    bool? stats = null;
                    string checkno = (vmodel.CheckNo != null) ? vmodel.CheckNo : "";
                    string bank = (vmodel.Bank != null) ? vmodel.Bank : "";
                    string pdcNote = (vmodel.pdcNote != null) ? vmodel.pdcNote : "";

                    var MlPayment = db.EnableSettings.Where(a => a.EnableType == "Payment").FirstOrDefault();
                    var MlPayments = MlPayment != null ? MlPayment.Status : Status.inactive;

                    //Save Receipt only if not override(if override saving is done in last section)
                        Pay.Date = vdate;
                        Pay.InvoiceNo = vmodel.InvoiceNo;
                        Pay.PayTo = vmodel.PayTo;
                        Pay.PayFrom = vmodel.PayFrom;
                        Pay.Tax = vmodel.Tax;
                        Pay.TaxAmount = vmodel.TaxAmount;
                        Pay.TaxPer = vmodel.TaxPer;
                        Pay.Remark = vmodel.Remark;
                        Pay.Branch = Branch;
                        Pay.Discount = vmodel.Discount;
                        if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                        {
                            Pay.PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                        }
                        else
                        {
                            Pay.PDCDate = null;
                        }
                        Pay.MOPayment = vmodel.MOPayment;
                        Pay.Balance = 0;
                        Pay.SubTotal = vmodel.SubTotal;

                        var Balance1 = com.Accbalance(vmodel.PayTo);
                        if (Balance1["acctype"] == (object)"Expense")
                        {
                            Pay.GrandTotal = vmodel.GrandTotal;
                            Pay.Paying = vmodel.GrandTotal;
                        }
                        else
                        {
                            Pay.GrandTotal = vmodel.Paying;
                            Pay.Paying = vmodel.Paying;
                        }

                        Pay.Project = vmodel.Project;
                        Pay.ProTask = vmodel.ProTask;

                        Pay.Ref1 = vmodel.Ref1;
                        Pay.Ref2 = vmodel.Ref2;
                        Pay.Ref3 = vmodel.Ref3;
                        Pay.Ref4 = vmodel.Ref4;
                        Pay.Ref5 = vmodel.Ref5;

                        Pay.PaymentStatus = vmodel.PaymentStatus;
                        Pay.OverrideStatus = vmodel.Override;
                        Pay.CheckNo = checkno;
                        Pay.Bank = bank;
                        Pay.PDCNote = pdcNote;
                        db.Entry(Pay).State = EntityState.Modified;
                        db.SaveChanges();

                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == Pay.PaymentId && a.Type == "Payment Approval2").FirstOrDefault();
                    var MrnPO = db.Approvals.Where(a => a.TransEntry == Pay.PaymentId && a.Type == "Payment Approval2").FirstOrDefault();
                    if (MrnPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == Pay.PaymentId && a.Type == "Payment Approval2"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == Pay.PaymentId && a.Type == "Payment Approval2"));
                            db.SaveChanges();
                        }
                    }

                    //Approved By
                    var Appby = Convert.ToString(vmodel.ApprovedBy);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = Pay.PaymentId;
                            approval.Type = "Payment Approval2";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    if (vmodel.submittype == "print")
                    {
                        vmodel.MOPay = Enum.GetName(typeof(ModeOfPayment), vmodel.MOPayment);
                        vmodel.creditor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
                        vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
                        vmodel.GrandTotal = (vmodel.GrandTotal + vmodel.TaxAmount) - (vmodel.Discount);

                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        vmodel.ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    }
                    var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    vmodel.BusinessType = BusinessType;
                  

                   

                }
               
            }


            msg = "Successfully Created Payment details.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, message = msg, type = vmodel.submittype, fmapp = fmapp } };
        }
        public ActionResult GetPaymentBill(long entry)
        {
            var data = (from b in db.DummyPayBills
                        join c in db.DummyPayments on b.Payment equals c.PaymentId
                        join d in db.PurchaseEntrys on b.InvoiceNo equals d.PurchaseEntryId into purs
                        from d in purs.DefaultIfEmpty()
                        join e in db.PEPayments on d.PurchaseEntryId equals e.PurchaseEntry into pay
                        from e in pay.DefaultIfEmpty()
                        join f in db.SalesReturns on b.InvoiceNo equals f.SalesReturnId into sret
                        from f in sret.DefaultIfEmpty()
                        join g in db.SRPayments on f.SalesReturnId equals g.SalesReturnId into spay
                        from g in spay.DefaultIfEmpty()
                        where c.PaymentId == entry //&& (b.InvoiceNo == d.SalesEntryId || b.InvoiceNo == f.PurchaseReturnId)
                        select new
                        {
                            b.Amount,
                            id = b.InvoiceNo,
                            type = b.BillType,
                            b.Payment,
                            b.Type,
                            b.BillType,
                            b.NewRefName,

                            PEDate = d != null ? (DateTime?)d.PEDate : null,
                            SRDate = f != null ? (DateTime?)f.SRDate : null,

                            PEBill = d.BillNo,
                            SRBill = f.BillNo,

                            PEGrandTotal = d != null ? d.PEGrandTotal : 0,
                            PEPaidAmount = e != null ? e.PEPaidAmount : 0,

                            SRGrandTotal = f != null ? f.SRGrandTotal : 0,
                            SReturnAmount = g != null ? g.SReturnAmount : 0,

                        }).AsEnumerable().Select(o => new
                        {
                            o.Amount,
                            o.id,
                            type = o.type == "Purchase" ? "Purchase" : "Sales Return",
                            o.Payment,
                            o.Type,
                            o.BillType,
                            o.NewRefName,
                            Date = o.BillType == "Purchase" ? o.PEDate : o.SRDate,
                            BillNo = o.BillType == "Purchase" ? o.PEBill : o.SRBill,
                            Balance = o.BillType == "Purchase" ? (o.PEGrandTotal - o.PEPaidAmount) : (o.SRGrandTotal - o.SReturnAmount),
                        }).ToList();

            return LegacyJson(data);
        }
        [HttpGet]
        public JsonResult GetApprovalby(long pt)
        {
            var ConD = (from a in db.accountmaps
                        join b in db.Employees on a.EmployeeId equals b.EmployeeId

                        where (a.AccountId == pt)
                        select new
                        {
                            ID = b.EmployeeId,
                            Name = b.FirstName + " " + b.LastName
                        }).ToList();

            //             where (a.MCId == mc)
            //                 ID = b.EmployeeId,
            //                 Name = b.FirstName + " " + b.LastName

            return Json(ConD);
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "Payment" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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
            Int64 PaymentId = 0;
            var Dpayy = db.DummyPayments.Where(a => a.PaymentId == id).FirstOrDefault();
            var payto = Dpayy.PayTo;
           var grp= db.Accountss.Where(o => o.AccountsID == payto).Select(o => o.Group).FirstOrDefault();
            if(grp!=9)
            {
                stat = false;
                msg = "Edit And Select Cash Account";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
            if (App.ApprovalStatus == ApprovalStatus.Approved )
            {
                var Dpay = db.DummyPayments.Where(a => a.PaymentId == id).FirstOrDefault();
                
                var Pbill = db.DummyPayBills.Where(a => a.Payment == id).ToList();
                var pdcs = db.PDCs.Where(a => a.Reference == id).FirstOrDefault();
                //Payment pay = new Payment
                //    Voucher = Dpay.Voucher,
                //    VoucherNo = Dpay.VoucherNo,
                //    InvoiceNo = Dpay.InvoiceNo,
                //    Date = Dpay.Date,
                //    MOPayment = Dpay.MOPayment,
                //    PayFrom = Dpay.PayFrom,
                //    PayTo = Dpay.PayTo,
                //    Tax = Dpay.Tax,
                //    TaxPer = Dpay.TaxPer,
                //    TaxAmount = Dpay.TaxAmount,
                //    Remark = Dpay.Remark,
                //    PDCDate = Dpay.PDCDate,
                //    Balance = 0,
                //    SubTotal = Dpay.SubTotal,
                //    GrandTotal = Dpay.GrandTotal,
                //    Paying = Dpay.Paying,
                //    Status = Status.active,
                //    CreatedBy = Dpay.CreatedBy,
                //    CreatedDate = Dpay.CreatedDate,
                //    Branch = Dpay.Branch,
                //    editable = Dpay.editable,
                //    Reference = Dpay.Reference,
                //    RefType = Dpay.RefType,
                //    Discount = Dpay.Discount,
                //    Project = Dpay.Project,
                //    ProTask = Dpay.ProTask,
                //    Ref1 = Dpay.Ref1,
                //    Ref2 = Dpay.Ref2,
                //    Ref3 = Dpay.Ref3,
                //    Ref4 = Dpay.Ref4,
                //    Ref5 = Dpay.Ref5,
                //    PaymentStatus = Dpay.PaymentStatus,
                //    OverrideStatus = Dpay.OverrideStatus
                decimal SubTotal;
                decimal Paying;
                decimal GrandTotal;
                decimal TaxAmount = Convert.ToDecimal(Dpay.TaxAmount);
                decimal PayTotal = Convert.ToDecimal(Dpay.Paying);
                decimal Discount = Convert.ToDecimal(Dpay.Discount);
                decimal PaytoAmount = PayTotal - TaxAmount + Discount;
                string checkno = (Dpay.CheckNo != null) ? Dpay.CheckNo : "";
                string bank = (Dpay.Bank != null) ? Dpay.Bank : "";
                string pdcNote = (Dpay.PDCNote != null) ? Dpay.PDCNote : "";
                var Balance = com.Accbalance(Dpay.PayTo);
                if (Balance["acctype"] == (object)"Expense")
                {
                    SubTotal = Dpay.SubTotal;
                    GrandTotal = Dpay.GrandTotal;
                    Paying = Dpay.GrandTotal;
                }
                else
                {
                    SubTotal = Dpay.SubTotal;
                    GrandTotal = Dpay.Paying;
                    Paying = Dpay.Paying;
                }
                var BranchID = db.Users.Where(a => a.Id == Dpay.CreatedBy).Select(a => a.BranchID).FirstOrDefault();
                PaymentId = com.addPayment3(Dpay.Date, Dpay.PayFrom, Dpay.PayTo, Dpay.SubTotal, Dpay.Paying, Dpay.GrandTotal, Dpay.Remark, Dpay.CreatedBy, Dpay.Branch, Convert.ToInt32(0), "Direct Payment", Dpay.TaxPer, Dpay.TaxAmount, Dpay.MOPayment, Dpay.Tax, Dpay.PDCDate, Dpay.editable, checkno, bank, pdcNote, Dpay.VoucherNo, Pbill, Dpay.Discount, Dpay.Project, Dpay.ProTask, Dpay.Ref1, Dpay.Ref2, Dpay.Ref3, Dpay.Ref4, Dpay.Ref5, Dpay.InvoiceNo, Dpay.PaymentStatus, Dpay.OverrideStatus);
                string acctype = Convert.ToString(Balance["acctype"]);
                Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();
                bool? Astatus = true;
                if (Dpay.MOPayment != ModeOfPayment.PDC)
                {
                    var billCheck = com.BillClearPayment2(Dpay.PayTo, PaytoAmount, PaymentId, Dpay.Date, BranchID, Dpay.CreatedBy, acctype, null, Pbill);
                    Astatus = null;
                }
                com.addAccountTrasaction(0, Dpay.Paying, Dpay.PayFrom, "Payment", PaymentId, DC.Credit, Dpay.Date, Astatus, null, Dpay.Project, Dpay.ProTask);
                com.addAccountTrasaction(PaytoAmount, 0, Dpay.PayTo, "Payment", PaymentId, DC.Debit, Dpay.Date, Astatus, null, Dpay.Project, Dpay.ProTask);
                if (TaxAmount > 0)
                    com.addAccountTrasaction(TaxAmount, 0, VATInput, "Expense Payment", PaymentId, DC.Debit, Dpay.Date, Astatus, null, Dpay.Project, Dpay.ProTask);
                if (Dpay.Discount > 0)
                    com.addAccountTrasaction(0, Dpay.Discount, 498, "Discount Received", PaymentId, DC.Credit, Dpay.Date, Astatus, null, Dpay.Project, Dpay.ProTask);
                // Add log
                com.addlog(LogTypes.Created, UserId, "Payment", "Payments", findip(), PaymentId, "Successfully added Payment details");
                DummyPayment pay = db.DummyPayments.Find(id);
                pay.stat = Status.inactive;
                db.Entry(pay).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (PaymentId != 0)
            {
                var empApprvl = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Payment Approval2").ToArray();

                Approval approval = new Approval();
                foreach (var emp in empApprvl)
                {
                    approval.TransEntry = PaymentId;
                    approval.Type = "Payment Approval";
                    approval.EmployeeId = emp.EmployeeId;
                    db.Approvals.Add(approval);
                    db.SaveChanges();
                }
                var MR = db.Payments.Where(a => a.PaymentId == PaymentId).FirstOrDefault();

                var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == PaymentId && a.Type == "Payment Approval").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

                if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
                {
                    ApprovalUpdate AppUp = new ApprovalUpdate();
                    AppUp.ApprovalStatus = App.ApprovalStatus;
                    AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    AppUp.ApprovedBy = UserId;
                    AppUp.Note = App.Note;
                    AppUp.RequestBy = MR.CreatedBy;
                    AppUp.Status = Status.active;
                    AppUp.TransEntry = PaymentId;
                    AppUp.Type = "Payment Approval";

                    db.ApprovalUpdates.Add(AppUp);
                    db.SaveChanges();
                    ApprovalUpdate AppUps = new ApprovalUpdate();
                    Payment vmodel = new Payment();
                }
            }
            var MR2 = db.DummyPayments.Where(a => a.PaymentId == id).FirstOrDefault();

            var chkappby2 = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "Payment Approval2").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if ((chkappby2 == null) || (chkappby2.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp2 = new ApprovalUpdate();
                AppUp2.ApprovalStatus = App.ApprovalStatus;
                AppUp2.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp2.ApprovedBy = UserId;
                AppUp2.Note = App.Note;
                AppUp2.RequestBy = MR2.CreatedBy;
                AppUp2.Status = Status.active;
                AppUp2.TransEntry = id;
                AppUp2.Type = "Payment Approval2";

                db.ApprovalUpdates.Add(AppUp2);
                db.SaveChanges();
                ApprovalUpdate AppUps = new ApprovalUpdate();
                Payment vmodel = new Payment();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                var Date = MR2.Date;
                decimal SubTotal;
                decimal Paying;
                decimal GrandTotal;
                decimal TaxAmount = MR2.TaxAmount;
                decimal PayTotal = Convert.ToDecimal(MR2.Paying);
                decimal Discount = Convert.ToDecimal(MR2.Discount);
                decimal PaytoAmount = PayTotal - TaxAmount + Discount;
                var RE = db.ApprovalUpdates.Where(a => a.TransEntry == id).FirstOrDefault();


                if (RE.ApprovalStatus == 0)
                {
                    bool? Astatus = true;
                    if (MR2.MOPayment != ModeOfPayment.PDC)
                    {
                        Astatus = null;
                    }
                    // if payment done update to transaction
                    //// Add log

                }

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
        [RedirectingAction]
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View Payment")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var MOP = db.DummyPayments.Where(x => x.PaymentId == id).Select(y => y.MOPayment).FirstOrDefault();
            ViewBag.MOPayment = (MOP == ModeOfPayment.Cash) ? 0 : 1;
            PaymentViewModel vmodel = new PaymentViewModel();
            vmodel = (from b in db.DummyPayments
                      join c in db.DummyPayBills on b.PaymentId equals c.Payment into pay
                      from c in pay.DefaultIfEmpty()
                      join d in db.Accountss on b.PayFrom equals d.AccountsID into payfrom
                      from d in payfrom.DefaultIfEmpty()
                      join e in db.Accountss on b.PayTo equals e.AccountsID into payto
                      from e in payto.DefaultIfEmpty()
                      join f in db.PDCs on b.PaymentId equals f.Reference into pdc
                      from f in pdc.DefaultIfEmpty()
                      where b.PaymentId == id
                      select new PaymentViewModel
                      {
                          VoucherNo = b.VoucherNo,
                          payfromname = d.Name,
                          paytoname = e.Name,
                          MOPayment = b.MOPayment,
                          PaymentDate = b.Date,
                          pdcdat = b.PDCDate,
                          CheckNo = f.CheckNo,
                          Discount = b.Discount,
                          GrandTotal = b.GrandTotal,
                          TaxAmount = b.TaxAmount,
                          Totamt = b.Balance,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                      }).FirstOrDefault();

            var paymentitems = db.DummyPayBills.Where(a => a.Payment == id)
            .Select(b => new PaymentBillViewModel
            {
                NewRefName = b.NewRefName,
                InvoiceNo = b.InvoiceNo,
                Amount = b.Amount,
            }).ToList();

            vmodel.PayItem = paymentitems;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Payment" && a.Status == Status.active).ToList();

            return View(vmodel);
        }
        public ActionResult UploadFiles()
        {
            var TransactionId = Request.Form.GetValues("id").First();
            long PaymentID = 0;

            if (TransactionId.Contains("undefined"))
            {
                var LastID = db.DummyPayments.OrderByDescending(a => a.PaymentId).FirstOrDefault();
                PaymentID = LastID.PaymentId;
            }
            else
            {
                PaymentID = Convert.ToInt32(TransactionId);
            }

            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/PaymentDocuments2/");

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/PaymentDocuments2/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/PaymentDocuments2/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";
                                }

                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/PaymentDocuments2/"), newName);
                                file.SaveAs(newName);

                                var PaymntDocument = new AttachmentDocuments
                                {
                                    TransactionID = PaymentID,
                                    TransactionType = "Payment2",
                                    FileName = newFName,
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.AttachmentDocuments.Add(PaymntDocument);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/PaymentDocuments2/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/PaymentDocuments2/"), resizeName);
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

    }
}
