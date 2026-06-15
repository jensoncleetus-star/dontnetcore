/*
  QuickNet migration — ASP.NET Identity 2 (EF6/OWIN)  ->  ASP.NET Core Identity  schema upgrade.

  WHY: ASP.NET Core Identity's UserStore/RoleStore query columns and tables that the legacy EF6
       Identity 2 schema does not have. Without this, EVERY UserManager/SignInManager call (incl. login)
       throws "Invalid column name ..." / "Invalid object name ...".

  WHAT it adds (idempotent — safe to re-run):
    - AspNetUsers : NormalizedUserName, NormalizedEmail, ConcurrencyStamp, LockoutEnd
    - <roles table> : NormalizedName, ConcurrencyStamp        (this app remaps IdentityRole -> "AppModules")
    - tables       : AspNetRoleClaims, AspNetUserTokens       (Core-only)

  NOTE: Core's PasswordHasher verifies the legacy Identity-2 PBKDF2 (v2) hashes automatically, so
        EXISTING PASSWORDS KEEP WORKING — no password reset needed.

  RUN once per branch database during cutover (the app uses one DB per branch). Verified on emirtechlatest.
*/

SET NOCOUNT ON;

/* ---- AspNetUsers: add the Core columns ---- */
IF COL_LENGTH('AspNetUsers','NormalizedUserName') IS NULL ALTER TABLE AspNetUsers ADD NormalizedUserName nvarchar(256) NULL;
IF COL_LENGTH('AspNetUsers','NormalizedEmail')    IS NULL ALTER TABLE AspNetUsers ADD NormalizedEmail    nvarchar(256) NULL;
IF COL_LENGTH('AspNetUsers','ConcurrencyStamp')   IS NULL ALTER TABLE AspNetUsers ADD ConcurrencyStamp   nvarchar(max) NULL;
IF COL_LENGTH('AspNetUsers','LockoutEnd')         IS NULL ALTER TABLE AspNetUsers ADD LockoutEnd         datetimeoffset NULL;
GO

/* ---- AspNetUsers: populate the normalized / stamp values ---- */
UPDATE AspNetUsers SET NormalizedUserName = UPPER(UserName)            WHERE NormalizedUserName IS NULL AND UserName IS NOT NULL;
UPDATE AspNetUsers SET NormalizedEmail    = UPPER(Email)               WHERE NormalizedEmail    IS NULL AND Email    IS NOT NULL;
UPDATE AspNetUsers SET ConcurrencyStamp   = CONVERT(nvarchar(36), NEWID()) WHERE ConcurrencyStamp IS NULL;
GO

/* ---- Roles table (remapped to AppModules in this app): add Core columns + populate ---- */
IF COL_LENGTH('AppModules','NormalizedName')    IS NULL ALTER TABLE AppModules ADD NormalizedName  nvarchar(256) NULL;
IF COL_LENGTH('AppModules','ConcurrencyStamp')  IS NULL ALTER TABLE AppModules ADD ConcurrencyStamp nvarchar(max) NULL;
GO
UPDATE AppModules SET NormalizedName   = UPPER(Name)                   WHERE NormalizedName   IS NULL AND Name IS NOT NULL;
UPDATE AppModules SET ConcurrencyStamp = CONVERT(nvarchar(36), NEWID()) WHERE ConcurrencyStamp IS NULL;
GO

/* ---- Core-only Identity tables (created empty; Core queries them) ---- */
IF OBJECT_ID('AspNetRoleClaims') IS NULL
CREATE TABLE AspNetRoleClaims (
    Id         int IDENTITY(1,1) CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY,
    RoleId     nvarchar(450) NOT NULL,
    ClaimType  nvarchar(max) NULL,
    ClaimValue nvarchar(max) NULL
);
GO

IF OBJECT_ID('AspNetUserTokens') IS NULL
CREATE TABLE AspNetUserTokens (
    UserId        nvarchar(450) NOT NULL,
    LoginProvider nvarchar(450) NOT NULL,
    Name          nvarchar(450) NOT NULL,
    Value         nvarchar(max) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name)
);
GO

PRINT 'Identity EF6 -> Core schema upgrade complete.';
