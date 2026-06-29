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
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class EnableSettingController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public EnableSettingController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [HttpPost]
        public ActionResult otherise(string password)
        {
            if (!String.IsNullOrEmpty(password))
            {
                String fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/config.txt"));

                string pass = System.IO.File.ReadAllText(fullPath);

                if (password == pass)
                {
                    Session["config"] = "success";
                    return RedirectToAction("Index", "EnableSetting");
                }
                else
                {
                    Danger("password incorrect", true);
                }
            }

            return RedirectToAction("oth", "EnableSetting");

        }

        [HttpGet]
        [RedirectingAction]
        public ActionResult oth()
        {
            return View();
        }

        public ActionResult AccountsDashboardConfig()
        {
            EnableSettingViewModel ebleset = new EnableSettingViewModel();
            ebleset.UpdatedDateExpiry = db.EnableSettings.Where(a => a.EnableType == "UpdatedDateExpiry").Select(a => a.TypeValue).FirstOrDefault();
            ebleset.NextDateExpiry = db.EnableSettings.Where(a => a.EnableType == "NextDateExpiry").Select(a => a.TypeValue).FirstOrDefault();
            return View(ebleset);
        }
        [HttpPost]
        [RedirectingAction]
        public ActionResult UpdateAccountsDashboardConfig(EnableSettingViewModel vmodel)
        {
            addMethod("UpdatedDateExpiry", Status.active, vmodel.UpdatedDateExpiry);
            addMethod("NextDateExpiry", Status.active, vmodel.NextDateExpiry);
            Success("Updated Successfully.", true);
            return RedirectToAction("AccountsDashboardConfig", "EnableSetting");
        }

        //  GET: EnableSetting 
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,EnableSettings")]
        public ActionResult Index()
        {
            if (Session["config"] == null)
            {
                return RedirectToAction("oth", "EnableSetting");
            }
            Session["config"] = null;
            IList<string> data = null;
            ViewBag.selectedRoles = data;
            var ts = db.TaskStatus.Select(o => new { o.TaskStatusId, o.StatusName }).ToList();
            ViewBag.taskstatus = QkSelect.List(ts, "TaskStatusId", "StatusName");

            //    .Select(s => new
            //        BranchID = s.BranchID,
            //        BranchDetails = s.BranchCode + " - " + s.BranchName

            MenuViewModel vmodel1 = new MenuViewModel();
            vmodel1.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
            ViewBag.UserRole = vmodel1;
            var taskqt = db.EnableSettings.Where(c => c.EnableType == "taskqt").SingleOrDefault();
            var bar = db.EnableSettings.Where(c => c.EnableType == "Barcode").SingleOrDefault();
            var CustomerDetails = db.EnableSettings.Where(c => c.EnableType == "CustomerDetailInInvoice").SingleOrDefault();
            var PartialMaterial = db.EnableSettings.Where(c => c.EnableType == "PartialMaterialConversion").SingleOrDefault();
            var autosave = db.EnableSettings.Where(c => c.EnableType == "Autosave").SingleOrDefault();
            var card = db.EnableSettings.Where(c => c.EnableType == "Jobcard").SingleOrDefault();
            var dvsale = db.EnableSettings.Where(c => c.EnableType == "DvToSale").SingleOrDefault();
            var bom = db.EnableSettings.Where(c => c.EnableType == "BOM").SingleOrDefault();
            var POS = db.EnableSettings.Where(c => c.EnableType == "POSInvoice").SingleOrDefault();
            var ItemCommision = db.EnableSettings.Where(c => c.EnableType == "ItemCommision").SingleOrDefault();
            var SaveAndMail = db.EnableSettings.Where(c => c.EnableType == "SaveAndMail").SingleOrDefault();
            var AutoUserCreate = db.EnableSettings.Where(c => c.EnableType == "AutoCreateUser").SingleOrDefault();
            var RackWiseStock = db.EnableSettings.Where(c => c.EnableType == "RackWiseStock").SingleOrDefault();
            var HideItemName = db.EnableSettings.Where(c => c.EnableType == "HideItemName").SingleOrDefault();
            var BillToBillReceipt = db.EnableSettings.Where(c => c.EnableType == "BillToBillReceipt").SingleOrDefault();
            var BillToBillPayment = db.EnableSettings.Where(c => c.EnableType == "BillToBillPayment").SingleOrDefault();

            var ItemPriceInPurchase = db.EnableSettings.Where(c => c.EnableType == "ItemPriceInPurchase").SingleOrDefault();

            var PredefinedCity = db.EnableSettings.Where(c => c.EnableType == "PredefinedCity").SingleOrDefault();

            var MCInTransaction = db.EnableSettings.Where(c => c.EnableType == "MCInTransaction").SingleOrDefault();
            var AssignUserMC = db.EnableSettings.Where(c => c.EnableType == "AssignUserMC").SingleOrDefault();

            var CNBillAdjust = db.EnableSettings.Where(c => c.EnableType == "CNBillAdjust").SingleOrDefault();
            var DNBillAdjust = db.EnableSettings.Where(c => c.EnableType == "DNBillAdjust").SingleOrDefault();

            var PartNoInItem = db.EnableSettings.Where(c => c.EnableType == "PartNoInItem").SingleOrDefault();

            var EnableBranch = db.EnableSettings.Where(c => c.EnableType == "EnableBranch").SingleOrDefault();

            var EnableJewellery = db.EnableSettings.Where(c => c.EnableType == "EnableJewellery").SingleOrDefault();

            var EnableCurrency = db.EnableSettings.Where(c => c.EnableType == "EnableCurrency").SingleOrDefault();

            var EnablePrefixCode = db.EnableSettings.Where(c => c.EnableType == "EnablePrefixCode").SingleOrDefault();

            var EnableItemCodeInPrint = db.EnableSettings.Where(c => c.EnableType == "EnableItemCodeInPrint").SingleOrDefault();

            var ItemOutOfStock = db.EnableSettings.Where(c => c.EnableType == "ItemOutOfStock").SingleOrDefault();
            var mcanddeliverystockeffect = db.EnableSettings.Where(c => c.EnableType == "mcanddeliverystockeffect").SingleOrDefault();

            
            var HideComHeaders = db.EnableSettings.Where(c => c.EnableType == "HideComHeaders").SingleOrDefault();


            var EnableItemBundle = db.EnableSettings.Where(c => c.EnableType == "EnableItemBundle").SingleOrDefault();

            var StckTrnsfrUpdt = db.EnableSettings.Where(c => c.EnableType == "StockTransferUpdate").SingleOrDefault();

            var BusinessType = db.EnableSettings.Where(c => c.EnableType == "BusinessType").Select(a => a.TypeValue).SingleOrDefault();

            var MenuColor = db.EnableSettings.Where(a => a.EnableType == "MenuColor").FirstOrDefault();

            var MenuOverColor = db.EnableSettings.Where(a => a.EnableType == "MenuOverColor").FirstOrDefault();

            var CreditLimit = db.EnableSettings.Where(a => a.EnableType == "CreditLimit").FirstOrDefault();

            var RemoveItemData = db.EnableSettings.Where(c => c.EnableType == "RemoveItemData").SingleOrDefault();

            var EnablePurchaseInvoice = db.EnableSettings.Where(c => c.EnableType == "EnablePurchaseInvoice").SingleOrDefault();

            var SalesReturnInSales = db.EnableSettings.Where(c => c.EnableType == "SalesReturnInSales").SingleOrDefault();

            var AutomaticBillNoInSales = db.EnableSettings.Where(c => c.EnableType == "AutomaticBillNoInSales").SingleOrDefault();
            var PreventOrderConvertion = db.EnableSettings.Where(c => c.EnableType == "PreventOrderConvertion").SingleOrDefault();

            var AutomaticMailInTransactions = db.EnableSettings.Where(c => c.EnableType == "AutomaticMailInTransactions").SingleOrDefault();

            var EnableVoucherEdit = db.EnableSettings.Where(c => c.EnableType == "EnableVoucherEdit").SingleOrDefault();

            
            var Chequeprint = db.EnableSettings.Where(c => c.EnableType == "Chequeprint").SingleOrDefault();

            var DiscountPercentage = db.EnableSettings.Where(c => c.EnableType == "DiscountPercentage").SingleOrDefault();

            var MultiLevelApproval = db.EnableSettings.Where(c => c.EnableType == "MultiLevelApproval").SingleOrDefault();
            var MLAMc = db.EnableSettings.Where(c => c.EnableType == "MLAMc").SingleOrDefault();
            var MLAQuot = db.EnableSettings.Where(c => c.EnableType == "MLAQuot").SingleOrDefault();
            var MLASEntry = db.EnableSettings.Where(c => c.EnableType == "MLASEntry").SingleOrDefault();
            var MLASOrder = db.EnableSettings.Where(c => c.EnableType == "MLASOrder").SingleOrDefault();
            var MLASReturn = db.EnableSettings.Where(c => c.EnableType == "MLASReturn ").SingleOrDefault();
            var MLAPQuot = db.EnableSettings.Where(c => c.EnableType == "MLAPQuot").SingleOrDefault();
            var MLAPEntry = db.EnableSettings.Where(c => c.EnableType == "MLAPEntry").SingleOrDefault();
            var MLAPOrder = db.EnableSettings.Where(c => c.EnableType == "MLAPOrder").SingleOrDefault();
            var MLAPReturn = db.EnableSettings.Where(c => c.EnableType == "MLAPReturn").SingleOrDefault();
            var MLADNote = db.EnableSettings.Where(c => c.EnableType == "MLADNote").SingleOrDefault();
            var MLAJCard = db.EnableSettings.Where(c => c.EnableType == "MLAJCard").SingleOrDefault();
            var MLAPForma = db.EnableSettings.Where(c => c.EnableType == "MLAPForma").SingleOrDefault();
            var MLASTran = db.EnableSettings.Where(c => c.EnableType == "MLASTran").SingleOrDefault();
            var MLASJour = db.EnableSettings.Where(c => c.EnableType == "MLASJour").SingleOrDefault();
            var MLAPList = db.EnableSettings.Where(c => c.EnableType == "MLAPList").SingleOrDefault();
            var MLAHReturn = db.EnableSettings.Where(c => c.EnableType == "MLAHReturn").SingleOrDefault();
            var MLAMRNote = db.EnableSettings.Where(c => c.EnableType == "MLAMRNote").SingleOrDefault();
            var MLAProd = db.EnableSettings.Where(c => c.EnableType == "MLAProd").SingleOrDefault();
            var MLAUAssem = db.EnableSettings.Where(c => c.EnableType == "MLAUAssem").SingleOrDefault();
            var Payment = db.EnableSettings.Where(c => c.EnableType == "Payment").SingleOrDefault();
            var Reciept = db.EnableSettings.Where(c => c.EnableType == "Reciept").SingleOrDefault();
            var StockValue = db.EnableSettings.Where(c => c.EnableType == "StockValue").SingleOrDefault();

            var Reminder = db.EnableSettings.Where(c => c.EnableType == "Reminder").SingleOrDefault();
            var RemindTask = db.EnableSettings.Where(c => c.EnableType == "RemindTask").SingleOrDefault();
            var RemindSale = db.EnableSettings.Where(c => c.EnableType == "RemindSale").SingleOrDefault();
            var RemindQuot = db.EnableSettings.Where(c => c.EnableType == "RemindQuot").SingleOrDefault();
            var RemindSReturn = db.EnableSettings.Where(c => c.EnableType == "RemindSReturn").SingleOrDefault();
            var RemindSOrder = db.EnableSettings.Where(c => c.EnableType == "RemindSOrder").SingleOrDefault();
            var RemindProForma = db.EnableSettings.Where(c => c.EnableType == "RemindProForma").SingleOrDefault();
            var RemindPurchase = db.EnableSettings.Where(c => c.EnableType == "RemindPurchase").SingleOrDefault();
            var RemindPReturn = db.EnableSettings.Where(c => c.EnableType == "RemindPReturn").SingleOrDefault();
            var RemindPOrder = db.EnableSettings.Where(c => c.EnableType == "RemindPOrder").SingleOrDefault();
            var RemindPQuot = db.EnableSettings.Where(c => c.EnableType == "RemindPQuot").SingleOrDefault();
            var RemindDNote = db.EnableSettings.Where(c => c.EnableType == "RemindDNote").SingleOrDefault();
            var RemindHReturn = db.EnableSettings.Where(c => c.EnableType == "RemindHReturn").SingleOrDefault();
            var RemindMReqn = db.EnableSettings.Where(c => c.EnableType == "RemindMReqn").SingleOrDefault();
            var RemindMRNote = db.EnableSettings.Where(c => c.EnableType == "RemindMRNote").SingleOrDefault();
            var RemindJobCard = db.EnableSettings.Where(c => c.EnableType == "RemindJobCard").SingleOrDefault();
            var RemindPackList = db.EnableSettings.Where(c => c.EnableType == "RemindPackList").SingleOrDefault();
            var RemindStkTrans = db.EnableSettings.Where(c => c.EnableType == "RemindStkTrans").SingleOrDefault();
            var RemindStkJnl = db.EnableSettings.Where(c => c.EnableType == "RemindStkJnl").SingleOrDefault();
            var RemindProd = db.EnableSettings.Where(c => c.EnableType == "RemindProd").SingleOrDefault();
            var RemindUnass = db.EnableSettings.Where(c => c.EnableType == "RemindUnass").SingleOrDefault();


            var AccInJournal = db.EnableSettings.Where(c => c.EnableType == "AccInJournal").SingleOrDefault();
            var MakeInTrans = db.EnableSettings.Where(c => c.EnableType == "MakeInTrans").SingleOrDefault();

            var BatchWiseStock = db.EnableSettings.Where(c => c.EnableType == "BatchWiseStock").SingleOrDefault();

            //prevent convertion
            var PreventConversion = db.EnableSettings.Where(c => c.EnableType == "PreventConversion").SingleOrDefault();
            var QuotToSale = db.EnableSettings.Where(c => c.EnableType == "QuotToSale").SingleOrDefault();
            var QuotToPForma = db.EnableSettings.Where(c => c.EnableType == "QuotToPForma").SingleOrDefault();
            var QuotToDvNote = db.EnableSettings.Where(c => c.EnableType == "QuotToDvNote").SingleOrDefault();
            var QuotToSOrder = db.EnableSettings.Where(c => c.EnableType == "QuotToSOrder").SingleOrDefault();
            var PFToSale = db.EnableSettings.Where(c => c.EnableType == "PFToSale").SingleOrDefault();
            var PFToDvNote = db.EnableSettings.Where(c => c.EnableType == "PFToDvNote").SingleOrDefault();
            var DvNoteToSale = db.EnableSettings.Where(c => c.EnableType == "DvNoteToSale").SingleOrDefault();
            var DvNoteToPF = db.EnableSettings.Where(c => c.EnableType == "DvNoteToPF").SingleOrDefault();
            var POrderToMRNote = db.EnableSettings.Where(c => c.EnableType == "POrderToMRNote").SingleOrDefault();
            var POrderToPEntry = db.EnableSettings.Where(c => c.EnableType == "POrderToPEntry").SingleOrDefault();
            var SOrderToSale = db.EnableSettings.Where(c => c.EnableType == "SOrderToSale").SingleOrDefault();
            var SOrderToPF = db.EnableSettings.Where(c => c.EnableType == "SOrderToPF").SingleOrDefault();
            var SOrderToDvNote = db.EnableSettings.Where(c => c.EnableType == "SOrderToDvNote").SingleOrDefault();
            var PQuotToPOrder = db.EnableSettings.Where(c => c.EnableType == "PQuotToPOrder").SingleOrDefault();
            var MRNotetToPEntry = db.EnableSettings.Where(c => c.EnableType == "MRNotetToPEntry").SingleOrDefault();
            var MRToPQuot = db.EnableSettings.Where(c => c.EnableType == "MRToPQuot").SingleOrDefault();

            var CustomizedDailySummary = db.EnableSettings.Where(c => c.EnableType == "CustomizedDailySummary").SingleOrDefault();
            var LastTransInSales = db.EnableSettings.Where(c => c.EnableType == "LastTransInSales").SingleOrDefault();
            var LastTransInPurchase = db.EnableSettings.Where(c => c.EnableType == "LastTransInPurchase").SingleOrDefault();

            var RepeatChequeNo = db.EnableSettings.Where(c => c.EnableType == "RepeatChequeNo").SingleOrDefault();
            var EnableCRM = db.EnableSettings.Where(c => c.EnableType == "EnableCRM").SingleOrDefault();

            var Usedmaterials = db.EnableSettings.Where(c => c.EnableType == "Usedmaterials").SingleOrDefault();
            var Usedmaterials2 = db.EnableSettings.Where(c => c.EnableType == "UsedmaterialsItemsInSE").SingleOrDefault();
            var InventoryMethod = db.EnableSettings.Where(c => c.EnableType == "InventoryMethod").SingleOrDefault();

            var Employees = db.EnableSettings.Where(c => c.EnableType == "Employees").SingleOrDefault();
            var ItemBulkUpload = db.EnableSettings.Where(c => c.EnableType == "ItemBulkUpload").SingleOrDefault();
            var EnablePayroll = db.EnableSettings.Where(c => c.EnableType == "EnablePayroll").SingleOrDefault();

            var PayAttendance = db.EnableSettings.Where(c => c.EnableType == "PayAttendance").SingleOrDefault();
            
            var enablepricestratagy = db.EnableSettings.Where(c => c.EnableType == "enablepricestratagy").SingleOrDefault();

            var Printlayout = db.EnableSettings.Where(c => c.EnableType == "Printlayout").SingleOrDefault();
            var TaxInclusive = db.EnableSettings.Where(c => c.EnableType == "TaxInclusive").SingleOrDefault();
            var stockcheckinvoice = db.EnableSettings.Where(c => c.EnableType == "stockcheckinvoice").SingleOrDefault();
            var SuperUserEdit = db.EnableSettings.Where(c => c.EnableType == "SuperUserEdit").SingleOrDefault();

            
             var materialcentrewiseminstock = db.EnableSettings.Where(c => c.EnableType == "materialcentrewiseminstock").SingleOrDefault();
            var salesrateupdateinpurchaseentrysame = db.EnableSettings.Where(c => c.EnableType == "salesrateupdateinpurchaseentrysame").SingleOrDefault();

            //printdesign--------------
            var bluedez = db.EnableSettings.Where(c => c.EnableType == "BlueDesign").SingleOrDefault();
            var Plaindez = db.EnableSettings.Where(c => c.EnableType == "PlainDesign").SingleOrDefault();
            ViewBag.MenuColor = MenuColor.TypeValue;
            ViewBag.MenuOverColor = MenuOverColor.TypeValue;

            var autovoucher = db.EnableSettings.Where(c => c.EnableType == "AutomaticVoucherNo").SingleOrDefault();

            var passwordchangedays = db.EnableSettings.Where(c => c.EnableType == "passwordchangedays").SingleOrDefault();
            var bonuscust = db.EnableSettings.Where(c => c.EnableType == "bonusforcustomer").SingleOrDefault();

            var setsellingpricefixeds = db.EnableSettings.Where(c => c.EnableType == "setsellingpricefixed").SingleOrDefault();

            //-------------------------
            EnableSettingViewModel ebleset = new EnableSettingViewModel();
            ebleset.passwordchangedays = passwordchangedays == null ? 30 : Convert.ToDecimal(passwordchangedays.TypeValue);
            if (materialcentrewiseminstock != null)
            {
                ebleset.Materialcentrewiseminimumstock = materialcentrewiseminstock.Status == 0 ? true : false;
            }
            if (setsellingpricefixeds != null)
            {
                ebleset.setsellingpricefixed = setsellingpricefixeds.Status == 0 ? true : false;
            }
            if (bluedez != null)
            {
                ebleset.Bluedesign = bluedez.Status == 0 ? true : false;
            }
            if (bonuscust != null)
            {
                ebleset.customerbonus = bonuscust.Status == 0 ? true : false;
            }
           

            if (salesrateupdateinpurchaseentrysame!=null)
            {
                ebleset.SalesRateUpdateInPurchaseEntrySame = salesrateupdateinpurchaseentrysame.Status == 0 ? true : false;

            }
            if(SuperUserEdit!=null)
            {
                ebleset.SuperUserEdit = SuperUserEdit.Status == 0 ? true : false;
            }
            if(RackWiseStock!=null)
            {
                ebleset.RackWiseStock = RackWiseStock.Status == 0 ? true : false;
            }
            if (enablepricestratagy!=null)
            {
                ebleset.enablepricestratagy = enablepricestratagy.Status == 0 ? true : false;
            }
            if (HideItemName != null)
            {
                ebleset.HideItemName = HideItemName.Status == 0 ? true : false;
            }
            if (autovoucher != null)
            {
                ebleset.AutomaticVoucherNo = autovoucher.Status == 0 ? true : false;
            }
            if (Plaindez != null)
            {
                ebleset.Plaindesign = Plaindez.Status == 0 ? true : false;
            }
            if (stockcheckinvoice != null)
            {
                ebleset.stockcheckinvoice = stockcheckinvoice.Status == 0 ? true : false;
            }
            if (bar != null)
            {
                ebleset.BarcodeChecked = bar.Status == 0 ? true : false;
            }
            if (CustomerDetails != null)
            {
                ebleset.CustomerDetailsInInvoice = CustomerDetails.Status == 0 ? true : false;
            }

            if (PartialMaterial != null)
            {
                ebleset.PartialMaterialConversion = PartialMaterial.Status == 0 ? true : false;
            }

            if (autosave != null)
            {
                ebleset.AutosaveChecked = autosave.Status == 0 ? true : false;
            }

            if (card != null)
            {
                ebleset.JobcardChecked = card.Status == 0 ? true : false;
            }
            if (dvsale != null)
            {
                ebleset.DvToSaleChecked = dvsale.Status == 0 ? true : false;
            }
            if (bom != null)
            {
                ebleset.BOMChecked = bom.Status == 0 ? true : false;
            }
            if (POS != null)
            {
                ebleset.POSInvoice = POS.Status == 0 ? true : false;
                ebleset.POSLayout = POS.TypeValue;
            }
            if (ItemCommision != null)
            {
                ebleset.ItemCommision = ItemCommision.Status == 0 ? true : false;
            }
            if (SaveAndMail != null)
            {
                ebleset.SaveAndMail = SaveAndMail.Status == 0 ? true : false;
            }
            if (AutoUserCreate != null)
            {
                ebleset.AutoCreateUser = AutoUserCreate.Status == 0 ? true : false;
            }

            if (BillToBillReceipt != null)
            {
                ebleset.BillToBillReceipt = BillToBillReceipt.Status == 0 ? true : false;
            }
            if (BillToBillPayment != null)
            {
                ebleset.BillToBillPayment = BillToBillPayment.Status == 0 ? true : false;
            }
            ebleset.PDCNotification = db.EnableSettings.Where(a => a.EnableType == "PDCNotification").Select(a => a.TypeValue).FirstOrDefault();
            if (ItemPriceInPurchase != null)
            {
                ebleset.ItemPriceInPurchase = ItemPriceInPurchase.Status == 0 ? true : false;
            }

            if (PredefinedCity != null)
            {
                ebleset.PredefinedCity = PredefinedCity.Status == 0 ? true : false;
            }

            if (MCInTransaction != null)
            {
                ebleset.MCInTransaction = MCInTransaction.Status == 0 ? true : false;
            }
            if (AssignUserMC != null)
            {
                ebleset.AssignUserMC = AssignUserMC.Status == 0 ? true : false;
            }

            if (CNBillAdjust != null)
            {


                ebleset.CNBillAdjust = CNBillAdjust.Status == 0 ? true : false;
            }
            if (DNBillAdjust != null)
            {
                ebleset.DNBillAdjust = DNBillAdjust.Status == 0 ? true : false;
            }
            if (PartNoInItem != null)
            {
                ebleset.PartNoInItem = PartNoInItem.Status == 0 ? true : false;
            }
            if (EnableBranch != null)
            {
                ebleset.EnableBranch = EnableBranch.Status == 0 ? true : false;
            }
            if (EnableJewellery != null)
            {
                ebleset.EnableJewellery = EnableJewellery.Status == 0 ? true : false;
            }
            if (EnableCurrency != null)
            {
                ebleset.EnableCurrency = EnableCurrency.Status == 0 ? true : false;
            }
            if (EnablePrefixCode != null)
            {
                ebleset.EnablePrefixCode = EnablePrefixCode.Status == 0 ? true : false;
            }
            if (EnableItemCodeInPrint != null)
            {
                ebleset.EnableItemCodeInPrint = EnableItemCodeInPrint.Status == 0 ? true : false;
            }
            if (ItemOutOfStock != null)
            {
                ebleset.ItemOutOfStock = ItemOutOfStock.Status == 0 ? true : false;
            }
            if(mcanddeliverystockeffect!=null)
            {
                ebleset.mcanddeliverystockeffect = mcanddeliverystockeffect.Status == 0 ? true : false;
            }
            if (HideComHeaders != null)
            {
                ebleset.HideComHeaders = HideComHeaders.Status == 0 ? true : false;
            }
            if (EnableItemBundle != null)
            {
                ebleset.EnableItemBundle = EnableItemBundle.Status == 0 ? true : false;
            }
            if (StckTrnsfrUpdt != null)
            {
                ebleset.StockTransferUpdate = StckTrnsfrUpdt.Status == 0 ? true : false;
            }
            if (RemoveItemData != null)
            {
                ebleset.RemoveItemData = RemoveItemData.Status == 0 ? true : false;
            }
            if (EnablePurchaseInvoice != null)
            {
                ebleset.EnablePurchaseInvoice = EnablePurchaseInvoice.Status == 0 ? true : false;
            }
            if (SalesReturnInSales != null)
            {
                ebleset.SalesReturnInSales = SalesReturnInSales.Status == 0 ? true : false;
            }
            if (AutomaticBillNoInSales != null)
            {
                ebleset.AutomaticBillNoInSales = AutomaticBillNoInSales.Status == 0 ? true : false;
            }
            if (PreventOrderConvertion != null)
            {
                ebleset.PreventOrderConvertion = PreventOrderConvertion.Status == 0 ? true : false;
            }
            if (AutomaticMailInTransactions != null)
            {
                ebleset.AutomaticMailInTransactions = AutomaticMailInTransactions.Status == 0 ? true : false;
            }
            if (EnableVoucherEdit != null)
            {
                ebleset.EnableVoucherEdit = EnableVoucherEdit.Status == 0 ? true : false;
            }
            if (Chequeprint != null)
            {
                ebleset.Chequeprint = Chequeprint.Status == 0 ? true : false;
            }
            if (DiscountPercentage != null)
            {
                ebleset.DiscountPercentage = DiscountPercentage.Status == 0 ? true : false;
            }
            if (MultiLevelApproval != null)
            {
                ebleset.MultiLevelApproval = MultiLevelApproval.Status == 0 ? true : false;
            }
            if (MLAMc != null)
            {
                ebleset.MLAMc = MLAMc.Status == 0 ? true : false;
            }
            if (MLAQuot != null)
            {
                ebleset.MLAQuot = MLAQuot.Status == 0 ? true : false;
            }
            if (MLASEntry != null)
            {
                ebleset.MLASEntry = MLASEntry.Status == 0 ? true : false;
            }
            if (MLASOrder != null)
            {
                ebleset.MLASOrder = MLASOrder.Status == 0 ? true : false;

            }
            if (MLASReturn != null)
            {
                ebleset.MLASReturn = MLASReturn.Status == 0 ? true : false;
            }
            if (MLAPQuot != null)
            {
                ebleset.MLAPQuot = MLAPQuot.Status == 0 ? true : false;
            }
            if (MLAPEntry != null)
            {
                ebleset.MLAPEntry = MLAPEntry.Status == 0 ? true : false;
            }
            if (MLAPOrder != null)
            {
                ebleset.MLAPOrder = MLAPOrder.Status == 0 ? true : false;
            }
            if (MLAPReturn != null)
            {
                ebleset.MLAPReturn = MLAPReturn.Status == 0 ? true : false;
            }
            if (MLADNote != null)
            {
                ebleset.MLADNote = MLADNote.Status == 0 ? true : false;
            }
            if (MLAJCard != null)
            {
                ebleset.MLAJCard = MLAJCard.Status == 0 ? true : false;
            }
            if (MLAPForma != null)
            {
                ebleset.MLAPForma = MLAPForma.Status == 0 ? true : false;
            }
            if (MLASTran != null)
            {
                ebleset.MLASTran = MLASTran.Status == 0 ? true : false;
            }
            if (MLASJour != null)
            {
                ebleset.MLASJour = MLASJour.Status == 0 ? true : false;
            }
            if (MLAPList != null)
            {
                ebleset.MLAPList = MLAPList.Status == 0 ? true : false;
            }
            if (MLAHReturn != null)
            {
                ebleset.MLAHReturn = MLAHReturn.Status == 0 ? true : false;
            }
            if (MLAMRNote != null)
            {
                ebleset.MLAMRNote = MLAMRNote.Status == 0 ? true : false;
            }
            if (MLAProd != null)
            {
                ebleset.MLAProd = MLAProd.Status == 0 ? true : false;
            }
            if (MLAUAssem != null)
            {
                ebleset.MLAUAssem = MLAUAssem.Status == 0 ? true : false;
            }
            if (Payment != null)
            {
                ebleset.Payment = Payment.Status == 0 ? true : false;
            }
            if (Reciept != null)
            {
                ebleset.Reciept = Reciept.Status == 0 ? true : false;
            }
            if (taskqt != null)
            {
                ebleset.qtaskstatusenable = taskqt.Status == 0 ? true : false;
                ebleset.qtaskstatus = taskqt.TypeValue;
            }
            if (Reminder != null)
            {
                ebleset.Reminder = Reminder.Status == 0 ? true : false;
            }
            if (RemindTask != null)
            {
                ebleset.RemindTask = RemindTask.Status == 0 ? true : false;
            }
            if (RemindSale != null)
            {
                ebleset.RemindSale = RemindSale.Status == 0 ? true : false;
            }
            if (RemindQuot != null)
            {
                ebleset.RemindQuot = RemindQuot.Status == 0 ? true : false;
            }
            if (RemindSReturn != null)
            {
                ebleset.RemindSReturn = RemindSReturn.Status == 0 ? true : false;
            }
            if (RemindSOrder != null)
            {
                ebleset.RemindSOrder = RemindSOrder.Status == 0 ? true : false;
            }
            if (RemindProForma != null)
            {
                ebleset.RemindProForma = RemindProForma.Status == 0 ? true : false;
            }
            if (RemindPurchase != null)
            {
                ebleset.RemindPurchase = RemindPurchase.Status == 0 ? true : false;
            }
            if (RemindPReturn != null)
            {
                ebleset.RemindPReturn = RemindPReturn.Status == 0 ? true : false;
            }
            if (RemindPOrder != null)
            {
                ebleset.RemindPOrder = RemindPOrder.Status == 0 ? true : false;
            }
            if (RemindPQuot != null)
            {
                ebleset.RemindPQuot = RemindPQuot.Status == 0 ? true : false;
            }
            if (RemindDNote != null)
            {
                ebleset.RemindDNote = RemindDNote.Status == 0 ? true : false;
            }
            if (RemindHReturn != null)
            {
                ebleset.RemindHReturn = RemindHReturn.Status == 0 ? true : false;
            }
            if (RemindMReqn != null)
            {
                ebleset.RemindMReqn = RemindMReqn.Status == 0 ? true : false;
            }
            if (RemindMRNote != null)
            {
                ebleset.RemindMRNote = RemindMRNote.Status == 0 ? true : false;
            }
            if (RemindJobCard != null)
            {
                ebleset.RemindJobCard = RemindJobCard.Status == 0 ? true : false;
            }
            if (RemindPackList != null)
            {
                ebleset.RemindPackList = RemindPackList.Status == 0 ? true : false;
            }


            if (RemindStkTrans != null)
            {
                ebleset.RemindStkTrans = RemindStkTrans.Status == 0 ? true : false;
            }
            if (RemindStkJnl != null)
            {
                ebleset.RemindStkJnl = RemindStkJnl.Status == 0 ? true : false;
            }
            if (RemindProd != null)
            {
                ebleset.RemindProd = RemindProd.Status == 0 ? true : false;
            }
            if (RemindUnass != null)
            {
                ebleset.RemindUnass = RemindUnass.Status == 0 ? true : false;
            }
            if (AccInJournal != null)
            {
                ebleset.AccInJournal = AccInJournal.Status == 0 ? true : false;
            }
            if (MakeInTrans != null)
            {
                ebleset.MakeInTrans = MakeInTrans.Status == 0 ? true : false;
            }
            if (BatchWiseStock != null)
            {
                ebleset.BatchWiseStock = BatchWiseStock.Status == 0 ? true : false;
            }

            if (PreventConversion != null)
            {
                ebleset.PreventConversion = PreventConversion.Status == 0 ? true : false;
            }
            if (QuotToSale != null)
            {
                ebleset.QuotToSale = QuotToSale.Status == 0 ? true : false;
            }
            if (QuotToPForma != null)
            {
                ebleset.QuotToPForma = QuotToPForma.Status == 0 ? true : false;
            }
            if (QuotToDvNote != null)
            {
                ebleset.QuotToDvNote = QuotToDvNote.Status == 0 ? true : false;
            }
            if (QuotToSOrder != null)
            {
                ebleset.QuotToSOrder = QuotToSOrder.Status == 0 ? true : false;
            }
            if (PFToSale != null)
            {
                ebleset.PFToSale = PFToSale.Status == 0 ? true : false;
            }
            if (PFToDvNote != null)
            {
                ebleset.PFToDvNote = PFToDvNote.Status == 0 ? true : false;
            }
            if (DvNoteToSale != null)
            {
                ebleset.DvNoteToSale = DvNoteToSale.Status == 0 ? true : false;
            }
            if (DvNoteToPF != null)
            {
                ebleset.DvNoteToPF = DvNoteToPF.Status == 0 ? true : false;
            }
            if (POrderToMRNote != null)
            {
                ebleset.POrderToMRNote = POrderToMRNote.Status == 0 ? true : false;
            }
            if (POrderToPEntry != null)
            {
                ebleset.POrderToPEntry = POrderToPEntry.Status == 0 ? true : false;
            }
            if (SOrderToSale != null)
            {
                ebleset.SOrderToSale = SOrderToSale.Status == 0 ? true : false;
            }
            if (SOrderToPF != null)
            {
                ebleset.SOrderToPF = SOrderToPF.Status == 0 ? true : false;
            }
            if (SOrderToDvNote != null)
            {
                ebleset.SOrderToDvNote = SOrderToDvNote.Status == 0 ? true : false;
            }
            if (PQuotToPOrder != null)
            {
                ebleset.PQuotToPOrder = PQuotToPOrder.Status == 0 ? true : false;
            }
            if (MRNotetToPEntry != null)
            {
                ebleset.MRNotetToPEntry = MRNotetToPEntry.Status == 0 ? true : false;
            }
            if (MRToPQuot != null)
            {
                ebleset.MRToPQuot = MRToPQuot.Status == 0 ? true : false;
            }

            if (CustomizedDailySummary != null)
            {
                ebleset.CustomizedDailySummary = CustomizedDailySummary.Status == 0 ? true : false;
            }
            if (LastTransInSales != null)
            {
                ebleset.LastTransInSales = LastTransInSales.Status == 0 ? true : false;
            }
            ebleset.LastTransSaleCount = db.EnableSettings.Where(a => a.EnableType == "LastTransInSales").Select(a => a.TypeValue).FirstOrDefault();

            if (LastTransInPurchase != null)
            {
                ebleset.LastTransInPurchase = LastTransInPurchase.Status == 0 ? true : false;
            }
            ebleset.LastTransPurCount = db.EnableSettings.Where(a => a.EnableType == "LastTransInPurchase").Select(a => a.TypeValue).FirstOrDefault();


            if (RepeatChequeNo != null)
            {
                ebleset.RepeatChequeNo = RepeatChequeNo.Status == 0 ? true : false;
            }

            if (EnableCRM != null)
            {
                ebleset.EnableCRM = EnableCRM.Status == 0 ? true : false;
            }
            if (Usedmaterials != null)
            {
                ebleset.Usedmaterials = Usedmaterials.Status == 0 ? true : false;
            }
            if (Usedmaterials2 != null)
            {
                ebleset.Usedmaterials2 = Usedmaterials2.Status == 0 ? true : false;
            }
            if (Employees != null)
            {
                ebleset.Employees = Employees.Status == 0 ? true : false;
            }
            if (ItemBulkUpload != null)
            {
                ebleset.ItemBulkUpload = ItemBulkUpload.Status == 0 ? true : false;
            }
            if (EnablePayroll != null)
            {
                ebleset.EnablePayroll = EnablePayroll.Status == 0 ? true : false;
            }
            if (CreditLimit != null)
            {
                ebleset.CreditLimit = CreditLimit.Status == 0 ? true : false;
            }
            if (TaxInclusive != null)
            {
                ebleset.TaxInclusive = TaxInclusive.Status == 0 ? true : false;
            }

            if (Printlayout != null)
            {
                ebleset.Printlayout = Printlayout.Status == 0 ? true : false;
            }
            ebleset.Invoices = db.EnableSettings.Where(a => a.EnableType == "Invoice").Select(a => a.TypeValue).FirstOrDefault();
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();
            ViewBag.invoice = QkSelect.List(invoice, "ID", "Name");

            ebleset.BusinessType = BusinessType;
            ViewBag.BisType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "General Trading", Value="General"},
                new SelectListItem() {Text = "Jewellery Management", Value="Jewellery"},
                new SelectListItem() {Text = "Scaffold Sale & Rental", Value="Scaffold"},
                new SelectListItem() {Text = "Delivery Management", Value="DeliveryManagement"},
                new SelectListItem() {Text = "Galvanizing", Value="Galvanizing"},
                new SelectListItem() {Text = "Project Based Business", Value="ProjectBasedBusiness"},
                new SelectListItem() {Text = "Workshop", Value="Workshop"},
                new SelectListItem() {Text = "Property Management", Value="Property"},
            }, "Value", "Text");
            ebleset.MenuNavColor = db.EnableSettings.Where(a => a.EnableType == "MenuColor").Select(a => a.TypeValue).FirstOrDefault();
            ebleset.MenuhOverColor = db.EnableSettings.Where(a => a.EnableType == "MenuOverColor").Select(a => a.TypeValue).FirstOrDefault();
            ebleset.SetTimeOut = db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault();
            ebleset.InventoryMethod = (InventoryMethod != null) ? InventoryMethod.TypeValue : "Basic";
            ebleset.StockValue = (StockValue != null) ? StockValue.TypeValue : "Standard";

            ViewBag.Inventory = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "FIFO", Value="Basic"},
                new SelectListItem() {Text = "Standard", Value="Average"},
            }, "Value", "Text");
            ViewBag.StockValueCost = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Standard", Value="Standard"},
                new SelectListItem() {Text = "Average", Value="Average"},
                new SelectListItem() {Text = "FIFO", Value="FIFO"},
                new SelectListItem() {Text = "LIFO", Value="LIFO"}
            }, "Value", "Text");

            ebleset.PayAttendance = (PayAttendance != null) ? PayAttendance.TypeValue : "Daily Attendance";
            ViewBag.PayAtt = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Daily Attendance", Value="Daily Attendance"},
                new SelectListItem() {Text = "Attendance Voucher", Value="Attendance Voucher"},
            }, "Value", "Text");

            //Material Centre
            //                 .Select(s => new
            //                     ID = s.MCId,
            //                     Name = s.MCName,

            return View(ebleset);
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,EnableSettings")]
        public ActionResult action(EnableSettingViewModel vmodel)
        {
            string role = Request.Form["Role"];
            if (!String.IsNullOrEmpty(role))
            {
                string[] mod = role.Split(',');
                foreach (var item in mod)
                {
                    AppModules module = db.AppModuless.Find(item);
                    module.Status = Status.inactive;
                    db.Entry(module).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            string roleen = Request.Form["Roleen"];
            if (!String.IsNullOrEmpty(roleen))
            {
                string[] mod = roleen.Split(',');
                foreach (var item in mod)
                {
                    AppModules module = db.AppModuless.Find(item);
                    module.Status = Status.active;
                    db.Entry(module).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            // enable based on project

            if (vmodel.BusinessType != "ProjectBasedBusiness")
            {

                //project
                AppModules project = db.AppModuless.Find("9c79d96a-2ac2-40cc-98d1-a6a8ab28624a");
                project.Status = Status.inactive;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                //project type
                AppModules projecttype = db.AppModuless.Find("2e7f175f-c85c-4c89-bdbd-d4a3a6ae0068");
                projecttype.Status = Status.inactive;
                db.Entry(projecttype).State = EntityState.Modified;
                db.SaveChanges();

                //project status
                AppModules projectstatus = db.AppModuless.Find("07b09dd2-0f04-4122-84dc-aa2aee09680d");
                projectstatus.Status = Status.inactive;
                db.Entry(projectstatus).State = EntityState.Modified;
                db.SaveChanges();

                //material requisition
                AppModules modulereq = db.AppModuless.Find("b6f80f6f-9ad9-4752-b296-1c276e431926");
                modulereq.Status = Status.inactive;
                db.Entry(modulereq).State = EntityState.Modified;
                db.SaveChanges();

                //material requisition report
                AppModules modulereqreport = db.AppModuless.Find("e5a97486-69b7-46cf-9531-9d39b9e076fa");
                modulereqreport.Status = Status.inactive;
                db.Entry(modulereqreport).State = EntityState.Modified;
                db.SaveChanges();

                //material recievnote
                AppModules modulerec = db.AppModuless.Find("d43f3da0-b0f2-423f-9390-a71511ffd943");
                modulerec.Status = Status.inactive;
                db.Entry(modulerec).State = EntityState.Modified;
                db.SaveChanges();

                //material recievnote report
                AppModules modulerecreport = db.AppModuless.Find("68ff7771-67d8-419b-bf45-8f5e1dff00eb");
                modulerecreport.Status = Status.inactive;
                db.Entry(modulerecreport).State = EntityState.Modified;
                db.SaveChanges();

                //Project report
                AppModules moduleProjectreport = db.AppModuless.Find("7f62c8df-bb92-48f5-896f-8c41113a3a87");
                moduleProjectreport.Status = Status.inactive;
                db.Entry(moduleProjectreport).State = EntityState.Modified;
                db.SaveChanges();

                //task
                AppModules tasks = db.AppModuless.Find("d649c819-02e7-4ecd-8ffe-89105f68f3fb");
                tasks.Status = Status.inactive;
                db.Entry(tasks).State = EntityState.Modified;
                db.SaveChanges();

                //processflow
                AppModules processflow = db.AppModuless.Find("0fe02951-7cda-4a0a-ac5e-67c352bf5f25");
                processflow.Status = Status.inactive;
                db.Entry(processflow).State = EntityState.Modified;
                db.SaveChanges();

                //check list
                AppModules checklist = db.AppModuless.Find("ecd59288-faba-4b87-aa4c-0f505c23ec16");
                checklist.Status = Status.inactive;
                db.Entry(checklist).State = EntityState.Modified;
                db.SaveChanges();

                //team
                AppModules team = db.AppModuless.Find("55bb2537-74d1-447a-80e3-02750ded7673");
                team.Status = Status.inactive;
                db.Entry(team).State = EntityState.Modified;
                db.SaveChanges();

                //tasktype
                AppModules tasktype = db.AppModuless.Find("00e11cbc-3dfe-4502-b149-408277dc5f20");
                tasktype.Status = Status.inactive;
                db.Entry(tasktype).State = EntityState.Modified;
                db.SaveChanges();

                //taskstatus
                AppModules taskstatus = db.AppModuless.Find("ec6bd2af-de47-4cad-86d2-76ff9bffa284");
                taskstatus.Status = Status.inactive;
                db.Entry(taskstatus).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                //project
                AppModules project = db.AppModuless.Find("9c79d96a-2ac2-40cc-98d1-a6a8ab28624a");
                project.Status = Status.active;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                //project type
                AppModules projecttype = db.AppModuless.Find("2e7f175f-c85c-4c89-bdbd-d4a3a6ae0068");
                projecttype.Status = Status.active;
                db.Entry(projecttype).State = EntityState.Modified;
                db.SaveChanges();

                //project status
                AppModules projectstatus = db.AppModuless.Find("07b09dd2-0f04-4122-84dc-aa2aee09680d");
                projectstatus.Status = Status.active;
                db.Entry(projectstatus).State = EntityState.Modified;
                db.SaveChanges();

                //material requisition
                AppModules modulereq = db.AppModuless.Find("b6f80f6f-9ad9-4752-b296-1c276e431926");
                modulereq.Status = Status.active;
                db.Entry(modulereq).State = EntityState.Modified;
                db.SaveChanges();

                //material requisition report
                AppModules modulereqreport = db.AppModuless.Find("e5a97486-69b7-46cf-9531-9d39b9e076fa");
                modulereqreport.Status = Status.active;
                db.Entry(modulereqreport).State = EntityState.Modified;
                db.SaveChanges();

                //material recievnote
                AppModules modulerec = db.AppModuless.Find("d43f3da0-b0f2-423f-9390-a71511ffd943");
                modulerec.Status = Status.active;
                db.Entry(modulerec).State = EntityState.Modified;
                db.SaveChanges();

                //material recievnote report
                AppModules modulerecreport = db.AppModuless.Find("68ff7771-67d8-419b-bf45-8f5e1dff00eb");
                modulerecreport.Status = Status.active;
                db.Entry(modulerecreport).State = EntityState.Modified;
                db.SaveChanges();

                //Project report
                AppModules moduleProjectreport = db.AppModuless.Find("7f62c8df-bb92-48f5-896f-8c41113a3a87");
                moduleProjectreport.Status = Status.active;
                db.Entry(moduleProjectreport).State = EntityState.Modified;
                db.SaveChanges();

                //task
                AppModules tasks = db.AppModuless.Find("d649c819-02e7-4ecd-8ffe-89105f68f3fb");
                tasks.Status = Status.active;
                db.Entry(tasks).State = EntityState.Modified;
                db.SaveChanges();
                //process flow
                AppModules processflow = db.AppModuless.Find("0fe02951-7cda-4a0a-ac5e-67c352bf5f25");
                processflow.Status = Status.active;
                db.Entry(processflow).State = EntityState.Modified;
                db.SaveChanges();

                //check list
                AppModules checklist = db.AppModuless.Find("ecd59288-faba-4b87-aa4c-0f505c23ec16");
                checklist.Status = Status.active;
                db.Entry(checklist).State = EntityState.Modified;
                db.SaveChanges();

                //team
                AppModules team = db.AppModuless.Find("55bb2537-74d1-447a-80e3-02750ded7673");
                team.Status = Status.active;
                db.Entry(team).State = EntityState.Modified;
                db.SaveChanges();

                //tasktype
                AppModules tasktype = db.AppModuless.Find("00e11cbc-3dfe-4502-b149-408277dc5f20");
                tasktype.Status = Status.active;
                db.Entry(tasktype).State = EntityState.Modified;
                db.SaveChanges();

                //taskstatus
                AppModules taskstatus = db.AppModuless.Find("ec6bd2af-de47-4cad-86d2-76ff9bffa284");
                taskstatus.Status = Status.active;
                db.Entry(taskstatus).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (vmodel.BusinessType != "Workshop")
            {
                AppModules project = db.AppModuless.Find("fdb5ecb3-9696-4f52-8a60-856ba14f99Vehicle Type");
                project.Status = Status.inactive;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                AppModules project1 = db.AppModuless.Find("fdb5ecb3-9696-4f52-8a60-856ba14f99manufacturer");
                project.Status = Status.inactive;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                AppModules project2 = db.AppModuless.Find("fdb5ecb3-9696-4f52-8a60-856ba14f99model");
                project.Status = Status.inactive;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                AppModules project3 = db.AppModuless.Find("782d9129-977a-43c2-b0ba-6f1f3adbccb8tworkshop");
                project.Status = Status.inactive;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                AppModules project = db.AppModuless.Find("fdb5ecb3-9696-4f52-8a60-856ba14f99Vehicle Type");
                project.Status = Status.active;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                AppModules project1 = db.AppModuless.Find("fdb5ecb3-9696-4f52-8a60-856ba14f99manufacturer");
                project.Status = Status.active;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                AppModules project2 = db.AppModuless.Find("fdb5ecb3-9696-4f52-8a60-856ba14f99model");
                project.Status = Status.active;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                AppModules project3 = db.AppModuless.Find("782d9129-977a-43c2-b0ba-6f1f3adbccb8tworkshop");
                project.Status = Status.active;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (vmodel.BusinessType != "Scaffold")
            {
                // Hire return
                AppModules hreturns = db.AppModuless.Find("f5f0c78d-b6cc-497e-b465-92622ed984e1");
                hreturns.Status = Status.inactive;
                db.Entry(hreturns).State = EntityState.Modified;
                db.SaveChanges();

                //Hire return report
                AppModules Hirereturnre = db.AppModuless.Find("2ecbedb9-a0e4-4b72-9462-3aef2e20ab2d");
                Hirereturnre.Status = Status.inactive;
                db.Entry(Hirereturnre).State = EntityState.Modified;
                db.SaveChanges();

                //cross Hire return
                AppModules crosshreturns = db.AppModuless.Find("d3d9d0c0-58ce-4761-8b74-e1c87d1e7778");
                crosshreturns.Status = Status.inactive;
                db.Entry(crosshreturns).State = EntityState.Modified;
                db.SaveChanges();

                //cross Hire return report
                AppModules crosshreturn = db.AppModuless.Find("81a7bbbb-aa25-4edb-938e-36ceeac689b5");
                crosshreturn.Status = Status.inactive;
                db.Entry(crosshreturn).State = EntityState.Modified;
                db.SaveChanges();

                //Hire Expire report
                AppModules HireExpire = db.AppModuless.Find("1f562886-71a1-4b58-9cd0-4bfdd4f719f7");
                HireExpire.Status = Status.inactive;
                db.Entry(HireExpire).State = EntityState.Modified;
                db.SaveChanges();

                //cross Hire expire stock report
                AppModules crossHirestockreport = db.AppModuless.Find("fb8e844a-c6e3-4d4d-80e5-abad713e641d");
                crossHirestockreport.Status = Status.inactive;
                db.Entry(crossHirestockreport).State = EntityState.Modified;
                db.SaveChanges();

                //Hire stock report
                AppModules Hirestockreport = db.AppModuless.Find("49de2bb3-8c39-4dd7-ae96-819e3483e60a");
                Hirestockreport.Status = Status.inactive;
                db.Entry(Hirestockreport).State = EntityState.Modified;
                db.SaveChanges();

                //packing list
                AppModules packinglist = db.AppModuless.Find("63db726f-7417-4af3-84ca-0a2d5798ada6");
                packinglist.Status = Status.inactive;
                db.Entry(packinglist).State = EntityState.Modified;
                db.SaveChanges();

                //sticker print
                AppModules stickerprint = db.AppModuless.Find("42016ef3-dc13-4861-a2c9-3ad2fc493399");
                if (stickerprint != null)
                {
                    stickerprint.Status = Status.inactive;
                    db.Entry(stickerprint).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                // Hire return
                AppModules hreturns = db.AppModuless.Find("f5f0c78d-b6cc-497e-b465-92622ed984e1");
                hreturns.Status = Status.active;
                db.Entry(hreturns).State = EntityState.Modified;
                db.SaveChanges();

                //Hire return report
                AppModules Hirereturnre = db.AppModuless.Find("2ecbedb9-a0e4-4b72-9462-3aef2e20ab2d");
                Hirereturnre.Status = Status.active;
                db.Entry(Hirereturnre).State = EntityState.Modified;
                db.SaveChanges();

                //cross Hire return
                AppModules crosshreturns = db.AppModuless.Find("d3d9d0c0-58ce-4761-8b74-e1c87d1e7778");
                crosshreturns.Status = Status.active;
                db.Entry(crosshreturns).State = EntityState.Modified;
                db.SaveChanges();

                //cross Hire return report
                AppModules crosshreturn = db.AppModuless.Find("81a7bbbb-aa25-4edb-938e-36ceeac689b5");
                crosshreturn.Status = Status.active;
                db.Entry(crosshreturn).State = EntityState.Modified;
                db.SaveChanges();

                //Hire Expire report
                AppModules HireExpire = db.AppModuless.Find("1f562886-71a1-4b58-9cd0-4bfdd4f719f7");
                HireExpire.Status = Status.active;
                db.Entry(HireExpire).State = EntityState.Modified;
                db.SaveChanges();

                //cross Hire expire stock report
                AppModules crossHirestockreport = db.AppModuless.Find("fb8e844a-c6e3-4d4d-80e5-abad713e641d");
                crossHirestockreport.Status = Status.active;
                db.Entry(crossHirestockreport).State = EntityState.Modified;
                db.SaveChanges();

                //Hire stock report
                AppModules Hirestockreport = db.AppModuless.Find("49de2bb3-8c39-4dd7-ae96-819e3483e60a");
                Hirestockreport.Status = Status.active;
                db.Entry(Hirestockreport).State = EntityState.Modified;
                db.SaveChanges();

                //packing list
                AppModules packinglist = db.AppModuless.Find("63db726f-7417-4af3-84ca-0a2d5798ada6");
                packinglist.Status = Status.active;
                db.Entry(packinglist).State = EntityState.Modified;
                db.SaveChanges();

                //sticker print
            }
            if (vmodel.RackWiseStock)
            {
                addMethod("RackWiseStock", Status.active, null);
            }
          
            else
            {
                addMethod("RackWiseStock", Status.inactive, null);
            }
            if (vmodel.setsellingpricefixed)
            {
                addMethod("setsellingpricefixed", Status.active, null);
            }

            else
            {
                addMethod("setsellingpricefixed", Status.inactive, null);
            }
            if (vmodel.customerbonus)
            {
                addMethod("bonusforcustomer", Status.active, null);
            }

            else
            {
                addMethod("bonusforcustomer", Status.inactive, null);
            }
            if (vmodel.enablepricestratagy)
            {
                addMethod("enablepricestratagy", Status.active, null);
            }
            else
            {
                addMethod("enablepricestratagy", Status.inactive, null);
            }
            if (vmodel.SuperUserEdit)
            {
                addMethod("SuperUserEdit", Status.active, null);
            }
            else
            {
                addMethod("SuperUserEdit", Status.inactive, null);
            }
            if (vmodel.HideItemName)
            {
                addMethod("HideItemName", Status.active, null);
            }
            else
            {
                addMethod("HideItemName", Status.inactive, null);
            }
            //barcode settings
            if (vmodel.SalesRateUpdateInPurchaseEntrySame)
            {
                addMethod("salesrateupdateinpurchaseentrysame", Status.active, null);
            }
            else
            {
                addMethod("salesrateupdateinpurchaseentrysame", Status.inactive, null);
            }
            if (vmodel.BarcodeChecked)
            {
                addMethod("Barcode", Status.active, null);
            }
            else
            {
                addMethod("Barcode", Status.inactive, null);
            }

            if (vmodel.CustomerDetailsInInvoice)
            {
                addMethod("CustomerDetailInInvoice", Status.active, null);
            }
            else
            {
                addMethod("CustomerDetailInInvoice", Status.inactive, null);
            }

            if (vmodel.PartialMaterialConversion)
            {
                addMethod("PartialMaterialConversion", Status.active, null);
            }
            else
            {
                addMethod("PartialMaterialConversion", Status.inactive, null);
            }
            if(vmodel.Materialcentrewiseminimumstock)
            {
                addMethod("materialcentrewiseminstock", Status.active, null);
            }
            else
            {
                addMethod("materialcentrewiseminstock", Status.inactive, null);
            }
            if (vmodel.AutomaticVoucherNo)
            {
                addMethod("AutomaticVoucherNo", Status.active, null);
            }
            else
            {
                addMethod("AutomaticVoucherNo", Status.inactive, null);
            }
            if (vmodel.AutosaveChecked)
            {
                addMethod("Autosave", Status.active, null);
            }
            else
            {
                addMethod("Autosave", Status.inactive, null);
            }

            if (vmodel.stockcheckinvoice)
            {
                addMethod("stockcheckinvoice", Status.active, null);
            }
            else
            {
                addMethod("stockcheckinvoice", Status.inactive, null);
            }
            if(vmodel.qtaskstatusenable)
            {
                addMethod("taskqt", Status.active, vmodel.qtaskstatus);
            }
            else
            {
                addMethod("taskqt", Status.inactive, vmodel.qtaskstatus);
            }
            //jobcaed enable

            if (vmodel.JobcardChecked)
            {
                addMethod("Jobcard", Status.active, null);

                //Jobcard
                AppModules module = db.AppModuless.Find("c543bbe5-bd63-456e-8b24-400c45fe178c");
                module.Status = Status.active;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                //Jobcard item setting
                AppModules mdle = db.AppModuless.Find("0f9b5b36-ced7-45af-8132-d3ff68178b1b");
                mdle.Status = Status.active;
                db.Entry(mdle).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                //Jobcard
                AppModules module = db.AppModuless.Find("c543bbe5-bd63-456e-8b24-400c45fe178c");
                module.Status = Status.inactive;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                //Jobcard item setting
                AppModules mdle = db.AppModuless.Find("0f9b5b36-ced7-45af-8132-d3ff68178b1b");
                mdle.Status = Status.inactive;
                db.Entry(mdle).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("Jobcard", Status.inactive, null);
            }
            //delivery note to sale enabled
            if (vmodel.DvToSaleChecked)
            {
                addMethod("DvToSale", Status.active, null);
            }
            else
            {
                addMethod("DvToSale", Status.inactive, null);
            }

            //bom , production and unassembles enable
            if (vmodel.BOMChecked)
            {
                //bom
                AppModules module = db.AppModuless.Find("beb8be40-5ab2-4d49-b080-bb75b4441288");
                module.Status = Status.active;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                //production
                AppModules mdle = db.AppModuless.Find("12c662bd-a798-4363-a77a-07ce1fd49560");
                mdle.Status = Status.active;
                db.Entry(mdle).State = EntityState.Modified;
                db.SaveChanges();
                //unassemble
                AppModules mdle1 = db.AppModuless.Find("d11c2f8d-6dee-4c6a-8dd3-0691b4b175ee");
                mdle1.Status = Status.active;
                db.Entry(mdle1).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("BOM", Status.active, null);
            }
            else
            {
                //bom
                AppModules module = db.AppModuless.Find("beb8be40-5ab2-4d49-b080-bb75b4441288");
                module.Status = Status.inactive;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                //production
                AppModules mdle = db.AppModuless.Find("12c662bd-a798-4363-a77a-07ce1fd49560");
                mdle.Status = Status.inactive;
                db.Entry(mdle).State = EntityState.Modified;
                db.SaveChanges();

                //unassemble
                AppModules mdle1 = db.AppModuless.Find("d11c2f8d-6dee-4c6a-8dd3-0691b4b175ee");
                mdle1.Status = Status.inactive;
                db.Entry(mdle1).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("BOM", Status.inactive, null);
            }

            //Enable POS Invoice
            if (vmodel.POSInvoice)
            {
                addMethod("POSInvoice", Status.active, vmodel.POSLayout);
            }
            else
            {
                addMethod("POSInvoice", Status.inactive, null);
            }

            //Enable ItemCommision
            if (vmodel.ItemCommision)
            {
                // ItemCommision report
                AppModules modules = db.AppModuless.Find("41e46678-f7e0-47ac-82ae-95ab37bf8e38");
                modules.Status = Status.active;
                db.Entry(modules).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("ItemCommision", Status.active, null);
            }
            else
            {
                AppModules modules = db.AppModuless.Find("41e46678-f7e0-47ac-82ae-95ab37bf8e38");
                modules.Status = Status.inactive;
                db.Entry(modules).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("ItemCommision", Status.inactive, null);
            }

            //Enable SaveAndMail
            if (vmodel.SaveAndMail)
            {
                addMethod("SaveAndMail", Status.active, null);
            }
            else
            {
                addMethod("SaveAndMail", Status.inactive, null);
            }
            //Enable AutoCreateUser
            if (vmodel.AutoCreateUser)
            {
                addMethod("AutoCreateUser", Status.active, null);
            }
            else
            {
                addMethod("AutoCreateUser", Status.inactive, null);
            }

            //Enable Bill To Bill Receipt
            if (vmodel.BillToBillReceipt)
            {
                addMethod("BillToBillReceipt", Status.active, null);
            }
            else
            {
                addMethod("BillToBillReceipt", Status.inactive, null);
            }

            if (vmodel.StockTransferUpdate)
            {
                addMethod("StockTransferUpdate", Status.active, null);
            }
            else
            {
                addMethod("StockTransferUpdate", Status.inactive, null);
            }

            //Enable Bill To Bill Payment
            if (vmodel.BillToBillPayment)
            {
                addMethod("BillToBillPayment", Status.active, null);
            }
            else
            {
                addMethod("BillToBillPayment", Status.inactive, null);
            }
            //printdesign
            if (vmodel.Bluedesign)
            {
                addMethod("BlueDesign", Status.active, null);
            }
            else
            {
                addMethod("BlueDesign", Status.inactive, null);
            }
            if (vmodel.Plaindesign)
            {
                addMethod("PlainDesign", Status.active, null);
            }
            else
            {
                addMethod("PlainDesign", Status.inactive, null);
            }


            //PDC Notification days 
            addMethod("PDCNotification", Status.active, vmodel.PDCNotification);

            if (vmodel.ItemPriceInPurchase)
            {
                addMethod("ItemPriceInPurchase", Status.active, null);
            }
            else
            {
                addMethod("ItemPriceInPurchase", Status.inactive, null);
            }

            if (vmodel.PredefinedCity)
            {
                addMethod("PredefinedCity", Status.active, null);
            }
            else
            {
                addMethod("PredefinedCity", Status.inactive, null);
            }

            if (vmodel.MCInTransaction)
            {
                AppModules module = db.AppModuless.Find("d32544ad-77d7-4eb8-ad98-8012e9acd735");
                if (module != null)
                {
                    module.Status = Status.active;
                    db.Entry(module).State = EntityState.Modified;
                    db.SaveChanges();
                }
                ////stock journal

                //stock transfer
                AppModules stmodule = db.AppModuless.Find("4ffedfb0-9f0e-4710-a3aa-94b7b3cb2335");
                if (module != null)
                {
                    stmodule.Status = Status.active;
                    db.Entry(stmodule).State = EntityState.Modified;
                    db.SaveChanges();
                }
                addMethod("MCInTransaction", Status.active, null);
            }
            else
            {
                AppModules module = db.AppModuless.Find("d32544ad-77d7-4eb8-ad98-8012e9acd735");
                if (module != null)
                {
                    module.Status = Status.inactive;
                    db.Entry(module).State = EntityState.Modified;
                    db.SaveChanges();
                }
                ////stock journal

                //stock transfer
                AppModules stmodule = db.AppModuless.Find("4ffedfb0-9f0e-4710-a3aa-94b7b3cb2335");
                if (module != null)
                {
                    stmodule.Status = Status.inactive;
                    db.Entry(stmodule).State = EntityState.Modified;
                    db.SaveChanges();
                }
                addMethod("MCInTransaction", Status.inactive, null);
            }
            //assigned user to mc
            if (vmodel.AssignUserMC)
            {
                addMethod("AssignUserMC", Status.active, null);
            }
            else
            {
                //set user to null in MC
                db.MCs.ToList().ForEach(f => f.AssignedUser = null);
                db.SaveChanges();

                addMethod("AssignUserMC", Status.inactive, null);
            }


            //creditnote bill adj
            if (vmodel.CNBillAdjust)
            {
                addMethod("CNBillAdjust", Status.active, null);
            }
            else
            {
                addMethod("CNBillAdjust", Status.inactive, null);
            }

            //debitnote bill adj
            if (vmodel.DNBillAdjust)
            {
                addMethod("DNBillAdjust", Status.active, null);
            }
            else
            {
                addMethod("DNBillAdjust", Status.inactive, null);
            }

            if (vmodel.PartNoInItem)
            {
                addMethod("PartNoInItem", Status.active, null);
            }
            else
            {
                addMethod("PartNoInItem", Status.inactive, null);
            }
            if (vmodel.EnableBranch)
            {
                AppModules module = db.AppModuless.Find("a836e8aa-71e9-4c15-83c9-9fb4b862bf4f");
                module.Status = Status.active;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableBranch", Status.active, null);
            }
            else
            {
                AppModules module = db.AppModuless.Find("a836e8aa-71e9-4c15-83c9-9fb4b862bf4f");
                module.Status = Status.inactive;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableBranch", Status.inactive, null);
            }
            if (vmodel.EnableJewellery)
            {
                // stock veryfication
                AppModules module = db.AppModuless.Find("e45bcf56-249a-41f9-99a0-be695fa86919");
                module.Status = Status.active;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                // Jewellery Barcode
                AppModules modules = db.AppModuless.Find("9430e424-bfff-4058-b800-a66aeff888f5");
                modules.Status = Status.active;
                db.Entry(modules).State = EntityState.Modified;
                db.SaveChanges();

                // stock moment == Stock Enquiry
                AppModules modulez = db.AppModuless.Find("f43f5d8e-c474-4b61-a36c-447e212fc1f1");
                modulez.viewName = "Stock Enquiry";
                db.Entry(modulez).State = EntityState.Modified;
                db.SaveChanges();

                //c8cf73c5-ba38-4f2c-aa56-7e491034bacd

                AppModules modulesz = db.AppModuless.Find("c8cf73c5-ba38-4f2c-aa56-7e491034bacd");
                modulesz.Status = Status.inactive;
                db.Entry(modulesz).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableJewellery", Status.active, null);
            }
            else
            {
                // stock veryfication
                AppModules module = db.AppModuless.Find("e45bcf56-249a-41f9-99a0-be695fa86919");
                module.Status = Status.inactive;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                // Jewellery Barcode
                AppModules modules = db.AppModuless.Find("9430e424-bfff-4058-b800-a66aeff888f5");
                modules.Status = Status.inactive;
                db.Entry(modules).State = EntityState.Modified;
                db.SaveChanges();

                // stock moment == Stock Enquiry
                AppModules modulez = db.AppModuless.Find("f43f5d8e-c474-4b61-a36c-447e212fc1f1");
                modulez.viewName = "Stock Moment";
                db.Entry(modulez).State = EntityState.Modified;
                db.SaveChanges();

                AppModules modulesz = db.AppModuless.Find("c8cf73c5-ba38-4f2c-aa56-7e491034bacd");
                modulesz.Status = Status.active;
                db.Entry(modulesz).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableJewellery", Status.inactive, null);
            }
            if (vmodel.EnableCurrency)
            {
                // currency
                AppModules module = db.AppModuless.Find("d1bd9270-a4a2-4208-a525-afccb13b43d6");
                module.Status = Status.active;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableCurrency", Status.active, null);
            }
            else
            {
                // currency
                AppModules module = db.AppModuless.Find("d1bd9270-a4a2-4208-a525-afccb13b43d6");
                module.Status = Status.inactive;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableCurrency", Status.inactive, null);
            }
            if (vmodel.EnablePrefixCode)
            {
                //prefix master
                AppModules module = db.AppModuless.Find("3494da2b-d9f1-4edb-931c-532e0722d8d4");
                module.Status = Status.active;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnablePrefixCode", Status.active, null);
            }
            else
            {
                //prefix master
                AppModules module = db.AppModuless.Find("3494da2b-d9f1-4edb-931c-532e0722d8d4");
                module.Status = Status.inactive;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnablePrefixCode", Status.inactive, null);
            }
            //itemcode invoice in print
            if (vmodel.EnableItemCodeInPrint)
            {
                addMethod("EnableItemCodeInPrint", Status.active, null);
            }
            else
            {
                addMethod("EnableItemCodeInPrint", Status.inactive, null);
            }

            if (vmodel.ItemOutOfStock)
            {
                addMethod("ItemOutOfStock", Status.active, null);
            }
            else
            {
                addMethod("ItemOutOfStock", Status.inactive, null);
            }
            if(vmodel.mcanddeliverystockeffect)
            {
                addMethod("mcanddeliverystockeffect", Status.active, null);
            }
            else
            {
                addMethod("mcanddeliverystockeffect", Status.inactive, null);
            }

            if (vmodel.HideComHeaders)
            {
                addMethod("HideComHeaders", Status.active, null);
            }
            else
            {
                addMethod("HideComHeaders", Status.inactive, null);
            }

            if (vmodel.EnableItemBundle)
            {
                AppModules ibmodule = db.AppModuless.Find("18b9a672-9188-4717-b2d6-645d853f5d25");
                ibmodule.Status = Status.active;
                db.Entry(ibmodule).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableItemBundle", Status.active, null);
            }
            else
            {
                AppModules ibmodule = db.AppModuless.Find("18b9a672-9188-4717-b2d6-645d853f5d25");
                ibmodule.Status = Status.inactive;
                db.Entry(ibmodule).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableItemBundle", Status.inactive, null);
            }

            if (vmodel.RemoveItemData)
            {
                addMethod("RemoveItemData", Status.active, null);
            }
            else
            {
                addMethod("RemoveItemData", Status.inactive, null);
            }

            //Default sales invoice 
            addMethod("Invoice", Status.active, vmodel.Invoices);

            //Business Type
            addMethod("BusinessType", Status.active, vmodel.BusinessType);

            //Menu Color
            addMethod("MenuColor", Status.active, vmodel.MenuNavColor);

            //Menu Hover Color
            addMethod("MenuOverColor", Status.active, vmodel.MenuhOverColor);

            addMethod("SetTimeOut", Status.active, vmodel.SetTimeOut);
            addMethod("passwordchangedays", Status.active, vmodel.passwordchangedays.ToString());
            if (vmodel.EnablePurchaseInvoice)
            {
                addMethod("EnablePurchaseInvoice", Status.active, null);
            }
            else
            {
                addMethod("EnablePurchaseInvoice", Status.inactive, null);
            }

            if (vmodel.SalesReturnInSales)
            {
                addMethod("SalesReturnInSales", Status.active, null);
            }
            else
            {
                addMethod("SalesReturnInSales", Status.inactive, null);
            }
            if (vmodel.AutomaticBillNoInSales)
            {
                addMethod("AutomaticBillNoInSales", Status.active, null);
            }
            else
            {
                addMethod("AutomaticBillNoInSales", Status.inactive, null);
            }

            if (vmodel.PreventOrderConvertion)
            {
                addMethod("PreventOrderConvertion", Status.active, null);
            }
            else
            {
                addMethod("PreventOrderConvertion", Status.inactive, null);
            }
            if (vmodel.AutomaticMailInTransactions)
            {
                addMethod("AutomaticMailInTransactions", Status.active, null);
            }
            else
            {
                addMethod("AutomaticMailInTransactions", Status.inactive, null);
            }
            if (vmodel.EnableVoucherEdit)
            {
                addMethod("EnableVoucherEdit", Status.active, null);
            }
            else
            {
                addMethod("EnableVoucherEdit", Status.inactive, null);
            }
            //Enable Chequeprint
            if (vmodel.Chequeprint)
            {
                addMethod("Chequeprint", Status.active, null);
            }
            else
            {
                addMethod("Chequeprint", Status.inactive, null);
            }

            //Enable DiscountPercentage
            if (vmodel.DiscountPercentage)
            {
                addMethod("DiscountPercentage", Status.active, null);
            }
            else
            {
                addMethod("DiscountPercentage", Status.inactive, null);
            }
            if (vmodel.MultiLevelApproval)
            {
                addMethod("MultiLevelApproval", Status.active, null);
                //multi level approval
                AppModules modules = db.AppModuless.Find("c24a8054-9c3f-43fa-b040-63ff9914260b");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MultiLevelApproval", Status.inactive, null);
                //multi level approval
                AppModules modules = db.AppModuless.Find("c24a8054-9c3f-43fa-b040-63ff9914260b");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAMc)
            {
                addMethod("MLAMc", Status.active, null);
                AppModules modules = db.AppModuless.Find("d756e27a-a2ff-4794-9875-bbe1dc8a000b");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAMc", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("d756e27a-a2ff-4794-9875-bbe1dc8a000b");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAQuot)
            {
                addMethod("MLAQuot", Status.active, null);
                AppModules modules = db.AppModuless.Find("04000036-96a0-43e1-9c10-439df7c944bc");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAQuot", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("04000036-96a0-43e1-9c10-439df7c944bc");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLASEntry)
            {
                addMethod("MLASEntry", Status.active, null);
                //diasble Sale Edit After Approval
                AppModules modules = db.AppModuless.Find("6d358828-d832-41a7-bcda-29a896dc5106");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLASEntry", Status.inactive, null);
                //diasble Sale Edit After Approval
                AppModules modules = db.AppModuless.Find("6d358828-d832-41a7-bcda-29a896dc5106");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLASOrder)
            {
                addMethod("MLASOrder", Status.active, null);
                AppModules modules = db.AppModuless.Find("e6264892-dee0-4241-acf4-acb842d1c3cb");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLASOrder", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("e6264892-dee0-4241-acf4-acb842d1c3cb");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLASReturn)
            {
                addMethod("MLASReturn", Status.active, null);
                AppModules modules = db.AppModuless.Find("58e0fdee-f791-4886-a17c-5bfe56f804a8");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLASReturn", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("58e0fdee-f791-4886-a17c-5bfe56f804a8");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAPQuot)
            {
                addMethod("MLAPQuot", Status.active, null);
                AppModules modules = db.AppModuless.Find("e31adfda-6d2e-4f12-bee0-f7086b3434be");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAPQuot", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("e31adfda-6d2e-4f12-bee0-f7086b3434be");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAPEntry)
            {
                addMethod("MLAPEntry", Status.active, null);
                AppModules modules = db.AppModuless.Find("456c90d0-860f-439d-b3a1-ba05f75621b7");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAPEntry", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("456c90d0-860f-439d-b3a1-ba05f75621b7");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAPOrder)
            {
                addMethod("MLAPOrder", Status.active, null);
                AppModules modules = db.AppModuless.Find("9c2a07d5-36fb-49e0-b397-acd6ad4392fc");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAPOrder", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("9c2a07d5-36fb-49e0-b397-acd6ad4392fc");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAPReturn)
            {
                addMethod("MLAPReturn", Status.active, null);
                AppModules modules = db.AppModuless.Find("32e31c9b-f6ed-464a-ab23-1eeda63e18aa");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAPReturn", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("32e31c9b-f6ed-464a-ab23-1eeda63e18aa");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLADNote)
            {
                addMethod("MLADNote", Status.active, null);
                AppModules modules = db.AppModuless.Find("cab1eafe-a6ff-4585-b76e-7e20f6e81805");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLADNote", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("cab1eafe-a6ff-4585-b76e-7e20f6e81805");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAJCard)
            {
                addMethod("MLAJCard", Status.active, null);
                AppModules modules = db.AppModuless.Find("ae3e13a4-970b-4614-9022-4dd4f1a1b123");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAJCard", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("ae3e13a4-970b-4614-9022-4dd4f1a1b123");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAPForma)
            {
                addMethod("MLAPForma", Status.active, null);
                AppModules modules = db.AppModuless.Find("d544190c-2472-4a53-bb82-d88e5566a8c6");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAPForma", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("d544190c-2472-4a53-bb82-d88e5566a8c6");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLASTran)
            {
                addMethod("MLASTran", Status.active, null);
                AppModules modules = db.AppModuless.Find("c1b5573e-0573-4ae0-9062-9ea8dfd71574");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLASTran", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("c1b5573e-0573-4ae0-9062-9ea8dfd71574");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLASJour)
            {
                addMethod("MLASJour", Status.active, null);
                AppModules modules = db.AppModuless.Find("edeac41c-aa2d-43d0-8fa9-0ccd6f083467");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLASJour", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("edeac41c-aa2d-43d0-8fa9-0ccd6f083467");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAPList)
            {
                addMethod("MLAPList", Status.active, null);
                AppModules modules = db.AppModuless.Find("01146ebd-e9bb-4750-b072-113acf46d3d4");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAPList", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("01146ebd-e9bb-4750-b072-113acf46d3d4");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAHReturn)
            {
                addMethod("MLAHReturn", Status.active, null);
                AppModules modules = db.AppModuless.Find("d7a87385-861c-4365-bd75-6e711432b648");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAHReturn", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("d7a87385-861c-4365-bd75-6e711432b648");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (vmodel.MLAMRNote)
            {
                addMethod("MLAMRNote", Status.active, null);
                AppModules modules = db.AppModuless.Find("ccf4df09-4adc-4850-87a0-c94ed426770f");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAMRNote", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("ccf4df09-4adc-4850-87a0-c94ed426770f");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAProd)
            {
                addMethod("MLAProd", Status.active, null);
                AppModules modules = db.AppModuless.Find("20bf18fb-412e-4642-b1e3-a0706a65d80e");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAProd", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("20bf18fb-412e-4642-b1e3-a0706a65d80e");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.MLAUAssem)
            {
                addMethod("MLAUAssem", Status.active, null);
                AppModules modules = db.AppModuless.Find("6b07d885-c972-48e6-b575-0f27b6ce632a");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("MLAUAssem", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("6b07d885-c972-48e6-b575-0f27b6ce632a");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }


            if (vmodel.Payment)
            {
                addMethod("Payment", Status.active, null);
                AppModules modules = db.AppModuless.Find("6b07d885-c972-48e6-b575-0f27b6ce632a");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("Payment", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("6b07d885-c972-48e6-b575-0f27b6ce632a");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (vmodel.Reciept)
            {
                addMethod("Reciept", Status.active, null);
                AppModules modules = db.AppModuless.Find("6b07d885-c972-48e6-b575-0f27b6ce632a");
                if (modules != null)
                {
                    modules.Status = Status.active;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                addMethod("Reciept", Status.inactive, null);
                AppModules modules = db.AppModuless.Find("6b07d885-c972-48e6-b575-0f27b6ce632a");
                if (modules != null)
                {
                    modules.Status = Status.inactive;
                    db.Entry(modules).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }



            if (vmodel.Reminder)
            {
                addMethod("Reminder", Status.active, null);
            }
            else
            {
                addMethod("Reminder", Status.inactive, null);
            }
            if (vmodel.RemindTask)
            {
                addMethod("RemindTask", Status.active, null);
            }
            else
            {
                addMethod("RemindTask", Status.inactive, null);
            }
            if (vmodel.RemindSale)
            {
                addMethod("RemindSale", Status.active, null);
            }
            else
            {
                addMethod("RemindSale", Status.inactive, null);
            }
            if (vmodel.RemindQuot)
            {
                addMethod("RemindQuot", Status.active, null);
            }
            else
            {
                addMethod("RemindQuot", Status.inactive, null);
            }
            if (vmodel.RemindSReturn)
            {
                addMethod("RemindSReturn", Status.active, null);
            }
            else
            {
                addMethod("RemindSReturn", Status.inactive, null);
            }
            if (vmodel.RemindSOrder)
            {
                addMethod("RemindSOrder", Status.active, null);
            }
            else
            {
                addMethod("RemindSOrder", Status.inactive, null);
            }
            if (vmodel.RemindProForma)
            {
                addMethod("RemindProForma", Status.active, null);
            }
            else
            {
                addMethod("RemindProForma", Status.inactive, null);
            }
            if (vmodel.RemindPurchase)
            {
                addMethod("RemindPurchase", Status.active, null);
            }
            else
            {
                addMethod("RemindPurchase", Status.inactive, null);
            }
            if (vmodel.RemindPReturn)
            {
                addMethod("RemindPReturn", Status.active, null);
            }
            else
            {
                addMethod("RemindPReturn", Status.inactive, null);
            }
            if (vmodel.RemindPOrder)
            {
                addMethod("RemindPOrder", Status.active, null);
            }
            else
            {
                addMethod("RemindPOrder", Status.inactive, null);
            }
            if (vmodel.RemindPQuot)
            {
                addMethod("RemindPQuot", Status.active, null);
            }
            else
            {
                addMethod("RemindPQuot", Status.inactive, null);
            }
            if (vmodel.RemindDNote)
            {
                addMethod("RemindDNote", Status.active, null);
            }
            else
            {
                addMethod("RemindDNote", Status.inactive, null);
            }
            if (vmodel.RemindHReturn)
            {
                addMethod("RemindHReturn", Status.active, null);
            }
            else
            {
                addMethod("RemindHReturn", Status.inactive, null);
            }
            if (vmodel.RemindMReqn)
            {
                addMethod("RemindMReqn", Status.active, null);
            }
            else
            {
                addMethod("RemindMReqn", Status.inactive, null);
            }
            if (vmodel.RemindMRNote)
            {
                addMethod("RemindMRNote", Status.active, null);
            }
            else
            {
                addMethod("RemindMRNote", Status.inactive, null);
            }
            if (vmodel.RemindJobCard)
            {
                addMethod("RemindJobCard", Status.active, null);
            }
            else
            {
                addMethod("RemindJobCard", Status.inactive, null);
            }
            if (vmodel.RemindPackList)
            {
                addMethod("RemindPackList", Status.active, null);
            }
            else
            {
                addMethod("RemindPackList", Status.inactive, null);
            }


            if (vmodel.RemindStkTrans)
            {
                addMethod("RemindStkTrans", Status.active, null);
            }
            else
            {
                addMethod("RemindStkTrans", Status.inactive, null);
            }
            if (vmodel.RemindStkJnl)
            {
                addMethod("RemindStkJnl", Status.active, null);
            }
            else
            {
                addMethod("RemindStkJnl", Status.inactive, null);
            }
            if (vmodel.RemindProd)
            {
                addMethod("RemindProd", Status.active, null);
            }
            else
            {
                addMethod("RemindProd", Status.inactive, null);
            }
            if (vmodel.RemindUnass)
            {
                addMethod("RemindUnass", Status.active, null);
            }
            else
            {
                addMethod("RemindUnass", Status.inactive, null);
            }

            if (vmodel.AccInJournal)
            {
                addMethod("AccInJournal", Status.active, null);
            }
            else
            {
                addMethod("AccInJournal", Status.inactive, null);
            }

            if (vmodel.MakeInTrans)
            {
                addMethod("MakeInTrans", Status.active, null);
            }
            else
            {
                addMethod("MakeInTrans", Status.inactive, null);
            }
            if (vmodel.BatchWiseStock)
            {
                AppModules module = db.AppModuless.Find("e5c4a7a6-4845-4d54-a938-ae417ab64fda");
                module.Status = Status.active;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                addMethod("BatchWiseStock", Status.active, null);
            }
            else
            {
                AppModules module = db.AppModuless.Find("e5c4a7a6-4845-4d54-a938-ae417ab64fda");
                module.Status = Status.inactive;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                addMethod("BatchWiseStock", Status.inactive, null);
            }

            if (vmodel.PreventConversion)
            {
                addMethod("PreventConversion", Status.active, null);
            }
            else
            {
                addMethod("PreventConversion", Status.inactive, null);
            }

            if (vmodel.QuotToSale)
            {
                addMethod("QuotToSale", Status.active, null);
            }
            else
            {
                addMethod("QuotToSale", Status.inactive, null);
            }

            if (vmodel.QuotToPForma)
            {
                addMethod("QuotToPForma", Status.active, null);
            }
            else
            {
                addMethod("QuotToPForma", Status.inactive, null);
            }

            if (vmodel.QuotToDvNote)
            {
                addMethod("QuotToDvNote", Status.active, null);
            }
            else
            {
                addMethod("QuotToDvNote", Status.inactive, null);
            }

            if (vmodel.QuotToSOrder)
            {
                addMethod("QuotToSOrder", Status.active, null);
            }
            else
            {
                addMethod("QuotToSOrder", Status.inactive, null);
            }

            if (vmodel.PFToSale)
            {
                addMethod("PFToSale", Status.active, null);
            }
            else
            {
                addMethod("PFToSale", Status.inactive, null);
            }

            if (vmodel.PFToDvNote)
            {
                addMethod("PFToDvNote", Status.active, null);
            }
            else
            {
                addMethod("PFToDvNote", Status.inactive, null);
            }

            if (vmodel.DvNoteToSale)
            {
                addMethod("DvNoteToSale", Status.active, null);
            }
            else
            {
                addMethod("DvNoteToSale", Status.inactive, null);
            }

            if (vmodel.DvNoteToPF)
            {
                addMethod("DvNoteToPF", Status.active, null);
            }
            else
            {
                addMethod("DvNoteToPF", Status.inactive, null);
            }

            if (vmodel.POrderToPEntry)
            {
                addMethod("POrderToPEntry", Status.active, null);
            }
            else
            {
                addMethod("POrderToPEntry", Status.inactive, null);
            }


            if (vmodel.POrderToMRNote)
            {
                addMethod("POrderToMRNote", Status.active, null);
            }
            else
            {
                addMethod("POrderToMRNote", Status.inactive, null);
            }

            if (vmodel.SOrderToSale)
            {
                addMethod("SOrderToSale", Status.active, null);
            }
            else
            {
                addMethod("SOrderToSale", Status.inactive, null);
            }

            if (vmodel.SOrderToPF)
            {
                addMethod("SOrderToPF", Status.active, null);
            }
            else
            {
                addMethod("SOrderToPF", Status.inactive, null);
            }

            if (vmodel.SOrderToDvNote)
            {
                addMethod("SOrderToDvNote", Status.active, null);
            }
            else
            {
                addMethod("SOrderToDvNote", Status.inactive, null);
            }

            if (vmodel.PQuotToPOrder)
            {
                addMethod("PQuotToPOrder", Status.active, null);
            }
            else
            {
                addMethod("PQuotToPOrder", Status.inactive, null);
            }

            if (vmodel.MRNotetToPEntry)
            {
                addMethod("MRNotetToPEntry", Status.active, null);
            }
            else
            {
                addMethod("MRNotetToPEntry", Status.inactive, null);
            }

            if (vmodel.MRToPQuot)
            {
                addMethod("MRToPQuot", Status.active, null);
            }
            else
            {
                addMethod("MRToPQuot", Status.inactive, null);
            }

            if (vmodel.CustomizedDailySummary)
            {
                addMethod("CustomizedDailySummary", Status.active, null);
            }
            else
            {
                addMethod("CustomizedDailySummary", Status.inactive, null);
            }

            if (vmodel.LastTransInSales)
            {
                addMethod("LastTransInSales", Status.active, vmodel.LastTransSaleCount);
            }
            else
            {
                addMethod("LastTransInSales", Status.inactive, null);
            }
            if (vmodel.LastTransInPurchase)
            {
                addMethod("LastTransInPurchase", Status.active, vmodel.LastTransPurCount);
            }
            else
            {
                addMethod("LastTransInPurchase", Status.inactive, null);
            }
            if (vmodel.RepeatChequeNo)
            {
                addMethod("RepeatChequeNo", Status.active, null);
            }
            else
            {
                addMethod("RepeatChequeNo", Status.inactive, null);
            }
            if (vmodel.EnableCRM)
            {
                //lead
                AppModules module = db.AppModuless.Find("cae3a040-6530-4068-bbae-23cc7f198e08");
                module.Status = Status.active;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                //pipeline
                AppModules modules = db.AppModuless.Find("f40189a4-0e15-4123-8cb7-2f87e87aca2b");
                modules.Status = Status.active;
                db.Entry(modules).State = EntityState.Modified;
                db.SaveChanges();

                AppModules pipeR = db.AppModuless.Find("7d4f86a0-c77b-46d3-9311-dfa6f19cb101");
                pipeR.Status = Status.active;
                db.Entry(pipeR).State = EntityState.Modified;
                db.SaveChanges();

                AppModules leadR = db.AppModuless.Find("14248330-307e-4f74-bd70-26c8b7ecdb8b");
                leadR.Status = Status.active;
                db.Entry(leadR).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableCRM", Status.active, null);
            }
            else
            {
                AppModules module = db.AppModuless.Find("cae3a040-6530-4068-bbae-23cc7f198e08");
                module.Status = Status.inactive;
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();

                //pipeline
                AppModules modules = db.AppModuless.Find("f40189a4-0e15-4123-8cb7-2f87e87aca2b");
                modules.Status = Status.inactive;
                db.Entry(modules).State = EntityState.Modified;
                db.SaveChanges();

                AppModules pipeR = db.AppModuless.Find("7d4f86a0-c77b-46d3-9311-dfa6f19cb101");
                pipeR.Status = Status.inactive;
                db.Entry(pipeR).State = EntityState.Modified;
                db.SaveChanges();

                AppModules leadR = db.AppModuless.Find("14248330-307e-4f74-bd70-26c8b7ecdb8b");
                leadR.Status = Status.inactive;
                db.Entry(leadR).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("EnableCRM", Status.inactive, null);
            }

            if (vmodel.StockValue != null)
            {
                addMethod("StockValue", Status.active, vmodel.StockValue);
            }
            if (vmodel.InventoryMethod == "Average")
            {
                //stock moment
                AppModules module = db.AppModuless.Find("f43f5d8e-c474-4b61-a36c-447e212fc1f1");
                module.Link = "/Inventory/Moment";
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                //Stock Item Wise
                AppModules STItem = db.AppModuless.Find("fdb5ecb3-9696-4f52-8a60-856ba14f99ee");
                STItem.Link = "/Inventory/ItemWise";
                db.Entry(STItem).State = EntityState.Modified;
                db.SaveChanges();
                //Stock Brand Wise
                AppModules STBrand = db.AppModuless.Find("ee4bc6a8-f465-434d-b400-0c18d98a9865");
                STBrand.Link = "/Inventory/BrandWise";
                db.Entry(STBrand).State = EntityState.Modified;
                db.SaveChanges();
                //Stock Category Wise
                AppModules STCategory = db.AppModuless.Find("20971764-cd87-4cc0-95f7-4d0d77bb5def");
                STCategory.Link = "/Inventory/CategoryWise";
                db.Entry(STCategory).State = EntityState.Modified;
                db.SaveChanges();
                //Till Date Stock
                AppModules STTill = db.AppModuless.Find("78e3a065-b82c-4140-b32e-d8505fc3ac56");
                STTill.Link = "/Inventory/Index";
                db.Entry(STTill).State = EntityState.Modified;
                db.SaveChanges();
                //Stock As On Date
                AppModules STAsOn = db.AppModuless.Find("36367d08-efd9-4ba3-9ba3-ca52017e786c");
                STAsOn.Link = "/Inventory/OnDate";
                db.Entry(STAsOn).State = EntityState.Modified;
                db.SaveChanges();
                //Stock Between Dates
                AppModules STBwD = db.AppModuless.Find("af99585d-0775-40af-9eec-6ba5ca2fb7d9");
                STBwD.Link = "/Inventory/StockBwDate";
                db.Entry(STBwD).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("InventoryMethod", Status.active, vmodel.InventoryMethod);
            }
            else
            {
                //stock moment
                AppModules module = db.AppModuless.Find("f43f5d8e-c474-4b61-a36c-447e212fc1f1");
                module.Link = "/StockReport/Moment";
                db.Entry(module).State = EntityState.Modified;
                db.SaveChanges();
                //Stock Item Wise
                AppModules STItem = db.AppModuless.Find("fdb5ecb3-9696-4f52-8a60-856ba14f99ee");
                STItem.Link = "/StockReport/ItemWise";
                db.Entry(STItem).State = EntityState.Modified;
                db.SaveChanges();
                //Stock Brand Wise
                AppModules STBrand = db.AppModuless.Find("ee4bc6a8-f465-434d-b400-0c18d98a9865");
                STBrand.Link = "/StockReport/BrandWise";
                db.Entry(STBrand).State = EntityState.Modified;
                db.SaveChanges();
                //Stock Category Wise
                AppModules STCategory = db.AppModuless.Find("20971764-cd87-4cc0-95f7-4d0d77bb5def");
                STCategory.Link = "/StockReport/CategoryWise";
                db.Entry(STCategory).State = EntityState.Modified;
                db.SaveChanges();
                //Till Date Stock
                AppModules STTill = db.AppModuless.Find("78e3a065-b82c-4140-b32e-d8505fc3ac56");
                STTill.Link = "/StockReport/Index";
                db.Entry(STTill).State = EntityState.Modified;
                db.SaveChanges();
                //Stock As On Date
                AppModules STAsOn = db.AppModuless.Find("36367d08-efd9-4ba3-9ba3-ca52017e786c");
                STAsOn.Link = "/StockReport/OnDate";
                db.Entry(STAsOn).State = EntityState.Modified;
                db.SaveChanges();
                //Stock Between Dates
                AppModules STBwD = db.AppModuless.Find("af99585d-0775-40af-9eec-6ba5ca2fb7d9");
                STBwD.Link = "/StockReport/StockBwDate";
                db.Entry(STBwD).State = EntityState.Modified;
                db.SaveChanges();

                addMethod("InventoryMethod", Status.active, vmodel.InventoryMethod);
            }
            if (vmodel.Usedmaterials)
            {
                addMethod("Usedmaterials", Status.active, null);
            }
            else
            {
                addMethod("Usedmaterials", Status.inactive, null);
            }
            if (vmodel.Usedmaterials2)
            {
                addMethod("UsedmaterialsItemsInSE", Status.active, null);
            }
            else
            {
                addMethod("UsedmaterialsItemsInSE", Status.inactive, null);
            }
            if (vmodel.Employees)
            {
                addMethod("Employees", Status.active, null);
            }
            else
            {
                addMethod("Employees", Status.inactive, null);
            }
            if (vmodel.ItemBulkUpload)
            {
                addMethod("ItemBulkUpload", Status.active, null);
            }
            else
            {
                addMethod("ItemBulkUpload", Status.inactive, null);
            }


            //Employees settings
            if (vmodel.Employees)
            {
                AppModules emp = db.AppModuless.Find("ec5361d5-a878-44d6-833a-066f7651d856");
                emp.Status = Status.active;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();
                AppModules salesexec = db.AppModuless.Find("d5b77a0f-7804-42de-9f7b-f5b152ab94c8");
                salesexec.Status = Status.inactive;
                db.Entry(salesexec).State = EntityState.Modified;
                db.SaveChanges();



            }

            else
            {
                AppModules emp = db.AppModuless.Find("ec5361d5-a878-44d6-833a-066f7651d856");
                emp.Status = Status.inactive;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();
                AppModules salesexec = db.AppModuless.Find("d5b77a0f-7804-42de-9f7b-f5b152ab94c8");
                salesexec.Status = Status.active;
                db.Entry(salesexec).State = EntityState.Modified;
                db.SaveChanges();

            }
            if (vmodel.EnablePayroll)
            {
                addMethod("EnablePayroll", Status.active, null);
                AppModules emp = db.AppModuless.Find("41544263-bab7-4ca5-a00d-0ea30753decf");
                emp.Status = Status.active;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();

                AppModules sett = db.AppModuless.Find("da3bd2dd-5e19-44de-b966-c3f7a42028b1");
                sett.Status = Status.active;
                db.Entry(sett).State = EntityState.Modified;
                db.SaveChanges();

                AppModules payrep = db.AppModuless.Find("e5444744-83b8-4205-bd48-4d7147524669");
                payrep.Status = Status.active;
                db.Entry(payrep).State = EntityState.Modified;
                db.SaveChanges();


                var chkpayhead = db.Payheads.Where(a => a.Name == "Basic Salary").FirstOrDefault();
                if (chkpayhead == null)
                {
                    var UserId = User.Identity.GetUserId();
                    decimal OpnBalance = 0;
                    decimal OpnBalanceCr = 0;
                    var Acc = new Accounts
                    {
                        Name = "Basic Salary",
                        PrintName = "Basic Salary",
                        Alias = "Basic Salary",
                        PrevBalance = 0,
                        Status = Status.active,
                        Group = 30,
                        CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                        CreatedBy = UserId,
                        Editable = 0,
                        OpnBalanceCr = OpnBalanceCr,
                        OpnBalance = OpnBalance
                    };
                    db.Accountss.Add(Acc);
                    db.SaveChanges();
                    Int64 AccID = Acc.AccountsID;

                    var Phead = new Payhead
                    {
                        Name = "Basic Salary",
                        Accountgroup = 30,
                        AttendanceType = null,
                        CalculationPeriod = null,
                        CalculationType = null,
                        IncomeType = "Fixed",
                        Leave = null,
                        NameinSlip = null,
                        affectnetsalary = false,
                        Type = 0,
                        days = null,
                        CalculationBasis = null,
                        Compute = null,
                        Specifiedformula = null,
                        Account = AccID,
                    };
                    db.Payheads.Add(Phead);
                    db.SaveChanges();
                    Int64 ID = Phead.ID;
                    com.addlog(LogTypes.Created, UserId, "Payhead", "Payhead", findip(), ID, "Payhead Type Added Successfully");
                }
            }
            else
            {
                addMethod("EnablePayroll", Status.inactive, null);
                AppModules emp = db.AppModuless.Find("41544263-bab7-4ca5-a00d-0ea30753decf");
                emp.Status = Status.inactive;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();

                AppModules sett = db.AppModuless.Find("da3bd2dd-5e19-44de-b966-c3f7a42028b1");
                sett.Status = Status.inactive;
                db.Entry(sett).State = EntityState.Modified;
                db.SaveChanges();

                AppModules payrep = db.AppModuless.Find("e5444744-83b8-4205-bd48-4d7147524669");
                payrep.Status = Status.inactive;
                db.Entry(payrep).State = EntityState.Modified;
                db.SaveChanges();
            }

            if (vmodel.PayAttendance != null)
            {
                addMethod("PayAttendance", Status.active, vmodel.PayAttendance);
                if (vmodel.PayAttendance == "Daily Attendance")
                {
                    AppModules emp = db.AppModuless.Find("a3fbbb67-ac9b-42f2-b186-01f8519b1b0a");
                    emp.Status = Status.active;
                    db.Entry(emp).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    AppModules emp = db.AppModuless.Find("a3fbbb67-ac9b-42f2-b186-01f8519b1b0a");
                    emp.Status = Status.inactive;
                    db.Entry(emp).State = EntityState.Modified;
                    db.SaveChanges();
                }

                if (vmodel.PayAttendance == "Attendance Voucher")
                {
                    AppModules emp = db.AppModuless.Find("ff0645ee-4490-409b-80c2-c0a3fb852e6b");
                    emp.Status = Status.active;
                    db.Entry(emp).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    AppModules emp = db.AppModuless.Find("ff0645ee-4490-409b-80c2-c0a3fb852e6b");
                    emp.Status = Status.inactive;
                    db.Entry(emp).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            if (vmodel.Printlayout)
            {
                addMethod("Printlayout", Status.active, null);
            }
            else
            {
                addMethod("Printlayout", Status.inactive, null);
            }
            if (vmodel.TaxInclusive)
            {
                addMethod("TaxInclusive", Status.active, null);
            }
            else
            {
                addMethod("TaxInclusive", Status.inactive, null);
            }
            if (vmodel.CreditLimit)
            {
                addMethod("CreditLimit", Status.active, null);
            }
            else
            {
                addMethod("CreditLimit", Status.inactive, null);
            }

            //Default Material Centre             

            Success("Configuration Updated Successfully.", true);
            return RedirectToAction("Index", "EnableSetting");

        }

        private void addMethod(string type, Status stat, string value)
        {
            var payid = db.EnableSettings.Where(c => c.EnableType == type).Select(c => c.EnableSettingId).FirstOrDefault();
            if (payid > 0)
            {
                EnableSetting method = db.EnableSettings.Find(payid);
                method.EnableType = type;
                method.Status = stat;
                method.TypeValue = value;

                db.Entry(method).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                EnableSetting enset = new EnableSetting();
                enset.EnableType = type;
                enset.Status = stat;

                db.EnableSettings.Add(enset);
                db.SaveChanges();
            }
        }
        public JsonResult GetInvoice(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.InvoiceLayouts.Where(p => p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Name, //each json object will have 
                                      id = b.Id
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.InvoiceLayouts.Select(b => new SelectFormat
                {
                    text = b.Name, //each json object will have 
                    id = b.Id
                }).OrderBy(b => b.text).ToList();

            }

            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Invoice" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //    //Do stuff with int here


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
