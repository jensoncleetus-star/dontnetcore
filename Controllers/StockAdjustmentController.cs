using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class StockAdjustmentController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public StockAdjustmentController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: StockAdjustment
        [QkAuthorize(Roles = "Dev,Stock Adjustment")]
        public ActionResult Index()
        {
            var nameitem = (from a in db.StockAdjustments
                            join b in db.Items on a.ItemID equals b.ItemID
                            select(new
                            {
                                ID = a.ItemID,
                                Name = b.ItemName
                            })).ToList();
            ViewBag.Name = QkSelect.List(nameitem, "ID", "Name");
            ViewBag.AdjustmentType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Less", Value="0"},
                new SelectListItem() {Text = "Add", Value="1"},
            }, "Value", "Text");
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Adjustment")]
        public JsonResult GetStockAdjustment(long? Name, decimal? Quantity, string AdjType)
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
            AdjustmentType At=new AdjustmentType();
            if (AdjType != "")
            {
                At = (AdjType == "0") ? AdjustmentType.Less : AdjustmentType.Add;
            };

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Stock Adjustment");
            var uDelete = User.IsInRole("Delete Stock Adjustment");

            var v = (from a in db.StockAdjustments
                     join b in db.Items on a.ItemID equals b.ItemID into itm
                     from b in itm.DefaultIfEmpty()
                     join c in db.ItemUnits on a.ItemUnitID equals c.ItemUnitID into unit
                     from c in unit.DefaultIfEmpty()
                     where
                     (Name == null || a.ItemID == Name) && (AdjType == "" || a.AdjustmentType == At) &&
                     (Quantity == null || a.ItemQuantity == Quantity)
                     select new
                     {
                         id = a.StockAdjustmentId,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Quantity = a.ItemQuantity,
                         Unit = c.ItemUnitName,
                         Date = a.AdjDate,
                         Type = a.AdjustmentType,
                         a.PurchaseRate,
                         a.Reason,
                         a.VoucherNo,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         a.CreatedDate
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => /*p.id.ToString().ToLower().Contains(search.ToLower()) ||*/
                                     p.Item.ToString().ToLower().Contains(search.ToLower()));
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
        [HttpGet]

        public ActionResult Createadd()
        {

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcs = db.MCs.Where(o=>o.MCId== 20017).Select(s => new
            {
                Id = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            ViewBag.LastEntry = db.StockAdjustments.Select(p => p.StockAdjustmentId).AsEnumerable().DefaultIfEmpty(0).Max();
            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            StockAdjustmentViewModel stk = new StockAdjustmentViewModel();
            stk.VoucherNo = VoucherNo();
            stk.AdjDate = (System.DateTime.Now).ToString("dd-MM-yyyy");
            _FinancialYear();
            return View(stk);
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create Stock Adjustment")]
        public ActionResult Create()
        {

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcs = db.MCs.Select(s => new
            {
                Id = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            ViewBag.LastEntry = db.StockAdjustments.Select(p => p.StockAdjustmentId).AsEnumerable().DefaultIfEmpty(0).Max();
            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            StockAdjustmentViewModel stk = new StockAdjustmentViewModel();
            stk.VoucherNo = VoucherNo();
            stk.AdjDate = (System.DateTime.Now).ToString("dd-MM-yyyy");
            _FinancialYear();
            return View(stk);
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create Stock Adjustment")]
        public ActionResult Createtwo()
        {

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcs = db.MCs.Select(s => new
            {
                Id = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            ViewBag.LastEntry = db.StockAdjustments.Select(p => p.StockAdjustmentId).AsEnumerable().DefaultIfEmpty(0).Max();
            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            StockAdjustmentViewModel stk = new StockAdjustmentViewModel();
            stk.VoucherNo = VoucherNo();
            stk.AdjDate = (System.DateTime.Now).ToString("dd-MM-yyyy");
            _FinancialYear();
            return View(stk);
        }


        [HttpPost]

        public ActionResult Createtwo(StockAdjustmentViewModel stkadj)
        {

            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                var Date = DateTime.Parse(stkadj.AdjDate.ToString(), new CultureInfo("en-GB"));

                var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
                var CurrentDate = Convert.ToDateTime(System.DateTime.Now);
                long MC = 0;

                if (MCcheck == Status.active)
                {
                    MC = Convert.ToInt64(stkadj.MaterialCenter);
                }
                else
                {
                    MC = 1;
                }

                var stad = new StockAdjustment
                {
                    VoucherNo = stkadj.VoucherNo,
                    SANo = GetVchNo(),
                    ItemID = stkadj.ItemID,
                    ItemQuantity = stkadj.ItemQuantity,
                    AdjustmentType = stkadj.AdjustmentType,
                    Reason = stkadj.Reason,
                    PurchaseRate = stkadj.PurchaseRate,
                    ItemUnitID = stkadj.ItemUnitID,
                    AdjDate = Date,
                    MaterialCenter = MC,
                    CreatedDate = CurrentDate,
                    CreatedBy = UserId,
                    Branch = Convert.ToInt64(BranchID),
                    Status = Status.active
                };
                db.StockAdjustments.Add(stad);
                db.SaveChanges();
                if (stkadj.ItemIDD != null && stkadj.ItemIDD != 0)
                {
                    var stad2 = new StockAdjustment
                    {
                        VoucherNo = stkadj.VoucherNo,
                        SANo = GetVchNo(),
                        ItemID = (long)stkadj.ItemIDD,
                        ItemQuantity =Convert.ToDecimal(stkadj.ItemQuantityTo),
                        AdjustmentType = (stkadj.AdjustmentType == AdjustmentType.Add) ? AdjustmentType.Less : AdjustmentType.Add,
                        Reason = stkadj.Reason,
                        PurchaseRate = stkadj.PurchaseRate,
                        ItemUnitID = stkadj.ItemUnitIDD,
                        AdjDate = Date,
                        MaterialCenter = MC,
                        CreatedDate = CurrentDate,
                        CreatedBy = UserId,
                        Branch = Convert.ToInt64(BranchID),
                        Status = Status.active
                    };
                    db.StockAdjustments.Add(stad2);
                    db.SaveChanges();
                }
                ItemTransactionInCreateMode(MC, stkadj, UserId, CurrentDate);

                Int64 StAdjID = stad.SANo;
                Int64 AccId = 499;
                //stock adjustable
                decimal amount = (stkadj.PurchaseRate * stkadj.ItemQuantity);
                db.Accountss.Where(a => a.AccountsID == 499).Select(a => a.AccountsID).SingleOrDefault();
                decimal Stockvalue = Convert.ToDecimal(stkadj.ItemQuantity * stkadj.PurchaseRate);
                if (stkadj.AdjustmentType == AdjustmentType.Add)
                {
                    ////com.addStock(stkadj.ItemQuantity, 0, stkadj.ItemID, stkadj.ItemUnitID, "Stock Adjustment-Add", StAdjID, stkadj.PurchaseRate, Stockvalue, Date, stkadj.MaterialCenter, Status.active);
                }
                if (stkadj.AdjustmentType == AdjustmentType.Less)
                {
                }

                // batch stock

                if (stkadj.bstmodel != null)
                {
                    foreach (var bst in stkadj.bstmodel)
                    {
                        if (bst.BatchNo != "" && bst.BatchNo != null)
                        {
                            DateTime? exp = null;
                            DateTime? mfg = null;
                            if (bst.EXP != null && bst.EXP != "")
                            {
                                exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                            }
                            if (bst.MFG != null && bst.MFG != "")
                            {
                                mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                            }
                            decimal bStockOut = 0;
                            decimal bStockIn = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStockOut = bst.StockOut * bst.cfactor;
                                bStockIn = bst.StockIn * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockOut = bStockOut;
                            Btst.StockIn = bStockIn;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = stad.StockAdjustmentId;
                            Btst.Type = "Adjustment";

                            Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            Btst.Date = Date;


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }

                com.addlog(LogTypes.Created, UserId, "StockAdjustment", "StockAdjustments", findip(), stad.StockAdjustmentId, "Successfully Created Stock Adjustment");


                Success("Successfully Create Stock Adjustment.", true);
                return RedirectToAction("Index", "Home");

                //    Warning("This Item Is Going To Out of Stock!!!", true);
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return (View());
            }

        }


        [HttpPost]
       
        public ActionResult Create(StockAdjustmentViewModel stkadj)
        {

            if (ModelState.IsValid)
            {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var Date = DateTime.Parse(stkadj.AdjDate.ToString(), new CultureInfo("en-GB"));

                    var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                    var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
                    var CurrentDate = Convert.ToDateTime(System.DateTime.Now);
                    long MC = 0;                    

                    if (MCcheck == Status.active)
                    {
                        MC = Convert.ToInt64(stkadj.MaterialCenter);
                    }
                    else
                    {
                        MC = 1;
                    }

                    var stad = new StockAdjustment
                    {
                        VoucherNo = stkadj.VoucherNo,
                        SANo = GetVchNo(),
                        ItemID = stkadj.ItemID,
                        ItemQuantity = stkadj.ItemQuantity,
                        AdjustmentType = stkadj.AdjustmentType,
                        Reason = stkadj.Reason,
                        PurchaseRate = stkadj.PurchaseRate,
                        ItemUnitID = stkadj.ItemUnitID,
                        AdjDate = Date,
                        MaterialCenter = MC,
                        CreatedDate = CurrentDate,
                        CreatedBy = UserId,
                        Branch = Convert.ToInt64(BranchID),
                        Status = Status.active
                    };
                    db.StockAdjustments.Add(stad);
                    db.SaveChanges();

                ItemTransactionInCreateMode(MC, stkadj, UserId, CurrentDate);

                Int64 StAdjID = stad.SANo;
                Int64 AccId = 499;
                //stock adjustable
                decimal amount = (stkadj.PurchaseRate * stkadj.ItemQuantity);
                db.Accountss.Where(a => a.AccountsID == 499).Select(a => a.AccountsID).SingleOrDefault();
                decimal Stockvalue = Convert.ToDecimal(stkadj.ItemQuantity * stkadj.PurchaseRate);
                if (stkadj.AdjustmentType == AdjustmentType.Add)
                {
                    ////com.addStock(stkadj.ItemQuantity, 0, stkadj.ItemID, stkadj.ItemUnitID, "Stock Adjustment-Add", StAdjID, stkadj.PurchaseRate, Stockvalue, Date, stkadj.MaterialCenter, Status.active);
                }
                if (stkadj.AdjustmentType == AdjustmentType.Less)
                {
                }

                // batch stock

                if (stkadj.bstmodel != null)
                    {
                        foreach (var bst in stkadj.bstmodel)
                        {
                            if (bst.BatchNo != "" && bst.BatchNo != null)
                            {
                                DateTime? exp = null;
                                DateTime? mfg = null;
                                if (bst.EXP != null && bst.EXP != "")
                                {
                                    exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                                }
                                if (bst.MFG != null && bst.MFG != "")
                                {
                                    mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                                }
                                decimal bStockOut = 0;
                                decimal bStockIn = 0;
                                if (bst.Unit == bst.Priunit)
                                {
                                    bStockOut = bst.StockOut * bst.cfactor;
                                    bStockIn = bst.StockIn * bst.cfactor;
                                }
                                BatchStock Btst = new BatchStock();
                                Btst.BatchNo = bst.BatchNo;
                                Btst.Item = bst.Item;
                                Btst.Unit = bst.Unit;
                                Btst.Cost = bst.Cost;
                                Btst.StockOut = bStockOut;
                                Btst.StockIn = bStockIn;
                                Btst.Order = bst.Order;
                                Btst.EXP = exp;
                                Btst.MFG = mfg;
                                Btst.Reference = stad.StockAdjustmentId;
                                Btst.Type = "Adjustment";

                                Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                Btst.Date = Date;


                                db.BatchStocks.Add(Btst);
                            }
                        }
                        db.SaveChanges();
                    }

                    com.addlog(LogTypes.Created, UserId, "StockAdjustment", "StockAdjustments", findip(), stad.StockAdjustmentId, "Successfully Created Stock Adjustment");


                    Success("Successfully Create Stock Adjustment.", true);
                    return RedirectToAction("Index", "Home");

                //    Warning("This Item Is Going To Out of Stock!!!", true);
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return (View());
            }

        }


        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Create Stock Adjustment")]
        public ActionResult Createassetadj()
        {  
            //For Dropdown Assets 
            ViewBag.Assets = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = null, Value = null},
                      }, "Value", "Text", 1);          

            ViewBag.LastEntry = db.AssetAdjustments.Select(p => p.AssetAdjustmentId).AsEnumerable().DefaultIfEmpty(0).Max();
         
            AssetAdjustmentViewModel vModel = new AssetAdjustmentViewModel();
            vModel.AdjustmentNo     = GetAdjustmentNo();
            vModel.AdjustmentDate   = (System.DateTime.Now).ToString("dd-MM-yyyy");

            _FinancialYear();
            return View(vModel);
        }

        [HttpPost]
       // [QkAuthorize(Roles = "Dev,Create Stock Adjustment")]
        public ActionResult Createassetadj(AssetAdjustmentViewModel AsstAdj)
        {

            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                var Date = DateTime.Parse(AsstAdj.AdjustmentDate.ToString(), new CultureInfo("en-GB"));

                var AdjObj = new AssetAdjustments
                {
                    AdjustmentNo    =   AsstAdj.AdjustmentNo,
                    AssetId         =   AsstAdj.AssetId,
                    AssetQuantity   =   AsstAdj.AssetQuantity,
                    AdjustmentType  =   AsstAdj.AdjustmentType,
                    Reason          =   AsstAdj.Reason,
                    PurchaseRate    =   AsstAdj.PurchaseRate,
                    AssetUnitId     =   AsstAdj.AssetUnitId,
                    AdjustmentDate  =   Date,
                    AssetAccountId  =   AsstAdj.AssetAccountId,
                    CreatedDate     =   Convert.ToDateTime(System.DateTime.Now),
                    CreatedBy       =   UserId,
                    Branch          =   Convert.ToInt64(BranchID),
                    Status          =   Status.active
                };
                db.AssetAdjustments.Add(AdjObj);
                db.SaveChanges();

                Int64 AdjID = AdjObj.AssetAdjustmentId;
                Int64 AccId = 555;

               // stock adjustable
                decimal amount = (AsstAdj.PurchaseRate * AsstAdj.AssetQuantity);

                if (AsstAdj.AdjustmentType == AdjustmentType.Add)
                {
                }
                if (AsstAdj.AdjustmentType == AdjustmentType.Less)
                {
                }

                com.addlog(LogTypes.Created, UserId, "AssetAdjustment", "AssetAdjustments", findip(), AdjID, "Successfully Created Asset Adjustment");

                Success("Successfully Created Asset Adjustment.", true);
                return RedirectToAction("Createassetadj", "StockAdjustment");
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return (View());
            }

        }

        public bool checkStock(StockAdjustmentViewModel stkadj)
        {

            bool retval = true;
            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where b.ItemID == stkadj.ItemID
                        select new
                        {
                            b.ItemUnit,
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

                            //for min stock check
                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                           

                        }).AsEnumerable().Select(o => new
                        {
                            o.ItemID,
                            o.ItemUnit,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
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

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj) - (o.PriSale + o.PriPReturn + o.PriLessAdj)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj) - (o.SubSale + o.SubPReturn + o.subLessAdj)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj) - (o.PriSale + o.PriPReturn + o.PriLessAdj)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj) - (o.SubSale + o.SubPReturn + o.subLessAdj)),


                        }).FirstOrDefault();




            if (item.KeepStock == true)
            {
                decimal qntmin = 0;
                decimal currentQty = 0;
                var minstock = item.MinStock * item.ConFactor;
                decimal totstock = 0;

                if (stkadj.ItemUnitID == item.ItemUnitID)
                {
                    qntmin = stkadj.ItemQuantity * item.ConFactor;
                    totstock = Convert.ToDecimal(item.total) + Convert.ToDecimal(qntmin);

                    currentQty = stkadj.ItemQuantity * item.ConFactor;

                    currentQty = currentQty - stkadj.ItemQuantity;

                    totstock = totstock - (currentQty);
                    minstock = minstock / item.ConFactor;
                    totstock = totstock / item.ConFactor;

                    var totalstock = totstock - stkadj.ItemQuantity;
                    var totalstock1 = totalstock / item.ConFactor;

                    if (totalstock1 < 0)
                    {
                        //out of stock
                        retval = false;
                    }

                }
                if (stkadj.ItemUnitID == item.SubUnitId)
                {
                    qntmin = stkadj.ItemQuantity;
                    totstock = Convert.ToDecimal(item.total + qntmin);

                    currentQty = stkadj.ItemQuantity;

                    currentQty = currentQty - stkadj.ItemQuantity;

                    totstock = totstock - (currentQty);
                    var totalstock = totstock - stkadj.ItemQuantity;
                    if (totalstock < 0)
                    {
                        retval = false;
                    }
                }

            }
            return retval;
        }


        [QkAuthorize(Roles = "Dev,Edit Stock Adjustment")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockAdjustment stkad = db.StockAdjustments.Find(id);

            if (stkad == null)
            {
                return NotFound();
            }
            StockAdjustmentViewModel stk = new StockAdjustmentViewModel();
            stk.VoucherNo = stkad.VoucherNo;
            stk.ItemID = stkad.ItemID;
            stk.ItemQuantity = stkad.ItemQuantity;
            stk.ItemUnitID = stkad.ItemUnitID;
            stk.PurchaseRate = stkad.PurchaseRate;
            stk.Reason = stkad.Reason;
            stk.AdjustmentType = stkad.AdjustmentType;
            stk.AdjDate = stkad.AdjDate.ToString("dd-MM-yyyy");
            stk.MaterialCenter = stkad.MaterialCenter;

            var itm = db.Items.Select(s => new
            {
                ID = s.ItemID,
                Name = s.ItemCode + " " + s.ItemName
            }).ToList();
            ViewBag.Item = QkSelect.List(itm, "ID", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcs = db.MCs.Select(s => new
            {
                Id = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            ViewBag.preEntry = db.StockAdjustments.Where(a => a.StockAdjustmentId < id).Select(a => a.StockAdjustmentId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.StockAdjustments.Where(a => a.StockAdjustmentId > id).Select(a => a.StockAdjustmentId).DefaultIfEmpty().Min();
            _FinancialYear();
            return View(stk);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Stock Adjustment")]
        public ActionResult Edit(StockAdjustmentViewModel stkadj, long id)
        {
            foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateEntry modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    var s = error.ErrorMessage;
                }
            }
            if (ModelState.IsValid)
            {
               
                //        ID = s.ItemID,
                //        Name = s.ItemCode + " " + s.ItemName
                //    Warning("This Item Is Going To Out of Stock!!!", true);

                var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
                long MC = 0;              

                var UserId = User.Identity.GetUserId();
                var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

                if (MCcheck == Status.active)
                {
                    MC = Convert.ToInt64(stkadj.MaterialCenter);
                }
                else
                {
                    MC = 1;
                }
                var Date = DateTime.Parse(stkadj.AdjDate.ToString(), new CultureInfo("en-GB"));

                StockAdjustment stad = db.StockAdjustments.Find(id);

                ItemTransactionInEditMode(stad, id, MC, stkadj, UserId, CurrentDate);               

                stad.ItemID = stkadj.ItemID;
                stad.ItemQuantity = stkadj.ItemQuantity;
                stad.AdjustmentType = stkadj.AdjustmentType;
                stad.Reason = stkadj.Reason;
                stad.PurchaseRate = stkadj.PurchaseRate;
                stad.ItemUnitID = stkadj.ItemUnitID;
                stad.AdjDate = Date;
                stad.MaterialCenter = MC;
                db.Entry(stad).State = EntityState.Modified;    
                db.SaveChanges();

                bool delete = com.DeleteAllAccountTransaction("Stock Adjustment", stad.StockAdjustmentId);
                Int64 AccId = 499;
                Int64 StAdjID = stad.SANo;
                decimal amount = (stkadj.PurchaseRate * stkadj.ItemQuantity);
                db.Accountss.Where(a => a.AccountsID == 499).Select(a => a.AccountsID).SingleOrDefault();
                decimal Stockvalue = Convert.ToDecimal(stkadj.ItemQuantity * stkadj.PurchaseRate);
                if (stkadj.AdjustmentType == AdjustmentType.Add)
                {
                }
                if (stkadj.AdjustmentType == AdjustmentType.Less)
                {
                }


                // batch stock

                var SEBst = db.BatchStocks.Where(a => a.Reference == stad.StockAdjustmentId && a.Type == "Adjustment").FirstOrDefault();
                if (SEBst != null)
                {
                    db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == stad.StockAdjustmentId && a.Type == "Adjustment"));
                    db.SaveChanges();
                }
                if (stkadj.bstmodel != null)
                {
                    foreach (var bst in stkadj.bstmodel)
                    {
                        if (bst.BatchNo != "" && bst.BatchNo != null)
                        {
                            DateTime? exp = null;
                            DateTime? mfg = null;
                            if (bst.EXP != null && bst.EXP != "")
                            {
                                exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                            }
                            if (bst.MFG != null && bst.MFG != "")
                            {
                                mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                            }
                            decimal bStockOut = 0;
                            decimal bStockIn = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStockOut = bst.StockOut * bst.cfactor;
                                bStockIn = bst.StockIn * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockOut = bStockOut;
                            Btst.StockIn = bStockIn;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = stad.StockAdjustmentId;
                            Btst.Type = "Adjustment";

                            Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            Btst.Date = Date;


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                
                com.addlog(LogTypes.Updated, UserId, "StockAdjustment", "StockAdjustments", findip(), stad.StockAdjustmentId, "Stock Adjustment Updated Successfully");

                Success("Successfully Updated Stock Adjustment.", true);
                return RedirectToAction("Edit/" + id, "StockAdjustment");
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return RedirectToAction("Edit/" + id, "StockAdjustment");
            }

        }

        [QkAuthorize(Roles = "Dev,Delete Stock Adjustment")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockAdjustment stk = db.StockAdjustments.Find(id);
            if (stk == null)
            {
                return NotFound();
            }
            return PartialView(stk);
        }

        [QkAuthorize(Roles = "Dev,Delete Stock Adjustment")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            //
            var UserId = User.Identity.GetUserId();
            StockAdjustment stk = db.StockAdjustments.Find(id);

            /*ItemTransaction in Delete Mode*/
            if (stk != null)
            {
                ItemTransactionInDeleteMode(stk, id, UserId);
            }
            bool delete = com.DeleteAllAccountTransaction("Stock Adjustment", stk.StockAdjustmentId);

            db.StockAdjustments.Remove(stk);
            db.SaveChanges();

            // batch stock

            var SEBst = db.BatchStocks.Where(a => a.Reference == stk.StockAdjustmentId && a.Type == "Adjustment").FirstOrDefault();
            if (SEBst != null)
            {
                db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == stk.StockAdjustmentId && a.Type == "Adjustment"));
                db.SaveChanges();
            }

            com.addlog(LogTypes.Deleted, UserId, "StockAdjustment", "StockAdjustments", findip(), stk.StockAdjustmentId, "Stock Adjustment Deleted Successfully");

            stat = true;
            msg = "Successfully deleted Stock Adjustment.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

       // [QkAuthorize(Roles = "Dev,Edit Asset Adjustment")]
        public ActionResult EditAssetAdjustment(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AssetAdjustments AsstAdj = db.AssetAdjustments.Find(id);

            if (AsstAdj == null)
            {
                return NotFound();
            }

            AssetAdjustmentViewModel vModel = new AssetAdjustmentViewModel();
            vModel.AdjustmentNo     =   AsstAdj.AdjustmentNo;
            vModel.AssetId          =   AsstAdj.AssetId;
            vModel.AssetQuantity    =   AsstAdj.AssetQuantity;
            vModel.AssetUnitId      =   AsstAdj.AssetUnitId;
            vModel.PurchaseRate     =   AsstAdj.PurchaseRate;
            vModel.Reason           =   AsstAdj.Reason;
            vModel.AdjustmentType   =   AsstAdj.AdjustmentType;
            vModel.AdjustmentDate   =   AsstAdj.AdjustmentDate.ToString("dd-MM-yyyy");
            vModel.AssetAccountId   =   AsstAdj.AssetAccountId;

            //For Dropdown Asset Accounts
            var AssetAccount = db.Accountss.Where(s => s.AccountsID == AsstAdj.AssetAccountId).
                Select(s => new
                {
                    Id = s.AccountsID,
                    Name = s.Name
                }).ToList();
            ViewBag.AssetAccounts = QkSelect.List(AssetAccount, "Id", "Name");

            //For Dropdown Assets    
            var Asset = db.AssetTransferDetails.Select(s => new
            {
                Id = s.AssetitementryId,
                Name = s.AssetName
            }).ToList();
            ViewBag.Assets = QkSelect.List(Asset, "Id", "Name");          

            ViewBag.preEntry = db.AssetAdjustments.Where(a => a.AssetAdjustmentId < id).Select(a => a.AssetAdjustmentId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.AssetAdjustments.Where(a => a.AssetAdjustmentId > id).Select(a => a.AssetAdjustmentId).DefaultIfEmpty().Min();
           
            _FinancialYear();
            return View(vModel);
        }

        [HttpPost]
       // [QkAuthorize(Roles = "Dev,Edit Asset Adjustment")]
        public ActionResult EditAssetAdjustment(AssetAdjustmentViewModel AssetAdj, long id)
        {
            if (ModelState.IsValid)
            {

                var UserId = User.Identity.GetUserId();
                var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

                var Date = DateTime.Parse(AssetAdj.AdjustmentDate.ToString(), new CultureInfo("en-GB"));

                AssetAdjustments AdjObj = db.AssetAdjustments.Find(id);
                
                AdjObj.AssetId          =   AssetAdj.AssetId;
                AdjObj.AssetQuantity    =   AssetAdj.AssetQuantity;
                AdjObj.AdjustmentType   =   AssetAdj.AdjustmentType;
                AdjObj.Reason           =   AssetAdj.Reason;
                AdjObj.PurchaseRate     =   AssetAdj.PurchaseRate;
                AdjObj.AssetUnitId      =   AssetAdj.AssetUnitId;
                AdjObj.AdjustmentDate   =   Date;
                AdjObj.AssetAccountId   =   AssetAdj.AssetAccountId;
                db.Entry(AdjObj).State  =   EntityState.Modified;
                db.SaveChanges();

                bool delete = com.DeleteAllAccountTransaction("Asset Adjustment", AdjObj.AssetAdjustmentId);
                Int64 AccId = 555;

                Int64 AdjId = AdjObj.AssetAdjustmentId;
                decimal amount = (AssetAdj.PurchaseRate * AssetAdj.AssetQuantity);
                
                if (AssetAdj.AdjustmentType == AdjustmentType.Add)
                {
                    com.addAccountTrasaction(amount, 0, AccId, "Asset Adjustment", AdjObj.AssetAdjustmentId, DC.Debit, Date);
                }
                if (AssetAdj.AdjustmentType == AdjustmentType.Less)
                {
                    com.addAccountTrasaction(0, amount, AccId, "Asset Adjustment", AdjObj.AssetAdjustmentId, DC.Credit, Date);
                }           

                com.addlog(LogTypes.Updated, UserId, "AssetAdjustment", "AssetAdjustments", findip(), AdjObj.AssetAdjustmentId, "Asset Adjustment Updated Successfully");

                Success("Successfully Updated Asset Adjustment.", true);
                return RedirectToAction("EditAssetAdjustment/" + id, "StockAdjustment");
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return RedirectToAction("EditAssetAdjustment/" + id, "StockAdjustment");
            }

        }

        //Asset -- DELETE
        //[QkAuthorize(Roles = "Dev,Delete Stock Adjustment")]       
        public ActionResult DeleteAsset(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssetAdjustments AssetObj = db.AssetAdjustments.Find(id);
            if (AssetObj == null)
            {
                return NotFound();
            }
            return PartialView(AssetObj);
        }


        //[QkAuthorize(Roles = "Dev,Delete Stock Adjustment")]
        [HttpPost, ActionName("DeleteAsset")]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteAssetConfirmed(long id)
        {
            bool stat = false;
            string msg;
           
            var UserId = User.Identity.GetUserId();
            AssetAdjustments AssetObj = db.AssetAdjustments.Find(id);
           
            if (AssetObj != null)
            {
                bool delete = com.DeleteAllAccountTransaction("Asset Adjustment", AssetObj.AssetAdjustmentId);

                db.AssetAdjustments.Remove(AssetObj);
                db.SaveChanges();
            }
                        
            com.addlog(LogTypes.Deleted, UserId, "AssetAdjustment", "AssetAdjustments", findip(), AssetObj.AssetAdjustmentId, "Asset Adjustment Deleted Successfully");

            stat = true;
            msg = "Successfully deleted Asset Adjustment.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        private long GetVchNo()
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
        private bool BillExist(string SENo)
        {
            var Exists = db.StockAdjustments.Any(c => c.VoucherNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }


        [HttpGet]
        public ActionResult GetAdjById(int StkId)
        {
            var stk = (from a in db.StockAdjustments
                       join b in db.Items on a.ItemID equals b.ItemID
                       join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                       from c in primary.DefaultIfEmpty()
                       join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                       from d in second.DefaultIfEmpty()
                       where a.StockAdjustmentId == StkId
                       select new
                       {
                           a.ItemID,
                           a.ItemUnitID,
                           priunitId = b.ItemUnitID,
                           b.SubUnitId,
                           PriUnit = c.ItemUnitName,
                           SubUnit = d.ItemUnitName,
                           b.ConFactor,
                           b.KeepStock,
                           ItemName = b.ItemName,
                           Unit = a.ItemUnitID,
                           Type = a.AdjustmentType,
                           b.PurchasePrice,
                           b.SellingPrice,
                           b.MRP,
                           batch = (from ay in db.BatchStocks
                                    join az in db.StockAdjustments on new { f1 = ay.Item, f2 = ay.Unit, f3 = ay.Reference, f4 = ay.Type }
                                         equals new { f1 = az.ItemID, f2 = az.ItemUnitID, f3 = az.StockAdjustmentId, f4 = "Adjustment" }
                                    where az.StockAdjustmentId == StkId && ay.Item == a.ItemID
                                    select new BatchStockPViewModel
                                    {
                                        BatchNo = ay.BatchNo,
                                        MFGd = ay.MFG,
                                        EXPd = ay.EXP,
                                        StockIn = ay.StockIn,
                                        StockOut = ay.StockOut,
                                        Item = ay.Item,
                                        cfactor = b.ConFactor,
                                        Priunit = b.ItemUnitID,
                                        Secunit = b.SubUnitId,
                                        Unit = ay.Unit,
                                        Cost = ay.Cost,
                                        origin = "Adjustment",
                                        Order = ay.Order
                                    }).ToList()
                       }).FirstOrDefault();
            return Json(stk);

        }
        public JsonResult SearchassetItem(string q, string x, string page,string assetaccount)
        {
            List<SelectFormat> serialisedJson;
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            long assac = Convert.ToInt64(assetaccount);
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AssetTransferDetails.Where(p => p.AssetAccountId== assac&& p.AssetName.ToLower().Contains(q.ToLower()) || p.AssetName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.AssetName, //each json object will have 
                                      id = b.AssetitementryId
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.AssetTransferDetails.Where(p => p.AssetAccountId == assac ).Select(b => new SelectFormat
                {
                    text = b.AssetName, //each json object will have 
                    id = b.AssetitementryId
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchVoucher(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.StockAdjustments.Where(p => p.VoucherNo.ToLower().Contains(q.ToLower()) || p.VoucherNo.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.VoucherNo, //each json object will have 
                                      id = b.StockAdjustmentId
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.StockAdjustments.Where(p => p.Status == Status.active|| p.Status== Status.inactive).Select(b => new SelectFormat
                {
                    text = b.VoucherNo, //each json object will have 
                    id = b.StockAdjustmentId
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        //Function to update ItemTransaction in Create Mode
        public ActionResult ItemTransactionInCreateMode(long MC, StockAdjustmentViewModel stkadj, string UserId, DateTime CurrentDate)
        {
            decimal Quantity = 0, ConvrtdQty = 0;
           
            Quantity = stkadj.ItemQuantity;

            ConvrtdQty = (from b in db.Items
                          where(b.ItemID == stkadj.ItemID)
                          select(stkadj.ItemUnitID == b.ItemUnitID) ?
                                (b.SubUnitId != null) ? (Quantity* b.ConFactor) : Quantity
                                : Quantity).FirstOrDefault();

            if (stkadj.AdjustmentType == AdjustmentType.Less)
                Quantity = -ConvrtdQty;
            else
                Quantity = ConvrtdQty;
               
            //*******Material Center
            ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == stkadj.ItemID && a.McId == MC)).FirstOrDefault();

            if (McItemObj == null)
                //Add Quantity to ItemTransaction Table
                com.AddItemTransaction(stkadj.ItemID, MC, Quantity, UserId, CurrentDate);
            else
                com.UpdateItemTransaction(stkadj.ItemID, MC, Quantity, UserId, CurrentDate);

            return Json("Item Transaction Created Successfully!");
        }

        //Function to update ItemTransaction in Edit Mode
        public ActionResult ItemTransactionInEditMode(StockAdjustment stad, long TransactionId, long MC, StockAdjustmentViewModel stkadj, string UserId, DateTime CurrentDate)
        {
            decimal Quantity = 0, PrevQty = 0, ConvrtdQty = 0;

            //(===>Delete and Insert)
            //***********Updating the previous quantity(===>Delete previous)
            var PrevList = (from a in db.StockAdjustments
                            join b in db.Items on a.ItemID equals b.ItemID
                            where (a.StockAdjustmentId == TransactionId)
                            select new
                            {
                                Quantity = ((a.ItemUnitID == b.ItemUnitID) ?
                                           (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                           : a.ItemQuantity)
                            }).FirstOrDefault();

            if (stad.AdjustmentType == AdjustmentType.Add)
                PrevQty = -PrevList.Quantity;
            else
                PrevQty = PrevList.Quantity;

            com.UpdateItemTransaction(stad.ItemID, stad.MaterialCenter, PrevQty, UserId, CurrentDate);

            //***********Updating the current quantity(===>Insert new)
            Quantity = stkadj.ItemQuantity;

            ConvrtdQty = (from b in db.Items
                          where (b.ItemID == stkadj.ItemID)
                          select (stkadj.ItemUnitID == b.ItemUnitID) ?
                                 (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                 : Quantity).FirstOrDefault();

            if (stkadj.AdjustmentType == AdjustmentType.Less)
                Quantity = -ConvrtdQty;
            else
                Quantity = ConvrtdQty;

            //*******Material Center
            ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == stkadj.ItemID && a.McId == MC)).FirstOrDefault();

            if (McItemObj == null)
                //Add Quantity to ItemTransaction Table
                com.AddItemTransaction(stkadj.ItemID, MC, Quantity, UserId, CurrentDate);
            else
                com.UpdateItemTransaction(stkadj.ItemID, MC, Quantity, UserId, CurrentDate);

            return Json("Item Transaction Updated Successfully!");
        }

        //Function to update ItemTransaction in Delete Mode
        public ActionResult ItemTransactionInDeleteMode(StockAdjustment stk, long TransactionId, string UserId)
        {
            decimal Quantity = 0;           
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            //*******Material Center
            ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == stk.ItemID && a.McId == stk.MaterialCenter)).FirstOrDefault();

            if (McItemObj != null)
            {  
                //***********Updating the previous quantity(===>Delete previous)
                var CurrObj = (from a in db.StockAdjustments
                                join b in db.Items on a.ItemID equals b.ItemID
                                where (a.StockAdjustmentId == TransactionId)
                                select new
                                {
                                    Quantity = ((a.ItemUnitID == b.ItemUnitID) ?
                                               (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                               : a.ItemQuantity)
                                }).FirstOrDefault();

                if (stk.AdjustmentType == AdjustmentType.Less)
                    Quantity = CurrObj.Quantity;
                else
                    Quantity = -CurrObj.Quantity;

                com.UpdateItemTransaction(stk.ItemID, stk.MaterialCenter, Quantity, UserId, CurrentDate);
            }

            return Json("Item Transaction Updated Successfully!");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Function To Get EntryNo
        private long GetAdjustmentNo()
        {
            Int64 AdjustmentNo = 0, LastNo = 0;

            LastNo = db.AssetAdjustments.Select(p => p.AdjustmentNo).DefaultIfEmpty().Max();

            if (LastNo == 0)
                AdjustmentNo = 1;
            else
                AdjustmentNo = LastNo + 1;

            return AdjustmentNo;
        }

        public ActionResult IndexAsset()
        {
            var nameAsset = (from a in db.AssetAdjustments
                            join b in db.AssetTransferDetails on a.AssetId equals b.AssetitementryId
                            select (new
                            {
                                ID = a.AssetId,
                                Name = b.AssetName
                            })).ToList();
            ViewBag.Name = QkSelect.List(nameAsset, "ID", "Name");

            //For Dropdown Assets 
            ViewBag.Assets = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = true, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            ViewBag.AdjustmentType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Less", Value="0"},
                new SelectListItem() {Text = "Add", Value="1"},
            }, "Value", "Text");

            return View();
        }


        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Stock Adjustment")]
        public JsonResult GetAssetAdjustment(long? Asset, decimal? Quantity, string AdjType)
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
            AdjustmentType At = new AdjustmentType();
            if (AdjType != "")
            {
                At = (AdjType == "0") ? AdjustmentType.Less : AdjustmentType.Add;
            };

            var v = (from a in db.AssetAdjustments
                     join b in db.AssetTransferDetails on a.AssetId equals b.AssetitementryId into itm
                     from b in itm.DefaultIfEmpty()
                     join c in db.ItemUnits on a.AssetUnitId equals c.ItemUnitID into unit
                     from c in unit.DefaultIfEmpty()
                     where   (Asset     == null || Asset    == 0    || a.AssetId        == Asset)   &&
                             (AdjType   == null || AdjType  == ""   || a.AdjustmentType == At)      &&
                             (Quantity  == null || a.AssetQuantity == Quantity)
                     select new
                     {
                         AdjustmentId   = a.AssetAdjustmentId,
                         Asset          = b.AssetName,
                         Quantity       = a.AssetQuantity,
                         Unit           = c.ItemUnitName,
                         AdjustmentDate = a.AdjustmentDate,
                         AdjustmentType = a.AdjustmentType,
                         a.PurchaseRate,
                         a.Reason,
                         a.CreatedDate
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => /*p.id.ToString().ToLower().Contains(search.ToLower()) ||*/
                                     p.Asset.ToString().ToLower().Contains(search.ToLower()));
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

        [HttpGet]
        public ActionResult GetAssetAdjById(int AdjId)
        {
            var stk = (from a in db.AssetAdjustments
                       join b in db.AssetTransferDetails on a.AssetId equals b.AssetitementryId
                       join c in db.ItemUnits on b.UnitId equals c.ItemUnitID into primary
                       from c in primary.DefaultIfEmpty()
                       where a.AssetAdjustmentId == AdjId
                       select new
                       {
                           a.AssetId,
                           a.AssetUnitId,
                           priunitId    =   b.UnitId,
                           PriUnit      =   c.ItemUnitName,
                           AssetName    =   b.AssetName,
                           Unit         =   a.AssetUnitId,
                           Type         =   a.AdjustmentType,
                           Quantity     =   a.AssetQuantity,
                           Price        =   a.PurchaseRate                         
                       }).FirstOrDefault();
            return Json(stk);

        }
    }
}
