using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class AccountsViewModel
    {
        public long AccountsID { get; set; }

        [Display(Name = "Account Name")]
        public string Name { get; set; }

        public string GroupName { get; set; }

        public string Statusname { get; set; }

        public string opbal { get; set; }

        public string Alias { get; set; }

        public string PrintName { get; set; }

        public long Group { get; set; }
        public virtual AccountsGroup AccountsGroup { get; set; }
        
        public decimal OpnBalance { get; set; }
        
        public decimal OpnBalanceCr { get; set; }
        

        public decimal PrevBalance { get; set; }
        public decimal PrevBalanceCr { get; set; }

        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public String CreatedBy { get; set; }

        public choice Editable { get; set; }

        public Status Status { get; set; }
        public DC DC { get; set; }

        public string TRN { get; set; }
        public long? ddlMC { get; set; }
    }

    public class ReferenceAccountViewModel
    {
        public string Invoice { get; set; }
        public long? Account { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Amount { get; set; }
        public string Type { get; set; }
        public string RADate { get; set; }
    }
}