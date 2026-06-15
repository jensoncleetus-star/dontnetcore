# Batch 25 verification (defensive fixes + Inventory projection)
Probe 'Inventory.getforcast item1'   'Inventory/getforcast'  @{ itemid='1'; ddmc1='20084' }
Probe 'Inventory.getforcast item0'   'Inventory/getforcast'  @{ itemid='0' }
# DailyAttendance GET overloads (Emp not found / empty AtDate were 500)
Probe 'DA.GetDaily emp-missing'      'Hr/DailyAttendance/GetDailyAttendance'    @{ Emp='999999999'; MonthYear='06-2026' } 'GET'
Probe 'DA.GetDaily emp-ok'           'Hr/DailyAttendance/GetDailyAttendance'    @{ Emp='1'; MonthYear='06-2026' } 'GET'
Probe 'DA.GetDept atdate-empty'      'Hr/DailyAttendance/GetDeptDailyAttendance' @{ Dept='0'; AtDate='' } 'GET'
Probe 'DA.GetDept atdate-ok'         'Hr/DailyAttendance/GetDeptDailyAttendance' @{ Dept='0'; AtDate='01-06-2026' } 'GET'
# SalesReport GetAllSaleprofitdepartment: SalesExecutive omitted (null) was 500; '0' control was 200
Probe 'SR.deptprofit no-exec'        'SalesReport/GetAllSaleprofitdepartment'   @{ seno=''; fromdate=''; todate='' }
Probe 'SR.deptprofit exec0'          'SalesReport/GetAllSaleprofitdepartment'   @{ seno=''; SalesExecutive='0'; fromdate=''; todate='' }
# SalaryStructure / PayrollVoucher empty-date guards
Probe 'SS.GetSalaryStrDetails empty' 'Hr/SalaryStructure/GetSalaryStrDetails'   @{ EmpID='1'; FDate=''; TDate='' }
Probe 'PV.GetAutoFill empty'         'Hr/PayrollVoucher/GetAutoFillVoucherDetail' @{ ProcessFor=''; FromDate=''; ToDate=''; SelEmp=''; Acc='0' } 'GET'
