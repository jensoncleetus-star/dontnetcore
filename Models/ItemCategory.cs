using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
     public class ItemCategory
    {
        
        public long ItemCategoryID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [Display(Name = "Category")]
        public string ItemCategoryName { get; set; }

        public long? Parent { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        //public string FileName { get; set; }

        public choice Editable { get; set; }
    }
}