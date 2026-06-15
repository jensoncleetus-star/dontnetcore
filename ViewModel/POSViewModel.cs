using QuickSoft.Models;
using System.Collections.Generic;
namespace QuickSoft.ViewModel
{
    public class POSViewModel
    {
        public SalesEntry saleData { get; set; }
        public ICollection<SEItemspos> seItems { get; set; }
        public SEPayment salePayment { get; set; }
        public PosData posData { get; set; }
        public string fnval { get; set; }
        public WalkinCustomer wCustomer { get; set; }
        public string SEDate { get; set; }
        public decimal? roundoff { get; set; }
        public bool? istax { get; set; }
        public string OrderNo { get; set; }
        public decimal dcharge { get; set; }
    }
    public class SEItemspos
    {
        public long SEItemsId { get; set; }

        public long SalesEntry { get; set; }
        public virtual SalesEntry SaleEntryId { get; set; }

        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public decimal ItemDiscount { get; set; }
        public string itemNote { get; set; }
        public string Note { get; set; }
        public choice editable { get; set; }
        public SEItemspos()
        {
            editable = choice.Yes;
        }
    }

}