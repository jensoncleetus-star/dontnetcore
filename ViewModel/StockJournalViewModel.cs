using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class StockJournalViewModel
    {
        public long? Id { get; set; }

        public long SJNo { get; set; }

        public DateTime SJDate { get; set; }

        public string Voucher { get; set; }

        public long MCFrom { get; set; }

        public long MCTo { get; set; }

        public long Employee { get; set; }

        [StringLength(200)]
        public string Description { get; set; }       

        public decimal GeneratedTotal { get; set; }

        public decimal ConsumedTotal { get; set; }

        public List<StockJournalViewModel> SJItem { get; set; }

        public long GeneratedItem { get; set; }

        public decimal GeneratedItemQuantity { get; set; }

        [StringLength(10)]
        public string GeneratedUnit { get; set; }

        public decimal GeneratedPrice { get; set; }

        public decimal GeneratedAmount { get; set; }

        public long ConsumedItem { get; set; }

        public decimal ConsumedItemQuantity { get; set; }

        [StringLength(10)]
        public string ConsumedUnit { get; set; }

        public decimal ConsumedPrice { get; set; }

        public decimal ConsumedAmount { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
    }
}