using System.ComponentModel.DataAnnotations;

namespace QuickSoftPilot.Models
{
    // Representative transaction HEADER, modelled on the legacy QuickSoft.Models.Quotation
    // (trimmed to the fields that prove the header + line-items pattern).
    public class Quotation
    {
        public long QuotationId { get; set; }
        public long QuotNo { get; set; }
        public string BillNo { get; set; }
        public DateTime QuotDate { get; set; }

        [Required, StringLength(150)]
        public string CustomerName { get; set; }

        public int QuotItems { get; set; }              // line-item count
        public decimal QuotItemQuantity { get; set; }   // total quantity
        public decimal QuotSubTotal { get; set; }
        public decimal QuotTaxPercent { get; set; }
        public decimal QuotTaxAmount { get; set; }
        public decimal QuotGrandTotal { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public DateTime QuotCreatedDate { get; set; }
        public string CreatedUserId { get; set; }

        // child collection — EF Core inserts these with the header in one transaction
        public List<QuotationItem> Items { get; set; } = new();
    }

    // LINE ITEM, modelled on the legacy QuotationItem.
    public class QuotationItem
    {
        public long QuotationItemId { get; set; }

        public long QuotationId { get; set; }       // FK to header
        public Quotation Quotation { get; set; }    // navigation back to header

        [Required, StringLength(150)]
        public string ItemName { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemUnitPrice { get; set; }
        public decimal ItemTotalAmount { get; set; }

        [StringLength(250)]
        public string ItemNote { get; set; }
    }
}
