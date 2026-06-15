using System;
using System.ComponentModel.DataAnnotations;
namespace QuickSoft.Models
{
    public class HireDetail
    {
        public long HireDetailId { get; set; }
        [Display(Name = "Start Date")]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }
        // sale,sale order,proforma,quotation,etc
        [StringLength(20)]
        public string Section { get; set; }
        public long Reference { get; set; }
        public long? HireType { get; set; }
    }
    public class HireRate
    {
        public int HireRateid { get; set; }
        public long type { get; set; }
        [DataType(DataType.Currency)]
        public decimal Rate { get; set; }
        public long ItemId { get; set; }
    }
    public class HireType
    {
        public int HireTypeId { get; set; }
        [StringLength(100)]
        [Display(Name = "Hire Type Name")]
        [Required]
        public string Name { get; set; }
        public int Period { get; set; }
        [StringLength(10)]
        public string PeriodType { get; set; }
        public string Note { get; set; }

        public Status Status { get; set; }
        public choice Editable { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long CreatedBranch { get; set; }
    }
}