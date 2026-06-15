using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class ProjectViewModel
    {
        public long? ProjectId { get; set; }
        [Required]
        [Display(Name = "Project Name")]
        public string ProjectName { get; set; }

        public long ProNo { get; set; }
        [Required]
        [Display(Name = "Project Code")]
        public string ProCode { get; set; }

        [Display(Name = "Lead/Pipeline/Customer")]
        public long Customer { get; set; }
        // Contact person for the current project in customer side
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        // Quotation approved date, project expected start date, end date
        [Display(Name = "Start Date")]
        public string ExStartDate { get; set; }
        [Display(Name = "End Date")]
        public string ExEndDate { get; set; }
        [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }
        [Display(Name = "Sales Reference")]
        public string SalesContact { get; set; }
        // project type -{New Work, Maintainence, etc}
        [Display(Name = "Project Type")]
        public long? ProType { get; set; }
        public ProjectType ProTypeId { get; set; }

        public string Location { get; set; }

        
        public string Details { get; set; }
        [Display(Name = "Short Note")]
        public string Note { get; set; }

        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }        

        [Phone]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Project Status")]
        public long? ProjectStatus { get; set; }
        //for view
        public string UserName { get; set; }
        public string TypeName { get; set; }
        public string CustomerName { get; set; }
        public string SalesPersonName { get; set; }

        public DateTime? newApDate { get; set; }
        public DateTime? newExStartDate { get; set; }
        public DateTime? newExEndDate { get; set; }

        [Display(Name = "Income Account")]
        public long IncomeAccount { get; set; }
        public string IncomeAccName { get; set; }

        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        public List<FieldMapping> FieldMapAll { get; set; }
    }
    public class ProjectDetailViewModel
    {
        public long? ProjectId { get; set; }

        public string ProjectName { get; set; }

        public long? ProNo { get; set; }


        public string ProCode { get; set; }
        public long? Customer { get; set; }

        public string ContactPerson { get; set; }

        public string ApDate { get; set; }

        public string ExStartDate { get; set; }
        public string ExEndDate { get; set; }

        public long? SalesPerson { get; set; }
        public string SalesContact { get; set; }

        public long? ProType { get; set; }
        public ProjectType ProTypeId { get; set; }

        // created type {based on qutation or direct}

        public string CreatedType { get; set; }
        public long? QuoteNo { get; set; }
        public Quotation Quote { get; set; }

        public string Location { get; set; }

        public string ProStatus { get; set; }

        
        public string Details { get; set; }
        public string ContactNumber { get; set; }

        public string Note { get; set; }
        public DateTime? CreatedDate { get; set; }


        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? ExStartTime { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? ExEndTime { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        //for view
        public string UserName { get; set; }
        public string TypeName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
        public string SalesPersonName { get; set; }

        public double? Disc { get; set; }

        public DateTime? newExStartDate { get; set; }
        public DateTime? newExEndDate { get; set; }


        public List<TaskImageViewModel> TaskImages { get; set; }
        public List<TaskUpdationViewModel> TaskUpd { get; set; }

        //customers
        public Customer Customers { get; set; }
        public string DC { get; set; }
        public decimal? OpenBalance { get; set; }
        public string custSalePerson { get; set; }
        //  public Contact Contact { get; set; }
        public Accounts Accounts { get; set; }
        public ProjectContactViewModel Contact { get; set; }

        //quotation
        // public Quotation Quotation { get; set; }
        public QuotationViewModel Quotation { get; set; }
        public string QuotSaleExec { get; set; }
        public string QuotCustomer { get; set; }
        public List<QuotItemViewModel> QuotItem { get; set; }

        public ProjectContactViewModel SContact { get; set; }
        public long? HireType { get; set; }
        public string CustType { get; set; }

        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        public ICollection<MobileViewModel> mob { get; set; }
    }
    public class TeamMembersViewModel
    {
        public int? id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Img { get; set; }
        public string Status { get; set; }
    }
    public class TaskUpdationViewModel
    {
        public string TaskStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Remarks { get; set; }
        public string TaskName { get; set; }
        public long? TaskId { get; set; }
        public string Assigned { get; set; }

        public long? TaskUpdId { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        // public DateTime? PCreatedDate { get; set; }
    }

    public class ProjectImageViewModel
    {
        public long? ProjectImageId { get; set; }
        public long? ProjectId { get; set; }
        public string FileName { get; set; }
        public string ProjectName { get; set; }
        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> proImage { get; set; }
    }
    public class TaskImageViewModel
    {
        public long? TaskImageId { get; set; }
        public long? TaskId { get; set; }
        public string FileName { get; set; }
        public string TaskName { get; set; }
        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> tskImage { get; set; }
    }
    public class ProjectContactViewModel
    {
        public string EmailId { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string Fax { get; set; }
        public string Reference { get; set; }
        public string ContactPerson { get; set; }

    }

    public class WorkLocationViewModel
    {
        public long? ProTaskId { get; set; }
        public string TaskName { get; set; }
        public long? Project { get; set; }
        public string ProjectName { get; set; }
        public string UserName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string TypeName { get; set; }
        public string AssignedTo { get; set; }
        public string TaskStatus { get; set; }
        public string Priority { get; set; }
        public List<EmployeeViewModel> TeamMembers { get; set; }
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
    }
    public class WorkLocationsViewModel
    {
        public List<WorkLocationViewModel> Work { get; set; }
    }
}