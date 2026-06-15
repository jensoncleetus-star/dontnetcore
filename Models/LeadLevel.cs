using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class LeadLevel
    {
        [Key]
        public long LevelId { get; set; }
        [Required]
       
        [Display(Name = "Level")]
        public string Level { get; set; }

    }
    public class ChequeStatus
    {
        [Key]
        public long chequestatusid { get; set; }
        [Required]

        [Display(Name = "Status")]
        public string ChequeStatusName { get; set; }

    }
}