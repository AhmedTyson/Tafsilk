using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Web.Models;

namespace TafsilkPlatform.Web.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        // OPTIMIZATION: Configure default query behavior
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = true;
        ChangeTracker.LazyLoadingEnabled = false;
    }

    // DbSet Properties - Essential entities only
    public virtual DbSet<CustomerProfile> CustomerProfiles { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<TailorProfile> TailorProfiles { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserAddress> UserAddresses { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderImages> OrderImages { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<Payment> Payment { get; set; }
    public virtual DbSet<PortfolioImage> PortfolioImages { get; set; }
    public virtual DbSet<TailorService> TailorServices { get; set; }
    public virtual DbSet<AppSetting> AppSettings { get; set; }
    // ✅ IDEMPOTENCY: Idempotency keys for preventing duplicate requests
    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
    
    // ✅ NEW: Loyalty and rewards system
    public virtual DbSet<CustomerLoyalty> CustomerLoyalties { get; set; }
    public virtual DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    
    // ✅ NEW: Saved measurements for faster rebooking
    public virtual DbSet<CustomerMeasurement> CustomerMeasurements { get; set; }
    
    // ✅ NEW: Complaints and support system
    public virtual DbSet<Complaint> Complaints { get; set; }
    public virtual DbSet<ComplaintAttachment> ComplaintAttachments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Fallback to named connection string (resolved from IConfiguration when available)
            optionsBuilder.UseSqlServer(
          "Name=ConnectionStrings:TafsilkPlatform",
    sqlOptions =>
    {
        // OPTIMIZATION: Enable query splitting to avoid cartesian explosion
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
        }

        // OPTIMIZATION: Only enable sensitive data logging in Development environment
        // This will be controlled by the environment-specific configuration in Program.cs
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User Entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07AA820590");

            entity.HasIndex(e => e.Email, "IX_Users_Email");
            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");
            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534975288F0").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.Role)
                         .WithMany(p => p.Users)
               .HasForeignKey(d => d.RoleId)
                         .OnDelete(DeleteBehavior.ClientSetNull)
                         .HasConstraintName("FK_Users_Roles");
        });

        // Role Entity
        modelBuilder.Entity<Role>(entity =>
 {
     entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07CB85E41E");

     entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
     entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
     entity.Property(e => e.Description).HasMaxLength(255);
     entity.Property(e => e.Name).HasMaxLength(50);
     entity.Property(e => e.Permissions).HasMaxLength(2000); // JSON permissions
     entity.Property(e => e.Priority).HasDefaultValue(0);
 });

        // CustomerProfile Entity
        modelBuilder.Entity<CustomerProfile>(entity =>
             {
                 entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07880E7F94");

                 entity.HasIndex(e => e.UserId, "IX_CustomerProfiles_UserId");
                 entity.HasIndex(e => e.UserId, "UQ__Customer__1788CC4D90808B91").IsUnique();

                 entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
                 entity.Property(e => e.City).HasMaxLength(100);
                 entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
                 entity.Property(e => e.FullName).HasMaxLength(255);
                 entity.Property(e => e.Gender).HasMaxLength(20);
                 entity.Property(e => e.Bio).HasMaxLength(1000);
#pragma warning disable CS0618
                 entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
#pragma warning restore CS0618
                 entity.Property(e => e.ProfilePictureContentType).HasMaxLength(100);
                 entity.Property(e => e.ProfilePictureData).HasColumnType("varbinary(max)");

                 entity.HasOne(d => d.User)
           .WithOne(p => p.CustomerProfile)
              .HasForeignKey<CustomerProfile>(d => d.UserId)
               .OnDelete(DeleteBehavior.NoAction)
         .HasConstraintName("FK_CustomerProfiles_Users");
             });

        // TailorProfile Entity
        modelBuilder.Entity<TailorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TailorPr__3214EC07A3FCF42C");

            entity.HasIndex(e => e.UserId, "IX_TailorProfiles_UserId");
            entity.HasIndex(e => e.UserId, "UQ__TailorPr__1788CC4D37A4BF4A").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.PricingRange).HasMaxLength(100);
            entity.Property(e => e.ShopName).HasMaxLength(255);
#pragma warning disable CS0618
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
#pragma warning restore CS0618
            entity.Property(e => e.ProfilePictureContentType).HasMaxLength(100);
            entity.Property(e => e.ProfilePictureData).HasColumnType("varbinary(max)");

            entity.HasOne(d => d.User)
        .WithOne(p => p.TailorProfile)
   .HasForeignKey<TailorProfile>(d => d.UserId)
        .OnDelete(DeleteBehavior.NoAction)
        .HasConstraintName("FK_TailorProfiles_Users");
});

// UserAddress Entity
        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserAddr__3214EC07DDE0E48B");

            entity.HasIndex(e => e.UserId, "IX_UserAddresses_UserId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsDefault).HasDefaultValue(false);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.Street).HasMaxLength(255);

            entity.HasOne(d => d.User)
        .WithMany(p => p.UserAddresses)
        .HasForeignKey(d => d.UserId)
        .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_UserAddresses_Users");
        });

        // Order Entity + relations
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CustomerId).IsRequired();
            entity.Property(o => o.TailorId).IsRequired();

            entity.HasOne(o => o.Customer)
                          .WithMany(c => c.orders)
              .HasForeignKey(o => o.CustomerId)
                 .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(o => o.Tailor)
                .WithMany()
                      .HasForeignKey(o => o.TailorId)
                 .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(o => o.Items)
 .WithOne(oi => oi.Order)
  .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(o => o.orderImages)
      .WithOne(oi => oi.Order)
     .HasForeignKey(oi => oi.OrderId)
  .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(o => o.Payments)
               .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // OrderItem Entity - Fix decimal precision
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").HasPrecision(18, 2);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)").HasPrecision(18, 2);

            entity.HasOne(e => e.Order)
              .WithMany(o => o.Items)
                   .HasForeignKey(e => e.OrderId)
     .OnDelete(DeleteBehavior.NoAction);
        });

        // Payment Entity - Fix decimal precision
        modelBuilder.Entity<Payment>(entity =>
           {
               entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").HasPrecision(18, 2);

               entity.HasOne(p => p.Customer)
      .WithMany(cp => cp.Payments)
 .HasForeignKey(p => p.CustomerId)
    .OnDelete(DeleteBehavior.NoAction);

               entity.HasOne(p => p.Order)
        .WithMany(o => o.Payments)
        .HasForeignKey(p => p.OrderId)
           .OnDelete(DeleteBehavior.NoAction);

               entity.HasOne(p => p.Tailor)
           .WithMany(tp => tp.Payments)
              .HasForeignKey(p => p.TailorId)
        .OnDelete(DeleteBehavior.NoAction);
           });

        // PortfolioImage Entity - Fix shadow property warning and decimal precision
        modelBuilder.Entity<PortfolioImage>(entity =>
        {
            entity.ToTable("PortfolioImages");
            entity.HasKey(e => e.PortfolioImageId).HasName("PK_PortfolioImages");

            entity.Property(e => e.PortfolioImageId).ValueGeneratedOnAdd();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            // FIXED: Add decimal precision for EstimatedPrice
            entity.Property(e => e.EstimatedPrice)
     .HasColumnType("decimal(18,2)")
        .HasPrecision(18, 2);

            entity.HasIndex(e => e.TailorId).HasDatabaseName("IX_PortfolioImages_TailorId");

            entity.HasOne(pi => pi.Tailor)
     .WithMany(t => t.PortfolioImages)
.HasForeignKey(pi => pi.TailorId)
      .HasPrincipalKey(t => t.Id)
  .OnDelete(DeleteBehavior.NoAction);
        });

        // TailorService Entity - Fix shadow property warning
        modelBuilder.Entity<TailorService>(entity =>
                {
                    entity.ToTable("TailorServices");
                    entity.HasKey(e => e.TailorServiceId).HasName("PK_TailorServices");

                    entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
                    entity.Property(e => e.Description).HasMaxLength(500);
                    entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)").HasPrecision(18, 2);

                    entity.HasIndex(e => e.TailorId).HasDatabaseName("IX_TailorServices_TailorId");

                    entity.HasOne(ts => ts.Tailor)
      .WithMany(t => t.TailorServices)
            .HasForeignKey(ts => ts.TailorId)
          .HasPrincipalKey(t => t.Id)
                    .OnDelete(DeleteBehavior.NoAction);
                });

        // OrderImages Entity - Fix shadow property warning
        modelBuilder.Entity<OrderImages>(entity =>
          {
              entity.HasIndex(e => e.OrderId).HasDatabaseName("IX_OrderImages_OrderId");

              entity.HasOne(oi => oi.Order)
         .WithMany(o => o.orderImages)
      .HasForeignKey(oi => oi.OrderId)
             .HasPrincipalKey(o => o.OrderId)
          .OnDelete(DeleteBehavior.NoAction);
          });

        // ✅ IDEMPOTENCY: IdempotencyKey Entity Configuration
    modelBuilder.Entity<IdempotencyKey>(entity =>
      {
     entity.ToTable("IdempotencyKeys");
    entity.HasKey(e => e.Key).HasName("PK_IdempotencyKeys");

    entity.Property(e => e.Key).IsRequired().HasMaxLength(128);
entity.Property(e => e.Status).HasDefaultValue(IdempotencyStatus.InProgress);
  entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("(getutcdate())");
   entity.Property(e => e.ContentType).HasMaxLength(100);
      entity.Property(e => e.Endpoint).HasMaxLength(500);
       entity.Property(e => e.Method).HasMaxLength(10);

  // Indexes for performance
     entity.HasIndex(e => e.Status).HasDatabaseName("IX_IdempotencyKeys_Status");
        entity.HasIndex(e => e.ExpiresAtUtc).HasDatabaseName("IX_IdempotencyKeys_ExpiresAtUtc");
     entity.HasIndex(e => e.UserId).HasDatabaseName("IX_IdempotencyKeys_UserId");
     });
        
        // ✅ NEW: CustomerLoyalty Entity Configuration
     modelBuilder.Entity<CustomerLoyalty>(entity =>
        {
        entity.ToTable("CustomerLoyalty");
            entity.HasKey(e => e.Id).HasName("PK_CustomerLoyalty");
            
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
  entity.Property(e => e.Tier).HasMaxLength(50).HasDefaultValue("Bronze");
   entity.Property(e => e.ReferralCode).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
       
   entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_CustomerLoyalty_CustomerId").IsUnique();
            entity.HasIndex(e => e.ReferralCode).HasDatabaseName("IX_CustomerLoyalty_ReferralCode");

            entity.HasOne(l => l.Customer)
    .WithOne(c => c.Loyalty)
           .HasForeignKey<CustomerLoyalty>(l => l.CustomerId)
  .OnDelete(DeleteBehavior.NoAction);
    });
        
        // ✅ NEW: LoyaltyTransaction Entity Configuration
        modelBuilder.Entity<LoyaltyTransaction>(entity =>
      {
   entity.ToTable("LoyaltyTransactions");
        entity.HasKey(e => e.Id).HasName("PK_LoyaltyTransactions");

      entity.Property(e => e.Id).ValueGeneratedOnAdd();
     entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
         entity.Property(e => e.Description).HasMaxLength(200);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
      
     entity.HasIndex(e => e.CustomerLoyaltyId).HasDatabaseName("IX_LoyaltyTransactions_CustomerLoyaltyId");
   entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_LoyaltyTransactions_CreatedAt");
    
    entity.HasOne(t => t.CustomerLoyalty)
     .WithMany(l => l.Transactions)
   .HasForeignKey(t => t.CustomerLoyaltyId)
     .OnDelete(DeleteBehavior.NoAction);
        });
        
  // ✅ NEW: CustomerMeasurement Entity Configuration
      modelBuilder.Entity<CustomerMeasurement>(entity =>
 {
            entity.ToTable("CustomerMeasurements");
   entity.HasKey(e => e.Id).HasName("PK_CustomerMeasurements");
    
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.GarmentType).HasMaxLength(50);
       entity.Property(e => e.CustomMeasurementsJson).HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(500);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            
            // Decimal precision for measurements
          entity.Property(e => e.Chest).HasColumnType("decimal(5,2)");
         entity.Property(e => e.Waist).HasColumnType("decimal(5,2)");
     entity.Property(e => e.Hips).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ShoulderWidth).HasColumnType("decimal(5,2)");
  entity.Property(e => e.SleeveLength).HasColumnType("decimal(5,2)");
            entity.Property(e => e.InseamLength).HasColumnType("decimal(5,2)");
            entity.Property(e => e.OutseamLength).HasColumnType("decimal(5,2)");
   entity.Property(e => e.NeckCircumference).HasColumnType("decimal(5,2)");
entity.Property(e => e.ArmLength).HasColumnType("decimal(5,2)");
            entity.Property(e => e.ThighCircumference).HasColumnType("decimal(5,2)");
       entity.Property(e => e.ThobeLength).HasColumnType("decimal(5,2)");
        entity.Property(e => e.AbayaLength).HasColumnType("decimal(5,2)");
         
   entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_CustomerMeasurements_CustomerId");
    
      entity.HasOne(m => m.Customer)
     .WithMany(c => c.SavedMeasurements)
      .HasForeignKey(m => m.CustomerId)
     .OnDelete(DeleteBehavior.NoAction);
        });
      
        // ✅ NEW: Complaint Entity Configuration
      modelBuilder.Entity<Complaint>(entity =>
        {
            entity.ToTable("Complaints");
      entity.HasKey(e => e.Id).HasName("PK_Complaints");
        
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
     entity.Property(e => e.ComplaintType).HasMaxLength(50).HasDefaultValue("Other");
            entity.Property(e => e.DesiredResolution).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Open");
        entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("Medium");
            entity.Property(e => e.AdminResponse).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
        
            entity.HasIndex(e => e.OrderId).HasDatabaseName("IX_Complaints_OrderId");
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_Complaints_CustomerId");
   entity.HasIndex(e => e.TailorId).HasDatabaseName("IX_Complaints_TailorId");
         entity.HasIndex(e => e.Status).HasDatabaseName("IX_Complaints_Status");
            
            entity.HasOne(c => c.Order)
      .WithMany(o => o.Complaints)
.HasForeignKey(c => c.OrderId)
     .OnDelete(DeleteBehavior.NoAction);
           
            entity.HasOne(c => c.Customer)
       .WithMany(cp => cp.Complaints)
  .HasForeignKey(c => c.CustomerId)
         .OnDelete(DeleteBehavior.NoAction);
      
            entity.HasOne(c => c.Tailor)
           .WithMany()
          .HasForeignKey(c => c.TailorId)
    .OnDelete(DeleteBehavior.NoAction);
        });
        
        // ✅ NEW: ComplaintAttachment Entity Configuration
        modelBuilder.Entity<ComplaintAttachment>(entity =>
        {
entity.ToTable("ComplaintAttachments");
      entity.HasKey(e => e.Id).HasName("PK_ComplaintAttachments");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.FileData).HasColumnType("varbinary(max)");
  entity.Property(e => e.ContentType).HasMaxLength(100);
       entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("(getutcdate())");
            
            entity.HasIndex(e => e.ComplaintId).HasDatabaseName("IX_ComplaintAttachments_ComplaintId");
            
  entity.HasOne(a => a.Complaint)
   .WithMany(c => c.Attachments)
   .HasForeignKey(a => a.ComplaintId)
    .OnDelete(DeleteBehavior.NoAction);
        });

        // Ensure all foreign keys use NoAction to prevent multiple cascade path errors
  foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
  {
 foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
        }

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
