using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;

namespace QuickSoft.ViewModel
{
    public class PayrollUnitViewModel
    {
        public string UnitName { get; set; }

        public string Symbol { get; set; }
        public string Type { get; set; }

        public long? first { get; set; }
        public long? second { get; set; }
        public decimal? convertion { get; set; }
        public long? Id { get; set; }
        public IEnumerable<PayrollUnit> PayrollUnitz { get; set; }
    }
}