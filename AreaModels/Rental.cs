using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Rental
    {
        [Key]
        public long RentalID { get; set; }
        public long PRNo { get; set; }
        public string VoucherNo { get; set; }

        public DateTime RDate { get; set; }

        public long Tenant { get; set; }

        public long Property { get; set; }

        public long Unit { get; set; }

        public decimal Amount { get; set; }

        public string Note { get; set; }

        public string Remark { get; set; }
        public string TermsCondition { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }

    }
}