using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class ItemDocumentViewModel
    {
        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemDocument { get; set; }
    }
}