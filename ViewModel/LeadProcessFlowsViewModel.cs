using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class LeadProcessFlowsViewModel
    {
        public long? LeadProcessFlowId { get; set; }

        [Display(Name = "Lead Status")]
        public long LeadStatus { get; set; }

        [Display(Name = "Assigned Team")]
        public long? AssignType { get; set; }

        [Display(Name = "Assigned Users")]
        public long[] AssignedUsers { get; set; }

        [Display(Name = "Remove Updating User")]
        public bool RemoveUpdateUser { get; set; }

        [Display(Name = "Approval Requierd")]
        public bool ApprovalRequierd { get; set; }

        [Display(Name = "Assign Users")]
        public long[] AssignUsers { get; set; }

        [Display(Name = "Task Status")]
        public long? TaskStatus { get; set; }

        [Display(Name = "Remove Updating User Team Members")]
        public bool RemoveUpdateUserTeams { get; set; }

        public long[] AssignTypeAll { get; set; }
        [Display(Name ="Move To Fied Service")]
        public bool MoveToFieldService { get; set; }
        
    }
}