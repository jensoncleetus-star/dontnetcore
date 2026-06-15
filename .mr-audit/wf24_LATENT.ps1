# wf24 — latent-issue triage probes

# (1) DailyAttendance — empJoin NPE & GetDeptDailyAttendance DateTime.Parse
#     GetDailyAttendance is [HttpGet] (Emp, MonthYear)
Probe 'DA_GetDailyAttendance_ok'    'Hr/DailyAttendance/GetDailyAttendance'    @{ Emp='1'; MonthYear='06-2026' } GET
Probe 'DA_GetDailyAttendance_badEmp' 'Hr/DailyAttendance/GetDailyAttendance'   @{ Emp='999999999'; MonthYear='06-2026' } GET
#     GetDeptDailyAttendance is [HttpGet] (Dept, AtDate) — empty AtDate -> FormatException
Probe 'DA_GetDeptDaily_ok'          'Hr/DailyAttendance/GetDeptDailyAttendance' @{ Dept='1'; AtDate='13-06-2026' } GET
Probe 'DA_GetDeptDaily_emptyDate'   'Hr/DailyAttendance/GetDeptDailyAttendance' @{ Dept='1'; AtDate='' } GET

# (2) SalesReport — GetAllSaleprofitdepartment SalesExecutive.Split null NRE
Probe 'SR_GetAllSaleprofitdept_null' 'SalesReport/GetAllSaleprofitdepartment' @{ seno=''; paymethod='0'; customer='0'; fromdate=''; todate=''; type='0'; ddMC='0'; satype=''; htype='0'; hfdate=''; htdate=''; project='0'; task='0' }
Probe 'SR_GetAllSaleprofitdept_emp'  'SalesReport/GetAllSaleprofitdepartment' @{ seno=''; SalesExecutive='0'; paymethod='0'; customer='0'; fromdate=''; todate=''; type='0'; ddMC='0'; satype=''; htype='0'; hfdate=''; htdate=''; project='0'; task='0' }

# (3) PayrollReport — 5 non-GetPaySheet view methods (in-projection ToList().Count/Sum check)
#     PaySlip & PaymentAdvice are [HttpPost]; the other three are GET
Probe 'PR_PaySlip'             'Hr/PayrollReport/PaySlip'             @{ ddlEmployee='1'; MonthYear='06-2026' }
Probe 'PR_GetPayrollStatement' 'Hr/PayrollReport/GetPayrollStatement' @{ ddlEmployee='0'; MonthYear='06-2026' } GET
Probe 'PR_PaymentAdvice'       'Hr/PayrollReport/PaymentAdvice'       @{ ddlEmployee='1'; MonthYear='06-2026' }
Probe 'PR_GetEmpPayHeadBreakup' 'Hr/PayrollReport/GetEmpPayHeadBreakup' @{ ddlEmployee='0'; ddlPayHead='0'; MonthYear='06-2026' } GET
Probe 'PR_GetPayHeadEmpBreakup' 'Hr/PayrollReport/GetPayHeadEmpBreakup' @{ ddlEmployee='0'; MonthYear='06-2026' } GET

# (4) CalendarTemplate — GetMonthlyLeave db.Holidays...First() Sequence-contains-no-elements
Probe 'CT_GetMonthlyLeave' 'Hr/CalendarTemplate/GetMonthlyLeave' @{ start='2026-06-01'; end='2026-06-30'; EntryID='1' }
