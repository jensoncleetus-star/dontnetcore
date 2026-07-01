using QuickSoft.Web;
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
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ItemBundleController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ItemBundleController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [QkAuthorize(Roles = "Dev,ItemBundle List")]
        public ActionResult Index()
        {
            var IndexList = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.ItemCatg = IndexList;
            ViewBag.ItemSrch = IndexList;
            ViewBag.KStock = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All"},
                new SelectListItem() {Text = "Yes", Value=true.ToString()},
                new SelectListItem() {Text = "No", Value=false.ToString()},
            }, "Value", "Text");

            var Tax = db.Taxs
                  .Select(s => new
                  {
                      Id = s.TaxID,
                      TaxName = s.TaxName
                  }).ToList();
            ViewBag.ItemTax = QkSelect.List(Tax, "Id", "TaxName");
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,ItemBundle List")]
        public ActionResult GetItemBundle(long? Item, long? Category, string btype, long? Tax, bool? KpStock)
        {
            var kst = (KpStock == null) ? null : (KpStock);

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

            var uDev = User.IsInRole("Dev");
            var uItemBundleView = User.IsInRole("View ItemBundle");

            var v = (from a in db.ItemBundles
                     join i in db.Items on a.mainItem equals i.ItemID
                     join b in db.ItemCategorys on i.ItemCategoryID equals b.ItemCategoryID into cat
                     from b in cat.DefaultIfEmpty()
                     join c in db.Taxs on i.TaxID equals c.TaxID into tax
                     from c in tax.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     where (Item == null || Item == 0 || i.ItemID == Item) &&
                         (Category == null || Category == 0 || b.ItemCategoryID == Category) &&
                         (KpStock == null || i.KeepStock == kst) &&
                         (Tax == null || Tax == 0 || i.TaxID == Tax) &&
                         (btype == null || btype == "0" || a.BundleType == btype)
                     select new
                     {
                         a.ItemBundleId,
                         i.ItemCode,
                         i.ItemName,
                         i.Barcode,
                         Category = b.ItemCategoryName,
                         Tax = c.TaxName,
                         ActualCost = i.PurchasePrice,
                         ActualPrice = i.MRP,
                         i.SellingPrice,
                         e.UserName,
                         a.BundleType,
                         Dev = uDev,
                         Details = uItemBundleView
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.ItemCode.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create ItemBundle")]
        public ActionResult Create()
        {
            ViewBag.Item = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                           }, "Value", "Text", 1);

            var cat = db.ItemCategorys.Select(s => new
            {
                ItemCategoryID = s.ItemCategoryID,
                ItemCategoryName = s.ItemCategoryName
            }).ToList();
            ViewBag.Category = QkSelect.List(cat, "ItemCategoryID", "ItemCategoryName");

            var taxs = db.Taxs.Select(s => new
            {
                TaxID = s.TaxID,
                TaxName = s.TaxName
            }).ToList();
            ViewBag.Tax = QkSelect.List(taxs, "TaxID", "TaxName");

            var vmodel = new ItemBundleViewModel
            {
                ItemCode = ItemCodes(),
                HireType = db.HireTypes.ToList()
                //Barcode = enable == true ? createBarcode().ToString() : ""

            };
            ViewBag.LastEntry = db.ItemBundles.Select(a => a.ItemBundleId).AsEnumerable().DefaultIfEmpty(0).Max();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View(vmodel);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create ItemBundle")]
        public JsonResult Create(ItemBundleViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var ItemCodeExists = db.Items.Any(u => u.ItemCode == vmodel.ItemCode);
            if (ItemCodeExists)
            {
                msg = "A Item with same code exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    DateTime? sDate = null;
                    DateTime? eDate = null;
                    if (vmodel.StartDate != null)
                    {
                        sDate = DateTime.Parse(vmodel.StartDate.ToString(), new CultureInfo("en-GB"));
                    }
                    if (vmodel.EndDate != null)
                    {
                        eDate = DateTime.Parse(vmodel.EndDate.ToString(), new CultureInfo("en-GB"));
                    }

                    var item = new Item
                    {
                        ItemCode = vmodel.ItemCode,
                        ItemName = vmodel.ItemName,
                        //ItemArabic = vmodel.ItemArabic,
                        ItemDescription = vmodel.Note,
                        SellingPrice = vmodel.SellingPrice,
                        PurchasePrice = vmodel.ActualCost,
                        MRP = vmodel.ActualPrice,
                        //BasePrice = vmodel.BasePrice,
                        KeepStock = vmodel.KeepStock,
                        ItemCategoryID = vmodel.ItemCategoryID,
                        ItemBrandID = null,

                        ItemColorID = null,
                        ItemSizeID = null,
                        TaxID = vmodel.TaxID,
                        CreatedBy = 1,
                        Status = Status.active,
                        OpeningStock = vmodel.StockQuantity,
                        MinStock = 0,
                        //------------------------------------
                        ItemType = 1,
                        CreatedUserID = User.Identity.GetUserId(),
                        ConFactor = 1
                        //Barcode = vmodel.Barcode
                    };
                    db.Items.Add(item);
                    db.SaveChanges();
                    Int64 ItemID = item.ItemID;

                    var CurrntDate = Convert.ToDateTime(System.DateTime.Now);
                    /************************* ItemTransaction ***********************/
                    //add Quantity as 0 to ItemTransaction Table(in MainCenter)
                    com.AddItemTransaction(ItemID, 1, 0, UserId, CurrntDate);
                    /*****************************************************************/

                    if (vmodel.ImgFileName != null)
                    {
                        foreach (IFormFile file in vmodel.ImgFileName)
                        {
                            //Checking file is available to save.  
                            if (file != null)
                            {
                                var ProdImg = new ItemImage
                                {
                                    ItemID = ItemID,
                                    FileName = Path.GetFileName(file.FileName),
                                    Status = 1
                                };
                                db.ItemImages.Add(ProdImg);
                                db.SaveChanges();
                                string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + ItemID);
                                if (!Directory.Exists(storePath))
                                    Directory.CreateDirectory(storePath);
                                var InputFileName = Path.GetFileName(file.FileName);
                                var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                                //Save file to server folder  
                                file.SaveAs(ServerSavePath);
                            }
                        }
                    }

                    ItemBundle items = new ItemBundle();
                    items.StartDate = sDate;
                    items.EndDate = eDate;
                    items.CreatedDate = today;
                    items.CreatedBy = UserId;
                    items.Status = Status.active;
                    items.Branch = BranchID;
                    items.mainItem = ItemID;
                    items.BundleType = vmodel.BundleType;

                    //    // files upload
                    //    var uploadUrl = LegacyWeb.MapPath("~/uploads/bundleitem/");



                    db.ItemBundles.Add(items);
                    db.SaveChanges();
                    Int64 BundleID = items.ItemBundleId;


                    var brcode = new Barcode
                    {
                        BarcodeNumber = vmodel.Barcode,
                        ItemID = ItemID
                    };
                    db.Barcodes.Add(brcode);
                    db.SaveChanges();



                    BundleItem bnItem = new BundleItem();
                    foreach (var arr in vmodel.bundleitem)
                    {
                        if (arr.ItemId != 0)
                        {
                            bnItem.ItemBundle = BundleID;
                            bnItem.ItemId = arr.ItemId;
                            bnItem.ItemUnit = arr.ItemUnit;
                            bnItem.ItemUnitPrice = arr.ItemUnitPrice;
                            bnItem.ItemQuantity = arr.ItemQuantity;
                            bnItem.ItemSubTotal = arr.ItemSubTotal;
                            bnItem.ItemTax = arr.ItemTax;
                            bnItem.ItemTaxAmount = arr.ItemTaxAmount;
                            bnItem.ItemTotalAmount = arr.ItemTotalAmount;

                            db.BundleItems.Add(bnItem);
                            db.SaveChanges();
                        }
                    }

                    if (vmodel.HireTypes != null)
                    {
                        foreach (HireTypeViewModel Hire in vmodel.HireTypes)
                        {
                            var rate = new HireRate
                            {
                                type = Hire.type,
                                Rate = Hire.Rate,
                                ItemId = ItemID
                            };
                            db.HireRates.Add(rate);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Created, UserId, "ItemBundle", "ItemBundles", findip(), ItemID, "ItemBundle Added Successfully");



                    msg = "Successfully added ItemBundle details.";
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

        }


        [QkAuthorize(Roles = "Dev,Edit ItemBundle")]
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemBundle item = db.ItemBundles.Find(id);
            Item items = db.Items.Find(item.mainItem);
            if (item == null)
            {
                return NotFound();
            }
            ItemBundleViewModel vmodel = new ItemBundleViewModel();
            vmodel.ItemBundleId = item.ItemBundleId;
            vmodel.ItemName = items.ItemName;
            vmodel.ItemCode = items.ItemCode;
            vmodel.ActualCost = items.PurchasePrice;
            vmodel.ActualPrice = items.MRP;
            vmodel.SellingPrice = items.SellingPrice;
            vmodel.KeepStock = items.KeepStock;
            vmodel.StockQuantity = items.OpeningStock;

            vmodel.mainItem = item.mainItem;
            vmodel.BundleType = item.BundleType;
            if (item.StartDate != null)
            {
                vmodel.StartDate = Convert.ToDateTime(item.StartDate).ToString("dd-MM-yyyy");
            }
            if (item.EndDate != null)
            {
                vmodel.EndDate = Convert.ToDateTime(item.EndDate).ToString("dd-MM-yyyy");
            }
            vmodel.ItemCategoryID = items.ItemCategoryID;
            vmodel.TaxID = items.TaxID;
            vmodel.Note = items.ItemDescription;

            var itemimage = db.ItemImages.Where(a => a.ItemID == item.mainItem).SingleOrDefault();
            if (itemimage != null)
            {
                vmodel.FileNameImg = itemimage.FileName;
            }


            ViewBag.Item = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                         }, "Value", "Text", 1);

            var cat = db.ItemCategorys.Select(s => new
            {
                ItemCategoryID = s.ItemCategoryID,
                ItemCategoryName = s.ItemCategoryName
            }).ToList();
            ViewBag.Category = QkSelect.List(cat, "ItemCategoryID", "ItemCategoryName");

            var taxs = db.Taxs.Select(s => new
            {
                TaxID = s.TaxID,
                TaxName = s.TaxName
            }).ToList();
            ViewBag.Tax = QkSelect.List(taxs, "TaxID", "TaxName");

            ViewBag.preEntry = db.ItemBundles.Where(a => a.ItemBundleId < id).Select(a => a.ItemBundleId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.ItemBundles.Where(a => a.ItemBundleId > id).Select(a => a.ItemBundleId).DefaultIfEmpty().Min();

            var ExistBItem = (db.SEItemss.Any(x => x.Item == item.mainItem) || db.SRItemss.Any(x => x.Item == item.mainItem)
                || db.SalesOrderItems.Any(x => x.Item == item.mainItem)
                || db.PEItemss.Any(x => x.Item == item.mainItem) || db.PRItemss.Any(x => x.Item == item.mainItem)
                || db.PurchaseOrderItems.Any(x => x.Item == item.mainItem)
                || db.PFItemss.Any(x => x.Item == item.mainItem) || db.DvItems.Any(x => x.Item == item.mainItem)
                || db.UnassembleItems.Any(x => x.ItemId == item.mainItem) || db.ProItems.Any(x => x.ItemId == item.mainItem));

            if (ExistBItem)
            {
                ViewBag.BItemExist = "true";
            }
            else
            {
                ViewBag.BItemExist = "false";
            }

            vmodel.HireType = db.HireTypes.ToList();
            vmodel.HireTypes = db.HireRates.Where(a => a.ItemId == item.mainItem).Select(a => new HireTypeViewModel() { type = a.type, Rate = a.Rate }).ToList();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View(vmodel);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit ItemBundle")]
        public JsonResult Edit(ItemBundleViewModel vmodel)
        {

            bool stat = false;
            string msg;

            ItemBundle items = db.ItemBundles.Find(vmodel.ItemBundleId);
            Item item = db.Items.Find(items.mainItem);

            var ItemCodeExists = db.Items.Any(u => u.ItemCode == vmodel.ItemCode && u.ItemID != items.mainItem && item.ItemCode != u.ItemCode);
            if (ItemCodeExists)
            {
                msg = "A Item with same Item code exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {

                var UserId = User.Identity.GetUserId();

                DateTime? sDate = null;
                DateTime? eDate = null;
                if (vmodel.StartDate != null)
                {
                    sDate = DateTime.Parse(vmodel.StartDate.ToString(), new CultureInfo("en-GB"));
                }
                if (vmodel.EndDate != null)
                {
                    eDate = DateTime.Parse(vmodel.EndDate.ToString(), new CultureInfo("en-GB"));
                }

                item.ItemName = vmodel.ItemName;
                item.ItemCode = vmodel.ItemCode;
                item.ItemDescription = vmodel.Note;
                item.SellingPrice = vmodel.SellingPrice;
                item.PurchasePrice = vmodel.ActualCost;
                item.MRP = vmodel.ActualPrice;
                item.KeepStock = vmodel.KeepStock;
                item.ItemCategoryID = vmodel.ItemCategoryID;
                item.OpeningStock = vmodel.StockQuantity;
                item.TaxID = vmodel.TaxID;

                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
                Int64 ItemID = item.ItemID;

                if (vmodel.ImgFileName != null)
                {

                    var ItemImg = db.ItemImages.Where(a => a.ItemID == ItemID).FirstOrDefault();
                    if (ItemImg != null)
                    {
                        db.ItemImages.RemoveRange(db.ItemImages.Where(a => a.ItemID == ItemID));
                        string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + ItemID);
                        if (Directory.Exists(storePath))
                            Directory.Delete(storePath, true);
                    }

                    foreach (IFormFile file in vmodel.ImgFileName)
                    {
                        //Checking file is available to save.  
                        if (file != null)
                        {
                            var ProdImg = new ItemImage
                            {
                                ItemID = ItemID,
                                FileName = Path.GetFileName(file.FileName),
                                Status = 1
                            };
                            db.ItemImages.Add(ProdImg);
                            db.SaveChanges();
                            string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + ItemID);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);
                            var InputFileName = Path.GetFileName(file.FileName);
                            var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                            //Save file to server folder  
                            file.SaveAs(ServerSavePath);
                        }
                    }
                }


                items.StartDate = sDate;
                items.EndDate = eDate;
                items.BundleType = vmodel.BundleType;
                db.Entry(items).State = EntityState.Modified;
                db.SaveChanges();
                Int64 bdId = items.ItemBundleId;




                var bItems = db.BundleItems.Where(a => a.ItemBundle == bdId).FirstOrDefault();
                if (bItems != null)
                {
                    db.BundleItems.RemoveRange(db.BundleItems.Where(a => a.ItemBundle == bdId));
                    db.SaveChanges();
                }

                BundleItem bnItem = new BundleItem();
                foreach (var arr in vmodel.bundleitem)
                {
                    if (arr.ItemId != 0)
                    {
                        bnItem.ItemBundle = bdId;
                        bnItem.ItemId = arr.ItemId;
                        bnItem.ItemUnit = arr.ItemUnit;
                        bnItem.ItemUnitPrice = arr.ItemUnitPrice;
                        bnItem.ItemQuantity = arr.ItemQuantity;
                        bnItem.ItemSubTotal = arr.ItemSubTotal;
                        bnItem.ItemTax = arr.ItemTax;
                        bnItem.ItemTaxAmount = arr.ItemTaxAmount;
                        bnItem.ItemTotalAmount = arr.ItemTotalAmount;

                        db.BundleItems.Add(bnItem);
                        db.SaveChanges();
                    }
                }

                //hire rates
                var HirRate = db.HireRates.Where(a => a.ItemId == items.mainItem).FirstOrDefault();
                if (HirRate != null)
                {
                    db.HireRates.RemoveRange(db.HireRates.Where(a => a.ItemId == items.mainItem));
                    db.SaveChanges();
                }

                if (vmodel.HireTypes != null)
                {
                    foreach (HireTypeViewModel Hire in vmodel.HireTypes)
                    {
                        var rate = new HireRate
                        {
                            type = Hire.type,
                            Rate = Hire.Rate,
                            ItemId = items.mainItem
                        };
                        db.HireRates.Add(rate);
                        db.SaveChanges();
                    }
                }

                com.addlog(LogTypes.Updated, UserId, "ItemBundle", "ItemBundless", findip(), bdId, "Successfully Updated ItemBundle");
                msg = "Successfully Updated ItemBundle.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,View ItemBundle")]
        public ActionResult Details(long? id)
        {
            ItemBundleViewModel vmodel = new ItemBundleViewModel();
            vmodel = (from a in db.ItemBundles
                      join i in db.Items on a.mainItem equals i.ItemID
                      join b in db.ItemCategorys on i.ItemCategoryID equals b.ItemCategoryID into cat
                      from b in cat.DefaultIfEmpty()
                      join c in db.Taxs on i.TaxID equals c.TaxID into tax
                      from c in tax.DefaultIfEmpty()
                      join e in db.Users on a.CreatedBy equals e.Id
                      where a.ItemBundleId == id
                      select new ItemBundleViewModel
                      {
                          ItemCode = i.ItemCode,
                          ItemName = i.ItemName,
                          //Barcode = i.Barcode,
                          ActualCost = i.PurchasePrice,
                          ActualPrice = i.MRP,
                          SellingPrice = i.SellingPrice,
                          StockKeep = i.KeepStock == true ? "True" : "False",
                          StockQuantity = i.OpeningStock,
                          SDate = a.StartDate,
                          EDate = a.StartDate,
                          CategoryName = b.ItemCategoryName,
                          TaxName = c.TaxName,
                          Note = i.ItemDescription.Replace("\n", "<br />"),
                          CreatedBy = e.UserName,
                          BundleType = a.BundleType
                      }).FirstOrDefault();
            vmodel.bundleitemvmodel = db.BundleItems.Where(a => a.ItemBundle == id)
            .Select(b => new BundleItemViewModel
            {
                ItemCode = db.Items.Where(a => a.ItemID == b.ItemId).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),

                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemTax = b.ItemTax,
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount
            }).ToList();
            return View(vmodel);
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete ItemBundle")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ItemBundle bun = db.ItemBundles.Find(id);
            if (bun == null)
            {
                return NotFound();
            }
            return PartialView(bun);
        }

        [QkAuthorize(Roles = "Dev,Delete ItemBundle")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(long id)
        {
            try
            {
                bool stat = false;
                string msg;
                long Itemid = db.ItemBundles.Where(x => x.ItemBundleId == id).Select(y => y.mainItem).FirstOrDefault();
                var UserId = User.Identity.GetUserId();
                var Msg = chkDeleteWithMsg(id);
                if (Msg != null)
                {
                    msg = Msg;
                    stat = false;
                }
                else
                {
                    stat = DeleteFn(id);
                    msg = "Successfully deleted Item Bundle.";
                }
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete ItemBundle")]
        public ActionResult DeleteAllItem(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr) == true) ? count++ : notdel++;
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
            return RedirectToAction("Index", "ItemBundle");
        }

        private Boolean DeleteItem(long id)
        {
            var Msg = chkDeleteWithMsg(id);
            bool res = (Msg != null) ? false : DeleteFn(id);
            return res;
        }

        public bool DeleteFn(long id)
        {
            ItemBundle bundle = db.ItemBundles.Find(id);
            var UserId = User.Identity.GetUserId();
            var ItemImg = db.ItemImages.Where(a => a.ItemID == bundle.mainItem).FirstOrDefault();
            if (ItemImg != null)
            {
                db.ItemImages.RemoveRange(db.ItemImages.Where(a => a.ItemID == bundle.mainItem));
                string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + bundle.mainItem);
                if (Directory.Exists(storePath))
                    Directory.Delete(storePath, true);
            }
            //remove item
            Item Items = db.Items.Find(bundle.mainItem);
            db.Items.Remove(Items);

            /*************Delete From ItemTransaction **********/
            com.DeleteItemTransaction(bundle.mainItem);

            var Item = db.BundleItems.Where(a => a.ItemBundle == id);
            if (Item != null)
            {
                db.BundleItems.RemoveRange(db.BundleItems.Where(a => a.ItemBundle == id));
            }
            db.ItemBundles.Remove(bundle);


            var HRates = db.HireRates.Where(a => a.ItemId == bundle.mainItem);
            if (HRates != null)
            {
                db.HireRates.RemoveRange(HRates);
            }
            db.SaveChanges();


            com.addlog(LogTypes.Deleted, UserId, "ItemBundle", "ItemBundles", findip(), id, "Successfully Deleted ItemBundle");
            db.SaveChanges();
            return true;
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.SEItemss.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Sales Entry !!";
            }
            else if (db.SRItemss.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Sales Return !!";
            }
            else if (db.DvItems.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Delivery Note !!";
            }
            else if (db.PFItemss.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in ProForma !!";
            }
            else if (db.PEItemss.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Purchase Entry !!";
            }
            else if (db.PurchaseOrderItems.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Purchase Order !!";
            }
            else if (db.PRItemss.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Purchase Return !!";
            }
            else if (db.QuotationItems.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Quotation !!";
            }
            else if (db.SalesOrderItems.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Sales Order !!";
            }
            else if (db.StockTransferItems.Any(c => c.Item == id))
            {
                msg = "Item Bundle Already used in Stock Transfer !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }

        [HttpGet]
        public ActionResult GetBundleItems(long EntryID)
        {
            var bundle = (from a in db.ItemBundles
                          join i in db.Items on a.mainItem equals i.ItemID
                          join b in db.ItemCategorys on i.ItemCategoryID equals b.ItemCategoryID into cat
                          from b in cat.DefaultIfEmpty()
                          join c in db.Taxs on i.TaxID equals c.TaxID into tax
                          from c in tax.DefaultIfEmpty()
                          join e in db.Users on a.CreatedBy equals e.Id
                          where a.ItemBundleId == EntryID
                          select new
                          {
                              a.ItemBundleId,
                              i.KeepStock,
                              a.StartDate,
                              a.EndDate,
                              ActualCost = i.PurchasePrice,
                              ActualPrice = i.MRP,
                              i.SellingPrice
                          }).FirstOrDefault();

            var item = (from a in db.BundleItems
                        join b in db.Items on a.ItemId equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.ItemBundle == EntryID
                        select new
                        {
                            Item = a.ItemId,
                            a.ItemQuantity,
                            a.ItemUnit,
                            a.ItemUnitPrice,
                            a.ItemTax,
                            a.ItemSubTotal,
                            a.ItemTaxAmount,
                            a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            b.ItemArabic,
                            b.PurchasePrice
                        }).ToList();

            var data = new { item = item, bundle = bundle };
            return Json(data);
        }
        public JsonResult searchitempos(string q,string x,string page)
        {
            
            DateTime today = Convert.ToDateTime(System.DateTime.Now);
            List<SelectFormat> items;                       
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                items = (from b in db.Items
                         join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                         from g in bundle.DefaultIfEmpty()
                         where b.Status == Status.active && (b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q))&&(  (g.StartDate == null || (EF.Functions.DateDiffDay(g.StartDate, today) >= 0 && EF.Functions.DateDiffDay(g.EndDate, today) <= 0)))
                         select new SelectFormat
                         {
                             text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                             id = b.ItemID
                         }).OrderBy(b => b.text).ToList();
            }
            else
            {

                items = (from b in db.Items
                             join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                             from g in bundle.DefaultIfEmpty()
                             where b.Status == Status.active && ( (g.StartDate == null || (EF.Functions.DateDiffDay(g.StartDate, today) >= 0 && EF.Functions.DateDiffDay(g.EndDate, today) <= 0)))
                             select new SelectFormat
                             {
                                 text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                 id = b.ItemID
                             }).OrderBy(b => b.text).ToList();
            }
            
            var serialisedJson = items;
            return Json(serialisedJson);
        }
        public JsonResult Search(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            DateTime today = Convert.ToDateTime(System.DateTime.Now);
            List<SelectFormat> items;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                items = (from b in db.ItemBundles
                         join g in db.Items on b.mainItem equals g.ItemID into itm
                         from g in itm.DefaultIfEmpty()
                         where b.Status == Status.active && (g.ItemName.ToLower().Contains(q.ToLower()) || g.ItemCode.ToLower().Contains(q.ToLower()) || g.Barcode.ToLower().Contains(q.ToLower()) || g.ItemName.Contains(q) || g.ItemCode.Contains(q) || g.Barcode.Contains(q))
                         select new SelectFormat
                         {
                             text = g.ItemCode + "-" + g.ItemName, //each json object will have 
                             id = g.ItemID
                         }).OrderBy(b => b.text).ToList();
            }
            else
            {
                items = (from b in db.ItemBundles
                         join g in db.Items on b.mainItem equals g.ItemID into itm
                         from g in itm.DefaultIfEmpty()
                         where b.Status == Status.active
                         select new SelectFormat
                         {
                             text = g.ItemCode + "-" + g.ItemName, //each json object will have 
                             id = g.ItemID
                         }).OrderBy(b => b.text).ToList();
            }
            var serialisedJson = items;
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchItem(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Items
                                  join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                                  from g in bundle.DefaultIfEmpty()
                                  where b.Status == Status.active && ((g.mainItem == null) || (g.StartDate == null && b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.ToLower().Contains(q.ToLower()) || b.ItemName.Contains(q) || b.ItemCode.Contains(q) || b.Barcode.Contains(q)))
                                  select new SelectFormat
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Items
                                  join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                                  from g in bundle.DefaultIfEmpty()
                                  where b.Status == Status.active && (g.mainItem == null) || (g.StartDate == null)
                                  select new SelectFormat
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

        public JsonResult SearchBarcode(string q)
        {
            DateTime today = Convert.ToDateTime(System.DateTime.Now);
            List<SelectFormat> serialisedJson = null;
            serialisedJson = (from b in db.Items
                              join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              where b.Status == Status.active && (b.Barcode.ToLower().Contains(q.ToLower()) || b.Barcode.Contains(q)) && ((g.mainItem == null) || (EF.Functions.DateDiffDay(g.StartDate, today) >= 0 && EF.Functions.DateDiffDay(g.EndDate, today) <= 0))
                              select new SelectFormat
                              {
                                  text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                  id = b.ItemID
                              }).OrderBy(b => b.text).ToList();
            return Json(serialisedJson);
        }
        [HttpPost]
        public ActionResult GetItem(int itemID)
        {
            DateTime today = Convert.ToDateTime(System.DateTime.Now);
            DateTime offdate = System.DateTime.Now.Date;
            var offerprice = (from a in db.BatchStocks
                              where a.Item == itemID
                              & offdate >= a.MFG && offdate <= a.EXP
                              select new
                              {
                                  a.Cost
                              }).Select(o => o.Cost).FirstOrDefault();
            var item = (from b in db.Items
                        join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                        from g in bundle.DefaultIfEmpty()
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                            //let h = db.InstantDiscounts.Where(cl => cl.ItemId == b.ItemID && (EF.Functions.DateDiffDay(cl.StartDate, today) >= 0 && EF.Functions.DateDiffDay(cl.EndDate, today) <= 0)).OrderByDescending(cl => cl.InstantDiscountId).FirstOrDefault()
                        where b.ItemID == itemID && (g.mainItem == null)
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
                            offer=(offerprice==null)?0:offerprice,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.Barcode,
                            b.MRP,
                            b.KeepStock,
                            // offer = h,
                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,
                            g.BundleType

                        }).AsEnumerable().Select(o => new
                        {
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.Barcode,
                            o.ItemArabic,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.offer,
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
                            //price = (o.offerprice != -1) ? o.offerprice:(o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            //o.offerprice,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                            subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            bundle = "",
                            o.BundleType,
                            ItemNote = "",
                            itemsize = (from a in db.itemsizeprice
                                        join b in db.ItemSizes on a.sizeid equals b.ItemSizeID
                                        where a.itemid == o.ItemID
                                        select new
                                        {
                                            a.itemid,
                                            b.ItemSizeName,
                                            a.price,
                                            a.sizepriceid
                                        }).ToList()


                            // o.offer
                        }).FirstOrDefault();
            if (item != null)
            {
                return new QuickSoft.Models.LegacyJsonResult { Data = new { item = item } };
            }
            else
            {
                var itemBundle = (from g in db.ItemBundles
                                  join b in db.Items on g.mainItem equals b.ItemID
                                  join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                  from c in primary.DefaultIfEmpty()
                                  join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                  from d in second.DefaultIfEmpty()
                                  join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                                  from e in cat.DefaultIfEmpty()
                                  join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                                  from f in taxss.DefaultIfEmpty()
                                  where b.ItemID == itemID && (g.StartDate == null || (EF.Functions.DateDiffDay(g.StartDate, today) >= 0 && EF.Functions.DateDiffDay(g.EndDate, today) <= 0))
                                  select new
                                  {
                                      b.ItemCode,
                                      b.ItemName,
                                      b.Barcode,
                                      b.ItemArabic,
                                      ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                      b.ItemUnitID,
                                      b.SubUnitId,
                                      PriUnit = c.ItemUnitName,
                                      SubUnit = d.ItemUnitName,
                                      ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                                      b.ItemID,
                                      b.OpeningStock,
                                      b.MinStock,
                                      categoryname = e.ItemCategoryName,
                                      Tax = f.Percentage,
                                      b.SellingPrice,
                                      b.PurchasePrice,
                                      b.BasePrice,
                                      b.MRP,
                                      b.KeepStock,

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
                                      // production ----
                                      // main item
                                      PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                                      SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
                                      // compined item
                                      PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                                      SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,
                                      // unassemble -----
                                      // main item
                                      PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                                      SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
                                      // compined item
                                      PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                                      SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,

                                      g.ItemBundleId,
                                      g.BundleType
                                  }).AsEnumerable().Select(o => new
                                  {
                                      o.ItemID,
                                      o.ItemCode,
                                      o.Barcode,
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

                                      PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                                      SubPurchase = (o.SubPurchase % o.ConFactor),

                                      PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                                      SubSale = (o.SubSale % o.ConFactor),

                                      PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                                      SubPReturn = (o.SubPReturn % o.ConFactor),

                                      PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                                      SubSReturn = (o.SubSReturn % o.ConFactor),

                                      pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                                      subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                                      total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),


                                      o.ItemBundleId,
                                      ItemNote = "",
                                      o.BundleType
                                  }).FirstOrDefault();
                var check = (itemBundle != null) ? itemBundle.ItemBundleId : 0;
                var bundle = (from a in db.BundleItems
                              join b in db.Items on a.ItemId equals b.ItemID
                              join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                              from c in primary.DefaultIfEmpty()
                              where a.ItemBundle == check
                              select new
                              {
                                  b.ItemCode,
                                  b.ItemName,
                                  c.ItemUnitName,
                                  ItemUnitPrice = a.ItemUnitPrice,
                                  quantity = a.ItemQuantity,
                                  ItemSubTotal = a.ItemSubTotal,
                                  ItemTax = a.ItemTax,
                                  ItemTaxAmount = a.ItemTaxAmount,
                                  ItemTotalAmount = a.ItemTotalAmount,
                              }).ToList();
                return new QuickSoft.Models.LegacyJsonResult { Data = new { item = itemBundle, bundle = bundle } };
            }
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
