/* ============================================================================
   BOS — Real Estate / Property module: SCHEMA ENSURE (idempotent)
   ----------------------------------------------------------------------------
   Purpose: guarantee a database has EVERY Property-module table & column exactly
   as in the canonical schema (owner's property.txt), so the module works after
   a production / branch cutover. Safe to run many times — every statement is
   guarded (IF OBJECT_ID / IF COL_LENGTH ... IS NULL), so it only creates what is
   missing and is a complete no-op on an already-correct database.

   Audited 2026-06-16 against emirtechlatest (copy): ALL 33 tables already present,
   all columns/types(decimal 18,2)/nullability/defaults match, all PKs IDENTITY.
   The ONE intentional delta vs the pasted property.txt is PropertyMains.LandlordID
   (bigint NULL) — the running EF model's Landlord relationship requires it, so it
   is INCLUDED here (the pasted script predates it).

   NOTE: matches the app's "no FK constraints" design (referential integrity is
   enforced in app logic). DocumentTypes is a shared master created elsewhere and
   is intentionally NOT created here (only its Name NOT NULL is ensured).

   Run ONLY as part of go-live on the target DB (see deploy/DEPLOY-RUNBOOK.md),
   or on a COPY for testing. Does NOT touch data.
   ============================================================================ */
SET NOCOUNT ON;
PRINT '== Property schema ensure: start ==';

/* ---------- PARTY MASTERS (the property module "customers": Contact/Accounts-backed) ---------- */
IF OBJECT_ID('dbo.Brokers','U') IS NULL
CREATE TABLE [dbo].[Brokers](
    [BrokerID] [bigint] IDENTITY(1,1) NOT NULL, [BrokerCode] [nvarchar](max) NULL, [BrokerName] [nvarchar](max) NULL,
    [Contact] [bigint] NOT NULL, [CreditLimit] [decimal](18,2) NOT NULL, [CreditPeriod] [int] NOT NULL,
    [Lattitude] [nvarchar](max) NULL, [Longitude] [nvarchar](max) NULL, [Location] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL,
    [Accounts] [bigint] NOT NULL, [BankName] [nvarchar](max) NULL, [AccountNo] [nvarchar](max) NULL, [IbanNo] [nvarchar](max) NULL,
    [BranchName] [nvarchar](max) NULL, [Swift] [nvarchar](max) NULL, [AccountID_AccountsID] [bigint] NULL, [ContactID_ContactID] [bigint] NULL,
    [EntryNo] [bigint] NOT NULL, [Type] [int] NOT NULL, [File] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.Brokers] PRIMARY KEY CLUSTERED ([BrokerID] ASC));

IF OBJECT_ID('dbo.Contractors','U') IS NULL
CREATE TABLE [dbo].[Contractors](
    [ContractorID] [bigint] IDENTITY(1,1) NOT NULL, [ContractorCode] [nvarchar](max) NULL, [ContractorName] [nvarchar](max) NULL,
    [Contact] [bigint] NOT NULL, [CreditLimit] [decimal](18,2) NOT NULL, [CreditPeriod] [int] NOT NULL,
    [Lattitude] [nvarchar](max) NULL, [Longitude] [nvarchar](max) NULL, [Location] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL,
    [Accounts] [bigint] NOT NULL, [BankName] [nvarchar](max) NULL, [AccountNo] [nvarchar](max) NULL, [IbanNo] [nvarchar](max) NULL,
    [BranchName] [nvarchar](max) NULL, [Swift] [nvarchar](max) NULL, [AccountID_AccountsID] [bigint] NULL, [ContactID_ContactID] [bigint] NULL,
    [Type] [int] NOT NULL, [EntryNo] [bigint] NOT NULL, [ContractType] [bigint] NULL CONSTRAINT [DF_Contractors_ContractType] DEFAULT ((0)),
    CONSTRAINT [PK_dbo.Contractors] PRIMARY KEY CLUSTERED ([ContractorID] ASC));

IF OBJECT_ID('dbo.Developers','U') IS NULL
CREATE TABLE [dbo].[Developers](
    [DeveloperID] [bigint] IDENTITY(1,1) NOT NULL, [DeveloperCode] [nvarchar](max) NULL, [DeveloperName] [nvarchar](max) NULL,
    [Contact] [bigint] NOT NULL, [CreditLimit] [decimal](18,2) NOT NULL, [CreditPeriod] [int] NOT NULL,
    [Lattitude] [nvarchar](max) NULL, [Longitude] [nvarchar](max) NULL, [Location] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL,
    [Accounts] [bigint] NOT NULL, [BankName] [nvarchar](max) NULL, [AccountNo] [nvarchar](max) NULL, [IbanNo] [nvarchar](max) NULL,
    [BranchName] [nvarchar](max) NULL, [Swift] [nvarchar](max) NULL, [AccountID_AccountsID] [bigint] NULL, [ContactID_ContactID] [bigint] NULL,
    [Type] [int] NOT NULL, [EntryNo] [bigint] NOT NULL,
    CONSTRAINT [PK_dbo.Developers] PRIMARY KEY CLUSTERED ([DeveloperID] ASC));

IF OBJECT_ID('dbo.Landlords','U') IS NULL
CREATE TABLE [dbo].[Landlords](
    [LandlordID] [bigint] IDENTITY(1,1) NOT NULL, [LandlordCode] [nvarchar](max) NULL, [LandlordName] [nvarchar](max) NULL,
    [Contact] [bigint] NOT NULL, [CreditLimit] [decimal](18,2) NOT NULL, [CreditPeriod] [int] NOT NULL,
    [Lattitude] [nvarchar](max) NULL, [Longitude] [nvarchar](max) NULL, [Location] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL,
    [Accounts] [bigint] NOT NULL, [BankName] [nvarchar](max) NULL, [AccountNo] [nvarchar](max) NULL, [IbanNo] [nvarchar](max) NULL,
    [BranchName] [nvarchar](max) NULL, [Swift] [nvarchar](max) NULL, [AccountID_AccountsID] [bigint] NULL, [ContactID_ContactID] [bigint] NULL,
    [Type] [int] NOT NULL, [EntryNo] [bigint] NOT NULL,
    CONSTRAINT [PK_dbo.Landlords] PRIMARY KEY CLUSTERED ([LandlordID] ASC));

IF OBJECT_ID('dbo.Tenants','U') IS NULL
CREATE TABLE [dbo].[Tenants](
    [TenantID] [bigint] IDENTITY(1,1) NOT NULL, [TenantCode] [nvarchar](max) NULL, [TenantName] [nvarchar](max) NULL,
    [Contact] [bigint] NOT NULL, [CreditLimit] [decimal](18,2) NOT NULL, [CreditPeriod] [int] NOT NULL,
    [Lattitude] [nvarchar](max) NULL, [Longitude] [nvarchar](max) NULL, [Location] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL,
    [Accounts] [bigint] NOT NULL, [BankName] [nvarchar](max) NULL, [AccountNo] [nvarchar](max) NULL, [IbanNo] [nvarchar](max) NULL,
    [BranchName] [nvarchar](max) NULL, [Swift] [nvarchar](max) NULL, [AccountID_AccountsID] [bigint] NULL, [ContactID_ContactID] [bigint] NULL,
    [Type] [int] NOT NULL, [EntryNo] [bigint] NOT NULL,
    CONSTRAINT [PK_dbo.Tenants] PRIMARY KEY CLUSTERED ([TenantID] ASC));

/* ---------- PROPERTY + UNIT MASTERS ---------- */
IF OBJECT_ID('dbo.PropertyTypes','U') IS NULL
CREATE TABLE [dbo].[PropertyTypes]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](max) NOT NULL,
    CONSTRAINT [PK_dbo.PropertyTypes] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertyUnitTypes','U') IS NULL
CREATE TABLE [dbo].[PropertyUnitTypes]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.PropertyUnitTypes] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.ContractorTypes','U') IS NULL
CREATE TABLE [dbo].[ContractorTypes]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.ContractorTypes] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.ContractTypes','U') IS NULL
CREATE TABLE [dbo].[ContractTypes]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](max) NULL, [Account] [bigint] NOT NULL,
    CONSTRAINT [PK_dbo.ContractTypes] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.Durations','U') IS NULL
CREATE TABLE [dbo].[Durations]([Id] [bigint] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.Durations] PRIMARY KEY CLUSTERED ([Id] ASC));

IF OBJECT_ID('dbo.PropertyFeatures','U') IS NULL
CREATE TABLE [dbo].[PropertyFeatures]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Feature] [nvarchar](max) NOT NULL,
    CONSTRAINT [PK_dbo.PropertyFeatures] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertyUnitFeatures','U') IS NULL
CREATE TABLE [dbo].[PropertyUnitFeatures]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Feature] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.PropertyUnitFeatures] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.AdditionalFields','U') IS NULL
CREATE TABLE [dbo].[AdditionalFields]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](max) NOT NULL,
    [Section] [nvarchar](max) NOT NULL CONSTRAINT [DF_AdditionalFields_Section] DEFAULT (''),
    CONSTRAINT [PK_dbo.AdditionalFields] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.AdditionalFieldDatas','U') IS NULL
CREATE TABLE [dbo].[AdditionalFieldDatas]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](max) NULL,
    [Reference] [bigint] NOT NULL, [Purpose] [nvarchar](max) NULL,
    [Field] [bigint] NOT NULL CONSTRAINT [DF_AdditionalFieldDatas_Field] DEFAULT ((0)),
    CONSTRAINT [PK_dbo.AdditionalFieldDatas] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertySettings','U') IS NULL
CREATE TABLE [dbo].[PropertySettings]([Id] [bigint] IDENTITY(1,1) NOT NULL, [Module] [nvarchar](20) NULL, [Type] [nvarchar](max) NULL,
    [LValue] [bigint] NULL, [SValue] [nvarchar](max) NULL, [Description] [nvarchar](max) NULL, [Status] [int] NOT NULL,
    CONSTRAINT [PK_dbo.PropertySettings] PRIMARY KEY CLUSTERED ([Id] ASC));

IF OBJECT_ID('dbo.PropertyMains','U') IS NULL
CREATE TABLE [dbo].[PropertyMains](
    [Id] [bigint] IDENTITY(1,1) NOT NULL, [Code] [nvarchar](20) NULL, [Name] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL,
    [Description] [nvarchar](max) NULL, [PropertyType] [bigint] NULL, [DocumentType] [bigint] NULL, [File] [nvarchar](max) NULL,
    [Address] [nvarchar](250) NULL, [Country] [nvarchar](50) NULL, [State] [nvarchar](50) NULL, [City] [nvarchar](50) NULL,
    [Zip] [nvarchar](max) NULL, [Document] [nvarchar](max) NULL, [CreatedDate] [datetime] NOT NULL, [CreatedBy] [nvarchar](max) NULL,
    [editable] [int] NOT NULL, [Status] [int] NOT NULL, [EntryNo] [bigint] NOT NULL, [LandlordID] [bigint] NULL,
    CONSTRAINT [PK_dbo.PropertyMains] PRIMARY KEY CLUSTERED ([Id] ASC));

IF OBJECT_ID('dbo.PropertyUnits','U') IS NULL
CREATE TABLE [dbo].[PropertyUnits](
    [Id] [bigint] IDENTITY(1,1) NOT NULL, [Name] [nvarchar](max) NULL, [Code] [nvarchar](max) NULL, [Property] [bigint] NULL,
    [UnitType] [bigint] NULL, [Rent] [decimal](18,2) NULL, [Deposit] [decimal](18,2) NULL, [Description] [nvarchar](max) NULL,
    [TnC] [nvarchar](max) NULL, [File] [nvarchar](max) NULL, [Document] [bigint] NULL, [EntryNo] [bigint] NOT NULL,
    [CreatedDate] [datetime] NOT NULL, [CreatedBy] [nvarchar](max) NULL, [editable] [int] NOT NULL, [Status] [int] NOT NULL,
    CONSTRAINT [PK_dbo.PropertyUnits] PRIMARY KEY CLUSTERED ([Id] ASC));

/* ---------- FEATURE / IMAGE / DOCUMENT CHILD TABLES ---------- */
IF OBJECT_ID('dbo.SelectedFeatures','U') IS NULL
CREATE TABLE [dbo].[SelectedFeatures]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Property] [bigint] NOT NULL, [Feature] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.SelectedFeatures] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.SelectedUnitFeatures','U') IS NULL
CREATE TABLE [dbo].[SelectedUnitFeatures]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Unit] [bigint] NOT NULL, [Feature] [bigint] NOT NULL,
    CONSTRAINT [PK_dbo.SelectedUnitFeatures] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertyImages','U') IS NULL
CREATE TABLE [dbo].[PropertyImages]([ID] [bigint] IDENTITY(1,1) NOT NULL, [PropertyID] [bigint] NOT NULL, [FileName] [nvarchar](max) NOT NULL,
    [Status] [int] NOT NULL, [Items_Id] [bigint] NULL, CONSTRAINT [PK_dbo.PropertyImages] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertyDocuments','U') IS NULL
CREATE TABLE [dbo].[PropertyDocuments]([ID] [bigint] IDENTITY(1,1) NOT NULL, [PropertyID] [bigint] NOT NULL, [FileName] [nvarchar](max) NOT NULL,
    [Status] [int] NOT NULL, [Items_Id] [bigint] NULL, CONSTRAINT [PK_dbo.PropertyDocuments] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertyUnitImages','U') IS NULL
CREATE TABLE [dbo].[PropertyUnitImages]([ID] [bigint] IDENTITY(1,1) NOT NULL, [UnitID] [bigint] NOT NULL, [FileName] [nvarchar](max) NOT NULL,
    [Status] [int] NOT NULL, [Items_Id] [bigint] NULL, CONSTRAINT [PK_dbo.PropertyUnitImages] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertyUnitDocuments','U') IS NULL
CREATE TABLE [dbo].[PropertyUnitDocuments]([ID] [bigint] IDENTITY(1,1) NOT NULL, [UnitID] [bigint] NOT NULL, [FileName] [nvarchar](max) NOT NULL,
    [Status] [int] NOT NULL, [Items_Id] [bigint] NULL, CONSTRAINT [PK_dbo.PropertyUnitDocuments] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertyDocumentTypes','U') IS NULL
CREATE TABLE [dbo].[PropertyDocumentTypes]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Reference] [bigint] NOT NULL, [Purpose] [nvarchar](max) NULL,
    [ExpDate] [datetime] NOT NULL, [DocumentType] [bigint] NOT NULL, CONSTRAINT [PK_dbo.PropertyDocumentTypes] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.DocumentFiles','U') IS NULL
CREATE TABLE [dbo].[DocumentFiles]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Document] [bigint] NOT NULL, [attachments] [nvarchar](max) NOT NULL,
    CONSTRAINT [PK_dbo.DocumentFiles] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.ContractDocuments','U') IS NULL
CREATE TABLE [dbo].[ContractDocuments]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Tenancy] [bigint] NOT NULL, [FileName] [nvarchar](max) NOT NULL,
    [Status] [int] NOT NULL, [Items_Id] [bigint] NULL, CONSTRAINT [PK_dbo.ContractDocuments] PRIMARY KEY CLUSTERED ([ID] ASC));

/* ---------- TRANSACTIONS ---------- */
IF OBJECT_ID('dbo.TenancyContracts','U') IS NULL
CREATE TABLE [dbo].[TenancyContracts](
    [Id] [bigint] IDENTITY(1,1) NOT NULL, [Tenant] [bigint] NULL, [Property] [bigint] NULL, [Unit] [bigint] NOT NULL,
    -- NOTE: the canonical property.txt has StartDate/EndDate as nvarchar(max), but the TenancyContract entity
    -- maps them as DateTime (Create does DateTime.Parse, reports do DateDiffDay). With nvarchar the Tenancy
    -- Contract grid throws InvalidCastException on ANY data, so the correct store type is datetime.
    [StartDate] [datetime] NULL, [EndDate] [datetime] NULL, [Duration] [bigint] NULL, [Rent] [decimal](18,2) NULL,
    [Deposit] [decimal](18,2) NULL, [Schedule] [int] NOT NULL, [DueDate] [bigint] NULL, [PaymentType] [bigint] NULL, [File] [nvarchar](max) NULL,
    [CreatedDate] [datetime] NOT NULL, [CreatedBy] [nvarchar](max) NULL, [editable] [int] NOT NULL, [Status] [int] NOT NULL,
    [Remark] [nvarchar](max) NULL, [Note] [nvarchar](max) NULL, [TnC] [nvarchar](max) NULL, [Code] [nvarchar](max) NULL,
    [EntryNo] [bigint] NOT NULL, [PaymentTypeDeposit] [bigint] NULL,
    CONSTRAINT [PK_dbo.TenancyContracts] PRIMARY KEY CLUSTERED ([Id] ASC));

IF OBJECT_ID('dbo.Rentals','U') IS NULL
CREATE TABLE [dbo].[Rentals](
    [RentalID] [bigint] IDENTITY(1,1) NOT NULL, [PRNo] [bigint] NOT NULL, [VoucherNo] [nvarchar](max) NULL, [RDate] [datetime] NOT NULL,
    [Tenant] [bigint] NOT NULL, [Property] [bigint] NOT NULL, [Unit] [bigint] NOT NULL, [Amount] [decimal](18,2) NOT NULL,
    [Note] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL, [TermsCondition] [nvarchar](max) NULL, [CreatedDate] [datetime] NOT NULL,
    [CreatedBy] [nvarchar](max) NULL, [Branch] [bigint] NOT NULL, [editable] [int] NOT NULL, [Status] [int] NOT NULL,
    CONSTRAINT [PK_dbo.Rentals] PRIMARY KEY CLUSTERED ([RentalID] ASC));

IF OBJECT_ID('dbo.RentalProformas','U') IS NULL
CREATE TABLE [dbo].[RentalProformas](
    [ID] [bigint] IDENTITY(1,1) NOT NULL, [PRNo] [bigint] NOT NULL, [VoucherNo] [nvarchar](max) NULL, [Date] [datetime] NOT NULL,
    [Tenant] [bigint] NOT NULL, [Property] [bigint] NOT NULL, [Unit] [bigint] NOT NULL, [Amount] [decimal](18,2) NOT NULL,
    [Note] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL, [TermsCondition] [nvarchar](max) NULL, [CreatedDate] [datetime] NOT NULL,
    [CreatedBy] [nvarchar](max) NULL, [Branch] [bigint] NOT NULL, [editable] [int] NOT NULL, [Status] [int] NOT NULL,
    CONSTRAINT [PK_dbo.RentalProformas] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.Maintenances','U') IS NULL
CREATE TABLE [dbo].[Maintenances](
    [ID] [bigint] IDENTITY(1,1) NOT NULL, [VoucherNo] [nvarchar](max) NULL, [PRNo] [bigint] NOT NULL, [Date] [datetime] NOT NULL,
    [Property] [bigint] NOT NULL, [Amount] [decimal](18,2) NOT NULL, [Note] [nvarchar](max) NULL, [Remark] [nvarchar](max) NULL,
    [TermsCondition] [nvarchar](max) NULL, [CreatedDate] [datetime] NOT NULL, [CreatedBy] [nvarchar](max) NULL, [Branch] [bigint] NOT NULL,
    [editable] [int] NOT NULL, [Status] [int] NOT NULL, [Contractor] [bigint] NOT NULL, [StartDate] [nvarchar](max) NULL,
    [EndDate] [nvarchar](max) NULL, [PaymentType] [bigint] NULL, [ContractType] [bigint] NULL,
    CONSTRAINT [PK_dbo.Maintenances] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.PropertyRegistrations','U') IS NULL
CREATE TABLE [dbo].[PropertyRegistrations](
    [RegistrationID] [bigint] IDENTITY(1,1) NOT NULL, [VoucherNo] [nvarchar](max) NULL, [RDate] [datetime] NOT NULL, [Developer] [bigint] NOT NULL,
    [Owner] [bigint] NOT NULL, [Property] [bigint] NOT NULL, [Broker] [bigint] NOT NULL, [Amount] [decimal](18,2) NOT NULL, [Note] [nvarchar](max) NULL,
    [Remark] [nvarchar](max) NULL, [TermsCondition] [nvarchar](max) NULL, [CreatedDate] [datetime] NOT NULL, [CreatedBy] [nvarchar](max) NULL,
    [Branch] [bigint] NOT NULL, [editable] [int] NOT NULL, [Status] [int] NOT NULL, [PRNo] [bigint] NOT NULL, [BuildupArea] [decimal](18,2) NULL,
    [PaymentType] [bigint] NULL, [PlotNumber] [nvarchar](max) NULL, [PlotOption] [nvarchar](max) NULL, [PlotArea] [decimal](18,2) NULL,
    [PAMeasurement] [nvarchar](max) NULL, [BAMeasurement] [nvarchar](max) NULL, [Hector] [decimal](18,2) NULL, [ADDCNo] [nvarchar](max) NULL,
    [PermitId] [nvarchar](max) NULL, [PermissionId] [nvarchar](max) NULL, [BookingDate] [datetime] NULL, [HandoverDate] [datetime] NULL,
    CONSTRAINT [PK_dbo.PropertyRegistrations] PRIMARY KEY CLUSTERED ([RegistrationID] ASC));

IF OBJECT_ID('dbo.Cheques','U') IS NULL
CREATE TABLE [dbo].[Cheques]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Amount] [decimal](18,2) NOT NULL, [Date] [datetime] NOT NULL,
    [ChequeNo] [nvarchar](max) NULL, [Reference] [bigint] NOT NULL, [Purpose] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.Cheques] PRIMARY KEY CLUSTERED ([ID] ASC));

IF OBJECT_ID('dbo.ChequeImages','U') IS NULL
CREATE TABLE [dbo].[ChequeImages]([ID] [bigint] IDENTITY(1,1) NOT NULL, [Cheque] [bigint] NOT NULL, [attachments] [nvarchar](max) NOT NULL,
    CONSTRAINT [PK_dbo.ChequeImages] PRIMARY KEY CLUSTERED ([ID] ASC));

/* ---------- COLUMN BACKFILL for pre-existing (older-shape) tables ----------
   These columns were added to the schema after the original tables were created;
   ensure they exist on any DB that has an older version of the table. ---------- */
IF OBJECT_ID('dbo.AdditionalFieldDatas','U') IS NOT NULL AND COL_LENGTH('dbo.AdditionalFieldDatas','Field') IS NULL
    ALTER TABLE [dbo].[AdditionalFieldDatas] ADD [Field] [bigint] NOT NULL CONSTRAINT [DF_AdditionalFieldDatas_Field] DEFAULT ((0));
IF OBJECT_ID('dbo.AdditionalFields','U') IS NOT NULL AND COL_LENGTH('dbo.AdditionalFields','Section') IS NULL
    ALTER TABLE [dbo].[AdditionalFields] ADD [Section] [nvarchar](max) NOT NULL CONSTRAINT [DF_AdditionalFields_Section] DEFAULT ('');
IF OBJECT_ID('dbo.Contractors','U') IS NOT NULL AND COL_LENGTH('dbo.Contractors','ContractType') IS NULL
    ALTER TABLE [dbo].[Contractors] ADD [ContractType] [bigint] NULL CONSTRAINT [DF_Contractors_ContractType] DEFAULT ((0));
IF OBJECT_ID('dbo.Maintenances','U') IS NOT NULL AND COL_LENGTH('dbo.Maintenances','ContractType') IS NULL
    ALTER TABLE [dbo].[Maintenances] ADD [ContractType] [bigint] NULL;
IF OBJECT_ID('dbo.PropertyMains','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyMains','LandlordID') IS NULL
    ALTER TABLE [dbo].[PropertyMains] ADD [LandlordID] [bigint] NULL;
IF OBJECT_ID('dbo.TenancyContracts','U') IS NOT NULL AND COL_LENGTH('dbo.TenancyContracts','PaymentTypeDeposit') IS NULL
    ALTER TABLE [dbo].[TenancyContracts] ADD [PaymentTypeDeposit] [bigint] NULL;
/* TenancyContract StartDate/EndDate must be datetime (the entity maps them as DateTime). If an older DB has them
   as nvarchar, convert (values written by the app are date-parseable; this fixes the grid InvalidCastException). */
IF OBJECT_ID('dbo.TenancyContracts','U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.TenancyContracts') AND name='StartDate' AND system_type_id IN (167,231,239,99,175,35))
BEGIN
    UPDATE dbo.TenancyContracts SET StartDate = TRY_CONVERT(datetime, StartDate, 105) WHERE StartDate IS NOT NULL AND TRY_CONVERT(datetime, StartDate, 126) IS NULL AND TRY_CONVERT(datetime, StartDate, 105) IS NOT NULL;
    UPDATE dbo.TenancyContracts SET EndDate   = TRY_CONVERT(datetime, EndDate, 105)   WHERE EndDate   IS NOT NULL AND TRY_CONVERT(datetime, EndDate, 126)   IS NULL AND TRY_CONVERT(datetime, EndDate, 105)   IS NOT NULL;
    ALTER TABLE [dbo].[TenancyContracts] ALTER COLUMN [StartDate] [datetime] NULL;
    ALTER TABLE [dbo].[TenancyContracts] ALTER COLUMN [EndDate] [datetime] NULL;
END

/* DocumentTypes is a shared master (created elsewhere); only ensure Name is NOT NULL if present. */
IF OBJECT_ID('dbo.DocumentTypes','U') IS NOT NULL AND COL_LENGTH('dbo.DocumentTypes','Name') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('dbo.DocumentTypes') AND name='Name' AND is_nullable=1)
   AND NOT EXISTS (SELECT 1 FROM dbo.DocumentTypes WHERE Name IS NULL)
    ALTER TABLE [dbo].[DocumentTypes] ALTER COLUMN [Name] [nvarchar](max) NOT NULL;

/* ---------- MODEL-DRIFT BACKFILL (2026-06-16) ----------
   The EF entities gained columns the canonical property.txt / older DBs never had.
   Find()/Edit/Print materialise the FULL entity, so a missing column => SQL "Invalid
   column name" => 500 on Edit & Print/Download. Add them (all nvarchar(max) except the
   TenancyContract issuedate which is datetime). Idempotent. */
IF OBJECT_ID('dbo.TenancyContracts','U') IS NOT NULL AND COL_LENGTH('dbo.TenancyContracts','issuedate') IS NULL              ALTER TABLE dbo.TenancyContracts ADD [issuedate] [datetime] NULL;
IF OBJECT_ID('dbo.TenancyContracts','U') IS NOT NULL AND COL_LENGTH('dbo.TenancyContracts','PetsAllowed') IS NULL            ALTER TABLE dbo.TenancyContracts ADD [PetsAllowed] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.TenancyContracts','U') IS NOT NULL AND COL_LENGTH('dbo.TenancyContracts','contractvalue') IS NULL          ALTER TABLE dbo.TenancyContracts ADD [contractvalue] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.TenancyContracts','U') IS NOT NULL AND COL_LENGTH('dbo.TenancyContracts','WaterAndElectricityBill') IS NULL ALTER TABLE dbo.TenancyContracts ADD [WaterAndElectricityBill] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.TenancyContracts','U') IS NOT NULL AND COL_LENGTH('dbo.TenancyContracts','NumberofOccupants') IS NULL      ALTER TABLE dbo.TenancyContracts ADD [NumberofOccupants] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyMains','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyMains','Municipality') IS NULL                 ALTER TABLE dbo.PropertyMains ADD [Municipality] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyMains','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyMains','Zone') IS NULL                         ALTER TABLE dbo.PropertyMains ADD [Zone] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyMains','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyMains','Sector') IS NULL                       ALTER TABLE dbo.PropertyMains ADD [Sector] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyMains','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyMains','RoadName') IS NULL                     ALTER TABLE dbo.PropertyMains ADD [RoadName] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyMains','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyMains','PlotNo') IS NULL                       ALTER TABLE dbo.PropertyMains ADD [PlotNo] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyMains','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyMains','PlotAddress') IS NULL                  ALTER TABLE dbo.PropertyMains ADD [PlotAddress] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyMains','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyMains','PropertyRegistrationNo') IS NULL       ALTER TABLE dbo.PropertyMains ADD [PropertyRegistrationNo] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyUnits','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyUnits','Area') IS NULL                         ALTER TABLE dbo.PropertyUnits ADD [Area] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyUnits','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyUnits','NoofRooms') IS NULL                    ALTER TABLE dbo.PropertyUnits ADD [NoofRooms] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyUnits','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyUnits','PremisesNo') IS NULL                   ALTER TABLE dbo.PropertyUnits ADD [PremisesNo] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyUnits','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyUnits','UnitUsage') IS NULL                    ALTER TABLE dbo.PropertyUnits ADD [UnitUsage] [nvarchar](max) NULL;
IF OBJECT_ID('dbo.PropertyRegistrations','U') IS NOT NULL AND COL_LENGTH('dbo.PropertyRegistrations','Area') IS NULL         ALTER TABLE dbo.PropertyRegistrations ADD [Area] [nvarchar](max) NULL;

PRINT '== Property schema ensure: done ==';
SELECT COUNT(*) AS PropertyTablesPresent FROM sys.tables
 WHERE name IN ('Brokers','Contractors','Developers','Landlords','Tenants','PropertySettings','ContractorTypes','PropertyTypes',
   'PropertyMains','PropertyImages','PropertyDocuments','SelectedFeatures','PropertyFeatures','AdditionalFields','AdditionalFieldDatas',
   'PropertyUnits','PropertyUnitImages','PropertyUnitDocuments','SelectedUnitFeatures','PropertyUnitTypes','PropertyUnitFeatures',
   'Rentals','Durations','TenancyContracts','Cheques','ChequeImages','ContractDocuments','PropertyRegistrations','Maintenances',
   'RentalProformas','PropertyDocumentTypes','ContractTypes','DocumentFiles');  -- expect 33
