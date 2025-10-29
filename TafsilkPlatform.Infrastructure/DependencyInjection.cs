using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TafsilkPlatform.Core.Interfaces;
using TafsilkPlatform.Infrastructure.Persistence;
using TafsilkPlatform.Infrastructure.Repositories;
using TafsilkPlatform.Infrastructure.Services;

namespace TafsilkPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TafsilkPlatform")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=TafsilkPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITailorRepository, TailorRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICorporateRepository, CorporateRepository>();
        services.AddScoped<IQuoteRepository, QuoteRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IRatingDimensionRepository, RatingDimensionRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<ITailorServiceRepository, TailorServiceRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IRFQRepository, RFQRepository>();
        services.AddScoped<IRFQBidRepository, RFQBidRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IDisputeRepository, DisputeRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
