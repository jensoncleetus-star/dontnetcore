using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class WorkShiftViewModel
    {
        public long WorkShiftId { get; set; }

        [Required]
        [Display(Name = "Work Shift Name")]
        public string WorkShiftName { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Late Count Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? LateCountTime { get; set; }
    }
}