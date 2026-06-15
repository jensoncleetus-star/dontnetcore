using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class QuotationViewModel
    {
        public long? QuotationId { get; set; }
        public long QuotNo { get; set; }
        public string BillNo { get; set; }
        public string OpenClose { get; set; }
        public DateTime QuotDate { get; set; }
        public long? sourceoflead { get; set; }


        // refer to table emploayee 
        public long? QuotCashier { get; set; }

        public long Customer { get; set; }

        public string CustomerName { get; set; }

        public long? lead { get; set; }

        public string leadname { get; set; }

        public string CreatedUser { get; set; }

        public decimal QuotDiscount { get; set; }
        public long? Currency { get; set; }
        public decimal? ConvertionRate { get; set; }
        public string DConvertionRate { get; set; }
        public decimal QuotSubTotal { get; set; }
        //[DataType(DataType.Currency)]
        public decimal QuotGrandTotal { get; set; }
        public decimal FCTotal { get; set; }
        public int? QuotValidity { get; set; }
        public quotationstatus qtnstatus { get; set; }

        public long? QuotationType { get; set; }
        public long? servicetype { get; set; }
        public int SaleTransCount { get; set; }
        public int PurTransCount { get; set; }

        
        public string TermsCondition { get; set; }

        public string CustomerEmail { get; set; }

        public string CreatedUserEmail { get; set; }
        public string EmployeeName { get; set; }
        public string Remarks { get; set; }

        public long Branch { get; set; }
        public string revision { get; set; }
        public long? leadsid { get; set; }
        public DateTime? expdate { get; set; }
        public int? quotationstatus { get; set; }
      

        public List<QuotItemViewModel> QuotItem { get; set; }
        public List<QtBillSundryViewModel> QtBillSundry { get; set; }
        public string PartNumber { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }
        public string ProjectName { get; set; }
        public SaleType SaleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? quotationexpdate { get; set; }

        public long? HireType { get; set; }
        public long SalesType { get; set; }
        public IEnumerable<SalesType> SalesTypes { get; set; }
        [Display(Name = "Payment Terms")]
        [StringLength(50)]
        public string PaymentTerms { get; set; }
        //public string ProjectCode { get; set; }

        public long? ConTypeId { get; set; }
        public string ConType { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public string QuotTypeName { get; set; }
        public string QTypeName { get; set; }
        public string EmailId { get; set; }

        public string HType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }

        public long PrintLayout { get; set; }
        public string InvoiceNo { get; set; }
    }
    public class QuotItemViewModel
    {
        public long QuotationItemId { get; set; }

        public long Quotation { get; set; }

        public long Item { get; set; }


        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public string ItemNote { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public string PartNumber { get; set; }
        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
    public class QtBillSundryViewModel
    {
        public string BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<QtBillSundry> qtbsundrys { get; set; }
    }
    public class quotationdocumentviewmodel
    {
       
        public long qutid { get; set; }
        public long quotationID { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }

    }


    public class EstimateViewModel
    {
        public long? EstimateId { get; set; }
        public String QuotNo { get; set; }
        public string BillNo { get; set; }
        public string EsttDate { get; set; }
        public long Customer { get; set; }
        public string CustomerName { get; set; }

        public string CreatedUser { get; set; }


        public decimal QuotGrandTotal { get; set; }
        public string Remarks { get; set; }
        public List<EstItemViewModel> QuotItem { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }
        public string ProjectName { get; set; }

        public long PrintLayout { get; set; }
        public string joborderno { get; set; }
        public string buildingno { get; set; }
        public string siteno { get; set; }
        public string flatno { get; set; }
        public string quoteref { get; set; }
        public List<EsBillSundriesviewmodel> bsmodel { get; set; }
    }
  
    public class EsBillSundriesviewmodel
    {
        public string BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
    }
    public class EstItemViewModel
    {
        public long EstimateItemId { get; set; }

     
        public string invno { get; set; }
        public string invdate { get; set; }

        public string description { get; set; }
        public decimal amount { get; set; }

    }

}