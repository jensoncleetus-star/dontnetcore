/* ============================================================================
   ROLLBACK for ReorganizeRealEstateMenu_v2.sql — restores the classic
   Master/Transactions/Reports/Settings/Insights layout exactly.
   Confined to 234343600-234343699. Idempotent.
   ============================================================================ */
SET NOCOUNT ON;

/* 1. restore every leaf to its original parent + order (arithmetic = mid - decade base) */
UPDATE AppModules SET Parent=234343601, MenuOrder=ModulesID-234343609 WHERE ModulesID BETWEEN 234343610 AND 234343617;  -- Master
UPDATE AppModules SET Parent=234343602, MenuOrder=ModulesID-234343619 WHERE ModulesID BETWEEN 234343620 AND 234343627;  -- Transactions
UPDATE AppModules SET Parent=234343603, MenuOrder=ModulesID-234343629 WHERE ModulesID BETWEEN 234343630 AND 234343636;  -- Reports
UPDATE AppModules SET Parent=234343604, MenuOrder=ModulesID-234343639 WHERE ModulesID BETWEEN 234343640 AND 234343648;  -- Settings
UPDATE AppModules SET Parent=234343606, MenuOrder=ModulesID-234343659 WHERE ModulesID BETWEEN 234343660 AND 234343675;  -- Insights
UPDATE AppModules SET Parent=234343600, MenuOrder=0 WHERE ModulesID = 234343605;                                        -- Dashboard leaf

/* 2. re-activate the old group parents */
UPDATE AppModules SET Status = 0 WHERE ModulesID IN (234343601, 234343602, 234343603, 234343604, 234343606);

/* 3. delete the v2 rows (group parents + new leaves) and their role assignments */
DECLARE @new TABLE (mid bigint);
INSERT INTO @new VALUES
 (234343650),(234343651),(234343652),(234343653),(234343654),(234343655),
 (234343681),(234343682),(234343683),(234343684),(234343685),
 (234343690),(234343691),(234343692),(234343693),(234343694);

DELETE ur FROM AspNetUserRoles ur
  INNER JOIN AppModules m ON ur.RoleId = m.Id
  INNER JOIN @new n ON m.ModulesID = n.mid;
DELETE FROM AppModules WHERE ModulesID IN (SELECT mid FROM @new);

SELECT COUNT(*) AS RealEstateRowsRemaining FROM AppModules WHERE ModulesID BETWEEN 234343600 AND 234343699;
