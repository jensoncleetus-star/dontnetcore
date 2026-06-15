using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class States
    {
        [Key]
        public int StateID { set; get; }

        public string StateCode { set; get; }

        public string StateName { set; get; }

        public int CountryID { set; get; }
    }
}