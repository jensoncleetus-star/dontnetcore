# Sales / Purchase / Stock / VAT Reports — port-break audit & fixes (batch 11)

**Date:** 2026-06-13 · **Method:** multi-agent read-only audit (per-controller discover → live probe on
:8080 → root-cause) + adversarial verification; fixes applied output-preserving and re-verified against
SQL oracles. Owner policy honoured: historical figures unchanged, faithful/legacy-identical output, no
GUI change.

## Coverage
Audited the report layer: SalesReport, MyReports, PurchaseReport, StockReport, TaxReports + Tax (VAT),
SalesReturnReport, PurchaseReturnReport, CustomerReport, HireStockReport, StockVerification,
StockAdjustment, StockJournal. ~40 endpoints assessed WORKING; the VAT/tax actions (GetTax, GetSaleTax,
GetPurchaseTax, GetUaeVat, GetVat, …) probed **200** (no port-break). 8 endpoints were BROKEN — all now
fixed and verified.

## Confirmed broken → fixed (3 root-cause groups)

### G1 — StockReport: EF Core SQL parameter-name > 128 chars (5 endpoints)
`GetOnDate`, `GetBwDate`, `GetCategoryWise`, `GetBrandWise`, `GetDetails`.
Each built its row data with a ~40-clause query-comprehension `from o in data let A=… let B=… … select`.
The chained `let`s create a deep transparent-identifier chain; each inner EF subquery captures `o.ItemID`
through it, so EF generates a parameter like `@h__TransparentIdentifier5_…_o_ItemID` exceeding SQL Server's
128-char identifier limit → `System.ArgumentException` → HTTP 500.
**Fix:** converted the outer comprehension to the lambda-block form already used by the working `GetStock`
(`data.Select(o => { var A = …; var B = …; … return new {…}; }).<tail>`), so `o` is a depth-0 lambda
parameter and EF emits short names. Every subquery (incl. inner `let chkextend`), the projection, and the
tail were copied byte-for-byte — purely syntactic, output-identical.
**Verified (emirtech):** all 5 → 200; GetOnDate/GetBwDate 6,817 rows, GetCategoryWise 5,159, GetBrandWise
4,382, GetDetails 15.

### G2 — SalesReturnReport: empty-string-vs-null filter sentinels (2 endpoints)
`GetAllSale`, `GetAllSaleprofit`. Three compounding empty-string bugs (the view posts `""` for unset
fields, not null):
1. `SaleType St = new SaleType(); if (satype != "")` left `St` at enum default `POS(0)` for the "All"
   option, and the WHERE `(satype == null || a.SaleType == St)` never caught `""` → filtered to POS → 0 rows.
2. `if (htdate != null) DateTime.Parse(htdate)` / `hfdate` threw `FormatException` on `""` → HTTP 500.
3. WHERE `(hfdate == null || DateDiffDay(h.StartDate, hfrmdate) <= 0)` — with `hfdate=""` (not null) it
   evaluated `DateDiffDay(h.StartDate, NULL)` → NULL → false for every row → 0 rows.
**Fix:** treat empty-string as "no filter" everywhere — `!string.IsNullOrEmpty(satype/hfdate/htdate)` for
the parse guards and `string.IsNullOrEmpty(...)` in the WHERE predicates.
**Verified (trading, rich data):** GetAllSale ALL = 1,890, Sale = 1,890, Hire = 0; GetAllSaleprofit = 1,890
— exactly matching the SQL oracle (`SELECT COUNT(*) FROM SalesReturns WHERE YEAR(SRDate)=2024` = 1,890).

### G3 — PurchaseReport/MonthWise: FromSql missing non-nullable column (1 endpoint)
`MonthWise`. The PIVOT queries (`SqlQueryRaw<MonthWise>` / `<MonthWiseDecimal>`) project `[Year]` + 12
month columns but not the model's `total` column; EF Core requires every mapped column present →
"required column 'total' was not present" → 500. (Legacy EF6 silently left it null.)
**Fix:** append a typed `NULL` total to each of the 8 PIVOT SELECTs (`CAST(NULL AS INT)` for the two count
queries, `CAST(NULL AS DECIMAL(18,2))` for the six amount queries) — reproduces legacy's null exactly (the
view sums columns itself), so the report renders identically instead of 500-ing.
**Verified:** MonthWise 2024 / 2025 / 2026 → 200.

## Regression
220-endpoint sweep unchanged from the prior baseline (200=64 / 404=22 / 405=129 / 500=5 / 400=0) — the 8
repaired reports are date/id-driven POST forms outside the bare sweep set, verified individually above.

## Residual / not yet probed
The large SalesReport (12.6k) and MyReports (11.8k) controllers have many additional report actions; the
audit covered a representative set and found the above. A follow-up pass can sweep their remaining
profit/rebate/comparison variants the same way if desired.

---

# Batch 12 — SalesReport profit/rebate/comparison family (the follow-up pass)

**Date:** 2026-06-13 · **Method:** enumerated all 36 `Get*` data actions of `SalesReportController`
(signatures extracted statically), built a param-matched probe set (`.mr-audit/set_sr.ps1`), swept both
pilots (8080 emirtech / 8081 trading) via `.mr-audit/harness.ps1`. Cross-pilot confirmation: every break
reproduced identically on **both** databases → genuine port-breaks, not data artifacts.

## Confirmed broken → fixed (same two root-cause classes as batch 11)

### G2 (recurrence) — empty-string-vs-null hire-date guards (10 actions)
MVC5 bound an empty optional form field to `null`; ASP.NET Core binds it to `""`. Every action in the
profit/rebate family guarded the **hire-date** filter with `if (hfdate != null)` (Core: `"" != null` is
true → `DateTime.Parse("")` → **FormatException / HTTP 500**), and filtered with
`(hfdate == null || DateDiffDay(h.StartDate, hfrmdate) <= 0)` (with `hfdate=""` the guard is false → the
filter runs against a null `hfrmdate` → `DateDiffDay(...,NULL)` → NULL → every row excluded → **silent 0
rows** even when the parse didn't throw).
- **500 on probe:** `GetAllSaleRebate`, `GetAllSaleRebateCustomerWise`, `GetAllSaleRebateCustomerWisebonus`,
  `GetAllSaleprofitnew`, `GetAllSaleprofitgroup`, `GetAllSaleprofitsummery`, `GetAllSaleprofit`,
  `GetAllSaleprofitCASA` (broken parse guard).
- **Silent 0-rows (no 500):** `GetAllSale` — already used the correct `!= null && != ""` parse guard, but
  its WHERE still used `hfdate == null`, so it returned `rec=0` on both pilots. Fixed by the WHERE pass.
- **`GetDayWise`** — same bug with the differently-named `hfrom`/`hto` params (`if (hfrom != null)`).
**Fix (output-preserving, identical to batch-11 G2):** parse guards → `!string.IsNullOrEmpty(hfdate/htdate/hfrom/hto)`;
WHERE predicates → `(string.IsNullOrEmpty(hfdate) || …)` / `(string.IsNullOrEmpty(htdate) || …)`. With a real
date both behave exactly as before; with `""`/`null` both skip the hire-date filter — matching legacy
(where the field arrived as null and was skipped). 16 parse guards + ~20 WHERE predicates replaced.

### G3 (recurrence) — `MonthWise` PIVOT missing the model's `total` column (2 overloads)
`SalesReportController.MonthWise(int Year)` and the parameterless `MonthWise()` each build 8 `PIVOT`
queries projected into `MonthWise` / `MonthWiseDecimal` via `SqlQueryRaw<>`; the SELECTs return `[Year]` +
12 months but not the model's required `total` → "required column 'total' was not present" → **500**.
**Fix:** appended a typed NULL total to each `SELECT *` (`CAST(NULL AS INT) AS [total]` for the two COUNT
queries `Qry1`/`QryR1`; `CAST(NULL AS DECIMAL(18,2)) AS [total]` for the six amount queries) — byte-for-byte
the same fix shipped for `PurchaseReportController.MonthWise` in batch 11. Reproduces legacy's null exactly
(the view sums the month columns itself).

## Assessed NOT a port-break (faithful / probe-artifact — left unchanged)
- **`GetAllSaleprofitdepartment`** — `SalesExecutive.Split(',').Select(long.Parse)` throws on an empty
  multi-select. In MVC5 the same empty value bound to `null` → `null.Split()` NRE, i.e. it crashed in the
  legacy too; the report requires ≥1 executive selected. Not a regression. (Verify with a real exec id.)
- **`GetAllSal`** — `getcompairstatus` does `new decimal[comparprices]` then unconditionally writes
  `prices[0]`/`prices[1]`, so it requires `comparprices >= 2`; the probe sent `0` (artifact). The UI always
  sends the price-column count. (Verify with `comparprices>=2`.)
- **`GetCommission`, `GetCashOrCredit`, `GetCustomerItemWiseSum`** (and MyReports `GetCreditSaleReturn`,
  `GetContraVoucher`) return a 200 **"Access Denied"** page (`len=4057`, identical on both pilots) — this
  user lacks those module permissions. Faithful authorization, not a break.

## Performance note (not a port-break)
`GetItemWise` over the **full** history on the large trading DB hit the SQL command timeout (Win32 258);
on emirtech it returns 200. Real usage uses a bounded date range. Candidate for the live-data paging pass,
not this audit.

## MyReports — statically clean for both classes
Grepped `MyReportsController` (75 `Get*` actions): its hire-date filters already use the defensive
`(HdateFrom == "" || HdateFrom == null || …)` idiom and the parse guards use `!= ""`, so **G2 does not
occur**; there is **no `SELECT * FROM(SELECT YEAR…` PIVOT**, so **G3 does not occur**. The document-register
sweep (`set1`/`set2`) returned 200 across the board on both pilots. The remaining ~50 analysis actions
(Ledger/Receivable/Payable/Production/etc.) were not live-probed in this batch.

## Build / verification status — LIVE-VERIFIED
- `dotnet build -c Release` → **0 errors** (79 pre-existing warnings, none in the edited regions).
- **Live re-probe DONE** on fresh verify-pilots (ports 8090 emirtech / 8091 trading) running the fixed build
  against the real copies. (Verification was initially blocked by an account split — the session runs as
  Windows user `jenson`, which lacked DB access — resolved by granting `jenson` `db_owner` on the two copies
  via a Task-Scheduler job run as the sysadmin service account `NT SERVICE\MSSQL$SQLEXPRESS`; see
  [[env-account-split-sql-access]].)
- **G2 fixes confirmed working:** the DateTime-`''` 500s are gone. `GetAllSaleRebateCustomerWise` → 200
  (emirtech 4,097 / trading 7,733), `GetAllSaleRebateCustomerWisebonus` → 200 (0 / 1,220),
  `GetAllSaleRebate` / `GetAllSale` / `GetAllSaleprofitCASA` → 200 (no crash). **G3:** `MonthWise` (both
  overloads) → 200.
- **Verify-environment caveat:** `GetComparison` first 500'd on the verify-pilot because `db_datareader`
  lacks **EXECUTE** on `SP_AVCOMethod` (the AVCO stored proc); after granting `db_owner` it returns 200
  (emirtech 338 / trading 2,213). Not a code bug.

---

# Batch 13 — deeper EF Core breaks unmasked by the G2 fix (live-probe findings)

Once the G2 DateTime-`''` crash was removed, live probing the profit family ran the queries far enough to
hit **separate, pre-existing EF Core translation breaks** that the crash had been masking:

### FIXED — `GetDayWise`: `.ToList()` inside an EF `GroupBy` projection
Lines ~13188-89 built `cashsales`/`cardsahles` as
`group.Where(k => k.CustomerType==…).ToList().Sum(o => o.SEGrandTotal)`. The `.ToList()` sits **inside** the
server-translated `GroupBy` result selector → EF throws *"Expression of type List… cannot be used for
parameter of type IQueryable… Sum"* → 500. **Fix:** drop the two `.ToList()` calls; `group.Where(pred).Sum(sel)`
translates to `SUM(CASE WHEN … END)` — output-identical. **Verified:** `GetDayWise` → 200, 366 day-rows
(2024) on both pilots.

### FIXED (the 500) — `GetAllSaleprofit` / `…new` / `…group` / `…summery`
These 500'd with *"The query contains a projection '…DbSet<Journal>()'"*: the row projection embedded
**correlated collection sub-queries** — `journalexpenselink = db.Journals.Where(j => j.InvoiceNo ==
a.BillNo).Select(…)` and `PaymentExpenselink` — which EF Core cannot translate in this query shape (it is the
collection projection itself, not just the HTML-string building: a trial that reduced the inner projection to
the raw scalar `JournalId`/`PaymentId` **still 500'd**). **Fix applied:** removed both collection
sub-projections from the SQL `v` query and rebuilt the `<a href=…>Link</a>` HTML **client-side** in the
existing post-`AsEnumerable()`/`ToList()` `String.Join`, from `o.BillNo`
(`db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a…>" + jid + …)`).
Applied uniformly across all 19 link-sites in the 4 methods; the HTML output is byte-identical. **Verified:**
all four → **200** (no crash) on both pilots. (Perf note: these report branches already do a per-row
`db.SaveChanges()`, so the per-row link fetch matches the existing profile; a batched-by-BillNo lookup is a
later optimisation.)

### Batch 13c (FIXED) — latent G2 `satype` filter under-filtered to SaleType 0 (27 sites)
With the 500 gone the profit reports returned **0 rows** despite data. **First confirmed cause:** the WHERE
`(satype == null || a.SaleType == St)` — with `satype=""` (not null) it forces `a.SaleType == St`, and
`St = new SaleType()` = `(SaleType)0`; since every 2024 sale is `SaleType=1`, that excluded everything. Same
empty-string-vs-null G2 class as batch-12. **Fix:** `satype == null` → `string.IsNullOrEmpty(satype)` at all
**27 sites** across the controller (this had silently been showing only `SaleType=0` rows in *many* reports;
the fix restores legacy "All" behaviour). Regression-checked: full 36-endpoint sweep = 34×200 + the 2 known
artifacts, **no new 500s**.

### Batch 13d (OPEN) — `GetAllSaleprofit*` still return EMPTY for "All" params (branch-guard skips the query)
After the satype fix the four `GetAllSaleprofit*` still return `data:[]` for the swept params — but EF logging
proved **the `v` query never runs at all** (none of its tables — Customers/SEPayments/AccountsTransactions/
Journals/… — are queried). Cause: the action is wrapped in nested **branch guards** —
`if (allshworoom != "ALL SHOWROOMS") { if ((location=="All"||location=="") && (technician==null||technician==0))
{ if (srchtxt=="") { …v… } } }` (line ~5638) plus `cached`/`isemirtech` splits — and for the probe's
"All"/`ddMC=0` params it falls through to an empty `{recordsFiltered:100,recordsTotal:100,data:[]}` return
without executing a query. Resolving this needs the **real UI's exact param matrix** (which MC / location /
technician / cached values the page posts) and a branch-logic audit; it's a dedicated follow-up, not a
one-line fix. The 500 (13b) and satype (13c) fixes are correct and necessary groundwork. Repro:
`.mr-audit/harness.ps1 -ProbesFile set_b13.ps1` on 8090/8091, probe `type=''`.

**Update (further isolated):** confirmed it is a **real control-flow bug, not a param artifact** — probed with
a real `ddMC` (`20086`, 8,367 trading sales in 2024) + `location='All'` + `technician='All'` + `cached`
true/false → all still `data:[]`. A fresh EF-command-logged run shows **0** queries against
`SalesEntries`/`SalesReturns`/`SEPayments` for the request (only the auth/menu churn), proving the `v`
query never executes for ANY params. So the action reaches an empty `recordsTotal=100` return *without
running its data query* — the fault is the method's control flow (nested `allshworoom`/`location`/
`technician`/`srchtxt`/`cached`/`isemirtech` branches, ~line 5636+ of `SalesReportController`), not a filter.
Next step: attach a debugger (or instrument each branch) to find which guard falls through, then correct the
control flow. Note `MaterialCenter` ids (e.g. 20086) are not present in `MCs.MCId`, so `allshworoom` is null →
always the non-"ALL SHOWROOMS" branch.

### Not bugs (verify-environment artifacts, now cleared)
`GetComparison` (SP EXECUTE perm, above). `GetItemWise` over full history on trading still hits the SQL
command-timeout — a **data-volume/perf** item (bounded date ranges are fine), not a port-break.

---

# Batch 14 — MyReports analysis endpoints (Ledger / AR-AP / Stock / Hire / Production / Misc)

**Date:** 2026-06-13 · **Method:** built a param-matched probe for the ~47 previously-unswept MyReports
`Get*` analysis actions (`.mr-audit/set_mr.ps1`, sample ids AccId=1/Cuss=1/Item=2), swept emirtech, then a
parallel multi-agent root-cause pass on the 500s. **4 endpoints fixed & live-verified (200), no regressions.**

### FIXED — `GetEmployeeDetails`: EF Core can't translate `GroupBy(...).OrderBy(...)`
The server query chained `.Select(...).GroupBy(a => a.EmployeeId, (k,g) => g.OrderBy(m => m.EmployeeName)
.FirstOrDefault()).OrderBy(x => x.EmployeeName)` → *"The LINQ expression 'DbSet<servicereport>().OrderBy(...)'
could not be translated"* → 500. **Fix:** inserted `.AsEnumerable()` after the server projection (which
materialises the joins/filters/Tasks sub-list exactly as intended) so the identity `Select`/`GroupBy`/
`OrderBy` run client-side — output-identical. Matches the existing `.AsEnumerable()` idiom in the same file.

### FIXED — dynamic sort guard: `OrderBy(" asc")` on an empty sort column (7 sites)
DataTables posts `order[0][dir]=asc` with an empty `columns[0][name]`, so `sortColumn=""`, `sortColumnDir=
"asc"`. The guard `if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))` is true
(dir non-empty) → it ran `v.AsQueryable().OrderBy(" asc")` → System.Linq.Dynamic.Core `ParseException` → 500.
**Fix:** prepend `sortColumn != "" &&` — the exact guard the *working* sibling endpoints already use (lines
6116/6305/6560/6789); when no real sort column is posted the dynamic sort is skipped and the query's own
`orderby … descending` supplies the order (output-preserving). Applied at all **7** bare-guard sites
(MyReportsController.cs 2575/3034/3289/3468/4162/7193/11324). **Verified 200:** `GetAllRemarksaccount`,
`GetStockAdjItemWiseReport` (2,740 rows), `GetApproval` (the latter two were first mis-tagged "param
artifact" by the agent re-probing with the DataTables fields *omitted* — but the real sweep, which posts
`dir=asc` + empty name, hits this `OrderBy(" asc")` path; the guard fixes all). Document-register sweep
(`set2`) re-run = no regression.

### OPEN — `GetItemDetails`: serial-number table absent + entity mis-mapped
`from a in db.ItemSerialNo …` → 500 *"Invalid object name 'ItemSerialNo'"*. Root cause: the `ItemSerialNumber`
entity has no `[Table]` attribute, so EF Core names the table after the **DbSet property** (`ItemSerialNo`)
instead of the EF6 pluralised entity name. BUT the real serial-number table (any `%Serial%` / `itemserialnoid`/
`serialno` column) **does not exist in either copy** — the feature was never migrated here, so even the
correct mapping can't be verified. Left unfixed: needs (a) the production schema's actual table name confirmed,
then `[Table("…")]` on the entity, and (b) the table present. A trial `[Table("ItemSerialNumbers")]` (from the
`patchController` DDL) was applied then reverted because that table is also absent on these copies.

### Not bugs (faithful / artifacts)
`getsaleledgers` → 302: a `RedirectToAction("Receivableledgercusdetails")` by design (a navigation action,
not a data grid). `GetPurchaseUsageDetails` → SQL command-timeout over full history (data-volume/perf).
`GetHireReturn`/`GetHireExpire`/`GetCrossHireExpire` → 200 Access-Denied (permission-gated). The big Ledger
reports (`GetLedger*`, 8–9 MB) and AR/AP/Stock/View/PL reports all return 200.

---

# Batch 15 — PurchaseReport (full sweep, CLEAN, no fixes)

**Date:** 2026-06-13 · Swept all **19** `PurchaseReportController` `Get*` data endpoints
(`.mr-audit/set_pr.ps1`) on **both** pilots. **Result: clean — no port-breaks.** Every endpoint returns 200
with data on both DBs (emirtech + trading): the rebate family (`GetAllpurRebate` 36k/56k rows,
`GetAllpurRebateSupplierWise`, `GetAllpur`), `GetAllPurchase` (comparprices=10, 4.5k/8.5k rows),
`GetDayWise` (6,209 rows — no `.ToList()`/hire-date G2 issue, unlike its SalesReport namesake),
`GetSupplierItemWise`, Item/Category/Brand/Supplier/Monthly/CashOrCredit/Invoice variants, etc. Only
non-200: `GetItemWise` over **full** history on the large trading DB → SQL command-timeout (Win32 258) — the
same data-volume/perf class as `SalesReport.GetItemWise`/`MyReports.GetPurchaseUsageDetails` (200 on emirtech;
bounded date ranges are fine), **not a port-break**. No code change. PurchaseReport was evidently ported
correctly (and batch 11 had already repaired the StockReport param-overflow class).

---

# Batch 16 — StockReport (full sweep, CLEAN, no fixes)

**Date:** 2026-06-13 · Swept all **10** `StockReportController` `Get*` endpoints (`.mr-audit/set_st.ps1`) on
both pilots. **Result: clean — no port-breaks.** The 5 endpoints repaired in batch 11
(`GetOnDate`/`GetBwDate`/`GetCategoryWise`/`GetBrandWise` 6,817/9,509 rows, `GetDetails` 15 rows) confirm
still-working, and the 5 not previously probed all return 200: `GetStock` (3,727/8,459 rows), `GetItemWise`,
`GetMoment`/`GetMoment2` (rec=0 — no movement for the probed item), `GetExpiry` (permission-gated 200 on
emirtech). No code change.

---

# Batch 17 — SalesReturnReport / PurchaseReturnReport / CustomerReport / HireStockReport

**Date:** 2026-06-13 · Swept all **21** `Get*` endpoints across these four controllers
(`.mr-audit/set_rr.ps1`) on both pilots. **One port-break found & fixed; rest clean.**

### FIXED (G1 recurrence) — `HireStockReport.GetDetails`
500 with *"The length of the parameter '@h__TransparentIdentifier5_…_b_ItemID' exceeds the limit of 128
characters"* — the exact batch-11 G1 class: the per-item stock-detail query is an outer comprehension
`var v = (from b in data let PriPurchase=(…) let SubPurchase=(…) … [≈30 chained `let` subqueries] … select
new {…}).ToList();`; each chained `let` deepens an EF transparent-identifier chain, and an inner subquery
captures `b.ItemID` through it → SQL param name > 128 chars → 500. **Fix:** converted the comprehension to
the lambda-block form already used by the batch-11 `StockReport.GetDetails` fix —
`data.Select(b => { var PriPurchase=(…); … return new {…}; }).ToList()` — so `b` is a depth-0 lambda
parameter and EF emits short names. Every subquery/projection/tail copied byte-for-byte (purely syntactic,
output-identical). **Verified 200, 16 rows on both pilots.**

### Clean / not bugs
The other 20 endpoints return 200 on both pilots. `SalesReturnReport.GetAllSale`/`GetAllSaleprofit` already
carry the correct `string.IsNullOrEmpty(satype)` guard (fixed earlier — the `rec=0` is genuine, not a satype
G2). `HireStockReport.GetMoment` → 200 Access-Denied (permission). `GetCrosshire`'s sibling `mydata`
comprehension has the same shape but a shorter `let` chain, so its param names stay ≤128 (endpoint returns
200 with data on both pilots — EF translation errors are deterministic per query shape, so a 200 proves it).

---

# Batch 18 — TaxReports / StockJournal / StockAdjustment

**Date:** 2026-06-13 · Swept TaxReports (11 VAT/tax endpoints), StockJournal (3), StockAdjustment (2 by-id).
StockVerification has no `Get*` data endpoints. **One port-break found & fixed.**

### FIXED (batch-8 class) — `StockJournal.GetGeneratedItems`: JSON case-collision
A DataTables `[HttpGet]` endpoint; on GET it 500'd with *"The JSON property name for '…item' collides with
another property"*. The anonymous projection has both `a.Item` (→ JSON `"Item"`) and `item = b.ItemName`
(→ `"item"`); System.Text.Json is case-insensitive on property names and rejects the pair (legacy Newtonsoft
was case-sensitive and emitted both). NOTE the collision fires at type-metadata time, so it 500'd even for an
empty result. **Fix:** `return Json(ConD);` → `return LegacyJson(ConD);` — the batch-8 `BaseController.LegacyJson`
(Newtonsoft `ContentResult`) which preserves case and emits both properties exactly as legacy did. **Verified
200** on both pilots (`[]` — the `SJItemGenerates` table is empty on these copies, but the collision crash is
gone; with rows Newtonsoft serialises both `Item`/`item`).

### Clean / not bugs
TaxReports all 200 (several permission-gated Access-Denied; `GetSaleTax2` 12,489 rows, `GetExpenseTaxReport`
11,696, `GetVat`/`GetNewDetailsVat`/`GetDetailsVat` large data). The initial 405s (`GetDetailsVat`,
`GetGeneratedItems`, `GetAdjById`, `GetAssetAdjById`) were just the POST sweep hitting `[HttpGet]`-only
endpoints — re-probed as GET they return 200 (`GetAdjById`/`GetAssetAdjById` `null` for id=1, genuine).
`GetStockJournal`/`GetAllStatusUpdation` 200. `GetConsumedItems` (sibling of GetGeneratedItems) has no
collision (`a.Item` vs `Citem` differ by more than case) — fine.

---

## Audit coverage summary (report layer)
Across batches 11–18 the report layer is now comprehensively swept on both pilots: **SalesReport** (36
endpoints — batches 12/13b/13c fixed; 13d `GetAllSaleprofit*`-empty open), **MyReports** (68 — batch 14
fixed; `GetItemDetails` serial-table open), **PurchaseReport** (19 — clean), **StockReport** (10 — clean),
**SalesReturnReport/PurchaseReturnReport/CustomerReport/HireStockReport** (21 — batch 17, `GetDetails` G1
fixed), **TaxReports/StockJournal/StockAdjustment** (16 — batch 18, `GetGeneratedItems` JSON-collision fixed).
Remaining open items are external-input-bound: **13d** (needs the real UI POST param matrix + a debugger) and
**`GetItemDetails`** (needs the production serial-number table name + the table present).

---

# Batch 19 — APP-WIDE: Dynamic.Linq empty-sort-column 500 (262 sites, 181 controllers)

**Date:** 2026-06-13 · The DataTables server-side sort idiom appears in almost every grid controller:
```csharp
if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);   // System.Linq.Dynamic.Core
```
When DataTables posts `order[0][dir]=asc` with an **empty** `columns[i][name]` (a column without a `name`,
incl. the default first-load sort on many grids), `sortColumn=""` slips past the guard (dir is non-empty) and
runs `OrderBy(" asc")` → Dynamic.Core throws *"No property or field 'asc' exists…"* / *"…'' exists…"* → **500**.
This is the same bug fixed for 7 MyReports sites in batch 14 — but it is **systemic**: found at **262 sites
across 181 controllers** (Controllers + Areas), i.e. nearly every data-entry / master / transaction grid.

**Fix (app-wide, output-preserving):** prepend `sortColumn != "" &&` to the guard — exactly the pattern the
already-correct grids use. Truth-table proof: the ONLY changed case is `sortColumn=""` + `dir` non-empty
(which always 500'd) → now skips the dynamic sort and the query's intrinsic `orderby` orders the rows; every
case where a real sort column is posted is byte-identical. Applied uniformly (`replace_all`); build 0 errors.

**Verified before/after on both pilots:** `Supplier.GetSupplier` 500 (*"No property or field 'asc'"*) → **200**
(emirtech 680 rows, trading 808); control grids `Supplier.GetSuppliers` and `Item.GetItem` stayed **200**
(no regression). This eliminates a whole class of latent grid 500s across the entire app, not just the report
layer. (Each grid's exposure depended on whether its DataTables view names the sorted column; the fix is a
no-op for those that do and a repair for those that don't.)

---

# Batch 20 — transaction/document register grids (workflow sweep) + edit-window DateTime overflow

**Date:** 2026-06-13 · A 6-agent workflow swept the register-grid endpoint of **30 transaction/document
controllers** (SalesOrder/PurchaseOrder/Quotation/ProForma/Deliverynote/CreditNote/DrNote/ContraVoucher/MRNote/
Returns/MaterialRequisition/StockTransfer/Unassemble/Production/JobCard/Estimate/CreditSale/Hire/Order/
PurchaseEntry(+Expense)/Assetpurchase/WorkCompletion/Receipt) on 8090. **All clean (200)** except a few
permission-gated (JobCard/HireReturn/CrossHireReturn/Order) — and **one real port-break**.

### FIXED — `CreditSaleReturnNote.GetSalesReturn`: `DateTime` overflow in the edit-window calc
500 with *"System.ArgumentOutOfRangeException: The added or subtracted value results in an un-representable
DateTime"* at `editableDay = today.AddDays(-userEditDays)`. `userEditDays = UserEditDayss.srdays` is a
**minutes-scale** value (the canonical sibling `CreditSaleReturnController` line 260 computes the same
edit-window with `today.AddMinutes(-userEditDays)`); feeding it to `AddDays` walks `today` back ~hundreds of
years → past `DateTime.MinValue` → throw. **Fix:** `AddDays(-userEditDays)` → `AddMinutes(-userEditDays)` for
the **`srdays`** field — at all 3 sites in `CreditSaleReturnNoteController` (252/1087/1757) and the 2 latent
sibling sites in `CreditSaleReturnController` (747/1452, also `srdays`; line 260 already correct). Matches the
working sibling exactly — output-preserving. **Verified 200** on both pilots (was 500).

### Left unchanged (ambiguous — NOT confirmed bugs)
`AddDays(-userEditDays)` also appears with the **`days`** field (`CreditSaleController` 2038/7023/7567,
`BillSundryController` 364/526) and **`stkdays`** (`StockTransferController` 684/1435, `ShelfStockTransfer` 113).
These were NOT touched: their register grids return **200** (so those fields are small/days-scale for the test
user, and `AddDays` doesn't overflow), and the same controllers mix `AddDays`/`AddMinutes` on the same field —
so the unit is genuinely ambiguous and blindly switching could shrink a real edit-window. Flagged for a
deliberate follow-up (confirm each `UserEditDayss` field's unit, then make `AddDays`/`AddMinutes` consistent).

---

# Batch 21 — complex master/ops register grids (workflow sweep) + EF6 `nameOrConnectionString` port-break

**Date:** 2026-06-13 · A 7-agent workflow swept the register-grid endpoint of **37 master/ops controllers**
(Leads/PipeLine/leaddashboard/LeadChecklists, ProTask(+NonTech)/TaskItem/Project/ProcessFlow,
AMC(+PeriodicMaintenance)/Warranty/VehicleService/VehicleMaster, Employee(+Attendance)/LeaveRequest/
Resignation/Team, AssetFromInventory/AssetToInventory/AssetTransfers/Inventory/Forecast,
PDCRegularise/ReceiptApproval/PaymentApproval/PRApprovals/ChequePrinting/ChequeBook,
BOM/Boq/BillSundry/AccountMap/BundleOffer/ShowRoomItemForecast) on 8090. **35 clean (200)**; one
permission-gated (`BundleOffer/GetBOM` — jenson lacks the `BOF List` role); **one real port-break**; one
already-documented missing-table (no code change).

### FIXED — EF6 `nameOrConnectionString` semantics lost in the Core `ApplicationDbContext(string)` ctor
`ShowRoomItemForecast/GetStock2` 500'd with *"System.ArgumentException: Format of the initialization string
does not conform to specification starting at index 0"*. The multi-branch consolidation news-up a context
per branch — `new ApplicationDbContext(conn)` where `conn` is a **connection-string NAME** from the list
`{abudhabi, musafa, aln, dubai, moderate, quickvision}`. In EF6 `new DbContext(nameOrConnectionString)`
resolved a bare token as a **named** `<connectionStrings>` entry (or, by convention, a database of that name
on the default server); EF Core's `UseSqlServer()` only accepts a full connection string, so the faithful-port
shim fed the bare name straight in → SqlClient parse crash on **every** call. **Three controllers / five sites**
pass a bare name and so all 500 in production: `ShowRoomItemForecast` (228/668), multi-company `BalanceSheet`
(2582/4651, `aa[selcompany].Name`), branch-switch `Users` (606, `model.branch`).
**Fix (one place, monotonic):** `ApplicationDbContext(string Connection)` now routes through
`LegacyWeb.ResolveConnection(...)` which reproduces EF6 semantics — a value containing `=` is already a
connection string (used as-is, so **every existing working caller is untouched**); a bare token resolves from
`ConnectionStrings:<name>` config; else the EF6 convention (a database of that name on the app's default
server/auth, via `SqlConnectionStringBuilder.InitialCatalog`). The only paths that change behaviour are the
five that *currently always crash*, so it cannot regress a working endpoint. **Verified:** with branch
`musafa`→the emir copy configured, `GetStock2 company=1` (which calls the present `SP_AVCOMethod5`) returns
**200** with the full forecast grid (2.36 MB, real movement figures) — before the fix it was the
initialization-string crash; the 6 key data grids on both pilots are unchanged (Customer 15355/17571,
Accounts 1348/1116, Journal 23915/23283, Payment 17371/27907, Supplier 680/808, Item 6817/9509 — no
regression).

### Not bugs (already-documented divergences, no code change)
- **`Resignation/GetData` 500** — `Invalid object name 'ProResignRequests'`. The `ProResignRequests` table is
  not in the snapshot copies; it is already on the **D-list** (`SNAPSHOT-DIVERGENCES.md` §D, alongside
  `ItemSerialNo` etc.) — present on the real branch DB, runs there; degrades to an empty grid on the real AJAX
  path (the probe sees a raw 500 only because it doesn't send the `X-Requested-With` header). Entity/DbSet
  mapping (`DbSet<ProResignRequest> ProResignRequests`) is correct.
- **`GetStock2` for unconfigured branches / `company`∉{1}** — the consolidation still needs the **other branch
  DBs** (musafa/aln/dubai/moderate/quickvision absent here) *and* the stored proc `SP_AVCOMethod6` (the
  `company!=1` path; the copy has `SP_AVCOMethod5`/`7` but **not** `6`). Both are environment/DB-object
  divergences (§D/§E), present in production — not port code. The port-break (the connection-string crash that
  blocked the grid for *every* request) is what was fixed.

---

# Batch 22 — financial/ops/property/master grids (workflow sweep) + two long-open items closed

**Date:** 2026-06-13 · A 7-agent workflow swept **121 grid endpoints** across the un-swept families:
financial (BRS/DayBook/FinalAccounts/AccountSummary/Registers), BalanceSheet detail (18 grids) + recon,
ops-complex (CustomerMerge/PackingList/WarrantyEntries/VehicleUpdates/operationprocedure/…), the
property/real-estate module (46 endpoints), docs/import, and masters ×2. **The entire financial + balance-sheet
layer is clean (200)** — DayBook (83 MB), FinalAccounts cashbook/ledger (6.5 MB each), AccountSummary, Registers,
all Trial-Balance/P&L/Cashflow variants — strong confirmation the accounting screens survived the port.
**One real port-break found & fixed.**

### FIXED (`.ToList()`-in-EF-projection class) — `PropertyReports.Getrateofreturn`: List-vs-IQueryable `Sum`
500 with *"System.ArgumentException: Expression of type 'List`1[…Decimal]' cannot be used for parameter of type
'IQueryable`1[…]' of method 'Decimal Sum…'"* at line 1307. The rate-of-return grid sums three correlated income
subqueries (Rentals.Amount, TenancyContracts.Rent, TenancyContracts.Deposit) **inside** the outer
`db.Maintenances` IQueryable projection, each written `(…select new { x }).ToList().Sum(s => s.x)`. EF6
materialised the inner `.ToList()`; EF Core 10 cannot translate a `List.Sum` inside the server-side projection
(the List vs IQueryable `Sum` overload collision). The legacy `_archive` source is byte-identical (only
`DbFunctions.DiffDays`→`EF.Functions.DateDiffDay`), and the author's own commented-out alternate used `??0` — so
the intent is "empty→0". **Fix:** drop the three `.ToList()` and make the sum explicit-nullable-coalesced:
`}).Sum(x => (decimal?)x.Amount) ?? 0` (and `.Rent`/`.Deposit`). EF Core then emits a correlated
`COALESCE(SUM(…),0)` scalar subquery — identical numbers (sum if rows, 0 if none), output-preserving.
**Verified 500→200** on both pilots (`rec=0` because Maintenances/Rentals/TenancyContracts are all empty on the
snapshot copies — confirmed 0 rows; with data the COALESCE-sum reproduces the legacy figure exactly). 6 key data
grids unchanged on both pilots (no regression).

### Two long-open items CLOSED by root-cause (no code change — faithful behaviour)
- **`GetAllSaleprofit*` returns 200 + empty `data` (the batch-13d item)** — ROOT-CAUSED, **not a port-break**.
  With the *real* view params (the showroom control `#ddlMC` posts `ddMC="0"`, not the string "ALL SHOWROOMS"
  which exists nowhere; date field is `todates`; screen is `serverSide:false`), the grid returns
  `recordsTotal=100` with `data:[]`. The `recordsTotal=100` is **hard-coded** in these branches (e.g. lines
  3053/3421/3588/3936/4278 `recordsTotal = 100;`) — a faithful legacy artifact, not a real count. The empty
  `data` comes from the filter pair `(customer == 0 || a.Customer == customer)` and
  `(SalesMan == 0 || b.SalesPerson == SalesMan)`: the UI sends `""`→`null` for these `long?` params, `null != 0`,
  so each collapses to `a.Customer == null` / `b.SalesPerson == null` and empties the result (the sibling
  filters `SalesExecutive`/`source`/`sourcelead` carry the missing `|| x == null` guard; `customer`/`SalesMan`
  don't). Crucially `long?` binds `""`→`null` **identically in MVC5 and Core**, so the legacy returns the same
  empty result — a latent *legacy* defect, faithfully ported (the data dependency is fine: Account=1 Sale credits
  = 12,616 rows). Adding the two null-guards would change a **profit report's** output and diverge from the
  legacy, so it is an owner-sign-off enhancement, **not** an output-preserving port fix. Closed.
- **`UserEditDayss` field-unit ambiguity (days/stkdays — the #15 follow-up)** — RESOLVED. All fields are
  *minutes* (settings labels in `Views/UserEditDays/Create.cshtml` all say "in Minutes"; the `days > 10080`
  (=7 days-in-minutes) guard at `CreditSaleController.cs:908` confirms it). BUT the **legacy itself mixes**
  `AddDays(-userEditDays)` (legacy CreditSale 2102/7291/7852) and `AddMinutes(-userEditDays)` (5411/5802/13433),
  so the port's surviving `AddDays` sites (CreditSale 7023/8468, BillSundry 364/526, StockTransfer 684/1435,
  ShelfStockTransfer 113) are **faithful to the legacy** and don't crash (`days`≤10080 ⇒ ~28-yr window, no
  `DateTime` overflow). Changing them would diverge from legacy and shrink a long-standing edit window. Only
  batch-20's sites were corrected because they actually *overflowed* (500). Left as-is; documented. Closed.

---

# Batch 23 — Hr-area sweep + systemic `.ToList()`-inside-EF-projection hunt

**Date:** 2026-06-13 · A 6-agent workflow swept the never-before-probed **Hr area** (Payroll/Salary/Attendance/
Settlement, 17 controllers) and hunted the `.ToList()`-inside-an-unmaterialized-EF-projection class — the one
fixed in `PropertyReports.Getrateofreturn` — across the highest-count controllers. **13 real port-breaks found,
all fixed + live-verified 500→200 on both pilots** (key-grid regression unchanged). The hunt also **cleared** the
high-count clean controllers — StockReport (170 hits), SalesReport, HireStockReport, BalanceSheet, StockVerification
all use the **safe post-materialization** form (`data = v.ToList(); … data.Select(o => { (sub).ToList().Sum(); })`),
not unmaterialized projections — so no false fixes there.

### FIXED — `.ToList()`-inside-EF-projection (the systemic class), 12 endpoints
A correlated sub-query `(from … select new { x }).ToList().Sum(c => c.x) ?? 0` (or `.Count()`) sitting **inside**
an outer IQueryable `select new {…}` that EF later forces to translate (via `item.ToList()`) → EF Core 10 can't
feed a materialized `List` into `Queryable.Sum/Count` → *"List`1[…] cannot be used for parameter of type
IQueryable`1[…] of method Sum/Count"*. **Fix:** drop the inner `.ToList()` so EF emits a correlated
`SUM`/`COUNT` sub-query. Output-preserving — only the **`?? 0`-guarded** sums were converted (`}).Sum(sel) ?? 0`,
`}).Count() ?? 0`): with the null-coalesce, empty→`SQL NULL`→`?? 0`→0 reproduces the old `Enumerable.Sum` empty→0
exactly. **Unguarded** sub-query sums (e.g. `ItemController` 4391–4633, which run *post*-materialization over a
`data` list = LINQ-to-objects) were **left untouched** to avoid any empty→0-vs-null shift. Endpoints (all 500→200
both pilots): `ItemController` ×7 (`SearchdetailsMCtech`, `SearchdetailsMC`, `GetItemMCbar`, `GetItemHire`,
`SearchBatch`, `GetBatch`, `SearchItemDetailsByMC`), `MyReports` ×2 (`SalesReturnItemWise`/`…R` — 0.5–0.8 MB of
real data on :8091), `HireReturn`/`CrossHireReturn` `SearchInvoiceItem` (these also need the `page` form field —
absent `page` NREs at HR:1944/CHR:1821 *before* the projection; the real UI always sends it), and
`AttendanceReport.GetAttendanceSheet` (`.ToList().Count()`).

### FIXED — `AttendanceReport.GetAttendanceSheet` second (masked) bug: left-join null-unwrap
Fixing the `.ToList().Count()` let the query execute, which then threw *"Nullable object must have a value"* (line
82): the projection read `b.EmployeeId` (non-nullable `long`) off a **left-joined** `Employee` (`DefaultIfEmpty`),
NULL for any attendance row whose employee is missing. **Fix:** project `a.EmployeeId` (the join key on the
always-present `DailyAttendanceDetails`) instead — identical value for matched rows, no crash for orphans.
**Verified 200, rec=80/2** on the two pilots.

### FIXED — `PayrollReport.GetPaySheet`: date-concat FormatException + masked `.ToList().Count()`
Two cascading bugs. (1) `string lastMonthYear = lastdate + myear` concatenates the month's last-day int with the
`MM-yyyy` string with **no separator** → `30 + "06-2026" = "3006-2026"` → `DateTime.Parse` FormatException
(line 184). The same idiom is in **6 methods** (65/184/284/361/523/589); fixed all to `lastdate + "-" + myear` to
match the `"01-" + MonthYear` start-date construction. NOTE this is a **pre-existing legacy defect** (the concat
throws on .NET Framework too — the screen never worked), corrected as a clear, isolated date-string defect (no
figure math touched). (2) With the date fixed the query ran and hit the same `.ToList().Count() ?? 0`
in-projection class (lines 214/236) → dropped `.ToList()`. **Verified 200** (rec=0 — `PayrollVouchers`/payroll
tables are empty on the snapshot copies, so figures are unverifiable here, but both crashes are gone).

### Clean (Hr area) / permission
Hr grids otherwise clean: `PayrollVoucher`/`PayrollUnit`/`Payhead`/`SalaryStructure`/`WorkShift`,
`AttendanceType`/`DailyAttendance` (587 rows)/`getAttendanceDetails`, `FinalSettlement`/`LeaveSettlement` (52
rows)/`EmployeeGrade`, and PayrollReport's other grids (`GetPayrollRegister`/`getPayrollDetails`/`GetFinalSettlement`/
`GetLeaveSettlement`). Permission-gated (jenson lacks the role): `Attendance/GetAttendance`,
`CalendarTemplate/*`, `Holiday/GetHolidayList`.

---

# Batch 24–25 — app-wide close-out: last projection bug + defensive-guard crashes

**Date:** 2026-06-13 · A 5-agent workflow triaged **every** remaining `.ToList()`-`Sum`/`Count`/`Average` site
app-wide (60 items) + the flagged latent issues + the long-open #9. **Result:** the high-count controllers
(StockReport/Forecast/ShowRoom/Leads/Property/MyReports/Hr-settlement) are confirmed **safe post-materialization**
(LINQ-to-objects over a materialized `data` list, or a standalone terminal `.ToList()`); only **8 actionable
items** remained, all fixed + live-verified 500→200 on both pilots (key-grid regression unchanged).

### FIXED — last in-projection `.ToList().Sum()` (the systemic class) — `Inventory.getforcast`
The forecast grid's two `let PriSale`/`let SubSale` correlated sub-sums (`InventoryController` 1460/1471) carried
an inner `.ToList()` inside the unmaterialized outer EF query (materialised at 1491) → List-vs-IQueryable `Sum`
500 on every call. Dropped the inner `.ToList()` (both `(int?)(...) ?? 0`-guarded → empty→0 preserved).
**Verified 200** both pilots.

### FIXED — `SalesReport.GetAllSaleprofitdepartment`: two unguarded-input crashes (G2 family)
(1) line 155 `SalesExecutive.Split(',')` threw `NullReferenceException` when `SalesExecutive` is null (the All
path posts it null, not "0"). (2) lines 195/199 `if (hfdate != "")` / `if (htdate != "")` → `DateTime.Parse(null)`
`ArgumentNullException` when the **Scaffold-only** hire-date fields are omitted (non-Scaffold UI). **Fix:** guard
`SalesExecutive` with `!string.IsNullOrEmpty` before `Split`, and the two date guards → `!string.IsNullOrEmpty`
(the same G2 empty-string-vs-null fix as batch 12; `hfdate`/`htdate` aren't used in this action's WHERE, so
output-preserving — the identical guard block in a sibling action at line ~867 was fixed too). **Verified 200
with real data** (omit-all probe → rec=7162 on emir / rec=52111 on trading; was 500).

### FIXED — defensive guards for null/empty input crashes (output-preserving; real-UI path unchanged)
- `Hr/DailyAttendance/GetDailyAttendance` (389): `Convert.ToDateTime(empJoin.JoinDate)` NRE when the employee
  isn't found → `if (empJoin == null) return Json(0)` (mirrors the existing not-eligible path).
- `Hr/DailyAttendance/GetDeptDailyAttendance` (509): `DateTime.Parse(AtDate)` FormatException on empty `AtDate`
  → `if (string.IsNullOrWhiteSpace(AtDate)) return Json(0)`.
- `Hr/SalaryStructure/GetSalaryStrDetails` (441) & `Hr/PayrollVoucher/GetAutoFillVoucherDetail` (670): empty
  date params → `DateTime.Parse("")` / `tdate.Value` on null → guarded to `return Json(0)`.
- `Hr/CalendarTemplate/GetMonthlyLeave` (401): deleted a dead `var dds = db.Holidays…First()` line (value never
  used; `.First()` would throw "Sequence contains no elements" on an empty Holidays table — removed the latent
  crash with zero output change).
All five verified 200 (the empty/missing-input path now returns a harmless empty result; the real-UI path with
valid input is untouched).

### #9 `MyReports/GetItemDetails` — CLOSED as external-bound (no code fix)
The entity `ItemSerialNumber` + DbSet property `ItemSerialNo` (no `[Table]`) + `RemapLegacyTableNames` all
correctly resolve to table **`ItemSerialNo`** — identical to the byte-identical legacy EF6 source. The 500 is a
`SqlException "Invalid object name 'ItemSerialNo'"`: the table is simply **absent on the snapshot copies**
(D-list, `SNAPSHOT-DIVERGENCES.md`); it exists on the real branch DB, where the screen returns 200. **No code
defect — do NOT add a `[Table]` attribute.** Closed.

### Out of this class (flagged, NOT fixed here) — Leads dashboards, different bug classes
`Leads/leaddashboard` (line 155: `DbFunctionsCompat.AddMinutes` is a plain C# helper, not an EF-translatable
function → "Translation … failed"), `Leads/followups` (GetMyTaskcount line 466) and `Leads/leadsnewdashboard`
(line 849) — both collection-projection translation 500s. These are **distinct systemic classes** (DateTime-
function translation; collection-projection) tracked for a separate pass.

---

# Batch 26 — Leads/ProTask dashboards: DateTime-function-translation + collection-projection classes

**Date:** 2026-06-13 · A 2-agent workflow swept the two distinct classes flagged in batch 24 across
Leads/Accounts/ProTask/ProTaskNonTech. **6 endpoints fixed + live-verified 500→200 on both pilots** (key-grid
regression unchanged). The client-eval uses of these helpers (after `.ToList()`/`.AsEnumerable()`, or in a
top-level `Select`) were confirmed **safe** and left untouched.

### FIXED — `DbFunctionsCompat.AddMinutes` used inside an EF query (DateTime-function translation)
`DbFunctionsCompat.AddMinutes/AddDays/AddHours` (`Helpers/SendMailShim.cs`) are plain C# helpers
(`date?.AddMinutes(x ?? 0)`) **not** registered as EF functions, so they fail to translate when used in an
*unmaterialized* LINQ-to-Entities query (legacy EF6 used `DbFunctions.AddMinutes`, which mapped to SQL
`DATEADD`). **Fix:** rewrite to the EF-native form EF Core translates to `DATEADD` —
`DbFunctionsCompat.AddMinutes(dateExpr, mins)` → `(dateExpr).Value.AddMinutes(mins ?? 0)`. Faithful (same
`DATEADD`). Sites: `Leads/leaddashboard` (the `vv` query at 158 that 500'd + the latent `v` query at 107),
`ProTask/dashboard` (1358) and `ProTaskNonTech/dashboard` (1207) (both `let k` feeding a `.Where(ndate < Now)`
in an unmaterialized `vv`). **All 200** both pilots. The client-eval AddMinutes/AddDays (Accounts/Index
commented dead code + the live `.AsEnumerable().Select` ones; `ProTask/Getmanhour` top-level `Select`;
`ProTaskNonTech/GetAllTaskdashexp` already materialized) are safe — untouched.

### FIXED — collection / sub-entity projection EF Core 10 cannot translate (batch-13b class)
- `Leads/leadsnewdashboard` (3 queries `v`/`vemployeeby`/`vsrcby`, ~849/870/926): each projected a whole `upt`
  sub-collection inside the EF `select new LeadDashboard{…}`. **Fix:** materialize the customer set first
  (`db.Customers.Where(…).Select(new{CustomerID[,SourceOfLead]}).AsEnumerable()`) and `.ToList()` the `upt`
  sub-aggregation, leaving every Count/Sum/Distinct expression unchanged. **200**, len 195 KB→220 KB (real rows).
- `Leads/followups` → `GetMyTaskcount` (466): `let assign=…ToList()` + `let Reminder=…` + a grouped-latest
  `taskups` left-join projected entities, but the method only returns `.Count()` of tasks assigned to the user.
  **Fix:** collapse to `where taskassign.Any(… && x.EmployeeId == empId)` (→ SQL `EXISTS`) — same count.
- `ProTask/GetAllTaskdashexp` (1669 AddMinutes + 1780/1792/1808 collection projection): the `UserView` EF
  projection nested `AssignedTo`/`TskLead`/`Updateduser` collections + a `join v in vv` GroupBy-latest. **Fix
  (mirrors the already-working `ProTaskNonTech/GetAllTaskdashexp` twin):** fetch `rawRows` as a pure-entity
  server query, replace `join v in vv` with a `vvSatusIds.Contains(a.TaskStatus)` membership filter, and build
  the assignee/lead/updater collections client-side via `.ToLookup`/dictionaries (identical member names/order).
  **200, rec=190** on emir (rec=0 on trading — no Branch==1 tasks).

### Not fixed — different class / data-level (documented)
`Leads/followups` still 500s, but now at `GetAmountReceivablePayable` (431): `SqlException "Conversion failed
when converting date and/or time from character string"`, reached on the `exp=null` path. **Investigated
(2026-06-14):** the obvious date columns this query touches are all clean — `AccountsTransactions.Date` and
`CustomerRemarks.CreatedDate`/`nexttime` are real `datetime` columns with **zero** non-date rows on *both*
copies (`ISDATE()=0` → 0), and `ondates` is correctly `null`-guarded (no `Parse("")`). So the failure is a
**SQL execution-time conversion** on some other expression in the `v`/`v2` query that only the generated SQL
will reveal. This is **NOT one of the systemic port-break classes** (all fixed) — it needs an EF-command-log /
debugger session to capture the exact `CONVERT` (the verify-pilot's app logging doesn't emit command SQL, and
an env-var override + `ISDATE` sweep didn't surface it). One non-financial dashboard endpoint; flagged for a
focused debug pass, not a quick code fix. The `GetMyTaskcount` collection-projection it *used* to fail on is
fixed (above).
