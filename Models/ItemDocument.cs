using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class ItemDocument
    {
        public long ItemDocumentID { get; set; }

        [Required]
        public long ItemID { get; set; }

        [Required]
        public string FileName { get; set; }

        public int Status { get; set; }

        public virtual Item Items { get; set; }
    }
}