param([string]$target = ".sweep-all.txt", [string]$resultsFile = ".sweep-all-results.txt", [string]$user = "sulaiman")
Set-Location "C:\Quick Soft 10-06-2026\QuickNetCore"
$ErrorActionPreference = "SilentlyContinue"
$base = "http://localhost:5099"
foreach($i in 1..60){ try{ if((Invoke-WebRequest "$base/Users/Login" -UseBasicParsing -TimeoutSec 3).StatusCode -eq 200){break} }catch{ Start-Sleep -Milliseconds 800 } }
$login=Invoke-WebRequest "$base/Users/Login" -UseBasicParsing -SessionVariable sess
$tok=([regex]::Match($login.Content,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')).Groups[1].Value
Invoke-WebRequest "$base/login" -Method POST -Body @{Email=$user;Password='Test1234';RememberMe='false';fromapp='';__RequestVerificationToken=$tok} -WebSession $sess -UseBasicParsing -MaximumRedirection 0 | Out-Null

# DataTables-safe params (search[value] present; columns[0][name]='' skips the dynamic sort). Wide dates so date-filtered
# reports return data instead of empty/parse-erroring. Filter superset, case-insensitive-unique keys.
$common = @{
  draw='1'; start='0'; length='10'; 'search[value]'=''; 'search[regex]'='false'
  'order[0][column]'='0'; 'order[0][dir]'=''; 'columns[0][name]'=''
  flag='1'; assignedto=''; AssTo='0'; customer='0'; supplier='0'
  ContractLevelId=''; taskstat='0'; taskstatus='0'; protaskstatus='0'; location=''; ContractName='0'; ContractType='0'
  opncls=''; ddlbind=''; fromdate='01-01-2015'; todate='31-12-2026'; StartDate='01-01-2015'; EndDate='31-12-2026'
  EFromDate='01-01-2015'; EndDateto='31-12-2026'; RFromDate='01-01-2015'; RToDate='31-12-2026'; nextdate=''; PDate=''; remdate=''
  ContractNo=''; priority='0'; taskname='0'; tasktype='0'; projects='0'; empl='0'; VType='0'; Vmanu='0'; Vmod='0'
  Mobile='All'; LastUpdDays='0'; LeadCode=''; LeadName='0'; LeadSource='0'; CreatedBy='0'; LeadStatus=''; LeadCustomer='0'
  LeadLevel='All'; LeadId='0'; ref1=''; ref2=''; ref3=''; ref4=''; ref5=''; remstatus='0'; local='0'; txtremarks=''; mc='0'
  documenttype='0'; type=''; employeename='0'; sourcename='0'; AmcId=''; apfrom=''; apto=''; status='0'; Project='0'; ProjectId='0'
  Invoice=''; InvoiceNo=''; BillNo=''; Item='0'; ItemId='0'; Warehouse='0'; MaterialCenter='0'; mcId='0'; Account='0'
  EmployeeId='0'; employee='0'; year='2024'; month='1'; date=''; id='0'; section='General'; BName='0'; Qty='0'; Unit='0'
  vouchertype='0'; paymethod='0'; Balance=''; salesstatus=''; appstat=''; PurchaseType='0'; PurchaseStatus=''
  RefenceNo=''; customername=''; salesperson='0'; user='0'; ProjectName='0'; servicetype='0'; sourceoflead='0'; Stats='0'
  Validity=''; Saletype='0'; HireType='0'; QuotationType='0'; Task='0'; generated='0'; BOM='0'; BomID='0'; BoqId='0'; AmcNo='0'
}
$grids = Get-Content $target | Where-Object { $_.Trim() -ne '' } | Sort-Object -Unique
"SWEEP-ALL START user=$user count=$($grids.Count)" | Set-Content $resultsFile
$n=0
foreach($g in $grids){
  $n++
  $url = "$base$($g.Trim())"
  $code=0; $bd=''
  try { $r = Invoke-WebRequest $url -Method POST -Body $common -WebSession $sess -UseBasicParsing -TimeoutSec 25 -MaximumRedirection 0; $code=$r.StatusCode; $bd=$r.Content }
  catch {
    if($_.Exception.Response){ $code=[int]$_.Exception.Response.StatusCode; try{ $bd=(New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())).ReadToEnd() }catch{} }
    elseif($_.Exception.Message -match 'timeout|timed out'){ $code=-2 } else { $code=-1; $bd=$_.Exception.Message }
  }
  $cls='OK'; $detail=''
  if($code -eq 200){
    if($bd -match '"?recordsTotal"?["\s:]+(\d+)'){ $cls='OK'; $detail="total=$($Matches[1])" }
    elseif($bd -match '"data"\s*:|"bom"\s*:|"item"\s*:|^\s*\[|"aaData"'){ $cls='OK'; $detail='json' }
    elseif($bd -match '<!DOCTYPE|<html|<title'){ $cls='NONDATA'; $detail="html$($bd.Length)" }
    else { $cls='OK?'; $detail="200,len=$($bd.Length)" }
  } elseif($code -eq -2){ $cls='TIMEOUT' }
  elseif($code -eq 302){ $cls='REDIRECT' }
  elseif($code -ge 400 -or $code -lt 0){
    if($bd -match 'property name for .* collides'){ $cls='JSONCOLLIDE'; $m=[regex]::Match($bd,"property name for '([^']{0,50})"); $detail=$m.Groups[1].Value }
    elseif($bd -match 'Invalid column name'){ $cls='SCHEMA'; $m=[regex]::Match($bd,"Invalid column name '([^']{0,40})"); $detail=$m.Groups[1].Value }
    elseif($bd -match 'could not be translated|ProjectionBindingExpression|Expression of type'){ $cls='TRANSLATE'; $m=[regex]::Match($bd,"(ProjectionBindingExpression: \d+|Expression of type '[^']{0,50})"); $detail=$m.Groups[1].Value }
    elseif($bd -match 'Nullable object must have a value'){ $cls='NULLABLE' }
    elseif($bd -match 'Incorrect syntax near|OPENJSON'){ $cls='SQLSYNTAX' }
    elseif($bd -match 'NullReferenceException'){ $cls='NRE'; $m=[regex]::Match($bd,'at QuickSoft[^\n]{0,60}:line (\d+)'); $detail=$m.Value }
    elseif($bd -match 'ArgumentNullException'){ $cls='ARGNULL' }
    elseif($bd -match 'FormatException|String was not recognized|Convert'){ $cls='FORMAT' }
    elseif($bd -match '(System\.[A-Za-z.]+Exception)'){ $cls='OTHER'; $detail=$Matches[1]; $l=[regex]::Match($bd,'([A-Za-z]+Controller)\.cs:line (\d+)'); if($l.Success){ $detail += " @$($l.Groups[1].Value):$($l.Groups[2].Value)" } }
    else { $cls="HTTP$code" }
  }
  "{0,-3} {1,-11} {2,-46} {3}" -f $n,$cls,$g.Trim(),$detail | Add-Content $resultsFile
}
"SWEEP-ALL DONE" | Add-Content $resultsFile
