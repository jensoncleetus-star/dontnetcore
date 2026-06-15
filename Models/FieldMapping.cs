using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class FieldMapping
    {
        public int FieldMappingId { get; set; }
        [StringLength(20)]
        public string Section { get; set; }
        [StringLength(50)]
        public string Field { get; set; }
        [StringLength(20)]
        public string FieldName { get; set; }
        public FMPrint Print { get; set; }
        public Status Status { get; set; }
        [StringLength(20)]
        public string Type { get; set; }
    }
    public class FieldMappingLock
    {
        [Key]
        public int FieldMappingId { get; set; }
        [StringLength(20)]
        public string Section { get; set; }
       
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
       
    }
    public class DocExpiryReminder
    {
        [Key]
        public long DocExpiryReminderId { get; set; }
        [StringLength(20)]
        public string Section { get; set; }
        public long EmployeeID { get; set; }
        public long days { get; set; }
      

    }

}