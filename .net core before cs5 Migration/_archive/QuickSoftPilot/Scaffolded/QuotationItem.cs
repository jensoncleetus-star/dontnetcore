using System;
using System.Collections.Generic;

namespace QuickSoftPilot.Scaffolded;

public partial class QuotationItem
{
    public long QuotationItemId { get; set; }

    public long Quotation { get; set; }

    public long Item { get; set; }

    public decimal ItemUnitPrice { get; set; }

    public decimal ItemQuantity { get; set; }

    public decimal ItemSubTotal { get; set; }

    public decimal ItemTax { get; set; }

    public decimal ItemTaxAmount { get; set; }

    public decimal ItemTotalAmount { get; set; }

    public string ItemNote { get; set; }

    public long? ItemIdItemId { get; set; }

    public long? QuotEntryIdQuotationId { get; set; }

    public decimal ItemDiscount { get; set; }

    public long? ItemUnit { get; set; }

    public virtual Item ItemIdItem { get; set; }

    public virtual Quotation QuotEntryIdQuotation { get; set; }
}
