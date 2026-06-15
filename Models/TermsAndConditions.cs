using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class TermsAndConditions
    {
        [Key]
        public long TermsConditionID { get; set; }

        public string ConditionTypeID { get; set; }

        [Required]
        [Display(Name = "Terms & Condition")]
        
        public string TermsCondit { get; set; }
    }
}