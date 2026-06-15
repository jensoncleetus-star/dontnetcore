# Batch 23 verification — every endpoint that was 500 before the .ToList()-projection / Count / null-unwrap / date-concat fixes.
# ItemController (route Item/*)
Probe 'Item.SearchdetailsMCtech'   'Item/SearchdetailsMCtech'   @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; page='1' }
Probe 'Item.SearchdetailsMC'       'Item/SearchdetailsMC'       @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; page='1' }
Probe 'Item.GetItemMCbar'          'Item/GetItemMCbar'          @{ itemID='0'; mc='0' }
Probe 'Item.GetItemHire'           'Item/GetItemHire'           @{ itemID='1'; mc='0'; SalType='0'; HireType='0' }
Probe 'Item.SearchBatch'           'Item/SearchBatch'           @{ q=''; x=''; itemid='1'; page='1' }
Probe 'Item.GetBatch'              'Item/GetBatch'              @{ BatchNo=''; itemid='1' }
Probe 'Item.SearchItemDetailsByMC' 'Item/SearchItemDetailsByMC' @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; page='1' }
# MyReports (route MyReports/*)
Probe 'MR.SalesReturnItemWiseR'    'MyReports/SalesReturnItemWiseR' @{ ddlItemCategory='0'; ddmc='0'; From='01-01-2020'; To='31-12-2026' }
Probe 'MR.SalesReturnItemWise'     'MyReports/SalesReturnItemWise'  @{ ddlItemCategory='0'; ddmc='0'; From='01-01-2020'; To='31-12-2026' }
# Hire/CrossHire SearchInvoiceItem (need page)
Probe 'HR.SearchInvoiceItem'       'HireReturn/SearchInvoiceItem'      @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; Invoice='0'; page='1' }
Probe 'CHR.SearchInvoiceItem'      'CrossHireReturn/SearchInvoiceItem' @{ q=''; x=''; cust='0'; mc='0'; constat='PurchaseEntry'; Invoice='0'; page='1' }
# Hr AttendanceReport + PayrollReport (route Hr/*)
Probe 'Hr.GetAttendanceSheet'      'Hr/AttendanceReport/GetAttendanceSheet' @{ Emp='0'; fromdate='01-01-2020'; todate='13-06-2026' }
Probe 'Hr.GetPaySheet'             'Hr/PayrollReport/GetPaySheet'           @{ empl='0'; monthyear='06-2026' }
