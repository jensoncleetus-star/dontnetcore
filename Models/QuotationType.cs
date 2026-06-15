using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;


namespace QuickSoft.Models
{
    public class QuotationType
    {
       [Key]
        public long QuotId { get; set; }
        public string QuotType { get; set; }

    }
    public class UserEditDays
    {
        [Key]
        public long id { get; set; }
        public string userid { get; set; }
        public int days { get; set; }
        public int srdays { get; set; }
        public decimal pedays { get; set; }
        public int prdays { get; set; }
        public int stkdays { get; set; }
        public int pedate { get; set; }
        public int seitem { get; set; }

    }
    
}