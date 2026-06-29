# Watchdog: start the QuickNet pilot on :8080 if it isn't already listening.
# Run by the scheduled task "QuickNetPilot" every few minutes (and at logon) so the
# pilot survives reboots/sleep/crashes during UAT. No admin required (runs as the logged-in user).
$listening = Get-NetTCPConnection -LocalPort 8080 -State Listen -ErrorAction SilentlyContinue
if (-not $listening) {
    $dll = "C:\Quick Soft 10-06-2026\QuickNetCore\publish\QuickSoft.dll"
    if (Test-Path $dll) {
        Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" `
            -ArgumentList "`"$dll`" --urls http://0.0.0.0:8080 --environment Production --contentRoot `"C:\Quick Soft 10-06-2026\QuickNetCore\publish`"" `
            -WindowStyle Hidden
    }
}
