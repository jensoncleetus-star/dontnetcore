using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuickSoftPilot.Scaffolded;

public partial class RealQuickNetContext : DbContext
{
    public RealQuickNetContext(DbContextOptions<RealQuickNetContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemCategory> ItemCategories { get; set; }

    public virtual DbSet<Quotation> Quotations { get; set; }

    public virtual DbSet<QuotationItem> QuotationItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK_dbo.Customers");

            entity.HasIndex(e => new { e.CustomerId, e.LeadStat }, "NonClusteredIndex-20210908-153356");

            entity.HasIndex(e => e.CustomerId, "NonClusteredIndex-20220617-113456");

            entity.HasIndex(e => e.Logtime, "NonClusteredIndex-20220808-090555");

            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.AccountIdAccountsId).HasColumnName("AccountID_AccountsID");
            entity.Property(e => e.Addres).IsUnicode(false);
            entity.Property(e => e.Bonusbaseamount).HasColumnName("bonusbaseamount");
            entity.Property(e => e.Bonuscheck).HasColumnName("bonuscheck");
            entity.Property(e => e.Bonusclimembility)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("bonusclimembility");
            entity.Property(e => e.Bonuspercentage)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("bonuspercentage");
            entity.Property(e => e.ContactIdContactId).HasColumnName("ContactID_ContactID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.CreatedBy).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.CustomerPrintName)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Expectedamount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("expectedamount");
            entity.Property(e => e.Includepdc).HasColumnName("includepdc");
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Logtime)
                .HasColumnType("datetime")
                .HasColumnName("logtime");
            entity.Property(e => e.Priority)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("priority");
            entity.Property(e => e.Ref1).HasMaxLength(50);
            entity.Property(e => e.Ref2).HasMaxLength(50);
            entity.Property(e => e.Ref3).HasMaxLength(50);
            entity.Property(e => e.Ref4).HasMaxLength(50);
            entity.Property(e => e.Ref5).HasMaxLength(50);
            entity.Property(e => e.SourceId).HasColumnName("SourceID");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Startbonusdate)
                .HasColumnType("datetime")
                .HasColumnName("startbonusdate");
            entity.Property(e => e.StateId).HasColumnName("StateID");
            entity.Property(e => e.TaxIdTrn)
                .HasMaxLength(70)
                .HasColumnName("TaxID_TRN");
            entity.Property(e => e.TaxRegNo)
                .IsRequired()
                .HasMaxLength(1000)
                .HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK_dbo.Items");

            entity.HasIndex(e => new { e.ItemName, e.ItemCode }, "NonClusteredIndex-20211106-084248");

            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Accmap).HasColumnName("accmap");
            entity.Property(e => e.Accountid).HasColumnName("accountid");
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Cashprice)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("cashprice");
            entity.Property(e => e.Commission).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ConFactor).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ConRate).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedUserId)
                .HasMaxLength(128)
                .HasColumnName("CreatedUserID");
            entity.Property(e => e.Creditprice)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("creditprice");
            entity.Property(e => e.Daysexpirty).HasColumnName("daysexpirty");
            entity.Property(e => e.ItemArabic).HasMaxLength(300);
            entity.Property(e => e.ItemBrandId).HasColumnName("ItemBrandID");
            entity.Property(e => e.ItemCategoryId).HasColumnName("ItemCategoryID");
            entity.Property(e => e.ItemCode).HasMaxLength(50);
            entity.Property(e => e.ItemColorId).HasColumnName("ItemColorID");
            entity.Property(e => e.ItemName).HasMaxLength(300);
            entity.Property(e => e.ItemSizeId).HasColumnName("ItemSizeID");
            entity.Property(e => e.ItemUnitId).HasColumnName("ItemUnitID");
            entity.Property(e => e.ItemUnitItemUnitId).HasColumnName("ItemUnit_ItemUnitID");
            entity.Property(e => e.Lockprice).HasColumnName("lockprice");
            entity.Property(e => e.MinStock).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Mrp)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("MRP");
            entity.Property(e => e.OpeningCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OpeningStock).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PricingStrategyValue)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Slreq).HasColumnName("slreq");
            entity.Property(e => e.StockValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubUnitItemUnitId).HasColumnName("SubUnit_ItemUnitID");
            entity.Property(e => e.SupplierRef).HasMaxLength(30);
            entity.Property(e => e.TaxId).HasColumnName("TaxID");

            entity.HasOne(d => d.ItemCategory).WithMany(p => p.Items)
                .HasForeignKey(d => d.ItemCategoryId)
                .HasConstraintName("FK_dbo.Items_dbo.ItemCategories_ItemCategoryID");
        });

        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.HasKey(e => e.ItemCategoryId).HasName("PK_dbo.ItemCategories");

            entity.Property(e => e.ItemCategoryId).HasColumnName("ItemCategoryID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ItemCategoryName)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Quotation>(entity =>
        {
            entity.HasKey(e => e.QuotationId).HasName("PK_dbo.Quotations");

            entity.Property(e => e.CompanyHeaderId).HasColumnName("CompanyHeaderID");
            entity.Property(e => e.ConvertionRate).HasMaxLength(10);
            entity.Property(e => e.CreatedBranchBranchId).HasColumnName("CreatedBranch_BranchID");
            entity.Property(e => e.EmailTemplateId).HasColumnName("EmailTemplateID");
            entity.Property(e => e.Expdate)
                .HasColumnType("datetime")
                .HasColumnName("expdate");
            entity.Property(e => e.Fctotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("FCTotal");
            entity.Property(e => e.Leadsid).HasColumnName("leadsid");
            entity.Property(e => e.PaymentTerms).HasMaxLength(50);
            entity.Property(e => e.QuotCreatedDate).HasColumnType("datetime");
            entity.Property(e => e.QuotDate).HasColumnType("datetime");
            entity.Property(e => e.QuotDiscount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuotGrandTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuotItemQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuotSubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuotTax).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuotTaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Quotationstatus).HasColumnName("quotationstatus");
            entity.Property(e => e.Ref1).HasMaxLength(50);
            entity.Property(e => e.Ref2).HasMaxLength(50);
            entity.Property(e => e.Ref3).HasMaxLength(50);
            entity.Property(e => e.Ref4).HasMaxLength(50);
            entity.Property(e => e.Ref5).HasMaxLength(50);
            entity.Property(e => e.Revision)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("revision");
            entity.Property(e => e.Servicetype)
                .HasDefaultValue(0L)
                .HasColumnName("servicetype");
            entity.Property(e => e.Sourceoflead).HasColumnName("sourceoflead");
        });

        modelBuilder.Entity<QuotationItem>(entity =>
        {
            entity.HasKey(e => e.QuotationItemId).HasName("PK_dbo.QuotationItems");

            entity.Property(e => e.ItemDiscount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ItemIdItemId).HasColumnName("ItemId_ItemID");
            entity.Property(e => e.ItemQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ItemSubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ItemTax).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ItemTaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ItemTotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ItemUnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuotEntryIdQuotationId).HasColumnName("QuotEntryId_QuotationId");

            entity.HasOne(d => d.ItemIdItem).WithMany(p => p.QuotationItems)
                .HasForeignKey(d => d.ItemIdItemId)
                .HasConstraintName("FK_dbo.QuotationItems_dbo.Items_ItemId_ItemID");

            entity.HasOne(d => d.QuotEntryIdQuotation).WithMany(p => p.QuotationItems)
                .HasForeignKey(d => d.QuotEntryIdQuotationId)
                .HasConstraintName("FK_dbo.QuotationItems_dbo.Quotations_QuotEntryId_QuotationId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
