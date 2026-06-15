using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class PriceCategory
    {
        [Key]
        public long pricestratagyid { get; set; }
        public string description { get; set; }
        public bool active { get; set; }
        public decimal value { get; set; }
        public long method { get; set; }
    }
}