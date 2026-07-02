using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class MonthWiseSaleReportViewModel
    {
        public ICollection<MonthWise> saleCount { get; set; }
        public ICollection<MonthWise> saleRetCount { get; set; }
        public ICollection<MonthWiseDecimal> taxableAmount { get; set; }
        public ICollection<MonthWiseDecimal> saleAmount { get; set; }
        public ICollection<MonthWiseDecimal> taxAmount { get; set; }
        public ICollection<MonthWiseDecimal> saleRetAmount { get; set; }
        public ICollection<MonthWiseDecimal> taxableRetAmount { get; set; }
        public ICollection<MonthWiseDecimal> taxRetAmount { get; set; }
        public ICollection<MonthWiseDecimal> netAmount { get; set; }
    }
    public class UaeVatReportViewModel
    {
        public ICollection<UaeVat> Sales { get; set; }
        public ICollection<UaeVat> SReturn { get; set; }
        public ICollection<UaeVat> Purchase { get; set; }
        public ICollection<UaeVat> PReturn { get; set; }
        public ICollection<UaeVat> Payments { get; set; }
    }
    public class UaeVat
    {
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public string State { get; set; }

    }
    public class hirereturs
    {
        public long SalesEntryId { get; set; }
        public string billno { get; set; }
        public DateTime sedate { get; set; }
        public string lpono { get; set; }
        public List<hirereturndetails> details { get; set; }


    }
    public class hirereturndetails
    {
        public decimal? ItemQuantity { get; set; }
        public DateTime? createddate { get; set; }
        public decimal selqty { get; set; }
        public decimal? balanceqty { get; set; }
        public string itemname { get; set; }
    }
    public class LedgerViewModel
    {
        public decimal OpeningBalance { get; set; }
        public string blnceType { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public string MainAccount { get; set; }
        public long? MainAccountID { get; set; }
        public ICollection<Ledger> Ledger { get; set; }
        public string Remark { get; set; }
        public string TRN { get; set; }
    }
    public class LedgerminiViewModel
    {
        public decimal OpeningBalance { get; set; }
        public string blnceType { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public string MainAccount { get; set; }
        public long? MainAccountID { get; set; }
        public ICollection<Ledgermini> Ledger { get; set; }
        public string Remark { get; set; }
        public string TRN { get; set; }
    }


    public class LedgertaxViewModel
    {
        public decimal OpeningBalance { get; set; }
        public string blnceType { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public string MainAccount { get; set; }
        public long? MainAccountID { get; set; }
        public ICollection<Ledgertax> Ledger { get; set; }
        public string Remark { get; set; }
        public string TRN { get; set; }
    }








    public class LedgerProViewModel
    {
        public decimal OpeningBalance { get; set; }
        public string blnceType { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public string MainAccount { get; set; }
        public long? MainAccountID { get; set; }
        public ICollection<Ledgerpro> Ledger { get; set; }
        public string Remark { get; set; }
        public string TRN { get; set; }
    }
    public class LedgerprofitViewModel
    {
       
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
       
        public ICollection<Ledgerprofit> Ledger { get; set; }
      
    }
    public class Ledgerprofit
    {

        public long transid { get; set; }
        public DateTime? transdate { get; set; }
        public string Pupose { get; set; }
        public long referance { get; set; }
        public string voucherno { get; set; }
        public string ledger { get; set; }
        public decimal? amount { get; set; }
        public string remark { get; set; }

    }
    public class Ledgerpro
    {
        public long? AccountId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? PdcDate { get; set; }
        public string chequeno { get; set; }
        public string bank { get; set; }
  
        public string Invoice { get; set; }
        public string Type { get; set; }
        public string RAccount { get; set; }
        public long? RAccountID { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string particulars { get; set; }
        public string Remark { get; set; }
        public decimal? Amount { get; set; }
        public long? MainId { get; set; }
        public string TRN { get; set; }
        public long TransactionId { get; set; }
        public long Reference { get; set; }
        public long Account { get; set; }
    }

    public class Ledger
    {
        public long? AccountId { get; set; }
        public DateTime? Date { get; set; }
        public string Invoice { get; set; }
        public string Type { get; set; }
        public string RAccount { get; set; }
        public long? RAccountID { get; set; }
        public string projectname { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string particulars { get; set; }
        public string Remark { get; set; }
        public decimal? Amount { get; set; }
        public long? MainId { get; set; }
        public string TRN { get; set; }
        public long TransactionId { get; set; }
        public long Reference { get; set; }
        public long Account { get; set; }
    }
    public class Ledgermini
    {
        public long? AccountId { get; set; }
        public DateTime? Date { get; set; }
        public string Invoice { get; set; }
        public string Type { get; set; }
        public string RAccount { get; set; }
        public long? RAccountID { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string particulars { get; set; }
        public string Remark { get; set; }
        public decimal? Amount { get; set; }
        public long? MainId { get; set; }
        public string TRN { get; set; }
        public long TransactionId { get; set; }
        public long Reference { get; set; }
        public long Account { get; set; }
        public string paymenttype { get; set; }
        public string ordertype { get; set; }
    }
    public class Ledgertax
    {
        public long? AccountId { get; set; }
        public DateTime? Date { get; set; }
        public string Invoice { get; set; }
        public string Type { get; set; }
        public string RAccount { get; set; }
        public long? RAccountID { get; set; }
        public decimal? Debit { get; set; }
        public string Debitwithouttax { get; set; }
        public string tax { get; set; }
        public decimal? Credit { get; set; }
        public string particulars { get; set; }
        public string Remark { get; set; }
        public decimal? Amount { get; set; }
        public long? MainId { get; set; }
        public string TRN { get; set; }
        public long TransactionId { get; set; }
        public long Reference { get; set; }
        public long Account { get; set; }
        public string paymenttype { get; set; }
        public string ordertype { get; set; }
    }
    public class OrderTypeWise
    {
        public decimal? SaleAmt { get; set; }
        public decimal? SaletaxAmt { get; set; }
        public decimal? SaletotAmt { get; set; }
        public long? NoOfVchSale { get; set; }
        public string Type { get; set; }
    }
    public class DailySummary
    {
        public cashnotes cn { get; set; }
            public cashnotes cno { get; set; }
        public decimal? salesreturn { get; set; }
        public decimal? startsection { get; set; }
        public decimal? endsection { get;set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm:ss}")]
        public DateTime? time { get; set; }
        public DateTime? Date { get; set; }
        public string by { get; set; }
        public DateTime? first { get; set; }
        public DateTime? last { get; set; }
        public string firstInvoice { get; set; }
        public string lastInvoice { get; set; }
        public decimal? customers { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Disc { get; set; }
        public decimal? Sales { get; set; }
        public decimal? STotal { get; set; }
        public decimal? GTotal { get; set; }
        public decimal? Average { get; set; }
        public ICollection<ItemSum> group { get; set; }
        public ICollection<ItemSum> item { get; set; }
        public ICollection<ItemSum> category { get; set; }
        public ICollection<ItemSum> Payment { get; set; }
        public ICollection<MenuSum> Menu { get; set; }
        public decimal? pysicalcash { get; set; }
        public string MCName { get; set; }


    }

    public class DailySummaryViewModel
    {

        public decimal? PurTotal { get; set; }

        public decimal? TotalSale { get; set; }
        public decimal? TotalCashSale { get; set; }
        public decimal? TotalCreditSale { get; set; }

        public decimal? TotalSalesReturn { get; set; }

        public decimal? TotalReceipt { get; set; }
        public decimal? RptTotalCash { get; set; }
        public decimal? RptTotalPdc { get; set; }

        public decimal? TotalPayment { get; set; }
        public decimal? PayTotalCash { get; set; }
        public decimal? PayTotalPdc { get; set; }


        public ICollection<SaleItemSum> SaleItems { get; set; }
        public ICollection<SaleItemSum> SaleRetItems { get; set; }
        public ICollection<SaleItemSum> PurItems { get; set; }
        public ICollection<SaleItemSum> PurRetItems { get; set; }
        public ICollection<SaleItemSum> TranItems { get; set; }
        public ICollection<SaleItemCategoryWise> SaleRetItemCatWise { get; set; }

        public DateTime? time { get; set; }
        public DateTime? Date { get; set; }
        public string by { get; set; }
        public string MCName { get; set; }

        public decimal? PTotal { get; set; }
        public decimal? PDiscount { get; set; }
        public decimal? PVat { get; set; }
        public decimal? PNet { get; set; }

        public decimal? PTotalC { get; set; }
        public decimal? PDiscountC { get; set; }
        public decimal? PVatC { get; set; }
        public decimal? PNetC { get; set; }

        public decimal? PRTotal { get; set; }
        public decimal? PRDiscount { get; set; }
        public decimal? PRVat { get; set; }
        public decimal? PRNet { get; set; }

        public decimal? NetPurchase { get; set; }

        public decimal? SDiscount { get; set; }
        public decimal? SVat { get; set; }
        public decimal? SaleNetAmount { get; set; }

        public decimal? SDiscountC { get; set; }
        public decimal? SVatC { get; set; }
        public decimal? SaleNetAmountC { get; set; }
        public decimal? TotalSaleC { get; set; }

        public decimal? SaleRetTotal { get; set; }
        public decimal? SaleRetDiscount { get; set; }
        public decimal? SaleRetVat { get; set; }
        public decimal? SaleRetNetAmount { get; set; }

        public decimal? SaleRetTotalC { get; set; }
        public decimal? SaleRetDiscountC { get; set; }
        public decimal? SaleRetVatC { get; set; }
        public decimal? SaleRetNetAmountC { get; set; }

        public decimal? NetSale { get; set; }

        public decimal? STransTotal { get; set; }

        public decimal? Receipt { get; set; }
        public decimal? Payment { get; set; }

        public decimal? ChqReceipt { get; set; }
        public decimal? ChqPayment { get; set; }

        public decimal? TotalVAT { get; set; }

        public string from { get; set; }
        public string to { get; set; }
        public string category { get; set; }
        public string UserName { get; set; }

        public List<CustDailySummary> CustSum { get; set; }
        public List<CustDailySummary> CustSumC { get; set; }
        public List<CustDailySummary> CustSumSR { get; set; }
        public List<CustDailySummary> CustSumSRC { get; set; }
        public List<CustDailySummary> CustSumR { get; set; }
        public List<CustDailySummary> CustSumRC { get; set; }
    }
    public class CustDailySummary
    {
        public string CustomerName { get; set; }
        public decimal? CustAmount { get; set; }
        public long? CustCount { get; set; }
    }
    public class SaleItemCategoryWise
    {
        // type -{item, category}
        public string PUName { get; set; }
        public string SUName { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal? Confactor { get; set; }
        public decimal? PriTotal { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? Quantity { get; set; }
    }

    public class ItemSum
    {
        // type -{item, category}
        public string type { get; set; }
        public string name { get; set; }
        public decimal? quantity { get; set; }
        public decimal? Amount { get; set; }
    }

    public class SaleItemSum
    {
        // type -{item, category}
        public string UName { get; set; }
        public string IName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Amount { get; set; }
    }

    public class MenuSum
    {
        // type -{item, category}
        public string name { get; set; }
        public decimal? customers { get; set; }
        public decimal? GTotal { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Disc { get; set; }
        public decimal? Sales { get; set; }
    }

    public class MonthlySalesReportVM
    {
        public DateTime? MonthYear { get; set; }

        public decimal? SESaleTax { get; set; }
        public decimal? SESaleTaxAmount { get; set; }
        public decimal? SESubTotal { get; set; }
        public decimal? SEGrandTotal { get; set; }

        public decimal? SRSaleTax { get; set; }
        public decimal? SRSaleTaxAmount { get; set; }
        public decimal? SRSubTotal { get; set; }
        public decimal? SRGrandTotal { get; set; }
    }

    public class AccountDaySummaryViewModel
    {
        public decimal OpeningBalance { get; set; }
        public string blnceType { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public string MainAccount { get; set; }
        public long? MainAccountID { get; set; }
        public ICollection<AccountDaySummary> Ledger { get; set; }
        public string Remark { get; set; }
    }
    public class AccountDaySummary
    {
        public DateTime Date { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string BalanceType { get; set; }
    }
    public class AccountMonthSummaryViewModel
    {
        public decimal OpeningBalance { get; set; }
        public string blnceType { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public string MainAccount { get; set; }
        public long? MainAccountID { get; set; }
        public ICollection<AccountMonthSummary> Ledger { get; set; }
        public string Remark { get; set; }
    }
    public class AccountMonthSummary
    {
        public string Month { get; set; }
        public string Year { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string BalanceType { get; set; }
    }
    public class TransactionSummary
    {
        public long id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> Opening { get; set; }
        public decimal SaleCr { get; set; }
        public decimal SaleDr { get; set; }
        public decimal PurCr { get; set; }
        public decimal PurDr { get; set; }
        public decimal PayCr { get; set; }
        public decimal PayDr { get; set; }
        public decimal RecCr { get; set; }
        public decimal RecDr { get; set; }
        public decimal JourCr { get; set; }
        public decimal JourDr { get; set; }
        public decimal SRetCr { get; set; }
        public decimal SRetDr { get; set; }
        public decimal PRetCr { get; set; }
        public decimal PRetDr { get; set; }
        public decimal ConCr { get; set; }
        public decimal ConDr { get; set; }
        public decimal DrnoteCr { get; set; }
        public decimal DrnoteDr { get; set; }
        public decimal CrnoteCr { get; set; }
        public decimal CrnoteDr { get; set; }
        public decimal Balance { get; set; }
        public string BalanceType { get; set; }
        public TransactionSummary()
        {
            Opening = null;
            SaleCr = 0;
            SaleDr = 0;
            PurCr = 0;
            PurDr = 0;
            PayCr = 0;
            PayDr = 0;
            RecCr = 0;
            RecDr = 0;
            JourCr = 0;
            JourDr = 0;
            SRetCr = 0;
            SRetDr = 0;
            PRetCr = 0;
            PRetDr = 0;
            ConCr = 0;
            ConDr = 0;
            DrnoteCr = 0;
            DrnoteDr = 0;
            CrnoteCr = 0;
            CrnoteDr = 0;
            Balance = 0;
        }
    }
    public class TransactionSummaryViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public ICollection<TransactionSummary> Ledger { get; set; }
        public string Remark { get; set; }
    }
    public class TransactionPropertySummaryViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public ICollection<TransactionSummary> Ledger { get; set; }
        public string Remark { get; set; }
        public string property { get; set; }
    }
    public class Transaction
    {
        public long RAccount { get; set; }
        public long Count { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public Transaction()
        {
            Count = 0;
            Debit = 0;
            Credit = 0;
        }
    }

    public class CompanyEmailFormat
    {
        public string BillNo { get; set; }
        public string Subject { get; set; }
    }

    public class pdfSummaryViewModel
    {
        public long id { get; set; }
        public long? mc { get; set; }
        public long? chid { get; set; }
        public long? salesretunid { get; set; }
        public string PartyName { get; set; }
        public string qutationtype { get; set; }
        public string zatcaBase64 { get; set; }
        public string BillNo { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public string Cashier { get; set; }
        public decimal? Discount { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Balance { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string TaskCode { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string TRN { get; set; }
        public string TermsCondition { get; set; }
        public string paytype { get; set; }
        public long BillId { get; set; }

        public string PONo { get; set; }
        public string ConvertType { get; set; }
        public string ConvertNo { get; set; }
        public string Location { get; set; }
        public decimal? bonusclaimed { get; set; }
        public string Remarks { get; set; }
        public long? Currency { get; set; }
        public string ConvertionRate { get; set; }
        public string currencysymbol { get; set; }
        public string currencycode { get; set; }
        public string customercode { get; set; }
        public decimal? FCTotal { get; set; }
        public decimal? CreditPeriod { get; set; }
        public Status? ProCheck { get; set; }
        public string PrjNameCode { get; set; }
        public long? AgainstInvoice { get; set; }

        public string ContactPerson { get; set; }
        public string PaymentTerms { get; set; }
        public long SalesType { get; set; }
        public SaleType SaleType { get; set; }
        public Status? ComHeadCheck { get; set; }
        public string HireType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string HSCode { get; set; }

        public Status? chkCode { get; set; }
        public Status? HideItemName { get; set; }
        public long? TimeOut { get; set; }

        public List<pdfItemViewModel> pdfItem { get; set; }
        public List<pdfItemViewModel> pdfHeading { get; set; }
        public List<pdfBillSundryViewModel> billsundry { get; set; }
      

        public decimal? PTax { get; set; }
        public string validity { get; set; }
        public string TaskName { get; set; }

        public string CMReqNo { get; set; }
        public long? CPQuotNo { get; set; }
        public long? CPorderNo { get; set; }
        public long? CMRNoteNo { get; set; }
        public long? PayTerms { get; set; }
        public string ContactNo { get; set; }

        public string ConvertFrom { get; set; }
        public string ConvertBill { get; set; }


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

        public DateTime? CreatedDate { get; set; }
        public string PreparedBy { get; set; }

        public ICollection<MobileViewModel> mobmodel;
        public List<msettlement> settlment;
        public string MCFrom { get; set; }
        public string MCTo { get; set; }

        public long? PurchaseType { get; set; }
        public string empemail { get; set; }
        public DateTime? DVNoteDate { get; set; }
        public string revision { get; set; }
        public int? validityqtn { get; set; }
    }
    public class msettlement
    {
        public EmployeePaymentType PaymentType;
        public decimal Amount;
    }
    public class pdfItemViewModel
    {
        public long Id { get; set; }
        public decimal? ItemUnitPrice { get; set; }
        public decimal? ItemQuantity { get; set; }
        public decimal? ItemSubTotal { get; set; }
        public string ItemNote { get; set; }
        public string WarrantyPeriod { get; set; }
        public decimal? ItemTax { get; set; }
        public decimal? ItemTaxAmount { get; set; }
        public decimal? ItemTotalAmount { get; set; }
        public decimal? ItemDiscount { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public string PartNumber { get; set; }

        public string CBM { get; set; }
        public string Weight { get; set; }
        public string ItemDescription { get; set; }


        public decimal? ItemPrice { get; set; }
        public string Barcode { get; set; }



        public Boolean KeepStock { get; set; }
        public string BatchNo { get; set; }
        public Boolean InSaleInvoice { get; set; }

        public List<pdfItemImg> img { get; set; }

        public Status? PNoStatus { get; set; }


        public List<pdfBundleViewModel> bundle { get; set; }


        public decimal? Damage { get; set; }
        public decimal? Missing { get; set; }
        public decimal? Received { get; set; }
        public decimal? DvItemQuantity { get; set; }
        public decimal? RetItemQuantity { get; set; }

        public decimal? Packet { get; set; }
        public decimal? MinQty { get; set; }
        public string Make { get; set; }
        public bool BomExist { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
    public class pdfBundleViewModel
    {
        public long Id { get; set; }
        public decimal? ItemUnitPrice { get; set; }
        public decimal? ItemQuantity { get; set; }
        public decimal? ItemSubTotal { get; set; }
        public string ItemNote { get; set; }

        public decimal? ItemTax { get; set; }
        public decimal? ItemTaxAmount { get; set; }
        public decimal? ItemTotalAmount { get; set; }
        public decimal? ItemDiscount { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public string PartNumber { get; set; }

        public string CBM { get; set; }
        public string Weight { get; set; }
        public string ItemDescription { get; set; }

        public Boolean InSaleInvoice { get; set; }
        public Boolean KeepStock { get; set; }

        public List<pdfItemImg> img { get; set; }

        public Status? PNoStatus { get; set; }


        public long Item { get; set; }
        public string ItemWithCode { get; set; }
        public long? ItemUnitID { get; set; }
        public long? SubUnitId { get; set; }
        public string ItemArabic { get; set; }

        public decimal? Damage { get; set; }
        public decimal? Missing { get; set; }
        public decimal? Received { get; set; }
        public decimal? DvItemQuantity { get; set; }
        public decimal? RetItemQuantity { get; set; }

        public decimal? Packet { get; set; }
        public decimal? MinQty { get; set; }

        public DateTime? CreatedDate { get; set; }

    }
    public class pdfItemImg
    {
        public string FileName { get; set; }
        public Int64 Status { get; set; }
        public long? ItemImageID { get; set; }
    }

    public class pdfBillSundryViewModel
    {
        public long? AmountType { get; set; }
        public decimal? BsAmount { get; set; }
        public long? BsType { get; set; }
        public decimal? BsValue { get; set; }

        public string BillSundry { get; set; }
    }

    public class profitabilityViewModel
    {
        public long? ProjectId { get; set; }
        public long? reference { get; set; }
        public string Name { get; set; }
        public decimal? income { get; set; }
        public decimal? expense { get; set; }
        public decimal? profit { get; set; }
        public bool parent { get; set; }
        public string customer { get; set; }
        public string Procode { get; set; }
        public string Invoice { get; set; }
        public long acc { get; set; }
        public long Account { get; set; }
    }
    public class AttachmentsModel
    {
        public long? AttachmentID { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
    }

    //public class PassingItem
    //{
    //    public decimal? TotTaxableAmount { get; set; }
    //    public decimal? TotTaxAmount { get; set; }
    //    public decimal? GrandTot { get; set; }
    //    public decimal? QtyTot { get; set; }
    //}
    public class PaySlipViewModel
    {
        public string Employee { get; set; }
        public string EmployeeCode { get; set; }
        public string Designation { get; set; }
        public DateTime? DateofJoin { get; set; }
        public string PayslipDate { get; set; }

        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string IbanNo { get; set; }
        public string BranchName { get; set; }
        public string Swift { get; set; }

        public List<AttendDetail> Attend { get; set; }
        public List<EarnDuductDetail> Earning { get; set; }
        public List<EarnDuductDetail> Deduction { get; set; }
        public DateTime? MonthYear { get; set; }
        public string Note { get; set; }

    }
    public class AttendDetail
    {
        public string Name { get; set; }
        public decimal? Value { get; set; }
        public string PeriodType { get; set; }
    }
    public class EarnDuductDetail
    {
        public string Name { get; set; }
        public decimal? Amount { get; set; }
        public decimal? GrossSalary { get; set; }
    }
    public class EventViewModel
    {
        public Int64 id { get; set; }

        public String title { get; set; }

        public String start { get; set; }

        public String end { get; set; }

        public bool allDay { get; set; }
    }
    public class HolidayListViewModel
    {
        public string HName { get; set; }
        public DateTime Date { get; set; }
    }
    public class PayrollStatementViewModel
    {
        public int PheadCount{ get; set; }
        public List<Payhead> Phead { get; set; }
        public List<PSettleVModel> PSettle { get; set; }
        public List<EmpPayHeadBreakupViewModel> EmpPhead { get; set; }

    }
    public class PSettleVModel
    {
        public string Department { get; set; }
        public string Employee { get; set; }
        public string EmployeeCode { get; set; }
        public string BankDetails { get; set; }
        public List<PayheadVModel> PheadVModel { get; set; }
        public decimal? Total { get; set; }
        public string Note { get; set; }
    }
    public class PayheadVModel
    {
        public long? ID { get; set; }
        public string Name { get; set; }
        public decimal Rate { get; set; }
    }
    public class PaymentAdviceViewModel
    {
        public DateTime? MonthYearF { get; set; }
        public DateTime? MonthYearT { get; set; }
        public string BankName { get; set; }
        public string CompanyAccount { get; set; }
        public List<PAdviceViewModel> PAdvice { get; set; }
    }
    public class PAdviceViewModel
    {
        public string EmpName { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string Branch { get; set; }
        public decimal? Amount { get; set; }
    }
    public class EmpPayHeadBreakupViewModel
    {
        public string Department { get; set; }
        public string Employee { get; set; }
        public decimal? OpeningBalance { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal? ClosingBalance { get; set; }

        public string CrDr { get; set; }

        public string PayHead { get; set; }
        public string AccountName { get; set; }

        public long? AccGroup { get; set; }
    }
    public class SalaryStructureDetail
    {
        public long? PayHeadId { get; set; }
        public string PayHead { get; set; }
        public decimal? Rate { get; set; }
        public long? Days { get; set; }
        public string HeadType { get; set; }

        public string CalType { get; set; }
        public string Computed { get; set; }
        public string Basis { get; set; }

        public long? EmpId { get; set; }
        public string EmpName { get; set; }
        public string Per { get; set; }
        public string Leave { get; set; }

        public string type { get; set; }

        public decimal? rateprice { get; set; }


    }
    public class SalaryStructureDetailPR
    {
        public List<SalaryStructureDetailWithEmp> SLEmp { get; set; }
    }
    public class SalaryStructureDetailWithEmp
    {
        public string EMPName { get; set; }
        public long EMPId { get; set; }
        public List<SalaryStructureDetail> SalStr { get; set; }
    }
}