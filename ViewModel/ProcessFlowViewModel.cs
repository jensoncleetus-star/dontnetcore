using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class ProcessFlowViewModel
    {
        public long? ProcessFlowId { get; set; }

        [Display(Name = "Task Status")]
        public long TaskStatus { get; set; }

        [Display(Name = "Assigned Team")]
        public long? AssignType { get; set; }

        [Display(Name = "Assigned Users")]
        public long[] AssignedUsers { get; set; }

        [Display(Name = "Remove Updating User")]
        public bool RemoveUpdateUser { get; set; }

        [Display(Name = "Remove Updating User Team Members")]
        public bool RemoveUpdateUserTeams { get; set; }

        public long[] AssignTypeAll { get; set; }
        [Display(Name ="Move To Lead")]
        public bool MoveToLead { get; set; }
        [Display(Name = "Lead Status")]
        public long? LeadStatus { get; set; }
        [Display(Name = "Assign Lead Previus Assign users")]
        public bool assignexistinguser { get; set; }
        public long[] leadAssignUsers { get; set; }
        [Display(Name ="Task Group")]
        public long? TaskGroup { get; set; }
    }
}