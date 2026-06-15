using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class TenancyContractViewModel
    {
        public long Id { get; set; }
        [StringLength(20)]
        public string Code { get; set; }
        [Required]
        [Display(Name = "Customer")]
        public long? Tenant { get; set; }
        [Required]
        [Display(Name = "Property")]
        public long? Property { get; set; }

        public long? Unit { get; set; }
        public string contractvalue
        {
            get; set;
        }
        public string issuedate { get; set; }
        [Required]
        public string StartDate { get; set; }
        [Required]
        public string EndDate { get; set; }
        [Required]
        public long? Duration { get; set; }

        //rent section
        
        [Display(Name = "Rent Amount")]
        public decimal? Rent { get; set; }
        public string  PetsAllowed { get; set; }
    public string WaterAndElectricityBill { get; set; }
    public string NumberofOccupants { get; set; }
[Display(Name = "Security Deposit Amount")]
        public decimal? Deposit { get; set; }
        public Schedule Schedule { get; set; }
        public long? DueDate { get; set; }
        public long? PaymentType { get; set; }
        public long? PaymentTypeDeposit { get; set; }
        public long? DocumentType { get; set; }
        public string File { get; set; }


        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemImage { get; set; }
        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemDocument { get; set; }
        

        public ICollection<ChequeViewModel> cheqmodel { get; set; }
        public ICollection<DocumentTypeViewModel> docmodel { get; set; }
        public ICollection<ChequeViewModel> cheqmodeldep { get; set; }
        public string DocName { get; set; }
        public long? DocId { get; set; }
        public long? ItmDocId { get; set; }

        public string Remark { get; set; }
        [Display(Name = "Additinal occupant Details")]
        public string Note { get; set; }
        public string TermsCondition { get; set; }

        public string Section { get; set; }

        public string PropertyName { get; set; }
        public string TenantName { get; set; }
        public string UnitName { get; set; }
        public string Schedulename { get; set; }
        public string DurationName { get; set; }
        public string Due { get; set; }
        public string Payment { get; set; }
        public DateTime? Date { get; set; }
    }

    public class ChequeViewModel
    {
        public int ID { get; set; }
        public decimal? Amount { get; set; }
        public string ChequeNo { get; set; }
        public string Date { get; set; }           
        public string Attachments { get; set; }
        public long? Bank { get; set; }
        public DateTime? ViewDate { get; set; }
    }

    public class DocumentTypeViewModel
    {
        public long ID { get; set; }
        public string Type { get; set; }
        public string Attachments { get; set; }
        public string Date { get; set; }

        public DateTime? ExpDate { get; set; }
    }
}