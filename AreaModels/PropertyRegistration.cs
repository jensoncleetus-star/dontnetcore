using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PropertyRegistration
    {
        [Key]
        public long RegistrationID { get; set; }

        public string VoucherNo { get; set; }
        public long PRNo { get; set; }

        public DateTime RDate { get; set; }

        public long Developer { get; set; }

        public long Owner { get; set; }

        public long Property { get; set; }
        public long? Broker { get; set; }
        public decimal Amount { get; set; }

        public string Note { get; set; }

        public string PlotNumber { get; set; }
        public string PlotOption { get; set; }
        public decimal? PlotArea { get; set; }
        public string PAMeasurement { get; set; }
        public decimal? BuildupArea { get; set; }
        public string BAMeasurement { get; set; }
        public long? PaymentType { get; set; }

        public string Remark { get; set; }
        public string TermsCondition { get; set; }


        public string Hector { get; set; }
        public string ADDCNo { get; set; }
        public string PermitId { get; set; }
        public string PermissionId { get; set; }
        public DateTime? BookingDate { get; set; }
        public DateTime? HandoverDate { get; set; }
        public string Area { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }

    }
}