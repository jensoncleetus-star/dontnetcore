# wf24 Hr re-probe with valid dates to drive past input-parsing to the .ToList().Sum sites
Probe 'HR_GetSalaryStrDetails_dt'      'Hr/SalaryStructure/GetSalaryStrDetails'   @{ EmpID='1'; FDate='01-06-2026'; TDate='30-06-2026' }
Probe 'HR_GetAutoFillVoucherDetail_dt' 'Hr/PayrollVoucher/GetAutoFillVoucherDetail' @{ ProcessFor='Salary'; FromDate='01-06-2026'; ToDate='30-06-2026'; SelEmp='1'; Acc='1' } GET
