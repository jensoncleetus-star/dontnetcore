# Dump raw content of the two rec=0 grids to confirm clean empty JSON (not Access-Denied HTML)
function ProbeRaw {
  param([string]$name,[string]$path,[hashtable]$body)
  $b=$global:base; $s=$global:sess
  foreach($k in $global:DT.Keys){ if(-not $body.ContainsKey($k)){ $body[$k]=$global:DT[$k] } }
  $body['__RequestVerificationToken']=$global:atok
  $r=Invoke-WebRequest "$b/$path" -Method POST -Body $body -WebSession $s -UseBasicParsing -TimeoutSec 120
  "=== $name (code $($r.StatusCode)) ==="
  $r.Content
}
ProbeRaw 'FS' 'Hr/FinalSettlement/GetFinalSettlement' @{ BName='0'; Item='0'; Qty='0'; Unit='0' }
ProbeRaw 'EG' 'Hr/EmployeeGrade/GetEmployeeGrade'     @{ }
