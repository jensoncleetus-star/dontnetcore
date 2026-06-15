using System;
using System.Collections.Generic;

using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class LeadConditions
    {

        public int id { get; set; }

        public string LeadCondition { get; set; }
    }
    public class leadconditionsview
    {
        public long leadid { get; set; }
        public string LeadCondition { get; set; }
    }
}