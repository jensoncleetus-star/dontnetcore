/* ============================================================================
   BOS — Add "Real Estate" as a top-level menu (Master / Transactions / Reports / Settings)
   mirroring "Customer Relation". The menu is data-driven: each row in AppModules is a
   role; a menu item shows only if the logged-in user has a role matching its Name
   (HomeController.Menu filters AppModuless by Role.Contains(Name) && addMenu==Yes).
   So this is fully "user-control wise" — enable per user in User management.

   Enums: choice.Yes=0 / choice.No=1 ; Status.active=0.
   IsParent: 0 = parent (has submenu), 1 = leaf (direct link).
   ModulesID range 234343600-234343699 is reserved for Real Estate (all > current MAX,
   guaranteed unique + easy to identify/rollback). IDEMPOTENT (IF NOT EXISTS by ModulesID).
   Rollback: see DeleteRealEstateMenu.sql.
   ============================================================================ */
SET NOCOUNT ON;

/* helper: insert one menu/role row if its ModulesID does not already exist */
DECLARE @disc nvarchar(128) = N'AppModules';

/* ---- TOP-LEVEL: Real Estate (role = "Real Estate") ---- */
IF NOT EXISTS (SELECT 1 FROM AppModules WHERE ModulesID = 234343600)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
VALUES (CONVERT(nvarchar(450), NEWID()), N'Real Estate', N'REAL ESTATE', CONVERT(nvarchar(450), NEWID()), @disc, 234343600, N'Real Estate', N'#', 0, 0, 0, 0, 8, 1, N'fa fa-building');

/* ---- DASHBOARD (direct leaf under Real Estate, shows first) ---- */
IF NOT EXISTS (SELECT 1 FROM AppModules WHERE ModulesID = 234343605)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
VALUES (CONVERT(nvarchar(450), NEWID()), N'RE Dashboard', N'RE DASHBOARD', CONVERT(nvarchar(450), NEWID()), @disc, 234343605, N'Dashboard', N'/Property/PropertyHome/Dashboard', 234343600, 1, 0, 0, 0, 1, N'fa fa-tachometer');

/* ---- SUB-PARENTS (Parent = 234343600) ---- */
IF NOT EXISTS (SELECT 1 FROM AppModules WHERE ModulesID = 234343601)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
VALUES (CONVERT(nvarchar(450), NEWID()), N'RealEstate Master', N'REALESTATE MASTER', CONVERT(nvarchar(450), NEWID()), @disc, 234343601, N'Master', N'#', 234343600, 0, 0, 0, 1, 1, N'fa fa-th');
IF NOT EXISTS (SELECT 1 FROM AppModules WHERE ModulesID = 234343602)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
VALUES (CONVERT(nvarchar(450), NEWID()), N'RealEstate Transactions', N'REALESTATE TRANSACTIONS', CONVERT(nvarchar(450), NEWID()), @disc, 234343602, N'Transactions', N'#', 234343600, 0, 0, 0, 2, 1, N'fa fa-th');
IF NOT EXISTS (SELECT 1 FROM AppModules WHERE ModulesID = 234343603)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
VALUES (CONVERT(nvarchar(450), NEWID()), N'RealEstate Reports', N'REALESTATE REPORTS', CONVERT(nvarchar(450), NEWID()), @disc, 234343603, N'Reports', N'#', 234343600, 0, 0, 0, 3, 1, N'fa fa-th');
IF NOT EXISTS (SELECT 1 FROM AppModules WHERE ModulesID = 234343604)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
VALUES (CONVERT(nvarchar(450), NEWID()), N'RealEstate Settings', N'REALESTATE SETTINGS', CONVERT(nvarchar(450), NEWID()), @disc, 234343604, N'Settings', N'#', 234343600, 0, 0, 0, 4, 1, N'fa fa-th');

/* ---- MASTER leaves (Parent = 234343601) ---- */
;WITH M(mid, nm, vn, lk, ord) AS (
    SELECT 234343610, N'RE Landlord',     N'Landlords',     N'/Property/Landlords/Index', 1 UNION ALL
    SELECT 234343611, N'RE Tenant',       N'Tenant',        N'/Property/Tenant/Index', 2 UNION ALL
    SELECT 234343612, N'RE Developer',    N'Developer',     N'/Property/Developer/Index', 3 UNION ALL
    SELECT 234343613, N'RE Contractor',   N'Contractor',    N'/Property/Contractor/Index', 4 UNION ALL
    SELECT 234343614, N'RE Broker',       N'Broker',        N'/Property/Broker/Index', 5 UNION ALL
    SELECT 234343615, N'RE PropertyType', N'Property Type', N'/Property/PropertyType/', 6 UNION ALL
    SELECT 234343616, N'RE Property',     N'Property',      N'/Property/PropertyMain/', 7 UNION ALL
    SELECT 234343617, N'RE Unit',         N'Units',         N'/Property/Unit/', 8
)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
SELECT CONVERT(nvarchar(450), NEWID()), nm, UPPER(nm), CONVERT(nvarchar(450), NEWID()), @disc, mid, vn, lk, 234343601, 1, 0, 0, ord, 1, N'fa fa-circle-o'
FROM M WHERE NOT EXISTS (SELECT 1 FROM AppModules a WHERE a.ModulesID = M.mid);

/* ---- TRANSACTION leaves (Parent = 234343602) ---- */
;WITH T(mid, nm, vn, lk, ord) AS (
    SELECT 234343620, N'RE PropertyRegistration', N'Property Registration', N'/Property/PropertyRegistration/Index', 1 UNION ALL
    SELECT 234343621, N'RE TenancyContract',      N'Tenancy Contract',      N'/Property/TenancyContract/Index', 2 UNION ALL
    SELECT 234343622, N'RE Rental',               N'Rental Invoice',        N'/Property/Rental/Index', 3 UNION ALL
    SELECT 234343623, N'RE RentalProforma',       N'Rental Proforma',       N'/Property/RentalProforma/Index', 4 UNION ALL
    SELECT 234343624, N'RE Maintenance',          N'Maintenance Contract',  N'/Property/Maintenance/Index', 5 UNION ALL
    SELECT 234343625, N'RE Journal',              N'Journal',               N'/Property/PJournalV/Index', 6 UNION ALL
    SELECT 234343626, N'RE Payment',              N'Payment',               N'/Property/PPayment/Index', 7 UNION ALL
    SELECT 234343627, N'RE Receipt',              N'Receipt',               N'/Property/PReceipt/Index', 8
)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
SELECT CONVERT(nvarchar(450), NEWID()), nm, UPPER(nm), CONVERT(nvarchar(450), NEWID()), @disc, mid, vn, lk, 234343602, 1, 0, 0, ord, 1, N'fa fa-circle-o'
FROM T WHERE NOT EXISTS (SELECT 1 FROM AppModules a WHERE a.ModulesID = T.mid);

/* ---- REPORT leaves (Parent = 234343603) ---- */
;WITH R(mid, nm, vn, lk, ord) AS (
    SELECT 234343630, N'RE Rpt Summary',      N'Property Summary',      N'/Property/PropertyReports/PropertySummery', 1 UNION ALL
    SELECT 234343631, N'RE Rpt Property',     N'Property Report',       N'/Property/PropertyReports/PropertyConsolidated', 2 UNION ALL
    SELECT 234343632, N'RE Rpt PL',           N'Property Based P&L',    N'/MyReports/PLSummaryProperty', 3 UNION ALL
    SELECT 234343633, N'RE Rpt Registration', N'Property Registration', N'/Property/PropertyReports/PropertyRegistration', 4 UNION ALL
    SELECT 234343634, N'RE Rpt Tenancy',      N'Tenancy Contract',      N'/Property/PropertyReports/TenancyContract', 5 UNION ALL
    SELECT 234343635, N'RE Rpt Rental',       N'Rental Invoice',        N'/Property/PropertyReports/RentalInvoice', 6 UNION ALL
    SELECT 234343636, N'RE Rpt Maintenance',  N'Maintenance Contract',  N'/Property/PropertyReports/Maintance', 7
)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
SELECT CONVERT(nvarchar(450), NEWID()), nm, UPPER(nm), CONVERT(nvarchar(450), NEWID()), @disc, mid, vn, lk, 234343603, 1, 0, 0, ord, 1, N'fa fa-circle-o'
FROM R WHERE NOT EXISTS (SELECT 1 FROM AppModules a WHERE a.ModulesID = R.mid);

/* ---- SETTINGS leaves (Parent = 234343604) ---- */
;WITH S(mid, nm, vn, lk, ord) AS (
    SELECT 234343640, N'RE ContractType',     N'Contract Type',    N'/Property/ContractType/Index', 1 UNION ALL
    SELECT 234343641, N'RE UnitType',         N'Unit Type',        N'/Property/UnitType/Index', 2 UNION ALL
    SELECT 234343642, N'RE PropertyFeature',  N'Property Feature', N'/Property/PropertyFeature/Index', 3 UNION ALL
    SELECT 234343643, N'RE UnitFeature',      N'Unit Feature',     N'/Property/UnitFeature/Index', 4 UNION ALL
    SELECT 234343644, N'RE ContractorType',   N'Contractor Type',  N'/Property/ContractorType/Index', 5 UNION ALL
    SELECT 234343645, N'RE Duration',         N'Duration',         N'/Property/Duration/Index', 6 UNION ALL
    SELECT 234343646, N'RE DocumentType',     N'Document Type',     N'/Property/DocumentType/Index', 7 UNION ALL
    SELECT 234343647, N'RE AdditionalField',  N'Additional Field', N'/Property/AdditionalField/Index', 8 UNION ALL
    SELECT 234343648, N'RE PropertySettings', N'Property Settings', N'/Property/PropertySettings/Index', 9
)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
SELECT CONVERT(nvarchar(450), NEWID()), nm, UPPER(nm), CONVERT(nvarchar(450), NEWID()), @disc, mid, vn, lk, 234343604, 1, 0, 0, ord, 1, N'fa fa-circle-o'
FROM S WHERE NOT EXISTS (SELECT 1 FROM AppModules a WHERE a.ModulesID = S.mid);

/* ---- INSIGHTS sub-parent (Parent = 234343600) + leaves (advanced analytics) ---- */
IF NOT EXISTS (SELECT 1 FROM AppModules WHERE ModulesID = 234343606)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
VALUES (CONVERT(nvarchar(450), NEWID()), N'RealEstate Insights', N'REALESTATE INSIGHTS', CONVERT(nvarchar(450), NEWID()), @disc, 234343606, N'Insights', N'#', 234343600, 0, 0, 0, 5, 1, N'fa fa-line-chart');
;WITH I(mid, nm, vn, lk, ord) AS (
    SELECT 234343660, N'RE Insight PL',          N'Property P&L',           N'/Property/PropertyInsights/ProfitLoss', 1 UNION ALL
    SELECT 234343661, N'RE Insight Contractor',  N'Contractor Allocation',  N'/Property/PropertyInsights/ContractorAllocation', 2 UNION ALL
    SELECT 234343662, N'RE Insight Unit',        N'Unit Insights',          N'/Property/PropertyInsights/UnitInsights', 3 UNION ALL
    SELECT 234343663, N'RE Insight Performance', N'Portfolio Performance',  N'/Property/PropertyInsights/Performance', 4 UNION ALL
    SELECT 234343664, N'RE Insight Location',    N'Portfolio by Location',  N'/Property/PropertyInsights/Locations', 5 UNION ALL
    SELECT 234343665, N'RE Insight Renewals',    N'Renewals & Expiry',      N'/Property/PropertyInsights/Renewals', 6 UNION ALL
    SELECT 234343666, N'RE Insight Trend',       N'Financial Trend',        N'/Property/PropertyInsights/FinancialTrend', 7 UNION ALL
    SELECT 234343667, N'RE Insight Ledger',      N'Tenant Ledger',          N'/Property/PropertyInsights/TenantLedger', 8 UNION ALL
    SELECT 234343668, N'RE Insight Payout',      N'Landlord Payout',        N'/Property/PropertyInsights/LandlordPayout', 9 UNION ALL
    SELECT 234343669, N'RE Insight Collection',  N'Rent Collection',        N'/Property/PropertyInsights/RentCollection', 10 UNION ALL
    SELECT 234343670, N'RE Insight Compare',     N'Property Comparison',    N'/Property/PropertyInsights/Comparison', 11 UNION ALL
    SELECT 234343671, N'RE Insight Property360',  N'Property 360',           N'/Property/PropertyInsights/Property360', 12 UNION ALL
    SELECT 234343672, N'RE Insight Availability', N'Availability Board',     N'/Property/PropertyInsights/Availability', 13 UNION ALL
    SELECT 234343673, N'RE Insight Gallery',      N'Property Gallery',       N'/Property/PropertyInsights/Gallery', 14 UNION ALL
    SELECT 234343674, N'RE Insight Calendar',     N'Maintenance Calendar',   N'/Property/PropertyInsights/MaintenanceCalendar', 15 UNION ALL
    SELECT 234343675, N'RE Insight Reminders',    N'Expiry Reminders',       N'/Property/PropertyInsights/Reminders', 16
)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
SELECT CONVERT(nvarchar(450), NEWID()), nm, UPPER(nm), CONVERT(nvarchar(450), NEWID()), @disc, mid, vn, lk, 234343606, 1, 0, 0, ord, 1, N'fa fa-circle-o'
FROM I WHERE NOT EXISTS (SELECT 1 FROM AppModules a WHERE a.ModulesID = I.mid);

SELECT COUNT(*) AS RealEstateMenuRows FROM AppModules WHERE ModulesID BETWEEN 234343600 AND 234343699;
