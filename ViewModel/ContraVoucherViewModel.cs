using QuickSoft.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class ContraVoucherViewModel
    {
        [Display(Name = " Voucher No:")]
        [Required]
        public string VoucherNo { get; set; }
        [Required]
        [Display(Name = "Date")]
        public string Date { get; set; }

        [Display(Name = "Pay From")]
        public long PayFrom { get; set; }
        [Display(Name = "Pay To")]
        public long PayTo { get; set; }
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string submittype { get; set; }
        public long Branch { get; set; }

        public string debitor { get; set; }
        public string creditor { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

    }
}