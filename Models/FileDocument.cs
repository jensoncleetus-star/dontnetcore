using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class FileDocument
    {
        public long Id { get; set; }

        [Required]
        [StringLength(150)]
        public string DocumentName { get; set; }

        public DateTime? ExpiryDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public int? openclose { get; set; }
        public string Document { get; set; }
        public long Documenttype { get; set; }
        public long reminderrepeate { get; set; }
        public string Note { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public long Branch { get; set; }

        public Status Status { get; set; }
        public DateTime? LogTime { get; set; }
    }
}