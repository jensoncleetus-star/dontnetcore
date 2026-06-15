using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Reminder
    {
        public long ReminderId { get; set; }
        public long Reference { get; set; }
        [StringLength(50)]
        public string Type { get; set; }

        public string Note { get; set; }
        [Display(Name = "Date")]
        public DateTime? RDate { get; set; }
        //[Display(Name = "Time")]
        //public DateTime? RTime { get; set; }

        public string RequestBy { get; set; }
        public string CreatedBy { get; set; }

        //open or close or postpond
        [Display(Name = "Status ")]
        public string RStatus { get; set; }

        public DateTime CreatedDate { get; set; }
        public Status Status { get; set; }

    }
    public class ReminderAssigned
    {
        public long ReminderAssignedID { get; set; }
        public long ReminderId { get; set; }
        public long EntryId { get; set; }
        [StringLength(50)]
        public string Type { get; set; }
        public long EmployeeId { get; set; }
    }
    public class snooze
    {
        [Key]
        public long snoozeId { get; set; }
        public DateTime createddate { get; set; }
        public long reminderassignedid { get; set; }
    }
    public class Reminderss
    {
       [Key]
        public long ReminderId { get; set; }
        public long Reference { get; set; }
        [StringLength(50)]
        public string Type { get; set; }

        public string Note { get; set; }
        [Display(Name = "Date")]
        public DateTime? RDate { get; set; }
        //[Display(Name = "Time")]
        //public DateTime? RTime { get; set; }

        public string RequestBy { get; set; }
        public string CreatedBy { get; set; }

        //open or close or postpond
        [Display(Name = "Status ")]
        public string RStatus { get; set; }

        public DateTime CreatedDate { get; set; }
        public Status Status { get; set; }
        public string actionurl { get; set; }

    }
    public class ReminderAssignedss
    {
        [Key]
        public long ReminderAssignedID { get; set; }
        public long ReminderId { get; set; }
        public long EntryId { get; set; }
        [StringLength(50)]
        public string Type { get; set; }
        public long EmployeeId { get; set; }
    }
}