using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class PaymentViewModel
    {
        [Display(Name = " Voucher No:")]
        public string VoucherNo { get; set; }

        public string InvoiceNo { get; set; }

        [Required]
        [Display(Name = "Date")]
        public string Date { get; set; }

        public ModeOfPayment MOPayment { get; set; }

        //public DateTime? PDCDate { get; set; }

        public string PDCDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Cheque/Voucher No")]
        public string CheckNo { get; set; }
        public string Bank { get; set; }
        public string pdcNote { get; set; }

        [Display(Name = "Pay From")]
        public long PayFrom { get; set; }

        [Display(Name = "Pay To")]
        public long PayTo { get; set; }
        // 2 categories 1. Regular 2. Taxable

        public string Category { get; set; }
        [Display(Name = "Sub Total")]
        public decimal SubTotal { get; set; }
        public decimal TaxPer { get; set; }
        public long? Tax { get; set; }
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }
        [Display(Name = "Grand Total")]
        public decimal GrandTotal { get; set; }

        public decimal Discount { get; set; }

        [Display(Name = "Amount")]
        public decimal Paying { get; set; }
        public decimal Balance { get; set; }

        public string Remark { get; set; }
        public string submittype { get; set; }
        public IEnumerable<Tax> Taxs { get; set; }

        // invoices id's
        public IEnumerable<long> bill { get; set; }


        public string debitor { get; set; }
        public string creditor { get; set; }

        public long Branch { get; set; }

        public ICollection<PaymentBill> invoicedata { get; set; }
        public decimal Totamt { get; set; }
        public string User { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }
        public string MOPay { get; set; }

        public string BusinessType { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        public string ApprovedBy { get; set; }
        public long? PaymentStatus { get; set; }
        public string Override { get; set; }        
        public string ComHeadCheck { get; set; }
        public List<PaymentBillViewModel> PayItem { get; set; }
        public List<PaymentBillViewModelnew> PayItemnew { get; set; }
        public string payfromname { get; set; }
        public string paytoname { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? pdcdat { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

        public long? Property { get; set; }
        public long? Unit { get; set; }
    }

    public class PaymentDocumentViewModel
    {
        public long DocumentID { get; set; }
        public long PaymentId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}