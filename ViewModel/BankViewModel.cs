using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class BankViewModel
    {

        public long BankId { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Account No.")]
        public string AccountNo { get; set; }

        [Display(Name = "IBAN No.")]
        public string IbanNo { get; set; }

        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }
        public string Swift { get; set; }

        public decimal OpnBalance { get; set; }

        public decimal OpnBalanceCr { get; set; }

        public string Note { get; set; }

        public long Branch { get; set; }

        [DefaultValue(true)]
        public Status Status { get; set; }

        public DC DC { get; set; }

        public string Alias { get; set; }
        public string StatusName { get; set; }
        public string opbal { get; set; }
    }
}