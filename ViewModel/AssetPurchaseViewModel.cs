using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class AssetPurchaseViewModel
    {
        

        [Display(Name = "ID")]
        public long AssetEntryId { get; set; }

        [Display(Name = "Invoice No")]
        public long? InvoiceNo { get; set; }

        public string PurchaseEntry { get; set; }

        [Display(Name = "Date")]
        public DateTime ? AssetEntryDate { get; set; }

        public long ? VendorName { get; set; }

        public long ?  Vat { get; set; }

        public decimal TotalAssetValue { get; set; }
        public List<FieldMapping> FieldMap { get; set; }

    }
    public class AssetPurchaseSubmitViewModel
    {

        public long AssetEntryId { get; set; }

        public string InvoiceNo { get; set; }

        public DateTime AssetEntryDate { get; set; }

        public string MaterialCentre { get; set; }

        public long VendorName { get; set; }

        public string Attachment { get; set; }

        public long Vat { get; set; }

        public long TotalAssetValue { get; set; }

        public List<AssetTransferDetail> lstassets { get; set; }

        

    }
    public class Assetdocvmodel
    {
        [Key]
        public long DocumentID { get; set; }

        public long TransactionID { get; set; }

        public string TransactionType { get; set; }

        public string FileName { get; set; }

        public Status Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public string DocumentType { get; set; }

        public DateTime? Expiry { get; set; }

        public string Notes { get; set; }


    }
}