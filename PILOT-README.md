# QuickNet (.NET 10) — Pilot on this box (UAT)

The migrated app is **running right now** as a background process, in **Production** mode, against
**`emirtechlatest`** (your safe copy of real data — the Identity schema upgrade is already applied, so
existing passwords work). Run it in parallel with the legacy QuickNet and compare.

## Access
- **On this PC (DESKTOP-UH8DQ0M):** http://localhost:8080  ✅ works now
- **From your team's PCs (same Wi-Fi/LAN):** http://192.168.35.222:8080
  → needs the **firewall rule** in step A below (Windows blocks inbound by default).
- **Login:** your existing users work with their real passwords (e.g. `jenson`). Test account: `027361` / `Test1234`.

## What to UAT
Log in and open the modules you actually use daily, comparing against the legacy app:
Sales / Quotation / CreditSale, Customer, Inventory / Item / Stock, Accounts / Ledger / Day Book,
AMC, Purchase, Reports, and download an invoice/quotation **PDF**. Note anything that looks or behaves
differently — that's the per-screen runtime tail I'll fix.

> ⚠️ The 7 `/Index` URLs that 500 (FinancialYear, Registers, VendorRelation, …) are **legacy dead-ends**
> (the old app has no such page either, and nothing links to them) — ignore them, the menu never goes there.

---

## Make it permanent (run ONCE in an **Administrator** PowerShell)

The background process I started stops if this PC reboots or you log off. To make it a proper
auto-starting Windows service (and let the team reach it):

```powershell
# A. Firewall — let the LAN reach the pilot on :8080
New-NetFirewallRule -DisplayName "QuickNet Pilot 8080" -Direction Inbound -Protocol TCP -LocalPort 8080 -Action Allow

# B. Stop the temporary background pilot (frees :8080 for the service)
Get-CimInstance Win32_Process -Filter "name='dotnet.exe'" |
  Where-Object { $_.CommandLine -match 'publish\\QuickSoft\.dll' } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force }

# C. Create + start the service. Run it as YOUR Windows user so the DB's Trusted_Connection works
#    (Get-Credential will prompt for your Windows username + password).
New-Service -Name QuickNetPilot -DisplayName "QuickNet (.NET10) Pilot" -StartupType Automatic `
  -BinaryPathName '"C:\Program Files\dotnet\dotnet.exe" "C:\Quick Soft 10-06-2026\QuickNetCore\publish\QuickSoft.dll" --urls http://0.0.0.0:8080 --environment Production --contentRoot "C:\Quick Soft 10-06-2026\QuickNetCore\publish"' `
  -Credential (Get-Credential)
Start-Service QuickNetPilot
```

Stop / remove later: `Stop-Service QuickNetPilot` · `sc.exe delete QuickNetPilot`

---

## Notes
- **DB account for the service:** running the service as your user (step C) is the simplest way to keep
  `Trusted_Connection`. Alternatively, create a SQL login, put it in `publish\appsettings.Production.json`
  (`...User Id=...;Password=...`), and you can run the service as the default `LocalSystem`.
- **Fresh data:** to UAT on cleaner data, restore a fresh copy of the real DB, run
  `Sql\01_Identity_EF6_to_Core_Upgrade.sql` on it, and point `appsettings.Production.json` at it.
- **Real production cutover later:** same bundle — change the connection string to the real branch DB,
  run the Identity SQL on it, put IIS/nginx in front for HTTPS. See `DEPLOYMENT.md`.
- **Restart my temporary pilot** (if you just want it back without a service):
  `Start-Process "C:\Program Files\dotnet\dotnet.exe" -Args '"C:\Quick Soft 10-06-2026\QuickNetCore\publish\QuickSoft.dll" --urls http://0.0.0.0:8080 --environment Production --contentRoot "C:\Quick Soft 10-06-2026\QuickNetCore\publish"' -WindowStyle Hidden`
