namespace TafsilkPlatform.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        ITailorRepository Tailors { get; }
        ICustomerRepository Customers { get; }
        ICorporateRepository Corporates { get; }
        IOrderRepository Orders { get; }
        IOrderItemRepository OrderItems { get; }
        IQuoteRepository Quotes { get; }
        IPaymentRepository Payments { get; }
        IWalletRepository Wallets { get; }
        IReviewRepository Reviews { get; }
        IRatingDimensionRepository RatingDimensions { get; }
        IPortfolioRepository PortfolioImages { get; }
        ITailorServiceRepository TailorServices { get; }
        INotificationRepository Notifications { get; }
        IAddressRepository Addresses { get; }
        IRFQRepository RFQs { get; }
        IRFQBidRepository RFQBids { get; }
        IContractRepository Contracts { get; }
        IDisputeRepository Disputes { get; }
        IAdminRepository Admins { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
