using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PayrollVoucher
    {
        public long PayrollVoucherId { get; set; }

        public long PRNo { get; set; }

        public string VoucherNo { get; set; }

        public DateTime PRDate { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Note { get; set; }
        public string Details { get; set; }

        public decimal? GrandTotal { get; set; }
        public long Acccount { get; set; }


        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
    public class PayrollVoucherEmployee
    {
        [Key]
        public long PayrollEmployeeId { get; set; }
        public long PayrollVoucherId { get; set; }
        public long EmployeeId { get; set; }
        public long? EmpAccount { get; set; }
    }

    public class PayrollVoucherSalary
    {
        [Key]
        public long PayrollVoucherSalaryId { get; set; }
        public long PayrollEmployeeId { get; set; }
        public long PayrollVoucherId { get; set; }
        public long EmployeeId { get; set; }
        public long PayHeadId { get; set; }
        public decimal? Rate { get; set; }
        public string CrDr { get; set; }
    }
}