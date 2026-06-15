# Cluster F register-grid endpoints — port-break sweep.
$fd='01-01-2010'; $td='31-12-2026'
Probe 'PurchaseEntry-GetPurchaseEntry'        'PurchaseEntry/GetPurchaseEntry'        @{BillNo='';FromDate=$fd;ToDate=$td;supplier='0';salesperson='0';type='0';user='';Balance='0';MC='0';appstat='';PurchaseType='';HireType='0';PurchaseStatus='0';RefenceNo=''}
Probe 'PurchaseEntryExpense-GetPurchaseEntry' 'PurchaseEntryExpense/GetPurchaseEntry' @{BillNo='';FromDate=$fd;ToDate=$td;supplier='0';salesperson='0';type='0';user='';Balance='0';MC='0';appstat='';PurchaseType='';HireType='0';PurchaseStatus='0';RefenceNo=''}
Probe 'Assetpurchase-GetAssets'               'Assetpurchase/GetAssets'               @{fromdate=$fd;todate=$td}
Probe 'WorkCompletion-GetWorkCompletion'      'WorkCompletion/GetWorkCompletion'      @{BillNo='';FromDate=$fd;ToDate=$td;customer='0';salesperson='0'}
Probe 'Receipt-GetReceipt'                     'Receipt/GetReceipt'                     @{InvoiceNo='';FromDate=$fd;ToDate=$td;type='0';PayFrom='0';PayTo='0';user=''}
