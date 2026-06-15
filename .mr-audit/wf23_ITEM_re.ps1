# WF23 re-probe: negative control (safe pattern) + varied params to rule out artifacts.

# NEGATIVE CONTROL: Searchdetails uses .AsEnumerable().Select(...) -> subqueries L2O -> should be 200.
Probe 'CTRL_Searchdetails'        'Item/Searchdetails'        @{ q=''; x=''; cust='0'; constat='SalesEntry'; page='1' }
# also another known-safe one with the same shaped params:
Probe 'CTRL_SearchdetailsMCSP'    'Item/SearchdetailsMCSP'    @{ q='a'; x=''; cust='0'; mc='0'; constat='SalesEntry'; page='1' }

# RE-PROBE candidates with realistic/non-empty q and varied mc, to confirm 500 is param-independent (translation bug).
Probe 'SearchdetailsMCtech_v2'    'Item/SearchdetailsMCtech'  @{ q='a'; x=''; cust='0'; mc='1'; constat='SalesEntry'; page='1' }
Probe 'SearchdetailsMC_v2'        'Item/SearchdetailsMC'      @{ q='a'; x=''; cust='0'; mc='1'; constat='SalesEntry'; page='1' }
Probe 'GetItemMCbar_v2'           'Item/GetItemMCbar'         @{ itemID='1'; mc='1' }
Probe 'GetItemHire_v2'            'Item/GetItemHire'          @{ itemID='1'; mc='1'; SalType='1'; HireType='0' }
Probe 'SearchBatch_v2'            'Item/SearchBatch'          @{ q='a'; x=''; itemid='1'; page='1' }
Probe 'GetBatch_v2'               'Item/GetBatch'             @{ BatchNo='1'; itemid='1' }
Probe 'SearchItemDetailsByMC_v2'  'Item/SearchItemDetailsByMC' @{ q='a'; x=''; cust='0'; mc='1'; constat='SalesEntry'; page='1' }
