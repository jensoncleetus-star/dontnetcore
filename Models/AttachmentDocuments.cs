using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    // For File attachment
    public class AttachmentDocuments
    {
        [Key]
        public long DocumentID { get; set; }

        public long TransactionID { get; set; }

        public string TransactionType { get; set; }

        public string FileName { get; set; }

        public Status Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public string DocumentType { get; set; }

        public DateTime? Expiry { get; set; }

        public string Notes { get; set; }
    }
}