using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class SalesReturn
    {
        public long SalesReturnId { get; set; }
        // seno defines bill number BillNo defines company.invoiceprefix + SENo

        public long SRNo { get; set; }
        [Required]
        public string BillNo { get; set; }

        //sales entryid=voucher No
        public long? SalesEntryId { get; set; }


        public ReturnType ReturnType { get; set; }


        [Display(Name = "Date")]
        [Required]
        public DateTime SRDate { get; set; }



        // refer to table emploayee
        public long? SRCashier { get; set; }

        // Sale type refers to POS Or Invoice
        public SaleType SaleType { get; set; }

        public long Customer { get; set; }

        //walking customer or default
        public CustomerType CustomerType { get; set; }

        // to definne payment type
        public string PayType { get; set; }

        // total items and total quantity
        public int SRItems { get; set; }
        public decimal  SRItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SRSubTotal { get; set; }

        public decimal SRTax { get; set; }


        public decimal SRTaxAmount { get; set; }

        public decimal SRDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SRGrandTotal { get; set; }

        // extra note option
        public string SRNote { get; set; }

        // print times may use
        public int Print { get; set; }

        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime SRCreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public long Branch { get; set; }

        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }
        public string Remarks { get; set; }

        public long? MaterialCenter { get; set; }

        public SalesReturn()
        {
            Print = 0;
            SalesType = 1;
            // PONo = 0;
        }

        public virtual ICollection<SRItems> SRItem { get; set; }

        public long? SalesType { get; set; }

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

        public long? SReturnAccount { get; set; }
    }
    // product items in sale entry
    public class SRItems
    {
        public long SRItemsId { get; set; }

        public long SalesReturnId { get; set; }
        public virtual SalesReturn SaleReturn { get; set; }

        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public decimal ItemDiscount { get; set; }

        public string itemNote { get; set; }
    }
    public class SRNoteItems
    {
        [Key]
        public long SRItemsId { get; set; }

        public long SalesReturnId { get; set; }
        public virtual SalesReturn SaleReturn { get; set; }

        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public decimal ItemDiscount { get; set; }

        public string itemNote { get; set; }
    }
    public class DummySRItem
    {
        public long DummySRItemId { get; set; }
        public long SalesReturnId { get; set; }
        public long Item { get; set; }
        public long? ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public decimal ItemDiscount { get; set; }

        public string itemNote { get; set; }
    }

    // sale entry payment details
    public class SRPayment
    {
        public long SRPaymentId { get; set; }
        public long SalesReturnId { get; set; }
        public virtual SalesReturn SRID { get; set; }
        public long CustomerId { get; set; }

        public DateTime SRDate { get; set; }


        public DateTime SREntryDate { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SRBillAmount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SReturnAmount { get; set; }

        public long CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public DateTime SRCreatedDate { get; set; }

        public int Status { get; set; }
    }

    // payment transaction details
    public class SRTransaction
    {
        public long SRTransactionId { get; set; }
        public long SalesReturnId { get; set; }
        public virtual SalesReturn SRID { get; set; }
        public long CustomerId { get; set; }


        // payment table reference
        public long PaymentId { get; set; }
        public DateTime SRPayDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal SRPayAmount { get; set; }

        public DateTime SRCreatedDate { get; set; }
        public long CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public int Status { get; set; }
    }
    public class SRBillSundry
    {
        public long SRBillSundryId { get; set; }
        public long SalesReturnId { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }
}