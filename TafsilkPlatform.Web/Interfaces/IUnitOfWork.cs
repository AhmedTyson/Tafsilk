using TafsilkPlatform.Web.Data;

namespace TafsilkPlatform.Web.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Expose DbContext for advanced queries when needed
        AppDbContext Context { get; }

        IUserRepository Users { get; }
        ITailorRepository Tailors { get; }
        ICustomerRepository Customers { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        IPaymentRepository Payments { get; }
        IPortfolioRepository PortfolioImages { get; }
        ITailorServiceRepository TailorServices { get; }
        IAddressRepository Addresses { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
