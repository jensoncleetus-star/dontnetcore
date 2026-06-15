using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class AssetToInventoryViewModel
    {
        public long EntryId { get; set; }

        public long EntryNo { get; set; }

        public DateTime Date { get; set; }

        public long? McFromId { get; set; }

        [StringLength(50)]
        public string AssetAccount { get; set; }

        public long AssetAccountID { get; set; }

        public decimal TotalAmount { get; set; }

        public long? StockTransferId { get; set; }
    }
}