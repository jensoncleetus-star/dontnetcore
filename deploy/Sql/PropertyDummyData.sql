/* ============================================================================
   BOS — Real Estate DUMMY/SAMPLE DATA (for testing the Property module).
   Inserts: Property Types, Unit Types, 10 Properties, and ~6 Units each
   (flats / villas / studios / offices / shops) — all FK-consistent.
   IDs are identity (auto). Idempotent: skips if PropertyMains already has rows.
   Rollback: deploy/Sql/PropertyDummyData_Delete.sql
   ============================================================================ */
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM PropertyMains)
BEGIN
    SELECT 'PropertyMains already has data — skipped (delete first to reseed).' AS Note;
    RETURN;
END

/* ---- 1. Property Types ---- */
DECLARE @ptype TABLE (ID bigint, Name nvarchar(200));
INSERT INTO PropertyTypes (Name)
OUTPUT inserted.ID, inserted.Name INTO @ptype
VALUES (N'Residential Building'), (N'Villa Compound'), (N'Commercial Tower'), (N'Mixed-Use Development'), (N'Townhouse Complex');

/* ---- 2. Unit Types ---- */
DECLARE @utype TABLE (ID bigint, Name nvarchar(200));
INSERT INTO PropertyUnitTypes (Name)
OUTPUT inserted.ID, inserted.Name INTO @utype
VALUES (N'Flat'), (N'Villa'), (N'Studio'), (N'Office'), (N'Shop'), (N'Penthouse');

/* ---- 3. Properties (10) ---- */
DECLARE @props TABLE (Id bigint, EntryNo bigint, City nvarchar(100));
;WITH P(nm, cd, city, ptName, en) AS (
    SELECT N'Marina Heights Tower',      N'PRP-001', N'Dubai Marina',        N'Residential Building',   1 UNION ALL
    SELECT N'Al Wasl Residences',        N'PRP-002', N'Al Wasl, Dubai',      N'Residential Building',   2 UNION ALL
    SELECT N'Jumeirah Villas Compound',  N'PRP-003', N'Jumeirah, Dubai',     N'Villa Compound',         3 UNION ALL
    SELECT N'Business Bay Offices',      N'PRP-004', N'Business Bay, Dubai', N'Commercial Tower',       4 UNION ALL
    SELECT N'Downtown Boulevard',        N'PRP-005', N'Downtown Dubai',      N'Mixed-Use Development',  5 UNION ALL
    SELECT N'Palm Vista Apartments',     N'PRP-006', N'Palm Jumeirah',       N'Residential Building',   6 UNION ALL
    SELECT N'Silicon Oasis Towers',      N'PRP-007', N'Dubai Silicon Oasis', N'Mixed-Use Development',  7 UNION ALL
    SELECT N'Arabian Ranches Villas',    N'PRP-008', N'Arabian Ranches',     N'Villa Compound',         8 UNION ALL
    SELECT N'Deira City Plaza',          N'PRP-009', N'Deira, Dubai',        N'Commercial Tower',       9 UNION ALL
    SELECT N'Greens Community Homes',    N'PRP-010', N'The Greens, Dubai',   N'Townhouse Complex',     10
)
INSERT INTO PropertyMains (Code, Name, PropertyType, Description, Address, Country, State, City, CreatedDate, CreatedBy, editable, Status, EntryNo)
OUTPUT inserted.Id, inserted.EntryNo, inserted.City INTO @props
SELECT P.cd, P.nm, (SELECT TOP 1 ID FROM @ptype WHERE Name = P.ptName),
       P.nm + N' - a ' + P.ptName, P.nm + N', ' + P.city + N', UAE',
       N'UAE', N'Dubai', P.city, GETDATE(), N'seed', 1, 0, P.en
FROM P;

/* ---- 4. Units (6 per property: 2 flats, 1 villa, 1 studio, 1 office, 1 shop) ---- */
DECLARE @tpl TABLE (seq int, label nvarchar(50), utName nvarchar(50), rent decimal(18,2));
INSERT INTO @tpl (seq, label, utName, rent) VALUES
    (1, N'Flat 101',  N'Flat',   55000),
    (2, N'Flat 102',  N'Flat',   58000),
    (3, N'Villa V1',  N'Villa',  140000),
    (4, N'Studio S1', N'Studio', 38000),
    (5, N'Office O1', N'Office', 95000),
    (6, N'Shop SH1',  N'Shop',   120000);

INSERT INTO PropertyUnits (Name, Code, Property, UnitType, Rent, Deposit, Description, CreatedDate, CreatedBy, editable, Status, EntryNo)
SELECT t.label,
       N'U' + CAST(p.EntryNo AS nvarchar(10)) + N'-' + CAST(t.seq AS nvarchar(10)),
       p.Id,
       (SELECT TOP 1 ID FROM @utype WHERE Name = t.utName),
       t.rent,
       CAST(t.rent * 0.1 AS decimal(18,2)),
       t.utName + N' unit at ' + p.City,
       GETDATE(), N'seed', 1, 0,
       (p.EntryNo - 1) * 6 + t.seq
FROM @props p CROSS JOIN @tpl t;

SELECT (SELECT COUNT(*) FROM PropertyTypes)      AS PropertyTypes,
       (SELECT COUNT(*) FROM PropertyUnitTypes)  AS UnitTypes,
       (SELECT COUNT(*) FROM PropertyMains)      AS Properties,
       (SELECT COUNT(*) FROM PropertyUnits)      AS Units;
