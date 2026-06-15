using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class PDC
    {
        public long PDCid { get; set; }
        public DateTime PDCDate { get; set; }
        // ex: reference for pdc ie: payment or reciept
        public string PDCType { get; set; }
        // corresponding pdc main table entry id ie; paymentid or receiptid
        public long Reference { get; set; }
        public string CheckNo { get; set; }

        [StringLength(50)]
        public string Bank { get; set; }

        public string Note { get; set; }
        public DateTime? PDCRegDate { get; set; }
        // affected 
        //public long AccountId { get; set; }
        //public virtual Accounts Accounts { get; set; }
        // is regularized or not
        public choice RegStatus { get; set; }
        //cleardate for BRS
        public DateTime? ClearDate { get; set; }
        // regularizing details{who when where}
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
        public string Bills { get; set; }
     
            public int? withhold { get; set; }
        public int? Type { get; set; } 
    }
}