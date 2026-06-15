# Batch 23: Hr-area SETTLEMENT/MISC grid endpoints
# FinalSettlement / LeaveSettlement / EmployeeGrade
# All three are POST DataTables grids; harness auto-merges draw/start/length/order/columns.
# FinalSettlement & LeaveSettlement declare unused filter params (BName/Item/Qty/Unit) -> send '0'/'' to bind.
# EmployeeGrade takes no params.

# --- FinalSettlementController ---
Probe 'FS_GetFinalSettlement'   'Hr/FinalSettlement/GetFinalSettlement'   @{ BName='0'; Item='0'; Qty='0'; Unit='0' }

# --- LeaveSettlementController ---
Probe 'LS_GetLeaveSettlement'   'Hr/LeaveSettlement/GetLeaveSettlement'   @{ BName='0'; Item='0'; Qty='0'; Unit='0' }

# --- EmployeeGradeController ---
Probe 'EG_GetEmployeeGrade'     'Hr/EmployeeGrade/GetEmployeeGrade'       @{ }
