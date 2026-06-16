using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    // Audit trail of expiry reminder emails sent by PropertyReminderService.
    // One row per (item, expiry) so the same contract/document is never emailed twice
    // for the same expiry window.
    public class PropertyReminderLog
    {
        [Key]
        public long ID { get; set; }
        public string Kind { get; set; }        // "ContractExpiry" | "DocExpiry"
        public long RefID { get; set; }          // TenancyContract.Id or PropertyDocumentType.ID
        public string Title { get; set; }        // human label (tenant / property / doc)
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime SentDate { get; set; }
        public string Result { get; set; }       // "Sent" | "No email" | "Failed: ..."
    }
}
