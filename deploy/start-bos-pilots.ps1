# ============================================================================
# BOS — start the pilot instances (8080 + 8081) on ALL network interfaces so the
# app is reachable by LAN IP (http://<this-pc-ip>:8080). Idempotent: only starts
# a port if it is not already listening. Logs to deploy\pilot-autostart.log.
# Runs at logon (Startup-folder entry BOS-Pilots-Autostart.vbs) so the app is
# ALWAYS up after a reboot.
# ============================================================================
$dll = "C:\Quick Soft 10-06-2026\QuickNetCore\publish\QuickSoft.dll"
$cr  = "C:\Quick Soft 10-06-2026\QuickNetCore\publish"
$log = "C:\Quick Soft 10-06-2026\QuickNetCore\deploy\pilot-autostart.log"

foreach ($port in 8080, 8081) {
    $listening = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
    if (-not $listening) {
        $argline = '"' + $dll + '" --urls http://0.0.0.0:' + $port + ' --environment Production --contentRoot "' + $cr + '"'
        Start-Process -FilePath "dotnet" -ArgumentList $argline -WindowStyle Hidden
        ((Get-Date).ToString('yyyy-MM-dd HH:mm:ss') + '  started pilot on 0.0.0.0:' + $port) | Out-File -FilePath $log -Append -Encoding utf8
    }
    else {
        ((Get-Date).ToString('yyyy-MM-dd HH:mm:ss') + '  port ' + $port + ' already listening - skip') | Out-File -FilePath $log -Append -Encoding utf8
    }
}
