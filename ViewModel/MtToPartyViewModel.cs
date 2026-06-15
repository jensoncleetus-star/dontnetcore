using QuickSoft.Models;
using System;
using System.Collections.Generic;

namespace QuickSoft.ViewModel
{
    #region Issued
    public class MtToPartyViewModel
    {
        public long Voucher { get; set; }
        public string VoucherNo { get; set; }
        public DateTime Date { get; set; }
        public long MC { get; set; }
        public long Party { get; set; }
        public MtToType MtToType { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; }
        public List<MtToPartyItemsViewModel> MtItem { get; set; }

        public List<MtToPartyBSundryViewModel> SEbs { get; set; }
        public string PartyName { get; set; }
        public string PartyCode { get; set; }
    }

    public class MtToPartyItemsViewModel
    {
        public long Item { get; set; }
        public string ItemNote { get; set; }
        public long? Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public long MtToParty { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string UnitName { get; set; }
    }

    public class MtToPartyBSundryViewModel
    {
        public long MtToParty { get; set; }
        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string BillSundry { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<MtToPartyBSundry> sebsundrys { get; set; }
    }

    #endregion

    #region Received

    public class MtFromPartyViewModel
    {
        public long Voucher { get; set; }
        public string VoucherNo { get; set; }
        public DateTime Date { get; set; }
        public long MC { get; set; }
        public long Party { get; set; }
        public MtFromType MtFromType { get; set; }
        public decimal TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; }
        public List<MtFromPartyItemsViewModel> MtItem { get; set; }

        public List<MtFromPartyBSundryViewModel> SEbs { get; set; }
        public string PartyName { get; set; }
        public string PartyCode { get; set; }
    }

    public class MtFromPartyItemsViewModel
    {
        public long Item { get; set; }
        public string ItemNote { get; set; }
        public long? Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public long MtFromParty { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string UnitName { get; set; }
    }

    public class MtFromPartyBSundryViewModel
    {
        public long MtFromParty { get; set; }
        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string BillSundry { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<MtFromPartyBSundry> sebsundrys { get; set; }
    }

    #endregion
}