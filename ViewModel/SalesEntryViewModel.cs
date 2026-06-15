using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;

namespace QuickSoft.ViewModel
{
    public class accbalance
    {
        public string acname { get; set; }
        public decimal opbalance { get; set; }
        public decimal clbalance { get; set; }
    }
    public class SalesEntryViewModel
    {
        public long SalesEntryId { get; set; }
        public long CreditLimit { get; set; }
        public long? pricecategoryid { get; set; }
        public decimal? currentbalance { get; set; }
        public long SENo { get; set; }

        [Required(ErrorMessage = "Invoice No. is required")]
        [Display(Name = "Invoice No.")]
        public string BillNo { get; set; }

        [Display(Name = "Date")]
        public DateTime SEDate { get; set; }

        [Display(Name = "DVNoteDate")]
        public DateTime? DVNotDate { get; set; }

        public string PONo { get; set; }

        public long Customer { get; set; }

        public long? SECashier { get; set; }

        public decimal SEDiscount { get; set; }

        public decimal SEPaidAmount { get; set; }
        public decimal SEDueAmount { get; set; }

        
        public decimal SETotal { get; set; }
        public decimal SEGrandTotal { get; set; }

        public string CustomerName { get; set; }

        public string EmployeeName { get; set; }

        public string MobileNo { get; set; }

        public string SENote { get; set; }

        public string PayType { get; set; }

        public CustomerType CustomerType { get; set; }
		//[EmailAddress]
        [Display(Name = "Email Address")]
        public string custEmailId { get; set; }

        [Display(Name = "Payment Method")]
        public long? PaymentMethod { get; set; }

        public long SalesType { get; set; }

        public string ConvertType { get; set; }
        public string ConvertNo { get; set; }

        public List<SEItemViewModel> SEItem { get; set; }
        public List<SEBillSundryViewModel> SEbs { get; set; }
        public IEnumerable<SalesType> SalesTypes { get; set; }
        public List<ConvertTransactionsViewModel> ConvModel { get; set; }

        [StringLength(150)]
        public string Location { get; set; }
        public string Remarks { get; set; }
        public decimal? bonus { get; set; }
        public long? MaterialCenter { get; set; }
        public string MCName { get; set; }

        public long ConTypeId { get; set; }
        public string ConType { get; set; }

        public long Branch { get; set; }

        public long? Currency { get; set; }
        public decimal? ConvertionRate { get; set; }
        public string DConvertionRate { get; set; }
        public decimal FCTotal { get; set; }        
        public SaleType SaleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? HireType { get; set; }
        public string HSCode { get; set; }
        [Display(Name = "Payment Terms")]
        [StringLength(150)]
        public string PaymentTerms { get; set; }
        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }
        public long[] ProTasks { get; set; }
        [Display(Name = "Disc%")]
        public decimal DisPercent { get; set; }

        [Display(Name = "Disc Amount")]
        public decimal DiscAmount { get; set; }

        public string convertFrom { get; set; }
        public string convertBill { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public int SaleTransCount { get; set; }
        public int PurTransCount { get; set; }

        public string SaleTypeName { get; set; }
        public string SalesTypeName { get; set; }
        public string EmailId { get; set; }

        public string HType { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }
        [Display(Name = "Print Layout")]
        public long? PrintLayout { get; set; }
    }
    public class commissionViewmodel
    {
            public string agent { get; set; }
        public string commisiontype { get; set; }
        public string commisionmode { get; set; }
        public string comvalue { get; set; }
    }
    public class SEItemViewModel
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
        public List<batches> ItemList { get; set; }
        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }

    }
public class batches
{
    public string batch { get; set; }
}
    public class SEPaymentViewModel
    {
        public long SENo { get; set; }
        public string BillNo { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime SEDate { get; set; }
        public string CustomerName { get; set; }

        //[DisplayFormat(DataFormatString = "{0:dd-mm-yyyy}")]
        public DateTime SEEntryDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal SEBillAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal SEPaidAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal SEBalanceAmount { get; set; }


        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "Pay Amount is required")]
        public decimal? SEPayAmount { get; set; }

        public DateTime SECreatedDate { get; set; }

        // public long SalesEntry { get; set; }

        //public long CreatedBranch { get; set; }
        //public string CreatedUserId { get; set; }
        //public DateTime SECreatedDate { get; set; }

        // public int Status { get; set; }
    }
    public class OptionalFieldViewModel
    {
        public long RefID { get; set; }
        //public string id { get; set; }
        // seno defines bill number BillNo defines company.invoiceprefix + SENo
        public string type { get; set; }
        public string field { get; set; }
        public long SENo { get; set; }
        [Required]
        public string BillNo { get; set; }

        [Display(Name = "Date")]
        [Required]

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime SEDate { get; set; }

        //purchase order number
        public string PONo { get; set; }

        // refer to table emploayee
        public long? SECashier { get; set; }

        // Sale type refers to POS Or Invoice
        public SaleType SaleType { get; set; }

        public long Customer { get; set; }

        //walking customer or default
        public CustomerType CustomerType { get; set; }

        // to define payment type
        public string PayType { get; set; }

        // total items and total quantity
        public int SEItems { get; set; }
        public decimal SEItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SESubTotal { get; set; }

        public decimal SETax { get; set; }


        public decimal SETaxAmount { get; set; }

        public decimal SEDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SEGrandTotal { get; set; }

        // extra note option
        public string SENote { get; set; }

        // print times may use
        public int Print { get; set; }

        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime SECreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public long Branch { get; set; }

        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }

        public long? OrderRefer { get; set; }

        // if payment method is null and customer type is cash its to be posted to cash account
        public long? PaymentMethod { get; set; }
        public long? PaymentAccount { get; set; }
        public long? MC { get; set; }

        //convert from quotation/dvnote etc..
        public string ConvertType { get; set; }
        public string ConvertNo { get; set; }

        [StringLength(150)]
        public string Location { get; set; }
        // POS-0,Sale-1,Hire-2,
        public long SalesType { get; set; }
        public string Remarks { get; set; }

        public long? MaterialCenter { get; set; }

        public long? Currency { get; set; }
        [StringLength(10)]
        public string ConvertionRate { get; set; }

        public decimal? FCTotal { get; set; }

        public long? Project { get; set; }
        public long? ProTask { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? DueDate { get; set; }
        public string DueReason { get; set; }

        //public SalesEntry()
        //{
        //    Print = 0;
        //    SEDiscount = 0;
        //    SalesType = 1;
        //    // PONo = 0;
        //}

        public virtual ICollection<SEItems> SEitem { get; set; }
        [StringLength(50)]
        public string HSCode { get; set; }
        [StringLength(50)]
        public string PaymentTerms { get; set; }

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

        public long? SaleAccount { get; set; }
    }
    public class SEBillSundryViewModel
    {
        public string BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<SEBillSundry> sebsundrys { get; set; }
        public ICollection<SEBillSundry> sebsundryr { get; set; }
    }

    public class ItemDetailViewModel
    {     
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal? ItemQuantity { get; set; }
        public string ItemUnit { get; set; }

        public decimal? ReceivedQty { get; set; }
        public decimal? DamageQty { get; set; }
        public decimal? MissingQty { get; set; }

        public decimal? DvQty { get; set; }
        public decimal? RetQty { get; set; }
        public decimal? TargetPrice { get; set; }
    }

    public class ConvertTransactionsViewModel
    {
        public long Id { get; set; }

        [StringLength(100)]
        [Required]
        public string ConvertFrom { get; set; }

        [StringLength(100)]
        [Required]
        public string ConvertTo { get; set; }

        public long From { get; set; }

        public long To { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public Status Status { get; set; }

        public long Branch { get; set; }

        public string BillNo { get; set; }
    }
    public class CreditSaleDocumentViewModel
    {
        public long DocumentID { get; set; }
        public long CreditSaleId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SettlementViewModel
    {       
        public string SEDate { get; set; }
        public EmployeePaymentType PaymentType { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNames { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string ChequeNo{ get; set; }
        public string CustomerName { get; set; }
        public string cuscash { get; set; }
        public string balcash { get; set; }
    }

}