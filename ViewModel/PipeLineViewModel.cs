using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;
using System.ComponentModel;

namespace QuickSoft.ViewModel
{
    public class PipeLineViewModel
    {

        public long? CustomerID { get; set; }

        [Required]
        [Display(Name = "PipeLine Code")]
        public string CustomerCode { get; set; }

        [Required]
        [Display(Name = "PipeLine Name")]
        public string CustomerName { get; set; }

        public long? Contact { get; set; }


        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }


        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        //public string Lattitude { get; set; }
        //public string Longitude { get; set; }

        public string Remark { get; set; }

        public long? Accounts { get; set; }


        public string Location { get; set; }
        [Display(Name = "Tax RegNo")]
        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Emirate")]
        public string State { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        public string Zip { get; set; }


        [StringLength(15)]
        [Phone]
        public string Phone { get; set; }

        [StringLength(15)]
        [Phone]
        public string Mobile { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Id")]
        public string EmailId { get; set; }

        public string Reference { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }


        [DefaultValue(true)]
        public Status Status { get; set; }


        [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        //bank details
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }
        [Display(Name = "Iban No")]
        public string IbanNo { get; set; }
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
        public string Swift { get; set; }
        [Display(Name = "Opening Balance")]
        public decimal OpnBalance { get; set; }
        //[Display(Name = "Opening Balance Credit")]
        //public decimal OpnBalanceCr { get; set; }
        public DC DC { get; set; }

        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }
        public string ConvertFrom { get; set; }
        [Display(Name = "Lead Status")]
        public long? LeadStat { get; set; }
        [Display(Name = "Lead Level")]
        public string LeadLevel { get; set; }

        [Display(Name = "AssignedTo")]
        public long[] AssignedTo { get; set; }

    }

    public class PipeLineSubmitViewModel
    {

        public long? CustomerID { get; set; }

        [Required]
        [Display(Name = "PipeLine Code")]
        public string CustomerCode { get; set; }

        [Required]
        [Display(Name = "PipeLine Name")]
        public string CustomerName { get; set; }

        public long? Contact { get; set; }


        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }


        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        //public string Lattitude { get; set; }
        //public string Longitude { get; set; }

        public string Remark { get; set; }

        public long? Accounts { get; set; }


        public string Location { get; set; }
        [Display(Name = "Tax RegNo")]
        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Emirate")]
        public string State { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        public string Zip { get; set; }


        [StringLength(15)]
        [Phone]
        public string Phone { get; set; }

        [StringLength(15)]
        [Phone]
        public string Mobile { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Id")]
        public string EmailId { get; set; }

        public string Reference { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }


        [DefaultValue(true)]
        public Status Status { get; set; }


        [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        //bank details
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }
        [Display(Name = "Iban No")]
        public string IbanNo { get; set; }
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
        public string Swift { get; set; }
        [Display(Name = "Opening Balance")]
        public decimal OpnBalance { get; set; }
        //[Display(Name = "Opening Balance Credit")]
        //public decimal OpnBalanceCr { get; set; }
        public DC DC { get; set; }

        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }
        public string ConvertFrom { get; set; }
        [Display(Name = "Lead Status")]
        public long? LeadStat { get; set; }
        [Display(Name = "Lead Level")]
        public string LeadLevel { get; set; }

        [Display(Name = "AssignedTo")]
        public long[] AssignedTo { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }
        public ICollection<Mobile> mob { get; set; }

    }

    public class PipeLineDetailsViewModel
    {

        public long CustomerID { get; set; }

        [Display(Name = "AssignedTo")]
        public long[] AssignedTo { get; set; }


        [Required]
        [Display(Name = "PipeLine Code")]
        public string CustomerCode { get; set; }

        [Required]
        [Display(Name = "PipeLine Name")]
        public string CustomerName { get; set; }

        public long? Contact { get; set; }


        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }


        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        //public string Lattitude { get; set; }
        //public string Longitude { get; set; }

        public string Remark { get; set; }

        public long? Accounts { get; set; }


        public string Location { get; set; }
        [Display(Name = "Tax RegNo")]
        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Emirate")]
        public string State { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        public string Zip { get; set; }


        [StringLength(15)]
        [Phone]
        public string Phone { get; set; }

        [StringLength(15)]
        [Phone]
        public string Mobile { get; set; }

        [StringLength(15)]
        public string Fax { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Id")]
        public string EmailId { get; set; }

        public string Reference { get; set; }

        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }

        [Display(Name = "Sales Person")]
        public long? SalesPerson { get; set; }


        [DefaultValue(true)]
        public Status Status { get; set; }


        [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        //bank details
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Account No")]
        public string AccountNo { get; set; }
        [Display(Name = "Iban No")]
        public string IbanNo { get; set; }
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
        public string Swift { get; set; }
        [Display(Name = "Opening Balance")]
        public decimal OpnBalance { get; set; }
        //[Display(Name = "Opening Balance Credit")]
        //public decimal OpnBalanceCr { get; set; }
        public DC DC { get; set; }

        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }

        [Display(Name = "Lead Status")]
        public long? LeadStat { get; set; }
        [Display(Name = "Lead Level")]
        public string LeadLevel { get; set; }
        public List<LeadDocumentViewModel> LeadDocuments { get; set; }

        public string leadStausDisp { get; set; }
        public string leadSourceDisp { get; set; }
        public List<LeadAssignToViewModel> LeadAssign { get; set; }
        public List<LeadCreatedViewModel> LeadCreated { get; set; }
        public string CreatedUser { get; set; }
        public List<LeadActivityViewModel> LeadActivity { get; set; }
        public List<LeadTimelineViewModel> LeadTimeLine { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

    }
}