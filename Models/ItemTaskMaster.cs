using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
namespace QuickSoft.Models
{
    public class ItemTaskMaster
    {
       [Key]
        public long TaskMasterId { get; set; }
        public string CreatedBy { get; set; }
       
        public DateTime? TaskDate { get; set; }
        public string TaskName { get; set; }
        public long TaskId { get; set; }
        public long McId { get; set; }
       
    }
}