using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class Accounts
    {
        public long AccountsID { get; set; }

        public string Name { get; set; }

        public string Alias { get; set; }

        public string PrintName { get; set; }

        public long Group { get; set; }
        public virtual AccountsGroup AccountsGroup { get; set; }

        // describe opening balance debit
        public decimal OpnBalance { get; set; }

        // describe opening balance Credit
        public decimal OpnBalanceCr { get; set; }

        // describe previous balance 

        public decimal PrevBalance { get; set; }
        public decimal PrevBalanceCr { get; set; }

        public string Note { get; set; }

        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public String CreatedBy { get; set; }

        public long Branch { get; set; }

        public Status Status { get; set; }

        public choice Editable { get; set; }

        [StringLength(25)]
        public String TRN { get; set; }

        public decimal PrvYearBalance { set; get; }
        public long? mc { get; set; }
        public long? shared { get; set; }
        public Accounts()
        {
            OpnBalance = 0;
            PrevBalance = 0;
            OpnBalanceCr = 0;
            Status = Status.active;
            Editable = 0;
        }
    }

    public class AccountsGroup
    {
        public long AccountsGroupID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Alias { get; set; }

        public choice Primary { get; set; }

        [Display(Name = "Parent")]
        public long Parent { get; set; }

        public choice Editable { get; set; }

        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }
        // AccountsGroup is read via Database.SqlQueryRaw (allchildGroups proc) whose ad-hoc mapper does
        // not allow navigations; NotMapped keeps the raw query working (it is never navigated in LINQ).
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public virtual Branch CreatedBranch { get; set; }
        public String CreatedBy { get; set; }

        public Status Status { get; set; }

        public AccountsGroup()
        {
            Editable = 0;
            CreatedDate = DateTime.Now;

        }
    }
    public class dummyAccountsTransactions
    {
        [Key]
        public long Id { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public long Account { get; set; }
        public virtual Accounts Accounts { get; set; }

        public string Purpose { get; set; }

        // show payed status {for PDC process}

        // used to find corresponding item
        public long reference { get; set; }

        public DC Type { get; set; }
        public DateTime? Date { get; set; }
        public DateTime CreatedDate { get; set; }

        public bool? Status { get; set; }

        [StringLength(250)]
        public string Narration { get; set; }

        public long? Project { get; set; }
        public long? ProTask { get; set; }
    }
    public class AccountsTransaction
    {
        [Key]
        public long Id { get; set; }

        public decimal Debit { get; set; }

        public decimal Credit { get; set; }

        public long Account { get; set; }
        public virtual Accounts Accounts { get; set; }

        public string Purpose { get; set; }

        // show payed status {for PDC process}

        // used to find corresponding item
        public long reference { get; set; }

        public DC Type { get; set; }
        public DateTime? Date { get; set; }
        public DateTime CreatedDate { get; set; }

        public bool? Status { get; set; }

        [StringLength(250)]
        public string Narration { get; set; }

        public long? Project { get; set; }
        public long? ProTask { get; set; }
    }
    public class sharedaccount
    {
        [Key]
        public long accsharedid { get; set; }
        public long accountid { get; set; }
        public long mcid { get; set; }
        public decimal percentage { get; set; }
    }
    public class passworddetail
    {
        [Key]
        public long PasswordDataId { get; set; }

        public string Title { get; set; }


        public long? LeadType { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string Notes { get; set; }
        public DateTime createddate { get; set; }
        public string createdby { get; set; }

      

    }
    public class filedocumentdetail
    {
        [Key]
        public long filedocumentDataId { get; set; }

        public string Title { get; set; }



        public string Notes { get; set; }
        public DateTime createddate { get; set; }
        public string createdby { get; set; }



    }

    //public class ReferenceAccount
    //{
    //    public long ReferenceAccountID { get; set; }
    //    public string Invoice { get; set; }
    //    public long Account { get; set; }
    //    public decimal Paid { get; set; }
    //    public decimal Amount { get; set; }
    //    public string Type { get; set; }
    //    public DateTime RADate { get; set; }
    //}
}