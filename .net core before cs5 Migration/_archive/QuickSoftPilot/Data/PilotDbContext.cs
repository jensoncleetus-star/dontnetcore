using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuickSoftPilot.Models;

namespace QuickSoftPilot.Data
{
    public class PilotDbContext : IdentityDbContext<ApplicationUser>
    {
        public PilotDbContext(DbContextOptions<PilotDbContext> options) : base(options) { }

        public DbSet<ItemCategory> ItemCategorys { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Keep the legacy table names.
            builder.Entity<ItemCategory>().ToTable("ItemCategorys");
            builder.Entity<Quotation>().ToTable("Quotations");
            builder.Entity<QuotationItem>().ToTable("QuotationItems");

            // Header -> line items: cascade delete the children with the header.
            builder.Entity<QuotationItem>()
                .HasOne(i => i.Quotation)
                .WithMany(q => q.Items)
                .HasForeignKey(i => i.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
