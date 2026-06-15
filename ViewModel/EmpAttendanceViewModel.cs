using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class EmpAttendanceViewModel
    {
       
        public long EmpattendanceId { get; set; }
        public long EmpattendancedetailsId { get; set; }
        public string userid { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? login { get; set; }
        public DateTime? logout { get; set; }
        public string Status { get; set; }
        public decimal Duration { get; set; }

        public long taskstatusid { get; set; }
        public long EmployeeId { get; set; }

        public DateTime starttime { get; set; }
        public long protaskid { get; set; }
        public long empattid { get; set; }
    }
}