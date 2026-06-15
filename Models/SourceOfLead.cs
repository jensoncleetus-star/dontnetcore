using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class SourceOfLead
    {
        public long SourceOfLeadId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Source Of Lead")]
        public string SrcName { get; set; }
    }
    public class LeadRemark
    {
        public long LeadRemarkId { get; set; }
        public long CustomerID { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }
        public long? Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class AssignedToLog
    {
        public long AssignedToLogID { get; set; }
        public long CustomerID { get; set; }
        public long EmployeeId { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime AssignedDate { get; set; }
        public string AddedUser { get; set; }
    }
    public class LeadStatus
    {
        public long LeadStatusID { get; set; }
        [Display(Name = "LeadStatus ")]
        public string StatusType { get; set; }
        public string Details { get; set; }
        public Status Status { get; set; }
        public choice Editable { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long? Branch { get; set; }
        public Branch CreatedBranch { get; set; }
    }

    public class LeadDocument
    {
        public long LeadDocumentId { get; set; }
        public long CustomerID { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DoucumentType { get; set; }
        public DateTime Expiry { get; set; }
        public string Notes { get; set; }

        public long DocumentTypeID { get; set; }
    }
    
}