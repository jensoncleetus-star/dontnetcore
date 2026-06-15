using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class PurchaseOrder
    {
        public long PurchaseOrderId { get; set; }
        // seno defines bill number BillNo defines saleprefix + QuotNo
        public long PONo { get; set; }
        public string BillNo { get; set; }

        public DateTime PODate { get; set; }
        public long? Currency { get; set; }
        public string ConvertionRate { get; set; }
        // refer to table emploayee
        public long? POCashier { get; set; }

        public long Supplier { get; set; }

        // total items and total quantity
        public int POItems { get; set; }
        public decimal POItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal POSubTotal { get; set; }

        public decimal POTax { get; set; }
        public decimal POTaxAmount { get; set; }

        public decimal PODiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal POGrandTotal { get; set; }

        // extra note option
        public string PONote { get; set; }

        // mail times may use
        public int Mail { get; set; }

        public int? POValidity { get; set; }

        
        public string TermsCondition { get; set; }
        //future use
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        public long PurchaseType { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime POCreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }
        public SupplierType SupplierType { get; set; }
        public string Remarks { get; set; }

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
        public int? PurchaseOrderStatus { get; set; }
    }
    public class PurchaseOrderItem
    {
        [Key]
        public long POItemId { get; set; }

        public long PurchaseOrder { get; set; }
        public virtual PurchaseOrder PurchaseOrderId { get; set; }
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

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }
        public long? Make { get; set; }

    }
    public class POBillSundry
    {
        public long POBillSundryId { get; set; }
        public long PurchaseOrder { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }
    public class DummyPOrderItem
    {
        [Key]
        public long DummyPOItemId { get; set; }

        public long PurchaseOrder { get; set; }
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

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }
        public long? Make { get; set; }

    }
}