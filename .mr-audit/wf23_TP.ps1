# HireReturn SearchInvoiceItem - true in-projection .ToList().Sum (line 1930)
Probe 'HR_SearchInvoiceItem_inv0' 'HireReturn/SearchInvoiceItem' @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; Invoice='0' }
Probe 'HR_SearchInvoiceItem_blank' 'HireReturn/SearchInvoiceItem' @{ q=''; x=''; cust=''; mc=''; constat=''; Invoice='' }
# CrossHireReturn SearchInvoiceItem - true in-projection .ToList().Sum (line 1807)
Probe 'CHR_SearchInvoiceItem_inv0' 'CrossHireReturn/SearchInvoiceItem' @{ q=''; x=''; cust='0'; mc='0'; constat='PurchaseEntry'; Invoice='0' }
Probe 'CHR_SearchInvoiceItem_blank' 'CrossHireReturn/SearchInvoiceItem' @{ q=''; x=''; cust=''; mc=''; constat=''; Invoice='' }
# Data grids for context (no in-projection .ToList, but confirm reachability/perms)
Probe 'HR_GetHireReturn' 'HireReturn/GetHireReturn' @{ BillNo=''; FromDate=''; ToDate=''; customer='0'; salesperson='0'; user=''; invoice='0'; appstat=''; ProjectName='0'; Task='0' }
Probe 'CHR_GetCrossHireReturn' 'CrossHireReturn/GetCrossHireReturn' @{ BillNo=''; FromDate=''; ToDate=''; supplier='0'; salesperson='0'; user=''; invoice='0'; appstat=''; ProjectName='0'; Task='0' }
Probe 'SV_GetData' 'StockVerification/GetData' @{ InvoiceNo=''; FromDate=''; ToDate=''; PayFrom='0'; PayTo='0' }
Probe 'SV_GetSVItems' 'StockVerification/GetSVItems' @{ EntryID='0' } 'GET'
# BalanceSheet GetBalanceSheet - needs dates (FormatException guard at line 62 ParseExact on fromdate)
Probe 'BS_GetBalanceSheet' 'BalanceSheet/GetBalanceSheet' @{ fromdate='01-04-2025'; todate='31-03-2026' }
