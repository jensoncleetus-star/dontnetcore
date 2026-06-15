using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Approval
    {
        public long ApprovalID { get; set; }
        public long TransEntry { get; set; }
        [StringLength(50)]
        public string Type { get; set; }
        public long EmployeeId { get; set; }
    }
    public class ApprovalUpdate
    {
        public long ApprovalUpdateID { get; set; }
        public long TransEntry { get; set; }
        [StringLength(50)]
        public string Type { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string RequestBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Status Status { get; set; }
        public string Note { get; set; }
    }
    public class ApprovalUpdatestwp
    {
        [Key]
        public long ApprovalUpdateID { get; set; }
        public long TransEntry { get; set; }
        [StringLength(50)]
        public string Type { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public string RequestBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Status Status { get; set; }
        public string Note { get; set; }
    }
}