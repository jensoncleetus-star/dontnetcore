# Builds the production deploy bundle: SELF-CONTAINED win-x64 publish (server needs no .NET install).
# Output: deploy\bundle\  â€” copy that folder to the server (one copy per branch) and follow DEPLOY-RUNBOOK.md.
$root = Split-Path $PSScriptRoot -Parent
$out  = Join-Path $PSScriptRoot "bundle"

# always start from a clean output dir so stale files from earlier builds never ship
if (Test-Path $out) { Remove-Item $out -Recurse -Force -Confirm:$false }

Write-Host "publishing self-contained win-x64 -> $out"
dotnet publish $root -c Release -r win-x64 --self-contained true -o $out --nologo `
    /p:PublishSingleFile=false /p:PublishTrimmed=false
if ($LASTEXITCODE -ne 0) { throw "publish failed" }

# Ship the per-branch config template + installer + cutover rehearsal beside the binaries.
Copy-Item (Join-Path $PSScriptRoot "appsettings.Production.template.json") $out -Force
Copy-Item (Join-Path $PSScriptRoot "install-quicksoft-service.ps1") $out -Force
Copy-Item (Join-Path $PSScriptRoot "cutover-dryrun.ps1") $out -Force
Copy-Item (Join-Path $root "Sql\01_Identity_EF6_to_Core_Upgrade.sql") $out -Force
Copy-Item (Join-Path $root "Sql\02_Performance_Indexes.sql") $out -Force
Copy-Item (Join-Path $root "Sql\03_Security_Lockout.sql") $out -Force
Copy-Item (Join-Path $root "Sql\04_AVCO_Performance.sql") $out -Force
Copy-Item (Join-Path $root "Sql\05_AccountsReports_Fixes.sql") $out -Force
Copy-Item (Join-Path $root "Sql\06_InvoiceTemplate.sql") $out -Force         # custom invoice designer: new table + seed + menu/role (idempotent)
Copy-Item (Join-Path $root "Sql\99_Voucher_Balance_Check.sql") $out -Force   # report-only book-watch
Copy-Item (Join-Path $PSScriptRoot "DEPLOY-RUNBOOK.md") $out -Force

$size = [math]::Round((Get-ChildItem $out -Recurse -File | Measure-Object Length -Sum).Sum/1MB,0)
Write-Host "bundle ready: $out ($size MB)"


