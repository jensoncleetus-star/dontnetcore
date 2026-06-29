using System.ComponentModel.DataAnnotations;

namespace QuickSoftPilot.ViewModels
{
    public class ItemCategoryViewModel
    {
        public long ItemCategoryID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        [Display(Name = "Category name")]
        public string ItemCategoryName { get; set; }

        [Display(Name = "Parent category")]
        public long? Parent { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
