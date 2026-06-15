# Phase 1 — Full System Audit (initial findings)

**Date:** 2026-06-12 · **Scope:** the .NET 10 port (post-migration) on the two company DB copies.
**Status: IN PROGRESS** — findings below are confirmed; per-module calculation validation continues in Phase 2 with golden-output comparison.

## 1. What is verified WORKING (migration evidence, carried forward)
- 204 + 43 controllers, every action present; 2,017/2,017 views compile; 486 grid endpoints, CRUD,
  PDF, approvals verified on BOTH companies (service + trading) with full-role login.
- 7 owner-UAT bugs fixed (2026-06); EF-translation family fixed app-wide; schema aligned.

## 2. Performance findings (→ Phase 3 backlog)
| # | Finding | Evidence | Recommendation |
|---|---|---|---|
| P1 | `SP_AVCOMethod` all-items stock valuation takes **~8.6 min** (per-item cursor over 6,738 items) | timed 519s/92K rows; **re-timed 524s AFTER adding the movement-table indexes (2026-06-12) → indexing does NOT move it; the cost is the cursor's row-by-row processing itself** | The only real fix is a set-based rewrite of the SP, gated by a 100%-row golden compare (old SP vs new SP on both company DBs) before swapping. Queued as its own pass. Per-item path (normal screens) is 120 ms — unaffected and fast. |
| P2 | ~~Heavy grids materialize ALL rows then page client-side~~ → **LARGELY RESOLVED 2026-06-12/13**: hybrid server-side paging (no-search + plain-column sort → server COUNT/ORDER BY/OFFSET-FETCH; anything else falls back to the original path) shipped on **28 grids** (batches 1–5, 2026-06-12/13): Quotation (7.2s→0.13-0.4s, ~20-50×), Payment (1.25s→0.35s), Receipt (0.89s→0.07s), Item (0.7s→0.12s), AMC (0.2s/784), Deliverynote, Estimate, AMCPeriodic, LeadsTest, MaterialRequisition, PaymentApproval, PRApprovals, ReceiptApproval, + batch-4/5 guard-mirrored grids (CreditSaleReturn, CreditSaleReturnNote, JobCard, MRNote, ProForma, PurchaseOrder ×2, PurchaseQuotation, PurchaseReturn 1.9s→85ms, PurchaseReturnNote, SalesOrder, StockJournal, StockTransfer 307ms, Unassemble, PurchaseEntry ×2 — filter-active requests transparently fall back to the original path; forced-fallback parity verified). Sorted pages == SQL oracle exactly; recordsTotal unchanged; 220-endpoint regression identical to baseline after every batch. **Batch 6 (2026-06-13) — Leads N+1 removal:** GetAllLeads / GetAllLeadsreport / GetAllMyLeads each ran 1-2 correlated DB queries **per result row** (custdetails + mobmodel; MyLeads' tail re-enumerates up to 3×) — ~6,200 queries per default Leads load, ~30,000 with a filter set. Replaced with set-based prefetch lookups over the surviving lead ids; GetAllLeadsreport additionally skips the ~107K-row `tksup` materialization when no type/employeename filter is active. Gated by 56/56 byte-identical golden responses across a 9-10-case param matrix on BOTH companies. Warm timings: grid days-filter 12.9s→0.43s (30×), mobile-filter 9.8s→0.36s (27×), default 2-3s→0.14s; report default 1.1s→0.05-0.4s; MyLeads 0.31s→0.05s. **Batch 7 (2026-06-13) — two BROKEN lead endpoints repaired:** `GetAllLeadsdash` computed `let g`/`let m`/assigned-employees per row server-side — any status with a few thousand leads blew the 30 s SQL command timeout, so the lead-dashboard drill-down failed outright (and took ~4 s even for 17 rows). `GetAllLeadsexp` (expired-leads drill) threw `InvalidOperationException` on EVERY call since the port (`DbFunctionsCompat.AddMinutes` + the Mobiles-UNION-contacts projection are untranslatable; legacy EF6 evaluated them). Both restructured to the proven base-query → set-based-lookups shape (AddMinutes → native `DateTime.AddMinutes` = the same DATEADD legacy EF6 emitted; keyed GroupBy → Distinct). Proof: dash 12/12 byte-identical golden vs old on the old-reachable small statuses (both companies; null-ldate tie order preserved via ThenBy(id)); big statuses now 0.4-0.9 s / capped 300 rows (was >30 s timeout); exp verified against an independent SQL oracle (DATEADD + EF C# null-semantics): 16==16 (trading) and 21==21 (service). **Remaining skips (documented, deferred):** PurchaseEntry payment work-queues 3346/3465 (unconditional approval filters; 0 rows on test copies — wire against live data post-go-live), and ~8 small/param-bounded variants. Template + wiring scripts in git history `30eafb5..e07052b`. | — | mostly done |
| P3 | ~~First hit of each screen compiles its view at runtime~~ → **RESOLVED 2026-06-13**: `ViewWarmupService` (background hosted service) pre-compiles the 25 hottest views ~5 s after service start (25/25 in ~9 s on both pilots); cold-process first hits now 0.16–0.95 s | runtime Razor by design (CS8103 prevents precompile) | done |
| P4 | No static-asset cache headers / no output caching of lookup dropdowns | Program.cs has compression only | Add `Cache-Control` for wwwroot + MemoryCache for hot lookups (units, types, branches) |
| P5 | DB compatibility level is old (2012-era) on the copies | OPENJSON workaround needed | After go-live stability: test compat-level raise on a copy (better planner); not required |
| P6 | 471 raw-SQL call sites (271 SqlQueryRaw/Dedup + 188 ExecuteSqlRaw) | scan | Inventory the hot ones; several can become indexed views/EF queries |

### P-batch 8 (2026-06-13) — the "17×500" sweep bucket fully triaged; 16 dead endpoints revived
The standing 220-endpoint sweep always showed 17 endpoints returning HTTP 500 ("param artifacts").
Deep triage proved **12 of them were REAL port defects**: their projections contain member pairs
differing only by case (`Type` + `type = b.BillType` — both carry *different* data), which MVC5's
JavaScriptSerializer emitted as two JSON keys but System.Text.Json rejects ("property name collides"),
so the endpoints had thrown on EVERY call since the port. These are the **bill/item pickers on the
Payment / Receipt / Journal edit screens** plus Asset-transfer, BOM-offer, Production and Unassemble
item grids — core Accounts workflows. Fix: `BaseController.LegacyJson` (Newtonsoft +
MicrosoftDateFormat) reproduces the exact legacy wire shape (both keys + `\/Date(ms)\/` dates) —
verified value-level on a real payment: `"type":"Purchase"` + `"Type":"Against Reference"` + msdate.
**16 endpoints revived** (the 12 + 4 sister copies outside the sweep list: PPayment/PReceipt area,
TaskItem, StockTransfer). Sweep distribution moved 500: 17 → 5, 200: 52 → 64 — the **remaining 5 are
proven parameter artifacts** (GetTaskItems2, Getdescription, GetItemMC, GetData, getallprifit: each
returns 200 with realistic UI params; with the sweep's bare params legacy MVC5 threw the same way).
**New regression baseline: 200=64, 404=22, 405=129, 500=5, 400=0.**

## 3. Security findings (→ Phase 3 backlog; none block the pilot)
| # | Finding | Evidence | Recommendation |
|---|---|---|---|
| S1 | **Password policy is weak**: length 4, no digit/case/symbol required | Program.cs Identity options (legacy parity) | Raise to ≥8 + complexity; force-change on next login for short passwords (owner decision — affects users) |
| S2 | ~~Anti-forgery coverage 22%~~ → **RESOLVED 2026-06-12: 100% enforcement** — global `AutoValidateAntiforgeryToken` filter live; coverage = BeginForm auto-tokens + the layout's ajaxSend header hook + explicit tokens added to the 13 raw-form views (18 forms) + 2 mobile App pages. Regression: no-token POST → 400 ✓; form-token, header-token, and create/delete write-paths all 200 ✓ | — | done |
| S3 | No HTTPS redirect / HSTS / security headers (X-Content-Type-Options, X-Frame-Options, CSP) | Program.cs | Add at Phase 3; TLS itself lands at deployment (runbook §5) |
| S4 | Old SMTP credential in git history (excluded file) | known | Rotate at go-live (already on the runbook) |
| S5 | `/dev/*` endpoints exist in Development builds | by design, absent in Production (verified 404) | Keep; remove before final hand-off if owner prefers |
| S6 | Raw SQL is parameterized (0 string-concat call sites found in scan) | scan B | Phase 3: manual review of the 471 sites to confirm 100% |
| S7 | Login lacks lockout/throttling for failed attempts (legacy parity) | UsersController | Enable Identity lockout (e.g. 5 tries / 5 min) — low-risk, high-value |

## 4. Code-quality findings (→ Phase 3 backlog)
| # | Finding | Evidence |
|---|---|---|
| C1 | Giant files: `Common.cs` **15,777** lines; `SalesReportController` 14,220; `CreditSaleController` 13,231; `MyReportsController` 12,497; `ProTaskController` 11,424; `ItemController` 10,977 (+6 more >7K) | refactor into per-module services AFTER golden tests exist |
| C2 | **37,353 commented-out lines** in Controllers alone | bulk-remove in Phase 3 (git keeps history) |
| C3 | Duplicate viewmodel classes (e.g. `custmodal` ×2) and copy-pasted query blocks across report controllers | consolidate |
| C4 | Legacy patterns: ad-hoc `new ApplicationDbContext()` per action (no DI scope), `JavaScriptSerializer` usage, magic strings for roles/types | normalize gradually behind the golden tests |
| C5 | Build hygiene: NoWarn list hides 13 warning families (faithful-port noise) | tighten per family in Phase 3 |

## 5. UI/UX findings (→ Phase 4 backlog)
- Bootstrap **3.x** + jQuery 1.x-era plugins: keep (structure mandate) but layer a refreshed theme
  (typography/spacing/colors/buttons) + modern login screen.
- Inconsistencies across modules: mixed button styles/colors, inconsistent toasts, varying table
  densities, icon-less menus in places — all fixable in the shared theme layer without touching layout.
- Responsiveness: desktop-first tables overflow on phone widths on most list screens (legacy
  behaviour); Phase 4 adds the reflow CSS on the top-20 screens.

## 6. Database findings
- Both companies run fine on existing schema (proven). Identity script = only required change.
- Test copies lack live-DB query stats → index tuning deferred to Phase 3 *after go-live* using
  real `sys.dm_db_missing_index_*` / Query Store data from production.
- Large-table candidates for archive policy (AccountsTransactions, SEItemss, stock movement) — size
  them on the real DBs during Phase 3.

## 7. Calculation-validation plan (the Phase 2 core)
Golden-output method: same DB copy → run the same report/screen on legacy (production) and the
new app → outputs must match exactly. Priority: VAT/tax reports → AVCO stock valuation →
invoice/quotation totals → AR/AP ledgers → payroll. Mismatches are triaged with the owner:
**port defect** (fix to match legacy) vs **pre-existing 2018-era bug** (owner decides the correct
behaviour; fix is then a documented business-rule change, not a silent one).

## Next steps
1. Owner reviews this report (+ MODERNIZATION-PLAN.md) → approves/edits the Phase 2+3 backlog.
2. Phase 2 starts with the golden-output harness on the Accounts/VAT reports.
