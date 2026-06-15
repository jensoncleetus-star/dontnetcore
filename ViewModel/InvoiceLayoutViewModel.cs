using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class InvoiceLayoutViewModel
    {
        public byte Id { get; set; }
        [StringLength(50)]
        public string Name { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public Status Status { get; set; }
        public ICollection<InvoiceField> InvoiceField { get; set; }
    }
    public class InvoiceLayoutViewModel1
    {
        public string InvoiceNo { get; set; }
        public string BillNo { get; set; }

        public string Customer { get; set; }
        public string Employee { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string TRN { get; set; }

        public DateTime Date { get; set; }
        public string PONo { get; set; }
        public string SalesExecutive { get; set; }
        public string PaymentType { get; set; }
        public string Note { get; set; }

        public decimal? Discount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? SubTotal { get; set; }
        public decimal? GrandTotal { get; set; }

        public string NumberWords { get; set; }
        
    }
}