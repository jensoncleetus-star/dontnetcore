using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class SupplierViewModel
    {


        public long SupplierID { get; set; }
        // Used by the mobile Supplier/App views (Create/Edit); the desktop views use SupplierSubmitViewModel.
        public string SupplierCode { get; set; }
        public string Remark { get; set; }
        [Required]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; }


        public virtual Contact ContactID { get; set; }

        [Display(Name = "Credit Limit")]

        public decimal? CreditLimit { get; set; }

        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        public string emark { get; set; }

        public virtual Accounts AccountID { get; set; }


        [Display(Name = "TRN")]
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
        [Display(Name = "Sales Person")]
        public string ContactPerson { get; set; }


        [StringLength(50)]
        [Phone]
        [Display(Name = "Sales Person Mobile")]
        public string SalesPMob { get; set; }

        [DefaultValue(true)]
        public Status Status { get; set; }



        //bank details
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Bank Name")]
        public long AccountsID { get; set; }
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
        [Display(Name = "Tax Type")]
        public TaxType? TaxType { get; set; }
        public string Addres { get; set; }
        public string DCname { get; set; }
        public List<ReferenceAccountViewModel> invoicedata { get; set; }
        public List<CustomerDocument> LstsupplierDocument { get; set; }

    }

    public class SupplierSubmitViewModel

    {
        public string Website { get; set; }
        public virtual Contact Contact { get; set; }
        public string TypeOfContact { get; set; }

        public long? ContactTypeID { get; set; }
        public long? ContactID_ContactID { get; set; }
        public int CountryID { get; set; }

        public long SupplierID { get; set; }

        [Required]
        [Display(Name = "Supplier Code")]
        public string SupplierCode { get; set; }

        [Required]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; }

        public virtual Contact ContactID { get; set; }

        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }

        [Display(Name = "Credit Period")]
        public int? CreditPeriod { get; set; }

        public string Remark { get; set; }

        public virtual Accounts AccountID { get; set; }


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
        [Display(Name = "Sales Person")]
        public string ContactPerson { get; set; }


        [StringLength(50)]
        [Phone]
        [Display(Name = "Sales Person Mobile")]
        public string SalesPMob { get; set; }

        [DefaultValue(true)]
        public Status Status { get; set; }


        //bank details
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }
        [Display(Name = "Bank Name")]
        public long AccountsID { get; set; }
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
        [Display(Name = "Tax Type")]
        public TaxType? TaxType { get; set; }

        public string Addres { get; set; }
        public string DCname { get; set; }
        public ICollection<MobileViewModel> mobmodel { get; set; }
        public string ConvertFrom { get; set; }

        public MobileViewModel mobmodel1 { get; set; }
        public List<ReferenceAccountViewModel> invoicedata { get; set; }



        [Display(Name = "Supplier Alias")]
        public string Alias { get; set; }
        public List<Contact> LstContacts { get; set; }
        public List<CustomerDocument> LstsupplierDocument { get; set; }
        public List<SupplierDocumentviewmodel> SuppliDocumentviewmodel { get; set; }

        public long[] SupplierItems { get; set; }
        public long[] SupplierCategories { get; set; }
        public long[] SupplierBrands { get; set; }
    }
    public class SupplierDocumentviewmodel
    {

        public long DocumnetId { get; set; }


        public long CutomerID { get; set; }


        public string DoucumentType { get; set; }

        public DateTime Expiry { get; set; }

        public string Notes { get; set; }

        public string FilePath { get; set; }
        public long ContactId { get; set; }

        public long DocumentTypeID { get; set; }
        public string filenamelead { get; set; }
    }
}