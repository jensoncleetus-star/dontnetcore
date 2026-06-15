using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class TenantViewModel
    {
        public long? TenantID { get; set; }

        [Required]
        [Display(Name = "Tenant Code")]
        public string TenantCode { get; set; }

        [Required]
        [Display(Name = "Tenant Name")]
        public string TenantName { get; set; }

        public virtual Contact ContactID { get; set; }


        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }


        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        //public string Lattitude { get; set; }
        //public string Longitude { get; set; }

        public string Remark { get; set; }

        public virtual Accounts AccountID { get; set; }


        public string Location { get; set; }
        [Display(Name = "TRN")]
        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

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



        //bank details
        [Display(Name = "AccountsID")]
        public long? AccountsID { get; set; }
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
        public decimal? OpnBalance { get; set; }
        //[Display(Name = "Opening Balance Credit")]
        //public decimal OpnBalanceCr { get; set; }
        public DC DC { get; set; }
        public string DCname { get; set; }

        public string SalesPersonName { get; set; }
        [Display(Name = "Tax Type")]
        public TaxType? TaxType { get; set; }

        public List<ReferenceAccountViewModel> invoicedata { get; set; }


        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }
        public string ConvertFrom { get; set; }
        [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

        public MobileViewModel mobmodel1 { get; set; }

        [Display(Name = "Tenant Alias")]
        public string Alias { get; set; }

        public ProType Type { get; set; }

        public ICollection<DocumentTypeViewModel> docmodel { get; set; }
        [Display(Name = "Account Group")]
        public long? AccountGroup { get; set; }

        public string Section { get; set; }
    }

    public class TenantSubmitViewModel
    {
        public long TenantID { get; set; }

        [Required]
        [Display(Name = "Tenant Code")]
        public string TenantCode { get; set; }

        [Required]
        [Display(Name = "Tenant Name")]
        public string TenantName { get; set; }

        public virtual Contact ContactID { get; set; }


        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }


        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        //public string Lattitude { get; set; }
        //public string Longitude { get; set; }

        public string Remark { get; set; }

        public virtual Accounts AccountID { get; set; }


        public string Location { get; set; }
        [Display(Name = "TRN")]
        public string TaxRegNo { get; set; }


        //contacts
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        //[Required]
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



        //bank details
        [Display(Name = "AccountsID")]
        public long AccountsID { get; set; }
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
        public string DCname { get; set; }

        public string SalesPersonName { get; set; }
        [Display(Name = "Tax Type")]
        public TaxType? TaxType { get; set; }

        public List<ReferenceAccountViewModel> invoicedata { get; set; }


        public string SrcLead { get; set; }
        public string SalesEmp { get; set; }
        public string ConvertFrom { get; set; }
        [Display(Name = "Source Of Lead")]
        public long? SourceOfLead { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

        public string Alias { get; set; }

        public ProType Type { get; set; }

        public ICollection<DocumentTypeViewModel> docmodel { get; set; }
        [Display(Name = "Account Group")]
        public long? AccountGroup { get; set; }

        public string Section { get; set; }
    }
}