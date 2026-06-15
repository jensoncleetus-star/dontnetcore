using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class DrNote
    {
        public long DrNoteId { get; set; }
        // seno defines bill number BillNo defines company.invoiceprefix + SENo
        [Required]
        public string BillNo { get; set; }
        public long DNNo { get; set; }

        public long DebitType { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime DNDate { get; set; }
        // refer to table emploayee

        public long PayFrom { get; set; }
        public long PayTo { get; set; }

        // extra note option
        public string DNNote { get; set; }
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
        public DrNote()
        {
            Print = 0;
        }
    }
    public class DrInvoice
    {
        public long Id { get; set; }
        public long DrNoteId { get; set; }
        public long TransId { get; set; }
        public long EntryNo { get; set; }
        // type for identify sale or purchase
        [StringLength(20)]
        public string TransType { get; set; }
        public decimal? DebitAmount { get; set; }
    }
}