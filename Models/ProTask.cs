using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class servicereportmember
    {
        [Key]
        public long servicereportmemberid { get; set; }
        public long servicereportid { get; set; }
        public long employeeid { get; set; }
    }
    public class additionaltaks
    {
        [Key]
        public long additionaltaskiad { get; set; }
        public long salesentryid { get; set; }
        public long taskid { get; set; }
    }
    public class SuggestItem
    {
        [Key]
        public long suggestid { get; set; }
        public long priitemid { get; set; }
        public long sugitemid { get; set; }
    }
    public class servicereport
    {
        [Key]
        public long servicereportid { get; set; }

        public long protaskid { get; set; }
        public DateTime? starttime { get; set; }
        public DateTime? endtime { get; set; }
        public long jobstatusid { get; set; }
        public string remark { get; set; }
        public JobType jobtypes { get; set; }
        public PaymentType paytype { get; set; }
        public decimal? amount { get; set; }
        public string chequenumber { get; set; }
        public string bankname { get; set; }
        public string createdby { get; set; }
    }
        public class ProTask
    {
        public long ProTaskId { get; set; }

        public string TaskName { get; set; }
        
        public string TaskCode { get; set; }
        public long TaskNo { get; set; }

        public long? ProjectId { get; set; }
        public Project Project { get; set; }

        public long? TaskType { get; set; }
        public ProTaskType TaskTypeId { get; set; }

        
        public string TaskDetails { get; set; }
        public string Note { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EndTime { get; set; }

        public TaskPriority Priority { get; set; }

        public long? TaskStatus { get; set; }
        //public TaskStatus TaskStatus { get; set; }

        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public long? CustomerID { get; set; }
        public Customer Customer { get; set; }
        public string Location { get; set; }

        public long? SalesPerson { get; set; }

        public long? salesexecutive { get; set; }
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

        public DateTime logtime { get; set; }
        public long? VTypId { get; set; }
        public long? VManuId { get; set; }
        public long? VModId { get; set; }
        public long? driver { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public int? OpenClose { get; set; }
    }
    //public class TaskTeam {
    //    public long TaskTeamId { get; set; }
    //    public long Task { get; set; }
    //    public long TeamLead { get; set; }
    //}
    public class ProTasknontech
    {
        public long ProTaskId { get; set; }

        public string TaskName { get; set; }

        public string TaskCode { get; set; }
        public long TaskNo { get; set; }

        public long? ProjectId { get; set; }
        public Project Project { get; set; }

        public long? TaskType { get; set; }
        public ProTaskType TaskTypeId { get; set; }

        
        public string TaskDetails { get; set; }
        public string Note { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? EndTime { get; set; }

        public TaskPriority Priority { get; set; }

        public long? TaskStatus { get; set; }
        //public TaskStatus TaskStatus { get; set; }

        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public long? CustomerID { get; set; }
        public Customer Customer { get; set; }
        public string Location { get; set; }

        public long? SalesPerson { get; set; }


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

        public DateTime logtime { get; set; }
        public long? VTypId { get; set; }
        public long? VManuId { get; set; }
        public long? VModId { get; set; }
        public long? driver { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public int? OpenClose { get; set; }
    }
    public class ProTaskUpdation
    {
        [Key]
        public long TaskUpdationID { get; set; }

        public long ProTaskId { get; set; }
        public ProTask ProTask { get; set; }
        [Required]
        //public TKUpdateStatus Status { get; set; }

        //public long? TaskTeamId { get; set; }
        //public TaskTeam TaskTeam { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string Location { get; set; }
        [StringLength(100)]
        public string Lattitude { get; set; }
        [StringLength(100)]
        public string Longitude { get; set; }

        public string Remarks { get; set; }
    }
    public class ProTaskType
    {
        [Key]
        public long TaskTypeId { get; set; }
        public string TypeName { get; set; }

        public Status Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long? Branch { get; set; }
        public Branch CreatedBranch { get; set; }
    }
    public class ProLeaveRequest
    {
        [Key]
        public long LeaveRequestId { get; set; }
        public int LeaveType { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime Createdate { get; set; }
        public string CreatedBy { get; set; }
        [Display(Name = "Leave From ")]
        public DateTime leavefromdate { get; set; }
        [Display(Name = "Leave From Time")]
        public DateTime? leavefromtime { get; set; }
        [Display(Name = "Leave To Date ")]
        public DateTime leavetodate { get; set; }
        [Display(Name = "Leave To Time")]
        public DateTime? leavetotime { get; set; }
        [Display(Name = "Reason")]

        public string leavereason { get; set; }
        public string approvedby { get; set; }
        public DateTime? approveddate { get; set; }
        public string notes { get; set; }
    }
    public class ProLeaveRequestviewmodel
    {
        
        public int LeaveType { get; set; }
        public long? SECashier { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        [Display(Name = "Leave From ")]
        public string leavefromdate { get; set; }
        [Display(Name = "Leave From Time")]
        public DateTime? leavefromtime { get; set; }
        [Display(Name = "Leave To Date ")]
        public string leavetodate { get; set; }
        [Display(Name = "Leave To Time")]
        public DateTime? leavetotime { get; set; }
        [Display(Name = "Reason")]

        public string leavereason { get; set; }
        public string approvedby { get; set; }
        public DateTime? approveddate { get; set; }
        public string notes { get; set; }
    }
    public class ProResignRequest
    {
        [Key]
        public long ResignRequestId { get; set; }
        public int ResignType { get; set; }
        public ApprovalStatus Status { get; set; }
        public DateTime Createdate { get; set; }
        public string CreatedBy { get; set; }
        [Display(Name = "Resign From ")]
        public DateTime Resignfromdate { get; set; }
        [Display(Name = "Resign From Time")]
        public DateTime? Resignfromtime { get; set; }
        [Display(Name = "Resign To Date ")]
        public DateTime Resigntodate { get; set; }
        [Display(Name = "Resign To Time")]
        public DateTime? Resigntotime { get; set; }
        [Display(Name = "Reason")]

        public string Resignreason { get; set; }
        public string approvedby { get; set; }
        public DateTime? approveddate { get; set; }
        public string notes { get; set; }
    }
    public class ProResignRequestviewmodel
    {

        public int ResignType { get; set; }
        public long? SECashier { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        [Display(Name = "Resign From ")]
        public string Resignfromdate { get; set; }
        [Display(Name = "Resign From Time")]
        public DateTime? Resignfromtime { get; set; }
        [Display(Name = "Resign To Date ")]
        public string Resigntodate { get; set; }
        [Display(Name = "Resign To Time")]
        public DateTime? Resigntotime { get; set; }
        [Display(Name = "Reason")]

        public string Resignreason { get; set; }
        public string approvedby { get; set; }
        public DateTime? approveddate { get; set; }
        public string notes { get; set; }
    } 
    public class ProTaskManner
    {
        [Key]
        public long TaskTypeId { get; set; }
        public string TypeName { get; set; }

        public Status Status { get; set; }
        public DateTime? CreatedDate { get; set; } 
        public string CreatedBy { get; set; }
        public long? Branch { get; set; }
        public Branch CreatedBranch { get; set; }
    }
    //public class TaskTeamMember
    //{
    //    public long TaskTeamMemberId { get; set; }
    //    public long TaskTeamId { get; set; }
    //    public long EmployeeId { get; set; }
    //}
    public class TaskAssigned
    {
        public long TaskAssignedId { get; set; }
        public long ProTaskId { get; set; }
        public long EmployeeId { get; set; }

        public string AssignBy { get; set; }
        public string Status { get; set; }
        public Status chkStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
    public class AssignTaskManner
    {
        [Key]
        public long TaskAssignTypeId { get; set; }
        public long ProTaskId { get; set; }
        public long? EmployeeId { get; set; }
        public long TaskMannerId { get; set; }
    }
    public class AssignTaskSupervisor
    {
        [Key]
        public long TaskAssignTypeId { get; set; }
        public long ProTaskId { get; set; }
        public long EmployeeId { get; set; }
    }
    public class TaskAssignType
    {
        public long TaskAssignTypeId { get; set; }
        public long ProTaskId { get; set; }
        public long TeamId { get; set; }
    }

    public class TaskImage
    {
        public long TaskImageId { get; set; }
        public long ProTaskId { get; set; }
        public long? TaskUpdationID { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public long? TaskRemarkId { get; set; }
        public string CreatedBy { get; set; }
        public string description { get; set; }
        public string newdescription { get; set; }

    }
    public class TaskStatus
    {
        [Key]
        public long TaskStatusId { get; set; }
        [Display(Name = "Status Name ")]
        public string StatusName { get; set; }

        public Status Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long? Branch { get; set; }
        public Branch CreatedBranch { get; set; }
    }
    public class TaskStatusDept
    {
        public long TaskStatusDeptId { get; set; }
        public long TaskStatusId { get; set; }
        public long DeptId { get; set; }
    }
    public class TaskStatusDesg
    {
        public long TaskStatusDesgId { get; set; }
        public long TaskStatusId { get; set; }
        public long DesgId { get; set; }
    }
    public class TaskRemark
    {
        public long TaskRemarkId { get; set; }
        public long TaskId { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }
        [Display(Name = "Task Status")]
        public long? TaskStatusID { get; set; }
        public long? TaskUpdationID { get; set; }
        public DateTime CreatedDate { get; set; }
        // long[] member list: EF Core 10 primitive-collection column absent from the snapshot; legacy EF6 kept it transient -> not mapped.
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public long[] AssignedMembers { get; set; }
        
       

    }



    public class RemarkChecklist
    {
        public long Id { get; set; }
        public long Remark { get; set; }
        public long? Checklistitemid { get; set; }
        [StringLength(250)]
        public string Note { get; set; }
        public bool Check { get; set; }
    }
    public class TaskMobile
    {
        public long TaskMobileId { get; set; }
        public long ProTaskId { get; set; }
        public string MobileNo { get; set; }
        public string Name { get; set; }
    }
    public class LeadRemarkChecklists
    {
        public long Id { get; set; }
        public long Remark { get; set; }
        public long? Checklistitemid { get; set; }
        [StringLength(250)]
        public string Note { get; set; }
        public bool Check { get; set; }
    }
    public class EmpAttDetails
    {
        [Key]
        public long empattdetailsid { get; set; }
        public long taskstatusid { get; set; }
        public string userid { get; set; }
        public DateTime starttime { get; set; }
        public long protaskid { get; set; }
        public long empattid { get; set; }

    }

    public class CustomerSatisfaction
    {
        [Key]
        public long Id { get; set; }
        public long ProTaskId { get; set; }       
        public string SatisfactionLevel { get; set; }
        public string Comments { get; set; }
        public string Signature { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
    public class TaskDocument
    {
        [Key]
        public long TaskDocumentId { get; set; }
        public long ProtaskID { get;set;}

        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }

        public string DoucumentType { get; set; }
        public DateTime Expiry { get; set; }
        public string Notes { get; set; }
        public long DocumentTypeID { get; set; }
    }
}