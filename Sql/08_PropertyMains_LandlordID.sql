/* ============================================================================
   08_PropertyMains_LandlordID.sql
   ----------------------------------------------------------------------------
   The PropertyMain entity (AreaModels/PropertyMain.cs) maps a nullable
   LandlordID, and the Property module reads/writes it (PropertyMainController
   Create/Edit, PropertyInsights, TenancyContract, reports). The PropertyMains
   table was missing the column, so inserting/updating a property threw
   SqlException: Invalid column name 'LandlordID' (DbUpdateException on
   db.SaveChanges).

   Add the column to match the model. ADDITIVE / IDEMPOTENT: nullable, no
   default, no backfill — existing rows are unaffected and historical data is
   preserved. Safe to re-run.
   ============================================================================ */
SET NOCOUNT ON;

IF COL_LENGTH('PropertyMains', 'LandlordID') IS NULL
BEGIN
    ALTER TABLE PropertyMains ADD LandlordID bigint NULL;
    PRINT 'PropertyMains.LandlordID added.';
END
ELSE
    PRINT 'PropertyMains.LandlordID already present.';
GO
