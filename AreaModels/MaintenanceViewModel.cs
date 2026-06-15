using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;

namespace QuickSoft.ViewModel
{
    public class MaintenanceViewModel
    {

        public long? ID { get; set; }

        [Required]
        [Display(Name = "Voucher No")]
        public string VoucherNo { get; set; }

        [Display(Name = "Date")]
        public string Date { get; set; }

        public long Developer { get; set; }

        public long Owner { get; set; }

        public long Property { get; set; }
        public long Contractor { get; set; }
        public decimal ContractAmount { get; set; }

        public string Note { get; set; }

        public string Remark { get; set; }
        public string TermsCondition { get; set; }

        public long Branch { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }
        public long? PaymentType { get; set; }

        public List<AdditionalField> AdditionalField { get; set; }
        public List<AdditionalFieldVieModel> AdditionalFieldVieModels { get; set; }

        public ICollection<ChequeViewModel> cheqmodel { get; set; }

        public string PropertyName { get; set; }

        public string ContractorName { get; set; }
        public string Payment { get; set; }
        public DateTime? CreatedDate { get; set; }
        [Required]
        public long? ContractType { get; set; }

    }
}