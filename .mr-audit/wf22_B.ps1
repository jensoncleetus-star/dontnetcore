# Family B: balancesheet/recon
# Dates dd-MM-yyyy. Broad range to capture data.
$fd = '01-01-2020'
$td = '13-06-2026'

# --- BalanceSheetController ---
Probe 'BS_GetBalanceSheet'            'BalanceSheet/GetBalanceSheet'            @{ fromdate=$fd; todate=$td }
Probe 'BS_GetBalanceSheetqucknet'     'BalanceSheet/GetBalanceSheetqucknet'     @{ fromdate=$fd; todate=$td }
Probe 'BS_GetBalanceSheetemirtech'    'BalanceSheet/GetBalanceSheetemirtech'    @{ fromdate=$fd; todate=$td }
Probe 'BS_Getcashflowdetailed'        'BalanceSheet/Getcashflowdetailed'        @{ selcompany=''; ddlMC='0'; fromdate=$fd; todate=$td }
Probe 'BS_Getcashflow'                'BalanceSheet/Getcashflow'                @{ selcompany=''; ddlMC='0'; fromdate=$fd; todate=$td }
Probe 'BS_GetProfitAndLoss'           'BalanceSheet/GetProfitAndLoss'           @{ selcompany=''; ddlMC='0'; fromdate=$fd; todate=$td }
Probe 'BS_GetProfitAndLossExpense'    'BalanceSheet/GetProfitAndLossExpense'    @{ fromdate=$fd; todate=$td; accgroup='0'; excluderoudoff='true' }
Probe 'BS_GetProfitAndLossShowroom'   'BalanceSheet/GetProfitAndLossShowroom'   @{ selcompany=''; ddlMC='0'; fromdate=$fd; todate=$td }
Probe 'BS_GetProfitAndLossMC'         'BalanceSheet/GetProfitAndLossMC'         @{ selcompany=''; ddlMC='0'; fromdate=$fd; todate=$td }
Probe 'BS_GroupTB_bscashflow'         'BalanceSheet/GetGroupTrialBalancebalancesheetcashflow' @{ AccGroup='0'; type2=''; frmdate=$fd; todate=$td; parent=''; pdc='false' }
Probe 'BS_GroupTB_balancesheet'       'BalanceSheet/GetGroupTrialBalancebalancesheet'         @{ AccGroup='0'; type2=''; frmdate=$fd; todate=$td; parent=''; pdc='false' }
Probe 'BS_GroupTB'                     'BalanceSheet/GetGroupTrialBalance'        @{ AccGroup='0'; type2=''; frmdate=$fd; todate=$td; parent=''; pdc='false' }
Probe 'BS_GroupTB_trial'              'BalanceSheet/GetGroupTrialBalancetrial'   @{ AccGroup='0'; type2=''; frmdate=$fd; todate=$td; parent=''; pdc='false' }
Probe 'BS_GroupTB3'                   'BalanceSheet/GetGroupTrialBalance3'       @{ AccGroup='0'; type2=''; frmdate=$fd; todate=$td; parent='' }
Probe 'BS_GetTrialBalance'           'BalanceSheet/GetTrialBalance'             @{ fromdate=$fd; todate=$td }
Probe 'BS_GetTrialBalancefortrial'   'BalanceSheet/GetTrialBalancefortrial'     @{ fromdate=$fd; todate=$td }
Probe 'BS_GetTrialBalanceAC'         'BalanceSheet/GetTrialBalanceAC'           @{ fromdate=$fd; todate=$td }
Probe 'BS_GetTrialBalanceAC2'        'BalanceSheet/GetTrialBalanceAC2'          @{ fromdate=$fd; todate=$td }

# --- AdditionalMCController ---
Probe 'AddMC_GetAddMc'               'AdditionalMC/GetAddMc'                    @{ }

# --- MCConversionController has no grid/data endpoint (Create/Createdamage are CRUD writes) ---
