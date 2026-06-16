/* ============================================================================
   BOS — ROLLBACK for PropertyDummyData.sql. Removes the sample property data.
   Only run on a COPY/test DB. Deletes ALL rows from these property tables
   (they were empty before the seed, so this clears exactly the seed data).
   ============================================================================ */
SET NOCOUNT ON;
DELETE FROM PropertyUnits;
DELETE FROM PropertyMains;
DELETE FROM PropertyUnitTypes;
DELETE FROM PropertyTypes;
SELECT (SELECT COUNT(*) FROM PropertyMains) AS Properties, (SELECT COUNT(*) FROM PropertyUnits) AS Units;
