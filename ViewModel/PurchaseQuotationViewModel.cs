using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class PurchaseQuotationViewModel
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

        public string SupplierName { get; set; }

        public string CreatedUser { get; set; }

        public decimal QuotDiscount { get; set; }

        public decimal PQuotDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PQuotGrandTotal { get; set; }
        //[DataType(DataType.Currency)]
        public decimal PQuotSubTotal { get; set; }

        // extra note option
        public string PQuotNote { get; set; }

        // mail times may use
        public int Mail { get; set; }

        public int? PQuotValidity { get; set; }


        

        public string TermsCondition { get; set; }

        public string SupplierEmail { get; set; }

        public string CreatedUserEmail { get; set; }
        public string EmployeeName { get; set; }
        public string Remarks { get; set; }

        public long Branch { get; set; }


        public List<PQuotItemViewModel> PQuotItem { get; set; }
        public List<PQtBillSundryViewModel> PQtBillSundry { get; set; }
        public string PartNumber { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }
        public string ProjectName { get; set; }
        public string ApprovedBy { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public IEnumerable<SalesType> SalesTypes { get; set; }
        [Display(Name = "Payment Terms")]
        [StringLength(50)]
        public string PaymentTerms { get; set; }
        public IEnumerable<PurchaseType> PurchaseTypes { get; set; }
        public long PurchaseType { get; set; }
        public long ConTypeId { get; set; }
        public string ConType { get; set; }

        public string CMReqNo { get; set; }

        public string convertFrom { get; set; }
        public string convertBill { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public string PayTerms { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }
        public long PrintLayout { get; set; }
    }

    public class PQuotItemViewModel
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
        public string MakeName { get; set; }
        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
    public class PQtBillSundryViewModel
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
}