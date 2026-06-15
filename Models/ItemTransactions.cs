using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class ItemTransactions
    {
        [Key]
        public long ItemTransId { get; set; }
        public long ItemId { get; set; }
        public long? McId { get; set; }
        public decimal TotalStock { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }

    }
}