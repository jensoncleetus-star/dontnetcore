param([string]$b='http://localhost:8080',[string]$ProbesFile,[string]$Email='jenson@gmail.com',[string]$Password='Test1234')
$ErrorActionPreference='SilentlyContinue'
$lp=Invoke-WebRequest "$b/Users/Login" -UseBasicParsing -SessionVariable s
$tok=([regex]::Match($lp.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')).Groups[1].Value
Invoke-WebRequest "$b/login" -Method POST -Body @{Email=$Email;Password=$Password;RememberMe='false';fromapp='';__RequestVerificationToken=$tok} -WebSession $s -UseBasicParsing -MaximumRedirection 0 -EA SilentlyContinue | Out-Null
$pg=Invoke-WebRequest "$b/ItemColor/Index" -WebSession $s -UseBasicParsing
$global:atok=([regex]::Match($pg.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')).Groups[1].Value
$global:sess=$s
$global:base=$b
$global:DT=@{
  'draw'='1'; 'start'='0'; 'length'='100';
  'search[value]'=''; 'search[regex]'='false';
  'order[0][column]'='0'; 'order[0][dir]'='asc';
  'columns[0][name]'=''; 'columns[0][data]'='0';
  'columns[0][searchable]'='true'; 'columns[0][orderable]'='true';
  'columns[0][search][value]'=''; 'columns[0][search][regex]'='false';
}
function Probe {
  param([string]$name,[string]$path,[hashtable]$body,[string]$verb='POST',[switch]$NoDT)
  $b=$global:base; $s=$global:sess
  if($verb -eq 'POST'){
    if(-not $NoDT){ foreach($k in $global:DT.Keys){ if(-not $body.ContainsKey($k)){ $body[$k]=$global:DT[$k] } } }
    $body['__RequestVerificationToken']=$global:atok
  }
  try {
    if($verb -eq 'GET'){
      $qs = ($body.GetEnumerator() | ForEach-Object { "$($_.Key)=$([uri]::EscapeDataString([string]$_.Value))" }) -join '&'
      $url = if($qs){"$b/$path`?$qs"}else{"$b/$path"}
      $r=Invoke-WebRequest $url -WebSession $s -UseBasicParsing -TimeoutSec 300
    } else {
      $r=Invoke-WebRequest "$b/$path" -Method POST -Body $body -WebSession $s -UseBasicParsing -TimeoutSec 300
    }
    $c=$r.Content
    # detect server-side datatables JSON record count
    $rec=''
    $m=[regex]::Match($c,'"recordsTotal"\s*:\s*(\d+)'); if($m.Success){ $rec=" rec="+$m.Groups[1].Value }
    "{0,-44} {1} len={2}{3}" -f $name,[int]$r.StatusCode,$c.Length,$rec
  } catch {
    $resp=$_.Exception.Response
    $code = try { [int]$resp.StatusCode } catch { 'ERR' }
    $msg=''
    try { $st=$resp.GetResponseStream(); $sr=New-Object IO.StreamReader($st); $msg=$sr.ReadToEnd() } catch {}
    if(-not $msg){ $msg=$_.Exception.Message }
    $line = ($msg -split "`r?`n" | Select-String 'System\.\w+(\.\w+)*Exception|could not be translated|Invalid (object|column) name|NullReference|collides|8623|InvalidOperation|FormatException|cannot be tracked|QuickSoft\.|ParseExact|Sequence contains' | Select-Object -First 4) -join '  ||  '
    if(-not $line){ $line = ($msg.Substring(0,[Math]::Min(200,$msg.Length))) -replace '\s+',' ' }
    "{0,-44} {1} ERR :: {2}" -f $name,$code,$line
  }
}
. $ProbesFile
