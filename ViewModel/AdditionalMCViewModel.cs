using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class AdditionalMCViewModel
    {              

        [Display(Name = "Employee Members")]
        public long[] EmployeeMembers { get; set; }

        public long[] Employees { get; set; }

        public long McId { get; set; }

        public string McName { get; set; }      
    }
    public class TaskGroupViewModel
    {

        [Display(Name = "Task Status")]
        public long[] TaskStatus { get; set; }

        
        public long TaskType { get; set; }

        public string TaskTypeName { get; set; }
    }

}