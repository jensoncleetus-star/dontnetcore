using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class PriceCategoryPercentage
    {
        [Key]
        public long CategoryId { get; set; }
        public long Category { get; set; }

        public string PriceCategory { get; set; }

        public long Percentage { get; set; }

         
    }
}