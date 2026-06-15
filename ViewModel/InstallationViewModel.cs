using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class InstallationViewModel
    {
        //company 
        #region company

        [Required]
        [Display(Name = "Company Name ")]
        public string CPName { get; set; }

        [Display(Name = "Logo ")]
        public string CPLogo { get; set; }

        [Display(Name = "Email ")]
        [StringLength(150, MinimumLength = 5)]
        public string CPEmail { get; set; }

        [Display(Name = "Phone ")]
        [StringLength(150, MinimumLength = 5)]
        public string CPPhone { get; set; }

        [Display(Name = "Mobile ")]
        [StringLength(150, MinimumLength = 5)]
        public string CPMobile { get; set; }

        [Display(Name = "Main Branch")]
        public long? CPMainBranch { get; set; }

        [Display(Name = "Tax RegNo")]
        public string TRN { get; set; }


        [StringLength(500)]
        [Display(Name = "Address")]
        public string CPAddress { get; set; }


        // smtp Settings start
        [StringLength(150)]
        public string SMTPEmail { get; set; }

        [StringLength(150)]
        public string SMTPHost { get; set; }

        [StringLength(150)]
        public string SMTPUsername { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string SMTPPassword { get; set; }

        public long? SMTPPort { get; set; }
        // smtp Settings end
        public Boolean EnableSsl { get; set; }

        #endregion

        //system config
        #region system config

        public int SystemConfigId { get; set; }
        // add encrypted code
        [Display(Name = "System Code")]
        public string SystemCode { get; set; }

        // add encrypted value to systemtypes by enum type of SystemType
        [Display(Name = "System Types")]
        public SystemType SystemTypes { get; set; }

        [Display(Name = "License Key")]
        public string LicenseKey { get; set; }
        [Display(Name = "License Type")]
        public string LicenseType { get; set; }

        // add encrypted date
        [Display(Name = "Install Date")]
        public string StartDate { get; set; }

        // add encrypted date
        public string EndDate { get; set; }

        // add encrypted count of days
        public string Extentdays { get; set; }

        public int? Year { get; set; }

        [Display(Name = "Financial End")]
        public FinancialEnd FinancialEnd { get; set; }
        public string MACID { get; set; }

        public Status status { get; set; }

        #endregion

        //branch 
        [Display(Name = "Branch")]
        public long BranchID { get; set; }

        //user
        #region user 

        public string id { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        [Display(Name = "No. of User")]
        public string NumberOfUsers { get; set; }

        #endregion
        [Display(Name = "Demo Period")]
        public string DemoPeriod { get; set; }

        [Display(Name = "Financial Year Starting Date")]
        public string FinStartDate { get; set; }

    }
    public class ExpireViewModel
    {
        public string Message { get; set; }
        public string Type { get; set; }
    }
}