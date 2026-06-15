using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.ViewModel
{
    public class ChequePrintingViewModel
    {

        [Display(Name = "Template No")]
        public long ChequePrintingId { get; set; }

        [Display(Name = "Formate Name")]
        public string FormateName { get; set; }

        //Formate Type
        [Display(Name = "Scale Mode")]
        public string ScaleMode { get; set; }

        [Display(Name = "Printing Mode")]
        public string PrintingMode { get; set; }

        //Printer Setting

        [Display(Name = "Printer Configuration")]
        public string PrinterConfiguration { get; set; }

        [Display(Name = "Top Margine")]
        public double TopMargin { get; set; }

        [Display(Name = "Left Margine")]
        public double LeftMargin { get; set; }

        //Cheque Setting
        [Display(Name = "Cheque Height")]
        public double ChequeHeight { get; set; }

        [Display(Name = "Cheque Width")]
        public double ChequeWidth { get; set; }

        public  ChequePrintingViewModel()
        {
            ScaleMode = "cm";
            PrintingMode = "Landscape";
            PrinterConfiguration = "N";
            TopMargin = 2.400;  //Inches
            LeftMargin = 3.700;  //Inches
            ChequeHeight = 400; //Inches
            ChequeWidth = 100; //Inches
        }
    }
    //public class ChequeDesign
    //{
    //    public long ChequeDesignid { get; set; }
    //    public long ChequePrintingId { get; set; }

    //    public string BankName { get; set; }
    //    public double BankLeft { get; set; }
    //    public double BankTop { get; set; }

    //    public string PayToTo { get; set; }
    //    public double PayToLeft { get; set; }
    //    public double PayToTop { get; set; }

    //    public string Amount { get; set; }
    //    public double AmountLeft { get; set; }
    //    public double AmountTop { get; set; }


    //    public string Date { get; set; }
    //    public double DateLeft { get; set; }
    //    public double DateTop { get; set; }


    //    public string ChequeNo { get; set; }
    //    public double ChequeNoLeft { get; set; }
    //    public double ChequeNoTop { get; set; }
    //}

   
}