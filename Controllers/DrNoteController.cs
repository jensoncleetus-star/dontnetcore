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
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class DrNoteController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public DrNoteController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,DrNote List")]
        public ActionResult Index()
        {
            ViewBag.Paidto = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.Paidfrom = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,DrNote List")]
        public ActionResult GetDrNote(string InvoiceNo, string FromDate, string ToDate, long? supplier, long? drtype, long? PayFrom, long? PayTo)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            drtype = drtype == 0 ? null : drtype;

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var v = (from a in db.DrNotes
                     join b in db.Accountss on a.PayTo equals b.AccountsID into Payt
                     from b in Payt.DefaultIfEmpty()
                     join d in db.Accountss on a.PayFrom equals d.AccountsID into payf
                     from d in payf.DefaultIfEmpty()
                     join c in db.Users on a.CreatedBy equals c.Id
                     where
                      (InvoiceNo == "" || a.BillNo == InvoiceNo) &&
                      (FromDate == "" || EF.Functions.DateDiffDay(a.DNDate, fdate) <= 0) &&
                      (ToDate == "" || EF.Functions.DateDiffDay(a.DNDate, tdate) >= 0) &&
                      (PayFrom == null || PayFrom == 0 || a.PayFrom == PayFrom) &&
                      (PayTo == null || PayTo == 0 || a.PayTo == PayTo) 
                      //&& (drtype == -1 || a.DebitType == drtype)
                     select new
                     {
                         a.DrNoteId,
                         a.BillNo,
                         a.DNDate,
                         a.GrandTotal,
                         PayFrom = d.Name,
                         PayTo = b.Name,
                         Tax = a.TaxAmount,
                         TaxP = a.TaxPer,
                         a.SubTotal,
                         Amount = a.GrandTotal,
                         a.DebitType,
                         User = c.UserName,
                         a.CreatedDate
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.PayFrom.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.PayTo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.DrNoteId.ToString().ToLower().Contains(search.ToLower())
                                 );
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            else
            {
                v = v.OrderByDescending(b => Convert.ToInt64(b.DrNoteId));
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,DrNote Entry")]
        [HttpGet]
        public ActionResult Create()
        {
            var PaidF = db.Accountss.Where(p => p.AccountsID != 499).Select(r => new //Except Stock adjustment Expense
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(PaidF, "ID", "Name");

            ViewBag.PaidTo = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1);


            var drnote = new DrNoteViewModel
            {
                BillNo = InvoiceNo(),
                DNDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                DNNote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "DrNote").Select(a => a.TermsCondit).FirstOrDefault(),
                Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList()
            };

            ViewBag.LastEntry = db.DrNotes.Select(p => p.DrNoteId).AsEnumerable().DefaultIfEmpty(0).Max();
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            //enable bill adjustment
            var billAdj = db.EnableSettings.Where(a => a.EnableType == "DNBillAdjust").FirstOrDefault();
            var DNBillAdj = billAdj != null ? (billAdj.Status == Status.active ? 0 : 1) : 1;
            ViewBag.DNBillAdj = DNBillAdj;

            companySet();
            return View(drnote);
        }

        //[RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,DrNote Entry")]
        public JsonResult CreateDrNote(string[][] array, string[] dndata)
        {
            bool stat = false;
            string msg;
            string action = dndata[12];
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var date = DateTime.Parse(dndata[1].ToString(), new CultureInfo("en-GB"));
            var CreatedDate = Convert.ToDateTime(System.DateTime.Now);
            var PayFrom = Convert.ToInt64(dndata[3]);
            var PayTo = Convert.ToInt64(dndata[4]);

            DrNote Drentry = new DrNote();

            Drentry.DNNo = GetCNNo();
            Drentry.BillNo = Convert.ToString(dndata[0]);
            Drentry.DebitType = Convert.ToInt32(dndata[2]);

            Drentry.DNDate = date;
            Drentry.PayFrom = PayFrom;
            Drentry.PayTo = PayTo;

            Drentry.DNNote = Convert.ToString(dndata[5]);
            Drentry.Remarks = Convert.ToString(dndata[6]);

            Drentry.SubTotal = Convert.ToDecimal(dndata[7]);
            Drentry.Tax = Convert.ToInt64(dndata[8]);
            Drentry.TaxPer = Convert.ToDecimal(dndata[9]);
            Drentry.TaxAmount = Convert.ToDecimal(dndata[10]);
            Drentry.GrandTotal = Convert.ToDecimal(dndata[11]);

            Drentry.CreatedDate = CreatedDate;
            Drentry.CreatedBy = UserId;
            Drentry.Status = 1;
            Drentry.Branch = BranchID;

            db.DrNotes.Add(Drentry);
            db.SaveChanges();
            Int64 DNId = Drentry.DrNoteId;

            var paying = Convert.ToDecimal(dndata[11]);//grand total
            //enable bill adjustment
            var billAdj = db.EnableSettings.Where(a => a.EnableType == "DNBillAdjust").FirstOrDefault();
            var DNBillAdj = billAdj != null ? (billAdj.Status == Status.active ? 0 : 1) : 1;
            if (DNBillAdj == 1)
            {
                addtransaction(DNId, paying);
            }
            var TaxAmount = Convert.ToDecimal(dndata[10]);
            var GrandTotal = Convert.ToDecimal(dndata[11]);
            decimal payamount = Convert.ToDecimal(dndata[7]);
            Int64 payfromAccID = db.Customers.Where(a => a.CustomerID == Drentry.PayFrom).Select(a => a.Accounts).FirstOrDefault();
            Int64 paytoAccID = db.Customers.Where(a => a.CustomerID == Drentry.PayTo).Select(a => a.Accounts).FirstOrDefault();
            Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();

            //add trasaction to debit note account
            com.addAccountTrasaction(payamount, 0, paytoAccID, "DebitNote", DNId, DC.Debit, date);
            //add trasaction 
            com.addAccountTrasaction(0, GrandTotal, payfromAccID, "DebitNote", DNId, DC.Credit, date);


            //add vat input in account transaction
            if (TaxAmount > 0)
            {
                com.addAccountTrasaction(TaxAmount, 0, VATOutput, "DebitNote", DNId, DC.Debit, date);
            }

            com.addlog(LogTypes.Created, UserId, "DrNote", "DrNotes", findip(), DNId, "Successfully Submitted Debit Note");

            if (action == "print")
            {
                var summary = (from a in db.DrNotes
                               join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                               from b in payfrom.DefaultIfEmpty()
                               join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                               from c in payto.DefaultIfEmpty()
                               where a.DrNoteId == DNId
                               select new
                               {
                                   Date = a.DNDate,
                                   VoucherNo = a.BillNo,
                                   PayFrom = b.Name,
                                   PayTo = c.Name,
                                   CrAmt = a.GrandTotal
                               }).FirstOrDefault();


                msg = "Successfully submitted Dr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, summary = summary, DNId = DNId } };
            }
            else
            {
                msg = "Successfully submitted Dr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, DNId = DNId } };
            }

        }

        [HttpPost]
        public JsonResult UpdatePayTrans(string[][] array, long cnId, string action)
        {
            bool stat = false;
            string msg;
            if (array != null)
            {
                updateinvoicebase(array, cnId);
            }
            DrNote DNentry = db.DrNotes.Find(cnId);
            if (DNentry.GrandTotal > 0)
            {
                var paying = DNentry.GrandTotal;
                addtransaction(cnId, paying);
            }

            if (action == "print")
            {
                var summary = (from a in db.DrNotes
                               join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                               from b in payfrom.DefaultIfEmpty()
                               join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                               from c in payto.DefaultIfEmpty()
                               where a.DrNoteId == cnId
                               select new
                               {
                                   Date = a.DNDate,
                                   VoucherNo = a.BillNo,
                                   PayFrom = b.Name,
                                   PayTo = c.Name,
                                   CrAmt = a.GrandTotal
                               }).FirstOrDefault();


                msg = "Successfully submitted Dr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, summary = summary } };
            }
            else
            {
                msg = "Successfully submitted Dr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [HttpPost]
        public JsonResult UpdatePayTransEdit(string[][] array, long cnId, string action)
        {
            bool stat = false;
            string msg;
            if (array != null)
            {
                var DrInvoice = db.DrInvoices.Where(a => a.DrNoteId == cnId).FirstOrDefault();
                if (DrInvoice != null)
                {
                    db.DrInvoices.RemoveRange(db.DrInvoices.Where(a => a.DrNoteId == cnId));
                }
                deletetransaction(cnId);
                updateinvoicebase(array, cnId);
            }
            DrNote DNentry = db.DrNotes.Find(cnId);
            if (DNentry.GrandTotal > 0)
            {
                var Suppid = (from a in db.Suppliers where a.Accounts == DNentry.PayFrom select new { a.SupplierID }).SingleOrDefault();
                if (Suppid != null)
                {
                    var paying = DNentry.GrandTotal;
                    deletetransaction(cnId);
                    addtransaction(cnId, paying);
                }
            }
            if (action == "print")
            {
                var summary = (from a in db.DrNotes
                               join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                               from b in payfrom.DefaultIfEmpty()
                               join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                               from c in payto.DefaultIfEmpty()
                               where a.DrNoteId == cnId
                               select new
                               {
                                   Date = a.DNDate,
                                   VoucherNo = a.BillNo,
                                   PayFrom = b.Name,
                                   PayTo = c.Name,
                                   CrAmt = a.GrandTotal
                               }).FirstOrDefault();


                msg = "Successfully submitted Dr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, summary = summary } };
            }
            else
            {
                msg = "Successfully submitted Dr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit DrNote")]
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DrNote DrNote = db.DrNotes.Find(id);

            if (DrNote == null)
            {
                return NotFound();
            }

            var PaidF = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(PaidF, "ID", "Name");



            DrNoteViewModel vmodel = new DrNoteViewModel();
            vmodel = (from a in db.DrNotes
                      join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                      from b in payfrom.DefaultIfEmpty()
                      where a.DrNoteId == id
                      select new DrNoteViewModel
                      {
                          DNDate = a.DNDate,
                          BillNo = a.BillNo,

                          DrNoteId = a.DrNoteId,
                          PayFrom = a.PayFrom,
                          PayTo = a.PayTo,
                          

                          SubTotal = a.SubTotal,
                          Tax = a.Tax,
                          TaxPer = a.TaxPer,
                          TaxAmount = a.TaxAmount,
                          GrandTotal = a.GrandTotal,

                          Remarks = a.Remarks,
                          DNNote = a.DNNote,
                          DebitType = a.DebitType,

                          Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList()
                      }).FirstOrDefault();

            var PaidT = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).
               Select(r => new
               {
                   ID = r.AccountsID,
                   Name = r.Name
               }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidT, "ID", "Name");


            ViewBag.preEntry = db.DrNotes.Where(a => a.DrNoteId < id).Select(a => a.DrNoteId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.DrNotes.Where(a => a.DrNoteId > id).Select(a => a.DrNoteId).DefaultIfEmpty().Min();

            //enable bill adjustment
            var billAdj = db.EnableSettings.Where(a => a.EnableType == "DNBillAdjust").FirstOrDefault();
            var DNBillAdj = billAdj != null ? (billAdj.Status == Status.active ? 0 : 1) : 1;
            ViewBag.DNBillAdj = DNBillAdj;

            companySet();
            return View(vmodel);
        }

        //[RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Edit DrNote")]
        public ActionResult UpdateDrNote(string[][] array, string[] dndata)
        {
            bool stat = false;
            string msg;
            string action = dndata[12];
            Int64 DNId = Convert.ToInt64(dndata[13]);
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            DrNote DNentry = db.DrNotes.Find(DNId);

            DNentry.DNNo = GetCNNo();
            DNentry.BillNo = Convert.ToString(dndata[0]);
            DNentry.DebitType = Convert.ToInt32(dndata[2]);

            DNentry.DNDate = DateTime.Parse(dndata[1].ToString(), new CultureInfo("en-GB"));
            DNentry.PayFrom = Convert.ToInt64(dndata[3]);
            DNentry.PayTo = Convert.ToInt64(dndata[4]);

            DNentry.DNNote = Convert.ToString(dndata[5]);
            DNentry.Remarks = Convert.ToString(dndata[6]);

            DNentry.SubTotal = Convert.ToDecimal(dndata[7]);
            DNentry.Tax = Convert.ToInt64(dndata[8]);
            DNentry.TaxPer = Convert.ToDecimal(dndata[9]);
            DNentry.TaxAmount = Convert.ToDecimal(dndata[10]);
            DNentry.GrandTotal = Convert.ToDecimal(dndata[11]);


            db.Entry(DNentry).State = EntityState.Modified;
            db.SaveChanges();

            var paying = Convert.ToDecimal(dndata[11]);//grand total
            //enable bill adjustment
            var billAdj = db.EnableSettings.Where(a => a.EnableType == "DNBillAdjust").FirstOrDefault();
            var DNBillAdj = billAdj != null ? (billAdj.Status == Status.active ? 0 : 1) : 1;
            if (DNBillAdj == 1)
            {
                var Suppid = (from a in db.Suppliers where a.Accounts == DNentry.PayFrom select new { a.SupplierID }).SingleOrDefault();
                if (Suppid != null)
                {
                    deletetransaction(DNId);
                    addtransaction(DNId, paying);
                }
            }
            var TaxAmount = Convert.ToDecimal(dndata[10]);
            var GrandTotal = Convert.ToDecimal(dndata[11]);
            decimal payamount = Convert.ToDecimal(dndata[7]);
            Int64 payfromAccID = db.Suppliers.Where(a => a.SupplierID == DNentry.PayFrom).Select(a => a.Accounts).FirstOrDefault();
            Int64 paytoAccID = db.Suppliers.Where(a => a.SupplierID == DNentry.PayTo).Select(a => a.Accounts).FirstOrDefault();
            Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();
            var date = DateTime.Parse(dndata[1], new CultureInfo("en-GB"));
            
            //delete
            bool delete = com.DeleteAllAccountTransaction("DebitNote", DNId);

            //add trasaction to credit note account
            com.addAccountTrasaction(payamount, 0, paytoAccID, "DebitNote", DNId, DC.Debit, date);
            //add trasaction 
            com.addAccountTrasaction(0, GrandTotal, payfromAccID, "DebitNote", DNId, DC.Credit, date);


            //add vat input in account transaction
            if (TaxAmount > 0)
            {
                com.addAccountTrasaction(TaxAmount, 0, VATOutput, "DebitNote", DNId, DC.Debit, date);
            }

            com.addlog(LogTypes.Created, UserId, "DrNote", "DrNotes", findip(), DNId, "Successfully Submitted Debit Note");

            if (action == "print")
            {
                var summary = (from a in db.DrNotes
                               join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                               from b in payfrom.DefaultIfEmpty()
                               join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                               from c in payto.DefaultIfEmpty()
                               where a.DrNoteId == DNId
                               select new
                               {
                                   Date = a.DNDate,
                                   VoucherNo = a.BillNo,
                                   PayFrom = b.Name,
                                   PayTo = c.Name,
                                   CrAmt = a.GrandTotal
                               }).FirstOrDefault();


                msg = "Successfully submitted Dr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, summary = summary, DNId = DNId } };
            }
            else
            {
                msg = "Successfully submitted Dr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, DNId = DNId } };
            }
        }

        public void deletetransaction(long DNId)
        {
            DrNote DNote = db.DrNotes.Find(DNId);
            var Suppid = (from a in db.Suppliers where a.Accounts == DNote.PayFrom select new { a.SupplierID }).SingleOrDefault();
            if (Suppid != null)
            {
                //delete
                var data = (from a in db.PETransactions
                            where a.SupplierId == Suppid.SupplierID && a.PaymentId == DNId && a.type == "DrNote"
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
                        PEP.DebitAmount = PEP.DebitAmount - paying;
                        db.Entry(PEP).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == DNId && a.type == "DrNote"));
                    db.SaveChanges();
                }
            }
            //                where a.SupplierId == Dsupplierid.SupplierID && a.PaymentId == CNId && a.type == "DrNote"
            //                orderby a.PETransactionId
            //                    a.PurchaseEntry,
            //                    a.PEPayAmount
        }


        public void addtransaction(long DNId, decimal paying)
        {
            DrNote DNentry = db.DrNotes.Find(DNId);
            //add
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var Suppid = (from a in db.Suppliers where a.Accounts == DNentry.PayFrom select new { a.SupplierID }).SingleOrDefault();
            if (Suppid != null)
            {
                var v = (from a in db.PurchaseEntrys
                         join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                         where a.Supplier == Suppid.SupplierID && (c.PEPaidAmount + c.DebitAmount) != c.PEBillAmount
                         orderby a.PurchaseEntryId
                         select new
                         {
                             invoiceno = a.BillNo,
                             Date = a.PEDate,
                             total = a.PEGrandTotal,
                             paid = c.PEPaidAmount,
                             sid = a.PurchaseEntryId
                         });
                var dataa = v.ToList();
                if (dataa.Count > 0)
                {
                    foreach (var ditem in dataa)
                    {
                        if (paying > 0)
                        {
                            PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.sid).FirstOrDefault();
                            //add to petransactions
                            PETransaction PEPT = new PETransaction();
                            PEPT.PurchaseEntry = PEP.PurchaseEntry;
                            PEPT.SupplierId = Convert.ToInt64(PEP.SupplierId);
                            PEPT.PEPayDate = DNentry.DNDate;
                            PEPT.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            PEPT.CreatedBranch = Convert.ToInt64(BranchID);
                            PEPT.CreatedUserId = UserId;
                            PEPT.Status = 0;
                            PEPT.PaymentId = DNId;
                            PEPT.type = "DrNote";
                            // transaction 
                            var balnceamount = PEP.PEBillAmount - (PEP.PEPaidAmount + PEP.DebitAmount);
                            if (balnceamount >= paying)
                            {
                                PEP.DebitAmount = PEP.DebitAmount + Convert.ToDecimal(paying);
                                PEPT.PEPayAmount = Convert.ToDecimal(paying);
                                paying = 0;
                            }
                            else
                            {
                                PEP.DebitAmount = PEP.DebitAmount + Convert.ToDecimal(balnceamount);
                                PEPT.PEPayAmount = Convert.ToDecimal(balnceamount);
                                paying -= Convert.ToDecimal(balnceamount);
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

        public void updateinvoicebase(string[][] array, long cnId)
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            DrInvoice DrInvoice = new DrInvoice();
            foreach (var arr in array)
            {
                var cramt = Convert.ToDecimal(arr[1]);
                if (arr[0] != null && arr[0] != "" && cramt > 0)
                {
                    var entrynos = Convert.ToInt64(arr[2]);

                    DrInvoice.DrNoteId = cnId;
                    DrInvoice.EntryNo = entrynos;
                    DrInvoice.TransType = Convert.ToString(arr[3]);
                    DrInvoice.DebitAmount = Convert.ToDecimal(arr[1]);

                    db.DrInvoices.Add(DrInvoice);
                    db.SaveChanges();

                    var pinvoice = db.PurchaseEntrys.Where(a => a.PurchaseEntryId == entrynos).FirstOrDefault();
                    var spyid = db.PEPayments.Where(a => a.PurchaseEntry == entrynos).FirstOrDefault();
                    decimal drAmount = Convert.ToDecimal(arr[1]);

                    //purchase payment
                    PEPayment ppay = db.PEPayments.Find(spyid.PEPaymentId);
                    ppay.DebitAmount = (ppay.DebitAmount != null ? ppay.DebitAmount : 0) + drAmount;
                    db.Entry(ppay).State = EntityState.Modified;
                    db.SaveChanges();

                    //add hire transacvtion
                    PETransaction PEtran = new PETransaction();
                    PEtran.PurchaseEntry = pinvoice.PurchaseEntryId;
                    PEtran.SupplierId = pinvoice.Supplier;
                    PEtran.PEPayDate = pinvoice.PEDate;
                    PEtran.PEPayAmount = drAmount;
                    PEtran.PaymentId = cnId;
                    PEtran.type = "DrNote";
                    PEtran.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    PEtran.CreatedUserId = UserId;
                    PEtran.Status = 1;
                    PEtran.CreatedBranch = Convert.ToInt64(BranchID);

                    db.PETransactions.Add(PEtran);
                    db.SaveChanges();
                }
            }
        }

        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Delete DrNote")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DrNote cnote = db.DrNotes.Find(id);
            if (cnote == null)
            {
                return NotFound();
            }
            return PartialView(cnote);
        }

        //[RedirectingAction]
        [HttpPost, ActionName("Delete")]
        //[QkAuthorize(Roles = "Dev,Delete DrNote")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            DrNote SErt = db.DrNotes.Find(id);
            var DrInvoice = db.DrInvoices.Where(a => a.DrNoteId == id).ToList();
           
            if (DrInvoice != null)
            {
                db.DrInvoices.RemoveRange(db.DrInvoices.Where(a => a.DrNoteId == id));
            }

            deletetransaction(id);

            bool delete = com.DeleteAllAccountTransaction("DebitNote", id);

            db.DrNotes.Remove(SErt);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "DrNote", "DrNotes", findip(), SErt.DrNoteId, "Successfully Deleted Debit Note");

            stat = true;
            msg = "Successfully deleted Dr.Note.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete DrNote")]
        public ActionResult DeleteAllDrNote(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteAll(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Dr.Note Entry.", true);
            return RedirectToAction("Index", "DrNote");
        }
        private Boolean DeleteAll(long sId)
        {
            var UserId = User.Identity.GetUserId();
            DrNote SErt = db.DrNotes.Find(sId);

            var DrInvoice = db.DrInvoices.Where(a => a.DrNoteId == sId).ToList();
            if (DrInvoice != null)
            {
                db.DrInvoices.RemoveRange(db.DrInvoices.Where(a => a.DrNoteId == sId));
            }

            deletetransaction(sId);
            bool delete = com.DeleteAllAccountTransaction("DebitNote", sId);

            com.addlog(LogTypes.Deleted, UserId, "DrNote", "DrNotes", findip(), SErt.DrNoteId, "Successfully Deleted Debit Note");
            db.DrNotes.Remove(SErt);
            db.SaveChanges();
            return true;
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Download DrNote")]
        public ActionResult Download(long id)
        {
            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = db.DrNotes.Where(s => s.DrNoteId == id).Select(s => s.BillNo).FirstOrDefault();

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id));
            return File(ms, "application/pdf", cname + "_DrNote_" + billno + "_" + System.DateTime.Now.ToShortDateString() + ".pdf");

        }
        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.DrNotes
                           join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                           from b in payfrom.DefaultIfEmpty()
                           join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                           from c in payto.DefaultIfEmpty()
                           where a.DrNoteId == id
                           select new
                           {
                               Date = a.DNDate,
                               a.BillNo,
                               PayFrom = b.Name,
                               PayTo = c.Name,
                               a.GrandTotal,
                               a.Remarks,
                               a.DNNote

                           }).FirstOrDefault();


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


            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {

                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Debit Note</b></td></tr></table>");
                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='100%' style='border-left: 1px solid #ccc;'>" +
                        "<table  style='border: 0px; width: 100 %;'><tr><th>Voucher No</th><td style='font-size:14px;font-weight:normal;'>: " + details.BillNo + "</td></tr>";

                    partyDetails += "<tr><th>Date تاريخ</th><td style='font-size:14px;font-weight:normal;'>: " + details.Date.ToString("dd-MM-yyyy") + "</td></tr></table></td></tr></table>";
                    sb.Append(partyDetails);


                    sb.Append("<table width='100%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc;'>");
                    sb.Append("<thead>");
                    sb.Append("<tr style='font-size:13px;'>");
                    sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;text-align: center;border: .1px solid #ccc;'>S/N</th>");
                    sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;text-align: center;border: .1px solid #ccc;'>Pay From</th>");
                    sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;text-align: center;border: .1px solid #ccc;'>Pay To</th>");
                    sb.Append("<th style='border: .5px #000000;padding: 5px;vertical-align: top;text-align: center;border: 1px solid #ccc;'>Debit Amount</th>");
                    sb.Append("</tr>");
                    sb.Append("</thead>");
                    sb.Append("<tbody>");
                    sb.Append("<tr style='font-size:10px;'>");
                    sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: center;border: .1px solid #ccc;'>1</td>");
                    sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: center;border: .1px solid #ccc;'>" + details.PayFrom + "</td>");
                    sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: center;border: .1px solid #ccc;'>" + details.PayTo + "</td>");
                    sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: center;border: .1px solid #ccc;'>" + details.GrandTotal + "</td>");
                    sb.Append("</tr>");
                    sb.Append("</tbody>");
                    sb.Append("</table>");

                    string words = com.ConvertToWords(details.GrandTotal.ToString());
                    sb.Append("<table width='100%' style='border-collapse: collapse;border: .1px solid #ccc;font-size: 14px;'>");

                    string word = "<tr class='border-top'><td width='50%' style='border: .1px solid #ccc;padding: 10px;font-size: 15px;' ><strong>" + words + " </strong></td><td style='border: .1px solid #ccc;padding: 10px;'>Amount كمية</td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.GrandTotal + "</td></tr>";

                    var dnote = "";
                    if (details.DNNote != null)
                    {
                        dnote = details.DNNote.Replace("\n", "<br />");
                    }

                    string tc = "<tr class='border-top'><td colspan='3' style='border: .1px solid #ccc;padding: 10px;' ><strong><u> Terms And Conditions :</u></strong><br/>" + dnote + " </td></tr>";

                    string remarks = "";
                    if (!string.IsNullOrEmpty(details.Remarks) && !string.IsNullOrWhiteSpace(details.Remarks))
                    {
                        remarks = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;'><strong><u>Remarks :</u></strong><br/>" + details.Remarks.Replace("\n", "<br />") + "</td></tr>";
                    }

                    sb.Append(word + tc + remarks);
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
        public JsonResult GetDNInvoice(long DrId)
        {
            var ConD = (from a in db.DrInvoices
                        join b in db.DrNotes on a.DrNoteId equals b.DrNoteId
                        where a.DrNoteId == DrId
                        select new
                        {
                            a.DebitAmount,
                            b.DebitType,
                            a.TransType,
                            a.Id,
                            EntryNo = a.EntryNo,
                            b.SubTotal,
                            b.TaxAmount,
                            b.GrandTotal,
                            CRGTotal = b.GrandTotal,
                            TaxPer = b.TaxPer,
                        }).Select(o => new
                        {
                            DebitAmount = o.DebitAmount,
                            CNType = o.DebitType,
                            InvoiceNo = o.Id,
                            EntryNo = o.EntryNo,
                            SubTotal = o.SubTotal,
                            TaxAmount = o.TaxAmount,
                            GrandTotal = o.GrandTotal,
                            CRGTotal = o.CRGTotal,
                            TaxPer = o.TaxPer,
                            TransType = o.TransType,
                            Purchase = (from z in db.PurchaseEntrys
                                     join y in db.PEPayments on z.PurchaseEntryId equals y.PurchaseEntry
                                     where z.PurchaseEntryId == o.EntryNo
                                     select new
                                     {
                                         SEBill = z.BillNo,
                                         SDate = z.PEDate,
                                         DebitAmount = y.DebitAmount,
                                         GrandTotalS = z.PEGrandTotal,
                                         PaidAmountS = y.PEPaidAmount,
                                     }).FirstOrDefault(),

                        }).AsEnumerable().Select(o => new
                        {
                            o.InvoiceNo,
                            o.EntryNo,
                            o.CNType,
                            o.SubTotal,
                            o.TaxAmount,
                            o.GrandTotal,
                            o.CRGTotal,
                            o.TaxPer,
                            o.DebitAmount,
                            o.TransType,
                            SEBill = o.Purchase.SEBill,
                            InvoiceDate = o.Purchase.SDate,
                            PaidAmountS = o.Purchase.PaidAmountS,
                            GrandTotalS = o.Purchase.GrandTotalS,
                            DebitAmounts = o.Purchase.DebitAmount,
                        }).ToList();

            return Json(ConD);
        }


        private long GetCNNo()
        {
            Int64 CNNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "DrNote").Select(a => a.number).FirstOrDefault();
            if ((db.DrNotes.Select(p => p.DNNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    CNNo = 1;
                }
                else
                {
                    CNNo = number;
                }
            }
            else
            {
                CNNo = db.DrNotes.Max(p => p.DNNo + 1);
            }

            return CNNo;
        }

        private string InvoiceNo(Int64 CNNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "DNote").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "DNote").Select(a => a.number).FirstOrDefault();
                if ((db.DrNotes.Select(p => p.DNNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    CNNo = db.DrNotes.Max(p => p.DNNo + 1);
                    billNo = companyPrefix + CNNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(CNNo, billNo);
                    }
                }
            }
            else
            {
                CNNo = CNNo + 1;
                billNo = companyPrefix + CNNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(CNNo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string CNNo)
        {
            var Exists = db.DrNotes.Any(c => c.BillNo == CNNo);
            bool res = (Exists) ? true : false;
            return res;
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
