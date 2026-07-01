/* ============================================================================
   10_RealEstate_DataDrivenMenu.sql
   ----------------------------------------------------------------------------
   Move the hardcoded property menu (_PropertyMenu / _RealEstateMenu: Property
   Management / Master / Transactions / Reports + Create-List sub-items) into
   AppModules as data-driven children of the "Real Estate" top-level node
   (ModulesID 234343600), assign the new roles to every user who already holds
   the "Real Estate" role, and deactivate the team's hub children so only this
   old structure shows. The hardcoded second-row menu is hidden in the view
   (_PropertyMenu.cshtml) as a companion change.

   Idempotent: the whole insert runs only if it hasn't already (guarded on the
   marker role 'RE2 2 Master'); role grants + hub deactivation use NOT EXISTS /
   Status guards. Identify by the unique role Name.
   ============================================================================ */
SET NOCOUNT ON;

DECLARE @reId    BIGINT = 234343600;                                   -- Real Estate top node ModulesID
DECLARE @reRole  NVARCHAR(450) = (SELECT Id FROM AppModules WHERE ModulesID=@reId AND Parent=0 AND Name='Real Estate');

IF @reRole IS NOT NULL AND NOT EXISTS (SELECT 1 FROM AppModules WHERE Name='RE2 2 Master')
BEGIN
    DECLARE @base BIGINT = (SELECT MAX(ModulesID) FROM AppModules);    -- new ModulesIDs = @base + seq

    DECLARE @m TABLE(seq INT, viewName NVARCHAR(100), Link NVARCHAR(200), parentSeq INT, isLeaf BIT);
    INSERT INTO @m(seq,viewName,Link,parentSeq,isLeaf) VALUES
    -- under Real Estate
    (1,'Property Management','/Property/PropertyHome/Index',0,1),
    (2,'Master','#',0,0),(3,'Transactions','#',0,0),(4,'Reports','#',0,0),
    -- Master groups
    (5,'Landlords','#',2,0),(6,'Tenant','#',2,0),(7,'Developer','#',2,0),(8,'Contractor','#',2,0),
    (9,'Broker','#',2,0),(10,'Property','#',2,0),(11,'Units','#',2,0),
    (12,'Bank Accounts','/Master/BankAccounts',2,1),(13,'Account','/Accounts/Index',2,1),
    (14,'Account Group','/Master/AccountsGroup',2,1),(15,'BillSundry','/BillSundry/Index',2,1),
    -- Master leaves
    (16,'Create','/Property/Landlords/Create',5,1),(17,'List','/Property/Landlords/Index',5,1),
    (18,'Create','/Customer/Create',6,1),(19,'List','/Customer/Index',6,1),
    (20,'Create','/Property/Developer/Create',7,1),(21,'List','/Property/Developer/Index',7,1),
    (22,'Create','/Property/Contractor/Create',8,1),(23,'List','/Property/Contractor/Index',8,1),
    (24,'Create','/Property/Broker/Create',9,1),(25,'List','/Property/Broker/Index',9,1),
    (26,'Property Type','/Property/PropertyType/',10,1),(27,'Property','/Property/PropertyMain/',10,1),
    (28,'Create','/Property/Unit/Create',11,1),(29,'List','/Property/Unit/',11,1),
    -- Transactions groups
    (30,'Property Registration','#',3,0),(31,'Tenancy Contract','#',3,0),(32,'Rental Invoice','#',3,0),
    (33,'PDC Regularize','/PDCRegularise/Index',3,1),(34,'Rental Proforma','#',3,0),
    (35,'Maintenance Contract','#',3,0),(36,'Journal','#',3,0),(37,'Payment','#',3,0),(38,'Receipt','#',3,0),
    -- Transactions leaves
    (39,'Create','/Property/PropertyRegistration/Create',30,1),(40,'List','/Property/PropertyRegistration/Index',30,1),
    (41,'Add','/Property/TenancyContract/Create',31,1),(42,'List','/Property/TenancyContract/Index',31,1),
    (43,'Add','/Property/Rental/Create',32,1),(44,'List','/Property/Rental/Index',32,1),
    (45,'Create','/Property/RentalProforma/Create',34,1),(46,'List','/Property/RentalProforma/Index',34,1),
    (47,'Create','/Property/Maintenance/Create',35,1),(48,'List','/Property/Maintenance/Index',35,1),
    (49,'Create','/Property/PJournalV/Create',36,1),(50,'List','/Property/PJournalV/Index',36,1),
    (51,'Create','/Property/PPayment/Create',37,1),(52,'List','/Property/PPayment/Index',37,1),
    (53,'Create','/Property/PReceipt/Create',38,1),(54,'List','/Property/PReceipt/Index',38,1),
    -- Reports leaves
    (55,'Property Summery','/Property/PropertyReports/PropertySummery',4,1),
    (56,'Property Report','/Property/PropertyReports/PropertyConsolidated',4,1),
    (57,'Property Based P&L','/MyReports/PLSummaryProperty',4,1),
    (58,'Property Registration','/Property/PropertyReports/PropertyRegistration',4,1),
    (59,'Tenancy Contract','/Property/PropertyReports/TenancyContract',4,1),
    (60,'Rental Invoice','/Property/PropertyReports/RentalInvoice',4,1),
    (61,'Maintance Contract','/Property/PropertyReports/Maintance',4,1),
    (62,'Document Expiry','/Property/PropertyReports/documentexpiry',4,1),
    (63,'Empty Units','/Property/PropertyReports/EmptyUnits',4,1),
    (64,'Expense','/Property/PropertyReports/Expense',4,1),
    (65,'Income','/Property/PropertyReports/Income',4,1),
    (66,'Payment','/Property/PropertyReports/Payment',4,1),
    (67,'Receipt','/Property/PropertyReports/Receipt',4,1),
    (68,'Journal','/Property/PropertyReports/Journal',4,1);

    INSERT INTO AppModules (Id, Name, ModulesID, viewName, Link, Parent, IsParent, Status,
                            Editable, Discriminator, iconClass, addMenu, MenuOrder, NormalizedName, ConcurrencyStamp)
    SELECT LOWER(CONVERT(varchar(36),NEWID())),
           'RE2 ' + CONVERT(varchar,seq) + ' ' + viewName,           -- unique role Name
           @base + seq,
           viewName,
           Link,
           CASE WHEN parentSeq = 0 THEN @reId ELSE @base + parentSeq END,
           CASE WHEN isLeaf = 1 THEN 1 ELSE 0 END,                   -- leaf => IsParent=1; dropdown => 0 (choice.Yes)
           0,                                                        -- Status active
           0,
           'AppModules',
           CASE WHEN seq = 1 THEN 'fa-dashboard' WHEN seq IN (2,3,4) THEN 'fa-th' ELSE 'fa-circle-o' END,
           0,                                                        -- addMenu = choice.Yes
           seq,                                                      -- MenuOrder (siblings ordered by seq)
           UPPER('RE2 ' + CONVERT(varchar,seq) + ' ' + viewName),
           CONVERT(varchar(36),NEWID())
    FROM @m;

    PRINT 'Inserted data-driven Real Estate property menu rows.';
END
ELSE
    PRINT 'Real Estate data-driven menu already present (or RE node missing) - insert skipped.';
GO

/* Grant every new RE2 role to all users who already hold the "Real Estate" role. */
DECLARE @reRole2 NVARCHAR(450) = (SELECT Id FROM AppModules WHERE ModulesID=234343600 AND Parent=0 AND Name='Real Estate');
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.UserId, n.Id
FROM (SELECT UserId FROM AspNetUserRoles WHERE RoleId=@reRole2) u
CROSS JOIN (SELECT Id FROM AppModules WHERE Name LIKE 'RE2 %') n
WHERE NOT EXISTS (SELECT 1 FROM AspNetUserRoles x WHERE x.UserId=u.UserId AND x.RoleId=n.Id);
PRINT 'Granted RE2 menu roles to Real Estate users.';
GO

/* Hide the team's hub children so only the old structure shows under Real Estate. */
UPDATE AppModules SET Status=1
WHERE ModulesID IN (234343650,234343651,234343652,234343653,234343654,234343655) AND Status<>1;
PRINT 'Deactivated Real Estate hub children.';
GO
