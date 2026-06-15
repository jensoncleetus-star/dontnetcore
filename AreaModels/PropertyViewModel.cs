using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class PropertyViewModel
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

        public long EntryNo { get; set; }
        [Required]
        public string Name { get; set; }

        public string Remark { get; set; }

        public string Description { get; set; }
        public string[] FeatureList { get; set; }
        public long? PropertyType { get; set; }
        public string PropertyTypeName { get; set; }
        public long[] Features { get; set; }
        public string[] Feature { get; set; }
        public string Document { get; set; }
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
        [Phone]
        public string Phone { get; set; }

        [StringLength(15)]
        public string Mobile { get; set; }

        public ICollection<MobileViewModel> mobmodel { get; set; }

        public MobileViewModel mobmodel1 { get; set; }

        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemImage { get; set; }

        [DataType(DataType.Upload)]
        public IEnumerable<IFormFile> ItemDocument { get; set; }
        

        //Doc Section
        public long? DocumentType { get; set; }
        public string File { get; set; }

        public List<AdditionalField> AdditionalField { get; set; }
        public List<AdditionalFieldVieModel> AdditionalFieldVieModels { get; set; }

        public string ImageName { get; set; }
        public long? ImageId { get; set; }
        public long? ItmImageId { get; set; }

        public string DocName { get; set; }
        public long? DocId { get; set; }
        public long? ItmDocId { get; set; }
        //details
        public List<HireType> HType { get; set; }
        public List<HireType> Featur { get; set; }

        public ICollection<DocumentTypeViewModel> docmodel { get; set; }

        public ICollection<PropertyFeature> PropFeature { get; set; }

        public string Section { get; set; }
        public long? ddlLandlord { get; set; }
    }

    public class AdditionalFieldVieModel
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public string Entrydata { get; set; }
        public long Field { get; set; }
    }
    public class propertyreportviewmodel
    {
        public string properyname { get; set; }
        public string owner { get; set; }
        public string address { get; set; }
        public string propertytype { get; set; }
        public string tenantname { get; set; }
        public string startdate { get; set; }
        public string enddate { get; set; }
        public string rent { get; set; }
        public string noinstallment { get; set; }
        public string contractorname { get; set; }
        public string amcstartdate { get; set; }
        public string amcenddate { get; set; }
        public string amcfees { get; set; }
        public string amcinstallment { get; set; }
        public ICollection<DocumentTypeViewModel> docmodel { get; set; }
        public ICollection<DocumentTypeViewModel> tenancydoc { get; set; }

    }

    public class propertysummery
    {
        
        public long propertyid { get; set; }
        public string properyname { get; set; }
        public decimal? RentalIncome { get; set; }
       
        public string startdate { get; set; }
        public string enddate { get; set; }
        
        public string tenstartdate { get; set; }
        public string tenenddate { get; set; }
        public ICollection<decimal> expenses { get; set; }
        public decimal? TotalExpenses { get; set; }

        public decimal? NetIncome { get; set; }
        public string tenantname { get; set; }
        public string tenentaddress { get; set; }
        public ICollection<chequeslist> receiptchequedetails { get; set; }
        public ICollection<chequeslist> paymentchequedetails { get; set; }
        public string amcstartdate { get; set; }
        public string amcenddate { get; set; }
        public decimal? amcamount { get; set; }

    }

    public class chequeslist
    {
        
        public decimal? Amount { get; set; }
        public string ChequeNo { get; set; }
      
        public string description { get; set; }
        public  string Bank { get; set; }
        public DateTime? pdcdate { get; set; }
    }

}