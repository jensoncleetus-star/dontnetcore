# QuickSoft (.NET 10) — Production Go-Live Runbook

One branch = one database = one Windows service = one port. Repeat §3–§6 per branch.
The bundle is **self-contained win-x64** — the server needs **no .NET installation**.

> **OPERATING POLICY (owner, 2026-06-12):** the live production database serves clients daily and is
> NEVER touched during development — all build/test/verification happens on copies. This runbook is
> executed only at the **final stage**, inside a **planned maintenance window (preferably overnight)**,
> after testing + UAT approval, so the migration carries every record up to that night. Rehearse the
> whole §2 flow on a fresh copy first (already proven on the two dev copies).

## 1. Build the deploy bundle (on the dev machine)
```powershell
cd "C:\Quick Soft 10-06-2026\QuickNetCore"
.\deploy\make-deploy-bundle.ps1          # -> deploy\bundle\  (QuickSoft.exe + wwwroot + Views + runtime)
```
Copy `deploy\bundle\` to the server (e.g. `C:\QuickSoft\<Branch>\`), one copy per branch.

> Build the bundle **FRESH at cutover time** — it republishes from current source, so it carries every
> latest fix (e.g. the systemic JSON save/print binding fix that repaired all 42 transaction screens, the
> futuristic dashboard, and the top-bar icons — all 2026-06-15). A stale bundle from an earlier date would
> ship without them. Views + `wwwroot` (incl. `dashboard-futuristic.css`) are included automatically.

## 2. FINAL DATA MIGRATION — cutover night, per branch (maintenance window)
The model: take that night's full production backup → restore it as the NEW system's database →
upgrade the restored copy → start the new services on it. **The live production DB itself is never
modified** — it keeps running as the instant rollback until sign-off.

1. **Full backup of the live branch DB** (end of business day / start of the window), then
   `RESTORE ... AS <BranchDb>_New` on the new system's SQL Server. All steps below run on the
   RESTORED database only.
2. Run the idempotent upgrade scripts against the restored DB **in order** (minutes total):
   - [`Sql/01_Identity_EF6_to_Core_Upgrade.sql`](../Sql/01_Identity_EF6_to_Core_Upgrade.sql) — Identity-Core columns/tables; all users + passwords preserved.
   - [`Sql/02_Performance_Indexes.sql`](../Sql/02_Performance_Indexes.sql) — additive indexes for the hottest ledger/line-item/approval queries (skips anything the schema can't index).
   - [`Sql/03_Security_Lockout.sql`](../Sql/03_Security_Lockout.sql) — enables the 5-try/5-min brute-force lockout, clears stale lock state.
   - [`Sql/04_AVCO_Performance.sql`](../Sql/04_AVCO_Performance.sql) — stock-valuation procedure rebuilt ~60× faster with byte-identical output (old body kept as `SP_AVCOMethod_LEGACY`).
   - [`Sql/05_AccountsReports_Fixes.sql`](../Sql/05_AccountsReports_Fixes.sql) — **run after 04.** Repairs the Trial Balance / Balance Sheet / P&L / Cashflow reports (audit-DB synonym fallback + `trialbalance.orderB`). See `docs/ACCOUNTS-AUDIT.md` §3a. If you also restore the `auditemirtechlatest` archive DB (decision D-D), do it **before** this script so the synonym binds to it.
   - [`Sql/06_InvoiceTemplate.sql`](../Sql/06_InvoiceTemplate.sql) — custom invoice-template designer: adds the new `InvoiceTemplate` table (+ a sample template) and the "Custom Design" menu/role under Settings. New table only — touches no existing data; idempotent. **Required** or the Custom Design feature 500s on a missing table.
3. Create/confirm a SQL login for the app (least privilege: db_datareader + db_datawriter + EXECUTE):
   ```sql
   CREATE LOGIN quicksoft_app WITH PASSWORD = '********';
   USE [<BranchDb>_New]; CREATE USER quicksoft_app FOR LOGIN quicksoft_app;
   ALTER ROLE db_datareader ADD MEMBER quicksoft_app;
   ALTER ROLE db_datawriter ADD MEMBER quicksoft_app;
   GRANT EXECUTE TO quicksoft_app;
   ```

### 2a. Rehearsed end-to-end (dry run, 2026-06-13)
The whole §2 path was rehearsed with [`cutover-dryrun.ps1`](cutover-dryrun.ps1) on the dev box using the
service-company copy (~3.7 GB data / 1.97 GB backup) and the **shipped bundle binaries**:

| step | measured |
|---|---|
| copy-only backup of the source DB | 2.8 s |
| restore as the scratch DB | 2.7 s |
| shipped Sql 01–04 (idempotent replay) | 0.3 s total |
| bundle `QuickSoft.exe` cold boot (Production) → first HTTP 200 | 19.4 s |
| login + 6-grid smoke (incl. 27K-row Quotation, lead dashboard drills) | all 200, 0.1–1.1 s each |

Also verified in Production mode: modern login renders, `/dev/*` endpoints are absent (404),
`X-Content-Type-Options`/`X-Frame-Options` headers present (HSTS header appears once TLS is on, §5).
Dev-box NVMe timings — **run the same script on the real server before the window** to get its true
numbers; the dominant cutover cost there will be the backup/restore I/O, not the scripts.

## 3. Configure the branch instance
In `C:\QuickSoft\<Branch>\`: copy `appsettings.Production.template.json` → **`appsettings.Production.json`**
and set the connection string to the RESTORED database (and the Kestrel certificate block if serving
HTTPS directly).

## 4. Install as a Windows service (elevated PowerShell)
```powershell
C:\QuickSoft\install-quicksoft-service.ps1 -Branch HeadOffice -Port 8080 -AppDir "C:\QuickSoft\HeadOffice"
```
The script creates auto-start service `QuickSoft-<Branch>`, crash auto-restart, the firewall rule,
starts it, and smoke-checks the login page.

## 5. TLS (choose one)
- **Kestrel direct**: put the `.pfx` on the server, fill the `Kestrel:Certificates` block, reinstall the
  service with `-Scheme https -Port 8443`.
- **Reverse proxy** (IIS/ARR or nginx): keep the service on localhost HTTP and bind the certificate on
  the proxy. (Matches the old IIS setup most closely.)

## 6. Same-night verification (per branch, before users arrive)
1. `http(s)://server:port/Users/Login` loads with styling (the modernized login).
2. Log in with a real account (passwords carried over from the production data).
3. Open: Customer list, Item list, one Quotation, one report (e.g. Sales register) — confirm the
   latest (yesterday's) records are present.
4. Create + delete a test record (e.g. an Item Category) to confirm writes.
5. Run the all-items stock valuation once (should complete in seconds now).
6. Run `99_Voucher_Balance_Check.sql` (in this bundle, report-only) against the restored DB and keep
   the output; re-run it after the first live day and at week-1 — the "last 14 days" result should
   stay empty and new discount rows must land on 497-Allowed / 498-Received
   (see `docs/ACCOUNTS-AUDIT.md`).

## 7. Switch + post-go-live hygiene
- Morning: point users at the new URL/port; the old app + old DB stay untouched and running as fallback.
- The `/dev/*` endpoints (`setpw`, `schemadiff`, `compileviews`) **do not exist** in Production
  (`ASPNETCORE_ENVIRONMENT=Production` — the installer sets this via `--environment Production`).
- Logs: Windows Event Log (Application) under source `.NET Runtime` / the service name.
- Updating the app later: stop service → replace folder contents (keep `appsettings.Production.json`) → start service.

### 7a. ⭐ SECURITY checklist — do at cutover (2026-06-15 audit)
The code-level findings are already FIXED in this build (credential-stealer beacons removed, no hardcoded
SMTP password in the binary, security headers incl. CSP frame-ancestors, QR uses the deployment's own origin).
These remaining items need an OWNER/ops action — none is a code change:
- [ ] **ROTATE the `rmo` SQL login** (`192.168.35.201`) — it was in source history. The app uses `quicksoft_app`
      (least-priv), NOT rmo, so rotating/disabling rmo is safe: `ALTER LOGIN rmo WITH PASSWORD = '<new>'` (or `DISABLE`).
- [ ] **ROTATE the 2 mailbox passwords** (`qucknet@quicknet.me`, `app@quicksoft.me`) — were in source history.
      Then set the new one as an env var on each service host: `setx Smtp__Password "<new>" /M` → restart the service.
      (The build now reads `Smtp__Password` from env, never a baked literal.)
- [ ] **Least-privilege SQL login** in the connection string (db_datareader + db_datawriter + EXECUTE), never sa (see §2 step 3).
- [ ] **`AllowedHosts`**: change `"*"` → the real hostname(s)/IP(s) of the branch in `appsettings.Production.json`.
- [ ] **HTTPS**: once TLS is the access path, set `"Security": { "RequireHttps": true }` in `appsettings.Production.json`
      (this turns on `UseHttpsRedirection`; cookies are already Secure under HTTPS). UseHsts is already on for non-Dev.
- [ ] Host the mobile **APK** (`ProTask/locationinstr.cshtml` currently `http://uk.ath.cx:1091/...emirtech.apk`) on the
      app's own https origin and update the link.
- [ ] (dev box) delete the stale `publish*/deploy/bundle` folders — inert old build artifacts that still hold the old
      rmo string on disk; they never ship (this runbook rebuilds a fresh bundle at §1).

## Rollback
Point users back at the OLD app/database (both untouched all night). Nothing to undo — the new
system ran on its own restored copy. Re-attempt the window another night.

## Reference
- Parity proof: [`MIGRATION-COMPLETENESS.md`](../MIGRATION-COMPLETENESS.md)
- Calculation/golden proofs: [`PHASE2-GOLDEN-RESULTS.md`](../PHASE2-GOLDEN-RESULTS.md)
- Snapshot-vs-live notes: [`SNAPSHOT-DIVERGENCES.md`](../SNAPSHOT-DIVERGENCES.md) (§C items to review on live data)
- UAT checklist: `UAT-CHECKLIST.xlsx`
