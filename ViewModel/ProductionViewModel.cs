using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class ProductionViewModel
    {
        public long ProductionId { get; set; }
        public long PrNo { get; set; }
        [Display(Name = "Voucher No")]
        public string VoucherNo { get; set; }
        [Display(Name = "Date")]
        public string PEDate { get; set; }
        [Display(Name = "BOM Name")]
        public long BOM { get; set; }
        [Display(Name = "Item Generated")]
        public long Item { get; set; }
        [Display(Name = "Quantity")]
        public decimal Qty { get; set; }
        public long? Unit { get; set; }
        public decimal Expense { get; set; }
        [Display(Name = "Production Cost")]
        public decimal? Productioncost { get; set; }
        [Display(Name = "Labour Cost")]
        public decimal? Labourcost { get; set; }
        [Display(Name = "Material Cost")]
        public decimal? meterialcost { get; set; }
        // default amount from table item
        public decimal Price { get; set; }
        // total ProItem.amount + (expense*qty)
        public decimal Amount { get; set; }

        public long? MaterialCenter { get; set; }

        // extra note option
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Status Status { get; set; }

        public Production prodata { get; set; }
        public ICollection<ProItem> proitem { get; set; }
        public ICollection<BillOfMaterial> progenerated { get; set; }
        public ICollection<MBomViewModel> bom { get; set; }
        public string ItemName { get; set; }
        public string ItemUnitName { get; set; }
        public string UserName { get; set; }
        public string BOMName { get; set; }
        public string fnvalue { get; set; }
        public string MC { get; set; }
        public string BranchName { get; set; }
        public List<ProItemViewModel> ProItemvmodel { get; set; }
        public List<ProItemViewModel> progen { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public long? Project { get; set; }
        [Display(Name = "Task")]
        public long? ProTask { get; set; }

        public string ProjectName { get; set; }
        public string TaskName { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }
    }
    public class ProItemViewModel
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Expense { get; set; }
        public long BOMId { get; set; }
        public string Unit { get; set; }
    }
    public class ProdViewModel
    {
        public Production prodata { get; set; }
        public ICollection<ProItViewModel> proitem { get; set; }
        public ICollection<ProItemViewModel> progenerated { get; set; }
        public ICollection<MBomViewModel> bom { get; set; }
        public string PEDate { get; set; }
        public string fnval { get; set; }
        public string ApprovedBy { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public long? Project { get; set; }
        public long? ProTask { get; set; }
    }
    public class ProItViewModel
    {
        public long ProItemId { get; set; }
        public long Production { get; set; }
        public Production ProductionId { get; set; }
        public long ItemId { get; set; }
        public long? Unit { get; set; }
        public decimal Quantity { get; set; }
        // unit price
        public decimal PPrice { get; set; }
        // total amount - puprice*pqty
        public decimal PAmount { get; set; }
        public long BOM { get; set; }
        public long bomitemid { get; set; }

    }

    public class ProPrintViewModel
    {
        public string ComHeadCheck { get; set; }
        public long Production { get; set; }
        public Production ProductionId { get; set; }
        public long ItemId { get; set; }
        public long? Unit { get; set; }
        public decimal Quantity { get; set; }
        // unit price
        public decimal PPrice { get; set; }
        // total amount - puprice*pqty
        public decimal PAmount { get; set; }
        public long BOM { get; set; }
        public long bomitemid { get; set; }

    }
}