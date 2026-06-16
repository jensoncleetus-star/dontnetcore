/* =====================================================================
   Real-Estate advanced features — schema + demo seed (idempotent).
   Safe to run repeatedly. Additive only; never touches existing tables
   except inserting two EnableSettings rows if absent.
   Tables:
     PropertyRentReceipts  -> receipt-based rent collection
     PropertyReminderLogs  -> expiry reminder audit trail
   ===================================================================== */

----------------------------------------------------------------------
-- 1. PropertyRentReceipts
----------------------------------------------------------------------
IF OBJECT_ID('dbo.PropertyRentReceipts','U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyRentReceipts (
        ID          bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ReceiptNo   nvarchar(50)  NULL,
        RentalID    bigint        NOT NULL CONSTRAINT DF_PRR_RentalID DEFAULT(0),
        Tenant      bigint        NOT NULL CONSTRAINT DF_PRR_Tenant   DEFAULT(0),
        Property    bigint        NOT NULL CONSTRAINT DF_PRR_Property DEFAULT(0),
        Unit        bigint        NOT NULL CONSTRAINT DF_PRR_Unit     DEFAULT(0),
        Amount      decimal(18,2) NOT NULL CONSTRAINT DF_PRR_Amount   DEFAULT(0),
        ReceiptDate datetime      NOT NULL CONSTRAINT DF_PRR_RDate    DEFAULT(getdate()),
        Mode        nvarchar(30)  NULL,
        ChequeNo    nvarchar(50)  NULL,
        Note        nvarchar(max) NULL,
        CreatedDate datetime      NOT NULL CONSTRAINT DF_PRR_CDate    DEFAULT(getdate()),
        CreatedBy   nvarchar(450) NULL,
        Status      int           NOT NULL CONSTRAINT DF_PRR_Status   DEFAULT(0)
    );
END

----------------------------------------------------------------------
-- 2. PropertyReminderLogs
----------------------------------------------------------------------
IF OBJECT_ID('dbo.PropertyReminderLogs','U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyReminderLogs (
        ID          bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Kind        nvarchar(40)  NULL,
        RefID       bigint        NOT NULL CONSTRAINT DF_PRL_RefID DEFAULT(0),
        Title       nvarchar(300) NULL,
        ToEmail     nvarchar(300) NULL,
        Subject     nvarchar(300) NULL,
        ExpiryDate  datetime      NULL,
        SentDate    datetime      NOT NULL CONSTRAINT DF_PRL_Sent DEFAULT(getdate()),
        Result      nvarchar(400) NULL
    );
END

----------------------------------------------------------------------
-- 3. Reminder settings (default auto-send OFF for production safety)
--    EnableSettings.Status: 0=active(ON), 1=inactive(OFF)
----------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM EnableSettings WHERE EnableType = 'ReminderAutoSend')
    INSERT INTO EnableSettings (EnableType, Status, TypeValue) VALUES ('ReminderAutoSend', 1, '0');
IF NOT EXISTS (SELECT 1 FROM EnableSettings WHERE EnableType = 'ReminderDaysAhead')
    INSERT INTO EnableSettings (EnableType, Status, TypeValue) VALUES ('ReminderDaysAhead', 0, '30');

----------------------------------------------------------------------
-- 4. DEMO seed: mark older rent invoices as collected (receipt-based).
--    Only seeds when the receipt table is empty so it stays idempotent.
--    Rentals older than 20 days are treated as paid; recent ones remain
--    outstanding, giving a realistic collected-vs-pending split.
----------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM PropertyRentReceipts)
BEGIN
    DECLARE @uid nvarchar(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'jenson');

    INSERT INTO PropertyRentReceipts
        (ReceiptNo, RentalID, Tenant, Property, Unit, Amount, ReceiptDate, Mode, ChequeNo, Note, CreatedDate, CreatedBy, Status)
    SELECT
        'RC-' + RIGHT('00000' + CAST(r.RentalID AS varchar(10)), 5),
        r.RentalID, r.Tenant, r.Property, r.Unit, r.Amount,
        DATEADD(day, 3, r.RDate),
        CASE r.RentalID % 3 WHEN 0 THEN 'Cheque' WHEN 1 THEN 'Bank' ELSE 'Cash' END,
        CASE WHEN r.RentalID % 3 = 0 THEN 'CHQ' + RIGHT('000000' + CAST(r.RentalID AS varchar(10)), 6) ELSE NULL END,
        'Collected (demo seed)',
        getdate(), @uid, 0
    FROM Rentals r
    WHERE r.Status = 0
      AND r.RDate <= DATEADD(day, -20, getdate());
END

----------------------------------------------------------------------
-- 5. PropertyMaintenanceTasks (inspection / maintenance task scheduler)
----------------------------------------------------------------------
IF OBJECT_ID('dbo.PropertyMaintenanceTasks','U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyMaintenanceTasks (
        ID            bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title         nvarchar(200) NULL,
        TaskType      nvarchar(40)  NULL,
        Property      bigint        NOT NULL CONSTRAINT DF_PMT_Property DEFAULT(0),
        Unit          bigint        NOT NULL CONSTRAINT DF_PMT_Unit     DEFAULT(0),
        Contractor    bigint        NOT NULL CONSTRAINT DF_PMT_Contr    DEFAULT(0),
        ScheduledDate datetime      NOT NULL CONSTRAINT DF_PMT_Sched    DEFAULT(getdate()),
        Priority      nvarchar(20)  NULL,
        Status        int           NOT NULL CONSTRAINT DF_PMT_Status   DEFAULT(0),
        Notes         nvarchar(max) NULL,
        CompletedDate datetime      NULL,
        CreatedDate   datetime      NOT NULL CONSTRAINT DF_PMT_CDate    DEFAULT(getdate()),
        CreatedBy     nvarchar(450) NULL
    );
END

-- demo seed: a spread of inspection/maintenance tasks across properties + contractors
IF NOT EXISTS (SELECT 1 FROM PropertyMaintenanceTasks)
BEGIN
    DECLARE @uid2 nvarchar(450) = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'jenson');
    ;WITH P AS (SELECT TOP 6 Id, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PropertyMains),
          C AS (SELECT ContractorID, ROW_NUMBER() OVER (ORDER BY ContractorID) rn FROM Contractors)
    INSERT INTO PropertyMaintenanceTasks (Title, TaskType, Property, Unit, Contractor, ScheduledDate, Priority, Status, Notes, CompletedDate, CreatedDate, CreatedBy)
    SELECT x.Title, x.TaskType, p.Id, 0,
           ISNULL((SELECT ContractorID FROM C WHERE C.rn = ((x.seq % (SELECT COUNT(*) FROM C)) + 1)), 0),
           DATEADD(day, x.dayoff, CAST(GETDATE() AS date)),
           x.Priority, x.Status, x.Notes,
           CASE WHEN x.Status = 2 THEN DATEADD(day, x.dayoff, CAST(GETDATE() AS date)) ELSE NULL END,
           getdate(), @uid2
    FROM P
    CROSS APPLY (VALUES
        (1, N'Annual fire-safety inspection', N'Inspection',  -3, N'High',   0, N'AC + fire systems due for annual check'),
        (2, N'AC servicing (quarterly)',      N'AMC Service',   5, N'Normal', 0, N'Quarterly AC maintenance visit'),
        (3, N'Lift maintenance',              N'Maintenance',  12, N'Normal', 1, N'Contractor on site'),
        (4, N'Common-area cleaning',          N'Cleaning',     -8, N'Low',    2, N'Completed'),
        (5, N'Plumbing repair - 2nd floor',   N'Repair',        2, N'High',   0, N'Reported leak'),
        (6, N'Periodic structural inspection',N'Inspection',   25, N'Normal', 0, N'Six-month structural review')
    ) AS x(seq, Title, TaskType, dayoff, Priority, Status, Notes)
    WHERE x.seq = p.rn;   -- one task per property (round-robins the 6 task templates)
END

SELECT
   (SELECT COUNT(*) FROM PropertyRentReceipts) AS Receipts,
   (SELECT COUNT(*) FROM Rentals WHERE Status = 0) AS Rentals,
   (SELECT COUNT(*) FROM PropertyReminderLogs) AS ReminderLogs,
   (SELECT COUNT(*) FROM PropertyMaintenanceTasks) AS MaintenanceTasks;
