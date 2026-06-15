using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class PurchaseEntry
    {
        public long PurchaseEntryId { get; set; }
        // seno defines bill number BillNo defines saleprefix + SENo
        public long PENo { get; set; }
        public string BillNo { get; set; }

        public DateTime PEDate { get; set; }

        public long Supplier { get; set; }

        // refer to table emploayee
        public long? PECashier { get; set; }

        // to definne payment type
        public string PayType { get; set; }

        // total items and total quantityy
        public int PEItems { get; set; }
        public int? PurchaseStatus  { get; set; }
        public decimal PEItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PESubTotal { get; set; }

        public decimal PETax { get; set; }
        public decimal PETaxAmount { get; set; }

        public decimal PEDiscount { get; set; }
        public bool requestpayment { get; set; }
        //[DataType(DataType.Currency)]
        public decimal PEGrandTotal { get; set; }

        // extra note option
        public string PENote { get; set; }

        // print times may use
        public int Print { get; set; }

        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime PECreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }

        public SupplierType SupplierType { get; set; }

        public long PurchaseType { get; set; }
        public string Remarks { get; set; }
        public long? MaterialCenter { get; set; }

        public long? Currency { get; set; }

        [StringLength(10)]
        public string ConvertionRate { get; set; }

        public decimal? FCTotal { get; set; }        

        public PurchaseEntry()
        {
            Print = 0;
            PurchaseType = 1;
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

        public PurchaseHireType PurType { get; set; }
        public long? PurchaseAccount { get; set; }
        public string ReferenceNo { get; set; }
       
    }
    // product items in purchase entry
    public class PEItems
    {
        public long PEItemsId { get; set; }

        public long PurchaseEntry { get; set; }
        public virtual PurchaseEntry PurchaseEntryId { get; set; }
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

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }
       // public long? BatchStockID { get; set; }
    }
    // purchase entry payment details
    public class PEPayment
    {
        public long PEPaymentId { get; set; }

        public long PurchaseEntry { get; set; }
        public virtual PurchaseEntry PEID { get; set; }
        public long SupplierId { get; set; }

        public DateTime PEDate { get; set; }
        public DateTime PEEntryDate { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PEBillAmount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PEPaidAmount { get; set; }

        public long CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public DateTime PECreatedDate { get; set; }

        public int Status { get; set; }

        public decimal? DebitAmount { get; set; }

    }

    // payment transaction details
    public class PETransaction
    {
        public long PETransactionId { get; set; }
        public long PurchaseEntry { get; set; }
        public virtual PurchaseEntry PEID { get; set; }
        public long SupplierId { get; set; }


        public DateTime PEPayDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal PEPayAmount { get; set; }

        // payment table reference
        public long PaymentId { get; set; }

        public DateTime PECreatedDate { get; set; }
        public long CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public int Status { get; set; }
        public string type { get; set; }

    }
    public class PEBillSundry
    {
        public long PEBillSundryId { get; set; }
        public long PurchaseEntry { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }

    public class PurchaseType
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public TaxType TaxType { get; set; }

        public choice editable { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public long Branch { get; set; }

        public Status Status { get; set; }
    }
    public class DummyPEItem2
    {
        [Key]
        public long DummyPEItemId { get; set; }
        public long PurchaseEntry { get; set; }
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

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }
    }
    public class DummyPEItem
    {
        public long DummyPEItemId { get; set; }
        public long PurchaseEntry { get; set; }
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

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }
    }
}