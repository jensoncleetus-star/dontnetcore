/* ============================================================================
   BOS — Real Estate menu REORGANIZATION v2 (recommended 6-group IA)
   Turns the flat Master/Transactions/Reports/Settings/Insights layout into a
   lifecycle/usage hybrid:
       Dashboard | Leasing & Operations | Portfolio & Parties |
       Maintenance | Reports & Insights | Configuration

   100% data-only (AppModules), confined to the reserved 234343600-234343699
   block, IDEMPOTENT (re-runnable), and REVERSIBLE (ReorganizeRealEstateMenu_v2_Rollback.sql).

   What it does:
     1. Adds 6 new GROUP parent rows (234343650-655) under the Real Estate root.
     2. Adds 10 new LEAF rows (234343681-685 reports/ledger + 234343690-694 hub overviews).
     3. RE-PARENTS + re-orders the 49 existing leaves into the new groups
        (links unchanged → no dead links; roles unchanged → no access lost).
     4. RETIRES the 5 old group parents (Master/Transactions/Reports/Settings/Insights)
        by setting Status=inactive so they stop rendering (rows kept for rollback).

   NOTE: each AppModules row is a ROLE. After running this, grant the 6 new group
   roles + 10 new leaf roles to the users who should see them (User management),
   or run the grant block at the bottom for a specific user.
   ============================================================================ */
SET NOCOUNT ON;
DECLARE @disc nvarchar(128) = N'AppModules';
DECLARE @root bigint = 234343600;

/* ---------- 1. NEW GROUP PARENTS (234343650-655), Link='#' (expanders) ---------- */
;WITH G(mid, nm, vn, ord, ic) AS (
    SELECT 234343650, N'RE Home',        N'Dashboard',             1, N'fa fa-tachometer'   UNION ALL
    SELECT 234343651, N'RE Leasing',     N'Leasing & Operations',  2, N'fa fa-handshake-o'  UNION ALL
    SELECT 234343652, N'RE Portfolio',   N'Portfolio & Parties',   3, N'fa fa-address-book-o' UNION ALL
    SELECT 234343653, N'RE Maint Group', N'Maintenance',           4, N'fa fa-wrench'        UNION ALL
    SELECT 234343654, N'RE Analytics',   N'Reports & Insights',    5, N'fa fa-line-chart'    UNION ALL
    SELECT 234343655, N'RE Config',      N'Configuration',         6, N'fa fa-cogs'
)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
SELECT CONVERT(nvarchar(450), NEWID()), nm, UPPER(nm), CONVERT(nvarchar(450), NEWID()), @disc, mid, vn, N'#', @root, 0, 0, 0, ord, 1, ic
FROM G WHERE NOT EXISTS (SELECT 1 FROM AppModules a WHERE a.ModulesID = G.mid);

/* ---------- 2. NEW LEAF ROWS (reports not yet in menu + per-group hub overviews) ---------- */
;WITH L(mid, nm, vn, lk, par, ord, ic) AS (
    -- recovered Property reports
    SELECT 234343681, N'RE Txn Ledger',     N'Transactions Ledger', N'/Property/PropertyTransactions/Index',       234343650, 2,  N'fa fa-exchange'  UNION ALL
    SELECT 234343682, N'RE Rpt EmptyUnits', N'Empty Units',         N'/Property/PropertyReports/EmptyUnits',        234343654, 8,  N'fa fa-th'        UNION ALL
    SELECT 234343683, N'RE Rpt Income',     N'Income',              N'/Property/PropertyReports/Income',            234343654, 9,  N'fa fa-line-chart' UNION ALL
    SELECT 234343684, N'RE Rpt RateReturn', N'Rate of Return',      N'/Property/PropertyReports/rateofreturn',      234343654, 10, N'fa fa-percent'   UNION ALL
    SELECT 234343685, N'RE Rpt DocExpiry',  N'Document Expiry',     N'/Property/PropertyReports/documentexpiry',    234343654, 11, N'fa fa-clock-o'   UNION ALL
    -- per-group hub overview landing pages
    SELECT 234343690, N'RE Leasing Hub',    N'Overview',            N'/Property/PropertyHome/LeasingHub',           234343651, 1,  N'fa fa-th-large'  UNION ALL
    SELECT 234343691, N'RE Portfolio Hub',  N'Overview',            N'/Property/PropertyHome/PortfolioHub',         234343652, 1,  N'fa fa-th-large'  UNION ALL
    SELECT 234343692, N'RE Maint Hub',      N'Overview',            N'/Property/PropertyHome/MaintenanceHub',       234343653, 1,  N'fa fa-th-large'  UNION ALL
    SELECT 234343693, N'RE Analytics Hub',  N'Overview',            N'/Property/PropertyHome/InsightsHub',          234343654, 1,  N'fa fa-th-large'  UNION ALL
    SELECT 234343694, N'RE Config Hub',     N'Overview',            N'/Property/PropertyHome/ConfigHub',            234343655, 1,  N'fa fa-th-large' UNION ALL
    SELECT 234343695, N'RE Inspection',     N'Inspection Scheduler', N'/Property/PropertyInsights/InspectionScheduler', 234343653, 7,  N'fa fa-tasks'
)
INSERT INTO AppModules (Id, Name, NormalizedName, ConcurrencyStamp, Discriminator, ModulesID, viewName, Link, Parent, IsParent, Status, addMenu, MenuOrder, Editable, iconClass)
SELECT CONVERT(nvarchar(450), NEWID()), nm, UPPER(nm), CONVERT(nvarchar(450), NEWID()), @disc, mid, vn, lk, par, 1, 0, 0, ord, 1, ic
FROM L WHERE NOT EXISTS (SELECT 1 FROM AppModules a WHERE a.ModulesID = L.mid);

/* ---------- 3. RE-PARENT + RE-ORDER the 49 existing leaves into the new groups ---------- */
;WITH MAP(mid, newparent, neworder) AS (
    -- Dashboard (650)
    SELECT 234343605, 234343650, 1 UNION ALL
    -- Leasing & Operations (651): register -> contract -> proforma -> invoice -> receipt -> payment -> journal -> renewals
    SELECT 234343620,234343651,2 UNION ALL SELECT 234343621,234343651,3 UNION ALL SELECT 234343623,234343651,4 UNION ALL
    SELECT 234343622,234343651,5 UNION ALL SELECT 234343627,234343651,6 UNION ALL SELECT 234343626,234343651,7 UNION ALL
    SELECT 234343625,234343651,8 UNION ALL SELECT 234343665,234343651,9 UNION ALL
    -- Portfolio & Parties (652): property/unit assets then 5 party masters then board/gallery
    SELECT 234343616,234343652,2 UNION ALL SELECT 234343617,234343652,3 UNION ALL SELECT 234343610,234343652,4 UNION ALL
    SELECT 234343611,234343652,5 UNION ALL SELECT 234343612,234343652,6 UNION ALL SELECT 234343614,234343652,7 UNION ALL
    SELECT 234343613,234343652,8 UNION ALL SELECT 234343672,234343652,9 UNION ALL SELECT 234343673,234343652,10 UNION ALL
    -- Maintenance (653)
    SELECT 234343624,234343653,2 UNION ALL SELECT 234343674,234343653,3 UNION ALL SELECT 234343661,234343653,4 UNION ALL
    SELECT 234343675,234343653,5 UNION ALL SELECT 234343636,234343653,6 UNION ALL
    -- Reports & Insights (654): formal reports (2-7), new reports (8-11), insights (12-21)
    SELECT 234343630,234343654,2 UNION ALL SELECT 234343631,234343654,3 UNION ALL SELECT 234343632,234343654,4 UNION ALL
    SELECT 234343633,234343654,5 UNION ALL SELECT 234343634,234343654,6 UNION ALL SELECT 234343635,234343654,7 UNION ALL
    SELECT 234343660,234343654,12 UNION ALL SELECT 234343666,234343654,13 UNION ALL SELECT 234343669,234343654,14 UNION ALL
    SELECT 234343667,234343654,15 UNION ALL SELECT 234343668,234343654,16 UNION ALL SELECT 234343663,234343654,17 UNION ALL
    SELECT 234343664,234343654,18 UNION ALL SELECT 234343670,234343654,19 UNION ALL SELECT 234343662,234343654,20 UNION ALL
    SELECT 234343671,234343654,21 UNION ALL
    -- Configuration (655)
    SELECT 234343615,234343655,2 UNION ALL SELECT 234343641,234343655,3 UNION ALL SELECT 234343642,234343655,4 UNION ALL
    SELECT 234343643,234343655,5 UNION ALL SELECT 234343640,234343655,6 UNION ALL SELECT 234343644,234343655,7 UNION ALL
    SELECT 234343646,234343655,8 UNION ALL SELECT 234343645,234343655,9 UNION ALL SELECT 234343647,234343655,10 UNION ALL
    SELECT 234343648,234343655,11
)
UPDATE a SET a.Parent = m.newparent, a.MenuOrder = m.neworder
FROM AppModules a INNER JOIN MAP m ON a.ModulesID = m.mid
WHERE a.Parent <> m.newparent OR a.MenuOrder <> m.neworder;

/* ---------- 3b. label clarity: the maintenance REPORT leaf collided with the
                  maintenance CONTRACT transaction inside the new Maintenance group ---------- */
UPDATE AppModules SET viewName = N'Maintenance Report'
WHERE ModulesID = 234343636 AND viewName <> N'Maintenance Report';

/* ---------- 4. RETIRE the 5 old group parents (hide, keep for rollback) ---------- */
UPDATE AppModules SET Status = 1
WHERE ModulesID IN (234343601, 234343602, 234343603, 234343604, 234343606) AND Status <> 1;

/* ---------- 5. OPTIONAL: grant every new RE role to a user (default: jenson) ----------
   Comment out / change the UserName as needed. Safe + idempotent. */
DECLARE @uid nvarchar(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'jenson');
IF @uid IS NOT NULL
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    SELECT @uid, m.Id FROM AppModules m
    WHERE m.ModulesID BETWEEN 234343600 AND 234343699
      AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles ur WHERE ur.UserId = @uid AND ur.RoleId = m.Id);

/* ---------- verify ---------- */
SELECT g.viewName AS [Group], COUNT(c.ModulesID) AS Children
FROM AppModules g
LEFT JOIN AppModules c ON c.Parent = g.ModulesID AND c.Status = 0
WHERE g.ModulesID BETWEEN 234343650 AND 234343655
GROUP BY g.viewName, g.MenuOrder
ORDER BY g.MenuOrder;
