using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    // Operational property inspection / maintenance task — schedule, assign to a contractor,
    // and track status (Pending -> In Progress -> Completed). Isolated/additive: separate from the
    // financial Maintenance contract entity and never posts to the GL.
    public class PropertyMaintenanceTask
    {
        [Key]
        public long ID { get; set; }
        public string Title { get; set; }
        public string TaskType { get; set; }     // Inspection / Maintenance / AMC Service / Cleaning / Repair
        public long Property { get; set; }
        public long Unit { get; set; }            // 0 = whole property
        public long Contractor { get; set; }      // assignee (0 = unassigned)
        public DateTime ScheduledDate { get; set; }
        public string Priority { get; set; }      // Low / Normal / High
        public int Status { get; set; }           // 0 = Pending, 1 = In Progress, 2 = Completed
        public string Notes { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
