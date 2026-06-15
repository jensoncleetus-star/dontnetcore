using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class UnassembleViewModel
    {
        public long UnassembleId { get; set; }
        public long EntryNo { get; set; }
        [Display(Name = "Voucher No")]
        public string VoucherNo { get; set; }
        [Display(Name = "Date")]
        public string PEDate { get; set; }
        [Display(Name = "BOM Name")]
        public long BOM { get; set; }
        [Display(Name = "Item Consumed")]
        public long Item { get; set; }
        [Display(Name = "Quantity")]
        public decimal Qty { get; set; }
        public long? Unit { get; set; }
       
        // default amount from table item
        public decimal Price { get; set; }
        // total ProItem.amount + (expense*qty)
        public decimal Amount { get; set; }

        // extra note option
        public string Note { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public long Branch { get; set; }
        public Status Status { get; set; }

        public long? MaterialCenter { get; set; }

        public Unassemble unasdata { get; set; }
        public ICollection<UnassembleItemViewModel> unasitem { get; set; }
        public ICollection<UnasItemViewModel> unasConsumed { get; set; }
        public ICollection<MBomViewModel> bom { get; set; }
        public string ItemName { get; set; }
        public string ItemUnitName { get; set; }
        public string UserName { get; set; }
        public string BOMName { get; set; }
        public long BOMId { get; set; }
        public string fnvalue { get; set; }
        public string BranchName { get; set; }
        public string MC { get; set; }
        public ICollection<UnasConsViewModel> UnasCon { get; set; }
        public List<UnassembleItemViewModel> UnasItemvmodel { get; set; }

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
    public class MBomViewModel
    {
        public long BOM_Id { get; set; }
        //public string BOMName { get; set; }
    }
        public class UnassembleItemViewModel
    {
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }

        public long UnItemId { get; set; }

        public long Unassemble { get; set; }
        public Unassemble UnassembleId { get; set; }
        public long ItemId { get; set; }
        public long? Unit { get; set; }
        public long? BOMId { get; set; }
        // unit price
        public decimal PPrice { get; set; }
        // total amount - puprice*pqty
        public decimal PAmount { get; set; }
    }
    public class UnasItemViewModel
    {
        public long UnasItemId { get; set; }
        public long Unassamble { get; set; }
        public Unassemble UnassembleId { get; set; }
        public long ItemId { get; set; }
        public long? Unit { get; set; }
        public decimal Quantity { get; set; }
        // unit price
        public decimal PPrice { get; set; }
        // total amount - puprice*pqty
        public decimal PAmount { get; set; }
        public long BOM { get; set; }
    }
    public class UnasConsViewModel
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemUnit { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public decimal Expense { get; set; }
        public decimal BOMId { get; set; }
        public string Unit { get; set; }
    }
    public class UnasViewModel
    {
        public Unassemble unasdata { get; set; }
        public ICollection<UnasItemViewModel> unasitem { get; set; }
        public ICollection<UnasConsViewModel> unasconsumed { get; set; }
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
}