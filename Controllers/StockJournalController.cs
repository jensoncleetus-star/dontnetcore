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
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class StockJournalController : BaseController
    {
        // GET: BillSundry
        ApplicationDbContext db;
        Common com;
        public StockJournalController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: StockJournel

        [RedirectingAction]
        [HttpGet]
        [QkAuthorize(Roles = "Dev,StockJournal List")]
        public ActionResult Index()
        {
            var mcfrom = db.MCs
                             .Select(s => new
                             {
                                 ID = s.MCId,
                                 Name = s.MCName,
                             })
                             .ToList();
            ViewBag.MC = QkSelect.List(mcfrom, "ID", "Name");

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Emp = QkSelect.List(use, "ID", "Name");

            var MlaSJour = db.EnableSettings.Where(a => a.EnableType == "MLASJour").FirstOrDefault();
            var MlaSJours = MlaSJour != null ? MlaSJour.Status : Status.inactive;
            ViewBag.MLASJour = MlaSJours;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindStkJnl").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            return View();
        }
        [RedirectingAction]
        [HttpGet]
        [QkAuthorize(Roles = "Dev,StockJournal Entry")]
        public ActionResult Create()
        {
            var mcfrom = db.MCs
                             .Select(s => new
                             {
                                 ID = s.MCId,
                                 Name = s.MCName,
                             })
                             .ToList();
            ViewBag.MC = QkSelect.List(mcfrom, "ID", "Name");

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Emp = QkSelect.List(use, "ID", "Name");

            var SJModel = new StockJournalViewModel
            {
                Voucher = VoucherNo(),
                SJDate = Convert.ToDateTime(DateTime.Now),
            };
            ViewBag.LastEntry = db.StockJournals.Select(p => p.Id).AsEnumerable().DefaultIfEmpty(0).Max();
            companySet();
            var userpermission = User.IsInRole("All StockJournal Entry");
            var UserId = User.Identity.GetUserId();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
            .Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.LastName
            })
            .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaSJour = db.EnableSettings.Where(a => a.EnableType == "MLASJour").FirstOrDefault();
            var MlaSJours = MlaSJour != null ? MlaSJour.Status : Status.inactive;
            ViewBag.MLASJour = MlaSJours;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //field mapping
            SJModel.FieldMap = db.FieldMappings.Where(a => a.Section == "StkJnl" && a.Status == Status.active).ToList();

            return View(SJModel);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,StockJournal Entry")]
        public JsonResult Create(string[][] array, string[] sjdata, string action, string[][] consumedarray)
        {
            bool stat = false;
            string msg;

            if (!BillExist(Convert.ToString(sjdata[0])))
            {
                var UserId = User.Identity.GetUserId();
                var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                StockJournal SJentry = new StockJournal();

                SJentry.SJNo = GetSJNo();
                SJentry.Voucher = Convert.ToString(sjdata[0]);
                SJentry.SJDate = DateTime.Parse(sjdata[1], new CultureInfo("en-GB"));
                SJentry.Employee = Convert.ToInt64(sjdata[2]);
                SJentry.MCFrom = Convert.ToInt32(sjdata[3]);
                SJentry.MCTo = Convert.ToInt32(sjdata[4]);
                SJentry.Description = (sjdata[5]);
                SJentry.ConsumedAmount = Convert.ToDecimal(sjdata[7]);
                SJentry.GeneratedAmount = Convert.ToDecimal(sjdata[6]);
                SJentry.CreatedDate = DateTime.Now;
                SJentry.Branch = Convert.ToInt64(BranchID);
                SJentry.CreatedBy = UserId;
                SJentry.Status = Status.active;
                SJentry.Editable = choice.Yes;

                SJentry.Ref1 = Convert.ToString(sjdata[9]);
                SJentry.Ref2 = Convert.ToString(sjdata[10]);
                SJentry.Ref3 = Convert.ToString(sjdata[11]);
                SJentry.Ref4 = Convert.ToString(sjdata[12]);
                SJentry.Ref5 = Convert.ToString(sjdata[13]);


                db.StockJournals.Add(SJentry);
                db.SaveChanges();
                Int64 SJId = 0;
                SJId = SJentry.Id;

                SJItemGenerate sj = new SJItemGenerate();
                foreach (var arr in array)
                {
                    sj.StockJournal = SJId;
                    sj.Item = Convert.ToInt32(arr[0]);
                    sj.Unit = (arr[1]);
                    sj.ItemQuantity = Convert.ToDecimal(arr[2]);
                    sj.Price = Convert.ToDecimal(arr[3]);
                    sj.Amount = Convert.ToDecimal(arr[5]);

                    db.SJItemGenerates.Add(sj);
                    db.SaveChanges();
                }

                SJItemConsume sc = new SJItemConsume();
                foreach (var arra in consumedarray)
                {
                    sc.StockJournal = SJId;
                    sc.Item = Convert.ToInt32(arra[0]);
                    sc.Unit = (arra[2]);
                    sc.ItemQuantity = Convert.ToDecimal(arra[3]);
                    sc.Price = Convert.ToDecimal(arra[4]);
                    sc.Amount = Convert.ToDecimal(arra[5]);

                    db.SJItemConsumes.Add(sc);
                    db.SaveChanges();
                }
                //Approved By
                var Appby = Convert.ToString(sjdata[8]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();
                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = SJId;
                        approval.Type = "StockJournal";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }
                com.addlog(LogTypes.Created, UserId, "StockJournal", "StockJournals", findip(), SJId, "Successfully Submitted StockJournal");

                if (action == "print")
                {
                    //               where b.DeliverynoteId == dvId
                    //                   PartyName = c.CustomerName,
                    //                   BillId = b.DvNo,
                    //                   BillNo = b.BillNo,
                    //                   Date = b.DvDate,
                    //                   Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                    //                   Note = b.TermsCondition,
                    //                   Discount = b.DvDiscount,
                    //                   GrandTotal = b.DvGrandTotal,
                    //                   Paid = "",
                    //                   Balance = "",
                    //                   Total = b.DvDiscount + b.DvGrandTotal,
                    //                   SubTotal = b.DvSubTotal,
                    //                   TaxAmount = b.DvTaxAmount,
                    //                   d.Address,
                    //                   d.City,
                    //                   d.State,
                    //                   d.Country,
                    //                   d.Zip,
                    //                   Email = d.EmailId,
                    //                   Phone = d.Phone,
                    //                   Mobile = d.Mobile,
                    //                   TRN = c.TaxRegNo,
                    //                   b.DvItems,
                    //                   b.TermsCondition,
                    //                   b.DvItemQuantity,
                    //                   b.DvSubTotal,
                    //                   b.DvTax,
                    //                   id = b.DeliverynoteId,
                    //                   CreditPeriod = c.CreditPeriod,
                    //    ItemUnitPrice = b.ItemUnitPrice,
                    //    ItemQuantity = b.ItemQuantity,
                    //    ItemSubTotal = b.ItemSubTotal,
                    //    ItemTax = b.ItemTax,
                    //    ItemNote = b.ItemNote,
                    //    ItemTaxAmount = b.ItemTaxAmount,
                    //    ItemTotalAmount = b.ItemTotalAmount,
                    //    ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault()
                    var item = "";
                    var summary = "";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary } };
                }
                else
                {
                    msg = "Successfully submitted StockJournal Entry.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Stockjournal No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
        }

        [HttpGet]
        public ActionResult GetGeneratedItems(long DvID)
        {
            var ConD = (from a in db.SJItemGenerates
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.StockJournal == DvID
                        select new
                        {
                            a.Item,
                            ItemUnit = c.ItemUnitName,
                            ItemName = b.ItemName,
                            item = b.ItemName,
                            ItemQuantity = a.ItemQuantity,
                            ItemUnitPrice = a.Price,
                            ItemCode = b.ItemCode,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ItemTotalAmount = a.Amount
                        }).ToList();
            return LegacyJson(ConD);
        }

        [HttpGet]
        public JsonResult GetConsumedItems(long DvID)
        {
            var ConDcon = (from a in db.SJItemConsumes
                           join b in db.Items on a.Item equals b.ItemID
                           join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                           from c in primary.DefaultIfEmpty()
                           join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                           from d in second.DefaultIfEmpty()
                           where a.StockJournal == DvID
                           select new
                           {
                               a.Item,
                               CItemUnit = c.ItemUnitName,
                               CItemName = b.ItemName,
                               Citem = b.ItemName,
                               CItemQuantity = a.ItemQuantity,
                               CItemUnitPrice = a.Price,
                               CItemCode = b.ItemCode,
                               CItemWithCode = b.ItemCode + " - " + b.ItemName,
                               b.ItemUnitID,
                               b.SubUnitId,
                               PriUnit = c.ItemUnitName,
                               SubUnit = d.ItemUnitName,
                               CItemTotalAmount = a.Amount
                           }).ToList();
            return Json(ConDcon);
        }

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit StockJournal")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockJournal sj = db.StockJournals.Find(id);
            if (sj == null)
            {
                return NotFound();
            }
            StockJournalViewModel vmodel = new StockJournalViewModel();

            var mcfrom = db.MCs
                            .Select(s => new
                            {
                                ID = s.MCId,
                                Name = s.MCName,
                            })
                            .ToList();
            ViewBag.MC = QkSelect.List(mcfrom, "ID", "Name");

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Emp = QkSelect.List(use, "ID", "Name");
            vmodel = (from b in db.StockJournals
                      join d in db.Employees on b.Employee equals d.EmployeeId into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.MCs on b.MCFrom equals e.MCId into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.MCs on b.MCTo equals f.MCId into cen
                      from f in cen.DefaultIfEmpty()
                      where b.Id == id
                      select new StockJournalViewModel
                      {
                          SJNo = b.Id,
                          Employee = b.Employee,
                          SJDate = b.SJDate,
                          Voucher = b.Voucher,
                          MCFrom = b.MCFrom,
                          MCTo = b.MCTo,
                          Description = b.Description,
                          GeneratedAmount = b.GeneratedAmount,
                          ConsumedAmount = b.ConsumedAmount,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();
            companySet();
            ViewBag.preEntry = db.StockJournals.Where(a => a.Id < id).Select(a => a.SJNo).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.StockJournals.Where(a => a.Id > id).Select(a => a.SJNo).DefaultIfEmpty().Min();

            var userpermission = User.IsInRole("All SalesOrder Entry");
            var UserId = User.Identity.GetUserId();

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "StockJournal").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaSJour = db.EnableSettings.Where(a => a.EnableType == "MLASJour").FirstOrDefault();
            var MlaSJours = MlaSJour != null ? MlaSJour.Status : Status.inactive;
            ViewBag.MLASJour = MlaSJours;

            var EditPermission = User.IsInRole("Disable StkJournal Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "StockJournal", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "StkJnl" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit StockJournal")]
        public JsonResult UpdateStockJournal(string[][] array, string[] dvdata, string action, string[][] consumedarray)
        {
            bool stat = false;
            string msg;

            Int64 dvId = Convert.ToInt64(dvdata[8]);
            var UserId = User.Identity.GetUserId();
            var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            var EditPermission = User.IsInRole("Disable StkJournal Edit After Approval");
            if (com.chkApproved(dvId, EditPermission, "StockJournal", UserId) == true)
            {

                StockJournal SJentry = db.StockJournals.Find(dvId);

                SJentry.Voucher = Convert.ToString(dvdata[0]);
                SJentry.SJDate = DateTime.Parse(dvdata[1], new CultureInfo("en-GB"));
                SJentry.Employee = Convert.ToInt64(dvdata[2]);
                SJentry.MCFrom = Convert.ToInt32(dvdata[3]);
                SJentry.MCTo = Convert.ToInt32(dvdata[4]);
                SJentry.Description = (dvdata[5]);
                SJentry.ConsumedAmount = Convert.ToDecimal(dvdata[7]);
                SJentry.GeneratedAmount = Convert.ToDecimal(dvdata[6]);
                SJentry.Branch = Convert.ToInt64(BranchID);
                SJentry.Status = Status.active;
                SJentry.Editable = choice.Yes;

                SJentry.Ref1 = Convert.ToString(dvdata[10]);
                SJentry.Ref2 = Convert.ToString(dvdata[11]);
                SJentry.Ref3 = Convert.ToString(dvdata[12]);
                SJentry.Ref4 = Convert.ToString(dvdata[13]);
                SJentry.Ref5 = Convert.ToString(dvdata[14]);

                db.Entry(SJentry).State = EntityState.Modified;
                db.SaveChanges();
                Int64 delyId = SJentry.Id;
                var GeneratedItem = db.SJItemGenerates.Where(a => a.StockJournal == delyId).FirstOrDefault();
                if (GeneratedItem != null)
                {
                    db.SJItemGenerates.RemoveRange(db.SJItemGenerates.Where(a => a.StockJournal == delyId));
                    db.SaveChanges();
                }
                SJItemGenerate sj = new SJItemGenerate();
                foreach (var arr in array)
                {
                    sj.StockJournal = delyId;
                    sj.Item = Convert.ToInt32(arr[0]);
                    sj.Unit = (arr[1]);
                    sj.ItemQuantity = Convert.ToDecimal(arr[2]);
                    sj.Price = Convert.ToDecimal(arr[3]);
                    sj.Amount = Convert.ToDecimal(arr[5]);
                    db.SJItemGenerates.Add(sj);
                    db.SaveChanges();
                }

                var ConsumedItem = db.SJItemConsumes.Where(a => a.StockJournal == delyId).FirstOrDefault();
                if (ConsumedItem != null)
                {
                    db.SJItemConsumes.RemoveRange(db.SJItemConsumes.Where(a => a.StockJournal == delyId));
                    db.SaveChanges();
                }
                SJItemConsume sc = new SJItemConsume();
                foreach (var arra in consumedarray)
                {
                    sc.Item = Convert.ToInt32(arra[0]);
                    sc.StockJournal = delyId;
                    sc.Unit = (arra[2]);
                    sc.ItemQuantity = Convert.ToDecimal(arra[3]);
                    sc.Price = Convert.ToDecimal(arra[4]);
                    sc.Amount = Convert.ToDecimal(arra[5]);
                    db.SJItemConsumes.Add(sc);
                    db.SaveChanges();
                }

                //Approved By
                var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == delyId && a.Type == "StockJournal").FirstOrDefault();

                var MrnPO = db.Approvals.Where(a => a.TransEntry == delyId && a.Type == "StockJournal").FirstOrDefault();
                if (MrnPO != null)
                {
                    if (chkapp != null)
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == delyId && a.Type == "StockJournal"));
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == delyId && a.Type == "StockJournal"));
                        db.SaveChanges();
                    }
                }
                var Appby = Convert.ToString(dvdata[9]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = delyId;
                        approval.Type = "StockJournal";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }


                com.addlog(LogTypes.Created, UserId, "StockJournal", "StockJournals", findip(), dvId, "Successfully Submitted StockJournal");
            }
            if (action == "print")
            {
                //               where b.DeliverynoteId == dvId
                //                   PartyName = c.CustomerName,
                //                   BillId = b.DvNo,
                //                   BillNo = b.BillNo,
                //                   Date = b.DvDate,
                //                   Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                //                   Note = b.TermsCondition,
                //                   Discount = b.DvDiscount,
                //                   GrandTotal = b.DvGrandTotal,
                //                   Paid = "",
                //                   Balance = "",
                //                   Total = b.DvDiscount + b.DvGrandTotal,
                //                   SubTotal = b.DvSubTotal,
                //                   TaxAmount = b.DvTaxAmount,
                //                   d.Address,
                //                   d.City,
                //                   d.State,
                //                   d.Country,
                //                   d.Zip,
                //                   Email = d.EmailId,
                //                   Phone = d.Phone,
                //                   Mobile = d.Mobile,
                //                   TRN = c.TaxRegNo,
                //                   b.DvItems,
                //                   b.TermsCondition,
                //                   b.DvItemQuantity,
                //                   b.DvSubTotal,
                //                   b.DvTax,
                //                   id = b.DeliverynoteId,
                //                   CreditPeriod = c.CreditPeriod,
                //    ItemUnitPrice = b.ItemUnitPrice,
                //    ItemQuantity = b.ItemQuantity,
                //    ItemSubTotal = b.ItemSubTotal,
                //    ItemTax = b.ItemTax,
                //    ItemNote = b.ItemNote,
                //    ItemTaxAmount = b.ItemTaxAmount,
                //    ItemTotalAmount = b.ItemTotalAmount,
                //    ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault()

                var item = "";
                var summary = "";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary } };
            }
            else
            {
                msg = "Successfully submitted StockJournal Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }


        private long GetSJNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "StockJournal").Select(a => a.number).FirstOrDefault();
            if ((db.StockJournals.Select(p => p.SJNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    SENo = 1;
                }
                else
                {
                    SENo = number;
                }
            }
            else
            {
                SENo = db.StockJournals.Max(p => p.SJNo + 1);
            }
            return SENo;
        }

        //[HttpGet]
        //[QkAuthorize(Roles = "Dev,View Deliverynote")]
        //vmodel = (from a in db.StockJournals
        //          let d = db.MCs.Where(x => x.MCId == a.MCTo).FirstOrDefault()
        //              Id = a.Id,
        //              SJDate = a.SJDate,
        //              Voucher = a.voucher,
        //              // us e.UserName,
        //              EmployeeId = c.EmployeeId,
        //              Description = a.Narration,
        //              MCIdfrom = b.MCId,
        //              MCIdto = d.MCId
        //.Select(b => new DvItemViewModel
        //    ItemUnitPrice = b.ItemUnitPrice,
        //    ItemQuantity = b.ItemQuantity,
        //    ItemSubTotal = b.ItemSubTotal,
        //    ItemTax = b.ItemTax,
        //    ItemNote = b.ItemNote,
        //    ItemTaxAmount = b.ItemTaxAmount,
        //    ItemTotalAmount = b.ItemTotalAmount,
        //    ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault()

        // index section below
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Deliverynote List")]
        public ActionResult GetStockJournal(string FromDate, string ToDate, long? Employee, long? MCFrom, long? MCTo, string appstat)
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

            var userpermission = User.IsInRole("All StockJournal Entry");
            var UserId = User.Identity.GetUserId();

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
            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit StockJournal");
            var uDelete = User.IsInRole("Delete StockJournal");

            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets) nor the `let d = db.MCs...FirstOrDefault()` correlated subquery
            // inside the executed select. Split SERVER from CLIENT: materialize only entity columns + simple
            // scalars (the MCTo lookup becomes a normal left-join) into serverRows, build client lookups keyed
            // by StockJournal Id, then re-project client-side with the SAME member names + order.
            var serverQuery = (from a in db.StockJournals
                     join b in db.MCs on a.MCFrom equals b.MCId into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Employees on a.Employee equals c.EmployeeId into mech
                     from c in mech.DefaultIfEmpty()
                     join dd in db.MCs on a.MCTo equals dd.MCId into mcto
                     from dd in mcto.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id

                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.

                     where (Employee == 0 || Employee == null || a.Employee == Employee) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.SJDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.SJDate, tdate) >= 0) &&
                     (MCFrom == null || MCFrom == 0 || a.MCFrom == MCFrom) &&
                     (MCTo == null || MCTo == 0 || a.MCTo == MCTo)
                     select new
                     {
                         a.Id,
                         a.SJDate,
                         Voucher = a.Voucher,
                         UserName = e.UserName,
                         Employee = c.FirstName + " " + c.LastName,
                         Description = a.Description,
                         MCFrom = b.MCName,
                         MCTo = dd.MCName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         b.CreatedDate
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "CreatedDate","Delete","Description","Dev","Edit","Employee","Id","MCFrom","MCTo","SJDate","UserName","Voucher" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("Id asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by StockJournal Id (missing key -> empty, no KeyNotFound).
            var sjIds = serverRows.Select(o => o.Id).ToList();
            // app = approver EmployeeIds (nested collection, keyed by TransEntry == StockJournal Id).
            var appLookup = db.Approvals
                .Where(x => x.Type == "StockJournal" && sjIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.EmployeeId })
                .ToList()
                .ToLookup(x => x.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(x => x.Type == "StockJournal" && sjIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.ApprovalStatus, x.ApprovedBy, x.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(x => x.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per journal.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(x => x.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(x => x.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.Id].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.Id].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.Id, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.Id,
                         o.SJDate,
                         o.Voucher,
                         o.UserName,
                         o.Employee,
                         o.Description,
                         o.MCFrom,
                         o.MCTo,
                         o.Dev,
                         o.Edit,
                         o.Delete,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate
                     };
                     });
            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.Voucher.ToString().ToLower().Contains(search.ToLower()));
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


        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete StockJournal")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockJournal sj = db.StockJournals.Find(id);
            if (sj == null)
            {
                return NotFound();
            }
            return PartialView(sj);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete StockJournal")]
        public ActionResult Delete(long id)
        {
            try
            {
                bool stat = false;
                string msg;
                #region Old Code

                #endregion

                var chk = DeleteStkJnl(id);
                if (chk == true)
                {
                    stat = true;
                    msg = "Successfully deleted StockJournals.";
                }
                else
                {
                    stat = false;
                    msg = "Looks like something went wrong. Please check your form.";
                }
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            catch
            {
                return View();
            }
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete StockJournal")]
        public ActionResult DeleteAllStkJournal(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteStkJnl(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " StockJournals.", true);
            return RedirectToAction("Index", "StockJournal");
        }
        private Boolean DeleteStkJnl(long id)
        {
            var UserId = User.Identity.GetUserId();
            StockJournal sj = db.StockJournals.Find(id);
            var genitem = db.SJItemGenerates.Where(a => a.StockJournal == id);
            var conitem = db.SJItemConsumes.Where(a => a.StockJournal == id);
            if (genitem != null)
            {
                db.SJItemGenerates.RemoveRange(db.SJItemGenerates.Where(a => a.StockJournal == id));
            }
            if (conitem != null)
            {
                db.SJItemConsumes.RemoveRange(db.SJItemConsumes.Where(a => a.StockJournal == id));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "StockJournal").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "StockJournal"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "StockJournal").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "StockJournal"));
            }
            db.StockJournals.Remove(sj);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "StockJournal", "StockJournals", findip(), id, "Successfully Deleted StockJournals");

            return true;
        }

        private string VoucherNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "StockJournal").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "StockJournal").Select(a => a.number).FirstOrDefault();
                if ((db.StockJournals.Select(p => p.SJNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.StockJournals.Max(p => p.SJNo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = VoucherNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = VoucherNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SENo)
        {
            var Exists = db.StockJournals.Any(c => c.Voucher == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "StockJournal" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.StockJournals.Where(a => a.Id == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "StockJournal").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = MR.CreatedBy;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "StockJournal";

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
                            join d in db.StockJournals on b.TransEntry equals d.Id into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "StockJournal"
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
