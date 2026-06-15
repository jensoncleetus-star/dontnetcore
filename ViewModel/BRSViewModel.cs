using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class BRSViewModel
    {
        public string AcName { get; set; }

        public DateTime? Date { get; set; }
        public string Type { get; set; }
        public string vchno { get; set; }
        public decimal? Deposits { get; set; }
        public decimal? Withdrawal { get; set; }
        public DateTime? ClearDate { get; set; }
        public decimal? BankAmount { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string Narration { get; set; }
        public long Id { get; set; }
        public int? ShowType { get; set; }
        public string CheType { get; set; }
        public DateTime? PDCDate { get; set; }
    }
    public class BRSFinalViewModel
    {
        public decimal OpeningBalance { get; set; }
        public decimal Balance { get; set; }
        public string blnceType { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? from { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime? to { get; set; }
        public string MainAccount { get; set; }
        public IEnumerable<BRSViewModel> BRS { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string footer { get; set; }

        public long Account { get; set; }
        public string AcName { get; set; }
        public int? ShowType { get; set; }

        public decimal? LedgerAmount { get; set; }
    }
    public class BRSStatement
    {
        public string Narration { get; set; }
        public decimal Amount { get; set; }
    }
    
}