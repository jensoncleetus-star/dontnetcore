using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class Project
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; }
        public long ProNo { get; set; }
        public string ProCode { get; set; }
        public long Customer { get; set; }

        public string ContactPerson { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public long? SalesPerson { get; set; }
        public string SalesContact { get; set; }

        public long? ProType { get; set; }
        public ProjectType ProTypeId { get; set; }

        [StringLength(100)]
        public string Lattitude { get; set; }
        [StringLength(100)]
        public string Longitude { get; set; }
        public string Location { get; set; }

        
        public string Details { get; set; }
        public string Note { get; set; }

        [StringLength(15)]
        public string ContactNumber { get; set; }

        public long? ProjectStatus { get; set; }

        public Status Status { get; set; }
        public choice Editable { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public long IncomeAccount { get; set; }

        //Refernce Field Added
        [StringLength(50)]
        public string Ref1 { get; set; }
        [StringLength(50)]
        public string Ref2 { get; set; }
        [StringLength(50)]
        public string Ref3 { get; set; }
        [StringLength(50)]
        public string Ref4 { get; set; }
        [StringLength(50)]
        public string Ref5 { get; set; }
    }

    public class ProjectType
    {
        public long ProjectTypeID { get; set; }
        [Display(Name = "Name ")]
        public string TypeName { get; set; }
        public string Details { get; set; }
        public Status Status { get; set; }
        public choice Editable { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long? Branch { get; set; }
        public Branch CreatedBranch { get; set; }
    }
    public class ProjectImage
    {
        public long ProjectImageId { get; set; }
        public long ProjectId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class ProjectStatus
    {
        [Key]
        public long ProjectStatusId { get; set; }
        [Display(Name = "Status Name ")]
        public string StatusName { get; set; }

        public Status Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long? Branch { get; set; }
        public Branch CreatedBranch { get; set; }
    }
}