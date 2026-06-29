using System.ComponentModel.DataAnnotations;

namespace QuickSoftPilot.Models
{
    // Ported as-is from the legacy QuickSoft.Models.ItemCategory.
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

        public choice Editable { get; set; }
    }

    // Legacy Yes/No enum (from General.cs).
    public enum choice
    {
        No = 0,
        Yes = 1
    }
}
