using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuickSoft.Models
{
    public class LeadRejection
    {
        [Key]
        public long RejectionId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Details")]
        public string Details { get; set; }
    }
}