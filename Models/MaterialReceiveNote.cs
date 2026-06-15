using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class MaterialReceiveNote
    {
        [Key]
        public long MRId { get; set; }

        public long MRNo { get; set; }
        public string BillNo { get; set; }

        public DateTime MRDate { get; set; }

        public long Supplier { get; set; }
        public long? Cashier { get; set; }

        public string Type { get; set; }

        public string Note { get; set; }


        public int MRNItems { get; set; }
        public decimal MRNQuantity { get; set; }
        public long? materialcenter { get; set; }
        public DateTime? RequestedDate { get; set; }

        // mail times may use
        public int Mail { get; set; }
        //future use
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        public DateTime CreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }
        public string Remarks { get; set; }

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

        
        public string TermsCondition { get; set; }
    }
    public class MRNoteItem
    {
        [Key]
        public long MRNoteItemId { get; set; }

        public long MRNote { get; set; }
        public virtual MaterialReceiveNote MRNoteId { get; set; }
        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemQuantity { get; set; }
        public decimal ItemDiscount { get; set; }
        public string Remarks { get; set; }
        public string ItemNote { get; set; }

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }
        public long? Make { get; set; }

    }
    public class DummyMRNoteItem
    {
        [Key]
        public long DummyMRNoteItemId { get; set; }

        public long MRNote { get; set; }
        public long Item { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemQuantity { get; set; }
        public decimal ItemDiscount { get; set; }
        public string Remarks { get; set; }
        public string ItemNote { get; set; }

        public long? ProjectId { get; set; }
        public long? TaskId { get; set; }
        public long? Make { get; set; }

    }
    public class MRNotePOrder
    {
        public long MRNotePOrderId { get; set; }
        public long MRId { get; set; }
        public long POrderId { get; set; }
    }
}