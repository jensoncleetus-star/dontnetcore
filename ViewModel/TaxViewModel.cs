using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QuickSoft.Models;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class TaxViewModel
    {

        //public long TaxID { get; set; }

        [Required]
        [StringLength(100)]
        public string TaxName { get; set; }

        //[StringLength(20)]
        //public string TaxType { get; set; }

        [Required]
        public decimal? Percentage { get; set; }

        // public Status Status { get; set; }




        //public long parent { get; set; }
        [Required]
        public long[] child { get; set; }

    }

    public class VatViewModel
    {
        public long? Id { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public decimal? Outward { get; set; }
        public decimal? Inward { get; set; }
        public decimal? Diff { get; set; }
        public decimal? TotalAmountOut { get; set; }
        public decimal? TotalAmountIn { get; set; }
        public decimal? Payable { get; set; }
        public decimal? Amount { get; set; }
        public IEnumerable<Ledger> Vat { get; set; }
        public string type { get; set; }
        public decimal? ExpenseAmount { get; set; }
        public decimal? ExpenseTax { get; set; }
    }
    public class VatTaxViewModel
    {
        public string particulars { get; set; }
        public decimal? amount { get; set; }
        public decimal? payable { get; set; }
        public string vchtype { get; set; }
        public long? vchno { get; set; }
        public DateTime? date { get; set; }
    }

    public class AccViewModel
    {
        public string Invoice { get; set; }
        public long? Id { get; set; }
    }
    //public class TaxEditViewModel
    //{
    //    public long TaxID { get; set; }

    //    [Required]
    //    [StringLength(100)]
    //    public string TaxName { get; set; }

    //    [Required]
    //    public decimal? Percentage { get; set; }
    //    public long[] child { get; set; }
    //}
}