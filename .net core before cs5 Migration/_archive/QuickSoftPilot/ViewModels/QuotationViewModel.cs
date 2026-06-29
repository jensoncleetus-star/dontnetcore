using System.ComponentModel.DataAnnotations;

namespace QuickSoftPilot.ViewModels
{
    public class QuotationViewModel
    {
        public long QuotationId { get; set; }
        public string BillNo { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Customer")]
        public string CustomerName { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime QuotDate { get; set; } = DateTime.Today;

        [Display(Name = "Tax %")]
        [Range(0, 100)]
        public decimal TaxPercent { get; set; } = 5m;   // UAE VAT default

        [StringLength(500)]
        public string Remarks { get; set; }

        public List<QuotationItemViewModel> Items { get; set; } = new();
    }

    public class QuotationItemViewModel
    {
        [StringLength(150)]
        public string ItemName { get; set; }

        [Range(0, 1000000)]
        public decimal ItemQuantity { get; set; }

        [Range(0, 100000000)]
        public decimal ItemUnitPrice { get; set; }

        [StringLength(250)]
        public string ItemNote { get; set; }
    }

    // Read model for showing a real quotation from emirtechlatest.
    public class QuotationDetailsViewModel
    {
        public long QuotationId { get; set; }
        public string BillNo { get; set; }
        public DateTime QuotDate { get; set; }
        public string CustomerName { get; set; }
        public int QuotItems { get; set; }
        public decimal QuotItemQuantity { get; set; }
        public decimal QuotSubTotal { get; set; }
        public decimal QuotTaxAmount { get; set; }
        public decimal QuotGrandTotal { get; set; }
        public string QuotNote { get; set; }
        public List<QuotationDetailsLine> Lines { get; set; } = new();
    }

    public class QuotationDetailsLine
    {
        public string ItemName { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemUnitPrice { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public string ItemNote { get; set; }
    }
}
