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
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ItemController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ItemController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [QkAuthorize(Roles = "Dev,Create Item")]
        public ActionResult mergeitem()
        {
            ViewBag.itemn = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                           }, "Value", "Text", 1);
            mergeitem vmodel = new mergeitem();
            return View(vmodel);
        }
        //-->GET -- Item Serial No. Report

        [QkAuthorize(Roles = "Dev,Create Item")]
        public ActionResult createmergeitem(mergeitem vmodel)
        {
            long pri = vmodel.pritemid;
            long sec = vmodel.secitemid;
            var itempri = db.Items.Find(pri);
            var itemsec = db.Items.Find(sec);
            if (itempri.PurchasePrice != itemsec.PurchasePrice || itempri.SellingPrice != itemsec.SellingPrice)
            {
                Danger("Failed,Purchase Price Or Sales Price Different these two item.", true);
                return RedirectToAction("mergeitem");
            }
            db.SEItemss.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.PEItemss.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.SRItemss.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.PRItemss.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.StockTransferItems.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.StockAdjustments.Where(o => o.ItemID == sec).ToList().ForEach(o => o.ItemID = pri);
            db.SaveChanges();
            db.QuotationItems.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.DvItems.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.PFItemss.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.SalesOrderItems.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.PurchaseOrderItems.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();

            db.PurchaseQuotationItems.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.MaterialRequisitionItems.Where(o => o.Item == sec).ToList().ForEach(o => o.Item = pri);
            db.SaveChanges();
            db.AssetTransferDetails.Where(o => o.RefItemId == sec).ToList().ForEach(o => o.RefItemId = pri);
            db.SaveChanges();
            db.AssetToInventoryDetails.Where(o => o.RefItemId == sec).ToList().ForEach(o => o.RefItemId = pri);
            db.SaveChanges();


            if (itempri.ConFactor != 1)
            {

            }


            db.Items.Where(o => o.ItemID == sec).ToList().ForEach(o => o.Status = Status.inactive);
            db.SaveChanges();


            Success("Success", true);


            return RedirectToAction("mergeitem");
        }
        public ActionResult SerialNoReport()
        {
            ViewBag.DropDowns = QkSelect.List(
                            new List<SelectListItem>
                            {
                             new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);


            ViewBag.AgeType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "30", Value="1"},
                new SelectListItem() {Text = "60", Value="2"},
                new SelectListItem() {Text = "90", Value="3"},
            }, "Value", "Text");

            return View();
        }
        public ActionResult pricingstrategy()
        {
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Strategy ON", Value = "1"},
                                    new SelectListItem {  Text = "Strategy Off", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.strategy = OptAll;
            var OptAll3 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Item = OptAll3;

            return View();
        }
        [HttpPost]
        public JsonResult getpricingstrategy(long? strategy, long? item)
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
            bool strat = false;
            if (strategy == 1)
            {
                strat = true;
            }
            var data = (from a in db.Items
                        where (a.PricingStrategy == strat &&
                        (item == null || item == 0 || item == a.ItemID)
                        )
                        select new
                        {
                            a.ItemID,
                            a.ItemCode,
                            a.ItemName,
                            method = a.PricingStrategyType == pricingstatagytype.LIFO ? "Last Purchase" : a.PricingStrategyType == pricingstatagytype.AVG ? "Avg" : "First Purchase",
                            type = a.PricingStrategyAmountType == AmountType.AbsoluteAmount ? "Absolute Amount" : "Percentage",
                            Value = a.PricingStrategyValue
                        }).ToList();
            recordsTotal = data.Count();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });



        }
        public JsonResult GetAllSerialNos(string BatchNo, long? Item, long Status, long? Brand, long? Category, string AsOnDate, int? AgeDays, int ddlType)
        {
            int recordsTotal = 0;

            DateTime? tdate = null;
            DateTime? tDateAge = null;

            if (ddlType == 0)
            {
                if (AsOnDate != "")
                {
                    tdate = DateTime.Parse(AsOnDate, new CultureInfo("en-GB"));
                }
            }
            else
            {
                if (AgeDays != null)
                {
                    AgeDays = (AgeDays * 30);
                    string agedate = DateTime.Now.AddDays(Convert.ToDouble(AgeDays)).ToString("dd-MM-yyyy");
                    tDateAge = DateTime.Parse(agedate, new CultureInfo("en-GB"));

                }
            }
            //=====>> Allocated List
            var v1 = (from a in db.BatchStocks
                      join b in db.Items on a.Item equals b.ItemID
                      join c in db.ItemCategorys on b.ItemCategoryID equals c.ItemCategoryID into cat
                      from c in cat.DefaultIfEmpty()
                      join d in db.ItemBrands on b.ItemBrandID equals d.ItemBrandID into brand
                      from d in brand.DefaultIfEmpty()
                      join e in db.SalesEntrys on a.Reference equals e.SalesEntryId
                      join f in db.Customers on e.Customer equals f.CustomerID
                      where (BatchNo == null || BatchNo == "" || a.BatchNo == BatchNo) &&
                            (Item == null || Item == 0 || a.Item == Item) &&
                            (Category == null || Category == 0 || c.ItemCategoryID == Category) &&
                            (Brand == null || Brand == 0 || d.ItemBrandID == Brand) &&
                            (tdate == null || EF.Functions.DateDiffDay(a.EXP, tdate) >= 0) &&
                            (tDateAge == null || EF.Functions.DateDiffDay(a.EXP, tDateAge) >= 0) &&
                            (a.StockOut > 0 && a.Type == "Sales")
                      select new
                      {
                          a.ID,
                          a.BatchNo,
                          a.Item,
                          a.MFG,
                          a.EXP,
                          b.ItemName,
                          SalesBillNo = e.BillNo,
                          CustomerName = f.CustomerName,
                          Status = "Allocated",
                          StockMov = a.StockOut,
                          VendorName = "",
                          PurchaseDate = a.Date,
                          PurchaseBillNo = ""
                      }).ToList();

            //=====>> Non Allocated List
            var v2 = (from a in db.BatchStocks
                      join d in db.Items on a.Item equals d.ItemID
                      join b in db.ItemCategorys on d.ItemCategoryID equals b.ItemCategoryID into cat
                      from b in cat.DefaultIfEmpty()
                      join c in db.ItemBrands on d.ItemBrandID equals c.ItemBrandID into brand
                      from c in brand.DefaultIfEmpty()
                      join e in db.PurchaseEntrys on a.Reference equals e.PurchaseEntryId
                      join f in db.Suppliers on e.Supplier equals f.SupplierID
                      where (BatchNo == null || BatchNo == "" || a.BatchNo == BatchNo) &&
                            (Item == null || Item == 0 || a.Item == Item) &&
                            (Category == null || Category == 0 || b.ItemCategoryID == Category) &&
                            (Brand == null || Brand == 0 || c.ItemBrandID == Brand) &&
                            (tdate == null || EF.Functions.DateDiffDay(a.EXP, tdate) >= 0) &&
                            (tDateAge == null || EF.Functions.DateDiffDay(a.EXP, tDateAge) >= 0) &&
                            (a.StockIn > 0 && a.Type == "Purchase")
                      select new
                      {
                          a.ID,
                          a.BatchNo,
                          a.Item,
                          a.MFG,
                          a.EXP,
                          d.ItemName,
                          SalesBillNo = "",
                          CustomerName = "",
                          Status = "Non Allocated",
                          StockMov = a.StockIn,
                          VendorName = f.SupplierName,
                          PurchaseDate = e.PEDate,
                          PurchaseBillNo = e.BillNo
                      }).ToList();

            //If BatchNo is used in Sales, Then Removing From Non Allocated List v1
            foreach (var i in v1)
            {
                var Index = v2.FindIndex(a => a.BatchNo == i.BatchNo && a.Item == i.Item);

                if (Index >= 0)
                    v2.RemoveAt(Index);
            }

            //=====>> Allocated List
            if (Status == 1)
            {
                var DataList1 = v1.OrderBy(y => y.ItemName);
                recordsTotal = DataList1.Count();

                var data = DataList1.ToList();

                return Json(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            //=====>> Non Allocated List
            else if (Status == 2)
            {
                var DataList2 = v2.OrderBy(y => y.ItemName);
                recordsTotal = DataList2.Count();

                var data = DataList2.ToList();

                return Json(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            //=====>> Both List
            else
            {
                var DataList3 = v1.Union(v2).OrderBy(y => y.ItemName).ToList();
                recordsTotal = DataList3.Count();

                var data = DataList3.ToList();

                return Json(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
        }

        //-->GET -- Item Serial No. Expiry Report
        public ActionResult SerialNoExpiryReport()
        {

            ViewBag.DropDowns = QkSelect.List(
                            new List<SelectListItem>
                            {
                             new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            ViewBag.AgeType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "30", Value="1"},
                new SelectListItem() {Text = "60", Value="2"},
                new SelectListItem() {Text = "90", Value="3"},
            }, "Value", "Text");

            return View();
        }

        public JsonResult GetAllSerialNosExpiry(string BatchNo, long? Item, string AsOnDate, int? AgeDays, int ddlType)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? tdate = null;
            DateTime? tDateAge = null;

            if (ddlType == 0)
            {
                if (AsOnDate != "")
                {
                    tdate = DateTime.Parse(AsOnDate, new CultureInfo("en-GB"));
                }
            }
            else
            {
                if (AgeDays != null)
                {
                    AgeDays = (AgeDays * 30);
                    string agedate = DateTime.Now.AddDays(Convert.ToDouble(AgeDays)).ToString("dd-MM-yyyy");
                    tDateAge = DateTime.Parse(agedate, new CultureInfo("en-GB"));

                }
            }
            //=====>> Allocated List
            var v1 = (from a in db.BatchStocks
                      join b in db.Items on a.Item equals b.ItemID
                      join c in db.ItemCategorys on b.ItemCategoryID equals c.ItemCategoryID into cat
                      from c in cat.DefaultIfEmpty()
                      join d in db.ItemBrands on b.ItemBrandID equals d.ItemBrandID into brand
                      from d in brand.DefaultIfEmpty()
                      join e in db.SalesEntrys on a.Reference equals e.SalesEntryId
                      join f in db.Customers on e.Customer equals f.CustomerID
                      where (BatchNo == null || BatchNo == "" || a.BatchNo == BatchNo) &&
                           (Item == null || Item == 0 || a.Item == Item) &&
                           (tdate == null || EF.Functions.DateDiffDay(a.EXP, tdate) >= 0) &&
                           (tDateAge == null || EF.Functions.DateDiffDay(a.EXP, tDateAge) >= 0) &&
                           (a.StockOut > 0 && a.Type == "Sales")
                      select new
                      {
                          a.ID,
                          a.BatchNo,
                          a.Item,
                          a.MFG,
                          a.EXP,
                          b.ItemName,
                          BillNo = e.BillNo,
                          CustomerName = f.CustomerName,
                          Status = "Allocated",
                          StockMov = a.StockOut
                      }).ToList();

            //=====>> Non Allocated List
            var v2 = (from a in db.BatchStocks
                      join d in db.Items on a.Item equals d.ItemID
                      join b in db.ItemCategorys on d.ItemCategoryID equals b.ItemCategoryID into cat
                      from b in cat.DefaultIfEmpty()
                      join c in db.ItemBrands on d.ItemBrandID equals c.ItemBrandID into brand
                      from c in brand.DefaultIfEmpty()
                      where (BatchNo == null || BatchNo == "" || a.BatchNo == BatchNo) &&
                            (Item == null || Item == 0 || a.Item == Item) &&
                            (tdate == null || EF.Functions.DateDiffDay(a.EXP, tdate) >= 0) &&
                            (tDateAge == null || EF.Functions.DateDiffDay(a.EXP, tDateAge) >= 0) &&
                           a.StockIn > 0
                      select new
                      {
                          a.ID,
                          a.BatchNo,
                          a.Item,
                          a.MFG,
                          a.EXP,
                          d.ItemName,
                          BillNo = "",
                          CustomerName = "",
                          Status = "Non Allocated",
                          StockMov = a.StockIn
                      }).ToList();

            //If BatchNo is used in Sales, Then Removing From Non Allocated List v1
            foreach (var i in v1)
            {
                var Index = v2.FindIndex(a => a.BatchNo == i.BatchNo && a.Item == i.Item);

                if (Index >= 0)
                    v2.RemoveAt(Index);
            }

            //=====>> Both List           
            var DataList3 = v1.Union(v2).OrderBy(y => y.ItemName).ToList();

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                DataList3 = DataList3.Where(p => p.BatchNo.ToString().ToLower().Contains(search.ToLower())
                               ).ToList();
            }

            recordsTotal = DataList3.Count();
            var data = DataList3.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public ActionResult MCItemMinimumStock()
        {
            var OptAll = QkSelect.List(
                        new List<SelectListItem>
                        {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                        }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;
            var OptAll2 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Category = OptAll2;
            var OptAll3 = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Item = OptAll3;
            List<SelectListItem> SelectPeriod = new List<SelectListItem>() {
                    new SelectListItem {
                          Text = "3 Month", Value = "3"
                                        },
                    new SelectListItem {
                        Text = "2 Month", Value = "2"
                                   },
                    new SelectListItem {

                         Text = "1 Month", Value = "1"
                          },
                    new SelectListItem {

                         Text = "1 Year", Value = "12"
                          },

            };
            ViewBag.Period = SelectPeriod;
            //    Id = s.MCId,
            //    Name = s.MCName

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");

            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");

            }
            return View();
        }
        public ActionResult EditMCItemMinStock(long? tempid, long? mc)
        {
            if (tempid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            McItemMinStocksViewModel Vehicle = new McItemMinStocksViewModel();
            mcitemminstocks itms = db.mcitemminstock.Where(a => a.ItemId == tempid && a.MCId == mc).FirstOrDefault();
            if (itms != null)
            {
                Vehicle.ItemId = itms.ItemId;
                Vehicle.MCId = itms.MCId;
                Vehicle.minstock = itms.minstock;
            }
            else
            {
                Vehicle.ItemId = tempid;
                Vehicle.MCId = mc;
            }
            if (Vehicle == null)
            {
                return NotFound();
            }

            var stands = db.VehicleTypes
                       .Select(s => new
                       {
                           VTypeId = s.VTypeId,
                           Type = s.Type
                       })
                        .ToList();
            ViewBag.VehicleType = QkSelect.List(stands, "VTypeId", "Type");

            return PartialView(Vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditMCItemMinStock([Bind("ItemId,MCId,minstock")] McItemMinStocksViewModel Vehicle)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                mcitemminstocks Exists = db.mcitemminstock.Where(c => c.ItemId == Vehicle.ItemId && c.MCId == Vehicle.MCId).FirstOrDefault();
                if (Exists != null)
                {
                    Exists.minstock = Convert.ToDecimal(Vehicle.minstock);
                    db.Entry(Exists).State = EntityState.Modified;
                    db.SaveChanges();
                    msg = "Successfully updated.";
                    stat = true;
                }
                else
                {
                    var pro = new mcitemminstocks
                    {
                        ItemId = Convert.ToInt64(Vehicle.ItemId),
                        minstock = Convert.ToDecimal(Vehicle.minstock),
                        MCId = Convert.ToInt64(Vehicle.MCId),
                    };
                    db.mcitemminstock.Add(pro);
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    msg = "Successfully updated.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [HttpPost]
        public JsonResult GetMCItemMinimumStock(long? ddmc)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int recordsTotal = 0;
            long mc = Convert.ToInt64(ddmc);
            var v = (from a in db.Items
                     join e in db.mcitemminstock on new { f1 = a.ItemID, f2 = mc } equals new { f1 = e.ItemId, f2 = e.MCId } into temp3
                     from e in temp3.DefaultIfEmpty()
                     where a.KeepStock == true

                     select new
                     {
                         id = a.ItemID,//e.mcitemminstock,
                         item = a.ItemCode + "-" + a.ItemName,
                         minstock = (e == null) ? 0 : e.minstock,// db.mcitemminstock.Where(aa => aa.ItemId == a.ItemID && aa.MCId == ddmc).Select(aa => aa.minstock).DefaultIfEmpty(0),
                         mc = ddmc,
                         // currstock = com.GetItemWisestock(a.ItemID,ddmc),//"<a href='javascript:void(0)' onclick='findstock()'>Show Stock</a>"
                     }).ToList().Select(o => new
                     {
                         o.id,
                         o.item,
                         o.minstock,
                         o.mc,
                         currstock = 0,// com.GetItemWisestock(o.id, ddmc)
                     });

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.item.ToString().ToLower().Contains(search.ToLower()));

            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        // GET: Item 
        [QkAuthorize(Roles = "Dev,Item")]
        public ActionResult Index()
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            ViewBag.JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType").Select(a => a.TypeValue).FirstOrDefault();

            var EnableBulkUpload = db.EnableSettings.Where(a => a.EnableType == "ItemBulkUpload").FirstOrDefault();
            ViewBag.BulkUpload = EnableBulkUpload != null ? EnableBulkUpload.Status : Status.inactive;

            ViewBag.ItemType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value="0"},
                new SelectListItem() {Text = "General", Value="1"},
                new SelectListItem() {Text = "Diamond", Value="2"},
                new SelectListItem() {Text = "Watch", Value="3"},
                new SelectListItem() {Text = "Object", Value="4"},
            }, "Value", "Text");

            ViewBag.KStock = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All"},
                new SelectListItem() {Text = "Yes", Value=true.ToString()},
                new SelectListItem() {Text = "No", Value=false.ToString()},
            }, "Value", "Text");

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All"},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");

            var IndexList = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.ItemCatg = IndexList;
            ViewBag.ItmBrand = IndexList;
            ViewBag.PUnit = IndexList;
            ViewBag.ItemSrch = IndexList;

            ViewBag.Supp = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            var Tax = db.Taxs
                   .Select(s => new
                   {
                       Id = s.TaxID,
                       TaxName = s.TaxName
                   }).ToList();
            ViewBag.ItemTax = QkSelect.List(Tax, "Id", "TaxName");

            var Size = db.ItemSizes
                   .Select(s => new
                   {
                       Id = s.ItemSizeID,
                       SizeName = s.ItemSizeName
                   }).ToList();
            ViewBag.ItemSize = QkSelect.List(Size, "Id", "SizeName");

            var color = db.ItemColors
                             .Select(s => new
                             {
                                 ID = s.ItemColorID,
                                 Name = s.ItemColorName,
                             })
                             .ToList();
            ViewBag.ItemColor = QkSelect.List(color, "ID", "Name");


            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Item")]
        public ActionResult GetItem(long? Item, int? ItemType, string PartNo, long? PUnit, long? Brand, long? Category, long? ItemSize,
        long? Color, long? Tax, long? ddlSupplier, string SuppRef, string Stats, bool? KpStock, string appstat)
        {

            var kst = (KpStock == null) ? null : (KpStock);

            Status st = new Status();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };
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

            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            var UserId = User.Identity.GetUserId();
            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var serverQuery = (from a in db.Items
                     join b in db.ItemCategorys on a.ItemCategoryID equals b.ItemCategoryID into cat
                     from b in cat.DefaultIfEmpty()
                     join c in db.ItemBrands on a.ItemBrandID equals c.ItemBrandID into brand
                     from c in brand.DefaultIfEmpty()
                     join d in db.ItemColors on a.ItemColorID equals d.ItemColorID into color
                     from d in color.DefaultIfEmpty()
                     join e in db.Taxs on a.TaxID equals e.TaxID into itax
                     from e in itax.DefaultIfEmpty()
                     join f in db.ItemSizes on a.ItemSizeID equals f.ItemSizeID into isize
                     from f in isize.DefaultIfEmpty()
                     join g in db.ItemUnits on a.ItemUnitID equals g.ItemUnitID into punit
                     from g in punit.DefaultIfEmpty()
                     join h in db.ItemUnits on a.SubUnitId equals h.ItemUnitID into sunit
                     from h in sunit.DefaultIfEmpty()
                     join i in db.Suppliers on a.Supplier equals i.SupplierID into scat
                     from i in scat.DefaultIfEmpty()
                     join j in db.ItemBundles on a.ItemID equals j.mainItem into bundle
                     from j in bundle.DefaultIfEmpty()
                     join k in db.Scaffolds on a.ItemID equals k.Item into scaf
                     from k in scaf.DefaultIfEmpty()

                     // EF Core 10 cannot translate the nested-collection projections (app / AppStatus /
                     // chkAppStatus) inside the executed select. Split SERVER from CLIENT: materialize only
                     // entity columns + simple scalars into serverRows, then build client lookups keyed by
                     // ItemID and re-project client-side with the SAME member names + order.
                     where (Item == null || Item == 0 || a.ItemID == Item) && j.ItemBundleId == null &&
                           (ItemType == null || ItemType == 0 || a.ItemType == ItemType) &&
                           (PartNo == null || PartNo == "" || a.PartNumber == PartNo) &&
                           (PUnit == null || PUnit == 0 || g.ItemUnitID == PUnit) && (Category == null || Category == 0 || b.ItemCategoryID == Category) &&
                           (Brand == null || Brand == 0 || c.ItemBrandID == Brand) && (ItemSize == null || ItemSize == 0 || f.ItemSizeID == ItemSize) &&
                           (KpStock == null || a.KeepStock == kst) && (ddlSupplier == null || ddlSupplier == 0 || a.Supplier == ddlSupplier) &&
                           (SuppRef == null || SuppRef == "" || a.SupplierRef == SuppRef) && (Tax == null || Tax == 0 || e.TaxID == Tax) &&
                           (Color == null || Color == 0 || d.ItemColorID == Color) && (Stats == null || Stats == "" || a.Status == st)
                    
                     select new
                     {
                         a.ItemID,
                         a.ItemCode,
                         a.ItemName,
                         a.ItemArabic,
                         a.ItemType,
                         a.Barcode,
                         a.ItemDescription,
                         a.SellingPrice,
                         a.PurchasePrice,
                         a.MRP,
                         a.BasePrice,
                         a.Status,
                         a.KeepStock,
                         a.slreq,
                         a.ItemUnitID,
                         a.PricingStrategy,
                       //  l.FileName,
                         ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                         PartNumber = a.PartNumber != "" ? a.PartNumber : "",
                         Supplier = i.SupplierName,
                         SupplierRef = a.SupplierRef,
                         Category = b.ItemCategoryName,
                         Brand = c.ItemBrandName,
                         Color = d.ItemColorName,
                         Tax = e.TaxName,
                         Size = f.ItemSizeName,
                         PUnit = g.ItemUnitName,
                         SUnit = h.ItemUnitName,
                         a.OpeningStock,
                         k.Weight,
                         k.CBM,
                     });

            // Performance (audit P2): server-side paging on the common path (no search, plain-column sort);
            // search/computed sorts fall back to the original materialize-all behaviour unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "Barcode","BasePrice","Brand","Category","CBM","Color","ConFactor","ItemArabic","ItemCode","ItemDescription","ItemID","ItemName","ItemType","ItemUnitID","KeepStock","MRP","OpeningStock","PartNumber","PricingStrategy","PUnit","PurchasePrice","SellingPrice","Size","slreq","Status","SUnit","Supplier","SupplierRef","Tax","Weight" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0
                && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn)
                    ? serverQuery.OrderBy("ItemID asc")
                    : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by ItemID (missing key -> empty list, no KeyNotFound).
            var itemIds = serverRows.Select(o => o.ItemID).ToList();
            // app = approver EmployeeIds for the item (nested collection, keyed by TransEntry == ItemID).
            var appLookup = db.Approvals
                .Where(x => x.Type == "ItemEntry" && itemIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.EmployeeId })
                .ToList()
                .ToLookup(x => x.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(x => x.Type == "ItemEntry" && itemIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.ApprovalStatus, x.ApprovedBy, x.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(x => x.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per item.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(x => x.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(x => x.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.ItemID].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.ItemID].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.ItemID, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.ItemID,
                         o.ItemCode,
                         o.ItemName,
                         o.ItemArabic,
                         o.ItemType,
                         o.Barcode,
                         o.ItemDescription,
                         o.SellingPrice,
                         o.PurchasePrice,
                         o.MRP,
                         o.BasePrice,
                         o.Status,
                         o.KeepStock,
                         o.slreq,
                         o.ItemUnitID,
                         o.PricingStrategy,
                         o.ConFactor,
                         o.PartNumber,
                       //  o.FileName,
                         o.Supplier,
                         o.SupplierRef,
                         o.Category,
                         o.Brand,
                         o.Color,
                         o.Tax,
                         o.Size,
                         o.PUnit,
                         o.SUnit,
                         OpeningStock=(ddlSupplier==0||ddlSupplier==null) ?o.OpeningStock:com.GetItemWisestock(o.ItemID,0),
                         o.Weight,
                         o.CBM,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null && empl.EmployeeId != 0) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,


                     };
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ItemCode.ToString().ToLower().Contains(search.ToLower()) ||
                                   p.ItemName.ToString().ToLower().Contains(search.ToLower())||
                                         (p.Barcode==null||p.Barcode.ToString().ToLower().Contains(search.ToLower()))
                                   );
            }

            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            else
            {
                v = v.OrderByDescending(c => c.ItemID);
            }
            if (!fastPage) { recordsTotal = v.Count(); }
            var data = fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList();
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
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "ItemEntry" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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


            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "ItemEntry").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = UserId;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "ItemEntry";

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
        //Get: View of Adding Remarks
        public ActionResult AddItemRemark(long? id)
        {
            long RemarkId = 0;
            string Remark = "";
            var UserId = User.Identity.GetUserId();
            var UpdDate = Convert.ToDateTime(System.DateTime.Now);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Item Item = db.Items.Find(id);

            if (Item == null)
            {
                return NotFound();
            }

            //Finding the remark details using ItemId
            var ItmRemarks = db.ItemRemarks.Where(a => a.ItemId == id).FirstOrDefault();

            if (ItmRemarks != null)
            {
                RemarkId = ItmRemarks.ItemRemarkId;
                Remark = ItmRemarks.Remark;
                UserId = ItmRemarks.LastUpdatedBy;
                UpdDate = ItmRemarks.LastUpdatedDate;
            }

            var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).FirstOrDefault();

            //To Retrieve data from 'ItemRemarks' table while loading
            var ItemRemark = new ItemRemark
            {
                ItemId = Item.ItemID,
                ItemRemarkId = RemarkId,
                Remark = Remark,
                LastUpdatedBy = UserName,
                LastUpdatedDate = UpdDate
            };

            return PartialView(ItemRemark);
        }

        //Saving of Remarks
        [HttpPost]

        public ActionResult AddItemRemark(ItemRemark ItmRemark)
        {
            Int64 ItemId = ItmRemark.ItemId;

            if (ModelState.IsValid)
            {
                Common com = new Common();
                var UserId = User.Identity.GetUserId();
                var Today = Convert.ToDateTime(System.DateTime.Now);

                //Add Remark If Not Exists
                if (ItmRemark.ItemRemarkId == 0)
                {
                    ItemRemark Obj = new ItemRemark
                    {
                        ItemId = ItmRemark.ItemId,
                        Remark = ItmRemark.Remark,
                        LastUpdatedBy = UserId,
                        LastUpdatedDate = Today,
                    };
                    db.ItemRemarks.Add(Obj);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "Item", "ItemRemarks", findip(), ItemId, "Remarks Added Successfully..");
                    Success("Remark added successfully...", true);
                }
                else
                {
                    ItemRemark Obj = db.ItemRemarks.Find(ItmRemark.ItemRemarkId);

                    Obj.Remark = ItmRemark.Remark;
                    Obj.LastUpdatedDate = Today;
                    Obj.LastUpdatedBy = UserId;

                    db.Entry(Obj).State = EntityState.Modified;
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "Item", "ItemRemarks", findip(), ItemId, "Remarks Added Successfully..");
                    Success("Remark updated successfully...", true);
                }
            }
            else
            {
                Danger("Failed to add Remarks...", true);
            }
            return RedirectToAction("Index", "Item");
        }
        public JsonResult BarcodeRead(string q)
        {
            //IL_4faa: Unknown result type (might be due to invalid IL or missing references)
            //IL_4faf: Unknown result type (might be due to invalid IL or missing references)
            //IL_4fbe: Expected O, but got Unknown
            string message = "";
            bool status = false;
            object item = new object();
            if (!string.IsNullOrEmpty(q) || !string.IsNullOrEmpty(q))
            {
                var source = (from p in (IQueryable<Item>)db.Items
                              where (int)p.Status == 0 && p.Barcode == q
                              select p into b
                              select new
                              {
                                  text = string.Concat(b.ItemCode + "-", b.ItemName),
                                  id = b.ItemID,
                                  type = "Direct"
                              } into b
                              orderby b.text
                              select b).ToList();
                if (source.Any())
                {
                    item = source.FirstOrDefault();
                    status = true;
                }
                else
                {
                    EnableSetting enableSetting = ((IQueryable<EnableSetting>)db.EnableSettings).Where((EnableSetting a) => a.EnableType == "WeighingMachineCode").FirstOrDefault();
                    if ((enableSetting?.Status ?? Status.inactive) == Status.active)
                    {
                        string text = ((enableSetting != null) ? enableSetting.TypeValue : "");
                        if (text.Length == q.Length)
                        {
                            string text2 = "";
                            string text3 = "";
                            string text4 = "";
                            string text5 = "";
                            string text6 = "";
                            int num = 0;
                            string text7 = "";
                            string text8 = "";
                            string text9 = text;
                            for (int i = 0; i < text9.Length; i++)
                            {
                                char c2 = text9[i];
                                if (c2.ToString() == "I")
                                {
                                    text5 += q[num];
                                }
                                else if (c2.ToString() == "W")
                                {
                                    text3 += q[num];
                                }
                                else if (c2.ToString() == "V")
                                {
                                    text2 += q[num];
                                }
                                else if (c2.ToString() == "Y")
                                {
                                    text4 += q[num];
                                }
                                else if (c2.ToString() == "P")
                                {
                                    text6 += q[num];
                                }
                                else if (c2.ToString() == "G")
                                {
                                    text8 += q[num];
                                }
                                else if (c2.ToString() == "U")
                                {
                                    text7 += q[num];
                                }
                                num++;
                            }
                            string value = "";
                            if (text6 != "")
                            {
                                int length = text6.Length;
                                value = text6.Substring(0, length - 2) + "." + text6.Substring(length - 2);
                            }
                            string value2 = "";
                            if (text3 != "" || text2 != "")
                            {
                                switch (text7)
                                {
                                    case "1":
                                        value2 = ((text3 != "") ? text3 : "0") + "." + ((text2 != "") ? text2 : "0");
                                        break;
                                    case "2":
                                    case "3":
                                        value2 = ((text3 != "") ? text3 : "0") + ((text2 != "") ? text2 : "0") + ((text4 != "") ? text4 : "0");
                                        break;
                                }
                            }
                            int num2 = Convert.ToInt32(text5);
                            string ItemCodeS = Convert.ToString(num2);
                            decimal num3 = default(decimal);
                            try
                            {
                                num3 = Convert.ToDecimal(value2);
                            }
                            catch (FormatException)
                            {
                                num3 = default(decimal);
                            }
                            decimal num4 = default(decimal);
                            try
                            {
                                num4 = Convert.ToDecimal(value);
                            }
                            catch (FormatException)
                            {
                                num4 = default(decimal);
                            }
                            Item item2 = ((IQueryable<Item>)db.Items).Where((Item a) => a.ItemCode == ItemCodeS).FirstOrDefault();
                            if (item2 != null)
                            {
                                decimal unitPrice = default(decimal);
                                decimal UnitWeight = default(decimal);
                                decimal totPrice = default(decimal);
                                if (num3 != 0m)
                                {
                                    UnitWeight = num3;
                                    if (num4 != 0m)
                                    {
                                        totPrice = num4;
                                        unitPrice = totPrice / num3;
                                    }
                                    else
                                    {
                                        unitPrice = item2.SellingPrice;
                                        totPrice = unitPrice * num3;
                                    }
                                }
                                else if (num4 != 0m)
                                {
                                    totPrice = num4;
                                    unitPrice = item2.SellingPrice;
                                    UnitWeight = totPrice / unitPrice;
                                }
                                else
                                {
                                    totPrice = default(decimal);
                                    unitPrice = item2.SellingPrice;
                                    UnitWeight = default(decimal);
                                }
                                DateTime today = Convert.ToDateTime(DateTime.Now);
                                item = (from b in (IQueryable<Item>)db.Items
                                        join c in (IEnumerable<ItemUnit>)db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                        from c in primary.DefaultIfEmpty()
                                        join d in (IEnumerable<ItemUnit>)db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                        from d in second.DefaultIfEmpty()
                                        join e in (IEnumerable<ItemCategory>)db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                                        from e in cat.DefaultIfEmpty()
                                        join f in (IEnumerable<Tax>)db.Taxs on b.TaxID equals f.TaxID into taxss
                                        from f in taxss.DefaultIfEmpty()
                                        let h = (from cl in (IQueryable<InstantDiscount>)db.InstantDiscounts
                                                 where cl.ItemId == b.ItemID && EF.Functions.DateDiffDay((DateTime?)cl.StartDate, (DateTime?)today) >= (int?)0 && EF.Functions.DateDiffDay((DateTime?)cl.EndDate, (DateTime?)today) <= (int?)0
                                                 orderby cl.InstantDiscountId descending
                                                 select cl).FirstOrDefault()
                                        where b.ItemCode == ItemCodeS
                                        select new
                                        {
                                            text = string.Concat(b.ItemCode + "-", b.ItemName),
                                            id = b.ItemID,
                                            type = "Weighing",
                                            subtotal = totPrice,
                                            price = unitPrice,
                                            Weight = UnitWeight,
                                            Tax = f.Percentage,
                                            ItemCode = b.ItemCode,
                                            ItemName = b.ItemName,
                                            ItemArabic = b.ItemArabic,
                                            ItemWithCode = string.Concat(b.ItemCode + " - ", b.ItemName),
                                            ItemUnitID = b.ItemUnitID,
                                            SubUnitId = b.SubUnitId,
                                            PriUnit = c.ItemUnitName,
                                            SubUnit = d.ItemUnitName,
                                            ConFactor = ((b.ConFactor != 0m) ? b.ConFactor : 1m),
                                            ItemID = b.ItemID,
                                            OpeningStock = b.OpeningStock,
                                            MinStock = b.MinStock,
                                            categoryname = e.ItemCategoryName,
                                            SellingPrice = b.SellingPrice,
                                            PurchasePrice = b.PurchasePrice,
                                            BasePrice = b.BasePrice,
                                            MRP = b.MRP,
                                            KeepStock = b.KeepStock,
                                            offerprice = ((h != null) ? h.OfferPrice : (-1m)),
                                            PriPurchase = (((int?)(int)(from a in (IQueryable<PEItems>)db.PEItemss
                                                                        where a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID
                                                                        select a.ItemQuantity).Sum()) ?? 0),
                                            SubPurchase = (((int?)(int)(from a in (IQueryable<PEItems>)db.PEItemss
                                                                        where a.Item == b.ItemID && a.ItemUnit == b.SubUnitId
                                                                        select a.ItemQuantity).Sum()) ?? 0),
                                            PriSale = (((int?)(int)(from a in (IQueryable<SEItems>)db.SEItemss
                                                                    where a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID
                                                                    select a.ItemQuantity).Sum()) ?? 0),
                                            SubSale = (((int?)(int)(from a in (IQueryable<SEItems>)db.SEItemss
                                                                    where a.Item == b.ItemID && a.ItemUnit == b.SubUnitId
                                                                    select a.ItemQuantity).Sum()) ?? 0),
                                            PriPReturn = (((int?)(int)(from a in (IQueryable<PRItems>)db.PRItemss
                                                                       where a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID
                                                                       select a.ItemQuantity).Sum()) ?? 0),
                                            SubPReturn = (((int?)(int)(from a in (IQueryable<PRItems>)db.PRItemss
                                                                       where a.Item == b.ItemID && a.ItemUnit == b.SubUnitId
                                                                       select a.ItemQuantity).Sum()) ?? 0),
                                            PriSReturn = (((int?)(int)(from a in (IQueryable<SRItems>)db.SRItemss
                                                                       where a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID
                                                                       select a.ItemQuantity).Sum()) ?? 0),
                                            SubSReturn = (((int?)(int)(from a in (IQueryable<SRItems>)db.SRItemss
                                                                       where a.Item == b.ItemID && a.ItemUnit == b.SubUnitId
                                                                       select a.ItemQuantity).Sum()) ?? 0),
                                            btype = b.ItemType
                                        }).FirstOrDefault();
                                message = "Success";
                                status = true;
                            }
                            else
                            {
                                message = "There Is An Error In Item Code";
                                status = false;
                                item = null;
                            }
                        }
                        else
                        {
                            message = "There is an issue in Barcode";
                            status = false;
                            item = null;
                        }
                    }
                    else
                    {
                        message = "Sorry BarCode Not Matching";
                        status = false;
                        item = null;
                    }
                }
            }
            var val = new QuickSoft.Models.LegacyJsonResult();
            val.Data = ((object)new { status, message, item });
            return val;
        }
        [QkAuthorize]
        public JsonResult Searchinactive(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat4> serialisedJson;
            string stt = "All";

            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {


                serialisedJson = db.Items.Where(p => (p.ItemName.ToLower().Contains(q.ToLower()) || p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.Contains(q))
                 && (secnd == "" || p.ItemName.ToLower().Contains(secnd.ToLower()))
                && (third == "" || p.ItemName.ToLower().Contains(third.ToLower()))

                )

                                  //|| p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(q) 
                                  // || p.ItemCode.Contains(q) || p.Barcode.Contains(q)


                                  .Select(b => new SelectFormat4
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID,
                                      pprice = b.PurchasePrice,
                                      sprice = b.SellingPrice,
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.Items.Select(b => new SelectFormat4
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID,
                    pprice = b.PurchasePrice,
                    sprice = b.SellingPrice,
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat4() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }



        public JsonResult SearchitemcodeCheck(string q, string x)
        {
            List<SelectFormatDisabled> serialisedJson;
            string stt = " ";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Items
                                  where (b.ItemCode.ToLower().Contains(q.ToLower()) || b.ItemCode.Contains(q))
                                  select new SelectFormatDisabled
                                  {
                                      text = b.ItemCode, //each json object will have 
                                      id = b.ItemCode,
                                      disabled = "true"
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Items

                                  select new SelectFormatDisabled
                                  {
                                      text = b.ItemCode, //each json object will have 
                                      id = b.ItemCode,
                                      disabled = "true"
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatDisabled() { id = "0", text = stt, disabled = "true" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson.Take(10).ToList());
        }
        public JsonResult SearchitemCheck(string q, string x)
        {
            List<SelectFormatDisabled> serialisedJson;
            string stt = " ";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Items
                                  where (b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q))
                                  select new SelectFormatDisabled
                                  {
                                      text = b.ItemName, //each json object will have 
                                      id = b.ItemCode,
                                      disabled = "true"
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Items

                                  select new SelectFormatDisabled
                                  {
                                      text = b.ItemName, //each json object will have 
                                      id = b.ItemCode,
                                      disabled = "true"
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatDisabled() { id = "0", text = stt, disabled = "true" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson.Take(10).ToList());
        }
        [QkAuthorize(Roles = "Dev,Create Item")]
        public ActionResult Create()
        {
            long selectid = 0;
            var mcminstock = db.EnableSettings.Where(o => o.EnableType == "materialcentrewiseminstock").SingleOrDefault();

            var acc2 = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 12 && a.Group != 14 && a.Group != 23 && a.Status == Status.active).Select(s => new SelectFormat
            {
                id = s.AccountsID,
                text = s.Name
            }).ToList();
            var initial = new SelectFormat() { id = 0, text = "Select Account" };
            acc2.Insert(0, initial);

            ViewBag.SetAccount = QkSelect.List(acc2, "id", "text");

            ViewBag.mcminstockwise = (mcminstock != null) ? mcminstock.Status : Status.inactive;
            //        .Select(s => new
            //            Id = s.ItemCategoryID,
            //            CategoryName = s.ItemCategoryName

            ViewBag.ItemCategory = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "General", Value = "1"},
                           }, "Value", "Text", 1);


            ViewBag.itemn = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                           }, "Value", "Text", 1);
            ViewBag.itemnc = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                       }, "Value", "Text", 1);
            var Brand = db.ItemBrands
                   .Select(s => new
                   {
                       Id = s.ItemBrandID,
                       BrandName = s.ItemBrandName
                   }).ToList();
            ViewBag.ItemBrand = QkSelect.List(Brand, "Id", "BrandName");

            var Tax = db.Taxs
                   .Select(s => new
                   {
                       Id = s.TaxID,
                       TaxName = s.TaxName
                   }).ToList();
            ViewBag.Tax = QkSelect.List(Tax, "Id", "TaxName");

            var Unit = db.ItemUnits
                   .Select(s => new
                   {
                       Id = s.ItemUnitID,
                       UnitName = s.ItemUnitName
                   }).ToList();
            ViewBag.ItemUnit = QkSelect.List(Unit, "Id", "UnitName");

            var Size = db.ItemSizes
                   .Select(s => new
                   {
                       Id = s.ItemSizeID,
                       SizeName = s.ItemSizeName
                   }).ToList();
            ViewBag.ItemSize = QkSelect.List(Unit, "Id", "SizeName");

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var Currency = db.CurrencyMasters
                    .Select(s => new
                    {
                        Id = s.Id,
                        Code = s.CurrencyCode
                    }).ToList();
            ViewBag.NCurrency = QkSelect.List(Currency, "Id", "Code");

            var Suppliers = db.Suppliers
                    .Select(s => new
                    {
                        Id = s.SupplierID,
                        Code = s.SupplierName
                    }).ToList();

            ViewBag.Supp = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "", Value = null},
                             }, "Value", "Text", 1);

            ViewBag.ItemType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "General", Value="1"},
                new SelectListItem() {Text = "Diamond", Value="2"},
                new SelectListItem() {Text = "Watch", Value="3"},
                new SelectListItem() {Text = "Object", Value="4"},
            }, "Value", "Text");

            var color = db.ItemColors
                             .Select(s => new
                             {
                                 ID = s.ItemColorID,
                                 Name = s.ItemColorName,
                             })
                             .ToList();
            ViewBag.Color = QkSelect.List(color, "ID", "Name");
            var Prefix = db.PrefixMasters
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.PrefixCode,
                             })
                             .ToList();
            ViewBag.Prefix = QkSelect.List(Prefix, "ID", "Name");

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            ViewBag.BCEnable = enable != null ? enable.Status : Status.inactive;

            var brCode = (ViewBag.BCEnable == Status.active) ? com.createBarcode().ToString() : "";
            //check enable commision
            var enablecomm = db.EnableSettings.Where(a => a.EnableType == "ItemCommision").FirstOrDefault();
            ViewBag.COMEnable = enablecomm != null ? enablecomm.Status : Status.inactive;


            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            ViewBag.BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;


            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            ViewBag.JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;


            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            ViewBag.EnableCurrency = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;


            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            ViewBag.PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType").Select(a => a.TypeValue).FirstOrDefault();

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            var viewModel = new ItemViewModel
            {
                Barcode = brCode,
                ItemCode = ItemCodes(),
                ItemCategorys = db.ItemCategorys.ToList(),
                ItemBrands = db.ItemBrands.ToList(),
                ItemColors = db.ItemColors.ToList(),
                ItemSizes = db.ItemSizes.ToList(),
                Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList(),
                ItemUnits = db.ItemUnits.ToList(),
                HireType = db.HireTypes.ToList(),
                KeepStock=true,
                TaxID=2
            };


            ViewBag.LastEntry = db.Items.Where(a => !db.ItemBundles.Select(b => b.mainItem).Contains(a.ItemID)).Select(a => a.ItemID).AsEnumerable().DefaultIfEmpty(0).Max();
            //                     //let bn=db.ItemBundles.Select(c=>c.mainItem).ToList()
            //                     //where 
            //                         a.ItemID
            viewModel.ItemUnitID = 5;
            viewModel.SubUnitId = 5;
            companySet();
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", viewModel);
            }
            else
            {
                viewModel.slreq = false;
                viewModel.accmap = false;
                return View(viewModel);
            }
        }



        [HttpPost]
        //   [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Item")]
        public JsonResult Create(ItemViewModel ProdViewModel, string fnval, int? printcount)
        {
            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            ViewBag.EnableCurrency = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;

            if (ViewBag.EnableCurrency == 0)
            {
                ProdViewModel.Currency = ProdViewModel.Currency;
                ProdViewModel.ConRate = ProdViewModel.ConRate;
            }
            else
            {
                ProdViewModel.Currency = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.Id).FirstOrDefault();
                var getrate = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.ConvertionRate).FirstOrDefault();
                ProdViewModel.ConRate = decimal.TryParse(getrate, out var parsedConRate) ? parsedConRate : 0; // guard: non-numeric/blank ConvertionRate string -> FormatException
            }

            bool stat = false;
            string msg;
            var BarCodeExists = ProdViewModel.Barcode != null ? db.Barcodes.Any(u => u.BarcodeNumber == ProdViewModel.Barcode) : false;
            var ItemCodeExists = db.Items.Any(u => u.ItemCode == ProdViewModel.ItemCode);
            var ItemNameExists = db.Items.Any(u => u.ItemName == ProdViewModel.ItemName);

            //    //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            //    //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            if (BarCodeExists)
            {
                msg = "An Item with same Barcode exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    var modelErrors = new List<string>();

                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var modelError in modelState.Errors)
                        {
                            modelErrors.Add(modelError.ErrorMessage);
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    var itID = com.Item(ProdViewModel);
                    var CurrntDate = Convert.ToDateTime(System.DateTime.Now);

                    if (ProdViewModel.ItemImage.ToList().First() != null)
                    {
                        var itimage = com.Images(ProdViewModel, itID);
                    }
                    if (ProdViewModel.ItemDocument.ToList().First() != null)
                    {
                        var itdoc = com.Document(ProdViewModel, itID);
                    }
                    if (ProdViewModel.HireTypes != null)
                    {
                        foreach (HireTypeViewModel Hire in ProdViewModel.HireTypes)
                        {
                            var rate = new HireRate
                            {
                                type = Hire.type,
                                Rate = Hire.Rate,
                                ItemId = itID
                            };
                            db.HireRates.Add(rate);
                            db.SaveChanges();
                        }
                    }

                    /************************************For Additional Images ***********************************/

                    if (ProdViewModel.LstAdditionalImages != null && ProdViewModel.LstAdditionalImages.Count > 0)
                    {
                        var fileName = "";
                        var FStatus = Status.active;
                        int i = 1;

                        foreach (var item in ProdViewModel.LstAdditionalImages)
                        {
                            if (item.FileName != "" && item.FileName != null)
                            {
                                // Files upload
                                IFormFile file = Request.Form.Files[i];//i is starting from 1, because 0 is ItemImage file

                                fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);

                                var uploadUrl = LegacyWeb.MapPath("~/uploads/itemimages/AdditionalImages/" + itID);

                                if (!Directory.Exists(uploadUrl))
                                    Directory.CreateDirectory(uploadUrl);

                                file.SaveAs(Path.Combine(uploadUrl, fileName));

                                AttachmentDocuments ImageDocuments = new AttachmentDocuments
                                {
                                    TransactionID = itID,
                                    TransactionType = "Item",
                                    FileName = fileName,
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };

                                db.AttachmentDocuments.Add(ImageDocuments);
                                db.SaveChanges();
                            }

                            i++;
                        }
                    }
                    /***********************************************************************/
                    Approval approval = new Approval();
                    var Approve = db.AssignedTos.Where(o => o.CustomerID == -1).Select(o => o.EmployeeId).ToArray();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = itID;
                        approval.Type = "ItemEntry";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                    var UserId = User.Identity.GetUserId();

                    /************************* ItemTransaction ************************/
                    //add Quantity as 0 to ItemTransaction Table(in MainCenter)
                    com.AddItemTransaction(itID, 1, 0, UserId, CurrntDate);
                    /******************************************************************/
                    com.addlog(LogTypes.Created, UserId, "Item", "Items", findip(), itID, "Item Added Successfully");

                    if (fnval == "print")
                    {
                        var item = db.Items.Where(n => n.ItemID == itID).Select(b => new
                        {
                            b.ItemName,
                            b.Barcode,
                            b.MRP,
                            ItemPrice = b.SellingPrice,
                            PCount = printcount
                        }).FirstOrDefault();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item } };
                    }
                    else
                    {
                        var Category = db.ItemCategorys.Where(p => p.Parent == 1 || p.ItemCategoryID == 1)
                                         .Select(s => new
                                         {
                                             Id = s.ItemCategoryID,
                                             CategoryName = s.ItemCategoryName
                                         }).ToList();
                        ViewBag.ItemCategory = QkSelect.List(Category, "Id", "CategoryName");

                        msg = "Successfully added Item details.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                    var viewModel = new ItemViewModel
                    {
                        ItemCategorys = db.ItemCategorys.ToList(),
                        ItemBrands = db.ItemBrands.ToList(),
                        ItemColors = db.ItemColors.ToList(),
                        ItemSizes = db.ItemSizes.ToList(),
                        Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList(),
                        ItemUnits = db.ItemUnits.ToList()

                    };
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

            }

        }
        public ActionResult AllItemsearch(string search)
        {
          
            var cn = db.Items.Count();
            string[] searchkey = search.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            string q = search;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }






            var item = db.Items.Where(b => (b.Status == Status.active)
               && (b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.Contains(q))
                && (secnd == "" || b.ItemName.ToLower().Contains(secnd.ToLower()))
               && (third == "" || b.ItemName.ToLower().Contains(third.ToLower()))).Select(b => new
               {
                   ItemID = b.ItemID,
                   b.ItemBrandID,
                   Brand = b.ItemBrands.ItemBrandName,
                   b.ItemCategoryID,
                   Category = b.ItemCategorys.ItemCategoryName,
                   b.ItemUnitID,
                   b.SubUnitId,
                   b.ConFactor,
                   Unit = b.ItemUnit.ItemUnitName,
                   b.TaxID,
                   Tax = db.Taxs.Where(a => a.TaxID == b.TaxID).Select(a => a.Percentage).FirstOrDefault(),
                   b.ItemColorID,
                   Color = b.ItemColors.ItemColorName,
                   b.ItemSizeID,
                   size = b.ItemSizes.ItemSizeName,

                   SellingPrice =  ((b.SellingPrice / (1 + (db.Taxs.Where(a => a.TaxID == b.TaxID).Select(a => a.Percentage).FirstOrDefault()) / 100))) ,
                   b.BasePrice,
                   b.PurchasePrice,
                   b.ItemName,
                   b.ItemArabic,
                   b.ItemCode,
                   b.ItemDescription,
                   b.MRP,
                   b.Barcode,
                   b.CreatedBy,
                   b.CreatedUser,
                   b.CreatedUserID,
                   SubUnit = db.ItemUnits.Where(o => o.ItemUnitID == b.SubUnitId).Select(o => o.ItemUnitName).FirstOrDefault(),

                   PriUnit = db.ItemUnits.Where(o => o.ItemUnitID == b.ItemUnitID).Select(o => o.ItemUnitName).FirstOrDefault(),
                   ImageFile = db.ItemImages.Where(a => a.ItemID == b.ItemID).Select(a => a.FileName).FirstOrDefault(),
               }).ToList().Take(20).OrderBy(c => c.ItemCategoryID);
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(item);
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;


        }


        [HttpGet]
        public JsonResult removeprimaryimage(long itemid)
        {

            var ItemImg = db.ItemImages.Where(a => a.ItemID == itemid).FirstOrDefault();

            if (ItemImg != null)
            {
                db.ItemImages.RemoveRange(db.ItemImages.Where(a => a.ItemID == itemid));
                db.SaveChanges();
                string storePath = LegacyWeb.MapPath("/uploads/itemimages/" + itemid);
                if (Directory.Exists(storePath))
                    try
                    {
                        Directory.Delete(storePath, true);
                    }
                    catch (Exception e)
                    {

                    }

            }

            return Json(ItemImg);

        }
        [QkAuthorize(Roles = "Dev,Edit Item")]
        public ActionResult Edit(long? id)
        {
            long selectid = 0;
            var acc2 = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 12 && a.Group != 14 && a.Group != 23 && a.Status == Status.active).Select(s => new SelectFormat
            {
                id = s.AccountsID,
                text = s.Name
            }).ToList();
            var initial = new SelectFormat() { id = 0, text = "Select Account" };
            acc2.Insert(0, initial); acc2.Insert(0, initial);
            ViewBag.SetAccount = QkSelect.List(acc2, "id", "text");
            var mcminstock = db.EnableSettings.Where(o => o.EnableType == "materialcentrewiseminstock").SingleOrDefault();
            ViewBag.mcminstockwise = 1;
            ViewBag.mcminstockwise = (mcminstock != null) ? mcminstock.Status : Status.inactive;
            var use = (from a in db.Items
                       join b in db.suggestItems on a.ItemID equals b.sugitemid
                       where b.priitemid == id
                       select new
                       {
                           ID = b.sugitemid,
                           Name = a.ItemCode + " " + a.ItemName
                       })
                 .ToList();
            long[] selids = use.Select(o => o.ID).ToArray();
            ViewBag.itemn = new MultiSelectList(use, "ID", "Name", selids);

            var Brand = db.ItemBrands
                   .Select(s => new
                   {
                       Id = s.ItemBrandID,
                       BrandName = s.ItemBrandName
                   }).ToList();
            ViewBag.ItemBrand = QkSelect.List(Brand, "Id", "BrandName");

            var Tax = db.Taxs
                   .Select(s => new
                   {
                       Id = s.TaxID,
                       TaxName = s.TaxName
                   }).ToList();
            ViewBag.Tax = QkSelect.List(Tax, "Id", "TaxName");

            var Unit = db.ItemUnits
                   .Select(s => new
                   {
                       Id = s.ItemUnitID,
                       UnitName = s.ItemUnitName
                   }).ToList();
            ViewBag.ItemUnit = QkSelect.List(Unit, "Id", "UnitName");

            var Size = db.ItemSizes
                   .Select(s => new
                   {
                       Id = s.ItemSizeID,
                       SizeName = s.ItemSizeName
                   }).ToList();
            ViewBag.ItemSize = QkSelect.List(Unit, "Id", "SizeName");

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var Currency = db.CurrencyMasters
                    .Select(s => new
                    {
                        Id = s.Id,
                        Code = s.CurrencyCode
                    }).ToList();
            ViewBag.NCurrency = QkSelect.List(Currency, "Id", "Code");

            var Suppliers = db.Suppliers
                    .Select(s => new
                    {
                        Id = s.SupplierID,
                        Code = s.SupplierName
                    }).ToList();

            ViewBag.Supp = QkSelect.List(Suppliers, "Id", "Code");

            ViewBag.JItemType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "General", Value="1"},
                new SelectListItem() {Text = "Diamond", Value="2"},
                new SelectListItem() {Text = "Watch", Value="3"},
                new SelectListItem() {Text = "Object", Value="4"},
            }, "Value", "Text");
            var color = db.ItemColors
                             .Select(s => new
                             {
                                 ID = s.ItemColorID,
                                 Name = s.ItemColorName,
                             })
                             .ToList();
            ViewBag.Color = QkSelect.List(color, "ID", "Name");

            var Prefix = db.PrefixMasters
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.PrefixCode,
                             })
                             .ToList();
            ViewBag.Prefix = QkSelect.List(Prefix, "ID", "Name");


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Item Item = db.Items.Find(id);

            if (Item == null)
            {
                return NotFound();
            }
            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            //check enable commision
            var enablecomm = db.EnableSettings.Where(a => a.EnableType == "ItemCommision").FirstOrDefault();
            var commcheck = enablecomm != null ? enablecomm.Status : Status.inactive;
            ViewBag.COMEnable = commcheck;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            ViewBag.JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            ViewBag.PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;

            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            ViewBag.EnableCurrency = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;


            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType").Select(a => a.TypeValue).FirstOrDefault();

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            ItemViewModel Prod = new ItemViewModel();
            var jewellery = db.Jewellerys.Where(y => y.Item == id).FirstOrDefault();
            var Watch = db.Watchs.Where(y => y.Item == id).FirstOrDefault();
            var Diamond = db.Diamonds.Where(y => y.Item == id).FirstOrDefault();
            var Scaf = db.Scaffolds.Where(y => y.Item == id).FirstOrDefault();
            if (jewellery != null)
            {
                Prod.PromotionalItem = jewellery.PromotionalItem;
                Prod.Country = jewellery.Country;
                Prod.Style = jewellery.Style;
                Prod.Type = jewellery.Type;
                Prod.SetRef = jewellery.SetRef;
                Prod.Tagline1 = jewellery.TagLine1;
                Prod.Tagline2 = jewellery.TagLine2;
                Prod.Tagline3 = jewellery.TagLine3;
                Prod.Tagline4 = jewellery.TagLine4;
                Prod.Tagline5 = jewellery.TagLine5;
            }

            if (Diamond != null)
            {
                Prod.Design = Diamond.Design;
                Prod.ComponentDetails = Diamond.ComponentDetails;
                Prod.Clarify = Diamond.Clarify;
                Prod.Fluorescence = Diamond.Fluorescence;
                Prod.Range = Diamond.Range;
                Prod.CertificateNo = Diamond.CertificateNo;
                Prod.Time = Diamond.Time;
            }
            if (Watch != null)
            {
                Prod.Refno = Watch.Refno;
                Prod.Warranty = Watch.Warranty;
                Prod.ModelNo = Watch.ModelNo;
                Prod.ModelName = Watch.ModelName;
                Prod.Straptype = Watch.Straptype;
                Prod.DialShape = Watch.DialShape;
                Prod.DialColor = Watch.DialColor;
                Prod.Material = Watch.Material;
                Prod.Movement = Watch.Movement;
                Prod.Weight = Watch.Weight;
                Prod.StoneType = Watch.StoneType;
            }
            if (Scaf != null)
            {
                Prod.SCWeight = Scaf.Weight;
                Prod.CBM = Scaf.CBM;
            }

            Prod.ItemCode = Item.ItemCode;
            Prod.ItemName = Item.ItemName;
            Prod.ItemArabic = Item.ItemArabic;
            Prod.ItemDescription = Item.ItemDescription;
            Prod.SellingPrice = Item.SellingPrice;
            Prod.PurchasePrice = Item.PurchasePrice;
            Prod.MRP = Item.MRP;
            Prod.BasePrice = Item.BasePrice;
            Prod.KeepStock = Item.KeepStock;
            Prod.slreq = Item.slreq;
            Prod.accmap = Item.accmap;
            Prod.ItemCategoryID = Convert.ToInt64(Item.ItemCategoryID);
            Prod.ItemBrandID = Convert.ToInt64(Item.ItemBrandID);
            Prod.PartNumber = Item.PartNumber;
            Prod.Prefix = Item.Prefix;
            Prod.Currency = Convert.ToInt64(Item.Currency);
            Prod.ConRate = Item.ConRate;

            Prod.ItemColorID = Item.ItemColorID;
            Prod.ItemSizeID = Item.ItemSizeID;
            Prod.TaxID = Convert.ToInt64(Item.TaxID);
            Prod.Barcode = Item.Barcode;
            Prod.Status = Item.Status;
            Prod.ItemCategorys = db.ItemCategorys.ToList();
            Prod.ItemBrands = db.ItemBrands.ToList();
            Prod.ItemColors = db.ItemColors.ToList();
            Prod.ItemSizes = db.ItemSizes.ToList();
            Prod.Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList();
            Prod.ItemUnits = db.ItemUnits.ToList();
            //-----------------------------
            Prod.ItemUnitID = Item.ItemUnitID;
            Prod.SubUnitId = Item.SubUnitId;
            Prod.ConFactor = Item.ConFactor;
            Prod.cashprice = (Item.cashprice == null) ? Item.SellingPrice : Item.cashprice;
            Prod.creditprice = (Item.creditprice == null) ? Item.SellingPrice : Item.creditprice;
            Prod.OpeningStock = Item.OpeningStock;
            Prod.accountid = Item.accountid;
            Prod.MinStock = Item.MinStock;
            Prod.Commission = Item.Commission;
            Prod.Branch = Item.Branch;
            Prod.ItemType = Item.ItemType;
            Prod.Supplier = Item.Supplier;
            Prod.SupplierRef = Item.SupplierRef;
            Prod.StockValue = Item.StockValue;
            Prod.InSaleInvoice = Item.InSaleInvoice;
            Prod.daysexpirty = Item.daysexpirty;
            Prod.pricingstatagy = Item.PricingStrategy;
            Prod.amounttype = Item.PricingStrategyAmountType;
            Prod.amount = Item.PricingStrategyValue;
            Prod.pricingstatagytype = Item.PricingStrategyType;
            Prod.lockprice = Item.lockprice;
            ViewBag.preEntry = db.Items.Where(a => !db.ItemBundles.Select(b => b.mainItem).Contains(a.ItemID) && a.ItemID < id).Select(a => a.ItemID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Items.Where(a => !db.ItemBundles.Select(b => b.mainItem).Contains(a.ItemID) && a.ItemID > id).Select(a => a.ItemID).DefaultIfEmpty().Min();

            Prod.HireType = db.HireTypes.ToList();
            Prod.HireTypes = db.HireRates.Where(a => a.ItemId == id).Select(a => new HireTypeViewModel() { type = a.type, Rate = a.Rate }).ToList();

            var Category = db.ItemCategorys.Where(p => p.ItemCategoryID == Item.ItemCategoryID)
                    .Select(s => new
                    {
                        Id = s.ItemCategoryID,
                        CategoryName = s.ItemCategoryName
                    }).ToList();
            ViewBag.ItemCategory = QkSelect.List(Category, "Id", "CategoryName");

            //-----------check item exists in trans------------------
            var Exists = (db.PEItemss.Any(c => c.Item == id) || db.PRItemss.Any(c => c.Item == id) || db.SEItemss.Any(c => c.Item == id) || db.SRItemss.Any(c => c.Item == id) ||
                 db.QuotationItems.Any(c => c.Item == id) || db.DvItems.Any(c => c.Item == id) || db.PFItemss.Any(c => c.Item == id) || db.QuotationItems.Any(c => c.Item == id)
                 || db.PurchaseOrderItems.Any(c => c.Item == id) || db.SalesOrderItems.Any(c => c.Item == id) || db.BOMItems.Any(c => c.ItemId == id)
                 || db.JCItems.Any(c => c.Item == id) || db.ProItems.Any(c => c.ItemId == id) || db.MtFromPartyItemss.Any(c => c.Item == id) || db.MtToPartyItemss.Any(c => c.Item == id));

            ViewBag.ItemExist = Exists == true ? 1 : 0;

            //-------------------------------------------------------

            var ImageBag = (from b in db.ItemImages
                            where b.ItemID == id
                            select new
                            {
                                ImgId = b.ItemImageID,
                                FileName = b.FileName,
                                ItemImId = b.ItemID
                            }).FirstOrDefault();
            if (ImageBag != null)
            {
                Prod.ImageName = ImageBag.FileName;
                Prod.ImageId = ImageBag.ImgId;
                Prod.ItmImageId = ImageBag.ItemImId;
            }

            var DocBag = (from b in db.ItemDocuments
                          where b.ItemID == id
                          select new
                          {
                              DocId = b.ItemDocumentID,
                              FileName = b.FileName,
                              ItemDoId = b.ItemID,

                          }).FirstOrDefault();
            if (DocBag != null)
            {
                Prod.DocId = DocBag.DocId;
                Prod.DocName = DocBag.FileName;
                Prod.ItmDocId = DocBag.ItemDoId;


            }
            List<AdditionaldocViewModel> DocBaglst = (from b in db.ItemDocuments
                                                      where b.ItemID == id
                                                      select new AdditionaldocViewModel
                                                      {
                                                          DocId = b.ItemDocumentID,
                                                          FileName = b.FileName,
                                                          ItmDocId = b.ItemID,

                                                      }).ToList();
            Prod.ItemDocumentlst = DocBaglst;
            Prod.LstAdditionalImages = (from a in db.AttachmentDocuments
                                        where (a.TransactionID == id & a.TransactionType == "Item")
                                        select new AdditionalImageViewModel
                                        {
                                            DocumentID = a.DocumentID,
                                            ItemId = a.TransactionID,
                                            FileName = a.FileName,
                                            FileNameSaved = a.FileName,
                                        }).ToList();

            companySet();
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Edit", Prod);
            }
            else
            {
                return View(Prod);
            }
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Item")]
        public JsonResult Edit(Int64 id, ItemViewModel ProdViewModel, string fnval, int? printcount)
        {
            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            ViewBag.EnableCurrency = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            var suggestitems = ProdViewModel.suggestitem;
            db.suggestItems.RemoveRange(db.suggestItems.Where(o => o.priitemid == id));
            db.SaveChanges();
            if (suggestitems != null)
            {
                foreach (long sugitemid in suggestitems)
                {
                    SuggestItem it = new SuggestItem
                    {
                        priitemid = id,
                        sugitemid = sugitemid

                    };
                    db.suggestItems.Add(it);
                    db.SaveChanges();
                }
            }
            if (ViewBag.EnableCurrency == 0)
            {
                ProdViewModel.Currency = ProdViewModel.Currency;
                ProdViewModel.ConRate = ProdViewModel.ConRate;
            }
            else
            {
                ProdViewModel.Currency = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.Id).FirstOrDefault();
                var getrate = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.ConvertionRate).FirstOrDefault();
                ProdViewModel.ConRate = decimal.TryParse(getrate, out var parsedConRate) ? parsedConRate : 0; // guard: non-numeric/blank ConvertionRate string -> FormatException
            }

            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                Item items = db.Items.Find(id);
                var BarCodeExists = false;// ProdViewModel.Barcode != null ? db.Barcodes.Any(u => u.BarcodeNumber == ProdViewModel.Barcode && u.ItemID != id) : false;
                var ItemCodeExists = db.Items.Any(u => u.ItemCode == ProdViewModel.ItemCode && u.ItemID != id && items.ItemCode != u.ItemCode);
                var ItemNameExists = db.Items.Any(u => u.ItemName == ProdViewModel.ItemName && u.ItemID != id && items.ItemName != u.ItemName);

                if (ItemCodeExists)
                {
                    msg = "A Item with same Item code exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if (ItemNameExists)
                {
                    msg = "A Item with same Name exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else if (BarCodeExists)
                {
                    msg = "A Item with same Barcode exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    var UserId = User.Identity.GetUserId();

                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = ProdViewModel.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    items.ItemCode = ProdViewModel.ItemCode;
                    items.ItemName = ProdViewModel.ItemName;
                    items.ItemArabic = ProdViewModel.ItemArabic;
                    items.accountid = ProdViewModel.accountid;
                    items.ItemDescription = ProdViewModel.ItemDescription;
                    items.SellingPrice = ProdViewModel.SellingPrice;
                    items.PurchasePrice = ProdViewModel.PurchasePrice;
                    items.MRP = ProdViewModel.MRP;
                    items.BasePrice = ProdViewModel.BasePrice;
                    items.KeepStock = ProdViewModel.KeepStock;
                    items.ItemCategoryID = ProdViewModel.ItemCategoryID;
                    items.ItemBrandID = ProdViewModel.ItemBrandID;
                    items.ItemUnitID = ProdViewModel.ItemUnitID;
                    items.ItemColorID = ProdViewModel.ItemColorID;
                    items.ItemSizeID = ProdViewModel.ItemSizeID;
                    items.TaxID = ProdViewModel.TaxID;
                    items.Barcode = ProdViewModel.Barcode;
                    items.Status = ProdViewModel.Status;
                    items.PartNumber = ProdViewModel.PartNumber;
                    items.Branch = Branch;
                    items.Supplier = ProdViewModel.Supplier;
                    items.SupplierRef = ProdViewModel.SupplierRef;
                    items.ItemType = ProdViewModel.ItemType;
                    items.Currency = ProdViewModel.Currency;
                    items.ConRate = ProdViewModel.ConRate;
                    items.cashprice = ProdViewModel.cashprice;
                    items.creditprice = ProdViewModel.creditprice;
                    items.ConFactor = (ProdViewModel.ConFactor <= 0) ? 1 : ProdViewModel.ConFactor; // PREVENT: clamp meaningless 0 conversion factor to 1 (root of the ConFactor divide-by-zero class)
                    items.OpeningStock = ProdViewModel.OpeningStock;
                    items.MinStock = ProdViewModel.MinStock;
                    items.Commission = ProdViewModel.Commission == null ? 0 : ProdViewModel.Commission;
                    items.Prefix = ProdViewModel.Prefix;
                    items.StockValue = ProdViewModel.StockValue;
                    items.OpeningCost = (ProdViewModel.StockValue != 0 && ProdViewModel.StockValue != null && ProdViewModel.OpeningStock != 0 && ProdViewModel.OpeningStock != null) ? (decimal)(ProdViewModel.StockValue / ProdViewModel.OpeningStock) : 0;
                    items.InSaleInvoice = ProdViewModel.InSaleInvoice;
                    items.slreq = ProdViewModel.slreq;
                    items.accmap = ProdViewModel.accmap;
                    items.daysexpirty = ProdViewModel.daysexpirty;
                    items.PricingStrategy = ProdViewModel.pricingstatagy;
                    items.lockprice = ProdViewModel.lockprice;
                    items.PricingStrategyAmountType = ProdViewModel.amounttype;
                    items.PricingStrategyValue = ProdViewModel.amount;
                    items.PricingStrategyType = ProdViewModel.pricingstatagytype;
                    //-----------------------------

                    //-----------------------------

                    var Exists = (db.PEItemss.Any(c => c.Item == id) || db.PRItemss.Any(c => c.Item == id) || db.SEItemss.Any(c => c.Item == id) || db.SRItemss.Any(c => c.Item == id) ||
                                    db.QuotationItems.Any(c => c.Item == id) || db.DvItems.Any(c => c.Item == id) || db.PFItemss.Any(c => c.Item == id) || db.QuotationItems.Any(c => c.Item == id)
                                    || db.PurchaseOrderItems.Any(c => c.Item == id) || db.SalesOrderItems.Any(c => c.Item == id) || db.BOMItems.Any(c => c.ItemId == id)
                                    || db.JCItems.Any(c => c.Item == id) || db.ProItems.Any(c => c.ItemId == id) || db.MtFromPartyItemss.Any(c => c.Item == id) || db.MtToPartyItemss.Any(c => c.Item == id));
                    if (1==1)
                    {
                        items.ItemUnitID = ProdViewModel.ItemUnitID;
                        items.SubUnitId = ProdViewModel.SubUnitId;
                    }


                    if (ProdViewModel.ItemImage.ToList().First() != null)
                    {
                        var itimage = com.Images(ProdViewModel, id);
                    }
                    if (ProdViewModel.ItemDocument.ToList().First() != null)
                    {
                        var itdoc = com.Document(ProdViewModel, id);
                    }

                    //hire rates
                    var HirRate = db.HireRates.Where(a => a.ItemId == id).FirstOrDefault();
                    if (HirRate != null)
                    {
                        db.HireRates.RemoveRange(db.HireRates.Where(a => a.ItemId == id));
                        db.SaveChanges();
                    }
                    if (ProdViewModel.HireTypes != null)
                    {
                        foreach (HireTypeViewModel Hire in ProdViewModel.HireTypes)
                        {
                            var rate = new HireRate
                            {
                                type = Hire.type,
                                Rate = Hire.Rate,
                                ItemId = id
                            };
                            db.HireRates.Add(rate);
                            db.SaveChanges();
                        }
                    }

                    if (ProdViewModel.ItemType == 2 || ProdViewModel.ItemType == 3 || ProdViewModel.ItemType == 4)
                    {
                        //// Create Jewellery Object
                        Jewellery jwl = db.Jewellerys.Where(y => y.Item == items.ItemID).FirstOrDefault();
                        if (jwl != null)
                        {
                            jwl.PromotionalItem = ProdViewModel.PromotionalItem;
                            jwl.Country = ProdViewModel.Country;
                            jwl.Style = ProdViewModel.Style;
                            jwl.Type = ProdViewModel.Type;
                            jwl.SetRef = ProdViewModel.SetRef;
                            jwl.TagLine1 = ProdViewModel.Tagline1;
                            jwl.TagLine2 = ProdViewModel.Tagline2;
                            jwl.TagLine3 = ProdViewModel.Tagline3;
                            jwl.TagLine4 = ProdViewModel.Tagline4;
                            jwl.TagLine5 = ProdViewModel.Tagline5;
                            db.Entry(jwl).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            var jl = new Jewellery
                            {
                                Item = items.ItemID,
                                PromotionalItem = ProdViewModel.PromotionalItem,
                                Country = ProdViewModel.Country,
                                Style = ProdViewModel.Style,
                                Type = ProdViewModel.Style,
                                SetRef = ProdViewModel.SetRef,
                                TagLine1 = ProdViewModel.Tagline1,
                                TagLine2 = ProdViewModel.Tagline2,
                                TagLine3 = ProdViewModel.Tagline3,
                                TagLine4 = ProdViewModel.Tagline4,
                                TagLine5 = ProdViewModel.Tagline5
                            };
                            db.Jewellerys.Add(jl);
                            db.SaveChanges();
                        }

                        if (ProdViewModel.ItemType == 2)
                        {
                            var wtch = db.Watchs.Where(o => o.Item == items.ItemID).FirstOrDefault();
                            if (wtch != null)
                            {
                                db.Watchs.RemoveRange(db.Watchs.Where(a => a.Item == items.ItemID));
                            }
                            Diamond dmd = db.Diamonds.Where(y => y.Item == items.ItemID).FirstOrDefault();
                            if (dmd != null)
                            {
                                dmd.Design = ProdViewModel.Design;
                                dmd.Time = ProdViewModel.Time;
                                dmd.ComponentDetails = ProdViewModel.ComponentDetails;
                                dmd.Clarify = ProdViewModel.Clarify;
                                dmd.CertificateNo = ProdViewModel.CertificateNo;
                                dmd.Fluorescence = ProdViewModel.Fluorescence;
                                dmd.Range = ProdViewModel.Range;
                                db.Entry(dmd).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                var sl = new Diamond
                                {
                                    Item = items.ItemID,
                                    Time = ProdViewModel.Time,
                                    Design = ProdViewModel.Design,
                                    ComponentDetails = ProdViewModel.ComponentDetails,
                                    Clarify = ProdViewModel.Clarify,
                                    CertificateNo = ProdViewModel.CertificateNo,
                                    Fluorescence = ProdViewModel.Fluorescence,
                                    Range = ProdViewModel.Range
                                };
                                db.Diamonds.Add(sl);
                                db.SaveChanges();
                            }

                        }
                        else if (ProdViewModel.ItemType == 3)
                        {
                            var diam = db.Diamonds.Where(o => o.Item == items.ItemID).FirstOrDefault();
                            if (diam != null)
                            {
                                db.Diamonds.RemoveRange(db.Diamonds.Where(a => a.Item == items.ItemID));
                            }
                            Watch wtc = db.Watchs.Where(y => y.Item == items.ItemID).FirstOrDefault();
                            if (wtc != null)
                            {
                                wtc.Refno = ProdViewModel.Refno;
                                wtc.Warranty = ProdViewModel.Warranty;
                                wtc.ModelNo = ProdViewModel.ModelNo;
                                wtc.ModelName = ProdViewModel.ModelName;
                                wtc.Straptype = ProdViewModel.Straptype;
                                wtc.DialShape = ProdViewModel.DialShape;
                                wtc.DialColor = ProdViewModel.DialColor;
                                wtc.Material = ProdViewModel.Material;
                                wtc.Movement = ProdViewModel.Movement;
                                wtc.Weight = ProdViewModel.Weight;
                                wtc.StoneType = ProdViewModel.StoneType;
                                db.Entry(wtc).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                            else
                            {
                                var wt = new Watch
                                {
                                    Item = items.ItemID,
                                    Refno = ProdViewModel.Refno,
                                    Warranty = ProdViewModel.Warranty,
                                    ModelNo = ProdViewModel.ModelNo,
                                    ModelName = ProdViewModel.ModelName,
                                    Straptype = ProdViewModel.Straptype,
                                    DialShape = ProdViewModel.DialShape,
                                    DialColor = ProdViewModel.DialColor,
                                    Material = ProdViewModel.Material,
                                    Movement = ProdViewModel.Movement,
                                    Weight = ProdViewModel.Weight,
                                    StoneType = ProdViewModel.StoneType
                                };
                                db.Watchs.Add(wt);
                                db.SaveChanges();
                            }

                        }
                        else
                        {
                            var diam = db.Diamonds.Where(o => o.Item == items.ItemID).FirstOrDefault();
                            var wtch = db.Watchs.Where(o => o.Item == items.ItemID).FirstOrDefault();
                            if (diam != null)
                            {
                                db.Diamonds.RemoveRange(db.Diamonds.Where(a => a.Item == items.ItemID));
                            }
                            if (wtch != null)
                            {
                                db.Watchs.RemoveRange(db.Watchs.Where(a => a.Item == items.ItemID));
                            }
                        }

                    }
                    else
                    {
                        var jewellery = db.Jewellerys.Where(y => y.Item == items.ItemID).FirstOrDefault();
                        var Watch = db.Watchs.Where(y => y.Item == items.ItemID).FirstOrDefault();
                        var Diamond = db.Diamonds.Where(y => y.Item == items.ItemID).FirstOrDefault();

                        if (jewellery != null)
                        {
                            db.Jewellerys.RemoveRange(db.Jewellerys.Where(a => a.Item == items.ItemID));
                        }
                        if (Watch != null)
                        {
                            db.Watchs.RemoveRange(db.Watchs.Where(a => a.Item == items.ItemID));
                        }
                        if (Diamond != null)
                        {
                            db.Diamonds.RemoveRange(db.Diamonds.Where(a => a.Item == items.ItemID));
                        }
                    }

                    if (ProdViewModel.ItemType == 5)
                    {
                        var scafold = db.Scaffolds.Where(o => o.Item == items.ItemID).FirstOrDefault();
                        if (scafold != null)
                        {
                            db.Scaffolds.RemoveRange(db.Scaffolds.Where(a => a.Item == items.ItemID));
                        }
                        Scaffold scfold = db.Scaffolds.Where(y => y.Item == items.ItemID).FirstOrDefault();
                        if (scfold != null)
                        {
                            scfold.Weight = ProdViewModel.SCWeight;
                            scfold.CBM = ProdViewModel.CBM;
                            db.Entry(scfold).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            var scafolds = new Scaffold
                            {
                                CBM = ProdViewModel.CBM,
                                Weight = ProdViewModel.SCWeight,
                                Item = items.ItemID,
                            };
                            db.Scaffolds.Add(scafolds);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        var scafold = db.Scaffolds.Where(y => y.Item == items.ItemID).FirstOrDefault();
                        if (scafold != null)
                        {
                            db.Scaffolds.RemoveRange(db.Scaffolds.Where(a => a.Item == items.ItemID));
                        }
                    }
                    // batch stock
                    var SEBst = db.BatchStocks.Where(a => a.Reference == items.ItemID && a.Type == "Opening").FirstOrDefault();
                    if (SEBst != null)
                    {
                        db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == items.ItemID && a.Type == "Opening"));
                        db.SaveChanges();
                    }
                    if (ProdViewModel.bstmodel != null && ProdViewModel.OpeningStock > 0 && ProdViewModel.KeepStock == true)
                    {
                        foreach (var bst in ProdViewModel.bstmodel)
                        {
                            decimal totBtch = 0;
                            decimal BOStock = (totBtch <= ProdViewModel.OpeningStock) ? bst.StockIn : (decimal)(ProdViewModel.OpeningStock - bst.StockIn);
                            decimal bStock = BOStock * ProdViewModel.ConFactor;
                            if (bst.BatchNo != "" && bst.BatchNo != null && bStock > 0)
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
                                BatchStock Btst = new BatchStock();
                                Btst.BatchNo = bst.BatchNo;
                                Btst.Item = items.ItemID;
                                Btst.Unit = ProdViewModel.ItemUnitID;
                                Btst.Cost = ProdViewModel.PurchasePrice;
                                Btst.StockIn = bStock;
                                Btst.StockOut = 0;
                                Btst.Order = 1;
                                Btst.EXP = exp;
                                Btst.MFG = mfg;
                                Btst.Reference = items.ItemID;
                                Btst.Type = "Opening";

                                Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                Btst.Date = Convert.ToDateTime(System.DateTime.Now);


                                db.BatchStocks.Add(Btst);
                            }
                        }
                        db.SaveChanges();
                    }

                    db.Entry(items).State = EntityState.Modified;
                    db.SaveChanges();
                    com.addlog(LogTypes.Updated, UserId, "Item", "Items", findip(), items.ItemID, "Item Updated Successfully");

                    /*************************************** For Additional Images **************************************************/

                    if (ProdViewModel.LstAdditionalImages != null && ProdViewModel.LstAdditionalImages.Count > 0)
                    {
                        //Deleting all Current Images Attached
                        var AddtnlImages = db.AttachmentDocuments.Where(a => (a.TransactionID == id && a.TransactionType == "Item")).FirstOrDefault();

                        if (AddtnlImages != null)
                        {
                            db.AttachmentDocuments.RemoveRange(db.AttachmentDocuments.Where(a => (a.TransactionID == id && a.TransactionType == "Item")));
                            db.SaveChanges();
                        }

                        var fileName = "";
                        var FStatus = Status.active;
                        int i = 1;

                        foreach (var item in ProdViewModel.LstAdditionalImages)
                        {
                            //For new rows
                            if (item.FileName != "" && item.FileName != null)
                            {
                                // Files upload
                                IFormFile file = Request.Form.Files[i];//i is starting from 1, because 0 is ItemImage file

                                fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);

                                var uploadUrl = LegacyWeb.MapPath("~/uploads/itemimages/AdditionalImages/" + id);

                                //Creating folder
                                if (!Directory.Exists(uploadUrl))
                                    Directory.CreateDirectory(uploadUrl);

                                file.SaveAs(Path.Combine(uploadUrl, fileName));
                            }
                            //For Retrieved rows
                            else if (item.FileNameSaved != null)
                            {
                                fileName = item.FileNameSaved;
                            }
                            //For Rows with no File selected
                            else
                                fileName = "";

                            if (fileName != "")
                            {
                                //Saving in table AttachmentDocuments
                                AttachmentDocuments ImageDocuments = new AttachmentDocuments
                                {
                                    TransactionID = id,
                                    TransactionType = "Item",
                                    FileName = fileName,
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };

                                db.AttachmentDocuments.Add(ImageDocuments);
                                db.SaveChanges();
                            }
                            i++;
                        }
                    }
                    //If there is no rows, delete all data
                    else
                    {
                        var AddtnlImages = db.AttachmentDocuments.Where(a => (a.TransactionID == id && a.TransactionType == "Item")).FirstOrDefault();

                        if (AddtnlImages != null)
                        {
                            db.AttachmentDocuments.RemoveRange(db.AttachmentDocuments.Where(a => (a.TransactionID == id && a.TransactionType == "Item")));
                            db.SaveChanges();
                        }

                        string storePath = LegacyWeb.MapPath("~/uploads/itemimages/AdditionalImages/" + id);
                        if (Directory.Exists(storePath))
                            Directory.Delete(storePath, true);
                    }
                    /*************************************************************************************/


                    if (fnval == "print")
                    {
                        var item = db.Items.Where(n => n.ItemID == id).Select(b => new
                        {
                            b.ItemName,
                            b.Barcode,
                            b.MRP,
                            ItemPrice = b.SellingPrice,
                            PCount = printcount
                        }).FirstOrDefault();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item } };
                    }
                    else
                    {
                        msg = "Successfully added Item details.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        [QkAuthorize(Roles = "Dev,Delete Item")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item Item = db.Items.Find(id);
            if (Item == null)
            {
                return NotFound();
            }
            return View(Item);
        }

        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Item")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var chk = DeleteItem(id);
            if (chk == "Success")
            {
                stat = true;
                msg = "Successfully deleted Item details.";
            }
            else
            {
                stat = false;
                msg = chk;//"Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteItemWithMsg(long itemId)
        {
            string msg = null;
            if (db.PEItemss.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Purchase !!";
            }
            else if (db.PRItemss.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Purchase Return !!";
            }
            else if (db.SEItemss.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Sales !!";
            }
            else if (db.SRItemss.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Sales Return !!";
            }
            else if (db.QuotationItems.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Quotation !!";
            }
            else if (db.DvItems.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in DeliveryNote !!";
            }
            else if (db.PFItemss.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Pro Forma !!";
            }
            else if (db.PurchaseOrderItems.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Purchase Order !!";
            }
            else if (db.SalesOrderItems.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Sales Order !!";
            }
            else if (db.JCItems.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Job Card !!";
            }
            else if (db.MtFromPartyItemss.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Materials Received From Party Entry !!";
            }
            else if (db.MtToPartyItemss.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Materials Issued to Party Entry !!";
            }
            else if (db.BundleItems.Any(c => c.ItemId == itemId))
            {
                msg = "Item Already used in Item Bundle !!";
            }
            else if (db.SJItemGenerates.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Stock Journal Generate !!";
            }
            else if (db.SJItemConsumes.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Stock Journal Consumes !!";
            }
            else if (db.StockTransferItems.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Stock Transfer !!";
            }
            else if (db.StockAdjustments.Any(c => c.ItemID == itemId))
            {
                msg = "Item Already used in Stock Adjustments !!";
            }
            else if (db.HrItems.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Hire Return !!";
            }
            else if (db.PLItems.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Packing List !!";
            }
            else if (db.BOMItems.Any(c => c.ItemId == itemId))
            {
                msg = "Item Already used in Bill Of Material !!";
            }
            else if (db.BillOfMaterials.Any(c => c.ItemId == itemId))
            {
                msg = "Item Already used in Bill Of Material !!";
            }
            else if (db.ProItems.Any(c => c.ItemId == itemId))
            {
                msg = "Item Already used in Production !!";
            }
            else if (db.GeneratedItem.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Production !!";
            }
            else if (db.UnassembleItems.Any(c => c.ItemId == itemId))
            {
                msg = "Item Already used in Unassembles !!";
            }
            else if (db.ConsumedItem.Any(c => c.Item == itemId))
            {
                msg = "Item Already used in Unassembles !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Item")]
        public ActionResult DeleteAllItem(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = DeleteItem(arr);
                if (chk == "Success")
                {
                    count++;
                }
                else
                {
                    notdel++;
                }
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Items, Unable to Delete " + notdel + " Items. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Items.", true);
            }
            else
            {
                Success("Deleted " + count + " Items.", true);
            }
            return RedirectToAction("Index", "Item");
        }

        private string DeleteItem(long itemId)
        {
            Item Item = db.Items.Find(itemId);
            //    || db.SEItemss.Any(c => c.Item == itemId) || db.SRItemss.Any(c => c.Item == itemId)
            //    || db.QuotationItems.Any(c => c.Item == itemId) || db.DvItems.Any(c => c.Item == itemId)
            //    || db.PFItemss.Any(c => c.Item == itemId)
            //    || db.PurchaseOrderItems.Any(c => c.Item == itemId) || db.SalesOrderItems.Any(c => c.Item == itemId)
            //    || db.BOMItems.Any(c => c.ItemId == itemId) || db.JCItems.Any(c => c.Item == itemId)
            //    || db.ProItems.Any(c => c.ItemId == itemId) || db.UnassembleItems.Any(c => c.ItemId == itemId)
            //    || db.MtFromPartyItemss.Any(c => c.Item == itemId) || db.MtToPartyItemss.Any(c => c.Item == itemId)
            //    || db.BundleItems.Any(c => c.ItemId == itemId))
            //    || db.SJItemGenerates.Any(c => c.Item == itemId) || db.SJItemConsumes.Any(c => c.Item == itemId)
            //    || db.StockTransferItems.Any(c => c.Item == itemId) || db.StockAdjustments.Any(c => c.ItemID == itemId)
            var Msg = chkDeleteItemWithMsg(itemId);
            if (Msg != null)
            {
                return Msg;
            }
            else
            {
                return DeleteFn(itemId);
            }
        }

        public string DeleteFn(long id)
        {
            Item Item = db.Items.Find(id);
            // prefix no
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            if (PreCheck == Status.active)
            {
                var lastNo = (Item.ItemCode.Length >= 5) ? Item.ItemCode.Substring(Item.ItemCode.Length - 5) : "0";
                Int64 No = Convert.ToInt64(lastNo);
                var ItemPrefix = db.ItemPrefixs.Where(a => a.Prefix == Item.Prefix && a.No == No);
                if (ItemPrefix != null)
                {
                    db.ItemPrefixs.RemoveRange(db.ItemPrefixs.Where(a => a.Prefix == Item.Prefix && a.No == No));
                }
            }
            //
            var JewItem = db.Jewellerys.Where(a => a.Item == id);
            var DItem = db.Diamonds.Where(a => a.Item == id);
            var WItem = db.Watchs.Where(a => a.Item == id);
            var ItemPrefixs = db.Items.Where(a => a.Prefix == id);
            if (JewItem != null)
            {
                db.Jewellerys.RemoveRange(db.Jewellerys.Where(a => a.Item == id));

            }
            if (DItem != null)
            {
                db.Diamonds.RemoveRange(db.Diamonds.Where(a => a.Item == id));
            }
            if (WItem != null)
            {
                db.Watchs.RemoveRange(db.Watchs.Where(a => a.Item == id));
            }

            var ItemDoc = db.ItemDocuments.Where(a => a.ItemID == id).FirstOrDefault();
            if (ItemDoc != null)
            {
                db.ItemDocuments.RemoveRange(db.ItemDocuments.Where(a => a.ItemID == id));

                string storePath = LegacyWeb.MapPath("~/uploads/itemdocuments/" + id);
                if (Directory.Exists(storePath))
                    Directory.Delete(storePath, true);
            }

            /*********** Delete from AttachmentDocuments Table *********************/
            var AddtnlImages = db.AttachmentDocuments.Where(a => (a.TransactionID == id && a.TransactionType == "Item")).FirstOrDefault();
            if (AddtnlImages != null)
            {
                db.AttachmentDocuments.RemoveRange(db.AttachmentDocuments.Where(a => (a.TransactionID == id && a.TransactionType == "Item")));

                string storePath = LegacyWeb.MapPath("~/uploads/itemimages/AdditionalImages/" + id);
                if (Directory.Exists(storePath))
                    Directory.Delete(storePath, true);
            }
            /***********************************************************************/

            var ItemImg = db.ItemImages.Where(a => a.ItemID == id).FirstOrDefault();
            if (ItemImg != null)
            {
                db.ItemImages.RemoveRange(db.ItemImages.Where(a => a.ItemID == id));

                string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + id);
                if (Directory.Exists(storePath))
                    Directory.Delete(storePath, true);

            }
            var ItemBrCode = db.Barcodes.Where(a => a.ItemID == id).FirstOrDefault();
            if (ItemBrCode != null)
            {
                db.Barcodes.RemoveRange(db.Barcodes.Where(a => a.ItemID == id));
            }

            var Scafold = db.Scaffolds.Where(a => a.Item == id).FirstOrDefault();
            if (Scafold != null)
            {
                db.Scaffolds.RemoveRange(db.Scaffolds.Where(a => a.Item == id));
            }
            var HRates = db.HireRates.Where(a => a.ItemId == id);
            if (HRates != null)
            {
                db.HireRates.RemoveRange(HRates);
            }

            // batch stock
            var SEBst = db.BatchStocks.Where(a => a.Reference == id && a.Type == "Opening").FirstOrDefault();
            if (SEBst != null)
            {
                db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == id && a.Type == "Opening"));
                db.SaveChanges();
            }

            /*************Delete From ItemTransaction **********/
            com.DeleteItemTransaction(id);

            db.Items.Remove(Item);
            db.SaveChanges();
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Item", "Items", findip(), Item.ItemID, "Item Deleted Successfully");
            return "Success";
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Item Status")]
        public ActionResult ChangeStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Item itm = db.Items.Find(id);
            if (itm == null)
            {
                return NotFound();
            }
            if (type == "active")
            {
                ViewBag.type = "Active";
                ViewBag.link = "active";
                ViewBag.status = Status.active;
            }
            else
            {
                ViewBag.type = "Inactive";
                ViewBag.link = "inactive";
                ViewBag.status = Status.inactive;
            }
            return PartialView();
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Item Status")]
        public JsonResult ChangeStatus(string type, long? id, Item item)
        {
            bool stat = false;
            string msg;
            string types = "";
            Item itm = db.Items.Find(id);
            if (item.Status == Status.inactive)
            {
                types = " Inactive";
                itm.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                itm.Status = Status.active;
            }

            db.Entry(itm).State = EntityState.Modified;
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "Item", "Items", findip(), item.ItemID, "Successfully Changed the Item to" + types);


            stat = true;
            msg = " Successfully Changed the Item to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [QkAuthorize(Roles = "Dev,View Item Image")]
        public ActionResult Image(long? id)
        {

            return View(db.ItemImages.Where(b => b.ItemID == id).ToList());
        }

        public ActionResult ImageDelete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemImage ItemImg = db.ItemImages.Find(id);
            if (ItemImg == null)
            {
                return NotFound();
            }
            return View(ItemImg);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult ImageDelete(long id)
        {
            bool stat = false;
            string msg;
            ItemImage ItemImg = db.ItemImages.Find(id);
            db.ItemImages.Remove(ItemImg);
            db.SaveChanges();

            string fullPath = LegacyWeb.MapPath("~/uploads/itemimages/" + ItemImg.ItemID + "/" + ItemImg.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Item", "ItemImages", findip(), ItemImg.ItemImageID, "Item Image Deleted Successfully");


            Int64 Id = id;
            stat = true;
            msg = "Successfully deleted Item image.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        public ActionResult ImageAdd(long? id)
        {
            return View();
        }
        public ActionResult ImageAddstr(long id)
        {
            ItemImageViewModel ItemImage = new ItemImageViewModel();
            var itemid = db.Items.Where(o => o.ItemID == id).FirstOrDefault();
            ItemImage.itemid = itemid.ItemID;
            ItemImage.itemcode = itemid.ItemCode;
            ItemImage.itemname = itemid.ItemName;

                var itemimg = db.ItemImages.Where(o => o.ItemID == itemid.ItemID).Select(o => o.FileName).FirstOrDefault();
            var imppath = "/uploads/itemimages/" + itemid.ItemID.ToString() + "/";
            if(itemimg!=null)
            {
                imppath = imppath + itemimg;
            }
            ItemImage.imagpath = imppath;
                return View(ItemImage);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ImageAddstradd(ItemImageViewModel ItemImage)
        {
            var msg = "";
            bool stat = false;
            long id = (long)ItemImage.itemid;
            string itemcode = ItemImage.itemcode;

            if (ModelState.IsValid)
            {
                foreach (IFormFile file in ItemImage.ItmImage)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        db.ItemImages.RemoveRange(db.ItemImages.Where(o => o.ItemID == id));
                        db.SaveChanges();
                        var ItemImg = new ItemImage
                        {
                            ItemID = id,
                            FileName = Path.GetFileName(file.FileName),
                            Status = 1
                        };
                        db.ItemImages.Add(ItemImg);
                        db.SaveChanges();
                        string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + id);
                        if (!Directory.Exists(storePath))
                            Directory.CreateDirectory(storePath);
                        var InputFileName = Path.GetFileName(file.FileName);
                        var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                        //Save file to server folder  
                        file.SaveAs(ServerSavePath);

                        var UserId = User.Identity.GetUserId();
                        com.addlog(LogTypes.Created, UserId, "Item", "ItemImages", findip(), ItemImg.ItemImageID, "Item Image Added Successfully");

                        stat = true;
                        msg = "Successfully added Item image.";
                    }
                }
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form!";
            }
            // return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = id } };
            var intx = db.Items.OrderBy(o => o.ItemCode).ToList().IndexOf(db.Items.Find(id));
            var nid = id + 1;
            long nextid = db.Items.OrderBy(o => o.ItemCode).ToList()[intx + 1].ItemID; 
            return RedirectToAction("ImageAddstr/"+ nid, "Item");
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ImageAdd(ItemImageViewModel ItemImage, long id)
        {
            var msg = "";
            bool stat = false;
            if (ModelState.IsValid)
            {
                foreach (IFormFile file in ItemImage.ItmImage)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        db.ItemImages.RemoveRange(db.ItemImages.Where(o => o.ItemID == id));
                        db.SaveChanges();
                        var ItemImg = new ItemImage
                        {
                            ItemID = id,
                            FileName = Path.GetFileName(file.FileName),
                            Status = 1
                        };
                        db.ItemImages.Add(ItemImg);
                        db.SaveChanges();
                        string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + id);
                        if (!Directory.Exists(storePath))
                            Directory.CreateDirectory(storePath);
                        var InputFileName = Path.GetFileName(file.FileName);
                        var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                        //Save file to server folder  
                        file.SaveAs(ServerSavePath);

                        var UserId = User.Identity.GetUserId();
                        com.addlog(LogTypes.Created, UserId, "Item", "ItemImages", findip(), ItemImg.ItemImageID, "Item Image Added Successfully");

                        stat = true;
                        msg = "Successfully added Item image.";
                    }
                }
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form!";
            }
            // return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = id } };
            Success("Successfully added Item image.", true);
            return RedirectToAction("Index", "Item");
        }

        [QkAuthorize(Roles = "Dev,View Item Document")]
        public ActionResult Document(long? id)
        {
            return View(db.ItemDocuments.Where(b => b.ItemID == id).ToList());
        }
        public ActionResult DocumentDelete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemDocument ItemDoc = db.ItemDocuments.Find(id);
            if (ItemDoc == null)
            {
                return NotFound();
            }
            return View(ItemDoc);
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public JsonResult DocumentDelete(long id)
        {
            bool stat = false;
            string msg;
            ItemDocument ItemDoc = db.ItemDocuments.Find(id);
            db.ItemDocuments.Remove(ItemDoc);
            db.SaveChanges();

            string fullPath = LegacyWeb.MapPath("~/uploads/itemdocuments/" + ItemDoc.ItemID + "/" + ItemDoc.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }


            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Item", "ItemDocuments", findip(), ItemDoc.ItemDocumentID, "Item ItemDocument Deleted Successfully");


            Int64 Id = id;
            stat = true;
            msg = "Successfully deleted Item document.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        public ActionResult DocumentAdd(long? id)
        {
            return View();
        }

        [HttpPost]
        // [ValidateAntiForgeryToken]
        public JsonResult DocumentAdd(ItemDocumentViewModel ItemDoc, long id)
        {
            var msg = "";
            bool stat = false;
            if (ModelState.IsValid)
            {
                foreach (IFormFile file in ItemDoc.ItemDocument)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        var ItemDocument = new ItemDocument
                        {
                            ItemID = id,
                            FileName = Path.GetFileName(file.FileName),
                            Status = 1
                        };
                        db.ItemDocuments.Add(ItemDocument);
                        db.SaveChanges();
                        string storePath = LegacyWeb.MapPath("~/uploads/itemdocuments/" + id);
                        if (!Directory.Exists(storePath))
                            Directory.CreateDirectory(storePath);
                        var InputFileName = Path.GetFileName(file.FileName);
                        var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                        //Save file to server folder  
                        file.SaveAs(ServerSavePath);

                        var UserId = User.Identity.GetUserId();
                        com.addlog(LogTypes.Created, UserId, "Item", "ItemDocuments", findip(), ItemDocument.ItemDocumentID, "ItemDocument Added Successfully");
                        stat = true;
                        msg = "Successfully Added ItemDocument";
                    }
                }
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form!";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = id } };
        }




        public JsonResult SearchCategory(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ItemCategorys.Where(p => p.ItemCategoryName.ToLower().Contains(q.ToLower()) || p.ItemCategoryName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemCategoryName, //each json object will have
                                      id = b.ItemCategoryID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ItemCategorys.Select(b => new SelectFormat
                {
                    text = b.ItemCategoryName, //each json object will have
                    id = b.ItemCategoryID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Item Category" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "default" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 1, text = "General" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        //for sales/purchase entry
        [QkAuthorize]
        public JsonResult Search(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat4> serialisedJson;
            string stt = "All";

            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {


                serialisedJson = db.Items.Where(p => (p.Status == Status.active)
                && (p.ItemName.ToLower().Contains(q.ToLower()) || p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.Contains(q))
                 && (secnd == "" || p.ItemName.ToLower().Contains(secnd.ToLower()))
                && (third == "" || p.ItemName.ToLower().Contains(third.ToLower()))

                )

                                  //|| p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(q) 
                                  // || p.ItemCode.Contains(q) || p.Barcode.Contains(q)


                                  .Select(b => new SelectFormat4
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID,
                                      pprice = b.PurchasePrice,
                                      sprice = b.SellingPrice,
                                      cashprice = (b.cashprice == null) ? b.SellingPrice : b.cashprice,
                                      creditprice = (b.creditprice == null) ? b.SellingPrice : b.creditprice,
                                      ItemDescription = b.ItemDescription,
                                      ItemImId = b.ItemID,
                                      FileName = db.ItemImages.Where(o => o.ItemID == b.ItemID).Select(o => o.FileName).FirstOrDefault(),
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Status == Status.active).Select(b => new SelectFormat4
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID,
                    pprice = b.PurchasePrice,
                    sprice = b.SellingPrice,
                    cashprice = (b.cashprice == null) ? b.SellingPrice : b.cashprice,
                    creditprice = (b.creditprice == null) ? b.SellingPrice : b.creditprice,
                    ItemDescription = b.ItemDescription,
                    ItemImId = b.ItemID,
                    FileName = db.ItemImages.Where(o => o.ItemID == b.ItemID).Select(o => o.FileName).FirstOrDefault(),

                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat4() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        [QkAuthorize]
        public JsonResult Searchwithsupplier(string q, string x, string page,long? supplier)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat4> serialisedJson;
            string stt = "All";

            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {


                serialisedJson = db.Items.Where(p => (p.Status == Status.active)
                && (p.ItemName.ToLower().Contains(q.ToLower()) || p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.Contains(q))
                 && (secnd == "" || p.ItemName.ToLower().Contains(secnd.ToLower()))
                && (third == "" || p.ItemName.ToLower().Contains(third.ToLower()) &&
                (supplier==null||supplier==0||p.Supplier==supplier)
                )

                )

                                  //|| p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(q) 
                                  // || p.ItemCode.Contains(q) || p.Barcode.Contains(q)


                                  .Select(b => new SelectFormat4
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID,
                                      pprice = b.PurchasePrice,
                                      sprice = b.SellingPrice,
                                      cashprice = (b.cashprice == null) ? b.SellingPrice : b.cashprice,
                                      creditprice = (b.creditprice == null) ? b.SellingPrice : b.creditprice,
                                      ItemDescription = b.ItemDescription,
                                      ItemImId = b.ItemID,
                                      FileName = db.ItemImages.Where(o => o.ItemID == b.ItemID).Select(o => o.FileName).FirstOrDefault(),
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Status == Status.active&&
                (supplier == null || supplier == 0 || p.Supplier == supplier)).Select(b => new SelectFormat4
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID,
                    pprice = b.PurchasePrice,
                    sprice = b.SellingPrice,
                    cashprice = (b.cashprice == null) ? b.SellingPrice : b.cashprice,
                    creditprice = (b.creditprice == null) ? b.SellingPrice : b.creditprice,
                    ItemDescription = b.ItemDescription,
                    ItemImId = b.ItemID,
                    FileName = db.ItemImages.Where(o => o.ItemID == b.ItemID).Select(o => o.FileName).FirstOrDefault(),

                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat4() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        [QkAuthorize]
        public JsonResult Searchcus(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat4> serialisedJson;
            string stt = "All";

            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {


                serialisedJson = db.Items.Where(p => (p.Status == Status.active) && p.BasePrice!=0 && p.KeepStock==true
                && (p.ItemName.ToLower().Contains(q.ToLower()) || p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.Contains(q))
                 && (secnd == "" || p.ItemName.ToLower().Contains(secnd.ToLower()))
                && (third == "" || p.ItemName.ToLower().Contains(third.ToLower()))

                )

                                  //|| p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(q) 
                                  // || p.ItemCode.Contains(q) || p.Barcode.Contains(q)


                                  .Select(b => new SelectFormat4
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID,
                                   
                                      sprice = b.BasePrice,
                                      
                                      ItemDescription = b.ItemDescription,
                                      ItemImId = b.ItemID,
                                      FileName = db.ItemImages.Where(o => o.ItemID == b.ItemID).Select(o => o.FileName).FirstOrDefault(),
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Status == Status.active && p.BasePrice != 0 && p.KeepStock == true).Select(b => new SelectFormat4
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID,

                    sprice = b.BasePrice,

                    ItemDescription = b.ItemDescription,
                    ItemImId = b.ItemID,
                    FileName = db.ItemImages.Where(o => o.ItemID == b.ItemID).Select(o => o.FileName).FirstOrDefault(),


                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat4() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        [QkAuthorize]
        public JsonResult Searchgetname(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat5> serialisedJson = new List<SelectFormat5>();
            List<SelectFormat5> serialisedJsonsingle;

            string stt = "All";

            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            long[] mcs = { 1, 2, 3, 4, 5, 6, 7 };
            string mcname = "";
            foreach (var ddmc in mcs)
            {
                if (ddmc == 1)
                {
                    db = new ApplicationDbContext("quicknet");
                    mcname = "monther company";

                }
                else if (ddmc == 2)
                {
                    db = new ApplicationDbContext("abudhabi");
                    mcname = "ABU DHABI";

                }


                else if (ddmc == 3)
                {
                    db = new ApplicationDbContext("musafa");
                    mcname = "MUSSAFFA";

                }
                else if (ddmc == 4)
                {
                    db = new ApplicationDbContext("aln");
                    mcname = "ALN SHOP";

                }

                else if (ddmc == 5)
                {
                    db = new ApplicationDbContext("dubai");
                    mcname = "DUBAI SHOP";

                }
                else if (ddmc == 6)
                {
                    db = new ApplicationDbContext("moderate");
                    mcname = "MODERATE";

                }
                else if (ddmc == 7)
                {
                    db = new ApplicationDbContext("quickvision");
                    mcname = "QUICK VISION";

                }
                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {


                    serialisedJsonsingle = db.Items.Where(p => (p.Status == Status.active)
                    && (p.ItemName.ToLower().Contains(q.ToLower()) || p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.Contains(q))
                     && (secnd == "" || p.ItemName.ToLower().Contains(secnd.ToLower()))
                    && (third == "" || p.ItemName.ToLower().Contains(third.ToLower()))

                    )

                                      //|| p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(q) 
                                      // || p.ItemCode.Contains(q) || p.Barcode.Contains(q)


                                      .Select(b => new SelectFormat5
                                      {
                                          text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                          id = b.ItemName,
                                          //    pprice = b.PurchasePrice,
                                          //    sprice = b.SellingPrice,
                                          //    ItemDescription = b.ItemDescription,
                                          //    ItemImId = b.ItemID,
                                      })
                                      .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    serialisedJsonsingle = db.Items.Where(p => p.Status == Status.active).Select(b => new SelectFormat5
                    {
                        text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                        id = b.ItemName,
                        //pprice = b.PurchasePrice,
                        //sprice = b.SellingPrice,
                        //ItemDescription = b.ItemDescription,
                        //ItemImId = b.ItemID,

                    }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                }//
                serialisedJson.AddRange(serialisedJsonsingle);



            }


            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat5() { id = "-select--", text = stt };
                serialisedJson.Insert(0, initial);
            }
            var data = (from a in serialisedJson
                        group new { a.id, a.text } by a.text into grp
                        select new SelectFormat5
                        {
                            id = grp.FirstOrDefault().id,
                            text = grp.FirstOrDefault().text
                        });
            return Json(data.OrderBy(o => o.text));
        }




        [QkAuthorize]
        public JsonResult Searchold(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                string[] items = q.Split(' ');
                List<SelectFormat> serialisedJson3 = new List<SelectFormat>();
                foreach (var qa in items)
                {
                    List<SelectFormat> serialisedJson2 = db.Items.Where(p => (p.Status == Status.active) && p.ItemName.ToLower().Contains(qa.ToLower()) || p.ItemCode.ToLower().Contains(qa.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(qa) || p.ItemCode.Contains(qa) || p.Barcode.Contains(qa))
                                      .Select(b => new SelectFormat
                                      {
                                          text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                          id = b.ItemID
                                      })
                                      .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                    serialisedJson3.AddRange(serialisedJson2);
                }
                serialisedJson = serialisedJson3;
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Status == Status.active).Select(b => new SelectFormat
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }






        //for sales/purchase entry
        [HttpPost]
        public JsonResult Search4Mobile1(string q, string x)
        {
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 4;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 4 : 0;
            var item = (from b in db.Items
                        where (b.Status == Status.active && (check == true || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q)))
                        select new
                        {
                            text = b.ItemCode + " - " + b.ItemName,
                            id = b.ItemID,
                            b.ItemName,
                        }).OrderBy(b => b.ItemName).Skip(skip).Take(pageSize).ToList();
            return Json(item);
        }
        [HttpPost]
        public JsonResult Search4Mobile(string q, string x)
        {
            using (ApplicationDbContext db1 = new ApplicationDbContext())
            {
                var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
                var start = Request.Form.GetValues("page").FirstOrDefault();
                int pageSize = 4;
                int skip = start != null ? (Convert.ToInt32(start) - 1) * 4 : 0;
                var item = db1.Items.Where(b => b.Status == Status.active && (check == true || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q))).
                    Select(b => new
                    {
                        text = b.ItemCode + " - " + b.ItemName,
                        id = b.ItemID,
                        b.ItemName,
                    }).OrderBy(b => b.ItemName).Skip(skip).Take(pageSize).ToList();
                return Json(item);
            }
        }
        public JsonResult Searchdetailsapp(string q, string x, long? cust, string constat)
        {


            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
            var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            var start = 0;
            int pageSize = 5000;
            int skip = 0;

            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                        equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                        from g in pur.DefaultIfEmpty()
                        join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                        equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                        from h in sale.DefaultIfEmpty()
                        where (b.Status == Status.active && (check == true || b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q) || ((b.PartNumber.ToLower().Contains(q.ToLower()) || b.PartNumber.Contains(q)) && PartNoCheck == Status.active)))

                        select new
                        {
                            text = b.ItemCode + " - " + b.ItemName,
                            id = b.ItemID,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.MinStock,
                            b.KeepStock,
                            Tax = f.Percentage,
                            b.ItemUnitID,
                            g = h,
                            PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                            lastSale = (decimal?)h.ItemUnitPrice,
                            lastSaleU = (decimal?)h.ItemUnit,

                            lastPur = (decimal?)g.ItemUnitPrice,
                            lastPurU = (decimal?)g.ItemUnit,

                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                            b.Barcode,

                            OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,

                            PriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,
                            // unassemble -----
                            // main item
                            PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,


                        }).Distinct().OrderBy(b => b.ItemName).Skip(skip).Take(pageSize).AsEnumerable().Select(o => new
                        {
                            o.text,
                            o.id,
                            unit = o.ItemUnitID,
                            price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                            cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                            o.KeepStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.Tax,
                            lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
                            lastSaleU = o.lastSaleU,
                            lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
                            lastPurU = o.lastPurU,
                            o.g,

                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            o.Barcode,
                            o.SubUnitId,
                            PartNumber = o.PartNumber,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.ConFactor,

                            //o.SellingPrice,
                            //o.PurchasePrice,
                            //o.BasePrice,
                            //o.MRP,

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            cust = cust,
                        }).OrderBy(b => b.text).ToList();
            if (constat == "SalesEntry")
            {
                item = item.Where(a => a.KeepStock == false || (a.KeepStock == true && a.total > 0)).Skip(skip).Take(pageSize).ToList();
            }
            return Json(item);
        }

        public JsonResult Searchdetails(string q, string x, long? cust, string constat)
        {
            var StockItemsPerm = User.IsInRole("List Stockable Items");

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
            var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 10;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;
            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                        equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                        from g in pur.DefaultIfEmpty()
                        join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                        equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                        from h in sale.DefaultIfEmpty()
                        where
                        (b.Status == Status.active)
                && (b.ItemName.ToLower().Contains(q.ToLower())
                 && (secnd == "" || b.ItemName.ToLower().Contains(secnd.ToLower()))
                && (third == "" || b.ItemName.ToLower().Contains(third.ToLower())))
              || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q) || ((b.PartNumber.ToLower().Contains(q.ToLower()) || b.PartNumber.Contains(q)) && PartNoCheck == Status.active)
                        select new
                        {
                            text = b.ItemCode + " - " + b.ItemName,
                            id = b.ItemID,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.MinStock,
                            b.KeepStock,
                            Tax = f.Percentage,
                            b.ItemUnitID,
                            g = h,
                            PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                            lastSale = (decimal?)h.ItemUnitPrice,
                            lastSaleU = (decimal?)h.ItemUnit,

                            lastPur = (decimal?)g.ItemUnitPrice,
                            lastPurU = (decimal?)g.ItemUnit,

                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                            b.Barcode,

                            OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,

                            PriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,
                            // unassemble -----
                            // main item
                            PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,


                        }).Distinct().OrderBy(b => b.ItemName).Skip(skip).Take(pageSize).AsEnumerable().Select(o => new
                        {
                            o.text,
                            o.id,
                            unit = o.ItemUnitID,
                            price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                            cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                            o.KeepStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.Tax,
                            lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
                            lastSaleU = o.lastSaleU,
                            lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
                            lastPurU = o.lastPurU,
                            o.g,

                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            o.Barcode,
                            o.SubUnitId,
                            PartNumber = o.PartNumber,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.ConFactor,

                            //o.SellingPrice,
                            //o.PurchasePrice,
                            //o.BasePrice,
                            //o.MRP,

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            cust = cust,
                        }).OrderBy(b => b.text).ToList();
            if (constat == "SalesEntry" && StockItemsPerm == true)
            {
                item = item.Where(a => a.KeepStock == false || (a.KeepStock == true && a.total > 0)).Skip(skip).Take(pageSize).ToList();
            }
            return Json(item);
        }

        public JsonResult DateWiseStockInItem(string q, string x, long? cust, string constat, string date, long? MC)
        {
            var StockItemsPerm = User.IsInRole("List Stockable Items");

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
            var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 10;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;

            DateTime? sdate = null;
            if (date != "" && date != null)
            {
                sdate = DateTime.Parse(date, new CultureInfo("en-GB"));
            }
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && MC == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var y = (from b in db.Items
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                     from f in taxss.DefaultIfEmpty()
                     join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                     equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                     from g in pur.DefaultIfEmpty()
                     join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                     equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                     from h in sale.DefaultIfEmpty()
                     where (b.Status == Status.active && (check == true || b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q) || (b.PartNumber.ToLower().Contains(q.ToLower()) || (b.PartNumber.Contains(q)) && PartNoCheck == Status.active)))
                     select new
                     {
                         text = b.ItemCode + " - " + b.ItemName,
                         id = b.ItemID,
                         b.SellingPrice,
                         b.PurchasePrice,
                         b.BasePrice,
                         b.MRP,
                         b.MinStock,
                         b.KeepStock,
                         Tax = f.Percentage,
                         b.ItemUnitID,
                         g = h,
                         PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                         lastSale = (decimal?)h.ItemUnitPrice,
                         lastSaleU = (decimal?)h.ItemUnit,
                         lastPur = (decimal?)g.ItemUnitPrice,
                         lastPurU = (decimal?)g.ItemUnit,
                         b.ItemCode,
                         b.ItemName,
                         b.ItemArabic,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.Barcode,
                         //OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,
                         OpeningStock = ((MC == 0 || MC == 1 || MC == null) && b.OpeningStock != null) ? b.OpeningStock : 0,

                     }).Distinct();
            y = y.OrderBy(b => b.ItemName);
            var data = y.Skip(skip).Take(pageSize).ToList();
            var item =
              (from o in data

               let PriPurchase = (from a in db.PEItemss
                                  join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                  where (a.Item == o.id && a.ItemUnit == o.ItemUnitID)
                                  && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity
                                  }).ToList().Sum(v => v.ItemQuantity)


               let SubPurchase = (from a in db.PEItemss
                                  join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId
                                  where (a.Item == o.id && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                  select new
                                  {
                                      a.ItemQuantity
                                  }).ToList().Sum(v => v.ItemQuantity)


               let PriSale = (from a in db.SEItemss
                              join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                              where (a.Item == o.id && a.ItemUnit == o.ItemUnitID)
                              && (date == "" || EF.Functions.DateDiffDay(c.SEDate, sdate) >= 0)
                              && (c.SaleType != SaleType.Hire)
                              && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                              select new
                              {
                                  a.ItemQuantity,
                              }).ToList().Sum(v => v.ItemQuantity)


               let SubSale = (from a in db.SEItemss
                              join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId
                              where
                              (date == "" || EF.Functions.DateDiffDay(c.SEDate, sdate) >= 0)
                              && (a.Item == o.id && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                              && (c.SaleType != SaleType.Hire)
                              && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                              select new
                              {
                                  a.ItemQuantity,
                              }).ToList().Sum(v => v.ItemQuantity)

               let PriPReturn = (from a in db.PRItemss
                                 join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                 where (a.Item == o.id && a.ItemUnit == o.ItemUnitID)
                                 && (date == "" || EF.Functions.DateDiffDay(c.PRDate, sdate) >= 0)
                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                 select new
                                 {
                                     a.ItemQuantity,
                                 }).ToList().Sum(v => v.ItemQuantity)


               let SubPReturn = (from a in db.PRItemss
                                 join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId
                                 where (a.Item == o.id && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                 && (date == "" || EF.Functions.DateDiffDay(c.PRDate, sdate) >= 0)
                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                 select new
                                 {
                                     a.ItemQuantity,
                                 }).ToList().Sum(v => v.ItemQuantity)

               let PriSReturn = (from a in db.SRItemss
                                 join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                 where (a.Item == o.id && a.ItemUnit == o.ItemUnitID)
                                 && (date == "" || EF.Functions.DateDiffDay(c.SRDate, sdate) >= 0)
                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                 && (c.SaleType != SaleType.Hire)
                                 select new
                                 {
                                     a.ItemQuantity,
                                 }).ToList().Sum(v => v.ItemQuantity)


               let SubSReturn = (from a in db.SRItemss
                                 join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId
                                 where (a.Item == o.id && a.ItemUnit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && (date == "" || EF.Functions.DateDiffDay(c.SRDate, sdate) >= 0)
                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                  && (c.SaleType != SaleType.Hire)
                                 select new
                                 {
                                     a.ItemQuantity,
                                 }).ToList().Sum(v => v.ItemQuantity)


               let PriAddAdj = (from a in db.StockAdjustments
                                where (a.ItemID == o.id && a.ItemUnitID == o.ItemUnitID && a.AdjustmentType == AdjustmentType.Add)
                                && (date == "" || EF.Functions.DateDiffDay(a.AdjDate, sdate) >= 0)
                                && ((!MCList.Any() && MC == null) || MCArray.Contains(a.MaterialCenter) || MC == a.MaterialCenter)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(v => v.ItemQuantity)


               let SubAddAdj = (from a in db.StockAdjustments
                                where (a.ItemID == o.id && a.ItemUnitID == o.SubUnitId
                                && a.AdjustmentType == AdjustmentType.Add && o.PriUnit != o.SubUnit)
                                && (date == "" || EF.Functions.DateDiffDay(a.AdjDate, sdate) >= 0)
                                && ((!MCList.Any() && MC == null) || MCArray.Contains(a.MaterialCenter) || MC == a.MaterialCenter)
                                select new
                                {
                                    a.ItemQuantity,
                                }).ToList().Sum(v => v.ItemQuantity)

               let PriLessAdj = (from a in db.StockAdjustments
                                 where (a.ItemID == o.id && a.ItemUnitID == o.ItemUnitID && a.AdjustmentType == AdjustmentType.Less)
                                 && (date == "" || EF.Functions.DateDiffDay(a.AdjDate, sdate) >= 0)
                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(a.MaterialCenter) || MC == a.MaterialCenter)
                                 select new
                                 {
                                     a.ItemQuantity,
                                 }).ToList().Sum(v => v.ItemQuantity)


               let subLessAdj = (from a in db.StockAdjustments
                                 where (a.ItemID == o.id && a.ItemUnitID == o.SubUnitId
                                 && a.AdjustmentType == AdjustmentType.Less && o.PriUnit != o.SubUnit)
                                 && (date == "" || EF.Functions.DateDiffDay(a.AdjDate, sdate) >= 0)
                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(a.MaterialCenter) || MC == a.MaterialCenter)
                                 select new
                                 {
                                     a.ItemQuantity,
                                 }).ToList().Sum(v => v.ItemQuantity)

               let PriProdItem = (from a in db.GeneratedItem
                                  join c in db.Productions on a.Production equals c.ProductionId
                                  where (a.Item == o.id && a.Unit == o.ItemUnitID)
                                  && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(v => v.Qty)


               let SubProdItem = (from a in db.GeneratedItem
                                  join c in db.Productions on a.Production equals c.ProductionId
                                  where (a.Item == o.id && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                  select new
                                  {
                                      a.Qty,
                                  }).ToList().Sum(v => v.Qty)

               let PriProdCItem = (from a in db.ProItems
                                   join c in db.Productions on a.Production equals c.ProductionId
                                   where (a.ItemId == o.id && a.Unit == o.ItemUnitID)
                                   && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                   && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(v => v.Quantity)


               let SubProdCItem = (from a in db.ProItems
                                   join c in db.Productions on a.Production equals c.ProductionId
                                   where (a.ItemId == o.id && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                   && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                   && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                   select new
                                   {
                                       a.Quantity,
                                   }).ToList().Sum(v => v.Quantity)

               let PriUnItem = (from a in db.ConsumedItem
                                join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                where (a.Item == o.id && a.Unit == o.ItemUnitID)
                                && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                select new
                                {
                                    a.Qty,
                                }).ToList().Sum(v => v.Qty)

               let SubUnItem = (from a in db.ConsumedItem
                                join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                where (a.Item == o.id && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                select new
                                {
                                    a.Qty,
                                }).ToList().Sum(v => v.Qty)

               let PriUnCItem = (from a in db.UnassembleItems
                                 join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                 where (a.ItemId == o.id && a.Unit == o.ItemUnitID)
                                 && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                 select new
                                 {
                                     a.Quantity,
                                 }).ToList().Sum(v => v.Quantity)

               let SubUnCItem = (from a in db.UnassembleItems
                                 join c in db.Unassembles on a.Unassemble equals c.UnassembleId
                                 where (a.ItemId == o.id && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                 && (date == "" || EF.Functions.DateDiffDay(c.PEDate, sdate) >= 0)
                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MaterialCenter) || MC == c.MaterialCenter)
                                 select new
                                 {
                                     a.Quantity,
                                 }).ToList().Sum(v => v.Quantity)
               let PriStTrFrom = (from a in db.StockTransferItems
                                  join c in db.StockTransfers on a.StockTransferId equals c.Id
                                  where (a.Item == o.id && a.Unit == o.ItemUnitID)
                                  && (date == "" || EF.Functions.DateDiffDay(c.Date, sdate) >= 0)
                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MCFrom) || MC == c.MCFrom)
                                  select new
                                  {
                                      a.Quantity
                                  }).ToList().Sum(m => m.Quantity)

               let PriStTrTo = (from a in db.StockTransferItems
                                join c in db.StockTransfers on a.StockTransferId equals c.Id
                                where (a.Item == o.id && a.Unit == o.ItemUnitID)
                                && (date == "" || EF.Functions.DateDiffDay(c.Date, sdate) >= 0)
                                && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MCTo) || MC == c.MCTo)
                                select new
                                {
                                    a.Quantity
                                }).ToList().Sum(m => m.Quantity)

               let SubStTrFrom = (from a in db.StockTransferItems
                                  join c in db.StockTransfers on a.StockTransferId equals c.Id
                                  where (a.Item == o.id && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                  && (date == "" || EF.Functions.DateDiffDay(c.Date, sdate) >= 0)
                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MCFrom) || MC == c.MCFrom)
                                  select new
                                  {
                                      a.Quantity
                                  }).ToList().Sum(m => m.Quantity)

               let SubStTrTo = (from a in db.StockTransferItems
                                join c in db.StockTransfers on a.StockTransferId equals c.Id
                                where (a.Item == o.id && a.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                && (date == "" || EF.Functions.DateDiffDay(c.Date, sdate) >= 0)
                                && ((!MCList.Any() && MC == null) || MCArray.Contains(c.MCTo) || MC == c.MCTo)
                                select new
                                {
                                    a.Quantity
                                }).ToList().Sum(m => m.Quantity)
               select new
               {
                   o.text,
                   o.id,
                   o.SellingPrice,
                   o.PurchasePrice,
                   o.BasePrice,
                   o.MRP,
                   o.MinStock,
                   o.KeepStock,
                   o.Tax,
                   o.ItemUnitID,
                   o.g,
                   o.PartNumber,

                   o.lastSale,
                   o.lastSaleU,

                   o.lastPur,
                   o.lastPurU,

                   o.ItemCode,
                   o.ItemName,
                   o.ItemArabic,
                   o.SubUnitId,
                   o.PriUnit,
                   o.SubUnit,
                   o.ConFactor,
                   o.Barcode,
                   o.OpeningStock,

                   PriPurchase = (PriPurchase + (int)(SubPurchase / o.ConFactor)),
                   SubPurchase = (SubPurchase % o.ConFactor),

                   PriSale = (PriSale + (int)(SubSale / o.ConFactor)),
                   SubSale = (SubSale % o.ConFactor),

                   PriPReturn = (PriPReturn + (int)(SubPReturn / o.ConFactor)),
                   SubPReturn = (SubPReturn % o.ConFactor),

                   PriSReturn = (PriSReturn + (int)(SubSReturn / o.ConFactor)),
                   SubSReturn = (SubSReturn % o.ConFactor),


                   PriAddAdj,
                   SubAddAdj,
                   PriLessAdj,
                   subLessAdj,

                   pritotal = ((o.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriStTrTo) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriStTrFrom)),
                   subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom)),
                   total = (((o.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriStTrTo) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriStTrFrom)) * o.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom)),
                   cust = cust,
               }).OrderBy(b => b.text).ToList();


            if (constat == "SalesEntry" && StockItemsPerm == true)
            {
                item = item.Where(a => a.KeepStock == false || (a.KeepStock == true && a.total > 0)).Skip(skip).Take(pageSize).ToList();
            }
            return Json(item);
        }

        [HttpGet]
        public ActionResult AddItemnoteSearch()
        {
            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();

                ViewBag.MC = QkSelect.List(mcs, "id", "text");
            }
            else
            {
                var mcs = db.MCs.Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();
                var initial = new SelectFormat() { id = 0, text = "All" };
                mcs.Insert(0, initial);
                ViewBag.MC = QkSelect.List(mcs, "id", "text");

            }

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;

            ViewBag.Category = OptAll;

            return PartialView();
        }
        [HttpGet]
        public ActionResult AddItemSearch()
        {
            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();

                ViewBag.MC = QkSelect.List(mcs, "id", "text");
            }
            else
            {
                var mcs = db.MCs.Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();
                var initial = new SelectFormat() { id = 0, text = "All" };
                mcs.Insert(0, initial);
                ViewBag.MC = QkSelect.List(mcs, "id", "text");

            }

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;

            ViewBag.Category = OptAll;

            return PartialView();
        }
        [HttpPost]
        public JsonResult GetAddItemSearch2(long? brand, long? category, long? mc, string itemWord)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (mc == 0 || mc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;


            var userpermission = User.IsInRole("All Customers");


            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");
            var stocks = 0.0;
            var CurrentStock = 0.0;
            DateTime datenow = DateTime.Now;
            string[] searchkey = itemWord.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                itemWord = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }

            var v1 = (from a in db.Items
                      join b in db.ItemBrands on a.ItemBrandID equals b.ItemBrandID into temp2
                      from b in temp2.DefaultIfEmpty()
                      join c in db.ItemCategorys on a.ItemCategoryID equals c.ItemCategoryID into temp3
                      from c in temp3.DefaultIfEmpty()

                      where (brand == 0 || b.ItemBrandID == brand) &&
                         (category == 0 || c.ItemCategoryID == category) &&

                         (itemWord == "" || a.ItemName.ToString().ToLower().Contains(itemWord.ToLower()) || a.ItemCode.ToString().ToLower().Contains(itemWord.ToLower()))
                          && (secnd == "" || a.ItemName.ToLower().Contains(secnd.ToLower()))
                          && (third == "" || a.ItemName.ToLower().Contains(third.ToLower()))

                      select new
                      {
                          a.ItemID,
                          a.ItemName,
                          a.ItemCode,
                          a.PurchasePrice,
                          a.SellingPrice,
                          CurrentStock,
                          difference=a.SellingPrice-a.PurchasePrice,
                          days = (from c in db.SalesEntrys
                                  join d in db.SEItemss on c.SalesEntryId equals d.SalesEntry
                                  where d.Item == a.ItemID
                                  select new
                                  {
                                      Days = (c.SEDate == null) ? 0 : EF.Functions.DateDiffDay(c.SEDate, datenow),

                                  }).OrderBy(o => o.Days).Select(o => o.Days).FirstOrDefault(),
                      }).ToList().Select(o => new StockDetails2
                      {
                          id = o.ItemID,
                          ItemName = o.ItemName,
                          ItemCode = o.ItemCode,
                          PurchasePrice = o.PurchasePrice,
                          SellingPrice = o.SellingPrice,
                          CurrentStock = 0,
                          days = o.days,
                          margin=(o.PurchasePrice==0)?0:(o.difference/o.PurchasePrice)*100

                      }).Distinct().ToList();

            mc = mc != null ? mc : 0;
            var full2 = v1.Select(z => z.id).ToList();
            List<StockDetailsmovement> datadd = new List<StockDetailsmovement>();
            for (int i = 0; i < full2.Count(); i++)
            {

                var itemid = v1[i].id;


                StockDetailsmovement datadd2 = new StockDetailsmovement();
                int index = v1.FindIndex(o => o.id == full2[i]);


            }
            v1 = v1.ToList();
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v1 = v1.Where(p => p.ItemCode.ToString().ToLower().Contains(search.ToLower())
                   || p.ItemName.ToString().ToLower().Contains(search.ToLower())).ToList();
                // v1=v1.Where(a=>a.ItemCode.ToString().ToLower())
            }
            var data = v1.OrderByDescending(a => a.days).ToList();
            recordsTotal = v1.Count();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [HttpPost]
        public JsonResult GetAddItemnoteSearch( string itemWord)
        {
            var UserId = User.Identity.GetUserId();
           
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
         
          

            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;


            var userpermission = User.IsInRole("All Customers");


            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");
            var stocks = 0.0;
            var CurrentStock = 0.0;
            DateTime datenow = DateTime.Now;
            string[] searchkey = itemWord.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                itemWord = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }

            var v1 = (from a in db.QuotationItems
                      where
                         (itemWord == "" || a.ItemNote.ToString().ToLower().Contains(itemWord.ToLower()))
                          && (secnd == "" || a.ItemNote.ToLower().Contains(secnd.ToLower()))
                          && (third == "" || a.ItemNote.ToLower().Contains(third.ToLower()))

                      select new
                      {
                          id = a.QuotationItemId,
                          a.ItemNote


                      }).Distinct().OrderByDescending(o=>o.id).Take(150).ToList();

         
            
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v1 = v1.Where(p => p.ItemNote.ToString().ToLower().Contains(search.ToLower())
                  || p.ItemNote.ToString().ToLower().Contains(search.ToLower())).ToList();
                // v1=v1.Where(a=>a.ItemCode.ToString().ToLower())
            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v1 = v1.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            var data = v1.ToList();

            recordsTotal = v1.Count();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpPost]
        public JsonResult GetAddItemSearch(long? brand, long? category, long? mc, string itemWord)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            if (!MCList.Any() && (mc == 0 || mc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            var MCArray = MCList.ToArray();

            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;


            var userpermission = User.IsInRole("All Customers");


            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");
            var stocks = 0.0;
            var CurrentStock = 0.0;
            DateTime datenow = DateTime.Now;
            string[] searchkey = itemWord.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                itemWord = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }

            var v1 = (from a in db.Items
                      join b in db.ItemBrands on a.ItemBrandID equals b.ItemBrandID into temp2
                      from b in temp2.DefaultIfEmpty()
                      join c in db.ItemCategorys on a.ItemCategoryID equals c.ItemCategoryID into temp3
                      from c in temp3.DefaultIfEmpty()

                      where (brand == 0 || b.ItemBrandID == brand) &&
                         (category == 0 || c.ItemCategoryID == category) &&
                         (a.Status == Status.active) &&
                         (itemWord == "" || a.ItemName.ToString().ToLower().Contains(itemWord.ToLower()) || a.ItemCode.ToString().ToLower().Contains(itemWord.ToLower()))
                          && (secnd == "" || a.ItemName.ToLower().Contains(secnd.ToLower()))
                          && (third == "" || a.ItemName.ToLower().Contains(third.ToLower())) && a.KeepStock==true

                      select new
                      {
                          a.ItemID,
                          a.ItemName,
                          a.ItemCode,
                          a.PurchasePrice,
                          a.SellingPrice,
                          CurrentStock,

                          days = (from c in db.SalesEntrys
                                  join d in db.SEItemss on c.SalesEntryId equals d.SalesEntry
                                  where d.Item == a.ItemID
                                  select new
                                  {
                                      Days = (c.SEDate == null) ? 0 : EF.Functions.DateDiffDay(c.SEDate, datenow),

                                  }).OrderBy(o => o.Days).Select(o => o.Days).FirstOrDefault(),
                      }).ToList().Select(o => new StockDetails2
                      {
                          id = o.ItemID,
                          ItemName = o.ItemName,
                          ItemCode = o.ItemCode,
                          PurchasePrice = o.PurchasePrice,
                          SellingPrice = o.SellingPrice,
                          CurrentStock = 0,
                          days = o.days,

                      }).Distinct().ToList();

            mc = mc != null ? mc : 0;
            var full2 = v1.Select(z => z.id).ToList();
            List<StockDetailsmovement> datadd = new List<StockDetailsmovement>();
            for (int i = 0; i < full2.Count(); i++)
            {

                var itemid = v1[i].id;


                StockDetailsmovement datadd2 = new StockDetailsmovement();
                int index = v1.FindIndex(o => o.id == full2[i]);
                datadd2.currstock = GetItemWisestock3(full2[i], mc);
                v1[index].CurrentStock = datadd2.currstock;


            }
            v1 = v1.ToList();
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v1 = v1.Where(p => p.ItemCode.ToString().ToLower().Contains(search.ToLower())
                  || p.ItemName.ToString().ToLower().Contains(search.ToLower())).ToList();
                // v1=v1.Where(a=>a.ItemCode.ToString().ToLower())
            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v1 = v1.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            var data = v1.ToList();

            recordsTotal = v1.Count();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public decimal? GetItemWisestock3(long? itemid, long? ddmc)
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


            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", itemid);
            var selmc = new SqlParameter("@MCId", ddmc);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "1");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();


            return data[0].ITotalQty;

        }

        public decimal? GetItemWisestock4new(long? ddmc2, string SrcTxt, long? brandid, long? catogory, long? itemiddsingle,long[] avoiditemid,string ondate="")
        {
            db.SetCommandTimeOut(60 * 60);
            DateTime? ondates = null;
            if (ondate != "")
            {
                ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
            }
            var selitem = new SqlParameter("@ItemId", "");
                var selmc = new SqlParameter("@MCId", ddmc2);
                var brand = new SqlParameter("@BrandId", "");
                var stkble = new SqlParameter("@Stockble", 1);
                var catgry = new SqlParameter("@CategoryId", "");
                var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            if (ondate!="")
                 todate = new SqlParameter("@todate", ondates);
                var stype = new SqlParameter("@Stype", "1");
               
                    var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

                          if(avoiditemid!=null)
            {
            }
                
           
             var sum = dataadd.Where(o=>o.ITotalQty > 0).Sum(o => Math.Round(Convert.ToDecimal(o.ITotalStockValue),2));
           

            return sum;

        }

        public decimal? GetItemWisestock4(long? ddmc2, string SrcTxt, long? brandid, long? catogory, long? itemiddsingle)
        {
            db.SetCommandTimeOut(60 * 60);
            var UserId = User.Identity.GetUserId();
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var stock = Convert.ToDecimal(0);
            var itmids = (from a in db.Items
                          join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                          from b in sti.DefaultIfEmpty()
                          join c in db.StockTransfers on b.StockTransferId equals c.Id
                          where c.MCTo == ddmc2 
                          select new
                          {
                              Itemid = a.ItemID
                          }).Distinct();

            var peitem = (from a in db.Items
                          join b in db.PEItemss on a.ItemID equals b.Item into sti
                          from b in sti.DefaultIfEmpty()
                          join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                          where c.MaterialCenter == ddmc2 
                          select new
                          {
                              Itemid = a.ItemID
                          }).Distinct();
            var pritems = (from a in db.Items
                           join b in db.PRItemss on a.ItemID equals b.Item into sti
                           from b in sti.DefaultIfEmpty()
                           join c in db.PurchaseReturns on b.PurchaseReturnId equals c.PurchaseReturnId
                           where c.MaterialCenter == ddmc2 
                           select new
                           {
                               Itemid = a.ItemID
                           }).Distinct();

            var serritem = (from a in db.Items
                            join b in db.SRItemss on a.ItemID equals b.Item into sti
                            from b in sti.DefaultIfEmpty()
                            join c in db.SalesReturns on b.SalesReturnId equals c.SalesReturnId
                            where c.MaterialCenter == ddmc2 
                            select new
                            {
                                Itemid = a.ItemID
                            }).Distinct();
            var assetitemid = (from a in db.Items
                               join b in db.AssetTransferDetails on a.ItemID equals b.RefItemId into sti
                               from b in sti.DefaultIfEmpty()
                               join c in db.AssetTransferMasters on b.AssetEntryId equals c.AssetEntryId
                               where c.McFromId == ddmc2
                               select new
                               {
                                   Itemid = a.ItemID
                               }).Distinct();
            var assettoinventry = (from a in db.Items
                                   join b in db.AssetToInventoryDetails on a.ItemID equals b.RefItemId into sti
                                   from b in sti.DefaultIfEmpty()
                                   join c in db.AssetToInventoryMasters on b.EntryId equals c.EntryId
                                   where c.McFromId == ddmc2
                                   select new
                                   {
                                       Itemid = a.ItemID
                                   }).Distinct();
            var itmidsFROM = (from a in db.Items
                              join b in db.StockTransferItems on a.ItemID equals b.Item into sti
                              from b in sti.DefaultIfEmpty()
                              join c in db.StockTransfers on b.StockTransferId equals c.Id
                              where c.MCFrom == ddmc2
                              select new
                              {
                                  Itemid = a.ItemID
                              }).Distinct();
            var itmidstkadj = (from a in db.Items
                               join b in db.StockAdjustments on a.ItemID equals b.ItemID into sti
                               from b in sti.DefaultIfEmpty()
                               where b.MaterialCenter == ddmc2
                               select new
                               {
                                   Itemid = a.ItemID
                               }).Distinct();
            itmids = itmids.Union(itmidstkadj);
            itmids = itmids.Union(itmidsFROM);
            itmids = itmids.Union(assettoinventry);
                itmids = itmids.Union(assetitemid);

            itmids = itmids.Union(peitem);
            itmids = itmids.Union(serritem);
            itmids = itmids.Union(pritems);
            var itemlist = itmids.Select(o => o.Itemid).Distinct().ToArray();
            var TotAmt = Convert.ToDecimal(0);
            int i = 0;
            long[][] arrays = itemlist.GroupBy(s => i++ / 50).Select(s => s.ToArray()).ToArray();
            if (SrcTxt != "")
            {
                var srctxtitmids = db.Items.Where(o => o.ItemName.Contains(SrcTxt)).Select(o => o.ItemID).ToArray();
                var itm = String.Join(",", srctxtitmids);
                var selitem = new SqlParameter("@ItemId", itm);
                var selmc = new SqlParameter("@MCId", ddmc2);
                var brand = new SqlParameter("@BrandId", "0");
                if (brandid == null || brandid == 0)
                    brand = new SqlParameter("@BrandId", "0");
                else
                    brand = new SqlParameter("@BrandId", brandid);

                var stkble = new SqlParameter("@Stockble", "1");
                var searchtext = new SqlParameter("@searchtext", "");
                if (SrcTxt != "")
                {
                    searchtext = new SqlParameter("@searchtext", "%" + SrcTxt + "%");
                }
                var catgry = new SqlParameter("@CategoryId", "0");
                if (catogory == 0 || catogory == null)
                    catgry = new SqlParameter("@CategoryId", "0");
                else
                    catgry = new SqlParameter("@CategoryId", catogory);

                var fromdate = new SqlParameter("@fromdate", "");
                var todate = new SqlParameter("@todate", "");
                var stype = new SqlParameter("@Stype", "1");
                IEnumerable<StockDetails> data = new List<StockDetails>();
                var datadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod3 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype,@searchtext", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype, searchtext).AsEnumerable().OrderBy(a => a.IItemName).ToList();
                datadd = datadd.Where(o=>o.ITotalQty!=0).ToList();
                foreach (var datas in datadd)

                {

                    var qty = Convert.ToDecimal(datas.ITotalQty);
                    var prc = Convert.ToDecimal(datas.ISellingPrice);
                    TotAmt = TotAmt + (qty * prc);
                }

            }
            else if (itemiddsingle == null || itemiddsingle == 0)
            {
                foreach (var it in arrays)
                {
                    var itm = String.Join(",", it);
                    var selitem = new SqlParameter("@ItemId", itm);
                    var selmc = new SqlParameter("@MCId", ddmc2);
                    var brand = new SqlParameter("@BrandId", "0");
                    if (brandid == null || brandid == 0)
                        brand = new SqlParameter("@BrandId", "0");
                    else
                        brand = new SqlParameter("@BrandId", brandid);

                    var stkble = new SqlParameter("@Stockble", "1");
                    var searchtext = new SqlParameter("@searchtext", "");
                    if (SrcTxt != "")
                    {
                        searchtext = new SqlParameter("@searchtext", "%" + SrcTxt + "%");
                    }
                    var catgry = new SqlParameter("@CategoryId", "0");
                    if (catogory == 0 || catogory == null)
                        catgry = new SqlParameter("@CategoryId", "0");
                    else
                        catgry = new SqlParameter("@CategoryId", catogory);

                    var fromdate = new SqlParameter("@fromdate", "");
                    var todate = new SqlParameter("@todate", "");
                    var stype = new SqlParameter("@Stype", "1");
                    IEnumerable<StockDetails> data = new List<StockDetails>();
                    var datadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod3 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype,@searchtext", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype, searchtext).AsEnumerable().OrderBy(a => a.IItemName).ToList();
                    datadd = datadd.Where(o => o.ITotalQty != 0).ToList();
                    foreach (var datas in datadd)

                    {
                        var qty = Convert.ToDecimal(datas.ITotalQty);
                        var prc = Convert.ToDecimal(datas.ISellingPrice);
                        TotAmt = TotAmt + (decimal)datas.ITotalStockValue;
                    }
                }
            }
            else

            {

                var selitem = new SqlParameter("@ItemId", itemiddsingle);
                var selmc = new SqlParameter("@MCId", ddmc2);
                var brand = new SqlParameter("@BrandId", "0");
                if (brandid == null || brandid == 0)
                    brand = new SqlParameter("@BrandId", "0");
                else
                    brand = new SqlParameter("@BrandId", brandid);

                var stkble = new SqlParameter("@Stockble", "1");


                var catgry = new SqlParameter("@CategoryId", "0");
                if (catogory == 0 || catogory == null)
                    catgry = new SqlParameter("@CategoryId", "0");
                else
                    catgry = new SqlParameter("@CategoryId", catogory);

                var fromdate = new SqlParameter("@fromdate", "");
                var todate = new SqlParameter("@todate", "");
                var stype = new SqlParameter("@Stype", "1");
                IEnumerable<StockDetails> data = new List<StockDetails>();
                var datadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();
                datadd = datadd.Where(o => o.ITotalQty != 0).ToList();
                foreach (var datas in datadd)

                {
                    var qty = Convert.ToDecimal(datas.ITotalQty);
                    var prc = Convert.ToDecimal(datas.ISellingPrice);
                    TotAmt = TotAmt + (qty * prc);
                }
            }

            return TotAmt;

        }
        public decimal? getstockmc(long itemidd, long mcc)
        {
            return com.GetItemWisestock(itemidd, mcc);
        }
        [HttpPost]
        public JsonResult GetStockValue(long? brand, long? category, string SrcText, long?[] mc, string itemWord, long? itemid,string ondate)
        {
            db.SetCommandTimeOut(60 * 60);
            var UserId = User.Identity.GetUserId();

            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;


            var userpermission = User.IsInRole("All Customers");


            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");
            var CurrentStock = 0.0;
            var totalStock = Convert.ToDecimal(0);
            DateTime datenow = DateTime.Now;
            DateTime? ondates = null;
            if (ondate != "")
            {
                ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
            }
            var selitem = new SqlParameter("@ItemId", "");
                var selmc = new SqlParameter("@MCId", "");
                var brandd = new SqlParameter("@BrandId", "");
                var stkble = new SqlParameter("@Stockble", 1);
                var catgry = new SqlParameter("@CategoryId", "");
                var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            if (ondate != "")
                todate = new SqlParameter("@todate", ondates);
           
                var stype = new SqlParameter("@Stype", "1");
            long[] avoiditemids=null;
            List<StockDetails> dataadd = new List<StockDetails>();
   
                 dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brandd, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();
           if(mc[0]==0)
             avoiditemids = dataadd.Where(o => o.ITotalQty <= 0).Select(o => o.IItemID).ToList().ToArray();
            
                var mainv1 = (from a in db.MCs
                          where (mc.Contains(0) || mc.Contains(a.MCId))
                          && a.Status==Status.active
                          select new StockDetails3
                          {
                              MCID = a.MCId,
                              MCName = a.MCName,
                              stock = 0,
                              brandid = brand,
                              category = category,
                              itemid = itemid,
                              srctxt = SrcText,
                          }).ToList();
            var full2 = mainv1.Select(z => z.MCID).ToList();
            List<StockDetailsmovement> datadd = new List<StockDetailsmovement>();
            for (int i = 0; i < full2.Count(); i++)
            {

                StockDetailsmovement datadd2 = new StockDetailsmovement();
                int index = mainv1.FindIndex(o => o.MCID == full2[i]);
                datadd2.currstock = GetItemWisestock4new(full2[i], SrcText, brand, category, itemid, avoiditemids,ondate);
                mainv1[index].stock = datadd2.currstock;


            }

            var data = mainv1.ToList();
            recordsTotal = mainv1.Count();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpGet]
        public JsonResult GetItemById2(long ItemId)
        {
            var data = db.Items.Where(b => b.ItemID == ItemId).Select(b => new
            {
                b.ItemID,
                b.ItemCode,
                b.ItemName,
                ItemWithCode = b.ItemCode + " - " + b.ItemName,
                b.ItemArabic,
                b.Barcode,
                b.ItemDescription,
                b.SellingPrice,
                b.PurchasePrice,
                b.MRP,
                b.BasePrice,
                b.Status,
                b.KeepStock,
                b.ItemUnit,
                b.ItemUnitID,
                b.SubUnitId,
                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                Category = db.ItemCategorys.Where(a => a.ItemCategoryID == b.ItemCategoryID).Select(a => a.ItemCategoryName).FirstOrDefault(),
                Brand = db.ItemBrands.Where(a => a.ItemBrandID == b.ItemBrandID).Select(a => a.ItemBrandName).FirstOrDefault(),
                Color = db.ItemColors.Where(a => a.ItemColorID == b.ItemColorID).Select(a => a.ItemColorName).FirstOrDefault(),
                Tax = db.Taxs.Where(a => a.TaxID == b.TaxID).Select(a => a.Percentage).FirstOrDefault(),
                Size = db.ItemSizes.Where(a => a.ItemSizeID == b.ItemSizeID).Select(a => a.ItemSizeName).FirstOrDefault(),
                PriUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnitID).Select(a => a.ItemUnitName).FirstOrDefault(),
                SubUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.SubUnitId).Select(a => a.ItemUnitName).FirstOrDefault()

            }).ToList();
            return Json(data);

        }
        [HttpPost]
        public ActionResult BulksfixedellingPriceUpdateset(long[] bill,decimal per)
        {

            foreach (long ItemId in bill)
            {
                var it = db.Items.Find(ItemId);
                it.SellingPrice = it.PurchasePrice + it.PurchasePrice * per / 100;
                db.Entry(it).State = EntityState.Modified;
                db.SaveChanges();

            }
                Success("Updated",true);
            return RedirectToAction("BulksfixedellingPriceUpdate");
        }
        public ActionResult SelectAllId(long[] bill)
        {
            List<itemUpdates> alldata = new List<itemUpdates>();
            SerialNoViewModel ViewModel = new SerialNoViewModel();

            foreach (long ItemId in bill)
            {

                //Item
                var Item = db.Items.Select(s => new
                {
                    Id = s.ItemID,
                    Name = s.ItemCode + "-" + s.ItemName,
                }).ToList();
                ViewBag.ddlItem = QkSelect.List(Item, "Id", "Name");

                var data = db.Items.Where(b => b.ItemID == ItemId).Select(b => new itemUpdates
                {
                    ItemID = b.ItemID,
                    ItemCode = b.ItemCode,
                    ItemName = b.ItemName,
                    SellingPrice = b.SellingPrice,
                    PurchasePrice = b.PurchasePrice,
                    lockprice = b.lockprice,
                    pricingstatagy = b.PricingStrategy,
                    pricingstatagytype = b.PricingStrategyType,
                    amounttype = b.PricingStrategyAmountType,
                    currstock = 0,
                    amount = b.PricingStrategyValue,
                    status = (b.Status == Status.active) ? true : false,
                }).ToList().Select(o => new itemUpdates
                {
                    ItemID = o.ItemID,
                    ItemCode = o.ItemCode,
                    ItemName = o.ItemName,
                    SellingPrice = o.SellingPrice,
                    PurchasePrice = o.PurchasePrice,
                    lockprice = o.lockprice,
                    pricingstatagy = o.pricingstatagy,
                    pricingstatagytype = o.pricingstatagytype,
                    amounttype = o.amounttype,
                    currstock = com.GetItemWisestock(o.ItemID, 0),
                    amount = o.amount,
                    status = o.status,

                });

                alldata.AddRange(data);


            }
            ViewModel.itemUpdatess = alldata;

            return View(ViewModel);
        }
        public ActionResult BulksfixedellingPriceUpdate()
        {
            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();

                ViewBag.MC = QkSelect.List(mcs, "id", "text");
            }
            else
            {
                var mcs = db.MCs.Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();
                var initial = new SelectFormat() { id = 0, text = "All" };
                mcs.Insert(0, initial);
                ViewBag.MC = QkSelect.List(mcs, "id", "text");

            }

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;

            ViewBag.Category = OptAll;
            return View();
        }

        public ActionResult BulkPriceUpdate()
        {
            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();

                ViewBag.MC = QkSelect.List(mcs, "id", "text");
            }
            else
            {
                var mcs = db.MCs.Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();
                var initial = new SelectFormat() { id = 0, text = "All" };
                mcs.Insert(0, initial);
                ViewBag.MC = QkSelect.List(mcs, "id", "text");

            }

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;

            ViewBag.Category = OptAll;
            return View();
        }

        [HttpPost]
        public ActionResult BulkPriceUpdate(SerialNoViewModel ViewModal)
        {
        
           

            if (ViewModal.itemUpdatess != null)
            {
                foreach (var Row in ViewModal.itemUpdatess)
                {
                    if (ViewModal.EXP != "" && ViewModal.MFG != "")
                    {
                        var offerfrom = DateTime.Parse(ViewModal.MFG, new CultureInfo("en-GB"));
                        var offerto = DateTime.Parse(ViewModal.EXP, new CultureInfo("en-GB"));
                        db.BatchStocks.RemoveRange(db.BatchStocks.Where(o => o.Item == Row.ItemID));
                        db.SaveChanges();
                        BatchStock st = new BatchStock
                        {
                            Item = Row.ItemID,
                            MFG = offerfrom,
                            EXP = offerto,
                            Type = "offer",
                            Reference = Row.ItemID,
                            Cost = (decimal)Row.offerprice,
                            Date=System.DateTime.Now,
                            CreatedDate=System.DateTime.Now



                        };
                        db.BatchStocks.Add(st);
                        db.SaveChanges();
                    }
                    else
                    {

                        Item items = db.Items.Find(Row.ItemID);
                        items.ItemName = Row.ItemName;
                        items.SellingPrice = Convert.ToDecimal(Row.SellingPrice);
                        items.PurchasePrice = Convert.ToDecimal(Row.PurchasePrice);

                        items.lockprice = Row.lockprice;
                        items.Status = (Row.status == true) ? Status.active : Status.inactive;
                        items.PricingStrategy = Row.pricingstatagy;
                        items.PricingStrategyType = Row.pricingstatagytype;
                        items.PricingStrategyAmountType = Row.amounttype;
                        items.PricingStrategyValue = Row.amount;
                        db.Entry(items).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                }
                Success("Successfully Updated", true);
            }

            return RedirectToAction("BulkPriceUpdate");

        }
        [HttpGet]
        public ActionResult ItemSelect()
        {
            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();

                ViewBag.MC = QkSelect.List(mcs, "id", "text");
            }
            else
            {
                var mcs = db.MCs.Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();
                var initial = new SelectFormat() { id = 0, text = "All" };
                mcs.Insert(0, initial);
                ViewBag.MC = QkSelect.List(mcs, "id", "text");

            }

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;

            ViewBag.Category = OptAll;

            return PartialView();
        }

        public JsonResult SearchdetailsMCtech(string q, string x, long? cust, long mc, string constat)
        {

            var StockItemsPerm = User.IsInRole("List Stockable Items");

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var stockcheckinvoice = db.EnableSettings.Where(a => a.EnableType == "stockcheckinvoice").FirstOrDefault();
            var stockcheck = stockcheckinvoice != null ? stockcheckinvoice.Status : Status.inactive;
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
            var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 10;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;
            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            if (1 == 1)
            {
                var item1 = (from b in db.Items
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                             from e in cat.DefaultIfEmpty()
                             join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                             from f in taxss.DefaultIfEmpty()
                             join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                             equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                             from g in pur.DefaultIfEmpty()
                             join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                             equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                             from h in sale.DefaultIfEmpty()
                                 // where (b.Status == Status.active && (check == true || b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q) || (b.PartNumber.ToLower().Contains(q.ToLower()) || (b.PartNumber.Contains(q)) && PartNoCheck == Status.active)) && stkk.MCTo == mc)
                             where (b.Status == Status.active)
                  && b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q) || ((b.PartNumber.ToLower().Contains(q.ToLower()) || b.PartNumber.Contains(q)) && PartNoCheck == Status.active)
                             select new
                             {
                                 text = b.ItemCode + " - " + b.ItemName,
                                 id = b.ItemID,
                                 b.SellingPrice,
                                 b.PurchasePrice,
                                 b.BasePrice,
                                 b.MRP,
                                 b.MinStock,
                                 b.KeepStock,
                                 Tax = f.Percentage,
                                 b.ItemUnitID,
                                 g = h,
                                 PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                                 lastSale = (decimal?)h.ItemUnitPrice,
                                 lastSaleU = (decimal?)h.ItemUnit,

                                 lastPur = (decimal?)g.ItemUnitPrice,
                                 lastPurU = (decimal?)g.ItemUnit,

                                 b.ItemCode,
                                 b.ItemName,
                                 b.ItemArabic,
                                 b.SubUnitId,
                                 PriUnit = c.ItemUnitName,
                                 SubUnit = d.ItemUnitName,
                                 ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                                 b.Barcode,

                                 OpeningStock = ((mc == 0 || mc == 1 || mc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,

                                 PriPurchase = (decimal?)(from v in db.PEItemss
                                                          join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                          where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.ItemQuantity
                                                          }).Sum(c => c.ItemQuantity) ?? 0,

                                 SubPurchase = (decimal?)(from v in db.PEItemss
                                                          join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                          where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.ItemQuantity
                                                          }).Sum(c => c.ItemQuantity) ?? 0,

                                 PriAsset = (decimal?)(from v in db.AssetTransferDetails
                                                       join w in db.AssetTransferMasters on v.AssetEntryId equals w.AssetEntryId
                                                       where v.RefItemId == b.ItemID
                                                       && (mc == 0 || mc == w.McFromId) && v.UnitId == b.ItemUnitID
                                                       select new
                                                       {
                                                           v.Quantity
                                                       }).Sum(c => c.Quantity) ?? 0,

                                 SubAsset = (decimal?)(from v in db.AssetTransferDetails
                                                       join w in db.AssetTransferMasters on v.AssetEntryId equals w.AssetEntryId
                                                       where v.RefItemId == b.ItemID && v.UnitId == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                       && (mc == 0 || mc == w.McFromId)

                                                       select new
                                                       {
                                                           v.Quantity
                                                       }).Sum(c => c.Quantity) ?? 0,



                                 PriSale = (decimal?)(from v in db.SEItemss
                                                      join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                      where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      && (w.SaleType != SaleType.Hire)
                                                      select new
                                                      {
                                                          v.ItemQuantity
                                                      }).Sum(c => c.ItemQuantity) ?? 0,

                                 SubSale = (decimal?)(from v in db.SEItemss
                                                      join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                      where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      && (w.SaleType != SaleType.Hire)
                                                      select new
                                                      {
                                                          v.ItemQuantity
                                                      }).Sum(c => c.ItemQuantity) ?? 0,

                                 PriAReturn = (decimal?)(from v in db.AssetToInventoryDetails
                                                         join w in db.AssetToInventoryMasters on v.EntryId equals w.EntryId
                                                         where v.RefItemId == b.ItemID && v.UnitId == b.ItemUnitID
                                                         && (mc == 0 || mc == w.McFromId)

                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(c => c.Quantity) ?? 0,

                                 SubAReturn = (decimal?)(from v in db.AssetToInventoryDetails
                                                         join w in db.AssetToInventoryMasters on v.EntryId equals w.EntryId
                                                         where v.RefItemId == b.ItemID && v.UnitId == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                         && (mc == 0 || mc == w.McFromId)

                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(c => c.Quantity) ?? 0,

                                 PriPReturn = (decimal?)(from v in db.PRItemss
                                                         join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,

                                 SubPReturn = (decimal?)(from v in db.PRItemss
                                                         join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,

                                 PriSReturn = (decimal?)(from v in db.SRItemss
                                                         join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         && (w.SaleType != SaleType.Hire)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,

                                 SubSReturn = (decimal?)(from v in db.SRItemss
                                                         join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         && (w.SaleType != SaleType.Hire)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,

                                 //stock adjustment---
                                 PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                                 SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,

                                 PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                                 subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                                 //-------
                                 // production ----
                                 PriProdItem = (decimal?)(from v in db.GeneratedItem
                                                          join w in db.Productions on v.Production equals w.ProductionId
                                                          where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.Qty
                                                          }).Sum(c => c.Qty) ?? 0,
                                 SubProdItem = (decimal?)(from v in db.GeneratedItem
                                                          join w in db.Productions on v.Production equals w.ProductionId
                                                          where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.Qty
                                                          }).Sum(c => c.Qty) ?? 0,
                                 // compined item
                                 PriProdCItem = (decimal?)(from v in db.ProItems
                                                           join w in db.Productions on v.Production equals w.ProductionId
                                                           where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                           && (mc == 0 || mc == w.MaterialCenter)
                                                           select new
                                                           {
                                                               v.Quantity
                                                           }).Sum(c => c.Quantity) ?? 0,

                                 SubProdCItem = (decimal?)(from v in db.ProItems
                                                           join w in db.Productions on v.Production equals w.ProductionId
                                                           where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                           && (mc == 0 || mc == w.MaterialCenter)
                                                           select new
                                                           {
                                                               v.Quantity
                                                           }).Sum(c => c.Quantity) ?? 0,

                                 // main item
                                 PriUnItem = (decimal?)(from v in db.ConsumedItem
                                                        join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                        where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                        && (mc == 0 || mc == w.MaterialCenter)
                                                        select new
                                                        {
                                                            v.Qty
                                                        }).Sum(c => c.Qty) ?? 0,
                                 SubUnItem = (decimal?)(from v in db.ConsumedItem
                                                        join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                        where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                        && (mc == 0 || mc == w.MaterialCenter)
                                                        select new
                                                        {
                                                            v.Qty
                                                        }).Sum(c => c.Qty) ?? 0,
                                 // compined item
                                 PriUnCItem = (decimal?)(from v in db.UnassembleItems
                                                         join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                         where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(c => c.Quantity) ?? 0,

                                 SubUnCItem = (decimal?)(from v in db.UnassembleItems
                                                         join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                         where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(c => c.Quantity) ?? 0,
                                 // stock transfer


                             }).Distinct();
                var vd = item1.OrderBy(b => b.ItemName).ToList();
                if (constat != "SalesEntry" || StockItemsPerm != true)
                {

                }
                else
                {
                    vd = item1.OrderBy(b => b.ItemName).ToList();
                }
                var data = (from o in vd
                            let PriStTrFrom = (decimal?)(from v in db.StockTransferItems
                                                         join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                         where (v.Item == o.id && v.Unit == o.ItemUnitID)
                                                         && (mc == 0 || mc == w.MCFrom)
                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(m => m.Quantity) ?? 0
                            let PriStTrTo = (decimal?)(from v in db.StockTransferItems
                                                       join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                       where (v.Item == o.id && v.Unit == o.ItemUnitID)
                                                       && (mc == w.MCTo)
                                                       select new
                                                       {
                                                           v.Quantity
                                                       }).Sum(m => m.Quantity) ?? 0
                            let SubStTrFrom = (decimal?)(from v in db.StockTransferItems
                                                         join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                         where (v.Item == o.id && v.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                                         && (mc == w.MCFrom)
                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(m => m.Quantity) ?? 0
                            let SubStTrTo = (decimal?)(from v in db.StockTransferItems
                                                       join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                       where (v.Item == o.id && v.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                                       && (mc == w.MCTo)
                                                       select new
                                                       {
                                                           v.Quantity
                                                       }).Sum(m => m.Quantity) ?? 0



                            select new
                            {
                                o.text,
                                o.id,
                                unit = o.ItemUnitID,
                                price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                                cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                                o.KeepStock,
                                MinStock = (o.MinStock != null) ? o.MinStock : 0,
                                o.Tax,
                                lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
                                lastSaleU = o.lastSaleU,
                                lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
                                lastPurU = o.lastPurU,
                                o.g,

                                o.ItemCode,
                                o.ItemName,
                                o.ItemArabic,
                                o.Barcode,
                                o.SubUnitId,
                                PartNumber = o.PartNumber,
                                PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                o.ConFactor,


                                pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo + o.PriAReturn) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom + o.PriAsset)),
                                subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo + o.SubAReturn) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom + o.SubAsset)),
                                total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo + o.PriAReturn) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom + o.PriAsset)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo + o.SubAReturn) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom + o.SubAsset)),

                                cust = cust,
                                o.PriSale,
                                o.PriPurchase,
                                o.PriPReturn,
                                stockcheck = 1
                            }).OrderBy(b => b.text).ToList();
                if (constat == "SalesEntry" && StockItemsPerm == true)
                {
                    data = data.Where(a => a.KeepStock == false || (a.KeepStock == true && a.total > 0)).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    data = data.Where(a => a.total > 0).Skip(skip).Take(pageSize).ToList();
                }

                return Json(data);
            }




        }
        public JsonResult SearchdetailsMCSP(string q, string x, long? cust, long? mc, string constat)
        {
            IEnumerable<StockDetailssp> data = new List<StockDetailssp>();
            q = q.Replace(" ", "%");
            if (q.Length >= 0 && q.Length < 3)
            {
                return Json(data);

            }
            var selitem = new SqlParameter("@ItemId", "0");
            var selitemname = new SqlParameter("@Itemname", q);
            var selmc = new SqlParameter("@MCId", mc);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "0");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");



            var datadd = db.Database.SqlQueryDedup<StockDetailssp>("SP_ITEMSEARCH @ItemId,@Itemname,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selitemname, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            for (int i = 0; i < datadd.Count(); i++)
            {
               // GetItemPurchasePrice
                    SalesReportController salesreport = new SalesReportController();
                
                var urprice = com.GetItemPurchasePriceold((long)datadd[i].ItemID, System.DateTime.Now, mc);
                
                var sellingprice = datadd[i].SellingPrice;// ((urprice < datadd[i].SellingPrice) ? datadd[i].SellingPrice : urprice * (decimal)1.05);

                var it = db.Items.Find(datadd[i].ItemID);
                datadd[i].total = datadd[i].TotalQty;
                datadd[i].price = sellingprice;// datadd[i].SellingPrice;
                datadd[i].cost = datadd[i].PurchasePrice;
                datadd[i].PriUnit = datadd[i].ItemUnitName;
                datadd[i].SubUnit = datadd[i].SubUnitName;
                datadd[i].cashprice = sellingprice;// (it.cashprice == null) ? datadd[i].SellingPrice : it.cashprice;
                datadd[i].creditprice = sellingprice;// (it.creditprice == null) ? datadd[i].SellingPrice : it.creditprice;
                datadd[i].id = datadd[i].ItemID;
                datadd[i].stockcheck = datadd[i].stockcheck;
                //datadd[i].lastPur =
                long itid = it.ItemID;

                if (ItemlastPurchasePrice == true)
                {
                    datadd[i].lastPur = urprice;
                }
                if (cust != null)
                {
                    if (ItemlastSalesPrice == true)
                    {
                        datadd[i].lastSale = db.Database.SqlQueryRaw<decimal>(@"select top 1 ItemUnitPrice from SEItems a
inner join SalesEntries b on a.SalesEntry=b.SalesEntryId  where sedate>'2024-01-01' and 
item='" + datadd[i].ItemID + "' and b.Customer='" + cust + "' order by SalesEntry desc").AsEnumerable().FirstOrDefault();

                    }
                }

                var dt = System.DateTime.Now.Date;
                var ifbill = db.BillOfMaterialsoffers.Any(o => o.ItemId == itid && o.BOMDateStart <= dt && o.BOMDateEnd >= dt);

                bool available = ifbill;
                if (ifbill)
                {

                    var bomid = db.BillOfMaterialsoffers.Where(o => o.ItemId == itid && o.BOMDateStart <= dt && o.BOMDateEnd >= dt).Select(o => o.BOMOfferId).FirstOrDefault();
                    var reqitem = db.BOMItemsoffers.Where(o => o.BOMOfferId == bomid).ToList();
                    if (bomid > 0 && reqitem.Count() > 0)
                    {
                        foreach (var reqit in reqitem)
                        {
                            if (reqit.Quantity > GetItemWisestock(reqit.ItemId, mc))
                            {
                                available = false;
                            }
                        }
                        if (available && datadd[i].total <= 0)
                        {
                            datadd[i].total = 1;
                        }
                        if (ItemlastPurchasePrice == true)
                        {
                            datadd[i].lastPur = db.BillOfMaterialsoffers.Where(o => o.BOMOfferId == bomid).Select(o => (decimal)o.Price).FirstOrDefault();
                        }
                    }
                }



            }
            data = datadd.ToList();
            return Json(data);


        }



        public JsonResult SearchdetailsMCSP2(string q, string x, long? cust, long? mc, string constat)
        {
            q = q.Replace(" ", "%");
            var selitem = new SqlParameter("@ItemId", "0");
            var selitemname = new SqlParameter("@Itemname", q);
            var selmc = new SqlParameter("@MCId", mc);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "0");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            IEnumerable<StockDetailssp> data = new List<StockDetailssp>();

            var datadd = db.Database.SqlQueryDedup<StockDetailssp>("SP_ITEMSEARCH2 @ItemId,@Itemname,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selitemname, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();


            for (int i = 0; i < datadd.Count(); i++)
            {
                var it = db.Items.Find(datadd[i].ItemID);
                datadd[i].total = datadd[i].TotalQty;
                datadd[i].price = datadd[i].SellingPrice;
                datadd[i].cost = datadd[i].PurchasePrice;
                datadd[i].PriUnit = datadd[i].ItemUnitName;
                datadd[i].SubUnit = datadd[i].SubUnitName;
                datadd[i].cashprice = (it.cashprice == null) ? datadd[i].SellingPrice : it.cashprice;
                datadd[i].creditprice = (it.creditprice == null) ? datadd[i].SellingPrice : it.creditprice;

                datadd[i].id = datadd[i].ItemID;
                datadd[i].stockcheck = datadd[i].stockcheck;
                //datadd[i].lastPur =
                datadd[i].lastPur = db.Database.SqlQueryRaw<decimal>(@"select top 1 ItemUnitPrice from PEItems where item=" + datadd[i].ItemID + " order by PurchaseEntry desc").AsEnumerable().FirstOrDefault();
                if (cust != null)
                {
                    datadd[i].lastSale = db.Database.SqlQueryRaw<decimal>(@"select top 1 ItemUnitPrice from SEItems a
inner join SalesEntries b on a.SalesEntry=b.SalesEntryId  where
item='" + datadd[i].ItemID + "' and b.Customer='" + cust + "' order by SalesEntry desc").AsEnumerable().FirstOrDefault();
                }




            }
            data = datadd.Where(o => (o.total > 0)).ToList();
            return Json(data);


        }


        public string GetItemWisestock2(long? itemid, long? ddmc)
        {


            var selitem = new SqlParameter("@ItemId", itemid);
            var selmc = new SqlParameter("@MCId", ddmc);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

            if (data.Count > 0)
                return data[0].ITotalQty.ToString();
            else
                return "0";



        }
        public decimal? GetItemWisestock(long? itemid, long? ddmc)
        {


            var selitem = new SqlParameter("@ItemId", itemid);
            var selmc = new SqlParameter("@MCId", ddmc);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

            if (data.Count > 0)
                return data[0].ITotalQty;
            else
                return 0;



        }
        public JsonResult SearchdetailsMC(string q, string x, long? cust, long? mc, string constat)
        {

            var StockItemsPerm = User.IsInRole("List Stockable Items");

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var stockcheckinvoice = db.EnableSettings.Where(a => a.EnableType == "stockcheckinvoice").FirstOrDefault();
            var stockcheck = stockcheckinvoice != null ? stockcheckinvoice.Status : Status.inactive;
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
            var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 10;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;
            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            if (stockcheck == Status.inactive)
            {
                var item1 = (from b in db.Items
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                             from e in cat.DefaultIfEmpty()
                             join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                             from f in taxss.DefaultIfEmpty()
                             join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                             equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                             from g in pur.DefaultIfEmpty()
                             join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                             equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                             from h in sale.DefaultIfEmpty()

                             where (b.Status == Status.active && (check == true ||

                             b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || (b.PartNumber.ToLower().Contains(q.ToLower()) || (b.PartNumber.Contains(q))
                             && PartNoCheck == Status.active))
                             && (secnd == "" || b.ItemName.ToLower().Contains(secnd.ToLower()))
                             && (third == "" || b.ItemName.ToLower().Contains(third.ToLower()))

                             )

                             select new
                             {
                                 text = b.ItemCode + " - " + b.ItemName,
                                 id = b.ItemID,
                                 b.SellingPrice,
                                 b.PurchasePrice,
                                 b.BasePrice,
                                 b.MRP,
                                 b.MinStock,
                                 b.KeepStock,
                                 Tax = f.Percentage,
                                 b.ItemUnitID,
                                 g = h,
                                 PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                                 lastSale = (decimal?)h.ItemUnitPrice,
                                 lastSaleU = (decimal?)h.ItemUnit,

                                 lastPur = (decimal?)g.ItemUnitPrice,
                                 lastPurU = (decimal?)g.ItemUnit,

                                 b.ItemCode,
                                 b.ItemName,
                                 b.ItemArabic,
                                 b.SubUnitId,
                                 PriUnit = c.ItemUnitName,
                                 SubUnit = d.ItemUnitName,
                                 ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                                 b.Barcode,

                                 OpeningStock = ((mc == 0 || mc == 1 || mc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,

                                 PriPurchase = (decimal?)(from v in db.PEItemss
                                                          join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                          where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.ItemQuantity
                                                          }).Sum(c => c.ItemQuantity) ?? 0,

                                 SubPurchase = (decimal?)(from v in db.PEItemss
                                                          join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                          where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.ItemQuantity
                                                          }).Sum(c => c.ItemQuantity) ?? 0,

                                 PriAsset = (decimal?)(from v in db.AssetTransferDetails
                                                       join w in db.AssetTransferMasters on v.AssetEntryId equals w.AssetEntryId
                                                       where v.RefItemId == b.ItemID
                                                       && (mc == 0 || mc == w.McFromId) && v.UnitId == b.ItemUnitID
                                                       select new
                                                       {
                                                           v.Quantity
                                                       }).Sum(c => c.Quantity) ?? 0,

                                 SubAsset = (decimal?)(from v in db.AssetTransferDetails
                                                       join w in db.AssetTransferMasters on v.AssetEntryId equals w.AssetEntryId
                                                       where v.RefItemId == b.ItemID && v.UnitId == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                       && (mc == 0 || mc == w.McFromId)

                                                       select new
                                                       {
                                                           v.Quantity
                                                       }).Sum(c => c.Quantity) ?? 0,
                                 PriAReturn = (decimal?)(from v in db.AssetToInventoryDetails
                                                         join w in db.AssetToInventoryMasters on v.EntryId equals w.EntryId
                                                         where v.RefItemId == b.ItemID && v.UnitId == b.ItemUnitID
                                                         && (mc == 0 || mc == w.McFromId)

                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(c => c.Quantity) ?? 0,

                                 SubAReturn = (decimal?)(from v in db.AssetToInventoryDetails
                                                         join w in db.AssetToInventoryMasters on v.EntryId equals w.EntryId
                                                         where v.RefItemId == b.ItemID && v.UnitId == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                         && (mc == 0 || mc == w.McFromId)

                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(c => c.Quantity) ?? 0,


                                 PriSale = (decimal?)(from v in db.SEItemss
                                                      join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                      where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      && (w.SaleType != SaleType.Hire)
                                                      select new
                                                      {
                                                          v.ItemQuantity
                                                      }).Sum(c => c.ItemQuantity) ?? 0,

                                 SubSale = (decimal?)(from v in db.SEItemss
                                                      join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                      where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      && (w.SaleType != SaleType.Hire)
                                                      select new
                                                      {
                                                          v.ItemQuantity
                                                      }).Sum(c => c.ItemQuantity) ?? 0,

                                 PriPReturn = (decimal?)(from v in db.PRItemss
                                                         join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,

                                 SubPReturn = (decimal?)(from v in db.PRItemss
                                                         join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,

                                 PriSReturn = (decimal?)(from v in db.SRItemss
                                                         join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         && (w.SaleType != SaleType.Hire)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,

                                 SubSReturn = (decimal?)(from v in db.SRItemss
                                                         join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         && (w.SaleType != SaleType.Hire)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,

                                 //stock adjustment---
                                 PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                                 SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,

                                 PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                                 subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                                 //-------
                                 // production ----
                                 PriProdItem = (decimal?)(from v in db.GeneratedItem
                                                          join w in db.Productions on v.Production equals w.ProductionId
                                                          where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.Qty
                                                          }).Sum(c => c.Qty) ?? 0,
                                 SubProdItem = (decimal?)(from v in db.GeneratedItem
                                                          join w in db.Productions on v.Production equals w.ProductionId
                                                          where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.Qty
                                                          }).Sum(c => c.Qty) ?? 0,
                                 // compined item
                                 PriProdCItem = (decimal?)(from v in db.ProItems
                                                           join w in db.Productions on v.Production equals w.ProductionId
                                                           where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                           && (mc == 0 || mc == w.MaterialCenter)
                                                           select new
                                                           {
                                                               v.Quantity
                                                           }).Sum(c => c.Quantity) ?? 0,

                                 SubProdCItem = (decimal?)(from v in db.ProItems
                                                           join w in db.Productions on v.Production equals w.ProductionId
                                                           where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                           && (mc == 0 || mc == w.MaterialCenter)
                                                           select new
                                                           {
                                                               v.Quantity
                                                           }).Sum(c => c.Quantity) ?? 0,

                                 // main item
                                 PriUnItem = (decimal?)(from v in db.ConsumedItem
                                                        join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                        where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                        && (mc == 0 || mc == w.MaterialCenter)
                                                        select new
                                                        {
                                                            v.Qty
                                                        }).Sum(c => c.Qty) ?? 0,
                                 SubUnItem = (decimal?)(from v in db.ConsumedItem
                                                        join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                        where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                        && (mc == 0 || mc == w.MaterialCenter)
                                                        select new
                                                        {
                                                            v.Qty
                                                        }).Sum(c => c.Qty) ?? 0,
                                 // compined item
                                 PriUnCItem = (decimal?)(from v in db.UnassembleItems
                                                         join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                         where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(c => c.Quantity) ?? 0,

                                 SubUnCItem = (decimal?)(from v in db.UnassembleItems
                                                         join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                         where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(c => c.Quantity) ?? 0,
                                 // stock transfer


                             }).Distinct();
                var vd = item1.ToList();
                if (constat != "SalesEntry" || StockItemsPerm != true)
                {

                }
                else
                {
                    vd = item1.ToList();
                }
                vd = item1.OrderBy(b => b.ItemName).Skip(skip).Take(15).ToList();
                var data = (from o in vd
                            let PriStTrFrom = (decimal?)(from v in db.StockTransferItems
                                                         join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                         where (v.Item == o.id && v.Unit == o.ItemUnitID)
                                                         && (mc == 0 || mc == w.MCFrom)
                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(m => m.Quantity) ?? 0
                            let PriStTrTo = (decimal?)(from v in db.StockTransferItems
                                                       join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                       where (v.Item == o.id && v.Unit == o.ItemUnitID)
                                                       && (mc == w.MCTo)
                                                       select new
                                                       {
                                                           v.Quantity
                                                       }).Sum(m => m.Quantity) ?? 0
                            let SubStTrFrom = (decimal?)(from v in db.StockTransferItems
                                                         join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                         where (v.Item == o.id && v.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                                         && (mc == w.MCFrom)
                                                         select new
                                                         {
                                                             v.Quantity
                                                         }).Sum(m => m.Quantity) ?? 0
                            let SubStTrTo = (decimal?)(from v in db.StockTransferItems
                                                       join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                       where (v.Item == o.id && v.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                                       && (mc == w.MCTo)
                                                       select new
                                                       {
                                                           v.Quantity
                                                       }).Sum(m => m.Quantity) ?? 0



                            select new
                            {
                                o.text,
                                o.id,
                                unit = o.ItemUnitID,
                                price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                                cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                                o.KeepStock,
                                MinStock = (o.MinStock != null) ? o.MinStock : 0,
                                o.Tax,
                                lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
                                lastSaleU = o.lastSaleU,
                                lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
                                lastPurU = o.lastPurU,
                                o.g,

                                o.ItemCode,
                                o.ItemName,
                                o.ItemArabic,
                                o.Barcode,
                                o.SubUnitId,
                                PartNumber = o.PartNumber,
                                PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                o.ConFactor,


                                pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo + o.PriAReturn) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom + o.PriAsset)),
                                subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo + o.SubAReturn) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom + o.SubAsset)),
                                total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo + o.PriAReturn) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom + o.PriAsset)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo + o.SubAReturn + o.SubAReturn) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom + o.SubAsset)),

                                cust = cust,
                                o.PriSale,
                                o.PriPurchase,
                                o.PriPReturn,
                                stockcheck = 1
                            }).OrderBy(b => b.text).ToList();
                if (constat == "SalesEntry" && StockItemsPerm == true)
                {
                    data = data.Where(a => a.KeepStock == false || (a.KeepStock == true && a.total > 0)).Skip(skip).Take(pageSize).ToList();
                }

                return Json(data);
            }
            else
            {
                var item2 = (from b in db.Items
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                             from e in cat.DefaultIfEmpty()
                             join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                             from f in taxss.DefaultIfEmpty()
                             join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                             equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                             from g in pur.DefaultIfEmpty()
                             join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                             equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                             from h in sale.DefaultIfEmpty()
                             where (b.Status == Status.active && (check == true ||

                             b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || (b.PartNumber.ToLower().Contains(q.ToLower()) || (b.PartNumber.Contains(q))
                             && PartNoCheck == Status.active))
                             && (secnd == "" || b.ItemName.ToLower().Contains(secnd.ToLower()))
                             && (third == "" || b.ItemName.ToLower().Contains(third.ToLower())))
                             select new
                             {
                                 text = b.ItemCode + " - " + b.ItemName,
                                 id = b.ItemID,
                                 b.SellingPrice,
                                 b.PurchasePrice,
                                 b.BasePrice,
                                 b.MRP,
                                 b.MinStock,
                                 b.KeepStock,
                                 Tax = f.Percentage,
                                 b.ItemUnitID,
                                 g = h,
                                 PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                                 lastSale = (decimal?)h.ItemUnitPrice,
                                 lastSaleU = (decimal?)h.ItemUnit,

                                 lastPur = (decimal?)g.ItemUnitPrice,
                                 lastPurU = (decimal?)g.ItemUnit,

                                 b.ItemCode,
                                 b.ItemName,
                                 b.ItemArabic,
                                 b.SubUnitId,
                                 PriUnit = c.ItemUnitName,
                                 SubUnit = d.ItemUnitName,
                                 ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                                 b.Barcode,

                                 OpeningStock = 0,

                                 PriPurchase = 0,

                                 SubPurchase = 0,

                                 PriSale = 0,

                                 SubSale = 0,

                                 PriPReturn = 0,

                                 SubPReturn = 0,

                                 PriSReturn = 0,

                                 SubSReturn = 0,

                                 //stock adjustment---
                                 PriAddAdj = 0,
                                 SubAddAdj = 0,

                                 PriLessAdj = 0,
                                 subLessAdj = 0,
                                 //-------
                                 // production ----
                                 PriProdItem = 0,
                                 SubProdItem = 0,
                                 // compined item
                                 PriProdCItem = 0,

                                 SubProdCItem = 0,

                                 // main item
                                 PriUnItem = 0,
                                 SubUnItem = 0,
                                 // compined item
                                 PriUnCItem = 0,

                                 SubUnCItem = 0,
                                 // stock transfer


                             }).Distinct();
                var vd = item2.OrderBy(b => b.ItemName).Skip(skip).Take(pageSize).ToList();
                if (constat != "SalesEntry" || StockItemsPerm != true)
                {

                }
                else
                {
                    vd = item2.OrderBy(b => b.ItemName).ToList();
                }
                var data = (from o in vd
                            let PriStTrFrom = 0
                            let PriStTrTo = 0
                            let SubStTrFrom = 0
                            let SubStTrTo = 0



                            select new
                            {
                                o.text,
                                o.id,
                                unit = o.ItemUnitID,
                                price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                                cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                                o.KeepStock,
                                MinStock = (o.MinStock != null) ? o.MinStock : 0,
                                o.Tax,
                                lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
                                lastSaleU = o.lastSaleU,
                                lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
                                lastPurU = o.lastPurU,
                                o.g,

                                o.ItemCode,
                                o.ItemName,
                                o.ItemArabic,
                                o.Barcode,
                                o.SubUnitId,
                                PartNumber = o.PartNumber,
                                PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                o.ConFactor,


                                pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom)),
                                subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom)),
                                total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom)),

                                cust = cust,
                                o.PriSale,
                                o.PriPurchase,
                                o.PriPReturn,
                                stockcheck = 0
                            }).OrderBy(b => b.text).ToList();
                if (constat == "SalesEntry" && StockItemsPerm == true)
                {
                    data = data.Where(a => a.KeepStock == false || (a.KeepStock == true)).Skip(skip).Take(pageSize).ToList();
                }

                return Json(data);
            }



        }
        public JsonResult SearchBarcode(string q)
        {

            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Items.Where(p => (p.Status == Status.active) && (p.Barcode.ToLower().Contains(q.ToLower()) || p.Barcode.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Status == Status.active).Select(b => new SelectFormat
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID
                }).OrderBy(b => b.text).ToList();

            }
            return Json(serialisedJson);
        }
        public JsonResult GetByBarcode(string q)
        {

            List<SelectFormat> serialisedJson;
            serialisedJson = db.Items.Where(p => (p.Status == Status.active) && (p.Barcode == q))
                                 .Select(b => new SelectFormat
                                 {
                                     text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID
                                 })
                                 .OrderBy(b => b.text).ToList();
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Items.Where(p => (p.Status == Status.active) && (p.Barcode == q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
             if (serialisedJson.Count() < 1&&q.Length>5)
            {
                var barcode = q.Substring(0, 7);
                var balance = q.Replace(barcode, "");
                serialisedJson = db.Items.Where(p => (p.Status == Status.active) && p.Barcode.Contains(barcode))
                                   .Select(b => new SelectFormat
                                   {
                                       text = balance, //each json object will have 
                                       id = b.ItemID
                                   })
                                   .OrderBy(b => b.text).ToList();
            }

            else
            {if (serialisedJson.Count() < 1)
                {
                    serialisedJson = null;
                }

            }
            return Json(serialisedJson);
        }

        [HttpGet]
        public decimal GetSelingPrice(long ItemId)
        {
            var data = db.Items.Where(b => b.ItemType == 1 && b.Status == Status.active && b.ItemID == ItemId).Select(b => new
            {

                sprice = b.SellingPrice
            }).FirstOrDefault();
            return data.sprice;
        }

        [HttpGet]
        public decimal? GetCashPrice(long ItemId)
        {
            var data = db.Items.Where(b => b.ItemType == 1 && b.Status == Status.active && b.ItemID == ItemId).Select(b => new
            {

                cash = (decimal?)((b.cashprice != null) ? b.cashprice : b.SellingPrice)
            }).FirstOrDefault();
            string formattedCashprice = data.cash.Value.ToString("0.00");
            decimal? cashprice = Convert.ToDecimal(formattedCashprice);
            return cashprice;
        }
        [HttpGet]
        public decimal? GetCreditPrice(long ItemId)
        {
            var data = db.Items.Where(b => b.ItemType == 1 && b.Status == Status.active && b.ItemID == ItemId).Select(b => new
            {

                credit = (decimal?)((b.creditprice != null) ? b.creditprice : b.SellingPrice)
            }).FirstOrDefault();
            string formattedCreditprice = data.credit.Value.ToString("0.00");
            decimal? creditprice = Convert.ToDecimal(formattedCreditprice);
            return creditprice;
        }
        [HttpGet]
        public decimal getpurprice(long ItemId)
        {
            var data = db.Items.Where(b => b.ItemType == 1 && b.Status == Status.active && b.ItemID == ItemId).Select(b => new
            {

                pprice = b.PurchasePrice
            }).FirstOrDefault();
            return data.pprice;
        }
        [HttpGet]
        public string GetItemByIdstring(long ItemId)
        {
            var data = db.Items.Where(b => b.ItemType == 1 && b.Status == Status.active && b.ItemID == ItemId).Select(b => new
            {

                ItemWithCode = b.ItemCode + " - " + b.ItemName,
            }).FirstOrDefault();
            return data.ItemWithCode;
        }
        [HttpGet]
        public JsonResult GetItemById(long ItemId)
        {
            var data = db.Items.Where(b => b.ItemType == 1 && b.Status == Status.active && b.ItemID == ItemId).Select(b => new
            {
                b.ItemID,
                b.ItemCode,
                b.ItemName,
                ItemWithCode = b.ItemCode + " - " + b.ItemName,
                b.ItemArabic,
                b.Barcode,
                b.ItemDescription,
                b.SellingPrice,
                b.PurchasePrice,
                b.MRP,
                b.BasePrice,
                b.Status,
                b.KeepStock,
                b.ItemUnit,
                b.ItemUnitID,
                b.SubUnitId,
                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                Category = db.ItemCategorys.Where(a => a.ItemCategoryID == b.ItemCategoryID).Select(a => a.ItemCategoryName).FirstOrDefault(),
                Brand = db.ItemBrands.Where(a => a.ItemBrandID == b.ItemBrandID).Select(a => a.ItemBrandName).FirstOrDefault(),
                Color = db.ItemColors.Where(a => a.ItemColorID == b.ItemColorID).Select(a => a.ItemColorName).FirstOrDefault(),
                Tax = db.Taxs.Where(a => a.TaxID == b.TaxID).Select(a => a.Percentage).FirstOrDefault(),
                Size = db.ItemSizes.Where(a => a.ItemSizeID == b.ItemSizeID).Select(a => a.ItemSizeName).FirstOrDefault(),
                PriUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnitID).Select(a => a.ItemUnitName).FirstOrDefault(),
                SubUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.SubUnitId).Select(a => a.ItemUnitName).FirstOrDefault()

            }).ToList();
            return Json(data);

        }

        [HttpGet]
        public ActionResult GetItemEdit(string itId)
        {
            var itid = Convert.ToInt64(itId);
            var PEItem = (from b in db.Items
                          join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                          from c in primary.DefaultIfEmpty()
                          join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                          from d in second.DefaultIfEmpty()
                          where b.ItemID == itid
                          select new
                          {
                              ItemCode = b.ItemCode,
                              ItemName = b.ItemName,
                              ItemWithCode = b.ItemCode + " - " + b.ItemName,
                              b.ItemUnitID,
                              b.SubUnitId,
                              PriUnit = c.ItemUnitName,
                              SubUnit = d.ItemUnitName,
                              b.BasePrice,
                              b.SellingPrice,
                              b.PurchasePrice,
                              b.MRP,
                              b.KeepStock,
                              b.ConFactor,
                              batch = (from ay in db.BatchStocks
                                       join az in db.Items on new { f1 = ay.Item, f4 = ay.Type }
                                            equals new { f1 = az.ItemID, f4 = "Opening" }
                                       where ay.Item == b.ItemID
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
                                           origin = "Opening",
                                           Order = ay.Order
                                       }).ToList()
                          }).FirstOrDefault();
            return Json(PEItem);
        }
        public ActionResult GetItemall(string itemcontain)
        {
            var ItemStockOut = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var CheckItemStock = ItemStockOut != null ? ItemStockOut.Status : Status.inactive;
            ViewBag.StockCheck = CheckItemStock;
            var uDev = User.IsInRole("Dev");
            var uItemView = User.IsInRole("View Item");
            var uEdit = User.IsInRole("Edit Item");
            var uDelete = User.IsInRole("Delete Item");
            var overridecost = User.IsInRole("Override Selling Price Below Cost");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");
            long? Catid = null;

            long? priceper = null;
            bool PrcCategory = true;
            bool res = true;
            //fetch pricecategory
        

            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.Jewellerys on b.ItemID equals g.Item into jewl
                        from g in jewl.DefaultIfEmpty()
                        join j in db.HrItems on b.ItemID equals j.Item into Hr
                        from j in Hr.DefaultIfEmpty()
                        join i in db.HireReturns on j.Hr equals i.HireReturnId into Return
                        from i in Return.DefaultIfEmpty()
                        join k in db.ItemImages on b.ItemID equals k.ItemID into itmimg
                        from k in itmimg.DefaultIfEmpty()
                        join l in db.ItemBundles on b.ItemID equals l.mainItem into bundle
                        from l in bundle.DefaultIfEmpty()
                        where (b.ItemCode.Contains(itemcontain)||b.ItemName.Contains(itemcontain))
                        select new
                        {
                            b.ItemCode,
                            b.ItemSizes.ItemSizeName,
                            b.ItemColors.ItemColorName,
                            b.ItemName,
                            b.ItemArabic,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            PartNumber = b.PartNumber,
                            Tax = f.Percentage,
                            SellingPrice = b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.slreq,
                            b.accmap,
                            b.Barcode,
                            b.SupplierRef,
                            k.FileName,
                            PricingStrategy = b.lockprice,
                            OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,

                            PriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,
                            // unassemble -----
                            // main item
                            PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,

                            g.TagLine1,
                            g.TagLine2,
                            g.TagLine3,
                            g.TagLine4,
                            g.TagLine5,

                            pricepersenage = priceper,
                            ItemBundleId = (l.ItemBundleId == null) ? 0 : l.ItemBundleId
                        }).Distinct().AsEnumerable().Select(o => new
                        {
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            size=o.ItemSizeName,
                            color=o.ItemColorName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.FileName,
                            o.PricingStrategy,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,

                            SellingPrice = (res == true) ? o.SellingPrice : (o.SellingPrice + (o.SellingPrice) * (priceper / 100)),
                            o.PurchasePrice,
                            BasePrice = (overridecost == true) ? 0 : o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.slreq,
                            o.Barcode,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),


                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),


                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            o.PriAddAdj,
                            o.SubAddAdj,
                            o.PriLessAdj,
                            o.subLessAdj,

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),

                            TagLine1 = o.TagLine1,
                            TagLine2 = o.TagLine2,
                            TagLine3 = o.TagLine3,
                            TagLine4 = o.TagLine4,
                            TagLine5 = o.TagLine5,
                            CheckStock = CheckItemStock,
                            o.SupplierRef,
                            lastPur = ItemlastPurchasePrice,
                            Dev = uDev,
                            Details = uItemView,
                            Edit = uEdit,
                            Delete = uDelete,
                            o.ItemBundleId,
                        });
            return Json(item);
        }

        public ActionResult GetItem(int itemID, long? Categoryid, long? Customerid, long? mc = 0, int? SalType = 0, int? HireType = 0, long? Invoice = 0)
        {
            var ItemStockOut = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var CheckItemStock = ItemStockOut != null ? ItemStockOut.Status : Status.inactive;
            ViewBag.StockCheck = CheckItemStock;
            var uDev = User.IsInRole("Dev");
            var uItemView = User.IsInRole("View Item");
            var uEdit = User.IsInRole("Edit Item");
            var uDelete = User.IsInRole("Delete Item");
            var overridecost = User.IsInRole("Override Selling Price Below Cost");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");
            long? Catid = null;

            long? priceper = null;
            bool PrcCategory = true;
            bool res = true;
            //fetch pricecategory
            if (Customerid != null)
            {
                Catid = db.Customers.Where(o => o.CustomerID == Customerid).Select(o => o.Category).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault();
                priceper = db.PriceCategoryPercentages.Where(o => o.Category == Catid).Select(o => o.Percentage).AsEnumerable().DefaultIfEmpty(0).FirstOrDefault();

                PrcCategory = db.PriceCategoryPercentages.Where(o => o.Category == Catid).Any(c => c.PriceCategory == "SalesPrice");
                res = (PrcCategory) ? true : false;


            }

            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.Jewellerys on b.ItemID equals g.Item into jewl
                        from g in jewl.DefaultIfEmpty()
                        join j in db.HrItems on b.ItemID equals j.Item into Hr
                        from j in Hr.DefaultIfEmpty()
                        join i in db.HireReturns on j.Hr equals i.HireReturnId into Return
                        from i in Return.DefaultIfEmpty()
                        join k in db.ItemImages on b.ItemID equals k.ItemID into itmimg
                        from k in itmimg.DefaultIfEmpty()
                        join l in db.ItemBundles on b.ItemID equals l.mainItem into bundle
                        from l in bundle.DefaultIfEmpty()
                        let hir = db.HireRates.Where(x => x.ItemId == itemID && x.type == HireType).Select(y => y.Rate).FirstOrDefault()
                        where (b.ItemID == itemID) && (Invoice == 0 || j.Item == itemID)
                        select new
                        {
                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            b.ItemDescription,

                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            PartNumber = b.PartNumber,
                            Tax = f.Percentage,
                            SellingPrice = (SalType != 2) ? b.SellingPrice : hir,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.slreq,
                            b.accmap,
                            b.Barcode,
                            b.SupplierRef,
                            k.FileName,
                            PricingStrategy = b.lockprice,
                            color=b.ItemColors.ItemColorName,
                            size=b.ItemSizes.ItemSizeName,
                            OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,

                            PriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,
                            // unassemble -----
                            // main item
                            PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,

                            g.TagLine1,
                            g.TagLine2,
                            g.TagLine3,
                            g.TagLine4,
                            g.TagLine5,

                            pricepersenage = priceper,
                            ItemBundleId = (l.ItemBundleId == null) ? 0 : l.ItemBundleId
                        }).Distinct().AsEnumerable().Select(o => new
                        {
                            o.ItemID,
                            o.ItemCode,
                            o.ItemDescription,
                            o.ItemName,
                            o.ItemArabic,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.FileName,
                            o.PricingStrategy,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.color,
                            o.size,

                            SellingPrice = (res == true) ? o.SellingPrice : (o.SellingPrice + (o.SellingPrice) * (priceper / 100)),
                            o.PurchasePrice,
                            BasePrice = (overridecost == true) ? 0 : o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.slreq,
                            o.Barcode,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),


                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),


                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            o.PriAddAdj,
                            o.SubAddAdj,
                            o.PriLessAdj,
                            o.subLessAdj,

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),

                            TagLine1 = o.TagLine1,
                            TagLine2 = o.TagLine2,
                            TagLine3 = o.TagLine3,
                            TagLine4 = o.TagLine4,
                            TagLine5 = o.TagLine5,
                            CheckStock = CheckItemStock,
                            o.SupplierRef,
                            lastPur = ItemlastPurchasePrice,
                            Dev = uDev,
                            Details = uItemView,
                            Edit = uEdit,
                            Delete = uDelete,
                            o.ItemBundleId,
                        }).FirstOrDefault();
            return Json(item);
        }

        public ActionResult GetItemMC(int itemID, long? mc,long? customerid)
        {
            decimal perc = 1;
            if(customerid!=null)
            {
                var percentage = (from a in db.Customers
                                  join b in db.PriceCategoryPercentages on a.Category equals b.Category
                                  where a.CustomerID ==customerid
                                  select new
                                  {
                                      b.Percentage
                                  }).Select(o=>o.Percentage).FirstOrDefault();
                if(percentage!=null)

                {
                    perc = 1+(decimal)percentage/100;
                }
            }
            var overridecost = User.IsInRole("Override Selling Price Below Cost");
            var stocktransfernotreadonly = User.IsInRole("Stock Transfer Price Make Not Read Only");
            var ItemStockOut = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var CheckItemStock = ItemStockOut != null ? ItemStockOut.Status : Status.inactive;
            ViewBag.StockCheck = CheckItemStock;
            var mcminstock = db.EnableSettings.Where(o => o.EnableType == "materialcentrewiseminstock").SingleOrDefault();
            var mcminstockwise = (mcminstock != null) ? mcminstock.Status : Status.inactive;
            var userid = User.Identity.GetUserId();

            var empid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            var keydaya = db.keytableviews.Where(o => o.employeeid == empid).ToList().Select(o => new
            {
                o.entrytime,
                o.expire,
                o.keyvalue,
                dt = o.entrytime.AddMinutes(o.expire),
            }).OrderByDescending(o => o.dt).FirstOrDefault();
            bool validkey = false;
            string keycode = "";
            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.Jewellerys on b.ItemID equals g.Item into jewl
                        from g in jewl.DefaultIfEmpty()

                        where b.ItemID == itemID
                        select new
                        {
                            id = b.ItemID,
                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            //b.MinStock,
                            categoryname = e.ItemCategoryName,
                            PartNumber = b.PartNumber,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            BasePrice = b.PurchasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.slreq,
                            b.Barcode,
                            b.SupplierRef,
                            PricingStrategy = b.lockprice,
                            OpeningStock = ((mc == 0 || mc == 1 || mc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,

                            MinStock = (mcminstockwise == Status.active && db.mcitemminstock.Where(o => o.ItemId == itemID && o.MCId == mc).FirstOrDefault() != null) ? db.mcitemminstock.Where(o => o.ItemId == itemID && o.MCId == mc).FirstOrDefault().minstock : b.MinStock,
                            //PriPurchase = (decimal?)(from v in db.PEItemss
                            //                         where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                            //                         && (mc == 0 || mc == w.MaterialCenter)
                            //                             v.ItemQuantity
                            //                         }).Sum(c => c.ItemQuantity) ?? 0,
                            //SubPurchase = (decimal?)(from v in db.PEItemss
                            //                         where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                            //                         && (mc == 0 || mc == w.MaterialCenter)
                            //                             v.ItemQuantity
                            //                         }).Sum(c => c.ItemQuantity) ?? 0,


                            //PriSale = (decimal?)(from v in db.SEItemss
                            //                     where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                            //                     && (mc == 0 || mc == w.MaterialCenter)
                            //                     && (w.SaleType != SaleType.Hire)
                            //                         v.ItemQuantity
                            //                     }).Sum(c => c.ItemQuantity) ?? 0,


                            //SubSale = (decimal?)(from v in db.SEItemss
                            //                     where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                            //                     && (mc == 0 || mc == w.MaterialCenter)
                            //                     && (w.SaleType != SaleType.Hire)
                            //                         v.ItemQuantity
                            //                     }).Sum(c => c.ItemQuantity) ?? 0,

                            //PriPReturn = (decimal?)(from v in db.PRItemss
                            //                        where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                            //                        && (mc == 0 || mc == w.MaterialCenter)
                            //                            v.ItemQuantity
                            //                        }).Sum(c => c.ItemQuantity) ?? 0,

                            //SubPReturn = (decimal?)(from v in db.PRItemss
                            //                        where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                            //                        && (mc == 0 || mc == w.MaterialCenter)
                            //                            v.ItemQuantity
                            //                        }).Sum(c => c.ItemQuantity) ?? 0,

                            //PriSReturn = (decimal?)(from v in db.SRItemss
                            //                        where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                            //                        && (mc == 0 || mc == w.MaterialCenter)
                            //                        && (w.SaleType != SaleType.Hire)
                            //                            v.ItemQuantity
                            //                        }).Sum(c => c.ItemQuantity) ?? 0,
                            //SubSReturn = (decimal?)(from v in db.SRItemss
                            //                        where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                            //                        && (mc == 0 || mc == w.MaterialCenter)
                            //                        && (w.SaleType != SaleType.Hire)
                            //                            v.ItemQuantity
                            //                        }).Sum(c => c.ItemQuantity) ?? 0,


                            ////stock adjustment---
                            //PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                            ////-------
                            //// production ----
                            //// main item
                            //PriProdItem = (decimal?)(from v in db.GeneratedItem
                            //                         where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                            //                         && (mc == 0 || mc == w.MaterialCenter)
                            //                             v.Qty
                            //                         }).Sum(c => c.Qty) ?? 0,
                            //SubProdItem = (decimal?)(from v in db.GeneratedItem
                            //                         where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                            //                         && (mc == 0 || mc == w.MaterialCenter)
                            //                             v.Qty
                            //                         }).Sum(c => c.Qty) ?? 0,

                            //// compined item
                            //PriProdCItem = (decimal?)(from v in db.ProItems
                            //                          where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                            //                          && (mc == 0 || mc == w.MaterialCenter)
                            //                              v.Quantity
                            //                          }).Sum(c => c.Quantity) ?? 0,

                            //SubProdCItem = (decimal?)(from v in db.ProItems
                            //                          where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                            //                          && (mc == 0 || mc == w.MaterialCenter)
                            //                              v.Quantity
                            //                          }).Sum(c => c.Quantity) ?? 0,

                            //// unassemble -----
                            //// main item
                            //PriUnItem = (decimal?)(from v in db.ConsumedItem
                            //                       where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                            //                       && (mc == 0 || mc == w.MaterialCenter)
                            //                           v.Qty
                            //                       }).Sum(c => c.Qty) ?? 0,
                            //SubUnItem = (decimal?)(from v in db.ConsumedItem
                            //                       where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                            //                       && (mc == 0 || mc == w.MaterialCenter)
                            //                           v.Qty
                            //                       }).Sum(c => c.Qty) ?? 0,
                            //// compined item
                            //PriUnCItem = (decimal?)(from v in db.UnassembleItems
                            //                        where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                            //                        && (mc == 0 || mc == w.MaterialCenter)
                            //                            v.Quantity
                            //                        }).Sum(c => c.Quantity) ?? 0,


                            //SubUnCItem = (decimal?)(from v in db.UnassembleItems
                            //                        where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                            //                        && (mc == 0 || mc == w.MaterialCenter)
                            //                            v.Quantity
                            //                        }).Sum(c => c.Quantity) ?? 0,
                            //// stock transfer
                            //PriStTrFrom = (decimal?)(from v in db.StockTransferItems
                            //                         where (v.Item == b.ItemID && v.Unit == b.ItemUnitID)
                            //                         && (mc == 0 || mc == w.MCFrom)
                            //                             v.Quantity
                            //                         }).Sum(m => m.Quantity) ?? 0,
                            //PriStTrTo = (decimal?)(from v in db.StockTransferItems
                            //                       where (v.Item == b.ItemID && v.Unit == b.ItemUnitID)
                            //                       && (mc == w.MCTo)
                            //                           v.Quantity
                            //                       }).Sum(m => m.Quantity) ?? 0,
                            //SubStTrFrom = (decimal?)(from v in db.StockTransferItems
                            //                         where (v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName)
                            //                         && (mc == w.MCFrom)
                            //                             v.Quantity
                            //                         }).Sum(m => m.Quantity) ?? 0,

                            //SubStTrTo = (decimal?)(from v in db.StockTransferItems
                            //                       where (v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName)
                            //                       && (mc == w.MCTo)
                            //                           v.Quantity
                            //                       }).Sum(m => m.Quantity) ?? 0,


                            g.TagLine1,
                            g.TagLine2,
                            g.TagLine3,
                            g.TagLine4,
                            g.TagLine5,


                        }).Distinct();

            var vd = item.ToList();
            var dt = System.DateTime.Now.Date;
            var ifbill = db.BillOfMaterialsoffers.Any(o => o.ItemId == itemID && o.BOMDateStart <= dt && o.BOMDateEnd >= dt);
            bool available = ifbill;
            decimal ifbuitemprice = 0;
            if (ifbill)
            {

                var bomid = db.BillOfMaterialsoffers.Where(o => o.ItemId == itemID && o.BOMDateStart <= dt && o.BOMDateEnd >= dt).Select(o => o.BOMOfferId).FirstOrDefault();

                var reqitem = db.BOMItemsoffers.Where(o => o.BOMOfferId == bomid).ToList();
                if (bomid > 0)
                {
                    foreach (var reqit in reqitem)
                    {
                        if (reqit.Quantity > GetItemWisestock(reqit.ItemId, mc))
                        {
                            available = false;
                        }
                    }
                    ifbuitemprice = db.BillOfMaterialsoffers.Where(o => o.BOMOfferId == bomid).Select(o => (decimal)o.Price).FirstOrDefault();

                }

            }
            var stocks = GetItemWisestock(itemID, mc);
            var purpirce = com.GetItemPurchasePriceold((long)itemID, System.DateTime.Now, mc);
            if (ifbuitemprice > 0)
            {
                purpirce = ifbuitemprice;
            }
            if (keydaya != null)
            {
                if (keydaya.dt > System.DateTime.Now)
                {
                    keycode = keydaya.keyvalue.Trim();
                    validkey = true;
                }
            }

            var data = (from o in vd
                            //            let PriStTrFrom = (decimal?)(from v in db.StockTransferItems
                            //                                         where (v.Item == o.id && v.Unit == o.ItemUnitID)
                            //                                         && (mc == 0 || mc == w.MCFrom)
                            //                                             v.Quantity
                            //                                         }).Sum(m => m.Quantity) ?? 0
                            //            let PriStTrTo = (decimal?)(from v in db.StockTransferItems
                            //                                       where (v.Item == o.id && v.Unit == o.ItemUnitID)
                            //                                       && (mc == w.MCTo)
                            //                                           v.Quantity
                            //                                       }).Sum(m => m.Quantity) ?? 0
                            //            let SubStTrFrom = (decimal?)(from v in db.StockTransferItems
                            //                                         where (v.Item == o.id && v.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                            //                                         && (mc == w.MCFrom)
                            //                                             v.Quantity
                            //                                         }).Sum(m => m.Quantity) ?? 0
                            //            let SubStTrTo = (decimal?)(from v in db.StockTransferItems
                            //                                       where (v.Item == o.id && v.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                            //                                       && (mc == w.MCTo)
                            //                                           v.Quantity
                            //                                       }).Sum(m => m.Quantity) ?? 0



                        select new
                        {
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            SellingPrice = (available == true) ? purpirce*perc : o.SellingPrice*perc,
                            PurchasePrice = (perc==1)?purpirce: o.SellingPrice * perc,
                            BasePrice = (perc == 1) ? ((overridecost == true) ? purpirce*perc : purpirce*perc): o.SellingPrice * perc,
                            //o.PurchasePrice,
                            //BasePrice=o.PurchasePrice,
                            o.MRP,
                            price = (available == true) ? purpirce*perc : ((o.SellingPrice != 0) ? o.SellingPrice*perc : o.MRP),
                            o.KeepStock,
                            o.slreq,
                            o.Barcode,

                            PriPurchase = 0, //(o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = 0,// (o.SubPurchase % o.ConFactor),


                            PriSale = 0,//(o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = 0,//(o.SubSale % o.ConFactor),

                            PriPReturn = 0,//(o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = 0,//(o.SubPReturn % o.ConFactor),


                            PriSReturn = 0,//(o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = 0,//(o.SubSReturn % o.ConFactor),

                            PriAddAdj = 0,
                            SubAddAdj = 0,
                            PriLessAdj = 0,
                            subLessAdj = 0,
                            PriStTrFrom = 0,
                            SubStTrFrom = 0,
                            PriStTrTo = 0,
                            SubStTrTo = 0,
                            stocktransfernotreadonly,
                            pritotal = stocks,//((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom)),
                            subtotal = stocks,//((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom)),
                            total = (available == true && stocks <= 0) ? 1 : stocks * o.ConFactor,// (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom)),

                            TagLine1 = o.TagLine1,
                            TagLine2 = o.TagLine2,
                            TagLine3 = o.TagLine3,
                            TagLine4 = o.TagLine4,
                            TagLine5 = o.TagLine5,
                            CheckStock = CheckItemStock,
                            o.SupplierRef,
                            o.PricingStrategy,
                            validkey,
                            keycode
                        }).FirstOrDefault();

            return Json(data);

        }


        public ActionResult GetItemMCbar(string itemID, long? mc)
        {
            var ItemStockOut = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var CheckItemStock = ItemStockOut != null ? ItemStockOut.Status : Status.inactive;
            ViewBag.StockCheck = CheckItemStock;

            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.Jewellerys on b.ItemID equals g.Item into jewl
                        from g in jewl.DefaultIfEmpty()
                        where b.Barcode == itemID
                        select new
                        {
                            id = b.ItemID,
                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            PartNumber = b.PartNumber,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.Barcode,
                            b.SupplierRef,

                            OpeningStock = ((mc == 0 || mc == 1 || mc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,


                            PriPurchase = (decimal?)(from v in db.PEItemss
                                                     join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                     where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.ItemQuantity
                                                     }).Sum(c => c.ItemQuantity) ?? 0,
                            SubPurchase = (decimal?)(from v in db.PEItemss
                                                     join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                     where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.ItemQuantity
                                                     }).Sum(c => c.ItemQuantity) ?? 0,


                            PriSale = (decimal?)(from v in db.SEItemss
                                                 join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                 where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                 && (mc == 0 || mc == w.MaterialCenter)
                                                 && (w.SaleType != SaleType.Hire)
                                                 select new
                                                 {
                                                     v.ItemQuantity
                                                 }).Sum(c => c.ItemQuantity) ?? 0,


                            SubSale = (decimal?)(from v in db.SEItemss
                                                 join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                 where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                 && (mc == 0 || mc == w.MaterialCenter)
                                                 && (w.SaleType != SaleType.Hire)
                                                 select new
                                                 {
                                                     v.ItemQuantity
                                                 }).Sum(c => c.ItemQuantity) ?? 0,

                            PriPReturn = (decimal?)(from v in db.PRItemss
                                                    join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            SubPReturn = (decimal?)(from v in db.PRItemss
                                                    join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            PriSReturn = (decimal?)(from v in db.SRItemss
                                                    join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    && (w.SaleType != SaleType.Hire)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,
                            SubSReturn = (decimal?)(from v in db.SRItemss
                                                    join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    && (w.SaleType != SaleType.Hire)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,


                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less && (mc == 0 || mc == a.MaterialCenter)).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)(from v in db.GeneratedItem
                                                     join w in db.Productions on v.Production equals w.ProductionId
                                                     where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.Qty
                                                     }).Sum(c => c.Qty) ?? 0,
                            SubProdItem = (decimal?)(from v in db.GeneratedItem
                                                     join w in db.Productions on v.Production equals w.ProductionId
                                                     where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.Qty
                                                     }).Sum(c => c.Qty) ?? 0,

                            // compined item
                            PriProdCItem = (decimal?)(from v in db.ProItems
                                                      join w in db.Productions on v.Production equals w.ProductionId
                                                      where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      select new
                                                      {
                                                          v.Quantity
                                                      }).Sum(c => c.Quantity) ?? 0,

                            SubProdCItem = (decimal?)(from v in db.ProItems
                                                      join w in db.Productions on v.Production equals w.ProductionId
                                                      where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      select new
                                                      {
                                                          v.Quantity
                                                      }).Sum(c => c.Quantity) ?? 0,

                            // unassemble -----
                            // main item
                            PriUnItem = (decimal?)(from v in db.ConsumedItem
                                                   join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                   where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                   && (mc == 0 || mc == w.MaterialCenter)
                                                   select new
                                                   {
                                                       v.Qty
                                                   }).Sum(c => c.Qty) ?? 0,
                            SubUnItem = (decimal?)(from v in db.ConsumedItem
                                                   join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                   where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                   && (mc == 0 || mc == w.MaterialCenter)
                                                   select new
                                                   {
                                                       v.Qty
                                                   }).Sum(c => c.Qty) ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)(from v in db.UnassembleItems
                                                    join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                    where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.Quantity
                                                    }).Sum(c => c.Quantity) ?? 0,


                            SubUnCItem = (decimal?)(from v in db.UnassembleItems
                                                    join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                    where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.Quantity
                                                    }).Sum(c => c.Quantity) ?? 0,
                            // stock transfer
                            //PriStTrFrom = (decimal?)(from v in db.StockTransferItems
                            //                         where (v.Item == b.ItemID && v.Unit == b.ItemUnitID)
                            //                         && (mc == 0 || mc == w.MCFrom)
                            //                             v.Quantity
                            //                         }).Sum(m => m.Quantity) ?? 0,
                            //PriStTrTo = (decimal?)(from v in db.StockTransferItems
                            //                       where (v.Item == b.ItemID && v.Unit == b.ItemUnitID)
                            //                       && (mc == w.MCTo)
                            //                           v.Quantity
                            //                       }).Sum(m => m.Quantity) ?? 0,
                            //SubStTrFrom = (decimal?)(from v in db.StockTransferItems
                            //                         where (v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName)
                            //                         && (mc == w.MCFrom)
                            //                             v.Quantity
                            //                         }).Sum(m => m.Quantity) ?? 0,

                            //SubStTrTo = (decimal?)(from v in db.StockTransferItems
                            //                       where (v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName)
                            //                       && (mc == w.MCTo)
                            //                           v.Quantity
                            //                       }).Sum(m => m.Quantity) ?? 0,


                            g.TagLine1,
                            g.TagLine2,
                            g.TagLine3,
                            g.TagLine4,
                            g.TagLine5,

                        }).Distinct();

            var vd = item.ToList();
            var data = (from o in vd
                        let PriStTrFrom = (decimal?)(from v in db.StockTransferItems
                                                     join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                     where (v.Item == o.id && v.Unit == o.ItemUnitID)
                                                     && (mc == 0 || mc == w.MCFrom)
                                                     select new
                                                     {
                                                         v.Quantity
                                                     }).Sum(m => m.Quantity) ?? 0
                        let PriStTrTo = (decimal?)(from v in db.StockTransferItems
                                                   join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                   where (v.Item == o.id && v.Unit == o.ItemUnitID)
                                                   && (mc == w.MCTo)
                                                   select new
                                                   {
                                                       v.Quantity
                                                   }).Sum(m => m.Quantity) ?? 0
                        let SubStTrFrom = (decimal?)(from v in db.StockTransferItems
                                                     join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                     where (v.Item == o.id && v.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                                     && (mc == w.MCFrom)
                                                     select new
                                                     {
                                                         v.Quantity
                                                     }).Sum(m => m.Quantity) ?? 0
                        let SubStTrTo = (decimal?)(from v in db.StockTransferItems
                                                   join w in db.StockTransfers on v.StockTransferId equals w.Id
                                                   where (v.Item == o.id && v.Unit == o.SubUnitId && o.PriUnit != o.SubUnit)
                                                   && (mc == w.MCTo)
                                                   select new
                                                   {
                                                       v.Quantity
                                                   }).Sum(m => m.Quantity) ?? 0



                        select new
                        {
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.Barcode,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),


                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),


                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            o.PriAddAdj,
                            o.SubAddAdj,
                            o.PriLessAdj,
                            o.subLessAdj,
                            PriStTrFrom,
                            SubStTrFrom,
                            PriStTrTo,
                            SubStTrTo,

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom)),

                            TagLine1 = o.TagLine1,
                            TagLine2 = o.TagLine2,
                            TagLine3 = o.TagLine3,
                            TagLine4 = o.TagLine4,
                            TagLine5 = o.TagLine5,
                            CheckStock = CheckItemStock,
                            o.SupplierRef
                        }).FirstOrDefault();
            return Json(data);

        }
        public ActionResult GetItemHire(int itemID, long? mc, int? SalType, int? HireType)
        {
            var ItemStockOut = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var CheckItemStock = ItemStockOut != null ? ItemStockOut.Status : Status.inactive;
            ViewBag.StockCheck = CheckItemStock;



            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.Jewellerys on b.ItemID equals g.Item into jewl
                        from g in jewl.DefaultIfEmpty()
                        let hir = db.HireRates.Where(x => x.ItemId == itemID && x.type == HireType).Select(y => y.Rate).FirstOrDefault()
                        where b.ItemID == itemID
                        select new
                        {
                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            PartNumber = b.PartNumber,
                            Tax = f.Percentage,
                            SellingPrice = (SalType != 2) ? b.SellingPrice : hir,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.Barcode,
                            b.SupplierRef,

                            OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,


                            PriPurchase = (decimal?)(from v in db.PEItemss
                                                     join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                     where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.ItemQuantity
                                                     }).Sum(c => c.ItemQuantity) ?? 0,
                            //(decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            SubPurchase = (decimal?)(from v in db.PEItemss
                                                     join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                     where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.ItemQuantity
                                                     }).Sum(c => c.ItemQuantity) ?? 0,

                            //(decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (decimal?)(from v in db.SEItemss
                                                 join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                 where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                 && (mc == 0 || mc == w.MaterialCenter)
                                                 && (w.SaleType != SaleType.Hire)
                                                 select new
                                                 {
                                                     v.ItemQuantity
                                                 }).Sum(c => c.ItemQuantity) ?? 0,

                            //(decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            SubSale = (decimal?)(from v in db.SEItemss
                                                 join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                 where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                 && (mc == 0 || mc == w.MaterialCenter)
                                                 && (w.SaleType != SaleType.Hire)
                                                 select new
                                                 {
                                                     v.ItemQuantity
                                                 }).Sum(c => c.ItemQuantity) ?? 0,

                            //(decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (decimal?)(from v in db.PRItemss
                                                    join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            //(decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (decimal?)(from v in db.PRItemss
                                                    join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            //(decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (decimal?)(from v in db.SRItemss
                                                    join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    && (w.SaleType != SaleType.Hire)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,
                            //(decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (decimal?)(from v in db.SRItemss
                                                    join w in db.SalesReturns on v.SalesReturnId equals w.SalesReturnId
                                                    where v.Item == b.ItemID && v.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    && (w.SaleType != SaleType.Hire)
                                                    select new
                                                    {
                                                        v.ItemQuantity
                                                    }).Sum(c => c.ItemQuantity) ?? 0,

                            //(decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && c.ItemUnitName != d.ItemUnitName && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)(from v in db.GeneratedItem
                                                     join w in db.Productions on v.Production equals w.ProductionId
                                                     where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.Qty
                                                     }).Sum(c => c.Qty) ?? 0,
                            SubProdItem = (decimal?)(from v in db.GeneratedItem
                                                     join w in db.Productions on v.Production equals w.ProductionId
                                                     where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                     && (mc == 0 || mc == w.MaterialCenter)
                                                     select new
                                                     {
                                                         v.Qty
                                                     }).Sum(c => c.Qty) ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)(from v in db.ProItems
                                                      join w in db.Productions on v.Production equals w.ProductionId
                                                      where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      select new
                                                      {
                                                          v.Quantity
                                                      }).Sum(c => c.Quantity) ?? 0,
                            //(decimal?)db.ProI
                            //tems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,

                            SubProdCItem = (decimal?)(from v in db.ProItems
                                                      join w in db.Productions on v.Production equals w.ProductionId
                                                      where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      select new
                                                      {
                                                          v.Quantity
                                                      }).Sum(c => c.Quantity) ?? 0,

                            //(decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,
                            // unassemble -----
                            // main item
                            PriUnItem = (decimal?)(from v in db.ConsumedItem
                                                   join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                   where v.Item == b.ItemID && v.Unit == b.ItemUnitID
                                                   && (mc == 0 || mc == w.MaterialCenter)
                                                   select new
                                                   {
                                                       v.Qty
                                                   }).Sum(c => c.Qty) ?? 0,
                            SubUnItem = (decimal?)(from v in db.ConsumedItem
                                                   join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                   where v.Item == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                   && (mc == 0 || mc == w.MaterialCenter)
                                                   select new
                                                   {
                                                       v.Qty
                                                   }).Sum(c => c.Qty) ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)(from v in db.UnassembleItems
                                                    join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                    where v.ItemId == b.ItemID && v.Unit == b.ItemUnitID
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.Quantity
                                                    }).Sum(c => c.Quantity) ?? 0,

                            //(decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,

                            SubUnCItem = (decimal?)(from v in db.UnassembleItems
                                                    join w in db.Unassembles on v.Unassemble equals w.UnassembleId
                                                    where v.ItemId == b.ItemID && v.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName
                                                    && (mc == 0 || mc == w.MaterialCenter)
                                                    select new
                                                    {
                                                        v.Quantity
                                                    }).Sum(c => c.Quantity) ?? 0,

                            //(decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && c.ItemUnitName != d.ItemUnitName).Select(a => a.Quantity).Sum() ?? 0,

                            g.TagLine1,
                            g.TagLine2,
                            g.TagLine3,
                            g.TagLine4,
                            g.TagLine5,

                        }).Distinct().AsEnumerable().Select(o => new
                        {
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.Barcode,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),


                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),


                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            o.PriAddAdj,
                            o.SubAddAdj,
                            o.PriLessAdj,
                            o.subLessAdj,

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),

                            TagLine1 = o.TagLine1,
                            TagLine2 = o.TagLine2,
                            TagLine3 = o.TagLine3,
                            TagLine4 = o.TagLine4,
                            TagLine5 = o.TagLine5,
                            CheckStock = CheckItemStock,
                            o.SupplierRef
                        }).FirstOrDefault();
            return Json(item);

        }
        public ActionResult GetTotalStock()
        {
            int recordsTotal = 0;

            string fdate = DateTime.Now.ToString("dd/MM/yyyy");
            DateTime ondates = DateTime.Parse(fdate, new CultureInfo("en-GB"));
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
                     where b.KeepStock == true
                     orderby b.ItemID ascending
                     select new
                     {
                         b.ItemID,
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,
                         b.KeepStock,

                         Purchase = (from i in db.PEItemss
                                     join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                     where i.Item == b.ItemID && (EF.Functions.DateDiffDay(j.PEDate, ondates) >= 0)
                                     group i by i.ItemId into g
                                     select new
                                     {
                                         PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                         SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                     }).FirstOrDefault(),



                         Sale = (from i in db.SEItemss
                                 join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                 where i.Item == b.ItemID && (EF.Functions.DateDiffDay(j.SEDate, ondates) >= 0) && (j.SaleType != SaleType.Hire)
                                 group i by i.ItemId into g
                                 select new
                                 {
                                     PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                     SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0

                                 }).FirstOrDefault(),


                         PReturn = (from i in db.PRItemss
                                    join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                    where i.Item == b.ItemID && (EF.Functions.DateDiffDay(j.PRDate, ondates) >= 0)
                                    group i by i.Item into g
                                    select new
                                    {
                                        PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                        SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                    }).FirstOrDefault(),


                         SReturn = (from i in db.SRItemss
                                    join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                    where i.Item == b.ItemID && (EF.Functions.DateDiffDay(j.SRDate, ondates) >= 0) && (j.SaleType != SaleType.Hire)
                                    group i by i.Item into g
                                    select new
                                    {
                                        PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                        SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0

                                    }).FirstOrDefault(),


                         //stock adjustment---
                         AddAdj = (from i in db.StockAdjustments
                                   where i.ItemID == b.ItemID && i.AdjustmentType == AdjustmentType.Add && (EF.Functions.DateDiffDay(i.AdjDate, ondates) >= 0)
                                   group i by i.ItemID into g
                                   select new
                                   {
                                       PriTotal = (decimal?)g.Where(x => x.ItemUnitID == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                       SubTotal = (decimal?)g.Where(x => x.ItemUnitID == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                   }).FirstOrDefault(),

                         LessAdj = (from i in db.StockAdjustments
                                    where i.ItemID == b.ItemID && i.AdjustmentType == AdjustmentType.Less && (EF.Functions.DateDiffDay(i.AdjDate, ondates) >= 0)
                                    group i by i.ItemID into g
                                    select new
                                    {
                                        PriTotal = (decimal?)g.Where(x => x.ItemUnitID == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                        SubTotal = (decimal?)g.Where(x => x.ItemUnitID == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                    }).FirstOrDefault(),


                         // production ----
                         // main item

                         ProdItem = (from i in db.GeneratedItem
                                     join j in db.Productions on i.Production equals j.ProductionId
                                     where i.Item == b.ItemID //&&i.ItemUnit == b.ItemUnitID &&
                                     group i by i.Item into g
                                     select new
                                     {
                                         PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Qty) ?? 0,
                                         SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Qty) ?? 0
                                     }).FirstOrDefault(),

                         // compined item
                         ProdCItem = (from i in db.ProItems
                                      where i.ItemId == b.ItemID //&&i.ItemUnit == b.ItemUnitID &&
                                      group i by i.ItemId into g
                                      select new
                                      {
                                          PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Quantity) ?? 0,
                                          SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Quantity) ?? 0
                                      }).FirstOrDefault(),

                         // unassemble -----
                         // main item

                         UnItem = (from i in db.ConsumedItem
                                   join j in db.Unassembles on i.Unassemble equals j.UnassembleId
                                   where i.Item == b.ItemID //&&i.ItemUnit == b.ItemUnitID &&
                                   group i by i.Item into g
                                   select new
                                   {
                                       PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Qty) ?? 0,
                                       SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Qty) ?? 0
                                   }).FirstOrDefault(),

                         // compined item
                         UnCItem = (from i in db.UnassembleItems
                                    where i.ItemId == b.ItemID  //&&i.ItemUnit == b.ItemUnitID &&
                                    group i by i.ItemId into g
                                    select new
                                    {
                                        PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Quantity) ?? 0,
                                        SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Quantity) ?? 0
                                    }).FirstOrDefault(),

                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),//(decimal?)f.ItemUnitPrice,
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),//(decimal?)f.ItemUnit,
                         b.PurchasePrice
                     }).Distinct().AsEnumerable().Select(k => new
                     {
                         k.ItemID,
                         k.ItemCode,
                         k.ItemName,
                         k.ItemUnitID,
                         k.SubUnitId,
                         k.PriUnit,
                         k.SubUnit,
                         k.OpeningStock,
                         k.ConFactor,

                         PriPurchase = (k.Purchase != null) ? k.Purchase.PriTotal : 0,
                         SubPurchase = (k.Purchase != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.Purchase.SubTotal : 0,

                         PriSale = (k.Sale != null) ? k.Sale.PriTotal : 0,
                         SubSale = (k.Sale != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.Sale.SubTotal : 0,

                         PriPReturn = (k.PReturn != null) ? k.PReturn.PriTotal : 0,
                         SubPReturn = (k.PReturn != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.PReturn.SubTotal : 0,

                         PriSReturn = (k.SReturn != null) ? k.SReturn.PriTotal : 0,
                         SubSReturn = (k.SReturn != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.SReturn.SubTotal : 0,

                         //PriHDNote = (k.HDNote != null) ? k.HDNote.PriTotal : 0,
                         //SubHDNote = (k.HDNote != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.HDNote.SubTotal : 0,

                         //PriRetNote = (k.RetNote != null) ? k.RetNote.PriTotal : 0,
                         //SubRetNote = (k.RetNote != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.RetNote.SubTotal : 0,

                         //PriHireMiss = (k.HireMiss != null) ? k.HireMiss.PriTotal : 0,
                         //SubHireMiss = (k.HireMiss != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.HireMiss.SubTotal : 0,

                         PriAddAdj = (k.AddAdj != null) ? k.AddAdj.PriTotal : 0,
                         SubAddAdj = (k.AddAdj != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.AddAdj.SubTotal : 0,

                         PriLessAdj = (k.LessAdj != null) ? k.LessAdj.PriTotal : 0,
                         subLessAdj = (k.LessAdj != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.LessAdj.SubTotal : 0,

                         PriProdItem = (k.ProdItem != null) ? k.ProdItem.PriTotal : 0,
                         SubProdItem = (k.ProdItem != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.ProdItem.SubTotal : 0,

                         PriUnCItem = (k.UnCItem != null) ? k.UnCItem.PriTotal : 0,
                         SubUnCItem = (k.UnCItem != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.UnCItem.SubTotal : 0,

                         PriProdCItem = (k.ProdCItem != null) ? k.ProdCItem.PriTotal : 0,
                         SubProdCItem = (k.ProdCItem != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.ProdCItem.SubTotal : 0,

                         PriUnItem = (k.UnItem != null) ? k.UnItem.PriTotal : 0,
                         SubUnItem = (k.UnItem != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.UnItem.SubTotal : 0,

                         cost = k.cost,
                         costu = k.costu,
                         k.PurchasePrice
                     })
                    .Distinct().AsEnumerable().Select(o => new
                    {
                        o.ItemID,
                        o.ItemCode,
                        o.ItemName,
                        o.ItemUnitID,
                        o.SubUnitId,
                        PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                        SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                        OpeningStock = o.OpeningStock,
                        o.ConFactor,

                        pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                        subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                        total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),

                        cost = (o.costu == o.ItemUnitID) ? o.cost : (o.cost * o.ConFactor),
                        o.PurchasePrice
                    }).Distinct().AsEnumerable().Select(o => new
                    {
                        o.ItemName,
                        stockvalue = (o.total / o.ConFactor) * (o.cost),
                        StockValueWithPurchase = (o.total / o.ConFactor) * (o.PurchasePrice),
                    });



            recordsTotal = v.Count();
            var data = v.ToList();
            var item = (decimal)data.Sum(a => a.stockvalue);

            return Json(item);
        }

        public ActionResult AllItem()
        {
            var item = db.Items.Where(c => c.Status == Status.active).Select(b => new
            {
                ItemID = b.ItemID,
                b.ItemBrandID,
                Brand = b.ItemBrands.ItemBrandName,
               b.ItemCategoryID,
                Category = b.ItemCategorys.ItemCategoryName,
                description=b.ItemCategorys.Description,
              //  b.ItemUnitID,
              //  Unit = b.ItemUnit.ItemUnitName,
             //   b.TaxID,
             //   b.ItemColorID,
             //   Color = b.ItemColors.ItemColorName,
              //  b.ItemSizeID,
             //   size = b.ItemSizes.ItemSizeName,
                b.SellingPrice,
                //b.BasePrice,
               // b.PurchasePrice,
                b.ItemName,
              //  b.ItemArabic,
                b.ItemCode,
             //   b.ItemDescription,
              //  b.MRP,
             //   b.Barcode,
               // b.CreatedBy,
               // b.CreatedUser,
               // b.CreatedUserID,

                ImageFile = db.ItemImages.Where(a => a.ItemID == b.ItemID).Select(a => a.FileName).FirstOrDefault(),
            }).ToList().OrderBy(c => c.description);

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(item);
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpGet]
        public ActionResult AddItem()
        {
            //        .Select(s => new
            //            Id = s.ItemCategoryID,
            //            CategoryName = s.ItemCategoryName

            ViewBag.ItemCategory = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = true, Text = "General", Value = "1"},
                         }, "Value", "Text", 1);

            var Brand = db.ItemBrands
                   .Select(s => new
                   {
                       Id = s.ItemBrandID,
                       BrandName = s.ItemBrandName
                   }).OrderBy(a => a.Id).ToList();
            ViewBag.ItemBrand = QkSelect.List(Brand, "Id", "BrandName");

            var Tax = db.Taxs
                   .Select(s => new
                   {
                       Id = s.TaxID,
                       TaxName = s.TaxName
                   }).OrderByDescending(x => x.Id).ToList();
            ViewBag.Tax = QkSelect.List(Tax, "Id", "TaxName");

            var Unit = db.ItemUnits
                   .Select(s => new
                   {
                       Id = s.ItemUnitID,
                       UnitName = s.ItemUnitName
                   }).OrderBy(a => a.Id).ToList();
            ViewBag.ItemUnit = QkSelect.List(Unit, "Id", "UnitName");

            var Size = db.ItemSizes
                   .Select(s => new
                   {
                       Id = s.ItemSizeID,
                       SizeName = s.ItemSizeName
                   }).OrderBy(a => a.Id).ToList();
            ViewBag.ItemSize = QkSelect.List(Unit, "Id", "SizeName");

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var Currency = db.CurrencyMasters
                    .Select(s => new
                    {
                        Id = s.Id,
                        Code = s.CurrencyCode
                    }).ToList();
            ViewBag.Currency = QkSelect.List(Currency, "Id", "Code");

            var Suppliers = db.Suppliers
                    .Select(s => new
                    {
                        Id = s.SupplierID,
                        Code = s.SupplierName
                    }).ToList();

            ViewBag.Supp = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "", Value = null},
                                }, "Value", "Text", 1);

            ViewBag.ItemType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "General", Value="1"},
                new SelectListItem() {Text = "Diamond", Value="2"},
                new SelectListItem() {Text = "Watch", Value="3"},
            }, "Value", "Text");

            var color = db.ItemColors
                             .Select(s => new
                             {
                                 ID = s.ItemColorID,
                                 Name = s.ItemColorName,
                             })
                             .ToList();
            ViewBag.Color = QkSelect.List(color, "ID", "Name");
            var Prefix = db.PrefixMasters
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.PrefixCode,
                             })
                             .ToList();
            ViewBag.Prefix = QkSelect.List(Prefix, "ID", "Name");

            if (User.IsInRole("Dev") || User.IsInRole("Create Item"))
            {
                var viewModel = new ItemViewModel
                {
                    Barcode = com.createBarcode().ToString(),
                    KeepStock = true,
                    ItemCode = ItemCodes(),
                    ItemCategorys = db.ItemCategorys.ToList(),
                    ItemBrands = db.ItemBrands.ToList(),
                    ItemColors = db.ItemColors.ToList(),
                    ItemSizes = db.ItemSizes.ToList(),
                    Taxs = db.Taxs.Where(b => b.Status == Status.active).OrderByDescending(x => x.TaxID).ToList(),
                    ItemUnits = db.ItemUnits.ToList(),
                    HireType = db.HireTypes.ToList()
                };
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
                ViewBag.PartNoCheck = PartNoCheck;

                var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
                ViewBag.BCEnable = enable != null ? enable.Status : Status.inactive;

                var brCode = (ViewBag.BCEnable == Status.active) ? com.createBarcode().ToString() : "";
                //check enable commision
                var enablecomm = db.EnableSettings.Where(a => a.EnableType == "ItemCommision").FirstOrDefault();
                ViewBag.COMEnable = enablecomm != null ? enablecomm.Status : Status.inactive;


                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                ViewBag.BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;


                var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
                ViewBag.JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;


                var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
                ViewBag.EnableCurrency = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;


                var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
                ViewBag.PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;

                ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType").Select(a => a.TypeValue).FirstOrDefault();

                var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
                var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
                ViewBag.BatchWiseStock = BatchCheck;

                companySet();
                return PartialView(viewModel);

            }
            else
            {
                return PartialView("../Shared/_AccessDenied");
            }
        }
        [HttpPost]
        //  [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Item")]
        public JsonResult AddItem(ItemViewModel ProdViewModel, string fnval, int? printcount)
        {

            bool stat = false;
            string msg;
            var BarCodeExists = false;// db.Barcodes.Any(u => u.BarcodeNumber == ProdViewModel.Barcode);
            var ItemCodeExists = db.Items.Any(u => u.ItemCode == ProdViewModel.ItemCode);
            var ItemNameExists = db.Items.Any(u => u.ItemName == ProdViewModel.ItemName);
            if (ItemCodeExists)
            {
                msg = "A Item with same Item code exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (ItemNameExists)
            {
                msg = "A Item with same Name exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (BarCodeExists)
            {
                msg = "A Item with same Barcode exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var itID = com.Item(ProdViewModel);
                    if (ProdViewModel.ItemImage != null)
                    {
                        if (ProdViewModel.ItemImage.ToList().First() != null)
                        {
                            var itimage = com.Images(ProdViewModel, itID);
                        }
                    }
                    if (ProdViewModel.ItemDocument != null)
                    {
                        if (ProdViewModel.ItemDocument.ToList().First() != null)
                        {
                            var itdoc = com.Document(ProdViewModel, itID);
                        }
                    }
                    if (ProdViewModel.HireTypes != null)
                    {
                        foreach (HireTypeViewModel Hire in ProdViewModel.HireTypes)
                        {
                            var rate = new HireRate
                            {
                                type = Hire.type,
                                Rate = Hire.Rate,
                                ItemId = itID
                            };
                            db.HireRates.Add(rate);
                            db.SaveChanges();
                        }
                    }

                    var UserId = User.Identity.GetUserId();

                    com.addlog(LogTypes.Created, UserId, "Item", "Items", findip(), itID, "Item Added Successfully");

                    if (fnval == "print")
                    {
                        var item = db.Items.Where(n => n.ItemID == itID).Select(b => new
                        {
                            b.ItemName,
                            b.Barcode,
                            b.MRP,
                            ItemPrice = b.SellingPrice,
                            PCount = printcount
                        }).FirstOrDefault();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item } };
                    }
                    else
                    {
                        var Category = db.ItemCategorys.Where(p => p.Parent == 1 || p.ItemCategoryID == 1)
                                        .Select(s => new
                                        {
                                            Id = s.ItemCategoryID,
                                            CategoryName = s.ItemCategoryName
                                        }).ToList();
                        ViewBag.ItemCategory = QkSelect.List(Category, "Id", "CategoryName");

                        msg = "Successfully added Item details.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

            }

        }
        [HttpPost]
        public JsonResult PriceUpdater(List<PriceUpdaterViewModel> Addon)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                for (int i = 0; i < Addon.Count; i++)
                {
                    Item items = db.Items.Find(Addon[i].ItemId);
                    items.SellingPrice = Addon[i].SellingPrice;
                    items.PurchasePrice = Addon[i].PurchasePrice;
                    items.cashprice = Addon[i].cashprice;
                    items.creditprice = Addon[i].creditprice;
                    if (items.ConFactor > 1 && Addon[i].SellingPrice < 1)
                    {
                        items.SellingPrice = Addon[i].SellingPrice * items.ConFactor;
                        items.PurchasePrice = Addon[i].PurchasePrice * items.ConFactor;
                    }
                    items.MRP = Addon[i].MRP;
                    items.BasePrice = Addon[i].BasePrice;
                    if (items.ConFactor > 1 && Addon[i].SellingPrice < 1)
                    {
                        items.SellingPrice = Addon[i].SellingPrice * items.ConFactor;
                        items.PurchasePrice = Addon[i].PurchasePrice * items.ConFactor;
                        items.MRP = Addon[i].MRP * items.ConFactor;
                        items.BasePrice = Addon[i].BasePrice * items.ConFactor;
                    }
                    db.Entry(items).State = EntityState.Modified;
                    db.SaveChanges();

                    com.addlog(LogTypes.Updated, UserId, "Item", "Items", findip(), items.ItemID, "Item Price Updated Successfully");
                }

                msg = "Successfully added Item details.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        [HttpGet]
        // [QkAuthorize(Roles = "Dev,View Item")]
        public ActionResult Details(long? id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            var JCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;
            ViewBag.JewCheck = JCheck;

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var ECur = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            ViewBag.EnableCurrency = ECur;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ItemViewModel vmodel = new ItemViewModel();

            vmodel = (from a in db.Items
                      join b in db.ItemCategorys on a.ItemCategoryID equals b.ItemCategoryID into cat
                      from b in cat.DefaultIfEmpty()
                      join c in db.ItemBrands on a.ItemBrandID equals c.ItemBrandID into brand
                      from c in brand.DefaultIfEmpty()
                      join d in db.ItemSizes on a.ItemSizeID equals d.ItemSizeID into Isize
                      from d in Isize.DefaultIfEmpty()
                      join e in db.Taxs on a.TaxID equals e.TaxID into taxes
                      from e in taxes.DefaultIfEmpty()
                      join f in db.ItemColors on a.ItemColorID equals f.ItemColorID into Icolor
                      from f in Icolor.DefaultIfEmpty()
                      join g in db.ItemUnits on a.ItemUnitID equals g.ItemUnitID into unt
                      from g in unt.DefaultIfEmpty()
                      join h in db.ItemUnits on a.SubUnitId equals h.ItemUnitID into sbunit
                      from h in sbunit.DefaultIfEmpty()
                      join i in db.CurrencyMasters on a.Currency equals i.Id into crncy
                      from i in crncy.DefaultIfEmpty()
                      join j in db.Branchs on a.Branch equals j.BranchID into brnch
                      from j in brnch.DefaultIfEmpty()
                      join k in db.Suppliers on a.Supplier equals k.SupplierID into suppl
                      from k in suppl.DefaultIfEmpty()
                      join l in db.Jewellerys on a.ItemID equals l.Item into jew
                      from l in jew.DefaultIfEmpty()
                      join m in db.Diamonds on a.ItemID equals m.Item into dia
                      from m in dia.DefaultIfEmpty()
                      join n in db.Watchs on a.ItemID equals n.Item into watch
                      from n in watch.DefaultIfEmpty()

                      where a.ItemID == id
                      select new ItemViewModel
                      {
                          ItemID = a.ItemID,
                          ItemCode = a.ItemCode,
                          ItemName = a.ItemName,
                          ItemArabic = a.ItemArabic,
                          Barcode = a.Barcode,
                          ItemDescription = a.ItemDescription.Replace("\n", ""),
                          SellingPrice = a.SellingPrice,
                          PurchasePrice = a.PurchasePrice,
                          BasePrice = a.BasePrice,
                          MRP = a.MRP,
                          CategoryName = b.ItemCategoryName,
                          BrandName = c.ItemBrandName,
                          ItemSize = d.ItemSizeName,
                          Tax = e.Percentage,
                          TaxName = e.TaxName,
                          PUnit = g.ItemUnitName,
                          SUnit = h.ItemUnitName,
                          CurrencyName = i.CurrencyCode,
                          ConRate = a.ConRate,
                          ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                          OpeningStock = a.OpeningStock,
                          MinStock = a.MinStock,
                          KeepStock = a.KeepStock,
                          Status = a.Status,
                          PartNumber = a.PartNumber,
                          ItemColor = f.ItemColorName,
                          Commission = a.Commission,
                          BranchName = j.BranchName,
                          SupplierName = k.SupplierName,
                          SupplierRef = a.SupplierRef,
                          StockKeepName = a.KeepStock != true ? "No Stock" : "Item Stocked",
                          ItemType = a.ItemType,
                          // Jewellery Details
                          Type = l.Type,
                          SetRef = l.SetRef,
                          Country = l.Country,
                          Style = l.Style,
                          Tagline1 = l.TagLine1,
                          Tagline2 = l.TagLine2,
                          Tagline3 = l.TagLine3,
                          Tagline4 = l.TagLine4,
                          Tagline5 = l.TagLine5,

                          //Diamond Details
                          Design = m.Design,
                          Clarify = m.Clarify,
                          Fluorescence = m.Fluorescence,
                          Range = m.Range,
                          Time = m.Time,
                          CertificateNo = m.CertificateNo,

                          //Watch Details
                          Refno = n.Refno,
                          ModelNo = n.ModelNo,
                          ModelName = n.ModelName,
                          Straptype = n.Straptype,
                          DialShape = n.DialShape,
                          DialColor = n.DialColor,
                          Material = n.Material,
                          Movement = n.Movement,
                          Weight = n.Weight,
                          StoneType = n.StoneType,
                          Warranty = n.Warranty,
                          HireTypes = (from d in db.HireRates
                                       join e in db.HireTypes on d.type equals e.HireTypeId
                                       where d.ItemId == a.ItemID
                                       select new HireTypeViewModel
                                       {
                                           TypeName = e.Name,
                                           Rate = d.Rate
                                       }).ToList(),
                      }).FirstOrDefault();
            return View(vmodel);
        }

        [HttpPost]
        public JsonResult ItemDetails(long[] items, decimal[] purprices, long[] unitpri)
        {
            var salesrateupdateinpurchaseentrysame = db.EnableSettings.Where(c => c.EnableType == "salesrateupdateinpurchaseentrysame").SingleOrDefault();
            if (items == null)
            {
                return Json(new { dataa = "", prii = "" });
            }
            var data = db.Items.Where(b => b.Status == Status.active && items.Contains(b.ItemID)).Select(b => new
            {
                b.ItemID,
                b.ItemCode,
                b.ItemName,
                cashprice = (b.cashprice == null) ? ((salesrateupdateinpurchaseentrysame.Status == Status.active) ? 0 : b.SellingPrice) : b.cashprice,
                creditprice = (b.creditprice == null) ? ((salesrateupdateinpurchaseentrysame.Status == Status.active) ? 0 : b.SellingPrice) : b.creditprice,
                ItemWithCode = b.ItemCode + " - " + b.ItemName,
                b.ItemArabic,
                b.Barcode,
                b.ItemDescription,
                SellingPrice = (salesrateupdateinpurchaseentrysame.Status == Status.active) ? 0 : b.SellingPrice,
                b.PurchasePrice,
                b.MRP,
                b.BasePrice,
                b.Status,
                b.KeepStock,
                b.ItemUnit,
                b.ItemUnitID,
                b.SubUnitId,
                ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                b.PricingStrategy

            }).ToList();
            data = data.OrderBy(x => Array.IndexOf(items, x.ItemID)).ToList();
            List<decimal[]> prices = new List<decimal[]>();
            int i = 0;
            foreach (var dt in data)
            {
                if (dt.PricingStrategy == true)
                {
                    if (dt.ItemUnitID == unitpri[i])
                        prices.Add(GetPricePurchase(dt.ItemID, purprices[i]));
                    else
                        prices.Add(GetPricePurchase(dt.ItemID, dt.PurchasePrice));
                }
                else
                {
                    decimal[] pr = { 0, 0 };

                    prices.Add(pr);
                }
                i++;
            }
            return Json(new { data = data, pri = prices });
        }
        public decimal[] GetPricePurchase(long itemid, decimal purchaseprice)
        {

            var item = (from a in db.Items
                        where a.ItemID == itemid
                        select new
                        {
                            a.PricingStrategy,
                            a.PricingStrategyAmountType,
                            a.PricingStrategyType,
                            a.PricingStrategyValue,
                            a.PurchasePrice,
                            a.SellingPrice,

                            cashprice = (a.cashprice == null) ? a.SellingPrice : a.cashprice,
                            creditprice = (a.creditprice == null) ? a.SellingPrice : a.creditprice,
                        }
                      ).FirstOrDefault();

            decimal purprice = 0;
            decimal salprice = 0;

            if (item.PricingStrategyType == pricingstatagytype.FIFO)
            {



                purprice = item.PurchasePrice;
                salprice = item.SellingPrice;

            }


            else if (item.PricingStrategyType == pricingstatagytype.AVG)
            {
                purprice = (item.PurchasePrice + purchaseprice) / 2;

                if (item.PricingStrategyAmountType == AmountType.AbsoluteAmount)
                    salprice = purprice + (decimal)item.PricingStrategyValue;
                else
                {
                    salprice = purprice + purprice * (decimal)item.PricingStrategyValue / 100;
                }
            }
            else if (item.PricingStrategyType == pricingstatagytype.ABS)
            {
                purprice = purchaseprice;
                salprice = (decimal)item.SellingPrice;



            }

            else
            {
                purprice = purchaseprice;


                if (item.PricingStrategyAmountType == AmountType.AbsoluteAmount)
                    salprice = purprice + (decimal)item.PricingStrategyValue;
                else
                {
                    salprice = purprice + purprice * (decimal)item.PricingStrategyValue / 100;
                }


            }

            decimal[] pr = { purprice, salprice };
            return pr;
        }

        [QkAuthorize(Roles = "Dev,Price Search")]
        public ActionResult PriceSearch()
        {
            var mcs = db.MCs.Select(s => new
            {
                Id = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            ViewBag.Item = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.Category = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                               }, "Value", "Text", 1);

            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Price Search")]
        public ActionResult GetPrice(long? itemid, long? category, long matrialcenter)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            var v = (from b in db.Items
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.PEItemsId).Max() }
                     equals new { f1 = f.Item, f2 = f.PEItemsId } into pur
                     from f in pur.DefaultIfEmpty()
                     join g in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.SEItemsId).Max() }
                     equals new { f1 = g.Item, f2 = g.SEItemsId } into sal
                     from g in sal.DefaultIfEmpty()
                     where (itemid == null || itemid == 0 || b.ItemID == itemid) && (category == null || category == 0 || b.ItemCategoryID == category)
                     orderby b.ItemID ascending
                     select new
                     {
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         ItemWithCode = b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.ItemID,
                         categoryname = e.ItemCategoryName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                         b.MRP,
                         b.PurchasePrice,
                         b.SellingPrice,
                         b.BasePrice,

                         PriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemUnitPrice).Sum() ?? 0,
                         SubPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemUnitPrice).Sum() ?? 0,
                         NoOfVchPur = (int?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             where (i.Item == b.ItemID)
                                             select new
                                             {
                                                 saleid = i.PurchaseEntry
                                             }).GroupBy(x => x.saleid).Count() ?? 1,
                         lastPur = (decimal?)f.ItemUnitPrice,
                         lastPurU = (decimal?)f.ItemUnit,


                         PriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemUnitPrice).Sum() ?? 0,
                         SubSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemUnitPrice).Sum() ?? 0,
                         NoOfVchSale = (int?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              where (i.Item == b.ItemID)
                                              select new
                                              {
                                                  saleid = i.SalesEntry
                                              }).GroupBy(x => x.saleid).Count() ?? 1,
                         lastSale = (decimal?)g.ItemUnitPrice,
                         lastSaleU = (decimal?)g.ItemUnit,


                         PriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemUnitPrice).Sum() ?? 0,
                         SubPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemUnitPrice).Sum() ?? 0,

                         PriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemUnitPrice).Sum() ?? 0,
                         SubSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemUnitPrice).Sum() ?? 0

                     }).AsEnumerable().Select(o => new
                     {
                         o.ItemID,
                         o.ItemCode,
                         o.ItemName,
                         o.ItemWithCode,
                         o.ItemUnitID,
                         o.SubUnitId,
                         o.categoryname,

                         o.MRP,
                         o.PurchasePrice,
                         o.SellingPrice,
                         o.BasePrice,
                         PurAvg = ((o.PriPurchase * o.ConFactor) + o.SubPurchase) / (o.NoOfVchPur != 0 ? o.NoOfVchPur : 1),
                         SaleAvg = ((o.PriSale * o.ConFactor) + o.SubSale) / (o.NoOfVchSale != 0 ? o.NoOfVchSale : 1),
                         //o.lastPurU,
                         //o.lastSale,
                         //o.lastSaleU
                         cost = (o.lastPurU == o.ItemUnitID) ? o.lastPur : (o.lastPur * o.ConFactor),
                         price = (o.lastSaleU == o.ItemUnitID) ? o.lastSale : (o.lastSale * o.ConFactor)
                     });


            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [QkAuthorize(Roles = "Dev,Price Search")]
        public ActionResult PriceSearchModel2()
        {
            var mcs = db.MCs.Select(s => new
            {
                Id = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            ViewBag.Item = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.Category = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                               }, "Value", "Text", 1);

            companySet();
            return View();
        }


        //shiyas
        [QkAuthorize(Roles = "Dev,Price Search,Price Search Shorrom Wise")]
        public ActionResult ItemWiseShowroom()
        {
            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;


            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;

            //                 .Select(s => new
            //                     id = s.ItemID,
            //                     text = s.ItemCode + "-" + s.ItemName
            //                 })
            ViewBag.Item = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "--Select Item--", Value = ""},
                             }, "Value", "Text", 1);
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            List<SelectFormat> use = new List<SelectFormat>();
            use.Add(new SelectFormat { id = 1, text = "Mother Company" });
            use.Add(new SelectFormat { id = 2, text = "abu dhabi" });
            use.Add(new SelectFormat { id = 3, text = "mussafa" });
            use.Add(new SelectFormat { id = 4, text = "aln" });
            use.Add(new SelectFormat { id = 5, text = "dubai" });
            use.Add(new SelectFormat { id = 6, text = "moderate" });
            use.Add(new SelectFormat { id = 7, text = "Quick Vision" });
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);

            long[] selmc = { 1, 6 };

            ViewBag.mc = new MultiSelectList(use, "id", "text", selmc);
            var use2 = db.Items.Where(p => (p.Status == Status.active)).Select(s => new SelectFormat { id = s.ItemID, text = s.ItemCode + "-" + s.ItemName }).ToList();

            use2.Insert(0, initial);

            long[] selmc2 = { };


            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName


            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        public ActionResult ItemWisecus()
        {
            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;


            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;

            //                 .Select(s => new
            //                     id = s.ItemID,
            //                     text = s.ItemCode + "-" + s.ItemName
            //                 })
            ViewBag.Item = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "--Select Item--", Value = ""},
                             }, "Value", "Text", 1);
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
         
            var use = QkSelect.List(new List<SelectListItem>
            {


                new SelectListItem { Value = "20085", Text = "MUSSAFAH" },
                new SelectListItem { Value = "20086", Text = "ABU DHABI" },
                     new SelectListItem { Value = "20087", Text = "ALAIN" },
                },
               "Value", "Text");
            var initial = new SelectFormat() { id = 0, text = "All" };
          

            long[] selmc = { 20085, 20086, 20087 };
            bool isquicknet = db.companys.Any(o => o.CPName.Contains("QUICK NET COMPUTERS"));
            ViewBag.mc = new MultiSelectList(use, "Value", "Text", selmc);
            
            

            var use2 = db.Items.Where(p => (p.Status == Status.active)).Select(s => new SelectFormat { id = s.ItemID, text = s.ItemCode + "-" + s.ItemName }).ToList();

            use2.Insert(0, initial);

            long[] selmc2 = { };


            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName


            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Price Search")]
        public ActionResult ItemWise()
        {
            var LastTransInSale = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").FirstOrDefault();
            var LastTransInSales = LastTransInSale != null ? LastTransInSale.Status : Status.inactive;
            ViewBag.LastTransInSale = LastTransInSales;


            var LastTransInPurchase = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").FirstOrDefault();
            var LastTransInPurchases = LastTransInPurchase != null ? LastTransInPurchase.Status : Status.inactive;
            ViewBag.LastTransInPurchase = LastTransInPurchases;

            //                 .Select(s => new
            //                     id = s.ItemID,
            //                     text = s.ItemCode + "-" + s.ItemName
            //                 })
            ViewBag.Item = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "--Select Item--", Value = ""},
                             }, "Value", "Text", 1);
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            var use = db.MCs.Select(s => new SelectFormat { id = s.MCId, text = s.MCName }).ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);

            long[] selmc = { 20085, 20086, 20087, 20084 };
            bool isquicknet = db.companys.Any(o => o.CPName.Contains("QUICK NET COMPUTERS"));
            ViewBag.mc = new MultiSelectList(use, "id", "text", selmc);
            if (!isquicknet)
            {
                ViewBag.mc = new MultiSelectList(use, "id", "text");
            }
           
            var use2 = db.Items.Where(p => (p.Status == Status.active)).Select(s => new SelectFormat { id = s.ItemID, text = s.ItemCode + "-" + s.ItemName }).ToList();

            use2.Insert(0, initial);

            long[] selmc2 = { };


            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName


            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Item Wise")]
        public ActionResult ItemWise(long?[] ddlItem, long?[] ddlMC)
        {
            if (ddlMC == null)
            {
                ddlMC[0] = 0;
            }


            if (ddlItem == null)
            {
                Danger("select Item", true);
                return RedirectToAction("ItemWise");
            }

            var items = String.Join(",", ddlItem);
            string mc = "";
            if (ddlMC == null)
                mc = "0";
            else
                mc = String.Join(",", ddlMC);

            return RedirectToAction("ViewItemWise", new { itemid = items, ddmc = mc });
        }
        [QkAuthorize(Roles = "Dev,Stock Item Wise,HireStock Item Wise")]
        public ActionResult GetItemWise(string itemidd, string ddmcc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            long[] mcs = new long[] { };
            if (ddmcc != "0")
                mcs = ddmcc.Split(',').Select(x => long.Parse(x)).ToArray();
            else
            {
                mcs = db.MCs.Select(x => x.MCId).ToArray();
            }
            int[] items = itemidd.Split(',').Select(x => int.Parse(x)).ToArray();

            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            List<forpricesearch> alldata = new List<forpricesearch>();



            foreach (long itemid in items)
            {
                foreach (long ddmc in mcs)
                {


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
                                 pprice = b.PurchasePrice,
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
                        (from b in data

                         let materialcenre = (from a in db.MCs
                                              where a.MCId == ddmc
                                              select new
                                              {
                                                  a.MCName
                                              }).FirstOrDefault()
                         let stocks = GetItemWisestock(Convert.ToInt64(b.ItemID), ddmc)

                         select new forpricesearch
                         {
                             ItemID = b.ItemID,
                             itemcode = b.ItemCode,

                             itemname = b.ItemName,

                             PriUnit = (b.PriUnit != null) ? b.PriUnit : "",

                             SubUnit = (b.SubUnit != null) ? b.SubUnit : "",

                             OpeningStock = b.OpeningStock,
                             MinStock = (b.MinStock != null) ? b.MinStock : 0,
                             ConFactor = b.ConFactor,



                             pritotal = stocks * b.ConFactor,// ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)),
                             subtotal = stocks * b.ConFactor * b.ConFactor,//((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross + SubAReturn) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross +SubAsset)),

                             total = stocks * b.ConFactor,// (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross+SubAsset)), // - (StTrFrom - StTrTo)

                             //  stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo +PriAReturn) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss+ SubAReturn)), // + StTrTo
                             //  stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriAsset) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubAsset)), // + StTrFrom 
                             cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                             price = b.price,
                             stcost = b.pprice,
                             MC = materialcenre.MCName,
                         }).ToList();



                    alldata.AddRange(mydata);


                }
            }

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                alldata = alldata.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = alldata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }
        [QkAuthorize(Roles = "Dev,Price Search,HireStock Item Wise,Price Search Shorrom Wise")]
        public ActionResult GetItemWisenewshowroom(string[] itemidd, long[] ddmcc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            long[] mcs = ddmcc;
            string[] items = itemidd;

            if (ddmcc == null)
            {
                long[] a = { 0, 1 };
                ddmcc = a;
            }
            var temp = db.MCs.Select(a => a.MCId).ToArray();
            foreach (long ddmc in ddmcc)
            {
                if (ddmc == 0)
                {
                    mcs = temp;
                }
            }


            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            List<forpricesearch> alldata = new List<forpricesearch>();

            string mcname = "";
            long itemid = 0;
            foreach (string itemiddd in items)
            {

                foreach (long ddmc in mcs)
                {
                    if (ddmc == 1)
                    {
                        db = new ApplicationDbContext("quicknet");
                        mcname = "monther company";

                    }
                    else if (ddmc == 2)
                    {
                        db = new ApplicationDbContext("abudhabi");
                        mcname = "ABU DHABI";

                    }


                    else if (ddmc == 3)
                    {
                        db = new ApplicationDbContext("musafa");
                        mcname = "MUSSAFFA";

                    }
                    else if (ddmc == 4)
                    {
                        db = new ApplicationDbContext("aln");
                        mcname = "ALN SHOP";

                    }

                    else if (ddmc == 5)
                    {
                        db = new ApplicationDbContext("dubai");
                        mcname = "DUBAI SHOP";

                    }
                    else if (ddmc == 6)
                    {
                        db = new ApplicationDbContext("moderate");
                        mcname = "MODERATE";

                    }
                    else if (ddmc == 7)
                    {
                        db = new ApplicationDbContext("quickvision");
                        mcname = "QUICK VISION";

                    }

                    itemid = db.Items.Where(o => o.ItemName == itemiddd).Select(o => o.ItemID).FirstOrDefault();
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
                                        where (az.Item == b.ItemID) && cz.MCTo == 1
                                        select new
                                        {
                                            az.Price,
                                            az.Unit
                                        }).FirstOrDefault()
                             where (b.ItemID == itemid)
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
                                 pprice = b.PurchasePrice,
                                 cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                                 costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                                 stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                                 stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                                 price = b.SellingPrice,
                                 b.BasePrice,
                                 b.MRP
                             }).Distinct();




                    v = v.OrderBy(b => b.ItemName);
                    recordsTotal = v.Count();
                    var data = v.Skip(skip).Take(pageSize).ToList();
                    var mydata =
                        (from b in data
                         let materialcenre = mcname
                         let stocks = (mcname == "monther company") ? GetItemWisestock(Convert.ToInt64(b.ItemID), 20083) : GetItemWisestock(Convert.ToInt64(b.ItemID), 0)
                         select new forpricesearch
                         {
                             ItemID = b.ItemID,
                             itemcode = b.ItemCode,

                             itemname = b.ItemName,

                             PriUnit = (b.PriUnit != null) ? b.PriUnit : "",

                             SubUnit = (b.SubUnit != null) ? b.SubUnit : "",

                             OpeningStock = b.OpeningStock,
                             MinStock = (b.MinStock != null) ? b.MinStock : 0,
                             ConFactor = b.ConFactor,



                             pritotal = stocks * b.ConFactor,// ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)),
                             subtotal = stocks * b.ConFactor * b.ConFactor,//((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross + SubAReturn) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross +SubAsset)),

                             total = stocks * b.ConFactor,// (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross+SubAsset)), // - (StTrFrom - StTrTo)

                             //  stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo +PriAReturn) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss+ SubAReturn)), // + StTrTo
                             //  stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriAsset) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubAsset)), // + StTrFrom 
                             cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                             price = b.price,
                             stcost = b.pprice,
                             BasePrice = b.BasePrice,
                             mrp = b.MRP,
                             MC = mcname,
                         }).ToList();



                    alldata.AddRange(mydata);


                }
            }
            db = new ApplicationDbContext();
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                alldata = alldata.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = alldata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        public ActionResult GetItemWisenew(long?[] itemidd, long[] ddmcc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            long[] mcs = ddmcc;
            long?[] items = itemidd;
            if (ddmcc == null)
            {
                long[] a = { 0, 1 };
                ddmcc = a;
            }
            var temp = db.MCs.Select(a => a.MCId).ToArray();
            foreach (long ddmc in ddmcc)
            {
                if (ddmc == 0)
                {
                    mcs = temp;
                }
            }


            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            List<forpricesearch> alldata = new List<forpricesearch>();



            foreach (long itemid in items)
            {
                foreach (long ddmc in mcs)
                {


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
                                 pprice = b.PurchasePrice,
                                 cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                                 costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                                 stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                                 stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                                 price = b.SellingPrice,
                                 b.BasePrice,
                                 b.MRP,
                                 CashPrice = (decimal?)((b.cashprice != null) ? b.cashprice : b.SellingPrice),
                                 CreditPrice = (decimal?)((b.creditprice != null) ? b.creditprice : b.SellingPrice),
                             }).Distinct();




                    v = v.OrderBy(b => b.ItemName);
                    recordsTotal = v.Count();
                    var data = v.Skip(skip).Take(pageSize).ToList();
                    var mydata =
                        (from b in data
                         let materialcenre = (from a in db.MCs
                                              where a.MCId == ddmc
                                              select new
                                              {
                                                  a.MCName
                                              }).FirstOrDefault()
                         let stocks = GetItemWisestock(Convert.ToInt64(b.ItemID), ddmc)
                         select new forpricesearch
                         {
                             ItemID = b.ItemID,
                             itemcode = b.ItemCode,

                             itemname = b.ItemName,

                             PriUnit = (b.PriUnit != null) ? b.PriUnit : "",

                             SubUnit = (b.SubUnit != null) ? b.SubUnit : "",

                             OpeningStock = b.OpeningStock,
                             MinStock = (b.MinStock != null) ? b.MinStock : 0,
                             ConFactor = b.ConFactor,



                             pritotal = stocks * b.ConFactor,// ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)),
                             subtotal = stocks * b.ConFactor * b.ConFactor,//((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross + SubAReturn) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross +SubAsset)),

                             total = stocks * b.ConFactor,// (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross+SubAsset)), // - (StTrFrom - StTrTo)

                             //  stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo +PriAReturn) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss+ SubAReturn)), // + StTrTo
                             //  stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriAsset) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubAsset)), // + StTrFrom 
                             cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                             price = b.price,
                             stcost = com.GetItemPurchasePriceold(b.ItemID, System.DateTime.Now, ddmc),
                             BasePrice = b.BasePrice,
                             mrp = b.MRP,
                             CashPrice = b.CashPrice,
                             CreditPrice = b.CreditPrice,
                             MC = materialcenre.MCName,
                             stockpostionlist = getpositionlist(b.ItemID, ddmc)
                         }).ToList();



                    alldata.AddRange(mydata);


                }
            }

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                alldata = alldata.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = alldata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }
        public List<StockPositionViewModel> getpositionlist(long id, long mc)
        {
            var findrackmc = (from az in db.shelfstockmovements
                              join bz in db.rackmaterialcentres on az.rackmciid equals bz.rackmcid into temp2
                              from bz in temp2.DefaultIfEmpty()
                              join dz in db.Racks on bz.rackid equals dz.RackId into temp3
                              from dz in temp3.DefaultIfEmpty()
                              join ez in db.Shelves on bz.shelfid equals ez.ShelfId into temp4
                              from ez in temp4.DefaultIfEmpty()
                              where (az.itemid == id) &&
                              (bz.mcid == mc) &&
                              (az.purpose == "Open" || az.purpose == "Stock Received" || az.purpose == "Purchase" || az.purpose == "Sales Return")
                              group new { ez.shelfName, dz.RackName, az.qty, az.rackmciid } by new { az.rackmciid } into g
                              select new StockPositionViewModel
                              {
                                  shelfname = g.FirstOrDefault().shelfName,
                                  rackname = g.FirstOrDefault().RackName,
                                  itemQuantity = g.Sum(k => k.qty),
                                  rackmc = g.FirstOrDefault().rackmciid,
                              }).ToList();
            var findrackmc2 = (from az in db.shelfstockmovements
                               join bz in db.rackmaterialcentres on az.rackmciid equals bz.rackmcid into temp2
                               from bz in temp2.DefaultIfEmpty()
                               join dz in db.Racks on bz.rackid equals dz.RackId into temp3
                               from dz in temp3.DefaultIfEmpty()
                               join ez in db.Shelves on bz.shelfid equals ez.ShelfId into temp4
                               from ez in temp4.DefaultIfEmpty()
                               where (az.itemid == id) &&
                               (bz.mcid == mc) &&
                               (az.purpose == "Stock Transfered" || az.purpose == "Sales" || az.purpose == "Purchase Return")
                               group new { ez.shelfName, dz.RackName, az.qty, az.rackmciid } by new { az.rackmciid } into g
                               select new StockPositionViewModel
                               {
                                   shelfname = g.FirstOrDefault().shelfName,
                                   rackname = g.FirstOrDefault().RackName,
                                   itemQuantity = g.Sum(k => -k.qty),
                                   rackmc = g.FirstOrDefault().rackmciid,
                               }).ToList();
            findrackmc.AddRange(findrackmc2);
            var findrackmc3 = (from az in findrackmc
                               group new { az.shelfname, az.rackname, az.itemQuantity, az.rackmc } by new { az.rackmc } into g
                               select new StockPositionViewModel
                               {
                                   shelfname = g.FirstOrDefault().shelfname,
                                   rackname = g.FirstOrDefault().rackname,
                                   itemQuantity = g.Sum(k => k.itemQuantity),
                                   rackmc = g.FirstOrDefault().rackmc,
                               }).ToList();
            findrackmc3 = findrackmc3.Where(a => a.itemQuantity != 0).ToList();

            return findrackmc3;
        }

        [QkAuthorize(Roles = "Dev,Price Search,HireStock Item Wise")]
        public ActionResult GetItemWisenewsugg(long?[] itemidd, long[] ddmcc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            long[] mcs = ddmcc;
            long?[] items = itemidd;

            if (ddmcc == null)
            {
                long[] a = { 0, 1 };
                ddmcc = a;
            }
            var temp = db.MCs.Select(a => a.MCId).ToArray();
            foreach (long ddmc in ddmcc)
            {
                if (ddmc == 0)
                {
                    mcs = temp;
                }
            }


            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            List<forpricesearch> alldata = new List<forpricesearch>();



            foreach (long itemid in items)
            {
                long[] sugitemid = db.suggestItems.Where(o => o.priitemid == itemid).Select(o => o.sugitemid).ToArray();
                if (sugitemid.Count() == 0)
                {
                    long? priid = db.suggestItems.Where(o => o.sugitemid == itemid).Select(o => o.priitemid).FirstOrDefault();

                    sugitemid = db.suggestItems.Where(o => o.priitemid == priid && o.sugitemid != itemid).Select(o => o.sugitemid).ToArray();
                    if (priid != null)
                    {
                        sugitemid = sugitemid.Concat(new long[] { (long)priid }).ToArray();
                    }
                }
                foreach (long sugid in sugitemid)
                {
                    foreach (long ddmc in mcs)
                    {


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
                                 where (b.ItemID == sugid)
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
                                     pprice = b.PurchasePrice,
                                     cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                                     costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                                     stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                                     stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                                     price = b.SellingPrice,
                                     b.BasePrice,
                                     b.MRP,
                                     CashPrice = (decimal?)((b.cashprice != null) ? b.cashprice : b.SellingPrice),
                                     CreditPrice = (decimal?)((b.creditprice != null) ? b.creditprice : b.SellingPrice),
                                 }).Distinct();




                        v = v.OrderBy(b => b.ItemName);
                        recordsTotal = v.Count();
                        var data = v.Skip(skip).Take(pageSize).ToList();
                        var mydata =
                            (from b in data
                             let materialcenre = (from a in db.MCs
                                                  where a.MCId == ddmc
                                                  select new
                                                  {
                                                      a.MCName
                                                  }).FirstOrDefault()
                             let stocks = GetItemWisestock(Convert.ToInt64(b.ItemID), ddmc)
                             select new forpricesearch
                             {
                                 ItemID = b.ItemID,
                                 itemcode = b.ItemCode,

                                 itemname = b.ItemName,

                                 PriUnit = (b.PriUnit != null) ? b.PriUnit : "",

                                 SubUnit = (b.SubUnit != null) ? b.SubUnit : "",

                                 OpeningStock = b.OpeningStock,
                                 MinStock = (b.MinStock != null) ? b.MinStock : 0,
                                 ConFactor = b.ConFactor,



                                 pritotal = stocks * b.ConFactor,// ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)),
                                 subtotal = stocks * b.ConFactor * b.ConFactor,//((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross + SubAReturn) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross +SubAsset)),

                                 total = stocks * b.ConFactor,// (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross+SubAsset)), // - (StTrFrom - StTrTo)

                                 //  stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo +PriAReturn) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss+ SubAReturn)), // + StTrTo
                                 //  stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriAsset) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubAsset)), // + StTrFrom 
                                 cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                                 price = b.price,
                                 stcost = b.pprice,
                                 BasePrice = b.BasePrice,
                                 mrp = b.MRP,
                                 MC = materialcenre.MCName,
                                 CashPrice = b.CashPrice,
                                 CreditPrice = b.CreditPrice,
                             }).ToList();



                        alldata.AddRange(mydata);


                    }
                }
            }

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                alldata = alldata.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = alldata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        [QkAuthorize(Roles = "Dev,Price Search,HireStock Item Wise,Price Search Shorrom Wise")]
        public ActionResult GetItemWisenewsuggshowroom(string[] itemidd, long[] ddmcc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            long[] mcs = ddmcc;
            string[] items = itemidd;

            if (ddmcc == null)
            {
                long[] a = { 0, 1 };
                ddmcc = a;
            }
            var temp = db.MCs.Select(a => a.MCId).ToArray();
            foreach (long ddmc in ddmcc)
            {
                if (ddmc == 0)
                {
                    mcs = temp;
                }
            }


            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            List<forpricesearch> alldata = new List<forpricesearch>();



            foreach (string itemids in items)
            {
                long itemid = db.Items.Where(o => o.ItemName == itemids).Select(o => o.ItemID).FirstOrDefault();
                long[] sugitemid = db.suggestItems.Where(o => o.priitemid == itemid).Select(o => o.sugitemid).ToArray();
                if (sugitemid.Count() == 0)
                {
                    long? priid = db.suggestItems.Where(o => o.sugitemid == itemid).Select(o => o.priitemid).FirstOrDefault();

                    sugitemid = db.suggestItems.Where(o => o.priitemid == priid && o.sugitemid != itemid).Select(o => o.sugitemid).ToArray();
                    if (priid != null)
                    {
                        sugitemid = sugitemid.Concat(new long[] { (long)priid }).ToArray();
                    }
                }
                foreach (long sugid in sugitemid)
                {
                    foreach (long ddmc in mcs)
                    {


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
                                 where (b.ItemID == sugid)
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
                                     pprice = b.PurchasePrice,
                                     cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                                     costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                                     stcost = (decimal?)((stv != null) ? (decimal?)stv.Price : null),
                                     stcostu = (long?)((stv != null) ? (decimal?)stv.Unit : null),
                                     price = b.SellingPrice,
                                     b.BasePrice,
                                     b.MRP
                                 }).Distinct();




                        v = v.OrderBy(b => b.ItemName);
                        recordsTotal = v.Count();
                        var data = v.Skip(skip).Take(pageSize).ToList();
                        var mydata =
                            (from b in data
                             let materialcenre = (from a in db.MCs
                                                  where a.MCId == ddmc
                                                  select new
                                                  {
                                                      a.MCName
                                                  }).FirstOrDefault()
                             let stocks = GetItemWisestock(Convert.ToInt64(b.ItemID), ddmc)
                             select new forpricesearch
                             {
                                 ItemID = b.ItemID,
                                 itemcode = b.ItemCode,

                                 itemname = b.ItemName,

                                 PriUnit = (b.PriUnit != null) ? b.PriUnit : "",

                                 SubUnit = (b.SubUnit != null) ? b.SubUnit : "",

                                 OpeningStock = b.OpeningStock,
                                 MinStock = (b.MinStock != null) ? b.MinStock : 0,
                                 ConFactor = b.ConFactor,



                                 pritotal = stocks * b.ConFactor,// ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)),
                                 subtotal = stocks * b.ConFactor * b.ConFactor,//((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + SubPurchaseCross + SubAReturn) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross +SubAsset)),

                                 total = stocks * b.ConFactor,// (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo + PriPurchaseCross + PriAReturn) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriRetNoteCross + PriAsset)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss + PriPurchaseCross) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubRetNoteCross+SubAsset)), // - (StTrFrom - StTrTo)

                                 //  stockIn = (((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem + PriRetNote + PriHireMiss + PriStTrTo +PriAReturn) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem + SubStTrTo + SubRetNote + SubHireMiss+ SubAReturn)), // + StTrTo
                                 //  stockout = (((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem + PriHDNote + PriStTrFrom + PriAsset) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem + SubStTrFrom + SubHDNote + SubAsset)), // + StTrFrom 
                                 cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor),
                                 price = b.price,
                                 stcost = b.pprice,
                                 BasePrice = b.BasePrice,
                                 mrp = b.MRP,
                                 MC = materialcenre.MCName,
                             }).ToList();



                        alldata.AddRange(mydata);


                    }
                }
            }

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                alldata = alldata.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = alldata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Item Wise")]
        public ActionResult ViewItemWise(string itemid, string ddmc)
        {



            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            int[] items = itemid.Split(',').Select(x => int.Parse(x)).ToArray();
            ViewBag.total = "";
            foreach (int it in items)
                ViewBag.total = ViewBag.total + GetItemByIdstring(Convert.ToInt64(it)) + " : " + GetItemWisestock(Convert.ToInt64(it), 0) + "<br/>";

            return View();
        }


        //for getting the Lastsale details
        [HttpPost]
        public ActionResult GetLastSales(long? ItemId)
        {
            //for drawing the datatable
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            //for sorting the datas in list
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            //end

            //for arrange the pagesize in list 
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            //end

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            //for getting the lastsale datas

            var v = (from b in db.SEItemss
                     join c in db.Items on b.Item equals c.ItemID
                     join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                     from e in punit.DefaultIfEmpty()

                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join s in db.SalesEntrys on b.SalesEntry equals s.SalesEntryId
                     join p in db.Customers on s.Customer equals p.CustomerID
                     where b.Item == ItemId
                     select new
                     {
                         b.SEItemsId,
                         s.SEDate,
                         s.BillNo,
                         CustomerName = p.CustomerName,
                         ItemUnitPrice = b.ItemUnitPrice,
                         ItemQuantity = b.ItemQuantity,
                         ItemSubTotal = b.ItemSubTotal,
                         ItemDiscount = b.ItemDiscount,
                         ItemTax = b.ItemTax,
                         itemNote = b.itemNote,
                         ItemTaxAmount = b.ItemTaxAmount,
                         ItemTotalAmount = b.ItemTotalAmount,
                         ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                         ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                         ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                         PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),

                     });
            //end

            //for sorting
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            //end

            //for catching the datas as pagesize
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            //end
        }


        [HttpPost]
        public ActionResult GetLastPurchase(long? ItemId)
        {
            //for drawing the datatable

            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            //for sorting the datas in list
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            //for arrange the pagesize
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            //for getting the datas

            var v = (from b in db.PEItemss
                     join c in db.Items on b.Item equals c.ItemID

                     join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                     from e in punit.DefaultIfEmpty()

                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join s in db.PurchaseEntrys on b.PurchaseEntry equals s.PurchaseEntryId into secondary
                     from s in secondary.DefaultIfEmpty()
                     join p in db.Suppliers on s.Supplier equals p.SupplierID

                     where b.Item == ItemId
                     select new
                     {
                         b.PEItemsId,
                         s.PEDate,
                         s.PurchaseEntryId,
                         s.BillNo,
                         p.SupplierName,
                         ItemUnitPrice = b.ItemUnitPrice,
                         ItemQuantity = b.ItemQuantity,
                         ItemSubTotal = b.ItemSubTotal,
                         ItemDiscount = b.ItemDiscount,
                         ItemTax = b.ItemTax,
                         itemNote = b.itemNote,
                         ItemTaxAmount = b.ItemTaxAmount,
                         ItemTotalAmount = b.ItemTotalAmount,
                         ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                         ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                         ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                         PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                         //bundleitem = (from ab in db.SEItemss
                         // .
                         //              where ab.itemNote == "-:{Bundle_Item}"
                         //              && b.Item == ab.ItemDiscount
                         //                  ItemCode = bb.ItemCode,
                         //                  ItemName = bb.ItemName,
                         //                  ItemUnit = cb.ItemUnitName,
                         //                  ItemQuantity = ab.ItemQuantity,
                         //              }).ToList()
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            //datas listed like as the pagesize
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        public JsonResult SearchItem(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Items.Where(p => (p.Status == Status.active) && p.ItemName.ToLower().Contains(q.ToLower()) || p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(q) || p.ItemCode.Contains(q) || p.Barcode.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Status == Status.active).Select(b => new SelectFormat
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchItemByPrefix(string q, string x, long px)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Items.Where(p => p.Prefix == px && (p.Status == Status.active) && p.ItemName.ToLower().Contains(q.ToLower()) || p.ItemCode.ToLower().Contains(q.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(q) || p.ItemCode.Contains(q) || p.Barcode.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Prefix == px && p.Status == Status.active).Select(b => new SelectFormat
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpPost]
        public JsonResult Convertionrate(long? currency)
        {
            var conv = currency != null ? db.CurrencyMasters.Where(x => x.Id == currency).Select(y => y.ConvertionRate).FirstOrDefault() : "1";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { conv = conv } };
        }

        [HttpPost]
        public JsonResult ItemPrefixCheck(string Prefix)
        {
            var PrefixChk = db.PrefixMasters.Where(x => x.PrefixCode == Prefix).Any();
            return new QuickSoft.Models.LegacyJsonResult { Data = new { prefix = PrefixChk } };
        }
        [HttpPost]
        public JsonResult ItemCodeCheck(string ItemCode)
        {
            var Itemdata = db.Items.Where(x => x.ItemCode == ItemCode).FirstOrDefault();
            return new QuickSoft.Models.LegacyJsonResult { Data = new { Itemdata } };
        }


        public JsonResult GetItemBetween(int ItemF, int ItemT, int Prefix)
        {
            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.Jewellerys on b.ItemID equals g.Item into jewl
                        from g in jewl.DefaultIfEmpty()
                        where b.Barcode != null
                        && b.ItemID >= ItemF && b.ItemID <= ItemT && b.Prefix == Prefix
                        select new
                        {
                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            PartNumber = b.PartNumber,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.Barcode,
                            b.SupplierRef,

                            g.TagLine1,
                            g.TagLine2,
                            g.TagLine3,
                            g.TagLine4,
                            g.TagLine5,
                        }).ToList();
            return Json(item);

        }

        [HttpPost]
        public ActionResult GetItemContains(long[] array)
        {
            var item = (from b in db.Items
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.Jewellerys on b.ItemID equals g.Item into jewl
                        from g in jewl.DefaultIfEmpty()
                        where b.Barcode != null
                        && array.Contains(b.ItemID)
                        select new
                        {
                            b.ItemCode,
                            b.ItemName,
                            b.ItemArabic,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            PartNumber = b.PartNumber,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            b.Barcode,
                            b.SupplierRef,

                            g.TagLine1,
                            g.TagLine2,
                            g.TagLine3,
                            g.TagLine4,
                            g.TagLine5,
                        }).ToList();
            return Json(item);

        }




        private string ItemCodes(Int64 INo = 0, string ICode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Item").Select(a => a.prefix).FirstOrDefault();
            if (ICode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Item").Select(a => a.number).FirstOrDefault();
                if ((db.Items.Select(p => p.ItemID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        ICode = prefix + 1;
                    }
                    else
                    {
                        ICode = prefix + number;
                    }
                }
                else
                {
                    INo = db.Items.Max(p => p.ItemID + 1);
                    ICode = prefix + INo;
                    if (CodeExist(ICode))
                    {
                        ICode = ItemCodes(INo, ICode);
                    }

                }
            }
            else
            {
                INo = INo + 1;
                ICode = prefix + INo;
                if (CodeExist(ICode))
                {
                    ICode = ItemCodes(INo, ICode);
                }
            }
            return ICode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.Items.Any(c => c.ItemCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        private string PrefixCodes(Int64 pre = 0, Int64 INo = 0, string ICode = null)
        {
            var prefix = (pre != 0) ? db.PrefixMasters.Where(a => a.Id == pre).Select(a => a.PrefixCode).FirstOrDefault() : "";
            if (ICode == null)
            {
                Int64 number = db.ItemPrefixs.Where(a => a.Prefix == pre).Select(a => a.No).AsEnumerable().DefaultIfEmpty(0).Max();
                if ((db.Items.Select(p => p.ItemID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        ICode = prefix + 1;
                    }
                    else
                    {
                        number++;
                        ICode = prefix + number;
                    }
                }
                else
                {
                    INo = number + 1;
                    ICode = prefix + INo;
                    if (PrefixCodeExist(ICode))
                    {
                        ICode = PrefixCodes(pre, INo, ICode);
                    }
                }
            }
            else
            {
                INo = INo + 1;
                ICode = prefix + INo;
                if (PrefixCodeExist(ICode))
                {
                    ICode = PrefixCodes(pre, INo, ICode);
                }
            }

            return ICode;
        }
        private bool PrefixCodeExist(string Code)
        {
            var Exists = db.Items.Any(c => c.ItemCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        public ActionResult BulkUpload()
        {

            var viewModel = new ItemViewModel
            {

                ItemCode = ItemCodes(),
                ItemCategorys = db.ItemCategorys.ToList(),
                ItemBrands = db.ItemBrands.ToList(),
                ItemColors = db.ItemColors.ToList(),
                ItemSizes = db.ItemSizes.ToList(),
                Taxs = db.Taxs.Where(b => b.Status == Status.active).ToList(),
                ItemUnits = db.ItemUnits.ToList()

            };
            return View(viewModel);
        }
        [HttpPost]
        public JsonResult BulkUploadItem(string[][] array)
        {
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var EnableCur = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var EnablePN = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            var EnableCommission = db.EnableSettings.Where(a => a.EnableType == "ItemCommision").FirstOrDefault();
            var EnableCom = EnableCommission != null ? EnableCommission.Status : Status.inactive;
            var EnableBarcode = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var EnableBar = EnableBarcode != null ? EnableBarcode.Status : Status.inactive;
            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            var EnableJew = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;

            var Msg = "";
            bool Stat;
            foreach (var arr in array)
            {
                //Data Analysing Section for Prefix Category Brand Tax and Unit
                var Type = Convert.ToInt32(arr[22]);
                var NewCategory = (arr[10] == null || arr[10] == "") ? 1 : com.GetCategoryId(arr[10]);
                var NewBrand = (arr[11] == null || arr[11] == "") ? 1 : com.GetBrandId(arr[11]);
                var UnitId = (arr[1] == null || arr[1] == "") ? 1 : com.GetUnitId(arr[1]);
                long TaxId = (Convert.ToInt32(arr[5]) == 0) ? 0 : com.GetTaxId(Convert.ToDecimal(arr[5]));
                long size = (arr[13] == null || arr[13] == "") ? 0 : com.GetSizeId(arr[13]);
                long color = (arr[9] == "") ? 0 : com.GetColorId(arr[9]);
                long NewPre = 0;
                string NewPrefix = "";
                var Currency = new CurrencyMaster { Id = 0, ConvertionRate = "" };

                //Item Adding Section
                ItemViewModel itemz = new ItemViewModel();
                if (PreCheck == Status.active && arr[21] != "")
                {
                    NewPre = (arr[21] == "") ? 0 : GetPrefixId(arr[21], NewBrand, NewCategory);
                    NewPrefix = PrefixCode(arr[21]);
                    itemz.Prefix = NewPre;
                    itemz.ItemCode = NewPrefix;
                    itemz.Barcode = NewPrefix;
                }
                else
                {
                    var code = arr[25] != "" ? arr[25] : ItemCodes();
                    itemz.ItemCode = code;
                    itemz.Barcode = Convert.ToString(com.createBarcode());
                }
                if (EnableCur == Status.active)
                {
                    Currency = db.CurrencyMasters.Where(x => x.editable == choice.No).FirstOrDefault();
                }
                itemz.ItemName = arr[0];
                if (EnablePN == Status.active)
                {
                    itemz.PartNumber = arr[6];
                }
                if (Convert.ToDouble(arr[24]) == 0.0)
                {
                    itemz.MinStock = 0;
                    itemz.KeepStock = false;
                    itemz.OpeningStock = Convert.ToDecimal(arr[2]);
                }
                else
                {
                    itemz.MinStock = Convert.ToDecimal(arr[24]);
                    itemz.OpeningStock = Convert.ToDecimal(arr[2]);
                    itemz.KeepStock = true;
                }
                if (EnableCom == Status.active && arr[25] != "")
                {
                    itemz.Commission = Convert.ToDecimal(arr[25]);
                }
                itemz.ItemCategoryID = NewCategory;
                itemz.ItemBrandID = NewBrand;
                itemz.SupplierRef = arr[23];
                itemz.PurchasePrice = Convert.ToDecimal(arr[12]);
                itemz.MRP = Convert.ToDecimal(arr[3]);
                itemz.SellingPrice = Convert.ToDecimal(arr[14]);
                itemz.BasePrice = Convert.ToDecimal(arr[4]);
                //time being tax direct given //2 = 5% tax and 1 = 0 tax
                itemz.TaxID = TaxId;
                itemz.ItemType = Type;
                itemz.ConFactor = 1;
                if (EnableJew == Status.active)
                {
                    itemz.Tagline1 = arr[16];
                    itemz.Tagline2 = arr[17];
                    itemz.Tagline3 = arr[18];
                    itemz.Tagline4 = arr[19];
                    itemz.Tagline5 = arr[20];
                }
                itemz.ItemDescription = arr[7];
                if (size != 0)
                {
                    itemz.ItemSizeID = size;
                }
                if (color != 0)
                {
                    itemz.ItemColorID = color;
                }
                itemz.ItemUnitID = (Type != 1) ? 5 : UnitId;
                itemz.Currency = Currency.Id;
                itemz.ConRate = Convert.ToDecimal(Currency.ConvertionRate);
                itemz.Status = 0;
                var id = com.Item(itemz);
            }
            Msg = "Success";
            Stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = Stat, message = Msg } };
        }

        public long GetPrefixId(string Prefix, long Brand, long Category)
        {
            var UserId = User.Identity.GetUserId();
            long branch = 0;
            Int64 preID = 0;
            branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            var Exists = db.PrefixMasters.Any(c => c.PrefixCode == Prefix);
            if (Exists)
            {
                preID = db.PrefixMasters.Where(c => c.PrefixCode == Prefix).Select(c => c.Id).FirstOrDefault();
            }
            else
            {
                PrefixMaster pre = new PrefixMaster();
                pre.PrefixCode = Prefix;
                pre.Branch = branch;
                pre.Brand = Brand;
                pre.Category = Category;
                pre.ConRate = Convert.ToString(1);
                pre.editable = choice.Yes;
                pre.CreatedBy = UserId;
                pre.CreatedDate = System.DateTime.Now;
                db.PrefixMasters.Add(pre);
                db.SaveChanges();
                preID = pre.Id;
                ItemPrefix it = new ItemPrefix();
                it.No = 1;
                it.Prefix = preID;
                db.ItemPrefixs.Add(it);
                db.SaveChanges();
            }
            return preID;
        }


        [HttpGet]
        public virtual ActionResult DownloadExcel(string file)
        {
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var EnableCur = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var EnablePN = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            var EnableCommission = db.EnableSettings.Where(a => a.EnableType == "ItemCommision").FirstOrDefault();
            var EnableCom = EnableCommission != null ? EnableCommission.Status : Status.inactive;
            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            var JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;
            string fullPath = "";
            string fileName = "";
            if (JewCheck == Status.active)
            {
                fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/ItemJewellery.xlsx"));
                fileName = "ItemJewellery.xlsx";
            }
            else
            {
                fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/GeneralItem.xlsx"));
                fileName = "GeneralItem.xlsx";
            }

            return File(fullPath, "application/vnd.ms-excel", fileName);
        }

        private string PrefixCode(string Prefix)
        {
            long Preid = 0;
            var NewPrefix = "";
            Preid = db.PrefixMasters.Where(x => x.PrefixCode == Prefix).Select(y => y.Id).FirstOrDefault();
            if (Preid != 0)
            {
                long lastid = 0;
                if (Preid != 0)
                {
                    lastid = db.ItemPrefixs.Where(x => x.Prefix == Preid).Select(x => x.No).Max();
                }
                var newprecode = Prefix + (lastid + 1);
                Regex re = new Regex(@"([a-zA-Z]+)(\d+)");
                Match result = re.Match(newprecode);
                string alphaPart = result.Groups[1].Value;
                string num = result.Groups[2].Value;
                int length = (result.Groups[2].Value).Length;

                if (length < 5)
                {
                    string s = new String('0', 5 - length);
                    string newnum = s + num;
                    NewPrefix = alphaPart + newnum;
                }
                else
                {
                    NewPrefix = Convert.ToString(lastid);
                }
            }
            return NewPrefix;
        }


        public ActionResult GetItemSales(string ItemCodes)
        {
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();
            var v = (from f in db.SalesEntrys
                     join d in db.SEItemss on f.SalesEntryId equals d.SalesEntry
                     join e in db.Items on d.Item equals e.ItemID
                     where e.ItemCode == ItemCodes
                     && (userpermission == true || f.CreatedBy == UserId)
                     select new
                     {
                         f.SalesEntryId,
                         Invoice = f.BillNo,
                         SEDate = f.SEDate,
                         SaleType = f.SaleType,
                         Discount = d.ItemDiscount,
                         Tax = d.ItemTaxAmount,
                         GrandTotal = d.ItemTotalAmount,
                         Quantity = d.ItemQuantity
                     });
            v = v.OrderByDescending(c => c.SEDate).Take(10);
            var data = v.ToList();
            return Json(new { data = data });

        }

        public ActionResult GetItemPurchase(string ItemCodes)
        {
            var userpermission = User.IsInRole("All Purchase Entry");
            var UserId = User.Identity.GetUserId();
            var v = (from f in db.PurchaseEntrys
                     join d in db.PEItemss on f.PurchaseEntryId equals d.PurchaseEntry
                     join e in db.Items on d.Item equals e.ItemID
                     join g in db.Suppliers on f.Supplier equals g.SupplierID
                     where e.ItemCode == ItemCodes
                     && (userpermission == true || f.CreatedBy == UserId)
                     select new
                     {
                         f.PurchaseEntryId,
                         Invoice = f.BillNo,
                         SEDate = f.PEDate,
                         Discount = d.ItemDiscount,
                         Tax = d.ItemTaxAmount,
                         GrandTotal = d.ItemTotalAmount,
                         Quantity = d.ItemQuantity,
                         Supplier = g.SupplierName
                     });
            v = v.OrderByDescending(c => c.SEDate).Take(10);
            var data = v.ToList();
            return Json(new { data = data });

        }

        public JsonResult SearchDetailsBySP(string q, long? cust, long? mc, string constat)
        {
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
            var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            var ItemN = new SqlParameter("@ItemName", q);
            var Status = new SqlParameter("@StatusItemDetailsLists", null);
            var Customr = (cust != null) ? new SqlParameter("@Customer", cust) : null;
            var MaterialC = (mc != null) ? new SqlParameter("@MaterialCenter", mc) : null;
            var chc = new SqlParameter("@chec", check);

            var item = db.Database.SqlQueryRaw<ItemData>("ItemDetailsLists @ItemName, @chec, @MaterialCenter",
                                                                              ItemN, chc, MaterialC).AsEnumerable().ToList();

            var data = item.Select(o => new
            {
                o.text,
                o.id,
                pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriStockAdjAdd + o.PriProdItem + o.PriUnasItem) - (o.Prisale + o.PriPReturn + o.PriStockAdjLess + o.PriProdItem + o.PriUnasItem)),
                subtotal = ((o.SubPurchase + o.SubSReturn + o.SubStockAdjAdd + o.SubProdItem + o.SubUnasItem) - (o.Subsale + o.SubPReturn + o.SubStockAdjLess + o.SubProdItem + o.SubUnasItem)),
                total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriStockAdjAdd + o.PriProdItem + o.PriUnasItem) - (o.Prisale + o.PriPReturn + o.PriStockAdjLess + o.PriProdItem + o.PriUnasItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubStockAdjAdd + o.SubProdItem + o.SubUnasItem) - (o.Subsale + o.SubPReturn + o.SubStockAdjLess + o.SubProdItem + o.SubUnasItem)),

                unit = o.ItemUnitID,
                price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                o.KeepStock,
                MinStock = (o.MinStock != null) ? o.MinStock : 0,
                Tax = o.Percentage,
                lastSale = (ItemlastSalesPrice == true) ? o.SEunitprice : (decimal?)null,
                lastSaleU = o.SEunit,
                lastPur = (ItemlastPurchasePrice == true) ? o.PEunitprice : (decimal?)null,
                lastPurU = o.PEunit,
                g = o.SE,

                o.ItemCode,
                o.ItemName,
                o.ItemArabic,
                o.Barcode,
                o.SubUnitId,
                PartNumber = (o.PartNumber != null) ? o.PartNumber : "",
                PriUnit = (o.ItemUnitName != null) ? o.ItemUnitName : "",
                SubUnit = (o.SubUnitName != null) ? o.SubUnitName : "",
                o.ConFactor,
                //o.SellingPrice,
                //o.PurchasePrice,
                //o.BasePrice,
                //o.MRP,

                cust = cust,
            }).OrderBy(b => b.text).ToList();
            return Json(data);
        }

        #region batch stock

        public JsonResult SearchBatch(string q, string x, long? itemid)
        {
            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 4;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 4 : 0;
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            var itemq = (
                from a in db.BatchStocks
                join b in db.Items on a.Item equals b.ItemID
                join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                from c in primary.DefaultIfEmpty()
                join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                from d in second.DefaultIfEmpty()
                where a.Item == itemid && (check == true || a.BatchNo.Contains(q))
                select new
                {
                    text = a.BatchNo,
                    id = a.BatchNo,
                    b.SellingPrice,
                    b.PurchasePrice,
                    b.BasePrice,
                    b.MRP,
                    b.MinStock,
                    b.KeepStock,
                    b.ItemUnitID,
                    b.ItemCode,
                    b.ItemName,
                    b.ItemArabic,
                    b.SubUnitId,
                    PriUnit = c.ItemUnitName,
                    SubUnit = d.ItemUnitName,
                    ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                    b.Barcode,
                    StockIn = (decimal?)(from v in db.BatchStocks
                                         where v.Item == a.Item && v.BatchNo == a.BatchNo
                                         select new { v.StockIn }).Sum(c => c.StockIn) ?? 0,
                    StockOut = (decimal?)(from v in db.BatchStocks
                                          where v.Item == a.Item && v.BatchNo == a.BatchNo
                                          select new { v.StockOut }).Sum(c => c.StockOut) ?? 0,
                }).Distinct();
            var vd = itemq.OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            var data = (from o in vd
                        select new
                        {
                            o.text,
                            o.id,
                            unit = o.ItemUnitID,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            cost = o.PurchasePrice,
                            o.KeepStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            o.Barcode,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.ConFactor,
                            Stock = o.StockIn - o.StockOut,
                            o.StockIn,
                            o.StockOut
                        }).OrderBy(b => b.text).ToList();
            return Json(data);
        }
        public string Getdescription(long itemid)
        {
            string desc = db.Items.Find(itemid).ItemDescription;



            return desc;
        }
        public JsonResult GetTotal(long?[] itmid)
        {
            var data = "";
            foreach (int it in itmid)
            {
                data = data + GetItemByIdstring(Convert.ToInt64(it)) + "  Stock : " + GetItemWisestock(Convert.ToInt64(it), 0);
                data = data + " Sel.Price :" + GetSelingPrice(Convert.ToInt64(it)).ToString();
                if (User.IsInRole("ItemPurchasePrice"))
                {
                    data = data + " Pur.Price :" + getpurprice(Convert.ToInt64(it)).ToString();

                }
                data = data + "  Cash.Price:" + GetCashPrice(Convert.ToInt64(it)).ToString();
                data = data + "  Credit.Price:" + GetCreditPrice(Convert.ToInt64(it)).ToString();
                data = data + "\n";
            }


            return Json(data);
        }
        public JsonResult GetTotal2(long?[] itmid)
        {
            var data = "";

            foreach (int it in itmid)
            {
                var ImageBag = (from b in db.ItemImages
                                where b.ItemID == it
                                select new
                                {
                                    ImgId = b.ItemImageID,
                                    FileName = b.FileName,
                                    ItemImId = b.ItemID
                                }).FirstOrDefault();
                if (ImageBag != null)
                {


                    data = data + "<div class='col-md-4'><img src='/uploads/itemimages/" + ImageBag.ItemImId + "/" + ImageBag.FileName + "'/></div>";


                }

                //              where b.ItemID == itmid[0]
                //                  DocId = b.ItemDocumentID,
                //                  FileName = b.FileName,
                //                  ItemDoId = b.ItemID,


                List<AdditionaldocViewModel> DocBaglst = (from b in db.ItemDocuments
                                                          where b.ItemID == it
                                                          select new AdditionaldocViewModel
                                                          {
                                                              DocId = b.ItemDocumentID,
                                                              FileName = b.FileName,
                                                              ItmDocId = b.ItemID,

                                                          }).ToList();


                var LstAdditionalImages = (from a in db.AttachmentDocuments
                                           where (a.TransactionID == it & a.TransactionType == "Item")
                                           select new AdditionalImageViewModel
                                           {
                                               DocumentID = a.DocumentID,
                                               ItemId = a.TransactionID,
                                               FileName = a.FileName,
                                               FileNameSaved = a.FileName,
                                           }).ToList();

                foreach (var ad in LstAdditionalImages)
                {



                    data = data + "<div class='col-md-4'><img src='/uploads/itemimages/AdditionalImages/" + ad.ItemId + "/" + ad.FileName + "'/></div>";

                }
                if (DocBaglst.Count() > 0)
                {
                    data = data + "<br><h3>Document List</h3><table>";
                }
                foreach (var ad in DocBaglst)
                {

                    data = data + "<tr><td> <a href='/uploads/itemdocuments/" + ad.ItmDocId + "/" + ad.FileName + "'  target='_blank'>" + ad.FileName + "</a></td></tr>";





                }
                if (DocBaglst.Count() > 0)
                {
                    data = data + "</table>";
                }
            }


            return Json(data);
        }
        public JsonResult GetBatch(string BatchNo, long? itemid)
        {

            var itemq = (
                from a in db.BatchStocks
                join b in db.Items on a.Item equals b.ItemID
                join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                from c in primary.DefaultIfEmpty()
                join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                from d in second.DefaultIfEmpty()
                where a.Item == itemid && a.BatchNo == BatchNo
                select new
                {
                    text = a.BatchNo,
                    id = a.BatchNo,
                    b.SellingPrice,
                    b.PurchasePrice,
                    b.BasePrice,
                    b.MRP,
                    b.MinStock,
                    b.KeepStock,
                    b.ItemUnitID,
                    b.ItemCode,
                    b.ItemName,
                    b.ItemArabic,
                    b.SubUnitId,
                    PriUnit = c.ItemUnitName,
                    SubUnit = d.ItemUnitName,
                    ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                    b.Barcode,
                    a.MFG,
                    a.EXP,
                    StockIn = (decimal?)(from v in db.BatchStocks
                                         where v.Item == a.Item && v.BatchNo == a.BatchNo
                                         select new { v.StockIn }).Sum(c => c.StockIn) ?? 0,
                    StockOut = (decimal?)(from v in db.BatchStocks
                                          where v.Item == a.Item && v.BatchNo == a.BatchNo
                                          select new { v.StockOut }).Sum(c => c.StockOut) ?? 0,

                }).Distinct();
            var vd = itemq.ToList();
            var data = (from o in vd
                        select new
                        {
                            o.text,
                            o.id,
                            unit = o.ItemUnitID,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            cost = o.PurchasePrice,
                            o.KeepStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemArabic,
                            o.Barcode,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.ConFactor,
                            Stock = o.StockIn - o.StockOut,
                            o.StockIn,
                            o.StockOut,
                            o.EXP,
                            o.MFG
                        }).FirstOrDefault();
            return Json(data);
        }

        #endregion

        public JsonResult SearchItemDetailsByMC(string q, string x, long? cust, long? mc, string constat)
        {
            var StockItemsPerm = User.IsInRole("List Stockable Items");
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var stockcheckinvoice = db.EnableSettings.Where(a => a.EnableType == "stockcheckinvoice").FirstOrDefault();
            var stockcheck = stockcheckinvoice != null ? stockcheckinvoice.Status : Status.inactive;
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));

            var ItemSalesPrice = User.IsInRole("ItemSalesPrice");
            var ItemPurchasePrice = User.IsInRole("ItemPurchasePrice");
            var ItemlastSalesPrice = User.IsInRole("ItemlastSalesPrice");
            var ItemlastPurchasePrice = User.IsInRole("ItemlastPurchasePrice");

            var start = Request.Form.GetValues("page").FirstOrDefault();
            int pageSize = 10;
            int skip = start != null ? (Convert.ToInt32(start) - 1) * 10 : 0;
            string[] searchkey = q.Split(' ');

            string secnd = "";
            string third = "";

            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }

            if (stockcheck == Status.inactive)
            {
                var item1 = (from a in db.ItemTransactions
                             join b in db.Items on a.ItemId equals b.ItemID
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                             from f in taxss.DefaultIfEmpty()
                             join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                             equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                             from g in pur.DefaultIfEmpty()
                             join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                             equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                             from h in sale.DefaultIfEmpty()
                             where (b.Status == Status.active &&
                             (check == true || b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || (b.PartNumber.ToLower().Contains(q.ToLower()) || (b.PartNumber.Contains(q)) && PartNoCheck == Status.active))
                              && (secnd == "" || b.ItemName.ToLower().Contains(secnd.ToLower()))
                              && (third == "" || b.ItemName.ToLower().Contains(third.ToLower()))
                             )
                              && (mc == 0 || mc == a.McId)
                             select new
                             {
                                 text = b.ItemCode + " - " + b.ItemName,
                                 id = b.ItemID,
                                 b.SellingPrice,
                                 b.PurchasePrice,
                                 b.BasePrice,
                                 b.MRP,
                                 b.MinStock,
                                 b.KeepStock,
                                 Tax = f.Percentage,
                                 b.ItemUnitID,
                                 g = h,
                                 PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                                 lastSale = (decimal?)h.ItemUnitPrice,
                                 lastSaleU = (decimal?)h.ItemUnit,

                                 lastPur = (decimal?)g.ItemUnitPrice,
                                 lastPurU = (decimal?)g.ItemUnit,

                                 b.ItemCode,
                                 b.ItemName,
                                 b.ItemArabic,
                                 b.SubUnitId,
                                 PriUnit = c.ItemUnitName,
                                 SubUnit = d.ItemUnitName,
                                 ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                 b.Barcode,
                                 a.TotalStock,
                                 OpeningStock = ((mc == 0 || mc == 1 || mc == null) && b.OpeningStock != null) ? b.OpeningStock : 0,

                                 PriPurchase = (decimal?)(from v in db.PEItemss
                                                          join w in db.PurchaseEntrys on v.PurchaseEntry equals w.PurchaseEntryId
                                                          where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                          && (mc == 0 || mc == w.MaterialCenter)
                                                          select new
                                                          {
                                                              v.ItemQuantity
                                                          }).Sum(c => c.ItemQuantity) ?? 0,

                                 PriSale = (decimal?)(from v in db.SEItemss
                                                      join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                      where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                      && (mc == 0 || mc == w.MaterialCenter)
                                                      && (w.SaleType != SaleType.Hire)
                                                      select new
                                                      {
                                                          v.ItemQuantity
                                                      }).Sum(c => c.ItemQuantity) ?? 0,


                                 PriPReturn = (decimal?)(from v in db.PRItemss
                                                         join w in db.PurchaseReturns on v.PurchaseReturnId equals w.PurchaseReturnId
                                                         where v.Item == b.ItemID && v.ItemUnit == b.ItemUnitID
                                                         && (mc == 0 || mc == w.MaterialCenter)
                                                         select new
                                                         {
                                                             v.ItemQuantity
                                                         }).Sum(c => c.ItemQuantity) ?? 0,
                             }).Distinct();

                var vd = item1.ToList();
                var data = (from o in vd
                            select new
                            {
                                o.text,
                                o.id,
                                unit = o.ItemUnitID,
                                price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                                cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                                o.KeepStock,
                                MinStock = (o.MinStock != null) ? o.MinStock : 0,
                                o.Tax,
                                lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
                                lastSaleU = o.lastSaleU,
                                lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
                                lastPurU = o.lastPurU,
                                o.g,
                                o.ItemCode,
                                o.ItemName,
                                o.ItemArabic,
                                o.Barcode,
                                o.SubUnitId,
                                PartNumber = o.PartNumber,
                                PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                o.ConFactor,
                                total = o.TotalStock + (o.OpeningStock * o.ConFactor),
                                cust = cust,
                                o.PriSale,
                                o.PriPurchase,
                                o.PriPReturn,
                                stockcheck = 1
                            }).OrderBy(b => b.text).ToList();

                if (constat == "SalesEntry" && StockItemsPerm == true)
                {
                    data = data.Where(a => a.KeepStock == false || (a.KeepStock == true && a.total > 0)).Skip(skip).Take(pageSize).ToList();
                }
                return Json(data);
            }
            else
            {
                var item2 = (from a in db.ItemTransactions
                             join b in db.Items on a.ItemId equals b.ItemID
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                             from f in taxss.DefaultIfEmpty()
                             join g in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(z => z.Item == b.ItemID && z.ItemUnitPrice != 0).Select(z => z.PEItemsId).Max() }
                             equals new { f1 = g.Item, f2 = g.PEItemsId } into pur
                             from g in pur.DefaultIfEmpty()
                             join h in db.SEItemss on new { f1 = b.ItemID, f2 = db.SEItemss.Join(db.SalesEntrys, seitem => seitem.SalesEntry, sale => sale.SalesEntryId, (seitem, sale) => new { seitem, sale }).Where(z => z.seitem.Item == b.ItemID && z.seitem.ItemUnitPrice != 0 && z.sale.Customer == cust).Select(z => z.seitem.SEItemsId).Max() }
                             equals new { f1 = h.Item, f2 = h.SEItemsId } into sale
                             from h in sale.DefaultIfEmpty()
                             where (b.Status == Status.active && (check == true ||
                             b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || (b.PartNumber.ToLower().Contains(q.ToLower()) || (b.PartNumber.Contains(q))
                             && PartNoCheck == Status.active))
                             && (secnd == "" || b.ItemName.ToLower().Contains(secnd.ToLower()))
                             && (third == "" || b.ItemName.ToLower().Contains(third.ToLower()))
                             )
                             && (mc == 0 || mc == a.McId)
                             select new
                             {
                                 text = b.ItemCode + " - " + b.ItemName,
                                 id = b.ItemID,
                                 b.SellingPrice,
                                 b.PurchasePrice,
                                 b.BasePrice,
                                 b.MRP,
                                 b.MinStock,
                                 b.KeepStock,
                                 Tax = f.Percentage,
                                 b.ItemUnitID,
                                 g = h,
                                 PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",

                                 lastSale = (decimal?)h.ItemUnitPrice,
                                 lastSaleU = (decimal?)h.ItemUnit,

                                 lastPur = (decimal?)g.ItemUnitPrice,
                                 lastPurU = (decimal?)g.ItemUnit,
                                 b.ItemCode,
                                 b.ItemName,
                                 b.ItemArabic,
                                 b.SubUnitId,
                                 PriUnit = c.ItemUnitName,
                                 SubUnit = d.ItemUnitName,
                                 ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                                 b.Barcode,

                                 OpeningStock = 0,

                                 PriPurchase = 0,

                                 SubPurchase = 0,

                                 PriSale = 0,

                                 SubSale = 0,

                                 PriPReturn = 0,

                                 SubPReturn = 0,

                                 PriSReturn = 0,

                                 SubSReturn = 0,

                                 //stock adjustment---
                                 PriAddAdj = 0,
                                 SubAddAdj = 0,

                                 PriLessAdj = 0,
                                 subLessAdj = 0,
                                 //-------
                                 // production ----
                                 PriProdItem = 0,
                                 SubProdItem = 0,
                                 // compined item
                                 PriProdCItem = 0,

                                 SubProdCItem = 0,

                                 // main item
                                 PriUnItem = 0,
                                 SubUnItem = 0,
                                 // compined item
                                 PriUnCItem = 0,

                                 SubUnCItem = 0,
                                 // stock transfer

                             }).Distinct();

                var vd = item2.OrderBy(b => b.ItemName).ToList();

                var data = (from o in vd
                            let PriStTrFrom = 0
                            let PriStTrTo = 0
                            let SubStTrFrom = 0
                            let SubStTrTo = 0
                            select new
                            {
                                o.text,
                                o.id,
                                unit = o.ItemUnitID,
                                price = (ItemSalesPrice == true) ? (o.SellingPrice != 0) ? o.SellingPrice : o.MRP : (decimal?)null,
                                cost = (ItemPurchasePrice == true) ? o.PurchasePrice : (decimal?)null,
                                o.KeepStock,
                                MinStock = (o.MinStock != null) ? o.MinStock : 0,
                                o.Tax,
                                lastSale = (ItemlastSalesPrice == true) ? o.lastSale : (decimal?)null,
                                lastSaleU = o.lastSaleU,
                                lastPur = (ItemlastPurchasePrice == true) ? o.lastPur : (decimal?)null,
                                lastPurU = o.lastPurU,
                                o.g,
                                o.ItemCode,
                                o.ItemName,
                                o.ItemArabic,
                                o.Barcode,
                                o.SubUnitId,
                                PartNumber = o.PartNumber,
                                PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                                o.ConFactor,

                                pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom)),
                                subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom)),
                                total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem + PriStTrTo) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem + PriStTrFrom)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem + SubStTrTo) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem + SubStTrFrom)),

                                cust = cust,
                                o.PriSale,
                                o.PriPurchase,
                                o.PriPReturn,
                                stockcheck = 0
                            }).OrderBy(b => b.text).ToList();
                if (constat == "SalesEntry" && StockItemsPerm == true)
                {
                    data = data.Where(a => a.KeepStock == false || (a.KeepStock == true && a.total > 0)).Skip(skip).Take(pageSize).ToList();
                }

                return Json(data);
            }
        }

        public JsonResult SearchItemByPurchase(string q, string x, long PurchEntryId)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Items
                                  join b in db.PEItemss on a.ItemID equals b.Item
                                  where (b.PurchaseEntry == PurchEntryId && a.slreq == true) &&
                                  (q == null || a.ItemName.ToLower().Contains(q.ToLower()) || a.ItemName.Contains(q) || a.ItemCode.ToLower().Contains(q.ToLower()) || a.ItemCode.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.ItemCode + "-" + a.ItemName,
                                      id = a.ItemID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Items
                                  join b in db.PEItemss on a.ItemID equals b.Item
                                  where (b.PurchaseEntry == PurchEntryId && a.slreq == true)
                                  select new SelectFormat
                                  {
                                      text = a.ItemCode + "-" + a.ItemName,
                                      id = a.ItemID
                                  }).OrderBy(b => b.text).ToList();
            }
            if(serialisedJson==null||serialisedJson.Count()<1)
            {

                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = (from a in db.Items
                                      join b in db.DummyPEItems2 on a.ItemID equals b.Item
                                      where (b.PurchaseEntry == PurchEntryId && a.slreq == true) &&
                                      (q == null || a.ItemName.ToLower().Contains(q.ToLower()) || a.ItemName.Contains(q) || a.ItemCode.ToLower().Contains(q.ToLower()) || a.ItemCode.Contains(q))
                                      select new SelectFormat
                                      {
                                          text = a.ItemCode + "-" + a.ItemName,
                                          id = a.ItemID
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = (from a in db.Items
                                      join b in db.DummyPEItems2 on a.ItemID equals b.Item
                                      where (b.PurchaseEntry == PurchEntryId && a.slreq == true)
                                      select new SelectFormat
                                      {
                                          text = a.ItemCode + "-" + a.ItemName,
                                          id = a.ItemID
                                      }).OrderBy(b => b.text).ToList();
                }

            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
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
