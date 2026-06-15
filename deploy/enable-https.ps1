<#
.SYNOPSIS
    Enable HTTPS/TLS for BOS (Business Operating System).

    Generates a self-signed certificate (covering localhost + this machine's hostname + LAN IP),
    optionally trusts it on THIS machine, and launches the published app over HTTPS via Kestrel.
    Daily HTTP pilots on 8080/8081 are NOT touched — this binds a separate HTTPS port.

.WHY HTTPS
    - Phones can only INSTALL the PWA / register the service worker on a "secure context"
      (HTTPS with a trusted cert, or localhost). Plain http://192.168.x.x will not install.
    - Protects login credentials + ERP data in transit on the LAN.

.EXAMPLES
    # First time on the server (generates cert, trusts it locally, runs on 8443 against emirtechlatest):
    .\enable-https.ps1 -Branch emirtechlatest -Port 8443 -Trust

    # Re-launch later (cert already exists):
    .\enable-https.ps1 -Branch emirtechlatest -Port 8443

.NOTES
    Self-signed is fine for LAN / on-prem (install bos-https.cer on each phone — see HTTPS-SETUP.md).
    For internet / SaaS use a REAL domain + auto-TLS (Caddy / win-acme / Let's Encrypt) instead.
#>
param(
    [string]$Branch       = 'emirtechlatest',
    [int]   $Port         = 8443,
    [string]$DbServer     = '.\SQLEXPRESS',
    [string]$AppDir       = 'C:\Quick Soft 10-06-2026\QuickNetCore\publish',
    [string]$CertDir      = 'C:\Quick Soft 10-06-2026\certs',
    [string]$CertPassword = 'BosHttps#2026',
    [string[]]$ExtraSanDns = @(),
    [string[]]$ExtraSanIp  = @(),
    [switch]$Trust,
    [string]$Environment  = 'Production'
)

$ErrorActionPreference = 'Stop'
$pfx = Join-Path $CertDir 'bos-https.pfx'
$cer = Join-Path $CertDir 'bos-https.cer'

# --- 1. Generate the certificate if it does not exist yet -----------------------------------------
if (-not (Test-Path $pfx)) {
    New-Item -ItemType Directory -Force -Path $CertDir | Out-Null

    $hostName = [System.Net.Dns]::GetHostName()
    $lanIps = Get-NetIPAddress -AddressFamily IPv4 |
        Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' } |
        Select-Object -ExpandProperty IPAddress

    $dnsList = @('localhost', $hostName) + $ExtraSanDns | Select-Object -Unique
    $ipList  = @('127.0.0.1') + $lanIps + $ExtraSanIp | Select-Object -Unique

    $sanParts = @()
    foreach ($d in $dnsList) { $sanParts += "DNS=$d" }
    foreach ($i in $ipList)  { $sanParts += "IPAddress=$i" }
    $san = "2.5.29.17={text}" + ($sanParts -join '&')
    Write-Host "Generating self-signed cert. SAN: $($sanParts -join ', ')"

    $cert = New-SelfSignedCertificate `
        -Subject "CN=BOS - Business Operating System" `
        -FriendlyName "BOS HTTPS (self-signed)" `
        -TextExtension $san `
        -KeyExportPolicy Exportable -KeyAlgorithm RSA -KeyLength 2048 `
        -Type SSLServerAuthentication -KeyUsage DigitalSignature, KeyEncipherment `
        -NotAfter (Get-Date).AddYears(3) `
        -CertStoreLocation Cert:\CurrentUser\My

    $pw = ConvertTo-SecureString $CertPassword -AsPlainText -Force
    Export-PfxCertificate -Cert $cert -FilePath $pfx -Password $pw | Out-Null
    Export-Certificate   -Cert $cert -FilePath $cer | Out-Null
    Write-Host "Cert written: $pfx (thumbprint $($cert.Thumbprint))"
} else {
    Write-Host "Reusing existing cert: $pfx"
}

# --- 2. Optionally trust the cert on THIS machine (one-time; shows a Windows consent dialog) -------
if ($Trust) {
    Write-Host "Trusting cert on this machine (click 'Yes' on the security prompt if it appears)..."
    Import-Certificate -FilePath $cer -CertStoreLocation Cert:\CurrentUser\Root | Out-Null
    Write-Host "Trusted in CurrentUser\Root."
}

# --- 3. Launch the app over HTTPS (separate process; does not affect the 8080/8081 HTTP pilots) ----
$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:ASPNETCORE_URLS        = "https://0.0.0.0:$Port"
$env:ASPNETCORE_Kestrel__Certificates__Default__Path     = $pfx
$env:ASPNETCORE_Kestrel__Certificates__Default__Password = $CertPassword
$env:ConnectionStrings__DefaultConnection =
    "Server=$DbServer;Database=$Branch;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"

Write-Host "Starting BOS over HTTPS on port $Port (branch: $Branch, env: $Environment)..."
Start-Process dotnet -ArgumentList 'QuickSoft.dll' -WorkingDirectory $AppDir `
    -RedirectStandardOutput "$env:TEMP\bos-https-$Port.log" `
    -RedirectStandardError  "$env:TEMP\bos-https-$Port.err"

Write-Host "Done. Open:  https://localhost:$Port   and (from phones, after trusting the cert)  https://<lan-ip>:$Port"
