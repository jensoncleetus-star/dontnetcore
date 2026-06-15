using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class CalendarTemplateViewModel
    {
        public long CalendarTemplateID { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string TemplateName { get; set; }
       
        public bool DefaultValue { get; set; }

        [Display(Name = "Weekly Holidays")]
        public string[] WeeklyHoliday { get; set; }


    }
}