using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    public class ChequePrinting
    {
        //Cheque NO
        public long ChequePrintingId { get; set; }

        
        public string FormateName { get; set; }

        //Formate Type
        
        public string ScaleMode { get; set; }

        
        public string PrintingMode { get; set; }

        //Printer Setting
       
        public string PrinterConfiguration { get; set; }

        
        public double TopMargin { get; set; }
        public double LeftMargin { get; set; }

        //Cheque Setting
        public double ChequeHeight { get; set; }
        public double ChequeWidth { get; set; }


        public ChequePrinting()
        {
            FormateName = "Default";
            ScaleMode = "Inches";
            PrintingMode = "Landscape";
            PrinterConfiguration = "N";
            TopMargin = 2.400;  //Inches
            LeftMargin = 3.700;  //Inches
            ChequeHeight = 400; //Inches
            ChequeWidth = 100; //Inches
        }

    }

    public class ChequeDesign
    {
        public long ChequeDesignid { get; set; }
        public long ChequePrintingId { get; set; }

        public string BankName { get; set; }
        public double BankLeft { get; set; }
        public double BankTop { get; set; }

        public string PayToTo { get; set; }
        public double PayToLeft { get; set; }
        public double PayToTop { get; set; }

        public string Amount { get; set; }
        public double AmountLeft { get; set; }
        public double AmountTop { get; set; }
       

        public string Date { get; set; }
        public double DateLeft { get; set; }
        public double DateTop { get; set; }
        

        public string ChequeNo { get; set; }
        public double ChequeNoLeft { get; set; }
        public double ChequeNoTop { get; set; }
    }

}