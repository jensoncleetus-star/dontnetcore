# StockReport data endpoints — sweep (batch 11 fixed GetOnDate/GetBwDate/GetCategoryWise/GetBrandWise/GetDetails).
Probe 'GetStock'        'StockReport/GetStock'        @{stockable='true';ddmc='0'}
Probe 'GetOnDate'       'StockReport/GetOnDate'       @{ondate='31-12-2026';stockable='true';ddmc='0'}
Probe 'GetItemWise'     'StockReport/GetItemWise'     @{itemid='2';ddmc='0'}
Probe 'GetCategoryWise' 'StockReport/GetCategoryWise' @{categoryid='0';stockable='true';ddmc='0'}
Probe 'GetBrandWise'    'StockReport/GetBrandWise'    @{brandid='0';stockable='true';ddmc='0'}
Probe 'GetMoment'       'StockReport/GetMoment'       @{iditem='2';ddmc='0'}
Probe 'GetMoment2'      'StockReport/GetMoment2'      @{iditem='2';ddmc='0';datefrom='01-01-2010';dateto='31-12-2026'}
Probe 'GetDetails'      'StockReport/GetDetails'      @{iditem='2';ddmc='0'}
Probe 'GetBwDate'       'StockReport/GetBwDate'       @{fromd='01-01-2010';to='31-12-2026';stockable='true';ddmc='0'}
Probe 'GetExpiry'       'StockReport/GetExpiry'       @{iditem='2'}
