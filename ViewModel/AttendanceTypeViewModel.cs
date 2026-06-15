using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class AttendanceTypeViewModel
    {
        public long? id { get; set; }
        public string Name { get; set; }

        public string Group { get; set; }
        public string Type { get; set; }
        public string PeriodType { get; set; }
        public long? Unit { get; set; }
    }
}