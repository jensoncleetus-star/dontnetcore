using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class StockTransferViewModel
    {

        public long Id { get; set; }
        public string Voucher { get; set; }
        public DateTime Date { get; set; }
        public long MCFrom { get; set; }
        public long MCTo { get; set; }
        public string Remarks { get; set; }
        public long Item { get; set; }
        public decimal TotalAmount { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public string From { get; set; }
        public string To { get; set; }

        public List<StockTransferItemViewModel> STItem { get; set; }

        public List<StockTransferBSundryViewModel> STbs { get; set; }

        public List<ApprovalViewModel> Emp { get; set; }
        public StockType StockType { get; set; }

        public string FinancialDate { get; set; }
        public string StkDate { get; set; }
    }

    public class StockTransferItemViewModel
    {
        public long Id { get; set; }

        public long Item { get; set; }

        public long? Unit { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }

        public string Itemname { get; set; }

        public string Unitname { get; set; }
    }

    public class StockTransferBSundryViewModel
    {
        public long Id { get; set; }

        public long StockTransferId { get; set; }

        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }

        public int AmountType { get; set; }

        public int BsType { get; set; }

        public decimal? BsAmount { get; set; }

        public long STNo { get; set; }

        public ICollection<StockTransferBSundry> sebsundrys { get; set; }

        public string BType { get; set; }

        public string BAmount { get; set; }
        public string name { get; set; }
    }
}
