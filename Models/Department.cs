using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Department
    {
        public long DepartmentID { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Department")]
        public string DepartmentName { get; set; }
        [Display(Name = "Calendar Template")]
        public long? CalendarTemplateId { get; set; }

    }
}