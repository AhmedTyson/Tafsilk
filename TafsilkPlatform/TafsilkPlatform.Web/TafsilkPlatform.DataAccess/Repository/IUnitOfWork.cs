using TafsilkPlatform.DataAccess.Data;

namespace TafsilkPlatform.DataAccess.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        // Expose DbContext for advanced queries when needed
        ApplicationDbContext Context { get; }

        IUserRepository Users { get; }
        ITailorRepository Tailors { get; }
        ICustomerRepository Customers { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        IPaymentRepository Payments { get; }
        IPortfolioRepository PortfolioImages { get; }
        ITailorServiceRepository TailorServices { get; }
        IAddressRepository Addresses { get; }
        
        // ✅ NEW: E-commerce repositories
        IProductRepository Products { get; }
        IShoppingCartRepository ShoppingCarts { get; }
        ICartItemRepository CartItems { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        /// <summary>
        /// Execute operation with automatic transaction management
        /// </summary>
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);

        /// <summary>
        /// Execute operation with automatic transaction management (no return value)
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> operation);
    }
}
