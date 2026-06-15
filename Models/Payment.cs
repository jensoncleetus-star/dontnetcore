using System;
using System.ComponentModel.DataAnnotations;


namespace QuickSoft.Models
{
    public class Payment
    {
        public long PaymentId { get; set; }
        public long Voucher { get; set; }
        public string VoucherNo { get; set; }

        public string InvoiceNo { get; set; }
        public DateTime Date { get; set; }
        public ModeOfPayment MOPayment { get; set; }
        public DateTime? PDCDate { get; set; }
        public long PayFrom { get; set; }
        public long PayTo { get; set; }
        // 2 categories 1. Regular 2. Taxable
        public string Category { get; set; }
        public decimal SubTotal { get; set; }
        public long? Tax { get; set; }
        public decimal TaxPer { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Paying { get; set; }
        public decimal Balance { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
        public long? Reference { get; set; }
        // values ={Purchase, SaleReturn, Expense}
        public string RefType { get; set; }

        public  long? Project { get; set; }
        public  long? ProTask { get; set; }

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
        public long? PaymentStatus { get; set; }
        public string OverrideStatus { get; set; }

        public Payment()
        {
            editable = choice.Yes;
            Reference = 0;
            RefType = "Purchase";
        }
    }

    public class DummyPayment
    {
        [Key]
        public long PaymentId { get; set; }
        public long Voucher { get; set; }
        public string VoucherNo { get; set; }

        public string InvoiceNo { get; set; }
        public DateTime Date { get; set; }
        public ModeOfPayment MOPayment { get; set; }
        public DateTime? PDCDate { get; set; }
        public long PayFrom { get; set; }
        public long PayTo { get; set; }
        // 2 categories 1. Regular 2. Taxable
        public string Category { get; set; }
        public decimal SubTotal { get; set; }
        public long? Tax { get; set; }
        public decimal TaxPer { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Paying { get; set; }
        public decimal Balance { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
        public long? Reference { get; set; }
        // values ={Purchase, SaleReturn, Expense}
        public string RefType { get; set; }

        public long? Project { get; set; }
        public long? ProTask { get; set; }

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
        public long? PaymentStatus { get; set; }
        public string OverrideStatus { get; set; }
        public string CheckNo { get; set; }
        public string Bank { get; set; }
        public string PDCNote { get; set; }
        public Status stat { get; set; }
        public DummyPayment()
        {
            editable = choice.Yes;
            Reference = 0;
            RefType = "Purchase";
        }
    }

    public class DummyPayBill
    {
        [Key]
        public long PaymentBillId { get; set; }
        public long Payment { get; set; }
        public long? InvoiceNo { get; set; }
        [StringLength(20)]
        public string BillType { get; set; }
        public decimal Amount { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
        [StringLength(100)]
        public string NewRefName { get; set; }
        public Status Status { get; set; }
    }
    // Bill to bill for Payment
    public class BtoBPayment
    {
        public long Id { get; set; }
        public long PaymentId { get; set; }
        public long TransId { get; set; }
        // type for identify purchase or sale return
        [StringLength(20)]
        public string TransType { get; set; }
        public decimal? CreditAmount { get; set; }
    }

    public class PaymentBill
    {
        public long PaymentBillId { get; set; }
        public long Payment { get; set; }
        public long? InvoiceNo { get; set; }
        [StringLength(20)]
        public string BillType { get; set; }
        public decimal Amount { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
        [StringLength(100)]
        public string NewRefName { get; set; }
        public Status Status { get; set; }
    }
    public class JornalPaymentBill
    {
        [Key]
        public long PaymentBillId { get; set; }
        public long Jornal { get; set; }
        public long? InvoiceNo { get; set; }
        [StringLength(20)]
        public string BillType { get; set; }
        public decimal Amount { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
        [StringLength(100)]
        public string NewRefName { get; set; }
        public Status Status { get; set; }
    }

    public class PaymentBillViewModel
    {

        public long? InvoiceNo { get; set; }
        public decimal Amount { get; set; }
        public string NewRefName { get; set; }
    }
    public class PaymentBillViewModelnew
    {

        public string InvoiceNo { get; set; }
        public decimal Amount { get; set; }
        public string NewRefName { get; set; }
    }
    public class DummyPaymentBill
    {
        public long DummyPaymentBillId { get; set; }
        public long Payment { get; set; }
        public long? InvoiceNo { get; set; }
        [StringLength(20)]
        public string BillType { get; set; }
        public decimal Amount { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
        [StringLength(100)]
        public string NewRefName { get; set; }
        public Status Status { get; set; }
    }
}