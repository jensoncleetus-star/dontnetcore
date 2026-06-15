$fd='01-01-2010'; $td='31-12-2026'
Probe 'GetAllSaleRebateCustomerWise'    'SalesReport/GetAllSaleRebateCustomerWise'    @{srchtxt='';brand='0';category='0';item='0';seno='';paymethod='0';customer='0';SalesExecutive='0';SalesMan='0';fromdate=$fd;todate=$td;type='0';ddMC='0';satype='';htype='0';hfdate='';htdate='';project='0';task='0';custyp='0'}
Probe 'GetAllSaleRebateCustomerWisebonus' 'SalesReport/GetAllSaleRebateCustomerWisebonus' @{srchtxt='';brand='0';category='0';item='0';seno='';paymethod='0';customer='0';SalesExecutive='0';SalesMan='0';fromdate=$fd;todate=$td;type='0';ddMC='0';satype='';htype='0';hfdate='';htdate='';project='0';task='0';custyp='0'}
Probe 'GetAllSaleprofitCASA'   'SalesReport/GetAllSaleprofitCASA'   @{seno='';paymethod='0';customer='0';SalesExecutive='0';fromdate=$fd;todate=$td;type='0';ddMC='0';satype='';htype='0';hfdate='';htdate='';project='0';task='0'}
Probe 'MonthWise'             'SalesReport/MonthWise'             @{Year='2024'}
Probe 'GetDayWise'           'SalesReport/GetDayWise'           @{From='01-01-2024';To='31-12-2024';ddmc='0';emp='0';task='0';project='0';customer='0';hfrom='';hto='';htype='0';stype=''}
