using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class ItemSerialNumber
    {
        [Key]
        public long itemserialnoid { get; set; }
        public long itemid { get; set; }
        public string serialno { get; set; }
        public DateTime? expirydate { get; set; }
    }
  public class lstitems
    {
       public string serialno { get; set; }
       public long itemid { get; set; }
      public DateTime? expirydate { get; set; }
    }
    public class ItemSerialNumberView
    {
        [Key]
        public long itemserialnoid { get; set; }
        [Display(Name = "Select Item ")]
        public long itemid { get; set; }
        public List<serialnoobj> serialnoobjs { get; set; }
       
    }
    public class serialnoobj
    {
        public string serialno { get; set; }
        public string expirydate { get; set; }
    }
}