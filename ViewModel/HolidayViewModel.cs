using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class HolidayViewModel
    {
        public long HolidayID { get; set; }
        [Required]
        public string HolidayName { get; set; }
        [Required]
        [Display(Name = "Calendar Template")]
        public long CalendarTemplateID { get; set; }
        [Display(Name = "From Date")]
        public string FromDate { get; set; }
        [Display(Name = "To Date")]
        public string ToDate { get; set; }
    }
}