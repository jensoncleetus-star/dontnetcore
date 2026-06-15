using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class JournalVViewModel
    {
        [Display(Name = " Voucher No:")]
        public string VoucherNo { get; set; }

        public string InvoiceNo { get; set; }
        [Required]
        [Display(Name = "Date")]
        public string Date { get; set; }

        [Display(Name = "Pay From (Credit)")]
        public long? PayFrom { get; set; }

        [Display(Name = "Pay To (Debit)")]
        public long? PayTo { get; set; }
        [Display(Name = "VAT Nature")]
        public int? VATNature { get; set; }

        //public decimal? SubTotal { get; set; }

        //[Display(Name = "Grand Total")]
        //public decimal GrandTotal { get; set; }

        //public decimal? Discount { get; set; }
        [Display(Name = "Amount")]
        public decimal? Paying { get; set; }
        //public decimal Balance { get; set; }

        public string Remark { get; set; }

        public string submittype { get; set; }
        public string debitor { get; set; }
        public string creditor { get; set; }
        public string UserName { get; set; }

        public long Branch { get; set; }
        public ICollection<JournalVItems> jnlitems { get; set; }
        public ICollection<ReceiptBill> invoicedataref { get; set; }
        public ICollection<ReceiptBill> invoicedataref2 { get; set; }
        public ICollection<PaymentBill> invoicedatapay { get; set; }
        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        [Display(Name = "MO Payment")]
        public ModeOfPayment MOPayment { get; set; }
        public string PDCDate { get; set; }
        [StringLength(50)]
        [Display(Name = "Cheque No")]
        public string CheckNo { get; set; }
        public string Bank { get; set; }
        public string pdcNote { get; set; }
        public string MOPay { get; set; }
        public string ComHeadCheck { get; set; }

        public List<JournalVItems> jounlitems { get; set; }

        public string payfromname { get; set; }
        public string paytoname { get; set; }
        public long? payfrom1 { get; set; }
        public long? payfrom2 { get; set; }
        public DateTime? jouDate { get; set; }
        public DateTime? pdcdat { get; set; }

        public decimal? SubTotal { get; set; }
        public decimal GrandTotal { get; set; }
    }
    public class JournalVItems
    {
        public int AccType { get; set; }
        public long AccountID { get; set; }

        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string Narration { get; set; }

        public string AccountName { get; set; }
        public string UserName { get; set; }

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }

        public string ProjectName { get; set; }
        public string TaskName { get; set; }

        public decimal? VATInput { get; set; }
    }
    public class JournalDocumentViewModel
    {
        public long DocumentID { get; set; }
        public long JournalId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}