namespace QuickSoft.Models
{
    // Stub models for two legacy ORPHAN views — confirmed dead (no controller action renders them, and
    // no View("...") reference targets them). Provided only so build-time Razor compilation of every view
    // is clean. These are plain POCOs not exposed on the DbContext, so EF never maps them (harmless).
    //
    //   Views/FileDocument/Createitemimage.cshtml  @model uploaditemimage  — binds no model fields
    //   Views/Master/ExpenseCreate.cshtml          @model Expense          — binds the fields below

    public class uploaditemimage
    {
    }

    public class Expense
    {
        public string Voucher_No { get; set; }
        public System.DateTime? PayDate { get; set; }
        public string CreatedBranch { get; set; }
        public string PayType { get; set; }
        public string ExpenseType { get; set; }
        public string PayFrom { get; set; }
        public string PayNote { get; set; }
        public decimal? Amount { get; set; }
    }
}
