using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class Contact
    {
        public long ContactID { get; set; }
        public string ContactCode { get; set; }
        
        public string Name { get; set; }
        
        public string FirstName { get; set; }
       
        public string LastName { get; set; }

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


        [StringLength(15)]
        public string Phone { get; set; }

        [StringLength(15)]
        public string Mobile { get; set; }

  
        public string Fax { get; set; }

        [StringLength(100)]
        public string EmailId { get; set; }

        public string Reference { get; set; }




        [StringLength(1000)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }


        [StringLength(50)]
        public string SalesPMob { get; set; }

        [DefaultValue(true)]
        public Status Status { get; set; }

        public long Group { get; set; }
        public virtual ContactGroup ContactGroup { get; set; }
        public long? ContactGroupID { get; set; }
        public string TypeOfContact { get; set; }

        public string Website { get; set; }

        public int CountryID { get; set; }

        public long? ContactTypeID { get; set; }

    }
    public class ContactGroup
    {
        public long ContactGroupID { get; set; }
        public long Parent { get; set; }
        public string Name { get; set; }
        public choice Editable { get; set; }
        public ContactGroup()
        {
            Editable = 0;
        }
    }

    public class Mobile
    {
        public long ID { get; set; }

        public long Contact { get; set; }
        [StringLength(15)]
        public string MobileNum { get; set; }
        [StringLength(500)]
        public string Name { get; set; }
    }

    public class CustomerDocument
    {
        [Key]
        public long DocumnetId { get; set; }


        public long CutomerID { get; set; }


        public string DoucumentType { get; set; }

        public DateTime Expiry { get; set; }

        public string Notes { get; set; }

        public string FilePath { get; set; }
        public long ContactId { get; set; }

        public long DocumentTypeID { get; set; }
     
    }

}