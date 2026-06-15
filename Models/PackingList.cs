using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PackingList
    {
        public long PackinglistId { get; set; }
        public long Invoice { get; set; }
        [StringLength(15)]
        public string  BillNo { get; set; }
        public DateTime PLDate { get; set; }        
        public long Customer { get; set; }
        public long? Employee { get; set; }
        public string LPO { get; set; }

        public string Remarks { get; set; }
        public string TermAndCondition { get; set; }
        [StringLength(50)]
        public string HSCode { get; set; }

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
    }

    public class PLItem
    {
        public long PLItemId { get; set; }
        public long PackingListId { get; set; }
        public long Item { get; set; }
        public long? ItemUnit { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal? ItemDiscount { get; set; }
        public string ItemNote { get; set; }
        public decimal? Packet { get; set; }
        public decimal? MinQty { get; set; }
    }
}