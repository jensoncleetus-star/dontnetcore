# Workflow 23 — Hr-area ATTENDANCE controllers sweep
# Dates dd-MM-yyyy. Numeric "All" dropdowns post '0'; some bind null with ''.
$fd = '01-01-2020'
$td = '13-06-2026'

# --- AttendanceController ---
Probe 'AT_GetAttendance'              'Hr/Attendance/GetAttendance'                @{ BName='0'; Item='0'; Qty='0'; Unit='0' }

# --- AttendanceReportController ---
Probe 'ATR_GetAttendanceSheet'        'Hr/AttendanceReport/GetAttendanceSheet'     @{ Emp='0'; fromdate=$fd; todate=$td }
Probe 'ATR_GetAttendanceRegister'     'Hr/AttendanceReport/GetAttendanceRegister'  @{ }
Probe 'ATR_getAttendanceDetails'      'Hr/AttendanceReport/getAttendanceDetails'   @{ month='6' }

# --- AttendanceTypeController ---
Probe 'ATT_GetAttType'                'Hr/AttendanceType/GetAttType'               @{ }

# --- CalendarTemplateController ---
Probe 'CT_GetCalendarTemplate'        'Hr/CalendarTemplate/GetCalendarTemplate'    @{ }
Probe 'CT_GetMonthlyLeave'            'Hr/CalendarTemplate/GetMonthlyLeave'        @{ start='2026-06-01'; end='2026-06-30'; EntryID='1' }

# --- DailyAttendanceController ---
# parameterless POST DataTables grid
Probe 'DA_GetDailyAttendance_grid'    'Hr/DailyAttendance/GetDailyAttendance'      @{ }
# GET overload Emp/MonthYear (MonthYear like MM-yyyy -> prepended with 01-)
Probe 'DA_GetDailyAttendance_emp'     'Hr/DailyAttendance/GetDailyAttendance'      @{ Emp='1'; MonthYear='06-2026' } 'GET'
# GET Dept/AtDate
Probe 'DA_GetDeptDailyAttendance'     'Hr/DailyAttendance/GetDeptDailyAttendance'  @{ Dept='0'; AtDate='01-06-2026' } 'GET'

# --- WorkShiftController ---
Probe 'WS_GetWorkShift'               'Hr/WorkShift/GetWorkShift'                  @{ }

# --- HolidayController ---
Probe 'HOL_GetHolidayList'            'Hr/Holiday/GetHolidayList'                  @{ }
