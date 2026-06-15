# Re-probe Getrateofreturn with non-empty params to rule out artifact
Probe 'Reports.Getrateofreturn.p0'   'Property/PropertyReports/Getrateofreturn' @{ landlord='0'; Property='0'; fromdate=''; todate='' }
Probe 'Reports.Getrateofreturn.pdate' 'Property/PropertyReports/Getrateofreturn' @{ landlord='0'; Property='0'; fromdate='01-01-2020'; todate='31-12-2026' }
Probe 'Reports.Getrateofreturn.pprop' 'Property/PropertyReports/Getrateofreturn' @{ landlord='0'; Property='1'; fromdate='01-01-2020'; todate='31-12-2026' }
# Re-probe PropertyTransactions with a real Property id to confirm it is the Find()/missing-column path, not empty-param
Probe 'PropTxn.find.p1'  'Property/PropertyTransactions/GetPropertyTransactions' @{ Property='1'; Unit='0'; fromdate='01-01-2020'; todate='31-12-2026' }
