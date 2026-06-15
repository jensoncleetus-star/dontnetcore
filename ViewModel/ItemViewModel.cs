using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

using QuickSoft.Models;
using System;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class ItemViewModel
    {
        [Required]
        [Display(Name = "Item Code")]
        [StringLength(50)]
        public string ItemCode { get; set; }

        [Required]
        [Display(Name = "Item Name")]
        [StringLength(300)]
        public string ItemName { get; set; }

        [Display(Name = "Arabic Name")]
        [StringLength(300)]
        public string ItemArabic { get; set; }

        // [RegularExpression("^[0-9]*$", ErrorMessage = "Barcode Must be Numeric")]
        [StringLength(100)]
        public string Barcode { get; set; }

        
        [Display(Name = "Item Description")]
        public string ItemDescription { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Purchase Price")]
        public decimal PurchasePrice { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Base Price")]
        public decimal BasePrice { get; set; }

        
        [DataType(DataType.Currency)]
        public decimal MRP { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Cash Price")]
        public decimal? cashprice { get; set; }
        [DataType(DataType.Currency)]
        [Display(Name = "Cedit Price")]
        public decimal? creditprice { get; set; }

        [Display(Name = "Keep Stock")]
        public bool KeepStock { get; set; }
        [Display(Name = "Serial Number Required")]
        public bool slreq { get; set; }
        [Display(Name = "Account Map")]
        public bool accmap { get; set; }
        [Display(Name = "No Of Days Expiry")]
        public long? daysexpirty { get; set; }

        [Display(Name = "Account Map")]
        public long? accountid { get; set; }
        public string StockKeepName { get; set; }

        [Required(ErrorMessage = "Item Category is required")]
        public long ItemCategoryID { get; set; }

        [Required(ErrorMessage = "Item Brand is required")]
        public long ItemBrandID { get; set; }

     //   public long? ItemUnitID { get; set; }

        public long? ItemColorID { get; set; }

        public long? ItemSizeID { get; set; }
        public Status Status { get; set; }

        [Required(ErrorMessage = "Item Tax is required")]
        public long TaxID { get; set; }

        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemImage { get; set; }

        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemDocument { get; set; }
        public List<AdditionaldocViewModel> ItemDocumentlst { get; set; }

        public List<AdditionalImageViewModel> LstAdditionalImages { get; set; }

        public string ImageName { get; set; }
        public long? ImageId { get; set; }
        public long? ItmImageId { get; set; }

        public string DocName { get; set; }
        public long? DocId { get; set; }
        public long? ItmDocId { get; set; }

        public IEnumerable<ItemCategory> ItemCategorys { get; set; }
        public IEnumerable<ItemBrand> ItemBrands { get; set; }
        public IEnumerable<ItemColor> ItemColors { get; set; }
        public IEnumerable<ItemSize> ItemSizes { get; set; }
        public IEnumerable<ItemUnit> ItemUnits { get; set; }
        public IEnumerable<Tax> Taxs { get; set; }

        public string CategoryName { get; set; }        
        public string BrandName { get; set; }
        public string ItemSize { get; set; }
        public decimal? Tax { get; set; }
        public string ItemColor { get; set; }
        public string PUnit { get; set; }
        public string SUnit { get; set; }

        public long? Prefix { get; set; }
        public long[] suggestitem { get; set; }
        //give main unit id as ItemUnitID
        // [Required]
        [Display(Name = "Primary Unit")]
        public long? ItemUnitID { get; set; }

        public ItemUnit ItemUnit { get; set; }

        public long? SubUnitId { get; set; }
        public ItemUnit SubUnit { get; set; }

        [Display(Name = "Convertion Factor")]
        public decimal ConFactor { get; set; }
        
        public decimal? Commission { get; set; }

        public decimal? OpeningStock { get; set; }
        public decimal? MinStock { get; set; }

        [Display(Name = "Part Number")]
        [StringLength(20)]
        public string PartNumber { get; set; }

        public long Branch { get; set; }
        [Display(Name = "Item Type")]
        public int ItemType { get; set; }
        public long Currency { get; set; }
        public decimal? ConRate { get; set; }
        public string CurrencyName { get; set; }
        public string TaxName { get; set; }
        public string ItemTypeName { get; set; }

        public string BranchName { get; set; }
        public long ItemID { get; set; }
        //Watch                
        public long ItemJewelleryId { get; set; }//Ref from currency master
        [StringLength(20)]
        public string Refno { get; set; }
        public bool PromotionalItem { get; set; }
        [StringLength(20)]
        public string ModelNo { get; set; }
        [StringLength(20)]
        public string ModelName { get; set; }
        [StringLength(20)]
        public string Straptype { get; set; }
        // public string StrapColor { get; set; } color in item
        [StringLength(20)]
        public string DialShape { get; set; }
        [StringLength(20)]
        public string DialColor { get; set; }//ref from color table
        [StringLength(20)]
        public string Material { get; set; }
        [StringLength(20)]
        public string Movement { get; set; }
        public decimal? Weight { get; set; }
        public long? Supplier { get; set; }//Ref from supplier table
        [StringLength(30)]
        public string SupplierRef { get; set; }
        public bool pricingstatagy { get; set; }
        public bool ShowSecUnits { get; set; }
        public bool lockprice { get; set; }
        public pricingstatagytype pricingstatagytype { get; set; }
        public AmountType amounttype { get; set; }
        public decimal? amount { get; set; }
        public string SupplierName { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
        [StringLength(20)]
        public string StoneType { get; set; }
        [StringLength(50)]
        public string Country { get; set; }
        [StringLength(20)]
        public string Style { get; set; }
        [StringLength(20)]
        public string SetRef { get; set; }
        public int? Warranty { get; set; }
        public string Tagline1 { get; set; }
        public string Tagline2 { get; set; }
        public string Tagline3 { get; set; }
        public string Tagline4 { get; set; }
        public string Tagline5 { get; set; }


        //Diamond
        [StringLength(50)]
        public string Design { get; set; }
        [StringLength(20)]
        public string Fluorescence { get; set; }
        [StringLength(50)]
        public string Clarify { get; set; }
        public int? Range { get; set; }
        public string Time { get; set; }
        [StringLength(20)]
        public string CertificateNo { get; set; }
        public bool ComponentDetails { get; set; }

        [Display(Name = "Weight")]
        public string SCWeight { get; set; }
        public string CBM { get; set; }
        public string CreatedBy { get; set; }
        public List<HireType> HireType { get; set; }
        public List<HireTypeViewModel> HireTypes { get; set; }
        public ICollection<SalesEntryViewModel> Sales { get; set; }
        public List<PurchaseEntryViewModel> Purchase { get; set; }
        public List<HrStockViewModel> HrStock { get; set; }

        public decimal? StockValue { get; set; }
        public decimal OpeningCost { get; set; }

        public bool InSaleInvoice { get; set; }
        public ICollection<BatchStockPViewModel> bstmodel { get; set; }
        public ItemViewModel(){
            StockValue = 0;
        }
    }

    public class PriceUpdaterViewModel
    {
        public long ItemId { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal? cashprice { get; set; }
        public decimal? creditprice { get; set; }
            
        public decimal PurchasePrice { get; set; }
        public decimal BasePrice { get; set; }
        public decimal MRP { get; set; }
    }
    public class ItemCategoryViewModel
    {
        public long ItemCategoryID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Category")]
        public string ItemCategoryName { get; set; }

        public long? Parent { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class ItemBundleViewModel
    {
        [StringLength(300)]
        [Required]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; }

        [StringLength(50)]
        [Required]
        [Display(Name = "Item Code")]
        public string ItemCode { get; set; }

        [StringLength(100)]
        public string Barcode { get; set; }


        [Display(Name = "Actual Cost")]
        [Required]
        public decimal ActualCost { get; set; }


        [Display(Name = "Actual Price")]
        [Required]
        public decimal ActualPrice { get; set; }


        [Display(Name = "Selling Price")]
        [Required]
        public decimal SellingPrice { get; set; }

        [Display(Name = "Keep Stock")]
        public bool KeepStock { get; set; }

        [Display(Name = "Stock Quantity")]
        public decimal? StockQuantity { get; set; }

        [Display(Name = "Start Date")]
        public string StartDate { get; set; }
        [Display(Name = "End Date")]
        public string EndDate { get; set; }

        [Display(Name = "Item Category")]
        public long? ItemCategoryID { get; set; }

        [Display(Name = "Tax")]
        public long? TaxID { get; set; }

        public string Note { get; set; }

        [StringLength(12)]
        [Display(Name = "Bundle Type")]
        public string BundleType { get; set; }
        [Display(Name = "Bundle Image")]
        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ImgFileName { get; set; }
        public string FileNameImg { get; set; }

        public ItemBundle bundledata { get; set; }
        public ICollection<BundleItem> bundleitem { get; set; }

        public string CategoryName { get; set; }
        public string StockKeep { get; set; }
        public string TaxName { get; set; }
        public string CreatedBy { get; set; }
        public List<BundleItemViewModel> bundleitemvmodel { get; set; }

        public long? ItemBundleId { get; set; }

        public DateTime? SDate { get; set; }
        public DateTime? EDate { get; set; }
        public long? mainItem { get; set; }        

        public List<HireType> HireType { get; set; }
        public List<HireTypeViewModel> HireTypes { get; set; }

        public ItemBundleViewModel()
        {
            StockQuantity = 0;
        }
    }
    public class BundleItemViewModel
    {

        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }
    }
    public class HireTypeViewModel
    {
        public long type { get; set; }
        public string TypeName { get; set; }
        public decimal Rate { get; set; }
    }
    public class HrStockViewModel
    {
        public decimal? PurchaseRate { get; set; }
        public decimal? StockInshop { get; set; }
        public decimal? StockOutside { get; set; }
        public decimal? StockValue { get; set; }
        public decimal? StockOutsideValue { get; set; }
        public decimal? TotQty { get; set; }
        public decimal? TotValue { get; set; }
    }
    public class forpricesearch
    {   
        public long ItemID { get; set; }
        public string itemcode { get; set; }       
        public string itemname { get; set; }
       
        public string PriUnit { get; set; }
        public string SubUnit { get; set; }
        
        public decimal? OpeningStock { get; set; }
        public decimal? MinStock { get; set; }
        public decimal? ConFactor { get; set; }
   

        public decimal? PriPurchase { get; set; }
        public decimal? SubPurchase { get; set; }

       

    

     

        public decimal? pritotal { get; set; }
        public decimal? subtotal { get; set; }

        public decimal? total { get; set; }

        public decimal? stockIn { get; set; }
        public decimal? stockout { get; set; }
        public decimal? cost { get; set; }
        public decimal? price { get; set; }
        public decimal? stcost { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? mrp { get; set; }
        public string MC { get; set; }
        public List<StockPositionViewModel> stockpostionlist { get; set; }
        public decimal? CashPrice { get; set; }
        public decimal? CreditPrice { get; set; }
    }
    public class StockPositionViewModel
    {
        public string shelfname { get; set; }
        public string rackname { get; set; }
        public decimal? itemQuantity { get; set; }
        public long? rackmc { get; set; }

    }
    public class ListItem
    {
        public long ItemID { get; set; }
        public decimal? itemQuantity { get; set; }
        public long? ItemUnit { get; set; }
        public long? ItemUnitID { get; set; }
        public long? SubUnitId { get; set; }
        public string PriUnit { get; set; }
        public string SubUnit { get; set; }
        public decimal ConFactor { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemWithCode { get; set; }

    }
    public class ShelfTransferViewModel
    {
        public long shelftransferId { get; set; }
        public long rackmcId { get; set; }
        public long rackid { get; set; }
        public long shelfid { get; set; }
        public long mcid { get; set; }
        public string transactionType { get; set; }
        public string createdBy { get; set; }
        public DateTime createdDate { get; set; }
    }
    public class ShelfStockTransferViewModel
    {
        public long shelftransferId { get; set; }
        public string VoucherNo { get; set; }
        public long? rackmcId { get; set; }
        [Required(ErrorMessage = "the rack field is required")]
        public long? Torackid { get; set; }
        [Required(ErrorMessage = "the shelf field is required")]
        public long? Toshelfid { get; set; }
        [Required(ErrorMessage = "the MC field is required")]
        public long? Tomcid { get; set; }
        [Required(ErrorMessage = "the rack field is required")]
        public long? Fromrackid { get; set; }
        [Required(ErrorMessage = "the shelf field is required")]
        public long? Fromshelfid { get; set; }
        [Required(ErrorMessage = "the MC field is required")]
        public long? Frommcid { get; set; }
        public string transactionType { get; set; }
        public string createdBy { get; set; }
        public DateTime createdDate { get; set; }
    }
    public class ItemList3
    {
        public long DeliverynoteId { get; set; }
        public decimal ItemQuantity { get; set; }
        public string BillNo { get; set; }
        public string Customer { get; set; }
        public string ItemName { get; set; }

    }
    public class ItemList
    {
        public long ItemID { get; set; }
        public string Itemcode { get; set; }
        public string Itemname { get; set; }
        public string Item { get; set; }
        public string AssetAccountName { get; set; }
        public DateTime AssetEntryDate { get; set; }
        public string PriUnit { get; set; }
        public string SubUnit { get; set; }

        public decimal? ConFactor { get; set; }

        public long? ItemUnitID { get; set; }

        public long? SubUnitId { get; set; }

        public decimal? SellingPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? Price { get; set; }

        public string PartNumber { get; set; }

        public decimal? AvgPrice { get; set; }
        public long SaleType { get; set; }

        public decimal? PriSaleQty { get; set; }
        public decimal? PripurQty { get; set; }
        public decimal? SubSaleQty { get; set; }
        public decimal? SubpurQty { get; set; }

        public decimal? PriRetQty { get; set; }

        public decimal? SubRetQty { get; set; }

        public decimal? NetQty { get; set; }
        public decimal? SaleAmt { get; set; }
        public decimal? purAmt { get; set; }
        public decimal? PriSale { get; set; }
        public decimal? Pripur { get; set; }
        public decimal? SubSale { get; set; }
        public decimal? Subpur { get; set; }
        public decimal? RetunAmt { get; set; }

        public int? NoOfVchSale { get; set; }

        public int? NoOfVchReturn { get; set; }

    }
    public class ItemList2
    {
        public long Item { get; set; }
        public decimal? ItemQuantity { get; set; }
        public long? ItemUnit { get; set; }
        public decimal? ItemUnitPrice { get; set; }
        public decimal? ItemTax { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemDiscount { get; set; }
        public string note { get; set; }
        public string ItemNote { get; set; }
        public decimal? ItemTotalAmount { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemWithCode { get; set; }
        public long? ItemUnitID { get; set; }
        public long? SubUnitId { get; set; }
        public string PriUnit { get; set; }
        public string SubUnit { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? MRP { get; set; }

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }
        public string ProjectName { get; set; }
        public string TaskName { get; set; }
        public long? ItemMake { get; set; }
        public string ItemMakeName { get; set; }
    }
    public class BatchStockPViewModel
    {
        public string BatchNo { get; set; }
        public string MFG { get; set; }
        public string EXP { get; set; }
        public DateTime? MFGd { get; set; }
        public DateTime? EXPd { get; set; }
        public decimal StockIn { get; set; }
        public decimal StockOut { get; set; }
        public long Item { get; set; }
        public decimal cfactor { get; set; }
        public long? Priunit { get; set; }
        public long? Secunit { get; set; }
        public long? Unit { get; set; }
        public decimal Cost { get; set; }
        public long Order { get; set; }
        public string origin { get; set; }

        public BatchStockPViewModel()
        {
            StockIn = 0;
            StockOut = 0;
            cfactor = 1;
            Cost = 0;
            Order = 0;
            cfactor = 1;
            Item = 0;
        }
    }
    public class RackStockPViewModel
    {
        public long? RackNo { get; set; }
        public long? ShelfNo { get; set; }
        public string RackName { get; set; }
        public string ShelfName { get; set; }
        public decimal StockIn { get; set; }
        public decimal StockOut { get; set; }
        public long Item { get; set; }
        public decimal cfactor { get; set; }
        public long? Priunit { get; set; }
        public long? Secunit { get; set; }
        public long? Unit { get; set; }
        public decimal Cost { get; set; }
        public long Order { get; set; }
        public string origin { get; set; }

        public RackStockPViewModel()
        {
            
            StockOut = 0;
            cfactor = 1;
            Cost = 0;
            Order = 0;
            cfactor = 1;
            Item = 0;
        }
    }

    //For the usedmaterials batch stock

    public class UBatchStockPViewModel
    {
        public string BatchNo { get; set; }
        public string MFG { get; set; }
        public string EXP { get; set; }
        public DateTime? MFGd { get; set; }
        public DateTime? EXPd { get; set; }
        public decimal StockIn { get; set; }
        public decimal StockOut { get; set; }
        public long Item { get; set; }
        public decimal cfactor { get; set; }
        public long? Priunit { get; set; }
        public long? Secunit { get; set; }
        public long? Unit { get; set; }
        public decimal Cost { get; set; }
        public long Order { get; set; }
        public string origin { get; set; }

        public UBatchStockPViewModel()
        {
            StockIn = 0;
            StockOut = 0;
            cfactor = 1;
            Cost = 0;
            Order = 0;
            cfactor = 1;
            Item = 0;
        }
    }

    //end
    public class AdditionaldocViewModel
    {
        public string DocName { get; set; }
        public long? DocId { get; set; }
        public long? ItmDocId { get; set; }
        public string FileName { get; set; }
    }

    public class AdditionalImageViewModel
    {
        public long DocumentID { get; set; }
        public long ItemId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string FileNameSaved { get; set; }
    }

}