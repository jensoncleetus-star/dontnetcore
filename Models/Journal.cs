using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class Journal
    {
        public long JournalId { get; set; }
        public long Voucher { get; set; }
        public string VoucherNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime Date { get; set; }
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
        // values ={Journal}
        public string RefType { get; set; }
        public int? VATNature { get; set; }

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

        public ModeOfPayment MOPayment { get; set; }
        public DateTime? PDCDate { get; set; }

        public Journal()
        {
            editable = choice.Yes;
            Reference = 0;
            RefType = "Journal";
        }
    }
    public class DummyJornalBill
    {
        public long DummyJornalBillId { get; set; }
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
    public class JornalBill
    {
        public long JornalBillId { get; set; }
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
        public long? payfrom { get; set; } 
    }

    // Bill to bill for Receipt
    public class BtoBJournal
    {
        public long Id { get; set; }
        public long JournalId { get; set; }
        public long TransId { get; set; }
        // type for identify sale or purchase and returns
        [StringLength(20)]
        public string TransType { get; set; }
        public decimal? CreditAmount { get; set; }
    }
}