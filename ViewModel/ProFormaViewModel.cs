using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuickSoft.Models;


namespace QuickSoft.ViewModel
{
    public class ProFormaViewModel
    {
        public long PFNo { get; set; }

        [Required(ErrorMessage = "Invoice No. is required")]
        [Display(Name = "Invoice No.")]
        public string BillNo { get; set; }

        [Display(Name = "Date")] 
        public DateTime PFDate { get; set; }

      
        public long Customer { get; set; }

        public long? PFCashier { get; set; }

        public decimal PFDiscount { get; set; }

        public decimal PFPaidAmount { get; set; }
        public decimal PFDueAmount { get; set; }


        public decimal PFTotal { get; set; }
        public decimal PFGrandTotal { get; set; }

        public string CustomerName { get; set; }

        public string EmployeeName { get; set; }

        public string MobileNo { get; set; }

        public string PFNote { get; set; }
		[EmailAddress]
        [Display(Name = "Email Address")]
        public string custEmailId { get; set; }

        public List<PFItemViewModel> PFItem { get; set; }
        public List<PFBillSundryViewModel> PFbs { get; set; }
        [StringLength(150)]
        public string Location { get; set; }
        public CustomerType CustomerType { get; set; }
        public string PayType { get; set; }

        public int? CreditPeriod { get; set; }
        public string Remarks { get; set; }

        public long? MaterialCenter { get; set; }
        public string MCName { get; set; }

        public long ConTypeId { get; set; }
        public string ConType { get; set; }

        public long Branch { get; set; }
        public SaleType SaleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? HireType { get; set; }
        public string HSCode { get; set; }

        public IEnumerable<SalesType> SalesTypes { get; set; }
        public long SalesType { get; set; }
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
    public class PFItemViewModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public string ItemUnit { get; set; }
        public long Item { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemDiscount { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public string itemNote { get; set; }
        public string PartNumber { get; set; }

        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
    public class PFBillSundryViewModel
    {
        public string BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<PFBillSundry> pfbsundrys { get; set; }
    }

    public class ProformaDocumentViewModel
    {
        public long DocumentID { get; set; }
        public long ProformaId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}