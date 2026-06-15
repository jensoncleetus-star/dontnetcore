using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class CustomerViewModel
    {
        public long CustomerID { get; set; }
   
        // [Required]
        // [Display(Name = "Customer Code")]
        public string CustomerCode { get; set; }

        //[Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        public virtual Contact ContactID { get; set; }


        //[Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }
        [Display(Name = "Include PDC")]
        public bool includepdc { get; set; }
        [Display(Name = "Create User")]
        public bool createuser { get; set; }
        [Display(Name = "Bonus Check")]
        public bool bonuscheck { get; set; }
        [Display(Name = "Bonus Based On")]
        public BonusBase bonusbaseamount { get; set; }
       
        [Display(Name = "Bonus Percentage")]
        public decimal? bonuspercentage { get; set; }
        [Display(Name = "Bonus Start Date")]
        public string startbonusdate { get; set; }
        [Display(Name = "Bonus Claim Percentange")]
        public decimal? bonusclimembility { get; set; }
        //[Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public string TermsandCondition { get; set; }
        public string Remark { get; set; }
        public long reminderrepeate { get; set; }
        public long[] AssignedMembers { get; set; }
        public virtual Accounts AccountID { get; set; }


        public string Location { get; set; }
        [Display(Name = "TRN")]
        public string TaxRegNo { get; set; }

        //[Display(Name = "TAX ID TRN ")]
        public string TaxID_TRN { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [StringLength(50)]
        [Display(Name = "Emirate")]
        public string State { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        public string Zip { get; set; }


        [StringLength(15)]
        [Phone]
        public string Phone { get; set; }

        [StringLength(15)]
        [Phone]
        public string Mobile { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Id")]
        public string EmailId { get; set; }

        public string Reference { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        //[Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }


        [DefaultValue(true)]
        public Status Status { get; set; }



        //bank details
        [Display(Name = "AccountsID")]
        public long AccountsID { get; set; }
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }
        [Display(Name = "Iban No")]
        public string IbanNo { get; set; }
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
        public string Swift { get; set; }
        //[Display(Name = "Opening Balance")]
        public decimal OpnBalance { get; set; }
        //[Display(Name = "Opening Balance Credit")]
        //public decimal OpnBalanceCr { get; set; }
        public DC DC { get; set; }
        public string DCname { get; set; }

        public string SalesPersonName { get; set; }
        [Display(Name = "Tax Type")]
        public TaxType? TaxType { get; set; }

        public List<ReferenceAccountViewModel> invoicedata { get; set; }

        public int CountryID { get; set; }
        public int StateID { get; set; }
        public int LocationID { get; set; }

        public long? Category { get; set; }

        public long? CustomerType { get; set; }

        public string Addres { get; set; }


        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }
        public string ConvertFrom { get; set; }
        //[Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

        public MobileViewModel mobmodel1 { get; set; }

        //[Display(Name = "Customer Alias")]
        public string Alias { get; set; }

        //[Display(Name = "Customer Print Name")]
        public string CustomerPrintName { get; set; }

        [Display(Name = "Type of contact")]
        public string TypeOfContact { get; set; }

        [Display(Name = "Type of contact")]
        public string CountryCode { get; set; }

        public string Website { get; set; }

        public long? ContactTypeID { get; set; }

        public decimal PreYearBalance { get; set; }
        public string Note { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public List<Contact> LstContacts { get; set; }

        public List<SalesEntry> LstSalesEntry { get; set; }

        public List<CustomerDocumentviewmodel> LstCutomerDocument { get; set; }
        public List<LeadDocument> LstLeadDocument { get; set; }
        public List<Accounts> LstAccounts { get; set; }
        public CustomerViewModel()
        {
            includepdc = false;
            CreditLimit = 1;
        }
    }
    public class AddCustomerLiteviewmodel
    {
        [Key]
        public long CustomerID { get; set; }
        public string CustomerCode { get; set; }
       

        //[Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(15)]
        [Phone]
        public string Mobile { get; set; }
    }
    public class CustomerDocumentviewmodel
    {

        public long DocumnetId { get; set; }


        public long CutomerID { get; set; }


        public string DoucumentType { get; set; }

        public DateTime Expiry { get; set; }

        public string Notes { get; set; }

        public string FilePath { get; set; }
        public long ContactId { get; set; }

        public long DocumentTypeID { get; set; }
        public string filenamelead { get; set; }
    }

    public class CustomerSubmitViewModel
    {
        public long CustomerID { get; set; }

        [Required]
        [Display(Name = "Customer Code")]
        public string CustomerCode { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        public virtual Contact ContactID { get; set; }


        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }
        [Display(Name = "Include PDC")]
        public bool includepdc { get; set; }
        [Display(Name = "Create User")]
        public bool createuser { get; set; }

        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        public string Lattitude { get; set; }
        public string Longitude { get; set; }

        public string Remark { get; set; }
        public string TermsandCondition { get; set; }
        public virtual Accounts AccountID { get; set; }


        public string Location { get; set; }
        [Display(Name = "TRN")]
        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        //[Required]
        //[StringLength(50)]
        //[Display(Name = "Emirate")]
        public string State { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        public string Zip { get; set; }


        [StringLength(15)]
        [Phone]
        public string Phone { get; set; }

        [StringLength(15)]
        [Phone]
        public string Mobile { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        //[EmailAddress]
        //[StringLength(100)]
        //[Display(Name = "Email Id")]
        public string EmailId { get; set; }

        public string Reference { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }


        [DefaultValue(true)]
        public Status Status { get; set; }



        //bank details
        [Display(Name = "AccountsID")]
        public long AccountsID { get; set; }
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }
        [Display(Name = "Iban No")]
        public string IbanNo { get; set; }
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
        public string Swift { get; set; }
        [Display(Name = "Opening Balance")]
        public decimal OpnBalance { get; set; }
        //[Display(Name = "Opening Balance Credit")]
        //public decimal OpnBalanceCr { get; set; }
        public DC DC { get; set; }
        public string DCname { get; set; }

        public string Note { get; set; }

        public decimal PreYearBalance { get; set; }

        public string SalesPersonName { get; set; }
        [Display(Name = "Tax Type")]
        public TaxType? TaxType { get; set; }

        public List<ReferenceAccountViewModel> invoicedata { get; set; }


        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }
        public string ConvertFrom { get; set; }
        [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }
        public List<LeadDocumentViewModel> CusDocuments { get; set; }
        public List<TaskDocumentViewModel> TaskDocuments { get; set; }
        public List<AmcDocumentViewModel> LstAmcDocument { get; set; }
        public List<CustTimelineViewModel> CustTimeLine { get; set; }
        public List<leaddetailsviewmodel> leads { get; set; }
        public string Alias { get; set; }
        public string CustomerPrintName { get; set; }

        public string TaxID_TRN { get; set; }

        public int? CountryID { get; set; }
        public int? StateID { get; set; }
        public int? LocationID { get; set; }

        public long? Category { get; set; }

        public long? CustomerType { get; set; }

        public string Addres { get; set; }
        [Display(Name = "Bonus Check")]
        public bool bonuscheck { get; set; }
        [Display(Name = "Bonus Based On")]
        public BonusBase bonusbaseamount { get; set; }

        [Display(Name = "Bonus Percentage")]
        public decimal? bonuspercentage { get; set; }

        [Display(Name = "Bonus Claim Percentange")]
        public decimal? bonusclimembility { get; set; }
        [Display(Name = "Bonus Start Date")]
        public string startbonusdate { get; set; }

        public string username { get; set; }
        public string password { get; set; }
        public List<Contact> LstContacts { get; set; }

        public List<SalesEntry> LstSalesEntry { get; set; }

        //public List<CustomerDocument> LstCutomerDocument { get; set; }
        public List<CustomerDocumentviewmodel> LstCutomerDocument { get; set; }

        public List<Accounts> LstAccounts { get; set; }

    }
    public class leaddetailsviewmodel
        {
        public long leadid { get; set; }
    public string leadname { get; set; }
        }
    public class CustTimelineViewModel
    {
        public string Name { get; set; }
        public string LogType { get; set; }
        public DateTime Time { get; set; }
        public string Details { get; set; }
    }
    public class CustomerRemarkviewmodal
    {
        
        public long CustomerId { get; set; }
        public string Remark { get; set; }
        public string AddedUser { get; set; }
        public decimal? expectedamount { get; set; }
        public decimal? priority { get; set; }
        public int? entrytype { get; set; }
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Next Date")]
        public string nextfolloupdate { get; set; }
        [Display(Name = "Next Time")]
        public DateTime? nextfolloupdatetime { get; set; }
        public string[] mobnumber { get; set; }
    }
    public class Appointmentviewmodal
    {

        public long CustomerId { get; set; }
        public string Remark { get; set; }
        public string AddedUser { get; set; }
  
        [Display(Name = "Appointment Date")]
        public string nextfolloupdate { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? nextfolloupdatetime { get; set; }
        public bool cancel { get; set; }
    }

}