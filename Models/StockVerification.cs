using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class StockVerification
    {
        public long StockVerificationId { get; set; }
        public long VoNo { get; set; }
        [Required]
        public string Voucher { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime Date { get; set; }

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

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
        public StockVerification()
        {
            editable = choice.Yes;
        }
    }
    public class SVItems{
        public long SVItemsId { get; set; }
        public long StockVerification { get; set; }
        public long Item { get; set; }
        public long? ItemUnit { get; set; }

        public decimal CSPcs { get; set; }
        public decimal CSqty { get; set; }
      //----------------------------------
        public decimal PSPcs { get; set; }
        public decimal PSqty { get; set; }
      //----------------------------------
        public decimal SDPcs { get; set; }
        public decimal SDqty { get; set; }
    }
}