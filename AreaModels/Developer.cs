using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Developer
    {
        public long DeveloperID { get; set; }

        public string DeveloperCode { get; set; }

        public string DeveloperName { get; set; }

        public long Contact { get; set; }
        public virtual Contact ContactID { get; set; }

        public decimal CreditLimit { get; set; }

        public int CreditPeriod { get; set; }

        public string Lattitude { get; set; }
        public string Longitude { get; set; }

        public string Location { get; set; }
        //public string TaxRegNo { get; set; }

        //public long? SalesPerson { get; set; }

        public string Remark { get; set; }

        public long Accounts { get; set; }
        public virtual Accounts AccountID { get; set; }


        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string IbanNo { get; set; }
        public string BranchName { get; set; }
        public string Swift { get; set; }

        public ProType Type { get; set; }
        public long EntryNo { get; set; }
    }
}