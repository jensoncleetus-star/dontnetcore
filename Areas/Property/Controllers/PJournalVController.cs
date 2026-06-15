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
    public class PJournalVController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PJournalVController()
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

            return View();
        }
        [QkAuthorize(Roles = "Dev,Journal List")]
        public JsonResult GetData(string InvoiceNo, string FromDate, string ToDate, long? PayFrom, long? PayTo, string user, long? type,int? vnature)
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

            var v = (from a in db.Journals
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payto
                     from c in payto.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     where (a.editable == choice.Yes)
                     && ((InvoiceNo == null || InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                    (type == null || a.MOPayment == Paymode) &&
                    (vnature == null || a.VATNature == vnature) &&
                    (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                    (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                    (PayFrom == 0 || PayFrom == null || a.PayFrom == PayFrom) &&
                    (PayTo == 0 || PayTo == null || a.PayTo == PayTo)) && (user == null || user == "" || g.Id == user)
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
                                 //p.MOPayment.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.PayFrom.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.PayTo.ToString().ToLower().Contains(search.ToLower()) ||
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
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // create 
        [QkAuthorize(Roles = "Dev,Create Journal")]
        public ActionResult Create()
        {
            _FinancialYear();
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
            ViewBag.id = 0;
            var Journal = new JournalViewModel
            {
                VoucherNo = JournalVoucherNo(),
                Date = (System.DateTime.Now).ToString("dd-MM-yyyy")
            };

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "Property" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;
            ViewBag.BusinessType= db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            //var proj = db.Projects
            //    .Select(s => new
            //    {
            //        ID = s.ProjectId,
            //        Name = s.ProjectName
            //    })
            //    .ToList();
            //ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            //var tsk = db.ProTasks
            // .Select(s => new
            // {
            //     ID = s.ProTaskId,
            //     Name = s.TaskName
            // })
            // .ToList();
            //ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");
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

            return View(Journal);
        }



        [HttpPost]
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object


            string id = Request.Form.GetValues("id").First();
            long jrlid = 0;
            if (id.Contains("undefined")||id.Contains("0"))
            {
               var  lastid = db.Journals.OrderByDescending(o => o.JournalId).FirstOrDefault();
                jrlid = lastid.JournalId;

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
                    
                        
                        var jrn = db.Journals.Find(jrlid);
                        jrn.Ref5 = fileNames;
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








        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Journal")]
        public JsonResult Create(JournalVViewModel vmodel)
        {
            string msg;
            bool stat;

            if (!journalBillExist(Convert.ToString(vmodel.VoucherNo)))
            {
                //using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                //{
                //    try
                //    {
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



              //  IFormFile file = Request.Form.Files["RecieptDoc"];
             //   var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
              //  vmodel.Ref5 = fileNames;
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
                };

                
               
/*
                if (file.FileName != "")
                {
                    // var fileNames = ReceiptId + System.IO.Path.GetExtension(file.FileName);
                    var uploadUrl = LegacyWeb.MapPath("~/uploads/RecieptDoc/");
                    if (!Directory.Exists(uploadUrl))
                        Directory.CreateDirectory(uploadUrl);
                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                }

                */






                db.Journals.Add(JOR);
                db.SaveChanges();
                Int64 JournalId = JOR.JournalId;
                bool? Astatus = null;
                if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                {
                    if (vmodel.MOPayment == ModeOfPayment.PDC)
                    {
                        Astatus = true;
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
                        Bills = null,
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
                                       join p in db.PropertyMains on a.Project equals p.Id into proj
                                       from p in proj.DefaultIfEmpty()
                                       join t in db.PropertyUnits on a.ProTask equals t.Id into protask
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
                                           ProjectName = p.Name,
                                           TaskName = t.Name,
                                       }).ToList();
                }
                var fmapp = db.FieldMappings.Where(a => a.Section == "Journal" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                //dbTran.Commit();
                msg = "Successfully Created Journal details.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, type = vmodel.submittype, message = msg, fmapp = fmapp } };
                //    }
                //    catch (Exception ex)
                //    {
                //        //dbTran.Rollback();
                //        msg = "Failed to Create Journal.";
                //        stat = false;
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                //    }
                //}
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
            if(id!=null)
            {
            ViewBag.id = id;
            var fname = db.Journals.Where(o => o.JournalId == id).FirstOrDefault();
            ViewBag.fname = fname.Ref5;
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Journal rpt = db.Journals.Find(id);
            if (rpt == null)
            {
                return NotFound();
            }
            var Journal = new JournalVViewModel
            {
                VoucherNo = rpt.VoucherNo,
                Date = rpt.Date.ToString("dd-MM-yyyy"),
                PayFrom = rpt.PayFrom,
                PayTo = rpt.PayTo,
                Remark = rpt.Remark,
                Paying = rpt.Paying,
                Branch = rpt.Branch,
                VATNature = rpt.VATNature,
                Ref1 = rpt.Ref1,
                Ref2 = rpt.Ref2,
                Ref3 = rpt.Ref3,
                Ref4 = rpt.Ref4,
                Ref5 = rpt.Ref5,

                MOPayment = rpt.MOPayment,
                PDCDate = rpt.PDCDate != null ? (rpt.PDCDate).Value.ToString("dd-MM-yyyy") : rpt.PDCDate.ToString(),
                Bank = db.PDCs.Where(p => (p.Reference == rpt.JournalId && p.PDCType == "Journal")).Select(p => p.Bank).FirstOrDefault(),
                CheckNo = db.PDCs.Where(p => (p.Reference == rpt.JournalId && p.PDCType == "Journal")).Select(p => p.CheckNo).FirstOrDefault(),
            };
            Journal.jnlitems = (from a in db.AccountsTransactions
                                join b in db.Accountss on a.Account equals b.AccountsID
                                join p in db.PropertyMains on a.Project equals p.Id into proj
                                from p in proj.DefaultIfEmpty()
                                join t in db.PropertyUnits on a.ProTask equals t.Id into protask
                                from t in protask.DefaultIfEmpty()
                                where a.reference == rpt.JournalId && a.Purpose == "Journal"
                                select new JournalVItems
                                {
                                    AccType = (a.Type == 0) ? 0 : 1,
                                    AccountID = a.Account,
                                    Debit = a.Debit,
                                    Credit = a.Credit,
                                    Narration = a.Narration,
                                    AccountName = b.Name,
                                    ProjectId = a.Project,
                                    ProjectName = p.Name,
                                    TaskId = a.ProTask,
                                    TaskName = t.Name,
                                }).ToList();
            var PaidTo = db.Accountss.Where(a => a.AccountsID == Journal.PayTo && a.Group != 23).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

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
            var PaidFr = db.Accountss.Where(a => a.AccountsID == Journal.PayFrom).
            Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(PaidFr, "ID", "Name");

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "Property" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            //var proj = db.Projects
            //    .Select(s => new
            //    {
            //        ID = s.ProjectId,
            //        Name = s.ProjectName
            //    })
            //    .ToList();
            //ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            //var tsk = db.ProTasks
            // .Select(s => new
            // {
            //     ID = s.ProTaskId,
            //     Name = s.TaskName
            // })
            // .ToList();
            //ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");
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

            return View(Journal);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Journal")]
        public ActionResult Edit(JournalVViewModel vmodel, long id)
        {
            string msg;
            bool stat;
            ViewBag.id = id;
            var Editable = db.Journals.Any(a => a.editable == choice.No && a.JournalId == id);
            if (Editable)
            {
                msg = "Sorry,This Journal Cannot be Editable.";
                stat = false;
            }
            else
            {
                //using (DbContextTransaction dbTran = db.Database.BeginTransaction())
                //{
                //    try
                //    {
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
                Journal rec = joudet;
                Journal RecTemp = joudet;
                var today = Convert.ToDateTime(System.DateTime.Now);
                DateTime Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));

                long PayFrom = vmodel.jnlitems.Where(a => a.Credit != null && a.Credit != 0).Select(a => a.AccountID).FirstOrDefault();
                long PayTo = vmodel.jnlitems.Where(a => a.Debit != null && a.Debit != 0).Select(a => a.AccountID).FirstOrDefault();

                //----------------------------------------------------------------------------
                var JournalId = rec.JournalId;
                // update pdc based on payment method
                bool delete = com.DeleteAllAccountTransaction("Journal", rec.JournalId);

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
                        pdc.Bills = null;

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
                            //stats = true;
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
                    else
                    {
                        if (vmodel.MOPayment == ModeOfPayment.PDC || vmodel.MOPayment == ModeOfPayment.CDC)
                        {
                            //stats = true;
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
                rec.PayTo = PayTo;
                rec.PayFrom = PayFrom;
                rec.Paying = (decimal)vmodel.Paying;
                rec.GrandTotal = (decimal)vmodel.Paying;
                //rec.Balance = vmodel.Balance;
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
                    if (vmodel.MOPayment == ModeOfPayment.PDC)
                    {
                        Astatus = true;
                    }
                }
                else
                {
                    rec.PDCDate = null;
                }

                db.Entry(rec).State = EntityState.Modified;
                db.SaveChanges();
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


                com.addlog(LogTypes.Updated, UserId, "Journal", "Journals", findip(), rec.JournalId, "Journal Updated Successfully");
                if (vmodel.submittype == "print")
                {
                    vmodel.MOPay = Enum.GetName(typeof(ModeOfPayment), vmodel.MOPayment);
                    vmodel.UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.UserName).FirstOrDefault();
                    vmodel.Date = Date.ToString("dd-MM-yyyy");
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    vmodel.ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    vmodel.jnlitems = (from a in db.AccountsTransactions
                                       join b in db.Accountss on a.Account equals b.AccountsID
                                       join p in db.PropertyMains on a.Project equals p.Id into proj
                                       from p in proj.DefaultIfEmpty()
                                       join t in db.PropertyUnits on a.ProTask equals t.Id into protask
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
                                           ProjectName = p.Name,
                                           TaskName = t.Name,
                                       }).ToList();
                }
                //dbTran.Commit();
                msg = "Successfully Updated Journal details.";
                stat = true;
                //    }
                //    catch (Exception ex)
                //    {
                //        //dbTran.Rollback();
                //        msg = "Failed to Update Journal.";
                //        stat = false;
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                //    }
                //}
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
            //var Editable = db.Journals.Any(a => a.editable == choice.No && a.JournalId == id);
            //if (Editable)
            //{
            //    msg = "Sorry,This Journal Cannot be Deleted.";
            //    stat = false;
            //}
            //else
            //{
            //    Journal Rec = db.Journals.Find(id);
            //    db.Journals.Remove(Rec);
            //    db.SaveChanges();
            //    bool delete = com.DeleteAllAccountTransaction("Journal", id);
            //    var UserId = User.Identity.GetUserId();
            //    com.addlog(LogTypes.Deleted, UserId, "Journal", "Journals", findip(), id, "Journal Deleted Successfully");
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
                    //sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Journal Voucher</b></td></tr></table>");

                    //string partyDetails = "<table class='table table-noborder'>" +
                    //                      "<tr style = 'border-top: 0px' >" +
                    //                      "<td width = '50%' style = 'padding: 0px 10px 0px 0px;border-right: 0px' >" +
                    //                      "<table class='table-nob'><tr><th></th><td></td><td></td></tr></table> " +
                    //                "</td><td style = 'padding:0px;' ><table class='table-nob jewel-cus' style='border-left: 1px solid #898989;border: 1px solid #000;width: 100%;height: auto;'>" +
                    //                "<tr><th>Voucher No</th><td>: </td></tr>" +
                    //                "<tr><th> Date </th><td>: </td></tr>" +
                    //                "<tr><th> Prepared By</th><td>: </td></tr>" +
                    //                "<tr><th> Time </th><td>: </td></tr>" +
                    //                "</table></td></tr></table> ";

                    //string vchname = "<div class='text-center' style='margin-top:10px;'>" +
                    //                 "<h4 style='margin-bottom: 5px;text-align: left;border: 1px solid #795548;background: #795548 !important;height: 6px;'>" +
                    //                 "<strong style='border:1px solid #000;padding:3px;background:#ffffff !important;'>JOURNAL VOUCHER</strong>" +
                    //                 "</h4></div>";


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

                    //partyDetails += "<tr><th style='padding: 5px;'> Time </th><td style='padding: 5px;'>: " + details.CreatedDate.ToString("hh:mm tt") + " </td></tr>" ;
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
                    //sb.Append("<th style='padding: 5px;vertical-align: top;text-align:center; border: .1px solid #ccc;'>D/C</th>");
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
                        //sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + accType + "</td>");
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

    }
}