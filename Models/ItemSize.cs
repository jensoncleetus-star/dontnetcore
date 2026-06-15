using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class ItemSize
    {
        public long ItemSizeID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Size")]
        public string ItemSizeName { get; set; }

        public string Description { get; set; }

        public choice Editable { get; set; }
    }
    public class itemsizeprice
    {
        [Key]
        public long sizepriceid { get; set; }

        public long itemid { get; set; }

        public long sizeid { get; set; }

        public decimal price { get; set; }
    }
}