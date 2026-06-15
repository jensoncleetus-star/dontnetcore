using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class LeaveSettlementViewModel
    {
        public long LeaveSettlementID { get; set; }
        [Required]
        public string Date { get; set; }
        [Required]
        public long Employee { get; set; }
        [Display(Name = "Leave Start Date")]
        public string LeaveStartDate { get; set; }
        [Display(Name = "Expected Resume Date")]
        public string ExpectedResumeDate { get; set; }
        public string Remarks { get; set; }
        public long? Branch { get; set; }

        public List<Payhead> FieldPayHead { get; set; }


        public DateTime? JoiningDate { get; set; }
        public string LastDutyResumeDate { get; set; }

        public int? TotalWorkingDays { get; set; }
        public int? DaysWorked { get; set; }
        public decimal? LeaveEntitled { get; set; }
        public decimal? LeaveSalary { get; set; }

        public decimal? BasicSalary { get; set; }

        public ICollection<SalaryStrDetailsViewModel> payheaddetail { get; set; }

        public ICollection<AdditionFinalViewModel> Additiondetails { get; set; }
        public ICollection<DeductionFinalViewModel> Deductiondetails { get; set; }
        public decimal? NetAmount { get; set; }
    }
    public class UpdateResumeDateViewModel
    {
        public long LeaveSettlementID { get; set; }
        public string DutyResumeDate { get; set; }
    }
}