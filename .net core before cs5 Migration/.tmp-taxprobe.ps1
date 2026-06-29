param([string]$b='http://localhost:8080')
$ErrorActionPreference='Stop'
$lp=Invoke-WebRequest "$b/Users/Login" -UseBasicParsing -SessionVariable s
$tok=([regex]::Match($lp.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')).Groups[1].Value
Invoke-WebRequest "$b/login" -Method POST -Body @{Email='jenson@gmail.com';Password='Test1234';RememberMe='false';fromapp='';__RequestVerificationToken=$tok} -WebSession $s -UseBasicParsing -MaximumRedirection 0 -EA SilentlyContinue | Out-Null
$pg=Invoke-WebRequest "$b/ItemColor/Index" -WebSession $s -UseBasicParsing
$atok=([regex]::Match($pg.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')).Groups[1].Value

function ProbeDT($action,$extra){
  $body=@{ draw='1'; start='0'; length='100'; 'order[0][column]'='0'; 'order[0][dir]'='asc'; 'search[value]'=''; '__RequestVerificationToken'=$atok }
  foreach($k in $extra.Keys){ $body[$k]=$extra[$k] }
  try {
    $r=Invoke-WebRequest "$b/TaxReports/$action" -Method POST -Body $body -WebSession $s -UseBasicParsing -TimeoutSec 300
    $len=$r.Content.Length
    $rec = ([regex]::Match($r.Content,'"recordsTotal":(\d+)')).Groups[1].Value
    $snip = ($r.Content.Substring(0,[Math]::Min(90,$len))) -replace '\s+',' '
    "{0}: {1} len={2} recs={3} :: {4}" -f $action,$r.StatusCode,$len,$rec,$snip
  } catch {
    $code=if($_.Exception.Response){$_.Exception.Response.StatusCode.value__}else{'noresp'}
    "{0}: HTTP-{1}" -f $action,$code
  }
}
# JSON-body endpoints (GetTax, GetUaeVat) - sent as application/json
function ProbeJson($action,$json){
  try {
    $r=Invoke-WebRequest "$b/TaxReports/$action" -Method POST -Body $json -ContentType 'application/json; charset=utf-8' -WebSession $s -UseBasicParsing -TimeoutSec 300
    $len=$r.Content.Length
    $snip = ($r.Content.Substring(0,[Math]::Min(160,$len))) -replace '\s+',' '
    "{0}: {1} len={2} :: {3}" -f $action,$r.StatusCode,$len,$snip
  } catch {
    $code=if($_.Exception.Response){$_.Exception.Response.StatusCode.value__}else{'noresp'}
    "{0}: HTTP-{1}" -f $action,$code
  }
}
# Form-POST view-returning endpoints (GetVat, ExpenseTaxReport, GetExpenseTaxReport)
function ProbeForm($action,$extra){
  $body=@{ '__RequestVerificationToken'=$atok }
  foreach($k in $extra.Keys){ $body[$k]=$extra[$k] }
  try {
    $r=Invoke-WebRequest "$b/TaxReports/$action" -Method POST -Body $body -WebSession $s -UseBasicParsing -TimeoutSec 300
    "{0}: {1} len={2}" -f $action,$r.StatusCode,$r.Content.Length
  } catch {
    $code=if($_.Exception.Response){$_.Exception.Response.StatusCode.value__}else{'noresp'}
    "{0}: HTTP-{1}" -f $action,$code
  }
}
function ProbeGet($action,$qs){
  try {
    $r=Invoke-WebRequest "$b/TaxReports/$action`?$qs" -WebSession $s -UseBasicParsing -TimeoutSec 300
    "{0}: {1} len={2}" -f $action,$r.StatusCode,$r.Content.Length
  } catch {
    $code=if($_.Exception.Response){$_.Exception.Response.StatusCode.value__}else{'noresp'}
    "{0}: HTTP-{1}" -f $action,$code
  }
}

$RANGE=$env:RANGE
if(-not $RANGE){$RANGE='2024'}
if($RANGE -eq 'full'){ $fd='01-01-2010'; $td='31-12-2026' } else { $fd='01-01-2024'; $td='31-12-2024' }
"### RANGE=$RANGE ($fd .. $td) on $b ###"

$dt=@{ vno=''; fromdate=$fd; todate=$td; emirates=''; type=''; customer='0'; supplier='0'; satype=''; expacc='0' }
ProbeDT 'GetSaleTax' $dt
ProbeDT 'GetSaleTax2' $dt
ProbeDT 'GetPurchaseTax' $dt
ProbeDT 'GetPurchaseReturnTax' $dt
ProbeDT 'GetSalesReturnTax' $dt
ProbeDT 'GetExpenseTax' $dt
ProbeJson 'GetTax' (@{fromdate=$fd;todate=$td;emirates=''}|ConvertTo-Json -Compress)
ProbeJson 'GetUaeVat' (@{fromdate=$fd;todate=$td}|ConvertTo-Json -Compress)
ProbeForm 'GetVat' @{fromdate=$fd;todate=$td}
ProbeForm 'GetExpenseTaxReport' @{fromdate=$fd;todate=$td;draw='1';start='0';length='100'}
ProbeGet 'GetDetailsVat' "taxtype=out&fromdate=$fd&todate=$td"
ProbeGet 'GetDetailsVat' "taxtype=in&fromdate=$fd&todate=$td"
ProbeGet 'GetNewDetailsVat' "taxtype=in&fromdate=$fd&todate=$td"
