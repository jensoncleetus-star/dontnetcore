using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Company
    {
        public long CompanyID { get; set; }

        [StringLength(200)]
        [Display(Name = "Company Name ")]
        [Required]
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

        [Display(Name = "Fax ")]
        [StringLength(150, MinimumLength = 5)]
        public string CPFax { get; set; }

        [Display(Name = "Main Branch")]
        public long? CPMainBranch { get; set; }

        [Display(Name = "Tax RegNo")]
        public string TRN { get; set; }


        [StringLength(500)]
        [Display(Name = "Address")]
        public string CPAddress { get; set; }

        // smtp Settings start
        [StringLength(150)]
        [Display(Name = "SMTP Email")]
        public string SMTPEmail { get; set; }

        [StringLength(150)]
        [Display(Name = "SMTP Host")]
        public string SMTPHost { get; set; }

        [StringLength(150)]
        [Display(Name = "SMTP Username")]
        public string SMTPUsername { get; set; }

        [StringLength(150)]
        [Display(Name = "SMTP Password")]
        public string SMTPPassword { get; set; }
        [Display(Name = "SMTP Port")]
        public long? SMTPPort { get; set; }
        // smtp Settings end

        public Boolean EnableSsl { get; set; }
        [StringLength(50)]
        [Display(Name = "SMS Sender Id")]
        public string smssenderid { get; set; }

        [StringLength(50)]
        [Display(Name = "SMS Username")]
        public string username { get; set; }

        [StringLength(150)]
        [Display(Name = "SMS Password")]
        public string password { get; set; }
        // invoice settings
        //[StringLength(8)]
        //[Display(Name = "Invoice Prefix")]
        //public string InvoicePrefix { get; set; }

        //[StringLength(8)]
        //[Display(Name = "Purchase Prefix")]
        //public string PurchasePrefix { get; set; }

        //[StringLength(8)]
        //[Display(Name = "Quotation Prefix")]
        //public string QuotationPrefix { get; set; }
        // invoice settings
        public string DbBackUpPath { get; set; }

        public long? SaleAccount { get; set; }
        public long? PurchaseAccount { get; set; }
        public long? SReturnAccount { get; set; }
        public long? PReturnAccount { get; set; }
        public long? SalaryAccount { get; set; }

        public long? TCAccount { get; set; }
        public long? TCSAccount { get; set; }
        public long? RentalAccount { get; set; }
        public long? RegdepositAccount { get; set; }

        public long? Broker { get; set; }
        public long? Landlord { get; set; }
        public long? Developer { get; set; }
        public long? Contractor { get; set; }
        public long? Tenant { get; set; }

        public long? BankAccount { get; set; }

        [Display(Name = "Website ")]
        [StringLength(150, MinimumLength = 8)]
        public string CPWebsite { get; set; }

        public DateTime? Payrolldate { get; set; }
    }
    public class CodePrefix
    {
        public int id { get; set; }
        [StringLength(20)]
        public string prefix { get; set; }
        public string section { get; set; }
        public int number { get; set; }
        public CodePrefix()
        {       
            number = 0;
        }
        //mannul or automatic
        //public string type { get; set; };

    }
    public class FinancialYear
    {
        public int id { get; set; }
        // automatic end date from satart date
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public long Company { get; set; }
        public Status Status { get; set; }
        public choice Active { get; set; }
        public FinancialYear()
        {
            Status = Status.active;
            Active = choice.Yes;
        }
    }

}