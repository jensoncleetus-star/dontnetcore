using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class FinalSettlement
    {
        public long FinalSettlementID { get; set; }
        public DateTime Date { get; set; }
        public long Employee { get; set; }
        public string Reason { get; set; }
        public DateTime? JoiningDate { get; set; }
        public DateTime? LastworkingDate { get; set; }
        public DateTime? LastDutyDate { get; set; }
        public int? DeductionDays { get; set; }
        public int? NoDaysWorked { get; set; }
        public int? NoDaysAbsent { get; set; }
        public int? TotalDays { get; set; }
        public int? GratuityDays { get; set; }
        public decimal? GratuityAmount { get; set; }
        //public DateTime? LastDutyDate { get; set; }
        public decimal? NetAmount { get; set; }
        public string Remarks { get; set; }
        public long? Designation { get; set; }
        public long? TypeofSettlement { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }

    public class PayheadFS
    {
        public long Id { get; set; }
        public long FinalSettlementId { get; set; }
        public long PayheadId { get; set; }
        public decimal? Amount { get; set; }
    }
}