using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Supplier
    {
        public long SupplierID { get; set; }

        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }

        //public string TaxRegNo { get; set; }

        public long Contact { get; set; }
        public virtual Contact ContactID { get; set; }

        public decimal CreditLimit { get; set; }

        public int CreditPeriod { get; set; }
        
        public string Remark { get; set; }

        public long Accounts { get; set; }
        public virtual Accounts AccountID { get; set; }


        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string IbanNo { get; set; }
        public string BranchName { get; set; }
        public string Swift { get; set; }

        public TaxType? TaxType { get; set; }
        public int? Status { get; set; }
        public string Addres { get; set; }
        public DateTime? logtime { get; set; }
        public Supplier()
        {
            CreditLimit = 0;
            CreditPeriod = 0;
            TaxType = null;
        }       
    }

    public class SupplierItems
    {
        [Key]
        public long Id { set; get; }
        public long SupplierId { set; get; }
        public long ItemId { set; get; }
    }

    public class SupplierCategories
    {
        [Key]
        public long Id { set; get; }
        public long SupplierId { set; get; }
        public long CategoryId { set; get; }
    }

    public class SupplierBrands
    {
        [Key]
        public long Id { set; get; }
        public long SupplierId { set; get; }
        public long BrandId { set; get; }
    }
    public class VenterRateDetails
    {
        [Key]
        public long VenterRateId { set; get; }
        public long  VenderRateMasterId { set; get; }
        public long supplierid { get; set; }
        public string ItemType { set; get; }
        public string ExternalModal { set; get; }
        public string InternalModal { set; get; }
        public decimal Rate { set; get; }
        public decimal? promorate { get; set; }
    	public string promotiondescription { get; set; }
    }

    public class VenderRateMaster
    {
        [Key]
        public long VenderRateMasterId { set; get; }
        public long SupplierId { set; get; }
        public DateTime createdatae { set; get; }
    }
}