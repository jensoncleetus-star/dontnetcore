using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class EnableSetting
    {
        public long EnableSettingId { get; set; }

        public string EnableType { get; set; }

        public Status Status { get; set; }
        [StringLength(20)]
        public string TypeValue { get; set; }

    }
    public class OptionalFields{
        public long id { get; set; }
        public string section { get; set; }
        public string FieldName { get; set; }
        public string slug { get; set; }
        public bool Print { get; set; }
        public int count { get; set; }
    }
}