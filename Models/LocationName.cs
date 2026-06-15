using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuickSoft.Models
{
    public class LocationName
    {

        [Key]
        public long LocationId { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Location")]
        public string Location { get; set; }
        [Required]



        [Display(Name = "State")]
        public long StateId { get; set; }
    }
    public class routemap
    {

        [Key]
        public long routeid { get; set; }
        public long locationid { get; set; }
        public long nearestlocationid { get; set; }
        public int locationorder { get; set; }
       
       
    }
    public class leaddashbordorder
    {

        [Key]
        public long leaddashboardid { get; set; }
        [Required]

        [Display(Name = "Lead Name")]
        public long lead { get; set; }
        [Required]
        [Display(Name = "Order")]
        public long dashboardposition { get; set; }
        [Display(Name = "Duration (Minutes)")]
        public int? duration { get; set; }
    }
    public class protaskdashbordorder
    {

        [Key]
        public long protaskdashboardid { get; set; }
        [Required]

        [Display(Name = "Lead Name")]
        public long task { get; set; }
        [Required]
        [Display(Name = "Order")]
        public long dashboardposition { get; set; }
        [Display(Name = "Duration (Minutes)")]
        public int? duration { get; set; }
    }
    public class ChequeBook
    {

        [Key]
        public long bookid { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Book Name/Bank Name")]
        public string bookname { get; set; }
        [Required]



        [Display(Name = "Book type")]
        public Docbooktype booktype { get; set; }

        [Display(Name = "Doc Start Number")]
        public long numberstarting { get; set; }

        [Display(Name = "Doc End Number")]
        public long endnumbering { get; set; }
        [Display(Name = "Cancelled Leaf")]
        public long cancelledleaf { get; set; }
        [Display(Name = "Used Leaf")]
        public long usedleaf { get; set; }


    }
    public class chequetransactionviewmodal
    {
        [Key]
       public long chequetransid { get; set; }
public long bookid { get; set; }
public long? referenceno { get; set; }
public string purpose { get; set; }
[Display(Name ="Reason")]
public string remarks { get; set; }
public Docbooktype transtype { get; set; }
public decimal? amount { get; set; }
        [Display(Name = "Date")]
        public string transdate { get; set; }
        [Display(Name = "Enter Valid Document No")]
        public long docserialno { get; set; }
    }
    public class chequetransaction
    {
        [Key]
        public long chequetransid { get; set; }
        public long bookid { get; set; }
        public long? referenceno { get; set; }
        public string purpose { get; set; }
        [Display(Name = "Reason")]
        public string remarks { get; set; }
        public Docbooktype transtype { get; set; }
        public decimal? amount { get; set; }
        [Display(Name = "Date")]
        public DateTime transdate { get; set; }
        [Display(Name = "Enter Valid Document No")]
        public long docserialno { get; set; }
    }
}