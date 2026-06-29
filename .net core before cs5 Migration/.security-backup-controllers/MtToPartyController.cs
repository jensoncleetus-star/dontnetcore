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
    public class MtToPartyController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public MtToPartyController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: MtToParty
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,MtToParty List")]
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
        public ActionResult GetMaterialIssued(string Voucher, long? Party, string fromdate, string todate, long? MC)
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
            var uDelete = User.IsInRole("Delete Sales Entry ");

            var v = (from a in db.MtToPartys
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
                         Id = a.MtToPartyId,
                         VoucherNo = a.VoucherNo,
                         Date = a.Date,
                         Type = a.MtToType,
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
                         Details = uSalesEntryView,
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
        //[QkAuthorize(Roles = "Dev,MtToParty Entry")]
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

            var MtTo = new MtToPartyViewModel
            {
                VoucherNo = VNo(),
                Date = DateTime.Now
            };

            return View(MtTo);
        }

        [RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,MtToParty Entry")]
        public JsonResult CreateMt(string[][] array, string[] mtdata, SEBillSundryViewModel bsmodel)
        {
            string billno = (mtdata[0]);
            var Billcheck = db.MtToPartys.Where(a => a.VoucherNo == billno).Any();

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
                var sl = new MtToParty
                {
                    Voucher = GetSeNo(),
                    VoucherNo = mtdata[0],
                    Date = DateTime.Parse(mtdata[1], new CultureInfo("en-GB")),
                    MtToType = (MtToType)Convert.ToInt64(mtdata[2]),
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
                db.MtToPartys.Add(sl);
                db.SaveChanges();

                Int64 MtToId = sl.MtToPartyId;

                MtToPartyItems mt = new MtToPartyItems();
                foreach (var arr in array)
                {
                    mt.Item = Convert.ToInt64(arr[0]);
                    mt.Unit = arr[1] != null ? Convert.ToInt64(arr[1]) : 0;
                    mt.Quantity = Convert.ToDecimal(arr[2]);
                    mt.Price = Convert.ToDecimal(arr[3]);
                    mt.Amount = Convert.ToDecimal(arr[5]);
                    mt.MtToParty = MtToId;
                    mt.ItemNote = arr[7];
                    db.MtToPartyItemss.Add(mt);
                    db.SaveChanges();
                }
                if (bsmodel.sebsundrys != null)
                {
                    MtToPartyBSundry mtbs = new MtToPartyBSundry();
                    foreach (var bs in bsmodel.sebsundrys)
                    {
                        mtbs.MtToParty = MtToId;
                        mtbs.BillSundry = bs.BillSundry;
                        mtbs.BsValue = bs.BsValue;
                        mtbs.AmountType = bs.AmountType;
                        mtbs.BsType = bs.BsType;
                        mtbs.BsAmount = bs.BsAmount;
                        db.MtToPartyBSundrys.Add(mtbs);
                        db.SaveChanges();
                    }
                }
                msg = "Successfully submitted Materials Issued to Party Entry.";
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
        //[QkAuthorize(Roles = "Dev,Edit MtToParty")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MtToParty mtentry = db.MtToPartys.Find(id);
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

            MtToPartyViewModel vmodel = new MtToPartyViewModel();

            vmodel = (from a in db.MtToPartys
                      join b in db.Accountss on a.Party equals b.AccountsID into prt
                      from b in prt.DefaultIfEmpty()
                      join c in db.MCs on a.MC equals c.MCId into mc
                      from c in mc.DefaultIfEmpty()
                      where a.MtToPartyId == id

                      select new MtToPartyViewModel
                      {
                          VoucherNo = a.VoucherNo,
                          Date = a.Date,
                          MC = a.MC,
                          Description = a.Description,
                          MtToType = a.MtToType,
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
        //[QkAuthorize(Roles = "Dev,Edit MtToParty")]
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
            MtToParty mts = db.MtToPartys.Find(TheId);
            if (mts.Voucher != null)
            {
                mts.VoucherNo = mtdata[0];
                mts.Date = DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
                mts.MtToType = (MtToType)Convert.ToInt64(mtdata[2]);
                mts.Party = Convert.ToInt64(mtdata[3]);
                mts.MC = MC;
                mts.Description = mtdata[5];
                mts.TotalQuantity = Convert.ToDecimal(mtdata[7]);
                mts.TotalAmount = Convert.ToDecimal(mtdata[9]);

                db.Entry(mts).State = EntityState.Modified;
                db.SaveChanges();

                Int64 MtId = mts.MtToPartyId;

                var mt = db.MtToPartyItemss.Where(a => a.MtToParty == MtId).FirstOrDefault();
                if (mt != null)
                {
                    db.MtToPartyItemss.RemoveRange(db.MtToPartyItemss.Where(a => a.MtToParty == MtId));
                    db.SaveChanges();
                }
                foreach (var arr in array)
                {
                    mt.Item = Convert.ToInt64(arr[0]);
                    mt.Unit = arr[1] != null ? Convert.ToInt64(arr[1]) : 0;
                    mt.Quantity = Convert.ToDecimal(arr[2]);
                    mt.Price = Convert.ToDecimal(arr[3]);
                    mt.Amount = Convert.ToDecimal(arr[5]);
                    mt.MtToParty = MtId;
                    mt.ItemNote = arr[7];
                    db.MtToPartyItemss.Add(mt);
                    db.SaveChanges();
                }

                var mtbs = db.MtToPartyBSundrys.Where(a => a.MtToParty == MtId).FirstOrDefault();
                if (mtbs != null)
                {
                    db.MtToPartyBSundrys.RemoveRange(db.MtToPartyBSundrys.Where(a => a.MtToParty == MtId));
                    db.SaveChanges();
                }
                foreach (var bs in bsmodel.sebsundrys)
                {
                    mtbs.MtToParty = MtId;
                    mtbs.BillSundry = bs.BillSundry;
                    mtbs.BsValue = bs.BsValue;
                    mtbs.AmountType = bs.AmountType;
                    mtbs.BsType = bs.BsType;
                    mtbs.BsAmount = bs.BsAmount;
                    db.MtToPartyBSundrys.Add(mtbs);
                    db.SaveChanges();
                }
                msg = "Successfully Updated Materials Issued to Party Entry.";
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
        //[QkAuthorize(Roles = "Dev,Delete MtToParty")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MtToParty mtd = db.MtToPartys.Find(id);
            
            if (mtd == null)
            {
                return NotFound();
            }
            return PartialView(mtd);
        }

        [RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete MtToParty")]
        public ActionResult DeleteConfirmed(long MtToPartyId)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();

            MtToParty mten = db.MtToPartys.Find(MtToPartyId);
            var MtItem = db.MtToPartyItemss.Where(a => a.MtToParty == MtToPartyId);
            var MtBs = db.MtToPartyBSundrys.Where(a => a.MtToParty == MtToPartyId).FirstOrDefault();

            if (MtItem != null)
            {
                db.MtToPartyItemss.RemoveRange(db.MtToPartyItemss.Where(a => a.MtToParty == MtToPartyId));
            }
            if (MtBs != null)
            {
                db.MtToPartyBSundrys.RemoveRange(db.MtToPartyBSundrys.Where(a => a.MtToParty == MtToPartyId));
            }            
            db.MtToPartys.Remove(mten);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "MtToParty", "MtToPartys", findip(), mten.MtToPartyId, "Successfully Deleted Materials Issued Entry");
                        
            stat = true;
            msg = "Successfully deleted Materials Issued Entry details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete MtToParty")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSale(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Issued To Party Entry.", true);
            return RedirectToAction("Index", "MtToParty");
        }


        private Boolean DeleteSale(long saleId)
        {
            var UserId = User.Identity.GetUserId();
            MtToParty SEen = db.MtToPartys.Find(saleId);
            var SEItem = db.MtToPartyItemss.Where(a => a.MtToParty == saleId);

            var SEBs = db.MtToPartyBSundrys.Where(a => a.MtToParty == saleId).FirstOrDefault();            

            if (SEItem != null)
            {
                db.MtToPartyItemss.RemoveRange(db.MtToPartyItemss.Where(a => a.MtToParty == saleId));
            }
            if (SEBs != null)
            {
                db.MtToPartyBSundrys.RemoveRange(db.MtToPartyBSundrys.Where(a => a.MtToParty == saleId));
            } 

            db.MtToPartys.Remove(SEen);

            com.addlog(LogTypes.Deleted, UserId, "SalesEntry", "SalesEntrys", findip(), SEen.MtToPartyId, "Successfully Deleted Materials Issued Entry");

            db.SaveChanges();

            return true;
        }

        [RedirectingAction]
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Details MtToParty")]
        public ActionResult Details(long? id)
        {
            MtToPartyViewModel vmodel = new MtToPartyViewModel();

            vmodel = (from a in db.MtToPartys
                      join b in db.MtToPartyItemss on a.MtToPartyId equals b.MtToParty into itms
                      from b in itms.DefaultIfEmpty()
                      join c in db.MtToPartyBSundrys on a.MtToPartyId equals c.MtToParty into sndr
                      from c in sndr.DefaultIfEmpty()
                      join d in db.Accountss on a.Party equals d.AccountsID into acc
                      from d in acc.DefaultIfEmpty()
                      where a.MtToPartyId == id
                      select new MtToPartyViewModel
                      {
                          Voucher = a.Voucher,
                          VoucherNo = a.VoucherNo,
                          Date = a.Date,
                          Description = a.Description,
                          MC = a.MC,
                          MtToType = a.MtToType,
                          Party = a.Party,
                          PartyName = d.Name,
                          PartyCode = d.AccountsID.ToString(),
                          TotalAmount = a.TotalAmount,
                          TotalQuantity = a.TotalQuantity

                      }).FirstOrDefault();

            vmodel.MtItem = db.MtToPartyItemss.Where(a => a.MtToParty == id)
                .Select(b => new MtToPartyItemsViewModel
                {     
                    ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                    ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                    UnitName = db.ItemUnits.Where(a => a.ItemUnitID == b.Item).Select(a => a.ItemUnitName).FirstOrDefault(),
                    Quantity = b.Quantity,
                    Price = b.Price,
                    Amount = b.Amount,
                    ItemNote = b.ItemNote
                }).ToList();

            vmodel.SEbs = db.MtToPartyBSundrys.Where(a => a.MtToParty == id)
         .Select(b => new MtToPartyBSundryViewModel
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
        public JsonResult GetMtToPartyItems(long EntryID)
        {
            var ConD = (from a in db.MtToPartyItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.MtToParty == EntryID
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
            var SEBs = (from a in db.MtToPartyBSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.MtToParty == EntryID
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
        private string VNo(Int64 SENo = 0, string billNo = "")
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
                    if (BillExist(billNo))
                    {
                        billNo = VNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = VNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SENo)
        {
            var Exists = db.MtToPartys.Any(c => c.VoucherNo == SENo);
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
