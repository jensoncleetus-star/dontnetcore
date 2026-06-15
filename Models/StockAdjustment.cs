using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class StockAdjustment
    {
        public long StockAdjustmentId { get; set; }
               
        public string VoucherNo { get; set; }
        public long SANo { get; set; }

        //[Required]
        public long ItemID { get; set; }
        public decimal ItemQuantity { get; set; }
        public long? ItemUnitID { get; set; }

        public DateTime AdjDate { get; set; }

        public AdjustmentType AdjustmentType { get; set; }

        public string Reason { get; set; }
        public decimal PurchaseRate { get; set; }

        public long? MaterialCenter { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public Status Status { get; set; }

        //public long? BatchStockID { get; set; }

    }

    public class AssetAdjustments
    {
        [Key]
        public long AssetAdjustmentId { get; set; }
        public long AdjustmentNo { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public long AssetAccountId { get; set; }
        public long AssetId { get; set; }       
        public long? AssetUnitId { get; set; }        
        public AdjustmentType AdjustmentType { get; set; }
        public decimal AssetQuantity { get; set; }        
        public decimal PurchaseRate { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Status Status { get; set; }
    }

}