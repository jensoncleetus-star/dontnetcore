using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Bank
    {
        public long BankId { get; set; }

        public string AccountNo { get; set; }

        public string IbanNo { get; set; }
        public string BranchName { get; set; }
        public string Swift { get; set; }

        public long AccountId { get; set; }

        public Accounts Account { get; set; }

    }
}