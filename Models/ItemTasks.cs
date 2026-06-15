using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class ItemTasks
    {
       [Key]
        public long TaskId { get; set; }
        
        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public long Unit { get; set; }
        public long protaskid { get; set; }
        public int? invoiced { get; set; }
        public long itemtasklistid { get; set; }
        public long? seitemid { get; set; }



    }
    public class itemtasklist
    {
        [Key]
        public long fieldmcid { get; set; }

        public long protaskid { get; set; }
        public string userid { get; set; }
        public long mcfrom { get; set; }
        public long itemid { get; set; }
       public decimal qty { get; set; }
        public long unit { get; set; }
        public long stocktransferitid { get; set; }

    }

}