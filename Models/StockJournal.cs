using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class StockJournal
    {
        public long Id { get; set; }
        //voucher no
        public long SJNo { get; set; }

        public DateTime SJDate { get; set; }

        [StringLength(20)]
        public string Voucher { get; set; }

        public long MCFrom { get; set; }

        public long MCTo { get; set; }

        public long Employee { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public decimal GeneratedAmount { get; set; }

        public decimal ConsumedAmount { get; set; }

        // common fields
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice Editable { get; set; }
        public Status Status { get; set; }

        //Refernce Field Added
        [StringLength(50)]
        public string Ref1 { get; set; }
        [StringLength(50)]
        public string Ref2 { get; set; }
        [StringLength(50)]
        public string Ref3 { get; set; }
        [StringLength(50)]
        public string Ref4 { get; set; }
        [StringLength(50)]
        public string Ref5 { get; set; }
    }

    public class SJItemGenerate
    {
        public long Id { get; set; }

        public long StockJournal { get; set; }

        public long Item { get; set; }

        public decimal ItemQuantity { get; set; }

        [StringLength(10)]
        public string Unit { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }

    }

    public class SJItemConsume
    {
        public long Id { get; set; }

        public long StockJournal { get; set; }

        public long Item { get; set; }

        public decimal ItemQuantity { get; set; }

        [StringLength(10)]
        public string Unit { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }

    }

    
}