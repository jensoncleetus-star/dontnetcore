using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class HireReturnViewModel
    {
        public long HrNo { get; set; }
        public string BillNo { get; set; }

        public DateTime Date { get; set; }

        // refer to table emploayee
        public long? Cashier { get; set; }

        public long Customer { get; set; }
        public string CustomerName { get; set; }
        // total items and total quantity 
        public int Items { get; set; }
        public decimal ItemQuantity { get; set; }

        // extra note option
        public string Note { get; set; }

        // mail times may use
        public int Mail { get; set; }
        

        public string RtType { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DvCreatedDate { get; set; }
        public long Branch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }

        
        public string TermsCondition { get; set; }
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        public string custEmailId { get; set; }
        public string CreatedUserEmail { get; set; }
        public string EmployeeName { get; set; }

        public List<HrItemViewModel> HrItem { get; set; }
        public string Remarks { get; set; }
        public long? MaterialCenter { get; set; }
        public string MCName { get; set; }

        public long ConTypeId { get; set; }
        public string ConType { get; set; }

        public long? Invoice { get; set; }
        public string InvoiceNo { get; set; }
        public long? HireType { get; set; }
        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }

        //public string convertFrom { get; set; }
        //public string convertBill { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public string InBillNo { get; set; }
        public string HType { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }
        public long? PrintLayout { get; set; }
    }
    public class HrItemViewModel
    {
        public long DvItemId { get; set; }

        public long Hr { get; set; }
        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }


        public string ItemNote { get; set; }
        public string PartNumber { get; set; }

        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }

        public decimal? ReceivedQty { get; set; }
        public decimal? DamageQty { get; set; }
        public decimal? MissingQty { get; set; }

        public decimal? DvQty { get; set; }
        public decimal? RetQty { get; set; }


        //----for details----
        public List<ItemDetailViewModel> bundleitem { get; set; }
    }
}