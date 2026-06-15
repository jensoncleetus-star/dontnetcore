using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Models
{
    public class Quotation
    {
        public long QuotationId { get; set; }
        // seno defines bill number BillNo defines saleprefix + QuotNo
        public long QuotNo { get; set; }
        public string BillNo { get; set; }
        public long? sourceoflead { get; set; }
        public DateTime QuotDate { get; set; }
        public long? Currency { get; set; }
        [StringLength(10)]
        public string ConvertionRate { get; set; }
        // refer to table emploayee
        public long? QuotCashier { get; set; }

        public long Customer { get; set; }

        // total items and total quantity
        public int QuotItems { get; set; }
        public decimal QuotItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal QuotSubTotal { get; set; }

        public decimal QuotTax { get; set; }
        public decimal QuotTaxAmount { get; set; }

        public decimal QuotDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal QuotGrandTotal { get; set; }

        // extra note option
        public string QuotNote { get; set; }

        // mail times may use
        public int Mail { get; set; }

        public int? QuotValidity { get; set; }

        public long? Project { get; set; }
        public long? ProTask { get; set; }

        
        public string TermsCondition { get; set; }
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime QuotCreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }
        public string Remarks { get; set; }

        public SaleType SaleType { get; set; }

        public long SalesType { get; set; }
        [StringLength(50)]
        public string PaymentTerms { get; set; }

        public long? leadsid { get; set; }
        public DateTime? expdate { get; set; }
        public int? quotationstatus { get; set; }


        public string revision { get; set; }

        public long? quotationtype { get; set; }
        public decimal? FCTotal { get; set; }
        public long? servicetype { get; set; }
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

    public class QuotationItem
    {
        public long QuotationItemId { get; set; }

        public long Quotation { get; set; }
        public virtual Quotation QuotEntryId { get; set; }
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
    public class QtBillSundry
    {
        public long QtBillSundryId { get; set; }
        public long Quotation { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }

    public class EsBillSundries
    {
        [Key]
        public long EsBillSundryId { get; set; }
        public long Estimate { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }

    public class DummyQuotItem
    {
        public long DummyQuotItemId { get; set; }
        public long Quotation { get; set; }
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
    public class quotationdocument
    {
        [Key]
        public long qutid { get; set; }
        public long quotationID { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DoucumentType { get; set; }
        public DateTime? Expiry { get; set; }
        public string Notes { get; set; }
    }

    public class geowall
    {
        [Key]
        public long geowallId { get; set; }
        [Display(Name = "latitude")]
        public string lat { get; set; }
        [Display(Name = "logitude")]
        public string log { get; set; }
        public int? distance { get; set; }



        [Display(Name = "Employee")]
        public long EmployeeId { get; set; }

    }
    public class accountmap
    {
        [Key]
        public long AccountMapId { get; set; }
        public int? level { get; set; }
        [Display(Name = "Account")]
        public long AccountId { get; set; }
        public EmployeePaymentType PaymentTypeId { get; set; }

        [Display(Name = "Employee")]
        public long EmployeeId { get; set; }
        [Display(Name = "Description")]
        public string description { get; set; }
        [Display(Name = "Show in Tax Exempt Invoice")]
        public bool notintaxinvoice { get; set; }

    }
    public class accountmapviewmodal
    {
        [Key]
        public long AccountMapId { get; set; }
        public int? level { get; set; }
        [Display(Name = "Account")]
        public long AccountId { get; set; }
        public EmployeePaymentType PaymentTypeId { get; set; }

        [Display(Name = "Employee")]
        public long[] EmployeeId { get; set; }
        [Display(Name = "Description")]
        public string description { get; set; }
        [Display(Name = "Show in Tax Exempt Invoice")]
        public bool notintaxinvoice { get; set; }

    }
    public class geowallviewmodal
    {
        [Key]
        public long geowallId { get; set; }
        [Display(Name = "latitude")]
        public string lat { get; set; }
        [Display(Name = "logitude")]
        public string log { get; set; }
        public int? distance { get; set; }



        [Display(Name = "Employee")]
        public long[] EmployeeId { get; set; }



    }
    public class keytableview
    {
        [Key]
        public long keyautoid { get; set; }
        public string keyvalue { get; set; }

        public string purpose { get; set; }
        public long expire { get; set; }


        public DateTime entrytime { get; set; }

        public long employeeid { get; set; }

    }
    public class cashnotes
    { 
        [Key]
   public long cashid { get; set; }

    public long thousand { get; set; }
    public long fivehundred { get; set; }
    public long hundred { get; set; }
    public long fifty { get; set; }

    public long twenty { get; set; }

        public long ten { get; set; }

        public long five { get; set; }

        public long one { get; set; }

        public long half { get; set; }

        public long quarters { get; set; }
    public string purpuse { get; set; }
        public string CreatedBy { get; set; }
    public DateTime trasdate  {get;set;}
        }
    public class handover
    {
       [Key]
        public long reqid { get; set; }
        public long reqby { get; set; }
        public long reqto { get; set; }   
        public DateTime reqdate { get; set; }
        public decimal amount { get; set; }
        public bool status { get; set; }
   }
    public class purchaseentrydocument
    {
        [Key]
        public long purid { get; set; }
        public long PurchaseId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DoucumentType { get; set; }
        public DateTime? Expiry { get; set; }
        public string Notes { get; set; }
    }
    public class Estimate
    {
        [Key]
        public long EstimateId { get; set; }
        public String QuotNo { get; set; }
        public string BillNo { get; set; }
        public DateTime EsttDate { get; set; }
        public long Customer { get; set; }
     

        public string CreatedUser { get; set; }


        public decimal QuotGrandTotal { get; set; }
        public string Remarks { get; set; }
 

        public long? Project { get; set; }
 
        public long? ProTask { get; set; }
        public string joborderno { get; set; }
        public string buildingno { get; set; }
        public string siteno { get; set; }
        public string flatno { get; set; }
        public string quoteref { get; set; }

    }
    public class EstimateItems
    {

        [Key]
        public long EstimateItemId { get; set; }
    
        public long EstimateId { get; set; }
        public string invno { get; set; }
        public DateTime invdate { get; set; }

        public string description { get; set; }
        public decimal amount { get; set; }

    }

}
