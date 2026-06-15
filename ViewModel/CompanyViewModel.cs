using QuickSoft.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class CompanyViewModel
    {
        public Company Company { get; set; }
        [StringLength(20)]
        [Display(Name = "Invoice Prefix")]
        public string InvoicePrefix { get; set; }
        [StringLength(20)]
        [Display(Name = "Purchase Prefix")]
        public string PurchasePrefix { get; set; }
        [StringLength(20)]
        [Display(Name = "Quotation Prefix")]
        public string QuotationPrefix { get; set; }
        [StringLength(20)]
        [Display(Name = "ProForma Prefix")]
        public string ProFormaPrefix { get; set; }
        [StringLength(20)]
        [Display(Name = "Customer Code Prefix")]
        public string Customer { get; set; }
        [StringLength(20)]
        [Display(Name = "Supplier Code Prefix")]
        public string Supplier { get; set; }
        [StringLength(20)]
        [Display(Name = "Item Code Prefix")]
        public string Item { get; set; }
        [StringLength(20)]
        [Display(Name = "Branch Code Prefix")]
        public string Branch { get; set; }
        [StringLength(20)]
        [Display(Name = "Delivery Note Prefix")]
        public string Deliverynote { get; set; }
        [StringLength(20)]
        [Display(Name = "Payment Prefix")]
        public string Payment { get; set; }
        [StringLength(20)]
        [Display(Name = "Receipt Prefix")]
        public string Reciept { get; set; }
        [StringLength(20)]
        [Display(Name = "Employee Prefix")]
        public string Employee { get; set; }
        [StringLength(20)]
        [Display(Name = "Purchase Return Prefix")]
        public string PReturn { get; set; }
        [StringLength(20)]
        [Display(Name = "Sales Return Prefix")]
        public string SReturn { get; set; }
        [StringLength(20)]
        [Display(Name = "Purchase Order Prefix")]
        public string POrder { get; set; }
        [StringLength(20)]
        [Display(Name = "Tax Exempt Invoice Prefix")]
        public string TaxExempt { get; set; }
        [Display(Name = "Back Up Folder Path")]
        public string DbBackUpPath { get; set; }

        public string Header { get; set; }

        public string Footer { get; set; }

        [Display(Name = "Remove Header And Footer")]
        public Boolean RemoveHeaderFooter { get; set; }

        [Display(Name = "Remove Logo")]
        public Boolean RemoveLogo { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CheckboxFinYear { get; set; }

        [StringLength(20)]
        [Display(Name = "Packing List Prefix")]
        public string Packinglist { get; set; }

        //hire section
        [StringLength(20)]
        [Display(Name = "Hire Invoice Prefix")]
        public string HireInvoicePrefix { get; set; }
        [StringLength(20)]
        [Display(Name = "Hire Sales Order Prefix")]
        public string HireSalesOrderPrefix { get; set; }
        [StringLength(20)]
        [Display(Name = " Hire ProForma Prefix")]
        public string HireProformaPrefix { get; set; }
        [StringLength(20)]
        [Display(Name = "Hire Quotation Prefix")]
        public string HireQuotationPrefix { get; set; }
        [StringLength(20)]
        [Display(Name = "Hire Delivery Note Prefix")]
        public string HireDelivernotePrefix { get; set; }
        [StringLength(20)]
        [Display(Name = "Hire Return Prefix")]
        public string HireReturn { get; set; }
        [StringLength(20)]
        [Display(Name = "Cross Hire Invoice Prefix")]
        public string CrossHireInvoicePrefix { get; set; }

        [StringLength(20)]
        [Display(Name = "Production Prefix")]
        public string Production { get; set; }
        [StringLength(20)]
        [Display(Name = "Unassemble Prefix")]
        public string Unassemble { get; set; }

        [StringLength(20)]
        [Display(Name = "Material Requisition Prefix")]
        public string MR { get; set; }
        [StringLength(20)]
        [Display(Name = "Material Receive Note Prefix")]
        public string MRNote { get; set; }
        [StringLength(20)]
        [Display(Name = "Purchase Quotation Prefix")]
        public string PQuotation { get; set; }
        [StringLength(20)]
        [Display(Name = "Journal Prefix")]
        public string Journal { get; set; }

        [StringLength(20)]
        [Display(Name = "JobCard Prefix")]
        public string JobCard { get; set; }

        [StringLength(20)]
        [Display(Name = "Material Center Prefix")]
        public string MC { get; set; }

        [StringLength(20)]
        [Display(Name = "Project Prefix")]
        public string Project { get; set; }

        [StringLength(20)]
        [Display(Name = "Task Prefix")]
        public string Task { get; set; }

        [StringLength(20)]
        [Display(Name = "SalesOrder Prefix")]
        public string SalesOrder { get; set; }

        [StringLength(20)]
        [Display(Name = "StockAdjustment Prefix")]
        public string StockAdjustment { get; set; }

        [StringLength(20)]
        [Display(Name = "StockJournal Prefix")]
        public string StockJournal { get; set; }

        [StringLength(20)]
        [Display(Name = "StockTransfer Prefix")]
        public string StockTransfer { get; set; }

        [StringLength(20)]
        [Display(Name = "StockVerification Prefix")]
        public string StockVerification { get; set; }

        [Display(Name = "Sale Account")]
        public long? SaleAccount { get; set; }
        [Display(Name = "Purchase Account")]
        public long? PurchaseAccount { get; set; }
        [Display(Name = "Sale Return Account")]
        public long? SReturnAccount { get; set; }
        [Display(Name = "Purchase Return Account")]
        public long? PReturnAccount { get; set; }

        [Display(Name = "Salary Account")]
        public long? SalaryAccount { get; set; }

        [Display(Name = "Tenancy Contract Account")]
        public long? tenancycontractaccount { get; set; }
        [Display(Name = "Tenancy Contract Security Deposit Account")]
        public long? tenancycontractSecurityDepositaccount { get; set; }
        [Display(Name = "Rental Income Account")]
        public long? RentalIncomeaccount { get; set; }
        [Display(Name = "Registration Deposit Account")]
        public long? Regdepositaccount { get; set; }
        public long? Broker { get; set; }
        public long? Landlord { get; set; }
        public long? Developer { get; set; }
        public long? Contractor { get; set; }
        public long? Tenant { get; set; }

        [Display(Name = "Bank Account")]
        public long? BankAccount { get; set; }

        public bool Custominvoice { get; set; }
        [Display(Name = "Payroll Start Date")]
        public string Payrolldate { get; set; }
        [Display(Name = "Purchase Payment Request Approved By")]
        public long[] AssignedTo { get; set; }
        public long[] AssignedToo { get; set; }
    }
}