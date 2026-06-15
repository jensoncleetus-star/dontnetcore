using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class BoqViewModel
    {
        public long BoqId { get; set; }
       
        public string QuotNo { get; set; }

        

        public long? BillNo { get; set; }

        [Display(Name = "Item To Produce")]




        public string UserName { get; set; }

       
        public long ConTypeId { get; set; }
        public string ConType { get; set; }

        public long? Customer { get; set; }

        public long? SalesExecutive { get; set; }
        public BillOfQty bomdata { get; set; }
        public ICollection<BoqItem> boqitems { get; set; }


        public List<BoqItemViewModel> BoqItemvmodel { get; set; }
        [Display(Name = "Date")]
        public string BoqDate { get; set; }

        public long PrintLayout { get; set; }


    }
    public class BoqItemViewModel
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public long ItemUnit { get; set; }
        public decimal Quantity { get; set; }

        public string ItemNote { get; set; }
    }
}