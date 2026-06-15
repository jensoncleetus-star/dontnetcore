using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class ProForma
    {
        public long ProFormaId { get; set; }
        // seno defines bill number BillNo defines company.invoiceprefix + SENo

        public long PFNo { get; set; }
        [Required]
        public string BillNo { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime PFDate { get; set; }

       
        // refer to table emploayee
        public long? PFCashier { get; set; }

        // Sale type refers to POS Or Invoice
        public SaleType SaleType { get; set; }

        public long Customer { get; set; }

        ////walking customer or default
        //public CustomerType CustomerType { get; set; }

        // total items and total quantity
        public int PFItems { get; set; }
        public decimal PFItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PFSubTotal { get; set; }

        public decimal PFTax { get; set; }


        public decimal PFTaxAmount { get; set; }

        public decimal PFDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal PFGrandTotal { get; set; }

        // extra note option
        public string PFNote { get; set; }

        // print times may use
        public int Print { get; set; }

        // [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime PFCreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public long Branch { get; set; }

        public virtual Branch CreatedBranch { get; set; }
        public int Status { get; set; }
        [StringLength(150)]
        public string Location { get; set; }
        public CustomerType CustomerType { get; set; }
        public string Remarks { get; set; }

        public long? MaterialCenter { get; set; }

        public long SalesType { get; set; }
        public long? Project { get; set; }
        public long? ProTask { get; set; }
        public ProForma()
        {
            Print = 0;
            PFDiscount = 0;
            // PONo = 0;
        }

        public virtual ICollection<PFItems> PFitem { get; set; }
        [StringLength(50)]
        public string HSCode { get; set; }
        [StringLength(50)]
        public string PaymentTerms { get; set; }

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

    }
    // product items in sale entry
    public class PFItems
    {
        public long PFItemsId { get; set; }

        public long ProForma { get; set; }
        public virtual ProForma ProFormaId { get; set; }

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
    }
    public class DummyPFItem
    {
        public long DummyPFItemId { get; set; }

        public long ProForma { get; set; }

        public long Item { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public decimal ItemDiscount { get; set; }

        public string itemNote { get; set; }
    }

    public class PFBillSundry
    {
        public long PFBillSundryId { get; set; }
        public long ProForma { get; set; }
        public long BillSundry { get; set; }

        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
    }

}