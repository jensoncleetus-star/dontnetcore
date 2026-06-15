using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class FinalSettlementViewModel
    {
        public long FinalSettlementID { get; set; }
        public string Date { get; set; }
        public long Employee { get; set; }
        public string Reason { get; set; }
        public string JoiningDate { get; set; }
        public string LastworkingDate { get; set; }

        public int? DeductionDays { get; set; }
        public int? NoDaysWorked { get; set; }
        public int? NoDaysAbsent { get; set; }
        public int? TotalDays { get; set; }
        public int? GratuityDays { get; set; }
        public decimal? GratuityAmount { get; set; }
        public string LastDutyDate { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? Addition { get; set; }
        public decimal? NetAmount { get; set; }
        public string Remarks { get; set; }
        public string Designation { get; set; }
        public string TypeofSettlement { get; set; }
        public decimal? Basic { get; set; }
        public JobStatus JobStatus { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
        public ICollection<AdditionFinalViewModel> Additiondetails { get; set; }
        public ICollection<DeductionFinalViewModel> Deductiondetails { get; set; }

    }
    public class FinalSettlementGetViewModel
    {
        public long FinalSettlementID { get; set; }
        public DateTime? Date { get; set; }
        public long Employee { get; set; }
        public string Reason { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? LastworkingDate { get; set; }

        public int? DeductionDays { get; set; }
        public int? NoDaysWorked { get; set; }
        public int? NoDaysAbsent { get; set; }
        public int? TotalDays { get; set; }
        public int? GratuityDays { get; set; }
        public decimal? GratuityAmount { get; set; }
        public DateTime? LastDutyDate { get; set; }
        public decimal? Deduction { get; set; }
        public decimal? Addition { get; set; }
        public decimal? NetAmount { get; set; }
        public string Remarks { get; set; }
        public string Designation { get; set; }
        public string TypeofSettlement { get; set; }
        public decimal? Basic { get; set; }
        public JobStatus JobStatus { get; set; }
        public DateTime? LastDutyReumeDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
        public ICollection<AdditionFinalViewModel> Additiondetails { get; set; }
        public ICollection<DeductionFinalViewModel> Deductiondetails { get; set; }

    }
    public class AdditionFinalViewModel
    {
        public long Id { get; set; }
        public string Payhead { get; set; }
        public decimal? Amount { get; set; }
    }
    public class DeductionFinalViewModel
    {
        public long Id { get; set; }
        public string Payhead { get; set; }
        public decimal? Amount { get; set; }
    }
}