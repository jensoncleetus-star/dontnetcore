using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
namespace QuickSoft.ViewModel
{
    public class ItemTaskViewModel
    {
        //
        public long? taskid { get; set; }
        public long? MaterialId { get; set; }
       
        public string TaskName { get; set; }
        public string TaskDate { get; set; }
        public long ItemId { get; set; }
        public long Quantity { get; set; }


        public long Unit { get; set; }

        public long protaskid { get; set; }
        public ItemTaskMaster bomdata { get; set; }
        public ICollection<ItemTasks> bomitems { get; set; }


    }
}