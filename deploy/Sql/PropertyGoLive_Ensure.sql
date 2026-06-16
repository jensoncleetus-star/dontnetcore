/* ============================================================================
   PROPERTY / REAL-ESTATE — PRODUCTION GO-LIVE (schema + config ONLY, NO demo data)
   Safe to run on the LIVE / existing database. 100% ADDITIVE + IDEMPOTENT:
   creates the new feature tables + two reminder config rows. It does NOT insert,
   update or delete ANY of your existing business data, and it loads NO demo data.

   RUN THIS ALONGSIDE (full production order — all are additive + idempotent):
     1. PropertySchema_Ensure.sql        -- adds the model columns the app needs (TenancyContract etc.)
     2. PropertyGoLive_Ensure.sql        -- THIS FILE: 3 new feature tables + reminder config
     3. AddRealEstateMenu.sql            -- the Real Estate menu rows (AppModules)
     4. ReorganizeRealEstateMenu_v2.sql  -- the 6-group menu structure + Inspection Scheduler leaf
   Then: deploy the app (publish folder) and grant the Real-Estate roles to your real users
   in User management (these menu rows default OFF / per-user).

   DO **NOT** run on production:  PropertyDummyDataFull.sql, PropertyDummyData.sql,
   PropertyTrendData.sql  -- those are DEMO data for the test copy only.
   ============================================================================ */
SET NOCOUNT ON;

/* ---- 1. Receipt-based rent collection ---- */
IF OBJECT_ID('dbo.PropertyRentReceipts','U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyRentReceipts (
        ID          bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ReceiptNo   nvarchar(50)  NULL,
        RentalID    bigint        NOT NULL CONSTRAINT DF_PRRg_RentalID DEFAULT(0),
        Tenant      bigint        NOT NULL CONSTRAINT DF_PRRg_Tenant   DEFAULT(0),
        Property    bigint        NOT NULL CONSTRAINT DF_PRRg_Property DEFAULT(0),
        Unit        bigint        NOT NULL CONSTRAINT DF_PRRg_Unit     DEFAULT(0),
        Amount      decimal(18,2) NOT NULL CONSTRAINT DF_PRRg_Amount   DEFAULT(0),
        ReceiptDate datetime      NOT NULL CONSTRAINT DF_PRRg_RDate    DEFAULT(getdate()),
        Mode        nvarchar(30)  NULL,
        ChequeNo    nvarchar(50)  NULL,
        Note        nvarchar(max) NULL,
        CreatedDate datetime      NOT NULL CONSTRAINT DF_PRRg_CDate    DEFAULT(getdate()),
        CreatedBy   nvarchar(450) NULL,
        Status      int           NOT NULL CONSTRAINT DF_PRRg_Status   DEFAULT(0)
    );
END

/* ---- 2. Expiry reminder log ---- */
IF OBJECT_ID('dbo.PropertyReminderLogs','U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyReminderLogs (
        ID          bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Kind        nvarchar(40)  NULL,
        RefID       bigint        NOT NULL CONSTRAINT DF_PRLg_RefID DEFAULT(0),
        Title       nvarchar(300) NULL,
        ToEmail     nvarchar(300) NULL,
        Subject     nvarchar(300) NULL,
        ExpiryDate  datetime      NULL,
        SentDate    datetime      NOT NULL CONSTRAINT DF_PRLg_Sent DEFAULT(getdate()),
        Result      nvarchar(400) NULL
    );
END

/* ---- 3. Inspection / maintenance task scheduler ---- */
IF OBJECT_ID('dbo.PropertyMaintenanceTasks','U') IS NULL
BEGIN
    CREATE TABLE dbo.PropertyMaintenanceTasks (
        ID            bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Title         nvarchar(200) NULL,
        TaskType      nvarchar(40)  NULL,
        Property      bigint        NOT NULL CONSTRAINT DF_PMTg_Property DEFAULT(0),
        Unit          bigint        NOT NULL CONSTRAINT DF_PMTg_Unit     DEFAULT(0),
        Contractor    bigint        NOT NULL CONSTRAINT DF_PMTg_Contr    DEFAULT(0),
        ScheduledDate datetime      NOT NULL CONSTRAINT DF_PMTg_Sched    DEFAULT(getdate()),
        Priority      nvarchar(20)  NULL,
        Status        int           NOT NULL CONSTRAINT DF_PMTg_Status   DEFAULT(0),
        Notes         nvarchar(max) NULL,
        CompletedDate datetime      NULL,
        CreatedDate   datetime      NOT NULL CONSTRAINT DF_PMTg_CDate    DEFAULT(getdate()),
        CreatedBy     nvarchar(450) NULL
    );
END

/* ---- 4. Reminder settings (auto-send OFF by default — owner enables when ready) ---- */
IF NOT EXISTS (SELECT 1 FROM EnableSettings WHERE EnableType = 'ReminderAutoSend')
    INSERT INTO EnableSettings (EnableType, Status, TypeValue) VALUES ('ReminderAutoSend', 1, '0');
IF NOT EXISTS (SELECT 1 FROM EnableSettings WHERE EnableType = 'ReminderDaysAhead')
    INSERT INTO EnableSettings (EnableType, Status, TypeValue) VALUES ('ReminderDaysAhead', 0, '30');

SELECT 'PropertyRentReceipts'    AS TableName, COUNT(*) AS Rows FROM PropertyRentReceipts
UNION ALL SELECT 'PropertyReminderLogs',     COUNT(*) FROM PropertyReminderLogs
UNION ALL SELECT 'PropertyMaintenanceTasks', COUNT(*) FROM PropertyMaintenanceTasks;
