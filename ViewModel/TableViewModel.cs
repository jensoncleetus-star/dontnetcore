using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class TableViewModel
    {
        [Required]
        [Display(Name = "Table Name")]
        public string TableName { get; set; }
        [Required]
        [Display(Name = "Area")]
        public long? AreaId { get; set; }
        [Required]
        [Display(Name = "Maximum Seats")]
        public int? MaxSeats { get; set; }
        [Display(Name = "Table Status")]
        public TableStatus TableStatus { get; set; }
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public Status Status { get; set; }

        public IEnumerable<Area> Areas { get; set; }
    }
}