using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Areas.Property.Controllers
{
    [RedirectingAction]
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class PPaymentController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PPaymentController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Payment
        [QkAuthorize(Roles = "Dev,Payment List")]
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
            //ViewBag.PayMode = QkSelect.List(
            //                    new List<SelectListItem>
            //                    {
            //                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
            //                    }, "Value", "Text", 1);
            _FinancialYear();
            return View();
        }
        // datatable fields listing
        [QkAuthorize(Roles = "Dev,Payment List")]
        public JsonResult GetPayment(string InvoiceNo, string FromDate, string ToDate, long? type, long? PayFrom, long? PayTo, string user)
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Payment");
            var uDelete = User.IsInRole("Delete Payment");
            var uDownload = User.IsInRole("Download Payment");

            var v = (from a in db.Payments
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                     from c in payto.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     where (a.editable == choice.Yes) &&
                     ((InvoiceNo == null || InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     (type == null || a.MOPayment == Paymode) &&
                     (PayFrom == null || PayFrom == 0 || a.PayFrom == PayFrom) &&
                     (PayTo == null || PayTo == 0 || a.PayTo == PayTo))
                     && (user == null || user == "" || g.Id == user)
                     && (userpermission == true || a.CreatedBy == UserId)
                     select new
                     {
                         VoucherNo = a.VoucherNo,
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
                         a.CreatedDate
                     }).AsEnumerable().Select(o => new
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
                         Edit = uEdit,
                         Delete = uDelete,
                         Download = uDownload,
                         Discount = o.Discount,
                         o.CreatedDate
                     });
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p =>/* p.PaymentId.ToString().ToLower().Contains(search.ToLower()) ||*/
                                 p.VoucherNo.ToString().ToLower().Equals(search.ToLower())
                                 //p.MOPayment.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.PayFrom.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.PayTo.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.TaxAmount.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.Date.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.PDCDate.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.SubTotal.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.Payer.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.Reciever.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.GrandTotal.ToString().ToLower().Contains(search.ToLower())
                                 );

            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                //v = v.OrderBy(c => c.ProductCategoryID);
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        // create 
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create Payment")]
        public ActionResult Create()
        {
            // payfrom account
            var Paidfrom = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(Paidfrom, "ID", "Name");

            ViewBag.PaidTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1);

            var payment = new PaymentViewModel
            {
                VoucherNo = com.PayVoucherNo(),
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

            var proj = db.PropertyMains
                .Select(s => new
                {
                    ID = s.Id,
                    Name = s.Code + " " + s.Name
                })
                .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.PropertyUnits
             .Select(s => new
             {
                 ID = s.Id,
                 Name = s.Name
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            //enable bill to bill payment
            var ToPayment = db.EnableSettings.Where(a => a.EnableType == "BillToBillPayment").FirstOrDefault();
            var BillTo = ToPayment != null ? (ToPayment.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToPayment = 1;

            //enable project
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "Property" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            //companyinfo
            companySet();
            var userpermission = User.IsInRole("All Payment Entry");
            var UserId = User.Identity.GetUserId();
            ViewBag.LastEntry = db.Payments.Where(p => (p.editable == choice.Yes) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.PaymentId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            _FinancialYear();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            //field mapping
            payment.FieldMap = db.FieldMappings.Where(a => a.Section == "Payment" && a.Status == Status.active).ToList();

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
        [QkAuthorize(Roles = "Dev,Create Payment")]
        public ActionResult Create(PaymentViewModel vmodel)
        {
            string msg;
            bool stat;
            if (!com.payBillExist(Convert.ToString(vmodel.VoucherNo)))
            {
                //using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                //{
                //    try
                //    {
                //var pdCheck = db.PDCs.Where(a => a.CheckNo == vmodel.CheckNo).FirstOrDefault();
                //var PayCheck = (pdCheck != null) ? db.Payments.Where(a => a.PaymentId == pdCheck.Reference).FirstOrDefault() : null;
                var pdCheck = (from a in db.PDCs
                               join b in db.Payments on a.Reference equals b.PaymentId
                               where (a.CheckNo == vmodel.CheckNo && vmodel.PayFrom == b.PayFrom && a.PDCType == "Payment")
                               select b
                                  ).Any();

                var RepeatChequeNo = db.EnableSettings.Where(a => a.EnableType == "RepeatChequeNo").FirstOrDefault();
                var RepeatChequeNos = RepeatChequeNo != null ? RepeatChequeNo.Status : Status.inactive;

                if (pdCheck == true && RepeatChequeNos == Status.inactive)
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
                    //   var Date = Convert.ToDateTime(vmodel.Date);

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
                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                    vmodel.Ref5 = fileNames;
                    Int64 PaymentId = com.addPayment(Date, vmodel.PayFrom, vmodel.PayTo, SubTotal, Paying, GrandTotal, vmodel.Remark, UserId, Branch, 0, "Direct Payment", vmodel.TaxPer, vmodel.TaxAmount, vmodel.MOPayment, vmodel.Tax, pdcDate, choice.Yes, vmodel.CheckNo, vmodel.Bank, vmodel.pdcNote, vmodel.VoucherNo, vmodel.invoicedata, vmodel.Discount, vmodel.Project, vmodel.ProTask, vmodel.Ref1, vmodel.Ref2, vmodel.Ref3, vmodel.Ref4, vmodel.Ref5);
                    // 


                    if (file.FileName != "")
                    {
                        // var fileNames = ReceiptId + System.IO.Path.GetExtension(file.FileName);
                        var uploadUrl = LegacyWeb.MapPath("~/uploads/RecieptDoc/");
                        if (!Directory.Exists(uploadUrl))
                            Directory.CreateDirectory(uploadUrl);
                        file.SaveAs(Path.Combine(uploadUrl, fileNames));

                    }










                    string acctype = Convert.ToString(Balance["acctype"]);
                    Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();

                    //Int64 DiscRecieved = db.Accountss.Where(a => a.Name == "Discount Received").Select(a => a.AccountsID).SingleOrDefault();
                    bool? Astatus = true;
                    if (vmodel.MOPayment != ModeOfPayment.PDC)
                    {
                        var billCheck = com.BillClearPayment(vmodel.PayTo, PaytoAmount, PaymentId, Date, BranchID, UserId, acctype, null, vmodel.invoicedata);
                        Astatus = null;
                    }
                    // if payment done update to transaction
                    com.addAccountTrasaction(0, vmodel.Paying, vmodel.PayFrom, "Payment", PaymentId, DC.Credit, Date, Astatus, null, vmodel.Project, vmodel.ProTask);
                    com.addAccountTrasaction(PaytoAmount, 0, vmodel.PayTo, "Payment", PaymentId, DC.Debit, Date, Astatus, null, vmodel.Project, vmodel.ProTask);
                    if (TaxAmount > 0)
                        com.addAccountTrasaction(TaxAmount, 0, VATInput, "Expense Payment", PaymentId, DC.Debit, Date, Astatus, null, vmodel.Project, vmodel.ProTask);
                    if (vmodel.Discount > 0)
                        com.addAccountTrasaction(0, vmodel.Discount, 498, "Discount Received", PaymentId, DC.Credit, Date, Astatus, null, vmodel.Project, vmodel.ProTask);
                    // Add log
                    com.addlog(LogTypes.Created, UserId, "Payment", "Payments", findip(), PaymentId, "Successfully added Payment details");

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
                                     Phone=b.Phone
                                 });

                        vmodel.Email = v.Select(x => x.Email).FirstOrDefault();
                        vmodel.Phone = v.Select(x => x.Phone).FirstOrDefault();

                        vmodel.mobmodel = v.Select(x => x.Mobile).FirstOrDefault();
                        //var bank = db.Accountss.Where(x => x.AccountsID == vmodel.PayFrom && x.Group == 8).Select(y => y).FirstOrDefault();
                        //if (bank != null)
                        //{
                        //    var BBranch = db.Banks.Where(x => x.AccountId == bank.AccountsID).Select(y => y.BranchName).FirstOrDefault();
                        //    vmodel.Bank = bank.Name + ' ' + BBranch;
                        //}

                    }
                    //dbTran.Commit();
                    //vmodel.RefNo = (vmodel.invoicedata).Select(x => x.).ToList();

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
                    var fmapp = db.FieldMappings.Where(a => a.Section == "Payment" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully Created Payment details.";
                    stat = true;
                    //return new QuickSoft.Models.LegacyJsonResult { Data = new { tbldata = Oustanding, status = stat, data = vmodel, type = vmodel.submittype, message = msg , fmapp = fmapp } };
                    return RedirectToAction("Index", "PPayment");

                }
                //    }
                //    catch (Exception ex)
                //    {
                //        dbTran.Rollback();
                //        msg = "Failed to Created Payment.";
                //        stat = false;
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                //    }
                //}
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            return RedirectToAction("Index", "PPayment");
        }

        [QkAuthorize(Roles = "Dev,Edit Payment")]
        public ActionResult Edit(long id)
        {
            var userpermission = User.IsInRole("All Payment Entry");
            var UserId = User.Identity.GetUserId();


            Payment pay = db.Payments.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.PaymentId == id).FirstOrDefault();
            //Payment pay = db.Payments.Find(id);

            if (id != null)
            {
                ViewBag.id = id;
                var fname = db.Payments.Where(o => o.PaymentId== id).FirstOrDefault();
                ViewBag.fname = fname.Ref5;
            }
            if (pay == null)
            {
                return NotFound();
            }

            var Paidfrom = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(Paidfrom, "ID", "Name");

            var PaidTo = db.Accountss.Where(a => a.AccountsID == pay.PayTo).
                                Select(r => new
                                {
                                    ID = r.AccountsID,
                                    Name = r.Name
                                }).ToList();

            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");
            var Balance = com.Accbalance(pay.PayTo);
            ViewBag.acctype = Balance["acctype"];
            var pdcDate = (pay.PDCDate != null) ? ((DateTime)pay.PDCDate).ToString("dd-MM-yyyy") : "";

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            //enable project
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "Property" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;


            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var proj = db.PropertyMains
                .Select(s => new
                {
                    ID = s.Id,
                    Name = s.Code + " " + s.Name
                })
                .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.PropertyUnits
             .Select(s => new
             {
                 ID = s.Id,
                 Name = s.Name
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var payment = new PaymentViewModel
            {
                VoucherNo = pay.VoucherNo,
                Date = (pay.Date).ToString("dd-MM-yyyy"),
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
            };
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
            var pdcchk = db.PDCs.Where(x => x.PDCType == "Payment" && x.PDCRegDate !=null && x.Reference == id).FirstOrDefault();
            ViewBag.chkPdc = pdcchk != null ? pdcchk.RegStatus : choice.No ;

            //dummy table operations
            var DBill= db.DummyPaymentBills.Where(a => a.Payment == id).FirstOrDefault();
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
        [QkAuthorize(Roles = "Dev,Edit Payment")]
        public JsonResult Edit(long id, PaymentViewModel vmodel)
        {
       
                string msg;
            bool stat;

            var Editable = db.Payments.Any(a => a.editable == choice.No && a.PaymentId == id);
            if (Editable)
            {
                msg = "Sorry,This Payment Cannot be Editable.";
                stat = false;
            }
            else
            {
                //using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                //{
                //    try
                //    {
                //var pdcid = db.PDCs.Where(a => a.Reference == id).Select(a => a.PDCid).FirstOrDefault();
                var pdCheck = (from a in db.PDCs
                               join b in db.Payments on a.Reference equals b.PaymentId
                               where (b.PaymentId != id && a.CheckNo == vmodel.CheckNo && vmodel.PayFrom == b.PayFrom && a.PDCType == "Payment")
                               select b
                                  ).Any();

                var RepeatChequeNo = db.EnableSettings.Where(a => a.EnableType == "RepeatChequeNo").FirstOrDefault();
                var RepeatChequeNos = RepeatChequeNo != null ? RepeatChequeNo.Status : Status.inactive;

                //    var CheckNoExists = db.PDCs.Any(u => u.CheckNo == vmodel.CheckNo && u.PDCid != pdcid);
                //if (CheckNoExists)
                if (pdCheck == true && RepeatChequeNos == Status.inactive)
                {
                    msg = "Check Number Already Exist ! Try Another One..";
                    stat = false;
                }
                else
                {
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    //if (!com.payBillExist(Convert.ToString(vmodel.VoucherNo),id))
                    //{
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
                    var paydet = db.Payments.Find(id);
                    Payment Pay = paydet;
                    Payment Paytemp = paydet;

                    var vdate = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));


                    //------delete---------------------
                    var Dsupplierid = (from a in db.Suppliers where a.Accounts == Pay.PayTo select new { a.SupplierID }).SingleOrDefault();
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
                                        PEPT.PaymentId = Pay.PaymentId;
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
                                        // update transaction
                                        db.PETransactions.Add(PEPT);
                                        db.SaveChanges();

                                    }
                                }
                            }
                        }
                    }
                    // in case of customer update sales return
                    var custid = (from a in db.Customers where a.Accounts == vmodel.PayTo select new { a.CustomerID }).SingleOrDefault();
                    if (custid != null)
                    {
                        decimal Amtsum = 0;
                        // sales return Payment delete
                        var data = (from a in db.SRTransactions
                                    where a.CustomerId == custid.CustomerID && a.PaymentId == id
                                    orderby a.SRTransactionId
                                    select new
                                    {
                                        a.SalesReturnId,
                                        a.SRPayAmount
                                    }).ToList();
                        if (data.Count > 0)
                        {
                            foreach (var ditem in data)
                            {
                                var paying = ditem.SRPayAmount;
                                SRPayment PEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.SalesReturnId).FirstOrDefault();
                                PEP.SReturnAmount = PEP.SReturnAmount - Convert.ToDecimal(paying);
                                db.Entry(PEP).State = EntityState.Modified;
                                db.SaveChanges();

                                Amtsum += ditem.SRPayAmount;
                            }
                            db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == id));
                            db.SaveChanges();
                            if (vmodel.GrandTotal > Amtsum)
                            {
                                decimal payAmt = vmodel.GrandTotal - Amtsum;
                                foreach (var ditem in data)
                                {
                                    if (payAmt > 0)
                                    {
                                        SRPayment SEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.SalesReturnId).FirstOrDefault();
                                        //add to petransactions
                                        SRTransaction SEPT = new SRTransaction();
                                        SEPT.SalesReturnId = SEP.SalesReturnId;
                                        SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                                        SEPT.SRPayDate = vdate;
                                        SEPT.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                        SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                        SEPT.CreatedUserId = UserId;
                                        SEPT.PaymentId = Pay.PaymentId;
                                        SEPT.Status = 0;

                                        // transaction 
                                        var balnceamount = SEP.SRBillAmount - SEP.SReturnAmount;
                                        if (balnceamount >= payAmt)
                                        {
                                            SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(payAmt);
                                            SEPT.SRPayAmount = Convert.ToDecimal(payAmt);
                                            payAmt = 0;

                                        }
                                        else
                                        {
                                            SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(balnceamount);
                                            SEPT.SRPayAmount = Convert.ToDecimal(balnceamount);
                                            payAmt -= balnceamount;
                                        }
                                        SEP.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                        if (SEP.SRBillAmount == SEP.SReturnAmount)
                                        {
                                            SEP.Status = 1;
                                        }
                                        db.Entry(SEP).State = EntityState.Modified;
                                        db.SaveChanges();
                                        // update transaction
                                        db.SRTransactions.Add(SEPT);
                                        db.SaveChanges();
                                    }
                                }
                            }

                            





                        }
                    }
                    //------delete---------------------end


                    Int64 PaymentId = Pay.PaymentId;
                    var paybillchk = com.PayBillAdjust(PaymentId);

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

                        var RecBill = db.PaymentBills.Where(a => a.Payment == id).FirstOrDefault();
                        if (RecBill != null)
                        {
                            var PBill = db.PaymentBills.Where(a => a.Payment == id).ToList();
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

                            db.PaymentBills.RemoveRange(db.PaymentBills.Where(a => a.Payment == id));
                            db.SaveChanges();

                        }
                        PaymentBill paybill = new PaymentBill();
                        foreach (var arr in vmodel.invoicedata)
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

                            paybill.Payment = id;
                            paybill.InvoiceNo = arr.InvoiceNo;
                            paybill.BillType = arr.BillType;
                            paybill.Amount = arr.Amount;
                            paybill.Type = arr.Type; //arr.Type;
                            paybill.NewRefName = arr.NewRefName;
                            paybill.Status = arr.Status;

                            db.PaymentBills.Add(paybill);
                            retval = db.SaveChanges();
                        };
                    }
                    if (retval > 0)
                    {
                        db.DummyPaymentBills.RemoveRange(db.DummyPaymentBills.Where(a => a.Payment == id));
                        db.SaveChanges();
                    }



                    Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();

                    decimal TaxAmount = Convert.ToDecimal(vmodel.TaxAmount);
                    decimal PayTotal = Convert.ToDecimal(vmodel.Paying);
                    decimal Discount = Convert.ToDecimal(vmodel.Discount);
                    decimal PaytoAmount = PayTotal - TaxAmount + Discount;


                    // update pdc based on payment method
                    //Int64 DiscRecieved = db.Accountss.Where(a => a.Group == 497 && a.Name == "Discount Receivedd").Select(a => a.AccountsID).SingleOrDefault();
                    bool? accstat = null;
                    bool? stats = null;
                    if (Paytemp.MOPayment == vmodel.MOPayment)
                    {
                        Discfunction(PaymentId, vmodel);
                        if (Paytemp.MOPayment == ModeOfPayment.PDC || Paytemp.MOPayment == ModeOfPayment.CDC)
                        {
                            accstat = true;
                            PDC pdc = db.PDCs.Where(a => (a.Reference == Paytemp.PaymentId) && (a.PDCType == "Payment")).FirstOrDefault();
                            //  pdc.PDCDate = pcdate;
                            pdc.PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB"));
                            pdc.Note = vmodel.pdcNote;
                            pdc.CheckNo = vmodel.CheckNo;
                            pdc.Bank = vmodel.Bank;
                            pdc.Type = (vmodel.MOPayment == ModeOfPayment.PDC) ? 0 : 1;

                            pdc.Type = (vmodel.MOPayment == ModeOfPayment.PDC) ? 0 : 1;
                            pdc.Bills = Bills;

                            db.Entry(pdc).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        //decimal PaytoAmount = Convert.ToDecimal(vmodel.Discount) + Convert.ToDecimal(vmodel.Paying);
                        var pdcaccdet = db.AccountsTransactions.Where(a => (a.Account == Paytemp.PayFrom) && (a.Purpose == "Payment") && a.reference == PaymentId).FirstOrDefault();
                        if (pdcaccdet != null)
                        {
                            var aid = pdcaccdet.Id;
                            com.UpdateAccountTrasaction(aid, 0, PayTotal /*debit*/, vmodel.PayFrom, "Payment", PaymentId, DC.Credit, vdate, pdcaccdet.Status, vmodel.Project, vmodel.ProTask);
                        }
                        var pdcaccdet1 = db.AccountsTransactions.Where(a => (a.Account == Paytemp.PayTo) && (a.Purpose == "Payment") && a.reference == PaymentId).FirstOrDefault();
                        if (pdcaccdet1 != null)
                        {
                            var aid = pdcaccdet1.Id;
                            com.UpdateAccountTrasaction(aid, PaytoAmount, 0, vmodel.PayTo, "Payment", PaymentId, DC.Debit, vdate, pdcaccdet1.Status, vmodel.Project, vmodel.ProTask);
                        }


                        //status no change
                        var taxchk = db.AccountsTransactions.Where(a => (a.Account == VATInput) && (a.Purpose == "Expense Payment") && a.reference == PaymentId).FirstOrDefault();
                        if (taxchk != null)
                        {
                            if (TaxAmount > 0)
                            {
                                var aid = taxchk.Id;
                                com.UpdateAccountTrasaction(aid, TaxAmount, 0, VATInput, "Expense Payment", PaymentId, DC.Debit, vdate, taxchk.Status, vmodel.Project, vmodel.ProTask);
                            }
                            else
                            {
                                bool deleteexp = com.DeleteAllAccountTransaction("Expense Payment", id);
                            }
                        }
                        else
                        {
                            if (TaxAmount > 0)
                                com.addAccountTrasaction(TaxAmount, 0, VATInput, "Expense Payment", PaymentId, DC.Debit, vdate, accstat, null, vmodel.Project, vmodel.ProTask);

                        }
                    }
                    else
                    {
                        Discfunction(PaymentId, vmodel);

                        //bool deleteexp = com.DeleteAllAccountTransaction("Expense Payment", id);
                        //bool delete = com.DeleteAllAccountTransaction("Payment", id);
                        // curent payment type is not pdc
                        if (Paytemp.MOPayment == ModeOfPayment.PDC || Paytemp.MOPayment == ModeOfPayment.CDC)//Old
                        {
                            var pdcdel = db.PDCs.Where(a => (a.Reference == Paytemp.PaymentId) && (a.PDCType == "Payment")).FirstOrDefault();
                            db.PDCs.Remove(pdcdel);

                            if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                            {
                                //stats = true;
                                PDC pd = new PDC
                                {
                                    PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB")),
                                    PDCType = "Payment",
                                    Reference = Paytemp.PaymentId,
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
                            //com.addAccountTrasaction(0, PayTotal, vmodel.PayFrom, "Payment", PaymentId, DC.Credit, vdate, stats);
                            //com.addAccountTrasaction(PaytoAmount, 0, vmodel.PayTo, "Payment", PaymentId, DC.Debit, vdate, stats);

                            //if (TaxAmount > 0)
                            //    com.addAccountTrasaction(TaxAmount, 0, VATInput, "Expense Payment", PaymentId, DC.Debit, vdate, stats);

                        }
                        else
                        {
                            //temp entry cash
                            if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                            {
                                //stats = true;
                                PDC pd = new PDC
                                {
                                    PDCDate = DateTime.Parse(vmodel.PDCDate.ToString(), new CultureInfo("en-GB")),
                                    PDCType = "Payment",
                                    Reference = Paytemp.PaymentId,
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

                            //com.addAccountTrasaction(0, PayTotal, vmodel.PayFrom, "Payment", PaymentId, DC.Credit, vdate, stats);
                            //com.addAccountTrasaction(PaytoAmount, 0, vmodel.PayTo, "Payment", PaymentId, DC.Debit, vdate, stats);

                            //if (TaxAmount > 0)
                            //    com.addAccountTrasaction(TaxAmount, 0, VATInput, "Expense Payment", PaymentId, DC.Debit, vdate, stats);

                        }

                        //pdc status true

                        //decimal totamt = Convert.ToDecimal(vmodel.Discount) + Convert.ToDecimal(vmodel.Paying);
                        if (vmodel.MOPayment == ModeOfPayment.PDC)
                        {
                            stats = true;
                        }
                        var pdcaccdet = db.AccountsTransactions.Where(a => (a.Account == Paytemp.PayFrom) && (a.Purpose == "Payment") && a.reference == PaymentId).FirstOrDefault();
                        if (pdcaccdet != null)
                        {
                            var aid = pdcaccdet.Id;
                            com.UpdateAccountTrasaction(aid, 0, PayTotal, vmodel.PayFrom, "Payment", PaymentId, DC.Credit, vdate, stats, vmodel.Project, vmodel.ProTask);
                        }
                        var pdcaccdet1 = db.AccountsTransactions.Where(a => (a.Account == Paytemp.PayTo) && (a.Purpose == "Payment") && a.reference == PaymentId).FirstOrDefault();
                        if (pdcaccdet1 != null)
                        {
                            var aid = pdcaccdet1.Id;
                            com.UpdateAccountTrasaction(aid, PaytoAmount, 0, vmodel.PayTo, "Payment", PaymentId, DC.Debit, vdate, stats, vmodel.Project, vmodel.ProTask);
                        }



                        //var pdcaccdid = db.AccountsTransactions.Where(a => (a.Account == vmodel.PayTo) && (a.Purpose == "Discount Received") && a.reference == PaymentId).FirstOrDefault();
                        //if (pdcaccdid != null)
                        //{
                        //    var aid = pdcaccdid.Id;

                        //    com.UpdateAccountTrasaction(aid, Convert.ToDecimal(vmodel.Discount), 0, vmodel.PayFrom, "Discount Received", PaymentId, DC.Credit, vdate, stats);
                        //}

                        var taxchk = db.AccountsTransactions.Where(a => (a.Account == VATInput) && (a.Purpose == "Expense Payment") && a.reference == PaymentId).FirstOrDefault();
                        if (taxchk != null)
                        {
                            if (TaxAmount > 0)
                            {
                                var aid = taxchk.Id;
                                com.UpdateAccountTrasaction(aid, TaxAmount, 0, VATInput, "Expense Payment", PaymentId, DC.Debit, vdate, stats, vmodel.Project, vmodel.ProTask);
                            }
                            else
                            {
                                bool deleteexp = com.DeleteAllAccountTransaction("Expense Payment", id);
                            }
                        }
                        else
                        {
                            if (TaxAmount > 0)
                                com.addAccountTrasaction(TaxAmount, 0, VATInput, "Expense Payment", PaymentId, DC.Debit, vdate, stats, null, vmodel.Project, vmodel.ProTask);
                        }

                    }

                    // update payment table
                    Pay.Date = vdate;
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
                    var Balance = com.Accbalance(vmodel.PayTo);
                    if (Balance["acctype"] == (object)"Expense")
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


                    IFormFile file = Request.Form.Files["RecieptDoc"];
                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                    if (fileNames == null)
                        fileNames = "";
                    if (file != null)
                    {
                     
                        



                        if (file.FileName != "")
                        {
                            // var fileNames = ReceiptId + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/RecieptDoc/");
                            if (!Directory.Exists(uploadUrl))
                                Directory.CreateDirectory(uploadUrl);
                            file.SaveAs(Path.Combine(uploadUrl, fileNames));
                            Pay.Ref5 = fileNames;

                        }

                    }
                   
                    db.Entry(Pay).State = EntityState.Modified;
                    db.SaveChanges();


                    string acctype = Convert.ToString(Balance["acctype"]);
                    if (stats == null && !(vmodel.MOPayment == ModeOfPayment.PDC && (Paytemp.MOPayment == ModeOfPayment.Cash || Paytemp.MOPayment == ModeOfPayment.CDC)))
                    {
                        //var billCheck = com.BillClearPayment(vmodel.PayTo, PaytoAmount, PaymentId, vdate, BranchID, UserId, acctype, bill);
                        var billCheck = com.BillClearPayment(vmodel.PayTo, PaytoAmount, PaymentId, vdate, BranchID, UserId, acctype, null, vmodel.invoicedata);
                    }
                    com.addlog(LogTypes.Created, UserId, "Payment", "Payments", findip(), PaymentId, "Successfully updated Payment details");

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
                    if (BusinessType == "Scaffold")
                    {
                        vmodel.User = db.Users.Where(x => x.Id == UserId).Select(y => y.UserName).FirstOrDefault();
                        var v = (from a in db.Suppliers
                                 join x in db.Accountss on a.Accounts equals x.AccountsID
                                 join b in db.Contacts on a.Contact equals b.ContactID into tmp
                                 from b in tmp.DefaultIfEmpty()
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
                                 });
                        vmodel.Email = v.Select(x => x.Email).FirstOrDefault();
                        vmodel.mobmodel = v.Select(x => x.Mobile).FirstOrDefault();
                        //var bank = db.Accountss.Where(x => x.AccountsID == vmodel.PayFrom && x.Group==8).Select(y=>y).FirstOrDefault();
                        //if (bank != null)
                        //{
                        //    var BBranch = db.Banks.Where(x => x.AccountId == bank.AccountsID).Select(y => y.BranchName).FirstOrDefault();
                        //    vmodel.Bank = bank.Name + ' ' + BBranch;
                        //}

                    }
                    //dbTran.Commit();
                    msg = "Successfully Updated Payment details.";
                    stat = true;
                }
                //    }
                //    catch (Exception ex)
                //    {
                //        //dbTran.Rollback();
                //        msg = "Failed to Update Payment.";
                //        stat = false;
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                //    }
                //}
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
            var fmapp = db.FieldMappings.Where(a => a.Section == "Payment" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();
            //return RedirectToAction("index", "PPayment");
              return new QuickSoft.Models.LegacyJsonResult { Data = new { tbldata = Oustanding, status = stat, data = vmodel, type = vmodel.submittype, message = msg , fmapp=fmapp } };

            //}
            //else
            //{
            //    msg = "Voucher No. Already Exists.";
            //    stat = false;
            //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            //}
        }


        public Boolean Discfunction(long PaymentId, PaymentViewModel vmodel)
        {
            var Discaccdid = db.AccountsTransactions.Where(a => (a.Account == 497 || a.Account == 498) && (a.Purpose == "Discount Received") && a.reference == PaymentId).FirstOrDefault();
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

                    com.UpdateAccountTrasaction(aid, 0, Convert.ToDecimal(vmodel.Discount), 498, "Discount Received", PaymentId, DC.Credit, Date, stat,  vmodel.Project, vmodel.ProTask);
                }
                else
                {
                    db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(a => (a.Account == 497 || a.Account == 498) && a.Purpose == "Discount Received" && a.reference == PaymentId));
                    int delete = db.SaveChanges();
                }
            }
            else if (vmodel.Discount > 0)
            {
                com.addAccountTrasaction(0, Convert.ToDecimal(vmodel.Discount), 498, "Discount Received", PaymentId, DC.Credit, Date, stat, null,  vmodel.Project, vmodel.ProTask);
            }
            return true;
        }

        [QkAuthorize(Roles = "Dev,Delete Payment")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Payment Entry");
            var UserId = User.Identity.GetUserId();
            Payment pay = db.Payments.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.PaymentId == id).FirstOrDefault();

            //Payment pay = db.Payments.Find(id);
            if (pay == null)
            {
                return NotFound();
            }
            return PartialView(pay);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Payment")]
        public JsonResult Delete(long id)
        {
            bool stat = false;
            string msg;

            #region Old Code
            //var Editable = db.Payments.Any(a => a.editable == choice.No && a.PaymentId == id);
            //if (Editable)
            //{
            //    msg = "Sorry,This Payment Cannot be Deleted.";
            //    stat = false;
            //}
            //else
            //{
            //    var UserId = User.Identity.GetUserId();
            //    // payment table
            //    Payment Pay = db.Payments.Find(id);
            //    // delete from petransaction and adjest rate of pepayment
            //    var supplierid = (from a in db.Suppliers where a.Accounts == Pay.PayTo select new { a.SupplierID }).SingleOrDefault();
            //    if (supplierid != null)
            //    {
            //        var data = (from a in db.PETransactions
            //                    where a.SupplierId == supplierid.SupplierID && a.PaymentId == id
            //                    orderby a.PETransactionId
            //                    select new
            //                    {
            //                        a.PurchaseEntry,
            //                        a.PEPayAmount
            //                    }).ToList();
            //        if (data.Count > 0)
            //        {
            //            foreach (var ditem in data)
            //            {
            //                var paying = ditem.PEPayAmount;
            //                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.PurchaseEntry).FirstOrDefault();
            //                PEP.PEPaidAmount = PEP.PEPaidAmount - Convert.ToDecimal(paying);
            //                db.Entry(PEP).State = EntityState.Modified;
            //                db.SaveChanges();
            //            }
            //            db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == id));
            //        }
            //    }
            //    // in case of customer update sales return
            //    var custid = (from a in db.Customers where a.Accounts == Pay.PayTo select new { a.CustomerID }).SingleOrDefault();
            //    if (custid != null)
            //    {
            //        // sales return Payment delete
            //        var data = (from a in db.SRTransactions
            //                    where a.CustomerId == custid.CustomerID && a.PaymentId == id
            //                    orderby a.SRTransactionId
            //                    select new
            //                    {
            //                        a.SalesReturnId,
            //                        a.SRPayAmount
            //                    }).ToList();
            //        if (data.Count > 0)
            //        {
            //            foreach (var ditem in data)
            //            {
            //                var paying = ditem.SRPayAmount;
            //                SRPayment PEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.SalesReturnId).FirstOrDefault();
            //                PEP.SReturnAmount = PEP.SReturnAmount - Convert.ToDecimal(paying);
            //                db.Entry(PEP).State = EntityState.Modified;
            //                db.SaveChanges();
            //            }
            //            db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == id));
            //        }
            //    }
            //    if (Pay.MOPayment == ModeOfPayment.PDC)
            //    {
            //        var pdcdel = db.PDCs.Where(a => (a.Reference == Pay.PaymentId) && (a.PDCType == "Payment")).FirstOrDefault();
            //        db.PDCs.Remove(pdcdel);
            //    }
            //    db.Payments.Remove(Pay);
            //    db.SaveChanges();
            //    bool delete = com.DeleteAllAccountTransaction("Payment", id);
            //    bool deleteexp = com.DeleteAllAccountTransaction("Expense Payment", id);
            //    com.addlog(LogTypes.Deleted, UserId, "Payment", "Payments", findip(), id, "Payment Deleted Successfully");
            #endregion
            var chk = DeletePay(id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Payment details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Payment")]
        public ActionResult DeleteAllPay(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeletePay(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Payments, Unable to Delete " + notdel + " Payments. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Payments.", true);
            }
            else
            {
                Success("Deleted " + count + " Payments.", true);
            }
            return RedirectToAction("Index", "Payment");
        }


        private Boolean DeletePay(long sId)
        {
            var Editable = db.Payments.Any(a => a.editable == choice.No && a.PaymentId == sId);
            if (Editable)
            {
                return false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                // payment table
                Payment Pay = db.Payments.Find(sId);
                var paybillchk = com.PayBillAdjust(sId);
                //decimal Amtsum1 = 0;
                //decimal Amtsum2 = 0;
                //var paybill = db.PaymentBills.Where(a => a.Payment == Pay.PaymentId && a.Type != "Against Reference").ToList();
                //decimal amt = paybill != null ? paybill.Select(a => a.Amount).Sum() : 0;
                //// delete from petransaction and adjest rate of pepayment
                //var supplierid = (from a in db.Suppliers where a.Accounts == Pay.PayTo select new { a.SupplierID }).SingleOrDefault();
                //if (supplierid != null)
                //{
                //    var data = (from a in db.PETransactions
                //                where a.SupplierId == supplierid.SupplierID && (a.PaymentId == 0 || a.PaymentId == sId)
                //                orderby a.PETransactionId
                //                select new
                //                {
                //                    a.PurchaseEntry,
                //                    a.PEPayAmount
                //                }).ToList();
                //    if (data.Count > 0)
                //    {
                //        foreach (var ditem in data)
                //        {
                //            var paying = ditem.PEPayAmount;
                //            if (paying > 0)
                //            {
                //                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.PurchaseEntry).FirstOrDefault();
                //                PEP.PEPaidAmount = PEP.PEPaidAmount - Convert.ToDecimal(paying);
                //                db.Entry(PEP).State = EntityState.Modified;
                //                db.SaveChanges();
                //            }

                //            Amtsum1 += ditem.PEPayAmount;
                //        }
                //        db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == 0 || a.PaymentId == sId));
                //        db.SaveChanges();
                //        Amtsum1 = Amtsum1 + amt;

                //        //if (chkbill==null  && Pay.GrandTotal > Amtsum1)
                //        //{
                //        //    var tranlist = db.PETransactions.Where(a => a.PaymentId == 0).OrderByDescending(a => a.PECreatedDate).ToList();
                //        //    decimal balamt = Pay.GrandTotal - Amtsum1;
                //        //    decimal amtval = 0;


                //        //    foreach (var ditem in data)
                //        //    {
                //        //        var paying = balamt;
                //        //        PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.PurchaseEntry).FirstOrDefault();
                //        //        PEP.PEPaidAmount = PEP.PEPaidAmount - Convert.ToDecimal(paying);
                //        //        db.Entry(PEP).State = EntityState.Modified;
                //        //        db.SaveChanges();
                //        //    }
                //        //    foreach (var seitem in tranlist)
                //        //    {
                //        //        amtval += seitem.PEPayAmount;
                //        //        if (amtval <= balamt)
                //        //        {
                //        //            db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == seitem.PaymentId));
                //        //            db.SaveChanges();
                //        //        }
                //        //    }
                //        //}
                //    }
                //}
                //// in case of customer update sales return
                //var custid = (from a in db.Customers where a.Accounts == Pay.PayTo select new { a.CustomerID }).SingleOrDefault();
                //if (custid != null)
                //{
                //    // sales return Payment delete
                //    var data = (from a in db.SRTransactions
                //                where a.CustomerId == custid.CustomerID && (a.PaymentId == 0 || a.PaymentId == sId)
                //                orderby a.SRTransactionId
                //                select new
                //                {
                //                    a.SalesReturnId,
                //                    a.SRPayAmount
                //                }).ToList();
                //    if (data.Count > 0)
                //    {
                //        foreach (var ditem in data)
                //        {
                //            var paying = ditem.SRPayAmount;
                //            if (paying > 0)
                //            {
                //                SRPayment PEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.SalesReturnId).FirstOrDefault();
                //                PEP.SReturnAmount = PEP.SReturnAmount - Convert.ToDecimal(paying);
                //                db.Entry(PEP).State = EntityState.Modified;
                //                db.SaveChanges();
                //            }
                //            Amtsum2 += ditem.SRPayAmount;
                //        }
                //        db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == 0 || a.PaymentId == sId));
                //        db.SaveChanges();
                //        Amtsum2 = Amtsum2 + amt;


                //        //if (Pay.GrandTotal > Amtsum2)
                //        //{
                //        //    var tranlist = db.SRTransactions.Where(a => a.PaymentId == 0).OrderByDescending(a => a.SRCreatedDate).ToList();
                //        //    decimal balamt = Pay.GrandTotal - Amtsum2;
                //        //    decimal amtval = 0;
                //        //    foreach (var ditem in data)
                //        //    {
                //        //        var paying = balamt;
                //        //        SRPayment SRP = db.SRPayments.Where(a => a.SalesReturnId == ditem.SalesReturnId).FirstOrDefault();
                //        //        SRP.SReturnAmount = SRP.SReturnAmount - Convert.ToDecimal(paying);
                //        //        db.Entry(SRP).State = EntityState.Modified;
                //        //        db.SaveChanges();
                //        //    }
                //        //    foreach (var seitem in tranlist)
                //        //    {
                //        //        amtval += seitem.SRPayAmount;
                //        //        if (amtval <= balamt)
                //        //        {
                //        //            db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == seitem.PaymentId));
                //        //            db.SaveChanges();
                //        //        }
                //        //    }
                //        //}

                //    }
                //}
                if (Pay.MOPayment == ModeOfPayment.PDC || Pay.MOPayment == ModeOfPayment.CDC)
                {
                    var pdcdel = db.PDCs.Where(a => (a.Reference == Pay.PaymentId) && (a.PDCType == "Payment")).FirstOrDefault();
                    db.PDCs.Remove(pdcdel);
                }
                var DiscRecieved = db.AccountsTransactions.Where(a => (a.Account == 497 || a.Account == 498) && (a.Purpose == "Discount Received") && a.reference == sId).FirstOrDefault();
                bool delete = com.DeleteAllAccountTransaction("Payment", sId);
                bool deleteexp = com.DeleteAllAccountTransaction("Expense Payment", sId);
                if (DiscRecieved != null)
                {
                    bool deleteDisc = com.DeleteAllAccountTransaction("Discount Received", sId);
                }


                //receipt bill remove
                var PayBill = db.PaymentBills.Where(a => a.Payment == Pay.PaymentId).FirstOrDefault();
                if (PayBill != null)
                {
                    db.PaymentBills.RemoveRange(db.PaymentBills.Where(a => a.Payment == Pay.PaymentId));
                    db.SaveChanges();
                }

                db.Payments.Remove(Pay);
                db.SaveChanges();
                //var AccCheck = db.Accountss.Where(x => x.AccountsID == Pay.PayTo).FirstOrDefault();

                //if ((Amtsum1 != 0 && Pay.GrandTotal > Amtsum1) || (Amtsum2 != 0 && Pay.GrandTotal > Amtsum2))
                //{
                //    var callfn = (AccCheck.Group == 12) ? com.CusPayment(AccCheck.AccountsID, (DateTime)Pay.Date, Pay.Branch, Pay.CreatedBy) : com.SuplPayment(AccCheck.AccountsID, (DateTime)Pay.Date, Pay.Branch, Pay.CreatedBy);
                //}
                com.addlog(LogTypes.Deleted, UserId, "Payment", "Payments", findip(), sId, "Payment Deleted Successfully");
                return true;
            }
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download Payment")]
        public ActionResult Download(long id)
        {
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            string HFCheck = ComHeadCheck.ToString();

            var Data = db.Payments.Where(s => s.PaymentId == id).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.VoucherNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            return File(ms, "application/pdf", "Payment Voucher" + "-" + billno + ".pdf");

            //var PayDet = db.Payments.Where(s => s.PaymentId == id).FirstOrDefault();
            //var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            //var billno = PayDet.VoucherNo;
            //SendMail sm = new SendMail();
            //byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            //return File(ms, "application/pdf", "Payment Voucher" + "-" + billno + ".pdf");            

        }

        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.Payments
                           join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                           from b in payfrom.DefaultIfEmpty()
                           join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                           from c in payto.DefaultIfEmpty()
                           let Cheque = db.PDCs.Where(a => a.Reference == id && a.PDCType == "Payment").Select(a => a.CheckNo).FirstOrDefault()
                           where a.PaymentId == id
                           select new
                           {
                               a.VoucherNo,
                               Payer = b.Name,
                               Reciever = c.Name,
                               a.PaymentId,
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
                               a.TaxAmount,
                               a.TaxPer,
                               ChequeNo = Cheque != null ? Cheque : "",
                           }).ToList().Select(o => new
                           {
                               o.VoucherNo,
                               o.Payer,
                               o.Reciever,
                               o.PaymentId,
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
                               o.TaxAmount,
                               o.TaxPer,
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
                    sb.Append("<table  width='100%' style='border: 0px;text-align:center;'><tr><td><b>Payment Voucher</b></td></tr></table>");

                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><td style='font-size:14px;font-weight:normal;'><b>Invoice No </b>: " + details.VoucherNo + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'><b>Date</b> : " + details.Date.ToString("dd-MM-yyyy") + "</td></tr></table> " +
                        "</td>" +
                        "<td style='border: 0px;'>";
                        if (details.MOPayment == ModeOfPayment.CDC || details.MOPayment == ModeOfPayment.PDC)
                        {
                            partyDetails += "<table  style='border: 0px !important;'><tr><td style='font-size:14px;font-weight:normal;'><b>MOPayment </b>: " + details.MOPay + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'><b>Cheque No</b> : " + details.ChequeNo + "</td></tr></table>";
                        }
                    partyDetails += "</td></tr></table>";

                    //string partyDetails = "<table  style='border: 0px; width: 100 %;'><tr><th>Invoice No</th><td style='font-size:14px;font-weight:normal;'>: " + details.VoucherNo + "</td></tr><tr><th>Date تاريخ</th><td style='font-size:14px;font-weight:normal;'>: " + details.Date.ToString("dd-MM-yyyy") + "</td></tr> </table>";
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
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>Cr " + details.Payer + "</td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + details.GrandTotal + "</td>");
                    }
                    sb.Append("</tr>");


                    if (details.TaxAmount > 0)
                    {
                        sb.Append("<tr style='font-size:10px;'>");
                        {
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>" + SI++ + "</b></td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>Dr " + details.Reciever + "</td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + details.SubTotal + "</td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>");

                        }
                        sb.Append("</tr>");

                        sb.Append("<tr style='font-size:10px;'>");
                        {
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>" + SI++ + "</b></td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>Tax Amount " + details.TaxPer + "(%)</td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + details.TaxAmount + "</td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>");
                        }
                        sb.Append("</tr>");
                    }
                    else
                    {
                        sb.Append("<tr style='font-size:10px;'>");
                        {
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>" + SI++ + "</b></td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>Dr " + details.Reciever + "</td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + details.GrandTotal + "</td>");
                            sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>");
                        }
                        sb.Append("</tr>");
                    }

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
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + details.GrandTotal + "</b></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + details.GrandTotal + "</b></td>");
                    }
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    if (details.Remark != null)
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr style='font-size:10px;'><td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>Narration</b> : " + details.Remark + "</td></tr>");
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



        public ActionResult GetPaymentBill(long entry)
        {
            var data = (from b in db.PaymentBills
                        join c in db.Payments on b.Payment equals c.PaymentId
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

            var MOP = db.Payments.Where(x => x.PaymentId == id).Select(y => y.MOPayment).FirstOrDefault();
            ViewBag.MOPayment = (MOP == ModeOfPayment.Cash) ? 0 : 1;
            PaymentViewModel vmodel = new PaymentViewModel();
            vmodel = (from b in db.Payments
                      join c in db.PaymentBills on b.PaymentId equals c.Payment into pay
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
                          VoucherNo=b.VoucherNo,
                          payfromname = d.Name,
                          paytoname=e.Name,
                          MOPayment=b.MOPayment,
                          PaymentDate = b.Date,
                          pdcdat = b.PDCDate,
                          CheckNo=f.CheckNo,
                          Discount=b.Discount,
                          GrandTotal=b.GrandTotal,
                          TaxAmount=b.TaxAmount,
                          Totamt=b.Balance,
                          Ref1=b.Ref1,
                          Ref2=b.Ref2,
                          Ref3=b.Ref3,
                          Ref4=b.Ref4,
                          Ref5=b.Ref5,

                      }).FirstOrDefault();

            var paymentitems = db.PaymentBills.Where(a => a.Payment == id)
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
        [HttpPost]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object


            string id = Request.Form.GetValues("id").First();
            long jrlid = 0;
            if (id.Contains("undefined") || id.Contains("0"))
            {
                
                var lastid = db.Payments.OrderByDescending(o => o.PaymentId).FirstOrDefault();
                jrlid = lastid.PaymentId;

            }
            else
            {
                jrlid = Convert.ToInt64(id);
            }
            if (Request.Form.Files.Count > 0)
            {
                try
                {




                    IFormFile file = Request.Form.Files[0];
                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                    if (file.FileName != "")
                    {
                        // var fileNames = ReceiptId + System.IO.Path.GetExtension(file.FileName);
                        var uploadUrl = LegacyWeb.MapPath("~/uploads/RecieptDoc/");
                        if (!Directory.Exists(uploadUrl))
                            Directory.CreateDirectory(uploadUrl);
                        file.SaveAs(Path.Combine(uploadUrl, fileNames));

                     
                        var jrn = db.Payments.Find(jrlid);
                        jrn.Ref5= fileNames;
                        db.Entry(jrn).State = EntityState.Modified;
                        db.SaveChanges();


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