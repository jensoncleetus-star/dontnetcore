using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class Branch
    {
        public long BranchID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Branch Code")]
        public string BranchCode { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }

        public string Address { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [StringLength(100)]
        public string State { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        public int? ZipCode { get; set; }

        [StringLength(15)]
        [Phone]
        public string LandLineNumber { get; set; }

        [StringLength(15)]
        [Phone]
        public string MobileNumber { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string EmailId { get; set; }

        public string TaxId { get; set; }

        
        public string Location { get; set; }

        public Status Status { get; set; }

        public bool MainBranch { get; set; }

        public Company CompanyID { get; set; }

        public choice Editable { get; set; }

        public Branch(){
            CompanyID = null;
        }
    }
}