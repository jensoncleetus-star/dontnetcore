using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class BillSundry
    {
        public long BillSundryId { get; set; }
        public string BSName { get; set; }

        public BSType BSType { get; set; }
        public long? BSNature { get; set; }

        public AmountType AmountType { get; set; }
        public decimal? DefaultValue { get; set; }
        public choice editable { get; set; }

        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Status Status { get; set; }
        [Range(1,long.MaxValue, ErrorMessage ="PLease Select Sales Account")]
        public long SAccount { get; set; }
        [Range(1, long.MaxValue, ErrorMessage = "PLease Select Purchase Account")]
        public long PAccount { get; set; }

        public BillSundry()
        {
            editable = choice.Yes;
            BSNature = 0;
        }
    }
    public class BSNature
    {
        public long BSNatureId { get; set; }
        public string BSNatureName { get; set; }
    }
}