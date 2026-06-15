# Family P: property-realestate. All grid/data endpoints in Areas/Property.

# --- Master grids (no params) ---
Probe 'UnitType.GetUnitType'             'Property/UnitType/GetUnitType' @{}
Probe 'UnitFeature.GetUnitFeature'       'Property/UnitFeature/GetUnitFeature' @{}
Probe 'ContractorType.GetContractorType' 'Property/ContractorType/GetContractorType' @{}
Probe 'Duration.GetDuration'             'Property/Duration/GetDuration' @{}
Probe 'DocumentType.GetDocumentType'     'Property/DocumentType/GetDocumentType' @{}
Probe 'PropertyFeature.GetPrpoertyFeature' 'Property/PropertyFeature/GetPrpoertyFeature' @{}
Probe 'AdditionalField.GetAdditionalField' 'Property/AdditionalField/GetAdditionalField' @{}
Probe 'PropertySettings.GetPropertySettings' 'Property/PropertySettings/GetPropertySettings' @{}
Probe 'ContractType.GetContractType'     'Property/ContractType/GetContractType' @{}
Probe 'PropertyType.GetPropertyType'     'Property/PropertyType/GetPropertyType' @{}

# --- Master grids with filters (party masters) ---
Probe 'Landlords.GetLandlord'   'Property/Landlords/GetLandlord'   @{ Landlord='0'; TaxReg=''; Mobile='0'; Phone='0'; CLimit='0'; CPeriod='0'; Employee='0'; TxType=''; MailId=''; Alias='' }
Probe 'Tenant.GetTenant'        'Property/Tenant/GetTenant'        @{ Tenant='0'; TaxReg=''; Mobile='0'; Phone='0'; CLimit='0'; CPeriod='0'; Employee='0'; TxType=''; MailId=''; Alias='' }
Probe 'Contractor.GetContractor' 'Property/Contractor/GetContractor' @{ Contractor='0'; TaxReg=''; Mobile='0'; Phone='0'; CLimit='0'; CPeriod='0'; Employee='0'; Type=''; MailId=''; Alias='' }
Probe 'Broker.GetBroker'        'Property/Broker/GetBroker'        @{ Broker='0'; TaxReg=''; Mobile='0'; Phone='0'; CLimit='0'; CPeriod='0'; Employee='0'; TxType=''; MailId=''; Alias='' }
Probe 'Developer.GetDeveloper'  'Property/Developer/GetDeveloper'  @{ Developer='0'; TaxReg=''; Mobile='0'; Phone='0'; CLimit='0'; CPeriod='0'; Employee='0'; TxType=''; MailId=''; Alias='' }

# --- Property + Unit grids ---
Probe 'PropertyMain.GetProperty' 'Property/PropertyMain/GetProperty' @{ Property='0'; DocumentType='0'; Landlord='0'; Feature='0'; PropertyType='0'; FromDate=''; ToDate='' }
Probe 'Unit.GetUnit'             'Property/Unit/GetUnit'             @{ Property='0'; UnitType='0'; Unit='0'; FromDate=''; ToDate='' }

# --- Transaction registers ---
Probe 'PropertyRegistration.GetPropertyReg' 'Property/PropertyRegistration/GetPropertyReg' @{ InvoiceNo=''; FromDate=''; ToDate=''; Developer='0'; Owner='0'; Property='0'; Broker='0' }
Probe 'Maintenance.GetPropertyReg'   'Property/Maintenance/GetPropertyReg' @{ InvoiceNo=''; FromDate=''; ToDate=''; Contractor='0'; Property='0' }
Probe 'TenancyContract.GetTenancyContract' 'Property/TenancyContract/GetTenancyContract' @{ Property='0'; Tenant='0'; Unit='0'; FromDate=''; ToDate='' }
Probe 'Rental.GetPropertyRental'     'Property/Rental/GetPropertyRental' @{ InvoiceNo=''; FromDate=''; ToDate=''; Tenant='0'; Property='0'; Unit='0' }
Probe 'RentalProforma.GetData'       'Property/RentalProforma/GetData' @{ InvoiceNo=''; FromDate=''; ToDate=''; Tenant='0'; Property='0' }
Probe 'PropertyTransactions.GetPropertyTransactions' 'Property/PropertyTransactions/GetPropertyTransactions' @{ Property='0'; Unit='0'; fromdate=''; todate='' }
Probe 'PPayment.GetPayment'          'Property/PPayment/GetPayment' @{ InvoiceNo=''; FromDate=''; ToDate=''; type='0'; PayFrom='0'; PayTo='0'; user='' }
Probe 'PReceipt.GetReceipt'          'Property/PReceipt/GetReceipt' @{ InvoiceNo=''; FromDate=''; ToDate=''; type='0'; PayFrom='0'; PayTo='0'; user='' }
Probe 'PJournalV.GetData'            'Property/PJournalV/GetData' @{ InvoiceNo=''; FromDate=''; ToDate=''; PayFrom='0'; PayTo='0'; user=''; type='0'; vnature='0' }

# --- Property reports ---
Probe 'Reports.GetPropertyRegistrations' 'Property/PropertyReports/GetPropertyRegistrations' @{ Voucher=''; Developer='0'; Owner='0'; Broker='0'; Property='0'; fromdate=''; todate='' }
Probe 'Reports.GetTenancyContract'   'Property/PropertyReports/GetTenancyContract' @{ Unit='0'; DocumentType='0'; Tenant='0'; Property='0'; PayType='0'; Schedule='0'; Duedate='0'; fromdate=''; todate='' }
Probe 'Reports.GetRentalInvoice'     'Property/PropertyReports/GetRentalInvoice' @{ Voucher=''; Tenant='0'; Unit='0'; Property='0'; fromdate=''; todate='' }
Probe 'Reports.GetMaintance'         'Property/PropertyReports/GetMaintance' @{ Voucher=''; Contractor='0'; Property='0'; fromdate=''; todate='' }
Probe 'Reports.GetPayment'           'Property/PropertyReports/GetPayment' @{ vno=''; payfrom='0'; payto='0'; fromdate=''; todate=''; Property='0'; Unit='0' }
Probe 'Reports.GetReceipt'           'Property/PropertyReports/GetReceipt' @{ vno=''; payfrom='0'; payto='0'; fromdate=''; todate=''; Property='0'; Unit='0' }
Probe 'Reports.GetJournal'           'Property/PropertyReports/GetJournal' @{ vno=''; payfrom='0'; payto='0'; fromdate=''; todate='' }
Probe 'Reports.GetEmptyUnits'        'Property/PropertyReports/GetEmptyUnits' @{ landlords='0'; Property='0' }
Probe 'Reports.GetExpense'           'Property/PropertyReports/GetExpense' @{ landlord='0'; Property='0'; fromdate=''; todate='' }
Probe 'Reports.GetIncome'            'Property/PropertyReports/GetIncome' @{ landlord='0'; Property='0'; fromdate=''; todate='' }
Probe 'Reports.Getrateofreturn'      'Property/PropertyReports/Getrateofreturn' @{ landlord='0'; Property='0'; fromdate=''; todate='' }
Probe 'Reports.Getdocumentexpiry'    'Property/PropertyReports/Getdocumentexpiry' @{ section=''; date='' }

# --- Home dashboard grids (no params) ---
Probe 'Home.GetDocumentExpairy'      'Property/PropertyHome/GetDocumentExpairy' @{}
Probe 'Home.GetCheques'              'Property/PropertyHome/GetCheques' @{}
Probe 'Home.GetExpiryCheques'        'Property/PropertyHome/GetExpiryCheques' @{}
Probe 'Home.GetRegistrations'        'Property/PropertyHome/GetRegistrations' @{}
Probe 'Home.GetTenancyContracts'     'Property/PropertyHome/GetTenancyContracts' @{}
Probe 'Home.GetMaintenanceContracts' 'Property/PropertyHome/GetMaintenanceContracts' @{}
Probe 'Home.GetHireExp'              'Property/PropertyHome/GetHireExp' @{}
Probe 'Home.GetCrossHireExp'         'Property/PropertyHome/GetCrossHireExp' @{}
