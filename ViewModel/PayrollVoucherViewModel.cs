using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace QuickSoft.ViewModel
{
    public class PayrollVoucherViewModel
    {
        public long? PayrollVoucherId { get; set; }

        public long PRNo { get; set; }
        [Required]
        [Display(Name = "Voucher No")]
        public string VoucherNo { get; set; }

        [Display(Name = "Date")]
        [Required]
        public string PRDate { get; set; }
        [Required]
        [Display(Name = "Pay Period From")]
        public string FromDate { get; set; }
        [Required]
        [Display(Name = "Pay Period To")]
        public string ToDate { get; set; }

        public string Note { get; set; }
        public string Details { get; set; }
        [Display(Name = "Grand Total")]
        public decimal? GrandTotal { get; set; }

        public long Acccount { get; set; }

        public long Branch { get; set; }


        public ICollection<SalaryStrPayrollViewModel> salarystr { get; set; }
        public ICollection<SalaryPayrollEmpViewModel> employee { get; set; }
    }
    public class SalaryStrPayrollViewModel
    {
        //public long PREmpId { get; set; }
        public long EmpId { get; set; }
        public long PayHeadId { get; set; }
        public decimal? Rate { get; set; }
        public string CrDr { get; set; }
    }
    public class SalaryPayrollEmpViewModel
    {
        public string CrDr { get; internal set; }
        public long Employee { get; set; }
    }
    public class PayrollVoucherAutoFillViewModel
    {
        public DateTime VFromDate { get; set; }
        public DateTime VToDate { get; set; }
        public long VAcccount { get; set; }

    }
}