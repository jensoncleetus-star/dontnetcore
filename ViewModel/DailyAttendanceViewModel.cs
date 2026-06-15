using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class DailyAttendanceViewModel
    {
        public long DailyAttendanceId { get; set; }
        public long EmployeeId { get; set; }
        //public DateTime MonthYear { get; set; }
        public string MonthYear { get; set; }
        public string ddlType { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }

        public string OTHr { get; set; }

        public string AtDate { get; set; }


        public ICollection<DailyAttendanceDetailViewModel> addattendance { get; set; }
        public ICollection<DailyAttendanceDetailViewModel> addattendanced { get; set; }
        public List<AttendanceType> AtType { get; set; }
    }
    public class DailyAttendanceDetailViewModel
    {
        public long DailyAttendanceId { get; set; }
        public long EmployeeId { get; set; }

        public string AtDate { get; set; }
        public long AtType { get; set; }
        public decimal Overtime { get; set; }
    }



    public class DailyAttendanceViewModel2
    {
        public long DailyAttendanceId { get; set; }
    
       
        public string ddlType { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }

        public string OTHr { get; set; }

        public string AtDate { get; set; }


        public ICollection<DailyAttendanceDetailViewModel2> addattendance { get; set; }
    
      
    }
    public class DailyAttendanceDetailViewModel2
    {
        public long DailyAttendanceId { get; set; }
        public long EmployeeId { get; set; }

        public string AtDate { get; set; }
        public long AtType { get; set; }
        public DateTime starttime { get; set; }
        public DateTime endtime { get; set; }
        public decimal Overtime { get; set; }
    }
}