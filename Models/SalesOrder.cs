using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Models
{
    public class SalesOrder
    {
        public long SalesOrderId { get; set; }
        // seno defines bill number BillNo defines sale order prefix + QuotNo
        public long SONo { get; set; }
        public string BillNo { get; set; }

        public DateTime SODate { get; set; }

        // refer to table emploayee
        public long? SOCashier { get; set; }

        public long Customer { get; set; }

        // total items and total quantity
        public int SOItems { get; set; }
        public decimal SOItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SOSubTotal { get; set; }

        public decimal SOTax { get; set; }
        public decimal SOTaxAmount { get; set; }

        public decimal SODiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SOGrandTotal { get; set; }

        // extra note option
        public string SONote { get; set; }

        // mail times may use
        public int Mail { get; set; }

        public int? SOValidity { get; set; }

        
        public string TermsCondition { get; set; }
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime SOCreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }
        public string Remarks { get; set; }
        
        public SaleType SaleType { get; set; }
        public long SalesType { get; set; }
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
    }
    public class SalesOrderItem
    {
        public long SalesOrderItemId { get; set; }

        public long SalesOrder { get; set; }
        public virtual SalesOrder SalesOrderId { get; set; }
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
    }

    public class DummySOrderItem
    {
        public long DummySOrderItemId { get; set; }

        public long SalesOrder { get; set; }
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
    }
    public class salesorderdocument
    {
        [Key]
        public long sid { get; set; }
        public long salesorderidID { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DoucumentType { get; set; }
        public DateTime? Expiry { get; set; }
        public string Notes { get; set; }
    }
}
