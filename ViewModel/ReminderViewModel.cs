using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class ReminderViewModel
    {
        public long ReminderId { get; set; }
        public long Reference { get; set; }
        [StringLength(50)]
        public string Type { get; set; }
        public string Note { get; set; }
        [Display(Name = "Date")]
        public string RDate { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public virtual DateTime RTime { get; set; }
        [Display(Name = "Status ")]
        public string RStatus { get; set; }

        public string RequestBy { get; set; }
        public string CreatedBy { get; set; }

        public long[] AssignedTo { get; set; }
    }
}