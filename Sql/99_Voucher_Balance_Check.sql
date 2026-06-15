-- ============================================================================
-- 99_Voucher_Balance_Check.sql — REPORT-ONLY accounting-invariant check.
-- Changes nothing. Run any time (go-live week, month-end) to watch the books.
-- A "voucher" is every AccountsTransactions group (Purpose, reference);
-- double-entry says each voucher's debits must equal its credits.
--
-- KNOWN LEGACY CONVENTIONS (expected, not new defects — see docs/ACCOUNTS-AUDIT.md):
--   * 'Stock Adjustment' and 'Opening Balance' vouchers are posted ONE-SIDED by the
--     legacy product (no offsetting leg), so they always appear here.
--   * Payment/Receipt discounts pair across two purposes: check the PAIRED queries.
--   * A small historical population of 'Sale' vouchers is off by fils-level rounding.
-- WHAT TO WATCH: the LAST query (recent vouchers). New rows after go-live should be
-- balanced except the documented one-sided purposes.
-- ============================================================================
SET NOCOUNT ON;

PRINT '== global ledger totals ==';
SELECT CAST(SUM(Debit) AS DECIMAL(18,2))  AS total_debit,
       CAST(SUM(Credit) AS DECIMAL(18,2)) AS total_credit,
       CAST(SUM(Debit)-SUM(Credit) AS DECIMAL(18,2)) AS delta
FROM AccountsTransactions;

PRINT '== unbalanced vouchers by purpose (|D-C| > 0.005) ==';
SELECT Purpose, COUNT(*) AS vouchers,
       CAST(SUM(diff) AS DECIMAL(18,2)) AS total_diff,
       MAX(latest) AS latest_created
FROM (SELECT Purpose, reference, SUM(Debit)-SUM(Credit) AS diff, MAX(CreatedDate) AS latest
      FROM AccountsTransactions GROUP BY Purpose, reference
      HAVING ABS(SUM(Debit)-SUM(Credit)) > 0.005) t
GROUP BY Purpose ORDER BY COUNT(*) DESC;

PRINT '== discount PAIRS (these should balance as pairs) ==';
SELECT 'Receipt + Discount Allowed' AS pair, COUNT(*) AS unbalanced_refs
FROM (SELECT reference FROM AccountsTransactions WHERE Purpose IN ('Receipt','Discount Allowed')
      GROUP BY reference HAVING ABS(SUM(Debit)-SUM(Credit)) > 0.005) t
UNION ALL
SELECT 'Payment + Discount Received', COUNT(*)
FROM (SELECT reference FROM AccountsTransactions WHERE Purpose IN ('Payment','Discount Received')
      GROUP BY reference HAVING ABS(SUM(Debit)-SUM(Credit)) > 0.005) t;

PRINT '== discount account heads (new rows should land 497=Allowed / 498=Received) ==';
SELECT a.Account, b.Name, a.Purpose, COUNT(*) AS rows,
       CAST(SUM(a.Debit) AS DECIMAL(18,2)) AS dr, CAST(SUM(a.Credit) AS DECIMAL(18,2)) AS cr,
       MAX(a.CreatedDate) AS latest
FROM AccountsTransactions a JOIN Accounts b ON b.AccountsID = a.Account
WHERE a.Account IN (497,498) AND a.Purpose IN ('Discount Allowed','Discount Received')
GROUP BY a.Account, b.Name, a.Purpose ORDER BY a.Account, a.Purpose;

PRINT '== unbalanced vouchers created in the LAST 14 DAYS (watch this after go-live) ==';
-- This list should stay EMPTY for healthy day-to-day postings. Discount legs post under a
-- different Purpose than their parent (Receipt+Discount Allowed / Payment+Discount Received) but
-- share the document reference, so we fold them into one "voucher family" before checking balance
-- -- otherwise every legitimate discounted receipt/payment would show as a false positive.
WITH v AS (
    SELECT CASE WHEN Purpose IN ('Receipt','Discount Allowed')  THEN 'Receipt+Discount'
                WHEN Purpose IN ('Payment','Discount Received')  THEN 'Payment+Discount'
                ELSE Purpose END AS vfamily,
           reference, Debit, Credit, CreatedDate
    FROM AccountsTransactions)
SELECT vfamily, reference, CAST(SUM(Debit)-SUM(Credit) AS DECIMAL(18,2)) AS diff, MAX(CreatedDate) AS created
FROM v GROUP BY vfamily, reference
HAVING ABS(SUM(Debit)-SUM(Credit)) > 0.005 AND MAX(CreatedDate) > DATEADD(day,-14,GETDATE())
   AND vfamily NOT IN ('Stock Adjustment','Opening Balance')   -- documented one-sided conventions
ORDER BY MAX(CreatedDate) DESC;
