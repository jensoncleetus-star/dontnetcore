using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.ViewModel
{
    public class PackinglistViewModel
    {
        public long PackinglistId { get; set; }
        public string BillNo { get; set; }
        [Display(Name = "Date")]
        public DateTime PLDate { get; set; }
        public long Invoice { get; set; }
        public long Customer { get; set; }
        public string CustomerName { get; set; }
        public string EmployeeName { get; set; }
        public long? Employee { get; set; }
        public string LPO { get; set; }
        public string Remarks { get; set; }
        public string TermsCondition { get; set; }
        public string HSCode { get; set; }

        public List<FieldMapping> FieldMap { get; set; }
        //Refernce Field Added
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }

        public List<PLItemViewModel> PLItems { get; set; }
        public List<ApprovalViewModel> Emp { get; set; }
    }

    public class PLItemViewModel
    {
        public long PLId { get; set; }

        public long PL { get; set; }
        public virtual Deliverynote DvEntryId { get; set; }
        public long Item { get; set; }
        public decimal ItemQuantity { get; set; }
        public string ItemUnit { get; set; }
        public decimal Packet { get; set; }
        public decimal MinQty { get; set; }
        public string ItemName { get; set; }
        public string ItemCode{ get; set; }
        public string ItemNote{ get; set; }
        public string PartNumber { get; set; }

        public List<ItemPkListViewModel> subitem { get; set; }
    }
    public class ItemPkListViewModel
    {
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public decimal ItemQuantity { get; set; }
        public string ItemUnit { get; set; }
        public decimal Packet { get; set; }
        public decimal MinQty { get; set; }
    }
    public class StickerViewModel
    {
        public long? SN { get; set; }
        public string itemSticker { get; set; }
        public string qty { get; set; }
        public char alph { get; set; }
        public Int32 quant { get; set; }
        public decimal? Minqty { get; set; }
    }
}