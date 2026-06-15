using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class TenancyContract
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public long EntryNo { get; set; }
        public long? Tenant { get; set; }

        public long? Property { get; set; }

        public long Unit { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        public DateTime? issuedate { get; set; }
        public string PetsAllowed { get; set; }
        public string contractvalue { get; set; }
        public string WaterAndElectricityBill { get; set; }
        public string NumberofOccupants { get; set; }
        public long? Duration { get; set; }

        //rent section
        public decimal? Rent { get; set; }

        public decimal? Deposit { get; set; }
        public Schedule Schedule { get; set; }
        public long? DueDate { get; set; }
        public long? PaymentType { get; set; }
        public long? PaymentTypeDeposit { get; set; }
        public string File { get; set; }

        public string Remark { get; set; }
        public string Note { get; set; }
        public string TnC { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }

    public class Cheque
    {
        public long ID { get; set; }
        public long Reference { get; set; }

        public string Purpose { get; set; }
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string ChequeNo { get; set; }

        public long? Bank { get; set; }

    }

    public class ChequeImage
    {
        public long ID { get; set; }

        [Required]
        public long Cheque { get; set; }

        [Required]
        public string attachments { get; set; }
    }

    public class ContractDocument
    {
        public long ID { get; set; }

        [Required]
        public long Tenancy { get; set; }

        [Required]
        public string FileName { get; set; }

        public int Status { get; set; }

        public virtual PropertyMain Items { get; set; }
    }

    public class TenancyDocumentType
    {
        public long ID { get; set; }
        public long TenancyContract { get; set; }
        [Required]
        public long DocumentType { get; set; }
    }
    public class DocumentFile
    {
        public long ID { get; set; }

        [Required]
        public long Document { get; set; }

        [Required]
        public string attachments { get; set; }
    }

    public class PropertyDocumentType
    {
        public long ID { get; set; }
        public long Reference { get; set; }
        public string Purpose { get; set; }
        public DateTime? ExpDate { get; set; }
        [Required]
        public long DocumentType { get; set; }
    }
}