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
    public class ReceiptController : BaseController
    {

        ApplicationDbContext db;
        Common com;
        public ReceiptController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Receipt
        [QkAuthorize(Roles = "Dev,Receipt List")]
        public ActionResult Index()
        {
            var Paidto = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PayTo = QkSelect.List(Paidto, "ID", "Name");

            ViewBag.PayFrom = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);

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
        // create 
        [QkAuthorize(Roles = "Dev,Create Receipt")]
        public ActionResult Create(long? id, string type)
        {
            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            ViewBag.Paidfrom = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1); ;

            var receipt = new ReceiptViewModel
            {
                VoucherNo = com.recVoucherNo(),
                Date = (System.DateTime.Now).ToString("dd-MM-yyyy")
            };
            //enable bill to bill receipt
            var ToReceipt = db.EnableSettings.Where(a => a.EnableType == "BillToBillReceipt").FirstOrDefault();
            var BillTo = ToReceipt != null ? (ToReceipt.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToReceipt = BillTo;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            //enable project
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

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            //companyinfo
            companySet();
            var userpermission = User.IsInRole("All Receipt Entry");
            var UserId = User.Identity.GetUserId();
            ViewBag.LastEntry = db.Receipts.Where(p => (p.editable == choice.Yes) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.ReceiptId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            _FinancialYear();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            //field mapping
            receipt.FieldMap = db.FieldMappings.Where(a => a.Section == "Receipt" && a.Status == Status.active).ToList();
            if(id!=null && type=="ReExtend")
            {
                Receipt rpt = db.Receipts.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.ReceiptId == id).FirstOrDefault();


                //    ID = r.AccountsID,
                //    Name = r.Name
                ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");
                 receipt = new ReceiptViewModel
                {
                     VoucherNo = com.recVoucherNo(),
                     Date = (System.DateTime.Now).ToString("dd-MM-yyyy"),
                    PayFrom = rpt.PayFrom,
                    PayTo = rpt.PayTo,
                    MOPayment = rpt.MOPayment,
                    PDCDate = rpt.PDCDate != null ? (rpt.PDCDate).Value.ToString("dd-MM-yyyy") : rpt.PDCDate.ToString(),
               //     Remark = rpt.Remark,
                   // GrandTotal = rpt.GrandTotal,
                  //  Balance = rpt.Balance,
                    Paying = rpt.Paying,
                    Branch = rpt.Branch,
                   // Discount = rpt.Discount,
                    Project = rpt.Project,
                    ProTask = rpt.ProTask,
                    Ref1 = rpt.Ref1,
                    Ref2 = rpt.Ref2,
                    Ref3 = rpt.Ref3,
                    Ref4 = rpt.Ref4,
                    Ref5 = rpt.Ref5,
                };
                receipt.FieldMap = db.FieldMappings.Where(a => a.Section == "Receipt" && a.Status == Status.active).ToList();

                var PaidFr = db.Accountss.Where(a => a.AccountsID == receipt.PayFrom).
                Select(r => new
                {
                    ID = r.AccountsID,
                    Name = r.Name
                }).ToList();
                ViewBag.Paidfrom = QkSelect.List(PaidFr, "ID", "Name");
                return View(receipt);

            }
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", receipt);
            }
            else
            {
                return View(receipt);
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Receipt")]
        public JsonResult Create(ReceiptViewModel vmodel)
        {
            string msg;
            bool stat;

            long chequetransid = 0;
            if (vmodel.leafno != null)
            {

                var bk = db.ChequeBooks.Any(a => a.booktype == Docbooktype.reciept && a.numberstarting <= vmodel.leafno && a.endnumbering >= vmodel.leafno);
                if (bk)
                {
                    var exist = db.chequetransactions.Any(o => o.docserialno == vmodel.leafno && o.transtype == Docbooktype.reciept);
                    if (!exist)
                    {
                        var book = db.ChequeBooks.Where(a => a.booktype == Docbooktype.reciept && a.numberstarting <= vmodel.leafno && a.endnumbering >= vmodel.leafno).FirstOrDefault();
                        chequetransaction nch = new chequetransaction
                        {
                            bookid = book.bookid,
                            remarks = "",
                            transdate = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB")),
                            transtype = Docbooktype.reciept,
                            purpose="Reciept",
                            docserialno = (long)vmodel.leafno,
                        };
                        db.chequetransactions.Add(nch);
                        db.SaveChanges();
                        chequetransid = nch.chequetransid;
                        db.ChequeBooks.Where(o => o.bookid == book.bookid).ToList().ForEach(o => o.usedleaf = o.usedleaf + 1);
                        db.SaveChanges();

                    }
                else
                    {
                        msg = "Reciept Leaf Already Used";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                    }
                }
                else
                {
                    msg = "Reciept Leaf Not Found";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }
            }
            var AutoBillVoucher = db.EnableSettings.Where(a => a.EnableType == "AutomaticVoucherNo").FirstOrDefault();
                var AutoBillVouchers = AutoBillVoucher != null ? AutoBillVoucher.Status : Status.inactive;

                if (!com.recBillExist(Convert.ToString(vmodel.VoucherNo)) || AutoBillVouchers == Status.active)
                {

                    var RepeatChequeNo = db.EnableSettings.Where(a => a.EnableType == "RepeatChequeNo").FirstOrDefault();
                var RepeatChequeNos = RepeatChequeNo != null ? RepeatChequeNo.Status : Status.inactive;

                var pdCheck = (from a in db.PDCs
                               join b in db.Receipts on a.Reference equals b.ReceiptId
                               where (a.CheckNo == vmodel.CheckNo && vmodel.PayFrom == b.PayFrom && a.PDCType == "Receipt")
                               select b
                                  ).Any();
                //pdCheck == true && RepeatChequeNos == Status.inactive
                if (1==2)
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
                        //same datepicker in CDC
                        pdcDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                    }
                    decimal PayTotal = Convert.ToDecimal(vmodel.Paying);
                    decimal Discount = Convert.ToDecimal(vmodel.Discount);
                    decimal PaytoAmount = PayTotal + Discount;
                    var Balance = com.Accbalance(vmodel.PayFrom);
                    vmodel.VoucherNo = (AutoBillVouchers == Status.active) ? com.recVoucherNo() : Convert.ToString(vmodel.VoucherNo);

                    Int64 ReceiptId = com.addReceipt(Date, vmodel.PayFrom, vmodel.PayTo, vmodel.Paying, vmodel.GrandTotal, vmodel.Remark, UserId, Branch, 0, "Direct Receipt", vmodel.MOPayment, pdcDate, choice.Yes, vmodel.CheckNo, vmodel.Bank, vmodel.pdcNote, vmodel.VoucherNo, vmodel.invoicedata, vmodel.Discount, vmodel.Project, vmodel.ProTask, vmodel.Ref1, vmodel.Ref2, vmodel.Ref3, vmodel.Ref4, vmodel.Ref5);

                    bool? Astatus = true;
                    if (vmodel.MOPayment != ModeOfPayment.PDC)
                    {
                        var billCheck = com.BillClearReciept(vmodel.PayFrom, PaytoAmount, ReceiptId, Date, BranchID, UserId, null, vmodel.invoicedata);
                        Astatus = null;
                        // if payment done update to transaction  \\creit      //debit   
                    }

                    com.addAccountTrasaction(Convert.ToDecimal(vmodel.Paying), 0, vmodel.PayTo, "Receipt", ReceiptId, DC.Debit, Date, Astatus, null, vmodel.Project, vmodel.ProTask);
                    com.addAccountTrasaction(0, PaytoAmount, vmodel.PayFrom, "Receipt", ReceiptId, DC.Credit, Date, Astatus, null, vmodel.Project, vmodel.ProTask);
                    com.clearsepayment();
                    if (vmodel.Discount > 0)                            //debit
                        com.addAccountTrasaction(Convert.ToDecimal(vmodel.Discount), 0, 497, "Discount Allowed", ReceiptId, DC.Debit, Date, Astatus, null, vmodel.Project, vmodel.ProTask);

                    com.addlog(LogTypes.Created, UserId, "Receipt", "Receipts", findip(), ReceiptId, "Successfully added Receipt details");
       
                            if (vmodel.submittype == "print")
                    {
                        vmodel.MOPay = Enum.GetName(typeof(ModeOfPayment), vmodel.MOPayment);
                        vmodel.creditor = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
                        vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        vmodel.ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    }
                    var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var Oustanding = (from a in db.SalesEntrys
                                      join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                      join e in db.PaymentMethods on a.PaymentMethod equals e.PaymentMethodId into paymeth
                                      from e in paymeth.DefaultIfEmpty()
                                      join f in db.Customers on vmodel.creditor equals f.CustomerName into cust
                                      from f in cust.DefaultIfEmpty()
                                      where
                                      (a.Customer == f.CustomerID)
                                      && ((((decimal?)c.SEBillAmount ?? 0) - ((decimal?)c.SEPaidAmount)) > 0)
                                      select new
                                      {
                                          Date = a.SEDate,
                                          VoucherNo = a.BillNo,
                                          Balance = ((decimal?)c.SEBillAmount ?? 0) - ((decimal?)c.SEPaidAmount ?? 0)
                                      }).ToList();
                    if (BusinessType == "Scaffold")
                    {
                        if (Oustanding.Count != 0)
                        {
                            vmodel.Totamt = Oustanding.Select(x => x.Balance).Sum();
                        }
                        vmodel.User = db.Users.Where(x => x.Id == UserId).Select(y => y.UserName).FirstOrDefault();
                        var v = (from a in db.Customers
                                 join x in db.Accountss on a.Accounts equals x.AccountsID
                                 join b in db.Contacts on a.Contact equals b.ContactID into tmp
                                 from b in tmp.DefaultIfEmpty()
                                 where (vmodel.creditor == a.CustomerName)
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
                    }



                    //send mail to company address
                    var sendmail = db.EnableSettings.Where(a => a.EnableType == "AutomaticMailInTransactions").FirstOrDefault();
                    var sendcmail = sendmail != null ? sendmail.Status : Status.inactive;
                    if (sendcmail == Status.active)
                    {
                        var payfrom = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
                        var payto = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();

                        CompanyEmailFormat CEmail = new CompanyEmailFormat();
                        CEmail.BillNo = "Receipt-" + vmodel.VoucherNo;
                        CEmail.Subject = "<table style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;' colspan='2' align='center'><b>Receipt Created</b></td><tr/> " +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Date               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + vmodel.Date + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Mode Of Payment    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + Enum.GetName(typeof(SaleType), vmodel.MOPayment) + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Pay From           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + payfrom + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Pay To             :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + payto + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Amount             :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + vmodel.Paying + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Discount           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + vmodel.Discount + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><b> Grand Total    :</b></td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><b> " + vmodel.GrandTotal + "</b></td><tr/></table>";

                        com.SendToCompanyMail(CEmail);
                    }

                    var fmapp = db.FieldMappings.Where(a => a.Section == "Receipt" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
                    var chkk = db.chequetransactions.Find(chequetransid);
                    if(chkk!=null)
                    {
                        chkk.referenceno = ReceiptId;
                        db.Entry(chkk).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    msg = "Successfully Created Receipt details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { tbldata = Oustanding, status = stat, data = vmodel, type = vmodel.submittype, message = msg, fmapp = fmapp } };
                    //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
        }

        [QkAuthorize(Roles = "Dev,Receipt List")]
        public JsonResult GetReceipt(string InvoiceNo, string FromDate, string ToDate, long? type, long? PayFrom, long? PayTo, string user)
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

            }


            string search = Request.Form.GetValues("search[value]")[0];

            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var userpermission = User.IsInRole("All Receipt Entry");
            var UserId = User.Identity.GetUserId();

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Receipt");
            var uDelete = User.IsInRole("Delete Receipt");
            var uDownload = User.IsInRole("Download Receipt");


            // EF Core 10 cannot translate the GroupBy-latest `chkAppStatus` nested collection inline.
            // Materialize entity columns/scalars server-side, then build a client lookup keyed by
            // ReceiptId (TransType == "Receipt Approval") and compute chkAppStatus client-side below.
            var serverQuery = (from a in db.Receipts
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                     from c in payto.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     where a.editable == choice.Yes &&
                     ((InvoiceNo == null || InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     (type == null || a.MOPayment == Paymode) &&
                     (PayFrom == null || PayFrom == 0 || a.PayFrom == PayFrom) &&
                     (PayTo == null || PayTo == 0 || a.PayTo == PayTo)) && (user == null || user == "" || g.Id == user)
                     && (userpermission == true || a.CreatedBy == UserId)

                     select new
                     {
                         Image=0,
                         //Image = (from aa in db.AttachmentDocuments
                         //         on aa.TransactionID equals bb.ReceiptId
                         //         where (aa.TransactionID == a.ReceiptId && aa.TransactionType == "Receipt")
                         //             DocumentID = aa.DocumentID,
                         //             FileName = aa.FileName,
                         //             Status = aa.Status,
                         //             CreatedDate = aa.CreatedDate
            VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.ReceiptId,
                         a.Date,
                         a.MOPayment,
                         a.PDCDate,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.editable,
                         a.Discount,
                         a.CreatedDate,
                         a.OverrideStatus,

                     });

            // Performance (audit P2): server-side paging on the common path (no search, plain-column sort);
            // search/computed sorts fall back to the original materialize-all behaviour unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "CreatedDate","Date","Discount","editable","GrandTotal","Image","MOPayment","OverrideStatus","Payer","PayFrom","Paying","PayTo","PDCDate","ReceiptId","Reciever","SubTotal","VoucherNo" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0
                && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn)
                    ? serverQuery.OrderBy("ReceiptId asc")
                    : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side chkAppStatus lookup keyed by ReceiptId (TransType == "Receipt Approval"):
            // latest ApprovalUpdate status per ApprovedBy. Missing key -> empty list.
            var rcptIds = serverRows.Select(o => o.ReceiptId).ToList();
            var chkAppStatusLookup = db.ApprovalUpdates
                .Where(q => q.Type == "Receipt Approval" && rcptIds.Contains(q.TransEntry))
                .Select(q => new { q.TransEntry, q.ApprovalStatus, q.ApprovedBy, q.CreatedDate })
                .ToList()
                .GroupBy(q => q.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(x => x.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.ReceiptId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.Image,
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.ReceiptId,
                         o.Date,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         o.MOPayment,
                         o.PDCDate,
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
                         Discount = o.Discount,
                         o.CreatedDate,
                         ApprovalStatus = chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : chkAppStatus.Contains(ApprovalStatus.Approved) ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval,// (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 && o.chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.OverrideStatus,
                         };
                     });

            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p =>// p.ReceiptId.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.VoucherNo.ToString().ToLower().Equals(search.ToLower())
                                 //p.GrandTotal.ToString().ToLower().Contains(search.ToLower())
                                 );

            }

            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!fastPage) { recordsTotal = v.Count(); }
            var data = fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
     
        public ActionResult Editprint(long? id)
        {
            com.clearsepayment();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Receipt Entry"); // Security S12: was hardcoded `true` (the IsInRole result was discarded) — IDOR let any user open any receipt by id.
            var UserId = User.Identity.GetUserId();
            Receipt rpt = db.Receipts.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.ReceiptId == id).FirstOrDefault();

            if (rpt == null)
            {
                return NotFound();
            }

            ViewBag.ReceiptId = id;
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.Receipts
                             on a.TransactionID equals b.ReceiptId
                             where (a.TransactionID == id && a.TransactionType == "Receipt")
                             select new ReceiptDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 FileName = a.FileName,
                                 Status = a.Status,
                                 CreatedDate = a.CreatedDate
                             }).ToList();


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

            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");
            //enable project
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

            var receipt = new ReceiptViewModel
            {
                VoucherNo = rpt.VoucherNo,
                Date = (rpt.Date).ToString("dd-MM-yyyy"),
                PayFrom = rpt.PayFrom,
                PayTo = rpt.PayTo,
                MOPayment = rpt.MOPayment,
                PDCDate = rpt.PDCDate != null ? (rpt.PDCDate).Value.ToString("dd-MM-yyyy") : rpt.PDCDate.ToString(),
                Remark = rpt.Remark,
                GrandTotal = rpt.GrandTotal,
                Balance = rpt.Balance,
                Bank = db.PDCs.Where(p => (p.Reference == rpt.ReceiptId && p.PDCType == "Receipt")).Select(p => p.Bank).FirstOrDefault(),
                CheckNo = db.PDCs.Where(p => (p.Reference == rpt.ReceiptId && p.PDCType == "Receipt")).Select(p => p.CheckNo).FirstOrDefault(),
                Paying = rpt.Paying,
                Branch = rpt.Branch,
                Discount = rpt.Discount,
                Project = rpt.Project,
                ProTask = rpt.ProTask,
                Ref1 = rpt.Ref1,
                Ref2 = rpt.Ref2,
                Ref3 = rpt.Ref3,
                Ref4 = rpt.Ref4,
                Ref5 = rpt.Ref5,
            };

            var PaidFr = db.Accountss.Where(a => a.AccountsID == receipt.PayFrom).
            Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(PaidFr, "ID", "Name");

            //companyinfo
            companySet();
            ViewBag.preEntry = db.Receipts.Where(a => (a.ReceiptId < id && a.editable == choice.Yes) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ReceiptId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Receipts.Where(a => (a.ReceiptId > id && a.editable == choice.Yes) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ReceiptId).DefaultIfEmpty().Min();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            _FinancialYear();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var VoucherEdit = db.EnableSettings.Where(a => a.EnableType == "EnableVoucherEdit").FirstOrDefault();
            ViewBag.EditVoucher = VoucherEdit != null ? EnableBranch.Status : Status.inactive;

            //field mapping
            receipt.FieldMap = db.FieldMappings.Where(a => a.Section == "Receipt" && a.Status == Status.active).ToList();

            //chk pdcregulated
            var pdcchk = db.PDCs.Where(x => x.PDCType == "Receipt" && x.PDCRegDate != null && x.Reference == id).FirstOrDefault();
            ViewBag.chkPdc = pdcchk != null ? pdcchk.RegStatus : choice.No;

            //dummy table operations
            var DBill = db.DummyReceiptBills.Where(a => a.Receipt == id).FirstOrDefault();
            var RBill = db.ReceiptBills.Where(a => a.Receipt == id).FirstOrDefault();
            if (RBill == null && DBill != null)
            {
                var DBills = db.DummyReceiptBills.Where(a => a.Receipt == id).ToList();
                foreach (var arr in DBills)
                {
                    //add to se-item table
                    ReceiptBill bills = new ReceiptBill();
                    bills.Receipt = arr.Receipt;
                    bills.InvoiceNo = arr.InvoiceNo;
                    bills.BillType = arr.BillType;
                    bills.Amount = arr.Amount;
                    bills.Type = arr.Type;
                    bills.NewRefName = arr.NewRefName;
                    bills.Status = arr.Status;
                    db.ReceiptBills.Add(bills);
                    db.SaveChanges();
                }

                db.DummyReceiptBills.RemoveRange(db.DummyReceiptBills.Where(a => a.Receipt == id));
                db.SaveChanges();
            }

            var rtype = Request.Query["rtype"];
            var ex = db.chequetransactions.Where(o => o.referenceno == id && o.purpose == "Reciept").FirstOrDefault();
            if (ex != null)
            {
                receipt.leafno = ex.docserialno;
            }
            if (rtype == "APP")
            {
                return View("App/Edit", receipt);
            }
            else
            {
                return View(receipt);
            }
        }
        [HttpPost]
        public ActionResult Editprintpost(ReceiptViewModel vmodel)
        {
var fmapp = db.FieldMappings.Where(a => a.Section == "Receipt" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
            string msg="";
            bool stat=true;
            vmodel.MOPay = Enum.GetName(typeof(ModeOfPayment), vmodel.MOPayment);
            vmodel.creditor = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
            vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            vmodel.ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";

            var Oustanding = (from a in db.SalesEntrys
                              join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                              join e in db.PaymentMethods on a.PaymentMethod equals e.PaymentMethodId into paymeth
                              from e in paymeth.DefaultIfEmpty()
                              join f in db.Customers on vmodel.creditor equals f.CustomerName into cust
                              from f in cust.DefaultIfEmpty()
                              where
                              (a.Customer == f.CustomerID)
                              && ((((decimal?)c.SEBillAmount ?? 0) - ((decimal?)c.SEPaidAmount)) > 0)
                              select new
                              {
                                  Date = a.SEDate,
                                  VoucherNo = a.BillNo,
                                  Balance = ((decimal?)c.SEBillAmount ?? 0) - ((decimal?)c.SEPaidAmount ?? 0)
                              }).ToList();
            if (Oustanding.Count != 0)
            {
                vmodel.Totamt = (Oustanding.Select(x => x.Balance).Sum());
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { tbldata = Oustanding, status = stat, data = vmodel, message = msg, type = vmodel.submittype, fmapp = fmapp } };

        }
        [QkAuthorize(Roles = "Dev,Edit Receipt")]
        public ActionResult Edit(long? id)
        {
            com.clearsepayment();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Receipt Entry"); // Security S12: was hardcoded `true` (the IsInRole result was discarded) — IDOR let any user open any receipt by id.
            var UserId = User.Identity.GetUserId();
            Receipt rpt = db.Receipts.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.ReceiptId == id).FirstOrDefault();

            if (rpt == null)
            {
                return NotFound();
            }

            ViewBag.ReceiptId = id;
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.Receipts
                             on a.TransactionID equals b.ReceiptId
                             where (a.TransactionID == id && a.TransactionType == "Receipt")
                             select new ReceiptDocumentViewModel
                             {
                                 DocumentID     =   a.DocumentID,
                                 FileName       =   a.FileName,
                                 Status         =   a.Status,
                                 CreatedDate    =   a.CreatedDate
                             }).ToList();


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

            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");
            //enable project
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

            var receipt = new ReceiptViewModel
            {
                VoucherNo = rpt.VoucherNo,
                Date = (rpt.Date).ToString("dd-MM-yyyy"),
                PayFrom = rpt.PayFrom,
                PayTo = rpt.PayTo,
                MOPayment = rpt.MOPayment,
                PDCDate = rpt.PDCDate != null ? (rpt.PDCDate).Value.ToString("dd-MM-yyyy") : rpt.PDCDate.ToString(),
                Remark = rpt.Remark,
                GrandTotal = rpt.GrandTotal,
                Balance = rpt.Balance,
                Bank = db.PDCs.Where(p => (p.Reference == rpt.ReceiptId && p.PDCType == "Receipt")).Select(p => p.Bank).FirstOrDefault(),
                CheckNo = db.PDCs.Where(p => (p.Reference == rpt.ReceiptId && p.PDCType == "Receipt")).Select(p => p.CheckNo).FirstOrDefault(),
                Paying = rpt.Paying,
                Branch = rpt.Branch,
                Discount = rpt.Discount,
                Project = rpt.Project,
                ProTask = rpt.ProTask,
                Ref1 = rpt.Ref1,
                Ref2 = rpt.Ref2,
                Ref3 = rpt.Ref3,
                Ref4 = rpt.Ref4,
                Ref5 = rpt.Ref5,
            };

            var PaidFr = db.Accountss.Where(a => a.AccountsID == receipt.PayFrom).
            Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(PaidFr, "ID", "Name");

            //companyinfo
            companySet();
            ViewBag.preEntry = db.Receipts.Where(a => (a.ReceiptId < id && a.editable == choice.Yes) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ReceiptId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Receipts.Where(a => (a.ReceiptId > id && a.editable == choice.Yes) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ReceiptId).DefaultIfEmpty().Min();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            _FinancialYear();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var VoucherEdit = db.EnableSettings.Where(a => a.EnableType == "EnableVoucherEdit").FirstOrDefault();
            ViewBag.EditVoucher = VoucherEdit != null ? EnableBranch.Status : Status.inactive;

            //field mapping
            receipt.FieldMap = db.FieldMappings.Where(a => a.Section == "Receipt" && a.Status == Status.active).ToList();

            //chk pdcregulated
            var pdcchk = db.PDCs.Where(x => x.PDCType == "Receipt" && x.PDCRegDate != null && x.Reference == id).FirstOrDefault();
            ViewBag.chkPdc = pdcchk != null ? pdcchk.RegStatus : choice.No;

            //dummy table operations
            var DBill = db.DummyReceiptBills.Where(a => a.Receipt == id).FirstOrDefault();
            var RBill = db.ReceiptBills.Where(a => a.Receipt == id).FirstOrDefault();
            if (RBill == null && DBill != null)
            {
                var DBills = db.DummyReceiptBills.Where(a => a.Receipt == id).ToList();
                foreach (var arr in DBills)
                {
                    //add to se-item table
                    ReceiptBill bills = new ReceiptBill();
                    bills.Receipt = arr.Receipt;
                    bills.InvoiceNo = arr.InvoiceNo;
                    bills.BillType = arr.BillType;
                    bills.Amount = arr.Amount;
                    bills.Type = arr.Type;
                    bills.NewRefName = arr.NewRefName;
                    bills.Status = arr.Status;
                    db.ReceiptBills.Add(bills);
                    db.SaveChanges();
                }

                db.DummyReceiptBills.RemoveRange(db.DummyReceiptBills.Where(a => a.Receipt == id));
                db.SaveChanges();
            }

            var rtype = Request.Query["rtype"];
            var ex = db.chequetransactions.Where(o => o.referenceno == id && o.purpose == "Reciept").FirstOrDefault();
            if (ex != null)
            {
                receipt.leafno = ex.docserialno;
            }
            if (rtype == "APP")
            {
                return View("App/Edit", receipt);
            }
            else
            {
                return View(receipt);
            }
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Receipt")]
        public ActionResult Edit(ReceiptViewModel vmodel, long id)
        {
            string msg;
            bool stat;
            var Editable = db.Receipts.Any(a => a.editable == choice.No && a.ReceiptId == id);
            if (Editable)
            {
                msg = "Sorry,This Receipt Cannot be Editable.";
                stat = false;
            }
            else
            {

                var ex = db.chequetransactions.Where(o => o.referenceno == id && o.purpose=="Reciept").FirstOrDefault();
                if (ex != null)
                {
                    db.chequetransactions.Remove(ex);
                    db.SaveChanges();


                    db.ChequeBooks.Where(o => o.bookid == ex.bookid).ToList().ForEach(o => o.usedleaf = o.usedleaf - 1);
                    db.SaveChanges();
                }

                if (vmodel.leafno != null)
                {

                    var bk = db.ChequeBooks.Any(a => a.booktype == Docbooktype.reciept && a.numberstarting <= vmodel.leafno && a.endnumbering >= vmodel.leafno);
                    if (bk)
                    {
                        var exist = db.chequetransactions.Any(o => o.docserialno == vmodel.leafno && o.transtype == Docbooktype.reciept&&o.referenceno!=id);
                        if (!exist)
                        {

                            var book = db.ChequeBooks.Where(a => a.booktype == Docbooktype.reciept && a.numberstarting <= vmodel.leafno && a.endnumbering >= vmodel.leafno).FirstOrDefault();
                            chequetransaction nch = new chequetransaction
                            {
                                bookid = book.bookid,
                                remarks = "",
                                transdate = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB")),
                                transtype = Docbooktype.reciept,
                                docserialno = (long)vmodel.leafno,
                                purpose="Reciept",
                                referenceno=id
                            };
                            db.chequetransactions.Add(nch);
                            db.SaveChanges();
                            db.ChequeBooks.Where(o => o.bookid == book.bookid).ToList().ForEach(o => o.usedleaf = o.usedleaf + 1);
                            db.SaveChanges();

                        }
                        else
                        {
                            msg = "Reciept Leaf Already Used";
                            stat = false;
                            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                        }
                    }
                    else
                    {
                        msg = "Reciept Leaf Not Found";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                    }
                }

                var RepeatChequeNo = db.EnableSettings.Where(a => a.EnableType == "RepeatChequeNo").FirstOrDefault();
                var RepeatChequeNos = RepeatChequeNo != null ? RepeatChequeNo.Status : Status.inactive;

                var pdCheck = (from a in db.PDCs
                               join b in db.Receipts on a.Reference equals b.ReceiptId
                               where (b.ReceiptId != id && a.CheckNo == vmodel.CheckNo && vmodel.PayFrom == b.PayFrom && a.PDCType == "Receipt")
                               select b
                                  ).Any();
                if (pdCheck == true && RepeatChequeNos == Status.inactive)
                {
                    msg = "Check Number Already Exist ! Try Another One..";
                    stat = false;
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

                    Receipt rec = db.Receipts.Find(id);
                    if (com.islocked("Receipt", rec.Date))
                    {
                        msg = "This Entry Is Locked";

                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    Receipt RecTemp = rec;
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    DateTime? pdcDate = null;
                    var Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                      if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                    {
                        //same datepicker in CDC
                        pdcDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                    }
                    
                    
                    //------delete---------------------
                    var Customerid = (from a in db.Customers where a.Accounts == rec.PayFrom select new { a.CustomerID }).SingleOrDefault();
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

                                        SEPT.Recieptid = rec.ReceiptId;
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

                    // in case of supplier
                    var supplierid = (from a in db.Suppliers where a.Accounts == rec.PayFrom select new { a.SupplierID }).SingleOrDefault();
                    if (supplierid != null)
                    {
                        decimal Amtsum = 0;
                        var data = (from a in db.PRTransactions
                                    where a.SupplierId == supplierid.SupplierID && a.Recieptid == id
                                    orderby a.PRTransactionId
                                    select new
                                    {
                                        a.PurchaseReturnId,
                                        a.PRPayAmount
                                    }).ToList();
                        if (data.Count > 0)
                        {
                            foreach (var ditem in data)
                            {
                                var paying = ditem.PRPayAmount;
                                PRPayment SEP = db.PRPayments.Where(a => a.PurchaseReturnId == ditem.PurchaseReturnId).FirstOrDefault();
                                SEP.PReturnAmount = SEP.PReturnAmount - Convert.ToDecimal(paying);
                                db.Entry(SEP).State = EntityState.Modified;
                                db.SaveChanges();

                                Amtsum += ditem.PRPayAmount;
                            }
                            db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == id));
                            db.SaveChanges();
                            if (vmodel.GrandTotal > Amtsum)
                            {
                                decimal payAmt = vmodel.GrandTotal - Amtsum;
                                foreach (var ditem in data)
                                {
                                    if (payAmt > 0)
                                    {
                                        PRPayment SEP = db.PRPayments.Where(a => a.PurchaseReturnId == ditem.PurchaseReturnId).FirstOrDefault();
                                        //add to petransactions
                                        PRTransaction SEPT = new PRTransaction();
                                        SEPT.PurchaseReturnId = SEP.PurchaseReturnId;
                                        SEPT.SupplierId = Convert.ToInt64(SEP.SupplierId);
                                        SEPT.PRPayDate = Date;
                                        SEPT.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                        SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                        SEPT.CreatedUserId = UserId;
                                        SEPT.Status = 0;

                                        SEPT.Recieptid = rec.ReceiptId;
                                        // transaction 
                                        var balnceamount = SEP.PRBillAmount - SEP.PReturnAmount;
                                        if (balnceamount >= payAmt)
                                        {
                                            SEP.PReturnAmount = SEP.PReturnAmount + Convert.ToDecimal(payAmt);
                                            SEPT.PRPayAmount = Convert.ToDecimal(payAmt);
                                            payAmt = 0;
                                        }
                                        else
                                        {
                                            SEP.PReturnAmount = SEP.PReturnAmount + Convert.ToDecimal(balnceamount);
                                            SEPT.PRPayAmount = Convert.ToDecimal(balnceamount);
                                            payAmt -= balnceamount;
                                        }
                                        SEP.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                        if (SEP.PRBillAmount == SEP.PReturnAmount)
                                        {
                                            SEP.Status = 1;
                                        }
                                        db.Entry(SEP).State = EntityState.Modified;
                                        db.SaveChanges();
                                        db.PRTransactions.Add(SEPT);
                                        db.SaveChanges();
                                    }
                                }




                            }
                        }

                    }

                    var ReceiptId = rec.ReceiptId;
                    var recbillchk = com.RecBillAdjust(ReceiptId);

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

                        var RecBill = db.ReceiptBills.Where(a => a.Receipt == id).FirstOrDefault();
                        if (RecBill != null)
                        {
                            var RBill = db.ReceiptBills.Where(a => a.Receipt == id).ToList();
                            foreach (var arr in RBill)
                            {
                                //add to dummy table
                                DummyReceiptBill bills = new DummyReceiptBill();
                                bills.Receipt = arr.Receipt;
                                bills.InvoiceNo = arr.InvoiceNo;
                                bills.BillType = arr.BillType;
                                bills.Amount = arr.Amount;
                                bills.Type = arr.Type;
                                bills.NewRefName = arr.NewRefName;
                                bills.Status = arr.Status;
                                db.DummyReceiptBills.Add(bills);
                                db.SaveChanges();
                            }

                            db.ReceiptBills.RemoveRange(db.ReceiptBills.Where(a => a.Receipt == id));
                            db.SaveChanges();

                        }

                        ReceiptBill recbill = new ReceiptBill();
                        foreach (var arr in vmodel.invoicedata)
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

                            recbill.Receipt = id;
                            recbill.InvoiceNo = arr.InvoiceNo;
                            recbill.BillType = arr.BillType;
                            recbill.Amount = arr.Amount;
                            recbill.Type = arr.Type; //arr.Type;
                            recbill.NewRefName = arr.NewRefName;
                            recbill.Status = arr.Status;

                            db.ReceiptBills.Add(recbill);
                            retval = db.SaveChanges();
                        };
                    }
                    if (retval > 0)
                    {
                        db.DummyReceiptBills.RemoveRange(db.DummyReceiptBills.Where(a => a.Receipt == id));
                        db.SaveChanges();
                    }

                    //----------------------------------------------------------------------------


                    bool? stats = null;
                    decimal PaytoAmount = Convert.ToDecimal(vmodel.Paying) + Convert.ToDecimal(vmodel.Discount);
                    bool? accstat = null;
                    if (RecTemp.MOPayment == vmodel.MOPayment)
                    {
                        if (RecTemp.MOPayment == ModeOfPayment.PDC || RecTemp.MOPayment == ModeOfPayment.CDC)
                        {
                            accstat = true;
                            PDC pdc = db.PDCs.Where(a => (a.Reference == RecTemp.ReceiptId) && (a.PDCType == "Receipt")).FirstOrDefault();
                            pdc.PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                            pdc.Note = vmodel.pdcNote;
                            pdc.CheckNo = vmodel.CheckNo;
                            pdc.Bank = vmodel.Bank;
                            pdc.Type = (vmodel.MOPayment == ModeOfPayment.PDC) ? 0 : 1;
                            pdc.Bills = Bills;

                            db.Entry(pdc).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        //type no change status no change
                        var pdcaccdet = db.AccountsTransactions.Where(a => (a.Account == RecTemp.PayFrom) && (a.Purpose == "Receipt") && a.reference == ReceiptId).FirstOrDefault();
                        if (pdcaccdet != null)
                        {
                            var aid = pdcaccdet.Id;
                            if(pdcDate ==null)
                            com.UpdateAccountTrasaction(aid, 0, PaytoAmount, vmodel.PayFrom, "Receipt", ReceiptId, DC.Credit, Date, pdcaccdet.Status, vmodel.Project, vmodel.ProTask);
                            else
                            com.UpdateAccountTrasaction(aid, 0, PaytoAmount, vmodel.PayFrom, "Receipt", ReceiptId, DC.Credit, pdcDate, pdcaccdet.Status, vmodel.Project, vmodel.ProTask);

                        }
                        var pdcaccdet1 = db.AccountsTransactions.Where(a => (a.Account == RecTemp.PayTo) && (a.Purpose == "Receipt") && a.reference == ReceiptId).FirstOrDefault();
                        if (pdcaccdet1 != null)
                        {
                            var aid = pdcaccdet1.Id;
                            if (pdcDate == null)
                                com.UpdateAccountTrasaction(aid, Convert.ToDecimal(vmodel.Paying), 0, vmodel.PayTo, "Receipt", ReceiptId, DC.Debit, Date, pdcaccdet1.Status, vmodel.Project, vmodel.ProTask);
                            else
                                com.UpdateAccountTrasaction(aid, Convert.ToDecimal(vmodel.Paying), 0, vmodel.PayTo, "Receipt", ReceiptId, DC.Debit, pdcDate, pdcaccdet1.Status, vmodel.Project, vmodel.ProTask);

                        }
                        Discfunction(ReceiptId, vmodel);

                    }
                    else
                    {

                        if (RecTemp.MOPayment == ModeOfPayment.PDC || RecTemp.MOPayment == ModeOfPayment.CDC)
                        {
                            var pdcdel = db.PDCs.Where(a => (a.Reference == RecTemp.ReceiptId) && (a.PDCType == "Receipt")).FirstOrDefault();
                            db.PDCs.Remove(pdcdel);
                            if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                            {
                                PDC pd = new PDC
                                {
                                    PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB")),
                                    PDCType = "Receipt",
                                    Reference = RecTemp.ReceiptId,
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
                            //cash status null
                            // if payment done update to transaction

                        }
                        else
                        {
                            if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                            {
                                PDC pd = new PDC
                                {
                                    PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB")),
                                    PDCType = "Receipt",
                                    Reference = RecTemp.ReceiptId,
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
                        if (vmodel.MOPayment == ModeOfPayment.PDC)
                        {
                            stats = true;
                        }

                        var pdcaccdet = db.AccountsTransactions.Where(a => (a.Account == RecTemp.PayFrom) && (a.Purpose == "Receipt") && a.reference == ReceiptId).FirstOrDefault();
                        if (pdcaccdet != null)
                        {
                            var aid = pdcaccdet.Id;
                            if(pdcDate==null)
                            com.UpdateAccountTrasaction(aid, 0, PaytoAmount, vmodel.PayFrom, "Receipt", ReceiptId, DC.Credit, Date, stats, vmodel.Project, vmodel.ProTask);
                            else
                           com.UpdateAccountTrasaction(aid, 0, PaytoAmount, vmodel.PayFrom, "Receipt", ReceiptId, DC.Credit, pdcDate, stats, vmodel.Project, vmodel.ProTask);


                        }
                        var pdcaccdet1 = db.AccountsTransactions.Where(a => (a.Account == RecTemp.PayTo) && (a.Purpose == "Receipt") && a.reference == ReceiptId).FirstOrDefault();
                        if (pdcaccdet1 != null)
                        {
                            var aid = pdcaccdet1.Id;
                            if (pdcDate == null)
                                com.UpdateAccountTrasaction(aid, Convert.ToDecimal(vmodel.Paying), 0, vmodel.PayTo, "Receipt", ReceiptId, DC.Debit, Date, stats, vmodel.Project, vmodel.ProTask);
                            else
                                com.UpdateAccountTrasaction(aid, Convert.ToDecimal(vmodel.Paying), 0, vmodel.PayTo, "Receipt", ReceiptId, DC.Debit, pdcDate, stats, vmodel.Project, vmodel.ProTask);


                        }
                        Discfunction(ReceiptId, vmodel);

                    }


                    rec.Date = Date;
                    rec.PayTo = vmodel.PayTo;
                    rec.PayFrom = vmodel.PayFrom;
                    rec.Paying = vmodel.Paying;
                    rec.GrandTotal = vmodel.GrandTotal;
                    rec.Balance = vmodel.Balance;
                    rec.Remark = vmodel.Remark;
                    rec.Branch = vmodel.Branch;
                    rec.Project = vmodel.Project;
                    rec.ProTask = vmodel.ProTask;

                    rec.Ref1 = vmodel.Ref1;
                    rec.Ref2 = vmodel.Ref2;
                    rec.Ref3 = vmodel.Ref3;
                    rec.Ref4 = vmodel.Ref4;
                    rec.Ref5 = vmodel.Ref5;

                    if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                    {
                        rec.PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                    }
                    else
                    {
                        rec.PDCDate = null;
                    }
                    rec.MOPayment = vmodel.MOPayment;
                    rec.Discount = vmodel.Discount;
                    db.Entry(rec).State = EntityState.Modified;
                    db.SaveChanges();


                    if (stats == null && !(vmodel.MOPayment == ModeOfPayment.PDC && (RecTemp.MOPayment == ModeOfPayment.Cash || RecTemp.MOPayment == ModeOfPayment.CDC)))
                    {
                        var billCheck = com.BillClearReciept(vmodel.PayFrom, PaytoAmount, ReceiptId, Date, BranchID, UserId, null, vmodel.invoicedata);
                    }
                    com.clearsepayment();
                    if (vmodel.submittype == "print")
                    {
                        vmodel.MOPay = Enum.GetName(typeof(ModeOfPayment), vmodel.MOPayment);
                        vmodel.creditor = db.Accountss.Where(a => a.AccountsID == rec.PayFrom).Select(a => a.Name).FirstOrDefault();
                        vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        vmodel.ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    }
                    var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

                    if (BusinessType == "Scaffold")
                    {
                        vmodel.User = db.Users.Where(x => x.Id == UserId).Select(y => y.UserName).FirstOrDefault();
                        var v = (from a in db.Customers
                                 join x in db.Accountss on a.Accounts equals x.AccountsID
                                 join b in db.Contacts on a.Contact equals b.ContactID into tmp
                                 from b in tmp.DefaultIfEmpty()
                                 where (vmodel.creditor == a.CustomerName)
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
                    }



                    //send mail to company address
                    var sendmail = db.EnableSettings.Where(a => a.EnableType == "AutomaticMailInTransactions").FirstOrDefault();
                    var sendcmail = sendmail != null ? sendmail.Status : Status.inactive;
                    if (sendcmail == Status.active)
                    {
                        var payfrom = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
                        var payto = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();


                        CompanyEmailFormat CEmail = new CompanyEmailFormat();
                        CEmail.BillNo = "Receipt-" + vmodel.VoucherNo;
                        CEmail.Subject = "<table style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;' colspan='2' align='center'><b>Receipt Updated</b></td><tr/> " +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Date               :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + vmodel.Date + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Mode Of Payment    :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + Enum.GetName(typeof(SaleType), vmodel.MOPayment) + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Pay From           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + payfrom + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Pay To             :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + payto + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Amount             :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + vmodel.Paying + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'>Discount           :</td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'> " + vmodel.Discount + "</td><tr/>" +
                                "<tr><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><b> Grand Total    :</b></td><td style='border: 1px solid black;border-collapse: collapse;padding: 5px;'><b> " + vmodel.GrandTotal + "</b></td><tr/></table>";

                        com.SendToCompanyMail(CEmail);
                    }



                    com.addlog(LogTypes.Updated, UserId, "Receipt", "Receipts", findip(), rec.ReceiptId, "Receipt Updated Successfully");
                    msg = "Successfully Updated Receipt details.";
                    stat = true;
                    //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            var Oustanding = (from a in db.SalesEntrys
                              join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                              join e in db.PaymentMethods on a.PaymentMethod equals e.PaymentMethodId into paymeth
                              from e in paymeth.DefaultIfEmpty()
                              join f in db.Customers on vmodel.creditor equals f.CustomerName into cust
                              from f in cust.DefaultIfEmpty()
                              where
                              (a.Customer == f.CustomerID)
                              && ((((decimal?)c.SEBillAmount ?? 0) - ((decimal?)c.SEPaidAmount)) > 0)
                              select new
                              {
                                  Date = a.SEDate,
                                  VoucherNo = a.BillNo,
                                  Balance = ((decimal?)c.SEBillAmount ?? 0) - ((decimal?)c.SEPaidAmount ?? 0)
                              }).ToList();
            if (Oustanding.Count != 0)
            {
                vmodel.Totamt = (Oustanding.Select(x => x.Balance).Sum());
            }
            var fmapp = db.FieldMappings.Where(a => a.Section == "Receipt" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

            return new QuickSoft.Models.LegacyJsonResult { Data = new { tbldata = Oustanding, status = stat, data = vmodel, message = msg, type = vmodel.submittype, fmapp = fmapp } };
        }

        public Boolean Discfunction(long ReceiptId, ReceiptViewModel vmodel)
        {
            var Discaccdid = db.AccountsTransactions.Where(a => (a.Account == 497 || a.Account == 498) && (a.Purpose == "Discount Allowed") && a.reference == ReceiptId).FirstOrDefault();
            var Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
            bool? stat = null;
            if (vmodel.MOPayment == ModeOfPayment.PDC)
            {
                stat = true;
            }
            if (Discaccdid != null)
            {
                var aid = Discaccdid.Id;
                if (vmodel.Discount > 0)
                {
                    com.UpdateAccountTrasaction(aid, Convert.ToDecimal(vmodel.Discount), 0, 497, "Discount Allowed", ReceiptId, DC.Debit, Date, stat, vmodel.Project, vmodel.ProTask);
                }
                else
                {
                    db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(a => (a.Account == 497 || a.Account == 498) && a.Purpose == "Discount Allowed" && a.reference == ReceiptId));
                }
            }
            else if (vmodel.Discount > 0)
            {
                com.addAccountTrasaction(Convert.ToDecimal(vmodel.Discount), 0, 497, "Discount Allowed", ReceiptId, DC.Debit, Date, stat, null, vmodel.Project, vmodel.ProTask);
            }
            return true;
        }

        [QkAuthorize(Roles = "Dev,Delete Receipt")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Receipt Entry");
            var UserId = User.Identity.GetUserId();
            Receipt rec = db.Receipts.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.ReceiptId == id).FirstOrDefault();

            if (rec == null)
            {
                return NotFound();
            }
            return PartialView(rec);
        }
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Receipt")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            #region Old Code
            //    //receipt
            //    // delete from petransaction and adjest rate of pepayment
            //                    where a.CustomerId == Customerid.CustomerID && a.Recieptid == id
            //                    orderby a.SETransactionId
            //                        a.SalesEntry,
            //                        a.SEPayAmount
            //    // in case of supplier
            //                    where a.SupplierId == supplierid.SupplierID && a.Recieptid == id
            //                    orderby a.PRTransactionId
            //                        a.PurchaseReturnId,
            //                        a.PRPayAmount

            #endregion

            var chk = DeleteRec(id);

            var ex = db.chequetransactions.Where(o => o.referenceno == id && o.purpose == "Receipt").FirstOrDefault();
            if (ex != null)
            {
                db.chequetransactions.Remove(ex);
                db.SaveChanges();


                db.ChequeBooks.Where(o => o.bookid == ex.bookid).ToList().ForEach(o => o.usedleaf = o.usedleaf - 1);
                db.SaveChanges();
            }

            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Receipt details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Receipt")]
        public ActionResult DeleteAllReceipt(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteRec(arr) == true) ? count++ : count;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Receipt, Unable to Delete " + notdel + " Receipt. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Receipt.", true);
            }
            else
            {
                Success("Deleted " + count + " Receipt.", true);
            }
            return RedirectToAction("Index", "Receipt");
        }

        private Boolean DeleteRec(long id)
        {
            var UserId = User.Identity.GetUserId();
            var Editable = db.Receipts.Any(a => a.editable == choice.No && a.ReceiptId == id);
            if (Editable)
            {
                return false;
            }
            else
            {
                //receipt
                Receipt Rec = db.Receipts.Find(id);

                var recbillchk = com.RecBillAdjust(id);

                //// delete from petransaction and adjust rate of pepayment
                //                where a.CustomerId == Customerid.CustomerID && (a.Recieptid == 0 || a.Recieptid == id)
                //                orderby a.SETransactionId
                //                    a.SalesEntry,
                //                    a.SEPayAmount



                //// in case of supplier
                //                where a.SupplierId == supplierid.SupplierID && (a.Recieptid == 0 || a.Recieptid == id)
                //                orderby a.PRTransactionId
                //                    a.PurchaseReturnId,
                //                    a.PRPayAmount






                if (Rec.MOPayment == ModeOfPayment.PDC || Rec.MOPayment == ModeOfPayment.CDC)
                {
                    var pdcdel = db.PDCs.Where(a => (a.Reference == Rec.ReceiptId) && (a.PDCType == "Receipt")).FirstOrDefault();
                    if (pdcdel != null)
                    {
                        db.PDCs.Remove(pdcdel);
                    }
                }


                //receipt bill remove
                var RecBill = db.ReceiptBills.Where(a => a.Receipt == Rec.ReceiptId).FirstOrDefault();
                if (RecBill != null)
                {
                    db.ReceiptBills.RemoveRange(db.ReceiptBills.Where(a => a.Receipt == Rec.ReceiptId));
                    db.SaveChanges();
                }

                db.Receipts.Remove(Rec);

                /*********** Delete from AttachmentDocuments Table *********************/
                List<AttachmentDocuments> DocumentLists = new List<AttachmentDocuments>();
                DocumentLists = db.AttachmentDocuments.Where(a => (a.TransactionID == id && a.TransactionType == "Receipt")).ToList();

                var i = 0;
                foreach(var row in DocumentLists)
                {
                    //To remove the attached file from folder
                    string FullPath = LegacyWeb.MapPath("~/uploads/ReceiptDocuments/" + DocumentLists.ElementAt(i).FileName);

                    if(System.IO.File.Exists(FullPath))
                    {
                        System.IO.File.Delete(FullPath);
                    }

                    //To remove the attached file from server
                    db.AttachmentDocuments.Remove(DocumentLists[i]);
                    i++;
                }
                db.SaveChanges();

                /***********************************************************************/


                bool delete = com.DeleteAllAccountTransaction("Receipt", id);
                var DiscAllowed = db.AccountsTransactions.Where(a => (a.Account == 497 || a.Account == 498) && (a.Purpose == "Discount Allowed") && a.reference == id).FirstOrDefault();
                if (DiscAllowed != null)
                {
                    bool deleteDisc = com.DeleteAllAccountTransaction("Discount Allowed", id);
                }
                db.SaveChanges();
                com.clearsepayment();
                com.addlog(LogTypes.Deleted, UserId, "Receipt", "Receipts", findip(), id, "Receipt Deleted Successfully");
                return true;
            }
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download Receipt")]
        public ActionResult Download(long id)
        {
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            string HFCheck = ComHeadCheck.ToString();

            var Data = db.Receipts.Where(s => s.ReceiptId == id).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.VoucherNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            return File(ms, "application/pdf", "Receipt Voucher" + "-" + billno + ".pdf");


        }

        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.Receipts
                           join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                           from b in payfrom.DefaultIfEmpty()
                           join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                           from c in payto.DefaultIfEmpty()
                           let Cheque = db.PDCs.Where(a => a.Reference == id && a.PDCType == "Receipt").Select(a => a.CheckNo).FirstOrDefault()
                           where a.ReceiptId == id
                           select new
                           {
                               a.VoucherNo,
                               Payer = b.Name,
                               Reciever = c.Name,
                               a.ReceiptId,
                               a.Date,
                               a.MOPayment,
                               a.PDCDate,
                               a.PayFrom,
                               a.PayTo,
                               a.SubTotal,
                               a.GrandTotal,
                               a.Discount,
                               a.Paying,
                               a.Remark,
                               a.editable,
                               ChequeNo = Cheque != null ? Cheque : "",
                           }).ToList().Select(o => new
                           {
                               o.VoucherNo,
                               o.Payer,
                               o.Reciever,
                               o.ReceiptId,
                               o.Date,
                               o.MOPayment,
                               MOPay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                               o.PDCDate,
                               o.PayFrom,
                               o.PayTo,
                               o.SubTotal,
                               o.GrandTotal,
                               o.Discount,
                               o.Paying,
                               o.Remark,
                               o.editable,
                               o.ChequeNo,
                           }).FirstOrDefault();
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

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Receipt Voucher</b></td></tr></table>");

                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%' style='border - right: 0px;'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><td style='font-size:14px;font-weight:normal;'><b>Invoice No </b>: " + details.VoucherNo + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'><b>Date</b> : " + details.Date.ToString("dd-MM-yyyy") + "</td></tr> </table>" +
                        "</td>" +
                        "<td style='border: 0px;'>";
                    if (details.MOPayment == ModeOfPayment.CDC || details.MOPayment == ModeOfPayment.PDC)
                    {
                        partyDetails += "<table style='border: 0px;'><tr style='border: 0px;'><td style='font-size:14px;font-weight:normal;'><b>MOPayment </b>: " + details.MOPay + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'><b>Cheque No</b> : " + details.ChequeNo + "</td></tr></table>";
                    }
                    partyDetails += "</td></tr></table>";

                    sb.Append(partyDetails);
                    sb.Append("<table width='100%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc; repeat-header:yes;'>");
                    sb.Append("<thead>");
                    sb.Append("<tr style='font-size:13px;'>");
                    sb.Append("<th width='5%' style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>S/N</th>");
                    sb.Append("<th style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Particulars</th>");
                    sb.Append("<th style='padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>Debit (AED)</th>");
                    sb.Append("<th style='padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>Credit (AED)</th>");
                    sb.Append("</tr>");
                    sb.Append("</thead>");
                    sb.Append("<tbody>");

                    sb.Append("<tr style='font-size:10px;'>");
                    {
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>" + SI++ + "</b></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>Dr " + details.Reciever + "</td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + details.Paying + "</td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>");
                    }
                    sb.Append("</tr>");

                    sb.Append("<tr style='font-size:10px;'>");
                    {
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>" + SI++ + "</b></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>Cr " + details.Payer + "</td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + details.Paying + "</td>");
                    }
                    sb.Append("</tr>");

                    if (details.Discount > 0)
                    {
                        sb.Append("<tr style='font-size:10px;'>");
                        {
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>" + SI++ + "</b></td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>Dr Discount</td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + details.Discount + "</td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>");
                        }
                        sb.Append("</tr>");
                    }


                    string words = com.ConvertToWords(details.GrandTotal.ToString());

                    sb.Append("<tr style='font-size:10px;'>");
                    {
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>Total</b></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + words + "</td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + details.Paying + "</b></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + details.Paying + "</b></td>");
                    }
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");

                    if (details.Remark != null)
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr style='font-size:10px;'><td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>Narration</b> :  " + details.Remark + "</td></tr>");
                        sb.Append("</table>");
                    }
                    sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                    sb.Append("<tr>");
                    sb.Append("<td align='left' width='347px' style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>Receiver's Signature:<br />توقيع المتلقي</div>");
                    sb.Append("</td>");
                    sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>");
                    sb.Append("For " + comdetails.CName + "");
                    sb.Append("</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                }
            }
            return sb;
        }


        ////private string RecVoucherNo(Int64 SENo = 0, string billNo = null)
        ////{
        ////    var prefix = db.CodePrefixs.Where(a => a.section == "Receipt").Select(a => a.prefix).FirstOrDefault();
        ////    if (billNo == null)
        ////    {
        ////        if ((db.Receipts.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
        ////        {
        ////            billNo = prefix + 1;
        ////        }
        ////        else
        ////        {
        ////            SENo = db.Receipts.Max(p => p.Voucher + 1);
        ////            billNo = prefix + SENo;
        ////            if (BillExist(billNo))
        ////            {
        ////                billNo = RecVoucherNo(SENo, billNo);
        ////            }
        ////        }
        ////    }
        ////    else
        ////    {
        ////        SENo = SENo + 1;
        ////        billNo = prefix + SENo;
        ////        if (BillExist(billNo))
        ////        {
        ////            billNo = RecVoucherNo(SENo, billNo);
        ////        }
        ////    }
        ////    return billNo;
        ////}
        ////private bool BillExist(string SENo)
        ////{
        ////    var Exists = db.Receipts.Any(c => c.VoucherNo == SENo);
        ////    if (Exists)
        ////    {
        ////        return true;
        ////    }
        ////    else
        ////    {
        ////        return false;
        ////    }
        ////}
        ////// get max value to voucher
        ////private long Maxvoucher()
        ////{
        ////    Int64 SENo = 0;
        ////    if ((db.Receipts.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
        ////    {
        ////        SENo = 1;
        ////    }
        ////    else
        ////    {
        ////        SENo = db.Receipts.Max(p => p.Voucher + 1);
        ////    }
        ////    return SENo;
        ////}


        public ActionResult GetReceiptBill(long entry)
        {
            var data = (from b in db.ReceiptBills
                        join c in db.Receipts on b.Receipt equals c.ReceiptId
                        join d in db.SalesEntrys on b.InvoiceNo equals d.SalesEntryId into sale
                        from d in sale.DefaultIfEmpty()
                        join e in db.SEPayments on d.SalesEntryId equals e.SalesEntry into pay
                        from e in pay.DefaultIfEmpty()
                        join f in db.PurchaseReturns on b.InvoiceNo equals f.PurchaseReturnId into purs
                        from f in purs.DefaultIfEmpty()
                        join g in db.PRPayments on f.PurchaseReturnId equals g.PurchaseReturnId into pret
                        from g in pret.DefaultIfEmpty()
                        where c.ReceiptId == entry //&& (b.InvoiceNo == d.SalesEntryId || b.InvoiceNo == f.PurchaseReturnId)
                        select new
                        {
                            b.Amount,
                            id = b.InvoiceNo,
                            type = b.BillType,
                            b.Receipt,
                            b.Type,
                            b.BillType,
                            b.NewRefName,

                            SEDate = d != null ? (DateTime?)d.SEDate : null,
                            PRDate = f != null ? (DateTime?)f.PRDate : null,

                            SEBill = d.BillNo,
                            PRBill = f.BillNo,

                            SEGrandTotal = d != null ? d.SEGrandTotal : 0,
                            SEPaidAmount = e != null ? e.SEPaidAmount : 0,

                            PRGrandTotal = f != null ? f.PRGrandTotal : 0,
                            PReturnAmount = g != null ? g.PReturnAmount : 0,

                            Discount = c.Discount
                        }).AsEnumerable().Select(o => new
                        {
                            o.Amount,
                            o.id,
                            type = o.type == "Sales" ? "Sales" : "Purchase Return",
                            o.Receipt,
                            o.Type,
                            o.BillType,
                            o.NewRefName,
                            o.Discount,
                            Date = o.BillType == "Sales" ? o.SEDate : o.PRDate,
                            BillNo = o.BillType == "Sales" ? o.SEBill : o.PRBill,
                            Balance = o.BillType == "Sales" ? (o.SEGrandTotal - o.SEPaidAmount) : (o.PRGrandTotal - o.PReturnAmount),
                        }).ToList();

            return LegacyJson(data);
        }



        public ActionResult GetJornalBill(long entry, long payfrom)
        {
            var data = (from b in db.JornaltBills
                        join c in db.Journals on b.Jornal equals c.JournalId
                        join d in db.SalesEntrys on b.InvoiceNo equals d.SalesEntryId into sale
                        from d in sale.DefaultIfEmpty()
                        join e in db.SEPayments on d.SalesEntryId equals e.SalesEntry into pay
                        from e in pay.DefaultIfEmpty()
                        join f in db.PurchaseReturns on b.InvoiceNo equals f.PurchaseReturnId into purs
                        from f in purs.DefaultIfEmpty()
                        join g in db.PRPayments on f.PurchaseReturnId equals g.PurchaseReturnId into pret
                        from g in pret.DefaultIfEmpty()
                        where c.JournalId == entry //&& (b.InvoiceNo == d.SalesEntryId || b.InvoiceNo == f.PurchaseReturnId)
                        && b.payfrom == payfrom
                        select new
                        {
                            b.Amount,
                            id = b.InvoiceNo,
                            type = b.BillType,
                            b.Jornal,
                            b.Type,
                            b.BillType,
                            b.NewRefName,

                            SEDate = d != null ? (DateTime?)d.SEDate : null,
                            PRDate = f != null ? (DateTime?)f.PRDate : null,

                            SEBill = d.BillNo,
                            PRBill = f.BillNo,

                            SEGrandTotal = d != null ? d.SEGrandTotal : 0,
                            SEPaidAmount = e != null ? e.SEPaidAmount : 0,

                            PRGrandTotal = f != null ? f.PRGrandTotal : 0,
                            PReturnAmount = g != null ? g.PReturnAmount : 0,

                            Discount = c.Discount
                        }).AsEnumerable().Select(o => new
                        {
                            o.Amount,
                            o.id,
                            type = o.type == "Sales" ? "Sales" : "Purchase Return",
                            o.Jornal,
                            o.Type,
                            o.BillType,
                            o.NewRefName,
                            o.Discount,
                            Date = o.BillType == "Sales" ? o.SEDate : o.PRDate,
                            BillNo = o.BillType == "Sales" ? o.SEBill : o.PRBill,
                            Balance = o.BillType == "Sales" ? (o.SEGrandTotal - o.SEPaidAmount) : (o.PRGrandTotal - o.PReturnAmount),
                        }).ToList();

            return LegacyJson(data);
        }

        [RedirectingAction]
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View Receipt")]
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
            ReceiptViewModel vmodel = new ReceiptViewModel();
            vmodel = (from b in db.Receipts
                      join c in db.ReceiptBills on b.ReceiptId equals c.Receipt into pay
                      from c in pay.DefaultIfEmpty()
                      join d in db.Accountss on b.PayFrom equals d.AccountsID into payfrom
                      from d in payfrom.DefaultIfEmpty()
                      join e in db.Accountss on b.PayTo equals e.AccountsID into payto
                      from e in payto.DefaultIfEmpty()
                      join f in db.PDCs on b.ReceiptId equals f.Reference into pdc
                      from f in pdc.DefaultIfEmpty()
                      where b.ReceiptId == id
                      select new ReceiptViewModel
                      {
                          VoucherNo = b.VoucherNo,
                          payfromname = d.Name,
                          paytoname = e.Name,
                          MOPayment = b.MOPayment,
                          receiptDate = b.Date,
                          pdcdat = b.PDCDate,
                          CheckNo = f.CheckNo,
                          Discount = b.Discount,
                          GrandTotal = b.GrandTotal,
                          Totamt = b.Balance,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();

            var Receiptitems = db.ReceiptBills.Where(a => a.Receipt == id)
            .Select(b => new ReceiptBillViewModel
            {
                NewRefName = b.NewRefName,
                InvoiceNo = b.InvoiceNo,
                Amount = b.Amount,

            }).ToList();

            vmodel.RecItem = Receiptitems;
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Receipt" && a.Status == Status.active).ToList();

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
            long ReceiptId = 0;

            //In Create Mode, ReceiptId is last saved ID
            if (Id.Contains("undefined"))
            {
                var LastID = db.Receipts.OrderByDescending(a => a.ReceiptId).FirstOrDefault();
                ReceiptId = LastID.ReceiptId;
            }
            //In Edit Mode, ReceiptId is passed from Edit page
            else
            {
                ReceiptId = Convert.ToInt64(Id);
            }

            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/ReceiptDocuments/");

                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {
                                var fileCount   =   db.AttachmentDocuments.Select(a => a.DocumentID).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName    =   Path.GetFileName(file.FileName);

                                String extension =  Path.GetExtension(fileName);

                                var FStatus     =   Status.active;
                                String newName  =   fileCount + extension;
                                string newFName =   fileCount + extension;
                                var thumbName   =   "";
                                var resizeName  =   "";

                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/ReceiptDocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/ReceiptDocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";
                                }

                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/ReceiptDocuments/"), newName);
                                file.SaveAs(newName);

                                var RecDocument = new AttachmentDocuments
                                {
                                    TransactionID   =   ReceiptId,
                                    TransactionType =   "Receipt",
                                    FileName        =   newFName,
                                    Status          =   FStatus,
                                    CreatedDate     =   Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.AttachmentDocuments.Add(RecDocument);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/ReceiptDocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/ReceiptDocuments/"), resizeName);
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
        [QkAuthorize(Roles = "Dev,Edit Receipt")]
        public JsonResult ImageDelete(long key)
        {          
            //To remove the attached file(single row) from database
            AttachmentDocuments Document = db.AttachmentDocuments.Find(key);
            db.AttachmentDocuments.Remove(Document);
            db.SaveChanges();

            //To remove the attached file from folder
            string fullpath = LegacyWeb.MapPath("~/uploads/ReceiptDocuments/" + Document.FileName);

            if (System.IO.File.Exists(fullpath))
            {
                System.IO.File.Delete(fullpath);
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Receipt", "AttachmentDocuments", findip(), Document.DocumentID, "Receipt Document Deleted Successfully");

            bool status = true;
            string message = "Successfully deleted Receipt attachment details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = status, message = message, Id = key } };
        }
    }
}
