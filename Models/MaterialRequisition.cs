using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class MaterialRequisition
    {
        public long MaterialRequisitionId { get; set; }

        // seno defines bill number BillNo defines saleprefix + QuotNo
        public long MRNo { get; set; }
        public string BillNo { get; set; }

        public DateTime MRDate { get; set; }

        // refer to table emploayee
        public long? MRCashier { get; set; }

        // total items and total quantity
        public int MRItems { get; set; }
        public decimal MRItemQuantity { get; set; }


        // extra note option
        public string MRNote { get; set; }
        public string MRRemark { get; set; }

        // mail times may use
        public int Mail { get; set; }

        public DateTime MRValidity { get; set; }
        public DateTime? ReminderDate { get; set; }
        public long? SupplierId { get; set; }

        public long? Customer { get; set; }


        

        //future use
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime MRCreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }    
        public string Remarks { get; set; }
        public long? Project { get; set; }
        public long? ProTask { get; set; }

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

        
        public string TermsCondition { get; set; }
        public string RequestStatus { get; set; }
    }
    public class MaterialRequisitionItem
    {
        [Key]
        public long MRItemId { get; set; }
        public long MaterialRequisition{ get; set; }     
        public long Item { get; set; }      
        public long? ItemUnit { get; set; }       
        public decimal ItemQuantity { get; set; }     
        public string ItemNote { get; set; }
        public long? Make { get; set; }
        public string ItemRemark { get; set; }
        public decimal? TargetPrice { get; set; }
    }
    public class DummyMaterialRequisitionItem
    {
        [Key]
        public long DummmyMRItemId { get; set; }
        public long MaterialRequisition { get; set; }
        public long Item { get; set; }
        public long? ItemUnit { get; set; }
        public decimal ItemQuantity { get; set; }
        public string ItemNote { get; set; }
        public long? Make { get; set; }
        public string ItemRemark { get; set; }
        public decimal? TargetPrice { get; set; }
    }

    public class AddedRemarksvm
    {
        [Key]
        public long RemarkId { get; set; }
        public long TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Remarks { get; set; }
        public string AddedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Next Date")]
        public string nextfolloupdate { get; set; }
        [Display(Name = "Next Time")]
        public DateTime? nextfolloupdatetime { get; set; }
    }
    public class AddedRemarks
    {
        [Key]
        public long RemarkId { get; set; }
        public long TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Remarks { get; set; }
        public string AddedUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? nexttime { get; set; }
        public DateTime? nextdate { get; set; }
    }

   

}

