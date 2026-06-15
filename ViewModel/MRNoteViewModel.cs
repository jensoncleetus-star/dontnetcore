using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class MRNoteViewModel
    {
        [Key]
        public long MRId { get; set; }

        public long MRNo { get; set; }
        public string BillNo { get; set; }

        public DateTime MRDate { get; set; }

        public long Supplier { get; set; }
        public long? Cashier { get; set; }

        public string Type { get; set; }
        public long Branch { get; set; }
        public string Note { get; set; }

        public DateTime? RequestedDate { get; set; }


        public int MRNItems { get; set; }
        public decimal MRNQuantity { get; set; }

        public string Remarks { get; set; }

        public long? materialcenter { get; set; }
        public string SupplierEmail { get; set; }

        public string CreatedUserEmail { get; set; }
        public string EmployeeName { get; set; }
        public string SupplierName { get; set; }
        public List<MRNoteItemViewModel> MRNItem { get; set; }
        public List<MRNotePOrderViewModel> MRNPo { get; set; }
        public long ConTypeId { get; set; }
        public string ConType { get; set; }

        public long? CPorderNo { get; set; }
        public long? CPQuotNo { get; set; }
        public long? CMReqNo { get; set; }

        public string convertFrom { get; set; }
        public string convertBill { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        
        public string TermsCondition { get; set; }
        public long PrintLayout { get; set; }
    }
    public class MRNoteItemViewModel
    {
        public long POItemId { get; set; }

        public long MRNote { get; set; }

        public long Item { get; set; }

        public decimal ItemQuantity { get; set; }


        public string ItemNote { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public string PartNumber { get; set; }
        public string Make { get; set; }
        public string Remarks { get; set; }
   
        public List<ItemDetailViewModel> bundleitem { get; set; }
        
    }
    public class MRNotePOrderViewModel
    {
        public string POrder { get; set; }
    }
}