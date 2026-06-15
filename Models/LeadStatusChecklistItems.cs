using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class LeadStatusChecklistItems
    {
        [Key]
        public long id { set; get; }
        public long NextActionId { set; get; }
        public long chekListItemID { set; get; }
        public int AddNote { set; get; }
    }
}