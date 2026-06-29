/* ============================================================================
   07_PropertyManagement_TopMenu.sql
   ----------------------------------------------------------------------------
   Promote the "Property Management" menu group to a TOP-LEVEL menu on the main
   bar, immediately AFTER "Human Resources" (before Settings), with its four
   items grouped under it: Landlord, Property, Units, Tenancy Contract.

   The horizontal menu is data-driven from AppModules (HomeController.Menu ->
   Views/Home/Menu.cshtml). A row renders on the bar when Parent = 0; it becomes
   a dropdown when IsParent = choice.Yes (0) AND Link = '#', recursing into rows
   whose Parent = its ModulesID. Visibility also needs addMenu = choice.Yes (0),
   Status = active (0) and the user holding the role named the same as the row.

   IMPORTANT DATA NOTE: ModulesID is NOT unique in AppModules. In affected DBs
   the row "Property Management" and the HR row "leave dashboard" (Leave
   Dashboard) share ModulesID 234343551, so any UPDATE ... WHERE ModulesID = x
   hits both. This script therefore identifies rows by the unique role Name and
   the GUID PK (Id), and first de-collides Leave Dashboard onto a fresh ModulesID
   (re-homed under Human Resources as a leaf link) so the property children
   attach to Property Management only.

   ADDITIVE / IDEMPOTENT: re-running yields the same tree + ordering. No row is
   created or deleted; only re-parented / re-ordered / (for the collision) given
   a fresh ModulesID. Role gating and historical data are untouched.
   ============================================================================ */
SET NOCOUNT ON;

DECLARE @pmId BIGINT = (SELECT TOP 1 ModulesID FROM AppModules WHERE Name = 'Property Management' ORDER BY ModulesID);

IF @pmId IS NULL
BEGIN
    PRINT 'Property Management node not found in this DB - nothing to do.';
END
ELSE
BEGIN
    /* 0) DE-COLLISION: if "leave dashboard" shares ModulesID with Property
          Management, give it a fresh unique ModulesID and re-home it under
          Human Resources as a leaf link to the Leave Dashboard page. Guarded so
          it only fires while the collision exists (idempotent). */
    IF EXISTS (SELECT 1 FROM AppModules WHERE Name = 'leave dashboard' AND ModulesID = @pmId)
    BEGIN
        DECLARE @hrId  BIGINT = (SELECT TOP 1 ModulesID FROM AppModules WHERE Parent = 0 AND Name = 'Human Resources' ORDER BY ModulesID);
        DECLARE @newId BIGINT = (SELECT ISNULL(MAX(ModulesID), 0) + 1 FROM AppModules);
        UPDATE AppModules
            SET ModulesID = @newId,
                Parent    = ISNULL(@hrId, Parent),
                IsParent  = 1,                                 -- leaf (renders as a plain link)
                Link      = '/LeaveRequest/LeaveDashborad',
                addMenu   = 0,
                Status    = 0,
                iconClass = 'fa-circle-o',
                MenuOrder = 11
            WHERE Name = 'leave dashboard';
        PRINT 'Leave Dashboard de-collided from Property Management and re-homed under Human Resources.';
    END

    /* 1) Make Property Management a top-level dropdown (match the sibling shape).
          Target by the unique role Name so the (now former) collision is moot. */
    UPDATE AppModules
        SET Parent    = 0,
            Link      = '#',
            IsParent  = 0,            -- choice.Yes => dropdown when Link = '#'
            addMenu   = 0,            -- choice.Yes => included by the menu query
            Status    = 0,            -- Status.active => visible
            iconClass = 'fa-building'
        WHERE Name = 'Property Management';

    /* 2) HR's current top-level slot; PM sorts right after it. */
    DECLARE @hr INT = (SELECT TOP 1 MenuOrder FROM AppModules WHERE Parent = 0 AND Name = 'Human Resources');
    IF @hr IS NULL
        SET @hr = ISNULL((SELECT MAX(MenuOrder) FROM AppModules WHERE Parent = 0), 0);

    /* Renumber ALL top-level rows deterministically (join on the unique Id PK,
       NOT ModulesID) so PM lands just after HR on every re-run. */
    ;WITH ordered AS (
        SELECT Id,
               ROW_NUMBER() OVER (
                   ORDER BY CASE WHEN Name = 'Property Management'
                                 THEN @hr + 0.5
                                 ELSE CONVERT(decimal(18,1), MenuOrder) END
               ) AS rn
        FROM AppModules
        WHERE Parent = 0
    )
    UPDATE a SET a.MenuOrder = o.rn
    FROM AppModules a JOIN ordered o ON a.Id = o.Id;

    /* 3) Group the four items under Property Management (consolidates any that
          were mis-parented elsewhere, e.g. Property/Tenancy under Contact). */
    UPDATE AppModules SET Parent = @pmId, Status = 0
        WHERE Name IN ('Landlord', 'Property', 'Units', 'Tenancy Contract')
          AND Parent <> @pmId;

    /* 4) Order the children: Landlord, Property, Units, Tenancy Contract. */
    UPDATE AppModules SET MenuOrder = 1 WHERE Parent = @pmId AND Name = 'Landlord';
    UPDATE AppModules SET MenuOrder = 2 WHERE Parent = @pmId AND Name = 'Property';
    UPDATE AppModules SET MenuOrder = 3 WHERE Parent = @pmId AND Name = 'Units';
    UPDATE AppModules SET MenuOrder = 4 WHERE Parent = @pmId AND Name = 'Tenancy Contract';

    PRINT 'Property Management promoted to a top-level menu after Human Resources.';
END
GO
