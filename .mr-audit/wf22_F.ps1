# Family F (financial) — grid/data endpoint port-break sweep (batch 22).
$fd='01-01-2010'; $td='31-12-2026'
# BRS
Probe 'BRS-GetStatement'        'BRS/GetStatement'        @{Account='1';FromDate=$fd;ToDate=$td}
Probe 'BRS-GetBRS'              'BRS/GetBRS'              @{Account='1';FromDate=$fd;ToDate=$td;ShowType='1'}
# DayBook (role: Dev,DayBook)
Probe 'DayBook-GetData'         'DayBook/GetData'         @{fromdate=$fd;todate=$td}
# FinalAccounts
Probe 'FinalAccounts-GetCashBook' 'FinalAccounts/GetCashBook' @{AccId='1';fromdate=$fd;todate=$td;pdcinclude='false'}
Probe 'FinalAccounts-GetLedger'   'FinalAccounts/GetLedger'   @{AccId='1';fromdate=$fd;todate=$td;AccGroup='0';pdcinclude='false'}
# AccountSummary
Probe 'AccountSummary-GetBalances'    'AccountSummary/GetBalances'    @{AccId='1';fromdate=$fd;todate=$td;AccGroup='0';pdc='false'}
Probe 'AccountSummary-GetDaily'       'AccountSummary/GetDaily'       @{AccId='1';fromdate=$fd;todate=$td;AccGroup='0';pdc='false'}
Probe 'AccountSummary-GetMonthly'     'AccountSummary/GetMonthly'     @{AccId='1';fromdate=$fd;todate=$td;AccGroup='0';pdc='false'}
Probe 'AccountSummary-GetTransaction' 'AccountSummary/GetTransaction' @{fromdate=$fd;todate=$td;pdcinclude='false'}
# Registers
Probe 'Registers-GetReceipt'    'Registers/GetReceipt'    @{vno='';payfrom='0';payto='0';fromdate=$fd;todate=$td}
Probe 'Registers-GetPayment'    'Registers/GetPayment'    @{vno='';payfrom='0';payto='0';fromdate=$fd;todate=$td}
