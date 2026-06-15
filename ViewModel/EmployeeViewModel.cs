using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;

namespace QuickSoft.ViewModel
{
    public class EmployeeViewModel
    {
        public long? id { get; set; }

        [Display(Name = "Employee Code")]
        public string EMPCode { get; set; }

        [Required]
        public Prefix Prefix { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }


        [StringLength(100, MinimumLength = 1)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Alias { get; set; }

        [Display(Name = "Gender")]
        public Gender Gender { get; set; }
        [Display(Name = "Per Hour Charge")]
        public decimal? perhour { get; set; }


        [Display(Name = "Company Phone Number")]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [Display(Name = "Company  Email")]
        public string Email { get; set; }
        [Display(Name = "Personal Phone Number")]
        public string PPhoneNumber { get; set; }

        [EmailAddress]
        [Display(Name = "Personal Email")]
        public string PEmail { get; set; }


        [Display(Name = "Passport No")]
        public string PassportNo { get; set; }

        [Display(Name = "Target amount")]
        public string OtherIdNo { get; set; }

        [StringLength(250, MinimumLength = 2)]
        public string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(100, MinimumLength = 2)]
        public string City { get; set; }

        [Display(Name = "Emirate")]
        [StringLength(50)]
        public string State { get; set; }

        [Display(Name = "Country")]
        [StringLength(50)]
        public string Country { get; set; }

        [Display(Name = "Zip Code")]
        public string PostalCode { get; set; }



        [Display(Name = "Address")]
        public string CAddress { get; set; }


        [Display(Name = "City")]
        public string CCity { get; set; }

        [Display(Name = "Emirate")]
        public string CState { get; set; }

        [Display(Name = "Country")]
        public string CCountry { get; set; }

        [Display(Name = "Zip Code")]
        public string CPostalCode { get; set; }

        [Display(Name = "User Image")]
        public string ImgFileName { get; set; }

        //[Required(ErrorMessage = "Designation is required")]
        [Display(Name = "Designation")]
        public long? DesignationID { get; set; }

        [Display(Name = "Department")]
        //[Required(ErrorMessage = "Department is required")]
        public long? DepartmentID { get; set; }

        [Display(Name = "Branch")]
        //[Required(ErrorMessage = "Branch is required")]
        public long BranchID { get; set; }


        public bool UserStatus { get; set; }
        public bool ChkAddress { get; set; }


        // [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }


        // [Required]

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        //[Required(ErrorMessage = "User Role is required")]
        //[Display(Name = "User Roles")]
        public string[] Role { get; set; }

        public IEnumerable<AppModules> UserRoles { get; set; }




        public IEnumerable<Department> dept { get; set; }
        public IEnumerable<Designation> degn { get; set; }

        public string Asnuser { get; set; }

        //public int Status { get; set; }
        //public DateTime CreatedDate { get; set; }

        //[Required(ErrorMessage = "Branch is required")]
        //[Display(Name = "Branch")]
        //public virtual Branch Branchs { get; set; }

        [Required]
        [Display(Name = "Branch Access")]
        public BranchAccess BranchAccess { get; set; }

        public long? usertype { get; set; }
        public string Assingeduser { get; set; }

        public decimal? Discount { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

        public ICollection<EmpEduViewModel> edumodel { get; set; }
        public ICollection<EmpProViewModel> promodel { get; set; }
        public ICollection<EmpDocViewModel> docmodel { get; set; }

        //add in models below
        [Display(Name = "Date of Birth")]
        public string DOB { get; set; }
        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }
        [Display(Name = "Marital Status")]
        public string MaritalStatus { get; set; }

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

        [Display(Name = "Salary Mode")]
        public SalaryMode SalaryMode { get; set; }
        [Display(Name = "Work   Shift")]
        public long? WorkShift { get; set; }
        [Display(Name = "Bonus Applicable")]
        public bool BonusApplicable { get; set; }
        [Display(Name = "Over Time Applicable")]
        public bool OTApplicable { get; set; }
        [Display(Name = "Specify Loan Account")]
        public bool SpecifyLoanAccount { get; set; }
        [Display(Name = "Loan Account")]
        public long? LoanAccount { get; set; }
        [Display(Name = "Specify Advance Account")]
        public bool SpecifyAdvanceAccount { get; set; }
        [Display(Name = "Advance Account")]
        public long? AdvanceAccount { get; set; }
        [Display(Name = "Employee Grade")]
        public long? EmployeeGrade { get; set; }

        [Display(Name = "Employee Account")]
        public long? EmployeeAccount { get; set; }

        public string Note { get; set; }

        [Display(Name = "Join Date")]
        public string JoinDate { get; set; }
        [Display(Name = "Leaving Date")]
        public string LeavingDate { get; set; }
        [Display(Name = "Reason For Leaving")]
        public string ReasonForLeaving { get; set; }

        [Display(Name = "Annual Leave Applicable")]
        public bool AnnualLeaveApplicable { get; set; }

        [Display(Name = "Last Duty Resume Date")]
        public string LastDutyResumeDate { get; set; }
        [Display(Name = "Job Status")]
        public JobStatus JobStatus { get; set; }

        public string currentUser { get; set; }
        public string UserId { get; set; }

        [Display(Name = "Duty Resume Date")]
        public string DutyResumeDate { get; set; }
        
        public int? TotalWorkingDays { get; set; }
        public int? NoDaysWorked { get; set; }
        public List<EmployeeAttendanceSummarysViewModel> EmpSummarys { get; set; }
        public List<EmployeeAttendanceSummarysViewModel> EmpSummaryData { get; set; }
        [Display(Name = "Task Notification")]
        public bool appaccessonly { get; set; }
        [Display(Name = "Is Employee?")]
        public bool isemployee { get; set; }
        public long[] SkillSet { get; set; }
        public long[] TaskManner { get; set; }
        public string Payrollstratdate { get; set; }
    }
    public class EditViewModel
    {
        [Display(Name = "Employee Id")]
        public string EmployeeId { get; set; }

        [Required]
        public Prefix Prefix { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Gender")]
        public Gender Gender { get; set; }


        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Display(Name = "Passport No")]
        public string PassportNo { get; set; }

        [Display(Name = "Other Id No")]
        public string OtherIdNo { get; set; }

        [StringLength(250, MinimumLength = 2)]
        public string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(100, MinimumLength = 2)]
        public string City { get; set; }

        [Display(Name = "State")]
        [StringLength(100, MinimumLength = 2)]
        public string State { get; set; }

        [Display(Name = "Country")]
        [StringLength(100, MinimumLength = 2)]
        public string Country { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }



        [Display(Name = "Address")]
        public string CAddress { get; set; }


        [Display(Name = "City")]
        public string CCity { get; set; }

        [Display(Name = "State")]
        public string CState { get; set; }

        [Display(Name = "Country")]
        public string CCountry { get; set; }

        [Display(Name = "Postal Code")]
        public string CPostalCode { get; set; }

        [Display(Name = "User Image")]
        public string ImgFileName { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [Display(Name = "Designation")]
        public long? DesignationID { get; set; }

        [Display(Name = "Department")]
        [Required(ErrorMessage = "Department is required")]
        public long? DepartmentID { get; set; }

        [Display(Name = "Branch")]
        [Required(ErrorMessage = "Branch is required")]
        public long BranchID { get; set; }


        [Required(ErrorMessage = "User Role is required")]
        [Display(Name = "User Roles")]
        public string[] UserRoles { get; set; }

        [Required]
        [Display(Name = "Branch Access")]
        public BranchAccess BranchAccess { get; set; }
    }
    public class UserDetailViewModel
    {
        [Display(Name = "Employee Id")]
        public string EmployeeId { get; set; }

        [Required]
        public Prefix Prefix { get; set; }


        [Display(Name = "First Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Gender")]
        public Gender Gender { get; set; }


        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Display(Name = "Passport No")]
        public string PassportNo { get; set; }

        [Display(Name = "Other Id No")]
        public string OtherIdNo { get; set; }

        [StringLength(250, MinimumLength = 2)]
        public string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(100, MinimumLength = 2)]
        public string City { get; set; }

        [Display(Name = "State")]
        [StringLength(100, MinimumLength = 2)]
        public string State { get; set; }

        [Display(Name = "Country")]
        [StringLength(100, MinimumLength = 2)]
        public string Country { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }



        [Display(Name = "Address")]
        public string CAddress { get; set; }


        [Display(Name = "City")]
        public string CCity { get; set; }

        [Display(Name = "State")]
        public string CState { get; set; }

        [Display(Name = "Country")]
        public string CCountry { get; set; }

        [Display(Name = "Postal Code")]
        public string CPostalCode { get; set; }

        [Display(Name = "User Image")]
        public string ImgFileName { get; set; }

        public string Department { get; set; }
        public string Designation { get; set; }
        public string Branch { get; set; }

        public string[] Roles { get; set; }

        //[Required(ErrorMessage = "User Role is required")]
        //[Display(Name = "User Roles")]
        //public string UserRoles { get; set; }

        [Required]
        [Display(Name = "Branch Access")]
        public BranchAccess BranchAccess { get; set; }
    }

    //public class EmployeeSubmitViewModel
    //{
    //    public long? id { get; set; }

    //    [Display(Name = "Employee Code")]
    //    public string EMPCode { get; set; }

    //    [Required]
    //    public Prefix Prefix { get; set; }

    //    [Required]
    //    [StringLength(100, MinimumLength = 2)]
    //    [Display(Name = "First Name")]
    //    public string FirstName { get; set; }

    //    [Display(Name = "Middle Name")]
    //    public string MiddleName { get; set; }


    //    [StringLength(100, MinimumLength = 2)]
    //    [Display(Name = "Last Name")]
    //    public string LastName { get; set; }

    //    [Display(Name = "Gender")]
    //    public Gender Gender { get; set; }


    //    [Display(Name = "Phone Number")]
    //    public string PhoneNumber { get; set; }

    //    [EmailAddress]
    //    [Display(Name = "Email")]
    //    public string Email { get; set; }


    //    [Display(Name = "Passport No")]
    //    public string PassportNo { get; set; }

    //    [Display(Name = "Other Id No")]
    //    public string OtherIdNo { get; set; }

    //    [StringLength(250, MinimumLength = 2)]
    //    public string Address { get; set; }

    //    [Display(Name = "City")]
    //    [StringLength(100, MinimumLength = 2)]
    //    public string City { get; set; }

    //    [Display(Name = "Emirate")]
    //    [StringLength(50)]
    //    public string State { get; set; }

    //    [Display(Name = "Country")]
    //    [StringLength(50)]
    //    public string Country { get; set; }

    //    [Display(Name = "Zip Code")]
    //    public string PostalCode { get; set; }



    //    [Display(Name = "Address")]
    //    public string CAddress { get; set; }


    //    [Display(Name = "City")]
    //    public string CCity { get; set; }

    //    [Display(Name = "Emirate")]
    //    public string CState { get; set; }

    //    [Display(Name = "Country")]
    //    public string CCountry { get; set; }

    //    [Display(Name = "Zip Code")]
    //    public string CPostalCode { get; set; }

    //    [Display(Name = "User Image")]
    //    public string ImgFileName { get; set; }

    //    //[Required(ErrorMessage = "Designation is required")]
    //    [Display(Name = "Designation")]
    //    public long? DesignationID { get; set; }

    //    [Display(Name = "Department")]
    //    //[Required(ErrorMessage = "Department is required")]
    //    public long? DepartmentID { get; set; }

    //    [Display(Name = "Branch")]
    //    //[Required(ErrorMessage = "Branch is required")]
    //    public long BranchID { get; set; }


    //    public bool UserStatus { get; set; }
    //    public bool ChkAddress { get; set; }


    //    // [Required]
    //    [Display(Name = "User Name")]
    //    public string UserName { get; set; }


    //    // [Required]
    //    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    //    [DataType(DataType.Password)]
    //    [Display(Name = "Password")]
    //    public string Password { get; set; }

    //    [DataType(DataType.Password)]
    //    [Display(Name = "Confirm password")]
    //    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    //    public string ConfirmPassword { get; set; }

    //    //[Required(ErrorMessage = "User Role is required")]
    //    //[Display(Name = "User Roles")]
    //    public string[] Role { get; set; }

    //    public IEnumerable<AppModules> UserRoles { get; set; }




    //    public IEnumerable<Department> dept { get; set; }
    //    public IEnumerable<Designation> degn { get; set; }

    //    public string Asnuser { get; set; }

    //    //public int Status { get; set; }
    //    //public DateTime CreatedDate { get; set; }

    //    //[Required(ErrorMessage = "Branch is required")]
    //    //[Display(Name = "Branch")]
    //    //public virtual Branch Branchs { get; set; }

    //    [Required]
    //    [Display(Name = "Branch Access")]
    //    public BranchAccess BranchAccess { get; set; }

    //    public long? usertype { get; set; }
    //    public string Assingeduser { get; set; }

    //    public decimal? Discount { get; set; }

    //    public ICollection<MobileViewModel> mobmodel { get; set; }

    //    public ICollection<EmpEduViewModel> edumodel { get; set; }
    //    public ICollection<EmpProViewModel> promodel { get; set; }
    //    public ICollection<EmpDocViewModel> docmodel { get; set; }

    //    //add in models below
    //    [Display(Name = "Date of Birth")]
    //    public string DOB { get; set; }
    //    [Display(Name = "Blood Group")]
    //    public string BloodGroup { get; set; }
    //    [Display(Name = "Marital Status")]
    //    public string MaritalStatus { get; set; }

    //    [Display(Name = "AccountsID")]
    //    public long AccountsID { get; set; }
    //    [Display(Name = "Bank Name")]
    //    public string BankName { get; set; }
    //    [Display(Name = "Account No")]
    //    public string AccountNo { get; set; }
    //    [Display(Name = "Iban No")]
    //    public string IbanNo { get; set; }
    //    [Display(Name = "Branch Name")]
    //    public string BranchName { get; set; }
    //    public string Swift { get; set; }

    //    [Display(Name = "Salary Mode")]
    //    public SalaryMode SalaryMode { get; set; }
    //    [Display(Name = "Work   Shift")]
    //    public long? WorkShift { get; set; }
    //    [Display(Name = "Bonus Applicable")]
    //    public bool BonusApplicable { get; set; }
    //    [Display(Name = "Over Time Applicable")]
    //    public bool OTApplicable { get; set; }
    //    [Display(Name = "Specify Loan Account")]
    //    public bool SpecifyLoanAccount { get; set; }
    //    [Display(Name = "Loan Account")]
    //    public long? LoanAccount { get; set; }
    //    [Display(Name = "Specify Advance Account")]
    //    public bool SpecifyAdvanceAccount { get; set; }
    //    [Display(Name = "Advance Account")]
    //    public long? AdvanceAccount { get; set; }
    //    [Display(Name = "Employee Grade")]
    //    public long? EmployeeGrade { get; set; }

    //    public string Note { get; set; }
    //}
    public class EmpEduViewModel
    {
        public string Course { get; set; }
        public string Specialization { get; set; }
        public string Institute { get; set; }
        public string University { get; set; }
        public string PassingYear { get; set; }
        public decimal? Percentage { get; set; }
    }
    public class EmpProViewModel
    {
        public string Organization { get; set; }
        public string Designation { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Responsibility { get; set; }
        public string Skills { get; set; }
    }
    public class EmpDocViewModel
    {
        public long? EmployeeDocumentId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentNo { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public string Note { get; set; }
        public string Attachments { get; set; }
        public string PersonalNo { get; set; }
    }

    public class EmployeeDetailViewModel
    {
        public string EMPCode { get; set; }
        public string Gender { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public string PPhoneNumber { get; set; }
        public string PEmail { get; set; }
        public long EmployeeId { get; set; }

        public ICollection<MobileViewModel> mob { get; set; }

        public DateTime? DOB { get; set; }

        public string MaritalStatus { get; set; }
        public string BloodGroup { get; set; }
        public string PassportNo { get; set; }
        public string OtherIdNo { get; set; }

        public DateTime? JoinDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public string ReasonForLeaving { get; set; }

        public string ImgFileName { get; set; }
        public string Note { get; set; }

        public string Department { get; set; }
        public string Designation { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }

        public string CAddress { get; set; }
        public string CCity { get; set; }
        public string CState { get; set; }
        public string CCountry { get; set; }
        public string CZip { get; set; }

        public ICollection<EmpEduViewModel> edumodel { get; set; }
        public ICollection<EmpProViewModel> promodel { get; set; }
        public ICollection<EmpDocViewModel> docmodel { get; set; }

        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string IbanNo { get; set; }
        public string BranchName { get; set; }
        public string Swift { get; set; }

        public string SalaryMode { get; set; }
        public string WorkShift { get; set; }
        public string BonusApplicable { get; set; }
        public string OTApplicable { get; set; }
        public string SpecifyLoanAccount { get; set; }
        public string LoanAccount { get; set; }
        public string SpecifyAdvanceAccount { get; set; }
        public string AdvanceAccount { get; set; }
        public string EmployeeGrade { get; set; }
        public string Jobstatus { get; set; }
        public ICollection<SalaryStrViewModel> SalModel { get; set; }
    }

    public class SalaryStrViewModel
    {
        public long? SalaryStrId { get; set; }
        public DateTime? EFDate { get; set; }
        public string EmpName { get; set; }
        public decimal? Amount { get; set; }
        public string Payhead { get; set; }
        public DateTime Date { get; set; }
    }
    public class EmployeeAttendanceSummarysViewModel
    {
        public long Id { get; set; }

        public long EmployeeId { get; set; }

        public long AttendanceType { get; set; }
        public int? Days { get; set; }
        public string Name { get; set; }
    }
    
}