# Phase 2 — Golden-Output Calculation Validation

**Method:** the app's endpoint output is compared against an independent SQL oracle computed
directly on the same DB copy; separately, stored data is self-checked (header totals vs the sum
of their own line items). Port-faithfulness and 2018-era data quality are thus tested separately.

## Wave 1 results (emirtechlatest — service company, 2026-06-12)

### A. Port faithfulness — the rewritten endpoints compute correctly
| Check | Result |
|---|---|
| `Quotation/GetQuotation` (server→client rewritten endpoint): GrandTotal + TaxAmount + Discount of 40 sampled rows vs the `Quotations` table | **40/40 PASS** (exact to the fils) |
| `Customer/GetCustomer` count vs SQL (from the count-bug fix) | **PASS** — 15,354 = DB exactly (trading: 18,234) |
| Trial balance / AVCO stock valuation | math lives in DB stored procedures (`trialbalance`, `SP_AVCOMethod`) — the SAME SPs legacy used → no port-drift surface; covered by browser UAT |

### B. Stored-data integrity (2018-era data — NOT port issues) → owner decided: keep as history
| Check | Result | Records |
|---|---|---|
| Quotation header SubTotal vs SUM(line items), newest 500 | **3 mismatches** | id 100351 (4,275 vs 2,340) · id 86478 (3,211,174 vs 3,157,364 — **53,810 off**) · id 65452 (5,025 vs 4,725) |
| Invoice header SubTotal vs SUM(lines `Type=0`), newest 500 | **1 mismatch** | Bill **10081** (id 172108): header 1,667.06 vs lines 2,603.06 |
| Invoice header VAT vs SUM(line VAT), newest 500 | **1 mismatch** | same Bill 10081: 83.35 vs 130.15 |

Likely cause for B: lines edited/added after the header totals were saved (a legacy-era save-path
gap). Owner decision (2026-06-12): historical records stay as-is; forward-correctness hardening is
the queued fix (see below).

### Method notes (for future oracles)
- `SEItems.Type`: **0 = real invoice line, 1 = auxiliary row** (44,516 of 70K rows are Type=1!) —
  any oracle/report joining SEItems MUST filter `Type = 0`, or totals inflate.
- `QuotationItems` has no Type flag — quotation lines compare directly.
- Dead hardcode `Stock-in-hand Debit = 2,783,344.19` ×4 in BalanceSheetController — **removed**
  (Phase-3 cleanup commit `8cb9ccc`); it was built but never displayed (its Union stayed commented out).

## Forward-correctness finding (save path)
Invoice/quotation header totals are **client-posted** (the page's running JS totals — e.g. CreditSale
Create reads `SESubTotal=saledata[8]`, `SETax=saledata[9]`, plus a header-level `SEDiscount=saledata[6]`).
So `GrandTotal ≠ SUM(lines)` by design (header discount/round-off), but `SubTotal` should equal
`SUM(line ItemSubTotal where Type=0)`. The rare mismatches are edit-path edge cases, not a formula bug
(999/1000 match). **Safe forward fix (queued as a dedicated pass, not rushed):** after the lines are
persisted, recompute `SESubTotal`/`SETaxAmount` from the saved Type=0 lines just before `SaveChanges`,
in EACH of the ~10 save variants (direct + convert-from quotation/DN/PF/SO/purchase), each guarded by a
create→read→delete golden test so the 99.9%-correct documents are provably unaffected.

## Wave 2 results (quicknetlatest-1200 — trading company, 2026-06-12)
| Check | Result |
|---|---|
| `Quotation/GetQuotation` endpoint vs DB (40 rows of 16,659) | **40/40 PASS** |
| Quotation header vs SUM(lines), newest 500 | **0 mismatches** |
| Invoice header vs SUM(lines `Type=0`), newest 500 | **0 mismatches** |
| Invoice VAT header vs SUM(line VAT), newest 500 | **0 mismatches** |

The trading company's stored data is fully consistent; the 4 outliers found in Wave 1 are the only
header-vs-lines discrepancies in the newest-500 windows of BOTH companies.

## Wave 3 results (2026-06-12)
| Check | Result |
|---|---|
| **Ledger balances** — `Accounts/GetDatalink` per-account Credit/Debit vs `SUM(AccountsTransactions WHERE Status IS NULL)` (25 accounts) | **25/25 PASS** — validates the 60-site projection-Sum rewrite class against a direct SQL oracle |
| **Hr/payroll** — `PayrollReport/GetLeaveSettlement` vs `LeaveSettlements` | **52 = 52 PASS** (only Hr surface with data on this copy) |
| **CSRF enforcement sweep** — 220 grid endpoints WITH token | **0 × HTTP 400** (no regressions; 405/404/500 = the usual GET-only/area-prefix/param test-artifacts) |

## Wave 4 — SP_AVCOMethod rebuild, golden-gated → SWAPPED (2026-06-12)
The stock-valuation SP was rebuilt for speed with the strictest gate of all: **every output row
byte-compared** between the legacy proc and the new one.

| Step | Result |
|---|---|
| Legacy determinism check (run old twice, same data) | 🚨 **two different outputs** — the legacy proc was NONDETERMINISTIC (no ORDER BY on its cursors; same-date movements processed in arbitrary order affect AVCO intermediates). A latent 2018-era flaw: the same report could differ run-to-run. |
| GATE (legacy + deterministic ORDER BY only) self-check | ✓ stable across runs |
| Rebuilt proc vs GATE — all-items, service DB | ✓ **93,479 / 93,479 rows byte-identical** |
| Single-item gates (incl. busiest keep-stock item, 3,511 rows) | ✓ identical |
| Trading bounded gate (`@MCId=1`, 21,021 rows) | ✓ **all 21,021 rows value-identical** (6 rows differed only in the internal insertion-sequence ID — a column the legacy proc itself shuffles run-to-run) |
| **Performance** | service all-items **490–617 s → 8–11 s (~60×)**; trading MCId=1 **46 s → 5–6 s**; single-item 1.78 s → 0.51 s |

Root causes found by measurement (each earlier hypothesis was gate-tested and discarded):
~187,000 `UPDATE … WHERE ID=@AID` against an unindexed table variable (full scan each — the real
O(N²)) + ~135,000 per-item assembly statements. Fix = mechanics only (temp table + ID index + one-time
bulk movement assembly + merged scalar reads); the AVCO math statements are untouched.

**NEW legacy-defect discovery:** the all-items run (`@MCId=0`) **fails on the trading company's data
in the LEGACY proc itself** — `Arithmetic overflow converting numeric to numeric` (trading's larger
values overflow the proc's internal `DECIMAL(18,2)` running totals). Full-inventory valuation has
NEVER completed on trading data with these parameters; the rebuilt proc inherits the same arithmetic
(defect parity by design). Widening the precision would CHANGE accounting outputs → logged as an
owner-decision item, not silently fixed.

**SWAP APPLIED to both test copies** via [`Sql/04_AVCO_Performance.sql`](Sql/04_AVCO_Performance.sql)
(same proc name — the app is untouched; old body preserved as `SP_AVCOMethod_LEGACY`). Post-swap
verification under the original name: service 93,479 rows hash-equal in 11 s; trading 21,021 rows
hash-equal in 6 s; app screen `StockReport/GetMoment` 200 in 0.3 s. Production receives this on
cutover night per the runbook (§2.2) — the live DB is never touched before that.

## Next waves
1. Forward-correctness pass (#35): header-recompute on save, golden-tested per save-variant.
2. `CreditSale/GetSalesEntry` golden under an MC-assigned user (material-center row-scoping).
3. Payment/Receipt row goldens; payroll goldens on live-scale data (Phase 5).
4. Owner's customer-reported calculation complaints (list requested) become priority oracles.
5. Owner decision: AVCO trading all-items overflow — widen internal precision (changes outputs; needs
   accountant approval) or keep parity.
