using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class points
    {
        public string pointname { get; set; }
        public string lat { get; set; }
        public string log { get; set; }
        public long customerid { get; set; }
        public long? taskid { get; set; }
        public long? amcid { get; set; }
        public string type { get; set; }
        public DateTime? logtime { get; set; }
    }
    public class sopViewModel
    {
        public long? id { get; set; }
   public string title { get; set; }
        
        public string note { get; set; }
        public long[] AssignedMembers { get; set; }
    }
    public class servicetypeViewModel
    {
        public long? id { get; set; }
        public string title { get; set; }
        
        public string note { get; set; }
   
    }



    public class ProTaskViewModel
    {
        public long? ProTaskId { get; set; }

        [Required]
        [Display(Name = "Task Name")]
        public string TaskName { get; set; }
        [Required]
        [Display(Name = "Task Code")]
        public string TaskCode { get; set; }

        [Required]
        [Display(Name = "Project")]
        public long? ProjectId { get; set; }

        [Display(Name = "Task Type")]
        public long? TaskType { get; set; }
        public ProTaskType TaskTypeId { get; set; }

        [Display(Name = "Task Details")]
        
        public string TaskDetails { get; set; }
        [Display(Name = "Remark/Notes/Google Map Link")]
        
        public string Note { get; set; }

        [Display(Name = "Appointment Date")]
        public string StartDate { get; set; }
        [Display(Name = " Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? StartTime { get; set; }
        [Display(Name = "End Date")]
        public string EndDate { get; set; }
        [Display(Name = "End Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }
        [Display(Name = "Team Head")]
        public long? driver { get; set; }

        [Display(Name = "Assigned To")]
        public long? AssignedTo { get; set; }
        public long[] AssignedMembers { get; set; }
        public TaskPriority Priority { get; set; }
        [Display(Name = "Task Status")]
        public long? TaskStatus { get; set; }

        [Display(Name ="Vehicle Type")]
        public long? VehicleType { get; set; }

        [Display(Name = "Vehicle Manufacturer")]
        public long? VehicleManufacturer { get; set; }

        [Display(Name = "Vehicle Model")]
        public long? VehicleModel { get; set; }

        //[Display(Name = "Task Status")]
        //public TaskStatus TaskStatus { get; set; }

        //[Display(Name = "Team Members")]
        //public long[] TeamMembers { get; set; }
        //public TKUpdateStatus? upStatus { get; set; }
        public string Location { get; set; }

        
        [Display(Name = "Customer")]
        public long? CustomerID { get; set; }

        [Display(Name = "Assign Team")]
        public string AssignType { get; set; }

        public long? SalesPerson { get; set; }
        public long? SalesExecutive { get; set; }
        public long[] TaskManner { get; set; }
        public long[] SuperWiser { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        public List<FieldMapping> FieldMapAll { get; set; }
        public List<Contact> LstContacts { get; set; }

        public long[] AssignTypeAll { get; set; }

        //reminder
        [Display(Name = "Date")]
        public string RDate { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public virtual DateTime RTime { get; set; }
        [Display(Name = "Note")]
        public string RemNote { get; set; }
        public long[] RemAssignedTo { get; set; }

        // public string[] TaskMobiles { get; set; }
        public string Redirection { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

        public string To { get; set; }
        public string From { get; set; }
        [Display(Name = "Lattitude")]
        public string Lattitude { get; set; }
        [Display(Name = "Longitude")]
        public string Longitude { get; set; }
        public int? OpenClose { get; set; }
    }

    public class ServiceReportViewModel
    {
        public long ProTaskId { get; set; }

        [Required]
        [Display(Name = "Task Name")]
        public string TaskName { get; set; }
        [Required]
        [Display(Name = "Task Code")]
        public string TaskCode { get; set; }

        [Required]
        [Display(Name = "Project")]
        public long? ProjectId { get; set; }
        public long? servicereportid { get; set; }
        [Display(Name = "Task Type")]
        public long? TaskType { get; set; }
        public ProTaskType TaskTypeId { get; set; }

        [Display(Name = "Task Details")]
        
        public string TaskDetails { get; set; }

        public string Note { get; set; }

        [Display(Name = "Start Date")]
        
        public string StartDate { get; set; }
        [Display(Name = "Start Time")]
        [Required]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? StartTime { get; set; }
        [Display(Name = "End Date")]
        
        public string EndDate { get; set; }
        [Display(Name = "End Time")]
        [Required]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }
        [Display(Name = "Team Head")]
        public long? driver { get; set; }
        public int? leavestatus { get; set; }
        [Display(Name = "Assigned To")]
        public long? AssignedTo { get; set; }
        public long[] AssignedMembers { get; set; }
        public TaskPriority Priority { get; set; }
        [Display(Name = "Task Status")]
        public long TaskStatus { get; set; }
        public JobType jobtypes { get; set; }
        public PaymentType paymenttypes { get; set; }
        [Display(Name = "Cash Amount")]
       // [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal amount { get; set; }
        [Display(Name = "Cheque No")]
        public string chequeno { get; set; }
        [Display(Name = "Bank Name")]
        public string bank { get; set; }
        [Display(Name = "Vehicle Type")]
        public long? VehicleType { get; set; }

        [Display(Name = "Vehicle Manufacturer")]
        public long? VehicleManufacturer { get; set; }

        [Display(Name = "Vehicle Model")]
        public long? VehicleModel { get; set; }

        //[Display(Name = "Task Status")]
        //public TaskStatus TaskStatus { get; set; }

        //[Display(Name = "Team Members")]
        //public long[] TeamMembers { get; set; }
        //public TKUpdateStatus? upStatus { get; set; }
        public string Location { get; set; }

        [Display(Name = "Customer")]
        public long? CustomerID { get; set; }
        [Display(Name = "Customer")]
        public string CustomerName { get; set; }

        [Display(Name = "Assign Team")]
        public string AssignType { get; set; }

        public long? SalesPerson { get; set; }

        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        public List<FieldMapping> FieldMapAll { get; set; }
        public List<Contact> LstContacts { get; set; }

        public long[] AssignTypeAll { get; set; }

        //reminder
        [Display(Name = "Date")]
        public string RDate { get; set; }
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public virtual DateTime RTime { get; set; }
        [Display(Name = "Note")]
        public string RemNote { get; set; }
        public long[] RemAssignedTo { get; set; }

        // public string[] TaskMobiles { get; set; }
        public string Redirection { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

        public string To { get; set; }
        public string From { get; set; }

    }
    public class ProTaskUpdatViewModel
    {
        public long ProTaskId { get; set; }
        public TKUpdateStatus Status { get; set; }
        public string Location { get; set; }
        [StringLength(100)]
        public string Lattitude { get; set; }
        [StringLength(100)]
        public string Longitude { get; set; }
        public string Remarks { get; set; }
        [Display(Name = "Assigned To")]
        public long? AssignedTo { get; set; }
        [Display(Name = "Team Members")]
        public long[] TeamMembers { get; set; }
        [Display(Name = " Date")]
        public string EndDate { get; set; }
        [Display(Name = "End Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }
    }
    public class ViewProTaskViewModel
    {
        public long? ProTaskId { get; set; }
        [Display(Name = "Task Name")]
        public string TaskName { get; set; }
        [Display(Name = "Task Code")]
        public string TaskCode { get; set; }
        [Display(Name = "Task Type")]
        public string TaskType { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy hh:mm tt}")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy hh:mm tt}")]
        public DateTime? EndDate { get; set; }
        public string Priority { get; set; }
        public string ProjectName { get; set; }
        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }
        public string CreatedBy { get; set; }

        public Contact Employee { get; set; }


        [Display(Name = "Start Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? StartTime { get; set; }
        [Display(Name = "End Time")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh:mm tt}")]
        public DateTime? EndTime { get; set; }




        [Display(Name = "Team Members")]
        public long[] TeamMembers { get; set; }
        public string Location { get; set; }
        public string ProLocation { get; set; }
        public string CustLocation { get; set; }

        public List<TaskUpdationViewModel> TaskUpd { get; set; }

        public List<TaskImageViewModel> TaskImages { get; set; }
        public List<TaskAssignToViewModel> TaskAssign { get; set; }
        public List<TaskDocumentViewModel> TaskDocuments { get; set; }
        public List<LeadDocumentViewModel> LeadcreateDocuments { get; set; }
        public List<TaskTimelineViewModel> TaskTimeLine { get; set; }
        public List<TaskTimelineViewModel> TaskAssignedVModel { get; set; }

        public string Access { get; set; }


        public string TaskStatus { get; set; }
        public DateTime? CreatedDate { get; set; }



        public string CustomerName { get; set; }
        public long? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string TaxRegNo { get; set; }
        //contacts
        public string Address { get; set; }
        public string Country { get; set; }
        [Display(Name = "Emirate")]
        public string State { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Phone]
        public string Mobile { get; set; }
        public string Fax { get; set; }
        [Display(Name = "Email Id")]
        public string EmailId { get; set; }
        public string Reference { get; set; }
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }
        [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }
        public string SalesPersonName { get; set; }


        
        public string TaskDetails { get; set; }
        public string Note { get; set; }
        public string CustType { get; set; }

        public List<FieldMapping> FieldMap { get; set; }

        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public ICollection<MobileViewModel> mob { get; set; }
        public DateTime? UpDate { get; set; }
        public List<CustTimelineViewModel> CustTimeLine { get; set; }
    }
    public class TaskAssignToViewModel
    {
        public string Empname { get; set; }
    }


    public class TaskDocumentViewModel
    {
        public long TaskDocumentId { get; set; }
        public long ProTaskId { get; set; }
        public string FileName { get; set; }
        public string thumpname { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string description { get; set; }
        public string newdescription { get; set; }
    }
    public class TaskTimelineViewModel
    {
        public string Name { get; set; }
        public string LogType { get; set; }
        public DateTime Time { get; set; }
        public string Details { get; set; }
        public string TStatus { get; set; }
        public long ProTaskId { get; set; }
        public string FileName { get; set; }
        public long TaskDocumentId { get; set; }
        public List<TaskImageViewModel> RImages { get; set; }
        public List<ChecklistViewModel> check { get; set; }

    }
    public class TaskStatusViewModel
    {
        public long TaskStatusId { get; set; }
        [Display(Name = "Status Name ")]
        public string StatusName { get; set; }

        public Status Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long? Branch { get; set; }
        public Branch CreatedBranch { get; set; }

        public string Department { get; set; }
        public string Designation { get; set; }

    }

    public class RemarkChecklistViewModel
    {
        public long Id { get; set; }
        public string Note { get; set; }
        public string Check { get; set; }

    }
    public class RemarklistViewModel
    {
        public long TaskRemarkId { get; set; }
        public long TaskId { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }
        [Display(Name = "Task Status")]
        public long? TaskStatusID { get; set; }
        public long? TaskUpdationID { get; set; }
        public DateTime CreatedDate { get; set; }
        public long[] AssignedMembers { get; set; }
        public List<Microsoft.AspNetCore.Http.IFormFile> fileupload { get; set; }
        public ICollection<RemarkChecklistViewModel> bstmodel { get; set; }
        public List<TaskDocument> TaskDocuments { get; set; }
        public DateTime? starttime { get; set; }
        [Display(Name ="Task Type")]
        public long? tasktype { get; set; }
        //public ICollection<RemarkChecklistViewModel> checklistmodel { get; set; }
    }

    public class RemarkCheckViewModel
    {
        public long? Id { get; set; }
        public string Remark { get; set; }
        public long? Checklistitemid { get; set; }
        [StringLength(250)]
        public string Note { get; set; }

        public string Stage { get; set; }
        public string Status { get; set; }

        public long? StatusId { get; set; }

        public long? ProTaskId { get; set; }
        public string Employee { get; set; }


        public List<FieldMapping> FieldMap { get; set; }
        public List<FieldMapping> FieldMapAll { get; set; }


        public ICollection<ImageRemarkViewModel> RImages { get; set; }

        public ICollection<ChecklistViewModel> CheckItems { get; set; }

        public DateTime Createddate { get; set; }

    }
    public class ImageViewModel
    {
        public string FileName { get; set; }
        public long? TaskUpdationID { get; set; }
    }
    public class ImageRemarkViewModel
    {
        public string FileName { get; set; }
        public long? TaskUpdationID { get; set; }
    }
    public class CustomerSatisfactionViewModel
    {
        [Key]
        public long Id { get; set; }
        public long ProTaskId { get; set; }
        public string SatisfactionLevel { get; set; }
        public string Comments { get; set; }
        public string Signature { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TransType { get; set; }
    }
    public class calview
        {
            public DateTime dt { get; set; }
            public bool selectedcol { get; set; }
            public string protaskid { get; set; }
            public string taskassigned { get; set; }
            public string taskname { get; set; }
            public string colorname { get; set; }
            public string firstname { get; set; }


        }
    public class calviewlist
    {
        public List<calview> mulcalview1 { get; set; }
        public List<calview> mulcalview2 { get; set; }
        public List<calview> mulcalview3 { get; set; }
    }
    public class SelectFormatpro
    {
        public long id { get; set; }
        public string text { get; set; }
        public string dt { get; set; }
    }

}