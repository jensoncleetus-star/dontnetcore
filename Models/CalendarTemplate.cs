using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class CalendarTemplate
    {
        public long CalendarTemplateID { get; set; }
        public string TemplateName { get; set; }
        public bool DefaultValue { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
    public class WeeklyHoliday
    {
        public long WeeklyHolidayID { get; set; }
        public long TemplateID { get; set; }
        public string SelDay { get; set; }
    }
    public class Holiday {
        public long HolidayID { get; set; }
        public string HolidayName { get; set; }
        public long CalendarTemplateID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

}