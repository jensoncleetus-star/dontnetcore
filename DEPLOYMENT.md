# QuickNet (.NET 10 / ASP.NET Core) — Deployment & Cutover Guide

This is the faithful port of the legacy ASP.NET MVC 5 app to **.NET 10 / EF Core 10**. Same database,
same screens, same look. This guide covers taking it from the dev box to a branch's production server.

> Status: foundation + core flows are done and proven (build = 0 errors; login, dashboard, most
> screens, invoice/quotation PDF, Property/Hr areas all work). A per-screen long tail of complex
> report queries remains (they degrade gracefully — see §7). Test thoroughly per branch before cutover.

---

## 1. Prerequisites (per server)
- **.NET 10 ASP.NET Core Runtime** (or the self-contained publish, which bundles it).
- **SQL Server** reachable from the app server (one database per branch, as today).
- Windows or Linux. (Kestrel; put IIS / nginx / a load balancer in front for TLS — see §5.)

## 2. ⭐ Database migration — run ONCE per branch database
ASP.NET Core Identity needs columns/tables that the legacy EF6 Identity-2 schema lacks. **Without this,
login fails** ("Invalid column/object name"). Run the idempotent script against EACH branch DB:

```
sqlcmd -S <server> -d <branchDb> -i Sql/01_Identity_EF6_to_Core_Upgrade.sql
```
It adds `NormalizedUserName/NormalizedEmail/ConcurrencyStamp/LockoutEnd` to AspNetUsers, the role
columns to the AppModules table, and the `AspNetRoleClaims`/`AspNetUserTokens` tables. It is safe to
re-run. **Existing passwords keep working** (Core verifies the legacy PBKDF2 hashes) — no reset needed.

> Note: the app ALSO maps each entity to its real EF6-pluralized table and shadow-FK columns at runtime
> (in `ApplicationDbContext.OnModelCreating`) by reading `INFORMATION_SCHEMA` — so no table renames are
> needed, but the app's DB login must have read access to `INFORMATION_SCHEMA`.

## 3. Configuration (per branch)
Set the connection string (REQUIRED in Production — the app throws on startup if missing):
- `appsettings.Production.json`:
  ```json
  { "ConnectionStrings": { "DefaultConnection": "Server=...;Database=<branchDb>;User Id=...;Password=...;TrustServerCertificate=True" } }
  ```
  or env var `ConnectionStrings__DefaultConnection`, or user-secrets.
- Set `ASPNETCORE_ENVIRONMENT=Production` (this also disables the dev `/dev/setpw` endpoint — see §6).
- `ASPNETCORE_URLS` (e.g. `http://0.0.0.0:5000`) or configure the reverse proxy.

## 4. Build / publish
```
dotnet publish QuickNetCore.csproj -c Release -o publish
```
Ship the `publish/` folder. Views are runtime-compiled (Razor RuntimeCompilation), so the `Views/` and
`Areas/*/Views/` folders and `wwwroot/` MUST be deployed alongside the DLLs.

## 5. Hosting / TLS
- Behind **IIS** (AspNetCoreModuleV2), **nginx**, or a load balancer terminating TLS.
- Kestrel header limit is already raised (admins have 100s of roles → large auth cookie). If a reverse
  proxy is used, raise ITS header limits too (nginx `large_client_header_buffers`, IIS request limits).

## 6. Security checklist (do before go-live)
- [ ] `ASPNETCORE_ENVIRONMENT=Production` is set → the **`/dev/setpw`** endpoint is NOT mapped. (It is
      Development-only by code, but verify. Better: delete the `if (app.Environment.IsDevelopment())`
      dev block in `Program.cs` for the production build.)
- [ ] Connection string uses a **least-privilege SQL login** (not sa), over an encrypted connection.
- [x] The earlier security finding (legacy "credential-stealer" on-load beacon to quicknet.fortiddns.com)
      is NOW actually removed (2026-06-15 audit — the prior "fixed" claim was false; two live beacons in
      StockValue/SalesProfit reports + a commented copy were deleted). polyfill.io: confirmed absent.
- [ ] **ROTATE** the `rmo` SQL login and the 2 SMTP mailbox passwords (were in source history); the build
      now reads the SMTP password from env `Smtp__Password`, not a literal. Full security checklist:
      [`deploy/DEPLOY-RUNBOOK.md`](deploy/DEPLOY-RUNBOOK.md) §7a.
- [ ] **Data Protection keys** are persisted to a stable location (auth-cookie + ticket encryption). The
      default is a per-machine folder; for **multiple servers** configure a shared key ring
      (`AddDataProtection().PersistKeysToFileSystem(<shared>).SetApplicationName("QuickNet")`).

## 7. Known limitations / follow-ups (track these)
- **Auth ticket store is in-memory** (`MemoryTicketStore`): users are logged out on app restart, and it
  is single-server. For multiple servers, swap `IMemoryCache` for an `IDistributedCache` (Redis/SQL).
- **Complex report queries** (e.g. `/MyReports/GetApproval` — a 12-section query) are not yet ported to
  EF Core; they 500 and the global `AjaxErrorFilter` returns an empty table (logged as `[AjaxError]` to
  stderr) so the dashboard stays usable. Port these per report.
- **Invoice/quotation PDF** works via **iText 7 pdfHTML** — ⚠️ iText is **AGPL or commercial**; obtain an
  iText commercial license for closed-source use, or switch to a browser-based renderer
  (PuppeteerSharp/Playwright). Multi-page header/footer (legacy ITextEvents) not yet re-created.
- **Deferred** (excluded from the build): `FileManagerController` (Syncfusion — licensed file browser),
  `VehicleServiceController` (a copy-paste duplicate of VehicleMaster, which works), the OWIN OAuth
  `Providers/**`, and the iTextSharp PDF helper files (the shim provides the working PDF path).
- **Per-screen tail**: not every one of the 204 controllers / ~2000 views has been exercised at runtime;
  test each module per branch. Recurring issues converge via the patterns already applied (SelectList
  case-insensitivity, dates, JSON casing, FK mapping, DefaultIfEmpty, etc.).

## 8. Post-deploy smoke test (per branch)
1. `GET /Users/Login` renders (styled).
2. Log in with an existing user (legacy password).
3. Dashboard loads with real counts/charts; PDC/Reminders tables populate; dates correct; no JS alerts.
4. Open the key modules the branch uses (Sales, Quotation, Customer, Inventory, Accounts, Property/Hr).
5. Download an invoice/quotation PDF.
6. Watch the app log for `[AjaxError]` / `[PDF]` / `[RemapLegacyTableNames]` lines.
