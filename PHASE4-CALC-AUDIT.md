# Phase 4 — Calculation / Correctness Audit

**Date:** 2026-06-29 · **Method:** 8 parallel code-inspection auditors across the financial controllers + `Models/Common.cs` + `Helpers/DocumentTotals.cs`, every finding type-traced and verified against sibling code before action.
**Important:** code inspection finds *mechanically wrong* arithmetic; it is **not** a substitute for the golden-output comparison vs the legacy system (Phase-2 plan). Anything whose "correct" value depends on a business rule was **reported, not changed**.

> Reassuring baseline: **money types are sound** (all currency is `decimal` end-to-end — no `double`/`float`/`(int)` truncation in any save path), and the **live Sale / Sale-Return / Payment / Receipt posting paths are arithmetically balanced** with correct debit/credit signs. The issues are concentrated in **report controllers** and a few **edge-case crashes**.

---

## ✅ FIXED — mechanically unambiguous (build-verified, 0 errors)

| # | Bug | File | Fix |
|---|-----|------|-----|
| **C1** | UAE VAT return: **Fujairah net sales-VAT** subtracted the return *taxable base* instead of the return *VAT amount* (every other emirate uses `...SRTaxAmt`; `FujSRTaxAmt` was computed but unused) | [TaxReportsController.cs:882](Controllers/TaxReportsController.cs) | `FujSRTaxable` → `FujSRTaxAmt` |
| **C2** | Same bug on the **Fujairah net purchase-VAT** cell | [TaxReportsController.cs:938](Controllers/TaxReportsController.cs) | `FujPRTaxable` → `FujPRTaxAmt` |
| **C3** | **Quotation autosave** persisted `QuotSubTotal/QuotTaxAmount = 0`: `RecomputeQuotation` ran **before** the SP inserted the lines (so it summed 0 lines). `CreateQuotation` does it in the right order. | [QuotationController.cs:1730](Controllers/QuotationController.cs) | Swapped: insert → recompute |
| **C4** | Same inverted order in **UpdateQuotation** (zeroed the header on every edit) | [QuotationController.cs:3070](Controllers/QuotationController.cs) | Swapped: insert → recompute |
| **C5** | Daily-summary **physical-cash** figure: `Account==504 \|\| Account==3 && date` — `&&` binds tighter than `\|\|`, so `504` rows ignored the date bound (pulled future-dated cash in) | [SalesReportController.cs:11221](Controllers/SalesReportController.cs) | Parenthesized: `(504 \|\| 3) && date` |
| **C6** | **Payment Edit** (mode-unchanged branch) debited the supplier `PaytoAmount − Discount`, leaving the entry **unbalanced by the discount** (sibling branch + Create use `PaytoAmount`) | [PaymentController.cs:1612](Controllers/PaymentController.cs) | `PaytoAmount-Discount` → `PaytoAmount` (×2) |
| **C7** | `DivideByZeroException` on weighted-avg purchase price when rows net to zero qty (guard checked row *count*, not the divisor) | [BalanceSheetController.cs:1546](Controllers/BalanceSheetController.cs) | Guard `&& totalstock != 0` |
| **C8** | `DivideByZeroException` deriving per-unit stock cost on item save when `OpeningStock==0` (guard only checked `StockValue`) | [Common.cs:4525](Models/Common.cs) | Guard `&& OpeningStock != 0` |

C1–C6 are **port defects or objective errors** (one place inconsistent with its own correct siblings, or a provably-unbalanced ledger / wrong operator precedence) — fixing them restores the intended/consistent behaviour. C7–C8 are pure crash-prevention (no change to valid-case output).

---

## ⚠️ NEEDS OWNER DECISION / GOLDEN VALIDATION — reported, NOT changed (exact fix given)

These change *saved or reported financial values* where "correct" depends on business rules or legacy convention — per the golden-output methodology, **you decide** (port-defect → fix to match legacy, vs pre-existing legacy behaviour → documented business change). I can apply any of these on your say-so.

| # | Issue | Location | Recommended fix | Why held |
|---|-------|----------|-----------------|----------|
| ~~N1~~ **✅FIXED** | **Payroll per-day off-by-one + crash.** `int days = ts.Days;` then `Rate/days`. Verified: identical computation to sibling `SalaryStructureController:446` which uses `ts.Days + 1`; `Basis=="AsperCalenderPeriod"` ⇒ inclusive calendar days (Jun 1–30 = 30, not 29). The voucher must agree with the structure that defines the rate. Objective error + `From==To` crash. | [PayrollVoucherController.cs:679](Areas/Hr/Controllers/PayrollVoucherController.cs) | **Applied:** `ts.Days + 1` (matches sibling). | Resolved — was the only saved-value calc bug. **Recommend** spot-checking one payslip total after restart. |
| ~~N2~~ **✅FIXED** | **AP payment posted both legs to cash.** Standalone `Payment` action debited AND credited `cashAccId` → net-zero on cash, supplier AP never reduced. | [PurchaseEntryController.cs:6623](Controllers/PurchaseEntryController.cs) | **Applied:** resolve `suppAccId = Suppliers…Select(a => a.Accounts)` (exactly as create path 2594) and debit it; credit stays cash. Now a balanced supplier-debit / cash-credit. | Resolved. ⚠️**Historical** entries written by the old code (both-to-cash) are pre-existing data — a separate one-off ledger correction for past payments is the owner's call. |
| ~~N3~~ **✅FIXED** | **DayWise report**: a dead one-to-many `HireDetails` join multiplied every header total ×N for hire invoices, and all filters (`Status==1`, date, MC) were commented out → aggregated *all* sales ever. | [SalesReportController.cs:12975](Controllers/SalesReportController.cs) | **Applied:** removed the unused join; restored the filters to exactly mirror the `sreturn` sibling in the same method (standard EF — low runtime risk). | ⚠️Report numbers change (now correctly excl. cancelled/other-MC). **Golden-check** vs the monthly report. |
| ~~N4~~ **✅FIXED** | **Employee-wise report**: `SaleAmt` summed `SESubTotal` with no discount; 4 columns used `group by Customer … FirstOrDefault().Total`, dropping all but the first customer for multi-customer employees (+ NRE on no-sales). | [SalesReportController.cs:10729](Controllers/SalesReportController.cs) | **Applied:** `SaleAmt = i.SESubTotal − i.SEDiscount`; the 4 columns now `.Sum(z => z.Total)` across customer groups (line-scoped to the employee-wise block only — the identical customer-wise block at 8705+ is *correct* and was left untouched). | ⚠️**Runtime-smoke** the employee-wise report once (EF nested-aggregate) + golden-check. Trivial revert if it errors. |
| **N5** | **Material cost ÷ ConFactor²**: `getbatchmaterialcostdt` divides *both* qty and price by ConFactor (sibling `getbatchmaterialcost` multiplies price). Sub-unit items understated by ConFactor². | [Common.cs:510](Models/Common.cs) | `ItemUnitPrice * ConFactor` (like the sibling) | **Identical in the MVC5 backup → faithful legacy**; fixing diverges from golden |
| **N6** | **Receivable report fan-out**: `GetReceivable` left-joins `SalesReturns` one-to-many, so a multi-return invoice yields N rows with duplicated bill/paid amounts. Sibling `GetReceivableLedger` uses a scalar `Sum(SRGrandTotal)`. | [MyReportsController.cs:2539](Controllers/MyReportsController.cs) | Replace the join with a scalar subquery (like the sibling) | Present in MVC5 backup → faithful legacy |
| **N7** | **VAT-report ledger** reconstructs VAT as `GrandTotal/1.05*5/100` (5%-inclusive) instead of reading stored `SETaxAmount`. Wrong for zero-rated/exempt or non-5% lines. | [Common.cs:6281](Models/Common.cs) | Sum stored `SETaxAmount` | Possibly a deliberate inclusive reconstruction — confirm convention |
| **N8** | **Admin re-posting tool** `salesrtrewritebook`: bill-sundry legs pass the Debit operand with a `DC.Credit` label (and vice-versa) — operand/label disagree. | [CreditSaleController.cs:13869](Controllers/CreditSaleController.cs) | Align operands with the intended `DC` sign | Admin/dev-only maintenance action; confirm intended sundry sign |
| **N9** | **GrandTotal ignores the header Discount field** if a discount is typed in the Discount box rather than entered as a bill-sundry (client computes `Σline + sundries`, no `− Discount` term). | `wwwroot/Content/js/qutinvoice.js` (Quotation/Estimate) | Subtract the header discount, or enforce sundry entry, or validate server-side | UI/workflow decision |

---

## Verified CLEAN (no mechanical error)
Live Sale/Sale-Return/Payment/Receipt create paths (balanced, correct signs) · `DocumentTotals.cs` · per-line tax `subtotal*rate/100` (decimal) · SalesOrder/StockJournal/StockTransfer/Deliverynote totals · Purchase create-path journal · opening-balance & ledger build · date boundaries (consistently inclusive) · the `(int)(qty/ConFactor)` + `% ConFactor` unit decomposition (intentional quotient/remainder, not truncation) · all `Math.Round(decimal, 2)`.

---
*Fixes C1–C8 applied & build-verified. Apply on next app restart. None touch the DB schema (old-DB-safe).*
