using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class Rack
    {
        [Key]
        public long RackId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Rack Name")]
        public string RackName { get; set; }
    }
    public class Shelf
    {
        [Key]
        public long ShelfId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "shelf Name")]
        public string shelfName { get; set; }
    }
    public class rackmaterialcentre
    {
        [Key]
        public long rackmcid { get; set; }
        public long rackid { get; set; }
        public long shelfid { get; set; }
        public long mcid { get; set; }

    }
    public class shelfstockmovement
    {
        [Key]
        public long stockmovementid { get; set; }
        public long rackmciid { get; set; }
        public long referenceid { get; set; }
        public string purpose { get; set; }


        public long itemid { get; set; }
        public long unitid { get; set; }


        public decimal qty { get; set; }


        public string createdby { get; set; }


      public DateTime createddate { get; set; }
   
    }
    public class ShelfStockTransfer
    {
        [Key]
        public long shelftransferId { get; set; }
        public string VoucherNo { get; set; }
        public long FromRackMcId { get; set; }
        public long ToRackMcId { get; set; }
        public string transactionType { get; set; }
        public string createdBy { get; set; }
        public DateTime createdDate { get; set; }
    }
    public class SSTItem
    {
        [Key]
        public long STItemId { get; set; }
        public long shelfTransfer { get; set; }
        public long item { get; set; }
        public long? itemUnit { get; set; }
        public decimal itemQuantity { get; set; }
    }
    }