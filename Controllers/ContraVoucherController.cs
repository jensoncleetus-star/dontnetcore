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
    public class ContraVoucherController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ContraVoucherController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }


        // GET: ContraVoucher
        [QkAuthorize(Roles = "Dev,ContraVoucher List")]
        public ActionResult Index()
        {
            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PayTo = QkSelect.List(PaidTo, "ID", "Name");
            ViewBag.PayFrom = QkSelect.List(PaidTo, "ID", "Name");

            return View();
        }
        // create 
        [QkAuthorize(Roles = "Dev,Create ContraVoucher")]
        public ActionResult Create()
        {
            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            ViewBag.Paidfrom = QkSelect.List(PaidTo, "ID", "Name");

            var ContraVoucher = new ContraVoucherViewModel
            {
                VoucherNo = VoucherNo(),
                Date = (System.DateTime.Now).ToString("dd-MM-yyyy")
            };

            //Warning before Save
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

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

            //companyinfo
            companySet();
            ViewBag.LastEntry = db.ContraVouchers.Where(p => p.editable == choice.Yes).Select(p => p.ContraVoucherId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            ContraVoucher.FieldMap = db.FieldMappings.Where(a => a.Section == "CVoucher" && a.Status == Status.active).ToList();
            _FinancialYear();
            return View(ContraVoucher);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create ContraVoucher")]
        public JsonResult Create(ContraVoucherViewModel vmodel)
        {
            string msg;
            bool stat;
            if (!com.recBillExist(Convert.ToString(vmodel.VoucherNo)))
            {
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
                            Branch = vmodel.Branch;
                        }
                        else
                        {
                            Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                        }

                        var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                        var Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));

                        var today = Convert.ToDateTime(System.DateTime.Now);
                        long recNo = Maxvoucher();
                        ContraVoucher CV = new ContraVoucher
                        {
                            Voucher = recNo,
                            VoucherNo = vmodel.VoucherNo,
                            Date = Date,
                            PayFrom = vmodel.PayFrom,
                            PayTo = vmodel.PayTo,
                            Remark = vmodel.Remark,
                            Amount = vmodel.Amount,

                            Status = Status.active,
                            CreatedBy = UserId,
                            CreatedDate = today,
                            Branch = Branch,
                            editable = choice.Yes,
                            Ref1=vmodel.Ref1,
                            Ref2=vmodel.Ref2,
                            Ref3=vmodel.Ref3,
                            Ref4=vmodel.Ref4,
                            Ref5=vmodel.Ref5,
                        };
                        db.ContraVouchers.Add(CV);
                        db.SaveChanges();
                        Int64 ContraVoucherId = CV.ContraVoucherId;

                        // if payment done update to transaction
                        com.addAccountTrasaction(0, Convert.ToDecimal(vmodel.Amount), vmodel.PayFrom, "ContraVoucher", ContraVoucherId, DC.Credit, Date);
                        com.addAccountTrasaction(Convert.ToDecimal(vmodel.Amount), 0, vmodel.PayTo, "ContraVoucher", ContraVoucherId, DC.Debit, Date);
                        com.addlog(LogTypes.Created, UserId, "ContraVoucher", "ContraVouchers", findip(), ContraVoucherId, "Successfully added Contra Voucher details");

                        if (vmodel.submittype == "print")
                        {
                            vmodel.creditor = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
                            vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
                        }
                        var fmapp = db.FieldMappings.Where(a => a.Section == "CVoucher" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        dbTran.Commit();
                        msg = "Successfully Created Contra Voucher details.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, type = vmodel.submittype, message = msg , fmapp = fmapp } };
                    }
                    catch (Exception ex)
                    {
                        dbTran.Rollback();
                        msg = "Failed to Create ContraVoucher. "+ex.Message;
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                }
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
        }

        [QkAuthorize(Roles = "Dev,ContraVoucher List")]
        public JsonResult GetContraVoucher(string InvoiceNo, string FromDate, string ToDate, long? PayFrom, long? PayTo)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit ContraVoucher");
            var uDelete = User.IsInRole("Delete ContraVoucher");

            var v = (from a in db.ContraVouchers
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                     from c in payto.DefaultIfEmpty()
                     where a.editable == choice.Yes &&
                     ((InvoiceNo == null || InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     (PayFrom == null || PayFrom == 0 || a.PayFrom == PayFrom) &&
                     (PayTo == null || PayTo == 0 || a.PayTo == PayTo))
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.ContraVoucherId,
                         a.Date,
                         a.PayFrom,
                         a.PayTo,
                         a.Amount,
                         a.Remark,
                         a.editable,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         a.CreatedDate
                     });

            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p =>p.VoucherNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Payer.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Reciever.ToString().ToLower().Contains(search.ToLower())
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
        [QkAuthorize(Roles = "Dev,Edit ContraVoucher")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContraVoucher rpt = db.ContraVouchers.Find(id);
            if (rpt == null)
            {
                return NotFound();
            }


            var ContraVoucher = new ContraVoucherViewModel
            {
                VoucherNo = rpt.VoucherNo,
                Date = (rpt.Date).ToString("dd-MM-yyyy"),
                PayFrom = rpt.PayFrom,
                PayTo = rpt.PayTo,
                Remark = rpt.Remark,
                Amount = rpt.Amount,
                Branch = rpt.Branch,
                Ref1=rpt.Ref1,
                Ref2=rpt.Ref2,
                Ref3=rpt.Ref3,
                Ref4=rpt.Ref4,
                Ref5=rpt.Ref5,
            };

            var PaidTo = db.Accountss.Where(a => a.AccountsID == ContraVoucher.PayTo).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            var PaidFr = db.Accountss.Where(a => a.AccountsID == ContraVoucher.PayFrom).
            Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(PaidFr, "ID", "Name");

            //Warning before Save
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

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


            //companyinfo
            companySet();
            ViewBag.preEntry = db.ContraVouchers.Where(a => a.ContraVoucherId < id && a.editable == choice.Yes).Select(a => a.ContraVoucherId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.ContraVouchers.Where(a => a.ContraVoucherId > id && a.editable == choice.Yes).Select(a => a.ContraVoucherId).DefaultIfEmpty().Min();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            ContraVoucher.FieldMap = db.FieldMappings.Where(a => a.Section == "CVoucher" && a.Status == Status.active).ToList();
            _FinancialYear();
            return View(ContraVoucher);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit ContraVoucher")]
        public ActionResult Edit(ContraVoucherViewModel vmodel, long id)
        {
            string msg;
            bool stat;
            var Editable = db.ContraVouchers.Any(a => a.editable == choice.No && a.ContraVoucherId == id);
            if (Editable)
            {
                msg = "Sorry, This Contra Voucher is not Editable.";
                stat = false;
            }
            else
            {
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
                            Branch = vmodel.Branch;
                        }
                        else
                        {
                            Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                        }
                        var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();

                        ContraVoucher cv = db.ContraVouchers.Find(id);
                        ContraVoucher RecTemp = cv;
                        var today = Convert.ToDateTime(System.DateTime.Now);
                        var Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                        
                        cv.Date = Date;
                        cv.PayTo = vmodel.PayTo;
                        cv.PayFrom = vmodel.PayFrom;
                        cv.Amount = vmodel.Amount;
                        cv.Remark = vmodel.Remark;
                        cv.Branch = Branch;
                        cv.Ref1 = vmodel.Ref1;
                        cv.Ref2 = vmodel.Ref2;
                        cv.Ref3 = vmodel.Ref3;
                        cv.Ref4 = vmodel.Ref4;
                        cv.Ref5 = vmodel.Ref5;

                        db.Entry(cv).State = EntityState.Modified;
                        db.SaveChanges();
                        var ContraVoucherId = cv.ContraVoucherId;
                        var pdcaccdet = db.AccountsTransactions.Where(a => (a.Account == RecTemp.PayFrom) && (a.Purpose == "ContraVoucher") && a.reference == ContraVoucherId).FirstOrDefault();
                        if (pdcaccdet != null)
                        {
                            var aid = pdcaccdet.Id;
                            com.UpdateAccountTrasaction(aid, 0, Convert.ToDecimal(vmodel.Amount), vmodel.PayFrom, "ContraVoucher", ContraVoucherId, DC.Credit, Date);
                        }
                        var pdcaccdet1 = db.AccountsTransactions.Where(a => (a.Account == RecTemp.PayTo) && (a.Purpose == "ContraVoucher") && a.reference == ContraVoucherId).FirstOrDefault();
                        if (pdcaccdet1 != null)
                        {
                            var aid = pdcaccdet1.Id;
                            com.UpdateAccountTrasaction(aid, Convert.ToDecimal(vmodel.Amount), 0, vmodel.PayTo, "ContraVoucher", ContraVoucherId, DC.Debit, Date);
                        }
                        com.addlog(LogTypes.Created, UserId, "ContraVoucher", "ContraVouchers", findip(), cv.ContraVoucherId, "Successfully added Contra Voucher details");

                        if (vmodel.submittype == "print")
                        {
                            vmodel.creditor = db.Accountss.Where(a => a.AccountsID == vmodel.PayFrom).Select(a => a.Name).FirstOrDefault();
                            vmodel.debitor = db.Accountss.Where(a => a.AccountsID == vmodel.PayTo).Select(a => a.Name).FirstOrDefault();
                        }

                        com.addlog(LogTypes.Updated, UserId, "ContraVoucher", "ContraVouchers", findip(), cv.ContraVoucherId, "Contra Voucher Updated Successfully");
                        dbTran.Commit();
                        msg = "Successfully Updated ContraVoucher details.";
                        stat = true;
                    }
                    catch (Exception ex)
                    {
                        dbTran.Rollback();
                        msg = "Failed to Update Contra Voucher. " + ex.Message;
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                }
            }
            var fmapp = db.FieldMappings.Where(a => a.Section == "CVoucher" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, message = msg, type = vmodel.submittype , fmapp = fmapp } };
        }


        [QkAuthorize(Roles = "Dev,Delete ContraVoucher")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContraVoucher cv = db.ContraVouchers.Find(id);
            if (cv == null)
            {
                return NotFound();
            }
            return PartialView(cv);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete ContraVoucher")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            #region Old Code
            //    //ContraVoucher
            #endregion
            var chk = DeleteCV(id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Contra Voucher details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete ContraVoucher")]
        public ActionResult DeleteAllContraVoucher(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteCV(arr) == true) ? count++ : count;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " ContraVoucher, Unable to Delete " + notdel + " Contra Voucher. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Contra Voucher.", true);
            }
            else
            {
                Success("Deleted " + count + " Contra Voucher.", true);
            }
            return RedirectToAction("Index", "ContraVoucher");
        }

        private Boolean DeleteCV(long id)
        {
            var Editable = db.ContraVouchers.Any(a => a.editable == choice.No && a.ContraVoucherId == id);
            if (Editable)
            {
                return false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                //ContraVoucher
                ContraVoucher CV = db.ContraVouchers.Find(id);
                db.ContraVouchers.Remove(CV);
                db.SaveChanges();
                bool delete = com.DeleteAllAccountTransaction("ContraVoucher", id);
                
                com.addlog(LogTypes.Deleted, UserId, "ContraVoucher", "ContraVouchers", findip(), id, "Contra Voucher Deleted Successfully");
                return true;
            }
        }


        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View ContraVoucher")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            ContraVoucherViewModel vmodel = new ContraVoucherViewModel();
            vmodel = (from b in db.ContraVouchers                     
                      join d in db.Accountss on b.PayFrom equals d.AccountsID into payfrom
                      from d in payfrom.DefaultIfEmpty()
                      join e in db.Accountss on b.PayTo equals e.AccountsID into payto
                      from e in payto.DefaultIfEmpty()
                      where b.ContraVoucherId == id
                      select new ContraVoucherViewModel
                      {
                          VoucherNo = b.VoucherNo,
                          debitor = d.Name,
                          creditor = e.Name,
                        
                          Date = b.Date.ToString(),
                          Amount = b.Amount,
                          Remark = b.Remark.Replace("\n", "<br />"),

                      }).FirstOrDefault();

            return View(vmodel);
        }


        // voucher numbering
        private long Maxvoucher()
        {
            Int64 SENo = 0;
            if ((db.ContraVouchers.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = 1;
            }
            else
            {
                SENo = db.ContraVouchers.Max(p => p.Voucher + 1);
            }

            return SENo;
        }
        private string VoucherNo(Int64 SENo = 0, string billNo = null)
        {
            Int32 number = db.CodePrefixs.Where(a => a.section == "ContraVoucher").Select(a => a.number).FirstOrDefault();
            var prefix = db.CodePrefixs.Where(a => a.section == "ContraVoucher").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.ContraVouchers.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        billNo = prefix + 1;
                    }
                    else
                    {
                        billNo = prefix + number;
                    }
                }
                else
                {
                    SENo = db.ContraVouchers.Max(p => p.Voucher + 1);
                    billNo = prefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = VoucherNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = prefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = VoucherNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SENo)
        {
            var Exists = db.ContraVouchers.Any(c => c.VoucherNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private bool BillExist(string SENo, long? recid = null)
        {
            bool res;
            if (recid != null)
            {
                var Exists = db.ContraVouchers.Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
            }
            else
            {
                var Exists = db.ContraVouchers.Where(a => a.ContraVoucherId != recid).Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
               
            }
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
