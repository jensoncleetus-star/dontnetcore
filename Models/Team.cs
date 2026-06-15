using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Team
    {
        public long TeamId { get; set; }
        public long TeamLead { get; set; }

        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }

        public string TeamName { get; set; }
        public string TeamTag { get; set; }

    }
    public class TeamMember
    {
        public long TeamMemberId { get; set; }
        public long TeamId { get; set; }
        public long EmployeeId { get; set; }
    }
    public class TeamTaskStatus
    {
        public long TeamTaskStatusId { get; set; }
        public long TeamId { get; set; }
        public long TaskStatusId { get; set; }
    }
    public class LeadTaskStatus
    { [Key]
        public long TeamTaskStatusId { get; set; }
        public long TeamId { get; set; }
        public long TaskStatusId { get; set; }
    }
    public class TeamAmcStatus
    {
        [Key]
        public long TeamamcStatusId { get; set; }
        public long TeamId { get; set; }
        public long amcStatusId { get; set; }
    }
    public class ProcessFlow
    {
        public long ProcessFlowId { get; set; }

        public long TaskStatus { get; set; }

        public bool RemoveUpdateUser { get; set; }

        public bool RemoveUpdateUserTeams { get; set; }

        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public bool MoveToLead { get; set; }
       
        public long LeadStatus { get; set; }

        public bool assignexistinguser { get; set; }
       

    }
    public class ProcessFlowAssignUser
    {
        public long ProcessFlowAssignUserId { get; set; }
        public long ProcessFlowId { get; set; }
        public long EmployeeId { get; set; }
    }
    public class ProcessFlowAssignUserstolead
    {
        [Key]
        public long ProcessFlowAssignUserId { get; set; }
        public long ProcessFlowId { get; set; }
        public long EmployeeId { get; set; }
    }
    public class ProcessFlowAssignType
    {
        public long ProcessFlowAssignTypeId { get; set; }
        public long ProcessFlowId { get; set; }
        public long TeamId { get; set; }
    }

    public class LeadProcessFlow
    {
        public long LeadProcessFlowId { get; set; }

        public long LeadStatus { get; set; }

        public bool RemoveUpdateUser { get; set; }

        public bool RemoveUpdateUserTeams { get; set; }

        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public bool movetofieldservice { get; set; }
        public long? taskid { get; set; }
        public bool approvalreq { get; set; }

    }

    public class LeadProcessFlowAssignUser
    {
        public long LeadProcessFlowAssignUserId { get; set; }
        public long LeadProcessFlowId { get; set; }
        public long EmployeeId { get; set; }
    }
    public class LeadProcessFlowAssignType
    {
        public long LeadProcessFlowAssignTypeId { get; set; }
        public long LeadProcessFlowId { get; set; }
        public long TeamId { get; set; }
    }
    public class LeadApprovals
    {
        [Key]
        public long LeadApprovalId { get; set; }
        public long LeadProcessFlowId { get; set; }
        public long LeadEmployeeId { get; set; }
        public long LeadTaskStatus { get; set; }
    }
}