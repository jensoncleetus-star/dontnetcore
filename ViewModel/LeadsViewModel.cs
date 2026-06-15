using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;
using System.ComponentModel;

namespace QuickSoft.ViewModel
{
    public class LeadsViewModel
    {
      
        public long CustomerID { get; set; }

        public long LeadId { get; set; }
        public bool converttotask { get; set; }
        [Display(Name = "Assign Team")]
        public string AssignType { get; set; }

        [Display(Name = "AssignedTo")]
        public long[] AssignedTo { get; set; }

        public long[] AssignedToo { get; set; }
        public long[] ScopeOfWork { get; set; }
        public long[] AssignTypeAll { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        // [Required(ErrorMessage= "Leads Code Required")]
        [Display(Name = "Leads Code")]
        public string CustomerCode { get; set; }

        //[Required(ErrorMessage = "Leads Name Required")]
        // [StringLength(100, MinimumLength = 1, ErrorMessage = "Leads Name Required")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        public long? Contact { get; set; }


        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }


        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        //public string Lattitude { get; set; }
        //public string Longitude { get; set; }

        public string Remark { get; set; }
        public int? OpenClose { get; set; }
        public long? Accounts { get; set; }
  


        public string Location { get; set; }
        public int LocationID { get; set; }
        [Display(Name = "Lead Task Name")]


        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }
        public int CountryID { get; set; }


        [StringLength(50)]
        [Display(Name = "Emirate")]
        public string State { get; set; }
        public int StateID { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        public string Zip { get; set; }


        [StringLength(15)]
        //[Phone]
        public string Phone { get; set; }

        [StringLength(15)]
        // [Phone]
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

        [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }


        [DefaultValue(true)]
        public Status Status { get; set; }


        [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        //bank details
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

        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }
        //[Required]
        public string leadcustomer { get; set; }
        [Display(Name = "Lead Status")]
        public long? LeadStat { get; set; }

       

        [Display(Name = "Lead Level")]
        public string LeadLevel { get; set; }

        [Display(Name = "Skill Set")]
        public int? LeadType { get; set; }

        [Display(Name = "Lead Condition")]
        public int? LeadCondition { get; set; }

        [Display(Name = "Current Action")]
        public int? CurrentAction { get; set; }

        [Display(Name = "Next Action")]
        public int? NextAction { get; set; }

        public DateTime? DateTime { get; set; }
        [Display(Name = "End Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Appointment Date")]
        public string StartDatee { get; set; }
        [Display(Name = "Appointment Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "Next Date")]
        public string EndDatee { get; set; }
        public List<Contact> LstContacts { get; set; }

        public List<LeadDocument> LstLeadDocument { get; set; }
       

        public long[] CheckedItems { get; set; }
        public ICollection<RemarkChecklistViewModel> bstmodel { get; set; }
        public List<LeadChecklistItems> LstLeadChecklistItems { get; set; }
        public List<FieldMapping> FieldMap { get; set; }
        public List<FieldMapping> FieldMapAll { get; set; }
    }

    public class LeadDocumentViewModel
    {
        public long LeadDocumentId { get; set; }
        public long CustomerID { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string notes { get; set; }
        
    }


    public class LeadsDetailsViewModel
    {

        public long CustomerID { get; set; }

        [Display(Name = "AssignedTo")]
        public long[] AssignedTo { get; set; }

       
        //[Required]
        [Display(Name = "Leads Code")]
        public string CustomerCode { get; set; }

        //[Required]
        [Display(Name = "Leads Name")]
        public string CustomerName { get; set; }

        public long? Contact { get; set; }


        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }


        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        //public string Lattitude { get; set; }
        //public string Longitude { get; set; }

        public string Remark { get; set; }

        public long? Accounts { get; set; }


        public string Location { get; set; }
        [Display(Name = "Tax RegNo")]
        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        // [Required]
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

        [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }


        [DefaultValue(true)]
        public Status Status { get; set; }


        [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        //bank details
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

        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }

        [Display(Name = "Lead Status")]
        public long? LeadStat { get; set; }
       
        public string leadcustomer { get; set; }
        [Display(Name = "Lead Level")]
        public string LeadLevel { get; set; }
        public List<LeadDocumentViewModel> LeadDocuments { get; set; }
        public List<LeadDocumentViewModel> LeadcreateDocuments { get; set; }
        public string leadStausDisp { get; set; }
       
        public string leadSourceDisp { get; set; }
        public List<LeadAssignToViewModel> LeadAssign { get; set; }
        public List<LeadCreatedViewModel> LeadCreated { get; set; }
        public string CreatedUser { get; set; }
        public List<LeadActivityViewModel> LeadActivity { get; set; }
        public List<LeadTimelineViewModel> LeadTimeLine { get; set; }
        public List<ChecklistViewModel> check { get; set; }
        public List<workscops> allscope { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

    }
    public class LeadAssignToViewModel
    {
        public string Empname { get; set; }

        // public DateTime? PCreatedDate { get; set; }
    }

    public class LeadCreatedViewModel
    {
        public string CreatedUser { get; set; }
        public string CreatedDate { get; set; }
        public string Converted { get; set; }

        // public DateTime? PCreatedDate { get; set; }
    }

    public class LeadActivityViewModel
    {
        public string LogManagerID { get; set; }
        public string name { get; set; }
        public string logtype { get; set; }
        public string section { get; set; }
        public DateTime time { get; set; }
        public string sectionid { get; set; }
        public string details { get; set; }

        public string date { get; set; }
    }
    public class LeadTimelineViewModel
    {
        public string Name { get; set; }
        public string LogType { get; set; }
        public DateTime? Time { get; set; }
        public string Details { get; set; }
        public string TStatus { get; set; }
        public long ProTaskId { get; set; }
        public string FileName { get; set; }
        public long TaskDocumentId { get; set; }
        public List<TaskImageViewModel> RImages { get; set; }
        public List<ChecklistViewModel> check { get; set; }
    }

    public class LeadsSubmitViewModel
    {
      
        public long CustomerID { get; set; }
        public long LeadId { get; set; }
        public string format { get; set; }
        public bool converttotask { get; set; }
        [Display(Name = "AssignedTo")]
        public long[] AssignedToo { get; set; }

        public long[] AssignTypeAll { get; set; }
        public long[] ScopeOfWork { get; set; }

        // [Required(ErrorMessage = "Leads Code Required")]
        //[Display(Name = "Leads Code")]
        public string CustomerCode { get; set; }

        //  [Required(ErrorMessage = "Leads Name Required")]
        // [StringLength(100, MinimumLength = 1, ErrorMessage = "Leads Name Required")]
        //[Display(Name = "Leads Name")]
        public string CustomerName { get; set; }

        public long? Contact { get; set; }
        public string StartDatee { get; set; }
        public string EndDatee { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        //[Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }


        // [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        //public string Lattitude { get; set; }
        //public string Longitude { get; set; }

        public string Remark { get; set; }
        public int? OpenClose { get; set; }
        public long? Accounts { get; set; }


        public string Location { get; set; }
        //[Display(Name = "Tax RegNo")]


        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }


        // [StringLength(50)]
        // [Display(Name = "Emirate")]
        public string State { get; set; }

        //[StringLength(50)]
        public string City { get; set; }

        public string Zip { get; set; }


        [StringLength(15)]
        //[Phone]
        public string Phone { get; set; }

        [StringLength(15)]
        // [Phone]
        public string Mobile { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        [EmailAddress]
        [StringLength(100)]
        //[Display(Name = "Email Id")]
        public string EmailId { get; set; }

        public string Reference { get; set; }

        [StringLength(50)]
        // [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        //  [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }


        [DefaultValue(true)]
        public Status Status { get; set; }


        // [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        //bank details
        // [Display(Name = "Bank Name")]
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

        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }
       
        [Display(Name = "Lead Status")]
        public long? LeadStat { get; set; }
        //[Required]
        public string leadcustomer { get; set; }
        [Display(Name = "Lead Level")]
        public string LeadLevel { get; set; }

        // public ICollection<MobileViewModel> mobmodel { get; set; }
        public int LeadType { get; set; }
        public int CountryID { get; set; }
        public int StateID { get; set; }
        public int LocationID { get; set; }
        public int LeadCondition { get; set; }
        public int CurrentAction { get; set; }
        public int NextAction { get; set; }
        public DateTime DateTime { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }
        public List<Contact> LstContacts { get; set; }
        public ICollection<LeadRemarkChecklistViewModel> bstmodel { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? StartTime { get; set; }

        public List<LeadDocument> LstLeadDocument { get; set; }
    }

    public class LeadRemarklistViewModel
    {
        public long TaskRemarkId { get; set; }
        public long TaskId { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }
        [Display(Name = "Task Status")]
        public long? TaskStatusID { get; set; }
        public DateTime? EndDate { get; set; }
        public long? TaskUpdationID { get; set; }
        public DateTime CreatedDate { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }
        public DateTime? FinishTime { get; set; }
        public long[] AssignedTo { get; set; }
        public DateTime? SEDate { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        public ICollection<LeadRemarkChecklistViewModel> bstmodel { get; set; }
    }

    public class LeadRemarkChecklistViewModel
    {
        public long Id { get; set; }
        public string Note { get; set; }
        public string Check { get; set; }
        public long ScopeOfWorkItemsid { get; set; }
    }

   
}