using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PropertyMain
    {
        public long Id { get; set; }
        [StringLength(20)]
        public string Code { get; set; }
        public string Municipality { get; set; }
        public string Zone { get; set; }
        public string Sector { get; set; }
        public string RoadName { get; set; }
        public string PlotNo { get; set; }
        public string PlotAddress { get; set; }

        public string PropertyRegistrationNo { get; set; }
        public string Name { get; set; }

        public string Remark { get; set; }

        public string Description { get; set; }

        public long? PropertyType { get; set; }

        //Doc Section
        public long? DocumentType { get; set; }
        public string File { get; set; }
        public string Document { get; set; }
        //Address fields
        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [StringLength(50)]
        [Display(Name = "Emirate")]
        public string State { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        public string Zip { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public choice editable { get; set; }
        public Status Status { get; set; }
        
        public long EntryNo { get; set; }
        public long? LandlordID { get; set; }


    }

    public class PropertyImage
    {
        public long ID { get; set; }

        [Required]
        public long PropertyID { get; set; }

        [Required]
        public string FileName { get; set; }

        public int Status { get; set; }

        public virtual PropertyMain Items { get; set; }

    }

    public class PropertyDocument
    {
        public long ID { get; set; }

        [Required]
        public long PropertyID { get; set; }

        [Required]
        public string FileName { get; set; }

        public int Status { get; set; }

        public virtual PropertyMain Items { get; set; }
    }
}