using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;


namespace QuickSoft.ViewModel
{
    public class DeliverynoteViewModel
    {
        public long DvNo { get; set; }
        public string BillNo { get; set; }

        public DateTime DvDate { get; set; }

        // refer to table emploayee
        public long? DvCashier { get; set; }

        public long Customer { get; set; }
        public string CustomerName { get; set; }
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

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DvCreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }

        
        public string TermsCondition { get; set; }
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        public string custEmailId { get; set; }

        public string CreatedUserEmail { get; set; }

        public string EmployeeName { get; set; }

        public List<DvItemViewModel> DvItem { get; set; }
        [StringLength(150)]
        public string Location { get; set; }
        public CustomerType CustomerType { get; set; }
        public string PayType { get; set; }

        public int? CreditPeriod { get; set; }
        public string LPONo { get; set; }
        public string Remarks { get; set; }
        public long? MaterialCenter { get; set; }
        public string MCName { get; set; }

        public long ConTypeId { get; set; }
        public string ConType { get; set; }
        public SaleType SaleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? HireType { get; set; }

        public long SalesType { get; set; }
        public IEnumerable<SalesType> SalesTypes { get; set; }
        [Display(Name = "Payment Terms")]
        [StringLength(50)]
        public string PaymentTerms { get; set; }
        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }

        public string convertFrom { get; set; }
        public string convertBill { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public string SaleTypeName { get; set; }
        public string SalesTypeName { get; set; }
        public string EmailId { get; set; }

        public string HType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }

        public long PrintLayout { get; set; }
    }
    public class DvItemViewModel
    {
        public long DvItemId { get; set; }

        public long Dv { get; set; }
        public virtual Deliverynote DvEntryId { get; set; }
        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public string ItemNote { get; set; }
        public string PartNumber { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public decimal ItemDiscount { get; set; }
        public string ItemWithCode { get; set; }
        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
    public class MultiDvItemViewModel
    {
        public long Item { get; set; }
        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public long? ItemUnit { get; set; }
        public decimal ItemDiscount { get; set; }
        public string ItemWithCode { get; set; }
        public string note { get; set; }
        public long? ItemUnitID { get; set; }
        public long? SubUnitId { get; set; }
        public decimal BasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal MRP { get; set; }
        public string PriUnit { get; set; }
        public string SubUnit { get; set; }
        public long[] Dvnum { get; set; }
        public long? Customer { get; set; }
        public string Custname { get; set; }
        public string CustCode { get; set; }
        public string DVInvoice { get; set; }
    }
}