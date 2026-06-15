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
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class StockReportController : BaseController
    {
        ApplicationDbContext db;
        public StockReportController()
        {
            db = new ApplicationDbContext();
        }
        #region till date

        // GET: StockReport
        [QkAuthorize(Roles = "Dev,Till Date Stock")]
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
        [QkAuthorize(Roles = "Dev,Till Date Stock,Till Date HireStock")]
        public ActionResult GetStock(bool stockable, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            int recordsTotal = 0;
            var itemids = db.SEItemss.Select(o => o.Item).Distinct().ToList().ToArray();
            var v = (from b in db.Items
                     join bb in itemids on b.ItemID equals bb
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.PEItemsId).Max() }
                     equals new { f1 = f.Item, f2 = f.PEItemsId } into pur
                     from f in pur.DefaultIfEmpty()
                     let stv = (from az in db.StockTransferItems
                                join cz in db.StockTransfers on az.StockTransferId equals cz.Id
                                where (az.Item == b.ItemID) && cz.MCTo == ddmc
                                select new
                                {
                                    az.Price,
                                    az.Unit
                                }).FirstOrDefault()
                     where (stockable == true || b.KeepStock == true)
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
                         OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null && !MCList1.Any()) ? b.OpeningStock : 0,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         b.KeepStock,
                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                         stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                         stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                         price = b.SellingPrice
                     }).Distinct();
            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.ToList();//.Skip(skip).Take(pageSize).ToList();
            var mydata =
                data.Select(b =>
                {
                    var PriPurchase = (from a in db.PEItemss
                                       join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                       where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType != PurchaseHireType.CrossHire)
                                       select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubPurchase = (from a in db.PEItemss
                                       join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                       where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit) &&
                                       ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType != PurchaseHireType.CrossHire)
                                       select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriSale = (from a in db.SEItemss
                                   join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubSale = (from a in db.SEItemss
                                   join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriPReturn = (from a in db.PRItemss
                                      join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType != PurchaseHireType.CrossHire)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubPReturn = (from a in db.PRItemss
                                      join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType != PurchaseHireType.CrossHire)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriSReturn = (from a in db.SRItemss
                                      join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      && (c.SaleType != SaleType.Hire)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubSReturn = (from a in db.SRItemss
                                      join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      && (c.SaleType != SaleType.Hire)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriAddAdj = (from a in db.StockAdjustments
                                     where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                     select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubAddAdj = (from a in db.StockAdjustments
                                     where (a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add && b.PriUnit != b.SubUnit)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                     select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriLessAdj = (from a in db.StockAdjustments
                                      where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var subLessAdj = (from a in db.StockAdjustments
                                      where (a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less && b.PriUnit != b.SubUnit)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriProdItem = (from a in db.GeneratedItem
                                       join c in db.Productions on a.Production equals c.ProductionId
                                       where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       select a).Sum(x => (decimal?)x.Qty) ?? 0;

                    var SubProdItem = (from a in db.GeneratedItem
                                       join c in db.Productions on a.Production equals c.ProductionId
                                       where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       select a).Sum(x => (decimal?)x.Qty) ?? 0;

                    var PriProdCItem = (from a in db.ProItems
                                        join c in db.Productions on a.Production equals c.ProductionId
                                        where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select a).Sum(x => (decimal?)x.Quantity) ?? 0;

                    var SubProdCItem = (from a in db.ProItems
                                        join c in db.Productions on a.Production equals c.ProductionId
                                        where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select a).Sum(x => (decimal?)x.Quantity) ?? 0;

                    var PriUnItem = (from a in db.ConsumedItem
                                     join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                     where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select a).Sum(x => (decimal?)x.Qty) ?? 0;

                    var SubUnItem = (from a in db.ConsumedItem
                                     join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                     where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select a).Sum(x => (decimal?)x.Qty) ?? 0;

                    var PriUnCItem = (from a in db.UnassembleItems
                                      join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                      where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.Quantity) ?? 0;

                    var SubUnCItem = (from a in db.UnassembleItems
                                      join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                      where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.Quantity) ?? 0;


                    var PriStTrFrom = (from a in db.StockTransferItems
                                       join c in db.StockTransfers on a.StockTransferId equals c.Id
                                       where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                       && (ddmc == c.MCFrom)
                                       select a).Sum(m => (decimal?)m.Quantity) ?? 0;

                    var PriStTrTo = (from a in db.StockTransferItems
                                     join c in db.StockTransfers on a.StockTransferId equals c.Id
                                     where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                     && (ddmc == c.MCTo)
                                     select a).Sum(m => (decimal?)m.Quantity) ?? 0;
                    var SubStTrFrom = (from a in db.StockTransferItems
                                       join c in db.StockTransfers on a.StockTransferId equals c.Id
                                       where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                       && (ddmc == c.MCFrom)
                                       select a).Sum(m => (decimal?)m.Quantity) ?? 0;

                    var SubStTrTo = (from a in db.StockTransferItems
                                     join c in db.StockTransfers on a.StockTransferId equals c.Id
                                     where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                     && (ddmc == c.MCTo)
                                     select a).Sum(m => (decimal?)m.Quantity) ?? 0;

                    var PriHDNote = (from a in db.SEItemss
                                     join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                     let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                     where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     && (c.SaleType == SaleType.Hire) && chkextend == null
                                     select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubHDNote = (from a in db.SEItemss
                                     join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                     let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                     where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     && (c.SaleType == SaleType.Hire) && chkextend == null
                                     select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriRetNote = (from a in db.HrItems
                                      join c in db.HireReturns on a.Hr equals c.HireReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubRetNote = (from a in db.HrItems
                                      join c in db.HireReturns on a.Hr equals c.HireReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriHireMiss = (from a in db.HrItems
                                       join c in db.HireReturns on a.Hr equals c.HireReturnId
                                       where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Missing")
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubHireMiss = (from a in db.HrItems
                                       join c in db.HireReturns on a.Hr equals c.HireReturnId
                                       where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Missing")
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;
                    var PriPurchaseCross = (from a in db.PEItemss
                                            join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                            let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                            where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                            && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                            && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                            select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubPurchaseCross = (from a in db.PEItemss
                                            join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                            let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                            where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit) &&
                                            ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                            && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                            select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;
                    var PriRetNoteCross = (from a in db.CrossHrItems
                                           join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                           where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                           && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                           select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubRetNoteCross = (from a in db.CrossHrItems
                                           join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                           where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
                                           && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                           select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;


                    return new
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

                        PriPurchase = (PriPurchase + (int)(SubPurchase / b.ConFactor)),
                        SubPurchase = (SubPurchase % b.ConFactor),

                        PriSale = (PriSale + (int)(SubSale / b.ConFactor)),
                        SubSale = (SubSale % b.ConFactor),

                        PriPReturn = (PriPReturn + (int)(SubPReturn / b.ConFactor)),
                        SubPReturn = (SubPReturn % b.ConFactor),

                        PriSReturn = (PriSReturn + (int)(SubSReturn / b.ConFactor)),
                        SubSReturn = (SubSReturn % b.ConFactor),

                        PriAddAdj,
                        SubAddAdj,
                        PriLessAdj,
                        subLessAdj,

                        PriStTrFrom,
                        PriStTrTo,
                        SubStTrFrom,
                        SubStTrTo,

                        PriHDNote,
                        SubHDNote,

                        PriRetNote,
                        SubRetNote,

                        PriHireMiss,
                        SubHireMiss,

                        PriPurchaseCross,
                        SubPurchaseCross,

                        PriRetNoteCross,
                        SubRetNoteCross,

                        pritotal = ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)),
                        subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)),

                        total = (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)), // - (StTrFrom - StTrTo)

                        stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss)), // + StTrTo
                        stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom ) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote )), // + StTrFrom
                        cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                        // stock outside
                        stockOutside = ((PriHDNote - (PriRetNote + PriHireMiss)) * b.ConFactor) + (SubHDNote - (SubRetNote + SubHireMiss)),
                        b.price,
                        stcost = (b.stcostu == b.ItemUnitID) ? b.stcost : (b.stcost * b.ConFactor),
                        MC = ddmc
                    };
                }).OrderBy(a => a.ItemName).ToList();

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? mydata.OrderBy(a => a.ItemCode).ToList() : mydata;
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion

        #region as on date
        // GET: StockReport on date
        [QkAuthorize(Roles = "Dev,Stock As On Date")]
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
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName
            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        [QkAuthorize(Roles = "Dev,Stock As On Date,HireStock As On Date")]
        public ActionResult GetOnDate(string ondate, bool stockable, long? ddmc)
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

            DateTime? ondates = null;
            if (ondate != "")
            {
                ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
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

                     let stv = (from az in db.StockTransferItems
                                join cz in db.StockTransfers on az.StockTransferId equals cz.Id
                                where (az.Item == b.ItemID) && cz.MCTo == ddmc
                                select new
                                {
                                    az.Price,
                                    az.Unit
                                }).FirstOrDefault()

                     where (stockable == true || b.KeepStock == true)

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
                         OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         b.KeepStock,

                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                         stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                         stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                         price = b.SellingPrice
                     }).Distinct();
            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            var mydata =
                data.Select(o =>
                {

                 var PriPurchase = (from a in db.PEItemss
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                    && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);


                 var SubPurchase = (from a in db.PEItemss
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);


                 var PriSale = (from a in db.SEItemss
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && (ondate == "" || EF.Functions.DateDiffDay(c.SEDate, ondates) >= 0)
                                && (c.SaleType != SaleType.Hire)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);


                 var SubSale = (from a in db.SEItemss
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where
                                (ondate == "" || EF.Functions.DateDiffDay(c.SEDate, ondates) >= 0)
                                && (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && (c.SaleType != SaleType.Hire)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);


                 var PriPReturn = (from a in db.PRItemss
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                   && (ondate == "" || EF.Functions.DateDiffDay(c.PRDate, ondates) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var SubPReturn = (from a in db.PRItemss
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                    && (ondate == "" || EF.Functions.DateDiffDay(c.PRDate, ondates) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var PriSReturn = (from a in db.SRItemss
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                   && (ondate == "" || EF.Functions.DateDiffDay(c.SRDate, ondates) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var SubSReturn = (from a in db.SRItemss
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                    && (ondate == "" || EF.Functions.DateDiffDay(c.SRDate, ondates) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var PriAddAdj = (from a in db.StockAdjustments
                                  where (a.ItemID == o.ItemID && a.ItemUnitID == o.ItemUnitID && a.AdjustmentType == AdjustmentType.Add)
                                  && (ondate == "" || EF.Functions.DateDiffDay(a.AdjDate, ondates) >= 0)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);


                 var SubAddAdj = (from a in db.StockAdjustments
                                  where (a.ItemID == o.ItemID && a.ItemUnitID == o.SubUnitId
                                  && a.AdjustmentType == AdjustmentType.Add && o.PriUnit != o.SubUnit)
                                  && (ondate == "" || EF.Functions.DateDiffDay(a.AdjDate, ondates) >= 0)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);


                 var PriLessAdj = (from a in db.StockAdjustments
                                   where (a.ItemID == o.ItemID && a.ItemUnitID == o.ItemUnitID && a.AdjustmentType == AdjustmentType.Less)
                                   && (ondate == "" || EF.Functions.DateDiffDay(a.AdjDate, ondates) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var subLessAdj = (from a in db.StockAdjustments
                                   where (a.ItemID == o.ItemID && a.ItemUnitID == o.SubUnitId
                                   && a.AdjustmentType == AdjustmentType.Less && o.PriUnit != o.SubUnit)
                                   && (ondate == "" || EF.Functions.DateDiffDay(a.AdjDate, ondates) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var PriProdItem = (from a in db.GeneratedItem
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == o.ItemID && a.Unit == o.ItemUnitID)
                                    && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);


                 var SubProdItem = (from a in db.GeneratedItem
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                    && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);


                 var PriProdCItem = (from a in db.ProItems
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == o.ItemID && a.Unit == o.ItemUnitID)
                                     && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);


                 var SubProdCItem = (from a in db.ProItems
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                     && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);


                 var PriUnItem = (from a in db.ConsumedItem
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == o.ItemID && a.Unit == o.ItemUnitID)
                                  && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var SubUnItem = (from a in db.ConsumedItem
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var PriUnCItem = (from a in db.UnassembleItems
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == o.ItemID && a.Unit == o.ItemUnitID)
                                   && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(x => x.Quantity);

                 var SubUnCItem = (from a in db.UnassembleItems
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                   && (ondate == "" || EF.Functions.DateDiffDay(c.PEDate, ondates) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(x => x.Quantity);



                 var PriStTrFrom = (from a in db.StockTransferItems
                                    join c in db.StockTransfers on a.StockTransferId equals c.Id
                                    where (a.Item == o.ItemID && a.Unit == o.ItemUnitID)
                                    && (ondate == "" || EF.Functions.DateDiffDay(c.Date, ondates) >= 0)
                                    && (ddmc == c.MCFrom)
                                    select new
                                    {
                                        a.Quantity
                                    }).ToList().Sum(m => m.Quantity);

                 var PriStTrTo = (from a in db.StockTransferItems
                                  join c in db.StockTransfers on a.StockTransferId equals c.Id
                                  where (a.Item == o.ItemID && a.Unit == o.ItemUnitID)
                                  && (ondate == "" || EF.Functions.DateDiffDay(c.Date, ondates) >= 0)
                                  && (ddmc == c.MCTo)
                                  select new
                                  {
                                      a.Quantity
                                  }).ToList().Sum(m => m.Quantity);
                 var SubStTrFrom = (from a in db.StockTransferItems
                                    join c in db.StockTransfers on a.StockTransferId equals c.Id
                                    where (a.Item == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                    && (ondate == "" || EF.Functions.DateDiffDay(c.Date, ondates) >= 0)
                                    && (ddmc == c.MCFrom)
                                    select new
                                    {
                                        a.Quantity
                                    }).ToList().Sum(m => m.Quantity);

                 var SubStTrTo = (from a in db.StockTransferItems
                                  join c in db.StockTransfers on a.StockTransferId equals c.Id
                                  where (a.Item == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && (ondate == "" || EF.Functions.DateDiffDay(c.Date, ondates) >= 0)
                                  && (ddmc == c.MCTo)
                                  select new
                                  {
                                      a.Quantity
                                  }).ToList().Sum(m => m.Quantity);

                 var PriHDNote = (from a in db.SEItemss
                                  join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                  let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                  where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  && (c.SaleType == SaleType.Hire) && chkextend == null
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var SubHDNote = (from a in db.SEItemss
                                  join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                  let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                  where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  && (c.SaleType == SaleType.Hire) && chkextend == null
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var PriRetNote = (from a in db.HrItems
                                   join c in db.HireReturns on a.Hr equals c.HireReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID && c.RtType == "Return")
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubRetNote = (from a in db.HrItems
                                   join c in db.HireReturns on a.Hr equals c.HireReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit && c.RtType == "Return")
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriHireMiss = (from a in db.HrItems
                                    join c in db.HireReturns on a.Hr equals c.HireReturnId
                                    where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID && c.RtType == "Missing")
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var SubHireMiss = (from a in db.HrItems
                                    join c in db.HireReturns on a.Hr equals c.HireReturnId
                                    where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit && c.RtType == "Missing")
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var PriPurchaseCross = (from a in db.PEItemss
                                         join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                         let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                         where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                         && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).ToList().Sum(x => x.ItemQuantity);

                 var SubPurchaseCross = (from a in db.PEItemss
                                         join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                         let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                         where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit) &&
                                         ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                         && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).ToList().Sum(x => x.ItemQuantity);
                 var PriRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID && c.RtType == "Return")
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity);

                 var SubRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit && c.RtType == "Return")
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity);

                 return new
                 {
                     o.ItemID,
                     o.ItemCode,
                     o.ItemName,
                     o.ItemWithCode,
                     o.ItemUnitID,
                     o.SubUnitId,
                     PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                     SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                     o.categoryname,
                     OpeningStock = o.OpeningStock,
                     MinStock = (o.MinStock != null) ? o.MinStock : 0,
                     o.ConFactor,
                     stockable = o.KeepStock,

                     PriPurchase = (PriPurchase + (int)(SubPurchase / o.ConFactor)),
                     SubPurchase = (SubPurchase % o.ConFactor),

                     PriSale = (PriSale + (int)(SubSale / o.ConFactor)),
                     SubSale = (SubSale % o.ConFactor),

                     PriPReturn = (PriPReturn + (int)(SubPReturn / o.ConFactor)),
                     SubPReturn = (SubPReturn % o.ConFactor),

                     PriSReturn = (PriSReturn + (int)(SubSReturn / o.ConFactor)),
                     SubSReturn = (SubSReturn % o.ConFactor),

                     PriStTrFrom,
                     PriStTrTo,
                     SubStTrFrom,
                     SubStTrTo,

                     PriAddAdj,
                     SubAddAdj,
                     PriLessAdj,
                     subLessAdj,

                     PriHDNote,
                     SubHDNote,

                     PriRetNote,
                     SubRetNote,

                     PriHireMiss,
                     SubHireMiss,

                     PriPurchaseCross,
                     SubPurchaseCross,

                     PriRetNoteCross,
                     SubRetNoteCross,

                     pritotal = ((o.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)),
                     subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss+ SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross )),

                     total = (((o.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)) * o.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)), // - (StTrFrom - StTrTo)

                     stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo) * o.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss)), // + StTrTo
                     stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom) * o.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)), // + StTrFrom 
                     cost = (o.costu == o.ItemUnitID) ? o.cost : (o.cost * o.ConFactor),
                     o.price,
                     stcost = (o.stcostu == o.ItemUnitID) ? o.stcost : (o.stcost * o.ConFactor),
                     MC = ddmc
                 };
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
        // GET: StockReport Item Wise 
        #region itemwise
        [QkAuthorize(Roles = "Dev,Stock Item Wise")]
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

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Item Wise")]
        public ActionResult ItemWise(long? ddlItem, long? ddlMC)
        {
            return RedirectToAction("ViewItemWise", new { itemid = ddlItem, ddmc = ddlMC });
        }
        [QkAuthorize(Roles = "Dev,Stock Item Wise,HireStock Item Wise")]
        public ActionResult GetItemWise(long? itemid, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            
            
               var v = (itemid==0)?
                (from b in db.Items
                         join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                         from c in primary.DefaultIfEmpty()
                         join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                         from d in second.DefaultIfEmpty()
                         join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                         from e in cat.DefaultIfEmpty()
                         join f in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.PEItemsId).Max() }
                         equals new { f1 = f.Item, f2 = f.PEItemsId } into pur
                         from f in pur.DefaultIfEmpty()
                         join g in db.StockTransferItems on b.ItemID equals g.Item
                         let stv = (from az in db.StockTransferItems
                                    join cz in db.StockTransfers on az.StockTransferId equals cz.Id
                                    where (az.Item == b.ItemID) && cz.MCTo == ddmc
                                    select new
                                    {
                                        az.Price,
                                        az.Unit
                                    }).FirstOrDefault()
                         where (itemid == 0 || b.ItemID == itemid)
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
                             OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                             b.MinStock,
                             categoryname = e.ItemCategoryName,
                             b.KeepStock,
                             b.PartNumber,
                             PIUnitName = c.ItemUnitName,
                             SIUnitName = d.ItemUnitName,

                             cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                             costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                             stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                             stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                             price = b.SellingPrice,
                         }).Distinct(): 
               (from b in db.Items
                         join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                         from c in primary.DefaultIfEmpty()
                         join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                         from d in second.DefaultIfEmpty()
                         join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                         from e in cat.DefaultIfEmpty()
                         join f in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.PEItemsId).Max() }
                         equals new { f1 = f.Item, f2 = f.PEItemsId } into pur
                         from f in pur.DefaultIfEmpty()
                     
                         let stv = (from az in db.StockTransferItems
                                    join cz in db.StockTransfers on az.StockTransferId equals cz.Id
                                    where (az.Item == b.ItemID) && cz.MCTo == ddmc
                                    select new
                                    {
                                        az.Price,
                                        az.Unit
                                    }).FirstOrDefault()
                         where (itemid == 0 || b.ItemID == itemid)
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
                             OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                             b.MinStock,
                             categoryname = e.ItemCategoryName,
                             b.KeepStock,
                             b.PartNumber,
                             PIUnitName = c.ItemUnitName,
                             SIUnitName = d.ItemUnitName,

                             cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                             costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                             stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                             stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                             price = b.SellingPrice,
                         }).Distinct();
            
            
           

            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            var mydata =
                data.Select(b =>
                {
                    var PriPurchase = (from a in db.PEItemss
                                       join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                       where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType != PurchaseHireType.CrossHire)
                                       select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubPurchase = (from a in db.PEItemss
                                       join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                       where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType != PurchaseHireType.CrossHire)
                                       select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriSale = (from a in db.SEItemss
                                   join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubSale = (from a in db.SEItemss
                                   join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriPReturn = (from a in db.PRItemss
                                      join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType != PurchaseHireType.CrossHire)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubPReturn = (from a in db.PRItemss
                                      join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType != PurchaseHireType.CrossHire)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriSReturn = (from a in db.SRItemss
                                      join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      && (c.SaleType != SaleType.Hire)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubSReturn = (from a in db.SRItemss
                                      join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      && (c.SaleType != SaleType.Hire)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriAddAdj = (from a in db.StockAdjustments
                                     where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                     select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubAddAdj = (from a in db.StockAdjustments
                                     where (a.ItemID == b.ItemID && b.PriUnit != b.SubUnit && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                     select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriLessAdj = (from a in db.StockAdjustments
                                      where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var subLessAdj = (from a in db.StockAdjustments
                                      where (a.ItemID == b.ItemID && b.PriUnit != b.SubUnit && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriProdItem = (from a in db.GeneratedItem
                                       join c in db.Productions on a.Production equals c.ProductionId
                                       where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       select a).Sum(x => (decimal?)x.Qty) ?? 0;

                    var SubProdItem = (from a in db.GeneratedItem
                                       join c in db.Productions on a.Production equals c.ProductionId
                                       where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       select a).Sum(x => (decimal?)x.Qty) ?? 0;

                    var PriProdCItem = (from a in db.ProItems
                                        join c in db.Productions on a.Production equals c.ProductionId
                                        where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select a).Sum(x => (decimal?)x.Quantity) ?? 0;

                    var SubProdCItem = (from a in db.ProItems
                                        join c in db.Productions on a.Production equals c.ProductionId
                                        where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select a).Sum(x => (decimal?)x.Quantity) ?? 0;

                    var PriUnItem = (from a in db.ConsumedItem
                                     join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                     where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select a).Sum(x => (decimal?)x.Qty) ?? 0;

                    var SubUnItem = (from a in db.ConsumedItem
                                     join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                     where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select a).Sum(x => (decimal?)x.Qty) ?? 0;

                    var PriUnCItem = (from a in db.UnassembleItems
                                      join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                      where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.Quantity) ?? 0;

                    var SubUnCItem = (from a in db.UnassembleItems
                                      join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                      where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.Quantity) ?? 0;


                    var PriStTrFrom = (from a in db.StockTransferItems
                                       join c in db.StockTransfers on a.StockTransferId equals c.Id
                                       where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                       && (ddmc == c.MCFrom)
                                       select a).Sum(m => (decimal?)m.Quantity) ?? 0;

                    var PriStTrTo = (from a in db.StockTransferItems
                                     join c in db.StockTransfers on a.StockTransferId equals c.Id
                                     where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                     && (ddmc == c.MCTo)
                                     select a).Sum(m => (decimal?)m.Quantity) ?? 0;
                    var SubStTrFrom = (from a in db.StockTransferItems
                                       join c in db.StockTransfers on a.StockTransferId equals c.Id
                                       where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                        && (ddmc == c.MCFrom)
                                       select a).Sum(m => (decimal?)m.Quantity) ?? 0;

                    var SubStTrTo = (from a in db.StockTransferItems
                                     join c in db.StockTransfers on a.StockTransferId equals c.Id
                                     where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                     && (ddmc == c.MCTo)
                                     select a).Sum(m => (decimal?)m.Quantity) ?? 0;

                    var PriHDNote = (from a in db.SEItemss
                                     join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                     let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                     where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     && (c.SaleType == SaleType.Hire) && chkextend == null
                                     select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubHDNote = (from a in db.SEItemss
                                     join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId

                                     let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                     where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     && (c.SaleType == SaleType.Hire) && chkextend == null
                                     select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriRetNote = (from a in db.HrItems
                                      join c in db.HireReturns on a.Hr equals c.HireReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubRetNote = (from a in db.HrItems
                                      join c in db.HireReturns on a.Hr equals c.HireReturnId
                                      where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
                                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                      select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriHireMiss = (from a in db.HrItems
                                       join c in db.HireReturns on a.Hr equals c.HireReturnId
                                       where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Missing")
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubHireMiss = (from a in db.HrItems
                                       join c in db.HireReturns on a.Hr equals c.HireReturnId
                                       where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Missing")
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var PriPurchaseCross = (from a in db.PEItemss
                                            join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                            let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                            where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID) &&
                                            ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                            && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                            select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubPurchaseCross = (from a in db.PEItemss
                                            join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                            let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                            where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit) &&
                                            ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                            && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                            select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;
                    var PriRetNoteCross = (from a in db.CrossHrItems
                                           join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                           where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                           && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                           select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    var SubRetNoteCross = (from a in db.CrossHrItems
                                           join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                           where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
                                           && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                           select a).Sum(x => (decimal?)x.ItemQuantity) ?? 0;

                    return new
                    {
                        b.ItemID,
                        b.ItemCode,
                        b.ItemName,
                        b.ItemWithCode,
                        b.ItemUnitID,
                        b.SubUnitId,
                        PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                        PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                        SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                        b.categoryname,
                        OpeningStock = b.OpeningStock,
                        MinStock = (b.MinStock != null) ? b.MinStock : 0,
                        b.ConFactor,
                        stockable = b.KeepStock,

                        PriPurchase = (PriPurchase + (int)(SubPurchase / b.ConFactor)),
                        SubPurchase = (SubPurchase % b.ConFactor),

                        PriStTrFrom,
                        PriStTrTo,
                        SubStTrFrom,
                        SubStTrTo,

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

                        PriHDNote,
                        SubHDNote,

                        PriRetNote,
                        SubRetNote,

                        PriHireMiss,
                        SubHireMiss,

                        PriPurchaseCross,
                        SubPurchaseCross,

                        PriRetNoteCross,
                        SubRetNoteCross,

                        pritotal = ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)),
                        subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)),

                        total = (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)), // - (StTrFrom - StTrTo)

                        stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo ) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss )), // + StTrTo
                        stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)), // + StTrFrom
                        cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                        b.price,
                        stcost = (b.stcostu == b.ItemUnitID) ? b.stcost : (b.stcost * b.ConFactor),
                        MC = ddmc,
                    };
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

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Item Wise")]
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
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }

        #endregion

        #region category wise
        // GET: StockReport Category Wise
        [QkAuthorize(Roles = "Dev,Stock Category Wise")]
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

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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
        [QkAuthorize(Roles = "Dev,Stock Category Wise")]
        public ActionResult CategoryWise(long? ddlCategory, long? ddlMC, bool stockable = false)
        {
            return RedirectToAction("ViewCategoryWise", new { categoryid = ddlCategory, stockable = stockable, ddmc = ddlMC });
        }
        [QkAuthorize(Roles = "Dev,Stock Category Wise,HireStock Category Wise")]
        public ActionResult GetCategoryWise(long? categoryid, bool stockable, long? ddmc)
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

                     let stv = (from az in db.StockTransferItems
                                join cz in db.StockTransfers on az.StockTransferId equals cz.Id
                                where (az.Item == b.ItemID) && cz.MCTo == ddmc
                                select new
                                {
                                    az.Price,
                                    az.Unit
                                }).FirstOrDefault()
                     where (stockable == true || b.KeepStock == true) &&
                        (categoryid == 0 || e.ItemCategoryID == categoryid)
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
                         OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         ItemCategoryID = b.ItemCategoryID,
                         b.KeepStock,
                         PIUnitName = c.ItemUnitName,
                         SIUnitName = d.ItemUnitName,


                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                         stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                         stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                         price = b.SellingPrice
                     }).Distinct();
            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            var mydata =
                data.Select(b =>
                {

                 var PriPurchase = (from a in db.PEItemss
                                    join d in db.Items on a.Item equals d.ItemID
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                    && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var SubPurchase = (from a in db.PEItemss
                                    join d in db.Items on a.Item equals d.ItemID
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var PriSale = (from a in db.SEItemss
                                join d in db.Items on a.Item equals d.ItemID
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && (c.SaleType != SaleType.Hire)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);

                 var SubSale = (from a in db.SEItemss
                                join d in db.Items on a.Item equals d.ItemID
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && (c.SaleType != SaleType.Hire)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);

                 var PriPReturn = (from a in db.PRItemss
                                   join d in db.Items on a.Item equals d.ItemID
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubPReturn = (from a in db.PRItemss
                                   join d in db.Items on a.Item equals d.ItemID
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriSReturn = (from a in db.SRItemss
                                   join d in db.Items on a.Item equals d.ItemID
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubSReturn = (from a in db.SRItemss
                                   join d in db.Items on a.Item equals d.ItemID
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriAddAdj = (from a in db.StockAdjustments
                                  join d in db.Items on a.ItemID equals d.ItemID
                                  where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var SubAddAdj = (from a in db.StockAdjustments
                                  join d in db.Items on a.ItemID equals d.ItemID
                                  where (a.ItemID == b.ItemID && b.PIUnitName != b.SIUnitName && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add)
                                  && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var PriLessAdj = (from a in db.StockAdjustments
                                   join d in db.Items on a.ItemID equals d.ItemID
                                   where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var subLessAdj = (from a in db.StockAdjustments
                                   join d in db.Items on a.ItemID equals d.ItemID
                                   where (a.ItemID == b.ItemID && b.PIUnitName != b.SIUnitName && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriProdItem = (from a in db.GeneratedItem
                                    join d in db.Items on a.Item equals d.ItemID
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                    && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);

                 var SubProdItem = (from a in db.GeneratedItem
                                    join d in db.Items on a.Item equals d.ItemID
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);

                 var PriProdCItem = (from a in db.ProItems
                                     join d in db.Items on a.ItemId equals d.ItemID
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                     && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);

                 var SubProdCItem = (from a in db.ProItems
                                     join d in db.Items on a.ItemId equals d.ItemID
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                     && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);

                 var PriUnItem = (from a in db.ConsumedItem
                                  join d in db.Items on a.Item equals d.ItemID
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                  && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var SubUnItem = (from a in db.ConsumedItem
                                  join d in db.Items on a.Item equals d.ItemID
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var PriUnCItem = (from a in db.UnassembleItems
                                   join d in db.Items on a.ItemId equals d.ItemID
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(x => x.Quantity);

                 var SubUnCItem = (from a in db.UnassembleItems
                                   join d in db.Items on a.ItemId equals d.ItemID
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && (categoryid == 0 || d.ItemCategoryID == categoryid)
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
                 var PriPurchaseCross = (from a in db.PEItemss
                                         join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                         let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                         where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                         && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).ToList().Sum(x => x.ItemQuantity);

                 var SubPurchaseCross = (from a in db.PEItemss
                                         join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                         let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                         where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit) &&
                                         ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                         && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).ToList().Sum(x => x.ItemQuantity);
                 var PriRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity);

                 var SubRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
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
                     PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                     SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                     b.categoryname,
                     OpeningStock = b.OpeningStock,
                     MinStock = (b.MinStock != null) ? b.MinStock : 0,
                     b.ConFactor,
                     stockable = b.KeepStock,

                     PriStTrFrom,
                     PriStTrTo,
                     SubStTrFrom,
                     SubStTrTo,

                     PriPurchase = (PriPurchase + (int)(SubPurchase / b.ConFactor)),
                     SubPurchase = (SubPurchase % b.ConFactor),


                     PriSale = (PriSale + ((int)SubSale / b.ConFactor)),
                     SubSale = (SubSale % b.ConFactor),

                     PriPReturn = (PriPReturn + (int)(SubPReturn / b.ConFactor)),
                     SubPReturn = (SubPReturn % b.ConFactor),


                     PriSReturn = (PriSReturn + (int)(SubSReturn / b.ConFactor)),
                     SubSReturn = (SubSReturn % b.ConFactor),


                     PriAddAdj = PriAddAdj,
                     SubAddAdj = SubAddAdj,
                     PriLessAdj = PriLessAdj,
                     subLessAdj = subLessAdj,


                     PriHDNote,
                     SubHDNote,

                     PriRetNote,
                     SubRetNote,

                     PriHireMiss,
                     SubHireMiss,

                     PriPurchaseCross,
                     SubPurchaseCross,

                     PriRetNoteCross,
                     SubRetNoteCross,

                     pritotal = ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)),
                     subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)),

                     total = (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)), // - (StTrFrom - StTrTo)

                     stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss)), // + StTrTo
                     stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom ) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)), // + StTrFrom 
                     cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                     b.price,
                     stcost = (b.stcostu == b.ItemUnitID) ? b.stcost : (b.stcost * b.ConFactor),
                     MC = ddmc,
                 };
                }).OrderBy(a => a.categoryname).ThenBy(a => a.ItemName).ToList();

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

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Category Wise")]
        public ActionResult ViewCategoryWise(long? itemid, string datefrom,string dateto, long? ddmc)
        {
           
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
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }

        #endregion

        #region brand wise

        // GET: StockReport Brand Wise
        [QkAuthorize(Roles = "Dev,Stock Brand Wise")]
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
        [QkAuthorize(Roles = "Dev,Stock Brand Wise")]
        public ActionResult BrandWise(long? ddlBrand, long? ddlMC, bool stockable = false)
        {
            return RedirectToAction("ViewBrandWise", new { brandid = ddlBrand, ddmc = ddlMC, stockable = stockable });
        }
        [QkAuthorize(Roles = "Dev,Stock Brand Wise,HireStock Brand Wise")]
        public ActionResult GetBrandWise(long? brandid, bool stockable, long? ddmc)
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

            var v = (from b in db.Items
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join h in db.ItemBrands on b.ItemBrandID equals h.ItemBrandID into brand
                     from h in brand.DefaultIfEmpty()
                     join f in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.PEItemsId).Max() }
                     equals new { f1 = f.Item, f2 = f.PEItemsId } into pur
                     from f in pur.DefaultIfEmpty()

                     let stv = (from az in db.StockTransferItems
                                join cz in db.StockTransfers on az.StockTransferId equals cz.Id
                                where (az.Item == b.ItemID) && cz.MCTo == ddmc
                                select new
                                {
                                    az.Price,
                                    az.Unit
                                }).FirstOrDefault()
                     where (stockable == true || b.KeepStock == true) && (brandid == 0 || b.ItemBrandID == brandid)
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
                         OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         b.KeepStock,
                         h.ItemBrandName,
                         PIUnitName = c.ItemUnitName,
                         SIUnitName = d.ItemUnitName,

                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                         stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                         stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                         price = b.SellingPrice
                     }).Distinct();
            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            var mydata =
                data.Select(b =>
                {


                 var PriPurchase = (from a in db.PEItemss
                                    join d in db.Items on a.Item equals d.ItemID
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                    && (brandid == 0 || d.ItemBrandID == brandid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);


                 var SubPurchase = (from a in db.PEItemss
                                    join d in db.Items on a.Item equals d.ItemID
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && (brandid == 0 || d.ItemBrandID == brandid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var PriSale = (from a in db.SEItemss
                                join d in db.Items on a.Item equals d.ItemID
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                && (brandid == 0 || d.ItemBrandID == brandid)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && (c.SaleType != SaleType.Hire)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);

                 var SubSale = (from a in db.SEItemss
                                join d in db.Items on a.Item equals d.ItemID
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                && (brandid == 0 || d.ItemBrandID == brandid)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && (c.SaleType != SaleType.Hire)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);

                 var PriPReturn = (from a in db.PRItemss
                                   join d in db.Items on a.Item equals d.ItemID
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && (brandid == 0 || d.ItemBrandID == brandid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubPReturn = (from a in db.PRItemss
                                   join d in db.Items on a.Item equals d.ItemID
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && (brandid == 0 || d.ItemBrandID == brandid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriSReturn = (from a in db.SRItemss
                                   join d in db.Items on a.Item equals d.ItemID
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && (brandid == 0 || d.ItemBrandID == brandid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubSReturn = (from a in db.SRItemss
                                   join d in db.Items on a.Item equals d.ItemID
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && (brandid == 0 || d.ItemBrandID == brandid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && (c.SaleType != SaleType.Hire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriAddAdj = (from a in db.StockAdjustments
                                  join d in db.Items on a.ItemID equals d.ItemID
                                  where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add)
                                  && (brandid == 0 || d.ItemBrandID == brandid)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var SubAddAdj = (from a in db.StockAdjustments
                                  join d in db.Items on a.ItemID equals d.ItemID
                                  where (a.ItemID == b.ItemID && b.PIUnitName != b.SIUnitName && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add)
                                  && (brandid == 0 || d.ItemBrandID == brandid)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var PriLessAdj = (from a in db.StockAdjustments
                                   join d in db.Items on a.ItemID equals d.ItemID
                                   where (a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less)
                                   && (brandid == 0 || d.ItemBrandID == brandid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var subLessAdj = (from a in db.StockAdjustments
                                   join d in db.Items on a.ItemID equals d.ItemID
                                   where (a.ItemID == b.ItemID && b.PIUnitName != b.SIUnitName && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less)
                                   && (brandid == 0 || d.ItemBrandID == brandid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriProdItem = (from a in db.GeneratedItem
                                    join d in db.Items on a.Item equals d.ItemID
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                    && (brandid == 0 || d.ItemBrandID == brandid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);

                 var SubProdItem = (from a in db.GeneratedItem
                                    join d in db.Items on a.Item equals d.ItemID
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && (brandid == 0 || d.ItemBrandID == brandid)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);

                 var PriProdCItem = (from a in db.ProItems
                                     join d in db.Items on a.ItemId equals d.ItemID
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                     && (brandid == 0 || d.ItemBrandID == brandid)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);

                 var SubProdCItem = (from a in db.ProItems
                                     join d in db.Items on a.ItemId equals d.ItemID
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                     && (brandid == 0 || d.ItemBrandID == brandid)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);

                 var PriUnItem = (from a in db.ConsumedItem
                                  join d in db.Items on a.Item equals d.ItemID
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == b.ItemID && a.Unit == b.ItemUnitID)
                                  && (brandid == 0 || d.ItemBrandID == brandid)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var SubUnItem = (from a in db.ConsumedItem
                                  join d in db.Items on a.Item equals d.ItemID
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                  && (brandid == 0 || d.ItemBrandID == brandid)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var PriUnCItem = (from a in db.UnassembleItems
                                   join d in db.Items on a.ItemId equals d.ItemID
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == b.ItemID && a.Unit == b.ItemUnitID)
                                   && (brandid == 0 || d.ItemBrandID == brandid)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(x => x.Quantity);

                 var SubUnCItem = (from a in db.UnassembleItems
                                   join d in db.Items on a.ItemId equals d.ItemID
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && (brandid == 0 || d.ItemBrandID == brandid)
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
                 var PriPurchaseCross = (from a in db.PEItemss
                                       join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                       let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                       where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                       select new
                                       {
                                           a.ItemQuantity,
                                       }).ToList().Sum(x => x.ItemQuantity);

                 var SubPurchaseCross = (from a in db.PEItemss
                                       join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                       let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                       where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                       select new
                                       {
                                           a.ItemQuantity,
                                       }).ToList().Sum(x => x.ItemQuantity);


                 var PriRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity);

                 var SubRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
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
                     PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                     SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                     b.categoryname,
                     OpeningStock = b.OpeningStock,
                     MinStock = (b.MinStock != null) ? b.MinStock : 0,
                     b.ConFactor,
                     stockable = b.KeepStock,

                     PriPurchase = (PriPurchase + (int)(SubPurchase / b.ConFactor)),
                     SubPurchase = (SubPurchase % b.ConFactor),

                     PriStTrFrom,
                     PriStTrTo,
                     SubStTrFrom,
                     SubStTrTo,

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


                     PriHDNote,
                     SubHDNote,

                     PriRetNote,
                     SubRetNote,

                     PriHireMiss,
                     SubHireMiss,

                     PriPurchaseCross,
                     SubPurchaseCross,

                     PriRetNoteCross,
                     SubRetNoteCross,

                     pritotal = ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)),
                     subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)),

                     total = (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)), // - (StTrFrom - StTrTo)

                     stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss)), // + StTrTo
                     stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom ) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)), // + StTrFrom 
                     cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                     
                     b.ItemBrandName,
                     b.price,
                     stcost = (b.stcostu == b.ItemUnitID) ? b.stcost : (b.stcost * b.ConFactor),
                     MC = ddmc,
                 };
                }).OrderBy(a => a.ItemBrandName).ThenBy(a => a.ItemName).ToList();

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

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Brand Wise")]
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

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }


        #endregion

        #region moment 
        // GET: StockReport Moment
        [QkAuthorize(Roles = "Dev,Stock Moment")]
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
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult Moment(long? ddlItem, long? ddlMC)
        {
            return RedirectToAction("ViewMoment", new { iditem = ddlItem, ddmc = ddlMC });
        }
        [QkAuthorize(Roles = "Dev,Stock Moment")]
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
                        && (c.PurType!=PurchaseHireType.CrossHire)
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
                            StockInP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                            StockInS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
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
                         && (c.PurType != PurchaseHireType.CrossHire)
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
                             StockInP = (decimal?)null,
                             StockInS = (decimal?)null,
                             StockOutP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                             StockOutS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
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
                             StockOutP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                             StockOutS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                             invoiceno = c.BillNo,
                             entry = (DateTime?)c.SECreatedDate
                         });
            var right1 = (from a in db.SRItemss
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                          where a.Item == iditem
                          && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                          && c.SaleType != SaleType.Hire
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
                              Type = "Sales Return",
                              StockInP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                              StockInS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                              StockOutP = (decimal?)null,
                              StockOutS = (decimal?)null,
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
                                invoiceno = a.VoucherNo,
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
            /*
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
            */
            var crosshire = (from a in db.PEItemss
                             join b in db.Items on a.Item equals b.ItemID
                             join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                             let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "Purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                             where a.Item == iditem
                             && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                             && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
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
                                 Type = "Cross Hire",
                                 StockInP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                                 StockInS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                                 StockOutP = (decimal?)null,
                                 StockOutS = (decimal?)null,
                                 invoiceno = c.BillNo,
                                 entry = (DateTime?)c.PECreatedDate
                             });
            var crossreturn = (from a in db.CrossHrItems
                                       join b in db.Items on a.Item equals b.ItemID
                                       join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                       where a.Item == iditem 
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
                                           Type = "Cross Hire Return",
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
            /*
            full = full.Union(hire);
            full = full.Union(returnnote);
            full = full.Union(hiremiss);
            
            */
            full = full.Union(crosshire);
            full = full.Union(crossreturn);
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
        [QkAuthorize(Roles = "Dev,Stock Moment")]
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



        #region moment2 
        // GET: StockReport Moment
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult Moment2()
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
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult Moment2(long? ddlItem, long? ddlMC)
        {
            return RedirectToAction("ViewMoment2", new { iditem = ddlItem, ddmc = ddlMC });
        }
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult GetMoment2(long? iditem, long? ddmc,string datefrom,string dateto)
        {
            var UserId = User.Identity.GetUserId();
            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
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
                        && (c.PurType != PurchaseHireType.CrossHire)
                        && (c.PEDate <= datetos)
                        && (c.PEDate >= datefroms)
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
                            StockInP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                            StockInS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
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
                         && (c.PurType != PurchaseHireType.CrossHire)
                           && (c.PRDate <= datetos)
                            && (c.PRDate >= datefroms)
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
                             StockInP = (decimal?)null,
                             StockInS = (decimal?)null,
                             StockOutP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                             StockOutS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                             invoiceno = c.BillNo,
                             entry = (DateTime?)c.PRCreatedDate
                         });

            var right = (from a in db.SEItemss
                         join b in db.Items on a.Item equals b.ItemID
                         join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                         where a.Item == iditem
                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                         && c.SaleType != SaleType.Hire
                         && (c.SEDate <= datetos)
                           && (c.SEDate >= datefroms)
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
                             StockOutP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                             StockOutS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                             invoiceno = c.BillNo,
                             entry = (DateTime?)c.SECreatedDate
                         });
            var right1 = (from a in db.SRItemss
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                          where a.Item == iditem
                          && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                          && c.SaleType != SaleType.Hire
                            && (c.SRDate <= datetos)
                             && (c.SRDate >= datefroms)
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
                              Type = "Sales Return",
                              StockInP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                              StockInS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                              StockOutP = (decimal?)null,
                              StockOutS = (decimal?)null,
                              invoiceno = c.BillNo,
                              entry = (DateTime?)c.SRCreatedDate
                          });
            //               where a.ItemID == iditem && a.OpeningStock > 0

            //                   Id = a.ItemID,
            //                   ItemCode = a.ItemCode,
            //                   ItemName = a.ItemName,
            //                   ItemWithCode = a.ItemName,
            //                   a.ItemUnitID,
            //                   a.SubUnitId,
            //                   ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
            //                   a.ItemID,
            //                   OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && a.OpeningStock != null) ? a.OpeningStock : 0,
            //                   a.MinStock,
            //                   a.KeepStock,
            //                   Date = (DateTime?)null,
            //                   Type = "Opening Stock",
            //                   StockInP = ((ddmc == 0 || ddmc == 1 || ddmc == null) && a.OpeningStock != null) ? a.OpeningStock : 0,
            //                   StockInS = (decimal?)null,
            //                   StockOutP = (decimal?)null,
            //                   StockOutS = (decimal?)null,
            //                   invoiceno = "",
            //                   entry = (DateTime?)null
            var stockadj = (from a in db.StockAdjustments
                            join b in db.Items on a.ItemID equals b.ItemID
                            where a.ItemID == iditem
                            && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                              && (a.AdjDate <= datetos)
                                   && (a.AdjDate >= datefroms)
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
                            join m in db.MCs on c.MCFrom equals m.MCId 
                            where a.Item == iditem
                            //&& (ddmc == c.MCFrom)
                            && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MCFrom) || ddmc == c.MCFrom)
                             && (c.Date <= datetos)
                                 && (c.Date >= datefroms)
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
                                Type = "Item Transfered  "+m.MCName,
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
                          join m in db.MCs on c.MCTo equals m.MCId
                          where a.Item == iditem
                         // && (ddmc == c.MCTo)
                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MCTo) || ddmc == c.MCTo)
                           && (c.Date <= datetos)
                               && (c.Date >= datefroms)
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
                              Type = "Item Received  " + m.MCName,
                              StockInP = (decimal?)((a.Unit == b.ItemUnitID) ? a.Quantity : 0),
                              StockInS = (decimal?)((a.Unit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.Quantity : 0),
                              StockOutP = (decimal?)null,
                              StockOutS = (decimal?)null,
                              invoiceno = c.Voucher,
                              entry = (DateTime?)c.CreatedDate
                          });
         
            var crosshire = (from a in db.PEItemss
                             join b in db.Items on a.Item equals b.ItemID
                             join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                             let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "Purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                             where a.Item == iditem
                             && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                             && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
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
                                 Type = "Cross Hire",
                                 StockInP = (decimal?)((a.ItemUnit == b.ItemUnitID) ? a.ItemQuantity : 0),
                                 StockInS = (decimal?)((a.ItemUnit == b.SubUnitId && b.ItemUnitID != b.SubUnitId) ? a.ItemQuantity : 0),
                                 StockOutP = (decimal?)null,
                                 StockOutS = (decimal?)null,
                                 invoiceno = c.BillNo,
                                 entry = (DateTime?)c.PECreatedDate
                             });
            var crossreturn = (from a in db.CrossHrItems
                               join b in db.Items on a.Item equals b.ItemID
                               join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                               where a.Item == iditem
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
                                   Type = "Cross Hire Return",
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
            full = full.Union(stockadj);
            full = full.Union(ProdItem);
            full = full.Union(ProdCItem);
            full = full.Union(UnaItem);
            full = full.Union(UnaCItem);
            full = full.Union(TranFrom);
            full = full.Union(TranTo);
            /*
            full = full.Union(hire);
            full = full.Union(returnnote);
            full = full.Union(hiremiss);
            
            */
            full = full.Union(crosshire);
            full = full.Union(crossreturn);
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
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult ViewMoment2(long? itemid, long? ddmc,string datefrom,string dateto)
        {
            if (itemid != 0&& itemid != null)
            {
                ViewBag.ItemName = (from a in db.Items
                                    where a.ItemID == itemid
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
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var SubPurchase = (from a in db.PEItemss
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
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
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubPReturn = (from a in db.PRItemss
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriSReturn = (from a in db.SRItemss
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && c.SaleType != SaleType.Hire
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubSReturn = (from a in db.SRItemss
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && c.SaleType != SaleType.Hire
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
                 var PriPurchaseCross = (from a in db.PEItemss
                                         join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                         let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                         where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID)
                                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                         && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).ToList().Sum(x => x.ItemQuantity);

                 var SubPurchaseCross = (from a in db.PEItemss
                                         join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                         let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                         where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit) &&
                                         ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                         && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                         select new
                                         {
                                             a.ItemQuantity
                                         }).ToList().Sum(x => x.ItemQuantity);
                 var PriRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID && c.RtType == "Return")
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity);

                 var SubRetNoteCross = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PriUnit != b.SubUnit && c.RtType == "Return")
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

                     PriPurchaseCross,
                     SubPurchaseCross,

                     PriRetNoteCross,
                     SubRetNoteCross,

                     pritotal = ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)),
                     subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)),

                     total = (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross)), // - (StTrFrom - StTrTo)

                     stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo ) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss)), // + StTrTo
                     stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom ) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)), // + StTrFrom 
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
            var CrossHire = (from a in v
                         select new
                         {
                             id = 11,
                             a.ItemID,
                             a.PriUnit,
                             PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                             a.SubUnit,
                             name = a.ItemName,
                             type = "CrossHire",
                             Stockin = (a.PriPurchaseCross * a.ConFactor) + a.SubPurchaseCross,
                             StockOut = (decimal)0,
                         });

            var CrossReturn = (from a in v
                               select new
                               {
                                   id = 12,
                                   a.ItemID,
                                   a.PriUnit,
                                   PartNumber = (a.PartNumber != null && a.PartNumber != "") ? a.PartNumber : "",
                                   a.SubUnit,
                                   name = a.ItemName,
                                   type = "CrossReturn",
                                   Stockin = (decimal)0,
                                   StockOut = (a.PriRetNoteCross * a.ConFactor) + a.SubRetNoteCross,
                               });


            /*
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
                            */

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
            full = full.Union(CrossHire);
            full = full.Union(CrossReturn);
            /*
            full = full.Union(Hire);
            full = full.Union(HireRet);
            full = full.Union(HireMiss);
            */

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

        #region Stock Between Dates
        [QkAuthorize(Roles = "Dev,StockBwDates")]
        public ActionResult StockBwDate()
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

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        [QkAuthorize(Roles = "Dev,StockBwDates")]
        public ActionResult GetBwDate(string fromd, string to, bool stockable, long? ddmc)
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
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromd != "")
            {
                fdate = DateTime.Parse(fromd, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB").DateTimeFormat);
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

                     let stv = (from az in db.StockTransferItems
                                join cz in db.StockTransfers on az.StockTransferId equals cz.Id
                                where (az.Item == b.ItemID) && cz.MCTo == ddmc
                                select new
                                {
                                    az.Price,
                                    az.Unit
                                }).FirstOrDefault()
                     where (stockable == true || b.KeepStock == true)
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
                         OpeningStock = ((ddmc == 0 || ddmc == 1 || ddmc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         b.KeepStock,
                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                         stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                         stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                         price = b.SellingPrice
                     }).Distinct();
            v = v.OrderBy(b => b.ItemName);
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            var mydata =
                data.Select(o =>
                {

                 var PriPurchase = (from a in db.PEItemss
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                    && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                    && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);


                 var SubPurchase = (from a in db.PEItemss
                                    join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                    where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                     && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);


                 var PriSale = (from a in db.SEItemss
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && (fromd == "" || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0)
                                && (to == "" || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0)
                                && (c.SaleType != SaleType.Hire)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);


                 var SubSale = (from a in db.SEItemss
                                join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                where
                                (fromd == "" || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0)
                                && (to == "" || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0)
                                && (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                && (c.SaleType != SaleType.Hire)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(x => x.ItemQuantity);


                 var PriPReturn = (from a in db.PRItemss
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                   && (fromd == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0)
                                   && (to == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var SubPReturn = (from a in db.PRItemss
                                   join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                   && (fromd == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0)
                                   && (to == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && (c.PurType != PurchaseHireType.CrossHire)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var PriSReturn = (from a in db.SRItemss
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                   && (fromd == "" || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0)
                                   && (to == "" || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   && c.SaleType != SaleType.Hire
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var SubSReturn = (from a in db.SRItemss
                                   join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                    && (fromd == "" || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0)
                                    && (to == "" || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    && c.SaleType != SaleType.Hire
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var PriAddAdj = (from a in db.StockAdjustments
                                  where (a.ItemID == o.ItemID && a.ItemUnitID == o.ItemUnitID && a.AdjustmentType == AdjustmentType.Add)
                                   && (fromd == "" || EF.Functions.DateDiffDay(a.AdjDate, fdate) <= 0)
                                   && (to == "" || EF.Functions.DateDiffDay(a.AdjDate, tdate) >= 0)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);


                 var SubAddAdj = (from a in db.StockAdjustments
                                  where (a.ItemID == o.ItemID && a.ItemUnitID == o.SubUnitId
                                  && a.AdjustmentType == AdjustmentType.Add && o.PriUnit != o.SubUnit)
                                  && (fromd == "" || EF.Functions.DateDiffDay(a.AdjDate, fdate) <= 0)
                                  && (to == "" || EF.Functions.DateDiffDay(a.AdjDate, tdate) >= 0)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);


                 var PriLessAdj = (from a in db.StockAdjustments
                                   where (a.ItemID == o.ItemID && a.ItemUnitID == o.ItemUnitID && a.AdjustmentType == AdjustmentType.Less)
                                   && (fromd == "" || EF.Functions.DateDiffDay(a.AdjDate, fdate) <= 0)
                                   && (to == "" || EF.Functions.DateDiffDay(a.AdjDate, tdate) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var subLessAdj = (from a in db.StockAdjustments
                                   where (a.ItemID == o.ItemID && a.ItemUnitID == o.SubUnitId
                                   && a.AdjustmentType == AdjustmentType.Less && o.PriUnit != o.SubUnit)
                                   && (fromd == "" || EF.Functions.DateDiffDay(a.AdjDate, fdate) <= 0)
                                   && (to == "" || EF.Functions.DateDiffDay(a.AdjDate, tdate) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity,
                                   }).ToList().Sum(x => x.ItemQuantity);


                 var PriProdItem = (from a in db.GeneratedItem
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == o.ItemID && a.Unit == o.ItemUnitID)
                                    && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                    && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);


                 var SubProdItem = (from a in db.GeneratedItem
                                    join c in db.Productions on a.Production equals c.ProductionId
                                    where (a.Item == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                    && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                    && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.Qty,
                                    }).ToList().Sum(x => x.Qty);


                 var PriProdCItem = (from a in db.ProItems
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == o.ItemID && a.Unit == o.ItemUnitID)
                                     && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                     && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);


                 var SubProdCItem = (from a in db.ProItems
                                     join c in db.Productions on a.Production equals c.ProductionId
                                     where (a.ItemId == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                     && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                     && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                     select new
                                     {
                                         a.Quantity,
                                     }).ToList().Sum(x => x.Quantity);


                 var PriUnItem = (from a in db.ConsumedItem
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == o.ItemID && a.Unit == o.ItemUnitID)
                                  && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                  && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var SubUnItem = (from a in db.ConsumedItem
                                  join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                  where (a.Item == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                  && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(x => x.Qty);

                 var PriUnCItem = (from a in db.UnassembleItems
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == o.ItemID && a.Unit == o.ItemUnitID)
                                   && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                   && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(x => x.Quantity);

                 var SubUnCItem = (from a in db.UnassembleItems
                                   join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                   where (a.ItemId == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                   && (fromd == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0)
                                   && (to == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(x => x.Quantity);


                 var PriStTrFrom = (from a in db.StockTransferItems
                                    join c in db.StockTransfers on a.StockTransferId equals c.Id
                                    where (a.Item == o.ItemID && a.Unit == o.ItemUnitID)
                                    && (fromd == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0)
                                    && (to == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)
                                    && (ddmc == c.MCFrom)
                                    select new
                                    {
                                        a.Quantity
                                    }).ToList().Sum(m => m.Quantity);

                 var PriStTrTo = (from a in db.StockTransferItems
                                  join c in db.StockTransfers on a.StockTransferId equals c.Id
                                  where (a.Item == o.ItemID && a.Unit == o.ItemUnitID)
                                  && (fromd == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0)
                                  && (to == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)
                                  && (ddmc == c.MCTo)
                                  select new
                                  {
                                      a.Quantity
                                  }).ToList().Sum(m => m.Quantity);
                 var SubStTrFrom = (from a in db.StockTransferItems
                                    join c in db.StockTransfers on a.StockTransferId equals c.Id
                                    where (a.Item == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                    && (fromd == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0)
                                    && (to == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)
                                    && (ddmc == c.MCFrom)
                                    select new
                                    {
                                        a.Quantity
                                    }).ToList().Sum(m => m.Quantity);

                 var SubStTrTo = (from a in db.StockTransferItems
                                  join c in db.StockTransfers on a.StockTransferId equals c.Id
                                  where (a.Item == o.ItemID && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && (fromd == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0)
                                  && (to == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)
                                  && (ddmc == c.MCTo)
                                  select new
                                  {
                                      a.Quantity
                                  }).ToList().Sum(m => m.Quantity);

                 var PriHDNote = (from a in db.SEItemss
                                  join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                  let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                  where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  && (c.SaleType == SaleType.Hire) && chkextend == null
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var SubHDNote = (from a in db.SEItemss
                                  join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                                  let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.ConvertTo == "Sale" && x.To == a.SalesEntry).FirstOrDefault()
                                  where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                  && (c.SaleType == SaleType.Hire) && chkextend == null
                                  select new
                                  {
                                      a.ItemQuantity,
                                  }).ToList().Sum(x => x.ItemQuantity);

                 var PriRetNote = (from a in db.HrItems
                                   join c in db.HireReturns on a.Hr equals c.HireReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID && c.RtType == "Return")
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var SubRetNote = (from a in db.HrItems
                                   join c in db.HireReturns on a.Hr equals c.HireReturnId
                                   where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit && c.RtType == "Return")
                                   && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                   select new
                                   {
                                       a.ItemQuantity
                                   }).ToList().Sum(x => x.ItemQuantity);

                 var PriHireMiss = (from a in db.HrItems
                                    join c in db.HireReturns on a.Hr equals c.HireReturnId
                                    where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID && c.RtType == "Missing")
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);

                 var SubHireMiss = (from a in db.HrItems
                                    join c in db.HireReturns on a.Hr equals c.HireReturnId
                                    where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit && c.RtType == "Missing")
                                    && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                    select new
                                    {
                                        a.ItemQuantity
                                    }).ToList().Sum(x => x.ItemQuantity);
                 var PriPurchaseCross = (from a in db.PEItemss
                                       join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                       let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                       where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                       select new
                                       {
                                           a.ItemQuantity,
                                       }).ToList().Sum(x => x.ItemQuantity);

                 var SubPurchaseCross = (from a in db.PEItemss
                                       join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                       let chkextend = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.ConvertTo == "purchase" && x.To == a.PurchaseEntry).FirstOrDefault()
                                       where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                       && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                       && (c.PurType == PurchaseHireType.CrossHire) && chkextend == null
                                       select new
                                       {
                                           a.ItemQuantity,
                                       }).ToList().Sum(x => x.ItemQuantity);

                 var PriCrossRetNote = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == o.ItemID && a.ItemUnit == o.ItemUnitID && c.RtType == "Return")
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity);

                 var SubCrossRetNote = (from a in db.CrossHrItems
                                        join c in db.CrossHireReturns on a.Hr equals c.HireReturnId
                                        where (a.Item == o.ItemID && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit && c.RtType == "Return")
                                        && ((!MCList.Any() && ddmc == null) || MCArray.Contains(c.MaterialCenter) || ddmc == c.MaterialCenter)
                                        select new
                                        {
                                            a.ItemQuantity
                                        }).ToList().Sum(x => x.ItemQuantity);

                 return new
                 {
                     o.ItemID,
                     o.ItemCode,
                     o.ItemName,
                     o.ItemWithCode,
                     o.ItemUnitID,
                     o.SubUnitId,
                     PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                     SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                     o.categoryname,
                     OpeningStock = o.OpeningStock,
                     MinStock = (o.MinStock != null) ? o.MinStock : 0,
                     o.ConFactor,
                     stockable = o.KeepStock,

                     PriPurchase = (PriPurchase + (int)(SubPurchase / o.ConFactor)),
                     SubPurchase = (SubPurchase % o.ConFactor),

                     PriSale = (PriSale + (int)(SubSale / o.ConFactor)),
                     SubSale = (SubSale % o.ConFactor),

                     PriPReturn = (PriPReturn + (int)(SubPReturn / o.ConFactor)),
                     SubPReturn = (SubPReturn % o.ConFactor),

                     PriSReturn = (PriSReturn + (int)(SubSReturn / o.ConFactor)),
                     SubSReturn = (SubSReturn % o.ConFactor),

                     SubStTrFrom,
                     PriStTrFrom,
                     SubStTrTo,
                     PriStTrTo,
                     PriAddAdj,
                     SubAddAdj,
                     PriLessAdj,
                     subLessAdj,

                     PriHDNote,
                     SubHDNote,

                     PriRetNote,
                     SubRetNote,

                     PriHireMiss,
                     SubHireMiss,

                     PriPurchaseCross,
                     SubPurchaseCross,

                     PriCrossRetNote,
                     SubCrossRetNote,

                     pritotal = ((o.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriCrossRetNote)),
                     subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubCrossRetNote)),

                     total = (((o.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriCrossRetNote)) * o.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubCrossRetNote)), // - (StTrFrom - StTrTo)

                     stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo) * o.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss)), // + StTrTo
                     stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom ) * o.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote)), // + StTrFrom 
                     cost = (o.costu == o.ItemUnitID) ? o.cost : (o.cost * o.ConFactor),
                     o.price,
                     stcost = (o.stcostu == o.ItemUnitID) ? o.stcost : (o.stcost * o.ConFactor),
                     MC = ddmc,

                 };
                }).OrderBy(a => a.ItemName).ToList();


            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            mydata = (PreCheck == Status.active) ? mydata.OrderBy(a => a.ItemCode).ToList() : mydata;

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

        #region Stock With Expiry
        // GET: StockReport Expiry
        [QkAuthorize(Roles = "Dev,Stock Expiry")]
        public ActionResult Expiry()
        {

            ViewBag.Category = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Expiry")]
        public ActionResult Expiry(long? ddlItem)
        {
            return RedirectToAction("ViewExpiry", new { iditem = ddlItem });
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Expiry")]
        public ActionResult ViewExpiry(long? iditem)
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
            companySet();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Stock Expiry")]
        public ActionResult GetExpiry(long? iditem)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            var expiry = (from a in db.BatchStocks
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                          from c in primary.DefaultIfEmpty()
                          join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                          from d in second.DefaultIfEmpty()
                          let InStock = (decimal?)db.BatchStocks.Where(z => z.Item == iditem && z.BatchNo == a.BatchNo && z.MFG == a.MFG && z.EXP == a.EXP).Select(z => z.StockIn).Sum()
                          let OutStock = (decimal?)db.BatchStocks.Where(z => z.Item == iditem && z.BatchNo == a.BatchNo && z.MFG == a.MFG && z.EXP == a.EXP).Select(z => z.StockOut).Sum()
                          where (iditem == 0 || iditem == null || b.ItemID == iditem) && (((InStock ?? (decimal)0) - OutStock) != (decimal)0)
                          select new
                          {
                              Id = a.Item,
                              ItemCode = b.ItemCode,
                              ItemName = b.ItemName,
                              BatchNo = a.BatchNo,
                              MFGd = a.MFG,
                              EXPd = a.EXP,
                              cfactor = b.ConFactor,
                              Priunit = b.ItemUnitID,
                              Secunit = b.SubUnitId,
                              PriName = c.ItemUnitName ?? "",
                              SubName = d.ItemUnitName ?? "",
                              Unit = a.Unit,
                              StockIn = InStock ?? 0,
                              StockOut = OutStock ?? 0,
                              Balance = (InStock ?? 0) - (OutStock ?? 0),
                          }).Distinct().AsQueryable().OrderBy("ItemName asc");

            recordsTotal = expiry.Count();
            var data = expiry.ToList();

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
