using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class ProRentalViewModel
    {
        public long? ID { get; set; }

        [Required]
        [Display(Name = "Voucher No")]
        public string VoucherNo { get; set; }

        [Display(Name = "Date")]
        public string Date { get; set; }

        public long Tenant { get; set; }


        public long Property { get; set; }
        public long Unit { get; set; }
        public decimal Amount { get; set; }

        public string Note { get; set; }

        public string Remark { get; set; }
        public string TermsCondition { get; set; }

        public long Branch { get; set; }

        public List<AdditionalField> AdditionalField { get; set; }
        public List<AdditionalFieldVieModel> AdditionalFieldVieModels { get; set; }
    }
}