# Accounts Invariants Audit — findings & forward fixes

**Date:** 2026-06-13 · **Scope:** AccountsTransactions double-entry integrity on BOTH company copies
(service: `emirtechlatest`, 221,690 rows / trading: `quicknetlatest-1200`, 857,536 rows).
**Policy (owner):** historical figures stay untouched (customers need their old statements);
the accounting standard is enforced **going forward**.

## 1. The invariant and the headline numbers
A voucher = all `AccountsTransactions` rows of one `(Purpose, reference)`. Double-entry requires
SUM(Debit) == SUM(Credit) per voucher.

| | service co. | trading co. |
|---|---|---|
| global SUM(Debit) − SUM(Credit) | **−1,600,606.34** | **−211,140,748.95** |
| vouchers total | 86,698 | 321,200 |
| vouchers unbalanced | 3,552 (4.1%) | 12,806 (4.0%) |

The deltas decompose **fully** into the classes below — and critically, **the most recent
production day's vouchers are all perfectly balanced**, i.e. the day-to-day flow is healthy.

## 2. Classification

### 2.1 Legacy one-sided conventions (largest share — EXPECTED, kept)
- **Stock Adjustment**: posted as a single row (e.g. one credit to 499 "Stock Adjustable Expense",
  no offsetting leg). 3,342 vouchers / −1.1M (service); 9,905 / −13.2M (trading).
- **Opening Balance**: one-sided per account by design. 52 / −440K (service); 565 / −197.9M (trading
  — this alone is ~94% of the trading delta).
- These are how the legacy product works; "fixing" them would rewrite history and change every
  report. **Kept as-is; documented.** (Optional forward change = owner/accountant decision D-A below.)

### 2.2 Discount pairing convention (EXPECTED, verified healthy)
Receipt discounts post under Purpose `Discount Allowed`, payment discounts under
`Discount Received` — each balances against its parent `Receipt`/`Payment` voucher.
Checked as pairs: service co. **0** unbalanced receipt-pairs, 4 payment-pairs;
trading 11 / 110 (historical edits).

### 2.3 Historical fils/rounding defects in 'Sale' vouchers (HISTORICAL, kept)
60 (service) / 380 (trading) sale vouchers off by small amounts (service avg 0.39 AED).
Anatomy of a sample (service ref 210296): revenue 238.09 cr + VAT 11.91 cr = customer 250.00 dr
**exactly balanced**, plus a stray 0.01 **debit** to 497 (the bill-rounding discount row) with no
offset — the legacy round-off discount row sometimes posts without adjusting the revenue leg.
Trading also carries larger old gaps (max −25,645.70, mostly 2018-2023 era edits/deletes).
**History kept** per policy; the new system's voucher writes are watched via
`Sql/99_Voucher_Balance_Check.sql` (last query) after go-live.

### 2.4 FIXED FORWARD — discount account heads were CROSSED (real defect, both companies)
Both charts of accounts seed **497 = "Discount Allowed"** and **498 = "Discount Received"**, but the
payment/receipt flows hardcoded them the wrong way around:

| flow | posts purpose | OLD account | should be |
|---|---|---|---|
| Payment / PaymentApproval / PRApprovals / PPayment / PDCRegularise | `Discount Received` | 497 (Allowed) ✗ | **498** |
| Receipt / ReceiptApproval / PReceipt | `Discount Allowed` | 498 (Received) ✗ | **497** |

Sale-time and purchase-time discounts already used the right heads — only the settlement-time
discount rows were crossed. Misposted totals: service ~3.78 + 86.60 AED; trading ~21,276.92 +
2,051.94 AED (P&L discount heads mutually misstated; net effect ≈ 0).

**Fix (41 sites, 8 controllers):** writes/updates now post `Discount Received → 498` and
`Discount Allowed → 497`; every read/remove/update lookup now matches BOTH accounts
(`Account IN (497,498) AND Purpose = …`) so old-era rows keep working in edit/void/regularise flows.
Historical rows are NOT migrated (policy).

**Proof (live forward test on the service copy, 2026-06-13):**
- Payment 96192 (pay 99, discount 1): cash cr 99.00 / supplier dr 100.00 /
  **Discount Received cr 1.00 @ 498** ✓ — voucher balanced.
- Receipt 95241 (receive 99, discount 1): cash dr 99.00 / customer cr 100.00 /
  **Discount Allowed dr 1.00 @ 497** ✓ — voucher balanced.
(The two test documents remain on the dev copy; copies are QA-polluted by design.)

### 2.5 FIXED FORWARD — dormant PDC-regularise discount bug
`PDCRegulariseController` looked up the receipt-discount row at account **497** while receipts wrote
it at 498 → the lookup NEVER matched, so regularising a discounted PDC receipt silently skipped
re-dating the discount row. Worse, the dormant update would have rewritten the row on the **credit**
side (the receipt's discount row is a debit). Fixed: both-era lookup + the update now preserves the
debit side. (Payment-side PDC lookup healed by the same both-era predicate.)

### 2.6 Minor data quirks (documented only)
- 1 trading row with junk Purpose `dd` (493.86 dr, manual 2019-era).
- A handful of manual Journal rows posted against 497/498 directly (user-chosen; legitimate).

## 3. Owner / accountant decision menu
- **D-A**: keep one-sided Stock-Adjustment & Opening-Balance vouchers forward (current behaviour),
  or have the new system post offsetting legs (changes P&L/BS presentation; needs accountant rule).
- **D-B**: optionally reclassify the small historical misposted discount totals (a single manual
  journal per company would move them between 497↔498) — cosmetic; accountant's call.
- **D-C (existing)**: AVCO precision widen (trading all-items report overflow) — pending.
- **D-D (new, §3a)**: migrate the `auditemirtechlatest` archive DB too (only if pre-2025 service-company
  stock valuations must match the legacy box byte-for-byte); otherwise the local-`Items` fallback applies.

## 3a. Batch 10 — six core Accounts REPORTS were dead since the port (fixed 2026-06-13)
A read-only multi-agent audit of every Trial Balance / Balance Sheet / P&L / Cashflow endpoint
(probed live on :8080, then adversarially re-verified) found **6 broken report actions** — all 500,
none from a single bad screen. Three distinct root causes, all now fixed (`Sql/05_AccountsReports_Fixes.sql`
for the DB-side; controller changes ship in the binary):

| # | endpoints | root cause | fix |
|---|---|---|---|
| A | GetBalanceSheet, **GetBalanceSheetemirtech** (live form default), GetBalanceSheetqucknet, GetProfitAndLoss | `SP_AVCOMethod` opening-stock branch hardcoded `auditemirtechlatest.dbo.Items` — a per-company **archive DB** that exists on the legacy box but is **not** in the new-server migration set → `Invalid object name`. (The trading SP names the *service* archive — proof the literal name is a copy-paste artifact; intended source is `audit`+DB_NAME().) | synonym `dbo.AvcoAuditItems` → the company's own audit DB **if it exists** (legacy-faithful), else local `dbo.Items`; SP repointed at the synonym. Zero historical-figure change where the archive is present. |
| B | GetTrialBalance, GetTrialBalancefortrial | `trialbalance` SP omitted the non-nullable `orderB` column the result class requires (siblings already emit it) → `FromSql` missing-column throw | add `0 as orderB` to the SP SELECT (ordering seed, overwritten downstream — value-neutral) |
| C | Getcashflow, Getcashflowdetailed, GetGroupTrialBalancebalancesheetcashflow | a ~30k-element `reference[]`/`refs[]` array materialized then `.Contains(a.reference)` — EF inlined it as a literal `IN (…30 000 consts…)`, so SQL Server threw **8623** "query processor ran out of internal resources" at full-FY scale (worked on narrow ranges) | keep the pre-pass as an **IQueryable** → SQL `IN (subquery)`, not constants. Output identical; the critic's "delete refs" idea was rejected because `refs` and `b.Group==AccGroup` are *different* filters (both needed for cashflow contra-leg logic). |

**Why this matters now:** the new production server will **not** carry the `auditemirtechlatest` archive (it is not in the runbook's migration set), so without Fix A the Balance Sheet and P&L would 500 on the new system even though they work on the legacy box today.

**Proof (live, both companies, 2026-06-13):** all 6 endpoints now 200 at full-FY (01-01-2010..31-12-2026):
GetBalanceSheetemirtech 1s, GetProfitAndLoss 1s, GetBalanceSheet 1s, GetBalanceSheetqucknet 1s,
GetTrialBalance 1s, GetTrialBalancefortrial 1s, Getcashflow 7s, GetGroupTBcashflow 1s,
Getcashflowdetailed 3s for a 1-year range (69s only for the all-history 2.6 MB extract — an unpaged-report
perf note, not a defect). 220-endpoint regression sweep unchanged. `Sql/05` is idempotent and re-runnable.

**Owner note (D-D):** if you want pre-2025 **service-company** stock valuations on the new server to match
the legacy box *byte-for-byte*, also migrate the `auditemirtechlatest` archive DB alongside `emirtechlatest`;
the synonym will pick it up automatically. If you don't (default), the report computes pre-2025 opening
stock from the current `Items` master — it works and is internally consistent, but the pre-2025 item-master
snapshot (if it ever differed) won't apply. Trading is unaffected (its archive reference was the copy-paste bug).

## 4. Watching the books after go-live
Run [`Sql/99_Voucher_Balance_Check.sql`](../Sql/99_Voucher_Balance_Check.sql) (ships in the deploy
bundle; report-only) the morning after go-live and at week-1/month-end. The last result set lists
recent unbalanced vouchers excluding the documented one-sided conventions — it should stay empty;
the discount-heads result should show new rows landing on 497-Allowed / 498-Received only.
