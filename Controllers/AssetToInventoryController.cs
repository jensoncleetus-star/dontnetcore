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
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic;
using System.Data;
using System.Collections.Generic;
using System.Net;
using Microsoft.Data.SqlClient;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class AssetToInventoryController : BaseController
    {
        ApplicationDbContext db;
        Common com;

        public AssetToInventoryController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: AssetToInventory List     
        public ActionResult Index()
        {
            //For Dropdown Asset Account
            ViewBag.AssetAccounts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            return View();
        }

        [RedirectingAction]
        [HttpPost]
        public ActionResult GetDetails(long? EntryNo, string FromDate, string ToDate, long? AssetAccount, long? TransType)
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
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit AssetToInventory Entry");
            var uDelete = User.IsInRole("Delete AssetToInventory Entry");

            var v = (from a in db.AssetToInventoryMasters
                     join b in db.Accountss
                     on a.AssetAccountId equals b.AccountsID
                     where
                     (EntryNo == 0 || EntryNo == null || a.EntryNo == EntryNo) &&
                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(a.EntryDate, fdate) <= 0) &&
                     (ToDate == "" || ToDate == null || EF.Functions.DateDiffDay(a.EntryDate, tdate) >= 0) &&
                     (AssetAccount == 0 || AssetAccount == null || a.AssetAccountId == AssetAccount) &&
                     (TransType == 0 || TransType == null || (a.StockTransferId == null && TransType == 1) || (a.StockTransferId != null && TransType == 2))

                     select new
                     {
                         EntryID        =   a.EntryId,
                         EntryNo        =   a.EntryNo,
                         EntryDate      =   a.EntryDate,
                         AssetAccount   =   b.Name,
                         TotalAmount    =   a.TotalAmount,
                         TransType      =   (a.StockTransferId != null ? "From Inventory" : "From Purchase"),
                         Dev    = uDev,
                         Edit   = uEdit,
                         Delete = uDelete,
                         a.StockTransferId
                     });

            //SEARCH
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.EntryNo.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal    = v.Count();
            var data        = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: AssetToInventory Create
        public ActionResult Create()
        {
            var UserId      = User.Identity.GetUserId();
            var warningMsg  = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg     = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;


            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");

            var ViewModel = new AssetToInventoryViewModel
            {
                EntryNo = GetEntryNo(),
                Date    = DateTime.Now
            };

            //For Dropdown Asset Account
            ViewBag.AssetAccounts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = null, Value = null},
                      }, "Value", "Text", 1);           

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            return View(ViewModel);
        }

        // GET: AssetPurchaseToInventory Create
        public ActionResult CreateAssetPurchase()
        {
            var UserId = User.Identity.GetUserId();
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            var ViewModel = new AssetToInventoryViewModel
            {
                EntryNo = GetEntryNo(),
                Date    = DateTime.Now
            };

            //For Dropdown Asset Account
            ViewBag.AssetAccounts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = null, Value = null},
                      }, "Value", "Text", 1);


            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            return View(ViewModel);
        }

        //Function To Get EntryNo
        private long GetEntryNo()
        {
            Int64 EntryNo = 0, LastNo = 0;

            LastNo = db.AssetToInventoryMasters.Select(p => p.EntryNo).DefaultIfEmpty().Max();

            if (LastNo == 0)
                EntryNo = 1;
            else
                EntryNo = LastNo + 1;

            return EntryNo;
        }

        //Saving in Create Mode
        [RedirectingAction]
        [HttpPost]
        public JsonResult Create(string[][] array, string[] mtdata, string action)
        {

            Int64 EntryNo           =   Convert.ToInt64(mtdata[0]); //EntryNo        
            var Today               =   Convert.ToDateTime(System.DateTime.Now);
            bool stat               =   false;
            string msg              =   "";
            string AssetName        =   "";
            Int64 STId              =   0;
            long ItemId             =   0;
            Int64 AssetAccountID    =   Convert.ToInt64(mtdata[2]);//AssetAccountID
            string TransType        =   mtdata[4]; //Transaction Type          
            

            //Checking is there any row with same EntryNo.
            var EntryNoCheck    =   db.AssetToInventoryMasters.Where(a => a.EntryNo == EntryNo).Any();
            var UserId          =   User.Identity.GetUserId();

            //MC From is the Material Centre
            var AssetMc = db.EnableSettings.Where(a => a.EnableType == "MaterialCentre").FirstOrDefault();

            if (!EntryNoCheck)
            {
                //    /************************** Stock Transfer ***************************/




                //    ////Getting StockTransferID

                //    //foreach (var arr in array)
                //    /********************************************************************/

                /********************** AssetToInventoryMasters ***********************/
                AssetToInventoryMasters MasterObj = new AssetToInventoryMasters();

                MasterObj.EntryNo           =   Convert.ToInt64(mtdata[0]);
                MasterObj.EntryDate         =   DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
                MasterObj.AssetAccountId    =   AssetAccountID;
                MasterObj.McFromId           = Convert.ToInt64(mtdata[5]);
                MasterObj.TotalAmount       =   Convert.ToDecimal(mtdata[3]);

                if (TransType == "AssetFromInventory")
                    MasterObj.StockTransferId = STId;

                db.AssetToInventoryMasters.Add(MasterObj);
                db.SaveChanges();
                /*********************************************************************/

                /*********************** AssetToInventoryDetail **********************/
                Int64 EntryId = MasterObj.EntryId;
                AssetToInventoryDetail DetailObj = new AssetToInventoryDetail();

                foreach (var arr in array)
                {
                    AssetName = arr[5];

                    DetailObj.EntryId                   =   EntryId;
                    DetailObj.AssetId                   =   Convert.ToInt64(arr[0]);
                    DetailObj.UnitId                    =   Convert.ToInt64(arr[1]);
                    DetailObj.AssetName                 =   AssetName;
                    DetailObj.Barcode                   =   arr[6] != "" ? arr[6] : null;
                    DetailObj.Quantity                  =   Convert.ToDecimal(arr[10]);
                    DetailObj.Price                     =   Convert.ToDecimal(arr[11]);
                    DetailObj.TotalPrice                =   Convert.ToDecimal(arr[12]);
                    DetailObj.DepreciationPercentage    =   Convert.ToInt64(arr[8]);
                    DetailObj.DepreciationAccountId     =   Convert.ToInt64(arr[4]);

                    db.AssetToInventoryDetails.Add(DetailObj);
                    db.SaveChanges();

                    if (TransType == "AssetPurchase")
                    {
                        var ItemDetails = db.Items.Where(a => (a.ItemName == AssetName)).FirstOrDefault();

                        if (ItemDetails == null)
                        {
                            //Calling Function To Save In Item Table
                            ItemId = AddItem(AssetName, DetailObj.UnitId, DetailObj.Quantity, DetailObj.Price, DetailObj.TotalPrice, UserId);
                        }
                        else
                        {
                            ItemId = ItemDetails.ItemID;

                            //Function to update Item details
                            UpdateItem(DetailObj.AssetId, ItemId);
                        }

                        DetailObj.RefItemId = ItemId;

                    }
                    else
                        DetailObj.RefItemId = Convert.ToInt64(arr[3]);

                    db.SaveChanges();

                }
                /*********************************************************************/

                /************************* AccountTrasaction ************************/
                //add Total Amount for Asset Account(Debit)


                // add Total Amount for Stock Account(Credit)

                /*********************************************************************/

                var StockAccount = db.Accountss.Where(x => x.Name == "Asset From Inventory").FirstOrDefault();
                com.addAccountTrasaction(MasterObj.TotalAmount,0, StockAccount.AccountsID, "Asset To Inventory", EntryId, DC.Credit, MasterObj.EntryDate, null, null, null, null);

                com.addAccountTrasaction( 0, MasterObj.TotalAmount,(long)MasterObj.AssetAccountId, "Asset To Inventory", EntryId, DC.Debit, MasterObj.EntryDate, null, null, null, null);

                msg = "Asset To Inventory Successfully Created....";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Entry No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        private long AddItem(string Name, long UnitId, decimal? Quantity, decimal Price, decimal TotalPrice, string UserId)
        {
            /************************** Stock Transfer ***************************/
            Item ItemObj = new Item();

            var ItemBarcode = com.createBarcode().ToString();

            ItemObj.ItemCode        =   ItemCodes();
            ItemObj.ItemName        =   Name;
            ItemObj.Barcode         =   ItemBarcode;
            ItemObj.ItemUnitID      =   UnitId;
            ItemObj.OpeningStock    =   Quantity;
            ItemObj.MinStock        =   0;
            ItemObj.PurchasePrice   =   Price;
            ItemObj.SellingPrice    =   0;
            ItemObj.BasePrice       =   0;
            ItemObj.MRP             =   0;
            ItemObj.KeepStock       =   true;
            ItemObj.ItemBrandID     =   1;
            ItemObj.ItemCategoryID  =   1;
            ItemObj.TaxID           =   1;
            ItemObj.CreatedBy       =   1;
            ItemObj.Status          =   Status.active;
            ItemObj.ItemType        =   1;
            ItemObj.CreatedUserID   =   UserId;
            ItemObj.ConFactor       =   1;
            ItemObj.Commission      =   0;
            ItemObj.Branch          =   1;
            ItemObj.Currency        =   1;
            ItemObj.InSaleInvoice   =   false;
            ItemObj.StockValue      =   TotalPrice;
            ItemObj.OpeningCost     =   Price;
            ItemObj.ItemColorID     =   null;
            ItemObj.ItemSizeID      =   null;

            db.Items.Add(ItemObj);
            db.SaveChanges();

            //Barcode
            Barcode BarcodeObj = new Barcode();

            BarcodeObj.BarcodeNumber    =   ItemBarcode;
            BarcodeObj.ItemID           =   ItemObj.ItemID;
            db.Barcodes.Add(BarcodeObj);

            return ItemObj.ItemID;
        }

        //Function to update Item details -- Opening Stock should be updated while changing Quantity
        private ActionResult UpdateItem(long AssetId, long ItemId)
        {
            decimal TotalQty = 0;
           
            var Asset = db.AssetToInventoryDetails.Where(x => x.AssetId == AssetId).FirstOrDefault();

            if (Asset != null)
            {
                TotalQty = (from a in db.AssetToInventoryDetails
                            where a.AssetId == AssetId
                            select a.Quantity).Sum();
            }

            var ItemDetails = db.Items.Find(ItemId);

            if (ItemDetails != null)
            {
                ItemDetails.OpeningStock = TotalQty;
            }

            db.SaveChanges();

            return Json("Item Updated Successfully!");
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
        private long GetVTNoForStockTransfer()
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

        // GET: AssetToInventory Edit
        public ActionResult Edit(long? id)
        {
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
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
            AssetToInventoryViewModel vmodel = new AssetToInventoryViewModel();

            vmodel = (from a in db.AssetToInventoryMasters
                      join b in db.Accountss
                      on a.AssetAccountId equals b.AccountsID
                      where a.EntryId == id
                      select new AssetToInventoryViewModel
                      {
                          EntryNo           =   a.EntryNo,
                          Date              =   a.EntryDate,
                          AssetAccountID    =   a.AssetAccountId,
                          AssetAccount      =   b.Name,
                          TotalAmount       =   a.TotalAmount,
                          StockTransferId   =   a.StockTransferId,
                          McFromId          =   a.McFromId
                      }).FirstOrDefault();

            return View(vmodel);
        }

        //Saving in Edit Mode
        [RedirectingAction]
        [HttpPost]
        public JsonResult Edit(string[][] array, string[] mtdata, string action)
        {
            string msg;
            bool stat           =   false;
            var UserId          =   User.Identity.GetUserId();
            var Today           =   Convert.ToDateTime(System.DateTime.Now);
            Int64 EntryID       =   Convert.ToInt64(mtdata[4]);
            string TransType    =   mtdata[5]; //Transaction Type 

            //Getting the Assets before removing
            var Assets = db.AssetToInventoryDetails.Where(a => a.EntryId == EntryID).ToList();

            /********************** AssetToInventoryMasters ***********************/
            AssetToInventoryMasters MasterObj = db.AssetToInventoryMasters.Find(EntryID);

            MasterObj.EntryDate     = DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
            MasterObj.TotalAmount   = Convert.ToDecimal(mtdata[3]);
            MasterObj.McFromId = Convert.ToInt64(mtdata[6]);

            db.Entry(MasterObj).State = EntityState.Modified;
            db.SaveChanges();

            /*********************************************************************/

            /*********************** AssetToInventoryDetail **********************/

            db.AssetToInventoryDetails.RemoveRange(db.AssetToInventoryDetails.Where(a => a.EntryId == EntryID));

            AssetToInventoryDetail DetailObj = new AssetToInventoryDetail();

            foreach (var arr in array)
            {
                DetailObj.EntryId                   =   EntryID;
                DetailObj.AssetId                   =   Convert.ToInt64(arr[0]);
                DetailObj.UnitId                    =   Convert.ToInt64(arr[1]);
                DetailObj.AssetName                 =   arr[5];
                DetailObj.RefItemId                 =   Convert.ToInt64(arr[3]);
                DetailObj.Barcode                   =   arr[6] != "" ? arr[6] : null;
                DetailObj.Quantity                  =   Convert.ToDecimal(arr[10]);
                DetailObj.Price                     =   Convert.ToDecimal(arr[11]);
                DetailObj.TotalPrice                =   Convert.ToDecimal(arr[12]);
                DetailObj.DepreciationPercentage    =   Convert.ToInt64(arr[8]);
                DetailObj.DepreciationAccountId     =   Convert.ToInt64(arr[4]);

                db.AssetToInventoryDetails.Add(DetailObj);
                db.SaveChanges();
            }
            /*********************************************************************/

            //    /************************** Stock Transfer ***************************/



            //    //*****Detail Table
            //    //Delete current StockTransfer then Insert new


            //    //foreach (var arr in array)

            //    /********************************************************************/
            if (TransType != "AssetFromInventory")
            {
                int i = 0;
                long AssetId, ItemId;
                foreach (var row in Assets)
                {
                    AssetId = Assets[i].AssetId;
                    ItemId = Assets[i].RefItemId;

                    //Function to update Item details
                    UpdateItem(AssetId, ItemId);

                    i++;
                }
            }

            /************************ Account Transaction ************************/
             bool delete = com.DeleteAllAccountTransaction("Asset To Inventory", EntryID);

            // add Total Amount for Asset Account(Debit)

            // add Total Amount for Stock Account(Credit)

            /*********************************************************************/
            var StockAccount = db.Accountss.Where(x => x.Name == "Asset From Inventory").FirstOrDefault();
            com.addAccountTrasaction(MasterObj.TotalAmount, 0, StockAccount.AccountsID, "Asset To Inventory", EntryID, DC.Credit, MasterObj.EntryDate, null, null, null, null);

            com.addAccountTrasaction(0, MasterObj.TotalAmount, (long)MasterObj.AssetAccountId, "Asset To Inventory", EntryID, DC.Debit, MasterObj.EntryDate, null, null, null, null);

            msg = "Asset To Inventory Details Successfully Updated..";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev, Delete AssetToInventory")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AssetToInventoryMasters MastObj = db.AssetToInventoryMasters.Find(id);

            if (MastObj == null)
            {
                return NotFound();
            }
            else
            {
                return PartialView(MastObj);
            }
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteAction(long id)
        {
            bool stat = false;
            string msg;

            var chk = DeleteEntry(id);

            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Asset To Inventory Entry details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //Function to delete all the transactions
        [HttpPost]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;

            foreach (var arr in bill)
            {
                var chk = (DeleteEntry(arr) == true) ? count++ : count;
            }

            Success("Deleted " + count + " Asset To Inventory Entry.", true);
            return RedirectToAction("Index", "AssetToInventory");
        }

        //Function To Delete Each Entry
        private Boolean DeleteEntry(long EntryId)
        {
            long    ItemId, AssetId;
            decimal Quantity;
            int i = 0;

            var Assets = db.AssetToInventoryDetails.Where(a => a.EntryId == EntryId).ToList();

            //If coming from 'Asset From Inventory'
            var StockTransferId = db.AssetToInventoryMasters.Find(EntryId).StockTransferId;           

            //    //***********Delete from table StockTransfers

            //    //***********Delete from table StockTransferItems
           
            //***********Delete from table AccountTransaction
            bool delete = com.DeleteAllAccountTransaction("Asset To Inventory", EntryId);

            //***********Delete from table AssetToInventoryDetails
            var AssetDetails = db.AssetToInventoryDetails.Where(a => a.EntryId == EntryId).FirstOrDefault();
            if (AssetDetails != null)
            {
                db.AssetToInventoryDetails.RemoveRange(db.AssetToInventoryDetails.Where(a => a.EntryId == EntryId));
            }

            //***********Delete from table AssetToInventoryMasters
            AssetToInventoryMasters MastObj = db.AssetToInventoryMasters.Find(EntryId);
            db.AssetToInventoryMasters.Remove(MastObj);

            db.SaveChanges();

            //If coming from Asset Purchase
            if (StockTransferId == null)
            {
                foreach (var row in Assets)
                {
                    AssetId = Assets[i].AssetId;
                    ItemId = Assets[i].RefItemId;
                    Quantity = Assets[i].Quantity;

                    //Function to update Item details
                    UpdateItem(AssetId, ItemId);

                    i++;
                }
            }

            return true;
        }

        //Function to display Asset Details according to AssetAccountID(Only in Create Mode)
        //Only the Items -- Asset From Inventory
        [HttpGet]
        public JsonResult GetItemsInCreate(long AssetAccountID, string TransType)
        {
            var ConD = (from a in db.AssetTransferDetails
                        join b in db.Accountss
                        on a.DepreciationAccountId equals b.AccountsID into primary
                        from b in primary.DefaultIfEmpty()
                        join c in db.ItemUnits
                        on a.UnitId equals c.ItemUnitID into secondary
                        from c in secondary.DefaultIfEmpty()
                        join d in db.AssetToInventoryDetails
                        on a.AssetitementryId equals d.AssetId into temp1
                        from d in temp1.DefaultIfEmpty()
                        join e in db.AssetTransferMasters
                        on a.AssetEntryId equals e.AssetEntryId
                        where a.AssetAccountId == AssetAccountID &&
                        ((TransType == "AssetFromInventory" && e.VendorName == null) ||
                         (TransType == "AssetPurchase" && e.VendorName != null)
                        )

                        select new
                        {
                            AssetId = a.AssetitementryId,
                            a.AssetName,
                            a.AssetEntryId,
                            Barcode = a.Barcode != null ? a.Barcode : "",
                            a.UnitId,
                            c.ItemUnitName,
                            e.McFromId,
                            a.Price,
                           // a.TotalPrice,
                            a.RefItemId,
                            a.DepreciationPercentage,
                            a.DepreciationAccountId,
                            DepreciationAccntName = b.Name,

                            //AssetQty ==> Total Quantity of an Asset, which is transferred from Inventory
                            AssetQty = a.Quantity,

                            //ConvertedQty ==> Total Quantity of an Asset, which is already transferred to Inventory
                            ConvertedQty = (db.AssetToInventoryDetails.Where(x => x.AssetId == a.AssetitementryId).Sum(x => (decimal?)x.Quantity) ?? 0),

                        }).AsEnumerable().Select(o => new
                        {
                            o.AssetId,
                            o.AssetName,
                            o.AssetEntryId,
                            o.Barcode,
                            o.UnitId,
                            o.ItemUnitName,
                            o.McFromId,
                            Quantity = o.AssetQty - o.ConvertedQty,
                            o.Price,
                            TotalPrice = (o.AssetQty - o.ConvertedQty )* o.Price,
                            o.RefItemId,
                            o.DepreciationPercentage,
                            o.DepreciationAccountId,
                            o.DepreciationAccntName
                        }).GroupBy(x => x.AssetId, (key, g) => g.OrderByDescending(m => m.AssetId).FirstOrDefault());

            return Json(ConD);
        }

        //Function to display Asset Details in Edit Mode
        [HttpGet]
        public JsonResult GetItemsInEditMode(long EntryID)
        {
            var ConD = (from a in db.AssetToInventoryMasters
                        join b in db.AssetToInventoryDetails
                        on a.EntryId equals b.EntryId
                        join c in db.Accountss
                        on a.AssetAccountId equals c.AccountsID
                        join d in db.Accountss
                        on b.DepreciationAccountId equals d.AccountsID into primary
                        from d in primary.DefaultIfEmpty()
                        join e in db.ItemUnits
                        on b.UnitId equals e.ItemUnitID into secondary
                        from e in secondary.DefaultIfEmpty()
                        where (a.EntryId == EntryID)
                        select new
                        {
                            a.EntryId,
                            b.AssetId,
                            b.AssetName,
                            Barcode = b.Barcode != null ? b.Barcode : "",
                            b.UnitId,
                            e.ItemUnitName,
                            b.Quantity,
                            b.Price,
                            b.TotalPrice,
                            b.RefItemId,
                            b.DepreciationPercentage,
                            b.DepreciationAccountId,
                            d.Name
                        }).AsEnumerable().Select(o => new
                        {
                            o.EntryId,
                            o.AssetId,
                            o.AssetName,
                            o.Barcode,
                            o.UnitId,
                            o.ItemUnitName,
                            o.Quantity,
                            o.Price,
                            o.TotalPrice,
                            o.RefItemId,
                            o.DepreciationPercentage,
                            o.DepreciationAccountId,
                            o.Name
                        }).ToList();

            return Json(ConD);
        }

        public JsonResult GetAccountsByGroup(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                                  where (b.Parent == 2 || b.Parent == 4)
                                   && (b.AccountsGroupID != 8 && b.AccountsGroupID != 9 && b.AccountsGroupID != 11 && b.AccountsGroupID != 21 && b.AccountsGroupID != 22 && b.AccountsGroupID != 23)
                                  || (a.Group == 2 || a.Group == 4) 
                                  && (q == null || a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID
                                  }).OrderBy(b => b.text).ToList();
                    }
                    else
                    {
                        serialisedJson = (from a in db.Accountss
                                          join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                                          where (b.Parent == 2 || b.Parent == 4)
                                           && (b.AccountsGroupID != 8 && b.AccountsGroupID != 9 && b.AccountsGroupID != 11 && b.AccountsGroupID != 21 && b.AccountsGroupID != 22 && b.AccountsGroupID != 23)
                                          || (a.Group == 2 || a.Group == 4)
                                          select new SelectFormat
                                          {
                                              text  =   a.Name,
                                              id    =   a.AccountsID
                                          }).OrderBy(b => b.text).ToList();
                    }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult GetAssetsByAccount(string q, string x, long AccountID)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.AssetTransferDetails
                                  where (AccountID != 0 && a.AssetAccountId == AccountID) &&
                                        (q == null || a.AssetName.ToLower().Contains(q.ToLower()) || a.AssetName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.AssetName,
                                      id = a.AssetitementryId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.AssetTransferDetails
                                  where (a.AssetAccountId == AccountID)
                                  select new SelectFormat
                                  {
                                      text = a.AssetName,
                                      id = a.AssetitementryId
                                  }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        [HttpGet]
        public JsonResult GetAssetDetails(long AssetId)
        {
            //Retrieving data from table AssetTransferDetails
            var ConD = (from a in db.AssetTransferDetails
                        join b in db.ItemUnits on a.UnitId equals b.ItemUnitID into punit
                        from b in punit.DefaultIfEmpty()
                        where a.AssetitementryId == AssetId
                        select new
                        {
                            AssetId     =   a.AssetitementryId,
                            AssetName   =   a.AssetName,
                            Barcode     =   a.Barcode,
                            a.UnitId,
                            PriUnit     =   b.ItemUnitName,
                            Price       =   a.Price
                        }).FirstOrDefault();

            return Json(ConD);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //Taking only Current Assets and Fixed Assets
        public JsonResult SpecialAccounts(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            //Taking children of Fixed Assets
            var supparentid = new SqlParameter("@parentid", 4);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();

            //Taking children of Current Assets(Excluding some groups)
            var expparentid = new SqlParameter("@parentid", 2);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Where(b => b.AccountsGroupID != 8 && b.AccountsGroupID != 9 && b.AccountsGroupID != 11 && b.AccountsGroupID != 12 && b.AccountsGroupID != 21 && b.AccountsGroupID != 22 && b.AccountsGroupID != 23).Select(a => a.AccountsGroupID).ToArray();
            
            var arr = supgpid.Union(expgpid).ToArray();
            
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Accountss.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Alias.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) || p.Alias.Contains(q)) && arr.Contains(p.Group) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Accountss.Where(p => arr.Contains(p.Group) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult GetAllAssets(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.AssetTransferDetails
                                  where (q == null || a.AssetName.ToLower().Contains(q.ToLower()) || a.AssetName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.AssetName,
                                      id = a.AssetitementryId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.AssetTransferDetails                                 
                                  select new SelectFormat
                                  {
                                      text = a.AssetName,
                                      id = a.AssetitementryId
                                  }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

    }
}
