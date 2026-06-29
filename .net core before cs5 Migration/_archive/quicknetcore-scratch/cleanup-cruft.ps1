# One-time cleanup of development cruft from the QuickNetCore folder.
# Review then run:  .\cleanup-cruft.ps1        (deletes only backups + test scratch, never source)
$p = $PSScriptRoot

# 1) Duplicate/backup JS files + the stray "js - Copy" folder (real app files are untouched).
Remove-Item "$p\wwwroot\Content\js\assetfrominventory - Copy.js",
            "$p\wwwroot\Content\js\saleinvoice - Copy.js",
            "$p\wwwroot\Content\js\saleinvoiceold.js",
            "$p\wwwroot\Content\js\salesinvoice2 - Copy.js",
            "$p\wwwroot\Content\js\salesinvoice2(orginal).js",
            "$p\wwwroot\Content\js\_salesinvoice2.js" -Force -ErrorAction SilentlyContinue
Remove-Item "$p\wwwroot\Content\js - Copy" -Recurse -Force -ErrorAction SilentlyContinue

# 2) Test/scratch artifacts from the migration sessions (leading-dot files at the repo root).
Get-ChildItem $p -File -Force |
    Where-Object { $_.Name -like '.*' -and $_.Extension -in '.txt','.csv','.json','.err','.out' } |
    Remove-Item -Force -ErrorAction SilentlyContinue

# 3) Scratch scripts no longer needed.
Remove-Item "$p\sweep.ps1","$p\ensure-pilot.ps1" -Force -ErrorAction SilentlyContinue

Write-Host "cleanup done."
