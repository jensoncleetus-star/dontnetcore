using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuickSoft.Models
{
    public class LeadType
    {
       [Key]
        public long TypeId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Type")]
        public string Type { get; set; }
    }
}