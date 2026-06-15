# wf24: triage UNGUARDED .ToList().Sum sites
# MyReports
Probe 'MR_GetPOrderOutstanding' 'MyReports/GetPOrderOutstanding' @{ item='0'; supp='0'; fromdate=''; todate='' }
Probe 'MR_GetSOrderOutstanding' 'MyReports/GetSOrderOutstanding' @{ item='0'; cust='0'; fromdate=''; todate='' }
Probe 'MR_GetProfitability'     'MyReports/GetProfitability'     @{ customer='0'; project='0'; fromdate=''; todate='' }
Probe 'MR_GetPLSummary'         'MyReports/GetPLSummary'         @{ customer='0'; fromdate=''; todate='' }

# Hr (GET actions -> append GET)
Probe 'HR_GetSalaryStrDetails'      'Hr/SalaryStructure/GetSalaryStrDetails'   @{ EmpID='0'; FDate=''; TDate='' }
Probe 'HR_GetAutoFillVoucherDetail' 'Hr/PayrollVoucher/GetAutoFillVoucherDetail' @{ ProcessFor='0'; FromDate=''; ToDate=''; SelEmp='0'; Acc='0' } GET
