using System;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuickSoft.Models
{
    public class CustomerTyp

    {
       [Key] 
        public long TypeId { get; set; }
        
        public string Type { get; set; }
    }
}