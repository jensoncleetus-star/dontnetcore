using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class ConvertTransactions
    {
        public long Id { get; set; }

        [StringLength(100)]
        [Required]
        public string ConvertFrom { get; set; }

        [StringLength(100)]
        [Required]
        public string ConvertTo { get; set; }

        public long From { get; set; }

        public long To { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public Status Status { get; set; }

        public long Branch { get; set; }
    }
}