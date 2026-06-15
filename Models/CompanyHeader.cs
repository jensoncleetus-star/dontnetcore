using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class CompanyHeader
    {
        public long CompanyHeaderID { get; set; }

        
        [StringLength(100)]
        public string Name { get; set; }
        
        public string Header { get; set; }      
        public string Footer { get; set; }
    }
}