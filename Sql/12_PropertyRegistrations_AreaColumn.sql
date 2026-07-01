/* ============================================================================
   12_PropertyRegistrations_AreaColumn.sql
   ----------------------------------------------------------------------------
   The PropertyRegistration entity maps a string Area (AreaModels/
   PropertyRegistration.cs), and PropertyRegistrationController.Create sets
   preg.Area, but the PropertyRegistrations table has no Area column ->
   SqlException: Invalid column name 'Area' on db.SaveChanges().

   Add the column to match the model. Additive / idempotent: nullable, no
   backfill; existing rows unaffected.
   ============================================================================ */
SET NOCOUNT ON;

IF COL_LENGTH('PropertyRegistrations', 'Area') IS NULL
BEGIN
    ALTER TABLE PropertyRegistrations ADD Area nvarchar(100) NULL;
    PRINT 'PropertyRegistrations.Area added.';
END
ELSE
    PRINT 'PropertyRegistrations.Area already present.';
GO
