# Snapshot Divergences — Old (legacy MVC5) vs New (.NET 10 port)

**Audit date:** 2026-06-11 · **DBs:** `emirtechlatest` + `quicknetlatest-1200` (non-production COPIES on `.\SQLEXPRESS`)
**Updated 2026-06-12:** sections A2 + B **RESOLVED** — the test DBs were aligned to the model schema
(missing columns added) and every snapshot accommodation was reverted to the faithful legacy logic.
Verified via `/dev/schemadiff` (0 A2-related phantoms on both DBs) + endpoint tests on both companies.

## Summary
The .NET 10 port faithfully reproduces the legacy MVC5 behaviour. The remaining divergences are only:
A1 (faithful-by-design), C (snapshot-data accommodations to review on live data), D (tables the live DB
has but these copies don't — no code change needed), E (a legacy stored-proc quirk).

---

## A. Phantom-column `[NotMapped]` on entities

### A1 — KEEP (permanent, faithful to EF6) — do NOT revert
These are `long[]` member-lists. EF Core 10 would map them as a primitive-collection column, but the
legacy EF6 never mapped them (kept transient, written to a junction table). `[NotMapped]` matches EF6.
- `Models/AMC.cs` — `AmcRemark.AssignedMembers`
- `Models/ProTask.cs` — `TaskRemark.AssignedMembers`
- `Models/Customer.cs` — `LeadTaskRemark.AssignedTo` (+ the viewmodel copy)

### A2 — ✅ DONE (2026-06-12): columns added to BOTH test DBs, `[NotMapped]` removed
| Table.Column | Type added | Notes |
|---|---|---|
| `BillOfMaterials.Labourcost` / `.meterialcost` | decimal(18,2) NULL | trading copy already had them |
| `Productions.Productioncost` / `.Labourcost` / `.meterialcost` | decimal(18,2) NULL | trading copy already had them |
| `POSOrders.dcharge` / `.tendering` | decimal(18,2) NOT NULL DEFAULT 0 | model has non-nullable `decimal` |
| `AssetTransferDetails.DeleteYN` | nvarchar(max) NULL | |
| `PropertyMains.LandlordID` | bigint NULL | for the GetPLSummaryProperty landlord join |

**At production cutover nothing remains to do for A2** — the live branch DBs already have these columns
(the columns came FROM the live schema); the model now maps them again, faithfully.

## B. Controller read-neutralisations — ✅ ALL REVERTED to legacy logic (2026-06-12)
- `BOMController.GetBOM` — `Expense = a.Expense + Labourcost + meterialcost` restored (legacy line 84)
- `BOMController.GetBOMDetails` — `Expense`/`labourcost`/`materialcost` restored (legacy lines 419-422)
- `MyReportsController.GetProductionSummary` (~8719) — `singlepcost/labourcost/materialcost` use
  `d.Labourcost` / `d.meterialcost` again (legacy lines 8706-8709)
- `MyReportsController.GetPLSummaryProperty` (~9760) — Landlords LEFT join + `landlord` filter restored
  (legacy lines 9740-9743); kept inside the server-side `propRows` query (the server/client split for
  EF Core 10 translatability stays)
- `TaxReportsController.GetSaleTax2` (~566) — `dcharge` = correlated `Sum(e => e.dcharge)` over
  `POSOrders` (same value as legacy's nested `.ToList().Sum()`, server-translatable)
- `AssetFromInventoryController` 653/745 + `MyReportsController` ~11264 — **no functional change ever
  needed**: legacy never used `DeleteYN`, and the narrow projections (null-safe casts / no SELECT *)
  produce identical output. Stale "column absent" comments cleaned up.

Verified: all six endpoints return 200 on BOTH companies (tables are empty in these copies → recs=0 is
genuine data, not an error).

---

## C. Behavioural changes for the STATIC snapshot — review on live data
The static copy has no recent activity, so "recent-only" default filters showed nothing. Relaxed so UAT
sees data; on LIVE data confirm the owner wants the original recent-window vs a top-N-by-recency cap:
- `Controllers/ProTaskController.cs:2588` — recent-filter relaxed (`flag == 1 || true`) + `:2667/2677` `Take(300)` (top-300 by last-updated)
- `Controllers/ProTaskNonTechController.cs:2401` — same pattern
- `Controllers/LeadsController.cs:2485` — `Take(300)` cap

---

## D. Missing TABLES (snapshot predates these features; present on the real DB)
These objects don't exist in the test copies; their screens degrade to an empty grid here (via
AjaxErrorFilter) and will run once the real branch DB (which has them) is connected — **no code change**:
`routemap`, `itemsizeprice`, `ItemSerialNo`, `SMSTemplates`, `ProResignRequests`,
`TenancyDocumentTypes`, `handover`, `AddCustomerLiteviewmodels`, + the real-estate/tenancy module's
data tables (PropertyMains/PropertyUnits/TenancyContracts extra columns — see `/dev/schemadiff`:
27 issues on emirtechlatest / 11 on quicknetlatest-1200, all D-list).

---

## E. DB-side (NOT port code)
- `SP_AVCOMethod` — its own internal `DECLARE TableA_cursor` is conditional (3 IF-branches); an
  uncovered param combo throws "cursor … does not exist". Param-dependent, present in the legacy SP too.
  The all-items path is slow (cursors 6,738 items, ~519s) — the port now honours the legacy 1-hour
  command timeout (`Helpers/SqlQueryDedup.cs`) so it completes.
- `ShowRoomItemForecast/GetStock2` (multi-branch stock consolidation) — needs (a) the branch databases
  `{abudhabi, musafa, aln, dubai, moderate, quickvision}` configured as connection strings + present on the
  server, and (b) the proc `SP_AVCOMethod6` (the `company != 1` path; the copies have `SP_AVCOMethod5`/`7`
  but not `6`). Neither is in the single-DB snapshot, so the grid only renders for a configured branch whose
  proc exists. The **port-break** that previously made it 500 on *every* request regardless of environment —
  EF6 `nameOrConnectionString` resolution lost in `ApplicationDbContext(string)` ("Format of the
  initialization string …") — was fixed in batch 21 (`LegacyWeb.ResolveConnection`; see `docs/REPORTS-AUDIT.md`
  Batch 21). What remains here is purely the missing branch DBs/proc, present on the real server.

---

## QA report resolution (2026-06-12)
- Bugs 1-6: fixed + verified (compression, TempData/blank-field/Supplier-create systemics, JS guards, FileManager redirect).
- **Bug 7 (Quotation Grand Total, SalesType 3 "VAT Voucher Wise"): CLOSED — not a bug.** The
  `GrandTotal = total − tax` behaviour exists identically in the legacy source (`Content/js/qutinvoice.js`);
  owner accepted keeping the original business behaviour (1:1 port).

## Also remove before production
- Dev-only endpoints in `Program.cs` (inside `IsDevelopment()`): `/dev/setpw`, `/dev/schemadiff`.
- Rotate the SMTP credential hard-coded in the excluded `Models/SendMail.cs` (not compiled/shipped, but git-history hygiene).
- Run with `ASPNETCORE_ENVIRONMENT=Production` (real exception pages off, ConnectionStrings required from config).
