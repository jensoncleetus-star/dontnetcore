/* ============================================================================
   BOS (Business Operating System) — Custom Invoice Template Designer (2026-06)
   ----------------------------------------------------------------------------
   NEW table only — does NOT touch any existing table, row or print flow.
   Stores user-designed drag-and-drop invoice templates as JSON.
   Idempotent: safe to run repeatedly on any copy.
   ============================================================================ */

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InvoiceTemplate')
BEGIN
    CREATE TABLE [dbo].[InvoiceTemplate]
    (
        [Id]           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_InvoiceTemplate PRIMARY KEY,
        [Name]         NVARCHAR(100)  NOT NULL,
        [DocType]      NVARCHAR(30)   NOT NULL CONSTRAINT DF_InvoiceTemplate_DocType DEFAULT ('Sale'),
        [PaperSize]    NVARCHAR(12)   NOT NULL CONSTRAINT DF_InvoiceTemplate_Paper   DEFAULT ('A4'),
        [Orientation]  NVARCHAR(12)   NOT NULL CONSTRAINT DF_InvoiceTemplate_Orient  DEFAULT ('portrait'),
        [DesignJson]   NVARCHAR(MAX)  NULL,
        [IsDefault]    BIT            NOT NULL CONSTRAINT DF_InvoiceTemplate_IsDef    DEFAULT (0),
        [Status]       INT            NOT NULL CONSTRAINT DF_InvoiceTemplate_Status   DEFAULT (0),  -- 0=active,1=inactive (matches enum Status)
        [CreatedDate]  DATETIME       NOT NULL CONSTRAINT DF_InvoiceTemplate_Created  DEFAULT (GETDATE()),
        [ModifiedDate] DATETIME       NULL
    );
END
GO

/* Seed one editable "Default (Sample)" template if the table is empty. */
IF NOT EXISTS (SELECT 1 FROM [dbo].[InvoiceTemplate])
BEGIN
    INSERT INTO [dbo].[InvoiceTemplate] ([Name],[DocType],[PaperSize],[Orientation],[IsDefault],[Status],[DesignJson])
    VALUES (
        'Default (Sample)', 'Sale', 'A4', 'portrait', 1, 0,
        N'{"paper":"A4","orientation":"portrait","elements":[
            {"id":"s1","type":"field","x":36,"y":28,"w":320,"h":30,"field":"COMPANY_HEADER.Name","fontSize":20,"bold":true,"align":"left"},
            {"id":"s2","type":"field","x":36,"y":58,"w":320,"h":18,"field":"COMPANY_HEADER.Address","fontSize":11,"bold":false,"align":"left"},
            {"id":"s3","type":"field","x":36,"y":76,"w":320,"h":18,"field":"COMPANY_HEADER.TRN","fontSize":11,"bold":false,"align":"left"},
            {"id":"s4","type":"text","x":430,"y":34,"w":300,"h":28,"text":"TAX INVOICE","fontSize":18,"bold":true,"align":"right"},
            {"id":"s5","type":"field","x":36,"y":120,"w":300,"h":18,"field":"CUSTOMER.Name","fontSize":12,"bold":true,"align":"left"},
            {"id":"s6","type":"field","x":430,"y":120,"w":300,"h":18,"field":"INVOICE.Number","fontSize":12,"bold":false,"align":"right"},
            {"id":"s7","type":"field","x":430,"y":140,"w":300,"h":18,"field":"INVOICE.Date","fontSize":12,"bold":false,"align":"right"},
            {"id":"s8","type":"items","x":36,"y":180,"w":724,"h":220,"fontSize":11},
            {"id":"s9","type":"totals","x":430,"y":420,"w":330,"h":120,"fontSize":12}
        ]}'
    );
END
GO

/* Permission role + menu node for the custom designer (idempotent). The ROLE name stays "Invoice Template"
   (gating); the MENU DISPLAY uses viewName="Custom Design", nested under "Company"/Settings (ModulesID 1069),
   linking to the central hub of all document-type designers. AppModules == the Identity role store. */
IF NOT EXISTS (SELECT 1 FROM AppModules WHERE Name = 'Invoice Template')
BEGIN
    DECLARE @mid BIGINT = (SELECT ISNULL(MAX(ModulesID), 900000) + 1 FROM AppModules);
    INSERT INTO AppModules (Id, Name, ModulesID, viewName, Link, Parent, IsParent, Status, Editable, Discriminator, iconClass, addMenu, MenuOrder, NormalizedName, ConcurrencyStamp)
    VALUES (LOWER(CONVERT(varchar(36), NEWID())), 'Invoice Template', @mid, 'Custom Design', '/InvoiceTemplate/Hub', 1069, 0, 0, 0, 'AppModules', 'fa-paint-brush', 1, 11, 'INVOICE TEMPLATE', CONVERT(varchar(36), NEWID()));
END
GO

/* Ensure an existing node shows as "Custom Design" under Settings and points at the hub. */
UPDATE AppModules SET viewName = 'Custom Design', Link = '/InvoiceTemplate/Hub', Parent = 1069, addMenu = 1 WHERE Name = 'Invoice Template';
GO
