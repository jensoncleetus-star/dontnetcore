$fd='01-01-2010'; $td='31-12-2026'
Probe 'CSRNote-GetSalesReturn' 'CreditSaleReturnNote/GetSalesReturn' @{BillNo='';FromDate=$fd;ToDate=$td;customer='0';salesperson='0';type='0';user='';Balance='0';MC='0';appstat='';ProjectName='0';Task='0'}
Probe 'CSR-GetSalesReturn'     'CreditSaleReturn/GetSalesReturn'     @{BillNo='';FromDate=$fd;ToDate=$td;customer='0';salesperson='0';type='0';user='';Balance='0';MC='0';appstat='';ProjectName='0';Task='0'}
