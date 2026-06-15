using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
       // [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
        public string branch { get; set; }
        public string fromapp { get; set; }
        public string lat { get; set; }
        public string log { get; set; }
    }
    public class Training
    {
        [Required(ErrorMessage = "Enter First Name")]
        [Display(Name = "First Name : ")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name : ")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Enter Mobile")]
        [Display(Name = "Whatsapp No : ")]
        public string mobile { get; set; }
        [Display(Name = "Company : ")]
        public string Company { get; set; }
        [Required(ErrorMessage = "Enter Email")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        [Display(Name = "Email : ")]

        public string email { get; set; }
        [Display(Name = "Address : ")]
        public string Address { get; set; }
        [Display(Name = "Enquiry : ")]
        public string Enqury { get; set; }
        

    }
    public class Register
    {
        [Required(ErrorMessage ="Enter First Name")]
        [Display(Name = "First Name : ")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name : ")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Enter Mobile")]
        [Display(Name = "Whatsapp No : ")]
        public string mobile { get; set; }
        [Required(ErrorMessage = "Enter Email")]
        [EmailAddress(ErrorMessage = "Enter valid email address")]
        [Display(Name = "Email : ")]
    
        public string email { get; set; }
        [Display(Name = "Address : ")]
        public string Address { get; set; }
        [Display(Name = "Enquiry : ")]
        public string Enqury { get; set; }
        [Required(ErrorMessage = "User Name Required")]
        [Display(Name = "User Name : ")]
           public string UserName { get; set; }
        [Required(ErrorMessage = "Password Required")]
        [Display(Name = "Password : ")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password Required")]
        [Display(Name = "Confirm Password : ")]
        [Compare("Password", ErrorMessage = "Confirm password doesn't match, Type again !")]
        public string ConfirmPassword { get; set; }


    }
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
