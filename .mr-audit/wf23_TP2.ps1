# Re-probe SearchInvoiceItem WITH page form field so we get past Request.Form.GetValues("page").FirstOrDefault()
Probe 'HR_SearchInvoiceItem_page' 'HireReturn/SearchInvoiceItem' @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; Invoice='0'; page='1' }
Probe 'CHR_SearchInvoiceItem_page' 'CrossHireReturn/SearchInvoiceItem' @{ q=''; x=''; cust='0'; mc='0'; constat='PurchaseEntry'; Invoice='0'; page='1' }
# also a non-empty q to drive ToLower().Contains branch + constat=SalesEntry to hit StockItemsPerm branch (line ~2160 unbounded ToList)
Probe 'HR_SearchInvoiceItem_q' 'HireReturn/SearchInvoiceItem' @{ q='a'; x=''; cust='0'; mc='0'; constat='SalesEntry'; Invoice='0'; page='1' }
Probe 'CHR_SearchInvoiceItem_q' 'CrossHireReturn/SearchInvoiceItem' @{ q='a'; x=''; cust='0'; mc='0'; constat='PurchaseEntry'; Invoice='0'; page='1' }
