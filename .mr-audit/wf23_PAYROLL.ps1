# PayrollVoucher
Probe 'PV_GetPayrollVoucher' 'Hr/PayrollVoucher/GetPayrollVoucher' @{}

# PayrollUnit
Probe 'PU_GetPayrollUnit' 'Hr/PayrollUnit/GetPayrollUnit' @{}

# Payhead
Probe 'PH_GetPayhead' 'Hr/Payhead/GetPayhead' @{}

# SalaryStructure
Probe 'SS_GetSalaryStructure' 'Hr/SalaryStructure/GetSalaryStructure' @{}

# PayrollReport grid/json endpoints
Probe 'PR_GetPaySheet'        'Hr/PayrollReport/GetPaySheet'        @{ empl='0'; monthyear='06-2026' }
Probe 'PR_GetPayrollRegister' 'Hr/PayrollReport/GetPayrollRegister' @{}
Probe 'PR_getPayrollDetails'  'Hr/PayrollReport/getPayrollDetails'  @{ month='6' }
Probe 'PR_GetFinalSettlement' 'Hr/PayrollReport/GetFinalSettlement' @{ empl='0'; fromdate=''; todate='' }
Probe 'PR_GetLeaveSettlement' 'Hr/PayrollReport/GetLeaveSettlement' @{ empl='0'; fromdate=''; todate='' }
