/* ============================================================================
   09_TenancyContracts_DateColumns.sql
   ----------------------------------------------------------------------------
   TenancyContracts.StartDate / EndDate were nvarchar(max) but the TenancyContract
   entity maps them as DateTime. EF inserts a DateTime (SQL stores it as a date
   string in the nvarchar column) but on read can't cast the string back to
   DateTime -> System.InvalidCastException when listing tenancy contracts
   (TenancyContract / PropertyReports grids). The table is empty on most copies,
   so it only surfaces once a contract exists.

   Convert both columns to datetime2 (EF Core's native mapping for DateTime;
   the stored strings carry 7 fractional digits, which datetime rejects but
   datetime2 accepts). Idempotent: only acts while the column is still character
   type; TRY_CONVERT(datetime2) nulls any genuinely unparseable values first so
   the ALTER cannot fail on stray/empty strings.
   ============================================================================ */
SET NOCOUNT ON;

IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON c.user_type_id=t.user_type_id
           WHERE c.object_id=OBJECT_ID('TenancyContracts') AND c.name='StartDate'
             AND t.name IN ('nvarchar','varchar','nchar','char'))
BEGIN
    UPDATE TenancyContracts SET StartDate = NULL WHERE StartDate IS NOT NULL AND TRY_CONVERT(datetime2, StartDate) IS NULL;
    ALTER TABLE TenancyContracts ALTER COLUMN StartDate datetime2 NULL;
    PRINT 'TenancyContracts.StartDate -> datetime2';
END
ELSE PRINT 'TenancyContracts.StartDate already non-character';

IF EXISTS (SELECT 1 FROM sys.columns c JOIN sys.types t ON c.user_type_id=t.user_type_id
           WHERE c.object_id=OBJECT_ID('TenancyContracts') AND c.name='EndDate'
             AND t.name IN ('nvarchar','varchar','nchar','char'))
BEGIN
    UPDATE TenancyContracts SET EndDate = NULL WHERE EndDate IS NOT NULL AND TRY_CONVERT(datetime2, EndDate) IS NULL;
    ALTER TABLE TenancyContracts ALTER COLUMN EndDate datetime2 NULL;
    PRINT 'TenancyContracts.EndDate -> datetime2';
END
ELSE PRINT 'TenancyContracts.EndDate already non-character';
GO
