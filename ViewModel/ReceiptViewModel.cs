using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class ReceiptViewModel
    {
        [Display(Name = " Voucher No:")]
        public string VoucherNo { get; set; }
        [Required]
        [Display(Name = "Date")]
        public string Date { get; set; }

        public ModeOfPayment MOPayment { get; set; }
        public string PDCDate { get; set; }

        //public DateTime? PDCDate { get; set; }
        [StringLength(50)]
        [Display(Name = "Cheque No")]
        public string CheckNo { get; set; }
        public string Bank { get; set; }
        public string pdcNote { get; set; }

        [Display(Name = "Pay From")]
        public long PayFrom { get; set; }

        [Display(Name = "Pay To")]
        public long PayTo { get; set; }
        [Display(Name = "Book Leaf No")]
        public long? leafno { get; set; }
        public decimal? SubTotal { get; set; }

        [Display(Name = "Grand Total")]
        public decimal GrandTotal { get; set; }
        [Display(Name = "Discount")]
        public decimal? Discount { get; set; }
        [Display(Name = "Amount")]
        public decimal Paying { get; set; }
        public decimal Balance { get; set; }

        public string Remark { get; set; }

        public string submittype { get; set; }


        public string debitor { get; set; }
        public string creditor { get; set; }
        public long Branch { get; set; }

        public ICollection<ReceiptBill> invoicedata { get; set; }
        public decimal Totamt { get; set; }
        public string User { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }

        public string MOPay { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        public string ApprovedBy { get; set; }
        public long? ReceiptStatus { get; set; }
        public string Override { get; set; }        
        public string ComHeadCheck { get; set; }


        public List<ReceiptBillViewModel> RecItem { get; set; }

        public string payfromname { get; set; }
        public string paytoname { get; set; }
        public DateTime? receiptDate { get; set; }
        public DateTime? pdcdat { get; set; }

        public long? Property { get; set; }
        public long? Unit { get; set; }

        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemImage { get; set; }

    }
    public class ReceiptDocumentViewModel
    {
        public long DocumentID { get; set; }
        public long ReceiptId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
