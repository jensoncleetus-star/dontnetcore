using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class StockAdjustmentViewModel
    {
        [Display(Name = "Voucher No")]
        public string VoucherNo { get; set; }
        public long SANo { get; set; }

        //[Required]
        [Display(Name = "Item Name")]
        public long ItemID { get; set; }
        [Display(Name = "Transfer To ")]
        public long? ItemIDD { get; set; }
        [Display(Name = "Asset Name")]
        public long AssetName { get; set; }
        [Display(Name = "Item Quantity")]
        public decimal ItemQuantity { get; set; }
        public long? ItemUnitID { get; set; }
        [Display(Name = "Item To Quantity")]
        public decimal? ItemQuantityTo { get; set; }
        public long? ItemUnitIDD { get; set; }

        [Required]
        [Display(Name = "Date")]
        public string AdjDate { get; set; }

        [Display(Name = "Adjustment Type")]
        public AdjustmentType AdjustmentType { get; set; }

        public string Reason { get; set; }
        [Display(Name = "Purchase Rate")]
        public decimal PurchaseRate { get; set; }

        public long? MaterialCenter { get; set; }
        public ICollection<BatchStockPViewModel> bstmodel { get; set; }
    }

    //For Asset Adjustment
    public class AssetAdjustmentViewModel
    {
        [Required]
        [Display(Name = "Adjustment No.")]
        public long AdjustmentNo { get; set; }

        [Required]
        [Display(Name = "Date")]
        public string AdjustmentDate { get; set; }

        [Required]
        [Display(Name = "Asset Account")]
        public long AssetAccountId { get; set; }

        [Required]
        [Display(Name = "Asset Name")]
        public long AssetId { get; set; }        

        public long? AssetUnitId { get; set; }

        [Required]
        [Display(Name = "Adjustment Type")]
        public AdjustmentType AdjustmentType { get; set; }

        [Required]
        [Display(Name = "Asset Quantity")]
        public decimal AssetQuantity { get; set; }

        [Required]
        [Display(Name = "Asset Price")]
        public decimal PurchaseRate { get; set; }

        public string Reason { get; set; }       
       
    }
}