using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Amc
    {
        [Key]
        public long AmcId { get; set; }
        public long AmcNo { get; set; }
        public long ContractId { get; set; }
        public long CustomerId { get; set; }        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ReminderDate { get; set; }
        public long? ContractTypeId { get; set; }
        public int? ContractLevelId { get; set; }
        public long? LocationId { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }   
        public bool PeriodicMaintReqrd { get; set; }
        public long? AmcStatusId { get; set; }
        public string AmcDetails { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LogTime { get; set; }
        public int? OpenClose { get; set; }
    }

    public class AmcContract
    {
        [Key]
        public long ContractId { get; set; }

        [Display(Name = "Contract Name")]
        public string ContractName { get; set; }
    }
    public class AmcContractType
    {
        [Key]
        public long TypeId { get; set; }
        public string Type { get; set; }
    }
    public class AmcStatus
    {
        [Key]
        public long AmcStatusId { get; set; }
        public string StatusName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long? Branch { get; set; }
    }
    public class AmcStatusDept
    {
        public long AmcStatusDeptId { get; set; }
        public long AmcStatusId { get; set; }
        public long DeptId { get; set; }
    }
    public class AmcStatusDesg
    {
        public long AmcStatusDesgId { get; set; }
        public long AmcStatusId { get; set; }
        public long DesgId { get; set; }
    }
  
    public class AmcAssignedTeam
    {
        [Key]
        public long AssignedTeamId { set; get; }
        public long AmcId { set; get; }
        public long TeamId { set; get; }
    }

    public class AmcAssignedTo
    {
        [Key]
        public long AssignedToId { get; set; }
        public long AmcId { get; set; }
        public long EmployeeId { get; set; }
        public String AssignBy { get; set; }
        public String Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Status ChkStatus { get; set; }
        public bool Approve { get; set; }
    }   

    public class AmcDocument
    {
        [Key]
        public long DocumentId { get; set; }
        public long TransId { get; set; }
        public string TransType { get; set; }
        public long? DocumentTypeID { get; set; }  
        public DateTime? Expiry { get; set; }
        public string Notes { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AmcUpdation
    {
        [Key]
        public long UpdationID { get; set; }
        public long TransId { get; set; }
        public string TransType { get; set; }
       // public Amc Amc { get; set; }

        [Required]     
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Location { get; set; }
        [StringLength(100)]
        public string Lattitude { get; set; }
        [StringLength(100)]
        public string Longitude { get; set; }
        public string Remarks { get; set; }
    }

    public class AmcRemark
    {
        [Key]
        public long RemarkId { get; set; }
        public long TransId { get; set; }
        public string TransType { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }

        [Display(Name = "Amc Status")]
        public long? StatusID { get; set; }
        public long? UpdationID { get; set; }
        public DateTime CreatedDate { get; set; }
        // long[] member list: EF Core 10 maps this as a primitive-collection column absent from the snapshot;
        // legacy EF6 had no such mapping (kept transient, written to a junction table) -> not mapped.
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public long[] AssignedMembers { get; set; }
    }

    public class AmcProcessFlow
    {
        [Key]
        public long AmcProcessFlowId { get; set; }
        public long AmcStatus { get; set; }
        public bool RemoveUpdateUser { get; set; }
        public bool RemoveUpdateUserTeams { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public bool AssignExistingUser { get; set; }
    }

    public class AmcProcessFlowAssignUser
    {
        [Key]
        public long AmcProcessFlowAssignUserId { get; set; }
        public long AmcProcessFlowId { get; set; }
        public long EmployeeId { get; set; }
    }
    public class AmcProcessFlowAssignType
    {
        [Key]
        public long AmcProcessFlowAssignTypeId { get; set; }
        public long AmcProcessFlowId { get; set; }
        public long TeamId { get; set; }
    }

    public class PeriodicMaintenance
    {
        [Key]
        public long PeriodicMaintenanceId { get; set; }
        public long PeriodicMaintenanceNo { get; set; }
        public long AmcId { get; set; }
        public long? NoOfPMaintenance { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LogTime { get; set; }
    }

    public class PeriodicMaintenanceDetail
    {
        [Key]
        public long PeriodicMaintDetailsId { get; set; }
        public long PeriodicMaintenanceId { get; set; }
        public DateTime PDate { get; set; }
        public string Notes { get; set; }
        public long? PeriodicMaintStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LogTime { get; set; }
    }

    public class PeriodicMaintAssignedTeam
    {
        [Key]
        public long AssignedTeamId { set; get; }
        public long PeriodicMaintDtlId { set; get; }
        public long TeamId { set; get; }
    }

    public class PeriodicMaintAssignedTo
    {
        [Key]
        public long AssignedToId { get; set; }
        public long PeriodicMaintDtlId { get; set; }
        public long EmployeeId { get; set; }
        public String AssignBy { get; set; }
        public String Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Status ChkStatus { get; set; }
        public bool Approve { get; set; }
    }
    public class PeriodicProcessFlow
    {
        [Key]
        public long PeriodicProcessFlowId { get; set; }
        public long PeriodicStatus { get; set; }
        public bool RemoveUpdateUser { get; set; }
        public bool RemoveUpdateUserTeams { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public bool AssignExistingUser { get; set; }
    }
    public class PeriodicProcessFlowAssignUser
    {
        [Key]
        public long PerdcProcessFlowAssignUserId { get; set; }
        public long PerdcProcessFlowId { get; set; }
        public long EmployeeId { get; set; }
    }
    public class PeriodicProcessFlowAssignType
    {
        [Key]
        public long PerdcProcessFlowAssignTypeId { get; set; }
        public long PerdcProcessFlowId { get; set; }
        public long TeamId { get; set; }
    }

    //public class PeriodicUpdation
    //{
    //    [Key]
    //    public long PeriodicUpdationID { get; set; }
    //    public long PeriodicDetailId { get; set; }
    //    //public PeriodicMaintenance PeriodicMaintenance { get; set; }

    //    [Required]
    //    public string CreatedBy { get; set; }
    //    public DateTime? CreatedDate { get; set; }
    //    public string Location { get; set; }
    //    [StringLength(100)]
    //    public string Lattitude { get; set; }
    //    [StringLength(100)]
    //    public string Longitude { get; set; }
    //    public string Remarks { get; set; }
    //}

}