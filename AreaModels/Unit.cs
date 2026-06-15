using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class PropertyUnit
    {
        public long Id { get; set; }
        public long EntryNo { get; set; }
        public string PremisesNo { get; set; }
        public string NoofRooms { get; set; }
        public string UnitUsage { get; set; }
        public string Area { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public long? Property { get; set; }
        public long? UnitType { get; set; }
        public decimal? Rent { get; set; }
        public decimal? Deposit { get; set; }
        public string Description { get; set; }
        public string TnC { get; set; }

        public string File { get; set; }
        public long? Document { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public choice editable { get; set; }
        public Status Status { get; set; }
    }

    public class PropertyUnitType
    {
        public long ID { get; set; }

        public string Name { get; set; }
    }

    public class PropertyUnitFeature
    {
        public long ID { get; set; }

        public string Feature { get; set; }
    }
    public class SelectedUnitFeature
    {
        public long ID { get; set; }

        public long Unit { get; set; }

        public string Feature { get; set; }
    }
    public class PropertyUnitImage
    {
        public long ID { get; set; }

        [Required]
        public long UnitID { get; set; }

        [Required]
        public string FileName { get; set; }

        public int Status { get; set; }

        public virtual PropertyUnit Items { get; set; }

    }

    public class PropertyUnitDocument
    {
        public long ID { get; set; }

        [Required]
        public long UnitID { get; set; }

        [Required]
        public string FileName { get; set; }

        public int Status { get; set; }

        public virtual PropertyUnit Items { get; set; }
    }
}