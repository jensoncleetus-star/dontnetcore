# ShowRoomItemForecast/GetStock2 — branch-name resolution check (post nameOrConnectionString fix).
$base=@{ srchtxt=''; zerostockitem='true'; itemid=''; categories=''; brandId=''; supplier='';
        datefrom='01-01-2024'; dateto='31-12-2026' }
# company=0 -> branch "abudhabi" (configured -> emirtechlatest on this pilot): expect 200 + data
Probe 'GetStock2 company=0 (abudhabi->emir)' 'ShowRoomItemForecast/GetStock2' ($base + @{ company='0' })
# company=1 -> branch "musafa" (NOT configured): expect clean "Cannot open database", NOT format-crash
Probe 'GetStock2 company=1 (musafa,absent)'  'ShowRoomItemForecast/GetStock2' ($base + @{ company='1' })
