# TaxReports + StockJournal + StockAdjustment data endpoints — sweep.
$fd='01-01-2010'; $td='31-12-2026'
# --- TaxReports (VAT) ---
Probe 'GetPurchaseTax'       'TaxReports/GetPurchaseTax'       @{vno='';supplier='0';type='0';fromdate=$fd;todate=$td;emirates=''}
Probe 'GetPurchaseReturnTax' 'TaxReports/GetPurchaseReturnTax' @{vno='';supplier='0';type='0';fromdate=$fd;todate=$td;emirates=''}
Probe 'GetSaleTax'           'TaxReports/GetSaleTax'           @{vno='';customer='0';type='0';fromdate=$fd;todate=$td;emirates='';satype=''}
Probe 'GetSaleTax2'          'TaxReports/GetSaleTax2'          @{vno='';customer='0';type='0';fromdate=$fd;todate=$td;emirates='';satype=''}
Probe 'GetSalesReturnTax'    'TaxReports/GetSalesReturnTax'    @{vno='';customer='0';type='0';fromdate=$fd;todate=$td;emirates=''}
Probe 'GetExpenseTax'        'TaxReports/GetExpenseTax'        @{vno='';fromdate=$fd;todate=$td;expacc='0'}
Probe 'GetUaeVat'            'TaxReports/GetUaeVat'            @{fromdate=$fd;todate=$td}
Probe 'GetExpenseTaxReport'  'TaxReports/GetExpenseTaxReport'  @{fromdate=$fd;todate=$td}
Probe 'GetVat'               'TaxReports/GetVat'               @{fromdate=$fd;todate=$td}
Probe 'GetDetailsVat'        'TaxReports/GetDetailsVat'        @{taxtype='out';fromdate=$fd;todate=$td}
Probe 'GetNewDetailsVat'     'TaxReports/GetNewDetailsVat'     @{taxtype='in';fromdate=$fd;todate=$td}
# --- StockJournal ---
Probe 'GetStockJournal'      'StockJournal/GetStockJournal'    @{FromDate=$fd;ToDate=$td;Employee='0';MCFrom='0';MCTo='0';appstat=''}
Probe 'GetGeneratedItems'    'StockJournal/GetGeneratedItems'  @{DvID='1'}
Probe 'GetAllStatusUpdation' 'StockJournal/GetAllStatusUpdation' @{MCId='1'}
# --- StockAdjustment (by-id editors) ---
Probe 'GetAdjById'           'StockAdjustment/GetAdjById'      @{StkId='1'}
Probe 'GetAssetAdjById'      'StockAdjustment/GetAssetAdjById' @{AdjId='1'}
