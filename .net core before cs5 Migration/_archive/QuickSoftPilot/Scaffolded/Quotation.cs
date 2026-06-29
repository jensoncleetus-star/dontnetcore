using System;
using System.Collections.Generic;

namespace QuickSoftPilot.Scaffolded;

public partial class Quotation
{
    public long QuotationId { get; set; }

    public long QuotNo { get; set; }

    public string BillNo { get; set; }

    public DateTime QuotDate { get; set; }

    public long? QuotCashier { get; set; }

    public long Customer { get; set; }

    public int QuotItems { get; set; }

    public decimal QuotItemQuantity { get; set; }

    public decimal QuotSubTotal { get; set; }

    public decimal QuotTax { get; set; }

    public decimal QuotTaxAmount { get; set; }

    public decimal QuotDiscount { get; set; }

    public decimal QuotGrandTotal { get; set; }

    public string QuotNote { get; set; }

    public int Mail { get; set; }

    public int? QuotValidity { get; set; }

    public string TermsCondition { get; set; }

    public long EmailTemplateId { get; set; }

    public long CompanyHeaderId { get; set; }

    public DateTime QuotCreatedDate { get; set; }

    public long Branch { get; set; }

    public string CreatedUserId { get; set; }

    public int Status { get; set; }

    public long? CreatedBranchBranchId { get; set; }

    public string Remarks { get; set; }

    public long? Project { get; set; }

    public long SalesType { get; set; }

    public string PaymentTerms { get; set; }

    public long? ProTask { get; set; }

    public string Ref1 { get; set; }

    public string Ref2 { get; set; }

    public string Ref3 { get; set; }

    public string Ref4 { get; set; }

    public string Ref5 { get; set; }

    public long? Leadsid { get; set; }

    public DateTime? Expdate { get; set; }

    public int? Quotationstatus { get; set; }

    public string Revision { get; set; }

    public int SaleType { get; set; }

    public long? QuotationType { get; set; }

    public long? Sourceoflead { get; set; }

    public long? Currency { get; set; }

    public string ConvertionRate { get; set; }

    public decimal? Fctotal { get; set; }

    public long? Servicetype { get; set; }

    public virtual ICollection<QuotationItem> QuotationItems { get; set; } = new List<QuotationItem>();
}
