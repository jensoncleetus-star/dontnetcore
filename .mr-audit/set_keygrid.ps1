# Key data-entry/master grids — post-batch-19 unmasking spot-check.
$fd='01-01-2010'; $td='31-12-2026'
Probe 'Customer-GetCustomer'  'Customer/GetCustomer'  @{FromDate=$fd;ToDate=$td;Customer='0';TaxReg='';Mobile='';Phone='';CLimit='';CPeriod='';Employee='0';Type='0';Source='0';TxType='';MailId='';Alias=''}
Probe 'Accounts-GetData'      'Accounts/GetData'      @{ddlAccounts='0';AccGroup='0';Stats='';TRN='';Alias=''}
Probe 'Journal-GetData'       'Journal/GetData'       @{InvoiceNo='';FromDate=$fd;ToDate=$td;PayFrom='0';PayTo='0';user=''}
Probe 'Payment-GetPayment'    'Payment/GetPayment'    @{InvoiceNo='';SaleInvoiceNo='';FromDate=$fd;ToDate=$td;type='0';PayFrom='0';PayTo='0';user=''}
Probe 'Supplier-GetSupplier'  'Supplier/GetSupplier'  @{}
Probe 'Item-GetItem'          'Item/GetItem'          @{}
