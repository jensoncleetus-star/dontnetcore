/* ============================================================================
   QuickSoft ERP — Security hardening: enable account lockout (audit S7)
   ----------------------------------------------------------------------------
   Identity only enforces the 5-attempts / 5-minute brute-force lockout for
   users whose AspNetUsers.LockoutEnabled = 1. Legacy rows often have 0.
   ADDITIVE + IDEMPOTENT: flips the flag only; clears any stale lock state so
   nobody starts out locked. Run once per branch DB (safe to re-run).
   ============================================================================ */
SET NOCOUNT ON;

IF COL_LENGTH('AspNetUsers', 'LockoutEnabled') IS NOT NULL
BEGIN
    UPDATE AspNetUsers SET LockoutEnabled = 1 WHERE LockoutEnabled = 0;
    PRINT CONCAT('LockoutEnabled set for ', @@ROWCOUNT, ' user(s)');
END

-- start everyone clean: no inherited failed-attempt counters or stale lock timestamps
IF COL_LENGTH('AspNetUsers', 'AccessFailedCount') IS NOT NULL
    UPDATE AspNetUsers SET AccessFailedCount = 0 WHERE AccessFailedCount <> 0;
IF COL_LENGTH('AspNetUsers', 'LockoutEnd') IS NOT NULL
    UPDATE AspNetUsers SET LockoutEnd = NULL WHERE LockoutEnd IS NOT NULL;

PRINT 'lockout hardening complete';
