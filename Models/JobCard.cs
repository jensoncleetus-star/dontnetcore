using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class JobCard
    {
        public long JobCardId { get; set; }

        public long JCNo { get; set; }
        [Required]
        public string JobCardNo { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime JCDate { get; set; }

        public long Customer { get; set; }

        //employee
        public long? Mechanic { get; set; }

        //employee
        public long? ReceivedBy { get; set; }

        public string PWCModel { get; set; }

        public string Details { get; set; }

        public decimal? TotalAmount { get; set; }

        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }

        //Refernce Field Added
        [StringLength(50)]
        public string Ref1 { get; set; }
        [StringLength(50)]
        public string Ref2 { get; set; }
        [StringLength(50)]
        public string Ref3 { get; set; }
        [StringLength(50)]
        public string Ref4 { get; set; }
        [StringLength(50)]
        public string Ref5 { get; set; }

    }
    public class JCItem
    {
        public long JCItemId { get; set; }

        public long JobCard { get; set; }
        public virtual JobCard JobCardId { get; set; }

        public long Item { get; set; }
        public virtual Item ItemId { get; set; }
        public decimal ItemTotalAmount { get; set; }
    }

    public class JobCardItemSetting
    {
        [Key]
        public long JCItemId { get; set; }

        public long Item { get; set; }
        public virtual Item ItemId { get; set; }
        public decimal ItemTotalAmount { get; set; }
    }
}