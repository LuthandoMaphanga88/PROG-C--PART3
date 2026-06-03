using Microsoft.EntityFrameworkCore;

namespace Techmove.Data;

/// <summary>
/// Helper class for database initialization and management.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Initializes the database by running pending migrations.
    /// This should be called during application startup.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void InitializeDatabase(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();

                // Apply pending migrations
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }
    }
}
