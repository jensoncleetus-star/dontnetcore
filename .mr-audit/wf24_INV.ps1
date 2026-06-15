# Discover a valid item id + MC id, then probe getforcast (the in-projection .ToList().Sum bug at lines 1460/1471)

# 1) discover an item id from the item dropdown JSON
try {
  $r = Invoke-WebRequest "$global:base/Inventory/getitems" -WebSession $global:sess -UseBasicParsing -TimeoutSec 120
  $ids = [regex]::Matches($r.Content,'"ItemID"\s*:\s*(\d+)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -First 5
  "DISCOVER items: " + ($ids -join ',')
} catch { "DISCOVER items ERR: " + $_.Exception.Message }

# 2) discover MC ids
try {
  $r2 = Invoke-WebRequest "$global:base/MC/GetMC" -WebSession $global:sess -UseBasicParsing -TimeoutSec 120
  $mcs = [regex]::Matches($r2.Content,'"MCId"\s*:\s*(\d+)') | ForEach-Object { $_.Groups[1].Value } | Select-Object -First 5
  "DISCOVER mcs: " + ($mcs -join ',')
} catch { "DISCOVER mcs ERR: " + $_.Exception.Message }

# 3) getforcast - empty/zero (still forces EF translation of the let-subqueries)
Probe 'INV_getforcast_zero'  'Inventory/getforcast' @{ period='0'; ddmc1='0'; itemid='0'; currstock='0'; datefromforecaste='01-01-2025'; datetoforecaste='13-06-2026' }

# 4) getforcast - realistic single item + MC (fill ITEMID/MCID below after discovery if needed)
Probe 'INV_getforcast_item'  'Inventory/getforcast' @{ period='30'; ddmc1='20084'; itemid='1'; currstock='0'; datefromforecaste='01-01-2025'; datetoforecaste='13-06-2026' }
