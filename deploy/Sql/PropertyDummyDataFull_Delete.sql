/* ============================================================================
   BOS — ROLLBACK for PropertyDummyDataFull.sql.
   Removes the seeded parties (+ their Contacts/Accounts/Mobiles), settings,
   feature links, document-expiry rows, and property transactions. Does NOT touch
   PropertyMains/PropertyUnits/PropertyTypes/PropertyUnitTypes (those belong to
   PropertyDummyData.sql — use PropertyDummyData_Delete.sql for them).
   COPY/TEST DB ONLY. These tables were empty before the seed, so this restores
   that state.
   ============================================================================ */
SET NOCOUNT ON;
DECLARE @contacts TABLE (id bigint);
DECLARE @accounts TABLE (id bigint);
INSERT INTO @contacts SELECT Contact  FROM Landlords UNION SELECT Contact  FROM Tenants UNION SELECT Contact  FROM Developers UNION SELECT Contact  FROM Contractors UNION SELECT Contact  FROM Brokers;
INSERT INTO @accounts SELECT Accounts FROM Landlords UNION SELECT Accounts FROM Tenants UNION SELECT Accounts FROM Developers UNION SELECT Accounts FROM Contractors UNION SELECT Accounts FROM Brokers;

/* transactions */
DELETE FROM PropertyRegistrations;
DELETE FROM TenancyContracts;
DELETE FROM Rentals;
DELETE FROM RentalProformas;
DELETE FROM Maintenances;

/* parties + their Contacts/Accounts/Mobiles */
DELETE FROM Mobiles WHERE Contact IN (SELECT id FROM @contacts);
DELETE FROM Landlords;
DELETE FROM Tenants;
DELETE FROM Developers;
DELETE FROM Contractors;
DELETE FROM Brokers;
DELETE FROM Contacts  WHERE ContactID  IN (SELECT id FROM @contacts);
DELETE FROM Accounts  WHERE AccountsID IN (SELECT id FROM @accounts);

/* feature links + document expiry + settings */
DELETE FROM SelectedFeatures;
DELETE FROM PropertyDocumentTypes;
DELETE FROM PropertyFeatures;
DELETE FROM PropertyUnitFeatures;
DELETE FROM ContractorTypes;
DELETE FROM Durations;
DELETE FROM ContractTypes;
DELETE FROM AdditionalFields;
DELETE FROM PropertySettings;

SELECT 'Landlords' t, COUNT(*) n FROM Landlords UNION ALL SELECT 'Tenants',COUNT(*) FROM Tenants
UNION ALL SELECT 'TenancyContracts',COUNT(*) FROM TenancyContracts UNION ALL SELECT 'PropertyFeatures',COUNT(*) FROM PropertyFeatures;
