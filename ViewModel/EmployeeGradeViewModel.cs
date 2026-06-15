using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class EmployeeGradeViewModel
    {
        public long? EmployeeGradeId { get; set; }

        [Display(Name = "Grade Name")]
        public string GradeName { get; set; }

        public string Note { get; set; }

        public ICollection<SalaryStrDetailsViewModel> salarystr { get; set; }
    }
}