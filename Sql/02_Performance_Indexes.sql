/* ============================================================================
   QuickSoft ERP — Performance indexes (Modernization Phase 3, 2026-06-12)
   ----------------------------------------------------------------------------
   ADDITIVE + IDEMPOTENT: creates an index only if the table+columns exist and
   the index doesn't. Targets the hottest query shapes measured during the
   .NET 10 migration (account balances, line-item sums, approval-status joins,
   latest-update-per-task lookups, convert-status subqueries, list filters).
   Safe to run on every branch DB (and re-run any time). No data is modified.
   ============================================================================ */
SET NOCOUNT ON;

DECLARE @sql nvarchar(max);

/* helper pattern: each block checks table + columns + missing index */

-- 1. Account balances: SUM(Debit/Credit) per Account filtered by Status/Date — used by
--    customer/supplier ledgers, balance sheet, trial balance, ~60 report subqueries.
IF OBJECT_ID('AccountsTransactions') IS NOT NULL
   AND COL_LENGTH('AccountsTransactions','Account') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AccTrans_Account_Status' AND object_id=OBJECT_ID('AccountsTransactions'))
BEGIN
    CREATE INDEX IX_AccTrans_Account_Status ON AccountsTransactions(Account, Status)
        INCLUDE (Debit, Credit, Date);
    PRINT 'created IX_AccTrans_Account_Status';
END

-- 2. Project-scoped P&L sums (property/project reports).
IF OBJECT_ID('AccountsTransactions') IS NOT NULL
   AND COL_LENGTH('AccountsTransactions','Project') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_AccTrans_Project_Status' AND object_id=OBJECT_ID('AccountsTransactions'))
BEGIN
    CREATE INDEX IX_AccTrans_Project_Status ON AccountsTransactions(Project, Status)
        INCLUDE (Account, Debit, Credit, Date);
    PRINT 'created IX_AccTrans_Project_Status';
END

-- 3. Invoice line sums: SUM(ItemSubTotal/Tax) per SalesEntry (Type=0 = real lines) —
--    VAT reports, gross-amount columns, header recompute.
IF OBJECT_ID('SEItems') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SEItems_SalesEntry_Type' AND object_id=OBJECT_ID('SEItems'))
BEGIN
    CREATE INDEX IX_SEItems_SalesEntry_Type ON SEItems(SalesEntry, Type)
        INCLUDE (ItemSubTotal, ItemTaxAmount, ItemTotalAmount, Item, ItemQuantity);
    PRINT 'created IX_SEItems_SalesEntry_Type';
END

-- 4. Quotation line sums per Quotation.
IF OBJECT_ID('QuotationItems') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_QuotItems_Quotation' AND object_id=OBJECT_ID('QuotationItems'))
BEGIN
    CREATE INDEX IX_QuotItems_Quotation ON QuotationItems(Quotation)
        INCLUDE (ItemSubTotal, ItemTaxAmount, ItemTotalAmount, Item, ItemQuantity);
    PRINT 'created IX_QuotItems_Quotation';
END

-- 5/6. Approval-status lookups: every transaction grid joins Approvals/ApprovalUpdates
--      by (TransEntry, Type/TransType) — the single most repeated join in the app.
IF OBJECT_ID('Approvals') IS NOT NULL
   AND COL_LENGTH('Approvals','TransEntry') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Approvals_TransEntry_Type' AND object_id=OBJECT_ID('Approvals'))
BEGIN
    SET @sql = N'CREATE INDEX IX_Approvals_TransEntry_Type ON Approvals(TransEntry'
             + CASE WHEN COL_LENGTH('Approvals','Type') IS NOT NULL THEN N', [Type]' ELSE N'' END
             + N') INCLUDE (EmployeeId)';
    EXEC sp_executesql @sql;
    PRINT 'created IX_Approvals_TransEntry_Type';
END
IF OBJECT_ID('ApprovalUpdates') IS NOT NULL
   AND COL_LENGTH('ApprovalUpdates','TransEntry') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ApprovalUpd_TransEntry_Type' AND object_id=OBJECT_ID('ApprovalUpdates'))
BEGIN
    SET @sql = N'CREATE INDEX IX_ApprovalUpd_TransEntry_Type ON ApprovalUpdates(TransEntry'
             + CASE WHEN COL_LENGTH('ApprovalUpdates','Type') IS NOT NULL THEN N', [Type]' ELSE N'' END
             + N') INCLUDE (ApprovalStatus, ApprovedBy, CreatedDate)';
    EXEC sp_executesql @sql;
    PRINT 'created IX_ApprovalUpd_TransEntry_Type';
END

-- 7. Latest-update-per-task (ProTask lists compute MAX(CreatedDate) per ProTaskId client-side).
IF OBJECT_ID('ProTaskUpdations') IS NOT NULL
   AND COL_LENGTH('ProTaskUpdations','ProTaskId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ProTaskUpd_Task_Created' AND object_id=OBJECT_ID('ProTaskUpdations'))
BEGIN
    CREATE INDEX IX_ProTaskUpd_Task_Created ON ProTaskUpdations(ProTaskId, CreatedDate DESC);
    PRINT 'created IX_ProTaskUpd_Task_Created';
END

-- 8. Task-assignee lookups (AssignedTo columns on task/AMC/lead lists).
IF OBJECT_ID('TaskAssigneds') IS NOT NULL
   AND COL_LENGTH('TaskAssigneds','ProTaskId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_TaskAssigned_Task' AND object_id=OBJECT_ID('TaskAssigneds'))
BEGIN
    CREATE INDEX IX_TaskAssigned_Task ON TaskAssigneds(ProTaskId) INCLUDE (EmployeeId);
    PRINT 'created IX_TaskAssigned_Task';
END

-- 9. Delivery-note → invoice conversion subquery (ConvertType + ConvertNo equality).
--    ConvertType/ConvertNo are nvarchar(max) on some branch DBs (not indexable as keys) →
--    only create when both are bounded types; otherwise the optimizer keeps using the PK scan.
IF OBJECT_ID('SalesEntries') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('SalesEntries') AND name='ConvertType' AND max_length <> -1)
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('SalesEntries') AND name='ConvertNo'  AND max_length <> -1)
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SalesEntries_Convert' AND object_id=OBJECT_ID('SalesEntries'))
BEGIN
    CREATE INDEX IX_SalesEntries_Convert ON SalesEntries(ConvertType, ConvertNo) INCLUDE (BillNo);
    PRINT 'created IX_SalesEntries_Convert';
END
ELSE IF OBJECT_ID('SalesEntries') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SalesEntries_Convert' AND object_id=OBJECT_ID('SalesEntries'))
    PRINT 'skipped IX_SalesEntries_Convert (Convert columns are MAX types)';

-- 10. Invoice list filters (date + customer are the default list predicates).
IF OBJECT_ID('SalesEntries') IS NOT NULL
   AND COL_LENGTH('SalesEntries','SEDate') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SalesEntries_Date' AND object_id=OBJECT_ID('SalesEntries'))
BEGIN
    CREATE INDEX IX_SalesEntries_Date ON SalesEntries(SEDate) INCLUDE (Customer, SEGrandTotal, BillNo);
    PRINT 'created IX_SalesEntries_Date';
END

-- 11. Contact lookups by relation (customer contact/mobile search).
IF OBJECT_ID('ContactRelations') IS NOT NULL
   AND COL_LENGTH('ContactRelations','RelationID') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ContactRel_Relation' AND object_id=OBJECT_ID('ContactRelations'))
BEGIN
    SET @sql = N'CREATE INDEX IX_ContactRel_Relation ON ContactRelations(RelationID'
             + CASE WHEN COL_LENGTH('ContactRelations','RelationType') IS NOT NULL THEN N', RelationType' ELSE N'' END
             + N') INCLUDE (ContactID)';
    EXEC sp_executesql @sql;
    PRINT 'created IX_ContactRel_Relation';
END

-- 12. Employee-by-user (every request resolves the logged-in employee). UserId is nvarchar(max)
--     on the legacy schema (not indexable); Employees is small, so a scan is fine — create only
--     where the column is bounded.
IF OBJECT_ID('Employees') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID('Employees') AND name='UserId' AND max_length <> -1)
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Employees_UserId' AND object_id=OBJECT_ID('Employees'))
BEGIN
    CREATE INDEX IX_Employees_UserId ON Employees(UserId) INCLUDE (EmployeeId, FirstName, LastName);
    PRINT 'created IX_Employees_UserId';
END
ELSE IF OBJECT_ID('Employees') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Employees_UserId' AND object_id=OBJECT_ID('Employees'))
    PRINT 'skipped IX_Employees_UserId (UserId is a MAX type)';

-- 13-15. SP_AVCOMethod (AVCO stock valuation) cursors loop PER ITEM over the movement tables,
--        filtering by the item id each iteration. SEItems + StockTransferItems already have an
--        Item index; add the missing ones so each cursor step is a seek, not a scan. (Pure speed,
--        the accounting SP logic is untouched.)
IF OBJECT_ID('PEItems') IS NOT NULL
   AND COL_LENGTH('PEItems','Item') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PEItems_Item' AND object_id=OBJECT_ID('PEItems'))
BEGIN
    CREATE INDEX IX_PEItems_Item ON PEItems(Item)
        INCLUDE (PurchaseEntry, ItemQuantity, ItemUnitPrice, ItemTotalAmount);
    PRINT 'created IX_PEItems_Item';
END

IF OBJECT_ID('StockAdjustments') IS NOT NULL
   AND COL_LENGTH('StockAdjustments','ItemID') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_StockAdj_Item' AND object_id=OBJECT_ID('StockAdjustments'))
BEGIN
    CREATE INDEX IX_StockAdj_Item ON StockAdjustments(ItemID)
        INCLUDE (ItemQuantity, AdjDate, Status);
    PRINT 'created IX_StockAdj_Item';
END

IF OBJECT_ID('SRItems') IS NOT NULL
   AND COL_LENGTH('SRItems','Item') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SRItems_Item' AND object_id=OBJECT_ID('SRItems'))
BEGIN
    CREATE INDEX IX_SRItems_Item ON SRItems(Item)
        INCLUDE (SalesReturnId, ItemQuantity, ItemUnitPrice, ItemTotalAmount);
    PRINT 'created IX_SRItems_Item';
END

PRINT 'index pass complete';
