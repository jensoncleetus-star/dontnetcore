using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class JobCardViewModel
    {
        public long? JobCardId { get; set; }

        public long JCNo { get; set; }
        [Required(ErrorMessage = "JobCard No is required")]
        [Display(Name = "JobCard No.")]
        public string JobCardNo { get; set; }

        [Display(Name = "Date")]
        [Required]
        public string JCDate { get; set; }

        public long Customer { get; set; }

        //employee
        public long? Mechanic { get; set; }

        //employee
        [Display(Name = "Received By")]
        public long? ReceivedBy { get; set; }

        [Display(Name = "PWC Model")]
        public string PWCModel { get; set; }

        [Display(Name = "PWC Details")]
        public string Details { get; set; }

        public decimal? TotalAmount { get; set; }

        public ICollection<JCItem> jcitems { get; set; }
        public List<JCItemViewModel> JCItem { get; set; }
        public string UserName { get; set; }
        public string CustName { get; set; }
        public string MechName { get; set; }
        public string RecName { get; set; }

        public DateTime? JobCDate { get; set; }

        public string action { get; set; }
        public long Branch { get; set; }
        public string ApprovedBy { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

    }
    public class JCItemViewModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public decimal ItemTotalAmount { get; set; }
    }
}