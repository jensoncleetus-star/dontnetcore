using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class SalesEntry
    {
        public long SalesEntryId { get; set; }
        // seno defines bill number BillNo defines company.invoiceprefix + SENo

        public long SENo { get; set; }
        [Required]
        public string BillNo { get; set; }

        [Display(Name = "Date")]
        [Required]

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime SEDate { get; set; }

        //purchase order number
        public string PONo { get; set; }

        // refer to table emploayee
        public long? SECashier { get; set; }

        // Sale type refers to POS Or Invoice
        public SaleType SaleType { get; set; }

        public long Customer { get; set; }

        //walking customer or default
        public CustomerType CustomerType { get; set; }

        // to define payment type
        public string PayType { get; set; }

        // total items and total quantity
        public int SEItems { get; set; }
        public decimal SEItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SESubTotal { get; set; }

        public decimal SETax { get; set; }

        public int? SalesStatus { get; set; }
        public decimal SETaxAmount { get; set; }

        public decimal SEDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SEGrandTotal { get; set; }
        public decimal? materialcost { get; set; }
        // extra note option
        public string SENote { get; set; }

        // print times may use
        public int Print { get; set; }

       // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime SECreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public long Branch { get; set; }

        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }

        public long? OrderRefer { get; set; }

        // if payment method is null and customer type is cash its to be posted to cash account
        public long? PaymentMethod { get; set; }
        public long? PaymentAccount { get; set; }
        public long? MC { get; set; }

        //convert from quotation/dvnote etc..
        public string ConvertType { get; set; }
        public string ConvertNo { get; set; }

        [StringLength(150)]
        public string Location { get; set; }
        // POS-0,Sale-1,Hire-2,
        public long SalesType { get; set; }
        public string Remarks { get; set; }

        public long? MaterialCenter { get; set; }

        public long? Currency { get; set; }
        [StringLength(10)]
        public string ConvertionRate { get; set; }

        public decimal? FCTotal { get; set; }

        public long? Project { get; set; }
        public long? ProTask { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? DueDate { get; set; }
        public string DueReason { get; set; }
        [Display(Name = "Customer Name")]
        public string customername { get; set; }
        [Display(Name = "Phone Number")]
        public string phonenumber { get; set; }
        public SalesEntry()
        {
            Print = 0;
            SEDiscount = 0;
            SalesType = 1;
            // PONo = 0;
        }

        public virtual ICollection<SEItems> SEitem { get; set; }
        [StringLength(50)]
        public string HSCode { get; set; }
        [StringLength(50)]
        public string PaymentTerms { get; set; }

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

        public long? SaleAccount { get; set; }
        public long? pricecategoryid { get; set; }
    }
    public class salesmanprofittarget
    {
        [Key]
       public long targetprofitid { get; set; }
        public long employeeid { get; set; }
        public long salesentryid { get; set; }
        public int? completed { get; set; }
        public long? contributionpercentage { get; set; }
       

    }
    public class WorkCompletion
    {
        public long WorkCompletionId { get; set; }
        public string BillNo { get; set; }
        public long? WcCashier { get; set; }
        public long Customer { get; set; }
        public int WCItems { get; set; }
        public decimal WCItemQuantity { get; set; }
        public DateTime WCDate { get; set; }
        public decimal WCSubTotal { get; set; }
        public decimal WCTax { get; set; }
        public decimal WCTaxAmount { get; set; }
        public decimal WCDiscount { get; set; }
        public decimal WCGrandTotal { get; set; }
        public string WCNote { get; set; }

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

        public string InvoiceNo { get; set; }
    }
    public class WCBillSundry
    {
        public long WCBillSundryId { get; set; }
        public long WorkCompletion { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }
    public class WCItems
    {
        [Key]
        public long WCItemsId { get; set; }

        public long WorkCompletion { get; set; }
        public virtual WorkCompletion WCId { get; set; }

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
        public bool Type { get; set; }
    }
    public class WarrantyEntries
    {
        [Key]
        public long WarrantyId { get; set; }
        public string BillNo { get; set; }
        public long? WCashier { get; set; }
        public bool openclose { get; set; }
        public long Customer { get; set; }
        public int WItems { get; set; }
        public decimal WItemQuantity { get; set; }
        public DateTime WDate { get; set; }
        public decimal WSubTotal { get; set; }
        public decimal WTax { get; set; }
        public decimal WTaxAmount { get; set; }
        public decimal WDiscount { get; set; }
        public decimal WGrandTotal { get; set; }
        public string WNote { get; set; }
        public DateTime logtime { get; set; }
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

        public string InvoiceNo { get; set; }
    }
    public class WEItems
    {
        [Key]
        public long WItemsId { get; set; }

        public long Warranty { get; set; }
        public virtual WarrantyEntries WId { get; set; }

        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public long? ItemUnit { get; set; }
        public string WarrantyPeriod { get; set; }
        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public decimal ItemDiscount { get; set; }

        public string itemNote { get; set; }
        public bool Type { get; set; }
    }
    public class WarrantyCertificate
    {
        [Key]
        public long WarrantyId { get; set; }
        public string BillNo { get; set; }
        public long? WCashier { get; set; }
        public long Customer { get; set; }
        public int WItems { get; set; }
        public decimal WItemQuantity { get; set; }
        public DateTime WDate { get; set; }
        public decimal WSubTotal { get; set; }
        public decimal WTax { get; set; }
        public decimal WTaxAmount { get; set; }
        public decimal WDiscount { get; set; }
        public decimal WGrandTotal { get; set; }
        public string WNote { get; set; }

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

        public string InvoiceNo { get; set; }
    }
    public class WItems
    {
        [Key]
        public long WItemsId { get; set; }

        public long Warranty { get; set; }
        public virtual WarrantyCertificate WId { get; set; }

        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public long? ItemUnit { get; set; }
        public string WarrantyPeriod { get; set; }
        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public decimal ItemDiscount { get; set; }

        public string itemNote { get; set; }
        public bool Type { get; set; }
    }
    public class WBillSundry
    {
        public long WBillSundryId { get; set; }
        public long Warranty { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }
    // product items in sale entry
    public class SEItems
    {
        public long SEItemsId { get; set; }

        public long SalesEntry { get; set; }
        public virtual SalesEntry SaleEntryId { get; set; }

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
        public bool Type { get; set; }
      
    }

    public class DummySEItem
    {
        public long DummySEItemId { get; set; }

        public long SalesEntry { get; set; }

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
        // public long? BatchStockID { get; set; }
    }

    // sale entry payment details
    public class SEPayment
    {
        public long SEPaymentId { get; set; }
        public long SalesEntry { get; set; }
        public virtual SalesEntry SEID { get; set; }
        public long CustomerId { get; set; }

        public DateTime SEDate { get; set; }


        public DateTime SEEntryDate { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SEBillAmount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SEPaidAmount { get; set; }
        
        public long CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public DateTime SECreatedDate { get; set; }

        public int Status { get; set; }

        public decimal? CreditAmount { get; set; }

    }

    // payment transaction details
    public class SETransaction
    {
        public long SETransactionId { get; set; }
        public long SalesEntry { get; set; }
        public virtual SalesEntry SEID { get; set; }
        public long CustomerId { get; set; }


        public DateTime SEPayDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal SEPayAmount { get; set; }

        // reciept table reference
        public long Recieptid { get; set; }
        public DateTime SECreatedDate { get; set; }
        public long CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public int Status { get; set; }
        // define Receipt or credit note
        public string type { get; set; }
       
        //public SETransaction()
        //{
        //    type = "Receipt";
        //}
    }
    public class SEBillSundry
    {
        public long SEBillSundryId { get; set; }
        public long SalesEntry { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }


    public class PaymentMethod
    {
        public long PaymentMethodId { get; set; }
        // name for methods
        [Required]
        [Display(Name = "Method Name")]
        public string MethodName { get; set; }
        [Required]
        [Display(Name = "Bank Account")]
        public long? AccountId { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }

    }

    public class SalesType
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public TaxType TaxType { get; set; }

        public choice editable { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public Status Status { get; set; }
    }
    public class commission
    {
        [Key]
        public long commid { get; set; }
        public long agent { get; set; }
        public int commisiontype { get; set; }
        public int commisionmode { get; set; }
        public decimal comvalue { get; set; }
        public long salesid { get; set; }


    }
    public class PosData
    {
        public long PosDataId { get; set; }
        public long SalesEntry { get; set; }
        public virtual SalesEntry SEID { get; set; }
        
        public string PayMethod { get; set; }
        public string PayMode { get; set; }
        public string TotTender { get; set; }
        public string ChangeDue { get; set; }
        public long? Account { get; set; }
    }
    public class ChequeDetails
    {
        public long Id { get; set; }
        public long TransId { get; set; }      
        public string TransType { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string ChequeNo { get; set; }
    }
}