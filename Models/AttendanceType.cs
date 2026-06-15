using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class AttendanceType
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Group { get; set; }
        public string Type { get; set; }
        public string PeriodType { get; set; }
        public long? Unit { get; set; }
        public Status Status { get; set; }
    }
    public class Attendance
    {
        public long AttendanceId { get; set; }
        public long AtNo { get; set; }
        [Required]
        public string VoucherNo { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime AtDate { get; set; }

        public string Note { get; set; }
        public string Remarks { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
    public class AttendanceDetail
    {
        public long AttendanceDetailId { get; set; }
        public long AttendanceId { get; set; }

        public long EmployeeId { get; set; }
        public long AttendanceType { get; set; }
        public Int32 Value { get; set; }
        public string Unit { get; set; }
    }


  }