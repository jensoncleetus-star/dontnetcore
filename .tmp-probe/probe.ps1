param([string]$ListFile,[string]$Base="http://localhost:8080")
$ErrorActionPreference="SilentlyContinue"
$b=$Base
$lp=Invoke-WebRequest "$b/Users/Login" -UseBasicParsing -SessionVariable s
$tok=([regex]::Match($lp.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')).Groups[1].Value
Invoke-WebRequest "$b/login" -Method POST -Body @{Email="jenson@gmail.com";Password="Test1234";RememberMe="false";fromapp="";__RequestVerificationToken=$tok} -WebSession $s -UseBasicParsing -MaximumRedirection 0 | Out-Null
$pg=Invoke-WebRequest "$b/ItemColor/Index" -WebSession $s -UseBasicParsing
$atok=([regex]::Match($pg.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')).Groups[1].Value
function DtBody(){
  return [ordered]@{
    "draw"="1";"start"="0";"length"="25";
    "search[value]"="";"search[regex]"="false";
    "order[0][column]"="0";"order[0][dir]"="asc";
    "columns[0][data]"="0";"columns[0][name]"="";"columns[0][searchable]"="true";"columns[0][orderable]"="true";"columns[0][search][value]"="";"columns[0][search][regex]"="false";
    "__RequestVerificationToken"=$atok
  }
}
function Probe($name,$url,$extra,$withDt){
  if($withDt -eq "1"){ $body=DtBody } else { $body=[ordered]@{ "__RequestVerificationToken"=$atok } }
  foreach($k in $extra.Keys){ $body[$k]=$extra[$k] }
  try {
    $r=Invoke-WebRequest "$b$url" -Method POST -Body $body -WebSession $s -UseBasicParsing -TimeoutSec 300
    $c=$r.Content
    $isJson = $c.TrimStart().StartsWith("{") -or $c.TrimStart().StartsWith("[")
    "RESULT|$name|$($r.StatusCode)|len=$($c.Length)|json=$isJson|head=$($c.Substring(0,[Math]::Min(180,$c.Length)) -replace '[\r\n]+',' ')"
  } catch {
    $code=$_.Exception.Response.StatusCode.value__
    "RESULT|$name|ERR$code|$($_.Exception.Message -replace '[\r\n]+',' ')"
  }
}
foreach($ln in (Get-Content $ListFile)){
  if([string]::IsNullOrWhiteSpace($ln)){continue}
  $parts=$ln.Split("|")
  $nm=$parts[0]; $u=$parts[1]; $ex=@{}; $dt=$parts[3]
  if($parts.Count -ge 3 -and $parts[2]){ foreach($kv in $parts[2].Split(";")){ if($kv){ $k,$v=$kv.Split("=",2); $ex[$k]=$v } } }
  Probe $nm $u $ex $dt
}
