using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ProfileImageMigratorTool
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection not found in configuration");

            var attachmentsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Attachments", "profile");
            Directory.CreateDirectory(attachmentsRoot);

            var profiles = new List<(Guid Id, byte[] Data, string? ContentType)>();

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var sql = "SELECT Id, ProfilePictureData, ProfilePictureContentType FROM TailorProfiles WHERE ProfilePictureData IS NOT NULL";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                while (await reader.ReadAsync())
                {
                    var id = reader.GetFieldValue<Guid>(0);
                    byte[]? data = null;
                    if (!await reader.IsDBNullAsync(1))
                    {
                        using var ms = new MemoryStream();
                        const int bufferSize = 8192;
                        long bytesRead = 0;
                        long offset = 0;
                        var outBuffer = new byte[bufferSize];
                        var ordinal = 1;
                        while ((bytesRead = reader.GetBytes(ordinal, offset, outBuffer, 0, outBuffer.Length)) > 0)
                        {
                            ms.Write(outBuffer, 0, (int)bytesRead);
                            offset += bytesRead;
                        }
                        data = ms.ToArray();
                    }
                    string? contentType = null;
                    if (!await reader.IsDBNullAsync(2))
                    {
                        contentType = reader.GetString(2);
                    }

                    if (data != null && data.Length > 0)
                    {
                        profiles.Add((id, data, contentType));
                    }
                }
            }

            Console.WriteLine($"Found {profiles.Count} profiles with DB-stored images");
            int migrated = 0;

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                foreach (var p in profiles)
                {
                    try
                    {
                        var ext = ".jpg";
                        var contentType = p.ContentType ?? "image/jpeg";
                        if (contentType.Contains("png")) ext = ".png";
                        else if (contentType.Contains("gif")) ext = ".gif";
                        else if (contentType.Contains("webp")) ext = ".webp";

                        var fileName = $"{Guid.NewGuid()}{ext}";
                        var filePath = Path.Combine(attachmentsRoot, fileName);
                        await File.WriteAllBytesAsync(filePath, p.Data);

                        var imgUrl = $"/Attachments/profile/{fileName}";

                        using var updateCmd = new SqlCommand("UPDATE TailorProfiles SET ProfileImageUrl = @url, ProfilePictureData = NULL, ProfilePictureContentType = NULL WHERE Id = @id", conn);
                        updateCmd.Parameters.AddWithValue("@url", imgUrl);
                        updateCmd.Parameters.AddWithValue("@id", p.Id);
                        await updateCmd.ExecuteNonQueryAsync();

                        migrated++;
                        Console.WriteLine($"Migrated profile {p.Id} -> {imgUrl}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to migrate profile {p.Id}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Migration complete. {migrated} profiles migrated.");
            return 0;
        }
    }
}
