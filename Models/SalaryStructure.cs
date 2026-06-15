using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class SalaryStructure
    {
        [Key]
        public long SalaryStrId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime EFDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
    public class SalaryStrDetail
    {
        public long SalaryStrDetailId { get; set; }

        public long SalaryStrId { get; set; }

        public long PayHeadId { get; set; }

        public decimal? Rate { get; set; }

    }
}