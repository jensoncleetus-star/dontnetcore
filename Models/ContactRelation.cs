using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class ContactRelation
    {

        [Key]
        public long ContactRelationID { set; get; }
        public long ContactID { set; get; }
        public long RelationType { set; get; }
        public long RelationID { set; get; }

    }
}