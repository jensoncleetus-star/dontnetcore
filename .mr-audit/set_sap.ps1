# GetAllSaleprofit — re-probe with the REAL view param matrix (SalesProfit.cshtml searchData),
# wide date range to rule out the today-only default. NoDT: the screen is serverSide:false.
$wide=@{ seno=''; perhourcost='40'; paymethod=''; completed=''; SalesMan=''; customer='';
        SalesExecutive=''; fromdate='01-01-2015'; todates='31-12-2026'; type=''; ddMC='0';
        satype=''; htype=''; hfdate=''; htdate=''; project='0'; task='0'; srchtxt='';
        employeehourrate='true'; source=''; location=''; technician=''; sourcelead='';
        cached='false'; cusage='0'; nostocktransfer='false' }
Probe 'GetAllSaleprofit ddMC=0 wide'        'SalesReport/GetAllSaleprofit'        $wide -NoDT
# same, but todates as a single recent day (the UI default is today-today) to compare
$today=$wide.Clone(); $today['fromdate']='01-01-2024'; $today['todates']='31-12-2024'
Probe 'GetAllSaleprofit ddMC=0 2024'        'SalesReport/GetAllSaleprofit'        $today -NoDT
# sibling control that runs ungated (should always return data) for comparison
Probe 'GetAllSaleprofitsummery ddMC=0 wide' 'SalesReport/GetAllSaleprofitsummery' $wide -NoDT
