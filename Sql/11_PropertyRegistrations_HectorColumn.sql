/* ============================================================================
   11_PropertyRegistrations_HectorColumn.sql
   ----------------------------------------------------------------------------
   PropertyRegistrations.Hector is a decimal column, but the PropertyRegistration
   entity (and its view model / form) map Hector as a string. Saving a property
   registration sends an nvarchar parameter to the decimal column ->
   SqlException: "Error converting data type nvarchar to numeric" on
   db.SaveChanges().

   Convert the column to nvarchar to match the model. Existing decimal values
   convert cleanly to their text form. Idempotent: only acts while the column is
   still numeric.
   ============================================================================ */
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON c.user_type_id=t.user_type_id
           WHERE c.object_id=OBJECT_ID('PropertyRegistrations') AND c.name='Hector'
             AND t.name IN ('decimal','numeric','money','float'))
BEGIN
    ALTER TABLE PropertyRegistrations ALTER COLUMN Hector nvarchar(100) NULL;
    PRINT 'PropertyRegistrations.Hector -> nvarchar(100)';
END
ELSE
    PRINT 'PropertyRegistrations.Hector already non-numeric';
GO
