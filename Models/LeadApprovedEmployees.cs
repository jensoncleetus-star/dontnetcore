using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class LeadApprovedEmployees
    {
        [Key]
        public long ID { get; set; }

        public long LeadID { get; set; }

        public long EmployeeID { get; set; }

        public string CreatedUser { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Status { get; set; }
    }
}