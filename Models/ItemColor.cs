using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class ItemColor
    {
        public long ItemColorID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Color")]
        public string ItemColorName { get; set; }


        public choice Editable { get; set; }

    }
}