using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class City
    {
        public long Id { get; set; }

        public string CityName { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public Branch Branch { get; set; }

        public Status Status { get; set; }
    }
}