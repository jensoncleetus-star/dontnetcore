using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class PrefixMaster
    {
        public long Id { get; set; }

        [StringLength(20)]
        [Required]
        public string PrefixCode { get; set; }

        [StringLength(10)]
        public string LastNo { get; set; }

        public string Description { get; set; }
       
        public long Currency { get; set; }

        [StringLength(20)]
        public string CCCode { get; set; }

        [StringLength(10)]
        public string ConRate { get; set; }

        public long Brand { get; set; }

        public long Category { get; set; }

        [StringLength(30)]
        public string Type { get; set; }

        [StringLength(50)]
        public string Country { get; set; }
        
        public long Branch { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }
}