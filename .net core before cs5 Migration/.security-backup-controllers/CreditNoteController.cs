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
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.ViewModel;
using System.Net;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Text;
using System.IO;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class CreditNoteController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public CreditNoteController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
         
        // GET: CreditNote
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,CreditNote List")]
        public ActionResult Index()
        {
            //    ID = r.AccountsID,
            //    Name = r.Name

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

        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,CreditNote List")]
        public ActionResult GetCreditNote(string InvoiceNo, string FromDate, string ToDate, long? customer, long? crtype, long? PayFrom, long? PayTo)
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

            crtype = crtype == 0 ? null : crtype;
            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var v = (from a in db.CreditNotes
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                     from c in payto.DefaultIfEmpty()
                     join d in db.Users on a.CreatedBy equals d.Id
                     where
                    (InvoiceNo == "" || a.BillNo == InvoiceNo) &&
                    (FromDate == "" || EF.Functions.DateDiffDay(a.CNDate, fdate) <= 0) &&
                    (ToDate == "" || EF.Functions.DateDiffDay(a.CNDate, tdate) >= 0) &&
                    (PayFrom == null || PayFrom == 0 || a.PayFrom == PayFrom) &&
                    (PayTo == null || PayTo == 0 || a.PayTo == PayTo) &&
                    (crtype == -1 || a.CreditType == crtype) 
                     select new
                     {
                         a.CreditnoteId,
                         BillNo = a.BillNo,
                         a.CNDate,
                         a.GrandTotal,
                         PayFrom = b.Name,
                         PayTo = c.Name,
                         User = d.UserName,
                         CreditType = a.CreditType,
                         Amount = a.GrandTotal,
                         a.CreatedDate,
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.PayTo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.PayFrom.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.CreditnoteId.ToString().ToLower().Contains(search.ToLower())
                                 );
            }

            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,CreditNote Entry")]
        [HttpGet]
        public ActionResult Create()
        {
            var PaidTo = db.Accountss.Where(p => p.AccountsID != 499).Select(r => new //Except Stock adjustment Expense
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            ViewBag.Paidfrom = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1);

            var crnote = new CreditNoteViewModel
            {
                BillNo = InvoiceNo(),
                CNDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                CNNote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "creditnote").Select(a => a.TermsCondit).FirstOrDefault(),
                Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList()
            };

            ViewBag.LastEntry = db.CreditNotes.Select(p => p.CreditnoteId).AsEnumerable().DefaultIfEmpty(0).Max();
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            //enable bill adjustment
            var billAdj = db.EnableSettings.Where(a => a.EnableType == "CNBillAdjust").FirstOrDefault();
            var CNBillAdj = billAdj != null ? (billAdj.Status == Status.active ? 0 : 1) : 1;
            ViewBag.CNBillAdj = CNBillAdj;

            companySet();
            return View(crnote);
        }

        //[RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,CreditNote Entry")]
        public JsonResult CreateCreditNote(string[][] array, string[] cndata)
        {
            bool stat = false;
            string msg;
            string action = cndata[12];
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            CreditNote CNentry = new CreditNote();

            CNentry.CNNo = GetCNNo();
            CNentry.BillNo = Convert.ToString(cndata[0]);
            CNentry.CreditType = Convert.ToInt32(cndata[2]);

            CNentry.CNDate = DateTime.Parse(cndata[1].ToString(), new CultureInfo("en-GB"));
            CNentry.PayFrom = Convert.ToInt64(cndata[3]);
            CNentry.PayTo = Convert.ToInt64(cndata[4]);

            CNentry.CNNote = Convert.ToString(cndata[5]);
            CNentry.Remarks = Convert.ToString(cndata[6]);

            CNentry.SubTotal = Convert.ToDecimal(cndata[7]);
            CNentry.Tax = Convert.ToInt64(cndata[8]);
            CNentry.TaxPer = Convert.ToDecimal(cndata[9]);
            CNentry.TaxAmount = Convert.ToDecimal(cndata[10]);
            CNentry.GrandTotal = Convert.ToDecimal(cndata[11]);

            CNentry.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
            CNentry.CreatedBy = UserId;
            CNentry.Status = 1;
            CNentry.Branch = Convert.ToInt64(BranchID);

            db.CreditNotes.Add(CNentry);
            db.SaveChanges();
            Int64 CNId = CNentry.CreditnoteId;

            var paying = Convert.ToDecimal(cndata[11]);//grand total
            //enable bill adjustment
            var billAdj = db.EnableSettings.Where(a => a.EnableType == "CNBillAdjust").FirstOrDefault();
            var CNBillAdj = billAdj != null ? (billAdj.Status == Status.active ? 0 : 1) : 1;
            if (CNBillAdj == 0)
            {
                if (array != null)
                {
                    updateinvoicebase(array, CNId);
                }
                if (CNentry.GrandTotal > 0)
                {
                    var payings = CNentry.GrandTotal;
                    addtransaction(CNId, payings);
                }
            }
            else
            {
                addtransaction(CNId, paying);
            }
            var TaxAmount = Convert.ToDecimal(cndata[10]);
            var GrandTotal = Convert.ToDecimal(cndata[11]);
            decimal payamount = Convert.ToDecimal(cndata[7]);
            Int64 payfromAccID = db.Customers.Where(a => a.CustomerID == CNentry.PayFrom).Select(a => a.Accounts).FirstOrDefault();
            Int64 paytoAccID = db.Customers.Where(a => a.CustomerID == CNentry.PayTo).Select(a => a.Accounts).FirstOrDefault();
            Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).FirstOrDefault();
            var date = DateTime.Parse(cndata[1], new CultureInfo("en-GB"));
            //credit note account

            //add trasaction to credit note account
            com.addAccountTrasaction(payamount, 0, paytoAccID, "CreditNote", CNId, DC.Debit, date);
            //add sale trasaction 
            com.addAccountTrasaction(0, GrandTotal, payfromAccID, "CreditNote", CNId, DC.Credit, date);


            //add vat input in account transaction
            if (TaxAmount > 0)
            {
                com.addAccountTrasaction(TaxAmount, 0, VATOutput, "CreditNote", CNId, DC.Debit, date);
            }

            com.addlog(LogTypes.Created, UserId, "CreditNote", "CreditNotes", findip(), CNId, "Successfully Submitted Credit Note");

            if (action == "print")
            {
                var summary = (from a in db.CreditNotes
                               join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                               from b in payfrom.DefaultIfEmpty()
                               join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                               from c in payto.DefaultIfEmpty()
                               where a.CreditnoteId == CNId
                               select new
                               {
                                   Date = a.CNDate,
                                   VoucherNo = a.BillNo,
                                   PayFrom = b.Name,
                                   PayTo = c.Name,
                                   CrAmt = a.GrandTotal
                               }).FirstOrDefault();


                msg = "Successfully submitted Cr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, summary = summary, CNId = CNId } };
            }
            else
            {
                msg = "Successfully submitted Cr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, CNId = CNId } };
            }

        }

        //[HttpPost]
        //    //if (ModelState.IsValid)

        //                       where a.CreditnoteId == cnId
        //                           Date = a.CNDate,
        //                           VoucherNo = a.BillNo,
        //                           PayFrom = b.Name,
        //                           PayTo = c.Name,
        //                           CrAmt = a.GrandTotal


        //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, summary = summary } };
        //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        //    //else
        //    //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };


        [HttpPost]
        public JsonResult UpdatePayTransEdit(string[][] array, long cnId, string action)
        {
            bool stat = false;
            string msg;
            if (array != null)
            {
                var CNInvoice = db.CNInvoices.Where(a => a.CreditnoteId == cnId).FirstOrDefault();
                if (CNInvoice != null)
                {
                    db.CNInvoices.RemoveRange(db.CNInvoices.Where(a => a.CreditnoteId == cnId));
                }
                deletetransaction(cnId);
                updateinvoicebase(array, cnId);
            }
            CreditNote CNentry = db.CreditNotes.Find(cnId);
            if (CNentry.GrandTotal > 0)
            {
                var Custid = (from a in db.Customers where a.Accounts == CNentry.PayFrom select new { a.CustomerID }).SingleOrDefault();
                if (Custid != null)
                {
                    var paying = CNentry.GrandTotal;
                    deletetransaction(cnId);
                    addtransaction(cnId, paying);
                }
            }
            if (action == "print")
            {
                var summary = (from a in db.CreditNotes
                               join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                               from b in payfrom.DefaultIfEmpty()
                               join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                               from c in payto.DefaultIfEmpty()
                               where a.CreditnoteId == cnId
                               select new
                               {
                                   Date = a.CNDate,
                                   VoucherNo = a.BillNo,
                                   PayFrom = b.Name,
                                   PayTo = c.Name,
                                   CrAmt = a.GrandTotal
                               }).FirstOrDefault();


                msg = "Successfully submitted Cr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, summary = summary } };
            }
            else
            {
                msg = "Successfully submitted Cr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit CreditNote")]
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNote creditnote = db.CreditNotes.Find(id);

            if (creditnote == null)
            {
                return NotFound();
            }

            var PaidTo = db.Accountss.Where(p => p.AccountsID != 499).Select(r => new //Except Stock adjustment Expense
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");



            CreditNoteViewModel vmodel = new CreditNoteViewModel();
            vmodel = (from a in db.CreditNotes
                      join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                      from b in payfrom.DefaultIfEmpty()
                      where a.CreditnoteId == id
                      select new CreditNoteViewModel
                      {
                          CNDate = a.CNDate,
                          BillNo = a.BillNo,

                          CreditnoteId = a.CreditnoteId,
                          PayFrom = a.PayFrom,
                          PayTo = a.PayTo,

                          SubTotal = a.SubTotal,
                          Tax = a.Tax,
                          TaxPer = a.TaxPer,
                          TaxAmount = a.TaxAmount,
                          GrandTotal = a.GrandTotal,

                          Remarks = a.Remarks,
                          CNNote = a.CNNote,
                          CreditType = a.CreditType,

                          Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList()
                      }).FirstOrDefault();

            //---emailid from supp/cust
            var accId = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).FirstOrDefault();
            if (accId.Group == 12)//customer
            {
                var cust = (from a in db.Customers
                            join b in db.Contacts on a.Contact equals b.ContactID into cont
                            from b in cont.DefaultIfEmpty()
                            where a.Accounts == accId.AccountsID
                            select new
                            {
                                b.EmailId
                            }).FirstOrDefault();
                vmodel.custEmailId = cust.EmailId;
            }
            else if (accId.Group == 14)//supplier
            {
                var supp = (from a in db.Suppliers
                            join b in db.Contacts on a.Contact equals b.ContactID into cont
                            from b in cont.DefaultIfEmpty()
                            where a.Accounts == accId.AccountsID
                            select new
                            {
                                b.EmailId
                            }).FirstOrDefault();
                vmodel.custEmailId = supp.EmailId;
            }
            else
            {
                vmodel.custEmailId = "";
            }

            var PaidFr = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).
               Select(r => new
               {
                   ID = r.AccountsID,
                   Name = r.Name
               }).ToList();
            ViewBag.Paidfrom = QkSelect.List(PaidFr, "ID", "Name");


            ViewBag.preEntry = db.CreditNotes.Where(a => a.CreditnoteId < id).Select(a => a.CreditnoteId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.CreditNotes.Where(a => a.CreditnoteId > id).Select(a => a.CreditnoteId).DefaultIfEmpty().Min();

            //enable bill adjustment
            var billAdj = db.EnableSettings.Where(a => a.EnableType == "CNBillAdjust").FirstOrDefault();
            var CNBillAdj = billAdj != null ? (billAdj.Status == Status.active ? 0 : 1) : 1;
            ViewBag.CNBillAdj = CNBillAdj;

            companySet();
            return View(vmodel);
        }

        //[RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Edit CreditNote")]
        public ActionResult UpdateCreditNote(string[][] array, string[] cndata)
        {
            bool stat = false;
            string msg;
            string action = cndata[12];
            Int64 CNId = Convert.ToInt64(cndata[13]);
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            CreditNote CNentry = db.CreditNotes.Find(CNId);

            CNentry.CNNo = GetCNNo();
            CNentry.BillNo = Convert.ToString(cndata[0]);
            CNentry.CreditType = Convert.ToInt32(cndata[2]);

            CNentry.CNDate = DateTime.Parse(cndata[1].ToString(), new CultureInfo("en-GB"));
            CNentry.PayFrom = Convert.ToInt64(cndata[3]);
            CNentry.PayTo = Convert.ToInt64(cndata[4]);

            CNentry.CNNote = Convert.ToString(cndata[5]);
            CNentry.Remarks = Convert.ToString(cndata[6]);
           

            CNentry.SubTotal = Convert.ToDecimal(cndata[7]);
            CNentry.Tax = Convert.ToInt64(cndata[8]);
            CNentry.TaxPer = Convert.ToDecimal(cndata[9]);
            CNentry.TaxAmount = Convert.ToDecimal(cndata[10]);
            CNentry.GrandTotal = Convert.ToDecimal(cndata[11]);


            db.Entry(CNentry).State = EntityState.Modified;
            db.SaveChanges();

            var paying = Convert.ToDecimal(cndata[11]);//grand total
            //enable bill adjustment
            var billAdj = db.EnableSettings.Where(a => a.EnableType == "CNBillAdjust").FirstOrDefault();
            var CNBillAdj = billAdj != null ? (billAdj.Status == Status.active ? 0 : 1) : 1;
            if (CNBillAdj == 0)
            {

            }
            else
            {
                var Custid = (from a in db.Customers where a.Accounts == CNentry.PayFrom select new { a.CustomerID }).SingleOrDefault();
                if (Custid != null)
                {
                    deletetransaction(CNId);
                    addtransaction(CNId, paying);
                }
            }

            var TaxAmount = Convert.ToDecimal(cndata[10]);
            var GrandTotal = Convert.ToDecimal(cndata[11]);
            decimal payamount = Convert.ToDecimal(cndata[7]);
            Int64 payfromAccID = db.Customers.Where(a => a.CustomerID == CNentry.PayFrom).Select(a => a.Accounts).FirstOrDefault();
            Int64 paytoAccID = db.Customers.Where(a => a.CustomerID == CNentry.PayTo).Select(a => a.Accounts).FirstOrDefault();
            Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).FirstOrDefault();
            var date = DateTime.Parse(cndata[1], new CultureInfo("en-GB"));
            //credit note account

            //delete
            bool delete = com.DeleteAllAccountTransaction("CreditNote", CNId);

            //add trasaction to credit note account
            com.addAccountTrasaction(payamount, 0, paytoAccID, "CreditNote", CNId, DC.Debit, date);
            //add sale trasaction 
            com.addAccountTrasaction(0, GrandTotal, payfromAccID, "CreditNote", CNId, DC.Credit, date);


            //add vat input in account transaction
            if (TaxAmount > 0)
            {
                com.addAccountTrasaction(TaxAmount, 0, VATOutput, "CreditNote", CNId, DC.Debit, date);
            }

            com.addlog(LogTypes.Created, UserId, "CreditNote", "CreditNotes", findip(), CNId, "Successfully Submitted Credit Note");

            if (action == "print")
            {
                var summary = (from a in db.CreditNotes
                               join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                               from b in payfrom.DefaultIfEmpty()
                               join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                               from c in payto.DefaultIfEmpty()
                               where a.CreditnoteId == CNId
                               select new
                               {
                                   Date = a.CNDate,
                                   VoucherNo = a.BillNo,
                                   PayFrom = b.Name,
                                   PayTo = c.Name,
                                   CrAmt = a.GrandTotal
                               }).FirstOrDefault();


                msg = "Successfully submitted Cr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, summary = summary, CNId = CNId } };
            }
            else
            {
                msg = "Successfully submitted Cr.Note.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, CNId = CNId } };
            }
        }

        //        //delete
        //                    where a.CustomerId == Custid.CustomerID && a.Recieptid == CNId && a.type == "CreditNote"
        //                    orderby a.SETransactionId
        //                        a.SalesEntry,
        //                        a.SEPayAmount

        public void deletetransaction(long CNId)
        {
            CreditNote CNote = db.CreditNotes.Find(CNId);
            var Custid = (from a in db.Customers where a.Accounts == CNote.PayFrom select new { a.CustomerID }).SingleOrDefault();
            if (Custid != null)
            {
                //delete
                var data = (from a in db.SETransactions
                            where a.CustomerId == Custid.CustomerID && a.Recieptid == CNId && a.type == "CreditNote"
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
                        SEP.CreditAmount = SEP.CreditAmount - paying;
                        db.Entry(SEP).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == CNId && a.type == "CreditNote"));
                    db.SaveChanges();
                }
            }
            //                where a.SupplierId == Dsupplierid.SupplierID && a.PaymentId == CNId && a.type == "CreditNote"
            //                orderby a.PETransactionId
            //                    a.PurchaseEntry,
            //                    a.PEPayAmount
        }


        public void addtransaction(long CNId, decimal paying)
        {
            CreditNote CNentry = db.CreditNotes.Find(CNId);
            //add
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var Custid = (from a in db.Customers where a.Accounts == CNentry.PayFrom select new { a.CustomerID }).SingleOrDefault();
            if (Custid != null)
            {
                var v = (from a in db.SalesEntrys
                         join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                         where a.Customer == Custid.CustomerID && (c.SEPaidAmount + c.CreditAmount) != c.SEBillAmount
                         orderby a.SalesEntryId
                         select new
                         {
                             invoiceno = a.BillNo,
                             Date = a.SEDate,
                             total = a.SEGrandTotal,
                             paid = c.SEPaidAmount,
                             sid = a.SalesEntryId
                         });
                var dataa = v.ToList();
                if (dataa.Count > 0)
                {
                    foreach (var ditem in dataa)
                    {
                        if (paying > 0)
                        {
                            SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.sid).FirstOrDefault();
                            //add to petransactions
                            SETransaction SEPT = new SETransaction();
                            SEPT.SalesEntry = SEP.SalesEntry;
                            SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                            SEPT.SEPayDate = CNentry.CNDate;
                            SEPT.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                            SEPT.CreatedUserId = UserId;
                            SEPT.Status = 0;
                            SEPT.Recieptid = CNId;
                            SEPT.type = "CreditNote";
                            // transaction 
                            var balnceamount = SEP.SEBillAmount - (SEP.SEPaidAmount + SEP.CreditAmount);
                            if (balnceamount >= paying)
                            {
                                SEP.CreditAmount = SEP.CreditAmount + Convert.ToDecimal(paying);
                                SEPT.SEPayAmount = Convert.ToDecimal(paying);
                                paying = 0;
                            }
                            else
                            {
                                SEP.CreditAmount = SEP.CreditAmount + Convert.ToDecimal(balnceamount);
                                SEPT.SEPayAmount = Convert.ToDecimal(balnceamount);
                                paying -= Convert.ToDecimal(balnceamount);
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
            // check is that account is a supplier account and its have pending purchase payment bills
            //    //based on checkbox selection 
            //             where a.Supplier == supplierid.SupplierID && (c.PEPaidAmount + c.CreditAmount) != c.PEBillAmount
            //             orderby a.PurchaseEntryId
            //                 invoiceno = a.BillNo,
            //                 Date = a.PEDate,
            //                 total = a.PEGrandTotal,
            //                 paid = c.PEPaidAmount,
            //                 pid = a.PurchaseEntryId
            //                //add to petransactions
            //                // transaction 
            //                // update transaction


        }

        public void updateinvoicebase(string[][] array, long cnId)
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            CNInvoice cninvoice = new CNInvoice();
            foreach (var arr in array)
            {
                var cramt = Convert.ToDecimal(arr[1]);
                if (arr[0] != null && arr[0] != "" && cramt > 0)
                {
                    var entrynos = Convert.ToInt64(arr[2]);

                    cninvoice.CreditnoteId = cnId;
                    cninvoice.EntryNo = entrynos;
                    cninvoice.TransType = Convert.ToString(arr[3]);
                    cninvoice.CreditAmount = Convert.ToDecimal(arr[1]);

                    db.CNInvoices.Add(cninvoice);
                    db.SaveChanges();

                    var sinvoice = db.SalesEntrys.Where(a => a.SalesEntryId == entrynos).FirstOrDefault();
                    var spyid = db.SEPayments.Where(a => a.SalesEntry == entrynos).FirstOrDefault();
                    decimal crAmount = Convert.ToDecimal(arr[1]);

                    //sale payment
                    SEPayment spay = db.SEPayments.Find(spyid.SEPaymentId);
                    spay.CreditAmount = (spay.CreditAmount != null ? spay.CreditAmount : 0) + crAmount;
                    db.Entry(spay).State = EntityState.Modified;
                    db.SaveChanges();

                    //add hire transacvtion
                    SETransaction SEtran = new SETransaction();
                    SEtran.SalesEntry = sinvoice.SalesEntryId;
                    SEtran.CustomerId = sinvoice.Customer;
                    SEtran.SEPayDate = sinvoice.SEDate;
                    SEtran.SEPayAmount = crAmount;
                    SEtran.Recieptid = cnId;
                    SEtran.type = "CreditNote";
                    SEtran.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    SEtran.CreatedUserId = UserId;
                    SEtran.Status = 1;
                    SEtran.CreatedBranch = Convert.ToInt64(BranchID);

                    db.SETransactions.Add(SEtran);
                    db.SaveChanges();
                }
            }
        }





        //            //sale payment

        //            //add hire transacvtion
        //            SEtran.CustomerId = Convert.ToInt64(cndata[3]);//payfrom




        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Delete CreditNote")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CreditNote cnote = db.CreditNotes.Find(id);
            if (cnote == null)
            {
                return NotFound();
            }
            return PartialView(cnote);
        }

        //[RedirectingAction]
        [HttpPost, ActionName("Delete")]
        //[QkAuthorize(Roles = "Dev,Delete CreditNote")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var chk = DeleteAll(id);
            if(chk == true)
            {
                stat = true;
                msg = "Successfully deleted Cr.Note.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }
            #region Old Code
            #endregion

            
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete CreditNote")]
        public ActionResult DeleteAllCreditNote(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteAll(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Cr.Note Entry.", true);
            return RedirectToAction("Index", "CreditNote");
        }

        private Boolean DeleteAll(long sId)
        {
            var UserId = User.Identity.GetUserId();
            CreditNote SErt = db.CreditNotes.Find(sId);
            var CNinvoice = db.CNInvoices.Where(a => a.CreditnoteId == sId).ToList();
            var customerId = db.Customers.Where(a => a.Accounts == SErt.PayFrom).Select(a => a.CustomerID).First();

            if (CNinvoice != null)
            {
                db.CNInvoices.RemoveRange(db.CNInvoices.Where(a => a.CreditnoteId == sId));
            }

            deletetransaction(sId);
            bool delete = com.DeleteAllAccountTransaction("CreditNote", sId);

            db.CreditNotes.Remove(SErt);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "CreditNote", "CreditNotes", findip(), SErt.CreditnoteId, "Successfully Deleted Credit Note");
            
            return true;
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Download CreditNote")]
        public ActionResult Download(long id)
        {
            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = db.CreditNotes.Where(s => s.CreditnoteId == id).Select(s => s.BillNo).FirstOrDefault();

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", cname + "_CrNote_" + billno + "_" + System.DateTime.Now.ToShortDateString() + ".pdf");

        }
        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.CreditNotes
                           join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                           from b in payfrom.DefaultIfEmpty()
                           join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                           from c in payto.DefaultIfEmpty()
                           where a.CreditnoteId == id
                           select new
                           {
                               Date = a.CNDate,
                               a.BillNo,
                               PayFrom = b.Name,
                               PayTo = c.Name,
                               a.GrandTotal,
                               a.Remarks,
                               a.CNNote

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

                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Credit Note</b></td></tr></table>");
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
                    sb.Append("<th style='border: .5px #000000;padding: 5px;vertical-align: top;text-align: center;border: 1px solid #ccc;'>Credit Amount</th>");
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

                    var cnote = "";
                    if (details.CNNote != null)
                    {
                        cnote = details.CNNote.Replace("\n", "<br />");
                    }
                    string tc = "<tr class='border-top'><td colspan='3' style='border: .1px solid #ccc;padding: 10px;' ><strong><u> Terms And Conditions :</u></strong><br/>" + cnote + " </td></tr>";

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
        public JsonResult GetCNInvoice(long CrId)
        {
            var ConD = (from a in db.CNInvoices
                        join b in db.CreditNotes on a.CreditnoteId equals b.CreditnoteId
                        where a.CreditnoteId == CrId
                        select new
                        {
                            a.CreditAmount,
                            b.CreditType,
                            a.TransType,
                            a.CNInvoiceId,
                            EntryNo = a.EntryNo,
                            b.SubTotal,
                            b.TaxAmount,
                            b.GrandTotal,
                            CRGTotal = b.GrandTotal,
                            TaxPer = b.TaxPer,
                        }).Select(o => new
                        {
                            CreditAmount = o.CreditAmount,
                            CNType = o.CreditType,
                            InvoiceNo = o.CNInvoiceId,
                            EntryNo = o.EntryNo,
                            SubTotal = o.SubTotal,
                            TaxAmount = o.TaxAmount,
                            GrandTotal = o.GrandTotal,
                            CRGTotal = o.CRGTotal,
                            TaxPer = o.TaxPer,
                            TransType = o.TransType,
                            Sales = (from z in db.SalesEntrys
                                     join y in db.SEPayments on z.SalesEntryId equals y.SalesEntry
                                     where z.SalesEntryId == o.EntryNo
                                     select new
                                     {
                                         SEBill = z.BillNo,
                                         SDate = z.SEDate,
                                         CreditAmount = y.CreditAmount,
                                         GrandTotalS = z.SEGrandTotal,
                                         PaidAmountS = y.SEPaidAmount,
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
                            o.CreditAmount,
                            o.TransType,
                            SEBill = o.Sales.SEBill,
                            InvoiceDate = o.Sales.SDate,
                            PaidAmountS = o.Sales.PaidAmountS,
                            GrandTotalS = o.Sales.GrandTotalS,
                            CreditAmounts = o.Sales.CreditAmount,
                        }).ToList();

            return Json(ConD);
        }


        private long GetCNNo()
        {
            Int64 CNNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "CreditNote").Select(a => a.number).FirstOrDefault();
            if ((db.CreditNotes.Select(p => p.CNNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                CNNo = db.CreditNotes.Max(p => p.CNNo + 1);
            }

            return CNNo;
        }

        private string InvoiceNo(Int64 CNNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "CNote").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "CNote").Select(a => a.number).FirstOrDefault();
                if ((db.CreditNotes.Select(p => p.CNNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    CNNo = db.CreditNotes.Max(p => p.CNNo + 1);
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
            var Exists = db.CreditNotes.Any(c => c.BillNo == CNNo);
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
