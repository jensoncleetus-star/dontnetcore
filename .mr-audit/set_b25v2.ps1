# Batch 25 re-verify with correct params
Probe 'Inventory.getforcast'   'Inventory/getforcast' @{ period='1'; ddmc1='20084'; itemid='1'; currstock='0'; datefromforecaste='01-01-2024'; datetoforecaste='31-12-2026' }
# GetAllSaleprofitdepartment with hfdate/htdate OMITTED (the original failing case) + SalesExecutive omitted
Probe 'SR.deptprofit omit-all' 'SalesReport/GetAllSaleprofitdepartment' @{ seno=''; fromdate='01-01-2024'; todate='31-12-2026' }
Probe 'SR.deptprofit exec0'    'SalesReport/GetAllSaleprofitdepartment' @{ seno=''; SalesExecutive='0'; fromdate='01-01-2024'; todate='31-12-2026' }
