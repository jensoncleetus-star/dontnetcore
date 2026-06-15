using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class CreditNote
    {
        public long CreditnoteId { get; set; }
        // seno defines bill number BillNo defines company.invoiceprefix + SENo
        [Required]
        public string BillNo { get; set; }
        public long CNNo { get; set; }

        public long CreditType { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime CNDate { get; set; }
        // refer to table emploayee

        public long PayFrom { get; set; }
        public long PayTo { get; set; }
        // extra note option
        public string CNNote { get; set; }
        public string Remarks { get; set; }

        // print times may use
        public int Print { get; set; }
        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }
        public long? MaterialCenter { get; set; }
      
        public decimal SubTotal { get; set; }
        public long? Tax { get; set; }
        public decimal? TaxPer { get; set; }
        public decimal TaxAmount { get; set; }
        //[DataType(DataType.Currency)]
        public decimal GrandTotal { get; set; }
        public CreditNote()
        {
            Print = 0;
        }
    }
    public class CNInvoice
    {
        public long CNInvoiceId { get; set; }
        public long CreditnoteId { get; set; }
        public long EntryNo { get; set; }
        // type for identify sale or purchase
        [StringLength(20)]
        public string TransType { get; set; }
        public decimal? CreditAmount { get; set; }
    }
}