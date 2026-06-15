using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class ItemImage
    {
        public long ItemImageID { get; set; }

        [Required]
        public long ItemID { get; set; }

        [Required]
        public string FileName { get; set; }

        public int Status { get; set; }

        public virtual Item Items { get; set; }
        
    }
}