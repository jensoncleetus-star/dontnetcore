using System;
using System.ComponentModel.DataAnnotations;
// Material Centers
namespace QuickSoft.Models
{
    public class MC
    {
        public long MCId { get; set; }
        [StringLength(50)]
        [Display(Name = "Code")]
        public string MCCode { get; set; }
        [StringLength(100)]
        [Display(Name = "Name")]
        public string MCName { get; set; }
        [Display(Name = "Note")]
        public string Note { get; set; }
        public Status Status { get; set; }

        public choice Editable { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public Branch Branch { get; set; }
        public long CreatedBranch { get; set; }

        [Display(Name = "Assigned User")]
        public string AssignedUser { get; set; }
        public MC()
        {
            Editable = choice.Yes;
            Status = Status.active;
        }
    }
    public class AdditionalMc
    {
        [Key]
        public long NewMcId { get; set; }
        public string UserId { get; set; }
        public long McId { get; set; }
        public string McName { get; set; }
    }
    public class purchaseapproval
    {
        [Key]
        public long NewMcId { get; set; }
        public string UserId { get; set; }
        public long McId { get; set; }
        public string McName { get; set; }
    }
    public class TaskGroup
    {
        [Key]
        public long TaskGroupId  { get; set; }
        public long  TaskStatusId { get; set; }
        public long TaskTypeId { get; set; }
        public string TaskTypeName { get; set; }
    }

}