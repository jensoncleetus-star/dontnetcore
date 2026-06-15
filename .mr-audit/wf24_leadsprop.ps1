# Leads dashboards containing the 4 .ToList().Sum/Count hits (all terminal-materialization, expected 200)
Probe "Leads/accountsdashboard"     "Leads/accountsdashboard"   @{} "GET"
Probe "Leads/customerdashboard"     "Leads/customerdashboard"   @{} "GET"
Probe "Leads/followups"             "Leads/followups"           @{} "GET"
Probe "Leads/leaddashboard"         "Leads/leaddashboard"       @{} "GET"
Probe "Leads/leadsnewdashboard"     "Leads/leadsnewdashboard"   @{} "GET"

# Property report endpoints whose correlated subqueries were inspected
# Getrateofreturn already fixed (-> .Sum(x=>(decimal?)..)??0); Income/Expense have subqueries commented out
Probe "Property/Getrateofreturn"    "Property/PropertyReports/Getrateofreturn"  @{ landlord='0'; Property='0'; fromdate=''; todate='' }
Probe "Property/GetIncome"          "Property/PropertyReports/GetIncome"        @{ landlord='0'; Property='0'; fromdate=''; todate='' }
Probe "Property/GetExpense"         "Property/PropertyReports/GetExpense"       @{ landlord='0'; Property='0'; fromdate=''; todate='' }
