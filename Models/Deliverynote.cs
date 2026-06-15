using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Models
{
    public class Deliverynote
    {
        public long DeliverynoteId { get; set; }
        // seno defines bill number BillNo defines saleprefix + DvNo
        public long DvNo { get; set; }
        public string BillNo { get; set; }
        
        public DateTime DvDate { get; set; }

        // refer to table emploayee
        public long? DvCashier { get; set; }

        public long Customer { get; set; }

        // total items and total quantity
        public int DvItems { get; set; }
        public decimal DvItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal DvSubTotal { get; set; }

        public decimal DvTax { get; set; }
        public decimal DvTaxAmount { get; set; }

        public decimal DvDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal DvGrandTotal { get; set; }

        // extra note option
        public string DvNote { get; set; }

        // mail times may use
        public int Mail { get; set; }

        public int? DvValidity { get; set; }

        
        public string TermsCondition { get; set; }
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DvCreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }
        [StringLength(150)]
        public string Location { get; set; }
        public CustomerType CustomerType { get; set; }

        public string LPONo { get; set; }
        public string Remarks { get; set; }
        public long? MaterialCenter { get; set; }
        
        public SaleType SaleType { get; set; }
        public long SalesType { get; set; }

        [StringLength(50)]
        public string PaymentTerms { get; set; }
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

    public class DvItem
    {
        public long DvItemId { get; set; }

        public long Dv { get; set; }
        public virtual Deliverynote DvEntryId { get; set; }
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

    public class DummyDvItem
    {
        public long DummyDvItemId { get; set; }
        public long Dv { get; set; }
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
}