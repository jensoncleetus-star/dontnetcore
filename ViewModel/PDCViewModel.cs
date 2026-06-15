using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class PDCViewModel
    {
        public long id { get; set; }
        
        public DateTime Date { get; set; }
        public string bankname { get; set; }
        public string Voucher { get; set; }

        public string Account { get; set; }
        
        public decimal? Issued { get; set; }
        
        public decimal? Receipt { get; set; }   
        public decimal? Journal { get; set; }
        public string check { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Branch { get; set; }
        public string CreatedBy { get; set; }
        public string remark { get; set; }
       public string pdctype { get; set; }
        public long? voucherid { get; set; }
        public int? withhold { get; set; }
        
    }
    public class OrderViewModel
    {
        public POSOrder orderdata { get; set; }
        public ICollection<POSOrderItemViewModel> orderitem { get; set; }
        public string OrderDate { get; set; }
        public OrderType OrderType { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public WalkinCustomer wCustomer { get; set; }

        public SEPayment salePayment { get; set; }
        public PosData posData { get; set; }
        public string fnval { get; set; }
        public decimal? ServiceExpense { get; set; }
        //public string Note { get; set; }
        public OrderViewModel()
        {
            ServiceExpense = 0;
        }
    }
    public class OrderDetailViewModel
    {
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }

        //takw away//delivery//dine in
        public string OrderType { get; set; }
        // 
        public string TableName { get; set; }

        public int? PeopleCount { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }

        public string WaiterName { get; set; }

        //payment//to kitchecn//hold//
        public string OrderStatus { get; set; }

        public int ItemCount { get; set; }

        public decimal Quantity { get; set; }

        public decimal SubTotal { get; set; }

        public decimal Tax { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal? Discount { get; set; }

        public decimal NetPayable { get; set; }

        public string OrderNote { get; set; }

        public string CreatedBy { get; set; }

        public List<SEItemViewModel> OrderItems { get; set; }
    }

    public class POSOrderItemViewModel
    {
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
        public string itemsize { get; set; }

        public decimal? ItemDiscount { get; set; }

        public string ItemNote { get; set; }

        public string Note { get; set; }

        public List<long> AddOnNote { get; set; }
        public long? AddOnItem { get; set; }

        public int? PrintCount { get; set; }

        public int? Prints { get; set; }
        public choice editable { get; set; }
    }
  
}