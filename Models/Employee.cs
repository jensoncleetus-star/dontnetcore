using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Employee
    {
        public long EmployeeId { get; set; }
        public bool appaccessonly { get; set; }
        [Display(Name = "Is Employee?")]
        public bool isemployee { get; set; }
        public string EMPCode { get; set; }

        public Prefix Prefix { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public decimal? perhour { get; set; }
        public string Alias { get; set; }

        public Gender Gender { get; set; }
        public string PassportNo { get; set; }
        public string OtherIdNo { get; set; }
        public string ImgFileName { get; set; }
        public long? BranchID { get; set; }
        public long? DesignationID { get; set; }
        public long? DepartmentID { get; set; }

        // permenant address reference
        public long PAddress { get; set; }
        // current address reference
        public long CAddress { get; set; }
        // check whether this employee is an user?
        public bool UserStatus { get; set; }
        public String UserId { get; set; }
        public int Status { get; set; }
        public choice sync_status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Note { get; set; }

        public DateTime? JoinDate { get; set; }
        public DateTime? LeavingDate { get; set; }
        public string ReasonForLeaving { get; set; }
        public JobStatus JobStatus { get; set; }

        public DateTime? DutyResumeDate { get; set; }

        public DateTime? LastDutyResumeDate { get; set; }
        public int? TotalWorkingDays { get; set; }
        public int? NoDaysWorked { get; set; }
        public int? profileupdate { get; set; }
        public int? profileupdateaccept { get; set; }
        public string profileupdatenote { get; set; }
      
    }
    public class employeetimesheet
    {
        public long entryid { get; set; }
        public long protaskid { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? servocedatefrom { get; set; }
        public DateTime? servocedateto { get; set; }
        public DateTime?  endtime { get; set; }
        public DateTime? starttime { get; set; }
        public DateTime? entrydate { get; set; }
        public DateTime? attdate { get; set; }
        public int? hours { get; set; }
        public int? minute { get; set; }
        public int? leavestatus { get; set; }
        public decimal? totlhour { get; set; }
        public decimal? totalminute { get; set; }
        public string strtotalhour { get; set; }
        public string strtotalminute { get; set; }
        public string userid { get; set; }


    }
    public class employeetimesheetabsend
    {
        public long? absantcount { get; set; }
        public long? presentcount { get; set; }
        public string EmployeeName { get; set; }
        public string userid { get; set; }
    }
    public class monthtimesheet
    {
        public string startdate { get; set; }
        public string enddate { get; set; }
        public string emplyeename { get; set; }
        public string day1 { get; set; }
        public string day2 { get; set; }
        public string day3 { get; set; }
        public string day4 { get; set; }
        public string day5 { get; set; }
        public string day6 { get; set; }
        public string day7 { get; set; }
        public string day8 { get; set; }
        public string day9 { get; set; }
        public string day10 { get; set; }
        public string day11 { get; set; }
        public string day12 { get; set; }
        public string day13 { get; set; }
        public string day14 { get; set; }
        public string day15 { get; set; }
        public string day16 { get; set; }
        public string day17 { get; set; }
        public string day18 { get; set; }
        public string day19 { get; set; }
        public string day20 { get; set; }
        public string day21 { get; set; }
        public string day22 { get; set; }
        public string day23 { get; set; }
        public string day24 { get; set; }
        public string day25 { get; set; }
        public string day26 { get; set; }
        public string day27 { get; set; }
        public string day28 { get; set; }
        public string day29 { get; set; }
        public string day30 { get; set; }
        public string day31 { get; set; }




    }
    public class employeetimesheetlist
    {
        public List<employeetimesheet> et { get; set; }
    }

    public class employeetimesheetlistabs
    {
        public List<employeetimesheetabsend> et { get; set; }
    }


    public class EmployeePersonal
    {
        public long EmployeePersonalId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime? DOB { get; set; }
        public string MaritalStatus { get; set; }
        public string BloodGroup { get; set; }
        public string Nationality { get; set; }
    }
    public class EmployeeBank
    {
        public long EmployeeBankId { get; set; }
        public long EmployeeId { get; set; }
        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string IbanNo { get; set; }
        public string BranchName { get; set; }
        public string Swift { get; set; }
    }
    public class EmployeeWorkDetail
    {
        public long EmployeeWorkDetailId { get; set; }
        public long EmployeeId { get; set; }

        public SalaryMode SalaryMode { get; set; }
        public long? WorkShift { get; set; }
        public bool BonusApplicable { get; set; }
        public bool OTApplicable { get; set; }
        public bool SpecifyLoanAccount { get; set; }
        public long? LoanAccount { get; set; }
        public bool SpecifyAdvanceAccount { get; set; }
        public long? AdvanceAccount { get; set; }
        public long? EmployeeGrade { get; set; }

        public long? EmployeeAccount { get; set; }
        public bool AnnualLeaveApplicable { get; set; }
    }

    public class EmployeeGrade
    {
        public long EmployeeGradeId { get; set; }
        [Required]
        [Display(Name = "Grade Name")]
        public string GradeName { get; set; }

        public string Note { get; set; }
    }
    public class EmpGradeSalaryDetail
    {
        public long EmpGradeSalaryDetailId { get; set; }
        public long EmployeeGradeId { get; set; }

        public DateTime EffectFrom { get; set; }
        public long Payhead { get; set; }
        public decimal? Rate { get; set; }
    }

    public class EmployeeEducation
    {
        [Key]
        public long EmpEducationId { get; set; }

        public long EmployeeId { get; set; }
        public string CourseTitle { get; set; }
        public string Specialization { get; set; }
        public string Institute { get; set; }
        public string BoardOrUniversity { get; set; }
        // string CourseType { get; set; }
        public string PassYear { get; set; }
        public decimal?	Percentage  { get; set; }
    }
    public class EmployeeProfession
    {
        [Key]
        public long EmployeeProId { get; set; }

        public long EmployeeId { get; set; }
        public string Organization { get; set; }
        public string Designation { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Responsibility { get; set; }
        public string Skills { get; set; }
    }

    public class EmployeeDocument
    {
        [Key]
        public long EmployeeDocumentId { get; set; }

        public long EmployeeId { get; set; }

        public string DocumentName { get; set; }
        public string DocumentNo { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Note { get; set; }
        public string Attachments { get; set; }
        public string PersonalNo { get; set; }

    }

    public class EmployeeAttendanceSummary
    {
        public long Id { get; set; }

        public long EmployeeId { get; set; }

        public long AttendanceType { get; set; }
        public int? Days { get; set; }

    }

}