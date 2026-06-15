using System;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class ReturnNote
    {
        public long ReturnNoteId { get; set; }
        // seno defines bill number BillNo defines sale-prefix + RtNo
        public long RtNo { get; set; }
        public string BillNo { get; set; }

        public DateTime RtDate { get; set; }

        // refer to table employee
        public long? RtCashier { get; set; }

        public long Customer { get; set; }
        public long? Project { get; set; }
        public string Location { get; set; }

        // total items and total quantity
        public int RtItems { get; set; }
        public decimal RtItemQuantity { get; set; }

        //[DataType(DataType.Currency)]
        public decimal RtSubTotal { get; set; }

        public decimal RtTax { get; set; }
        public decimal RtTaxAmount { get; set; }

        public decimal RtDiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal RtGrandTotal { get; set; }

        // extra note option
        public string RtNote { get; set; }
        public string RtType { get; set; }


        // mail times may use
        public int Mail { get; set; }

        public int? RtValidity { get; set; }
        public long HireType { get; set; }
        public string Transportation { get; set; }
        public string Driver { get; set; }

        
        public string TermsCondition { get; set; }
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        public long? MaterialCenter { get; set; }
        public long? HireOrder { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime RtCreatedDate { get; set; }
        public long Branch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }

        public string Remarks { get; set; }
    }
    public class RtItem
    {
        public long RtItemId { get; set; }

        public long Rt { get; set; }
        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public long? ItemUnit { get; set; }
        public decimal ItemQuantity { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public decimal ItemDiscount { get; set; }

        //public decimal RetItemQuantity { get; set; }
        //public decimal DvItemQuantity { get; set; }
        //public decimal ItemBalance { get; set; }


        public string ItemNote { get; set; }
        // hire
        public long? HireType { get; set; }


    }
}