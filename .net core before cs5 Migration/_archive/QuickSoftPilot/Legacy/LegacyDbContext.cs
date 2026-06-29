using Microsoft.EntityFrameworkCore;

namespace QuickSoftPilot.Legacy
{
    // The new app's data layer over the real QuickNet database (emirtechlatest) on SQL Server Express.
    // Read + write. Entities map just the columns the pilot screens use; EF Core ignores the rest.
    public class LegacyDbContext : DbContext
    {
        public LegacyDbContext(DbContextOptions<LegacyDbContext> options) : base(options) { }

        public DbSet<LegacyItemCategory> ItemCategories { get; set; }
        public DbSet<LegacyQuotation> Quotations { get; set; }
        public DbSet<LegacyQuotationItem> QuotationItems { get; set; }
        public DbSet<LegacyCustomer> Customers { get; set; }
        public DbSet<LegacyItem> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<LegacyItemCategory>().ToTable("ItemCategories").HasKey(x => x.ItemCategoryID);
            b.Entity<LegacyQuotation>().ToTable("Quotations").HasKey(x => x.QuotationId);
            b.Entity<LegacyQuotationItem>().ToTable("QuotationItems").HasKey(x => x.QuotationItemId);
            b.Entity<LegacyCustomer>().ToTable("Customers").HasKey(x => x.CustomerID);
            b.Entity<LegacyItem>().ToTable("Items").HasKey(x => x.ItemId);
        }
    }

    public class LegacyItemCategory
    {
        public long ItemCategoryID { get; set; }   // identity in the real DB
        public string ItemCategoryName { get; set; }
        public long? Parent { get; set; }
        public string Description { get; set; }
        public int Editable { get; set; }           // NOT NULL in the real DB
    }

    public class LegacyQuotation
    {
        public long QuotationId { get; set; }
        public long QuotNo { get; set; }
        public string BillNo { get; set; }
        public DateTime QuotDate { get; set; }
        public long Customer { get; set; }
        public int QuotItems { get; set; }
        public decimal QuotItemQuantity { get; set; }
        public decimal QuotSubTotal { get; set; }
        public decimal QuotTaxAmount { get; set; }
        public decimal QuotGrandTotal { get; set; }
        public string QuotNote { get; set; }
    }

    public class LegacyQuotationItem
    {
        public long QuotationItemId { get; set; }
        public long Quotation { get; set; }   // FK -> Quotations.QuotationId
        public long Item { get; set; }        // FK -> Items.ItemId
        public decimal ItemQuantity { get; set; }
        public decimal ItemUnitPrice { get; set; }
        public decimal ItemTotalAmount { get; set; }
        public string ItemNote { get; set; }
    }

    public class LegacyCustomer
    {
        public long CustomerID { get; set; }
        public string CustomerName { get; set; }
    }

    public class LegacyItem
    {
        public long ItemId { get; set; }
        public string ItemName { get; set; }
    }
}
