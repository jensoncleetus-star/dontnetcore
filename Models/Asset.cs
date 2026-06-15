using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{

    public class AssetTransferDetail
    {
        [Key]
        public long AssetitementryId { get; set; }

        public long AssetEntryId { get; set; }

        public string AssetName { get; set; }

        public string Barcode { get; set; }

        public long UnitId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        public long DepreciationPercentage { get; set; }

        public long AssetAccountId { get; set; }

        public long DepreciationAccountId { get; set; }
        public long? RefItemId { get; set; }
        public string DeleteYN { get; set; }

    }
    public class AssetTransferMasters
    {
        [Key]
        public long AssetEntryId { get; set; }

        public long InvoiceNo { get; set; }

        public string PurchaseEntry { get; set; }

        public DateTime AssetEntryDate { get; set; }


        public long? VendorName { get; set; }

        public long? Vat { get; set; }

        public decimal TotalAssetValue { get; set; }

        public long? McFromId { get; set; }

        public long? StockTransferId { get; set; }

    }
    public class AssetToInventoryMasters
    {
        [Key]
        public long EntryId { get; set; }

        public long EntryNo { get; set; }

        public DateTime EntryDate { get; set; }

        public long AssetAccountId { get; set; }

        public long? McFromId { get; set; }
        public decimal TotalAmount { get; set; }

        public long? StockTransferId { get; set; }

    }
    public class AssetToInventoryDetail
    {
        [Key]
        public long ItemEntryId { get; set; }

        public long EntryId { get; set; }

        public long AssetId { get; set; }

        public long RefItemId { get; set; }

        public string AssetName { get; set; }

        public string Barcode { get; set; }

        public long UnitId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        public long DepreciationAccountId { get; set; }

        public long DepreciationPercentage { get; set; }
    }
}