using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class SalaryStructuresViewModel
    {
        public long SalaryStrId { get; set; }
        [Required]
        [Display(Name = "Employee Name")]
        public long EmployeeId { get; set; }

        [Required]
        [Display(Name = "Effective From")]
        public string EFDate { get; set; }

        public long Branch { get; set; }

        public ICollection<SalaryStrDetailsViewModel> salarystr { get; set; }
    }
    public class SalaryStrDetailsViewModel
    {
        public long PayHeadId { get; set; }
        public decimal? Rate { get; set; }
        public string EffectFrom { get; set; }
    }
}