using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;
using System.ComponentModel;

namespace QuickSoft.ViewModel
{
    public class AMCProcessFlowViewModel
    {
        public long? AmcProcessFlowId { get; set; }
        public long? PeriodicProcessFlowId { get; set; }

        [Display(Name = "Amc Status")]
        public long AmcStatus { get; set; }

        [Display(Name = "Periodic Status")]
        public long PeriodicStatus { get; set; }


        [Display(Name = "Assigned Team")]
        public long? AssignType { get; set; }

        [Display(Name = "Assigned Users")]
        public long[] AssignedUsers { get; set; }

        [Display(Name = "Remove Updating User")]
        public bool RemoveUpdateUser { get; set; }

        [Display(Name = "Remove Updating User Team Members")]
        public bool RemoveUpdateUserTeams { get; set; }
        public long[] AssignTypeAll { get; set; }
    }
}