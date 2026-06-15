using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class EnableSettingViewModel
    {
        public Boolean RackWiseStock { get; set; }
        public Boolean customerbonus { get; set; }
        public Boolean enablepricestratagy { get; set; }
        public Boolean HideItemName { get; set; }
        public Boolean SalesRateUpdateInPurchaseEntrySame { get; set; }
        public Boolean CreditLimit { get; set; }
        public Boolean BarcodeChecked { get; set; }
        public Boolean CustomerDetailsInInvoice { get; set; }
        public Boolean PartialMaterialConversion { get; set; }
        public Boolean AutosaveChecked { get; set; }
        public Boolean JobcardChecked { get; set; }
        public Boolean DvToSaleChecked { get; set; }
        public Boolean BOMChecked { get; set; }
        public Boolean POSInvoice { get; set; }
        public string POSLayout { get; set; }
        public Boolean ItemCommision { get; set; }
        public Boolean SaveAndMail { get; set; }
        public Boolean AutoCreateUser { get; set; }

        public Boolean BillToBillReceipt { get; set; }
        public Boolean BillToBillPayment { get; set; }
        public string PDCNotification { get; set; }
        public Boolean ItemPriceInPurchase { get; set; }
        public Boolean setsellingpricefixed { get; set; }
        public Boolean PredefinedCity { get; set; }
        public Boolean MCInTransaction { get; set; }
        public Boolean CNBillAdjust { get; set; }
        public Boolean DNBillAdjust { get; set; }
        public Boolean PartNoInItem { get; set; }
        public Boolean EnableBranch { get; set; }
        public Boolean EnableJewellery { get; set; }
        public Boolean EnableCurrency { get; set; }
        public Boolean EnablePrefixCode { get; set; }
        public string Invoices { get; set; }
        public Boolean EnableItemCodeInPrint { get; set; }
        public Boolean ItemOutOfStock { get; set; }
        public Boolean mcanddeliverystockeffect { get; set; }
        public Boolean Materialcentrewiseminimumstock { get; set; }
        public Boolean HideComHeaders { get; set; }

        public Boolean EnableItemBundle { get; set; }
        public Boolean StockTransferUpdate { get; set; }
        public Boolean AssignUserMC { get; set; }
        public string BusinessType { get; set; }
        public Boolean RemoveItemData { get; set; }
        public string SetTimeOut { get; set; }
        public Boolean EnablePurchaseInvoice { get; set; }
        public Boolean SuperUserEdit { get; set; }
        public Boolean SalesReturnInSales { get; set; }
        public Boolean AutomaticBillNoInSales { get; set; }
        public Boolean AutomaticVoucherNo { get; set; }
        public Boolean PreventOrderConvertion { get; set; }
        public Boolean AutomaticMailInTransactions { get; set; }
        public Boolean EnableVoucherEdit { get; set; }
        public Boolean Chequeprint { get; set; }
        public Boolean DiscountPercentage { get; set; }
        public Boolean MultiLevelApproval { get; set; }

        public Boolean MLAMc { get; set; }
        public Boolean MLAQuot { get; set; }
        public Boolean MLASEntry { get; set; }
        public Boolean MLASOrder { get; set; }
        public Boolean MLASReturn { get; set; }
        public Boolean MLAPQuot { get; set; }
        public Boolean MLAPEntry { get; set; }
        public Boolean MLAPOrder { get; set; }
        public Boolean MLAPReturn { get; set; }
        public Boolean MLADNote { get; set; }
        public Boolean MLAJCard { get; set; }
        public Boolean MLAPForma { get; set; }
        public Boolean MLASTran { get; set; }
        public Boolean MLASJour { get; set; }
        public Boolean MLAPList { get; set; }
        public Boolean MLAHReturn { get; set; }
        public Boolean MLAMRNote { get; set; }
        public Boolean MLAProd { get; set; }
        public Boolean MLAUAssem { get; set; }
        public Boolean Payment { get; set; }
        public Boolean Reciept { get; set; }


        public Boolean Reminder { get; set; }
        public Boolean RemindTask { get; set; }
        public Boolean RemindSale { get; set; }
        public Boolean RemindQuot { get; set; }
        public Boolean RemindSReturn { get; set; }
        public Boolean RemindSOrder { get; set; }
        public Boolean RemindProForma { get; set; }
        public Boolean RemindPurchase { get; set; }
        public Boolean RemindPReturn { get; set; }
        public Boolean RemindPOrder { get; set; }
        public Boolean RemindPQuot { get; set; }
        public Boolean RemindDNote { get; set; }
        public Boolean RemindHReturn { get; set; }
        public Boolean RemindMReqn { get; set; }
        public Boolean Bluedesign { get; set; }
        public Boolean Plaindesign { get; set; }
        public Boolean RemindMRNote { get; set; }
        public Boolean RemindJobCard { get; set; }
        public Boolean RemindPackList { get; set; }
        public Boolean RemindStkTrans { get; set; }
        public Boolean RemindStkJnl { get; set; }
        public Boolean RemindProd { get; set; }
        public Boolean RemindUnass { get; set; }


        public Boolean AccInJournal { get; set; }
        public Boolean MakeInTrans { get; set; }

        public Boolean BatchWiseStock { get; set; }

        public Boolean PreventConversion { get; set; }
        public Boolean QuotToSale { get; set; }
        public Boolean QuotToPForma { get; set; }
        public Boolean QuotToDvNote { get; set; }
        public Boolean QuotToSOrder { get; set; }
        public Boolean PFToSale { get; set; }
        public Boolean PFToDvNote { get; set; }
        public Boolean DvNoteToSale { get; set; }
        public Boolean DvNoteToPF { get; set; }
        public Boolean POrderToMRNote { get; set; }
        public Boolean POrderToPEntry { get; set; }
        public Boolean SOrderToSale { get; set; }
        public Boolean SOrderToPF { get; set; }
        public Boolean SOrderToDvNote { get; set; }
        public Boolean PQuotToPOrder { get; set; }
        public Boolean MRNotetToPEntry { get; set; }
        public Boolean MRToPQuot { get; set; }
        public Boolean LastTransInSales { get; set; }
        public string LastTransSaleCount { get; set; }

        public Boolean LastTransInPurchase { get; set; }
        public string LastTransPurCount { get; set; }

        public Boolean CustomizedDailySummary { get; set; }
        public Boolean RepeatChequeNo { get; set; }

        public Boolean EnableCRM { get; set; }
        public Boolean Usedmaterials2 { get; set; }
        public Boolean Usedmaterials { get; set; }
        public string InventoryMethod { get; set; }
        public string StockValue { get; set; }
        public Boolean Employees { get; set; }
        public Boolean ItemBulkUpload { get; set; }
        public Boolean EnablePayroll { get; set; }

        public Boolean Printlayout { get; set; }

        public string PayAttendance { get; set; }
        public Boolean TaxInclusive { get; set; }
        public Boolean stockcheckinvoice { get; set; }
        public List<string> Role;
        public string qtaskstatus { get; set; }
        public Boolean qtaskstatusenable { get; set; }

        public string MenuNavColor { get; set; }
        public string MenuhOverColor { get; set; }
        public decimal passwordchangedays { get; set; }

        public string UpdatedDateExpiry { get; set; }
        public string NextDateExpiry { get; set; }

    }
    public class McItemMinStocksViewModel
    {

        public long? mcitemminstock { get; set; }
        public long? MCId { get; set; }
        public long? ItemId { get; set; }
        public decimal? minstock { get; set; }
        // public long? Item { get; set; }

    }
}