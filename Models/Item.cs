using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class Item
    {
        public long ItemID { get; set; }

        [StringLength(50)]
        public string ItemCode { get; set; }

        [StringLength(300)]
        public string ItemName { get; set; }

        [StringLength(300)]
        public string ItemArabic { get; set; }

        
        public string ItemDescription { get; set; }

        [DataType(DataType.Currency)]
        public decimal SellingPrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal PurchasePrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal BasePrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal MRP { get; set; }
        [DataType(DataType.Currency)]
        public decimal? cashprice { get; set; }
        [DataType(DataType.Currency)]
        public decimal? creditprice { get; set; }

        public long? accountid { get; set; }
        public bool KeepStock { get; set; }
        public bool slreq { get; set; }
        public bool accmap { get; set; }
        public long? daysexpirty { get; set; }
        public long? ItemCategoryID { get; set; }

        public long? ItemBrandID { get; set; }



        public long? ItemColorID { get; set; }

        public long? ItemSizeID { get; set; }

        public long? TaxID { get; set; }

        public long CreatedBy { get; set; }

        public long Branch { get; set; }

        public Status Status { get; set; }

        public int ItemType { get; set; }

        public virtual ItemCategory ItemCategorys { get; set; }
        public virtual ItemBrand ItemBrands { get; set; }
        public virtual ItemColor ItemColors { get; set; }
        public virtual ItemSize ItemSizes { get; set; }


        //  public virtual ItemUnit ItemUnits { get; set; }

        public ApplicationUser CreatedUser { get; set; }
        public string CreatedUserID { get; set; }

        [StringLength(100)]
        public string Barcode { get; set; }

        //give main unit id as ItemUnitID
        public long? ItemUnitID { get; set; }
        //give main unit id as ItemUnitID
        //  public long MainUnitId { get; set; }
        public virtual ItemUnit ItemUnit { get; set; }
        public long? SubUnitId { get; set; }
        public virtual ItemUnit SubUnit { get; set; }
        public decimal ConFactor { get; set; }

        public decimal? OpeningStock { get; set; }
        public decimal? MinStock { get; set; }

        public decimal? Commission { get; set; }

        [StringLength(20)]
        public string PartNumber { get; set; }

        public long? Currency { get; set; }
        // currency conversion rate
        public decimal? ConRate { get; set; }
        public long? Supplier { get; set; }//vendor
        [StringLength(30)]
        public string SupplierRef { get; set; }//Vendor ref  
        public long? Prefix { get; set; }
        public decimal? StockValue { get; set; }
        public decimal OpeningCost { get; set; }

        public bool InSaleInvoice { get; set; }
        public bool PricingStrategy { get; set; }
        public bool lockprice { get; set; }
        public pricingstatagytype PricingStrategyType { get; set; }
        public AmountType PricingStrategyAmountType { get; set; }
        public decimal? PricingStrategyValue { get; set; }
        public Item()
        {
            ItemCategoryID = 0;
            ItemColorID = 0;
            ItemSizeID = 0;
            ItemUnitID = 0;
            ItemBrandID = 0;
            PricingStrategy = false;
        }
    }
    #region Jewellery
    public class Jewellery
    {
        public long Id { get; set; }

        public long Item { get; set; }

        [StringLength(50)]
        public string TagLine1 { get; set; }

        [StringLength(50)]
        public string TagLine2 { get; set; }

        [StringLength(50)]
        public string TagLine3 { get; set; }

        [StringLength(50)]
        public string TagLine4 { get; set; }

        [StringLength(50)]
        public string TagLine5 { get; set; }
        public bool PromotionalItem { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
        public string SetRef { get; set; }
        [StringLength(50)]
        public string Country { get; set; }
        [StringLength(20)]
        public string Style { get; set; }

    }
    public class Diamond
    {
        public long Id { get; set; }
        public long Item { get; set; }//ref
        [StringLength(50)]
        public string Design { get; set; }
        public bool ComponentDetails { get; set; }
        [StringLength(50)]
        public string Clarify { get; set; }
        [StringLength(20)]
        public string Fluorescence { get; set; }
        public int? Range { get; set; }
        [StringLength(20)]
        public string CertificateNo { get; set; }
        //Doubts        
        public string Time { get; set; }

    }
    public class Watch
    {
        [Key]
        public long Id { get; set; }
        public long Item { get; set; }//ref
        [StringLength(20)]
        public string Refno { get; set; }
        [StringLength(20)]
        public string ModelNo { get; set; }
        [StringLength(20)]
        public string ModelName { get; set; }
        public int? Warranty { get; set; }
        [StringLength(20)]
        public string Straptype { get; set; }
        // public string StrapColor { get; set; } color in item
        [StringLength(20)]
        public string DialShape { get; set; }
        public string DialColor { get; set; }//ref from color table
        [StringLength(20)]
        public string Material { get; set; }
        [StringLength(20)]
        public string Movement { get; set; }
        public decimal? Weight { get; set; }
        [StringLength(20)]
        public string StoneType { get; set; }

    }
    #endregion
    #region scaffold
    public class Scaffold
    {
        public long Id { get; set; }
        public long Item { get; set; }
        [StringLength(10)]
        public string Weight { get; set; }
        [StringLength(10)]
        public string CBM { get; set; }
    }
    #endregion
    public class batchstock
    {
        public DateTime? TDate { get; set; }
        public decimal? OQty { get; set; }
        public decimal? BQty { get; set; }
        public decimal? UnitPrice { get; set; }
        public string TItemType { get; set; }
        public decimal? currstock { get; set; }
        public decimal? confactor { get; set; }
        public long transactiondid { get; set; }
        public long itemid { get; set; }
        public string invoice { get; set; }
    }
    public class mergeitem
    {
        public long pritemid { get; set; }
        public long secitemid { get; set; }
    }
    public class ItemPrefix
    {
        public long Id { get; set; }
        public long Prefix { get; set; }
        public long No { get; set; }
    }
    public class mcitemminstocks
    {
        [Key]
        public long mcitemminstock { get; set; }
        public long MCId { get; set; }
        public long ItemId { get; set; }
        public decimal minstock { get; set; }


    }
    public class MultiPrice
    {
        public long id { get; set; }
        public string Name { get; set; }
        public long item { get; set; }
        [DataType(DataType.Currency)]
        public decimal SellingPrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal PurchasePrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal BasePrice { get; set; }

        [DataType(DataType.Currency)]
        public decimal MRP { get; set; }

    }

    public class Stock
    {
        public long StockId { get; set; }
        public long Item { get; set; }
        public long? Unit { get; set; }
        public decimal stockIn { get; set; }
        public decimal stockOut { get; set; }
        //Purchase Price
        public decimal Cost { get; set; }
        // quantity * Cost
        public decimal StockValue { get; set; }

        public string Purpose { get; set; }
        public long reference { get; set; }
        public long? MC { get; set; }
        public DateTime? Date { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Stock()
        {
            Status = Status.active;
        }
    }
    #region Item Bundle
    public class ItemBundle
    {
        public long ItemBundleId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public long mainItem { get; set; }
        public Item mainItemId { get; set; }


        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public Status Status { get; set; }

        public string BundleType { get; set; }
        public ItemBundle()
        {
            BundleType = "ComboItem";
        }
    }
    public class BundleItem
    {
        public long BundleItemId { get; set; }
        public long ItemBundle { get; set; }
        public ItemBundle ItemBundleId { get; set; }
        public long ItemId { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

    }
    #endregion
    public class ItemAddOn
    {
        public int ItemAddOnID { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        public long AddOnItem { get; set; }
        public virtual Item ItemId { get; set; }
        public long? Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }

        public long MainItem { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Status Status { get; set; }
    }

    public class ItemRemark
    {
        [Key]
        public long ItemRemarkId { get; set; }
        public long ItemId { get; set; }
        public string Remark { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}