using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    // Dedicated, ledger-independent rent-collection receipt for the Real-Estate module.
    // Records an actual payment against a rent invoice (Rental) so collection is receipt-driven
    // rather than guessed from invoice age. Intentionally isolated from the accounting Receipt
    // entity so seeding/recording here never touches the GL.
    public class PropertyRentReceipt
    {
        [Key]
        public long ID { get; set; }
        public string ReceiptNo { get; set; }
        public long RentalID { get; set; }   // the rent invoice paid (0 = ad-hoc)
        public long Tenant { get; set; }
        public long Property { get; set; }
        public long Unit { get; set; }
        public decimal Amount { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string Mode { get; set; }     // Cash / Cheque / Bank / Online
        public string ChequeNo { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Status Status { get; set; }   // active = collected, inactive = cancelled
    }
}
