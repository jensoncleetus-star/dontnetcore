using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;

namespace QuickSoft.ViewModel
{
    public class RentalViewModel
    {
        public long? RentalID { get; set; }

        [Required]
        [Display(Name = "Voucher No")]
        public string VoucherNo { get; set; }

        [Display(Name = "Rental Date")]
        public string RDate { get; set; }

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

        public string TenantName { get; set; }
        public string PropertyName { get; set; }
        public string UnitName { get; set; }
        public DateTime? Date { get; set; }
    }
}