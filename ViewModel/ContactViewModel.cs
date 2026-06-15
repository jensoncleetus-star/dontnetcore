using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class ContactViewModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string EmailId { get; set; }
        public string Reference { get; set; }
        public string ContactPerson { get; set; }
        public string SalesPMob { get; set; }
        public string GroupName { get; set; }
        public Status Status { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

    }

    public class ContactformViewModel
    {
        public long ContactID { get; set; }       

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required]
        public string Name { get; set; }

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

        [StringLength(15)]
        public string Fax { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string EmailId { get; set; }

        public string Reference { get; set; }

        public string Website { get; set; }


        [StringLength(50)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }


        [StringLength(50)]
        public string SalesPMob { get; set; }

        [DefaultValue(true)]
        public Status Status { get; set; }

        public long Group { get; set; }
        public virtual ContactGroup ContactGroup { get; set; }

        //public ICollection<MobileViewModel> mobmodel { get; set; }

        public ICollection<CustomerDocumentViewModel> CustomerDocumentViewModel { get; set; }

        public ICollection<MobileViewModel> MobileViewModel { get; set; }

        public long? ContactGroupID { get; set; }
        public string ContactCode { get; set; }
        public int CountryID { get; set; }
        public long? ContactTypeID { get; set; }

    }
    public class MobileViewModel
    {
        public long ID { get; set; }
        public string Num { get; set; }

        public string Name { get; set; }
        public string emails { get; set; }
        public string Fax { get; set; }
    }

    public class CustomerDocumentViewModel
    {
        public long DocumnetId { get; set; }
        public long CutomerID { get; set; }


        public string DoucumentType { get; set; }

        public DateTime Expiry { get; set; }

        public string Notes { get; set; }

        public IFormFile File { get; set; }

        public long DocumentTypeID { get; set; }

    }

}