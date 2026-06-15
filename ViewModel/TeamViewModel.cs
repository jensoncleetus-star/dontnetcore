using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class TeamViewModel
    {
        [Display(Name = "Team Members")]
        public long[] TeamMembers { get; set; }

        [Display(Name = "Team Lead")]
        [Required]
        public long TeamLead { get; set; }
        [Display(Name = "Team Name")]
        [Required]
        public string TeamName { get; set; }
        [Display(Name = "Team Tag")]
        public string TeamTag { get; set; }

        [Display(Name = "Task Status")]
        public long[] TaskStatus { get; set; }
        [Display(Name = "Lead Status")]
        public long[] LeadStatus { get; set; }
        [Display(Name = "Amc Status")]
        public long[] amcstatus { get; set; }
        public bool CustomerRelation { get; set; }
        public bool amcteam { get; set; }
        public long Branch { get; set; }

    }
    public class CustomerMergeViewModel
    {
        [Display(Name = "Old Customer")]
        public long[] OldCurstomerIds { get; set; }

        [Display(Name = "New Customer")]
        [Required]
        public long CustomerId { get; set; }
       

    }

}