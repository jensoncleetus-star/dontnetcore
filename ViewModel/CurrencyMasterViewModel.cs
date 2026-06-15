using QuickSoft.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.ViewModel
{
    public class CurrencyMasterViewModel
    {       
       
        [Required]
        public string CurrencyCode { get; set; }

        [StringLength(50)]
        public string Description { get; set; }

        [Required]
        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal ConvertionRate { get; set; }

        [StringLength(25)]
        [Required]
        public string Fraction { get; set; }

        [StringLength(10)]
        public string Symbol { get; set; }
       
        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal? MinConvertionRate { get; set; }
        
        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal? MaxConvertionRate { get; set; }

        public long Branch { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
}