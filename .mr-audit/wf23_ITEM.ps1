# WF23: ItemController in-projection .ToList().Sum/Select port-break candidates.
# Each action below has a (subquery...).ToList().Sum(...) INSIDE an unmaterialized
# EF 'select new {...}' projection that is later forced to translate via item.ToList().

# SearchdetailsMCtech: item1 (5819) projection has .ToList().Sum, materialized at vd=item1...ToList() (6069)
Probe 'SearchdetailsMCtech' 'Item/SearchdetailsMCtech' @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; page='1' }

# SearchdetailsMC: item1 (6411, stockcheck inactive branch) projection has .ToList().Sum, materialized vd=item1.ToList() (6666)
Probe 'SearchdetailsMC' 'Item/SearchdetailsMC' @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; page='1' }

# GetItemMCbar: item (7868) projection has .ToList().Sum at 7915.., materialized vd=item.ToList() (8099)
Probe 'GetItemMCbar' 'Item/GetItemMCbar' @{ itemID='0'; mc='0' }

# GetItemHire: item (8206) projection has .ToList().Sum at 8253.., .Distinct().AsEnumerable() at 8431
Probe 'GetItemHire' 'Item/GetItemHire' @{ itemID='1'; mc='0'; SalType='0'; HireType='0' }

# SearchBatch: itemq (11460) projection has .ToList().Sum at 11489/11492, materialized vd (11494)
Probe 'SearchBatch' 'Item/SearchBatch' @{ q=''; x=''; itemid='1'; page='1' }

# GetBatch: itemq (11631) projection has .ToList().Sum at 11662/11665, materialized vd=itemq.ToList() (11668)
Probe 'GetBatch' 'Item/GetBatch' @{ BatchNo=''; itemid='1' }

# SearchItemDetailsByMC: item1 (11738) projection has .ToList().Sum at 11796.., materialized vd=item1.ToList() (11819)
Probe 'SearchItemDetailsByMC' 'Item/SearchItemDetailsByMC' @{ q=''; x=''; cust='0'; mc='0'; constat='SalesEntry'; page='1' }
