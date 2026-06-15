using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.Models
{
    public class googlereview
    {
        [Key]
        public long googlereviewid { get; set; }
        
        public DateTime QuotDate { get; set; }
        public long QuotCashier { get; set; }
        public string createedby { get; set; }
        public DateTime createddate { get; set; }
    }
    public class googlereviewmodal
    {
        

        public string QuotDate { get; set; }
        public long QuotCashier { get; set; }
       
    }
    public class vehicleupdation
    {
        [Key]
        public long vehicleupdateid { get; set; }
        public long? startvupid { get; set; }
        public long employeeid  { get; set; }


    public long vehicleid  { get; set; }


    public long usetype  { get; set; }
  

    public long? protaskid   { get; set; }
   public long? leadid { get; set; }
	    public decimal readings  { get; set; }

        public DateTime createddate  { get; set; }


         public string createdby   { get; set; }
        public string remarks { get; set; }


      public int direction   { get; set; }
    }
    public class Vehicleupdateviewmodal
    {


      public long? id { get; set; }
        public long employee { get; set; }
        public long vehicleid { get; set; } 
        public int usetype { get; set; }
        public long? taskid { get; set; }
        public long? leadid { get; set; }
       public decimal reading { get; set; }
        public string remarks { get; set; }

    }
    public class FilemultipleDocuments
    {
        public long Id { get; set; }

        public long RelationID { get; set; }
        [StringLength(150)]
        public string DocumentName { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string Document { get; set; }

        public string Note { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public long Branch { get; set; }

        public Status Status { get; set; }

    }
}