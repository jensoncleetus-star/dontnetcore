using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuickSoft.Models
{
    public class Emirate
    {
        [Key]
        public long EmirateId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Emirate")]
        public string StateName { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; }
    }
}