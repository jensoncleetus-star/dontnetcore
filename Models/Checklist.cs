using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Checklist
    {
        public long ChecklistId { get; set; }

        public long Stage { get; set; }

    }

    public class ChecklistItem
    {
        public long Id { get; set; }

        public long Checklist { get; set; }

        [StringLength(250)]
        public string ListName { get; set; }

        public bool AddNote { get; set; }

    }
    public class ScopeOfWork
    {   [Key]
        public long ChecklistId { get; set; }

        public long Stage { get; set; }


    }

    public class ScopeOfWorkItem
    {
        [Key]
        public long Id { get; set; }

        public long Checklist { get; set; }

        [StringLength(250)]
        public string ListName { get; set; }

        public bool AddNote { get; set; }

    }
    


}