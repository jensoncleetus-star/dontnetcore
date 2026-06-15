using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class MaterialRequisitionViewModel
    {
        public long MRNo { get; set; }
        public string BillNo { get; set; }

        public DateTime MRDate { get; set; }

        // refer to table emploayee 
        public long? MRCashier { get; set; }


        public string CreatedUser { get; set; }

        public decimal MRDiscount { get; set; }  

        public DateTime MRValidity { get; set; }
        public DateTime? ReminderDate { get; set; }
        public long? SupplierId { get; set; }
        

        public long? Customer { get; set; }

        public long? AccountId { get; set; }
        public string CustomerName { get; set; }
        public string SupplierEmail { get; set; }

        public string CreatedUserEmail { get; set; }
        public string EmployeeName { get; set; }

        public ICollection<MaterialRequisitionItem> mritem { get; set; }
        public List<MRItemViewModel> MRItem { get; set; }
       

        public string PayType { get; set; }
        public int? CreditPeriod { get; set; }
        public string Remarks { get; set; }
        public long Branch { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }
        public string ProjectName { get; set; }
        public string ApprovedBy { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        public List<FieldMapping> FieldMapAll { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        
        public string TermsCondition { get; set; }
        public long? PrintLayout { get; set; }
        public decimal? GrandTotal { get; set; }
        public string Requeststat { get; set; }
        

    }
    public class MRItemViewModel
    {
        public long MRItemId { get; set; }

        public long MaterialRequisition { get; set; }

        public long Item { get; set; }

        public decimal? TargetPrice { get; set; }
        public decimal? TotalPrice { get; set; }
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
        public long? Make { get; set; }
        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
    
}