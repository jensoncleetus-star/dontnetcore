using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class POSOrder
    {
        public long POSOrderId { get; set; }



        public long EntryNo { get; set; }

        public string OrderNo { get; set; }

        public string BillNo { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        //takw away//delivery//dine in
        public OrderType OrderType { get; set; }
        // 
        public long? TableId { get; set; }

        public int? PeopleCount { get; set; }

        public CustomerType CustomerType { get; set; }
        public long? Customer { get; set; }


        public long? WaiterId { get; set; }

        //payment//to kitchecn//hold//
        public OrderStatus OrderStatus { get; set; }

        public int ItemCount { get; set; }

        public decimal Quantity { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Tax { get; set; }
        public decimal dcharge { get; set; }
        public decimal tendering { get; set; }
        public decimal TaxAmount { get; set; }

        public decimal? Discount { get; set; }

        public decimal NetPayable { get; set; }

        public string OrderNote { get; set; }

        public string custName { get; set; }
        public string custMob { get; set; }


        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public Status Status { get; set; }

        public virtual ICollection<POSOrderItem> OrderItems { get; set; }

        public bool? taxAFdisc { get; set; }

    }
    public class POSOrderItem
    {
        [Key]
        public long OrderItemId { get; set; }

        public long OrderId { get; set; }

        public long Item { get; set; }


        public long? ItemUnit { get; set; }
        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public decimal? ItemDiscount { get; set; }

        public string ItemNote { get; set; }

        public string Note { get; set; }

        public int? PrintCount { get; set; }

        public int? Prints { get; set; }
        public choice editable { get; set; }
        public POSOrderItem()
        {
            editable = choice.Yes;
        }
    }
    public class Table
    {
        public long TableId { get; set; }

        public string TableName { get; set; }

        public long? AreaId { get; set; }

        public int? MaxSeats { get; set; }

        public TableStatus TableStatus { get; set; }
        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Branch CreatedBranch { get; set; }
        public Status Status { get; set; }
    }

    public class Area
    {
        public long AreaId { get; set; }
        [Required]
        [Display(Name = "Area Name")]
        public string AreaName { get; set; }
    }

    public class InstantDiscount
    {
        public long InstantDiscountId { get; set; }

        public long ItemId { get; set; }

        public Item Items { get; set; }

        public decimal OfferPrice { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public long Branch { get; set; }

        public Branch CreatedBranch { get; set; }

        public Status Status { get; set; }
    }

}