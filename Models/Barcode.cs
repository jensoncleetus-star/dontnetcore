using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Barcode
    {
        public long BarcodeId { get; set; }
        public string BarcodeNumber { get; set; }
        public long ItemID { get; set; }
    }
    public class contactlist
    {
        public string FirstName { get; set; }
        public string EmailId { get; set; }
       public string Mobile { get; set; }
        public long? contactrelationid { get; set; }
        public long? contactid { get; set; }
        public string Fax { get; set; }
    }
}