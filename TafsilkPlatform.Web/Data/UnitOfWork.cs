using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TafsilkPlatform.Web.Interfaces;

namespace TafsilkPlatform.Web.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _tx;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(AppDbContext db,
        IUserRepository users,
        ITailorRepository tailors,
        ICustomerRepository customers,
        IOrderRepository orders,
        IOrderItemRepository orderItems,
        IPaymentRepository payments,
        IPortfolioRepository portfolioImages,
        ITailorServiceRepository tailorServices,
        IAddressRepository addresses,
        IProductRepository products,
        IShoppingCartRepository shoppingCarts,
        ICartItemRepository cartItems,
        ILogger<UnitOfWork> logger)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        Users = users ?? throw new ArgumentNullException(nameof(users));
        Tailors = tailors ?? throw new ArgumentNullException(nameof(tailors));
        Customers = customers ?? throw new ArgumentNullException(nameof(customers));
        Orders = orders ?? throw new ArgumentNullException(nameof(orders));
        OrderItems = orderItems ?? throw new ArgumentNullException(nameof(orderItems));
        Payments = payments ?? throw new ArgumentNullException(nameof(payments));
        PortfolioImages = portfolioImages ?? throw new ArgumentNullException(nameof(portfolioImages));
        TailorServices = tailorServices ?? throw new ArgumentNullException(nameof(tailorServices));
        Addresses = addresses ?? throw new ArgumentNullException(nameof(addresses));
        Products = products ?? throw new ArgumentNullException(nameof(products));
        ShoppingCarts = shoppingCarts ?? throw new ArgumentNullException(nameof(shoppingCarts));
        CartItems = cartItems ?? throw new ArgumentNullException(nameof(cartItems));
    }

    public IUserRepository Users { get; }
    public ITailorRepository Tailors { get; }
    public ICustomerRepository Customers { get; }
    public IOrderRepository Orders { get; }
    public IOrderItemRepository OrderItems { get; }
    public IPaymentRepository Payments { get; }
    public IPortfolioRepository PortfolioImages { get; }
    public ITailorServiceRepository TailorServices { get; }
    public IAddressRepository Addresses { get; }
    
    // ✅ NEW: E-commerce repositories
    public IProductRepository Products { get; }
    public IShoppingCartRepository ShoppingCarts { get; }
    public ICartItemRepository CartItems { get; }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict while saving changes");
            throw;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while saving changes");
            throw;
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_tx is not null)
        {
            _logger.LogWarning("Transaction already in progress");
            return;
        }

        _logger.LogDebug("Beginning database transaction");
        _tx = await _db.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_tx is null)
        {
            _logger.LogWarning("No active transaction to commit");
            return;
        }

        try
        {
            _logger.LogDebug("Committing database transaction");
            await _tx.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            throw;
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_tx is null)
        {
            _logger.LogWarning("No active transaction to rollback");
            return;
        }

        try
        {
            _logger.LogWarning("Rolling back database transaction");
            await _tx.RollbackAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction");
            throw;
        }
        finally
        {
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    /// <summary>
    /// Execute operation with automatic transaction management
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async (ct) =>
        {
            await BeginTransactionAsync();
            try
            {
                var result = await operation();
                await CommitTransactionAsync();
                return result;
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }, cancellationToken: default);
    }

    /// <summary>
    /// Execute operation with automatic transaction management (no return value)
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async (ct) =>
        {
            await BeginTransactionAsync();
            try
            {
                await operation();
                await CommitTransactionAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
        }, cancellationToken: default);
    }

    public void Dispose()
    {
        if (_tx is not null)
        {
            _logger.LogWarning("Disposing UnitOfWork with active transaction");
            _tx.Dispose();
            _tx = null;
        }
        _db.Dispose();
    }

    // Expose DbContext for advanced queries (use sparingly)
    public AppDbContext Context => _db;
}
