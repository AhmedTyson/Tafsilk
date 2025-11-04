using Microsoft.EntityFrameworkCore.Storage;
using TafsilkPlatform.Web.Interfaces;
using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(AppDbContext db,
        IUserRepository users,
        ITailorRepository tailors,
        ICustomerRepository customers,
        IOrderRepository orders,
        IOrderItemRepository orderItems,
        IPaymentRepository payments,
        IReviewRepository reviews,
        IRatingDimensionRepository ratingDimensions,
        IPortfolioRepository portfolioImages,
        ITailorServiceRepository tailorServices,
        INotificationRepository notifications,
        IAddressRepository addresses)
    {
        _db = db;
        Users = users;
        Tailors = tailors;
        Customers = customers;
        Orders = orders;
        OrderItems = orderItems;
        Payments = payments;
        Reviews = reviews;
        RatingDimensions = ratingDimensions;
        PortfolioImages = portfolioImages;
        TailorServices = tailorServices;
        Notifications = notifications;
        Addresses = addresses;
    }

    public IUserRepository Users { get; }
    public ITailorRepository Tailors { get; }
    public ICustomerRepository Customers { get; }
    public IOrderRepository Orders { get; }
    public IOrderItemRepository OrderItems { get; }
    public IPaymentRepository Payments { get; }
    public IReviewRepository Reviews { get; }
    public IRatingDimensionRepository RatingDimensions { get; }
    public IPortfolioRepository PortfolioImages { get; }
    public ITailorServiceRepository TailorServices { get; }
    public INotificationRepository Notifications { get; }
    public IAddressRepository Addresses { get; }

    public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task BeginTransactionAsync()
    {
        if (_tx is not null) return;
        _tx = await _db.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_tx is null) return;
        await _tx.CommitAsync();
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_tx is null) return;
        await _tx.RollbackAsync();
        await _tx.DisposeAsync();
        _tx = null;
    }

    public void Dispose()
    {
        _tx?.Dispose();
        _db.Dispose();
    }

    // Expose DbContext for advanced queries (use sparingly)
    public AppDbContext Context => _db;
}
