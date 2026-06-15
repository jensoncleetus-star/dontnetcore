using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;

namespace QuickSoft.ViewModel
{
    public class PropertyRegistrationViewModel
    {
        public long? RegistrationID { get; set; }

       
        [Display(Name = "Voucher No")]
        public string VoucherNo { get; set; }

        [Display(Name = "Registration Date")]
        public string RDate { get; set; }

        public long Developer { get; set; }

        public long Owner { get; set; }

        public long Property { get; set; }
        public string Broker { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }

        public string Remark { get; set; }
        public string TermsCondition { get; set; }

        public long Branch { get; set; }
        public List<AdditionalField> AdditionalField { get; set; }
        public List<AdditionalFieldVieModel> AdditionalFieldVieModels { get; set; }

        public DateTime? Date { get; set; }
        public string DeveloperName { get; set; }

        public string OwnerName { get; set; }
        public string PropertyName { get; set; }
        public string BrokerName { get; set; }

        public string PlotNumber { get; set; }
        public string PlotOption { get; set; }
        public decimal? PlotArea { get; set; }
        public string PAMeasurement { get; set; }
        public decimal? BuildupArea { get; set; }
        public string BAMeasurement { get; set; }
        public long? PaymentType { get; set; }
        public string Area { get; set; }

        public ICollection<ChequeViewModel> cheqmodel { get; set; }
        public ICollection<DocumentTypeViewModel> docmodel { get; set; }
        public string Hector { get; set; }
        public string ADDCNo { get; set; }
        public string PermitId { get; set; }
        public string PermissionId { get; set; }
        public string BookingDate { get; set; }
        public string HandoverDate { get; set; }
    }
}