using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class FileDocumentViewModel
    {
        public long Id { get; set; }
        [Required]
        [Display(Name = "Document")]
        public string Document { get; set; }
        public long[] AssignedMembers { get; set; }
        public long Documenttype { get; set; }
        [Required]
        [Display(Name = "Document Name")]
        public string DocumentName { get; set; }
        public string Documentview { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ReminderDate { get; set; }
        public long reminderrepeate { get; set; }
        public string Note { get; set; }
        public Status Status { get; set; }
        public long Branch { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public int? openclose { get; set; }
        public List<FileDocument> Lstfdocs { get; set; }
        public List<FilemultipleDocuments> Multidoc { get; set; }
        public List<Multiviewmodel> lstMultidocview { get; set; }
        public List<Realfdoc> lstrealdoc { get; set; }
        public List<FileDocumentViewModel> Lstfdocsvm { get; set; }
        //public long? amcdocumentid { get; set; }
    }
    public class GoogleReviewViewModel
    {
       
        
        public long QuotCashier { get; set; }
      
        public DateTime QuotDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public List<FileDocument> Lstfdocs { get; set; }
        public List<FilemultipleDocuments> Multidoc { get; set; }
        public List<Multiviewmodel> lstMultidocview { get; set; }
        public List<Realfdoc> lstrealdoc { get; set; }
        public List<FileDocumentViewModel> Lstfdocsvm { get; set; }
    }
    public class Multiviewmodel
    {

        public long Id { get; set; }
        public long? documentid { get; set; }
        public string Document { get; set; }
        public string DocumentName { get; set; }
        public long Documenttype { get; set; }
        public string Documentview { get; set; }
        public string ExpiryDate { get; set; }
        public string Note { get; set; }
        public Status Status { get; set; }
        public long Branch { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string filenamelead { get; set; }
    }
    public class FilemultipleDocumentsview
    {

        public long Id { get; set; }
        public long? documentid { get; set; }
        public string Document { get; set; }
        public string DocumentName { get; set; }
        public long Documenttype { get; set; }
        public string Documentview { get; set; }
        public string ExpiryDate { get; set; }
        public string Note { get; set; }
        public Status Status { get; set; }
        public long Branch { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string filenamelead { get; set; }
    }

    public class Realfdoc
    {

        public long Id { get; set; }
        public string DocumentName { get; set; }
        public string ExpiryDate { get; set; }
        public string Document { get; set; }
        public long Documenttype { get; set; }
       
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Status Status { get; set; }
        
      
    }

}