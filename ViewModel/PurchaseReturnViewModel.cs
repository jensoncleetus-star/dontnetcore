using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class PurchaseReturnViewModel
    {
        [Required(ErrorMessage = "Voucher No. is required")]
        public string BillNo { get; set; }
        public long PRNo { get; set; }

        public long? purchaseEntryId { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime PRDate { get; set; }

        public long Supplier { get; set; }

        // refer to table emploayee 
        public long? PRCashier { get; set; }

        // to definne payment type
        public string PayType { get; set; }

        // total items and total quantity
        public int PRItems { get; set; }
        public int PRItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PRSubTotal { get; set; }

        public decimal PRTax { get; set; }
        public decimal PRTaxAmount { get; set; }

        public decimal PRDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PRGrandTotal { get; set; }

        // extra note option
        public string PRNote { get; set; }

        // print times may use
        public int Print { get; set; }

        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime PRCreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }

        public virtual ICollection<PRItemViewModel> PRItemNote { get; set; }

        public SupplierType SupplierType { get; set; }

        public decimal PReturnAmount { get; set; }
        public decimal PRDueAmount { get; set; }
        public decimal PRTotal { get; set; }

        public ReturnType ReturnType { get; set; }

        public string SupplierName { get; set; }
        public string EmployeeName { get; set; }
		[EmailAddress]
        [Display(Name = "Email Address")]
        public string suppEmailId { get; set; }
        public List<PRItemViewModel> PRItemss { get; set; }
        public List<PRBillSundryViewModel> PRbs { get; set; }
        public string Remarks { get; set; }

        public long? MaterialCenter { get; set; }
        public string MCName { get; set; }
        public long? PurchaseType { get; set; }
        public IEnumerable<PurchaseType> PurchaseTypes { get; set; }

        public List<FieldMapping> FieldMap { get; set; }

        public PurchaseHireType PurType { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public string PurTypeName { get; set; }
        public string PursTypeName { get; set; }
        public string EmailId { get; set; }
        public string ReturnTypeName { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }
        public long? PrintLayout { get; set; }
    }
    public class PurchaseRtnDocumentViewModel
    {
        public long DocumentID { get; set; }
        public long PurchRtnId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class PRItemViewModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public string ItemUnit { get; set; }
        public long Item { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public string itemNote { get; set; }
        public string PartNumber { get; set; }
        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
    public class PRBillSundryViewModel
    {
        public string BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<PRBillSundry> prbsundrys { get; set; }
    }
}