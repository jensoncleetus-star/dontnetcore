using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PropertyType
    {
        public long ID { get; set; }
        [Required]
        public string Name { get; set; }
    }

    public class PropertyFeature
    {
        public long ID { get; set; }
        [Required]
        public string Feature { get; set; }
    }
    public class SelectedFeature
    {
        public long ID { get; set; }
        [Required]
        public long Property { get; set; }

        public string Feature { get; set; }
    }
    public class DocumentType
    {
        public long ID { get; set; }
        [Required]
        public string Name { get; set; }

        public string Section { get; set; }
    }

    public class AdditionalField
    {
        public long ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Section { get; set; }
    }

    public class AdditionalFieldData
    {
        public long ID { get; set; }

        public long Reference { get; set; }

        public string Name { get; set; }

        public string Purpose { get; set; }

        public long Field { get; set; }
    }
}