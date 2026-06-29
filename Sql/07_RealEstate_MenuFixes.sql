/* ============================================================================
   07_RealEstate_MenuFixes.sql
   ----------------------------------------------------------------------------
   Real-estate menu is now served by the "Real Estate" top-bar menu
   (Views/Shared/_RealEstateMenu.cshtml, rendered from Views/Home/Menu.cshtml).
   This script makes the two DB-side adjustments that go with it:

   (1) DE-COLLISION (bug fix, keep): ModulesID is NOT unique in AppModules. The
       row "Property Management" and the HR row "leave dashboard" (Leave
       Dashboard) shared ModulesID 234343551, so the property children
       (Landlord/Property/Units/Tenancy) also rendered under HR's Leave
       Dashboard. Give Leave Dashboard a fresh ModulesID and re-home it under
       Human Resources as a leaf link to its page.

   (2) RETIRE the old data-driven "Property Management" menu + its four items
       (Landlord, Property, Units, Tenancy Contract) by marking them inactive,
       so they no longer appear anywhere in the menu now that the "Real Estate"
       top-bar menu replaces them. Status only affects menu rendering — the
       underlying Identity roles are untouched, so authorization on the Property
       controllers and the Real Estate menu's role gate keep working.

   Identify rows by the unique role Name (ModulesID is not unique).
   ADDITIVE / IDEMPOTENT: re-running yields the same state. No row is created or
   deleted; historical data and role membership are untouched.
   ============================================================================ */
SET NOCOUNT ON;

DECLARE @pmId BIGINT = (SELECT TOP 1 ModulesID FROM AppModules WHERE Name = 'Property Management' ORDER BY ModulesID);

/* (1) DE-COLLISION: only fires while "leave dashboard" still shares the
       Property Management ModulesID (idempotent). */
IF @pmId IS NOT NULL AND EXISTS (SELECT 1 FROM AppModules WHERE Name = 'leave dashboard' AND ModulesID = @pmId)
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

/* (2) RETIRE the old Property Management menu + its four items (now replaced by
       the Real Estate top-bar menu). Status = 1 (inactive) hides them from the
       menu render; the roles themselves stay intact. */
UPDATE AppModules
    SET Status = 1
    WHERE Name IN ('Property Management', 'Landlord', 'Property', 'Units', 'Tenancy Contract')
      AND Status <> 1;
PRINT 'Old Property Management menu + items retired (hidden); Real Estate menu replaces them.';
GO
