/* ============================================================================
   BOS — ROLLBACK for AddRealEstateMenu.sql.
   Removes the Real Estate top menu (ModulesID 234343600-234343699): first any
   user-role assignments pointing at those roles, then the role/menu rows.
   Safe / idempotent.
   ============================================================================ */
SET NOCOUNT ON;

DELETE ur
FROM AspNetUserRoles ur
JOIN AppModules r ON ur.RoleId = r.Id
WHERE r.ModulesID BETWEEN 234343600 AND 234343699;

DELETE FROM AppModules WHERE ModulesID BETWEEN 234343600 AND 234343699;

SELECT COUNT(*) AS RemainingRealEstateRows FROM AppModules WHERE ModulesID BETWEEN 234343600 AND 234343699;
