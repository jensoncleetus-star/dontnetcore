using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class AttendanceViewModel
    {
        public long AttendanceId { get; set; }
        //public long AtNo { get; set; }
        [Required]
        public string VoucherNo { get; set; }

        [Display(Name = "Date")]
        [Required]
        public string AtDate { get; set; }

        public string Note { get; set; }
        public string Remarks { get; set; }
        public long? Branch { get; set; }
        public ICollection<AttendanceDetailVModel> empitems { get; set; }
    }
    public class AttendanceDetailVModel
    {
        public long AttendanceDetailId { get; set; }
        public long AttendanceId { get; set; }

        public long EmployeeId { get; set; }
        public long AttendanceType { get; set; }
        public Int32 Value { get; set; }
        public string Unit { get; set; }

        public string EmpName { get; set; }
        public string EmpCode { get; set; }
        public string ATypeName { get; set; }
    }
    public class AttendanceAutoFillViewModel
    {
        [Required]
        public long[] Employees { get; set; }
    }
}