using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class ContraVoucher
    {
        public long ContraVoucherId { get; set; }
        public long Voucher { get; set; }
        public string VoucherNo { get; set; }
        public DateTime Date { get; set; }
        public long PayFrom { get; set; }
        public long PayTo { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
        public ContraVoucher()
        {
            editable = choice.Yes;
        }

        //Refernce Field Added
        [StringLength(50)]
        public string Ref1 { get; set; }
        [StringLength(50)]
        public string Ref2 { get; set; }
        [StringLength(50)]
        public string Ref3 { get; set; }
        [StringLength(50)]
        public string Ref4 { get; set; }
        [StringLength(50)]
        public string Ref5 { get; set; }

    }
}