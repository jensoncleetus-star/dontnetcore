using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{

    public class userResetPwdModel
    {

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class RolesViewModel
    {
        public string Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Roles")]
        public string Name { get; set; }
    }
    public class permissioncopy
    {
        [Required]
        public string emp1 { get; set; }
        [Required]
      public string[] emp2 { get; set; }
    }
    public class changepwd
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name ="Old Password")]
        public string oldpassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string newpassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("newpassword", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string confirmpassword { get; set; }
    }
    public class UserViewModel
    {
        //public long? id { get; set; }
        public string id { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }


        //[Display(Name = "Branch")]
        ////[Required(ErrorMessage = "Branch is required")]
        //public long BranchID { get; set; }

        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string[] Role { get; set; }

        public long Branch { get; set; }

        public IEnumerable<IdentityRole> UserRoles { get; set; }
        public decimal? Discount { get; set; }
        public long[] mcid { get; set; }
        public string[] purpose { get; set; }

        //[Required]
        //[Display(Name = "Branch Access")]
        //public BranchAccess BranchAccess { get; set; }


    }
    public class smssend
    {
      
        [Display(Name = "Mobile Number")]
        public string mobileno { get; set; }
        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }
        public string imageurl { get; set; }

    }
    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string EmailId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class passworddetailsViewModel
    {
       

        public long? passworddetailsid { get; set; }
        public string Title { get; set; }

        [Display(Name = "Password Group")]
        public long? LeadType { get; set; }
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Display(Name = "Url")]
        public string Url { get; set; }
        public string Notes { get; set; }
        public bool comefromtask { get; set; }
    public long[] AssignedMembers { get; set; }
    }
    public class filedocumentsViewModel
    {


        public long? filedocumentsid { get; set; }
        public string Title { get; set; }

       
  
        public string Notes { get; set; }
    
        public long[] AssignedMembers { get; set; }
    }


}