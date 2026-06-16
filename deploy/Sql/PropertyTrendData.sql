/* ============================================================================
   BOS — Real Estate DEMO data for the Financial Trend (month-wise income/expense).
   Adds monthly rental invoices (per active tenancy contract, last 8 months) and
   spreads existing maintenance dates across months, so the trend chart shows a
   real curve. Idempotent. COPY/TEST DB ONLY.
   ============================================================================ */
SET NOCOUNT ON;
DECLARE @uid nvarchar(450) = '405c5575-2d86-4c34-9255-9603b9462184';
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id=@uid) SET @uid=(SELECT TOP 1 Id FROM AspNetUsers WHERE UserName LIKE '%jenson%');

/* monthly rental invoices (income) across the last 8 months */
IF NOT EXISTS (SELECT 1 FROM Rentals WHERE VoucherNo LIKE 'RI-M%')
  INSERT INTO Rentals (PRNo, VoucherNo, RDate, Tenant, Property, Unit, Amount, Note, CreatedDate, CreatedBy, Branch, editable, Status)
  SELECT 200 + ROW_NUMBER() OVER (ORDER BY tc.Id, n.m),
         'RI-M' + CAST(ROW_NUMBER() OVER (ORDER BY tc.Id, n.m) AS varchar(6)),
         DATEADD(DAY, 2, DATEADD(MONTH, -n.m, GETDATE())),
         tc.Tenant, tc.Property, tc.Unit,
         CAST(ISNULL(tc.Rent, 60000) / 12.0 AS decimal(18,2)),
         'Monthly rent', GETDATE(), @uid, 1, 1, 0
  FROM TenancyContracts tc
  CROSS JOIN (VALUES (0),(1),(2),(3),(4),(5),(6),(7)) AS n(m)
  WHERE tc.Status = 0;

/* spread existing maintenance expense across the last 8 months */
UPDATE Maintenances SET [Date] = DATEADD(DAY, 5, DATEADD(MONTH, -((ID - 1) % 8), GETDATE()));

SELECT (SELECT COUNT(*) FROM Rentals) AS Rentals, (SELECT COUNT(*) FROM Maintenances) AS Maintenances;
