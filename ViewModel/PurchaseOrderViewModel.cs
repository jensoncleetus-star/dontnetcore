using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class PurchaseOrderViewModel
    {
        public long PONo { get; set; }
        public string BillNo { get; set; }

        public DateTime PODate { get; set; }

        // refer to table emploayee 
        public long? POCashier { get; set; }
        public long? Currency { get; set; }
        public string ConvertionRate { get; set; }
        public string DConvertionRate { get; set; }
        public long Supplier { get; set; }
        public string SupplierName { get; set; }
        public string CreatedUser { get; set; }

        public decimal PODiscount { get; set; }

        //[DataType(DataType.Currency)]
        public decimal POGrandTotal { get; set; }

        public int? POValidity { get; set; }

        
        public string TermsCondition { get; set; }

        public string SupplierEmail { get; set; }

        public string CreatedUserEmail { get; set; }
        public string EmployeeName { get; set; }

        public long PurchaseType { get; set; }
        public IEnumerable<PurchaseType> PurchaseTypes { get; set; }

        public ICollection<PurchaseOrderItem> porderitem { get; set; }
        public List<PurchaseOrderItemViewModel> POItem { get; set; }
        public List<POBillSundryViewModel> PObs { get; set; }
        public SupplierType SupplierType { get; set; }
        public string PayType { get; set; }
        public int? CreditPeriod { get; set; }
        public string Remarks { get; set; }
        public long Branch { get; set; }

        public long? ConTypeId { get; set; }
        public string ConType { get; set; }

        public long? CPQuotNo { get; set; }
        public string CMReqNo { get; set; }

        public string convertFrom { get; set; }
        public string convertBill { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public string PursTypeName { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }
        public long PrintLayout { get; set; }
    }
    public class PurchaseOrderItemViewModel
    {
        public long POItemId { get; set; }

        public long PurchaseOrder { get; set; }

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
        public string MakeName { get; set; }
        
        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
    public class POBillSundryViewModel
    {
        public string BillSundry { get; set; }
        public decimal? BsValue { get; set; }
        public int AmountType { get; set; }
        public int BsType { get; set; }
        public decimal? BsAmount { get; set; }
        public string Type { get; set; }
        public string AmtType { get; set; }
        public ICollection<POBillSundry> pobsundrys { get; set; }
    }

    public class PurchOrdrDocumentViewModel
    {
        public long DocumentID { get; set; }
        public long PurchOrdId { get; set; }
        public string FileName { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}