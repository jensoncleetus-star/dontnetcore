using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class PurchaseQuotation
    {
        [Key]
        public long PQuotationId { get; set; }
        // seno defines bill number BillNo defines saleprefix + QuotNo
        public long PQuotNo { get; set; }
        public string BillNo { get; set; }

        public DateTime PQuotDate { get; set; }

        // refer to table employee
        public long? PQuotCashier { get; set; }

        public long Supplier { get; set; }

        // total items and total quantity
        public int PQuotItems { get; set; }
        public decimal PQuotItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PQuotSubTotal { get; set; }

        public decimal PQuotTax { get; set; }
        public decimal PQuotTaxAmount { get; set; }

        public decimal PQuotDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PQuotGrandTotal { get; set; }

        // extra note option
        public string PQuotNote { get; set; }

        // mail times may use
        public int Mail { get; set; }

        public int? PQuotValidity { get; set; }

        public long? Project { get; set; }
        public long? ProTask { get; set; }

        
        public string TermsCondition { get; set; }
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime PQuotCreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }
        public string Remarks { get; set; }

        public long PurchaseType { get; set; }
        public IEnumerable<PurchaseType> PurchaseTypes { get; set; }

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

    }
    public class PurchaseQuotationItem
    {
        public long PurchaseQuotationItemId { get; set; }

        public long PQuotation { get; set; }
        public virtual PurchaseQuotation PQuotEntryId { get; set; }
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

        public string ItemNote { get; set; }
        public long? Make { get; set; }

    }
    public class DummyPQuotationItem
    {
        public long DummyPQuotationItemId { get; set; }

        public long PQuotation { get; set; }
        public long Item { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public decimal ItemDiscount { get; set; }

        public string ItemNote { get; set; }
        public long? Make { get; set; }

    }
    public class PQtBillSundry
    {
        public long PQtBillSundryId { get; set; }
        public long PQuotation { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }
}