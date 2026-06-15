using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuickSoft.Models;

namespace QuickSoft.ViewModel
{
    public class PurchaseEntryViewModel
    {
        public long PENo { get; set; }

        [Display(Name = "Invoice No:"), Required]
        [RegularExpression(@"^\S*$", ErrorMessage = "Space Not allowed In InvoiceNo")]
        public string BillNo { get; set; }
        public string ReferenceNo { get; set; }
        [Display(Name = "Date")]
        public DateTime PEDate { get; set; }

        // refer to table emploayee 
        public long? PECashier { get; set; }
        public long Supplier { get; set; }

        public decimal PEDiscount { get; set; }

        public decimal PEPaidAmount { get; set; }
        public decimal PEDueAmount { get; set; }


        public decimal PETotal { get; set; }
        public decimal PEGrandTotal { get; set; }

        public string SupplierName { get; set; }
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string suppEmailId { get; set; }
        public string PENote { get; set; }
        public string EmployeeName { get; set; }

        public SupplierType SupplierType { get; set; }

        public long PurchaseType { get; set; }

        public string PayType { get; set; }
        public string Remarks { get; set; }

        public long? MaterialCenter { get; set; }
        public string MCName { get; set; }

        public long ConTypeId { get; set; }
        public string ConType { get; set; }
        public string ConvertNo { get; set; }
        public long Branch { get; set; }
        public long? Currency { get; set; }
        public decimal? ConvertionRate { get; set; }

        public string DConvertionRate { get; set; }

        public decimal FCTotal { get; set; }
        [Display(Name = "Request Payment")]
        public bool requestpayment { get; set; }
        public List<PEItemViewModel> PEItem { get; set; }
        public List<PEBillSundryViewModel> PEbs { get; set; }
        public IEnumerable<PurchaseType> PurchaseTypes { get; set; }

        public List<ConvertTransactionsViewModel> ConvModel { get; set; }

        public long? CMReqNo { get; set; }
        public long? CPorderNo { get; set; }
        public long? CPQuotNo { get; set; }
        public long? CMRNoteNo { get; set; }

        public string convertFrom { get; set; }
        public string convertBill { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public PurchaseHireType PurchaseHireType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? CrossHireType { get; set; }

        [Display(Name = "Disc%")]
        public decimal DisPercent { get; set; }

        [Display(Name = "Disc Amount")]
        public decimal DiscAmount { get; set; }

        public string PurTypeName { get; set; }
        public string PursTypeName { get; set; }
        public string EmailId { get; set; }

        public string HType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }

        public long PrintLayout { get; set; }
    }
    public class PEItemViewModel
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
    public class PEPaymentViewModel
    {
        public long PENo { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime PEDate { get; set; }
        public string SupplierName { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime PEEntryDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal PEBillAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal PEPaidAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal PEBalanceAmount { get; set; }


        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "Pay Amount is required")]
        public decimal? PEPayAmount { get; set; }

        public DateTime PECreatedDate { get; set; }

    }
    public class PEBillSundryViewModel
    {
        public string BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<PEBillSundry> pebsundrys { get; set; }
    }

    public class SerialNoViewModel
    {
        public string BatchNo { get; set; }
        public string MFG { get; set; }
        public string EXP { get; set; }
        public DateTime? MFGd { get; set; }
        public DateTime? EXPd { get; set; }
        public decimal StockIn { get; set; }
        public decimal StockOut { get; set; }
        public bool itemstatus { get; set; }
        public bool status { get; set; }
        [Display(Name = "Item"), Required]
        public long ItemId { get; set; }
        public decimal cfactor { get; set; }
        public long? Priunit { get; set; }
        public long? Secunit { get; set; }
        public long? Unit { get; set; }
        public decimal Cost { get; set; }
        public long Order { get; set; }
        public string origin { get; set; }
        public long PurchaseEntryId { get; set; }
        public DateTime MfgDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public List<SerialNoObj> SerialNoObjs { get; set; }
        public List<itemUpdates> itemUpdatess { get; set; }
        public List<taskUpdates> taskUpdatess { get; set; }

    }

    public class SerialNoObj
    {

        public string SerialNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? MfgDate { get; set; }
        public decimal Quantity { get; set; }
        public long? UnitId { get; set; }
    }
    public class VenderRateViewModel
    {
        public long supplierid { get; set; }
        public DateTime entrydate { get; set; }
        public List<VendorNewRate> vendorNewRates { get; set; }


    }

    public class VendorNewRate
    {

        public long supplierentryid { get; set; }
        public string ItemType { get; set; }
        public string ExternalModal { get; set; }
        public string InternalModal { get; set; }
        public decimal Rate { get; set; }
        public string promotiondescription { get; set; }
        public decimal? promorate { get; set; }
        public decimal? stock { get; set; }
    }
    public class itemUpdateViewmodel
    {
        public long ItemId { get; set; }

    }
    public class itemUpdates
    {
        public long ItemID { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal? offerprice { get; set; }
        public bool lockprice { get; set; }
        public bool pricingstatagy { get; set; }
        public pricingstatagytype pricingstatagytype { get; set; }
        public AmountType amounttype { get; set; }
        public decimal? amount { get; set; }
        public decimal? currstock { get; set; }
        public bool status { get; set; }
    }
    public class taskUpdates
    {
        public long ProTaskId { get; set; }
        public string TaskCode { get; set; }
        public string TaskName { get; set; }
        public string CustomerName { get; set; }
        public long[] bulkAssign { get; set; }
        //public decimal? SellingPrice { get; set; }
        //public decimal PurchasePrice { get; set; }
        //public bool lockprice { get; set; }
        //public bool pricingstatagy { get; set; }
        //public pricingstatagytype pricingstatagytype { get; set; }
        //public AmountType amounttype { get; set; }
        //public decimal? amount { get; set; }
        //public decimal? currstock { get; set; }
        //public bool status { get; set; }
    }
    public class AccountMapUpdates
    {
        public string Name { get; set; }
        public decimal? ClosingBalance { get; set; }
        public long? Accntid { get; set; }
        public string FromDate { get; set; }
        public string toDate { get; set; }
    }
    public class AccntDashboardViewModel
    {
        public List<AccountMapUpdates> AccountMapUpdatess { get; set; }
    }
}