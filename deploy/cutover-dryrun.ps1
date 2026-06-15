# Cutover dry-run rehearsal (DEPLOY-RUNBOOK §2) — run this ON THE TARGET SERVER before the real
# maintenance window to prove the whole path and measure its duration there:
#   copy-only backup -> restore as a scratch DB -> shipped Sql 01-04 -> boot the bundle exe in
#   Production mode against the scratch DB -> login + grid smoke -> cleanup.
# The PRODUCTION database is only read (COPY_ONLY backup); everything else happens on the scratch copy.
#
# usage (defaults shown):
#   .\cutover-dryrun.ps1 -SqlInstance .\SQLEXPRESS -SourceDb emirtechlatest -ScratchDb cutover_DRYRUN `
#                        -BundleDir "$PSScriptRoot\bundle" -Port 8082 -LoginEmail jenson@gmail.com -LoginPassword '...'
param(
    [string]$SqlInstance = ".\SQLEXPRESS",
    [string]$SourceDb = "emirtechlatest",
    [string]$ScratchDb = "cutover_DRYRUN",
    [string]$BundleDir = (Join-Path $PSScriptRoot "bundle"),
    [int]$Port = 8082,
    [string]$LoginEmail = "jenson@gmail.com",
    [string]$LoginPassword = ""
)

$ErrorActionPreference = 'Stop'
function Step([string]$name, [scriptblock]$body) {
    $t = Measure-Command { & $body }
    "{0,-46} {1,8:N1} s" -f $name, $t.TotalSeconds
}

# resolve the instance's default backup/data dirs from the master files
$dataDir = (sqlcmd -S $SqlInstance -E -Q "SET NOCOUNT ON; SELECT TOP 1 LEFT(physical_name, LEN(physical_name)-CHARINDEX('\', REVERSE(physical_name))) FROM sys.master_files WHERE database_id = DB_ID('$SourceDb')" -W -h -1).Trim()
$bak = Join-Path (Split-Path $dataDir -Parent) "Backup\$ScratchDb.bak"

"== cutover dry-run: $SourceDb -> $ScratchDb (instance $SqlInstance) =="

Step "1. copy-only backup of $SourceDb" {
    sqlcmd -S $SqlInstance -E -Q "BACKUP DATABASE [$SourceDb] TO DISK=N'$bak' WITH COPY_ONLY, INIT" -b | Out-Null
}

# logical file names travel with the backup — read them from the file itself
$fl = sqlcmd -S $SqlInstance -E -Q "RESTORE FILELISTONLY FROM DISK=N'$bak'" -W -h -1
$logicalData = (($fl | Where-Object { $_ -match '\.mdf' }) -split '\s+')[0]
$logicalLog  = (($fl | Where-Object { $_ -match '\.ldf' }) -split '\s+')[0]

Step "2. restore as $ScratchDb" {
    sqlcmd -S $SqlInstance -E -Q "RESTORE DATABASE [$ScratchDb] FROM DISK=N'$bak' WITH MOVE N'$logicalData' TO N'$dataDir\$ScratchDb.mdf', MOVE N'$logicalLog' TO N'$dataDir\${ScratchDb}_log.ldf', REPLACE" -b | Out-Null
}

foreach ($f in '01_Identity_EF6_to_Core_Upgrade.sql','02_Performance_Indexes.sql','03_Security_Lockout.sql','04_AVCO_Performance.sql') {
    Step "3. $f" {
        $out = sqlcmd -S $SqlInstance -d $ScratchDb -E -i (Join-Path $BundleDir $f) -b 2>&1
        if ($LASTEXITCODE -ne 0) { throw "script failed: $f`n$($out | Select-Object -Last 5)" }
    }
}

"4. booting bundle exe (Production) on :$Port against $ScratchDb..."
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:ASPNETCORE_URLS = "http://0.0.0.0:$Port"
$env:ConnectionStrings__DefaultConnection = "Server=$SqlInstance;Database=$ScratchDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
$proc = Start-Process (Join-Path $BundleDir "QuickSoft.exe") -WindowStyle Hidden -PassThru
$boot = Measure-Command {
    $ok = $false
    foreach ($i in 1..90) { Start-Sleep 1; try { if ((Invoke-WebRequest "http://localhost:$Port/Users/Login" -UseBasicParsing -TimeoutSec 5).StatusCode -eq 200) { $ok = $true; break } } catch {} }
    if (-not $ok) { throw "bundle exe did not come up on :$Port" }
}
"{0,-46} {1,8:N1} s" -f "   boot -> first 200", $boot.TotalSeconds

try { Invoke-WebRequest "http://localhost:$Port/dev/setpw" -UseBasicParsing -TimeoutSec 10 | Out-Null; "   WARNING: /dev/* reachable in Production!" }
catch { "   /dev/* endpoints absent in Production: OK" }

if ($LoginPassword -ne "") {
    $lp = Invoke-WebRequest "http://localhost:$Port/Users/Login" -UseBasicParsing -SessionVariable websess
    $tok = ([regex]::Match($lp.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')).Groups[1].Value
    $r = Invoke-WebRequest "http://localhost:$Port/login" -Method POST -Body @{Email=$LoginEmail;Password=$LoginPassword;RememberMe='false';fromapp='';__RequestVerificationToken=$tok} -WebSession $websess -UseBasicParsing -MaximumRedirection 0 -EA SilentlyContinue
    "   login as $LoginEmail -> HTTP $($r.StatusCode) (302 = success)"
} else {
    "   (no -LoginPassword given: log in manually at http://localhost:$Port and click through a few screens)"
}

Read-Host "smoke-test in the browser now if desired; press ENTER to clean up (stops exe, drops $ScratchDb)"

Stop-Process -Id $proc.Id -Force -Confirm:$false -ErrorAction SilentlyContinue
sqlcmd -S $SqlInstance -E -Q "IF DB_ID('$ScratchDb') IS NOT NULL BEGIN ALTER DATABASE [$ScratchDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$ScratchDb]; END" -b | Out-Null
sqlcmd -S $SqlInstance -E -Q "EXEC master.sys.xp_delete_file 0, N'$bak'" -b 2>$null | Out-Null
"cleaned up (scratch DB dropped; backup file removed if the instance allows xp_delete_file)."
