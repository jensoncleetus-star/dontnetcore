using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class AMCViewModel
    {
        [Required]
        [Display(Name = "Contract No.")]
        public long AmcNo { get; set; }

        [Required]
        [Display(Name = "Contract Name")]
        public long ContractId { get; set; }

        [Required]
        [Display(Name = "Customer Name")]
        public long? CustomerId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public string StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public string EndDate { get; set; }

        [Required]
        [Display(Name = "Reminder Date")]
        public string ReminderDate { get; set; }
        public long? ContractTypeId { get; set; }
        //public TaskPriority ContractLevel { get; set; }
        public int? ContractLevelId { get; set; }
        //public string ContractLevel { get; set; }
        public long? LocationId { get; set; }

        [Display(Name = "Assign Team")]
        public string AssignType { get; set; }

        [Display(Name = "Assigned To")]
        public long[] AssignedTo { get; set; }
        public long[] AssignedToo { get; set; }
        public long[] AssignTypeAll { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }         
        public string Lattitude { get; set; }
        public string Longitude { get; set; }
        public bool PerdcMaintReq { get; set; }
        public long? AmcStatusId { get; set; }
        public string AmcDetails { get; set; }
        public string Notes { get; set; }
        public long? NoOfPMaint { get; set; }

        public List<Contact> LstContacts { get; set; }
        public List<AmcDocumentViewModel> LstAmcDocument { get; set; }
        //public List<AmcDocumentViewModel> LstAmcDocumentEdit { get; set; }
        public List<SalesEntry> LstSalesEntry { get; set; }
        public List<PrdcMaintViewModel> LstPerdcMaint { get; set; }
        public List<FieldMapping> FieldMap { get; set; }
        public List<FieldMapping> FieldMapAll { get; set; }
        public int? OpenClose { get; set; }

    }

    public class AmcDocumentViewModel
    {
        public long DocumentId { get; set; }
        public long TransId { get; set; }
        public string TransType { get; set; }
        public IFormFile FileName { get; set; }
        public string FileNameAmc { get; set; }
        public long? DocumentTypeID { get; set; }
        public DateTime? Expiry { get; set; }
        public string Notes { get; set; }
    }

    public class PrdcMaintViewModel
    {
        public long PMaintDtlsId { get; set; }
        public long PMaintId { get; set; }
        public DateTime PDate { get; set; }
        public string Notes { get; set; }        
        public long AmcId { get; set; }
    }
    public class StatusUpdateViewModel
    {
        public long RemarkId { get; set; }
        public long TransId { get; set; }
        public string TransType { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }

        [Display(Name = "Amc Status")]
        public long? AmcStatusId { get; set; }

        [Display(Name = "Status")]
        public long? PMaintStatusId { get; set; }
        public long? UpdationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public long[] AssignedMembers { get; set; }
    }

    public class AmcStatusViewModel
    {
        public long AmcStatusId { get; set; }

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

    public class AmcPeriodicMaintenanceViewModel
    {
        public long PMaintId { get; set; }

        [Required]
        [Display(Name = "Periodic Sl No.")]
        public long PMaintNo { get; set; }

        [Required]
        [Display(Name = "AMC No.")]
        public long AmcId { get; set; }
        public long NoOfPMaint { get; set; }
        public string PDate { get; set; }
        public string Notes { get; set; }

        [Display(Name = "Assign Team")]
        public string AssignType { get; set; }

        [Display(Name = "Assigned To")]
        public string AssignedTos { get; set; }        
        public long[] AssignedTo { get; set; }
        public long[] AssignTypeAll { get; set; }
        public string CustomerName { get; set; }
        public string ContractName { get; set; }
        public long PMainDetailId { get; set; }
    }

    public class PeriodicStatusUpdateViewModel
    {
        public long PeriodicRemarkId { get; set; }
        public long PeriodicId { get; set; }
        public string AddedUser { get; set; }
        public string Remark { get; set; }
        public string Level { get; set; }

        [Display(Name = "Periodic Maintenance Status")]
        public long? PeriodicMaintStatusId { get; set; }
        public long? PeriodicMaintUpdationId { get; set; }
        public DateTime CreatedDate { get; set; }
        public long[] AssignedMembers { get; set; }
    }
    public class ViewAMCViewModel
    {
        public long AmcId { get; set; }
        public long AmcNo { get; set; }
        public string ContractName { get; set; }
        public string CustomerName { get; set; }
        //public DateTime StartDate { get; set; }
        //public DateTime EndDate { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy hh:mm tt}")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(DataType.Time), DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MM-yyyy hh:mm tt}")]
        public DateTime? EndDate { get; set; }
        public string ReminderDate { get; set; }
        public string ContractType { get; set; }
        public string ContractLevel { get; set; }
        public string Location { get; set; }
        public string CustLocation { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpDate { get; set; }
        public string AmcStatus { get; set; }
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
        
        public string AmcDetails { get; set; }
        public string Notes { get; set; }
        public string CustType { get; set; }
        public long? NoOfPMaint { get; set; }
        public ICollection<MobileViewModel> mob { get; set; }
        public List<FieldMapping> FieldMap { get; set; }
        public List<FieldMapping> FieldMapAll { get; set; }
        public List<AssignToViewModel> AssignTo { get; set; }
        public List<AmcDocumentViewModel> AmcDocuments { get; set; }
    }    
    public class amcdocattach
    {
        public long amcid { get; set; }
        public List<AmcDocumentViewModel> AmcDocuments { get; set; }
        public FileDocumentViewModel filedoc { get; set; }
    }
    public class AssignToViewModel
    {
        public string Empname { get; set; }
    }

}