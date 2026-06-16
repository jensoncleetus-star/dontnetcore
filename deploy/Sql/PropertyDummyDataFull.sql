/* ============================================================================
   BOS — Real Estate / Property module: FULL DUMMY DATA (all areas)
   ----------------------------------------------------------------------------
   Seeds Settings, the 5 party masters (Landlords/Tenants/Developers/Contractors/
   Brokers — each with a Contacts + Accounts + Mobile row, the way the app's
   Create action builds them), property Features links, document-expiry rows, and
   the property Transactions (Property Registration / Tenancy Contract / Rental
   Invoice / Rental Proforma / Maintenance Contract).

   Builds on PropertyDummyData.sql (5 PropertyTypes, 6 UnitTypes, 10 Properties,
   60 Units must already exist — run that first if needed).

   Account.CreatedBy is stamped with jenson's UserId so the party grids'
   permission gate (userpermission || Account.CreatedBy == current user) shows
   them. NO FK constraints exist on these tables (app-logic integrity only).

   Idempotent: each section is guarded by IF NOT EXISTS, so re-running is a no-op.
   NOT seeded: Journal / Payment / Receipt — those post balanced double-entry to
   the chart of accounts; seeding them wrong corrupts the ledger/financial reports
   (create one manually in-app to test those screens).

   COPY/TEST DB ONLY. Rollback: PropertyDummyDataFull_Delete.sql
   ============================================================================ */
SET NOCOUNT ON;
DECLARE @uid nvarchar(450) = '405c5575-2d86-4c34-9255-9603b9462184'; -- jenson@gmail.com
DECLARE @branch bigint = 1;
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = @uid)
    SET @uid = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName LIKE '%jenson%' OR Email LIKE '%jenson%');

/* ============================ SETTINGS ============================ */
IF NOT EXISTS (SELECT 1 FROM PropertyFeatures)
  INSERT INTO PropertyFeatures (Feature) VALUES (N'Swimming Pool'),(N'Gymnasium'),(N'Covered Parking'),(N'24/7 Security'),(N'Central A/C'),(N'Children Play Area');
IF NOT EXISTS (SELECT 1 FROM PropertyUnitFeatures)
  INSERT INTO PropertyUnitFeatures (Feature) VALUES (N'Built-in Wardrobes'),(N'Kitchen Appliances'),(N'Sea View'),(N'Maid Room'),(N'Fully Furnished'),(N'Private Balcony');
IF NOT EXISTS (SELECT 1 FROM ContractorTypes)
  INSERT INTO ContractorTypes (Name) VALUES (N'Plumbing'),(N'Electrical'),(N'HVAC / A/C'),(N'General Maintenance'),(N'Cleaning');
IF NOT EXISTS (SELECT 1 FROM Durations)
  INSERT INTO Durations (Name) VALUES (N'Monthly'),(N'Quarterly'),(N'Half-Yearly'),(N'Yearly'),(N'2 Years');
IF NOT EXISTS (SELECT 1 FROM ContractTypes)
  INSERT INTO ContractTypes (Name, Account) VALUES (N'Annual Maintenance Contract',0),(N'One-Time Service',0),(N'Warranty Service',0);
IF NOT EXISTS (SELECT 1 FROM AdditionalFields)
  INSERT INTO AdditionalFields (Name, Section) VALUES (N'Parking Slot No',N'Unit'),(N'Plot Number',N'Property'),(N'Emirates ID',N'Tenant'),(N'Trade License',N'Developer');
IF NOT EXISTS (SELECT 1 FROM PropertySettings)
  INSERT INTO PropertySettings (Module, [Type], Status) VALUES (N'Property',N'General',0),(N'Rental',N'General',0),(N'Maintenance',N'General',0);

/* ============================ PARTY MASTERS ============================
   Helper pattern (set-based): insert Contacts -> capture (ContactID,Name);
   insert one Account per contact -> capture (AccountsID,Name); insert party
   rows joining on Name; insert one Mobile per contact. (Names unique per type.) */

/* ---- LANDLORDS (5) ---- */
IF NOT EXISTS (SELECT 1 FROM Landlords)
BEGIN
  DECLARE @lc TABLE (cid bigint, nm nvarchar(200));
  INSERT INTO Contacts (Name, Address, City, [State], Country, Phone, EmailId, [Group], Status, CountryID)
  OUTPUT inserted.ContactID, inserted.Name INTO @lc (cid, nm)
  SELECT nm, city + N', UAE', city, N'Dubai', N'UAE', phone, email, 2, 0, 0 FROM (VALUES
    (N'Ahmed Al Maktoum',     N'Dubai Marina',  N'0501100001', N'ahmed.landlord@example.ae'),
    (N'Fatima Holdings LLC',  N'Business Bay',  N'0501100002', N'fatima.holdings@example.ae'),
    (N'Mohammed Bin Rashid',  N'Jumeirah',      N'0501100003', N'mbr.landlord@example.ae'),
    (N'Gulf Real Estate Co',  N'Downtown',      N'0501100004', N'gulf.re@example.ae'),
    (N'Sara Al Habtoor',      N'Palm Jumeirah', N'0501100005', N'sara.landlord@example.ae')
  ) AS L(nm, city, phone, email);
  DECLARE @la TABLE (aid bigint, nm nvarchar(200));
  INSERT INTO Accounts (Name, PrintName, [Group], OpnBalance, OpnBalanceCr, PrevBalance, PrevBalanceCr, PrvYearBalance, CreatedDate, CreatedBy, Status, Editable, Branch)
  OUTPUT inserted.AccountsID, inserted.Name INTO @la (aid, nm)
  SELECT nm, nm, 0,0,0,0,0,0, GETDATE(), @uid, 0, 1, @branch FROM @lc;
  ;WITH X AS (SELECT c.cid, a.aid, c.nm, ROW_NUMBER() OVER (ORDER BY c.cid) rn FROM @lc c JOIN @la a ON a.nm=c.nm)
  INSERT INTO Landlords (LandlordCode, LandlordName, Contact, Accounts, CreditLimit, CreditPeriod, [Type], EntryNo, Location, Remark)
  SELECT N'LL-'+RIGHT('000'+CAST(rn AS varchar(3)),3), nm, cid, aid, 100000, 30, 0, rn, N'Dubai, UAE', N'Property owner' FROM X;
  INSERT INTO Mobiles (Contact, MobileNum, Name) SELECT cid, N'0551100'+RIGHT('000'+CAST(ROW_NUMBER() OVER(ORDER BY cid) AS varchar(3)),3), nm FROM @lc;
END

/* ---- TENANTS (6) ---- */
IF NOT EXISTS (SELECT 1 FROM Tenants)
BEGIN
  DECLARE @tc TABLE (cid bigint, nm nvarchar(200));
  INSERT INTO Contacts (Name, Address, City, [State], Country, Phone, EmailId, [Group], Status, CountryID)
  OUTPUT inserted.ContactID, inserted.Name INTO @tc (cid, nm)
  SELECT nm, city + N', UAE', city, N'Dubai', N'UAE', phone, email, 2, 0, 0 FROM (VALUES
    (N'John Smith',           N'Dubai Marina',  N'0502200001', N'john.tenant@example.ae'),
    (N'Acme Trading FZE',     N'Business Bay',  N'0502200002', N'acme.tenant@example.ae'),
    (N'Priya Nair',           N'Al Wasl',       N'0502200003', N'priya.tenant@example.ae'),
    (N'Omar Abdullah',        N'Deira',         N'0502200004', N'omar.tenant@example.ae'),
    (N'Bright Solutions LLC', N'Silicon Oasis', N'0502200005', N'bright.tenant@example.ae'),
    (N'Maria Garcia',         N'The Greens',    N'0502200006', N'maria.tenant@example.ae')
  ) AS T(nm, city, phone, email);
  DECLARE @ta TABLE (aid bigint, nm nvarchar(200));
  INSERT INTO Accounts (Name, PrintName, [Group], OpnBalance, OpnBalanceCr, PrevBalance, PrevBalanceCr, PrvYearBalance, CreatedDate, CreatedBy, Status, Editable, Branch)
  OUTPUT inserted.AccountsID, inserted.Name INTO @ta (aid, nm)
  SELECT nm, nm, 0,0,0,0,0,0, GETDATE(), @uid, 0, 1, @branch FROM @tc;
  ;WITH X AS (SELECT c.cid, a.aid, c.nm, ROW_NUMBER() OVER (ORDER BY c.cid) rn FROM @tc c JOIN @ta a ON a.nm=c.nm)
  INSERT INTO Tenants (TenantCode, TenantName, Contact, Accounts, CreditLimit, CreditPeriod, [Type], EntryNo, Location, Remark)
  SELECT N'TN-'+RIGHT('000'+CAST(rn AS varchar(3)),3), nm, cid, aid, 50000, 15, 0, rn, N'Dubai, UAE', N'Tenant' FROM X;
  INSERT INTO Mobiles (Contact, MobileNum, Name) SELECT cid, N'0552200'+RIGHT('000'+CAST(ROW_NUMBER() OVER(ORDER BY cid) AS varchar(3)),3), nm FROM @tc;
END

/* ---- DEVELOPERS (4) ---- */
IF NOT EXISTS (SELECT 1 FROM Developers)
BEGIN
  DECLARE @dc TABLE (cid bigint, nm nvarchar(200));
  INSERT INTO Contacts (Name, Address, City, [State], Country, Phone, EmailId, [Group], Status, CountryID)
  OUTPUT inserted.ContactID, inserted.Name INTO @dc (cid, nm)
  SELECT nm, city + N', UAE', city, N'Dubai', N'UAE', phone, email, 2, 0, 0 FROM (VALUES
    (N'Emaar Properties',     N'Downtown',      N'0503300001', N'contact@emaar.example.ae'),
    (N'Nakheel Developments', N'Palm Jumeirah', N'0503300002', N'contact@nakheel.example.ae'),
    (N'Damac Group',          N'Business Bay',  N'0503300003', N'contact@damac.example.ae'),
    (N'Meraas Holding',       N'City Walk',     N'0503300004', N'contact@meraas.example.ae')
  ) AS D(nm, city, phone, email);
  DECLARE @da TABLE (aid bigint, nm nvarchar(200));
  INSERT INTO Accounts (Name, PrintName, [Group], OpnBalance, OpnBalanceCr, PrevBalance, PrevBalanceCr, PrvYearBalance, CreatedDate, CreatedBy, Status, Editable, Branch)
  OUTPUT inserted.AccountsID, inserted.Name INTO @da (aid, nm)
  SELECT nm, nm, 0,0,0,0,0,0, GETDATE(), @uid, 0, 1, @branch FROM @dc;
  ;WITH X AS (SELECT c.cid, a.aid, c.nm, ROW_NUMBER() OVER (ORDER BY c.cid) rn FROM @dc c JOIN @da a ON a.nm=c.nm)
  INSERT INTO Developers (DeveloperCode, DeveloperName, Contact, Accounts, CreditLimit, CreditPeriod, [Type], EntryNo, Location, Remark)
  SELECT N'DV-'+RIGHT('000'+CAST(rn AS varchar(3)),3), nm, cid, aid, 500000, 45, 0, rn, N'Dubai, UAE', N'Developer' FROM X;
  INSERT INTO Mobiles (Contact, MobileNum, Name) SELECT cid, N'0553300'+RIGHT('000'+CAST(ROW_NUMBER() OVER(ORDER BY cid) AS varchar(3)),3), nm FROM @dc;
END

/* ---- CONTRACTORS (4) ---- */
IF NOT EXISTS (SELECT 1 FROM Contractors)
BEGIN
  DECLARE @cc TABLE (cid bigint, nm nvarchar(200));
  INSERT INTO Contacts (Name, Address, City, [State], Country, Phone, EmailId, [Group], Status, CountryID)
  OUTPUT inserted.ContactID, inserted.Name INTO @cc (cid, nm)
  SELECT nm, city + N', UAE', city, N'Dubai', N'UAE', phone, email, 2, 0, 0 FROM (VALUES
    (N'Al Futtaim Engineering', N'Al Quoz',     N'0504400001', N'contact@aleng.example.ae'),
    (N'CoolTech A/C Services',  N'Al Qusais',   N'0504400002', N'contact@cooltech.example.ae'),
    (N'BrightClean Facilities', N'Ras Al Khor', N'0504400003', N'contact@brightclean.example.ae'),
    (N'PowerFix Electrical',    N'Al Quoz',     N'0504400004', N'contact@powerfix.example.ae')
  ) AS C(nm, city, phone, email);
  DECLARE @ca TABLE (aid bigint, nm nvarchar(200));
  INSERT INTO Accounts (Name, PrintName, [Group], OpnBalance, OpnBalanceCr, PrevBalance, PrevBalanceCr, PrvYearBalance, CreatedDate, CreatedBy, Status, Editable, Branch)
  OUTPUT inserted.AccountsID, inserted.Name INTO @ca (aid, nm)
  SELECT nm, nm, 0,0,0,0,0,0, GETDATE(), @uid, 0, 1, @branch FROM @cc;
  ;WITH X AS (SELECT c.cid, a.aid, c.nm, ROW_NUMBER() OVER (ORDER BY c.cid) rn FROM @cc c JOIN @ca a ON a.nm=c.nm)
  INSERT INTO Contractors (ContractorCode, ContractorName, Contact, Accounts, CreditLimit, CreditPeriod, [Type], EntryNo, Location, Remark, ContractType)
  SELECT N'CT-'+RIGHT('000'+CAST(rn AS varchar(3)),3), nm, cid, aid, 75000, 30, 0, rn, N'Dubai, UAE', N'Contractor', 0 FROM X;
  INSERT INTO Mobiles (Contact, MobileNum, Name) SELECT cid, N'0554400'+RIGHT('000'+CAST(ROW_NUMBER() OVER(ORDER BY cid) AS varchar(3)),3), nm FROM @cc;
END

/* ---- BROKERS (4) ---- */
IF NOT EXISTS (SELECT 1 FROM Brokers)
BEGIN
  DECLARE @bc TABLE (cid bigint, nm nvarchar(200));
  INSERT INTO Contacts (Name, Address, City, [State], Country, Phone, EmailId, [Group], Status, CountryID)
  OUTPUT inserted.ContactID, inserted.Name INTO @bc (cid, nm)
  SELECT nm, city + N', UAE', city, N'Dubai', N'UAE', phone, email, 2, 0, 0 FROM (VALUES
    (N'Better Homes Realty',  N'Sheikh Zayed Rd', N'0505500001', N'contact@betterhomes.example.ae'),
    (N'Allsopp & Allsopp',    N'Barsha Heights',  N'0505500002', N'contact@allsopp.example.ae'),
    (N'Espace Real Estate',   N'JLT',             N'0505500003', N'contact@espace.example.ae'),
    (N'Haus & Haus',          N'Marina',          N'0505500004', N'contact@hausandhaus.example.ae')
  ) AS B(nm, city, phone, email);
  DECLARE @ba TABLE (aid bigint, nm nvarchar(200));
  INSERT INTO Accounts (Name, PrintName, [Group], OpnBalance, OpnBalanceCr, PrevBalance, PrevBalanceCr, PrvYearBalance, CreatedDate, CreatedBy, Status, Editable, Branch)
  OUTPUT inserted.AccountsID, inserted.Name INTO @ba (aid, nm)
  SELECT nm, nm, 0,0,0,0,0,0, GETDATE(), @uid, 0, 1, @branch FROM @bc;
  ;WITH X AS (SELECT c.cid, a.aid, c.nm, ROW_NUMBER() OVER (ORDER BY c.cid) rn FROM @bc c JOIN @ba a ON a.nm=c.nm)
  INSERT INTO Brokers (BrokerCode, BrokerName, Contact, Accounts, CreditLimit, CreditPeriod, [Type], EntryNo, Location, Remark)
  SELECT N'BR-'+RIGHT('000'+CAST(rn AS varchar(3)),3), nm, cid, aid, 60000, 30, 0, rn, N'Dubai, UAE', N'Broker' FROM X;
  INSERT INTO Mobiles (Contact, MobileNum, Name) SELECT cid, N'0555500'+RIGHT('000'+CAST(ROW_NUMBER() OVER(ORDER BY cid) AS varchar(3)),3), nm FROM @bc;
END

/* ============================ PROPERTY FEATURE LINKS ============================ */
IF NOT EXISTS (SELECT 1 FROM SelectedFeatures)
  INSERT INTO SelectedFeatures (Property, Feature)
  SELECT p.Id, f.Feature
  FROM PropertyMains p
  CROSS APPLY (SELECT TOP 3 Feature FROM PropertyFeatures ORDER BY (SELECT ABS(CHECKSUM(NEWID())) % 100)) f;

/* ============================ DOCUMENT EXPIRY (dashboard) ============================ */
IF NOT EXISTS (SELECT 1 FROM PropertyDocumentTypes)
  INSERT INTO PropertyDocumentTypes (Reference, Purpose, ExpDate, DocumentType)
  SELECT TOP 6 p.Id, N'Property', DATEADD(DAY, 30 + (p.Id % 40), GETDATE()),
         (SELECT TOP 1 ID FROM DocumentTypes ORDER BY ID)
  FROM PropertyMains p ORDER BY p.Id;

/* ============================ TRANSACTIONS ============================ */
/* Property Registration (5): Developer + Owner(Landlord) + Property + Broker */
IF NOT EXISTS (SELECT 1 FROM PropertyRegistrations)
  INSERT INTO PropertyRegistrations (VoucherNo, RDate, Developer, Owner, Property, Broker, Amount, Note, CreatedDate, CreatedBy, Branch, editable, Status, PRNo)
  SELECT N'PR-'+RIGHT('000'+CAST(p.rn AS varchar(3)),3), DATEADD(DAY,-(p.rn*10),GETDATE()),
         d.DeveloperID, l.LandlordID, p.Id, b.BrokerID, 1500000 + p.rn*250000, N'Sample registration',
         GETDATE(), @uid, @branch, 1, 0, p.rn
  FROM (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PropertyMains) p
    JOIN (SELECT DeveloperID, ROW_NUMBER() OVER (ORDER BY DeveloperID) rn FROM Developers) d ON d.rn = ((p.rn-1) % (SELECT COUNT(*) FROM Developers))+1
    JOIN (SELECT LandlordID, ROW_NUMBER() OVER (ORDER BY LandlordID) rn FROM Landlords) l ON l.rn = ((p.rn-1) % (SELECT COUNT(*) FROM Landlords))+1
    JOIN (SELECT BrokerID, ROW_NUMBER() OVER (ORDER BY BrokerID) rn FROM Brokers) b ON b.rn = ((p.rn-1) % (SELECT COUNT(*) FROM Brokers))+1
  WHERE p.rn <= 5;

/* Tenancy Contract (6): Tenant + Property + Unit */
IF NOT EXISTS (SELECT 1 FROM TenancyContracts)
  INSERT INTO TenancyContracts (Tenant, Property, Unit, StartDate, EndDate, Duration, Rent, Deposit, Schedule, CreatedDate, CreatedBy, editable, Status, Code, EntryNo)
  SELECT t.TenantID, u.Property, u.Id,
         DATEADD(MONTH,-2,GETDATE()), DATEADD(MONTH,10,GETDATE()),   -- StartDate/EndDate are datetime in the model
         (SELECT TOP 1 Id FROM Durations WHERE Name=N'Yearly'), u.Rent, ISNULL(u.Deposit,0), 0,
         GETDATE(), @uid, 1, 0, N'TC-'+RIGHT('000'+CAST(u.rn AS varchar(3)),3), u.rn
  FROM (SELECT Id, Property, Rent, Deposit, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PropertyUnits) u
    JOIN (SELECT TenantID, ROW_NUMBER() OVER (ORDER BY TenantID) rn FROM Tenants) t ON t.rn = ((u.rn-1) % (SELECT COUNT(*) FROM Tenants))+1
  WHERE u.rn <= 6;

/* Rental Invoice (6) */
IF NOT EXISTS (SELECT 1 FROM Rentals)
  INSERT INTO Rentals (PRNo, VoucherNo, RDate, Tenant, Property, Unit, Amount, Note, CreatedDate, CreatedBy, Branch, editable, Status)
  SELECT u.rn, N'RI-'+RIGHT('000'+CAST(u.rn AS varchar(3)),3), DATEADD(DAY,-(u.rn*5),GETDATE()),
         t.TenantID, u.Property, u.Id, ISNULL(u.Rent,50000)/12, N'Monthly rent', GETDATE(), @uid, @branch, 1, 0
  FROM (SELECT Id, Property, Rent, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PropertyUnits) u
    JOIN (SELECT TenantID, ROW_NUMBER() OVER (ORDER BY TenantID) rn FROM Tenants) t ON t.rn = ((u.rn-1) % (SELECT COUNT(*) FROM Tenants))+1
  WHERE u.rn <= 6;

/* Rental Proforma (5) */
IF NOT EXISTS (SELECT 1 FROM RentalProformas)
  INSERT INTO RentalProformas (PRNo, VoucherNo, [Date], Tenant, Property, Unit, Amount, Note, CreatedDate, CreatedBy, Branch, editable, Status)
  SELECT u.rn, N'RP-'+RIGHT('000'+CAST(u.rn AS varchar(3)),3), DATEADD(DAY,-(u.rn*3),GETDATE()),
         t.TenantID, u.Property, u.Id, ISNULL(u.Rent,50000)/12, N'Proforma rent', GETDATE(), @uid, @branch, 1, 0
  FROM (SELECT Id, Property, Rent, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PropertyUnits) u
    JOIN (SELECT TenantID, ROW_NUMBER() OVER (ORDER BY TenantID) rn FROM Tenants) t ON t.rn = ((u.rn-1) % (SELECT COUNT(*) FROM Tenants))+1
  WHERE u.rn <= 5;

/* Maintenance Contract (5): Property + Contractor */
IF NOT EXISTS (SELECT 1 FROM Maintenances)
  INSERT INTO Maintenances (VoucherNo, PRNo, [Date], Property, Amount, Note, CreatedDate, CreatedBy, Branch, editable, Status, Contractor, StartDate, EndDate, ContractType)
  SELECT N'MC-'+RIGHT('000'+CAST(p.rn AS varchar(3)),3), p.rn, DATEADD(DAY,-(p.rn*7),GETDATE()),
         p.Id, 25000 + p.rn*5000, N'Annual maintenance', GETDATE(), @uid, @branch, 1, 0, c.ContractorID,
         CONVERT(nvarchar(20), DATEADD(MONTH,-1,GETDATE()), 105), CONVERT(nvarchar(20), DATEADD(MONTH,11,GETDATE()), 105),
         (SELECT TOP 1 ID FROM ContractTypes ORDER BY ID)
  FROM (SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) rn FROM PropertyMains) p
    JOIN (SELECT ContractorID, ROW_NUMBER() OVER (ORDER BY ContractorID) rn FROM Contractors) c ON c.rn = ((p.rn-1) % (SELECT COUNT(*) FROM Contractors))+1
  WHERE p.rn <= 5;

/* ============================ SUMMARY ============================ */
SELECT 'Landlords' t, COUNT(*) n FROM Landlords UNION ALL SELECT 'Tenants',COUNT(*) FROM Tenants
UNION ALL SELECT 'Developers',COUNT(*) FROM Developers UNION ALL SELECT 'Contractors',COUNT(*) FROM Contractors
UNION ALL SELECT 'Brokers',COUNT(*) FROM Brokers UNION ALL SELECT 'ContractTypes',COUNT(*) FROM ContractTypes
UNION ALL SELECT 'PropertyFeatures',COUNT(*) FROM PropertyFeatures UNION ALL SELECT 'UnitFeatures',COUNT(*) FROM PropertyUnitFeatures
UNION ALL SELECT 'ContractorTypes',COUNT(*) FROM ContractorTypes UNION ALL SELECT 'Durations',COUNT(*) FROM Durations
UNION ALL SELECT 'AdditionalFields',COUNT(*) FROM AdditionalFields UNION ALL SELECT 'PropertySettings',COUNT(*) FROM PropertySettings
UNION ALL SELECT 'SelectedFeatures',COUNT(*) FROM SelectedFeatures UNION ALL SELECT 'DocExpiry',COUNT(*) FROM PropertyDocumentTypes
UNION ALL SELECT 'PropertyRegistrations',COUNT(*) FROM PropertyRegistrations UNION ALL SELECT 'TenancyContracts',COUNT(*) FROM TenancyContracts
UNION ALL SELECT 'Rentals',COUNT(*) FROM Rentals UNION ALL SELECT 'RentalProformas',COUNT(*) FROM RentalProformas
UNION ALL SELECT 'Maintenances',COUNT(*) FROM Maintenances;
