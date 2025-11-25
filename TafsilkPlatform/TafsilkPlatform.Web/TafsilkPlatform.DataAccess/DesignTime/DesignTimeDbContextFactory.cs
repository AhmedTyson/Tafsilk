using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace TafsilkPlatform.DataAccess.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Use a single simple env var. If missing/empty/"false" -> use LocalDB fallback.
            var raw = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
            var conn = string.IsNullOrWhiteSpace(raw) || string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase)
                ? "Server=(localdb)\\MSSQLLocalDB;Database=Tafsilk_Tmp;Trusted_Connection=True;MultipleActiveResultSets=true"
                : raw;

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(conn, b => b.MigrationsAssembly("TafsilkPlatform.DataAccess"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
