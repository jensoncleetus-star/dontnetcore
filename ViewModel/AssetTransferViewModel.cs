using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class AssetTransferViewModel
    {

        public long AssetEntryId { get; set; }

        public long InvoiceNo { get; set; }

      
        public long? PurchaseEntry { get; set; }

        public DateTime AssetEntryDate { get; set; }

        
        public string VendorName { get; set; }

        public long? Vat { get; set; }

        public decimal TotalAssetValue { get; set; }

        public long? McFromId { get; set; }

        public long? StockTransferId { get; set; }

        public string McName { get; set; }

        public List<AssetTransferItemViewModel> STItem { get; set; }
    }
    public class AssetTransferItemViewModel
    {
        public long AssetitementryId { get; set; }

        public long AssetEntryId { get; set; }

        public string AssetName { get; set; }

        public string Barcode { get; set; }

        public long UnitId { get; set; }

        public decimal Quantity { get; set; }

        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Required")]
        public long DepreciationPercentage { get; set; }

        [Required]
        public long AssetAccountId { get; set; }

        public long DepreciationAccountId { get; set; }
        public long? RefItemId { get; set; }
        

        public string Itemname { get; set; }

        public string Unitname { get; set; }
    }
}