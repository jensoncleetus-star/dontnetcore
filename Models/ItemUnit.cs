using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class ItemUnit
    {
        public long ItemUnitID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Unit")]
        public string ItemUnitName { get; set; }

        public string Description { get; set; }

        public choice Editable { get; set; }
    }
    public class UnitConversion
    {
        public long UnitConversionId { get; set; }

        
        public long MainUnitId { get; set; }
        public ItemUnit MainUnit { get; set; }
        public long SubUnitId { get; set; }
        public ItemUnit SubUnit { get; set; }
        public decimal ConFactor { get; set; }
    }
}