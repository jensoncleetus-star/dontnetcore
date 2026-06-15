using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class DailyAttendance
    {
        public long DailyAttendanceId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime MonthYear { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
    public class DailyAttendanceDetail
    {
        public long DailyAttendanceDetailId { get; set; }
        public long DailyAttendanceId { get; set; }
        public long EmployeeId { get; set; }

        public DateTime AtDate { get; set; }
        public long AtType { get; set; }
        public decimal Overtime { get; set; }
    }
}