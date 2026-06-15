Probe 'DN ddlMC=0'   'MyReports/GetDeliverynote' @{quno='';customer='0';fromdate='01-01-2010';todate='31-12-2026';ddlMC='0';SType='0';HireType='0';HdateFrom='';HdateTo='';project='0';task='0'}
Probe 'DN ddlMC=1'   'MyReports/GetDeliverynote' @{quno='';customer='0';fromdate='01-01-2010';todate='31-12-2026';ddlMC='1';SType='0';HireType='0';HdateFrom='';HdateTo='';project='0';task='0'}
Probe 'DN no ddlMC'  'MyReports/GetDeliverynote' @{quno='';customer='0';fromdate='01-01-2010';todate='31-12-2026';SType='0';HireType='0';HdateFrom='';HdateTo='';project='0';task='0'}
# dump raw
$body=@{quno='';customer='0';fromdate='01-01-2010';todate='31-12-2026';ddlMC='1';SType='0';HireType='0';HdateFrom='';HdateTo='';project='0';task='0'}
foreach($k in $global:DT.Keys){ $body[$k]=$global:DT[$k] }; $body['__RequestVerificationToken']=$global:atok
$r=Invoke-WebRequest "$($global:base)/MyReports/GetDeliverynote" -Method POST -Body $body -WebSession $global:sess -UseBasicParsing -TimeoutSec 120
"RAW(ddlMC=1) first 300: "+$r.Content.Substring(0,[Math]::Min(300,$r.Content.Length))
