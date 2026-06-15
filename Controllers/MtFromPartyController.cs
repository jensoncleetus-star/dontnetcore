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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class MtFromPartyController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public MtFromPartyController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,MtFromParty List")]
        public ActionResult Index()
        {
            var OpAll = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            ViewBag.Voucher = OpAll;
            var acc = db.Accountss.Select(s => new
            {
                AccId = s.AccountsID,
                Name = s.Name
            }).ToList();
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");
            ViewBag.Party = QkSelect.List(acc, "AccId", "Name");
            return View();
        }

        [RedirectingAction]
        [HttpPost]
        public ActionResult GetMaterialReceived(string Voucher, long? Party, string fromdate, string todate, long? MC)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? tDateAge = null;
            DateTime datenow = DateTime.Now;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var uDev = User.IsInRole("Dev");
            var uSalesEntryView = User.IsInRole("View Sales Entry");
            var uEdit = User.IsInRole("Edit Sales Entry");
            var uDelete = User.IsInRole("Delete Sales Entry");

            var v = (from a in db.MtFromPartys
                     join b in db.Accountss on a.Party equals b.AccountsID into prt
                     from b in prt.DefaultIfEmpty()
                     join c in db.MCs on a.MC equals c.MCId into mc
                     from c in mc.DefaultIfEmpty()
                     join d in db.Users on a.CreatedBy equals d.Id
                     where (Voucher == null || Voucher == "" || Voucher == a.VoucherNo) &&
                     (Party == null || Party == 0 || a.Party == Party) &&
                      (MC == null || MC == 0 || a.MC == MC) && ((fromdate == null || fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                              (todate == null || todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0))
                     select new
                     {
                         Id = a.MtFromPartyId,
                         VoucherNo = a.VoucherNo,
                         Date = a.Date,
                         Type = a.MtFromType,
                         Party = b.AccountsID + " - " + b.Name,
                         MC = c.MCName,
                         Description = a.Description,
                         TotalQuantity = a.TotalQuantity,
                         TotalAmount = a.TotalAmount,
                         User = d.UserName,
                         a.CreatedDate

                     }).ToList().Select(b => new 
                     {
                         b.Id,
                         b.VoucherNo,
                         b.Date,
                         Type = b.Type.GetType().GetMember(b.Type.ToString()).First().GetCustomAttribute<DisplayAttribute>().Name,
                         Party = b.Party,
                         b.MC,
                         b.Description,
                         b.TotalQuantity,
                         b.TotalAmount,
                         b.User,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         b.CreatedDate
                     });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.VoucherNo.ToString().ToLower().Contains(search.ToLower()));
            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,MtFromParty Entry")]
        public ActionResult Create()
        {
            var acc = db.Accountss.Select(s => new
            {
                AccId = s.AccountsID,
                Name = s.Name
            }).ToList();
            ViewBag.Party = QkSelect.List(acc, "AccId", "Name");

            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            var MtFrom = new MtFromPartyViewModel
            {
                VoucherNo = VNo(),
                Date = DateTime.Now
            };
            return View(MtFrom);
        }
        [RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,MtFromParty Entry")]
        public JsonResult CreateMt(string[][] array, string[] mtdata, SEBillSundryViewModel bsmodel)
        {
            string billno = (mtdata[0]);
            var Billcheck = db.MtFromPartys.Where(a => a.VoucherNo == billno).Any();
            bool stat = false;
            string msg = "";
            if (!Billcheck)
            {
                var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                long MC = 0;
                if (MCcheck == Status.active)
                {
                    MC = Convert.ToInt32(mtdata[4]);
                }
                else
                {
                    MC = 1;
                }

                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var sl = new MtFromParty
                {
                    Voucher = GetSeNo(),
                    VoucherNo = mtdata[0],
                    Date = DateTime.Parse(mtdata[1], new CultureInfo("en-GB")),
                    MtFromType = (MtFromType)Convert.ToInt64(mtdata[2]),
                    Party = Convert.ToInt64(mtdata[3]),
                    MC = MC,
                    Description = mtdata[5],
                    TotalQuantity = Convert.ToDecimal(mtdata[7]),
                    TotalAmount = Convert.ToDecimal(mtdata[9]),
                    CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                    CreatedBy = UserId,
                    Status = Status.active,
                    editable = choice.Yes,
                    Branch = Convert.ToInt64(BranchID),
                };
                db.MtFromPartys.Add(sl);
                db.SaveChanges();

                Int64 MtFromId = sl.MtFromPartyId;                

                MtFromPartyItems mt = new MtFromPartyItems();
                foreach (var arr in array)
                {
                    mt.Item = Convert.ToInt64(arr[0]);
                    mt.Unit = arr[1] != null ? Convert.ToInt64(arr[1]) : 0;
                    mt.Quantity = Convert.ToDecimal(arr[2]);
                    mt.Price = Convert.ToDecimal(arr[3]);
                    mt.Amount = Convert.ToDecimal(arr[5]);
                    mt.MtFromParty = MtFromId;
                    mt.ItemNote = arr[7];
                    db.MtFromPartyItemss.Add(mt);
                    db.SaveChanges();
                }
                if (bsmodel.sebsundrys != null)
                {
                    MtFromPartyBSundry mtbs = new MtFromPartyBSundry();
                    foreach (var bs in bsmodel.sebsundrys)
                    {
                        mtbs.MtFromParty = MtFromId;
                        mtbs.BillSundry = bs.BillSundry;
                        mtbs.BsValue = bs.BsValue;
                        mtbs.AmountType = bs.AmountType;
                        mtbs.BsType = bs.BsType;
                        mtbs.BsAmount = bs.BsAmount;
                        db.MtFromPartyBSundrys.Add(mtbs);
                        db.SaveChanges();
                    }
                }
                msg = "Successfully submitted Materials Received From Party Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Edit MtFromParty")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MtFromParty mtentry = db.MtFromPartys.Find(id);
            if (mtentry == null)
            {
                return NotFound();
            }

            var acc = db.Accountss.Select(s => new
            {
                AccId = s.AccountsID,
                Name = s.Name
            }).ToList();
            ViewBag.Party1 = QkSelect.List(acc, "AccId", "Name");

            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");

            MtFromPartyViewModel vmodel = new MtFromPartyViewModel();

            vmodel = (from a in db.MtFromPartys
                      join b in db.Accountss on a.Party equals b.AccountsID into prt
                      from b in prt.DefaultIfEmpty()
                      join c in db.MCs on a.MC equals c.MCId into mc
                      from c in mc.DefaultIfEmpty()
                      where a.MtFromPartyId == id

                      select new MtFromPartyViewModel
                      {
                          VoucherNo = a.VoucherNo,
                          Date = a.Date,
                          MC = a.MC,
                          Description = a.Description,
                          MtFromType = a.MtFromType,
                          Party = a.Party,
                          TotalAmount = a.TotalAmount,
                          TotalQuantity = a.TotalQuantity,
                          Voucher = a.Voucher
                      }).FirstOrDefault();


            companySet();
            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            return View(vmodel);
        }

        [RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Edit MtFromParty")]
        public JsonResult EditMt(string[][] array, string[] mtdata, SEBillSundryViewModel bsmodel)
        {
            bool stat = false;
            string msg;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

            long MC = 0;
            if (MCcheck == Status.active)
            {
                MC = Convert.ToInt32(mtdata[4]);
            }
            else
            {
                MC = 1;
            }

            var TheId = Convert.ToInt64(mtdata[10]);
            MtFromParty mts = db.MtFromPartys.Find(TheId);
            if (mts.Voucher != null)
            {
                mts.VoucherNo = mtdata[0];
                mts.Date = DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
                mts.MtFromType = (MtFromType)Convert.ToInt64(mtdata[2]);
                mts.Party = Convert.ToInt64(mtdata[3]);
                mts.MC = MC;
                mts.Description = mtdata[5];
                mts.TotalQuantity = Convert.ToDecimal(mtdata[7]);
                mts.TotalAmount = Convert.ToDecimal(mtdata[9]);

                db.Entry(mts).State = EntityState.Modified;
                db.SaveChanges();

                Int64 MtId = mts.MtFromPartyId;
                
                var mt = db.MtFromPartyItemss.Where(a => a.MtFromParty == MtId).FirstOrDefault();
                if (mt != null)
                {
                    db.MtFromPartyItemss.RemoveRange(db.MtFromPartyItemss.Where(a => a.MtFromParty == MtId));
                    db.SaveChanges();
                }
                foreach (var arr in array)
                {
                    mt.Item = Convert.ToInt64(arr[0]);
                    mt.Unit = arr[1] != null ? Convert.ToInt64(arr[1]) : 0;
                    mt.Quantity = Convert.ToDecimal(arr[2]);
                    mt.Price = Convert.ToDecimal(arr[3]);
                    mt.Amount = Convert.ToDecimal(arr[5]);
                    mt.MtFromParty = MtId;
                    mt.ItemNote = arr[7];
                    db.MtFromPartyItemss.Add(mt);
                    db.SaveChanges();
                }
                
                var mtbs = db.MtFromPartyBSundrys.Where(a => a.MtFromParty == MtId).FirstOrDefault();
                if (mtbs != null)
                {
                    db.MtFromPartyBSundrys.RemoveRange(db.MtFromPartyBSundrys.Where(a => a.MtFromParty == MtId));
                    db.SaveChanges();
                }
                foreach (var bs in bsmodel.sebsundrys)
                {
                    mtbs.MtFromParty = MtId;
                    mtbs.BillSundry = bs.BillSundry;
                    mtbs.BsValue = bs.BsValue;
                    mtbs.AmountType = bs.AmountType;
                    mtbs.BsType = bs.BsType;
                    mtbs.BsAmount = bs.BsAmount;
                    db.MtFromPartyBSundrys.Add(mtbs);
                    db.SaveChanges();
                }
                msg = "Successfully Updated Materials Received From Party Entry.";
                stat = true;
            }
            else
            {
                msg = "Voucher Number Already Exists.";
                stat = false;

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }



        [RedirectingAction]
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Delete MtFromParty")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MtFromParty mtd = db.MtFromPartys.Find(id);

            if (mtd == null)
            {
                return NotFound();
            }
            return PartialView(mtd);
        }

        [RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete MtFromParty")]
        public ActionResult DeleteConfirmed(long MtFromPartyId)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();

            MtFromParty mten = db.MtFromPartys.Find(MtFromPartyId);
            var MtItem = db.MtFromPartyItemss.Where(a => a.MtFromParty == MtFromPartyId);
            var MtBs = db.MtFromPartyBSundrys.Where(a => a.MtFromParty == MtFromPartyId).FirstOrDefault();

            if (MtItem != null)
            {
                db.MtFromPartyItemss.RemoveRange(db.MtFromPartyItemss.Where(a => a.MtFromParty == MtFromPartyId));
            }
            if (MtBs != null)
            {
                db.MtFromPartyBSundrys.RemoveRange(db.MtFromPartyBSundrys.Where(a => a.MtFromParty == MtFromPartyId));
            }
            db.MtFromPartys.Remove(mten);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "MtToParty", "MtToPartys", findip(), mten.MtFromPartyId, "Successfully Deleted Materials Received Entry");

            stat = true;
            msg = "Successfully deleted Materials Received Entry details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete MtFromParty")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSale(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Received From Party Entry.", true);
            return RedirectToAction("Index", "MtFromParty");
        }


        private Boolean DeleteSale(long saleId)
        {
            var UserId = User.Identity.GetUserId();
            MtFromParty SEen = db.MtFromPartys.Find(saleId);
            var SEItem = db.MtFromPartyItemss.Where(a => a.MtFromParty == saleId);

            var SEBs = db.MtFromPartyBSundrys.Where(a => a.MtFromParty == saleId).FirstOrDefault();

            if (SEItem != null)
            {
                db.MtFromPartyItemss.RemoveRange(db.MtFromPartyItemss.Where(a => a.MtFromParty == saleId));
            }
            if (SEBs != null)
            {
                db.MtFromPartyBSundrys.RemoveRange(db.MtFromPartyBSundrys.Where(a => a.MtFromParty == saleId));
            }

            db.MtFromPartys.Remove(SEen);

            com.addlog(LogTypes.Deleted, UserId, "SalesEntry", "SalesEntrys", findip(), SEen.MtFromPartyId, "Successfully Deleted Materials Received Entry");

            db.SaveChanges();

            return true;
        }

        [RedirectingAction]
        [HttpGet]
        public ActionResult Details(long? id)
        {
            MtFromPartyViewModel vmodel = new MtFromPartyViewModel();

            vmodel = (from a in db.MtFromPartys
                      join b in db.MtFromPartyItemss on a.MtFromPartyId equals b.MtFromParty into itms
                      from b in itms.DefaultIfEmpty()
                      join c in db.MtFromPartyBSundrys on a.MtFromPartyId equals c.MtFromParty into sndr
                      from c in sndr.DefaultIfEmpty()
                      join d in db.Accountss on a.Party equals d.AccountsID into acc
                      from d in acc.DefaultIfEmpty()
                      where a.MtFromPartyId == id
                      select new MtFromPartyViewModel
                      {
                          Voucher = a.Voucher,
                          VoucherNo = a.VoucherNo,
                          Date = a.Date,
                          Description = a.Description,
                          MC = a.MC,
                          MtFromType = a.MtFromType,
                          Party = a.Party,
                          PartyName = d.Name,
                          PartyCode = d.AccountsID.ToString(),
                          TotalAmount = a.TotalAmount,
                          TotalQuantity = a.TotalQuantity,                         
                      }).FirstOrDefault();

            vmodel.MtItem = db.MtFromPartyItemss.Where(a => a.MtFromParty == id)
                .Select(b => new MtFromPartyItemsViewModel
                {
                    ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                    ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                    UnitName = db.ItemUnits.Where(a => a.ItemUnitID == b.Item).Select(a => a.ItemUnitName).FirstOrDefault(),
                    Quantity = b.Quantity,
                    Price = b.Price,
                    Amount = b.Amount,
                    ItemNote = b.ItemNote
                }).ToList();

            vmodel.SEbs = db.MtFromPartyBSundrys.Where(a => a.MtFromParty == id)
         .Select(b => new MtFromPartyBSundryViewModel
         {
             AmountType = b.AmountType,
             BsAmount = b.BsAmount,
             BsType = b.BsType,
             BsValue = b.BsValue,
             Type = b.BsType == 0 ? "Add" : "Less",
             AmtType = b.AmountType == 0 ? "" : "%",
             BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
         }).ToList();

            return PartialView(vmodel);
        }

        [HttpGet]
        public JsonResult GetMtFromPartyItems(long EntryID)
        {
            var ConD = (from a in db.MtFromPartyItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.MtFromParty == EntryID
                        select new
                        {
                            a.Item,
                            a.Quantity,
                            a.Unit,
                            a.Price,
                            a.Amount,
                            note = a.ItemNote.Replace("<br />", "\n"),

                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.Quantity,
                            o.Unit,
                            o.Price,
                            o.Amount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                        }).ToList();
            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetMtBillSundry(long EntryID)
        {
            var SEBs = (from a in db.MtFromPartyBSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.MtFromParty == EntryID
                        select new
                        {
                            a.AmountType,
                            a.BillSundry,
                            a.BsAmount,
                            a.BsType,
                            a.BsValue,
                            c.BSName
                        }).ToList();
            return Json(SEBs);
        }

        private long GetSeNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Invoice").Select(a => a.number).FirstOrDefault();
            if ((db.MtToPartys.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                SENo = db.MtToPartys.Max(p => p.Voucher + 1);
            }

            return SENo;
        }

        private string VNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Invoice").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Invoice").Select(a => a.number).FirstOrDefault();
                if ((db.MtToPartys.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.MtToPartys.Max(p => p.Voucher + 1);
                    billNo = companyPrefix + SENo;
                    if (MTBillExist(billNo))
                    {
                        billNo = VNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (MTBillExist(billNo))
                {
                    billNo = VNo(SENo, billNo);
                }

            }
            return billNo;
        }


        public JsonResult SearchVoucherFrom(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.MtFromPartys.Where(p => p.VoucherNo.ToLower().Contains(q.ToLower()) || p.VoucherNo.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.VoucherNo, //each json object will have 
                                      id = b.MtFromPartyId
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.MtFromPartys.Where(p => p.Status == Status.active || p.Status == Status.inactive).Select(b => new SelectFormat
                {
                    text = b.VoucherNo, //each json object will have 
                    id = b.MtFromPartyId
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchVoucherTo(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.MtToPartys.Where(p => p.VoucherNo.ToLower().Contains(q.ToLower()) || p.VoucherNo.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.VoucherNo, //each json object will have 
                                      id = b.MtToPartyId
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.MtToPartys.Where(p => p.Status == Status.active || p.Status == Status.inactive).Select(b => new SelectFormat
                {
                    text = b.VoucherNo, //each json object will have 
                    id = b.MtToPartyId
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        public bool MTBillExist(string SENo, long? fromid = null)
        {
            if (fromid != null)
            {
                var Exists = db.MtFromPartys.Any(c => c.VoucherNo == SENo);
                bool res = (Exists) ? true : false;
                return res;
            }
            else
            {
                var Exists = db.MtFromPartys.Where(a => a.MtFromPartyId != fromid).Any(c => c.VoucherNo == SENo);
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
