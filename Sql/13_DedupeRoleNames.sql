/* ============================================================================
   Role-store dedupe — AppModules doubles as the ASP.NET Core Identity role store
   ----------------------------------------------------------------------------
   The legacy EF6 app tolerated duplicate role names in AppModules because its
   role lookup used FirstOrDefault. EF Core Identity's UserStore.FindRoleAsync
   uses SingleOrDefaultAsync(r => r.NormalizedName == ...), which THROWS
   "Sequence contains more than one element" the moment a user is assigned a role
   whose name appears twice (observed on "Invoice Template" during Users/Edit).

   This collapses every duplicate NormalizedName down to one row, keeping the
   lowest MenuOrder (then lowest Id) as the canonical role, and re-points any
   AspNetUserRoles that referenced a removed duplicate. Idempotent: a second run
   finds no duplicates and does nothing.
   ============================================================================ */

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#rank') IS NOT NULL DROP TABLE #rank;
SELECT Id, NormalizedName,
       ROW_NUMBER() OVER (PARTITION BY NormalizedName ORDER BY MenuOrder, Id) AS rn
INTO   #rank
FROM   dbo.AppModules
WHERE  NormalizedName IS NOT NULL;

IF OBJECT_ID('tempdb..#map') IS NOT NULL DROP TABLE #map;
SELECT d.Id AS DupId, k.Id AS KeepId
INTO   #map
FROM   #rank d
JOIN   #rank k ON k.NormalizedName = d.NormalizedName AND k.rn = 1
WHERE  d.rn > 1;

/* Drop user-role rows that would violate the (UserId, RoleId) PK after repoint
   (i.e. the user already holds the keeper role), then repoint the rest. */
DELETE ur
FROM   dbo.AspNetUserRoles ur
JOIN   #map m ON ur.RoleId = m.DupId
WHERE  EXISTS (SELECT 1 FROM dbo.AspNetUserRoles x
               WHERE x.UserId = ur.UserId AND x.RoleId = m.KeepId);

UPDATE ur
SET    ur.RoleId = m.KeepId
FROM   dbo.AspNetUserRoles ur
JOIN   #map m ON ur.RoleId = m.DupId;

/* Remove the duplicate role rows. */
DELETE mo
FROM   dbo.AppModules mo
JOIN   #map m ON mo.Id = m.DupId;

DECLARE @removed INT = @@ROWCOUNT;
PRINT 'Duplicate role rows removed: ' + CONVERT(varchar(10), @removed);

DROP TABLE #rank;
DROP TABLE #map;
GO
