using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PriceCategoryMaster
    {
       [Key]

        public long CategoryId { get; set; }
        public string Category { get; set; }
    }
}