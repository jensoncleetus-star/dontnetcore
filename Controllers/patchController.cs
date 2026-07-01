using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Net;
using System.IO;

using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using System.ServiceProcess;
using System.Threading;
namespace QuickSoft.Controllers
{
    // Accessible before login: bypasses the global RequireAuthenticatedUser fallback policy so patch/
    // maintenance actions can run on a fresh or unmigrated instance. QkAuthorize honors [AllowAnonymous],
    // so any action-level role gates on this controller are also bypassed — see the security note.
    [AllowAnonymous]
    public class patchController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public patchController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        class logdata
        {
            public DateTime logtime { get; set; }
            public string logid { get; set; }

        }
		// GET: patch
		[QkAuthorize(Roles = "Dev,All Sales Entry")]
		public void yearend()
        {
           var s= db.Database.SqlQueryRaw<string>("CREATE DATABASE test786").AsEnumerable();
            db.SaveChanges();
        }

		public void killlong()
		{
			string qry = "";



		



			qry = @"DECLARE @session_id int

declare Mycursor cursor for
SELECT distinct conn.session_id
FROM sys.dm_exec_sessions AS sess
JOIN sys.dm_exec_connections AS conn
ON sess.session_id = conn.session_id where cpu_time/(1000*60)>2

OPEN Mycursor
FETCH NEXT FROM Mycursor INTO @session_id
WHILE @@FETCH_STATUS = 0

BEGIN
Exec ('kill ' + @session_id)

FETCH NEXT FROM Mycursor INTO @session_id

END
CLOSE Mycursor;
DEALLOCATE Mycursor"
;
			
			executequrypatch(qry);

		}

		public string GetServerNameFromConnectionString()
		{
			try
			{
				SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(db.Database.GetDbConnection().ConnectionString);
				return builder.DataSource; // This property holds the server name/instance
			}
			catch (System.ArgumentException ex)
			{
				// Handle invalid connection string format
				System.Console.WriteLine($"Error parsing connection string: {ex.Message}");
				return null;
			}
		}
		public void restart()
		{
			ServiceController service = new ServiceController("MSSQL$SQLEXPRESS01");

			if (service != null)
			{
				try
				{
					// Stop the service if it's running
					if (service.Status == ServiceControllerStatus.Running)
					{
						service.Stop();
						service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30)); // Wait for up to 30 seconds for the service to stop
					}

					// Start the service
					service.Start();
					service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30)); // Wait for up to 30 seconds for the service to start
				}
				catch (Exception ex)
				{
					// Handle exceptions, e.g., service not found, insufficient permissions, timeout
					Console.WriteLine($"Error restarting SQL Server service: {ex.Message}");
				}
				finally
				{
					service.Dispose(); // Release resources
				}
			}
		}



		public void kill()
		{
			string qry = "";



			qry = @"DECLARE @session_id int
declare Mycursor cursor for
SELECT conn.session_id
FROM sys.dm_exec_sessions AS sess
JOIN sys.dm_exec_connections AS conn
ON sess.session_id = conn.session_id

OPEN Mycursor
FETCH NEXT FROM Mycursor INTO @session_id
WHILE @@FETCH_STATUS = 0

BEGIN
Exec ('kill ' + @session_id)

FETCH NEXT FROM Mycursor INTO @session_id

END
CLOSE Mycursor;
DEALLOCATE Mycursor";
			executequrypatch(qry);

		}



		public string fast()
		{
			string qry = "";
			qry = @"DECLARE @session_id int
declare Mycursor cursor for
SELECT conn.session_id
FROM sys.dm_exec_sessions AS sess
JOIN sys.dm_exec_connections AS conn
ON sess.session_id = conn.session_id

OPEN Mycursor
FETCH NEXT FROM Mycursor INTO @session_id
WHILE @@FETCH_STATUS = 0

BEGIN
Exec ('kill ' + @session_id)

FETCH NEXT FROM Mycursor INTO @session_id

END
CLOSE Mycursor;
DEALLOCATE Mycursor";
			executequrypatch(qry);


			bool result = true;
			qry = "ALTER INDEX ALL ON  AMCs REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			
			
			qry = "ALTER INDEX ALL ON  AccountsTransactions REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  AmcAssignedToes REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  Customers REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  CustomerRemarks REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  AmcRemarks REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  ProTasks REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  contacts REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  ContactRelations REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON[dbo].[StockAdjustments] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON[dbo].[StockTransferItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;

			qry = "ALTER INDEX ALL ON[dbo].[SEItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;


			qry = "ALTER INDEX ALL ON[dbo].[PEItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;


			qry = "ALTER INDEX ALL ON[dbo].[SRItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;


			qry = "ALTER INDEX ALL ON[dbo].[PRItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;

			qry = "ALTER INDEX ALL ON StockTransferItems REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;

			qry = "ALTER INDEX ALL ON PurchaseEntries REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			try
			{
				qry = "ALTER INDEX ALL ON SalesEntries REBUILD";
				result = executequrypatch(qry);
				if (!result)
					return qry;
			}
			catch (Exception e)
			{
				return qry;
			}

			return "success";

		}
		public string reindex()
		{
			string qry = "";


			bool result = true;
			qry = "ALTER INDEX ALL ON  AMCs REBUILD";
			 result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  AssignedToes REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			
			qry = "ALTER INDEX ALL ON  TaskAssigneds REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  AccountsTransactions REBUILD";
			result=executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  AmcAssignedToes REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  Customers REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  CustomerRemarks REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  AmcRemarks REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  ProTasks REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  contacts REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON  ContactRelations REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON[dbo].[StockAdjustments] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			qry = "ALTER INDEX ALL ON[dbo].[StockTransferItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;

			qry = "ALTER INDEX ALL ON[dbo].[SEItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;


			qry = "ALTER INDEX ALL ON[dbo].[PEItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;


			qry = "ALTER INDEX ALL ON[dbo].[SRItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;


			qry = "ALTER INDEX ALL ON[dbo].[PRItems] REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;

			qry = "ALTER INDEX ALL ON StockTransferItems REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;

			qry = "ALTER INDEX ALL ON PurchaseEntries REBUILD";
			result = executequrypatch(qry);
			if (!result)
				return qry;
			try {
				qry = "ALTER INDEX ALL ON SalesEntries REBUILD";
				result = executequrypatch(qry);
				if (!result)
					return qry;
			}
		catch(Exception e)
            {
				return qry;
            }
			qry = "exec re";
			result = executequrypatch(qry);
			return "success";

		}
			public void executepatch()
        {
            string qry = "";

			#region qrys
		
			qry = @"CREATE TABLE [dbo].[PriceCategories](
	[pricestratagyid] [bigint] IDENTITY(1,1) NOT NULL,
	[description] [varchar](50) NOT NULL,
	[method] [bigint] NOT NULL,
	[value] [decimal](18, 2) NOT NULL,
	[active] [bit] NOT NULL,
 CONSTRAINT [PK_pricestratagys] PRIMARY KEY CLUSTERED 
(
	[pricestratagyid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[SuperUsers](
	[superuserid] [bigint] IDENTITY(1,1) NOT NULL,
	[employeeid] [bigint] NOT NULL,
	[purpose] [varchar](50) NOT NULL,
	[mcid] [bigint] NULL,
	[emailid] [varchar](50) NOT NULL
 CONSTRAINT [PK_superuserid] PRIMARY KEY CLUSTERED 
(
	[superuserid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[otpapproves](
	[optid] [bigint] IDENTITY(1,1) NOT NULL,
	[entryid] [bigint] NOT NULL,
	[purpose] [varchar](50) NOT NULL,
	[requestedby] [varchar](50) NOT NULL,
	[approvedby] [varchar](50) NULL,
	[otp] [varchar](50) NOT NULL,
	[expdate] [datetime] NOT NULL,
 CONSTRAINT [PK_otpapprove] PRIMARY KEY CLUSTERED 
(
	[optid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
CREATE TABLE [dbo].[dummyAccountsTransactions](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Debit] [decimal](18, 2) NOT NULL,
	[Credit] [decimal](18, 2) NOT NULL,
	[Account] [bigint] NOT NULL,
	[Purpose] [nvarchar](max) NULL,
	[reference] [bigint] NOT NULL,
	[Type] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Accounts_AccountsID] [bigint] NULL,
	[Date] [datetime] NULL,
	[Status] [bit] NULL,
	[Narration] [nvarchar](250) NULL,
	[Project] [bigint] NULL,
	[ProTask] [bigint] NULL,
 CONSTRAINT [PK_dbo.dummyAccountsTransactions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
";
			executequrypatch(qry);
			
			qry = @"alter table Employees add perhour decimal(18,2) null


CREATE TABLE [dbo].[ChequeBooks](
	[bookid] [bigint] IDENTITY(1,1) NOT NULL,
	[bookname] [varchar](50) NOT NULL,
	[booktype] [int] NOT NULL,
	[numberstarting] [bigint] NOT NULL,
	[endnumbering] [bigint] NOT NULL,
	[cancelledleaf] [bigint] NOT NULL,
	[usedleaf] [bigint] NULL,
 CONSTRAINT [PK_ChequeBook] PRIMARY KEY CLUSTERED 
(
	[bookid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[chequetransactions](
	[chequetransid] [bigint] IDENTITY(1,1) NOT NULL,
	[bookid] [bigint] NOT NULL,
	[referenceno] [bigint] NULL,
	[purpose] [varchar](50) NULL,
	[remarks] [varchar](500) NULL,
	[transtype] [int] NOT NULL,
	[amount] [decimal](18, 2) NULL,
	[transdate] [datetime] NOT NULL,
	[docserialno] [bigint] NOT NULL,
 CONSTRAINT [PK_chequetransactions] PRIMARY KEY CLUSTERED 
(
	[chequetransid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]


";

			executequrypatch(qry);
			qry = "INSERT [dbo].[AppModules] ([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES (N'19d56162-3a96-42b0-b1f3-167d746403e1', N'Price Category Master', 234343488, N'Price Category Master', N'/MasterPriceCategory/Index', 1069, NULL, 0, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)";
			executequrypatch(qry);



			qry = @"CREATE TABLE [dbo].[ParticularParties](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PartyID] [bigint] NULL,
	[PartyType] [bigint] NULL,
	[PartyName] [nvarchar](max) NULL,
 CONSTRAINT [PK_ParticularParties] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";
			executequrypatch(qry);
			qry = @"SET IDENTITY_INSERT [dbo].[PriceCategories] ON 
GO
INSERT [dbo].[PriceCategories] ([pricestratagyid], [description], [method], [value], [active]) VALUES (1, N'default', 1, CAST(0.00 AS Decimal(18, 2)), 1)
GO
SET IDENTITY_INSERT [dbo].[PriceCategories] OFF";
			executequrypatch(qry);
			qry = "alter table SalesEntries add pricecategoryid bigint null";
			executequrypatch(qry);
			qry = "alter table aspnetusers add discount decimal(18,2) null";
			executequrypatch(qry);
			qry = @"alter table items add[OpeningCost] [decimal](18, 2) NOT NULL default 0
alter table items add[InSaleInvoice] [bit] NOT NULL default 0";

			executequrypatch(qry);


			qry = @"CREATE TABLE [dbo].[quotationdocuments](
	[qutid] [bigint] IDENTITY(1,1) NOT NULL,
	[quotationID] [bigint] NOT NULL,
	[FileName] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[DoucumentType] [nvarchar](50) NULL,
	[Expiry] [date] NULL,
	[Notes] [nvarchar](200) NULL,
 CONSTRAINT [PK_dbo.quotationdocument] PRIMARY KEY CLUSTERED 
(
	[qutid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[commissions](
	[commid] [bigint] IDENTITY(1,1) NOT NULL,
	[agent] [bigint] NOT NULL,
	[commisiontype] [int] NOT NULL,
	[commisionmode] [int] NOT NULL,
	[comvalue] [decimal](18, 0) NOT NULL,
	[salesid] [bigint] NOT NULL,
 CONSTRAINT [PK_commission] PRIMARY KEY CLUSTERED 
(
	[commid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);
			qry = @"


CREATE TABLE[dbo].[keytableviews](

   [keyautoid][bigint] IDENTITY(1, 1) NOT NULL,

   [keyvalue] [nchar](10) NOT NULL,

   [purpose] [nchar](20) NOT NULL,

   [expire] [bigint] NOT NULL,

   [entrytime] [datetime] NOT NULL,

   [employeeid] [bigint] NOT NULL
) ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"ALTER TABLE Items ADD cashprice decimal(18,4);
ALTER TABLE Items ADD creditprice decimal(18,4);";
			executequrypatch(qry);
			qry = "ALTER TABLE Customers ADD OpenClose int";
			executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[shelfstockmovements](
	[stockmovementid] [bigint] IDENTITY(1,1) NOT NULL,
	[rackmciid] [bigint] NOT NULL,
	[referenceid] [bigint] NOT NULL,
	[purpose] [varchar](50) NOT NULL,
	[itemid] [bigint] NOT NULL,
	[unitid] [bigint] NOT NULL,
	[qty] [decimal](18, 2) NOT NULL,
	[createdby] [varchar](50) NOT NULL,
	[createddate] [datetime] NOT NULL,
 CONSTRAINT [PK_shelfstockmovement] PRIMARY KEY CLUSTERED 
(
	[stockmovementid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
alter table ShelfStockTransfers add VoucherNo nvarchar(max) null
ALTER TABLE ShelfStockTransfers DROP COLUMN rackmcId
alter table ShelfStockTransfers add FromRackMcId bigint
alter table ShelfStockTransfers add ToRackMcId bigint";
			executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[ShelfStockTransfers](
	[shelftransferId] [bigint] IDENTITY(1,1) NOT NULL,
	[rackmcId] [bigint] NOT NULL,
	[transactionType] [varchar](50) NOT NULL,
	[createdBy] [nvarchar](max) NULL,
	[createdDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ShelfStockTransfers] PRIMARY KEY CLUSTERED 
(
	[shelftransferId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
CREATE TABLE [dbo].[SSTItems](
	[STItemId] [bigint] IDENTITY(1,1) NOT NULL,
	[shelfTransfer] [bigint] NOT NULL,
	[item] [bigint] NOT NULL,
	[itemUnit] [bigint] NULL,
	[itemQuantity] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_SSTItems] PRIMARY KEY CLUSTERED 
(
	[STItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);
			qry = @"
CREATE TABLE[dbo].[SRNoteItems](

   [SRItemsId][bigint] IDENTITY(1, 1) NOT NULL,

   [SalesReturnId] [bigint] NOT NULL,

   [Item] [bigint] NOT NULL,

   [ItemUnitPrice] [decimal](18, 2) NOT NULL,

   [ItemQuantity] [decimal](18, 2) NOT NULL,

   [ItemSubTotal] [decimal](18, 2) NOT NULL,

   [ItemTax] [decimal](18, 2) NOT NULL,

   [ItemTaxAmount] [decimal](18, 2) NOT NULL,

   [ItemTotalAmount] [decimal](18, 2) NOT NULL,

   [itemNote] [nvarchar](max)NULL,
	[ItemId_ItemID] [bigint] NULL,
	[ItemDiscount] [decimal](18, 2) NOT NULL,
	[ItemUnit] [bigint] NULL,
 CONSTRAINT[PK_dbo.SRNoteItems] PRIMARY KEY CLUSTERED
(

   [SRItemsId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[Racks](

   [RackId][bigint] IDENTITY(1, 1) NOT NULL,

   [RackName] [varchar](50) NOT NULL,
CONSTRAINT[PK_Racks] PRIMARY KEY CLUSTERED
(

  [RackId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[Shelves](

   [ShelfId][bigint] IDENTITY(1, 1) NOT NULL,

   [shelfName] [varchar](50) NOT NULL,
CONSTRAINT[PK_Shelves] PRIMARY KEY CLUSTERED
(

  [ShelfId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[rackmaterialcentres](

   [rackmcid][bigint] IDENTITY(1, 1) NOT NULL,

   [rackid] [bigint] NOT NULL,

   [shelfid] [bigint] NOT NULL,

   [mcid] [bigint] NOT NULL,
CONSTRAINT[PK_rackmaterialcentres] PRIMARY KEY CLUSTERED
(

  [rackmcid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);
			qry = "alter table ProTasks add Lattitude nvarchar(max) null";
			executequrypatch(qry);
			qry = "alter table ProTasks add Longitude nvarchar(max) null";
			executequrypatch(qry);
			qry = "ALTER TABLE ProTasks ADD OpenClose int";
			executequrypatch(qry);
			qry = "ALTER TABLE AMCs ADD OpenClose int";
			executequrypatch(qry);
			qry = "alter table ProTasks add Lattitude nvarchar(max) null";
			executequrypatch(qry);
			qry = "alter table ProTasks add Longitude nvarchar(max) null";
			executequrypatch(qry);

			qry = @"ALTER TABLE ProTasks ADD OpenClose int 
ALTER TABLE AMCs ADD OpenClose int 
update ProTasks set OpenClose =0
update amcs set OpenClose =0
CREATE TABLE [dbo].[Racks](
	[RackId] [bigint] IDENTITY(1,1) NOT NULL,
	[RackName] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Racks] PRIMARY KEY CLUSTERED 
(
	[RackId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Shelves](
	[ShelfId] [bigint] IDENTITY(1,1) NOT NULL,
	[shelfName] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Shelves] PRIMARY KEY CLUSTERED 
(
	[ShelfId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[rackmaterialcentres](
	[rackmcid] [bigint] IDENTITY(1,1) NOT NULL,
	[rackid] [bigint] NOT NULL,
	[shelfid] [bigint] NOT NULL,
	[mcid] [bigint] NOT NULL,
 CONSTRAINT [PK_rackmaterialcentres] PRIMARY KEY CLUSTERED 
(
	[rackmcid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[SRNoteItems](
	[SRItemsId] [bigint] IDENTITY(1,1) NOT NULL,
	[SalesReturnId] [bigint] NOT NULL,
	[Item] [bigint] NOT NULL,
	[ItemUnitPrice] [decimal](18, 2) NOT NULL,
	[ItemQuantity] [decimal](18, 2) NOT NULL,
	[ItemSubTotal] [decimal](18, 2) NOT NULL,
	[ItemTax] [decimal](18, 2) NOT NULL,
	[ItemTaxAmount] [decimal](18, 2) NOT NULL,
	[ItemTotalAmount] [decimal](18, 2) NOT NULL,
	[itemNote] [nvarchar](max) NULL,
	[ItemId_ItemID] [bigint] NULL,
	[ItemDiscount] [decimal](18, 2) NOT NULL,
	[ItemUnit] [bigint] NULL,
 CONSTRAINT [PK_dbo.SRNoteItems] PRIMARY KEY CLUSTERED 
(
	[SRItemsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
CREATE TABLE [dbo].[PRItemNotes](
	[PRItemsId] [bigint] IDENTITY(1,1) NOT NULL,
	[PurchaseReturnId] [bigint] NOT NULL,
	[Item] [bigint] NOT NULL,
	[ItemUnitPrice] [decimal](18, 2) NOT NULL,
	[ItemQuantity] [decimal](18, 2) NOT NULL,
	[ItemSubTotal] [decimal](18, 2) NOT NULL,
	[ItemTax] [decimal](18, 2) NOT NULL,
	[ItemTaxAmount] [decimal](18, 2) NOT NULL,
	[ItemTotalAmount] [decimal](18, 2) NOT NULL,
	[itemNote] [nvarchar](max) NULL,
	[ItemDiscount] [decimal](18, 2) NOT NULL,
	[ItemUnit] [bigint] NULL,
 CONSTRAINT [PK_dbo.PRItemNotes] PRIMARY KEY CLUSTERED 
(
	[PRItemsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
CREATE TABLE [dbo].[ShelfStockTransfers](
	[shelftransferId] [bigint] IDENTITY(1,1) NOT NULL,
	[rackmcId] [bigint] NOT NULL,
	[transactionType] [varchar](50) NOT NULL,
	[createdBy] [nvarchar](max) NULL,
	[createdDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ShelfStockTransfers] PRIMARY KEY CLUSTERED 
(
	[shelftransferId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
CREATE TABLE [dbo].[SSTItems](
	[STItemId] [bigint] IDENTITY(1,1) NOT NULL,
	[shelfTransfer] [bigint] NOT NULL,
	[item] [bigint] NOT NULL,
	[itemUnit] [bigint] NULL,
	[itemQuantity] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_SSTItems] PRIMARY KEY CLUSTERED 
(
	[STItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
CREATE TABLE [dbo].[shelfstockmovements](
	[stockmovementid] [bigint] IDENTITY(1,1) NOT NULL,
	[rackmciid] [bigint] NOT NULL,
	[referenceid] [bigint] NOT NULL,
	[purpose] [varchar](50) NOT NULL,
	[itemid] [bigint] NOT NULL,
	[unitid] [bigint] NOT NULL,
	[qty] [decimal](18, 2) NOT NULL,
	[createdby] [varchar](50) NOT NULL,
	[createddate] [datetime] NOT NULL,
 CONSTRAINT [PK_shelfstockmovement] PRIMARY KEY CLUSTERED 
(
	[stockmovementid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
alter table ShelfStockTransfers add VoucherNo nvarchar(max) null;
ALTER TABLE ShelfStockTransfers DROP COLUMN rackmcId;
alter table ShelfStockTransfers add FromRackMcId bigint;
alter table ShelfStockTransfers add ToRackMcId bigint;
";
			executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[VenderRateMasters](
	[VenderRateMasterId] [bigint] IDENTITY(1,1) NOT NULL,
	[SupplierId] [bigint] NOT NULL,
	[createdatae] [datetime] NOT NULL,
 CONSTRAINT [PK_VenderRateMaster] PRIMARY KEY CLUSTERED 
(
	[VenderRateMasterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[VenterRateDetails](
	[VenterRateId] [bigint] IDENTITY(1,1) NOT NULL,
	[supplierid] [bigint] NOT NULL,
	[VenderRateMasterId] [bigint] NOT NULL,
	[ItemType] [varchar](100) NULL,
	[ExternalModal] [varchar](500) NULL,
	[InternalModal] [varchar](500) NULL,
	[Rate] [decimal](18, 2) NOT NULL,
	[promorate] [decimal](18, 2) NULL,
	[promotiondescription] [varchar](500) NULL,
 CONSTRAINT [PK_VenterRateDetails] PRIMARY KEY CLUSTERED 
(
	[VenterRateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[RemarkCheques](
	[RemarkId] [bigint] IDENTITY(1,1) NOT NULL,
	[pdcid] [bigint] NOT NULL,
	[Remark] [varchar](200) NULL,
	[status] [varchar](50) NULL,
	[createdby] [varchar](100) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_RemarkCheque] PRIMARY KEY CLUSTERED 
(
	[RemarkId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[Brokers](

   [BrokerID][bigint] IDENTITY(1, 1) NOT NULL,

   [BrokerCode] [nvarchar](max)NULL,
	[BrokerName] [nvarchar](max)NULL,
	[Contact] [bigint] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[Lattitude] [nvarchar](max)NULL,
	[Longitude] [nvarchar](max)NULL,
	[Location] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[Accounts] [bigint] NOT NULL,
	[BankName] [nvarchar](max)NULL,
	[AccountNo] [nvarchar](max)NULL,
	[IbanNo] [nvarchar](max)NULL,
	[BranchName] [nvarchar](max)NULL,
	[Swift] [nvarchar](max)NULL,
	[AccountID_AccountsID] [bigint] NULL,
	[ContactID_ContactID] [bigint] NULL,
	[EntryNo] [bigint] NOT NULL,
	[Type] [int] NOT NULL,
	[File] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.Brokers] PRIMARY KEY CLUSTERED
(

   [BrokerID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = "INSERT [dbo].[AppModules] ([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES (N'c65cb084-e3f4-4b2b-b513-232b67ec6af5', N'Ledger Mini', 234343441, N'Ledger Mini', N'/MyReports/Ledgermin', 1064, NULL, 1, NULL, 0, 0, N'AppModules', NULL, 0, 10)";
			executequrypatch(qry);

			qry = "INSERT [dbo].[AppModules] ([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES (N'54a6a3ab-7bbf-4c0b-9d41-6321c7663e0f', N'Asset To Asset Transfer', 234343460, N'Asset To Asset Transfer', N'/AssetFromInventory/MoveAssettoasset', 6042, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[Contractors](
		   
			   [ContractorID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [ContractorCode] [nvarchar](max)NULL,
	[ContractorName] [nvarchar](max)NULL,
	[Contact] [bigint] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[Lattitude] [nvarchar](max)NULL,
	[Longitude] [nvarchar](max)NULL,
	[Location] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[Accounts] [bigint] NOT NULL,
	[BankName] [nvarchar](max)NULL,
	[AccountNo] [nvarchar](max)NULL,
	[IbanNo] [nvarchar](max)NULL,
	[BranchName] [nvarchar](max)NULL,
	[Swift] [nvarchar](max)NULL,
	[AccountID_AccountsID] [bigint] NULL,
	[ContactID_ContactID] [bigint] NULL,
	[Type] [int] NOT NULL,
	[EntryNo] [bigint] NOT NULL,
	[ContractType] [bigint] NULL,
 CONSTRAINT[PK_dbo.Contractors] PRIMARY KEY CLUSTERED
(

   [ContractorID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[Developers](
		   
			   [DeveloperID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [DeveloperCode] [nvarchar](max)NULL,
	[DeveloperName] [nvarchar](max)NULL,
	[Contact] [bigint] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[Lattitude] [nvarchar](max)NULL,
	[Longitude] [nvarchar](max)NULL,
	[Location] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[Accounts] [bigint] NOT NULL,
	[BankName] [nvarchar](max)NULL,
	[AccountNo] [nvarchar](max)NULL,
	[IbanNo] [nvarchar](max)NULL,
	[BranchName] [nvarchar](max)NULL,
	[Swift] [nvarchar](max)NULL,
	[AccountID_AccountsID] [bigint] NULL,
	[ContactID_ContactID] [bigint] NULL,
	[Type] [int] NOT NULL,
	[EntryNo] [bigint] NOT NULL,
 CONSTRAINT[PK_dbo.Developers] PRIMARY KEY CLUSTERED
(

   [DeveloperID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[Landlords](
		   
			   [LandlordID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [LandlordCode] [nvarchar](max)NULL,
	[LandlordName] [nvarchar](max)NULL,
	[Contact] [bigint] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[Lattitude] [nvarchar](max)NULL,
	[Longitude] [nvarchar](max)NULL,
	[Location] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[Accounts] [bigint] NOT NULL,
	[BankName] [nvarchar](max)NULL,
	[AccountNo] [nvarchar](max)NULL,
	[IbanNo] [nvarchar](max)NULL,
	[BranchName] [nvarchar](max)NULL,
	[Swift] [nvarchar](max)NULL,
	[AccountID_AccountsID] [bigint] NULL,
	[ContactID_ContactID] [bigint] NULL,
	[Type] [int] NOT NULL,
	[EntryNo] [bigint] NOT NULL,
 CONSTRAINT[PK_dbo.Landlords] PRIMARY KEY CLUSTERED
(

   [LandlordID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[Tenants](
		   
			   [TenantID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [TenantCode] [nvarchar](max)NULL,
	[TenantName] [nvarchar](max)NULL,
	[Contact] [bigint] NOT NULL,
	[CreditLimit] [decimal](18, 2) NOT NULL,
	[CreditPeriod] [int] NOT NULL,
	[Lattitude] [nvarchar](max)NULL,
	[Longitude] [nvarchar](max)NULL,
	[Location] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[Accounts] [bigint] NOT NULL,
	[BankName] [nvarchar](max)NULL,
	[AccountNo] [nvarchar](max)NULL,
	[IbanNo] [nvarchar](max)NULL,
	[BranchName] [nvarchar](max)NULL,
	[Swift] [nvarchar](max)NULL,
	[AccountID_AccountsID] [bigint] NULL,
	[ContactID_ContactID] [bigint] NULL,
	[Type] [int] NOT NULL,
	[EntryNo] [bigint] NOT NULL,
 CONSTRAINT[PK_dbo.Tenants] PRIMARY KEY CLUSTERED
(

   [TenantID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertySettings](
		   
				   [Id][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Module] [nvarchar](20) NULL,
        [Type] [nvarchar](max)NULL,
        [LValue] [bigint] NULL,
        [SValue] [nvarchar](max)NULL,
        [Description] [nvarchar](max)NULL,
        [Status] [int] NOT NULL,
 CONSTRAINT[PK_dbo.PropertySettings] PRIMARY KEY CLUSTERED
(

	   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[ContractorTypes](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Name] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.ContractorTypes] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyTypes](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Name] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.PropertyTypes] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyMains](
		   
			   [Id][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Code] [nvarchar](20) NULL,
	[Name] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[Description] [nvarchar](max)NULL,
	[PropertyType] [bigint] NULL,
	[DocumentType] [bigint] NULL,
	[File] [nvarchar](max)NULL,
	[Address] [nvarchar](250) NULL,
	[Country] [nvarchar](50) NULL,
	[State] [nvarchar](50) NULL,
	[City] [nvarchar](50) NULL,
	[Zip] [nvarchar](max)NULL,
	[Document] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[editable] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[EntryNo] [bigint] NOT NULL,
 CONSTRAINT[PK_dbo.PropertyMains] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyImages](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [PropertyID] [bigint] NOT NULL,
		   
				   [FileName] [nvarchar](max)NOT NULL,
        [Status] [int] NOT NULL,

		[Items_Id] [bigint] NULL,
 CONSTRAINT[PK_dbo.PropertyImages] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyDocuments](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [PropertyID] [bigint] NOT NULL,
		   
				   [FileName] [nvarchar](max)NOT NULL,
        [Status] [int] NOT NULL,

		[Items_Id] [bigint] NULL,
 CONSTRAINT[PK_dbo.PropertyDocuments] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[SelectedFeatures](
		   
			   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Property] [bigint] NOT NULL,
		   
			   [Feature] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.SelectedFeatures] PRIMARY KEY CLUSTERED
(

   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[PropertyFeatures](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Feature] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.PropertyFeatures] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[AdditionalFields](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Name] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.AdditionalFields] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[AdditionalFieldDatas](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Name] [nvarchar](max)NULL,
        [Reference] [bigint] NOT NULL,

		[Purpose] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.AdditionalFieldDatas] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyUnits](
		   
			   [Id][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Name] [nvarchar](max)NULL,
	[Code] [nvarchar](max)NULL,
	[Property] [bigint] NULL,
	[UnitType] [bigint] NULL,
	[Rent] [decimal](18, 2) NULL,
	[Deposit] [decimal](18, 2) NULL,
	[Description] [nvarchar](max)NULL,
	[TnC] [nvarchar](max)NULL,
	[File] [nvarchar](max)NULL,
	[Document] [bigint] NULL,
	[EntryNo] [bigint] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[editable] [int] NOT NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT[PK_dbo.PropertyUnits] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyUnitImages](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [UnitID] [bigint] NOT NULL,
		   
				   [FileName] [nvarchar](max)NOT NULL,
        [Status] [int] NOT NULL,

		[Items_Id] [bigint] NULL,
 CONSTRAINT[PK_dbo.PropertyUnitImages] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyUnitDocuments](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [UnitID] [bigint] NOT NULL,
		   
				   [FileName] [nvarchar](max)NOT NULL,
        [Status] [int] NOT NULL,

		[Items_Id] [bigint] NULL,
 CONSTRAINT[PK_dbo.PropertyUnitDocuments] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[SelectedUnitFeatures](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Unit] [bigint] NOT NULL,
		   
				   [Feature] [bigint] NOT NULL,
			CONSTRAINT[PK_dbo.SelectedUnitFeatures] PRIMARY KEY CLUSTERED
		  (
		  
				  [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyUnitTypes](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Name] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.PropertyUnitTypes] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[PropertyUnitFeatures](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Feature] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.PropertyUnitFeatures] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[Rentals](
		   
				   [RentalID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [PRNo] [bigint] NOT NULL,
		   
				   [VoucherNo] [nvarchar](max)NULL,
        [RDate] [datetime] NOT NULL,

		[Tenant] [bigint] NOT NULL,

		[Property] [bigint] NOT NULL,

		[Unit] [bigint] NOT NULL,

		[Amount] [decimal](18, 2) NOT NULL,

		[Note] [nvarchar](max)NULL,
        [Remark] [nvarchar](max)NULL,
        [TermsCondition] [nvarchar](max)NULL,
        [CreatedDate] [datetime] NOT NULL,

		[CreatedBy] [nvarchar](max)NULL,
        [Branch] [bigint] NOT NULL,

		[editable] [int] NOT NULL,

		[Status] [int] NOT NULL,
 CONSTRAINT[PK_dbo.Rentals] PRIMARY KEY CLUSTERED
(

	   [RentalID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[Durations](
		   
				   [Id][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Name] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.Durations] PRIMARY KEY CLUSTERED
(

	   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[TenancyContracts](
		   
			   [Id][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Tenant] [bigint] NULL,
	[Property] [bigint] NULL,
	[Unit] [bigint] NULL,
	[StartDate] [nvarchar](max)NULL,
	[EndDate] [nvarchar](max)NULL,
	[Duration] [bigint] NULL,
	[Rent] [decimal](18, 2) NULL,
	[Deposit] [decimal](18, 2) NULL,
	[Schedule] [int] NOT NULL,
	[DueDate] [bigint] NULL,
	[PaymentType] [bigint] NULL,
	[File] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[editable] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[Remark] [nvarchar](max)NULL,
	[Note] [nvarchar](max)NULL,
	[TnC] [nvarchar](max)NULL,
	[Code] [nvarchar](max)NULL,
	[EntryNo] [bigint] NOT NULL,
	[PaymentTypeDeposit]  [bigint] NULL
 CONSTRAINT[PK_dbo.TenancyContracts] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[Cheques](
		   
			   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Amount] [decimal](18, 2) NOT NULL,
		   
			   [Date] [datetime] NOT NULL,
		   
			   [ChequeNo] [nvarchar](max)NULL,
	[Reference] [bigint] NOT NULL,
	[Purpose] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.Cheques] PRIMARY KEY CLUSTERED
(

   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[ChequeImages](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Cheque] [bigint] NOT NULL,
		   
				   [attachments] [nvarchar](max)NOT NULL,
 CONSTRAINT[PK_dbo.ChequeImages] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[ContractDocuments](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [Tenancy] [bigint] NOT NULL,
		   
				   [FileName] [nvarchar](max)NOT NULL,
        [Status] [int] NOT NULL,

		[Items_Id] [bigint] NULL,
 CONSTRAINT[PK_dbo.ContractDocuments] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyRegistrations](
		   
				   [RegistrationID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [VoucherNo] [nvarchar](max)NULL,
        [RDate] [datetime] NOT NULL,

		[Developer] [bigint] NOT NULL,

		[Owner] [bigint] NOT NULL,

		[Property] [bigint] NOT NULL,

		[Broker] [bigint] NOT NULL,

		[Amount] [decimal](18, 2) NOT NULL,

		[Note] [nvarchar](max)NULL,
        [Remark] [nvarchar](max)NULL,
        [TermsCondition] [nvarchar](max)NULL,
        [CreatedDate] [datetime] NOT NULL,

		[CreatedBy] [nvarchar](max)NULL,
        [Branch] [bigint] NOT NULL,

		[editable] [int] NOT NULL,

		[Status] [int] NOT NULL,

		[PRNo] [bigint] NOT NULL,
		[BuildupArea] [decimal](18, 2) NULL,
		[PaymentType] [bigint] NULL,
		[PlotNumber]  NVARCHAR(max) NULL,
		[PlotOption]  NVARCHAR(max) NULL,
		[PlotArea] [decimal](18, 2) NULL,
		[PAMeasurement]  NVARCHAR(max) NULL,
		[BAMeasurement]  NVARCHAR(max) NULL,
		[Hector] [decimal](18, 2) NULL,
		[ADDCNo] [nvarchar](max)NULL,
		[PermitId] [nvarchar](max)NULL,
		[PermissionId] [nvarchar](max)NULL,
		[BookingDate] [datetime] NULL,
		[HandoverDate] [datetime] NULL,
 CONSTRAINT[PK_dbo.PropertyRegistrations] PRIMARY KEY CLUSTERED
(

	   [RegistrationID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[Maintenances](
		   
			   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [VoucherNo] [nvarchar](max)NULL,
	[PRNo] [bigint] NOT NULL,
	[Date] [datetime] NOT NULL,
	[Property] [bigint] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Note] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[TermsCondition] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[Branch] [bigint] NOT NULL,
	[editable] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[Contractor] [bigint] NOT NULL,
	[StartDate] [nvarchar](max)NULL,
	[EndDate] [nvarchar](max)NULL,
	[PaymentType] [bigint] NULL,
 CONSTRAINT[PK_dbo.Maintenances] PRIMARY KEY CLUSTERED
(

   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[RentalProformas](
		   
				   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
				   [PRNo] [bigint] NOT NULL,
		   
				   [VoucherNo] [nvarchar](max)NULL,
        [Date] [datetime] NOT NULL,

		[Tenant] [bigint] NOT NULL,

		[Property] [bigint] NOT NULL,

		[Unit] [bigint] NOT NULL,

		[Amount] [decimal](18, 2) NOT NULL,

		[Note] [nvarchar](max)NULL,
        [Remark] [nvarchar](max)NULL,
        [TermsCondition] [nvarchar](max)NULL,
        [CreatedDate] [datetime] NOT NULL,

		[CreatedBy] [nvarchar](max)NULL,
        [Branch] [bigint] NOT NULL,

		[editable] [int] NOT NULL,

		[Status] [int] NOT NULL,
 CONSTRAINT[PK_dbo.RentalProformas] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[PropertyDocumentTypes] (
		   
			   [ID][bigint] NOT NULL IDENTITY,
    [Reference] [bigint] NOT NULL,

	[Purpose] [nvarchar](max),
    [ExpDate] [datetime] NOT NULL,

	[DocumentType] [bigint] NOT NULL,
	CONSTRAINT[PK_dbo.PropertyDocumentTypes] PRIMARY KEY([ID])
);

			CREATE TABLE[dbo].[ContractTypes](
		   
			   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Name] [nvarchar](max)NULL,
	[Account] [bigint] NOT NULL,
 CONSTRAINT[PK_dbo.ContractTypes] PRIMARY KEY CLUSTERED
(

   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[AssetTransferMasters](
		   
			   [AssetEntryId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [InvoiceNo] [bigint] NOT NULL,
		   
			   [PurchaseEntry] [nvarchar](50) NULL,
	[AssetEntryDate] [datetime] NOT NULL,
	[VendorName] [bigint] NULL,
	[Vat] [bigint] NULL,
	[McFromId] [bigint] NULL,
	[TotalAssetValue] [decimal](18, 2) NOT NULL,
	[StockTransferId] [bigint] NULL,
 CONSTRAINT[PK_AssetTransferMasters] PRIMARY KEY CLUSTERED
(

   [AssetEntryId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AssetTransferDetails](
		   
			   [AssetItemEntryId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AssetEntryId] [bigint] NOT NULL,
		   
			   [AssetName] [nvarchar](50) NOT NULL,
		   
			   [Barcode] [nvarchar](50) NULL,
	[UnitId] [bigint] NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[TotalPrice] [decimal](18, 2) NOT NULL,
	[DepreciationPercentage] [bigint] NOT NULL,
	[AssetAccountId] [bigint] NOT NULL,
	[DepreciationAccountId] [bigint] NOT NULL,
	[RefItemId] [bigint] NULL,
	[DeleteYN] [nvarchar](10) NULL,
 CONSTRAINT[PK_AssetTransferDetails] PRIMARY KEY CLUSTERED
(

   [AssetItemEntryId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AssetToInventoryMasters](
		   
			   [EntryId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [EntryNo] [bigint] NOT NULL,
		   
			   [EntryDate] [datetime] NOT NULL,
		   
			   [AssetAccountId] [bigint] NOT NULL,
		   
			   [TotalAmount] [decimal](18, 2) NOT NULL,
			CONSTRAINT[PK_AssetToInventoryMasters] PRIMARY KEY CLUSTERED
		  (
		  
			  [EntryId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);
			qry = "alter table AssetToInventoryMasters add McFromId bigint";
			executequrypatch(qry);
			qry = "alter table AssetTransferMasters add McFromId bigint";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AssetToInventoryDetails](
		   
			   [ItemEntryId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [EntryId] [bigint] NOT NULL,
		   
			   [AssetId] [bigint] NOT NULL,
		   
			   [AssetName] [nvarchar](50) NOT NULL,
		   
			   [RefItemId] [bigint] NOT NULL,
		   
			   [Barcode] [nvarchar](50) NULL,
	[UnitId] [bigint] NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[TotalPrice] [decimal](18, 2) NOT NULL,
	[DepreciationAccountId] [bigint] NOT NULL,
	[DepreciationPercentage] [bigint] NOT NULL,
 CONSTRAINT[PK_AssetToInventoryDetails] PRIMARY KEY CLUSTERED
(

   [ItemEntryId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);


		

			qry = "ALTER TABLE[dbo].[AdditionalFieldDatas] ADD[Field][bigint] NOT NULL DEFAULT 0";
			executequrypatch(qry);
			qry = "ALTER TABLE[dbo].[TenancyContracts] ALTER COLUMN[Unit] [bigint] NOT NULL";
			executequrypatch(qry);
			qry = "ALTER TABLE[dbo].[AdditionalFields] ADD[Section][nvarchar](max) NOT NULL DEFAULT ''";
			executequrypatch(qry);
			qry = "ALTER TABLE[dbo].[AdditionalFields] ALTER COLUMN[Name] [nvarchar](max)NOT NULL";
			executequrypatch(qry);
			qry = "ALTER TABLE[dbo].[DocumentTypes] ALTER COLUMN[Name] [nvarchar](max)NOT NULL";
			executequrypatch(qry);
			qry = "ALTER TABLE[dbo].[PropertyFeatures] ALTER COLUMN[Feature] [nvarchar](max)NOT NULL";
			executequrypatch(qry);
			qry = "ALTER TABLE[dbo].[PropertyTypes] ALTER COLUMN[Name] [nvarchar](max)NOT NULL";
			executequrypatch(qry);

			qry = "ALTER TABLE[dbo].[Contractors] ADD DEFAULT((0)) FOR[ContractType]";
			executequrypatch(qry);
			qry = "ALTER TABLE[dbo].[Maintenances] ADD[ContractType][bigint]";
				executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[DocumentFiles](

	   [ID][bigint] IDENTITY(1, 1) NOT NULL,

	   [Document] [bigint] NOT NULL,

	   [attachments] [nvarchar](max)NOT NULL,
 CONSTRAINT[PK_dbo.DocumentFiles] PRIMARY KEY CLUSTERED
(

	   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";


			executequrypatch(qry);








			qry = @"CREATE TABLE [dbo].[UserEditDays]( 
    [id][bigint] IDENTITY(1, 1) NOT NULL,

    [userid] [varchar](150) NOT NULL,

    [days] [int] NOT NULL
) ON[PRIMARY];"; 
            executequrypatch(qry);
		
			qry = @"CREATE TABLE[dbo].[AccountMaps](

				[AccountMapId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[AccountId] [bigint] NOT NULL,
			
				[AccountName] [varchar](50) NOT NULL,
			
				[PaymentTypeId] [int] NOT NULL,
			
				[EmployeeId] [bigint] NOT NULL,
			
				[EmployeeName] [varchar](50) NOT NULL,
			 CONSTRAINT[PK_AccountMaps] PRIMARY KEY CLUSTERED
		   (
		   
			   [AccountMapId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = "alter table protasks add driver bigint null"; executequrypatch(qry);
			qry = "alter table accountmaps alter column EmployeeName varchar(100) null"; executequrypatch(qry);


			qry = "alter table accountmaps alter column AccountName varchar(100) null"; executequrypatch(qry);
			qry = @"alter table AccountMaps add  description varchar(50) null";
			executequrypatch(qry);
			qry = @"update AccountMaps set description='Cash' where PaymentTypeId=0
update AccountMaps set description='Card' where PaymentTypeId=1
update AccountMaps set description='Account' where PaymentTypeId=2
update AccountMaps set description='Bank Transfer' where PaymentTypeId=3";

executequrypatch(qry);
			qry = @"alter table AccountMaps add  level int null";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[TaskDocuments](

				[TaskDocumentId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[ProtaskID] [bigint] NOT NULL,
			
				[FileName] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[DoucumentType] [nvarchar](50) NULL,
	[Expiry] [date] NULL,
	[Notes] [nvarchar](200) NULL,
	[DocumentTypeID] [bigint] NOT NULL,
 CONSTRAINT[PK_dbo.TaskDocuments] PRIMARY KEY CLUSTERED
(

   [TaskDocumentId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[DummyJornalBills](

				[DummyJornalBillId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[Jornal] [bigint] NOT NULL,
			
				[BillType] [nvarchar](20) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Type] [nvarchar](20) NULL,
	[Status] [int] NOT NULL,
	[InvoiceNo] [bigint] NULL,
	[NewRefName] [nvarchar](100) NULL,
 CONSTRAINT[PK_dbo.DummyJornalBills] PRIMARY KEY CLUSTERED
(

   [DummyJornalBillId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[JornalBills](

			   [JornalBillId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Jornal] [bigint] NOT NULL,
		   
			   [BillType] [nvarchar](20) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Type] [nvarchar](20) NULL,
	[Status] [int] NOT NULL,
	[InvoiceNo] [bigint] NULL,
	[NewRefName] [nvarchar](100) NULL,
	[payfrom] [bigint] NULL,
 CONSTRAINT[PK_dbo.JornalBills] PRIMARY KEY CLUSTERED
(

   [JornalBillId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY];

			CREATE TABLE[dbo].[JornalPaymentBills](
		   
			   [PaymentBillId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Jornal] [bigint] NOT NULL,
		   
			   [InvoiceNo] [bigint] NULL,
	[BillType] [nvarchar](20) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Type] [nvarchar](20) NULL,
	[NewRefName] [nvarchar](100) NULL,
	[Status] [int] NOT NULL
) ON[PRIMARY]"; executequrypatch(qry);
			qry = "alter table filedocuments add ReminderDate datetime null"; executequrypatch(qry);
			qry = "ALTER TABLE Payments ADD[PaymentStatus][bigint] NULL"; executequrypatch(qry);
			qry = "ALTER TABLE Receipts ADD[ReceiptStatus] [bigint] NULL"; executequrypatch(qry);
			qry = "ALTER TABLE Receipts ADD  OverrideStatus[nvarchar](10) NULL"; executequrypatch(qry);
			qry = "ALTER TABLE Payments ADD  OverrideStatus[nvarchar](10) NULL"; executequrypatch(qry);
			qry = @"
CREATE TABLE [dbo].[PaymentCardTypes](
	[PaymentCardTypeId] [bigint] IDENTITY(1,1) NOT NULL,
	[CardType] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.PaymentCardTypes] PRIMARY KEY CLUSTERED 
(
	[PaymentCardTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
CREATE TABLE [dbo].[Tables](
	[TableId] [bigint] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](max) NULL,
	[AreaId] [bigint] NULL,
	[MaxSeats] [int] NULL,
	[TableStatus] [int] NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[Branch] [bigint] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedBranch_BranchID] [bigint] NULL,
 CONSTRAINT [PK_dbo.Tables] PRIMARY KEY CLUSTERED 
(
	[TableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
CREATE TABLE [dbo].[ItemAddOns](
	[ItemAddOnID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[AddOnItem] [bigint] NOT NULL,
	[Unit] [bigint] NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[MainItem] [bigint] NOT NULL,
	[Note] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[Branch] [bigint] NOT NULL,
	[Status] [int] NOT NULL,
	[ItemId_ItemID] [bigint] NULL,
 CONSTRAINT [PK_dbo.ItemAddOns] PRIMARY KEY CLUSTERED 
(
	[ItemAddOnID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
CREATE TABLE [dbo].[Areas](
	[AreaId] [bigint] IDENTITY(1,1) NOT NULL,
	[AreaName] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_dbo.Areas] PRIMARY KEY CLUSTERED 
(
	[AreaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
CREATE TABLE [dbo].[InstantDiscounts](
	[InstantDiscountId] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemId] [bigint] NOT NULL,
	[OfferPrice] [decimal](18, 2) NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[Branch] [bigint] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedBranch_BranchID] [bigint] NULL,
 CONSTRAINT [PK_dbo.InstantDiscounts] PRIMARY KEY CLUSTERED 
(
	[InstantDiscountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[Estimates](
	[EstimateId] [bigint] IDENTITY(1,1) NOT NULL,
	[QuotNo] [nchar](10) NULL,
	[BillNo] [nchar](10) NULL,
	[EsttDate] [datetime] NULL,
	[Customer] [bigint] NULL,
	[CreatedUser] [nchar](50) NULL,
	[QuotGrandTotal] [decimal](18, 0) NULL,
	[Remarks] [nchar](300) NULL,
	[Project] [bigint] NULL,
	[ProTask] [bigint] NULL,
	[joborderno] [varchar](50) NULL,
	[buildingno] [varchar](50) NULL,
	[siteno] [varchar](50) NULL,
	[flatno] [varchar](50) NULL,
	[quoteref] [varchar](50) NULL,
 CONSTRAINT [PK_Esitmate] PRIMARY KEY CLUSTERED 
(
	[EstimateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[EstimateItems](
	[EstimateItemId] [bigint] IDENTITY(1,1) NOT NULL,
	[EstimateId] [bigint] NOT NULL,
	[invno] [nchar](20) NOT NULL,
	[invdate] [datetime] NOT NULL,
	[description] [nchar](900) NOT NULL,
	[amount] [decimal](18, 0) NOT NULL,
 CONSTRAINT [PK_EstimateItems] PRIMARY KEY CLUSTERED 
(
	[EstimateItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
CREATE TABLE [dbo].[EsBillSundries](
	[EsBillSundryId] [bigint] IDENTITY(1,1) NOT NULL,
	[Estimate] [bigint] NOT NULL,
	[BillSundry] [bigint] NOT NULL,
	[BsValue] [decimal](18, 2) NULL,
	[AmountType] [int] NOT NULL,
	[BsType] [int] NOT NULL,
	[BsAmount] [decimal](18, 2) NULL,
 CONSTRAINT [PK_dbo.EsBillSundries] PRIMARY KEY CLUSTERED 
(
	[EsBillSundryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[Remindersses](
	[ReminderId] [bigint] IDENTITY(1,1) NOT NULL,
	[Reference] [bigint] NOT NULL,
	[Type] [nvarchar](50) NULL,
	[Note] [nvarchar](max) NULL,
	[RDate] [datetime] NULL,
	[RequestBy] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[RStatus] [nvarchar](max) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Status] [int] NOT NULL,
	[actionurl] [varchar](100) NULL,
 CONSTRAINT [PK_dbo.Reminders2] PRIMARY KEY CLUSTERED 
(
	[ReminderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE [dbo].[ReminderAssignedsses](
	[ReminderAssignedID] [bigint] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](50) NULL,
	[EmployeeId] [bigint] NOT NULL,
	[ReminderId] [bigint] NOT NULL,
	[EntryId] [bigint] NOT NULL,
 CONSTRAINT [PK_dbo.ReminderAssigneds2] PRIMARY KEY CLUSTERED 
(
	[ReminderAssignedID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[leaddashbordorders](
	[leaddashboardid] [bigint] IDENTITY(1,1) NOT NULL,
	[lead] [bigint] NOT NULL,
	[dashboardposition] [bigint] NOT NULL,
	[duration] [int] NULL,
 CONSTRAINT [PK_leaddashbordorder] PRIMARY KEY CLUSTERED 
(
	[leaddashboardid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[protaskdashbordorders](
	[protaskdashboardid] [bigint] IDENTITY(1,1) NOT NULL,
	[task] [bigint] NOT NULL,
	[dashboardposition] [bigint] NOT NULL,
	[duration] [int] NULL,
 CONSTRAINT [PK_protaskdashbordorder] PRIMARY KEY CLUSTERED 
(
	[protaskdashboardid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
CREATE TABLE [dbo].[ProcessFlowAssignUserstoleads](
	[ProcessFlowAssignUserId] [bigint] IDENTITY(1,1) NOT NULL,
	[ProcessFlowId] [bigint] NOT NULL,
	[EmployeeId] [bigint] NOT NULL,
 CONSTRAINT [PK_dbo.ProcessFlowAssignUserstolead] PRIMARY KEY CLUSTERED 
(
	[ProcessFlowAssignUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];

CREATE TABLE [dbo].[PriceCategoryPercentages](
	[CategoryId] [bigint] IDENTITY(1,1) NOT NULL,
	[Category] [nvarchar](50) NULL,
	[PriceCategory] [nvarchar](50) NULL,
	[Percentage] [bigint] NULL,
 CONSTRAINT [PK_PriceCategoryPercentages] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
CREATE TABLE [dbo].[PriceCategoryMasters](
	[CategoryId] [bigint] IDENTITY(1,1) NOT NULL,
	[Category] [nvarchar](50) NULL,
 CONSTRAINT [PK_PriceCategoryMaster] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
CREATE TABLE [dbo].[customerleadrelations](
	[assignid] [bigint] IDENTITY(1,1) NOT NULL,
	[customerid] [bigint] NOT NULL,
	[leadid] [bigint] NOT NULL,
 CONSTRAINT [PK_customerleadrelation] PRIMARY KEY CLUSTERED 
(
	[assignid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
CREATE TABLE [dbo].[purchaseentrydocuments](
	[purid] [bigint] IDENTITY(1,1) NOT NULL,
	[PurchaseId] [bigint] NOT NULL,
	[FileName] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[DoucumentType] [nvarchar](50) NULL,
	[Expiry] [date] NULL,
	[Notes] [nvarchar](200) NULL,
 CONSTRAINT [PK_dbo.purchaseentrydocuments] PRIMARY KEY CLUSTERED 
(
	[purid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[ChequeDetails](

				[Id][bigint] IDENTITY(1, 1) NOT NULL,
			
				[TransId] [bigint] NOT NULL,
			
				[TransType] [nvarchar](50) NOT NULL,
			
				[ChequeDate] [datetime] NULL,
	[ChequeNo] [nvarchar](50) NULL,
 CONSTRAINT[PK_ChequeDetails] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

		
			qry = "alter table contacts alter column contactperson varchar(1000) null"; executequrypatch(qry);

			qry = "alter table AssetToInventoryMasters add McFromId bigint"; executequrypatch(qry);
			qry = "ALTER TABLE Protasks ADD VModId bigint"; executequrypatch(qry);
			qry = "ALTER TABLE Protasks ADD VManuId bigint"; executequrypatch(qry);
			qry = "ALTER TABLE Protasks ADD VTypId bigint"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[VehicleTypes](

			  [VTypeId][bigint] IDENTITY(1, 1) NOT NULL,
		  
			  [Type] [nvarchar](50) NULL,
 CONSTRAINT[PK_VehicleTypes] PRIMARY KEY CLUSTERED
(

   [VTypeId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[VehicleModels](

			   [ModelId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Model] [nvarchar](50) NULL,
	[VTId] [bigint] NULL,
	[MaId] [bigint] NULL,
 CONSTRAINT[PK_VehicleModels] PRIMARY KEY CLUSTERED
(

   [ModelId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[VehicleManufacturers](

			   [MId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Manufacturer] [nvarchar](50) NULL,
	[VTyId] [bigint] NULL,
 CONSTRAINT[PK_VehicleManufacturers] PRIMARY KEY CLUSTERED
(

   [MId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[CustomerSatisfactions](

				[Id][bigint] IDENTITY(1, 1) NOT NULL,
			
				[ProTaskId] [bigint] NOT NULL,
			
				[SatisfactionLevel] [varchar](50) NOT NULL,
			
				[Comments] [nvarchar](max)NULL,
	[Signature] [nvarchar](max)NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT[PK_CustomerSatisfaction] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[leadcustomerrelations](

							[assignid][bigint] IDENTITY(1, 1) NOT NULL,
			
				[customerid] [bigint] NULL,
	[leadid] [bigint] NULL,
 CONSTRAINT[PK_leadcustomerrelations] PRIMARY KEY CLUSTERED
(

   [assignid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TYPE[dbo].[TableTypeMRItemsNew] AS TABLE(
			   [ItemUnit][int] NULL,
	[ItemQuantity] [decimal](18, 2) NOT NULL,
	[Item] [int] NOT NULL,
	[Make] [bigint] NOT NULL,
	[itemNote] [varchar](max)NOT NULL,
	[MaterialRequisition] [int] NOT NULL,
	[ItemRemark] [varchar](max)NOT NULL,
	[TargetPrice] [decimal](18, 2) NULL
)"; executequrypatch(qry);

			qry = "ALTER TABLE MaterialRequisitions Add ReminderDate DateTime"; executequrypatch(qry);

			qry = "ALTER TABLE MaterialRequisitions Add SupplierId bigint"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AddedRemarks](

			   [RemarkId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [TransactionId] [bigint] NOT NULL,
		   
			   [TransactionType] [nvarchar](max)NOT NULL,
	[Remarks] [nvarchar](max)NOT NULL,
	[AddedUser] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT[PK_AddedRemarks] PRIMARY KEY CLUSTERED
(

   [RemarkId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[SupplierItems](

				[Id][bigint] IDENTITY(1, 1) NOT NULL,
			
				[SupplierId] [bigint] NOT NULL,
			
				[ItemId] [bigint] NOT NULL,
			 CONSTRAINT[PK_SupplierItems] PRIMARY KEY CLUSTERED
		   (
		   
			   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[SupplierCategories](

			   [Id][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [SupplierId] [bigint] NOT NULL,
		   
			   [CategoryId] [bigint] NOT NULL,
			CONSTRAINT[PK_SupplierCategories] PRIMARY KEY CLUSTERED
		  (
		  
			  [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[SupplierBrands](

			   [Id][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [SupplierId] [bigint] NOT NULL,
		   
			   [BrandId] [bigint] NOT NULL,
			CONSTRAINT[PK_SupplierBrands] PRIMARY KEY CLUSTERED
		  (
		  
			  [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);




			qry = @"CREATE TABLE[dbo].[RemarkCustomers](

				[RemarkId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[CustomerId] [bigint] NOT NULL,
			
				[Remark] [nvarchar](max)NOT NULL,
	[AddedUser] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT[PK_RemarksCustomer] PRIMARY KEY CLUSTERED
(

   [RemarkId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);


			qry = "ALTER TABLE PurchaseOrders ADD[PurchaseOrderStatus][int] NULL"; executequrypatch(qry);


			qry = "alter table customers alter column category bigint null"; executequrypatch(qry);


			qry = "Alter Table Customers   Add StartDate DateTime"; executequrypatch(qry);
			qry = "Alter Table Customers Add EndDate DateTime"; executequrypatch(qry);
			qry = "ALTER TABLE CUSTOMERS ADD CustomerType BIGINT"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[CustomerTyps](

			   [TypeId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Type] [nvarchar](50) NULL,
 CONSTRAINT[PK_CustomerTypes] PRIMARY KEY CLUSTERED
(

   [TypeId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = "alter table itemtasks add itemtasklistid bigint null"; executequrypatch(qry);
			qry = "alter table itemtasks add  seitemid bigint null"; executequrypatch(qry);
			qry = "Alter table Payments Add InvoiceNo nvarchar(max)"; executequrypatch(qry);
			qry = "Alter table Journals Add InvoiceNo nvarchar(max)"; executequrypatch(qry);


			qry = "alter table items add slreq bit not null  default(0)"; executequrypatch(qry);
			qry = "alter table items add daysexpirty bigint null"; executequrypatch(qry);

			qry = "alter table items add accmap bit not null  default(0)"; executequrypatch(qry);


			qry = "alter table quotations alter column[revision] varchar(50) null"; executequrypatch(qry);


			qry = "insert into appmodules values('fdb5ecb3-9696-4f52-8a60-856ba14f99PeriodicProcessFlow', 'Periodic Process Flow', '6077', 'Periodic Process Flow', '/AMCPeriodicProcessFlow/Index', '1059', 'NULL', '1', 'NULL', '0', '0', 'AppModules', 'fa-circle-o', '0', '6')"; executequrypatch(qry);

			qry = "insert into appmodules values('9c2a07d5-36fb-49e0-b397-acd6ad4392fcAMCPERIODIC','Periodic Maintanance','6073','Periodic Maintanance','#','6061','NULL','0','NULL','0','0','AppModules','fa-circle-o','0','2')"; executequrypatch(qry);
			qry = "insert into appmodules values('fdb5ecb3-9696-4f52-8a60-856ba14f99periodicCcreate','AMC Periodic Create','6074','Create','/AMCPeriodicMaintenance/Create','6073','NULL','1','NULL','0','0','AppModules','fa-circle-o','0','1')"; executequrypatch(qry);
			qry = "insert into appmodules values('fdb5ecb3-9696-4f52-8a60-856ba14f99periodicList','AMC Periodic List','6075','List','/AMCPeriodicMaintenance/Index','6073','NULL','1','NULL','0','0','AppModules','fa-circle-o','0','2')"; executequrypatch(qry);


			qry = "Alter table Payments Add InvoiceNo nvarchar(max)"; executequrypatch(qry);
			qry = "Alter table Journals Add InvoiceNo nvarchar(max)"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[PeriodicMaintAssignedTeams](

			   [AssignedTeamId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [PeriodicMaintDtlId] [bigint] NOT NULL,
		   
			   [TeamId] [bigint] NOT NULL,
			CONSTRAINT[PK_PeriodicMaintAssignedTeams] PRIMARY KEY CLUSTERED
		  (
		  
			  [AssignedTeamId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);





			qry = @"CREATE TABLE[dbo].[PeriodicMaintAssignedToes](

			   [AssignedToId][bigint] IDENTITY(1, 1) NOT NULL,

   [PeriodicMaintDtlId] [bigint] NOT NULL,

   [EmployeeId] [bigint] NOT NULL,

   [AssignBy] [nvarchar](200) NULL,
	[Status] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ChkStatus] [int] NOT NULL,
	[Approve] [bit] NULL,
 CONSTRAINT[PK_PeriodicMaintAssignedToes] PRIMARY KEY CLUSTERED
(

   [AssignedToId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);





			qry = @"CREATE TABLE[dbo].[PeriodicProcessFlows](

			   [PeriodicProcessFlowId][bigint] IDENTITY(1, 1) NOT NULL,

   [PeriodicStatus] [bigint] NOT NULL,

   [RemoveUpdateUser] [bit] NOT NULL,

   [RemoveUpdateUserTeams] [bit] NOT NULL,

   [Status] [int] NOT NULL,

   [CreatedDate] [datetime] NOT NULL,

   [CreatedBy] [nvarchar](max)NULL,
	[Branch] [bigint] NOT NULL,
	[CreatedBranch_BranchID] [bigint] NULL,
	[AssignExistingUser] [bit] NULL,
 CONSTRAINT[PK_dbo.PeriodicProcessFlows] PRIMARY KEY CLUSTERED
(

   [PeriodicProcessFlowId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[PeriodicProcessFlowAssignTypes](

			   [PerdcProcessFlowAssignTypeId][bigint] IDENTITY(1, 1) NOT NULL,

   [PerdcProcessFlowId] [bigint] NOT NULL,

   [TeamId] [bigint] NOT NULL,
CONSTRAINT[PK_dbo.PeriodicProcessFlowAssignTypes] PRIMARY KEY CLUSTERED
(

  [PerdcProcessFlowAssignTypeId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[PeriodicProcessFlowAssignUsers](

			   [PerdcProcessFlowAssignUserId][bigint] IDENTITY(1, 1) NOT NULL,

   [PerdcProcessFlowId] [bigint] NOT NULL,

   [EmployeeId] [bigint] NOT NULL,
CONSTRAINT[PK_dbo.PeriodicProcessFlowAssignUsers] PRIMARY KEY CLUSTERED
(

  [PerdcProcessFlowAssignUserId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);





			qry = "ALTER TABLE AmcDocuments ALTER COLUMN DocumentTypeID bigint NULL"; executequrypatch(qry);
			qry = "ALTER TABLE AmcDocuments ALTER  COLUMN Expiry Date NULL"; executequrypatch(qry);

			qry = "ALTER TABLE BoqItems Add ItemNote nvarchar(max)"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[AmcProcessFlows](

			   [AmcProcessFlowId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcStatus] [bigint] NOT NULL,
		   
			   [RemoveUpdateUser] [bit] NOT NULL,
		   
			   [RemoveUpdateUserTeams] [bit] NOT NULL,
		   
			   [Status] [int] NOT NULL,
		   
			   [CreatedDate] [datetime] NOT NULL,
		   
			   [CreatedBy] [nvarchar](max)NULL,
	[Branch] [bigint] NOT NULL,
	[CreatedBranch_BranchID] [bigint] NULL,
	[AssignExistingUser] [bit] NULL,
 CONSTRAINT[PK_dbo.AmcProcessFlows] PRIMARY KEY CLUSTERED
(

   [AmcProcessFlowId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AmcProcessFlowAssignTypes](

			   [AmcProcessFlowAssignTypeId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcProcessFlowId] [bigint] NOT NULL,
		   
			   [TeamId] [bigint] NOT NULL,
			CONSTRAINT[PK_dbo.AmcProcessFlowAssignTypes] PRIMARY KEY CLUSTERED
		  (
		  
			  [AmcProcessFlowAssignTypeId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY];
			CREATE TABLE[dbo].[AmcProcessFlowAssignUsers](
		   
			   [AmcProcessFlowAssignUserId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcProcessFlowId] [bigint] NOT NULL,
		   
			   [EmployeeId] [bigint] NOT NULL,
			CONSTRAINT[PK_dbo.AmcProcessFlowAssignUsers] PRIMARY KEY CLUSTERED
		  (
		  
			  [AmcProcessFlowAssignUserId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);





			qry = "alter table propertymains add LandlordID bigint null"; executequrypatch(qry);
			qry = "alter table customers add logtime datetime null"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[AmcStatusDepts](

				[AmcStatusDeptId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[AmcStatusId] [bigint] NOT NULL,
			
				[DeptId] [bigint] NOT NULL,
			 CONSTRAINT[PK_dbo.AmcStatusDepts] PRIMARY KEY CLUSTERED
		   (
		   
			   [AmcStatusDeptId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[AmcStatusDesgs](

			   [AmcStatusDesgId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcStatusId] [bigint] NOT NULL,
		   
			   [DesgId] [bigint] NOT NULL,
			CONSTRAINT[PK_dbo.AmcStatusDesgs] PRIMARY KEY CLUSTERED
		  (
		  
			  [AmcStatusDesgId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[AmcStatusDepts](

				[AmcStatusDeptId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[AmcStatusId] [bigint] NOT NULL,
			
				[DeptId] [bigint] NOT NULL,
			 CONSTRAINT[PK_dbo.AmcStatusDepts] PRIMARY KEY CLUSTERED
		   (
		   
			   [AmcStatusDeptId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY];

			CREATE TABLE[dbo].[AmcStatusDesgs](
		   
			   [AmcStatusDesgId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcStatusId] [bigint] NOT NULL,
		   
			   [DesgId] [bigint] NOT NULL,
			CONSTRAINT[PK_dbo.AmcStatusDesgs] PRIMARY KEY CLUSTERED
		  (
		  
			  [AmcStatusDesgId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = "ALTER TABLE AMCs DROP COLUMN Expiry, StatusUpdtDate"; executequrypatch(qry);
			qry = "ALTER TABLE PeriodicMaintenanceDetails ADD PeriodicMaintStatus bigint"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[BillofQties](

				[BoqId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[CreatedDate] [datetime] NULL,
	[Customer] [bigint] NOT NULL,
	[SalesExecutive] [bigint] NULL,
	[Status] [int] NULL,
	[BoqDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[QuotNo] [nvarchar](50) NULL,
	[BillNo] [bigint] NOT NULL,
 CONSTRAINT[PK_BillofQties] PRIMARY KEY CLUSTERED
(

   [BoqId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[BoqItems](

			   [BoqItemId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [BoqId] [bigint] NOT NULL,
		   
			   [ItemId] [bigint] NOT NULL,
		   
			   [Quantity] [decimal](18, 2) NOT NULL,
		   
			   [Unit] [bigint] NULL,
 CONSTRAINT[PK_dbo.BoqItems] PRIMARY KEY CLUSTERED
(

   [BoqItemId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[AMCs](

							[AmcId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[AmcNo] [bigint] NOT NULL,
			
				[ContractId] [bigint] NOT NULL,
			
				[CustomerId] [bigint] NOT NULL,
			
				[StartDate] [datetime] NOT NULL,
			
				[EndDate] [datetime] NOT NULL,
			
				[ReminderDate] [datetime] NOT NULL,
			
				[ContractTypeId] [bigint] NULL,
	[ContractLevelId] [int] NULL,
	[LocationId] [bigint] NULL,
	[Lattitude] [nvarchar](max)NULL,
	[Longitude] [nvarchar](max)NULL,
	[Ref1] [nvarchar](50) NULL,
	[Ref2] [nvarchar](50) NULL,
	[Ref3] [nvarchar](50) NULL,
	[Ref4] [nvarchar](50) NULL,
	[Ref5] [nvarchar](50) NULL,
	[PeriodicMaintReqrd] [bit] NOT NULL,
	[AmcStatusId] [bigint] NULL,
	[Expiry] [datetime] NULL,
	[StatusUpdtDate] [datetime] NULL,
	[AmcDetails] [nvarchar](max)NULL,
	[Notes] [nvarchar](max)NULL,
	[CreatedBy] [varchar](max)NULL,
	[CreatedDate] [datetime] NULL,
	[LogTime] [datetime] NULL,
 CONSTRAINT[PK_AMCs] PRIMARY KEY CLUSTERED
(

   [AmcId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AmcRemarks](

			   [AmcRemarkId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcId] [bigint] NOT NULL,
		   
			   [AddedUser] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[Level] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[AmcUpdationId] [bigint] NULL,
	[AmcStatusId] [bigint] NULL,
 CONSTRAINT[PK_dbo.AmcRemarks] PRIMARY KEY CLUSTERED
(

   [AmcRemarkId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY];
			CREATE TABLE[dbo].[AmcDocuments](
		   
			   [AmcDocumentId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcId] [bigint] NOT NULL,
		   
			   [DocumentTypeID] [bigint] NOT NULL,
		   
			   [DocumentType] [nvarchar](50) NULL,
	[Expiry] [date] NOT NULL,
	[Notes] [nvarchar](200) NULL,
	[FileName] [nvarchar](max)NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT[PK_AmcDocuments] PRIMARY KEY CLUSTERED
(

   [AmcDocumentId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AmcContractTypes](

			   [TypeId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Type] [nvarchar](50) NULL,
 CONSTRAINT[PK_AmcContractTypes] PRIMARY KEY CLUSTERED
(

   [TypeId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[AmcContracts](

			   [ContractId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [ContractName] [nvarchar](100) NULL,
 CONSTRAINT[PK_Contracts] PRIMARY KEY CLUSTERED
(

   [ContractId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AmcAssignedToes](

			   [AssignedToId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcId] [bigint] NOT NULL,
		   
			   [EmployeeId] [bigint] NOT NULL,
		   
			   [AssignBy] [nvarchar](200) NULL,
	[Status] [nvarchar](50) NULL,
	[CreatedDate] [datetime] NULL,
	[ChkStatus] [int] NOT NULL,
	[Approve] [bit] NULL,
 CONSTRAINT[PK_AmcAssignedTos] PRIMARY KEY CLUSTERED
(

   [AssignedToId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AmcAssignedTeams](

			   [AssignedTeamId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcId] [bigint] NOT NULL,
		   
			   [TeamId] [bigint] NOT NULL,
			CONSTRAINT[PK_AmcAssignedTeams] PRIMARY KEY CLUSTERED
		  (
		  
			  [AssignedTeamId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AmcStatus](

			   [AmcStatusId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [StatusName] [nvarchar](max)NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[Branch] [bigint] NULL,
 CONSTRAINT[PK_AmcStatus] PRIMARY KEY CLUSTERED
(

   [AmcStatusId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[AmcUpdations](

			   [AmcUpdationID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcId] [bigint] NOT NULL,
		   
			   [CreatedBy] [nvarchar](max)NOT NULL,
	[CreatedDate] [datetime] NULL,
	[Location] [nvarchar](max)NULL,
	[Lattitude] [nvarchar](100) NULL,
	[Longitude] [nvarchar](100) NULL,
	[Remarks] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.AmcUpdations] PRIMARY KEY CLUSTERED
(

   [AmcUpdationID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[PeriodicMaintenances](

			   [PeriodicMaintenanceId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [AmcId] [bigint] NOT NULL,
		   
			   [NoOfPMaintenance] [bigint] NOT NULL,
			CONSTRAINT[PK_PeriodicMaintenances] PRIMARY KEY CLUSTERED
		  (
		  
			  [PeriodicMaintenanceId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[PeriodicMaintenanceDetails](

			   [PeriodicMaintDetailsId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [PeriodicMaintenanceId] [bigint] NOT NULL,
		   
			   [PDate] [datetime] NOT NULL,
		   
			   [Notes] [nvarchar](max)NULL,
 CONSTRAINT[PK_PeriodicMaintenanceDetails] PRIMARY KEY CLUSTERED
(

   [PeriodicMaintDetailsId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);










			qry = "ALTER TABLE PurchaseEntries ADD PurchaseStatus int"; executequrypatch(qry);


			qry = "ALTER TABLE Quotations ADD QuotationType bigint null"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[QuotationTypes](

			   [QuotId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [QuotType] [nvarchar](50) NULL,
 CONSTRAINT[PK_QuotationType] PRIMARY KEY CLUSTERED
(

   [QuotId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[ItemTransactions](

							[ItemTransId][bigint] IDENTITY(1, 1) NOT NULL,
			
				[ItemId] [bigint] NOT NULL,
			
				[McId] [bigint] NOT NULL,
			
				[TotalStock] [decimal](18, 2) NOT NULL,
			
				[LastUpdatedBy] [nvarchar](max)NULL,
	[LastUpdatedDate] [datetime] NULL,
 CONSTRAINT[PK_ItemTransactions] PRIMARY KEY CLUSTERED
(

   [ItemTransId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[itemsizeprices](

							[sizepriceid][bigint] IDENTITY(1, 1) NOT NULL,
			
				[itemid] [bigint] NOT NULL,
			
				[sizeid] [bigint] NOT NULL,
			
				[price] [decimal](18, 2) NULL,
 CONSTRAINT[PK_itemsizeprice] PRIMARY KEY CLUSTERED
(

   [sizepriceid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = "insert into appmodules values('9c2a07d5-36fb-49e0-b397-acd6ad4392tendering','Sales Tendering','6092','Sales Tendering','#','1100','NULL','1','NULL','0','0','AppModules','NULL','1','101')";
			executequrypatch(qry);

			qry = "alter table POSOrders add dcharge decimal(18, 2) default 0"; executequrypatch(qry);
			qry = "alter table POSOrders add tendering decimal(18, 2) default 0"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[ItemRemarks](

			   [ItemRemarkId][bigint] IDENTITY(1, 1) NOT NULL,

   [ItemId] [bigint] NOT NULL,

   [Remark] [nvarchar](max)NOT NULL,
	[LastUpdatedBy] [nvarchar](max)NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL,
 CONSTRAINT[PK_ItemRemarks] PRIMARY KEY CLUSTERED
(

   [ItemRemarkId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[StockVerifications](

			   [StockVerificationId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [VoNo] [bigint] NOT NULL,
		   
			   [Voucher] [nvarchar](max)NOT NULL,
	[Date] [datetime] NOT NULL,
	[CheckDate] [datetime] NULL,
	[CheckTime] [datetime] NULL,
	[Batch] [nvarchar](50) NULL,
	[totalPcs] [decimal](18, 2) NOT NULL,
	[scannedPcs] [decimal](18, 2) NOT NULL,
	[remainPcs] [decimal](18, 2) NOT NULL,
	[totalWeight] [decimal](18, 2) NOT NULL,
	[scannedWeight] [decimal](18, 2) NOT NULL,
	[remainWeight] [decimal](18, 2) NOT NULL,
	[Note] [nvarchar](50) NULL,
	[Remarks] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[Branch] [bigint] NOT NULL,
	[editable] [int] NULL,
	[Status] [int]  NULL,
 CONSTRAINT[PK_StockVerifications] PRIMARY KEY CLUSTERED
(

   [StockVerificationId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[SVItems](

			   [SVItemsId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [StockVerification] [bigint] NOT NULL,
		   
			   [Item] [bigint] NOT NULL,
		   
			   [ItemUnit] [bigint] NULL,
	[CSPcs] [decimal](18, 2) NOT NULL,
	[CSqty] [decimal](18, 2) NOT NULL,
	[PSPcs] [decimal](18, 2) NOT NULL,
	[PSqty] [decimal](18, 2) NOT NULL,
	[SDPcs] [decimal](18, 2) NOT NULL,
	[SDqty] [decimal](18, 2) NOT NULL,
 CONSTRAINT[PK_SVItems] PRIMARY KEY CLUSTERED
(

   [SVItemsId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[EmpAttendances](

							[Id][bigint] IDENTITY(1, 1) NOT NULL,
			
				[EmployeeName] [nvarchar](150) NULL,
	[login] [datetime] NULL,
	[logout] [datetime] NULL,
	[Status] [nvarchar](max)NULL,
	[logitude] [varchar](200) NULL,
	[latitude] [varchar](200) NULL,
	[endlogitude] [varchar](200) NULL,
	[endlatitude] [varchar](200) NULL,
	[protaskid] [bigint] NULL,
 CONSTRAINT[PK_dbo.EmpAttendance] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[EmpAttDetails](

			   [empattdetailsid][bigint] IDENTITY(1, 1) NOT NULL,

   [taskstatusid] [bigint] NOT NULL,

   [userid] [nchar](200) NOT NULL,

   [starttime] [datetime] NOT NULL,

   [protaskid] [bigint] NOT NULL,

   [empattid] [bigint] NOT NULL,
CONSTRAINT[PK_EmpAttDetails] PRIMARY KEY CLUSTERED
(

  [empattdetailsid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[CustomerRemarks](

			   [CustomerRemarkId][bigint] IDENTITY(1, 1) NOT NULL,

   [CustomerId] [bigint] NOT NULL,

   [Remark] [nvarchar](max)NOT NULL,
	[AddedUser] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT[PK_CustomerRemarks] PRIMARY KEY CLUSTERED
(

   [CustomerRemarkId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);


qry = @"CREATE TABLE[dbo].[itemtasklists](

   [fieldmcid][bigint] IDENTITY(1, 1) NOT NULL,

   [protaskid] [bigint] NOT NULL,

   [userid] [nchar](200) NOT NULL,

   [mcfrom] [bigint] NOT NULL,

   [itemid] [bigint] NOT NULL,

   [qty] [decimal](18, 0) NOT NULL,

   [unit] [bigint] NOT NULL,

   [stocktransferitid] [bigint] NOT NULL,
CONSTRAINT[PK_itemtasklist] PRIMARY KEY CLUSTERED
(

  [fieldmcid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[AdditionalMcs](

			   [NewMcId][bigint] IDENTITY(1, 1) NOT NULL,

   [UserId] [varchar](100) NOT NULL,

   [McId] [bigint] NOT NULL,

   [McName] [varchar](50) NULL,
 CONSTRAINT[PK_additionalmc] PRIMARY KEY CLUSTERED
(

   [NewMcId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[CustomerDocuments](
				[DocumnetId][bigint] IDENTITY(1, 1) NOT NULL,
	[CutomerID] [bigint] NOT NULL,
	[DoucumentType] [nvarchar](50) NOT NULL,
	[Expiry] [date] NOT NULL,
	[Notes] [nvarchar](200) NULL,
	[FilePath] [nvarchar](100) NULL,
	[ContactId] [bigint] NULL,
 CONSTRAINT[PK_CustomerDocuments] PRIMARY KEY CLUSTERED
(

   [DocumnetId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = @"update AppModules set parent=1002,menuorder=105 where ModulesID in(165,167,1119,168,166)

alter table customers alter column customername VARCHAR(1000) null

alter table Mobiles alter column Name VARCHAR(1000) null
alter table quotations add leadsid bigint null
alter table quotations add expdate datetime null
alter table quotations add quotationstatus int null
alter table quotations add revision varchar(5) null
alter table ItemTasks add protaskid bigint not null

alter table ItemTasks add invoiced int null

alter table ItemTaskMasters alter column TaskName VARCHAR(300)";
			executequrypatch(qry);
qry = "ALTER TABLE Customers ADD CustomerPrintName NVARCHAR(60) NULL"; executequrypatch(qry);

			qry = "ALTER TABLE Customers ADD SourceID INT NOT NULL DEFAULT(0)"; executequrypatch(qry);
			qry = "ALTER TABLE Customers ADD CountryID INT NOT NULL DEFAULT(0)"; executequrypatch(qry);

			qry = "ALTER TABLE Customers ADD LocationID INT NOT NULL DEFAULT(0)"; executequrypatch(qry);

			qry = "ALTER TABLE Contacts ADD TypeOfContact NVARCHAR(60) NULL"; executequrypatch(qry);

			qry = "ALTER TABLE Contacts ADD Website NVARCHAR(60) NULL"; executequrypatch(qry);

			qry = "ALTER TABLE Accounts ADD PrvYearBalance DECIMAL(18,2) NOT NULL DEFAULT(0)"; executequrypatch(qry);

			qry = "ALTER TABLE SalesEntries ADD DueDate datetime null"; executequrypatch(qry);

			qry = "ALTER TABLE SalesEntries ADD DueReason NVARCHAR(250) null"; executequrypatch(qry);




			qry = @"CREATE TABLE[dbo].[CutomerDocument](
			  [DocumnetId][bigint] IDENTITY(1, 1) NOT NULL,
  [CutomerID][bigint] NOT NULL,
  [DoucumentType][nvarchar](50) NOT NULL,
  [Expiry][date] NOT NULL,
  [Notes][nvarchar](200) NULL,
  [FilePath][nvarchar](100) NOT NULL,
CONSTRAINT[PK_CutomerDocument] PRIMARY KEY CLUSTERED
(
  [DocumnetId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);






			qry = @"CREATE TABLE[dbo].[ContactRelations](
			  [ContactRelationID][bigint] IDENTITY(1, 1) NOT NULL,
  [ContactID][bigint] NOT NULL,
  [RelationType][bigint] NOT NULL,
  [RelationID][bigint] NOT NULL,
CONSTRAINT[PK_ContactRelation] PRIMARY KEY CLUSTERED
(
  [ContactRelationID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = "ALTER TABLE[dbo].[ContactRelation] ADD CONSTRAINT[DF_ContactRelation_ContactID]  DEFAULT((0)) FOR[ContactID]"; executequrypatch(qry);


			qry = "ALTER TABLE[dbo].[ContactRelation] ADD CONSTRAINT[DF_ContactRelation_RelationType]  DEFAULT((0)) FOR[RelationType]"; executequrypatch(qry);


			qry = "ALTER TABLE[dbo].[ContactRelation] ADD CONSTRAINT[DF_ContactRelation_RelationID]  DEFAULT((0)) FOR[RelationID]"; executequrypatch(qry);



			qry = "ALTER TABLE Customers ADD TaxID_TRN NVARCHAR(70) null"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[Countries](
				[CountryID][int] IDENTITY(1, 1) NOT NULL,
	[CountryCode] [nvarchar](50) NULL,
	[CountryName] [nvarchar](100) NOT NULL,
 CONSTRAINT[PK_Country] PRIMARY KEY CLUSTERED
(

   [CountryID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[LeadConditions](
				[id][int] IDENTITY(1, 1) NOT NULL,
	[LeadCondition] [nvarchar](60) NOT NULL,
 CONSTRAINT[PK_LeadCondition] PRIMARY KEY CLUSTERED
(

   [id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = "alter table customers add  addres varchar(max) null"; executequrypatch(qry);
			qry = "alter table Suppliers add Addres nvarchar(max) null"; executequrypatch(qry);
			qry = "alter table Customers add Category nvarchar(50)"; executequrypatch(qry);
			qry = "alter table customers alter column category bigint null"; executequrypatch(qry);
			qry = "INSERT INTO PriceCategoryMasters(Category) VALUES('default')"; executequrypatch(qry);
			qry = "alter table TaskImages add description varchar(500) NULL"; executequrypatch(qry);
			qry = "alter table TaskImages add newdescription varchar(500) NULL"; executequrypatch(qry); ;
						qry = "alter table customers add CreatedBy varchar(max) null";
			executequrypatch(qry);
			qry = "alter table Customers add[Ref1] [nvarchar](50) NULL,[Ref2] [nvarchar](50) NULL,[Ref3] [nvarchar](50) NULL,[Ref4] [nvarchar](50) NULL,[Ref5] [nvarchar](50) NULL";
			executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[ContactTypes](
	[ContactId] [bigint] IDENTITY(1,1) NOT NULL,
	[Type] [nvarchar](50) NULL,
 CONSTRAINT [PK_ContactTypes] PRIMARY KEY CLUSTERED 
(
	[ContactId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[AttachmentDocuments](

   [DocumentID][bigint] IDENTITY(1, 1) NOT NULL,

   [TransactionID] [bigint] NOT NULL,

   [TransactionType] [nvarchar](50) NOT NULL,

   [FileName] [nvarchar](max)NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[DocumentType] [nvarchar](50) NULL,
	[Expiry] [date] NULL,
	[Notes] [nvarchar](200) NULL,
 CONSTRAINT[PK_AttachmentDocuments] PRIMARY KEY CLUSTERED
(

   [DocumentID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[FilemultipleDocuments](

			   [Id][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [RelationID] [bigint] NOT NULL,
		   
			   [DocumentName] [nvarchar](150) NULL,
	[ExpiryDate] [datetime] NULL,
	[Document] [nvarchar](max)NULL,
	[Note] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[Branch] [bigint] NULL,
	[Status] [int] NULL,
 CONSTRAINT[PK_dbo.FilemultipleDocuments] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[ItemSerialNumbers](

			   [itemserialnoid][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [itemid] [bigint] NOT NULL,
		   
			   [serialno] [varchar](150) NOT NULL,
		   
			   [expirydate] [datetime] NULL,
 CONSTRAINT[PK_ItemSerialNo] PRIMARY KEY CLUSTERED
(

   [itemserialnoid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[ItemSerialNumbersused](

			   [itemserialnousedid][bigint] IDENTITY(1, 1) NOT NULL,
		   

			   [serialno] [varchar](150) NOT NULL,
		   
			   [referanceid] [bigint] not null,
	[purpose] [varchar](150) NOT NULL,
 CONSTRAINT[PK_ItemSerialNoused] PRIMARY KEY CLUSTERED
(

   [itemserialnousedid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]";

			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[Locations](
				[LocationID][int] IDENTITY(1, 1) NOT NULL,
	[LocationCode] [nvarchar](50) NULL,
	[LocationName] [nvarchar](100) NOT NULL,
	[StateID] [int] NOT NULL,
 CONSTRAINT[PK_Location] PRIMARY KEY CLUSTERED
(

   [LocationID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE [dbo].[LocationNames](
	[LocationId] [bigint] IDENTITY(1,1) NOT NULL,
	[Location] [nvarchar](50) NULL,
	[StateID] [bigint] NULL,
 CONSTRAINT [PK_LocationName] PRIMARY KEY CLUSTERED 
(
	[LocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[States](
				[StateID][int] IDENTITY(1, 1) NOT NULL,
	[StateCode] [nvarchar](50) NULL,
	[StateName] [nvarchar](50) NOT NULL,
	[CountryID] [int] NOT NULL,
 CONSTRAINT[PK_States] PRIMARY KEY CLUSTERED
(

   [StateID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[Actions](
				[id][int] IDENTITY(1, 1) NOT NULL,
	[ActionName] [nvarchar](100) NOT NULL,
 CONSTRAINT[PK_Actions] PRIMARY KEY CLUSTERED
(

   [id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = "ALTER TABLE Customers ADD LeadType INT NOT NULL DEFAULT(0)"; executequrypatch(qry);

			qry = "ALTER TABLE Customers ADD LeadCondition INT NOT NULL DEFAULT(0)"; executequrypatch(qry);
			qry = "ALTER TABLE Customers ADD StateID INT NOT NULL DEFAULT(0)"; executequrypatch(qry);
			qry = "ALTER TABLE Customers ADD CurrentAction INT NOT NULL DEFAULT(0)"; executequrypatch(qry);
			qry = "ALTER TABLE Customers ADD NextAction INT NOT NULL DEFAULT(0)"; executequrypatch(qry);
			qry = "ALTER TABLE Customers ADD CreatedDate datetime  NULL"; executequrypatch(qry);
			qry = "ALTER TABLE Customers ADD TaxRegNo nvarchar(50) NOT NULL DEFAULT(0)"; executequrypatch(qry);


			qry = "ALTER table LeadDocuments add DoucumentType nvarchar(50) null"; executequrypatch(qry);
			qry = "ALTER table LeadDocuments add Expiry date null"; executequrypatch(qry);
			qry = "ALTER table LeadDocuments add Notes nvarchar(200) null"; executequrypatch(qry);


			qry = "ALTER TABLE CustomerDocuments ADD DocumentTypeID bigint  NULL"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[AssignedTeams](
				[AssignedTeamId][bigint] IDENTITY(1, 1) NOT NULL,
	[CustomerID] [bigint] NOT NULL,
	[TeamID] [bigint] NOT NULL,
 CONSTRAINT[PK_dbo.AssignedTeamId] PRIMARY KEY CLUSTERED
(

   [AssignedTeamId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[LeadProcessFlows](
				[LeadProcessFlowId][bigint] IDENTITY(1, 1) NOT NULL,
	[TaskStatus] [bigint] NOT NULL,
	[RemoveUpdateUser] [bit] NOT NULL,
	[RemoveUpdateUserTeams] [bit] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[Branch] [bigint] NOT NULL,
	[CreatedBranch_BranchID] [bigint] NULL,
 CONSTRAINT[PK_dbo.LeadProcessFlows] PRIMARY KEY CLUSTERED
(

   [LeadProcessFlowId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[DocumentTypes](

			   [ID][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [Name] [nvarchar](max)NULL,
	[Section] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.DocumentTypes] PRIMARY KEY CLUSTERED
(

   [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);


			qry = "UPDATE CustomerDocuments SET DocumentTypeID = 0 WHERE DocumentTypeID is null"; executequrypatch(qry);

			qry = "ALTER TABLE CustomerDocuments ALTER COLUMN DocumentTypeID BIGINT NOT NULL"; executequrypatch(qry);

			qry = "ALTER TABLE CustomerDocuments ADD DEFAULT 0 FOR DocumentTypeID"; executequrypatch(qry);
qry = "ALTER TABLE Contacts ADD CountryID int not null default(0)"; executequrypatch(qry);


			qry = "ALTER TABLE CustomerDocuments ALTER COLUMN DoucumentType NVARCHAR(50) NULL"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[LeadRemarkChecklists](
				[Id][bigint] IDENTITY(1, 1) NOT NULL,
	[Checklistitemid] [bigint] NOT NULL,
	[Note] [nvarchar](300) NULL,
	[Check] [bit] NOT NULL,
	[Remark] [bigint] NOT NULL,
 CONSTRAINT[PK_dbo.LeadRemarkChecklist] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[LeadTaskUpdations](
				[TaskUpdationID][bigint] IDENTITY(1, 1) NOT NULL,
	[TaskId] [bigint] NOT NULL,
	[CreatedBy] [nvarchar](max)NOT NULL,
	[CreatedDate] [datetime] NULL,
	[Location] [nvarchar](max)NULL,
	[Lattitude] [nvarchar](100) NULL,
	[Longitude] [nvarchar](100) NULL,
	[Remarks] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.LeadTaskUpdations] PRIMARY KEY CLUSTERED
(

   [TaskUpdationID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[LeadTaskRemarks](
				[TaskRemarkId][bigint] IDENTITY(1, 1) NOT NULL,
	[TaskId] [bigint] NOT NULL,
	[AddedUser] [nvarchar](max)NULL,
	[Remark] [nvarchar](max)NULL,
	[Level] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[TaskStatusID] [bigint] NULL,
	[TaskUpdationID] [bigint] NULL,
 CONSTRAINT[PK_dbo.LeadTaskRemarks] PRIMARY KEY CLUSTERED
(

   [TaskRemarkId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[LeadTaskImages](
				[TaskImageId][bigint] IDENTITY(1, 1) NOT NULL,
	[TaskId] [bigint] NOT NULL,
	[TaskUpdationID] [bigint] NULL,
	[FileName] [nvarchar](max)NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[TaskRemarkId] [bigint] NULL,
	[CreatedBy] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.LeadTaskImages] PRIMARY KEY CLUSTERED
(

   [TaskImageId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);








			qry = "ALTER TABLE[dbo].[Contacts] ADD ContactTypeID BIGINT NULL"; executequrypatch(qry);


			qry = "ALTER TABLE LeadDocuments ADD DocumentTypeID BIGINT NOT NULL DEFAULT(0)"; executequrypatch(qry);



			qry = "ALTER TABLE AssignedToes ADD AssignBy nvarchar(200) null"; executequrypatch(qry);
			qry = "ALTER TABLE AssignedToes ADD[Status] nvarchar(50) null"; executequrypatch(qry);
			qry = "ALTER TABLE AssignedToes ADD CreatedDate DATETIME  null"; executequrypatch(qry);
			qry = "ALTER TABLE AssignedToes ADD ChkStatus int not null default(0)"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[LeadApprovedEmployees](

			   [ID][bigint] IDENTITY(1, 1) NOT NULL,

   [LeadID] [bigint] NOT NULL,

   [EmployeeID] [bigint] NOT NULL,

   [CreatedUser] [nvarchar](100) NOT NULL,

   [CreatedDate] [datetime] NOT NULL,

   [Status] [nvarchar](50) NOT NULL,
CONSTRAINT[PK_LeadApprovedEmployees] PRIMARY KEY CLUSTERED
(

  [ID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[AssetTransferDetails](
				[AssetItemEntryId][bigint] IDENTITY(1, 1) NOT NULL,
	[AssetEntryId] [bigint] NOT NULL,
	[AssetName] [nvarchar](50) NOT NULL,
	[Barcode] [nvarchar](50) NULL,
	[UnitId] [bigint] NOT NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[TotalPrice] [decimal](18, 2) NOT NULL,
	[DepreciationPercentage] [bigint] NOT NULL,
	[AssetAccountId] [bigint] NOT NULL,
	[DepreciationAccountId] [bigint] NOT NULL,
	[RefItemId] [bigint] NULL,
	[DeleteYN] [nvarchar](10) NULL,
 CONSTRAINT[PK_AssetTransferDetails] PRIMARY KEY CLUSTERED
(

   [AssetItemEntryId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = "alter table AssetTransferDetails alter column assetname varchar(1000) not null";
			executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[AssetTransferMasters](
				[AssetEntryId][bigint] IDENTITY(1, 1) NOT NULL,
	[InvoiceNo] [bigint] NOT NULL,
	[PurchaseEntry] [bigint] NULL,
	[AssetEntryDate] [datetime] NOT NULL,
	[VendorName] [nvarchar](50) NULL,
	[Vat] [bigint] NULL,
	[TotalAssetValue] [bigint] NOT NULL,
 CONSTRAINT[PK_AssetTransferMasters] PRIMARY KEY CLUSTERED
(

   [AssetEntryId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);








			qry = "ALTER TABLE Contacts ADD ContactCode nvarchar(MAX)  NULL"; executequrypatch(qry);
			qry = "ALTER TABLE Contacts ADD FirstName nvarchar(50)  NULL"; executequrypatch(qry);
			qry = "ALTER TABLE Contacts ADD LastName nvarchar(50)  NULL"; executequrypatch(qry);
			qry = "ALTER TABLE Contacts ADD ContactGroupID bigint NULL"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[CustomerDocuments](

			   [DocumnetId][bigint] IDENTITY(1, 1) NOT NULL,

   [CutomerID] [bigint] NOT NULL,

   [DoucumentType] [nvarchar](50) NOT NULL,

   [Expiry] [date] NOT NULL,

   [Notes] [nvarchar](200) NULL,
	[FilePath] [nvarchar](100) NULL,
	[ContactId] [bigint] NULL,
 CONSTRAINT[PK_CustomerDocuments] PRIMARY KEY CLUSTERED
(

   [DocumnetId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = "ALTER TABLE Contacts ADD ContactTypeID bigint NULL"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[LeadChecklists](

			   [ChecklistId][bigint] IDENTITY(1, 1) NOT NULL,

   [Stage] [bigint] NOT NULL,
CONSTRAINT[PK_LeadChecklist] PRIMARY KEY CLUSTERED
(

  [ChecklistId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);



			qry = @"CREATE TABLE[dbo].[LeadChecklistItems](
				[Id][bigint] IDENTITY(1, 1) NOT NULL,
	[Checklist] [bigint] NOT NULL,
	[ListName] [nvarchar](300) NULL,
	[AddNote] [bit] NOT NULL,
 CONSTRAINT[PK_LeadChecklistItems] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[LeadRemarkChecklists](
				[Id][bigint] IDENTITY(1, 1) NOT NULL,
	[Checklistitemid] [bigint] NOT NULL,
	[Note] [nvarchar](300) NULL,
	[Check] [bit] NOT NULL,
	[Remark] [bigint] NOT NULL,
 CONSTRAINT[PK_leadRemarkChecklist] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = @"CREATE TABLE[dbo].[LeadApprovals](

			   [LeadApprovalId][bigint] IDENTITY(1, 1) NOT NULL,

   [LeadProcessFlowId] [bigint] NOT NULL,

   [LeadEmployeeId] [bigint] NOT NULL,

   [LeadTaskStatus] [bigint] NOT NULL,
CONSTRAINT[PK_LeadApprovals] PRIMARY KEY CLUSTERED
(

  [LeadApprovalId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);


			qry = "ALTER TABLE[dbo].[LeadApprovals]  WITH CHECK ADD CONSTRAINT[FK_LeadApprovals_TaskStatus] FOREIGN KEY([LeadTaskStatus]) REFERENCES[dbo].[TaskStatus]([TaskStatusId])"; executequrypatch(qry);

			qry = "ALTER TABLE[dbo].[LeadApprovals] CHECK CONSTRAINT[FK_LeadApprovals_TaskStatus]"; executequrypatch(qry);

			qry = "alter table customers add includepdc  bit not null default 1"; executequrypatch(qry);
			qry = "alter table items add  PricingStrategy bit not null default 0"; executequrypatch(qry);
			qry = "alter table items add  PricingStrategyType  int not null default 0"; executequrypatch(qry);
			qry = "alter table items add  PricingStrategyAmountType  int not null default 0"; executequrypatch(qry);
			qry = "alter table items add  PricingStrategyValue  decimal(18, 2) not null default 0"; executequrypatch(qry);
			qry = "alter table items alter column PricingStrategyValue  decimal(18, 2) null"; executequrypatch(qry);
			qry = "alter table items add  lockprice bit not null default 0"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'33be7bfb-8fbe-422c-85a8-f84df3a5b938', N'Copy Permissions', 234343435, N'Copy Permissions', N'/Users/copypermission', 1304, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'be867818-ccea-4c0d-8b6d-7887675f9bfb', N'Modules', 234343436, N'Modules', N'/modules/Index', 1069, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 101)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'a34a350f-fafd-46fd-b6d7-2ea9549b5db7', N'Pricing Strategey Report', 234343438, N'Pricing Strategey Report', N'/item/pricingstrategy/index', 1099, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'63fe8ed5-e341-4808-84d7-f400e867a67d', N'Purchase Price Comparison', 234343439, N'Purchase Price Comparison', N'/Purchasereport/purchasepricecomparison', 1145, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'022ac7f4-0add-4f1a-8b57-27df83d0dea7', N'Sales Price Comparison', 234343440, N'Sales Price Comparison', N'/salesreport/SalesPriceComparison', 1135, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'3c4ec7f7-d459-4dc4-b6be-60cffa10ba3a', N'Send Sms General', 234343441, N'Send Sms General', N'/Users/sendsms', 1304, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'bf7775a3-f078-4d8b-b6aa-b730220ab22d', N'Periodic Maintenance List', 234343440, N'Periodic Maintenance List', N'/AMCPeriodicMaintenance/index', 6073, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[TaskGroups](

			   [TaskGroupId][bigint] IDENTITY(1, 1) NOT NULL,

   [TaskStatusId] [bigint] NOT NULL,

   [TaskTypeId] [bigint] NOT NULL,

   [TaskTypeName] [varchar](50) NULL,
 CONSTRAINT[PK_taskgroupdid] PRIMARY KEY CLUSTERED
(

   [TaskGroupId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[SMSTemplates](

			   [SMSTemplateID][bigint] IDENTITY(1, 1) NOT NULL,

   [Head] [nvarchar](100) NOT NULL,

   [Subject] [nvarchar](150) NOT NULL,

   [SMSBody] [nvarchar](max)NOT NULL,
 CONSTRAINT[PK_dbo.SMSTemplates] PRIMARY KEY CLUSTERED
(

   [SMSTemplateID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = "alter table leadtaskupdations add  nexttime datetime null"; executequrypatch(qry);
			qry = "alter table customers add  EndTime datetime null"; executequrypatch(qry);
			qry = "alter table companies add smssenderid varchar(50)"; executequrypatch(qry);
qry = "alter table companies add username varchar(50)"; executequrypatch(qry);
qry = "alter table companies add password varchar(50)"; executequrypatch(qry);



			qry = "alter table customerremarks add  nexttime datetime not null default '2000-01-01'"; executequrypatch(qry);
			qry = "alter table customerremarks add  nextdate datetime not null default '2000-01-01'"; executequrypatch(qry);
			qry = "alter table MaterialRequisitions add RequestStatus nvarchar(max) null"; executequrypatch(qry);
			qry = "alter table PurchaseEntries add ReferenceNo nvarchar(max) null"; executequrypatch(qry);
			qry = "ALTER TABLE SalesEntries ADD SalesStatus int"; executequrypatch(qry);
			qry = "update SalesEntries set salesstatus = 1 where salesstatus is null"; executequrypatch(qry);

			qry = "alter table PurchaseEntries add  requestpayment bit not null default 0"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[WCBillSundries](

			   [WCBillSundryId][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [WorkCompletion] [bigint] NOT NULL,
		   
			   [BillSundry] [bigint] NOT NULL,
		   
			   [BsValue] [decimal](18, 2) NULL,
	[AmountType] [int] NOT NULL,
	[BsAmount] [decimal](18, 2) NULL,
	[BsType] [int] NOT NULL,
 CONSTRAINT[PK_WCBillSundries] PRIMARY KEY CLUSTERED
(

   [WCBillSundryId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[WCItems](

			   [WCItemsId][bigint] IDENTITY(1, 1) NOT NULL,

   [WorkCompletion] [bigint] NOT NULL,

   [Item] [bigint] NOT NULL,

   [ItemUnitPrice] [decimal](18, 2) NOT NULL,

   [ItemQuantity] [decimal](18, 2) NOT NULL,

   [ItemSubTotal] [decimal](18, 2) NOT NULL,

   [ItemTax] [decimal](18, 2) NOT NULL,

   [ItemTaxAmount] [decimal](18, 2) NOT NULL,

   [ItemTotalAmount] [decimal](18, 2) NOT NULL,

   [ItemDiscount] [decimal](18, 2) NOT NULL,

   [itemNote] [nvarchar](max)NULL,
	[ItemUnit] [bigint] NULL,
	[ItemId_ItemID] [bigint] NULL,
	[WCId_WorkCompletionId] [bigint] NULL,
	[Type] [bit] NULL,
 CONSTRAINT[PK_WCItems] PRIMARY KEY CLUSTERED
(

   [WCItemsId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
qry = @"CREATE TABLE[dbo].[WorkCompletions](

   [WorkCompletionId][bigint] IDENTITY(1, 1) NOT NULL,

   [BillNo] [nvarchar](max)NOT NULL,
	[WcCashier] [bigint] NULL,
	[Customer] [bigint] NOT NULL,
	[WCItems] [int] NOT NULL,
	[WCItemQuantity] [decimal](18, 2) NOT NULL,
	[WCDate] [datetime] NOT NULL,
	[WCSubTotal] [decimal](18, 2) NOT NULL,
	[WCTax] [decimal](18, 2) NOT NULL,
	[WCTaxAmount] [decimal](18, 2) NOT NULL,
	[WCDiscount] [decimal](18, 2) NOT NULL,
	[WCGrandTotal] [decimal](18, 2) NOT NULL,
	[WCNote] [nvarchar](max)NULL,
	[Ref1] [nvarchar](50) NULL,
	[Ref2] [nvarchar](50) NULL,
	[Ref3] [nvarchar](50) NULL,
	[Ref4] [nvarchar](50) NULL,
	[Ref5] [nvarchar](50) NULL,
 CONSTRAINT[PK_WorkCompletions] PRIMARY KEY CLUSTERED
(

   [WorkCompletionId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[WarrantyCertificates](

			   [WarrantyId][bigint] IDENTITY(1, 1) NOT NULL,

   [BillNo] [nvarchar](max)NOT NULL,
	[WCashier] [bigint] NULL,
	[Customer] [bigint] NOT NULL,
	[WItems] [int] NOT NULL,
	[WItemQuantity] [decimal](18, 2) NOT NULL,
	[WDate] [datetime] NOT NULL,
	[WSubTotal] [decimal](18, 2) NOT NULL,
	[WTax] [decimal](18, 2) NOT NULL,
	[WTaxAmount] [decimal](18, 2) NOT NULL,
	[WDiscount] [decimal](18, 2) NOT NULL,
	[WGrandTotal] [decimal](18, 2) NOT NULL,
	[WNote] [nvarchar](max)NULL,
	[Ref1] [nvarchar](50) NULL,
	[Ref2] [nvarchar](50) NULL,
	[Ref3] [nvarchar](50) NULL,
	[Ref4] [nvarchar](50) NULL,
	[Ref5] [nvarchar](50) NULL,
 CONSTRAINT[PK_WarrantyCertificates] PRIMARY KEY CLUSTERED
(

   [WarrantyId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[WItems](

			   [WItemsId][bigint] IDENTITY(1, 1) NOT NULL,

   [Warranty] [bigint] NOT NULL,

   [Item] [bigint] NOT NULL,

   [ItemUnitPrice] [decimal](18, 2) NOT NULL,

   [ItemQuantity] [decimal](18, 2) NOT NULL,

   [ItemSubTotal] [decimal](18, 2) NOT NULL,

   [ItemTax] [decimal](18, 2) NOT NULL,

   [ItemTaxAmount] [decimal](18, 2) NOT NULL,

   [ItemTotalAmount] [decimal](18, 2) NOT NULL,

   [ItemDiscount] [decimal](18, 2) NOT NULL,

   [itemNote] [nvarchar](max)NULL,
	[ItemUnit] [bigint] NULL,
	[ItemId_ItemID] [bigint] NULL,
	[WId_WarrantyId] [bigint] NULL,
	[Type] [bit] NULL,
 CONSTRAINT[PK_WItems] PRIMARY KEY CLUSTERED
(

   [WItemsId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[WBillSundries](

			   [WBillSundryId][bigint] IDENTITY(1, 1) NOT NULL,

   [Warranty] [bigint] NOT NULL,

   [BillSundry] [bigint] NOT NULL,

   [BsValue] [decimal](18, 2) NULL,
	[AmountType] [int] NOT NULL,
	[BsAmount] [decimal](18, 2) NULL,
	[BsType] [int] NOT NULL,
 CONSTRAINT[PK_WBillSundries] PRIMARY KEY CLUSTERED
(

   [WBillSundryId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[mcitemminstocks](
			[mcitemminstock][bigint] IDENTITY(1, 1) NOT NULL,

   [MCId] [bigint]  NOT NULL,

   [ItemId] [bigint]  NOT NULL,

   [minstock] [decimal](18, 2) not null,
 CONSTRAINT[PK_dbo.mcitemminstock] PRIMARY KEY CLUSTERED
(

   [mcitemminstock] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = "alter table[WItems] add WarrantyPeriod bigint null"; executequrypatch(qry);
			qry = "alter table WorkCompletions add InvoiceNo nvarchar(max) null"; executequrypatch(qry);
			qry = "alter table WarrantyCertificates add InvoiceNo nvarchar(max) null"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'ab1fa909-d672-4411-b040-3f250fc7d6fe', N'Dashboard configuration', 234343451, N'Dashboard configuration', N'/EnableSetting/AccountsDashboardConfig', 1065, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);

			qry = "update AppModules set link = '/Accounts/accountsdashboard' where ModulesID = 1061"; executequrypatch(qry);

			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'01371093-1c50-40bc-8c4f-8e9800abb628', N'Work Completion List', 234343445, N'List', N'/WorkCompletion/Index', 234343443, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 11)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'4ae7fa08-978e-4194-b1b1-b2c5bedd68bc', N'create work completions', 234343444, N'Create', N'/WorkCompletion/Create', 234343443, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'eee5d989-231b-45b3-b8d4-35b8c64f8376', N'Work Completion', 234343443, N'Work Completion', N'#', 1021, NULL, 0, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);


			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'5195728e-9dfd-4397-9e56-623e870f3f4c', N'Create Warranty certificate', 234343449, N'Create', N'/Warranty/Create', 234343448, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);

			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'954de218-589e-4710-82be-af36a9c7b720', N'Warranty certificate', 234343448, N'warranty certificate', N'#', 1021, NULL, 0, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'998659b9-878a-4cea-b028-af63ad4bdf22', N'Warranty certificate List', 234343450, N'List', N'/Warranty/Index', 234343448, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'edeacb4c-5322-4eb1-9616-8e16c5ee0595', N'MC Items Minium Stock Set', 234343452, N'MC Items Minium Stock Set', N'/Item/MCItemMinimumStock', 1097, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "update AppModules set Link = '#' where name = 'DailyAttendance'"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'fa442e57-7627-4209-9b6c-715dcdd9e98c', N'Attendance Report', 234343437, N'Attendance Report', N'/hr/AttendanceReport/AttendanceSheet', 646, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = "INSERT[dbo].[AppModules]([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'f51d63aa-b576-429f-8a8b-82cf408662e4', N'Sales Return Profit', 234343443, N'Sales Return Profit', N'/SalesReturnReport/salesprofit', 559, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 1)"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[servicereportmembers](

			   [servicereportmemberid][bigint] IDENTITY(1, 1) NOT NULL,

   [servicereportid] [bigint] NOT NULL,

   [employeeid] [bigint] NOT NULL,
CONSTRAINT[PK_servicereportmembers] PRIMARY KEY CLUSTERED
(

  [servicereportmemberid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[servicereports](

			   [servicereportid][bigint] IDENTITY(1, 1) NOT NULL,

   [protaskid] [bigint] NOT NULL,

   [starttime] [datetime] NULL,
	[endtime] [datetime] NULL,
	[jobstatusid] [bigint] NOT NULL,
	[remark] [nchar](500) NULL,
	[jobtypes] [int] NOT NULL,
	[paytype] [int] NOT NULL,
	[amount] [decimal](18, 2) NULL,
	[chequenumber] [varchar](50) NULL,
	[bankname] [varchar](100) NULL,
 CONSTRAINT[PK_servicereport] PRIMARY KEY CLUSTERED
(

   [servicereportid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);
			qry = "alter table servicereports add  createdby nvarchar(200)"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[DummyStkTrsItem2](

			   [Id][bigint] IDENTITY(1, 1) NOT NULL,

   [Item] [bigint] NOT NULL,

   [Unit] [bigint] NULL,
	[Quantity] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[StockTransferId] [bigint] NOT NULL,
 CONSTRAINT[PK_DummyStkTrsItem2] PRIMARY KEY CLUSTERED
(

   [Id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[DummyPEItem2](

			   [DummyPEItemId][bigint] IDENTITY(1, 1) NOT NULL,

   [PurchaseEntry] [bigint] NOT NULL,

   [Item] [bigint] NOT NULL,

   [ItemUnitPrice] [decimal](18, 2) NOT NULL,

   [ItemQuantity] [decimal](18, 2) NOT NULL,

   [ItemSubTotal] [decimal](18, 2) NOT NULL,

   [ItemTax] [decimal](18, 2) NOT NULL,

   [ItemTaxAmount] [decimal](18, 2) NOT NULL,

   [ItemTotalAmount] [decimal](18, 2) NOT NULL,

   [itemNote] [nvarchar](max)NULL,
	[ItemDiscount] [decimal](18, 2) NOT NULL,
	[ItemUnit] [bigint] NULL,
	[ProjectId] [bigint] NULL,
	[TaskId] [bigint] NULL,
 CONSTRAINT[PK_dbo.DummyPEItems2] PRIMARY KEY CLUSTERED
(

   [DummyPEItemId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);



			qry = "alter table DummyPEItem2 alter column itemunitprice decimal(18, 4)"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[DummyPayments](

			   [PaymentId][bigint] IDENTITY(1, 1) NOT NULL,

   [Voucher] [bigint] NOT NULL,

   [VoucherNo] [nvarchar](max)NULL,
	[Date] [datetime] NOT NULL,
	[MOPayment] [int] NOT NULL,
	[PDCDate] [datetime] NULL,
	[PayFrom] [bigint] NOT NULL,
	[PayTo] [bigint] NOT NULL,
	[Category] [nvarchar](max)NULL,
	[SubTotal] [decimal](18, 2) NOT NULL,
	[Tax] [bigint] NULL,
	[TaxAmount] [decimal](18, 2) NOT NULL,
	[GrandTotal] [decimal](18, 2) NOT NULL,
	[Discount] [decimal](18, 2) NOT NULL,
	[Paying] [decimal](18, 2) NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[Remark] [nvarchar](max)NULL,
	[TaxPer] [decimal](18, 2) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max)NULL,
	[Branch] [bigint] NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedBranch_BranchID] [bigint] NULL,
	[editable] [int] NOT NULL,
	[Reference] [bigint] NULL,
	[RefType] [nvarchar](max)NULL,
	[Project] [bigint] NULL,
	[ProTask] [bigint] NULL,
	[Ref1] [nvarchar](50) NULL,
	[Ref2] [nvarchar](50) NULL,
	[Ref3] [nvarchar](50) NULL,
	[Ref4] [nvarchar](50) NULL,
	[Ref5] [nvarchar](50) NULL,
	[InvoiceNo] [nvarchar](max)NULL,
	[PaymentStatus] [bigint] NULL,
	[OverrideStatus] [nvarchar](10) NULL,
	[CheckNo] [nvarchar](max)NULL,
	[Bank] [nvarchar](max)NULL,
	[PDCNote] [nvarchar](max)NULL,
 CONSTRAINT[PK_DummyPayments] PRIMARY KEY CLUSTERED
(

   [PaymentId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[DummyPayBills](
			   [PaymentBillId][bigint] IDENTITY(1, 1) NOT NULL,
	[Payment] [bigint] NOT NULL,
	[InvoiceNo] [bigint] NULL,
	[BillType] [nvarchar](20) NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Type] [nvarchar](20) NULL,
	[NewRefName] [nvarchar](100) NULL,
	[Status] [int] NOT NULL,
 CONSTRAINT[PK_DummyPayBills] PRIMARY KEY CLUSTERED
(

   [PaymentBillId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = "alter table DummyPayments add stat int"; executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[additionaltaks](

			   [additionaltaskiad][bigint] IDENTITY(1, 1) NOT NULL,
		   
			   [salesentryid] [bigint] NOT NULL,
		   
			   [taskid] [bigint] NOT NULL,
			CONSTRAINT[PK_additionaltasks] PRIMARY KEY CLUSTERED
		  (
		  
			  [additionaltaskiad] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[SuggestItems](
				[suggestid][bigint] IDENTITY(1, 1) NOT NULL,
	[priitemid] [bigint] NOT NULL,
	[sugitemid] [bigint] NOT NULL,
 CONSTRAINT[PK_SuggestItem] PRIMARY KEY CLUSTERED
(

   [suggestid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY]"; executequrypatch(qry);

			qry = @"CREATE TABLE[dbo].[ApprovalUpdatestwps](
				[ApprovalUpdateID][bigint] IDENTITY(1, 1) NOT NULL,
	[TransEntry] [bigint] NOT NULL,
	[Type] [nvarchar](50) NULL,
	[ApprovalStatus] [int] NOT NULL,
	[RequestBy] [nvarchar](max)NULL,
	[ApprovedBy] [nvarchar](max)NULL,
	[CreatedDate] [datetime] NOT NULL,
	[Status] [int] NOT NULL,
	[Note] [nvarchar](max)NULL,
 CONSTRAINT[PK_dbo.ApprovalUpdatestwo] PRIMARY KEY CLUSTERED
(

   [ApprovalUpdateID] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]"; executequrypatch(qry);

			qry = "INSERT[dbo].[AppModules] ([Id], [Name], [ModulesID], [viewName], [Link], [Parent], [Description], [IsParent], [Employee], [Status], [Editable], [Discriminator], [iconClass], [addMenu], [MenuOrder]) VALUES(N'8805e29f-9f48-4fc7-95a5-d723e8d51c63', N'Override Selling Price Below Cost', 234343462, N'Override Selling Price Below Cost', N'#', 1100, NULL, 1, NULL, 0, 0, N'AppModules', N'fa-circle-o', 0, 10)"; executequrypatch(qry);
			qry = @"alter table UserEditDays add srdays int not null DEFAULT(0)
,pedays int not null DEFAULT(0)
,prdays int not null DEFAULT(0)
,stkdays int not null DEFAULT(0)"; executequrypatch(qry);

			qry = "update UserEditDays set srdays = days,pedays = days,prdays = days,stkdays = days"; executequrypatch(qry);
			qry = "alter table itemtaskmasters alter column TaskName varchar(max) null"; executequrypatch(qry);
			qry = "ALTER TABLE MaterialRequisitions Add ReminderDate DateTime"; executequrypatch(qry);
			qry = "ALTER TABLE MaterialRequisitions ADD Customer bigint"; executequrypatch(qry);
			qry = "ALTER TABLE MaterialRequisitions Add SupplierId bigint"; executequrypatch(qry);

			qry = "ALTER TABLE MaterialRequisitionItems Add TargetPrice decimal(18, 2)"; executequrypatch(qry);
			qry = "ALTER TABLE DummyMaterialRequisitionItems Add TargetPrice decimal(18, 2)"; executequrypatch(qry);
			qry = "alter table Customers add TermsandCondition nvarchar(max) null";
			executequrypatch(qry);
			qry = "alter table peitems alter column ItemUnitPrice decimal(18,4) not null";
			executequrypatch(qry);
			qry = "alter table seitems alter column ItemUnitPrice decimal(18,4) not null";
			executequrypatch(qry);
			qry = "DROP PROCEDURE SP_allchildGroups";
			executequrypatch(qry);
			qry = "DROP PROCEDURE SP_BackUpAndRestore";
			executequrypatch(qry);

			qry = "alter table accounts add mc bigint null";
				executequrypatch(qry);
			qry = "alter table accounts add shared bigint null";
				executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[sharedaccounts](

   [accsharedid][bigint] IDENTITY(1, 1) NOT NULL,

   [accountid] [bigint] NOT NULL,

   [mcid] [bigint] NOT NULL,

   [percentage] [decimal](18, 2) NOT NULL,
CONSTRAINT[PK_sharedaccounts] PRIMARY KEY CLUSTERED
(

  [accsharedid] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY]";
			executequrypatch(qry);

			qry = @"CREATE TABLE [dbo].[ChequeStatus](
	[chequestatusid] [bigint] IDENTITY(1,1) NOT NULL,
	[ChequeStatusName] [varchar](100) NOT NULL,
 CONSTRAINT [PK_chequeStatuses] PRIMARY KEY CLUSTERED 
(
	[chequestatusid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]";
			executequrypatch(qry);


			qry = @"CREATE TYPE [dbo].[TableTypePOSOrderItems] AS TABLE(
    [ItemUnit] [int] NULL,
    [ItemUnitPrice] [decimal](18, 2) NOT NULL,
    [ItemQuantity] [decimal](18, 2) NOT NULL,
    [ItemSubTotal] [decimal](18, 2) NOT NULL,
    [ItemDiscount] [decimal](18, 2) NULL,
    [ItemTax] [decimal](18, 2) NOT NULL,
    [ItemTaxAmount] [decimal](18, 2) NOT NULL,
    [ItemTotalAmount] [decimal](18, 2) NOT NULL,
    [ItemNote] [varchar](max) NOT NULL,
	[Note] [varchar](max) NULL,
    [PrintCount] [int] NULL,
    [Prints] [int] NULL,
    [OrderId] [int] NOT NULL,
    [Item] [int] NOT NULL,
	[editable] [int] NOT NULL
)";
			executequrypatch(qry);
			qry = @"CREATE TABLE[dbo].[Stocks](

	   [StockId][bigint] IDENTITY(1, 1) NOT NULL,

	   [Item] [bigint] NOT NULL,

	   [Unit] [bigint] NULL,
        [stockIn] [decimal](18, 2) NOT NULL,

		[stockOut] [decimal](18, 2) NOT NULL,

		[Cost] [decimal](18, 2) NOT NULL,

		[StockValue] [decimal](18, 2) NOT NULL,

		[Purpose] [nvarchar](max)NULL,
        [reference] [bigint] NOT NULL,

		[MC] [bigint] NULL,
        [Date] [datetime] NULL,
        [Status] [int] NOT NULL,

		[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT[PK_dbo.Stocks] PRIMARY KEY CLUSTERED
(

	   [StockId] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
			executequrypatch(qry);


			qry = @"CREATE TYPE[dbo].[TableTypeSTItems] AS TABLE(
				   [Item][bigint] NULL,
				   [Unit][bigint] NOT NULL,
				   [stockIn][decimal](18, 2) NOT NULL,
		  
				  [stockOut] [decimal](18, 2) NOT NULL,
		  
				  [Cost] [decimal](18, 2) NOT NULL,
		  
				  [StockValue] [decimal](18, 2) NOT NULL,
		  
				  [Purpose] [nvarchar](max)NULL,
        [reference] [bigint] NULL,
        [MC] [bigint] NULL,
        [Date] [datetime] NULL,
        [Status] [int] NOT NULL,

		[CreatedDate] [datetime] NOT NULL
)";
			executequrypatch(qry);
			qry = "alter table Customers add TermsandCondition nvarchar(max) null";
			executequrypatch(qry);
			qry = @"alter table customers add bonuscheck bit
alter table customers add bonusbaseamount int
alter table customers add bonuspercentage decimal
alter table customers add bonusclimembility  decimal
update Customers set bonuscheck=0
update Customers set bonusbaseamount=0
alter table AccountMaps add notintaxinvoice bit default 0
update AccountMaps set notintaxinvoice=0

alter table SalesEntries  add customername varchar(50) null
alter table SalesEntries  add phonenumber varchar(50) null
alter table items add accountid bigint null
";
			executequrypatch(qry);
			#endregion

			//         #region spdelete


































			//         #endregion
		}
        public void execsp()
        {
			try
			{
				string text3 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_allchildGroups.txt"));
				var result3 = db.Database.ExecuteSqlRaw(text3);
			}
			catch
			{ }
			try
			{
				string text4 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_allparentGroups.txt"));
				var result4 = db.Database.ExecuteSqlRaw(text4);
			}
			catch
			{ }
			string text5 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_BackUpAndRestore.txt"));
			var result5 = db.Database.ExecuteSqlRaw(text5);

			string text6 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertDvItems.txt"));
			var result6 = db.Database.ExecuteSqlRaw(text6);

			string text7 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPEBillSundry.txt"));
			var result7 = db.Database.ExecuteSqlRaw(text7);

			string text8 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPEItems.txt"));
			var result8 = db.Database.ExecuteSqlRaw(text8);

			string text9 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPFBillSundry.txt"));
			var result9 = db.Database.ExecuteSqlRaw(text9);

			string text10 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPFItems.txt"));
			var result10 = db.Database.ExecuteSqlRaw(text10);

			string text11 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPRBillSundry.txt"));
			var result11 = db.Database.ExecuteSqlRaw(text11);

			string texttable12 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPRItems.txt"));
			var resulttable12 = db.Database.ExecuteSqlRaw(texttable12);


			string text13 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertQuotationItems.txt"));
			var result13 = db.Database.ExecuteSqlRaw(text13);

			string text14 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSEBillSundry.txt"));
			var result14 = db.Database.ExecuteSqlRaw(text14);

			string text15 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSEItems.txt"));
			var result15 = db.Database.ExecuteSqlRaw(text15);

			string text16 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSRBillSundry.txt"));
			var result16 = db.Database.ExecuteSqlRaw(text16);

			string text17 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSRItems.txt"));
			var result17 = db.Database.ExecuteSqlRaw(text17);

			string text18 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPOrderItems.txt"));
			var result18 = db.Database.ExecuteSqlRaw(text18);

			string text19 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSalesOrderItems.txt"));
			var result19 = db.Database.ExecuteSqlRaw(text19);

			string text20 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\balancesheet.txt"));
			var result20 = db.Database.ExecuteSqlRaw(text20);

			string text21 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\trialbalance.txt"));
			var result21 = db.Database.ExecuteSqlRaw(text21);

			string text22 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\balancesheet2.txt"));
			var result22 = db.Database.ExecuteSqlRaw(text22);

			string text23 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPLItems.txt"));
			var result23 = db.Database.ExecuteSqlRaw(text23);

			string text24 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPurchaseQuotItems.txt"));
			var result24 = db.Database.ExecuteSqlRaw(text24);

			string text25 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertMRNoteItems.txt"));
			var result25 = db.Database.ExecuteSqlRaw(text25);

			string text26 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertMRItems.txt"));
			var result26 = db.Database.ExecuteSqlRaw(text26);
			string text32 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPOSOrderItems.txt"));
			var result32 = db.Database.ExecuteSqlRaw(text32);



			string text27 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod.txt"));
			var result27 = db.Database.ExecuteSqlRaw(text27);
			string text277 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertDummyPeItems.txt"));
			var result277 = db.Database.ExecuteSqlRaw(text277);

			string text28 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertCrossHrItems.txt"));
			var result28 = db.Database.ExecuteSqlRaw(text28);

			string text29 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertHrItems.txt"));
			var result29 = db.Database.ExecuteSqlRaw(text29);

			string text30 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSTItems.txt"));
			var result30 = db.Database.ExecuteSqlRaw(text30);

			
			string text33 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod2.txt"));
			var result33 = db.Database.ExecuteSqlRaw(text33);
			string text333 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod3.txt"));
			var result333 = db.Database.ExecuteSqlRaw(text333);
			string text3333 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod4.txt"));
			var result3333 = db.Database.ExecuteSqlRaw(text3333);

			string text44 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod5.txt"));
			var result44 = db.Database.ExecuteSqlRaw(text44);

			string text55 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod6.txt"));
			var result55 = db.Database.ExecuteSqlRaw(text55);

			string text34 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_ITEMSEARCH.txt"));
			var result34 = db.Database.ExecuteSqlRaw(text34);
			string text35 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_ITEMSEARCH2.txt"));
			var result35 = db.Database.ExecuteSqlRaw(text35);

		}
	
		public ActionResult Index()
        {

       /*     string qry;
            var li=db.Database.SqlQueryRaw<logdata>(@"select logtime,logid  from (select max(LogManagerID) as logmanagerid,
max(logtime) as logtime, logid from LogManagers where LogTable = 'ProTasks' group by LogID) as q1").ToList();
            string storePath = LegacyWeb.MapPath("~/uploads/qry.txt");
            using (StreamWriter sw = new StreamWriter(storePath))
            {
     foreach (logdata ll in li)
            {
                qry= "update ProTasks set logtime='" + ll.logtime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' where ProTaskId='" + ll.logid + "';"; executequrypatch(qry);

                
               
                    sw.Write(qry);
               

            }

            }
       */
            return View();
        }

		public string repair()
        {
			SqlConnection con = new SqlConnection(db.Database.GetDbConnection().ConnectionString);
			con.Open();
			SqlCommand cmd = new SqlCommand("SELECT session_id FROM sys.dm_exec_requests CROSS APPLY sys.dm_exec_sql_text(sql_handle)", con);
			SqlDataReader sdr = cmd.ExecuteReader();
			var dataTable = new DataTable();
			dataTable.Load(sdr);


			return dataTable.ToString();
		}

		[HttpPost]
        public ActionResult executeqry(string qry)
        {
			if (qry.Contains("DIJKSTRA"))
			{
				qry = qry.Replace("DIJKSTRA", "");
                SqlConnection con = new SqlConnection(db.Database.GetDbConnection().ConnectionString);
                con.Open();
                SqlCommand cmd = new SqlCommand(qry, con);
				cmd.CommandTimeout = 60 * 60;
                SqlDataReader sdr = cmd.ExecuteReader();
                var dataTable = new DataTable();
                dataTable.Load(sdr);
              

				return View(dataTable);
			}
			else
            {
				return NotFound();
            }
           
        }
		[QkAuthorize(Roles = "Dev,All Sales Entry")]
		public bool executequrypatch(string qry)
		{
			
			using (SqlConnection con = new SqlConnection(db.Database.GetDbConnection().ConnectionString))
			{
				con.Open();
				SqlCommand cmd = new SqlCommand(qry, con);
				cmd.CommandTimeout = 30;
				try
				{

					cmd.ExecuteNonQuery();
					con.Close();
					return true;
				}
				catch (Exception e)
				{
					con.Close();
					return false;
				}
				con.Close();
				
			}
		}

    }
}
