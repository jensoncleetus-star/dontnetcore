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
using System.Collections;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class MCConversionController : Controller
    {
        ApplicationDbContext db;
        Common com;

        public MCConversionController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: MCConversion
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var STTo = new StockTransferViewModel
            {
                Voucher = GetVTNo(),
                Date = DateTime.Now
            };

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");


            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            if (mcchk != null)
            {
                var mcs1 = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MCToBag = QkSelect.List(mcs1, "Id", "Name");
            }
            else
            {
                var mcs1 = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MCToBag = QkSelect.List(mcs1, "Id", "Name");
            }

            return View(STTo);
        }
        [HttpPost]
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Create ItemTask")]
        public ActionResult Create(string q,string frmdate, string x, long? cust, long mc, long mcto,int? chknegative, string action)
        {
            db.SetCommandTimeOut(60 * 60);
            bool stat = false;
            string msg;
            var today = DateTime.Parse(frmdate, new CultureInfo("en-GB"));
            var UserId = User.Identity.GetUserId();
            if (chknegative == 0||chknegative==null)
            {
                StockTransfer sl = new StockTransfer();

                sl.STNo = InvoiceNo();
                sl.Voucher = "MC-TO- MC-TRANSFER" + sl.STNo;
                sl.Date = today;
                sl.MCFrom = mc;
                sl.MCTo = mcto;
                sl.Remarks = "";
                sl.TotalAmount = 0;
                sl.CreatedDate = today;

                sl.CreatedBy = UserId;
                sl.Status = Status.active;
                sl.editable = choice.Yes;
                sl.Branch = 1;

                string str = "";
                StockType Stype = StockType.StockTransfer;
                sl.StockType = Stype;
                sl.Ref1 = "";
                sl.Ref2 = "";
                sl.Ref3 = "";
                sl.Ref4 = "";
                sl.Ref5 = "";

                db.StockTransfers.Add(sl);
                db.SaveChanges();


                var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
                var MCList = MCList1;
                if (!MCList.Any() && mc == 0)
                {
                    MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
                }
                var MCArray = MCList.ToArray();

                int recordsTotal = 0;
       
                StockTransfer sl2 = new StockTransfer();

                sl2.STNo = InvoiceNo();
                sl2.Voucher = "MC-TO- MC-TRANSFER" + sl.STNo;
                sl2.Date = today;
                sl2.MCFrom = mcto;
                sl2.MCTo = mc;
                sl2.Remarks = "";
                sl2.TotalAmount = 0;
                sl2.CreatedDate = today;

                sl2.CreatedBy = UserId;
                sl2.Status = Status.active;
                sl2.editable = choice.Yes;
                sl2.Branch = 1;

                str = "";
                StockType Stype2 = StockType.StockTransfer;
                sl2.StockType = Stype2;
                sl2.Ref1 = "";
                sl2.Ref2 = "";
                sl2.Ref3 = "";
                sl2.Ref4 = "";
                sl2.Ref5 = "";

                db.StockTransfers.Add(sl2);
                db.SaveChanges();


                List<StockDetails> data = new List<StockDetails>();
                mc = mc != null ? mc : 0;
                var itmids = (from a in db.Items
                              join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                              from b in sti.DefaultIfEmpty()
                              join c in db.StockTransfers on b.StockTransferId equals c.Id
                              where c.MCTo == mc
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();

                var seitems = (from a in db.Items
                               join b in db.SEItemss on a.ItemID equals b.Item into sti
                               from b in sti.DefaultIfEmpty()
                               join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId
                               where c.MaterialCenter == mc
                               select new
                               {
                                   Itemid = a.ItemID
                               }).Distinct();
                var peitem = (from a in db.Items
                              join b in db.PEItemss on a.ItemID equals b.Item into sti
                              from b in sti.DefaultIfEmpty()
                              join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                              where c.MaterialCenter == mc
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();
                var serritem = (from a in db.Items
                                join b in db.SRItemss on a.ItemID equals b.Item into sti
                                from b in sti.DefaultIfEmpty()
                                join c in db.SalesReturns on b.SalesReturnId equals c.SalesReturnId
                                where c.MaterialCenter == mc
                                select new
                                {
                                    Itemid = a.ItemID
                                }).Distinct();
                var itmidsFROM = (from a in db.Items
                                  join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                                  from b in sti.DefaultIfEmpty()
                                  join c in db.StockTransfers on b.StockTransferId equals c.Id
                                  where c.MCFrom == mc
                                  select new
                                  {
                                      Itemid = a.ItemID
                                  }).Distinct();
                var itmidstkadj = (from a in db.Items
                                   join b in db.StockAdjustments on a.ItemID equals b.ItemID into sti
                                   from b in sti.DefaultIfEmpty()
                                   where b.MaterialCenter == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                itmids = itmids.Union(itmidstkadj);
                itmids = itmids.Union(itmidsFROM);
                var assetitemid = (from a in db.Items
                                   join b in db.AssetTransferDetails on a.ItemID equals b.RefItemId into sti
                                   from b in sti.DefaultIfEmpty()
                                   join c in db.AssetTransferMasters on b.AssetEntryId equals c.AssetEntryId
                                   where c.McFromId == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                var assettoinventry = (from a in db.Items
                                   join b in db.AssetToInventoryDetails on a.ItemID equals b.RefItemId into sti
                                   from b in sti.DefaultIfEmpty()
                                   join c in db.AssetToInventoryMasters on b.EntryId  equals c.EntryId
                                   where c.McFromId == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                itmids = itmids.Union(assettoinventry);
                itmids = itmids.Union(assetitemid);
                itmids = itmids.Union(seitems);
                itmids = itmids.Union(peitem);
                itmids = itmids.Union(serritem);
             
                var itlist = itmids.Select(o => o.Itemid).Distinct().ToList();
                DateTime ondates = today;
                foreach (var it in itlist)
                {
                    var selitem = new SqlParameter("@ItemId", it);
                    var selmc = new SqlParameter("@MCId", mc);
                    var brand = new SqlParameter("@BrandId", "");
                    var stkble = new SqlParameter("@Stockble", 1);
                    var catgry = new SqlParameter("@CategoryId", "");
                    var fromdate = new SqlParameter("@fromdate", "");
                    var todate = new SqlParameter("@todate", ondates);
                    var stype = new SqlParameter("@Stype", "1");

                    var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).FirstOrDefault();

                    if (dataadd.ITotalQty > 0)
                    {

                        Int64 STId = sl.Id;
                        StockTransferItem mt = new StockTransferItem();

                        mt.StockTransferId = STId;
                        mt.Item = dataadd.IItemID;
                        mt.Unit = dataadd.IUnitId;
                        mt.Quantity = Convert.ToDecimal(dataadd.ITotalQty);
                        mt.Price = Convert.ToDecimal(dataadd.ISellingPrice);
                        var qty = Convert.ToDecimal(dataadd.ITotalQty);
                        var prc = Convert.ToDecimal(dataadd.ISellingPrice);

                        mt.Amount = qty * prc;

                        db.StockTransferItems.Add(mt);


                        db.SaveChanges();


                    }
                    else if (dataadd.ITotalQty < 0)
                    {

                        Int64 STIdd = sl2.Id;
                        StockTransferItem mt = new StockTransferItem();

                        mt.StockTransferId = STIdd;
                        mt.Item = dataadd.IItemID;
                        mt.Unit = dataadd.IUnitId;
                        mt.Quantity = Convert.ToDecimal(dataadd.ITotalQty) * -1;
                        mt.Price = Convert.ToDecimal(dataadd.ISellingPrice);
                        var qty = Convert.ToDecimal(dataadd.ITotalQty) * -1;
                        var prc = Convert.ToDecimal(dataadd.ISellingPrice);

                        mt.Amount = qty * prc;

                        db.StockTransferItems.Add(mt);


                        db.SaveChanges();
                    }

                }

                var stt = db.StockTransferItems.Where(o => o.StockTransferId == sl.Id).FirstOrDefault();
                if (stt != null)
                {
                    var tottalamout = db.StockTransferItems.Where(o => o.StockTransferId == sl.Id).Sum(o => o.Amount);
                    var sttr = db.StockTransfers.Find(sl.Id);
                    sttr.TotalAmount = tottalamout;
                    db.Entry(sttr).State = EntityState.Modified;
                    db.SaveChanges();
                }
                var stt2 = db.StockTransferItems.Where(o => o.StockTransferId == sl2.Id).FirstOrDefault();
                if (stt2 != null)
                {
                    var tottalamout = db.StockTransferItems.Where(o => o.StockTransferId == sl2.Id).Sum(o => o.Amount);
                    var sttr = db.StockTransfers.Find(sl2.Id);
                    sttr.TotalAmount = tottalamout;
                    db.Entry(sttr).State = EntityState.Modified;
                    db.SaveChanges();
                }

                msg = "MCConversion Successfully Updated...";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
               
                    
                  
                  


                    var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
                    var MCList = MCList1;
                    if (!MCList.Any() && mc == 0)
                    {
                        MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
                    }
                    var MCArray = MCList.ToArray();

                    int recordsTotal = 0;

                    StockTransfer sl2 = new StockTransfer();

                    sl2.STNo = InvoiceNo();
                    sl2.Voucher = "MC-TO- MC-TRANSFER" + sl2.STNo;
                    sl2.Date = today;
                    sl2.MCFrom = mcto;
                    sl2.MCTo = mc;
                    sl2.Remarks = "";
                    sl2.TotalAmount = 0;
                    sl2.CreatedDate = today;

                    sl2.CreatedBy = UserId;
                    sl2.Status = Status.active;
                    sl2.editable = choice.Yes;
                    sl2.Branch = 1;

                    string str = "";
                    StockType Stype2 = StockType.StockTransfer;
                    sl2.StockType = Stype2;
                    sl2.Ref1 = "";
                    sl2.Ref2 = "";
                    sl2.Ref3 = "";
                    sl2.Ref4 = "";
                    sl2.Ref5 = "";

                    db.StockTransfers.Add(sl2);
                    db.SaveChanges();


                    List<StockDetails> data = new List<StockDetails>();
                    mc = mc != null ? mc : 0;
                    var itmids = (from a in db.Items
                                  join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                                  from b in sti.DefaultIfEmpty()
                                  join c in db.StockTransfers on b.StockTransferId equals c.Id
                                  where c.MCTo == mc
                                  select new
                                  {
                                      Itemid = a.ItemID
                                  }).Distinct();

                    var seitems = (from a in db.Items
                                   join b in db.SEItemss on a.ItemID equals b.Item into sti
                                   from b in sti.DefaultIfEmpty()
                                   join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId
                                   where c.MaterialCenter == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                    var peitem = (from a in db.Items
                                  join b in db.PEItemss on a.ItemID equals b.Item into sti
                                  from b in sti.DefaultIfEmpty()
                                  join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                                  where c.MaterialCenter == mc
                                  select new
                                  {
                                      Itemid = a.ItemID
                                  }).Distinct();
                    var serritem = (from a in db.Items
                                    join b in db.SRItemss on a.ItemID equals b.Item into sti
                                    from b in sti.DefaultIfEmpty()
                                    join c in db.SalesReturns on b.SalesReturnId equals c.SalesReturnId
                                    where c.MaterialCenter == mc
                                    select new
                                    {
                                        Itemid = a.ItemID
                                    }).Distinct();
                var assetitemid = (from a in db.Items
                                   join b in db.AssetTransferDetails on a.ItemID equals b.RefItemId into sti
                                   from b in sti.DefaultIfEmpty()
                                   join c in db.AssetTransferMasters on b.AssetEntryId equals c.AssetEntryId
                                   where c.McFromId == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                var assettoinventry = (from a in db.Items
                                       join b in db.AssetToInventoryDetails on a.ItemID equals b.RefItemId into sti
                                       from b in sti.DefaultIfEmpty()
                                       join c in db.AssetToInventoryMasters on b.EntryId equals c.EntryId
                                   where c.McFromId == mc
                                       select new
                                       {
                                           Itemid = a.ItemID
                                       }).Distinct();
                var itmidstkadj = (from a in db.Items
                                   join b in db.StockAdjustments on a.ItemID equals b.ItemID into sti
                                   from b in sti.DefaultIfEmpty()
                                   where b.MaterialCenter == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                itmids = itmids.Union(itmidstkadj);
                itmids = itmids.Union(assettoinventry);
                itmids = itmids.Union(assetitemid);
                itmids = itmids.Union(seitems);
                    itmids = itmids.Union(peitem);
                    itmids = itmids.Union(serritem);
                var itmidsFROM = (from a in db.Items
                                  join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                                  from b in sti.DefaultIfEmpty()
                                  join c in db.StockTransfers on b.StockTransferId equals c.Id
                                  where c.MCFrom == mc
                                  select new
                                  {
                                      Itemid = a.ItemID
                                  }).Distinct();
                itmids = itmids.Union(itmidsFROM);
             
                var itlist = itmids.Select(o => o.Itemid).Distinct().ToList();
                    DateTime ondates = today;
                    foreach (var it in itlist)
                    {
                        var selitem = new SqlParameter("@ItemId", it);
                        var selmc = new SqlParameter("@MCId", mc);
                        var brand = new SqlParameter("@BrandId", "");
                        var stkble = new SqlParameter("@Stockble", 1);
                        var catgry = new SqlParameter("@CategoryId", "");
                        var fromdate = new SqlParameter("@fromdate", "");
                        var todate = new SqlParameter("@todate", ondates);
                        var stype = new SqlParameter("@Stype", "1");

                        var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).FirstOrDefault();

                       
                         if (dataadd.ITotalQty < 0)
                        {

                            Int64 STIdd = sl2.Id;
                            StockTransferItem mt = new StockTransferItem();

                            mt.StockTransferId = STIdd;
                            mt.Item = dataadd.IItemID;
                            mt.Unit = dataadd.IUnitId;
                            mt.Quantity = Convert.ToDecimal(dataadd.ITotalQty) * -1;
                            mt.Price = Convert.ToDecimal(dataadd.ISellingPrice);
                            var qty = Convert.ToDecimal(dataadd.ITotalQty) * -1;
                            var prc = Convert.ToDecimal(dataadd.ISellingPrice);

                            mt.Amount = qty * prc;

                            db.StockTransferItems.Add(mt);


                            db.SaveChanges();
                        }

                    }

              
                    var stt2 = db.StockTransferItems.Where(o => o.StockTransferId == sl2.Id).FirstOrDefault();
                    if (stt2 != null)
                    {
                        var tottalamout = db.StockTransferItems.Where(o => o.StockTransferId == sl2.Id).Sum(o => o.Amount);
                        var sttr = db.StockTransfers.Find(sl2.Id);
                        sttr.TotalAmount = tottalamout;
                        db.Entry(sttr).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    msg = "MCConversion Successfully Updated...";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

            
        }
        public ActionResult Createdamage()
        {
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;
            ViewBag.Item = QkSelect.List(
          new List<SelectListItem>
          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
          }, "Value", "Text", 0);
            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var STTo = new StockTransferViewModel
            {
                Voucher = GetVTNo(),
                Date = DateTime.Now
            };

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");


            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            if (mcchk != null)
            {
                var mcs1 = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MCToBag = QkSelect.List(mcs1, "Id", "Name");
            }
            else
            {
                var mcs1 = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MCToBag = QkSelect.List(mcs1, "Id", "Name");
            }

            return View(STTo);
        }
        private string VoucherNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "StockAdjustment").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.StockAdjustments.Select(p => p.SANo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = companyPrefix + 1;
                }
                else
                {
                    SENo = db.StockAdjustments.Max(p => p.SANo + 1);
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

        private long GetstVchNo()
        {
            Int64 SENo = 0;
            if ((db.StockAdjustments.Select(p => p.SANo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = 1;
            }
            else
            {
                SENo = db.StockAdjustments.Max(p => p.SANo + 1);
            }

            return SENo;
        }
        [HttpPost]
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Create ItemTask")]
        public ActionResult createdatamages(string q,string frmdate, string x, long? cust, long mc,string action, int? chknegative,long itemsel)
        {
            db.SetCommandTimeOut(60 * 60);
            bool stat = false;
            string msg;
            var today = DateTime.Parse(frmdate, new CultureInfo("en-GB"));
            var UserId = User.Identity.GetUserId();


            if (chknegative == 0 || chknegative == null)
            {
                List<StockDetails> data = new List<StockDetails>();
                mc = mc != null ? mc : 0;
                var itmids = (from a in db.Items
                              join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                              from b in sti.DefaultIfEmpty()
                              join c in db.StockTransfers on b.StockTransferId equals c.Id
                              where c.MCTo == mc
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();

                var seitems = (from a in db.Items
                               join b in db.SEItemss on a.ItemID equals b.Item into sti
                               from b in sti.DefaultIfEmpty()
                               join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId
                               where c.MaterialCenter == mc
                               select new
                               {
                                   Itemid = a.ItemID
                               }).Distinct();
                var peitem = (from a in db.Items
                              join b in db.PEItemss on a.ItemID equals b.Item into sti
                              from b in sti.DefaultIfEmpty()
                              join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                              where c.MaterialCenter == mc
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();

                var serritem = (from a in db.Items
                                join b in db.SRItemss on a.ItemID equals b.Item into sti
                                from b in sti.DefaultIfEmpty()
                                join c in db.SalesReturns on b.SalesReturnId equals c.SalesReturnId
                                where c.MaterialCenter == mc
                                select new
                                {
                                    Itemid = a.ItemID
                                }).Distinct();
                var itmidstkadj = (from a in db.Items
                              join b in db.StockAdjustments on a.ItemID equals b.ItemID into sti
                                   from b in sti.DefaultIfEmpty()
                                   where b.MaterialCenter==mc
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();
                itmids = itmids.Union(itmidstkadj);
                itmids = itmids.Union(seitems);
                itmids = itmids.Union(peitem);
                itmids = itmids.Union(serritem);
                var itmidsFROM = (from a in db.Items
                                  join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                                  from b in sti.DefaultIfEmpty()
                                  join c in db.StockTransfers on b.StockTransferId equals c.Id
                                  where c.MCFrom == mc
                                  select new
                                  {
                                      Itemid = a.ItemID
                                  }).Distinct();
                itmids = itmids.Union(itmidsFROM);
                var assetitemid = (from a in db.Items
                                   join b in db.AssetTransferDetails on a.ItemID equals b.RefItemId into sti
                                   from b in sti.DefaultIfEmpty()
                                   join c in db.AssetTransferMasters on b.AssetEntryId equals c.AssetEntryId
                                   where c.McFromId == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                var assettoinventry = (from a in db.Items
                                       join b in db.AssetToInventoryDetails on a.ItemID equals b.RefItemId into sti
                                       from b in sti.DefaultIfEmpty()
                                       join c in db.AssetToInventoryMasters on b.EntryId equals c.EntryId
                                   where c.McFromId == mc
                                       select new
                                       {
                                           Itemid = a.ItemID
                                       }).Distinct();
                itmids = itmids.Union(assettoinventry);
                itmids = itmids.Union(assetitemid);
                var itlist = itmids.Select(o => o.Itemid).Distinct().ToList();
                if (itemsel != 0)
                {
                    List<long> itlistt = new List<long> { itemsel };

                    itlist = itlistt;
                }
                DateTime ondates = today;// System.DateTime.Now.Date.AddYears(2);
               if(1==1)
                {
                    var selitem = new SqlParameter("@ItemId", "");
                   
                    var selmc = new SqlParameter("@MCId", mc);
                   
                    var brand = new SqlParameter("@BrandId", "");
                    var stkble = new SqlParameter("@Stockble", 1);
                    var catgry = new SqlParameter("@CategoryId", "");
                    var fromdate = new SqlParameter("@fromdate", "");
                    var todate = new SqlParameter("@todate", ondates);
                    var stype = new SqlParameter("@Stype", "1");

                    var dataaddf = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

                    foreach (var dataadd in dataaddf)
                    {
                        if (dataadd.ITotalQty > 0)
                        {


                            var date = today;// System.DateTime.Now.AddDays(-1);
                            var stad = new StockAdjustment
                            {
                                VoucherNo = VoucherNo(),
                                SANo = GetstVchNo(),
                                ItemID = dataadd.IItemID,
                                ItemQuantity = (decimal)dataadd.ITotalQty,
                                AdjustmentType = AdjustmentType.Less,
                                Reason = "mc to damage",
                                PurchaseRate = (decimal)dataadd.IPurchasePrice,
                                ItemUnitID = dataadd.IItemUnitID,
                                AdjDate = date,
                                MaterialCenter = mc,
                                CreatedDate = date,
                                CreatedBy = UserId,
                                Branch = Convert.ToInt64(1),
                                Status = Status.active
                            };
                            db.StockAdjustments.Add(stad);
                            db.SaveChanges();



                            Int64 StAdjID = stad.SANo;
                            Int64 AccId = 499;
                            //stock adjustable
                            decimal amount = (decimal)dataadd.IPurchasePrice * (decimal)dataadd.ITotalQty;
                            db.Accountss.Where(a => a.AccountsID == 499).Select(a => a.AccountsID).SingleOrDefault();
                            decimal Stockvalue = amount;
                            com.addAccountTrasaction(0, amount, AccId, "Stock Adjustment", stad.StockAdjustmentId, DC.Credit, date);



                        }
                        else if (dataadd.ITotalQty < 0)
                        {

                            var date = today;// System.DateTime.Now.AddDays(-1);
                            var stad = new StockAdjustment
                            {
                                VoucherNo = VoucherNo(),
                                SANo = GetstVchNo(),
                                ItemID = dataadd.IItemID,
                                ItemQuantity = (decimal)dataadd.ITotalQty * -1,
                                AdjustmentType = AdjustmentType.Add,
                                Reason = "mc to damage",
                                PurchaseRate = (decimal)dataadd.IPurchasePrice,
                                ItemUnitID = dataadd.IItemUnitID,
                                AdjDate = date,
                                MaterialCenter = mc,
                                CreatedDate = date,
                                CreatedBy = UserId,
                                Branch = Convert.ToInt64(1),
                                Status = Status.active
                            };
                            db.StockAdjustments.Add(stad);
                            db.SaveChanges();



                            Int64 StAdjID = stad.SANo;
                            Int64 AccId = 499;
                            //stock adjustable
                            decimal amount = (decimal)dataadd.IPurchasePrice * (decimal)dataadd.ITotalQty;
                            db.Accountss.Where(a => a.AccountsID == 499).Select(a => a.AccountsID).SingleOrDefault();
                            decimal Stockvalue = amount;
                            com.addAccountTrasaction(amount, 0, AccId, "Stock Adjustment", stad.StockAdjustmentId, DC.Debit, date);

                        }
                    }
                }

            }
            else
            {
                List<StockDetails> data = new List<StockDetails>();
                mc = mc != null ? mc : 0;
                var itmids = (from a in db.Items
                              join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                              from b in sti.DefaultIfEmpty()
                              join c in db.StockTransfers on b.StockTransferId equals c.Id
                              where c.MCTo == mc
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();
               

                var seitems = (from a in db.Items
                               join b in db.SEItemss on a.ItemID equals b.Item into sti
                               from b in sti.DefaultIfEmpty()
                               join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId
                               where c.MaterialCenter == mc
                               select new
                               {
                                   Itemid = a.ItemID
                               }).Distinct();
                var peitem = (from a in db.Items
                              join b in db.PEItemss on a.ItemID equals b.Item into sti
                              from b in sti.DefaultIfEmpty()
                              join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                              where c.MaterialCenter == mc
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();
                var serritem = (from a in db.Items
                                join b in db.SRItemss on a.ItemID equals b.Item into sti
                                from b in sti.DefaultIfEmpty()
                                join c in db.SalesReturns on b.SalesReturnId equals c.SalesReturnId
                                where c.MaterialCenter == mc
                                select new
                                {
                                    Itemid = a.ItemID
                                }).Distinct();
                var itmidsFROM = (from a in db.Items
                              join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                              from b in sti.DefaultIfEmpty()
                              join c in db.StockTransfers on b.StockTransferId equals c.Id
                              where c.MCFrom == mc
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();
                var itmidstkadj = (from a in db.Items
                                   join b in db.StockAdjustments on a.ItemID equals b.ItemID into sti
                                   from b in sti.DefaultIfEmpty()
                                   where b.MaterialCenter == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                itmids = itmids.Union(itmidstkadj);
                itmids = itmids.Union(itmidsFROM);
                itmids = itmids.Union(seitems);
                itmids = itmids.Union(peitem);
                itmids = itmids.Union(serritem);
                var assetitemid = (from a in db.Items
                                   join b in db.AssetTransferDetails on a.ItemID equals b.RefItemId into sti
                                   from b in sti.DefaultIfEmpty()
                                   join c in db.AssetTransferMasters on b.AssetEntryId equals c.AssetEntryId
                                   where c.McFromId == mc
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
                var assettoinventry = (from a in db.Items
                                       join b in db.AssetToInventoryDetails on a.ItemID equals b.RefItemId into sti
                                       from b in sti.DefaultIfEmpty()
                                       join c in db.AssetToInventoryMasters on b.EntryId equals c.EntryId
                                   where c.McFromId == mc
                                       select new
                                       {
                                           Itemid = a.ItemID
                                       }).Distinct();
                itmids = itmids.Union(assettoinventry);
                itmids = itmids.Union(assetitemid);
                var itlist = itmids.Select(o => o.Itemid).Distinct().ToList();
                if (itemsel != 0)
                {
                    List<long> itlistt = new List<long> { itemsel };
                    
                    itlist = itlistt;
                }
                DateTime ondates = today;// System.DateTime.Now.Date.AddYears(2);
                var selitem = new SqlParameter("@ItemId", "");
                var selmc = new SqlParameter("@MCId", mc);
                long newmc = 0;
                if (itlist.Count() == 1)
                    selmc = new SqlParameter("@MCId", newmc);
                var brand = new SqlParameter("@BrandId", "");
                var stkble = new SqlParameter("@Stockble", 1);
                var catgry = new SqlParameter("@CategoryId", "");
                var fromdate = new SqlParameter("@fromdate", "");
                var todate = new SqlParameter("@todate", ondates);
                var stype = new SqlParameter("@Stype", "1");
                var dataaddf = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

                foreach (var dataadd in dataaddf)
                {
                    



                    if (dataadd.ITotalQty < 0)
                    {

                        var date = today;// System.DateTime.Now.AddDays(-1);
                        var stad = new StockAdjustment
                        {
                            VoucherNo = VoucherNo(),
                            SANo = GetstVchNo(),
                            ItemID = dataadd.IItemID,
                            ItemQuantity = (decimal)dataadd.ITotalQty * -1,
                            AdjustmentType = AdjustmentType.Add,
                            Reason = "mc to damage",
                            PurchaseRate = (decimal)dataadd.IPurchasePrice,
                            ItemUnitID = dataadd.IItemUnitID,
                            AdjDate = date,
                            MaterialCenter = mc,
                            CreatedDate = date,
                            CreatedBy = UserId,
                            Branch = Convert.ToInt64(1),
                            Status = Status.active
                        };
                        db.StockAdjustments.Add(stad);
                        db.SaveChanges();



                        Int64 StAdjID = stad.SANo;
                        Int64 AccId = 499;
                        //stock adjustable
                        decimal amount = (decimal)dataadd.IPurchasePrice * (decimal)dataadd.ITotalQty;
                        db.Accountss.Where(a => a.AccountsID == 499).Select(a => a.AccountsID).SingleOrDefault();
                        decimal Stockvalue = amount;
                        com.addAccountTrasaction(amount, 0, AccId, "Stock Adjustment", stad.StockAdjustmentId, DC.Debit, date);

                    }
                }
                

            }

            msg = "MCConversion Successfully Updated...";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
          
        


        }

        private bool BillExist(string STNo)
        {
            var Exists = db.StockTransfers.Any(c => c.Voucher == (STNo));
            bool res = (Exists) ? true : false;
            return res;

        }
        private long InvoiceNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "StockTransfer").Select(a => a.number).FirstOrDefault();
            if ((db.StockTransfers.Select(p => p.STNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                SENo = db.StockTransfers.Max(p => p.STNo + 1);
            }
            return SENo;
        }
        private string GetVTNo(Int64 SENo = 0, string billNo = "")
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "StockTransfer").Select(a => a.prefix).FirstOrDefault();
            if (billNo == "" || billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "StockTransfer").Select(a => a.number).FirstOrDefault();
                if ((db.StockTransfers.Select(p => p.STNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.StockTransfers.Max(p => p.STNo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = GetVTNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = GetVTNo(SENo, billNo);
                }

            }
            return billNo;
        }

    }
}
