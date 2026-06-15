# PurchaseReport data endpoints — port-break sweep.
$fd='01-01-2010'; $td='31-12-2026'
Probe 'GetAllPurchaseReturn'    'PurchaseReport/GetAllPurchaseReturn'    @{srchtxt='';peno='';supplier='0';fromdate=$fd;SalesExecutive='0';todate=$td;type='0';ddlMC='0';Taxtype='0'}
Probe 'GetAllpurRebate'         'PurchaseReport/GetAllpurRebate'         @{peno='';supplier='0';fromdate=$fd;SalesExecutive='0';todate=$td;type='0';ddlMC='0';Taxtype='0';srchtxt='';brand='0';category='0';item='0'}
Probe 'GetAllpurRebateSupplierWise' 'PurchaseReport/GetAllpurRebateSupplierWise' @{peno='';supplier='0';fromdate=$fd;SalesExecutive='0';todate=$td;type='0';ddlMC='0';Taxtype='0';srchtxt='';brand='0';category='0';item='0'}
Probe 'GetAllpur'               'PurchaseReport/GetAllpur'               @{Task='0';project='0';peno='';supplier='0';fromdate=$fd;SalesExecutive='0';todate=$td;type='0';ddlMC='0';Taxtype='0';srchtxt='';brand='0';category='0';item='0'}
Probe 'GetAllPurchase'          'PurchaseReport/GetAllPurchase'          @{srchtxt='';supplier='0';fromdate=$fd;todate=$td;comparprices='10';brand='0';category='0'}
Probe 'GetSupplierWise'         'PurchaseReport/GetSupplierWise'         @{Supplier='0';fromdate=$fd;todate=$td;ddmc='0'}
Probe 'GetItemWise'             'PurchaseReport/GetItemWise'             @{item='';fromdate=$fd;todate=$td;ddlMC='0';Salety='';Brand='0';SalesExecutive='0';Category='0';Salesman='0'}
Probe 'GetCategoryWise'         'PurchaseReport/GetCategoryWise'         @{category='0';fromdate=$fd;todate=$td;ddlMC='0'}
Probe 'GetBrandWise'            'PurchaseReport/GetBrandWise'            @{brand='0';fromdate=$fd;todate=$td;ddlMC='0'}
Probe 'GetpursExeWise'          'PurchaseReport/GetpursExeWise'          @{pursexec='0';fromdate=$fd;todate=$td;ddmc='0'}
Probe 'getitembrand'           'PurchaseReport/getitembrand'           @{brand='0';fromdate=$fd;to=$td;ddmc='0'}
Probe 'getitemDetails'          'PurchaseReport/getitemDetails'          @{item='0';supplier='0';fromdate=$fd;to=$td;ddmc='0'}
Probe 'getsupDetails'           'PurchaseReport/getsupDetails'           @{sups='0';fromdate=$fd;to=$td;ddmc='0'}
Probe 'GetSupplierItemWise'     'PurchaseReport/GetSupplierItemWise'     @{ddlSupplier='0';ddlItem='0';From=$fd;To=$td;ddmc='0'}
Probe 'GetMonthlyPurchase'      'PurchaseReport/GetMonthlyPurchase'      @{fromdate=$fd;todate=$td;ddmc='0'}
Probe 'GetDayWise'              'PurchaseReport/GetDayWise'              @{From=$fd;To=$td;ddmc='0';emp='0';supplier='0';hfrom='';hto='';htype='0';ptype=''}
Probe 'GetCashOrCredit'         'PurchaseReport/GetCashOrCredit'         @{From=$fd;To=$td;ddmc='0'}
Probe 'getInvoiceWise'          'PurchaseReport/getInvoiceWise'          @{item='0';supplier='0';fromdate=$fd;to=$td;ddmc='0';project='0';task='0'}
Probe 'GetSuppllierItemWise'    'PurchaseReport/GetSuppllierItemWise'    @{Supp='0';Item='0';fromdate=$fd;todate=$td;MC='0'}
