# Deployment artifact — READ FIRST

Release build of QuickSoft / EMIRTECH ERP, produced 2026-06-29, **including all Phase-3 security fixes (S8–S21) and Phase-4 calculation fixes (C1–C8, N1–N4).** Framework-dependent (.NET 10).

## Prerequisites on the target server
- **ASP.NET Core Runtime 10** (Hosting Bundle if using IIS; or just the runtime if self-hosting / Windows Service).
- SQL Server reachable with the **original/branch database** attached.

## Required step — point at the original database
This artifact ships `appsettings.Production.json` with the **pilot** connection string (`emirtechlatest`). Before go-live, set the real one (either edit the file or use an env var):

```
ConnectionStrings__DefaultConnection = "Server=...;Database=<branchDb>;User Id=...;Password=...;TrustServerCertificate=True;Encrypt=True"
```
- Use a **least-privilege SQL login** (db_datareader + db_datawriter + EXECUTE), never `sa`.
- In Production the app **throws on startup** if no connection string is configured (by design) — so this step is mandatory.
- The fixes are in code and apply to **any** database automatically — no re-audit, no schema migration needed (none of the fixes touch the schema).

## Manual security items (do once)
1. **Rotate the FCM push key** and set `Fcm__ServerKey` (the old key is compromised in git history). See security S11.
2. **Verify roles exist & are assigned** in the original DB: `All Payment Entry`, `All Receipt Entry`, `Dev, Approvals`, `Dev,Edit User`, `All Landlords`, `All vehicle`. The new authorization/scoping guards depend on them.
3. Emergency valve: if the global auth change (S8) ever locks someone out, set `"Security": { "RequireAuthByDefault": false }` and restart — no redeploy.
4. At TLS cutover: set `"Security": { "RequireHttps": true }` to turn on HTTPS redirect + Secure cookies.

## Verify after first start (golden / spot checks)
- Payslip total (N1), a supplier payment posting (N2), DayWise + Employee-wise reports (N3/N4 — confirm they render), VAT report Fujairah row (C1/C2), a quotation save (C3/C4).

## Optional — historical data
Code fixes correct all **new** transactions. Records written by the **old** code are not auto-corrected (e.g. past supplier payments posted both-legs-to-cash, quotations saved with SubTotal=0). Run a one-off data script only if those historical rows matter.

See `PHASE3-SECURITY-AUDIT.md` and `PHASE4-CALC-AUDIT.md` in the source repo for full detail.
