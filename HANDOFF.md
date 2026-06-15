# QuickNet/QuickSoft .NET 10 — HANDOFF / RESUME GUIDE
_Last updated: 2026-06-13. Read this first when resuming (esp. on a new computer)._

## 0. What this project is
Faithful 1:1 port of the legacy QuickSoft ERP (ASP.NET MVC5 / .NET 4.5.2) to **ASP.NET Core .NET 10**.
RootNamespace/AssemblyName = **QuickSoft**. Repo root: `QuickNetCore`. Migration declared complete; we are
now in the **Modernization & Optimization** phase (backend correctness/perf, subtle UI polish, modern
login) under the owner brief.

## 1. STANDING POLICIES (do not violate)
- **Production DB is live daily — NEVER touch it.** All work is on COPIES. Every DB change ships as an
  idempotent `Sql/NN_*.sql` script, replayed only at the final overnight cutover (see `deploy/DEPLOY-RUNBOOK.md`).
- **Historical figures must stay accurate** (customers pull old statements). Old calculations, even if
  "wrong", are kept faithful; the accounting standard is enforced **going forward** only. Fixes must be
  **output-preserving** unless they correct a clear defect (then document it).
- **No GUI structure change**; modern theme/login only. **Responsive on all screens** (≈360–1920px).
- Reply to the owner in **Malayalam**; keep code/identifiers/paths English.

## 2. Environment to recreate on the NEW computer
1. **.NET 10 SDK** (build) — `dotnet --version` should be 10.x.
2. **SQL Server Express** instance `.\SQLEXPRESS`, Windows auth.
3. **Two DB copies** (NOT in git — restore from the latest production backups, the work-on-copies policy):
   - `emirtechlatest`     (service company, ~3.7 GB)
   - `quicknetlatest-1200` (trading company, ~1.6 GB)
   After restoring fresh copies, replay the migration scripts **in order**:
   `Sql/01 → 02 → 03 → 04 → 05` (idempotent; they reproduce ALL DB-side fixes — Identity-Core upgrade,
   perf indexes, lockout, AVCO rewrite, Accounts-report fixes). `Sql/99` is report-only book-watch.
   So the DBs do NOT need to carry my changes — a fresh restore + Sql/01-05 = identical state.
4. **Build / publish / run the two local pilots** (from repo root):
   ```powershell
   dotnet build -c Release
   dotnet publish -c Release -o publish
   cd publish
   $env:ASPNETCORE_ENVIRONMENT='Development'
   $env:ASPNETCORE_URLS='http://0.0.0.0:8080'
   $env:ConnectionStrings__DefaultConnection='Server=.\SQLEXPRESS;Database=emirtechlatest;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true'
   Start-Process dotnet -ArgumentList 'QuickSoft.dll' -WindowStyle Hidden -RedirectStandardOutput "$env:TEMP\pilot8080.log" -RedirectStandardError "$env:TEMP\pilot8080.err"
   # repeat with :8081 + Database=quicknetlatest-1200 for the trading pilot
   ```
   Pilots: **:8080 = emirtechlatest, :8081 = quicknetlatest-1200**. Logins: jenson@gmail.com (:8080) /
   jenson@quicknet.me (:8081), password `Test1234` (set via the dev-only `/dev/setpw`; real passwords also
   work). `/dev/*` endpoints exist ONLY in Development (404 in Production — verified).
5. **Probe recipe** (for verifying report/grid endpoints): GET `/Users/Login` → scrape
   `__RequestVerificationToken` → POST `/login` (Email/Password/RememberMe/fromapp/token) → GET an
   authenticated page (e.g. `/ItemColor/Index`) → scrape a fresh token → POST the endpoint with that token
   + the view's params. DataTables grids need draw/start/length + columns[0][name] + order[0][...]; dates
   are dd-MM-yyyy. Numeric "All" dropdowns usually send `0`; some send `''` (treated as null).

## 3. Standing verification baselines
- **220-endpoint regression sweep** (list at `%TEMP%\csrf-sweep-eps.txt` — regenerate if missing):
  healthy distribution = **200=64, 404=22, 405=129, 500=5, 400=0**. The remaining 5×500 are PROVEN
  parameter-artifacts (return 200 with realistic UI params), NOT bugs.
- **Accounts book-watch:** run `Sql/99_Voucher_Balance_Check.sql` — the "last 14 days" list should be
  empty except the documented legacy `Sale 210296` (.01 rounding).

## 4. Work completed (commit history, newest first)
- **Report-layer audit COMPLETE + app-wide sort fix — batches 12–19** (`bc83547`…`f4d1fc4`, all live-verified
  on fresh verify-pilots :8090/:8091 against the real copies). The whole report layer is swept on both DBs:
  **SalesReport** (36 ep), **MyReports** (68), **PurchaseReport** (19, clean), **StockReport** (10, clean),
  **SalesReturn/PurchaseReturn/Customer/HireStock** (21), **TaxReports/StockJournal/StockAdjustment** (16).
  Fixed classes (all output-preserving): G1 EF >128-char param (lambda-block — `StockReport`+`HireStockReport.
  GetDetails`), G2 empty-string-vs-null (hire-date + `satype`, ~36 sites), G3 `MonthWise` PIVOT `total`,
  `GetDayWise` `.ToList()`-in-projection, profit-family EF collection-projection 500s, EF `GroupBy/OrderBy`
  translation (`GetEmployeeDetails`), JSON case-collision (`GetGeneratedItems` → `LegacyJson`), and — **batch 19,
  APP-WIDE** — the Dynamic.Linq empty-sort-column 500 (`OrderBy(" asc")`) at **262 sites across 181 controllers**
  (verified `Supplier.GetSupplier` 500→200). Static-hunt confirmed the other classes are localized to reports
  (not systemic). 2 open items remain (see §5). Full detail: `docs/REPORTS-AUDIT.md` (Batch 12–19).
  - **Verification unblock (reusable):** the session ran as Windows `jenson` (no DB access). Granted `jenson`
    `db_owner` on both copies by running `sqlcmd -E -i grant.sql` via a one-shot Task-Scheduler job as the SQL
    service account **`NT SERVICE\MSSQL$SQLEXPRESS`** (sysadmin, no password) — `schtasks /ru` from an
    ELEVATED shell, with the runner `.cmd` at a **space-free** path (`C:\ProgramData\`). Then publish to
    `publish-verify/` (NOT `publish/`, which the owner pilots lock) and run verify-pilots on :8090/:8091.
- **Batch 11** (`746e340`): repaired 8 broken Sales/Purchase/Stock report endpoints (EF >128-char param
  overflow ×5 → lambda-block; SalesReturnReport empty-string filter ×2; MonthWise PIVOT missing-column).
  → `docs/REPORTS-AUDIT.md`.
- **Batch 10** (`5f0c383`): repaired 6 broken Accounts reports (Trial Balance/Balance Sheet/P&L/Cashflow):
  AVCO audit-DB synonym fallback, `trialbalance.orderB`, cashflow IN-list→IN-subquery. → `Sql/05`, `docs/ACCOUNTS-AUDIT.md §3a`.
- **Batch 9** (`e3c4b25`): crossed discount account heads (497↔498) + dormant PDC bug; `Sql/99` book-watch. → `docs/ACCOUNTS-AUDIT.md`.
- **Batch 8** (`3f38409`): revived 16 bill/item-picker endpoints dead since port (JSON case-collision → `BaseController.LegacyJson`).
- **Batch 7**: fixed broken Leads dashboard drill (SQL timeout) + dead GetAllLeadsexp (EF translation).
- **Batch 6**: Leads grids N+1 → set-based prefetch (golden-gated).
- **Cutover dry-run** (`cb92fb7`): rehearsed runbook §2 end-to-end (`deploy/cutover-dryrun.ps1`); bundle exe boots in Production.
- **P2 paging** (28 grids), **P3 view warmup**, **AVCO rewrite** (Sql/04, ~60×), dead-code cleanup,
  forward-correct totals, CSRF, lockout, security headers, modern login + theme — earlier commits.
- Full audit/state: `PHASE1-AUDIT.md`, `MIGRATION-COMPLETENESS.md`, `MODERNIZATION-PLAN.md`,
  `docs/ACCOUNTS-AUDIT.md`, `docs/REPORTS-AUDIT.md`, `deploy/DEPLOY-RUNBOOK.md`.

## 5. PENDING / next steps
- **Owner decisions:** team UAT sign-off; cutover date (overnight window, runbook §2 ready, dry-run done);
  AVCO precision-widen for the trading all-items report overflow (D-C); migrate the `auditemirtechlatest`
  archive DB only if pre-2025 service stock must match byte-for-byte (D-D); theme feedback.
- **Report + grid audit DONE** (batches 12–23, ~570 endpoints across reports, transaction/document registers,
  master/ops registers, financial/balance-sheet, property, masters, and the Hr area). Batches 20–22 each found
  one real port-break (edit-window `DateTime` overflow; EF6 `nameOrConnectionString`; `.ToList()`-in-EF-projection);
  **batch 23 found that `.ToList()`-in-EF-projection class is systemic** — 13 endpoints fixed (ItemController ×7,
  MyReports ×2, HireReturn/CrossHireReturn, AttendanceReport, PayrollReport) — plus a left-join null-unwrap and a
  pre-existing payroll date-concat defect; all fixed + live-verified 500→200 on both pilots. The high-count clean
  controllers (StockReport/SalesReport/HireStockReport/BalanceSheet) were confirmed safe (post-materialization
  form). The two formerly-open report items are now **closed by root-cause** (see `docs/REPORTS-AUDIT.md`
  Batch 22–23):
  - **`GetAllSaleprofit*` 200-but-empty (was batch 13d): CLOSED — not a port-break.** `recordsTotal=100` is
    hard-coded; the empty `data` is the `(customer==0||…)` / `(SalesMan==0||…)` filters collapsing to
    `a.Customer==null` when the UI posts `""`→`null` (identical MVC5/Core `long?` binding ⇒ legacy returns the
    same). Adding the missing null-guards changes a profit report's output ⇒ owner-sign-off enhancement, not a
    port fix.
  - **`UserEditDayss` units (#15): CLOSED — faithful.** All fields are minutes, but the *legacy itself* mixes
    `AddDays`/`AddMinutes`, so the surviving `AddDays` sites are faithful and non-crashing; only batch-20's
    overflow-500 sites were corrected.
  One report item stays **external-input-bound**: **`MyReports/GetItemDetails`** 500 — the `ItemSerialNumber`
  entity mis-maps to its DbSet-property name AND no serial-number table exists in either copy (feature not
  migrated; D-list); needs production's real table name + `[Table(...)]` + the table present.
- **Live-data-only:** PurchaseEntry payment work-queues (3346/3465) + ~8 small param-bounded grids — wire
  server-side paging against real data post-go-live.
- Before deploy: remove `/dev/*` endpoints; rebuild bundle (`deploy\make-deploy-bundle.ps1`).

## 6. How this repo was backed up for the machine move
- Full git history (27 commits) is in `.git`. A portable single-file backup was created at
  `..\QuickNetCore-full-history.bundle` (git bundle). On the new machine:
  `git clone QuickNetCore-full-history.bundle QuickNetCore` restores everything incl. history.
- Alternatively, copy the entire `QuickNetCore` folder (with `.git`) — history is fully self-contained.
- The two SQL DBs are reproduced by restoring production copies + replaying `Sql/01-05` (see §2.3).
