using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class CutomerDocument
    {
        [Key]
        public long DocumnetId { get; set; }

        public long CutomerID { get; set; }

        public string DoucumentType { get; set; }

        public DateTime Expiry { get; set; }

        public string Notes { get; set; }

        public string FilePath { get; set; }
    }
}