# Family ops-complex (TAG O)
# CustomerMerge
Probe 'CustomerMerge-getmergecustomer' 'CustomerMerge/getmergecustomer' @{}
# PackingList
Probe 'PackingList-GetPackingList' 'PackingList/GetPackingList' @{InvoiceNo='';LPO='';FromDate='';ToDate='';Customer='0';user='';appstat=''}
Probe 'PackingList-GetAllStatusUpdation' 'PackingList/GetAllStatusUpdation' @{MCId='0'}
# WarrantyEntries
Probe 'WarrantyEntries-GetWarranty' 'WarrantyEntries/GetWarranty' @{BillNo='';FromDate='';ToDate='';customer='0';salesperson='0';openclose=''}
Probe 'WarrantyEntries-GetAllRemarksadded' 'WarrantyEntries/GetAllRemarksadded' @{RequisitionId='0'}
# VehicleUpdates
Probe 'VehicleUpdates-GetVehiclereadingsstatus' 'VehicleUpdates/GetVehiclereadingsstatus' @{emp='0';vehicle='0';usage='0';fromdate='';todate=''}
Probe 'VehicleUpdates-GetVehiclereadings' 'VehicleUpdates/GetVehiclereadings' @{taskname='0';leadname='0';emp='0';vehicle='0';usage='0';fromdate='';todate=''}
# operationprocedure
Probe 'operationprocedure-GetDetails' 'operationprocedure/GetDetails' @{AmcId='0';PDate=''}
Probe 'operationprocedure-GetmyDetails' 'operationprocedure/GetmyDetails' @{AmcId='0';PDate=''}
# taskcalendar
Probe 'taskcalendar-getalltime' 'taskcalendar/getalltime' @{empid='0';fromdate='';todate=''}
