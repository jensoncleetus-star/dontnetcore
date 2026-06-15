# SalesReturnReport / PurchaseReturnReport / CustomerReport / HireStockReport — sweep.
$fd='01-01-2010'; $td='31-12-2026'
# --- SalesReturnReport ---
Probe 'SR-GetAllSaleprofit'  'SalesReturnReport/GetAllSaleprofit'  @{seno='';paymethod='0';customer='0';SalesExecutive='0';fromdate=$fd;todate=$td;type='0';ddMC='0';satype='';htype='0';hfdate='';htdate='';project='0';task='0';srchtxt=''}
Probe 'SR-GetAllSale'        'SalesReturnReport/GetAllSale'        @{srchtxt='';brand='0';category='0';item='0';seno='';paymethod='0';customer='0';SalesExecutive='0';SalesMan='0';fromdate=$fd;todate=$td;type='0';ddMC='0';satype='';htype='0';hfdate='';htdate='';project='0';task='0'}
Probe 'SR-GetItemWise'       'SalesReturnReport/GetItemWise'       @{item='0';fromdate=$fd;todate=$td;ddlMC='0'}
Probe 'SR-GetCustomerItemWise' 'SalesReturnReport/GetCustomerItemWise' @{ddlCustomer='0';ddlItem='0';From=$fd;To=$td;ddmc='0'}
Probe 'SR-GetCustomerWise'   'SalesReturnReport/GetCustomerWise'   @{customer='0';fromdate=$fd;todate=$td;ddMC='0';project='0';task='0'}
Probe 'SR-GetCategoryWise'   'SalesReturnReport/GetCategoryWise'   @{category='0';fromdate=$fd;todate=$td;ddMC='0'}
Probe 'SR-GetBrandWise'      'SalesReturnReport/GetBrandWise'      @{brand='0';fromdate=$fd;todate=$td;ddmc='0'}
Probe 'SR-GetExecutiveWise'  'SalesReturnReport/GetExecutiveWise'  @{salesexec='0';fromdate=$fd;todate=$td;ddmc='0'}
Probe 'SR-getInvoiceWise'    'SalesReturnReport/getInvoiceWise'    @{item='0';customer='0';fromdate=$fd;to=$td;ddmc='0'}
# --- PurchaseReturnReport ---
Probe 'PR-GetItemWise'       'PurchaseReturnReport/GetItemWise'    @{item='0';fromdate=$fd;todate=$td;ddlMC='0'}
Probe 'PR-GetSupplierItemWise' 'PurchaseReturnReport/GetSupplierItemWise' @{ddlSupplier='0';ddlItem='0';From=$fd;To=$td;ddmc='0'}
Probe 'PR-GetSupplierWise'   'PurchaseReturnReport/GetSupplierWise' @{Supplier='0';fromdate=$fd;todate=$td;ddMC='0';project='0';task='0'}
Probe 'PR-GetCategoryWise'   'PurchaseReturnReport/GetCategoryWise' @{category='0';fromdate=$fd;todate=$td;ddMC='0'}
Probe 'PR-GetBrandWise'      'PurchaseReturnReport/GetBrandWise'    @{brand='0';fromdate=$fd;todate=$td;ddmc='0'}
Probe 'PR-GetExecutiveWise'  'PurchaseReturnReport/GetExecutiveWise' @{salesexec='0';fromdate=$fd;todate=$td;ddmc='0'}
Probe 'PR-getInvoiceWise'    'PurchaseReturnReport/getInvoiceWise'  @{item='0';Supplier='0';fromdate=$fd;to=$td;ddmc='0'}
# --- CustomerReport ---
Probe 'CR-GetCustomer'       'CustomerReport/GetCustomer'          @{Customer='0';LastUpdDays='';mc='0'}
Probe 'CR-GetAllRemarks'     'CustomerReport/GetAllRemarks'        @{CustomerId='1'}
# --- HireStockReport ---
Probe 'HS-GetMoment'         'HireStockReport/GetMoment'           @{iditem='2';ddmc='0'}
Probe 'HS-GetDetails'        'HireStockReport/GetDetails'          @{iditem='2';ddmc='0'}
Probe 'HS-GetCrosshire'      'HireStockReport/GetCrosshire'        @{Category='0';Supplier='0';Item='0';Todate=$td;Stock='false'}
