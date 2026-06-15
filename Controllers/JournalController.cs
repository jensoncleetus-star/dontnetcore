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
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class JournalController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public JournalController()
        {
            db = new ApplicationDbContext();
            com = new Common();
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
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            _FinancialYear();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Journal List")]
        public JsonResult GetData(string InvoiceNo, string FromDate, string ToDate, long? PayFrom, long? PayTo, string user)
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
            var userpermission = User.IsInRole("All Journal Entry");
            var UserId = User.Identity.GetUserId();

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Journal");
            var uDownload = User.IsInRole("Download Journal");
            var uDelete = User.IsInRole("Delete Journal");

            var v = (from a in db.Journals
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                     from c in payto.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     where (a.editable == choice.Yes)
                     && ((InvoiceNo == null || InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                   (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                   (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                    (PayFrom == 0 || PayFrom == null || a.PayFrom == PayFrom) &&
                   (PayTo == 0 || PayTo == null || a.PayTo == PayTo) ) && (user == null || user == "" || g.Id == user)
                   && (userpermission == true || a.CreatedBy == UserId)
                     select new
                     {
                         VoucherNo = a.VoucherNo,
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
                         a.CreatedDate
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
                         Download = uDownload,
                         Delete = uDelete,
                         o.CreatedDate
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
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // create 
        [QkAuthorize(Roles = "Dev,Create Journal")]
        public ActionResult Create()
        {
            var list = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "Search Accounts", Value = ""},
                             }, "Value", "Text", 1);
            ViewBag.list = list;

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
            //companyinfo
            companySet();
            var userpermission = User.IsInRole("All Journal Entry");
            var UserId = User.Identity.GetUserId();
            ViewBag.LastEntry = db.Journals.Where(p => (p.editable == choice.Yes) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.JournalId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            _FinancialYear();
            return View(Journal);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Journal")]
        public JsonResult Create(JournalViewModel vmodel)
        {
            string msg;
            bool stat;
            if (!journalBillExist(Convert.ToString(vmodel.VoucherNo)))
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

                        var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                        var Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                        var today = Convert.ToDateTime(System.DateTime.Now);
                        var jornNo = JournalMaxvoucher();

                        Journal JOR = new Journal
                        {
                            Voucher = jornNo,
                            VoucherNo = vmodel.VoucherNo,
                            Date = Date,
                            PayFrom = vmodel.PayFrom,
                            PayTo = vmodel.PayTo,
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
                            RefType = "Journal"
                        };
                        db.Journals.Add(JOR);
                        db.SaveChanges();
                        Int64 JournalId = JOR.JournalId;
                        com.addAccountTrasaction(0, Convert.ToDecimal(vmodel.Paying), vmodel.PayFrom, "Journal", JournalId, DC.Credit, Date);
                        com.addAccountTrasaction(Convert.ToDecimal(vmodel.Paying), 0, vmodel.PayTo, "Journal", JournalId, DC.Debit, Date);

                        com.addlog(LogTypes.Created, UserId, "Journal", "Journals", findip(), JournalId, "Successfully added Journal details");                        

                        if (vmodel.submittype == "print")
                        {
                            vmodel.creditor = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
                            vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
                        }
                        msg = "Successfully Created Journal details.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, type = vmodel.submittype, message = msg } };
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
        }


        [QkAuthorize(Roles = "Dev,Edit Journal")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Journal Entry");
            var UserId = User.Identity.GetUserId();
            Journal rpt = db.Journals.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.JournalId== id).FirstOrDefault();
            if (rpt == null)
            {
                return NotFound();
            }
            var Journal = new JournalViewModel
            {
                VoucherNo = rpt.VoucherNo,
                Date = (rpt.Date).ToString("dd-MM-yyyy"),
                PayFrom = rpt.PayFrom,
                PayTo = rpt.PayTo,
                Remark = rpt.Remark,
                GrandTotal = rpt.GrandTotal,
                Balance = rpt.Balance,
                Paying = rpt.Paying,
                Branch = rpt.Branch,
            };
            var PaidTo = db.Accountss.Where(a => a.AccountsID == Journal.PayTo && a.Group != 23).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

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

            var PaidFr = db.Accountss.Where(a => a.AccountsID == Journal.PayFrom).
            Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(PaidFr, "ID", "Name");

            //companyinfo
            companySet();
            
            ViewBag.preEntry = db.Journals.Where(a => a.JournalId < id && a.editable == choice.Yes && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.JournalId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Journals.Where(a => a.JournalId > id && a.editable == choice.Yes && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.JournalId).DefaultIfEmpty().Min();


            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            _FinancialYear();
            var VoucherEdit = db.EnableSettings.Where(a => a.EnableType == "EnableVoucherEdit").FirstOrDefault();
            ViewBag.EditVoucher = VoucherEdit != null ? EnableBranch.Status : Status.inactive;
            return View(Journal);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Journal")]
        public ActionResult Edit(JournalViewModel vmodel, long id)
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

                        Journal rec = db.Journals.Find(id);
                        Journal RecTemp = rec;
                        var today = Convert.ToDateTime(System.DateTime.Now);
                        var Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                        

                        //----------------------------------------------------------------------------
                        var JournalId = rec.JournalId;
                        bool delete = com.DeleteAllAccountTransaction("Journal", rec.JournalId);
                        rec.Date = Date;
                        rec.PayTo = vmodel.PayTo;
                        rec.Paying = (decimal)vmodel.Paying;
                        rec.GrandTotal = (decimal)vmodel.Paying;
                        rec.Balance = vmodel.Balance;
                        rec.Remark = vmodel.Remark;
                        rec.Branch = Branch;
                        db.Entry(rec).State = EntityState.Modified;
                        db.SaveChanges();                        


                        com.addAccountTrasaction(0, Convert.ToDecimal(vmodel.Paying), vmodel.PayFrom, "Journal", JournalId, DC.Credit, Date);
                        com.addAccountTrasaction(Convert.ToDecimal(vmodel.Paying), 0, vmodel.PayTo, "Journal", JournalId, DC.Debit, Date);
                        com.addlog(LogTypes.Updated, UserId, "Journal", "Journals", findip(), rec.JournalId, "Journal Updated Successfully");
                        if (vmodel.submittype == "print")
                        {
                            vmodel.creditor = db.Accountss.Where(a => a.AccountsID == rec.PayFrom).Select(a => a.Name).FirstOrDefault();
                            vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
                        }
                        msg = "Successfully Updated Journal details.";
                        stat = true;
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, message = msg, type = vmodel.submittype } };
        }


        [QkAuthorize(Roles = "Dev,Delete Journal")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Journal Entry");
            var UserId = User.Identity.GetUserId();
            Journal rec = db.Journals.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.JournalId == id).FirstOrDefault();
            
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
            return RedirectToAction("Index", "Journal");
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
                db.Journals.Remove(Rec);

                bool delete = com.DeleteAllAccountTransaction("Journal", JrId);
                db.SaveChanges();
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

            var Data = db.Journals.Where(s => s.JournalId == id).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.VoucherNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            return File(ms, "application/pdf", "Journal Voucher" + "-" + billno + ".pdf");



        }

        public StringBuilder generatePdf(long id)
        {           

            var details = (from a in db.Journals
                           join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                           from b in payfrom.DefaultIfEmpty()
                           join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                           from c in payto.DefaultIfEmpty()
                           where a.JournalId == id
                           select new
                           {
                               VoucherNo = a.VoucherNo,
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
                               //HFStatus = ComHeadCheck
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
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Journal Voucher</b></td></tr></table>");

                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><td style='font-size:14px;font-weight:normal;'><b>Invoice No </b>: " + details.VoucherNo + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'><b>Date</b> : " + details.Date.ToString("dd-MM-yyyy") + "</td></tr> </table></td></tr></table>";
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

                    sb.Append("<tr style='font-size:10px;'>");
                    {
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>" + SI++ + "</b></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>Dr " + details.Reciever + "</td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + details.GrandTotal + "</td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>");
                    }
                    sb.Append("</tr>");

                    sb.Append("<tr style='font-size:10px;'>");
                    {
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>Total</b></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + details.GrandTotal + "</b></td>");
                        sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + details.GrandTotal + "</b></td>");
                    }

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
                    billNo = (number == 0)? (prefix + 1) :(prefix + number);
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
            if (recid != null)
            {
                var Exists = db.Journals.Any(c => c.VoucherNo == SENo);
                bool res = (Exists) ? true : false;
                return res;
            }
            else
            {
                var Exists = db.Journals.Where(a => a.JournalId != recid).Any(c => c.VoucherNo == SENo);
                bool res = (Exists) ? true : false;
                return res;
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
