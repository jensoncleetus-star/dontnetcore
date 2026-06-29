using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace QuickSoft.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Faithful-port shims: the legacy code news-up the context ad-hoc (parameterless or by connection).
        public ApplicationDbContext() : base(BuildOptions(LegacyWeb.ConnectionString)) { }
        public ApplicationDbContext(string Connection) : base(BuildOptions(LegacyWeb.ResolveConnection(Connection))) { }
        private static DbContextOptions<ApplicationDbContext> BuildOptions(string conn)
            // TranslateParameterizedCollectionsToConstants: EF Core 10 translates list.Contains(col) via
            // OPENJSON ("Incorrect syntax near '$'" on older SQL Server compat levels); inline as constants.
            => new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(conn, sql => sql.TranslateParameterizedCollectionsToConstants()).Options;

        public void SetCommandTimeOut(int Timeout)
        {
            this.Database.SetCommandTimeout(Timeout);
        }
        public virtual DbSet<RoleGroup> RoleGroups { get; set; }
        public DbSet<AppModules> AppModuless { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Company> companys { get; set; }
        public DbSet<Tax> Taxs { get; set; }
        public DbSet<passworddetailsas> passworddetailsass { get; set; }
        public DbSet<assigncommon> assigncommons { get; set; }
        public DbSet<TaxGroup> TaxGroups { get; set; }
        public DbSet<LogManager> LogManagers { get; set; }
        public DbSet<passworddetail> passworddetails { get; set; }
        public DbSet<filedocumentdetail> filedocumentdetails { get; set; }
        public DbSet<filedocumentdetailsa> filedocumentdetailsas { get; set; }
        #region General Tables

        #region Amc
        public DbSet<Amc> Amcs { get; set; }
        public DbSet<AmcContract> AmcContracts { get; set; }
        public DbSet<AmcContractType> AmcContractTypes { get; set; }
        public DbSet<AmcStatus> AmcStatuss { get; set; }
        public DbSet<AmcStatusDept> AmcStatusDepts { get; set; }
        public DbSet<AmcStatusDesg> AmcStatusDesgs { get; set; }
        public DbSet<AmcDocument> AmcDocuments { get; set; }
        public DbSet<AmcAssignedTo> AmcAssignedTos { get; set; }
        public DbSet<AmcAssignedTeam> AmcAssignedTeams { get; set; }
        public DbSet<AmcUpdation> AmcUpdations { get; set; }
        public DbSet<AmcRemark> AmcRemarks { get; set; }
        public DbSet<AmcProcessFlow> AmcProcessFlows { get; set; }
        public DbSet<AmcProcessFlowAssignUser> AmcProcessFlowAssignUsers { get; set; }
        public DbSet<AmcProcessFlowAssignType> AmcProcessFlowAssignTypes { get; set; }
        public DbSet<PeriodicMaintenance> PeriodicMaintenances { get; set; }
        public DbSet<PeriodicMaintenanceDetail> PeriodicMaintenanceDetails { get; set; }
        public DbSet<PeriodicMaintAssignedTeam> PeriodicMaintAssignedTeams { get; set; }
        public DbSet<PeriodicMaintAssignedTo> PeriodicMaintAssignedToes { get; set; }
        public DbSet<PeriodicProcessFlow> PeriodicProcessFlows { get; set; }
        public DbSet<PeriodicProcessFlowAssignType> PeriodicProcessFlowAssignTypes { get; set; }
        public DbSet<PeriodicProcessFlowAssignUser> PeriodicProcessFlowAssignUsers { get; set; }

        #endregion
        
       public DbSet<salesmanprofittarget> salesmanprofittargets { get; set; }
        public DbSet<AddedRemarks> AddedRemarks { get; set; }
  
        public DbSet<servicereport> servicereports { get; set; }
        public DbSet<servicereportmember> servicereportmembers { get; set; }
        public DbSet<SuggestItem> suggestItems { get; set; }
        public DbSet<additionaltaks> additionaltasks { get; set; }
        
        public DbSet<AssetTransferDetail> AssetTransferDetails { get; set; }
        public DbSet<AssetTransferMasters> AssetTransferMasters { get; set; }

        public DbSet<AssetToInventoryMasters> AssetToInventoryMasters { get; set; }
        public DbSet<AssetToInventoryDetail> AssetToInventoryDetails { get; set; }
        public DbSet<AssetAdjustments> AssetAdjustments { get; set; }

        public DbSet<Accounts> Accountss { get; set; }
        public DbSet<AccountsGroup> AccountsGroups { get; set; }
        public DbSet<AccountsTransaction> AccountsTransactions { get; set; }
        public DbSet<dummyAccountsTransactions> dummyAccountsTransactions { get; set; }
        public DbSet<otpapprove> otpapproves { get; set; }
        public DbSet<SuperUser> SuperUsers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactType> ContactTypes { get; set; }
        public DbSet<ContactGroup> ContactGroups { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Branch> Branchs { get; set; }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }


        public DbSet<ItemCategory> ItemCategorys { get; set; }
        public DbSet<ItemBrand> ItemBrands { get; set; }
        public DbSet<ItemUnit> ItemUnits { get; set; }
        public DbSet<ItemSize> ItemSizes { get; set; }
        public DbSet<ItemColor> ItemColors { get; set; }
        public DbSet<ItemDocument> ItemDocuments { get; set; }
        public DbSet<ItemTransactions> ItemTransactions { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<customermerge> customermerges { get; set; }
        public DbSet<customermergelog> customermergelogs { get; set; }

        public DbSet<SupplierItems> SupplierItems { get; set; }
        public DbSet<SupplierCategories> SupplierCategories { get; set; }
        public DbSet<SupplierBrands> SupplierBrands { get; set; }
        public DbSet<SalesEntry> SalesEntrys { get; set; }
        public DbSet<SEItems> SEItemss { get; set; }
        public DbSet<SEPayment> SEPayments { get; set; }
        public DbSet<SETransaction> SETransactions { get; set; }
        public DbSet<SEBillSundry> SEBillSundrys { get; set; }
        public DbSet<ChequeDetails> ChequeDetails { get; set; }

        public DbSet<ProForma> ProFormas { get; set; }
        public DbSet<PFItems> PFItemss { get; set; }
        public DbSet<PFBillSundry> PFBillSundrys { get; set; }


        public DbSet<PurchaseEntry> PurchaseEntrys { get; set; }
        public DbSet<PEItems> PEItemss { get; set; }
        public DbSet<PEPayment> PEPayments { get; set; }
        public DbSet<PETransaction> PETransactions { get; set; }
        public DbSet<PEBillSundry> PEBillSundrys { get; set; }

        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }

        
        public DbSet<QuotationType> QuotationTypes { get; set; }
        public DbSet<UserEditDays> UserEditDayss { get; set; }
        
        public DbSet<QtBillSundry> QtBillSundrys { get; set; }



        public DbSet<SalesReturn> SalesReturns { get; set; }
        public DbSet<SRItems> SRItemss { get; set; }
        public DbSet<SRNoteItems> SRNoteItemss { get; set; }
        public DbSet<PRItemNotes> PRItemNotes { get; set; }
        public DbSet<SRPayment> SRPayments { get; set; }
        public DbSet<SRTransaction> SRTransactions { get; set; }
        public DbSet<SRBillSundry> SRBillSundrys { get; set; }

        public DbSet<PurchaseReturn> PurchaseReturns { get; set; }
        public DbSet<PRItems> PRItemss { get; set; }
        public DbSet<PRPayment> PRPayments { get; set; }
        public DbSet<PRTransaction> PRTransactions { get; set; }
        public DbSet<PRBillSundry> PRBillSundrys { get; set; }

        public DbSet<Deliverynote> Deliverynotes { get; set; }
        public DbSet<DvItem> DvItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<DummyPayment> DummyPayments { get; set; }
        public DbSet<DummyPayBill> DummyPayBills { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<PDC> PDCs { get; set; }
        public DbSet<ReceiptBill> ReceiptBills { get; set; }
        public DbSet<PaymentBill> PaymentBills { get; set; }
        public DbSet<JornalBill> JornaltBills { get; set; }
        public DbSet<JornalPaymentBill> JornalPaymentBills { get; set; }
        public DbSet<BSNature> BSNatures { get; set; }
        public DbSet<BillSundry> BillSundrys { get; set; }

        public DbSet<Barcode> Barcodes { get; set; }

        public DbSet<StockAdjustment> StockAdjustments { get; set; }


        public DbSet<BillOfMaterial> BillOfMaterials { get; set; }
        public DbSet<BOMItem> BOMItems { get; set; }
        public DbSet<BillOfMaterialsoffer> BillOfMaterialsoffers { get; set; }
        public DbSet<BOMItemsoffer> BOMItemsoffers { get; set; }
        public DbSet<BillOfQty> BillOfQyts { get; set; }
        public DbSet<BoqItem> BoqItems { get; set; }

        public DbSet<Production> Productions { get; set; }
        public DbSet<ProItem> ProItems { get; set; }

        public DbSet<Unassemble> Unassembles { get; set; }
        public DbSet<UnassembleItem> UnassembleItems { get; set; }

        public DbSet<JobCard> JobCards { get; set; }
        public DbSet<JCItem> JCItems { get; set; }
        public DbSet<EnableSetting> EnableSettings { get; set; }
        public DbSet<JobCardItemSetting> JobCardItemSettings { get; set; }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        //public DbSet<EmpAttendance> EmpAttendances { get; set; }
        public DbSet<POBillSundry> POBillSundrys { get; set; }

        public DbSet<PurchaseQuotation> PurchaseQuotations { get; set; }
        public DbSet<PurchaseQuotationItem> PurchaseQuotationItems { get; set; }
        public DbSet<PQtBillSundry> PQtBillSundrys { get; set; }

        // general tables
        public DbSet<CodePrefix> CodePrefixs { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }

        public DbSet<CompanyHeader> CompanyHeaders { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<SMSTemplate> SMSTemplates { get; set; }
        public DbSet<TermsAndConditions> TermsAndConditionss { get; set; }

        public DbSet<Journal> Journals { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<MC> MCs { get; set; }

        public DbSet<AppVersion> AppVersions { get; set; }

        public DbSet<InvoiceLayout> InvoiceLayouts { get; set; }
        public DbSet<InvoiceField> InvoiceFields { get; set; }
        public DbSet<InvoiceTemplate> InvoiceTemplates { get; set; }   // BOS custom drag-drop invoice designer

        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<FileDocument> FileDocuments { get; set; }

        public DbSet<StickyLabel> StickyLabels { get; set; }
        public DbSet<StickyNote> StickyNotes { get; set; }

        public DbSet<PurchaseType> PurchaseTypes { get; set; }
        public DbSet<SalesType> SalesTypes { get; set; }

        public DbSet<ContraVoucher> ContraVouchers { get; set; }
        // public DbSet<Config> Configs { get; set; }

        public DbSet<CreditNote> CreditNotes { get; set; }
        public DbSet<CNInvoice> CNInvoices { get; set; }

        public DbSet<DrNote> DrNotes { get; set; }
        public DbSet<DrInvoice> DrInvoices { get; set; }

        public DbSet<MtToParty> MtToPartys { get; set; }
        public DbSet<MtToPartyItems> MtToPartyItemss { get; set; }
        public DbSet<MtToPartyBSundry> MtToPartyBSundrys { get; set; }

        public DbSet<MtFromParty> MtFromPartys { get; set; }
        public DbSet<MtFromPartyItems> MtFromPartyItemss { get; set; }
        public DbSet<MtFromPartyBSundry> MtFromPartyBSundrys { get; set; }

        public DbSet<ConvertTransactions> ConvertTransactionss { get; set; }

        public DbSet<StockJournal> StockJournals { get; set; }
        public DbSet<SJItemGenerate> SJItemGenerates { get; set; }
        public DbSet<SJItemConsume> SJItemConsumes { get; set; }

        public DbSet<CurrencyMaster> CurrencyMasters { get; set; }

        public DbSet<PrefixMaster> PrefixMasters { get; set; }

        public DbSet<Jewellery> Jewellerys { get; set; }
        public DbSet<Watch> Watchs { get; set; }
        public DbSet<Diamond> Diamonds { get; set; }
        public DbSet<keytableview> keytableviews { get; set; }
        
        public DbSet<StockVerification> StockVerifications { get; set; }
        public DbSet<SVItems> SVItemss { get; set; }

        public DbSet<ItemPrefix> ItemPrefixs { get; set; }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectType> ProjectTypes { get; set; }
        public DbSet<ProjectImage> ProjectImages { get; set; }
        public DbSet<ProjectStatus> ProjectStatus { get; set; }
        public DbSet<TaskRemark> TaskRemarks { get; set; }
        public DbSet<LeadTaskRemark> LeadTaskRemarks { get; set; }
        public DbSet<CustomerRemark> CustomerRemarks { get; set; }
        public DbSet<RemarkCheque> RemarkCheque { get; set; }
        public DbSet<RemarkCustomer> RemarkCustomers { get; set; }
        public DbSet<ItemRemark> ItemRemarks { get; set; }
        public DbSet<ProTask> ProTasks { get; set; }
       // public DbSet<ProTasknontech> ProTasknontechs { get; set; }
        public DbSet<ProTaskType> ProTaskTypes { get; set; }
        public DbSet<ProTaskManner> ProTaskManners { get; set; }
        public DbSet<ProResignRequest> ProResignRequests { get; set; }
        public DbSet<ProLeaveRequest> ProLeaveRequests { get; set; }
        public DbSet<TaskAssigned> TaskAssigneds { get; set; }
        //public DbSet<LeadTaskAssigned> LeadTaskAssigneds { get; set; }
        // public DbSet<TaskTeam> TaskTeams { get; set; }
        // public DbSet<TaskTeamMember> TaskTeamMembers { get; set; }
        public DbSet<ProTaskUpdation> ProTaskUpdations { get; set; }
        public DbSet<CustomerSatisfaction> CustomerSatisfactions { get; set; }
        public DbSet<LeadTaskUpdation> LeadTaskUpdations { get; set; }
        public DbSet<TaskImage> TaskImages { get; set; }


        public DbSet<ItemTaskMaster> ItemTaskMasters { get; set; }
        public DbSet<ItemTasks> ItemTask { get; set; }
        public DbSet<LeadTaskImage> LeadTaskImages { get; set; }
        public DbSet<TaskStatus> TaskStatus { get; set; }



        //public DbSet<DrNote> DrNotes { get; set; }
        //public DbSet<DrInvoice> DrInvoices { get; set; }

        public DbSet<Stock> Stocks { get; set; }

        public DbSet<ItemBundle> ItemBundles { get; set; }
        public DbSet<BundleItem> BundleItems { get; set; }


        public DbSet<FinancialYear> FinancialYears { get; set; }

        public DbSet<StockTransfer> StockTransfers { get; set; }
        public DbSet<StockTransferItem> StockTransferItems { get; set; }
        public DbSet<StockTransferBSundry> StockTransferBSundrys { get; set; }

        //stock
        public DbSet<BatchStock> BatchStocks { get; set; }
        public DbSet<shelfstockmovement> shelfstockmovements { get; set; }

        public DbSet<Scaffold> Scaffolds { get; set; }

        public DbSet<HireRate> HireRates { get; set; }
        public DbSet<HireType> HireTypes { get; set; }
        public DbSet<HireDetail> HireDetails { get; set; }

        //public DbSet<ReturnNote> ReturnNotes { get; set; }
        //public DbSet<RtItem> RtItems { get; set; }
        ////Hire return
        public DbSet<HireReturn> HireReturns { get; set; }
        public DbSet<HrItem> HrItems { get; set; }

        public DbSet<PackingList> PackingLists { get; set; }
        public DbSet<PLItem> PLItems { get; set; }

        public DbSet<ConsumedItems> ConsumedItem { get; set; }
        public DbSet<GeneratedItems> GeneratedItem { get; set; }
        public DbSet<MaterialRequisition> MaterialRequisitions { get; set; }
        public DbSet<MaterialRequisitionItem> MaterialRequisitionItems { get; set; }
        public DbSet<routemap> routemap { get; set; }
        public DbSet<LocationName> LocationNames { get; set; }
        public DbSet<ChequeBook> ChequeBooks { get; set; }
        public DbSet<chequetransaction> chequetransactions { get; set; }
        //public DbSet<ReferenceAccount> ReferenceAccounts { get; set; }

        public DbSet<MaterialReceiveNote> MaterialReceiveNotes { get; set; }
        public DbSet<MRNoteItem> MRNoteItems { get; set; }
        public DbSet<MRNotePOrder> MRNotePOrders { get; set; }

        public DbSet<Approval> Approvals { get; set; }
        public DbSet<ApprovalUpdate> ApprovalUpdates { get; set; }
        public DbSet<ApprovalUpdatestwp> ApprovalUpdatestwp { get; set; }
        public DbSet<ChequePrinting> ChequePrintings { get; set; }
        public DbSet<ChequeDesign> ChequeDesigns { get; set; }

        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Reminderss> Reminderss { get; set; }
        public DbSet<ReminderAssigned> ReminderAssigneds { get; set; }
        public DbSet<snooze> Snoozees { get; set; }
        public DbSet<ReminderAssignedss> ReminderAssignedss { get; set; }
        public DbSet<DocExpiryReminder> DocExpiryReminders { get; set; }


        
        // Lead 
        public DbSet<SourceOfLead> SourceOfLeads { get; set; }
        public DbSet<CustomerConversion> CustomerConversions { get; set; }
        public DbSet<AssignedTo> AssignedTos { get; set; }
        public DbSet<AssignedTeams> AssignedTeams { get; set; }
        public DbSet<LeadStatus> LeadStatuss { get; set; }
        public DbSet<LeadDocument> LeadDocuments { get; set; }
        public DbSet<LeadRemark> LeadRemarks { get; set; }
        public DbSet<LeadRejection> LeadRejections { get; set; }
        public DbSet<LeadConditions> LeadCondition { get; set; }
        public DbSet<LeadType> LeadTypes { get; set; }
        public DbSet<PriceCategory> PriceCategories { get; set; }
        public DbSet<ParticularParty> ParticularParties { get; set; }
        public DbSet<Rack> Racks { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<rackmaterialcentre> rackmaterialcentres { get; set; }
        public DbSet<ShelfStockTransfer> ShelfStockTransfers { get; set; }
        public DbSet<SSTItem> SSTItems { get; set; }
        public DbSet<vehicleupdation> vehicleupdations { get; set; }
        public DbSet<vehiclereminder> vehiclereminder { get; set; }
        public DbSet<vehiclemaster> vehiclemasters { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<VehicleManufacturer> VehicleManufacturers { get; set; }
        public DbSet<VehicleModel> VehicleModels { get; set; }
        public DbSet<CustomerTyp> CustomerTyps { get; set; }
        public DbSet<WorkCompletion> WorkCompletions { get; set; }
        public DbSet<WCItems> WCItems { get; set; }
        public DbSet<WCBillSundry> WCBillSundries { get; set; }
        public DbSet<WarrantyCertificate> WarrantyCertificates { get; set; }

        public DbSet<WItems> WItems { get; set; }


        public DbSet<WarrantyEntries> WarrantyEntries { get; set; }

        public DbSet<WEItems> WEItems { get; set; }


        public DbSet<WBillSundry> WBillSundries { get; set; }
        public DbSet<mcitemminstocks> mcitemminstock { get; set; }
        public DbSet<PriceCategoryMaster> PriceCategoryMasters { get; set; }

        public DbSet<PriceCategoryPercentage> PriceCategoryPercentages { get; set; }

        public DbSet<LeadLevel> LeadLevels { get; set; }
        public DbSet<ChequeStatus> chequeStatuses { get; set; }
        public DbSet<AssignedToLog> AssignedToLogs { get; set; }

        // Field Mapping
        public DbSet<FieldMapping> FieldMappings { get; set; }
        public DbSet<FieldMappingLock> FieldMappingsLocks { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<TeamTaskStatus> TeamTaskStatus { get; set; }
        public DbSet<LeadTaskStatus> LeadTaskStatus { get; set; }
        public DbSet<TeamAmcStatus> TeamAmcStatus { get; set; }
        ////Cross Hire return
        public DbSet<CrossHireReturn> CrossHireReturns { get; set; }
        public DbSet<CrossHrItem> CrossHrItems { get; set; }

        public DbSet<DummySEItem> DummySEItems { get; set; }
        public DbSet<DummySRItem> DummySRItems { get; set; }
        public DbSet<DummyQuotItem> DummyQuotItems { get; set; }
        public DbSet<DummyPFItem> DummyPFItems { get; set; }
        public DbSet<DummyDvItem> DummyDvItems { get; set; }
        public DbSet<DummyPEItem> DummyPEItems { get; set; }
        public DbSet<DummyPEItem2> DummyPEItems2 { get; set; }
        public DbSet<DummyPRItem> DummyPRItems { get; set; }
        public DbSet<DummyCrossHrItem> DummyCrossHrItems { get; set; }
        public DbSet<DummyPOrderItem> DummyPOrderItems { get; set; }
        public DbSet<DummySOrderItem> DummySOrderItems { get; set; }
        public DbSet<DummyStkTrsItem> DummyStkTrsItems { get; set; }
        public DbSet<DummyStkTrsItem2> DummyStkTrsItem2 { get; set; }
        public DbSet<DummyPQuotationItem> DummyPQuotationItems { get; set; }
        public DbSet<DummyMRNoteItem> DummyMRNoteItems { get; set; }
        public DbSet<DummyMaterialRequisitionItem> DummyMaterialRequisitionItems { get; set; }

        public DbSet<DummyReceiptBill> DummyReceiptBills { get; set; }
        public DbSet<DummyPaymentBill> DummyPaymentBills { get; set; }
        public DbSet<DummyJornalBill> DummyJornalBills { get; set; }
        public DbSet<TaskStatusDept> TaskStatusDepts { get; set; }
        public DbSet<TaskStatusDesg> TaskStatusDesgs { get; set; }

        public DbSet<ProcessFlow> ProcessFlows { get; set; }
        public DbSet<LeadProcessFlow> LeadProcessFlows { get; set; }

        public DbSet<ProcessFlowAssignUser> ProcessFlowAssignUsers { get; set; }
        public DbSet<ProcessFlowAssignUserstolead> ProcessFlowAssignUserstolead { get; set; }

        public DbSet<LeadProcessFlowAssignUser> LeadProcessFlowAssignUsers { get; set; }
        public DbSet<AssignTaskManner> AssignTaskManners { get; set; }
        public DbSet<AssignTaskSupervisor> AssignTaskSupervisors { get; set; }
        public DbSet<TaskAssignType> TaskAssignTypes { get; set; }
        public DbSet<ProcessFlowAssignType> ProcessFlowAssignTypes { get; set; }
        public DbSet<LeadProcessFlowAssignType> LeadProcessFlowAssignTypes { get; set; }


        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<ScopeOfWork> ScopeOfWorks { get; set; }
        public DbSet<ScopeOfWorkItem> ScopeOfWorkItems { get; set; }

        public DbSet<ScopeOfWorksData> ScopeOfWorksDatas { get; set; }
        public DbSet<RemarkChecklist> RemarkChecklists { get; set; }
        public DbSet<LeadRemarkChecklist> LeadRemarkChecklists { get; set; }
      
        //  public DbSet<LeadStatusChecklistItems> LeadStatusChecklistItems { get; set; }

        public DbSet<LeadChecklists> LeadChecklists { get; set; }
        public DbSet<LeadChecklistItems> LeadChecklistItems { get; set; }
   
        public DbSet<ScopeOfWorkRemarkChecklist> ScopeOfWorkRemarkChecklists { get; set; }
        public DbSet<Mobile> Mobiles { get; set; }
        public DbSet<TaskMobile> TaskMobiles { get; set; }
        public DbSet<TaskDocument> TaskDocuments { get; set; }
        

        public DbSet<WorkShift> WorkShifts { get; set; }
        public DbSet<PayrollUnit> payrollunits { get; set; }
        public DbSet<AttendanceType> AttendanceTypes { get; set; }
        public DbSet<EmployeeGrade> EmployeeGrades { get; set; }
        public DbSet<EmployeeEducation> EmployeeEducations { get; set; }
        public DbSet<EmployeeProfession> EmployeeProfessions { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        public DbSet<EmployeePersonal> EmployeePersonals { get; set; }
        public DbSet<EmployeeBank> EmployeeBanks { get; set; }
        public DbSet<EmployeeWorkDetail> EmployeeWorkDetails { get; set; }
        public DbSet<Payhead> Payheads { get; set; }
        public DbSet<SpecifiedFormula> SpecifiedFormulas { get; set; }
        public DbSet<Computeinfo> Computeinfos { get; set; }
        public DbSet<GratuityDetails> GratuityDetailss { get; set; }
        public DbSet<EmployeeAttendanceSummary> EmployeeAttendanceSummarys { get; set; }
        #endregion
        public DbSet<itemsizeprice> itemsizeprice { get; set; }
        public DbSet<AttachmentDocuments> AttachmentDocuments { get; set; }
        public DbSet<FilemultipleDocuments> MultipleDocuments { get; set; }
        public DbSet<googlereview> googlereviews { get; set; }
        public DbSet<customerleadrelation> customerleadrelation { get; set; }
        public DbSet<customerbonus> customerbonus { get; set; }
        
        public DbSet<leadcustomerrelation> leadcustomerrelation { get; set; }
        public DbSet<accountmap> accountmaps { get; set; }
        public DbSet<geowall> geowalls { get; set; }
        
        public DbSet<handover> handover { get; set; }
        public DbSet<cashnotes> cashnotes { get; set; }
        public DbSet<EmpAttendance> EmpAttendances { get; set; }
        public DbSet<EmpAttDetails> EmpAttDetails { get; set; }

        public DbSet<quotationdocument> quotationdocuments { get; set; }
        public DbSet<purchaseentrydocument> purchaseentrydocuments { get; set; }
        public DbSet<salesorderdocument> salesorderdocuments { get; set; }
        public DbSet<commission> commissions { get; set; }
        public DbSet<ItemSerialNumber> ItemSerialNo { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendanceDetail> AttendanceDetails { get; set; }
        public DbSet<PayrollVoucher> PayrollVouchers { get; set; }
        public DbSet<PayrollVoucherEmployee> PayrollVoucherEmployees { get; set; }
        public DbSet<PayrollVoucherSalary> PayrollVoucherSalarys { get; set; }
        public DbSet<InstantDiscount> InstantDiscounts { get; set; }
        public DbSet<EsBillSundries> EsBillSundries { get; set; }
        public DbSet<leaddashbordorder> leaddashbordorder { get; set; }
        public DbSet<protaskdashbordorder> protaskdashbordorder { get; set; }
        public DbSet<itemtasklist> itemtasklist { get; set; }
        public DbSet<AdditionalMc> AdditionalMc { get; set; }
        public DbSet<purchaseapproval> purchaseapproval { get; set; }
        
        public DbSet<TaskGroup> TaskGroup { get; set; }

        public DbSet<sharedaccount> sharedaccounts { get; set; }

        public DbSet<SalaryStructure> SalaryStructures { get; set; }
        public DbSet<SalaryStrDetail> SalaryStrDetails { get; set; }
        public DbSet<EmpGradeSalaryDetail> EmpGradeSalaryDetails { get; set; }

        public DbSet<CalendarTemplate> CalendarTemplates { get; set; }
        public DbSet<WeeklyHoliday> WeeklyHolidays { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        //pos
        public DbSet<WalkinCustomer> WalkinCustomers { get; set; }
        public DbSet<DailyAttendance> DailyAttendances { get; set; }
        public DbSet<PosData> PPosDatas { get; set; }
        public DbSet<POSOrder> POSOrders { get; set; }
        public DbSet<POSOrderItem> POSOrderItems { get; set; }
        public DbSet<PaymentCardType> PaymentCardTypes { get; set; }
        public DbSet<DailyAttendanceDetail> DailyAttendanceDetails { get; set; }

        public DbSet<LeaveSettlement> LeaveSettlements { get; set; }
        public DbSet<LeaveSettlementPayHead> LeaveSettlementPayHeads { get; set; }
        public DbSet<ItemAddOn> ItemAddOns { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<FinalSettlement> FinalSettlements { get; set; }
        public DbSet<PayheadFS> PayheadFSs { get; set; }



        public DbSet<VenderRateMaster> VenderRateMaster { get; set; }
        public DbSet<VenterRateDetails> VenterRateDetails { get; set; }
        // public DbSet<LeadApprovals> LeadApprovals { get; set; }

        public DbSet<LeadApprovedEmployees> LeadApprovedEmployees { get; set; }
        public DbSet<sop> sops { get; set; }
        public DbSet<servicetype> servicetypes { get; set; }
        public DbSet<sopdet> sopdets { get; set; }
        #region Property
        public DbSet<ContractorType> ContractorTypes { get; set; }
        public DbSet<PropertyType> PropertyTypes { get; set; }
        public DbSet<PropertySettings> PropertySettingss { get; set; }

        public DbSet<Landlord> Landlords { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Broker> Brokers { get; set; }
        public DbSet<Developer> Developers { get; set; }
        public DbSet<Contractor> Contractors { get; set; }

        public DbSet<PropertyMain> PropertyMains { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyDocument> PropertyDocuments { get; set; }
        public DbSet<SelectedFeature> SelectedFeatures { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<PropertyFeature> PropertyFeatures { get; set; }
        public DbSet<AdditionalField> AdditionalFields { get; set; }
        public DbSet<AdditionalFieldData> AdditionalFieldDatas { get; set; }

        public DbSet<PropertyUnit> PropertyUnits { get; set; }
        public DbSet<PropertyUnitImage> PropertyUnitImages { get; set; }
        public DbSet<PropertyUnitDocument> PropertyUnitDocuments { get; set; }
        public DbSet<SelectedUnitFeature> SelectedUnitFeatures { get; set; }
        public DbSet<PropertyUnitType> PropertyUnitTypes { get; set; }
        public DbSet<PropertyUnitFeature> PropertyUnitFeatures { get; set; }
        public DbSet<PropertyRegistration> PropertyRegistrations { get; set; }
        public DbSet<Rental> Rentals { get; set; }

        public DbSet<Duration> Durations { get; set; }
        public DbSet<TenancyContract> TenancyContracts { get; set; }
        public DbSet<Cheque> Cheques { get; set; }
        public DbSet<ChequeImage> ChequeImages { get; set; }
        public DbSet<ContractDocument> ContractDocuments { get; set; }

        public DbSet<TenancyDocumentType> TenancyDocumentTypes { get; set; }
        public DbSet<DocumentFile> DocumentFiles { get; set; }

        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<RentalProforma> RentalProformas { get; set; }

        // Real-Estate advanced features (additive, isolated):
        public DbSet<PropertyRentReceipt> PropertyRentReceipts { get; set; }
        public DbSet<PropertyReminderLog> PropertyReminderLogs { get; set; }
        public DbSet<PropertyMaintenanceTask> PropertyMaintenanceTasks { get; set; }

        public DbSet<PropertyDocumentType> PropertyDocumentTypes { get; set; }

        public DbSet<ContractType> ContractTypes { get; set; }

        public DbSet<ContactRelation> ContactRelation { get; set; }
        public DbSet<CustomerDocument> CustomerDocuments { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<States> States { get; set; }
        //public DbSet<Location> Location { get; set; }
        public DbSet<Emirate> Emirates { get; set; }
        public DbSet<Actions> Actions { get; set; }
        public DbSet<LeadApprovals> LeadApprovals { get; set; }
        public DbSet<EstimateItems> EstimateItems { get; set; }
        public DbSet<Estimate> Estimates { get; set; }
        #endregion

        // Global decimal precision — replaces the EF6 DecimalPropertyConvention(38,18).
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(38, 18);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // IdentityRole shares the "AppModules" table (as in the legacy app).
            modelBuilder.Entity<IdentityRole>().ToTable("AppModules");

            modelBuilder.Entity<RoleGroup>().HasMany(g => g.UserGroup).WithOne().HasForeignKey(ag => ag.RoleGroupId);
            modelBuilder.Entity<UserGroup>().HasKey(r => new { r.UserId, r.RoleGroupId });
            modelBuilder.Entity<UserGroup>().ToTable("UserGroups");

            modelBuilder.Entity<RoleGroup>().HasMany(g => g.GroupModules).WithOne().HasForeignKey(ap => ap.RoleGroupId);
            modelBuilder.Entity<RoleGroupModule>().HasKey(gr => new { gr.RoleGroupId, gr.ModuleId });
            modelBuilder.Entity<RoleGroupModule>().ToTable("RoleGroupModule");

            // Keyless DTO for tenancy expiry query
            modelBuilder.Entity<TenancyContractExpiryDto>().HasNoKey();

            RemapLegacyTableNames(modelBuilder);
        }

        // The legacy EF6 model had ZERO [Table] attributes and relied on EF6's automatic pluralization
        // of the ENTITY TYPE name (Company->Companies, Tax->Taxes). EF Core does not pluralize, so every
        // table name mismatches. We read the real table names from the DB once at model-build time and
        // map each entity to its actual table — candidate pluralizations are validated against the real
        // table set, so even irregular EF6 plurals resolve correctly (and an unmatched entity keeps its
        // default name rather than guessing wrong). Identity tables (AspNet*/AppModules) are left as the
        // base IdentityDbContext / explicit maps already set them.
        private void RemapLegacyTableNames(ModelBuilder modelBuilder)
        {
            var real = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            var cols = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>(System.StringComparer.OrdinalIgnoreCase);
            try
            {
                using var cn = new Microsoft.Data.SqlClient.SqlConnection(LegacyWeb.ConnectionString);
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                    using var rd = cmd.ExecuteReader();
                    while (rd.Read()) { var t = rd.GetString(0); real[t] = t; }
                }
                using (var cmd2 = cn.CreateCommand())
                {
                    cmd2.CommandText = "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS";
                    using var rd2 = cmd2.ExecuteReader();
                    while (rd2.Read())
                    {
                        var t = rd2.GetString(0); var c = rd2.GetString(1);
                        if (!cols.TryGetValue(t, out var set)) { set = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase); cols[t] = set; }
                        set.Add(c);
                    }
                }
            }
            catch (System.Exception ex) { System.Console.Error.WriteLine("[RemapLegacyTableNames] FAILED: " + ex.Message); return; }
            System.Console.Error.WriteLine("[RemapLegacyTableNames] read " + real.Count + " tables, " + cols.Count + " with columns");
            if (real.Count == 0) return;

            // (1) Table names: map each entity to its real (EF6-pluralized) table.
            foreach (var et in modelBuilder.Model.GetEntityTypes())
            {
                var clr = et.ClrType;
                if (clr == null || clr == typeof(ApplicationUser)) continue;
                if (clr.Namespace == "Microsoft.AspNetCore.Identity") continue;
                if (typeof(IdentityRole).IsAssignableFrom(clr)) continue; // AppModules + IdentityRole
                var current = et.GetTableName();
                if (current != null && real.ContainsKey(current)) continue; // already correct (incl. explicit maps)
                foreach (var cand in TableNameCandidates(clr.Name))
                    if (real.TryGetValue(cand, out var actual)) { et.SetTableName(actual); break; }
            }

            // (2) EF6 used the FK-column convention {NavigationName}_{PrincipalKey} (e.g. Customer.AccountID
            //     nav -> column "AccountID_AccountsID"); EF Core uses {Nav}{PrincipalKey} -> it fabricates a
            //     SHADOW column ("AccountIDAccountsID") that doesn't exist and crashes every query on that
            //     table. For each shadow FK, point it at the real EF6 column when that column exists -> the
            //     legacy LINQ navigations (customer.AccountID.CreatedBy) translate against the real schema.
            //     When no matching real column exists the relationship is truly phantom -> ignore the nav.
            var toIgnore = new System.Collections.Generic.HashSet<(System.Type, string)>();
            int mapped = 0;
            foreach (var et in modelBuilder.Model.GetEntityTypes())
            {
                var clr = et.ClrType;
                if (clr == null || clr == typeof(ApplicationUser)) continue;
                if (clr.Namespace == "Microsoft.AspNetCore.Identity") continue;
                if (typeof(IdentityRole).IsAssignableFrom(clr)) continue;
                var tbl = et.GetTableName();
                System.Collections.Generic.HashSet<string> tableCols = null;
                if (tbl != null) cols.TryGetValue(tbl, out tableCols);
                foreach (var fk in System.Linq.Enumerable.ToList(et.GetForeignKeys()))
                {
                    if (fk.Properties.Count != 1) continue;
                    var prop = fk.Properties[0];
                    if (!prop.IsShadowProperty()) continue; // real scalar FK already maps to its column
                    var navName = fk.DependentToPrincipal != null ? fk.DependentToPrincipal.Name : null;
                    var pkName = fk.PrincipalKey.Properties.Count == 1 ? fk.PrincipalKey.Properties[0].Name : null;
                    var ef6Col = (navName != null && pkName != null) ? navName + "_" + pkName : null;
                    if (ef6Col != null && tableCols != null && tableCols.Contains(ef6Col))
                    {
                        prop.SetColumnName(ef6Col);
                        mapped++;
                    }
                    else
                    {
                        if (fk.DependentToPrincipal != null) toIgnore.Add((fk.DeclaringEntityType.ClrType, fk.DependentToPrincipal.Name));
                        if (fk.PrincipalToDependent != null) toIgnore.Add((fk.PrincipalEntityType.ClrType, fk.PrincipalToDependent.Name));
                    }
                }
            }
            foreach (var (type, nav) in toIgnore)
            {
                try { modelBuilder.Entity(type).Ignore(nav); } catch { }
            }
            System.Console.Error.WriteLine("[RemapLegacyTableNames] mapped " + mapped + " shadow FKs to EF6 columns, ignored " + toIgnore.Count + " phantom navs");
        }

        private static System.Collections.Generic.IEnumerable<string> TableNameCandidates(string n)
        {
            yield return Pluralize(n);                                   // EF6-style best guess
            yield return n + "s";
            yield return n + "es";
            if (n.EndsWith("y")) yield return n.Substring(0, n.Length - 1) + "ies";
            yield return n + "ies";
            yield return n;                                             // unpluralized fallback
        }

        private static string Pluralize(string n)
        {
            if (string.IsNullOrEmpty(n)) return n;
            var l = n.ToLowerInvariant();
            if (l.EndsWith("y") && n.Length > 1 && "aeiou".IndexOf(l[l.Length - 2]) < 0)
                return n.Substring(0, n.Length - 1) + "ies";
            if (l.EndsWith("s") || l.EndsWith("x") || l.EndsWith("z") || l.EndsWith("ch") || l.EndsWith("sh"))
                return n + "es";
            return n + "s";
        }

        public DbSet<QuickSoft.ViewModel.AddCustomerLiteviewmodel> AddCustomerLiteviewmodels { get; set; }
    }
}
