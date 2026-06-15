using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Customer
    {
        [Key]
        public long CustomerID { get; set; }

        public string CustomerCode { get; set; }
        [Required]
        public string CustomerName { get; set; }
       public bool bonuscheck { get; set; }
public BonusBase bonusbaseamount { get; set; }
public decimal? bonuspercentage { get; set; }
public decimal? bonusclimembility { get; set; }
        public long Contact { get; set; }
        public virtual Contact ContactID{ get; set; }

        public decimal CreditLimit { get; set; }

        public int CreditPeriod { get; set; }

        public string Lattitude { get; set; }
        public string Longitude { get; set; }

        public string Location { get; set; }

        //public string TaxRegNo { get; set; }

        public string TaxID_TRN { get; set; }

        public long? SalesPerson { get; set; }

        public string Remark { get; set; }

        public long Accounts { get; set; }
        public virtual Accounts AccountID { get; set; }

        public int? OpenClose { get; set; }
        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string IbanNo { get; set; }
        public string BranchName { get; set; }
        public string Swift { get; set; }

        public TaxType? TaxType { get; set; }

        public CRMCustomerType Type { get; set; }

        public long? SourceOfLead { get; set; }

        public long EntryNo { get; set; }
        public long? LeadStat { get; set; }
        public string LeadLevel { get; set; }

        public string CustomerPrintName { get; set; }

        public int LocationID { get; set; }
        public int CountryID { get; set; }
        public int SourceID { get; set; }
        public string TaxRegNo { get; internal set; }
        public int LeadType { get; internal set; }
        public int StateID { get; internal set; }
        public int LeadCondition { get; internal set; }
        public int CurrentAction { get; internal set; }
        public int NextAction { get; internal set; }
        public DateTime? CreatedDate { get; internal set; }
        public string CreatedBy { get; internal set; }
        public long? Category { get; set; }
        public long? CustomerType { get; set; }
        public string TermsandCondition { get; set; }
        public string Addres { get; set; }
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
       public DateTime? logtime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool includepdc { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? startbonusdate { get; set; }
        public decimal? expectedamount { get; set; }
        public decimal? priority { get; set; }
        public Customer()
        {
            CreditLimit = 1;
            CreditPeriod = 0;
            TaxType = null;
            expectedamount = 0;
            priority = 0;
        }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartTime { get; set; }


    }
    public class customerleadrelation
    {
        [Key]
        public long assignid { get; set; }
        public long customerid { get; set; }
        public long leadid { get; set; }
    }
    public class customermerge
    {
        [Key]
        public long custmergeid { get; set; }
        public long oldcustomerid { get; set; }
        public long newcustomerid { get; set; }
        public DateTime createddate { get; set; }
        public string createduser { get; set; }
    }
    public class customermergelog
    {
        [Key]
        public long customermergelogid { get; set; }
        public long customermergeid { get; set; }
        public CustomerMergePurposeId purpose { get; set; }
        public long entryid { get; set; }
     
    }
    public class customerbonus
    {
        [Key]
        public long customerbonusid { get; set; }
        public long customerid { get; set; }
        public long salesentryid { get; set; }
        public decimal? invoiceamountwithouttax { get; set; }
        public decimal? materialcost { get; set; }
        public decimal? salesreturn { get; set; }
        public decimal? netprofit { get; set; }
        public decimal? expenses { get; set; }
        public decimal? claimamount { get; set; }
        public decimal? climableamount { get; set; }

    }
    public class leadcustomerrelation
    {
        [Key]
        public long assignid { get; set; }
        public long? customerid { get; set; }
        public long leadid { get; set; }
    }

    // for leads/pipeline
    public class CustomerConversion
    {
        [Key]
        public long ConverterID { get; set; }

        public long CustomerID { get; set; }

        public CRMCustomerType Type { get; set; }

        public string ConvertFrom { get; set; }

        public string ConvertedUser { get; set; }

        public DateTime ConvertedDate { get; set; }

        public string CreatedUser { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Remarks { get; set; }

        public Customer Customers { get; set; }
    }
    public class AssignedTo
    {
        public long AssignedToId { get; set; }
        public long CustomerID { get; set; }
        public long EmployeeId { get; set; }
        public String AssignBy { get; set; }
        public String Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ChkStatus { get; set; }
        public bool approve { get; set; }
    }

    public class LeadTaskRemark
    {
        [Key]
        public long TaskRemarkId { get; set; }
        public long TaskId { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }
        [Display(Name = "Task Status")]
        public long? TaskStatusID { get; set; }
        public long? TaskUpdationID { get; set; }
        public DateTime CreatedDate { get; set; }

        //public int CurrentAction { get; set; }
        //public int NextAction { get; set; }
        // long[] assignee list: EF Core 10 primitive-collection column absent from the snapshot; legacy EF6 kept it transient -> not mapped.
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public long[] AssignedTo { get; set; }
        
    }
    public class LeadTaskRemarkviewmodal
    {
        [Key]
        public long TaskRemarkId { get; set; }
        public long TaskId { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }
        [Display(Name = "Task Status")]
        public long? TaskStatusID { get; set; }
        public long? LocationId { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime?    CreatedDate { get; set; }
        [Display(Name = "End Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }
        public long? TaskUpdationID { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]

        public DateTime? FinishTime { get; set; }
        //public int CurrentAction { get; set; }
        //public int NextAction { get; set; }
        // long[] assignee list: EF Core 10 primitive-collection column absent from the snapshot; legacy EF6 kept it transient -> not mapped.
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public long[] AssignedTo { get; set; }

        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        public List<FieldMapping> FieldMap { get; set; }
    }

    public class LeadTaskAssigned
    {
        public long TaskAssignedId { get; set; }
        public long ProTaskId { get; set; }
        public long EmployeeId { get; set; }

        public string AssignBy { get; set; }
        public string Status { get; set; }
        public Status chkStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class LeadTaskUpdation
    {
        [Key]
        public long TaskUpdationID { get; set; }

        public long TaskId { get; set; }
        [Required]

        public string CreatedBy { get; set; }
        public DateTime? nextdate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? nexttime { get; set; }
        public string Location { get; set; }
        [StringLength(100)]
        public string Lattitude { get; set; }
        [StringLength(100)]
        public string Longitude { get; set; }

        public string Remarks { get; set; }
        public long leadstatus { get; set; }
        public DateTime? finishdate { get; set; }
        
        public DateTime? finishtime { get; set; }
    }

    public class LeadRemarkChecklist
    {
        [Key]
        public long Id { get; set; }
        public long Remark { get; set; }
        public long? Checklistitemid { get; set; }
        [StringLength(250)]
        public string Note { get; set; }
        public bool Check { get; set; }
    }
    public class ScopeOfWorkRemarkChecklist
    {
        [Key]
        public long Id { get; set; }
        public long Remark { get; set; }
        public long? Checklistitemid { get; set; }
        [StringLength(250)]
        public string Note { get; set; }
        public bool Check { get; set; }
        public long ScopeOfWorkItemsid { get; set; }
    }
    public class LeadTaskImage
    {
        [Key]
        public long TaskImageId { get; set; }
        public long TaskId { get; set; }
        public long? TaskUpdationID { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? TaskRemarkId { get; set; }
        public string CreatedBy { get; set; }

    }

    public class CustomerRemark
    {
        [Key]
        public long CustomerRemarkId { get; set; }
        public long CustomerId { get; set; }
        public string Remark { get; set; }
        public string AddedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime nexttime { get; set; }
        public DateTime nextdate { get; set; }


    }
    public class RemarkCustomer
    {
        [Key]
        public long RemarkId { get; set; }
        public long CustomerId { get; set; }
        public string Remark { get; set; }
        public string AddedUser { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public class RemarkCheque
    {
        [Key]
        public long RemarkId { get; set; }
        public long pdcid { get; set; }
        public string Remark { get; set; }
        public string status { get; set; }
        public string createdby { get; set; }
        public DateTime CreatedDate { get; set; }

    }


}