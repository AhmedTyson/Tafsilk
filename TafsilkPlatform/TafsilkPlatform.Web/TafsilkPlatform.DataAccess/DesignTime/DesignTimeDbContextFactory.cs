using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace TafsilkPlatform.DataAccess.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Locate appsettings.json by walking up directories from the current directory
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            string? appSettingsPath = null;
            for (int i = 0; i < 6 && dir != null; i++)
            {
                var candidate = Path.Combine(dir.FullName, "appsettings.json");
                if (File.Exists(candidate))
                {
                    appSettingsPath = candidate;
                    break;
                }
                dir = dir.Parent;
            }

            if (string.IsNullOrWhiteSpace(appSettingsPath))
            {
                throw new InvalidOperationException("Could not find 'appsettings.json' in the current directory or any parent directories. The connection string must be specified in appsettings.json under 'ConnectionStrings:DefaultConnection'.");
            }

            string json = File.ReadAllText(appSettingsPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ConnectionStrings", out var connSection) ||
                !connSection.TryGetProperty("DefaultConnection", out var connValue) ||
                string.IsNullOrWhiteSpace(connValue.GetString()))
            {
                throw new InvalidOperationException($"'ConnectionStrings:DefaultConnection' was not found or is empty in '{appSettingsPath}'. Please add the connection string there.");
            }

            var conn = connValue.GetString()!;

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // If the connection string looks like a SQLite file (Data Source=...), use SQLite provider
            if (conn.TrimStart().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                optionsBuilder.UseSqlite(conn, b => b.MigrationsAssembly("TafsilkPlatform.DataAccess"));
            }
            else
            {
                optionsBuilder.UseSqlServer(conn, b => b.MigrationsAssembly("TafsilkPlatform.DataAccess"));
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
