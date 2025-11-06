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
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<TailorProfile> TailorProfiles { get; set; }
    public virtual DbSet<TailorVerification> TailorVerifications { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserAddress> UserAddresses { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderImages> OrderImages { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<Payment> Payment { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<RatingDimension> RatingDimensions { get; set; }
    public virtual DbSet<PortfolioImage> PortfolioImages { get; set; }
    public virtual DbSet<TailorService> TailorServices { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<AppSetting> AppSettings { get; set; }

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
            entity.Property(e => e.EmailNotifications).HasDefaultValue(true);
            entity.Property(e => e.SmsNotifications).HasDefaultValue(true);
            entity.Property(e => e.PromotionalNotifications).HasDefaultValue(true);

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

        // TailorVerification Entity
        modelBuilder.Entity<TailorVerification>(entity =>
        {
            entity.ToTable("TailorVerifications");
         entity.HasKey(e => e.Id).HasName("PK_TailorVerifications");

          entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.NationalIdNumber).IsRequired().HasMaxLength(50);
      entity.Property(e => e.FullLegalName).IsRequired().HasMaxLength(200);
 entity.Property(e => e.Nationality).HasMaxLength(100);
     entity.Property(e => e.CommercialRegistrationNumber).HasMaxLength(100);
            entity.Property(e => e.ProfessionalLicenseNumber).HasMaxLength(100);
         entity.Property(e => e.IdDocumentFrontContentType).HasMaxLength(100);
        entity.Property(e => e.IdDocumentBackContentType).HasMaxLength(100);
    entity.Property(e => e.CommercialRegistrationContentType).HasMaxLength(100);
            entity.Property(e => e.ProfessionalLicenseContentType).HasMaxLength(100);
            entity.Property(e => e.ReviewNotes).HasMaxLength(1000);
            entity.Property(e => e.RejectionReason).HasMaxLength(1000);
            entity.Property(e => e.AdditionalNotes).HasMaxLength(500);
    entity.Property(e => e.Status).HasDefaultValue(VerificationStatus.Pending);
   entity.Property(e => e.SubmittedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasIndex(e => e.TailorProfileId).HasDatabaseName("IX_TailorVerifications_TailorProfileId");
     entity.HasIndex(e => e.Status).HasDatabaseName("IX_TailorVerifications_Status");

            entity.HasOne(v => v.TailorProfile)
           .WithOne(t => t.Verification)
   .HasForeignKey<TailorVerification>(v => v.TailorProfileId)
    .HasPrincipalKey<TailorProfile>(t => t.Id)
            .OnDelete(DeleteBehavior.NoAction);

         entity.HasOne(v => v.ReviewedByAdmin)
            .WithMany()
    .HasForeignKey(v => v.ReviewedByAdminId)
                .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.NoAction)
     .IsRequired(false);
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

        // RefreshToken Entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07E9DA722D");

            entity.HasIndex(e => e.ExpiresAt, "IX_RefreshTokens_ExpiresAt");
            entity.HasIndex(e => e.UserId, "IX_RefreshTokens_UserId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User)
           .WithMany(p => p.RefreshTokens)
     .HasForeignKey(d => d.UserId)
         .OnDelete(DeleteBehavior.NoAction)
        .HasConstraintName("FK_RefreshTokens_Users");
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

        // Notification Entity - Fix shadow property warning
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(e => e.NotificationId).HasName("PK_Notifications");

            entity.Property(e => e.NotificationId).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserId).IsRequired(false); // NULL for system messages
            entity.Property(e => e.AudienceType).HasMaxLength(50); // "All", "Customers", "Tailors"
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.SentAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_Notifications_UserId");
            entity.HasIndex(e => e.AudienceType).HasDatabaseName("IX_Notifications_AudienceType");

            entity.HasOne(n => n.User)
   .WithMany()
        .HasForeignKey(n => n.UserId)
     .HasPrincipalKey(u => u.Id)
        .OnDelete(DeleteBehavior.NoAction)
    .IsRequired(false);
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

        // Review Entity - FIXED: Remove ambiguous relationships
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(e => e.ReviewId).HasName("PK_Reviews");

            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasIndex(e => e.OrderId).HasDatabaseName("IX_Reviews_OrderId");
            entity.HasIndex(e => e.TailorId).HasDatabaseName("IX_Reviews_TailorId");
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_Reviews_CustomerId");

            // FIXED: Use explicit HasForeignKey with proper principal keys
            entity.HasOne(r => r.Order)
                    .WithMany()
                .HasForeignKey(r => r.OrderId)
           .HasPrincipalKey(o => o.OrderId)
          .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(r => r.Tailor)
       .WithMany()
        .HasForeignKey(r => r.TailorId)
 .HasPrincipalKey(t => t.Id)
 .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(r => r.Customer)
           .WithMany()
         .HasForeignKey(r => r.CustomerId)
                .HasPrincipalKey(c => c.Id)
           .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(r => r.RatingDimensions)
                .WithOne(rd => rd.Review)
                   .HasForeignKey(rd => rd.ReviewId)
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

        // Ensure all foreign keys use NoAction to prevent multiple cascade path errors
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
