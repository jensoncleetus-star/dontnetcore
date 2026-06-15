using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class SalesReturnViewModel
    {
        [Required(ErrorMessage = "Voucher No. is required")]
        public string BillNo { get; set; }
        public decimal pricecategoryper { get; set; }
        
        //sales entryid=voucher No
        public long? SalesEntryId { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime SRDate { get; set; }


        // refer to table emploayee 
        public long? SRCashier { get; set; }

        // Sale type refers to POS Or Invoice
        public SaleType SaleType { get; set; }

        public long Customer { get; set; }

        //walking customer or default
        public CustomerType CustomerType { get; set; }

        // to definne payment type
        public string PayType { get; set; }

        // total items and total quantity
        public int SRItems { get; set; }
        public int SRItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SRSubTotal { get; set; }

        public decimal SRTax { get; set; }


        public decimal SRTaxAmount { get; set; }

        public decimal SRDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SRGrandTotal { get; set; }

        public string SRNote { get; set; }


        public int Print { get; set; }

        public DateTime SRCreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public long Branch { get; set; }

        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }

        public virtual ICollection<SEItems> SRItem { get; set; }
		[EmailAddress]
        [Display(Name = "Email Address")]
        public string custEmailId { get; set; }

        //walking customer
        public string CustomerName { get; set; }
        public string MobileNo { get; set; }


        public decimal SReturnAmount { get; set; }
        public decimal SRDueAmount { get; set; }
        public decimal SRTotal { get; set; }

        public string EmployeeName { get; set; }

        public ReturnType ReturnType { get; set; }
        public string Remarks { get; set; }

        public long? MaterialCenter { get; set; }

        public string MCName { get; set; }

        public long? SalesType { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }

        public List<SRItemViewModel> SRItemss { get; set; }
        public List<SRBillSundryViewModel> SRbs { get; set; }

        public IEnumerable<SalesType> SalesTypes { get; set; }
        public long SRNo { get; set; }

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
        public string ReturnTypeName{ get; set; }
        public List<ApprovalViewModel> Emp { get; set; }

        public long PrintLayout { get; set; }
    }
    public class SRItemViewModel
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
    public class SRBillSundryViewModel
    {
        public string BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<SRBillSundry> srbsundrys { get; set; }
    }
    public class SalesRtnDocumentViewModel
    {
        public long DocumentID { get; set; }
        public long SalesRtnId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}