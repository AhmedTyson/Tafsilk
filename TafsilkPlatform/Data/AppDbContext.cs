using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TafsilkPlatform.Models;

namespace TafsilkPlatform.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    
    public virtual DbSet<CorporateAccount> CorporateAccounts { get; set; }

    public virtual DbSet<CustomerProfile> CustomerProfiles { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TailorProfile> TailorProfiles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<RatingDimension> RatingDimensions { get; set; }
    public DbSet<TailorBadge> TailorBadges{ get; set; }
    public DbSet<PortfolioImage> PortfolioImages { get; set; }
    public DbSet<TailorService> TailorServices { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SystemMessage> SystemMessages { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }
    public DbSet<TailorPerformanceView> TailorPerformanceViews { get; set; }
    public DbSet<RevenueReport> RevenueReports { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=Tyson\\SQLEXPRESS;Database=TafsilkPlatform;Trusted_Connection=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CorporateAccount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Corporat__3214EC07AC002547");

            entity.HasIndex(e => e.UserId, "IX_CorporateAccounts_UserId");

            entity.HasIndex(e => e.UserId, "UQ__Corporat__1788CC4D39440DF8").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.ContactPerson).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.IsApproved).HasDefaultValue(false);
            entity.Property(e => e.TaxNumber).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.CorporateAccount)
                .HasForeignKey<CorporateAccount>(d => d.UserId)
                .HasConstraintName("FK_CorporateAccounts_Users");
        });

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

            entity.HasOne(d => d.User).WithOne(p => p.CustomerProfile)
                .HasForeignKey<CustomerProfile>(d => d.UserId)
                .HasConstraintName("FK_CustomerProfiles_Users");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07E9DA722D");

            entity.HasIndex(e => e.ExpiresAt, "IX_RefreshTokens_ExpiresAt");

            entity.HasIndex(e => e.UserId, "IX_RefreshTokens_UserId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                  .HasForeignKey(d => d.UserId)
                  .HasConstraintName("FK_RefreshTokens_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07CB85E41E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<TailorProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TailorPr__3214EC07A3FCF42C");

            entity.HasIndex(e => e.UserId, "IX_TailorProfiles_UserId");

            entity.HasIndex(e => e.UserId, "UQ__TailorPr__1788CC4D37A4BF4A").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsVerified).HasDefaultValue(false);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11, 8)");
            entity.Property(e => e.PricingRange).HasMaxLength(100);
            entity.Property(e => e.ShopName).HasMaxLength(255);

            entity.HasOne(d => d.User).WithOne(p => p.TailorProfile)
                  .HasForeignKey<TailorProfile>(d => d.UserId)
                  .HasConstraintName("FK_TailorProfiles_Users");
        });

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

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

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

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserAddresses_Users");
        });

        //Eriny Domain
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("Reviews");
            entity.HasKey(e => e.ReviewId).HasName("PK_Reviews");

            entity.Property(e => e.Comment).HasMaxLength(1000).HasComment("Comment cannot exceed 1000 characters"); ;
            entity.Property(e => e.Rating).IsRequired().HasComment("Rating is required");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property<bool>("IsDeleted") 
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");

            entity.HasIndex(e => e.OrderId).HasDatabaseName("IX_Reviews_OrderId");
            entity.HasIndex(e => e.TailorId).HasDatabaseName("IX_Reviews_TailorId");
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_Reviews_CustomerId");

            entity.HasOne<Order>()
                  .WithOne()
                  .HasForeignKey<Review>(r => r.OrderId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Reviews_Orders");

            entity.HasOne<TailorProfile>()
                  .WithMany()
                  .HasForeignKey(r => r.TailorId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Reviews_TailorProfiles");

            entity.HasOne<CustomerProfile>()
                  .WithMany()
                  .HasForeignKey(r => r.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_Reviews_CustomerProfiles");

            entity.HasMany(r => r.RatingDimensions)
                  .WithOne(rd => rd.Review)
                  .HasForeignKey(rd => rd.ReviewId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RatingDimension>(entity =>
        {
            entity.ToTable("RatingDimensions");
            entity.HasKey(rd => rd.RatingDimensionId).HasName("PK_RatingDimensions");

            entity.Property(rd => rd.DimensionName)
                  .HasMaxLength(100)
                  .IsRequired()
                  .HasComment("Dimension name is required, max 100 chars");


            entity.Property(rd => rd.Score)
                  .IsRequired()
                  .HasComment("Score is required");

            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");


        });

        modelBuilder.Entity<TailorBadge>(entity =>
        {
            entity.ToTable("TailorBadges");
            entity.HasKey(tb => tb.TailorBadgeId).HasName("PK_TailorBadges");

            entity.Property(tb => tb.BadgeName)
                  .HasMaxLength(150)
                  .IsRequired()
                  .HasComment("Badge name is required, max 150 chars");

            entity.Property(tb => tb.Description)
                  .HasMaxLength(500)
                  .HasComment("Description max 500 chars");

            entity.Property(tb => tb.EarnedAt)
                  .HasDefaultValueSql("(getutcdate())");

            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");

            entity.HasIndex(tb => tb.TailorId).HasDatabaseName("IX_TailorBadges_TailorId");

            entity.HasOne<TailorProfile>()
                  .WithMany()
                  .HasForeignKey(tb => tb.TailorId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .HasConstraintName("FK_TailorBadges_TailorProfiles");
        });

        modelBuilder.Entity<PortfolioImage>(entity =>
        {
            entity.ToTable("PortfolioImages");
            entity.HasKey(e => e.PortfolioImageId).HasName("PK_PortfolioImages");

            entity.Property(e => e.PortfolioImageId)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.ImageUrl)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.UploadedAt)
                  .HasDefaultValueSql("(getutcdate())");

            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");
            entity.Property(e => e.IsBeforeAfter)
                  .IsRequired()
                  .HasComment("Indicates if image is before/after");


            entity.HasIndex(e => e.TailorId).HasDatabaseName("IX_PortfolioImages_TailorId");
            entity.HasIndex(e => e.IsBeforeAfter).HasDatabaseName("IX_PortfolioImages_IsBeforeAfter");
            entity.HasIndex(e => e.UploadedAt).HasDatabaseName("IX_PortfolioImages_UploadedAt");

            entity.HasIndex(e => new { e.TailorId, e.UploadedAt })
                  .HasDatabaseName("IX_PortfolioImages_TailorId_UploadedAt");

            entity.HasOne<TailorProfile>()
                  .WithMany()
                  .HasForeignKey(pi => pi.TailorId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_PortfolioImages_TailorProfiles");
        });

        modelBuilder.Entity<TailorService>(entity =>
        {
            entity.ToTable("TailorServices");
            entity.HasKey(e => e.TailorServiceId).HasName("PK_TailorServices");

            entity.Property(e => e.TailorServiceId)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.ServiceName)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasComment("Service name is required, max 100 chars");


            entity.Property(e => e.Description)
                  .HasMaxLength(500)
                  .HasComment("Description max 500 chars");

            entity.Property(e => e.BasePrice)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.EstimatedDuration)
                  .IsRequired()
                  .HasComment("Estimated duration is required in minutes");

            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");

            entity.HasIndex(e => e.TailorId).HasDatabaseName("IX_TailorServices_TailorId");
            entity.HasIndex(e => e.ServiceName).HasDatabaseName("IX_TailorServices_ServiceName");

            entity.HasOne<TailorProfile>()
                  .WithMany()
                  .HasForeignKey(ts => ts.TailorId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_TailorServices_TailorProfiles");

        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(e => e.NotificationId).HasName("PK_Notifications");
            entity.Property(e => e.NotificationId)
                  .ValueGeneratedOnAdd();
            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(200)
                  .HasComment("Notification title is required, max 200 chars");

            entity.Property(e => e.Message)
                  .IsRequired()
                  .HasMaxLength(2000)
                  .HasComment("Notification message is required, max 2000 chars");
            entity.Property(e => e.Type)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.Property(e => e.IsRead)
                  .HasDefaultValue(false);
            entity.Property(e => e.SentAt)
                  .HasDefaultValueSql("(getutcdate())");
            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");
                  

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_Notifications_UserId");
            entity.HasIndex(e => e.IsRead).HasDatabaseName("IX_Notifications_IsRead");
            entity.HasIndex(e => e.SentAt).HasDatabaseName("IX_Notifications_SentAt");
            entity.HasIndex(e => e.Type).HasDatabaseName("IX_Notifications_Type");
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.SentAt })
                  .HasDatabaseName("IX_Notifications_User_Read_Date");

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_Notifications_Users");

        });

        modelBuilder.Entity<SystemMessage>(entity =>
        {
            entity.ToTable("SystemMessages");
            entity.HasKey(e => e.SystemMessageId).HasName("PK_SystemMessages");

            entity.Property(e => e.SystemMessageId)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.Title)
                  .IsRequired()
                  .HasMaxLength(200)
                  .HasComment("Message title is required, max 200 chars");
            entity.Property(e => e.Content)
                  .IsRequired()
                  .HasMaxLength(4000)
                  .HasComment("Message content is required, max 4000 chars");
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.AudienceType)
                  .IsRequired()
                  .HasMaxLength(50);
            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");

            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_SystemMessages_CreatedAt");
            entity.HasIndex(e => e.AudienceType).HasDatabaseName("IX_SystemMessages_AudienceType");


        });

        modelBuilder.Entity<DeviceToken>(entity =>
        {
            entity.ToTable("DeviceTokens");
            entity.HasKey(e => e.DeviceTokenId).HasName("PK_DeviceTokens");

            entity.Property(e => e.DeviceTokenId)
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.DeviceToken)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.Platform)
                  .IsRequired()
                  .HasMaxLength(20);

            entity.Property(e => e.RegisteredAt)
                  .HasDefaultValueSql("(getutcdate())");
            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_DeviceTokens_UserId");
            entity.HasIndex(e => e.Platform).HasDatabaseName("IX_DeviceTokens_Platform");
            entity.HasIndex(e => e.DeviceToken).HasDatabaseName("IX_DeviceTokens_Token");

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(dt => dt.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_DeviceTokens_Users");

        });

        modelBuilder.Entity<TailorPerformanceView>(entity =>
        {
            entity.ToView("TailorPerformanceView"); 
            entity.HasNoKey();

            entity.HasOne<TailorProfile>()
                  .WithMany()
                  .HasForeignKey(tpv => tpv.TailorId)
                  .HasConstraintName("FK_TailorPerformanceView_TailorProfiles")
                  .OnDelete(DeleteBehavior.ClientSetNull) 
                  .IsRequired(false);
        });


        modelBuilder.Entity<RevenueReport>(entity =>
        {

            entity.HasKey(e => new { e.TailorId, e.Month }).HasName("PK_RevenueReports");

            entity.Property(e => e.Month)
                  .IsRequired()
                  .HasColumnType("date");

            entity.Property(e => e.TotalRevenue)
                  .HasColumnType("decimal(18,2)");

            entity.Property(e => e.GeneratedAt)
                  .HasDefaultValueSql("(getutcdate())");
            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");

            entity.HasOne<TailorProfile>()
                  .WithMany()
                  .HasForeignKey(rr => rr.TailorId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_RevenueReports_TailorProfiles");
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.ToTable("UserActivityLogs");
            entity.HasKey(e => e.UserActivityLogId).HasName("PK_UserActivityLogs");

            entity.Property(e => e.Action)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasComment("Action description is required, max 100 chars");
            entity.Property(e => e.EntityType)
                  .HasMaxLength(50)
                  .HasComment("Entity type max 50 chars");
            entity.Property(e => e.IpAddress)
                  .HasMaxLength(45) 
                  .HasComment("IP address max 45 chars, supports IPv6");
            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("(getutcdate())");
            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");

            entity.HasIndex(e => e.UserId).HasDatabaseName("IX_UserActivityLogs_UserId");
            entity.HasIndex(e => e.Action).HasDatabaseName("IX_UserActivityLogs_Action");
            entity.HasIndex(e => e.EntityType).HasDatabaseName("IX_UserActivityLogs_EntityType");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_UserActivityLogs_CreatedAt");
            entity.HasIndex(e => new { e.UserId, e.Action, e.CreatedAt })
                  .HasDatabaseName("IX_UserActivityLogs_User_Action_Date");

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(ual => ual.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_UserActivityLogs_Users");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.ToTable("ErrorLogs");
            entity.HasKey(e => e.ErrorLogId).HasName("PK_ErrorLogs");


            entity.Property(e => e.Message)
                  .IsRequired()
                  .HasMaxLength(2000)
                  .HasComment("Error message is required, max 2000 chars");

            entity.Property(e => e.Severity)
                  .IsRequired()
                  .HasMaxLength(20)
                  .HasDefaultValue("Error")
                  .HasComment("Severity level required, default 'Error', max 20 chars");

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("(getutcdate())");
            entity.Property<bool>("IsDeleted")
                  .HasDefaultValue(false)
                  .HasComment("Soft delete flag");

            entity.HasIndex(e => e.Severity).HasDatabaseName("IX_ErrorLogs_Severity");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_ErrorLogs_CreatedAt");
            entity.HasIndex(e => new { e.Severity, e.CreatedAt })
                  .HasDatabaseName("IX_ErrorLogs_Severity_CreatedAt");
        });

      OnModelCreatingPartial(modelBuilder);

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
