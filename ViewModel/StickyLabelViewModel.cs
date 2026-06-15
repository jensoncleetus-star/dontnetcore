using QuickSoft.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class StickyLabelViewModel
    {
        public long LabelId { get; set; }

        [Required]
        public string LabelName { get; set; }

        [Required]
        public string LabelColor { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Status Status { get; set; }

        public long Branch { get; set; }
    }
}