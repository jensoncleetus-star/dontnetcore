namespace QuickSoft
{
    // Legacy localization resource (App_GlobalResources/Resources.resx) referenced by the POS receipt view
    // (Views/POSRES/Details.cshtml) as @Resources.X. The resx wasn't carried into the port; these plain
    // English labels let the view compile/render. Swap for a real resx + IStringLocalizer if the
    // POS-Restaurant module is activated and needs localization.
    public static class Resources
    {
        public static string CustomerName => "Customer Name";
        public static string Date => "Date";
        public static string DueAmount => "Due Amount";
        public static string GrandTotal => "Grand Total";
        public static string InvoiceNo => "Invoice No";
        public static string Item => "Item";
        public static string Items => "Items";
        public static string ItemUnit => "Item Unit";
        public static string PaidAmount => "Paid Amount";
        public static string PaymentMethod => "Payment Method";
        public static string POS => "POS";
        public static string Qty => "Qty";
        public static string SubTotal => "Sub Total";
        public static string Tax => "Tax";
        public static string TaxAmount => "Tax Amount";
        public static string TermsAndConditions => "Terms And Conditions";
        public static string TotalAmount => "Total Amount";
        public static string UnitPrice => "Unit Price";
        public static string ViewPOSDetails => "View POS Details";
        public static string Waiter => "Waiter";
    }
}
