using System.Linq;
using QuickSoft.Models;

namespace QuickSoft.Helpers
{
    // Forward-correctness (modernization, 2026-06 — audit/golden finding):
    // a handful of historical documents had header SubTotal/TaxAmount that no longer matched the
    // SUM of their own line items (lines edited after the client-posted header totals were saved).
    // These helpers run at the END of every save path that (re)writes a document's lines and make
    // the header equal the saved lines again.
    //
    // Deliberately NO-OP when the header already matches (the 99.9% case) — zero behaviour change
    // for correct flows. GrandTotal/Discount are NOT touched: the header-level discount and the
    // grand-total rule remain exactly as the page computed them (legacy business behaviour).
    public static class DocumentTotals
    {
        /// <summary>Header SESubTotal/SETaxAmount := SUM of the invoice's REAL lines (Type=false).</summary>
        public static void RecomputeSalesEntry(ApplicationDbContext db, long? salesEntryId)
        {
            if (salesEntryId == null || salesEntryId == 0) return;
            var id = salesEntryId.Value;
            var header = db.SalesEntrys.Find(id);
            if (header == null) return;

            var lines = db.SEItemss.Where(x => x.SalesEntry == id && x.Type == false);
            var sub = lines.Sum(x => (decimal?)x.ItemSubTotal) ?? 0m;
            var tax = lines.Sum(x => (decimal?)x.ItemTaxAmount) ?? 0m;

            if (header.SESubTotal != sub || header.SETaxAmount != tax)
            {
                header.SESubTotal = sub;
                header.SETaxAmount = tax;
                db.SaveChanges();
            }
        }

        /// <summary>Header QuotSubTotal/QuotTaxAmount := SUM of the quotation's lines.</summary>
        public static void RecomputeQuotation(ApplicationDbContext db, long? quotationId)
        {
            if (quotationId == null || quotationId == 0) return;
            var id = quotationId.Value;
            var header = db.Quotations.Find(id);
            if (header == null) return;

            var lines = db.QuotationItems.Where(x => x.Quotation == id);
            var sub = lines.Sum(x => (decimal?)x.ItemSubTotal) ?? 0m;
            var tax = lines.Sum(x => (decimal?)x.ItemTaxAmount) ?? 0m;

            if (header.QuotSubTotal != sub || header.QuotTaxAmount != tax)
            {
                header.QuotSubTotal = sub;
                header.QuotTaxAmount = tax;
                db.SaveChanges();
            }
        }
    }
}
