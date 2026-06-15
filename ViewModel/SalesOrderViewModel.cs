using QuickSoft.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class SalesOrderViewModel
    {
        public long SONo { get; set; }
        public string BillNo { get; set; }

        public DateTime SODate { get; set; }

        // refer to table emploayee 
        public long? SOCashier { get; set; }

        public long Customer { get; set; }
        public string CustomerName { get; set; }
        public string CreatedUser { get; set; }

        public decimal SODiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal SOGrandTotal { get; set; }

        public int? SOValidity { get; set; }

        
        public string TermsCondition { get; set; }

        public string CustomerEmail { get; set; }

        public string CreatedUserEmail { get; set; }
        public string EmployeeName { get; set; }
        public string Remarks { get; set; }

        public long Branch { get; set; }
        public SaleType SaleType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long? HireType { get; set; }
        public List<SOItemViewModel> SOItem { get; set; }
        public IEnumerable<SalesType> SalesTypes { get; set; }
        public long SalesType { get; set; }
        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }

        public long ConTypeId { get; set; }
        public string ConType { get; set; }

        public string convertFrom { get; set; }
        public string convertBill { get; set; }


        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public string SaleTypeName { get; set; }
        public string SalesTypeName { get; set; }
        public string EmailId { get; set; }

        public string HType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }
        public long PrintLayout { get; set; }
    }
    public class SOItemViewModel
    {
        public long SalesOrderItemId { get; set; }

        public long SalesOrder { get; set; }

        public long Item { get; set; }


        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }
        public decimal ItemSubTotal { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ItemTaxAmount { get; set; }
        public decimal ItemTotalAmount { get; set; }

        public string ItemNote { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public string PartNumber { get; set; }
        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
    public class salesorderdocumentviewmodel
    {

        public long soid { get; set; }
        public long salesorderID { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }


    }
}
   