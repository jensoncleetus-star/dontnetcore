using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PurchaseReturn
    {
        public long PurchaseReturnId { get; set; }

        [Required]
        public string BillNo { get; set; }

        public long PRNo { get; set; }
        //sales entryid=voucher No
        public long? purchaseEntryId { get; set; }

        public ReturnType ReturnType { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime PRDate { get; set; }

        public long Supplier { get; set; }

        // refer to table emploayee
        public long? PRCashier { get; set; }

        // to definne payment type
        public string PayType { get; set; }

        // total items and total quantity
        public int PRItems { get; set; }
        public decimal PRItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PRSubTotal { get; set; }

        public decimal PRTax { get; set; }
        public decimal PRTaxAmount { get; set; }

        public decimal PRDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PRGrandTotal { get; set; }

        // extra note option
        public string PRNote { get; set; }

        // print times may use
        public int Print { get; set; }

        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime PRCreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }

        public SupplierType SupplierType { get; set; }
        public string Remarks { get; set; }
        public long? MaterialCenter { get; set; }
        public PurchaseReturn()
        {
            Print = 0;
            PurchaseType = 1;
        }
        public virtual ICollection<PRItems> PRItem { get; set; }
        public long? PurchaseType { get; set; }

        public PurchaseHireType PurType { get; set; }

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

        public long? PReturnAccount { get; set; }
    }
    public class PRItems
    {
        public long PRItemsId { get; set; }

        public long PurchaseReturnId { get; set; }
      
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
    public class PRItemNotes
    {
        [Key]
        public long PRItemsId { get; set; }

        public long PurchaseReturnId { get; set; }

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
    public class PRPayment
    {
        public long PRPaymentId { get; set; }

        public long PurchaseReturnId { get; set; }
        public virtual PurchaseReturn PRID { get; set; }
        public long SupplierId { get; set; }

        public DateTime PRDate { get; set; }
        public DateTime PREntryDate { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PRBillAmount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PReturnAmount { get; set; }

        public long CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public DateTime PRCreatedDate { get; set; }

        public int Status { get; set; }
    }

    // payment transaction details
    public class PRTransaction
    {
        public long PRTransactionId { get; set; }
        public long PurchaseReturnId { get; set; }
        public virtual PurchaseReturn PRID { get; set; }
        public long SupplierId { get; set; }


        public DateTime PRPayDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal PRPayAmount { get; set; }
        // Reciept table reference
        public long Recieptid { get; set; }

        public DateTime PRCreatedDate { get; set; }
        public long CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public int Status { get; set; }
    }
    public class otpapprove
    {
        [Key]
        public long optid { get; set; }
        public long entryid { get; set; }
        public string purpose { get; set; }
        public string requestedby { get; set; }
        public string approvedby { get; set; }
        public string otp { get; set; }
        public DateTime expdate { get; set; }
    }
        public class SuperUser
        {
            [Key]
            public long superuserid { get; set; }

    public long employeeid { get; set; }
         

    public string purpose { get; set; }
 
     public long mcid { get; set; }
	public string emailid { get; set; }
        }
      
    
    public class PRBillSundry
    {
        public long PRBillSundryId { get; set; }
        public long PurchaseReturnId { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }

    public class DummyPRItem
    {
        public long DummyPRItemId { get; set; }

        public long PurchaseReturnId { get; set; }

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
}