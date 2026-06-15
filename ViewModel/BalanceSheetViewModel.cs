using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class BalanceSheetViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public string showroom { get; set; }
        public ICollection<BalanceSheetLiability> BalanceSheetLiability { get; set; }
        public ICollection<BalanceSheetAsset> BalanceSheetAsset { get; set; }
        public ICollection<BalanceSheet> BalanceSheet { get; set; }
        public ICollection<BalanceSheetDisplay> BalanceSheetDisplay { get; set; }

        public ICollection<ProfitAndLossDisplay> ProfitAndLossDisplay { get; set; }
        public List<TrialBalanceDisplay> TrialBalanceDisplay { get; set; }

        public int? DebitCountOne { get; set; }
        public int? CreditCountOne { get; set; }

        public int? DebitCountTwo { get; set; }
        public int? CreditCountTwo { get; set; }
    }
    public class BalanceSheetLiability
    {
        public long? Account { get; set; }
        // public string type { get; set; }
        public string AccGroup { get; set; }
        public long AccGroupId { get; set; }
        public string Particulars { get; set; }
        public decimal? Amount { get; set; }

    }
    public class BalanceSheetAsset
    {
        public long? Account { get; set; }
        public string AccGroup { get; set; }
        public long AccGroupId { get; set; }
        //public string type { get; set; }
        public string Particulars { get; set; }
        public decimal? Amount { get; set; }

    }
    public class BalanceSheet
    {
        public long AccountsGroupID { get; set; }
        public string Particulars { get; set; }
        public long? Parent { get; set; }
        public string GpName { get; set; }
        public string AccType { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string ParentName { get; set; }
        public int orderB { get; set; }
        public int Temp { get; set; }
    }

    public class ItemData
    {
        public long ItemID { get; set; }
        public long? id { get; set; }
        public ICollection<Item> it { get; set; }
        public string ItemUnitName { get; set; }
        public string SubUnitName { get; set; }
        public string ItemCategoryName { get; set; }
        public string ItemBrandName { get; set; }
        public decimal? Percentage { get; set; }
        public long? TaxID { get; set; }
        public string TaxName { get; set; }
        public string TagLine1 { get; set; }
        public string TagLine2 { get; set; }
        public string TagLine3 { get; set; }
        public string TagLine4 { get; set; }
        public string TagLine5 { get; set; }
        public int? Editable { get; set; }
        public string ItemColorName { get; set; }
        public string ItemSizeName { get; set; }
        public string Description { get; set; }
        public string SEItemUnitPrice { get; set; }
        public long? SEItemUnit { get; set; }
        public string PEItemUnitPrice { get; set; }
        public long? PEItemUnit { get; set; }
        public decimal? PriPurchase { get; set; }
        public decimal? SubPurchase { get; set; }
        public decimal? Prisale { get; set; }
        public decimal? Subsale { get; set; }
        public decimal? PriPReturn { get; set; }
        public decimal? SubPReturn { get; set; }
        public decimal? PriSReturn { get; set; }
        public decimal? SubSReturn { get; set; }
        public decimal? PriStockAdjAdd { get; set; }
        public decimal? SubStockAdjAdd { get; set; }
        public decimal? PriStockAdjLess { get; set; }
        public decimal? SubStockAdjLess { get; set; }
        public decimal? PriStockAdj { get; set; }
        public decimal? SubStockAdj { get; set; }
        public decimal? PriProdItem { get; set; }
        public decimal? SubProdItem { get; set; }
        public decimal? PriUnas { get; set; }
        public decimal? SubUnas { get; set; }
        public decimal? PriUnasItem { get; set; }
        public decimal? SubUnasItem { get; set; }
        public string text { get; set; }
        public string ItemName { get; set; }
        public decimal? ConFactor { get; set; }
        public long? ItemUnitID { get; set; }
        public long? SubUnitId { get; set; }
        public bool KeepStock { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? MRP { get; set; }
        public decimal? MinStock { get; set; }
        public decimal? SEunitprice { get; set; }
        public decimal? PEunitprice { get; set; }
        public long? SEunit { get; set; }
        public long? PEunit { get; set; }
        public string ItemCode { get; set; }
        public string ItemArabic { get; set; }
        public string Barcode { get; set; }
        public decimal? OpeningStock { get; set; }
        public string SE { get; set; }
        public string PartNumber { get; set; }
    }
    public class BalanceSheetDisplay
    {
        public long? AccountGroupIDA { get; set; }
        public string ParticularA { get; set; }
        public decimal? DebitA { get; set; }
        public decimal? CreditA { get; set; }
        public long? ParentA { get; set; }

        public long? AccountGroupIDL { get; set; }
        public string ParticularL { get; set; }
        public decimal? DebitL { get; set; }
        public decimal? CreditL { get; set; }
        public long? ParentL { get; set; }

        public long? AccountsGroupIDA { get; set; }
        public long? AccountsGroupIDL { get; set; }
    }
    public class TrialBalanceDisplay
    {
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }

        public string Particular { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public Dictionary<string, object> opening { get; set; }
        
        public long? Parent { get; set; }
        public string AccType { get; set; }
        public long AccountsGroupID { get; set; }
        public string groupname { get; set; }
    }
    public class TrialBalanceAccmodal2
    {
        public long? ID { get; set; }
        public string text { get; set; }
        public long? Parent { get; set; }
        public string Type { get; set; }
        public string icon { get; set; }
        public long? AccountId { get; set; }
        public decimal? opening { get; set; }
        public decimal? closing { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public long? code { get; set; }
        public long? order { get; set; }
        public decimal? amount { get; set; }
        public long? Group { get; set; }

    }
    // account wise Trial Balance
    public class TrialBalanceAcc
    {
        public long? ID { get; set; }
        public string text { get; set; }
        public long? Parent { get; set; }
        public string Type { get; set; }
        public string icon { get; set; }
        public long? AccountId { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public long? code { get; set; }
        public long? order { get; set; }
        public decimal? amount { get; set; }
        public long? Group { get; set; }

    }
    public class TrialBalanceAccViewModel
    {
        public virtual ICollection<TrialBalanceAcc> Data { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? To { get; set; }
        public DateTime? From { get; set; }
        public virtual IOrderedEnumerable<TrialBalanceAcc> SimpleData { get; set; }
    }

    public class TrialBalanceAccViewModeltwo
    {
        public virtual ICollection<TrialBalanceAccmodal2> Data { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? To { get; set; }
        public DateTime? From { get; set; }
        public virtual IOrderedEnumerable<TrialBalanceAccmodal2> SimpleData { get; set; }
    }


    public class ProfitAndLoss
    {
        public long? Parent { get; set; }
        public string Particulars { get; set; }
        public string ParentName { get; set; }
        public decimal? Amount { get; set; }
        public int Orders { get; set; }
        public int Temp { get; set; }

        public long? AccountId { get; set; }

    }
    public class ProfitAndLossDisplay
    {
        public string ParticularA { get; set; }
        public decimal? DebitA { get; set; }
        public long? ParentA { get; set; }

        public string ParticularL { get; set; }
        public decimal? DebitL { get; set; }
        public long? ParentL { get; set; }

        public long? AccountIdDr { get; set; }
        public long? AccountIdCr { get; set; }

        public int Orders { get; set; }

    }
    public class purchaseussageFINAL
    {
        public decimal? daydiff { get; set; }
        public long ItemID { get; set; }
        public decimal? closingstock { get; set; }
        public decimal? openingstock { get; set; }
        public decimal? currentstock { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string purpose { get; set; }
        public decimal? purchaseprice { get; set; }
        public decimal? totalpurchase { get; set; }
        public decimal? totalpurchasereturn { get; set; }
        public decimal? totalsales { get; set; }
        public decimal? totalsalesreturn { get; set; }
        public decimal? totalstckin { get; set; }
        public decimal? totalstockout { get; set; }
    }
    public class purchaseussage
    {
      public  long itemid { get; set; }
       public decimal? stock { get; set; }
        public decimal? opstock { get; set; }
        public  string itemname { get; set; }
        public string purpose { get; set; }
        public decimal? puprice { get; set; }
        public decimal? purqty { get; set; }
        public decimal? purretqty { get; set; }
        public decimal? saleqty { get; set; }
        public decimal? saleretqty { get; set; }
        public decimal? stockint { get; set; }
     public decimal? stockout { get; set; }
    }
    public class StockDetails
    {

        public long IItemID { get; set; }
 
        public string IItemName { get; set; }
        public long? IUnitId { get; set; }
        public long? ISubUnitId { get; set; }
        public long? IItemUnitID { get; set; }
    
    public string purpose { get; set; }
        public decimal? ISellingPrice { get; set; }
        public decimal? IPurchasePrice { get; set; }
        public bool IKeepStock { get; set; }

        public decimal? IMinStock { get; set; }

        public string IItemCode { get; set; }
        public string IItemArabic { get; set; }
        public string IBarcode { get; set; }

        public decimal? IConFactor { get; set; }
        public decimal? IOpeningStock { get; set; }
        public decimal? IMRP { get; set; }

        public string IPartNumber { get; set; }
        public string IItemUnitName { get; set; }

        public string ISubUnitName { get; set; }
        public string IItemCategoryName { get; set; }
        public string IItemBrandName { get; set; }

        public decimal? IPercentage { get; set; }

        public long? ITaxID { get; set; }
        public string ITaxName { get; set; }

        public decimal? SEunitprice { get; set; }
        public decimal? PEunitprice { get; set; }

        public long? SEunit { get; set; }
        public long? PEunit { get; set; }

        public string Itext { get; set; }

        public bool? Ipartnum { get; set; }

        public decimal? ITotalCost { get; set; }
        public decimal? ITotalStockValue { get; set; }
        public decimal? ITotalQty { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? TotalStockValue { get; set; }

    }
    public class StockDetailssp
    {

        public long ItemID { get; set; }

        public string ItemName { get; set; }
        public long? UnitId { get; set; }
        public long? SubUnitId { get; set; }

        public decimal? SellingPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? cashprice { get; set; }
        public decimal? creditprice { get; set; }
        public decimal? price { get; set; }
        public decimal? cost { get; set; }
        public bool KeepStock { get; set; }
        public bool stockcheck { get; set; }
        public decimal? MinStock { get; set; }

        public string ItemCode { get; set; }
        public string ItemArabic { get; set; }
        public string Barcode { get; set; }
        public string ItemDescription { get; set; }

        public long? ItemImId { get; set; }
        public string FileName { get; set; }
        public decimal? ConFactor { get; set; }
        public decimal? OpeningStock { get; set; }
        public decimal? MRP { get; set; }

        public string PartNumber { get; set; }
        public string ItemUnitName { get; set; }

        public string SubUnitName { get; set; }

        public string PriUnit { get; set; }

        public string SubUnit { get; set; }
        public string ItemCategoryName { get; set; }
        public string ItemBrandName { get; set; }

        public decimal? Percentage { get; set; }

        public long? TaxID { get; set; }
        public string TaxName { get; set; }

        public decimal? SEunitprice { get; set; }
        public decimal? PEunitprice { get; set; }

        public long? SEunit { get; set; }
        public long? PEunit { get; set; }

        public string text { get; set; }

        public bool partnum { get; set; }

        public decimal? TotalCost { get; set; }
        public decimal? TotalStockValue { get; set; }
        public decimal? TotalQty { get; set; }
       public decimal? total { get; set; }
        public decimal lastSale { get; set; }
        public decimal lastPur { get; set; }
        public long id { get; set; }


    }
    public class StockDetailsmovement
    {

        public long IItemID { get; set; }
        public decimal stockin { get; set; }
        public decimal stockout { get; set; }
        public decimal netvalue { get; set; }
        public decimal onemonth { get; set; }
        public decimal onweek { get; set; }
        public decimal twomonth { get; set; }
        public decimal threemonth { get; set; }
        public decimal twelvemonth { get; set; }
        public string IItemName { get; set; }
        public long? IUnitId { get; set; }
        public long? ISubUnitId { get; set; }

        public decimal? ISellingPrice { get; set; }
        public decimal? IPurchasePrice { get; set; }
        public bool IKeepStock { get; set; }

        public decimal? IMinStock { get; set; }

        public string IItemCode { get; set; }
        public string IItemArabic { get; set; }
        public string IBarcode { get; set; }

        public decimal? IConFactor { get; set; }
        public decimal? IOpeningStock { get; set; }
        public decimal? IMRP { get; set; }

        public string IPartNumber { get; set; }
        public string IItemUnitName { get; set; }

        public string ISubUnitName { get; set; }
        public string IItemCategoryName { get; set; }
        public string IItemBrandName { get; set; }

        public decimal? IPercentage { get; set; }

        public long? ITaxID { get; set; }
        public string ITaxName { get; set; }

        public decimal? SEunitprice { get; set; }
        public decimal? PEunitprice { get; set; }

        public long? SEunit { get; set; }
        public long? PEunit { get; set; }

        public string Itext { get; set; }

        public bool Ipartnum { get; set; }
        public string suppliername { get; set; }
        public decimal? ITotalCost { get; set; }
        public decimal? ITotalStockValue { get; set; }
        public decimal? ITotalQty { get; set; }
        public decimal? currstock { get; set; }
        public double? datediff { get; set; }
        public double? datediffforcaste { get; set; }
        public decimal? invoicecount { get; set; }
        public decimal? currstockmcto { get; set; }
        public decimal? onemonthmcto { get; set; }
    }
    public class stockdetailsreturn
    {
       public decimal? totalpurchase { get; set; }
        public decimal? totalsales { get; set; }
        public decimal? totalpurchasereturn { get; set; }
        public decimal? totalsalesreturn { get; set; }
        public decimal? totalstckin { get; set; }
        public decimal? totalstockout { get; set; }

    }
    public class StockDataDetails
    {
        public long TItemId { get; set; }
        public long ItemId { get; set; }
        public DateTime TDate { get; set; }
        
        public string Invoice { get; set; }
        public string TItemType { get; set; }

        public decimal? IQty { get; set; }
        public decimal? ICost { get; set; }
        public decimal? ICostValue { get; set; }

        public decimal? OQty { get; set; }
        public decimal? OCost { get; set; }
        public decimal? OCostValue { get; set; }

        public decimal? BQty { get; set; }
        public decimal? BCost { get; set; }
        public decimal? BCostValue { get; set; }

        public decimal? Qty { get; set; }
        public decimal? UnitPrice { get; set; }
        public long invoiceid { get; set; }
        public decimal? confactor { get; set; }
    }

    public class StockDetailsmovement2
    {

        public long IItemID { get; set; }
        public decimal stockin { get; set; }
        public decimal stockout { get; set; }
        public decimal netvalue { get; set; }

        public string IItemName { get; set; }
        public long? IUnitId { get; set; }
        public long? ISubUnitId { get; set; }

        public decimal? ISellingPrice { get; set; }
        public decimal? IPurchasePrice { get; set; }
        public bool IKeepStock { get; set; }

        public decimal? IMinStock { get; set; }

        public string IItemCode { get; set; }
        public string IItemArabic { get; set; }
        public string IBarcode { get; set; }

        public decimal? IConFactor { get; set; }
        public decimal? IOpeningStock { get; set; }
        public decimal? IMRP { get; set; }

        public string IPartNumber { get; set; }
        public string IItemUnitName { get; set; }

        public string ISubUnitName { get; set; }
        public string IItemCategoryName { get; set; }
        public string IItemBrandName { get; set; }

        public decimal? IPercentage { get; set; }

        public long? ITaxID { get; set; }
        public string ITaxName { get; set; }

        public decimal? SEunitprice { get; set; }
        public decimal? PEunitprice { get; set; }

        public long? SEunit { get; set; }
        public long? PEunit { get; set; }

        public string Itext { get; set; }

        public bool Ipartnum { get; set; }

        public decimal? ITotalCost { get; set; }
        public decimal? ITotalStockValue { get; set; }
        public decimal? ITotalQty { get; set; }
        public decimal?[] currstock { get; set; }

    }
    public class StockDetails2
    {
        public long id { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? CurrentStock { get; set; }
        public long? ItemCategoryID { get; set; }
        public long? ItemBrandID { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? margin { get; set; }
        public long? days { get; set; }
        
    }
    public class StockDetails3
    {
        public long MCID { get; set; }
        public string MCName { get; set; }
        public decimal? stock { get; set; }
        public string ddmc { get; set; }
        public string srctxt { get; set; }
        public long? category { get; set; }
        public long? itemid { get; set; }
        public long? brandid { get; set; }
    }
    public class StockDetails4
    {
        public long id { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? CurrentStock { get; set; }
        public decimal? totalpruchase { get; set; }
        public decimal? confactor { get; set; }
        public long? ItemCategoryID { get; set; }
        public long? ItemBrandID { get; set; }
        public decimal? SellingPrice { get; set; }
        public long? days { get; set; }

    }
}