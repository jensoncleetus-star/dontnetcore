using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class VehicleModel
    {
        [Key]
        public long ModelId { get; set; }
        [Display(Name = "Model")]
        public string Model { get; set; }
        public long? MaId { get; set; }
        public long? VTId { get; set; }
    }
    public class vehiclereminder
    {
        [Key]
        public long vehiclereminderid  { get; set; }
        public long vehicleid { get; set; }
        public decimal km { get; set; }
        public string note { get; set; }
        public DateTime reminderdate { get; set; }
    }
public class vehiclemaster
    {
        [Key]
        public long vehicleid { get; set; }

        public string VechicleName { get; set; }

        public string RegistrationNumber { get; set; }
        public DateTime? openingkelometerdate { get; set; }
        public decimal openingkelometer { get; set; }
        public decimal currentkelometer { get; set; }
        public DateTime? logtime { get; set; }
        public string remarks { get; set; }
        public string createdby { get; set; }
        public DateTime? createddate { get; set; }
    }
    public class vehiclemasterviewmodel
    {

        public long? vehicleid { get; set; }
        public string VechicleName { get; set; }

        public string RegistrationNumber { get; set; }
        public string openingkelometerdate { get; set; }
        public decimal openingkelometer { get; set; }
        public decimal currentkelometer { get; set; }
        
        public string remarks { get; set; }
        public ICollection<CustomerDocumentViewModel> vehilereminder { get; set; }
    }

    public class CustomerDocumentViewModel
    {
    
   
        public decimal km { get; set; }
        public string note { get; set; }
        public string reminderdate { get; set; }
    }

    public class VehicleType
    {
        [Key]
        public long VTypeId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Type")]
        public string Type { get; set; }
    }
    public class VehicleManufacturer
    {
        [Key]
        public long MId { get; set; }
        [Display(Name = "Manufacturer")]
        public string Manufacturer { get; set; }
        public long? VTyId { get; set; }
    }
}