using System;
using System.Collections.Generic;

namespace QuickSoftPilot.Scaffolded;

public partial class Item
{
    public long ItemId { get; set; }

    public string ItemCode { get; set; }

    public string ItemName { get; set; }

    public string ItemDescription { get; set; }

    public decimal SellingPrice { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal BasePrice { get; set; }

    public decimal Mrp { get; set; }

    public bool KeepStock { get; set; }

    public long? ItemCategoryId { get; set; }

    public long? ItemBrandId { get; set; }

    public long? ItemColorId { get; set; }

    public long? ItemSizeId { get; set; }

    public long? TaxId { get; set; }

    public long CreatedBy { get; set; }

    public int Status { get; set; }

    public int ItemType { get; set; }

    public string CreatedUserId { get; set; }

    public string Barcode { get; set; }

    public long? ItemUnitId { get; set; }

    public long? SubUnitId { get; set; }

    public decimal ConFactor { get; set; }

    public decimal? OpeningStock { get; set; }

    public decimal? MinStock { get; set; }

    public long? ItemUnitItemUnitId { get; set; }

    public long? SubUnitItemUnitId { get; set; }

    public string ItemArabic { get; set; }

    public decimal? Commission { get; set; }

    public string PartNumber { get; set; }

    public long Branch { get; set; }

    public long? Currency { get; set; }

    public decimal? ConRate { get; set; }

    public long? Supplier { get; set; }

    public string SupplierRef { get; set; }

    public long? Prefix { get; set; }

    public decimal? StockValue { get; set; }

    public decimal OpeningCost { get; set; }

    public bool InSaleInvoice { get; set; }

    public bool Slreq { get; set; }

    public long? Daysexpirty { get; set; }

    public bool PricingStrategy { get; set; }

    public int PricingStrategyType { get; set; }

    public int PricingStrategyAmountType { get; set; }

    public decimal? PricingStrategyValue { get; set; }

    public bool Lockprice { get; set; }

    public decimal? Cashprice { get; set; }

    public decimal? Creditprice { get; set; }

    public bool Accmap { get; set; }

    public long? Accountid { get; set; }

    public virtual ItemCategory ItemCategory { get; set; }

    public virtual ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
}
