using System;
using System.ComponentModel.DataAnnotations;


namespace QuickSoft.Models
{
    public class Receipt
    {
        public long ReceiptId { get; set; }
        public long Voucher { get; set; }
        public string VoucherNo { get; set; }
        public DateTime Date { get; set; }
        public ModeOfPayment MOPayment { get; set; }
        public DateTime? PDCDate { get; set; }
        public long PayFrom { get; set; }
        public long PayTo { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal? Discount { get; set; }
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
        // values ={Sales, PurchaseReturn}
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
        public long? ReceiptStatus { get; set; }
        public string OverrideStatus { get; set; }
        public Receipt()
        {
            editable = choice.Yes;
            Reference = 0;
            RefType = "Sales";
        }
    }
    // Bill to bill for Receipt
    public class BtoBReceipt
    {
        public long Id { get; set; }
        public long ReceiptId { get; set; }
        public long TransId { get; set; }
        // type for identify sale or purchase return
        [StringLength(20)]
        public string TransType { get; set; }
        public decimal? CreditAmount { get; set; }
    }

    public class ReceiptBill
    {
        public long ReceiptBillId { get; set; }
        public long Receipt { get; set; }
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

    public class ReceiptBillViewModel
    {
        public long? InvoiceNo { get; set; }
        public decimal Amount { get; set; }
        public string NewRefName { get; set; }
    }

    public class DummyReceiptBill
    {
        public long DummyReceiptBillId { get; set; }
        public long Receipt { get; set; }
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