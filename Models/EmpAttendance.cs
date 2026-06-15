using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class EmpAttendance
    {
        public long Id { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? login { get; set; }
        public DateTime? logout { get; set; }
        public string Status { get; set; }
        //public decimal Duration { get; set; }
        public string latitude { get; set; }
        public string logitude { get; set; }
        public string endlatitude { get; set; }
        public string endlogitude { get; set; }
        public int? leavestatus { get; set; }
        public long? protaskid { get; set; }

    }
}