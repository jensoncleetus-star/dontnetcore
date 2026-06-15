$fd='01-01-2010'; $td='31-12-2026'
Probe 'GetEmployeeDetails' 'MyReports/GetEmployeeDetails' @{Type='';EmployeeId='0';From=$fd;CutOff='0'}
Probe 'GetAllRemarksaccount' 'MyReports/GetAllRemarksaccount' @{CustomerId='1'}
Probe 'GetStockAdjItemWiseReport' 'MyReports/GetStockAdjItemWiseReport' @{Item='0';fromdate=$fd;todate=$td}
Probe 'GetApproval' 'MyReports/GetApproval' @{FromDate=$fd;ToDate=$td;invoice='';type='';appstat='0';sortfive='false'}
