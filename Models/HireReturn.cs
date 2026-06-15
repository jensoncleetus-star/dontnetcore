using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Models
{
    public class HireReturn
    {
        public long HireReturnId { get; set; }
        // seno defines bill number BillNo defines saleprefix + No
        public long HrNo { get; set; }
        public string BillNo { get; set; }

        public DateTime Date { get; set; }

        // refer to table emploayee
        public long? Cashier { get; set; }

        public long Customer { get; set; }

        // total items and total quantity
        public int Items { get; set; }
        public decimal ItemQuantity { get; set; }


        // extra note option
        public string Note { get; set; }

        // mail times may use
        public int Mail { get; set; }
        

        [StringLength(20)]
        public string RtType { get; set; }

        
        public string TermsCondition { get; set; }
        public long EmailTemplateID { get; set; }
        public long CompanyHeaderID { get; set; }

        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedDate { get; set; }
        public long Branch { get; set; }
        public virtual Branch CreatedBranch { get; set; }
        public string CreatedUserId { get; set; }
        public Status Status { get; set; }
        
        
        public string Remarks { get; set; }
        public long? MaterialCenter { get; set; }
        public long? Invoice { get; set; }
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

    }

    public class HrItem
    {
        public long HrItemId { get; set; }

        public long Hr { get; set; }
        public long Item { get; set; }
        public virtual Item ItemId { get; set; }

        public long? ItemUnit { get; set; }

        public decimal ItemUnitPrice { get; set; }
        public decimal ItemQuantity { get; set; }

        public decimal ItemDiscount { get; set; }
        public string ItemNote { get; set; }

        public decimal? ReceivedQty { get; set; }
        public decimal? DamageQty { get; set; }
        public decimal? MissingQty { get; set; }
    }
}