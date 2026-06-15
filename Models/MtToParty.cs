using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    #region Material Issued To Party Section

    public class MtToParty
    {
        public long MtToPartyId { get; set; }

        public long Voucher { get; set; }

        public string VoucherNo { get; set; }

        public DateTime Date { get; set; }

        public long MC { get; set; }

        public long Party { get; set; }

        public MtToType MtToType { get; set; }

        public decimal TotalQuantity { get; set; }

        public decimal TotalAmount { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }

    public class MtToPartyItems
    {
        public long Id { get; set; }

        public long Item { get; set; }

        public string ItemNote { get; set; }

        public long? Unit { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }

        public long MtToParty { get; set; }
    }

    public class MtToPartyBSundry
    {
        public long Id { get; set; }

        public long MtToParty { get; set; }

        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }

        public int AmountType { get; set; }

        public int BsType { get; set; }

        public decimal? BsAmount { get; set; }

    }
    #endregion


    #region Material Received From Party Section

    public class MtFromParty
    {
        public long MtFromPartyId { get; set; }

        public long Voucher { get; set; }

        public string VoucherNo { get; set; }

        public DateTime Date { get; set; }

        public long MC { get; set; }

        public long Party { get; set; }

        public MtFromType MtFromType { get; set; }

        public decimal TotalQuantity { get; set; }

        public decimal TotalAmount { get; set; }

        public string Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public choice editable { get; set; }
        public Status Status { get; set; }
    }

    public class MtFromPartyItems
    {
        public long Id { get; set; }

        public long Item { get; set; }

        public string ItemNote { get; set; }

        public long? Unit { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal Amount { get; set; }

        public long MtFromParty { get; set; }
    }

    public class MtFromPartyBSundry
    {
        public long Id { get; set; }

        public long MtFromParty { get; set; }

        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }

        public int AmountType { get; set; }

        public int BsType { get; set; }

        public decimal? BsAmount { get; set; }

    }

    #endregion
}