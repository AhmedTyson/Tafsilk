using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Text;

namespace TafsilkPlatform.Web;

/// <summary>
/// Diagnostic helper to identify IFormFile processing issues
/// </summary>
public static class DiagnosticHelper
{
    /// <summary>
    /// Logs detailed information about an IFormFile
    /// Set breakpoint here to inspect file properties
    /// </summary>
    public static void InspectFormFile(IFormFile? file, ILogger logger, string context = "Unknown")
    {
        // BREAKPOINT: Uncomment next line to enable breakpoint during debugging
        #if DEBUG
        // Debugger.Break();
        #endif
        
        if (file == null)
        {
            logger.LogWarning("[{Context}] IFormFile is NULL", context);
            return;
        }

        try
        {
            var info = new StringBuilder();
            info.AppendLine($"[{context}] IFormFile Details:");
            info.AppendLine($"  FileName: {file.FileName ?? "NULL"}");
            info.AppendLine($"  ContentType: {file.ContentType ?? "NULL"}");
            info.AppendLine($"  Length: {file.Length} bytes");
            info.AppendLine($"  Name: {file.Name ?? "NULL"}");
            
            // Try to get headers if available
            if (file.Headers != null && file.Headers.Count > 0)
            {
                info.AppendLine($"  Headers Count: {file.Headers.Count}");
                foreach (var header in file.Headers)
                {
                    info.AppendLine($"    {header.Key}: {header.Value}");
                }
            }

            logger.LogInformation(info.ToString());

            // Try to read first few bytes
            if (file.Length > 0 && file.Length < 100 * 1024 * 1024) // Less than 100MB
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    var buffer = new byte[Math.Min(16, (int)file.Length)];
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);
                    
                    var hexString = BitConverter.ToString(buffer, 0, bytesRead);
                    logger.LogInformation("[{Context}] First {BytesRead} bytes (hex): {HexString}", 
                        context, bytesRead, hexString);
                    
                    // IMPORTANT: Reset stream position for subsequent operations
                    stream.Position = 0;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{Context}] Error reading file stream", context);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Context}] Error inspecting IFormFile", context);
        }
    }

    /// <summary>
    /// Validates that a stream is readable and not corrupted
    /// Set breakpoint here to inspect stream state
    /// </summary>
    public static bool ValidateStream(Stream? stream, ILogger logger, string context = "Unknown")
    {
        // BREAKPOINT: Uncomment next line to enable breakpoint during debugging
        #if DEBUG
        // Debugger.Break();
        #endif
        
        if (stream == null)
        {
            logger.LogError("[{Context}] Stream is NULL", context);
            return false;
        }

        try
        {
            logger.LogInformation("[{Context}] Stream validation - CanRead: {CanRead}, CanSeek: {CanSeek}, Position: {Position}, Length: {Length}",
                context, stream.CanRead, stream.CanSeek, 
                stream.CanSeek ? stream.Position : -1, 
                stream.CanSeek ? stream.Length : -1);

            if (!stream.CanRead)
            {
                logger.LogError("[{Context}] Stream is not readable", context);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Context}] Error validating stream", context);
            return false;
        }
    }

    /// <summary>
    /// Tests memory allocation to detect potential OOM issues
    /// Set breakpoint here before large allocations
    /// </summary>
    public static bool TestMemoryAllocation(long sizeBytes, ILogger logger, string context = "Unknown")
    {
        // BREAKPOINT: Uncomment next line to enable breakpoint during debugging
        #if DEBUG
        // Debugger.Break();
        #endif
        
        try
        {
            var gc = GC.GetGCMemoryInfo();
            logger.LogInformation("[{Context}] Memory before allocation - Total: {Total:N0} bytes, Available: {Available:N0} bytes, Heap: {Heap:N0} bytes",
                context, gc.TotalAvailableMemoryBytes, gc.TotalAvailableMemoryBytes - gc.HeapSizeBytes, gc.HeapSizeBytes);

            if (sizeBytes > gc.TotalAvailableMemoryBytes / 2)
            {
                logger.LogWarning("[{Context}] Requested allocation ({Size:N0} bytes) is more than 50% of available memory", 
                    context, sizeBytes);
                return false;
            }

            // Test allocation
            var testArray = new byte[Math.Min(sizeBytes, 1024 * 1024)]; // Test with max 1MB
            testArray = null;
            GC.Collect(); // Force cleanup

            return true;
        }
        catch (OutOfMemoryException)
        {
            logger.LogError("[{Context}] OUT OF MEMORY - Cannot allocate {Size:N0} bytes", context, sizeBytes);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{Context}] Error testing memory allocation", context);
            return false;
        }
    }
}
