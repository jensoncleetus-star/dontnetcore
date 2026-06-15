using QuickSoft.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuickSoft.ViewModel
{
    public class POSEntryViewModel
    {
        public long SENo { get; set; }

        [Required(ErrorMessage = "Invoice No. is required")]
        [Display(Name = "Invoice No.")]
        public string BillNo { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime SEDate { get; set; }

        public string OrderNo { get; set; }

        public long Customer { get; set; }

        public long? SECashier { get; set; }

        public decimal SEDiscount { get; set; }
        public decimal RoundOff { get; set; }

        public decimal SEPaidAmount { get; set; }
        public decimal SEDueAmount { get; set; }


        public decimal SETotal { get; set; }
        public decimal SEGrandTotal { get; set; }

        public string CustomerName { get; set; }

        public string EmployeeName { get; set; }

        public string MobileNo { get; set; }

        public string SENote { get; set; }

        public string PayType { get; set; }
        public string PayMode { get; set; }

        public CustomerType CustomerType { get; set; }

        public bool taxAFdisc { get; set; }

    }

    public class POSRESEntryViewModel
    {
        public long SENo { get; set; }

        [Required(ErrorMessage = "Invoice No. is required")]
        [Display(Name = "Invoice No.")]
        public string BillNo { get; set; }

        [Display(Name = "Date")]
        public DateTime SEDate { get; set; }

        public string OrderNo { get; set; }

        public long Customer { get; set; }

        public long? SECashier { get; set; }

        public decimal SEDiscount { get; set; }

        public decimal SEPaidAmount { get; set; }
        public decimal SEDueAmount { get; set; }


        public decimal SETotal { get; set; }
        public decimal SEGrandTotal { get; set; }

        public string CustomerName { get; set; }

        public string EmployeeName { get; set; }

        public string MobileNo { get; set; }

        public string SENote { get; set; }

        public string PayType { get; set; }
        public string PayMode { get; set; }

        public CustomerType CustomerType { get; set; }

    }

}