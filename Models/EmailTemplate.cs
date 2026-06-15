using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class EmailTemplate
    {
        public long EmailTemplateID { get; set; }

        [Required]
        [StringLength(100)]
        public string Head { get; set; }

        [Required]
        [StringLength(150)]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "Email Body")]
        
        public string EmailBody { get; set; }
    }
    public class SMSTemplate
    {
        public long SMSTemplateID { get; set; }

        [Required]
        [StringLength(100)]
        public string Head { get; set; }

        [Required]
        [StringLength(150)]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "SMS Body")]
        
        public string SMSBody { get; set; }
    }
}
