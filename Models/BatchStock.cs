using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class BatchStock
    {
        [Key]
        public long ID { get; set; }
        [StringLength(200)]
        public string BatchNo { get; set; }
        public DateTime? MFG { get; set; }
        public DateTime? EXP { get; set; }
        public decimal StockIn { get; set; }
        public decimal StockOut { get; set; }
        public long Item { get; set; }
        public long? Unit { get; set; }
        public long Reference { get; set; }
        [StringLength(10)]
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal Cost { get; set; }
        public long Order { get; set; }
        public BatchStock()
        {
            StockIn = 0;
            StockOut = 0;
            Cost = 0;
        }
    }
}