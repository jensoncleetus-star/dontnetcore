using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PayrollUnit
    {
        public long Id { get; set; }

        public string UnitName { get; set; }

        public string Symbol { get; set; }
        public string Type { get; set; }
        public decimal? Convertion { get; set; }
    }
}