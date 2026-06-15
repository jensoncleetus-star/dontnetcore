using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class ItemBrand
    {
        public long ItemBrandID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Brand")]
        public string ItemBrandName { get; set; }

        public string Description { get; set; }

        public choice Editable { get; set; }
    }
}