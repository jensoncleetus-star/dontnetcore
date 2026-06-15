# HireReturn/CrossHireReturn SearchInvoiceItem WITH the page form field (real UI sends it; absence NREs at 1944/1821 BEFORE the projection).
Probe 'HR_SearchInvoiceItem'  'HireReturn/SearchInvoiceItem'      @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; Invoice='0'; page='1' }
Probe 'CHR_SearchInvoiceItem' 'CrossHireReturn/SearchInvoiceItem' @{ q=''; x=''; cust='0'; mc='0'; constat='PurchaseEntry'; Invoice='0'; page='1' }
