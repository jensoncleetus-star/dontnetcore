# Installs ONE QuickSoft branch instance as an auto-start Windows service + opens its firewall port.
# Run from an ELEVATED PowerShell on the production server, once per branch, e.g.:
#
#   .\install-quicksoft-service.ps1 -Branch HeadOffice -Port 8080 -AppDir "C:\QuickSoft\HeadOffice"
#   .\install-quicksoft-service.ps1 -Branch Sharjah    -Port 8081 -AppDir "C:\QuickSoft\Sharjah"
#
# Before running: copy the publish bundle into $AppDir and place a filled-in appsettings.Production.json
# beside QuickSoft.exe (see appsettings.Production.template.json).
param(
    [Parameter(Mandatory=$true)][string]$Branch,
    [Parameter(Mandatory=$true)][int]$Port,
    [Parameter(Mandatory=$true)][string]$AppDir,
    [string]$Scheme = "http"   # use "https" only with the Kestrel certificate configured
)

$exe = Join-Path $AppDir "QuickSoft.exe"
if (-not (Test-Path $exe)) { throw "QuickSoft.exe not found in $AppDir - copy the publish bundle there first." }
if (-not (Test-Path (Join-Path $AppDir "appsettings.Production.json"))) {
    throw "appsettings.Production.json not found in $AppDir - copy + fill the template first (it holds the branch connection string)."
}

$svc = "QuickSoft-$Branch"
$bin = "`"$exe`" --urls ${Scheme}://0.0.0.0:$Port --environment Production --contentRoot `"$AppDir`""

# (Re)create the service.
sc.exe stop $svc 2>$null | Out-Null
sc.exe delete $svc 2>$null | Out-Null
sc.exe create $svc binPath= $bin start= auto DisplayName= "BOS - Business Operating System ($Branch)" | Out-Null
sc.exe description $svc "BOS (Business Operating System, .NET 10) - $Branch branch on port $Port" | Out-Null
sc.exe failure $svc reset= 86400 actions= restart/5000/restart/5000/restart/5000 | Out-Null  # auto-restart on crash

# Firewall: allow inbound on the branch port.
$rule = "QuickSoft $Branch ($Port)"
Get-NetFirewallRule -DisplayName $rule -ErrorAction SilentlyContinue | Remove-NetFirewallRule
New-NetFirewallRule -DisplayName $rule -Direction Inbound -Action Allow -Protocol TCP -LocalPort $Port | Out-Null

sc.exe start $svc | Out-Null
Start-Sleep -Seconds 5
$state = (sc.exe query $svc | Select-String "STATE").ToString()
Write-Host "service $svc -> $state"
try {
    $r = Invoke-WebRequest "${Scheme}://localhost:$Port/Users/Login" -UseBasicParsing -TimeoutSec 30
    Write-Host "login page -> HTTP $($r.StatusCode) OK"
} catch {
    Write-Warning "login page not reachable yet: $($_.Exception.Message). Check appsettings.Production.json + Windows Event Log (Application)."
}
