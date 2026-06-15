using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class PrefixMasterViewModel
    {
        public long Id { get; set; }

        [StringLength(20)]
        [Required]
        public string PrefixCode { get; set; }
       
        public string LastNo { get; set; }

        public string Description { get; set; }

        public long LNo { get; set; }
        
        public long Currency { get; set; }

        public string CCCode { get; set; }

        [Required]
        public string ConRate { get; set; }

        public long Brand { get; set; }

        public long Category { get; set; }

        public string Type { get; set; }

        public string Country { get; set; }

        public long Branch { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
    }
}