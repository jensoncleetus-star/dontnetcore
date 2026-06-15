using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickSoft.Models
{
    public class ParticularParty
    {
        [Key]
        public long Id { get; set; }
        public long? PartyID { get; set; }
        public long? PartyType { get; set; }
        public string PartyName { get; set; }
    }
}