using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class ItemImageViewModel
    {
        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItmImage { get; set; }
        public long? itemid { get; set; }
        public string itemcode { get; set; }
        public string itemname { get; set; }
        public string imagpath { get; set; }
    }
}