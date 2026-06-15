using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Designation
    {
        public long DesignationID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Designation ")]
        public string DesignationName { get; set; }
        [Display(Name = "Department  ")]
        public long? department { get; set; }
    }
}