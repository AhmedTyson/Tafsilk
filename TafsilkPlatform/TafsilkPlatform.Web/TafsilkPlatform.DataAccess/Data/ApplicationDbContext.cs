using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TafsilkPlatform.Models.Models;

namespace TafsilkPlatform.DataAccess.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        // ✅ FIXED: Removed NoTracking default - causes issues with updates/inserts
        // Use AsNoTracking() explicitly in read-only queries instead
        ChangeTracker.AutoDetectChangesEnabled = true;
        ChangeTracker.LazyLoadingEnabled = false;

        // ✅ Enable sensitive data logging only in development (controlled by Program.cs)
        // This prevents accidental logging of sensitive data in production
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

    // ✅ IDEMPOTENCY: Idempotency keys for preventing duplicate requests
    public virtual DbSet<IdempotencyKey> IdempotencyKeys { get; set; }



    // ✅ ECOMMERCE: Products and shopping cart
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }
    public virtual DbSet<CartItem> CartItems { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Do not provide a silent fallback provider here. Using a hard-coded
            // "Name=ConnectionStrings:DefaultConnection" caused the runtime to
            // attempt SQL Server even when the development connection string
            // pointed to a SQLite file (e.g. "Data Source=tafsilk-dev.db")
            // That produced confusing errors like attempts to connect to a
            // SQL Server named "tafsilk-dev.db".
            //
            // Require callers to configure the provider explicitly (via
            // AddDbContext in Program.cs or by using the DesignTime factory).
            throw new InvalidOperationException(
                "ApplicationDbContext was constructed without configured DbContextOptions. " +
                "Register the context with AddDbContext in Program.cs or supply a properly configured DbContextOptions when creating ApplicationDbContext. " +
                "If you intended to use a named connection string, configure the provider explicitly (UseSqlServer or UseSqlite)."
            );
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQL Server defaults (SQLite support removed)
        string guidDefaultSql = "(newid())";
        string utcNowDefaultSql = "(getutcdate())";
        string blobColumnType = "varbinary(max)";

        // Check if using SQLite
        var isSqlite = Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

        if (isSqlite)
        {
            // SQLite defaults
            guidDefaultSql = null; // Let EF Core generate GUIDs client-side
            utcNowDefaultSql = "(strftime('%s', 'now') * 1000)"; // Unix milliseconds for DateTimeOffset converter
            blobColumnType = "BLOB";
        }

        // Apply DateTimeOffset converters when using SQLite provider
        // isSqlite is already defined above
        if (isSqlite)
        {
            // SQLite does not support DateTimeOffset natively. Store as INTEGER (Unix ms).
            var dtoToLongConverter = new ValueConverter<DateTimeOffset, long>(
                v => v.ToUnixTimeMilliseconds(),
                v => DateTimeOffset.FromUnixTimeMilliseconds(v));

            var nullableDtoToLongConverter = new ValueConverter<DateTimeOffset?, long?>(
                v => v.HasValue ? v.Value.ToUnixTimeMilliseconds() : (long?)null,
                v => v.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(v.Value) : (DateTimeOffset?)null);

            // Apply converters to all DateTimeOffset properties across entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                if (clrType == null) continue;

                var dtoProps = clrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset));
                foreach (var prop in dtoProps)
                {
                    modelBuilder.Entity(clrType).Property(prop.Name)
                        .HasConversion(dtoToLongConverter)
                        .HasColumnType("INTEGER");
                }

                var nullableDtoProps = clrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset?));
                foreach (var prop in nullableDtoProps)
                {
                    modelBuilder.Entity(clrType).Property(prop.Name)
                        .HasConversion(nullableDtoToLongConverter)
                        .HasColumnType("INTEGER");
                }
            }
        }

        // User Entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07AA820590");

            entity.HasIndex(e => e.Email, "IX_Users_Email");
            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");
            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534975288F0").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql(guidDefaultSql);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(utcNowDefaultSql);
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

            entity.Property(e => e.Id).HasDefaultValueSql(guidDefaultSql);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(utcNowDefaultSql);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Permissions).HasMaxLength(2000);
            entity.Property(e => e.Priority).HasDefaultValue(0);
        });

        // CustomerProfile Entity
        modelBuilder.Entity<CustomerProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07880E7F94");

            entity.HasIndex(e => e.UserId, "IX_CustomerProfiles_UserId");
            entity.HasIndex(e => e.UserId, "UQ__Customer__1788CC4D90808B91").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql(guidDefaultSql);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(utcNowDefaultSql);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Bio).HasMaxLength(1000);

            // Add check constraint for Gender
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_CustomerProfiles_Gender",
                "[Gender] IS NULL OR [Gender] IN ('Male', 'Female')"
            ));

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

            entity.Property(e => e.Id).HasDefaultValueSql(guidDefaultSql);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(utcNowDefaultSql);
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.PricingRange).HasMaxLength(100);
            entity.Property(e => e.ShopName).HasMaxLength(255);
#pragma warning disable CS0618
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);
#pragma warning restore CS0618
            entity.Property(e => e.ProfilePictureContentType).HasMaxLength(100);
            entity.Property(e => e.ProfilePictureData).HasColumnType(blobColumnType);

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

            entity.Property(e => e.Id).HasDefaultValueSql(guidDefaultSql);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(utcNowDefaultSql);
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
                          .WithMany(c => c.Orders)
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

            entity.HasMany(o => o.OrderImages)
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
               entity.Property(e => e.RefundedAmount).HasColumnType("decimal(18,4)"); // Or use HasPrecision(18, 4)

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
            entity.Property(e => e.UploadedAt).HasDefaultValueSql(utcNowDefaultSql);
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
        modelBuilder.Entity<IdempotencyKey>(entity =>
          {
              entity.ToTable("IdempotencyKeys");
              entity.HasKey(e => e.Key).HasName("PK_IdempotencyKeys");

              entity.Property(e => e.Key).IsRequired().HasMaxLength(128);
              entity.Property(e => e.Status).HasDefaultValue(IdempotencyStatus.InProgress);
              entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql(utcNowDefaultSql);
              entity.Property(e => e.ContentType).HasMaxLength(100);
              entity.Property(e => e.Endpoint).HasMaxLength(500);
              entity.Property(e => e.Method).HasMaxLength(10);

              // Indexes for performance
              entity.HasIndex(e => e.Status).HasDatabaseName("IX_IdempotencyKeys_Status");
              entity.HasIndex(e => e.ExpiresAtUtc).HasDatabaseName("IX_IdempotencyKeys_ExpiresAtUtc");
              entity.HasIndex(e => e.UserId).HasDatabaseName("IX_IdempotencyKeys_UserId");
          });







        // ✅ ECOMMERCE: Product Entity
        modelBuilder.Entity<Product>(entity =>
          {
              entity.HasKey(e => e.ProductId);
              entity.Property(e => e.Price).HasColumnType("decimal(18,2)").HasPrecision(18, 2);
              entity.Property(e => e.DiscountedPrice).HasColumnType("decimal(18,2)").HasPrecision(18, 2);
              entity.Property(e => e.CreatedAt).HasDefaultValueSql(utcNowDefaultSql);

              entity.HasIndex(e => e.Category);
              entity.HasIndex(e => e.Slug).IsUnique();
              entity.HasIndex(e => e.IsAvailable);
              entity.HasIndex(e => e.IsFeatured);

              entity.HasOne(p => p.Tailor)
  .WithMany()
         .HasForeignKey(p => p.TailorId)
    .OnDelete(DeleteBehavior.NoAction);
          });

        // ✅ ECOMMERCE: ShoppingCart Entity
        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.CartId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql(utcNowDefaultSql);

            entity.HasIndex(e => e.CustomerId).IsUnique();

            entity.HasOne(c => c.Customer)
                .WithOne(cp => cp.ShoppingCart)
                         .HasForeignKey<ShoppingCart>(c => c.CustomerId)
                   .OnDelete(DeleteBehavior.NoAction);
        });

        // ✅ ECOMMERCE: CartItem Entity
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").HasPrecision(18, 2);
            entity.Property(e => e.AddedAt).HasDefaultValueSql(utcNowDefaultSql);

            entity.HasIndex(e => e.CartId);
            entity.HasIndex(e => e.ProductId);

            entity.HasOne(ci => ci.Cart)
           .WithMany(c => c.Items)
         .HasForeignKey(ci => ci.CartId)
          .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(ci => ci.Product)
                        .WithMany(p => p.CartItems)
                    .HasForeignKey(ci => ci.ProductId)
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
