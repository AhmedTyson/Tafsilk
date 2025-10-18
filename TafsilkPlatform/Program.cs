using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TafsilkPlatform.Data;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace TafsilkPlatform
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            var connectionString = configuration.GetConnectionString("TafsilkPlatform")
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=TafsilkPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (args.Any(a => string.Equals(a, "--rebuild-db", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(a, "rebuild-db", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Dropping database (if exists)...");
                await db.Database.EnsureDeletedAsync();
                Console.WriteLine("Creating schema from current model...");
                await db.Database.EnsureCreatedAsync();
                Console.WriteLine("Database rebuild complete.");
                return;
            }

            var canConnect = await db.Database.CanConnectAsync();
            Console.WriteLine($"Database connection: {(canConnect ? "OK" : "FAILED")}");
        }
    }
}
