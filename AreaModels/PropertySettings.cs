using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PropertySettings
    {
        public long Id { get; set; }
        [StringLength(20)]
        public string Module { get; set; }

        public string Type { get; set; }

        public long? LValue { get; set; }

        public string SValue { get; set; }

        public string Description { get; set; }

        public Status Status { get; set; }
    }
}