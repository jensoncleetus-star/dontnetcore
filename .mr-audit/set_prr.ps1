# PropertyReports/Getrateofreturn — rate-of-return grid (Maintenances + Rental/TenancyContract income sums).
Probe 'Getrateofreturn all/blank'  'Property/PropertyReports/Getrateofreturn' @{ landlord='0'; Property='0'; fromdate=''; todate='' }
Probe 'Getrateofreturn all/wide'   'Property/PropertyReports/Getrateofreturn' @{ landlord='0'; Property='0'; fromdate='01-01-2015'; todate='31-12-2026' }
Probe 'Getrateofreturn Property=1' 'Property/PropertyReports/Getrateofreturn' @{ landlord='0'; Property='1'; fromdate=''; todate='' }
