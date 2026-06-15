using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Tax
    {
        public long TaxID { get; set; }

        [Required]
        [StringLength(100)]
        public string TaxName { get; set; }

        [StringLength(20)]
        public string TaxType { get; set; }

        [Required]
        public decimal? Percentage { get; set; }

        [DefaultValue(true)]
        public Status Status { get; set; }

    }
    public class TaxGroup
    {
        public long TaxGroupID { get; set; }

        public long parent { get; set; }

        public long child { get; set; }

    }
}