using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class SVViewModel
    {
        public long StockVerificationId { get; set; }
        public long VoNo { get; set; }
        [Required]
        public string Voucher { get; set; }

        [Display(Name = "Date")]
        [Required]
        public string Date { get; set; }

        public DateTime? CheckDate { get; set; }
        public DateTime? CheckTime { get; set; }

        public string Batch { get; set; }

        public decimal totalPcs { get; set; }
        public decimal scannedPcs { get; set; }
        public decimal remainPcs { get; set; }


        public decimal totalWeight { get; set; }
        public decimal scannedWeight { get; set; }
        public decimal remainWeight { get; set; }

        // extra note option
        public string Note { get; set; }
        public string Remarks { get; set; }
        public ICollection<SVItemViewModel> svItemzz { get; set; }
        public long Branch { get; set; }

        public string action { get; set; }

    }
    public class SVItemViewModel
    {
        public long SVItemsId { get; set; }
        // public long? StockVerification { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public string ItemUnit { get; set; }
        public long ItemUnitId { get; set; }
        public long Item { get; set; }

        public decimal CSPcs { get; set; }
        public decimal CSqty { get; set; }
        //----------------------------------
        public decimal PSPcs { get; set; }
        public decimal PSqty { get; set; }
        //----------------------------------
        public decimal SDPcs { get; set; }
        public decimal SDqty { get; set; }
    }

    public class RemainStkViewModel
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public string ItemCode{ get; set; }
        public decimal? RemainQty { get; set; }
    }
}