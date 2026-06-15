using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class BOMViewModel
    {
        public long BOMId { get; set; }

        [Required]
        [Display(Name = "BOM Name")]
        public string BOMName { get; set; }

        [Display(Name = "Item To Produce")]
        public long ItemId { get; set; }
        [Required]
        public decimal Quantity { get; set; }

        public long? Unit { get; set; }
        [Display(Name = "Production Cost")]
        public decimal? Expense { get; set; }
        [Display(Name = "Labour Cost")]
        public decimal? Labourcost { get; set; }
        [Display(Name = "Material Cost")]
        public decimal? meterialcost { get; set; }
        public string ItemName { get; set; }
        public string ItemUnitName { get; set; }
        public string UserName { get; set; }

        public long Branch { get; set; }

        public BillOfMaterial bomdata { get; set; }
        public ICollection<BOMItem> bomitems { get; set; }

        public List<BOMItemViewModel> BOMItemvmodel { get; set; }
        [Display(Name = "Date")]
        public string BOMDate { get; set; }
        public long? MaterialCenter { get; set; }

    }
    public class BOMItemViewModel
    {
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public decimal Quantity { get; set; }
    }
    public class BOFViewModel
    {
        public long BOMId { get; set; }

        [Required]
        [Display(Name = "Bundle Offer Name")]
        public string BOMName { get; set; }

        [Display(Name = "Bundel Offer Item")]
        public long ItemId { get; set; }
        

        public long? Unit { get; set; }

        public decimal? Price { get; set; }


        public string ItemName { get; set; }
        public string ItemUnitName { get; set; }
        public string UserName { get; set; }

        public long Branch { get; set; }

        public BillOfMaterialsoffer bomdata { get; set; }
        public ICollection<BOMItem> bomitems { get; set; }

        public List<BOFItemViewModel> BOMItemvmodel { get; set; }
        [Display(Name = "Start Date")]
        public string BOMDateStart { get; set; }
        [Display(Name = "End Date")]
        public string BOMDateEnd { get; set; }
        public long? MaterialCenter { get; set; }

    }
    public class BOFItemViewModel
    {
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public decimal Quantity { get; set; }
    }
}