using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class PropertyUnitViewModel
    {

        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Code { get; set; }
        public string PremisesNo { get; set; }
public string NoofRooms { get; set; }
public string UnitUsage { get; set; }
public string Area { get; set; }
        public string Prefix { get; set; }
        public long? Property { get; set; }
        public long? UnitType { get; set; }
       
        public decimal? Rent { get; set; }
     
        public decimal? Deposit { get; set; }
        public string Description { get; set; }
        public string TnC { get; set; }

        public string File { get; set; }
        public long? Document { get; set; }
        public string[] Feature { get; set; }



        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemImage { get; set; }

        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemDocument { get; set; }

        public string ImageName { get; set; }
        public long? ImageId { get; set; }
        public long? ItmImageId { get; set; }

        public string DocName { get; set; }
        public long? DocId { get; set; }
        public long? ItmDocId { get; set; }

        public string Unitname { get; set; }
        public string Propertyname { get; set; }

        public ICollection<DocumentTypeViewModel> docmodel { get; set; }

        public string Section { get; set; }
    }
}