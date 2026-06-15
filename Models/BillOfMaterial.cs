using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class BillOfMaterial
    {
        [Key]
        public long BOMId { get; set; }
        public string BOMName { get; set; }
        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public long? Unit { get; set; }
        public decimal? Expense { get; set; }
        public decimal? Labourcost { get; set; }
        public decimal? meterialcost { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public Status Status { get; set; }

        public DateTime? BOMDate { get; set; }
        public long? MaterialCenter { get; set; }


        public BillOfMaterial()
        {
            Expense = 0;
        }
    }
    public class BOMItem
    {
        public long BOMItemId { get; set; }
        public long BOMId { get; set; }

        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public long? Unit { get; set; }
    }

    public class BillOfMaterialsoffer
    {
        [Key]
        public long BOMOfferId { get; set; }
        public string BOMName { get; set; }
        public long ItemId { get; set; }
      
        public long? Unit { get; set; }
        public decimal? Price { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
       
        public Status Status { get; set; }

        public DateTime? BOMDateStart { get; set; }
        public DateTime? BOMDateEnd { get; set; }
        public long? MaterialCenter { get; set; }

  
        public BillOfMaterialsoffer()
        {
            Price = 0;
        }
    }
    public class BOMItemsoffer
    {
        [Key]
        public long BOMItemId { get; set; }
        public long BOMOfferId { get; set; }

        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public long? Unit { get; set; }
    }

    public class BillOfQty
    {
        [Key]
        public long BoqId { get; set; }

        
        public long? BillNo { get; set; }

        public string QuotNo { get; set; }

      

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
      

        public long? Customer { get; set; }

        public long SalesExecutive { get; set; }
       
        public Status Status { get; set; }

        public DateTime BOQDate { get; set; }




    }
    public class BoqItem
    {
        public long BoqItemId { get; set; }
        public long BoqId { get; set; }

        public long ItemId { get; set; }
        public decimal Quantity { get; set; }
        public long? Unit { get; set; }

        public string ItemNote { get; set; }
    }







}