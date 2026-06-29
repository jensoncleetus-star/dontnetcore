-- ============================================================================
-- 05_AccountsReports_Fixes.sql
-- Repairs three DB-side defects that made core Accounts reports throw on the
-- .NET-10 port (Trial Balance, Balance Sheet, P&L, Cashflow). Idempotent;
-- safe to re-run. Run AFTER 04_AVCO_Performance.sql at cutover.
--
-- Companion C# fixes (cashflow IN-list -> IN-subquery) ship in the app binary.
-- See docs/ACCOUNTS-AUDIT.md (batch 10).
-- ============================================================================
SET NOCOUNT ON;

-- ----------------------------------------------------------------------------
-- FIX A: SP_AVCOMethod hardcoded a cross-database item source
--        'auditemirtechlatest.dbo.Items' in its pre-2025 opening-stock branch.
--        That archive DB exists on the legacy production box but is NOT carried
--        to the new server (it is not in the migration set), so Balance Sheet
--        and P&L 500'd with "Invalid object name auditemirtechlatest.dbo.Items".
--        The intended source is the per-company audit DB ('audit'+DB_NAME());
--        e.g. the trading SP wrongly named the SERVICE archive, confirming the
--        literal name is a copy-paste artifact.
--
--        Resilient fix: a synonym dbo.AvcoAuditItems that resolves to the
--        company's own audit DB IF it exists (byte-identical to legacy where the
--        archive is present), else to the local dbo.Items (works everywhere the
--        archive is absent). Then repoint the SP at the synonym.
-- ----------------------------------------------------------------------------
IF OBJECT_ID('dbo.AvcoAuditItems', 'SN') IS NOT NULL
    DROP SYNONYM dbo.AvcoAuditItems;

DECLARE @auditdb sysname = N'audit' + DB_NAME();
IF DB_ID(@auditdb) IS NOT NULL
BEGIN
    DECLARE @syn nvarchar(400) = N'CREATE SYNONYM dbo.AvcoAuditItems FOR [' + @auditdb + N'].dbo.Items';
    EXEC (@syn);
    PRINT 'AvcoAuditItems -> [' + @auditdb + '].dbo.Items (archive present; legacy-faithful)';
END
ELSE
BEGIN
    EXEC ('CREATE SYNONYM dbo.AvcoAuditItems FOR dbo.Items');
    PRINT 'AvcoAuditItems -> dbo.Items (no archive DB; local fallback)';
END

-- Repoint SP_AVCOMethod's stale cross-DB reference at the synonym (only if still present).
DECLARE @def nvarchar(max) = OBJECT_DEFINITION(OBJECT_ID('dbo.SP_AVCOMethod'));
IF @def IS NOT NULL AND @def LIKE '%auditemirtechlatest.dbo.Items%'
BEGIN
    SET @def = REPLACE(@def, 'auditemirtechlatest.dbo.Items', 'dbo.AvcoAuditItems');
    -- Convert the leading CREATE header to ALTER. Whitespace-tolerant: the stored
    -- definition can render as "CREATE   PROCEDURE" (multiple spaces), which a literal
    -- REPLACE(...,'CREATE PROCEDURE',...) silently misses, leaving a plain CREATE that
    -- then collides with the SP that 04 already installed (Msg 2714). The first CREATE
    -- token is the procedure header (this SP has no leading comment), so STUFF it in place.
    DECLARE @cp int = CHARINDEX('CREATE', @def);
    IF @cp > 0 AND @cp < 20
        SET @def = STUFF(@def, @cp, 6, 'ALTER ');
    EXEC (@def);
    PRINT 'SP_AVCOMethod repointed to dbo.AvcoAuditItems';
END
ELSE
    PRINT 'SP_AVCOMethod already clean (no auditemirtechlatest reference)';

-- ----------------------------------------------------------------------------
-- FIX B: trialbalance SP omitted the non-nullable column orderB that the
--        BalanceSheet result class requires (siblings balancesheet/balancesheet2
--        already emit it), so GetTrialBalance / GetTrialBalancefortrial threw
--        "The required column 'orderB' was not present in the results of a
--        FromSql operation." orderB is an ordering seed that is overwritten
--        downstream, so a constant 0 is value-neutral.
-- ----------------------------------------------------------------------------
DECLARE @tb nvarchar(max) = OBJECT_DEFINITION(OBJECT_ID('dbo.trialbalance'));
IF @tb IS NOT NULL AND @tb NOT LIKE '%orderB%'
BEGIN
    SET @tb = REPLACE(@tb, '@AccType as AccType,Temp=0,', '@AccType as AccType,0 as orderB,Temp=0,');
    IF @tb NOT LIKE '%CREATE OR ALTER%'
        SET @tb = REPLACE(REPLACE(@tb, 'CREATE PROCEDURE', 'CREATE OR ALTER PROCEDURE'), 'CREATE PROC ', 'CREATE OR ALTER PROC ');
    EXEC (@tb);
    PRINT 'trialbalance: added 0 as orderB';
END
ELSE
    PRINT 'trialbalance already has orderB';
