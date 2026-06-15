using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class Country
    {
        [Key]
        public int CountryID { get; set; }

        public string CountryCode { get; set; }

        public string CountryName { get; set; }
    }
}