using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class StockTransfer
    {
        public long Id { get; set; }
        public long STNo { get; set; }

        public string Voucher { get; set; }
        public DateTime Date { get; set; }
        public long MCFrom { get; set; }
        public long MCTo { get; set; }
        public string Remarks { get; set; }
        public long Item { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }

        //Refernce Field Added
        [StringLength(50)]
        public string Ref1 { get; set; }
        [StringLength(50)]
        public string Ref2 { get; set; }
        [StringLength(50)]
        public string Ref3 { get; set; }
        [StringLength(50)]
        public string Ref4 { get; set; }
        [StringLength(50)]
        public string Ref5 { get; set; }

        public StockType StockType { get; set; }
    }

    public class StockTransferItem
    {
        public long Id { get; set; }

        public long StockTransferId { get; set; }

        public long Item { get; set; }

        public long? Unit { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }
    }

    public class DummyStkTrsItem
    {
        public long Id { get; set; }

        public long StockTransferId { get; set; }

        public long Item { get; set; }

        public long? Unit { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }
    }

    public class DummyStkTrsItem2
    {
        public long Id { get; set; }

        public long StockTransferId { get; set; }

        public long Item { get; set; }

        public long? Unit { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }
    }
    public class StockTransferBSundry
    {
        public long Id { get; set; }

        public long StockTransferId { get; set; }

        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }

        public int AmountType { get; set; }

        public int BsType { get; set; }

        public decimal? BsAmount { get; set; }

    }
}