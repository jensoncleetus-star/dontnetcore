using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class LeaveSettlement
    {
        public long LeaveSettlementID { get; set; }
        public DateTime Date { get; set; }
        public long Employee { get; set; }
        public DateTime? LeaveStartDate { get; set; }
        public DateTime? ExpectedResumeDate { get; set; }
        public string Remarks { get; set; }

        public int? TotalWorkingDays { get; set; }
        public int? NoDaysWorked { get; set; }
        public decimal? LeaveEntitled { get; set; }
        public decimal? LeaveSalary { get; set; }
        public decimal? Netamount { get; set; }
        public DateTime? DutyResumeDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
    public class LeaveSettlementPayHead
    {
        public long LeaveSettlementPayHeadID { get; set; }
        public long LeaveSettlementID { get; set; }

        public long PayHeadID { get; set; }
        public decimal? PayHeadAmt { get; set; }

    }

}