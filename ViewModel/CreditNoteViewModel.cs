using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuickSoft.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class CreditNoteViewModel
    {
        public long CreditnoteId { get; set; }
        // seno defines bill number BillNo defines company.invoiceprefix + SENo
        [Required]
        public string BillNo { get; set; }

        public long CNNo { get; set; }
        public long CreditType { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime CNDate { get; set; }
        // refer to table emploayee

        [Display(Name = "Pay From")]
        public long PayFrom { get; set; }
        [Display(Name = "Pay To")]
        public long PayTo { get; set; }
        
        // extra note option
        public string CNNote { get; set; }
        public string Remarks { get; set; }
        public string Description { get; set; }
        // print times may use
        public int Print { get; set; }
        
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public long Branch { get; set; }
        public int Status { get; set; }
        public long? MaterialCenter { get; set; }

        public decimal SubTotal { get; set; }
        public long? Tax { get; set; }
        public decimal? TaxPer { get; set; }
        public decimal TaxAmount { get; set; }
        //[DataType(DataType.Currency)]
        public decimal GrandTotal { get; set; }

        public List<CNInvoiceViewModel> CNInvoice { get; set; }
        public IEnumerable<Tax> Taxs { get; set; }

        [Display(Name = "Email Address")]
        public string custEmailId { get; set; }
    }
    public class CNInvoiceViewModel
    {
        public long CreditnoteId { get; set; }
        public long EntryNo { get; set; }
        public decimal? CreditAmount { get; set; }
    }
}