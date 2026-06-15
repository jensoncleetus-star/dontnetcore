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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    public class HireStockReportController : BaseController
    {
        ApplicationDbContext db;
        public HireStockReportController()
        {
            db = new ApplicationDbContext();
        }
        #region till date

        // GET: StockReport
        [QkAuthorize(Roles = "Dev,Till Date HireStock")]
        public ActionResult Index()
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);



            return View();
        }
        #endregion

        #region as on date
        // GET: StockReport on date
        [QkAuthorize(Roles = "Dev,HireStock As On Date")]
        public ActionResult OnDate()
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        #endregion

        #region itemwise
        [QkAuthorize(Roles = "Dev,HireStock Item Wise")]
        public ActionResult ItemWise()
        {
            ViewBag.Item = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,HireStock Item Wise")]
        public ActionResult ItemWise(long? ddlItem, long? ddlMC)
        {
            return RedirectToAction("ViewItemWise", new { itemid = ddlItem, ddmc = ddlMC });
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,HireStock Item Wise")]
        public ActionResult ViewItemWise(long? itemid, long? ddmc)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            if (itemid != 0)
            {
                ViewBag.ItemName = (from a in db.Items
                                    where a.ItemID == itemid
                                    select new
                                    {
                                        ItemName = a.ItemName
                                    }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.ItemName = "All";
            }

            if (ddmc != null && ddmc != 0)
            {
                string mcn = db.MCs.Where(z => z.MCId == ddmc).Select(z => z.MCName).FirstOrDefault();
                ViewBag.MCSName = "Material Center : " + mcn;
            }
            else
            {
                ViewBag.MCSName = "";
            }


            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;
            return View();
        }

        #endregion

        #region category wise
        // GET: StockReport Category Wise
        [QkAuthorize(Roles = "Dev,HireStock Category Wise")]
        public ActionResult CategoryWise()
        {
            ViewBag.Category = QkSelect.List(
                new List<SelectListItem>
                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                }, "Value", "Text", 1);
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,HireStock Category Wise")]
        public ActionResult CategoryWise(long? ddlCategory, long? ddlMC, bool stockable = false)
        {
            return RedirectToAction("ViewCategoryWise", new { categoryid = ddlCategory, stockable = stockable, ddmc = ddlMC });
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,HireStock Category Wise")]
        public ActionResult ViewCategoryWise(long? categoryid, bool stockable, long? ddmc)
        {
            if (categoryid != 0)
            {
                ViewBag.CatName = (from a in db.ItemCategorys
                                   where a.ItemCategoryID == categoryid
                                   select new
                                   {
                                       Name = a.ItemCategoryName
                                   }).FirstOrDefault().Name;
            }
            else
            {
                ViewBag.CatName = "All";
            }

            if (ddmc != null && ddmc != 0)
            {
                string mcn = db.MCs.Where(z => z.MCId == ddmc).Select(z => z.MCName).FirstOrDefault();
                ViewBag.MCSName = "Material Center : " + mcn;
            }
            else
            {
                ViewBag.MCSName = "";
            }
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;
            return View();
        }

        #endregion

        #region brand wise

        // GET: StockReport Brand Wise
        [QkAuthorize(Roles = "Dev,HireStock Brand Wise")]
        public ActionResult BrandWise()
        {
            ViewBag.Brand = QkSelect.List(
            new List<SelectListItem>
            {
                new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,HireStock Brand Wise")]
        public ActionResult BrandWise(long? ddlBrand, long? ddlMC, bool stockable = false)
        {
            return RedirectToAction("ViewBrandWise", new { brandid = ddlBrand, ddmc = ddlMC, stockable = stockable });
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,HireStock Brand Wise")]
        public ActionResult ViewBrandWise(long? brandid, bool stockable, long? ddmc)
        {
            if (brandid != 0)
            {
                ViewBag.BrName = (from a in db.ItemBrands
                                  where a.ItemBrandID == brandid
                                  select new
                                  {
                                      Name = a.ItemBrandName
                                  }).FirstOrDefault().Name;
            }
            else
            {
                ViewBag.BrName = "All";
            }
            if (ddmc != null && ddmc != 0)
            {
                string mcn = db.MCs.Where(z => z.MCId == ddmc).Select(z => z.MCName).FirstOrDefault();
                ViewBag.MCSName = "Material Center : " + mcn;
            }
            else
            {
                ViewBag.MCSName = "";
            }
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;
            companySet();
            return View();
        }


        #endregion


        #region moment 
        // GET: StockReport Moment
        [QkAuthorize(Roles = "Dev,Hire Stock Moment")]
        public ActionResult Moment()
        {

            ViewBag.Category = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                }, "Value", "Text", 0);
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            ViewBag.JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Hire Stock Moment")]
        public ActionResult Moment(long? ddlItem, long? ddlMC)
        {
            return RedirectToAction("ViewMoment", new { iditem = ddlItem, ddmc = ddlMC });
        }
        [QkAuthorize(Roles = "Dev,Hire Stock Moment")]
        public ActionResult GetMoment(long? iditem, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            var left = (from a in db.PEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                        where a.Item == iditem
                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                        select new
                        {
                            Id = a.PEItemsId,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                            b.MinStock,
                            b.KeepStock,
                            Date = (DateTime?)c.PEDate,
                            Type = "Purchase",
                            StockInP = (decimal?)db.PEItemss.Where(x => x.Item == b.ItemID && x.PurchaseEntry == a.PurchaseEntry && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum(),
                            StockInS = (decimal?)db.PEItemss.Where(x => x.Item == b.ItemID && x.PurchaseEntry == a.PurchaseEntry && a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId).Select(a => a.ItemQuantity).Sum(),
                            StockOutP = (decimal?)null,
                            StockOutS = (decimal?)null,
                            invoiceno = c.BillNo,
                            entry = (DateTime?)c.PECreatedDate
                        });
            var left1 = (from a in db.PRItemss
                         join b in db.Items on a.Item equals b.ItemID
                         join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                         where a.Item == iditem
                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                         select new
                         {
                             Id = a.PRItemsId,
                             ItemCode = b.ItemCode,
                             ItemName = b.ItemName,
                             ItemWithCode = b.ItemName,
                             b.ItemUnitID,
                             b.SubUnitId,
                             ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                             b.ItemID,
                             OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                             b.MinStock,
                             b.KeepStock,
                             Date = (DateTime?)c.PRDate,
                             Type = "Purchase Return",
                             StockInP = (decimal?)db.PRItemss.Where(x => x.Item == b.ItemID && x.PurchaseReturnId == a.PurchaseReturnId && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum(),
                             StockInS = (decimal?)db.PRItemss.Where(x => x.Item == b.ItemID && x.PurchaseReturnId == a.PurchaseReturnId && a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId).Select(a => a.ItemQuantity).Sum(),
                             StockOutP = (decimal?)null,
                             StockOutS = (decimal?)null,
                             invoiceno = c.BillNo,
                             entry = (DateTime?)c.PRCreatedDate
                         });

            var right = (from a in db.SEItemss
                         join b in db.Items on a.Item equals b.ItemID
                         join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                         where a.Item == iditem
                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                         && c.SaleType != SaleType.Hire
                         select new
                         {
                             Id = a.SEItemsId,
                             ItemCode = b.ItemCode,
                             ItemName = b.ItemName,
                             ItemWithCode = b.ItemName,
                             b.ItemUnitID,
                             b.SubUnitId,
                             ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                             b.ItemID,
                             OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                             b.MinStock,
                             b.KeepStock,
                             Date = (DateTime?)c.SEDate,
                             Type = "Sale",
                             StockInP = (decimal?)null,
                             StockInS = (decimal?)null,
                             StockOutP = (decimal?)db.SEItemss.Where(x => x.Item == b.ItemID && x.SalesEntry == a.SalesEntry && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum(),
                             StockOutS = (decimal?)db.SEItemss.Where(x => x.Item == b.ItemID && x.SalesEntry == a.SalesEntry && a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId).Select(a => a.ItemQuantity).Sum(),
                             invoiceno = c.BillNo,
                             entry = (DateTime?)c.SECreatedDate
                         });
            var right1 = (from a in db.SRItemss
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                          where a.Item == iditem
                          && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                          select new
                          {

                              Id = a.SRItemsId,
                              ItemCode = b.ItemCode,
                              ItemName = b.ItemName,
                              ItemWithCode = b.ItemName,
                              b.ItemUnitID,
                              b.SubUnitId,
                              ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                              b.ItemID,
                              OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                              b.MinStock,
                              b.KeepStock,
                              Date = (DateTime?)c.SRDate,
                              Type = "Sale",
                              StockInP = (decimal?)null,
                              StockInS = (decimal?)null,
                              StockOutP = (decimal?)db.SRItemss.Where(x => x.Item == b.ItemID && x.SalesReturnId == a.SalesReturnId && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum(),
                              StockOutS = (decimal?)db.SRItemss.Where(x => x.Item == b.ItemID && x.SalesReturnId == a.SalesReturnId && a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId).Select(a => a.ItemQuantity).Sum(),
                              invoiceno = c.BillNo,
                              entry = (DateTime?)c.SRCreatedDate
                          });
            var opening = (from a in db.Items
                           where a.ItemID == iditem && a.OpeningStock > 0
                           select new
                           {

                               Id = a.ItemID,
                               ItemCode = a.ItemCode,
                               ItemName = a.ItemName,
                               ItemWithCode = a.ItemName,
                               a.ItemUnitID,
                               a.SubUnitId,
                               ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                               a.ItemID,
                               OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && a.OpeningStock != null) ? a.OpeningStock : 0,
                               a.MinStock,
                               a.KeepStock,
                               Date = (DateTime?)null,
                               Type = "Opening Stock",
                               StockInP = ((ddmc == 0 || ddmc == 1 || ddmc == null) && a.OpeningStock != null) ? a.OpeningStock : 0,
                               StockInS = (decimal?)null,
                               StockOutP = (decimal?)null,
                               StockOutS = (decimal?)null,
                               invoiceno = "",
                               entry = (DateTime?)null
                           });
            var stockadj = (from a in db.StockAdjustments
                            join b in db.Items on a.ItemID equals b.ItemID
                            where a.ItemID == iditem
                            && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                            select new
                            {

                                Id = a.StockAdjustmentId,
                                ItemCode = b.ItemCode,
                                ItemName = b.ItemName,
                                ItemWithCode = b.ItemName,
                                a.ItemUnitID,
                                b.SubUnitId,
                                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                a.ItemID,
                                OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                                b.MinStock,
                                b.KeepStock,
                                Date = (DateTime?)a.AdjDate,
                                Type = "Stock Adjustment",
                                StockInP = (decimal?)((a.AdjustmentType == AdjustmentType.Add && a.ItemUnitID == b.ItemUnitID) ? a.ItemQuantity : 0),
                                StockInS = (decimal?)((a.AdjustmentType == AdjustmentType.Add && a.ItemUnitID == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                                StockOutP = (decimal?)((a.AdjustmentType == AdjustmentType.Less && a.ItemUnitID == b.ItemUnitID) ? a.ItemQuantity : 0),
                                StockOutS = (decimal?)((a.AdjustmentType == AdjustmentType.Less && a.ItemUnitID == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                                invoiceno = "",
                                entry = (DateTime?)a.CreatedDate
                            });

            var ProdItem = (from a in db.GeneratedItem
                            join b in db.Items on a.Item equals b.ItemID
                            join c in db.Productions on a.Production equals c.ProductionId
                            where a.Item == iditem
                            && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                            select new
                            {
                                Id = a.GeneratedID,
                                ItemCode = b.ItemCode,
                                ItemName = b.ItemName,
                                ItemWithCode = b.ItemName,
                                ItemUnitID = a.Unit,
                                b.SubUnitId,
                                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                ItemID = a.Item,
                                OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                                b.MinStock,
                                b.KeepStock,
                                Date = (DateTime?)c.PEDate,
                                Type = "Item Produced",
                                StockInP = (decimal?)((a.Unit == b.ItemUnitID) ? a.Qty : 0),
                                StockInS = (decimal?)((a.Unit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.Qty : 0),
                                StockOutP = (decimal?)null,
                                StockOutS = (decimal?)null,
                                invoiceno = c.VoucherNo,
                                entry = (DateTime?)c.CreatedDate
                            });
            var ProdCItem = (from a in db.ProItems
                             join b in db.Items on a.ItemId equals b.ItemID
                             join c in db.Productions on a.Production equals c.ProductionId
                             where a.ItemId == iditem
                             && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                             select new
                             {

                                 Id = a.ProItemId,
                                 ItemCode = b.ItemCode,
                                 ItemName = b.ItemName,
                                 ItemWithCode = b.ItemName,
                                 ItemUnitID = a.Unit,
                                 b.SubUnitId,
                                 ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                 ItemID = a.ItemId,
                                 OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                                 b.MinStock,
                                 b.KeepStock,
                                 Date = (DateTime?)c.PEDate,
                                 Type = "Item Consumed",
                                 StockInP = (decimal?)null,
                                 StockInS = (decimal?)null,
                                 StockOutP = (decimal?)((a.Unit == b.ItemUnitID) ? a.Quantity : 0),
                                 StockOutS = (decimal?)((a.Unit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.Quantity : 0),
                                 invoiceno = c.VoucherNo,
                                 entry = (DateTime?)c.CreatedDate
                             });

            var UnaItem = (from a in db.ConsumedItem
                           join b in db.Items on a.Item equals b.ItemID
                           join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                           where a.Item == iditem
                           && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                           select new
                           {

                               Id = a.ConsumedID,
                               ItemCode = b.ItemCode,
                               ItemName = b.ItemName,
                               ItemWithCode = b.ItemName,
                               ItemUnitID = a.Unit,
                               b.SubUnitId,
                               ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                               ItemID = a.Item,
                               OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                               b.MinStock,
                               b.KeepStock,
                               Date = (DateTime?)c.PEDate,
                               Type = "Item Unassembled",
                               StockInP = (decimal?)((a.Unit == b.ItemUnitID) ? a.Qty : 0),
                               StockInS = (decimal?)((a.Unit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.Qty : 0),
                               StockOutP = (decimal?)null,
                               StockOutS = (decimal?)null,
                               invoiceno = c.VoucherNo,
                               entry = (DateTime?)c.CreatedDate
                           });
            var UnaCItem = (from a in db.UnassembleItems
                            join b in db.Items on a.ItemId equals b.ItemID
                            join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                            where a.ItemId == iditem
                            && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                            select new
                            {

                                Id = a.UnItemId,
                                ItemCode = b.ItemCode,
                                ItemName = b.ItemName,
                                ItemWithCode = b.ItemName,
                                ItemUnitID = a.Unit,
                                b.SubUnitId,
                                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                ItemID = a.ItemId,
                                OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                                b.MinStock,
                                b.KeepStock,
                                Date = (DateTime?)c.PEDate,
                                Type = "Item Consumed",
                                StockInP = (decimal?)null,
                                StockInS = (decimal?)null,
                                StockOutP = (decimal?)((a.Unit == b.ItemUnitID) ? a.Quantity : 0),
                                StockOutS = (decimal?)((a.Unit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.Quantity : 0),
                                invoiceno = c.VoucherNo,
                                entry = (DateTime?)c.CreatedDate
                            });

            var TranFrom = (from a in db.StockTransferItems
                            join b in db.Items on a.Item equals b.ItemID
                            join c in db.StockTransfers on a.StockTransferId equals c.Id
                            where a.Item == iditem
                            && (ddmc == c.MCFrom)
                            select new
                            {

                                Id = a.StockTransferId,
                                ItemCode = b.ItemCode,
                                ItemName = b.ItemName,
                                ItemWithCode = b.ItemName,
                                ItemUnitID = a.Unit,
                                b.SubUnitId,
                                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                ItemID = a.Item,
                                OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                                b.MinStock,
                                b.KeepStock,
                                Date = (DateTime?)c.Date,
                                Type = "Item Transfered",
                                StockInP = (decimal?)null,
                                StockInS = (decimal?)null,
                                StockOutP = (decimal?)((a.Unit == b.ItemUnitID) ? a.Quantity : 0),
                                StockOutS = (decimal?)((a.Unit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.Quantity : 0),
                                invoiceno = c.Voucher,
                                entry = (DateTime?)c.CreatedDate
                            });

            var TranTo = (from a in db.StockTransferItems
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.StockTransfers on a.StockTransferId equals c.Id
                          where a.Item == iditem
                          && (ddmc == c.MCTo)
                          select new
                          {

                              Id = a.StockTransferId,
                              ItemCode = b.ItemCode,
                              ItemName = b.ItemName,
                              ItemWithCode = b.ItemName,
                              b.ItemUnitID,
                              b.SubUnitId,
                              ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                              b.ItemID,
                              OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                              b.MinStock,
                              b.KeepStock,
                              Date = (DateTime?)c.Date,
                              Type = "Item Received",
                              StockInP = (decimal?)((a.Unit == b.ItemUnitID) ? a.Quantity : 0),
                              StockInS = (decimal?)((a.Unit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.Quantity : 0),
                              StockOutP = (decimal?)null,
                              StockOutS = (decimal?)null,
                              invoiceno = c.Voucher,
                              entry = (DateTime?)c.CreatedDate
                          });
            var hire = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                        let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                        where a.Item == iditem
                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                        && c.SaleType == SaleType.Hire && chkextend == null
                        select new
                        {

                            Id = a.SEItemsId,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,//b.OpeningStock != null ? b.OpeningStock : 0,
                            b.MinStock,
                            b.KeepStock,
                            Date = (DateTime?)c.SEDate,
                            Type = "Hire Delivery",
                            StockInP = (decimal?)null,
                            StockInS = (decimal?)null,
                            StockOutP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                            StockOutS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                            invoiceno = c.BillNo,
                            entry = (DateTime?)c.SECreatedDate
                        });



            var returnnote = (from a in db.HrItems
                              join b in db.Items on a.Item equals b.ItemID
                              join c in db.HireReturns on a.Hr equals c.HireReturnId
                              where a.Item == iditem && c.RtType == "Return"
                             && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                              select new
                              {
                                  Id = a.HrItemId,
                                  ItemCode = b.ItemCode,
                                  ItemName = b.ItemName,
                                  ItemWithCode = b.ItemName,
                                  b.ItemUnitID,
                                  b.SubUnitId,
                                  ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                  b.ItemID,
                                  OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,
                                  b.MinStock,
                                  b.KeepStock,
                                  Date = (DateTime?)c.Date,
                                  Type = "Hire Return",
                                  StockInP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                                  StockInS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                                  StockOutP = (decimal?)null,
                                  StockOutS = (decimal?)null,
                                  invoiceno = c.BillNo,
                                  entry = (DateTime?)c.CreatedDate
                              });

            var hiremiss = (from a in db.HrItems
                            join b in db.Items on a.Item equals b.ItemID
                            join c in db.HireReturns on a.Hr equals c.HireReturnId
                            where a.Item == iditem && c.RtType == "Missing"
                           && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                            select new
                            {

                                Id = a.HrItemId,
                                ItemCode = b.ItemCode,
                                ItemName = b.ItemName,
                                ItemWithCode = b.ItemName,
                                b.ItemUnitID,
                                b.SubUnitId,
                                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                b.ItemID,
                                OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,
                                b.MinStock,
                                b.KeepStock,
                                Date = (DateTime?)c.Date,
                                Type = "Hire Missing",
                                StockInP = (decimal?)null,
                                StockInS = (decimal?)null,
                                StockOutP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                                StockOutS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                                invoiceno = c.BillNo,
                                entry = (DateTime?)c.CreatedDate
                            });
            var full = left.Union(right);
            var full1 = left1.Union(right1);

            full = full.Union(full1);
            full = full.Union(opening);
            full = full.Union(stockadj);
            full = full.Union(ProdItem);
            full = full.Union(ProdCItem);
            full = full.Union(UnaItem);
            full = full.Union(UnaCItem);
            full = full.Union(TranFrom);
            full = full.Union(TranTo);
            full = full.Union(hire);
            full = full.Union(returnnote);
            full = full.Union(hiremiss);
            full = full.AsQueryable().OrderBy("Date asc, entry asc");

            var v =
                   (from o in full
                    join c in db.ItemUnits on o.ItemUnitID equals c.ItemUnitID into primary
                    from c in primary.DefaultIfEmpty()
                    join d in db.ItemUnits on o.SubUnitId equals d.ItemUnitID into second
                    from d in second.DefaultIfEmpty()
                    select new
                    {
                        o.ItemCode,
                        o.ItemName,
                        o.ItemWithCode,
                        o.ItemUnitID,
                        o.SubUnitId,
                        o.ConFactor,
                        o.ItemID,
                        OpeningStock = o.OpeningStock,
                        o.MinStock,
                        o.KeepStock,
                        o.Date,
                        o.Type,
                        o.StockInP,
                        o.StockInS,
                        o.StockOutP,
                        o.StockOutS,
                        o.invoiceno,
                        o.entry,
                        StockIn = (decimal?)((o.StockInP ?? 0) + ((o.StockInS ?? 0) / o.ConFactor)) ?? 0,
                        StockOut = (decimal?)((o.StockOutP ?? 0) + ((o.StockOutS ?? 0) / o.ConFactor)) ?? 0,
                        PriUnit = c.ItemUnitName ?? "",
                        SubUnit = d.ItemUnitName ?? "",
                    }).OrderBy(a => a.Date);

            recordsTotal = v.Count();
            var data = v.ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Hire Stock Moment")]
        public ActionResult ViewMoment(long? iditem, long? ddmc)
        {
            if (iditem != 0)
            {
                ViewBag.ItemName = (from a in db.Items
                                    where a.ItemID == iditem
                                    select new
                                    {
                                        Name = a.ItemName
                                    }).FirstOrDefault().Name;
            }
            else
            {
                ViewBag.ItemName = "All";
            }

            if (ddmc != null && ddmc != 0)
            {
                string mcn = db.MCs.Where(z => z.MCId == ddmc).Select(z => z.MCName).FirstOrDefault();
                ViewBag.MCSName = "Material Center : " + mcn;
            }
            else
            {
                ViewBag.MCSName = "";
            }
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            ViewBag.JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;

            return View();
        }

        #endregion

        #region stock details

        public ActionResult Details(long iditem, long? ddmc)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;
            ViewBag.Mc = db.MCs.Where(b => b.MCId == ddmc).Select(a => a.MCName).FirstOrDefault();
            ViewBag.Item = db.Items.Where(b => b.ItemID == iditem).Select(a => a.ItemName).FirstOrDefault();
            return View();
        }
        [HttpPost]
        public ActionResult GetDetails(long iditem, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var data = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.PEItemsId).Max() }
                        equals new { f1 = f.Item, f2 = f.PEItemsId } into pur
                        from f in pur.DefaultIfEmpty()
                        where b.ItemID == iditem
                        select new
                        {
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            b.PartNumber,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,

                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            b.KeepStock,
                            PIUnitName = c.ItemUnitName,
                            SIUnitName = d.ItemUnitName,

                            cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                            costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                        }).Distinct().ToList();

            var v =
                data.Select(b =>
                {

                 var PriPurchase = (from a in db.PEItemss
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var SubPurchase = (from a in db.PEItemss
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var PriSale = (from a in db.SEItemss
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where (a.Item == b.ItemID && (a.ItemUnit == b.ItemUnitID || a.ItemUnit == null))
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && c.SaleType != SaleType.Hire
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);

                 var SubSale = (from a in db.SEItemss
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where
                                (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && c.SaleType != SaleType.Hire
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);

                 var PriPReturn = (from a in db.PRItemss
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubPReturn = (from a in db.PRItemss
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriSReturn = (from a in db.SRItemss
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubSReturn = (from a in db.SRItemss
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriAddAdj = (from a in db.StockAdjustments
                                  where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var SubAddAdj = (from a in db.StockAdjustments
                                  where (a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add && b.PIUnitName != b.SIUnitName)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var PriLessAdj = (from a in db.StockAdjustments
                                   where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var subLessAdj = (from a in db.StockAdjustments
                                   where (a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less && b.PIUnitName != b.SIUnitName)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriProdItem = (from a in db.GeneratedItem
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);

                 var SubProdItem = (from a in db.GeneratedItem
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);

                 var PriProdCItem = (from a in db.ProItems
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);

                 var SubProdCItem = (from a in db.ProItems
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);

                 var PriUnItem = (from a in db.ConsumedItem
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);


                 var SubUnItem = (from a in db.ConsumedItem
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var PriUnCItem = (from a in db.UnassembleItems
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(x => x.Quantity);

                 var SubUnCItem = (from a in db.UnassembleItems
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(x => x.Quantity);


                 var PriStTrFrom = (from a in db.StockTransferItems
                                    join c in db.StockTransfers on a.StockTransferId equals c.Id
                                    where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                    && (ddmc == c.MCFrom)
                                    select new
                                    {
                                        a.Quantity
                                    }).ToList().Sum(m => m.Quantity);

                 var PriStTrTo = (from a in db.StockTransferItems
                                  join c in db.StockTransfers on a.StockTransferId equals c.Id
                                  where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                  && (ddmc == c.MCTo)
                                  select new
                                  {
                                      a.Quantity
                                  }).ToList().Sum(m => m.Quantity);
                 var SubStTrFrom = (from a in db.StockTransferItems
                                    join c in db.StockTransfers on a.StockTransferId equals c.Id
                                    where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && (ddmc == c.MCFrom)
                                    select new
                                    {
                                        a.Quantity
                                    }).ToList().Sum(m => m.Quantity);

                 var SubStTrTo = (from a in db.StockTransferItems
                                  join c in db.StockTransfers on a.StockTransferId equals c.Id
                                  where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                  && (ddmc == c.MCTo)
                                  select new
                                  {
                                      a.Quantity
                                  }).ToList().Sum(m => m.Quantity);


                 var PriHDNote = (from a in db.SEItemss
                                  join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                  let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                  where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  && (c.SaleType == SaleType.Hire) && chkextend == null
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var SubHDNote = (from a in db.SEItemss
                                  join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                  let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                  where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  && (c.SaleType == SaleType.Hire) && chkextend == null
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var PriRetNote = (from a in db.HrItems
                                   join c in db.HireReturns on a.Hr equals c.HireReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubRetNote = (from a in db.HrItems
                                   join c in db.HireReturns on a.Hr equals c.HireReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriHireMiss = (from a in db.HrItems
                                    join c in db.HireReturns on a.Hr equals c.HireReturnId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Missing")
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var SubHireMiss = (from a in db.HrItems
                                    join c in db.HireReturns on a.Hr equals c.HireReturnId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Missing")
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 return new
                 {
                     b.ItemID,
                     b.ItemCode,
                     b.ItemName,
                     b.ItemWithCode,
                     b.ItemUnitID,
                     b.SubUnitId,
                     b.PartNumber,
                     PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                     SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                     b.categoryname,
                     OpeningStock = b.OpeningStock ?? 0,
                     MinStock = (b.MinStock != null) ? b.MinStock : 0,
                     b.ConFactor,
                     stockable = b.KeepStock,

                     PriStTrFrom,
                     PriStTrTo,
                     SubStTrFrom,
                     SubStTrTo,

                     PriPurchase = (PriPurchase + (int)(SubPurchase / b.ConFactor)),
                     SubPurchase = (SubPurchase % b.ConFactor),

                     PriSale = (PriSale + (int)(SubSale / b.ConFactor)),
                     SubSale = (SubSale % b.ConFactor),

                     PriPReturn = (PriPReturn + (int)(SubPReturn / b.ConFactor)),
                     SubPReturn = (SubPReturn % b.ConFactor),

                     PriSReturn = (PriSReturn + (int)(SubSReturn / b.ConFactor)),
                     SubSReturn = (SubSReturn % b.ConFactor),

                     PriAddAdj = PriAddAdj,
                     SubAddAdj = SubAddAdj,
                     PriLessAdj = PriLessAdj,
                     subLessAdj = subLessAdj,

                     PriProdItem = PriProdItem,
                     SubProdItem = SubProdItem,

                     PriProdCItem = PriProdCItem,
                     SubProdCItem = SubProdCItem,

                     PriUnItem = PriUnItem,
                     SubUnItem = SubUnItem,

                     PriUnCItem = PriUnCItem,
                     SubUnCItem = SubUnCItem,

                     PriHDNote,
                     SubHDNote,

                     PriRetNote,
                     SubRetNote,

                     PriHireMiss,
                     SubHireMiss,

                     pritotal = ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom)),
                     subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)),

                     total = (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)), // - (StTrFrom - StTrTo)

                     stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss)), // + StTrTo
                     stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)), // + StTrFrom 
                     cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                 };
                }).ToList();
            var Purch = (from a in v
                         select new
                         {
                             id = 1,
                             a.ItemID,
                             a.PriUnit,
                             PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                             a.SubUnit,
                             name = a.ItemName,
                             type = "Purchase",
                             Stockin = (a.PriPurchase * a.ConFactor) + a.SubPurchase,
                             StockOut = (decimal)0,
                         });
            var PurchRet = (from a in v
                            select new
                            {
                                id = 2,
                                a.ItemID,
                                a.PriUnit,
                                PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                a.SubUnit,
                                name = a.ItemName,
                                type = "Purchase Return",
                                Stockin = (decimal)0,
                                StockOut = (a.PriPReturn * a.ConFactor) + a.SubPReturn,
                            });
            var Sales = (from a in v
                         select new
                         {
                             id = 3,
                             a.ItemID,
                             a.PriUnit,
                             PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                             a.SubUnit,
                             name = a.ItemName,
                             type = "Sales",
                             Stockin = (decimal)0,
                             StockOut = (a.PriSale * a.ConFactor) + a.SubSale
                         });
            var SalesRet = (from a in v
                            select new
                            {
                                id = 4,
                                a.ItemID,
                                a.PriUnit,
                                PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                a.SubUnit,
                                name = a.ItemName,
                                type = "Sales Return",
                                Stockin = (a.PriSReturn * a.ConFactor) + a.SubSReturn,
                                StockOut = (decimal)0,
                            });
            var AddAdj = (from a in v
                          select new
                          {
                              id = 5,
                              a.ItemID,
                              a.PriUnit,
                              PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                              a.SubUnit,
                              name = a.ItemName,
                              type = "Stock Adjustment - Added",
                              Stockin = (a.PriAddAdj * a.ConFactor) + a.SubAddAdj,
                              StockOut = (decimal)0,
                          });
            var LessAdj = (from a in v
                           select new
                           {
                               id = 6,
                               a.ItemID,
                               a.PriUnit,
                               PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                               a.SubUnit,
                               name = a.ItemName,
                               type = "Stock Adjustment - Lessed",
                               Stockin = (decimal)0,
                               StockOut = (a.PriLessAdj * a.ConFactor) + a.PriLessAdj,
                           });
            var Production = (from a in v
                              select new
                              {
                                  id = 7,
                                  a.ItemID,
                                  a.PriUnit,
                                  PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                  a.SubUnit,
                                  name = a.ItemName,
                                  type = "Item Produced",
                                  Stockin = (a.PriProdItem * a.ConFactor) + a.SubProdItem,
                                  StockOut = (decimal)0,
                              });
            var Consumed = (from a in v
                            select new
                            {
                                id = 8,
                                a.ItemID,
                                a.PriUnit,
                                PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                a.SubUnit,
                                name = a.ItemName,
                                type = "Item Consumed",
                                Stockin = (decimal)0,
                                StockOut = (a.PriProdCItem * a.ConFactor) + a.SubProdCItem
                            });
            var Unasseble = (from a in v
                             select new
                             {
                                 id = 9,
                                 a.ItemID,
                                 a.PriUnit,
                                 PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                 a.SubUnit,
                                 name = a.ItemName,
                                 type = "Item Unassembled",
                                 Stockin = (decimal)0,
                                 StockOut = (a.PriUnItem * a.ConFactor) + a.SubUnItem
                             });
            var Generated = (from a in v
                             select new
                             {
                                 id = 10,
                                 a.ItemID,
                                 a.PriUnit,
                                 PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                 a.SubUnit,
                                 name = a.ItemName,
                                 type = "Item Generated",
                                 Stockin = (a.PriUnCItem * a.ConFactor) + a.SubUnCItem,
                                 StockOut = (decimal)0
                             });

            var TranTo = (from a in v
                          select new
                          {
                              id = 10,
                              a.ItemID,
                              a.PriUnit,
                              PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                              a.SubUnit,
                              name = a.ItemName,
                              type = "Stock Transfered",
                              Stockin = ((a.PriStTrTo * a.ConFactor) + a.SubStTrTo),
                              StockOut = (decimal)0,
                          });

            var TranFrom = (from a in v
                            select new
                            {
                                id = 10,
                                a.ItemID,
                                a.PriUnit,
                                PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                a.SubUnit,
                                name = a.ItemName,
                                type = "Stock Received",
                                Stockin = (decimal)0,
                                StockOut = ((a.PriStTrFrom * a.ConFactor) + a.SubStTrFrom),

                            });

            var Hire = (from a in v
                        select new
                        {
                            id = 3,
                            a.ItemID,
                            a.PriUnit,
                            PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                            a.SubUnit,
                            name = a.ItemName,
                            type = "Hire Delivery",
                            Stockin = (decimal)0,
                            StockOut = (a.PriHDNote * a.ConFactor) + a.SubHDNote
                        });
            var HireRet = (from a in v
                           select new
                           {
                               id = 4,
                               a.ItemID,
                               a.PriUnit,
                               PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                               a.SubUnit,
                               name = a.ItemName,
                               type = "Hire Return",
                               Stockin = (a.PriRetNote * a.ConFactor) + a.SubRetNote,
                               StockOut = (decimal)0,
                           });

            var HireMiss = (from a in v
                            select new
                            {
                                id = 4,
                                a.ItemID,
                                a.PriUnit,
                                PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                a.SubUnit,
                                name = a.ItemName,
                                type = "Hire Miss",
                                Stockin = (a.PriHireMiss * a.ConFactor) + a.SubHireMiss,
                                StockOut = (decimal)0,
                            });

            var left = Purch.Union(PurchRet);
            var right = Sales.Union(SalesRet);
            var full = left.Union(right);
            full = full.Union(AddAdj);
            full = full.Union(LessAdj);
            full = full.Union(Production);
            full = full.Union(Consumed);
            full = full.Union(Unasseble);
            full = full.Union(Generated);
            full = full.Union(TranTo);
            full = full.Union(TranFrom);
            full = full.Union(Hire);
            full = full.Union(HireRet);
            full = full.Union(HireMiss);

            if (ddmc == null || ddmc == 0)
            {
                var opening = (from a in v
                               select new
                               {
                                   id = 0,
                                   a.ItemID,
                                   a.PriUnit,
                                   PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                   a.SubUnit,
                                   name = a.ItemName,
                                   type = "Opening Stock",
                                   Stockin = a.OpeningStock * a.ConFactor,
                                   StockOut = (decimal)0,
                               });
                full = full.Union(opening);
            }
            full = full.AsQueryable().OrderBy("id asc");
            //SORT
            recordsTotal = full.Count();
            var newdata = full.ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = newdata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion

        #region Crosshire

        // GET: StockReport
        [QkAuthorize(Roles = "Dev,CrossHire  Report")]
        public ActionResult Crosshire()
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.Category = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);
            ViewBag.Item = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            return View();
        }
        [QkAuthorize(Roles = "Dev,CrossHire  Report")]
        public ActionResult GetCrosshire(long? Category, long?Supplier, long? Item, string Todate, bool Stock = false)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? To = null;
            if (Todate != "")
            {
                To = DateTime.Parse(Todate, new CultureInfo("en-GB"));
            }

            var v = (from b in db.Items
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.PEItemsId).Max() }
                     equals new { f1 = f.Item, f2 = f.PEItemsId } into pur
                     from f in pur.DefaultIfEmpty()
                     where ((Item == 0)||(Item==b.ItemID)) && ((Category == 0) || (b.ItemCategoryID == Category))
                     orderby b.ItemID ascending
                     select new
                     {
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         ItemWithCode = b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         //OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,
                         OpeningStock = (b.OpeningStock != null) ? b.OpeningStock : 0,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         b.KeepStock,
                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                         pprice=b.PurchasePrice,
                         price = b.SellingPrice
                     }).Distinct();
            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            var mydata =
                (from b in data
                 let PriPurchaseCross = (from a in db.PEItemss
                                         join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                         let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                         where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                         && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                         && ((Supplier==0)||(c.Supplier==Supplier))
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).ToList().Sum(x => x.ItemQuantity)

                 let SubPurchaseCross = (from a in db.PEItemss
                                         join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                         let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                         where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit) 
                                         && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                          && ((Supplier == 0) || (c.Supplier == Supplier))
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).ToList().Sum(x => x.ItemQuantity)
                 let PriRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                         && ((Supplier == 0) || (c.Supplier == Supplier))
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity)

                 let SubRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
                                         && ((Supplier == 0) || (c.Supplier == Supplier))
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity)


                 select new
                 {
                     b.ItemID,
                     b.ItemCode,
                     b.ItemName,
                     b.ItemWithCode,
                     b.ItemUnitID,
                     b.SubUnitId,
                     PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                     SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                     b.categoryname,
                     OpeningStock = b.OpeningStock,
                     MinStock = (b.MinStock != null) ? b.MinStock : 0,
                     b.ConFactor,
                     stockable = b.KeepStock,

                     PriPurchase = (PriPurchaseCross + (int)(SubPurchaseCross / b.ConFactor)),
                     SubPurchase = (SubPurchaseCross % b.ConFactor),
                     
                     
                     stockIn = ((((PriPurchaseCross) * b.ConFactor)) + (SubPurchaseCross)), // + StTrTo
                     stockOut = ((((PriRetNoteCross) * b.ConFactor)) + (SubRetNoteCross)),
                     
                     balance=((((PriPurchaseCross) * b.ConFactor) - ((PriRetNoteCross) * b.ConFactor)) + (SubPurchaseCross - SubRetNoteCross)),

                     costrate= b.pprice,
                     costvalue = (((PriPurchaseCross * b.ConFactor) - ((PriRetNoteCross) * b.ConFactor)) + (SubPurchaseCross - SubRetNoteCross)) * b.pprice,

                     salerate= b.price,
                     salevalue = ((((PriPurchaseCross) * b.ConFactor) - ((PriRetNoteCross) * b.ConFactor)) + (SubPurchaseCross - SubRetNoteCross)) * b.price,

                     b.price,
                 }).OrderBy(a => a.ItemName).ToList();

            mydata= (from b in mydata
                     where ((Stock==false) || (b.balance !=0))
                     select new
                     {
                         b.ItemID,
                         b.ItemCode,
                         b.ItemName,
                         b.ItemWithCode,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.PriUnit,
                         b.SubUnit,
                         b.categoryname,
                         b.OpeningStock,
                         b.MinStock,
                         b.ConFactor,
                         b.stockable,

                         b.PriPurchase,
                         b.SubPurchase,


                         b.stockIn,
                         b.stockOut,

                         b.balance,

                         b.costrate,
                         b.costvalue,

                         b.salerate,
                         b.salevalue,

                         b.price,
                     }).OrderBy(a => a.ItemName).ToList();


            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? mydata.OrderBy(a => a.ItemCode).ToList() : mydata;
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion


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
