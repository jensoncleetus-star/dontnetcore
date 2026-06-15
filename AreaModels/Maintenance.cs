using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Maintenance
    {

        [Key]
        public long ID { get; set; }

        public string VoucherNo { get; set; }
        public long PRNo { get; set; }

        public DateTime Date { get; set; }

        //public long Developer { get; set; }

        //public long Owner { get; set; }

        public long Property { get; set; }
        public long Contractor { get; set; }
        public decimal Amount { get; set; }

        public string Note { get; set; }

        public string Remark { get; set; }
        public string TermsCondition { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public long? PaymentType { get; set; }
        public long? ContractType { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
}