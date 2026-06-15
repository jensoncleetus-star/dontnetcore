using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Unassemble
    {
        public long UnassembleId { get; set; }
        public long EntryNo { get; set; }
        public string VoucherNo { get; set; }
        public DateTime PEDate { get; set; }
        public long? MaterialCenter { get; set; }

        // extra note option
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
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

        public long? Project { get; set; }
        public long? ProTask { get; set; }

    }
    public class ConsumedItems
    {
        [Key]
        public long ConsumedID { get; set; }
        public long BOM { get; set; }
        public long Unassemble { get; set; }
        public long Item { get; set; }
        public long? Unit { get; set; }
        public decimal Qty { get; set; }
        // unit price
        public decimal Price { get; set; }
        // total amount - puprice*pqty
        public decimal Amount { get; set; }

        //public long? BatchStockID { get; set; }
    }

    public class UnassembleItem
    {
        [Key]
        public long UnItemId { get; set; }
        public long Unassemble { get; set; }
        public Unassemble UnassembleId { get; set; }
        public long ItemId { get; set; }
        public long? Unit { get; set; }
        public decimal Quantity { get; set; }
        // unit price
        public decimal PPrice { get; set; }
        // total amount - puprice*pqty
        public decimal PAmount { get; set; }

        //public long? BatchStockID { get; set; }
    }
}