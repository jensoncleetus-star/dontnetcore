using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
namespace QuickSoft.ViewModel
{
    public class MCViewModel
    {
        //
        public long? id { get; set; }
        public long? MCFrom { get; set; }
        public long? MCTo { get; set; }
        public ICollection<PEItems> purchaseitems { get; set; }
        public ICollection<SEItems> saleitems { get; set; }

        public ICollection<PRItems> purchasereturn { get; set; }
        public ICollection<SRItems> salereturn { get; set; }

        public ICollection<StockAdjustment> stockadjustments { get; set; }
        
        public ICollection<GeneratedItems>Gitems { get; set; }

        public ICollection<ProItem> Pitems { get; set; }

        public ICollection<ConsumedItems> Citems { get; set; }

        public ICollection<UnassembleItem> Uitems { get; set; }

        public ICollection<StockTransferItem> Stitems { get; set; }
        
    }




    }
